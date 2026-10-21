Shader "EchoSphere/NativeWorldRegion"
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
            float _NativeOpacity;
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float4 shape:TEXCOORD0;float4 arc:TEXCOORD1;};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float4 shape:TEXCOORD0;float4 arc:TEXCOORD1;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.shape=v.shape;o.arc=v.arc;return o;}
            float4 frag(v2f i):SV_Target
            {
                float coverage=1;
                if(i.shape.z>=0){float radius=length(i.shape.xy);float d=i.shape.w<0?radius-i.shape.z:abs(radius-i.shape.z)-i.shape.w*.5;
                    if(i.arc.z>.5){float centre=(i.arc.x+i.arc.y)*.5,halfSpan=(i.arc.y-i.arc.x)*.5;float angle=atan2(i.shape.y,i.shape.x)-centre;angle=atan2(sin(angle),cos(angle));float endDistance=radius*sin(clamp(abs(angle)-halfSpan,-1.570796327,1.570796327));d=max(d,endDistance);}
                    coverage=saturate(.5-d/max(.00001,fwidth(d)));}
                float alpha=i.color.a*coverage*_NativeOpacity;return float4(i.color.rgb*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
