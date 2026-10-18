Shader "EchoSphere/NativeMainMenuBackground"
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
            float4 _LogicalSize;
            float4 _BackingSize;
            float Radius(float2 size,float2 center)
            { return length(max(center,size-center)); }
            float2 RepeatPixel(float2 pixel,float2 period)
            {
                float2 phase=pixel/period;
                float2 grid=pixel-floor(phase)*period;
                grid.x=pixel.x==round(phase.x)*period.x?0:grid.x;
                grid.y=pixel.y==round(phase.y)*period.y?0:grid.y;
                return grid;
            }
            float4 frag(v2f_img input):SV_Target
            {
                // Final .es-main-menu-legacy background and ::before, expressed
                // directly in GPU paint. Values are CSS/sRGB, not lighting colors.
                float2 size=_LogicalSize.xy;
                float2 pixel=floor(float2(input.uv.x,1-input.uv.y)*_BackingSize.xy)+.5;
                float2 p=pixel/_BackingSize.z;
                // CSS background-repeat also covers the fractional last brush
                // texel, even when its center lies exactly at the next tile.
                float2 ownPoint=RepeatPixel(pixel,size*_BackingSize.z)/_BackingSize.z;
                float t=ownPoint.y/size.y;
                float3 color=t<=.55?lerp(float3(2,5,13),float3(4,9,20),t/.55):lerp(float3(4,9,20),float3(2,4,10),(t-.55)/.45);
                color/=255;
                float2 center=size*float2(.5,.42);
                float alpha=.12*saturate(1-length(ownPoint-center)/(.28*Radius(size,center)));
                // Preserve the source brush's fractional border-pixel coverage
                // at DPR < 1 instead of stretching or dropping that texel.
                float coverage=saturate(.5+min(min(p.x,size.x-p.x),min(p.y,size.y-p.y))*_BackingSize.z);
                float ownAlpha=coverage+alpha*coverage*(1-coverage);
                color=(color*coverage*(1-alpha*coverage)+float3(41,108,170)/255*alpha*coverage)/max(ownAlpha,1e-8);
                // Two repeating one-CSS-pixel gradient lines form one group;
                // group opacity .18 and the radial mask apply after their union.
                // Work in backing pixels so non-integer DPR does not amplify
                // interpolated-UV error at an exact repeating-gradient seam.
                float period=42*_BackingSize.z;
                // A reciprocal division can produce 8.999999 for an exact ninth
                // seam. Test that seam in backing coordinates, without moving
                // any near-seam sample or quantizing the authored grid.
                float2 grid=RepeatPixel(pixel,period.xx);
                float x=grid.x<=_BackingSize.z?.12:0;
                float y=grid.y<=_BackingSize.z?.12:0;
                center=size*float2(.5,.48);
                float mask=saturate(1-length(p-center)/(.76*Radius(size,center)));
                x*=coverage;y*=coverage;
                alpha=(x+y-x*y)*.18*mask;
                float finalAlpha=alpha+ownAlpha*(1-alpha);
                color=(color*ownAlpha*(1-alpha)+float3(78,175,225)/255*alpha)/max(finalAlpha,1e-8);
                return float4(color,finalAlpha);
            }
            ENDHLSL
        }
    }
}
