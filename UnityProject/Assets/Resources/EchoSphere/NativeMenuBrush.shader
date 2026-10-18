Shader "EchoSphere/NativeMenuBrush"
{
    Properties { _MainTex ("Source", 2D) = "black" {} _OuterA ("Outer shadow A", 2D) = "black" {} _OuterB ("Outer shadow B", 2D) = "black" {} _Inner ("Inset shadow", 2D) = "black" {} }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always Blend Off
        HLSLINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_OuterA,_OuterB,_Inner;
        float4 _MainTex_TexelSize,_Size,_Pixels,_MaskSize,_Offset,_Direction,_Radius,_Fill,_BorderColor,_Origin,_ShadowA,_ShadowB,_ShadowInner;
        float4 _Colors[8];float _Stops[8];int _KernelRadius,_StopCount;
        float _Outside,_Border,_Linear,_Sigma;
        float2 Pixel(float2 uv,float2 size){return floor(float2(uv.x,1-uv.y)*size)+.5;}
        float Shape(float2 p,float2 size,float2 radius)
        {
            if(any(size<=0))return 0;
            float2 center=float2(p.x<size.x*.5?radius.x:size.x-radius.x,p.y<size.y*.5?radius.y:size.y-radius.y);
            float distance=max(max(-p.x,p.x-size.x),max(-p.y,p.y-size.y));
            bool corner=(p.x<size.x*.5?p.x<radius.x:p.x>size.x-radius.x)&&(p.y<size.y*.5?p.y<radius.y:p.y>size.y-radius.y);
            if(all(radius>0)&&corner){float2 d=p-center;float norm=length(d/radius);float gradient=norm>0?length(d/(radius*radius))/norm:1/min(radius.x,radius.y);distance=(norm-1)/max(.00001,gradient);}
            return saturate(.5-distance*_Size.z);
        }
        float4 Premul(float4 c){return float4(c.rgb*c.a,c.a);}
        float4 Over(float4 under,float4 over){return over+under*(1-over.a);}
        float maskFragment(v2f_img input):SV_Target
        {
            float2 p=(Pixel(input.uv,_MaskSize.xy)-_MaskSize.z)/_Size.z-_Size.w;
            float shape=Shape(p-_Offset.xy,_Size.xy,_Radius.xy);
            return _MaskSize.w>0?1-shape:shape;
        }
        float blurFragment(v2f_img input):SV_Target
        {
            float value=0,total=0;
            [loop]for(int i=-_KernelRadius;i<=_KernelRadius;i++){
                float2 uv=input.uv+_Direction.xy*i;
                float sampleValue=any(uv<0)||any(uv>1)?_Outside:tex2Dlod(_MainTex,float4(uv,0,0)).r;
                float weight=exp(-float(i)*i/(2*_Sigma*_Sigma));value+=sampleValue*weight;total+=weight;
            }
            return value/total;
        }
        float cropFragment(v2f_img input):SV_Target
        {float2 p=Pixel(input.uv,_Pixels.xy)+_MaskSize.z;return tex2Dlod(_MainTex,float4(p.x/_MaskSize.x,1-p.y/_MaskSize.y,0,0)).r;}
        float4 paintFragment(v2f_img input):SV_Target
        {
            float2 p=(Pixel(input.uv,_Pixels.xy)-_Size.w*_Size.z)/_Size.z;
            float shape=Shape(p,_Size.xy,_Radius.xy);
            float inner=Shape(p-_Border,_Size.xy-_Border*2,max(0,_Radius.xy-_Border));
            float4 color=Premul(_Fill);
            if(_StopCount>0){
                float2 period=max(.00001,_Size.xy-_Border*2); float2 local=p-_Border; float2 q=local-floor(local/period)*period;
                float2 center=_Origin.xy*period;float extent=length(max(center,period-center));
                float t=_Linear>0?q.y/period.y:length(q-center)/max(.00001,extent);
                float4 gradient=Premul(_Colors[0]);
                [loop]for(int i=1;i<_StopCount;i++){float span=_Stops[i]-_Stops[i-1];if(t>=_Stops[i])gradient=Premul(_Colors[i]);else{if(t>_Stops[i-1])gradient=lerp(Premul(_Colors[i-1]),Premul(_Colors[i]),span>0?(t-_Stops[i-1])/span:1);break;}}
                color=Over(color,gradient);
            }
            float4 shadow=Premul(_ShadowInner);shadow*=tex2D(_Inner,input.uv).r*inner/max(.00001,shape);color=Over(color,shadow);
            float4 border=Premul(_BorderColor);border*=max(0,shape-inner)/max(.00001,shape);color=Over(color,border)*shape;
            float4 a=Premul(_ShadowA)*tex2D(_OuterA,input.uv).r*(1-shape);
            float4 b=Premul(_ShadowB)*tex2D(_OuterB,input.uv).r*(1-shape);
            color=Over(Over(b,a),color);return float4(color.a>0?color.rgb/color.a:0,color.a);
        }
        ENDHLSL
        Pass {HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert_img
            #pragma fragment maskFragment
        ENDHLSL}
        Pass {HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert_img
            #pragma fragment blurFragment
        ENDHLSL}
        Pass {HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert_img
            #pragma fragment paintFragment
        ENDHLSL}
        Pass {HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert_img
            #pragma fragment cropFragment
        ENDHLSL}
    }
}
