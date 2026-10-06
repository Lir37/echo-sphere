import test from 'node:test';
import assert from 'node:assert/strict';
import { analyzeSphereNetwork, getSphereNetworkProfile, getLinkedNodeIndexes, areNetworkNodesLinked, getFormationBonusMultiplier } from '../src/network.ts';

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

test('four compact spheres choose Square as dominant Geometry while allowing a secondary layer', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(90, 0), node(0, 90), node(90, 90),
  ]);
  assert.ok(state.square || state.cluster);
  assert.equal(state.dominantFormation?.type, 'square');
  assert.ok(state.dominantFormation?.dominanceScore);
  assert.ok(state.secondaryFormation);
  assert.ok([state.square, state.cluster].filter(Boolean).length >= 1);
});

test('a clear row of spheres activates line resonance', () => {
  const state = analyzeSphereNetwork([
    node(-100, 0), node(0, 0), node(100, 0), node(200, 4),
  ]);
  assert.ok(state.line);
  assert.equal(state.line.nodes.length, 4);
});

test('line formation is detected from three aligned Spheres inside a larger Triangle network', () => {
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, 86.6025),
    node(200, 0),
  ]);
  assert.ok(state.triangle);
  assert.ok(state.line);
  assert.equal(state.line.nodes.length, 3);
  assert.deepEqual(state.line.nodes, [0, 1, 3]);
});

test('dead spheres do not contribute links or geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), { pos: { x: 50, y: 0 }, alive: false }, node(100, 0),
  ]);
  assert.equal(state.nodes.length, 2);
  assert.equal(state.links.length, 1);
});


test('four evenly spaced points form Square and can share nodes with a secondary Geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
  ]);
  assert.ok(state.square);
  assert.equal(state.square.nodes.length, 4);
  assert.equal(getSphereNetworkProfile(state, 0).square, true);
  assert.equal(getSphereNetworkProfile(state, 0).cluster, Boolean(state.cluster));
  assert.equal(getSphereNetworkProfile(state, 0).ring, Boolean(state.ring));
});

test('a Sphere may participate in two active Geometries when their scores are close', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(50, h / 2),
  ]);
  assert.ok(state.triangle);
  assert.ok(state.cluster);
  const triangleNodes = new Set(state.triangle.nodes);
  assert.ok((state.cluster?.nodes || []).some((index) => triangleNodes.has(index)));
});


test('Network link queries expose the same canonical links used by geometry', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(200, 0),
  ]);

  assert.deepEqual(getLinkedNodeIndexes(state, 1), [0, 2]);
  assert.equal(areNetworkNodesLinked(state, 0, 1), true);
  assert.equal(areNetworkNodesLinked(state, 0, 2), true);
});


test('four-node closed loop keeps Square dominant and Ring available as secondary Geometry', () => {
  const state = analyzeSphereNetwork([
    node(-90, -90), node(90, -90), node(90, 90), node(-90, 90),
  ]);
  assert.ok(state.square || state.ring);
  const chosen = state.square || state.ring;
  assert.equal(chosen.nodes.length, 4);
  assert.equal(state.dominantFormation?.type, 'square');
  assert.ok(state.ring);
});

test('two connected triangle cells can resolve to advanced Geometry without requiring exclusive nodes', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(50, h), node(50, -h),
  ]);
  assert.ok(state.lattice || state.triangle);
  const active = [state.fractal, state.lattice, state.triangle].filter(Boolean);
  assert.ok(active.length <= 2);
  if (state.lattice) assert.equal(state.dominantFormation?.type, 'lattice');
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


test('adding a fourth Sphere does not evict an established Triangle unless a stronger form is actually built', () => {
  const h = Math.sqrt(3) * 50;
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(50, 180),
  ]);
  assert.ok(state.triangle);
  assert.equal(state.dominantFormation?.type, 'triangle');
});

test('temporarily disabled Dominant Triangle keeps its slot identity and returns after recovery', () => {
  const h = Math.sqrt(3) * 50;
  const activeNodes = [
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(200, 0),
  ];
  const base = analyzeSphereNetwork(activeNodes);
  assert.equal(base.dominantFormation?.type, 'triangle');

  const disabledNodes = activeNodes.map((value, index) =>
    index === 2 ? { ...value, alive: false } : value
  );
  const suppressed = analyzeSphereNetwork(
    disabledNodes,
    220,
    'triangle',
    { dominant: 'triangle', secondary: 'line' },
    { ...base.dominantFormation, active: false, inactiveReason: 'network-disabled' },
    true,
  );
  assert.equal(suppressed.dominantFormation?.type, 'triangle');
  assert.equal(suppressed.dominantFormation?.active, false);
  assert.equal(suppressed.secondaryFormation?.type, 'line');
  assert.equal(getFormationBonusMultiplier(suppressed, 'triangle'), 0);
  assert.equal(getFormationBonusMultiplier(suppressed, 'line'), 0.5);

  const restored = analyzeSphereNetwork(
    activeNodes,
    220,
    'triangle',
    { dominant: 'triangle', secondary: 'line' },
    suppressed.dominantFormation,
  );
  assert.equal(restored.dominantFormation?.type, 'triangle');
  assert.notEqual(restored.dominantFormation?.active, false);
  assert.equal(restored.secondaryFormation?.type, 'line');
  assert.equal(getFormationBonusMultiplier(restored, 'triangle'), 1);
});
test('a player can switch dominance by creating a materially stronger Square layout', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(100, 100), node(0, 100),
  ], 220, 'triangle');
  assert.equal(state.dominantFormation?.type, 'square');
  assert.ok((state.dominantFormation?.dominanceScore || 0) >= 100);
});

test('small geometric drift keeps the previous dominant formation when the challenger is inside the switch margin', () => {
  const h = Math.sqrt(3) * 50;
  const state = analyzeSphereNetwork([
    node(0, 0),
    node(100, 0),
    node(50, h),
    node(50, 180),
  ], 220, 'triangle');
  assert.equal(state.dominantFormation?.type, 'triangle');
});


test('Triangle can remain dominant while an overlapping Square becomes the secondary formation', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(50, 86.6025),
    node(100, 100), node(0, 100),
  ], 220, 'triangle', { dominant: 'triangle', secondary: 'square' });
  assert.equal(state.dominantFormation?.type, 'triangle');
  assert.equal(state.secondaryFormation?.type, 'square');
  assert.equal(getFormationBonusMultiplier(state, 'triangle'), 1);
  assert.equal(getFormationBonusMultiplier(state, 'square'), 0.5);
});

test('HUD-style formation swap selection reverses the two active formation slots', () => {
  const before = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(50, 86.6025),
    node(100, 100), node(0, 100),
  ], 220, 'triangle', { dominant: 'triangle', secondary: 'square' });
  assert.equal(before.dominantFormation?.type, 'triangle');
  assert.equal(before.secondaryFormation?.type, 'square');

  const swapped = analyzeSphereNetwork(
    [
      node(0, 0), node(100, 0), node(50, 86.6025),
      node(100, 100), node(0, 100),
    ],
    220,
    'triangle',
    { dominant: 'square', secondary: 'triangle' },
  );
  assert.equal(swapped.dominantFormation?.type, 'square');
  assert.equal(swapped.secondaryFormation?.type, 'triangle');
});

test('Secondary Geometry exposes half potency for a formation member', () => {
  const state = analyzeSphereNetwork([
    node(0, 0), node(100, 0), node(50, 86.6025),
    node(100, 100), node(0, 100),
  ], 220, 'triangle', { dominant: 'triangle', secondary: 'square' });
  assert.equal(getFormationBonusMultiplier(state, 'triangle', 0), 1);
  assert.equal(getFormationBonusMultiplier(state, 'square', 0), 0.5);
  assert.equal(getFormationBonusMultiplier(state, 'square', 2), 0);
});
