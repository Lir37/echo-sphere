import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

function prismWeapon(ctx:CanvasRenderingContext2D,r:number,color:string,disabled:boolean):void{
  ctx.globalAlpha=disabled?.13:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.25;
  ctx.beginPath();ctx.moveTo(r*.38,-r*.21);ctx.lineTo(r*1.02,-r*.17);ctx.lineTo(r*1.32,0);ctx.lineTo(r*1.02,r*.17);ctx.lineTo(r*.38,r*.21);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=disabled?.08:.76;ctx.strokeStyle='#ffffff';ctx.beginPath();ctx.moveTo(r*1.13,-r*.10);ctx.lineTo(r*1.13,r*.10);ctx.arc(r*1.32,0,r*.08,-Math.PI*.7,Math.PI*.7);ctx.stroke();
}
export function renderPrismSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#ff8de1',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.30,st.color,st.disabled?.05:.13);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(sphere.rotation||0);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.16:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.65;
  ctx.beginPath();ctx.moveTo(0,-r*.96);ctx.lineTo(r*.70,-r*.36);ctx.lineTo(r*.80,r*.22);ctx.lineTo(r*.36,r*.76);ctx.lineTo(0,r*.90);ctx.lineTo(-r*.42,r*.67);ctx.lineTo(-r*.76,r*.22);ctx.lineTo(-r*.68,-r*.42);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=st.disabled?.08:.50;ctx.strokeStyle='#ffffff';ctx.lineWidth=.85;ctx.beginPath();ctx.moveTo(-r*.46,0);ctx.lineTo(0,-r*.52);ctx.lineTo(r*.50,0);ctx.lineTo(0,r*.52);ctx.closePath();ctx.stroke();
  prismWeapon(ctx,r,st.color,st.disabled);
  for(let i=0;i<2;i++){ctx.globalAlpha=st.disabled?.07:.30;ctx.strokeStyle=st.color;ctx.lineWidth=.8;ctx.beginPath();ctx.moveTo(r*.58,-r*(.42+i*.10));ctx.lineTo(r*(1.06+i*.08),-r*(.16-i*.04));ctx.stroke();}
  core(ctx,r*.44,st.color,st.pulse);finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderPrismSphereRuntimeVfx;
