import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderPulseSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#ffd35a', time);
  const r = 24 * scale, phase = st.animationTime * 1.25;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.55, st.color, st.disabled ? .05 : .16);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalAlpha = st.disabled ? .18 : .84;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  for (let i = 0; i < 3; i++) {
    const q = (phase * .32 + i / 3) % 1;
    const rr = r * (1.02 + q * .88);
    ctx.beginPath();
    ctx.ellipse(0, 0, rr * 1.20, rr * .42, phase * .08, 0, Math.PI * 2);
    ctx.stroke();
  }
  ctx.globalAlpha = st.disabled ? .12 : .54;
  ctx.beginPath();
  ctx.ellipse(0, 0, r * 1.02, r * .58, -.14, Math.PI * .14, Math.PI * 1.86);
  ctx.stroke();
  ctx.beginPath();
  ctx.ellipse(0, 0, r * .72, r * .38, .14, -Math.PI * .86, Math.PI * .12);
  ctx.stroke();
  core(ctx, r * .50, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderPulseSphereRuntimeVfx;
