export type SphereTargetingRule = 'nearest' | 'high_value_far';

export interface TargetPosition {
  x: number;
  y: number;
}

export interface TargetableEnemy {
  pos: TargetPosition;
  hp: number;
  isBoss?: boolean;
  isElite?: boolean;
  type?: 'normal' | 'fast' | 'tank' | 'boss';
}

function distanceSquared(a: TargetPosition, b: TargetPosition): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  return dx * dx + dy * dy;
}

function valueTier(enemy: TargetableEnemy): number {
  if (enemy.isBoss || enemy.type === 'boss') return 4;
  if (enemy.isElite) return 3;
  if (enemy.type === 'tank') return 2;
  if (enemy.type === 'fast') return 1;
  return 0;
}

export function selectSphereTarget<T extends TargetableEnemy>(
  spherePos: TargetPosition,
  range: number,
  enemies: readonly T[],
  rule: SphereTargetingRule,
): T | null {
  const rangeSquared = range * range;
  const candidates = enemies.filter(
    (enemy) => enemy.hp > 0 && distanceSquared(enemy.pos, spherePos) <= rangeSquared,
  );

  if (candidates.length === 0) return null;

  if (rule === 'high_value_far') {
    let best = candidates[0];
    let bestTier = valueTier(best);
    let bestDistance = distanceSquared(best.pos, spherePos);

    for (let i = 1; i < candidates.length; i++) {
      const candidate = candidates[i];
      const tier = valueTier(candidate);
      const distance = distanceSquared(candidate.pos, spherePos);
      if (tier > bestTier || (tier === bestTier && distance > bestDistance)) {
        best = candidate;
        bestTier = tier;
        bestDistance = distance;
      }
    }
    return best;
  }

  let nearest = candidates[0];
  let nearestDistance = distanceSquared(nearest.pos, spherePos);
  for (let i = 1; i < candidates.length; i++) {
    const candidate = candidates[i];
    const distance = distanceSquared(candidate.pos, spherePos);
    if (distance < nearestDistance) {
      nearest = candidate;
      nearestDistance = distance;
    }
  }
  return nearest;
}
