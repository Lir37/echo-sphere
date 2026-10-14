using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public readonly struct CharacterMasteryDefinition
    {
        public readonly int Level;
        public readonly string TitleRu;
        public readonly string TitleEn;
        public readonly string DescriptionRu;
        public readonly string DescriptionEn;

        public CharacterMasteryDefinition(int level, string titleRu, string titleEn, string descriptionRu, string descriptionEn)
        {
            Level = level; TitleRu = titleRu; TitleEn = titleEn; DescriptionRu = descriptionRu; DescriptionEn = descriptionEn;
        }
    }

    /// <summary>Source-mapped character definitions, unlock costs, passive modifiers and mastery thresholds.</summary>
    public sealed class CharacterDefinition
    {
        public readonly SphereCharacterId Id;
        public readonly string NameRu, NameEn, RoleRu, RoleEn;
        public readonly string DescriptionRu, DescriptionEn, ColorHex, MechanicRu, MechanicEn;
        public readonly SphereId SignatureSphere, PartnerSphere;
        public readonly int UnlockCost;
        public readonly float SphereDamage, SphereRadius, SphereAttackSpeed, MoveSpeed, MaxHp, StatusDuration, StatusDamage, DamageTaken;
        public readonly IReadOnlyList<CharacterMasteryDefinition> Mastery;

        public CharacterDefinition(SphereCharacterId id, string nameRu, string nameEn, string roleRu, string roleEn,
            string descriptionRu, string descriptionEn, string colorHex, string mechanicRu, string mechanicEn,
            SphereId signatureSphere, SphereId partnerSphere, int unlockCost, float sphereDamage, float sphereRadius,
            float sphereAttackSpeed, float moveSpeed, float maxHp, float statusDuration, float statusDamage, float damageTaken,
            IReadOnlyList<CharacterMasteryDefinition> mastery)
        {
            Id = id; NameRu = nameRu; NameEn = nameEn; RoleRu = roleRu; RoleEn = roleEn;
            DescriptionRu = descriptionRu; DescriptionEn = descriptionEn; ColorHex = colorHex; MechanicRu = mechanicRu; MechanicEn = mechanicEn;
            SignatureSphere = signatureSphere; PartnerSphere = partnerSphere; UnlockCost = unlockCost;
            SphereDamage = sphereDamage; SphereRadius = sphereRadius; SphereAttackSpeed = sphereAttackSpeed; MoveSpeed = moveSpeed;
            MaxHp = maxHp; StatusDuration = statusDuration; StatusDamage = statusDamage; DamageTaken = damageTaken; Mastery = mastery;
        }
    }

    public static class CharacterRules
    {
        private static readonly int[] MasteryThresholds = { 0, 250, 750, 1500, 2500, 4000, 6000, 8500, 11500, 15000 };
        private static readonly CharacterDefinition[] Definitions =
        {
            new CharacterDefinition(
                SphereCharacterId.Spherist, "Сферист", "Spherist", "Универсал", "All-rounder",
                "Получает дополнительные преимущества от большого количества активных сфер.", "Gets additional benefits from having many active spheres.", "#55dfff",
                "Резонанс: +3% скорости атаки всех сфер за каждую сферу после первой. При 5+ сферах дополнительно +5% урона.", "Resonance: +3% attack speed for all spheres for each sphere after the first. At 5+ spheres, gain an additional +5% damage.",
                SphereId.Standard, SphereId.Orbital, 0,
                0.05f, 0.05f, 0f, 0f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Резонанс", "Resonance", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Гармония", "Harmony", "Порог полного бонуса Резонанса снижается с 5 до 4 сфер.", "Resonance full-bonus threshold is reduced from 5 to 4 spheres."),
                    new CharacterMasteryDefinition(3, "Стабильность", "Stability", "+2% урона сфер.", "+2% sphere damage."),
                    new CharacterMasteryDefinition(4, "Резонанс+", "Resonance+", "Каждая сфера после первой даёт ещё +0.5% скорости атаки.", "Each sphere after the first grants another +0.5% attack speed."),
                    new CharacterMasteryDefinition(5, "Хор сфер", "Sphere Chorus", "При 8 сферах: ещё +5% радиуса и +5% скорости атаки.", "At 8 spheres: gain another +5% radius and +5% attack speed."),
                    new CharacterMasteryDefinition(6, "Созвучие", "Accord", "При 6+ сферах: ещё +2% урона.", "At 6+ spheres: another +2% sphere damage."),
                    new CharacterMasteryDefinition(7, "Резонансный контур", "Resonant Circuit", "При 7+ сферах: ещё +2% скорости атаки.", "At 7+ spheres: another +2% attack speed."),
                    new CharacterMasteryDefinition(8, "Глубокий резонанс", "Deep Resonance", "Полный бонус Резонанса начинается с 3 сфер.", "Full Resonance bonus begins at 3 spheres."),
                    new CharacterMasteryDefinition(9, "Хоровой импульс", "Chorus Pulse", "При 8+ сферах каждый 10-й успешный удар досрочно активирует ещё одну сферу.", "With 8+ spheres, every 10th successful hit activates another Sphere early."),
                    new CharacterMasteryDefinition(10, "Хоровая синхронизация", "Chorus Sync", "При 8+ сферах каждый 8-й успешный удар синхронизирует две сферы.", "With 8+ spheres, every 8th successful hit synchronizes two Spheres.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Hunter, "Охотник", "Hunter", "Одиночная цель", "Single Target",
                "Специалист по элитам и боссам: отмечает приоритетные цели и усиливает повторные попадания.", "Specialist against elites and bosses: marks priority targets and rewards repeated hits.", "#e86cff",
                "Метка добычи: элиты и боссы получают Метку на 5 секунд. Сферы наносят отмеченной цели +20% урона; 5 попаданий подряд запускают Охоту ещё на 3 секунды (+30% урона от Снайперской и Цепной сфер).", "Prey Mark: elites and bosses are marked for 5 seconds. Spheres deal +20% damage to the marked target; 5 consecutive hits trigger Hunt for 3 more seconds (+30% Sniper/Chain damage).",
                SphereId.Sniper, SphereId.Void, 1500,
                0.08f, 0.1f, -0.05f, 0f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Метка добычи", "Prey Mark", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Следопыт", "Tracker", "Длительность метки увеличивается до 6 секунд.", "Mark duration increases to 6 seconds."),
                    new CharacterMasteryDefinition(3, "Точный выстрел", "True Shot", "+2% шанс крита против отмеченных целей.", "+2% crit chance against marked targets."),
                    new CharacterMasteryDefinition(4, "Натиск", "Onslaught", "Для запуска Охоты требуется 4 попадания вместо 5.", "Hunt requires 4 consecutive hits instead of 5."),
                    new CharacterMasteryDefinition(5, "Трофей", "Trophy", "Убийство отмеченной элитной цели временно даёт +10% скорости движения на 4 секунды.", "Killing a marked elite grants +10% move speed for 4 seconds."),
                    new CharacterMasteryDefinition(6, "Слабое место", "Weak Point", "Отмеченные цели получают ещё +5% урона.", "Marked targets take another +5% damage."),
                    new CharacterMasteryDefinition(7, "Дальний след", "Long Trail", "Метка сохраняется дольше после повторного попадания.", "Repeated hits extend the active Mark window."),
                    new CharacterMasteryDefinition(8, "Без пощады", "No Mercy", "Охота дополнительно усиливает Снайперскую и Цепную сферы.", "Hunt further amplifies Sniper/Chain damage."),
                    new CharacterMasteryDefinition(9, "Приоритет", "Priority", "Элитные и босс-цели получают дополнительный шанс критического удара.", "Elite and Boss targets grant an additional critical-hit chance."),
                    new CharacterMasteryDefinition(10, "Мастер охоты", "Master Hunter", "Финальная Сигнатура: Метка добычи и Охота получают максимальное усиление.", "Final Signature: Prey Mark and Hunt reach their maximum enhancement.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Engineer, "Инженер", "Engineer", "Сеть", "Network",
                "Усиливает близко стоящие сферы и вознаграждает компактные боевые конструкции.", "Empowers nearby spheres and rewards compact battlefield structures.", "#57d5ff",
                "Связь: сфера получает +6% урона за каждого соседнего союзника в пределах 220 px, максимум 2 соседа. Сеть из 3+ связанных сфер получает +8% дальности. Попадание одной связанной сферы открывает 0.4-секундное окно ретрансляции для другой.", "Link: a sphere gains +6% damage for each allied sphere within 220 px, up to 2 neighbours. A network of 3+ linked spheres gains +8% range. A hit by one linked sphere opens a 0.4-second relay window for another linked sphere.",
                SphereId.Chain, SphereId.Aura, 2000,
                0f, 0.05f, 0f, -0.05f,
                0.1f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Связь", "Link", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Проводник", "Conductor", "Радиус связи увеличивается с 220 до 240 px.", "Link radius increases from 220 to 240 px."),
                    new CharacterMasteryDefinition(3, "Узел", "Node", "+3% урона сети, если в ней 4+ сферы.", "+3% network damage when the network contains 4+ spheres."),
                    new CharacterMasteryDefinition(4, "Быстрая передача", "Fast Relay", "Окно ретрансляции увеличивается до 0.55 секунды.", "Relay window increases to 0.55 seconds."),
                    new CharacterMasteryDefinition(5, "Главный узел", "Master Node", "Сфера с 2 соседями даёт им ещё +2% урона.", "A sphere with 2 neighbours grants them another +2% damage."),
                    new CharacterMasteryDefinition(6, "Широкая сеть", "Wide Network", "Радиус инженерной связи увеличивается ещё на 20 пикселей.", "Engineer link radius increases by another 20 px."),
                    new CharacterMasteryDefinition(7, "Точка опоры", "Anchor Point", "Сеть из 4+ сфер получает ещё +3% урона.", "A 4+ sphere network gains another +3% damage."),
                    new CharacterMasteryDefinition(8, "Ретранслятор", "Repeater", "Окно ретрансляции становится ещё стабильнее.", "The relay window becomes more consistent."),
                    new CharacterMasteryDefinition(9, "Магистраль", "Mainline", "Сфера с двумя соседями усиливает их дополнительным бонусом.", "A two-neighbour Sphere grants an additional relay bonus."),
                    new CharacterMasteryDefinition(10, "Главный конструктор", "Master Engineer", "Финальная Сигнатура: сильный связанный узел передаёт усиление соседям.", "Final Signature: a strongly connected master node transfers amplified power.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Berserker, "Берсерк", "Berserker", "Риск / ближний бой", "Risk / Close Range",
                "Чем опаснее положение игрока, тем сильнее становится его боевая сеть.", "The more dangerous the situation, the stronger the combat network becomes.", "#ff625d",
                "Ярость: каждые потерянные 20% HP дают +7% урона и +4% скорости атаки, максимум +28%/+16%. Сферы получают ещё +12% урона по врагам в радиусе 110 px от игрока.", "Fury: every missing 20% HP grants +7% damage and +4% attack speed, up to +28%/+16%. Spheres also deal +12% damage to enemies within 110 px of the player.",
                SphereId.Shotgun, SphereId.Void, 2000,
                0.08f, -0.05f, 0f, 0.08f,
                -0.1f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Ярость", "Fury", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Жажда боя", "Bloodlust", "Ближний бонус увеличивается с +12% до +15%.", "Close-range bonus increases from +12% to +15%."),
                    new CharacterMasteryDefinition(3, "Бешенство", "Frenzy", "+2% базовой скорости атаки сфер.", "+2% base sphere attack speed."),
                    new CharacterMasteryDefinition(4, "На грани", "On the Edge", "Максимальный бонус Ярости активируется уже при 80% потерянного HP.", "Maximum Fury activates at 80% missing HP instead of 100%."),
                    new CharacterMasteryDefinition(5, "Кровавый след", "Blood Trail", "После убийства врага в ближнем радиусе игрок получает +5% скорости на 2 секунды.", "Killing an enemy in close range grants +5% move speed for 2 seconds."),
                    new CharacterMasteryDefinition(6, "Железная воля", "Iron Will", "При низком HP бонус ближнего боя усиливается.", "At low HP, the close-range bonus is stronger."),
                    new CharacterMasteryDefinition(7, "Красная зона", "Red Zone", "При 30% HP и ниже каждый 5-й удар сфер создаёт Redline-импульс вокруг игрока.", "At 30% HP or lower, every 5th Sphere hit creates a Redline pulse around the player."),
                    new CharacterMasteryDefinition(8, "Без тормозов", "No Brakes", "Redline-импульс получает дополнительную силу.", "The Redline pulse gains additional power."),
                    new CharacterMasteryDefinition(9, "Кровь за кровь", "Blood for Blood", "Убийства рядом с игроком сохраняют импульс скорости и продвигают Redline.", "Close-range kills preserve the speed burst and advance Redline."),
                    new CharacterMasteryDefinition(10, "Воплощение ярости", "Avatar of Fury", "При 30% HP и ниже Redline срабатывает каждый 4-й удар с максимальной силой.", "At 30% HP or lower, Redline triggers every 4th hit at maximum power.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Alchemist, "Алхимик", "Alchemist", "Статусы", "Status Effects",
                "Строит сеть из одноэлементных сфер и соединяет Огонь, Заморозку и Яд в реакции.", "Builds a network of single-element Spheres and combines Fire, Freeze and Poison into reactions.", "#57e6b4",
                "Реакции: Огонь+Яд = Воспламенение токсинов; Заморозка+Яд = Крио-токсин; Огонь+Заморозка = Термошок. Одна сфера остаётся одноэлементной, реакция возникает между статусами разных сфер и расходует их.", "Reactions: Fire+Poison = Toxin Ignition; Freeze+Poison = Cryotoxin; Fire+Freeze = Thermal Shock. Each Sphere remains single-element; reactions occur between statuses from different sources and consume them.",
                SphereId.Aura, SphereId.Chain, 2500,
                -0.05f, 0f, 0f, 0f,
                0f, 0.3f, 0.15f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Реакции", "Reactions", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Катализатор", "Catalyst", "Радиус реакций увеличивается на 10%.", "Reaction radius increases by 10%."),
                    new CharacterMasteryDefinition(3, "Концентрация", "Concentration", "+5% урона периодического действия.", "+5% DoT damage."),
                    new CharacterMasteryDefinition(4, "Цепная реакция", "Chain Reaction", "Реакция может передать 50% своего урона-взрыва ещё одной цели рядом.", "A reaction can transfer 50% of its burst damage to one nearby target."),
                    new CharacterMasteryDefinition(5, "Философский камень", "Philosopher Stone", "После реакции следующий наложенный статус длится на 50% дольше.", "After a reaction, the next applied status lasts 50% longer."),
                    new CharacterMasteryDefinition(6, "Катализ", "Catalysis", "Радиус реакций увеличивается ещё на 10%.", "Reaction radius increases by another 10%."),
                    new CharacterMasteryDefinition(7, "Чистая формула", "Pure Formula", "+5% урона реакций.", "+5% reaction damage."),
                    new CharacterMasteryDefinition(8, "Цепь катализаторов", "Catalyst Chain", "Передача урона-взрыва реакции становится сильнее.", "Reaction burst transfer becomes stronger."),
                    new CharacterMasteryDefinition(9, "Тройная смесь", "Triple Mixture", "Третья последовательная реакция получает дополнительный импульс.", "Every third sequential reaction gains an additional pulse."),
                    new CharacterMasteryDefinition(10, "Алхимический круг", "Alchemical Circle", "Финальная Сигнатура: реакционные цепочки получают максимальное усиление.", "Final Signature: reaction chains reach their maximum enhancement.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Architect, "Архитектор", "Architect", "Геометрия", "Geometry",
                "Распознаёт форму локального расположения сфер и превращает её в боевой бонус.", "Recognizes the shape of nearby sphere placement and turns it into a combat bonus.", "#ffc56a",
                "Форма: Архитектор использует авторитетную сеть Геометрии. Доминирующая форма определяет бонус, а Вторая остаётся дополнительным активным слоем.", "Formation: Architect uses the authoritative Geometry network. The Dominant formation determines the character bonus while Secondary remains an active secondary layer.",
                SphereId.Gravity, SphereId.Prism, 3000,
                0f, 0.05f, 0f, -0.05f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Форма", "Formation", "Основная механика персонажа.", "Core character mechanic."),
                    new CharacterMasteryDefinition(2, "Глаз конструктора", "Builder Eye", "Допуск распознавания формы увеличивается примерно на 15%.", "Formation tolerance increases by roughly 15%."),
                    new CharacterMasteryDefinition(3, "Модульность", "Modularity", "+2% радиуса сфер.", "+2% sphere radius."),
                    new CharacterMasteryDefinition(4, "Перестройка", "Rebuild", "После изменения формации новый бонус активируется на 2 секунды с +25% эффективности.", "After changing formation, the new bonus is 25% stronger for 2 seconds."),
                    new CharacterMasteryDefinition(5, "Архитектурный шедевр", "Masterwork", "При 5+ сферах активная форма получает ещё +5% к своему ключевому бонусу.", "At 5+ spheres, the active formation gains another +5% to its key bonus."),
                    new CharacterMasteryDefinition(6, "Точная разметка", "Precise Layout", "Допуск распознавания формации увеличивается ещё на 10%.", "Formation recognition tolerance increases by another 10%."),
                    new CharacterMasteryDefinition(7, "Ритм конструкции", "Construction Rhythm", "Ключевой бонус формации усиливается на 5%.", "Formation key bonuses gain another 5% effectiveness."),
                    new CharacterMasteryDefinition(8, "Перекройка", "Redesign", "Смена формации даёт более сильный краткий импульс.", "Changing formation grants a stronger brief power spike."),
                    new CharacterMasteryDefinition(9, "Многослойность", "Layering", "При 5+ локальных сферах геометрический бонус усиливается.", "With 5+ local spheres, geometry bonuses are amplified."),
                    new CharacterMasteryDefinition(10, "Великий архитектор", "Grand Architect", "Финальная Сигнатура: смена формы превращается в мощный тактический импульс.", "Финальная Сигнатура: смена формации превращается в мощный тактический импульс.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Conductor, "Проводник", "Conductor", "Разряд Резонанса", "Resonance Discharge",
                "Превращает события Резонанса в короткие окна Перегруза и Каскада.", "Turns Resonance Events into short Overdrive and Cascade windows.", "#39d8ff",
                "Каждое событие Резонанса открывает короткое окно Перегруза и сверхзаряжает доминирующую Геометрию.", "Each Resonance Event opens a short Overdrive and supercharges the Dominant Geometry.",
                SphereId.Pulse, SphereId.Chain, 3500,
                0f, 0f, 0.03f, 0f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Разряд", "Discharge", "Resonance Event открывает Overdrive.", "A Resonance Event opens Overdrive."),
                    new CharacterMasteryDefinition(2, "Синхронизация", "Sync", "Окно Перегруза длится дольше.", "Overdrive lasts longer."),
                    new CharacterMasteryDefinition(3, "Резонансный провод", "Resonant Wire", "Line получает два заряда Surge; Triangle, Cluster и Square получают свой сверхзаряд.", "Line gets two Surge charges; Triangle, Cluster and Square receive their own supercharge."),
                    new CharacterMasteryDefinition(4, "Чистый ток", "Pure Current", "Перегрузка усиливается до +23% скорости атаки.", "Overdrive increases to +23% attack speed."),
                    new CharacterMasteryDefinition(5, "Третий узел", "Third Node", "Открывается 3-я Сигнатурная сфера.", "Unlocks the 3rd Signature Sphere."),
                    new CharacterMasteryDefinition(6, "Cascade", "Cascade", "Каждый третий разряд выпускает вторичный импульс.", "Every third discharge emits a secondary discharge."),
                    new CharacterMasteryDefinition(7, "Проводимость", "Conductivity", "Каскад получает дополнительный переход.", "Cascade gains an extra relay jump."),
                    new CharacterMasteryDefinition(8, "Перегрузка", "Overdrive", "Перегрузка усиливается до +28% скорости атаки.", "Overdrive increases to +28% attack speed."),
                    new CharacterMasteryDefinition(9, "Резонансный хор", "Resonant Choir", "Третий разряд может поражать до трёх целей сверхзаряда.", "The third discharge can hit up to three supercharge targets."),
                    new CharacterMasteryDefinition(10, "Абсолютный разряд", "Absolute Discharge", "Открывается 4-я Сигнатурная сфера.", "Unlocks the 4th Signature Sphere.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Oracle, "Оракул", "Oracle", "Предвидение", "Forecast",
                "Читает реальные источники выбора уровня и улучшает Закрепление и Переброс без гарантии случайного выбора.", "Reads real Level-Up sources and improves Lock/Reroll without guaranteeing RNG.", "#b68cff",
                "Перед выбором улучшения отмечает наиболее вероятные доступные карты.", "Before Level-Up, marks the most likely live cards.",
                SphereId.Prism, SphereId.Pulse, 4000,
                0f, 0.04f, 0f, 0f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Прогноз", "Forecast", "Отмечает вероятную живую карту.", "Marks a likely live card."),
                    new CharacterMasteryDefinition(2, "Чтение потока", "Flow Reading", "Учитывает давление незавершённых систем.", "Учитывает давление незавершённых систем."),
                    new CharacterMasteryDefinition(3, "Знак ветви", "Branch Sign", "Учитывает выбранную ветку Сферы.", "Accounts for the selected Sphere branch."),
                    new CharacterMasteryDefinition(4, "Двойное зрение", "Double Sight", "Показывает два варианта.", "Shows two options."),
                    new CharacterMasteryDefinition(5, "Третий осколок", "Third Shard", "Открывается 3-я Сигнатурная сфера.", "Unlocks the 3rd Signature Sphere."),
                    new CharacterMasteryDefinition(6, "Предсказанный Lock", "Foreseen Lock", "Один Переброс может сохранить прогноз.", "Один Переброс может сохранить прогноз."),
                    new CharacterMasteryDefinition(7, "Взгляд за грань", "Beyond Sight", "Прогнозирует источник следующего выбора.", "Forecasts the next choice source."),
                    new CharacterMasteryDefinition(8, "Точное предвидение", "True Foresight", "Прогноз стабильнее при Закреплении и Перебросе.", "Forecast is more stable around Lock/Reroll."),
                    new CharacterMasteryDefinition(9, "Картина билда", "Build Pattern", "Показывает пару поддерживающих карт.", "Shows a supporting pair of cards."),
                    new CharacterMasteryDefinition(10, "Абсолютное зрение", "Absolute Sight", "Открывается 4-я Сигнатурная сфера.", "Unlocks the 4th Signature Sphere.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Voidwalker, "Пустотник", "Voidwalker", "Разрыв сети", "Network Breach",
                "Превращает временные разрывы Сети в слабые Фантомные узлы.", "Turns temporary Network breaks into weaker Phantom Nodes.", "#8a5a8a",
                "При разрыве Сети остаётся Фантомный узел.", "A Network break leaves a Phantom Node.",
                SphereId.Void, SphereId.Gravity, 4500,
                0.04f, 0f, 0f, 0f,
                0f, 0f, 0f, 0.03f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Фантомный узел", "Phantom Node", "Разрыв Сети создаёт слабый фантом.", "A Network break creates a weak phantom."),
                    new CharacterMasteryDefinition(2, "След пустоты", "Void Trace", "Фантом живёт дольше.", "Phantom lasts longer."),
                    new CharacterMasteryDefinition(3, "Разрыв", "Breach", "Фантом замедляет врагов.", "Phantom slows enemies."),
                    new CharacterMasteryDefinition(4, "Тонкая грань", "Thin Edge", "Фантом наносит 60% обычного урона.", "Phantom deals 60% damage."),
                    new CharacterMasteryDefinition(5, "Третий силуэт", "Third Silhouette", "Открывается 3-я Сигнатурная сфера.", "Unlocks the 3rd Signature Sphere."),
                    new CharacterMasteryDefinition(6, "Двойной след", "Double Trace", "Новый разрыв обновляет Фантомный узел.", "A new break refreshes the phantom."),
                    new CharacterMasteryDefinition(7, "Хищная пустота", "Predatory Void", "Фантом сильнее поражает ослабленные цели.", "Phantom hits weakened targets harder."),
                    new CharacterMasteryDefinition(8, "Провал связи", "Link Collapse", "Пустотная и Гравитационная сферы лучше синхронизируются.", "Void/Gravity synchronize better."),
                    new CharacterMasteryDefinition(9, "Отголосок разрыва", "Breach Echo", "Первый импульс усилен.", "First pulse is empowered."),
                    new CharacterMasteryDefinition(10, "Аватар пустоты", "Void Avatar", "Открывается 4-я Сигнатурная сфера.", "Unlocks the 4th Signature Sphere.")
                }),
            new CharacterDefinition(
                SphereCharacterId.Fractal, "Фрактал", "Fractal", "Последовательность", "Sequence",
                "Запоминает последовательность реальной Геометрии и возвращает прошлую форму как Эхо.", "Remembers real Geometry sequences and returns a prior form as an Echo.", "#ffb84d",
                "Три разные доминирующие формации подряд запускают Рекурсивное Эхо.", "Three different Dominant formations trigger Recursive Echo.",
                SphereId.Orbital, SphereId.Aura, 5000,
                0.03f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                new CharacterMasteryDefinition[]
                {
                    new CharacterMasteryDefinition(1, "Память формы", "Shape Memory", "Запоминает доминирующие формации.", "Remembers Dominant formations."),
                    new CharacterMasteryDefinition(2, "Глубокая память", "Deep Memory", "История хранит четыре шага.", "History keeps four steps."),
                    new CharacterMasteryDefinition(3, "Рекурсия", "Recursion", "Открывает запуск Рекурсивного Эха после трёх разных формаций.", "Unlocks Recursive Echo after three different formations."),
                    new CharacterMasteryDefinition(4, "Устойчивый Echo", "Stable Echo", "Эхо длится 4 секунды.", "Echo lasts 4 seconds."),
                    new CharacterMasteryDefinition(5, "Третий виток", "Third Loop", "Открывается 3-я Сигнатурная сфера.", "Unlocks the 3rd Signature Sphere."),
                    new CharacterMasteryDefinition(6, "Сила Echo", "Echo Strength", "Эхо достигает 60% мощности.", "Echo reaches 60%."),
                    new CharacterMasteryDefinition(7, "Четвёртый шаг", "Fourth Step", "История хранит четыре разные формы.", "History can hold four different forms."),
                    new CharacterMasteryDefinition(8, "Рекурсивная волна", "Recursive Wave", "Эхо получает дополнительный импульс.", "Echo gains an extra pulse."),
                    new CharacterMasteryDefinition(9, "Самоподобие", "Self Similarity", "Следующая уникальная форма ускоряет новую последовательность.", "Next unique form advances a new chain."),
                    new CharacterMasteryDefinition(10, "Рекурсивный мастер", "Recursive Master", "Открывается 4-я Сигнатурная сфера и 70% силы Эха.", "Unlocks the 4th Signature Sphere and 70% Echo.")
                })
        };

        public static IReadOnlyList<CharacterDefinition> All => Definitions;
        public static int MasteryMaxXp => MasteryThresholds[MasteryThresholds.Length - 1];

        public static CharacterDefinition Get(SphereCharacterId id)
        {
            for (var i = 0; i < Definitions.Length; i++) if (Definitions[i].Id == id) return Definitions[i];
            return Definitions[0];
        }

        public static bool TryParseSourceId(string raw, out SphereCharacterId id)
        {
            id = SphereCharacterId.Spherist;
            if (string.IsNullOrEmpty(raw)) return false;
            for (var i = 0; i < Definitions.Length; i++)
                if (string.Equals(raw, ToSourceId(Definitions[i].Id), StringComparison.Ordinal)) { id = Definitions[i].Id; return true; }
            return false;
        }

        public static string ToSourceId(SphereCharacterId id) => id.ToString().ToLowerInvariant();

        public static int GetMasteryThreshold(int level) => MasteryThresholds[Math.Max(1, Math.Min(10, level)) - 1];

        public static int GetMasteryLevelForXp(int xp)
        {
            var safeXp = Math.Max(0, Math.Min(MasteryMaxXp, xp));
            var level = 1;
            for (var i = 1; i < MasteryThresholds.Length; i++) if (safeXp >= MasteryThresholds[i]) level = i + 1;
            return level;
        }

        public static int GetNextMasteryThreshold(int level) => level >= 10 ? -1 : MasteryThresholds[Math.Max(1, level)];

        public static int GetUpdatedMasteryXp(int currentXp, int amount)
        {
            var current = Math.Max(0, Math.Min(MasteryMaxXp, currentXp));
            return Math.Max(0, Math.Min(MasteryMaxXp, current + Math.Max(0, amount)));
        }

        public static float GetSphereAffinityWeight(SphereCharacterId character, SphereId type)
        {
            var definition = Get(character);
            if (type == definition.SignatureSphere) return 0.70f;
            if (type == definition.PartnerSphere) return 0.30f;
            return 0f;
        }

        public static float GetBerserkerFuryDamageBonus(float hpRatio)
        {
            var missingRatio = Math.Max(0f, Math.Min(1f, 1f - hpRatio));
            // Preserve the source's exact 20% HP threshold despite float representation of 0.6/0.4.
            return Math.Min(4, (int)Math.Floor((missingRatio + 0.00001f) / 0.2f)) * 0.07f;
        }

        public static float GetBerserkerFuryAttackSpeedBonus(float hpRatio)
        {
            var missingRatio = Math.Max(0f, Math.Min(1f, 1f - hpRatio));
            // Use the same boundary handling as the source Fury damage calculation.
            return Math.Min(4, (int)Math.Floor((missingRatio + 0.00001f) / 0.2f)) * 0.04f;
        }

        public static float GetBerserkerCloseDamageBonus(int masteryLevel)
            => masteryLevel >= 10 ? 0.20f : masteryLevel >= 2 ? 0.15f : 0.12f;

        public static float GetSpheristDamageBonus(int masteryLevel, int sphereCount)
        {
            var threshold = masteryLevel >= 8 ? 3 : masteryLevel >= 2 ? 4 : 5;
            var bonus = sphereCount >= threshold ? 0.05f : 0f;
            if (masteryLevel >= 3) bonus += 0.02f;
            if (masteryLevel >= 6 && sphereCount >= 6) bonus += 0.02f;
            if (masteryLevel >= 10 && sphereCount >= 8) bonus += 0.03f;
            return bonus;
        }

        public static float GetSpheristAttackSpeedBonus(int masteryLevel, int sphereCount)
        {
            var bonus = Math.Max(0, sphereCount - 1) * 0.03f;
            if (masteryLevel >= 4) bonus += Math.Max(0, sphereCount - 1) * 0.005f;
            if (masteryLevel >= 5 && sphereCount >= 8) bonus += 0.05f;
            if (masteryLevel >= 7 && sphereCount >= 7) bonus += 0.02f;
            if (masteryLevel >= 10 && sphereCount >= 8) bonus += 0.03f;
            return bonus;
        }

        // Source characterRuntime.ts: Fractal Recursive Echo timing and strength.
        public static float GetFractalEchoStrength(int masteryLevel) =>
            masteryLevel >= 10 ? 0.70f : masteryLevel >= 6 ? 0.60f : 0.50f;
        public static float GetFractalEchoDuration(int masteryLevel) => masteryLevel >= 4 ? 4f : 3.5f;
        public static float GetFractalEchoPulseInterval(int masteryLevel) => masteryLevel >= 8 ? 0.65f : 0.8f;

        // Source characterRuntime.ts: Voidwalker Phantom Node.
        public static float GetVoidPhantomDuration(int masteryLevel) =>
            masteryLevel >= 10 ? 3.5f : masteryLevel >= 2 ? 3f : 2.5f;
        public static float GetVoidPhantomDamage(
            int playerLevel, int masteryLevel, float enemyHpFraction, bool activeVoidGravity, int pulseCount)
        {
            var damage = (12f + playerLevel) *
                (masteryLevel >= 10 ? 0.70f : masteryLevel >= 4 ? 0.60f : 0.50f);
            if (masteryLevel >= 7 && enemyHpFraction <= 0.35f)
                damage *= masteryLevel >= 10 ? 1.35f : 1.20f;
            if (masteryLevel >= 8 && activeVoidGravity) damage *= 1.10f;
            if (masteryLevel >= 9 && pulseCount == 1) damage *= 1.50f;
            return damage;
        }
        public const float VoidPhantomSlowDuration = 0.6f;
        public const float VoidPhantomSlowMultiplier = 0.82f;
    }
}
