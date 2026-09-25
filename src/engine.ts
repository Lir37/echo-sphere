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
  getCharacterMaxHpMultiplier,
  getCharacterMoveSpeedMultiplier,
} from './characterRuntime';
import { loadCharacterId, loadCharacterProfiles } from './persistence';
import { getArtifactMoveSpeedMultiplier, getArtifactMaxHpBonus, getArtifactXpMultiplier, getArtifactRegenPerSecond, getArtifactCooldownMultiplier, pickArtifactChoices, pickStellaArtifactChoice } from './artifactSystem';
import { SPHERE_PROGRESSION, ABILITY_PROGRESSION, spherePriority, sphereLevel, sphereModifiers, SPHERE_ABILITY_SYNERGIES, getActiveSphereAbilitySynergies } from './sphereProgression';
import { selectSphereTarget } from './targeting';
import { analyzeSphereNetwork, getSphereNetworkProfile, getLinkedNodeIndexes } from './network';
import { RUNE_DEFS, type RuneType } from './runes';
import { createRunSeed, createRngState, nextRandom } from './rng';
import { addResonanceChargeFromSource, type ResonanceSource } from './resonance';
import { canReceivePlayerDamage, CRIT_BASE, CRIT_MULTIPLIER_BASE, getContextualCritChance } from './combatRules';
import type { GameState, ShopState, PlayerState, SphereEntity, EnemyEntity, SphereProjectile, SphereMods, SphereUpgradeChoice, UpgradeChoice, DamageNumber, ChestEntity, RuneEntity, MinionEntity, LightningBolt, BossProjectile, XPOrb, HealthPack, Particle, FireTrailSegment, Vec } from './engineTypes';
export type {
  GameState, ShopState, PlayerState, SphereEntity, EnemyEntity, SphereProjectile, SphereMods,
  SphereUpgradeChoice, UpgradeChoice, DamageNumber, ChestEntity, RuneEntity, MinionEntity,
  LightningBolt, BossProjectile, XPOrb, HealthPack, Particle, FireTrailSegment, Vec,
} from './engineTypes';

// Facade: preserves the public engine API while gameplay systems live in dedicated subsystems.

import { BALANCE } from './engineBalance';
import {
  BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY,
  getMaxSpheres, getSphereRadius, getSphereDamage, getSphereDpsEstimate, getSphereDelay,
  setSphereType, placeSphere, removeSphere, updateSpheres,
} from './engineSpheres';
import { getSlowRadius, getSlowFactor, spawnEnemy, startWave, updateMinions, updateEnemies } from './engineEnemies';
import {
  getCritChance, getDodgeChance, getVampirePercent, getDamageTakenMult,
  dealDamageToEnemy, damagePlayer,
} from './engineCombat';
import { activateByKey } from './engineAbilities';
import { assignHotkey, getUpgradeSourceWeight, getSphereUpgradeChoiceWeight, generateUpgradeChoices, applyUpgrade, applySphereUpgrade } from './engineProgression';
import {
  dist, rand, clamp, getNetworkNodes, getAbilityBranchId, getNearestSphere, getSphereFinalIndex,
} from './engineRuntime';

export { BALANCE } from './engineBalance';
export {
  DEFAULT_MAX_SPHERES, MAX_SPHERES_CAP, BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY,
  getMaxSpheres, getSphereRadius, getSphereDamage, getSphereDpsEstimate, getSphereDelay,
  setSphereType, placeSphere, removeSphere,
} from './engineSpheres';
export { getSlowRadius, getSlowFactor } from './engineEnemies';
export { getCritChance, getDodgeChance, getVampirePercent, getDamageTakenMult } from './engineCombat';
export { activateByKey } from './engineAbilities';
export { assignHotkey, getUpgradeSourceWeight, getSphereUpgradeChoiceWeight, generateUpgradeChoices, applyUpgrade, applySphereUpgrade } from './engineProgression';

export interface LeaderEntry {
  name: string;
  time: number;
  wave: number;
  date: number;
}

export const BASE_PLAYER_SPEED = 180;
export const PLAYER_RADIUS = 16;
export const STELLA_LEGENDARY_CUTOFF_SECONDS = 25 * 60;

// Centralised run-balance constants. Keep the early game readable and let
// difficulty come from enemy composition + gradual scaling rather than HP walls.

export function getXpToNextLevel(level: number): number {
  return Math.max(
    BALANCE.firstLevelXp,
    Math.round(
      BALANCE.firstLevelXp +
      Math.max(0, level - 1) * BALANCE.xpGrowthLinear +
      Math.pow(Math.max(0, level - 1), 2) * BALANCE.xpGrowthQuadratic
    ),
  );
}

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
  runSeedOverride?: number,
): GameState {
  const runSeed = runSeedOverride === undefined ? createRunSeed(playerName, difficulty, mapTheme) : (runSeedOverride >>> 0) || 1;
  const rngState = createRngState(runSeed);
  const characterId = loadCharacterId();
  const profile = loadCharacterProfiles().find((item) => item.id === characterId);
  const characterMasteryLevel = profile?.masteryLevel || 1;
  const baseHp = BALANCE.startingHp + (shop.upgrades.hp || 0) * 10;
  const startHp = baseHp * getCharacterMaxHpMultiplierForId(characterId);
  const player: PlayerState = {
    pos: { x: 0, y: 0 },
    hp: startHp,
    maxHp: startHp,
    speed: BASE_PLAYER_SPEED,
    level: 1,
    xp: 0,
    xpToNext: BALANCE.firstLevelXp,
    abilities: {},
    activeAbilitySlots: 0,
    evolutions: [],
    artifacts: [],
    kills: 0,
    mutationStage: 0,
    invulnerableTimer: 0,
    contactDamageCooldown: 0,
    resonanceCharge: 0,
    resonanceEventActive: false,
    resonanceGeometryKey: 'none',
    resonanceGeometryNodes: [],
    resonanceLineBurst: 0,
    resonanceRingTimer: 0,
    resonanceRingPulseTimer: 0,
    resonanceRingCursor: 0,
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
    fireCatalystTimer: 0,
    blinkHpCost: false,
    sphereXpAccumulator: 0,
    sphereUpgradeCount: 0,
    sphereProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0, orbital: 0, prism: 0, gravity: 0, pulse: 0, void: 0 },
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
    spheres: [{
      pos: { x: 0, y: -95 },
      radius: BASE_SPHERE_RADIUS,
      damage: BASE_SPHERE_DAMAGE,
      attackDelay: BASE_SPHERE_DELAY,
      attackTimer: 0,
      rotation: 0,
      alive: true,
      networkDisabledTimer: 0,
      killsContribution: 0,
      formationHitCount: 0,
      resonancePulseTimer: 0,
      visualTier: 0,
      type: 'standard',
      auraTimer: 0,
    }],
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
    levelUpPity: { ability: 0, sphere: 0, modifier: 0 },
    pendingArtifact: null,
    pendingStella: false,
    stellaClaims: 0,
    stellaLegendaryClaims: 0,
    stats: { time: 0, wave: 0, enemiesKilled: 0, goldEarned: 0 },
    screenShake: 0,
    bossArrow: null,
    flashText: null,
    keys: {},
    mouse: { x: 0, y: 0, down: false },
    formationMemory: null,
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
    runes: [],
    difficulty: difficulty,
    evolutionsThisRun: 0,
    selectedSphereType: 'standard',
    shopUpgrades: { ...shop.upgrades },
    runSeed,
    rngState,
  };
}

function getCharacterMaxHpMultiplierForId(id: CharacterId): number {
  return 1 + CHARACTER_DEFS[id].baseModifiers.maxHp;
}

// ===== Derived stats =====
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

export function getCooldownMult(s: GameState): number {
  return getArtifactCooldownMultiplier(s);
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
function getResonanceFormation(network: ReturnType<typeof analyzeSphereNetwork>) {
  // Prefer the most structurally expressive active geometry for the event.
  return network.fractal ?? network.lattice ?? network.ring ?? network.square ?? network.triangle ?? network.cluster ?? network.line;
}

function resonanceFormationCenter(s: GameState, nodes: number[], networkNodes = getNetworkNodes(s)): Vec {
  const positions = nodes.map((index) => networkNodes[index]?.pos).filter((pos): pos is Vec => Boolean(pos));
  if (positions.length === 0) return { ...s.player.pos };
  return positions.reduce((acc, pos) => ({ x: acc.x + pos.x / positions.length, y: acc.y + pos.y / positions.length }), { x: 0, y: 0 });
}

export function triggerResonanceEvent(s: GameState): void {
  const networkNodes = getNetworkNodes(s);
  const network = analyzeSphereNetwork(networkNodes);
  const formation = getResonanceFormation(network);
  const type = formation?.type ?? 'none';
  const center = formation ? resonanceFormationCenter(s, formation.nodes, networkNodes) : { ...s.player.pos };
  const baseDamage = 16 + s.player.level * 2;
  s.player.resonanceEventActive = true;
  try {
    if (type === 'fractal') {
      // Fractal Echo replays the strongest lower-order geometry and then
      // emits a secondary echo around the whole fractal.
      const replay = network.lattice ?? network.ring ?? network.square ?? network.triangle;
      const replayNodes = (replay?.nodes || []).filter((index) => index < s.spheres.length);
      for (const index of replayNodes) {
        const sphere = s.spheres[index];
        if (!sphere?.alive) continue;
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.35);
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.45);
        s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.45, maxLife: 0.45, color: '#ffb84d', size: 4 });
        for (const enemy of s.enemies) {
          if (enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= 95) {
            dealDamageToEnemy(s, enemy, baseDamage * 0.55, sphere, false);
          }
        }
      }
      for (const enemy of s.enemies) {
        if (enemy.hp > 0 && dist(enemy.pos, center) <= 155) {
          dealDamageToEnemy(s, enemy, baseDamage * 0.65, undefined, false);
        }
      }
      s.flashText = { text: 'FRACTAL ECHO', life: 0.9, color: '#ffb84d' };
    } else if (type === 'lattice') {
      // Lattice Cascade synchronizes attack timing across the formation.
      for (const index of (formation?.nodes || []).filter((value) => value < s.spheres.length)) {
        const sphere = s.spheres[index];
        if (sphere?.alive) {
          sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.55);
          sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.40);
          s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.35, maxLife: 0.35, color: '#39d8ff', size: 4 });
        }
      }
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, center) <= 145) dealDamageToEnemy(s, enemy, baseDamage * 0.95, undefined, false);
      s.flashText = { text: 'LATTICE CASCADE', life: 0.9, color: '#39d8ff' };
    } else if (type === 'ring') {
      // Ring Loop continues for a short duration and advances node by node.
      s.player.resonanceRingTimer = 2.4;
      s.player.resonanceRingPulseTimer = 0;
      s.player.resonanceRingCursor = 0;
      s.flashText = { text: 'RING LOOP', life: 0.9, color: '#55e69a' };
    } else if (type === 'square') {
      // Square Shell uses the existing shield-charge gate, not global i-frames.
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 2);
      s.player.shieldTimer = Math.max(s.player.shieldTimer, 2.5);
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, center) <= 150) dealDamageToEnemy(s, enemy, baseDamage * 1.2, undefined, false);
      s.flashText = { text: 'SQUARE RESONANCE', life: 0.9, color: '#d4943d' };
    } else if (type === 'triangle') {
      // Triangle Arc is explicitly routed through the active linked nodes.
      const triangleNodes = (formation?.nodes || []).filter((index) => index < s.spheres.length && s.spheres[index]?.alive);
      const targets = s.enemies
        .filter((enemy) => enemy.hp > 0)
        .sort((a, b) => dist(a.pos, center) - dist(b.pos, center));
      const usedTargets = new Set<EnemyEntity>();
      for (let i = 0; i < triangleNodes.length; i++) {
        const nodeIndex = triangleNodes[i];
        const sphere = s.spheres[nodeIndex];
        if (!sphere) continue;
        const linked = getLinkedNodeIndexes(network, nodeIndex).find((index) => triangleNodes.includes(index));
        const nextIndex = linked ?? triangleNodes[(i + 1) % triangleNodes.length];
        const nextSphere = s.spheres[nextIndex];
        if (nextSphere) s.lightnings.push({ from: { ...sphere.pos }, to: { ...nextSphere.pos }, life: 0.22 });
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.38);
        const target = targets.find((enemy) => !usedTargets.has(enemy)) ?? targets[i % Math.max(1, targets.length)];
        if (target) {
          usedTargets.add(target);
          dealDamageToEnemy(s, target, baseDamage * 0.65, sphere, false);
        }
      }
      s.flashText = { text: 'TRIANGLE RESONANCE', life: 0.9, color: '#8a5a8a' };
    } else if (type === 'cluster') {
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, center) > 135) continue;
        const dx = enemy.pos.x - center.x, dy = enemy.pos.y - center.y, d = Math.hypot(dx, dy) || 1;
        enemy.pos.x += dx / d * 36;
        enemy.pos.y += dy / d * 36;
        dealDamageToEnemy(s, enemy, baseDamage * 0.9, undefined, false);
      }
      s.flashText = { text: 'CLUSTER RESONANCE', life: 0.9, color: '#c4453d' };
    } else if (type === 'line') {
      // Line Surge is consumed by the next actual Sphere attack.
      s.player.resonanceLineBurst = Math.max(s.player.resonanceLineBurst, 1);
      s.flashText = { text: 'LINE RESONANCE', life: 0.9, color: '#4a7a8a' };
    } else {
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, s.player.pos) <= 90) dealDamageToEnemy(s, enemy, baseDamage * 0.6, undefined, false);
      s.flashText = { text: 'RESONANCE', life: 0.9, color: '#d4943d' };
    }
    s.screenShake = Math.min(0.18, s.screenShake + 0.06);
  } finally {
    s.player.resonanceEventActive = false;
  }
}

export function updateResonanceRing(
  s: GameState,
  dt: number,
  network: ReturnType<typeof analyzeSphereNetwork>,
): void {
  if (s.player.resonanceRingTimer <= 0) return;
  s.player.resonanceRingTimer = Math.max(0, s.player.resonanceRingTimer - dt);
  s.player.resonanceRingPulseTimer -= dt;
  if (s.player.resonanceRingPulseTimer > 0) return;

  const ringNodes = (network.ring?.nodes || []).filter((index) => index < s.spheres.length && s.spheres[index]?.alive);
  if (ringNodes.length === 0) {
    s.player.resonanceRingTimer = 0;
    return;
  }

  s.player.resonanceRingPulseTimer = 0.42;
  const cursor = s.player.resonanceRingCursor % ringNodes.length;
  const nodeIndex = ringNodes[cursor];
  const nextIndex = ringNodes[(cursor + 1) % ringNodes.length];
  s.player.resonanceRingCursor = (cursor + 1) % ringNodes.length;

  const sphere = s.spheres[nodeIndex];
  const nextSphere = s.spheres[nextIndex];
  if (!sphere) return;

  sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.40);
  sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.30);
  if (nextSphere) s.lightnings.push({ from: { ...sphere.pos }, to: { ...nextSphere.pos }, life: 0.24 });

  for (const enemy of s.enemies) {
    if (enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= 105) {
      dealDamageToEnemy(s, enemy, (16 + s.player.level * 2) * 0.42, sphere, false);
    }
  }
}

export function chargeResonance(s: GameState, source: ResonanceSource): void {
  if (s.player.resonanceEventActive) return;
  const events = addResonanceChargeFromSource(s.player, source);
  for (let i = 0; i < events; i++) triggerResonanceEvent(s);
}

export function syncResonanceGeometry(s: GameState, network = analyzeSphereNetwork(getNetworkNodes(s))): void {
  const formation = getResonanceFormation(network);
  const key = formation ? formation.type + ':' + formation.nodes.join(',') : 'none';
  if (key === s.player.resonanceGeometryKey) return;

  if (s.player.resonanceGeometryKey !== 'none' && s.player.resonanceGeometryNodes.length > 0) {
    const previousNodes = s.player.resonanceGeometryNodes
      .map((index) => s.spheres[index]?.pos)
      .filter((pos): pos is Vec => Boolean(pos))
      .map((pos) => ({ ...pos }));
    if (previousNodes.length >= 2) {
      s.formationMemory = {
        type: s.player.resonanceGeometryKey.split(':', 1)[0],
        nodes: previousNodes,
        expiresAt: s.time + 0.9,
      };
    }
  }

  const hadFormation = s.player.resonanceGeometryKey !== 'none';
  s.player.resonanceGeometryKey = key;
  s.player.resonanceGeometryNodes = formation ? [...formation.nodes] : [];
  if (formation && (hadFormation || key !== 'none')) chargeResonance(s, 'geometry');
}



// ===== Wave spawning =====
export function claimStella(s: GameState): void {
  if (!s.pendingStella) return;

  s.pendingStella = false;
  const legendary = s.time < STELLA_LEGENDARY_CUTOFF_SECONDS
    ? pickStellaArtifactChoice(s, () => nextRandom(s))
    : null;

  if (legendary) {
    s.stellaLegendaryClaims++;
    s.pendingArtifact = [legendary];
  } else {
    s.pendingArtifact = pickArtifacts(s);
  }
  playSound('chest');
}

function emitSpherePulse(s: GameState, sphere: SphereEntity, damage: number, radius: number, color: string, slow = false, sourceSphere?: SphereEntity): void {
  for (const e of s.enemies) {
    if (e.hp <= 0 || dist(e.pos, sphere.pos) > radius) continue;
    dealDamageToEnemy(s, e, damage, sourceSphere);
    if (slow) {
      e.slowTimer = Math.max(e.slowTimer, 1.2);
      e.slowFactor = Math.min(e.slowFactor, 0.6);
    }
  }
  for (let i = 0; i < 10; i++) {
    const a = nextRandom(s) * Math.PI * 2;
    s.particles.push({
      pos: { ...sphere.pos },
      vel: { x: Math.cos(a) * rand(s,60, 160), y: Math.sin(a) * rand(s,60, 160) },
      life: 0.45, maxLife: 0.45, color, size: rand(s,2, 5),
    });
  }
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
  if (s.pendingUpgrade || s.pendingArtifact || s.pendingStella) return;

  s.time += dt;
  s.stats.time = s.time;
  // Progressive active slots: Dash is always free, then three non-Dash slots
  // open during the run. This keeps the active layer tactical rather than
  // turning the HUD into a keyboard.
  if (s.player.level >= 5) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 1);
  if (s.player.level >= 12) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 2);
  if (s.player.level >= 20) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 3);

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
    if (nextRandom(s) < 0.5) {
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
      s.player.chaosOrbBuff = nextRandom(s) < 0.5 ? 'dmg' : 'radius';
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
  if (s.player.teleportDamageBuffTimer > 0) s.player.teleportDamageBuffTimer = Math.max(0, s.player.teleportDamageBuffTimer - dt);
  if (s.player.fireCatalystTimer > 0) s.player.fireCatalystTimer = Math.max(0, s.player.fireCatalystTimer - dt);
  if (s.player.timestopTimer > 0) s.player.timestopTimer = Math.max(0, s.player.timestopTimer - dt);
  if (s.player.invulnerableTimer > 0) s.player.invulnerableTimer = Math.max(0, s.player.invulnerableTimer - dt);
  if (s.player.dodgeTimer > 0) s.player.dodgeTimer -= dt;
  for (const sphere of s.spheres) {
    if (sphere.networkDisabledTimer > 0) sphere.networkDisabledTimer = Math.max(0, sphere.networkDisabledTimer - dt);
  }
  if (s.player.contactDamageCooldown > 0) s.player.contactDamageCooldown = Math.max(0, s.player.contactDamageCooldown - dt);

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
  updateRunes(s, dt);

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
      const a = nextRandom(s) * Math.PI * 2;
      s.particles.push({
        pos: { ...s.player.pos },
        vel: { x: Math.cos(a) * rand(s,100, 250), y: Math.sin(a) * rand(s,100, 250) },
        life: 1, maxLife: 1, color: ['#8a5a8a', '#c4453d', '#d4943d', '#e8dcc0'][stage - 1], size: rand(s,3, 6),
      });
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
  const gained = amount * mult;
  // echo accumulator: spheres absorb xp
  if (s.player.evolutions.includes('echoaccumulator')) {
    s.player.sphereXpAccumulator += gained * 0.3;
  }
  s.player.xp += gained;
  while (s.player.xp >= s.player.xpToNext) {
    s.player.xp -= s.player.xpToNext;
    s.player.level++;
    // HP per level: +8 max HP and heal 8
    s.player.maxHp += BALANCE.hpPerLevel;
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + BALANCE.hpPerLevel);
    s.player.xpToNext = getXpToNextLevel(s.player.level);
    s.pendingUpgrade = generateUpgradeChoices(s);
    playSound('levelup');
  }
}

function activateRune(s: GameState, rune: RuneEntity): void {
  const radius = 240;
  switch (rune.type) {
    case 'overdrive':
      for (const sphere of s.spheres) sphere.attackTimer = Math.max(0, sphere.attackTimer - sphere.attackDelay * 0.75);
      break;
    case 'phase':
      s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 1.0);
      break;
    case 'harvest':
      for (let i = s.xpOrbs.length - 1; i >= 0; i--) {
        const orb = s.xpOrbs[i];
        if (dist(orb.pos, rune.pos) <= 450) {
          gainXp(s, orb.value);
          s.xpOrbs.splice(i, 1);
        }
      }
      break;
    case 'purge':
      for (const enemy of s.enemies) {
        if (!enemy.isBoss && enemy.hp > 0 && dist(enemy.pos, rune.pos) <= radius) {
          dealDamageToEnemy(s, enemy, 45 + s.player.level * 4);
        }
      }
      break;
    case 'resonance':
      chargeResonance(s, 'rune');
      for (const sphere of s.spheres) {
        sphere.formationHitCount += 2;
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.5);
      }
      break;
    case 'fortify':
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 2);
      break;
    case 'hunt': {
      const target = s.enemies
        .filter((enemy) => enemy.hp > 0 && (enemy.isElite || enemy.isBoss))
        .sort((a, b) => dist(a.pos, rune.pos) - dist(b.pos, rune.pos))[0];
      if (target) {
        s.player.hunterMarkTarget = target;
        s.player.hunterMarkTimer = 8;
      }
      break;
    }
    case 'echo':
      s.player.buffTimer = Math.max(s.player.buffTimer, 4);
      break;
    case 'gravity':
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, rune.pos) > radius) continue;
        const dx = rune.pos.x - enemy.pos.x;
        const dy = rune.pos.y - enemy.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        const pull = Math.min(90, d * 0.55);
        enemy.pos.x += dx / d * pull;
        enemy.pos.y += dy / d * pull;
      }
      break;
  }
  const def = RUNE_DEFS[rune.type];
  s.flashText = { text: def.name.ru.toUpperCase(), life: 1.0, color: def.color };
  for (let i = 0; i < 18; i++) {
    const a = nextRandom(s) * Math.PI * 2;
    s.particles.push({ pos: { ...rune.pos }, vel: { x: Math.cos(a) * 150, y: Math.sin(a) * 150 }, life: 0.55, maxLife: 0.55, color: def.color, size: 3 });
  }
}

function updateRunes(s: GameState, dt: number): void {
  for (let i = s.runes.length - 1; i >= 0; i--) {
    const rune = s.runes[i];
    rune.life -= dt;
    if (rune.life <= 0 || !rune.alive) {
      s.runes.splice(i, 1);
      continue;
    }
    rune.pos.y += Math.sin((s.time + i) * 3) * dt * 3;
    if (dist(rune.pos, s.player.pos) <= rune.radius + PLAYER_RADIUS) {
      activateRune(s, rune);
      s.runes.splice(i, 1);
    }
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
        s.particles.push({ pos: { ...hp.pos }, vel: { x: rand(s,-100, 100), y: rand(s,-100, 100) }, life: 0.5, maxLife: 0.5, color: '#5a8c4a', size: 3 });
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
