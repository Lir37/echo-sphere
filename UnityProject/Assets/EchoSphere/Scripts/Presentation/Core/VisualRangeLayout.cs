using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    // Browser-painted native range parts retain their own height and knob size.
    // Only the uniform track spans stretch when the authored flex row resizes.
    public static class VisualRangeLayout
    {
        public sealed class Part {public string Name;public double X,Width,U,Uw;}
        public static List<Part> Build(double width,double fraction)
        {
            var result=new List<Part>();var center=8d+Math.Max(0d,width-16d)*Math.Max(0d,Math.Min(1d,fraction));var thumbLeft=center-8.5d;var thumbRight=center+8.5d;
            void Add(string name,double x,double w,double clipLeft,double clipRight){var left=Math.Max(x,clipLeft);var right=Math.Min(x+w,clipRight);if(right>left&&w>0)result.Add(new Part {Name=name,X=left,Width=right-left,U=(left-x)/w,Uw=(right-left)/w});}
            Add("range-left",0,8,0,Math.Min(width,thumbLeft));Add("range-filled",8,Math.Max(0,width-16),8,thumbLeft);
            Add("range-empty",8,Math.Max(0,width-16),thumbRight,width-8);Add("range-right",width-8,8,Math.Max(0,thumbRight),width);
            Add("range-thumb",thumbLeft,17,0,width);return result;
        }
    }
}
