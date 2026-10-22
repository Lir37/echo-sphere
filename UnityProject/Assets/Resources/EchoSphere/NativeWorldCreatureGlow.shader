Shader "EchoSphere/NativeWorldCreatureGlow"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float4 _NativeColor;
        float _NativeOpacity,_Radius,_GlowAlpha,_Core;
        struct Varyings {float4 position:SV_POSITION;float2 local:TEXCOORD0;};
        Varyings vert(float4 vertex:POSITION){Varyings o;o.position=UnityObjectToClipPos(vertex);o.local=vertex.xy/max(_Radius,1e-12);return o;}
        float4 frag(Varyings i):SV_Target
        {
            float t=saturate(length(i.local));float4 c;
            if(_Core>.5){float4 a=float4(1,1,1,.95),b=float4(_NativeColor.rgb,.95),d=float4(_NativeColor.rgb,0);c=t<.24?lerp(a,b,t/.24):lerp(b,d,(t-.24)/.76);}
            else{float4 a=float4(_NativeColor.rgb,_GlowAlpha*2.8),b=float4(_NativeColor.rgb,_GlowAlpha*.9),d=float4(_NativeColor.rgb,0);c=t<.35?lerp(a,b,t/.35):lerp(b,d,(t-.35)/.65);}
            c.a*=_NativeOpacity;return float4(c.rgb*c.a,c.a);
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
