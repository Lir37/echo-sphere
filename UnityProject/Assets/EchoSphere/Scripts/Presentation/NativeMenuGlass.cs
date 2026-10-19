using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation
{
    // GPU image prefixes are prepared before UI rendering, avoiding the previous
    // frame readback of a separate UIDocument target. No text/vector is inferred.
    internal sealed class NativeMenuGlass : IDisposable
    {
        private readonly VisualElement _element,_clip;
        private readonly Image _input;
        private readonly Material _prefixMaterial;
        private RenderTexture _prefixTexture,_scratch,_horizontal,_blurTexture;
        private SourceUiBackdrop _retainedPrefix;
        private Rect _area;private bool _disposed;
        internal int PrepareCount {get;private set;}
        internal RenderTexture Texture=>_blurTexture;
        internal bool UsesRetainedPrefix=>_retainedPrefix!=null;
        internal NativeMenuGlass(VisualElement element,float sigma)
        {
            _element=element;
            _prefixMaterial=new Material(Resources.Load<Shader>("EchoSphere/NativeMenuPrefix")){name="Native menu prefix"};
            _clip=new VisualElement {name="native-menu-glass-clip",pickingMode=PickingMode.Ignore,userData=new SourceUiBackdrop.ClipTag{Owner=element}};
            _clip.style.position=Position.Absolute;_clip.style.overflow=Overflow.Hidden;
            _input=new Image {pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};_input.style.position=Position.Absolute;
            _clip.Add(_input);element.hierarchy.Insert(0,_clip);
        }
        private static void Box(VisualElement e,float x,float y,float w,float h)=>SourceUiBackdrop.LocalBox(e,x,y,w,h);
        private static RenderTexture Target(RenderTexture previous,int width,int height,bool linear=false)
        {
            if(previous!=null&&previous.width==width&&previous.height==height)return previous;
            if(previous!=null){previous.Release();UnityEngine.Object.Destroy(previous);}
            var target=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32,linear?RenderTextureReadWrite.Linear:RenderTextureReadWrite.sRGB){name="Native menu image prefix",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            if(!target.Create())throw new InvalidOperationException("Native menu prefix allocation failed");return target;
        }
        internal void Prepare(VisualElement root,float sigma,float scale)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMenuGlass));
            var box=_element.layout;var resolved=_element.resolvedStyle;
            if(!float.IsFinite(box.width+box.height)||box.width<=0||box.height<=0||resolved.display==DisplayStyle.None){Visible(false);return;}
            Visible(true);
            var area=_element.worldBound;float padding=Mathf.Ceil(4*sigma);area.xMin=Mathf.Floor(area.xMin-padding);area.yMin=Mathf.Floor(area.yMin-padding);area.xMax=Mathf.Ceil(area.xMax+padding);area.yMax=Mathf.Ceil(area.yMax+padding);
            int width=Mathf.CeilToInt(area.width*scale),height=Mathf.CeilToInt(area.height*scale);
            if(width>SystemInfo.maxTextureSize||height>SystemInfo.maxTextureSize)throw new InvalidOperationException("Native menu prefix exceeds texture limits");
            area.width=width/scale;area.height=height/scale;_area=area;
            _prefixTexture=Target(_prefixTexture,width,height);_scratch=Target(_scratch,width,height);
            if(!PaintPrefix(root,_element.worldBound)){
                _clip.style.display=DisplayStyle.None;
                if(_retainedPrefix==null)_retainedPrefix=new SourceUiBackdrop(_element,context=>{},element=>null,null);
                _retainedPrefix.Prepare(root,sigma);PrepareCount++;return;
            }
            if(_retainedPrefix!=null){_retainedPrefix.Dispose();_retainedPrefix=null;}
            _horizontal=Target(_horizontal,width,height,true);_blurTexture=Target(_blurTexture,width,height);
            var previous=RenderTexture.active;bool srgb=GL.sRGBWrite;
            try{
                GL.sRGBWrite=false;var panel=_element.worldBound;
                _prefixMaterial.SetVector("_Pixels",new Vector4(width,height,0,0));_prefixMaterial.SetFloat("_Sigma",sigma*scale);
                _prefixMaterial.SetVector("_Panel",new Vector4((panel.x-area.x)*scale,(panel.y-area.y)*scale,panel.width*scale,panel.height*scale));
                Graphics.Blit(_prefixTexture,_horizontal,_prefixMaterial,1);Graphics.Blit(_horizontal,_blurTexture,_prefixMaterial,2);
            }finally{GL.sRGBWrite=srgb;RenderTexture.active=previous;}
            _input.image=_blurTexture;
            Box(_clip,-resolved.borderLeftWidth,-resolved.borderTopWidth,box.width,box.height);
            _clip.style.borderTopLeftRadius=resolved.borderTopLeftRadius;_clip.style.borderTopRightRadius=resolved.borderTopRightRadius;_clip.style.borderBottomLeftRadius=resolved.borderBottomLeftRadius;_clip.style.borderBottomRightRadius=resolved.borderBottomRightRadius;
            Box(_input,area.x-_element.worldBound.x,area.y-_element.worldBound.y,area.width,area.height);PrepareCount++;
        }
        private bool PaintPrefix(VisualElement root,Rect panel)
        {
            var old=RenderTexture.active;bool srgb=GL.sRGBWrite;
            try{
                GL.sRGBWrite=false;RenderTexture.active=_prefixTexture;GL.Clear(false,true,Color.clear);
                foreach(var item in VisualBackdrop.PaintPrefix(root,_element,e=>e.hierarchy.childCount,(e,i)=>e.hierarchy[i],e=>e.resolvedStyle.display==DisplayStyle.None||e.userData is SourceUiBackdrop.ClipTag)){
                    var e=item.Node;var bounds=e.worldBound;var coverage=bounds;
                    if(e is TextElement){var shadow=e.style.textShadow.value;float reach=(shadow.color.a>0?shadow.blurRadius*2:0)+e.resolvedStyle.unityTextOutlineWidth;coverage.xMin-=Mathf.Max(0,reach-shadow.offset.x);coverage.xMax+=Mathf.Max(0,reach+shadow.offset.x);coverage.yMin-=Mathf.Max(0,reach-shadow.offset.y);coverage.yMax+=Mathf.Max(0,reach+shadow.offset.y);}
                    if(!coverage.Overlaps(panel))continue;
                    float opacity=1;for(var parent=e;parent!=null&&parent!=root;parent=parent.parent)opacity*=parent.resolvedStyle.opacity;
                    if(opacity<=0||e.resolvedStyle.visibility==Visibility.Hidden)continue;
                    // Very short authored layouts can put text beneath the nav.
                    // Keep native UI Toolkit's complete retained prefix there;
                    // never omit text or invent a different responsive layout.
                    if(e is TextElement text&&!string.IsNullOrEmpty(text.text))return false;
                    if(e is Image image){
                        if(image.image==null){if(image.sprite!=null||image.vectorImage!=null)return false;continue;}
                        if(image.scaleMode!=ScaleMode.StretchToFill)return false;
                        var matrix=e.worldTransform;if(matrix.m00<=0||matrix.m11<=0||Mathf.Abs(matrix.m01)>1e-5f||Mathf.Abs(matrix.m10)>1e-5f)return false;
                        for(var parent=e;parent!=null;parent=parent.parent){var filters=parent.resolvedStyle.filter;if(filters!=null)foreach(var filter in filters)return false;if(parent==root)break;}
                        Draw(image.image,bounds,image.uv,image.tintColor,opacity);continue;
                    }
                    var style=e.resolvedStyle;var background=style.backgroundImage;
                    if(background.texture!=null||background.sprite!=null||background.vectorImage!=null||background.renderTexture!=null)return false;
                    if(style.borderLeftWidth>0&&style.borderLeftColor.a>0||style.borderRightWidth>0&&style.borderRightColor.a>0||style.borderTopWidth>0&&style.borderTopColor.a>0||style.borderBottomWidth>0&&style.borderBottomColor.a>0)return false;
                    var color=style.backgroundColor;if(color.a>0){if(style.borderTopLeftRadius>0||style.borderTopRightRadius>0||style.borderBottomLeftRadius>0||style.borderBottomRightRadius>0)return false;Draw(Texture2D.whiteTexture,bounds,new Rect(0,0,1,1),color,opacity);}
                }
                return true;
            }finally{GL.sRGBWrite=srgb;RenderTexture.active=old;}
        }
        private void Draw(Texture image,Rect bounds,Rect uv,Color tint,float opacity)
        {
            _prefixMaterial.SetTexture("_LayerTex",image);_prefixMaterial.SetVector("_LayerRect",new Vector4((bounds.x-_area.x)/_area.width,(bounds.y-_area.y)/_area.height,bounds.width/_area.width,bounds.height/_area.height));
            _prefixMaterial.SetVector("_LayerUv",new Vector4(uv.x,uv.y,uv.width,uv.height));_prefixMaterial.SetVector("_Tint",new Vector4(tint.r,tint.g,tint.b,tint.a*opacity));
            Graphics.Blit(_prefixTexture,_scratch,_prefixMaterial,0);var swap=_prefixTexture;_prefixTexture=_scratch;_scratch=swap;
        }
        internal void Visible(bool visible){_clip.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;if(!visible&&_retainedPrefix!=null){_retainedPrefix.Dispose();_retainedPrefix=null;}}
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;_retainedPrefix?.Dispose();_retainedPrefix=null;_clip.RemoveFromHierarchy();UnityEngine.Object.Destroy(_prefixMaterial);
            if(_prefixTexture!=null){_prefixTexture.Release();UnityEngine.Object.Destroy(_prefixTexture);}if(_scratch!=null){_scratch.Release();UnityEngine.Object.Destroy(_scratch);}
            if(_horizontal!=null){_horizontal.Release();UnityEngine.Object.Destroy(_horizontal);}if(_blurTexture!=null){_blurTexture.Release();UnityEngine.Object.Destroy(_blurTexture);}
        }
    }
}
