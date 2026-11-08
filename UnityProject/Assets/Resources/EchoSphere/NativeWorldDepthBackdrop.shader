Shader "EchoSphere/NativeWorldDepthBackdrop"
{
    Properties
    {
        _Backdrop("Distant environment",2D)="black"{}
        _PreviousBackdrop("Previous environment",2D)="black"{}
    }
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
            sampler2D _Backdrop,_PreviousBackdrop;
            float4 _FrameSize,_DepthCamera,_DepthStyle,_DepthReady;
            float _NativeOpacity;
            struct Varyings {float4 position:SV_POSITION;float2 local:TEXCOORD0;};
            Varyings vert(float4 position:POSITION)
            {Varyings o;o.position=UnityObjectToClipPos(position);o.local=position.xy;return o;}
            float Line(float value,float period,float width)
            {float d=abs(frac(value/period+.5)-.5)*period;return 1-smoothstep(width,width+max(.65,fwidth(value)),d);}
            float3 Environment(float2 uv)
            {
                float region=_DepthStyle.x,trial=_DepthStyle.y;
                // Displaced mineral seams, not displaced world or input coordinates.
                if(region>2.5&&region<3.5&&trial>2.5)
                    uv.x+=(frac(floor(uv.y*11)*.618)-.5)*.006;
                float3 c=tex2D(_Backdrop,saturate(uv)).rgb;
                if((region>0.5&&region<1.5&&trial>2.5)||(region>4.5&&trial>2.5))
                {
                    float cells=step(.53,frac(floor(uv.x*5)*.37+floor(uv.y*8)*.61));
                    float3 echo=tex2D(_Backdrop,saturate(uv+float2(.018,-.012))).rgb;
                    c=lerp(c,echo,cells*(region>4.5?.15:.10));
                }
                if(region>.5&&region<1.5&&trial>.5&&trial<1.5)
                {
                    float cycle=fmod(_DepthStyle.z,72);
                    float phase=smoothstep(35.4,36.6,cycle)*(1-smoothstep(71.4,72,cycle));
                    float3 mirror=tex2D(_Backdrop,float2(1-saturate(uv.x),saturate(uv.y))).rgb;
                    c=lerp(c,mirror,.10*phase);
                }
                float dark=0;float3 tint=float3(1,1,1);
                float2 q=uv*float2(640,1120);
                if(trial>.5)
                {
                    if(region<.5)
                    {
                        if(trial<1.5)dark=Line(q.x+q.y*.37,170,1.2)*.12;
                        else if(trial<2.5)tint=float3(1.10,.99,.93);
                        else c=lerp(c,tex2D(_Backdrop,saturate(uv+float2(.005,.008))).rgb,.08);
                    }
                    else if(region<1.5){if(trial>1.5&&trial<2.5)tint=float3(1.06,.96,1.07);}
                    else if(region<2.5)
                    {
                        if(trial<1.5)dark=Line(q.x-q.y*.53,180,1.4)*.15;
                        else if(trial<2.5)tint=float3(1.04,1.07,.94);
                        else dark=max(Line(q.x+q.y*.35,110,1.5),Line(q.y-q.x*.2,140,1))*.13;
                    }
                    else if(region<3.5)
                    {
                        if(trial<1.5)tint=float3(.97,1.02,1.06);
                        else if(trial<2.5)dark=max(Line(q.x,150,1),Line(q.y,180,1))*.13;
                        else dark=Line(q.x+q.y*.47,180,1.1)*.12;
                    }
                    else if(region<4.5)
                    {
                        if(trial<1.5)tint=float3(.88,.88,.90);
                        else if(trial<2.5){float band=.5+.5*sin((q.x+q.y*.4)*.012);tint=lerp(float3(1,1,1),float3(1.10,1.02,.94),band);}
                        else dark=Line(q.x+q.y*.32,170,1.4)*step(.45,frac(q.y/130))*.16;
                    }
                    else
                    {
                        if(trial<1.5)dark=max(Line(q.x,140,2),Line(q.y,200,2))*.14;
                        else if(trial<2.5)tint=float3(.91,.88,.98);
                    }
                }
                return c*tint*(1-dark);
            }
            float4 frag(Varyings i):SV_Target
            {
                float2 p=i.local-_FrameSize.xy*.5;
                float2 distant=p+_DepthCamera.xy*_DepthCamera.z;
                float2 uv=float2(.5+(distant.x+_DepthCamera.w*distant.y)/_FrameSize.z,
                                  .5-distant.y*_DepthReady.z/_FrameSize.w);
                float3 fallback=float3(.012,.020,.031);
                float3 current=_DepthReady.x>.5?Environment(uv):fallback;
                float3 previous=_DepthReady.y>.5?tex2D(_PreviousBackdrop,saturate(uv)).rgb:fallback;
                float3 c=lerp(previous,current,_DepthStyle.w);
                // The arena is a quiet translucent plane above the distant scene.
                float luma=dot(c,float3(.2126,.7152,.0722));
                c=lerp(luma.xxx,c,.88);
                c=lerp(c,fallback,.10);
                float2 nearWorld=p+_DepthCamera.xy;
                float grid=max(Line(nearWorld.x,120,.35),Line(nearWorld.y,120,.35));
                c+=float3(.004,.007,.009)*grid;
                return float4(c*_NativeOpacity,_NativeOpacity);
            }
            ENDHLSL
        }
    }
}
