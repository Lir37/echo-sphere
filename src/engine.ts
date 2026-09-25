import type { GameState } from './engineTypes';

// Public engine facade. Gameplay implementations live in dedicated runtime modules.
// Keep this file intentionally small: callers should depend on this stable API.

export type {
  GameState, ShopState, PlayerState, SphereEntity, EnemyEntity, SphereProjectile, SphereMods,
  SphereUpgradeChoice, UpgradeChoice, DamageNumber, ChestEntity, RuneEntity, MinionEntity,
  LightningBolt, BossProjectile, XPOrb, HealthPack, Particle, FireTrailSegment, Vec,
} from './engineTypes';

export { BALANCE } from './engineBalance';

export {
  DEFAULT_MAX_SPHERES, MAX_SPHERES_CAP, BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY,
  getMaxSpheres, getSphereRadius, getSphereDamage, getSphereDpsEstimate, getSphereDelay,
  setSphereType, placeSphere, removeSphere,
} from './engineSpheres';

export { getSlowRadius, getSlowFactor } from './engineEnemies';

export {
  getCritChance, getDodgeChance, getVampirePercent, getDamageTakenMult,
} from './engineCombat';

export { activateByKey } from './engineAbilities';

export {
  assignHotkey, getUpgradeSourceWeight, getSphereUpgradeChoiceWeight,
  generateUpgradeChoices, applyUpgrade, applySphereUpgrade,
} from './engineProgression';

export type { LeaderEntry, MapTheme } from './engineState';
export {
  BASE_PLAYER_SPEED, PLAYER_RADIUS, STELLA_LEGENDARY_CUTOFF_SECONDS,
  getXpToNextLevel, MAP_THEMES, createInitialState,
} from './engineState';

export { getMoveSpeed, getXpMult, getMagnetRadius } from './engineStats';

export {
  claimStella, applyArtifact, update, activateDash,
} from './engineLoop';

import {
  triggerResonanceEvent as triggerResonanceEventRuntime,
  updateResonanceRing as updateResonanceRingRuntime,
  chargeResonance as chargeResonanceRuntime,
  syncResonanceGeometry as syncResonanceGeometryRuntime,
} from './engineResonance';
import { dealDamageToEnemy } from './engineCombat';

export function triggerResonanceEvent(s: GameState): void {
  triggerResonanceEventRuntime(s, dealDamageToEnemy);
}

export function updateResonanceRing(
  s: GameState,
  dt: number,
  network: Parameters<typeof updateResonanceRingRuntime>[2],
): void {
  updateResonanceRingRuntime(s, dt, network, dealDamageToEnemy);
}

export function chargeResonance(
  s: GameState,
  source: Parameters<typeof chargeResonanceRuntime>[1],
): void {
  chargeResonanceRuntime(s, source, dealDamageToEnemy);
}

export function syncResonanceGeometry(
  s: GameState,
  network?: Parameters<typeof syncResonanceGeometryRuntime>[1],
): void {
  syncResonanceGeometryRuntime(s, network, dealDamageToEnemy);
}
