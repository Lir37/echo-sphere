Shader "EchoSphere/NativeAppBackground"
{
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline"}
  Pass {
   Cull Off ZWrite Off ZTest Always Blend Off
   HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   float4 _LogicalSize,_BackingSize;
   float3 Over(float3 under,float3 over,float alpha){return lerp(under,over,saturate(alpha));}
   float Radial(float2 p,float2 size,float2 origin,float stop){float2 c=size*origin;return saturate(1-length(p-c)/(length(max(c,size-c))*stop));}
   float4 frag(v2f_img i):SV_Target {
    float2 size=_LogicalSize.xy;float2 p=(floor(float2(i.uv.x,1-i.uv.y)*_BackingSize.xy)+.5)/_BackingSize.z;
    float3 c=float3(2,5,12)/255;
    c=Over(c,float3(173,75,255)/255,.06*Radial(p,size,float2(.9,.2),.25));
    c=Over(c,float3(36,199,255)/255,.08*Radial(p,size,float2(.15,.85),.30));
    c=Over(c,float3(49,83,160)/255,.11*Radial(p,size,float2(.5,.4),.30));
    c=Over(c,lerp(float3(4,9,17),float3(1,4,9),p.y/size.y)/255,lerp(.92,.98,p.y/size.y));
    c=Over(c,float3(70,111,255)/255,.05*Radial(p,size,float2(.5,.5),.38));
    float2 q=p-floor(p/140)*140;
    float ax=.025*saturate(1-abs(q.x/140-.5)/.002),ay=.022*saturate(1-abs(q.y/140-.5)/.002);
    float2 center=size*.5;float t=length(p-center)/length(center);float mask=saturate((.84-t)/(.84-.18));
    c=Over(c,float3(120,211,255)/255,(ax+ay-ax*ay)*mask);
    return float4(c,1);
   }
   ENDHLSL
  }
 }
}
