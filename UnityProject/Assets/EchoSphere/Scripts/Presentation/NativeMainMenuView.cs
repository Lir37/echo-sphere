using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained control/layout portion of App.tsx Menu. Artwork and composition
    // are separate native layers; this view does not replace those with drawings.
    // Runtime selection remains with the owner until visual acceptance is complete.
    public sealed class NativeMainMenuView : IDisposable
    {
        public enum Destination { Map, Characters, Shop, Leaderboard, Achievements, Knowledge, Settings, Continue }

        public readonly struct State : IEquatable<State>
        {
            public readonly bool Russian;
            public readonly string Name, CharacterName, CharacterRole, Difficulty;
            public readonly string DifficultyTitle, NamePlaceholder;
            public readonly string ShopTitle, LeaderboardTitle, AchievementsTitle, SettingsTitle;
            public readonly double? SavedRunTime;
            public State(bool russian,string name,string characterName,string characterRole,string difficulty,
                string difficultyTitle,string namePlaceholder,string shopTitle,string leaderboardTitle,
                string achievementsTitle,string settingsTitle,double? savedRunTime)
            {
                Russian=russian;Name=name;CharacterName=characterName;CharacterRole=characterRole;Difficulty=difficulty;
                DifficultyTitle=difficultyTitle;NamePlaceholder=namePlaceholder;ShopTitle=shopTitle;
                LeaderboardTitle=leaderboardTitle;AchievementsTitle=achievementsTitle;SettingsTitle=settingsTitle;SavedRunTime=savedRunTime;
            }
            public bool Equals(State other)=>Russian==other.Russian&&Name==other.Name&&CharacterName==other.CharacterName&&
                CharacterRole==other.CharacterRole&&Difficulty==other.Difficulty&&DifficultyTitle==other.DifficultyTitle&&
                NamePlaceholder==other.NamePlaceholder&&ShopTitle==other.ShopTitle&&LeaderboardTitle==other.LeaderboardTitle&&
                AchievementsTitle==other.AchievementsTitle&&SettingsTitle==other.SettingsTitle&&SavedRunTime==other.SavedRunTime;
        }

        private static readonly string[] DifficultyIds={"easy","normal","hard","nightmare"};
        private readonly Action<Destination> _navigate;
        private readonly Action<string> _setName,_setDifficulty;
        private readonly VisualElement _root,_parent;
        private readonly TextField _name;
        private readonly Button[] _difficulty=new Button[4];
        private readonly Button[] _navigation=new Button[6];
        private readonly Button _continue;
        private bool _disposed,_hasState;
        private State _shown;
        public VisualElement Root=>_root;
        public VisualElement Artwork=>_root.Q("core-artwork");
        public VisualElement Background=>_root.Q("background-artwork");
        public VisualElement Avatar=>_root.Q("character-avatar");
        public VisualElement Navigation=>_root.Q("menu-navigation");

        public NativeMainMenuView(VisualElement parent,Action<Destination> navigate,Action<string> setName,Action<string> setDifficulty)
        {
            _parent=parent??throw new ArgumentNullException(nameof(parent));
            _navigate=navigate??throw new ArgumentNullException(nameof(navigate));
            _setName=setName??throw new ArgumentNullException(nameof(setName));
            _setDifficulty=setDifficulty??throw new ArgumentNullException(nameof(setDifficulty));
            var tree=Resources.Load<VisualTreeAsset>("EchoSphere/NativeMainMenu");
            var styles=Resources.Load<StyleSheet>("EchoSphere/NativeMainMenu");
            if(tree==null||styles==null)throw new InvalidOperationException("Native main menu authoring assets missing");
            _root=tree.CloneTree();_root.name="native-main-menu";_root.styleSheets.Add(styles);
            _root.AddToClassList("menu-host");
            var icons=new[]{"UserRound","Store","Trophy","Award","BookOpen","Settings"};
            for(int i=0;i<icons.Length;i++){
                var icon=Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/"+icons[i]);
                if(icon==null)throw new InvalidOperationException("Native menu icon missing: "+icons[i]);
                _root.Q("nav-icon-"+i).style.backgroundImage=new StyleBackground(icon);
                if(i==0)_root.Q("character-icon").style.backgroundImage=new StyleBackground(icon);
            }
            _name=_root.Q<TextField>("player-name");_name.maxLength=16;
            _name.RegisterValueChangedCallback(NameChanged);
            Bind("open-map",Destination.Map);Bind("open-character",Destination.Characters);
            _continue=Bind("continue-run",Destination.Continue);
            var destinations=new[]{Destination.Characters,Destination.Shop,Destination.Leaderboard,Destination.Achievements,Destination.Knowledge,Destination.Settings};
            for(int i=0;i<destinations.Length;i++)_navigation[i]=Bind("nav-"+i,destinations[i]);
            for(int i=0;i<DifficultyIds.Length;i++){
                var id=DifficultyIds[i];var button=_root.Q<Button>("difficulty-"+id);_difficulty[i]=button;
                button.clicked+=()=>_setDifficulty(id);
            }
            _parent.Add(_root);_parent.RegisterCallback<GeometryChangedEvent>(ViewportChanged);
            Resize(_parent.contentRect.width,_parent.contentRect.height);
            // Never expose an unbound first-frame control tree.
            _root.style.display=DisplayStyle.None;
        }

        public void Show(in State state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMainMenuView));
            if(!_hasState||!_shown.Equals(state)){
                var ru=state.Russian;
                Text("menu-subtitle",ru?"СИСТЕМА ВЫЖИВАНИЯ РЕЗОНАНСА":"RESONANCE SURVIVAL SYSTEM");
                Text("core-caption","ECHO CORE // "+(ru?"ГОТОВ":"READY"));
                Text("core-title",ru?"РЕГИОНЫ ЭХА":"ECHO REGIONS");
                Text("select-run",ru?"ВЫБРАТЬ ЗАБЕГ":"SELECT RUN");
                Text("core-copy",ru?"Выбери область, стабилизируй её, открой испытания и доберись до Бесконечности.":"Choose a region, stabilize it, unlock challenges and reach Endless.");
                Text("character-name",state.CharacterName);Text("character-role",state.CharacterRole);Text("difficulty-title",state.DifficultyTitle?.ToUpperInvariant());
                _name.SetValueWithoutNotify(state.Name??string.Empty);_name.textEdition.placeholder=state.NamePlaceholder;
                _name.textEdition.hidePlaceholderOnFocus=false;_name.textSelection.selectAllOnFocus=false;_name.textSelection.selectAllOnMouseUp=false;
                for(int i=0;i<DifficultyIds.Length;i++){
                    var profile=Core.DifficultyRules.GetProfile(DifficultyIds[i]);
                    _difficulty[i].text=ru?profile.NameRu:profile.NameEn;
                    _difficulty[i].EnableInClassList("is-active",state.Difficulty==DifficultyIds[i]);
                }
                var labels=new[]{ru?"Персонажи":"Characters",state.ShopTitle,state.LeaderboardTitle,state.AchievementsTitle,ru?"Архив Эха":"Echo Archive",state.SettingsTitle};
                for(int i=0;i<labels.Length;i++)_navigation[i].Q<Label>("nav-label-"+i).text=labels[i]?.ToUpperInvariant();
                Text("continue-title",ru?"ПРОДОЛЖИТЬ ЗАБЕГ":"CONTINUE RUN");
                _continue.style.display=state.SavedRunTime.HasValue?DisplayStyle.Flex:DisplayStyle.None;
                if(state.SavedRunTime.HasValue){
                    var seconds=(long)Math.Floor(state.SavedRunTime.Value);
                    Text("continue-time",(ru?"Сохранено на ":"Saved at ")+seconds/60+":"+(seconds%60).ToString("00"));
                }
                _shown=state;_hasState=true;
            }
            _root.style.display=DisplayStyle.Flex;
        }

        public void Hide(){if(!_disposed)_root.style.display=DisplayStyle.None;}
        private Button Bind(string name,Destination destination)
        {var button=_root.Q<Button>(name);if(button==null)throw new InvalidOperationException("Native menu control missing: "+name);button.clicked+=()=>_navigate(destination);return button;}
        private void NameChanged(ChangeEvent<string> evt)
        {
            // App.tsx onChange slices UTF-16 to 16, including changes assigned
            // by an IME/accessibility integration rather than native key input.
            var value=evt.newValue??string.Empty;
            if(value.Length>16){value=value.Substring(0,16);_name.SetValueWithoutNotify(value);}
            _setName(value);
        }
        private void Text(string name,string value)=>_root.Q<Label>(name).text=value??string.Empty;
        private void ViewportChanged(GeometryChangedEvent evt)=>Resize(evt.newRect.width,evt.newRect.height);
        private void Resize(float width,float height)
        {
            if(!float.IsFinite(width)||!float.IsFinite(height)||width<=0||height<=0)return;
            _root.EnableInClassList("narrow",width<=650);_root.EnableInClassList("short",height<=700);
            _root.Q("menu-core").style.width=Mathf.Min(520,width*.88f);
            var size=height<=700?Mathf.Min(155,height*.35f):Mathf.Min(width<=650?210:270,width*.48f);
            Artwork.style.width=size;Artwork.style.height=size;
            var logo=_root.Q<Label>("menu-logo");var title=_root.Q<Label>("core-title");
            var logoSize=Mathf.Clamp(width*.04f,18,27);var titleSize=Mathf.Clamp(width*.038f,15,22);
            logo.style.fontSize=logoSize;logo.style.letterSpacing=logoSize*(width<=650?.24f:.34f);
            title.style.fontSize=titleSize;title.style.letterSpacing=titleSize*.18f;
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
            _parent.UnregisterCallback<GeometryChangedEvent>(ViewportChanged);
            _name.UnregisterValueChangedCallback(NameChanged);_root.RemoveFromHierarchy();
        }
    }
}
