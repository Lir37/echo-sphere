import assert from 'node:assert/strict';
import {readFileSync,readdirSync,writeFileSync} from 'node:fs';
import {createHash} from 'node:crypto';

const manifestPath='tools/EchoSphere.VisualChecks/source-reference-manifest.json';
const hash=file=>{const bytes=readFileSync(file);return createHash('sha256').update(/\.(?:gz|png)$/.test(file)?bytes:bytes.toString('utf8').replaceAll('\r\n','\n')).digest('hex');};
function files(directory){return readdirSync(directory,{withFileTypes:true}).flatMap(entry=>entry.isDirectory()?files(directory+'/'+entry.name):[directory+'/'+entry.name]);}
const inputs=[...files('src').filter(file=>/\.(?:tsx?|css|js)$/.test(file)),...readdirSync('scripts').filter(file=>/^(?:export-unity-(?:visual|ui|css|animation|transition|paint|canvas|control)|source-(?:ui-view|css-browser))/.test(file)).map(file=>'scripts/'+file),'package-lock.json','tailwind.config.js','postcss.config.js'];
const outputs=[...files('tools/EchoSphere.VisualChecks').filter(file=>/\/source-(?!reference-manifest).*\.(?:json|gz)$/.test(file)&&!file.includes('/bin/')&&!file.includes('/obj/')),'UnityProject/Assets/Resources/EchoSphere/SourceUiIcons.json'];
if(process.argv.includes('--record'))writeFileSync(manifestPath,JSON.stringify({referenceCommit:'737ef2af4a877e0995db5f92331d536d2f29a934',referenceBranch:'new-desing',runtime:'Original locked TypeScript 5.6.3 / React 18.3.1; browser references from Edge 155.0.4283.45',inputs:Object.fromEntries(inputs.sort().map(file=>[file,hash(file)])),outputs:Object.fromEntries(outputs.sort().map(file=>[file,hash(file)]))},null,2)+'\n');
const manifest=JSON.parse(readFileSync(manifestPath,'utf8'));
for(const [file,expected]of Object.entries({...manifest.inputs,...manifest.outputs}))assert.equal(hash(file),expected,'Source reference provenance changed: '+file);
for(const file of [...inputs,...outputs])assert.ok(file in manifest.inputs||file in manifest.outputs,'Unrecorded source reference: '+file);

const presentation='UnityProject/Assets/EchoSphere/Scripts/Presentation';
const code=files('UnityProject/Assets/EchoSphere/Scripts').filter(file=>file.endsWith('.cs')).map(file=>({file,text:readFileSync(file,'utf8')}));
const generated=new Set(files(presentation+'/Generated').filter(file=>file.endsWith('.cs')).flatMap(file=>[...readFileSync(file,'utf8').matchAll(/\bclass\s+(Visual_[A-Za-z0-9_]+)/g)].map(match=>match[1])));
for(const {file,text}of code)for(const match of text.matchAll(/\b(Visual_[A-Za-z0-9_]+)\.Scope\b/g))assert.ok(generated.has(match[1]),'Unresolved generated presentation module '+match[1]+' in '+file);
const runtime=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereRuntime.cs','utf8');
assert.ok(!/\bvoid\s+OnGUI\s*\(/.test(runtime),'Legacy migration GUI must not overlay the authored source UI');
const bridge=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourcePresentation.cs','utf8');
assert.ok(bridge.includes('Visual_renderer.Scope["render"].Call')&&bridge.includes('DrawSourceDocument();'),'The source renderer and source UI must both be on the active frame path');
assert.ok(bridge.includes('renderer.enabled=false'),'Legacy native visual approximations must stay disabled');
assert.ok(!code.filter(item=>item.file.includes('/Presentation/')).some(item=>/WebView|Jint|ClearScript|UnityEditor/.test(item.text)),'Source presentation must run as native runtime C#');
console.log(`Visual reference provenance and native renderer wiring: PASS (${generated.size} translated modules, ${Object.keys(manifest.outputs).length} reference outputs).`);
