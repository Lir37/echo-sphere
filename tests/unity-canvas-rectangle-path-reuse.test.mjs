import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const canvas = readFileSync(new URL('../UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs', import.meta.url), 'utf8');
assert.match(canvas, /private readonly List<Contour> _rectSavedPath=new List<Contour>\(\);/);
assert.match(canvas, /case "fillRect":case "strokeRect":\s*_rectSavedPath\.Clear\(\);_rectSavedPath\.AddRange\(_path\);/);
assert.match(canvas, /try\{Rectangle\(N\(0\),N\(1\),N\(2\),N\(3\)\);Paint\(name=="strokeRect",false\);\}/);
assert.match(canvas, /finally\{_path\.Clear\(\);_path\.AddRange\(_rectSavedPath\);_rectSavedPath\.Clear\(\);_contour=oldContour;\}/);
assert.doesNotMatch(canvas, /new List<Contour>\(_path\)/);
console.log('Canvas rectangle path scratch reuse: PASS');
