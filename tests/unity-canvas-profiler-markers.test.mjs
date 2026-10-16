import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
test('source Canvas exposes distinct native profiling markers',()=>{
 for(const name of ['CanvasEndMarker','CanvasTextMarker','CanvasMeshMarker','CanvasGradientMarker','CanvasDrawMarker','CanvasPresentMarker']){
  assert.match(s,new RegExp('ProfilerMarker '+name+'=new ProfilerMarker\\('));
  assert.match(s,new RegExp(name+'\\.Auto\\(\\)'));
 }
 assert.match(s,/using Unity\.Profiling;/);
});
test('source Canvas retains original shadow RT IDs as static shader properties',()=>{
 for(const [field,value] of [['ShadowMaskId','_SourceShadowMask'],['ShadowPingId','_SourceShadowPing'],['ShadowBlurId','_SourceShadowBlur']]){
  assert.ok(s.includes('int '+field+'=Shader.PropertyToID("'+value+'")'));
 }
 assert.match(s,/var mask=ShadowMaskId;var ping=ShadowPingId;var blur=ShadowBlurId/);
 assert.match(s,/_commands\.GetTemporaryRT\(mask,/);
 assert.match(s,/_commands\.ReleaseTemporaryRT\(mask\)/);
});
