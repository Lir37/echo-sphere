using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped active artifact pair synergies from src/artifactSystem.ts.
    /// Base artifact effects remain in ArtifactRewardRules; these are only the
    /// conditional effects that activate when both required artifacts are owned.
    /// </summary>
    public static class ArtifactSynergyRules
    {
        public static bool HasFortressNetworkSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "heavy_core", "resonance_core");

        public static bool HasGlassCannonSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "overclock", "void_contract");

        public static bool HasSingularitySynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "lone_bastion", "singularity_engine");

        public static bool HasCoreRelaySynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "unified_mind", "echo_thread");

        public static bool HasVoidHorizonSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "void_ink", "void_mark");

        public static bool HasOrbitalPrismSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "orbital_blade", "prism_filter");

        public static bool HasGravityPulseSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "gravity_hook", "pulse_driver");

        public static float GetFortressNetworkDamageMultiplier(bool hasSynergy, bool hasNearbySphere) =>
            hasSynergy && hasNearbySphere ? 1.10f : 1f;

        public static float GetGlassCannonDamageMultiplier(bool hasSynergy) =>
            hasSynergy ? 1.12f : 1f;

        public static float GetGlassCannonDamageTakenMultiplier(bool hasSynergy) =>
            hasSynergy ? 1.05f : 1f;

        public static float GetSingularityDamageMultiplier(bool hasSynergy, int uniqueTypeCount) =>
            hasSynergy && uniqueTypeCount <= 1 ? 1.20f : 1f;

        public static float GetCoreRelayDamageMultiplier(bool hasSynergy, int uniqueTypeCount) =>
            hasSynergy && uniqueTypeCount >= 2 ? 1.08f : 1f;

        public static float GetVoidHorizonDamageMultiplier(
            bool hasSynergy, bool isVoidSphere, bool hasLivingElite) =>
            hasSynergy && isVoidSphere && hasLivingElite ? 1.15f : 1f;

        public static float GetOrbitalPrismDamageMultiplier(bool hasSynergy, bool isOrbitalSphere) =>
            hasSynergy && isOrbitalSphere ? 1.15f : 1f;

        public static float GetGravityPulseDamageMultiplier(
            bool hasSynergy, bool isPulseSphere, int groupedLivingEnemyCount) =>
            hasSynergy && isPulseSphere && groupedLivingEnemyCount >= 3 ? 1.15f : 1f;

        private static bool HasBoth(IList<string> ownedArtifacts, string first, string second)
        {
            if (ownedArtifacts == null) return false;
            var hasFirst = false;
            var hasSecond = false;
            for (var i = 0; i < ownedArtifacts.Count; i++)
            {
                if (string.Equals(ownedArtifacts[i], first, StringComparison.Ordinal)) hasFirst = true;
                if (string.Equals(ownedArtifacts[i], second, StringComparison.Ordinal)) hasSecond = true;
            }
            return hasFirst && hasSecond;
        }
    }
}
