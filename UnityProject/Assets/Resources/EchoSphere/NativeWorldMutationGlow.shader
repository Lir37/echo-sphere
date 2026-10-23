Shader "EchoSphere/NativeWorldMutationGlow"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
            #include "UnityCG.cginc"
            float4 _NativeColor,_StopAlphas;float _Radius,_NativeOpacity;
            struct Varyings{float4 position:SV_POSITION;float2 local:TEXCOORD0;};
            Varyings vert(float4 v:POSITION){Varyings o;o.position=UnityObjectToClipPos(v);o.local=v.xy/max(_Radius,1e-12);return o;}
            float4 frag(Varyings i):SV_Target{float t=saturate(length(i.local)),a=t<.28?lerp(_StopAlphas.x,_StopAlphas.y,t/.28):t<.72?lerp(_StopAlphas.y,_StopAlphas.z,(t-.28)/.44):lerp(_StopAlphas.z,0,(t-.72)/.28);a*=_NativeOpacity;return float4(_NativeColor.rgb*a,a);}
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
