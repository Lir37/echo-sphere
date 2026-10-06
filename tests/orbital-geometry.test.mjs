import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getOrbitalRingCounts,
  orbitalElementAngle,
  isAngleOnOrbitalSweep,
} from '../src/spheres/orbitalGeometry.ts';

test('Orbital Level I-VII counts follow the exact dual-ring progression', () => {
  const expected = [
    [1, 1],
    [1, 2],
    [2, 2],
    [2, 3],
    [3, 3],
    [3, 4],
    [4, 4],
  ];
  for (let level = 1; level <= 7; level += 1) {
    const [inner, outer] = expected[level - 1];
    assert.deepEqual(getOrbitalRingCounts(level), { inner, outer });
  }
});

test('Orbital inner ring is clockwise and outer ring is counter-clockwise in Canvas space', () => {
  assert.equal(orbitalElementAngle(Math.PI / 2, 'inner', 0, 1), Math.PI / 2);
  assert.equal(orbitalElementAngle(Math.PI / 2, 'outer', 0, 1), -Math.PI / 2);
});

test('Orbital swept collision detects the whole travelled arc', () => {
  assert.equal(isAngleOnOrbitalSweep(0.50, 0.00, 1.00, 1, 0.05), true);
  assert.equal(isAngleOnOrbitalSweep(0.50, 0.00, 0.40, 1, 0.05), false);
  assert.equal(isAngleOnOrbitalSweep(0.02, 6.10, 0.10, 1, 0.05), true);
  assert.equal(isAngleOnOrbitalSweep(-0.50, 0.00, -1.00, -1, 0.05), true);
  assert.equal(isAngleOnOrbitalSweep(-1.50, 0.00, -1.00, -1, 0.05), false);
});

test('Orbital optional elements are added after the canonical 4+4 core', () => {
  assert.deepEqual(getOrbitalRingCounts(7, 1), { inner: 4, outer: 5 });
  assert.deepEqual(getOrbitalRingCounts(7, 2), { inner: 5, outer: 5 });
});
