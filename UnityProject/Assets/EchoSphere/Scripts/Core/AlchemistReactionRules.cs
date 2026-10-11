using System;

namespace EchoSphere.Core
{
    public enum AlchemistReactionKind
    {
        None = 0,
        ToxinIgnition = 1,
        Cryotoxin = 2,
        ThermalShock = 3
    }

    /// <summary>
    /// Source-mapped reaction selection and mastery numbers from new-desing/src/characterRuntime.ts.
    /// </summary>
    public static class AlchemistReactionRules
    {
        public static AlchemistReactionKind Resolve(bool fireActive, bool freezeActive, bool poisonActive)
        {
            if (fireActive && poisonActive) return AlchemistReactionKind.ToxinIgnition;
            if (freezeActive && poisonActive) return AlchemistReactionKind.Cryotoxin;
            if (fireActive && freezeActive) return AlchemistReactionKind.ThermalShock;
            return AlchemistReactionKind.None;
        }

        public static float GetBaseDamage(int playerLevel) => 20f + Math.Max(1, playerLevel) * 2f;

        public static float GetBurstMultiplier(AlchemistReactionKind kind, int masteryLevel, int reactionCount)
        {
            var multiplier = kind == AlchemistReactionKind.ToxinIgnition ? 2f
                : kind == AlchemistReactionKind.Cryotoxin ? 1.6f
                : kind == AlchemistReactionKind.ThermalShock ? 1.8f : 0f;
            if (multiplier <= 0f) return 0f;
            if (masteryLevel >= 7) multiplier *= 1.05f;
            if (masteryLevel >= 9 && reactionCount > 0 && reactionCount % 3 == 0) multiplier *= 1.25f;
            return multiplier;
        }

        public static int GetReactionRadiusSourcePixels(int masteryLevel) =>
            masteryLevel >= 6 ? 70 : masteryLevel >= 2 ? 60 : 55;

        public static int GetSplashTargetCount(int masteryLevel) =>
            masteryLevel >= 10 ? 2 : masteryLevel >= 4 ? 1 : 0;

        public static float GetSplashDamageMultiplier() => 0.5f;
        public static float GetSlowDuration(AlchemistReactionKind kind) =>
            kind == AlchemistReactionKind.Cryotoxin || kind == AlchemistReactionKind.ThermalShock ? 2f : 0f;

        public static float GetSlowMultiplier(AlchemistReactionKind kind) =>
            kind == AlchemistReactionKind.Cryotoxin ? 0.35f
            : kind == AlchemistReactionKind.ThermalShock ? 0.40f : 1f;

        public static float GetCatalystDurationSeconds(int masteryLevel) => masteryLevel >= 5 ? 2f : 0f;
    }
}
