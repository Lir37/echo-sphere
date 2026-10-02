import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawConductor(ctx: CanvasRenderingContext2D, r: number, angle: number, color: string, alpha: number): void {
  const p = orbitPoint(r, angle, .34, 1.10);
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.rotate(angle + Math.PI / 2);
  ctx.globalAlpha = alpha * p.depth;
  ctx.strokeStyle = color;
  ctx.lineWidth = Math.max(.8, r * .032);
  ctx.beginPath();
  ctx.moveTo(-r * .08, 0);
  ctx.lineTo(-r * .01, -r * .07);
  ctx.lineTo(r * .07, r * .03);
  ctx.lineTo(r * .14, -r * .02);
  ctx.lineTo(r * .21, r * .06);
  ctx.stroke();
  ctx.restore();
}

export function renderChainSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#ffe25b', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.35, st.color, st.disabled ? .05 : .11);
  drawSphereOrbit(ctx, r * 1.02, st.color, time * .17, st.disabled ? .12 : .78, 'lightning', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);

  for (let i = 0; i < 4; i++) {
    drawConductor(ctx, r, i * Math.PI / 2 + time * .28, st.color, st.disabled ? .10 : .56);
  }

  const surge = .5 + .5 * Math.sin(time * 10);
  ctx.globalAlpha = st.disabled ? .06 : .20 + surge * .16;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.arc(0, 0, coreR * (1.04 + surge * .08), 0, Math.PI * 2);
  ctx.stroke();

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderChainSphereRuntimeVfx;
