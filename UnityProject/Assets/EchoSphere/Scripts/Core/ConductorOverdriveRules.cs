namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Conductor Overdrive and Resonance Cascade thresholds.
    /// Values and event cadence come from src/engineResonance.ts,
    /// src/characterRuntime.ts and src/characterMastery.ts.
    /// </summary>
    public static class ConductorOverdriveRules
    {
        public static float GetOverdriveDurationSeconds(int masteryLevel) =>
            masteryLevel >= 2 ? 2.5f : 2f;

        public static float GetAttackSpeedBonus(int masteryLevel) =>
            masteryLevel >= 8 ? 0.28f : masteryLevel >= 4 ? 0.23f : 0.18f;

        public static int GetLineSurgeCharges(int masteryLevel) =>
            masteryLevel >= 7 ? 2 : 1;

        public static float GetTriangleDischargeDamageMultiplier(int masteryLevel) =>
            masteryLevel >= 8 ? 0.55f : 0.40f;

        public static float GetClusterAttackAdvanceSeconds(int masteryLevel) =>
            masteryLevel >= 8 ? 0.48f : 0.32f;

        public static int GetSquareShieldChargeBonus(int masteryLevel) =>
            masteryLevel >= 8 ? 2 : 1;

        public static bool ShouldTriggerCascade(int masteryLevel, int resonanceEventCount) =>
            masteryLevel >= 6 && resonanceEventCount > 0 && resonanceEventCount % 3 == 0;

        public static int GetCascadeTargetCount(int masteryLevel) =>
            masteryLevel >= 9 ? 3 : masteryLevel >= 7 ? 2 : 1;

        public static float GetCascadeDamageMultiplier() => 0.55f;
    }
}
