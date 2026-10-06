import type { GameState, SphereEntity, Vec } from './engineTypes';

export const FOLLOW_ACTIVATE_RADIUS = 160;

function distance(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function clampPosition(value: number, limit: number): number {
  return Math.max(-limit, Math.min(limit, value));
}

/**
 * The activation anchor is the centroid of Spheres currently active in the
 * canonical Network input. Temporarily disrupted Spheres are not eligible
 * for the centroid, but remain attached while FOLLOW is active.
 */
export function getFormationCentroid(s: GameState): Vec | null {
  const activeSpheres = s.spheres.filter(
    (sphere) => sphere.alive && (sphere.networkDisabledTimer || 0) <= 0,
  );
  if (activeSpheres.length === 0) return null;

  const sum = activeSpheres.reduce(
    (acc, sphere) => ({ x: acc.x + sphere.pos.x, y: acc.y + sphere.pos.y }),
    { x: 0, y: 0 },
  );

  return {
    x: sum.x / activeSpheres.length,
    y: sum.y / activeSpheres.length,
  };
}

export function canActivateFormationFollow(s: GameState): boolean {
  const centroid = getFormationCentroid(s);
  return Boolean(centroid && distance(s.player.pos, centroid) <= FOLLOW_ACTIVATE_RADIUS);
}

export function syncFormationFollow(s: GameState): void {
  if (!s.player.formationFollowActive) return;

  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    if (!sphere.formationFollowOffset) {
      sphere.formationFollowOffset = {
        x: sphere.pos.x - s.player.pos.x,
        y: sphere.pos.y - s.player.pos.y,
      };
      continue;
    }
    sphere.pos.x = s.player.pos.x + sphere.formationFollowOffset.x;
    sphere.pos.y = s.player.pos.y + sphere.formationFollowOffset.y;
  }
}

export function setFormationFollow(s: GameState, active: boolean): boolean {
  if (!active) {
    s.player.formationFollowActive = false;
    for (const sphere of s.spheres) sphere.formationFollowOffset = undefined;
    return true;
  }

  if (s.player.formationFollowActive) return true;
  if (!canActivateFormationFollow(s)) return false;

  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    sphere.formationFollowOffset = {
      x: sphere.pos.x - s.player.pos.x,
      y: sphere.pos.y - s.player.pos.y,
    };
  }

  s.player.formationFollowActive = true;
  syncFormationFollow(s);
  return true;
}

export function updateSphereFollowOffset(s: GameState, sphere: SphereEntity): void {
  if (!s.player.formationFollowActive || !sphere.alive) return;
  sphere.formationFollowOffset = {
    x: sphere.pos.x - s.player.pos.x,
    y: sphere.pos.y - s.player.pos.y,
  };
}

/**
 * Move the Core and, when FOLLOW is active, apply the same realized Core
 * displacement to the attached formation.
 */
export function applyCoreDisplacement(s: GameState, dx: number, dy: number): Vec {
  const from = { ...s.player.pos };
  const halfWidth = s.worldWidth / 2;
  const halfHeight = s.worldHeight / 2;

  s.player.pos.x = clampPosition(from.x + dx, halfWidth);
  s.player.pos.y = clampPosition(from.y + dy, halfHeight);

  const applied = {
    x: s.player.pos.x - from.x,
    y: s.player.pos.y - from.y,
  };

  if (s.player.formationFollowActive && (applied.x !== 0 || applied.y !== 0)) {
    syncFormationFollow(s);
  }

  return applied;
}
