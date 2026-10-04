import { SPHERE_TYPES, sphereUsesProjectileModifiers } from './gameData';
import type { SphereType } from './gameData';
import { playSound } from './audio';
import {
  getCharacterRadiusMultiplier,
  getCharacterAttackSpeedMultiplier,
  getCharacterStatusDurationMultiplier,
  getCharacterStatusDamageMultiplier,
  getCharacterId,
  applyAlchemistReaction,
  getCharacterFormation,
} from './characterRuntime';
import {
  getArtifactSphereRadiusMultiplier,
  getArtifactSphereDamageMultiplier,
  getArtifactSphereDelayMultiplier,
  getSphereArtifactDamageMultiplier,
} from './artifactSystem';
import {
  sphereModifiers, sphereLevel, getActiveSphereAbilitySynergies,
  getSphereElementForBranch, getSphereElementMasteryForBranch, SPHERE_ELEMENT_META,
} from './sphereProgression';
import { selectSphereTarget } from './targeting';
import type { SphereNetworkState } from './network';
import { getSphereNetworkProfile, getLinkedNodeIndexes, getFormationBonusMultiplier } from './network';
import { nextRandom } from './rng';
import type { GameState, SphereEntity, EnemyEntity, Vec } from './engineTypes';
import {
  dist, rand, getSphereFinalIndex, getAbilityBranchId, getNetworkFrame
} from './engineRuntime';
import {
  dealDamageToEnemy, onEnemyDeath, triggerEngineerRelay, consumeEngineerRelayBonus, emitSpherePulse
} from './engineCombat';
import {
  chargeResonance, syncResonanceGeometry, updateResonanceRing
} from './engineResonance';
import {
  DEFAULT_MAX_SPHERES, MAX_SPHERES_CAP, MAX_SAME_SPHERE_COPIES,
  BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY
} from './engineBalanceConstants';

export {
  DEFAULT_MAX_SPHERES, MAX_SPHERES_CAP, MAX_SAME_SPHERE_COPIES,
  BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY,
} from './engineBalanceConstants';

export function getMaxSpheres(s: GameState): number {
  const m = DEFAULT_MAX_SPHERES + (s.player.abilities.maxspheres || 0) + (s.shopUpgrades.spheres || 0);
  return Math.min(m, MAX_SPHERES_CAP);
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

export function getSphereDamage(s: GameState, sphere: SphereEntity, network?: SphereNetworkState): number {
  let d = BASE_SPHERE_DAMAGE;
  const lvl = s.player.abilities.damage || 0;
  d *= 1 + lvl * 0.15;
  d *= 1 + (s.shopUpgrades.dmg || 0) * 0.05;
  if (s.player.mutationStage >= 1) d *= 1.1;
  if (s.player.chaosOrbBuff === 'dmg' && s.player.chaosOrbBuffTimer > 0) d *= 1.2;
  if (s.player.teleportDamageBuffTimer > 0) d *= 2;
  if (s.player.overloadTimer > 0) d *= 1.25 + (s.player.abilities.darkritual || 0) * 0.04;
  if (s.player.fireTrailTimer > 0 && sphere && sphereModifiers(s, sphere.type, sphere).fire > 0) {
    d *= 1.2 + (s.player.abilities.firetrail || 0) * 0.025;
  }
  if (s.player.fireCatalystTimer > 0 && sphere && sphereModifiers(s, sphere.type, sphere).fire > 0) d *= 1.35;
  if (sphere && sphereModifiers(s, sphere.type, sphere).fire > 0 && getAbilityBranchId(s, 'firetrail', 4) === 'firetrail_overdrive') d *= 1.15;
  if (s.player.timestopTimer > 0 && s.player.timestopTimer <= 1 && getAbilityBranchId(s, 'timestop', 7) === 'timestop_temporal_core') d *= 2;
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
  if (sphere) {
    const networkState = network ?? getNetworkFrame(s);
    const profile = getSphereNetworkProfile(networkState, s.spheres.indexOf(sphere));
  }
  return d;
}

export function getSphereDpsEstimate(s: GameState, sphere: SphereEntity): number {
  const def = SPHERE_TYPES[sphere.type];
  const mods = sphereModifiers(s, sphere.type, sphere);
  const network = getNetworkFrame(s);
  const damage = getSphereDamage(s, sphere, network);
  const delay = Math.max(0.05, getSphereDelay(s, sphere, network) * def.delayMult * mods.delay);
  if (sphere.type === 'orbital') {
    const satellites = sphereLevel(s, 'orbital') + 1 + (s.player.artifacts.includes('orbital_crown') ? 1 : 0);
    return (damage * satellites * 2.2) / Math.max(0.12, 0.42 * mods.auraPulse);
  }
  if (sphere.type === 'prism') return (damage * Math.max(1, 1 + mods.multishot)) / delay;
  if (sphere.type === 'pulse') {
    const waves = 1 + (s.player.artifacts.includes('pulse_crown') ? 1 : 0);
    const interval = Math.max(0.25, 1.15 * mods.auraPulse * (s.player.artifacts.includes('pulse_driver') ? 0.90 : 1));
    return (damage * waves * 4) / interval;
  }
  if (sphere.type === 'gravity') return (damage * 4) / Math.max(0.25, 0.80 * mods.auraPulse);
  if (sphere.type === 'void') return (damage * (sphereLevel(s, 'void') >= 2 ? 1.30 : 1.15)) / delay;
  if (def.aura) return damage / delay;
  const shots = (1 + mods.multishot) * def.pellets;
  return (damage * shots) / delay;
}

export function getSphereDelay(s: GameState, sphere?: SphereEntity, network?: SphereNetworkState): number {
  let d = BASE_SPHERE_DELAY;
  const lvl = s.player.abilities.attackspeed || 0;
  d *= Math.pow(0.93, lvl);
  d /= Math.max(0.01, getCharacterAttackSpeedMultiplier(s));
  d *= getArtifactSphereDelayMultiplier(s);
  if (s.player.overloadTimer > 0) d *= 0.72;
  if (s.player.fireTrailTimer > 0 && sphere && sphereModifiers(s, sphere.type, sphere).fire > 0) d *= 0.78;
  if (sphere) {
    const networkState = network ?? getNetworkFrame(s);
    const profile = getSphereNetworkProfile(networkState, s.spheres.indexOf(sphere));
    const clusterBonus = getFormationBonusMultiplier(networkState, 'cluster', s.spheres.indexOf(sphere));
    if (clusterBonus > 0) d *= 1 - 0.10 * clusterBonus;
  }
  return d;
}

function applyDirectSphereStatus(s: GameState, enemy: EnemyEntity, effect: 'fire' | 'freeze' | 'poison', sourceSphere?: SphereEntity): void {
  if (enemy.hp <= 0) return;
  const mods = sourceSphere ? sphereModifiers(s, sourceSphere.type, sourceSphere) : null;
  const branch = sourceSphere ? s.player.sphereBranches?.[sourceSphere.type] : undefined;
  const element = getSphereElementForBranch(branch);
  const finalIndex = sourceSphere ? getSphereFinalIndex(s, sourceSphere.type) : null;
  const mastery = getSphereElementMasteryForBranch(branch, finalIndex);

  const fireLevel = Math.max(Number(mods?.fire ?? s.player.sphereMods.fire ?? 0), element === 'fire' ? 1 : 0);
  const freezeLevel = Math.max(Number(mods?.freeze ?? s.player.sphereMods.freeze ?? 0), element === 'freeze' ? 1 : 0);
  const poisonLevel = Math.max(Number(mods?.poison ?? s.player.sphereMods.poison ?? 0), element === 'poison' ? 1 : 0);

  if (effect === 'fire') {
    let duration = 3 * getCharacterStatusDurationMultiplier(s);
    let dps = (5 + fireLevel * 3) * getCharacterStatusDamageMultiplier(s);
    if (element === 'fire' && mastery?.id === 'fire_power') dps *= 1.35;
    if (element === 'fire' && mastery?.id === 'fire_duration') duration *= 1.45;
    if (element === 'fire' && mastery?.id === 'fire_tempo') {
      enemy.fireTickInterval = 0.12;
      enemy.fireTickTimer = 0;
    } else {
      enemy.fireTickInterval = undefined;
      enemy.fireTickTimer = 0;
    }
    if (getCharacterId(s) === 'alchemist' && s.player.characterMasteryLevel >= 3) dps *= 1.05;
    if (getCharacterId(s) === 'alchemist' && s.player.alchemistCatalystTimer > 0) {
      duration *= 1.5;
      s.player.alchemistCatalystTimer = 0;
    }
    enemy.fireTimer = Math.max(enemy.fireTimer || 0, duration);
    enemy.fireDps = dps;
  } else if (effect === 'freeze') {
    let duration = (0.5 + freezeLevel * 0.3) * getCharacterStatusDurationMultiplier(s);
    if (element === 'freeze' && mastery?.id === 'freeze_duration') duration *= 1.40;
    if (getCharacterId(s) === 'alchemist' && s.player.characterMasteryLevel >= 3) {
      duration *= 1.5;
      s.player.alchemistCatalystTimer = 0;
    }
    enemy.freezeTimer = Math.max(enemy.freezeTimer || 0, duration);
    if (element === 'freeze' && mastery?.id === 'freeze_impact') {
      enemy.freezeVulnerabilityTimer = Math.max(enemy.freezeVulnerabilityTimer || 0, duration);
      enemy.freezeVulnerabilitySource = sourceSphere?.type;
    } else {
      enemy.freezeVulnerabilityTimer = 0;
      enemy.freezeVulnerabilitySource = undefined;
    }
    if (element === 'freeze' && mastery?.id === 'freeze_permafrost') {
      enemy.slowTimer = Math.max(enemy.slowTimer, 1.0);
      enemy.slowFactor = Math.min(enemy.slowFactor, 0.72);
    }
  } else {
    let duration = 4 * getCharacterStatusDurationMultiplier(s);
    let dps = (3 + poisonLevel * 2) * getCharacterStatusDamageMultiplier(s);
    if (element === 'poison' && mastery?.id === 'poison_power') dps *= 1.35;
    if (element === 'poison' && mastery?.id === 'poison_duration') duration *= 1.45;
    if (element === 'poison' && mastery?.id === 'poison_tempo') {
      enemy.poisonTickInterval = 0.12;
      enemy.poisonTickTimer = 0;
    } else {
      enemy.poisonTickInterval = undefined;
      enemy.poisonTickTimer = 0;
    }
    if (getCharacterId(s) === 'alchemist' && s.player.characterMasteryLevel >= 3) dps *= 1.05;
    if (getCharacterId(s) === 'alchemist' && s.player.alchemistCatalystTimer > 0) {
      duration *= 1.5;
      s.player.alchemistCatalystTimer = 0;
    }
    enemy.poisonTimer = Math.max(enemy.poisonTimer || 0, duration);
    enemy.poisonDps = dps;
  }
  if (applyAlchemistReaction(s, enemy) && enemy.hp <= 0) onEnemyDeath(s, enemy, sourceSphere);
}


function isSpecialElementalBranch(branch?: string): boolean {
  return Boolean(branch && (
    branch.startsWith('chain_') ||
    branch.startsWith('aura_') ||
    branch.startsWith('gravity_') ||
    branch.startsWith('pulse_')
  ));
}

function applySpecialElementalMastery(
  s: GameState,
  enemy: EnemyEntity,
  effect: 'fire' | 'freeze' | 'poison',
  sphere: SphereEntity,
  mode: 'conduction' | 'field',
): void {
  const branch = s.player.sphereBranches?.[sphere.type];
  const mastery = getSphereElementMasteryForBranch(branch, getSphereFinalIndex(s, sphere.type));
  applyDirectSphereStatus(s, enemy, effect, sphere);

  if (mode === 'conduction') {
    if (mastery?.id === 'conduction_power') {
      if (effect === 'fire') enemy.fireDps *= 1.35;
      else if (effect === 'poison') enemy.poisonDps *= 1.35;
      else enemy.freezeTimer = Math.max(enemy.freezeTimer || 0, 0.5) * 1.35;
    }
    if (mastery?.id === 'conduction_duration') {
      if (effect === 'fire') enemy.fireTimer *= 1.45;
      else if (effect === 'poison') enemy.poisonTimer *= 1.45;
      else enemy.freezeTimer = Math.max(enemy.freezeTimer || 0, 0.5) * 1.45;
    }
  } else {
    if (mastery?.id === 'field_power') {
      if (effect === 'fire') enemy.fireDps *= 1.35;
      else if (effect === 'poison') enemy.poisonDps *= 1.35;
      else {
        enemy.freezeTimer = Math.max(enemy.freezeTimer || 0, 0.5) * 1.35;
        enemy.slowTimer = Math.max(enemy.slowTimer, 0.8);
        enemy.slowFactor = Math.min(enemy.slowFactor, 0.62);
      }
    }
    if (mastery?.id === 'field_duration') {
      if (effect === 'fire') enemy.fireTimer *= 1.45;
      else if (effect === 'poison') enemy.poisonTimer *= 1.45;
      else enemy.freezeTimer = Math.max(enemy.freezeTimer || 0, 0.5) * 1.45;
    }
  }
}

function triggerChainElementalReaction(
  s: GameState,
  enemy: EnemyEntity,
  sphere: SphereEntity,
): void {
  const branch = s.player.sphereBranches?.chain;
  const element = getSphereElementForBranch(branch);
  if (!element || enemy.hp <= 0) return;

  const mastery = getSphereElementMasteryForBranch(branch, getSphereFinalIndex(s, 'chain'));
  const threshold = mastery?.id === 'conduction_rate' ? 2 : 3;
  const source = enemy.elementalConductionSource;
  if (source !== 'chain') {
    enemy.elementalConductionSource = 'chain';
    enemy.elementalConduction = 0;
  }
  if ((enemy.elementalReactionTimer || 0) > 0) return;

  enemy.elementalConduction = Math.min(threshold, (enemy.elementalConduction || 0) + 1);
  if ((enemy.elementalConduction || 0) < threshold) return;

  enemy.elementalConduction = 0;
  enemy.elementalReactionTimer = mastery?.id === 'conduction_rate' ? 1.4 : 2.0;
  const reactionDamage = sphere.damage * (mastery?.id === 'conduction_power' ? 0.27 : 0.20);
  dealDamageToEnemy(s, enemy, reactionDamage, sphere, false);
  if (enemy.hp <= 0) return;

  applySpecialElementalMastery(s, enemy, element, sphere, 'conduction');
  s.particles.push({
    pos: { ...enemy.pos },
    vel: { x: 0, y: -18 },
    life: 0.38,
    maxLife: 0.38,
    color: SPHERE_ELEMENT_META[element].color,
    size: 5,
  });
}

function applyElementalFieldReaction(
  s: GameState,
  enemy: EnemyEntity,
  sphere: SphereEntity,
): void {
  const branch = s.player.sphereBranches?.[sphere.type];
  const element = getSphereElementForBranch(branch);
  if (!element || !isSpecialElementalBranch(branch) || enemy.hp <= 0) return;

  const mastery = getSphereElementMasteryForBranch(branch, getSphereFinalIndex(s, sphere.type));
  if ((enemy.elementalReactionTimer || 0) > 0) return;
  enemy.elementalReactionTimer = mastery?.id === 'field_frequency' ? 1.25 : 1.8;

  applySpecialElementalMastery(s, enemy, element, sphere, 'field');
  s.particles.push({
    pos: { ...enemy.pos },
    vel: { x: 0, y: -14 },
    life: 0.34,
    maxLife: 0.34,
    color: SPHERE_ELEMENT_META[element].color,
    size: 4,
  });
}

function getActiveStatusEffect(s: GameState, sphere?: SphereEntity): 'none' | 'fire' | 'freeze' | 'poison' {
  const branch = sphere ? s.player.sphereBranches?.[sphere.type] : undefined;
  const element = getSphereElementForBranch(branch);
  if (element) return element;
  const mods = sphere ? sphereModifiers(s, sphere.type, sphere) : null;
  if ((mods?.fire ?? s.player.sphereMods.fire) > 0) return 'fire';
  if ((mods?.freeze ?? s.player.sphereMods.freeze) > 0) return 'freeze';
  if ((mods?.poison ?? s.player.sphereMods.poison) > 0) return 'poison';
  return 'none';
}

function updateOrbitalSphere(s: GameState, sphere: SphereEntity, damage: number, mods: ReturnType<typeof sphereModifiers>, networkProfile: ReturnType<typeof getSphereNetworkProfile>, network: SphereNetworkState, dt: number): void {
  const branch = s.player.sphereBranches?.orbital;
  const finalIndex = getSphereFinalIndex(s, 'orbital');
  const satelliteCount = Math.max(1, sphereLevel(s, 'orbital') + 1 + (s.player.artifacts.includes('orbital_crown') ? 1 : 0) + (finalIndex === 2 ? 1 : 0));
  let angularSpeed = 1.8 + Math.min(2.4, sphereLevel(s, 'orbital') * 0.28);
  if (branch === 'orbital_dance') angularSpeed *= finalIndex === 1 ? 1.55 : 1.28;
  if (branch === 'orbital_halo') angularSpeed *= 1.08;
  if (branch === 'orbital_blade') angularSpeed *= 1.12;
  sphere.rotation += angularSpeed * dt;

  sphere.auraTimer -= dt;
  if (sphere.auraTimer > 0) return;
  sphere.auraTimer = Math.max(0.12, 0.42 * mods.auraPulse);

  let orbitRadius = (78 + 12 * Math.min(7, sphereLevel(s, 'orbital'))) * mods.radius;
  const sphereIndex = s.spheres.indexOf(sphere);
  const clusterBonus = getFormationBonusMultiplier(network, 'cluster', sphereIndex);
  const ringBonus = getFormationBonusMultiplier(network, 'ring', sphereIndex);
  if (clusterBonus > 0) orbitRadius *= 1 + 0.08 * clusterBonus;
  if (ringBonus > 0) orbitRadius *= 1 + 0.12 * ringBonus;
  orbitRadius *= 1 + Math.min(0.20, networkProfile.linkedNeighbours * 0.03);

  const status = getActiveStatusEffect(s, sphere);
  const band = finalIndex === 1 ? 26 : 19;
  let hitSomething = false;
  for (const enemy of s.enemies) {
    if (enemy.hp <= 0) continue;
    const dx = enemy.pos.x - sphere.pos.x;
    const dy = enemy.pos.y - sphere.pos.y;
    const d = Math.hypot(dx, dy) || 1;
    if (Math.abs(d - orbitRadius) > band) continue;
    const angle = Math.atan2(dy, dx);
    let bestAngularDistance = Math.PI;
    for (let satellite = 0; satellite < satelliteCount; satellite++) {
      const satelliteAngle = sphere.rotation + satellite * (Math.PI * 2 / satelliteCount);
      const diff = Math.atan2(Math.sin(angle - satelliteAngle), Math.cos(angle - satelliteAngle));
      bestAngularDistance = Math.min(bestAngularDistance, Math.abs(diff));
    }
    if (bestAngularDistance > (finalIndex === 1 ? 0.30 : 0.22)) continue;

    let hitDamage = damage * 0.95;
    if (branch === 'orbital_dance') hitDamage *= 0.96;
    if (branch === 'orbital_halo') hitDamage *= 0.92;
    if (branch === 'orbital_blade') hitDamage *= finalIndex === 2 ? 1.30 : 1.15;
    if (clusterBonus > 0) hitDamage *= 1 + 0.08 * clusterBonus;

    dealDamageToEnemy(s, enemy, hitDamage, sphere);
    if (status !== 'none') applyDirectSphereStatus(s, enemy, status, sphere);
    hitSomething = true;

    if (branch === 'orbital_halo' && finalIndex === 2) {
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
    }
  }

  if (networkProfile.ring && s.player.artifacts.includes('orbital_blade') && s.player.artifacts.includes('prism_filter')) {
    const linked = getLinkedNodeIndexes(network, s.spheres.indexOf(sphere))
      .filter((index) => index < s.spheres.length && s.spheres[index]?.alive);
    if (linked.length > 0) {
      const relay = s.spheres[linked[0]];
      s.lightnings.push({ from: { ...sphere.pos }, to: { ...relay.pos }, life: 0.16 });
    }
  }

  if (hitSomething) triggerEngineerRelay(s, sphere);
  s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.22, maxLife: 0.22, color: SPHERE_TYPES.orbital.color, size: finalIndex === 2 ? 9 : 7 });
}

function updatePrismSphere(s: GameState, sphere: SphereEntity, damage: number, radius: number, mods: ReturnType<typeof sphereModifiers>, networkProfile: ReturnType<typeof getSphereNetworkProfile>, network: SphereNetworkState, dt: number): void {
  const branch = s.player.sphereBranches?.prism;
  const finalIndex = getSphereFinalIndex(s, 'prism');
  sphere.attackTimer -= dt;
  if (sphere.attackTimer > 0) return;

  const delay = Math.max(0.14, getSphereDelay(s, sphere) * SPHERE_TYPES.prism.delayMult * mods.delay);
  sphere.attackTimer = delay;

  const candidates = s.enemies
    .filter((enemy) => enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= radius)
    .sort((a, b) => b.hp - a.hp);
  if (candidates.length === 0) return;

    // Prism is a directed beam emitter and its visual weapon follows the selected target.
  sphere.rotation = Math.atan2(candidates[0].pos.y - sphere.pos.y, candidates[0].pos.x - sphere.pos.x);

let beamCount = Math.max(1, 1 + mods.multishot);
  if (branch === 'prism_split' && finalIndex === 2) beamCount += 1;
  const status = getActiveStatusEffect(s, sphere);
  const used = new Set<EnemyEntity>();

  for (let beam = 0; beam < beamCount; beam++) {
    const target = candidates.find((enemy) => !used.has(enemy)) || candidates[beam % candidates.length];
    if (!target) continue;
    used.add(target);

    let beamDamage = damage;
    if (branch === 'prism_split') beamDamage *= beam === 0 ? 1.0 : (finalIndex === 1 ? 0.72 : 0.62);
    if (branch === 'prism_spectrum' && status !== 'none') beamDamage *= 1.08;
    const sphereIndex = s.spheres.indexOf(sphere);
    const lineBonus = getFormationBonusMultiplier(network, 'line', sphereIndex);
    const latticeBonus = getFormationBonusMultiplier(network, 'lattice', sphereIndex);
    if (lineBonus > 0) beamDamage *= 1 + 0.12 * lineBonus;
    if (latticeBonus > 0) beamDamage *= 1 + 0.08 * latticeBonus;

    dealDamageToEnemy(s, target, beamDamage, sphere);
    if (status !== 'none') applyDirectSphereStatus(s, target, status, sphere);
    s.lightnings.push({ from: { ...sphere.pos }, to: { ...target.pos }, life: 0.10 });
  }

  if (branch === 'prism_mirror' || s.player.artifacts.includes('prism_filter')) {
    const sphereIndex = s.spheres.indexOf(sphere);
    const ringBonus = getFormationBonusMultiplier(network, 'ring', sphereIndex);
    const ringReflection = ringBonus >= 1 || (ringBonus > 0 && nextRandom(s) < ringBonus) ? 1 : 0;
    const reflectionCount = (branch === 'prism_mirror' ? (finalIndex === 2 ? 2 : 1) : 0)
      + (s.player.artifacts.includes('prism_filter') ? 1 : 0)
      + (s.player.artifacts.includes('prism_crown') ? 1 : 0)
      + ringReflection;
    if (reflectionCount > 0) {
      const linked = getLinkedNodeIndexes(network, s.spheres.indexOf(sphere))
        .filter((index) => index < s.spheres.length && s.spheres[index]?.alive)
        .slice(0, reflectionCount);
      const target = candidates[0];
      for (const index of linked) {
        const relay = s.spheres[index];
        dealDamageToEnemy(s, target, damage * 0.42, relay, false);
        if (status !== 'none') applyDirectSphereStatus(s, target, status, sphere);
        s.lightnings.push({ from: { ...relay.pos }, to: { ...target.pos }, life: 0.12 });
      }
    }
  }

  if (networkProfile.triangle && branch === 'prism_spectrum') chargeResonance(s, 'network', dealDamageToEnemy);
  triggerEngineerRelay(s, sphere);
}

function updateGravitySphere(s: GameState, sphere: SphereEntity, damage: number, mods: ReturnType<typeof sphereModifiers>, networkProfile: ReturnType<typeof getSphereNetworkProfile>, dt: number): void {
  const branch = s.player.sphereBranches?.gravity;
  const finalIndex = getSphereFinalIndex(s, 'gravity');
  sphere.auraTimer -= dt;
  sphere.rotation += dt * (branch === 'gravity_tide' ? 1.9 : 0.9);
  if (sphere.auraTimer > 0) return;
  sphere.auraTimer = Math.max(0.22, 0.80 * mods.auraPulse * (s.player.artifacts.includes('gravity_hook') ? 0.92 : 1));

  const pullRadius = SPHERE_TYPES.gravity.auraRadius * mods.radius * mods.auraRadius;
  let pullStrength = 34 * Math.min(1.6, sphereLevel(s, 'gravity') * 0.18 + 0.5);
  pullStrength *= 1 + (s.player.artifacts.includes('gravity_bead') ? 0.12 : 0);
  pullStrength *= 1 + (s.player.artifacts.includes('gravity_hook') ? 0.10 : 0);
  const sphereIndex = s.spheres.indexOf(sphere);
  const clusterBonus = getFormationBonusMultiplier((getNetworkFrame(s)), 'cluster', sphereIndex);
  if (clusterBonus > 0) pullStrength *= 1 + 0.20 * clusterBonus;
  if (branch === 'gravity_well') pullStrength *= finalIndex === 1 ? 1.35 : 1.15;
  if (branch === 'gravity_tide') pullStrength *= 1.05;
  if (branch === 'gravity_collapse') pullStrength *= 0.90;

  const phase = branch === 'gravity_tide' ? Math.sin(sphere.rotation) : 1;
  const status = getActiveStatusEffect(s, sphere);
  const grouped = s.enemies.filter((enemy) => enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= pullRadius).length;

  for (const enemy of s.enemies) {
    if (enemy.hp <= 0 || dist(enemy.pos, sphere.pos) > pullRadius) continue;
    enemy.slowTimer = Math.max(enemy.slowTimer, branch === 'gravity_well' ? 0.75 : 0.45);
    enemy.slowFactor = Math.min(enemy.slowFactor, branch === 'gravity_well' ? 0.56 : 0.72);

    let hitDamage = damage;
    if (branch === 'gravity_collapse') {
      if (grouped >= 4) hitDamage *= 1.20;
      if (enemy.hp < enemy.maxHp * 0.45) hitDamage *= finalIndex === 2 ? 1.30 : 1.12;
    }
    if (clusterBonus > 0) hitDamage *= 1 + 0.08 * clusterBonus;
    dealDamageToEnemy(s, enemy, hitDamage, sphere);
    if (status !== 'none') {
      if (isSpecialElementalBranch(branch)) applyElementalFieldReaction(s, enemy, sphere);
      else applyDirectSphereStatus(s, enemy, status, sphere);
    }
  }

  if (grouped >= 4 && branch === 'gravity_collapse' && finalIndex === 2) {
    emitSpherePulse(s, sphere, damage * 0.65, pullRadius * 0.45, SPHERE_TYPES.gravity.color, true);
  }
  triggerEngineerRelay(s, sphere);
}

function updatePulseSphere(s: GameState, sphere: SphereEntity, damage: number, mods: ReturnType<typeof sphereModifiers>, networkProfile: ReturnType<typeof getSphereNetworkProfile>, dt: number): void {
  const branch = s.player.sphereBranches?.pulse;
  const finalIndex = getSphereFinalIndex(s, 'pulse');
  sphere.auraTimer -= dt;
  if (sphere.auraTimer > 0) return;

  const waveCount = 1 + (s.player.artifacts.includes('pulse_crown') ? 1 : 0) + (branch === 'pulse_burst' && finalIndex === 2 ? 1 : 0);
  const intervalMultiplier = s.player.artifacts.includes('pulse_driver') ? 0.90 : 1;
  sphere.auraTimer = Math.max(0.25, 1.15 * mods.auraPulse * intervalMultiplier);
  let radius = SPHERE_TYPES.pulse.auraRadius * mods.radius;
  if (branch === 'pulse_wave') radius *= finalIndex === 1 ? 1.24 : 1.10;
  const sphereIndex = s.spheres.indexOf(sphere);
  const clusterBonus = getFormationBonusMultiplier(getNetworkFrame(s), 'cluster', sphereIndex);
  if (clusterBonus > 0) radius *= 1 + 0.06 * clusterBonus;

  for (let wave = 0; wave < waveCount; wave++) {
    const waveDamage = damage * (wave === 0 ? 1 : 0.46 + (branch === 'pulse_burst' ? 0.14 : 0));
    const pulseRadius = radius * (wave === 0 ? 1 : 0.68);
    emitSpherePulse(s, sphere, waveDamage, pulseRadius, SPHERE_TYPES.pulse.color, branch === 'pulse_wave', sphere);
    const pulseStatus = getActiveStatusEffect(s, sphere);
    if (pulseStatus !== 'none') {
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, sphere.pos) > pulseRadius) continue;
        if (isSpecialElementalBranch(branch)) applyElementalFieldReaction(s, enemy, sphere);
        else applyDirectSphereStatus(s, enemy, pulseStatus, sphere);
      }
    }
    if (branch === 'pulse_wave') {
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, sphere.pos) > radius) continue;
        const dx = enemy.pos.x - sphere.pos.x;
        const dy = enemy.pos.y - sphere.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        enemy.pos.x += dx / d * 18;
        enemy.pos.y += dy / d * 18;
      }
    }
  }

  if (branch === 'pulse_resonator' && (networkProfile.triangle || networkProfile.lattice || networkProfile.ring)) {
    const sphereIndex = s.spheres.indexOf(sphere);
    const formationBonus = Math.max(
      getFormationBonusMultiplier(getNetworkFrame(s), 'triangle', sphereIndex),
      getFormationBonusMultiplier(getNetworkFrame(s), 'lattice', sphereIndex),
      getFormationBonusMultiplier(getNetworkFrame(s), 'ring', sphereIndex),
    );
    chargeResonance(s, 'network', dealDamageToEnemy, formationBonus || 1);
  }
  if (branch === 'pulse_burst' && networkProfile.cluster) chargeResonance(s, 'geometry', dealDamageToEnemy);
  triggerEngineerRelay(s, sphere);
}

export function updateSpheres(s: GameState, dt: number): void {
  // Network topology is stable for the duration of this Sphere update pass.
  // Analyze it once so Resonance geometry cannot double-charge within a frame
  // and the per-Sphere profiles all observe the same topology snapshot.
  const networkState = getNetworkFrame(s);
  syncResonanceGeometry(s, networkState, dealDamageToEnemy);
  updateResonanceRing(s, dt, networkState, dealDamageToEnemy);

  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    sphere.resonancePulseTimer = Math.max(0, sphere.resonancePulseTimer - dt);
    const stype = SPHERE_TYPES[sphere.type];
    const radius = getSphereRadius(s, sphere) * stype.rangeMult;
    const damage = getSphereDamage(s, sphere, networkState) * stype.damageMult;
    const delay = getSphereDelay(s, sphere, networkState) * stype.delayMult * sphereModifiers(s, sphere.type).delay;
    const branch = s.player.sphereBranches?.[sphere.type];
    const networkProfile = getSphereNetworkProfile(networkState, s.spheres.indexOf(sphere));
    // Area-control Sphere archetypes use distinct loops rather than pretending to be generic turrets.
    if (sphere.type === 'orbital') {
      updateOrbitalSphere(s, sphere, damage, sphereModifiers(s, sphere.type, sphere), networkProfile, networkState, dt);
      continue;
    }
    if (sphere.type === 'prism') {
      updatePrismSphere(s, sphere, damage, radius, sphereModifiers(s, sphere.type, sphere), networkProfile, networkState, dt);
      continue;
    }
    if (sphere.type === 'gravity') {
      updateGravitySphere(s, sphere, damage, sphereModifiers(s, sphere.type, sphere), networkProfile, dt);
      continue;
    }
    if (sphere.type === 'pulse') {
      updatePulseSphere(s, sphere, damage, sphereModifiers(s, sphere.type, sphere), networkProfile, dt);
      continue;
    }
    // Void has a normal projectile loop, but its identity is an execution threshold.
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
            } else if (branch === 'aura_overgrowth') {
              for (const ally of s.spheres) {
                if (ally !== sphere && ally.alive && dist(ally.pos, sphere.pos) < 110) {
                  ally.attackTimer = Math.max(0, ally.attackTimer - dt * 0.08);
                }
              }
            }
            dealDamageToEnemy(s, e, damage, sphere);
            if (branch && isSpecialElementalBranch(branch)) applyElementalFieldReaction(s, e, sphere);
            attacked = true;
          }
        }
        if (branch === 'aura_sanctum') {
          for (const ally of s.spheres) {
            if (ally !== sphere && ally.alive && dist(ally.pos, sphere.pos) < 120) {
              ally.attackTimer = Math.max(0, ally.attackTimer - 0.12);
              s.particles.push({ pos: { ...ally.pos }, vel: { x: 0, y: 0 }, life: 0.18, maxLife: 0.18, color: '#8a5a8a', size: 3 });
            }
          }
        }
        if (attacked) triggerEngineerRelay(s, sphere);
      }
      continue;
    }
    // Target selection is data-driven per Sphere type.
    const nearest = selectSphereTarget(
      sphere.pos,
      radius,
      s.enemies,
      stype.targetingRule,
    );
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
        const mods = sphereModifiers(s, sphere.type, sphere);
        const sphereMods = mods;
        const shots = sphere.type === 'shotgun' ? stype.pellets + mods.multishot : 1 + mods.multishot;
        const relayMultiplier = consumeEngineerRelayBonus(s, sphere);
        const formation = getCharacterFormation(s);
        const resonanceLineBurst = s.player.resonanceLineBurst > 0;
        if (resonanceLineBurst) s.player.resonanceLineBurst = 0;
        const formationPierce = getCharacterId(s) === 'architect' && formation.type === 'line' ? 1 : 0;
        const networkLineBonus = getFormationBonusMultiplier(networkState, 'line', s.spheres.indexOf(sphere));
        for (let i = 0; i < shots; i++) {
          const spread = shots > 1 ? (i - (shots - 1) / 2) * ((stype.spread * (sphereModifiers(s, sphere.type).spreadMult || 1)) / Math.max(1, shots - 1) || 0.15) : 0;
          const angle = Math.atan2(dirY, dirX) + spread;
          const isChainAttack = stype.chain;
          const branchElement = getSphereElementForBranch(s.player.sphereBranches?.[sphere.type]);
          let effect: 'none' | 'fire' | 'freeze' | 'poison' = branchElement ?? 'none';
          if (effect === 'none') {
            if (sphereMods.fire > 0) effect = 'fire';
            else if (sphereMods.freeze > 0) effect = 'freeze';
            else if (sphereMods.poison > 0) effect = 'poison';
          }
          let color = stype.color;
          if (effect === 'fire') color = '#ff743d';
          else if (effect === 'freeze') color = '#d9f6ff';
          else if (effect === 'poison') color = '#72f08e';
          const speed = 350 * stype.projectileSpeedMult;

          if (!isChainAttack) {
            s.sphereProjectiles.push({
              pos: { ...sphere.pos },
              vel: { x: Math.cos(angle) * speed, y: Math.sin(angle) * speed },
              damage: damage * relayMultiplier * (resonanceLineBurst ? 1.60 : 1),
              radius: 5,
              alive: true,
              color,
              pierce: mods.pierce + formationPierce + (networkLineBonus >= 1 || (networkLineBonus > 0 && nextRandom(s) < networkLineBonus) ? 1 : 0),
              hitEnemies: new Set(),
              effect,
              ricochet: sphereUsesProjectileModifiers(sphere.type) ? sphereMods.ricochet : 0,
              life: 2,
              sourceSphere: sphere,
            });
            const modifierRuntime = sphereModifiers(s, sphere.type, sphere);
            if (modifierRuntime.echoChance > 0 && nextRandom(s) < modifierRuntime.echoChance) {
              const echoAngle = angle + (nextRandom(s) - 0.5) * 0.18;
              s.sphereProjectiles.push({
                pos: { ...sphere.pos },
                vel: { x: Math.cos(echoAngle) * speed, y: Math.sin(echoAngle) * speed },
                damage: damage * relayMultiplier * 0.28,
                radius: 4,
                alive: true,
                color,
                pierce: 0,
                hitEnemies: new Set(),
                effect: 'none',
                ricochet: 0,
                life: 1.1,
                procOnHit: false,
                sourceSphere: sphere,
              });
            }
          } else {
            // Chain attack is electrical and instant: no projectile object is spawned.
            const chainTargets: EnemyEntity[] = [];
            let current = nearest;
            const hitSet = new Set<EnemyEntity>([current]);

            for (let c = 0; c < Math.max(0, sphereModifiers(s, sphere.type).chainTargets); c++) {
              let next: EnemyEntity | null = null;
              let closestDistance = Infinity;
              for (const candidate of s.enemies) {
                if (candidate.hp <= 0 || hitSet.has(candidate)) continue;
                const distance = dist(candidate.pos, current.pos);
                if (distance < 150 && distance < closestDistance) {
                  closestDistance = distance;
                  next = candidate;
                }
              }
              if (!next) break;
              chainTargets.push(next);
              hitSet.add(next);
              current = next;
            }

            const chainBranch = s.player.sphereBranches?.[sphere.type];
            const chainFinalId = (s.player.evolutions || []).find((id: string) => id.startsWith('sphere:' + sphere.type + ':7:'));
            const chainFinalIndex = chainFinalId ? Number(chainFinalId.split(':').pop()) : null;
            const toxicNetwork = getActiveSphereAbilitySynergies(s).some((link) =>
              link.character === 'alchemist' && link.sphere === 'chain' && link.ability === 'lightning'
            );

            s.lightnings.push({ from: { ...sphere.pos }, to: { ...nearest.pos }, life: 0.30, sourceSphere: sphere });

            for (let chainIndex = 0; chainIndex < chainTargets.length; chainIndex++) {
              const target = chainTargets[chainIndex];
              const stormMultiplier = chainBranch === 'chain_storm' ? 1 + chainIndex * 0.15 : 1;
              const finalMultiplier = chainFinalIndex === 0 ? 1.15 : 1;
              dealDamageToEnemy(s, target, damage * 0.7 * relayMultiplier * stormMultiplier * finalMultiplier, sphere);
              if (toxicNetwork) {
                target.fireTimer = Math.max(target.fireTimer || 0, 1.5);
                target.poisonTimer = Math.max(target.poisonTimer || 0, 1.5);
              }
              triggerChainElementalReaction(s, target, sphere);
              s.lightnings.push({ from: { ...nearest.pos }, to: { ...target.pos }, life: 0.30, sourceSphere: sphere });
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
        dealDamageToEnemy(s, e, p.damage, p.sourceSphere, p.procOnHit !== false);
        p.hitEnemies.add(e);
        hit = true;
        // impact effect particles
        for (let k = 0; k < 6; k++) {
          const a = nextRandom(s) * Math.PI * 2;
          s.particles.push({ pos: { ...p.pos }, vel: { x: Math.cos(a) * 80, y: Math.sin(a) * 80 }, life: 0.3, maxLife: 0.3, color: p.color, size: 2 });
        }
        // Elemental status is routed through the same authoritative path as direct hits.
        if (p.effect !== 'none') applyDirectSphereStatus(s, e, p.effect, p.sourceSphere);
        const hitMods = p.sourceSphere ? sphereModifiers(s, p.sourceSphere.type, p.sourceSphere) : null;
        if (hitMods && hitMods.splitChance > 0 && p.procOnHit !== false && nextRandom(s) < hitMods.splitChance) {
          const baseAngle = Math.atan2(p.vel.y, p.vel.x);
          for (const offset of [-0.34, 0.34]) {
            const shardAngle = baseAngle + offset;
            s.sphereProjectiles.push({
              pos: { ...p.pos },
              vel: { x: Math.cos(shardAngle) * 290, y: Math.sin(shardAngle) * 290 },
              damage: p.damage * 0.30,
              radius: 3.5,
              alive: true,
              color: p.color,
              pierce: 0,
              hitEnemies: new Set(p.hitEnemies),
              effect: 'none',
              ricochet: 0,
              life: 0.9,
              procOnHit: false,
              sourceSphere: p.sourceSphere,
            });
          }
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

export function setSphereType(s: GameState, type: SphereType): void {
  s.selectedSphereType = type;
  playSound('place');
}

export function placeSphere(s: GameState, x: number, y: number): void {
  const selectedType = s.selectedSphereType;
  const sameTypeCount = s.spheres.filter((sphere) => sphere.alive && sphere.type === selectedType).length;
  // toggle: if clicking near an existing sphere, remove it instead
  const existing = s.spheres.find(sp => sp.alive && Math.hypot(sp.pos.x - x, sp.pos.y - y) < 26);
  if (existing) {
    removeSphere(s, existing);
    return;
  }
  const max = getMaxSpheres(s);
  if (s.spheres.length >= max) return;
  if (sameTypeCount >= MAX_SAME_SPHERE_COPIES) {
    s.flashText = { text: 'MAX 2', life: 0.8, color: SPHERE_TYPES[selectedType].color };
    return;
  }
  s.spheres.push({
    pos: { x, y },
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
    visualTier: sphereLevel(s, s.selectedSphereType),
    type: s.selectedSphereType,
    auraTimer: 0,
  });
  if (!s.player.knownSphereTypes.includes(selectedType)) s.player.knownSphereTypes.push(selectedType);

  const stype = SPHERE_TYPES[s.selectedSphereType];
  for (let i = 0; i < 15; i++) {
    const a = nextRandom(s) * Math.PI * 2;
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
    const a = nextRandom(s) * Math.PI * 2;
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.5, maxLife: 0.5, color: '#b8475a', size: 3 });
  }
}
