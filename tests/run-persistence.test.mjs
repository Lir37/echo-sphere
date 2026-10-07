import assert from 'node:assert/strict';
import fs from 'node:fs';

const { createInitialState } = await import('../src/engineState.ts');
const {
  serializeRunSnapshot,
  restoreRunSnapshot,
} = await import('../src/runPersistence.ts');

const state = createInitialState({ gold: 0, upgrades: {} }, 'Tester', 'normal', 1234);
state.time = 137.8;
state.stats.time = state.time;
state.paused = false;
state.keys = { ArrowUp: true };
state.mouse = { x: 4, y: 5, down: true };
state.sphereProjectiles.push({
  pos: { x: 1, y: 2 },
  vel: { x: 3, y: 4 },
  damage: 5,
  radius: 2,
  alive: true,
  color: '#fff',
  pierce: 0,
  hitEnemies: new Set(),
  effect: 'none',
  ricochet: 0,
  life: 1,
});

const raw = serializeRunSnapshot(state);
assert.match(raw, /"version":1/);
const resumed = restoreRunSnapshot(raw);
assert.ok(resumed);
assert.equal(resumed.paused, true);
assert.equal(resumed.time, 137.8);
assert.deepEqual(resumed.keys, {});
assert.deepEqual(resumed.mouse, { x: 0, y: 0, down: false });
assert.ok(resumed.sphereProjectiles[0].hitEnemies instanceof Set);
assert.equal(resumed.player.hunterMarkTarget, null);
assert.equal(resumed.player.hunterHuntTarget, null);
assert.equal(resumed.player.engineerRelaySource, null);
assert.equal(resumed.player.engineerRelayTimer, 0);
assert.equal(resumed.player.voidPhantomSource, null);

const app = fs.readFileSync(new URL('../src/App.tsx', import.meta.url), 'utf8');
assert.match(app, /document\.addEventListener\('visibilitychange'/);
assert.match(app, /window\.addEventListener\('blur'/);
assert.match(app, /window\.addEventListener\('pagehide'/);
assert.match(app, /current\.paused = true/);
assert.match(app, /saveRunSnapshot\(current\)/);
assert.match(app, /runSaveAccumulatorRef/);
assert.match(app, /ПРОДОЛЖИТЬ ЗАБЕГ/);
assert.match(app, /initialState \|\| createInitialState/);

const persistence = fs.readFileSync(new URL('../src/runPersistence.ts', import.meta.url), 'utf8');
assert.match(persistence, /localStorage\.setItem\(RUN_SNAPSHOT_KEY/);
assert.match(persistence, /localStorage\.removeItem\(RUN_SNAPSHOT_KEY/);
assert.match(persistence, /state\.paused = true/);

const resetPersistence = fs.readFileSync(new URL('../src/persistence.ts', import.meta.url), 'utf8');
assert.match(resetPersistence, /clearSavedRun\(\)/);

console.log('run persistence: OK');
