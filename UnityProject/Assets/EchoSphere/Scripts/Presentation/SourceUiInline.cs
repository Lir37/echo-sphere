using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EchoSphere.Presentation
{
    internal static class SourceUiInline
    {
        internal static bool CanMerge(VisualCss sheet,VisualCssNode node)
        {
            if(node.Children.Count==0||new[]{"input","canvas","svg"}.Contains(node.Tag))return false;
            var display=sheet.Get(node,"display");if(display=="flex"||display=="grid"||display=="inline-flex"||display=="inline-grid")return node.Children.All(child=>child.Tag=="#text");
            bool Inline(VisualCssNode child)=>child.Tag=="#text"||child.Pseudo==null&&child.Tag!="svg"&&sheet.Get(child,"display")=="inline"&&sheet.Get(child,"position","static")=="static"&&!((System.Collections.Generic.Dictionary<string,V>)child.Properties.Raw).Keys.Any(key=>key.StartsWith("on",StringComparison.Ordinal))&&child.Children.All(Inline);
            return node.Children.All(Inline);
        }
        internal static string Text(VisualCss sheet,VisualCssNode node)
        {
            var result=new StringBuilder();
            string Escape(string text)=>"<noparse>"+text+"</noparse>";
            void Append(VisualCssNode child){
                if(child.Tag=="#text"){
                    var text=child.Text;var whitespace=sheet.Get(child,"white-space");if(whitespace!="pre"&&whitespace!="pre-wrap")text=Regex.Replace(text,@"\s+"," ");
                    var transform=sheet.Get(child,"text-transform");if(transform=="uppercase")text=text.ToUpperInvariant();else if(transform=="lowercase")text=text.ToLowerInvariant();
                    result.Append(Escape(text));return;
                }
                var color=VisualCssColor.Parse(sheet.Get(child,"color","#000000"));
                string Hex(double value)=>Math.Max(0,Math.Min(255,(int)Math.Round(value*255d))).ToString("X2",CultureInfo.InvariantCulture);
                result.Append("<color=#").Append(Hex(color.R)).Append(Hex(color.G)).Append(Hex(color.B)).Append(Hex(color.A)).Append(">");
                result.Append("<size=").Append(sheet.Get(child,"font-size","16px").Replace("px","")).Append(">");
                var weight=sheet.Get(child,"font-weight","400");result.Append("<font-weight=").Append(weight).Append(">");
                var italic=sheet.Get(child,"font-style","normal")!="normal";if(italic)result.Append("<i>");
                foreach(var nested in child.Children)Append(nested);
                if(italic)result.Append("</i>");result.Append("</font-weight></size></color>");
            }
            foreach(var child in node.Children)Append(child);return result.ToString();
        }
    }
}
