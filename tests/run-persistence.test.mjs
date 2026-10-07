import assert from 'node:assert/strict';
import fs from 'node:fs';

class MemoryStorage {
  store = new Map();
  getItem(key){ return this.store.has(key) ? this.store.get(key) : null; }
  setItem(key,value){ this.store.set(key,String(value)); }
  removeItem(key){ this.store.delete(key); }
  clear(){ this.store.clear(); }
}

Object.defineProperty(globalThis, 'localStorage', { value: new MemoryStorage(), configurable: true, writable: true });

const { createInitialState } = await import('../src/engineState.ts');
const {
  saveRunSnapshot,
  loadSavedRun,
  clearSavedRun,
  getSavedRunTime,
  serializeRunSnapshot,
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
  hitEnemies: new Set(state.enemies),
  effect: 'none',
  ricochet: 0,
  life: 1,
});

assert.equal(saveRunSnapshot(state), true);
const resumed = loadSavedRun();
assert.ok(resumed);
assert.equal(resumed.paused, true);
assert.equal(resumed.time, 137.8);
assert.deepEqual(resumed.keys, {});
assert.deepEqual(resumed.mouse, { x: 0, y: 0, down: false });
assert.ok(resumed.sphereProjectiles[0].hitEnemies instanceof Set);
assert.equal(getSavedRunTime(), 137);
assert.match(serializeRunSnapshot(state), /"version":1/);

clearSavedRun();
assert.equal(loadSavedRun(), null);

const app = fs.readFileSync(new URL('../src/App.tsx', import.meta.url), 'utf8');
assert.match(app, /document\.addEventListener\('visibilitychange'/);
assert.match(app, /window\.addEventListener\('blur'/);
assert.match(app, /window\.addEventListener\('pagehide'/);
assert.match(app, /current\.paused = true/);
assert.match(app, /saveRunSnapshot\(current\)/);
assert.match(app, /runSaveAccumulatorRef/);
assert.match(app, /ПРОДОЛЖИТЬ ЗАБЕГ/);

const persistence = fs.readFileSync(new URL('../src/persistence.ts', import.meta.url), 'utf8');
assert.match(persistence, /clearSavedRun\(\)/);

console.log('run persistence: OK');
