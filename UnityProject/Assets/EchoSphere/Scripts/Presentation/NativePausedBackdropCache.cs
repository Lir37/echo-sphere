using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // A frozen game screen needs one full-density blur, not a live backdrop pass
 // on every pause-menu frame. The overlay itself stays native and interactive.
 internal sealed class NativePausedBackdropCache:IDisposable {
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
  internal static bool DisableForValidation=false;
#endif
  private readonly MonoBehaviour _owner;private readonly VisualElement _overlay;private readonly Image _image;private readonly Action<bool> _setLive;private readonly float _sigma;private readonly Material _blur;
  private RenderTexture _output;private Coroutine _capture;private bool _active,_failed,_ready,_disposed,_liveEnabled=true;private int _generation,_began,_settled,_width,_height,_key;private float _density;
  internal bool Ready=>_ready&&_output!=null&&_output.IsCreated();internal int BakeCount{get;private set;}
  internal NativePausedBackdropCache(MonoBehaviour owner,VisualElement overlay,float sigma,Action<bool> setLive){_owner=owner;_overlay=overlay;_sigma=sigma;_setLive=setLive;_blur=new Material(Resources.Load<Shader>("EchoSphere/NativeStaticMenuGlass")){name="Frozen pause background blur",hideFlags=HideFlags.DontSave};_image=new Image{name="native-frozen-pause-backdrop",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};_image.style.position=Position.Absolute;_image.style.left=_image.style.top=0;_image.style.width=_image.style.height=Length.Percent(100);_image.style.display=DisplayStyle.None;overlay.parent.Insert(overlay.parent.IndexOf(overlay),_image);}
  private void Live(bool value){if(_liveEnabled==value)return;_liveEnabled=value;_setLive(value);}
  private void HideImage(){_image.image=null;_image.style.display=DisplayStyle.None;}
  private void ReleaseOutput(){if(_output!=null)UnityEngine.Object.Destroy(_output);_output=null;_ready=false;}
  internal void Reset(){_generation++;if(_capture!=null&&_owner!=null)_owner.StopCoroutine(_capture);_capture=null;_active=_failed=false;HideImage();ReleaseOutput();Live(true);}
  private void Fallback(){_generation++;if(_capture!=null&&_owner!=null)_owner.StopCoroutine(_capture);_capture=null;_failed=true;HideImage();ReleaseOutput();_overlay.style.opacity=1;Live(true);}
  private static bool TooltipVisible(VisualElement e){if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity<=0)return false;if((e.name??"").IndexOf("tooltip",StringComparison.OrdinalIgnoreCase)>=0)return true;foreach(var c in e.Children())if(TooltipVisible(c))return true;return false;}
  private bool HasTransientPrefix(){foreach(var sibling in _overlay.parent.Children()){if(sibling==_overlay)break;if(sibling!=_image&&TooltipVisible(sibling))return true;}return false;}
  internal void Update(bool active,bool captureAllowed,float density,int key){
   if(_disposed)return;if(!active){if(_active)Reset();return;}
   bool disabled=false;
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
   disabled=DisableForValidation;
#endif
   if(!_active){_active=true;_began=_settled=Time.frameCount;_width=Screen.width;_height=Screen.height;_density=density;_key=key;_failed=!captureAllowed||disabled||QualitySettings.activeColorSpace!=ColorSpace.Gamma||SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null||HasTransientPrefix();}
   if(disabled){Fallback();return;}
   bool changed=_width!=Screen.width||_height!=Screen.height||_density!=density||_key!=key;
   if(changed){if(_ready||_capture!=null){Fallback();return;}_width=Screen.width;_height=Screen.height;_density=density;_key=key;_settled=Time.frameCount;}
   if(_failed){HideImage();_overlay.style.opacity=1;Live(true);return;}
   if(_ready){if(!Ready){Fallback();return;}_image.style.display=DisplayStyle.Flex;_overlay.style.opacity=1;Live(false);return;}
   if(!captureAllowed||Time.frameCount-_began>8){Fallback();return;}
   _overlay.style.opacity=0;Live(false);
   if(_capture==null&&Time.frameCount-_settled>=2)_capture=_owner.StartCoroutine(Capture(_generation));
  }
  private IEnumerator Capture(int generation){
   yield return new WaitForEndOfFrame();if(_disposed||!_active||generation!=_generation)yield break;
   RenderTexture source=null,ping=null;var previous=RenderTexture.active;bool srgb=GL.sRGBWrite;
   try{var descriptor=new RenderTextureDescriptor(_width,_height,RenderTextureFormat.ARGB32,0){sRGB=false};source=new RenderTexture(descriptor){name="Pause prefix capture",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear};source.Create();ScreenCapture.CaptureScreenshotIntoRenderTexture(source);ping=RenderTexture.GetTemporary(descriptor);ping.filterMode=FilterMode.Bilinear;ping.wrapMode=TextureWrapMode.Clamp;_output=new RenderTexture(descriptor){name="Retained frozen pause backdrop",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};_output.Create();GL.sRGBWrite=false;float pixels=_sigma*_density;_blur.SetFloat("_Sigma",pixels);bool cached=NativeGaussianWeights.TryGet(pixels,out var weights);_blur.SetInt("_WeightCount",cached?Mathf.CeilToInt(4*pixels)+1:0);if(cached)_blur.SetVectorArray("_Weights",weights);Graphics.Blit(source,ping,_blur,2);Graphics.Blit(ping,_output,_blur,3);_image.uv=SystemInfo.graphicsUVStartsAtTop?new Rect(0,1,1,-1):new Rect(0,0,1,1);_image.image=_output;_image.MarkDirtyRepaint();_ready=true;BakeCount++;_capture=null;}
   catch(Exception error){_capture=null;Fallback();Debug.LogWarning("[EchoSphere] Pause backdrop retained native fallback: "+error.Message);}
   finally{RenderTexture.active=previous;GL.sRGBWrite=srgb;if(ping!=null)RenderTexture.ReleaseTemporary(ping);if(source!=null)UnityEngine.Object.Destroy(source);}
  }
  public void Dispose(){if(_disposed)return;Reset();_disposed=true;_image.RemoveFromHierarchy();UnityEngine.Object.Destroy(_blur);}
 }
}
