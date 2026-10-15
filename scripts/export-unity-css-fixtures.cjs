const fs=require('node:fs'),path=require('node:path');
const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const stylePath='UnityProject/Assets/Resources/EchoSphere/SourceUiStyles.json';
const treePath='tools/EchoSphere.VisualChecks/source-ui-fixtures.json';
const output='tools/EchoSphere.VisualChecks/source-css-fixtures.json.gz';
const styles=JSON.parse(fs.readFileSync(stylePath,'utf8'));
const trees=JSON.parse(fs.readFileSync(treePath,'utf8'));
const properties=['display','position','color','font-size','font-weight','line-height','letter-spacing','text-align','text-transform','white-space','opacity','border-top-color','border-right-color','border-bottom-color','border-left-color','border-top-width','border-right-width','border-bottom-width','border-left-width','border-top-style','border-top-left-radius','border-top-right-radius','border-bottom-left-radius','border-bottom-right-radius','background-color','visibility','pointer-events','user-select','overflow-wrap','text-shadow'];
const css=styles.rules.map(rule=>{let text=rule.selector+'{'+rule.declarations.map(declaration=>declaration.property+':'+declaration.value+(declaration.important?'!important':'')+';').join('')+'}';for(const condition of [...rule.conditions].reverse())text='@'+condition.kind+' '+condition.value+'{'+text+'}';return text;}).join('\n')+'\n'+Object.entries(styles.keyframes).map(([name,frames])=>'@keyframes '+name+'{'+frames.map(frame=>frame.offset+'{'+frame.declarations.map(declaration=>declaration.property+':'+declaration.value+';').join('')+'}').join('')+'}').join('\n');
async function main(){
 const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});const fixtures=[];
 try{
  for(const viewport of [{width:390,height:844},{width:1280,height:720},{width:360,height:520}]){
   const page=await browser.newPage({viewport,deviceScaleFactor:1});await page.setContent('<html><head></head><body><div id="root"><div class="es-app min-h-screen w-full text-[#dcecff] overflow-hidden flex items-center justify-center"></div></div></body></html>');await page.addStyleTag({content:css});
   for(const fixture of trees){
    const nodes=await page.evaluate(({tree,properties})=>{
     const root=document.querySelector('#root>.es-app');root.replaceChildren();
     const unitless=new Set(['opacity','fontWeight','zIndex','flex','flexGrow','flexShrink','order','lineHeight']);
     function append(value,parent,svg=false){if(value==null||typeof value==='boolean')return;if(Array.isArray(value)){value.forEach(child=>append(child,parent,svg));return;}if(typeof value!=='object'){parent.append(document.createTextNode(String(value)));return;}if(value.tag==='fragment'){value.children.forEach(child=>append(child,parent,svg));return;}
      const isSvg=svg||value.tag==='svg';const element=isSvg?document.createElementNS('http://www.w3.org/2000/svg',value.tag):document.createElement(value.tag);
      for(const[key,attribute]of Object.entries(value.properties||{})){if(key.startsWith('on')||key==='ref'||key==='children'||attribute==null||attribute==='$function')continue;if(key==='style'){for(const[property,content]of Object.entries(attribute)){const name=property.replace(/[A-Z]/g,letter=>'-'+letter.toLowerCase());element.style.setProperty(name,typeof content==='number'&&!property.startsWith('--')&&!unitless.has(property)?content+'px':String(content));}}else if(key==='className')element.setAttribute('class',attribute);else if(key==='disabled'){if(attribute)element.setAttribute('disabled','');}else element.setAttribute(isSvg&&key!=='viewBox'?key.replace(/[A-Z]/g,letter=>'-'+letter.toLowerCase()):key,String(attribute));}
      value.children.forEach(child=>append(child,element,isSvg));parent.append(element);
     }
     append(tree,root);for(const animation of document.getAnimations()){animation.pause();animation.currentTime=0;}
     const nodes=[];function visit(node,path){if(node.nodeType!==1)return;const style=getComputedStyle(node);const rect=node.getBoundingClientRect();nodes.push({path,tag:node.tagName.toLowerCase(),rect:{x:rect.x,y:rect.y,width:rect.width,height:rect.height},layout:{width:parseFloat(style.width)||0,height:parseFloat(style.height)||0},style:Object.fromEntries(properties.map(property=>[property,style.getPropertyValue(property)]))});for(const pseudo of ['::before','::after']){const computed=getComputedStyle(node,pseudo);if(!['none','normal'].includes(computed.content))nodes.push({path:path+pseudo,tag:node.tagName.toLowerCase(),rect:{x:0,y:0,width:0,height:0},style:Object.fromEntries(properties.map(property=>[property,computed.getPropertyValue(property)]))});}Array.from(node.childNodes).filter(child=>child.nodeName!=='HEAD').forEach((child,index)=>visit(child,path+'/'+index));}visit(document.documentElement,'0');function pathOf(element){if(!element)return null;if(element===document.documentElement)return '0';return pathOf(element.parentNode)+'/'+Array.from(element.parentNode.childNodes).filter(child=>child.nodeName!=='HEAD').indexOf(element);}const paths=new Map();function index(element){paths.set(pathOf(element),element);for(const child of element.children)if(child.tagName!=='HEAD')index(child);}index(document.documentElement);for(const node of nodes)if(paths.has(node.path))node.offsetParent=pathOf(paths.get(node.path).offsetParent);return nodes;
    },{tree:fixture.tree,properties});fixtures.push({label:fixture.label+'/'+viewport.width+'x'+viewport.height,ui:fixture.label,viewport,nodes});
   }
   await page.close();
  }
  const data=require('node:zlib').gzipSync(JSON.stringify(fixtures)+'\n');if(process.argv.includes('--check')){if(!fs.readFileSync(output).equals(data))throw new Error('Source computed CSS changed');}else fs.writeFileSync(output,data);
  console.log(`Browser-authored CSS reference: ${fixtures.length} screens / viewport combinations; ${fixtures.reduce((sum,fixture)=>sum+fixture.nodes.length,0)} computed element styles.`);
 }finally{await browser.close();}
}
main().catch(error=>{console.error(error);process.exitCode=1;});
