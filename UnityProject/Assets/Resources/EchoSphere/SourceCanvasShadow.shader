Shader "EchoSphere/SourceCanvasShadow" {
    Properties { _MainTex ("Source mask",2D)="black" {} }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;float4 _SourceShadowDirection,_SourceShadowColor,_SourceShadowOffset,_SourceShadowRect;float _SourceShadowSigma;
        float Alpha(float2 uv){return any(uv<0)||any(uv>1)?0:tex2D(_MainTex,uv).a;}
        float4 gaussian(v2f_img input):SV_Target {
            float sigma=_SourceShadowSigma;if(sigma<.01)return Alpha(input.uv).xxxx;
            int radius=(int)ceil(sigma*4);float total=0,alpha=0;
            [loop] for(int i=-radius;i<=radius;i++) {float weight=exp(-.5*(float)(i*i)/(sigma*sigma));total+=weight;alpha+=Alpha(input.uv+_SourceShadowDirection.xy*i)*weight;}
            return (alpha/total).xxxx;
        }
        float4 tint(v2f_img input):SV_Target {float alpha=Alpha(_SourceShadowRect.xy+input.uv*_SourceShadowRect.zw-_SourceShadowOffset.xy)*_SourceShadowColor.a;return float4(_SourceShadowColor.rgb*alpha,alpha);}
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
