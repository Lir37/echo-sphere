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
  assert.equal(ARTIFACT_SETS.length, 12);
  for (const set of ARTIFACT_SETS) {
    assert.ok(set.synergyIds.length >= 2);
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
    ARTIFACT_SETS.map((set) => set.id),
  );
});


test('all current Artifact Sets expose protocol discovery and completion bonus', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const all = Object.values(ARTIFACT_META).map((item) => item.id);
  const states = mod.getArtifactProtocolStates({
    player: { artifacts: all, combo: 10, resonanceGeometryKey: 'ring', timestopTimer: 1, teleportDamageBuffTimer: 1, sphereMods: { fire: 1 } },
    enemies: [
      { type: 'x', hp: 100, isElite: true, isBoss: false },
      { type: 'x', hp: 100, isElite: false, isBoss: false },
      { type: 'x', hp: 100, isElite: false, isBoss: false },
      { type: 'x', hp: 100, isElite: false, isBoss: false },
    ],
    spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 10, y: 0 } },
      { type: 'prism', alive: true, pos: { x: 0, y: 10 } },
      { type: 'pulse', alive: true, pos: { x: 20, y: 20 } },
      { type: 'void', alive: true, pos: { x: -20, y: -20 } },
      { type: 'orbital', alive: true, pos: { x: 40, y: -20 } },
    ]
  });
  assert.equal(states.length, 12);
  assert.ok(states.every((x) => x.discovered));
  assert.ok(states.every((x) => x.active));
  assert.equal(mod.getArtifactSetCompletionPulse({ player: { artifacts: all } }), 1);
});

test('Artifact Set completion is wired into behavioral combat effects', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const all = Object.values(ARTIFACT_META).map((item) => item.id);
  const baseState = {
    player: { artifacts: all, combo: 10, resonanceGeometryKey: 'ring', timestopTimer: 1, teleportDamageBuffTimer: 1, sphereMods: { fire: 1 } },
    enemies: [
      { hp: 100, isElite: true, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
    ],
    spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
      { type: 'chain', alive: true, pos: { x: 0, y: 100 } },
    ],
  };

  const behaviors = mod.getArtifactSetBehavior(baseState);
  assert.deepEqual(behaviors, {
    resonanceGrid: true,
    echoArchitecture: true,
    singularityPath: true,
    geometryCraft: true,
    voidHorizon: true,
    temporalFold: true,
    statusCircuit: true,
    hunterDoctrine: true,
    pulseEngineering: true,
    coreForge: true,
    prismDominion: true,
    networkLegacy: true,
  });

  const modifiers = mod.getSphereArtifactModifiers(baseState, 'standard', baseState.spheres[0]);
  assert.ok(modifiers.damage > 1, 'completed Sets must affect behavioral Sphere modifiers');

  const completionPulse = mod.getArtifactSetCompletionPulse(baseState);
  assert.equal(completionPulse, 1);
});


test('expanded Artifact catalog has long-run buildcraft scale and complete metadata', async () => {
  const mod = await import('../src/artifactSystem.ts');
  assert.ok(Object.keys(ARTIFACT_META).length >= 82);
  assert.ok(Object.values(ARTIFACT_META).some((x) => x.rarity === 'legendary'));
  assert.ok(mod.ARTIFACT_SYNERGIES.some((x) => x.id === 'geometry_loop'));
});


test('removed artifact concepts are absent from the live catalog', () => {
  for (const id of ['dash_relay','overdrive_matrix','singularity_seed','echo_archive','zero_point_relay','infinite_loop']) {
    assert.equal(ARTIFACT_META[id], undefined);
  }
});

test('artifact descriptions match concrete runtime effects', () => {
  assert.equal(ARTIFACT_META.formation_compass.effects.sphereRadius, 0.08);
  assert.equal(ARTIFACT_META.crit_sigil.effects.critChance, 0.05);
  assert.equal(ARTIFACT_META.tempo_ring.effects.sphereDelay, -0.08);
});
