import type { AbilityType, SphereType } from './gameData';
import type { SphereMods } from './engine';

export type CharacterId =
  | 'spherist'
  | 'hunter'
  | 'engineer'
  | 'berserker'
  | 'alchemist'
  | 'architect'
  | 'conductor'
  | 'oracle'
  | 'voidwalker'
  | 'fractal';

export type CharacterStat =
  | 'sphereDamage'
  | 'sphereRadius'
  | 'sphereAttackSpeed'
  | 'moveSpeed'
  | 'maxHp'
  | 'statusDuration'
  | 'statusDamage'
  | 'damageTaken';

export interface CharacterBaseModifiers {
  sphereDamage: number;
  sphereRadius: number;
  sphereAttackSpeed: number;
  moveSpeed: number;
  maxHp: number;
  statusDuration: number;
  statusDamage: number;
  damageTaken: number;
}

export type CharacterFavoriteTowerMod = keyof SphereMods;

export interface CharacterMasteryLevel {
  level: 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10;
  title: { ru: string; en: string };
  description: { ru: string; en: string };
}

export interface CharacterDef {
  id: CharacterId;
  name: { ru: string; en: string };
  role: { ru: string; en: string };
  description: { ru: string; en: string };
  color: string;
  baseModifiers: CharacterBaseModifiers;
  signatureSphereType: SphereType;
  partnerSphereType: SphereType;
  preferredSphereTypes: SphereType[];
  preferredSphereMods: CharacterFavoriteTowerMod[];
  preferredAbilities: AbilityType[];
  mastery: CharacterMasteryLevel[];
  mechanic: {
    ru: string;
    en: string;
  };
}

const ZERO_MODIFIERS: CharacterBaseModifiers = {
  sphereDamage: 0,
  sphereRadius: 0,
  sphereAttackSpeed: 0,
  moveSpeed: 0,
  maxHp: 0,
  statusDuration: 0,
  statusDamage: 0,
  damageTaken: 0,
};


const makeMastery = (rows: Array<[string,string,string,string]>): CharacterMasteryLevel[] => rows.map(([ru,en,rd,ed],i)=>({level:(i+1) as CharacterMasteryLevel['level'],title:{ru,en},description:{ru:rd,en:ed}}));

export const CHARACTER_DEFS: Record<CharacterId, CharacterDef> = {
  spherist: {
    id: 'spherist',
    name: { ru: 'Сферист', en: 'Spherist' },
    role: { ru: 'Универсал', en: 'All-rounder' },
    description: {
      ru: 'Получает дополнительные преимущества от большого количества активных сфер.',
      en: 'Gets additional benefits from having many active spheres.',
    },
    color: '#55dfff',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.05, sphereRadius: 0.05 },
    signatureSphereType: 'standard',
    partnerSphereType: 'orbital',
    preferredSphereTypes: ['standard', 'orbital'],
    preferredSphereMods: ['multishot', 'ricochet'],
    preferredAbilities: ['blast', 'lightning', 'shield'],
    mechanic: {
      ru: 'Резонанс: +3% скорости атаки всех сфер за каждую сферу после первой. При 5+ сферах дополнительно +5% урона.',
      en: 'Resonance: +3% attack speed for all spheres for each sphere after the first. At 5+ spheres, gain an additional +5% damage.',
    },
    mastery: [
      { level: 1, title: { ru: 'Резонанс', en: 'Resonance' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Гармония', en: 'Harmony' }, description: { ru: 'Порог полного бонуса Резонанса снижается с 5 до 4 сфер.', en: 'Resonance full-bonus threshold is reduced from 5 to 4 spheres.' } },
      { level: 3, title: { ru: 'Стабильность', en: 'Stability' }, description: { ru: '+2% урона сфер.', en: '+2% sphere damage.' } },
      { level: 4, title: { ru: 'Резонанс+', en: 'Resonance+' }, description: { ru: 'Каждая сфера после первой даёт ещё +0.5% скорости атаки.', en: 'Each sphere after the first grants another +0.5% attack speed.' } },
      { level: 5, title: { ru: 'Хор сфер', en: 'Sphere Chorus' }, description: { ru: 'При 8 сферах: ещё +5% радиуса и +5% скорости атаки.', en: 'At 8 spheres: gain another +5% radius and +5% attack speed.' } },
      { level: 6, title: { ru: 'Созвучие', en: 'Accord' }, description: { ru: 'При 6+ сферах: ещё +2% урона.', en: 'At 6+ spheres: another +2% sphere damage.' } },
      { level: 7, title: { ru: 'Резонансный контур', en: 'Resonant Circuit' }, description: { ru: 'При 7+ сферах: ещё +2% скорости атаки.', en: 'At 7+ spheres: another +2% attack speed.' } },
      { level: 8, title: { ru: 'Глубокий резонанс', en: 'Deep Resonance' }, description: { ru: 'Полный бонус Резонанса начинается с 3 сфер.', en: 'Full Resonance bonus begins at 3 spheres.' } },
      { level: 9, title: { ru: 'Эхо-корона', en: 'Echo Crown' }, description: { ru: 'При 8+ сферах: ещё +3% радиуса.', en: 'At 8+ spheres: another +3% radius.' } },
      { level: 10, title: { ru: 'Хоровая синхронизация', en: 'Chorus Sync' }, description: { ru: 'При 8+ сферах все бонусы Сфериста получают финальное усиление.', en: 'At 8+ spheres, the Spherist signature bonuses gain a final amplification.' } },

    ],
  },

  hunter: {
    id: 'hunter',
    name: { ru: 'Охотник', en: 'Hunter' },
    role: { ru: 'Одиночная цель', en: 'Single Target' },
    description: {
      ru: 'Специалист по элитам и боссам: отмечает приоритетные цели и усиливает повторные попадания.',
      en: 'Specialist against elites and bosses: marks priority targets and rewards repeated hits.',
    },
    color: '#e86cff',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.08, sphereRadius: 0.10, sphereAttackSpeed: -0.05 },
    signatureSphereType: 'sniper',
    partnerSphereType: 'void',
    preferredSphereTypes: ['sniper', 'void'],
    preferredSphereMods: ['pierce', 'ricochet'],
    preferredAbilities: ['teleport', 'lightning', 'blast'],
    mechanic: {
      ru: 'Метка добычи: элиты и боссы получают Метку на 5 секунд. Сферы наносят отмеченной цели +20% урона; 5 попаданий подряд запускают Охоту ещё на 3 секунды (+30% урона от Sniper/Chain).',
      en: 'Prey Mark: elites and bosses are marked for 5 seconds. Spheres deal +20% damage to the marked target; 5 consecutive hits trigger Hunt for 3 more seconds (+30% Sniper/Chain damage).',
    },
    mastery: [
      { level: 1, title: { ru: 'Метка добычи', en: 'Prey Mark' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Следопыт', en: 'Tracker' }, description: { ru: 'Длительность метки увеличивается до 6 секунд.', en: 'Mark duration increases to 6 seconds.' } },
      { level: 3, title: { ru: 'Точный выстрел', en: 'True Shot' }, description: { ru: '+2% шанс крита против отмеченных целей.', en: '+2% crit chance against marked targets.' } },
      { level: 4, title: { ru: 'Натиск', en: 'Onslaught' }, description: { ru: 'Для запуска Охоты требуется 4 попадания вместо 5.', en: 'Hunt requires 4 consecutive hits instead of 5.' } },
      { level: 5, title: { ru: 'Трофей', en: 'Trophy' }, description: { ru: 'Убийство отмеченной элитной цели временно даёт +10% скорости движения на 4 секунды.', en: 'Killing a marked elite grants +10% move speed for 4 seconds.' } },
      { level: 6, title: { ru: 'Слабое место', en: 'Weak Point' }, description: { ru: 'Отмеченные цели получают ещё +5% урона.', en: 'Marked targets take another +5% damage.' } },
      { level: 7, title: { ru: 'Дальний след', en: 'Long Trail' }, description: { ru: 'Метка сохраняется дольше после повторного попадания.', en: 'Repeated hits extend the active Mark window.' } },
      { level: 8, title: { ru: 'Без пощады', en: 'No Mercy' }, description: { ru: 'Hunt дополнительно усиливает Sniper/Chain.', en: 'Hunt further amplifies Sniper/Chain damage.' } },
      { level: 9, title: { ru: 'Приоритет', en: 'Priority' }, description: { ru: 'Элитные и босс-цели получают дополнительный шанс критического удара.', en: 'Elite and Boss targets grant an additional critical-hit chance.' } },
      { level: 10, title: { ru: 'Мастер охоты', en: 'Master Hunter' }, description: { ru: 'Финальный Signature: Метка добычи и Охота получают максимальное усиление.', en: 'Final Signature: Prey Mark and Hunt reach their maximum enhancement.' } },

    ],
  },

  engineer: {
    id: 'engineer',
    name: { ru: 'Инженер', en: 'Engineer' },
    role: { ru: 'Сеть', en: 'Network' },
    description: {
      ru: 'Усиливает близко стоящие сферы и вознаграждает компактные боевые конструкции.',
      en: 'Empowers nearby spheres and rewards compact battlefield structures.',
    },
    color: '#57d5ff',
    baseModifiers: { ...ZERO_MODIFIERS, maxHp: 0.10, sphereRadius: 0.05, moveSpeed: -0.05 },
    signatureSphereType: 'chain',
    partnerSphereType: 'aura',
    preferredSphereTypes: ['chain', 'aura'],
    preferredSphereMods: ['freeze', 'multishot'],
    preferredAbilities: ['shield', 'minion', 'lightning'],
    mechanic: {
      ru: 'Связь: сфера получает +6% урона за каждого соседнего союзника в пределах 220 px, максимум 2 соседа. Сеть из 3+ связанных сфер получает +8% дальности. Попадание одной связанной сферы открывает 0.4-секундное окно ретрансляции для другой.',
      en: 'Link: a sphere gains +6% damage for each allied sphere within 220 px, up to 2 neighbours. A network of 3+ linked spheres gains +8% range. A hit by one linked sphere opens a 0.4-second relay window for another linked sphere.',
    },
    mastery: [
      { level: 1, title: { ru: 'Связь', en: 'Link' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Проводник', en: 'Conductor' }, description: { ru: 'Радиус связи увеличивается с 220 до 240 px.', en: 'Link radius increases from 220 to 240 px.' } },
      { level: 3, title: { ru: 'Узел', en: 'Node' }, description: { ru: '+3% урона сети, если в ней 4+ сферы.', en: '+3% network damage when the network contains 4+ spheres.' } },
      { level: 4, title: { ru: 'Быстрая передача', en: 'Fast Relay' }, description: { ru: 'Окно ретрансляции увеличивается до 0.55 секунды.', en: 'Relay window increases to 0.55 seconds.' } },
      { level: 5, title: { ru: 'Главный узел', en: 'Master Node' }, description: { ru: 'Сфера с 2 соседями даёт им ещё +2% урона.', en: 'A sphere with 2 neighbours grants them another +2% damage.' } },
      { level: 6, title: { ru: 'Широкая сеть', en: 'Wide Network' }, description: { ru: 'Радиус инженерной связи увеличивается ещё на 20 px.', en: 'Engineer link radius increases by another 20 px.' } },
      { level: 7, title: { ru: 'Точка опоры', en: 'Anchor Point' }, description: { ru: 'Сеть из 4+ сфер получает ещё +3% урона.', en: 'A 4+ sphere network gains another +3% damage.' } },
      { level: 8, title: { ru: 'Ретранслятор', en: 'Repeater' }, description: { ru: 'Окно ретрансляции становится ещё стабильнее.', en: 'The relay window becomes more consistent.' } },
      { level: 9, title: { ru: 'Магистраль', en: 'Mainline' }, description: { ru: 'Сфера с двумя соседями усиливает их дополнительным бонусом.', en: 'A two-neighbour Sphere grants an additional relay bonus.' } },
      { level: 10, title: { ru: 'Главный конструктор', en: 'Master Engineer' }, description: { ru: 'Финальный Signature: сильный связанный узел передаёт усиление соседям.', en: 'Final Signature: a strongly connected master node transfers amplified power.' } },

    ],
  },

  berserker: {
    id: 'berserker',
    name: { ru: 'Берсерк', en: 'Berserker' },
    role: { ru: 'Риск / ближний бой', en: 'Risk / Close Range' },
    description: {
      ru: 'Чем опаснее положение игрока, тем сильнее становится его боевая сеть.',
      en: 'The more dangerous the situation, the stronger the combat network becomes.',
    },
    color: '#ff625d',
    baseModifiers: { ...ZERO_MODIFIERS, maxHp: -0.10, moveSpeed: 0.08, sphereDamage: 0.08, sphereRadius: -0.05 },
    signatureSphereType: 'shotgun',
    partnerSphereType: 'void',
    preferredSphereTypes: ['shotgun', 'void'],
    preferredSphereMods: ['multishot', 'fire'],
    preferredAbilities: ['shield', 'darkritual', 'firetrail'],
    mechanic: {
      ru: 'Ярость: каждые потерянные 20% HP дают +7% урона и +4% скорости атаки, максимум +28%/+16%. Сферы получают ещё +12% урона по врагам в радиусе 110 px от игрока.',
      en: 'Fury: every missing 20% HP grants +7% damage and +4% attack speed, up to +28%/+16%. Spheres also deal +12% damage to enemies within 110 px of the player.',
    },
    mastery: [
      { level: 1, title: { ru: 'Ярость', en: 'Fury' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Жажда боя', en: 'Bloodlust' }, description: { ru: 'Ближний бонус увеличивается с +12% до +15%.', en: 'Close-range bonus increases from +12% to +15%.' } },
      { level: 3, title: { ru: 'Бешенство', en: 'Frenzy' }, description: { ru: '+2% базовой скорости атаки сфер.', en: '+2% base sphere attack speed.' } },
      { level: 4, title: { ru: 'На грани', en: 'On the Edge' }, description: { ru: 'Максимальный бонус Ярости активируется уже при 80% потерянного HP.', en: 'Maximum Fury activates at 80% missing HP instead of 100%.' } },
      { level: 5, title: { ru: 'Кровавый след', en: 'Blood Trail' }, description: { ru: 'После убийства врага в ближнем радиусе игрок получает +5% скорости на 2 секунды.', en: 'Killing an enemy in close range grants +5% move speed for 2 seconds.' } },
      { level: 6, title: { ru: 'Железная воля', en: 'Iron Will' }, description: { ru: 'При низком HP бонус ближнего боя усиливается.', en: 'At low HP, the close-range bonus is stronger.' } },
      { level: 7, title: { ru: 'Красная зона', en: 'Red Zone' }, description: { ru: 'Максимальный бонус Ярости держится дольше в опасной зоне.', en: 'Maximum Fury remains stronger in the danger zone.' } },
      { level: 8, title: { ru: 'Без тормозов', en: 'No Brakes' }, description: { ru: 'Ярость влияет на скорость атаки ещё сильнее.', en: 'Fury affects attack speed even more strongly.' } },
      { level: 9, title: { ru: 'Кровь за кровь', en: 'Blood for Blood' }, description: { ru: 'Убийства рядом с игроком дают более сильный временный импульс скорости.', en: 'Close-range kills grant a stronger temporary speed burst.' } },
      { level: 10, title: { ru: 'Воплощение ярости', en: 'Avatar of Fury' }, description: { ru: 'Финальный Signature: критически низкое HP открывает максимальную боевую ярость.', en: 'Final Signature: critically low HP unlocks the full Fury signature.' } },

    ],
  },

  alchemist: {
    id: 'alchemist',
    name: { ru: 'Алхимик', en: 'Alchemist' },
    role: { ru: 'Статусы', en: 'Status Effects' },
    description: {
      ru: 'Строит сеть из одноэлементных сфер и соединяет Fire, Freeze и Poison в реакции.',
      en: 'Builds a network of single-element Spheres and combines Fire, Freeze and Poison into reactions.',
    },
    color: '#57e6b4',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: -0.05, statusDuration: 0.30, statusDamage: 0.15 },
    signatureSphereType: 'aura',
    partnerSphereType: 'chain',
    preferredSphereTypes: ['aura', 'chain'],
    preferredSphereMods: ['fire', 'freeze', 'poison'],
    preferredAbilities: ['firetrail', 'lightning', 'timestop'],
    mechanic: {
      ru: 'Реакции: Fire+Poison = Воспламенение токсинов; Freeze+Poison = Крио-токсин; Fire+Freeze = Термошок. Одна сфера остаётся одноэлементной, реакция возникает между статусами разных сфер и расходует их.',
      en: 'Reactions: Fire+Poison = Toxin Ignition; Freeze+Poison = Cryotoxin; Fire+Freeze = Thermal Shock. Each Sphere remains single-element; reactions occur between statuses from different sources and consume them.',
    },
    mastery: [
      { level: 1, title: { ru: 'Реакции', en: 'Reactions' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Катализатор', en: 'Catalyst' }, description: { ru: 'Радиус реакций увеличивается на 10%.', en: 'Reaction radius increases by 10%.' } },
      { level: 3, title: { ru: 'Концентрация', en: 'Concentration' }, description: { ru: '+5% урона DoT.', en: '+5% DoT damage.' } },
      { level: 4, title: { ru: 'Цепная реакция', en: 'Chain Reaction' }, description: { ru: 'Реакция может передать 50% своего burst-урона ещё одной цели рядом.', en: 'A reaction can transfer 50% of its burst damage to one nearby target.' } },
      { level: 5, title: { ru: 'Философский камень', en: 'Philosopher Stone' }, description: { ru: 'После реакции следующий наложенный статус длится на 50% дольше.', en: 'After a reaction, the next applied status lasts 50% longer.' } },
      { level: 6, title: { ru: 'Катализ', en: 'Catalysis' }, description: { ru: 'Радиус реакций увеличивается ещё на 10%.', en: 'Reaction radius increases by another 10%.' } },
      { level: 7, title: { ru: 'Чистая формула', en: 'Pure Formula' }, description: { ru: '+5% урона реакций.', en: '+5% reaction damage.' } },
      { level: 8, title: { ru: 'Цепь катализаторов', en: 'Catalyst Chain' }, description: { ru: 'Передача burst-урона реакции становится сильнее.', en: 'Reaction burst transfer becomes stronger.' } },
      { level: 9, title: { ru: 'Тройная смесь', en: 'Triple Mixture' }, description: { ru: 'Третья последовательная реакция получает дополнительный импульс.', en: 'Every third sequential reaction gains an additional pulse.' } },
      { level: 10, title: { ru: 'Алхимический круг', en: 'Alchemical Circle' }, description: { ru: 'Финальный Signature: реакционные цепочки получают максимальное усиление.', en: 'Final Signature: reaction chains reach their maximum enhancement.' } },

    ],
  },

  architect: {
    id: 'architect',
    name: { ru: 'Архитектор', en: 'Architect' },
    role: { ru: 'Геометрия', en: 'Geometry' },
    description: {
      ru: 'Распознаёт форму локального расположения сфер и превращает её в боевой бонус.',
      en: 'Recognizes the shape of nearby sphere placement and turns it into a combat bonus.',
    },
    color: '#ffc56a',
    baseModifiers: { ...ZERO_MODIFIERS, sphereRadius: 0.05, moveSpeed: -0.05 },
    signatureSphereType: 'gravity',
    partnerSphereType: 'prism',
    preferredSphereTypes: ['gravity', 'prism'],
    preferredSphereMods: ['pierce', 'freeze'],
    preferredAbilities: ['teleport', 'timestop', 'shield'],
    mechanic: {
      ru: 'Форма: Архитектор использует авторитетную Geometry-сеть. Dominant форма определяет бонус, а Secondary остаётся дополнительным активным слоем.',
      en: 'Formation: Architect uses the authoritative Geometry network. The Dominant formation determines the character bonus while Secondary remains an active secondary layer.',
    },
    mastery: [
      { level: 1, title: { ru: 'Форма', en: 'Formation' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Глаз конструктора', en: 'Builder Eye' }, description: { ru: 'Допуск распознавания формы увеличивается примерно на 15%.', en: 'Formation tolerance increases by roughly 15%.' } },
      { level: 3, title: { ru: 'Модульность', en: 'Modularity' }, description: { ru: '+2% радиуса сфер.', en: '+2% sphere radius.' } },
      { level: 4, title: { ru: 'Перестройка', en: 'Rebuild' }, description: { ru: 'После изменения формации новый бонус активируется на 2 секунды с +25% эффективности.', en: 'After changing formation, the new bonus is 25% stronger for 2 seconds.' } },
      { level: 5, title: { ru: 'Архитектурный шедевр', en: 'Masterwork' }, description: { ru: 'При 5+ сферах активная форма получает ещё +5% к своему ключевому бонусу.', en: 'At 5+ spheres, the active formation gains another +5% to its key bonus.' } },
      { level: 6, title: { ru: 'Точная разметка', en: 'Precise Layout' }, description: { ru: 'Допуск распознавания формации увеличивается ещё на 10%.', en: 'Formation recognition tolerance increases by another 10%.' } },
      { level: 7, title: { ru: 'Ритм конструкции', en: 'Construction Rhythm' }, description: { ru: 'Ключевой бонус формации усиливается на 5%.', en: 'Formation key bonuses gain another 5% effectiveness.' } },
      { level: 8, title: { ru: 'Перекройка', en: 'Redesign' }, description: { ru: 'Смена формации даёт более сильный краткий импульс.', en: 'Changing formation grants a stronger brief power spike.' } },
      { level: 9, title: { ru: 'Многослойность', en: 'Layering' }, description: { ru: 'При 5+ локальных сферах геометрический бонус усиливается.', en: 'With 5+ local spheres, geometry bonuses are amplified.' } },
      { level: 10, title: { ru: 'Великий архитектор', en: 'Grand Architect' }, description: { ru: 'Финальный Signature: смена формы превращается в мощный тактический импульс.', en: 'Final Signature: formation changes become a powerful tactical surge.' } },

    ],
  },
  conductor: {
    id: 'conductor', name: { ru: 'Проводник', en: 'Conductor' }, role: { ru: 'Разряд Резонанса', en: 'Resonance Discharge' },
    description: { ru: 'Превращает Resonance Events в короткие Overdrive и Cascade.', en: 'Turns Resonance Events into short Overdrive and Cascade windows.' }, color: '#39d8ff',
    baseModifiers: { ...ZERO_MODIFIERS, sphereAttackSpeed: 0.03 },
    signatureSphereType: 'pulse', partnerSphereType: 'chain', preferredSphereTypes: ['pulse', 'chain'],
    preferredSphereMods: ['resonant', 'echo'], preferredAbilities: ['blast', 'lightning', 'resonance'],
    mechanic: { ru: 'Каждый Resonance Event открывает короткий Overdrive.', en: 'Each Resonance Event opens a short Overdrive.' }, mastery: makeMastery([["Разряд","Discharge","Resonance Event открывает Overdrive.","A Resonance Event opens Overdrive."],["Синхронизация","Sync","Overdrive длится дольше.","Overdrive lasts longer."],["Резонансный провод","Resonant Wire","Следующий Pulse/Chain получает Echo.","Next Pulse/Chain gains Echo."],["Чистый ток","Pure Current","Overdrive усиливается.","Overdrive is stronger."],["Третий узел","Third Node","Открывается 3-я Signature-сфера.","Unlocks the 3rd Signature Sphere."],["Cascade","Cascade","Каждый третий разряд выпускает вторичный разряд.","Every third discharge emits a secondary discharge."],["Проводимость","Conductivity","Cascade получает дополнительный прыжок.","Cascade gains an extra relay jump."],["Перегрузка","Overdrive","Overdrive усиливается без роста Resonance gain.","Overdrive strengthens without increasing Resonance gain."],["Резонансный хор","Resonant Choir","Cascade получает ещё один безопасный прыжок.","Cascade gains another safe jump."],["Абсолютный разряд","Absolute Discharge","Открывается 4-я Signature-сфера.","Unlocks the 4th Signature Sphere."]]),
  },

  oracle: {
    id: 'oracle', name: { ru: 'Оракул', en: 'Oracle' }, role: { ru: 'Предвидение', en: 'Forecast' },
    description: { ru: 'Читает реальные Level-Up источники и улучшает Lock/Reroll без гарантии RNG.', en: 'Reads real Level-Up sources and improves Lock/Reroll without guaranteeing RNG.' }, color: '#b68cff',
    baseModifiers: { ...ZERO_MODIFIERS, sphereRadius: 0.04 },
    signatureSphereType: 'prism', partnerSphereType: 'pulse', preferredSphereTypes: ['prism', 'pulse'],
    preferredSphereMods: ['ricochet', 'resonant'], preferredAbilities: ['teleport', 'timestop', 'crit'],
    mechanic: { ru: 'Перед Level-Up отмечает наиболее вероятные живые карты.', en: 'Before Level-Up, marks the most likely live cards.' }, mastery: makeMastery([["Прогноз","Forecast","Отмечает вероятную живую карту.","Marks a likely live card."],["Чтение потока","Flow Reading","Учитывает давление незавершённых систем.","Accounts for unfinished-system pressure."],["Знак ветви","Branch Sign","Учитывает выбранную ветку Sphere.","Accounts for the selected Sphere branch."],["Двойное зрение","Double Sight","Показывает два варианта.","Shows two options."],["Третий осколок","Third Shard","Открывается 3-я Signature-сфера.","Unlocks the 3rd Signature Sphere."],["Предсказанный Lock","Foreseen Lock","Один Reroll может сохранить прогноз.","One Reroll can preserve the forecast."],["Взгляд за грань","Beyond Sight","Прогнозирует источник следующего выбора.","Forecasts the next choice source."],["Точное предвидение","True Foresight","Прогноз стабильнее при Lock/Reroll.","Forecast is more stable around Lock/Reroll."],["Картина билда","Build Pattern","Показывает пару поддерживающих карт.","Shows a supporting pair of cards."],["Абсолютное зрение","Absolute Sight","Открывается 4-я Signature-сфера.","Unlocks the 4th Signature Sphere."]]),
  },

  voidwalker: {
    id: 'voidwalker', name: { ru: 'Пустотник', en: 'Voidwalker' }, role: { ru: 'Разрыв сети', en: 'Network Breach' },
    description: { ru: 'Превращает временные разрывы Network в слабые Phantom Nodes.', en: 'Turns temporary Network breaks into weaker Phantom Nodes.' }, color: '#8a5a8a',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.04, damageTaken: 0.03 },
    signatureSphereType: 'void', partnerSphereType: 'gravity', preferredSphereTypes: ['void', 'gravity'],
    preferredSphereMods: ['corrupt', 'drain'], preferredAbilities: ['teleport', 'darkritual', 'shield'],
    mechanic: { ru: 'При сетевом разрыве остаётся Phantom Node.', en: 'A Network break leaves a Phantom Node.' }, mastery: makeMastery([["Фантомный узел","Phantom Node","Сетевой разрыв создаёт слабый фантом.","A Network break creates a weak phantom."],["След пустоты","Void Trace","Фантом живёт дольше.","Phantom lasts longer."],["Разрыв","Breach","Фантом замедляет врагов.","Phantom slows enemies."],["Тонкая грань","Thin Edge","Фантом наносит 60% урона.","Phantom deals 60% damage."],["Третий силуэт","Third Silhouette","Открывается 3-я Signature-сфера.","Unlocks the 3rd Signature Sphere."],["Двойной след","Double Trace","Новый разрыв обновляет фантом.","A new break refreshes the phantom."],["Хищная пустота","Predatory Void","Фантом сильнее бьёт ослабленные цели.","Phantom hits weakened targets harder."],["Провал связи","Link Collapse","Void/Gravity лучше синхронизируются.","Void/Gravity synchronize better."],["Отголосок разрыва","Breach Echo","Первый импульс усилен.","First pulse is empowered."],["Аватар пустоты","Void Avatar","Открывается 4-я Signature-сфера.","Unlocks the 4th Signature Sphere."]]),
  },

  fractal: {
    id: 'fractal', name: { ru: 'Фрактал', en: 'Fractal' }, role: { ru: 'Последовательность', en: 'Sequence' },
    description: { ru: 'Запоминает последовательность реальных Geometry и возвращает прошлую форму как Echo.', en: 'Remembers real Geometry sequences and returns a prior form as an Echo.' }, color: '#ffb84d',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.03 },
    signatureSphereType: 'orbital', partnerSphereType: 'aura', preferredSphereTypes: ['orbital', 'aura'],
    preferredSphereMods: ['echo', 'gravitic'], preferredAbilities: ['blast', 'timestop', 'minion'],
    mechanic: { ru: 'Три разные Dominant-формации подряд запускают Recursive Echo.', en: 'Three different Dominant formations trigger Recursive Echo.' }, mastery: makeMastery([["Память формы","Shape Memory","Запоминает Dominant-формации.","Remembers Dominant formations."],["Глубокая память","Deep Memory","История хранит четыре шага.","History keeps four steps."],["Рекурсия","Recursion","Три разные формации запускают Echo.","Three different formations trigger Echo."],["Устойчивый Echo","Stable Echo","Echo длится 4 секунды.","Echo lasts 4 seconds."],["Третий виток","Third Loop","Открывается 3-я Signature-сфера.","Unlocks the 3rd Signature Sphere."],["Сила Echo","Echo Strength","Echo достигает 60%.","Echo reaches 60%."],["Четвёртый шаг","Fourth Step","История хранит четыре разные формы.","History can hold four different forms."],["Рекурсивная волна","Recursive Wave","Echo получает дополнительный импульс.","Echo gains an extra pulse."],["Самоподобие","Self Similarity","Следующая уникальная форма ускоряет новую цепочку.","Next unique form advances a new chain."],["Рекурсивный мастер","Recursive Master","Открывается 4-я Signature-сфера и 70% Echo.","Unlocks the 4th Signature Sphere and 70% Echo."]]),
  },

};

export const CHARACTER_LIST = Object.values(CHARACTER_DEFS);

export const DEFAULT_CHARACTER_ID: CharacterId = 'spherist';

export const CHARACTER_UNLOCK_COST: Record<CharacterId, number> = {
  spherist: 0,
  hunter: 1500,
  engineer: 2000,
  berserker: 2000,
  alchemist: 2500,
  architect: 3000,
  conductor: 3500,
  oracle: 4000,
  voidwalker: 4500,
  fractal: 5000,
};

export interface CharacterProfile {
  id: CharacterId;
  masteryLevel: number;
  unlocked: boolean;
}

export function createDefaultCharacterProfiles(): CharacterProfile[] {
  return CHARACTER_LIST.map((character) => ({
    id: character.id,
    masteryLevel: 1,
    unlocked: character.id === DEFAULT_CHARACTER_ID,
  }));
}

export function getCharacterDef(id: CharacterId): CharacterDef {
  return CHARACTER_DEFS[id];
}

export function getSphereCountResonanceBonus(character: CharacterId, sphereCount: number): number {
  if (character !== 'spherist') return 0;
  return Math.max(0, sphereCount - 1) * 0.03;
}

export function getSpheristFiveSphereBonus(character: CharacterId, sphereCount: number): number {
  return character === 'spherist' && sphereCount >= 5 ? 0.05 : 0;
}

export function getCharacterSphereCopyCap(character: CharacterId, sphereType: SphereType, masteryLevel: number): number {
  const def = CHARACTER_DEFS[character];
  if (sphereType !== def.signatureSphereType) return 2;
  if (masteryLevel >= 10) return 4;
  if (masteryLevel >= 5) return 3;
  return 2;
}
export function getSphereAffinityWeight(character: CharacterId, sphereType: SphereType): number {
  const def = CHARACTER_DEFS[character];
  if (sphereType === def.signatureSphereType) return 0.70;
  if (sphereType === def.partnerSphereType) return 0.30;
  return 0;
}
export function getBerserkerFuryBonus(character: CharacterId, hpRatio: number): { damage: number; attackSpeed: number } {
  if (character !== 'berserker') return { damage: 0, attackSpeed: 0 };
  const missingRatio = Math.max(0, Math.min(1, 1 - hpRatio));
  const steps = Math.min(4, Math.floor(missingRatio / 0.2));
  return { damage: steps * 0.07, attackSpeed: steps * 0.04 };
}

export function isPreferredSphere(character: CharacterId, sphereType: SphereType): boolean {
  return CHARACTER_DEFS[character].preferredSphereTypes.includes(sphereType);
}

export function isPreferredTowerMod(character: CharacterId, mod: CharacterFavoriteTowerMod): boolean {
  return CHARACTER_DEFS[character].preferredSphereMods.includes(mod);
}
