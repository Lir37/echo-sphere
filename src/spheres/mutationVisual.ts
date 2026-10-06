import type { LightningBolt, PlayerState, SphereEntity, SphereProjectile } from '../engine';
import type { SphereType } from '../gameData';
import { SPHERE_TYPES, sphereUsesProjectileModifiers } from '../gameData';

type MutationBranch = string;
const TAU=Math.PI*2;

function rgbOf(hex:string):string{
  const n=Number.parseInt(hex.slice(1),16);
  return ((n>>16)&255)+','+((n>>8)&255)+','+(n&255);
}
function branchOf(p:PlayerState,t:SphereType):MutationBranch|null{
  return (p.sphereBranches?.[t] as string|undefined) ?? null;
}
function finalOf(p:PlayerState,t:SphereType):number{
  const id=(p.evolutions||[]).find((x:string)=>x.startsWith('sphere:'+t+':7:'));
  if(!id)return -1;
  const n=Number(id.split(':').pop());
  return Number.isFinite(n)?n:-1;
}
function glow(ctx:CanvasRenderingContext2D,r:number,color:string,a=.07):void{
  const rgb=rgbOf(color),g=ctx.createRadialGradient(0,0,0,0,0,r);
  g.addColorStop(0,'rgba('+rgb+','+(a*3.4).toFixed(3)+')');
  g.addColorStop(.28,'rgba('+rgb+','+(a*1.15).toFixed(3)+')');
  g.addColorStop(.72,'rgba('+rgb+','+(a*.25).toFixed(3)+')');
  g.addColorStop(1,'rgba('+rgb+',0)');
  ctx.save();ctx.globalCompositeOperation='lighter';ctx.fillStyle=g;ctx.beginPath();ctx.arc(0,0,r,0,TAU);ctx.fill();ctx.restore();
}
function fin(ctx:CanvasRenderingContext2D,x:number,y:number,l:number,w:number,c:string,a:number,alpha=.75):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(a);ctx.globalAlpha=alpha;ctx.fillStyle='rgba(3,8,15,.92)';ctx.strokeStyle=c;ctx.lineWidth=Math.max(.7,w*.07);
  ctx.beginPath();ctx.moveTo(0,-w*.20);ctx.quadraticCurveTo(l*.42,-w*.82,l,-w*.06);ctx.quadraticCurveTo(l*.48,w*.32,0,w*.22);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();
}
function shard(ctx:CanvasRenderingContext2D,x:number,y:number,s:number,c:string,a:number,alpha=.8):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(a);ctx.globalAlpha=alpha;ctx.fillStyle='rgba(4,9,16,.94)';ctx.strokeStyle=c;ctx.lineWidth=Math.max(.55,s*.11);
  ctx.beginPath();ctx.moveTo(s,0);ctx.lineTo(0,-s*.54);ctx.lineTo(-s*.68,-s*.25);ctx.lineTo(-s*.44,s*.28);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();
}
function arc(ctx:CanvasRenderingContext2D,r:number,c:string,w:number,a0:number,a1:number,alpha=.65):void{
  ctx.save();ctx.strokeStyle=c;ctx.lineWidth=w;ctx.globalAlpha=alpha;ctx.lineCap='round';ctx.beginPath();ctx.arc(0,0,r,a0,a1);ctx.stroke();ctx.restore();
}
function tether(ctx:CanvasRenderingContext2D,sx:number,sy:number,ex:number,ey:number,b:number,c:string,w:number,alpha=.65,phase=0):void{
  const dx=ex-sx,dy=ey-sy,len=Math.hypot(dx,dy)||1,nx=-dy/len,ny=dx/len;
  ctx.save();ctx.strokeStyle=c;ctx.lineWidth=w;ctx.lineCap='round';ctx.globalAlpha=alpha;ctx.beginPath();ctx.moveTo(sx,sy);
  ctx.quadraticCurveTo((sx+ex)/2+nx*(b+Math.sin(phase)*w*2),(sy+ey)/2+ny*(b+Math.sin(phase)*w*2),ex,ey);ctx.stroke();ctx.restore();
}
function eye(ctx:CanvasRenderingContext2D,x:number,y:number,s:number,c:string,a=0):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(a);ctx.globalCompositeOperation='lighter';ctx.strokeStyle=c;ctx.lineWidth=Math.max(.6,s*.14);ctx.globalAlpha=.9;
  ctx.beginPath();ctx.moveTo(-s,0);ctx.quadraticCurveTo(0,-s*.55,s,0);ctx.quadraticCurveTo(0,s*.55,-s,0);ctx.stroke();
  ctx.fillStyle='#ffffff';ctx.beginPath();ctx.ellipse(0,0,s*.23,s*.48,0,0,TAU);ctx.fill();
  ctx.fillStyle=c;ctx.beginPath();ctx.ellipse(0,0,s*.10,s*.32,0,0,TAU);ctx.fill();ctx.restore();
}
function pulseRing(ctx:CanvasRenderingContext2D,r:number,c:string,t:number,scale=1):void{
  const q=(t*.62)%1;arc(ctx,r*(.72+q*1.02),c,Math.max(.7,r*.028),0,TAU,(.44*(1-q))*scale);
}
function attackPeak(s:SphereEntity):number{
  const d=Math.max(.15,s.attackDelay||1),p=Math.max(0,Math.min(1,1-s.attackTimer/d));
  return Math.exp(-Math.pow((p-.95)/.07,2));
}
function emit(ctx:CanvasRenderingContext2D,r:number,c:string,t:number,branch:string,final:number,sphere:SphereEntity):void{
  const p=attackPeak(sphere);
  if(!Number.isFinite(p)||p<.03)return;
  ctx.save();ctx.globalCompositeOperation='lighter';
  const strength=.28+.72*p;
  if(branch==='standard_resonator'){for(let i=0;i<3;i++){const a=t*.55+i*TAU/3;fin(ctx,Math.cos(a)*r*1.18,Math.sin(a)*r*.72,r*.44,r*.13,c,a+.2,.45+.4*p);}pulseRing(ctx,r*1.25,c,t,1);}
  else if(branch==='standard_singularity'){for(let i=0;i<4;i++){const a=-t*.35+i*TAU/4;tether(ctx,Math.cos(a)*r*1.15,Math.sin(a)*r*.7,Math.cos(a+.6)*r*.42,Math.sin(a+.6)*r*.25,r*.16,c,1.1,.35+.4*p,t+i);}arc(ctx,r*(1.08+p*.42),'#d8b9ff',1.2,.4,2.55,.8);}
  else if(branch==='standard_swarm'){for(let i=0;i<5;i++){const a=t*.48+i*TAU/5;shard(ctx,Math.cos(a)*r*(1.0+strength*.5),Math.sin(a)*r*.67,r*.16,c,a+.7,.45+.4*p);}}
  else if(branch==='sniper_oracle'){ctx.strokeStyle='#f5ddff';ctx.lineWidth=1.2;ctx.globalAlpha=.45+.45*p;ctx.beginPath();ctx.arc(r*1.15,0,r*(.25+.07*p),-1.0,1.0);ctx.stroke();ctx.beginPath();ctx.moveTo(r*.75,-r*.06);ctx.lineTo(r*(1.62+.20*p),0);ctx.lineTo(r*.75,r*.06);ctx.stroke();}
  else if(branch==='sniper_assassin'){for(const y of [-1,1]){ctx.strokeStyle=c;ctx.lineWidth=1.2;ctx.globalAlpha=.35+.45*p;ctx.beginPath();ctx.moveTo(r*.65,y*r*.16);ctx.quadraticCurveTo(r*(1.20+p*.25),y*r*.30,r*(1.72+p*.20),y*r*.05);ctx.stroke();}}
  else if(branch==='sniper_beacon'){for(let i=0;i<3;i++)pulseRing(ctx,r*(1.05+i*.25),c,t+i*.17,1);fin(ctx,0,-r*1.05,r*.40,r*.13,c,-Math.PI/2,.8);}
  else if(branch==='shotgun_burst'){for(let i=0;i<3;i++){ctx.strokeStyle=c;ctx.lineWidth=1.3;ctx.globalAlpha=.4+.3*p;ctx.beginPath();ctx.moveTo(r*.72,(i-1)*r*.15);ctx.lineTo(r*(1.58+p*.3),(i-1)*r*.22);ctx.stroke();}}
  else if(branch==='shotgun_cataclysm'){ctx.strokeStyle='#ffd29c';ctx.lineWidth=1.4;ctx.globalAlpha=.4+.5*p;ctx.beginPath();ctx.moveTo(r*.65,-r*.20);ctx.lineTo(r*(1.28+p*.30),0);ctx.lineTo(r*.65,r*.20);ctx.stroke();ctx.beginPath();ctx.arc(r*(1.35+p*.2),0,r*(.08+.06*p),0,TAU);ctx.stroke();}
  else if(branch==='shotgun_hail'){for(let i=0;i<6;i++){const a=-.75+i*.30;const rr=r*(1.05+p*.55);shard(ctx,Math.cos(a)*rr,Math.sin(a)*rr*.55,r*.10,c,a+.7,.35+.4*p);}}
  else if(branch==='chain_web'||branch==='chain_storm'||branch==='chain_leech'){const count=branch==='chain_storm'?7:8;for(let i=0;i<count;i++){const a=t*1.3+i*TAU/count,rr=r*(.8+strength*.9);tether(ctx,Math.cos(a)*r*.18,Math.sin(a)*r*.13,Math.cos(a)*rr,Math.sin(a)*rr*.62,r*.10,c,branch==='chain_storm'?1.35:1,.30+.45*p,a);} }
  else if(branch==='aura_sanctum'||branch==='aura_gravity'||branch==='aura_overgrowth'){for(let i=0;i<6;i++){const a=t*.3+i*TAU/6,rr=r*(1.0+strength*.8);tether(ctx,Math.cos(a)*r*.25,Math.sin(a)*r*.16,Math.cos(a)*rr,Math.sin(a)*rr*.58,r*.18,c,1.0,.22+.5*p,a);}arc(ctx,r*(1.15+strength*.55),c,1.0,-Math.PI*.75,Math.PI*.75,.35+.35*p);}
  else if(branch==='orbital_dance'||branch==='orbital_halo'||branch==='orbital_blade'){const n=final===2?6:4;for(let i=0;i<n;i++){const a=t*(branch==='orbital_dance'?1.8:.85)+i*TAU/n;fin(ctx,Math.cos(a)*r*(1.04+strength*.5),Math.sin(a)*r*.62,r*(.30+.18*p),r*.12,c,a,.32+.48*p);} }
  else if(branch==='prism_split'||branch==='prism_spectrum'||branch==='prism_mirror'){const n=branch==='prism_spectrum'?5:branch==='prism_split'?3:2;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.28;shard(ctx,r*(1.00+strength*.25),y,r*.13,c,0,.38+.45*p);}ctx.strokeStyle='#ffffff';ctx.globalAlpha=.30+.45*p;ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(r*.72,0);ctx.lineTo(r*(1.68+p*.2),0);ctx.stroke();}
  else if(branch==='gravity_well'||branch==='gravity_tide'||branch==='gravity_collapse'){for(let i=0;i<3;i++){const q=i/3,a=-t*.3+q*1.5;ctx.strokeStyle=c;ctx.globalAlpha=.22+.46*p;ctx.lineWidth=1.1;ctx.beginPath();for(let k=0;k<=14;k++){const u=k/14,rr=r*(.35+u*(.85+strength*.7)),ang=a+u*2.15;const x=Math.cos(ang)*rr,y=Math.sin(ang)*rr*.56;if(k===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.stroke();}}
  else if(branch==='pulse_wave'||branch==='pulse_resonator'||branch==='pulse_burst'){const lobes=branch==='pulse_resonator'?8:branch==='pulse_wave'?6:5;const rr=r*(.9+strength*.85);ctx.strokeStyle=c;ctx.globalAlpha=.25+.55*p;ctx.lineWidth=1.1;ctx.beginPath();for(let i=0;i<=48;i++){const a=i/48*TAU,q=1+.16*Math.sin(a*lobes+t*4.8)+.08*Math.sin(a*(lobes+2)-t*3.4),x=Math.cos(a)*rr*q,y=Math.sin(a)*rr*.54*q;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.stroke();if(branch==='pulse_burst')arc(ctx,rr*.46,'#fff0a9',1,.0,TAU,.40+.4*p);}
  else if(branch==='void_hunger'||branch==='void_reaper'||branch==='void_execution'){for(let i=0;i<4;i++){const a=t*.24+i*TAU/4;tether(ctx,Math.cos(a)*r*.3,Math.sin(a)*r*.2,Math.cos(a+.55)*r*(1.1+strength*.55),Math.sin(a+.55)*r*.65,r*.18,c,1.25,.30+.46*p,a);}if(branch==='void_reaper')fin(ctx,r*1.02,0,r*(.62+.18*p),r*.18,c,0,.8);else if(branch==='void_execution'){ctx.strokeStyle=c;ctx.lineWidth=1.25;ctx.globalAlpha=.4+.4*p;ctx.beginPath();ctx.moveTo(r*.8,-r*.28);ctx.lineTo(r*(1.55+.25*p),0);ctx.lineTo(r*.8,r*.28);ctx.stroke();}}
  ctx.restore();
}
type FinalExpansionMode = 'core' | 'cascade' | 'apex';

// Level VII is additive: the branch silhouette stays visible and the final form
// contributes one persistent animated motif of its own.
function drawFinalExpansion(ctx:CanvasRenderingContext2D,r:number,c:string,t:number,branch:string,final:number,sphere:SphereEntity):void{
  if(final<0)return;
  const mode:FinalExpansionMode=final===0?'core':final===1?'cascade':'apex';
  const family=branch.split('_')[0];
  const dir=Number.isFinite(sphere.rotation)?sphere.rotation:0;
  const phase=t*(mode==='core'?.72:mode==='cascade'?1.05:1.35);
  const rgb=rgbOf(c);
  ctx.save();ctx.globalCompositeOperation='lighter';ctx.lineCap='round';ctx.lineJoin='round';
  if(family==='orbital'){
    const count=mode==='core'?3:mode==='cascade'?4:5;
    const orbitR=r*(1.36+(mode==='apex'?.10:0));
    for(let i=0;i<count;i++){const a=phase+i*TAU/count;shard(ctx,Math.cos(a)*orbitR,Math.sin(a)*orbitR*.66,r*(mode==='apex'?.16:.12),c,a,.72);}
    if(mode==='cascade')pulseRing(ctx,r*1.18,c,t,.8);
    if(mode==='apex')arc(ctx,r*1.52,c,1.4,-.52,.52,.72);
  }else if(family==='chain'||family==='aura'||family==='gravity'){
    if(mode==='core'){
      for(let i=0;i<3;i++){const a=phase+i*TAU/3;tether(ctx,Math.cos(a)*r*.34,Math.sin(a)*r*.22,Math.cos(a)*r*1.25,Math.sin(a)*r*.70,r*.08,c,1.1,.62,a);}
      pulseRing(ctx,r*1.18,c,t,.72);
    }else if(mode==='cascade'){
      for(let i=0;i<4;i++){const a=-.92+i*.62,q=.55+.22*Math.sin(phase+i);tether(ctx,r*.10,0,Math.cos(a)*r*(.92+q),Math.sin(a)*r*(.52+q*.35),r*.10,c,1.05,.42+.08*i,phase+i);}
      arc(ctx,r*1.42,c,1.25,-1.05,1.05,.70);
    }else{
      ctx.save();ctx.rotate(dir);fin(ctx,r*.62,0,r*.72,r*.18,c,0,.90);fin(ctx,r*.96,0,r*.48,r*.13,c,0,.62);ctx.restore();pulseRing(ctx,r*1.32,c,t,1);
    }
  }else if(family==='pulse'){
    const lobes=mode==='core'?6:mode==='cascade'?8:10,rr=r*(1.24+(mode==='apex'?.12:0));
    ctx.strokeStyle=c;ctx.globalAlpha=.62;ctx.lineWidth=mode==='apex'?1.55:1.15;ctx.beginPath();
    for(let i=0;i<=60;i++){const a=i/60*TAU,q=1+.12*Math.sin(a*lobes+phase)+.05*Math.sin(a*(lobes+2)-phase*1.4),x=Math.cos(a)*rr*q,y=Math.sin(a)*rr*.56*q;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}
    ctx.stroke();if(mode==='cascade')pulseRing(ctx,r*1.48,c,t,.75);if(mode==='apex')arc(ctx,r*1.58,'rgba('+rgb+',.95)',1.6,-.28,.28,.85);
  }else if(family==='prism'){
    const count=mode==='core'?3:mode==='cascade'?4:5;
    for(let i=0;i<count;i++){const a=(i-(count-1)/2)*.22+Math.sin(phase+i)*.03,rr=r*(1.10+(mode==='apex'?.18:0));shard(ctx,Math.cos(a)*rr,Math.sin(a)*rr*.38,r*(mode==='apex'?.15:.11),c,a,.78);}
    if(mode==='cascade')tether(ctx,r*.62,0,r*1.58,0,r*.08,'#ffffff',.8,.52,phase);
    if(mode==='apex'){ctx.save();ctx.rotate(dir);fin(ctx,r*.72,0,r*.88,r*.17,c,0,.92);ctx.restore();}
  }else{
    ctx.save();if(family==='sniper'||family==='shotgun'||family==='standard'||family==='void')ctx.rotate(dir);
    if(mode==='core'){for(const y of [-1,1])fin(ctx,r*.62,y*r*.28,r*.56,r*.14,c,y>0?.18:-.18,.72);pulseRing(ctx,r*1.22,c,t,.72);}
    else if(mode==='cascade'){for(let i=0;i<4;i++){const q=i/3;fin(ctx,r*(.52+q*.58),Math.sin(phase+i)*r*.34,r*(.34+.10*q),r*(.11+.02*q),c,phase+i,.72-.08*q);}arc(ctx,r*1.46,c,1.15,-.70,.70,.70);}
    else{fin(ctx,r*.54,0,r*1.02,r*.22,c,0,.94);ctx.strokeStyle=c;ctx.lineWidth=1.35;ctx.globalAlpha=.82;ctx.beginPath();ctx.moveTo(r*.82,-r*.30);ctx.lineTo(r*1.62,0);ctx.lineTo(r*.82,r*.30);ctx.stroke();pulseRing(ctx,r*1.52,c,t,.82);}
    ctx.restore();
  }
  ctx.restore();
}
function mutationSilhouette(ctx:CanvasRenderingContext2D,r:number,c:string,t:number,branch:string,final:number):void{
  ctx.save();ctx.globalCompositeOperation='source-over';ctx.lineJoin='round';ctx.lineCap='round';
  const pulse=.92+.08*Math.sin(t*2.4);
  if(branch==='standard_resonator'){for(const y of [-1,1])fin(ctx,-r*.10,y*r*.62,r*.72,r*.16,c,y>0?.55:-.55,.70);arc(ctx,r*1.22,c,1.1,-.9,.9,.60);}
  else if(branch==='standard_singularity'){arc(ctx,r*(1.16+final*.08),'#d8b9ff',1.4,.2,5.9,.72);for(const a of [-.55,.55])tether(ctx,Math.cos(a)*r*.92,Math.sin(a)*r*.52,0,0,r*.18,c,1.2,.55,a);}
  else if(branch==='standard_swarm'){for(const y of [-1,1]){shard(ctx,r*1.02,y*r*.56,r*.18,c,y>0?.35:-.35,.78);shard(ctx,r*.72,y*r*.76,r*.13,c,y>0?.8:-.8,.64);}}
  else if(branch==='sniper_oracle'){arc(ctx,r*1.18,'#f5ddff',1.5,-.95,.95,.82);for(const y of [-1,1])fin(ctx,r*.42,y*r*.30,r*.72,r*.13,c,y>0?.12:-.12,.88);ctx.strokeStyle='#f5ddff';ctx.lineWidth=1;ctx.globalAlpha=.78;ctx.beginPath();ctx.moveTo(r*.78,-r*.08);ctx.lineTo(r*1.72,0);ctx.lineTo(r*.78,r*.08);ctx.stroke();}
  else if(branch==='sniper_assassin'){for(const y of [-1,1])fin(ctx,r*.40,y*r*.28,r*.82,r*.16,c,y>0?.34:-.34,.92);arc(ctx,r*1.42,c,1.1,-.42,.42,.70);}
  else if(branch==='sniper_beacon'){for(const y of [-1,1])fin(ctx,-r*.18,y*r*.48,r*.58,r*.15,c,y>0?-.95:.95,.82);arc(ctx,r*1.34,c,1.3,-.65,.65,.72);arc(ctx,r*1.56,c,.8,-.28,.28,.55);}
  else if(branch==='shotgun_burst'){for(const y of [-1,0,1]){const yy=y*r*.22;ctx.strokeStyle=c;ctx.lineWidth=2;ctx.globalAlpha=.78;ctx.beginPath();ctx.moveTo(r*.34,yy);ctx.lineTo(r*1.16,yy*(1+.08*final));ctx.stroke();}arc(ctx,r*1.26,c,1.2,-.42,.42,.55);}
  else if(branch==='shotgun_cataclysm'){arc(ctx,r*1.30,'#ffd29c',2,-.34,.34,.86);for(const y of [-1,1])fin(ctx,r*.30,y*r*.32,r*.72,r*.18,c,y>0?.18:-.18,.90);ctx.fillStyle='rgba(255,169,77,.28)';ctx.beginPath();ctx.arc(r*1.30,0,r*(.18+.04*final),0,TAU);ctx.fill();}
  else if(branch==='shotgun_hail'){for(let i=0;i<5;i++){const y=(i-2)*r*.24;shard(ctx,r*(.78+.06*(i%2)),y,r*.12,c,0,.72);}arc(ctx,r*1.12,c,1,-.72,.72,.56);}
  else if(branch==='chain_web'){for(const y of [-1,1])tether(ctx,-r*.25,y*r*.20,r*1.10,y*r*.72,r*.18,c,1.1,.62,y*.8);arc(ctx,r*1.15,c,1,-.9,.9,.58);}
  else if(branch==='chain_storm'){for(let i=0;i<5;i++){const a=-.8+i*.4;shard(ctx,r*(.86+.12*(i%2)),Math.sin(a)*r*.55,r*.12,c,a,.72);}}
  else if(branch==='chain_leech'){for(const y of [-1,1])fin(ctx,r*.15,y*r*.45,r*.62,r*.16,c,y>0?.7:-.7,.76);arc(ctx,r*1.12,c,1,-1,1,.58);}
  else if(branch.startsWith('aura_')){arc(ctx,r*(1.18+final*.08),c,1.5,-Math.PI*.82,Math.PI*.82,.70);for(const y of [-1,1])tether(ctx,0,y*r*.20,r*.96,y*r*.66,r*.16,c,1,.48,y*.7);}
  else if(branch.startsWith('orbital_')){const n=final===2?5:4;for(let i=0;i<n;i++){const a=t*(branch==='orbital_dance'?1.4:.55)+i*TAU/n;shard(ctx,Math.cos(a)*r*1.18,Math.sin(a)*r*.72,r*.13,c,a,.74);}}
  else if(branch.startsWith('prism_')){const n=branch==='prism_spectrum'?5:branch==='prism_split'?3:2;for(let i=0;i<n;i++)shard(ctx,r*1.12,(i-(n-1)/2)*r*.28,r*.12,c,0,.74);}
  else if(branch.startsWith('gravity_')){arc(ctx,r*(1.12+final*.10),c,1.25,0,TAU,.64);for(let i=0;i<3;i++)tether(ctx,r*.18,0,r*(.72+i*.18),0,r*.10,c,1,.46,t+i);}
  else if(branch.startsWith('pulse_')){const lobes=branch==='pulse_resonator'?8:branch==='pulse_wave'?6:5;ctx.strokeStyle=c;ctx.lineWidth=1.25;ctx.globalAlpha=.62;ctx.beginPath();for(let i=0;i<=36;i++){const a=i/36*TAU,rr=r*(1.02+.12*Math.sin(a*lobes+t*2.4)),x=Math.cos(a)*rr,y=Math.sin(a)*rr*.54;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.stroke();}
  else if(branch.startsWith('void_')){for(const y of [-1,1])fin(ctx,r*.36,y*r*.24,r*.72,r*.18,c,y>0?.22:-.22,.78);arc(ctx,r*(1.15+final*.08),c,1.2,.15,TAU-.15,.64);}
  if(final>=0){const fs=1+final*.08;ctx.globalAlpha=.42;ctx.strokeStyle=c;ctx.lineWidth=1;ctx.beginPath();ctx.ellipse(r*.92,0,r*.30*fs,r*.62*fs,0,0,TAU);ctx.stroke();ctx.globalAlpha=.9;const a=t*(.35+final*.08);shard(ctx,r*1.20*Math.cos(a),r*.72*Math.sin(a),r*.11,c,a,.76);}
  ctx.globalAlpha=pulse;ctx.restore();
}

function emitter(ctx:CanvasRenderingContext2D,r:number,c:string,t:number,branch:string,final:number):void{
  if(!sphereUsesProjectileModifiers(branch.split('_')[0] as SphereType))return;
  ctx.save();ctx.translate(r*1.03,0);ctx.globalAlpha=.86;ctx.strokeStyle=c;ctx.fillStyle='rgba(5,11,19,.96)';ctx.lineWidth=1.05;
  const rgb=rgbOf(c);
  if(branch.startsWith('standard_')){const n=final+2;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.15;ctx.beginPath();ctx.moveTo(-r*.15,y);ctx.lineTo(r*.34,y*.82);ctx.stroke();}ctx.beginPath();ctx.arc(r*.33,0,r*(.07+.02*final),0,TAU);ctx.fill();ctx.stroke();}
  else if(branch.startsWith('sniper_')){ctx.beginPath();ctx.moveTo(-r*.14,0);ctx.lineTo(r*.40,0);ctx.stroke();if(branch==='sniper_oracle')arc(ctx,r*.20,c,1,-.9,.9,.8);else if(branch==='sniper_assassin')fin(ctx,r*.12,0,r*(.46+.08*final),r*.10,c,0,.88);else {ctx.beginPath();ctx.moveTo(r*.06,-r*.16);ctx.lineTo(r*.06,r*.16);ctx.stroke();}}
  else if(branch.startsWith('shotgun_')){const n=branch==='shotgun_cataclysm'?1:branch==='shotgun_hail'?4:3;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.14;ctx.beginPath();ctx.moveTo(-r*.16,y);ctx.lineTo(r*.38,y);ctx.stroke();}if(branch==='shotgun_cataclysm'){ctx.fillStyle='rgba(255,169,77,.78)';ctx.beginPath();ctx.arc(r*.35,0,r*.09,0,TAU);ctx.fill();}}
  else if(branch.startsWith('prism_')){const n=branch==='prism_split'?3:branch==='prism_spectrum'?5:2;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.13;ctx.beginPath();ctx.moveTo(-r*.12,0);ctx.quadraticCurveTo(r*.15,y*.2,r*.38,y);ctx.stroke();}shard(ctx,r*.37,0,r*(.12+.02*final),c,0,.82);}
  else if(branch.startsWith('void_')){if(branch==='void_hunger'){ctx.beginPath();ctx.ellipse(r*.25,0,r*.16,r*.11,0,0,TAU);ctx.stroke();}else if(branch==='void_reaper')fin(ctx,r*.2,0,r*(.48+.08*final),r*.13,c,0,.9);else {ctx.beginPath();ctx.moveTo(r*.02,-r*.18);ctx.lineTo(r*(.42+.08*final),0);ctx.lineTo(r*.02,r*.18);ctx.stroke();}}
  ctx.restore();
}
function projectileCore(ctx:CanvasRenderingContext2D,p:SphereProjectile,player:PlayerState):void{
  const t=p.sourceSphere?.type as SphereType|undefined;
  const b=t?branchOf(player,t):null,f=t?finalOf(player,t):-1,r=Math.max(2,p.radius),c=p.color;
  ctx.save();ctx.globalCompositeOperation='lighter';
  if(t==='standard'){ctx.strokeStyle=c;ctx.lineWidth=Math.max(.7,r*.17);ctx.beginPath();ctx.moveTo(-r*.9,0);ctx.lineTo(r*1.35,0);ctx.stroke();ctx.beginPath();ctx.arc(r*.22,0,r*.30,0,TAU);ctx.stroke();if(b==='standard_singularity'){ctx.fillStyle='#05020b';ctx.beginPath();ctx.arc(0,0,r*.62,0,TAU);ctx.fill();}if(b==='standard_swarm'){for(const q of [-1,0,1])shard(ctx,q*r*.42,0,r*.25,c,q*.2,.8);}}
  else if(t==='sniper'){ctx.strokeStyle=c;ctx.lineWidth=Math.max(.7,r*.14);ctx.beginPath();ctx.moveTo(-r*.75,0);ctx.lineTo(r*1.35,0);ctx.stroke();if(b==='sniper_oracle'){ctx.beginPath();ctx.arc(r*.35,0,r*.50,0,TAU);ctx.stroke();}else if(b==='sniper_assassin')fin(ctx,r*.18,0,r*(.85+.15*f),r*.18,c,0,.9);else {arc(ctx,r*1.0,c,.8,-.8,.8,.9);}}
  else if(t==='shotgun'){const n=b==='shotgun_cataclysm'?1:b==='shotgun_hail'?4:3;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.34;ctx.beginPath();ctx.moveTo(-r*.70,y);ctx.lineTo(r*.92,y);ctx.stroke();}ctx.fillStyle='#fff';ctx.beginPath();ctx.arc(r*.58,0,r*.18,0,TAU);ctx.fill();}
  else if(t==='prism'){const n=b==='prism_split'?3:b==='prism_spectrum'?5:2;for(let i=0;i<n;i++){const y=(i-(n-1)/2)*r*.38;shard(ctx,0,y,r*.34,c,0,.75);}ctx.strokeStyle='#ffffff';ctx.lineWidth=.7;ctx.beginPath();ctx.moveTo(-r*.28,0);ctx.lineTo(r*1.04,0);ctx.stroke();}
  else if(t==='void'){if(b==='void_hunger'){ctx.fillStyle='#08030f';ctx.beginPath();ctx.arc(0,0,r*.68,0,TAU);ctx.fill();arc(ctx,r*.42,c,.9,.2,5.4,.9);}else if(b==='void_reaper')fin(ctx,0,0,r*.95,r*.24,c,0,.9);else {ctx.strokeStyle=c;ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(-r*.44,0);ctx.lineTo(r*.9,0);ctx.stroke();ctx.beginPath();ctx.moveTo(r*.55,-r*.25);ctx.lineTo(r*.9,0);ctx.lineTo(r*.55,r*.25);ctx.stroke();}}
  ctx.restore();
}
function projectileTail(ctx:CanvasRenderingContext2D,r:number,c:string,speed:number,t:number):void{
  const len=Math.min(58,14+speed*.045),rgb=rgbOf(c);
  ctx.globalAlpha=.17;ctx.fillStyle=c;ctx.beginPath();ctx.moveTo(0,0);ctx.quadraticCurveTo(-len*.35,-r*.82,-len*.86,-r*.12);ctx.quadraticCurveTo(-len,0,-len*.86,r*.12);ctx.quadraticCurveTo(-len*.35,r*.82,0,0);ctx.fill();
  ctx.globalAlpha=.10;ctx.strokeStyle='rgba('+rgb+',.80)';ctx.lineWidth=Math.max(1,r*.65);ctx.beginPath();ctx.moveTo(-len*.88,0);ctx.lineTo(-r*.10,0);ctx.stroke();
}
export function renderSphereMutationVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const branch=branchOf(player,sphere.type);if(!branch)return;
  const final=finalOf(player,sphere.type),r=24*scale,c=SPHERE_TYPES[sphere.type].color;
  const directedWeapon=sphereUsesProjectileModifiers(sphere.type);
  ctx.save();
  ctx.translate(sphere.pos.x,sphere.pos.y);
  // Directed mutation weapon parts belong to the base weapon axis.
  if(directedWeapon) ctx.rotate(sphere.rotation||0);
  glow(ctx,r*1.75,c,.055);
  // Mutation silhouette is persistent. Attack emitters are transient accents only.
  mutationSilhouette(ctx,r,c,time,branch,final);
  drawFinalExpansion(ctx,r,c,time,branch,final,sphere);
  emitter(ctx,r,c,time,branch,final);
  emit(ctx,r,c,time,branch,final,sphere);
  ctx.restore();
}
export function renderSphereProjectileVfx(ctx:CanvasRenderingContext2D,p:SphereProjectile,player:PlayerState,time:number):void{
  const a=Math.atan2(p.vel.y,p.vel.x),speed=Math.hypot(p.vel.x,p.vel.y)||1;
  const r=Math.max(2,p.radius);
  const rgb=rgbOf(p.color);
  ctx.save();
  ctx.translate(p.pos.x,p.pos.y);
  ctx.rotate(a);
  projectileTail(ctx,r,p.color,speed,time);
  // Projectile glows are numerous in high-density builds. Avoid creating a
  // radial gradient for every projectile while keeping a readable energy core.
  ctx.globalCompositeOperation='lighter';
  ctx.globalAlpha=.18;
  ctx.fillStyle='rgba('+rgb+',1)';
  ctx.beginPath();ctx.arc(0,0,r*1.65,0,TAU);ctx.fill();
  ctx.globalAlpha=1;
  projectileCore(ctx,p,player);
  ctx.restore();
}
export function renderChainLightningVfx(ctx:CanvasRenderingContext2D,bolt:LightningBolt,player:PlayerState,alpha:number,time:number):boolean{
  const sphere=bolt.sourceSphere;if(!sphere||sphere.type!=='chain')return false;
  const b=branchOf(player,'chain'),f=finalOf(player,'chain');if(!b)return false;
  const c=SPHERE_TYPES.chain.color,rgb=rgbOf(c),dx=bolt.to.x-bolt.from.x,dy=bolt.to.y-bolt.from.y,len=Math.hypot(dx,dy)||1,nx=-dy/len,ny=dx/len;
  const pieces=b==='chain_web'?10:b==='chain_storm'?8:12;
  ctx.save();ctx.globalCompositeOperation='lighter';ctx.lineCap='round';
  for(let pass=0;pass<2;pass++){
    ctx.strokeStyle=pass===0?'rgba('+rgb+','+(.18*alpha)+')':'rgba(245,255,255,'+(.88*alpha)+')';
    ctx.lineWidth=pass===0?Math.max(4,len*.015):Math.max(1.2,len*.0045);ctx.beginPath();ctx.moveTo(bolt.from.x,bolt.from.y);
    for(let i=1;i<pieces;i++){const q=i/pieces,amp=len*(b==='chain_storm'?.12:b==='chain_leech'?.06:.09),zig=(i%2?-1:1)*amp*(.7+.3*Math.sin(time*22+i));ctx.lineTo(bolt.from.x+dx*q+nx*zig,bolt.from.y+dy*q+ny*zig);}
    ctx.lineTo(bolt.to.x,bolt.to.y);ctx.stroke();
  }
  if(b==='chain_web'){ctx.strokeStyle='rgba(255,244,150,'+(.68*alpha)+')';ctx.lineWidth=1;for(let i=0;i<4;i++){const a=time*2+i*TAU/4;ctx.beginPath();ctx.moveTo(bolt.to.x,bolt.to.y);ctx.lineTo(bolt.to.x+Math.cos(a)*(8+f*2),bolt.to.y+Math.sin(a)*(6+f));ctx.stroke();}}
  else if(b==='chain_storm'){ctx.fillStyle='rgba(255,249,190,'+(.9*alpha)+')';ctx.beginPath();ctx.arc(bolt.to.x,bolt.to.y,3+f*.8,0,TAU);ctx.fill();}
  else {ctx.strokeStyle='rgba(255,255,255,'+(.7*alpha)+')';ctx.lineWidth=1;ctx.beginPath();ctx.arc(bolt.to.x,bolt.to.y,3.0+Math.sin(time*16)*.8,0,TAU);ctx.stroke();}
  ctx.restore();return true;
}
