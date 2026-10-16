import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const source=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('first menu source UI separates tree, reconcile and layout profiling',()=>{
 for(const [name,description] of [['UiBuildMarker','EchoSphere.UI.BuildTree'],['UiReconcileMarker','EchoSphere.UI.Reconcile'],['UiLayoutMarker','EchoSphere.UI.Layout']]){
  assert.ok(source.includes(name+'=new ProfilerMarker("'+description+'")'));
  assert.ok(source.includes(name+'.Auto()'));
 }
 assert.match(source,/using\(UiBuildMarker\.Auto\(\)\)\{var tree=Render\(\);nextRoot=VisualCssNode\.Build\(tree\);CopyInteraction\(_root,nextRoot\);_root=nextRoot;_sheet\.Apply\(_root\);\}/);
 assert.match(source,/using\(UiReconcileMarker\.Auto\(\)\)Reconcile\(_root,_document\.rootVisualElement,"root"\)/);
 assert.match(source,/using\(UiLayoutMarker\.Auto\(\)\)UpdateLayout\(_root\);PreparePaintResources\(_root\);_layout\.Tracks\(_root\)/);
});
