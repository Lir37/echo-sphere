import test from 'node:test';
import assert from 'node:assert/strict';

import {
  generateUpgradeChoices,
  getUpgradeSourceWeight,
  applyUpgrade,
} from '../src/engine.ts';

function makeState(overrides = {}) {
  const sphereProgression = {
    standard: 0,
    sniper: 0,
    chain: 0,
    shotgun: 0,
    aura: 0,
    orbital: 0,
    prism: 0,
    gravity: 0,
    pulse: 0,
    void: 0,
  };

  return {
    player: {
      characterId: 'spherist',
      abilities: {},
      activeAbilitySlots: 0,
      sphereProgression,
      sphereBranches: {},
      sphereMods: {
        multishot: 0,
        pierce: 0,
        ricochet: 0,
        fire: 0,
        freeze: 0,
        poison: 0,
      },
    },
    spheres: [{ alive: true, type: 'standard' }],
    activeKeyMap: {},
    pendingUpgrade: null,
    flashText: null,
    levelUpPity: { ability: 0, sphere: 0, modifier: 0 },
    shopUpgrades: {},
    ...overrides,
  };
}

test('Level-Up source weight grows with pity', () => {
  const state = makeState();
  const fresh = getUpgradeSourceWeight(state, 'ability');
  state.levelUpPity.ability = 4;
  const pitied = getUpgradeSourceWeight(state, 'ability');
  assert.ok(pitied > fresh);
});

test('Level-Up source pressure favors an underrepresented system', () => {
  const state = makeState({
    player: {
      ...makeState().player,
      abilities: { blast: 7, shield: 7, teleport: 7 },
      sphereProgression: {
        standard: 7,
        sniper: 7,
        chain: 7,
        shotgun: 7,
        aura: 7,
        orbital: 0,
        prism: 0,
        gravity: 0,
        pulse: 0,
        void: 0,
      },
      sphereMods: {
        multishot: 1,
        pierce: 1,
        ricochet: 1,
        fire: 1,
        freeze: 1,
        poison: 1,
      },
    },
    spheres: [{ alive: true, type: 'standard' }],
  });

  assert.ok(getUpgradeSourceWeight(state, 'ability') < getUpgradeSourceWeight(state, 'sphere'));
  assert.equal(
    getUpgradeSourceWeight(state, 'modifier'),
    getUpgradeSourceWeight(state, 'ability'),
  );
});

test('unavailable active abilities are suppressed until an active slot opens', () => {
  const state = makeState();
  const choices = generateUpgradeChoices(state);
  assert.ok(choices.length > 0);
  assert.equal(
    choices.some((choice) => choice.type === 'ability' && ['blast', 'shield', 'teleport', 'firetrail', 'minion', 'lightning', 'timestop', 'darkritual'].includes(choice.ability || '')),
    false,
  );
});

test('Level-Up keeps source diversity when three live sources exist', () => {
  const state = makeState();
  state.player.activeAbilitySlots = 1;
  state.player.abilities = { blast: 1 };
  state.player.sphereProgression.standard = 1;

  const choices = generateUpgradeChoices(state);
  const sources = new Set(choices.map((choice) => choice.type));

  assert.equal(choices.length, 3);
  assert.ok(sources.has('ability'));
  assert.ok(sources.has('sphere'));
  assert.ok(sources.has('modifier'));
});

test('choosing a Level-Up source resets its pity and increments the others', () => {
  const state = makeState();
  const choice = {
    type: 'modifier',
    modifier: 'multishot',
    currentLevel: 0,
    newLevel: 1,
  };
  state.pendingUpgrade = [choice];
  state.levelUpPity = { ability: 2, sphere: 3, modifier: 4 };

  applyUpgrade(state, choice);

  assert.deepEqual(state.levelUpPity, { ability: 3, sphere: 4, modifier: 0 });
  assert.equal(state.player.sphereMods.multishot, 1);
});
