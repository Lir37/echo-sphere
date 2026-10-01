import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import { addResonanceCharge } from '../src/resonance.ts';

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

test('global Resonance uses a player-level resource separate from local formation cadence', () => {
  const player = { resonanceCharge: 0 };
  const sphere = { formationHitCount: 0 };

  addResonanceCharge(player, 5);
  sphere.formationHitCount += 1;

  assert.equal(player.resonanceCharge, 5);
  assert.equal(sphere.formationHitCount, 1);
});

test('Build Diagnostics is descriptive and does not expose a ranking score', async () => {
  const statsSource = await fs.readFile(new URL('../src/engineStats.ts', import.meta.url), 'utf8');
  const appSource = await fs.readFile(new URL('../src/App.tsx', import.meta.url), 'utf8');
  assert.match(statsSource, /export function getBuildDiagnostics\(s: GameState\): BuildDiagnosticRow\[\]/);
  assert.match(statsSource, /id: 'offense'/);
  assert.match(statsSource, /id: 'control'/);
  assert.match(statsSource, /id: 'survival'/);
  assert.match(statsSource, /id: 'network'/);
  assert.match(statsSource, /id: 'resonance'/);
  assert.match(statsSource, /id: 'synergy'/);
  assert.doesNotMatch(statsSource, /score|rank|rating/i);
  assert.match(appSource, /getBuildDiagnostics\(st\)/);
});
