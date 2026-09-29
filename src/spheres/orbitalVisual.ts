import type { EnemyEntity, PlayerState, SphereEntity } from '../engine';

const TAU=Math.PI*2;
const BASE='#8ef0ff';
const WHITE='#ffffff';
const RESONANCE='#ffd166';
const DISABLED='#ff4d70';
const LINE=1.55;

type VisualState={lastTime:number;lastAura:number;attack:number;resonance:number;phase:number};
const STATES=new WeakMap<SphereEntity,VisualState>();
const GRADIENTS=new WeakMap<CanvasRenderingContext2D,Map<string,CanvasGradient>>();

function stateOf(s:SphereEntity):VisualState{
  let v=STATES.get(s);
  if(v)return v;
  v={lastTime:0,lastAura:Number.isFinite(s.auraTimer)?s.auraTimer:0,attack:0,resonance:0,phase:((s.pos.x*.011+s.pos.y*.007)%TAU+TAU)%TAU};
  STATES.set(s,v);
  return v;
}
function cache(c:CanvasRenderingContext2D):Map<string,CanvasGradient>{
  let m=GRADIENTS.get(c);if(m)return m;m=new Map();GRADIENTS.set(c,m);return m;
}
function glow(c:CanvasRenderingContext2D,mode:number):CanvasGradient{
  const key=String(mode),old=cache(c).get(key);if(old)return old;
  const g=c.createRadialGradient(0,0,0,0,0,1);
  if(mode===1){
    g.addColorStop(0,'rgba(255,209,102,.72)');g.addColorStop(.18,'rgba(255,209,102,.40)');g.addColorStop(.52,'rgba(255,209,102,.12)');g.addColorStop(1,'rgba(255,209,102,0)');
  }else if(mode===2){
    g.addColorStop(0,'rgba(255,77,112,.60)');g.addColorStop(.18,'rgba(255,77,112,.30)');g.addColorStop(.52,'rgba(255,77,112,.10)');g.addColorStop(1,'rgba(255,77,112,0)');
  }else{
    g.addColorStop(0,'rgba(255,255,255,.78)');g.addColorStop(.16,'rgba(142,240,255,.44)');g.addColorStop(.52,'rgba(142,240,255,.14)');g.addColorStop(1,'rgba(142,240,255,0)');
  }
  cache(c).set(key,g);return g;
}
function drawGlow(c:CanvasRenderingContext2D,r:number,mode:number,a:number):void{
  c.save();c.globalAlpha=a;c.fillStyle=glow(c,mode);c.scale(r,r);c.beginPath();c.arc(0,0,1,0,TAU);c.fill();c.restore();
}
function update(s:SphereEntity,p:PlayerState,t:number):VisualState{
  const v=stateOf(s),dt=v.lastTime>0?Math.min(.05,Math.max(0,t-v.lastTime)):0;v.lastTime=t;
  const disabled=s.networkDisabledTimer>0||!s.alive;
  if(!disabled){
    const aura=Number.isFinite(s.auraTimer)?s.auraTimer:0;
    if(aura>v.lastAura+.12)v.attack=.28;
    v.lastAura=aura;v.attack=Math.max(0,v.attack-dt);
    if(p?.resonanceEventActive)v.resonance=1.15;
    v.resonance=Math.max(0,v.resonance-dt);
  }
  return v;
}
function drawCore(c:CanvasRenderingContext2D,r:number,color:string,pulse:number,disabled:boolean):void{
  const scale=.96+pulse*.05;
  c.save();c.scale(scale,scale);
  c.globalAlpha=disabled?.24:.92;c.fillStyle='#07111d';c.strokeStyle=color;c.lineWidth=LINE;
  c.beginPath();
  for(let i=0;i<12;i++){const a=-Math.PI/2+i*TAU/12,rr=i%2===0?r*.82:r*.70,x=Math.cos(a)*rr,y=Math.sin(a)*rr*.86;if(i===0)c.moveTo(x,y);else c.lineTo(x,y);}
  c.closePath();c.fill();c.stroke();
  c.globalAlpha=disabled?.10:.44;c.strokeStyle='#dff8ff';c.lineWidth=.9;
  c.beginPath();c.moveTo(-r*.50,-r*.12);c.lineTo(0,-r*.52);c.lineTo(r*.50,-r*.12);c.moveTo(-r*.46,r*.20);c.lineTo(0,r*.50);c.lineTo(r*.46,r*.20);c.stroke();
  c.globalAlpha=disabled?.12:.95;c.fillStyle=WHITE;c.beginPath();c.arc(-r*.05,-r*.06,r*(.14+pulse*.018),0,TAU);c.fill();
  c.globalAlpha=disabled?.10:.55;c.fillStyle=color;c.beginPath();c.arc(-r*.05,-r*.06,r*.27,0,TAU);c.fill();
  c.restore();
}
function drawRail(c:CanvasRenderingContext2D,r:number,rotation:number,color:string,a:number,w:number):void{
  c.save();c.rotate(rotation);c.globalAlpha=a;c.strokeStyle=color;c.lineWidth=w;
  c.beginPath();c.ellipse(0,0,r*1.24,r*.46,0,0,TAU);c.stroke();
  c.globalAlpha=a*.46;c.lineWidth=Math.max(.65,w*.42);c.beginPath();c.ellipse(0,0,r*1.24,r*.46,0,Math.PI*.12,Math.PI*1.88);c.stroke();c.restore();
}
function drawCrystal(c:CanvasRenderingContext2D,x:number,y:number,size:number,color:string,rotation:number):void{
  c.save();c.translate(x,y);c.rotate(rotation);c.globalAlpha=.90;c.fillStyle='#07111d';c.strokeStyle=color;c.lineWidth=1.3;
  c.beginPath();c.moveTo(0,-size);c.lineTo(size*.46,-size*.22);c.lineTo(size*.30,size*.72);c.lineTo(0,size);c.lineTo(-size*.30,size*.72);c.lineTo(-size*.46,-size*.22);c.closePath();c.fill();c.stroke();
  c.globalAlpha=.78;c.fillStyle=WHITE;c.beginPath();c.moveTo(0,-size*.55);c.lineTo(size*.20,-size*.08);c.lineTo(0,size*.28);c.lineTo(-size*.20,-size*.08);c.closePath();c.fill();c.restore();
}
function drawSatellite(c:CanvasRenderingContext2D,x:number,y:number,size:number,color:string,a:number,rotation:number):void{
  c.save();c.translate(x,y);c.rotate(rotation);c.globalAlpha=a;c.fillStyle='#07111d';c.strokeStyle=color;c.lineWidth=1.25;
  c.beginPath();c.moveTo(0,-size);c.lineTo(size*.70,-size*.24);c.lineTo(size*.46,size*.68);c.lineTo(0,size);c.lineTo(-size*.46,size*.68);c.lineTo(-size*.70,-size*.24);c.closePath();c.fill();c.stroke();
  c.globalAlpha=a*.92;c.fillStyle=WHITE;c.beginPath();c.arc(0,0,size*.20,0,TAU);c.fill();c.restore();
}

export function renderOrbitalSphereRuntimeVfx(c:CanvasRenderingContext2D,s:SphereEntity,p:PlayerState,time:number,scale=1,_enemies:EnemyEntity[]=[]):void{
  const v=update(s,p,time),disabled=s.networkDisabledTimer>0||!s.alive,resonance=!disabled&&v.resonance>0,mode=disabled?2:resonance?1:0;
  const color=mode===2?DISABLED:mode===1?RESONANCE:BASE,pulse=disabled?.5:.5+.5*Math.sin(time*3.1+v.phase),r=24*scale;
  c.save();c.translate(s.pos.x,s.pos.y);c.globalCompositeOperation='lighter';drawGlow(c,r*2.0,mode,disabled?.07:.14);c.restore();

  c.save();c.translate(s.pos.x,s.pos.y);c.globalCompositeOperation='source-over';
  drawRail(c,r,0,color,disabled?.16:resonance?.58:.52,1.55);
  drawRail(c,r,Math.PI/2,color,disabled?.12:resonance?.40:.36,1.20);
  drawCore(c,r*.96,color,pulse,disabled);
  const tilt=Math.sin(time*.75+v.phase)*.025;
  drawCrystal(c,0,-r*1.16,r*.43,color,tilt);
  drawCrystal(c,0,r*1.16,r*.37,color,-tilt);
  if(resonance){c.globalAlpha=.42;c.strokeStyle=RESONANCE;c.lineWidth=1.0;c.beginPath();c.arc(0,0,r*1.42,time*.65,time*.65+Math.PI*1.12);c.stroke();}
  if(disabled){c.globalAlpha=.52;c.strokeStyle=DISABLED;c.lineWidth=1.65;c.beginPath();c.moveTo(-r*.76,-r*.76);c.lineTo(r*.76,r*.76);c.moveTo(r*.76,-r*.76);c.lineTo(-r*.76,r*.76);c.stroke();}
  c.restore();
}

export function renderOrbitalSphereAttackersVfx(c:CanvasRenderingContext2D,s:SphereEntity,p:PlayerState,time:number,scale=1,enemies:EnemyEntity[]=[]):void{
  if(!s.alive||s.networkDisabledTimer>0)return;
  const v=update(s,p,time),r=24*scale,tier=Math.max(1,Math.min(7,s.visualTier||1)),resonance=v.resonance>0;
  const color=resonance?RESONANCE:BASE,branch=p?.sphereBranches?.orbital;
  const speed=branch==='orbital_dance'?.72:branch==='orbital_halo'?.30:branch==='orbital_blade'?.50:.42;
  const phase=time*speed+v.phase;
  c.save();c.translate(s.pos.x,s.pos.y);c.globalCompositeOperation='lighter';

  for(let i=0;i<tier;i++){
    const a=phase+i*TAU/tier,x=Math.cos(a)*r*1.24,y=Math.sin(a)*r*.46;
    const depth=.54+.46*((Math.sin(a)+1)/2);
    drawSatellite(c,x,y,r*(.12+(Math.sin(a)+1)*.012),color,depth,a+Math.PI/2);
  }

  if(v.attack>0){
    const q=1-v.attack/.28,current=phase+(1-Math.pow(1-q,3))*.95;
    const sx=Math.cos(current)*r*1.24,sy=Math.sin(current)*r*.46;
    let ix=sx,iy=sy,near=Infinity;
    for(const e of enemies){
      if(!e||e.hp<=0)continue;
      const ex=e.pos.x-s.pos.x-sx,ey=e.pos.y-s.pos.y-sy,d=Math.hypot(ex,ey);
      if(d<near&&d<=r*.95){near=d;ix=sx+ex;iy=sy+ey;}
    }
    c.save();c.translate(sx,sy);c.rotate(current+Math.PI/2);c.strokeStyle=color;c.lineWidth=1.05;
    for(let i=3;i>=1;i--){c.globalAlpha=.30*(1-q)/i;c.beginPath();c.moveTo(-r*.10*i,0);c.lineTo(r*.02,0);c.stroke();}
    c.restore();
    c.globalAlpha=.70*(1-q);c.fillStyle=WHITE;c.beginPath();c.arc(ix,iy,r*.07,0,TAU);c.fill();
  }
  c.restore();
}
export default renderOrbitalSphereRuntimeVfx;
