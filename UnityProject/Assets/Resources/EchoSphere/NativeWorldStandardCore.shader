Shader "EchoSphere/NativeWorldStandardCore"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float _Radius,_NativeOpacity,_Family;float4 _NativeColor;
        struct Varyings {float4 position:SV_POSITION;float2 local:TEXCOORD0;};
        Varyings vert(float4 vertex:POSITION){Varyings o;o.position=UnityObjectToClipPos(vertex);o.local=vertex.xy;return o;}
        float4 frag(Varyings i):SV_Target
        {
            // Same two-circle radial solution as the authored Canvas gradient:
            // focal (-.08r,-.10r), inner radius 1, outer centre (0,0)/radius .62r.
            float2 a=float2(-.08,-.10)*_Radius,d=-a,p=i.local-a;float dr=.62*_Radius-1;
            float aa=dot(d,d)-dr*dr,bb=-2*(dot(p,d)+dr),cc=dot(p,p)-1;
            float disc=max(0,bb*bb-4*aa*cc),root=sqrt(disc),t=max((-bb-root)/(2*aa),(-bb+root)/(2*aa));
            if(1+t*dr<0)t=min((-bb-root)/(2*aa),(-bb+root)/(2*aa));t=saturate(t);
            float4 c0=float4(1,1,1,.98),c1=float4(176.0/255,244.0/255,1,.96),c2=float4(38.0/255,170.0/255,1,.78),c3=float4(20.0/255,92.0/255,220.0/255,.34),c4=float4(0,30.0/255,90.0/255,0);
            if(_Family>.5){c1.rgb=_NativeColor.rgb;c2.rgb=_NativeColor.rgb;c3.rgb=_NativeColor.rgb;c4.rgb=_NativeColor.rgb;}
            float4 c=t<.20?lerp(c0,c1,t/.20):t<.48?lerp(c1,c2,(t-.20)/.28):t<.82?lerp(c2,c3,(t-.48)/.34):lerp(c3,c4,(t-.82)/.18);
            c.a*=_NativeOpacity;return float4(c.rgb*c.a,c.a);
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
