import test from 'node:test';
import assert from 'node:assert/strict';
import {
  ARTIFACT_META,
  ARTIFACT_SETS,
  getArtifactSetProgress,
  getCompletedArtifactSets,
} from '../src/artifactSystem.ts';

const state = (artifacts = []) => ({ player: { artifacts } });

test('Artifact Sets are data-driven and reference existing pair synergies', () => {
  assert.equal(ARTIFACT_SETS.length, 3);
  for (const set of ARTIFACT_SETS) {
    assert.equal(set.synergyIds.length, 2);
    assert.ok(set.synergyIds.every((id) =>
      ['fortress_network', 'echo_relay', 'glass_cannon', 'singularity', 'perfect_network', 'unified_core'].includes(id)
    ));
  }
});

test('Artifact Set progress reports partial activation from existing artifacts', () => {
  const progress = getArtifactSetProgress(state(['heavy_core', 'resonance_core']));
  const resonance = progress.find((set) => set.id === 'resonance_grid');
  assert.ok(resonance);
  assert.equal(resonance.activeSynergies, 1);
  assert.equal(resonance.totalSynergies, 2);
  assert.equal(resonance.complete, false);
});

test('Artifact Set completion is derived from its pair synergies', () => {
  const all = Object.values(ARTIFACT_META).map((item) => item.id);
  const completed = getCompletedArtifactSets(state(all));
  assert.deepEqual(
    completed.map((set) => set.id),
    ['resonance_grid', 'echo_architecture', 'singularity_path'],
  );
});
