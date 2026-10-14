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

        // Exact src/region.ts:getRegionResonanceMultiplier().
        public static float GetResonanceMultiplier(
            EchoRegionId region, string challengeId, float elapsedSeconds, float currentCharge)
        {
            if (region == EchoRegionId.SpectralRift && challengeId == "volatile_core")
                return currentCharge >= 70f ? 1.28f : 1.12f;
            return System.Math.Sin(elapsedSeconds / 18.0) > 0.62 ? 1.18f : 1f;
        }

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

    /// <summary>
    /// Exact regional ecology multipliers and nearest-pocket selection from
    /// src/region.ts. Source coordinates use the same 720px-to-world conversion
    /// as RegionFieldVisuals; pocket selection is by nearest center, not radius.
    /// </summary>
    public readonly struct RegionPocketMultipliers
    {
        public readonly float Hp;
        public readonly float Speed;
        public readonly float Damage;
        public readonly float Xp;

        public RegionPocketMultipliers(float hp, float speed, float damage, float xp)
        {
            Hp = hp;
            Speed = speed;
            Damage = damage;
            Xp = xp;
        }
    }

    public static class RegionPocketRules
    {
        private const float SourcePixelsToWorld = 12.6f / 720f;

        private readonly struct Pocket
        {
            public readonly float X;
            public readonly float Y;
            public readonly int Ecology;

            public Pocket(float x, float y, int ecology)
            {
                X = x * SourcePixelsToWorld;
                Y = y * SourcePixelsToWorld;
                Ecology = ecology;
            }
        }

        // Ecology IDs: 0 flow, 1 swarm, 2 pressure, 3 fracture, 4 hollow.
        private static readonly Pocket[] Basin =
        {
            new Pocket(0f, 0f, 0), new Pocket(520f, -260f, 1),
            new Pocket(500f, 430f, 3), new Pocket(-480f, 430f, 4),
            new Pocket(-520f, -300f, 2)
        };

        private static readonly Pocket[] Rift =
        {
            new Pocket(0f, 0f, 0), new Pocket(520f, -270f, 1),
            new Pocket(470f, 450f, 2), new Pocket(-500f, 430f, 3),
            new Pocket(-520f, -290f, 4)
        };

        public static RegionPocketMultipliers GetAt(EchoRegionId region, float worldX, float worldY)
        {
            var pockets = region == EchoRegionId.SpectralRift ? Rift : Basin;
            var nearest = pockets[0];
            var nearestDistance = float.PositiveInfinity;
            for (var i = 0; i < pockets.Length; i++)
            {
                var dx = pockets[i].X - worldX;
                var dy = pockets[i].Y - worldY;
                var distance = dx * dx + dy * dy;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = pockets[i];
                }
            }

            switch (nearest.Ecology)
            {
                case 1: return new RegionPocketMultipliers(0.82f, 1.22f, 0.88f, 1.05f);
                case 2: return new RegionPocketMultipliers(1.28f, 0.86f, 1.18f, 1.22f);
                case 3: return new RegionPocketMultipliers(0.94f, 1.10f, 1.08f, 1.15f);
                case 4: return new RegionPocketMultipliers(1.08f, 0.96f, 1.12f, 1.18f);
                default: return new RegionPocketMultipliers(1f, 1f, 1f, 1f);
            }
        }

        public static string SelectRole(EchoRegionId region, float worldX, float worldY, float roll)
        {
            var pockets = region == EchoRegionId.SpectralRift ? Rift : Basin;
            var nearest = pockets[0];
            var nearestDistance = float.PositiveInfinity;
            for (var i = 0; i < pockets.Length; i++)
            {
                var dx = pockets[i].X - worldX;
                var dy = pockets[i].Y - worldY;
                var distance = dx * dx + dy * dy;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = pockets[i];
                }
            }

            var value = float.IsNaN(roll) || float.IsInfinity(roll)
                ? 0.5f : System.Math.Max(0f, System.Math.Min(0.999999f, roll));
            // Exact ecology pools from src/region.ts:getRegionEnemyRole.
            string[] roles;
            switch (nearest.Ecology)
            {
                case 1: roles = new[] { "swarmer", "splitter", "scavenger", "phase" }; break;
                case 2: roles = new[] { "tank_guard", "ranged", "sniper", "healer" }; break;
                case 3: roles = new[] { "phase", "disruptor", "corruptor", "charger" }; break;
                case 4: roles = new[] { "leech", "bomber", "anchor", "tank_guard" }; break;
                default: roles = new[] { "grunt", "ranged", "charger", "anchor" }; break;
            }
            return roles[System.Math.Min(roles.Length - 1, (int)(value * roles.Length))];
        }

        public static bool SelectsPhaseRole(EchoRegionId region, float worldX, float worldY, float roll) =>
            SelectRole(region, worldX, worldY, roll) == "phase";
    }
}
