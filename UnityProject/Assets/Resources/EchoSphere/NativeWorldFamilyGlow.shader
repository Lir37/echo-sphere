Shader "EchoSphere/NativeWorldFamilyGlow"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float4 _NativeColor;float _NativeOpacity;float4x4 _GradientMap;
        struct Varyings{float4 position:SV_POSITION;float2 gradient:TEXCOORD0;};
        Varyings vert(float4 vertex:POSITION){Varyings o;o.position=UnityObjectToClipPos(vertex);o.gradient=mul(_GradientMap,vertex).xy;return o;}
        float4 frag(Varyings i):SV_Target{float t=length(i.gradient),a=t<.20?lerp(.72,.38,saturate(t/.20)):t<.55?lerp(.38,.12,(t-.20)/.35):lerp(.12,0,saturate((t-.55)/.45));a*=_NativeOpacity;return float4(_NativeColor.rgb*a,a);}
        ENDHLSL
        Pass{Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass{Blend One One
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass{Blend One OneMinusSrcColor, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
