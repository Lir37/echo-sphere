using System;

namespace EchoSphere.Core
{
    // Active source contracts from engineLoop.ts, engineStats.ts and engineState.ts.
    public static class PlayerLoopRules
    {
        public const float DashDuration = 0.2f;
        public const float DashCooldown = 3f;
        public const float DashSpeedPixels = 600f;
        public const float DeathDuration = 0.90f;
        public const float PlayerRadiusPixels = 16f;
        public const float OrbPickupRadiusPixels = 22f;
        public static int MutationStage(int level) => level >= 40 ? 4 : level >= 30 ? 3 : level >= 20 ? 2 : level >= 10 ? 1 : 0;
        public static float MutationDamage(int stage) => stage >= 1 ? 1.1f : 1f;
        public static float MutationRadius(int stage) => stage >= 2 ? 1.15f : 1f;
        public static float MutationSpeed(int stage) => stage >= 3 ? 1.2f : 1f;
        public static float MutationRegen(int stage) => stage >= 4 ? 1.25f : 0f;
        public static float MagnetRadius(int level, int magnetic) => 60f * (1f + level * 0.2f) * (magnetic > 0 ? 1f + magnetic * 0.15f : 1f);
        public static uint CreateRunSeed(string playerName, string difficulty, long epochMilliseconds)
        {
            var text = playerName + "|" + difficulty + "|" + epochMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
            uint hash = 2166136261;
            unchecked { foreach (var character in text) { hash ^= character; hash *= 16777619; } }
            return hash == 0 ? 1u : hash;
        }
    }
}
