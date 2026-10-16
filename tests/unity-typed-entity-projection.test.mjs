import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('live Unity entity projection uses typed accessors rather than private-field reflection', () => {
  const source = read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourcePresentation.cs');
  const start = source.indexOf('private V RenderEntity(MonoBehaviour entity)');
  const end = source.indexOf('private static V RenderShape', start);
  assert.ok(start >= 0 && end > start);
  const method = source.slice(start, end);
  assert.doesNotMatch(method, /RenderField\(/);
  for (const key of ['SourceProjectionOrbitalContactCheckTimer', 'SourceProjectionCreatureVisual', 'SourceProjectionDirection', 'SourceProjectionHitRadius', 'SourceProjectionMaxLife']) {
    assert.ok(method.includes(key), 'missing typed projection accessor: ' + key);
  }
  assert.match(read('UnityProject/Assets/EchoSphere/Scripts/Runtime/EnemyAgent2D.cs'), /SourceProjectionBossCombat => _bossCombat/);
});
