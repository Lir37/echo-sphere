Shader "EchoSphere/NativeMenuPrefix"
{
 Properties { _MainTex("Previous straight prefix",2D)="black"{} _LayerTex("Native image",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always Blend Off
  CGINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_LayerTex;float4 _LayerRect,_LayerUv,_Tint,_Pixels,_Panel;float _Sigma;
  float4 Gamma(float4 color){
   #ifndef UNITY_COLORSPACE_GAMMA
    color.rgb=LinearToGammaSpace(color.rgb);
   #endif
   return color;
  }
  float2 Mirror(float2 p){p-=_Panel.xy;p=_Panel.zw-abs(_Panel.zw-frac(p/(2*_Panel.zw))*(2*_Panel.zw));return p+_Panel.xy;}
  float4 Sample(float2 p,bool horizontal){
   p=Mirror(p);float2 uv=(floor(p)+.5)/_Pixels.xy;uv.y=1-uv.y;
   float4 color=tex2D(_MainTex,uv);if(horizontal){color=Gamma(color);color.rgb*=color.a;}return color;
  }
  float4 Blur(v2f_img i,bool horizontal){
   float2 p=floor(float2(i.uv.x,1-i.uv.y)*_Pixels.xy)+.5;
   float4 result=Sample(p,horizontal);float total=1;
   int radius=(int)ceil(4*_Sigma);float2 axis=horizontal?float2(1,0):float2(0,1);
   [loop]for(int offset=1;offset<=radius;offset++){float weight=exp(-offset*(float)offset/(2*_Sigma*_Sigma));result+=(Sample(p+axis*offset,horizontal)+Sample(p-axis*offset,horizontal))*weight;total+=2*weight;}
   result/=total;if(!horizontal)result.rgb=result.a>0?result.rgb/result.a:0;return result;
  }
  ENDCG
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   float4 frag(v2f_img i):SV_Target {
    float4 previous=Gamma(tex2D(_MainTex,i.uv));float2 p=(float2(i.uv.x,1-i.uv.y)-_LayerRect.xy)/_LayerRect.zw;
    if(any(p<0)||any(p>1))return previous;
    float2 uv=_LayerUv.xy+float2(p.x,1-p.y)*_LayerUv.zw;
    float4 layer=Gamma(tex2D(_LayerTex,uv))*_Tint;
    float alpha=layer.a+previous.a*(1-layer.a);
    return float4(alpha>0?(layer.rgb*layer.a+previous.rgb*previous.a*(1-layer.a))/alpha:0,alpha);
   }
   ENDCG
  }
  Pass {
   CGPROGRAM
   #pragma target 3.5
   #pragma vertex vert_img
   #pragma fragment horizontal
   float4 horizontal(v2f_img i):SV_Target{return Blur(i,true);}
   ENDCG
  }
  Pass {
   CGPROGRAM
   #pragma target 3.5
   #pragma vertex vert_img
   #pragma fragment vertical
   float4 vertical(v2f_img i):SV_Target{return Blur(i,false);}
   ENDCG
  }
 // Persistent native stencil masks retain UI Toolkit's exact rounded clip.
 Pass {
  CGPROGRAM
  #pragma vertex vert_img
  #pragma fragment MaskAlpha
  float4 MaskAlpha(v2f_img i):SV_Target {float4 color=tex2D(_MainTex,i.uv);color.a*=tex2D(_LayerTex,i.uv).a;return color;}
  ENDCG
 }
 }
}
