using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private int _runShardsEarned;
#pragma warning disable CS0414 // Retained and read by the backwards-compatible reflection snapshot codec.
  private int _runArchiveDataEarned;
#pragma warning restore CS0414
  private readonly List<string> _pendingKnowledgeRewards=new List<string>();
  private void GrantCompletedRunCurrencies()
  {
            var difficulty = CurrentDifficulty;
            _runGoldEarned=MetaCurrencyRules.RunCredits(_runTime,_bossesDefeated,_kills,_regionStabilized,_selectedRegion,_runEliteKills,difficulty.GoldMultiplier,RuneRules.CellReward(_runEconomy?.Runes));
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
   // Legacy ArchiveData stays readable in old wallets, but discoveries no longer mint a currency.
   for(int i=0;i<6;i++){var region=(EchoRegionId)i;if(IsRegionStabilized(region))MetaCurrencyRules.TryGrantOnce(wallet,"legacy-clear:"+RegionSourceId(region),0,MetaCurrencyRules.RunShards(3,true),0);}
   MetaCurrencyRules.TryGrantOnce(wallet,migration,0,0,0);WriteMetaWallet(wallet);
  }
  private bool RecordNaturalKnowledge(string id)
  {if(!_knownKnowledgeIds.Add(id))return false;_pendingKnowledgeRewards.Add(id);return true;}
  private void PersistKnowledgeProgress()
  {
   _pendingKnowledgeRewards.Clear();
   // One existing knowledge save for batched discoveries. No currency award or Save per ID.
   PlayerPrefs.SetString("echosphere_knowledge_v1",JsonUtility.ToJson(new KnowledgeSave{ids=new List<string>(_knownKnowledgeIds),appearances=new List<BestiaryAppearance>(_bestiaryAppearances.Values)}));PlayerPrefs.Save();
  }
  private void RestoreLegacyPurchasedMastery(CharacterProfileData profile,int level)
  {
   profile.masteryXp=Mathf.Max(profile.masteryXp,CharacterRules.GetMasteryThreshold(level));
   profile.masteryLevel=Mathf.Max(profile.masteryLevel,CharacterRules.GetMasteryLevelForXp(profile.masteryXp));
   if(profile.id==CharacterRules.ToSourceId(_sphereCharacterId))_characterMasteryLevel=profile.masteryLevel;
  }
 }
}
