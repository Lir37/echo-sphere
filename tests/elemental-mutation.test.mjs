import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const read = (file) => fs.readFile(new URL('../' + file, import.meta.url), 'utf8');
const [progression, engineProgression, engineSpheres, enemies, combat, renderer, app, orbital] = await Promise.all([
  read('src/sphereProgression.ts'), read('src/engineProgression.ts'), read('src/engineSpheres.ts'),
  read('src/engineEnemies.ts'), read('src/engineCombat.ts'), read('src/renderer.ts'),
  read('src/App.tsx'), read('src/spheres/orbitalVisual.ts'),
]);

test('Elemental Mutation I keeps completed direct branches untouched and adds special archetypes', () => {
  for (const token of [
    "standard_resonator: 'fire'","standard_singularity: 'freeze'","standard_swarm: 'poison'",
    "sniper_oracle: 'poison'","sniper_assassin: 'fire'","sniper_beacon: 'freeze'",
    "shotgun_burst: 'fire'","shotgun_cataclysm: 'poison'","shotgun_hail: 'freeze'",
    "orbital_dance: 'poison'","orbital_halo: 'freeze'","orbital_blade: 'fire'",
    "prism_split: 'poison'","prism_spectrum: 'fire'","prism_mirror: 'freeze'",
    "void_hunger: 'poison'","void_reaper: 'fire'","void_execution: 'freeze'",
  ]) assert.ok(progression.includes(token), token);
  for (const token of [
    "chain_web: 'freeze'","chain_storm: 'fire'","chain_leech: 'poison'",
    "aura_sanctum: 'freeze'","aura_gravity: 'fire'","aura_overgrowth: 'poison'",
    "gravity_well: 'freeze'","gravity_tide: 'fire'","gravity_collapse: 'poison'",
    "pulse_wave: 'freeze'","pulse_resonator: 'fire'","pulse_burst: 'poison'",
  ]) assert.ok(progression.includes(token), token);
});

test('Elemental Mutation II exposes mastery and keeps existing final choice', () => {
  assert.match(engineProgression, /getSphereElementMasteryForBranch\(branch\.id,\s*index\)/);
  for (const token of ['fire_power','fire_tempo','fire_duration','freeze_duration','freeze_impact','freeze_permafrost','poison_power','poison_tempo','poison_duration']) {
    assert.ok(progression.includes(token), token);
  }
  assert.ok(engineSpheres.includes("mastery?.id === 'fire_power'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'fire_tempo'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'fire_duration'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'poison_power'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'poison_tempo'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'poison_duration'"));
});

test('Fire and Poison Tempo use faster status ticks without raising DPS/sec', () => {
  assert.match(enemies, /const tickInterval = Number(e.fireTickInterval || 0)/);
  assert.ok(enemies.includes('e.hp -= e.fireDps * tickInterval;'));
  assert.match(enemies, /const tickInterval = Number(e.poisonTickInterval || 0)/);
  assert.ok(enemies.includes('e.hp -= e.poisonDps * tickInterval;'));
});

test('Freeze mastery provides same-Sphere vulnerability or post-thaw control', () => {
  assert.ok(combat.includes('fromSphere?.type === enemy.freezeVulnerabilitySource'));
  assert.ok(engineSpheres.includes("mastery?.id === 'freeze_impact'"));
  assert.ok(engineSpheres.includes("mastery?.id === 'freeze_permafrost'"));
});

test('Elemental VFX is a persistent secondary layer', () => {
  assert.ok(renderer.includes('renderSphereElementalVfx(ctx, sphere, s.player, time, scale)'));
  assert.match(app, /SPHERE_ELEMENT_META/);
  assert.match(app, /borderColor: elementMeta.color/);
  assert.ok(orbital.includes('getSphereElementForBranch(branch)'));
  assert.match(orbital, /elementColor/);
});

test('Elemental orbit matches the flattened Sphere orbit grammar and counters it', async () => {
  assert.match(renderer, /renderSphereElementalVfx\(ctx, sphere, s\.player, time, scale\)/);
  const elemental = await read('src/spheres/elementalVisual.ts');
  assert.match(elemental, /const ringRx = radius \* 1\.16/);
  assert.match(elemental, /const ringRy = radius \* \.34/);
  assert.match(elemental, /const ringSpin = -time \* \.18/);
  assert.match(elemental, /const mainWidth = Math\.max\(1\.0, radius \* \.040\)/);
  assert.match(elemental, /const particleCount = 6/);
  assert.match(elemental, /i % 3/);
  assert.match(elemental, /drawFlame\(ctx, size, variant\)/);
  assert.match(elemental, /drawSnowflake\(ctx, size, variant\)/);
  assert.match(elemental, /drawPoisonCloud\(ctx, size, variant\)/);
  assert.doesNotMatch(elemental, /setLineDash/);
});

test('Projectile effect follows selected branch element', () => {
  assert.match(engineSpheres, /const branchElement = getSphereElementForBranch/);
  assert.match(engineSpheres, /let effect: 'none' | 'fire' | 'freeze' | 'poison' = branchElement ?? 'none'/);
  assert.ok(engineSpheres.includes('effect,\n              ricochet:'));
});


test('Chain and Field Level VII use dedicated elemental mastery pools', () => {
  for (const token of ['conduction_power','conduction_rate','conduction_duration','field_power','field_frequency','field_duration']) {
    assert.ok(progression.includes(token), token);
  }
  assert.match(progression, /specialChain = Boolean/);
  assert.match(progression, /specialField = Boolean/);
});

test('Chain elemental identity is conduction, not per-hit direct status spam', () => {
  assert.match(engineSpheres, /function triggerChainElementalReaction/);
  assert.match(engineSpheres, /const threshold = mastery?.id === 'conduction_rate' ? 2 : 3/);
  assert.match(engineSpheres, /enemy.elementalConduction/);
  assert.match(engineSpheres, /triggerChainElementalReaction(s, target, sphere)/);
});

test('Aura, Gravity and Pulse use per-target Field reaction cadence', () => {
  assert.match(engineSpheres, /function applyElementalFieldReaction/);
  assert.match(engineSpheres, /enemy.elementalReactionTimer/);
  assert.match(engineSpheres, /if (isSpecialElementalBranch(branch)) applyElementalFieldReaction/);
  assert.match(enemies, /e.elementalReactionTimer = Math.max(0, e.elementalReactionTimer - dt)/);
});

test('Special elemental VFX has distinct Chain / Aura / Gravity / Pulse signatures', () => {
  const elemental = await read('src/spheres/elementalVisual.ts');
  assert.match(elemental, /function drawSpecialElementalSignature/);
  for (const token of ["sphere.type === 'chain'","sphere.type === 'aura'","sphere.type === 'gravity'"]) assert.ok(elemental.includes(token), token);
  assert.match(elemental, /radius * (0.72 + q * 1.15)/);
});
