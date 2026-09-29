import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

function barrel(ctx:CanvasRenderingContext2D,r:number,y:number,color:string,disabled:boolean):void{
  ctx.globalAlpha=disabled?.13:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.25;
  ctx.beginPath();ctx.moveTo(r*.32,y-r*.10);ctx.lineTo(r*1.18,y-r*.09);ctx.lineTo(r*1.40,y);ctx.lineTo(r*1.18,y+r*.09);ctx.lineTo(r*.32,y+r*.10);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=disabled?.08:.70;ctx.strokeStyle='#ffffff';ctx.beginPath();ctx.arc(r*1.39,y,r*.075,0,Math.PI*2);ctx.stroke();
}
export function renderShotgunSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#ff8f3d',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.30,st.color,st.disabled?.05:.14);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(sphere.rotation||0);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.17:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.65;
  ctx.beginPath();for(let i=0;i<10;i++){const a=-Math.PI/2+i*Math.PI/5,rr=i%2===0?r*.86:r*.68,x=Math.cos(a)*rr,y=Math.sin(a)*rr;if(i===0)ctx.moveTo(x,y);else ctx.lineTo(x,y);}ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=st.disabled?.08:.38;ctx.strokeStyle='#ffffff';ctx.lineWidth=.8;ctx.beginPath();ctx.moveTo(-r*.38,-r*.32);ctx.lineTo(0,-r*.10);ctx.lineTo(r*.34,-r*.30);ctx.moveTo(-r*.38,r*.32);ctx.lineTo(0,r*.10);ctx.lineTo(r*.34,r*.30);ctx.stroke();
  barrel(ctx,r,-r*.25,st.color,st.disabled);barrel(ctx,r,0,st.color,st.disabled);barrel(ctx,r,r*.25,st.color,st.disabled);
  if(Math.sin(st.animationTime*6)>0){ctx.globalAlpha=.22;ctx.strokeStyle=st.color;ctx.lineWidth=.9;ctx.beginPath();ctx.arc(r*1.40,0,r*.18,0,Math.PI*2);ctx.stroke();}
  core(ctx,r*.45,st.color,st.pulse);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderShotgunSphereRuntimeVfx;
