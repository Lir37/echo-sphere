using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal static class SourceCanvasTypography
    {
        private static readonly Dictionary<string,Font> Fonts=new Dictionary<string,Font>();
        private static readonly TextGenerator Generator=new TextGenerator();
        internal static float Size(string declaration)
        {var match=Regex.Match(declaration,@"([\d.]+)px\s+");return match.Success?Mathf.Max(.01f,float.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture)):10f;}
        internal static Font Resolve(string declaration,out int size,out FontStyle style,float rasterScale=1f)
        {
            var match=Regex.Match(declaration,@"([\d.]+)px\s+(.+)$");
            size=Mathf.Max(1,Mathf.CeilToInt(Size(declaration)*rasterScale));
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
#if UNITY_ANDROID && !UNITY_EDITOR
                    names.Add("Roboto");names.Add("Noto Sans");
#elif UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_EDITOR_OSX
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
            return Generator.GetPreferredWidth(Normalize(text),Settings(font,size,style));
        }
        // Submit the complete run to Unity's native text layout. Per-character
        // advance summation loses the layout engine's spacing and fallback.
        internal static TextGenerationSettings Settings(Font font,int size,FontStyle style)=>new TextGenerationSettings {
            font=font,fontSize=size,fontStyle=style,color=Color.white,scaleFactor=1f,lineSpacing=1f,
            richText=false,textAnchor=TextAnchor.UpperLeft,pivot=Vector2.zero,
            horizontalOverflow=HorizontalWrapMode.Overflow,verticalOverflow=VerticalWrapMode.Overflow,
            generationExtents=Vector2.zero,alignByGeometry=false,resizeTextForBestFit=false,updateBounds=true
        };
        private static string Normalize(string text)=>Regex.Replace(text,@"[\t\n\f\r]"," ");
        internal static UIVertex[] Vertices(Font font,string text,int size,FontStyle style)
        {Generator.Invalidate();Generator.Populate(Normalize(text),Settings(font,size,style));var vertices=new UIVertex[System.Math.Max(0,Generator.verts.Count-4)];for(var index=0;index<vertices.Length;index++)vertices[index]=Generator.verts[index];return vertices;}
    }
}
