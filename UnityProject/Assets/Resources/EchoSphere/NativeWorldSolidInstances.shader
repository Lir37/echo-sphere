Shader "EchoSphere/NativeWorldSolidInstances"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        UNITY_INSTANCING_BUFFER_START(NativeInstances)
            UNITY_DEFINE_INSTANCED_PROP(float4, _NativeColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _NativeOpacity)
        UNITY_INSTANCING_BUFFER_END(NativeInstances)
        struct Input {float4 vertex:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
        struct Varyings {float4 vertex:SV_POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
        Varyings vert(Input input){UNITY_SETUP_INSTANCE_ID(input);Varyings output;UNITY_TRANSFER_INSTANCE_ID(input,output);output.vertex=UnityObjectToClipPos(input.vertex);return output;}
        float4 frag(Varyings input):SV_Target {UNITY_SETUP_INSTANCE_ID(input);float4 color=UNITY_ACCESS_INSTANCED_PROP(NativeInstances,_NativeColor);float alpha=color.a*UNITY_ACCESS_INSTANCED_PROP(NativeInstances,_NativeOpacity);return float4(color.rgb*alpha,alpha);}
        ENDHLSL
        Pass {Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options forcemaxcount:64 nolodfade nolightprobe nolightmap
            ENDHLSL
        }
        Pass {Blend One One
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options forcemaxcount:64 nolodfade nolightprobe nolightmap
            ENDHLSL
        }
        Pass {Blend One OneMinusSrcColor, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options forcemaxcount:64 nolodfade nolightprobe nolightmap
            ENDHLSL
        }
    }
}
