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
  assert.match(runtime, /previousDominant/);
  assert.match(loop, /s\.networkFrameId \+= 1;/);
  assert.doesNotMatch(loop, /s\.networkFrame = null;/);
  assert.doesNotMatch(spheres, /analyzeSphereNetwork\(/);
  assert.doesNotMatch(combat, /analyzeSphereNetwork\(/);
  assert.doesNotMatch(resonance, /analyzeSphereNetwork\(/);
});


test('render contract separates Echo Drone network links from Sphere-only geometry', async () => {
  const { getSphereVisualNetwork } = await import('../src/networkRender.ts');

  const sourceNetwork = {
    linkDistance: 220,
    nodes: [0, 1, 2],
    links: [
      { a: 0, b: 1, distance: 180 },
      { a: 0, b: 2, distance: 90 },
    ],
    formationCandidates: [
      { type: 'line', strength: 0.9, nodes: [0, 1, 2] },
    ],
    dominantFormation: { type: 'line', strength: 0.9, nodes: [0, 1, 2] },
    secondaryFormation: null,
    line: { type: 'line', strength: 0.9, nodes: [0, 1, 2] },
    triangle: null,
    square: null,
    cluster: null,
    ring: null,
    lattice: null,
    fractal: null,
  };

  const rendered = getSphereVisualNetwork(sourceNetwork, 2);
  assert.deepEqual(rendered.externalLinks, [{ a: 0, b: 2, distance: 90 }]);
  assert.deepEqual(rendered.network.links, [{ a: 0, b: 1, distance: 180 }]);
  assert.deepEqual(rendered.network.line?.nodes, [0, 1]);
});
