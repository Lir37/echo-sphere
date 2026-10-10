namespace EchoSphere.Core
{
    /// <summary>
    /// Balance values ported from new-desing/src/engineBalance.ts.
    /// Keep formulas deterministic and independent of UnityEngine.
    /// </summary>
    public static class RunBalanceRules
    {
        public const int StartingHp = 100;
        public const int HpPerLevel = 6;
        public const int FirstLevelXp = 10;
        public const int XpGrowthLinear = 3;
        public const float XpGrowthQuadratic = 0.08f;

        /// <summary>
        /// Mirrors getXpToNextLevel(level) from engineState.ts.
        /// JavaScript Math.round for this non-negative formula is reproduced
        /// with floor(value + 0.5), avoiding C# banker's rounding differences.
        /// </summary>
        public static int GetXpToNextLevel(int level)
        {
            var normalizedLevel = level < 1 ? 1 : level;
            var offset = normalizedLevel - 1;
            var value = FirstLevelXp
                + offset * XpGrowthLinear
                + offset * offset * XpGrowthQuadratic;
            var rounded = (int)System.Math.Floor(value + 0.5d);
            return rounded < FirstLevelXp ? FirstLevelXp : rounded;
        }
    }
}
