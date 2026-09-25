import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');
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

test('global Resonance is not backed by per-Sphere hit counters', () => {
  assert.doesNotMatch(engineSource, /resonanceHits/);
  assert.match(engineSource, /player\.resonanceCharge/);
  assert.match(engineSource, /formationHitCount/);
});