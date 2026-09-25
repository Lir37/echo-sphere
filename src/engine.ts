import {
  ABILITIES, ACTIVE_KEYS, EVOLUTION_BY_PAIR, EVOLUTION_MAP,
  BOSS_TYPES, DIFFICULTIES,
} from './gameData';
import type {
  AbilityType, ArtifactId, BossType, Difficulty,
  Vec, PlayerState, SphereEntity, EnemyEntity, BossProjectile,
  XPOrb, HealthPack, Particle, FireTrailSegment, MinionEntity, LightningBolt,
  UpgradeChoice, GameStats, DamageNumber, ChestEntity, GameState, ShopState,
  TowerMods, TowerUpgradeChoice, LeaderEntry, MapTheme,
} from './engineTypes';
import { playSound } from './audio';
import {
  DEFAULT_MAX_SPHERES,
  MAX_SPHERES_CAP,
  BASE_SPHERE_RADIUS,
  BASE_SPHERE_DAMAGE,
  BASE_SPHERE_DELAY,
  createSphere,
  getMaxSpheres,
  getSphereRadius,
  getSphereDamage,
  getSphereDelay,
  updateSpheres,
  setSphereType,
  placeSphere,
  removeSphere,
  syncSphereVisualTier,
} from './engineSpheres';
export {
  DEFAULT_MAX_SPHERES,
  MAX_SPHERES_CAP,
  BASE_SPHERE_RADIUS,
  BASE_SPHERE_DAMAGE,
  BASE_SPHERE_DELAY,
  getMaxSpheres,
  getSphereRadius,
  getSphereDamage,
  getSphereDelay,
  setSphereType,
  placeSphere,
  removeSphere,
};

import { activateByKey } from './engineAbilities';
import {
  dealDamageToEnemy,
  onEnemyDeath,
  pickArtifacts,
  damagePlayer,
  dist,
  rand,
  getCritChance,
  getDodgeChance,
  getVampirePercent,
  getDamageTakenMult,
  getCooldownMult,
} from './engineCombat';

export const BASE_PLAYER_SPEED = 180;
export const BASE_SPHERE_RADIUS = 130;
export const BASE_SPHERE_DAMAGE = 12;
export const BASE_SPHERE_DELAY = 1.2;
export const PLAYER_RADIUS = 16;

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
  const startSpheres = DEFAULT_MAX_SPHERES + (shop.upgrades.spheres || 0);
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
  const sphere = createSphere({ x: 0, y: 0 }, 'standard', 0);
  return {
    player,
    spheres: [sphere],
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







export function getSlowRadius(): number {
  return 300;
}

export function getSlowFactor(s: GameState): number {
  const lvl = s.player.abilities.slow || 0;
  return lvl > 0 ? 1 - (0.1 + (lvl - 1) * 0.05) : 1;
}





export function getXpMult(s: GameState): number {
  let m = 1;
  m *= 1 + (s.shopUpgrades.xp || 0) * 0.05;
  if (s.player.artifacts.includes('ring_xp')) m *= 1.2;
  return m;
}

export function getMagnetRadius(s: GameState): number {
  let r = 60;
  const lvl = s.player.abilities.magnet || 0;
  r *= 1 + lvl * 0.2;
  return r;
}

// ===== Helpers =====


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

// ===== Damage application =====








// ===== Active abilities =====




















// ===== Upgrade generation =====
export function generateUpgradeChoices(s: GameState): UpgradeChoice[] {
  // check evolution availability
  const evo = checkEvolution(s);
  if (evo) {
    return [{ type: 'evolve', evolution: evo, currentLevel: 5, newLevel: 1 }];
  }
  const choices: UpgradeChoice[] = [];
  const available: AbilityType[] = [];
  for (const id of Object.keys(ABILITIES) as AbilityType[]) {
    const def = ABILITIES[id];
    const cur = s.player.abilities[id] || 0;
    if (cur < def.maxLevel) available.push(id);
  }
  // shuffle
  const pool = [...available].sort(() => Math.random() - 0.5);
  for (const id of pool.slice(0, 3)) {
    const cur = s.player.abilities[id] || 0;
    choices.push({ type: 'ability', ability: id, currentLevel: cur, newLevel: cur + 1 });
  }
  return choices;
}

function checkEvolution(s: GameState): string | null {
  for (const evo of Object.keys(EVOLUTION_MAP)) {
    const def = EVOLUTION_MAP[evo];
    if (s.player.evolutions.includes(evo)) continue;
    const aLvl = s.player.abilities[def.a] || 0;
    const bLvl = s.player.abilities[def.b] || 0;
    if (aLvl >= ABILITIES[def.a].maxLevel && bLvl >= ABILITIES[def.b].maxLevel) {
      return evo;
    }
  }
  return null;
}

export function applyUpgrade(s: GameState, choice: UpgradeChoice): void {
  if (choice.type === 'ability' && choice.ability) {
    s.player.abilities[choice.ability] = choice.newLevel;
    // assign hotkey for active abilities
    const def = ABILITIES[choice.ability];
    if (def.category === 'active' && choice.currentLevel === 0) {
      assignHotkey(s, choice.ability);
    }
    // vitality increases maxHp
    if (choice.ability === 'vitality') {
      s.player.maxHp += 20;
      s.player.hp += 20;
    }
    // track tower-related upgrades
    const towerAbilities = ['radius', 'damage', 'attackspeed', 'maxspheres', 'sphereboost'] as const;
    if (towerAbilities.includes(choice.ability as any)) {
      s.player.towerUpgradeCount++;
      // synchronize sphere visual tiers
      syncSphereVisualTier(s);
      // every 5 tower upgrades, offer tower mod choice
      if (s.player.towerUpgradeCount % 5 === 0) {
        s.pendingTowerUpgrade = generateTowerUpgradeChoices(s);
      }
    }
  } else if (choice.type === 'evolve' && choice.evolution) {
    const def = EVOLUTION_MAP[choice.evolution];
    s.player.abilities[def.a] = undefined;
    s.player.abilities[def.b] = undefined;
    s.player.evolutions.push(choice.evolution);
    s.evolutionsThisRun++;
    playSound('evolve');
    // assign hotkey if needed (barrier uses blast key, etc.)
    // re-assign hotkeys for evolved active abilities
    if (choice.evolution === 'barrier') assignHotkey(s, 'blast');
    if (choice.evolution === 'blink') assignHotkey(s, 'teleport');
    if (choice.evolution === 'icepath') assignHotkey(s, 'firetrail');
    if (choice.evolution === 'devourers') assignHotkey(s, 'minion');
    if (choice.evolution === 'thunderstorm') assignHotkey(s, 'lightning');
  }
}

export function applyTowerUpgrade(s: GameState, choice: TowerUpgradeChoice): void {
  s.player.towerMods[choice.id] = (s.player.towerMods[choice.id] || 0) + 1;
  s.pendingTowerUpgrade = null;
}

function generateTowerUpgradeChoices(s: GameState): TowerUpgradeChoice[] {
  const all: TowerUpgradeChoice[] = [
    { id: 'multishot', name: { ru: 'Мультивыстрел', en: 'Multishot' }, desc: { ru: '+1 снаряд за выстрел', en: '+1 projectile per shot' } },
    { id: 'pierce', name: { ru: 'Пробитие', en: 'Pierce' }, desc: { ru: 'Снаряд пробивает +1 врага', en: 'Projectile pierces +1 enemy' } },
    { id: 'ricochet', name: { ru: 'Рикошет', en: 'Ricochet' }, desc: { ru: '+1 отскок снаряда', en: '+1 projectile bounce' } },
    { id: 'fire', name: { ru: 'Поджог', en: 'Fire' }, desc: { ru: 'Снаряды поджигают врагов (DoT)', en: 'Projectiles set enemies on fire (DoT)' } },
    { id: 'freeze', name: { ru: 'Заморозка', en: 'Freeze' }, desc: { ru: 'Снаряды замедляют врагов', en: 'Projectiles slow enemies' } },
    { id: 'poison', name: { ru: 'Яд', en: 'Poison' }, desc: { ru: 'Снаряды отравливают врагов (DoT)', en: 'Projectiles poison enemies (DoT)' } },
  ];
  // filter out maxed ones (max 3 levels each)
  const available = all.filter(c => (s.player.towerMods[c.id] || 0) < 3);
  // pick 3 random
  const shuffled = [...available].sort(() => Math.random() - 0.5);
  return shuffled.slice(0, 3);
}

export function applyArtifact(s: GameState, id: ArtifactId): void {
  s.player.artifacts.push(id);
  if (id === 'amulet_hp') { s.player.maxHp += 30; s.player.hp += 30; }
  if (id === 'dragon_heart') { s.player.maxHp += 50; s.player.hp += 50; }
  playSound('chest');
}

// ===== Main update =====
export function update(s: GameState, dt: number): void {
  if (s.paused || s.gameOver) return;
  if (s.pendingUpgrade || s.pendingArtifact || s.pendingEvolution || s.pendingTowerUpgrade) return;

  s.time += dt;
  s.stats.time = s.time;

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
      s.pendingChest = chest;
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
  if (s.player.fireTrailTimer > 0) {
    s.player.fireTrailTimer -= dt;
    const lvl = s.player.abilities.firetrail || 0;
    const dmg = 4 + lvl * 2;
    s.fireTrails.push({ pos: { ...s.player.pos }, life: 0.6, maxLife: 0.6, damage: dmg });
  }

  // regen stone artifact
  if (s.player.artifacts.includes('regen_stone')) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + dt);
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

function updateMinions(s: GameState, dt: number): void {
  for (let i = s.minions.length - 1; i >= 0; i--) {
    const m = s.minions[i];
    m.life -= dt;
    m.rotation += dt * 3;
    if (m.life <= 0) { s.minions.splice(i, 1); continue; }
    m.attackTimer -= dt;
    if (m.attackTimer <= 0) {
      let nearest: EnemyEntity | null = null;
      let nd = Infinity;
      for (const e of s.enemies) {
        if (e.hp <= 0) continue;
        const d = dist(e.pos, m.pos);
        if (d < 200 && d < nd) { nd = d; nearest = e; }
      }
      if (nearest) {
        let dmg = m.damage;
        // synergy: minion + crit -> minions can crit
        if ((s.player.abilities.crit || 0) > 0 && Math.random() < getCritChance(s)) dmg *= 2;
        dealDamageToEnemy(s, nearest, dmg);
        m.attackTimer = 0.6;
        // devourers evolution: heal on kill
        if (s.player.evolutions.includes('devourers') && nearest.hp <= 0) {
          s.player.hp = Math.min(s.player.maxHp, s.player.hp + 5);
        }
      }
    }
    // move minion toward nearest enemy
    let nearest: EnemyEntity | null = null;
    let nd = Infinity;
    for (const e of s.enemies) {
      if (e.hp <= 0) continue;
      const d = dist(e.pos, m.pos);
      if (d < nd) { nd = d; nearest = e; }
    }
    if (nearest && nd > 80) {
      const dx = nearest.pos.x - m.pos.x;
      const dy = nearest.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      m.pos.x += dx / d * 120 * dt;
      m.pos.y += dy / d * 120 * dt;
    } else {
      // follow player
      const dx = s.player.pos.x - m.pos.x;
      const dy = s.player.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      if (d > 60) { m.pos.x += dx / d * 100 * dt; m.pos.y += dy / d * 100 * dt; }
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
    if (s.player.artifacts.includes('invisibility_cloak') && s.player.hp / s.player.maxHp < 0.3) {
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

