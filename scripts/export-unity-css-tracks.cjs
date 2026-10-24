const fs=require('node:fs');const {chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');
const styles=JSON.parse(fs.readFileSync('UnityProject/Assets/Editor/EchoSphereReference/SourceUiStyles.json'));const output='tools/EchoSphere.VisualChecks/source-grid-tracks.json';
(async()=>{const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});try{const page=await browser.newPage();const fixtures=[];const tracks=[...new Set(styles.rules.flatMap(rule=>rule.declarations.filter(d=>d.property==='grid-template-columns').map(d=>d.value)))];
 for(const template of tracks)for(const width of [360,760,1280])fixtures.push(await page.evaluate(({template,width})=>{
  document.body.innerHTML='';document.body.style.margin='0';const grid=document.createElement('div');grid.style.cssText=`display:grid;grid-template-columns:${template};width:${width}px;gap:5px;padding:7px;border:1px solid;box-sizing:border-box;`;document.body.append(grid);
  for(let i=0;i<14;i++){const cell=document.createElement('div');cell.style.cssText='height:20px;min-width:0';grid.append(cell);}const r=grid.getBoundingClientRect();return{template,width,height:r.height,cells:[...grid.children].map(cell=>{const b=cell.getBoundingClientRect();return{x:b.x-r.x,y:b.y-r.y,width:b.width,height:b.height};})};
 },{template,width}));
 const text=JSON.stringify(fixtures,null,2)+'\n';if(process.argv.includes('--check')){if(fs.readFileSync(output,'utf8')!==text)throw Error('Source grid track reference changed');}else fs.writeFileSync(output,text);console.log(`Original CSS grid declarations: ${tracks.length} templates / ${fixtures.length*14} cells.`);
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1});
