import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const source = readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');

test('native UI animation retains its style comparison snapshot across frames', () => {
  assert.match(source,/readonly Dictionary<string,string> MotionStyleSnapshot=new Dictionary<string,string>\(\)/);
  assert.match(source,/var before=entry!=null\?entry\.MotionStyleSnapshot:_motionFallbackSnapshot/);
  assert.match(source,/before\.Clear\(\);foreach\(var pair in node\.Style\)before\[pair\.Key\]=pair\.Value/);
  assert.match(source,/if\(before\.Count!=node\.Style\.Count\|\|before\.Any\(pair=>!node\.Style\.TryGetValue/);
  assert.match(source,/readonly Dictionary<string,string> _motionFallbackSnapshot=new Dictionary<string,string>\(\)/);
  assert.doesNotMatch(source,/var before=.*new Dictionary<string,string>/);
});
