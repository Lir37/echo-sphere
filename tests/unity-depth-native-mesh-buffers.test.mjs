import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const code=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDepth.cs','utf8');
const render=code.split('internal Texture Render(')[1].split('private void ReleaseTargets(')[0];

test('preserve-3d renders original polygon fan using reusable native mesh buffers',()=>{
 assert.match(code,/readonly List<Vector3> _meshVertices=new List<Vector3>\(\)/);
 assert.match(code,/readonly List<Vector2> _meshUvs=new List<Vector2>\(\)/);
 assert.match(code,/readonly List<int> _meshTriangles=new List<int>\(\)/);
 assert.match(render,/mesh\.SetVertices\(_meshVertices\);mesh\.SetUVs\(0,_meshUvs\);mesh\.SetTriangles\(_meshTriangles,0\)/);
 assert.match(render,/_meshTriangles\.Add\(0\);_meshTriangles\.Add\(i\);_meshTriangles\.Add\(i\+1\)/);
 assert.doesNotMatch(render,/mesh\.vertices=|mesh\.uv=|mesh\.triangles=|new List<int>\(\)/);
});

test('preserve-3d retains ordered lists and independent per-mesh material blocks',()=>{
 assert.match(render,/var paintOrder=_paintOrder;paintOrder\.Clear\(\)/);
 assert.match(render,/var ordered=_ordered;ordered\.Clear\(\)/);
 assert.match(render,/if\(preserveDepth\)Traverse\(tree,/);
 assert.match(render,/else ordered\.AddRange\(paintOrder\)/);
 assert.match(render,/_meshProperties\.Add\(new MaterialPropertyBlock\(\)\)/);
 assert.match(render,/var properties=_meshProperties\[meshIndex\]/);
 assert.match(render,/properties\.Clear\(\)/);
 assert.match(render,/_commands\.DrawMesh\(mesh,Matrix4x4\.identity,_material/);
});
