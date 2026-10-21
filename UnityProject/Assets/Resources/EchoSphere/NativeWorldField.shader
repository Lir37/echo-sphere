Shader "EchoSphere/NativeWorldField"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Pass
        {
            Cull Off ZWrite Off ZTest Always Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Field;float _NativeOpacity;
            struct appdata {float4 vertex:POSITION;};
            struct v2f {float4 vertex:SV_POSITION;float2 position2D:TEXCOORD0;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.position2D=v.vertex.xy;return o;}
            float4 frag(v2f input):SV_Target
            {
                // Same two-circle gradient equation as the current Canvas GPU
                // backend: inner centre is offset and has a10px initial radius.
                float2 c0=float2(_Field.z,_Field.w-_Field.y*.14),c1=_Field.zw;
                float2 d=c1-c0,p=input.position2D-c0;float r0=10,dr=max(_Field.x,_Field.y)*.78-r0;
                float aa=dot(d,d)-dr*dr,bb=-2*(dot(p,d)+r0*dr),cc=dot(p,p)-r0*r0;
                float t;if(abs(aa)<1e-12){if(abs(bb)<1e-12)return 0;t=-cc/bb;}
                else {float disc=bb*bb-4*aa*cc;if(disc<0)return 0;float root=sqrt(disc),a=(-bb-root)/(2*aa),b=(-bb+root)/(2*aa);t=max(a,b);if(r0+t*dr<0)t=min(a,b);if(r0+t*dr<0)return 0;}
                float4 a=float4(41/255.,84/255.,142/255.,.18),b=float4(58/255.,37/255.,121/255.,.10),c=float4(3/255.,10/255.,23/255.,.18),darker=float4(0,0,0,.82);
                float4 color=t<.26?lerp(a,b,saturate(t/.26)):t<.62?lerp(b,c,saturate((t-.26)/.36)):lerp(c,darker,saturate((t-.62)/.38));
                color.a*=_NativeOpacity;return float4(color.rgb*color.a,color.a);
            }
            ENDHLSL
        }
    }
}
