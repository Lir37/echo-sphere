import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderChainSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#ffe25b', time);
  const r = 24 * scale, spin = st.animationTime * .38;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.2, st.color, st.disabled ? .05 : .14);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.rotate(spin);
  ctx.globalAlpha = st.disabled ? .20 : .86;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.moveTo(0, -r * .94);
  for (let i = 1; i <= 8; i++) {
    const a = -Math.PI / 2 + i * Math.PI / 4;
    const rr = i % 2 ? r * .74 : r * .92;
    const x = Math.cos(a) * rr, y = Math.sin(a) * rr;
    if (i === 1) ctx.quadraticCurveTo(x * .74, y * .74, x, y);
    else ctx.lineTo(x, y);
  }
  ctx.closePath();
  ctx.stroke();

  ctx.globalAlpha = st.disabled ? .12 : .62;
  for (let i = 0; i < 4; i++) {
    const a = spin * .8 + i * Math.PI / 2;
    const x = Math.cos(a) * r * .92, y = Math.sin(a) * r * .54;
    ctx.beginPath();
    ctx.arc(x, y, r * .11, 0, Math.PI * 2);
    ctx.stroke();
    ctx.beginPath();
    ctx.moveTo(x * .50, y * .50);
    ctx.quadraticCurveTo(0, 0, x * .78, y * .72);
    ctx.stroke();
  }
  core(ctx, r * .49, st.color, st.pulse);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderChainSphereRuntimeVfx;
