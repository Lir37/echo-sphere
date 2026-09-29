import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

const TAU=Math.PI*2;
function blade(ctx:CanvasRenderingContext2D,r:number,angle:number,color:string,alpha:number):void{
  ctx.save();ctx.rotate(angle);ctx.globalAlpha=alpha;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.2;
  ctx.beginPath();ctx.moveTo(r*.38,0);ctx.lineTo(r*1.10,-r*.18);ctx.lineTo(r*1.28,0);ctx.lineTo(r*1.10,r*.18);ctx.closePath();ctx.fill();ctx.stroke();ctx.restore();
}
export function renderAuraSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#57e6b4',time),r=24*scale,pulse=.97+st.pulse*.07;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.45,st.color,st.disabled?.05:.13);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.scale(pulse,pulse);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.16:.90;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.55;
  ctx.beginPath();for(let i=0;i<12;i++){const a=-Math.PI/2+i*TAU/12,rr=i%2===0?r*.88:r*.69,x=Math.cos(a)*rr,y=Math.sin(a)*rr;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.closePath();ctx.fill();ctx.stroke();
  for(let i=0;i<6;i++)blade(ctx,r,i*TAU/6+st.animationTime*.06,st.color,st.disabled?.08:.54);
  ctx.globalAlpha=st.disabled?.08:.34;ctx.strokeStyle='#dfffee';ctx.lineWidth=.9;ctx.beginPath();ctx.arc(0,0,r*1.22,0,TAU);ctx.stroke();
  core(ctx,r*.49,st.color,st.pulse);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderAuraSphereRuntimeVfx;
