import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const ui=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
const depth=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDepth.cs','utf8');
test('Unity Profiler samples distinguish UI brush, depth and backdrop preparation',()=>{
 for(const name of ['UiBrushesMarker','UiDepthMarker','UiBackdropMarker','UiSvgMarker','UiCssMarker'])assert.match(ui,new RegExp('ProfilerMarker '+name+'=new ProfilerMarker\\('));
 for(const name of ['UiBrushesMarker','UiDepthMarker','UiBackdropMarker','UiSvgMarker','UiCssMarker'])assert.match(ui,new RegExp(name+'\\.Auto\\(\\)'));
 assert.match(depth,/ProfilerMarker DepthRenderMarker=new ProfilerMarker/);
 assert.match(depth,/using var depthSample=DepthRenderMarker\.Auto\(\)/);
});
