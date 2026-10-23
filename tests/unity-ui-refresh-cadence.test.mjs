import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Native run bounds model refreshes while native rendering remains frame-driven', () => {
  const document = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs');
  const bridge = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs');

  assert.match(document, /public float RefreshInterval;/);
  assert.match(document, /RefreshInterval>0f&&now>=_nextRenderAt/);
  assert.match(document, /AdvanceMotion\(_root\);using\(UiLayoutMarker\.Auto\(\)\)UpdateLayout\(_root\);PreparePaintResources\(_root\);_layout\.Tracks\(_root\);return;/);
  assert.match(bridge, /if\(_sourceUiWasPaused!=_userPaused\)\{_sourceUiWasPaused=_userPaused;_sourceUi\.Dirty=true;\}/);
  const native = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeRunDocument.cs');
  assert.match(native, /bool update=_sourceUi\.Dirty\|\|\(!_userPaused&&now>=_nativeRunNextModel\)/);
  assert.match(native, /_nativeRunNextModel=now\+\.1/);
  assert.match(native, /if\(update\)ReadNativeMobileState\(\)/);
  assert.match(native, /PrepareNativeRunPresenters\(anchors\);_nativeRunDocument\.End\(SourceDevicePixelRatio\)/);
  assert.doesNotMatch(bridge, /_sourceUi\.Draw\(/);
});
