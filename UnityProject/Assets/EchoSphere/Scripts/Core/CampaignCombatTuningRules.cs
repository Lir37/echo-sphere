using System;

namespace EchoSphere.Core
{
    /// <summary>
    /// Campaign-only tuning, separated from the generated source compatibility
    /// tables. See GPT/UNITY_COMBAT_BALANCE_2026-11-02.md for measured arenas.
    /// </summary>
    public static class CampaignCombatTuningRules
    {
        // Pulse used to exceed Sniper's single-target output at level I while
        // hitting every target in its larger area. Preserve late evolution power
        // with a gradual 0.60 -> 0.80 factor, without changing cadence or effects.
        public static float SphereDamageMultiplier(SphereId type, int level) =>
            type == SphereId.Pulse
                ? .60f + .20f * (Math.Max(1, Math.Min(7, level)) - 1) / 6f
                : 1f;
    }
}
