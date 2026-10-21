using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeUpgradeView : IDisposable
    {
        public sealed class Choice
        {
            public int Index;public string Key,Category,Name,Description,Level,Element,ElementColor,HintNames,HintDescription;
            public bool ElementCard,Locked,CanLock,Forecast;
        }
        public sealed class State { public bool Russian,ShowReroll,CanReroll;public string Title,Reroll,Signature;public IReadOnlyList<Choice> Choices; }
        public VisualElement Root {get;}
        public bool Ready {get;private set;}
        public string Signature {get;private set;}
        private readonly SourceUiDocument _document;
        private readonly Action<int> _pick,_lock;
        private readonly Action _reroll;
        private readonly VisualElement _content,_grid;
        private readonly List<VisualElement> _cards=new List<VisualElement>();
        private readonly List<NativeHudSurface> _paint=new List<NativeHudSurface>();
        private readonly List<IDisposable> _backdrops=new List<IDisposable>();
        private readonly List<Button> _buttons=new List<Button>();
        private State _state;private int _geometryFrame;private bool _disposed,_consumed;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private string L(string ru,string en)=>_state.Russian?ru:en;
        public NativeUpgradeView(VisualElement parent,SourceUiDocument document,Action<int> pick,Action<int> lockChoice,Action reroll)
        {
            _document=document;_pick=pick;_lock=lockChoice;_reroll=reroll;
            Root=Box(parent,"native-upgrade-picker");Root.style.position=Position.Absolute;Root.style.left=Root.style.right=Root.style.top=Root.style.bottom=0;Root.style.alignItems=Align.Center;Root.style.justifyContent=Justify.Center;Root.style.opacity=0;
            _content=Box(Root,"upgrade-content");_content.style.width=Length.Percent(100);_content.style.maxWidth=672;_content.style.paddingLeft=_content.style.paddingRight=24;
            _grid=Box(_content,"upgrade-grid");
        }
        public void Bind(State state)
        {
            if(Signature==state.Signature&&!_consumed)return;
            bool shown=Ready;Clear();_state=state;Signature=state.Signature;_consumed=false;Ready=false;
            var title=Text(_content,"upgrade-title",state.Title,24,"#39d8ff","700",true,32d/24);title.style.marginBottom=24;title.style.unityTextAlign=TextAnchor.MiddleCenter;title.PlaceBehind(_grid);
            foreach(var choice in state.Choices)BuildChoice(choice);
            if(state.ShowReroll){var row=Box(_content,"upgrade-reroll-row",true);row.style.marginTop=16;row.style.justifyContent=Justify.Center;
                var button=Button(row,"upgrade-reroll",()=>Consume(_reroll));button.SetEnabled(state.CanReroll);button.style.opacity=state.CanReroll?1:.3f;button.style.paddingLeft=button.style.paddingRight=16;button.style.paddingTop=button.style.paddingBottom=8;Border(button,8);
                Paint(button,new NativeMenuBrush.Paint {Radius=8,Border=1,Fill=C("#39d8ff1a"),BorderColor=C("#39d8ff66")});var label=Text(button,"upgrade-reroll-label",state.Reroll.ToUpperInvariant(),10,"#9fefff","700");label.style.letterSpacing=5;
            }
            foreach(var button in _buttons)_backdrops.Add(_document.RegisterNativeBackdrop(button,8));
            _geometryFrame=Time.frameCount;if(!shown)Root.style.opacity=0;
        }
        private void BuildChoice(Choice choice)
        {
            var card=Box(_grid,"upgrade-choice-"+choice.Index);_cards.Add(card);card.style.paddingLeft=card.style.paddingRight=card.style.paddingTop=card.style.paddingBottom=8;Border(card,12);
            Color element=choice.ElementCard?C(choice.ElementColor):Color.clear;
            Paint(card,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=choice.ElementCard?new Color(element.r,element.g,element.b,13f/255):C("#0d1726"),BorderColor=choice.ElementCard?new Color(element.r,element.g,element.b,136f/255):C("#5a8c4a4d"),Inset=choice.ElementCard?(NativeMenuBrush.Shadow?)new NativeMenuBrush.Shadow(24,Vector2.zero,new Color(element.r,element.g,element.b,11f/255)):null});
            var pick=Button(card,"pick-upgrade-"+choice.Index,()=>Consume(()=>_pick?.Invoke(choice.Index)));pick.style.width=Length.Percent(100);pick.style.paddingLeft=pick.style.paddingRight=pick.style.paddingTop=pick.style.paddingBottom=12;
            var category=Text(pick,"upgrade-category",choice.Category,10,"#5a8c4a");category.style.letterSpacing=5;category.style.marginBottom=4;
            var name=Box(pick,"upgrade-name-row",true);name.style.alignItems=Align.Center;name.style.marginBottom=8;
            if(choice.Forecast){var forecast=Badge(name,L("ПРОГНОЗ","FORECAST"),9,"#d8c8ff",C("#b68cff1a"),C("#b68cff73"));forecast.style.marginRight=8;}
            var title=Text(name,"upgrade-name",choice.Name,18,"#dcecff","700",true,28d/18);title.style.flexShrink=1;
            if(choice.ElementCard){var badge=Badge(name,choice.Element,10,choice.ElementColor,new Color(element.r,element.g,element.b,18f/255),new Color(element.r,element.g,element.b,102f/255));badge.style.marginLeft=8;}
            var description=Text(pick,"upgrade-description",choice.Description,14,"#b6c9de","400",true,20d/14);description.style.marginBottom=8;
            if(!string.IsNullOrEmpty(choice.HintNames))Hints(pick,choice);
            Text(pick,"upgrade-level",choice.Level,12,"#7f9bb8b3","400",true);
            var locked=Button(card,"lock-upgrade-"+choice.Index,()=>Consume(()=>_lock?.Invoke(choice.Index)));locked.style.position=Position.Absolute;locked.style.top=locked.style.right=8;locked.style.paddingLeft=locked.style.paddingRight=8;locked.style.paddingTop=locked.style.paddingBottom=4;Border(locked,6);locked.SetEnabled(choice.CanLock);locked.style.opacity=!choice.CanLock&&!choice.Locked?.2f:1;
            Paint(locked,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=C(choice.Locked?"#d4943d33":"#d4943d0d"),BorderColor=C(choice.Locked?"#d4943db3":"#d4943d4d")});
            var lockLabel=Text(locked,"upgrade-lock-label",choice.Locked?L("ЗАКРЕПЛЕНО","LOCKED"):L("ЗАКРЕПИТЬ","LOCK"),9,choice.Locked?"#ffd18a":"#d4943d","700");lockLabel.style.letterSpacing=5;
        }
        private VisualElement Badge(VisualElement parent,string value,float size,string color,Color fill,Color border){var box=Box(parent,"upgrade-badge");box.style.paddingLeft=box.style.paddingRight=6;box.style.paddingTop=box.style.paddingBottom=2;Border(box,6);Paint(box,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=fill,BorderColor=border});Text(box,"upgrade-badge-text",value,size,color,"700");return box;}
        private void Hints(VisualElement parent,Choice choice)
        {
            var hint=Box(parent,"upgrade-synergy-hint");hint.style.marginTop=8;hint.style.paddingLeft=hint.style.paddingRight=10;hint.style.paddingTop=hint.style.paddingBottom=8;Border(hint,8);Paint(hint,new NativeMenuBrush.Paint {Radius=8,Border=1,Fill=C("#39d8ff0d"),BorderColor=C("#39d8ff40")});
            var row=Box(hint,"synergy-heading",true);row.style.alignItems=Align.Center;
            var orbit=Box(row,"synergy-orbit");orbit.style.width=orbit.style.height=28;orbit.style.marginRight=8;orbit.style.alignItems=Align.Center;orbit.style.justifyContent=Justify.Center;Border(orbit,999);Paint(orbit,new NativeMenuBrush.Paint {Ellipse=true,Border=1,BorderColor=C("#39d8ff73"),OuterA=new NativeMenuBrush.Shadow(12,Vector2.zero,C("#39d8ff1a"))});
            var ring=Box(orbit,"synergy-orbit-ring");ring.style.position=Position.Absolute;ring.style.left=ring.style.right=ring.style.top=ring.style.bottom=4;ring.generateVisualContent+=context=>{float radius=Mathf.Min(ring.contentRect.width,ring.contentRect.height)*.5f-.5f;if(radius<=0)return;var p=context.painter2D;p.lineWidth=1;p.strokeColor=C("#39d8ff47");p.SetDashPattern(new[]{3f,3f});p.BeginPath();p.Arc(ring.contentRect.center,radius,0,360);p.ClosePath();p.Stroke();};
            var star=Box(orbit,"synergy-icon");star.style.width=star.style.height=10;star.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/Sparkles"));star.style.unityBackgroundImageTintColor=C("#39d8ff");
            var dot=Box(orbit,"synergy-orbit-sphere");dot.style.position=Position.Absolute;dot.style.width=dot.style.height=5;dot.style.top=1;dot.style.right=2;Paint(dot,new NativeMenuBrush.Paint {Ellipse=true,Fill=C("#ffcf7a"),OuterA=new NativeMenuBrush.Shadow(6,Vector2.zero,C("#ffcf7a"))});
            var copy=Box(row,"synergy-copy");copy.style.flexShrink=1;var heading=Text(copy,"synergy-label",L("БУДУЩАЯ СИНЕРГИЯ","FUTURE SYNERGY"),8,"#39d8ff","900");heading.style.letterSpacing=12;
            foreach(var value in choice.HintNames.Split('\n')){var line=Text(copy,"synergy-name",value,9,"#dcecff","700");line.style.overflow=Overflow.Hidden;line.style.textOverflow=TextOverflow.Ellipsis;}
            Text(hint,"synergy-description",choice.HintDescription,8,"#7f9bb8","400",true,1.5).style.marginTop=4;
            var footer=Text(hint,"synergy-activation",L("Активируется: Сфера VII + Способность VII","Activates: Sphere VII + Ability VII").ToUpperInvariant(),7,"#7f9bb8cc","400",true);footer.style.marginTop=4;footer.style.letterSpacing=5;
        }
        public void Prepare(float scale,float viewportWidth)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeUpgradeView));Root.BringToFront();bool columns=viewportWidth>=640;_grid.style.flexDirection=columns?FlexDirection.Row:FlexDirection.Column;
            for(int i=0;i<_cards.Count;i++){var card=_cards[i];card.style.flexBasis=columns?0:StyleKeyword.Auto;card.style.flexGrow=columns?1:0;card.style.flexShrink=1;card.style.marginRight=columns&&i<_cards.Count-1?16:0;card.style.marginBottom=!columns&&i<_cards.Count-1?16:0;}
            if(Root.layout.width<=0||_content.layout.width<=0||Time.frameCount-_geometryFrame<2)return;foreach(var paint in _paint)paint.Prepare(scale);Ready=true;Root.style.opacity=1;
        }
        private void Consume(Action action){if(_consumed)return;_consumed=true;foreach(var button in _buttons)button.SetEnabled(false);action?.Invoke();}
        private void Paint(VisualElement e,NativeMenuBrush.Paint paint)=>_paint.Add(new NativeHudSurface(e,paint));
        private Button Button(VisualElement parent,string name,Action action){var button=new Button(action){name=name};button.style.marginLeft=button.style.marginRight=button.style.marginTop=button.style.marginBottom=0;button.style.paddingLeft=button.style.paddingRight=button.style.paddingTop=button.style.paddingBottom=0;button.style.borderLeftWidth=button.style.borderRightWidth=button.style.borderTopWidth=button.style.borderBottomWidth=0;button.style.backgroundColor=Color.clear;parent.Add(button);_buttons.Add(button);return button;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var label=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);label.text=value;return label;}
        private static void Border(VisualElement e,float radius){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        private void Clear(){foreach(var backdrop in _backdrops)backdrop.Dispose();_backdrops.Clear();foreach(var paint in _paint)paint.Dispose();_paint.Clear();_buttons.Clear();_cards.Clear();_grid.Clear();for(int i=_content.hierarchy.childCount-1;i>=0;i--)if(_content.hierarchy[i]!=_grid)_content.hierarchy.RemoveAt(i);}
        public void Dispose(){if(_disposed)return;_disposed=true;Clear();Root.RemoveFromHierarchy();}
    }
}
