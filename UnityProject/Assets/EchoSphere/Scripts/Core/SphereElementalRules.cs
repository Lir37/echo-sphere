using System;

namespace EchoSphere.Core
{
    public enum SphereElement
    {
        None = 0,
        Fire = 1,
        Freeze = 2,
        Poison = 3
    }

    public enum SphereElementalMode
    {
        Direct = 0,
        Conduction = 1,
        Field = 2
    }

    public readonly struct SphereElementalStatus
    {
        public SphereElement Element { get; }
        public float Duration { get; }
        public float DamagePerSecond { get; }
        public float TickInterval { get; }
        public bool FreezeVulnerability { get; }
        public float AdditionalSlowDuration { get; }
        public float AdditionalSlowMultiplier { get; }

        public SphereElementalStatus(SphereElement element, float duration, float damagePerSecond,
            float tickInterval, bool freezeVulnerability, float additionalSlowDuration, float additionalSlowMultiplier)
        {
            Element = element;
            Duration = Math.Max(0f, duration);
            DamagePerSecond = Math.Max(0f, damagePerSecond);
            TickInterval = Math.Max(0f, tickInterval);
            FreezeVulnerability = freezeVulnerability;
            AdditionalSlowDuration = Math.Max(0f, additionalSlowDuration);
            AdditionalSlowMultiplier = additionalSlowMultiplier <= 0f ? 1f : Math.Min(1f, additionalSlowMultiplier);
        }
    }

    /// <summary>Source-mapped elemental Mutation rules from new-desing/src/sphereProgression.ts and src/engineSpheres.ts.</summary>
    public static class SphereElementalRules
    {
        public static SphereElement ResolveElement(string branch, string finalId = null)
        {
            if (string.IsNullOrEmpty(branch)) return SphereElement.None;
            if (branch == "prism_spectrum")
            {
                var choice = GetFinalChoiceIndex(finalId);
                return choice == 1 ? SphereElement.Freeze : choice == 2 ? SphereElement.Poison : SphereElement.Fire;
            }

            switch (branch)
            {
                case "standard_resonator":
                case "sniper_assassin":
                case "shotgun_burst":
                case "orbital_blade":
                case "chain_storm":
                case "aura_gravity":
                case "gravity_tide":
                case "pulse_resonator":
                case "void_reaper":
                    return SphereElement.Fire;

                case "standard_singularity":
                case "sniper_beacon":
                case "shotgun_hail":
                case "orbital_halo":
                case "prism_mirror":
                case "chain_web":
                case "aura_sanctum":
                case "gravity_well":
                case "pulse_wave":
                case "void_execution":
                    return SphereElement.Freeze;

                case "standard_swarm":
                case "sniper_oracle":
                case "shotgun_cataclysm":
                case "orbital_dance":
                case "prism_split":
                case "chain_leech":
                case "aura_overgrowth":
                case "gravity_collapse":
                case "pulse_burst":
                case "void_hunger":
                    return SphereElement.Poison;

                default:
                    return SphereElement.None;
            }
        }

        public static string GetMasteryId(string branch, string finalId)
        {
            var choice = GetFinalChoiceIndex(finalId);
            if (choice < 0 || string.IsNullOrEmpty(branch)) return null;
            if (IsChainBranch(branch))
                return choice == 0 ? "conduction_power" : choice == 1 ? "conduction_rate" : "conduction_duration";
            if (IsFieldBranch(branch))
                return choice == 0 ? "field_power" : choice == 1 ? "field_frequency" : "field_duration";

            switch (ResolveElement(branch, finalId))
            {
                case SphereElement.Fire:
                    return choice == 0 ? "fire_power" : choice == 1 ? "fire_tempo" : "fire_duration";
                case SphereElement.Freeze:
                    return choice == 0 ? "freeze_duration" : choice == 1 ? "freeze_impact" : "freeze_permafrost";
                case SphereElement.Poison:
                    return choice == 0 ? "poison_power" : choice == 1 ? "poison_tempo" : "poison_duration";
                default:
                    return null;
            }
        }

        public static bool IsChainBranch(string branch) =>
            !string.IsNullOrEmpty(branch) && branch.StartsWith("chain_", StringComparison.Ordinal);

        public static bool IsFieldBranch(string branch) =>
            !string.IsNullOrEmpty(branch) &&
            (branch.StartsWith("aura_", StringComparison.Ordinal) ||
             branch.StartsWith("gravity_", StringComparison.Ordinal) ||
             branch.StartsWith("pulse_", StringComparison.Ordinal));

        public static int GetConductionThreshold(string masteryId) => masteryId == "conduction_rate" ? 2 : 3;
        public static float GetConductionCooldown(string masteryId) => masteryId == "conduction_rate" ? 1.4f : 2f;
        public static float GetConductionDamageMultiplier(string masteryId) => masteryId == "conduction_power" ? 0.27f : 0.20f;
        public static float GetFieldCooldown(string masteryId) => masteryId == "field_frequency" ? 1.25f : 1.8f;

        public static SphereElementalStatus CreateStatus(SphereElement element, string masteryId,
            SphereElementalMode mode, float characterDurationMultiplier = 1f, float characterDamageMultiplier = 1f)
        {
            if (element == SphereElement.None)
                return new SphereElementalStatus(SphereElement.None, 0f, 0f, 0f, false, 0f, 1f);

            var duration = element == SphereElement.Fire ? 3f : element == SphereElement.Freeze ? 0.8f : 4f;
            var damagePerSecond = element == SphereElement.Fire ? 8f : element == SphereElement.Poison ? 5f : 0f;
            var tickInterval = 0f;
            var vulnerability = false;
            var slowDuration = 0f;
            var slowMultiplier = 1f;

            duration *= Math.Max(0f, characterDurationMultiplier);
            damagePerSecond *= Math.Max(0f, characterDamageMultiplier);

            if (mode == SphereElementalMode.Direct)
            {
                if (element == SphereElement.Fire)
                {
                    if (masteryId == "fire_power") damagePerSecond *= 1.35f;
                    if (masteryId == "fire_duration") duration *= 1.45f;
                    if (masteryId == "fire_tempo") tickInterval = 0.12f;
                }
                else if (element == SphereElement.Freeze)
                {
                    if (masteryId == "freeze_duration") duration *= 1.40f;
                    if (masteryId == "freeze_impact") vulnerability = true;
                    if (masteryId == "freeze_permafrost") { slowDuration = 1f; slowMultiplier = 0.72f; }
                }
                else
                {
                    if (masteryId == "poison_power") damagePerSecond *= 1.35f;
                    if (masteryId == "poison_duration") duration *= 1.45f;
                    if (masteryId == "poison_tempo") tickInterval = 0.12f;
                }
            }
            else if (mode == SphereElementalMode.Conduction)
            {
                if (masteryId == "conduction_power")
                {
                    if (element == SphereElement.Freeze) duration = Math.Max(duration, 0.5f) * 1.35f;
                    else damagePerSecond *= 1.35f;
                }
                if (masteryId == "conduction_duration") duration = Math.Max(duration, 0.5f) * 1.45f;
            }
            else
            {
                if (masteryId == "field_power")
                {
                    if (element == SphereElement.Freeze)
                    {
                        duration = Math.Max(duration, 0.5f) * 1.35f;
                        slowDuration = 0.8f;
                        slowMultiplier = 0.62f;
                    }
                    else damagePerSecond *= 1.35f;
                }
                if (masteryId == "field_duration") duration = Math.Max(duration, 0.5f) * 1.45f;
            }

            return new SphereElementalStatus(element, duration, damagePerSecond, tickInterval,
                vulnerability, slowDuration, slowMultiplier);
        }

        private static int GetFinalChoiceIndex(string finalId)
        {
            if (string.IsNullOrEmpty(finalId)) return -1;
            if (finalId.EndsWith("_final_1", StringComparison.Ordinal)) return 0;
            if (finalId.EndsWith("_final_2", StringComparison.Ordinal)) return 1;
            if (finalId.EndsWith("_final_3", StringComparison.Ordinal)) return 2;
            return -1;
        }
    }
}
