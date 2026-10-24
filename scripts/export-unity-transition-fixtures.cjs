// Reference interpolation uses only values present in the authored stylesheet.
const fs=require('node:fs');
const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const output='tools/EchoSphere.VisualChecks/source-transition-fixtures.json';
async function main(){
 const styles=JSON.parse(fs.readFileSync('UnityProject/Assets/Editor/EchoSphereReference/SourceUiStyles.json','utf8'));
 const wanted=new Set(['transform','opacity','filter','box-shadow','text-shadow','color','background-color','border-color']);const seen=new Set(),cases=[];
 for(const rule of styles.rules)for(const d of rule.declarations){
  if(!wanted.has(d.property)||/var\(|currentColor|color-mix\(/i.test(d.value))continue;
  const key=d.property+':'+d.value;if(seen.has(key))continue;seen.add(key);cases.push({label:rule.selector,property:d.property,value:d.value});
 }
 const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});
 try{
  const page=await browser.newPage({viewport:{width:390,height:844}});await page.setContent('<div id="sample" style="width:240px;height:160px;color:rgb(0,0,0)"></div>');
  const fixtures=[];
  for(const item of cases){const result=await page.evaluate(item=>{
   const el=document.getElementById('sample');el.getAnimations().forEach(a=>a.cancel());el.style.removeProperty(item.property);
   const property=item.property==='border-color'?'border-top-color':item.property;
   const from=getComputedStyle(el).getPropertyValue(property);el.style.setProperty(item.property,item.value);const to=getComputedStyle(el).getPropertyValue(property);
   if(from===to||!to)return null;
   const animationProperty=property.replace(/-([a-z])/g,(_,letter)=>letter.toUpperCase());
   // Preserve authored transform lists: converting to a computed matrix loses
   // whole turns and changes CSS's compatible-function interpolation path.
   const target=property==='transform'?item.value:to;
   const animation=el.animate([{[animationProperty]:from},{[animationProperty]:target}],{duration:1000,easing:'ease',fill:'both'});animation.pause();
   const samples=[0,.031,.17,.53,.91,1].map(time=>{animation.currentTime=time*1000;const value=getComputedStyle(el).getPropertyValue(property);return{time,value,matrix:property==='transform'?Array.from(new DOMMatrixReadOnly(value==='none'?undefined:value).toFloat64Array()):undefined};});animation.cancel();el.style.removeProperty(item.property);
   return{property,from,to:target,samples};
  },item);if(result)fixtures.push({label:item.label,...result});}
  const text=JSON.stringify(fixtures,null,2)+'\n';if(process.argv.includes('--check')){if(fs.readFileSync(output,'utf8')!==text)throw Error('Browser transition reference changed');}else fs.writeFileSync(output,text);
  console.log(`Authored CSS transition values: ${fixtures.length} cases / ${fixtures.reduce((n,x)=>n+x.samples.length,0)} samples.`);
 }finally{await browser.close();}
}
main().catch(error=>{console.error(error);process.exitCode=1;});
