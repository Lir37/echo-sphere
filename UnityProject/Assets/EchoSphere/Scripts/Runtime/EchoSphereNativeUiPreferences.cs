using System;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private static string NativeText(string key,bool russian)=>NativeLocalizationCatalog.Instance.Translate(key,russian);
        // Preserve JS Number conversion for native string preferences. The older
        // IMGUI settings screen wrote this same key with SetFloat; read that
        // existing representation without rewriting the user's preference.
        private static double NativeInterfaceScale
        {
            get {const string key="echosphere_interface_scale",missing="\u0001";string text=PlayerPrefs.GetString(key,missing);
                double raw=text==missing?(PlayerPrefs.HasKey(key)?PlayerPrefs.GetFloat(key,float.NaN):0):text.Trim().Length==0?0:double.TryParse(text,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out double number)?number:double.NaN;
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
