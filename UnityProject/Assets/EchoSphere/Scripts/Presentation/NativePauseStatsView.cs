using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained native content for PausePlanner's stats tab. Header, navigation,
    // modal ordering and the native ScrollView remain with the screen owner.
    public sealed class NativePauseStatsView : IDisposable
    {
        private const string Family="Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif";
        private static readonly Regex Whitespace=new Regex(@"\s+");
        private sealed class StatCell {internal VisualElement Root;internal Label Label,Value;}
        private sealed class SphereCard
        {
            internal VisualElement Root,Ring,Grid;
            internal Label Name,Level,Badge,Description,Branch;
            internal NativeHudSurface BadgePaint;
            internal readonly List<StatCell> Metrics=new List<StatCell>();
        }
        private readonly VisualElement _root,_statsGrid;
        private readonly Label _characterTitle,_sphereTitle;
        private readonly List<StatCell> _stats=new List<StatCell>();
        private readonly List<SphereCard> _spheres=new List<SphereCard>();
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly NativeMenuBrush.Paint _cardPaint=new NativeMenuBrush.Paint {Radius=3,Border=1,BorderColor=A(76,165,214,.16f),Fill=A(6,17,29,.78f)};
        private readonly NativeMenuBrush.Paint _metricPaint=new NativeMenuBrush.Paint {Radius=6,Fill=A(0,0,0,.05f)};
        private readonly NativeMenuBrush.Paint _placedPaint=new NativeMenuBrush.Paint {Radius=6,Fill=A(90,140,74,.15f)};
        private readonly NativeMenuBrush.Paint _emptyPaint=new NativeMenuBrush.Paint {Radius=6,Fill=A(0,0,0,.05f)};
        private bool _disposed,_bound,_flow;
        private int _geometryFrame,_columns=4;
        private float _viewportWidth;
        public VisualElement Root=>_root;
        public bool Ready {get;private set;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        private static Color C(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)
        {
            var box=new VisualElement {name=name,pickingMode=PickingMode.Ignore};box.style.flexShrink=0;box.style.minWidth=0;
            if(row)box.style.flexDirection=FlexDirection.Row;parent.Add(box);return box;
        }
        private static Label Text(VisualElement parent,string name,float size,string color,string weight="400",double line=1.5,float spacing=.192f,bool wrap=false)
        {
            var label=new Label {name=name,pickingMode=PickingMode.Ignore,enableRichText=false};
            label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;
            label.style.fontSize=size;label.style.color=C(color);label.style.letterSpacing=spacing*100/size;
            label.style.flexShrink=0;label.style.minWidth=0;label.style.whiteSpace=wrap?WhiteSpace.Normal:WhiteSpace.NoWrap;
            if(!wrap)label.style.height=(float)(size*line);
            var font=SourceUiTypography.Resolve(Family,weight,false,line);if(font!=null)label.style.unityFontDefinition=FontDefinition.FromSDFFont(font);label.style.unityFontStyleAndWeight=FontStyle.Normal;
            parent.Add(label);return label;
        }
        private static void Set(Label label,string value)
        {
            if((string)label.userData==value)return;label.userData=value;
            label.text=Whitespace.Replace(value??""," ");
        }
        private NativeHudSurface Paint(VisualElement box,NativeMenuBrush.Paint paint)
        {var surface=new NativeHudSurface(box,paint);_surfaces.Add(surface);return surface;}
        private void Card(VisualElement box,float x,float y,NativeMenuBrush.Paint paint,bool border)
        {
            box.style.paddingLeft=box.style.paddingRight=x;box.style.paddingTop=box.style.paddingBottom=y;
            if(border){box.style.borderLeftWidth=box.style.borderRightWidth=box.style.borderTopWidth=box.style.borderBottomWidth=1;box.style.borderLeftColor=box.style.borderRightColor=box.style.borderTopColor=box.style.borderBottomColor=Color.clear;}
            Paint(box,paint);
        }
        private StatCell Cell(VisualElement parent,string name,bool metric)
        {
            var box=Box(parent,name);Card(box,metric?8:12,metric?6:8,metric?_metricPaint:_cardPaint,!metric);
            return new StatCell {Root=box,Label=Text(box,name+"-label",9,"#64849b"),Value=Text(box,name+"-value",metric?10:14,"#dcecff","700",metric?1.5:20d/14)};
        }
        public NativePauseStatsView(VisualElement parent)
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));
            _root=Box(parent,"native-pause-stats");_root.style.position=Position.Absolute;_root.style.left=_root.style.top=0;_root.style.width=Length.Percent(100);_root.style.opacity=0;
            _characterTitle=Text(_root,"pause-character-title",10,"#64849b","700",spacing:.5f);_characterTitle.style.marginBottom=8;
            _statsGrid=Box(_root,"pause-stat-grid",true);_statsGrid.style.flexWrap=Wrap.Wrap;
            for(int i=0;i<8;i++)_stats.Add(Cell(_statsGrid,"pause-stat-"+i,false));
            _sphereTitle=Text(_root,"pause-sphere-title",10,"#64849b","700",spacing:.5f);_sphereTitle.style.marginTop=16;_sphereTitle.style.marginBottom=8;
            _root.RegisterCallback<GeometryChangedEvent>(GeometryChanged);_geometryFrame=Time.frameCount;
        }
        private SphereCard AddSphere(int index)
        {
            var card=new SphereCard {Root=Box(_root,"pause-sphere-"+index)};Card(card.Root,12,12,_cardPaint,true);if(index>0)card.Root.style.marginTop=8;
            var heading=Box(card.Root,"pause-sphere-heading-"+index,true);heading.style.justifyContent=Justify.SpaceBetween;heading.style.alignItems=Align.Center;
            var left=Box(heading,"pause-sphere-heading-left-"+index,true);left.style.flexShrink=1;left.style.alignItems=Align.Center;left.style.marginRight=8;
            card.Ring=Box(left,"pause-sphere-ring-"+index);card.Ring.style.width=card.Ring.style.height=12;card.Ring.style.borderLeftWidth=card.Ring.style.borderRightWidth=card.Ring.style.borderTopWidth=card.Ring.style.borderBottomWidth=2;card.Ring.style.borderTopLeftRadius=card.Ring.style.borderTopRightRadius=card.Ring.style.borderBottomLeftRadius=card.Ring.style.borderBottomRightRadius=999;card.Ring.style.marginRight=8;
            card.Name=Text(left,"pause-sphere-name-"+index,16,"#dcecff","700");card.Name.style.flexShrink=1;card.Name.style.overflow=Overflow.Hidden;card.Name.style.textOverflow=TextOverflow.Ellipsis;card.Name.style.marginRight=8;
            card.Level=Text(left,"pause-sphere-level-"+index,10,"#64849b");
            var badge=Box(heading,"pause-sphere-badge-"+index);badge.style.paddingLeft=badge.style.paddingRight=8;badge.style.paddingTop=badge.style.paddingBottom=4;card.BadgePaint=Paint(badge,_emptyPaint);card.Badge=Text(badge,"pause-sphere-placement-"+index,10,"#64849b","700");
            card.Grid=Box(card.Root,"pause-sphere-metrics-"+index,true);card.Grid.style.flexWrap=Wrap.Wrap;card.Grid.style.marginTop=8;
            for(int i=0;i<4;i++)card.Metrics.Add(Cell(card.Grid,"pause-metric-"+index+"-"+i,true));
            card.Description=Text(card.Root,"pause-sphere-description-"+index,10,"#64849b",wrap:true);card.Description.style.marginTop=8;
            card.Branch=Text(card.Root,"pause-sphere-branch-"+index,10,"#dcecff",wrap:true);card.Branch.enableRichText=true;card.Branch.style.marginTop=8;
            _spheres.Add(card);return card;
        }
        public void UseFlowLayout(bool flow)
        {
            if(_flow==flow)return;_flow=flow;_root.style.position=flow?Position.Relative:Position.Absolute;
            if(flow){_root.style.left=_root.style.top=StyleKeyword.Auto;}else _root.style.left=_root.style.top=0;
        }
        private void GeometryChanged(GeometryChangedEvent evt){_geometryFrame=Time.frameCount;Layout();}
        private void Layout()
        {
            float width=_root.contentRect.width;if(!float.IsFinite(width)||width<=0)return;
            _columns=_viewportWidth<640?2:4;float cell=(width-8*(_columns-1))/_columns;
            for(int i=0;i<_stats.Count;i++){var box=_stats[i].Root;box.style.width=cell;box.style.marginRight=i%_columns==_columns-1?0:8;box.style.marginTop=i>=_columns?8:0;}
            foreach(var card in _spheres){float inner=Mathf.Max(0,width-26);float metric=(inner-16)/3;for(int i=0;i<card.Metrics.Count;i++){var box=card.Metrics[i].Root;box.style.width=metric;box.style.marginRight=i%3==2?0:8;box.style.marginTop=i>=3?8:0;}}
        }
        public void Bind(NativePauseStatsState state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseStatsView));
            if(state==null||state.Stats.Count!=8)throw new ArgumentException("Original eight PausePlanner stats required");
            _bound=true;Set(_characterTitle,state.CharacterHeading.ToUpperInvariant());Set(_sphereTitle,state.SpheresHeading.ToUpperInvariant());
            for(int i=0;i<8;i++){Set(_stats[i].Label,state.Stats[i].Label.ToUpperInvariant());Set(_stats[i].Value,state.Stats[i].Value);}
            while(_spheres.Count<state.Spheres.Count)AddSphere(_spheres.Count);
            for(int i=0;i<_spheres.Count;i++){
                var card=_spheres[i];card.Root.style.display=i<state.Spheres.Count?DisplayStyle.Flex:DisplayStyle.None;if(i>=state.Spheres.Count)continue;var model=state.Spheres[i];
                Set(card.Name,model.Name);Set(card.Level,model.Level);Set(card.Badge,model.Placement);card.Badge.style.color=model.OnField?C("#5a8c4a"):C("#64849b");card.BadgePaint.SetPaint(model.OnField?_placedPaint:_emptyPaint);var color=C(model.Color);card.Ring.style.borderLeftColor=card.Ring.style.borderRightColor=card.Ring.style.borderTopColor=card.Ring.style.borderBottomColor=color;
                card.Grid.style.display=model.OnField?DisplayStyle.Flex:DisplayStyle.None;card.Description.style.display=model.OnField?DisplayStyle.None:DisplayStyle.Flex;Set(card.Description,model.Description);
                for(int j=0;j<card.Metrics.Count;j++){var metric=card.Metrics[j];metric.Root.style.display=j<model.Metrics.Count?DisplayStyle.Flex:DisplayStyle.None;if(j<model.Metrics.Count){Set(metric.Label,model.Metrics[j].Label);Set(metric.Value,model.Metrics[j].Value);}}
                card.Branch.style.display=model.HasBranch?DisplayStyle.Flex:DisplayStyle.None;
                string Literal(string text)=>"<noparse>"+Whitespace.Replace(text??""," ")+"</noparse>";
                var branch="<color=#8064a8><b>"+Literal(model.BranchLabel)+"</b></color> "+Literal(model.BranchName)+(model.FinalName==null?"":" <color=#ff6b6b><b>→ "+Literal(model.FinalName)+"</b></color>");if(card.Branch.text!=branch)card.Branch.text=branch;
            }
            Layout();
        }
        public void Prepare(float scale,float viewportWidth)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativePauseStatsView));
            if(viewportWidth!=_viewportWidth){_viewportWidth=viewportWidth;Layout();}
            var bounds=_root.layout;
            if(!_bound||!float.IsFinite(bounds.width+bounds.height)||bounds.width<=0||bounds.height<=0)return;
            foreach(var surface in _surfaces)surface.Prepare(scale);
            if(!Ready&&Time.frameCount-_geometryFrame>=2){Ready=true;_root.style.opacity=1;}
        }
        public void Dispose(){if(_disposed)return;_disposed=true;_root.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);foreach(var surface in _surfaces)surface.Dispose();_root.RemoveFromHierarchy();}
    }
}
