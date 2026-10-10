namespace EchoSphere.Core
{
    public enum EnemyArchetype { Normal, Fast, Tank, Elite }
    public readonly struct EnemyArchetypeProfile
    {
        public readonly float HpMultiplier, SpeedMultiplier, VisualScale;
        public readonly int XpReward;
        public EnemyArchetypeProfile(float hpMultiplier, float speedMultiplier, float visualScale, int xpReward)
        { HpMultiplier = hpMultiplier; SpeedMultiplier = speedMultiplier; VisualScale = visualScale; XpReward = xpReward; }
    }
    /// <summary>Deterministic enemy-role selection and scaling for the Unity migration harness.</summary>
    public static class EnemySpawnRules
    {
        public static EnemyArchetype Select(float elapsedSeconds, float roll)
        {
            var time = IsFinite(elapsedSeconds) ? System.Math.Max(0f, elapsedSeconds) : 0f;
            var value = IsFinite(roll) ? System.Math.Max(0f, System.Math.Min(0.999999f, roll)) : 0.5f;
            if (time < 12f) return EnemyArchetype.Normal;
            if (time < 35f) { if (value < 0.24f) return EnemyArchetype.Fast; if (time >= 24f && value < 0.34f) return EnemyArchetype.Tank; return EnemyArchetype.Normal; }
            if (time < 75f) { if (value < 0.26f) return EnemyArchetype.Fast; if (value < 0.44f) return EnemyArchetype.Tank; return EnemyArchetype.Normal; }
            if (value < 0.27f) return EnemyArchetype.Fast;
            if (value < 0.49f) return EnemyArchetype.Tank;
            if (value < 0.59f) return EnemyArchetype.Elite;
            return EnemyArchetype.Normal;
        }
        public static EnemyArchetypeProfile GetProfile(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Fast: return new EnemyArchetypeProfile(0.62f, 1.48f, 0.76f, 1);
                case EnemyArchetype.Tank: return new EnemyArchetypeProfile(2.55f, 0.58f, 1.30f, 3);
                case EnemyArchetype.Elite: return new EnemyArchetypeProfile(4.0f, 0.82f, 1.42f, 5);
                default: return new EnemyArchetypeProfile(1f, 1f, 1f, 1);
            }
        }
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
