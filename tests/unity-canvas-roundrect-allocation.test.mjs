import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
const start=s.indexOf('private void RoundRectangle('), end=s.indexOf('private void AddQuadCorner(');
const method=s.slice(start,end);
test('Canvas roundRect reads one to four source radii without allocating a list',()=>{
 assert.match(method,/var radiusList=radii\.Raw is List<V> \? radii\.Items : null;/);
 assert.match(method,/if\(count==0\|\|count>4\)throw new ArgumentOutOfRangeException\(nameof\(radii\)\)/);
 assert.match(method,/var a=radiusList==null\?\(float\)radii\.Number:\(float\)radiusList\[0\]\.Number;/);
 assert.match(method,/var b=count>1\?\(float\)radiusList\[1\]\.Number:a;/);
 assert.match(method,/var c=count>2\?\(float\)radiusList\[2\]\.Number:a;/);
 assert.match(method,/var d=count>3\?\(float\)radiusList\[3\]\.Number:b;/);
 assert.doesNotMatch(method,/new List<float>\(|RadiusAt\(/);
});
test('Canvas roundRect retains dimension normalization and ellipse path order',()=>{
 assert.match(method,/if\(width<0f\)/);
 assert.match(method,/if\(height<0f\)/);
 assert.match(method,/var shrink=Mathf\.Min\(1f,width\/Mathf\.Max\(a\+b,c\+d\),height\/Mathf\.Max\(a\+d,b\+c\)\)/);
 assert.match(method,/Ellipse\(x\+width-b,y\+b,b,b/);
 assert.match(method,/Ellipse\(x\+width-c,y\+height-c,c,c/);
});
