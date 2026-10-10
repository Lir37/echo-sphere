using System;

namespace EchoSphere.Core
{
    /// <summary>First runtime-backed Standard evolution rules, source-mapped to sphereProgression.ts.</summary>
    public static class SphereEvolutionCombatRules
    {
        public static bool ShouldTriggerStandardResonatorPulse(int hitCount) => hitCount > 0 && hitCount % 3 == 0;

        public static float GetStandardResonatorPulseDamageMultiplier(int level, string finalId)
        {
            if (finalId == "standard_resonator_final_1") return 0.65f;
            if (finalId == "standard_resonator_final_2") return 0.45f;
            if (finalId == "standard_resonator_final_3") return 0.35f;
            return level >= 6 ? 0.55f : level >= 5 ? 0.50f : 0.45f;
        }

        public static float GetStandardResonatorPulseRadius(int level, string finalId)
        {
            if (finalId == "standard_resonator_final_1") return 1.15f;
            if (finalId == "standard_resonator_final_2") return 1.00f;
            if (finalId == "standard_resonator_final_3") return 0.90f;
            return level >= 6 ? 1.00f : level >= 5 ? 0.95f : 0.90f;
        }

        public static float GetStandardSingularityPullDistance(int level) => level >= 6 ? 3.6f : level >= 5 ? 3.0f : 2.2f;
        public static float GetStandardSingularitySlowDuration(int level) => level >= 6 ? 1.2f : level >= 5 ? 0.9f : 0.6f;

        public static int GetStandardSwarmShardCount(int level, string finalId)
        {
            if (finalId == "standard_swarm_final_1") return 1;
            if (finalId == "standard_swarm_final_2") return 2;
            if (finalId == "standard_swarm_final_3") return 3;
            return level >= 6 ? 2 : 1;
        }

        public static float GetStandardSwarmShardDamageMultiplier(int level) => level >= 6 ? 0.50f : level >= 5 ? 0.45f : 0.35f;

        public static int GetPrismPierce(int basePierce, string branch, string finalId)
        {
            var extra = branch == "prism_split" && finalId == "prism_split_final_2" ? 2
                : branch == "prism_mirror" && finalId == "prism_mirror_final_2" ? 1
                : branch == "prism_mirror" && finalId == "prism_mirror_final_3" ? 1 : 0;
            return Math.Max(0, basePierce) + extra;
        }

        public static int GetPrismMirrorBounces(string finalId) =>
            finalId == "prism_mirror_final_1" ? 1 : finalId == "prism_mirror_final_2" || finalId == "prism_mirror_final_3" ? 2 : 0;

        private static int[] GetOrbitalRingCounts(int level, string finalId)
        {
            var effectiveLevel = Math.Max(1, Math.Min(7, level));
            var inner = 1; var outer = 1;
            for (var currentLevel = 2; currentLevel <= effectiveLevel; currentLevel++)
                if (currentLevel % 2 == 0) outer++; else inner++;
            var extras = finalId == "orbital_dance_final_3" ? 1 : 0;
            for (var extra = 0; extra < extras; extra++)
                if (outer < inner) outer++; else if (inner < outer) inner++; else if (extra % 2 == 0) outer++; else inner++;
            return new[] { inner, outer };
        }
        public static int GetOrbitalInnerCount(int level, string finalId) => GetOrbitalRingCounts(level, finalId)[0];
        public static int GetOrbitalOuterCount(int level, string finalId) => GetOrbitalRingCounts(level, finalId)[1];
        public static float GetOrbitalAngularSpeed(int level, string finalId)
        {
            var speed = 1.8f + Math.Min(2.4f, Math.Max(1, level) * 0.28f);
            return finalId == "orbital_dance_final_2" ? speed * 1.55f : speed;
        }
        public static float GetOrbitalContactRadius(string finalId) =>
            finalId == "orbital_halo_final_2" ? 0.26f : finalId == "orbital_dance_final_3" ? 0.18f : 0.20f;
        public static float GetOrbitalDamageMultiplier(int level, string branch, string finalId)
        {
            if (branch == "orbital_dance") return 0.96f;
            if (branch == "orbital_halo") return 0.92f;
            if (branch == "orbital_blade")
            {
                var multiplier = finalId == "orbital_blade_final_3" ? 1.30f : 1.15f;
                if (string.IsNullOrEmpty(finalId)) multiplier *= GetChainBranchPower(level);
                return multiplier;
            }
            return 0.95f;
        }
        public static float GetOrbitalAfterimageMultiplier(string branch, string finalId)
        {
            if (branch == "orbital_dance" && finalId == "orbital_dance_final_1") return 0.28f;
            if (branch == "orbital_blade" && finalId == "orbital_blade_final_2") return 0.25f;
            if (branch == "orbital_blade" && finalId == "orbital_blade_final_3") return 0.32f;
            return 0f;
        }

        public static (float duration, float multiplier) GetGravityWellSlow(string finalId)
        {
            if (finalId == "gravity_well_final_1") return (0.75f, 0.55f);
            if (finalId == "gravity_well_final_2") return (0.75f, 0.62f);
            if (finalId == "gravity_well_final_3") return (0.75f, 0.50f);
            return (0.45f, 0.72f);
        }

        public static float GetGravityWellRadius(int level, string finalId)
        {
            if (finalId == "gravity_well_final_1") return 2.6f;
            if (finalId == "gravity_well_final_2") return 3.0f;
            if (finalId == "gravity_well_final_3") return 3.2f;
            return 2.5f * GetChainBranchPower(level);
        }

        public static float GetGravityWellPullDistance(int level, string finalId)
        {
            if (finalId == "gravity_well_final_1") return 0.35f;
            if (finalId == "gravity_well_final_2") return 0.50f;
            if (finalId == "gravity_well_final_3") return 0.58f;
            return 0.28f * GetChainBranchPower(level);
        }

        public static float GetGravityTideRadius(string finalId) =>
            finalId == "gravity_tide_final_2" ? 3.4f : 3f;

        public static float GetGravityTideDistance(int level) => level >= 6 ? 0.72f : level >= 5 ? 0.62f : 0.52f;

        public static float GetAuraSanctumSlowDuration(string finalId) =>
            finalId == "aura_sanctum_final_1" ? 1.4f : finalId == "aura_sanctum_final_2" ? 2f : finalId == "aura_sanctum_final_3" ? 0.9f : 0.8f;

        public static float GetAuraSanctumSlowMultiplier(string finalId) =>
            finalId == "aura_sanctum_final_1" ? 0.50f : finalId == "aura_sanctum_final_2" ? 0.42f : finalId == "aura_sanctum_final_3" ? 0.62f : 0.65f;

        public static float GetAuraGravityPullRadius(int level, string finalId)
        {
            if (finalId == "aura_gravity_final_1") return 3f;
            if (finalId == "aura_gravity_final_2" || finalId == "aura_gravity_final_3") return 3.8f;
            return 2.8f * GetChainBranchPower(level);
        }

        public static float GetAuraGravityPullDistance(int level, string finalId)
        {
            if (finalId == "aura_gravity_final_1") return 1.1f;
            if (finalId == "aura_gravity_final_2" || finalId == "aura_gravity_final_3") return 1.6f;
            return 0.55f * GetChainBranchPower(level);
        }

        public static float GetAuraOvergrowthRadius(int level, string finalId)
        {
            if (finalId == "aura_overgrowth_final_2") return 3.6f;
            if (finalId == "aura_overgrowth_final_1") return 2.8f;
            if (finalId == "aura_overgrowth_final_3") return 2.4f;
            return 2.2f * GetChainBranchPower(level);
        }

        public static float GetAuraOvergrowthAttackTimerReduction(int level, string finalId)
        {
            if (finalId == "aura_overgrowth_final_1") return 0.18f;
            if (finalId == "aura_overgrowth_final_2") return 0.30f;
            if (finalId == "aura_overgrowth_final_3") return 0.10f;
            return 0.08f * GetChainBranchPower(level);
        }

        public static float GetChainBranchPower(int level) => level >= 6 ? 1.30f : level >= 5 ? 1.15f : 1f;

        public static float GetChainWebSlowDuration(int level, string finalId)
        {
            var duration = finalId == "chain_web_final_1" ? 0.7f : finalId == "chain_web_final_2" ? 1.4f : finalId == "chain_web_final_3" ? 0.5f : 0.7f;
            return string.IsNullOrEmpty(finalId) ? duration * GetChainBranchPower(level) : duration;
        }

        public static float GetChainWebSlowMultiplier(int level, string finalId)
        {
            var multiplier = finalId == "chain_web_final_1" ? 0.70f : finalId == "chain_web_final_2" ? 0.55f : finalId == "chain_web_final_3" ? 0.72f : 0.70f;
            return string.IsNullOrEmpty(finalId) ? Math.Max(0.42f, multiplier - (GetChainBranchPower(level) - 1f) * 0.08f) : multiplier;
        }

        public static float GetChainStormRadius(int level, string finalId)
        {
            var radius = finalId == "chain_storm_final_1" ? 1.4f : finalId == "chain_storm_final_2" ? 2f : finalId == "chain_storm_final_3" ? 1.1f : 1.4f;
            return string.IsNullOrEmpty(finalId) ? radius * GetChainBranchPower(level) : radius;
        }

        public static float GetChainStormSplash(int level, string finalId)
        {
            var splash = finalId == "chain_storm_final_1" ? 0.25f : finalId == "chain_storm_final_2" ? 0.40f : finalId == "chain_storm_final_3" ? 0.20f : 0.22f;
            return string.IsNullOrEmpty(finalId) ? splash * GetChainBranchPower(level) : splash;
        }

        public static float GetChainLeechHealRatio(int level, string finalId)
        {
            var ratio = finalId == "chain_leech_final_1" ? 0.025f : finalId == "chain_leech_final_2" ? 0.045f : finalId == "chain_leech_final_3" ? 0.018f : 0.020f;
            return string.IsNullOrEmpty(finalId) ? ratio * GetChainBranchPower(level) : ratio;
        }

        public static float GetShotgunCataclysmRadius(int level, string finalId)
        {
            var radius = finalId == "shotgun_cataclysm_final_1" ? 1.20f
                : finalId == "shotgun_cataclysm_final_2" ? 1.70f
                : finalId == "shotgun_cataclysm_final_3" ? 1.10f : 1.05f;
            if (string.IsNullOrEmpty(finalId)) radius *= level >= 6 ? 1.30f : level >= 5 ? 1.15f : 1f;
            return radius;
        }

        public static float GetShotgunCataclysmSplash(int level, string finalId)
        {
            var splash = finalId == "shotgun_cataclysm_final_1" ? 0.45f
                : finalId == "shotgun_cataclysm_final_2" ? 0.65f
                : finalId == "shotgun_cataclysm_final_3" ? 0.35f : 0.30f;
            if (string.IsNullOrEmpty(finalId)) splash *= level >= 6 ? 1.30f : level >= 5 ? 1.15f : 1f;
            return splash;
        }

        public static float GetShotgunHailChance(int level, string finalId)
        {
            if (finalId == "shotgun_hail_final_1") return 0.25f;
            if (finalId == "shotgun_hail_final_2") return 0.40f;
            if (finalId == "shotgun_hail_final_3") return 0.32f;
            return level >= 6 ? 0.60f : level >= 5 ? 0.45f : 0.32f;
        }

        public static int GetShotgunHailShardCount(int level, string finalId)
        {
            if (finalId == "shotgun_hail_final_2") return 8;
            return string.IsNullOrEmpty(finalId) && level >= 6 ? 7 : 6;
        }

        public static float GetShotgunHailRadius(int level, string finalId)
        {
            if (finalId == "shotgun_hail_final_2") return 1.3f;
            return string.IsNullOrEmpty(finalId) ? (level >= 6 ? 1.2f : level >= 5 ? 1.04f : 0.9f) : 0.9f;
        }

        public static float GetShotgunHailShardDamageMultiplier(string finalId) =>
            finalId == "shotgun_hail_final_2" ? 0.22f : finalId == "shotgun_hail_final_3" ? 0.19f : 0.16f;

        public static float GetShotgunHailSplashMultiplier(string finalId) =>
            finalId == "shotgun_hail_final_2" ? 0.18f : 0.12f;

        public static float GetShotgunBurstCloseRangeThreshold(int level, string finalId)
        {
            var threshold = finalId == "shotgun_burst_final_2" ? 1.8f : 1.5f;
            if (level >= 6) threshold += 0.50f;
            else if (level >= 5) threshold += 0.25f;
            return threshold;
        }

        public static float GetShotgunBurstDamageMultiplier(int level, string finalId, float targetDistance)
        {
            var multiplier = 1.08f;
            if (finalId == "shotgun_burst_final_1") multiplier *= 1.20f;
            if (targetDistance <= GetShotgunBurstCloseRangeThreshold(level, finalId))
            {
                if (level >= 6) multiplier *= 1.25f;
                else if (level >= 5) multiplier *= 1.15f;

                if (finalId == "shotgun_burst_final_1") multiplier *= 1.30f;
                else if (finalId == "shotgun_burst_final_2") multiplier *= 1.50f;
                else if (finalId == "shotgun_burst_final_3") multiplier *= 1.22f;
            }
            return multiplier;
        }

        public static int GetShotgunBurstExtraPellets(int level, string finalId, float targetDistance)
        {
            if (finalId != "shotgun_burst_final_1" && finalId != "shotgun_burst_final_3") return 0;
            return targetDistance <= GetShotgunBurstCloseRangeThreshold(level, finalId) ? 2 : 0;
        }

        public static bool ShouldShotgunBurstApplySlow(int level, string finalId, float targetDistance)
        {
            if (finalId != "shotgun_burst_final_3") return false;
            var threshold = 0.9f + (level >= 6 ? 0.2f : level >= 5 ? 0.1f : 0f);
            return targetDistance <= threshold;
        }

        public static int GetShotgunCataclysmPierce(int basePierce, string finalId)
        {
            var branchPierce = 2;
            if (finalId == "shotgun_cataclysm_final_1") branchPierce += 2;
            return Math.Max(0, basePierce) + branchPierce;
        }

        public static int GetShotgunHailBonusPellets(string finalId) =>
            1 + (finalId == "shotgun_hail_final_1" ? 1 : 0);

        public static float GetSniperOracleCriticalMultiplier(string finalId)
        {
            if (finalId == "sniper_oracle_final_1") return 1.50f;
            if (finalId == "sniper_oracle_final_2") return 1.30f;
            if (finalId == "sniper_oracle_final_3") return 1.22f;
            return 1.15f;
        }

        public static float GetSniperOracleMarkDuration(string finalId) =>
            finalId == "sniper_oracle_final_2" ? 3.5f : finalId == "sniper_oracle_final_3" ? 3.0f : 2.5f;

        public static float GetSniperAssassinThreshold(string finalId, int level)
        {
            if (finalId == "sniper_assassin_final_1") return 0.45f;
            if (finalId == "sniper_assassin_final_2") return 0.35f;
            return level >= 6 ? 0.35f : 0.30f;
        }

        public static float GetSniperAssassinDamageMultiplier(int level, string finalId, float hpFraction)
        {
            var threshold = GetSniperAssassinThreshold(finalId, level);
            if (hpFraction > threshold) return 1f;
            if (finalId == "sniper_assassin_final_2" && hpFraction <= 0.20f) return 2.0f;
            if (finalId == "sniper_assassin_final_3") return level >= 6 ? 1.85f : 1.65f;
            return level >= 6 ? 1.80f : 1.50f;
        }

        public static float GetSniperAssassinKillHeal(string finalId) => finalId == "sniper_assassin_final_3" ? 7f : 0f;

        public static float GetSniperBeaconMarkDuration(int level, string finalId)
        {
            var duration = level >= 6 ? 3.5f : level >= 5 ? 3f : 2.25f;
            if (finalId == "sniper_beacon_final_2" || finalId == "sniper_beacon_final_3") duration += 1.5f;
            return duration;
        }

        public static float GetSniperBeaconRadius(int level, string finalId)
        {
            var radius = level >= 6 ? 2.2f : level >= 5 ? 1.8f : 1.45f;
            if (finalId == "sniper_beacon_final_2") radius *= 1.35f;
            if (finalId == "sniper_beacon_final_3") radius *= 1.15f;
            return radius;
        }

        public static float GetSniperBeaconSlowMultiplier(string finalId) =>
            finalId == "sniper_beacon_final_1" ? 0.45f : finalId == "sniper_beacon_final_3" ? 0.50f : 0.62f;
    }
}
