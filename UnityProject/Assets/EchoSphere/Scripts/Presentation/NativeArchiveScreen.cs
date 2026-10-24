using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeArchiveScreen : MonoBehaviour
    {
        public sealed class Card { public string Id,Group,Title,Description,Details;public bool Open;public int BasicIndex=-1; }
        public sealed class State { public bool Russian;public string Section,Query;public int Known;public IReadOnlyList<Card> Cards; }
        private static readonly string[] Sections={"basics","spheres","monsters","bosses","abilities","artifacts","evolutions"};
        private static readonly string[] Icons={"BookOpen","CircleDot","Swords","Shield","Zap","Gem","Network"};
        private static readonly string[] LabelsRu={"Ядро игры","Сферы","Монстры","Боссы","Способности","Артефакты","Эволюции"};
        private static readonly string[] LabelsEn={"Core","Spheres","Monsters","Bosses","Abilities","Artifacts","Evolutions"};
        private static readonly string[] SubRu={"Как работает забег","Узлы боевой сети","Противники","Главные угрозы","Активные и пассивные","Редкие усиления","Ветки и финалы"};
        private static readonly string[] SubEn={"How a run works","Combat network nodes","Enemies","Major threats","Active and passive","Rare upgrades","Branches and finals"};
        private UIDocument _document;private PanelSettings _panel;private NativeMainMenuBackground _background;
        private VisualElement _root,_page,_header,_layout,_nav,_main,_searchBox,_cards,_known;
        private ScrollView _navigation,_scroll;private TextField _search;private Label _count,_title;
        private readonly List<NativeHudSurface> _chrome=new List<NativeHudSurface>(),_cardPaint=new List<NativeHudSurface>();
        private readonly List<NativeMenuGlass> _glass=new List<NativeMenuGlass>();
        private readonly List<VisualElement> _columns=new List<VisualElement>(),_conceptColumns=new List<VisualElement>();
        private readonly List<Label> _subtitles=new List<Label>();
        private readonly List<Button> _tabs=new List<Button>();
        private readonly List<Button> _buttons=new List<Button>();
        private readonly List<Label> _tabLabels=new List<Label>();
        private State _state;private Action _back;private Action<string> _select,_query;
        private bool _visible;private int _geometryFrame;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private string L(string ru,string en)=>_state.Russian?ru:en;
        private void Ensure()
        {
            if(_document!=null)return;
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Native archive panel";_panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1001;_panel.forceGammaRendering=true;_panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;_root=_document.rootVisualElement;Fill(_root);_root.style.alignItems=Align.Center;_root.pickingMode=PickingMode.Ignore;_root.styleSheets.Add(Resources.Load<StyleSheet>("EchoSphere/NativeProgression"));
            _background=new NativeMainMenuBackground(_root,"EchoSphere/NativeAppBackground");
            _page=Box(_root,"native-archive-page");_page.style.width=Length.Percent(100);_page.style.maxWidth=820;_page.style.height=Length.Percent(100);_page.RegisterCallback<GeometryChangedEvent>(_=>_geometryFrame=Time.frameCount);
        }
        public void Show(State state,float scale,Action back,Action<string> select,Action<string> query)
        {
            Ensure();_back=back;_select=select;_query=query;_panel.scale=scale;_root.style.display=DisplayStyle.Flex;NativeMenuRetainedState.Visible(_root,true);
            if(_state!=null&&_state.Russian!=state.Russian){Clear();_state=null;}
            bool opening=_state==null;_visible=true;bool changed=!NativeMenuRetainedState.Same(_state,state);_state=state;
            if(opening){BuildChrome();_geometryFrame=Time.frameCount;_page.style.opacity=0;}
            if(changed){_search.SetValueWithoutNotify(state.Query);_searchBox.style.display=state.Section=="basics"?DisplayStyle.None:DisplayStyle.Flex;BindCards();_geometryFrame=Time.frameCount;_cards.style.opacity=0;_count.text=state.Known.ToString();for(int i=0;i<_tabs.Count;i++){var color=C(Sections[i]==state.Section?"#cffafe":"#64748b");_tabLabels[i].style.color=color;}}
            float width=Screen.width/scale;bool sm=width>=640,md=width>=768;Pad(_page,sm?20:12);
            _page.style.paddingTop=Mathf.Max(sm?20:12,(Screen.height-Screen.safeArea.yMax)/scale);_page.style.paddingBottom=Mathf.Max(sm?20:12,Screen.safeArea.yMin/scale);
            _title.style.fontSize=sm?24:20;_known.style.display=sm?DisplayStyle.Flex:DisplayStyle.None;
            _layout.style.flexDirection=md?FlexDirection.Row:FlexDirection.Column;
            _navigation.style.width=md?224:StyleKeyword.Auto;_navigation.style.marginRight=md?12:0;_navigation.style.marginBottom=md?0:12;
            _navigation.style.maxHeight=md?StyleKeyword.None:64;
            _nav.style.flexDirection=md?FlexDirection.Column:FlexDirection.Row;foreach(var tab in _tabs){tab.style.marginRight=md?0:8;tab.style.marginBottom=md?8:0;}
            foreach(var subtitle in _subtitles)subtitle.style.display=md?DisplayStyle.Flex:DisplayStyle.None;
            foreach(var column in _columns)column.style.width=Length.Percent(sm?50:100);foreach(var column in _conceptColumns)column.style.width=Length.Percent(sm?50:100);
            Pad(_main,sm?20:12);_background.SetViewport(width,Screen.height/scale,scale);
            if(Time.frameCount-_geometryFrame>=2){foreach(var p in _chrome)p.Prepare(scale);foreach(var p in _cardPaint)p.Prepare(scale);foreach(var glass in _glass)glass.Prepare(_root,8,scale);_page.style.opacity=1;_cards.style.opacity=1;}
        }
        private void BuildChrome()
        {
            Paint(_page,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,.15f),Colors=new[]{new Color(62f/255,145f/255,201f/255,.10f),Color.clear},Stops=new[]{0f,.35f}},false);
            Paint(_page,new NativeMenuBrush.Paint {Linear=true,Colors=new[]{new Color(2f/255,7f/255,14f/255,.98f),new Color(3f/255,8f/255,15f/255,.98f)},Stops=new[]{0f,1f}},false);
            _header=Box(_page,"archive-header",true);_header.style.alignItems=Align.Center;_header.style.paddingBottom=12;_header.style.borderBottomWidth=1;_header.style.borderBottomColor=C("#67e8f91a");
            var back=Button(_header,"archive-back",()=>_back?.Invoke());back.style.width=back.style.height=36;back.style.alignItems=Align.Center;back.style.justifyContent=Justify.Center;back.style.marginRight=12;Surface(back,false);Icon(back,"ArrowLeft",17,C("#dcefff"));
            var copy=Box(_header,"archive-heading");copy.style.flexGrow=1;copy.style.flexShrink=1;
            var kicker=Text(copy,"archive-kicker",L("ЭХО СФЕРА // АРХИВ","ECHO SPHERE // ARCHIVE"),9,"#67e8f9b3","700");kicker.style.letterSpacing=28;
            _title=Text(copy,"archive-title",L("Архив Эха","Echo Archive"),24,"#f1f5f9","600");_title.style.marginTop=4;_title.style.letterSpacing=2.5f;
            Text(copy,"archive-subtitle",L("База знаний, которая открывается вместе с твоим прогрессом.","A knowledge base that grows with your progress."),12,"#64748b","400",true);
            _known=Box(_header,"archive-known");_known.style.paddingLeft=_known.style.paddingRight=12;_known.style.paddingTop=_known.style.paddingBottom=8;_known.style.marginLeft=12;Surface(_known,false);Text(_known,"archive-known-label",L("ОТКРЫТО ЗАПИСЕЙ","ENTRIES UNLOCKED"),8,"#64748b");_count=Text(_known,"archive-known-count","",14,"#a5f3fc","700");_count.style.unityTextAlign=TextAnchor.MiddleRight;
            _layout=Box(_page,"archive-layout");_layout.style.flexGrow=1;_layout.style.flexShrink=1;_layout.style.minHeight=0;_layout.style.marginTop=12;
            _navigation=new ScrollView(ScrollViewMode.VerticalAndHorizontal);_navigation.style.flexShrink=0;_navigation.horizontalScrollerVisibility=_navigation.verticalScrollerVisibility=ScrollerVisibility.Hidden;_layout.Add(_navigation);_nav=_navigation.contentContainer;
            for(int i=0;i<Sections.Length;i++){int index=i;var button=Button(_nav,"archive-section-"+Sections[i],()=>_select?.Invoke(Sections[index]));button.style.flexDirection=FlexDirection.Row;button.style.alignItems=Align.Center;button.style.paddingLeft=button.style.paddingRight=12;button.style.paddingTop=button.style.paddingBottom=8;Surface(button,false);Icon(button,Icons[i],17,C("#64748b")).style.marginRight=8;var label=Box(button,"archive-section-copy");_tabLabels.Add(Text(label,"section-label",_state.Russian?LabelsRu[i]:LabelsEn[i],12,"#64748b","600"));_subtitles.Add(Text(label,"section-subtitle",_state.Russian?SubRu[i]:SubEn[i],9,"#475569"));Icon(button,"ChevronRight",13,C("#64748b66")).style.marginLeft=8;_tabs.Add(button);}
            _scroll=new ScrollView(ScrollViewMode.Vertical);_scroll.style.flexGrow=1;_scroll.style.flexShrink=1;_scroll.style.minHeight=_scroll.style.minWidth=0;_scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;_layout.Add(_scroll);_main=_scroll.contentContainer;Surface(_scroll,false);
            _searchBox=Box(_main,"archive-search",true);_searchBox.style.alignItems=Align.Center;_searchBox.style.paddingLeft=_searchBox.style.paddingRight=12;_searchBox.style.paddingTop=_searchBox.style.paddingBottom=8;_searchBox.style.marginBottom=16;Surface(_searchBox,false);Icon(_searchBox,"Search",14,C("#64748b")).style.marginRight=8;
            _search=new TextField {name="archive-query"};_search.style.flexGrow=1;_search.style.minWidth=0;_search.style.fontSize=12;_search.style.backgroundColor=Color.clear;_search.style.color=C("#dcefff");_search.textEdition.placeholder=L("Поиск по архиву","Search archive");_search.RegisterValueChangedCallback(evt=>_query?.Invoke(evt.newValue));_searchBox.Add(_search);
            _cards=Box(_main,"archive-cards");_cards.style.marginLeft=_cards.style.marginRight=-6;
            foreach(var button in _buttons)_glass.Add(new NativeMenuGlass(button,8));
        }
        private void BindCards()
        {
            foreach(var paint in _cardPaint)paint.Dispose();_cardPaint.Clear();_columns.Clear();_conceptColumns.Clear();_cards.Clear();
            VisualElement grid=null;string previous=null;
            foreach(var card in _state.Cards){
                if(grid==null||card.Group!=previous){
                    previous=card.Group;
                    if(!string.IsNullOrEmpty(card.Group)){var heading=Text(_cards,"archive-group",GroupName(card.Group),12,"#f5d0fe","700");heading.style.marginLeft=6;heading.style.marginBottom=8;heading.style.marginTop=grid==null?0:8;}
                    grid=Box(_cards,"archive-grid",true);grid.style.flexWrap=Wrap.Wrap;
                }
                var column=Box(grid,"archive-column");column.style.width=Length.Percent(_state.Section=="basics"?100:50);column.style.paddingLeft=column.style.paddingRight=6;column.style.paddingBottom=_state.Section=="basics"?16:12;if(_state.Section!="basics")_columns.Add(column);
                var article=Box(column,"archive-card");Pad(article,16);Surface(article,true);article.style.opacity=card.Open?1:.8f;
                var row=Box(article,"archive-card-row",true);row.style.alignItems=Align.FlexStart;
                var icon=Box(row,"archive-card-icon");icon.style.width=icon.style.height=36;icon.style.marginTop=2;icon.style.marginRight=12;icon.style.alignItems=Align.Center;icon.style.justifyContent=Justify.Center;Surface(icon,true);Icon(icon,card.Open?"Sparkles":"LockKeyhole",15,C(card.Open?"#a5f3fc":"#64748b"));
                var copy=Box(row,"archive-card-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;
                Text(copy,"archive-card-title",card.Open?card.Title:L("Запись не открыта","Entry locked"),16,"#f1f5f9","600",true);
                Text(copy,"archive-card-description",card.Open?card.Description:L("Получите этот объект во время игры, чтобы открыть запись.","Obtain or encounter this during a run to unlock the entry."),12,"#94a3b8","400",true,20d/12).style.marginTop=4;
                if(card.Open&&!string.IsNullOrEmpty(card.Details))Details(copy,card);
            }
        }
        private void Details(VisualElement parent,Card card)
        {
            if(card.BasicIndex==0){var grid=Box(parent,"core-concepts",true);grid.style.flexWrap=Wrap.Wrap;grid.style.marginTop=16;grid.style.marginLeft=grid.style.marginRight=-6;foreach(string line in card.Details.Split('\n')){int colon=line.IndexOf(": ",StringComparison.Ordinal);if(colon<0)continue;var column=Box(grid,"core-concept-column");column.style.paddingLeft=column.style.paddingRight=6;column.style.paddingBottom=12;_conceptColumns.Add(column);var box=Box(column,"core-concept");Pad(box,12);Surface(box,true);Text(box,"core-concept-name",line.Substring(0,colon).ToUpperInvariant(),12,"#a5f3fc","700",true);Text(box,"core-concept-copy",line.Substring(colon+2),12,"#94a3b8","400",true,20d/12).style.marginTop=4;}return;}
            if(card.BasicIndex==1){int index=0;foreach(string paragraph in card.Details.Split(new[]{"\n\n"},StringSplitOptions.None)){var text=Text(parent,"resonance-explanation",paragraph,14,index++==3?"#a5f3fc":"#cbd5e1","400",true,24d/14);text.style.marginTop=index==1?16:12;}return;}
            Text(parent,"archive-details",card.Details,12,_state.Section=="spheres"?"#94a3b8":"#64748b","400",true,_state.Section=="spheres"?20d/12:1.5).style.marginTop=_state.Section=="monsters"?8:12;
        }
        private string GroupName(string group)=>group=="sphere-branches"?L("ВЕТКИ СФЕР","SPHERE BRANCHES"):group=="sphere-finals"?L("ФИНАЛЬНЫЕ ФОРМЫ СФЕР","FINAL SPHERE FORMS"):L("ЭВОЛЮЦИИ СПОСОБНОСТЕЙ","ABILITY EVOLUTIONS");
        private void Surface(VisualElement e,bool card){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;Paint(e,NativeProgressionScreen.CardPaint(true,default,default),card);}
        private void Paint(VisualElement e,NativeMenuBrush.Paint paint,bool card){var surface=new NativeHudSurface(e,paint);(card?_cardPaint:_chrome).Add(surface);}
        private Button Button(VisualElement parent,string name,Action action){var button=new Button(action){name=name};button.AddToClassList("progression-button");parent.Add(button);_buttons.Add(button);return button;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var text=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);text.text=value;return text;}
        private static VisualElement Icon(VisualElement parent,string name,float size,Color color){var e=Box(parent,"icon-"+name);e.style.width=e.style.height=size;e.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+name));e.style.unityBackgroundImageTintColor=color;return e;}
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static void Pad(VisualElement e,float p){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=p;}
        private void Clear(){foreach(var glass in _glass)glass.Dispose();_glass.Clear();foreach(var p in _chrome)p.Dispose();_chrome.Clear();foreach(var p in _cardPaint)p.Dispose();_cardPaint.Clear();_buttons.Clear();_tabs.Clear();_tabLabels.Clear();_subtitles.Clear();_columns.Clear();_conceptColumns.Clear();_page.Clear();}
        public void Hide(){if(!_visible)return;_visible=false;NativeMenuRetainedState.Visible(_root,false);}
        private void OnDestroy(){if(_document!=null)Clear();_background?.Dispose();if(_panel!=null)Destroy(_panel);}
    }
}
