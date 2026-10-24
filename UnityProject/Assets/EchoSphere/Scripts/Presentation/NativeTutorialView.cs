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
            _document=document;Root=Box(parent,"native-tutorial");Fill(Root);Root.style.opacity=0;
            // CSS lists the huge black shadow above the cyan glow, then paints
            // the border. Keep that order instead of brightening the glow.
            _glow=Box(Root,"tutorial-focus-glow");_glow.style.position=Position.Absolute;_glowPaint=new NativeHudSurface(_glow,new NativeMenuBrush.Paint {Radius=12,OuterA=new NativeMenuBrush.Shadow(24,Vector2.zero,C("#39d8ff4d"))});
            _shade=new Image {name="tutorial-shade",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};Fill(_shade);Root.Add(_shade);
            _shadeMaterial=new Material(Resources.Load<Shader>("EchoSphere/NativeTutorialShade")){name="Native tutorial spotlight",hideFlags=HideFlags.DontSave};
            _focus=Box(Root,"tutorial-focus");_focus.style.position=Position.Absolute;
            _focusPaint=new NativeHudSurface(_focus,new NativeMenuBrush.Paint {Radius=12,Border=2,BorderColor=C("#63e6ff")});
            _ring=Box(Root,"tutorial-pulse-ring");_ring.style.position=Position.Absolute;_ringPaint=new NativeHudSurface(_ring,new NativeMenuBrush.Paint {Radius=12,Border=1,BorderColor=C("#dcecff4d")});
            _card=Box(Root,"tutorial-card");_card.pickingMode=PickingMode.Position;_card.style.position=Position.Absolute;_card.style.paddingLeft=_card.style.paddingRight=_card.style.paddingTop=_card.style.paddingBottom=16;Border(_card,16,1);
            _cardPaint=new NativeHudSurface(_card,new NativeMenuBrush.Paint {Radius=16,Border=1,BorderColor=C("#63e6ff4d"),Fill=C("#030a14f2"),OuterA=new NativeMenuBrush.Shadow(50,new Vector2(0,16),new Color(0,0,0,.55f))});
            _kicker=Text(_card,"tutorial-step",9,"#63e6ff","900");_kicker.style.letterSpacing=22;
            _title=Text(_card,"tutorial-title",18,"#dcecff","700",true,28d/18);_title.style.marginTop=4;
            _body=Text(_card,"tutorial-body",12,"#a9c0d2","400",true,1.625);_body.style.marginTop=8;
            _status=Box(_card,"tutorial-status");_status.style.marginTop=12;_status.style.paddingLeft=_status.style.paddingRight=12;_status.style.paddingTop=_status.style.paddingBottom=8;Border(_status,8,1);
            _waiting=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#243b55"),Fill=C("#09121f")};_success=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#55e6c166"),Fill=C("#55e6c11a")};_upgrade=new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#b68cff4d"),Fill=C("#b68cff1a")};_statusPaint=new NativeHudSurface(_status,_waiting);_statusText=Text(_status,"tutorial-status-text",10,"#7089a0","700",true);
            _footer=Box(_card,"tutorial-footer",true);_footer.style.justifyContent=Justify.SpaceBetween;_footer.style.alignItems=Align.Center;_footer.style.marginTop=16;
            _skip=new Button(skip){name="tutorial-skip"};ButtonReset(_skip);_skip.style.paddingLeft=_skip.style.paddingRight=_skip.style.paddingTop=_skip.style.paddingBottom=8;_skip.style.borderTopLeftRadius=_skip.style.borderTopRightRadius=_skip.style.borderBottomLeftRadius=_skip.style.borderBottomRightRadius=8;_footer.Add(_skip);_skipText=Text(_skip,"tutorial-skip-text",9,"#718aa1","700");_skipText.style.letterSpacing=5;
            _next=new Button(next){name="tutorial-next"};ButtonReset(_next);_next.style.marginLeft=8;_next.style.paddingLeft=_next.style.paddingRight=16;_next.style.paddingTop=_next.style.paddingBottom=10;Border(_next,12,1);_footer.Add(_next);_nextPaint=new NativeHudSurface(_next,new NativeMenuBrush.Paint {Radius=12,Border=1,BorderColor=C("#63e6ff66"),Fill=C("#63e6ff1a")});_nextText=Text(_next,"tutorial-next-text",10,"#dffaff","900");_nextText.style.letterSpacing=5;
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
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,float size,string color,string weight="400",bool wrap=false,double line=1.5)=>NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);
        private static void Set(Label label,string value)=>NativePauseStatsView.Set(label,value);
        private static void ButtonReset(Button b){b.style.marginLeft=b.style.marginRight=b.style.marginTop=b.style.marginBottom=0;b.style.backgroundColor=Color.clear;b.style.borderLeftWidth=b.style.borderRightWidth=b.style.borderTopWidth=b.style.borderBottomWidth=0;}
        private static void Border(VisualElement e,float radius,float width){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=width;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        public void Dispose(){if(_disposed)return;_disposed=true;_nextBackdrop?.Dispose();foreach(var backdrop in _backdrops)backdrop.Dispose();_cardPaint.Dispose();_glowPaint.Dispose();_focusPaint.Dispose();_ringPaint.Dispose();_statusPaint.Dispose();_nextPaint.Dispose();ReleaseShade();UnityEngine.Object.Destroy(_shadeMaterial);Root.RemoveFromHierarchy();}
    }
}
