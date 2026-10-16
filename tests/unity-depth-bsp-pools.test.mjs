import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const code=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDepth.cs','utf8');
const add=code.split('private void Add(ref Branch branch,Polygon polygon)')[1].split('private static void Traverse(')[0];
const render=code.split('internal Texture Render(')[1].split('private void ReleaseTargets(')[0];
test('BSP depth renderer reuses split-point and polygon objects without cross-frame aliases',()=>{
 assert.match(code,/readonly List<List<Point>> _pointPool=new List<List<Point>>\(\)/);
 assert.match(code,/readonly List<Polygon> _polygonPool=new List<Polygon>\(\)/);
 assert.match(code,/var points=_pointPool\[_pointCursor\+\+\];points\.Clear\(\);return points/);
 assert.match(code,/var polygon=_polygonPool\[_polygonCursor\+\+\];polygon\.Points=points;polygon\.Image=image;return polygon/);
 assert.match(render,/_pointCursor=0;_polygonCursor=0/);
 assert.match(add,/var front=RentPoints\(\);var back=RentPoints\(\)/);
 assert.match(add,/if\(hasFront\)Add\(ref branch\.Front,RentPolygon\(front,polygon\.Image\)\)/);
 assert.match(add,/if\(hasBack\)Add\(ref branch\.Back,RentPolygon\(back,polygon\.Image\)\)/);
});
test('depth rectangles retain authored four corner and UV order without allocating two arrays',()=>{
 assert.match(code,/UnitUvs=\{new Vector2\(0,1\),new Vector2\(1,1\),new Vector2\(1,0\),new Vector2\(0,0\)\}/);
 assert.match(render,/index==0\?new Vector2\(r\.xMin,r\.yMin\):index==1\?new Vector2\(r\.xMax,r\.yMin\):index==2\?new Vector2\(r\.xMax,r\.yMax\):new Vector2\(r\.xMin,r\.yMax\)/);
 assert.match(render,/points\.Add\(new Point \{Position=p,Uv=UnitUvs\[index\]\}\)/);
 assert.doesNotMatch(render,/var positions=new\[\]|var uvs=new\[\]|var points=new List<Point>\(\)/);
});
