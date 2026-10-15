Shader "EchoSphere/SourceUiSurfaceFilters"
{
 Properties { _MainTex("Source",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend One Zero
  HLSLINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_Original;float4 _SourceRect,_Direction,_ShadowColor,_Offset;float _Sigma,_StraightInput,_Amount,_Kind;
  float4 Sample(float2 uv){return any(uv<0)||any(uv>1)?0:tex2D(_MainTex,uv);}
  float4 Copy(v2f_img i):SV_Target {float4 c=Sample((i.uv-_SourceRect.xy)/_SourceRect.zw);if(_StraightInput>0){
   #ifndef UNITY_COLORSPACE_GAMMA
    c.rgb=LinearToGammaSpace(c.rgb);
   #endif
   c.rgb*=c.a;}return c;}
  float4 Blur(float2 uv){if(_Sigma<.001)return Sample(uv);int radius=(int)ceil(4*_Sigma);float4 sum=0;float total=0;[loop]for(int x=-radius;x<=radius;x++){float weight=exp(-.5*x*(float)x/(_Sigma*_Sigma));sum+=Sample(uv+_Direction.xy*x)*weight;total+=weight;}return sum/max(.000001,total);}
  float4 ColorBlur(v2f_img i):SV_Target{return Blur(i.uv);}
  float4 AlphaBlur(v2f_img i):SV_Target{return Blur(i.uv).aaaa;}
  float4 Shadow(v2f_img i):SV_Target {float alpha=Sample(i.uv-_Offset.xy).a*_ShadowColor.a;float4 shadow=float4(_ShadowColor.rgb*alpha,alpha);float4 original=tex2D(_Original,i.uv);return original+shadow*(1-original.a);}
  float4 ColorFilter(v2f_img i):SV_Target {float4 c=Sample(i.uv);float3 rgb=c.a>0?c.rgb/c.a:0;if(_Kind<.5){float luma=dot(rgb,float3(.213,.715,.072));rgb=lerp(luma.xxx,rgb,_Amount);}else if(_Kind<1.5)rgb*=_Amount;else if(_Kind<2.5)rgb=(rgb-.5)*_Amount+.5;else c.a*=_Amount;return float4(saturate(rgb)*c.a,c.a);}
  float4 Present(v2f_img i):SV_Target {float4 c=Sample(i.uv);c.rgb=c.a>0?c.rgb/c.a:0;
   #ifndef UNITY_COLORSPACE_GAMMA
    c.rgb=GammaToLinearSpace(c.rgb);
   #endif
   return c;}
  ENDHLSL
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment Copy
   ENDHLSL }
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment ColorBlur
   #pragma target 3.5
   ENDHLSL }
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment AlphaBlur
   #pragma target 3.5
   ENDHLSL }
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment Shadow
   ENDHLSL }
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment ColorFilter
   ENDHLSL }
  Pass { HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment Present
   ENDHLSL }
 }
}
