import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceCanvasSurface.cs','utf8');
test('Canvas shader properties use stable Unity native IDs at runtime',()=>{
 for(const [name,property] of Object.entries({ImageTexId:'_ImageTex',ImageTexelSizeId:'_ImageTex_TexelSize',PaintsId:'_Paints',ShadowSigmaId:'_SourceShadowSigma',ShadowDirectionId:'_SourceShadowDirection',ShadowColorId:'_SourceShadowColor',ShadowOffsetId:'_SourceShadowOffset',ShadowRectId:'_SourceShadowRect'})){
  assert.ok(s.includes('int '+name+'=Shader.PropertyToID("'+property+'")'));
 }
 for(const name of ['ImageTexId','ImageTexelSizeId','ShadowSigmaId','ShadowDirectionId','ShadowColorId','ShadowOffsetId','ShadowRectId']) assert.ok(s.includes('('+name+','));
 assert.ok(s.includes('_material.SetTexture(PaintsId,'));
});
test('Canvas profiling distinguishes path tessellation, shadow commands and GPU command execution',()=>{
 for(const [field,label] of [['CanvasTessellationMarker','EchoSphere.Canvas.Tessellate'],['CanvasShadowCommandsMarker','EchoSphere.Canvas.ShadowCommands'],['CanvasExecuteMarker','EchoSphere.Canvas.ExecuteCommands']]){
  assert.ok(s.includes(field+'=new ProfilerMarker("'+label+'")'));
  assert.ok(s.includes(field+'.Auto()'));
 }
 assert.match(s,/VectorUtils\.TessellateScene\(scene,options\)/);
 assert.match(s,/Graphics\.ExecuteCommandBuffer\(_commands\)/);
});
