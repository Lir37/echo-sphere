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

test('four compact spheres form a cluster and profiles expose membership', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(90, 0), node(0, 90), node(90, 90),
  ]);
  assert.ok(state.cluster);
  for (let i = 0; i < 4; i++) {
    const profile = getSphereNetworkProfile(state, i);
    assert.equal(profile.cluster, true);
    assert.ok(profile.linkedNeighbours >= 2);
  }
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


test('four evenly spaced points form a square and expose square membership', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
  ]);
  assert.ok(state.square);
  assert.equal(state.square.nodes.length, 4);
  for (let i = 0; i < 4; i++) assert.equal(getSphereNetworkProfile(state, i).square, true);
});

test('triangle and cluster can coexist while line is suppressed', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(50, h / 2),
  ]);
  assert.ok(state.triangle);
  assert.ok(state.cluster);
  assert.equal(state.line, null);
});


test('Network link queries expose the same canonical links used by geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(200, 0),
  ]);

  assert.deepEqual(getLinkedNodeIndexes(state, 1), [0, 2]);
  assert.equal(areNetworkNodesLinked(state, 0, 1), true);
  assert.equal(areNetworkNodesLinked(state, 0, 2), true);
});
