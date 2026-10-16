import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Settings pilot uses native UI Toolkit controls and existing runtime callbacks', () => {
  const screen = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceSettingsToolkitScreen.cs');
  const bridge = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs');
  assert.match(screen, /AddComponent<UIDocument>/);
  assert.match(screen, /new Button/);
  assert.match(screen, /new Slider\(/);
  assert.match(screen, /new ScrollView/);
  assert.match(bridge, /_sourceUi\.gameObject\.SetActive\(false\)/);
  assert.match(bridge, /SourceUiBinding\("persistence\.ts","resetAll"\)\.Call\(\)/);
});
