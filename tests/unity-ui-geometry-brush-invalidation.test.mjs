import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('geometry callback does not force CSS brush recreation without fingerprint change',()=>{
 assert.match(s,/RegisterCallback<GeometryChangedEvent>\(evt=>\{UpdateBounds\(entry\.Node,entry\.Frame\);FrameVersion\+\+;\}\)/);
 assert.doesNotMatch(s,/GeometryChangedEvent>\(evt=>\{[^}]*PaintKey=null/);
 assert.match(s,/if\(entry\.PaintKey!=fingerprint\)/);
 assert.match(s,/node\.Width\.ToString\("R",CultureInfo\.InvariantCulture\)/);
 assert.match(s,/node\.Height\.ToString\("R",CultureInfo\.InvariantCulture\)/);
});
