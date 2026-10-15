Shader "EchoSphere/SourceCanvas"
{
    Properties { _Paints ("Source gradient stops",2D)="white" {} _ImageTex ("Source image",2D)="white" {} }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        HLSLINCLUDE
                        #include "UnityCG.cginc"
            sampler2D _Paints,_ImageTex;float4 _Paints_TexelSize,_ImageTex_TexelSize;
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float4 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float4 uv:TEXCOORD0;};
            v2f vert(appdata v) {v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
            float4 paintPixel(float x,float row) {return tex2Dlod(_Paints,float4((x+.5)*_Paints_TexelSize.x,(row+.5)*_Paints_TexelSize.y,0,0));}
            float4 gradient(float2 samplePosition,float row)
            {
                float4 a=paintPixel(0,row),b=paintPixel(1,row);float t;
                if(a.w>1.5) {float2 d=b.xy-a.xy;float square=dot(d,d);if(square<1e-12)return 0;t=dot(samplePosition-a.xy,d)/square;}
                else
                {
                    float2 d=b.xy-a.xy,p=samplePosition-a.xy;float dr=b.z-a.z;
                    float aa=dot(d,d)-dr*dr,bb=-2*(dot(p,d)+a.z*dr),cc=dot(p,p)-a.z*a.z;
                    if(abs(aa)<1e-12) {if(abs(bb)<1e-12)return 0;t=-cc/bb;}
                    else {float disc=bb*bb-4*aa*cc;if(disc<0)return 0;float root=sqrt(disc);float t0=(-bb-root)/(2*aa),t1=(-bb+root)/(2*aa);t=max(t0,t1);if(a.z+t*dr<0)t=min(t0,t1);if(a.z+t*dr<0)return 0;}
                }
                int count=(int)b.w;if(count<=0)return 0;
                float4 previous=paintPixel(2,row);float previousOffset=paintPixel(3,row).r;
                if(t<previousOffset)return previous;
                [loop]for(int i=1;i<16;i++)
                {
                    if(i>=count)break;float4 next=paintPixel(2+i*2,row);float offset=paintPixel(3+i*2,row).r;
                    if(t<offset)return lerp(previous,next,saturate((t-previousOffset)/max(1e-12,offset-previousOffset)));
                    previous=next;previousOffset=offset;
                }
                return previous;
            }
            float4 imagePixel(float2 uv)
            {
                float4 c=tex2Dlod(_ImageTex,float4(uv,0,0));
                #ifndef UNITY_COLORSPACE_GAMMA
                c.rgb=LinearToGammaSpace(c.rgb);
                #endif
                return float4(c.rgb*c.a,c.a);
            }
            float4 imageSample(float2 uv)
            {
                // Canvas interpolates image pixels with premultiplied alpha.
                // Hardware interpolation of straight RGB darkens transparent
                // sprite edges and lets invisible RGB bleed into the result.
                float2 samplePosition=uv*_ImageTex_TexelSize.zw-.5;float2 fraction=frac(samplePosition);float2 first=(floor(samplePosition)+.5)*_ImageTex_TexelSize.xy;
                return lerp(lerp(imagePixel(first),imagePixel(first+float2(_ImageTex_TexelSize.x,0)),fraction.x),lerp(imagePixel(first+float2(0,_ImageTex_TexelSize.y)),imagePixel(first+_ImageTex_TexelSize.xy),fraction.x),fraction.y);
            }
            float4 frag(v2f input):SV_Target
            {
                float4 color=input.color;
                if(input.uv.z>.5)color*=gradient(input.uv.xy,input.uv.z-1);
                if(input.uv.z<-.5) {
                    float4 image=imageSample(input.uv.xy);float opacity=saturate(color.a);
                    return float4(image.rgb*color.rgb*opacity,image.a*opacity);
                }
                float alpha=saturate(color.a);return float4(color.rgb*alpha,alpha);
            }
        ENDHLSL
        Pass { ZWrite Off Cull Off Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            ENDHLSL
        }
        Pass { ZWrite Off Cull Off Blend One One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass { ZWrite Off Cull Off Blend One OneMinusSrcColor, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
