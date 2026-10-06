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

const canonicalBranchIds = [
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
assert.equal(new Set(canonicalBranchIds).size, 30, 'Canonical branch roster must contain 30 unique branches.');
for (const id of canonicalBranchIds) {
  assert.match(progression, new RegExp(id + ".*signature behavior"), 'Branch description missing: ' + id);
}

assert.equal(canonicalBranchIds.length * 3, 90, 'Canonical Sphere roster must expose exactly 90 Level-VII finals.');
for (const id of canonicalBranchIds) {
  for (const index of [0, 1, 2]) {
    assert.match(progression, new RegExp(id + "_final_" + (index + 1)), 'Final description missing: ' + id + ' final ' + (index + 1));
  }
}

assert.match(progression, /\['\+15% орбитального урона','\+15% радиуса орбиты','\+15% скорости вращения боевых элементов'\]/,
  'Orbital Level III must describe combat-element rotation speed, not an undefined interval.');
assert.match(gameData, /id: 'orbital',[\s\S]*delayMult: 1, projectileSpeedMult: 1/,
  'Orbital must not expose a misleading attack-delay multiplier.');
assert.match(progression, /rotationSpeed\*=1\.15/,
  'Orbital Level III rotation speed modifier is missing.');
assert.match(engine, /const bodyRadius = Math\.max\(15, Math\.min\(25, sphere\.radius \* 0\.19 \+ level \* 0\.8\)\)/,
  'Orbital combat radius must use the same body-radius basis as the renderer.');
assert.match(engine, /bodyRadius \* 1\.68 \* radiusMultiplier/,
  'Orbital inner combat ring is not aligned with the authored visual orbit.');
assert.match(engine, /bodyRadius \* 2\.02 \* radiusMultiplier/,
  'Orbital outer combat ring is not aligned with the authored visual orbit.');
assert.match(engine, /branch === 'orbital_dance' && finalIndex === 2/,
  'Only Orbital Dance final III should add the extra core satellite.');
assert.match(progression, /orbital_dance:\{0:\{afterimage:2\},1:\{afterimage:2\},2:\{afterimage:2\}\}/,
  'Orbital Dance finals must retain their authored Afterimage behavior.');
assert.match(progression, /prism_spectrum:\{0:\{fire:1\},1:\{freeze:1\},2:\{poison:1\}\}/,
  'Prism Spectrum finals must map to Fire/Freeze/Poison.');
assert.match(engine, /const bounceCount = Math\.min\(2, mods\.ricochet\)/,
  'Prism Mirror finals must consume authored Ricochet levels.');
assert.match(read('src/spheres/orbitalGeometry.ts'), /export function isAngleOnOrbitalSweep/,
  'Orbital swept collision contract must live in the shared geometry module.');
assert.ok(engine.includes('finalIndex === 1 ? 1.35 : finalIndex === 2 ? 1.50 : 1.15'),
  'Gravity Well final pull scaling must preserve three authored strengths.');
assert.ok(engine.includes('finalIndex === 2 ? 1.50'),
  'Gravity Well final III pull scaling must be stronger than the base branch.'););
assert.match(engine, /finalIndex === null || finalIndex === 0 || finalIndex === 2/,
  'Standard Singularity base branch must pull enemies before Level VII selection.');
assert.match(engine, /const baseExecuteChance = voidLevel >= 2 ? 0\.10 : 0/,
  'Void Level II execute chance must have a concrete runtime consumer.');


const baseLevelContracts = [
  [/if\\(type==='standard'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.15/, 'Standard L1 damage'],
  [/if\\(type==='standard'\\)[\\s\\S]*if\\(l>=2\\) pierce\\+=1/, 'Standard L2 pierce'],
  [/if\\(type==='standard'\\)[\\s\\S]*if\\(l>=3\\) delay\\*=\\.9/, 'Standard L3 delay'],
  [/if\\(type==='sniper'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.25/, 'Sniper L1 damage'],
  [/if\\(type==='sniper'\\)[\\s\\S]*if\\(l>=2\\) radius\\*=1\\.15/, 'Sniper L2 range'],
  [/sphere\\?\\.type === 'sniper' && sphereLevel\\(s, 'sniper'\\) >= 3/, 'Sniper L3 crit'],
  [/if\\(type==='shotgun'\\)[\\s\\S]*if\\(l>=1\\) multishot\\+=1/, 'Shotgun L1 pellet'],
  [/fromSphere\\?\\.type === 'shotgun' && sphereLevel\\(s, 'shotgun'\\) >= 2/, 'Shotgun L2 close damage'],
  [/if\\(l>=3\\) spreadMult\\*=\\.88/, 'Shotgun L3 spread'],
  [/if\\(type==='chain'\\)[\\s\\S]*if\\(l>=1\\) chainTargets\\+=1/, 'Chain L1 target'],
  [/if\\(type==='chain'\\)[\\s\\S]*if\\(l>=2\\) damage\\*=1\\.10/, 'Chain L2 damage'],
  [/if\\(type==='chain'\\)[\\s\\S]*if\\(l>=3\\) delay\\*=\\.85/, 'Chain L3 interval'],
  [/if\\(type==='aura'\\)[\\s\\S]*if\\(l>=1\\) auraRadius\\*=1\\.20/, 'Aura L1 radius'],
  [/if\\(type==='aura'\\)[\\s\\S]*if\\(l>=2\\) auraPulse\\*=\\.9/, 'Aura L2 interval'],
  [/if\\(type==='aura'\\)[\\s\\S]*if\\(l>=3\\) damage\\*=1\\.10/, 'Aura L3 damage'],
  [/if\\(type==='orbital'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.15/, 'Orbital L1 damage'],
  [/if\\(type==='orbital'\\)[\\s\\S]*if\\(l>=2\\) radius\\*=1\\.15/, 'Orbital L2 radius'],
  [/if\\(type==='orbital'\\)[\\s\\S]*if\\(l>=3\\) rotationSpeed\\*=1\\.15/, 'Orbital L3 rotation speed'],
  [/if\\(type==='prism'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.20/, 'Prism L1 damage'],
  [/if\\(type==='prism'\\)[\\s\\S]*if\\(l>=2\\) radius\\*=1\\.15/, 'Prism L2 range'],
  [/if\\(type==='prism'\\)[\\s\\S]*if\\(l>=3\\) multishot\\+=1/, 'Prism L3 direction'],
  [/if\\(type==='gravity'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.20/, 'Gravity L1 damage'],
  [/if\\(type==='gravity'\\)[\\s\\S]*if\\(l>=2\\) radius\\*=1\\.15/, 'Gravity L2 radius'],
  [/if\\(type==='gravity'\\)[\\s\\S]*if\\(l>=3\\) auraPulse\\*=\\.85/, 'Gravity L3 interval'],
  [/if\\(type==='pulse'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.20/, 'Pulse L1 damage'],
  [/if\\(type==='pulse'\\)[\\s\\S]*if\\(l>=2\\) radius\\*=1\\.15/, 'Pulse L2 radius'],
  [/if\\(type==='pulse'\\)[\\s\\S]*if\\(l>=3\\) auraPulse\\*=\\.88/, 'Pulse L3 interval'],
  [/if\\(type==='void'\\)[\\s\\S]*if\\(l>=1\\) damage\\*=1\\.20/, 'Void L1 damage'],
  [/if\\(type==='void'\\)[\\s\\S]*if\\(l>=3\\) radius\\*=1\\.15/, 'Void L3 range'],
];
for (const [pattern, label] of baseLevelContracts) {
  assert.match(progression, pattern, label + ' contract missing.');
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
assert.match(read('src/spheres/orbitalVisual.ts'), /getOrbitalRingCounts/, 'Orbital visual ring progression missing');
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