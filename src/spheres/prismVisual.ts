import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderPrismSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#ff8de1', time);
  const r = 24 * scale, rot = st.animationTime * .16;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.25, st.color, st.disabled ? .05 : .14);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.rotate(rot);
  ctx.globalAlpha = st.disabled ? .18 : .90;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.moveTo(0, -r);
  ctx.quadraticCurveTo(r * .82, -r * .52, r * .72, r * .30);
  ctx.quadraticCurveTo(r * .48, r * .92, 0, r * .72);
  ctx.quadraticCurveTo(-r * .48, r * .92, -r * .72, r * .30);
  ctx.quadraticCurveTo(-r * .82, -r * .52, 0, -r);
  ctx.closePath();
  ctx.stroke();
  ctx.globalAlpha = st.disabled ? .12 : .58;
  ctx.beginPath();
  ctx.moveTo(-r * .52, 0);
  ctx.quadraticCurveTo(-r * .14, -r * .22, 0, -r * .58);
  ctx.quadraticCurveTo(r * .14, -r * .22, r * .52, 0);
  ctx.quadraticCurveTo(r * .14, r * .22, 0, r * .58);
  ctx.quadraticCurveTo(-r * .14, r * .22, -r * .52, 0);
  ctx.stroke();
  for (let i = 0; i < 3; i++) {
    const a = st.animationTime * .34 + i * Math.PI * 2 / 3;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * r * .44, Math.sin(a) * r * .44);
    ctx.quadraticCurveTo(Math.cos(a + .45) * r * .96, Math.sin(a + .45) * r * .72, Math.cos(a) * r * 1.18, Math.sin(a) * r * .50);
    ctx.stroke();
  }
  core(ctx, r * .50, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderPrismSphereRuntimeVfx;
