using System;

namespace EchoSphere.Presentation
{
    // Canvas clips the composited shadow, not the shape used to generate it.
    // Keep the blur's read margins outside the final canvas until the last pass.
    public sealed class VisualCanvasShadowViewport
    {
        public readonly int Width,Height,PaddingX,PaddingY;
        public readonly double U,V,UWidth,VHeight;
        public VisualCanvasShadowViewport(int width,int height,int paddingX,int paddingY)
        {
            PaddingX=paddingX;PaddingY=paddingY;Width=checked(width+paddingX*2);Height=checked(height+paddingY*2);
            U=paddingX/(double)Width;V=paddingY/(double)Height;UWidth=width/(double)Width;VHeight=height/(double)Height;
        }
        public static double Sigma(double blur)=>Math.Max(0d,blur)*.5d;
        public static int Padding(double blur,double offset)=>(int)Math.Ceiling(Sigma(blur)*4d+Math.Abs(offset))+1;
    }
}
