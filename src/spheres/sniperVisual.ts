import type { PlayerState, SphereEntity } from '../engine';
import { core, drawCrescent, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderSniperSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#e86cff', time);
  const r = 24 * scale;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.2, st.color, st.disabled ? .05 : .13);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  const t = st.animationTime;
  const rot = t * .18 + sphere.pos.y * .001;
  ctx.rotate(rot);
  ctx.globalCompositeOperation = 'source-over';
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  ctx.globalAlpha = st.disabled ? .22 : .92;
  ctx.beginPath();
  ctx.moveTo(-r * .92, -r * .34);
  ctx.quadraticCurveTo(-r * .55, -r * .72, 0, -r * .82);
  ctx.quadraticCurveTo(r * .55, -r * .72, r * .92, -r * .34);
  ctx.quadraticCurveTo(r * 1.04, 0, r * .92, r * .34);
  ctx.quadraticCurveTo(r * .55, r * .72, 0, r * .82);
  ctx.quadraticCurveTo(-r * .55, r * .72, -r * .92, r * .34);
  ctx.quadraticCurveTo(-r * 1.04, 0, -r * .92, -r * .34);
  ctx.closePath();
  ctx.stroke();
  drawCrescent(ctx, r * .86, st.color, 0, st.disabled ? .18 : .64, .24);
  drawCrescent(ctx, r * .86, st.color, Math.PI, st.disabled ? .18 : .64, .24);
  ctx.strokeStyle = st.color;
  ctx.globalAlpha = st.disabled ? .16 : .70;
  ctx.beginPath();
  ctx.moveTo(-r * .48, 0);
  ctx.quadraticCurveTo(0, -r * .18, r * .48, 0);
  ctx.quadraticCurveTo(0, r * .18, -r * .48, 0);
  ctx.stroke();
  core(ctx, r * .52, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderSniperSphereRuntimeVfx;
