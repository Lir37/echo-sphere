Shader "EchoSphere/NativeRegionBackground" {
 SubShader { Tags {"RenderPipeline"="UniversalPipeline"}
 Pass { Cull Off ZWrite Off ZTest Always Blend Off
 HLSLPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _LogicalSize,_BackingSize;
 float4 Star(float2 p,float2 size,float2 origin,float3 color,float alpha){float a=alpha*saturate((1.7-length(p-size*origin))/.7);return float4(color/255*a,a);}
 float4 Over(float4 a,float4 b){return b+a*(1-b.a);}
 float4 frag(v2f_img i):SV_Target {
  float2 size=_LogicalSize.xy,p=(floor(float2(i.uv.x,1-i.uv.y)*_BackingSize.xy)+.5)/_BackingSize.z;float t=p.y/size.y;
  float3 c=(t<.58?lerp(float3(2,6,13),float3(4,10,20),t/.58):lerp(float3(4,10,20),float3(2,5,11),(t-.58)/.42))/255;
  float2 center=size*float2(.5,.15);float alpha=.055*saturate(1-length(p-center)/(.28*length(max(center,size-center))));c=lerp(c,float3(57,216,255)/255,alpha);
  float4 stars=Star(p,size,float2(.22,.82),float3(170,225,255),.42);
  stars=Over(stars,Star(p,size,float2(.88,.74),float3(214,191,255),.44));
  stars=Over(stars,Star(p,size,float2(.77,.14),float3(170,211,255),.55));
  stars=Over(stars,Star(p,size,float2(.15,.18),float3(224,249,255),.76));
  return float4(c*(1-stars.a*.20)+stars.rgb*.20,1);
 }
 ENDHLSL
 } }
}
