using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    // Source SVG serialization, shared by the runtime and portable checks.
    public static class VisualSvg
    {
        private static string Escape(string text)=>text.Replace("&","&amp;").Replace("\"","&quot;").Replace("<","&lt;").Replace(">","&gt;");
        private static readonly Regex RgbFunction=new Regex(@"(?i)^rgba?\(\s*([^)]*)\s*\)$",RegexOptions.Compiled);
        private static string NormalizeSvgColor(string value,out float alpha)
        {
            value=value.Trim();
            // Source icon currentColor can resolve to color-mix(), and drag
            // overlays use #RRGGBBAA. Normalize these before the SVG 1.1 parser.
            if(value.StartsWith("color-mix(",StringComparison.Ordinal)||value.StartsWith("color(",StringComparison.Ordinal)||value=="transparent"||value.StartsWith("#",StringComparison.Ordinal)&&(value.Length==5||value.Length==9)){
                var color=VisualCssColor.Parse(value);alpha=(float)Math.Max(0,Math.Min(1,color.A));
                int ByteChannel(double component)=>(int)Math.Max(0,Math.Min(255,Math.Round(component*255)));
                return string.Format(CultureInfo.InvariantCulture,"rgb({0},{1},{2})",ByteChannel(color.R),ByteChannel(color.G),ByteChannel(color.B));
            }
            alpha=1f;var match=RgbFunction.Match(value.Trim());if(!match.Success)return value;
            // Unity.VectorGraphics accepts legacy comma-separated rgb(), not modern CSS Color 4 syntax.
            var parts=Regex.Split(match.Groups[1].Value.Trim(),@"[,\s/]+").Where(part=>part.Length>0).ToArray();
            if(parts.Length<3||parts.Length>4)return value;
            float Channel(string part)
            {
                var percent=part.EndsWith("%",StringComparison.Ordinal);var numberText=percent?part.Substring(0,part.Length-1):part;
                if(!float.TryParse(numberText,NumberStyles.Float,CultureInfo.InvariantCulture,out var number))return float.NaN;
                return (float)Math.Max(0,Math.Min(255,Math.Round(percent?number*2.55f:number)));
            }
            var red=Channel(parts[0]);var green=Channel(parts[1]);var blue=Channel(parts[2]);
            if(float.IsNaN(red)||float.IsNaN(green)||float.IsNaN(blue))return value;
            if(parts.Length==4)
            {
                var alphaText=parts[3];var percent=alphaText.EndsWith("%",StringComparison.Ordinal);if(percent)alphaText=alphaText.Substring(0,alphaText.Length-1);
                if(!float.TryParse(alphaText,NumberStyles.Float,CultureInfo.InvariantCulture,out alpha))return value;
                if(percent)alpha/=100f;alpha=Math.Max(0f,Math.Min(1f,alpha));
            }
            return string.Format(CultureInfo.InvariantCulture,"rgb({0},{1},{2})",(int)red,(int)green,(int)blue);
        }
        private static readonly HashSet<string> ExactNames=new HashSet<string>{"viewBox","preserveAspectRatio","gradientUnits","gradientTransform","spreadMethod","markerWidth","markerHeight","refX","refY","textLength","lengthAdjust"};
        public static string Serialize(VisualCssNode node,VisualCss sheet)
        {
            var result=new StringBuilder();
            void Append(VisualCssNode item) {
                if(item.Tag=="text")return; // Native text overlays preserve SVG baselines.
                if(item.Tag=="#text"){result.Append(Escape(item.Text));return;}
                result.Append('<').Append(item.Tag);
                var attributes=((Dictionary<string,V>)item.Properties.Raw).Where(pair=>pair.Key!="children"&&pair.Key!="key"&&pair.Key!="ref"&&pair.Key!="className"&&pair.Key!="style"&&!pair.Key.StartsWith("on",StringComparison.Ordinal)&&!pair.Value.Undefined&&!pair.Value.Null).Select(pair=>new KeyValuePair<string,V>(ExactNames.Contains(pair.Key)?pair.Key:Regex.Replace(pair.Key,"[A-Z]",match=>"-"+match.Value.ToLowerInvariant()),pair.Value)).ToArray();
                var fillAlpha=1f;var strokeAlpha=1f;
                foreach(var pair in attributes) {
                    var value=pair.Value.Text;if(value=="currentColor")value=sheet.Get(item,"color","#000000");
                    if(pair.Key=="fill")NormalizeSvgColor(value,out fillAlpha);
                    else if(pair.Key=="stroke")NormalizeSvgColor(value,out strokeAlpha);
                }
                var hasFillOpacity=attributes.Any(pair=>pair.Key=="fill-opacity");var hasStrokeOpacity=attributes.Any(pair=>pair.Key=="stroke-opacity");
                foreach(var pair in attributes) {
                    var name=ExactNames.Contains(pair.Key)?pair.Key:Regex.Replace(pair.Key,"[A-Z]",match=>"-"+match.Value.ToLowerInvariant());
                    var value=pair.Value.Text;if(value=="currentColor")value=sheet.Get(item,"color","#000000");
                    if(pair.Key=="fill"||pair.Key=="stroke")value=NormalizeSvgColor(value,out _);
                    if(pair.Key=="fill-opacity"&&fillAlpha<1f&&float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var existingFillOpacity))value=(existingFillOpacity*fillAlpha).ToString("0.######",CultureInfo.InvariantCulture);
                    if(pair.Key=="stroke-opacity"&&strokeAlpha<1f&&float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var existingStrokeOpacity))value=(existingStrokeOpacity*strokeAlpha).ToString("0.######",CultureInfo.InvariantCulture);
                    result.Append(' ').Append(name).Append("=\"").Append(Escape(value)).Append('"');
                }
                if(fillAlpha<1f&&!hasFillOpacity)result.Append(" fill-opacity=\"").Append(fillAlpha.ToString("0.######",CultureInfo.InvariantCulture)).Append('"');
                if(strokeAlpha<1f&&!hasStrokeOpacity)result.Append(" stroke-opacity=\"").Append(strokeAlpha.ToString("0.######",CultureInfo.InvariantCulture)).Append('"');
                if(item==node){if(item.Properties["xmlns"].Undefined)result.Append(" xmlns=\"http://www.w3.org/2000/svg\"");if(item.Properties["width"].Undefined)result.Append(" width=\"").Append(item.Width.ToString(CultureInfo.InvariantCulture)).Append('"');if(item.Properties["height"].Undefined)result.Append(" height=\"").Append(item.Height.ToString(CultureInfo.InvariantCulture)).Append('"');if(item.Properties["viewBox"].Undefined)result.Append(" viewBox=\"0 0 ").Append(item.Width.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(item.Height.ToString(CultureInfo.InvariantCulture)).Append('"');}
                result.Append('>');foreach(var child in item.Children)Append(child);result.Append("</").Append(item.Tag).Append('>');
            }
            Append(node);return result.ToString();
        }
    }
}
