using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained run hierarchy. The simulation and existing native presenters own
    // state/actions; this document owns geometry, paint order and blur prefixes.
    public class NativeRunDocument : MonoBehaviour, INativeUiBackdrops
    {
        private UIDocument _document; private PanelSettings _panel;private NativePanelLease _panelLease;
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
        protected virtual VisualTreeAsset SceneLayout => Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeBattle")??throw new InvalidOperationException("Native battle UXML is missing");
        private static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
        private void Paint(VisualElement e,NativeMenuBrush.Paint p)=>_paint.Add(new NativeHudSurface(e,p));
        public void Initialize(Action invalidate,Action closeDetail)
        {
            if(_document!=null)throw new InvalidOperationException("Native run document is already initialized");
            _panelLease=new NativePanelLease(gameObject,"Native run panel",1000,SceneLayout);
            _document=_panelLease.Document;_panel=_panelLease.Panel;
            T Require<T>(string name) where T:VisualElement=>_document.rootVisualElement.Q<T>(name)??throw new InvalidOperationException("Native battle layout is missing "+name);
            _root=Require<VisualElement>("native-run-root");
            _root.RegisterCallback<GeometryChangedEvent>(_=>invalidate());
            _backdrops=new NativeUiBackdropRegistry();
            _field=Require<Image>("native-world-field");
            _field.scaleMode=ScaleMode.StretchToFill;
            var vignette=Require<VisualElement>("native-field-vignette");
            Paint(vignette,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,.48f),Colors=new[]{Color.clear,new Color(0,0,0,.06f),new Color(0,0,0,.24f)},Stops=new[]{.48f,.72f,1f}});
            ControlsPredecessor=vignette;
            _left=Require<VisualElement>("native-hud-left-anchor");_right=Require<VisualElement>("native-hud-right-anchor");
            _combo=Require<VisualElement>("native-combo");_comboTitle=Require<Label>("combo-title");_comboMultiplier=Require<Label>("combo-multiplier");
            NativePauseStatsView.ConfigureText(_comboTitle,24,"#ff4d5d","700",32d/24);
            NativePauseStatsView.ConfigureText(_comboMultiplier,14,"#b6c9de","700",20d/14);
            _comboBar=Require<VisualElement>("combo-progress");
            _pause=Require<VisualElement>("native-pause-overlay");_shell=Require<VisualElement>("native-pause-shell");
            _registrations.Add(RegisterNativeBackdrop(_pause,4));
            Paint(_shell,new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=new Color(84/255f,190/255f,238/255f,.24f),Linear=true,Colors=new[]{new Color(7/255f,15/255f,25/255f,.92f),new Color(2/255f,7/255f,13/255f,.97f)},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(34,new Vector2(0,12),new Color(0,0,0,.34f)),Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),new Color(1,1,1,.035f))});
            var highlight=_highlight=Require<VisualElement>("pause-shell-highlight");
            Paint(highlight,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,0),Colors=new[]{new Color(113/255f,101/255f,1,.08f),Color.clear},Stops=new[]{0f,.38f}});
            Paint(highlight,new NativeMenuBrush.Paint {Linear=true,Horizontal=true,Colors=new[]{Color.clear,new Color(110/255f,225/255f,1,.035f),Color.clear},Stops=new[]{0f,.5f,1f}});
            _detail=Require<VisualElement>("native-pause-detail-host");
            _detail.RegisterCallback<PointerDownEvent>(evt=>{closeDetail();invalidate();evt.StopPropagation();});
            _result=Require<VisualElement>("native-result-host");_upgrade=Require<VisualElement>("native-upgrade-host");
            _rewards=Require<VisualElement>("native-rewards-host");_registrations.Add(RegisterNativeBackdrop(_rewards,4));
            _tutorial=Require<VisualElement>("native-tutorial-host");
            Anchors.ReadVersion=()=>_version;Anchors.Backdrops=this;Anchors.Invalidate=invalidate;
        }
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
#if UNITY_EDITOR
            if(NativeBackdropValidation.UseLegacy){
                _backdrops.BeginFrame();
                void Visit(VisualElement e){if(e.style.visibility==Visibility.Hidden||e.resolvedStyle.display==DisplayStyle.None)return;if(_backdrops.Contains(e)){var prefix=_root;for(var parent=e.parent;parent!=null;parent=parent.parent)if(parent==_pause||parent==_rewards){prefix=parent;break;}_backdrops.Prepare(e,prefix,dpr);}foreach(var child in e.Children())if(child.userData!=SourceUiBackdrop.ReplicaTag&&!(child.userData is SourceUiBackdrop.ClipTag))Visit(child);}
                Visit(_root);
            }
#endif
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
        protected virtual void OnDestroy(){foreach(var r in _registrations)r.Dispose();_backdrops?.Dispose();foreach(var p in _paint)p.Dispose();_panelLease?.Dispose();}
    }
}
