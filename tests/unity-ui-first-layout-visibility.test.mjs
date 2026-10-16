import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('source UI is initially hidden without disabling Yoga layout',()=>{
 assert.match(s,/_document\.rootVisualElement\.style\.visibility=Visibility\.Hidden;/);
 assert.doesNotMatch(s,/_document\.rootVisualElement\.style\.display=DisplayStyle\.None/);
 assert.match(s,/if\(!_firstLayoutVisible\)\{_firstChildLayoutSeen=true;_firstLayoutChanges\+\+;\}/);
});
test('source UI first reveal waits for initial geometry to settle',()=>{
 const block=s.slice(s.indexOf('private void RevealAfterFirstLayout('),s.indexOf('private void Reconcile('));
 assert.match(s,/if\(Render==null\)return;\s*RevealAfterFirstLayout\(\)/);
 assert.match(block,/if\(_firstLayoutVisible\|\|!_firstChildLayoutSeen\|\|_root==null\)return;/);
 assert.match(block,/_observedFirstLayoutChanges!=_firstLayoutChanges/);
 assert.match(block,/if\(\+\+_firstLayoutStablePasses<2\)return;/);
 assert.match(block,/_document\.rootVisualElement\.style\.visibility=Visibility\.Visible;/);
});
