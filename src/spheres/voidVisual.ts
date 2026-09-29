import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

const TAU=Math.PI*2;
function voidWeapon(ctx:CanvasRenderingContext2D,r:number,color:string,disabled:boolean):void{
  ctx.globalAlpha=disabled?.13:.92;ctx.fillStyle='#040710';ctx.strokeStyle=color;ctx.lineWidth=1.3;
  ctx.beginPath();ctx.moveTo(r*.30,-r*.20);ctx.lineTo(r*1.00,-r*.16);ctx.lineTo(r*1.34,-r*.06);ctx.lineTo(r*1.34,r*.06);ctx.lineTo(r*1.00,r*.16);ctx.lineTo(r*.30,r*.20);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=disabled?.08:.72;ctx.strokeStyle='#e8d8ff';ctx.beginPath();ctx.arc(r*1.32,0,r*.08,0,TAU);ctx.stroke();
}
export function renderVoidSphereRuntimeVfx(ctx:CanvasRenderingContext2D,sphere:SphereEntity,player:PlayerState,time:number,scale=1):void{
  const st=stateColor(sphere,player,'#c28cff',time),r=24*scale;
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.globalCompositeOperation='lighter';glow(ctx,r*2.30,st.color,st.disabled?.04:.10);ctx.restore();
  ctx.save();ctx.translate(sphere.pos.x,sphere.pos.y);ctx.rotate(sphere.rotation||0);ctx.globalCompositeOperation='source-over';
  ctx.globalAlpha=st.disabled?.16:.92;ctx.fillStyle='#040710';ctx.strokeStyle=st.color;ctx.lineWidth=1.65;
  ctx.beginPath();ctx.moveTo(0,-r*.98);ctx.lineTo(r*.54,-r*.60);ctx.lineTo(r*.86,-r*.12);ctx.lineTo(r*.66,r*.54);ctx.lineTo(0,r*.82);ctx.lineTo(-r*.66,r*.52);ctx.lineTo(-r*.88,-r*.08);ctx.lineTo(-r*.48,-r*.62);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.globalAlpha=st.disabled?.08:.38;ctx.strokeStyle='#ffffff';ctx.lineWidth=.85;ctx.beginPath();ctx.moveTo(-r*.43,-r*.08);ctx.lineTo(0,-r*.46);ctx.lineTo(r*.48,-r*.08);ctx.lineTo(0,r*.44);ctx.closePath();ctx.stroke();
  voidWeapon(ctx,r,st.color,st.disabled);
  core(ctx,r*.45,st.color,st.pulse*.92);
  if((sphere.visualTier||0)>=4){ctx.globalAlpha=st.disabled?.06:.30;ctx.strokeStyle=st.color;ctx.lineWidth=.9;ctx.beginPath();ctx.arc(0,0,r*1.14,-.62,.62);ctx.stroke();}
  finishDisabled(ctx,r,st.color,st.disabled);ctx.restore();
}
export default renderVoidSphereRuntimeVfx;
