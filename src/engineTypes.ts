import type { AbilityType, ArtifactId, SphereType, BossType, Difficulty } from './gameData';
export type { AbilityType, ArtifactId, SphereType, BossType, Difficulty } from './gameData';

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

export type MapTheme = 'parchment' | 'bamboo' | 'ocean' | 'sunset';

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
  pendingEvolution: { choices: string[] } | null;
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
  pendingTowerUpgrade: TowerUpgradeChoice[] | null;
  damageNumbers: DamageNumber[];
  chests: ChestEntity[];
  pendingChest: ChestEntity | null;
  difficulty: Difficulty;
  evolutionsThisRun: number;
  selectedSphereType: SphereType;
  shopUpgrades: Record<string, number>;
}

export interface ShopState {
  gold: number;
  upgrades: Record<string, number>; // id -> level
}

export interface TowerMods {
  multishot: number;  // extra projectiles per shot
  pierce: number;    // enemies a projectile passes through
  ricochet: number;  // bounce count
  fire: number;      // fire effect level (DoT)
  freeze: number;    // freeze effect level
  poison: number;    // poison effect level
}

export interface TowerUpgradeChoice {
  id: keyof TowerMods;
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
