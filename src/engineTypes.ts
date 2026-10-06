import type { AbilityType, ArtifactId, SphereType, BossType, Difficulty } from './gameData';
import type { CharacterId } from './characters';
import type { RuneType } from './runes';
import type { SphereNetworkState } from './network';

export interface Vec { x: number; y: number; }

export interface SphereMods {
  multishot: number;
  pierce: number;
  ricochet: number;
  fire: number;
  freeze: number;
  poison: number;
  breach: number;
  overload: number;
  split: number;
  shatter: number;
  execute: number;
  mark: number;
  echo: number;
  anchor: number;
  phase: number;
  static: number;
  resonant: number;
  magnetic: number;
  vampiric: number;
  corrupt: number;
  drain: number;
  afterimage: number;
  impact: number;
  gravitic: number;
  rotationSpeed: number;
}

export interface PlayerState {
  pos: Vec;
  hp: number;
  maxHp: number;
  speed: number;
  level: number;
  xp: number;
  xpToNext: number;
  abilities: Partial<Record<AbilityType, number>>;
  activeAbilitySlots: number;
  evolutions: string[];
  artifacts: ArtifactId[];
  kills: number;
  mutationStage: number;
  invulnerableTimer: number;
  contactDamageCooldown: number;
  deathTimer: number;
  resonanceCharge: number;
  resonanceEventActive: boolean;
  resonanceGeometryKey: string;
  resonanceGeometryNodes: number[];
  resonanceLastActiveFormationKey: string;
  resonanceFormationCandidateKeys: string[];
  resonanceEventsTriggered: number;
  resonanceLineBurst: number;
  spheristChorusHits: number;
  berserkerRedlineHits: number;
  resonanceRingTimer: number;
  resonanceRingPulseTimer: number;
  resonanceRingCursor: number;
  invulnUsed: boolean;
  shieldCharges: number;
  shieldTimer: number;
  shieldVisualPulse: number;
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
  fireCatalystTimer: number;
  blinkHpCost: boolean;
  sphereXpAccumulator: number;
  sphereUpgradeCount: number;
  sphereMods: SphereMods;
  sphereMovementLocked: boolean;
  knownSphereTypes: SphereType[];
  sphereProgression: Partial<Record<SphereType, number>>;
  sphereBranches: Partial<Record<SphereType, import('./sphereProgression').SphereEvolutionId>>;
  dashCooldown: number;
  dashTimer: number;
  dashDir: Vec;
  dashCount: number;
  combo: number;
  comboTimer: number;
  comboMult: number;
  buffTimer: number;
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
  alchemistReactionCount: number;
  conductorOverdriveTimer: number;
  conductorDischargeFormation: string;
  oracleForecastKeys: string[];
  oracleForecastRerollUsed: boolean;
  fractalFormationHistory: Array<{ type: string; key: string; nodes: Vec[] }>;
  fractalEchoTimer: number;
  fractalEchoPulseTimer: number;
  fractalEchoCooldown: number;
  fractalEchoNodes: Vec[];
  fractalEchoStrength: number;
  voidPhantomTimer: number;
  voidPhantomPulseTimer: number;
  voidPhantomPulseCount: number;
  voidPhantomSource: SphereEntity | null;
  voidPhantomPos: Vec | null;
}

export interface SphereEntity {
  pos: Vec;
  radius: number;
  damage: number;
  attackDelay: number;
  attackTimer: number;
  rotation: number;
  alive: boolean;
  networkDisabledTimer: number;
  killsContribution: number;
  formationHitCount: number;
  formationHitCounts: Partial<Record<import('./network').NetworkFormation, number>>;
  resonancePulseTimer: number;
  visualTier: number;
  type: SphereType;
  auraTimer: number;
  orbitalLastSweepRotation?: number;
}

export interface SphereProjectile {
  pos: Vec;
  vel: Vec;
  damage: number;
  radius: number;
  alive: boolean;
  color: string;
  pierce: number;
  hitEnemies: Set<EnemyEntity>;
  effect: 'none' | 'fire' | 'freeze' | 'poison';
  ricochet: number;
  life: number;
  sourceSphere?: SphereEntity;
  procOnHit?: boolean;
}

export type EliteVariant =
  | 'linkbreaker'
  | 'resonance_leech'
  | 'phantom_hunter'
  | 'geometry_shifter'
  | 'splitter_prime'
  | 'mirror_warden'
  | 'stasis_warden'
  | 'nullifier'
  | 'pyroclast'
  | 'scavenger_prime';

export type EnemyRole =
  | 'grunt'
  | 'swarmer'
  | 'charger'
  | 'tank_guard'
  | 'ranged'
  | 'splitter'
  | 'healer'
  | 'bomber'
  | 'leech'
  | 'sniper'
  | 'disruptor'
  | 'anchor'
  | 'phase'
  | 'scavenger'
  | 'corruptor';

export interface EnemyEntity {
  pos: Vec;
  hp: number;
  maxHp: number;
  speed: number;
  radius: number;
  damage: number;
  type: 'normal' | 'fast' | 'tank' | 'elite' | 'boss';
  role?: EnemyRole;
  /** Presentation-only silhouette variant. Does not affect combat rules. */
  visualVariant?: 'wisp' | 'skitter' | 'moth' | 'beetle' | 'brute' | 'prism' | 'linkbreaker' | 'serpent' | 'leech' | 'stalker';
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
  /** Optional elemental mastery tick cadence. Omitted = legacy continuous DoT. */
  fireTickTimer?: number;
  fireTickInterval?: number;
  poisonTimer: number;
  poisonDps: number;
  /** Optional elemental mastery tick cadence. Omitted = legacy continuous DoT. */
  poisonTickTimer?: number;
  poisonTickInterval?: number;
  /** Freeze mastery vulnerability window for the source Sphere family. */
  freezeVulnerabilityTimer?: number;
  freezeVulnerabilitySource?: SphereType;
  /** Per-target cooldown for special Chain/Field elemental reactions. */
  elementalReactionTimer?: number;
  /** Chain-only conduction progress and its source family. */
  elementalConduction?: number;
  elementalConductionSource?: SphereType;
  /** Temporary stacking state for the authored Void/Corrupt mechanic. */
  corruptStacks?: number;
  isElite: boolean;
  eliteVariant?: EliteVariant;
  elitePulseTimer: number;
  elitePulseTelegraphTimer?: number;
  elitePulseTarget?: SphereEntity;
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
  rotation: number;
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
  anchorType: SphereType;
}

export interface LightningBolt {
  from: Vec;
  to: Vec;
  life: number;
  sourceSphere?: SphereEntity;
}

export interface UpgradeChoice {
  type: 'ability' | 'sphere' | 'modifier';
  ability?: AbilityType;
  abilityEvolutionIndex?: number;
  abilityStage?: 'upgrade' | 'branch' | 'final';
  sphereType?: SphereType;
  modifier?: keyof SphereMods;
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
  sourceSphereType?: SphereType;
}

export interface ChestEntity {
  pos: Vec;
  alive: boolean;
  radius: number;
  kind?: 'artifact' | 'stella';
}

export interface RuneEntity {
  pos: Vec;
  alive: boolean;
  radius: number;
  type: RuneType;
  life: number;
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
  levelUpPity: { ability: number; sphere: number };
  levelUpRerollsRemaining: number;
  levelUpLockChoiceKey: string | null;
  levelUpLocksRemaining: number;
  recentUpgradeKeys: string[];
  pendingArtifact: ArtifactId[] | null;
  pendingStella: boolean;
  stellaClaims: number;
  stellaLegendaryClaims: number;
  stellaChests: ChestEntity[];
  stats: GameStats;
  screenShake: number;
  bossArrow: Vec | null;
  flashText: { text: string; life: number; color: string } | null;
  keys: Record<string, boolean>;
  mouse: { x: number; y: number; down: boolean };
  formationMemory: { type: string; nodes: Vec[]; expiresAt: number } | null;
  worldWidth: number;
  worldHeight: number;
  camera: Vec;
  activeKeyMap: Record<string, AbilityType>;
  sphereProjectiles: SphereProjectile[];
  artifactPickupPending: boolean;
  pendingSphereUpgrade: SphereUpgradeChoice[] | null;
  damageNumbers: DamageNumber[];
  chests: ChestEntity[];
  runes: RuneEntity[];
  difficulty: Difficulty;
  tutorialMode: boolean;
  evolutionsThisRun: number;
  selectedSphereType: SphereType;
  shopUpgrades: Record<string, number>;
  runSeed: number;
  rngState: number;
  networkFrame: { frameId: number; network: SphereNetworkState } | null;
  networkFrameId: number;
  /** Optional player-selected order of the two active Network formations. */
  networkFormationSelection: import('./network').NetworkFormationSelection | null;
}

export interface ShopState {
  gold: number;
  upgrades: Record<string, number>;
}

export interface SphereUpgradeChoice {
  id: keyof SphereMods;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}