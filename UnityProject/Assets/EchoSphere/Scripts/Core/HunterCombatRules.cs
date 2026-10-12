namespace EchoSphere.Core
{
    /// <summary>Pure source-mapped rules for Hunter Prey Mark, Hunt, marked critical chance and Trophy.</summary>
    public static class HunterCombatRules
    {
        public static bool CanMarkTarget(SphereId sphere, bool isBoss, bool isElite, EnemyArchetype archetype)
        {
            var supportedSphere = sphere == SphereId.Sniper || sphere == SphereId.Chain ||
                sphere == SphereId.Prism || sphere == SphereId.Void;
            var priorityTarget = isBoss || isElite || archetype == EnemyArchetype.Tank;
            return supportedSphere && priorityTarget;
        }

        public static float GetMarkDuration(int masteryLevel) => masteryLevel >= 2 ? 6f : 5f;

        public static int GetHuntHitThreshold(int masteryLevel) => masteryLevel >= 4 ? 4 : 5;

        public static float GetMarkedDamageMultiplier(int masteryLevel, bool isMarked, bool huntActive)
        {
            if (!isMarked) return 1f;
            var mark = masteryLevel >= 6 ? 1.25f : 1.20f;
            var hunt = huntActive ? (masteryLevel >= 8 ? 1.35f : 1.30f) : 1f;
            return mark * hunt;
        }

        public static float GetMarkedTargetCritBonus(int masteryLevel, bool isMarked) =>
            isMarked && masteryLevel >= 3 ? 0.02f : 0f;

        public static float GetTrophyMoveSpeedBonus(int masteryLevel, bool trophyActive) =>
            trophyActive && masteryLevel >= 5 ? 0.10f : 0f;
    }
}
