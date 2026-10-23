import assert from 'node:assert/strict';
import {readFileSync,readdirSync,writeFileSync} from 'node:fs';
import {createHash} from 'node:crypto';

const manifestPath='tools/EchoSphere.VisualChecks/source-reference-manifest.json';
const hash=file=>{const bytes=readFileSync(file);return createHash('sha256').update(/\.(?:gz|png)$/.test(file)?bytes:bytes.toString('utf8').replaceAll('\r\n','\n')).digest('hex');};
function files(directory){return readdirSync(directory,{withFileTypes:true}).flatMap(entry=>entry.isDirectory()?files(directory+'/'+entry.name):[directory+'/'+entry.name]);}
const inputs=[...files('src').filter(file=>/\.(?:tsx?|css|js)$/.test(file)),...readdirSync('scripts').filter(file=>/^(?:export-unity-(?:visual|ui|css|animation|transition|paint|canvas|control|backdrop)|source-(?:ui-view|css-browser))/.test(file)).map(file=>'scripts/'+file),'package-lock.json','tailwind.config.js','postcss.config.js'];
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
const nativeFrame=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeFrame.cs','utf8');
const nativeViews=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeWorldViews.cs','utf8');
const world=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeWorld.cs','utf8');
assert.ok(bridge.includes('RenderNativeWorldFrame(width,height);')&&world.includes('RenderNativeFrameContents(width,height);')&&!world.includes('Visual_renderer.Scope["render"].Call')&&nativeFrame.includes('DrawNativeViewVoidAmbient(w,h,time)')&&!nativeFrame.includes('params V[] args')&&!nativeViews.includes('switch(name)')&&nativeViews.includes('_nativeWorldField.Draw')&&nativeViews.includes('_nativeWorldAmbient.Draw')&&bridge.includes('DrawSourceDocument();'),'Native frame coordinator, native world layers and remaining UI must share the active ordered frame path');
assert.ok(bridge.includes('AddComponent<NativeWorldSurface>()')&&!bridge.includes('AddComponent<SourceCanvasSurface>()'),'Active world must use the native mesh compositor');
assert.ok(bridge.includes('_camera.cullingMask=0')&&!bridge.includes('FindObjectsByType<Renderer>'),'The source UI texture must replace scene rendering without a full-scene Renderer scan every frame');
assert.ok(!code.filter(item=>item.file.includes('/Presentation/')).some(item=>/WebView|Jint|ClearScript|UnityEditor/.test(item.text)),'Source presentation must run as native runtime C#');
const uiBridge=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceUiBridge.cs','utf8');
const runOwner=readFileSync('UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereNativeRunDocument.cs','utf8');
const runDocument=readFileSync(presentation+'/NativeRunDocument.cs','utf8');
assert.ok(uiBridge.includes('new NativeUiDataContext()')&&!uiBridge.includes('AddComponent<SourceUiDocument>')&&uiBridge.includes('DrawNativeRunDocument();')&&!uiBridge.includes('SourceUiTree(')&&!uiBridge.includes('_sourceUi.Draw('),'Production UI must not build legacy JSX/DOM trees');
assert.ok(runOwner.includes('AddComponent<NativeRunDocument>()')&&runOwner.includes('PrepareNativeRunPresenters(anchors)')&&!/VisualDom|VisualCssNode/.test(runDocument),'Native run document must own retained UI hosts');
for(const item of code.filter(item=>item.file.includes('/Runtime/')))
  assert.ok(!/Visual_(?:renderer|App|i18n|canvasLocale|orbitalGeometry|gameData|sphereProgression|artifactSystem|engineProgression|characters|networkRuntime|networkRender|networkPreview|spaceCollision)\.Scope|SourceCanvasImage|VisualJs\.ImageFactory/.test(item.text),'Retired renderer/localization/image bridge in '+item.file);
for(const item of code.filter(item=>/\/Runtime\/EchoSphereNative[^/]*\.cs$/.test(item.file)))
  assert.ok(!/Visual_(?:gameData|sphereProgression|artifactSystem)\.Scope/.test(item.text),'Retired translated run catalog in '+item.file);
for(const name of ['NativeLocalizationCatalog','NativeWorldArtCatalog','NativeRunCatalog'])assert.ok(readFileSync('UnityProject/Assets/Resources/EchoSphere/'+name+'.asset','utf8').includes('SourceCommit: 737ef2af4a877e0995db5f92331d536d2f29a934'),'Native catalog provenance missing: '+name);
console.log(`Visual reference provenance and native renderer wiring: PASS (${generated.size} translated modules, ${Object.keys(manifest.outputs).length} reference outputs).`);
