import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const progression = fs.readFileSync(new URL('../src/sphereProgression.ts', import.meta.url), 'utf8');
const abilities = fs.readFileSync(new URL('../src/engineAbilities.ts', import.meta.url), 'utf8');
const app = fs.readFileSync(new URL('../src/App.tsx', import.meta.url), 'utf8');

const matrix = progression.slice(
  progression.indexOf('export const SPHERE_ABILITY_SYNERGIES'),
  progression.indexOf('export function getActiveSphereAbilitySynergies'),
);

test('every one of the 30 Sphere Mutation branches has exactly one Ability Synergy', () => {
  const entries = [...matrix.matchAll(/\{id:'([^']+)',sphere:'([^']+)',sphereBranch:'([^']+)',ability:'([^']+)'/g)]
    .map((m) => ({ id: m[1], branch: m[2], ability: m[3] }));
  assert.equal(entries.length, 30);
  assert.equal(new Set(entries.map((e) => e.branch)).size, 30);
  assert.equal(new Set(entries.map((e) => e.id)).size, 30);
});

test('multiple Spheres can converge on the same Ability with different behavioral riders', () => {
  const byAbility = {};
  for (const m of matrix.matchAll(/sphere:'([^']+)',sphereBranch:'([^']+)',ability:'([^']+)'[\s\S]*?behavior:'([^']+)'/g)) {
    const [_, sphere, branch, ability, behavior] = m;
    (byAbility[ability] ||= []).push({ sphere, branch, behavior });
  }
  assert.ok((byAbility.blast || []).length >= 4);
  assert.equal(new Set((byAbility.blast || []).map((e) => e.sphere)).size, (byAbility.blast || []).length);
  assert.equal(new Set((byAbility.blast || []).map((e) => e.behavior)).size, (byAbility.blast || []).length);
  assert.ok((byAbility.shield || []).length >= 3);
  assert.ok((byAbility.teleport || []).length >= 3);
});

test('all 30 synergies are behavioral and routed through the Ability activation layer', () => {
  assert.equal((matrix.match(/behavior:'/g) || []).length, 30);
  for (const ability of ['blast','shield','teleport','firetrail','minion','lightning','timestop','darkritual']) {
    assert.match(abilities, new RegExp(`applySphereAbilitySynergyRiders\\(s, '\\${ability}'\\)`));
  }
});

test('Mutation I hint cards read the branch-specific synergy and pause exposes the full catalog', () => {
  assert.match(progression, /getSphereMutationSynergyHints/);
  assert.match(app, /getSphereMutationSynergyHints\(st, choice\.sphereType/);
  assert.match(app, /SPHERE_ABILITY_SYNERGIES\.map\(\(link\)/);
  assert.match(app, /В ПРОЦЕССЕ/);
  assert.match(app, /ДОСТУПНА/);
  assert.match(app, /ЗАКРЫТА/);
});

test('multi-source synergy on one Ability has diminishing rider strength', () => {
  assert.match(abilities, /function synergyStrength\(index:number\)/);
  assert.match(abilities, /1 \/ \(1 \+ index \* 0\.18\)/);
});
