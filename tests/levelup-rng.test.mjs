import { generateUpgradeChoices, getUpgradeChoiceKey } from '../src/engineProgression.ts';
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

test('Level-Up retains source diversity and seeded weighted ordering', () => {
  assert.match(progressionSource, /const sourcePools = \[abilityPool, spherePool, modifierPool\];/);
  assert.match(progressionSource, /if \(pool\[0\] && mixedPool\.length < 3\) mixedPool\.push\(pool\[0\]\);/);
  assert.match(progressionSource, /for \(const choice of weightedShuffle\(s, candidates, \(\) => 1\)\)/);
  assert.match(progressionSource, /const seen = new Set\(mixedPool\.map/);
});

test('active Ability slots stay bounded and progressive', () => {
  assert.match(engineLoopSource, /if \(s\.player\.level >= 5\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 12\) s\.player\.activeAbilitySlots/);
  assert.match(engineLoopSource, /if \(s\.player\.level >= 20\) s\.player\.activeAbilitySlots/);
});


function makeLevelUpFixture() {
  return {
    spheres: [{ alive: true, type: 'standard' }],
    levelUpPity: { ability: 0, sphere: 0, modifier: 0 },
    recentUpgradeKeys: [],
    player: {
      abilities: {},
      sphereProgression: {
        standard: 1, sniper: 1, shotgun: 1, chain: 1, aura: 1,
        orbital: 1, prism: 1, gravity: 1, pulse: 1, void: 1,
      },
      sphereBranches: {},
      sphereMods: { multishot: 0, pierce: 0, ricochet: 0, fire: 0, freeze: 0, poison: 0 },
      activeAbilitySlots: 0,
      activeKeyMap: {},
      characterId: 'spherist',
    },
  };
}

test('Level-Up suppresses a recently selected logical card when alternatives exist', () => {
  const state = makeLevelUpFixture();
  const first = generateUpgradeChoices(state);
  assert.equal(first.length, 3);

  const blockedKey = getUpgradeChoiceKey(first[0]);
  state.recentUpgradeKeys = [blockedKey];

  const second = generateUpgradeChoices(state);
  assert.equal(second.length, 3);
  assert.ok(!second.some((choice) => getUpgradeChoiceKey(choice) === blockedKey));
});

test('Level-Up keeps three cards when the recent suppression pool becomes too small', () => {
  const state = makeLevelUpFixture();
  const first = generateUpgradeChoices(state);
  state.recentUpgradeKeys = first.map(getUpgradeChoiceKey);

  const second = generateUpgradeChoices(state);
  assert.equal(second.length, 3);
});
