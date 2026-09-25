export interface RandomState {
  rngState: number;
}

export function createRngState(seed: number): number {
  return (seed >>> 0) || 1;
}

export function createRunSeed(playerName: string, difficulty: string, mapTheme: string): number {
  const entropy = Date.now();
  const input = playerName + '|' + difficulty + '|' + mapTheme + '|' + entropy;
  let hash = 2166136261;
  for (let i = 0; i < input.length; i++) {
    hash ^= input.charCodeAt(i);
    hash = Math.imul(hash, 16777619);
  }
  return (hash >>> 0) || 1;
}

export function nextRandom(state: RandomState): number {
  state.rngState = (state.rngState + 0x6D2B79F5) | 0;
  let t = state.rngState;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}

export function randomInt(state: RandomState, maxExclusive: number): number {
  if (maxExclusive <= 0) return 0;
  return Math.floor(nextRandom(state) * maxExclusive);
}
