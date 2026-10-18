using System;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Explicit owner API, not an automatic production renderer switch.
        // The composition owner must supply accepted background/core/icon layers
        // before replacing the currently selected menu.
        public NativeMainMenuView CreateNativeMainMenuView(VisualElement parent)
        {
            if(!_mainMenu||_regionSelect||_mainMenuPage!=MainMenuPage.Home)
                throw new InvalidOperationException("Native main menu must be opened from the existing Home state");
            return new NativeMainMenuView(parent,NavigateNativeMainMenu,
                value=>{_playerName=value;PlayerPrefs.SetString(NamePreferenceKey,value);PlayerPrefs.Save();},
                value=>{PlayerPrefs.SetString(DifficultyPreferenceKey,value);PlayerPrefs.Save();});
        }

        // Read on entry or a real preference/save change, not on every frame.
        public NativeMainMenuView.State ReadNativeMainMenuState()
        {
            var ru=IsRussianUi;var lang=ru?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            var character=CharacterRules.Get(_sphereCharacterId);
            var snapshot=LoadRunSnapshot();
            return new NativeMainMenuView.State(ru,_playerName,ru?character.NameRu:character.NameEn,ru?character.RoleRu:character.RoleEn,
                PlayerPrefs.GetString(DifficultyPreferenceKey,"normal"),T("difficulty"),T("namePlaceholder"),T("shop"),T("leaderboard"),T("achievements"),T("settings"),
                snapshot==null?(double?)null:Math.Floor(snapshot.time));
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
