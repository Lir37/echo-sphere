Shader "EchoSphere/SourceCanvasShadow" {
    Properties { _MainTex ("Source mask",2D)="black" {} }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;float4 _SourceShadowDirection,_SourceShadowColor,_SourceShadowOffset;float _SourceShadowSigma;
        float4 gaussian(v2f_img input):SV_Target {
            float sigma=_SourceShadowSigma;if(sigma<.01)return tex2D(_MainTex,input.uv).aaaa;
            int radius=(int)ceil(sigma*4);float total=0,alpha=0;
            [loop] for(int i=-radius;i<=radius;i++) {float weight=exp(-.5*(float)(i*i)/(sigma*sigma));total+=weight;alpha+=tex2D(_MainTex,input.uv+_SourceShadowDirection.xy*i).a*weight;}
            return (alpha/total).xxxx;
        }
        float4 tint(v2f_img input):SV_Target {float alpha=tex2D(_MainTex,input.uv-_SourceShadowOffset.xy).a*_SourceShadowColor.a;return float4(_SourceShadowColor.rgb*alpha,alpha);}
        ENDHLSL
        Pass { HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment gaussian
            #pragma target 3.5
            ENDHLSL
        }
        Pass { Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment tint
            ENDHLSL
        }
        Pass { Blend One One
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment tint
            ENDHLSL
        }
        Pass { Blend One OneMinusSrcColor, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment tint
            ENDHLSL
        }
    }
}
