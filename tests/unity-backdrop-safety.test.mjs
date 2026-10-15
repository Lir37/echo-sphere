import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

// A render-pass regression guard: native UI mesh generation must never submit
// capture/blit commands or upload textures. Rendering behavior is checked by the
// portable C# policy + independent browser pixel fixtures, not by this guard.
const base='UnityProject/Assets/EchoSphere/Scripts/Presentation/';
const code=name=>readFileSync(base+name,'utf8').replace(/\/\/[^\n]*|\/\*[\s\S]*?\*\//g,'');
test('backdrop composition cannot capture or allocate GPU textures during UI repaint',()=>{
 for(const name of ['SourceUiBackdrop.cs','SourceUiReplica.cs']){
  assert.doesNotMatch(code(name),/\b(?:Graphics|GL)\s*\.|\b(?:ImmediateModeElement|BuiltinRenderTextureType|CommandBuffer)\b|\bnew\s+(?:RenderTexture|Texture2D)\b|\b(?:ReadPixels|CaptureScreenshot)\s*\(/,name);
 }
});
test('source UI paint callback consumes prepared resources without texture uploads',()=>{
 const document=code('SourceUiDocument.cs');const start=document.indexOf('private void Paint(');const end=document.indexOf('private void PreparePaintResources(',start);
 assert.ok(start>0&&end>start);assert.doesNotMatch(document.slice(start,end),/\b(?:Graphics|GL|Resources)\s*\.|\b(?:Apply|Rasterize|EnsureBrush|PrepareDepth|PrepareSvg|ExecuteCommandBuffer)\s*\(/);
 const range=code('SourceUiRange.cs').split('internal static void Paint(')[1];assert.ok(range);assert.doesNotMatch(range,/\bResources\s*\./);
});
test('force-gamma UI filters linearize only the final attachment and preserve premultiplied RGB',()=>{
 for(const name of ['SourceUiBackdrop.shader','SourceUiFilters.shader']){
  const shader=readFileSync('UnityProject/Assets/Resources/EchoSphere/'+name,'utf8');
  assert.match(shader,/#if defined\(_UIE_OUTPUT_LINEAR\) && !defined\(UNITY_COLORSPACE_GAMMA\)/,name);
  assert.doesNotMatch(shader,/_CssWritesGamma|GammaToLinearSpace\([^)]*\.rgb\s*\//,name);
 }
});
