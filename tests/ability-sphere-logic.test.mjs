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

test('every Ability evolution id is referenced by engine logic', () => {
  const ids = [...progression.matchAll(/ae\('([^']+)'/g)].map((m) => m[1]);
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
  assert.match(engine, /damage: actual \* 0\.50/);
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
  assert.equal(finalEntries.length, 30);
});

test('Sphere Mutation II final choices keep distinct authored modifier mappings where required', () => {
  assert.match(progression, /prism_mirror:\{0:\{ricochet:1\},1:\{ricochet:2\},2:\{ricochet:2,echo:1\}\}/);
  assert.match(progression, /pulse_resonator:\{0:\{resonant:2\},1:\{resonant:2,impact:1\},2:\{resonant:2,shatter:1\}\}/);
  assert.match(progression, /void_hunger:\{0:\{corrupt:2\},1:\{corrupt:2,resonant:1\},2:\{corrupt:2,drain:1\}\}/);
});
