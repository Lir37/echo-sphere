import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Unity source UI bounds logical tree reconciliation while preserving frame-driven motion', () => {
  const document = read('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs');
  const bridge = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs');

  assert.match(document, /public float RefreshInterval;/);
  assert.match(document, /RefreshInterval>0f&&now>=_nextRenderAt/);
  assert.match(document, /AdvanceMotion\(_root\);using\(UiLayoutMarker\.Auto\(\)\)UpdateLayout\(_root\);PreparePaintResources\(_root\);_layout\.Tracks\(_root\);return;/);
  assert.match(bridge, /if\(_sourceUiWasPaused!=_userPaused\)\{_sourceUiWasPaused=_userPaused;_sourceUi\.Dirty=true;\}/);
  assert.match(bridge, /_sourceUi\.Continuous=false;_sourceUi\.RefreshInterval=_mainMenu\|\|_userPaused\?0f:0\.1f/);
});
