using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped active Ability cards and passive catalog metadata from
    /// new-desing/src/gameData.ts and src/sphereProgression.ts. Routine Level-Up
    /// offers active Abilities and Spheres; the constructed passive pool is not
    /// included in the source choice pools.
    /// </summary>
    public static class LevelUpAbilityRules
    {
        // Source game constructs a passive pool but excludes it from routine Level-Up choices.
        // Keep these IDs as catalog metadata only; the runtime choice generator must not consume them.
        private static readonly RunUpgradeKind[] PassiveCatalogKinds =
        {
            RunUpgradeKind.Damage, RunUpgradeKind.AttackSpeed, RunUpgradeKind.MaxSpheres, RunUpgradeKind.MoveSpeed,
            RunUpgradeKind.Slow, RunUpgradeKind.Vitality, RunUpgradeKind.Dodge,
            RunUpgradeKind.Crit, RunUpgradeKind.Magnet, RunUpgradeKind.Armor,
            RunUpgradeKind.Regen, RunUpgradeKind.Stability, RunUpgradeKind.Radius, RunUpgradeKind.Vampire,
            RunUpgradeKind.SphereBoost, RunUpgradeKind.Cooldown, RunUpgradeKind.Momentum,
            RunUpgradeKind.Resonance, RunUpgradeKind.CritPower, RunUpgradeKind.Range,
            RunUpgradeKind.ProjectileSpeed, RunUpgradeKind.Fortune
        };

        public static IReadOnlyList<RunUpgradeKind> SourcePassiveCatalog => PassiveCatalogKinds;

        public static int GetMaxLevel(RunUpgradeKind kind) =>
            kind == RunUpgradeKind.MaxSpheres ? 3 : 7;

        public static bool IsAbilityChoice(RunUpgradeKind kind) => kind != RunUpgradeKind.Sphere;

        public static bool CanUpgrade(RunUpgradeKind kind, int currentLevel) =>
            IsAbilityChoice(kind) && currentLevel >= 0 && currentLevel < GetMaxLevel(kind);

        private static string GetSourceActiveAbilityName(RunUpgradeKind kind, bool russian)
        {
            switch (kind)
            {
                case RunUpgradeKind.Blast: return russian ? "Эхо-импульс" : "Echo Pulse";
                case RunUpgradeKind.Shield: return russian ? "Сферный барьер" : "Sphere Barrier";
                case RunUpgradeKind.Teleport: return russian ? "Эхо-прыжок" : "Echo Jump";
                case RunUpgradeKind.FireTrail: return russian ? "Перегрев" : "Overheat";
                case RunUpgradeKind.Minion: return russian ? "Эхо-дрон" : "Echo Drone";
                case RunUpgradeKind.Lightning: return russian ? "Цепной разряд" : "Chain Lightning";
                case RunUpgradeKind.TimeStop: return russian ? "Эхо-заморозка" : "Echo Freeze";
                case RunUpgradeKind.DarkRitual: return russian ? "Перегрузка" : "Overload";
                default: return string.Empty;
            }
        }

        private static string GetSourceActiveAbilityDescription(RunUpgradeKind kind, int currentLevel, bool russian)
        {
            string[] ru;
            string[] en;
            switch (kind)
            {
                case RunUpgradeKind.Blast:
                    ru = new[] { "Импульс проходит через все активные сферы и наносит 30 урона вокруг каждой. Перезарядка: 30 с.",
                        "Импульс наносит 40 урона вокруг каждой сферы. Перезарядка: 28 с.",
                        "Импульс наносит 50 урона вокруг каждой сферы. Перезарядка: 26 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Импульс начинает усиливать выбранную ветку сети.",
                        "Передача импульса между сферами становится стабильнее и мощнее.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "Pulse travels through all active Spheres and deals 30 damage around each. Cooldown: 30s.",
                        "Pulse deals 40 damage around each Sphere. Cooldown: 28s.",
                        "Pulse deals 50 damage around each Sphere. Cooldown: 26s.",
                        "The next choice opens one of three ability branches",
                        "Pulse begins empowering the selected network branch.",
                        "Pulse transfer between Spheres becomes more stable and powerful.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.Shield:
                    ru = new[] { "Сферы создают защитный контур вокруг игрока и поглощают 1 удар. Длительность: 10 с. Перезарядка: 20 с.",
                        "Ближайшие сферы добавляют защитные заряды. Перезарядка: 20 с.",
                        "Сеть создаёт до 2 защитных зарядов. Длительность: 10 с. Перезарядка: 20 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Защита распространяется на ближайшие сферы и выбранную ветку.",
                        "Защитный контур получает дополнительные заряды и усиливает сеть.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "Spheres create a defensive circuit around the player and absorb 1 hit. Duration: 10s. Cooldown: 20s.",
                        "Nearby Spheres add shield charges. Cooldown: 20s.",
                        "The network creates up to 2 shield charges. Duration: 10s. Cooldown: 20s.",
                        "The next choice opens one of three ability branches",
                        "Defense spreads to nearby Spheres and the selected branch.",
                        "The defensive circuit gains additional charges and strengthens the network.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.Teleport:
                    ru = new[] { "Телепорт перемещает игрока к ближайшей активной сфере. Перезарядка: 15 с.",
                        "Прыжок к сфере происходит чаще. Перезарядка: 13 с.",
                        "Прыжок работает на более дальнюю сферу сети. Перезарядка: 11 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Телепорт начинает использовать сферу как точку назначения выбранной ветки.",
                        "Переход между узлами создаёт дополнительный сетевой эффект.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "Teleport moves the player to the nearest active Sphere. Cooldown: 15s.",
                        "Jumping to a Sphere happens more often. Cooldown: 13s.",
                        "The jump can target a farther Sphere in the network. Cooldown: 11s.",
                        "The next choice opens one of three ability branches",
                        "Teleport begins using the Sphere as the selected branch's destination point.",
                        "Moving between nodes creates an additional network effect.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.FireTrail:
                    ru = new[] { "Перегружает сферы с огненным эффектом на 5 с: +20% урона. Перезарядка: 25 с.",
                        "Перегрев даёт +25% урона и ускоряет атаки огненных сфер. Перезарядка: 25 с.",
                        "Перегрев даёт +30% урона и ещё сильнее ускоряет огненные сферы. Перезарядка: 25 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Перегрев начинает взаимодействовать со статусами и выбранной веткой.",
                        "Перегрев передаётся через сеть и усиливает реакции.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "Overheats fire-aligned Spheres for 5s: +20% damage. Cooldown: 25s.",
                        "Overheat grants +25% damage and speeds up fire-aligned attacks. Cooldown: 25s.",
                        "Overheat grants +30% damage and speeds up fire-aligned Spheres even more. Cooldown: 25s.",
                        "The next choice opens one of three ability branches",
                        "Overheat begins interacting with statuses and the selected branch.",
                        "Overheat is transmitted through the network and strengthens reactions.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.Minion:
                    ru = new[] { "Создаёт 1 Echo Drone на 10 с, который подключается к ближайшей сфере. Перезарядка: 30 с.",
                        "Дрон связывает соседние сферы и наносит 8 урона врагам. Перезарядка: 30 с.",
                        "Создаёт 2 Echo Drone на 10 с. Дроны становятся дополнительными узлами сети. Перезарядка: 30 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Дроны начинают передавать импульсы между ближайшими сферами.",
                        "Дроны чаще соединяют узлы и ускоряют их следующий выстрел.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "Creates 1 Echo Drone for 10s that connects to the nearest Sphere. Cooldown: 30s.",
                        "The drone links neighboring Spheres and deals 8 damage to enemies. Cooldown: 30s.",
                        "Creates 2 Echo Drones for 10s. Drones become additional network nodes. Cooldown: 30s.",
                        "The next choice opens one of three ability branches",
                        "Drones begin transferring pulses between nearby Spheres.",
                        "Drones connect nodes more often and accelerate their next shot.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.Lightning:
                    ru = new[] { "Разряд проходит от игрока через Chain-сферы к 1 цели и наносит 55 урона. Перезарядка: 20 с.",
                        "Разряд наносит 70 урона и усиливается на каждом узле Chain. Перезарядка: 20 с.",
                        "Разряд поражает до 2 целей, проходя через сеть Chain. Урон: 85 каждой. Перезарядка: 20 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Каждая Chain-сфера становится проводником разряда.",
                        "Переходы между узлами усиливают следующий разряд.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "The discharge travels from the player through Chain Spheres to 1 target and deals 55 damage. Cooldown: 20s.",
                        "The discharge deals 70 damage and grows stronger at each Chain node. Cooldown: 20s.",
                        "The discharge hits up to 2 targets through the Chain network. Damage: 85 each. Cooldown: 20s.",
                        "The next choice opens one of three ability branches",
                        "Each Chain Sphere becomes a conductor for the discharge.",
                        "Transitions between nodes strengthen the next discharge.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.TimeStop:
                    ru = new[] { "Сеть останавливает врагов вокруг ближайшей сферы на 3 с. Перезарядка: 40 с.",
                        "Сеть удерживает остановку 4 с. Перезарядка: 40 с.",
                        "Сеть удерживает остановку 5 с и продолжает атаковать через сферы. Перезарядка: 40 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Во время остановки сферы продолжают атаковать.",
                        "Остановка распространяется между узлами сети и усиливает контроль.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "The network stops enemies around the nearest Sphere for 3s. Cooldown: 40s.",
                        "The network holds the stop for 4s. Cooldown: 40s.",
                        "The network holds the stop for 5s and continues attacking through Spheres. Cooldown: 40s.",
                        "The next choice opens one of three ability branches",
                        "Spheres continue attacking during the stop.",
                        "The stop spreads between network nodes and strengthens control.",
                        "The next choice opens one of three final forms" };
                    break;
                case RunUpgradeKind.DarkRitual:
                    ru = new[] { "Жертвуете 20% максимального HP и перегружаете всю сеть на 5 с. Сферы получают +29% урона. Перезарядка: 30 с.",
                        "Жертвуете 20% максимального HP. Перегрузка даёт +33% урона и ускоряет сферы. Перезарядка: 30 с.",
                        "Жертвуете 20% максимального HP. Перегрузка даёт +37% урона и ускоряет сеть сильнее. Перезарядка: 30 с.",
                        "Следующий выбор откроет одну из трёх веток способности",
                        "Потеря HP становится топливом для выбранной ветки и ближайших сфер.",
                        "Перегрузка глубже взаимодействует с сетью и усиливает риск/награду.",
                        "Следующий выбор откроет одну из трёх финальных форм" };
                    en = new[] { "You sacrifice 20% max HP and overload the entire network for 5s. Spheres gain +29% damage. Cooldown: 30s.",
                        "You sacrifice 20% max HP. Overload grants +33% damage and speeds up Spheres. Cooldown: 30s.",
                        "You sacrifice 20% max HP. Overload grants +37% damage and speeds up the network further. Cooldown: 30s.",
                        "The next choice opens one of three ability branches",
                        "Lost HP becomes fuel for the selected branch and nearby Spheres.",
                        "Overload interacts more deeply with the network and strengthens risk/reward.",
                        "The next choice opens one of three final forms" };
                    break;
                default:
                    return string.Empty;
            }
            var index = Math.Max(0, Math.Min(6, currentLevel));
            return russian ? ru[index] : en[index];
        }

        public static RunUpgradeChoice CreateChoice(RunUpgradeKind kind, int currentLevel, bool russian = false)
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
                case RunUpgradeKind.Cooldown:
                    title = "CHRONO CORE / " + next;
                    description = "-" + (next * 4) + "% active ability cooldown.";
                    break;
                case RunUpgradeKind.Momentum:
                    title = "MOMENTUM / " + next;
                    description = "+" + (next * 5) + "% movement speed briefly after Dash.";
                    break;
                case RunUpgradeKind.Resonance:
                    title = "RESONATOR / " + next;
                    description = "+" + (next * 8) + "% Resonance gain.";
                    break;
                case RunUpgradeKind.CritPower:
                    title = "CRITICAL RESONANCE / " + next;
                    description = "+" + (next * 8) + "% critical damage multiplier.";
                    break;
                case RunUpgradeKind.Range:
                    title = "FIELD FOCUS / " + next;
                    description = "+" + (next * 5) + "% Sphere attack range.";
                    break;
                case RunUpgradeKind.ProjectileSpeed:
                    title = "VECTOR DRIVE / " + next;
                    description = "+" + (next * 8) + "% projectile speed.";
                    break;
                case RunUpgradeKind.Fortune:
                    title = "FORTUNE / " + next;
                    description = "+" + (next * 2) + "% rare drop chance.";
                    break;
                case RunUpgradeKind.Blast:
                case RunUpgradeKind.Shield:
                case RunUpgradeKind.Teleport:
                case RunUpgradeKind.FireTrail:
                case RunUpgradeKind.Minion:
                case RunUpgradeKind.Lightning:
                case RunUpgradeKind.TimeStop:
                case RunUpgradeKind.DarkRitual:
                    title = GetSourceActiveAbilityName(kind, russian);
                    description = GetSourceActiveAbilityDescription(kind, current, russian);
                    break;







                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a source-mapped routine Ability.");
            }
            return new RunUpgradeChoice(kind, SphereId.Standard, current, next, title, description);
        }

        public static float GetSphereRadiusMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.15f;

        public static float GetSphereRangeMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.05f;

        public static float GetProjectileSpeedMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static float GetCritPowerMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static float GetResonanceGainMultiplier(int level) =>
            1f + Math.Max(0, Math.Min(7, level)) * 0.08f;

        public static int GetSphereBoostKillsRequired(int level) =>
            Math.Max(50, 100 - (Math.Max(1, Math.Min(7, level)) - 1) * 10);

        public static int GetSphereBoostDamageBonus(int totalKills, int level) =>
            level <= 0 ? 0 : Math.Max(0, totalKills) / GetSphereBoostKillsRequired(level);

        public static float GetMomentumSpeedMultiplier(int level, bool dashing) =>
            dashing ? 1f + Math.Max(0, Math.Min(7, level)) * 0.05f : 1f;

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
                        new SphereEvolutionOption("teleport_echo_jump_f1", "Прыжок • Дальняя дуга", "Телепорт предпочитает ближайшую доступную дальнюю сферу за пределами ближней зоны."),
                        new SphereEvolutionOption("teleport_echo_jump_f2", "Прыжок • Цепь", "Телепорт оставляет реле между исходной и целевой сферами, временно связывая их."),
                        new SphereEvolutionOption("teleport_echo_jump_f3", "Прыжок • Возврат", "После прыжка следующий телепорт в течение короткого окна может вернуть игрока к исходному узлу.")
                    };
                    case "teleport_beacon": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("teleport_beacon_f1", "Маяк • Метка", "Приземление на сферу помечает ближайшего врага и усиливает следующий удар по нему."),
                        new SphereEvolutionOption("teleport_beacon_f2", "Маяк • Волна", "Приземление выпускает импульс вокруг целевой сферы и временно замедляет врагов."),
                        new SphereEvolutionOption("teleport_beacon_f3", "Маяк • Разветвление", "Приземление у сферы создаёт второй прыжковый ориентир и расширяет выбор следующего узла.")
                    };
                    default: return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("teleport_phase_f1", "Фаза • Сквозь сеть", "Во время прыжка игрок игнорирует столкновения с врагами и получает короткое окно после приземления."),
                        new SphereEvolutionOption("teleport_phase_f2", "Фаза • Разрыв", "Траектория прыжка режет врагов между исходным и целевым узлами."),
                        new SphereEvolutionOption("teleport_phase_f3", "Фаза • Укрытие", "После приземления ближайшая сфера создаёт краткую область, в которой игрок неуязвим.")
                    };
                }
            }
            if (kind == RunUpgradeKind.TimeStop)
            {
                if (!final) return new SphereEvolutionOption[]
                {
                    new SphereEvolutionOption("timestop_echo_phase", "Фазовый залп", "При запуске остановки все Сферы получают немедленный залп; во время остановки их урон увеличен на 18%."),
                    new SphereEvolutionOption("timestop_closed_time", "Замкнутое время", "Зона остановки расширяется через сеть."),
                    new SphereEvolutionOption("timestop_time_anchor", "Якорь времени", "Остановка фиксирует врагов вокруг ближайшей сферы.")
                };
                switch (branchId)
                {
                    case "timestop_echo_phase": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_echo_phase_f1", "Фаза • Залп", "При запуске остановки все сферы дают немедленный залп, а первый удар после остановки создаёт дополнительный импульс."),
                        new SphereEvolutionOption("timestop_echo_phase_f2", "Фаза • Окно", "Во время остановки сферы атакуют заметно чаще, но эффект сразу прекращается после возобновления времени."),
                        new SphereEvolutionOption("timestop_echo_phase_f3", "Фаза • Шрам", "Каждая поражённая во время остановки цель получает метку, усиливающую следующий удар по ней.")
                    };
                    case "timestop_closed_time": return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_closed_time_f1", "Замкнутое время • Кольцо", "Зона остановки распространяется по активному кольцу связанных сфер."),
                        new SphereEvolutionOption("timestop_closed_time_f2", "Замкнутое время • Коридор", "Между связанными узлами возникает коридор остановленного времени, замедляющий входящих врагов."),
                        new SphereEvolutionOption("timestop_closed_time_f3", "Замкнутое время • Клетка", "Связанные сферы замыкают вокруг группы врагов временную клетку, ограничивая выход.")
                    };
                    default: return new SphereEvolutionOption[]
                    {
                        new SphereEvolutionOption("timestop_time_anchor_f1", "Якорь • Фиксация", "Ближайшая сфера фиксирует несколько целей вокруг себя на всё время остановки."),
                        new SphereEvolutionOption("timestop_time_anchor_f2", "Якорь • Перенос", "После снятия остановки зафиксированные цели передают остаток эффекта ближайшим врагам."),
                        new SphereEvolutionOption("timestop_time_anchor_f3", "Якорь • Разрыв", "При снятии якоря зафиксированные цели получают короткий импульс и замедление.")
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
