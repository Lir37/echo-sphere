import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

function conductor(ctx:CanvasRenderingContext2D,r:number,angle:number,color:string,alpha:number):void{
  ctx.save();ctx.rotate(angle);ctx.globalAlpha=alpha;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.25;
  ctx.beginPath();ctx.moveTo(r*.35,-r*.10);ctx.lineTo(r*.86,-r*.18);ctx.lineTo(r*1.12,0);ctx.lineTo(r*.86,r*.18);ctx.lineTo(r*.35,r*.10);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=alpha*.72;ctx.beginPath();ctx.arc(r*1.10,0,r*.09,0,Math.PI*2);ctx.stroke();ctx.restore();
}
export function renderChainSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#ffe25b',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.15,st.color,st.disabled?.05:.12);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(st.animationTime*.12);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.17:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.65;
  ctx.beginPath();ctx.moveTo(0,-r*.82);ctx.lineTo(r*.62,-r*.50);ctx.lineTo(r*.78,0);ctx.lineTo(r*.52,r*.58);ctx.lineTo(0,r*.82);ctx.lineTo(-r*.52,r*.58);ctx.lineTo(-r*.78,0);ctx.lineTo(-r*.62,-r*.50);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=st.disabled?.08:.42;ctx.strokeStyle='#ffffff';ctx.lineWidth=.8;ctx.beginPath();ctx.moveTo(-r*.52,-r*.24);ctx.lineTo(0,-r*.52);ctx.lineTo(r*.52,-r*.24);ctx.moveTo(-r*.52,r*.24);ctx.lineTo(0,r*.52);ctx.lineTo(r*.52,r*.24);ctx.stroke();
  for(let i=0;i<4;i++)conductor(ctx,r,i*Math.PI/2,st.color,st.disabled?.10:.70);
  ctx.globalAlpha=st.disabled?.08:.34;ctx.strokeStyle=st.color;ctx.lineWidth=1;for(let i=0;i<4;i++){const a=i*Math.PI/2+time*.8;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.35,Math.sin(a)*r*.35);ctx.lineTo(Math.cos(a)*r*.70,Math.sin(a)*r*.70);ctx.stroke();}
  core(ctx,r*.47,st.color,st.pulse);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderChainSphereRuntimeVfx;
