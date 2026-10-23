Shader "EchoSphere/NativeWorldFrameBackground"
{
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "UnityCG.cginc"
        float4 _FrameSize,_Center,_Middle;
        float _NativeOpacity;
        struct Varyings {float4 position:SV_POSITION;float2 local:TEXCOORD0;};
        Varyings vert(float4 vertex:POSITION){Varyings o;o.position=UnityObjectToClipPos(vertex);o.local=vertex.xy;return o;}
        float4 frag(Varyings i):SV_Target
        {
            float t=saturate(length(i.local-_FrameSize.xy*float2(.5,.46))/(max(_FrameSize.x,_FrameSize.y)*.72));
            float4 c=t<.68?lerp(_Center,_Middle,t/.68):lerp(_Middle,float4(2/255.,5/255.,11/255.,1),(t-.68)/.32);
            c.a*=_NativeOpacity;return float4(c.rgb*c.a,c.a);
        }
        ENDHLSL
        Pass
        {
            Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Blend One One
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Blend One OneMinusSrcColor
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
