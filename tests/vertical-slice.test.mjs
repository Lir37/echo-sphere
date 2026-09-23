import test from 'node:test';
import assert from 'node:assert/strict';
import { generateUpgradeChoices, VERTICAL_SLICE_SPHERE_TYPES } from '../src/engine.ts';

const baseState = (mods = {}) => ({
  player: {
    sphereProgression: { standard: 0, sniper: 0, shotgun: 0, chain: 0, aura: 0 },
    sphereBranches: {},
    sphereMods: {
      multishot: 0, pierce: 0, ricochet: 0, fire: 0, freeze: 0, poison: 0,
      ...mods,
    },
  },
});

test('first vertical slice exposes only Standard, Sniper and Chain sphere upgrades', () => {
  const state = baseState();
  for (let i = 0; i < 30; i++) {
    const sphereChoices = generateUpgradeChoices(state)
      .filter((choice) => choice.type === 'sphere')
      .map((choice) => choice.sphereType);

    assert.ok(
      sphereChoices.every((type) => VERTICAL_SLICE_SPHERE_TYPES.includes(type)),
      'unexpected sphere type in first-slice pool: ' + sphereChoices.join(', '),
    );
  }
});

test('when modifiers are already known, the first-slice pool contains all three core sphere types', () => {
  const state = baseState({
    multishot: 1, pierce: 1, ricochet: 1, fire: 1, freeze: 1, poison: 1,
  });

  const choices = generateUpgradeChoices(state);
  const sphereTypes = choices
    .filter((choice) => choice.type === 'sphere')
    .map((choice) => choice.sphereType);

  assert.equal(sphereTypes.length, 3);
  assert.deepEqual(new Set(sphereTypes), new Set(VERTICAL_SLICE_SPHERE_TYPES));
});
