using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal static class SourceCanvasTypography
    {
        private static readonly Dictionary<string,Font> Fonts=new Dictionary<string,Font>();
        internal static Font Resolve(string declaration,out int size,out FontStyle style)
        {
            var match=Regex.Match(declaration,@"([\d.]+)px\s+(.+)$");
            size=match.Success?Mathf.Max(1,Mathf.RoundToInt(float.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture))):10;
            style=declaration.Contains("bold")||Regex.IsMatch(declaration,@"(?:^|\s)[6-9]00(?:\s|$)")?FontStyle.Bold:FontStyle.Normal;
            if(declaration.Contains("italic"))style=style==FontStyle.Bold?FontStyle.BoldAndItalic:FontStyle.Italic;
            var family=match.Success?match.Groups[2].Value:"sans-serif";
            if(Fonts.TryGetValue(family,out var font))return font;
            var names=Families(family);
            font=Font.CreateDynamicFontFromOSFont(names.ToArray(),size);Fonts.Add(family,font);return font;
        }
        internal static List<string> Families(string family)
        {
            var names=new List<string>();
            foreach(var part in family.Split(',')) {
                var name=part.Trim().Trim('\'', '"');
                if(name=="system-ui"||name=="ui-sans-serif"||name=="sans-serif"||name=="-apple-system"||name=="BlinkMacSystemFont") {
#if UNITY_ANDROID
                    names.Add("Roboto");names.Add("Noto Sans");
#elif UNITY_STANDALONE_OSX || UNITY_IOS
                    names.Add("Helvetica Neue");names.Add("Helvetica");
#else
                    names.Add("Segoe UI");names.Add("Arial");
#endif
                } else if(name=="monospace"||name=="ui-monospace") {names.Add("Consolas");names.Add("Roboto Mono");names.Add("Menlo");}
                else if(name=="serif"||name=="ui-serif") {names.Add("Times New Roman");names.Add("Noto Serif");names.Add("Times");}
                else names.Add(name);
            }
            return names;
        }
        internal static float Width(Font font,string text,int size,FontStyle style)
        {
            font.RequestCharactersInTexture(text,size,style);float width=0;
            foreach(var c in text)if(font.GetCharacterInfo(c,out var glyph,size,style))width+=glyph.advance;
            return width;
        }
    }
}
