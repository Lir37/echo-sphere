import {
  ABILITIES, ACTIVE_KEYS, ARTIFACT_MAP, EVOLUTION_BY_PAIR, EVOLUTION_MAP,
  SPHERE_TYPES, BOSS_TYPES, DIFFICULTIES,
  type AbilityType, type ArtifactId, type SphereType, type BossType, type Difficulty,
} from './gameData';
import { playSound } from './audio';

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
  timestopTimer: number;
  swiftBootsTimer: number;
  chaosOrbTimer: number;
  chaosOrbBuff: 'dmg' | 'radius' | null;
  chaosOrbBuffTimer: number;
  teleportDamageBuffTimer: number;
  blinkHpCost: boolean;
  sphereXpAccumulator: number;
  towerUpgradeCount: number;
  towerMods: TowerMods;
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
  type: 'ability' | 'evolve';
  ability?: AbilityType;
  evolution?: string;
  currentLevel: number;
}

export interface DamageNumber {
  pos: Vec;
  value: number;
  life: number;
  color: string;
}

export interface ChestEntity {
  pos: Vec;
  radius: number;
  alive: boolean;
}

export interface TowerMods {
  multishot: number;
  pierce: number;
  ricochet: number;
  fire: number;
  freeze: number;
  poison: number;
}

export interface GameStats {
  time: number;
  wave: number;
  enemiesKilled: number;
  goldEarned: number;
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
  pendingEvolution: string[] | null;
  stats: GameStats;
  screenShake: number;
  bossArrow: Vec | null;
  flashText: { text: string; life: number } | null;
  keys: Record<string, boolean>;
  mouse: { x: number; y: number; down: boolean };
  worldWidth: number;
  worldHeight: number;
  mapTheme: MapTheme;
  camera: Vec;
  activeKeyMap: Record<string, string>;
  sphereProjectiles: SphereProjectile[];
  artifactPickupPending: boolean;
  pendingTowerUpgrade: boolean | null;
  damageNumbers: DamageNumber[];
  chests: ChestEntity[];
  pendingChest: boolean | null;
  difficulty: Difficulty;
  evolutionsThisRun: number;
  selectedSphereType: SphereType;
  shopUpgrades: ShopUpgrades;
}

export type MapTheme = 'parchment' | 'bamboo' | 'ocean' | 'sunset';

export interface ShopUpgrades {
  damage: number;
  radius: number;
  speed: number;
  spheres: number;
  hp: number;
  crit: number;
  xp: number;
}

export interface ShopState {
  upgrades: ShopUpgrades;
  gold: number;
  totalRuns: number;
  bestTime: number;
  bestKills: number;
  unlockedAchievements: string[];
}

export const BASE_PLAYER_SPEED = 220;
export const BASE_SPHERE_RADIUS = 22;
export const BASE_SPHERE_DAMAGE = 10;
export const BASE_SPHERE_DELAY = 0.8;
export const DEFAULT_MAX_SPHERES = 5;
export const MAX_SPHERES_CAP = 8;

export function createInitialState(
  shop: ShopState,
  playerName: string,
  difficulty: Difficulty = 'normal',
  mapTheme: MapTheme = 'parchment',
): GameState {
  const startHp = 100 + (shop.upgrades.hp || 0) * 10;
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
    timestopTimer: 0,
    swiftBootsTimer: 0,
    chaosOrbTimer: 0,
    chaosOrbBuff: null,
    chaosOrbBuffTimer: 0,
    teleportDamageBuffTimer: 0,
    blinkHpCost: false,
    sphereXpAccumulator: 0,
    towerUpgradeCount: 0,
    towerMods: { multishot: 0, pierce: 0, ricochet: 0, fire: 0, freeze: 0, poison: 0 },
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
    pendingEvolution: null,
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
    pendingTowerUpgrade: null,
    damageNumbers: [],
    chests: [],
    pendingChest: null,
    difficulty: difficulty,
    evolutionsThisRun: 0,
    selectedSphereType: 'standard',
    shopUpgrades: { ...shop.upgrades },
  };
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
  if (s.player.artifacts.includes('crystal_speed')) sp *= 1.15;
  if (s.player.artifacts.includes('dragon_heart')) sp *= 0.9;
  if (s.player.swiftBootsTimer > 0) sp *= 1.1;
  if (s.player.mutationStage >= 3) sp *= 1.2;
  return sp;
}

export function getSphereRadius(s: GameState, sphere: SphereEntity): number {
  let r = BASE_SPHERE_RADIUS;
  const lvl = s.player.abilities.radius || 0;
  r *= 1 + lvl * 0.08;
  r *= 1 + (s.shopUpgrades.radius || 0) * 0.05;
  if (s.player.artifacts.includes('radius_shard')) r *= 1.12;
  if (s.player.chaosOrbBuff === 'radius' && s.player.chaosOrbBuffTimer > 0) r *= 1.3;
  return r;
}
