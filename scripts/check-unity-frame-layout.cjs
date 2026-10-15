// Check the CSS box/content split in an independent layout engine. This does
// not execute Yoga/UI Toolkit; the Unity runtime remains a separate manual check.
const fs=require('node:fs');const{chromium}=require(process.env.ECHO_PLAYWRIGHT_PATH||'playwright');const{png}=require('./source-css-browser.cjs');
const frame=fs.readFileSync('UnityProject/Assets/EchoSphere/Scripts/Presentation/SourceUiFrame.cs','utf8');
const outer=[...frame.matchAll(/outer\.(\w+)=value\.\1/g)].map(match=>match[1]);
if(outer.length<25)throw Error('Native frame outward-style inventory changed');
const cases=[
 {name:'intrinsic text and clipped outer shadow',style:'padding:8px 11px;border:2px solid #55dfff;border-radius:8px;box-shadow:0 7px 14px #0008;background:#07111d;color:#dcecff;overflow:hidden',html:'Исходная панель / Source panel'},
 {name:'fixed border box and centered text',style:'width:220px;height:74px;padding:8px;border:1px solid #55dfff88;border-radius:50%;box-shadow:0 0 16px #55dfff88;background:#07111d;color:#fff;overflow:hidden;display:flex;align-items:center;justify-content:center',html:'КРИСТАЛЛ'},
 {name:'flex row and shrinking text',style:'width:250px;padding:7px;display:flex;gap:8px;align-items:center;border:1px solid #55dfff;overflow:hidden;border-radius:8px;background:#07111d',html:'<div style="flex:0 0 32px;height:32px;background:#55dfff"></div><div style="min-width:0;flex:1;white-space:normal">Длинная исходная строка и перенос слов внутри строки</div>'},
 {name:'column intrinsic height',style:'width:220px;padding:7px;border:1px solid #a98cff;border-radius:8px;overflow:hidden;display:flex;flex-direction:column;gap:6px;background:#07111d',html:'<div style="padding:3px;border:1px solid #55dfff">Первый ряд</div><div style="padding:5px">Второй ряд с длинным текстом и переносом</div>'},
 {name:'auto width nested content and opacity',style:'display:flex;flex-direction:column;align-items:stretch;opacity:.72;padding:9px;border:1px solid #55dfff;box-shadow:0 0 18px #55dfff66;border-radius:8px;overflow:hidden;background:#07111d',html:'<div>Auto width</div><div style="font-weight:700">СОСТОЯНИЕ</div>'},
 {name:'scroll viewport',style:'height:90px;width:220px;padding:6px;border:1px solid #55dfff;overflow:auto;background:#07111d',html:'<div style="height:180px;flex-shrink:0;background:linear-gradient(#182d42,#07111d)">Прокрутка / scroll</div>'},
 {name:'min height and percentage child',style:'min-height:90px;width:250px;display:flex;align-items:center;justify-content:space-between;border:1px solid #a98cff;padding:9px;overflow:hidden;background:#07111d',html:'<div style="width:45%;padding:5px;border:1px solid #55dfff">Левая</div><div style="width:45%;padding:5px;border:1px solid #55dfff">Правая</div>'},
 {name:'positioned child and clipped border',style:'position:relative;width:220px;height:74px;border:2px solid #55dfff;padding:8px;border-radius:8px;overflow:hidden;box-shadow:0 7px 14px #0008;background:#07111d',html:'<div style="position:absolute;left:0;top:0;width:100%;height:26px;background:#325b78">absolute</div>'}
];
(async()=>{const browser=await chromium.launch({headless:true,channel:process.env.ECHO_ART_BROWSER||'msedge'});try{
 let count=0,worst=0;
 for(const dpr of[1,1.75])for(const item of cases){const context=await browser.newContext({viewport:{width:360,height:300},deviceScaleFactor:dpr});const page=await context.newPage();
  await page.setContent('<style>*{box-sizing:border-box}html,body{margin:0;background:#162534;color:#dcecff;font:14px Arial}#host{position:absolute;left:42px;top:42px;display:flex;align-items:flex-start}</style><div id="host"><div id="subject" style="'+item.style+'">'+item.html+'</div></div>');
  const beforeBytes=await page.screenshot();const before=png(beforeBytes);
  const geometry=await page.evaluate(outer=>{
   const records=[];function wrap(content){const children=[...content.children];const bounds=content.getBoundingClientRect();const frame=document.createElement('div');const brush=document.createElement('div');const css=content.style;
    for(const property of outer)frame.style[property]=css[property];frame.style.position=css.position||'relative';frame.style.overflow='visible';frame.style.display=css.display==='none'?'none':'flex';frame.style.flexDirection='column';frame.style.alignItems='stretch';frame.style.padding='0';frame.style.border='0';
    for(const property of ['background','backgroundColor','backgroundImage','backgroundSize','backgroundPosition','backgroundRepeat','border','borderColor','borderWidth','borderStyle','borderRadius','boxShadow'])brush.style[property]=css[property];
    Object.assign(brush.style,{position:'absolute',left:'0',top:'0',width:'100%',height:'100%',pointerEvents:'none'});
    content.replaceWith(frame);frame.append(brush,content);
    Object.assign(css,{position:'relative',left:'auto',right:'auto',top:'auto',bottom:'auto',width:'auto',height:'auto',minWidth:'0',minHeight:'0',maxWidth:'none',maxHeight:'none',aspectRatio:'auto',margin:'0',flexGrow:'1',flexShrink:'1',flexBasis:'auto',alignSelf:'stretch',transform:'none',opacity:'1',filter:'none',background:'transparent',borderColor:'transparent',boxShadow:'none'});
    records.push({frame,bounds:{x:bounds.x,y:bounds.y,width:bounds.width,height:bounds.height}});for(const child of children)wrap(child);
   }wrap(document.querySelector('#subject'));return records.map(({frame,bounds})=>{const actual=frame.getBoundingClientRect();return Math.max(...['x','y','width','height'].map(key=>Math.abs(actual[key]-bounds[key])));});
  },outer);
  const afterBytes=await page.screenshot();const after=png(afterBytes);let error=0;for(let i=0;i<before.pixels.length;i++)error+=Math.abs(before.pixels[i]-after.pixels[i]);const mean=error/before.pixels.length;worst=Math.max(worst,mean);
  if(Math.max(...geometry)>.05||mean>.15){fs.writeFileSync('../.migration-tools/frame-before.png',beforeBytes);fs.writeFileSync('../.migration-tools/frame-after.png',afterBytes);throw Error(item.name+' DPR '+dpr+': box delta '+Math.max(...geometry)+', image mean '+mean.toFixed(4)+'/255');}count++;await context.close();
 }
 console.log(`Native CSS frame/content split: PASS (${count} browser layout/image cases, DPR 1/1.75; worst mean ${worst.toFixed(4)}/255). UI Toolkit execution not covered.`);
}finally{await browser.close();}})().catch(error=>{console.error(error);process.exitCode=1});
