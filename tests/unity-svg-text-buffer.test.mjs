import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const svg=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiSvg.cs','utf8');
const doc=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiDocument.cs','utf8');
test('SVG text collection reuses its retained entry scratch list',()=>{
  assert.match(doc,/readonly List<VisualCssNode> SvgTextNodes=new List<VisualCssNode>\(\)/);
  assert.equal((doc.match(/SourceUiSvg\.Text\(node,_sheet,entry\.Element,entry\.SvgTexts,entry\.SvgTextNodes\)/g)||[]).length,2);
  assert.match(svg,/List<Label> labels,List<VisualCssNode> texts/);
  assert.match(svg,/texts\.Clear\(\);void Collect\(VisualCssNode node\)/);
  assert.doesNotMatch(svg,/var texts=new List<VisualCssNode>\(\)/);
});
