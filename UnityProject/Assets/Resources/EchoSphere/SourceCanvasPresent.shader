Shader "EchoSphere/SourceCanvasPresent" {
    Properties {_MainTex ("Source Canvas",2D)="black" {}}
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Pass {
            ZWrite Off Cull Off Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 frag(v2f_img input):SV_Target {
                float4 color=tex2D(_MainTex,input.uv);float3 rgb=color.a>0?color.rgb/color.a:0;
                // Active !important .es-game-screen canvas rule in conceptStyle.css.
                float luma=dot(rgb,float3(.213,.715,.072));rgb=lerp(luma.xxx,rgb,1.16);rgb=((rgb-.5)*1.08+.5)*.98;
                #ifndef UNITY_COLORSPACE_GAMMA
                rgb=GammaToLinearSpace(rgb);
                #endif
                return float4(saturate(rgb)*color.a,color.a);
            }
            ENDHLSL
        }
        Pass {
            ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment uiFragment
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 uiFragment(v2f_img input):SV_Target {
                float4 color=tex2D(_MainTex,input.uv);color.rgb=color.a>0?color.rgb/color.a:0;
                #ifndef UNITY_COLORSPACE_GAMMA
                color.rgb=GammaToLinearSpace(color.rgb);
                #endif
                return color;
            }
            ENDHLSL
        }
    }
}
