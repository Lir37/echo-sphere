import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('source UI no longer uses unverified first-layout visibility gate',()=>{
 assert.doesNotMatch(s,/RevealAfterFirstLayout|_firstLayoutVisible|_firstLayoutStablePasses/);
 assert.doesNotMatch(s,/_document\.rootVisualElement\.style\.visibility=Visibility\.Hidden/);
 assert.match(s,/entry\.Frame\.RegisterCallback<GeometryChangedEvent>/);
 assert.match(s,/using\(UiLayoutMarker\.Auto\(\)\)UpdateLayout\(_root\)/);
});
