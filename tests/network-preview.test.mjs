import test from 'node:test';
import assert from 'node:assert/strict';

import { buildGhostSnapPreview } from '../src/networkPreview.ts';

const sphere = (x, y, alive = true, networkDisabledTimer = 0) => ({
  pos: { x, y },
  alive,
  networkDisabledTimer,
});

test('Ghost Snap predicts candidate links without mutating live spheres', () => {
  const spheres = [sphere(-80, 0), sphere(80, 0), sphere(0, 120)];
  const before = JSON.stringify(spheres);

  const preview = buildGhostSnapPreview(spheres, { x: 0, y: -80 });

  assert.ok(preview);
  assert.equal(preview.candidateIndex, 3);
  assert.ok(preview.linkedNodeIndexes.length >= 2);
  assert.ok(preview.formation);
  assert.equal(JSON.stringify(spheres), before);
});

test('Ghost Snap preserves source indexes when disabled/dead spheres are present', () => {
  const spheres = [
    sphere(-100, 0),
    sphere(0, 0, false),
    sphere(100, 0, true, 2),
    sphere(0, 120),
  ];

  const preview = buildGhostSnapPreview(spheres, { x: 0, y: -120 });

  assert.ok(preview);
  assert.equal(preview.candidateIndex, 4);
  assert.ok(preview.nodes[1] && preview.nodes[1].alive === false);
  assert.ok(preview.nodes[2] && preview.nodes[2].alive === false);
  assert.ok(preview.linkedNodeIndexes.includes(0));
  assert.ok(preview.linkedNodeIndexes.includes(3));
});

test('Ghost Snap ignores disabled spheres when projecting a new link', () => {
  const spheres = [sphere(-80, 0), sphere(80, 0), sphere(0, 120)];
  spheres[1].networkDisabledTimer = 3;

  const preview = buildGhostSnapPreview(spheres, { x: 0, y: -120 });

  assert.ok(preview);
  assert.ok(!preview.linkedNodeIndexes.includes(1));
});

test('Ghost Snap previews repositioning without adding a duplicate node', () => {
  const spheres = [
    sphere(-100, 0),
    sphere(100, 0),
    sphere(0, 100),
    sphere(0, -100),
  ];
  const before = JSON.stringify(spheres);

  const preview = buildGhostSnapPreview(spheres, { x: 200, y: 0 }, 220, 1);

  assert.ok(preview);
  assert.equal(preview.candidateIndex, 1);
  assert.equal(preview.nodes.length, spheres.length);
  assert.equal(preview.nodes[1].pos.x, 200);
  assert.equal(preview.nodes[1].pos.y, 0);
  assert.equal(preview.nodes.filter((node) => node.alive !== false).length, 4);
  assert.equal(JSON.stringify(spheres), before);
});

test('Ghost Snap reports advanced Geometry formations, not only basic shapes', () => {
  const h = Math.sqrt(3) * 100 / 2;
  const spheres = [
    sphere(0, 0),
    sphere(100, 0),
    sphere(50, h),
    sphere(50, -h),
  ];

  const preview = buildGhostSnapPreview(spheres, { x: 150, y: 0 });

  assert.ok(preview);
  assert.ok(['triangle', 'cluster', 'lattice', 'fractal'].includes(preview.formation?.type || ''));
});
