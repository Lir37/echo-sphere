using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native retained joystick and placement rings. Positions/lifetime are
    // supplied by the existing gesture controller; this class has no game rules.
    internal sealed class NativeMobileFeedbackView : IDisposable
    {
        internal sealed class State {internal string Id,Color;internal bool Joystick,Active;internal float KnobX,KnobY;}
        internal VisualElement Root {get;}
        internal bool Ready {get;private set;}
        private readonly VisualElement _outer,_inner;
        private readonly NativeHudSurface _outerPaint,_innerPaint;
        private readonly IDisposable _backdrop;
        private readonly bool _joystick;
        private readonly AnimationCurve _ping;
        private string _id,_color;private float _started;private int _created;
        private static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        internal NativeMobileFeedbackView(VisualElement parent,INativeUiBackdrops document,bool joystick)
        {
            _joystick=joystick;Root=Box(parent,joystick?"native-joystick":"native-placement");Fill(Root);Root.style.opacity=0;
            _outer=Box(Root,"outer");Fill(_outer);_inner=Box(Root,"inner");_inner.style.position=Position.Absolute;
            if(joystick){
                Border(_outer,58);Border(_inner,24);_inner.style.width=_inner.style.height=48;
                // Source .absolute.rounded-full.border is more specific than
                // .bg-white/25: both authored circles inherit this border.
                _outerPaint=new NativeHudSurface(_outer,new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=A(134,228,255,.34f),Fill=A(2,8,16,.52f)});
                _innerPaint=new NativeHudSurface(_inner,new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=A(134,228,255,.34f),Fill=A(111,218,255,.22f),OuterA=new NativeMenuBrush.Shadow(16,Vector2.zero,A(80,215,255,.10f))});
                _backdrop=document.RegisterNativeBackdrop(_outer,2);
            }else{
                _inner.style.left=_inner.style.right=_inner.style.top=_inner.style.bottom=9;Border(_outer,26);Border(_inner,17);
                _outerPaint=new NativeHudSurface(_outer,new NativeMenuBrush.Paint());_innerPaint=new NativeHudSurface(_inner,new NativeMenuBrush.Paint());
            }
            var a=new Keyframe(0,0,0,0,1f/3,0){weightedMode=WeightedMode.Out};var b=new Keyframe(1,1,0,0,.8f,1f/3){weightedMode=WeightedMode.In};_ping=new AnimationCurve(a,b);_created=Time.frameCount;
        }
        internal void Bind(State s)
        {
            if(_joystick){_inner.style.left=s.KnobX;_inner.style.top=s.KnobY;_inner.style.opacity=s.Active ? .9f : .7f;}
            else{
                if(_id!=s.Id){_id=s.Id;_started=Time.unscaledTime;}
                if(_color!=s.Color){_color=s.Color;var color=C(s.Color);_outerPaint.SetPaint(new NativeMenuBrush.Paint {Ellipse=true,Border=2,BorderColor=color});_innerPaint.SetPaint(new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=A(134,228,255,.34f)});}
                _inner.style.opacity=.55f;
            }
        }
        internal void Prepare(float scale)
        {
            if(!_joystick){float phase=Mathf.Repeat(Time.unscaledTime-_started,1);float t=_ping.Evaluate(Mathf.Min(1,phase/.75f));_outer.style.scale=new Scale(new Vector3(1+t,1+t,1));_outer.style.opacity=.7f*(1-t);}
            if(Root.layout.width<=0||Root.layout.height<=0)return;_outerPaint.Prepare(scale);_innerPaint.Prepare(scale);if(Time.frameCount-_created>=2){Ready=true;Root.style.opacity=1;}
        }
        private static VisualElement Box(VisualElement p,string name){var e=new VisualElement {name=name,pickingMode=PickingMode.Ignore};p.Add(e);return e;}
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static void Border(VisualElement e,float radius){e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        public void Dispose(){_backdrop?.Dispose();_outerPaint.Dispose();_innerPaint.Dispose();Root.RemoveFromHierarchy();}
    }
}
