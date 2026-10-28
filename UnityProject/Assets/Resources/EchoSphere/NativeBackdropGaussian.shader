Shader "EchoSphere/NativeBackdropGaussian"
{
 Properties { _MainTex("Captured Unity backdrop",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  HLSLINCLUDE
  #include "UnityCG.cginc"
  #include "UnityUIEFilter.cginc"
  sampler2D _MainTex;float4 _MainTex_ST,_MainTex_TexelSize;float _Sigma,_CssDpr,_CssReadsGamma;
  float4 _BackdropWeights[128];int _BackdropWeightCount;float _BackdropPairing;
  float4 _Roi,_PanelSize,_ToPanelX,_ToPanelY,_ToScreenX,_ToScreenY;
  struct effectOutput {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;nointerpolation uint rectIndex:TEXCOORD1;float2 css:TEXCOORD2;};
  effectOutput effectVertex(FilterVertexInput v){effectOutput o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.rectIndex=GetFilterRectIndex(v);o.css=_Roi.xy+v.vertex.xy/_CssDpr;return o;}
  float2 Mirror(float2 uv,float2 screen){
   // Unity's filter quad uses local panel coordinates. Its atlas UV domain can
   // include allocator padding, so it is not the CSS capture rectangle.
   float2 panel=float2(dot(_ToPanelX.xyz,float3(screen,1)),dot(_ToPanelY.xyz,float3(screen,1)));
   // CSS backdrop blur mirrors at the transformed border quad. It must not
   // mix unrelated pixels just outside a panel into the panel's edge.
   panel=_PanelSize.xy-abs(_PanelSize.xy-frac(panel/(2*_PanelSize.xy))*(2*_PanelSize.xy));
   float2 reflected=float2(dot(_ToScreenX.xyz,float3(panel,1)),dot(_ToScreenY.xyz,float3(panel,1)));
   return uv+(reflected-screen)*float2(1,-1)*_CssDpr*_MainTex_TexelSize.xy;
  }
  float4 Read(float2 uv,float4 rect,bool mirror,float2 screen){
   if(mirror)uv=Mirror(uv,screen);
   // UI Toolkit packs independent filter surfaces into an atlas. Never sample
   // neighbouring panels. UIR supplies premultiplied pixels, not straight RGBA.
   if(any(uv<rect.xy)||any(uv>rect.xy+rect.zw))return float4(0,0,0,0);
   float4 c=tex2D(_MainTex,uv);
   if(_CssReadsGamma<.5)c.rgb=c.a>0?LinearToGammaSpace(c.rgb/c.a)*c.a:0;
   return c;
  }
  float4 PairRead(float2 uv,float4 rect,bool mirror,float2 screen,float2 axis,int offset,float a,float b){
   float2 delta=axis*_MainTex_TexelSize.xy;
   float2 cssStep=float2(axis.x,-axis.y)/_CssDpr;
   float2 uvA=uv+delta*offset,screenA=screen+cssStep*offset;
   float4 value=float4(0,0,0,0);bool paired=false;
   if(b>0&&_BackdropPairing>.5&&_CssReadsGamma>.5){
    float2 uvB=uvA+delta,screenB=screenA+cssStep;
    float2 localA=float2(dot(_ToPanelX.xyz,float3(screenA,1)),dot(_ToPanelY.xyz,float3(screenA,1)));
    float2 localB=float2(dot(_ToPanelX.xyz,float3(screenB,1)),dot(_ToPanelY.xyz,float3(screenB,1)));
    float2 phase=uvA*abs(_MainTex_TexelSize.zw)-.5;
    bool interior=all(uvA>=rect.xy)&&all(uvA<=rect.xy+rect.zw)&&all(uvB>=rect.xy)&&all(uvB<=rect.xy+rect.zw);
    // Pair only the linear, texel-centred interior. Reflections, atlas edges,
    // fractional phases and gamma conversion retain the original two taps.
    if(interior&&all(abs(phase-round(phase))<.001)&&(!mirror||(all(localA>=0)&&all(localA<=_PanelSize.xy)&&all(localB>=0)&&all(localB<=_PanelSize.xy)))){
     value=tex2D(_MainTex,(uvA*a+uvB*b)/(a+b))*(a+b);paired=true;
    }
   }
   if(!paired){value=Read(uvA,rect,mirror,screenA)*a;if(b>0)value+=Read(uvA+delta,rect,mirror,screenA+cssStep)*b;}
   return value;
  }
  float4 Blur(effectOutput i,float2 axis,bool mirror){
   float4 rect=GetFilterUVRect(i.rectIndex);float sigma=max(0,_Sigma*_CssDpr);float4 result=float4(0,0,0,0);
   if(sigma<.001)result=Read(i.uv,rect,mirror,i.css);
   else {
    int radius=(int)ceil(4*sigma);float normalization=1;result=Read(i.uv,rect,mirror,i.css);
    if(_BackdropPairing>.5&&_BackdropWeightCount>radius){
     [loop]for(int offset=1;offset<=radius;offset+=2){int next=min(offset+1,radius);
      float a=_BackdropWeights[offset>>2][offset&3],b=offset<radius?_BackdropWeights[next>>2][next&3]:0;
      result+=PairRead(i.uv,rect,mirror,i.css,axis,offset,a,b)+PairRead(i.uv,rect,mirror,i.css,-axis,offset,a,b);normalization+=2*(a+b);
     }
    }else{
     [loop]for(int offset=1;offset<=radius;offset++){
      float weight=_BackdropWeightCount>radius?_BackdropWeights[offset>>2][offset&3]:exp(-offset*(float)offset/(2*sigma*sigma));
      float2 delta=axis*_MainTex_TexelSize.xy*offset;
      float2 step=float2(axis.x,-axis.y)*offset/_CssDpr;
      result+=(Read(i.uv+delta,rect,mirror,i.css+step)+Read(i.uv-delta,rect,mirror,i.css-step))*weight;normalization+=2*weight;
     }
    }
    result/=max(.000001,normalization);
   }
   // forceGammaRendering keeps intermediate surfaces in gamma. Only the
   // final SRGB attachment needs a linear shader output. Convert the stored
   // premultiplied RGB directly so hardware SRGB encoding restores it exactly.
   #if defined(_UIE_OUTPUT_LINEAR) && !defined(UNITY_COLORSPACE_GAMMA)
    result.rgb=GammaToLinearSpace(result.rgb);
   #endif
   return result;
  }
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex effectVertex
   #pragma fragment horizontal
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 horizontal(effectOutput i):SV_Target {return Blur(i,float2(1,0),true);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex effectVertex
   #pragma fragment vertical
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 vertical(effectOutput i):SV_Target {float4 color=Blur(i,float2(0,1),true);color.rgb=color.a>0?color.rgb/color.a:0;return color;}
   ENDHLSL
  }
 }
}
