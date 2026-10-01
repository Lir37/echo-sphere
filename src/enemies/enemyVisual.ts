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
function segmentBetween(ctx:CanvasRenderingContext2D,x1:number,y1:number,x2:number,y2:number,w:number,color:string,alpha=.9):void{
  const dx=x2-x1,dy=y2-y1;
  seg(ctx,x1,y1,Math.hypot(dx,dy),w,Math.atan2(dy,dx),color,alpha);
}
function articulatedLeg(ctx:CanvasRenderingContext2D,hipX:number,hipY:number,len:number,side:number,phase:number,color:string,thick=1):void{
  // Local +X is forward. The hip/knee/ankle stay on one anatomical side;
  // only the stride travels along +X/-X, producing a readable gait.
  const stride=Math.sin(phase)*len*.30;
  const lift=Math.max(0,Math.cos(phase))*len*.12;
  const kneeX=hipX+stride*.46+len*.05;
  const kneeY=hipY+side*(len*.34-lift);
  const ankleX=hipX+stride;
  const ankleY=hipY+side*(len*.76-lift*.45);
  segmentBetween(ctx,hipX,hipY,kneeX,kneeY,thick*5.8,color,.94);
  joint(ctx,kneeX,kneeY,thick*3.5,color);
  segmentBetween(ctx,kneeX,kneeY,ankleX,ankleY,thick*4.0,color,.90);
  joint(ctx,ankleX,ankleY,thick*1.9,color);
  const footX=ankleX+len*.15;
  const footY=ankleY+side*len*.035;
  segmentBetween(ctx,ankleX,ankleY,footX,footY,thick*1.45,color,.92);
}
function pairedLegs(ctx:CanvasRenderingContext2D,r:number,xs:number[],bodySideY:number,len:number,phase:number,color:string,thick=1):void{
  for(let i=0;i<xs.length;i++){
    const x=xs[i]*r;
    for(const side of [-1,1]){
      const sidePhase=phase+i*1.05+(side>0?0:Math.PI);
      articulatedLeg(ctx,x,side*bodySideY*r,len*r,side,sidePhase,color,thick);
    }
  }
}
function tail(ctx:CanvasRenderingContext2D,sx:number,sy:number,len:number,color:string,t:number,segments=7,amp=.18):void{
  // Tail origin is explicitly at the rear (-X). Every segment continues rearward,
  // so the tail cannot grow out of the torso or sweep sideways from the anchor.
  let px=sx,py=sy;
  for(let i=0;i<segments;i++){
    const q0=i/segments,q1=(i+1)/segments;
    const x= sx-len*q1;
    const y= sy+Math.sin(t*3.0-q1*4.2)*len*amp*(.30+.70*q1);
    const w=len*(.14-.009*i);
    segmentBetween(ctx,px,py,x,y,w,color,.82-.025*i);
    if(i<segments-1)joint(ctx,x,y,Math.max(.9,w*.22),color);
    px=x;py=y;
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
  wing(ctx,-r*.18,-r*.38,r*.82,r*.34,-1.15,color,t*5);
  wing(ctx,-r*.18, r*.38,r*.82,r*.34, 1.15,color,t*5);
  pairedLegs(ctx,r,[-.28,.20],.30,.58,t*3,color,.60);
  eye(ctx,r*.30,-r*.12,r*.13,color,-.08);core(ctx,-r*.16,r*.10,r*.09,color);
}\nfunction drawGraveLeech(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  for(let i=0;i<7;i++){
    const q=i/6,x=-r*.62+q*r*1.22,y=Math.sin(t*3.2+q*4.8)*r*.14;
    ctx.save();ctx.translate(x,y);ctx.rotate(Math.sin(t*3.0+q*3.2)*.14);
    body(ctx,r*(.34-.025*q),color,0,.72);ctx.restore();
    if(i<6)joint(ctx,x+r*.14,y,r*.07,color);
  }
  ctx.save();ctx.translate(r*.55,Math.sin(t*3.2+5)*r*.10);
  body(ctx,r*.34,color,.05,.66);eye(ctx,r*.14,-r*.10,r*.09,color);eye(ctx,r*.14,r*.10,r*.09,color);ctx.restore();
  pairedLegs(ctx,r,[-.36,-.02],.20,.30,t*4,color,.34);
}\nfunction drawFangedCoil(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
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
  pairedLegs(ctx,r,[-.40,-.04,.32],.22,.58,t*7,color,.62);
  ctx.save();ctx.translate(r*.45,0);body(ctx,r*.34,color,-.05,.70);eye(ctx,0,-r*.10,r*.075,color);eye(ctx,0,r*.10,r*.075,color);ctx.restore();
}\nfunction drawUmbralMoth(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  wing(ctx,-r*.08,-r*.40,r*.92,r*.62,-2.72,color,t*8);
  wing(ctx,-r*.08, r*.40,r*.92,r*.62, 2.72,color,t*8);
  ctx.save();ctx.scale(.45,1.25);body(ctx,r*.72,color,0,.9);ctx.restore();
  pairedLegs(ctx,r,[-.16,.10],.28,.48,t*8,color,.36);
  eye(ctx,-r*.07,-r*.26,r*.09,color);eye(ctx,r*.07,-r*.26,r*.09,color);
}\nfunction drawRiftScarab(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r,color,Math.sin(t*1.6)*.03,.68);
  ctx.save();ctx.strokeStyle='rgba(235,249,255,.28)';ctx.lineWidth=Math.max(.6,r*.025);for(let i=-1;i<=1;i++){ctx.beginPath();ctx.moveTo(-r*.52,i*r*.12);ctx.quadraticCurveTo(0,i*r*.22,r*.50,i*r*.08);ctx.stroke();}ctx.restore();
  pairedLegs(ctx,r,[-.34,.12],.24,.55,t*3,color,.55);
  eye(ctx,r*.48,-r*.10,r*.07,color);eye(ctx,r*.48,r*.10,r*.07,color);core(ctx,-r*.22,0,r*.08,color);
}\nfunction drawBonebackBrute(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*1.05,color,Math.sin(t*1.7)*.025,.88);
  pairedLegs(ctx,r,[-.45,-.12,.20,.48],.30,.72,t*6,color,.78);
  for(let i=0;i<3;i++){ctx.save();ctx.translate(-r*.25+i*r*.22,-r*.40);ctx.rotate(-.1+i*.1);ctx.fillStyle='rgba(6,10,16,.98)';ctx.strokeStyle='rgba(230,240,250,.46)';ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(0,0);ctx.lineTo(r*.10,-r*.30);ctx.lineTo(r*.20,0);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();}
  eye(ctx,r*.36,-r*.17,r*.11,color);eye(ctx,r*.36,r*.17,r*.11,color);
}\nfunction drawGlassHound(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*.92,color,Math.sin(t*2.5)*.04,.62);
  pairedLegs(ctx,r,[-.34,.10],.30,.60,t*5,color,.48);
  ctx.save();ctx.translate(r*.48,-r*.10);body(ctx,r*.42,color,-.12,.62);eye(ctx,r*.13,-r*.10,r*.09,color);ctx.restore();
  tail(ctx,-r*.76,0,r*.78,color,t,6,.20);
}\nfunction drawHollowStalker(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  ctx.save();ctx.translate(0,Math.sin(t*4)*r*.06);body(ctx,r*.72,color,0,1.25);ctx.restore();
  pairedLegs(ctx,r,[-.22,.16],.24,.90,t*5,color,.52);
  eye(ctx,r*.10,-r*.24,r*.12,color);core(ctx,-r*.12,r*.20,r*.075,color);
}\nfunction drawCableWidow(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  body(ctx,r*.92,color,0,.72);
  for(let i=0;i<4;i++){
    const a=-.72+i*.48;
    const x=(-.30+i*.20)*r;
    const side=i%2===0?-1:1;
    articulatedLeg(ctx,x,side*r*.18,r*.66,side,t*4+i,color,.42);
  }
  // Explicitly mirrored outer pair keeps the silhouette readable.
  pairedLegs(ctx,r,[-.18,.20],.18,.58,t*4,color,.40);
  eye(ctx,r*.26,-r*.10,r*.085,'#ff6480');eye(ctx,r*.26,r*.10,r*.085,'#ff6480');core(ctx,-r*.20,0,r*.08,'#ff6480');
}\nfunction bossFrame(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  ctx.save();ctx.strokeStyle='rgba('+rgbOf(color)+',.30)';ctx.lineWidth=Math.max(.8,r*.025);ctx.beginPath();ctx.arc(0,0,r*1.16,t*.2,t*.2+4.9);ctx.stroke();ctx.restore();
}
function drawVoidLancerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);
  body(ctx,r*1.12,color,Math.sin(t*1.7)*.025,.62);
  // +X is the head/front. Wings and legs mirror across the body centerline.
  wing(ctx,-r*.18,-r*.48,r*1.00,r*.36,-1.15,color,t*4);
  wing(ctx,-r*.18, r*.48,r*1.00,r*.36, 1.15,color,t*4+.35);
  pairedLegs(ctx,r,[-.30,.18],.34,.62,t*3,color,.82);
  ctx.save();ctx.translate(r*.62,0);body(ctx,r*.48,color,0,.58);
  eye(ctx,r*.18,-r*.12,r*.14,color);eye(ctx,r*.18,r*.12,r*.14,color);ctx.restore();
  tail(ctx,-r*.90,0,r*1.10,color,t,8,.16);
  core(ctx,-r*.12,0,r*.12,color);
}\nfunction drawDreadChargerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);body(ctx,r*1.14,color,Math.sin(t*2)*.025,.84);
  pairedLegs(ctx,r,[-.34,.20],.35,.82,t*6,color,.90);
  ctx.save();ctx.translate(r*.50,0);body(ctx,r*.50,color,0,.72);
  eye(ctx,r*.20,-r*.14,r*.15,color);eye(ctx,r*.20,r*.14,r*.15,color);ctx.restore();
  for(const side of [-1,1]){
    wing(ctx,-r*.05,side*r*.50,r*.72,r*.25,side>0?.72:-.72,color,t*3+(side>0?0:.35));
  }
  core(ctx,-r*.30,0,r*.14,color);
}\nfunction drawBroodMatriarchBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);body(ctx,r*1.0,color,Math.sin(t)*.02,.96);
  pairedLegs(ctx,r,[-.34,.18],.38,1.00,t*3,color,.95);
  ctx.save();ctx.translate(r*.34,-r*.05);body(ctx,r*.52,color,-.05,.76);eye(ctx,r*.20,-r*.12,r*.14,color);eye(ctx,r*.20,r*.12,r*.14,color);ctx.restore();
  for(let i=0;i<5;i++){const a=t*.8+i*TAU/5,x=Math.cos(a)*r*1.10,y=Math.sin(a)*r*.72;ctx.save();ctx.translate(x,y);ctx.scale(.8+.15*Math.sin(t*3+i),1);body(ctx,r*.24,color,a,.9);eye(ctx,0,-r*.03,r*.055,color);ctx.restore();}
  core(ctx,-r*.20,r*.06,r*.13,color);
}\nfunction drawAbyssalLeviathanBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  bossFrame(ctx,r,color,t);
  for(let i=0;i<8;i++){const q=i/7,x=-r*.76+i*r*.22,y=Math.sin(t*2.0+i*.72)*r*.16;ctx.save();ctx.translate(x,y);body(ctx,r*(.50-.035*i),color,Math.sin(t+i)*.12,.70);ctx.restore();}
  ctx.save();ctx.translate(r*.54,0);body(ctx,r*.58,color,-.08,.68);eye(ctx,r*.22,-r*.16,r*.15,color);eye(ctx,r*.22,r*.16,r*.15,color);ctx.restore();
  pairedLegs(ctx,r,[-.28,.10],.32,.82,t*2,color,.70);
  tail(ctx,-r*.90,0,r*1.16,color,t,9,.14);
  core(ctx,-r*.08,0,r*.12,color);
}\nfunction creature(ctx:CanvasRenderingContext2D,r:number,color:string,t:number,variant:string):void{
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
