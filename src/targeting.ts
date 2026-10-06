export type SphereTargetingRule = 'nearest' | 'high_value_far' | 'area_control' | 'highest_hp' | 'lowest_hp';

export interface TargetPosition {
  x: number;
  y: number;
}

export interface TargetableEnemy {
  pos: TargetPosition;
  hp: number;
  isBoss?: boolean;
  isElite?: boolean;
  type?: 'normal' | 'fast' | 'tank' | 'elite' | 'boss';
}

function distanceSquared(a: TargetPosition, b: TargetPosition): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  return dx * dx + dy * dy;
}

function valueTier(enemy: TargetableEnemy): number {
  if (enemy.isBoss || enemy.type === 'boss') return 4;
  if (enemy.isElite || enemy.type === 'elite') return 3;
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
  let best: T | null = null;
  let bestDistance = Infinity;
  let bestHp = rule === 'lowest_hp' ? Infinity : -Infinity;
  let bestTier = -Infinity;

  for (const enemy of enemies) {
    if (enemy.hp <= 0) continue;
    const distance = distanceSquared(enemy.pos, spherePos);
    if (distance > rangeSquared) continue;

    if (!best) {
      best = enemy;
      bestDistance = distance;
      bestHp = enemy.hp;
      bestTier = valueTier(enemy);
      continue;
    }

    if (rule === 'highest_hp') {
      if (enemy.hp > bestHp) {
        best = enemy;
        bestHp = enemy.hp;
      }
      continue;
    }

    if (rule === 'lowest_hp') {
      if (enemy.hp < bestHp) {
        best = enemy;
        bestHp = enemy.hp;
      }
      continue;
    }

    if (rule === 'high_value_far') {
      const tier = valueTier(enemy);
      if (tier > bestTier || (tier === bestTier && distance > bestDistance)) {
        best = enemy;
        bestTier = tier;
        bestDistance = distance;
      }
      continue;
    }

    // nearest and area_control use the same closest-in-range selection.
    if (distance < bestDistance) {
      best = enemy;
      bestDistance = distance;
    }
  }

  return best;
}
