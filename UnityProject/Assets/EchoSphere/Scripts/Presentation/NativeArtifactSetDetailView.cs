using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeArtifactSetDetailView : IDisposable
    {
        public sealed class Item { public NativePauseArtifactsState.Artifact Artifact;public bool Owned; }
        public sealed class State { public string Id,Name,Description;public int Owned,Total;public bool Russian;public IReadOnlyList<Item> Items; }
        public VisualElement Root {get;}
        public string Id {get;}
        public bool Ready {get;private set;}
        private readonly VisualElement _shell;
        private readonly List<NativeHudSurface> _paint=new List<NativeHudSurface>();
        private readonly List<NativeArtifactGlyph> _glyphs=new List<NativeArtifactGlyph>();
        private readonly IDisposable _backdrop;
        private readonly int _createdFrame;
        private bool _disposed;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        public NativeArtifactSetDetailView(VisualElement parent,State state,SourceUiDocument document,Action close,bool dim=true)
        {
            Id=state.Id;string L(string a,string b)=>state.Russian?a:b;
            Root=Box(parent,"native-artifact-set-detail");Root.pickingMode=PickingMode.Position;Root.style.position=Position.Absolute;Root.style.left=Root.style.right=Root.style.top=Root.style.bottom=0;Root.style.alignItems=Align.Center;Root.style.justifyContent=Justify.Center;Root.style.opacity=0;
            if(dim){Root.style.backgroundColor=new Color(0,0,0,.75f);Root.style.paddingLeft=Root.style.paddingRight=Root.style.paddingTop=Root.style.paddingBottom=16;}
            Root.RegisterCallback<PointerDownEvent>(evt=>{close?.Invoke();evt.StopPropagation();});
            _shell=Box(Root,"set-detail-shell");_shell.style.maxWidth=448;_shell.style.paddingLeft=_shell.style.paddingRight=_shell.style.paddingTop=_shell.style.paddingBottom=20;Border(_shell,8,1);_shell.RegisterCallback<PointerDownEvent>(evt=>evt.StopPropagation());
            var before=Box(_shell,"set-detail-shell-highlight");before.style.position=Position.Absolute;before.style.left=before.style.right=before.style.top=before.style.bottom=0;
            Paint(before,new NativeMenuBrush.Paint {Linear=true,Horizontal=true,Colors=new[]{Color.clear,C("#6ee1ff09"),Color.clear},Stops=new[]{0f,.5f,1f}});
            Paint(before,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,0),Colors=new[]{C("#7165ff14"),Color.clear},Stops=new[]{0f,.38f}});
            Paint(_shell,new NativeMenuBrush.Paint {Radius=8,Border=1,BorderColor=C("#54beee3d"),Linear=true,Colors=new[]{C("#081420f7"),C("#030910fa")},Stops=new[]{0f,1f},OuterA=new NativeMenuBrush.Shadow(80,new Vector2(0,20),new Color(0,0,0,.60f)),Inset=new NativeMenuBrush.Shadow(60,Vector2.zero,C("#3491ca09"))});
            var header=Box(_shell,"set-detail-header",true);header.style.alignItems=Align.Center;
            var icon=Box(header,"set-detail-icon");icon.style.width=icon.style.height=52;icon.style.marginRight=12;icon.style.alignItems=Align.Center;icon.style.justifyContent=Justify.Center;Border(icon,999,2);
            Paint(icon,new NativeMenuBrush.Paint {Ellipse=true,Border=2,BorderColor=C("#ffb84db3"),OuterA=new NativeMenuBrush.Shadow(20,Vector2.zero,C("#ffb84d29"))});
            var quarters=Box(icon,"set-detail-conic");quarters.style.position=Position.Absolute;quarters.style.left=quarters.style.right=quarters.style.top=quarters.style.bottom=0;quarters.generateVisualContent+=context=>{var p=context.painter2D;p.fillColor=C("#ffb84d");var center=quarters.contentRect.center;float r=Mathf.Min(quarters.contentRect.width,quarters.contentRect.height)*.5f;for(int i=0;i<2;i++){p.BeginPath();p.MoveTo(center);p.LineTo(center+new Vector2(0,i==0?-r:r));p.Arc(center,r,-90+i*180,i*180);p.ClosePath();p.Fill();}};
            Paint(quarters,new NativeMenuBrush.Paint {Ellipse=true,Colors=new[]{C("#ffb84d2e"),C("#ffb84d08"),Color.clear},Stops=new[]{0f,.52f,.53f}});
            var hole=Box(icon,"set-detail-icon-hole");hole.style.width=hole.style.height=34;hole.style.alignItems=Align.Center;hole.style.justifyContent=Justify.Center;Paint(hole,new NativeMenuBrush.Paint {Ellipse=true,Fill=C("#07111d")});Text(hole,"set-detail-count",state.Owned+"/"+state.Total,9,"#ffcf7a","900");
            var heading=Box(header,"set-detail-title-group");heading.style.flexShrink=1;var kicker=Text(heading,"set-detail-kicker",L("НАБОР АРТЕФАКТОВ","ARTIFACT SET"),9,"#ffb84d");kicker.style.letterSpacing=18;
            var name=Text(heading,"set-detail-name",state.Name,18,"#dcecff","700",false,28d/18);name.style.overflow=Overflow.Hidden;name.style.textOverflow=TextOverflow.Ellipsis;
            var effect=Box(_shell,"set-detail-effect");effect.style.marginTop=16;Pad(effect,12);Border(effect,6,1);Paint(effect,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=C("#ffb84d0d"),BorderColor=C("#ffb84d33")});var label=Text(effect,"set-detail-effect-label",L("ФИНАЛЬНЫЙ ЭФФЕКТ","COMPLETION EFFECT"),9,"#ffb84d","700");label.style.letterSpacing=5;Text(effect,"set-detail-description",state.Description,14,"#cfe7f3","400",true,20d/14).style.marginTop=4;
            var items=Box(_shell,"set-detail-items");items.style.marginTop=16;var itemLabel=Text(items,"set-detail-items-label",L("АРТЕФАКТЫ НАБОРА","SET ARTIFACTS"),9,"#64849b","700");itemLabel.style.marginBottom=8;itemLabel.style.letterSpacing=5;
            for(int i=0;i<state.Items.Count;i++){var item=state.Items[i];var row=Box(items,"set-detail-artifact-"+i,true);row.style.alignItems=Align.Center;row.style.paddingLeft=row.style.paddingRight=8;row.style.paddingTop=row.style.paddingBottom=6;if(i>0)row.style.marginTop=6;row.style.opacity=item.Owned?1:.6f;Border(row,6,1);Paint(row,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=item.Owned?C("#ffb84d0a"):new Color(0,0,0,.10f),BorderColor=item.Owned?C("#ffb84d40"):C("#4ca5d629")});var slot=Box(row,"set-detail-glyph");slot.style.marginRight=8;var glyph=new NativeArtifactGlyph(slot,28,16);glyph.Bind(item.Artifact.Glyph,item.Artifact.GlyphColor);_glyphs.Add(glyph);var text=Text(row,"set-detail-artifact-name",item.Owned?item.Artifact.Name:"???",10,"#dcecff","700",true);text.style.flexGrow=1;text.style.flexShrink=1;Text(row,"set-detail-owned",item.Owned?L("ЕСТЬ","OWNED"):L("НЕТ","MISSING"),8,"#64849b");}
            var button=new Button(close){name="set-detail-close"};button.style.marginLeft=button.style.marginRight=button.style.marginBottom=0;button.style.marginTop=16;button.style.paddingTop=button.style.paddingBottom=8;button.style.paddingLeft=button.style.paddingRight=0;button.style.backgroundColor=Color.clear;Border(button,5,1);_shell.Add(button);Paint(button,new NativeMenuBrush.Paint {Radius=5,Border=1,Fill=C("#06111dc7"),BorderColor=C("#4ca5d629")});var closeLabel=Text(button,"set-detail-close-copy",L("Закрыть","Close").ToUpperInvariant(),10,"#cfe7f3","700");closeLabel.style.unityTextAlign=TextAnchor.MiddleCenter;closeLabel.style.letterSpacing=5;_backdrop=document.RegisterNativeBackdrop(button,8);
            _createdFrame=Time.frameCount;
        }
        public void Prepare(float scale,float viewportWidth,float viewportHeight)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeArtifactSetDetailView));Root.BringToFront();_shell.style.width=Mathf.Min(1040,viewportWidth-28);_shell.style.maxHeight=Mathf.Min(viewportHeight*.92f,780);
            if(_shell.layout.width<=0||Time.frameCount-_createdFrame<2)return;foreach(var paint in _paint)paint.Prepare(scale);foreach(var glyph in _glyphs)glyph.Prepare(scale);Ready=true;Root.style.opacity=1;
        }
        private void Paint(VisualElement e,NativeMenuBrush.Paint paint)=>_paint.Add(new NativeHudSurface(e,paint));
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var label=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);label.text=value;return label;}
        private static void Pad(VisualElement e,float p){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=p;}
        private static void Border(VisualElement e,float radius,float width){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=width;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        public void Dispose(){if(_disposed)return;_disposed=true;_backdrop?.Dispose();foreach(var glyph in _glyphs)glyph.Dispose();foreach(var paint in _paint)paint.Dispose();Root.RemoveFromHierarchy();}
    }
}
