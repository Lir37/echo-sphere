import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawPrismEmitter(ctx: CanvasRenderingContext2D, r: number, color: string, disabled: boolean): void {
  ctx.save();
  ctx.translate(r * 1.18, 0);
  ctx.globalAlpha = disabled ? .12 : .94;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.05;
  ctx.beginPath();
  ctx.moveTo(0, -r * .20);
  ctx.lineTo(r * .17, -r * .06);
  ctx.lineTo(r * .27, 0);
  ctx.lineTo(r * .17, r * .06);
  ctx.lineTo(0, r * .20);
  ctx.lineTo(r * .08, 0);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();

  ctx.globalAlpha = disabled ? .06 : .68;
  ctx.strokeStyle = '#ffffff';
  ctx.beginPath();
  ctx.moveTo(r * .09, -r * .10);
  ctx.lineTo(r * .21, 0);
  ctx.lineTo(r * .09, r * .10);
  ctx.stroke();
  ctx.restore();
}

export function renderPrismSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#ff8de1', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.40, st.color, st.disabled ? .05 : .12);
  drawSphereOrbit(ctx, r * 1.03, st.color, time * .16, st.disabled ? .12 : .66, 'prism', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);

  ctx.rotate(sphere.rotation || 0);
  drawPrismEmitter(ctx, r, st.color, st.disabled);

  for (let i = 0; i < 2; i++) {
    const a = time * (.30 + i * .05) + i * Math.PI;
    const p = orbitPoint(r, a, .34, 1.10);
    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.rotate(a + Math.PI / 2);
    ctx.globalAlpha = st.disabled ? .06 : .42 * p.depth;
    ctx.fillStyle = '#07111d';
    ctx.strokeStyle = st.color;
    ctx.lineWidth = .85;
    ctx.beginPath();
    ctx.moveTo(0, -r * .09);
    ctx.lineTo(r * .07, 0);
    ctx.lineTo(0, r * .09);
    ctx.lineTo(-r * .07, 0);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();
    ctx.restore();
  }

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderPrismSphereRuntimeVfx;
