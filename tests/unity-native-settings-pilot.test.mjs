import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Settings uses native UI Toolkit controls and existing runtime callbacks', () => {
  const screen = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceSettingsToolkitScreen.cs');
  const bridge = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs');
  const owner = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeSettings.cs');
  const layout = read('UnityProject/Assets/Resources/EchoSphere/NativeBattle/NativeSettings.uxml');
  const styles = read('UnityProject/Assets/Resources/EchoSphere/NativeBattle/NativeSettings.uss');
  const panels = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/NativePanelLease.cs');
  assert.match(screen, /NativePanelLease/);
  assert.match(panels, /GetComponent<UIDocument>/);
  assert.match(layout, /<ui:Button name="back"/);
  assert.match(layout, /<ui:Slider[^>]+low-value="0.75" high-value="1.25"/);
  assert.match(layout, /<ui:ScrollView/);
  assert.doesNotMatch(bridge, /_sourceUi\.gameObject\.SetActive\(false\)/);
  assert.doesNotMatch(bridge, /SetSourceScreenHidden/);
  assert.match(styles, /background-color: #02050d/);
  assert.match(owner, /NativeResetAllProgress\(\)/);
  const commands = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeUiPreferences.cs');
  assert.match(commands, /ClearRunSnapshot\(\);PlayerPrefs\.Save\(\)/);
  assert.match(bridge, /case "resetAll":NativeResetAllProgress\(\);return default/);
  assert.match(owner, /_nativeSettingsScreen\.Show\(SourceDevicePixelRatio\*scale/);

  assert.match(layout, /name="settings-confirmation"[\s\S]*name="cancel"[\s\S]*name="ok"/);
  assert.match(layout, /class="settings-pair"><ui:Button name="ru"[^\n]+name="en"/);

});
