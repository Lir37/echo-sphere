using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeTutorialView : IDisposable
    {
        public sealed class State { public int Step;public bool CanAdvance,ShowAction;public string Kicker,Title,Body,Status,Skip,Next; }
        public VisualElement Root {get;}
        public bool Ready {get;private set;}
        private readonly VisualElement _card,_focus,_glow,_ring,_status,_footer;
        private readonly Image _shade;
        private readonly Label _kicker,_title,_body,_statusText,_skipText,_nextText;
        private readonly Button _skip,_next;
        private readonly NativeHudSurface _cardPaint,_focusPaint,_glowPaint,_ringPaint,_statusPaint,_nextPaint;
        private readonly NativeMenuBrush.Paint _waiting,_success,_upgrade;
        private readonly INativeUiBackdrops _document;
        private IDisposable _nextBackdrop;
        private readonly List<IDisposable> _backdrops=new List<IDisposable>();
        private readonly Material _shadeMaterial;
        private RenderTexture _shadeTarget;
        private readonly AnimationCurve _pulse;
        private Rect _cut;private float _scale=-1,_width=-1,_height=-1,_pulseStart;
        private bool _hasCut,_disposed;private readonly int _createdFrame;private State _state;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        public NativeTutorialView(VisualElement parent,INativeUiBackdrops document,Action next,Action skip)
        {
            _document=document;
            var layout=Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeTutorial")??throw new InvalidOperationException("Native tutorial UXML is missing");
            layout.CloneTree(parent);Root=parent.Q<VisualElement>("native-tutorial");
            T Require<T>(string name) where T:VisualElement=>Root.Q<T>(name)??throw new InvalidOperationException("Native tutorial layout is missing "+name);
            _glow=Require<VisualElement>("tutorial-focus-glow");_glowPaint=new NativeHudSurface(_glow,new NativeMenuBrush.Paint {Radius=12,OuterA=new NativeMenuBrush.Shadow(24,Vector2.zero,C("#39d8ff4d"))});
            _shade=Require<Image>("tutorial-shade");_shade.scaleMode=ScaleMode.StretchToFill;
            _shadeMaterial=new Material(Resources.Load<Shader>("EchoSphere/NativeTutorialShade")){name="Native tutorial spotlight",hideFlags=HideFlags.DontSave};
            _focus=Require<VisualElement>("tutorial-focus");_focusPaint=new NativeHudSurface(_focus,new NativeMenuBrush.Paint {Radius=12,Border=2,BorderColor=C("#63e6ff")});
            _ring=Require<VisualElement>("tutorial-pulse-ring");_ringPaint=new NativeHudSurface(_ring,new NativeMenuBrush.Paint {Radius=12,Border=1,BorderColor=C("#dcecff4d")});
            _card=Require<VisualElement>("tutorial-card");_cardPaint=new NativeHudSurface(_card,new NativeMenuBrush.Paint {Radius=16,Border=1,BorderColor=C("#63e6ff4d"),Fill=C("#030a14f2"),OuterA=new NativeMenuBrush.Shadow(50,new Vector2(0,16),new Color(0,0,0,.55f))});
            _kicker=Require<Label>("tutorial-step");_title=Require<Label>("tutorial-title");_body=Require<Label>("tutorial-body");
            NativePauseStatsView.ConfigureFont(_kicker,"900",1.5);NativePauseStatsView.ConfigureFont(_title,"700",28d/18);NativePauseStatsView.ConfigureFont(_body,"400",1.625);
            _status=Require<VisualElement>("tutorial-status");
            _waiting=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#243b55"),Fill=C("#09121f")};_success=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#55e6c166"),Fill=C("#55e6c11a")};_upgrade=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#b68cff4d"),Fill=C("#b68cff1a")};_statusPaint=new NativeHudSurface(_status,_waiting);
            _statusText=Require<Label>("tutorial-status-text");NativePauseStatsView.ConfigureFont(_statusText,"700",1.5);
            _footer=Require<VisualElement>("tutorial-footer");_skip=Require<Button>("tutorial-skip");_next=Require<Button>("tutorial-next");_skip.clicked+=skip;_next.clicked+=next;
            _skipText=Require<Label>("tutorial-skip-text");_nextText=Require<Label>("tutorial-next-text");
            NativePauseStatsView.ConfigureFont(_skipText,"700",1.5);NativePauseStatsView.ConfigureFont(_nextText,"900",1.5);
            _nextPaint=new NativeHudSurface(_next,new NativeMenuBrush.Paint {Radius=12,Border=1,BorderColor=C("#63e6ff66"),Fill=C("#63e6ff1a")});
            _backdrops.Add(document.RegisterNativeBackdrop(_skip,8));_nextBackdrop=document.RegisterNativeBackdrop(_next,8);
            var a=new Keyframe(0,0,0,0,1f/3,.4f){weightedMode=WeightedMode.Out};var b=new Keyframe(1,1,0,0,.4f,1f/3){weightedMode=WeightedMode.In};_pulse=new AnimationCurve(a,b);_createdFrame=Time.frameCount;
        }
        public void Bind(State state)
        {
            _state=state;Set(_kicker,state.Kicker);Set(_title,state.Title);Set(_body,state.Body);Set(_statusText,state.Status);Set(_skipText,state.Skip.ToUpperInvariant());Set(_nextText,state.Next.ToUpperInvariant());
            _status.style.display=state.ShowAction||state.Step==4?DisplayStyle.Flex:DisplayStyle.None;
            _statusPaint.SetPaint(state.Step==4?_upgrade:state.CanAdvance?_success:_waiting);_statusText.style.color=C(state.Step==4?"#dbcfff":state.CanAdvance?"#b9ffe8":"#7089a0");
            // This flow button genuinely leaves layout on the choice step.
            // Retire its nested filter before display:none, then create a fresh
            // prefix when it returns; old prefixes can reference closed cards.
            if(state.Step==4){_nextBackdrop?.Dispose();_nextBackdrop=null;}else if(_nextBackdrop==null)_nextBackdrop=_document.RegisterNativeBackdrop(_next,8);
            _next.style.display=state.Step==4?DisplayStyle.None:DisplayStyle.Flex;_next.SetEnabled(state.CanAdvance);_next.style.opacity=state.CanAdvance?1:.3f;
        }
        public void Prepare(float width,float height,float scale,Rect? target)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeTutorialView));if(_state==null)return;Root.BringToFront();
            float cardWidth=Mathf.Min(340,width-24);_card.style.width=cardWidth;
            if(target.HasValue){
                var rect=target.Value;float top=rect.y>height*.48f?rect.y-170:rect.yMax+14;
                _card.style.left=Mathf.Min(width-cardWidth-12,Mathf.Max(12,rect.center.x-cardWidth*.5f));_card.style.top=Mathf.Min(height-188,Mathf.Max(12,top));_card.style.translate=new Translate(0,0,0);
                var cut=new Rect(Mathf.Max(4,rect.x-8),Mathf.Max(4,rect.y-8),rect.width+16,rect.height+16);
                Place(_glow,cut);Place(_focus,cut);Place(_ring,new Rect(cut.x-3,cut.y-3,cut.width+6,cut.height+6));
                if(!_hasCut)_pulseStart=Time.unscaledTime;
                if(!_hasCut||cut!=_cut||width!=_width||height!=_height||scale!=_scale)RenderShade(width,height,scale,cut);
                _hasCut=true;_cut=cut;float phase=Mathf.Repeat(Time.unscaledTime-_pulseStart,2);_ring.style.opacity=phase<1?Mathf.Lerp(1,.5f,_pulse.Evaluate(phase)):Mathf.Lerp(.5f,1,_pulse.Evaluate(phase-1));
            }else{_hasCut=false;_card.style.left=Mathf.Max(12,(width-cardWidth)*.5f);_card.style.top=height*.5f;_card.style.translate=new Translate(0,Length.Percent(-50),0);}
            _width=width;_height=height;_scale=scale;_shade.style.display=_glow.style.display=_focus.style.display=_ring.style.display=_hasCut?DisplayStyle.Flex:DisplayStyle.None;
            if(_card.layout.width<=0||Time.frameCount-_createdFrame<2)return;
            _cardPaint.Prepare(scale);if(_hasCut){_glowPaint.Prepare(scale);_focusPaint.Prepare(scale);_ringPaint.Prepare(scale);}_statusPaint.Prepare(scale);_nextPaint.Prepare(scale);Ready=true;Root.style.opacity=1;
        }
        private void RenderShade(float width,float height,float scale,Rect cut)
        {
            int w=Mathf.CeilToInt(width*scale),h=Mathf.CeilToInt(height*scale);
            if(_shadeTarget==null||_shadeTarget.width!=w||_shadeTarget.height!=h){ReleaseShade();_shadeTarget=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native tutorial spotlight",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};if(!_shadeTarget.Create())throw new InvalidOperationException("Tutorial spotlight allocation failed");_shade.image=_shadeTarget;}
            _shadeMaterial.SetVector("_Pixels",new Vector4(w,h,0,0));_shadeMaterial.SetVector("_Cut",new Vector4(cut.x,cut.y,cut.width,cut.height));_shadeMaterial.SetFloat("_Scale",scale);
            var old=RenderTexture.active;bool srgb=GL.sRGBWrite;try{GL.sRGBWrite=false;Graphics.Blit(Texture2D.blackTexture,_shadeTarget,_shadeMaterial);}finally{GL.sRGBWrite=srgb;RenderTexture.active=old;}
        }
        private void ReleaseShade(){if(_shadeTarget==null)return;_shade.image=null;_shadeTarget.Release();UnityEngine.Object.Destroy(_shadeTarget);_shadeTarget=null;}
        private static void Place(VisualElement e,Rect r){e.style.left=r.x;e.style.top=r.y;e.style.width=r.width;e.style.height=r.height;}
        private static void Set(Label label,string value)=>NativePauseStatsView.Set(label,value);
        public void Dispose(){if(_disposed)return;_disposed=true;_nextBackdrop?.Dispose();foreach(var backdrop in _backdrops)backdrop.Dispose();_cardPaint.Dispose();_glowPaint.Dispose();_focusPaint.Dispose();_ringPaint.Dispose();_statusPaint.Dispose();_nextPaint.Dispose();ReleaseShade();UnityEngine.Object.Destroy(_shadeMaterial);Root.RemoveFromHierarchy();}
    }
}
