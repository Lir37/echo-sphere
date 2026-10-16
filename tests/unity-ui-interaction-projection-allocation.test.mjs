import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
const method=s.slice(s.indexOf('private static void CopyInteraction('),s.indexOf('private static void InheritSubtree('));
test('retained UI interaction projection avoids intermediate LINQ child list',()=>{
 assert.doesNotMatch(method,/\.Where\(|\.ToList\(|FirstOrDefault\(/);
 assert.match(method,/after\.Hover=before\.Hover;after\.Active=before\.Active;after\.Focused=before\.Focused/);
 assert.match(method,/if\(candidate\.Pseudo!=null\)continue;/);
 assert.match(method,/if\(ordinal==index\)\{match=candidate;break;\}/);
 assert.match(method,/if\(candidate\.Properties\["key"\]==key\)\{match=candidate;break;\}/);
 assert.match(method,/CopyInteraction\(match,child\);/);
});
