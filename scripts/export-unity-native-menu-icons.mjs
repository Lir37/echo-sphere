import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import assert from 'node:assert/strict';

// Existing locked Lucide source geometry, imported once by Unity as VectorImage.
// White strokes receive native USS tint, including hover. No runtime SVG parser.
const icons=JSON.parse(readFileSync('UnityProject/Assets/Resources/EchoSphere/SourceUiIcons.json','utf8'));
const directory='UnityProject/Assets/Resources/EchoSphere/NativeMenuIcons';
const escape=value=>String(value).replaceAll('&','&amp;').replaceAll('"','&quot;').replaceAll('<','&lt;');
const names={strokeWidth:'stroke-width',strokeLinecap:'stroke-linecap',strokeLinejoin:'stroke-linejoin',className:'class'};
function element(node){
 const properties={...node.properties};delete properties.className;
 if(node.tag==='svg')properties.stroke='#ffffff';
 return `<${node.tag}${Object.entries(properties).map(([name,value])=>` ${names[name]??name}="${escape(value)}"`).join('')}>${node.children.map(element).join('')}</${node.tag}>`;
}
if(!process.argv.includes('--check'))mkdirSync(directory,{recursive:true});
for(const name of ['UserRound','Store','Trophy','Award','BookOpen','Settings']){
 const expected=element(icons[name])+'\n',file=directory+'/'+name+'.svg';
 if(process.argv.includes('--check'))assert.equal(readFileSync(file,'utf8').replaceAll('\r\n','\n'),expected,'Native icon drift: '+name);
 else writeFileSync(file,expected);
}
console.log('Native menu icons: 6 source geometries '+(process.argv.includes('--check')?'verified':'exported'));
