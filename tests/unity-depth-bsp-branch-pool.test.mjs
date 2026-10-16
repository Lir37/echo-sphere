import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const code=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDepth.cs','utf8');
const rent=code.split('private Branch RentBranch(')[1].split('private Polygon RentPolygon(')[0];
const add=code.split('private void Add(ref Branch branch,Polygon polygon)')[1].split('private static void Traverse(')[0];
const render=code.split('internal Texture Render(')[1].split('private void ReleaseTargets(')[0];
test('preserve-3d pool reinitializes each BSP branch before reuse',()=>{
 assert.match(code,/readonly List<Branch> _branchPool=new List<Branch>\(\)/);
 assert.match(rent,/branch\.Normal=normal;branch\.Position=position/);
 assert.match(rent,/branch\.Front=null;branch\.Back=null;branch\.Coplanar\.Clear\(\)/);
 assert.match(render,/_pointCursor=0;_polygonCursor=0;_branchCursor=0/);
 assert.match(add,/RentBranch\(normal\.normalized,polygon\.Points\[0\]\.Position\)/);
 assert.match(add,/branch\.Coplanar\.Add\(polygon\)/);
 assert.doesNotMatch(add,/new Branch \{/);
});
