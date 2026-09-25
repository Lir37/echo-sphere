import test from 'node:test';
import assert from 'node:assert/strict';

import { buildGhostSnapPreview } from '../src/networkPreview.ts';

const sphere = (x, y) => ({
  pos: { x, y },
  alive: true,
  networkDisabledTimer: 0,
});

test('Ghost Snap predicts candidate links without mutating live spheres', () => {
  const spheres = [sphere(-80, 0), sphere(80, 0), sphere(0, 120)];
  const before = JSON.stringify(spheres);

  const preview = buildGhostSnapPreview(spheres, { x: 0, y: -120 });

  assert.ok(preview);
  assert.equal(preview.candidateIndex, 3);
  assert.ok(preview.linkedNodeIndexes.length >= 2);
  assert.ok(preview.formation);
  assert.equal(JSON.stringify(spheres), before);
});

test('Ghost Snap ignores disabled spheres when projecting a new link', () => {
  const spheres = [sphere(-80, 0), sphere(80, 0), sphere(0, 120)];
  spheres[1].networkDisabledTimer = 3;

  const preview = buildGhostSnapPreview(spheres, { x: 0, y: -120 });

  assert.ok(preview);
  assert.ok(!preview.linkedNodeIndexes.includes(1));
});
