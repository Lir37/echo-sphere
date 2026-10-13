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

        public static bool HasPerfectNetworkSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "fivefold_resonance", "triangle_circuit");

        public static bool HasHunterDoctrineSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "foresight_eye", "sniper_scope");

        public static bool HasPulseEngineeringSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "pulse_driver", "pulse_lens");

        public static bool HasCoreForgeSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "echo_weaver", "folded_core");

        public static bool HasHunterFocusSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "sniper_crown", "long_lens");

        public static bool HasPulseOverloadSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "pulse_crown", "tempo_ring");

        public static bool HasAxiomFoldSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "axiom_core", "universal_fold");

        public static bool HasGeometryLoopSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "triangle_engine", "ring_engine");

        public static bool HasLatticeMemorySynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "lattice_engine", "fractal_seed");

        public static bool HasNetworkMemorySynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "network_anchor", "echo_weaver");

        public static bool HasStatusCircuitSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "prism_filter", "stasis_mandala");

        public static bool HasStatusLoopSynergy(IList<string> ownedArtifacts) =>
            HasBoth(ownedArtifacts, "prism_crown", "stasis_mandala");

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

        public static float GetPerfectNetworkDamageMultiplier(
            bool hasSynergy, int uniqueTypeCount, bool sphereFormsTriangle) =>
            hasSynergy && uniqueTypeCount >= 3 && sphereFormsTriangle ? 1.10f : 1f;

        public static bool IsTriangleWithinSourceRange(
            float abDistancePixels, float asDistancePixels, float bsDistancePixels)
        {
            return IsFiniteNonNegative(abDistancePixels) &&
                IsFiniteNonNegative(asDistancePixels) &&
                IsFiniteNonNegative(bsDistancePixels) &&
                abDistancePixels <= 300f && asDistancePixels <= 300f && bsDistancePixels <= 300f;
        }

        public static float GetHunterDoctrineDamageMultiplier(
            bool hasSynergy, bool isSniper, bool hasLivingEliteOrBoss) =>
            hasSynergy && isSniper && hasLivingEliteOrBoss ? 1.12f : 1f;

        public static float GetPulseEngineeringDamageMultiplier(
            bool hasSynergy, bool isPulse, int livingEnemyCount) =>
            hasSynergy && isPulse && livingEnemyCount >= 4 ? 1.12f : 1f;

        public static float GetCoreForgeDamageMultiplier(bool hasSynergy, int uniqueTypeCount) =>
            hasSynergy && uniqueTypeCount >= 3 ? 1.06f : 1f;

        public static float GetHunterFocusDamageMultiplier(
            bool hasSynergy, bool isSniper, bool hasLongLens) =>
            hasSynergy && isSniper && hasLongLens ? 1.10f : 1f;

        public static float GetPulseOverloadDamageMultiplier(
            bool hasSynergy, bool isPulse, bool hasPulseDriver) =>
            hasSynergy && isPulse && hasPulseDriver ? 1.10f : 1f;

        public static float GetAxiomFoldDamageMultiplier(bool hasSynergy, int uniqueTypeCount) =>
            hasSynergy && uniqueTypeCount >= 3 ? 1.06f : 1f;

        public static float GetGeometryLoopDamageMultiplier(
            bool hasSynergy, SphereNetworkFormation geometryType) =>
            hasSynergy && (geometryType == SphereNetworkFormation.Ring ||
                geometryType == SphereNetworkFormation.Lattice ||
                geometryType == SphereNetworkFormation.Fractal) ? 1.08f : 1f;

        public static float GetLatticeMemoryDamageMultiplier(
            bool hasSynergy, bool hasResonanceMemory, SphereNetworkFormation memoryType) =>
            hasSynergy && hasResonanceMemory &&
                (memoryType == SphereNetworkFormation.Lattice || memoryType == SphereNetworkFormation.Fractal)
                ? 1.08f : 1f;

        public static float GetNetworkMemoryDamageMultiplier(bool hasSynergy, bool hasResonanceMemory) =>
            hasSynergy && hasResonanceMemory ? 1.08f : 1f;

        private static bool IsFiniteNonNegative(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;

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
