import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineStateSource = await fs.readFile(new URL('../src/engineState.ts', import.meta.url), 'utf8');
const engineLoopSource = await fs.readFile(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
const progressionSource = await fs.readFile(new URL('../src/engineProgression.ts', import.meta.url), 'utf8');
const engineTypesSource = await fs.readFile(new URL('../src/engineTypes.ts', import.meta.url), 'utf8');
const sourceWithTypes = `${engineTypesSource}\n${engineStateSource}`;

test('Level-Up has a seeded-source pity state in GameState', () => {
  assert.match(sourceWithTypes, /levelUpPity: \{ ability: number; sphere: number; modifier: number \};/);
  assert.match(engineStateSource, /levelUpPity: \{ ability: 0, sphere: 0, modifier: 0 \}/);
  assert.match(progressionSource, /function recordLevelUpSourcePick\(s: GameState, choice: UpgradeChoice\)/);
  assert.match(progressionSource, /Math\.min\(4, \(s\.levelUpPity\[source\] \|\| 0\) \+ 1\)/);
});

test('Level-Up source weight combines pity and underrepresented-system pressure', () => {
  assert.match(progressionSource, /export function getUpgradeSourceWeight\(s: GameState, source: UpgradeSource\): number/);
  assert.match(progressionSource, /const pityWeight = 1 \+ pity \* 0\.20;/);
  assert.match(progressionSource, /const underrepresentedWeight = scores\[source\] <= minimum \+ 0\.001 \? 1\.15 : 1;/);
  assert.match(progressionSource, /return pityWeight \* underrepresentedWeight;/);
});

test('Level-Up suppresses dead choices at the final eligibility gate', () => {
  assert.match(progressionSource, /function isLiveUpgradeChoice\(s: GameState, choice: UpgradeChoice\): boolean/);
  assert.match(progressionSource, /return sphereLevel\(s, choice\.sphereType\) < 7;/);
  assert.match(progressionSource, /return \(s\.player\.sphereMods\[choice\.modifier\] \|\| 0\) <= 0;/);
  assert.match(progressionSource, /if \(!def \|\| current >= def\.maxLevel\) return false;/);
  assert.match(progressionSource, /if \(activeCount >= s\.player\.activeAbilitySlots\) return false;/);
  assert.match(progressionSource, /\.filter\(\(choice\) => isLiveUpgradeChoice\(s, choice\)\)/);
});

test('Level-Up uses weighted choice selection without forcing one card per source', () => {
  assert.match(progressionSource, /const sourcePools = \[abilityPool, spherePool, modifierPool\];/);
  assert.match(progressionSource, /const allChoices = sourcePools\.flatMap\(\(pool\) => pool\);/);
  assert.match(progressionSource, /const cooledChoices = allChoices\.filter/);
  assert.match(progressionSource, /const candidates = cooledChoices\.length >= 3 \? cooledChoices : allChoices;/);
  assert.doesNotMatch(progressionSource, /for \(const pool of sourcePools\) \{[\s\S]*mixedPool\.push\(pool\[0\]\)/);
  assert.match(progressionSource, /pickWeightedOne\(s, remaining/);
  assert.match(progressionSource, /const diversityMultiplier = mixedPool\.length === 0/);
});

test('active Ability slots stay bounded and progressive', () => {
  assert.match(engineLoopSource, /if \(s\.player\.level >= 5\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 12\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 20\) s\.player\.activeAbilitySlots/);
});



test('Level-Up uses a short recent-choice cooldown without collapsing the live pool', () => {
  assert.match(progressionSource, /export function getUpgradeChoiceKey\(choice: UpgradeChoice\): string/);
  assert.match(progressionSource, /const recentKeys = new Set\(s\.recentUpgradeKeys \|\| \[\]\);/);
  assert.match(progressionSource, /const cooledChoices = allChoices\.filter\(\(choice\) => !recentKeys\.has\(getUpgradeChoiceKey\(choice\)\)\);/);
  assert.match(progressionSource, /const candidates = cooledChoices\.length >= 3 \? cooledChoices : allChoices;/);
  assert.match(progressionSource, /s\.recentUpgradeKeys = \[/);
});

test('Level-Up selection applies actual per-choice weights and soft source diversity', () => {
  assert.match(progressionSource, /function pickWeightedOne<T>\(s: GameState, items: T\[\], getWeight: \(item: T\) => number\)/);
  assert.match(progressionSource, /const baseWeight = choice\.type === 'ability'/);
  assert.match(progressionSource, /getSphereUpgradeChoiceWeight\(s, choice\.sphereType\)/);
  assert.match(progressionSource, /getModifierUpgradeChoiceWeight\(s, choice\.modifier\)/);
  assert.match(progressionSource, /const diversityMultiplier = mixedPool\.length === 0/);
});
