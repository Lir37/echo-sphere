export const RESONANCE_BASE_CAP = 100;
export const RESONANCE_OVERFLOW_CAP = 150;

// Authoritative run-level charge amounts. Local formation counters are separate.
export const RESONANCE_CHARGE = {
  sphereHit: 1,
  geometry: 5,
  network: 5,
  rune: 60,
} as const;

export interface ResonanceState {
  resonanceCharge: number;
}

export function clampResonanceCharge(value: number, allowOverflow = false): number {
  const cap = allowOverflow ? RESONANCE_OVERFLOW_CAP : RESONANCE_BASE_CAP;
  return Math.max(0, Math.min(cap, value));
}

export function addResonanceCharge(state: ResonanceState, amount: number, allowOverflow = false): number {
  if (!Number.isFinite(amount) || amount <= 0) return 0;
  const before = clampResonanceCharge(state.resonanceCharge, allowOverflow);
  const rawCharge = before + amount;
  const events = Math.floor(rawCharge / RESONANCE_BASE_CAP);
  const remainder = rawCharge - events * RESONANCE_BASE_CAP;
  state.resonanceCharge = clampResonanceCharge(remainder, allowOverflow);
  return events;
}

export function setResonanceCharge(state: ResonanceState, value: number, allowOverflow = false): void {
  state.resonanceCharge = clampResonanceCharge(value, allowOverflow);
}
