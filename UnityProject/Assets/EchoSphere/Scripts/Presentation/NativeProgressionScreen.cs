using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // App.tsx ShopScreen, LeaderboardScreen and AchievementsScreen, authored as
    // retained UI Toolkit controls. State/callbacks belong to the runtime, not UI.
    public sealed class NativeProgressionScreen : MonoBehaviour
    {
        public enum Page { Shop, Leaderboard, Achievements }
        public sealed class ShopItem { public string Id,Name,Description,Owned,Price; public bool CanBuy,Maxed; }
        public sealed class Score { public string Name,Wave,Time; }
        public sealed class Achievement { public string Id,Name,Description,Status; public bool Unlocked; }
        public sealed class State
        {
            public Page Page; public string Title,Back,Gold,Count,Empty;
            public string Rank,Player,Wave,Score;
            public IReadOnlyList<ShopItem> Shop=Array.Empty<ShopItem>();
            public IReadOnlyList<Score> Scores=Array.Empty<Score>();
            public IReadOnlyList<Achievement> Achievements=Array.Empty<Achievement>();
        }
        private UIDocument _document;
        private PanelSettings _panel;
        private VisualElement _root,_content,_body;
        private ScrollView _scroll;
        private NativeMainMenuBackground _background;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly List<NativeMenuGlass> _glass=new List<NativeMenuGlass>();
        private readonly List<ShopRow> _shopRows=new List<ShopRow>();
        private Label _balance;
        private Action _back;
        private Action<string> _buy;
        private State _shown;
        private bool _visible;
        private int _geometryFrame;
        private float _scale=1;
        private sealed class ShopRow { internal Label Owned,Price; internal Button Button; internal ShopItem State; }
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        internal static NativeMenuBrush.Paint CardPaint(bool concept,Color fill,Color border,float radius=12)=>concept?new NativeMenuBrush.Paint {
            Radius=5,Border=1,BorderColor=A(86,182,238,.20f),LinearAngle=145,
            Colors=new[]{A(9,23,39,.91f),A(2,8,15,.97f)},Stops=new[]{0f,1f},
            Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),A(155,225,255,.025f)),
            InsetB=new NativeMenuBrush.Shadow(28,Vector2.zero,A(42,143,198,.025f)),
            OuterA=new NativeMenuBrush.Shadow(28,new Vector2(0,12),A(0,0,0,.20f))
        }:new NativeMenuBrush.Paint {Radius=radius,Border=1,Fill=fill,BorderColor=border};
        private void Ensure()
        {
            if(_document!=null)return;
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Native progression panel";
            _panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1001;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            _root=_document.rootVisualElement;_root.name="native-progression-root";Fill(_root);_root.pickingMode=PickingMode.Ignore;
            _root.styleSheets.Add(Resources.Load<StyleSheet>("EchoSphere/NativeProgression"));
            _background=new NativeMainMenuBackground(_root,"EchoSphere/NativeAppBackground");
            _scroll=new ScrollView(ScrollViewMode.Vertical);Fill(_scroll);_scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
            _scroll.contentContainer.style.alignItems=Align.Center;_scroll.contentContainer.style.minHeight=Length.Percent(100);_scroll.contentContainer.style.justifyContent=Justify.Center;_root.Add(_scroll);
            _content=Box(_scroll,"progression-content");_content.style.width=Length.Percent(100);_content.style.flexShrink=0;
            _content.style.paddingLeft=_content.style.paddingRight=24;_content.style.paddingTop=_content.style.paddingBottom=32;
            _content.RegisterCallback<GeometryChangedEvent>(_=>_geometryFrame=Time.frameCount);
        }
        public void Show(State state,float scale,Action back,Action<string> buy)
        {
            Ensure();_back=back;_buy=buy;_scale=scale;_panel.scale=scale;
            bool rebuilt=!NativeMenuRetainedState.Same(_shown,state);
            if(rebuilt){ClearContent();Build(state);_shown=state;_geometryFrame=Time.frameCount;_content.style.opacity=0;}
            _document.enabled=true;_root.style.display=DisplayStyle.Flex;NativeMenuRetainedState.Visible(_root,true);_visible=true;
            _content.style.maxWidth=state.Page==Page.Achievements?672:820;
            _content.style.paddingTop=Mathf.Max(32,(Screen.height-Screen.safeArea.yMax)/scale);
            _content.style.paddingBottom=Mathf.Max(32,Screen.safeArea.yMin/scale);
            _background.SetViewport(Screen.width/scale,Screen.height/scale,scale);
            Prepare();
        }
        public void UpdateShop(State state)
        {
            if(_shown==null||_shown.Page!=Page.Shop||state.Shop.Count!=_shopRows.Count)return;
            _balance.text=state.Gold;
            for(int i=0;i<_shopRows.Count;i++){
                var row=_shopRows[i];var item=state.Shop[i];row.State=item;row.Owned.text=item.Owned;row.Price.text=item.Price;
                row.Button.SetEnabled(item.CanBuy);row.Button.EnableInClassList("can-buy",item.CanBuy);
                row.Price.style.color=item.CanBuy?C("#dcecff"):A(127,155,184,.5f);
            }
            _shown=state;
        }
        private void Build(State state)
        {
            bool concept=state.Page!=Page.Achievements;
            if(concept){
                Paint(_content,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,.15f),Colors=new[]{A(62,145,201,.10f),Color.clear},Stops=new[]{0f,.35f}});
                Paint(_content,new NativeMenuBrush.Paint {Linear=true,Colors=new[]{A(2,7,14,.98f),A(3,8,15,.98f)},Stops=new[]{0f,1f}});
            }
            var header=Box(_content,"progression-header",true);header.style.alignItems=Align.Center;header.style.justifyContent=Justify.SpaceBetween;header.style.marginBottom=24;
            var back=Button(header,"progression-back",()=>_back?.Invoke());back.AddToClassList("back");back.style.flexDirection=FlexDirection.Row;back.style.alignItems=Align.Center;
            var arrow=Icon(back,"ArrowLeft",20,C("#7f9bb8"));arrow.style.marginRight=8;
            Text(back,"back-text",state.Back,16,"#7f9bb8");_glass.Add(new NativeMenuGlass(back,8));
            if(state.Page==Page.Shop){var gold=Box(header,"shop-balance",true);gold.style.alignItems=Align.Center;Text(gold,"shop-coin","●",24,"#ffb84d","700").style.marginRight=8;_balance=Text(gold,"shop-gold",state.Gold,16,"#ffb84d","700");}
            else if(state.Page==Page.Achievements)Text(header,"achievement-count",state.Count,14,"#7f9bb8b3");
            var title=Text(_content,"progression-title",state.Title,24,concept?"#e3f4ff":"#dcecff",concept?"500":"700");
            title.style.unityTextAlign=TextAnchor.MiddleCenter;title.style.height=32;title.style.marginBottom=24;
            if(concept)title.style.letterSpacing=18;
            _body=Box(_content,"progression-body");
            if(state.Page==Page.Shop)foreach(var item in state.Shop)Shop(item);
            else if(state.Page==Page.Leaderboard)Leaderboard(state);
            else foreach(var item in state.Achievements)AchievementCard(item);
        }
        private void Shop(ShopItem item)
        {
            var card=Box(_body,"shop-"+item.Id,true);card.style.alignItems=Align.Center;card.style.marginBottom=12;Padding(card,16);
            ReserveBorder(card);Paint(card,CardPaint(true,default,default));
            var copy=Box(card,"shop-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;copy.style.marginRight=16;
            Text(copy,"shop-name",item.Name,16,"#dcefff","500",true);
            Text(copy,"shop-description",item.Description,14,"#7f9bb8","400",true);
            var owned=Text(copy,"shop-owned",item.Owned,12,"#7f9bb880");owned.style.marginTop=4;
            var row=new ShopRow {Owned=owned,State=item};
            var button=row.Button=Button(card,"buy-"+item.Id,()=>_buy?.Invoke(row.State.Id));
            button.AddToClassList("purchase");button.style.minWidth=96;button.style.paddingLeft=button.style.paddingRight=20;
            button.style.paddingTop=button.style.paddingBottom=10;ReserveBorder(button);
            Paint(button,CardPaint(true,default,default));_glass.Add(new NativeMenuGlass(button,8));
            row.Price=Text(button,"shop-price",item.Price,14,item.CanBuy?"#dcecff":"#7f9bb880","500");
            row.Price.style.unityTextAlign=TextAnchor.MiddleCenter;button.SetEnabled(item.CanBuy);button.EnableInClassList("can-buy",item.CanBuy);
            _shopRows.Add(row);
        }
        private void Leaderboard(State state)
        {
            if(state.Scores.Count==0){var empty=Text(_body,"leader-empty",state.Empty,16,"#7f9bb8b3","400",true);empty.style.unityTextAlign=TextAnchor.MiddleCenter;empty.style.paddingTop=empty.style.paddingBottom=48;return;}
            ScoreRow(new[]{state.Rank.ToUpperInvariant(),state.Player.ToUpperInvariant(),state.Wave.ToUpperInvariant(),state.Score.ToUpperInvariant()},-1);
            for(int i=0;i<state.Scores.Count;i++){var score=state.Scores[i];ScoreRow(new[]{"#"+(i+1),score.Name,score.Wave,score.Time},i);}
        }
        private void ScoreRow(string[] values,int index)
        {
            var row=Box(_body,index<0?"leader-columns":"leader-row-"+index,true);row.style.alignItems=Align.Center;
            row.style.paddingLeft=row.style.paddingRight=16;row.style.marginBottom=8;
            if(index>=0){row.style.paddingTop=row.style.paddingBottom=12;if(index==0)ReserveBorder(row);Paint(row,CardPaint(true,default,default));}
            for(int i=0;i<4;i++){
                var label=Text(row,"leader-cell-"+i,values[i],index<0?12:(i<2?16:14),index<0?"#7f9bb8b3":i==0?"#39d8ff":i==2?"#7f9bb8":"#dcefff",i==0&&index>=0?"700":"400");
                label.style.flexBasis=0;label.style.flexGrow=i==1?6:2;label.style.flexShrink=1;if(i<3)label.style.marginRight=8;
                if(i>=2)label.style.unityTextAlign=TextAnchor.MiddleRight;
                if(i==1){label.style.overflow=Overflow.Hidden;label.style.textOverflow=TextOverflow.Ellipsis;}
                if(i==3&&index>=0){var font=SourceUiTypography.Resolve("ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace","400",false,20d/14);if(font!=null)label.style.unityFontDefinition=FontDefinition.FromSDFFont(font);}
            }
        }
        private void AchievementCard(Achievement item)
        {
            var card=Box(_body,"achievement-"+item.Id,true);card.style.alignItems=Align.Center;card.style.marginBottom=12;Padding(card,16);ReserveBorder(card);
            card.style.opacity=item.Unlocked?1:.5f;
            Paint(card,CardPaint(false,item.Unlocked?A(255,184,77,.10f):C("#0d1726"),item.Unlocked?A(255,184,77,.30f):C("#243b55")));
            var circle=Box(card,"achievement-medal");circle.style.width=circle.style.height=40;circle.style.marginRight=16;circle.style.alignItems=Align.Center;circle.style.justifyContent=Justify.Center;
            Paint(circle,new NativeMenuBrush.Paint {Ellipse=true,Fill=item.Unlocked?A(255,184,77,.20f):C("#0d1726")});
            Icon(circle,"Award",20,item.Unlocked?C("#ffb84d"):A(127,155,184,.5f));
            var copy=Box(card,"achievement-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;copy.style.marginRight=16;
            Text(copy,"achievement-name",item.Name,16,item.Unlocked?"#ffb84d":"#7f9bb8","500",true);
            Text(copy,"achievement-description",item.Description,14,"#7f9bb8b3","400",true);
            Text(card,"achievement-status",item.Status,12,item.Unlocked?"#ffb84d":"#dcecff33","700");
        }
        private void Prepare()
        {
            if(!_visible||Time.frameCount-_geometryFrame<2)return;
            foreach(var surface in _surfaces)surface.Prepare(_scale);
            foreach(var glass in _glass)glass.Prepare(_root,8,_scale);
            _content.style.opacity=1;
        }
        private void Paint(VisualElement node,NativeMenuBrush.Paint paint)=>_surfaces.Add(new NativeHudSurface(node,paint));
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static void Padding(VisualElement e,float value){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=value;}
        private static void ReserveBorder(VisualElement e){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false){var text=NativePauseStatsView.Text(parent,name,size,color,weight,1.5,.192f,wrap);text.text=value;return text;}
        private static Button Button(VisualElement parent,string name,Action action){var button=new Button(action){name=name};button.AddToClassList("progression-button");parent.Add(button);return button;}
        private static VisualElement Icon(VisualElement parent,string name,float size,Color color){var e=Box(parent,"icon-"+name);e.style.width=e.style.height=size;e.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+name));e.style.unityBackgroundImageTintColor=color;return e;}
        private void ClearContent(){foreach(var glass in _glass)glass.Dispose();_glass.Clear();foreach(var surface in _surfaces)surface.Dispose();_surfaces.Clear();_shopRows.Clear();_content.Clear();}
        public void Hide(){if(_document==null||!_visible)return;_visible=false;NativeMenuRetainedState.Visible(_root,false);}
        private void OnDestroy(){if(_document!=null)ClearContent();_background?.Dispose();if(_panel!=null)Destroy(_panel);}
    }
}
