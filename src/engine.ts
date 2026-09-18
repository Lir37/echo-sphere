import {
  ABILITIES, ACTIVE_KEYS, ARTIFACT_MAP,
  SPHERE_TYPES, BOSS_TYPES, DIFFICULTIES,
  type AbilityType, type ArtifactId, type SphereType, type BossType, type Difficulty,
} from './gameData';
import { playSound } from './audio';
import {
  CHARACTER_DEFS,
  type CharacterId,
} from './characters';
import {
  getCharacterId,
  getCharacterDamageMultiplier,
  getCharacterAttackSpeedMultiplier,
  getCharacterRadiusMultiplier,
  getCharacterMoveSpeedMultiplier,
  getCharacterMaxHpMultiplier,
  getCharacterDamageTakenMultiplier,
  getCharacterStatusDurationMultiplier,
  getCharacterStatusDamageMultiplier,
  getHunterMarkMultiplier,
  shouldMarkHunterTarget,
  applyAlchemistReaction,
  getEngineerNetworkRange,
  getCharacterFormation,
  getFormationDamageTakenMultiplier,
} from './characterRuntime';
import { loadCharacterId, loadCharacterProfiles } from './persistence';
import { getArtifactMoveSpeedMultiplier, getArtifactMaxHpBonus, getArtifactXpMultiplier, getArtifactRegenPerSecond, getArtifactSphereRadiusMultiplier, getArtifactSphereDamageMultiplier, getArtifactCooldownMultiplier, getArtifactSphereDelayMultiplier, getArtifactDamageTakenMultiplier, getArtifactCritChanceBonus, getArtifactDodgeChanceBonus, getArtifactVampireBonus, getArtifactReflectChance, getSphereArtifactDamageMultiplier, pickArtifactChoices } from './artifactSystem';
import { SPHERE_PROGRESSION, ABILITY_PROGRESSION, spherePriority, sphereLevel, sphereModifiers, SPHERE_ABILITY_SYNERGIES } from './sphereProgression';

export interface Vec { x: number; y: number; }

export interface PlayerState {
  pos: Vec;
  hp: number;
  maxHp: number;
  speed: number;
  level: number;
  xp: number;
  xpToNext: number;
  abilities: Partial<Record<AbilityType, number>>; // ability -> level
  evolutions: string[];
  artifacts: ArtifactId[];
  kills: number;
  mutationStage: number; // 0..4
  invulnerableTimer: number;
  invulnUsed: boolean;
  shieldCharges: number;
  shieldTimer: number;
  dodgeTimer: number;
  fireTrailTimer: number;
  fireTrailCooldown: number;
  blastCooldown: number;
  teleportCooldown: number;
  shieldCooldown: number;
  minionCooldown: number;
  lightningCooldown: number;
  timestopCooldown: number;
  darkritualCooldown: number;
  overloadTimer: number;
  timestopTimer: number;
  swiftBootsTimer: number;
  chaosOrbTimer: number;
  chaosOrbBuff: 'dmg' | 'radius' | null;
  chaosOrbBuffTimer: number;
  teleportDamageBuffTimer: number;
  blinkHpCost: boolean;
  sphereXpAccumulator: number;
  sphereUpgradeCount: number;
  sphereMods: SphereMods;
  sphereProgression: Partial<Record<SphereType, number>>;
  sphereBranches: Partial<Record<SphereType, import('./sphereProgression').SphereEvolutionId>>;
  dashCooldown: number;
  dashTimer: number; // active dash i-frames
  dashDir: Vec;
  dashCount: number; // for achievement tracking
  combo: number;
  comboTimer: number;
  comboMult: number;
  buffTimer: number; // temporary damage buff from chest
  eliteKills: number;
  chestOpens: number;
  characterId: CharacterId;
  characterMasteryLevel: number;
  hunterMarkTarget: EnemyEntity | null;
  hunterMarkTimer: number;
  hunterHitCount: number;
  hunterHuntTarget: EnemyEntity | null;
  hunterHuntTimer: number;
  hunterTrophyTimer: number;
  engineerRelaySource: SphereEntity | null;
  engineerRelayTimer: number;
  alchemistCatalystTimer: number;
}

export interface SphereEntity {
  pos: Vec;
  radius: number;
  damage: number;
  attackDelay: number;
  attackTimer: number;
  rotation: number;
  alive: boolean;
  killsContribution: number;
  visualTier: number;
  type: SphereType;
  auraTimer: number;
}

export interface SphereProjectile {
  pos: Vec;
  vel: Vec;
  damage: number;
  radius: number;
  alive: boolean;
  color: string;
  pierce: number; // how many enemies it can pass through
  hitEnemies: Set<EnemyEntity>;
  effect: 'none' | 'fire' | 'freeze' | 'poison';
  ricochet: number; // bounces remaining
  life: number;
  sourceSphere?: SphereEntity;
}

export interface EnemyEntity {
  pos: Vec;
  hp: number;
  maxHp: number;
  speed: number;
  radius: number;
  damage: number;
  type: 'normal' | 'fast' | 'tank' | 'boss';
  color: string;
  shape: 'circle' | 'square' | 'triangle' | 'hexagon';
  slowTimer: number;
  slowFactor: number;
  freezeTimer: number;
  hitFlash: number;
  isBoss: boolean;
  bossShootTimer: number;
  bossProjectiles: BossProjectile[];
  xpValue: number;
  rotation: number;
  tier: number;
  trailTimer: number;
  fireTimer: number;
  fireDps: number;
  poisonTimer: number;
  poisonDps: number;
  isElite: boolean;
  bossType: BossType;
  chargeTimer: number;
  isCharging: boolean;
  chargeDir: Vec;
  summonTimer: number;
  auraRadius: number;
  auraDps: number;
}

export interface BossProjectile {
  pos: Vec;
  vel: Vec;
  damage: number;
  radius: number;
  alive: boolean;
}

export interface XPOrb {
  pos: Vec;
  value: number;
  radius: number;
  alive: boolean;
  vel: Vec;
}

export interface HealthPack {
  pos: Vec;
  alive: boolean;
  radius: number;
}

export interface Particle {
  pos: Vec;
  vel: Vec;
  life: number;
  maxLife: number;
  color: string;
  size: number;
}

export interface FireTrailSegment {
  pos: Vec;
  life: number;
  maxLife: number;
  damage: number;
}

export interface MinionEntity {
  pos: Vec;
  hp: number;
  attackTimer: number;
  life: number;
  radius: number;
  damage: number;
  rotation: number;
}

export interface LightningBolt {
  from: Vec;
  to: Vec;
  life: number;
}

export interface UpgradeChoice {
  type: 'ability' | 'sphere';
  ability?: AbilityType;
  abilityEvolutionIndex?: number;
  abilityStage?: 'upgrade' | 'branch' | 'final';
  sphereType?: SphereType;
  sphereBranch?: import('./sphereProgression').SphereEvolutionId;
  sphereFinalIndex?: number;
  sphereStage?: 'upgrade' | 'branch' | 'final';
  name?: { ru: string; en: string };
  desc?: { ru: string; en: string };
  currentLevel: number;
  newLevel: number;
}

export interface GameStats {
  time: number;
  wave: number;
  enemiesKilled: number;
  goldEarned: number;
}

export interface DamageNumber {
  pos: Vec;
  value: number;
  life: number;
  maxLife: number;
  crit: boolean;
  vel: Vec;
}

export interface ChestEntity {
  pos: Vec;
  alive: boolean;
  radius: number;
}

export interface GameState {
  player: PlayerState;
  spheres: SphereEntity[];
  enemies: EnemyEntity[];
  xpOrbs: XPOrb[];
  healthPacks: HealthPack[];
  particles: Particle[];
  fireTrails: FireTrailSegment[];
  minions: MinionEntity[];
  lightnings: LightningBolt[];
  wave: number;
  waveTimer: number;
  waveEnemiesToSpawn: number;
  waveSpawnTimer: number;
  bossActive: boolean;
  bossDefeated: number;
  time: number;
  paused: boolean;
  gameOver: boolean;
  pendingUpgrade: UpgradeChoice[] | null;
  pendingArtifact: ArtifactId[] | null;
  stats: GameStats;
  screenShake: number;
  bossArrow: Vec | null;
  flashText: { text: string; life: number; color: string } | null;
  keys: Record<string, boolean>;
  mouse: { x: number; y: number; down: boolean };
  worldWidth: number;
  worldHeight: number;
  mapTheme: MapTheme;
  camera: Vec;
  activeKeyMap: Record<string, AbilityType>; // hotkey -> ability
  sphereProjectiles: SphereProjectile[];
  artifactPickupPending: boolean;
  pendingSphereUpgrade: SphereUpgradeChoice[] | null;
  damageNumbers: DamageNumber[];
  chests: ChestEntity[];
  difficulty: Difficulty;
  evolutionsThisRun: number;
  selectedSphereType: SphereType;
  shopUpgrades: Record<string, number>;
}

export interface ShopState {
  gold: number;
  upgrades: Record<string, number>; // id -> level
}

export interface SphereMods {
  multishot: number;  // extra projectiles per shot
  pierce: number;    // enemies a projectile passes through
  ricochet: number;  // bounce count
  fire: number;      // fire effect level (DoT)
  freeze: number;    // freeze effect level
  poison: number;    // poison effect level (DoT)
}

export interface SphereUpgradeChoice {
  id: keyof SphereMods;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export interface LeaderEntry {
  name: string;
  time: number;
  wave: number;
  date: number;
}

export const DEFAULT_MAX_SPHERES = 5;
export const MAX_SPHERES_CAP = 8;
export const BASE_PLAYER_SPEED = 180;
export const BASE_SPHERE_RADIUS = 130;
export const BASE_SPHERE_DAMAGE = 12;
export const BASE_SPHERE_DELAY = 1.2;
export const PLAYER_RADIUS = 16;

export type MapTheme = 'parchment' | 'bamboo' | 'ocean' | 'sunset';

export const MAP_THEMES: { id: MapTheme; name: { ru: string; en: string } }[] = [
  { id: 'parchment', name: { ru: 'Свиток', en: 'Scroll' } },
  { id: 'bamboo', name: { ru: 'Бамбук', en: 'Bamboo' } },
  { id: 'ocean', name: { ru: 'Океан', en: 'Ocean' } },
  { id: 'sunset', name: { ru: 'Закат', en: 'Sunset' } },
];

export function createInitialState(
  shop: ShopState,
  playerName: string,
  difficulty: Difficulty = 'normal',
  mapTheme: MapTheme = 'parchment',
): GameState {
  const characterId = loadCharacterId();
  const profile = loadCharacterProfiles().find((item) => item.id === characterId);
  const characterMasteryLevel = profile?.masteryLevel || 1;
  const baseHp = 100 + (shop.upgrades.hp || 0) * 10;
  const startHp = baseHp * getCharacterMaxHpMultiplierForId(characterId);
  const player: PlayerState = {
    pos: { x: 0, y: 0 },
    hp: startHp,
    maxHp: startHp,
    speed: BASE_PLAYER_SPEED,
    level: 1,
    xp: 0,
    xpToNext: 5,
    abilities: {},
    evolutions: [],
    artifacts: [],
    kills: 0,
    mutationStage: 0,
    invulnerableTimer: 0,
    invulnUsed: false,
    shieldCharges: 0,
    shieldTimer: 0,
    dodgeTimer: 0,
    fireTrailTimer: 0,
    fireTrailCooldown: 0,
    blastCooldown: 0,
    teleportCooldown: 0,
    shieldCooldown: 0,
    minionCooldown: 0,
    lightningCooldown: 0,
    timestopCooldown: 0,
    darkritualCooldown: 0,
    overloadTimer: 0,
    timestopTimer: 0,
    swiftBootsTimer: 0,
    chaosOrbTimer: 0,
    chaosOrbBuff: null,
    chaosOrbBuffTimer: 0,
    teleportDamageBuffTimer: 0,
    blinkHpCost: false,
    sphereXpAccumulator: 0,
    sphereUpgradeCount: 0,
    sphereProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },
    sphereBranches: {},
    sphereMods: { multishot: 0, pierce: 0, ricochet: 0, fire: 0, freeze: 0, poison: 0 },
    dashCooldown: 0,
    dashTimer: 0,
    dashDir: { x: 0, y: 0 },
    dashCount: 0,
    combo: 0,
    comboTimer: 0,
    comboMult: 1,
    buffTimer: 0,
    eliteKills: 0,
    chestOpens: 0,
    characterId,
    characterMasteryLevel,
    hunterMarkTarget: null,
    hunterMarkTimer: 0,
    hunterHitCount: 0,
    hunterHuntTarget: null,
    hunterHuntTimer: 0,
    hunterTrophyTimer: 0,
    engineerRelaySource: null,
    engineerRelayTimer: 0,
    alchemistCatalystTimer: 0,
  };
  return {
    player,
    spheres: [],
    enemies: [],
    xpOrbs: [],
    healthPacks: [],
    particles: [],
    fireTrails: [],
    minions: [],
    lightnings: [],
    wave: 0,
    waveTimer: 3,
    waveEnemiesToSpawn: 0,
    waveSpawnTimer: 0,
    bossActive: false,
    bossDefeated: 0,
    time: 0,
    paused: false,
    gameOver: false,
    pendingUpgrade: null,
    pendingArtifact: null,
    stats: { time: 0, wave: 0, enemiesKilled: 0, goldEarned: 0 },
    screenShake: 0,
    bossArrow: null,
    flashText: null,
    keys: {},
    mouse: { x: 0, y: 0, down: false },
    worldWidth: 2400,
    worldHeight: 2400,
    mapTheme,
    camera: { x: 0, y: 0 },
    activeKeyMap: {},
    sphereProjectiles: [],
    artifactPickupPending: false,
    pendingSphereUpgrade: null,
    damageNumbers: [],
    chests: [],
    difficulty: difficulty,
    evolutionsThisRun: 0,
    selectedSphereType: 'standard',
    shopUpgrades: { ...shop.upgrades },
  };
}

function getCharacterMaxHpMultiplierForId(id: CharacterId): number {
  return 1 + CHARACTER_DEFS[id].baseModifiers.maxHp;
}

// ===== Derived stats =====
export function getMaxSpheres(s: GameState): number {
  let m = DEFAULT_MAX_SPHERES + (s.player.abilities.maxspheres || 0) + (s.shopUpgrades.spheres || 0);
  return Math.min(m, MAX_SPHERES_CAP);
}

export function getMoveSpeed(s: GameState): number {
  let sp = BASE_PLAYER_SPEED;
  const lvl = s.player.abilities.movespeed || 0;
  sp *= 1 + lvl * 0.1;
  sp *= 1 + (s.shopUpgrades.speed || 0) * 0.05;
  sp *= getArtifactMoveSpeedMultiplier(s);
  if (s.player.swiftBootsTimer > 0) sp *= 1.1;
  if (s.player.mutationStage >= 3) sp *= 1.2;
  sp *= getCharacterMoveSpeedMultiplier(s);
  if (s.player.hunterTrophyTimer > 0 && getCharacterId(s) === 'hunter') sp *= 1.1;
  if (getCharacterId(s) === 'berserker' && s.player.characterMasteryLevel >= 5 && s.player.buffTimer > 0) sp *= 1.05;
  return sp;
}

export function getSphereRadius(s: GameState, sphere: SphereEntity): number {
  let r = BASE_SPHERE_RADIUS;
  const lvl = s.player.abilities.radius || 0;
  r *= 1 + lvl * 0.15;
  r *= 1 + (s.shopUpgrades.radius || 0) * 0.05;
  r *= getArtifactSphereRadiusMultiplier(s);
  if (s.player.chaosOrbBuff === 'radius' && s.player.chaosOrbBuffTimer > 0) r *= 1.2;
  if (s.player.mutationStage >= 2) r *= 1.15;
  r *= getCharacterRadiusMultiplier(s);
  r *= sphereModifiers(s, sphere.type).radius;
  if (getCharacterId(s) === 'architect' && s.player.characterMasteryLevel >= 3) r *= 1.02;
  return r;
}

export function getSphereDamage(s: GameState, sphere: SphereEntity): number {
  let d = BASE_SPHERE_DAMAGE;
  const lvl = s.player.abilities.damage || 0;
  d *= 1 + lvl * 0.2;
  d *= 1 + (s.shopUpgrades.dmg || 0) * 0.05;
  if (s.player.mutationStage >= 1) d *= 1.1;
  if (s.player.chaosOrbBuff === 'dmg' && s.player.chaosOrbBuffTimer > 0) d *= 1.2;
  if (s.player.teleportDamageBuffTimer > 0) d *= 2;
  if (s.player.overloadTimer > 0) d *= 1.25 + (s.player.abilities.darkritual || 0) * 0.04;
  if (s.player.fireTrailTimer > 0 && sphere && s.player.sphereMods.fire > 0) {
    d *= 1.2 + (s.player.abilities.firetrail || 0) * 0.025;
  }
  const sbLvl = s.player.abilities.sphereboost || 0;
  if (sbLvl > 0) {
    const per = Math.max(50, 100 - (sbLvl - 1) * 10);
    d += Math.floor(s.player.kills / per);
  }
  if (s.player.evolutions.includes('echoaccumulator')) {
    d += s.player.sphereXpAccumulator * 0.5;
  }
  d *= getArtifactSphereDamageMultiplier(s);
  d *= getSphereArtifactDamageMultiplier(s, sphere);
  d *= sphereModifiers(s, sphere.type, sphere).damage;
  return d;
}

export function getSphereDelay(s: GameState): number {
  let d = BASE_SPHERE_DELAY;
  const lvl = s.player.abilities.attackspeed || 0;
  d *= Math.pow(0.9, lvl);
  d /= Math.max(0.01, getCharacterAttackSpeedMultiplier(s));
  d *= getArtifactSphereDelayMultiplier(s);
  if (s.player.overloadTimer > 0) d *= 0.72;
  if (s.player.fireTrailTimer > 0) d *= 0.78;
  return d;
}

export function getCritChance(s: GameState, sphere?: SphereEntity): number {
  let c = (s.player.abilities.crit || 0) * 0.1;
  c += (s.shopUpgrades.crit || 0) * 0.05;
  c += getArtifactCritChanceBonus(s);
  if (sphere?.type === 'sniper' && sphereLevel(s, 'sniper') >= 3) c += 0.15;
  return c;
}

export function getDodgeChance(s: GameState): number {
  return Math.min(0.75, (s.player.abilities.dodge || 0) * 0.1 + getArtifactDodgeChanceBonus(s));
}

export function getVampirePercent(s: GameState): number {
  const lvl = s.player.abilities.vampire || 0;
  return lvl * 0.03 + getArtifactVampireBonus(s);
}

export function getSlowRadius(): number {
  return 300;
}

export function getSlowFactor(s: GameState): number {
  const lvl = s.player.abilities.slow || 0;
  return lvl > 0 ? 1 - (0.1 + (lvl - 1) * 0.05) : 1;
}

export function getCooldownMult(s: GameState): number {
  return getArtifactCooldownMultiplier(s);
}

export function getDamageTakenMult(s: GameState): number {
  let m = 1;
  m *= getCharacterDamageTakenMultiplier(s);
  m *= getFormationDamageTakenMultiplier(s);
  return m;
}

export function getXpMult(s: GameState): number {
  let m = 1;
  m *= 1 + (s.shopUpgrades.xp || 0) * 0.05;
  m *= getArtifactXpMultiplier(s);
  return m;
}

export function getMagnetRadius(s: GameState): number {
  let r = 60;
  const lvl = s.player.abilities.magnet || 0;
  r *= 1 + lvl * 0.2;
  return r;
}

// ===== Helpers =====
function dist(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}
function rand(min: number, max: number): number {
  return min + Math.random() * (max - min);
}
function clamp(v: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, v));
}

export function assignHotkey(s: GameState, ability: AbilityType): string {
  // already mapped?
  for (const k of Object.keys(s.activeKeyMap)) {
    if (s.activeKeyMap[k] === ability) return k;
  }
  for (const k of ACTIVE_KEYS) {
    if (!(k in s.activeKeyMap)) {
      s.activeKeyMap[k] = ability;
      return k;
    }
  }
  return '';
}

// ===== Wave spawning =====
function spawnEnemy(s: GameState, isBoss: boolean): EnemyEntity {
  const wave = s.wave;
  const angle = Math.random() * Math.PI * 2;
  const spawnDist = 700;
  const px = s.player.pos.x + Math.cos(angle) * spawnDist;
  const py = s.player.pos.y + Math.sin(angle) * spawnDist;
  const diff = DIFFICULTIES.find(d => d.id === s.difficulty)!;
  if (isBoss) {
    const hp = (400 + wave * 60) * diff.enemyHpMult;
    // pick boss type based on boss count
    const bossTypes: BossType[] = ['shooter', 'charger', 'summoner', 'aura'];
    const bt = bossTypes[s.bossDefeated % bossTypes.length];
    const bdef = BOSS_TYPES[bt];
    const baseSpeed = bt === 'charger' ? 80 + wave * 2 : 50 + wave * 1.5;
    return {
      pos: { x: px, y: py },
      hp, maxHp: hp,
      speed: baseSpeed * diff.enemySpeedMult,
      radius: 42,
      damage: 25 * diff.enemyDamageMult,
      type: 'boss',
      color: bdef.color,
      shape: 'hexagon',
      slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
      isBoss: true,
      bossShootTimer: 5,
      bossProjectiles: [],
      xpValue: 50 + wave * 5,
      rotation: 0,
      tier: Math.floor(wave / 10),
      trailTimer: 0,
      fireTimer: 0, fireDps: 0,
      poisonTimer: 0, poisonDps: 0,
      isElite: false,
      bossType: bt,
      chargeTimer: 3,
      isCharging: false,
      chargeDir: { x: 0, y: 0 },
      summonTimer: 4,
      auraRadius: bt === 'aura' ? 120 : 0,
      auraDps: bt === 'aura' ? 10 + wave * 2 : 0,
    };
  }
  const r = Math.random();
  let type: EnemyEntity['type'] = 'normal';
  let hp = (20 + wave * 6) * diff.enemyHpMult;
  let speed = (70 + wave * 2) * diff.enemySpeedMult;
  let radius = 14;
  let dmg = 10 * diff.enemyDamageMult;
  let color = '#4a7a8a';
  let shape: EnemyEntity['shape'] = 'circle';
  if (r < 0.2 && wave > 2) { type = 'fast'; hp = (12 + wave * 4) * diff.enemyHpMult; speed = (130 + wave * 3) * diff.enemySpeedMult; radius = 10; color = '#d4a830'; shape = 'triangle'; }
  else if (r < 0.35 && wave > 4) { type = 'tank'; hp = (50 + wave * 14) * diff.enemyHpMult; speed = (45 + wave) * diff.enemySpeedMult; radius = 20; color = '#8a5a8a'; shape = 'square'; dmg = 10 * diff.enemyDamageMult; }
  // elite chance: 5% after wave 5, scales up
  const isElite = wave > 5 && Math.random() < Math.min(0.12, 0.03 + wave * 0.005);
  if (isElite) {
    hp *= 3;
    radius += 4;
    dmg *= 1.5;
    color = '#b8475a';
  }
  return {
    pos: { x: px, y: py },
    hp, maxHp: hp,
    speed, radius, damage: dmg, type,
    color, shape,
    slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
    isBoss: false, bossShootTimer: 0, bossProjectiles: [],
    xpValue: (type === 'tank' ? 4 : type === 'fast' ? 2 : 1) * (isElite ? 5 : 1),
    rotation: 0,
    tier: s.bossDefeated,
    trailTimer: 0,
    fireTimer: 0, fireDps: 0,
    poisonTimer: 0, poisonDps: 0,
    isElite,
    bossType: 'shooter',
    chargeTimer: 0, isCharging: false, chargeDir: { x: 0, y: 0 },
    summonTimer: 0, auraRadius: 0, auraDps: 0,
  };
}

function startWave(s: GameState): void {
  s.wave++;
  s.stats.wave = s.wave;
  const isBossWave = s.wave % 10 === 0;
  if (isBossWave) {
    s.bossActive = true;
    s.enemies.push(spawnEnemy(s, true));
    // also spawn some minions
    s.waveEnemiesToSpawn = 3 + Math.floor(s.wave / 10);
    playSound('boss');
  } else {
    s.waveEnemiesToSpawn = 4 + Math.floor(s.wave * 1.4);
    playSound('wave');
  }
  s.waveSpawnTimer = 0.5;
}

function registerHunterHit(s: GameState, enemy: EnemyEntity, sphere: SphereEntity): void {
  if (getCharacterId(s) !== 'hunter') return;
  if (!['sniper', 'chain'].includes(sphere.type)) return;
  if (!shouldMarkHunterTarget(enemy)) return;

  const p = s.player;
  const duration = p.characterMasteryLevel >= 2 ? 6 : 5;
  const threshold = p.characterMasteryLevel >= 4 ? 4 : 5;
  const activeSameTarget = p.hunterMarkTarget === enemy && p.hunterMarkTimer > 0;

  if (!activeSameTarget) {
    p.hunterMarkTarget = enemy;
    p.hunterMarkTimer = duration;
    p.hunterHitCount = 1;
    for (let i = 0; i < 10; i++) {
      const a = Math.random() * Math.PI * 2;
      const speed = 50 + Math.random() * 80;
      s.particles.push({
        pos: { ...enemy.pos },
        vel: { x: Math.cos(a) * speed, y: Math.sin(a) * speed },
        life: 0.35, maxLife: 0.35, color: '#b8475a', size: 2.5,
      });
    }
    return;
  }

  p.hunterMarkTimer = duration;
  p.hunterHitCount++;
  if (p.hunterHitCount >= threshold) {
    p.hunterHuntTarget = enemy;
    p.hunterHuntTimer = 3;
    p.hunterHitCount = 0;
    for (let i = 0; i < 18; i++) {
      const a = Math.random() * Math.PI * 2;
      const r = 18 + Math.random() * 24;
      s.particles.push({
        pos: { x: enemy.pos.x + Math.cos(a) * r, y: enemy.pos.y + Math.sin(a) * r },
        vel: { x: 0, y: -25 },
        life: 0.5, maxLife: 0.5, color: '#c4453d', size: 3,
      });
    }
    s.screenShake = Math.min(0.16, s.screenShake + 0.04);
  }
}

function consumeEngineerRelayBonus(s: GameState, sphere: SphereEntity): number {
  if (getCharacterId(s) !== 'engineer') return 1;
  const source = s.player.engineerRelaySource;
  if (!source || s.player.engineerRelayTimer <= 0 || source === sphere) return 1;
  const range = getEngineerNetworkRange(s);
  if (dist(source.pos, sphere.pos) > range) return 1;
  s.player.engineerRelaySource = null;
  s.player.engineerRelayTimer = 0;
  return 1.25;
}

function triggerEngineerRelay(s: GameState, sphere: SphereEntity): void {
  if (getCharacterId(s) !== 'engineer') return;
  s.player.engineerRelaySource = sphere;
  s.player.engineerRelayTimer = s.player.characterMasteryLevel >= 4 ? 0.55 : 0.4;
  for (const neighbour of s.spheres) {
    if (neighbour === sphere || !neighbour.alive) continue;
    if (dist(neighbour.pos, sphere.pos) <= getEngineerNetworkRange(s)) {
      s.lightnings.push({ from: { ...sphere.pos }, to: { ...neighbour.pos }, life: 0.12 });
    }
  }
}

// ===== Damage application =====
function dealDamageToEnemy(s: GameState, enemy: EnemyEntity, dmg: number, fromSphere?: SphereEntity, allowSphereProc = true): void {
  if (enemy.hp <= 0) return;
  if (fromSphere) registerHunterHit(s, enemy, fromSphere);

  let actual = dmg;
  if (fromSphere) {
    actual *= getCharacterDamageMultiplier(s, fromSphere);
    actual *= getHunterMarkMultiplier(s, enemy);
  }
  let isCrit = false;
  let critChance = getCritChance(s, fromSphere);
  if (fromSphere && getCharacterId(s) === 'hunter' && s.player.hunterMarkTarget === enemy && s.player.hunterMarkTimer > 0 && s.player.characterMasteryLevel >= 3) {
    critChance += 0.02;
  }
  if (fromSphere && getCharacterId(s) === 'architect' && getCharacterFormation(s).type === 'triangle') {
    critChance += 0.10;
  }
  // crit
  if (fromSphere && Math.random() < critChance) { actual *= 2; isCrit = true; }
  // predator claw: every 5th hit
  if (fromSphere && s.player.artifacts.includes('predator_claw')) {
    fromSphere.killsContribution++;
    if (fromSphere.killsContribution % 5 === 0) { actual *= 2; isCrit = true; }
  }
  // Sphere evolution mechanics: evolutions alter the combat loop, not just stats.
  if (fromSphere && allowSphereProc) {
    const branch = s.player.sphereBranches?.[fromSphere.type];
    const finalId = (s.player.evolutions || []).find((x: string) => x.startsWith('sphere:' + fromSphere.type + ':7:'));
    const finalIndex = finalId ? Number(finalId.split(':').pop()) : null;

    if (branch === 'standard_resonator') {
      const hits = ((fromSphere as any).evolutionHits || 0) + 1;
      (fromSphere as any).evolutionHits = hits;
      if (hits % 3 === 0) {
        const shockDamage = finalIndex === 0 ? actual * 0.65 : finalIndex === 1 ? actual * 0.45 : actual * 0.35;
        const shockRadius = finalIndex === 0 ? 115 : finalIndex === 1 ? 100 : 90;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < shockRadius) {
            dealDamageToEnemy(s, nearby, shockDamage, fromSphere, false);
            if (finalIndex === 2) {
              nearby.slowTimer = Math.max(nearby.slowTimer, 0.8);
              nearby.slowFactor = Math.min(nearby.slowFactor, 0.7);
            }
          }
        }
        if (finalIndex === 1) {
          for (const nearby of s.enemies) {
            if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < 100) {
              const dx = nearby.pos.x - enemy.pos.x, dy = nearby.pos.y - enemy.pos.y;
              const d = Math.hypot(dx, dy) || 1;
              nearby.pos.x += dx / d * 18;
              nearby.pos.y += dy / d * 18;
            }
          }
        }
        s.screenShake = Math.min(0.14, s.screenShake + (finalIndex === 0 ? 0.04 : 0.025));
      }
    } else if (branch === 'standard_singularity') {
      enemy.slowTimer = Math.max(enemy.slowTimer, finalIndex === 0 ? 1.2 : finalIndex === 1 ? 1.8 : 0.9);
      enemy.slowFactor = Math.min(enemy.slowFactor, finalIndex === 0 ? 0.5 : finalIndex === 1 ? 0.58 : 0.68);
      if (finalIndex === 0 || finalIndex === 2) {
        const pull = finalIndex === 0 ? 24 : 40;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < (finalIndex === 0 ? 75 : 100)) {
            const dx = enemy.pos.x - nearby.pos.x, dy = enemy.pos.y - nearby.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            nearby.pos.x += dx / d * pull;
            nearby.pos.y += dy / d * pull;
          }
        }
      }
      if (finalIndex === 2 && enemy.hp < enemy.maxHp * 0.5) actual *= 1.15;
    } else if (branch === 'standard_swarm') {
      const count = finalIndex === 2 ? 2 : 1;
      const chance = finalIndex === null ? 1 : finalIndex === 0 ? 0.35 : finalIndex === 1 ? 0.55 : 1;
      if (Math.random() < chance) {
        for (let i = 0; i < count; i++) {
          const a = Math.atan2(enemy.pos.y - fromSphere.pos.y, enemy.pos.x - fromSphere.pos.x) + (i === 0 ? 0.35 : -0.35);
          s.sphereProjectiles.push({
            pos: { ...fromSphere.pos }, vel: { x: Math.cos(a) * 430, y: Math.sin(a) * 430 },
            damage: actual * (finalIndex === 0 ? 0.28 : 0.22), radius: 4, alive: true, color: '#d4943d', pierce: 0,
            hitEnemies: new Set(), effect: 'none', ricochet: 0, life: 1.2, sourceSphere: fromSphere,
          });
        }
      }
    } else if (branch === 'sniper_oracle' && s.player.hunterMarkTarget === enemy && isCrit) {
      actual *= finalIndex === 0 ? 1.5 : finalIndex === 1 ? 1.3 : 1.22;
      if (finalIndex === 2) {
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < 65) {
            dealDamageToEnemy(s, nearby, actual * 0.25, fromSphere, false);
          }
        }
      }
    } else if (branch === 'sniper_assassin' && enemy.hp / enemy.maxHp < 0.35) {
      actual *= finalIndex === 0 ? 1.7 : finalIndex === 1 ? 2.2 : 1.45;
      if (finalIndex === 2) s.player.hp = Math.min(s.player.maxHp, s.player.hp + actual * 0.01);
    } else if (branch === 'sniper_beacon') {
      s.player.hunterMarkTarget = enemy;
      s.player.hunterMarkTimer = Math.max(s.player.hunterMarkTimer, finalIndex === 1 ? 5 : 3);
      enemy.slowTimer = Math.max(enemy.slowTimer, finalIndex === 0 ? 0.9 : finalIndex === 1 ? 1.2 : 0.7);
      enemy.slowFactor = Math.min(enemy.slowFactor, finalIndex === 0 ? 0.65 : finalIndex === 1 ? 0.7 : 0.6);
      const radius = finalIndex === 0 ? 90 : finalIndex === 1 ? 140 : 110;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          nearby.slowTimer = Math.max(nearby.slowTimer, finalIndex === 1 ? 1 : 0.5);
          nearby.slowFactor = Math.min(nearby.slowFactor, finalIndex === 2 ? 0.65 : 0.8);
        }
      }
    } else if (branch === 'shotgun_burst') {
      if (dist(enemy.pos, fromSphere.pos) < (finalIndex === 1 ? 180 : 150)) actual *= finalIndex === 0 ? 1.3 : finalIndex === 1 ? 1.5 : 1.22;
      if (fromSphere.type === 'shotgun' && sphereLevel(s, 'shotgun') >= 2 && dist(enemy.pos, fromSphere.pos) < 110) actual *= 1.20;
      if (finalIndex === 2 && dist(enemy.pos, fromSphere.pos) < 90) enemy.slowTimer = Math.max(enemy.slowTimer, 0.4);
    } else if (branch === 'shotgun_cataclysm') {
      const radius = finalIndex === 0 ? 60 : finalIndex === 1 ? 85 : 55;
      const splash = finalIndex === 0 ? 0.45 : finalIndex === 1 ? 0.65 : 0.35;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          dealDamageToEnemy(s, nearby, actual * splash, fromSphere, false);
        }
      }
      if (finalIndex === 2) {
        enemy.slowTimer = Math.max(enemy.slowTimer, 0.8);
        enemy.slowFactor = Math.min(enemy.slowFactor, 0.65);
      }
    } else if (branch === 'shotgun_hail') {
      const chance = finalIndex === 0 ? 0.25 : finalIndex === 1 ? 0.4 : 0.32;
      if (Math.random() < chance) {
        const radius = finalIndex === 1 ? 65 : 45;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
            dealDamageToEnemy(s, nearby, actual * (finalIndex === 1 ? 0.38 : 0.3), fromSphere, false);
          }
        }
        if (finalIndex === 2) actual *= 1.08;
      }
    } else if (branch === 'chain_web') {
      enemy.slowTimer = Math.max(enemy.slowTimer, finalIndex === 0 ? 0.7 : finalIndex === 1 ? 1.4 : 0.5);
      enemy.slowFactor = Math.min(enemy.slowFactor, finalIndex === 0 ? 0.7 : finalIndex === 1 ? 0.55 : 0.72);
      if (finalIndex === 2 && s.player.hunterMarkTarget === enemy) actual *= 1.25;
    } else if (branch === 'chain_storm') {
      const radius = finalIndex === 0 ? 70 : finalIndex === 1 ? 100 : 55;
      const splash = finalIndex === 0 ? 0.25 : finalIndex === 1 ? 0.4 : 0.2;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          dealDamageToEnemy(s, nearby, actual * splash, fromSphere, false);
          s.lightnings.push({ from: { ...enemy.pos }, to: { ...nearby.pos }, life: 0.2 });
        }
      }
      if (finalIndex === 2) actual *= 1.12;
    } else if (branch === 'chain_leech') {
      const heal = finalIndex === 0 ? 0.025 : finalIndex === 1 ? 0.045 : 0.018;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + actual * heal);
      if (finalIndex === 2 && enemy.hp < enemy.maxHp * 0.4) actual *= 1.2;
    } else if (branch === 'aura_sanctum') {
      enemy.slowTimer = Math.max(enemy.slowTimer, finalIndex === 0 ? 1.4 : finalIndex === 1 ? 2 : 0.9);
      enemy.slowFactor = Math.min(enemy.slowFactor, finalIndex === 0 ? 0.5 : finalIndex === 1 ? 0.42 : 0.62);
      if (finalIndex === 2) actual *= 1.12;
    } else if (branch === 'aura_gravity') {
      if (finalIndex === 0 || finalIndex === 1) {
        const pull = finalIndex === 0 ? 55 : 80;
        for (const nearby of s.enemies) {
          if (nearby.hp > 0 && dist(nearby.pos, fromSphere.pos) < (finalIndex === 0 ? 150 : 190)) {
            const dx = fromSphere.pos.x - nearby.pos.x, dy = fromSphere.pos.y - nearby.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            nearby.pos.x += dx / d * pull;
            nearby.pos.y += dy / d * pull;
          }
        }
      } else {
        actual *= 1.18;
      }
    } else if (branch === 'aura_overgrowth') {
      const bonus = finalIndex === 0 ? 0.18 : finalIndex === 1 ? 0.3 : 0.1;
      for (const ally of s.spheres) {
        if (ally !== fromSphere && ally.alive && dist(ally.pos, fromSphere.pos) < (finalIndex === 1 ? 180 : 140)) {
          ally.attackTimer = Math.max(0, ally.attackTimer - bonus);
        }
      }
      if (finalIndex === 2) actual *= 1.12;
    }
  }

  // buff from chest
  if (s.player.buffTimer > 0) actual *= 1.3;
  enemy.hp -= actual;
  enemy.hitFlash = 0.15;

  // Juicier impact: a short, directional burst makes every sphere hit readable.
  const impactCount = enemy.isBoss ? 10 : isCrit ? 9 : enemy.isElite ? 7 : 4;
  for (let i = 0; i < impactCount; i++) {
    const angle = Math.random() * Math.PI * 2;
    const speed = rand(isCrit ? 120 : 80, isCrit ? 260 : 180);
    s.particles.push({
      pos: { x: enemy.pos.x, y: enemy.pos.y },
      vel: { x: Math.cos(angle) * speed, y: Math.sin(angle) * speed },
      life: rand(0.16, isCrit ? 0.42 : 0.3),
      maxLife: 0.42,
      color: isCrit ? '#c4453d' : enemy.color,
      size: rand(isCrit ? 2.5 : 1.5, isCrit ? 4.5 : 3.2),
    });
  }
  if (isCrit) {
    s.screenShake = Math.min(0.24, s.screenShake + 0.06);
  }

  // damage number
  s.damageNumbers.push({
    pos: { x: enemy.pos.x + rand(-8, 8), y: enemy.pos.y - enemy.radius - 5 },
    value: Math.round(actual), life: 0.8, maxLife: 0.8, crit: isCrit,
    vel: { x: rand(-30, 30), y: -60 },
  });
  if (isCrit) playSound('crit'); else if (fromSphere) playSound('hit');
  // vampire
  if (fromSphere) {
    const vPct = getVampirePercent(s);
    if (vPct > 0) {
      const heal = actual * vPct;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + heal);
    }
  }
  // vampire ring artifact
  if (enemy.hp <= 0) {
    if (s.player.artifacts.includes('vampire_ring')) {
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + 5);
    }
    onEnemyDeath(s, enemy);
  }
}

function onEnemyDeath(s: GameState, enemy: EnemyEntity): void {
  if (!enemy.isBoss) {
    s.player.kills++;
    s.stats.enemiesKilled++;
    // combo system
    s.player.combo++;
    s.player.comboTimer = 3;
    if (s.player.combo >= 10) s.player.comboMult = 1.5;
    if (s.player.combo >= 25) s.player.comboMult = 2;
    if (s.player.combo >= 50) s.player.comboMult = 3;
    if (s.player.combo >= 100) s.player.comboMult = 4;
  }
  if (enemy.isElite) {
    s.player.eliteKills++;
    if (getCharacterId(s) === 'hunter' && s.player.characterMasteryLevel >= 5 && s.player.hunterMarkTarget === enemy) {
      s.player.hunterTrophyTimer = 4;
    }
    playSound('elite');
  }
  if (getCharacterId(s) === 'hunter' && s.player.hunterMarkTarget === enemy) {
    s.player.hunterMarkTarget = null;
    s.player.hunterMarkTimer = 0;
    s.player.hunterHitCount = 0;
  }
  if (getCharacterId(s) === 'hunter' && s.player.hunterHuntTarget === enemy) {
    s.player.hunterHuntTarget = null;
    s.player.hunterHuntTimer = 0;
  }
  playSound(enemy.isBoss ? 'explosion' : 'kill');
  // Stronger death burst, scaled by enemy importance.
  const deathParticles = enemy.isBoss ? 56 : enemy.isElite ? 18 : 10;
  for (let i = 0; i < deathParticles; i++) {
    s.particles.push({
      pos: { x: enemy.pos.x, y: enemy.pos.y },
      vel: { x: rand(-180, 180), y: rand(-180, 180) },
      life: rand(0.3, 0.8), maxLife: 0.8,
      color: enemy.color, size: rand(2, 5),
    });
  }
  // XP orb (combo multiplier applies)
  s.xpOrbs.push({
    pos: { x: enemy.pos.x, y: enemy.pos.y },
    value: Math.round(enemy.xpValue * s.player.comboMult), radius: 6, alive: true,
    vel: { x: rand(-40, 40), y: rand(-40, 40) },
  });
  // health pack drop chance
  const dropChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.5 : 0.04);
  if (Math.random() < dropChance) {
    s.healthPacks.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 10 });
  }
  // chest drop: 2% from normal, 20% from elite, 100% from boss
  const chestChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.2 : 0.02);
  if (Math.random() < chestChance) {
    s.chests.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 14 });
  }
  // swift boots
  if (s.player.artifacts.includes('swift_boots')) {
    s.player.swiftBootsTimer = 3;
  }
  // berserker mastery 5: close-range kill gives temporary speed via buffTimer
  if (getCharacterId(s) === 'berserker' && s.player.characterMasteryLevel >= 5 && dist(enemy.pos, s.player.pos) <= 110) {
    s.player.buffTimer = Math.max(s.player.buffTimer, 2);
  }
  // supernova evolution: sphere explodes on kill
  if (s.player.evolutions.includes('supernova')) {
    for (const e of s.enemies) {
      if (e !== enemy && e.hp > 0 && dist(e.pos, enemy.pos) < 90) {
        dealDamageToEnemy(s, e, 15 + s.player.level * 2);
      }
    }
    for (let i = 0; i < 15; i++) {
      s.particles.push({
        pos: { x: enemy.pos.x, y: enemy.pos.y },
        vel: { x: rand(-250, 250), y: rand(-250, 250) },
        life: 0.5, maxLife: 0.5, color: '#d4943d', size: rand(3, 6),
      });
    }
  }
  if (enemy.isBoss) {
    s.bossActive = false;
    s.bossDefeated++;
    const diff = DIFFICULTIES.find(d => d.id === s.difficulty)!;
    s.stats.goldEarned += Math.round((15 + s.wave * 1) * diff.goldMult);
    s.screenShake = 0.5;
    s.flashText = { text: 'BOSS DEFEATED!', life: 2, color: '#d4943d' };
    playSound('boss');
    // trigger artifact choice
    s.pendingArtifact = pickArtifacts(s);
  }
}

function pickArtifacts(s: GameState): ArtifactId[] {
  return pickArtifactChoices(s, 3);
}

function damagePlayer(s: GameState, amount: number): void {
  if (s.player.invulnerableTimer > 0) return;
  // dodge
  if (Math.random() < getDodgeChance(s)) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: 0, y: -60 }, life: 0.5, maxLife: 0.5, color: '#e8dcc0', size: 3 });
    return;
  }
  amount *= getArtifactDamageTakenMultiplier(s);
  if (Math.random() < getArtifactReflectChance(s)) {
    const nearest = s.enemies.reduce((best: EnemyEntity | null, enemy) => {
      if (enemy.hp <= 0) return best;
      const d = Math.hypot(enemy.pos.x - s.player.pos.x, enemy.pos.y - s.player.pos.y);
      if (d > 180) return best;
      if (!best) return enemy;
      const bestD = Math.hypot(best.pos.x - s.player.pos.x, best.pos.y - s.player.pos.y);
      return d < bestD ? enemy : best;
    }, null);
    if (nearest) dealDamageToEnemy(s, nearest, amount * 1.25);
  }
  // shield
  if (s.player.shieldCharges > 0) {
    s.player.shieldCharges--;
    const shieldBranch = getAbilityBranchId(s, 'shield', 4);
    const shieldFinal = getAbilityBranchId(s, 'shield', 7);
    if (shieldBranch === 'shield_reflector') {
      const nearest = s.enemies
        .filter((enemy) => enemy.hp > 0)
        .sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos))[0];
      if (nearest && dist(nearest.pos, s.player.pos) < 220) {
        dealDamageToEnemy(s, nearest, amount * 0.75);
      }
    }
    if (shieldFinal === 'shield_resonant_guard') {
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
    }
    for (let i = 0; i < 12; i++) {
      s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(-150, 150), y: rand(-150, 150) }, life: 0.4, maxLife: 0.4, color: '#4a7a8a', size: 3 });
    }
    return;
  }
  let dmg = amount * getDamageTakenMult(s);
  // mirror reflect
  if (s.player.artifacts.includes('mirror') && Math.random() < 0.2) {
    // reflect: find nearest enemy and damage
    const nearest = s.enemies.reduce((best, e) => {
      if (e.hp <= 0) return best;
      const d = dist(e.pos, s.player.pos);
      return d < (best?.dist ?? Infinity) ? { e, dist: d } : best;
    }, null as null | { e: EnemyEntity; dist: number });
    if (nearest) dealDamageToEnemy(s, nearest.e, dmg);
    return;
  }
  s.player.hp -= dmg;
  s.screenShake = Math.min(0.4, s.screenShake + 0.2);
  playSound('damage');
  // combo break on taking damage
  s.player.combo = 0;
  s.player.comboMult = 1;
  s.player.comboTimer = 0;
  // freeze amulet
  if (s.player.artifacts.includes('stasis_core')) {
    for (const e of s.enemies) {
      e.slowTimer = 2; e.slowFactor = 0.5;
    }
  }
  // invulnerability evolution
  if (s.player.evolutions.includes('invulnerability') && !s.player.invulnUsed && s.player.hp > 0 && s.player.hp / s.player.maxHp < 0.2) {
    s.player.invulnerableTimer = 3;
    s.player.invulnUsed = true;
    s.flashText = { text: 'INVULNERABLE!', life: 2, color: '#d4943d' };
  }
  if (s.player.hp <= 0) {
    s.player.hp = 0;
    s.gameOver = true;
    s.stats.time = s.time;
    playSound('gameover');
  }
}

// ===== Active abilities =====
function getAbilityBranchId(s: GameState, ability: AbilityType, stage: 4 | 7): string | null {
  const prefix = 'ability:' + ability + ':' + stage + ':';
  const marker = (s.player.evolutions || []).find((x: string) => x.startsWith(prefix));
  return marker ? marker.slice(prefix.length) : null;
}

function getNearestSphere(s: GameState, origin: Vec, predicate?: (sphere: SphereEntity) => boolean): SphereEntity | null {
  let nearest: SphereEntity | null = null;
  let best = Infinity;
  for (const sphere of s.spheres) {
    if (!sphere.alive || (predicate && !predicate(sphere))) continue;
    const d = dist(origin, sphere.pos);
    if (d < best) {
      best = d;
      nearest = sphere;
    }
  }
  return nearest;
}

function emitSpherePulse(s: GameState, sphere: SphereEntity, damage: number, radius: number, color: string, slow = false): void {
  for (const e of s.enemies) {
    if (e.hp <= 0 || dist(e.pos, sphere.pos) > radius) continue;
    dealDamageToEnemy(s, e, damage);
    if (slow) {
      e.slowTimer = Math.max(e.slowTimer, 1.2);
      e.slowFactor = Math.min(e.slowFactor, 0.6);
    }
  }
  for (let i = 0; i < 10; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({
      pos: { ...sphere.pos },
      vel: { x: Math.cos(a) * rand(60, 160), y: Math.sin(a) * rand(60, 160) },
      life: 0.45, maxLife: 0.45, color, size: rand(2, 5),
    });
  }
}

function activateBlast(s: GameState): void {
  const lvl = s.player.abilities.blast || 0;
  if (lvl === 0 || s.player.blastCooldown > 0) return;
  s.player.blastCooldown = Math.max(8, (30 - (lvl - 1) * 2) * getCooldownMult(s));

  const spheres = s.spheres.filter((sphere) => sphere.alive);
  const baseDamage = 30 + (lvl - 1) * 10;
  const radius = 125 + lvl * 12;
  const branch = getAbilityBranchId(s, 'blast', 4);
  const final = getAbilityBranchId(s, 'blast', 7);

  if (spheres.length === 0) {
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, s.player.pos) <= 180) dealDamageToEnemy(s, e, baseDamage);
    }
  } else {
    const ordered = [...spheres].sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
    let strength = 1;
    for (const sphere of ordered) {
      emitSpherePulse(s, sphere, baseDamage * strength, radius, '#c46d3d', final === 'blast_resonant_core');
      if (branch === 'blast_core') strength *= 1.12;
      if (branch === 'blast_network') strength *= 1.08;
      if (final === 'blast_echo_network') strength *= 1.15;
      if (final === 'blast_resonant_core' && sphere.type === 'standard') {
        emitSpherePulse(s, sphere, baseDamage * 0.45, radius * 0.75, '#d4943d');
      }
    }
    if (final === 'blast_infinite_pulse') {
      for (const sphere of [...ordered].reverse()) {
        emitSpherePulse(s, sphere, baseDamage * 0.35, radius * 0.75, '#d4943d');
      }
    }
  }

  if (branch === 'blast_core') {
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, s.player.pos) <= 100) dealDamageToEnemy(s, e, baseDamage * 0.5);
    }
  }
  if (branch === 'blast_resonance') {
    const standard = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'standard');
    for (const sphere of standard) emitSpherePulse(s, sphere, baseDamage * 0.25, 90, '#d4943d');
  }

  s.screenShake = 0.18;
  s.flashText = { text: 'ECHO PULSE', life: 0.9, color: '#c46d3d' };
}

function activateShield(s: GameState): void {
  const lvl = s.player.abilities.shield || 0;
  if (lvl === 0 || s.player.shieldCooldown > 0) return;
  s.player.shieldCooldown = 20 * getCooldownMult(s);
  const nearby = s.spheres.filter((sphere) => sphere.alive && dist(sphere.pos, s.player.pos) <= 260).length;
  const networkBonus = lvl >= 2 ? Math.min(3, Math.floor(nearby / 2)) : 0;
  const branchBonus = getAbilityBranchId(s, 'shield', 4) === 'shield_echo_guard' ? Math.min(2, Math.floor(nearby / 3)) : 0;
  s.player.shieldCharges = Math.min(5, 1 + Math.floor((lvl - 1) / 2) + networkBonus + branchBonus);
  s.player.shieldTimer = 10 + (lvl >= 5 ? 2 : 0);
  const branch = getAbilityBranchId(s, 'shield', 4);
  const final = getAbilityBranchId(s, 'shield', 7);

  if (branch === 'shield_echo_guard' || final === 'shield_network_guard') {
    for (const sphere of s.spheres) {
      if (sphere.alive && dist(sphere.pos, s.player.pos) <= 260) {
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.35);
      }
    }
  }
  if (branch === 'shield_bastion' || final === 'shield_iron_dome' || final === 'shield_resonant_guard') {
    if (branch === 'shield_bastion') s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
    if (final === 'shield_iron_dome') s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 2);
    for (const sphere of s.spheres) {
      if (sphere.alive && dist(sphere.pos, s.player.pos) <= 260) {
        s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.8, maxLife: 0.8, color: '#4a7a8a', size: 5 });
      }
    }
  }
  s.flashText = { text: 'SPHERE BARRIER', life: 0.9, color: '#4a7a8a' };
}

function doTeleportTo(s: GameState, target: Vec): void {
  const from = { ...s.player.pos };
  for (let i = 0; i < 16; i++) {
    s.particles.push({ pos: { ...from }, vel: { x: rand(-140, 140), y: rand(-140, 140) }, life: 0.45, maxLife: 0.45, color: '#5a8c4a', size: 3 });
  }
  s.player.pos.x = clamp(target.x, -s.worldWidth / 2, s.worldWidth / 2);
  s.player.pos.y = clamp(target.y, -s.worldHeight / 2, s.worldHeight / 2);
  for (let i = 0; i < 16; i++) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(-140, 140), y: rand(-140, 140) }, life: 0.45, maxLife: 0.45, color: '#5a8c4a', size: 3 });
  }
  const branch = getAbilityBranchId(s, 'teleport', 4);
  const final = getAbilityBranchId(s, 'teleport', 7);
  if (branch === 'teleport_phase' || final === 'teleport_phase_break') {
    s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 0.8);
  }
  if (final === 'teleport_phase_break') {
    const origin = from;
    const dx = s.player.pos.x - origin.x, dy = s.player.pos.y - origin.y;
    const distanceTravelled = Math.hypot(dx, dy);
    if (distanceTravelled > 140) {
      const steps = Math.max(1, Math.floor(distanceTravelled / 140));
      for (let i = 1; i < steps; i++) {
        const point = { x: origin.x + dx * (i / steps), y: origin.y + dy * (i / steps) };
        for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, point) < 70) dealDamageToEnemy(s, enemy, 14, undefined);
        s.particles.push({ pos: point, vel: { x: 0, y: 0 }, life: 0.35, maxLife: 0.35, color: '#5a8c4a', size: 5 });
      }
    }
  }
  if (final === 'teleport_spatial_network') {
    const destination = getNearestSphere(s, s.player.pos);
    if (destination) {
      for (const sphere of s.spheres) {
        if (sphere !== destination && sphere.alive && dist(sphere.pos, destination.pos) <= 220) {
          s.lightnings.push({ from: { ...destination.pos }, to: { ...sphere.pos }, life: 0.18 });
        }
      }
    }
  }
}

function activateTeleport(s: GameState): void {
  const lvl = s.player.abilities.teleport || 0;
  if (lvl === 0 || s.player.teleportCooldown > 0) return;
  const cd = (15 - Math.min(4, (lvl - 1) * 2)) * getCooldownMult(s);
  s.player.teleportCooldown = Math.max(5, cd);

  const branch = getAbilityBranchId(s, 'teleport', 4);
  const final = getAbilityBranchId(s, 'teleport', 7);
  const targetSphere = final === 'teleport_hunter_beacon' || branch === 'teleport_echo_jump'
    ? getNearestSphere(s, s.player.pos)
    : getNearestSphere(s, s.player.pos, (sphere) => dist(sphere.pos, s.player.pos) <= 700);

  if (targetSphere) {
    const target = { ...targetSphere.pos };
    doTeleportTo(s, target);
    if (branch === 'teleport_beacon') s.player.teleportDamageBuffTimer = 4;
    if (final === 'teleport_hunter_beacon') s.player.teleportDamageBuffTimer = 5;
  } else {
    doTeleportTo(s, { x: rand(s.player.pos.x - 300, s.player.pos.x + 300), y: rand(s.player.pos.y - 300, s.player.pos.y + 300) });
  }
  s.flashText = { text: 'ECHO JUMP', life: 0.9, color: '#5a8c4a' };
}

function activateFireTrail(s: GameState): void {
  const lvl = s.player.abilities.firetrail || 0;
  if (lvl === 0 || s.player.fireTrailCooldown > 0) return;
  s.player.fireTrailCooldown = 25 * getCooldownMult(s);
  s.player.fireTrailTimer = 5 + Math.min(3, lvl - 1);
  const branch = getAbilityBranchId(s, 'firetrail', 4);
  const final = getAbilityBranchId(s, 'firetrail', 7);
  const fireAligned = s.spheres.filter((sphere) => sphere.alive && s.player.sphereMods.fire > 0);
  for (const sphere of fireAligned) {
    sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.5);
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.9, maxLife: 0.9, color: '#c46d3d', size: 6 });
  }
  if (branch === 'firetrail_sanctum') {
    for (const sphere of s.spheres) {
      if (sphere.alive && sphere.type === 'aura') sphere.attackTimer = 0;
    }
  }
  if (branch === 'firetrail_ignition' || final === 'firetrail_catalyst') {
    for (const e of s.enemies) {
      if (e.hp > 0 && (e.fireTimer > 0 || e.poisonTimer > 0 || e.freezeTimer > 0)) {
        e.fireTimer = Math.max(e.fireTimer, 2);
        e.fireDps = Math.max(e.fireDps, 5 + lvl * 2);
      }
    }
  }
  s.flashText = { text: 'OVERHEAT', life: 0.9, color: '#c46d3d' };
}

function activateMinion(s: GameState): void {
  const lvl = s.player.abilities.minion || 0;
  if (lvl === 0 || s.player.minionCooldown > 0) return;
  s.player.minionCooldown = 30 * getCooldownMult(s);
  const count = 1 + Math.floor((lvl - 1) / 2);
  const branch = getAbilityBranchId(s, 'minion', 4);
  const final = getAbilityBranchId(s, 'minion', 7);
  for (let i = 0; i < count; i++) {
    const anchor = getNearestSphere(s, s.player.pos);
    const angle = (i / Math.max(1, count)) * Math.PI * 2;
    s.minions.push({
      pos: anchor ? { x: anchor.pos.x + Math.cos(angle) * 42, y: anchor.pos.y + Math.sin(angle) * 42 } : { ...s.player.pos },
      hp: 1, attackTimer: 0, life: 10 + (lvl >= 5 ? 2 : 0), radius: 12, damage: 6 + lvl * 2, rotation: 0,
    });
    if (anchor) {
      s.particles.push({ pos: { ...anchor.pos }, vel: { x: 0, y: 0 }, life: 0.7, maxLife: 0.7, color: '#4a7a8a', size: 5 });
    }
  }
  if (branch === 'minion_relay_drone' || final === 'minion_network_nodes') {
    const alive = s.spheres.filter((sphere) => sphere.alive);
    for (let i = 1; i < alive.length; i++) {
      s.lightnings.push({ from: { ...alive[i - 1].pos }, to: { ...alive[i].pos }, life: 0.12 });
    }
  }
  s.flashText = { text: 'ECHO DRONE', life: 0.9, color: '#4a7a8a' };
}

function activateLightning(s: GameState): void {
  const lvl = s.player.abilities.lightning || 0;
  if (lvl === 0 || s.player.lightningCooldown > 0) return;
  s.player.lightningCooldown = 20 * getCooldownMult(s);
  const branch = getAbilityBranchId(s, 'lightning', 4);
  const final = getAbilityBranchId(s, 'lightning', 7);
  const chainSpheres = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'chain');
  const ordered = [...chainSpheres].sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  const targets = s.enemies.filter((e) => e.hp > 0).sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos));
  if (targets.length === 0) return;
  const maxTargets = 1 + Math.floor((lvl - 1) / 2);
  let previous: Vec = { ...s.player.pos };
  let jump = 0;
  for (const sphere of ordered) {
    s.lightnings.push({ from: { ...previous }, to: { ...sphere.pos }, life: 0.24 });
    previous = { ...sphere.pos };
    jump++;
  }
  for (let i = 0; i < Math.min(maxTargets, targets.length); i++) {
    const target = targets[i];
    s.lightnings.push({ from: { ...previous }, to: { ...target.pos }, life: 0.3 });
    const damage = (40 + lvl * 15) * (1 + jump * 0.12);
    dealDamageToEnemy(s, target, damage);
    previous = { ...target.pos };
    jump++;
    if (branch === 'lightning_relay' || final === 'lightning_thunder_chain') {
      const next = targets[(i + 1) % targets.length];
      if (next && next !== target) s.lightnings.push({ from: { ...target.pos }, to: { ...next.pos }, life: 0.22 });
    }
  }
  if (branch === 'lightning_overload' || final === 'lightning_overload_core') {
    const last = targets[Math.min(maxTargets, targets.length) - 1];
    if (last) dealDamageToEnemy(s, last, 30 + lvl * 10);
  }
  if (final === 'lightning_storm_network' && ordered.length > 0) {
    for (let i = ordered.length - 1; i >= 0; i--) {
      const from = i > 0 ? ordered[i - 1].pos : s.player.pos;
      s.lightnings.push({ from: { ...from }, to: { ...ordered[i].pos }, life: 0.18 });
    }
  }
  s.flashText = { text: 'CHAIN LIGHTNING', life: 0.9, color: '#4a7a8a' };
}

function activateTimeStop(s: GameState): void {
  const lvl = s.player.abilities.timestop || 0;
  if (lvl === 0 || s.player.timestopCooldown > 0) return;
  s.player.timestopCooldown = 40 * getCooldownMult(s);
  s.player.timestopTimer = 3 + Math.min(2, lvl - 1);
  const nearest = getNearestSphere(s, s.player.pos);
  const branch = getAbilityBranchId(s, 'timestop', 4);
  const final = getAbilityBranchId(s, 'timestop', 7);
  const radius = nearest
    ? branch === 'timestop_closed_time' || final === 'timestop_closed_network' ? 760 : 520
    : Infinity;
  for (const e of s.enemies) {
    if (e.hp <= 0) continue;
    if (!nearest || dist(e.pos, nearest.pos) <= radius) {
      e.freezeTimer = s.player.timestopTimer + (branch === 'timestop_time_anchor' ? 1 : 0);
    }
  }
  if (final === 'timestop_closed_network') {
    for (const sphere of s.spheres) {
      if (!sphere.alive) continue;
      for (const e of s.enemies) {
        if (e.hp > 0 && dist(e.pos, sphere.pos) < 260) e.freezeTimer = Math.max(e.freezeTimer, s.player.timestopTimer);
      }
    }
  }
  if (nearest) {
    const pulseDamage = final === 'timestop_temporal_core' ? 10 + lvl * 8 : 10 + lvl * 4;
    emitSpherePulse(s, nearest, pulseDamage, 150, '#4a7a8a');
  }
  s.flashText = { text: 'ECHO FREEZE', life: 1.2, color: '#4a7a8a' };
}

function activateDarkRitual(s: GameState): void {
  const lvl = s.player.abilities.darkritual || 0;
  if (lvl === 0 || s.player.darkritualCooldown > 0) return;
  const hpCost = s.player.maxHp * 0.2;
  if (s.player.hp <= hpCost) return;
  s.player.darkritualCooldown = 30 * getCooldownMult(s);
  s.player.hp -= hpCost;
  s.player.overloadTimer = 5 + Math.min(3, lvl - 1);
  const branch = getAbilityBranchId(s, 'darkritual', 4);
  const final = getAbilityBranchId(s, 'darkritual', 7);
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.8);
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 1, maxLife: 1, color: '#8a5a8a', size: 7 });
  }
  if (branch === 'darkritual_blood_link' || final === 'darkritual_blood_network') {
    const standard = s.spheres.filter((sphere) => sphere.alive && sphere.type === 'standard');
    const target = getNearestSphere(s, s.player.pos, (sphere) => sphere.type === 'standard');
    if (branch === 'darkritual_blood_link' && target) target.attackTimer = Math.max(0, target.attackTimer - 1.4);
    for (const sphere of standard) {
      if (final === 'darkritual_blood_network' || branch === 'darkritual_blood_link') sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.8);
    }
  }
  if (branch === 'darkritual_void_pact' || final === 'darkritual_void_engine') {
    const ratio = s.player.hp / s.player.maxHp;
    if (ratio < 0.35) s.player.overloadTimer += 2;
  }
  if (branch === 'darkritual_sacrifice' || final === 'darkritual_sacrifice_core') {
    emitSpherePulse(s, getNearestSphere(s, s.player.pos) || { pos: { ...s.player.pos } } as SphereEntity, 20 + lvl * 5, 130, '#8a5a8a');
  }
  if (final === 'darkritual_void_engine' && s.player.hp / s.player.maxHp < 0.2) {
    for (const sphere of s.spheres) {
      if (sphere.alive) emitSpherePulse(s, sphere, 18 + lvl * 6, 110, '#8a5a8a');
    }
  }
  s.screenShake = 0.22;
  s.flashText = { text: 'OVERLOAD', life: 1.2, color: '#8a5a8a' };
}

export function activateByKey(s: GameState, key: string): void {
  const ability = s.activeKeyMap[key];
  if (!ability) return;
  switch (ability) {
    case 'blast': activateBlast(s); break;
    case 'shield': activateShield(s); break;
    case 'teleport': activateTeleport(s); break;
    case 'firetrail': activateFireTrail(s); break;
    case 'minion': activateMinion(s); break;
    case 'lightning': activateLightning(s); break;
    case 'timestop': activateTimeStop(s); break;
    case 'darkritual': activateDarkRitual(s); break;
  }
}

// ===== Upgrade generation =====
function getSphereEvolutionChoices(s:GameState, type:SphereType, level:4|7):UpgradeChoice[] {
  const def=SPHERE_PROGRESSION[type];
  if(level===4){
    return def.evolution4Choices.slice(0,3).map((branch)=>({
      type:'sphere' as const,
      sphereType:type,
      sphereBranch:branch.id,
      sphereStage:'branch' as const,
      name:{ru:branch.name.ru,en:branch.name.en},
      desc:branch.desc,
      currentLevel:4,
      newLevel:4,
    }));
  }
  const branchId=s.player.sphereBranches[type];
  const branch=def.evolution4Choices.find((x)=>x.id===branchId);
  if(!branch) return [];
  return branch.final.slice(0,3).map((finalChoice,index)=>({
    type:'sphere' as const,
    sphereType:type,
    sphereBranch:branch.id,
    sphereFinalIndex:index,
    sphereStage:'final' as const,
    name:finalChoice.name,
    desc:finalChoice.desc,
    currentLevel:7,
    newLevel:7,
  }));
}

function getAbilityEvolutionChoices(s:GameState, ability:AbilityType, level:4|7):UpgradeChoice[] {
  const progression=ABILITY_PROGRESSION[ability];
  if(!progression) return [];
  const pool=level===4?progression.evolution4:progression.evolution7;
  return pool.slice(0,3).map((evolution,index)=>({
    type:'ability' as const,
    ability,
    abilityEvolutionIndex:index,
    abilityStage:level===4?'branch' as const:'final' as const,
    name:evolution.name,
    desc:evolution.desc,
    currentLevel:level,
    newLevel:level,
  }));
}

export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {
  const sphereTypes=Object.keys(SPHERE_PROGRESSION) as SphereType[];
  const availableSpheres=sphereTypes.filter((type)=>sphereLevel(s,type)<7);
  const sphereChoices=availableSpheres.map((type)=>{
    const currentLevel=sphereLevel(s,type);
    const nextLevel=currentLevel+1;
    const def=SPHERE_PROGRESSION[type];
    const levelDef=def.levels[nextLevel-1];
    const branchId=s.player.sphereBranches[type];
    const branch=branchId?def.evolution4Choices.find((x)=>x.id===branchId):null;
    const branchProgress=(nextLevel===5||nextLevel===6)&&!!branch;
    const isFirstMutation=nextLevel===4;
    const isFinalMutation=nextLevel===7;

    let nameRu=def.name.ru+' — уровень '+nextLevel;
    let nameEn=def.name.en+' — level '+nextLevel;
    let descRu=levelDef.desc.ru;
    let descEn=levelDef.desc.en;

    if(isFirstMutation){
      nameRu+=': Мутация I';
      nameEn+=': Mutation I';
      descRu='Повышает сферу до IV уровня. После выбора откроется отдельное окно с 3 мутациями, из которых можно выбрать одну.';
      descEn='Raises the sphere to level IV. After this choice, a separate window opens with 3 mutations and you choose one.';
    } else if(isFinalMutation){
      nameRu+=': Мутация II';
      nameEn+=': Mutation II';
      descRu='Повышает сферу до VII уровня. После этого откроется отдельное окно с 3 финальными специализациями.';
      descEn='Raises the sphere to level VII. After this choice, a separate window opens with 3 final specializations.';
    } else if(branchProgress){
      descRu=nextLevel===5?branch!.level5.ru:branch!.level6.ru;
      descEn=nextLevel===5?branch!.level5.en:branch!.level6.en;
      nameRu+=': ветка «'+branch!.name.ru+'»';
      nameEn+=': branch “'+branch!.name.en+'”';
    }

    return {
      type:'sphere' as const,
      sphereType:type,
      sphereBranch:branchId,
      sphereStage:'upgrade' as const,
      currentLevel,
      newLevel:nextLevel,
      name:{ru:nameRu,en:nameEn},
      desc:{ru:descRu,en:descEn},
    };
  });

  const preferredAbilities=CHARACTER_DEFS[s.player.characterId]?.preferredAbilities??[];
  const activeIds=(Object.keys(ABILITIES) as AbilityType[])
    .filter((id)=>ABILITIES[id].category==='active'&&(s.player.abilities[id]||0)<ABILITIES[id].maxLevel);

  const activeChoices=activeIds.map((id)=>{
    const level=s.player.abilities[id]||0;
    const nextLevel=level+1;
    const progression=ABILITY_PROGRESSION[id];
    const levelDef=progression?.levels[nextLevel-1];
    const preferred=preferredAbilities.includes(id);
    let name=levelDef?.name??ABILITIES[id].name;
    let desc=levelDef?.desc??{ru:ABILITIES[id].desc.ru(nextLevel),en:ABILITIES[id].desc.en(nextLevel)};

    if(nextLevel<=3){
      name={ru:ABILITIES[id].name.ru+' — уровень '+nextLevel,en:ABILITIES[id].name.en+' — level '+nextLevel};
    } else if(nextLevel===4){
      name={ru:ABILITIES[id].name.ru+' — уровень IV: Мутация I',en:ABILITIES[id].name.en+' — level IV: Mutation I'};
      desc={
        ru:'Повышает способность до IV уровня. После выбора откроется отдельное окно с 3 ветками мутации.',
        en:'Raises the ability to level IV. After this choice, a separate window opens with 3 mutation branches.',
      };
    } else if(nextLevel===7){
      name={ru:ABILITIES[id].name.ru+' — уровень VII: Мутация II',en:ABILITIES[id].name.en+' — level VII: Mutation II'};
      desc={
        ru:'Повышает способность до VII уровня. После этого откроется отдельное окно с 3 финальными формами.',
        en:'Raises the ability to level VII. After this choice, a separate window opens with 3 final forms.',
      };
    } else if(level>=5&&progression){
      const branch=s.player.evolutions?.find((x:string)=>x.startsWith('ability:'+id+':4:'));
      if(branch){
        const evolutionId=branch.split(':').slice(3).join(':');
        const selected=progression.evolution4.find((x)=>x.id===evolutionId);
        if(selected){
          desc=progression.levels[nextLevel-1].desc;
          name={ru:progression.levels[nextLevel-1].name.ru+' · '+selected.name.ru,en:progression.levels[nextLevel-1].name.en+' · '+selected.name.en};
        }
      }
    }

    return {
      id,
      score:Math.random()*(preferred?1.35:0.75),
      choice:{
        type:'ability' as const,
        ability:id,
        abilityStage:'upgrade' as const,
        name,
        desc,
        currentLevel:level,
        newLevel:nextLevel,
      },
    };
  }).sort((a,b)=>b.score-a.score).map((item)=>item.choice);

  if(sphereChoices.length>=2&&activeChoices.length>0){
    const firstTwo=sphereChoices.slice(0,2);
    const active=activeChoices[Math.floor(Math.random()*Math.min(3,activeChoices.length))];
    return [firstTwo[0],firstTwo[1],active].sort(()=>Math.random()-0.5);
  }
  if(sphereChoices.length===1&&activeChoices.length>0){
    return [sphereChoices[0],...activeChoices.slice(0,2)].slice(0,3).sort(()=>Math.random()-0.5);
  }
  if(sphereChoices.length>0) return sphereChoices.slice(0,3);
  return activeChoices.slice(0,3);
}

export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {
  s.pendingUpgrade=null;

  if(choice.type==='sphere'&&choice.sphereType){
    const type=choice.sphereType;
    const current=sphereLevel(s,type);

    // Mutation choices are a second, separate decision. They do not advance
    // the level again because the normal upgrade already moved the sphere to IV/VII.
    if(choice.sphereStage==='branch'&&choice.sphereBranch){
      if(current!==4) return;
      s.player.sphereBranches[type]=choice.sphereBranch;
      s.player.evolutions.push('sphere:'+type+':4:'+choice.sphereBranch);
      s.evolutionsThisRun++;
      s.flashText={text:choice.name?.ru??'Мутация сферы I',life:2.2,color:'#d4943d'};
      playSound('evolve');
      return;
    }

    if(choice.sphereStage==='final'){
      if(current!==7) return;
      s.player.evolutions.push('sphere:'+type+':7:'+(choice.sphereBranch??'unknown')+':'+(choice.sphereFinalIndex??0));
      s.evolutionsThisRun++;
      s.flashText={text:choice.name?.ru??'Мутация сферы II',life:2.2,color:'#c4453d'};
      playSound('evolve');
      return;
    }

    if(current>=7) return;
    const next=current+1;
    s.player.sphereProgression[type]=next;
    for(const sphere of s.spheres) if(sphere.type===type) sphere.visualTier=next;

    if(next===4){
      s.pendingUpgrade=getSphereEvolutionChoices(s,type,4);
    } else if(next===7){
      s.pendingUpgrade=getSphereEvolutionChoices(s,type,7);
    }
    return;
  }

  if(choice.type==='ability'&&choice.ability){
    const ability=choice.ability;
    const current=s.player.abilities[ability]||0;

    if(choice.abilityStage==='branch'){
      if(current!==4) return;
      const progression=ABILITY_PROGRESSION[ability];
      const evolution=progression?.evolution4[Math.max(0,Math.min((progression?.evolution4.length||1)-1,choice.abilityEvolutionIndex??0))];
      if(evolution){
        const marker='ability:'+ability+':4:'+evolution.id;
        s.player.evolutions.push(marker);
        s.evolutionsThisRun++;
        s.flashText={text:evolution.name.ru,life:2.2,color:'#d4943d'};
        playSound('evolve');
      }
      return;
    }

    if(choice.abilityStage==='final'){
      if(current!==7) return;
      const progression=ABILITY_PROGRESSION[ability];
      const evolution=progression?.evolution7[Math.max(0,Math.min((progression?.evolution7.length||1)-1,choice.abilityEvolutionIndex??0))];
      if(evolution){
        const marker='ability:'+ability+':7:'+evolution.id;
        s.player.evolutions.push(marker);
        s.evolutionsThisRun++;
        s.flashText={text:evolution.name.ru,life:2.2,color:'#c4453d'};
        playSound('evolve');
      }
      return;
    }

    if(current>=7) return;
    const next=Math.min(7,current+1);
    s.player.abilities[ability]=next;
    const def=ABILITIES[ability];
    if(def.category==='active'&&current===0) assignHotkey(s,ability);

    if(next===4){
      s.pendingUpgrade=getAbilityEvolutionChoices(s,ability,4);
    } else if(next===7){
      s.pendingUpgrade=getAbilityEvolutionChoices(s,ability,7);
    }
    return;
  }
}

export function applySphereUpgrade(s: GameState, choice: SphereUpgradeChoice): void {
  const type = (choice as SphereUpgradeChoice & { sphereType?: SphereType }).sphereType;
  if (!type) return;
  applyUpgrade(s, { type: 'sphere', sphereType: type, sphereStage: 'upgrade', sphereBranch: s.player.sphereBranches[type], currentLevel: sphereLevel(s, type), newLevel: sphereLevel(s, type) + 1 });
}

export function applyArtifact(s: GameState, id: ArtifactId): void {
  if (s.player.artifacts.includes(id)) return;
  s.player.artifacts.push(id);
  const hpBonus = getArtifactMaxHpBonus(id);
  if (hpBonus > 0) { s.player.maxHp += hpBonus; s.player.hp += hpBonus; }
  playSound('chest');
}

// ===== Main update =====
export function update(s: GameState, dt: number): void {
  if (s.paused || s.gameOver) return;
  if (s.pendingUpgrade || s.pendingArtifact) return;

  s.time += dt;
  s.stats.time = s.time;

  // character timers
  if (s.player.hunterMarkTimer > 0) {
    s.player.hunterMarkTimer -= dt;
    if (s.player.hunterMarkTimer <= 0) {
      s.player.hunterMarkTimer = 0;
      s.player.hunterMarkTarget = null;
      s.player.hunterHitCount = 0;
    }
  }
  if (s.player.hunterHuntTimer > 0) {
    s.player.hunterHuntTimer -= dt;
    if (s.player.hunterHuntTimer <= 0) {
      s.player.hunterHuntTimer = 0;
      s.player.hunterHuntTarget = null;
    }
  }
  if (s.player.hunterTrophyTimer > 0) s.player.hunterTrophyTimer = Math.max(0, s.player.hunterTrophyTimer - dt);
  if (s.player.engineerRelayTimer > 0) {
    s.player.engineerRelayTimer -= dt;
    if (s.player.engineerRelayTimer <= 0) {
      s.player.engineerRelayTimer = 0;
      s.player.engineerRelaySource = null;
    }
  }
  if (s.player.alchemistCatalystTimer > 0) s.player.alchemistCatalystTimer = Math.max(0, s.player.alchemistCatalystTimer - dt);

  // combo timer
  if (s.player.comboTimer > 0) {
    s.player.comboTimer -= dt;
    if (s.player.comboTimer <= 0) {
      s.player.combo = 0;
      s.player.comboMult = 1;
    }
  }
  // buff timer
  if (s.player.buffTimer > 0) s.player.buffTimer -= dt;
  // dash cooldown
  if (s.player.dashCooldown > 0) s.player.dashCooldown -= dt;
  // dash active
  if (s.player.dashTimer > 0) {
    s.player.dashTimer -= dt;
    s.player.pos.x += s.player.dashDir.x * 600 * dt;
    s.player.pos.y += s.player.dashDir.y * 600 * dt;
    s.player.pos.x = clamp(s.player.pos.x, -s.worldWidth / 2, s.worldWidth / 2);
    s.player.pos.y = clamp(s.player.pos.y, -s.worldHeight / 2, s.worldHeight / 2);
    // dash trail particles
    if (Math.random() < 0.5) {
      s.particles.push({ pos: { ...s.player.pos }, vel: { x: 0, y: 0 }, life: 0.3, maxLife: 0.3, color: '#d4943d', size: 3 });
    }
  }

  // damage numbers
  for (let i = s.damageNumbers.length - 1; i >= 0; i--) {
    const dn = s.damageNumbers[i];
    dn.pos.x += dn.vel.x * dt;
    dn.pos.y += dn.vel.y * dt;
    dn.vel.y += 80 * dt;
    dn.life -= dt;
    if (dn.life <= 0) s.damageNumbers.splice(i, 1);
  }

  // chests pickup
  for (let i = s.chests.length - 1; i >= 0; i--) {
    const chest = s.chests[i];
    if (!chest.alive) { s.chests.splice(i, 1); continue; }
    if (dist(chest.pos, s.player.pos) < PLAYER_RADIUS + chest.radius) {
      chest.alive = false;
      s.player.chestOpens++;
      s.pendingArtifact = pickArtifacts(s);
      s.chests.splice(i, 1);
      playSound('chest');
    }
  }

  // mutation check
  checkMutation(s);

  // player movement
  let mx = 0, my = 0;
  if (s.keys['w'] || s.keys['arrowup']) my -= 1;
  if (s.keys['s'] || s.keys['arrowdown']) my += 1;
  if (s.keys['a'] || s.keys['arrowleft']) mx -= 1;
  if (s.keys['d'] || s.keys['arrowright']) mx += 1;
  const len = Math.hypot(mx, my);
  if (len > 0) { mx /= len; my /= len; }
  const sp = getMoveSpeed(s);
  s.player.pos.x += mx * sp * dt;
  s.player.pos.y += my * sp * dt;
  s.player.pos.x = clamp(s.player.pos.x, -s.worldWidth / 2, s.worldWidth / 2);
  s.player.pos.y = clamp(s.player.pos.y, -s.worldHeight / 2, s.worldHeight / 2);

  // fire trail
  if (s.player.fireTrailTimer > 0) s.player.fireTrailTimer = Math.max(0, s.player.fireTrailTimer - dt);

  const artifactRegen = getArtifactRegenPerSecond(s);
  if (artifactRegen > 0) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + artifactRegen * dt);
  }
  // mutation stage 4: +25% regen
  if (s.player.mutationStage >= 4) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + dt * 1.25);
  }

  // chaos orb
  if (s.player.artifacts.includes('chaos_orb')) {
    s.player.chaosOrbTimer += dt;
    if (s.player.chaosOrbTimer >= 10) {
      s.player.chaosOrbTimer = 0;
      s.player.chaosOrbBuff = Math.random() < 0.5 ? 'dmg' : 'radius';
      s.player.chaosOrbBuffTimer = 3;
    }
  }
  if (s.player.chaosOrbBuffTimer > 0) s.player.chaosOrbBuffTimer -= dt;

  // cooldowns
  const p = s.player;
  if (p.blastCooldown > 0) p.blastCooldown = Math.max(0, p.blastCooldown - dt);
  if (p.teleportCooldown > 0) p.teleportCooldown = Math.max(0, p.teleportCooldown - dt);
  if (p.shieldCooldown > 0) p.shieldCooldown = Math.max(0, p.shieldCooldown - dt);
  if (p.minionCooldown > 0) p.minionCooldown = Math.max(0, p.minionCooldown - dt);
  if (p.lightningCooldown > 0) p.lightningCooldown = Math.max(0, p.lightningCooldown - dt);
  if (p.timestopCooldown > 0) p.timestopCooldown = Math.max(0, p.timestopCooldown - dt);
  if (p.darkritualCooldown > 0) p.darkritualCooldown = Math.max(0, p.darkritualCooldown - dt);
  if (p.overloadTimer > 0) p.overloadTimer = Math.max(0, p.overloadTimer - dt);
  if (p.fireTrailCooldown > 0) p.fireTrailCooldown = Math.max(0, p.fireTrailCooldown - dt);
  if (s.player.shieldTimer > 0) s.player.shieldTimer -= dt;
  if (s.player.swiftBootsTimer > 0) s.player.swiftBootsTimer -= dt;
  if (s.player.teleportDamageBuffTimer > 0) s.player.teleportDamageBuffTimer -= dt;
  if (s.player.invulnerableTimer > 0) s.player.invulnerableTimer -= dt;
  if (s.player.dodgeTimer > 0) s.player.dodgeTimer -= dt;

  // spheres
  updateSpheres(s, dt);

  // minions
  updateMinions(s, dt);

  // waves
  s.waveTimer -= dt;
  if (s.waveTimer <= 0 && s.waveEnemiesToSpawn > 0) {
    s.enemies.push(spawnEnemy(s, false));
    s.waveEnemiesToSpawn--;
    s.waveTimer = Math.max(0.3, (1.2 - s.wave * 0.02) / (DIFFICULTIES.find(d => d.id === s.difficulty)?.spawnRateMult || 1));
  }
  if (s.waveEnemiesToSpawn <= 0 && s.enemies.filter(e => !e.isBoss).length === 0 && !s.bossActive) {
    s.waveTimer = 3;
    startWave(s);
  }
  // initial wave
  if (s.wave === 0) {
    startWave(s);
  }

  // enemies
  updateEnemies(s, dt);

  // xp orbs
  updateXpOrbs(s, dt);

  // health packs
  updateHealthPacks(s);

  // particles
  for (let i = s.particles.length - 1; i >= 0; i--) {
    const p = s.particles[i];
    p.pos.x += p.vel.x * dt; p.pos.y += p.vel.y * dt;
    p.vel.x *= 0.95; p.vel.y *= 0.95;
    p.life -= dt;
    if (p.life <= 0) s.particles.splice(i, 1);
  }

  // fire trails
  for (let i = s.fireTrails.length - 1; i >= 0; i--) {
    const ft = s.fireTrails[i];
    ft.life -= dt;
    if (ft.life <= 0) { s.fireTrails.splice(i, 1); continue; }
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, ft.pos) < 25) {
        dealDamageToEnemy(s, e, ft.damage * dt * 3);
        // ice path evolution: freeze
        if (s.player.evolutions.includes('icepath')) {
          e.freezeTimer = 2;
        }
        // synergy: firetrail + slow -> extra slow
        if ((s.player.abilities.slow || 0) > 0) {
          e.slowTimer = 2; e.slowFactor = 0.7;
        }
      }
    }
  }

  // lightnings
  for (let i = s.lightnings.length - 1; i >= 0; i--) {
    s.lightnings[i].life -= dt;
    if (s.lightnings[i].life <= 0) s.lightnings.splice(i, 1);
  }

  // screen shake
  if (s.screenShake > 0) s.screenShake = Math.max(0, s.screenShake - dt);
  if (s.flashText) {
    s.flashText.life -= dt;
    if (s.flashText.life <= 0) s.flashText = null;
  }

  // camera
  s.camera.x = s.player.pos.x;
  s.camera.y = s.player.pos.y;

  // boss arrow
  s.bossArrow = null;
  if (s.player.artifacts.includes('foresight_eye')) {
    const boss = s.enemies.find(e => e.isBoss && e.hp > 0);
    if (boss) {
      const dx = boss.pos.x - s.player.pos.x;
      const dy = boss.pos.y - s.player.pos.y;
      const d = Math.hypot(dx, dy);
      if (d > 0) s.bossArrow = { x: dx / d, y: dy / d };
    }
  }
}

function checkMutation(s: GameState): void {
  const lvl = s.player.level;
  let stage = 0;
  if (lvl >= 40) stage = 4;
  else if (lvl >= 30) stage = 3;
  else if (lvl >= 20) stage = 2;
  else if (lvl >= 10) stage = 1;
  if (stage > s.player.mutationStage) {
    s.player.mutationStage = stage;
    s.flashText = { text: 'MUTATION!', life: 2, color: '#b8475a' };
    for (let i = 0; i < 40; i++) {
      const a = Math.random() * Math.PI * 2;
      s.particles.push({
        pos: { ...s.player.pos },
        vel: { x: Math.cos(a) * rand(100, 250), y: Math.sin(a) * rand(100, 250) },
        life: 1, maxLife: 1, color: ['#8a5a8a', '#c4453d', '#d4943d', '#e8dcc0'][stage - 1], size: rand(3, 6),
      });
    }
  }
}

function updateSpheres(s: GameState, dt: number): void {
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    const stype = SPHERE_TYPES[sphere.type];
    const radius = getSphereRadius(s, sphere) * stype.rangeMult;
    const damage = getSphereDamage(s, sphere) * stype.damageMult;
    const delay = getSphereDelay(s) * stype.delayMult * sphereModifiers(s, sphere.type).delay;
    // aura type: continuous AoE damage — no barrel rotation
    if (stype.aura) {
      sphere.auraTimer -= dt;
      if (sphere.auraTimer <= 0) {
        sphere.auraTimer = sphereModifiers(s, sphere.type).auraPulse;
        let attacked = false;
        for (const e of s.enemies) {
          if (e.hp <= 0) continue;
          const sphereStats = sphereModifiers(s, sphere.type);
          if (dist(e.pos, sphere.pos) < stype.auraRadius * sphereStats.auraRadius) {
            const branch = s.player.sphereBranches?.[sphere.type];
            if (branch === 'aura_sanctum') {
              e.slowTimer = Math.max(e.slowTimer, 0.8);
              e.slowFactor = Math.min(e.slowFactor, 0.65);
            } else if (branch === 'aura_gravity') {
              const dx = sphere.pos.x - e.pos.x, dy = sphere.pos.y - e.pos.y;
              const d = Math.hypot(dx, dy) || 1;
              e.pos.x += dx / d * 28 * dt;
              e.pos.y += dy / d * 28 * dt;
            } else if (branch === 'aura_sanctum') {
              for (const ally of s.spheres) {
                if (ally !== sphere && ally.alive && dist(ally.pos, sphere.pos) < 120) {
                  ally.attackTimer = Math.max(0, ally.attackTimer - dt * 0.12);
                }
              }
            } else if (branch === 'aura_overgrowth') {
              for (const ally of s.spheres) {
                if (ally !== sphere && ally.alive && dist(ally.pos, sphere.pos) < 110) {
                  ally.attackTimer = Math.max(0, ally.attackTimer - dt * 0.08);
                }
              }
            }
            dealDamageToEnemy(s, e, damage, sphere);
            attacked = true;
          }
        }
        if (attacked) triggerEngineerRelay(s, sphere);
      }
      continue;
    }
    // find nearest enemy for aiming (every frame, not just on fire)
    let nearest: EnemyEntity | null = null;
    let nd = Infinity;
    for (const e of s.enemies) {
      if (e.hp <= 0) continue;
      const d = dist(e.pos, sphere.pos);
      if (d < radius && d < nd) { nd = d; nearest = e; }
    }
    // aim turret at nearest enemy; stay static if no enemies
    if (nearest) {
      const adx = nearest.pos.x - sphere.pos.x;
      const ady = nearest.pos.y - sphere.pos.y;
      sphere.rotation = Math.atan2(ady, adx);
    }
    sphere.attackTimer -= dt;
    if (sphere.attackTimer <= 0) {
      if (nearest) {
        sphere.attackTimer = delay;
        const dx = nearest.pos.x - sphere.pos.x;
        const dy = nearest.pos.y - sphere.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        const dirX = dx / d;
        const dirY = dy / d;
        const mods = s.player.sphereMods;
        const shots = (1 + mods.multishot + (sphere.type === 'shotgun' ? sphereModifiers(s, sphere.type).multishot : 0)) * stype.pellets;
        const relayMultiplier = consumeEngineerRelayBonus(s, sphere);
        const formation = getCharacterFormation(s);
        const formationPierce = getCharacterId(s) === 'architect' && formation.type === 'line' ? 1 : 0;
        for (let i = 0; i < shots; i++) {
          const spread = shots > 1 ? (i - (shots - 1) / 2) * ((stype.spread * (sphereModifiers(s, sphere.type).spreadMult || 1)) / Math.max(1, shots - 1) || 0.15) : 0;
          const a = Math.atan2(dirY, dirX) + spread;
          let effect: 'none' | 'fire' | 'freeze' | 'poison' = 'none';
          if (mods.fire > 0) effect = 'fire';
          else if (mods.freeze > 0) effect = 'freeze';
          else if (mods.poison > 0) effect = 'poison';
          let color = stype.color;
          if (effect === 'fire') color = '#c46d3d';
          else if (effect === 'freeze') color = '#4a7a8a';
          else if (effect === 'poison') color = '#5a8c4a';
          const speed = 350 * stype.projectileSpeedMult;
          s.sphereProjectiles.push({
            pos: { ...sphere.pos },
            vel: { x: Math.cos(a) * speed, y: Math.sin(a) * speed },
            damage: damage * relayMultiplier,
            radius: 5,
            alive: true,
            color,
            pierce: mods.pierce + formationPierce + (stype.chain ? Math.max(1, sphereModifiers(s, 'chain').chainTargets) : sphereModifiers(s, sphere.type).pierce),
            hitEnemies: new Set(),
            effect,
            ricochet: mods.ricochet,
            life: 2,
            sourceSphere: sphere,
          });
          // chain lightning: instantly hit nearby enemies
          if (stype.chain) {
            const chainTargets: EnemyEntity[] = [];
            let current = nearest;
            const hitSet = new Set<EnemyEntity>([current]);
            for (let c = 0; c < Math.max(0, sphereModifiers(s, sphere.type).chainTargets); c++) {
              let next: EnemyEntity | null = null;
              let cd2 = Infinity;
              for (const e2 of s.enemies) {
                if (e2.hp <= 0 || hitSet.has(e2)) continue;
                const dd = dist(e2.pos, current.pos);
                if (dd < 150 && dd < cd2) { cd2 = dd; next = e2; }
              }
              if (!next) break;
              chainTargets.push(next);
              hitSet.add(next);
              current = next;
            }
            // apply damage to chain targets
            for (const ct of chainTargets) {
              dealDamageToEnemy(s, ct, damage * 0.7 * relayMultiplier, sphere);
              s.lightnings.push({ from: { ...nearest.pos }, to: { ...ct.pos }, life: 0.3 });
            }
          }
        }
        triggerEngineerRelay(s, sphere);
        playSound('shoot');
      }
    }
  }
  // update traveling projectiles
  for (let i = s.sphereProjectiles.length - 1; i >= 0; i--) {
    const p = s.sphereProjectiles[i];
    p.pos.x += p.vel.x * dt;
    p.pos.y += p.vel.y * dt;
    p.life -= dt;
    let hit = false;
    for (const e of s.enemies) {
      if (e.hp <= 0 || p.hitEnemies.has(e)) continue;
      if (dist(p.pos, e.pos) < p.radius + e.radius) {
        dealDamageToEnemy(s, e, p.damage, p.sourceSphere);
        p.hitEnemies.add(e);
        hit = true;
        // impact effect particles
        for (let k = 0; k < 6; k++) {
          const a = Math.random() * Math.PI * 2;
          s.particles.push({ pos: { ...p.pos }, vel: { x: Math.cos(a) * 80, y: Math.sin(a) * 80 }, life: 0.3, maxLife: 0.3, color: p.color, size: 2 });
        }
        // apply status effects
        if (p.effect === 'fire') {
          let duration = 3 * getCharacterStatusDurationMultiplier(s);
          let dps = (5 + s.player.sphereMods.fire * 3) * getCharacterStatusDamageMultiplier(s);
          if (getCharacterId(s) === 'alchemist' && s.player.characterMasteryLevel >= 3) dps *= 1.05;
          if (getCharacterId(s) === 'alchemist' && s.player.alchemistCatalystTimer > 0) {
            duration *= 1.5;
            s.player.alchemistCatalystTimer = 0;
          }
          e.fireTimer = (e.fireTimer || 0) + duration;
          e.fireDps = dps;
        } else if (p.effect === 'freeze') {
          let duration = (0.5 + s.player.sphereMods.freeze * 0.3) * getCharacterStatusDurationMultiplier(s);
          if (getCharacterId(s) === 'alchemist' && s.player.alchemistCatalystTimer > 0) {
            duration *= 1.5;
            s.player.alchemistCatalystTimer = 0;
          }
          e.freezeTimer = Math.max(e.freezeTimer || 0, duration);
        } else if (p.effect === 'poison') {
          let duration = 4 * getCharacterStatusDurationMultiplier(s);
          let dps = (3 + s.player.sphereMods.poison * 2) * getCharacterStatusDamageMultiplier(s);
          if (getCharacterId(s) === 'alchemist' && s.player.characterMasteryLevel >= 3) dps *= 1.05;
          if (getCharacterId(s) === 'alchemist' && s.player.alchemistCatalystTimer > 0) {
            duration *= 1.5;
            s.player.alchemistCatalystTimer = 0;
          }
          e.poisonTimer = (e.poisonTimer || 0) + duration;
          e.poisonDps = dps;
        }
        if (p.effect !== 'none' && applyAlchemistReaction(s, e) && e.hp <= 0) {
          onEnemyDeath(s, e);
        }
        if (p.pierce > 0) {
          p.pierce--;
          hit = false;
        } else if (p.ricochet > 0) {
          p.ricochet--;
          // find new target
          let next: EnemyEntity | null = null;
          let nd = Infinity;
          for (const e2 of s.enemies) {
            if (e2.hp <= 0 || p.hitEnemies.has(e2)) continue;
            const d = dist(p.pos, e2.pos);
            if (d < nd) { nd = d; next = e2; }
          }
          if (next) {
            const dx = next.pos.x - p.pos.x;
            const dy = next.pos.y - p.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            p.vel.x = (dx / d) * 350;
            p.vel.y = (dy / d) * 350;
            hit = false;
          } else { hit = true; }
        } else { hit = true; }
        break;
      }
    }
    if (hit || p.life <= 0) s.sphereProjectiles.splice(i, 1);
  }
}

function updateMinions(s: GameState, dt: number): void {
  for (let i = s.minions.length - 1; i >= 0; i--) {
    const m = s.minions[i];
    m.life -= dt;
    m.rotation += dt * 3;
    if (m.life <= 0) { s.minions.splice(i, 1); continue; }

    const anchor = getNearestSphere(s, m.pos);
    if (anchor) {
      const angle = m.rotation * 0.7 + i * 2.1;
      const targetX = anchor.pos.x + Math.cos(angle) * 48;
      const targetY = anchor.pos.y + Math.sin(angle) * 48;
      m.pos.x += (targetX - m.pos.x) * Math.min(1, dt * 4);
      m.pos.y += (targetY - m.pos.y) * Math.min(1, dt * 4);

      const abilityBranch = getAbilityBranchId(s, 'minion', 4);
      const abilityFinal = getAbilityBranchId(s, 'minion', 7);
      const nearby = getNearestSphere(s, m.pos, (sphere) => sphere !== anchor && dist(sphere.pos, m.pos) < 240);
      if (abilityBranch === 'minion_relay_drone' && nearby && Math.random() < dt * 4) {
        s.lightnings.push({ from: { ...anchor.pos }, to: { ...nearby.pos }, life: 0.1 });
        anchor.attackTimer = Math.max(0, anchor.attackTimer - 0.16);
        nearby.attackTimer = Math.max(0, nearby.attackTimer - 0.08);
      } else if (abilityBranch === 'minion_guardian') {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - dt * 0.18);
      } else if (nearby && Math.random() < dt * 2) {
        s.lightnings.push({ from: { ...anchor.pos }, to: { ...nearby.pos }, life: 0.08 });
      }
      if (abilityFinal === 'minion_sphere_guard') {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - dt * 0.12);
      }
    } else {
      const dx = s.player.pos.x - m.pos.x;
      const dy = s.player.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      m.pos.x += dx / d * 90 * dt;
      m.pos.y += dy / d * 90 * dt;
    }

    m.attackTimer -= dt;
    if (m.attackTimer <= 0) {
      let nearest: EnemyEntity | null = null;
      let nd = Infinity;
      for (const e of s.enemies) {
        if (e.hp <= 0) continue;
        const d = dist(e.pos, m.pos);
        if (d < 180 && d < nd) { nd = d; nearest = e; }
      }
      if (nearest) {
        const dmg = (5 + (s.player.abilities.minion || 0) * 2) * (s.player.overloadTimer > 0 ? 1.25 : 1);
        dealDamageToEnemy(s, nearest, dmg);
        m.attackTimer = 0.8;
      }
    }
  }
}

function updateEnemies(s: GameState, dt: number): void {
  const slowLvl = s.player.abilities.slow || 0;
  for (let i = s.enemies.length - 1; i >= 0; i--) {
    const e = s.enemies[i];
    if (e.hp <= 0) { s.enemies.splice(i, 1); continue; }
    e.rotation += dt;
    if (e.hitFlash > 0) e.hitFlash -= dt;
    // DoT: fire
    if (e.fireTimer > 0) {
      e.fireTimer -= dt;
      e.hp -= e.fireDps * dt;
      if (Math.random() < 0.3) {
        s.particles.push({ pos: { x: e.pos.x + rand(-e.radius, e.radius), y: e.pos.y + rand(-e.radius, e.radius) }, vel: { x: 0, y: -30 }, life: 0.3, maxLife: 0.3, color: '#c46d3d', size: 2 });
      }
      if (e.hp <= 0) { onEnemyDeath(s, e); s.enemies.splice(i, 1); continue; }
    }
    // DoT: poison
    if (e.poisonTimer > 0) {
      e.poisonTimer -= dt;
      e.hp -= e.poisonDps * dt;
      if (Math.random() < 0.2) {
        s.particles.push({ pos: { x: e.pos.x + rand(-e.radius, e.radius), y: e.pos.y + rand(-e.radius, e.radius) }, vel: { x: 0, y: -20 }, life: 0.4, maxLife: 0.4, color: '#5a8c4a', size: 2 });
      }
      if (e.hp <= 0) { onEnemyDeath(s, e); s.enemies.splice(i, 1); continue; }
    }
    // tier-based trailing particles for tougher enemies
    if (e.tier > 0 && !e.isBoss) {
      e.trailTimer -= dt;
      if (e.trailTimer <= 0) {
        e.trailTimer = 0.15;
        s.particles.push({ pos: { ...e.pos }, vel: { x: 0, y: 0 }, life: 0.4, maxLife: 0.4, color: e.color, size: 2 });
      }
    }
    if (e.freezeTimer > 0) { e.freezeTimer -= dt; continue; }
    if (e.slowTimer > 0) e.slowTimer -= dt;
    else e.slowFactor = 1;

    // slow aura
    let speedMult = e.slowFactor;
    if (slowLvl > 0 && dist(e.pos, s.player.pos) < getSlowRadius()) {
      speedMult = Math.min(speedMult, getSlowFactor(s));
    }
    // invisibility cloak: lower aggression
    let aggro = 1;
    if (s.player.artifacts.includes('veil_cloak') && s.player.hp / s.player.maxHp < 0.3) {
      aggro = 0.5;
    }

    const dx = s.player.pos.x - e.pos.x;
    const dy = s.player.pos.y - e.pos.y;
    const d = Math.hypot(dx, dy) || 1;
    e.pos.x += (dx / d) * e.speed * speedMult * aggro * dt;
    e.pos.y += (dy / d) * e.speed * speedMult * aggro * dt;

    // collision with player
    if (d < e.radius + PLAYER_RADIUS) {
      damagePlayer(s, e.damage);
      // boss projectile enemies don't self-damage on contact; normal enemies bounce
      if (!e.isBoss) {
        // knockback
        e.pos.x -= (dx / d) * 10;
        e.pos.y -= (dy / d) * 10;
      }
    }

    // boss attacks
    if (e.isBoss) {
      if (e.bossType === 'charger') {
        e.chargeTimer -= dt;
        if (e.isCharging) {
          e.pos.x += e.chargeDir.x * e.speed * 3 * dt;
          e.pos.y += e.chargeDir.y * e.speed * 3 * dt;
          e.isCharging = false; // will re-check below
          // damage on contact during charge
          if (dist(e.pos, s.player.pos) < e.radius + PLAYER_RADIUS) {
            damagePlayer(s, e.damage * 1.5);
            e.isCharging = false;
            e.chargeTimer = 4;
          }
          // stop charging after some distance
          if (!e.isCharging) e.chargeTimer = 4;
        } else if (e.chargeTimer <= 0) {
          // start charge
          const cdx = s.player.pos.x - e.pos.x;
          const cdy = s.player.pos.y - e.pos.y;
          const cd = Math.hypot(cdx, cdy) || 1;
          e.chargeDir = { x: cdx / cd, y: cdy / cd };
          e.isCharging = true;
          e.chargeTimer = 0.5; // charge duration
          playSound('bosshit');
        }
      } else if (e.bossType === 'summoner') {
        e.summonTimer -= dt;
        if (e.summonTimer <= 0) {
          e.summonTimer = 5;
          // spawn 3 minions
          for (let k = 0; k < 3; k++) {
            const a = Math.random() * Math.PI * 2;
            const sx = e.pos.x + Math.cos(a) * 60;
            const sy = e.pos.y + Math.sin(a) * 60;
            s.enemies.push({
              pos: { x: sx, y: sy },
              hp: 20 + s.wave * 4, maxHp: 20 + s.wave * 4,
              speed: 100, radius: 10, damage: 8, type: 'normal',
              color: '#8a5a8a', shape: 'circle',
              slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
              isBoss: false, bossShootTimer: 0, bossProjectiles: [],
              xpValue: 2, rotation: 0, tier: 0, trailTimer: 0,
              fireTimer: 0, fireDps: 0, poisonTimer: 0, poisonDps: 0,
              isElite: false, bossType: 'shooter',
              chargeTimer: 0, isCharging: false, chargeDir: { x: 0, y: 0 },
              summonTimer: 0, auraRadius: 0, auraDps: 0,
            });
          }
        }
        // also shoot
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 7;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 20, radius: 8, alive: true });
          }
        }
      } else if (e.bossType === 'aura') {
        // aura damage to player
        if (dist(e.pos, s.player.pos) < e.auraRadius) {
          damagePlayer(s, e.auraDps * dt);
        }
        // also shoot occasionally
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 6;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 20, radius: 8, alive: true });
          }
        }
      } else {
        // shooter (default)
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 5;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 25, radius: 8, alive: true });
          }
        }
      }
      // update boss projectiles
      for (let j = e.bossProjectiles.length - 1; j >= 0; j--) {
        const bp = e.bossProjectiles[j];
        bp.pos.x += bp.vel.x * dt;
        bp.pos.y += bp.vel.y * dt;
        const pd = dist(bp.pos, s.player.pos);
        if (pd < bp.radius + PLAYER_RADIUS) {
          damagePlayer(s, bp.damage);
          bp.alive = false;
        }
        if (Math.abs(bp.pos.x - s.player.pos.x) > 1000 || Math.abs(bp.pos.y - s.player.pos.y) > 1000) bp.alive = false;
        if (!bp.alive) e.bossProjectiles.splice(j, 1);
      }
    }
  }
}

function updateXpOrbs(s: GameState, dt: number): void {
  const magnetR = getMagnetRadius(s);
  for (let i = s.xpOrbs.length - 1; i >= 0; i--) {
    const orb = s.xpOrbs[i];
    orb.pos.x += orb.vel.x * dt; orb.pos.y += orb.vel.y * dt;
    orb.vel.x *= 0.9; orb.vel.y *= 0.9;
    const d = dist(orb.pos, s.player.pos);
    if (d < magnetR) {
      const dx = s.player.pos.x - orb.pos.x;
      const dy = s.player.pos.y - orb.pos.y;
      const pull = Math.min(1, 20 * dt);
      orb.pos.x += dx * pull;
      orb.pos.y += dy * pull;
    }
    if (d < PLAYER_RADIUS + 6) {
      gainXp(s, orb.value);
      playSound('pickup');
      s.xpOrbs.splice(i, 1);
    }
  }
}

function gainXp(s: GameState, amount: number): void {
  const mult = getXpMult(s);
  let gained = amount * mult;
  // echo accumulator: spheres absorb xp
  if (s.player.evolutions.includes('echoaccumulator')) {
    s.player.sphereXpAccumulator += gained * 0.3;
  }
  s.player.xp += gained;
  while (s.player.xp >= s.player.xpToNext) {
    s.player.xp -= s.player.xpToNext;
    s.player.level++;
    s.player.xpToNext = Math.floor(s.player.xpToNext * 1.4 + 3);
    // HP per level: +8 max HP and heal 8
    s.player.maxHp += 8;
    s.player.hp += 8;
    s.pendingUpgrade = generateUpgradeChoices(s);
    playSound('levelup');
  }
}

function updateHealthPacks(s: GameState): void {
  for (let i = s.healthPacks.length - 1; i >= 0; i--) {
    const hp = s.healthPacks[i];
    const d = dist(hp.pos, s.player.pos);
    if (d < PLAYER_RADIUS + hp.radius) {
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + 20);
      s.healthPacks.splice(i, 1);
      playSound('health');
      for (let k = 0; k < 10; k++) {
        s.particles.push({ pos: { ...hp.pos }, vel: { x: rand(-100, 100), y: rand(-100, 100) }, life: 0.5, maxLife: 0.5, color: '#5a8c4a', size: 3 });
      }
    }
  }
}

export function activateDash(s: GameState): void {
  if (s.player.dashCooldown > 0) return;
  let dx = 0, dy = 0;
  if (s.keys['w'] || s.keys['arrowup']) dy -= 1;
  if (s.keys['s'] || s.keys['arrowdown']) dy += 1;
  if (s.keys['a'] || s.keys['arrowleft']) dx -= 1;
  if (s.keys['d'] || s.keys['arrowright']) dx += 1;
  if (dx === 0 && dy === 0) { dx = 0; dy = -1; } // dash up by default
  const len = Math.hypot(dx, dy) || 1;
  s.player.dashDir = { x: dx / len, y: dy / len };
  s.player.dashTimer = 0.2;
  s.player.dashCooldown = 3;
  s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 0.3);
  s.player.dashCount++;
  playSound('dash');
}

export function setSphereType(s: GameState, type: SphereType): void {
  s.selectedSphereType = type;
  playSound('place');
}

export function placeSphere(s: GameState, x: number, y: number): void {
  // toggle: if clicking near an existing sphere, remove it instead
  const existing = s.spheres.find(sp => sp.alive && Math.hypot(sp.pos.x - x, sp.pos.y - y) < 26);
  if (existing) {
    removeSphere(s, existing);
    return;
  }
  const max = getMaxSpheres(s);
  if (s.spheres.length >= max) return;
  s.spheres.push({
    pos: { x, y },
    radius: BASE_SPHERE_RADIUS,
    damage: BASE_SPHERE_DAMAGE,
    attackDelay: BASE_SPHERE_DELAY,
    attackTimer: 0,
    rotation: 0,
    alive: true,
    killsContribution: 0,
    visualTier: sphereLevel(s, s.selectedSphereType),
    type: s.selectedSphereType,
    auraTimer: 0,
  });
  const stype = SPHERE_TYPES[s.selectedSphereType];
  for (let i = 0; i < 15; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({ pos: { x, y }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.5, maxLife: 0.5, color: stype.color, size: 3 });
  }
  playSound('place');
}

export function removeSphere(s: GameState, sphere: SphereEntity): void {
  sphere.alive = false;
  s.spheres = s.spheres.filter(sp => sp !== sphere);
  if (s.player.engineerRelaySource === sphere) {
    s.player.engineerRelaySource = null;
    s.player.engineerRelayTimer = 0;
  }
  for (let i = 0; i < 15; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.5, maxLife: 0.5, color: '#b8475a', size: 3 });
  }
}