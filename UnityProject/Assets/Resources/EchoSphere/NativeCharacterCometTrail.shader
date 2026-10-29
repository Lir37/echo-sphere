Shader "EchoSphere/NativeCharacterCometTrail"
{
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off ZTest Always
  HLSLINCLUDE
  #include "UnityCG.cginc"
  float _NativeOpacity;
  struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
  struct Varyings {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
  Varyings vert(Input v){Varyings o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
  float4 frag(Varyings i):SV_Target {
   float across=abs(i.uv.y*2-1);float feather=1-smoothstep(.22,1,across);
   float alpha=i.color.a*_NativeOpacity*feather;
   float core=(1-smoothstep(.04,.32,across))*.20*i.uv.x*i.uv.x;
   return float4(lerp(i.color.rgb,1,core)*alpha,alpha);
  }
  ENDHLSL
  Pass {Blend One OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert
   #pragma fragment frag
   ENDHLSL
  }
  Pass {Blend One One
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert
   #pragma fragment frag
   ENDHLSL
  }
  Pass {Blend One OneMinusSrcColor, One OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert
   #pragma fragment frag
   ENDHLSL
  }
 }
}
