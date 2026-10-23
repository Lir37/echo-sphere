import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Settings uses native UI Toolkit controls and existing runtime callbacks', () => {
  const screen = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceSettingsToolkitScreen.cs');
  const bridge = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs');
  const owner = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeSettings.cs');
  assert.match(screen, /AddComponent<UIDocument>/);
  assert.match(screen, /new Button/);
  assert.match(screen, /new Slider\(/);
  assert.match(screen, /new ScrollView/);
  assert.doesNotMatch(bridge, /_sourceUi\.gameObject\.SetActive\(false\)/);
  assert.doesNotMatch(bridge, /SetSourceScreenHidden/);
  assert.match(screen, /_root\.style\.backgroundColor=Hex\("#02050d"\)/);
  assert.match(owner, /NativeResetAllProgress\(\)/);
  const commands = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeUiPreferences.cs');
  assert.match(commands, /ClearRunSnapshot\(\);PlayerPrefs\.Save\(\)/);
  assert.match(bridge, /case "resetAll":NativeResetAllProgress\(\);return default/);
  assert.match(owner, /_nativeSettingsScreen\.Show\(SourceDevicePixelRatio\*scale/);

  assert.match(screen, /_confirmation\.Add\(row\)/);
  assert.match(screen, /_content\.Add\(language\);_activeRow=null/);

});
