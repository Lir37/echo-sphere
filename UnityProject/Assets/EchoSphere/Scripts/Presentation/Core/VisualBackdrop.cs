using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    // CSS policy and affine decomposition shared by the native adapter and the
    // portable checks. No screen capture, frame history or Unity native calls.
    public static class VisualBackdrop
    {
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        public static double Sigma(VisualCss sheet,VisualCssNode node)
        {
            var value=sheet.Get(node,"backdrop-filter",sheet.Get(node,"-webkit-backdrop-filter","none")).Trim();
            if(value=="none"||value.Length==0)return -1d;
            var match=Regex.Match(value,@"^blur\(([^()]*)\)$");
            if(!match.Success)throw new InvalidOperationException("Unsupported authored backdrop filter: "+value);
            return Math.Max(0d,sheet.Length(match.Groups[1].Value.Length==0?"0":match.Groups[1].Value,node.Width));
        }
#endif
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        public static bool IsRoot(VisualCss sheet,VisualCssNode node)
        {
            if(node.Parent==null)return true;
            if(sheet.Length(sheet.Get(node,"opacity","1"),1d)<1d)return true;
            foreach(var property in new[]{"filter","mask","mask-image","mask-border","clip-path","backdrop-filter","-webkit-backdrop-filter"})
                if(sheet.Get(node,property,"none").Trim()!="none")return true;
            if(sheet.Get(node,"mix-blend-mode","normal")!="normal")return true;
            return sheet.Get(node,"will-change","").Split(',').Any(value=>new[]{"opacity","filter","mask","mask-image","mask-border","clip-path","backdrop-filter","mix-blend-mode"}.Contains(value.Trim()));
        }
#endif
        // [a c; b d] = Rotate(first) * Scale(x,y) * Rotate(second).
        // Two retained elements represent shear/reflection as well as rotation,
        // avoiding a lossy TRS decomposition of nested CSS transforms.
        public static (double First,double X,double Y,double Second) Decompose(double a,double b,double c,double d)
        {
            var sum=Math.Atan2(b-c,a+d);var difference=Math.Atan2(b+c,a-d);
            var plus=Math.Sqrt((a+d)*(a+d)+(b-c)*(b-c));var minus=Math.Sqrt((a-d)*(a-d)+(b+c)*(b+c));
            return ((sum+difference)*.5d,(plus+minus)*.5d,(plus-minus)*.5d,(sum-difference)*.5d);
        }
        public static IEnumerable<(T Node,T Parent)> PaintPrefix<T>(T root,T stop,Func<T,int> count,Func<T,int,T> child,Func<T,bool> skip) where T:class
        {
            var pending=new Stack<(T Node,T Parent)>();pending.Push((root,null));
            while(pending.Count>0){var item=pending.Pop();if(ReferenceEquals(item.Node,stop))yield break;if(skip(item.Node))continue;yield return item;
                for(var index=count(item.Node)-1;index>=0;index--)pending.Push((child(item.Node,index),item.Node));}
        }
    }
}
