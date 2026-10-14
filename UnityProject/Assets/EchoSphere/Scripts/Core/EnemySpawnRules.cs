namespace EchoSphere.Core
{
    public enum EnemyArchetype { Normal, Fast, Tank, Elite }

    public readonly struct EnemyArchetypeProfile
    {
        public readonly float HpBase, HpPerWave;
        public readonly float SpeedBase, SpeedPerWave;
        public readonly float DamageBase, DamagePerWave;
        public readonly float Radius, VisualScale;
        public readonly int XpReward;

        public EnemyArchetypeProfile(float hpBase, float hpPerWave, float speedBase, float speedPerWave,
            float damageBase, float damagePerWave, float radius, float visualScale, int xpReward)
        {
            HpBase = hpBase;
            HpPerWave = hpPerWave;
            SpeedBase = speedBase;
            SpeedPerWave = speedPerWave;
            DamageBase = damageBase;
            DamagePerWave = damagePerWave;
            Radius = radius;
            VisualScale = visualScale;
            XpReward = xpReward;
        }
    }

    /// <summary>
    /// Source-mapped base archetype stats and elite conversion from
    /// new-desing/src/engineEnemies.ts and src/engineBalance.ts.
    /// </summary>
    public static class EnemySpawnRules
    {
        public static EnemyArchetype SelectBase(int wave, float roll)
        {
            var normalizedWave = System.Math.Max(0, wave);
            var value = NormalizeRoll(roll);
            if (value < 0.2f && normalizedWave > 2) return EnemyArchetype.Fast;
            if (value < 0.35f && normalizedWave > 4) return EnemyArchetype.Tank;
            return EnemyArchetype.Normal;
        }

        public static bool ShouldBeElite(int wave, float roll)
        {
            var normalizedWave = System.Math.Max(0, wave);
            return normalizedWave > 5 &&
                NormalizeRoll(roll) < System.Math.Min(0.12f, 0.03f + normalizedWave * 0.005f);
        }

        public static EnemyArchetypeProfile GetProfile(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Fast:
                    return new EnemyArchetypeProfile(11f, 3.4f, 125f, 2.4f, 8f, 0.32f, 15f, 0.76f, 2);
                case EnemyArchetype.Tank:
                    return new EnemyArchetypeProfile(42f, 10.5f, 44f, 0.8f, 11f, 0.38f, 26f, 1.30f, 4);
                default:
                    // Elite is a conversion of this base archetype, not a fourth
                    // independently rolled archetype. Its conversion is applied in SpawnEnemy.
                    return new EnemyArchetypeProfile(18f, 5.5f, 68f, 1.8f, 7f, 0.30f, 19f, 1f, 1);
            }
        }

        private static float NormalizeRoll(float roll)
        {
            if (float.IsNaN(roll) || float.IsInfinity(roll)) return 0.5f;
            return System.Math.Max(0f, System.Math.Min(0.999999f, roll));
        }
    }
}
