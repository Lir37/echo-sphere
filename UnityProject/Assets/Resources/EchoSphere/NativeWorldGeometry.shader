Shader "EchoSphere/NativeWorldGeometry"
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
            float4 _Field,_Ambient;float _NativeOpacity;
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float4 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float3 shape:TEXCOORD0;};
            v2f vert(appdata v)
            {
                v2f o;o.color=v.color;
                if(v.uv.w>1.5){float i=v.uv.w-2;float baseY=fmod(i*53.71,_Ambient.y);v.vertex.y+=fmod(i*53.71+_Ambient.z*(2+fmod(i,3)),_Ambient.y)-baseY;o.color.a*=.28+.22*sin(_Ambient.z*1.4+i);}
                else v.vertex.y+=v.uv.w*_Field.w;
                o.vertex=UnityObjectToClipPos(v.vertex);o.shape=v.uv.xyz;return o;
            }
            float4 frag(v2f input):SV_Target
            {
                float coverage=1;
                if(input.shape.z>.5){float d=length(input.shape.xy)-1;coverage=saturate(.5-d/max(1e-6,fwidth(d)));}
                float alpha=input.color.a*coverage*_NativeOpacity;return float4(input.color.rgb*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
