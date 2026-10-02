import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const read = (rel) => fs.readFileSync(path.join(root, rel), 'utf8');

test('Orbital production module has no forbidden legacy visual path', () => {
  const orbital = read('src/spheres/orbitalVisual.ts');
  assert.equal(orbital.includes('orbital-idle-sheet'), false);
  assert.equal(orbital.includes('reference-48'), false);
  assert.equal(orbital.includes('.b64'), false);
  assert.equal(orbital.includes('shadowBlur'), false);
});

test('renderer owns composition, not viewport/DPR policy', () => {
  const renderer = read('src/renderer.ts');
  assert.equal(renderer.includes('window.devicePixelRatio'), false);
  assert.equal(renderer.includes('Math.min(window.innerWidth'), false);
  assert.equal(renderer.includes('reference-48'), false);
  assert.equal(renderer.includes('orbital-idle-sheet'), false);
});

test('public has no reference dumps or obsolete Orbital sheet', () => {
  const publicDir = path.join(root, 'public');
  const files = [];
  const walk = (dir) => {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) walk(full);
      else files.push(path.relative(publicDir, full).replaceAll(path.sep, '/'));
    }
  };
  walk(publicDir);
  assert.equal(files.some((p) => p.endsWith('.b64.txt')), false);
  assert.equal(files.some((p) => p.includes('reference-48')), false);
  assert.equal(files.some((p) => p === 'art/orbital-idle-sheet-10x256.png'), false);
});
