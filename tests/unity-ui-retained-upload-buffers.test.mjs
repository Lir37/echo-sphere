import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const code=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('UI filters reuse an entry-owned empty collection',()=>{
 assert.match(code,/readonly List<FilterFunction> EmptyFilters=new List<FilterFunction>\(\)/);
 assert.match(code,/else element\.style\.filter=entry\.EmptyFilters;/);
 assert.doesNotMatch(code,/else element\.style\.filter=new List<FilterFunction>\(\)/);
});
test('CSS raster keeps the original flipped pixels but reuses the upload buffer',()=>{
 assert.match(code,/public byte\[\] PaintUploadPixels;/);
 assert.match(code,/entry\.PaintUploadPixels==null\|\|entry\.PaintUploadPixels\.Length!=raster\.Pixels\.Length/);
 assert.match(code,/entry\.PaintUploadPixels=new byte\[raster\.Pixels\.Length\]/);
 assert.match(code,/Buffer\.BlockCopy\(raster\.Pixels,y\*stride,entry\.PaintUploadPixels,\(raster\.Height-y-1\)\*stride,stride\)/);
 assert.match(code,/entry\.Paint\.LoadRawTextureData\(entry\.PaintUploadPixels\)/);
 assert.doesNotMatch(code,/var pixels=new byte\[raster\.Pixels\.Length\]/);
});
