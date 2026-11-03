using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Typed immutable menu underlays only. No UI-tree replicas or gameplay rendering.
 internal sealed class NativeStaticMenuGlass:IDisposable {
  internal readonly struct Layer {internal readonly Texture Texture;internal readonly Rect Bounds;internal readonly float Radius;internal readonly int Version;internal Layer(Texture texture,Rect bounds,float radius=0,int version=0){Texture=texture;Bounds=bounds;Radius=radius;Version=version;}}
  private readonly VisualElement _host,_clip;private readonly Image _image;private readonly Material _material;
  private readonly List<Layer> _inputs=new List<Layer>();
  private bool SameInputs(Rect bounds,List<Layer> layers){int index=0;foreach(var layer in layers){if(layer.Texture==null||!layer.Bounds.Overlaps(bounds))continue;if(index>=_inputs.Count)return false;var old=_inputs[index++];if(layer.Texture!=old.Texture||layer.Bounds!=old.Bounds||layer.Radius!=old.Radius||layer.Version!=old.Version)return false;}return index==_inputs.Count;}
  private RenderTexture _output;private Rect _bounds,_viewport;private int _version=-1;private float _scale,_sigma;
  internal Texture Texture=>_output;internal bool Ready=>_output!=null&&_output.IsCreated();internal Rect Bounds=>_bounds;internal int BakeCount{get;private set;}
  internal NativeStaticMenuGlass(VisualElement host){_host=host;_clip=new VisualElement{name="native-static-menu-glass",pickingMode=PickingMode.Ignore};_clip.style.position=Position.Absolute;_clip.style.overflow=Overflow.Hidden;_image=new Image{pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};_image.style.width=_image.style.height=Length.Percent(100);_clip.Add(_image);host.hierarchy.Insert(0,_clip);_material=new Material(Resources.Load<Shader>("EchoSphere/NativeStaticMenuGlass")){hideFlags=HideFlags.DontSave};}
  internal bool Owns(VisualElement node)=>node==_clip||_clip.Contains(node);
  internal void Show(bool visible){_clip.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;}
  internal void Bake(Texture background,Rect viewport,Rect bounds,float scale,float sigma,float radius,int version,List<Layer> layers){
   if(Ready&&_bounds==bounds&&_viewport==viewport&&_scale==scale&&_sigma==sigma&&_version==version&&SameInputs(bounds,layers)){Show(true);return;}
   int width=Mathf.CeilToInt(bounds.width*scale),height=Mathf.CeilToInt(bounds.height*scale);if(width<1||height<1)return;
   var descriptor=new RenderTextureDescriptor(width,height,RenderTextureFormat.ARGB32,0){sRGB=false};
   var a=RenderTexture.GetTemporary(descriptor);var b=RenderTexture.GetTemporary(descriptor);a.filterMode=b.filterMode=FilterMode.Bilinear;a.wrapMode=b.wrapMode=TextureWrapMode.Clamp;var old=RenderTexture.active;bool srgb=GL.sRGBWrite;
   try{GL.sRGBWrite=false;_material.SetVector("_SourceRect",new Vector4((bounds.x-viewport.x)/viewport.width,1-(bounds.yMax-viewport.y)/viewport.height,bounds.width/viewport.width,bounds.height/viewport.height));Graphics.Blit(background,a,_material,0);
    _material.SetVector("_SourceRect",new Vector4(0,0,bounds.width,bounds.height));_material.SetFloat("_Density",scale);
    foreach(var layer in layers){if(layer.Texture==null||!layer.Bounds.Overlaps(bounds))continue;var r=layer.Bounds;_material.SetTexture("_Overlay",layer.Texture);_material.SetVector("_LayerRect",new Vector4((r.x-bounds.x)/bounds.width,1-(r.yMax-bounds.y)/bounds.height,r.width/bounds.width,r.height/bounds.height));_material.SetFloat("_Radius",layer.Radius);Graphics.Blit(a,b,_material,1);var swap=a;a=b;b=swap;}
    float pixels=sigma*scale;_material.SetFloat("_Sigma",pixels);bool cached=NativeGaussianWeights.TryGet(pixels,out var weights);_material.SetInt("_WeightCount",cached?Mathf.CeilToInt(4*pixels)+1:0);if(cached)_material.SetVectorArray("_Weights",weights);Graphics.Blit(a,b,_material,2);
    if(_output==null||_output.width!=width||_output.height!=height){_image.image=null;if(_output!=null){UnityEngine.Object.Destroy(_output);}_output=new RenderTexture(descriptor){name="Retained static menu glass",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};_output.Create();}
    if(!_output.IsCreated())_output.Create();Graphics.Blit(b,_output,_material,3);_image.image=null;_image.image=_output;_image.MarkDirtyRepaint();
   }finally{RenderTexture.active=old;GL.sRGBWrite=srgb;RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
   _inputs.Clear();foreach(var layer in layers)if(layer.Texture!=null&&layer.Bounds.Overlaps(bounds))_inputs.Add(layer);
   _bounds=bounds;_viewport=viewport;_scale=scale;_sigma=sigma;_version=version;BakeCount++;
   var style=_host.resolvedStyle;_clip.style.left=-style.borderLeftWidth;_clip.style.top=-style.borderTopWidth;_clip.style.width=bounds.width;_clip.style.height=bounds.height;_clip.style.borderTopLeftRadius=_clip.style.borderTopRightRadius=_clip.style.borderBottomLeftRadius=_clip.style.borderBottomRightRadius=radius;Show(true);
  }
  public void Dispose(){_inputs.Clear();_image.image=null;_clip.RemoveFromHierarchy();if(_output!=null){UnityEngine.Object.Destroy(_output);}UnityEngine.Object.Destroy(_material);}
 }
}
