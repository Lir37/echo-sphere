using System;
using System.Reflection;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

// Isolated native UIR capture: exercises live texture updates through a cached
// filtered replica, rather than comparing managed tree descriptions.
public sealed class EchoSphereBackdropCacheProbe : IDisposable
{
    public static EchoSphereBackdropCacheProbe Current;
    private NativeUiCapturePanel _capture;private RenderTexture _input,_output;
    private Image _background;private VisualElement _target,_extra,_unrelated;private Label _label;
    private Color32[] _bluePatch,_greenPatch;private int _patchError;
    private object _registry;private Type _type;private IDisposable _binding,_secondary;
    private int _phase,_frames,_capturedFrame=-1,_preparedAtRed;private Color32 _red,_blue,_referenceBlue,_green,_referenceGreen;
    private string _state="running",_error;private bool _ready,_disposed;
    private static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object StartProbe()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("UI capture probe requires Play Mode");
        if(Current!=null)throw new InvalidOperationException("UI capture probe already exists");
        Current=new EchoSphereBackdropCacheProbe();try{Current.Initialize();UnityEditor.EditorApplication.update+=Current.LateUpdate;UnityEditor.EditorApplication.playModeStateChanged+=Current.PlayState;UnityEditor.AssemblyReloadEvents.beforeAssemblyReload+=Current.Dispose;return Current.Status();}catch{Current.Dispose();throw;}
    }
    public object Status()=>new {state=_state,error=_error,phase=_phase,frames=_frames,layout=_registry==null?0:Count("LayoutPrepareCount"),repaints=_registry==null?0:Count("DynamicRepaintCount"),patchChannelError=_patchError,offRegionAnimation=true,red=_red,blue=_blue,referenceBlue=_referenceBlue,green=_green,referenceGreen=_referenceGreen};
    private int Count(string name)=>(int)_type.GetProperty(name,Hidden).GetValue(_registry);
    private void Initialize()
    {
        _capture=new NativeUiCapturePanel(null,"Backdrop cache validation",31,new Vector2(180,120),Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme"));
        _input=new RenderTexture(180,120,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Backdrop probe live input"};_input.Create();
        _output=new RenderTexture(180,120,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Backdrop probe output"};_output.Create();
        var root=_capture.Root;_background=new Image {image=_input,scaleMode=ScaleMode.StretchToFill};Box(_background,0,0,180,120);root.Add(_background);
        _unrelated=new VisualElement();Box(_unrelated,0,0,4,4);_unrelated.style.backgroundColor=Color.white;root.Add(_unrelated);
        _label=new Label("live prefix");Box(_label,25,75,120,15);_label.style.fontSize=10;root.Add(_label);
        _target=new VisualElement();Box(_target,20,20,140,80);root.Add(_target);
        _type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeUiBackdropRegistry",true);
        Func<VisualElement,Action<MeshGenerationContext>> painter=e=>e is Image||e is TextElement?null:e.generateVisualContent;
        _registry=Activator.CreateInstance(_type,Hidden,null,new object[]{painter,null,true},null);
        _binding=(IDisposable)_type.GetMethod("RegisterNativeBackdrop").Invoke(_registry,new object[]{_target,2f});
        Application.onBeforeRender+=Capture;_ready=true;
    }
    private static void Box(VisualElement e,float x,float y,float width,float height){e.style.position=Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=width;e.style.height=height;}
    private void LateUpdate()
    {
        if(!_ready||_state!="running")return;
        try{
            _type.GetField("DisableLayoutCacheForValidation",Hidden).SetValue(_registry,_phase==2||_phase==4);
            var previous=RenderTexture.active;try{RenderTexture.active=_input;GL.Clear(false,true,_phase==0?Color.red:_phase<3?Color.blue:Color.green);}finally{RenderTexture.active=previous;}
            _background.MarkDirtyRepaint();
            _unrelated.style.opacity=.5f+Mathf.Sin(Time.unscaledTime*3)*.1f;
            if(_extra!=null)_type.GetMethod("Prepare",Hidden).Invoke(_registry,new object[]{_extra,_capture.Root,1f});
            _type.GetMethod("Prepare",Hidden).Invoke(_registry,new object[]{_target,_capture.Root,1f});
        }catch(Exception e){_state="failed";_error=e.ToString();}
    }
    private Color32 Pixel()
    {
        var previous=RenderTexture.active;var image=new Texture2D(1,1,TextureFormat.RGBA32,false);
        try{RenderTexture.active=_output;image.ReadPixels(new Rect(90,60,1,1),0,0);image.Apply();return image.GetPixels32()[0];}finally{RenderTexture.active=previous;Destroy(image);}
    }
    private static int Difference(Color32 a,Color32 b)=>Math.Max(Math.Max(Math.Abs(a.r-b.r),Math.Abs(a.g-b.g)),Math.Max(Math.Abs(a.b-b.b),Math.Abs(a.a-b.a)));
    private Color32[] Patch()
    {
        var previous=RenderTexture.active;var image=new Texture2D(140,80,TextureFormat.RGBA32,false);
        try{RenderTexture.active=_output;image.ReadPixels(new Rect(20,20,140,80),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;Destroy(image);}
    }
    private void ComparePatch(Color32[] expected){var actual=Patch();for(int i=0;i<actual.Length;i++)_patchError=Math.Max(_patchError,Difference(expected[i],actual[i]));if(_patchError>1)throw new InvalidOperationException("Cached filtered panel pixels differ from full preparation");}
    private void Capture()
    {
        if(!_ready||_state!="running"||_capturedFrame==Time.frameCount)return;_capturedFrame=Time.frameCount;
        try{
            if(!_capture.TryRender(_output))return;if(++_frames<8)return;_frames=0;
            var pixel=Pixel();
            if(_phase==0){_red=pixel;_preparedAtRed=Count("LayoutPrepareCount");if(_red.r<_red.b+100)throw new InvalidOperationException("Red live prefix was not rendered");}
            if(_phase==1){_blue=pixel;_bluePatch=Patch();if(_blue.b<_blue.r+100)throw new InvalidOperationException("Cached prefix froze the live texture");if(Count("LayoutPrepareCount")!=_preparedAtRed)throw new InvalidOperationException("Live pixels/off-region animation rebuilt unchanged prefix styles");}
            if(_phase==2){_referenceBlue=pixel;ComparePatch(_bluePatch);if(Difference(_blue,pixel)>1)throw new InvalidOperationException("Cached live backdrop differs from full preparation");_background.style.opacity=.5f;_target.style.borderTopLeftRadius=12;_label.text="updated text";}
            if(_phase==3){_green=pixel;_greenPatch=Patch();}
            if(_phase==4){_referenceGreen=pixel;ComparePatch(_greenPatch);if(Difference(_green,pixel)>1)throw new InvalidOperationException("Backdrop missed an opacity/clip change");_extra=new VisualElement();Box(_extra,0,0,8,8);_capture.Root.Insert(1,_extra);_secondary=(IDisposable)_type.GetMethod("RegisterNativeBackdrop").Invoke(_registry,new object[]{_extra,2f});}
            if(_phase==5){_secondary.Dispose();_secondary=null;_extra.RemoveFromHierarchy();_extra=null;}
            if(_phase==6){if(Difference(_referenceGreen,pixel)>1)throw new InvalidOperationException("Backdrop retained a removed filter owner");_state="passed";}
            _phase++;
        }catch(Exception e){_state="failed";_error=e.ToString();}
    }
    private void PlayState(UnityEditor.PlayModeStateChange state){if(state==UnityEditor.PlayModeStateChange.ExitingPlayMode)Dispose();}
    private static void Destroy(UnityEngine.Object value){if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
    public void Dispose()
    {
        if(_disposed)return;_disposed=true;
        _ready=false;UnityEditor.EditorApplication.update-=LateUpdate;UnityEditor.EditorApplication.playModeStateChanged-=PlayState;UnityEditor.AssemblyReloadEvents.beforeAssemblyReload-=Dispose;
        Application.onBeforeRender-=Capture;_secondary?.Dispose();_binding?.Dispose();(_registry as IDisposable)?.Dispose();_capture?.Dispose();
        if(_input!=null){_input.Release();Destroy(_input);}if(_output!=null){_output.Release();Destroy(_output);}if(Current==this)Current=null;
    }
}
