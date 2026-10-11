namespace EchoSphere.Core
{
    public enum EchoRegionId { ResonanceBasin, SpectralRift }
    public enum EchoRegionMode { Stabilization, Endless }
    public enum RegionBossSignalKind { None, SpawnBoss, RegionCleared }

    public readonly struct RegionBossSignal
    {
        public RegionBossSignalKind Kind { get; }
        public string BossType { get; }
        public bool IsFinalBoss { get; }

        public RegionBossSignal(RegionBossSignalKind kind, string bossType = "", bool isFinalBoss = false)
        {
            Kind = kind;
            BossType = bossType ?? "";
            IsFinalBoss = isFinalBoss;
        }
    }

    /// <summary>
    /// Boss-spawn subset of src/region.ts: createRegionState/updateRegion.
    /// POIs, challenges, lore and region mastery remain separate unported work.
    /// </summary>
    public sealed class RegionBossScheduleState
    {
        public EchoRegionId RegionId { get; }
        public EchoRegionMode Mode { get; }
        public int MajorBossesSpawned { get; internal set; }
        public float NextBossAt { get; internal set; }
        public bool FinalBossSpawned { get; internal set; }
        public bool FinalBossDefeated { get; internal set; }
        public bool Cleared { get; internal set; }

        internal RegionBossScheduleState(EchoRegionId regionId, EchoRegionMode mode, float firstBossAt)
        {
            RegionId = regionId;
            Mode = mode;
            NextBossAt = firstBossAt;
        }
    }

    public static class RegionBossScheduleRules
    {
        public const float FinalPrepSeconds = 1620f;
        public const float StabilizationSeconds = 1800f;
        public const float EndlessBossIntervalSeconds = 150f;

        public static RegionBossScheduleState Create(
            EchoRegionId regionId = EchoRegionId.ResonanceBasin,
            EchoRegionMode mode = EchoRegionMode.Stabilization)
        {
            return new RegionBossScheduleState(regionId, mode, GetRegularBossTime(regionId, 0));
        }

        public static int GetRegularBossCount(EchoRegionId regionId) => 2;

        public static float GetRegularBossTime(EchoRegionId regionId, int index)
        {
            if (index <= 0) return regionId == EchoRegionId.SpectralRift ? 450f : 480f;
            if (index == 1) return regionId == EchoRegionId.SpectralRift ? 930f : 960f;
            return float.PositiveInfinity;
        }

        public static string GetRegularBossType(EchoRegionId regionId, int index)
        {
            if (regionId == EchoRegionId.SpectralRift)
                return index <= 0 ? "null" : "aura";
            return index <= 0 ? "conductor" : "architect";
        }

        public static string GetFinalBossType(EchoRegionId regionId) => "stella_warden";

        public static int GetBaseGoldReward(EchoRegionId regionId) =>
            regionId == EchoRegionId.SpectralRift ? 360 : 250;

        public static int GetRegionXpReward(int playerLevel) => System.Math.Max(1, playerLevel * 24);

        public static RegionBossSignal Advance(RegionBossScheduleState state, float elapsedSeconds, bool bossActive)
        {
            if (state == null || state.Cleared || !IsFinite(elapsedSeconds))
                return new RegionBossSignal(RegionBossSignalKind.None);

            var time = System.Math.Max(0f, elapsedSeconds);
            if (state.Mode == EchoRegionMode.Endless)
            {
                if (time < state.NextBossAt || bossActive)
                    return new RegionBossSignal(RegionBossSignalKind.None);

                var typeIndex = state.MajorBossesSpawned;
                var type = GetEndlessBossType(state.RegionId, typeIndex);
                state.MajorBossesSpawned++;
                state.NextBossAt += EndlessBossIntervalSeconds;
                return new RegionBossSignal(RegionBossSignalKind.SpawnBoss, type);
            }

            // This order mirrors updateRegion: scheduled major bosses get priority
            // over the final-boss signal if the player still has an earlier boss alive.
            if (state.MajorBossesSpawned < GetRegularBossCount(state.RegionId))
            {
                var index = state.MajorBossesSpawned;
                if (time >= GetRegularBossTime(state.RegionId, index) && !bossActive)
                {
                    state.MajorBossesSpawned++;
                    return new RegionBossSignal(RegionBossSignalKind.SpawnBoss, GetRegularBossType(state.RegionId, index));
                }
            }

            if (!state.FinalBossSpawned && time >= FinalPrepSeconds && !bossActive)
            {
                state.FinalBossSpawned = true;
                return new RegionBossSignal(RegionBossSignalKind.SpawnBoss, GetFinalBossType(state.RegionId), true);
            }

            if (state.FinalBossSpawned && time >= StabilizationSeconds && !bossActive)
            {
                state.FinalBossDefeated = true;
                state.Cleared = true;
                return new RegionBossSignal(RegionBossSignalKind.RegionCleared);
            }

            return new RegionBossSignal(RegionBossSignalKind.None);
        }

        private static string GetEndlessBossType(EchoRegionId regionId, int index)
        {
            if (regionId == EchoRegionId.SpectralRift)
                return index == 0 ? "null" : index == 1 ? "aura" : "stella_warden";
            return index == 0 ? "conductor" : index == 1 ? "architect" : "stella_warden";
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
