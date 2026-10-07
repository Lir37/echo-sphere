import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const sources = await Promise.all([
  '../src/engine.ts',
  '../src/engineCombat.ts',
  '../src/engineSpheres.ts',
  '../src/engineAbilities.ts',
  '../src/engineEnemies.ts',
  '../src/engineProgression.ts',
].map((p) => fs.readFile(new URL(p, import.meta.url), 'utf8')));
const engine = sources.join('\n');
const progression = await fs.readFile(new URL('../src/sphereProgression.ts', import.meta.url), 'utf8');

const activeIds = ['blast','shield','teleport','firetrail','minion','lightning','timestop','darkritual'];

test('all current active Abilities have progression data', () => {
  for (const id of activeIds) {
    assert.ok(progression.includes("ability:'" + id + "'"), 'missing progression for ' + id);
  }
});

test('every base Ability evolution id is referenced by engine logic', () => {
  const ids = [...progression.matchAll(/ae\('([^']+)'/g)].map((m) => m[1])
    .filter((id) => !/_f[1-3]$/.test(id) && id !== 'teleport_echo_jump');
  assert.ok(ids.length >= 40);
  for (const id of ids) assert.ok(engine.includes(id), 'unwired ability evolution ' + id);
});

test('all Sphere branch ids are referenced by engine combat logic', () => {
  const ids = [...progression.matchAll(/br\('([^']+)'/g)].map((m) => m[1]);
  for (const id of ids) assert.ok(engine.includes(id), 'unwired sphere branch ' + id);
});

test('gameplay engine has no unseeded Math.random calls', () => {
  assert.doesNotMatch(engine, /Math\.random\(\)/);
});

test('Standard Swarm is not implemented as hidden permanent Multishot', () => {
  assert.match(engine, /branch === 'standard_swarm'/);
  assert.match(progression, /Standard Swarm is implemented as side shards/);
  assert.doesNotMatch(progression, /type==='standard'&&branch==='standard_swarm'\)\{multishot\+=1/);
});

test('Vitality has a real max-HP effect when acquired', () => {
  assert.match(engine, /if \(ability === 'vitality'\)/);
  assert.match(engine, /s\.player\.maxHp \+= hpGain/);
});


test('Standard Swarm shards spawn from the impact and cannot recursively proc', () => {
  assert.match(engine, /pos: \{ \.\.\.enemy\.pos \}, vel: \{ x: Math\.cos\(a\) \* 320, y: Math\.sin\(a\) \* 320 \}/);
  assert.match(engine, /const shardDamageMultiplier = finalIndex === null/);
  assert.match(engine, /damage: actual \* shardDamageMultiplier/);
  assert.match(engine, /hitEnemies: new Set\(\[enemy\]\)[\s\S]*procOnHit: false/);
  assert.doesNotMatch(engine, /damage: actual \* \(finalIndex === 0 \? 0\.30 : finalIndex === 1 \? 0\.26 : 0\.22\)/);
});


test('Ability Mutation II choices depend on the selected Mutation I branch', () => {
  assert.match(progression, /evolution7ByBranch|ABILITY_FINAL_POOLS/);
  assert.match(engine, /getAbilityEvolutionPool\(s,ability,7\)/);
  for (const id of activeIds) {
    assert.match(progression, new RegExp("ability:'" + id + "[\\s\\S]*?evolution4:[\\s\\S]*?evolution7:[\\s\\S]*?"));
  }
});

test('Every Ability has exactly 9 authored Mutation II finals', () => {
  const specs = progression.slice(
    progression.indexOf('const ABILITY_FINAL_VARIANT_SPECS:'),
    progression.indexOf('function buildAuthoredAbilityFinalVariants'),
  );
  for (const branchId of [
    'blast_resonance','blast_network','blast_core',
    'shield_echo_guard','shield_reflector','shield_bastion',
    'teleport_echo_jump','teleport_beacon','teleport_phase',
    'firetrail_overdrive','firetrail_ignition','firetrail_sanctum',
    'minion_echo_drone','minion_relay_drone','minion_guardian',
    'lightning_echo_storm','lightning_relay','lightning_overload',
    'timestop_echo_phase','timestop_closed_time','timestop_time_anchor',
    'darkritual_blood_link','darkritual_sacrifice','darkritual_void_pact',
  ]) {
    const ids = [...specs.matchAll(new RegExp("\\['(" + branchId + "_f[1-3])'","g"))].map((m) => m[1]);
    assert.equal(ids.length, 3, branchId + ' must have 3 Mutation II finals');
    assert.equal(new Set(ids).size, 3, branchId + ' Mutation II finals must be unique');
  }
  const all = [...specs.matchAll(/\['([^']+_f[1-3])'/g)].map((m) => m[1]);
  assert.equal(all.length, 72);
  assert.equal(new Set(all).size, 72);
});

test('Sphere Ability synergies target exact unique Ability Mutation II finals', () => {
  const targets = [...progression.matchAll(/abilityFinal:'([^']+)'/g)].map((m) => m[1]);
  assert.equal(targets.length, 30);
  assert.equal(new Set(targets).size, 30);
  assert.match(progression, /getAbilityEvolutionChoice\(s, link\.ability, 7\)/);
});

test('Shield Bastion has real defensive behavior', () => {
  assert.match(engine, /shieldBranch === 'shield_bastion'/);
  assert.match(engine, /enemy\.pos\.x \+= \(dx \/ len\) \* 36/);
  assert.match(engine, /shieldBranch === 'shield_bastion' && s\.player\.shieldTimer > 0 \? 0\.70 : 1/);
});

test('Teleport keeps the long-range Sphere targeting unlocked at level 3+', () => {
  assert.match(engine, /if \(lvl >= 3\)/);
  assert.match(engine, /d >= 220 && d <= 700/);
  assert.match(engine, /longTarget \?\? getNearestSphere/);
});


test('Ability mutation VFX is emitted after active Ability activation', () => {
  assert.match(engine, /function emitAbilityMutationVfx/);
  assert.match(engine, /emitAbilityMutationVfx\(s, ability\)/);
  assert.match(engine, /final \? 18 : 10/);
});

test('Sphere Mutation II choices are gameplay mutations, not visual variants', () => {
  const block = progression.slice(
    progression.indexOf('const SPHERE_FINAL_VARIANTS:'),
    progression.indexOf('const finalsFor=', progression.indexOf('const SPHERE_FINAL_VARIANTS:')),
  );
  assert.doesNotMatch(block, /постоянный трёхточечный контур|внешний каскад|апексный элемент/);
  assert.doesNotMatch(block, /persistent three-point crown|outer cascade|directed apex element/);
  const finalEntries = [...block.matchAll(/f\('([^']+)_final_[123]'/g)].map((m) => m[1]);
  assert.equal(finalEntries.length, 90);
  assert.equal(new Set(finalEntries).size, 30);
});

test('Sphere Mutation II final choices keep distinct authored modifier mappings where required', () => {
  assert.match(progression, /prism_mirror:\{0:\{ricochet:1\},1:\{ricochet:2\},2:\{ricochet:2,echo:1\}\}/);
  assert.match(progression, /pulse_resonator:\{0:\{resonant:2\},1:\{resonant:2,impact:1\},2:\{resonant:2,shatter:1\}\}/);
  assert.match(progression, /void_hunger:\{0:\{corrupt:2\},1:\{corrupt:2,resonant:1\},2:\{corrupt:2,drain:1\}\}/);
});

test('Minion Guardian branch has an authored combat effect', () => {
  assert.match(engine, /branch === 'minion_guardian'/);
  assert.match(engine, /minion_guardian'[\s\S]*?anchor\.attackTimer = Math\.max\(0, anchor\.attackTimer - 0\.35\)/);
});

test('Resonance Charge passive scales every authored resonance gain source', async () => {
  assert.match(await fs.readFile(new URL('../src/engineResonance.ts', import.meta.url), 'utf8'),
    /resonanceGainMultiplier = 1 \+ resonanceLevel \* 0\.08/);
  assert.match(await fs.readFile(new URL('../src/engineResonance.ts', import.meta.url), 'utf8'),
    /addResonanceChargeFromSource\(\s*s\.player,\s*source,\s*resonanceGainMultiplier\s*\*\s*sourceEffectMultiplier,/);
});

test('Link Stability passive scales all authored network-disable timers', async () => {
  const enemies = await fs.readFile(new URL('../src/engineEnemies.ts', import.meta.url), 'utf8');
  assert.match(enemies, /function getNetworkDisableDuration/);
  assert.doesNotMatch(enemies, /target\.networkDisabledTimer = 1;/);
  assert.doesNotMatch(enemies, /target\.networkDisabledTimer = 2;/);
  assert.match(enemies, /getNetworkDisableDuration\(s, LINK_BREAKER_DISABLED_SECONDS\)/);
});
