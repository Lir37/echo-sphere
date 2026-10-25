using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native CharacterSelect.tsx owner; display-only state, no combat simulation.
    public sealed class NativeCharacterScreen : MonoBehaviour
    {
        public sealed class Mastery { public string Title,Description,Threshold; public bool Reached,Current; }
        public sealed class Affinity { public string Text,Color; }
        public sealed class Character
        {
            public string Id,Name,Role,Description,Mechanic,Color,MasteryLabel,MasteryXp,Action,Toggle,NextLabel,NextTitle,NextDescription;
            public bool Unlocked,Selected,Expanded,Complete;public float MasteryPercent;
            public IReadOnlyList<Affinity> Affinities;public IReadOnlyList<Mastery> Masteries;
        }
        public sealed class State { public bool Russian; public string Gold,Message;public IReadOnlyList<Character> Characters; }
        private NativePanelLease _panelLease;
        private UIDocument _document;private PanelSettings _panel;
        private VisualElement _root,_page,_heading,_message,_body;private ScrollView _scroll;
        private NativeMainMenuBackground _background;
        private readonly List<NativeHudSurface> _surfaces=new List<NativeHudSurface>();
        private readonly List<NativeMenuGlass> _glass=new List<NativeMenuGlass>();
        private readonly List<Button> _buttons=new List<Button>();
        private readonly List<VisualElement> _cards=new List<VisualElement>();
        private readonly List<(Image image,string id)> _portraits=new List<(Image,string)>();
        private State _state;private Action _back;private Action<string> _choose,_expand;
        private bool _visible,_narrow,_restoreScroll;private int _geometryFrame;private Vector2 _pendingScroll;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        private string L(string ru,string en)=>_state.Russian?ru:en;
        private void Ensure()
        {
            if(_document!=null)return;
            _panelLease=new NativePanelLease(gameObject,"Native character panel",1001,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeCharacters")??throw new InvalidOperationException("Native screen UXML is missing: NativeCharacters"));
            _document=_panelLease.Document;_panel=_panelLease.Panel;_root=_document.rootVisualElement;Fill(_root);_root.pickingMode=PickingMode.Ignore;_root.style.alignItems=Align.Center;
            _root.styleSheets.Add(Resources.Load<StyleSheet>("EchoSphere/NativeProgression"));_background=new NativeMainMenuBackground(_root,"EchoSphere/NativeAppBackground");
            _page=_root.Q<VisualElement>("native-character-page");_heading=_root.Q<VisualElement>("character-heading");_message=_root.Q<VisualElement>("character-message");
            _scroll=_root.Q<ScrollView>("character-scroll");
            _body=_scroll.contentContainer;
            _page.RegisterCallback<GeometryChangedEvent>(_=>_geometryFrame=Time.frameCount);
        }
        public void Show(State state,float scale,Action back,Action<string> choose,Action<string> expand)
        {
            Ensure();_back=back;_choose=choose;_expand=expand;_panel.scale=scale;_visible=true;_root.style.display=DisplayStyle.Flex;NativeMenuRetainedState.Visible(_root,true);
            bool narrow=Screen.width/scale<=650;
            if(!NativeMenuRetainedState.Same(_state,state)||narrow!=_narrow){
                _pendingScroll=_state==null?Vector2.zero:_scroll.scrollOffset;_restoreScroll=true;
                bool rebuild=_state==null||narrow!=_narrow||_state.Russian!=state.Russian||_state.Message!=state.Message||_state.Characters.Count!=state.Characters.Count;
                if(!rebuild)for(int i=0;i<state.Characters.Count;i++)if(_state.Characters[i].Id!=state.Characters[i].Id){rebuild=true;break;}
                var previous=_state;_narrow=narrow;_state=state;
                if(rebuild){Clear();Build();foreach(var button in _buttons)_glass.Add(new NativeMenuGlass(button,8));}
                else UpdateCards(previous);
                _geometryFrame=Time.frameCount;_page.style.opacity=0;
            }
            Pad(_page,narrow?10:16);_page.style.paddingTop=Mathf.Max(narrow?10:16,(Screen.height-Screen.safeArea.yMax)/scale);_page.style.paddingBottom=Mathf.Max(narrow?10:16,Screen.safeArea.yMin/scale);
            _background.SetViewport(Screen.width/scale,Screen.height/scale,scale);
            LoadVisiblePortraits();
            if(Time.frameCount-_geometryFrame>=2){if(_restoreScroll){_scroll.scrollOffset=_pendingScroll;_restoreScroll=false;}foreach(var paint in _surfaces)paint.Prepare(scale);foreach(var glass in _glass)glass.Prepare(_root,8,scale);_page.style.opacity=1;}
        }
        private void Build()
        {
            Paint(_page,new NativeMenuBrush.Paint {Origin=new Vector2(.5f,.15f),Colors=new[]{A(62,145,201,.10f),Color.clear},Stops=new[]{0f,.35f}});
            Paint(_page,new NativeMenuBrush.Paint {Linear=true,Colors=new[]{A(2,7,14,.98f),A(3,8,15,.98f)},Stops=new[]{0f,1f}});
            var top=Box(_heading,"character-top",true);top.style.justifyContent=Justify.SpaceBetween;top.style.alignItems=Align.Center;top.style.marginBottom=16;
            var back=Button(top,"character-back",()=>_back?.Invoke());back.style.flexDirection=FlexDirection.Row;back.style.alignItems=Align.Center;back.style.paddingLeft=back.style.paddingRight=12;back.style.paddingTop=back.style.paddingBottom=8;Border(back,6);Paint(back,new NativeMenuBrush.Paint {Radius=6,Border=1,Fill=C("#0d1726"),BorderColor=C("#243b55")});Icon(back,"ArrowLeft",18,C("#dcecff")).style.marginRight=8;Text(back,"back",L("Назад","Back"),16,"#dcecff");
            Text(top,"character-gold",_state.Gold,14,"#dcecff","700");
            var title=Text(_heading,"character-title",L("ПЕРСОНАЖИ","CHARACTERS"),24,"#e9f8ff","500");title.style.letterSpacing=18;
            Text(_heading,"character-subtitle",L("Персонаж меняет стиль билда, но не управление.","Characters change your build style, not the controls."),12,"#648198","400",true).style.marginTop=4;
            _message.style.display=string.IsNullOrEmpty(_state.Message)?DisplayStyle.None:DisplayStyle.Flex;
            if(!string.IsNullOrEmpty(_state.Message)){Paint(_message,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=A(255,184,77,.10f),BorderColor=A(255,107,107,.4f)});Text(_message,"character-error",_state.Message,12,"#ffb84d","400",true);}
            foreach(var character in _state.Characters)Card(character);
        }
        private static NativeMenuBrush.Paint CardPaint(bool hover)=>new NativeMenuBrush.Paint {
            Radius=5,Border=1,BorderColor=hover?A(94,217,255,.38f):A(79,174,225,.18f),LinearAngle=145,Colors=new[]{A(9,23,39,.91f),A(2,8,15,.97f)},Stops=new[]{0f,1f},
            OuterA=hover?new NativeMenuBrush.Shadow(22,Vector2.zero,A(59,198,255,.07f)):new NativeMenuBrush.Shadow(28,new Vector2(0,12),A(0,0,0,.20f)),
            Inset=hover?new NativeMenuBrush.Shadow(30,Vector2.zero,A(42,143,198,.045f)):new NativeMenuBrush.Shadow(0,new Vector2(0,1),A(155,225,255,.025f)),
            InsetB=hover?(NativeMenuBrush.Shadow?)null:new NativeMenuBrush.Shadow(28,Vector2.zero,A(42,143,198,.025f))
        };
        private void Card(Character character)
        {
            var card=Box(_body,"character-"+character.Id);card.AddToClassList("character-card");_cards.Add(card);Pad(card,_narrow?11:16);Border(card,5);card.style.marginBottom=12;
            var normal=CardPaint(false);var hover=CardPaint(true);var surface=Paint(card,normal);
            card.RegisterCallback<PointerEnterEvent>(_=>{surface.SetPaint(hover);card.style.translate=new Translate(0,-1,0);});
            card.RegisterCallback<PointerLeaveEvent>(_=>{surface.SetPaint(normal);card.style.translate=new Translate(0,0,0);});
            var choose=Button(card,"choose-"+character.Id,()=>_choose?.Invoke(character.Id));choose.style.width=Length.Percent(100);choose.style.borderTopLeftRadius=choose.style.borderTopRightRadius=choose.style.borderBottomLeftRadius=choose.style.borderBottomRightRadius=6;
            var row=Box(choose,"character-main",true);row.style.alignItems=Align.FlexStart;
            var glyph=Box(row,"character-glyph");glyph.style.width=glyph.style.height=44;glyph.style.marginRight=12;glyph.style.alignItems=Align.Center;glyph.style.justifyContent=Justify.Center;Border(glyph,12);Paint(glyph,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=C("#081522"),BorderColor=C("#243b55")});
            var image=new Image {name="character-portrait",scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};image.style.position=Position.Absolute;image.style.left=image.style.right=image.style.top=image.style.bottom=0;image.style.opacity=character.Unlocked?1:.35f;glyph.Add(image);_portraits.Add((image,character.Id));
            if(!character.Unlocked)Icon(glyph,"Lock",18,C(character.Color));
            var copy=Box(row,"character-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;
            var nameRow=Box(copy,"character-name-row",true);nameRow.style.justifyContent=Justify.SpaceBetween;nameRow.style.alignItems=Align.Center;
            var names=Box(nameRow,"character-name-group");names.style.flexGrow=1;names.style.flexShrink=1;
            Text(names,"character-name",character.Name,16,"#e8f8ff","700",true);var role=Text(names,"character-role",character.Role,10,"#7f9bb8");role.style.letterSpacing=5;
            if(character.Selected)Icon(nameRow,"Check",18,C("#63e6ff"));
            Text(copy,"character-description",character.Description,_narrow?10:12,"#dcecff","400",true,1.625).style.marginTop=8;
            var affinities=Box(copy,"character-affinities",true);affinities.style.flexWrap=Wrap.Wrap;affinities.style.marginTop=8;
            foreach(var affinity in character.Affinities){var tag=Box(affinities,"character-affinity");tag.style.marginRight=6;tag.style.marginBottom=6;tag.style.paddingLeft=tag.style.paddingRight=8;tag.style.paddingTop=tag.style.paddingBottom=4;Border(tag,999);var color=C(affinity.Color);Paint(tag,new NativeMenuBrush.Paint {Radius=999,Border=1,Fill=new Color(color.r,color.g,color.b,18f/255),BorderColor=new Color(color.r,color.g,color.b,102f/255)});Text(tag,"affinity-copy",affinity.Text,9,"#bfeeff");}
            Text(copy,"character-mechanic",character.Mechanic,10,"#7f9bb8","400",true,1.625).style.marginTop=2;
            var mastery=Box(copy,"character-mastery");mastery.style.marginTop=12;var summary=Box(mastery,"mastery-summary",true);summary.style.justifyContent=Justify.SpaceBetween;
            Text(summary,"mastery-label",character.MasteryLabel,10,"#7f9bb8","400",true).style.flexShrink=1;Text(summary,"mastery-xp",character.MasteryXp,10,"#7f9bb8");
            var track=Box(mastery,"mastery-track");track.style.height=6;track.style.marginTop=4;track.style.overflow=Overflow.Hidden;track.style.borderTopLeftRadius=track.style.borderTopRightRadius=track.style.borderBottomLeftRadius=track.style.borderBottomRightRadius=999;track.style.backgroundColor=A(36,59,85,.7f);
            var fill=Box(track,"mastery-fill");fill.style.width=Length.Percent(character.MasteryPercent);fill.style.height=Length.Percent(100);fill.style.backgroundColor=C("#39d8ff");
            if(character.Unlocked){var toggle=Button(card,"expand-"+character.Id,()=>_expand?.Invoke(character.Id));toggle.style.marginTop=12;toggle.style.alignItems=Align.FlexStart;Text(toggle,"mastery-toggle",character.Toggle.ToUpperInvariant(),10,"#63e6ff","700");}
            if(character.Unlocked&&character.Expanded)Masteries(card,character);
            var action=Text(card,"character-action",character.Action, _narrow?10:12,character.Unlocked?"#63e6ff":"#ffb84d","700");action.style.marginTop=12;action.style.unityTextAlign=TextAnchor.MiddleRight;
        }
        private void Masteries(VisualElement parent,Character character)
        {
            var block=Box(parent,"mastery-details");block.style.marginTop=12;block.style.paddingTop=12;block.style.borderTopWidth=1;block.style.borderTopColor=A(36,59,85,.7f);
            foreach(var mastery in character.Masteries){
                var row=Box(block,"mastery-level",true);row.style.marginBottom=8;row.style.paddingLeft=row.style.paddingRight=12;row.style.paddingTop=row.style.paddingBottom=8;Border(row,12);
                Paint(row,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=mastery.Current?A(57,216,255,.10f):mastery.Reached?A(90,140,74,.05f):A(196,184,144,.20f),BorderColor=mastery.Current?A(57,216,255,.30f):mastery.Reached?A(90,140,74,.20f):Color.clear});
                var badge=Box(row,"mastery-badge");badge.style.width=badge.style.height=20;badge.style.marginTop=2;badge.style.marginRight=8;badge.style.alignItems=Align.Center;badge.style.justifyContent=Justify.Center;Border(badge,999);Paint(badge,new NativeMenuBrush.Paint {Ellipse=true,Border=1,Fill=mastery.Reached?A(90,140,74,.15f):A(196,184,144,.20f),BorderColor=mastery.Reached?A(90,140,74,.40f):C("#243b55")});Icon(badge,mastery.Reached?"Check":"Lock",mastery.Reached?12:11,mastery.Reached?C("#5a8c4a"):C("#7f9bb8"));
                var copy=Box(row,"mastery-copy");copy.style.flexGrow=1;copy.style.flexShrink=1;var heading=Box(copy,"mastery-heading",true);heading.style.justifyContent=Justify.SpaceBetween;
                Text(heading,"mastery-name",mastery.Title,_narrow?10:12,mastery.Reached?"#e8f8ff":"#7f9bb8","700",true).style.flexShrink=1;Text(heading,"mastery-threshold",mastery.Threshold,9,"#7f9bb8");Text(copy,"mastery-description",mastery.Description,10,mastery.Reached?"#dcecff":"#7f9bb8","400",true,1.625).style.marginTop=2;
            }
            var next=Box(block,"mastery-next");next.style.marginTop=8;next.style.paddingLeft=next.style.paddingRight=12;next.style.paddingTop=next.style.paddingBottom=8;Border(next,12);Paint(next,new NativeMenuBrush.Paint {Radius=12,Border=1,Fill=character.Complete?A(90,140,74,.10f):A(255,184,77,.10f),BorderColor=character.Complete?A(90,140,74,.30f):A(212,148,61,.30f)});
            if(character.Complete)Text(next,"mastery-complete",character.NextLabel,_narrow?10:12,"#4b713d","700",true);
            else {Text(next,"mastery-next-label",character.NextLabel.ToUpperInvariant(),9,"#a86631","700",true);Text(next,"mastery-next-name",character.NextTitle,_narrow?10:12,"#e8f8ff","700",true).style.marginTop=4;Text(next,"mastery-next-description",character.NextDescription,10,"#dcecff","400",true,1.625).style.marginTop=2;}
        }
        private NativeHudSurface Paint(VisualElement node,NativeMenuBrush.Paint paint){var surface=new NativeHudSurface(node,paint);_surfaces.Add(surface);return surface;}
        private Button Button(VisualElement parent,string name,Action action){var button=new Button(action){name=name};button.AddToClassList("progression-button");parent.Add(button);_buttons.Add(button);return button;}
        private static VisualElement Box(VisualElement parent,string name,bool row=false)=>NativePauseStatsView.Box(parent,name,row);
        private static Label Text(VisualElement parent,string name,string value,float size,string color,string weight="400",bool wrap=false,double line=1.5){var l=NativePauseStatsView.Text(parent,name,size,color,weight,line,.192f,wrap);l.text=value;return l;}
        private static VisualElement Icon(VisualElement parent,string name,float size,Color color){var e=Box(parent,"icon-"+name);e.style.width=e.style.height=size;e.style.backgroundImage=new StyleBackground(Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+name));e.style.unityBackgroundImageTintColor=color;return e;}
        private static void Fill(VisualElement e){e.style.position=Position.Absolute;e.style.left=e.style.right=e.style.top=e.style.bottom=0;}
        private static void Pad(VisualElement e,float p){e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=p;}
        private static void Border(VisualElement e,float radius){e.style.borderLeftWidth=e.style.borderRightWidth=e.style.borderTopWidth=e.style.borderBottomWidth=1;e.style.borderLeftColor=e.style.borderRightColor=e.style.borderTopColor=e.style.borderBottomColor=Color.clear;e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;}
        private void LoadVisiblePortraits()
        {
            // Prioritize the actual viewport over the prefetch margin.
            for(int pass=0;pass<2;pass++)foreach(var portrait in _portraits){
                if(portrait.image.sprite!=null||!NativeHudSurface.InViewport(portrait.image,pass==0?0:200))continue;
                var art=NativeCharacterArtwork.ResolveAsync(portrait.id);if(art!=null)portrait.image.sprite=art.Sprite;
            }
        }
        private void UpdateCards(State previous)
        {
            _heading.Q<Label>("character-gold").text=_state.Gold;
            for(int i=0;i<_state.Characters.Count;i++){
                if(NativeMenuRetainedState.Same(previous.Characters[i],_state.Characters[i]))continue;
                var old=_cards[i];int index=_body.IndexOf(old);
                // Glass and button lists share registration order. Detach the
                // filter owners before disposing their source artwork.
                for(int j=_buttons.Count-1;j>=0;j--)if(old.Contains(_buttons[j])){_glass[j].Dispose();_glass.RemoveAt(j);_buttons.RemoveAt(j);}
                for(int j=_surfaces.Count-1;j>=0;j--)if(_surfaces[j].Host==old||old.Contains(_surfaces[j].Host)){_surfaces[j].Dispose();_surfaces.RemoveAt(j);}
                for(int j=_portraits.Count-1;j>=0;j--)if(old.Contains(_portraits[j].image))_portraits.RemoveAt(j);
                old.RemoveFromHierarchy();int firstButton=_buttons.Count;Card(_state.Characters[i]);
                var replacement=_cards[_cards.Count-1];_cards.RemoveAt(_cards.Count-1);_cards[i]=replacement;_body.Insert(index,replacement);
                for(int j=firstButton;j<_buttons.Count;j++)_glass.Add(new NativeMenuGlass(_buttons[j],8));
            }
            var buttons=new List<Button>();var glass=new List<NativeMenuGlass>();
            void Visit(VisualElement node){
                if(node.userData is SourceUiBackdrop.ClipTag||ReferenceEquals(node.userData,SourceUiBackdrop.ReplicaTag))return;
                if(node is Button button){int index=_buttons.IndexOf(button);if(index>=0){buttons.Add(button);glass.Add(_glass[index]);}}
                for(int i=0;i<node.hierarchy.childCount;i++)Visit(node.hierarchy[i]);
            }
            Visit(_root);_buttons.Clear();_buttons.AddRange(buttons);_glass.Clear();_glass.AddRange(glass);
        }
        private void Clear(){foreach(var glass in _glass)glass.Dispose();_glass.Clear();foreach(var paint in _surfaces)paint.Dispose();_surfaces.Clear();_buttons.Clear();_cards.Clear();_portraits.Clear();_heading.Clear();_message.Clear();_body.Clear();}
        public void Hide(){if(!_visible)return;_visible=false;NativeMenuRetainedState.Visible(_root,false);}
        private void OnDestroy(){if(_document!=null)Clear();_background?.Dispose();_panelLease?.Dispose();}
    }
}
