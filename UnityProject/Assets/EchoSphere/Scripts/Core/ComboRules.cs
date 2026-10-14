using System;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped kill combo and XP thresholds from new-desing/src/engineCombat.ts
    /// and src/engineLoop.ts. Presentation is kept outside this deterministic contract.
    /// </summary>
    public static class ComboRules
    {
        public const float WindowSeconds = 3f;

        public static int IncrementOnKill(int currentCombo, bool isBoss) =>
            isBoss ? Math.Max(0, currentCombo) : Math.Max(0, currentCombo) + 1;

        public static float GetExperienceMultiplier(int combo)
        {
            var current = Math.Max(0, combo);
            if (current >= 100) return 4f;
            if (current >= 50) return 3f;
            if (current >= 25) return 2f;
            if (current >= 10) return 1.5f;
            return 1f;
        }

        public static float TickTimer(float remainingSeconds, float deltaTime) =>
            Math.Max(0f, Math.Max(0f, remainingSeconds) - Math.Max(0f, deltaTime));

        /// <summary>Matches JavaScript Math.round for the non-negative XP orb values used by the source.</summary>
        public static int GetExperienceOrbValue(float baseExperience, float multiplier)
        {
            var scaled = Math.Max(0, baseExperience) * (double)Math.Max(0f, multiplier);
            return Math.Max(0, (int)Math.Floor(scaled + 0.5d));
        }
    }
}
