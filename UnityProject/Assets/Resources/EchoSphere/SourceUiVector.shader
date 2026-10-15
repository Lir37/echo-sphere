Shader "EchoSphere/SourceUiVector"
{
 Properties { _MainTex("SVG gradient atlas",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   Blend One OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma vertex vertex
   #pragma fragment fragment
   #include "UnityCG.cginc"
   #include "Packages/com.unity.vectorgraphics/Runtime/Shaders/VectorGradient.cginc"
   sampler2D _MainTex;float4 _MainTex_TexelSize;float _HasAtlas;
   struct attributes {float4 position:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 setting:TEXCOORD2;};
   struct varyings {float4 position:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 setting:TEXCOORD2;};
   varyings vertex(attributes i){varyings o;o.position=UnityObjectToClipPos(i.position);o.color=i.color;o.uv=i.uv;o.setting=i.setting;return o;}
   float4 fragment(varyings i):SV_Target {float4 gradient=_HasAtlas>.5?EvaluateGradient(i.setting.x,i.uv,_MainTex,_MainTex_TexelSize.xy):float4(1,1,1,1);float4 color=gradient*i.color;color.rgb*=color.a;return color;}
   ENDHLSL
  }
  Pass {
   Blend Off
   HLSLPROGRAM
   #pragma vertex vert_img
   #pragma fragment straight
   #include "UnityCG.cginc"
   sampler2D _MainTex;
   float4 straight(v2f_img i):SV_Target {float4 color=tex2D(_MainTex,i.uv);color.rgb=color.a>0?color.rgb/color.a:0;
   #ifndef UNITY_COLORSPACE_GAMMA
    color.rgb=GammaToLinearSpace(color.rgb);
   #endif
    return color;}
   ENDHLSL
  }
 }
}
