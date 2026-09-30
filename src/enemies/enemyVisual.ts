const TAU = Math.PI * 2;

function rgbOf(hex:string):string {
  const n=Number.parseInt(hex.slice(1),16);
  return ((n>>16)&255)+','+((n>>8)&255)+','+(n&255);
}

function glow(ctx:CanvasRenderingContext2D,r:number,color:string,alpha=.10):void {
  const rgb=rgbOf(color);
  const g=ctx.createRadialGradient(0,0,0,0,0,r);
  g.addColorStop(0,'rgba('+rgb+','+(alpha*3.0).toFixed(3)+')');
  g.addColorStop(.32,'rgba('+rgb+','+(alpha*1.1).toFixed(3)+')');
  g.addColorStop(.72,'rgba('+rgb+','+(alpha*.28).toFixed(3)+')');
  g.addColorStop(1,'rgba('+rgb+',0)');
  ctx.save();ctx.globalCompositeOperation='lighter';ctx.fillStyle=g;
  ctx.beginPath();ctx.arc(0,0,r,0,TAU);ctx.fill();ctx.restore();
}

function glow(ctx:CanvasRenderingContext2D,r:number,color:string,alpha=.10):void {
  const rgb=rgbOf(color);
  const g=ctx.createRadialGradient(0,0,0,0,0,r);
  g.addColorStop(0,'rgba('+rgb+','+(alpha*3).toFixed(3)+')');
  g.addColorStop(.32,'rgba('+rgb+','+(alpha*1.1).toFixed(3)+')');
  g.addColorStop(.72,'rgba('+rgb+','+(alpha*.28).toFixed(3)+')');
  g.addColorStop(1,'rgba('+rgb+',0)');
  ctx.save();
  ctx.globalCompositeOperation='lighter';
  ctx.fillStyle=g;
  ctx.beginPath();ctx.arc(0,0,r,0,TAU);ctx.fill();
  ctx.restore();
}

function plate(ctx:CanvasRenderingContext2D,x:number,y:number,w:number,h:number,color:string,angle:number,alpha=.86):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);
  ctx.globalAlpha=alpha;
  ctx.fillStyle='rgba(5,9,16,.95)';
  ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,w*.07);
  ctx.beginPath();
  ctx.moveTo(-w*.46,-h*.28);
  ctx.quadraticCurveTo(0,-h*.58,w*.46,-h*.20);
  ctx.lineTo(w*.33,h*.30);
  ctx.quadraticCurveTo(0,h*.50,-w*.38,h*.22);
  ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle='rgba(236,249,255,.27)';ctx.lineWidth=Math.max(.45,w*.025);
  ctx.beginPath();ctx.moveTo(-w*.24,-h*.12);ctx.quadraticCurveTo(0,-h*.30,w*.23,-h*.06);ctx.stroke();
  ctx.restore();
}

function fin(ctx:CanvasRenderingContext2D,x:number,y:number,len:number,w:number,color:string,angle:number,alpha=.8):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);
  ctx.globalAlpha=alpha;ctx.fillStyle='rgba(4,9,17,.92)';ctx.strokeStyle=color;
  ctx.lineWidth=Math.max(.65,w*.07);
  ctx.beginPath();
  ctx.moveTo(0,-w*.18);
  ctx.quadraticCurveTo(len*.40,-w*.82,len,-w*.08);
  ctx.quadraticCurveTo(len*.48,w*.32,0,w*.24);
  ctx.closePath();ctx.fill();ctx.stroke();
  ctx.restore();
}

function claw(ctx:CanvasRenderingContext2D,x:number,y:number,len:number,color:string,angle:number,alpha=.8):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);
  ctx.strokeStyle=color;ctx.lineWidth=Math.max(.8,len*.10);ctx.lineCap='round';ctx.globalAlpha=alpha;
  ctx.beginPath();ctx.moveTo(0,0);ctx.quadraticCurveTo(len*.55,-len*.30,len*.92,-len*.08);ctx.quadraticCurveTo(len*.58,len*.32,len*.18,len*.22);ctx.stroke();
  ctx.restore();
}

function eye(ctx:CanvasRenderingContext2D,x:number,y:number,size:number,color:string,angle:number):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);
  ctx.globalCompositeOperation='lighter';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.65,size*.13);ctx.globalAlpha=.92;
  ctx.beginPath();ctx.moveTo(-size,0);ctx.quadraticCurveTo(0,-size*.55,size,0);ctx.quadraticCurveTo(0,size*.55,-size,0);ctx.stroke();
  ctx.fillStyle='#f9feff';ctx.globalAlpha=.86;ctx.beginPath();ctx.ellipse(0,0,size*.28,size*.50,0,0,TAU);ctx.fill();
  ctx.fillStyle=color;ctx.globalAlpha=1;ctx.beginPath();ctx.ellipse(0,0,size*.12,size*.36,0,0,TAU);ctx.fill();
  ctx.restore();
}

function teeth(ctx:CanvasRenderingContext2D,x:number,y:number,w:number,count:number,color:string,alpha=.82):void {
  ctx.save();ctx.translate(x,y);ctx.globalAlpha=alpha;ctx.strokeStyle=color;ctx.lineWidth=Math.max(.65,w*.045);
  for(let i=0;i<count;i++){
    const p=i/Math.max(1,count-1);
    ctx.beginPath();ctx.moveTo(-w*.5+p*w,0);ctx.lineTo(-w*.45+p*w,-w*.18-(i%2)*w*.04);ctx.stroke();
  }
  ctx.restore();
}

function tendril(ctx:CanvasRenderingContext2D,sx:number,sy:number,ex:number,ey:number,bend:number,color:string,width:number,alpha:number,phase:number):void {
  const dx=ex-sx,dy=ey-sy,len=Math.hypot(dx,dy)||1,nx=-dy/len,ny=dx/len;
  ctx.save();ctx.strokeStyle=color;ctx.lineWidth=width;ctx.lineCap='round';ctx.globalAlpha=alpha;
  ctx.beginPath();ctx.moveTo(sx,sy);
  ctx.quadraticCurveTo((sx+ex)/2+nx*(bend+Math.sin(phase)*width*2.5),(sy+ey)/2+ny*(bend+Math.sin(phase)*width*2.5),ex,ey);
  ctx.stroke();ctx.restore();
}

function ribs(ctx:CanvasRenderingContext2D,x:number,y:number,r:number,color:string,angle:number,count=4):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);ctx.strokeStyle='rgba(235,249,255,.30)';ctx.lineWidth=Math.max(.5,r*.025);
  for(let i=0;i<count;i++){
    const q=i/(count-1||1)-.5;
    ctx.beginPath();
    ctx.moveTo(-r*.22,q*r*.65);
    ctx.quadraticCurveTo(0,q*r*.88,r*.24,q*r*.62);
    ctx.stroke();
  }
  ctx.restore();
}

function mouth(ctx:CanvasRenderingContext2D,x:number,y:number,w:number,h:number,color:string,angle:number):void {
  ctx.save();ctx.translate(x,y);ctx.rotate(angle);
  ctx.fillStyle='rgba(2,4,7,.98)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.7,w*.06);
  ctx.beginPath();ctx.ellipse(0,0,w,h,0,0,TAU);ctx.fill();ctx.stroke();
  teeth(ctx,0,-h*.22,w*.82,6,'rgba(240,248,255,.75)',.75);
  ctx.restore();
}

function segmentedTail(ctx:CanvasRenderingContext2D,x:number,y:number,len:number,color:string,time:number,segments=6):void {
  for(let i=0;i<segments;i++){
    const q=i/segments;
    const a=time*.9+q*1.9;
    const px=x+Math.cos(a)*len*(q-.25);
    const py=y+Math.sin(a)*len*(q-.25)*.55;
    plate(ctx,px,py,len*(.20-.07*q),len*(.16-.05*q),color,a,.78);
  }
}

function drawVeilRipper(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  const breathe=1+Math.sin(t*2.7)*.035;
  ctx.save();ctx.scale(breathe,breathe);
  const rgb=rgbOf(color);
  ctx.fillStyle='rgba(4,9,17,.96)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.9,r*.055);
  ctx.beginPath();
  ctx.moveTo(-r*.68,-r*.15);ctx.quadraticCurveTo(-r*.42,-r*.72,r*.08,-r*.58);
  ctx.quadraticCurveTo(r*.62,-r*.50,r*.78,-r*.06);
  ctx.quadraticCurveTo(r*.42,r*.28,r*.02,r*.52);
  ctx.quadraticCurveTo(-r*.50,r*.46,-r*.72,r*.08);ctx.closePath();ctx.fill();ctx.stroke();
  fin(ctx,-r*.08,-r*.42,r*.52,r*.22,color,-.62,.62);fin(ctx,r*.25,-r*.18,r*.46,r*.18,color,.48,.72);
  ribs(ctx,r*.02,r*.03,r*.43,color,-.18,5);
  eye(ctx,r*.26,-r*.10,r*.16,color,-.1);
  for(let i=0;i<4;i++){
    const a=-1.1+i*.72;
    tendril(ctx,-r*.34+i*r*.18,r*.24,Math.cos(a)*r*.84,Math.sin(a)*r*.62,r*.22,color,Math.max(.8,r*.035),.68,t*1.2+i);
  }
  ctx.strokeStyle='rgba('+rgb+',.32)';ctx.lineWidth=Math.max(.55,r*.025);
  ctx.beginPath();ctx.arc(0,0,r*.86,-1.0,.62);ctx.stroke();
  ctx.restore();
}

function drawGraveLeech(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();ctx.rotate(Math.sin(t*1.4)*.08);
  const n=7;
  for(let i=0;i<n;i++){
    const q=i/(n-1),x=r*(.60-q*1.18),y=Math.sin(t*2.1+q*4.2)*r*.13;
    plate(ctx,x,y,r*(.32-.05*q),r*(.24-.025*q),color,q*.8+t*.3,.88);
  }
  mouth(ctx,r*.64,0,r*.25,r*.17,color,0);
  eye(ctx,r*.43,-r*.10,r*.095,color,-.25);
  eye(ctx,r*.43,r*.10,r*.095,color,.25);
  for(let i=0;i<3;i++){
    const x=-r*.12-i*r*.20;
    fin(ctx,x,-r*.19,r*.18,r*.09,color,-.8,.62);
    fin(ctx,x,r*.19,r*.18,r*.09,color,.8,.62);
  }
  segmentedTail(ctx,-r*.58,0,r*.48,color,t,5);
  ctx.restore();
}

function drawFangedCoil(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  for(let i=0;i<8;i++){
    const q=i/7,a=t*.92+q*5.3,x=Math.cos(a)*r*(.22+.10*q),y=Math.sin(a)*r*(.48+.05*q);
    plate(ctx,x,y,r*(.28-.018*i),r*(.18-.01*i),color,a+.5,.86);
  }
  ctx.save();ctx.translate(r*.30,-r*.26);ctx.rotate(-.34);
  ctx.fillStyle='rgba(4,8,15,.97)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.9,r*.055);
  ctx.beginPath();ctx.moveTo(-r*.30,0);ctx.quadraticCurveTo(0,-r*.25,r*.42,-r*.02);ctx.quadraticCurveTo(r*.18,r*.22,-r*.30,r*.16);ctx.closePath();ctx.fill();ctx.stroke();
  mouth(ctx,r*.14,r*.02,r*.21,r*.09,color,0);eye(ctx,r*.22,-r*.10,r*.10,color,-.15);
  ctx.restore();
  for(let i=0;i<4;i++) fin(ctx,-r*.18+i*r*.10,r*(i%2?-.34:.34),r*.20,r*.08,color,(i%2?-.8:.8),.62);
  ctx.restore();
}

function drawCarrionSkitter(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  plate(ctx,-r*.05,0,r*1.05,r*.56,color,Math.sin(t*2)*.04,.96);
  plate(ctx,r*.26,-r*.05,r*.55,r*.36,color,.10,.82);
  plate(ctx,-r*.25,-r*.03,r*.48,r*.30,color,-.12,.82);
  eye(ctx,r*.33,-r*.12,r*.085,color,-.2);eye(ctx,r*.33,r*.12,r*.085,color,.2);
  for(let i=0;i<3;i++){
    const y=(i-1)*r*.28;
    claw(ctx,-r*.30,y,r*.46,color,Math.PI-.45,.88);
    claw(ctx,r*.05,y,r*.44,color,.45,.78);
  }
  for(let i=0;i<4;i++){
    const a=t*.35+i*TAU/4;
    fin(ctx,Math.cos(a)*r*.30,Math.sin(a)*r*.18,r*.30,r*.08,color,a,.55);
  }
  ctx.restore();
}

function drawUmbralMoth(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  const wingPulse=.92+.08*Math.sin(t*5.0);
  for(const side of [-1,1]){
    const s=side;
    ctx.fillStyle='rgba(4,10,18,.94)';ctx.strokeStyle=color;ctx.globalAlpha=.92;ctx.lineWidth=Math.max(.8,r*.045);
    ctx.beginPath();
    ctx.moveTo(0,-r*.06);
    ctx.quadraticCurveTo(s*r*.50,-r*.72*wingPulse,s*r*.82,-r*.12);
    ctx.quadraticCurveTo(s*r*.68,r*.18,s*r*.34,r*.38);
    ctx.quadraticCurveTo(s*r*.18,r*.24,0,r*.08);ctx.closePath();ctx.fill();ctx.stroke();
    ctx.strokeStyle='rgba(236,249,255,.30)';ctx.lineWidth=Math.max(.55,r*.025);
    for(let i=0;i<3;i++){
      const q=(i+1)/4;
      ctx.beginPath();ctx.moveTo(0,0);ctx.quadraticCurveTo(s*r*(.28+.12*q),-r*(.42-.08*q),s*r*(.64-.10*q),-r*(.04-.08*q));ctx.stroke();
    }
  }
  ctx.fillStyle='rgba(3,7,13,.98)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.9,r*.055);
  ctx.beginPath();ctx.ellipse(0,r*.05,r*.18,r*.52,0,0,TAU);ctx.fill();ctx.stroke();
  eye(ctx,-r*.06,-r*.09,r*.10,color,0);eye(ctx,r*.06,-r*.09,r*.10,color,0);
  fin(ctx,-r*.11,-r*.56,r*.38,r*.07,color,-2.0,.64);fin(ctx,r*.11,-r*.56,r*.38,r*.07,color,-1.15,.64);
  ctx.restore();
}

function drawRiftScarab(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();ctx.rotate(Math.sin(t*1.7)*.03);
  plate(ctx,-r*.06,0,r*1.08,r*.62,color,0,.98);
  plate(ctx,r*.32,0,r*.46,r*.40,color,.02,.80);
  ctx.strokeStyle='rgba(238,251,255,.34)';ctx.lineWidth=Math.max(.6,r*.028);
  ctx.beginPath();ctx.moveTo(-r*.48,0);ctx.quadraticCurveTo(0,r*.10,r*.50,0);ctx.stroke();
  mouth(ctx,r*.60,-r*.02,r*.22,r*.11,color,0);
  for(const y of [-.22,0,.22]){
    claw(ctx,-r*.34,y*r,r*.42,color,Math.PI-.55,.85);
    claw(ctx,r*.02,y*r,r*.38,color,.50,.72);
  }
  eye(ctx,r*.34,-r*.12,r*.075,color,-.15);eye(ctx,r*.34,r*.12,r*.075,color,.15);
  ctx.restore();
}

function drawBonebackBrute(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  ctx.translate(-r*.04,0);
  plate(ctx,0,0,r*1.24,r*.82,color,Math.sin(t*1.2)*.03,.98);
  plate(ctx,-r*.34,-r*.30,r*.48,r*.32,color,-.35,.80);
  plate(ctx,r*.30,-r*.28,r*.46,r*.34,color,.32,.80);
  fin(ctx,-r*.32,r*.28,r*.42,r*.14,color,-2.6,.82);
  fin(ctx,r*.34,r*.28,r*.42,r*.14,color,-.55,.82);
  mouth(ctx,r*.54,0,r*.26,r*.20,color,0);
  eye(ctx,r*.28,-r*.20,r*.12,color,-.16);eye(ctx,r*.28,r*.20,r*.12,color,.16);
  for(const x of [-.42,-.10,.22,.48]) fin(ctx,r*x,r*.34,r*.34,r*.11,color,(x>0?-.95:-2.2),.70);
  ctx.restore();
}

function drawGlassHound(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  ctx.rotate(Math.sin(t*1.8)*.025);
  ctx.fillStyle='rgba(4,8,15,.96)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.9,r*.05);
  ctx.beginPath();
  ctx.moveTo(-r*.62,r*.12);ctx.quadraticCurveTo(-r*.30,-r*.40,r*.18,-r*.30);
  ctx.quadraticCurveTo(r*.56,-r*.24,r*.72,r*.02);
  ctx.quadraticCurveTo(r*.42,r*.36,-r*.02,r*.35);
  ctx.quadraticCurveTo(-r*.40,r*.33,-r*.62,r*.12);ctx.closePath();ctx.fill();ctx.stroke();
  fin(ctx,-r*.34,-r*.18,r*.35,r*.12,color,-1.15,.76);fin(ctx,r*.02,-r*.28,r*.42,r*.13,color,-.38,.78);
  mouth(ctx,r*.56,r*.03,r*.22,r*.13,color,-.05);
  eye(ctx,r*.31,-r*.12,r*.12,color,-.1);
  claw(ctx,-r*.18,r*.28,r*.48,color,1.95,.74);claw(ctx,r*.28,r*.27,r*.50,color,1.15,.74);
  ctx.strokeStyle='rgba(255,255,255,.30)';ctx.lineWidth=Math.max(.45,r*.025);
  ctx.beginPath();ctx.moveTo(-r*.12,-r*.14);ctx.lineTo(r*.18,r*.12);ctx.lineTo(r*.40,-r*.04);ctx.stroke();
  ctx.restore();
}

function drawHollowStalker(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();ctx.translate(0,Math.sin(t*3)*r*.045);
  ctx.fillStyle='rgba(3,7,13,.97)';ctx.strokeStyle=color;ctx.lineWidth=Math.max(.8,r*.045);
  ctx.beginPath();ctx.moveTo(-r*.18,-r*.58);ctx.quadraticCurveTo(r*.10,-r*.72,r*.27,-r*.46);ctx.lineTo(r*.18,r*.22);ctx.quadraticCurveTo(r*.48,r*.54,r*.33,r*.70);ctx.lineTo(r*.02,r*.36);ctx.lineTo(-r*.30,r*.72);ctx.quadraticCurveTo(-r*.44,r*.52,-r*.18,r*.20);ctx.closePath();ctx.fill();ctx.stroke();
  ribs(ctx,0,-r*.06,r*.30,color,0,4);
  eye(ctx,r*.08,-r*.34,r*.13,color,0);
  for(const side of [-1,1]){
    const s=side;
    claw(ctx,s*r*.11,r*.02,r*.68,color,s>0?.45:Math.PI-.45,.86);
    claw(ctx,s*r*.16,r*.18,r*.56,color,s>0?.7:Math.PI-.7,.72);
  }
  fin(ctx,-r*.12,-r*.58,r*.28,r*.08,color,-1.8,.68);fin(ctx,r*.12,-r*.58,r*.28,r*.08,color,-1.35,.68);
  ctx.restore();
}

function drawCableWidow(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  ctx.save();
  plate(ctx,0,0,r*1.00,r*.62,color,0,.98);
  plate(ctx,-r*.20,0,r*.48,r*.34,color,-.12,.80);
  ctx.strokeStyle='rgba(255,87,116,.70)';ctx.lineWidth=Math.max(.9,r*.045);
  for(let i=0;i<4;i++){const a=-.9+i*.60;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.28,Math.sin(a)*r*.18);ctx.lineTo(Math.cos(a)*r*.68,Math.sin(a)*r*.54);ctx.stroke();}
  eye(ctx,r*.26,-r*.10,r*.09,'#ff6480',-.15);eye(ctx,r*.26,r*.10,r*.09,'#ff6480',.15);
  for(let i=0;i<6;i++){
    const a=t*.30+i*TAU/6;
    claw(ctx,Math.cos(a)*r*.26,Math.sin(a)*r*.18,r*.52,color,a+.35,.82);
  }
  ctx.restore();
}

function bossFrame(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  const rgb=rgbOf(color);
  ctx.save();
  ctx.globalAlpha=.18+.08*Math.sin(t*2.2);
  ctx.strokeStyle='rgba('+rgb+',1)';ctx.lineWidth=Math.max(.8,r*.03);
  ctx.beginPath();ctx.arc(0,0,r*1.14,t*.15,t*.15+4.7);ctx.stroke();
  ctx.restore();
}

function drawVoidLancerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  bossFrame(ctx,r,color,t);ctx.save();
  ctx.translate(r*.04,0);
  plate(ctx,0,0,r*1.48,r*.82,color,.02,.99);
  plate(ctx,r*.28,-r*.16,r*.62,r*.44,color,-.10,.86);
  fin(ctx,-r*.38,-r*.18,r*.80,r*.20,color,-2.35,.82);
  fin(ctx,-r*.20,r*.24,r*.74,r*.18,color,2.45,.80);
  mouth(ctx,r*.62,0,r*.38,r*.23,color,-.02);
  eye(ctx,r*.28,-r*.18,r*.16,color,-.12);eye(ctx,r*.28,r*.18,r*.16,color,.12);
  ribs(ctx,-r*.15,0,r*.52,color,0,6);
  for(let i=0;i<4;i++){
    const a=t*.70+i*TAU/4;
    fin(ctx,Math.cos(a)*r*.62,Math.sin(a)*r*.34,r*.46,r*.13,color,a,.58);
  }
  ctx.restore();
}

function drawDreadChargerBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  bossFrame(ctx,r,color,t);ctx.save();
  const bob=Math.sin(t*2.1)*r*.025;ctx.translate(0,bob);
  plate(ctx,-r*.02,0,r*1.55,r*1.02,color,0,.99);
  plate(ctx,-r*.34,-r*.36,r*.62,r*.44,color,-.18,.84);
  plate(ctx,-r*.34,r*.34,r*.62,r*.44,color,.18,.84);
  fin(ctx,-r*.68,-r*.25,r*.62,r*.20,color,-2.5,.88);fin(ctx,-r*.68,r*.25,r*.62,r*.20,color,2.5,.88);
  mouth(ctx,r*.70,0,r*.36,r*.25,color,0);
  eye(ctx,r*.38,-r*.22,r*.15,color,-.08);eye(ctx,r*.38,r*.22,r*.15,color,.08);
  for(const x of [-.50,-.10,.30]){
    claw(ctx,r*x,-r*.44,r*.54,color,-1.3,.78);
    claw(ctx,r*x,r*.44,r*.54,color,1.3,.78);
  }
  ctx.strokeStyle='rgba(255,237,198,.26)';ctx.lineWidth=Math.max(.6,r*.025);
  ctx.beginPath();ctx.moveTo(-r*.38,0);ctx.quadraticCurveTo(0,-r*.10,r*.38,0);ctx.stroke();
  ctx.restore();
}

function drawBroodMatriarchBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  bossFrame(ctx,r,color,t);ctx.save();
  plate(ctx,-r*.05,r*.04,r*1.36,r*1.08,color,0,.99);
  plate(ctx,r*.20,-r*.26,r*.72,r*.54,color,-.05,.84);
  mouth(ctx,r*.46,r*.04,r*.30,r*.20,color,0);
  for(let i=0;i<4;i++){
    const a=t*.42+i*Math.PI/2;
    const sx=Math.cos(a)*r*.38,sy=Math.sin(a)*r*.28;
    claw(ctx,sx,sy,r*.86,color,a+.55,.88);
  }
  for(let i=0;i<6;i++){
    const a=-1.1+i*.44;
    const x=Math.cos(a)*r*.92,y=Math.sin(a)*r*.72;
    plate(ctx,x,y,r*.30,r*.25,color,a,.70);
    eye(ctx,x,y-r*.01,r*.07,color,a);
  }
  for(let i=0;i<5;i++){
    const a=t*.55+i*TAU/5;
    const x=Math.cos(a)*r*1.12,y=Math.sin(a)*r*.86;
    ctx.fillStyle='rgba(6,12,18,.95)';ctx.strokeStyle=color;ctx.lineWidth=1;
    ctx.beginPath();ctx.ellipse(x,y,r*.12,r*.15,0,0,TAU);ctx.fill();ctx.stroke();
    eye(ctx,x,y,r*.045,color,0);
  }
  ctx.restore();
}

function drawAbyssalLeviathanBoss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void {
  bossFrame(ctx,r,color,t);ctx.save();
  for(let i=0;i<7;i++){
    const q=i/6,x=-r*.70+i*r*.23,y=Math.sin(t*1.2+i*.9)*r*.13;
    plate(ctx,x,y,r*(.48-.03*i),r*(.34-.015*i),color,Math.sin(t+i)*.14,.92);
  }
  plate(ctx,r*.54,-r*.04,r*.66,r*.55,color,-.12,.98);
  mouth(ctx,r*.76,-r*.02,r*.30,r*.20,color,0);
  eye(ctx,r*.61,-r*.20,r*.13,color,-.16);eye(ctx,r*.61,r*.20,r*.13,color,.16);
  for(let i=0;i<5;i++){
    const a=t*.48+i*TAU/5;
    const x=-r*.15+Math.cos(a)*r*.62,y=Math.sin(a)*r*.40;
    fin(ctx,x,y,r*.42,r*.13,color,a+.6,.66);
  }
  for(let i=0;i<4;i++){
    const a=-.75+i*.5;
    tendril(ctx,-r*.36,r*.22,Math.cos(a)*r*.98,Math.sin(a)*r*.72,r*.26,color,Math.max(1,r*.045),.62,t+i);
  }
  ctx.restore();
}

function creature(ctx:CanvasRenderingContext2D,r:number,color:string,t:number,variant:string):void {
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

function boss(ctx:CanvasRenderingContext2D,r:number,color:string,t:number,type:string):void {
  if(type==='shooter')drawVoidLancerBoss(ctx,r,color,t);
  else if(type==='charger')drawDreadChargerBoss(ctx,r,color,t);
  else if(type==='summoner')drawBroodMatriarchBoss(ctx,r,color,t);
  else drawAbyssalLeviathanBoss(ctx,r,color,t);
}

export function drawEnemyCreature(ctx:CanvasRenderingContext2D,e:{radius:number;color:string;visualVariant?:string},t:number):void {
  const r=e.radius,color=e.color,variant=e.visualVariant||'wisp';
  ctx.save();
  ctx.globalCompositeOperation='source-over';
  glow(ctx,r*2.2,color,.055);
  creature(ctx,r,color,t,variant);
  ctx.restore();
}

export function drawBossCreature(ctx:CanvasRenderingContext2D,e:{radius:number;color:string;bossType:string},t:number):void {
  const r=e.radius,color=e.color;
  ctx.save();
  ctx.globalCompositeOperation='source-over';
  glow(ctx,r*2.7,color,.08);
  boss(ctx,r,color,t,e.bossType);
  ctx.restore();
}