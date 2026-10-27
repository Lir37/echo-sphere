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
    private GameObject _screenOwner;private PanelSettings _screenSettings;private VisualElement _root;private Texture2D[] _patterns;
    private Image _background;private VisualElement _target,_extra,_unrelated,_group;private Label _label;
    private Color32[] _bluePatch,_greenPatch;private int _patchError;private double _meanPatchError;private bool _blueNative,_greenNative;
    private object _registry;private Type _type;private IDisposable _binding,_secondary;
    private int _phase,_frames,_capturedFrame=-1,_preparedAtRed;private Color32 _red,_blue,_referenceBlue,_green,_referenceGreen;
    private string _state="running",_error;private bool _ready,_disposed;private float _sigma=2f,_density=1f;private bool _screen,_explicit,_translucent;
    private float FrameWidth=>_explicit?256:180;
    private float FrameHeight=>_explicit?192:120;
    private static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    public static object StartProbe(float sigma=2f,bool screenSpace=false,float density=1f,bool explicitLayers=false,bool translucent=false)
    {
        if(!Application.isPlaying)throw new InvalidOperationException("UI capture probe requires Play Mode");
        if(Current!=null)throw new InvalidOperationException("UI capture probe already exists");
        Current=new EchoSphereBackdropCacheProbe {_sigma=sigma,_screen=screenSpace,_density=density,_explicit=explicitLayers,_translucent=translucent};try{Current.Initialize();UnityEditor.EditorApplication.update+=Current.LateUpdate;UnityEditor.EditorApplication.playModeStateChanged+=Current.PlayState;UnityEditor.AssemblyReloadEvents.beforeAssemblyReload+=Current.Dispose;return Current.Status();}catch{Current.Dispose();throw;}
    }
    public object Status()=>new {state=_state,error=_error,phase=_phase,frames=_frames,sigma=_sigma,density=_density,explicitLayers=_explicit,translucent=_translucent,layout=_registry==null?0:Count("LayoutPrepareCount"),repaints=_registry==null?0:Count("DynamicRepaintCount"),patchChannelError=_patchError,meanPatchError=_meanPatchError,blueNative=_blueNative,greenNative=_greenNative,capturePrepares=_registry==null?0:Count("CapturePrepareCount"),fallbacks=_registry==null?0:Count("RetainedFallbackCount"),offRegionAnimation=true,red=_red,blue=_blue,referenceBlue=_referenceBlue,green=_green,referenceGreen=_referenceGreen};
    private int Count(string name)=>(int)_type.GetProperty(name,Hidden).GetValue(_registry);
    private void Initialize()
    {
        if(!_screen)_capture=new NativeUiCapturePanel(null,"Backdrop cache validation",31,new Vector2(FrameWidth,FrameHeight),Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme"));
        _input=new RenderTexture(180,120,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Backdrop probe live input"};_input.Create();
        _output=new RenderTexture(Mathf.CeilToInt(FrameWidth*_density),Mathf.CeilToInt(FrameHeight*_density),24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Backdrop probe output"};_output.Create();
        if(_screen){_screenSettings=ScriptableObject.CreateInstance<PanelSettings>();_screenSettings.renderMode=PanelRenderMode.ScreenSpaceOverlay;_screenSettings.scaleMode=PanelScaleMode.ConstantPixelSize;_screenSettings.scale=_density;_screenSettings.forceGammaRendering=true;_screenSettings.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");_screenSettings.targetTexture=_output;_screenSettings.clearColor=true;_screenSettings.colorClearValue=Color.clear;_screenOwner=new GameObject("Bounded screen prefix probe");var document=_screenOwner.AddComponent<UIDocument>();document.panelSettings=_screenSettings;_root=document.rootVisualElement;Box(_root,0,0,FrameWidth,FrameHeight);}else _root=_capture.Root;
        var root=_root;root.style.backgroundColor=new Color(.03f,.02f,.01f,_translucent?.5f:1);var group=_group=new VisualElement();Box(group,0,0,180,120);group.style.opacity=.75f;group.style.paddingLeft=group.style.paddingTop=4;group.style.borderLeftWidth=group.style.borderTopWidth=2;group.style.borderLeftColor=group.style.borderTopColor=Color.clear;group.style.translate=new Translate(Length.Percent(5),Length.Percent(7));root.Add(group);_background=new Image {image=_input,scaleMode=ScaleMode.StretchToFill};Box(_background,0,0,180,120);group.Add(_background);
        _patterns=new Texture2D[3];for(int channel=0;channel<3;channel++){var texture=_patterns[channel]=new Texture2D(180,120,TextureFormat.RGBA32,false);var colors=new Color32[180*120];for(int y=0;y<120;y++)for(int x=0;x<180;x++){byte value=(byte)(120+(x*7+y*11)%136);colors[y*180+x]=new Color32(channel==0?value:(byte)0,channel==1?value:(byte)0,channel==2?value:(byte)0,255);}texture.SetPixels32(colors);texture.Apply();}
        _unrelated=new VisualElement();Box(_unrelated,-1000,-1000,4,4);_unrelated.style.backgroundColor=Color.white;root.Add(_unrelated);
        _label=new Label("live prefix");Box(_label,25,75,120,15);_label.style.fontSize=10;group.Add(_label);var distant=new Label("out of region text");Box(distant,0,-500,180,30);distant.style.fontSize=18;group.Add(distant);
        _target=new VisualElement();Box(_target,_explicit?60:20,_explicit?60:20,140,80);root.Add(_target);
        _type=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeUiBackdropRegistry",true);
        Func<VisualElement,Action<MeshGenerationContext>> painter=e=>e is Image||e is TextElement?null:e.generateVisualContent;
        _registry=Activator.CreateInstance(_type,Hidden,null,new object[]{painter,null,true,true,_explicit,null},null);
        _binding=(IDisposable)_type.GetMethod("RegisterNativeBackdrop").Invoke(_registry,new object[]{_target,_sigma});
        Application.onBeforeRender+=Capture;_ready=true;
    }
    private static void Box(VisualElement e,float x,float y,float width,float height){e.style.position=Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=width;e.style.height=height;}
    private void LateUpdate()
    {
        if(!_ready||_state!="running")return;
        try{
            _type.GetMethod("BeginFrame",Hidden).Invoke(_registry,null);
            _type.GetField("DisableExplicitLayersForValidation",Hidden).SetValue(_registry,_phase==2||_phase==4);
            _type.GetField("DisableLayoutCacheForValidation",Hidden).SetValue(_registry,_phase==2||_phase==4);
            typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.SourceUiBackdrop",true).GetField("DisableKernelCacheForValidation",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,_phase==2||_phase==4);
            typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.SourceUiReplica",true).GetField("DisableBoundsForValidation",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,_phase==2||_phase==4);
            var previous=RenderTexture.active;try{Graphics.Blit(_patterns[_phase==0?0:_phase<3?2:1],_input);}finally{RenderTexture.active=previous;}
            _background.MarkDirtyRepaint();
            _unrelated.style.opacity=.5f+Mathf.Sin(Time.unscaledTime*3)*.1f;
            if(_extra!=null)_type.GetMethod("Prepare",Hidden).Invoke(_registry,new object[]{_extra,_root,_density});
            _type.GetMethod("Prepare",Hidden).Invoke(_registry,new object[]{_target,_root,_density});
        }catch(Exception e){_state="failed";_error=e.ToString();}
    }
    private Color32 Pixel()
    {
        var previous=RenderTexture.active;var image=new Texture2D(1,1,TextureFormat.RGBA32,false);
        try{RenderTexture.active=_output;image.ReadPixels(new Rect(Mathf.Floor(_target.layout.center.x*_density),Mathf.Floor((FrameHeight-_target.layout.center.y)*_density),1,1),0,0);image.Apply();return image.GetPixels32()[0];}finally{RenderTexture.active=previous;Destroy(image);}
    }
    private static int Difference(Color32 a,Color32 b)=>Math.Max(Math.Max(Math.Abs(a.r-b.r),Math.Abs(a.g-b.g)),Math.Max(Math.Abs(a.b-b.b),Math.Abs(a.a-b.a)));
    private Color32[] Patch()
    {
        var previous=RenderTexture.active;var image=new Texture2D(Mathf.CeilToInt(140*_density),Mathf.CeilToInt(80*_density),TextureFormat.RGBA32,false);
        try{RenderTexture.active=_output;image.ReadPixels(new Rect(_target.layout.x*_density,(FrameHeight-_target.layout.yMax)*_density,Mathf.CeilToInt(140*_density),Mathf.CeilToInt(80*_density)),0,0);image.Apply();return image.GetPixels32();}finally{RenderTexture.active=previous;Destroy(image);}
    }
    private bool UsesCapture()=>(bool)_type.GetMethod("UsesCapture",Hidden).Invoke(_registry,new object[]{_target});
    private void ComparePatch(Color32[] expected,bool nativeRaster)
    {
        var actual=Patch();long sum=0;for(int i=0;i<actual.Length;i++){var a=expected[i];var b=actual[i];_patchError=Math.Max(_patchError,Difference(a,b));sum+=Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)+Math.Abs(a.a-b.a);}
        _meanPatchError=Math.Max(_meanPatchError,sum/(actual.Length*4d));if(_patchError>1)throw new InvalidOperationException("Filtered panel pixels differ from retained reference");
    }
    private void Capture()
    {
        if(!_ready||_state!="running"||_capturedFrame==Time.frameCount)return;_capturedFrame=Time.frameCount;
        try{
            if(_capture!=null&&!_capture.TryRender(_output)||_root.panel==null||_target.layout.width<=0)return;if(++_frames<8)return;_frames=0;
            var pixel=Pixel();
            // Signal dominance works in both the linear world-space capture
            // and gamma screen panel. Exactness is checked over the full patch.
            if(_phase==0){_red=pixel;_preparedAtRed=Count("LayoutPrepareCount");if(_red.r<_red.b+40)throw new InvalidOperationException("Red live prefix was not rendered");}
            if(_phase==1){_blue=pixel;_bluePatch=Patch();_blueNative=UsesCapture();if(_blue.b<_blue.r+40)throw new InvalidOperationException("Cached prefix froze the live texture");if(Count("LayoutPrepareCount")!=_preparedAtRed)throw new InvalidOperationException("Live pixels/off-region animation rebuilt unchanged prefix styles");}
            if(_phase==2){_referenceBlue=pixel;ComparePatch(_bluePatch,_blueNative);if(Difference(_blue,pixel)>1)throw new InvalidOperationException("Cached live backdrop differs from full preparation");_group.style.width=120;_group.style.height=90;_group.style.overflow=Overflow.Hidden;_group.style.borderBottomRightRadius=16;_background.style.opacity=.5f;_background.uv=new Rect(.1f,.2f,.7f,.6f);_target.style.borderTopLeftRadius=12;_label.text="updated text";}
            if(_phase==3){_green=pixel;_greenPatch=Patch();_greenNative=UsesCapture();}
            if(_phase==4){_referenceGreen=pixel;ComparePatch(_greenPatch,_greenNative);if(Difference(_green,pixel)>1)throw new InvalidOperationException("Backdrop missed an opacity/clip change");_extra=new VisualElement();Box(_extra,0,0,8,8);_root.Insert(1,_extra);_secondary=(IDisposable)_type.GetMethod("RegisterNativeBackdrop").Invoke(_registry,new object[]{_extra,2f});}
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
        typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.SourceUiBackdrop",true).GetField("DisableKernelCacheForValidation",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,false);
        typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.SourceUiReplica",true).GetField("DisableBoundsForValidation",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,false);
        Application.onBeforeRender-=Capture;_secondary?.Dispose();_binding?.Dispose();(_registry as IDisposable)?.Dispose();_capture?.Dispose();
        if(_screenOwner!=null){_screenOwner.SetActive(false);Destroy(_screenOwner);}if(_screenSettings!=null)Destroy(_screenSettings);if(_patterns!=null)foreach(var texture in _patterns)Destroy(texture);
        if(_input!=null){_input.Release();Destroy(_input);}if(_output!=null){_output.Release();Destroy(_output);}if(Current==this)Current=null;
    }
}
