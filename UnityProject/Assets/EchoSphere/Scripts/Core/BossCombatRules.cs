using System;

namespace EchoSphere.Core
{
    /// <summary>Canonical boss roster and balance values from gameData.ts, engineEnemies.ts and bossBalance.ts.</summary>
    public enum BossChargerPhase { Ready, Windup, CommittedDash }

    public static class BossCombatRules
    {
        private static readonly string[] BossTypes =
        {
            "shooter", "charger", "summoner", "aura",
            "conductor", "architect", "null", "stella_warden"
        };

        public const float SourceUnitsPerWorldUnit = 720f / 12.6f;
        public const float BossRadiusSourceUnits = 34f;
        public const float PlayerRadiusSourceUnits = 16f;
        public const float ChargerWindupSeconds = 0.45f;
        public const float ChargerCommitSeconds = 0.35f;
        public const float ChargerTotalTelegraphSeconds = ChargerWindupSeconds + ChargerCommitSeconds;
        public const float ShooterTelegraphSeconds = 0.70f;
        public const float SummonerTelegraphSeconds = 0.90f;
        public const float AuraTelegraphSeconds = 0.80f;
        public const float BossSpawnDistanceSourceUnits = 700f;

        public static int BossTypeCount => BossTypes.Length;
        public static float BossRadiusWorld => BossRadiusSourceUnits / SourceUnitsPerWorldUnit;
        public static float PlayerRadiusWorld => PlayerRadiusSourceUnits / SourceUnitsPerWorldUnit;
        public static float BossSpawnDistanceWorld => BossSpawnDistanceSourceUnits / SourceUnitsPerWorldUnit;

        public static string GetTypeForDefeatedCount(int count)
        {
            var index = count % BossTypes.Length;
            if (index < 0) index += BossTypes.Length;
            return BossTypes[index];
        }

        public static bool IsKnownBossType(string type)
        {
            for (var i = 0; i < BossTypes.Length; i++)
                if (string.Equals(BossTypes[i], type, StringComparison.Ordinal)) return true;
            return false;
        }

        public static float GetMaxHp(int wave) => 450f + SafeWave(wave) * 55f;
        public static float GetContactDamage(int wave) => 28f + SafeWave(wave) * 0.65f;
        public static int GetXpReward(int wave) => 50 + SafeWave(wave) * 5;

        public static float GetMoveSpeedSourceUnitsPerSecond(string type, int wave) =>
            string.Equals(type, "charger", StringComparison.Ordinal)
                ? 72f + SafeWave(wave) * 1.8f
                : 48f + SafeWave(wave) * 1.2f;

        public static float ToWorldUnits(float sourceUnits) => sourceUnits / SourceUnitsPerWorldUnit;
        public static float GetMoveSpeedWorldUnitsPerSecond(string type, int wave) =>
            ToWorldUnits(GetMoveSpeedSourceUnitsPerSecond(type, wave));

        public static float GetAuraRadiusWorld() => ToWorldUnits(120f);
        public static float GetAuraDamagePerSecond(int wave) => 10f + SafeWave(wave) * 2f;

        public static BossChargerPhase GetChargerPhase(float timer)
        {
            if (timer <= 0f) return BossChargerPhase.Ready;
            return timer <= ChargerCommitSeconds ? BossChargerPhase.CommittedDash : BossChargerPhase.Windup;
        }

        public static string GetColorHex(string type)
        {
            switch (type)
            {
                case "shooter": return "#ff6a5f";
                case "charger": return "#ff9c3d";
                case "summoner": return "#a27cff";
                case "aura": return "#ff62b9";
                case "conductor": return "#4fd8ff";
                case "architect": return "#ffc56a";
                case "null": return "#9b7cff";
                case "stella_warden": return "#ffd15a";
                default: return "#ff6a5f";
            }
        }

        public static string GetVisualVariant(string type)
        {
            switch (type)
            {
                case "charger": return "brute";
                case "summoner": return "prism";
                case "aura": return "wisp";
                case "conductor": return "serpent";
                case "architect": return "prism";
                case "null": return "leech";
                case "stella_warden": return "moth";
                default: return "beetle";
            }
        }

        private static int SafeWave(int wave) => Math.Max(0, wave);
    }
}
