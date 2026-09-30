import type { EnemyEntity } from '../engineTypes';

function rgbOf(hex: string): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `${(n >> 16) & 255},${(n >> 8) & 255},${n & 255}`;
}

function glow(ctx: CanvasRenderingContext2D, radius: number, color: string, alpha: number): void {
  const g = ctx.createRadialGradient(0, 0, 0, 0, 0, radius);
  g.addColorStop(0, `rgba(${rgbOf(color)},${alpha})`);
  g.addColorStop(.36, `rgba(${rgbOf(color)},${alpha * .34})`);
  g.addColorStop(1, 'rgba(0,0,0,0)');
  ctx.save(); ctx.globalCompositeOperation = 'lighter'; ctx.fillStyle = g;
  ctx.beginPath(); ctx.arc(0, 0, radius, 0, Math.PI * 2); ctx.fill(); ctx.restore();
}

function eye(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, color: string, pulse: number): void {
  const rgb = rgbOf(color);
  ctx.save(); ctx.globalCompositeOperation = 'lighter';
  ctx.fillStyle = `rgba(${rgb},.20)`; ctx.beginPath(); ctx.arc(x, y, r * (2.5 + pulse * .5), 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = '#f7fdff'; ctx.beginPath(); ctx.arc(x, y, r * .78, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color; ctx.beginPath(); ctx.arc(x, y, r * .48, 0, Math.PI * 2); ctx.fill();
  ctx.restore();
}

function leg(ctx: CanvasRenderingContext2D, r: number, x: number, side: number, phase: number, reach: number): void {
  const swing = Math.sin(phase) * r * .14;
  ctx.beginPath(); ctx.moveTo(x, side * r * .12);
  ctx.lineTo(x + r * .16, side * (r * .46 + swing));
  ctx.lineTo(x + r * .30, side * (r * reach - swing * .35)); ctx.stroke();
}

function tentacle(ctx: CanvasRenderingContext2D, r: number, angle: number, length: number, phase: number): void {
  const bend = Math.sin(phase) * r * .12;
  const ex = Math.cos(angle) * length, ey = Math.sin(angle) * length;
  ctx.beginPath(); ctx.moveTo(Math.cos(angle) * r * .10, Math.sin(angle) * r * .10);
  ctx.quadraticCurveTo(ex * .48 - Math.sin(angle) * bend, ey * .48 + Math.cos(angle) * bend, ex, ey); ctx.stroke();
}

function darkFill(color: string): string { return `rgba(4,10,18,.97)`; }

function drawVeilJelly(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color), bob=Math.sin(t*3.8)*r*.08;
  ctx.save();ctx.translate(0,bob);glow(ctx,r*1.7,color,.14);
  ctx.strokeStyle=`rgba(${rgb},.90)`;ctx.lineWidth=Math.max(1.2,r*.065);ctx.fillStyle=darkFill(color);
  ctx.beginPath();ctx.moveTo(-r*.78,0);ctx.quadraticCurveTo(-r*.70,-r*.64,0,-r*.78);ctx.quadraticCurveTo(r*.70,-r*.64,r*.78,0);ctx.quadraticCurveTo(r*.54,r*.24,r*.28,r*.14);ctx.quadraticCurveTo(0,r*.44,-r*.28,r*.14);ctx.quadraticCurveTo(-r*.54,r*.24,-r*.78,0);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(${rgb},.42)`;ctx.lineWidth=Math.max(1,r*.04);for(let i=0;i<4;i++)tentacle(ctx,r,-1.15+i*.76,r*(.78+.10*(i%2)),t*4+i);
  ctx.strokeStyle=`rgba(225,249,255,.30)`;ctx.lineWidth=Math.max(.7,r*.025);ctx.beginPath();ctx.moveTo(-r*.48,-r*.22);ctx.quadraticCurveTo(0,-r*.52,r*.46,-r*.20);ctx.stroke();
  eye(ctx,r*.30,-r*.08,r*.11,color,.5+.5*Math.sin(t*5));ctx.restore();
}

function drawLeech(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),p=.5+.5*Math.sin(t*7);glow(ctx,r*1.5,color,.11);ctx.save();
  ctx.translate(Math.sin(t*5)*r*.03,Math.cos(t*4)*r*.025);ctx.strokeStyle=`rgba(${rgb},.9)`;ctx.lineWidth=Math.max(1.2,r*.06);ctx.fillStyle=darkFill(color);
  ctx.beginPath();ctx.ellipse(-r*.05,0,r*.78,r*.32,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.fillStyle=`rgba(${rgb},.18)`;ctx.beginPath();ctx.ellipse(-r*.05,-r*.03,r*.58,r*.20,0,0,Math.PI*2);ctx.fill();
  ctx.fillStyle='rgba(2,6,12,.98)';ctx.beginPath();ctx.moveTo(r*.42,-r*.18);ctx.quadraticCurveTo(r*.98,0,r*.42,r*.18);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(255,225,210,${.30+.22*p})`;ctx.lineWidth=Math.max(.7,r*.03);for(let i=0;i<4;i++){const y=(-.11+i*.073)*r;ctx.beginPath();ctx.moveTo(r*.48,y);ctx.lineTo(r*.74,y);ctx.stroke();}
  eye(ctx,r*.56,0,r*.075,color,p);ctx.restore();
}

function drawSerpent(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),phase=t*5.5;glow(ctx,r*1.5,color,.12);ctx.save();
  ctx.lineCap='round';ctx.strokeStyle=`rgba(${rgb},.84)`;ctx.lineWidth=Math.max(1.5,r*.18);ctx.beginPath();
  for(let i=0;i<8;i++){const p=i/7,x=-r*.82+p*r*1.32,y=Math.sin(phase+p*5.6)*r*(.10+.05*Math.sin(p*Math.PI));if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.stroke();
  ctx.strokeStyle='rgba(3,8,15,.98)';ctx.lineWidth=Math.max(1.1,r*.12);ctx.beginPath();
  for(let i=0;i<8;i++){const p=i/7,x=-r*.82+p*r*1.32,y=Math.sin(phase+p*5.6)*r*(.10+.05*Math.sin(p*Math.PI));if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.stroke();
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.94)`;ctx.lineWidth=Math.max(1,r*.055);
  ctx.beginPath();ctx.moveTo(r*.40,-r*.20);ctx.quadraticCurveTo(r*.82,-r*.22,r*1.02,0);ctx.quadraticCurveTo(r*.82,r*.22,r*.40,r*.20);ctx.closePath();ctx.fill();ctx.stroke();
  eye(ctx,r*.66,-r*.04,r*.07,color,.7);ctx.restore();
}

function drawSkitter(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color);glow(ctx,r*1.4,color,.10);ctx.save();ctx.scale(1+Math.sin(t*10)*.02,1-Math.sin(t*10)*.015);
  ctx.strokeStyle=`rgba(${rgb},.80)`;ctx.lineWidth=Math.max(1.1,r*.075);ctx.lineCap='round';
  for(let i=0;i<3;i++){const x=-r*.36+i*r*.32;leg(ctx,r,x,-1,t*12+i*1.8,.88);leg(ctx,r,x,1,t*12+i*1.8+Math.PI,.88);}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.92)`;ctx.lineWidth=Math.max(1,r*.06);ctx.beginPath();ctx.ellipse(-r*.05,0,r*.62,r*.38,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(12,27,43,.98)';ctx.beginPath();ctx.moveTo(r*.20,-r*.30);ctx.lineTo(r*.78,0);ctx.lineTo(r*.20,r*.30);ctx.closePath();ctx.fill();ctx.stroke();
  eye(ctx,r*.47,-r*.07,r*.07,color,.6);eye(ctx,r*.47,r*.07,r*.07,color,.6);ctx.restore();
}

function drawMoth(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),flap=Math.sin(t*8)*r*.16;glow(ctx,r*1.6,color,.11);ctx.save();ctx.translate(0,Math.sin(t*5)*r*.06);
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.88)`;ctx.lineWidth=Math.max(1,r*.055);
  for(const side of [-1,1]){ctx.beginPath();ctx.moveTo(side*r*.08,0);ctx.quadraticCurveTo(side*r*.52,-r*.56-flap,side*r*1.02,-r*.25);ctx.quadraticCurveTo(side*r*.72,r*.04,side*r*.10,r*.20);ctx.closePath();ctx.fill();ctx.stroke();}
  ctx.beginPath();ctx.ellipse(0,0,r*.18,r*.48,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(${rgb},.38)`;ctx.lineWidth=Math.max(.7,r*.03);ctx.beginPath();ctx.moveTo(-r*.10,-r*.32);ctx.lineTo(-r*.28,-r*.62);ctx.moveTo(r*.10,-r*.32);ctx.lineTo(r*.28,-r*.62);ctx.stroke();
  eye(ctx,0,-r*.10,r*.07,color,.6);ctx.restore();
}

function drawBeetle(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color);glow(ctx,r*1.45,color,.10);ctx.save();ctx.strokeStyle=`rgba(${rgb},.80)`;ctx.lineWidth=Math.max(1.2,r*.065);ctx.lineCap='round';
  for(let i=0;i<3;i++){const x=-r*.42+i*r*.40;leg(ctx,r,x,-1,t*9+i*1.6,.78);leg(ctx,r,x,1,t*9+i*1.6+Math.PI,.78);}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.94)`;ctx.lineWidth=Math.max(1.2,r*.065);ctx.beginPath();ctx.ellipse(0,0,r*.72,r*.56,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.fillStyle=`rgba(${rgb},.16)`;ctx.beginPath();ctx.ellipse(-r*.05,-r*.08,r*.54,r*.38,0,0,Math.PI*2);ctx.fill();
  ctx.strokeStyle=`rgba(${rgb},.44)`;ctx.lineWidth=Math.max(.8,r*.03);ctx.beginPath();ctx.moveTo(0,-r*.48);ctx.lineTo(0,r*.48);ctx.moveTo(-r*.48,0);ctx.lineTo(r*.48,0);ctx.stroke();
  eye(ctx,r*.60,-r*.09,r*.065,color,.65);eye(ctx,r*.60,r*.09,r*.065,color,.65);ctx.restore();
}

function drawBrute(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),gait=Math.sin(t*7);glow(ctx,r*1.55,color,.11);ctx.save();ctx.translate(Math.abs(gait)*r*.03,Math.abs(gait)*r*.02);
  ctx.strokeStyle=`rgba(${rgb},.76)`;ctx.lineWidth=Math.max(1.5,r*.085);ctx.lineCap='round';
  for(const x of [-r*.42,r*.34]){leg(ctx,r,x,-1,t*7+(x<0?0:Math.PI),.72);leg(ctx,r,x,1,t*7+(x<0?Math.PI:0),.72);}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.94)`;ctx.lineWidth=Math.max(1.3,r*.07);ctx.beginPath();ctx.moveTo(-r*.65,-r*.36);ctx.quadraticCurveTo(-r*.56,-r*.72,0,-r*.68);ctx.quadraticCurveTo(r*.56,-r*.72,r*.72,-r*.20);ctx.lineTo(r*.60,r*.42);ctx.quadraticCurveTo(0,r*.74,-r*.58,r*.38);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(13,24,39,.99)';ctx.beginPath();ctx.moveTo(r*.28,-r*.30);ctx.lineTo(r*.90,-r*.16);ctx.lineTo(r*.98,r*.10);ctx.lineTo(r*.72,r*.28);ctx.lineTo(r*.28,r*.22);ctx.closePath();ctx.fill();ctx.stroke();
  eye(ctx,r*.55,-r*.08,r*.07,color,.7);eye(ctx,r*.55,r*.08,r*.07,color,.7);ctx.restore();
}

function drawPrismHound(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),bob=Math.sin(t*10)*r*.06;glow(ctx,r*1.48,color,.11);ctx.save();ctx.translate(0,bob);
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.92)`;ctx.lineWidth=Math.max(1,r*.06);ctx.beginPath();ctx.moveTo(-r*.62,0);ctx.lineTo(-r*.18,-r*.40);ctx.lineTo(r*.56,-r*.24);ctx.lineTo(r*.80,0);ctx.lineTo(r*.56,r*.24);ctx.lineTo(-r*.18,r*.40);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle=`rgba(${rgb},.20)`;ctx.beginPath();ctx.moveTo(-r*.34,-r*.26);ctx.lineTo(r*.28,-r*.18);ctx.lineTo(r*.52,0);ctx.lineTo(r*.04,r*.02);ctx.closePath();ctx.fill();
  eye(ctx,r*.54,0,r*.07,color,.8);ctx.restore();
}

function drawStalker(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),p=Math.sin(t*8);glow(ctx,r*1.55,color,.10);ctx.save();
  ctx.strokeStyle=`rgba(${rgb},.82)`;ctx.lineWidth=Math.max(1.1,r*.06);ctx.lineCap='round';
  for(const side of [-1,1]){ctx.beginPath();ctx.moveTo(-r*.18,side*r*.16);ctx.lineTo(r*.08,side*(r*.64+p*r*.10));ctx.lineTo(r*.54,side*r*.78);ctx.stroke();ctx.beginPath();ctx.moveTo(r*.14,side*r*.04);ctx.lineTo(r*.42,side*(r*.42-p*r*.08));ctx.lineTo(r*.70,side*r*.36);ctx.stroke();}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.94)`;ctx.lineWidth=Math.max(1.2,r*.06);ctx.beginPath();ctx.moveTo(-r*.46,-r*.26);ctx.lineTo(r*.20,-r*.34);ctx.lineTo(r*.68,0);ctx.lineTo(r*.20,r*.34);ctx.lineTo(-r*.46,r*.26);ctx.closePath();ctx.fill();ctx.stroke();
  eye(ctx,r*.68,0,r*.065,color,.7);ctx.restore();
}

function drawLinkbreaker(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),p=.5+.5*Math.sin(t*8);glow(ctx,r*1.65,color,.15);ctx.save();
  ctx.strokeStyle=`rgba(${rgb},.86)`;ctx.lineWidth=Math.max(1.4,r*.075);ctx.lineCap='round';
  for(const side of [-1,1]){for(let i=0;i<3;i++){const x=-r*.36+i*r*.34;ctx.beginPath();ctx.moveTo(x,side*r*.10);ctx.lineTo(x-side*r*.18,side*(r*.46+Math.sin(t*10+i)*r*.08));ctx.lineTo(x+side*r*.12,side*r*.84);ctx.stroke();}}ctx.lineCap='butt';
  ctx.fillStyle='rgba(15,8,18,.98)';ctx.strokeStyle='#ff4d70';ctx.lineWidth=Math.max(1.4,r*.065);ctx.beginPath();ctx.moveTo(-r*.62,-r*.34);ctx.lineTo(r*.18,-r*.50);ctx.lineTo(r*.76,0);ctx.lineTo(r*.18,r*.50);ctx.lineTo(-r*.62,r*.34);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(${rgb},${.35+.20*p})`;ctx.lineWidth=Math.max(1,r*.035);ctx.beginPath();ctx.moveTo(-r*.34,0);ctx.lineTo(r*.48,0);ctx.stroke();
  eye(ctx,r*.58,-r*.08,r*.075,'#ff4d70',p);eye(ctx,r*.58,r*.08,r*.075,'#ff4d70',p);ctx.restore();
}

function creature(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void {
  const v=e.visualVariant||'wisp',r=e.radius;
  if(v==='moth')drawMoth(ctx,r,e.color,t);
  else if(v==='skitter')drawSkitter(ctx,r,e.color,t);
  else if(v==='beetle')drawBeetle(ctx,r,e.color,t);
  else if(v==='brute')drawBrute(ctx,r,e.color,t);
  else if(v==='prism')drawPrismHound(ctx,r,e.color,t);
  else if(v==='serpent')drawSerpent(ctx,r,e.color,t);
  else if(v==='leech')drawLeech(ctx,r,e.color,t);
  else if(v==='stalker')drawStalker(ctx,r,e.color,t);
  else if(v==='linkbreaker')drawLinkbreaker(ctx,r,e.color,t);
  else drawVeilJelly(ctx,r,e.color,t);
}

function drawVoidLancer(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),flap=Math.sin(t*4.2);glow(ctx,r*2,color,.18);ctx.save();
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.92)`;ctx.lineWidth=Math.max(2,r*.045);ctx.lineJoin='round';
  ctx.beginPath();ctx.moveTo(-r*.96,0);ctx.quadraticCurveTo(-r*.56,-r*(.80+flap*.08),r*.08,-r*.36);ctx.lineTo(r*.96,-r*.14);ctx.lineTo(r*.66,r*.16);ctx.quadraticCurveTo(r*.30,r*.60,-r*.38,r*.46);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.beginPath();ctx.moveTo(-r*.28,-r*.22);ctx.lineTo(-r*1.12,-r*.62);ctx.lineTo(-r*.72,-r*.10);ctx.moveTo(-r*.28,r*.22);ctx.lineTo(-r*1.12,r*.62);ctx.lineTo(-r*.72,r*.10);ctx.stroke();
  ctx.fillStyle='rgba(8,18,30,.99)';ctx.beginPath();ctx.moveTo(r*.28,-r*.22);ctx.lineTo(r*1.12,0);ctx.lineTo(r*.28,r*.22);ctx.closePath();ctx.fill();ctx.stroke();
  eye(ctx,r*.56,-r*.10,r*.09,color,.8);eye(ctx,r*.56,r*.10,r*.09,color,.8);
  ctx.restore();
}

function drawDreadCharger(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const rgb=rgbOf(color),gait=Math.sin(t*6.5);glow(ctx,r*2,color,.16);ctx.save();ctx.translate(Math.abs(gait)*r*.03,Math.abs(gait)*r*.025);
  ctx.strokeStyle=`rgba(${rgb},.84)`;ctx.lineWidth=Math.max(2,r*.05);ctx.lineCap='round';
  for(const x of [-r*.42,r*.40]){leg(ctx,r,x,-1,t*7+(x<0?0:Math.PI),.88);leg(ctx,r,x,1,t*7+(x<0?Math.PI:0),.88);}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.96)`;ctx.lineWidth=Math.max(2,r*.055);ctx.beginPath();ctx.moveTo(-r*.78,-r*.38);ctx.quadraticCurveTo(-r*.60,-r*.88,0,-r*.70);ctx.quadraticCurveTo(r*.65,-r*.82,r*.78,-r*.30);ctx.lineTo(r*.60,r*.48);ctx.quadraticCurveTo(0,r*.84,-r*.64,r*.42);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(8,15,25,.99)';ctx.beginPath();ctx.moveTo(r*.22,-r*.30);ctx.lineTo(r*1.02,-r*.18);ctx.lineTo(r*1.18,0);ctx.lineTo(r*.98,r*.18);ctx.lineTo(r*.22,r*.28);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(255,186,82,.72)`;ctx.lineWidth=Math.max(2,r*.035);ctx.beginPath();ctx.moveTo(r*.08,-r*.30);ctx.lineTo(r*.46,-r*.76);ctx.lineTo(r*.62,-r*.30);ctx.moveTo(r*.08,r*.30);ctx.lineTo(r*.46,r*.76);ctx.lineTo(r*.62,r*.30);ctx.stroke();
  eye(ctx,r*.62,-r*.10,r*.085,color,.8);eye(ctx,r*.62,r*.10,r*.085,color,.8);ctx.restore();
}

function drawBroodMatriarch(ctx: CanvasRenderingContext2D, r: number, color: string, t: number, damagePhase: number): void {
  const rgb=rgbOf(color);glow(ctx,r*2.05,color,.18);ctx.save();ctx.translate(0,Math.sin(t*2.5)*r*.035);
  ctx.strokeStyle=`rgba(${rgb},.84)`;ctx.lineWidth=Math.max(1.8,r*.045);ctx.lineCap='round';for(let i=0;i<6;i++){const a=-1.25+i*.50;tentacle(ctx,r,a,r*(1+.12*Math.sin(t*3+i)),t*3+i);}ctx.lineCap='butt';
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.96)`;ctx.lineWidth=Math.max(2,r*.05);ctx.beginPath();ctx.ellipse(-r*.05,r*.08,r*.78,r*.82,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(17,22,39,.99)';ctx.beginPath();ctx.ellipse(r*.28,r*.10,r*.50,r*.64,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  for(let i=0;i<5;i++){const a=t*.85+i*Math.PI*2/5,x=Math.cos(a)*r*1.14,y=Math.sin(a)*r*.86;ctx.save();ctx.translate(x,y);ctx.fillStyle='rgba(6,12,20,.99)';ctx.strokeStyle=`rgba(${rgb},.70)`;ctx.beginPath();ctx.ellipse(0,0,r*.15,r*.21,0,0,Math.PI*2);ctx.fill();ctx.stroke();eye(ctx,0,-r*.02,r*.045,color,.65);ctx.restore();}
  eye(ctx,r*.22,-r*.10,r*.10,color,.85);
  if(damagePhase>.55){ctx.strokeStyle='rgba(255,94,130,.75)';ctx.lineWidth=Math.max(1.5,r*.032);ctx.beginPath();ctx.moveTo(-r*.48,-r*.34);ctx.lineTo(-r*.12,-r*.10);ctx.lineTo(-r*.32,r*.28);ctx.moveTo(r*.32,-r*.20);ctx.lineTo(r*.50,r*.26);ctx.stroke();}
  ctx.restore();
}

function drawAbyssalLeviathan(ctx: CanvasRenderingContext2D, r: number, color: string, t: number, damagePhase: number): void {
  const rgb=rgbOf(color),p=.5+.5*Math.sin(t*3.2);glow(ctx,r*2.2,color,.19);ctx.save();ctx.translate(0,Math.sin(t*2.3)*r*.05);
  ctx.strokeStyle=`rgba(${rgb},.82)`;ctx.lineWidth=Math.max(2,r*.042);for(let i=0;i<8;i++){const a=-1.35+i*.385;tentacle(ctx,r,a,r*(1+.18*Math.sin(t*2+i)),t*2.3+i*.7);}
  ctx.fillStyle=darkFill(color);ctx.strokeStyle=`rgba(${rgb},.96)`;ctx.lineWidth=Math.max(2,r*.05);ctx.beginPath();ctx.moveTo(-r*.90,0);ctx.quadraticCurveTo(-r*.62,-r*.78,0,-r*.80);ctx.quadraticCurveTo(r*.72,-r*.76,r*.92,0);ctx.quadraticCurveTo(r*.68,r*.74,0,r*.80);ctx.quadraticCurveTo(-r*.64,r*.76,-r*.90,0);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(11,21,36,.99)';ctx.beginPath();ctx.ellipse(0,r*.08,r*.44,r*.50,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  for(let i=0;i<6;i++){const a=t*.38+i*Math.PI/3;ctx.strokeStyle=`rgba(${rgb},${.34+.18*p})`;ctx.lineWidth=Math.max(1,r*.025);ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.54,Math.sin(a)*r*.40);ctx.lineTo(Math.cos(a)*r*1.30,Math.sin(a)*r*.72);ctx.stroke();}
  eye(ctx,-r*.27,-r*.14,r*.09,color,p);eye(ctx,r*.27,-r*.14,r*.09,color,p);eye(ctx,0,r*.16,r*.08,color,p);
  if(damagePhase>.65){ctx.strokeStyle='rgba(255,82,118,.72)';ctx.lineWidth=Math.max(1.5,r*.03);for(let i=0;i<4;i++){const a=i*Math.PI/2+t*.2;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.34,Math.sin(a)*r*.30);ctx.lineTo(Math.cos(a+.35)*r*.66,Math.sin(a+.35)*r*.56);ctx.stroke();}}
  ctx.restore();
}

export function drawEnemyCreature(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void { creature(ctx,e,t); }

export function drawBossCreature(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void {
  const phase=1-Math.max(0,Math.min(1,e.hp/Math.max(1,e.maxHp)));
  if(e.bossType==='charger')drawDreadCharger(ctx,e.radius,e.color,t);
  else if(e.bossType==='shooter')drawVoidLancer(ctx,e.radius,e.color,t);
  else if(e.bossType==='summoner')drawBroodMatriarch(ctx,e.radius,e.color,t,phase);
  else drawAbyssalLeviathan(ctx,e.radius,e.color,t,phase);
}