import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('live player projection uses explicit typed assignments for runtime fields', () => {
  const projection = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourcePresentation.cs');
  assert.doesNotMatch(projection, /runtimeFields=RenderFields\(typeof\(EchoSphereRuntime\)\)/);
  assert.match(projection, /player\["activeAbilitySlots"\]=_activeAbilitySlots/);
  assert.match(projection, /player\["fractalFormationHistory"\]=RenderValue\(_fractalFormationHistory\)/);
  assert.match(projection, /value is SourceSphereModifiers modifiers/);
  assert.match(projection, /value is FractalFormationSnapshot snapshot/);
});
