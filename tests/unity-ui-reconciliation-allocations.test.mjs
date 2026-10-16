import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const source = readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs', 'utf8');

test('source UI reconciliation reuses its stale-entry removal buffer', () => {
  assert.match(source, /readonly List<string> _staleEntryKeys=new List<string>\(\)/);
  assert.match(source, /_staleEntryKeys\.Clear\(\);[\s\S]*?foreach\(var pair in _entries\)if\(!pair\.Value\.Used\)_staleEntryKeys\.Add\(pair\.Key\)/);
  assert.match(source, /foreach\(var key in _staleEntryKeys\)\{var entry=_entries\[key\];Dispose\(entry\);_entries\.Remove\(key\);\}/);
  assert.doesNotMatch(source, /_entries\.Where\(pair=>!pair\.Value\.Used\)\.ToArray\(\)/);
});
