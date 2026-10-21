using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native result content. The existing game overlay retains the exact world
    // dimming and ordered backdrop until the shared game compositor is replaced.
    public sealed class NativeRunResultView : IDisposable
    {
        public readonly struct Row
        {
            public readonly string Label,Value;
            public Row(string label,string value){Label=label;Value=value;}
        }
        public sealed class State { public string Title,Record,Return;public bool Clear,NewRecord;public IReadOnlyList<Row> Rows; }
        public VisualElement Root {get;}
        public bool Ready {get;private set;}
        private readonly VisualElement _panel,_returnHost;
        private readonly Label _record;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly IDisposable _backdrop;
        private readonly NativeHudSurface _returnPaint;
        private readonly NativeMenuBrush.Paint _normal,_hover;
        private readonly AnimationCurve _ease;
        private readonly int _createdFrame;
        private readonly float _start;
        private bool _disposed;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        public NativeRunResultView(VisualElement parent,State state,SourceUiDocument document,Action exit)
        {
            Root=NativePauseStatsView.Box(parent,"native-run-result");Root.style.position=Position.Absolute;Root.style.left=Root.style.right=Root.style.top=Root.style.bottom=0;Root.style.alignItems=Align.Center;Root.style.justifyContent=Justify.Center;Root.style.opacity=0;
            _panel=NativePauseStatsView.Box(Root,"native-result-content");_panel.style.maxWidth=384;_panel.style.flexShrink=1;_panel.style.paddingLeft=_panel.style.paddingRight=24;
            var title=Text(_panel,"result-title",state.Title,36,state.Clear?"#55e6c1":"#ff4d5d","700",true,40d/36);title.style.unityTextAlign=TextAnchor.MiddleCenter;title.style.marginBottom=8;
            if(state.NewRecord){_record=Text(_panel,"result-record",state.Record,24,"#ffb84d","700",true,32d/24);_record.style.unityTextAlign=TextAnchor.MiddleCenter;_record.style.marginBottom=16;}
            var card=NativePauseStatsView.Box(_panel,"result-summary");card.style.paddingLeft=card.style.paddingRight=card.style.paddingTop=card.style.paddingBottom=24;card.style.marginBottom=24;ReserveBorder(card);
            Paint(card,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=C("#0d1726"),BorderColor=C("#243b55")});
            for(int i=0;i<state.Rows.Count;i++){var row=NativePauseStatsView.Box(card,"result-row-"+i,true);row.style.justifyContent=Justify.SpaceBetween;if(i>0)row.style.marginTop=8;
                Text(row,"result-label",state.Rows[i].Label,16,"#7f9bb8");Text(row,"result-value",state.Rows[i].Value,16,"#dcecff","500");}
            _returnHost=NativePauseStatsView.Box(_panel,"result-return-host");_returnHost.style.borderTopLeftRadius=_returnHost.style.borderTopRightRadius=_returnHost.style.borderBottomLeftRadius=_returnHost.style.borderBottomRightRadius=12;
            var button=new Button(exit){name="result-return"};button.style.marginLeft=button.style.marginRight=button.style.marginTop=button.style.marginBottom=0;button.style.paddingLeft=button.style.paddingRight=32;button.style.paddingTop=button.style.paddingBottom=12;button.style.backgroundColor=Color.clear;ReserveBorder(button);
            button.style.borderTopLeftRadius=button.style.borderTopRightRadius=button.style.borderBottomLeftRadius=button.style.borderBottomRightRadius=12;_returnHost.Add(button);
            _normal=new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=new Color(57f/255,216f/255,1,.20f),BorderColor=new Color(57f/255,216f/255,1,.40f)};
            _hover=new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=new Color(57f/255,216f/255,1,.30f),BorderColor=new Color(57f/255,216f/255,1,.40f)};
            _returnPaint=Paint(button,_normal);button.RegisterCallback<PointerEnterEvent>(_=>_returnPaint.SetPaint(_hover));button.RegisterCallback<PointerLeaveEvent>(_=>_returnPaint.SetPaint(_normal));
            Text(button,"result-return-copy",state.Return,16,"#dcecff").style.unityTextAlign=TextAnchor.MiddleCenter;
            _backdrop=document.RegisterNativeBackdrop(_returnHost,8);
            // Weighted native Unity tangents encode CSS cubic-bezier(.4,0,.6,1)
            // exactly, rather than replacing the authored pulse with a sine.
            var a=new Keyframe(0,0,0,0,1f/3,.4f){weightedMode=WeightedMode.Out};var b=new Keyframe(1,1,0,0,.4f,1f/3){weightedMode=WeightedMode.In};_ease=new AnimationCurve(a,b);
            _createdFrame=Time.frameCount;_start=Time.unscaledTime;
        }
        public void Prepare(float scale)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeRunResultView));
            Root.BringToFront();
            if(Root.layout.width<=0||Root.layout.height<=0||_panel.layout.width<=0||Time.frameCount-_createdFrame<2)return;
            foreach(var surface in _surfaces)surface.Prepare(scale);
            if(_record!=null){float phase=Mathf.Repeat(Time.unscaledTime-_start,2);_record.style.opacity=phase<1?Mathf.Lerp(1,.5f,_ease.Evaluate(phase)):Mathf.Lerp(.5f,1,_ease.Evaluate(phase-1));}
            Ready=true;Root.style.opacity=1;
        }
        private NativeHudSurface Paint(VisualElement e,NativeMenuBrush.Paint paint){var surface=new NativeHudSurface(e,paint);_surfaces.Add(surface);return surface;}
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var label=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);label.text=value;return label;}
        private static void ReserveBorder(VisualElement e){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;}
        public void Dispose(){if(_disposed)return;_disposed=true;_backdrop?.Dispose();foreach(var surface in _surfaces)surface.Dispose();Root.RemoveFromHierarchy();}
    }
}
