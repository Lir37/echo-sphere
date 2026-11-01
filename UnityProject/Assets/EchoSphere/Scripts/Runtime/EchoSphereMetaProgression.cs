using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private int _runShardsEarned,_runArchiveDataEarned;
  private readonly List<string> _pendingKnowledgeRewards=new List<string>();
  private void GrantCompletedRunCurrencies()
  {
            var difficulty = CurrentDifficulty;
            var goldFormula = System.Math.Floor(_runTime) * 0.15d + _bossesDefeated * 25d + _kills * 0.05d;
            _runGoldEarned = (int)System.Math.Floor(goldFormula * difficulty.GoldMultiplier);
            if (_regionStabilized)
            {
                // Exact src/region.ts reward: region base + 5 per elite + 35 per defeated boss.
                _runGoldEarned += RegionBossScheduleRules.GetBaseGoldReward(_selectedRegion) +
                    _runEliteKills * 5 + _bossesDefeated * 35;
            }
            _runGoldEarned=(int)Math.Min(int.MaxValue,(long)_runGoldEarned+RuneRules.CellReward(_runEconomy?.Runes));
            if(string.IsNullOrEmpty(_runMetaReceipt))_runMetaReceipt=Guid.NewGuid().ToString("N");
            _runShardsEarned=MetaCurrencyRules.RunShards(_bossesDefeated,_regionStabilized);
            GrantMetaOnce("run:"+_runMetaReceipt,_runGoldEarned,_runShardsEarned,0);

  }
  private void ClearRegionalProgressKeys()
  {
   for(int i=0;i<6;i++){var region=(EchoRegionId)i;PlayerPrefs.DeleteKey(RegionStabilizedKey(region));PlayerPrefs.DeleteKey(RegionChallengesKey(region));PlayerPrefs.DeleteKey(RegionEndlessKey(region));PlayerPrefs.DeleteKey(RegionMasteryKey(region));}
  }
  private void InitializeMetaProgression()
  {
   var wallet=ReadMetaWallet();const string migration="migration:meta-progression-v1";
   if(MetaCurrencyRules.HasReceipt(wallet,migration))return;
   EnsureKnowledgeLoaded();
   foreach(string id in _knownKnowledgeIds)if(!MetaCurrencyRules.HasReceipt(wallet,"purchase:lore:"+id))MetaCurrencyRules.TryGrantOnce(wallet,"discovery:"+id,0,0,MetaCurrencyRules.NaturalDiscoveryData);
   for(int i=0;i<6;i++){var region=(EchoRegionId)i;if(IsRegionStabilized(region))MetaCurrencyRules.TryGrantOnce(wallet,"legacy-clear:"+RegionSourceId(region),0,MetaCurrencyRules.RunShards(3,true),0);}
   MetaCurrencyRules.TryGrantOnce(wallet,migration,0,0,0);WriteMetaWallet(wallet);
  }
  private bool RecordNaturalKnowledge(string id)
  {if(!_knownKnowledgeIds.Add(id))return false;_pendingKnowledgeRewards.Add(id);return true;}
  private void PersistKnowledgeProgress()
  {
   var wallet=ReadMetaWallet();foreach(string id in _pendingKnowledgeRewards){
    if(MetaCurrencyRules.HasReceipt(wallet,"purchase:lore:"+id))continue;
    if(MetaCurrencyRules.TryGrantOnce(wallet,"discovery:"+id,0,0,MetaCurrencyRules.NaturalDiscoveryData)&&!_mainMenu&&_runArchiveDataEarned<int.MaxValue)_runArchiveDataEarned+=MetaCurrencyRules.NaturalDiscoveryData;
   }
   _pendingKnowledgeRewards.Clear();
   // One existing knowledge save, including batched currency receipts. No Save per ID.
   PlayerPrefs.SetString("echosphere_knowledge_v1",JsonUtility.ToJson(new KnowledgeSave{ids=new List<string>(_knownKnowledgeIds)}));WriteMetaWallet(wallet);
  }
  private void ResearchNativeCharacterMastery(string id,int expectedLevel)
  {
   if(!_mainMenu||_mainMenuPage!=MainMenuPage.Characters||!CharacterRules.TryParseSourceId(id,out var character))return;
   var profile=GetCharacterProfile(character);if(!profile.unlocked||profile.masteryLevel+1!=expectedLevel||expectedLevel>10)return;
   int cost=MetaCurrencyRules.MasteryResearchShards(profile.masteryLevel);
   if(!TryBuyMeta("purchase:mastery:"+id+":"+expectedLevel,CurrencyId.ResonanceShards,cost)){
    _nativeCharacterMessage=IsRussianUi?"Недостаточно осколков резонанса.":"Not enough Resonance Shards.";
   }else{
    ApplyMasteryResearch(profile,expectedLevel);SaveCharacterProfiles();_nativeCharacterMessage="";_nativeMenuModelVersion++;
   }
   _nativeCharacterState=ReadNativeCharacterState();
  }
  private void ApplyMasteryResearch(CharacterProfileData profile,int level)
  {
   profile.masteryXp=Mathf.Max(profile.masteryXp,CharacterRules.GetMasteryThreshold(level));
   profile.masteryLevel=Mathf.Max(profile.masteryLevel,CharacterRules.GetMasteryLevelForXp(profile.masteryXp));
   if(profile.id==CharacterRules.ToSourceId(_sphereCharacterId))_characterMasteryLevel=profile.masteryLevel;
  }
  private void ResearchNativeLore(string id)
  {
   if(!_mainMenu||_mainMenuPage!=MainMenuPage.Knowledge||_knowledgeSection!="regions")return;
   EnsureKnowledgeLoaded();if(_knownKnowledgeIds.Contains(id))return;
   foreach(var lore in CampaignRegionLore.All)if(lore.Id==id){
    if(!IsRegionAvailable(lore.Region)||!TryBuyMeta("purchase:lore:"+id,CurrencyId.ArchiveData,MetaCurrencyRules.LoreResearchCost))return;
    _knownKnowledgeIds.Add(id);PersistKnowledgeProgress();_nativeArchiveState=ReadNativeArchiveState();return;
   }
  }
 }
}
