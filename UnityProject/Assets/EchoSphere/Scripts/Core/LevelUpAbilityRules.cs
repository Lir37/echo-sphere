using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Level-Up passive abilities from new-desing/src/gameData.ts,
    /// engineStats.ts and engineCombat.ts. Only abilities whose effects are wired
    /// in Unity are offered; active abilities and incomplete passive effects stay
    /// out of the choice pool instead of appearing functional when they are not.
    /// </summary>
    public static class LevelUpAbilityRules
    {
        private static readonly RunUpgradeKind[] RoutineKinds =
        {
            RunUpgradeKind.Damage, RunUpgradeKind.AttackSpeed, RunUpgradeKind.MoveSpeed,
            RunUpgradeKind.Slow, RunUpgradeKind.Vitality, RunUpgradeKind.Dodge,
            RunUpgradeKind.Crit, RunUpgradeKind.Magnet, RunUpgradeKind.Armor,
            RunUpgradeKind.Regen, RunUpgradeKind.Stability
        };

        public static IReadOnlyList<RunUpgradeKind> RoutineAbilityKinds => RoutineKinds;

        public static int GetMaxLevel(RunUpgradeKind kind) =>
            kind == RunUpgradeKind.MaxSpheres ? 3 : 7;

        public static bool IsAbilityChoice(RunUpgradeKind kind) => kind != RunUpgradeKind.Sphere;

        public static bool CanUpgrade(RunUpgradeKind kind, int currentLevel) =>
            IsAbilityChoice(kind) && currentLevel >= 0 && currentLevel < GetMaxLevel(kind);

        public static RunUpgradeChoice CreateChoice(RunUpgradeKind kind, int currentLevel)
        {
            var current = Math.Max(0, Math.Min(GetMaxLevel(kind), currentLevel));
            var next = Math.Min(GetMaxLevel(kind), current + 1);
            string title;
            string description;
            switch (kind)
            {
                case RunUpgradeKind.Damage:
                    title = "SPHERE DAMAGE / " + next;
                    description = "+15% Sphere damage per Ability level.";
                    break;
                case RunUpgradeKind.AttackSpeed:
                    title = "ATTACK SPEED / " + next;
                    description = "Sphere attack delay uses the source 0.93^level multiplier.";
                    break;
                case RunUpgradeKind.MaxSpheres:
                    title = "MAX SPHERES / " + next;
                    description = "+1 maximum active Sphere.";
                    break;
                case RunUpgradeKind.MoveSpeed:
                    title = "MOVEMENT SPEED / " + next;
                    description = "+10% player movement speed per level.";
                    break;
                case RunUpgradeKind.Slow:
                    title = "ENEMY SLOW / " + next;
                    description = "-" + (10 + (next - 1) * 5) + "% enemy movement speed.";
                    break;
                case RunUpgradeKind.Vitality:
                    title = "VITALITY / " + next;
                    description = "+20 maximum Core HP; restores up to 20 HP.";
                    break;
                case RunUpgradeKind.Dodge:
                    title = "DODGE / " + next;
                    description = "+10% dodge chance per level, subject to the source 75% cap.";
                    break;
                case RunUpgradeKind.Crit:
                    title = "CRITICAL CHANCE / " + next;
                    description = "+10% critical-hit chance per level, subject to the source 75% cap.";
                    break;
                case RunUpgradeKind.Magnet:
                    title = "XP MAGNET / " + next;
                    description = "+20% XP attraction radius per level.";
                    break;
                case RunUpgradeKind.Armor:
                    title = "STABLE HULL / " + next;
                    description = "-" + (next * 4) + "% incoming damage.";
                    break;
                case RunUpgradeKind.Regen:
                    title = "REGENERATION / " + next;
                    description = "+" + (next * 0.6f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " Core HP per second.";
                    break;
                case RunUpgradeKind.Stability:
                    title = "NETWORK STABILITY / " + next;
                    description = "-" + (next * 12) + "% network-disable duration, with the source 16% floor.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a source-mapped routine Ability.");
            }
            return new RunUpgradeChoice(kind, SphereId.Standard, current, next, title, description);
        }

        public static float GetSphereDamageMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.15f;

        public static float GetAttackSpeedMultiplier(int level) =>
            1f / (float)Math.Pow(0.93d, Math.Max(0, Math.Min(7, level)));

        public static float GetMoveSpeedMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.10f;

        public static float GetEnemySlowMultiplier(int level)
        {
            var current = Math.Max(0, Math.Min(7, level));
            return current == 0 ? 1f : 1f - (0.10f + (current - 1) * 0.05f);
        }

        public static float GetDodgeChance(int level, float artifactBonus = 0f) =>
            Math.Min(0.75f, Math.Max(0, level) * 0.10f + Math.Max(0f, artifactBonus));

        public static float GetCritChanceBonus(int level) =>
            Math.Max(0, level) * 0.10f;

        public static float GetDamageTakenMultiplier(int level) =>
            Math.Max(0.45f, 1f - Math.Max(0, Math.Min(7, level)) * 0.04f);

        public static float GetRegenPerSecond(int level) =>
            Math.Max(0, level) * 0.6f;

        public static float GetMagnetRadiusWorld(int level) =>
            60f * SpherePlacementRules.SourcePixelsToWorld * (1f + Math.Max(0, level) * 0.20f);

        public static float GetNetworkDisableDuration(float baseDuration, int level) =>
            Math.Max(0f, baseDuration) * Math.Max(0.16f, 1f - Math.Max(0, level) * 0.12f);
    }
}
