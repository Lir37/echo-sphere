Shader "EchoSphere/NativeWorldRetainedRing" {
 Properties {_ImageTex("Retained ring",2D)="black" {}}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off ZTest Always
 HLSLINCLUDE
 #include "UnityCG.cginc"
 sampler2D _ImageTex;float _NativeOpacity;
 struct I {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct O {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 O vert(I i){O o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
 float4 frag(O i):SV_Target{return tex2D(_ImageTex,i.uv)*_NativeOpacity;}
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
