import type { PlayerState, SphereEntity } from '../engine';
import { core, drawCrescent, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderAuraSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#57e6b4', time);
  const r = 24 * scale, pulse = .96 + st.pulse * .08;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.5, st.color, st.disabled ? .05 : .14);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalAlpha = st.disabled ? .16 : .72;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  ctx.scale(pulse, pulse);
  for (let i = 0; i < 4; i++) {
    const a = st.animationTime * .18 + i * Math.PI / 2;
    ctx.save();
    ctx.rotate(a);
    ctx.beginPath();
    ctx.moveTo(r * .34, 0);
    ctx.quadraticCurveTo(r * .86, -r * .78, r * 1.26, 0);
    ctx.quadraticCurveTo(r * .86, r * .78, r * .34, 0);
    ctx.stroke();
    ctx.restore();
  }
  drawCrescent(ctx, r * 1.04, st.color, st.animationTime * .10, st.disabled ? .16 : .46, .34);
  core(ctx, r * .52, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderAuraSphereRuntimeVfx;
