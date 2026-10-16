import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const code=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
const end=code.split('public void End()')[1].split('private void ')[0];
test('native Canvas gradient stop ordering retains stable ties without LINQ list allocations',()=>{
 assert.match(code,/readonly List<\(double Key,int Index\)> _gradientStopOrder=new List<\(double Key,int Index\)>\(\)/);
 assert.match(end,/_gradientStopOrder\.Clear\(\)/);
 assert.match(end,/_gradientStopOrder\.Add\(\(gradient\.Stops\[i\]\.Key,i\)\)/);
 assert.match(end,/var order=a\.Key\.CompareTo\(b\.Key\);return order!=0\?order:a\.Index\.CompareTo\(b\.Index\)/);
 assert.match(end,/var stop=gradient\.Stops\[_gradientStopOrder\[i\]\.Index\]/);
 assert.doesNotMatch(end,/gradient\.Stops\.OrderBy\(stop=>stop\.Key\)\.ToList\(\)/);
});
