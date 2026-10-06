import type { AbilityType } from './gameData';
import type { GameState, SphereEntity, Vec } from './engineTypes';

export const FOLLOW_ACTIVATE_RADIUS = 160;
export const FOLLOW_STRAIN_MAX = 100;
export const FOLLOW_STRAIN_BUILD_PER_SECOND = 0.80;
export const FOLLOW_STRAIN_MOVING_RECOVERY_PER_SECOND = 0.55;
export const FOLLOW_STRAIN_IDLE_RECOVERY_PER_SECOND = 8;
export const FOLLOW_MAX_STEERING_PENALTY = 0.18;
export const FOLLOW_MAX_STRAIN_SPEED_PENALTY = 0.14;
export const FOLLOW_MIN_MOVEMENT_MULTIPLIER = 0.72;
export const FOLLOW_MAX_MOVEMENT_MULTIPLIER = FOLLOW_MIN_MOVEMENT_MULTIPLIER;
export const FOLLOW_MIN_NETWORK_EFFICIENCY = 0.72;
export const FOLLOW_MIN_RESONANCE_EFFICIENCY = 0.65;
export const FOLLOW_MOVEMENT_ABILITIES: ReadonlySet<AbilityType> = new Set(['teleport']);
export const FOLLOW_BREACH_ROLES = new Set(['charger', 'phase'] as const);

function distance(a: Vec, b: Vec): number { return Math.hypot(a.x - b.x, a.y - b.y); }
function clampPosition(value: number, limit: number): number { return Math.max(-limit, Math.min(limit, value)); }
function clamp01(value: number): number { return Math.max(0, Math.min(1, value)); }
function normalizeDirection(x: number, y: number): Vec | null {
  const length = Math.hypot(x, y);
  return length <= 0.0001 ? null : { x: x / length, y: y / length };
}
function directionTurnRatio(previous: Vec, current: Vec): number {
  if (Math.hypot(previous.x, previous.y) <= 0.0001) return 0;
  return clamp01((1 - (previous.x * current.x + previous.y * current.y)) / 2);
}

export function getFormationCentroid(s: GameState): Vec | null {
  const activeSpheres = s.spheres.filter((sphere) => sphere.alive && (sphere.networkDisabledTimer || 0) <= 0);
  if (activeSpheres.length === 0) return null;
  const sum = activeSpheres.reduce(
    (acc, sphere) => ({ x: acc.x + sphere.pos.x, y: acc.y + sphere.pos.y }),
    { x: 0, y: 0 },
  );
  return { x: sum.x / activeSpheres.length, y: sum.y / activeSpheres.length };
}
export function canActivateFormationFollow(s: GameState): boolean {
  const centroid = getFormationCentroid(s);
  return Boolean(centroid && distance(s.player.pos, centroid) <= FOLLOW_ACTIVATE_RADIUS);
}
export function getFormationFollowNetworkEfficiency(s: GameState): number {
  if (!s.player.formationFollowActive) return 1;
  const strain = clamp01((s.player.formationFollowStrain || 0) / FOLLOW_STRAIN_MAX);
  return 1 - strain * (1 - FOLLOW_MIN_NETWORK_EFFICIENCY);
}
export function getFormationFollowResonanceEfficiency(s: GameState): number {
  if (!s.player.formationFollowActive) return 1;
  const strain = clamp01((s.player.formationFollowStrain || 0) / FOLLOW_STRAIN_MAX);
  return 1 - strain * (1 - FOLLOW_MIN_RESONANCE_EFFICIENCY);
}
export function isFormationFollowMovementAbility(ability: AbilityType): boolean {
  return FOLLOW_MOVEMENT_ABILITIES.has(ability);
}
export function getFormationFollowMovementMultiplier(s: GameState, dx: number, dy: number): number {
  if (!s.player.formationFollowActive) return 1;
  const current = normalizeDirection(dx, dy);
  if (!current) return 1;
  const turnRatio = directionTurnRatio(s.player.formationFollowLastDirection || { x: 0, y: 0 }, current);
  const strainRatio = clamp01((s.player.formationFollowStrain || 0) / FOLLOW_STRAIN_MAX);
  return Math.max(
    FOLLOW_MIN_MOVEMENT_MULTIPLIER,
    1 - FOLLOW_MAX_STEERING_PENALTY * turnRatio - FOLLOW_MAX_STRAIN_SPEED_PENALTY * strainRatio,
  );
}
export function updateFormationFollowMotion(s: GameState, dx: number, dy: number, dt: number): void {
  if (!s.player.formationFollowActive) {
    s.player.formationFollowStrain = 0;
    s.player.formationFollowLastDirection = { x: 0, y: 0 };
    return;
  }
  const current = normalizeDirection(dx, dy);
  if (!current) {
    s.player.formationFollowStrain = Math.max(0, s.player.formationFollowStrain - FOLLOW_STRAIN_IDLE_RECOVERY_PER_SECOND * dt);
    s.player.formationFollowLastDirection = { x: 0, y: 0 };
    return;
  }
  const turnRatio = directionTurnRatio(s.player.formationFollowLastDirection || { x: 0, y: 0 }, current);
  const continuousCost = Math.max(0, FOLLOW_STRAIN_BUILD_PER_SECOND - FOLLOW_STRAIN_MOVING_RECOVERY_PER_SECOND) * dt;
  const steeringCost = turnRatio * 14;
  s.player.formationFollowStrain = Math.max(
    0,
    Math.min(FOLLOW_STRAIN_MAX, s.player.formationFollowStrain + continuousCost + steeringCost),
  );
  s.player.formationFollowLastDirection = current;
}
export function syncFormationFollow(s: GameState): void {
  if (!s.player.formationFollowActive) return;
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    if (!sphere.formationFollowOffset) {
      sphere.formationFollowOffset = { x: sphere.pos.x - s.player.pos.x, y: sphere.pos.y - s.player.pos.y };
      continue;
    }
    sphere.pos.x = s.player.pos.x + sphere.formationFollowOffset.x;
    sphere.pos.y = s.player.pos.y + sphere.formationFollowOffset.y;
  }
}
export function setFormationFollow(s: GameState, active: boolean): boolean {
  if (!active) {
    s.player.formationFollowActive = false;
    s.player.formationFollowStrain = 0;
    s.player.formationFollowLastDirection = { x: 0, y: 0 };
    for (const sphere of s.spheres) sphere.formationFollowOffset = undefined;
    return true;
  }
  if (s.player.formationFollowActive) return true;
  if (!canActivateFormationFollow(s)) return false;
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    sphere.formationFollowOffset = { x: sphere.pos.x - s.player.pos.x, y: sphere.pos.y - s.player.pos.y };
  }
  s.player.formationFollowActive = true;
  s.player.formationFollowStrain = 0;
  s.player.formationFollowLastDirection = { x: 0, y: 0 };
  syncFormationFollow(s);
  return true;
}
export function updateSphereFollowOffset(s: GameState, sphere: SphereEntity): void {
  if (!s.player.formationFollowActive || !sphere.alive) return;
  sphere.formationFollowOffset = { x: sphere.pos.x - s.player.pos.x, y: sphere.pos.y - s.player.pos.y };
}
export function applyCoreDisplacement(s: GameState, dx: number, dy: number): Vec {
  const from = { ...s.player.pos };
  s.player.pos.x = clampPosition(from.x + dx, s.worldWidth / 2);
  s.player.pos.y = clampPosition(from.y + dy, s.worldHeight / 2);
  const applied = { x: s.player.pos.x - from.x, y: s.player.pos.y - from.y };
  if (s.player.formationFollowActive && (applied.x !== 0 || applied.y !== 0)) syncFormationFollow(s);
  return applied;
}
