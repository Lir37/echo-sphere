Shader "EchoSphere/NativeWorldSolid"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float4 _NativeColor;float _NativeOpacity;
        float4 vert(float4 vertex:POSITION):SV_POSITION{return UnityObjectToClipPos(vertex);}
        float4 frag():SV_Target{float alpha=_NativeColor.a*_NativeOpacity;return float4(_NativeColor.rgb*alpha,alpha);}
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
