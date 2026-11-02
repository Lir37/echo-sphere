using System;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRegionScreen _nativeRegionScreen;
        private NativeRegionScreen.State _nativeRegionState;
        private bool _nativeRegionWasActive;
        private bool TryDrawNativeRegions()
        {
            if(!_mainMenu||!_regionSelect){
                if(_nativeRegionWasActive){_nativeRegionScreen.Hide();_nativeRegionWasActive=false;
                    if(_sourceUi!=null&&!UsesNativeMainMenu&&!_nativeProgressionWasActive&&!_nativeSettingsWasActive){_sourceUi.Dirty=true;}}
                return false;
            }
            if(!_nativeRegionWasActive){
                if(_nativeRegionScreen==null)_nativeRegionScreen=AcquireNativeScreen<NativeRegionScreen>("Native Region Selection UI Toolkit");
                _nativeRegionState=ReadNativeRegionState();_nativeRegionWasActive=true;
                _camera.cullingMask=0;
            }
            _nativeRegionScreen.Show(_nativeRegionState,NativeSubmenuScale,()=>{if(!_mainMenu||!_regionSelect||NativeRoutes.RegionLaunchPending)return;TraceNativeRoute("region-back");_regionSelect=false;_mainMenuPage=MainMenuPage.Home;},StartNativeRegion);
            return true;
        }
        private NativeRegionScreen.State ReadNativeRegionState()
        {
            EnsureKnowledgeLoaded();var regions=new List<NativeRegionScreen.Region>();
            foreach(var entry in _sourceKnowledge.Planner){
                if(entry.category!="region")continue;
                EchoRegionId id=entry.id=="spectral_rift"?EchoRegionId.SpectralRift:EchoRegionId.ResonanceBasin;
                bool available=IsRegionAvailable(id);var completed=GetCompletedRegionChallenges(id);
                var challenges=new List<NativeRegionScreen.Challenge>();
                foreach(string challengeId in entry.requires){var definition=PlannerEntry("region-challenge",challengeId);challenges.Add(new NativeRegionScreen.Challenge {Id=challengeId,Name=PlannerName(definition),Description=PlannerDesc(definition),Accent=NativeRegionChallengeAccent(challengeId,entry.color),Completed=Array.IndexOf(completed,challengeId)>=0});}
                regions.Add(new NativeRegionScreen.Region {Id=entry.id,Name=PlannerName(entry),Subtitle=PlannerDesc(entry),Accent=entry.color,Available=available,Stable=available&&IsRegionStabilized(id),Endless=available&&IsRegionEndlessUnlocked(id),Completed=completed.Length,Challenges=challenges});
            }
            foreach(var profile in CampaignRegions.Additional){bool available=IsRegionAvailable(profile.Region);var previous=(EchoRegionId)((int)profile.Region-1);var prior=NativeRunCatalog.Instance.GetRegion(CampaignRegions.SourceId(previous));regions.Add(new NativeRegionScreen.Region{Id=profile.Id,Name=IsRussianUi?profile.Ru:profile.En,Subtitle=NativePlayerCopy.Text(IsRussianUi?profile.DescriptionRu:profile.DescriptionEn,IsRussianUi),Accent=profile.Accent,Available=available,Stable=IsRegionStabilized(profile.Region),Endless=false,Completed=0,Challenges=new List<NativeRegionScreen.Challenge>(),UnlockRu="Откроется после стабилизации: "+prior.Name(true)+".",UnlockEn="Unlocks after stabilizing "+prior.Name(false)+"."});}
            foreach(var row in regions)
            {
                CampaignRegions.TryParse(row.Id,out var id);var completed=GetCompletedRegionChallenges(id);
                if((int)id>=2){var list=new List<NativeRegionScreen.Challenge>();foreach(var c in CampaignRegionContent.GetContracts(id))list.Add(new NativeRegionScreen.Challenge{Id=c.Id,Name=IsRussianUi?c.Ru:c.En,Description=NativePlayerCopy.Text(IsRussianUi?c.DescriptionRu:c.DescriptionEn,IsRussianUi),Accent=row.Accent,Completed=Array.IndexOf(completed,c.Id)>=0});row.Challenges=list;row.Completed=completed.Length;row.Endless=IsRegionEndlessUnlocked(id);}
                foreach(var challenge in row.Challenges){var definition=CampaignRegionContent.Find(id,challenge.Id);if(definition!=null)challenge.Description=NativePlayerCopy.Text(IsRussianUi?definition.DescriptionRu:definition.DescriptionEn,IsRussianUi);}
                row.Mastery=CampaignRegionContent.MasteryDescriptions(id,IsRussianUi);row.MasteryMask=PlayerPrefs.GetInt(RegionMasteryKey(id),0);
                int known=0;foreach(var lore in CampaignRegionLore.All)if(lore.Region==id&&_knownKnowledgeIds.Contains(lore.Id))known++;row.LoreKnown=known;
            }
            regions.Add(new NativeRegionScreen.Region{Id="convergence",Name=IsRussianUi?"СХОЖДЕНИЕ":"CONVERGENCE",Subtitle=IsRussianUi?"0–25 мин: смена экологий; 25 мин: главный бой; 25–40 мин: смешанные угрозы; 40 мин: финал; далее эскалация.":"0–25 min: ecology rotations; 25 min: major boss; 25–40 min: mixed threats; 40 min: final boss; then escalation.",Accent="#f3d18b",Available=IsConvergenceUnlocked(),Endless=true,Convergence=true,Challenges=new List<NativeRegionScreen.Challenge>(),UnlockRu="Стабилизируйте все шесть регионов.",UnlockEn="Stabilize all six regions."});
            return new NativeRegionScreen.State {Russian=IsRussianUi,Regions=regions};
        }
        // EchoMapScreen.tsx challengeAccent; the shared planner catalog has no
        // challenge color field, so do not infer one from unrelated metadata.
        private static string NativeRegionChallengeAccent(string id,string fallback)
        {
            switch(id){case "fractured_network":return "#ff6b6b";case "overload":return "#ffb84d";case "low_gravity":return "#a88cff";case "mirror_tide":return "#7fd8ff";case "volatile_core":return "#f49cff";case "phase_drift":return "#64e4c4";default:return fallback;}
        }
        private void StartNativeRegion(string sourceId,string mode,string challenge)
        {
            if(!_mainMenu||!_regionSelect||NativeRoutes.RegionLaunchPending){TraceNativeRoute("rejected-region-start");return;}
            bool convergence=sourceId=="convergence";
            if(convergence&&(mode!="endless"||challenge!="none"||!IsConvergenceUnlocked()))return;
            if(!convergence&&!CampaignRegions.TryParse(sourceId,out _))return;
            var region=EchoRegionId.ResonanceBasin;if(!convergence)CampaignRegions.TryParse(sourceId,out region);
            if(mode!="stabilization"&&mode!="endless")return;
            if(challenge!="none"&&(mode=="endless"||CampaignRegionContent.Find(region,challenge)==null))return;
            if(!IsRegionAvailable(region))return;
            if(mode=="endless"&&!convergence&&!IsRegionEndlessUnlocked(region))return;
            if(challenge!="none"&&!IsRegionStabilized(region))return;
            NativeRoutes.RegionLaunchPending=true;TraceNativeRoute("region-start:"+sourceId+":"+mode+":"+challenge);
            // Exact original onStartRun handoff: reload constructs fresh player,
            // spheres and purchased stats with the selected character.
            PlayerPrefs.SetInt("echo-sphere:pending_convergence",convergence?1:0);
            PlayerPrefs.SetInt(SelectedRegionPreferenceKey,(int)region);
            PlayerPrefs.SetString("echo-sphere:pending_region_challenge",challenge);
            PlayerPrefs.SetInt("echo-sphere:pending_region_endless",mode=="endless"?1:0);
            PlayerPrefs.SetInt("echo-sphere:start_selected_region_after_reload",1);
            PlayerPrefs.Save();NativeRoutes.LoadRegionScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
