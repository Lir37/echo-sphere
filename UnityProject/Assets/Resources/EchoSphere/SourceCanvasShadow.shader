Shader "EchoSphere/SourceCanvasShadow" {
    Properties { _MainTex ("Source mask",2D)="black" {} }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float _SourceFullscreen;
        v2f_img SourceFullscreenVertex(appdata_img v){v2f_img o=vert_img(v);if(_SourceFullscreen>.5)o.pos=v.vertex;return o;}
        sampler2D _MainTex;float4 _SourceShadowDirection,_SourceShadowColor,_SourceShadowOffset,_SourceShadowRect;float _SourceShadowSigma;
        float4 _SourceShadowWeights[128];int _SourceShadowWeightCount;
        float _SourceShadowAtlas;float4 _SourceShadowSampleRect;
        float Alpha(float2 uv){if(_SourceShadowAtlas>.5&&(any(uv<_SourceShadowSampleRect.xy)||any(uv>_SourceShadowSampleRect.zw)))return 0;return any(uv<0)||any(uv>1)?0:tex2D(_MainTex,uv).a;}
        float4 gaussian(v2f_img input):SV_Target {
            float sigma=_SourceShadowSigma;if(sigma<.01)return Alpha(input.uv).xxxx;
            int radius=(int)ceil(sigma*4);float total=0,alpha=0;
            [branch] if(_SourceShadowWeightCount>radius){
                float center=_SourceShadowWeights[0].x;total=center;alpha=Alpha(input.uv)*center;
                // Linear filtering evaluates two adjacent weighted taps exactly.
                [loop] for(int i=1;i<=radius;i+=2){int j=min(i+1,radius);float a=_SourceShadowWeights[i>>2][i&3],b=i<radius?_SourceShadowWeights[j>>2][j&3]:0;float weight=a+b;if(weight>0){float offset=i+b/weight;total+=2*weight;alpha+=(Alpha(input.uv+_SourceShadowDirection.xy*offset)+Alpha(input.uv-_SourceShadowDirection.xy*offset))*weight;}}
            }else{
                [loop] for(int i=-radius;i<=radius;i++){float weight=exp(-.5*(float)(i*i)/(sigma*sigma));total+=weight;alpha+=Alpha(input.uv+_SourceShadowDirection.xy*i)*weight;}
            }
            return (alpha/total).xxxx;
        }
        float4 tint(v2f_img input):SV_Target {float alpha=Alpha(_SourceShadowRect.xy+input.uv*_SourceShadowRect.zw-_SourceShadowOffset.xy)*_SourceShadowColor.a;return float4(_SourceShadowColor.rgb*alpha,alpha);}
        ENDHLSL
        Pass { HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
            #pragma fragment gaussian
            #pragma target 3.5
            ENDHLSL
        }
        Pass { Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
            #pragma fragment tint
            ENDHLSL
        }
        Pass { Blend One One
            HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
            #pragma fragment tint
            ENDHLSL
        }
        Pass { Blend One OneMinusSrcColor, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex SourceFullscreenVertex
            #pragma fragment tint
            ENDHLSL
        }
    }
}
