import {createRequire} from 'node:module';
import {readFileSync,writeFileSync,mkdirSync,readdirSync} from 'node:fs';
import {createHash} from 'node:crypto';
import {resolve,dirname} from 'node:path';
const output='UnityProject/Assets/Editor/EchoSphereReference/SourceUiStyles.json';
const manifestPath='UnityProject/Assets/EchoSphere/Scripts/Presentation/Generated/source-ui-style-manifest.json';
const hash=text=>createHash('sha256').update(text.replaceAll('\r\n','\n')).digest('hex');
if(process.argv.includes('--check')){
  const manifest=JSON.parse(readFileSync(manifestPath,'utf8'));
  for(const [file,digest] of Object.entries(manifest.sources))if(hash(readFileSync(file,'utf8'))!==digest)throw new Error('Source UI changed: '+file);
  if(hash(readFileSync(output,'utf8'))!==manifest.output)throw new Error('Source CSS export changed');console.log('Source CSS/utility/keyframe provenance: PASS');process.exit(0);
}
const require=createRequire(process.env.ECHO_UI_DEPENDENCIES?resolve(process.env.ECHO_UI_DEPENDENCIES,'package.json'):import.meta.url);
const postcss=require('postcss'),tailwind=require('tailwindcss');
const {default:config}=await import('../tailwind.config.js');
const css=readFileSync('src/index.css','utf8')+'\n'+readFileSync('src/conceptStyle.css','utf8');
const result=await postcss([tailwind(config)]).process(css,{from:resolve('src/conceptStyle.css')});
const rules=[],keyframes={};
function walk(nodes,conditions=[]) {
  for(const node of nodes) {
    if(node.type==='rule')rules.push({selector:node.selector,conditions,declarations:node.nodes.filter(n=>n.type==='decl').map(n=>({property:n.prop,value:n.value,important:!!n.important}))});
    else if(node.type==='atrule'&&node.name.endsWith('keyframes'))keyframes[node.params]=node.nodes.map(frame=>({offset:frame.selector,declarations:frame.nodes.filter(n=>n.type==='decl').map(n=>({property:n.prop,value:n.value}))}));
    else if(node.type==='atrule'&&node.nodes)walk(node.nodes,[...conditions,{kind:node.name,value:node.params}]);
  }
}
walk(result.root.nodes);const text=JSON.stringify({rules,keyframes})+'\n';mkdirSync(dirname(output),{recursive:true});writeFileSync(output,text);
function contentFiles(directory){return readdirSync(directory,{withFileTypes:true}).flatMap(entry=>entry.isDirectory()?contentFiles(directory+'/'+entry.name):/\.(?:[cm]?[jt]sx?|html|css)$/.test(entry.name)?[directory+'/'+entry.name]:[]);}
const sources=['src/index.css','src/conceptStyle.css','tailwind.config.js','postcss.config.js','package-lock.json','scripts/export-unity-ui-styles.mjs',...contentFiles('src'),...config.content.filter(x=>!x.includes('*'))];
writeFileSync(manifestPath,JSON.stringify({sources:Object.fromEntries([...new Set(sources)].map(file=>[file,hash(readFileSync(file,'utf8'))])),output:hash(text)},null,2)+'\n');
console.log(`Source UI CSS: ${rules.length} rules; ${Object.keys(keyframes).length} authored animation timelines.`);
