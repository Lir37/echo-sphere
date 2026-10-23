using System;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativeMainMenuEnabled=true;
        private NativeMainMenuScreen _nativeMainMenuScreen;
        private bool _nativeMainMenuWasActive;
        private int _nativeMenuModelVersion,_nativeMenuShownVersion;
        private NativeMainMenuView.State _nativeMenuState;
        private Color _nativeMenuCharacterColor;
        public bool UsesNativeMainMenu=>_nativeMainMenuWasActive&&_nativeMainMenuScreen!=null&&_nativeMainMenuScreen.Ready;
        private bool TryDrawNativeMainMenu()
        {
            var home=_nativeMainMenuEnabled&&_mainMenu&&!_regionSelect&&_mainMenuPage==MainMenuPage.Home;
            if(!home){
                if(_nativeMainMenuWasActive){_nativeMainMenuScreen.Hide();if(_sourceUi!=null){_sourceUi.Dirty=true;}_nativeMainMenuWasActive=false;}
                return false;
            }
            if(!_nativeMainMenuWasActive){
                if(_nativeSettingsScreen!=null)_nativeSettingsScreen.Hide();
                if(_sourceUi!=null){
                    // Native run pages do not mount source hooks or DOM trees.
                    _nativeSettingsWasActive=false;_sourceUi.Dirty=true;
                }else{
                    // Menu's original mount effect persists its loaded name.
                    PlayerPrefs.SetString(NamePreferenceKey,_playerName);PlayerPrefs.Save();
                }
                if(_nativeMainMenuScreen==null){_nativeMainMenuScreen=new GameObject("Native Main Menu UI Toolkit").AddComponent<NativeMainMenuScreen>();_nativeMainMenuScreen.Initialize(CreateNativeMainMenuView);}
                _nativeMenuState=ReadNativeMainMenuState();_nativeMenuShownVersion=_nativeMenuModelVersion;
                ColorUtility.TryParseHtmlString(CharacterRules.Get(_sphereCharacterId).ColorHex,out _nativeMenuCharacterColor);
                _nativeMainMenuWasActive=true;_camera.cullingMask=0;
            }
            if(_nativeMenuShownVersion!=_nativeMenuModelVersion){_nativeMenuState=ReadNativeMainMenuState(_nativeMenuState.SavedRunTime);_nativeMenuShownVersion=_nativeMenuModelVersion;}
            _nativeMainMenuScreen.Show(_nativeMenuState,SourceDevicePixelRatio,_nativeMenuCharacterColor);

            return true;
        }
        // The native owner supplies the full accepted background/core/icon layers.
        public NativeMainMenuView CreateNativeMainMenuView(VisualElement parent)
        {
            if(!_mainMenu||_regionSelect||_mainMenuPage!=MainMenuPage.Home)
                throw new InvalidOperationException("Native main menu must be opened from the existing Home state");
            return new NativeMainMenuView(parent,NavigateNativeMainMenu,
                value=>{_playerName=value;PlayerPrefs.SetString(NamePreferenceKey,value);PlayerPrefs.Save();_nativeMenuModelVersion++;},
                value=>{PlayerPrefs.SetString(DifficultyPreferenceKey,value);PlayerPrefs.Save();_nativeMenuModelVersion++;});
        }

        // Read on entry or a real preference/save change, not on every frame.
        public NativeMainMenuView.State ReadNativeMainMenuState()
        {
            var snapshot=LoadRunSnapshot();return ReadNativeMainMenuState(snapshot==null?(double?)null:Math.Floor(snapshot.time));
        }
        private NativeMainMenuView.State ReadNativeMainMenuState(double? savedRunTime)
        {
            var ru=IsRussianUi;var lang=ru?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            var character=CharacterRules.Get(_sphereCharacterId);
            return new NativeMainMenuView.State(ru,_playerName,ru?character.NameRu:character.NameEn,ru?character.RoleRu:character.RoleEn,
                PlayerPrefs.GetString(DifficultyPreferenceKey,"normal"),T("difficulty"),T("namePlaceholder"),T("shop"),T("leaderboard"),T("achievements"),T("settings"),
                savedRunTime);
        }

        private void NavigateNativeMainMenu(NativeMainMenuView.Destination destination)
        {
            switch(destination){
                case NativeMainMenuView.Destination.Map:_regionSelect=true;break;
                case NativeMainMenuView.Destination.Characters:_mainMenuPage=MainMenuPage.Characters;break;
                case NativeMainMenuView.Destination.Shop:_mainMenuPage=MainMenuPage.Shop;break;
                case NativeMainMenuView.Destination.Leaderboard:_mainMenuPage=MainMenuPage.Leaderboard;break;
                case NativeMainMenuView.Destination.Achievements:_mainMenuPage=MainMenuPage.Achievements;break;
                case NativeMainMenuView.Destination.Knowledge:_mainMenuPage=MainMenuPage.Knowledge;break;
                case NativeMainMenuView.Destination.Settings:_mainMenuPage=MainMenuPage.Settings;break;
                case NativeMainMenuView.Destination.Continue:ContinueSavedRun();break;
                default:throw new ArgumentOutOfRangeException(nameof(destination));
            }
            if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
    }
}
