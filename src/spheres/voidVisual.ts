import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderVoidSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#c28cff', time);
  const r = 24 * scale, rot = st.animationTime * -.22;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.30, st.color, st.disabled ? .04 : .11);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.rotate(rot);
  ctx.globalAlpha = st.disabled ? .18 : .88;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.moveTo(0, -r * .98);
  ctx.quadraticCurveTo(r * .22, -r * .60, r * .72, -r * .34);
  ctx.quadraticCurveTo(r * .98, 0, r * .56, r * .34);
  ctx.quadraticCurveTo(r * .14, r * .76, -r * .30, r * .60);
  ctx.quadraticCurveTo(-r * .74, r * .44, -r * .68, 0);
  ctx.quadraticCurveTo(-r * .62, -r * .48, 0, -r * .98);
  ctx.closePath();
  ctx.stroke();
  ctx.globalAlpha = st.disabled ? .12 : .58;
  for (let i = 0; i < 3; i++) {
    const a = rot * .8 + i * Math.PI * 2 / 3;
    const rr = r * (1.06 + i * .18);
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * rr, Math.sin(a) * rr * .58);
    ctx.quadraticCurveTo(0, 0, Math.cos(a + .72) * rr * .54, Math.sin(a + .72) * rr * .34);
    ctx.stroke();
  }
  ctx.globalAlpha = st.disabled ? .14 : .50;
  ctx.beginPath();
  ctx.arc(0, 0, r * .70, -.55, 2.45);
  ctx.stroke();
  core(ctx, r * .48, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderVoidSphereRuntimeVfx;
