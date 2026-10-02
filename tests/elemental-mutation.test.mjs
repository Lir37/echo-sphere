import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const read = (file) => fs.readFile(new URL('../' + file, import.meta.url), 'utf8');
const [progression, engineProgression, engineSpheres, enemies, combat, renderer, app, orbital] = await Promise.all([
  read('src/sphereProgression.ts'), read('src/engineProgression.ts'), read('src/engineSpheres.ts'),
  read('src/engineEnemies.ts'), read('src/engineCombat.ts'), read('src/renderer.ts'),
  read('src/App.tsx'), read('src/spheres/orbitalVisual.ts'),
]);

test('Elemental Mutation I maps only eligible direct-weapon branches', () => {
  for (const token of [
    "standard_resonator: 'fire'","standard_singularity: 'freeze'","standard_swarm: 'poison'",
    "sniper_oracle: 'poison'","sniper_assassin: 'fire'","sniper_beacon: 'freeze'",
    "shotgun_burst: 'fire'","shotgun_cataclysm: 'poison'","shotgun_hail: 'freeze'",
    "orbital_dance: 'poison'","orbital_halo: 'freeze'","orbital_blade: 'fire'",
    "prism_split: 'poison'","prism_spectrum: 'fire'","prism_mirror: 'freeze'",
    "void_hunger: 'poison'","void_reaper: 'fire'","void_execution: 'freeze'",
  ]) assert.ok(progression.includes(token), token);
});

test('Elemental Mutation II exposes mastery and keeps existing final choice', () => {
  assert.match(engineProgression, /getSphereElementMasteryForBranch(branch.id,index)/);
  for (const token of ['fire_power','fire_tempo','fire_duration','freeze_duration','freeze_impact','freeze_permafrost','poison_power','poison_tempo','poison_duration']) {
    assert.ok(progression.includes(token), token);
  }
  assert.match(engineSpheres, /mastery?.id === 'fire_power'/);
  assert.match(engineSpheres, /mastery?.id === 'fire_tempo'/);
  assert.match(engineSpheres, /mastery?.id === 'fire_duration'/);
  assert.match(engineSpheres, /mastery?.id === 'poison_power'/);
  assert.match(engineSpheres, /mastery?.id === 'poison_tempo'/);
  assert.match(engineSpheres, /mastery?.id === 'poison_duration'/);
});

test('Fire and Poison Tempo use faster status ticks without raising DPS/sec', () => {
  assert.match(enemies, /const tickInterval = Number(e.fireTickInterval || 0)/);
  assert.match(enemies, /e.hp -= e.fireDps * tickInterval/);
  assert.match(enemies, /const tickInterval = Number(e.poisonTickInterval || 0)/);
  assert.match(enemies, /e.hp -= e.poisonDps * tickInterval/);
});

test('Freeze mastery provides same-Sphere vulnerability or post-thaw control', () => {
  assert.match(combat, /fromSphere?.type === enemy.freezeVulnerabilitySource/);
  assert.match(engineSpheres, /mastery?.id === 'freeze_impact'/);
  assert.match(engineSpheres, /mastery?.id === 'freeze_permafrost'/);
});

test('Elemental VFX is a persistent secondary layer', () => {
  assert.match(renderer, /renderSphereElementalVfx(ctx, sphere, s.player, time, scale)/);
  assert.match(app, /SPHERE_ELEMENT_META/);
  assert.match(app, /borderColor: elementMeta.color/);
  assert.match(orbital, /getSphereElementForBranch(branch)/);
  assert.match(orbital, /elementColor/);
});

test('Projectile status follows selected branch element', () => {
  assert.match(engineSpheres, /const branchElement = getSphereElementForBranch/);
  assert.match(engineSpheres, /let effect: 'none' | 'fire' | 'freeze' | 'poison' = branchElement ?? 'none'/);
  assert.match(engineSpheres, /if (p.effect !== 'none') applyDirectSphereStatus/);
});
