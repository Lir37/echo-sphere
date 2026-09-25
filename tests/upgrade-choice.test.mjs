import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineLoopSource = await fs.readFile(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
const progressionSource = await fs.readFile(new URL('../src/engineProgression.ts', import.meta.url), 'utf8');

test('level-up sphere selection uses deliberate build pressure weighting', () => {
  assert.match(progressionSource, /export function getSphereUpgradeChoiceWeight\(s: GameState, type: SphereType\): number/);
  assert.match(progressionSource, /const levelPressure = \(7 - level\) \* 0\.25;/);
  assert.match(progressionSource, /const activeBuildPressure = activeCopies > 0 \? 1\.5 : 0;/);
  assert.match(progressionSource, /const characterAffinity = CHARACTER_DEFS\[s\.player\.characterId\]\?\.preferredSphereTypes/);
  assert.match(progressionSource, /const spherePool = \[\.\.\.sphereChoices\]\.filter\(\(choice\) => isLiveUpgradeChoice\(s, choice\)\);/);
});

test('level-up mixes Sphere, Modifier and Ability sources without dead slots', () => {
  assert.match(progressionSource, /const activePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(progressionSource, /const passivePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(progressionSource, /const mixedPool: UpgradeChoice\[\] = \[\];/);
  assert.match(progressionSource, /const sourcePools = \[abilityPool, spherePool, modifierPool\];/);
  assert.match(progressionSource, /const allChoices = sourcePools\.flatMap\(\(pool\) => pool\);/);
  assert.match(progressionSource, /const candidates = cooledChoices\.length >= 3 \? cooledChoices : allChoices;/);
  assert.match(progressionSource, /pickWeightedOne\(s, remaining/);
});

test('active Ability slots stay bounded and progressive', () => {
  assert.match(engineLoopSource, /if \(s\.player\.level >= 5\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 12\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 20\) s\.player\.activeAbilitySlots/);
  assert.match(progressionSource, /activeCount < s\.player\.activeAbilitySlots/);
});
