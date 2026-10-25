using System;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeCharacterScreen _nativeCharacterScreen;
        private NativeCharacterScreen.State _nativeCharacterState;
        private bool _nativeCharacterWasActive;
        private string _nativeCharacterMessage="";
        private bool TryDrawNativeCharacters()
        {
            if(!_mainMenu||_regionSelect||_mainMenuPage!=MainMenuPage.Characters){
                if(_nativeCharacterWasActive){_nativeCharacterScreen.Hide();_nativeCharacterWasActive=false;if(_sourceUi!=null&&!UsesNativeMainMenu&&!_nativeProgressionWasActive&&!_nativeSettingsWasActive&&!_nativeRegionWasActive){_sourceUi.Dirty=true;}}
                return false;
            }
            if(!_nativeCharacterWasActive){
                if(_nativeCharacterScreen==null)_nativeCharacterScreen=AcquireNativeScreen<NativeCharacterScreen>("Native Character Selection UI Toolkit");
                _expandedCharacterId=CharacterRules.ToSourceId(_sphereCharacterId);_nativeCharacterMessage="";
                _nativeCharacterState=ReadNativeCharacterState();_nativeCharacterWasActive=true;
                _camera.cullingMask=0;
            }
            _nativeCharacterScreen.Show(_nativeCharacterState,SourceDevicePixelRatio,()=>_mainMenuPage=MainMenuPage.Home,ChooseNativeCharacter,ExpandNativeCharacter);
            return true;
        }
        private void ChooseNativeCharacter(string id)
        {
            if(!_mainMenu||_mainMenuPage!=MainMenuPage.Characters||!CharacterRules.TryParseSourceId(id,out var character))return;
            var profile=GetCharacterProfile(character);var definition=CharacterRules.Get(character);
            if(!profile.unlocked){
                if(GetGold()<definition.UnlockCost){_nativeCharacterMessage=IsRussianUi?"Недостаточно золота.":"Not enough gold.";_nativeCharacterState=ReadNativeCharacterState();return;}
                profile.unlocked=true;SaveCharacterProfiles();SaveGold(GetGold()-definition.UnlockCost);
            }
            // Match CharacterSelect choose/unlock + saveCharacterId binding.
            // Do not call the old IMGUI helper: it also rewrites prepared HP and
            // starter spheres, which the actual TSX selection does not do.
            _sphereCharacterId=character;_characterMasteryLevel=profile.masteryLevel;
            PlayerPrefs.SetString(CharacterPreferenceKey,id);PlayerPrefs.Save();
            _expandedCharacterId=id;_nativeCharacterMessage="";_nativeMenuModelVersion++;
            _nativeCharacterState=ReadNativeCharacterState();
        }
        private void ExpandNativeCharacter(string id)
        {
            if(!_mainMenu||_mainMenuPage!=MainMenuPage.Characters)return;
            _expandedCharacterId=_expandedCharacterId==id?"":id;_nativeCharacterState=ReadNativeCharacterState();
        }
        private NativeCharacterScreen.State ReadNativeCharacterState()
        {
            EnsureCharacterProfilesLoaded();bool ru=IsRussianUi;
            string L(string a,string b)=>ru?a:b;
            var rows=new List<NativeCharacterScreen.Character>();
            foreach(var definition in CharacterRules.All){
                var profile=GetCharacterProfile(definition.Id);string id=CharacterRules.ToSourceId(definition.Id);
                int next=CharacterRules.GetNextMasteryThreshold(profile.masteryLevel),previous=profile.masteryLevel<=1?0:CharacterRules.GetMasteryThreshold(profile.masteryLevel-1);
                var mastery=new List<NativeCharacterScreen.Mastery>();string currentTitle="",nextTitle="",nextDescription="";
                foreach(var level in definition.Mastery){
                    string name=L(level.TitleRu,level.TitleEn),description=L(level.DescriptionRu,level.DescriptionEn);
                    if(level.Level==profile.masteryLevel)currentTitle=name;
                    if(level.Level==profile.masteryLevel+1){nextTitle=name;nextDescription=description;}
                    mastery.Add(new NativeCharacterScreen.Mastery {Title=level.Level+". "+name,Description=description,Threshold=CharacterRules.GetMasteryThreshold(level.Level)+" XP",Reached=level.Level<=profile.masteryLevel,Current=level.Level==profile.masteryLevel});
                }
                var affinities=new List<NativeCharacterScreen.Affinity>();var types=new[]{definition.SignatureSphere,definition.PartnerSphere};
                for(int i=0;i<types.Length;i++){var sphere=GameCatalog.Spheres[(int)types[i]];affinities.Add(new NativeCharacterScreen.Affinity {Text=(i==0?L("СИГНАТУРНАЯ","SIGNATURE"):L("ПАРТНЁР","PARTNER"))+" · "+L(sphere.DisplayNameRu,sphere.DisplayName),Color=sphere.ColorHex});}
                rows.Add(new NativeCharacterScreen.Character {Id=id,Name=L(definition.NameRu,definition.NameEn),Role=L(definition.RoleRu,definition.RoleEn).ToUpperInvariant(),Description=L(definition.DescriptionRu,definition.DescriptionEn),Mechanic=L(definition.MechanicRu,definition.MechanicEn),Color=definition.ColorHex,
                    Unlocked=profile.unlocked,Selected=_sphereCharacterId==definition.Id,Expanded=_expandedCharacterId==id,Complete=string.IsNullOrEmpty(nextTitle),Affinities=affinities,Masteries=mastery,
                    MasteryLabel=L("Мастерство ","Mastery ")+profile.masteryLevel+"/10"+(currentTitle.Length>0?" · "+currentTitle:""),MasteryXp=next<0?L("МАКС","MAX"):profile.masteryXp+"/"+next+" XP",
                    MasteryPercent=next<0?100:Mathf.Clamp01((profile.masteryXp-previous)/(float)Math.Max(1,next-previous))*100,
                    Action=profile.unlocked?(_sphereCharacterId==definition.Id?L("Выбран","Selected"):L("Выбрать","Select")):"🪙 "+definition.UnlockCost,
                    Toggle=_expandedCharacterId==id?L("Скрыть мастерство","Hide mastery"):L("Показать мастерство","Show mastery"),
                    NextLabel=string.IsNullOrEmpty(nextTitle)?L("Мастерство полностью развито.","Mastery fully developed."):L("Следующий уровень · ещё ","Next level · ")+Math.Max(0,next-profile.masteryXp)+L(" XP"," XP remaining"),NextTitle=nextTitle,NextDescription=nextDescription});
            }
            return new NativeCharacterScreen.State {Russian=ru,Gold="🪙 "+GetGold(),Message=_nativeCharacterMessage,Characters=rows};
        }
    }
}
