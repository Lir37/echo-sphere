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
            RunUpgradeKind.Regen, RunUpgradeKind.Stability, RunUpgradeKind.Radius, RunUpgradeKind.Vampire,
            RunUpgradeKind.SphereBoost, RunUpgradeKind.Resonance, RunUpgradeKind.Cooldown, RunUpgradeKind.CritPower,
            RunUpgradeKind.Range, RunUpgradeKind.ProjectileSpeed, RunUpgradeKind.Momentum, RunUpgradeKind.Fortune
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
                case RunUpgradeKind.Radius:
                    title = "SPHERE RADIUS / " + next;
                    description = "+" + (next * 15) + "% Sphere attack radius.";
                    break;
                case RunUpgradeKind.Vampire:
                    title = "VAMPIRISM / " + next;
                    description = "+" + (next * 3) + "% of Sphere damage restored as Core HP.";
                    break;
                case RunUpgradeKind.SphereBoost:
                    title = "SPHERE BOOST / " + next;
                    description = "+1 flat Sphere damage per " + GetSphereBoostKillsRequired(next) + " kills.";
                    break;
                case RunUpgradeKind.Resonance:
                    title = "RESONATOR / " + next;
                    description = "+" + (next * 8) + "% Resonance gain.";
                    break;
                case RunUpgradeKind.Cooldown:
                    title = "CHRONO CORE / " + next;
                    description = "-" + (next * 4) + "% active ability cooldown.";
                    break;
                case RunUpgradeKind.CritPower:
                    title = "CRITICAL RESONANCE / " + next;
                    description = "+" + (next * 8) + "% critical damage multiplier.";
                    break;
                case RunUpgradeKind.Range:
                    title = "FIELD FOCUS / " + next;
                    description = "+" + (next * 5) + "% Sphere range.";
                    break;
                case RunUpgradeKind.ProjectileSpeed:
                    title = "VECTOR DRIVE / " + next;
                    description = "+" + (next * 8) + "% projectile speed.";
                    break;
                case RunUpgradeKind.Momentum:
                    title = "MOMENTUM / " + next;
                    description = "+" + (next * 5) + "% movement speed briefly after Dash.";
                    break;
                case RunUpgradeKind.Fortune:
                    title = "FORTUNE / " + next;
                    description = "+" + (next * 2) + "% rare drop chance.";
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

        public static float GetSphereRadiusMultiplier(int radiusLevel, int rangeLevel) =>
            (1f + Math.Max(0, Math.Min(7, radiusLevel)) * 0.15f) *
            (1f + Math.Max(0, Math.Min(7, rangeLevel)) * 0.05f);

        public static float GetProjectileSpeedMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static float GetCritPowerMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static float GetResonanceGainMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static int GetSphereBoostKillsRequired(int level) =>
            Math.Max(50, 100 - Math.Max(0, Math.Min(7, level) - 1) * 10);

        public static float GetMomentumSpeedMultiplier(int level, bool active) =>
            active ? 1f + Math.Max(0, Math.Min(7, level)) * 0.05f : 1f;

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
                switch (branchId)
                {
                    case "blast_resonance": return new SphereEvolutionOption[] { new SphereEvolutionOption("blast_resonance_f1", "Резонанс • Цветение", "Standard добавляет вторичный импульс."), new SphereEvolutionOption("blast_resonance_f2", "Резонанс • Волна", "Связанные сферы выпускают дополнительный всплеск."), new SphereEvolutionOption("blast_resonance_f3", "Резонанс • Возврат", "Импульс возвращается по сети.") };
                    case "blast_network": return new SphereEvolutionOption[] { new SphereEvolutionOption("blast_network_f1", "Сеть • Эстафета", "Волна последовательно проходит по связанным узлам."), new SphereEvolutionOption("blast_network_f2", "Сеть • Разрыв", "Каждый второй узел создаёт боковой взрыв."), new SphereEvolutionOption("blast_network_f3", "Сеть • Обратный контур", "Волна возвращается к началу сети.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("blast_core_f1", "Ядро • Детонация", "Центр сети создаёт отдельную детонацию."), new SphereEvolutionOption("blast_core_f2", "Ядро • Сжатие", "Враги стягиваются к центру перед взрывом."), new SphereEvolutionOption("blast_core_f3", "Ядро • Перегрузка", "Ближайшие сферы быстрее атакуют после импульса.") };
                }
            }
            if (kind == RunUpgradeKind.Shield)
            {
                if (!final) return new SphereEvolutionOption[] { new SphereEvolutionOption("shield_echo_guard", "Эхо-барьер", "Ближайшие сферы добавляют заряды щита."), new SphereEvolutionOption("shield_reflector", "Отражающий контур", "Защитный заряд отражает давление на врагов."), new SphereEvolutionOption("shield_bastion", "Бастион", "Сферы образуют единый защитный контур.") };
                switch (branchId)
                {
                    case "shield_echo_guard": return new SphereEvolutionOption[] { new SphereEvolutionOption("shield_echo_guard_f1", "Барьер • Резерв", "Пары сфер формируют резервный заряд."), new SphereEvolutionOption("shield_echo_guard_f2", "Барьер • Реле", "Щит ускоряет ближайшие сферы."), new SphereEvolutionOption("shield_echo_guard_f3", "Барьер • Обратный контур", "Последний заряд создаёт короткую защиту.") };
                    case "shield_reflector": return new SphereEvolutionOption[] { new SphereEvolutionOption("shield_reflector_f1", "Отражатель • Контрудар", "Поглощённый удар поражает ближайшую цель."), new SphereEvolutionOption("shield_reflector_f2", "Отражатель • Рикошет", "Отражение перескакивает на вторую цель."), new SphereEvolutionOption("shield_reflector_f3", "Отражатель • Разряд сети", "Отражение проходит через связанную сферу.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("shield_bastion_f1", "Бастион • Стена", "Сферы снижают входящий урон."), new SphereEvolutionOption("shield_bastion_f2", "Бастион • Контур", "Защитный импульс отталкивает врагов."), new SphereEvolutionOption("shield_bastion_f3", "Бастион • Периметр", "Каждая сфера создаёт зону перехвата.") };
                }
            }
            if (kind == RunUpgradeKind.FireTrail)
            {
                if (!final) return new SphereEvolutionOption[] { new SphereEvolutionOption("firetrail_overdrive", "Перегрев", "Огненные сферы получают дополнительное ускорение атак."), new SphereEvolutionOption("firetrail_ignition", "Воспламенитель", "Перегрев поджигает врагов с активным статусом."), new SphereEvolutionOption("firetrail_sanctum", "Пылающее святилище", "Мгновенно усиливает Aura-сферу.") };
                switch (branchId)
                {
                    case "firetrail_overdrive": return new SphereEvolutionOption[] { new SphereEvolutionOption("firetrail_overdrive_f1", "Перегрев • Передача", "Ускорение передаётся через сеть."), new SphereEvolutionOption("firetrail_overdrive_f2", "Перегрев • Катализатор", "Статусы усиливают следующий огненный импульс."), new SphereEvolutionOption("firetrail_overdrive_f3", "Перегрев • Инферно", "Каждый перегретый узел усиливает сеть.") };
                    case "firetrail_ignition": return new SphereEvolutionOption[] { new SphereEvolutionOption("firetrail_ignition_f1", "Воспламенитель • Передача", "Огонь переходит к соседним целям."), new SphereEvolutionOption("firetrail_ignition_f2", "Воспламенитель • Катализатор", "Статусная реакция усиливает следующий импульс."), new SphereEvolutionOption("firetrail_ignition_f3", "Воспламенитель • Инферно", "Поджог распространяется по сети.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("firetrail_sanctum_f1", "Святилище • Передача", "Перегрев распространяется по узлам."), new SphereEvolutionOption("firetrail_sanctum_f2", "Святилище • Катализатор", "Aura усиливает огненные реакции."), new SphereEvolutionOption("firetrail_sanctum_f3", "Святилище • Инферно", "Каждый узел усиливает общий перегрев.") };
                }
            }
            if (kind == RunUpgradeKind.Minion)
            {
                if (!final) return new SphereEvolutionOption[] { new SphereEvolutionOption("minion_echo_drone", "Эхо-дрон", "Временный дрон поддерживает ближайшую сферу."), new SphereEvolutionOption("minion_relay_drone", "Релейный дрон", "Дрон передаёт импульс между узлами."), new SphereEvolutionOption("minion_guardian", "Дрон-страж", "Дрон ускоряет следующую атаку сферы.") };
                switch (branchId)
                {
                    case "minion_echo_drone": return new SphereEvolutionOption[] { new SphereEvolutionOption("minion_echo_drone_f1", "Echo Swarm", "Дроны создают дополнительные импульсы."), new SphereEvolutionOption("minion_echo_drone_f2", "Echo Relay", "Дроны передают импульсы между узлами."), new SphereEvolutionOption("minion_echo_drone_f3", "Echo Guard", "Дроны защищают ближайшие сферы.") };
                    case "minion_relay_drone": return new SphereEvolutionOption[] { new SphereEvolutionOption("minion_relay_drone_f1", "Network Nodes", "Дроны создают дополнительные узлы сети."), new SphereEvolutionOption("minion_relay_drone_f2", "Relay Chain", "Дроны усиливают переходы между сферами."), new SphereEvolutionOption("minion_relay_drone_f3", "Relay Storm", "Каждый дрон ускоряет следующий переход.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("minion_guardian_f1", "Sphere Guardians", "Дроны усиливают защищаемые сферы."), new SphereEvolutionOption("minion_guardian_f2", "Guardian Pulse", "Дроны создают защитный импульс."), new SphereEvolutionOption("minion_guardian_f3", "Guardian Network", "Защита передаётся по сети.") };
                }
            }
            if (kind == RunUpgradeKind.Lightning)
            {
                if (!final) return new SphereEvolutionOption[] { new SphereEvolutionOption("lightning_echo_storm", "Эхо-шторм", "Разряд проходит через Chain-сферы."), new SphereEvolutionOption("lightning_relay", "Релейный разряд", "Разряд перескакивает между связанными сферами."), new SphereEvolutionOption("lightning_overload", "Перегрузка", "Последний разряд серии наносит дополнительный урон.") };
                switch (branchId)
                {
                    case "lightning_echo_storm": return new SphereEvolutionOption[] { new SphereEvolutionOption("lightning_echo_storm_f1", "Эхо-шторм • Сеть", "Разряды проходят через все Chain-узлы."), new SphereEvolutionOption("lightning_echo_storm_f2", "Эхо-шторм • Гроза", "Каждый узел усиливает следующий разряд."), new SphereEvolutionOption("lightning_echo_storm_f3", "Эхо-шторм • Ядро", "Финальный разряд наносит дополнительный урон.") };
                    case "lightning_relay": return new SphereEvolutionOption[] { new SphereEvolutionOption("lightning_relay_f1", "Реле • Сеть", "Разряд проходит по связанным узлам."), new SphereEvolutionOption("lightning_relay_f2", "Реле • Гроза", "Каждый переход добавляет урон."), new SphereEvolutionOption("lightning_relay_f3", "Реле • Ядро", "Сеть завершается мощным разрядом.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("lightning_overload_f1", "Перегрузка • Сеть", "Разряды проходят через все узлы."), new SphereEvolutionOption("lightning_overload_f2", "Перегрузка • Гроза", "Каждый переход добавляет урон."), new SphereEvolutionOption("lightning_overload_f3", "Перегрузка • Ядро", "Сеть завершается мощным разрядом.") };
                }
            }
            if (kind == RunUpgradeKind.DarkRitual)
            {
                if (!final) return new SphereEvolutionOption[] { new SphereEvolutionOption("darkritual_blood_link", "Кровавая связь", "Передаёт часть перегрузки Standard-сфере."), new SphereEvolutionOption("darkritual_sacrifice", "Жертвенный круг", "Потеря HP создаёт дополнительный импульс."), new SphereEvolutionOption("darkritual_void_pact", "Договор пустоты", "Низкий запас HP продлевает перегрузку.") };
                switch (branchId)
                {
                    case "darkritual_blood_link": return new SphereEvolutionOption[] { new SphereEvolutionOption("darkritual_blood_link_f1", "Кровавая связь • Сеть", "Перегрузка передаёт заряд по сети."), new SphereEvolutionOption("darkritual_blood_link_f2", "Кровавая связь • Ядро", "Жертва создаёт усиленный импульс."), new SphereEvolutionOption("darkritual_blood_link_f3", "Кровавая связь • Пустота", "Низкий HP усиливает сеть.") };
                    case "darkritual_sacrifice": return new SphereEvolutionOption[] { new SphereEvolutionOption("darkritual_sacrifice_f1", "Жертвенный круг • Сеть", "Перегрузка передаёт заряд по сети."), new SphereEvolutionOption("darkritual_sacrifice_f2", "Жертвенный круг • Ядро", "Жертва создаёт усиленный импульс."), new SphereEvolutionOption("darkritual_sacrifice_f3", "Жертвенный круг • Пустота", "Низкий HP усиливает сеть.") };
                    default: return new SphereEvolutionOption[] { new SphereEvolutionOption("darkritual_void_pact_f1", "Договор пустоты • Сеть", "Перегрузка передаёт заряд по сети."), new SphereEvolutionOption("darkritual_void_pact_f2", "Договор пустоты • Ядро", "Жертва создаёт усиленный импульс."), new SphereEvolutionOption("darkritual_void_pact_f3", "Договор пустоты • Пустота", "Низкий HP усиливает сеть.") };
                }
            }
            return Array.Empty<SphereEvolutionOption>();
        }


    }
}
