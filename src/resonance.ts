export const RESONANCE_BASE_CAP = 100;
export const RESONANCE_OVERFLOW_CAP = 150;

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
  const cap = allowOverflow ? RESONANCE_OVERFLOW_CAP : RESONANCE_BASE_CAP;
  let charge = Math.min(cap, before + amount);
  let events = 0;
  while (charge >= RESONANCE_BASE_CAP) {
    charge -= RESONANCE_BASE_CAP;
    events++;
    if (!allowOverflow) break;
  }
  state.resonanceCharge = clampResonanceCharge(charge, allowOverflow);
  return events;
}

export function setResonanceCharge(state: ResonanceState, value: number, allowOverflow = false): void {
  state.resonanceCharge = clampResonanceCharge(value, allowOverflow);
}
