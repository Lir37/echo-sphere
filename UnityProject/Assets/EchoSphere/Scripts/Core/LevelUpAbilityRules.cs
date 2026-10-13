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
                case RunUpgradeKind.Blast:
                    title = "ECHO PULSE / " + next;
                    description = "Pulse damage scales with level; source cooldown starts at 30s and falls by 2s per level.";
                    break;
                case RunUpgradeKind.Shield:
                    title = "ECHO GUARD / " + next;
                    description = "Grants shield charges and a 10s defensive window; source cooldown 20s.";
                    break;
                case RunUpgradeKind.Teleport:
                    title = "ECHO JUMP / " + next;
                    description = "Teleport to a nearby Sphere; cooldown " + Math.Max(5, 15 - Math.Min(4, (next - 1) * 2)) + "s.";
                    break;
                case RunUpgradeKind.FireTrail:
                    title = "OVERHEAT / " + next;
                    description = "Ignites the player's trail for " + (5 + Math.Min(3, next - 1)) + "s; cooldown 25s.";
                    break;
                case RunUpgradeKind.Minion:
                    title = "ECHO DRONE / " + next;
                    description = "Deploys " + (1 + (next - 1) / 2) + " temporary combat drone(s); cooldown 30s.";
                    break;
                case RunUpgradeKind.Lightning:
                    title = "CHAIN LIGHTNING / " + next;
                    description = "Lightning chains across Spheres and damages nearby enemies; cooldown 20s.";
                    break;
                case RunUpgradeKind.TimeStop:
                    title = "ECHO FREEZE / " + next;
                    description = "Freeze enemies for " + (3 + Math.Min(2, next - 1)) + "s; cooldown 40s.";
                    break;
                case RunUpgradeKind.DarkRitual:
                    title = "DARK RITUAL / " + next;
                    description = "Unleashes a high-cost offensive ritual; cooldown and effects scale with level.";
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
            if (kind == RunUpgradeKind.Blast)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("blast_resonance", "Резонансный импульс", "Standard выпускает дополнительный импульс."),
                    new SphereEvolutionOption("blast_network", "Сетевой импульс", "Импульс последовательно проходит по связанным сферам."),
                    new SphereEvolutionOption("blast_core", "Ядро взрыва", "Усиливает импульс в центре сети.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("blast_resonance_f1", "Резонанс • Цветение", "Standard добавляет вторичный импульс."),
                    new SphereEvolutionOption("blast_network_f2", "Сеть • Разрыв", "Посещённые узлы наносят боковой урон."),
                    new SphereEvolutionOption("blast_core_f3", "Ядро • Перегрузка", "После импульса сферы быстрее атакуют.")
                };
            }
            if (kind == RunUpgradeKind.Shield)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("shield_echo_guard", "Эхо-барьер", "Ближайшие сферы добавляют заряды щита."),
                    new SphereEvolutionOption("shield_reflector", "Отражающий контур", "Защитный заряд отражает часть давления на врагов."),
                    new SphereEvolutionOption("shield_bastion", "Бастион", "Сферы образуют единый защитный контур.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("shield_network_guard", "Network Bastion", "Связанные сферы увеличивают запас щита."),
                    new SphereEvolutionOption("shield_iron_dome", "Iron Dome", "Сферы рядом с игроком добавляют заряды."),
                    new SphereEvolutionOption("shield_resonant_guard", "Resonant Guard", "Защита усиливает боевую сеть.")
                };
            }
            if (kind == RunUpgradeKind.FireTrail)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("firetrail_overdrive", "Перегрев", "Огненные сферы получают дополнительное ускорение атак."),
                    new SphereEvolutionOption("firetrail_ignition", "Воспламенитель", "Перегрев поджигает врагов с активным статусом."),
                    new SphereEvolutionOption("firetrail_sanctum", "Пылающее святилище", "Мгновенно усиливает Aura-сферу.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("firetrail_network_f1", "Thermal Network", "Перегрев передаётся между узлами."),
                    new SphereEvolutionOption("firetrail_catalyst_f2", "Catalyst Flame", "Статусы усиливают следующий огненный импульс."),
                    new SphereEvolutionOption("firetrail_inferno_f3", "Network Inferno", "Каждый перегретый узел усиливает сеть.")
                };
            }
            if (kind == RunUpgradeKind.Minion)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("minion_echo_drone", "Эхо-дрон", "Временный дрон поддерживает ближайшую сферу."),
                    new SphereEvolutionOption("minion_relay_drone", "Релейный дрон", "Дрон передаёт импульс между узлами."),
                    new SphereEvolutionOption("minion_guardian", "Дрон-страж", "Дрон ускоряет следующую атаку сферы.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("minion_echo_drone_f1", "Echo Swarm", "Дроны создают дополнительные импульсы."),
                    new SphereEvolutionOption("minion_relay_drone_f2", "Network Nodes", "Дроны усиливают сетевые переходы."),
                    new SphereEvolutionOption("minion_guardian_f3", "Sphere Guardians", "Дроны усиливают защищаемые сферы.")
                };
            }
            if (kind == RunUpgradeKind.Lightning)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("lightning_echo_storm", "Эхо-шторм", "Разряд проходит через Chain-сферы."),
                    new SphereEvolutionOption("lightning_relay", "Релейный разряд", "Разряд перескакивает между связанными сферами."),
                    new SphereEvolutionOption("lightning_overload", "Перегрузка", "Последний разряд серии наносит дополнительный урон.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("lightning_storm_network_f1", "Storm Network", "Создаёт последовательные разряды между узлами."),
                    new SphereEvolutionOption("lightning_thunder_chain_f2", "Thunder Network", "Каждая Chain-сфера добавляет переход."),
                    new SphereEvolutionOption("lightning_overload_core_f3", "Overload Core", "Сеть завершается мощным разрядом.")
                };
            }
            if (kind == RunUpgradeKind.DarkRitual)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("darkritual_blood_link", "Кровавая связь", "Передаёт часть перегрузки Standard-сфере."),
                    new SphereEvolutionOption("darkritual_sacrifice", "Жертвенный круг", "Потеря HP создаёт дополнительный импульс."),
                    new SphereEvolutionOption("darkritual_void_pact", "Договор пустоты", "Низкий запас HP продлевает перегрузку.")
                };
                return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("darkritual_blood_network_f1", "Blood Network", "Перегрузка передаёт заряд по сети."),
                    new SphereEvolutionOption("darkritual_sacrifice_core_f2", "Sacrifice Core", "Жертва создаёт усиленный импульс."),
                    new SphereEvolutionOption("darkritual_void_engine_f3", "Void Engine", "Низкий HP усиливает перегрузку.")
                };
            }
            return Array.Empty<SphereEvolutionOption>();
        }


    }
}
