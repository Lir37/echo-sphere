import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const source=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
const method=source.slice(source.indexOf('private void EnsureBrush('),source.indexOf('private void PrepareSvg('));
test('UI CSS brush keeps the same native texture handle for same sized raster',()=>{
 assert.match(method,/if\(entry\.Paint==null\|\|entry\.Paint\.width!=raster\.Width\|\|entry\.Paint\.height!=raster\.Height\)/);
 assert.match(method,/entry\.Paint\.LoadRawTextureData\(entry\.PaintUploadPixels\)/);
 assert.match(method,/entry\.Paint\.Apply\(false,false\)/);
 assert.doesNotMatch(method,/entry\.Paint\.Apply\(false,true\)/);
});
