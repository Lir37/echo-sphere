Shader "EchoSphere/SourceUiBackdrop"
{
 Properties { _MainTex("Underlay",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend Off
  HLSLINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_ClipMask;float4 _MainTex_TexelSize,_CaptureOrigin,_CaptureAxisX,_CaptureAxisY,_HorizontalStep,_VerticalRect;float _Sigma;
  float4 Read(float2 uv,bool capture){float4 c=tex2D(_MainTex,uv);
  #ifndef UNITY_COLORSPACE_GAMMA
   if(capture)c.rgb=LinearToGammaSpace(c.rgb);
  #endif
   c.rgb*=c.a;return c;}
  float4 Blur(float2 uv,float2 direction,bool capture){if(_Sigma<.001){float4 c=Read(uv,capture);c.rgb/=max(.000001,c.a);return c;}int radius=(int)ceil(4*_Sigma);float4 color=0;float total=0;[loop]for(int offset=-radius;offset<=radius;offset++){float weight=exp(-offset*(float)offset/(2*_Sigma*_Sigma));color+=Read(uv+direction*offset,capture)*weight;total+=weight;}color/=max(.000001,total);color.rgb/=max(.000001,color.a);return color;}
  ENDHLSL
  Pass {
   HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment horizontal
   float4 horizontal(v2f_img i):SV_Target {float2 uv=_CaptureOrigin.xy+i.uv.x*_CaptureAxisX.xy+i.uv.y*_CaptureAxisY.xy;return Blur(uv,_HorizontalStep.xy,true);}
   ENDHLSL
  }
  Pass {
   HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment vertical
   float4 vertical(v2f_img i):SV_Target {float4 c=Blur(_VerticalRect.xy+i.uv*_VerticalRect.zw,float2(0,_MainTex_TexelSize.y),false);c.a*=tex2D(_ClipMask,i.uv).a;
   #ifndef UNITY_COLORSPACE_GAMMA
    c.rgb=GammaToLinearSpace(c.rgb);
   #endif
    return c;}
   ENDHLSL
  }
 }
}
