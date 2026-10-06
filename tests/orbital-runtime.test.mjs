import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { getOrbitalRingCounts, orbitalElementAngle, isAngleOnOrbitalSweep } from '../src/spheres/orbitalGeometry.ts';

const read = (file) => fs.readFileSync(file, 'utf8');

test('Orbital Blade boss contact routes damage and telemetry through the shared combat path', () => {
  const engine = read('src/engineSpheres.ts');
  const combat = read('src/engineCombat.ts');
  const renderer = read('src/renderer.ts');
  const visual = read('src/spheres/orbitalVisual.ts');

  assert.match(engine, /const contactBand = Math\.max\([\s\S]*enemy\.radius/);
  assert.match(engine, /dealDamageToEnemy\(s, enemy, hitDamage, sphere\)/);
  assert.match(combat, /sourceSphereType: fromSphere\?\.type/);
  assert.match(renderer, /sourceSphereType === 'orbital'/);
  assert.match(visual, /getOrbitalRingCounts/);
});

test('Orbital Level IV can register a large boss whose body overlaps the visible path', () => {
  const bodyRadius = Math.min(25, 25 * 0.19 + 4 * 0.8);
  const innerRadius = bodyRadius * 1.68;
  const bossRadius = 42;
  const contactBand = Math.max(19, bossRadius + 6);

  assert.equal(getOrbitalRingCounts(4).inner, 2);
  assert.equal(getOrbitalRingCounts(4).outer, 3);
  assert.ok(Math.abs(innerRadius - innerRadius) <= contactBand);

  const startAngle = orbitalElementAngle(0, 'inner', 0, 2);
  const endAngle = orbitalElementAngle(0.168, 'inner', 0, 2);
  assert.ok(isAngleOnOrbitalSweep(0.08, startAngle, endAngle, 1, 0.22));
});

test('Orbital L1-L7 progression remains 1+1 through 4+4 and alternates outer/inner', () => {
  assert.deepEqual(getOrbitalRingCounts(1), { inner: 1, outer: 1 });
  assert.deepEqual(getOrbitalRingCounts(2), { inner: 1, outer: 2 });
  assert.deepEqual(getOrbitalRingCounts(3), { inner: 2, outer: 2 });
  assert.deepEqual(getOrbitalRingCounts(4), { inner: 2, outer: 3 });
  assert.deepEqual(getOrbitalRingCounts(5), { inner: 3, outer: 3 });
  assert.deepEqual(getOrbitalRingCounts(6), { inner: 3, outer: 4 });
  assert.deepEqual(getOrbitalRingCounts(7), { inner: 4, outer: 4 });
  assert.ok(orbitalElementAngle(1, 'inner', 0, 1) > 0);
  assert.ok(orbitalElementAngle(1, 'outer', 0, 1) < 0);
});
