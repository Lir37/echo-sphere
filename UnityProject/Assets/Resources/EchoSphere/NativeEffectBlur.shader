Shader "EchoSphere/NativeEffectBlur"
{
 Properties {_MainTex("Native captured layer",2D)="black"{}}
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend Off
  HLSLINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex;float4 _MainTex_TexelSize,_Roi,_PanelSize,_ToPanelX,_ToPanelY,_ToScreenX,_ToScreenY;
  float _Sigma,_Density;float4 _Weights[128];int _WeightCount;
  float4 Read(float2 uv,bool horizontal){
   if(horizontal){
    float2 screen=_Roi.xy+float2(uv.x,1-uv.y)*_Roi.zw;
    float2 panel=float2(dot(_ToPanelX.xyz,float3(screen,1)),dot(_ToPanelY.xyz,float3(screen,1)));
    panel=_PanelSize.xy-abs(_PanelSize.xy-frac(panel/(2*_PanelSize.xy))*(2*_PanelSize.xy));
    float2 reflected=float2(dot(_ToScreenX.xyz,float3(panel,1)),dot(_ToScreenY.xyz,float3(panel,1)));
    uv+=(reflected-screen)*float2(1,-1)/_Roi.zw;
   }
   if(any(uv<0)||any(uv>1))return 0;
   float4 color=tex2D(_MainTex,uv);
   #ifndef UNITY_COLORSPACE_GAMMA
    if(horizontal)color.rgb=LinearToGammaSpace(color.rgb);
   #endif
   return color;
  }
  float4 Blur(v2f_img i,bool horizontal){
   float2 axis=horizontal?float2(1,0):float2(0,1);float4 color=Read(i.uv,horizontal);float total=1;
   int radius=(int)ceil(4*_Sigma);
   [loop]for(int offset=1;offset<=radius;offset++){
    float weight=_WeightCount>radius?_Weights[offset>>2][offset&3]:exp(-offset*(float)offset/(2*_Sigma*_Sigma));
    float2 delta=axis*_MainTex_TexelSize.xy*offset;color+=(Read(i.uv+delta,horizontal)+Read(i.uv-delta,horizontal))*weight;total+=2*weight;
   }
   color/=max(.000001,total);
   // UI Images consume straight RGBA; the captured/native intermediate is premultiplied.
   if(!horizontal){
    // UIR's reference filter stores premultiplied RGBA8 between passes.
    // Match that quantization before converting to the Image's straight RGBA;
    // the half-float output avoids a second lossy straight-alpha quantization.
    color=round(saturate(color)*255)/255;
    color.rgb=color.a>0?color.rgb/color.a:0;
    #ifndef UNITY_COLORSPACE_GAMMA
     color.rgb=GammaToLinearSpace(color.rgb);
    #endif
   }
   return color;
  }
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert_img
   #pragma fragment horizontal
   float4 horizontal(v2f_img i):SV_Target{return Blur(i,true);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert_img
   #pragma fragment vertical
   float4 vertical(v2f_img i):SV_Target{return Blur(i,false);}
   ENDHLSL
  }
 }
}
