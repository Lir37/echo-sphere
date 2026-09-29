import test from 'node:test';
import assert from 'node:assert/strict';
import { calculateBackingSize, clampDpr, DPR_CAP } from '../src/renderScale.ts';

test('DPR is capped at 1.75', () => {
  assert.equal(clampDpr(1), 1);
  assert.equal(clampDpr(2), DPR_CAP);
  assert.equal(clampDpr(3), DPR_CAP);
});

test('backing store is deterministic for DPR 1/2/3', () => {
  assert.deepEqual(calculateBackingSize(360, 800, 1), { width: 360, height: 800, dpr: 1 });
  assert.deepEqual(calculateBackingSize(360, 800, 2), { width: 630, height: 1400, dpr: 1.75 });
  assert.deepEqual(calculateBackingSize(360, 800, 3), { width: 630, height: 1400, dpr: 1.75 });
});
