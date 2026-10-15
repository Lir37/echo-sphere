// Browser reference for every authored animation declaration, including dormant
// CSS rules. The keyframe data and shorthand declarations come from the source.
const fs=require('node:fs');
const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const styles=JSON.parse(fs.readFileSync('UnityProject/Assets/Resources/EchoSphere/SourceUiStyles.json','utf8'));
const output='tools/EchoSphere.VisualChecks/source-animation-fixtures.json';
async function main(){
 const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});
 try{
  const page=await browser.newPage({viewport:{width:390,height:844}});
  const keyframes=Object.entries(styles.keyframes).map(([name,frames])=>'@keyframes '+name+'{'+frames.map(frame=>frame.offset+'{'+frame.declarations.map(d=>d.property+':'+d.value+';').join('')+'}').join('')+'}').join('\n');
  await page.setContent('<html><head></head><body></body></html>');await page.addStyleTag({content:keyframes});
  const rules=styles.rules.filter(rule=>rule.declarations.some(d=>d.property==='animation'||d.property==='animation-name'));
  const fixtures=[];
  for(const rule of rules){
   const data=await page.evaluate(({declarations})=>{
    document.body.replaceChildren();const element=document.createElement('div');document.body.append(element);
    for(const d of declarations)element.style.setProperty(d.property,d.value,d.important?'important':'');
    element.style.setProperty('width','240px','important');element.style.setProperty('height','160px','important');
    const clone=element.cloneNode();clone.style.setProperty('animation','none','important');document.body.append(clone);
    const read=el=>{const s=getComputedStyle(el);return{transform:s.transform,opacity:s.opacity,filter:s.filter,matrix:Array.from(new DOMMatrixReadOnly(s.transform==='none'?undefined:s.transform).toFloat64Array())};};
    const base=read(clone);clone.remove();const animations=element.getAnimations();for(const a of animations){a.pause();a.currentTime=0;}
    const samples=[0,.031,.17,.53,1.37,7.91,64.13].map(time=>{for(const a of animations)a.currentTime=time*1000;return{time,...read(element)};});
    return{base,samples,width:element.offsetWidth,height:element.offsetHeight};
   },rule);
   fixtures.push({label:rule.selector,declarations:rule.declarations,...data});
  }
  const text=JSON.stringify(fixtures,null,2)+'\n';if(process.argv.includes('--check')){if(fs.readFileSync(output,'utf8')!==text)throw Error('Browser animation fixture changed');}else fs.writeFileSync(output,text);
  console.log(`Browser keyframe references: ${fixtures.length} declarations / ${fixtures.reduce((n,x)=>n+x.samples.length,0)} timeline samples.`);
 }finally{await browser.close();}
}
main().catch(error=>{console.error(error);process.exitCode=1;});
