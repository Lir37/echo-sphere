Shader "EchoSphere/SourceUiDepth"
{
 Properties { _MainTex("Plane", 2D)="white" {} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  HLSLINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_ClipMask;float _Opacity,_ClipEnabled;float4 _Bounds,_Eye,_ClipRect;
  struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
  struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 plane:TEXCOORD1;};
  output vert(input v){output o;float w=1-v.vertex.z/_Eye.z;float2 p=v.vertex.xy-_Eye.xy+(_Eye.xy-_Bounds.xy)*w;o.vertex=float4(2*p.x/_Bounds.z-w,w-2*p.y/_Bounds.w,0,w);o.uv=v.uv;o.plane=v.vertex.xy;return o;}
  float4 plane(output i):SV_Target {float4 c=tex2D(_MainTex,i.uv);
  #ifndef UNITY_COLORSPACE_GAMMA
   c.rgb=LinearToGammaSpace(c.rgb);
  #endif
   float clip=1;if(_ClipEnabled>.5){float2 uv=(i.plane-_ClipRect.xy)/_ClipRect.zw;clip=all(uv>=0)&&all(uv<=1)?tex2D(_ClipMask,float2(uv.x,1-uv.y)).a:0;}
   c.a*=_Opacity*clip;c.rgb*=c.a;return c;}
  ENDHLSL
  Pass {
   Blend One OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment plane
   ENDHLSL
  }
  Pass {
   Blend Off
   HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   float4 frag(v2f_img i):SV_Target {float4 c=tex2D(_MainTex,i.uv);c.rgb=c.a>0?c.rgb/c.a:0;
   #ifndef UNITY_COLORSPACE_GAMMA
    c.rgb=GammaToLinearSpace(c.rgb);
   #endif
    return c;}
   ENDHLSL
  }
  Pass {
   Blend OneMinusDstColor One, One OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment plane
   ENDHLSL
  }
 }
}
