using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Profiling;

namespace EchoSphere.Presentation
{
    // Explicit native capture/blur layer. Unity UIR paints the prefix through a
    // camera; owned ROI textures bound GPU work independently of UIR filter bounds.
    // The retained prefix bridge is temporary until individual views author their
    // producer layers directly. No render work runs from generateVisualContent.
    internal sealed class NativeCaptureBackdrop : IDisposable
    {
        private static readonly HashSet<int> ReservedLayers=new HashSet<int>();
        private static readonly ProfilerMarker CaptureMarker=new ProfilerMarker("EchoSphere.UI.NativeEffectCapture"),BlurMarker=new ProfilerMarker("EchoSphere.UI.NativeEffectBlur");
        private readonly VisualElement _original,_clip,_placement,_rotation,_foreground;
        private readonly Image _image;
        private readonly Action<MeshGenerationContext> _normalPaint;
        private readonly SourceUiReplica _replica;
        private readonly Transform _owner;
        private NativeUiCapturePanel _capture;
        private VisualElement _prefixPlacement,_prefixRotation;
        private RenderTexture _source,_horizontal,_output;
        private Material _blur;
        private Rect _area;private Matrix4x4 _matrix;
        private float _scale,_sigma;private int _layer=-1;private bool _disposed;
        internal bool Scheduled;
        internal NativeCaptureBackdrop(VisualElement original,Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,Transform owner)
        {
            _original=original;_owner=owner;_normalPaint=original.generateVisualContent;
            _clip=Box("Native effect border clip");_clip.userData=new SourceUiBackdrop.ClipTag {Owner=original};_clip.style.overflow=Overflow.Hidden;
            _placement=Box("Native effect placement");_rotation=Box("Native effect rotation");_image=new Image {name="Native effect image",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill,userData=SourceUiBackdrop.ReplicaTag};
            _clip.Add(_placement);_placement.Add(_rotation);_rotation.Add(_image);
            _foreground=original is TextElement?(VisualElement)new Label():Box("Native effect foreground");_foreground.userData=SourceUiBackdrop.ReplicaTag;_foreground.pickingMode=PickingMode.Ignore;
            var textPaint=_foreground.generateVisualContent;_foreground.generateVisualContent=context=>textPaint?.Invoke(context);
            original.generateVisualContent=null;original.hierarchy.Insert(0,_clip);original.hierarchy.Insert(1,_foreground);
            _replica=new SourceUiReplica(paint,snapshots,true);
        }
        private static VisualElement Box(string name){var element=new VisualElement {name=name,pickingMode=PickingMode.Ignore,userData=SourceUiBackdrop.ReplicaTag};SourceUiBackdrop.LocalBox(element,0,0,0,0);element.style.overflow=Overflow.Visible;return element;}
        private void EnsureCapture(Vector2 size)
        {
            if(_capture!=null){_capture.Resize(size);return;}
            var occupied=new HashSet<int>();foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(renderer.enabled&&renderer.gameObject.activeInHierarchy)occupied.Add(renderer.gameObject.layer);
            for(int layer=31;layer>=8;layer--)if(!ReservedLayers.Contains(layer)&&!occupied.Contains(layer)){_layer=layer;ReservedLayers.Add(layer);break;}
            if(_layer<0)throw new InvalidOperationException("No free layer for native UI effect capture");
            try{
                _capture=new NativeUiCapturePanel(_owner,"Native effect "+_original.name,_layer,size,Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme"));
                _capture.Root.style.overflow=Overflow.Hidden;_prefixPlacement=Box("Native layer prefix placement");_prefixRotation=Box("Native layer prefix rotation");
                _capture.Root.Add(_prefixPlacement);_prefixPlacement.Add(_prefixRotation);
                var shader=Resources.Load<Shader>("EchoSphere/NativeEffectBlur");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native effect blur shader unavailable");
                _blur=new Material(shader){name="Native ROI blur",hideFlags=HideFlags.DontSave};
            }catch{ReservedLayers.Remove(_layer);_layer=-1;_capture?.Dispose();_capture=null;throw;}
        }
        internal void Prepare(VisualElement root,float sigma,float scale)
        {
            var box=_original.layout;var resolved=_original.resolvedStyle;
            if(!float.IsFinite(box.width+box.height)||box.width<=0||box.height<=0){Scheduled=false;return;}
            _scale=scale;_sigma=sigma;
            SourceUiReplica.Appearance(_original,_foreground);SourceUiBackdrop.LocalBox(_foreground,-resolved.borderLeftWidth,-resolved.borderTopWidth,box.width,box.height);
            _foreground.style.translate=new Translate(0,0);_foreground.style.rotate=new Rotate(0);_foreground.style.scale=new Scale(Vector3.one);_foreground.style.opacity=1;_foreground.style.filter=StyleKeyword.Null;_foreground.style.overflow=Overflow.Visible;
            if(_foreground is TextElement text&&_original is TextElement source){text.text=source.text;text.enableRichText=source.enableRichText;}
            SourceUiBackdrop.LocalBox(_clip,-resolved.borderLeftWidth,-resolved.borderTopWidth,box.width,box.height);
            _clip.style.borderTopLeftRadius=resolved.borderTopLeftRadius;_clip.style.borderTopRightRadius=resolved.borderTopRightRadius;_clip.style.borderBottomLeftRadius=resolved.borderBottomLeftRadius;_clip.style.borderBottomRightRadius=resolved.borderBottomRightRadius;
            var frame=_original as SourceUiFrame;_matrix=_original.worldTransform*(frame?.BrushMatrix??Matrix4x4.identity);
            if(!float.IsFinite(_matrix.determinant)||Mathf.Abs(_matrix.determinant)<1e-8f){Scheduled=false;return;}
            if(frame!=null){_clip.style.rotate=frame.Content.style.rotate;_clip.style.transformOrigin=frame.Content.style.transformOrigin;}
            var bounds=SourceUiBackdrop.WorldBounds(_original);float padding=Mathf.Ceil(4*sigma);
            _area=Rect.MinMaxRect(Mathf.Floor((bounds.xMin-padding)*scale)/scale,Mathf.Floor((bounds.yMin-padding)*scale)/scale,Mathf.Ceil((bounds.xMax+padding)*scale)/scale,Mathf.Ceil((bounds.yMax+padding)*scale)/scale);
            int width=Mathf.RoundToInt(_area.width*scale),height=Mathf.RoundToInt(_area.height*scale);EnsureCapture(_area.size);
            if(_source==null||_source.width!=width||_source.height!=height){ReleaseTargets();try{_source=Target("Native effect capture",width,height,24);_horizontal=Target("Native effect horizontal",width,height,0);_output=Target("Native effect output",width,height,0);_image.image=_output;}catch{ReleaseTargets();throw;}}
            SourceUiBackdrop.SetMatrix(_prefixPlacement,_prefixRotation,Matrix4x4.Translate(new Vector3(-_area.x,-_area.y,0))*root.worldTransform);
            SourceUiBackdrop.SetMatrix(_placement,_rotation,_matrix.inverse*Matrix4x4.Translate(new Vector3(_area.x,_area.y,0)));
            SourceUiBackdrop.LocalBox(_image,0,0,_area.width,_area.height);_replica.Update(root,_original,_prefixRotation,_area);RepaintDynamic();Scheduled=true;
        }
        internal void RepaintDynamic(){_foreground.MarkDirtyRepaint();_replica.RepaintDynamic();}
        internal void Render()
        {
            if(!Scheduled||_capture==null)return;
            var previous=RenderTexture.active;bool srgb=GL.sRGBWrite;
            try{
                using(CaptureMarker.Auto()){if(!_capture.TryRender(_source))return;}
                GL.sRGBWrite=false;_blur.SetFloat("_Sigma",_sigma*_scale);_blur.SetFloat("_Density",_scale);_blur.SetVector("_Roi",new Vector4(_area.x,_area.y,_area.width,_area.height));
                _blur.SetVector("_PanelSize",new Vector4(_original.layout.width,_original.layout.height,0,0));var inverse=_matrix.inverse;
                _blur.SetVector("_ToPanelX",new Vector4(inverse.m00,inverse.m01,inverse.m03,0));_blur.SetVector("_ToPanelY",new Vector4(inverse.m10,inverse.m11,inverse.m13,0));
                _blur.SetVector("_ToScreenX",new Vector4(_matrix.m00,_matrix.m01,_matrix.m03,0));_blur.SetVector("_ToScreenY",new Vector4(_matrix.m10,_matrix.m11,_matrix.m13,0));
                bool cached=NativeGaussianWeights.TryGet(_sigma*_scale,out var weights);_blur.SetInt("_WeightCount",cached?Mathf.CeilToInt(4*_sigma*_scale)+1:0);if(cached)_blur.SetVectorArray("_Weights",weights);
                using(BlurMarker.Auto()){Graphics.Blit(_source,_horizontal,_blur,0);Graphics.Blit(_horizontal,_output,_blur,1);}
            }finally{GL.sRGBWrite=srgb;RenderTexture.active=previous;}
        }
        private static RenderTexture Target(string name,int width,int height,int depth)
        {var target=new RenderTexture(width,height,depth,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};if(!target.Create()){UnityEngine.Object.Destroy(target);throw new InvalidOperationException("Native effect target allocation failed");}return target;}
        private void ReleaseTargets(){_image.image=null;Release(ref _source);Release(ref _horizontal);Release(ref _output);}
        private static void Release(ref RenderTexture texture){if(texture==null)return;texture.Release();UnityEngine.Object.Destroy(texture);texture=null;}
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;Scheduled=false;_original.generateVisualContent=_normalPaint;_clip.RemoveFromHierarchy();_foreground.RemoveFromHierarchy();_replica.Dispose();_capture?.Dispose();ReleaseTargets();
            if(_blur!=null)UnityEngine.Object.Destroy(_blur);if(_layer>=0)ReservedLayers.Remove(_layer);
        }
    }
}
