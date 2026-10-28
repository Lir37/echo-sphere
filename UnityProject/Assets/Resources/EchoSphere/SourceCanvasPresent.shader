Shader "EchoSphere/SourceCanvasPresent" {
    Properties {_MainTex ("Source Canvas",2D)="black" {}}
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float _SourceFullscreen;
        v2f_img SourceFullscreenVertex(appdata_img v){v2f_img o=vert_img(v);if(_SourceFullscreen>.5)o.pos=v.vertex;return o;}
        ENDHLSL
        Pass {
            ZWrite Off Cull Off Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
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
            #pragma vertex SourceFullscreenVertex
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
        // Native run Image receives the complete authored color treatment.
        // Avoid retained UI filter render targets becoming stale on viewport resize.
        Pass {
            ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
            #pragma fragment nativeFieldFragment
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 nativeFieldFragment(v2f_img input):SV_Target {
                float4 color=tex2D(_MainTex,input.uv);float3 rgb=color.a>0?color.rgb/color.a:0;
                // Preserve the order AND per-stage clamping of the three UI passes.
                float luma=dot(rgb,float3(.213,.715,.072));
                rgb=saturate(lerp(luma.xxx,rgb,1.16));
                rgb=saturate((rgb-.5)*1.08+.5);
                rgb=saturate(rgb*.98);
                #ifndef UNITY_COLORSPACE_GAMMA
                rgb=GammaToLinearSpace(rgb);
                #endif
                return float4(rgb,color.a);
            }
            ENDHLSL
        }
    }
}
