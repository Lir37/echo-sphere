using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Exact state-writing rules for Rune of Resonance and the Echo Architecture
    /// artifact set, mapped from src/engineLoop.ts and src/engineCombat.ts.
    /// </summary>
    public static class FormationHitRules
    {
        public const int ResonanceRuneHitIncrement = 2;
        public const float ResonanceRunePulseSeconds = 0.5f;
        public const int EchoArchitectureRelayHitIncrement = 2;
        public const float EchoArchitectureRelayPulseSeconds = 0.35f;
        public const float EchoArchitectureRelayLightningSeconds = 0.20f;

        public static bool ShouldTriggerEchoArchitectureRelay(int formationHitCount) =>
            formationHitCount > 0 && formationHitCount % 5 == 0;

        public static bool HasEchoRelaySynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "echo_conductor", "network_relay");

        public static bool HasUnifiedCoreSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "unified_mind", "zero_sphere");

        public static bool HasCompletedEchoArchitecture(IList<string> ownedArtifacts) =>
            HasEchoRelaySynergy(ownedArtifacts) && HasUnifiedCoreSynergy(ownedArtifacts);

        public static float GetEchoRelayDamageMultiplier(bool hasSynergy, int uniqueTypeCount) =>
            hasSynergy && uniqueTypeCount >= 2 ? 1.08f : 1f;

        public static float GetEchoArchitectureDamageMultiplier(
            bool setComplete, int uniqueTypeCount, bool hasNearbySphere) =>
            setComplete && uniqueTypeCount >= 2 && hasNearbySphere ? 1.06f : 1f;

        public static float GetUnifiedCoreDamageMultiplier(
            bool hasSynergy, int sphereLevel, int strongestSphereLevel) =>
            hasSynergy && sphereLevel < strongestSphereLevel
                ? 1f + strongestSphereLevel * 0.05f
                : 1f;

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
