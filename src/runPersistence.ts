import type { GameState } from './engineTypes';

const RUN_SNAPSHOT_KEY = 'echosphere_active_run_v1';
const RUN_SNAPSHOT_VERSION = 1;

interface RunSnapshotEnvelope {
  version: number;
  savedAt: number;
  state: GameState;
}

const DROP_KEYS = new Set([
  'hitEnemies',
  'sourceSphere',
  'hunterMarkTarget',
  'hunterHuntTarget',
  'engineerRelaySource',
  'voidPhantomSource',
]);

function replacer(key: string, value: unknown): unknown {
  return DROP_KEYS.has(key) ? undefined : value;
}

export function serializeRunSnapshot(state: GameState): string {
  const envelope: RunSnapshotEnvelope = {
    version: RUN_SNAPSHOT_VERSION,
    savedAt: Date.now(),
    state,
  };
  return JSON.stringify(envelope, replacer);
}

export function restoreRunSnapshot(raw: string): GameState | null {
  try {
    const envelope = JSON.parse(raw) as Partial<RunSnapshotEnvelope>;
    if (!envelope || envelope.version !== RUN_SNAPSHOT_VERSION || !envelope.state) return null;

    const state = envelope.state as GameState;
    if (
      !state.player ||
      !Array.isArray(state.spheres) ||
      !Array.isArray(state.enemies) ||
      !Array.isArray(state.sphereProjectiles) ||
      !Array.isArray(state.lightnings) ||
      typeof state.time !== 'number' ||
      typeof state.region !== 'object'
    ) return null;

    // Resume always starts paused. Runtime object references are intentionally
    // reconstructed/reset because JSON cannot preserve entity identity.
    state.paused = true;
    state.keys = {};
    state.mouse = { x: 0, y: 0, down: false };

    state.player.hunterMarkTarget = null;
    state.player.hunterMarkTimer = 0;
    state.player.hunterHitCount = 0;
    state.player.hunterHuntTarget = null;
    state.player.hunterHuntTimer = 0;
    state.player.engineerRelaySource = null;
    state.player.engineerRelayTimer = 0;
    state.player.voidPhantomSource = null;

    for (const projectile of state.sphereProjectiles) {
      projectile.hitEnemies = new Set();
    }

    if (state.gameOver) return null;
    return state;
  } catch {
    return null;
  }
}

export function saveRunSnapshot(state: GameState): boolean {
  if (typeof localStorage === 'undefined' || state.gameOver || state.tutorialMode) return false;
  try {
    localStorage.setItem(RUN_SNAPSHOT_KEY, serializeRunSnapshot(state));
    return true;
  } catch {
    return false;
  }
}

export function loadSavedRun(): GameState | null {
  if (typeof localStorage === 'undefined') return null;
  const raw = localStorage.getItem(RUN_SNAPSHOT_KEY);
  if (!raw) return null;
  const state = restoreRunSnapshot(raw);
  if (!state) {
    localStorage.removeItem(RUN_SNAPSHOT_KEY);
    return null;
  }
  return state;
}

export function hasSavedRun(): boolean {
  return loadSavedRun() !== null;
}

export function clearSavedRun(): void {
  if (typeof localStorage !== 'undefined') localStorage.removeItem(RUN_SNAPSHOT_KEY);
}

export function getSavedRunTime(): number | null {
  const state = loadSavedRun();
  return state ? Math.max(0, Math.floor(state.time)) : null;
}
