import test from 'node:test';
import assert from 'node:assert/strict';
import { createInitialState } from '../src/engineState.ts';
import { updateSpheres } from '../src/engineSpheres.ts';

const storage = new Map();
globalThis.localStorage = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => storage.set(key, String(value)),
  removeItem: (key) => storage.delete(key),
  clear: () => storage.clear(),
  key: (index) => [...storage.keys()][index] ?? null,
  get length() { return storage.size; },
};

function makeState() {
  const state = createInitialState({ gold: 0, upgrades: {} }, 'orbital-audit', 'normal', 12345);
  state.player.characterId = 'spherist';
  state.player.sphereProgression.orbital = 4;
  state.player.sphereBranches.orbital = 'orbital_blade';
  state.player.evolutions.push('sphere:orbital:4:orbital_blade');
  state.spheres = [{
    pos: { x: 0, y: 0 },
    radius: 130,
    damage: 12,
    attackDelay: 1.2,
    attackTimer: 0,
    rotation: 0,
    alive: true,
    networkDisabledTimer: 0,
    killsContribution: 0,
    formationHitCount: 0,
    formationHitCounts: {},
    resonancePulseTimer: 0,
    visualTier: 4,
    type: 'orbital',
    auraTimer: 0,
  }];
  state.enemies = [{
    pos: { x: 0, y: 41 },
    hp: 10000,
    maxHp: 10000,
    speed: 0,
    radius: 42,
    damage: 0,
    type: 'boss',
    color: '#ffffff',
    shape: 'circle',
    slowTimer: 0,
    slowFactor: 1,
    freezeTimer: 0,
    hitFlash: 0,
    isBoss: true,
    bossShootTimer: 0,
    bossProjectiles: [],
    xpValue: 100,
    rotation: 0,
    tier: 1,
    trailTimer: 0,
    fireTimer: 0,
    fireDps: 0,
    poisonTimer: 0,
    poisonDps: 0,
    isElite: false,
    elitePulseTimer: 0,
    bossType: 'shooter',
    chargeTimer: 0,
    isCharging: false,
    chargeDir: { x: 0, y: 0 },
    summonTimer: 0,
    auraRadius: 0,
    auraDps: 0,
  }];
  return state;
}

test('Level IV Orbital Blade damages a large boss and emits floating damage telemetry', () => {
  const state = makeState();
  const beforeHp = state.enemies[0].hp;

  updateSpheres(state, 0.25);

  assert.ok(state.enemies[0].hp < beforeHp, 'Orbital Blade should reduce boss HP when its path overlaps the boss body');
  assert.ok(
    state.damageNumbers.some((item) => item.sourceSphereType === 'orbital'),
    'Orbital contact must emit tagged floating damage telemetry',
  );
});
