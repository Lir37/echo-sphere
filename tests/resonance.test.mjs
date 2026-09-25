import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { addResonanceCharge, addResonanceChargeFromSource, clampResonanceCharge, RESONANCE_CHARGE, setResonanceCharge } from '../src/resonance.ts';
import { syncResonanceGeometry } from '../src/engineResonance.ts';

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

test('global Resonance stays separate from local formation cadence state', () => {
  const player = { resonanceCharge: 94 };
  const sphere = { formationHitCount: 0 };

  assert.equal(addResonanceChargeFromSource(player, 'sphereHit'), 0);
  assert.equal(player.resonanceCharge, 95);

  sphere.formationHitCount += 1;
  assert.equal(sphere.formationHitCount, 1);
  assert.equal(player.resonanceCharge, 95);
});

test('invalid Resonance state is normalized instead of poisoning the run resource', () => {
  const state = { resonanceCharge: Number.NaN };
  assert.equal(addResonanceCharge(state, 5), 0);
  assert.equal(state.resonanceCharge, 5);
});


test('Resonance event runtime is extracted from the engine facade', () => {
  const engine = fs.readFileSync(new URL('../src/engine.ts', import.meta.url), 'utf8');
  const runtime = fs.readFileSync(new URL('../src/engineResonance.ts', import.meta.url), 'utf8');
  const spheres = fs.readFileSync(new URL('../src/engineSpheres.ts', import.meta.url), 'utf8');
  const combat = fs.readFileSync(new URL('../src/engineCombat.ts', import.meta.url), 'utf8');

  assert.match(runtime, /export function triggerResonanceEvent/);
  assert.match(runtime, /export function updateResonanceRing/);
  assert.match(runtime, /export function syncResonanceGeometry/);
  assert.match(engine, /triggerResonanceEvent as triggerResonanceEventRuntime/);
  assert.doesNotMatch(engine, /function getResonanceFormation/);
  assert.doesNotMatch(engine, /s\.player\.resonanceEventActive = true/);
  assert.doesNotMatch(spheres, /from '\.\/engine';/);
  assert.doesNotMatch(combat, /from '\.\/engine';/);
});


const emptyGeometry = { line: null, triangle: null, square: null, cluster: null, ring: null, lattice: null, fractal: null };
const triangleGeometry = { ...emptyGeometry, triangle: { type: 'triangle', strength: 1, nodes: [0, 1, 2] } };
const makeGeometryState = () => ({
  time: 0,
  player: {
    resonanceCharge: 0,
    resonanceEventActive: false,
    resonanceGeometryKey: 'none',
    resonanceGeometryNodes: [],
    resonanceLastActiveFormationKey: 'none',
  },
  spheres: [
    { pos: { x: -30, y: 0 } },
    { pos: { x: 0, y: 30 } },
    { pos: { x: 30, y: 0 } },
  ],
});

test('formation Resonance charges only for a genuinely new formation', () => {
  const state = makeGeometryState();
  syncResonanceGeometry(state, triangleGeometry, () => {});
  assert.equal(state.player.resonanceCharge, 5);

  const recoveryState = makeGeometryState();
  recoveryState.player.resonanceGeometryKey = 'triangle:0,1,2';
  recoveryState.player.resonanceGeometryNodes = [0, 1, 2];
  recoveryState.player.resonanceLastActiveFormationKey = 'triangle:0,1,2';
  syncResonanceGeometry(recoveryState, emptyGeometry, () => {});
  assert.equal(recoveryState.player.resonanceCharge, 0);
  syncResonanceGeometry(recoveryState, triangleGeometry, () => {});
  assert.equal(recoveryState.player.resonanceCharge, 0);
});

test('placing or removing a sphere no longer directly charges Resonance', () => {
  const source = fs.readFileSync(new URL('../src/engineSpheres.ts', import.meta.url), 'utf8');
  assert.doesNotMatch(source, /export function placeSphere[\\s\\S]*chargeResonance\\(s, 'network'/);
  assert.doesNotMatch(source, /export function removeSphere[\\s\\S]*chargeResonance\\(s, 'network'/);
});
