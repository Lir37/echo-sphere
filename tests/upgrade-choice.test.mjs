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

test('level-up mixes only Sphere and Ability sources without dead slots', () => {
  assert.match(progressionSource, /const activePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(progressionSource, /const passivePool = \(Object\.keys\(ABILITIES\) as AbilityType\[\]\)/);
  assert.match(progressionSource, /const mixedPool: UpgradeChoice\[\] = \[\];/);
  assert.match(progressionSource, /const sourcePools = \[abilityPool, spherePool\];/);
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


test('routine Level-Up exposes one limited reroll without mutation rerolling', () => {
  assert.match(progressionSource, /export function rerollUpgradeChoices\(s: GameState\): boolean/);
  assert.match(progressionSource, /s\.levelUpRerollsRemaining <= 0/);
  assert.match(progressionSource, /s\.levelUpRerollsRemaining--/);
  assert.match(progressionSource, /choice\.sphereStage === 'upgrade'/);
  assert.match(progressionSource, /choice\.abilityStage\)/);
});


test('Level-Up Lock persists until the locked choice is selected', () => {
  assert.match(progressionSource, /const lockedKey = s\.levelUpLockChoiceKey;/);
  assert.match(progressionSource, /const lockedChoice = lockedKey/);
  assert.match(progressionSource, /const mixedPool: UpgradeChoice\[\] = lockedChoice \? \[lockedChoice\] : \[\];/);
  assert.match(progressionSource, /const selectedChoiceKey = getUpgradeChoiceKey\(choice\);/);
  assert.match(progressionSource, /if \(s\.levelUpLockChoiceKey === selectedChoiceKey\)/);
  assert.match(progressionSource, /const lockedChoice = lockedKey/);
  assert.match(progressionSource, /s\.levelUpRerollsRemaining--;/);
  assert.doesNotMatch(progressionSource, /s\.pendingUpgrade = next;\n  s\.levelUpLockChoiceKey = null;/);
});
