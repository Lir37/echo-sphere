export const CRIT_BASE = 0.05;
export const CRIT_MULTIPLIER_BASE = 1.5;
export const CRIT_HARD_CAP = 0.75;

export interface ContextualCritBonuses {
  hunterMarked?: boolean;
  architectTriangle?: boolean;
}

export function clampCritChance(value: number): number {
  if (!Number.isFinite(value)) return 0;
  return Math.max(0, Math.min(CRIT_HARD_CAP, value));
}

export function getContextualCritChance(
  baseChance: number,
  context: ContextualCritBonuses = {},
): number {
  let chance = Number.isFinite(baseChance) ? baseChance : CRIT_BASE;
  if (context.hunterMarked) chance += 0.02;
  if (context.architectTriangle) chance += 0.10;
  return clampCritChance(chance);
}

export function canReceivePlayerDamage(
  invulnerableTimer: number,
  contactDamageCooldown: number,
): boolean {
  const invulnerable = Number.isFinite(invulnerableTimer) && invulnerableTimer > 0;
  const grace = Number.isFinite(contactDamageCooldown) && contactDamageCooldown > 0;
  return !invulnerable && !grace;
}
