using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private CampaignRegionProgress _campaignRegionProgress;
  private bool _convergenceRun;
  private ConvergenceState _convergenceState;
  public EchoRegionId ActiveCampaignRegion => _convergenceRun&&UsesCampaignCombat?ConvergenceRules.Primary(_runTime):_selectedRegion;
  private bool IsConvergenceUnlocked()=>ConvergenceRules.Unlocked(IsRegionStabilized);
  private static string RegionMasteryKey(EchoRegionId region)=>RegionChallengePreferencePrefix+RegionSourceId(region)+":mastery";
  private bool HasCampaignContract(RegionContractRule rule)=>UsesCampaignCombat&&!_convergenceRun&&CampaignRegionContent.Has(_selectedRegion,_regionChallengeId,rule);
  private void ResetCampaignRegionalProgress()
  {
   _campaignRegionProgress=new CampaignRegionProgress();_convergenceState=_convergenceRun?new ConvergenceState():null;
   if(!_convergenceRun)DiscoverCampaignLore(_selectedRegion,0);
  }
  private void InitializeRestoredCampaignProgress()
  {
   if(!UsesCampaignCombat||_campaignRegionProgress!=null)return;
   // Historical snapshots did not record peak size; do not award a whole-run size objective from the final count.
   _campaignRegionProgress=new CampaignRegionProgress{PeakSpheres=int.MaxValue,DominantSeconds=_regionMasteryDominantSeconds};
   foreach(var geometry in _regionMasteryGeometryTypes)if(geometry!=SphereNetworkFormation.None)_campaignRegionProgress.GeometryMask|=1<<(int)geometry;
   foreach(var poi in _regionLoreDiscovered){int index=CampaignRegionContent.PoiIndex(poi);if(index>=0)_campaignRegionProgress.PoiMask|=1<<index;}
  }
  private void DiscoverCampaignLore(EchoRegionId region,int index)
  {
   if(!UsesCampaignCombat)return;EnsureKnowledgeLoaded();
   string id="region-lore:"+RegionSourceId(region)+":"+index;
   if(!RecordNaturalKnowledge(id))return;
   PersistKnowledgeProgress();
  }
  private void TickCampaignRegionProgress(float dt)
  {
   if(!UsesCampaignCombat)return;if(_campaignRegionProgress==null)_campaignRegionProgress=new CampaignRegionProgress();
   int living=0,seven=0;foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){living++;}
   foreach(var pair in _sphereLevels)if(pair.Value>=7)seven++;
   _campaignRegionProgress.PeakSpheres=Mathf.Max(_campaignRegionProgress.PeakSpheres,living);
   _campaignRegionProgress.LevelSevenFamilies=Mathf.Max(_campaignRegionProgress.LevelSevenFamilies,seven);
   var formation=_networkState?.DominantFormation;
   if(formation!=null&&formation.Active&&formation.Type!=SphereNetworkFormation.None){_campaignRegionProgress.GeometryMask|=1<<(int)formation.Type;_campaignRegionProgress.DominantSeconds+=Mathf.Max(0,dt);}
   if(_convergenceRun&&_regionFieldVisuals!=null&&_regionFieldVisuals.ActiveRegionId!=ActiveCampaignRegion)
   { _regionFieldVisuals.SetRegion(ActiveCampaignRegion,true);_regionHudPocketId="";ShowTimedCombatMessage(IsRussianUi?"СХОЖДЕНИЕ · СМЕНА ЭКОЛОГИИ":"CONVERGENCE · ECOLOGY SHIFT",1.2f); }
  }
  private void RecordCampaignPoi(string type)
  {
   if(!UsesCampaignCombat)return;if(_campaignRegionProgress==null)_campaignRegionProgress=new CampaignRegionProgress();
   int i=CampaignRegionContent.PoiIndex(type);if(i>=0)_campaignRegionProgress.PoiMask|=1<<i;
   if(type=="resonance_cache"||type=="echo_relay"||type=="breach_node")DiscoverCampaignLore(ActiveCampaignRegion,1);
   if(type=="lost_signal")DiscoverCampaignLore(ActiveCampaignRegion,2);
   if(type=="elite_nest"||type=="rupture")DiscoverCampaignLore(ActiveCampaignRegion,3);
   if(type=="boss_trace")DiscoverCampaignLore(ActiveCampaignRegion,4);
  }
  private void CompleteCampaignRegionalProgress()
  {
   if(!UsesCampaignCombat||_convergenceRun)return;
   TickCampaignRegionProgress(0);
   int mask=CampaignRegionContent.MasteryMask(_selectedRegion,_campaignRegionProgress);
   PlayerPrefs.SetInt(RegionMasteryKey(_selectedRegion),PlayerPrefs.GetInt(RegionMasteryKey(_selectedRegion),0)|mask);
   _regionMasteryChallengesCompleted=CampaignRegionContent.Bits(mask);
   PlayerPrefs.SetInt(RegionEndlessKey(_selectedRegion),1);
   DiscoverCampaignLore(_selectedRegion,4);
  }
  private string CampaignSpawnRole(EchoRegionId region,float roll)
  {
   bool pressure=_regionEndlessMode||_convergenceRun||HasCampaignContract(RegionContractRule.Mixed);
   float share=pressure?Mathf.Min(.5f,.2f+Mathf.Floor(_runTime/600f)*.05f):0;
   if(roll<share)return CampaignRegionContent.SelectSupport(region,roll/Mathf.Max(.01f,share),_runTime);
   return RegionPocketRules.SelectCampaignRole(region,_player.position.x,-_player.position.y,(roll-share)/Mathf.Max(.01f,1-share),_runTime);
  }
 }
}
