using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained run hierarchy. The simulation and existing native presenters own
    // state/actions; this document owns geometry, paint order and blur prefixes.
    public sealed class NativeRunDocument : MonoBehaviour, INativeUiBackdrops
    {
        private UIDocument _document; private PanelSettings _panel;
        private readonly SourceUiReplicaSnapshotCache _snapshots = new SourceUiReplicaSnapshotCache();
        private NativeUiBackdropRegistry _backdrops;
        private readonly List<NativeHudSurface> _paint = new List<NativeHudSurface>();
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private Image _field;
        private VisualElement _highlight;
        private VisualElement _root,_left,_right,_pause,_shell,_detail,_result,_upgrade,_rewards,_tutorial,_combo,_comboBar;
        private Label _comboTitle,_comboMultiplier;
        private int _version; private bool _visible;
        internal NativeRunUiAnchors Anchors {get;} = new NativeRunUiAnchors();
        internal VisualElement ControlsParent => _root;
        internal VisualElement ControlsPredecessor {get;private set;}
        internal VisualElement Field => _field;
        private static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
        private static VisualElement Box(VisualElement p,string name,bool fill=false)
        {var e=new VisualElement {name=name,pickingMode=PickingMode.Ignore};p.Add(e);if(fill)Fill(e);return e;}
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private void Paint(VisualElement e,NativeMenuBrush.Paint p)=>_paint.Add(new NativeHudSurface(e,p));
        public void Initialize(Action invalidate,Action closeDetail)
        {
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Native run panel";_panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1000;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            _root=_document.rootVisualElement;Fill(_root);_root.pickingMode=PickingMode.Ignore;_root.style.backgroundColor=C("#02060d");
            _root.RegisterCallback<GeometryChangedEvent>(_=>invalidate());
            _root.style.color=C("#dcecff");_root.style.fontSize=16;
            _backdrops=new NativeUiBackdropRegistry(ReplicaPaint,_snapshots,true);
            _field=new Image {name="native-world-field",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};
            _field.style.width=Length.Percent(100);_field.style.height=Length.Percent(100);_field.style.maxWidth=1280;_field.style.maxHeight=800;_field.style.flexShrink=0;
            _root.style.alignItems=Align.Center;_root.style.justifyContent=Justify.Center;_root.Add(_field);
            // Field color treatment is baked by NativeWorldSurface, outside UI filter targets.
            var vignette=Box(_root,"native-field-vignette",true);
            Paint(vignette,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,.48f),Colors=new[]{Color.clear,new Color(0,0,0,.06f),new Color(0,0,0,.24f)},Stops=new[]{.48f,.72f,1f}});
            ControlsPredecessor=vignette;
            _left=Box(_root,"native-hud-left-anchor");_right=Box(_root,"native-hud-right-anchor");
            _left.style.position=_right.style.position=Position.Absolute;
            _combo=Box(_root,"native-combo");_combo.style.position=Position.Absolute;_combo.style.top=64;_combo.style.left=Length.Percent(50);_combo.style.translate=new Translate(Length.Percent(-50),0);_combo.style.alignItems=Align.Center;
            _comboTitle=NativePauseStatsView.Text(_combo,"combo-title",24,"#ff4d5d","700",32d/24);_comboTitle.style.unityTextAlign=TextAnchor.MiddleCenter;
            _comboMultiplier=NativePauseStatsView.Text(_combo,"combo-multiplier",14,"#b6c9de","700",20d/14);
            var track=Box(_combo,"combo-track");track.style.width=96;track.style.height=4;track.style.marginTop=4;track.style.backgroundColor=C("#243b55");track.style.overflow=Overflow.Hidden;
            track.style.borderTopLeftRadius=track.style.borderTopRightRadius=track.style.borderBottomLeftRadius=track.style.borderBottomRightRadius=999;
            _comboBar=Box(track,"combo-progress");_comboBar.style.height=Length.Percent(100);_comboBar.style.backgroundColor=C("#ffb84d");
            _pause=Overlay("native-pause-overlay",new Color(1/255f,4/255f,10/255f,.82f));_pause.style.alignItems=Align.Center;_pause.style.justifyContent=Justify.Center;
            _registrations.Add(RegisterNativeBackdrop(_pause,4));
            _shell=Box(_pause,"native-pause-shell");_shell.style.position=Position.Relative;_shell.style.flexShrink=1;_shell.style.overflow=Overflow.Hidden;
            _shell.style.borderLeftWidth=_shell.style.borderRightWidth=_shell.style.borderTopWidth=_shell.style.borderBottomWidth=1;
            _shell.style.borderLeftColor=_shell.style.borderRightColor=_shell.style.borderTopColor=_shell.style.borderBottomColor=Color.clear;
            _shell.style.borderTopLeftRadius=_shell.style.borderTopRightRadius=_shell.style.borderBottomLeftRadius=_shell.style.borderBottomRightRadius=3;
            Paint(_shell,new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=new Color(84/255f,190/255f,238/255f,.24f),Linear=true,Colors=new[]{new Color(7/255f,15/255f,25/255f,.92f),new Color(2/255f,7/255f,13/255f,.97f)},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(34,new Vector2(0,12),new Color(0,0,0,.34f)),Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),new Color(1,1,1,.035f))});
            var highlight=_highlight=Box(_shell,"pause-shell-highlight",true);
            // Background layers paint bottom to top, then native pause content.
            Paint(highlight,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,0),Colors=new[]{new Color(113/255f,101/255f,1,.08f),Color.clear},Stops=new[]{0f,.38f}});
            Paint(highlight,new NativeMenuBrush.Paint {Linear=true,Horizontal=true,Colors=new[]{Color.clear,new Color(110/255f,225/255f,1,.035f),Color.clear},Stops=new[]{0f,.5f,1f}});
            _detail=Box(_shell,"native-pause-detail-host",true);_detail.style.backgroundColor=new Color(0,0,0,.75f);_detail.pickingMode=PickingMode.Position;_detail.style.paddingLeft=_detail.style.paddingRight=_detail.style.paddingTop=_detail.style.paddingBottom=16;
            _detail.RegisterCallback<PointerDownEvent>(evt=>{closeDetail();invalidate();evt.StopPropagation();});
            _result=Overlay("native-result-host",new Color(0,0,0,.8f));
            _upgrade=Overlay("native-upgrade-host",new Color(0,0,0,.7f));
            _rewards=Overlay("native-rewards-host",new Color(0,0,0,.7f));_registrations.Add(RegisterNativeBackdrop(_rewards,4));
            _tutorial=Box(_root,"native-tutorial-host",true);
            Anchors.ReadVersion=()=>_version;Anchors.Backdrops=this;Anchors.Invalidate=invalidate;
        }
        private VisualElement Overlay(string name,Color dim){var e=Box(_root,name,true);e.pickingMode=PickingMode.Position;var shade=Box(e,name+"-shade",true);shade.style.backgroundColor=dim;return e;}
        internal void Begin(Texture world,float dpr,bool invalidate,bool active,bool pause,bool detail,bool upgrade,bool rewards,bool tutorial,
            double combo,double comboTimer,string comboTitle,string multiplier)
        {
            _visible=true;_root.style.display=DisplayStyle.Flex;_root.style.visibility=Visibility.Visible;_root.style.opacity=1;_root.SetEnabled(true);_panel.scale=dpr;_field.image=world;
            if(invalidate)_version++;
            Anchors.Clear();Bind(Anchors.HudLeft,_left);Bind(Anchors.HudRight,_right);
            float w=Screen.width/dpr,h=Screen.height/dpr;_shell.style.width=Mathf.Max(0,Mathf.Min(768,w-28));_shell.style.maxHeight=Mathf.Min(h*.92f,780);
            Show(_pause,pause);Show(_detail,pause&&detail);Show(_result,!active);Show(_upgrade,active&&upgrade);Show(_rewards,active&&rewards);Show(_tutorial,tutorial);
            if(pause){Bind(Anchors.PauseShell,_shell);if(detail)Bind(Anchors.PauseDetail,_detail);}
            if(!active)Bind(Anchors.Result,_result);if(active&&upgrade)Bind(Anchors.Upgrade,_upgrade);if(active&&rewards)Bind(Anchors.Rewards,_rewards);if(tutorial)Bind(Anchors.Tutorial,_tutorial);
            Show(_combo,active&&combo>=5);_comboTitle.text=comboTitle;_comboTitle.style.color=C(combo>=50?"#ffb84d":combo>=25?"#ff6b6b":"#ff4d5d");_comboMultiplier.text=multiplier;_comboBar.style.width=Length.Percent((float)(comboTimer/3*100));
        }
        private static void Bind(NativeRunUiAnchors.Slot slot,VisualElement element)=>slot.Bind(element,element,null);
        // Retain filtered render trees when an absolute overlay is hidden. Unity's
        // filter compositor can retain freed texture IDs across display:none.
        private static void Show(VisualElement e,bool show)
        {
            e.style.visibility=show?Visibility.Visible:Visibility.Hidden;
            // Replica descendants can have explicit visibility. Opacity applies
            // to the whole subtree, including those descendants.
            e.style.opacity=show?1:0;
        }
        internal void End(float dpr)
        {
            foreach(var p in _paint)p.Prepare(dpr);
            void Visit(VisualElement e){if(e.style.visibility==Visibility.Hidden||e.resolvedStyle.display==DisplayStyle.None)return;if(_backdrops.Contains(e)){var prefix=_root;for(var p=e.parent;p!=null;p=p.parent)if(p==_pause||p==_rewards){prefix=p;break;}_backdrops.Prepare(e,prefix,dpr);}for(int i=0;i<e.hierarchy.childCount;i++){var c=e.hierarchy[i];if(c.userData==SourceUiBackdrop.ReplicaTag||c.userData is SourceUiBackdrop.ClipTag)continue;Visit(c);}}
            _snapshots.Begin();try{Visit(_root);}finally{_snapshots.End();}
        }
        internal void OrderPauseContent(VisualElement content)
        {if(content.parent==_shell)content.PlaceInFront(_highlight);}
        internal void Hide()
        {
            if(!_visible)return;_visible=false;
            // Retain filter target ownership across menu/run transitions, as for
            // modal hosts. display:none invalidates Unity 6.3 filter texture IDs
            // still referenced by the retained render chain after a resize.
            _root.style.visibility=Visibility.Hidden;_root.style.opacity=0;_root.SetEnabled(false);
        }
        public IDisposable RegisterNativeBackdrop(VisualElement e,float sigma)=>_backdrops.RegisterNativeBackdrop(e,sigma);
        private static Action<MeshGenerationContext> ReplicaPaint(VisualElement source)=>source.userData is SourceUiReplica.PaintTag tag?tag.Paint:source is Image||source is TextElement?null:source.generateVisualContent;
        private void OnDestroy(){foreach(var r in _registrations)r.Dispose();_backdrops?.Dispose();foreach(var p in _paint)p.Dispose();if(_panel!=null)Destroy(_panel);}
    }
}
