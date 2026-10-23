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
                if(_nativeRegionScreen==null)_nativeRegionScreen=new GameObject("Native Region Selection UI Toolkit").AddComponent<NativeRegionScreen>();
                _nativeRegionState=ReadNativeRegionState();_nativeRegionWasActive=true;
                _camera.cullingMask=0;
            }
            _nativeRegionScreen.Show(_nativeRegionState,SourceDevicePixelRatio,()=>{_regionSelect=false;_mainMenuPage=MainMenuPage.Home;},StartNativeRegion);
            return true;
        }
        private NativeRegionScreen.State ReadNativeRegionState()
        {
            EnsureKnowledgeLoaded();var regions=new List<NativeRegionScreen.Region>();
            foreach(var entry in _sourceKnowledge.planner){
                if(entry.category!="region")continue;
                EchoRegionId id=entry.id=="spectral_rift"?EchoRegionId.SpectralRift:EchoRegionId.ResonanceBasin;
                bool available=IsRegionAvailable(id);var completed=GetCompletedRegionChallenges(id);
                var challenges=new List<NativeRegionScreen.Challenge>();
                foreach(string challengeId in entry.requires){var definition=PlannerEntry("region-challenge",challengeId);challenges.Add(new NativeRegionScreen.Challenge {Id=challengeId,Name=PlannerName(definition),Description=PlannerDesc(definition),Accent=NativeRegionChallengeAccent(challengeId,entry.color),Completed=Array.IndexOf(completed,challengeId)>=0});}
                regions.Add(new NativeRegionScreen.Region {Id=entry.id,Name=PlannerName(entry),Subtitle=PlannerDesc(entry),Accent=entry.color,Available=available,Stable=available&&IsRegionStabilized(id),Endless=available&&IsRegionEndlessUnlocked(id),Completed=completed.Length,Challenges=challenges});
            }
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
            if(!_mainMenu||!_regionSelect)return;
            EchoRegionId region=sourceId=="spectral_rift"?EchoRegionId.SpectralRift:EchoRegionId.ResonanceBasin;
            if(!IsRegionAvailable(region))return;
            if(mode=="endless"&&!IsRegionEndlessUnlocked(region))return;
            if(challenge!="none"&&!IsRegionStabilized(region))return;
            // Exact original onStartRun handoff: reload constructs fresh player,
            // spheres and purchased stats with the selected character.
            PlayerPrefs.SetInt(SelectedRegionPreferenceKey,(int)region);
            PlayerPrefs.SetString("echo-sphere:pending_region_challenge",challenge);
            PlayerPrefs.SetInt("echo-sphere:pending_region_endless",mode=="endless"?1:0);
            PlayerPrefs.SetInt("echo-sphere:start_selected_region_after_reload",1);
            PlayerPrefs.Save();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
