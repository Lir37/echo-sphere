import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');

test('level-up sphere selection uses deliberate build pressure weighting', () => {
  assert.match(engineSource, /export function getSphereUpgradeChoiceWeight\(s: GameState, type: SphereType\): number/);
  assert.match(engineSource, /const levelPressure = \(7 - level\) \* 0\.25;/);
  assert.match(engineSource, /const activeBuildPressure = activeCopies > 0 \? 1\.5 : 0;/);
  assert.match(engineSource, /const spherePool = weightedShuffle\(sphereChoices/);
});

test('level-up fills the full choice set when only one Sphere upgrade remains', () => {
  assert.match(engineSource, /const mixedChoices = \[modifierPool\[0\], \.\.\.spherePool\.slice\(0, 2\)\];/);
  assert.match(engineSource, /for \(const modifier of modifierPool\.slice\(1\)\)/);
  assert.match(engineSource, /if \(mixedChoices\.length >= 3\) break;/);
});

test('level-up choice pool remains duplicate-free by construction', () => {
  assert.match(engineSource, /const modifierPool = weightedShuffle\(modifierChoices, \(\) => 1\);/);
  assert.match(engineSource, /return weightedShuffle\(mixedChoices, \(\) => 1\)\.slice\(0, 3\);/);
});
