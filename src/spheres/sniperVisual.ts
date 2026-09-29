import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

function drawWeapon(ctx:CanvasRenderingContext2D,r:number,color:string,disabled:boolean,time:number):void{
  ctx.globalAlpha=disabled?.13:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=color;ctx.lineWidth=1.25;
  ctx.beginPath();ctx.moveTo(r*.28,-r*.16);ctx.lineTo(r*1.12,-r*.16);ctx.lineTo(r*1.46,-r*.08);ctx.lineTo(r*1.58,0);ctx.lineTo(r*1.46,r*.08);ctx.lineTo(r*1.12,r*.16);ctx.lineTo(r*.28,r*.16);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=disabled?.08:.74;ctx.beginPath();ctx.moveTo(r*.38,-r*.30);ctx.lineTo(r*1.28,-r*.30);ctx.moveTo(r*.38,r*.30);ctx.lineTo(r*1.28,r*.30);ctx.stroke();
  ctx.globalAlpha=disabled?.08:.82;ctx.strokeStyle='#ffffff';ctx.beginPath();ctx.arc(r*1.50,0,r*(.065+.008*Math.sin(time*6)),0,Math.PI*2);ctx.stroke();
}
export function renderSniperSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#e86cff',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.25,st.color,st.disabled?.05:.13);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(sphere.rotation||0);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.18:.92;ctx.fillStyle='#07111d';ctx.strokeStyle=st.color;ctx.lineWidth=1.7;
  ctx.beginPath();ctx.moveTo(-r*.90,-r*.34);ctx.lineTo(-r*.60,-r*.72);ctx.lineTo(0,-r*.84);ctx.lineTo(r*.64,-r*.72);ctx.lineTo(r*.94,-r*.30);ctx.lineTo(r*1.02,0);ctx.lineTo(r*.94,r*.30);ctx.lineTo(r*.64,r*.72);ctx.lineTo(0,r*.84);ctx.lineTo(-r*.60,r*.72);ctx.lineTo(-r*.90,r*.34);ctx.lineTo(-r*1.02,0);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=st.disabled?.08:.42;ctx.strokeStyle='#ffffff';ctx.lineWidth=.85;ctx.beginPath();ctx.moveTo(-r*.56,0);ctx.lineTo(0,-r*.34);ctx.lineTo(r*.55,0);ctx.lineTo(0,r*.34);ctx.closePath();ctx.stroke();
  drawWeapon(ctx,r,st.color,st.disabled,time);
  core(ctx,r*.46,st.color,st.pulse);
  if((sphere.visualTier||0)>=4){ctx.globalAlpha=st.disabled?.08:.32;ctx.strokeStyle=st.color;ctx.lineWidth=.85;ctx.beginPath();ctx.arc(0,0,r*1.14,time*.25,time*.25+Math.PI*.9);ctx.stroke();}
  finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderSniperSphereRuntimeVfx;
