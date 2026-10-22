Shader "EchoSphere/NativeWorldPlayerCore"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float4 _NativeColor;float _Radius,_NativeOpacity;
        struct Varyings {float4 position:SV_POSITION;float2 local:TEXCOORD0;};
        Varyings vert(float4 vertex:POSITION){Varyings o;o.position=UnityObjectToClipPos(vertex);o.local=vertex.xy;return o;}
        float4 frag(Varyings i):SV_Target
        {
            float2 a=float2(-.18,-.20)*_Radius,d=-a,p=i.local-a;float dr=.55*_Radius-1;
            float aa=dot(d,d)-dr*dr,bb=-2*(dot(p,d)+dr),cc=dot(p,p)-1,disc=bb*bb-4*aa*cc;if(disc<0)return 0;
            float root=sqrt(disc),t=max((-bb-root)/(2*aa),(-bb+root)/(2*aa));if(1+t*dr<0)t=min((-bb-root)/(2*aa),(-bb+root)/(2*aa));if(1+t*dr<0)return 0;t=saturate(t);
            float4 c0=float4(1,1,1,1),c1=float4(_NativeColor.rgb,1),c2=float4(_NativeColor.rgb,.62),c3=0;
            float4 c=t<.28?lerp(c0,c1,t/.28):t<.68?lerp(c1,c2,(t-.28)/.40):lerp(c2,c3,(t-.68)/.32);c.a*=_NativeOpacity;return float4(c.rgb*c.a,c.a);
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
