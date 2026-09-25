import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');
const progressionSource = await fs.readFile(new URL('../src/engineProgression.ts', import.meta.url), 'utf8');
const engineTypesSource = await fs.readFile(new URL('../src/engineTypes.ts', import.meta.url), 'utf8');
const sourceWithTypes = `${engineTypesSource}\n${engineSource}`;

test('Level-Up has a seeded-source pity state in GameState', () => {
  assert.match(sourceWithTypes, /levelUpPity: \{ ability: number; sphere: number; modifier: number \};/);
  assert.match(engineSource, /levelUpPity: \{ ability: 0, sphere: 0, modifier: 0 \}/);
  assert.match(progressionSource, /function recordLevelUpSourcePick\\(s: GameState, choice: UpgradeChoice\\)/);
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

test('Level-Up retains source diversity and seeded weighted ordering', () => {
  assert.match(progressionSource, /const sourcePools = \[abilityPool, spherePool, modifierPool\];/);
  assert.match(progressionSource, /if \(pool\[0\] && mixedPool\.length < 3\) mixedPool\.push\(pool\[0\]\);/);
  assert.match(progressionSource, /for \(const choice of weightedShuffle\(s, candidates, \(\) => 1\)\)/);
  assert.match(progressionSource, /const seen = new Set\(mixedPool\.map/);
});

test('active Ability slots stay bounded and progressive', () => {
  assert.match(engineSource, /if \(s\.player\.level >= 5\) s\.player\.activeAbilitySlots/);
  assert.match(engineSource, /if \(s\.player\.level >= 12\) s\.player\.activeAbilitySlots/);
  assert.match(engineSource, /if \(s\.player\.level >= 20\) s\.player\.activeAbilitySlots/);
});
