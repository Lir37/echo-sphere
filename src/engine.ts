import type { GameState } from './engineTypes';

// Public engine facade. Gameplay implementations live in dedicated runtime modules.
// Keep this file intentionally small: callers should depend on this stable API.

export type {
  GameState, ShopState, PlayerState, SphereEntity, EnemyEntity, SphereProjectile, SphereMods,
  SphereUpgradeChoice, UpgradeChoice, DamageNumber, ChestEntity, RuneEntity, MinionEntity,
  LightningBolt, BossProjectile, XPOrb, HealthPack, Particle, FireTrailSegment, Vec,
} from './engineTypes';

export { BALANCE } from './engineBalance';

export { getNetworkFrame } from './engineRuntime';

export {
  FOLLOW_ACTIVATE_RADIUS,
  getFormationCentroid,
  canActivateFormationFollow,
  setFormationFollow,
  syncFormationFollow,
  applyCoreDisplacement,
  updateSphereFollowOffset,
} from './formationFollow.ts';

export {
  DEFAULT_MAX_SPHERES, MAX_SPHERES_CAP, BASE_SPHERE_RADIUS, BASE_SPHERE_DAMAGE, BASE_SPHERE_DELAY,
  getMaxSpheres, getSphereRadius, getSphereDamage, getSphereDpsEstimate, getSphereDelay,
  setSphereType, placeSphere, removeSphere,
} from './engineSpheres';

export { getSlowRadius, getSlowFactor } from './engineEnemies';

export {
  getCritChance, getDodgeChance, getVampirePercent, getCooldownMult, getDamageTakenMult, damagePlayerDoT,
} from './engineCombat';

export { activateByKey } from './engineAbilities';

export {
  assignHotkey, getUpgradeSourceWeight, getSphereUpgradeChoiceWeight, getUpgradeChoiceKey,
  generateUpgradeChoices, lockUpgradeChoice, rerollUpgradeChoices, applyUpgrade, applySphereUpgrade,
} from './engineProgression';

export type { LeaderEntry } from './engineState';
export {
  BASE_PLAYER_SPEED, PLAYER_RADIUS, STELLA_LEGENDARY_CUTOFF_SECONDS,
  getXpToNextLevel, createInitialState,
} from './engineState';

export { getMoveSpeed, getXpMult, getMagnetRadius, getBuildDiagnostics, type BuildDiagnosticRow } from './engineStats';

export {
  claimStella, applyArtifact, update, activateDash, openTutorialUpgrade,
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
