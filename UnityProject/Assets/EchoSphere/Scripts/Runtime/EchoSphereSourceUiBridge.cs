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
        private SourceUiDocument _sourceUi;
        private SourceSettingsToolkitScreen _nativeSettingsScreen;
        private bool _nativeSettingsWasActive;
        private bool _sourceUiWasPaused;
        private V _sourceUiStateRef,_sourceUiCanvasRef,_sourceUiTutorialRef;
        private V _sourceUiSavedRunTime;
        private void DrawSourceDocument()
        {
            if(_sourceUi==null){
                _sourceUi=new GameObject("Source TSX interface").AddComponent<SourceUiDocument>();_sourceUi.Initialize();
                _sourceUiStateRef=VisualJs.Obj("current",_sourceRenderState);_sourceUiCanvasRef=VisualJs.Obj("current",VisualJs.Null);_sourceUiTutorialRef=VisualJs.Obj("current",VisualJs.Null);
                var saved=LoadRunSnapshot();_sourceUiSavedRunTime=saved==null?VisualJs.Null:new V(Math.Floor(saved.time));
                _sourceUi.Render=()=>ProjectNativeUpgrade(ProjectNativeRunResult(ProjectNativePauseScreen(ProjectNativeHudIsland(SourceUiTree()))));_sourceUi.AfterEvent=ApplySourceUiWrites;
                _sourceUi.BeforePaint=root=>{PrepareNativeHudIsland(root);PrepareNativePauseScreen(root);PrepareNativePauseChromeIsland(root);PrepareNativePauseStatsIsland(root);PrepareNativePauseAbilitiesIsland(root);PrepareNativePauseArtifactsIsland(root);PrepareNativePauseSynergiesIsland(root);CompleteNativePauseScreen();PrepareNativeRunResult(root);PrepareNativeUpgrade(root);};
                _sourceUi.CanvasTexture=()=>_sourceCanvas.UiTexture;
                var environment=_sourceUi.Environment;
                environment.ReadStorage=key=>PlayerPrefs.HasKey(key)?new V(PlayerPrefs.GetString(key)):VisualJs.Null;
                environment.WriteStorage=(key,value)=>{PlayerPrefs.SetString(key,value.Text);PlayerPrefs.Save();};
                environment.RemoveStorage=key=>{PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();};
                environment.Reload=()=>SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                VisualUiBindings.Resolve=SourceUiBinding;
            }
            if(_sourceUiWasPaused!=_userPaused){_sourceUiWasPaused=_userPaused;_sourceUi.Dirty=true;}
            _sourceUiStateRef["current"]=_sourceRenderState;UpdateSourceUiState();
            // Active HUD data updates at 10 Hz. Paused simulation data is stable;
            // actions, pointer events and resize invalidate immediately. Motion,
            // native controls, layout, world paint and backdrop filters still tick.
            _sourceUi.Continuous=false;_sourceUi.RefreshInterval=_mainMenu||_userPaused?0f:0.1f;_sourceUi.Draw(SourceDevicePixelRatio);
        }
        private V SourceUiTree()
        {
            var lang=IsRussianUi?"ru":"en";
            V Action(Action<V[]> action)=>VisualJs.Fn(args=>{action(args);return default;});
            void Page(MainMenuPage page){_mainMenuPage=page;_regionSelect=false;}
            var props=VisualJs.Obj("lang",lang,"t",VisualJs.Fn(args=>Visual_i18n.Scope["translations"][lang][args[0]]),
                "onBack",Action(args=>{_regionSelect=false;Page(MainMenuPage.Home);}),
                "onExit",Action(args=>{ClearRunSnapshot();PlayerPrefs.SetInt("echo-sphere:open_region_select",1);PlayerPrefs.Save();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}),
                "gold",GetGold(),"shop",VisualJs.Obj("gold",GetGold(),"upgrades",RenderValue(_shopSave.upgrades)),
                "difficulty",PlayerPrefs.GetString(DifficultyPreferenceKey,"normal"),"soundOn",SourceAudio2D.IsEnabled,"handedness",IsLeftHanded?"left":"right",
                "setDifficulty",Action(args=>{PlayerPrefs.SetString(DifficultyPreferenceKey,args[0].Text);PlayerPrefs.Save();}),
                "setLang",Action(args=>{PlayerPrefs.SetString(LanguagePreferenceKey,args[0].Text);PlayerPrefs.Save();}),
                "setSoundOn",Action(args=>{PlayerPrefs.SetInt(SoundPreferenceKey,args[0].Truth?1:0);PlayerPrefs.Save();SourceAudio2D.SetEnabled(args[0].Truth);}),
                "setHandedness",Action(args=>{PlayerPrefs.SetString(HandednessPreferenceKey,args[0].Text);PlayerPrefs.Save();}),
                "onReplayTutorial",Action(args=>{PlayerPrefs.SetInt(TutorialCompletedPreferenceKey,0);PlayerPrefs.Save();Page(MainMenuPage.Home);}),
                "onGoldChange",Action(args=>SaveGold((int)args[0].Number)),
                "setShop",Action(args=>SourceUiBinding("persistence.ts","saveShop").Call(args[0])));
            if(_mainMenu){
                if(_regionSelect){props["onStartRun"]=Action(args=>{
                    var region=args[0].Text=="spectral_rift"?EchoRegionId.SpectralRift:EchoRegionId.ResonanceBasin;
                    PlayerPrefs.SetInt(SelectedRegionPreferenceKey,(int)region);PlayerPrefs.SetString("echo-sphere:pending_region_challenge",args[2].Text);PlayerPrefs.SetInt("echo-sphere:pending_region_endless",args[1].Text=="endless"?1:0);PlayerPrefs.SetInt("echo-sphere:start_selected_region_after_reload",1);PlayerPrefs.Save();
                    // Construct fresh player/spheres with the selected character
                    // and purchased upgrades, as createInitialState does.
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });return VisualDom.Component("EchoMapScreen",Visual_EchoMapScreen.Scope["EchoMapScreen"],props);}
                var name=_mainMenuPage==MainMenuPage.Shop?"ShopScreen":_mainMenuPage==MainMenuPage.Settings?"SettingsScreen":_mainMenuPage==MainMenuPage.Leaderboard?"LeaderboardScreen":_mainMenuPage==MainMenuPage.Achievements?"AchievementsScreen":_mainMenuPage==MainMenuPage.Characters?"CharacterSelect":_mainMenuPage==MainMenuPage.Knowledge?"KnowledgeBase":"Menu";
                props["onOpenMap"]=Action(args=>_regionSelect=true);props["onShop"]=Action(args=>Page(MainMenuPage.Shop));props["onSettings"]=Action(args=>Page(MainMenuPage.Settings));props["onLeader"]=Action(args=>Page(MainMenuPage.Leaderboard));props["onAchievements"]=Action(args=>Page(MainMenuPage.Achievements));props["onCharacters"]=Action(args=>Page(MainMenuPage.Characters));props["onKnowledge"]=Action(args=>Page(MainMenuPage.Knowledge));
                props["savedRunTime"]=_sourceUiSavedRunTime;props["onContinue"]=Action(args=>ContinueSavedRun());
                var module=name=="CharacterSelect"?Visual_CharacterSelect.Scope:name=="KnowledgeBase"?Visual_KnowledgeBase.Scope:Visual_App.Scope;
                var tree=VisualDom.Component(name,module[name],props);if(name=="SettingsScreen")InstallSourceScaleControl(tree);return tree;
            }
            props["st"]=_sourceRenderState;props["stateRef"]=_sourceUiStateRef;props["canvasRef"]=_sourceUiCanvasRef;
            props["tutorialStep"]=_tutorialStep<0?VisualJs.Null:new V(_tutorialStep);_sourceUiTutorialRef["current"]=props["tutorialStep"];props["tutorialStepRef"]=_sourceUiTutorialRef;
            props["tutorialStartPosRef"]=VisualJs.Obj("current",SourcePoint((Vector2)_tutorialStartPosition));
            props["paused"]=_userPaused;props["setPaused"]=Action(args=>SetUserPaused(args[0].Truth));
            props["transitionTutorial"]=Action(args=>{if(args[0].Null)FinishTutorial(false);else SetTutorialStep((int)args[0].Number);});
            props["setTutorialStep"]=Action(args=>{if(args[0].Null)FinishTutorial(true);else SetTutorialStep((int)args[0].Number);});
            var tabs=new[]{"stats","skills","artifacts","synergies"};props["pauseTab"]=tabs[_pausePlannerTab];props["setPauseTab"]=Action(args=>_pausePlannerTab=Math.Max(0,Array.IndexOf(tabs,args[0].Text)));
            // Skip compatibility content before the generated component builds
            // it. Post-build pruning alone still paid for every discarded card.
            props["nativePauseContent"]=UsesNativePauseStats||UsesNativePauseAbilities||UsesNativePauseArtifacts||NativePauseSynergiesRequested;
            props["nativePauseChrome"]=UsesNativePauseChrome;
            props["bindNativePauseChrome"]=Action(args=>{_nativePauseChromeResume=args[0];_nativePauseChromeExit=args[1];_nativePauseChromeSelect=args[2];});
            props["selectedArtifactSetId"]=_pauseArtifactSetId==null?VisualJs.Null:new V(_pauseArtifactSetId);props["setSelectedArtifactSetId"]=Action(args=>_pauseArtifactSetId=args[0].Null?null:args[0].Text);
            props["gameOverData"]=_gameOver?VisualJs.Obj("time",Math.Floor(_runTime),"wave",_wave,"kills",_kills,"gold",_runGoldEarned,"rank",_sourceResultRank,"isNewRecord",_sourceResultNewRecord,"regionClear",_regionStabilized,"challengeProgress",GetCompletedRegionChallenges(EchoRegionId.ResonanceBasin).Length,"endlessUnlocked",IsRegionEndlessUnlocked(EchoRegionId.ResonanceBasin),"masteryChallengesCompleted",_regionMasteryChallengesCompleted):VisualJs.Null;
            return VisualDom.Component("SourceGameView",Visual_App.Scope["SourceGameView"],props);
        }
        private void InstallSourceScaleControl(V tree)
        {
            IEnumerable<VisualDom.Node> Descendants(V value){if(value.Raw is VisualDom.Node node){yield return node;foreach(var child in node.Children)foreach(var nested in Descendants(child))yield return nested;}else if(value.Raw is List<V>)foreach(var child in value.Items)foreach(var nested in Descendants(child))yield return nested;}
            var labels=Descendants(tree).Where(node=>node.Tag=="label").ToList();
            bool HasText(VisualDom.Node node,string ru,string en)=>string.Concat(node.Children.Select(value=>value.Text)).Trim()==ru||string.Concat(node.Children.Select(value=>value.Text)).Trim()==en;
            var controlsLabel=labels.FirstOrDefault(node=>HasText(node,"Управление","Controls"));if(controlsLabel==null)return;
            VisualDom.Node Parent(VisualDom.Node target)=>Descendants(tree).FirstOrDefault(node=>node.Children.Any(value=>ReferenceEquals(value.Raw,target)));
            var controlsSection=Parent(Parent(controlsLabel));var container=Parent(controlsSection);if(container==null)return;
            var control=VisualDom.Component("SourceInterfaceScaleMarkup",Visual_App.Scope["SourceInterfaceScaleMarkup"],VisualJs.Obj());
            var scale=Visual_interfaceScale.Scope["loadInterfaceScale"].Call();foreach(var node in Descendants(control)){
                if(!node.Properties["data-interface-scale-title"].Undefined)node.Children.Add(IsRussianUi?"Масштаб интерфейса":"Interface scale");
                if(!node.Properties["data-interface-scale-value"].Undefined)node.Children.Add(Visual_interfaceScale.Scope["formatInterfaceScale"].Call(scale));
                if(!node.Properties["data-interface-scale-range"].Undefined){node.Properties["value"]=scale;node.Properties["onInput"]=VisualJs.Fn(args=>{Visual_interfaceScale.Scope["saveInterfaceScale"].Call(VisualJs.Number(args[0]["target"]["value"]));return default;});}
            }
            container.Children.Insert(container.Children.FindIndex(value=>ReferenceEquals(value.Raw,controlsSection)),control);
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
            if(module=="global")return _sourceUi.Environment.Get(name);
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
                    case "resetAll":foreach(var key in new[]{GoldPreferenceKey,ShopPreferenceKey,LeaderboardPreferenceKey,AchievementPreferenceKey,DifficultyPreferenceKey,SoundPreferenceKey,CharacterPreferenceKey,CharacterProfilesPreferenceKey,HandednessPreferenceKey,LanguagePreferenceKey,NamePreferenceKey,"echosphere_knowledge_v1",TutorialCompletedPreferenceKey})PlayerPrefs.DeleteKey(key);foreach(var region in new[]{EchoRegionId.ResonanceBasin,EchoRegionId.SpectralRift}){PlayerPrefs.DeleteKey(RegionStabilizedKey(region));PlayerPrefs.DeleteKey(RegionChallengesKey(region));PlayerPrefs.DeleteKey(RegionEndlessKey(region));}ClearRunSnapshot();PlayerPrefs.Save();return default;
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
                    case "activateByKey":foreach(var binding in _activeAbilityHotkeys)if(binding.Value.ToString().Replace("Alpha","").ToLowerInvariant()==args[1].Text)ActivateActiveAbility(binding.Key);break;
                    case "activateDash":if(_playerCore.TryDash(SourceUiMovement()))SourceAudio2D.Play("dash");break;
                    case "setSphereType":if(Enum.TryParse<SphereId>(args[1].Text,true,out var sphereType)){_selectedSphereType=sphereType;SourceAudio2D.Play("place");}break;
                    case "placeSphere":TryPlaceOrRemoveSphereAt(new Vector2((float)args[1].Number,-(float)args[2].Number)*SpherePlacementRules.SourcePixelsToWorld);break;
                    case "repositionSphere":var sphere=_spheres.Find(agent=>agent!=null&&RenderEntity(agent)==args[1]);if(sphere==null)return false;var position=new Vector2((float)args[2].Number,-(float)args[3].Number)*SpherePlacementRules.SourcePixelsToWorld;if(!CanPlaceSphereAt(position,sphere))return false;var clamped=SpherePlacementRules.ClampReposition(new Vec2(position.x,position.y));sphere.transform.position=new Vector3(clamped.X,clamped.Y,sphere.transform.position.z);sphere.SetFollowMode(_follow,(Vector2)_player.position);RefreshNetworkState();UpdateSourceRenderState();return true;
                    case "canActivateFormationFollow":return FormationFollowRules.CanActivate(new Vec2(_player.position.x,_player.position.y),_spheres.Where(agent=>agent!=null).Select(agent=>new FormationNode(new Vec2(agent.transform.position.x,agent.transform.position.y),true)).ToList());
                    case "setFormationFollow":var followed=SetFormationFollowMode(args[1].Truth);UpdateSourceRenderState();UpdateSourceUiState();return followed;
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
