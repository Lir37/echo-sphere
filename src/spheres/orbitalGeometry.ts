const TAU = Math.PI * 2;

export type OrbitalRing = 'inner' | 'outer';

export interface OrbitalRingCounts {
  inner: number;
  outer: number;
}

/**
 * Core Orbital progression:
 * L1 = 1 element on the original clockwise orbit + 1 on the larger counter-clockwise orbit.
 * Each later level adds exactly one element, alternating outer/inner:
 * L2 outer, L3 inner, L4 outer, L5 inner, L6 outer, L7 inner.
 *
 * Extra combat elements from artifacts/final forms are added after the canonical
 * 1..7 progression and are balanced between the two rings.
 */
export function getOrbitalRingCounts(level: number, extraElements = 0): OrbitalRingCounts {
  const effectiveLevel = Math.max(1, Math.min(7, Math.floor(level)));
  let inner = 1;
  let outer = 1;

  for (let currentLevel = 2; currentLevel <= effectiveLevel; currentLevel += 1) {
    if (currentLevel % 2 === 0) outer += 1;
    else inner += 1;
  }

  for (let extra = 0; extra < Math.max(0, Math.floor(extraElements)); extra += 1) {
    if (outer < inner) outer += 1;
    else if (inner < outer) inner += 1;
    else if (extra % 2 === 0) outer += 1;
    else inner += 1;
  }

  return { inner, outer };
}

export function orbitalElementAngle(
  rotation: number,
  ring: OrbitalRing,
  index: number,
  count: number,
): number {
  const safeCount = Math.max(1, count);
  // Canvas screen coordinates have +Y downward, so +angle is clockwise.
  const direction = ring === 'inner' ? 1 : -1;
  return rotation * direction + index * (TAU / safeCount);
}

export function isAngleOnOrbitalSweep(
  targetAngle: number,
  startAngle: number,
  endAngle: number,
  direction: 1 | -1,
  tolerance = 0,
): boolean {
  const tau = Math.PI * 2;
  const safeTolerance = Math.max(0, tolerance);
  const normalizeAnglePositive = (angle: number): number => ((angle % tau) + tau) % tau;
  const travelled = direction === 1
    ? normalizeAnglePositive(endAngle - startAngle)
    : normalizeAnglePositive(startAngle - endAngle);
  const targetTravel = direction === 1
    ? normalizeAnglePositive(targetAngle - startAngle)
    : normalizeAnglePositive(startAngle - targetAngle);
  if (travelled >= tau - safeTolerance) return true;
  return targetTravel <= travelled + safeTolerance || targetTravel >= tau - safeTolerance;
}

export function orbitalElementPosition(
  centerX: number,
  centerY: number,
  radius: number,
  rotation: number,
  ring: OrbitalRing,
  index: number,
  count: number,
): { x: number; y: number } {
  const angle = orbitalElementAngle(rotation, ring, index, count);
  return {
    x: centerX + Math.cos(angle) * radius,
    y: centerY + Math.sin(angle) * radius,
  };
}
