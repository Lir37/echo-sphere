import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');
const mobileControlsSource = await fs.readFile(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

test('first vertical slice is pinned to Standard, Sniper and Chain', () => {
  assert.match(
    engineSource,
    /VERTICAL_SLICE_SPHERE_TYPES: readonly SphereType\[\] = \['standard', 'sniper', 'chain'\]/,
  );
  assert.match(
    engineSource,
    /const sphereTypes = VERTICAL_SLICE_SPHERE_TYPES\.filter\(\(type\) => type in SPHERE_PROGRESSION\);/,
  );
});

test('mobile sphere selector uses the same first-slice source of truth', () => {
  assert.match(
    mobileControlsSource,
    /const types: SphereType\[\] = \[\.\.\.VERTICAL_SLICE_SPHERE_TYPES\];/,
  );
});
