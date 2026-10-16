import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const source=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
const depth=source.split('private void RenderDepthLayer(')[1].split('internal static void Quad(')[0];
test('each retained UI depth root pools its own plane records',()=>{
 assert.match(source,/readonly List<SourceUiDepth\.PlaneImage> DepthImagePool=new List<SourceUiDepth\.PlaneImage>\(\)/);
 assert.match(source,/if\(DepthImageCursor==DepthImagePool\.Count\)DepthImagePool\.Add\(new SourceUiDepth\.PlaneImage\(\)\)/);
 assert.match(depth,/images\.Clear\(\);root\.DepthImageCursor=0/);
 assert.match(depth,/image\.Texture=texture;image\.Rect=rect;image\.Matrix=matrix;image\.Opacity=opacity/);
 assert.match(depth,/image\.Clip=mask;image\.ClipRect=clipRect;image\.Blend=blend;images\.Add\(image\)/);
 assert.match(depth,/Append\(entry\.DepthTexture,entry\.DepthRect,matrix,opacity,clip,_sheet\.Get\(node,"mix-blend-mode","normal"\)\)/);
 assert.match(depth,/Append\(entry\.Paint,entry\.PaintRect,matrix,opacity,node==root\.Node\?null:clip,_sheet\.Get\(node,"mix-blend-mode","normal"\)\)/);
 assert.doesNotMatch(depth,/new SourceUiDepth\.PlaneImage \{/);
});
