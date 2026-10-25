using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // EchoMapScreen.tsx, with native layout, scrolling, vector icons and input.
    public sealed class NativeRegionScreen : MonoBehaviour
    {
        public sealed class Challenge { public string Id,Name,Description,Accent; public bool Completed; }
        public sealed class Region { public string Id,Name,Subtitle,Accent; public bool Available,Stable,Endless; public IReadOnlyList<Challenge> Challenges; public int Completed; }
        public sealed class State { public bool Russian; public IReadOnlyList<Region> Regions; }
        private NativePanelLease _panelLease;
        private UIDocument _document;
        private PanelSettings _panel;
        private VisualElement _root,_header,_content;
        private ScrollView _scroll;
        private NativeMainMenuBackground _background;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly List<NativeMenuGlass> _glass=new List<NativeMenuGlass>();
        private readonly List<VisualElement> _cards=new List<VisualElement>();
        private readonly List<Label> _titles=new List<Label>();
        private readonly List<Button> _buttons=new List<Button>();
        private State _state;private Action _back;private Action<string,string,string> _start;
        private bool _visible,_narrow;private int _geometryFrame;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        private static Color Alpha(Color c,float a)=>new Color(c.r,c.g,c.b,a);
        private string L(string ru,string en)=>_state.Russian?ru:en;
        private void Ensure()
        {
            if(_document!=null)return;
            _panelLease=new NativePanelLease(gameObject,"Native region selection panel",1001,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeRegions")??throw new InvalidOperationException("Native screen UXML is missing: NativeRegions"));
            _document=_panelLease.Document;_panel=_panelLease.Panel;_root=_document.rootVisualElement;Fill(_root);_root.pickingMode=PickingMode.Ignore;_root.name="native-region-root";
            _root.styleSheets.Add(Resources.Load<StyleSheet>("EchoSphere/NativeProgression"));
            _background=new NativeMainMenuBackground(_root,"EchoSphere/NativeRegionBackground");
            _header=_root.Q<VisualElement>("region-header");_scroll=_root.Q<ScrollView>("region-scroll");
            _scroll.contentContainer.style.alignItems=Align.Center;_content=_root.Q<VisualElement>("region-content");
            _content.RegisterCallback<GeometryChangedEvent>(_=>_geometryFrame=Time.frameCount);
        }
        public void Show(State state,float scale,Action back,Action<string,string,string> start)
        {
            Ensure();_back=back;_start=start;_panel.scale=scale;_visible=true;_root.style.display=DisplayStyle.Flex;NativeMenuRetainedState.Visible(_root,true);
            if(!NativeMenuRetainedState.Same(_state,state)){Clear();_state=state;Build();foreach(var button in _buttons)_glass.Add(new NativeMenuGlass(button,8));_geometryFrame=Time.frameCount;_content.style.opacity=0;}
            bool narrow=Screen.width/scale<=560;
            if(_narrow!=narrow){_narrow=narrow;foreach(var card in _cards)Pad(card,narrow?10:12);foreach(var title in _titles)title.style.fontSize=narrow?12:15;_geometryFrame=Time.frameCount;}
            var safe=Screen.safeArea;_root.style.paddingTop=(Screen.height-safe.yMax)/scale;_root.style.paddingBottom=safe.yMin/scale;
            _scroll.style.paddingLeft=_scroll.style.paddingRight=narrow?9:12;_scroll.style.paddingTop=narrow?12:16;_scroll.style.paddingBottom=narrow?16:18;
            _background.SetViewport(Screen.width/scale,Screen.height/scale,scale);
            if(Time.frameCount-_geometryFrame>=2){foreach(var surface in _surfaces)surface.Prepare(scale);foreach(var glass in _glass)glass.Prepare(_root,8,scale);_content.style.opacity=1;}
        }
        private void Build()
        {
            var heading=Box(_header,"region-heading");
            Text(heading,"region-title",L("РЕГИОНЫ ЭХА","ECHO REGIONS"),13,"#e7faff","900",.20f).style.unityTextAlign=TextAnchor.MiddleCenter;
            var subtitle=Text(heading,"region-subtitle",L("СТАБИЛИЗИРУЙ · ИСПЫТАЙ · УГЛУБИСЬ","STABILIZE · CHALLENGE · DESCEND"),6,"#527187","400",.16f);subtitle.style.marginTop=3;subtitle.style.unityTextAlign=TextAnchor.MiddleCenter;
            var back=Button(_header,"region-back",()=>_back?.Invoke());back.style.position=Position.Absolute;back.style.left=12;back.style.top=10;back.style.minHeight=34;back.style.paddingLeft=back.style.paddingRight=10;
            Border(back,5);Paint(back,new NativeMenuBrush.Paint {Radius=5,Border=1,Fill=A(3,10,18,.84f),BorderColor=A(90,194,241,.17f)});
            Text(back,"region-back-chevron","‹",18,"#a1e7ff");Text(back,"region-back-copy",L("В МЕНЮ","MENU"),7,"#7895a9","900",.13f).style.marginLeft=5;
            var intro=Box(_content,"region-intro");intro.style.marginBottom=10;intro.style.paddingLeft=intro.style.paddingRight=2;
            Text(intro,"region-select-label",L("ВЫБОР ОБЛАСТИ","REGION SELECT"),7,"#537388","900",.18f);
            Text(intro,"region-select-intro",L("Один регион · несколько способов пройти его.","One region · several ways to run it."),10,"#cfe7f4","700").style.marginTop=3;
            for(int i=0;i<_state.Regions.Count;i++)BuildRegion(_state.Regions[i],i);
            var foot=Box(_content,"region-foot",true);foot.style.alignItems=Align.Center;foot.style.marginTop=1;
            Icon(foot,"Target",13,C("#48687d")).style.marginRight=7;
            Text(foot,"region-foot-copy",L("Каждая область меняет ритм врагов, события и давление на Сеть.","Each region changes enemy rhythm, events and Network pressure."),6.5f,"#48687d","400",.012f,true);
        }
        private void BuildRegion(Region region,int index)
        {
            Color accent=C(region.Accent);var card=Box(_content,"region-"+region.Id);_cards.Add(card);Pad(card,_narrow?10:12);Border(card,9);card.style.marginBottom=10;card.style.opacity=region.Available?1:.52f;
            Paint(card,new NativeMenuBrush.Paint {Radius=9,Border=1,BorderColor=Alpha(accent,.20f),Linear=true,Colors=new[]{A(8,17,29,.94f),A(4,10,18,.96f)},Stops=new[]{0f,1f},Inset=new NativeMenuBrush.Shadow(28,Vector2.zero,A(65,165,208,.025f))});
            var header=Box(card,"region-card-header",true);header.style.alignItems=Align.FlexStart;
            var number=Text(header,"region-index","0"+(index+1),18,"#ffffff","900");number.style.color=Color.Lerp(accent,Color.white,.28f);number.style.width=30;number.style.height=18;number.style.marginRight=10;
            var copy=Box(header,"region-card-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;copy.style.marginRight=10;
            Text(copy,"region-kicker",L("РЕГИОН ","REGION ")+(index+1).ToString("00"),6,"#49697e","900",.18f);
            var title=Text(copy,"region-name",region.Name,_narrow?12:15,"#e8f8ff","900",.10f,true);title.style.marginTop=3;_titles.Add(title);
            Text(copy,"region-description",region.Subtitle,7,"#7795aa","400",.012f,true).style.marginTop=3;
            if(region.Available)Text(header,"region-status",region.Stable?L("СТАБИЛЕН","STABLE"):L("ДОСТУПЕН","AVAILABLE"),6,region.Stable?"#70e7c7":"#67869a","900",.12f);
            else Icon(header,"LockKeyhole",15,C("#67869a"));
            var rule=Box(card,"region-rule");rule.style.height=1;rule.style.marginTop=rule.style.marginBottom=10;rule.style.backgroundColor=A(100,190,230,.08f);
            if(!region.Available){var locked=Box(card,"region-locked-message",true);locked.style.alignItems=Align.Center;locked.style.minHeight=54;locked.style.paddingLeft=locked.style.paddingRight=4;Icon(locked,"LockKeyhole",15,C("#688296")).style.marginRight=9;Text(locked,"region-lock-copy",L("Откроется после стабилизации Резонансного Бассейна.","Unlocks after Resonance Basin is stabilized."),8,"#688296","400",.012f,true).style.flexShrink=1;return;}
            var run=Button(card,"start-"+region.Id,()=>_start?.Invoke(region.Id,"stabilization","none"));Pad(run,10);Border(run,7);
            Paint(run,new NativeMenuBrush.Paint {Radius=7,Border=1,BorderColor=Alpha(accent,.36f),Linear=true,Colors=new[]{A(27,88,109,.30f),A(9,31,45,.52f)},Stops=new[]{0f,1f}});
            var runIcon=IconFrame(run,"Swords",17,accent,Alpha(accent,.32f),A(2,8,15,.72f));runIcon.style.marginRight=10;
            var runCopy=Box(run,"run-copy");runCopy.style.flexGrow=1;runCopy.style.flexShrink=1;runCopy.style.marginRight=10;
            Text(runCopy,"run-title",region.Stable?L("ПОВТОРИТЬ СТАБИЛИЗАЦИЮ","REPLAY STABILIZATION"):L("СТАБИЛИЗАЦИЯ · 30:00","STABILIZATION · 30:00"),9,"#e8fbff","700",.06f);
            Text(runCopy,"run-description",L("Базовый маршрут · открывает историю региона","Base route · opens the region story"),7,"#7593a7").style.marginTop=2;Icon(run,"ChevronRight",18,C("#e8fbff"));
            var section=Box(card,"region-challenge-heading",true);section.style.justifyContent=Justify.SpaceBetween;section.style.marginTop=11;section.style.marginBottom=6;section.style.marginLeft=section.style.marginRight=2;
            Text(section,"challenges-label",L("ИСПЫТАНИЯ","CHALLENGES"),7,"#648299","900",.16f);Text(section,"challenges-progress",region.Completed+"/3",7,"#cfe8f4","900",.16f);
            for(int i=0;i<region.Challenges.Count;i++)BuildChallenge(card,region,region.Challenges[i],i);
            var endless=Button(card,"endless-"+region.Id,()=>{if(region.Endless)_start?.Invoke(region.Id,"endless","none");});endless.SetEnabled(region.Endless);endless.style.opacity=region.Endless?1:.45f;endless.style.marginTop=9;endless.style.paddingLeft=endless.style.paddingRight=10;endless.style.paddingTop=endless.style.paddingBottom=9;Border(endless,7);
            Paint(endless,new NativeMenuBrush.Paint {Radius=7,Border=1,BorderColor=A(255,194,103,region.Endless?.42f:.22f),Fill=region.Endless?A(52,39,18,.30f):A(20,17,10,.30f)});
            IconFrame(endless,"Infinity",20,C("#ffd47c"),A(255,205,120,.30f),A(17,11,5,.46f)).style.marginRight=9;
            var endlessCopy=Box(endless,"endless-copy");endlessCopy.style.flexGrow=1;endlessCopy.style.flexShrink=1;endlessCopy.style.marginRight=9;
            Text(endlessCopy,"endless-title",L("БЕСКОНЕЧНОСТЬ","ENDLESS"),9,"#eee5d1","700",.09f);
            Text(endlessCopy,"endless-description",region.Endless?L("Все три испытания пройдены · вход открыт","All three challenges cleared · access open"):L("Закрыто · испытания ","Locked · challenges ")+region.Completed+"/3",6.5f,"#9b8662","400",.012f,true).style.marginTop=2;Icon(endless,region.Endless?"ChevronRight":"LockKeyhole",region.Endless?18:15,C("#eee5d1"));
        }
        private void BuildChallenge(VisualElement parent,Region region,Challenge challenge,int index)
        {
            Color accent=C(challenge.Accent);var button=Button(parent,"challenge-"+challenge.Id,()=>{if(region.Stable)_start?.Invoke(region.Id,"stabilization",challenge.Id);});button.SetEnabled(region.Stable);button.style.opacity=region.Stable?1:.48f;button.style.paddingLeft=button.style.paddingRight=9;button.style.paddingTop=button.style.paddingBottom=8;if(index>0)button.style.marginTop=5;Border(button,6);
            Paint(button,new NativeMenuBrush.Paint {Radius=6,Border=1,BorderColor=challenge.Completed?Alpha(accent,.34f):A(87,161,204,.12f),Fill=challenge.Completed?Color.Lerp(C("#06101b"),accent,.07f):A(7,16,28,.78f)});
            var mark=Box(button,"challenge-mark");mark.style.width=6;mark.style.height=22;mark.style.backgroundColor=accent;mark.style.opacity=.72f;mark.style.marginRight=8;
            var copy=Box(button,"challenge-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;copy.style.marginRight=8;
            Text(copy,"challenge-name",challenge.Name,8,"#d7e9f4","700");Text(copy,"challenge-description",challenge.Description,6.5f,"#718da2","400",.012f,true).style.marginTop=2;
            if(challenge.Completed)Text(button,"challenge-state",L("ПРОЙДЕНО","CLEARED"),6,challenge.Accent,"900",.06f);else Icon(button,region.Stable?"ChevronRight":"LockKeyhole",region.Stable?15:14,accent);
        }
        private VisualElement IconFrame(VisualElement parent,string icon,float size,Color color,Color border,Color fill){var box=Box(parent,"icon-frame");box.style.width=box.style.height=32;box.style.alignItems=Align.Center;box.style.justifyContent=Justify.Center;Border(box,6);Paint(box,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=fill,BorderColor=border});Icon(box,icon,size,color);return box;}
        private void Paint(VisualElement node,NativeMenuBrush.Paint paint)=>_surfaces.Add(new NativeHudSurface(node,paint));
        private Button Button(VisualElement parent,string name,Action action){var b=new Button(action){name=name};b.AddToClassList("progression-button");b.style.flexDirection=FlexDirection.Row;b.style.alignItems=Align.Center;parent.Add(b);_buttons.Add(b);return b;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",float em=.012f,bool wrap=false){var l=NativePauseStatsView.Text(parent,name,size,color,weight,1.5,em*size,wrap);l.text=value;return l;}
        private static VisualElement Icon(VisualElement parent,string icon,float size,Color color){var e=Box(parent,"icon-"+icon);e.style.width=e.style.height=size;e.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+icon));e.style.unityBackgroundImageTintColor=color;return e;}
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static void Pad(VisualElement e,float p){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=p;}
        private static void Border(VisualElement e,float radius){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        private void Clear(){foreach(var glass in _glass)glass.Dispose();_glass.Clear();foreach(var surface in _surfaces)surface.Dispose();_surfaces.Clear();_cards.Clear();_titles.Clear();_buttons.Clear();_header.Clear();_content.Clear();}
        public void Hide(){if(!_visible)return;_visible=false;NativeMenuRetainedState.Visible(_root,false);}
        private void OnDestroy(){if(_document!=null)Clear();_background?.Dispose();_panelLease?.Dispose();}
    }
}
