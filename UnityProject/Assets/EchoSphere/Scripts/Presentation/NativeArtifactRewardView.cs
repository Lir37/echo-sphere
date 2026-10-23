using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeArtifactRewardView : IDisposable
    {
        public sealed class Choice { public NativePauseArtifactsState.Artifact Artifact;public string Mechanic;public IReadOnlyList<string> Synergies; }
        public sealed class State { public bool Russian;public string Signature;public IReadOnlyList<Choice> Choices; }
        public VisualElement Root {get;}
        public bool Ready {get;private set;}
        public string Signature {get;private set;}
        private readonly INativeUiBackdrops _document;
        private readonly Action<string> _pick,_selectSet;
        private readonly VisualElement _content,_grid;
        private readonly List<VisualElement> _cards=new List<VisualElement>();
        private readonly List<NativeHudSurface> _paint=new List<NativeHudSurface>();
        private readonly List<NativeArtifactGlyph> _glyphs=new List<NativeArtifactGlyph>();
        private readonly List<NativeArtifactSetBadge> _badges=new List<NativeArtifactSetBadge>();
        private readonly List<IDisposable> _backdrops=new List<IDisposable>();
        private readonly List<Button> _buttons=new List<Button>();
        private State _state;private bool _consumed,_disposed;private int _geometryFrame;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private string L(string ru,string en)=>_state.Russian?ru:en;
        public NativeArtifactRewardView(VisualElement parent,INativeUiBackdrops document,Action<string> pick,Action<string> selectSet)
        {
            _document=document;_pick=pick;_selectSet=selectSet;Root=Box(parent,"native-artifact-rewards");Root.style.position=Position.Absolute;Root.style.left=Root.style.right=Root.style.top=Root.style.bottom=0;Root.style.alignItems=Align.Center;Root.style.justifyContent=Justify.Center;Root.style.opacity=0;Root.styleSheets.Add(Resources.Load<StyleSheet>("EchoSphere/NativeProgression"));
            _content=Box(Root,"artifact-reward-content");_content.style.width=Length.Percent(100);_content.style.maxWidth=1024;_content.style.paddingLeft=_content.style.paddingRight=24;_grid=Box(_content,"artifact-reward-grid");
        }
        public void Bind(State state)
        {
            if(Signature==state.Signature&&!_consumed)return;bool shown=Ready;Clear();_state=state;Signature=state.Signature;Ready=false;_consumed=false;
            var title=Text(_content,"artifact-reward-title",L("Артефакт","Artifact"),24,"#dcecff","700",true,32d/24);title.style.unityTextAlign=TextAnchor.MiddleCenter;title.style.marginBottom=8;title.style.letterSpacing=8;title.style.textShadow=new TextShadow {color=C("#48d3ff2e"),blurRadius=20,offset=Vector2.zero};title.PlaceBehind(_grid);
            var subtitle=Text(_content,"artifact-reward-subtitle",L("Артефакты меняют правила взаимодействия сфер и персонажа","Artifacts change the rules of how spheres and the player interact"),14,"#7f9bb8","400",true,20d/14);subtitle.style.unityTextAlign=TextAnchor.MiddleCenter;subtitle.style.marginBottom=24;subtitle.PlaceBehind(_grid);
            foreach(var choice in state.Choices)Build(choice);
            foreach(var button in _buttons)_backdrops.Add(_document.RegisterNativeBackdrop(button,8));foreach(var badge in _badges)_backdrops.Add(_document.RegisterNativeBackdrop(badge.Root,8));
            _geometryFrame=Time.frameCount;if(!shown)Root.style.opacity=0;
        }
        private static NativeMenuBrush.Paint CardPaint(bool hover)=>new NativeMenuBrush.Paint {Radius=7,Border=2,BorderColor=hover?C("#5dddff85"):C("#52b8ea33"),LinearAngle=150,Colors=new[]{C("#0c1d2df5"),C("#040b13fa")},Stops=new[]{0f,1f},Inset=new NativeMenuBrush.Shadow(24,Vector2.zero,hover?C("#41a6d70f"):C("#41a6d709")),OuterA=hover?(NativeMenuBrush.Shadow?)new NativeMenuBrush.Shadow(24,Vector2.zero,C("#37caff17")):null};
        private void Build(Choice choice)
        {
            var artifact=choice.Artifact;var button=new Button(()=>{if(_consumed)return;_consumed=true;foreach(var b in _buttons)b.SetEnabled(false);_pick?.Invoke(artifact.Id);}){name="artifact-reward-"+artifact.Id};button.AddToClassList("progression-button");button.AddToClassList("native-artifact-reward-card");_grid.Add(button);_cards.Add(button);_buttons.Add(button);Pad(button,16);Border(button,7,2);
            var normal=CardPaint(false);var hover=CardPaint(true);var paint=Paint(button,normal);button.RegisterCallback<PointerEnterEvent>(_=>{paint.SetPaint(hover);button.style.scale=new Scale(new Vector3(1.02f,1.02f,1));});button.RegisterCallback<PointerLeaveEvent>(_=>{paint.SetPaint(normal);button.style.scale=new Scale(Vector3.one);});
            var row=Box(button,"artifact-reward-main",true);row.style.alignItems=Align.FlexStart;var slot=Box(row,"artifact-reward-glyph");slot.style.marginRight=12;var glyph=new NativeArtifactGlyph(slot,74,16);glyph.Bind(artifact.Glyph,artifact.GlyphColor);_glyphs.Add(glyph);
            var body=Box(row,"artifact-reward-body");body.style.flexGrow=1;body.style.flexShrink=1;var heading=Box(body,"artifact-reward-rarity-row",true);heading.style.justifyContent=Justify.SpaceBetween;heading.style.alignItems=Align.Center;
            string rarity=artifact.Rarity=="common"?"#7f9bb8":artifact.BorderColor;var rarityLabel=Text(heading,"artifact-reward-rarity",artifact.RarityLabel.ToUpperInvariant(),9,rarity,"700");rarityLabel.style.letterSpacing=10;Text(heading,"artifact-reward-star","✦",16,"#ffb84d");Text(body,"artifact-reward-name",artifact.Name,18,"#dcecff","700",true,28d/18).style.marginBottom=4;Text(body,"artifact-reward-description",artifact.Description,10,"#b6c9de","400",true);
            var paths=Box(button,"artifact-reward-paths");paths.style.marginTop=12;paths.style.paddingTop=8;paths.style.borderTopWidth=1;paths.style.borderTopColor=C("#243b55");var pathsLabel=Text(paths,"artifact-reward-paths-label",L("Связанные сборки","Build paths").ToUpperInvariant(),8,"#7f9bb8");pathsLabel.style.marginBottom=6;pathsLabel.style.letterSpacing=5;
            var badges=Box(paths,"artifact-reward-sets",true);badges.style.flexWrap=Wrap.Wrap;badges.style.marginRight=badges.style.marginBottom=-8;
            foreach(var set in artifact.Sets){var badge=new NativeArtifactSetBadge(badges,id=>_selectSet?.Invoke(id));badge.Bind(set);badge.Root.style.marginRight=badge.Root.style.marginBottom=8;_badges.Add(badge);}
            foreach(var synergy in choice.Synergies){var box=Box(button,"artifact-reward-synergy");box.style.marginTop=12;box.style.paddingLeft=box.style.paddingRight=8;box.style.paddingTop=box.style.paddingBottom=6;Border(box,8,1);Paint(box,new NativeMenuBrush.Paint {Radius=8,Border=1,Fill=C("#8064a81a"),BorderColor=C("#8064a84d")});var label=Text(box,"artifact-reward-synergy-label",L("Активирует синергию","Activates synergy").ToUpperInvariant(),9,"#8064a8","700");label.style.letterSpacing=5;Text(box,"artifact-reward-synergy-name",synergy,12,"#8064a8","700",true);}
            if(!string.IsNullOrEmpty(choice.Mechanic)){var tag=Box(button,"artifact-reward-mechanic");tag.style.alignSelf=Align.FlexStart;tag.style.marginTop=12;tag.style.paddingLeft=tag.style.paddingRight=8;tag.style.paddingTop=tag.style.paddingBottom=4;Paint(tag,new NativeMenuBrush.Paint {Radius=6,Fill=new Color(0,0,0,.05f)});var label=Text(tag,"artifact-reward-mechanic-copy",choice.Mechanic.ToUpperInvariant(),10,rarity,"700");label.style.letterSpacing=5;}
        }
        public void Prepare(float scale,float viewportWidth)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeArtifactRewardView));Root.BringToFront();bool columns=viewportWidth>=640;_grid.style.flexDirection=columns?FlexDirection.Row:FlexDirection.Column;
            for(int i=0;i<_cards.Count;i++){var card=_cards[i];card.style.flexBasis=columns?0:StyleKeyword.Auto;card.style.flexGrow=columns?1:0;card.style.flexShrink=1;card.style.marginRight=columns&&i<_cards.Count-1?16:0;card.style.marginBottom=!columns&&i<_cards.Count-1?16:0;}
            if(_content.layout.width<=0||Time.frameCount-_geometryFrame<2)return;foreach(var paint in _paint)paint.Prepare(scale);foreach(var glyph in _glyphs)glyph.Prepare(scale);Ready=true;Root.style.opacity=1;
        }
        private NativeHudSurface Paint(VisualElement e,NativeMenuBrush.Paint paint){var surface=new NativeHudSurface(e,paint);_paint.Add(surface);return surface;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var label=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);label.text=value;return label;}
        private static void Pad(VisualElement e,float p){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=p;}
        private static void Border(VisualElement e,float radius,float width){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=width;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        private void Clear(){foreach(var backdrop in _backdrops)backdrop.Dispose();_backdrops.Clear();foreach(var badge in _badges)badge.Dispose();_badges.Clear();foreach(var glyph in _glyphs)glyph.Dispose();_glyphs.Clear();foreach(var paint in _paint)paint.Dispose();_paint.Clear();_buttons.Clear();_cards.Clear();_grid.Clear();for(int i=_content.hierarchy.childCount-1;i>=0;i--)if(_content.hierarchy[i]!=_grid)_content.hierarchy.RemoveAt(i);}
        public void Dispose(){if(_disposed)return;_disposed=true;Clear();Root.RemoveFromHierarchy();}
    }
}
