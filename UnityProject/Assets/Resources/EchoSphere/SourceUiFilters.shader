Shader "EchoSphere/SourceUiFilters"
{
 Properties { _MainTex("Subtree",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  HLSLINCLUDE
  #include "UnityCG.cginc"
  #include "UnityUIEFilter.cginc"
  sampler2D _MainTex;float4 _MainTex_ST,_MainTex_TexelSize;float _Amount,_OffsetX,_OffsetY,_Sigma,_CssDpr,_CssReadsGamma,_CssWritesGamma;float4 _CssColor;
  struct effectOutput {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;uint rectIndex:TEXCOORD1;};
  effectOutput effectVertex(FilterVertexInput v){effectOutput o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.rectIndex=GetFilterRectIndex(v);return o;}
  float4 Finish(float4 c){
   if(_CssWritesGamma<.5)c.rgb=c.a>0?GammaToLinearSpace(c.rgb/c.a)*c.a:0;
   return c;
  }
  float Alpha(float2 uv,float4 rect){return any(uv<rect.xy)||any(uv>rect.xy+rect.zw)?0:tex2D(_MainTex,uv).a;}
  float4 Read(float2 uv){float4 c=tex2D(_MainTex,uv);if(_CssReadsGamma<.5)c.rgb=c.a>0?LinearToGammaSpace(c.rgb/c.a)*c.a:0;return c;}
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentSaturate
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentSaturate(effectOutput i):SV_Target {float4 c=Read(i.uv);float3 rgb=c.a>0?c.rgb/c.a:0;float luma=dot(rgb,float3(.213,.715,.072));c.rgb=saturate(lerp(luma.xxx,rgb,_Amount))*c.a;return Finish(c);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentBrightness
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentBrightness(effectOutput i):SV_Target {float4 c=Read(i.uv);c.rgb=min(c.a,max(0,c.rgb*_Amount));return Finish(c);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentShadow
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   #pragma target 3.5
   float4 fragmentShadow(effectOutput i):SV_Target {
    float4 rect=GetFilterUVRect(i.rectIndex);float4 original=Read(i.uv);float sigma=max(0,_Sigma*_CssDpr);float2 origin=i.uv-float2(_OffsetX,-_OffsetY)*_CssDpr*_MainTex_TexelSize.xy;float alpha=0;
    if(sigma<.001)alpha=Alpha(origin,rect);
    else {
     int radius=(int)ceil(4*sigma);float normalization=0;
     [loop]for(int y=-radius;y<=radius;y++){
      float wy=exp(-y*(float)y/(2*sigma*sigma));
      [loop]for(int x=-radius;x<=radius;x++){float weight=wy*exp(-x*(float)x/(2*sigma*sigma));alpha+=Alpha(origin+float2(x,y)*_MainTex_TexelSize.xy,rect)*weight;normalization+=weight;}
     }
     alpha/=max(.000001,normalization);
    }
    float4 color=_CssColor;
    #ifndef UNITY_COLORSPACE_GAMMA
     color.rgb=LinearToGammaSpace(color.rgb);
    #endif
    alpha*=color.a;float4 shadow=float4(color.rgb*alpha,alpha);return Finish(original+shadow*(1-original.a));
   }
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex effectVertex
   #pragma fragment fragmentContrast
   #pragma multi_compile _ _UIE_OUTPUT_LINEAR
   float4 fragmentContrast(effectOutput i):SV_Target {float4 c=Read(i.uv);float3 rgb=c.a>0?c.rgb/c.a:0;c.rgb=saturate((rgb-.5)*_Amount+.5)*c.a;return Finish(c);}
   ENDHLSL
  }
 }
}
