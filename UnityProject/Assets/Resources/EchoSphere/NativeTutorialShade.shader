Shader "EchoSphere/NativeTutorialShade" {
 SubShader { Tags {"RenderPipeline"="UniversalPipeline"}
 Pass { Cull Off ZWrite Off ZTest Always Blend Off
 HLSLPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Pixels,_Cut;float _Scale;
 float4 frag(v2f_img i):SV_Target {
  float2 p=(floor(float2(i.uv.x,1-i.uv.y)*_Pixels.xy)+.5)/_Scale-_Cut.xy;
  float2 size=_Cut.zw;float radius=min(12,min(size.x,size.y)*.5);
  float2 center=float2(p.x<size.x*.5?radius:size.x-radius,p.y<size.y*.5?radius:size.y-radius);
  float distance=max(max(-p.x,p.x-size.x),max(-p.y,p.y-size.y));
  bool corner=(p.x<size.x*.5?p.x<radius:p.x>size.x-radius)&&(p.y<size.y*.5?p.y<radius:p.y>size.y-radius);
  if(radius>0&&corner)distance=length(p-center)-radius;
  float shape=saturate(.5-distance*_Scale);
  return float4(0,0,0,.72*(1-shape));
 }
 ENDHLSL
 } }
}
