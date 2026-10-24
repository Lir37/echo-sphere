using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    public readonly struct VisualCssColor
    {
        public readonly double R,G,B,A;
        public VisualCssColor(double r,double g,double b,double a=1d){R=r;G=g;B=b;A=a;}
        public string Canvas=>"rgba("+NativePresentationMath.NumberText(R*255d)+","+NativePresentationMath.NumberText(G*255d)+","+NativePresentationMath.NumberText(B*255d)+","+NativePresentationMath.NumberText(A)+")";
        public static VisualCssColor Lerp(VisualCssColor a,VisualCssColor b,double t)
        {var alpha=a.A*(1d-t)+b.A*t;if(alpha<=0d)return new VisualCssColor(0d,0d,0d,0d);return new VisualCssColor((a.R*a.A*(1d-t)+b.R*b.A*t)/alpha,(a.G*a.A*(1d-t)+b.G*b.A*t)/alpha,(a.B*a.A*(1d-t)+b.B*b.A*t)/alpha,alpha);}
        public static VisualCssColor Parse(string text,string current="#000000")
        {
            text=text.Trim();if(text=="currentColor"||text=="currentcolor")return Parse(current);if(text=="transparent")return new VisualCssColor(0,0,0,0);
            if(text=="black")return new VisualCssColor(0,0,0);if(text=="white")return new VisualCssColor(1,1,1);
            if(text.StartsWith("#",StringComparison.Ordinal)) {
                var hex=text.Substring(1);if(hex.Length==3||hex.Length==4)hex=string.Concat(hex.Select(c=>new string(c,2)));if(hex.Length==6)hex+="ff";
                if(hex.Length!=8)throw new FormatException("Invalid authored CSS color "+text);
                return new VisualCssColor(int.Parse(hex.Substring(0,2),NumberStyles.HexNumber)/255d,int.Parse(hex.Substring(2,2),NumberStyles.HexNumber)/255d,int.Parse(hex.Substring(4,2),NumberStyles.HexNumber)/255d,int.Parse(hex.Substring(6,2),NumberStyles.HexNumber)/255d);
            }
            if(text.StartsWith("color-mix(",StringComparison.Ordinal)) {
                var args=VisualCss.Split(text.Substring(10,text.Length-11),',');
                (VisualCssColor color,double? weight) Part(string value){var match=Regex.Match(value.Trim(),@"^(.*\S)\s+([\d.]+)%$");return match.Success?(Parse(match.Groups[1].Value,current),double.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture)/100d):(Parse(value.Trim(),current),(double?)null);}
                var a=Part(args[1]);var b=Part(args[2]);var aw=a.weight??(b.weight.HasValue?1d-b.weight.Value:.5d);var bw=b.weight??(a.weight.HasValue?1d-a.weight.Value:.5d);var total=aw+bw;
                var mixed=Lerp(a.color,b.color,total>0d?bw/total:0d);return new VisualCssColor(mixed.R,mixed.G,mixed.B,mixed.A*Math.Min(1d,total));
            }
            var function=text.Substring(0,Math.Max(0,text.IndexOf('(')));var body=text.Substring(text.IndexOf('(')+1).TrimEnd(')');
            if(function=="color"&&body.StartsWith("srgb ",StringComparison.Ordinal))body=body.Substring(5);
            var parts=Regex.Split(body.Replace(","," ").Replace("/"," "),@"\s+").Where(value=>value.Length>0).ToArray();
            double Component(int index,double divisor)=>parts[index].EndsWith("%",StringComparison.Ordinal)?double.Parse(parts[index].TrimEnd('%'),CultureInfo.InvariantCulture)/100d:double.Parse(parts[index],CultureInfo.InvariantCulture)/divisor;
            var alpha=parts.Length>3?Component(3,1d):1d;
            if(function=="rgb"||function=="rgba"||function=="color")return new VisualCssColor(Component(0,function=="color"?1d:255d),Component(1,function=="color"?1d:255d),Component(2,function=="color"?1d:255d),alpha);
            if(function=="hsl"||function=="hsla") {
                var h=double.Parse(parts[0].Replace("deg",""),CultureInfo.InvariantCulture)/360d;var saturation=Component(1,1d);var light=Component(2,1d);
                double Hue(double t){t-=Math.Floor(t);var q=light<.5d?light*(1d+saturation):light+saturation-light*saturation;var p=2d*light-q;return t<1d/6d?p+(q-p)*6d*t:t<.5d?q:t<2d/3d?p+(q-p)*(2d/3d-t)*6d:p;}
                return saturation==0d?new VisualCssColor(light,light,light,alpha):new VisualCssColor(Hue(h+1d/3d),Hue(h),Hue(h-1d/3d),alpha);
            }
            throw new FormatException("Unimplemented authored CSS color "+text);
        }
    }
}
