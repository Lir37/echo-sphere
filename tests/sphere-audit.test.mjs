import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const read = (file) => fs.readFileSync(path.join(root, file), 'utf8');
const gameData = read('src/gameData.ts');
const progression = read('src/sphereProgression.ts');
const engine = [
  'src/engine.ts',
  'src/engineCombat.ts',
  'src/engineSpheres.ts',
  'src/engineAbilities.ts',
  'src/engineEnemies.ts',
].map(read).join('\n');
const characters = read('src/characters.ts');
const artifacts = read('src/artifactSystem.ts');
const renderer = read('src/renderer.ts');
const mobileControls = read('src/MobileControls.tsx');
const collision = read('src/spaceCollision.ts');


const allTypes = ['standard','sniper','shotgun','chain','aura','orbital','prism','gravity','pulse','void'];
for (const type of allTypes) {
  assert.match(gameData, new RegExp('\\b' + type + '\\s*:'), 'Sphere type missing in gameData: ' + type);
}

const progressionEntries = [
  'standard:sphere(', 'sniper:sphere(', 'shotgun:sphere(', 'chain:sphere(', 'aura:sphere(',
  'orbital:sphere(', 'prism:sphere(', 'gravity:sphere(', 'pulse:sphere(', 'void:sphere('
];
for (const token of progressionEntries) assert.match(progression, new RegExp(token.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')), 'Progression entry missing: ' + token);

const branches = {
  orbital: ['orbital_dance','orbital_halo','orbital_blade'],
  prism: ['prism_split','prism_spectrum','prism_mirror'],
  gravity: ['gravity_well','gravity_tide','gravity_collapse'],
  pulse: ['pulse_wave','pulse_resonator','pulse_burst'],
  void: ['void_hunger','void_reaper','void_execution'],
};
for (const [type, ids] of Object.entries(branches)) {
  for (const id of ids) {
    assert.match(progression, new RegExp(id), 'Branch missing from ' + type + ': ' + id);
    assert.match(engine, new RegExp(id), 'Runtime branch missing from engine: ' + id);
  }
}

for (const fn of ['updateOrbitalSphere','updatePrismSphere','updateGravitySphere','updatePulseSphere']) {
  assert.match(engine, new RegExp('function ' + fn + '\\('), 'Dedicated runtime missing: ' + fn);
}
for (const type of ['orbital','prism','gravity','pulse']) {
  assert.match(engine, new RegExp("sphere\\.type === '" + type + "'"), 'Runtime dispatch missing: ' + type);
}
assert.match(engine, /fromSphere\?\.type === 'void'/, 'Void execution logic missing');
assert.match(engine, /void_reaper/, 'Void Reaper kill logic missing');

for (const type of ['orbital','prism','gravity','pulse','void']) {
  assert.match(characters, new RegExp('preferredSphereTypes:.*' + type), 'Character affinity missing for new sphere: ' + type);
}

for (const token of ['orbitalDamage','prismDamage','gravityRadius','pulseRadius','voidDamage','voidWeakened']) {
  assert.match(artifacts, new RegExp(token), 'Artifact effect field missing: ' + token);
}
for (const token of ['orbital_crown','prism_filter','prism_crown','gravity_bead','gravity_hook','pulse_driver','pulse_crown','void_mark','void_lantern','void_star']) {
  assert.match(artifacts, new RegExp(token), 'Artifact definition missing: ' + token);
}
for (const token of ['orbital_crown','prism_filter','prism_crown','gravity_bead','gravity_hook','pulse_driver','pulse_crown','void_mark','void_lantern','void_star']) {
  if (!['prism_filter','prism_crown'].includes(token)) assert.match(engine, new RegExp(token), 'Runtime artifact hook missing: ' + token);
}


for (const type of ['orbital','prism','gravity','pulse','void']) {
  assert.match(renderer, new RegExp("sphere-" + type), 'Renderer art key missing: ' + type);
  assert.match(renderer, new RegExp("/art/" + type + "\\.svg"), 'Artwork path missing: ' + type);
}
assert.match(renderer, /sphere\.type === 'orbital'[\s\S]*satelliteCount/, 'Orbital satellite visuals missing');
assert.match(engine, /satelliteCount[\s\S]*bestAngularDistance/, 'Orbital satellite damage targeting missing');
assert.match(collision, /resolvePlayerTowerCollisions/, 'Sphere physical collision layer missing');
assert.match(collision, /TOWER_BODY_RADIUS/, 'Sphere body radius contract missing');
assert.match(mobileControls, /canvas\.width \/ rect\.width/, 'Pointer/CSS coordinate mapping missing');
assert.match(engine, /placeSphere\(s: GameState, x: number, y: number\)/, 'Sphere placement entry point missing');

console.log('sphere-audit: OK');