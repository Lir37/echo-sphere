#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Editor-only composition experiment, never selected by the game runtime.
    // Tests the prerequisite for a replica-free menu: an earlier native panel
    // renders once into a shared texture; a later panel filters that texture.
    // This is not an accepted replacement for the complete menu or its input.
    public sealed class NativeBackdropPanelProbe : MonoBehaviour
    {
        private const int Size=256;
        private static readonly Rect PanelRect=new Rect(48,64,160,120);
        private static readonly Rect BlurArea=new Rect(16,32,224,184); // Complete four-sigma support, as in SourceUiBackdrop.
        private readonly List<GameObject> _objects=new List<GameObject>();
        private readonly List<PanelSettings> _settings=new List<PanelSettings>();
        private readonly List<RenderTexture> _targets=new List<RenderTexture>();
        private readonly Color32[] _pixels=new Color32[Size*Size];
        private Texture2D _pattern,_referenceRead,_candidateRead;
        private RenderTexture _referenceTarget,_backgroundTarget,_candidateTarget;
        private VisualElement _referenceRoot,_referenceGlass;
        private SourceUiBackdrop _reference;
        private Material _material;
        private FilterFunctionDefinition _definition;
        private int _phase;
        public bool Animate {get;set;}=true;
        public int Phase=>_phase;
        public int ReferenceCopies {get;private set;}
        public int CandidateReplicaCount=>0;
        public int SampleCount {get;private set;}
        public double MaximumMeanError {get;private set;}
        public int MaximumChannelError {get;private set;}
        public int DifferentPixels {get;private set;}
        public long TargetBytes=>3L*Size*Size*4;
        public bool ClickVerified {get;private set;}
        private bool _ready;
        private bool _externalPrefix;

        public void Initialize(int backgroundOrder=2101,bool externalPrefix=false)
        {
            if(_ready)return;
            _externalPrefix=externalPrefix;
            _pattern=new Texture2D(Size,Size,TextureFormat.RGBA32,false,false){name="Native backdrop probe animated pattern",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            _referenceRead=new Texture2D(Size,Size,TextureFormat.RGBA32,false,false);
            _candidateRead=new Texture2D(Size,Size,TextureFormat.RGBA32,false,false);
            SetPhase(0);
            _referenceTarget=Target("Reference prefix");
            _backgroundTarget=Target("Native shared prefix");
            _candidateTarget=Target("Native composed output");
            _referenceRoot=Panel("Reference",2100,_referenceTarget);
            Pattern(_referenceRoot);
            _referenceGlass=Glass();_referenceRoot.Add(_referenceGlass);
            _reference=new SourceUiBackdrop(_referenceGlass,context=>{},source=>null,null);

            var background=Panel("Native background",backgroundOrder,_backgroundTarget);
            Pattern(background);
            var candidate=Panel("Native composite",2102,_candidateTarget);
            var baseImage=TextureImage(_backgroundTarget);candidate.Add(baseImage);
            var clip=new VisualElement();Box(clip,PanelRect.x,PanelRect.y,PanelRect.width,PanelRect.height);
            clip.style.overflow=Overflow.Hidden;Radius(clip,12);candidate.Add(clip);
            var crop=new VisualElement();Box(crop,BlurArea.x-PanelRect.x,BlurArea.y-PanelRect.y,BlurArea.width,BlurArea.height);
            crop.style.overflow=Overflow.Hidden;clip.Add(crop);
            var blurred=TextureImage(_backgroundTarget);blurred.style.left=-BlurArea.x;blurred.style.top=-BlurArea.y;
            crop.Add(blurred);NativeBlur(crop);
            candidate.Add(Glass());

            // A native control in the candidate panel verifies event ownership.
            // It is outside the render target and is not a menu mockup.
            var button=new Button(()=>ClickVerified=true){name="native-probe-button"};
            Box(button,Size+20,0,8,8);candidate.Add(button);
            using(var evt=NavigationSubmitEvent.GetPooled()){evt.target=button;button.SendEvent(evt);}

            var display=Panel("Probe comparison",2103,null);
            var left=TextureImage(_referenceTarget);Box(left,8,8,Size,Size);display.Add(left);
            var right=TextureImage(_candidateTarget);Box(right,Size+16,8,Size,Size);display.Add(right);
            _ready=true;
        }
        private RenderTexture Target(string label)
        {
            var target=new RenderTexture(Size,Size,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native backdrop probe "+label,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            target.Create();_targets.Add(target);return target;
        }
        private VisualElement Panel(string label,int order,RenderTexture target)
        {
            var settings=ScriptableObject.CreateInstance<PanelSettings>();settings.name="Native backdrop probe "+label;
            settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.sortingOrder=order;settings.forceGammaRendering=true;
            settings.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            settings.targetTexture=target;settings.clearColor=true;settings.colorClearValue=Color.clear;_settings.Add(settings);
            var owner=new GameObject(settings.name);owner.transform.SetParent(transform,false);_objects.Add(owner);
            var doc=owner.AddComponent<UIDocument>();doc.panelSettings=settings;
            var root=doc.rootVisualElement;Box(root,0,0,Size,Size);root.pickingMode=PickingMode.Ignore;
            return root;
        }
        private void Pattern(VisualElement root)
        {
            var image=new VisualElement();Box(image,0,0,Size,Size);image.style.backgroundImage=new StyleBackground(_pattern);root.Add(image);
            var label=new Label("ECHO / native prefix");Box(label,60,80,140,28);label.style.fontSize=12;label.style.color=Color.white;root.Add(label);
        }
        private static VisualElement Glass()
        {
            var element=new VisualElement();Box(element,PanelRect.x,PanelRect.y,PanelRect.width,PanelRect.height);
            Radius(element,12);element.style.backgroundColor=new Color(.03f,.07f,.12f,.35f);
            element.style.borderLeftWidth=element.style.borderRightWidth=element.style.borderTopWidth=element.style.borderBottomWidth=1;
            element.style.borderLeftColor=element.style.borderRightColor=element.style.borderTopColor=element.style.borderBottomColor=new Color(.3f,.7f,1,.5f);
            return element;
        }
        private static Image TextureImage(Texture texture)
        {var image=new Image {image=texture,scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};Box(image,0,0,Size,Size);return image;}
        private static void Box(VisualElement element,float x,float y,float width,float height)
        {element.style.position=Position.Absolute;element.style.left=x;element.style.top=y;element.style.width=width;element.style.height=height;element.style.flexShrink=0;}
        private static void Radius(VisualElement element,float radius)
        {element.style.borderTopLeftRadius=element.style.borderTopRightRadius=element.style.borderBottomLeftRadius=element.style.borderBottomRightRadius=radius;}
        private void NativeBlur(VisualElement image)
        {
            var shader=_externalPrefix?UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>("Assets/Editor/EchoSphereNativeBackdropExternalProbe.shader"):Resources.Load<Shader>("EchoSphere/SourceUiBackdrop");
            _material=new Material(shader);
            _definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_definition.name=_definition.filterName="native-panel-prefix-probe";
            _definition.parameters=new[]{new FilterParameterDeclaration {name="sigma",interpolationDefaultValue=new FilterParameter(8f)}};
            var passes=new PostProcessingPass[2];
            for(int axis=0;axis<2;axis++)passes[axis]=new PostProcessingPass {material=_material,passIndex=axis,applySettingsCallback=(properties,context)=>{
                properties.SetFloat("_Sigma",context.filterFunction.GetParameter(0).floatValue);properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);
                if(_externalPrefix){properties.SetTexture("_PrefixTex",_backgroundTarget);properties.SetVector("_PrefixSize",new Vector4(Size,Size,0,0));}
                properties.SetVector("_Roi",new Vector4(BlurArea.x,BlurArea.y,BlurArea.width,BlurArea.height));properties.SetVector("_PanelSize",new Vector4(PanelRect.width,PanelRect.height,0,0));
                properties.SetVector("_ToPanelX",new Vector4(1,0,-PanelRect.x,0));properties.SetVector("_ToPanelY",new Vector4(0,1,-PanelRect.y,0));
                properties.SetVector("_ToScreenX",new Vector4(1,0,PanelRect.x,0));properties.SetVector("_ToScreenY",new Vector4(0,1,PanelRect.y,0));
            }};
            _definition.passes=passes;var filter=new FilterFunction(_definition);filter.AddParameter(new FilterParameter(8f));image.style.filter=new List<FilterFunction>{filter};
        }
        public void SetPhase(int phase)
        {
            _phase=phase;
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){
                var odd=((x/16+y/16+phase)&1)!=0;
                _pixels[y*Size+x]=odd?new Color32(22,68,118,255):new Color32(111,32,71,255);
            }
            _pattern.SetPixels32(_pixels);_pattern.Apply(false,false);
        }
        private void Update()
        {
            if(!_ready)return;
            if(Time.frameCount%4==0&&Animate)SetPhase(_phase+1);
        }
        private void LateUpdate()
        {
            if(!_ready)return;
            _reference.Prepare(_referenceRoot,8);
            // Only diagnostic reflection; the candidate does not access replicas.
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var replica=typeof(SourceUiBackdrop).GetField("_replica",flags).GetValue(_reference);
            ReferenceCopies=((System.Collections.IDictionary)replica.GetType().GetField("_copies",flags).GetValue(replica)).Count;
        }
        public string Compare()
        {
            var previous=RenderTexture.active;
            try{
                RenderTexture.active=_referenceTarget;_referenceRead.ReadPixels(new Rect(0,0,Size,Size),0,0,false);_referenceRead.Apply(false,false);
                RenderTexture.active=_candidateTarget;_candidateRead.ReadPixels(new Rect(0,0,Size,Size),0,0,false);_candidateRead.Apply(false,false);
            }finally{RenderTexture.active=previous;}
            var reference=_referenceRead.GetPixels32();var candidate=_candidateRead.GetPixels32();
            long total=0;int maximum=0,different=0;
            // The event-test control is outside both render targets.
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){
                var i=y*Size+x;var a=reference[i];var b=candidate[i];
                int r=Math.Abs(a.r-b.r),g=Math.Abs(a.g-b.g),blue=Math.Abs(a.b-b.b),alpha=Math.Abs(a.a-b.a);
                int error=Math.Max(Math.Max(r,g),Math.Max(blue,alpha));if(error!=0)different++;maximum=Math.Max(maximum,error);total+=r+g+blue+alpha;
            }
            var mean=total/((double)Size*Size*4);SampleCount++;MaximumMeanError=Math.Max(MaximumMeanError,mean);MaximumChannelError=Math.Max(MaximumChannelError,maximum);DifferentPixels=different;
            return "phase="+_phase+", mean="+mean+", max="+maximum+", different="+different+", reference copies="+ReferenceCopies+", candidate copies=0";
        }
        private void OnDestroy()
        {
            _reference?.Dispose();
            foreach(var owner in _objects)if(owner!=null){var document=owner.GetComponent<UIDocument>();if(document!=null)document.enabled=false;Destroy(owner);}
            foreach(var settings in _settings)if(settings!=null){settings.targetTexture=null;Destroy(settings);}
            foreach(var target in _targets)if(target!=null)Destroy(target);
            if(_pattern!=null)Destroy(_pattern);if(_referenceRead!=null)Destroy(_referenceRead);if(_candidateRead!=null)Destroy(_candidateRead);
            if(_definition!=null)Destroy(_definition);if(_material!=null)Destroy(_material);
        }
    }
}
#endif
