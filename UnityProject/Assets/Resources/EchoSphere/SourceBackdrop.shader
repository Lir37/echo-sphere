Shader "EchoSphere/SourceBackdrop"
{
    Properties { _Viewport ("Source viewport", Vector) = (720,720,0,0) _Focus ("Source player", Vector) = (0,0,0,0) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float4 _Viewport, _Focus;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input input)
            {
                Output output; output.vertex=UnityObjectToClipPos(input.vertex); output.uv=input.uv;
                float2 world=mul(unity_ObjectToWorld,input.vertex).xy;
                output.world=float2(world.x,-world.y)/(12.6/720.0); return output;
            }
            float4 fieldGradient(float radius)
            {
                float t=saturate((radius-10.0)/(2400.0*.78-10.0));
                float4 a=float4(41,84,142,255*.18)/255.0, b=float4(58,37,121,255*.10)/255.0;
                float4 c=float4(3,10,23,255*.18)/255.0, d=float4(0,0,0,.82);
                if(t<=.26) return lerp(a,b,t/.26);
                if(t<=.62) return lerp(b,c,(t-.26)/.36);
                return lerp(c,d,(t-.62)/.38);
            }
            float3 over(float3 background,float4 foreground) { return lerp(background,foreground.rgb,foreground.a); }
            float lineCoverage(float distance) { return 1.0-smoothstep(.35,.8,abs(distance)); }
            fixed4 frag(Output input) : SV_Target
            {
                // renderer.ts drawVoidField; the literal \n arena block is commented out in the source.
                float2 screen=float2(input.uv.x,1.0-input.uv.y)*_Viewport.xy;
                float t=saturate(length(screen-_Viewport.xy*float2(.5,.46))/(max(_Viewport.x,_Viewport.y)*.72));
                float3 color=t<=.68?lerp(float3(7,16,29),float3(4,9,18),t/.68)/255.0:lerp(float3(4,9,18),float3(2,5,11),(t-.68)/.32)/255.0;
                float2 p=input.world;
                if(abs(p.x)<=1200 && abs(p.y)<=1200)
                {
                    color=over(color,fieldGradient(length(p-(_Focus.xy-float2(0,2400*.14)))));
                    if(length(p-float2(-1200+2400*.18,-1200+2400*.30))<=2400*.24) color=over(color,float4(44,168,255,255*.035)/255.0);
                    if(length(p-float2(-1200+2400*.76,-1200+2400*.28))<=2400*.22) color=over(color,float4(178,75,255,255*.030)/255.0);
                    if(length(p-float2(-1200+2400*.52,-1200+2400*.69))<=2400*.30) color=over(color,float4(70,117,255,255*.030)/255.0);
                    if(p.x>=-1200+2400*.08 && p.x<=-1200+2400*.92)
                        for(int i=1;i<5;i++) color=over(color,float4(float3(96,184,255)/255.0,.032*lineCoverage(p.y-(_Focus.y+2400*.22+i*i*2400*.045))));
                    if(p.x>=-1200+2400*.06 && p.x<=-1200+2400*.94)
                        for(int j=0;j<8;j++) color=over(color,float4(float3(115,206,255)/255.0,.055*lineCoverage(p.y-(-1200+2400*.40+pow(j/8.0,1.9)*2400*.48))));
                }
                #ifndef UNITY_COLORSPACE_GAMMA
                color=GammaToLinearSpace(color);
                #endif
                return float4(color,1);
            }
            ENDCG
        }
    }
}
