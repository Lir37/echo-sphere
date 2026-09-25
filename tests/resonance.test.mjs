import test from 'node:test';
import assert from 'node:assert/strict';
import { addResonanceCharge, clampResonanceCharge, setResonanceCharge } from '../src/resonance.ts';

test('Resonance uses a 0..100 base charge and emits an event at 100', () => {
  const state = { resonanceCharge: 96 };
  assert.equal(addResonanceCharge(state, 3), 0);
  assert.equal(state.resonanceCharge, 99);
  assert.equal(addResonanceCharge(state, 1), 1);
  assert.equal(state.resonanceCharge, 0);
});

test('normal Resonance charge cannot overflow above 100 before event resolution', () => {
  const state = { resonanceCharge: 90 };
  assert.equal(addResonanceCharge(state, 40), 1);
  assert.equal(state.resonanceCharge, 0);
});

test('overflow is limited to 150 only when explicitly enabled', () => {
  const state = { resonanceCharge: 90 };
  assert.equal(addResonanceCharge(state, 60, true), 1);
  assert.equal(state.resonanceCharge, 50);
  assert.equal(clampResonanceCharge(999, false), 100);
  assert.equal(clampResonanceCharge(999, true), 150);
});

test('Resonance charge can be set explicitly for deterministic tests and tools', () => {
  const state = { resonanceCharge: 0 };
  setResonanceCharge(state, 73);
  assert.equal(state.resonanceCharge, 73);
  setResonanceCharge(state, -10);
  assert.equal(state.resonanceCharge, 0);
});
