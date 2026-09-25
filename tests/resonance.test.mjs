import test from 'node:test';
import assert from 'node:assert/strict';
import { addResonanceCharge, addResonanceChargeFromSource, clampResonanceCharge, RESONANCE_CHARGE, setResonanceCharge } from '../src/resonance.ts';

test('Resonance uses a 0..100 base charge and emits an event at 100', () => {
  const state = { resonanceCharge: 96 };
  assert.equal(addResonanceCharge(state, 3), 0);
  assert.equal(state.resonanceCharge, 99);
  assert.equal(addResonanceCharge(state, 1), 1);
  assert.equal(state.resonanceCharge, 0);
});

test('crossing 100 preserves the remainder after the Resonance Event', () => {
  const state = { resonanceCharge: 90 };
  assert.equal(addResonanceCharge(state, 40), 1);
  assert.equal(state.resonanceCharge, 30);

  state.resonanceCharge = 50;
  assert.equal(addResonanceCharge(state, RESONANCE_CHARGE.rune), 1);
  assert.equal(state.resonanceCharge, 10);
});

test('authoritative source amounts match the current Blueprint-aligned runtime', () => {
  assert.equal(RESONANCE_CHARGE.sphereHit, 1);
  assert.equal(RESONANCE_CHARGE.geometry, 5);
  assert.equal(RESONANCE_CHARGE.network, 5);
  assert.equal(RESONANCE_CHARGE.rune, 60);
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

test('explicit overflow opt-in remains separate from the baseline 0..100 contract', () => {
  const state = { resonanceCharge: 0 };
  setResonanceCharge(state, 150, false);
  assert.equal(state.resonanceCharge, 100);
  setResonanceCharge(state, 150, true);
  assert.equal(state.resonanceCharge, 150);
});

test('source-routed charge uses the authoritative source amount and multiplier', () => {
  const state = { resonanceCharge: 94 };
  assert.equal(addResonanceChargeFromSource(state, 'geometry'), 0);
  assert.equal(state.resonanceCharge, 99);
  assert.equal(addResonanceChargeFromSource(state, 'sphereHit'), 1);
  assert.equal(state.resonanceCharge, 0);

  const scaled = { resonanceCharge: 50 };
  assert.equal(addResonanceChargeFromSource(scaled, 'rune', 0.5), 0);
  assert.equal(scaled.resonanceCharge, 80);
});

test('invalid Resonance state is normalized instead of poisoning the run resource', () => {
  const state = { resonanceCharge: Number.NaN };
  assert.equal(addResonanceCharge(state, 5), 0);
  assert.equal(state.resonanceCharge, 5);
});
