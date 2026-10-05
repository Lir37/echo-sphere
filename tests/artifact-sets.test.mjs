import fs from 'node:fs/promises';
import test from 'node:test';
import assert from 'node:assert/strict';
import {
  ARTIFACT_META,
  ARTIFACT_SETS,
  getArtifactSetProgress,
  getArtifactSetArtifactProgress,
  getArtifactSetsForArtifact,
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
  assert.equal(ARTIFACT_META.network_coil.effects.sphereDamage, 0.06);
  assert.equal(ARTIFACT_META.crit_sigil.effects.critChance, 0.05);
  assert.equal(ARTIFACT_META.tempo_ring.effects.sphereDelay, -0.08);
});


test('all 12 Artifact Sets are wired to live completion behavior', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const source = await fs.readFile(new URL('../src/artifactSystem.ts', import.meta.url), 'utf8');
  const all = Object.values(mod.ARTIFACT_META).map((item) => item.id);
  const completed = mod.getArtifactSetBehavior({
    player: { artifacts: all },
  });

  const expected = [
    ['resonance_grid', completed.resonanceGrid],
    ['echo_architecture', completed.echoArchitecture],
    ['singularity_path', completed.singularityPath],
    ['geometry_craft', completed.geometryCraft],
    ['void_horizon', completed.voidHorizon],
    ['temporal_fold', completed.temporalFold],
    ['status_circuit', completed.statusCircuit],
    ['hunter_doctrine', completed.hunterDoctrine],
    ['pulse_engineering', completed.pulseEngineering],
    ['core_forge', completed.coreForge],
    ['prism_dominion', completed.prismDominion],
    ['network_legacy', completed.networkLegacy],
  ];

  for (const [id, active] of expected) {
    assert.equal(active, true, id + ' must become active when its Set is complete');
    const behaviorKey = {
      resonance_grid: 'resonanceGrid',
      echo_architecture: 'echoArchitecture',
      singularity_path: 'singularityPath',
      geometry_craft: 'geometryCraft',
      void_horizon: 'voidHorizon',
      temporal_fold: 'temporalFold',
      status_circuit: 'statusCircuit',
      hunter_doctrine: 'hunterDoctrine',
      pulse_engineering: 'pulseEngineering',
      core_forge: 'coreForge',
      prism_dominion: 'prismDominion',
      network_legacy: 'networkLegacy',
    }[id];
    assert.match(source, new RegExp("setBehavior\\." + behaviorKey + "\\b"), id + ' must have a runtime completion branch');
  }
});

test('live synergy catalog has concrete runtime coverage for every synergy', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const source = await fs.readFile(new URL('../src/artifactSystem.ts', import.meta.url), 'utf8');
  for (const synergy of mod.ARTIFACT_SYNERGIES) {
    assert.ok(synergy.requires.every((id) => mod.ARTIFACT_META[id]), synergy.id + ' references unknown artifact');
    const runtimeMentions = source.split("'" + synergy.id + "'").length - 1;
    assert.ok(runtimeMentions >= 2, synergy.id + ' must appear in catalog and runtime handling');
  }
});


test('each Artifact Protocol changes a measurable combat modifier when its condition is met', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const all = Object.values(mod.ARTIFACT_META).map((item) => item.id);
  const base = {
    player: { artifacts: all, combo: 0, resonanceGeometryKey: 'none', timestopTimer: 0, teleportDamageBuffTimer: 0, sphereMods: {} },
    spheres: [{ type: 'standard', alive: true, pos: { x: 0, y: 0 } }],
    enemies: [],
  };
  const cases = [
    ['triangle_resonance', { player: { ...base.player, resonanceGeometryKey: 'ring' }, spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
      { type: 'prism', alive: true, pos: { x: 200, y: 0 } },
    ] }, 'standard'],
    ['sphere_relay', { player: { ...base.player }, spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
    ] }, 'standard'],
    ['critical_echo', { player: { ...base.player, combo: 5 } }, 'standard'],
    ['geometry_circuit', { player: { ...base.player, resonanceGeometryKey: 'ring' } }, 'standard'],
    ['void_horizon_protocol', { player: { ...base.player }, spheres: [{ type: 'void', alive: true, pos: { x: 0, y: 0 } }], enemies: [{ hp: 100, isElite: true, isBoss: false }] }, 'void'],
    ['temporal_fold_protocol', { player: { ...base.player, timestopTimer: 1 } }, 'standard'],
    ['status_circuit_protocol', { player: { ...base.player, sphereMods: { fire: 1 } }, spheres: [{ type: 'prism', alive: true, pos: { x: 0, y: 0 } }] }, 'prism'],
    ['hunter_doctrine_protocol', { player: { ...base.player }, spheres: [{ type: 'sniper', alive: true, pos: { x: 0, y: 0 } }], enemies: [{ hp: 100, isElite: true, isBoss: false }] }, 'sniper'],
    ['pulse_engineering_protocol', { player: { ...base.player }, enemies: [
      { hp: 100, isElite: false, isBoss: false }, { hp: 100, isElite: false, isBoss: false },
      { hp: 100, isElite: false, isBoss: false }, { hp: 100, isElite: false, isBoss: false },
    ], spheres: [{ type: 'pulse', alive: true, pos: { x: 0, y: 0 } }] }, 'pulse'],
    ['core_forge_protocol', { player: { ...base.player }, spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
      { type: 'prism', alive: true, pos: { x: 0, y: 100 } },
    ] }, 'standard'],
    ['prism_dominion_protocol', { player: { ...base.player }, spheres: [
      { type: 'prism', alive: true, pos: { x: 0, y: 0 } },
      { type: 'standard', alive: true, pos: { x: 100, y: 0 } },
    ] }, 'prism'],
    ['network_legacy_protocol', { player: { ...base.player, resonanceGeometryKey: 'ring' }, spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
      { type: 'prism', alive: true, pos: { x: 0, y: 100 } },
    ] }, 'standard'],
  ];
  for (const [id, overrides, sphereType] of cases) {
    const state = { ...base, ...overrides, player: { ...base.player, ...overrides.player } };
    if (id === 'triangle_resonance') {
      state.spheres = [
        { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
        { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
        { type: 'prism', alive: true, pos: { x: 0, y: 100 } },
      ];
    }
    const before = mod.getSphereArtifactModifiers(base, sphereType, base.spheres[0]).damage;
    const after = mod.getSphereArtifactModifiers(state, sphereType, state.spheres[0]).damage;
    assert.ok(after > before, id + ' must change combat damage when active');
  }
});

test('Artifact Protocols require their real conditions and have combat runtime branches', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const source = await fs.readFile(new URL('../src/artifactSystem.ts', import.meta.url), 'utf8');
  const all = Object.values(mod.ARTIFACT_META).map((item) => item.id);
  const incomplete = mod.getArtifactProtocolStates({
    player: { artifacts: [], combo: 8, resonanceGeometryKey: 'ring' },
    spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 1000, y: 0 } },
      { type: 'chain', alive: true, pos: { x: 0, y: 1000 } },
    ],
  });
  assert.ok(incomplete.every((protocol) => !protocol.discovered && !protocol.active));

  const active = mod.getArtifactProtocolStates({
    player: { artifacts: all, combo: 8, resonanceGeometryKey: 'ring', timestopTimer: 1, sphereMods: { fire: 1 } },
    spheres: [
      { type: 'standard', alive: true, pos: { x: 0, y: 0 } },
      { type: 'sniper', alive: true, pos: { x: 100, y: 0 } },
      { type: 'chain', alive: true, pos: { x: 0, y: 100 } },
      { type: 'prism', alive: true, pos: { x: 100, y: 100 } },
      { type: 'pulse', alive: true, pos: { x: 150, y: 150 } },
      { type: 'void', alive: true, pos: { x: 200, y: 200 } },
      { type: 'orbital', alive: true, pos: { x: 250, y: 250 } },
    ],
    enemies: [
      { hp: 100, isElite: true, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
      { hp: 100, isElite: false, isBoss: false },
    ],
  });
  assert.equal(active.length, 12);
  assert.ok(active.every((protocol) => protocol.discovered && protocol.active));
  for (const protocol of mod.ARTIFACT_PROTOCOLS) {
    assert.ok(source.includes("protocolActive('" + protocol.id + "')"), protocol.id + ' must affect combat runtime');
  }
  assert.match(source, /formsTriangle\(s, sphere\)/, 'Triangle Protocol must require a real geometric triangle');
});


test('Artifact Set UI model exposes concrete component progress for each set', () => {
  const partial = getArtifactSetArtifactProgress(state(['heavy_core', 'resonance_core']));
  const resonance = partial.find((set) => set.setId === 'resonance_grid');
  assert.ok(resonance);
  assert.equal(resonance.owned, 2);
  assert.equal(resonance.total, 4);
  assert.equal(resonance.complete, false);
  assert.ok(getArtifactSetsForArtifact(state(['heavy_core']), 'heavy_core').some((set) => set.setId === 'resonance_grid'));
});

test('Artifact Set UI model reaches complete state from all required concrete artifacts', () => {
  const all = Object.values(ARTIFACT_META).map((item) => item.id);
  const progress = getArtifactSetArtifactProgress(state(all));
  assert.ok(progress.every((set) => set.complete));
});


test('Sphere mutation synergy hints expose the future Ability path from Level IV', async () => {
  const mod = await import('../src/sphereProgression.ts');
  const state = { player: { characterId: 'spherist', sphereProgression: {}, sphereBranches: {}, abilities: {} } };
  const hints = mod.getSphereMutationSynergyHints(state, 'pulse', 'pulse_wave');
  assert.equal(hints.length, 1);
  assert.equal(hints[0].ability, 'blast');
});

test('Sphere mutation synergy activates only for the selected branch at Sphere VII + Ability VII', async () => {
  const mod = await import('../src/sphereProgression.ts');
  const state = {
    player: {
      characterId: 'spherist',
      sphereProgression: { pulse: 7 },
      sphereBranches: { pulse: 'pulse_wave' },
      abilities: { blast: 7 },
    },
  };
  assert.equal(mod.getActiveSphereAbilitySynergies(state).some((x) => x.sphere === 'pulse' && x.sphereBranch === 'pulse_wave' && x.ability === 'blast'), true);
  state.player.sphereBranches.pulse = 'pulse_burst';
  assert.equal(mod.getActiveSphereAbilitySynergies(state).some((x) => x.sphere === 'pulse' && x.sphereBranch === 'pulse_wave'), false);
});

test('Artifact Protocol activation uses each protocol\'s authored condition', async () => {
  const mod = await import('../src/artifactSystem.ts');
  const all = Object.values(mod.ARTIFACT_META).map((item) => item.id);
  const base = {
    player: {
      artifacts: all,
      combo: 0,
      resonanceGeometryKey: 'none',
      timestopTimer: 0,
      teleportDamageBuffTimer: 0,
      sphereMods: {},
    },
    spheres: [
      { type: 'prism', alive: true, pos: { x: 0, y: 0 } },
      { type: 'standard', alive: true, pos: { x: 100, y: 0 } },
    ],
    enemies: [],
  };

  const temporalOff = mod.getArtifactProtocolStates(base).find((x) => x.id === 'temporal_fold_protocol');
  const prismOff = mod.getArtifactProtocolStates(base).find((x) => x.id === 'prism_dominion_protocol');
  assert.equal(temporalOff?.active, false, 'Temporal Fold must not activate from sphere count alone');
  assert.equal(prismOff?.active, false, 'Prism Dominion must require Prism + Orbital/Void');

  const temporalOn = mod.getArtifactProtocolStates({
    ...base,
    player: { ...base.player, timestopTimer: 1 },
  }).find((x) => x.id === 'temporal_fold_protocol');
  assert.equal(temporalOn?.active, true, 'Temporal Fold must activate during time control');

  const prismOn = mod.getArtifactProtocolStates({
    ...base,
    spheres: [
      ...base.spheres,
      { type: 'void', alive: true, pos: { x: 0, y: 100 } },
    ],
  }).find((x) => x.id === 'prism_dominion_protocol');
  assert.equal(prismOn?.active, true, 'Prism Dominion must activate with Prism + specialized partner');
});

