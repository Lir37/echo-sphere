import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

export function renderPulseSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#ffd35a', time);
  const r = 24 * scale;
  const coreR = r;
  const phase = st.animationTime * 1.10;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.45, st.color, st.disabled ? .05 : .12);
  drawSphereOrbit(ctx, r * 1.03, st.color, phase * .13, st.disabled ? .10 : .66, 'pulse', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);

  // Four small resonator plates rotate around the main orbit instead of
  // forming a bulky second body.
  for (let i = 0; i < 4; i++) {
    const a = phase * .28 + i * Math.PI / 2;
    const p = orbitPoint(r, a, .34, 1.10);
    const q = .5 + .5 * Math.sin(phase * 2.8 + i);
    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.rotate(a + Math.PI / 2);
    ctx.globalAlpha = st.disabled ? .06 : (.36 + q * .20) * p.depth;
    ctx.strokeStyle = st.color;
    ctx.fillStyle = '#07111d';
    ctx.lineWidth = .8;
    ctx.beginPath();
    ctx.moveTo(-r * .08, -r * .10);
    ctx.lineTo(r * .08, -r * .06);
    ctx.lineTo(r * .08, r * .06);
    ctx.lineTo(-r * .08, r * .10);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();
    ctx.restore();
  }

  const q = ((phase * .42) % 1 + 1) % 1;
  ctx.globalAlpha = st.disabled ? .06 : (1 - q) * .36;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.0;
  ctx.beginPath();
  ctx.arc(0, 0, r * (1.02 + q * .86), 0, Math.PI * 2);
  ctx.stroke();

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderPulseSphereRuntimeVfx;
