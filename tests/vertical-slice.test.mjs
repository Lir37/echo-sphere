import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const storage = new Map();
globalThis.localStorage = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => { storage.set(key, String(value)); },
  removeItem: (key) => { storage.delete(key); },
  clear: () => { storage.clear(); },
};
const mobileControlsSource = await fs.readFile(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

test('expanded Sphere roster is the gameplay data source of truth', async () => {
  const gameDataSource = await fs.readFile(new URL('../src/gameData.ts', import.meta.url), 'utf8');
  const expected = ['standard', 'sniper', 'shotgun', 'chain', 'aura', 'orbital', 'prism', 'gravity', 'pulse', 'void'];
  for (const type of expected) {
    assert.match(gameDataSource, new RegExp(`['"]${type}['"]`));
  }
  assert.match(gameDataSource, /export const SPHERE_TYPES/);
});

test('mobile sphere selector uses the complete Sphere roster', () => {
  assert.match(
    mobileControlsSource,
    /Object\.keys\(SPHERE_TYPES\) as SphereType\[\]/,
  );
});

test('global Resonance is stored on PlayerState while local formation cadence stays on Spheres', async () => {
  const { createInitialState, placeSphere } = await import('../src/engine.ts');
  const state = createInitialState(
    { gold: 0, upgrades: {} },
    'Test',
    'normal',
    'parchment',
    12345,
  );

  assert.equal(state.player.resonanceCharge, 0);
  assert.equal(state.spheres[0].formationHitCount, 0);

  placeSphere(state, 300, 0);

  assert.equal(state.player.resonanceCharge, 5);
  assert.equal(state.spheres[0].formationHitCount, 0);
  assert.equal(state.spheres[1].formationHitCount, 0);
});