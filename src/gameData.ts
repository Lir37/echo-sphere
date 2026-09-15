import type { Lang } from './i18n';

export type AbilityType = 'radius' | 'damage' | 'attackspeed' | 'maxspheres' | 'blast' | 'shield' | 'teleport' | 'movespeed' | 'slow' | 'vitality' | 'firetrail' | 'minion' | 'vampire' | 'lightning' | 'dodge' | 'crit' | 'timestop' | 'magnet' | 'sphereboost' | 'darkritual';
export type AbilityCategory = 'active' | 'passive';

export interface AbilityDef {
  id: AbilityType;
  category: AbilityCategory;
  maxLevel: number;
  key?: string; // hotkey for active
  name: { ru: string; en: string };
  desc: { ru: (lvl: number) => string; en: (lvl: number) => string };
}

export const ABILITIES: Record<AbilityType, AbilityDef> = {
  radius: { id: 'radius', category: 'passive', maxLevel: 5, name: { ru: 'Радиус сфер', en: 'Sphere Radius' }, desc: { ru: () => '+15% радиуса', en: () => '+15% radius' } },
  damage: { id: 'damage', category: 'passive', maxLevel: 5, name: { ru: 'Урон сфер', en: 'Sphere Damage' }, desc: { ru: () => '+20% урона', en: () => '+20% damage' } },
  attackspeed: { id: 'attackspeed', category: 'passive', maxLevel: 5, name: { ru: 'Скорость атаки', en: 'Attack Speed' }, desc: { ru: () => '-10% задержки', en: () => '-10% delay' } },
  maxspheres: { id: 'maxspheres', category: 'passive', maxLevel: 3, name: { ru: '+1 Сфера', en: '+1 Sphere' }, desc: { ru: () => '+1 макс. сфер', en: () => '+1 max spheres' } },
  blast: { id: 'blast', category: 'active', maxLevel: 5, key: 'e', name: { ru: 'Взрывная волна', en: 'Blast Wave' }, desc: { ru: (l) => `Урон по радиусу 200px, КД ${30 - (l - 1) * 2}с`, en: (l) => `Damage in 200px, CD ${30 - (l - 1) * 2}s` } },
  shield: { id: 'shield', category: 'active', maxLevel: 5, key: 'q', name: { ru: 'Щит', en: 'Shield' }, desc: { ru: (l) => `Поглощает ${1 + Math.floor((l - 1) / 2)} ударов, КД 20с`, en: (l) => `Absorbs ${1 + Math.floor((l - 1) / 2)} hits, CD 20s` } },
  teleport: { id: 'teleport', category: 'active', maxLevel: 5, key: 'r', name: { ru: 'Телепорт', en: 'Teleport' }, desc: { ru: (l) => `Прыжок в случайную точку, КД ${15 - (l - 1) * 2}с`, en: (l) => `Jump to random spot, CD ${15 - (l - 1) * 2}s` } },
  movespeed: { id: 'movespeed', category: 'passive', maxLevel: 5, name: { ru: 'Скорость движения', en: 'Move Speed' }, desc: { ru: () => '+10% скорости', en: () => '+10% speed' } },
  slow: { id: 'slow', category: 'passive', maxLevel: 5, name: { ru: 'Замедление врагов', en: 'Enemy Slow' }, desc: { ru: (l) => `-${10 + (l - 1) * 5}% скорости врагов`, en: (l) => `-${10 + (l - 1) * 5}% enemy speed` } },
  vitality: { id: 'vitality', category: 'passive', maxLevel: 5, name: { ru: 'Живучесть', en: 'Vitality' }, desc: { ru: () => '+20 макс. HP', en: () => '+20 max HP' } },
  firetrail: { id: 'firetrail', category: 'active', maxLevel: 5, key: 'f', name: { ru: 'Огненный след', en: 'Fire Trail' }, desc: { ru: (l) => `След ${5 + (l - 1)}с, КД 25с`, en: (l) => `Trail ${5 + (l - 1)}s, CD 25s` } },
  minion: { id: 'minion', category: 'active', maxLevel: 5, key: 'g', name: { ru: 'Призыв миньона', en: 'Summon Minion' }, desc: { ru: (l) => `${1 + Math.floor((l - 1) / 2)} миньон(а), 10с, КД 30с`, en: (l) => `${1 + Math.floor((l - 1) / 2)} minion(s), 10s, CD 30s` } },
  vampire: { id: 'vampire', category: 'passive', maxLevel: 5, name: { ru: 'Вампиризм', en: 'Vampirism' }, desc: { ru: (l) => `${l * 3}% урона -> HP`, en: (l) => `${l * 3}% damage -> HP` } },
  lightning: { id: 'lightning', category: 'active', maxLevel: 5, key: 'e', name: { ru: 'Молния', en: 'Lightning' }, desc: { ru: (l) => `${1 + Math.floor((l - 1) / 2)} цель, КД 20с`, en: (l) => `${1 + Math.floor((l - 1) / 2)} target(s), CD 20s` } },
  dodge: { id: 'dodge', category: 'passive', maxLevel: 5, name: { ru: 'Уклонение', en: 'Dodge' }, desc: { ru: (l) => `${10 + (l - 1) * 5}% шанс`, en: (l) => `${10 + (l - 1) * 5}% chance` } },
  crit: { id: 'crit', category: 'passive', maxLevel: 5, name: { ru: 'Критический урон', en: 'Critical Hit' }, desc: { ru: (l) => `${10 + (l - 1) * 5}% шанс x2`, en: (l) => `${10 + (l - 1) * 5}% chance x2` } },
  timestop: { id: 'timestop', category: 'active', maxLevel: 5, key: 'q', name: { ru: 'Временная остановка', en: 'Time Stop' }, desc: { ru: (l) => `Заморозка ${3 + (l - 1)}с, КД 40с`, en: (l) => `Freeze ${3 + (l - 1)}s, CD 40s` } },
  magnet: { id: 'magnet', category: 'passive', maxLevel: 5, name: { ru: 'Магнит опыта', en: 'XP Magnet' }, desc: { ru: () => '+20% радиус подбора', en: () => '+20% pickup radius' } },
  sphereboost: { id: 'sphereboost', category: 'passive', maxLevel: 5, name: { ru: 'Усиление сфер', en: 'Sphere Boost' }, desc: { ru: (l) => `+1 урон за ${Math.max(50, 100 - (l - 1) * 10)} убийств`, en: (l) => `+1 dmg per ${Math.max(50, 100 - (l - 1) * 10)} kills` } },
  darkritual: { id: 'darkritual', category: 'active', maxLevel: 5, key: 'r', name: { ru: 'Тёмный ритуал', en: 'Dark Ritual' }, desc: { ru: (l) => `-20% HP, урон по 500px, КД 30с`, en: (l) => `-20% HP, damage in 500px, CD 30s` } },
};

// Active ability hotkey assignment (order of acquisition). Keys: e, q, r, f, g
export const ACTIVE_KEYS = ['e', 'q', 'r', 'f', 'g'] as const;

export interface EvolutionDef {
  id: string;
  a: AbilityType;
  b: AbilityType;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const EVOLUTIONS: EvolutionDef[] = [
  { id: 'supernova', a: 'radius', b: 'damage', name: { ru: 'Сверхновая', en: 'Supernova' }, desc: { ru: 'Сферы взрываются при убийстве врага', en: 'Spheres explode on enemy kill' } },
  { id: 'barrier', a: 'blast', b: 'shield', name: { ru: 'Барьер', en: 'Barrier' }, desc: { ru: 'Взрывная волна создаёт щит на 5с', en: 'Blast creates a shield for 5s' } },
  { id: 'blink', a: 'teleport', b: 'movespeed', name: { ru: 'Мгновенное перемещение', en: 'Blink' }, desc: { ru: 'Телепорт без КД, -10% HP за use', en: 'No-CD teleport, -10% HP per use' } },
  { id: 'icepath', a: 'firetrail', b: 'slow', name: { ru: 'Ледяной плен', en: 'Ice Path' }, desc: { ru: 'След замораживает врагов на 2с', en: 'Trail freezes enemies for 2s' } },
  { id: 'devourers', a: 'minion', b: 'vampire', name: { ru: 'Пожиратели', en: 'Devourers' }, desc: { ru: 'Миньоны восстанавливают HP за убийство', en: 'Minions heal you on kill' } },
  { id: 'thunderstorm', a: 'lightning', b: 'crit', name: { ru: 'Громовой шторм', en: 'Thunderstorm' }, desc: { ru: 'Молния бьёт по всем врагам', en: 'Lightning hits all enemies' } },
  { id: 'invulnerability', a: 'dodge', b: 'vitality', name: { ru: 'Неуязвимость', en: 'Invulnerability' }, desc: { ru: 'При HP<20% неуязвимость 3с (1 раз)', en: 'At HP<20% invulnerable 3s (once)' } },
  { id: 'echoaccumulator', a: 'magnet', b: 'sphereboost', name: { ru: 'Эхо-накопитель', en: 'Echo Accumulator' }, desc: { ru: 'Сферы поглощают опыт, +урон постоянно', en: 'Spheres absorb XP, +damage permanently' } },
];

export const EVOLUTION_MAP: Record<string, EvolutionDef> = Object.fromEntries(EVOLUTIONS.map(e => [e.id, e]));
export const EVOLUTION_BY_PAIR: Record<string, string> = {};
for (const e of EVOLUTIONS) {
  EVOLUTION_BY_PAIR[`${e.a}+${e.b}`] = e.id;
  EVOLUTION_BY_PAIR[`${e.b}+${e.a}`] = e.id;
}

export type ArtifactId = 'crystal_speed' | 'amulet_hp' | 'ring_xp' | 'regen_stone' | 'radius_shard' | 'mirror' | 'vampire_ring' | 'swift_boots' | 'luck_talisman' | 'freeze_amulet' | 'mage_pendant' | 'defense_medallion' | 'chaos_orb' | 'foresight_eye' | 'dragon_heart' | 'predator_claw' | 'invisibility_cloak';

export interface ArtifactDef {
  id: ArtifactId;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const ARTIFACTS: ArtifactDef[] = [
  { id: 'crystal_speed', name: { ru: 'Кристалл скорости', en: 'Crystal of Speed' }, desc: { ru: '+15% скорость движения', en: '+15% move speed' } },
  { id: 'amulet_hp', name: { ru: 'Амулет здоровья', en: 'Health Amulet' }, desc: { ru: '+30 макс. HP', en: '+30 max HP' } },
  { id: 'ring_xp', name: { ru: 'Кольцо опыта', en: 'XP Ring' }, desc: { ru: '+20% получаемого опыта', en: '+20% XP gained' } },
  { id: 'regen_stone', name: { ru: 'Камень восстановления', en: 'Regen Stone' }, desc: { ru: '+1 HP/сек', en: '+1 HP/sec' } },
  { id: 'radius_shard', name: { ru: 'Осколок радиуса', en: 'Radius Shard' }, desc: { ru: '+10% радиус всех сфер', en: '+10% sphere radius' } },
  { id: 'mirror', name: { ru: 'Зеркало', en: 'Mirror' }, desc: { ru: '20% шанс отразить урон', en: '20% chance to reflect damage' } },
  { id: 'vampire_ring', name: { ru: 'Кольцо вампира', en: 'Vampire Ring' }, desc: { ru: '+5 HP за убийство', en: '+5 HP per kill' } },
  { id: 'swift_boots', name: { ru: 'Ботинки скорости', en: 'Swift Boots' }, desc: { ru: '+10% скорости 3с после убийства', en: '+10% speed 3s after kill' } },
  { id: 'luck_talisman', name: { ru: 'Талисман удачи', en: 'Luck Talisman' }, desc: { ru: '+15% шанс крита', en: '+15% crit chance' } },
  { id: 'freeze_amulet', name: { ru: 'Амулет заморозки', en: 'Freeze Amulet' }, desc: { ru: 'При уроне замедляет всех 2с', en: 'On hit, slows all enemies 2s' } },
  { id: 'mage_pendant', name: { ru: 'Кулон мага', en: 'Mage Pendant' }, desc: { ru: '-10% перезарядки способностей', en: '-10% ability cooldowns' } },
  { id: 'defense_medallion', name: { ru: 'Медальон защиты', en: 'Defense Medallion' }, desc: { ru: '-15% получаемого урона', en: '-15% damage taken' } },
  { id: 'chaos_orb', name: { ru: 'Сфера хаоса', en: 'Chaos Orb' }, desc: { ru: 'Каждые 10с +20% урон/радиус 3с', en: 'Every 10s +20% dmg/radius 3s' } },
  { id: 'foresight_eye', name: { ru: 'Око предвидения', en: 'Eye of Foresight' }, desc: { ru: 'Стрелка к ближайшему боссу', en: 'Arrow to nearest boss' } },
  { id: 'dragon_heart', name: { ru: 'Сердце дракона', en: 'Dragon Heart' }, desc: { ru: '+50 HP, -10% скорости', en: '+50 HP, -10% speed' } },
  { id: 'predator_claw', name: { ru: 'Коготь хищника', en: 'Predator Claw' }, desc: { ru: 'Каждый 5-й удар x2 урон', en: 'Every 5th hit x2 damage' } },
  { id: 'invisibility_cloak', name: { ru: 'Плащ невидимости', en: 'Invisibility Cloak' }, desc: { ru: 'При HP<30% враги реже атакуют', en: 'At HP<30% enemies attack less' } },
];

export const ARTIFACT_MAP: Record<ArtifactId, ArtifactDef> = Object.fromEntries(ARTIFACTS.map(a => [a.id, a])) as Record<ArtifactId, ArtifactDef>;

export interface ShopUpgradeDef {
  id: 'dmg' | 'radius' | 'speed' | 'spheres' | 'hp' | 'crit' | 'xp';
  name: { ru: string; en: string };
  desc: { ru: (lvl: number) => string; en: (lvl: number) => string };
  baseCost: number;
  maxLevel: number;
}

export const SHOP_UPGRADES: ShopUpgradeDef[] = [
  { id: 'dmg', name: { ru: 'Урон сфер', en: 'Sphere Damage' }, desc: { ru: () => '+5% урона', en: () => '+5% damage' }, baseCost: 200, maxLevel: 10 },
  { id: 'radius', name: { ru: 'Радиус сфер', en: 'Sphere Radius' }, desc: { ru: () => '+5% радиуса', en: () => '+5% radius' }, baseCost: 200, maxLevel: 10 },
  { id: 'speed', name: { ru: 'Скорость движения', en: 'Move Speed' }, desc: { ru: () => '+5% скорости', en: () => '+5% speed' }, baseCost: 300, maxLevel: 5 },
  { id: 'spheres', name: { ru: 'Стартовые сферы', en: 'Start Spheres' }, desc: { ru: () => '+1 сфера', en: () => '+1 sphere' }, baseCost: 500, maxLevel: 3 },
  { id: 'hp', name: { ru: 'Стартовое HP', en: 'Start HP' }, desc: { ru: () => '+10 HP', en: () => '+10 HP' }, baseCost: 150, maxLevel: 10 },
  { id: 'crit', name: { ru: 'Шанс крита', en: 'Crit Chance' }, desc: { ru: () => '+5% шанс крита', en: () => '+5% crit chance' }, baseCost: 400, maxLevel: 5 },
  { id: 'xp', name: { ru: 'Получаемый опыт', en: 'XP Gain' }, desc: { ru: () => '+5% опыта', en: () => '+5% XP' }, baseCost: 300, maxLevel: 5 },
];

export function shopCost(def: ShopUpgradeDef, currentLevel: number): number {
  return Math.floor(def.baseCost * Math.pow(3, currentLevel));
}

// ===== Sphere Types =====
export type SphereType = 'standard' | 'sniper' | 'shotgun' | 'chain' | 'aura';

export interface SphereTypeDef {
  id: SphereType;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  color: string;
  damageMult: number;
  rangeMult: number;
  delayMult: number;
  projectileSpeedMult: number;
  pellets: number; // shots per fire
  spread: number; // radians
  chain: boolean; // chains to nearby enemies
  aura: boolean; // continuous AoE damage
  auraRadius: number;
}

export const SPHERE_TYPES: Record<SphereType, SphereTypeDef> = {
  standard: {
    id: 'standard', name: { ru: 'Стандартная', en: 'Standard' },
    desc: { ru: 'Сбалансированная сфера', en: 'Balanced sphere' },
    color: '#5a8c4a', damageMult: 1, rangeMult: 1, delayMult: 1, projectileSpeedMult: 1,
    pellets: 1, spread: 0, chain: false, aura: false, auraRadius: 0,
  },
  sniper: {
    id: 'sniper', name: { ru: 'Снайпер', en: 'Sniper' },
    desc: { ru: 'Высокий урон, большая дальность, медленная', en: 'High damage, long range, slow' },
    color: '#c46d3d', damageMult: 2.5, rangeMult: 2, delayMult: 2, projectileSpeedMult: 2,
    pellets: 1, spread: 0, chain: false, aura: false, auraRadius: 0,
  },
  shotgun: {
    id: 'shotgun', name: { ru: 'Дробовик', en: 'Shotgun' },
    desc: { ru: '3 снаряда, короткая дальность', en: '3 pellets, short range' },
    color: '#c4453d', damageMult: 0.6, rangeMult: 0.6, delayMult: 1.2, projectileSpeedMult: 0.8,
    pellets: 3, spread: 0.4, chain: false, aura: false, auraRadius: 0,
  },
  chain: {
    id: 'chain', name: { ru: 'Цепная', en: 'Chain' },
    desc: { ru: 'Молния прыгает между врагами', en: 'Lightning jumps between enemies' },
    color: '#d4a830', damageMult: 1, rangeMult: 1, delayMult: 1.3, projectileSpeedMult: 1.5,
    pellets: 1, spread: 0, chain: true, aura: false, auraRadius: 0,
  },
  aura: {
    id: 'aura', name: { ru: 'Аура', en: 'Aura' },
    desc: { ru: 'Непрерывный урон по площади', en: 'Continuous AoE damage' },
    color: '#8a5a8a', damageMult: 0.4, rangeMult: 0.5, delayMult: 0.2, projectileSpeedMult: 1,
    pellets: 0, spread: 0, chain: false, aura: true, auraRadius: 80,
  },
};

// ===== Boss Types =====
export type BossType = 'shooter' | 'charger' | 'summoner' | 'aura';

export interface BossTypeDef {
  id: BossType;
  name: { ru: string; en: string };
  color: string;
}

export const BOSS_TYPES: Record<BossType, BossTypeDef> = {
  shooter: { id: 'shooter', name: { ru: 'Стрелок', en: 'Shooter' }, color: '#c4453d' },
  charger: { id: 'charger', name: { ru: 'Зарядник', en: 'Charger' }, color: '#d4943d' },
  summoner: { id: 'summoner', name: { ru: 'Призыватель', en: 'Summoner' }, color: '#8a5a8a' },
  aura: { id: 'aura', name: { ru: 'Аура', en: 'Aura' }, color: '#b8475a' },
};

// ===== Difficulty =====
export type Difficulty = 'easy' | 'normal' | 'hard' | 'nightmare';

export interface DifficultyDef {
  id: Difficulty;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  enemyHpMult: number;
  enemySpeedMult: number;
  enemyDamageMult: number;
  goldMult: number;
  spawnRateMult: number;
}

export const DIFFICULTIES: DifficultyDef[] = [
  { id: 'easy', name: { ru: 'Лёгкая', en: 'Easy' }, desc: { ru: 'Медленные враги, больше HP', en: 'Slower enemies, more HP' },
    enemyHpMult: 0.7, enemySpeedMult: 0.8, enemyDamageMult: 0.7, goldMult: 0.8, spawnRateMult: 0.8 },
  { id: 'normal', name: { ru: 'Обычная', en: 'Normal' }, desc: { ru: 'Сбалансированный вызов', en: 'Balanced challenge' },
    enemyHpMult: 1, enemySpeedMult: 1, enemyDamageMult: 1, goldMult: 1, spawnRateMult: 1 },
  { id: 'hard', name: { ru: 'Сложная', en: 'Hard' }, desc: { ru: 'Быстрые враги, больше урона', en: 'Faster enemies, more damage' },
    enemyHpMult: 1.4, enemySpeedMult: 1.2, enemyDamageMult: 1.3, goldMult: 1.5, spawnRateMult: 1.2 },
  { id: 'nightmare', name: { ru: 'Кошмар', en: 'Nightmare' }, desc: { ru: 'Враги свирепые, золото x2', en: 'Brutal enemies, gold x2' },
    enemyHpMult: 2, enemySpeedMult: 1.4, enemyDamageMult: 1.6, goldMult: 2, spawnRateMult: 1.5 },
];

// ===== Achievements =====
export type AchievementId =
  | 'kills_500' | 'kills_1000' | 'wave_30' | 'wave_50' | 'boss_5' | 'boss_10'
  | 'evolve_1' | 'evolve_3' | 'elite_10' | 'combo_50' | 'chest_5' | 'dash_50';

export interface AchievementDef {
  id: AchievementId;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const ACHIEVEMENTS: AchievementDef[] = [
  { id: 'kills_500', name: { ru: 'Палач', en: 'Executioner' }, desc: { ru: 'Убить 500 врагов', en: 'Kill 500 enemies' } },
  { id: 'kills_1000', name: { ru: 'Мясник', en: 'Butcher' }, desc: { ru: 'Убить 1000 врагов', en: 'Kill 1000 enemies' } },
  { id: 'wave_30', name: { ru: 'Выживший', en: 'Survivor' }, desc: { ru: 'Достичь волны 30', en: 'Reach wave 30' } },
  { id: 'wave_50', name: { ru: 'Легенда', en: 'Legend' }, desc: { ru: 'Достичь волны 50', en: 'Reach wave 50' } },
  { id: 'boss_5', name: { ru: 'Охотник', en: 'Hunter' }, desc: { ru: 'Убить 5 боссов', en: 'Kill 5 bosses' } },
  { id: 'boss_10', name: { ru: 'Герой', en: 'Hero' }, desc: { ru: 'Убить 10 боссов', en: 'Kill 10 bosses' } },
  { id: 'evolve_1', name: { ru: 'Эволюция', en: 'Evolution' }, desc: { ru: 'Эволюционировать способность', en: 'Evolve an ability' } },
  { id: 'evolve_3', name: { ru: 'Совершенство', en: 'Perfection' }, desc: { ru: '3 эволюции за забег', en: '3 evolutions in one run' } },
  { id: 'elite_10', name: { ru: 'Элитный охотник', en: 'Elite Hunter' }, desc: { ru: 'Убить 10 элитных врагов', en: 'Kill 10 elite enemies' } },
  { id: 'combo_50', name: { ru: 'Мастер комбо', en: 'Combo Master' }, desc: { ru: 'Комбо x50', en: 'Reach combo x50' } },
  { id: 'chest_5', name: { ru: 'Сокровищница', en: 'Treasure Hunter' }, desc: { ru: 'Открыть 5 сундуков', en: 'Open 5 chests' } },
  { id: 'dash_50', name: { ru: 'Призрак', en: 'Phantom' }, desc: { ru: '50 рывков за забег', en: '50 dashes in one run' } },
];

export function abilityName(id: AbilityType, lang: Lang): string {
  return ABILITIES[id].name[lang];
}
export function artifactName(id: ArtifactId, lang: Lang): string {
  return ARTIFACT_MAP[id].name[lang];
}
