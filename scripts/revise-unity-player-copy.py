"""Build exact presentation revisions from the exported Unity text inventory.

Input comes from PlayerCopyInventory; gameplay catalogs remain unchanged.
The committed PlayerCopy.json is the runtime copy source, not a regex filter.
"""
import json
import pathlib
import sys

root = pathlib.Path(__file__).resolve().parents[1]
inventory = json.loads((root / "UnityProject/Library/player-copy/inventory.json").read_text(encoding="utf-8"))

ru_terms = {
    "Resonance Event": "всплеск резонанса", "Resonance": "резонанс",
    "Standard-сферы": "Стандартной сферы", "Standard-сфера": "Стандартная сфера",
    "Standard-импульс": "импульс Стандартной сферы", "Standard": "Стандартной сферы",
    "Sniper-сфере": "Снайперской сфере", "Sniper": "Снайперской сферы",
    "Chain-сферы": "Цепной сферы", "Chain-сфера": "Цепная сфера", "Chain-цикла": "цепного разряда",
    "узле Chain": "узле цепи", "сеть Chain": "цепную сеть", "через Chain": "через Цепную сферу", "Chain": "Цепной сферы",
    "Aura-сферу": "Ауру", "Aura": "Аура",
    "Void-удар": "удар Пустотной сферы", "вокруг Void": "вокруг Пустотной сферы", "Убийство Void": "Убийство Пустотной сферой", "Void": "Пустотной сферы",
    "Dark Ritual": "Перегрузка", "Echo Drone": "эхо-дрон", "Dash": "рывка",
    "Afterimage-следы": "призрачные следы", "Afterimage-следов": "призрачных следов",
    "эффект Afterimage": "призрачный след", "Afterimage": "призрачный след",
    "Phase-пробитие": "усиленное пробитие", "Phase": "пробитие", "Corrupt": "ослабление",
    "порог исполнения": "порог добивания", "Execute": "добивание",
    "Anchor": "сдерживание", "Shatter": "раскол", "Impact": "ударную волну",
    "Freeze": "заморозка", "Burst": "взрыва", "Redline": "яростный", "Overdrive": "сверхзаряд",
    "Line": "Линия", "Triangle": "Треугольник", "Cluster": "Скопление", "Square": "Квадрат",
    "Surge": "усиления", "Core": "ядро", "Echo": "Эхо", "Lock": "закрепление",
    "более частыми тиками": "чаще", "наносит свой урон": "наносит урон",
    "критического множителя": "урона критических ударов", "множитель добивания": "урон добивания",
    "билда": "сборки", "билд": "сборка", "HP": "ОЗ", "КД ": "перезарядка: ",
    "->": "→", "XP": "опыта",
}
en_terms = {
    "Burn deals its damage in more frequent ticks.": "Burn deals damage more often.",
    "Poison deals its damage in more frequent ticks.": "Poison deals damage more often.",
    "widens the hit window along its path": "hits enemies across a wider path",
    "and gains Impact": "and creates a shockwave", "sustain the Void loop": "help you stay in the fight",
    "the current formation": "your formation", "along real links": "along linked Spheres",
    "critical multiplier": "critical damage", "crit multiplier": "critical damage",
}
ru_phrases = {
    "После конечного удара часть заряда возвращается к первой Chain-сфере и создаёт второй слабый разряд.": "После последнего удара часть заряда возвращается к первой Цепной сфере и создаёт второй слабый разряд.",
    "Разряд проходит от игрока через Chain-сферы к 1 цели и наносит 55 урона.": "Разряд проходит от игрока через Цепные сферы к 1 цели и наносит 55 урона.",
    "Разряд проходит через все Chain-сферы и усиливает каждый следующий перескок.": "Разряд проходит через все Цепные сферы и усиливается с каждым следующим перескоком.",
    "Разряд проходит через каждую Chain-сферу перед ударом по цели.": "Перед ударом по цели разряд проходит через каждую Цепную сферу.",
    "Anchor и статический разряд работают вместе, превращая цепь в плотную зону контроля.": "Разряды создают плотную зону замедления и сдерживают врагов.",
    "Execute и Phase объединяются; Phase добавляет пробитие, а порог исполнения повышается до 30% HP.": "Атаки пробивают врагов и могут добивать цели с 30% ОЗ или меньше.",
    "Freeze и Anchor работают вместе, а сама аура получает +12% урона.": "Аура сильнее замораживает и сдерживает врагов, а её урон увеличивается на +12%.",
    "Impact и Afterimage работают вместе, а урон лезвий возрастает ещё сильнее.": "Удары лезвий и их призрачные следы становятся ещё сильнее.",
    "Shatter и Impact работают вместе, а Burst создаёт дополнительную волну.": "Импульс наносит усиленный урон, отбрасывает врагов и выпускает дополнительную волну.",
    "Аура получает более сильный Anchor и удерживает врагов до 2 с.": "Аура сильнее сдерживает врагов и удерживает их до 2 с.",
    "Боевые лезвия получают усиление Impact и сильнее бьют по траектории.": "Боевые лезвия наносят больше урона врагам на своей траектории.",
    "Крит по отмеченной цели наносит +22% урона, дополнительно задевает врагов рядом с целью и получает усиление Execute по целям ниже 30% HP.": "Критическое попадание по отмеченной цели наносит +22% урона, задевает врагов рядом и становится сильнее против целей ниже 30% ОЗ.",
    "Метка и Anchor работают вместе, распространяя замедление на область вокруг цели.": "Метка замедляет цель и врагов рядом с ней.",
    "Основной импульс получает усиление Impact и лучше отбрасывает врагов.": "Основной импульс сильнее отбрасывает врагов.",
    "Основной импульс получает усиление Shatter.": "Попадания импульса по замороженным целям дополнительно ранят врагов рядом.",
    "Перегрев усиливает следующий импульс Aura и поджигает врагов внутри её зоны.": "Перегрев усиливает следующий импульс Ауры и поджигает врагов в её области действия.",
    "Повышенный Resonance-синергический эффект дополняется Shatter для усиления боевого импульса.": "Импульсы сильнее заряжают резонанс. Попадания по замороженным целям дополнительно ранят врагов рядом.",
    "Повышенный Resonance-синергический эффект дополняется усилением Impact для контроля врагов.": "Импульсы сильнее заряжают резонанс и лучше отбрасывают врагов.",
    "Помеченная цель замедляется сильнее, а зона контроля вокруг неё получает усиливающийся эффект Anchor.": "Метка сильнее замедляет цель и сдерживает врагов рядом с ней.",
    "Попадание создаёт большой взрыв радиусом 85, наносящий соседним врагам 65% урона и получает Impact.": "Попадание создаёт взрыв радиусом 85, который отбрасывает соседних врагов и наносит им 65% урона.",
    "Попадание создаёт взрыв радиусом 60, наносящий соседним врагам 45% урона; взрыв получает Shatter.": "Попадание создаёт взрыв радиусом 60, наносящий соседним врагам 45% урона. Удары по замороженным целям дополнительно ранят врагов рядом.",
    "Попадание создаёт взрыв радиусом 55 с 35% вторичного урона, Shatter/Impact и замедляет поражённых врагов.": "Попадание создаёт взрыв радиусом 55, наносит врагам рядом 35% урона, отбрасывает их и замедляет основную цель. Удары по замороженным целям дополнительно ранят врагов рядом.",
    "След лезвия получает эффект Afterimage, усиливая повторные атаки орбитали.": "Лезвия оставляют призрачные следы, усиливающие повторные атаки.",
    "Сфера получает Phase, добавляющий пробитие атакам.": "Атаки сферы пробивают врагов.",
    "Цепь усиливает замедление цели, а Anchor дополнительно удерживает её в зоне контроля.": "Цепь сильнее замедляет цель и дольше сдерживает её.",
    "Redline-импульс": "Импульс ярости", "продвигают Redline": "приближают всплеск ярости", "Redline срабатывает": "всплеск ярости срабатывает",
    "После прохождения Standard импульс расширяется": "После прохождения Стандартной сферы импульс расширяется",
    "Телепорт оставляет реле между исходной и целевой сферами, временно связывая их.": "После прыжка сферы в начале и конце пути на время соединяются.",
    "Телепорт соединяет исходный и целевой узлы визуальным сетевым реле.": "После прыжка сферы в начале и конце пути соединяет световая связь.",
    "Последний переход сохраняет заряд и сокращает время до следующего Chain-цикла.": "Последний переход сохраняет заряд, ускоряя следующий цепной разряд.",
    "Второй разряд становится мощнее, сохраняя двойную волну Burst.": "Второй разряд становится мощнее. Импульс по-прежнему выпускает две волны.",
    "Сильная тяга и Anchor объединяются; собранная группа получает усиленный импульсный урон.": "Притягивает и замедляет врагов, затем поражает собранную группу усиленным импульсом.",
    "Anchor и усиленная тяга работают вместе, превращая центр в устойчивую точку контроля.": "Сильнее притягивает врагов к центру и замедляет их.",
    "Крит по отмеченной цели": "Критическое попадание по отмеченной цели",
}
en_phrases = {
    "Critical hits against marked targets deal 22% more damage, also damage nearby enemies, and gain stronger Execute pressure below 30% HP.": "Critical hits against marked targets deal 22% more damage, hit nearby enemies, and become stronger against targets below 30% HP.",
    "Hits create a blast with radius 60, dealing 45% of the hit damage to nearby enemies; the blast gains Shatter.": "Hits create a blast with radius 60, dealing 45% of the hit damage to nearby enemies. Hitting a frozen target also damages enemies around it.",
    "Hits create a large blast with radius 85, dealing 65% of the hit damage to nearby enemies and gains Impact.": "Hits create a blast with radius 85, dealing 65% of the hit damage to nearby enemies and knocking them back.",
    "Hits create a blast with radius 55, dealing 35% secondary damage, Shatter/Impact, and slowing affected enemies.": "Hits create a blast with radius 55, dealing 35% of the hit damage to nearby enemies, knocking them back, and slowing the main target. Hitting a frozen target also damages enemies around it.",
    "Strong pull and Anchor combine; the gathered group takes an empowered pulse hit.": "Pulls enemies together and slows them. The gathered group takes a stronger pulse hit.",
    "Combat blades gain an Impact effect and hit harder along their path.": "Combat blades hit harder along their path and knock enemies back.",
    "Blade trails gain an Afterimage effect, strengthening repeated orbital attacks.": "Blades leave echoes that strengthen follow-up attacks.",
    "Impact and Afterimage work together, and blade damage increases further.": "Blade hits and their echoes become stronger and knock enemies back.",
    "Anchor and stronger pull work together, turning the center into a stable control point.": "Stronger pull gathers enemies at the center and slows them.",
    "The stronger Resonance interaction is combined with an Impact effect for better control.": "Pulses grant more Resonance and knock enemies back more effectively.",
    "The stronger Resonance interaction is combined with Shatter for stronger pulse damage.": "Pulses grant more Resonance. Hitting a frozen target also damages enemies around it.",
    "The main pulse gains a Shatter effect.": "Hitting a frozen target with the main pulse also damages enemies around it.",
    "The main pulse gains an Impact effect and knocks enemies away more effectively.": "The main pulse knocks enemies back more effectively.",
    "Shatter and Impact combine, and Burst creates an additional wave.": "The pulse knocks enemies back and releases an extra wave. Hitting a frozen target also damages enemies around it.",
    "Execute chance and damage against weakened targets are increased.": "Increases the chance to finish off weakened enemies and the damage dealt to them.",
    "The Sphere gains Phase, adding penetration to its attacks.": "Sphere attacks pierce enemies.",
    "Execute and Phase combine; Phase adds penetration, and the execution threshold rises to 30% HP.": "Attacks pierce enemies and can finish off targets at 30% HP or less.",
}
ru_exact = {
    "undefined": "", "[object Object]": "",
    "Echo-Sphere": "ECHO SPHERE", "Что такое Echo Sphere?": "Как играть в ECHO SPHERE?",
    "ПЕРВЫЙ СТАБИЛЬНЫЙ ECHO": "ПЕРВЫЙ УСТОЙЧИВЫЙ РЕЗОНАНС",
    "Echo Network": "Эхо-сеть", "Resonant Core": "Резонансное ядро", "Infinite Pulse": "Бесконечный импульс",
    "Network Bastion": "Бастион сети", "Iron Dome": "Железный купол", "Resonant Guard": "Резонансный страж",
    "Spatial Network": "Пространственная сеть", "Hunter Beacon": "Маяк охотника", "Phase Break": "Фазовый разрыв",
    "Thermal Network": "Тепловая сеть", "Catalyst Flame": "Катализатор пламени", "Network Inferno": "Огненная сеть",
    "Echo Swarm": "Эхо-рой", "Network Nodes": "Узлы сети", "Sphere Guardians": "Стражи сфер",
    "Storm Network": "Грозовая сеть", "Thunder Network": "Сеть молний", "Overload Core": "Ядро перегрузки",
    "Temporal Core": "Ядро времени", "Blood Network": "Кровавая сеть", "Sacrifice Core": "Жертвенное ядро",
    "Void Engine": "Двигатель пустоты", "Cascade": "Каскад", "Устойчивый Echo": "Устойчивое эхо", "Сила Echo": "Сила эха",
    "Предсказанный Lock": "Предвидение", "Картина билда": "Общий замысел",
    "Читает реальные источники выбора уровня и улучшает Закрепление и Переброс без гарантии случайного выбора.":
        "Предвидит будущие улучшения и помогает выбирать, какие из них сохранить или заменить.",
    "Прогнозирует источник следующего выбора.": "Предсказывает, какой вид улучшений появится следующим.",
    "Запоминает последовательность реальной Геометрии и возвращает прошлую форму как Эхо.":
        "Запоминает чередование формаций и повторяет их силу в виде эха.",
    "Выбери любое улучшение. Так каждый забег начинает собирать собственный билд.":
        "Выбери улучшение, чтобы усилить героя или его сферы.",
    "Резонанс — это не второй опыт и не просто счётчик попаданий. Это общий ресурс забега, который награждает активную игру сети.":
        "Атаки сфер заряжают резонанс. Когда шкала заполнится, сеть выпустит мощный боевой всплеск.",
    "Свайп/джойстик — движение": "Перемещайся с помощью джойстика.",
    "Множитель золота": "Бонус кредитов",
    "Уровень I начинается с 1+1, затем добавления чередуются до 4+4 на VII.":
        "На первом уровне по одному боевому элементу на каждой орбите. С развитием сферы их число растёт до четырёх на каждой.",
}
ru_exact.update({
    "Золото": "Кредиты", "Золото получено": "Получено кредитов", "Золото (+{amount})": "Кредиты (+{amount})",
    "Сбросить весь прогресс (золото, улучшения, рекорды)?": "Удалить весь прогресс? Персонажи, улучшения, открытые регионы и сохранённый забег будут сброшены.",
    "Заряд событий × 0,65; POI и руны без изменений.": "События дают резонанс с силой × 0,65. Руны и особые места сохраняют обычную силу.",
    "За босса +1, за стабилизацию +3. Открытие персонажей и необязательное ускорение мастерства; обычный рост от XP сохраняется.": "Получайте +1 за каждого босса и +3 за стабилизацию региона. Тратьте осколки на открытие героев и повышение мастерства.",
    "За новую находку +1. Исследование региональной записи стоит 3 и не приносит новых данных.": "Получайте +1 за каждое новое открытие. Исследование одной записи о регионе стоит 3.",
    "Мгновенно даёт 60 резонанса с действующими модификаторами заряда.": "Мгновенно даёт 60 резонанса. Бонусы к его накоплению усиливают заряд.",
    "Получаемый урон ×0,65 на 8 с; щиты не добавляются.": "На 8 с снижает получаемый урон на 35%.",
    "Помечает ближайшую элиту или босса на 8 с. Метка поддерживает действующие бонусы Охотника.": "На 8 с помечает ближайшего элитного врага или босса. Охотник использует эту метку для усиления своих атак.",
    "Повторяет следующую успешную атаку сферы. Пустая атака заряд не расходует; заряды не складываются.": "Повторяет следующую атаку сферы, попавшую по врагу. Можно хранить один заряд.",
    "+5 кредитов в конце забега. Выпадает только в Endless и Convergence.": "+5 кредитов в конце забега. Можно найти в Бесконечности и Схождении.",
    "45 + 4 × уровень урона обычным врагам в радиусе 240. Элиты и боссы не затрагиваются.": "В радиусе 240 наносит обычным врагам 45 урона и ещё 4 за каждый уровень героя. На элиту и боссов не действует.",
    "SPHERE NULLIFIED": "СФЕРА ОТКЛЮЧЕНА", "DISRUPTOR": "ПОМЕХИ", "FRACTAL ECHO": "ФРАКТАЛЬНОЕ ЭХО",
    "LATTICE CASCADE": "КАСКАД РЕШЁТКИ", "RING LOOP": "СИЛА КОЛЬЦА", "SQUARE RESONANCE": "РЕЗОНАНС КВАДРАТА",
    "TRIANGLE RESONANCE": "РЕЗОНАНС ТРЕУГОЛЬНИКА", "CLUSTER RESONANCE": "РЕЗОНАНС СКОПЛЕНИЯ",
    "LINE RESONANCE": "РЕЗОНАНС ЛИНИИ", "RESONANCE": "РЕЗОНАНС", "STELLA CACHE EMPTY": "Новых наград пока нет",
    "ARTIFACT CACHE EMPTY": "Новых артефактов пока нет", "STELLA": "СТЕЛЛА", "BOSS DEFEATED!": "БОСС ПОВЕРЖЕН!",
    "Move closer to the Sphere formation first.": "Подойди ближе к своим сферам.",
    "Formation Follow engaged.": "Сферы следуют за тобой.", "Formation Follow disengaged.": "Сферы остаются на месте.",
    "LOCK": "ЗАКРЕПЛЕНО", "LOCK + REROLL": "ВЫБОР ОБНОВЛЁН", "REROLL": "НОВЫЙ ВЫБОР",
    "CHORUS SYNC": "СОЗВУЧИЕ", "CHORUS PULSE": "ИМПУЛЬС СОЗВУЧИЯ", "REDLINE": "ВСПЛЕСК ЯРОСТИ",
    "RECURSIVE ECHO": "ПОВТОРНОЕ ЭХО", "PHANTOM NODE": "ПРИЗРАЧНЫЙ УЗЕЛ", "BREACH REFRESH": "НОВЫЙ ПРОРЫВ",
    "ECHO PULSE": "ЭХО-ИМПУЛЬС", "SPHERE BARRIER": "СФЕРНЫЙ БАРЬЕР", "OVERHEAT": "ПЕРЕГРЕВ",
    "ECHO DRONE": "ЭХО-ДРОН", "CHAIN LIGHTNING": "ЦЕПНОЙ РАЗРЯД", "OVERLOAD": "ПЕРЕГРУЗКА",
    "ECHO FREEZE": "ЭХО-ЗАМОРОЗКА", "FOLLOW: TELEPORT LOCKED": "Отключи следование сфер, чтобы прыгнуть.",
    "FOLLOW: DASH LOCKED": "Отключи следование сфер, чтобы совершить рывок.", "ECHO JUMP": "ЭХО-ПРЫЖОК",
    "GEOMETRY AUTO": "Формация выбирается автоматически", "SECONDARY GEOMETRY AUTO": "Дополнительная формация выбирается автоматически",
    "Path blocked.": "Путь перекрыт.", "SPLITTER": "РАСКОЛ", "GEOMETRY SHIFT": "СМЕНА ФОРМАЦИИ",
    "STASIS FIELD": "ПОЛЕ СТАЗИСА", "NETWORK BREAK": "РАЗРЫВ СЕТИ", "MUTATION!": "МУТАЦИЯ!",
})
en_exact = {
    "undefined": "", "[object Object]": "",
    "Reads real Level-Up sources and improves Lock/Reroll without guaranteeing RNG.":
        "Foresees upcoming upgrades and helps you decide which to keep or replace.",
    "Forecasts the next choice source.": "Reveals which kind of upgrade will appear next.",
    "Remembers real Geometry sequences and returns a prior form as an Echo.":
        "Remembers the order of your formations and echoes their power.",
    "The real run starts now. Build the Network, evolve Spheres, and discover powerful synergies.":
        "Your journey begins. Build your network, evolve your Spheres, and discover powerful combinations.",
    "Resonance is not XP and not merely a hit counter. It is a shared run resource that rewards active Network play.":
        "Sphere attacks charge Resonance. Fill the meter to unleash a powerful burst from your network.",
}
en_exact.update({
    "Gold": "Credits", "Gold earned": "Credits earned", "Gold (+{amount})": "Credits (+{amount})", "Gold multiplier": "Credit bonus",
    "Reset all progress (gold, upgrades, records)?": "Delete all progress? This resets your heroes, upgrades, unlocked regions, and saved run.",
    "Event charge × 0.65; POIs and runes unchanged.": "Resonance gained from events is multiplied by 0.65. Runes and special locations keep their usual strength.",
    "+1 per Boss, +3 for stabilization. Character unlocks and optional mastery acceleration; normal XP progression remains available.": "Earn +1 for each boss and +3 for stabilizing a region. Spend Shards to unlock heroes and advance their mastery.",
    "New discovery +1. Regional lore research costs 3 and does not grant new Data.": "Earn +1 for each new discovery. Researching a regional lore entry costs 3.",
    "Instantly adds 60 Resonance, using existing charge modifiers.": "Instantly grants 60 Resonance. Bonuses to Resonance gain increase the charge.",
    "Damage taken ×0.65 for 8 s; grants no shield charges.": "Reduces damage taken by 35% for 8 s.",
    "Marks the nearest Elite or Boss for 8 s, supporting the Hunter’s existing mark bonuses.": "Marks the nearest elite enemy or boss for 8 s. The Hunter can use this mark to strengthen attacks.",
    "Repeats the next successful Sphere attack. Empty attacks do not consume it; charges do not stack.": "Repeats the next Sphere attack that hits an enemy. You can hold one charge.",
    "45 + 4 × level damage to ordinary enemies within 240. Elites and bosses are excluded.": "Deals 45 damage plus 4 per hero level to normal enemies within 240. Does not affect elites or bosses.",
    "SPHERE NULLIFIED": "SPHERE DISABLED", "STELLA CACHE EMPTY": "No new rewards available",
    "ARTIFACT CACHE EMPTY": "No new artifacts available", "RECURSIVE ECHO": "ECHO RETURNS",
    "FOLLOW: TELEPORT LOCKED": "Stop following before teleporting.", "FOLLOW: DASH LOCKED": "Stop following before dashing.",
    "GEOMETRY AUTO": "Formation selection is automatic", "SECONDARY GEOMETRY AUTO": "Secondary formation selection is automatic",
})

ru_terms[" px"] = ""
en_terms[" px"] = ""
ru_exact.update({
    "Учитывает давление незавершённых систем.": "Прогноз учитывает развитие ваших сфер и способностей.",
    "ORACLE: SPHERE": "ПРОГНОЗ: СФЕРА", "ORACLE: ABILITY": "ПРОГНОЗ: СПОСОБНОСТЬ",
})
en_exact.update({
    "-12% интервала": "Attack interval reduced by 12%",
    "Финальная Сигнатура: смена формации превращается в мощный тактический импульс.": "Final Signature: changing formation unleashes a powerful tactical pulse.",
    "Учитывает давление незавершённых систем.": "Forecasts account for the development of your Spheres and abilities.",
    "Один Переброс может сохранить прогноз.": "One reroll can preserve a forecasted upgrade.",
})

def rewrite(value, ru):
    original = value
    exact = ru_exact if ru else en_exact
    if value in exact:
        return exact[value]
    for before, after in (ru_phrases if ru else en_phrases).items():
        value = value.replace(before, after)
    for before, after in (ru_terms if ru else en_terms).items():
        value = value.replace(before, after)
    if ru:
        # Grammar corrections after glossary expansion; these are authored phrases,
        # not runtime substitutions or changes to the underlying effects.
        polish = {
            "связанные Стандартной сферы": "связанные Стандартные сферы",
            "ближайшей Стандартной сферы-сфере": "ближайшей Стандартной сфере",
            "Активация эхо-дрон запускает": "Призыв эхо-дрона запускает",
            "Создаёт 1 эхо-дрон на": "Создаёт 1 эхо-дрона на",
            "Создаёт 2 эхо-дрон на": "Создаёт 2 эхо-дрона на",
            "получает усиление ослабление для работы по ослабленным целям": "ослабляет врагов, усиливая следующие попадания по ним",
            "Если игрок получает урон от жертвы,": "При потере здоровья от жертвы,",
            "потерянного ОЗ": "потерянного здоровья",
            "потраченного ОЗ": "потраченного здоровья",
            "максимального ОЗ": "максимального здоровья",
            "Потерянное ОЗ": "Потерянное здоровье",
            "Жертва ОЗ": "Жертва здоровья",
            "Потеря ОЗ": "Потеря здоровья",
            "возвращает ОЗ": "восстанавливает здоровье",
            "возвращают ОЗ": "восстанавливают здоровье",
            "возвращает немного ОЗ": "восстанавливает немного здоровья",
            "в ОЗ игрока": "в здоровье игрока",
            "с силой × 0,65": "в размере 65% от обычного",
        }
        for before, after in polish.items(): value = value.replace(before, after)
    if value and original and original[0].isupper() and value[0].islower():
        value = value[0].upper() + value[1:]
    return value

texts = {True: set(), False: set()}
def collect(value, field=""):
    if isinstance(value, dict):
        for key, item in value.items():
            if isinstance(item, str):
                if key.endswith(("Ru", "Russian")) or key == "Ru": texts[True].add(item)
                if key.endswith(("En", "English")) or key == "En": texts[False].add(item)
            elif isinstance(item, (dict, list)): collect(item, key)
    elif isinstance(value, list):
        for item in value:
            if isinstance(item, str):
                if field.endswith("Ru"): texts[True].add(item)
                if field.endswith("En"): texts[False].add(item)
            else: collect(item, field)

collect(inventory)
texts[True].update(ru_exact)
texts[False].update(en_exact)

# Rewrite the full explanatory paragraph instead of exposing internal event names.
for value in texts[True]:
    if value.startswith("Как копится: попадания сфер дают +1;"):
        ru_exact[value] = "Попадания сфер дают +1 к резонансу, а новая формация — +5. Руны и некоторые эффекты дают дополнительный заряд.\n\nПри 100 единицах резонанс срабатывает автоматически. Его эффект зависит от вашей формации: Треугольник атакует вдоль связей, Скопление отбрасывает врагов, а Линия усиливает следующий выстрел."
for value in texts[False]:
    if value.startswith("How it charges: Sphere hits give +1;"):
        en_exact[value] = "Sphere hits grant +1 Resonance; forming a new shape grants +5. Runes and other effects can add more charge.\n\nAt 100, Resonance activates automatically. Its effect depends on your formation: Triangle attacks along links, Cluster knocks enemies back, and Line empowers the next shot."

abilities = [
    ("radius", "Увеличивает дальность атак и область действия сфер.", "Increases Sphere range and area of effect."),
    ("damage", "Увеличивает урон сфер.", "Increases Sphere damage."),
    ("attackspeed", "Сферы атакуют чаще.", "Makes Spheres attack faster."),
    ("maxspheres", "Позволяет размещать больше сфер.", "Lets you place more Spheres."),
    ("blast", "Выпускает боевой импульс через сеть сфер.", "Sends a damaging pulse through your Sphere network."),
    ("shield", "Сферы образуют защитный барьер.", "Your Spheres form a protective barrier."),
    ("teleport", "Мгновенно перемещает к сфере.", "Instantly teleports you to a Sphere."),
    ("movespeed", "Увеличивает скорость передвижения.", "Increases movement speed."),
    ("slow", "Замедляет врагов.", "Slows enemies."),
    ("vitality", "Увеличивает максимальное здоровье.", "Increases maximum health."),
    ("firetrail", "На короткое время усиливает сеть сфер.", "Temporarily empowers your Sphere network."),
    ("minion", "Призывает временных дронов, помогающих сферам.", "Summons temporary drones that support your Spheres."),
    ("vampire", "Восстанавливает здоровье при нанесении урона.", "Restores health when you deal damage."),
    ("lightning", "Выпускает цепной разряд через сеть сфер.", "Unleashes chain lightning through your Sphere network."),
    ("dodge", "Даёт шанс избежать урона.", "Grants a chance to avoid damage."),
    ("crit", "Увеличивает шанс критического удара.", "Increases critical hit chance."),
    ("timestop", "Ненадолго останавливает врагов.", "Briefly stops enemies in time."),
    ("magnet", "Позволяет собирать опыт с большего расстояния.", "Lets you collect experience from farther away."),
    ("sphereboost", "Усиливает сферы за убийства врагов.", "Strengthens your Spheres as you defeat enemies."),
    ("darkritual", "Усиливает сеть ценой части здоровья.", "Empowers your network at the cost of some health."),
    ("armor", "Уменьшает получаемый урон.", "Reduces incoming damage."),
    ("regen", "Постепенно восстанавливает здоровье.", "Restores health over time."),
    ("resonance", "Ускоряет накопление резонанса.", "Increases Resonance gained."),
    ("cooldown", "Сокращает перезарядку способностей.", "Reduces ability cooldowns."),
    ("critpower", "Усиливает критические удары.", "Makes critical hits deal more damage."),
    ("range", "Увеличивает дальность и область действия сфер.", "Increases Sphere range and area of effect."),
    ("proj_speed", "Ускоряет снаряды сфер.", "Makes Sphere projectiles travel faster."),
    ("momentum", "После рывка временно повышает скорость передвижения.", "Temporarily increases movement speed after a dash."),
    ("fortune", "Повышает шанс редких находок.", "Improves your chance of finding rare rewards."),
    ("stability", "Сферы быстрее восстанавливаются после отключения.", "Helps disabled Spheres recover sooner."),
]
result = {
    "Ru": [{"Source": value, "Text": rewrite(value, True)} for value in sorted(texts[True]) if rewrite(value, True) != value],
    "En": [{"Source": value, "Text": rewrite(value, False)} for value in sorted(texts[False]) if rewrite(value, False) != value],
    "Abilities": [{"Id": key, "Ru": ru, "En": en} for key, ru, en in abilities],
}
target = root / "UnityProject/Assets/Resources/EchoSphere/PlayerCopy.json"
target.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print("Player copy revisions:", len(result["Ru"]), "RU,", len(result["En"]), "EN;", len(abilities), "ability summaries")
