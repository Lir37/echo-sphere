import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const engineSource = await fs.readFile(new URL('../src/engine.ts', import.meta.url), 'utf8');
const mobileControlsSource = await fs.readFile(new URL('../src/MobileControls.tsx', import.meta.url), 'utf8');

test('expanded Sphere roster is the gameplay source of truth', () => {
  for (const type of ['standard', 'sniper', 'shotgun', 'chain', 'aura', 'orbital', 'prism', 'gravity', 'pulse', 'void']) {
    assert.match(engineSource, new RegExp(`['"]${type}['"]`));
  }
  assert.match(
    engineSource,
    /const sphereTypes = \(Object\.keys\(SPHERE_PROGRESSION\) as SphereType\[\]\)\.filter\(\(type\) => type in SPHERE_TYPES\);/,
  );
});

test('mobile sphere selector uses the complete Sphere roster', () => {
  assert.match(
    mobileControlsSource,
    /Object\.keys\(SPHERE_TYPES\) as SphereType\[\]/,
  );
});
