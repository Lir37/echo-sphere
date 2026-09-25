import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { buildRuntimeNetworkNodes } from '../src/networkRuntime.ts';

const sphere = (x, y, alive = true) => ({ pos: { x, y }, alive });
const minion = (x, y, life) => ({ pos: { x, y }, life });

test('runtime Network Nodes preserve sphere indexes and append live/dead drones', () => {
  const nodes = buildRuntimeNetworkNodes(
    [sphere(0, 0), sphere(200, 0)],
    [minion(100, 0, 5), minion(300, 0, 0)],
    true,
  );

  assert.equal(nodes.length, 4);
  assert.deepEqual(nodes[0], { pos: { x: 0, y: 0 }, alive: true });
  assert.deepEqual(nodes[1], { pos: { x: 200, y: 0 }, alive: true });
  assert.deepEqual(nodes[2], { pos: { x: 100, y: 0 }, alive: true });
  assert.deepEqual(nodes[3], { pos: { x: 300, y: 0 }, alive: false });
});

test('runtime Network Nodes exclude drones until Minion reaches node-capable level', () => {
  const nodes = buildRuntimeNetworkNodes(
    [sphere(0, 0)],
    [minion(100, 0, 5)],
    false,
  );

  assert.equal(nodes.length, 1);
  assert.deepEqual(nodes[0], { pos: { x: 0, y: 0 }, alive: true });
});


test('frame-level Network analysis is cached for gameplay readers', () => {
  const runtime = fs.readFileSync(new URL('../src/engineRuntime.ts', import.meta.url), 'utf8');
  const loop = fs.readFileSync(new URL('../src/engineLoop.ts', import.meta.url), 'utf8');
  const spheres = fs.readFileSync(new URL('../src/engineSpheres.ts', import.meta.url), 'utf8');
  const combat = fs.readFileSync(new URL('../src/engineCombat.ts', import.meta.url), 'utf8');
  const resonance = fs.readFileSync(new URL('../src/engineResonance.ts', import.meta.url), 'utf8');

  assert.match(runtime, /export function getNetworkFrame/);
  assert.match(runtime, /s\.networkFrame = \{ frameId: s\.networkFrameId, network \};/);
  assert.match(loop, /s\.networkFrameId \+= 1;/);
  assert.match(loop, /s\.networkFrame = null;/);
  assert.doesNotMatch(spheres, /analyzeSphereNetwork\(/);
  assert.doesNotMatch(combat, /analyzeSphereNetwork\(/);
  assert.doesNotMatch(resonance, /analyzeSphereNetwork\(/);
});
