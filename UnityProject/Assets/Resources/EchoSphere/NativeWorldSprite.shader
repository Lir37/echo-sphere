Shader "EchoSphere/NativeWorldSprite"
{
    Properties {_ImageTex("Imported sprite",2D)="white" {}}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        sampler2D _ImageTex;float4 _ImageTex_TexelSize;float _NativeOpacity;
        struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
        struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
        v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
        float4 SamplePixel(float2 uv){float4 c=tex2Dlod(_ImageTex,float4(uv,0,0));
            #ifndef UNITY_COLORSPACE_GAMMA
            c.rgb=LinearToGammaSpace(c.rgb);
            #endif
            return float4(c.rgb*c.a,c.a);}
        float4 frag(v2f i):SV_Target
        {
            float2 q=i.uv*_ImageTex_TexelSize.zw-.5,f=frac(q),a=(floor(q)+.5)*_ImageTex_TexelSize.xy;
            float4 color=lerp(lerp(SamplePixel(a),SamplePixel(a+float2(_ImageTex_TexelSize.x,0)),f.x),lerp(SamplePixel(a+float2(0,_ImageTex_TexelSize.y)),SamplePixel(a+_ImageTex_TexelSize.xy),f.x),f.y);return color*saturate(_NativeOpacity);
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
