import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawFieldBlade(ctx: CanvasRenderingContext2D, r: number, angle: number, color: string, alpha: number): void {
  const p = orbitPoint(r, angle, .34, 1.08);
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.rotate(angle + Math.PI / 2);
  ctx.globalAlpha = alpha * p.depth;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = .9;
  ctx.beginPath();
  ctx.moveTo(-r * .10, 0);
  ctx.lineTo(0, -r * .14);
  ctx.lineTo(r * .11, 0);
  ctx.lineTo(0, r * .14);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.restore();
}

export function renderAuraSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#57e6b4', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.45, st.color, st.disabled ? .05 : .12);
  drawSphereOrbit(ctx, r * 1.03, st.color, time * .14, st.disabled ? .12 : .68, 'aura', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);

  for (let i = 0; i < 6; i++) {
    drawFieldBlade(ctx, r, i * Math.PI / 3 + time * .10, st.color, st.disabled ? .07 : .48);
  }

  const q = .5 + .5 * Math.sin(time * 2.7);
  ctx.globalAlpha = st.disabled ? .05 : .16 + q * .08;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.0;
  ctx.beginPath();
  ctx.arc(0, 0, r * (1.04 + q * .06), 0, Math.PI * 2);
  ctx.stroke();

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderAuraSphereRuntimeVfx;
