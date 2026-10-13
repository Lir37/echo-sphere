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
                case RunUpgradeKind.Teleport:
                    title = "ECHO JUMP / " + next;
                    description = "Teleport to a nearby Sphere; cooldown " + Math.Max(5, 15 - Math.Min(4, (next - 1) * 2)) + "s.";
                    break;
                case RunUpgradeKind.TimeStop:
                    title = "ECHO FREEZE / " + next;
                    description = "Freeze enemies for " + (3 + Math.Min(2, next - 1)) + "s; cooldown 40s.";
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

        /// <summary>Source evolution choices for the two currently ported active abilities.</summary>
        public static IReadOnlyList<SphereEvolutionOption> GetActiveAbilityEvolutionChoices(
            RunUpgradeKind kind, bool final, string branchId = null)
        {
            if (kind == RunUpgradeKind.Teleport)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("teleport_echo_jump", "Эхо-прыжок", "Прыжок использует ближайшую сферу как точку назначения."),
                    new SphereEvolutionOption("teleport_beacon", "Скачок-маяк", "Прыжок к сфере усиливает следующую атаку."),
                    new SphereEvolutionOption("teleport_phase", "Фазовый прыжок", "После прыжка игрок получает краткое окно неуязвимости.")
                };
                switch (branchId)
                {
                    case "teleport_echo_jump": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("teleport_spatial_network", "Прыжок • Цепь", "Телепорт оставляет реле между исходным и целевым узлом."),
                        new SphereEvolutionOption("teleport_hunter_beacon", "Прыжок • Возврат", "Финальная специализация усиливает прыжок к Снайперской сфере."),
                        new SphereEvolutionOption("teleport_phase_break", "Прыжок • Разрыв", "Траектория прыжка наносит дополнительный урон врагам.")
                    };
                    case "teleport_beacon": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("teleport_spatial_network", "Маяк • Метка", "Приземление связывает исходный и целевой узлы сети."),
                        new SphereEvolutionOption("teleport_hunter_beacon", "Маяк • Волна", "Прыжок к Снайперской сфере усиливает следующую атаку."),
                        new SphereEvolutionOption("teleport_phase_break", "Маяк • Разветвление", "Приземление создаёт дополнительный боевой эффект на траектории.")
                    };
                    default: return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("teleport_spatial_network", "Фаза • Сквозь сеть", "Телепорт соединяет исходный и целевой узлы."),
                        new SphereEvolutionOption("teleport_hunter_beacon", "Фаза • Укрытие", "Прыжок к Снайперской сфере усиливает следующую атаку."),
                        new SphereEvolutionOption("teleport_phase_break", "Фаза • Разрыв", "Сферы между точками прыжка создают дополнительные импульсы.")
                    };
                }
            }
            if (kind == RunUpgradeKind.TimeStop)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("timestop_echo_phase", "Фазовый залп", "При запуске остановки все сферы получают немедленный залп."),
                    new SphereEvolutionOption("timestop_closed_time", "Замкнутое время", "Зона остановки расширяется через сеть."),
                    new SphereEvolutionOption("timestop_time_anchor", "Якорь времени", "Остановка дольше удерживает врагов вокруг ближайшей сферы.")
                };
                switch (branchId)
                {
                    case "timestop_echo_phase": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_outside_time", "Фаза • Залп", "Остановка времени сочетается с дополнительным залпом сфер."),
                        new SphereEvolutionOption("timestop_closed_network", "Фаза • Окно", "Сеть сохраняет контроль между связанными узлами."),
                        new SphereEvolutionOption("timestop_temporal_core", "Фаза • Шрам", "Финальный импульс остановки наносит повышенный урон.")
                    };
                    case "timestop_closed_time": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_outside_time", "Замкнутое время • Кольцо", "Зона остановки распространяется по связанным сферам."),
                        new SphereEvolutionOption("timestop_closed_network", "Замкнутая сеть времени", "Сеть сохраняет эффект остановки между узлами."),
                        new SphereEvolutionOption("timestop_temporal_core", "Замкнутое время • Клетка", "Финальный импульс остановки наносит повышенный урон.")
                    };
                    default: return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_outside_time", "Якорь • Фиксация", "Ближайшая сфера фиксирует цели на время остановки."),
                        new SphereEvolutionOption("timestop_closed_network", "Якорь • Перенос", "Эффект остановки распространяется через сеть."),
                        new SphereEvolutionOption("timestop_temporal_core", "Якорь • Разрыв", "Финальный импульс остановки наносит повышенный урон.")
                    };
                }
            }
            return Array.Empty<SphereEvolutionOption>();
        }


    }
}
