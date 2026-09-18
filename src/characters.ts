import type { AbilityType, SphereType } from './gameData';
import type { SphereMods } from './engine';

export type CharacterId =
  | 'spherist'
  | 'hunter'
  | 'engineer'
  | 'berserker'
  | 'alchemist'
  | 'architect';

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
  level: 1 | 2 | 3 | 4 | 5;
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

export const CHARACTER_DEFS: Record<CharacterId, CharacterDef> = {
  spherist: {
    id: 'spherist',
    name: { ru: 'Сферист', en: 'Spherist' },
    role: { ru: 'Универсал', en: 'All-rounder' },
    description: {
      ru: 'Получает дополнительные преимущества от большого количества активных сфер.',
      en: 'Gets additional benefits from having many active spheres.',
    },
    color: '#5a8c4a',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.05, sphereRadius: 0.05 },
    preferredSphereTypes: ['standard', 'chain'],
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
    color: '#c46d3d',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: 0.08, sphereRadius: 0.10, sphereAttackSpeed: -0.05 },
    preferredSphereTypes: ['sniper', 'chain'],
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
    color: '#4a7a8a',
    baseModifiers: { ...ZERO_MODIFIERS, maxHp: 0.10, sphereRadius: 0.05, moveSpeed: -0.05 },
    preferredSphereTypes: ['standard', 'aura'],
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
    color: '#c4453d',
    baseModifiers: { ...ZERO_MODIFIERS, maxHp: -0.10, moveSpeed: 0.08, sphereDamage: 0.08, sphereRadius: -0.05 },
    preferredSphereTypes: ['shotgun', 'standard'],
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
    ],
  },

  alchemist: {
    id: 'alchemist',
    name: { ru: 'Алхимик', en: 'Alchemist' },
    role: { ru: 'Статусы', en: 'Status Effects' },
    description: {
      ru: 'Соединяет Fire, Freeze и Poison в цепочки реакций.',
      en: 'Combines Fire, Freeze and Poison into reaction chains.',
    },
    color: '#8a5a8a',
    baseModifiers: { ...ZERO_MODIFIERS, sphereDamage: -0.05, statusDuration: 0.30, statusDamage: 0.15 },
    preferredSphereTypes: ['aura', 'chain'],
    preferredSphereMods: ['fire', 'freeze', 'poison'],
    preferredAbilities: ['firetrail', 'lightning', 'timestop'],
    mechanic: {
      ru: 'Реакции: Fire+Poison = Воспламенение токсинов; Freeze+Poison = Крио-токсин; Fire+Freeze = Термошок. Реакции должны быть мгновенными, но расходуют участвующие статусы.',
      en: 'Reactions: Fire+Poison = Toxin Ignition; Freeze+Poison = Cryotoxin; Fire+Freeze = Thermal Shock. Reactions are instant and consume the participating statuses.',
    },
    mastery: [
      { level: 1, title: { ru: 'Реакции', en: 'Reactions' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Катализатор', en: 'Catalyst' }, description: { ru: 'Радиус реакций увеличивается на 10%.', en: 'Reaction radius increases by 10%.' } },
      { level: 3, title: { ru: 'Концентрация', en: 'Concentration' }, description: { ru: '+5% урона DoT.', en: '+5% DoT damage.' } },
      { level: 4, title: { ru: 'Цепная реакция', en: 'Chain Reaction' }, description: { ru: 'Реакция может передать 50% своего burst-урона ещё одной цели рядом.', en: 'A reaction can transfer 50% of its burst damage to one nearby target.' } },
      { level: 5, title: { ru: 'Философский камень', en: 'Philosopher Stone' }, description: { ru: 'После реакции следующий наложенный статус длится на 50% дольше.', en: 'After a reaction, the next applied status lasts 50% longer.' } },
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
    color: '#d4943d',
    baseModifiers: { ...ZERO_MODIFIERS, sphereRadius: 0.05, moveSpeed: -0.05 },
    preferredSphereTypes: ['standard', 'aura', 'sniper'],
    preferredSphereMods: ['pierce', 'freeze'],
    preferredAbilities: ['teleport', 'timestop', 'shield'],
    mechanic: {
      ru: 'Форма: игра автоматически распознаёт Линию (3+ сферы), Треугольник (3), Квадрат (4) или Кластер (4+). Одновременно активна только одна форма.',
      en: 'Formation: the game automatically recognizes Line (3+ spheres), Triangle (3), Square (4), or Cluster (4+). Only one formation is active at a time.',
    },
    mastery: [
      { level: 1, title: { ru: 'Форма', en: 'Formation' }, description: { ru: 'Основная механика персонажа.', en: 'Core character mechanic.' } },
      { level: 2, title: { ru: 'Глаз конструктора', en: 'Builder Eye' }, description: { ru: 'Допуск распознавания формы увеличивается примерно на 15%.', en: 'Formation tolerance increases by roughly 15%.' } },
      { level: 3, title: { ru: 'Модульность', en: 'Modularity' }, description: { ru: '+2% радиуса сфер.', en: '+2% sphere radius.' } },
      { level: 4, title: { ru: 'Перестройка', en: 'Rebuild' }, description: { ru: 'После изменения формации новый бонус активируется на 2 секунды с +25% эффективности.', en: 'After changing formation, the new bonus is 25% stronger for 2 seconds.' } },
      { level: 5, title: { ru: 'Архитектурный шедевр', en: 'Masterwork' }, description: { ru: 'При 5+ сферах активная форма получает ещё +5% к своему ключевому бонусу.', en: 'At 5+ spheres, the active formation gains another +5% to its key bonus.' } },
    ],
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
