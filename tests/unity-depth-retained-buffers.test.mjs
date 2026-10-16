import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const source=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
const clip=source.split('private Texture2D DepthClip(Entry entry)')[1].split('private void RenderDepthLayer(')[0];
const depth=source.split('private void RenderDepthLayer(')[1].split('internal static void Quad(')[0];
test('depth clipping reuses its flipped RGBA upload buffer',()=>{
 assert.match(source,/public byte\[\] DepthClipUploadPixels;/);
 assert.match(clip,/entry\.DepthClipUploadPixels==null\|\|entry\.DepthClipUploadPixels\.Length!=raster\.Pixels\.Length/);
 assert.match(clip,/Buffer\.BlockCopy\(raster\.Pixels,y\*stride,entry\.DepthClipUploadPixels,\(raster\.Height-y-1\)\*stride,stride\)/);
 assert.match(clip,/entry\.GroupClip\.LoadRawTextureData\(entry\.DepthClipUploadPixels\)/);
 assert.doesNotMatch(clip,/var pixels=new byte/);
});
test('depth layer uses a per-root cleared plane buffer and keeps rendering order',()=>{
 assert.match(source,/readonly List<SourceUiDepth\.PlaneImage> DepthImages=new List<SourceUiDepth\.PlaneImage>\(\)/);
 assert.match(depth,/var images=root\.DepthImages;images\.Clear\(\)/);
 assert.match(depth,/Add\(root\.Node,Matrix4x4\.identity,1f\)/);
 assert.match(depth,/root\.Depth\.Render\(images,/);
 assert.doesNotMatch(depth,/var images=new List<SourceUiDepth\.PlaneImage>\(\)/);
});
