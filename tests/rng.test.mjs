import test from 'node:test';
import assert from 'node:assert/strict';

import { createRngState, nextRandom, randomInt } from '../src/rng.ts';

test('seeded RNG reproduces the same sequence', () => {
  const a = { rngState: createRngState(123456) };
  const b = { rngState: createRngState(123456) };
  const seqA = Array.from({ length: 12 }, () => nextRandom(a));
  const seqB = Array.from({ length: 12 }, () => nextRandom(b));
  assert.deepEqual(seqA, seqB);
  assert.ok(seqA.every((value) => value >= 0 && value < 1));
});

test('different seeds produce different random streams', () => {
  const a = { rngState: createRngState(1) };
  const b = { rngState: createRngState(2) };
  assert.notDeepEqual(
    Array.from({ length: 8 }, () => nextRandom(a)),
    Array.from({ length: 8 }, () => nextRandom(b)),
  );
});

test('randomInt stays inside its half-open range', () => {
  const state = { rngState: createRngState(42) };
  const values = Array.from({ length: 100 }, () => randomInt(state, 7));
  assert.ok(values.every((value) => Number.isInteger(value) && value >= 0 && value < 7));
  assert.equal(randomInt(state, 0), 0);
});
