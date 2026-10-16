import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const svg=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiSvg.cs','utf8');
const doc=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('SVG rasterization preserves native RenderTexture handle at matching dimensions',()=>{
 assert.match(svg,/RenderTexture reuse=null/);
 assert.match(svg,/reuse!=null&&reuse\.width==pixelsWide&&reuse\.height==pixelsHigh\?reuse:new RenderTexture/);
 assert.match(svg,/if\(!result\.IsCreated\(\)\)result\.Create\(\)/);
 assert.match(svg,/Graphics\.Blit\(raw,result,material,1\)/);
 assert.match(svg,/if\(result!=reuse\)\{result\.Release\(\);UnityEngine\.Object\.Destroy\(result\);\}/);
});
test('SVG UI entry destroys its old handle only when a new one was required',()=>{
 assert.match(doc,/var previous=entry\.Svg;var updated=SourceUiSvg\.Rasterize\(/);
 assert.match(doc,/out entry\.SvgRect,previous as RenderTexture/);
 assert.match(doc,/if\(previous!=null&&previous!=updated\)Destroy\(previous\)/);
 assert.match(doc,/entry\.Svg=updated;entry\.SvgKey=key/);
});
