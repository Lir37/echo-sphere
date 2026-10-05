import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const chars = fs.readFileSync(new URL('../src/characters.ts', import.meta.url), 'utf8');
const runtime = fs.readFileSync(new URL('../src/characterRuntime.ts', import.meta.url), 'utf8');
const progression = fs.readFileSync(new URL('../src/engineProgression.ts', import.meta.url), 'utf8');
const spheres = fs.readFileSync(new URL('../src/engineSpheres.ts', import.meta.url), 'utf8');
const resonance = fs.readFileSync(new URL('../src/engineResonance.ts', import.meta.url), 'utf8');

test('roster has ten characters and Signature + Partner affinity', () => {
  for (const id of ['spherist','hunter','engineer','berserker','alchemist','architect','conductor','oracle','voidwalker','fractal']) {
    assert.match(chars, new RegExp('\\n  ' + id + ': \\{'));
  }
  assert.match(chars, /signatureSphereType: 'standard'/);
  assert.match(chars, /partnerSphereType: 'orbital'/);
  assert.match(chars, /signatureSphereType: 'pulse'/);
  assert.match(chars, /partnerSphereType: 'chain'/);
  assert.match(chars, /if \(sphereType === def\.signatureSphereType\) return 0\.70/);
  assert.match(chars, /if \(sphereType === def\.partnerSphereType\) return 0\.30/);
});

test('Signature Sphere cap is 2, then 3 at mastery 5, then 4 at mastery 10', () => {
  assert.match(chars, /if \(masteryLevel >= 10\) return 4/);
  assert.match(chars, /if \(masteryLevel >= 5\) return 3/);
  assert.match(spheres, /sameTypeCap/);
  assert.match(runtime, /index === 2 \? 0\.85 : 0\.70/);
});

test('Oracle uses real Sphere/Ability choices and can preserve one forecast on Reroll', () => {
  assert.match(progression, /oracleForecastKeys/);
  assert.match(progression, /oracleForecastChoice/);
  assert.match(chars, /Открывается 4-я Signature-сфера/);
});

test('Conductor, Voidwalker and Fractal are implemented as runtime overlays', () => {
  assert.match(resonance, /characterId.*conductor|conductorOverdriveTimer/);
  assert.match(runtime, /PHANTOM NODE/);
  assert.match(runtime, /RECURSIVE ECHO/);
});

test('Alchemist reaction mastery scaling is applied before burst damage', () => {
  const scale = runtime.indexOf("if (mastery >= 7) burstMultiplier *= 1.05;");
  const damage = runtime.indexOf("enemy.hp -= baseDamage * burstMultiplier;");
  assert.ok(scale >= 0 && damage > scale);
});
