import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderShotgunSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#ff8f3d', time);
  const r = 24 * scale, spin = st.animationTime * .22;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.15, st.color, st.disabled ? .05 : .15);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.rotate(spin);
  ctx.globalAlpha = st.disabled ? .22 : .92;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  for (let i = 0; i < 3; i++) {
    const a = i * Math.PI * 2 / 3;
    const x = Math.cos(a) * r * .62, y = Math.sin(a) * r * .62;
    ctx.beginPath();
    ctx.moveTo(x * .42, y * .42);
    ctx.quadraticCurveTo(x * .86, y * .18, x * 1.08, y);
    ctx.quadraticCurveTo(x * .86, y * .42, x * .42, y * .55);
    ctx.closePath();
    ctx.stroke();
  }
  ctx.beginPath();
  ctx.arc(0, 0, r * .88, 0, Math.PI * 2);
  ctx.stroke();
  ctx.globalAlpha = st.disabled ? .14 : .56;
  ctx.beginPath();
  ctx.arc(0, 0, r * .64, -.85, .85);
  ctx.stroke();
  core(ctx, r * .50, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderShotgunSphereRuntimeVfx;
