import test from 'node:test';
import assert from 'node:assert/strict';
import { generateUpgradeChoices, getSphereUpgradeChoiceWeight } from '../src/engine.ts';

const makeState = ({
  levels = { standard: 1, sniper: 1, chain: 1 },
  spheres = [{ type: 'standard', alive: true }],
  mods = {},
} = {}) => ({
  player: {
    sphereProgression: { standard: 0, sniper: 0, chain: 0, shotgun: 0, aura: 0, ...levels },
    sphereMods: {
      multishot: 0,
      pierce: 0,
      ricochet: 0,
      fire: 0,
      freeze: 0,
      poison: 0,
      ...mods,
    },
  },
  spheres,
});

test('sphere upgrade weighting favours active and less-developed spheres', () => {
  const state = makeState({
    levels: { standard: 1, sniper: 3, chain: 3 },
    spheres: [{ type: 'standard', alive: true }],
  });

  const standardWeight = getSphereUpgradeChoiceWeight(state, 'standard');
  const sniperWeight = getSphereUpgradeChoiceWeight(state, 'sniper');

  assert.ok(standardWeight > sniperWeight);
  assert.equal(standardWeight, 4);
  assert.equal(sniperWeight, 2);
});

test('level-up keeps three choices when only one Sphere upgrade remains', () => {
  const state = makeState({
    levels: { standard: 1, sniper: 7, chain: 7 },
  });

  for (let i = 0; i < 30; i++) {
    const choices = generateUpgradeChoices(state);
    assert.equal(choices.length, 3);
    assert.equal(new Set(choices.map((choice) => choice.modifier || `sphere:${choice.sphereType}`)).size, 3);
    assert.equal(choices.filter((choice) => choice.type === 'sphere').length, 1);
    assert.equal(choices.filter((choice) => choice.type === 'modifier').length, 2);
  }
});

test('level-up choices do not contain duplicate entries', () => {
  const state = makeState();

  for (let i = 0; i < 100; i++) {
    const choices = generateUpgradeChoices(state);
    const keys = choices.map((choice) => choice.modifier || `sphere:${choice.sphereType}`);
    assert.equal(new Set(keys).size, keys.length);
  }
});
