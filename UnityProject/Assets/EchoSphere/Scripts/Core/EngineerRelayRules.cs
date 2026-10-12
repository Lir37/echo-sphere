using System;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Engineer relay contracts from src/engineCombat.ts and
    /// src/characterRuntime.ts. Distances are source logical pixels.
    /// </summary>
    public static class EngineerRelayRules
    {
        public static int GetNetworkRangePixels(int masteryLevel) =>
            masteryLevel >= 6 ? 260 : masteryLevel >= 2 ? 240 : 220;

        public static float GetWindowSeconds(int masteryLevel) =>
            masteryLevel >= 4 ? 0.55f : 0.40f;

        public static float GetDamageMultiplier() => 1.25f;

        public static bool ConsumesOnAttack(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard:
                case SphereId.Sniper:
                case SphereId.Shotgun:
                case SphereId.Chain:
                case SphereId.Void:
                    return true;
                default:
                    // These types use their own source update paths and do not
                    // consume the generic projectile-loop relay bonus.
                    return false;
            }
        }

        public static bool CanConsumeRelay(
            bool isEngineer, bool hasSource, float timeLeft, bool isSourceSphere,
            float distancePixels, int masteryLevel)
        {
            if (!isEngineer || !hasSource || isSourceSphere ||
                !IsFinite(timeLeft) || timeLeft <= 0f ||
                !IsFinite(distancePixels) || distancePixels < 0f)
                return false;

            return distancePixels <= GetNetworkRangePixels(masteryLevel);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
