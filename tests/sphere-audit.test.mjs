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
  'src/engineProgression.ts',
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


for (const type of ['prism','gravity','pulse','void']) {
  assert.match(renderer, new RegExp("sphere-" + type), 'Renderer art key missing: ' + type);
  assert.match(renderer, new RegExp("/art/" + type + "\\.svg"), 'Artwork path missing: ' + type);
}
assert.match(renderer, /renderOrbitalSphereRuntimeVfx/, 'Orbital dedicated visual module is not integrated');
assert.ok(read('src/spheres/orbitalVisual.ts').includes('./visualHelpers'), 'Orbital shared visual helper import missing');
assert.match(read('src/spheres/orbitalVisual.ts'), /core\(ctx/, 'Orbital shared 2.5D core missing');
assert.match(read('src/spheres/orbitalVisual.ts'), /drawSphereOrbit\(ctx/, 'Orbital shared orbit compositor missing');
for (const [type, token] of [
  ['sniper','renderSniperSphereRuntimeVfx'],
  ['shotgun','renderShotgunSphereRuntimeVfx'],
  ['chain','renderChainSphereRuntimeVfx'],
  ['aura','renderAuraSphereRuntimeVfx'],
  ['prism','renderPrismSphereRuntimeVfx'],
  ['gravity','renderGravitySphereRuntimeVfx'],
  ['pulse','renderPulseSphereRuntimeVfx'],
  ['void','renderVoidSphereRuntimeVfx'],
]) {
  assert.match(renderer, new RegExp(token), 'Dedicated visual integration missing: ' + type);
  assert.match(read('src/spheres/' + type + 'Visual.ts'), /createRadialGradient|glow\(/, 'Sphere glow missing: ' + type);
}
assert.match(renderer, /drawStandardSphereAssembly/, 'Standard layered visual assembly is missing');
assert.match(renderer, /sphere-standard-(upper-crystal|core|ring|lower-crystal)/, 'Standard production layers are not integrated');
assert.doesNotMatch(renderer, /sphere-standard-panels|external-panels\.png/, 'Obsolete Standard reference-panel image remains wired into runtime');
assert.doesNotMatch(renderer, /['\"]sphere-standard['\"]\s*:\s*['\"]\/art\/standard\.svg/, 'Full Standard reference sticker is still wired into the renderer');
assert.doesNotMatch(renderer, /drawOrbitalSatelliteArt|drawSphereCoreArt/, 'Legacy procedural Orbital visual remains in renderer');
assert.doesNotMatch(renderer, /sphere-standard-panels|external-panels\\.png/, 'Removed Standard reference panel returned');
assert.match(renderer, /renderOrbitalSphereAttackersVfx/, 'Orbital attacker visual layer missing');
assert.match(renderer, /getOrbitalRingCounts/, 'Orbital visual ring progression missing');
assert.match(engine, /getOrbitalRingCounts/, 'Orbital dual-ring progression missing');
assert.match(engine, /orbitalElementAngle/, 'Orbital angular geometry missing');
assert.match(engine, /isAngleOnOrbitalSweep/, 'Orbital swept collision targeting missing');
assert.match(collision, /resolvePlayerTowerCollisions/, 'Sphere physical collision layer missing');
assert.match(collision, /TOWER_BODY_RADIUS/, 'Sphere body radius contract missing');
assert.match(mobileControls, /canvas\.width \/ rect\.width/, 'Pointer/CSS coordinate mapping missing');
assert.match(engine, /placeSphere\(s: GameState, x: number, y: number\)/, 'Sphere placement entry point missing');
assert.match(read('src/engineBalanceConstants.ts'), /MAX_SAME_SPHERE_COPIES\s*=\s*2/, 'Duplicate Sphere cap must be centralized at 2 copies.');
assert.match(engine, /sameTypeCount\s*=\s*s\.spheres\.filter/, 'Sphere placement must count existing copies.');


console.log('sphere-audit: OK');