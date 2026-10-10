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
