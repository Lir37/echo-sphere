using System;
using System.Collections.Generic;
using System.Globalization;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeProgressionScreen _nativeProgressionScreen;
        private NativeProgressionCatalog _nativeProgressionCatalog;
        private MainMenuPage _nativeProgressionPage;
        private bool _nativeProgressionWasActive;
        private NativeProgressionScreen.State _nativeProgressionState;
        private bool TryDrawNativeProgression()
        {
            bool selected=_mainMenu&&!_regionSelect&&(_mainMenuPage==MainMenuPage.Shop||_mainMenuPage==MainMenuPage.Leaderboard||_mainMenuPage==MainMenuPage.Achievements);
            if(!selected){if(_nativeProgressionWasActive){_nativeProgressionScreen.Hide();_nativeProgressionWasActive=false;if(_sourceUi!=null){if(!UsesNativeMainMenu)_sourceUi.SetPresentationSuspended(false);_sourceUi.Dirty=true;}}return false;}
            if(!_nativeProgressionWasActive||_nativeProgressionPage!=_mainMenuPage){
                if(_nativeProgressionScreen==null)_nativeProgressionScreen=new GameObject("Native Progression UI Toolkit").AddComponent<NativeProgressionScreen>();
                _nativeProgressionPage=_mainMenuPage;_nativeProgressionState=ReadNativeProgressionState();_nativeProgressionWasActive=true;
                if(_nativeSettingsScreen!=null)_nativeSettingsScreen.Hide();
                if(_sourceUi!=null)_sourceUi.SetPresentationSuspended(true);
                _camera.cullingMask=0;
            }
            _nativeProgressionScreen.Show(_nativeProgressionState,SourceDevicePixelRatio,BackFromNativeProgression,BuyNativeShopUpgrade);
            return true;
        }
        private void BackFromNativeProgression(){_mainMenuPage=MainMenuPage.Home;_nativeMenuModelVersion++;if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void BuyNativeShopUpgrade(string id)
        {
            // Same guards, cost and save schema as App.tsx buy. UI never mutates
            // gameplay stats directly; a new run reads the existing shop save.
            if(!_mainMenu||_mainMenuPage!=MainMenuPage.Shop||Array.IndexOf(PersistentUpgradeRules.Ids,id)<0)return;
            int level=GetShopUpgradeLevel(id),cost=PersistentUpgradeRules.GetCost(id,level),gold=GetGold();
            if(level>=PersistentUpgradeRules.GetMaxLevel(id)||cost<=0||gold<cost)return;
            _shopSave.upgrades.Set(id,level+1);SaveShopState();SaveGold(gold-cost);
            _nativeProgressionState=ReadNativeProgressionState();_nativeProgressionScreen.UpdateShop(_nativeProgressionState);
        }
        private NativeProgressionScreen.State ReadNativeProgressionState()
        {
            string lang=IsRussianUi?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            var state=new NativeProgressionScreen.State {Back=T("back")};
            if(_mainMenuPage==MainMenuPage.Shop){
                EnsureShopSaveLoaded();state.Page=NativeProgressionScreen.Page.Shop;state.Title=T("shopTitle");state.Gold=GetGold().ToString(CultureInfo.InvariantCulture);
                var items=new List<NativeProgressionScreen.ShopItem>();
                foreach(string id in PersistentUpgradeRules.Ids){int level=GetShopUpgradeLevel(id),max=PersistentUpgradeRules.GetMaxLevel(id),cost=PersistentUpgradeRules.GetCost(id,level);bool maxed=level>=max;
                    items.Add(new NativeProgressionScreen.ShopItem {Id=id,Name=GetShopUpgradeName(id),Description=GetShopUpgradeDescription(id),Owned=T("owned")+": "+level+"/"+max,Price=maxed?T("max"):cost+" ●",Maxed=maxed,CanBuy=!maxed&&GetGold()>=cost});
                }state.Shop=items;
            }else if(_mainMenuPage==MainMenuPage.Leaderboard){
                state.Page=NativeProgressionScreen.Page.Leaderboard;state.Title=T("leaderTitle");state.Empty=T("noScores");
                state.Rank=T("rank");state.Player=T("player");state.Wave=T("wave");state.Score=T("score");
                var scores=new List<NativeProgressionScreen.Score>();foreach(var entry in LoadLeaderboardEntries())scores.Add(new NativeProgressionScreen.Score {Name=entry.name,Wave=entry.wave.ToString(CultureInfo.InvariantCulture),Time=entry.time.ToString("G9",CultureInfo.InvariantCulture)+"s"});state.Scores=scores;
            }else{
                state.Page=NativeProgressionScreen.Page.Achievements;state.Title=T("achievementsTitle");
                if(_nativeProgressionCatalog==null)_nativeProgressionCatalog=Resources.Load<NativeProgressionCatalog>("EchoSphere/NativeProgressionCatalog");
                if(_nativeProgressionCatalog==null)throw new InvalidOperationException("Native progression catalog missing");
                var unlocked=LoadUnlockedAchievementIds();state.Count=unlocked.Count+"/"+_nativeProgressionCatalog.Achievements.Count;
                var rows=new List<NativeProgressionScreen.Achievement>();foreach(var definition in _nativeProgressionCatalog.Achievements){bool has=unlocked.Contains(definition.Id);rows.Add(new NativeProgressionScreen.Achievement {Id=definition.Id,Name=IsRussianUi?definition.NameRu:definition.NameEn,Description=IsRussianUi?definition.DescriptionRu:definition.DescriptionEn,Status=T(has?"achievementsUnlocked":"achievementsLocked"),Unlocked=has});}state.Achievements=rows;
            }
            return state;
        }
    }
}
