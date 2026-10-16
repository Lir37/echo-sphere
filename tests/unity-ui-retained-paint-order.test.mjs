import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const s=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('UI paint-order reconciliation uses retained scratch without LINQ sorting',()=>{
 assert.match(s,/private readonly List<\(VisualElement Element,double Order,double Z,int Index\)> _paintOrderScratch=/);
 assert.match(s,/_paintOrderScratch\.Clear\(\)/);
 assert.match(s,/if\(!\(child\.Native is VisualElement element\)\)continue;/);
 assert.match(s,/_paintOrderScratch\.Add\(\(element,_sheet\.Length\(_sheet\.Get\(child,"order","0"\),1d\),_sheet\.Length\(_sheet\.Get\(child,"z-index","0"\),1d\),i\)\)/);
 assert.match(s,/var order=a\.Order\.CompareTo\(b\.Order\)/);
 assert.match(s,/var z=a\.Z\.CompareTo\(b\.Z\)/);
 assert.match(s,/a\.Index\.CompareTo\(b\.Index\)/);
 assert.match(s,/foreach\(var item in _paintOrderScratch\)item\.Element\.BringToFront\(\)/);
 assert.doesNotMatch(s,/node\.Children\.Where\(child=>child\.Native is VisualElement\)\.OrderBy\(/);
});
