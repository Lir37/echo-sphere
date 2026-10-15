const fs=require('node:fs');const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const styles=JSON.parse(fs.readFileSync('UnityProject/Assets/Resources/EchoSphere/SourceUiStyles.json'));const output='tools/EchoSphere.VisualChecks/source-transition-timelines.json';
async function main(){const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});try{const page=await browser.newPage();const fixtures=[];
 const unique=new Map();for(const rule of styles.rules){const declarations=rule.declarations.filter(d=>d.property.startsWith('transition'));if(declarations.length)unique.set(JSON.stringify(declarations),{selector:rule.selector,declarations});}const rules=[...unique.values()];
 const tailwind=styles.rules.find(rule=>rule.selector==='.transition');for(const rule of [...rules])if(rule.declarations.some(d=>d.property==='transition'))rules.push({selector:'.transition + '+rule.selector,declarations:[...tailwind.declarations.filter(d=>d.property.startsWith('transition')),...rule.declarations]});
 for(const rule of rules){const fixture=await page.evaluate(async rule=>{
  document.body.replaceChildren();const node=document.createElement('div');node.style.cssText='width:240px;height:160px;opacity:1;transform:none;background-color:rgb(13,23,38);border-color:rgb(36,59,85);';document.body.append(node);
  const start={opacity:'1',transform:'none','background-color':'rgb(13,23,38)','border-top-color':'rgb(36,59,85)',color:'rgb(13,23,38)',display:'block'};const end={opacity:'.5',transform:'scale(.95)','background-color':'rgb(57,216,255)','border-top-color':'rgb(57,216,255)',color:'rgb(57,216,255)',display:'flex'};
  node.style.color=start.color;node.style.display=start.display;
  const child=document.createElement('span');child.textContent='Эхо AV';node.append(child);
  for(const d of rule.declarations)if(d.property.startsWith('transition'))node.style.setProperty(d.property,d.value);
  void node.offsetWidth;for(const [name,value]of Object.entries(end))node.style.setProperty(name,value);void node.offsetWidth;
  const animations=node.getAnimations();for(const a of animations){a.pause();a.currentTime=0;}
  const samples=[0,.031,.075,.149,.16,.4].map(time=>{for(const a of animations)a.currentTime=time*1000;const computed=getComputedStyle(node);return{time,style:Object.fromEntries(Object.keys(end).map(name=>[name,computed.getPropertyValue(name)])),inheritedColor:getComputedStyle(child).color,matrix:Array.from(new DOMMatrixReadOnly(computed.transform==='none'?undefined:computed.transform).toFloat64Array())};});
  return{from:start,to:end,samples};
 },rule);fixtures.push({label:rule.selector,declarations:rule.declarations.filter(d=>d.property.startsWith('transition')),...fixture});}
 const text=JSON.stringify(fixtures,null,2)+'\n';if(process.argv.includes('--check')){if(fs.readFileSync(output,'utf8')!==text)throw Error('Source transition timeline changed');}else fs.writeFileSync(output,text);console.log(`Original CSS transition timelines: ${fixtures.length} declarations / ${fixtures.reduce((n,x)=>n+x.samples.length,0)} samples.`);
}finally{await browser.close();}}main().catch(e=>{console.error(e);process.exitCode=1});
