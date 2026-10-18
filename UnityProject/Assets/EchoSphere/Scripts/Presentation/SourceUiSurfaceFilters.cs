using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // A flattened CSS 3D subtree already owns a texture. Filter that texture
    // directly: separable Gaussian passes keep the authored sigma and kernel
    // while avoiding a quadratic kernel per fragment for the large menu core.
    internal sealed class SourceUiSurfaceFilters : IDisposable
    {
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiSurfaceFilters"));
        private readonly List<RenderTexture> _targets=new List<RenderTexture>();
        private int _cursor;
        internal Rect Bounds;
        // Typed native entry point for authored menu artwork. Uses the same
        // proven GPU passes, with no CSS sheet, nodes or runtime filter parser.
        internal Texture Blur(Texture source,Rect bounds,float sigma,float scale)
        {
            _cursor=0;Bounds=bounds;var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{
                GL.sRGBWrite=false;_material.SetVector("_SourceRect",new Vector4(0,0,1,1));_material.SetFloat("_StraightInput",1);
                var input=Target(source.width,source.height);Graphics.Blit(source,input,_material,0);_material.SetFloat("_StraightInput",0);
                var padding=Mathf.Ceil(4*sigma*scale)/scale;Bounds=Rect.MinMaxRect(bounds.xMin-padding,bounds.yMin-padding,bounds.xMax+padding,bounds.yMax+padding);
                var width=Mathf.CeilToInt(Bounds.width*scale);var height=Mathf.CeilToInt(Bounds.height*scale);var padded=Target(width,height);
                _material.SetVector("_SourceRect",new Vector4((bounds.xMin-Bounds.xMin)/Bounds.width,(Bounds.yMax-bounds.yMax)/Bounds.height,bounds.width/Bounds.width,bounds.height/Bounds.height));Graphics.Blit(input,padded,_material,0);_material.SetVector("_SourceRect",new Vector4(0,0,1,1));
                var horizontal=Target(width,height);var vertical=Target(width,height);_material.SetFloat("_Sigma",sigma*scale);
                _material.SetVector("_Direction",new Vector4(1f/width,0,0,0));Graphics.Blit(padded,horizontal,_material,1);
                _material.SetVector("_Direction",new Vector4(0,1f/height,0,0));Graphics.Blit(horizontal,vertical,_material,1);
                var result=Target(width,height,true);GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(vertical,result,_material,5);return result;
            }finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private RenderTexture Target(int width,int height,bool srgb=false)
        {
            var index=_cursor++;if(index==_targets.Count)_targets.Add(null);var target=_targets[index];
            if(target==null||target.width!=width||target.height!=height||target.sRGB!=srgb){if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(Math.Max(1,width),Math.Max(1,height),0,RenderTextureFormat.ARGB32,srgb?RenderTextureReadWrite.sRGB:RenderTextureReadWrite.Linear){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="Source CSS surface filter"};target.Create();_targets[index]=target;}return target;
        }
        internal Texture Render(Texture source,Rect bounds,string filters,VisualCss sheet,VisualCssNode node,float scale)
        {
            _cursor=0;Bounds=bounds;var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{
                GL.sRGBWrite=false;_material.SetVector("_SourceRect",new Vector4(0,0,1,1));_material.SetFloat("_StraightInput",1f);
                var current=Target(source.width,source.height);Graphics.Blit(source,current,_material,0);_material.SetFloat("_StraightInput",0f);
                foreach(Match filter in Regex.Matches(filters,@"([a-z-]+)\(([^()]*(?:\([^()]*\)[^()]*)*)\)")){
                    var name=filter.Groups[1].Value;var argument=filter.Groups[2].Value.Trim();
                    if(name=="blur"||name=="drop-shadow"){
                        var words=VisualCss.Split(argument,' ').Where(word=>word.Length>0).ToArray();var lengths=words.Where(word=>Regex.IsMatch(word,@"^[-+\d.]")).Select(word=>(float)sheet.Length(word,node.Width)).ToArray();
                        var sigma=name=="blur"?(float)sheet.Length(argument,node.Width):lengths.Length>2?lengths[2]:0f;var offset=name=="blur"?Vector2.zero:new Vector2(lengths.Length>0?lengths[0]:0f,lengths.Length>1?lengths[1]:0f);
                        var padding=Mathf.Ceil(4f*sigma*scale)/scale;var old=Bounds;Bounds=Rect.MinMaxRect(old.xMin+Mathf.Min(0,offset.x)-padding,old.yMin+Mathf.Min(0,offset.y)-padding,old.xMax+Mathf.Max(0,offset.x)+padding,old.yMax+Mathf.Max(0,offset.y)+padding);
                        var width=Mathf.CeilToInt(Bounds.width*scale);var height=Mathf.CeilToInt(Bounds.height*scale);var padded=Target(width,height);
                        _material.SetVector("_SourceRect",new Vector4((old.xMin-Bounds.xMin)/Bounds.width,(Bounds.yMax-old.yMax)/Bounds.height,old.width/Bounds.width,old.height/Bounds.height));Graphics.Blit(current,padded,_material,0);_material.SetVector("_SourceRect",new Vector4(0,0,1,1));
                        var horizontal=Target(width,height);var vertical=Target(width,height);_material.SetFloat("_Sigma",sigma*scale);_material.SetVector("_Direction",new Vector4(1f/width,0,0,0));Graphics.Blit(padded,horizontal,_material,name=="blur"?1:2);_material.SetVector("_Direction",new Vector4(0,1f/height,0,0));Graphics.Blit(horizontal,vertical,_material,name=="blur"?1:2);
                        if(name=="blur"){current=vertical;continue;}
                        var css=VisualCssColor.Parse(words.FirstOrDefault(word=>word.StartsWith("#",StringComparison.Ordinal)||word.Contains("(")||word=="currentColor")??sheet.Get(node,"color","#000000"),sheet.Get(node,"color","#000000"));
                        _material.SetVector("_ShadowColor",new Vector4((float)css.R,(float)css.G,(float)css.B,(float)css.A));_material.SetVector("_Offset",new Vector4(offset.x/Bounds.width,-offset.y/Bounds.height,0,0));_material.SetTexture("_Original",padded);current=Target(width,height);Graphics.Blit(vertical,current,_material,3);
                    }else{
                        var amount=argument.EndsWith("%",StringComparison.Ordinal)?float.Parse(argument.TrimEnd('%'),CultureInfo.InvariantCulture)*.01f:float.Parse(argument,CultureInfo.InvariantCulture);var kind=name=="saturate"?0:name=="brightness"?1:name=="contrast"?2:name=="opacity"?3:-1;if(kind<0)throw new InvalidOperationException("Unimplemented authored 3D subtree filter "+name);
                        _material.SetFloat("_Amount",amount);_material.SetFloat("_Kind",kind);var next=Target(current.width,current.height);Graphics.Blit(current,next,_material,4);current=next;
                    }
                }
                var result=Target(current.width,current.height,true);GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(current,result,_material,5);return result;
            }finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        public void Dispose(){foreach(var target in _targets)if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}UnityEngine.Object.Destroy(_material);}
    }
}
