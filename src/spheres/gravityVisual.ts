import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

const TAU=Math.PI*2;
function anchor(ctx:CanvasRenderingContext2D,r:number,angle:number,color:string,alpha:number):void{
  ctx.save();ctx.rotate(angle);ctx.globalAlpha=alpha;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.15;
  ctx.beginPath();ctx.moveTo(r*.38,0);ctx.lineTo(r*.88,-r*.17);ctx.lineTo(r*1.15,0);ctx.lineTo(r*.88,r*.17);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.beginPath();ctx.arc(r*1.14,0,r*.08,0,TAU);ctx.stroke();ctx.restore();
}
export function renderGravitySphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#a58cff',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.30,st.color,st.disabled?.05:.13);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(st.animationTime*-.08);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.15:.88;ctx.strokeStyle=st.color;ctx.fillStyle='#07111d';ctx.lineWidth=1.45;
  for(let i=0;i<3;i++){ctx.beginPath();ctx.arc(0,0,r*(.94+i*.25),Math.PI*(.18+i*.18),Math.PI*(1.55+i*.18));ctx.stroke();}
  for(let i=0;i<6;i++)anchor(ctx,r,i*TAU/6,st.color,st.disabled?.08:.54);
  ctx.globalAlpha=st.disabled?.07:.30;ctx.strokeStyle='#ffffff';ctx.lineWidth=.8;ctx.beginPath();ctx.moveTo(-r*.28,0);ctx.lineTo(0,-r*.28);ctx.lineTo(r*.28,0);ctx.lineTo(0,r*.28);ctx.closePath();ctx.stroke();
  core(ctx,r*.43,st.color,st.pulse*.75);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderGravitySphereRuntimeVfx;
