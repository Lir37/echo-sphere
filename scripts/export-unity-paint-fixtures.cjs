const fs=require('node:fs'),zlib=require('node:zlib');const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const {stylesheet,png}=require('./source-css-browser.cjs');
const styles=JSON.parse(fs.readFileSync('UnityProject/Assets/Resources/EchoSphere/SourceUiStyles.json','utf8'));
const output='tools/EchoSphere.VisualChecks/source-paint-fixtures.json.gz';
const properties=['background-image','background-color','background-size','background-position','background-repeat','background-origin','background-clip','color','box-shadow','clip-path','mask-image','font-size',...['top','right','bottom','left'].flatMap(side=>['width','style','color'].map(kind=>'border-'+side+'-'+kind)),...['top-left','top-right','bottom-left','bottom-right'].map(corner=>'border-'+corner+'-radius')];
async function main(){const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});try{
 const page=await browser.newPage({viewport:{width:480,height:400},deviceScaleFactor:1});await page.setContent('<html><head></head><body><div id="paint"></div></body></html>');await page.addStyleTag({content:stylesheet(styles)});
 await page.addStyleTag({content:'html,body { background:transparent!important; background-image:none!important; opacity:1!important;overflow:hidden!important; } body {display:block!important; margin:0!important;}'});
 const fixtures=[],seen=new Set();
 for(const rule of styles.rules){if(!rule.declarations.some(d=>/^(?:background|box-shadow|mask-image|clip-path)/.test(d.property)&&/gradient\(|(?:px.*px)/.test(d.value)||d.property.startsWith('border')&&d.value.includes('dashed')))continue;
  const style=await page.evaluate(({declarations,properties})=>{const e=document.querySelector('#paint');e.style.cssText='';for(const d of declarations)e.style.setProperty(d.property,d.value,d.important?'important':'');for(const [key,value]of Object.entries({position:'absolute',left:'120px',top:'110px',width:'180px',height:'140px',minWidth:'0',minHeight:'0',maxWidth:'none',maxHeight:'none',margin:'0',padding:'0',boxSizing:'border-box',transform:'none',filter:'none',opacity:'1',animation:'none',transition:'none',visibility:'visible',display:'block'}))e.style.setProperty(key.replace(/[A-Z]/g,x=>'-'+x.toLowerCase()),value,'important');
   const s=getComputedStyle(e);return Object.fromEntries(properties.map(key=>[key,s.getPropertyValue(key)]));
  },{declarations:rule.declarations,properties});
  if(style['background-image']==='none'&&style['box-shadow']==='none'&&style['mask-image']==='none'&&style['clip-path']==='none'&&style['border-top-style']!=='dashed')continue;
  const signature=JSON.stringify(style);if(seen.has(signature))continue;seen.add(signature);
  const shot=png(await page.screenshot({omitBackground:true}));fixtures.push({label:rule.selector,viewport:{width:480,height:400},x:120,y:110,width:180,height:140,style,pixels:shot.pixels.toString('base64')});
 }
 const buffer=zlib.gzipSync(JSON.stringify(fixtures)+'\n');if(process.argv.includes('--check')){if(!fs.readFileSync(output).equals(buffer))throw Error('Browser paint fixture changed');}else fs.writeFileSync(output,buffer);console.log(`Browser CSS brush references: ${fixtures.length} unique authored gradients / shadows / masks.`);
}finally{await browser.close();}}
main().catch(error=>{console.error(error);process.exitCode=1;});
