using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeUiDataContext _sourceUi;
        private SourceSettingsToolkitScreen _nativeSettingsScreen;
        private bool _nativeSettingsWasActive;
        private bool _sourceUiWasPaused;
        private void DrawSourceDocument()
        {
            if(_sourceUi==null){
                _sourceUi=new NativeUiDataContext();
                var environment=_sourceUi;
                environment.ReadStorage=key=>PlayerPrefs.HasKey(key)?new V(PlayerPrefs.GetString(key)):VisualJs.Null;
                environment.WriteStorage=(key,value)=>{PlayerPrefs.SetString(key,value.Text);PlayerPrefs.Save();};
                environment.RemoveStorage=key=>{PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();};
                environment.Reload=()=>SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                VisualUiBindings.Resolve=SourceUiBinding;
            }
            if(_sourceUiWasPaused!=_userPaused){_sourceUiWasPaused=_userPaused;_sourceUi.Dirty=true;}
            UpdateSourceUiState();
            if(!_mainMenu){TickNativeMobileTransients();DrawNativeRunDocument();return;}
            // Every production menu has its own retained native screen. There
            // is no generated JSX/DOM fallback in the active runtime anymore.

        }
        private void UpdateSourceUiState()
        {
            EnsureShopSaveLoaded();var scene=_sourceRenderState;
            var hotkeys=VisualJs.Obj();foreach(var pair in _activeAbilityHotkeys)hotkeys[pair.Value.ToString().Replace("Alpha","").ToLowerInvariant()]=SourceAbilityId(pair.Key);scene["activeKeyMap"]=hotkeys;
            scene["selectedSphereType"]=_selectedSphereType.ToString().ToLowerInvariant();var player=scene["player"];player["sphereMovementLocked"]=_sphereMovementLocked;
            scene["bossActive"]=VisualJs.Null;foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive&&enemy.IsBoss){scene["bossActive"]=RenderEntity(enemy);break;}
            var choices=new List<V>();
            if(_levelUp){
                if(_choosingEvolution){
                    var sourceChoices=Visual_engineProgression.Scope[_pendingEvolutionIsAbility?"getAbilityEvolutionChoices":"getSphereEvolutionChoices"].Call(scene,_pendingEvolutionIsAbility?SourceAbilityId(_pendingEvolutionAbility):_pendingEvolutionSphere.ToString().ToLowerInvariant(),_pendingFinalEvolution?7:4);
                    foreach(var choice in sourceChoices.Items)choices.Add(choice);
                    if(choices.Count!=_evolutionChoices.Count)throw new InvalidOperationException("Source evolution cards and native choices differ");
                }else foreach(var choice in _levelUpChoices){
                    var text=SourceUpgradeText(choice);if(text==null)throw new InvalidOperationException("Missing source upgrade text: "+GetUpgradeChoiceKey(choice));
                    var value=VisualJs.Obj("type",choice.Kind==RunUpgradeKind.Sphere?"sphere":"ability","currentLevel",choice.CurrentLevel,"newLevel",choice.NextLevel,"name",VisualJs.Obj("ru",text.nameRu,"en",text.nameEn),"desc",VisualJs.Obj("ru",text.descRu,"en",text.descEn));
                    if(choice.Kind==RunUpgradeKind.Sphere){value["sphereType"]=choice.Sphere.ToString().ToLowerInvariant();value["sphereStage"]="upgrade";var branch=GetSphereBranch(choice.Sphere);if(!string.IsNullOrEmpty(branch))value["sphereBranch"]=branch;}
                    else {value["ability"]=SourceAbilityId(choice.Kind);value["name"]=Visual_gameData.Scope["ABILITIES"][value["ability"]]["name"];}
                    choices.Add(value);
                }
            }
            for(var index=0;index<choices.Count;index++){var choice=choices[index];choice["nativeIndex"]=index;}
            scene["pendingUpgrade"]=_levelUp?VisualJs.Arr(choices.ToArray()):VisualJs.Null;
            scene["pendingArtifact"]=_stellaRewardPending?VisualJs.Arr(_stellaRewardChoices.Select(choice=>new V(choice.Id)).ToArray()):VisualJs.Null;scene["pendingStella"]=VisualJs.Null;
            scene["levelUpRerollsRemaining"]=_levelUpRerollsRemaining;scene["levelUpLocksRemaining"]=_levelUpLocksRemaining;
            scene["levelUpLockChoiceKey"]=_levelUpLockChoiceKey==null?VisualJs.Null:new V(_levelUpLockChoiceKey);scene["oracleForecastKeys"]=RenderValue(_oracleForecastKeys);
        }
        private V SourceUiBinding(string module,string name)
        {
            if(module=="global")return _sourceUi.Get(name);
            if(module=="persistence.ts")return VisualJs.Fn(args=>{
                switch(name){
                    case "loadName":return _playerName;case "saveName":_playerName=args[0].Text;PlayerPrefs.SetString(NamePreferenceKey,_playerName);PlayerPrefs.Save();return default;
                    case "loadCharacterId":return CharacterRules.ToSourceId(_sphereCharacterId);
                    case "saveCharacterId":if(CharacterRules.TryParseSourceId(args[0].Text,out var selected)){_sphereCharacterId=selected;PlayerPrefs.SetString(CharacterPreferenceKey,args[0].Text);PlayerPrefs.Save();_characterMasteryLevel=GetCharacterProfile(selected).masteryLevel;}return default;
                    case "loadCharacterProfiles":EnsureCharacterProfilesLoaded();return RenderValue(_characterProfiles);
                    case "saveCharacterProfiles":_characterProfiles=args[0].Items.Select(value=>new CharacterProfileData {id=value["id"].Text,unlocked=value["unlocked"].Truth,masteryLevel=(int)value["masteryLevel"].Number,masteryXp=(int)value["masteryXp"].Number}).ToList();SaveCharacterProfiles();return default;
                    case "loadKnowledge":EnsureKnowledgeLoaded();return RenderValue(_knownKnowledgeIds);
                    case "loadAchievements":return RenderValue(LoadUnlockedAchievementIds());
                    case "loadLeaderboard":return RenderValue(LoadLeaderboardEntries());
                    case "saveGold":SaveGold((int)args[0].Number);return default;
                    case "saveShop":EnsureShopSaveLoaded();foreach(var id in PersistentUpgradeRules.Ids)_shopSave.upgrades.Set(id,args[0]["upgrades"][id].Undefined?0:(int)args[0]["upgrades"][id].Number);SaveShopState();return default;
                    case "saveTutorialCompleted":PlayerPrefs.SetInt(TutorialCompletedPreferenceKey,args[0].Truth?1:0);PlayerPrefs.Save();return default;
                    case "resetAll":NativeResetAllProgress();return default;
                    default:throw new InvalidOperationException("Unbound source persistence action "+name);
                }
            });
            if(module=="region.ts")return VisualJs.Fn(args=>{var region=args.Length>0&&args[0].Text=="spectral_rift"?EchoRegionId.SpectralRift:EchoRegionId.ResonanceBasin;switch(name){case "loadRegionStabilized":return IsRegionStabilized(region);case "loadRegionEndlessUnlock":return IsRegionEndlessUnlocked(region);case "loadRegionChallengeCompletions":return RenderValue(GetCompletedRegionChallenges(region));default:throw new InvalidOperationException("Unbound source region state "+name);}});
            if(module=="runPersistence.ts"&&name=="saveRunSnapshot")return VisualJs.Fn(args=>{SaveCurrentRun();return default;});
            if(module=="audio.ts")return VisualJs.Fn(args=>{if(name=="playSound")SourceAudio2D.Play(args[0].Text);else if(name=="setAudioEnabled")SourceAudio2D.SetEnabled(args[0].Truth);else throw new InvalidOperationException("Unbound source audio "+name);return default;});
            return VisualJs.Fn(args=>{
                switch(name){
                    case "applyUpgrade":if(_choosingEvolution)ChooseEvolution((int)args[1]["nativeIndex"].Number);else ChooseUpgrade((int)args[1]["nativeIndex"].Number);break;
                    case "lockUpgradeChoice":return LockLevelUpChoice((int)args[1]["nativeIndex"].Number);
                    case "rerollUpgradeChoices":return RerollLevelUpChoices();
                    case "applyArtifact":var reward=_stellaRewardChoices.FindIndex(choice=>choice.Id==args[1].Text);if(reward>=0)ChooseStellaReward(reward);break;
                    case "activateByKey":NativeActivateHotkey(args[1].Text);return default;
                    case "activateDash":NativeActivateDash();return default;
                    case "setSphereType":NativeSelectSphere(args[1].Text);return default;
                    case "placeSphere":NativePlaceOrRemoveSphere(args[1].Number,args[2].Number);return default;
                    case "repositionSphere":return NativeRepositionSphere(NativeSphereForView(args[1]),args[2].Number,args[3].Number);
                    case "canActivateFormationFollow":return NativeCanActivateFollow();
                    case "setFormationFollow":return NativeSetFormationFollow(args[1].Truth);
                    default:throw new InvalidOperationException("Unbound source UI action "+module+"/"+name);
                }
                UpdateSourceRenderState();UpdateSourceUiState();return default;
            });
        }
        private Vector2 SourceUiMovement()
        {if(_sourceRenderState.Undefined)return Vector2.zero;var keys=_sourceRenderState["keys"];return new Vector2((keys["d"].Truth||keys["arrowright"].Truth?1f:0f)-(keys["a"].Truth||keys["arrowleft"].Truth?1f:0f),(keys["w"].Truth||keys["arrowup"].Truth?1f:0f)-(keys["s"].Truth||keys["arrowdown"].Truth?1f:0f));}
        private void ApplySourceUiWrites()
        {
            if(_sourceRenderState.Undefined)return;_sphereMovementLocked=_sourceRenderState["player"]["sphereMovementLocked"].Truth;
            var selection=_sourceRenderState["networkFormationSelection"];if(!selection.Null&&!selection.Undefined&&Enum.TryParse<SphereNetworkFormation>(selection["dominant"].Text,true,out var dominant)&&Enum.TryParse<SphereNetworkFormation>(selection["secondary"].Text,true,out var secondary)){_preferredDominantFormation=dominant;_preferredSecondaryFormation=secondary;RefreshNetworkState();_sourceRenderState["networkFormationSelection"]=VisualJs.Null;}
            var flash=_sourceRenderState["flashText"];if(!flash.Null&&!flash.Undefined&&flash["text"].Text!=_message)ShowTimedCombatMessage(flash["text"].Text,(float)flash["life"].Number,flash["color"].Text);
        }
    }
}
