export const RESONANCE_BASE_CAP = 100;
export const RESONANCE_OVERFLOW_CAP = 150;

// Authoritative run-level charge amounts. Local formation counters are separate.
export const RESONANCE_CHARGE = {
  sphereHit: 1,
  geometry: 5,
  network: 5,
  rune: 60,
} as const;

export type ResonanceSource = keyof typeof RESONANCE_CHARGE;

export interface ResonanceState {
  resonanceCharge: number;
}

function finiteOrZero(value: number): number {
  return Number.isFinite(value) ? value : 0;
}

export function clampResonanceCharge(value: number, allowOverflow = false): number {
  const cap = allowOverflow ? RESONANCE_OVERFLOW_CAP : RESONANCE_BASE_CAP;
  return Math.max(0, Math.min(cap, finiteOrZero(value)));
}

/**
 * Adds run-level Resonance and returns the number of formation Events crossed.
 *
 * Baseline contract:
 * - normal charge is always 0..100;
 * - every crossing of 100 emits one Event;
 * - the remainder survives the Event;
 * - overflow above 100 is never produced by ordinary gameplay.
 *
 * The optional overflow flag is reserved for explicit mechanics that
 * intentionally operate with the 150 special cap. It does not disable the
 * 100-charge Event threshold.
 */
export function addResonanceCharge(state: ResonanceState, amount: number, allowOverflow = false): number {
  if (!Number.isFinite(amount) || amount <= 0) return 0;

  const before = clampResonanceCharge(state.resonanceCharge, allowOverflow);
  const rawCharge = before + amount;
  const events = Math.floor(rawCharge / RESONANCE_BASE_CAP);
  const remainder = rawCharge - events * RESONANCE_BASE_CAP;

  state.resonanceCharge = clampResonanceCharge(remainder, allowOverflow);
  return events;
}

/**
 * Source-routed helper for the authoritative Resonance resource.
 * Keeping source selection here prevents gameplay call sites from inventing
 * one-off baseline amounts and makes future source modifiers explicit.
 */
export function addResonanceChargeFromSource(
  state: ResonanceState,
  source: ResonanceSource,
  multiplier = 1,
  allowOverflow = false,
): number {
  if (!Number.isFinite(multiplier) || multiplier <= 0) return 0;
  return addResonanceCharge(
    state,
    RESONANCE_CHARGE[source] * multiplier,
    allowOverflow,
  );
}

export function setResonanceCharge(state: ResonanceState, value: number, allowOverflow = false): void {
  state.resonanceCharge = clampResonanceCharge(value, allowOverflow);
}
