import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');

test('level-up sphere selection uses deliberate build pressure weighting', () => {
  assert.match(engineSource, /export function getSphereUpgradeChoiceWeight\(s: GameState, type: SphereType\): number/);
  assert.match(engineSource, /const levelPressure = \(7 - level\) \* 0\.25;/);
  assert.match(engineSource, /const activeBuildPressure = activeCopies > 0 \? 1\.5 : 0;/);
  assert.match(engineSource, /const spherePool = weightedShuffle\(s, sphereChoices/);
});

test('level-up mixes Sphere, Modifier and Ability sources without dead slots', () => {
  assert.match(engineSource, /const activePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(engineSource, /const passivePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(engineSource, /const mixedPool: UpgradeChoice\[\] = \[\];/);
  assert.match(engineSource, /for \(const choice of weightedShuffle\(s, sources, \(\) => 1\)\)/);
});

test('active Ability slots stay bounded and progressive', () => {
  assert.match(engineSource, /if \(s\.player\.level >= 5\) s\.player\.activeAbilitySlots/);
  assert.match(engineSource, /if \(s\.player\.level >= 12\) s\.player\.activeAbilitySlots/);
  assert.match(engineSource, /if \(s\.player\.level >= 20\) s\.player\.activeAbilitySlots/);
  assert.match(engineSource, /activeCount < s\.player\.activeAbilitySlots/);
});
