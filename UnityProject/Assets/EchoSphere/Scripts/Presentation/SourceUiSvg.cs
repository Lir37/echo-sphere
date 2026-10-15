using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    internal static class SourceUiSvg
    {
        private static string Escape(string text)=>text.Replace("&","&amp;").Replace("\"","&quot;").Replace("<","&lt;").Replace(">","&gt;");
        private static readonly HashSet<string> ExactNames=new HashSet<string>{"viewBox","preserveAspectRatio","gradientUnits","gradientTransform","spreadMethod","markerWidth","markerHeight","refX","refY","textLength","lengthAdjust"};
        internal static string Serialize(VisualCssNode node,VisualCss sheet)
        {
            var result=new StringBuilder();
            void Append(VisualCssNode item) {
                if(item.Tag=="text")return; // Native text overlays preserve SVG baselines.
                if(item.Tag=="#text"){result.Append(Escape(item.Text));return;}
                result.Append('<').Append(item.Tag);
                foreach(var pair in (Dictionary<string,V>)item.Properties.Raw) {
                    if(pair.Key=="children"||pair.Key=="key"||pair.Key=="ref"||pair.Key=="className"||pair.Key=="style"||pair.Key.StartsWith("on",StringComparison.Ordinal)||pair.Value.Undefined||pair.Value.Null)continue;
                    var name=ExactNames.Contains(pair.Key)?pair.Key:Regex.Replace(pair.Key,"[A-Z]",match=>"-"+match.Value.ToLowerInvariant());
                    var value=pair.Value.Text;if(value=="currentColor")value=sheet.Get(item,"color","#000000");result.Append(' ').Append(name).Append("=\"").Append(Escape(value)).Append('"');
                }
                if(item==node){if(item.Properties["xmlns"].Undefined)result.Append(" xmlns=\"http://www.w3.org/2000/svg\"");if(item.Properties["width"].Undefined)result.Append(" width=\"").Append(item.Width.ToString(CultureInfo.InvariantCulture)).Append('"');if(item.Properties["height"].Undefined)result.Append(" height=\"").Append(item.Height.ToString(CultureInfo.InvariantCulture)).Append('"');if(item.Properties["viewBox"].Undefined)result.Append(" viewBox=\"0 0 ").Append(item.Width.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(item.Height.ToString(CultureInfo.InvariantCulture)).Append('"');}
                result.Append('>');foreach(var child in item.Children)Append(child);result.Append("</").Append(item.Tag).Append('>');
            }
            Append(node);return result.ToString();
        }
        internal static Texture Rasterize(string xml,float width,float height,float scale,Material material,bool overflow,out Rect paintRect)
        {
            var scene=SVGParser.ImportSVG(new StringReader(xml));
            var geometry=VectorUtils.TessellateScene(scene.Scene,new VectorUtils.TessellationOptions {StepDistance=1f,MaxCordDeviation=.1f,MaxTanAngleDeviation=.1f,SamplingStepSize=.01f},scene.NodeOpacity);
            var viewport=scene.SceneViewport;var bounds=viewport;
            if(overflow){foreach(var item in geometry)foreach(var vertex in item.Vertices){var point=item.WorldTransform*vertex;bounds=Rect.MinMaxRect(Math.Min(bounds.xMin,point.x),Math.Min(bounds.yMin,point.y),Math.Max(bounds.xMax,point.x),Math.Max(bounds.yMax,point.y));}}
            paintRect=new Rect((bounds.x-viewport.x)*width/viewport.width,(bounds.y-viewport.y)*height/viewport.height,bounds.width*width/viewport.width,bounds.height*height/viewport.height);
            var atlas=VectorUtils.GenerateAtlasAndFillUVs(geometry,128);
            var pixelsWide=Math.Max(1,Mathf.CeilToInt(paintRect.width*scale));var pixelsHigh=Math.Max(1,Mathf.CeilToInt(paintRect.height*scale));
            var raw=RenderTexture.GetTemporary(pixelsWide,pixelsHigh,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear,4);
            var result=new RenderTexture(pixelsWide,pixelsHigh,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Source SVG",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};result.Create();
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;var oldTexture=material.mainTexture;GL.PushMatrix();
            try{
                RenderTexture.active=raw;GL.sRGBWrite=false;GL.Clear(false,true,Color.clear);GL.LoadOrtho();material.mainTexture=atlas?.Texture;material.SetFloat("_HasAtlas",atlas!=null?1f:0f);material.SetPass(0);GL.Begin(GL.TRIANGLES);
                foreach(var item in geometry)foreach(var index in item.Indices){var point=item.WorldTransform*item.Vertices[index];var uv=item.UVs!=null?item.UVs[index]:Vector2.zero;GL.Color(item.Color);GL.TexCoord2(uv.x,uv.y);GL.MultiTexCoord2(2,item.SettingIndex,0);GL.Vertex3((point.x-bounds.x)/bounds.width,1f-(point.y-bounds.y)/bounds.height,0);}
                GL.End();GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(raw,result,material,1);return result;
            }catch{result.Release();UnityEngine.Object.Destroy(result);throw;}
            finally{material.mainTexture=oldTexture;RenderTexture.active=previous;GL.sRGBWrite=gamma;GL.PopMatrix();RenderTexture.ReleaseTemporary(raw);if(atlas?.Texture!=null)UnityEngine.Object.Destroy(atlas.Texture);}
        }
        internal static void Text(VisualCssNode root,VisualCss sheet,VisualElement container,List<Label> labels)
        {
            var texts=new List<VisualCssNode>();void Collect(VisualCssNode node){if(node.Tag=="text")texts.Add(node);else foreach(var child in node.Children)Collect(child);}Collect(root);
            var viewBox=root.Properties["viewBox"].Text;var parts=Regex.Split(viewBox.Trim(),@"[\s,]+");
            float Number(string value,float fallback=0f)=>float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var result)?result:fallback;
            var vx=parts.Length==4?Number(parts[0]):0f;var vy=parts.Length==4?Number(parts[1]):0f;var vw=parts.Length==4?Number(parts[2]):(float)root.Width;var vh=parts.Length==4?Number(parts[3]):(float)root.Height;
            var scale=Math.Min((float)root.Width/Math.Max(.00001f,vw),(float)root.Height/Math.Max(.00001f,vh));var ox=((float)root.Width-vw*scale)*.5f;var oy=((float)root.Height-vh*scale)*.5f;
            for(var index=0;index<texts.Count;index++){
                if(index>=labels.Count){var next=new Label {pickingMode=PickingMode.Ignore,enableRichText=false};container.Add(next);labels.Add(next);}
                var node=texts[index];var label=labels[index];label.style.display=DisplayStyle.Flex;label.style.position=Position.Absolute;label.style.marginLeft=label.style.marginTop=label.style.marginRight=label.style.marginBottom=0;label.style.paddingLeft=label.style.paddingTop=label.style.paddingRight=label.style.paddingBottom=0;
                string Content(VisualCssNode item)=>item.Tag=="#text"?item.Text:string.Concat(item.Children.Select(Content));label.text=Content(node);
                var size=Number(node.Properties["fontSize"].Text,(float)sheet.Length(sheet.Get(node,"font-size","16px"),16))*scale;
                var typography=new VisualCssNode {Tag="span"};foreach(var pair in node.Style)typography.Style[pair.Key]=pair.Value;typography.Style["line-height"]="normal";if(!node.Properties["fontWeight"].Undefined)typography.Style["font-weight"]=node.Properties["fontWeight"].Text;
                label.style.fontSize=size;SourceUiTypography.Apply(sheet,typography,label,size);var font=SourceUiTypography.Resolve(sheet,typography,size);var face=font.faceInfo;
                var fill=VisualCssColor.Parse(node.Properties["fill"].Undefined?sheet.Get(node,"fill","#000000"):node.Properties["fill"].Text,sheet.Get(node,"color","#000000"));label.style.color=new Color((float)fill.R,(float)fill.G,(float)fill.B,(float)fill.A);label.style.whiteSpace=WhiteSpace.NoWrap;label.style.unityTextAlign=TextAnchor.UpperLeft;
                var measured=label.MeasureTextSize(label.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);var anchor=node.Properties["textAnchor"].Text;
                label.style.left=ox+(Number(node.Properties["x"].Text)-vx)*scale-(anchor=="middle"?measured.x*.5f:anchor=="end"?measured.x:0f);label.style.top=oy+(Number(node.Properties["y"].Text)-vy)*scale-size*face.ascentLine/face.pointSize;label.style.width=measured.x+1f;label.style.height=Math.Max(measured.y,size*(face.ascentLine-face.descentLine)/face.pointSize);
            }
            for(var index=texts.Count;index<labels.Count;index++)labels[index].style.display=DisplayStyle.None;
        }
    }
}
