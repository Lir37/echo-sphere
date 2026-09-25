import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engine = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');
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
