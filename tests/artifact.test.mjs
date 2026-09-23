import test from 'node:test';
import assert from 'node:assert/strict';
import { pickArtifactChoices, pickStellaArtifactChoice, ARTIFACT_META } from '../src/artifactSystem.ts';

const state = (artifacts = []) => ({ player: { artifacts } });

test('ordinary artifact rolls exclude Legendary rewards', () => {
  for (let i = 0; i < 40; i++) {
    const picks = pickArtifactChoices(state(), 3);
    assert.ok(picks.every((id) => ARTIFACT_META[id].rarity !== 'legendary'));
  }
});

test('Stella returns an unowned Legendary artifact when one is available', () => {
  const pick = pickStellaArtifactChoice(state());
  assert.ok(pick);
  assert.equal(ARTIFACT_META[pick].rarity, 'legendary');
});

test('Stella has no Legendary reward after all current Legendary artifacts are owned', () => {
  const legendaryIds = Object.values(ARTIFACT_META)
    .filter((item) => item.rarity === 'legendary')
    .map((item) => item.id);
  assert.equal(pickStellaArtifactChoice(state(legendaryIds)), null);
});
