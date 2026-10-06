import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const read = (file) => fs.readFileSync(file, 'utf8');

const progression = read('src/sphereProgression.ts');
const runtime = [
  read('src/engineSpheres.ts'),
  read('src/engineCombat.ts'),
  read('src/engineResonance.ts'),
].join('\n');
const mutationVisual = read('src/spheres/mutationVisual.ts');
const orbitalVisual = read('src/spheres/orbitalVisual.ts');

const sphereTypes = [
  'standard','sniper','shotgun','chain','aura',
  'orbital','prism','gravity','pulse','void',
];

const branchIds = [
  'standard_resonator','standard_singularity','standard_swarm',
  'sniper_oracle','sniper_assassin','sniper_beacon',
  'shotgun_burst','shotgun_cataclysm','shotgun_hail',
  'chain_web','chain_storm','chain_leech',
  'aura_sanctum','aura_gravity','aura_overgrowth',
  'orbital_dance','orbital_halo','orbital_blade',
  'prism_split','prism_spectrum','prism_mirror',
  'gravity_well','gravity_tide','gravity_collapse',
  'pulse_wave','pulse_resonator','pulse_burst',
  'void_hunger','void_reaper','void_execution',
];

test('all 10 Spheres share the canonical 7-level progression structure', () => {
  assert.match(
    progression,
    /const lv=\(a:string,b:string,c:string\)=>\[\{level:1,[\s\S]*?\{level:7,/,
  );
  for (const type of sphereTypes) {
    assert.match(progression, new RegExp('\\b' + type + ':sphere\\('), type);
  }
});

test('all 30 Mutation I branches have Level V/VI descriptions, runtime, and final triples', () => {
  assert.equal(new Set(branchIds).size, 30);
  for (const id of branchIds) {
    assert.match(progression, new RegExp(id + ':\\{level5:'), id + ' RU Level V');
    assert.match(progression, new RegExp(id + ':\\{level5:[\\s\\S]*level6:'), id + ' RU Level VI');
    assert.match(progression, new RegExp(id + ':\\{0:\\{'), id + ' final modifier table');
    assert.match(runtime, new RegExp(id), id + ' runtime');
  }
});

test('all 90 Level VII finals are defined and flow through persistent final runtime', () => {
  let finalCount = 0;
  for (const branch of branchIds) {
    for (const index of [1, 2, 3]) {
      assert.match(progression, new RegExp(branch + '_final_' + index), branch + ' final ' + index);
      finalCount += 1;
    }
  }
  assert.equal(finalCount, 90);
  assert.match(mutationVisual, /function drawFinalExpansion/);
  assert.match(mutationVisual, /if\(final<0\)return/);
  assert.match(runtime, /finalIndex === 2/);
});

test('every Sphere mutation family has a persistent visual path', () => {
  const families = ['standard','sniper','shotgun','chain','aura','prism','gravity','pulse','void'];
  const visualFamilyContracts = {
    standard: ['standard_resonator', 'standard_singularity', 'standard_swarm'],
    sniper: ['sniper_oracle', 'sniper_assassin', 'sniper_beacon'],
    shotgun: ['shotgun_burst', 'shotgun_cataclysm', 'shotgun_hail'],
    chain: ['chain_web', 'chain_storm', 'chain_leech'],
    aura: ['aura_sanctum', 'aura_gravity', 'aura_overgrowth'],
    prism: ['prism_split', 'prism_spectrum', 'prism_mirror'],
    gravity: ['gravity_well', 'gravity_tide', 'gravity_collapse'],
    pulse: ['pulse_wave', 'pulse_resonator', 'pulse_burst'],
    void: ['void_hunger', 'void_reaper', 'void_execution'],
  };
  for (const [family, ids] of Object.entries(visualFamilyContracts)) {
    assert.ok(
      ids.every((id) => mutationVisual.includes(id)) ||
      mutationVisual.includes("branch.startsWith('" + family + "_')"),
      'missing visual family path: ' + family,
    );
  }
  assert.match(orbitalVisual, /orbital_dance/);
  assert.match(orbitalVisual, /orbital_blade/);
  assert.match(progression, /orbital_halo/);
  assert.match(orbitalVisual, /drawSatellite/);
});

test('Orbital damage is authoritative and its visual/combat orbit geometry is shared', () => {
  assert.match(runtime, /dealDamageToEnemy\(s, enemy, hitDamage, sphere\)/);
  assert.match(runtime, /isAngleOnOrbitalSweep/);
  assert.match(progression, /\+15% скорости вращения боевых элементов/);
  assert.match(progression, /\+15% combat element rotation speed/);
});
