using System;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Preserve JS Number conversion, including absent/empty -> zero before
        // clamping and non-finite -> 1. No DOM/global-storage adapter is needed.
        private static double NativeInterfaceScale
        {
            get {double raw=VisualJs.Number(new V(PlayerPrefs.GetString("echosphere_interface_scale",""))).Number;
                return double.IsFinite(raw)?Math.Min(1.25,Math.Max(.75,raw)):1;}
        }
        private void NativeResetAllProgress()
        {
            foreach(var key in new[]{GoldPreferenceKey,ShopPreferenceKey,LeaderboardPreferenceKey,AchievementPreferenceKey,DifficultyPreferenceKey,SoundPreferenceKey,CharacterPreferenceKey,CharacterProfilesPreferenceKey,HandednessPreferenceKey,LanguagePreferenceKey,NamePreferenceKey,"echosphere_knowledge_v1",TutorialCompletedPreferenceKey})PlayerPrefs.DeleteKey(key);
            foreach(var region in new[]{EchoSphere.Core.EchoRegionId.ResonanceBasin,EchoSphere.Core.EchoRegionId.SpectralRift}){PlayerPrefs.DeleteKey(RegionStabilizedKey(region));PlayerPrefs.DeleteKey(RegionChallengesKey(region));PlayerPrefs.DeleteKey(RegionEndlessKey(region));}
            ClearRunSnapshot();PlayerPrefs.Save();
        }
    }
}
