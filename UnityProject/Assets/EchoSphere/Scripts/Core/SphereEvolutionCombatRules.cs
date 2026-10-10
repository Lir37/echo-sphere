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
    }
}
