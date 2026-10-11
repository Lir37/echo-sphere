using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum ArtifactRarity { Common, Rare, Epic, Special, Legendary }

    public sealed class ArtifactRewardDefinition
    {
        public string Id { get; }
        public string NameEnglish { get; }
        public string NameRussian { get; }
        public string DescriptionEnglish { get; }
        public string DescriptionRussian { get; }
        public ArtifactRarity Rarity { get; }
        public string Mechanic { get; }
        public IReadOnlyDictionary<string, float> Effects { get; }

        public ArtifactRewardDefinition(string id, string nameEnglish, string nameRussian,
            string descriptionEnglish, string descriptionRussian, ArtifactRarity rarity,
            string mechanic, Dictionary<string, float> effects)
        {
            Id=id; NameEnglish=nameEnglish; NameRussian=nameRussian;
            DescriptionEnglish=descriptionEnglish; DescriptionRussian=descriptionRussian;
            Rarity=rarity; Mechanic=mechanic;
            Effects=effects ?? new Dictionary<string,float>(StringComparer.Ordinal);
        }
        public float GetEffect(string key) => Effects.TryGetValue(key, out var value) ? value : 0f;
    }

    /// <summary>Source artifact catalog/rarity/effects from gameData.ts + artifactSystem.ts.</summary>
    public static class ArtifactRewardRules
    {
        public const float StellaLegendaryCutoffSeconds = 1500f;
        public const int BaseStellaChoiceCount = 3;
        private static readonly List<ArtifactRewardDefinition> Items = new List<ArtifactRewardDefinition>
        {
            new ArtifactRewardDefinition("crystal_speed", "Crystal of Speed", "Кристалл скорости", "+15% move speed", "+15% скорости движения", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "moveSpeed", 0.15f } }),
            new ArtifactRewardDefinition("amulet_hp", "Health Amulet", "Амулет здоровья", "+30 max HP", "+30 макс. HP", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "maxHp", 30f } }),
            new ArtifactRewardDefinition("ring_xp", "XP Ring", "Кольцо опыта", "+20% XP gained", "+20% получаемого опыта", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "xpGain", 0.2f } }),
            new ArtifactRewardDefinition("regen_stone", "Regen Stone", "Камень восстановления", "+1 HP/sec", "+1 HP/сек", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "regen", 1f } }),
            new ArtifactRewardDefinition("radius_shard", "Radius Shard", "Осколок радиуса", "+10% sphere radius", "+10% радиуса сфер", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereRadius", 0.1f } }),
            new ArtifactRewardDefinition("vampire_ring", "Vampire Ring", "Кольцо вампира", "+3% lifesteal", "+3% вампиризма", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "vampire", 0.03f } }),
            new ArtifactRewardDefinition("swift_boots", "Swift Boots", "Ботинки скорости", "kills grant +10% speed for 3s", "после убийства +10% скорости на 3с", ArtifactRarity.Common, "swift", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("luck_talisman", "Luck Talisman", "Талисман удачи", "+15% crit chance", "+15% шанс крита", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "critChance", 0.15f } }),
            new ArtifactRewardDefinition("mage_pendant", "Mage Pendant", "Кулон мага", "-10% ability cooldowns", "-10% перезарядки способностей", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "cooldown", -0.1f } }),
            new ArtifactRewardDefinition("long_lens", "Long Lens", "Дальняя линза", "+18% sniper range", "+18% радиуса снайперской сферы", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sniperRadius", 0.18f } }),
            new ArtifactRewardDefinition("stasis_core", "Stasis Core", "Ядро стазиса", "taking damage slows all enemies for 2s", "при получении урона замедляет всех врагов на 2 секунды", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("dragon_heart", "Dragon Heart", "Сердце дракона", "+50 HP, -10% speed", "+50 HP, -10% скорости", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "maxHp", 50f }, { "moveSpeed", -0.1f } }),
            new ArtifactRewardDefinition("mirror", "Mirror", "Зеркало", "20% of incoming damage is reflected", "20% полученного урона отражается ближайшему врагу", ArtifactRarity.Rare, "mirror", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("predator_claw", "Predator Claw", "Коготь хищника", "+8% sphere damage", "+8% урона сфер", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f } }),
            new ArtifactRewardDefinition("foresight_eye", "Foresight Eye", "Око предвидения", "+10% crit and +8% sniper damage", "+10% крита и +8% урона снайперской сферы", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "critChance", 0.1f }, { "sniperDamage", 0.08f } }),
            new ArtifactRewardDefinition("echo_conductor", "Echo Conductor", "Проводник Эха", "+12% chain sphere damage", "+12% урона цепной сферы", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "chainDamage", 0.12f } }),
            new ArtifactRewardDefinition("heavy_core", "Heavy Core", "Тяжёлое ядро", "+15% standard sphere damage", "+15% урона стандартной сферы", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "standardDamage", 0.15f } }),
            new ArtifactRewardDefinition("scattering_matrix", "Scattering Matrix", "Матрица рассеивания", "+12% shotgun damage", "+12% урона дробовика", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "shotgunDamage", 0.12f } }),
            new ArtifactRewardDefinition("aura_lens", "Aura Lens", "Линза ауры", "+18% aura radius", "+18% радиуса ауры", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "auraRadius", 0.18f } }),
            new ArtifactRewardDefinition("network_relay", "Network Relay", "Сетевой реле", "with 2+ sphere types, +5% network damage", "при 2+ типах сфер +5% урона сети", ArtifactRarity.Rare, "network", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("chaos_orb", "Stability Matrix", "Матрица стабильности", "every 10s grants a random damage or radius buff for 3s", "каждые 10 секунд даёт случайный бонус урона или радиуса на 3 секунды", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("resonance_core", "Resonance Core", "Сердце резонанса", "nearby spheres deal +12% damage", "сфера рядом с другой сферой получает +12% урона", ArtifactRarity.Epic, "resonance", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.05f } }),
            new ArtifactRewardDefinition("lone_bastion", "Lone Bastion", "Одинокий бастион", "with one sphere type, it gains +30% damage", "если активен только один тип сферы, он получает +30% урона", ArtifactRarity.Epic, "lone", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f } }),
            new ArtifactRewardDefinition("fivefold_resonance", "Fivefold Resonance", "Резонанс пяти", "each unique sphere type grants +4% network damage", "каждый уникальный тип сферы даёт +4% урона сети", ArtifactRarity.Epic, "fivefold", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.02f } }),
            new ArtifactRewardDefinition("relay_matrix", "Relay Matrix", "Релейная матрица", "a level VII sphere empowers the network by +10%", "наличие сферы VII уровня усиливает остальные на +10%", ArtifactRarity.Epic, "relay", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("triangle_circuit", "Triangle Circuit", "Треугольный контур", "spheres in a triangular network deal +15% damage", "сферы внутри треугольной сети получают +15% урона", ArtifactRarity.Epic, "triangle", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("overclock", "Overclock", "Разгон ядра", "-12% sphere delay with a small efficiency cost", "-12% задержки сфер, но небольшая потеря эффективности", ArtifactRarity.Epic, "overclock", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.03f }, { "sphereDelay", -0.12f } }),
            new ArtifactRewardDefinition("soul_engine", "Soul Engine", "Двигатель душ", "+5% sphere damage and +10% XP", "+5% урона сфер и +10% опыта", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.05f }, { "xpGain", 0.1f } }),
            new ArtifactRewardDefinition("time_anchor", "Time Anchor", "Якорь времени", "-12% ability cooldowns", "-12% перезарядки способностей", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "cooldown", -0.12f } }),
            new ArtifactRewardDefinition("void_contract", "Void Contract", "Контракт пустоты", "+22% Sphere damage, but +15% damage taken", "+22% урона сфер, но +15% получаемого урона", ArtifactRarity.Special, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.22f }, { "damageTakenReduction", -0.15f } }),
            new ArtifactRewardDefinition("mirror_network", "Mirror Network", "Зеркальная сеть", "two or more sphere types grant +12% network damage", "две и более сферы дают сети +12% урона", ArtifactRarity.Special, "network", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f } }),
            new ArtifactRewardDefinition("singularity_engine", "Singularity Engine", "Двигатель сингулярности", "a 1–2 sphere-type build gains +18% damage", "билд из 1–2 типов сфер получает ещё +18% урона", ArtifactRarity.Special, "singularity", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.1f } }),
            new ArtifactRewardDefinition("quantum_core", "Quantum Core", "Квантовое ядро", "+8% sphere damage and +8% dodge", "+8% урона сфер и +8% уклонения", ArtifactRarity.Special, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "dodgeChance", 0.08f } }),
            new ArtifactRewardDefinition("zero_sphere", "Zero Sphere", "Нулевая сфера", "+35% Sphere damage", "+35% урона сфер", ArtifactRarity.Legendary, "zero", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.35f } }),
            new ArtifactRewardDefinition("unified_mind", "Unified Mind", "Единый разум", "the highest sphere level grants 3% damage per level", "самый высокий уровень сферы передаёт 3% за уровень всей сети", ArtifactRarity.Legendary, "unified", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.16f } }),
            new ArtifactRewardDefinition("network_anchor", "Network Anchor", "Якорь сети", "+4% Sphere damage", "+4% урона сфер", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("pulse_lens", "Pulse Lens", "Линза импульса", "+10% Pulse radius", "+10% к радиусу импульсов", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "pulseRadius", 0.1f } }),
            new ArtifactRewardDefinition("orbit_charm", "Orbit Charm", "Талисман орбиты", "+12% Orbital strength", "+12% к силе орбит", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "orbitalDamage", 0.12f } }),
            new ArtifactRewardDefinition("prism_shard", "Prism Shard", "Осколок призмы", "+10% Prism damage", "+10% к урону Призмы", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "prismDamage", 0.1f } }),
            new ArtifactRewardDefinition("gravity_bead", "Gravity Bead", "Гравитационная бусина", "+12% Gravity pull strength", "+12% к силе гравитации", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("void_ink", "Void Ink", "Чернила пустоты", "+10% Void damage", "+10% к урону Пустоты", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "voidDamage", 0.1f } }),
            new ArtifactRewardDefinition("echo_thread", "Echo Thread", "Нить Эха", "+5% damage from linked Spheres", "+5% к урону связанных Сфер", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("folded_core", "Folded Core", "Складное ядро", "+8% damage for all Spheres", "+8% к урону всех Сфер", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("paper_ward", "Paper Ward", "Бумажный щит", "+4% Sphere damage", "+4% урона сфер", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("mirror_dust", "Mirror Dust", "Зеркальная пыль", "+8% reflection chance", "+8% к шансу отражения", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("resonant_leaf", "Resonant Leaf", "Резонансный лист", "+10% Resonance effects", "+10% к Резонансу", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("signal_knot", "Signal Knot", "Сигнальный узел", "+5% Network recovery speed", "+5% скорости восстановления сети", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("lattice_chip", "Lattice Chip", "Чип решётки", "+10% Lattice strength", "+10% силы Решётки", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("fractal_seed", "Fractal Seed", "Фрактальное семя", "+10% Fractal strength", "+10% силы Фрактала", ArtifactRarity.Common, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.04f } }),
            new ArtifactRewardDefinition("sniper_scope", "Sniper Scope", "Прицел охотника", "+10% Sniper damage", "+10% урона Снайпера", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("chain_battery", "Chain Battery", "Цепной аккумулятор", "+10% Chain damage", "+10% урона Цепи", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("shotgun_shell", "Shotgun Shell", "Дробь усиления", "+10% Shotgun damage", "+10% урона Дробовика", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("aura_mist", "Aura Mist", "Туман ауры", "+10% Aura radius", "+10% радиуса Ауры", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("orbital_blade", "Orbital Blade", "Орбитальный клинок", "+10% Orbital damage", "+10% урона Орбитальной", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "orbitalDamage", 0.1f } }),
            new ArtifactRewardDefinition("prism_filter", "Prism Filter", "Призматический фильтр", "+10% Prism pierce", "+10% пробития Призмы", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("gravity_hook", "Gravity Hook", "Гравитационный крюк", "+10% Gravity control", "+10% контроля Гравитационной", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("pulse_driver", "Pulse Driver", "Импульсный драйвер", "+10% Pulse frequency", "+10% частоты Импульсной", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("void_mark", "Void Mark", "Метка пустоты", "+10% damage to weakened targets", "+10% урона по ослабленным", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "voidWeakened", 0.1f } }),
            new ArtifactRewardDefinition("formation_compass", "Formation Compass", "Компас формации", "increases Sphere radius by 8%", "увеличивает радиус сфер на 8%", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereRadius", 0.08f } }),
            new ArtifactRewardDefinition("geometry_die", "Geometry Die", "Куб геометрии", "increases Sphere damage by 8% during active Geometry", "+8% урона сфер во время активной Геометрии", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("network_coil", "Network Coil", "Сетевая катушка", "+8% damage with 2+ Sphere types", "+8% урона при 2+ типах сфер", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.06f } }),
            new ArtifactRewardDefinition("crit_sigil", "Crit Sigil", "Критическая печать", "+5% critical chance", "+5% шанса критического удара", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "critChance", 0.05f } }),
            new ArtifactRewardDefinition("tempo_ring", "Tempo Ring", "Кольцо темпа", "-8% Sphere attack delay", "-8% задержки сфер", ArtifactRarity.Rare, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDelay", -0.08f } }),
            new ArtifactRewardDefinition("triangle_engine", "Triangle Engine", "Двигатель треугольника", "+12% Triangle strength", "+12% силы Треугольника", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("ring_engine", "Ring Engine", "Двигатель кольца", "+12% Ring strength", "+12% силы Кольца", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("lattice_engine", "Lattice Engine", "Двигатель решётки", "+12% Lattice strength", "+12% силы Решётки", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("fractal_engine", "Fractal Engine", "Двигатель фрактала", "+12% Fractal strength", "+12% силы Фрактала", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("resonance_lattice", "Resonance Lattice", "Резонансная решётка", "+15% Resonance effects", "+15% эффектов Резонанса", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("echo_weaver", "Echo Weaver", "Ткач Эха", "Each active Network link grants a small bonus", "каждая связь даёт небольшой бонус", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("gravity_crown", "Gravity Crown", "Корона гравитации", "Gravity gains +20% radius", "Гравитационная получает +20% радиуса", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "gravityRadius", 0.2f } }),
            new ArtifactRewardDefinition("void_lantern", "Void Lantern", "Фонарь пустоты", "Void gains +20% execution damage", "Пустотная получает +20% урона добивания", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("prism_crown", "Prism Crown", "Корона призмы", "Prism gains +1 reflection", "Призма получает +1 отражение", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("orbital_crown", "Orbital Crown", "Корона орбиты", "Orbital gains +1 satellite", "Орбитальная получает +1 спутник", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("pulse_crown", "Pulse Crown", "Корона импульса", "Pulse gains +1 wave", "Импульсная получает +1 волну", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal)),
            new ArtifactRewardDefinition("chain_crown", "Chain Crown", "Корона цепи", "Chain gains +1 jump", "Цепная получает +1 перескок", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("sniper_crown", "Sniper Crown", "Корона снайпера", "Sniper gains +10% range", "Снайперская получает +10% дальности", ArtifactRarity.Epic, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.08f }, { "cooldown", -0.03f } }),
            new ArtifactRewardDefinition("time_splitter", "Time Splitter", "Разделитель времени", "+15% damage after Time Stop", "после временной остановки +15% урона", ArtifactRarity.Legendary, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.12f }, { "critChance", 0.03f } }),
            new ArtifactRewardDefinition("stasis_mandala", "stasis mandala", "Мандала стазиса", "Part of damage taken becomes a shield charge", "полученный урон частично превращается в щит", ArtifactRarity.Legendary, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.12f }, { "critChance", 0.03f } }),
            new ArtifactRewardDefinition("quantum_fold", "quantum fold", "Квантовый сгиб", "Stella rewards gain one additional choice", "Стелла появляется с дополнительным выбором", ArtifactRarity.Special, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.12f }, { "critChance", 0.03f } }),
            new ArtifactRewardDefinition("void_star", "void star", "Звезда пустоты", "Void gains an additional execution chance", "Пустота получает дополнительный шанс добивания", ArtifactRarity.Special, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "critChance", 0.03f } }),
            new ArtifactRewardDefinition("axiom_core", "axiom core", "Аксиома ядра", "+5% to key combat effects", "+5% ко всем ключевым эффектам", ArtifactRarity.Legendary, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.15f }, { "cooldown", -0.08f } }),
            new ArtifactRewardDefinition("universal_fold", "universal fold", "Универсальный сгиб", "All assembled systems gain a small synergy bonus", "все собранные системы получают небольшой синергетический бонус", ArtifactRarity.Legendary, "", new Dictionary<string, float>(StringComparer.Ordinal) { { "sphereDamage", 0.15f }, { "cooldown", -0.08f } }),
        };
        private static readonly Dictionary<string, ArtifactRewardDefinition> Index = BuildIndex();
        private static Dictionary<string, ArtifactRewardDefinition> BuildIndex()
        {
            var result = new Dictionary<string, ArtifactRewardDefinition>(StringComparer.Ordinal);
            for(var i=0;i<Items.Count;i++) result[Items[i].Id]=Items[i];
            return result;
        }

        public static IReadOnlyList<ArtifactRewardDefinition> All => Items;
        public static int LegendaryCount
        {
            get { var count=0; for(var i=0;i<Items.Count;i++) if(Items[i].Rarity==ArtifactRarity.Legendary) count++; return count; }
        }
        public static bool IsLegendaryStellaWindow(float runTime) => runTime < StellaLegendaryCutoffSeconds;
        public static int GetStellaChoiceCount(IList<string> owned) => Contains(owned,"quantum_fold") ? 4 : BaseStellaChoiceCount;
        public static ArtifactRewardDefinition Get(string id) =>
            id!=null && Index.TryGetValue(id,out var item) ? item : null;

        public static float GetEffectTotal(IList<string> owned,string key)
        {
            var sum=0f;
            if(owned==null)return sum;
            for(var i=0;i<owned.Count;i++){var item=Get(owned[i]);if(item!=null)sum+=item.GetEffect(key);}
            return sum;
        }

        public static List<ArtifactRewardDefinition> PickChoices(IList<string> owned,Func<float> random,bool legendaryOnly,int count)
        {
            var pool=new List<ArtifactRewardDefinition>();
            for(var i=0;i<Items.Count;i++)
            {
                var item=Items[i];
                if(Contains(owned,item.Id))continue;
                if(legendaryOnly ? item.Rarity!=ArtifactRarity.Legendary : item.Rarity==ArtifactRarity.Legendary)continue;
                pool.Add(item);
            }
            var result=new List<ArtifactRewardDefinition>();
            if(random==null)random=()=>0f;
            while(result.Count<Math.Max(0,count)&&pool.Count>0)
            {
                var total=0f;
                for(var i=0;i<pool.Count;i++)total+=legendaryOnly?1f:RarityWeight(pool[i].Rarity);
                if(total<=0f)break;
                var roll=ClampRandom(random())*total;
                var chosen=pool[pool.Count-1];
                for(var i=0;i<pool.Count;i++)
                {
                    roll-=legendaryOnly?1f:RarityWeight(pool[i].Rarity);
                    if(roll<=0f){chosen=pool[i];break;}
                }
                result.Add(chosen);
                pool.Remove(chosen);
            }
            return result;
        }

        private static float RarityWeight(ArtifactRarity rarity)
        {
            switch(rarity)
            {
                case ArtifactRarity.Common:return 62f;
                case ArtifactRarity.Rare:return 24f;
                case ArtifactRarity.Epic:return 9f;
                case ArtifactRarity.Special:return 4f;
                default:return 1f;
            }
        }
        private static bool Contains(IList<string> values,string id)
        {
            if(values==null)return false;
            for(var i=0;i<values.Count;i++)if(string.Equals(values[i],id,StringComparison.Ordinal))return true;
            return false;
        }
        private static float ClampRandom(float value) =>
            float.IsNaN(value)||float.IsInfinity(value)?0f:(float)Math.Max(0d,Math.Min(0.99999994d,(double)value));
    }
}
