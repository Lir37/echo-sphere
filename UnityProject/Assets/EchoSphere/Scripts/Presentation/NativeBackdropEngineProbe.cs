#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation
{
    public sealed class NativeBackdropEngineProbe:MonoBehaviour
    {
        public static NativeBackdropEngineProbe Current;
        public readonly RenderTexture[] Outputs=new RenderTexture[2];
        public readonly VisualElement[] Roots=new VisualElement[2],Targets=new VisualElement[2];
        private readonly PanelSettings[] _panels=new PanelSettings[2];
        private LegacyUiBackdropRegistry _legacy;private NativeUiBackdropRegistry _native;
        private readonly IDisposable[] _registrations=new IDisposable[2];
        private Texture2D _pattern;private float _density;public int Frames {get;private set;}
        private static void Box(VisualElement e,float x,float y,float w,float h){e.style.position=Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=w;e.style.height=h;}
        public static void StartProbe(float density,float sigma)
        {if(Current!=null)throw new InvalidOperationException("Dispose previous probe");Current=new GameObject("Unity native backdrop comparison").AddComponent<NativeBackdropEngineProbe>();Current.Build(density,sigma);}
        private void Build(float density,float sigma)
        {
            _density=density;_pattern=new Texture2D(180,120,TextureFormat.RGBA32,false);var colors=new Color32[180*120];
            for(int y=0;y<120;y++)for(int x=0;x<180;x++)colors[y*180+x]=new Color32((byte)(40+x*7%160),(byte)(30+y*11%160),(byte)(80+(x+y)*3%160),255);_pattern.SetPixels32(colors);_pattern.Apply();
            for(int i=0;i<2;i++){
                var owner=new GameObject(i==0?"Legacy backdrop reference":"Engine backdrop candidate");owner.transform.SetParent(transform,false);
                var panel=_panels[i]=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=density;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
                Outputs[i]=new RenderTexture(Mathf.RoundToInt(256*density),Mathf.RoundToInt(192*density),24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name=owner.name};Outputs[i].Create();panel.targetTexture=Outputs[i];panel.clearColor=true;panel.colorClearValue=Color.clear;
                var doc=owner.AddComponent<UIDocument>();doc.panelSettings=panel;var root=Roots[i]=doc.rootVisualElement;Box(root,0,0,256,192);root.style.backgroundColor=new Color(.03f,.02f,.01f,.5f);
                var group=new VisualElement();Box(group,0,0,180,120);group.style.opacity=.75f;group.style.paddingLeft=group.style.paddingTop=4;group.style.borderLeftWidth=group.style.borderTopWidth=2;group.style.borderLeftColor=group.style.borderTopColor=Color.clear;group.style.translate=new Translate(Length.Percent(5),Length.Percent(7));root.Add(group);
                var image=new Image{image=_pattern,scaleMode=ScaleMode.StretchToFill};Box(image,0,0,180,120);group.Add(image);
                var label=new Label("native backdrop");Box(label,25,75,120,15);label.style.fontSize=10;group.Add(label);
                var target=Targets[i]=new VisualElement{name="blurred-panel"};Box(target,60,60,140,80);target.style.borderTopLeftRadius=target.style.borderTopRightRadius=target.style.borderBottomLeftRadius=target.style.borderBottomRightRadius=12;root.Add(target);
                if(i==0){_legacy=new LegacyUiBackdropRegistry(e=>e is Image||e is TextElement?null:e.generateVisualContent,null,true,true);_registrations[i]=_legacy.RegisterNativeBackdrop(target,sigma);}
                else{_native=new NativeUiBackdropRegistry();_registrations[i]=_native.RegisterNativeBackdrop(target,sigma);}
            }
        }
        private void LateUpdate(){_legacy.BeginFrame();_legacy.Prepare(Targets[0],Roots[0],_density);Frames++;}
        private void OnDestroy(){foreach(var registration in _registrations)registration?.Dispose();_legacy?.Dispose();_native?.Dispose();for(int i=0;i<2;i++){if(_panels[i]!=null){_panels[i].targetTexture=null;Destroy(_panels[i]);}if(Outputs[i]!=null){Outputs[i].Release();Destroy(Outputs[i]);}}Destroy(_pattern);Current=null;}
    }
}
#endif
