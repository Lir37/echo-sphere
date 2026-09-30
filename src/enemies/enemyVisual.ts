const TAU=Math.PI*2;

function rgbOf(hex:string):string{
  const n=Number.parseInt(hex.slice(1),16);
  return ((n>>16)&255)+','+((n>>8)&255)+','+(n&255);
}
function glow(ctx:CanvasRenderingContext2D,r:number,color:string,a=.08):void{
  const g=ctx.createRadialGradient(0,0,0,0,0,r);
  const c=rgbOf(color);
  g.addColorStop(0,'rgba('+c+','+(a*2.8)+')');g.addColorStop(.35,'rgba('+c+','+(a*.9)+')');g.addColorStop(1,'rgba('+c+',0)');
  ctx.save();ctx.globalCompositeOperation='lighter';ctx.fillStyle=g;ctx.beginPath();ctx.arc(0,0,r,0,TAU);ctx.fill();ctx.restore();
}
function joint(ctx:CanvasRenderingContext2D,x:number,y:number,r:number,color:string):void{
  ctx.save();ctx.fillStyle='rgba(4,8,14,.98)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,r*.18);ctx.beginPath();ctx.arc(x,y,r,0,TAU);ctx.fill();ctx.stroke();ctx.restore();
}
function seg(ctx:CanvasRenderingContext2D,x:number,y:number,len:number,w:number,ang:number,color:string,alpha=.9):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(ang);ctx.globalAlpha=alpha;ctx.fillStyle='rgba(4,8,15,.98)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,w*.09);
  ctx.beginPath();ctx.moveTo(0,-w*.34);ctx.quadraticCurveTo(len*.48,-w*.48,len,-w*.10);ctx.quadraticCurveTo(len*.62,w*.42,0,w*.30);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.restore();
}
function limb(ctx:CanvasRenderingContext2D,sx:number,sy:number,len:number,a:number,color:string,phase:number,thick=1):void{
  // Controlled two-joint limb. The upper segment carries the gait, the knee
  // remains readable, and the short distal segment prevents the old "rubber
  // sausage" look caused by independent free-form wobble.
  const gait=Math.sin(phase)*.10;
  const knee=Math.sin(phase+0.65)*.07;
  const k1=len*.48,k2=len*.40;
  const a1=a+gait;
  const a2=a+knee+(a>Math.PI*.5?-.04:.04);
  seg(ctx,sx,sy,k1,thick*5.8,a1,color,.92);
  const jx=sx+Math.cos(a1)*k1,jy=sy+Math.sin(a1)*k1;
  joint(ctx,jx,jy,thick*3.5,color);
  seg(ctx,jx,jy,k2,thick*4.0,a2,color,.88);
  const ex=jx+Math.cos(a2)*k2,ey=jy+Math.sin(a2)*k2;
  joint(ctx,ex,ey,thick*1.9,color);
  ctx.save();
  ctx.translate(ex,ey);ctx.rotate(a2);
  ctx.strokeStyle=color;ctx.lineWidth=Math.max(.65,thick*1.5);ctx.lineCap='round';
  ctx.beginPath();ctx.moveTo(0,0);ctx.lineTo(thick*3.2,thick*.55);ctx.stroke();
  ctx.restore();
}
function tail(ctx:CanvasRenderingContext2D,sx:number,sy:number,len:number,color:string,t:number,segments=7,amp=.28):void{
  let x=sx,y=sy;
  for(let i=0;i<segments;i++){
    const q=i/segments,w=len*(.16-.008*i),a=Math.sin(t*3.0+i*.9)*amp+(i%2?.10:-.04);
    const l=len/segments*(1-.035*i),ang=a+(i>0?Math.atan2(y-(sy),x-(sx)):0);
    seg(ctx,x,y,l,w,ang,color,.78);
    x+=Math.cos(ang)*l;y+=Math.sin(ang)*l;
  }
}
function eye(ctx:CanvasRenderingContext2D,x:number,y:number,r:number,color:string,look=0):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(look);ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,r*.16);ctx.fillStyle='rgba(245,252,255,.94)';
  ctx.beginPath();ctx.ellipse(0,0,r,r*.62,0,0,TAU);ctx.fill();ctx.stroke();ctx.fillStyle=color;ctx.beginPath();ctx.ellipse(r*.18,0,r*.20,r*.42,0,0,TAU);ctx.fill();ctx.restore();
}
function core(ctx:CanvasRenderingContext2D,x:number,y:number,r:number,color:string):void{
  ctx.save();ctx.globalCompositeOperation='lighter';const g=ctx.createRadialGradient(x,y,0,x,y,r*2.2),c=rgbOf(color);g.addColorStop(0,'rgba(255,255,255,.95)');g.addColorStop(.24,'rgba('+c+',.95)');g.addColorStop(1,'rgba('+c+',0)');ctx.fillStyle=g;ctx.beginPath();ctx.arc(x,y,r*2.2,0,TAU);ctx.fill();ctx.restore();
  ctx.save();ctx.fillStyle='#07111b';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,r*.15);ctx.beginPath();ctx.arc(x,y,r,0,TAU);ctx.fill();ctx.stroke();ctx.restore();
}
function body(ctx:CanvasRenderingContext2D,r:number,color:string,rot=0,scaleY=1):void{
  ctx.save();ctx.rotate(rot);ctx.scale(1,scaleY);ctx.fillStyle='rgba(3,7,13,.98)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(1,r*.055);
  ctx.beginPath();ctx.moveTo(-r*.72,0);ctx.quadraticCurveTo(-r*.52,-r*.62,0,-r*.58);ctx.quadraticCurveTo(r*.58,-r*.52,r*.70,0);ctx.quadraticCurveTo(r*.52,r*.56,0,r*.62);ctx.quadraticCurveTo(-r*.56,r*.55,-r*.72,0);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();
}
function wing(ctx:CanvasRenderingContext2D,x:number,y:number,len:number,w:number,a:number,color:string,flap:number):void{
  ctx.save();ctx.translate(x,y);ctx.rotate(a+Math.sin(flap)*.14);ctx.fillStyle='rgba(4,9,17,.96)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.8,w*.07);
  ctx.beginPath();ctx.moveTo(0,0);ctx.quadraticCurveTo(len*.35,-w*.85,len,-w*.18);ctx.quadraticCurveTo(len*.68,w*.55,len*.12,w*.36);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle='rgba(235,249,255,.25)';ctx.lineWidth=Math.max(.5,w*.025);for(let i=1;i<4;i++){ctx.beginPath();ctx.moveTo(len*.08,0);ctx.lineTo(len*(.24+i*.18),-w*(.42-i*.08));ctx.stroke();}
  ctx.restore();
}

function drawVeilRipper(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r,color,Math.sin(t*2.4)*.04,.82);
  wing(ctx,-r*.18,-r*.16,r*.72,r*.45,-2.55,color,t*5);wing(ctx,r*.12,-r*.14,r*.76,r*.42,-.55,color,t*5+.6);
  for(const x of [-r*.28,r*.20]){
    limb(ctx,x,-r*.34,r*.60,-.82,color,t*3+(x<0?0:1),.62);
    limb(ctx,x, r*.34,r*.60, .82,color,t*3+(x<0?1:2),.62);
  }
  eye(ctx,r*.30,-r*.12,r*.13,color,-.08);core(ctx,-r*.16,r*.10,r*.09,color);
}
function drawGraveLeech(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  for(let i=0;i<7;i++){
    const q=i/6,x=-r*.62+q*r*1.22,y=Math.sin(t*3.2+q*4.8)*r*.14;
    ctx.save();ctx.translate(x,y);ctx.rotate(Math.sin(t*3.0+q*3.2)*.14);
    body(ctx,r*(.34-.025*q),color,0,.72);ctx.restore();
    if(i<6) joint(ctx,x+r*.14,y,r*.07,color);
  }
  ctx.save();ctx.translate(r*.55,Math.sin(t*3.2+5)*r*.10);
  body(ctx,r*.34,color,.05,.66);eye(ctx,r*.14,-r*.10,r*.09,color);eye(ctx,r*.14,r*.10,r*.09,color);ctx.restore();
  for(let i=0;i<3;i++){
    const x=-r*.20-i*r*.18;
    limb(ctx,x,-r*.22,r*.30,-1.1,color,t*4+i,.34);limb(ctx,x,r*.22,r*.30,1.1,color,t*4+i+1,.34);
  }
}
function drawFangedCoil(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  // Head leads at +X; the body follows behind in a controlled sine wave.
  for(let i=0;i<8;i++){
    const q=i/7;
    const x=r*.46-q*r*1.10;
    const y=Math.sin(t*3.1+q*4.8)*r*(.08+.06*q);
    const dx=-r*1.10/7;
    const dy=Math.cos(t*3.1+q*4.8)*r*(.08+.06*q)*4.8/7;
    const a=Math.atan2(dy,dx);
    ctx.save();ctx.translate(x,y);body(ctx,r*(.29-.018*i),color,a,.64);ctx.restore();
    if(i<7) joint(ctx,x+dx*.5,y+dy*.5,r*.055,color);
  }
  ctx.save();ctx.translate(r*.50,-r*.06);body(ctx,r*.36,color,-.06,.62);
  eye(ctx,r*.14,-r*.10,r*.09,color);eye(ctx,r*.14,r*.10,r*.09,color);
  ctx.fillStyle='#02050a';ctx.strokeStyle=color;ctx.lineWidth=1;
  ctx.beginPath();ctx.moveTo(r*.12,0);ctx.lineTo(r*.34,r*.05);ctx.lineTo(r*.12,r*.10);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.restore();
}
function drawCarrionSkitter(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r,color,Math.sin(t*4)*.05,.58);
  for(let i=0;i<3;i++){const y=(i-1)*r*.27;limb(ctx,-r*.38,y,r*.62,Math.PI-.48,color,t*7+i,.62);limb(ctx,r*.18,y,r*.58,.52,color,t*7+i+1,.58);}
  const headX=r*.45+Math.sin(t*4)*r*.03;body(ctx,r*.34,color,-.05,.70);ctx.save();ctx.translate(headX,0);eye(ctx,0,-r*.10,r*.075,color);eye(ctx,0,r*.10,r*.075,color);ctx.restore();
}
function drawUmbralMoth(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  wing(ctx,-r*.08,0,r*.92,r*.70,-2.72,color,t*8);wing(ctx,r*.08,0,r*.92,r*.70,-.42,color,t*8+.7);
  ctx.save();ctx.scale(.45,1.25);body(ctx,r*.72,color,0,.9);ctx.restore();
  limb(ctx,-r*.14,-r*.34,r*.48,-1.05,color,t*8,.36);limb(ctx,r*.10,-r*.34,r*.48,-1.05,color,t*8+1,.36);
  limb(ctx,-r*.14, r*.34,r*.48, 1.05,color,t*8+2,.36);limb(ctx,r*.10, r*.34,r*.48, 1.05,color,t*8+3,.36);
  eye(ctx,-r*.07,-r*.26,r*.09,color);eye(ctx,r*.07,-r*.26,r*.09,color);
}
function drawRiftScarab(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r,color,Math.sin(t*1.6)*.03,.68);
  ctx.save();ctx.strokeStyle='rgba(235,249,255,.28)';ctx.lineWidth=Math.max(.6,r*.025);for(let i=-1;i<=1;i++){ctx.beginPath();ctx.moveTo(-r*.52,i*r*.12);ctx.quadraticCurveTo(0,i*r*.22,r*.50,i*r*.08);ctx.stroke();}ctx.restore();
  for(let i=0;i<3;i++){const y=(i-1)*r*.25;limb(ctx,-r*.34,y,r*.55,Math.PI-.55,color,t*3+i,.55);limb(ctx,r*.12,y,r*.48,.50,color,t*3+i+1,.52);}
  eye(ctx,r*.48,-r*.10,r*.07,color);eye(ctx,r*.48,r*.10,r*.07,color);core(ctx,-r*.22,0,r*.08,color);
}
function drawBonebackBrute(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*1.05,color,Math.sin(t*1.7)*.025,.88);
  for(const x of [-.45,-.12,.20,.48]){const bob=Math.sin(t*6+x*4)*.05;limb(ctx,r*x,r*.38+bob,r*.72,x>0?.8:2.35,color,t*6+x,.78);}
  for(let i=0;i<3;i++){ctx.save();ctx.translate(-r*.25+i*r*.22,-r*.40);ctx.rotate(-.1+i*.1);ctx.fillStyle='rgba(6,10,16,.98)';ctx.strokeStyle='rgba(230,240,250,.46)';ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(0,0);ctx.lineTo(r*.10,-r*.30);ctx.lineTo(r*.20,0);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();}
  eye(ctx,r*.36,-r*.17,r*.11,color);eye(ctx,r*.36,r*.17,r*.11,color);
}
function drawGlassHound(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*.92,color,Math.sin(t*2.5)*.04,.62);
  for(const x of [-r*.34,r*.00]){
    limb(ctx,x,-r*.34,r*.60,-.82,color,t*5+x,.48);
    limb(ctx,x, r*.34,r*.60, .82,color,t*5+x+1,.48);
  }
  ctx.save();ctx.translate(r*.48,-r*.10);body(ctx,r*.42,color,-.12,.62);eye(ctx,r*.13,-r*.10,r*.09,color);ctx.restore();
  tail(ctx,-r*.56,r*.08,r*.70,color,t,6,.24);
}
function drawHollowStalker(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  ctx.save();ctx.translate(0,Math.sin(t*4)*r*.06);body(ctx,r*.72,color,0,1.25);ctx.restore();
  for(const side of [-1,1]){limb(ctx,side*r*.18,-r*.20,r*.95,side>0?.25:Math.PI-.25,color,t*5+side,.52);limb(ctx,side*r*.16,r*.20,r*.90,side>0?-.55:Math.PI+.55,color,t*5+side+1,.52);}
  eye(ctx,r*.10,-r*.24,r*.12,color);core(ctx,-r*.12,r*.20,r*.075,color);
}
function drawCableWidow(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*.92,color,0,.72);
  for(let i=0;i<8;i++){const a=i*TAU/8;limb(ctx,Math.cos(a)*r*.28,Math.sin(a)*r*.18,r*.66,a+(i%2?.25:-.25),color,t*4+i,.42);}
  eye(ctx,r*.26,-r*.10,r*.085,'#ff6480');eye(ctx,r*.26,r*.10,r*.085,'#ff6480');core(ctx,-r*.20,0,r*.08,'#ff6480');
}
function bossFrame(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  ctx.save();ctx.strokeStyle='rgba('+rgbOf(color)+',.30)';ctx.lineWidth=Math.max(.8,r*.025);ctx.beginPath();ctx.arc(0,0,r*1.16,t*.2,t*.2+4.9);ctx.stroke();ctx.restore();
}
function drawVoidLancerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);
  body(ctx,r*1.12,color,Math.sin(t*1.7)*.025,.62);
  // Head/front is +X. Wings and legs are paired from the body sides.
  wing(ctx,-r*.18,-r*.48,r*1.00,r*.36,-1.15,color,t*4);
  wing(ctx,-r*.18, r*.48,r*1.00,r*.36, 1.15,color,t*4+.35);
  for(const x of [-r*.30,r*.18]){
    limb(ctx,x,-r*.42,r*.62,-.78,color,t*3+(x<0?0:1),.82);
    limb(ctx,x, r*.42,r*.62, .78,color,t*3+(x<0?1:2),.82);
  }
  ctx.save();ctx.translate(r*.62,0);body(ctx,r*.48,color,0,.58);
  eye(ctx,r*.18,-r*.12,r*.14,color);eye(ctx,r*.18,r*.12,r*.14,color);
  ctx.restore();
  // Tail is anchored to the rear of the body, never to the middle/front.
  tail(ctx,-r*.76,0,r*1.05,color,t,8,.18);
  core(ctx,-r*.12,0,r*.12,color);
}
function drawDreadChargerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);body(ctx,r*1.14,color,Math.sin(t*2)*.025,.84);
  for(const x of [-r*.34,r*.20]){
    limb(ctx,x,-r*.43,r*.82,-.82,color,t*6+(x<0?0:1),.90);
    limb(ctx,x, r*.43,r*.82, .82,color,t*6+(x<0?1:2),.90);
  }
  ctx.save();ctx.translate(r*.50,0);body(ctx,r*.50,color,0,.72);
  eye(ctx,r*.20,-r*.14,r*.15,color);eye(ctx,r*.20,r*.14,r*.15,color);ctx.restore();
  for(const side of [-1,1]){
    wing(ctx,-r*.05,side*r*.50,r*.72,r*.25,side>0?.72:-.72,color,t*3+(side>0?0:.35));
  }
  core(ctx,-r*.30,0,r*.14,color);
}
function drawBroodMatriarchBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);body(ctx,r*1.0,color,Math.sin(t)*.02,.96);
  for(const x of [-r*.34,r*.18]){
    limb(ctx,x,-r*.42,r*1.00,-.82,color,t*3,.95);
    limb(ctx,x, r*.42,r*1.00, .82,color,t*3+1,.95);
  }
  limb(ctx,-r*.18,-r*.46,r*.82,-1.05,color,t*3+2,.78);
  limb(ctx,-r*.18, r*.46,r*.82, 1.05,color,t*3+3,.78);
  ctx.save();ctx.translate(r*.34,-r*.05);body(ctx,r*.52,color,-.05,.76);eye(ctx,r*.20,-r*.12,r*.14,color);eye(ctx,r*.20,r*.12,r*.14,color);ctx.restore();
  for(let i=0;i<5;i++){const a=t*.8+i*TAU/5,x=Math.cos(a)*r*1.10,y=Math.sin(a)*r*.72;ctx.save();ctx.translate(x,y);ctx.scale(.8+.15*Math.sin(t*3+i),1);body(ctx,r*.24,color,a,.9);eye(ctx,0,-r*.03,r*.055,color);ctx.restore();}
  core(ctx,-r*.20,r*.06,r*.13,color);
}
function drawAbyssalLeviathanBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);
  for(let i=0;i<8;i++){const q=i/7,x=-r*.76+i*r*.22,y=Math.sin(t*2.0+i*.72)*r*.16;ctx.save();ctx.translate(x,y);body(ctx,r*(.50-.035*i),color,Math.sin(t+i)*.12,.70);ctx.restore();}
  ctx.save();ctx.translate(r*.54,0);body(ctx,r*.58,color,-.08,.68);eye(ctx,r*.22,-r*.16,r*.15,color);eye(ctx,r*.22,r*.16,r*.15,color);ctx.restore();
  for(const x of [-r*.28,r*.10]){
    limb(ctx,x,-r*.38,r*.82,-.92,color,t*2+(x<0?0:1),.70);
    limb(ctx,x, r*.38,r*.82, .92,color,t*2+(x<0?1:2),.70);
  }
  tail(ctx,-r*.78,0,r*1.12,color,t,9,.16);
  core(ctx,-r*.08,0,r*.12,color);
}

function creature(ctx:CanvasRenderingContext2D,r:number,color:string,t:number,variant:string):void{
  if(variant==='wisp')drawVeilRipper(ctx,r,color,t);
  else if(variant==='leech')drawGraveLeech(ctx,r,color,t);
  else if(variant==='serpent')drawFangedCoil(ctx,r,color,t);
  else if(variant==='skitter')drawCarrionSkitter(ctx,r,color,t);
  else if(variant==='moth')drawUmbralMoth(ctx,r,color,t);
  else if(variant==='beetle')drawRiftScarab(ctx,r,color,t);
  else if(variant==='brute')drawBonebackBrute(ctx,r,color,t);
  else if(variant==='prism')drawGlassHound(ctx,r,color,t);
  else if(variant==='stalker')drawHollowStalker(ctx,r,color,t);
  else drawCableWidow(ctx,r,color,t);
}
function boss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number,type:string):void{
  if(type==='shooter')drawVoidLancerBoss(ctx,r,color,t);
  else if(type==='charger')drawDreadChargerBoss(ctx,r,color,t);
  else if(type==='summoner')drawBroodMatriarchBoss(ctx,r,color,t);
  else drawAbyssalLeviathanBoss(ctx,r,color,t);
}
export function drawEnemyCreature(ctx:CanvasRenderingContext2D,e:{radius:number;color:string;visualVariant?:string},t:number):void{
  const r=e.radius,color=e.color;ctx.save();glow(ctx,r*2.15,color,.065);creature(ctx,r,color,t,e.visualVariant||'wisp');ctx.restore();
}
export function drawBossCreature(ctx:CanvasRenderingContext2D,e:{radius:number;color:string;bossType:string},t:number):void{
  const r=e.radius,color=e.color;ctx.save();glow(ctx,r*2.55,color,.085);boss(ctx,r,color,t,e.bossType);ctx.restore();
}
