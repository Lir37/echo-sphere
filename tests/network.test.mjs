import test from 'node:test';
import assert from 'node:assert/strict';
import { analyzeSphereNetwork, getSphereNetworkProfile, getLinkedNodeIndexes, areNetworkNodesLinked } from '../src/network.ts';

const node = (x, y) => ({ pos: { x, y }, alive: true });

test('links connect neighbouring spheres inside the network radius', () => {
  const state = analyzeSphereNetwork([node(0, 0), node(100, 0), node(330, 0)]);
  assert.equal(state.links.length, 1);
  assert.deepEqual(state.links[0], { a: 0, b: 1, distance: 100 });
});

test('three evenly spaced connected spheres form a triangle', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([node(0, 0), node(100, 0), node(50, h)]);
  assert.ok(state.triangle);
  assert.ok(state.triangle.strength >= 0.95);
});

test('four compact spheres resolve to one canonical Geometry without overlap', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(90, 0), node(0, 90), node(90, 90),
  ]);
  assert.ok(state.square || state.cluster);
  const active = [state.square, state.cluster].filter(Boolean);
  assert.equal(active.length, 1);
  assert.ok(active[0].nodes.length === 4);
});

test('a clear row of spheres activates line resonance', () => {
  const state = analyzeSphereNetwork([
    node(-100, 0), node(0, 0), node(100, 0), node(200, 4),
  ]);
  assert.ok(state.line);
  assert.equal(state.line.nodes.length, 4);
});

test('dead spheres do not contribute links or geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), { pos: { x: 50, y: 0 }, alive: false }, node(100, 0),
  ]);
  assert.equal(state.nodes.length, 2);
  assert.equal(state.links.length, 1);
});


test('four evenly spaced points form a square and own all four nodes exclusively', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
  ]);
  assert.ok(state.square);
  assert.equal(state.square.nodes.length, 4);
  assert.equal(getSphereNetworkProfile(state, 0).square, true);
  assert.equal(getSphereNetworkProfile(state, 0).cluster, false);
  assert.equal(getSphereNetworkProfile(state, 0).ring, false);
});

test('one Sphere cannot belong to multiple active Geometries', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(50, h / 2),
  ]);
  assert.ok(state.triangle);
  assert.equal(state.cluster, null);
  assert.equal(state.line, null);
  const triangleNodes = new Set(state.triangle.nodes);
  for (const index of state.cluster?.nodes || []) assert.equal(triangleNodes.has(index), false);
});


test('Network link queries expose the same canonical links used by geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(200, 0),
  ]);

  assert.deepEqual(getLinkedNodeIndexes(state, 1), [0, 2]);
  assert.equal(areNetworkNodesLinked(state, 0, 1), true);
  assert.equal(areNetworkNodesLinked(state, 0, 2), true);
});


test('four-node closed loop activates Ring without requiring diagonals', () => {
  const state = analyzeSphereNetwork([
    node(-90, -90), node(90, -90), node(90, 90), node(-90, 90),
  ]);
  assert.ok(state.ring);
  assert.equal(state.ring.nodes.length, 4);
});

test('two connected triangle cells resolve to one advanced Geometry without node overlap', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(50, h), node(50, -h),
  ]);
  assert.ok(state.lattice || state.triangle);
  const active = [state.fractal, state.lattice, state.triangle].filter(Boolean);
  assert.ok(active.length <= 2);
  if (state.lattice) assert.equal(state.triangle, null);
});


test('disjoint active Geometries can coexist without sharing Spheres', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
    node(450, 0), node(550, 0), node(500, h),
  ]);
  assert.ok(state.square);
  assert.ok(state.triangle);
  const squareNodes = new Set(state.square.nodes);
  for (const index of state.triangle.nodes) assert.equal(squareNodes.has(index), false);
});

test('active Geometry is capped at two simultaneous formations', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
    node(450, 0), node(550, 0), node(500, 86),
    node(850, 0), node(950, 0), node(900, 86),
  ]);
  const active = [
    state.line, state.triangle, state.cluster, state.square,
    state.ring, state.lattice, state.fractal,
  ].filter(Boolean);
  assert.ok(active.length <= 2);
});
