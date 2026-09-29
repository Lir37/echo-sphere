import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

const TAU=Math.PI*2;
function capacitor(ctx:CanvasRenderingContext2D,r:number,angle:number,color:string,alpha:number):void{
  ctx.save();ctx.rotate(angle);ctx.globalAlpha=alpha;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.15;
  ctx.beginPath();ctx.moveTo(r*.48,-r*.14);ctx.lineTo(r*.90,-r*.22);ctx.lineTo(r*1.13,0);ctx.lineTo(r*.90,r*.22);ctx.lineTo(r*.48,r*.14);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=alpha*.68;ctx.beginPath();ctx.moveTo(r*.68,-r*.10);ctx.lineTo(r*.68,r*.10);ctx.moveTo(r*.84,-r*.13);ctx.lineTo(r*.84,r*.13);ctx.stroke();ctx.restore();
}
export function renderPulseSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#ffd35a',time),r=24*scale,phase=st.animationTime*1.10;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.45,st.color,st.disabled?.05:.13);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.16:.88;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.5;
  ctx.beginPath();for(let i=0;i<12;i++){const a=-Math.PI/2+i*TAU/12,rr=i%2===0?r*.86:r*.69,x=Math.cos(a)*rr,y=Math.sin(a)*rr;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.closePath();ctx.fill();ctx.stroke();
  for(let i=0;i<4;i++)capacitor(ctx,r,i*Math.PI/2+phase*.04,st.color,st.disabled?.08:.64);
  const q=((phase*.42)%1+1)%1;ctx.globalAlpha=st.disabled?.08:(1-q)*.50;ctx.strokeStyle=st.color;ctx.lineWidth=1.05;ctx.beginPath();ctx.arc(0,0,r*(.96+q*1.05),0,TAU);ctx.stroke();
  ctx.globalAlpha=st.disabled?.06:.28;ctx.beginPath();ctx.arc(0,0,r*(.62+q*.52),0,TAU);ctx.stroke();
  core(ctx,r*.48,st.color,st.pulse);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderPulseSphereRuntimeVfx;
