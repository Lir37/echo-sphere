import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const source=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
test('native Canvas Quad emits corners in original source order without temporary arrays',()=>{
 const quad=source.slice(source.indexOf('private void Quad('),source.indexOf('private void Image('));
 assert.match(quad,/AddCorner\(a,ua\);AddCorner\(b,ub\);AddCorner\(c,uc\);AddCorner\(d,ud\);/);
 assert.match(quad,/_triangles\.Add\(first\);_triangles\.Add\(first\+1\);_triangles\.Add\(first\+2\);/);
 assert.doesNotMatch(quad,/new\[\]\s*\{/);
});
test('odd Canvas dash arrays repeat their original values without a temporary list',()=>{
 assert.match(source,/if\(dash\.Count%2==1\)\{var count=dash\.Count;for\(var i=0;i<count;i\+\+\)dash\.Add\(dash\[i\]\);\}/);
 assert.doesNotMatch(source,/new List<float>\(dash\)/);
});
