Shader "EchoSphere/NativeMenuCore"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Cull Off ZWrite Off ZTest Always Blend Off
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _LogicalSize, _BackingSize;
            float _AnimationTime;
            float Disk(float r, float radius, float aa) { return 1-smoothstep(radius-aa,radius+aa,r); }
            float Band(float r, float radius, float width, float aa) { return 1-smoothstep(width-aa,width+aa,abs(r-radius)); }
            float Segment(float2 p, float2 a, float2 b, float width, float aa)
            {
                float2 delta=b-a;
                return 1-smoothstep(width-aa,width+aa,length(p-a-delta*saturate(dot(p-a,delta)/dot(delta,delta))));
            }
            void Over(inout float4 dst,float3 color,float alpha)
            {
                alpha=saturate(alpha);
                dst.rgb=color*alpha+dst.rgb*(1-alpha); dst.a=alpha+dst.a*(1-alpha);
            }
            float4 frag(v2f_img input):SV_Target
            {
                float2 p=(input.uv-.5)*2;
                p.x*=_LogicalSize.x/_LogicalSize.y;
                float r=length(p), angle=atan2(p.y,p.x), aa=2.0/_BackingSize.y;
                float t=_AnimationTime;
                float3 cyan=float3(.28,.84,1), ice=float3(.72,.96,1), muted=float3(.11,.32,.43);
                float4 c=0;
                // Restrained halo, with all its extent inside the retained box.
                Over(c,cyan,.11*exp(-18*abs(r-.51))*Disk(r,.93,aa));
                Over(c,muted,Band(r,.86,.0025,aa)*.4);
                float outerPhase=frac((angle+.15-t*.12)/6.2831853*3);
                Over(c,cyan,Band(r,.77,.006,aa)*step(.065,outerPhase)*step(outerPhase,.89)*.5);
                // Flat dark shell and six separated conducting armour sectors.
                Over(c,float3(.018,.054,.083),Disk(r,.61,aa));
                Over(c,cyan,Band(r,.60,.008,aa)*.8);
                float sector=abs(frac((angle+.5235988+t*.075)/1.0471976)-.5)*2;
                float plates=step(.15,sector)*step(sector,.85);
                float plateBand=Disk(r,.55,aa)*(1-Disk(r,.405,aa))*plates;
                Over(c,float3(.045,.17,.23),plateBand);
                Over(c,cyan,Band(r,.55,.004,aa)*plates*.7);
                Over(c,ice,Band(r,.405,.004,aa)*plates*.55);
                // Three circuit bridges and detached nodes; no perspective or gloss.
                [unroll] for(int k=0;k<3;k++)
                {
                    float theta=1.5707963+k*2.0943951+t*.20;
                    float2 axis=float2(cos(theta),sin(theta));
                    Over(c,cyan,Segment(p,axis*.61,axis*.71,.005,aa)*.75);
                    float d=length(p-axis*.74);
                    Over(c,float3(.025,.10,.15),Disk(d,.052,aa));
                    Over(c,ice,Band(d,.052,.006,aa)*.9);
                    Over(c,cyan,Disk(d,.021,aa));
                }
                // Contained luminous core, concentric structure instead of a white blob.
                Over(c,cyan,(.14+.012*sin(t*1.1))*exp(-r*r/0.035));
                Over(c,float3(.025,.14,.20),Disk(r,.32,aa));
                Over(c,cyan,Band(r,.32,.006,aa));
                Over(c,muted,Band(r,.265,.003,aa)*.7);
                float inner=abs(frac((angle+.2-t*.32)/2.0943951)-.5)*2;
                Over(c,ice,Band(r,.215,.012,aa)*step(.18,inner)*.8);
                float travel=frac((angle-t*.8)/6.2831853);
                Over(c,ice,Band(r,.265,.005,aa)*pow(saturate(1-travel*7),3)*.85);
                float diamond=abs(p.x)+abs(p.y);
                Over(c,cyan,Disk(diamond,.135,aa));
                Over(c,ice,Disk(diamond,.083,aa));
                Over(c,float3(.93,1,1),Disk(diamond,.033,aa));
                // UI Toolkit expects straight-alpha texture samples.
                c.rgb/=max(c.a,1e-6);
                return c;
            }
            ENDHLSL
        }
    }
}
