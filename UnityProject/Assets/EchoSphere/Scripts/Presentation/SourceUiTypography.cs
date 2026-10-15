using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Resolve the source CSS stack on the current OS. Fonts stay system fonts,
    // just as in the browser; no replacement font artwork is bundled.
    internal static class SourceUiTypography
    {
        private static readonly Dictionary<string,FontAsset> Fonts=new Dictionary<string,FontAsset>();
        private static readonly Dictionary<string,string> Faces=new Dictionary<string,string>();
        private static bool _initialized;
        internal static FontAsset Resolve(VisualCss sheet,VisualCssNode node,float size)
        {
            var family=sheet.Get(node,"font-family","system-ui,sans-serif");
            var weight=sheet.Get(node,"font-weight","400");
            var italic=sheet.Get(node,"font-style","normal")!="normal";
            var height=sheet.Get(node,"line-height","normal");
            var ratio=height=="normal"?double.NaN:double.TryParse(height,NumberStyles.Float,CultureInfo.InvariantCulture,out var number)?number:sheet.Length(height,size,size)/size;
            var key=family+"/"+weight+"/"+italic+"/"+ratio.ToString("R",CultureInfo.InvariantCulture);
            if(Fonts.TryGetValue(key,out var existing))return existing;
            if(!_initialized){FontEngine.InitializeFontEngine();_initialized=true;}
            var names=SourceCanvasTypography.Families(family);
            // CSS falls back to the platform's generic sans-serif faces when an
            // explicitly requested family (for example Roboto on Windows) is absent.
            foreach(var fallback in SourceCanvasTypography.Families("system-ui,sans-serif"))
                if(!names.Contains(fallback))names.Add(fallback);
            // The last families are the browser's platform glyph fallbacks.
#if UNITY_ANDROID && !UNITY_EDITOR
            names.Add("Noto Sans");names.Add("Noto Color Emoji");names.Add("Noto Sans Symbols");
#elif UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_EDITOR_OSX
            names.Add("Apple Color Emoji");names.Add("Apple Symbols");
#else
            names.Add("Segoe UI Emoji");names.Add("Segoe UI Symbol");
#endif
            FontAsset primary=null;var fallbacks=new List<FontAsset>();var seen=new HashSet<string>();
            foreach(var name in names){
                if(!seen.Add(name))continue;var style=Face(name,weight,italic);if(style==null)continue;
                var assetKey=(primary==null?"stack:"+key+"/":"fallback:")+name+"/"+style+"/"+ratio.ToString("R",CultureInfo.InvariantCulture);
                if(!Fonts.TryGetValue(assetKey,out var asset)){
                    asset=FontAsset.CreateFontAsset(name,style,90);if(asset==null)continue;
                    asset.name="Source CSS "+assetKey;asset.getFontFeatures=true;
                    SetLineHeight(asset,ratio);
                    if(primary==null)foreach(var inlineWeight in new[]{100,200,300,400,500,600,700,800,900}){
                        var inlineStyle=Face(name,inlineWeight.ToString(CultureInfo.InvariantCulture),italic);if(inlineStyle==null)continue;
                        var faceKey="inline:"+name+"/"+inlineStyle+"/"+ratio.ToString("R",CultureInfo.InvariantCulture);
                        if(!Fonts.TryGetValue(faceKey,out var typeface)){typeface=FontAsset.CreateFontAsset(name,inlineStyle,90);if(typeface==null)continue;typeface.getFontFeatures=true;SetLineHeight(typeface,ratio);Fonts[faceKey]=typeface;}
                        var pair=asset.fontWeightTable[inlineWeight/100];pair.regularTypeface=typeface;asset.fontWeightTable[inlineWeight/100]=pair;
                    }
                    Fonts[assetKey]=asset;
                }
                if(primary==null)primary=asset;else if(asset!=primary)fallbacks.Add(asset);
            }
            // Do not convert a legacy dynamic Font to FontAsset here. That path
            // may capture into a RenderTexture while UI is being drawn and can
            // trigger D3D12's "GrabIntoRenderTexture ... during a render pass" error.
            // If no system face is available, leave UI Toolkit's default font intact.
            if(primary==null)return null;
            primary.fallbackFontAssetTable=fallbacks;Fonts[key]=primary;return primary;
        }
        private static void SetLineHeight(FontAsset asset,double ratio)
        {
            if(ratio<0d||double.IsNaN(ratio))return;var face=asset.faceInfo;var target=(float)(face.pointSize*ratio);var leading=target-(face.ascentLine-face.descentLine);face.ascentLine+=leading*.5f;face.descentLine-=leading*.5f;face.lineHeight=target;asset.faceInfo=face;
        }
        private static string Face(string family,string weight,bool italic)
        {
            var key=family+"/"+weight+"/"+italic;if(Faces.TryGetValue(key,out var cached))return cached;
            var value=weight=="bold"?700:weight=="normal"?400:int.TryParse(weight,out var parsed)?parsed:400;
            // CSS font matching first chooses a face within this family, before
            // moving on to the next family. Aliases cover OS face naming.
            var candidates=value>=850?new[]{"Black","Heavy","ExtraBold","Extra Bold","Bold","Semibold","Regular","Normal"}:
                value>=750?new[]{"ExtraBold","Extra Bold","Black","Bold","Semibold","Regular","Normal"}:
                value>=650?new[]{"Bold","Semibold","SemiBold","Demi Bold","ExtraBold","Regular","Normal"}:
                value>=550?new[]{"Semibold","SemiBold","Demi Bold","Bold","Medium","Regular","Normal"}:
                value>=450?new[]{"Medium","Regular","Normal","Semibold","Bold"}:
                value<=150?new[]{"Thin","Hairline","ExtraLight","Light","Regular","Normal"}:
                value<=250?new[]{"ExtraLight","Extra Light","Light","Thin","Regular","Normal"}:
                value<=350?new[]{"Light","ExtraLight","Regular","Normal","Medium"}:new[]{"Regular","Normal","Book","Medium","Light","Bold"};
            foreach(var candidate in candidates){
                var style=italic?(candidate=="Regular"||candidate=="Normal"?"Italic":candidate+" Italic"):candidate;
                if((int)FontEngine.LoadFontFace(family,style,90)==0){Faces[key]=style;return style;}
            }
            Faces[key]=null;return null;
        }
        internal static void Apply(VisualCss sheet,VisualCssNode node,VisualElement element,float size)
        {
            var asset=Resolve(sheet,node,size);
            if(asset==null)return;
            element.style.unityFontDefinition=FontDefinition.FromSDFFont(asset);
            element.style.unityFontStyleAndWeight=FontStyle.Normal;
        }
    }
}
