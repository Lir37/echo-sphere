Shader "EchoSphere/NativeWorldLabel"
{
    Properties {_Glyphs("Glyph atlas",2D)="white" {}}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
            #include "UnityCG.cginc"
            sampler2D _Glyphs;float4 _Glyphs_TexelSize,_LabelOffset,_LabelColor;float _NativeOpacity;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            v2f vert(appdata v){v2f o;v.vertex.xy+=_LabelOffset.xy;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
            float4 frag(v2f i):SV_Target
            {
                float2 q=i.uv*_Glyphs_TexelSize.zw-.5,f=frac(q),a=(floor(q)+.5)*_Glyphs_TexelSize.xy;
                float glyph=lerp(lerp(tex2Dlod(_Glyphs,float4(a,0,0)).a,tex2Dlod(_Glyphs,float4(a+float2(_Glyphs_TexelSize.x,0),0,0)).a,f.x),lerp(tex2Dlod(_Glyphs,float4(a+float2(0,_Glyphs_TexelSize.y),0,0)).a,tex2Dlod(_Glyphs,float4(a+_Glyphs_TexelSize.xy,0,0)).a,f.x),f.y);
                float alpha=glyph*_LabelColor.a*_NativeOpacity;return float4(_LabelColor.rgb*alpha,alpha);
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
