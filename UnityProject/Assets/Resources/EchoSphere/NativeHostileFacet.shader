Shader "EchoSphere/NativeHostileFacet"
{
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline"}
 Cull Off ZWrite Off ZTest Always
 HLSLINCLUDE
 #include "UnityCG.cginc"
 float4 _NativeColor;float _NativeOpacity,_Hit;
 struct appdata {float4 vertex:POSITION;float4 color:COLOR;};
 struct varyings {float4 vertex:SV_POSITION;float4 color:COLOR;};
 varyings vert(appdata v){varyings o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
 float4 frag(varyings i):SV_Target {float3 c=lerp(_NativeColor.rgb,i.color.rgb,i.color.a);c=lerp(c,float3(.86,.94,1),_Hit*.65);return float4(c*_NativeOpacity,_NativeOpacity);}
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
