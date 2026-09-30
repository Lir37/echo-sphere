import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawGravityAnchor(ctx: CanvasRenderingContext2D, r: number, angle: number, color: string, alpha: number): void {
  const p = orbitPoint(r, angle, .34, 1.08);
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.rotate(angle);
  ctx.globalAlpha = alpha * p.depth;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = .9;
  ctx.beginPath();
  ctx.moveTo(-r * .09, 0);
  ctx.lineTo(0, -r * .12);
  ctx.lineTo(r * .12, 0);
  ctx.lineTo(0, r * .12);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.restore();
}

export function renderGravitySphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#a58cff', time);
  const r = 24 * scale;
  const coreR = r * .68;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.40, st.color, st.disabled ? .05 : .11);
  drawSphereOrbit(ctx, r * 1.03, st.color, -time * .12, st.disabled ? .12 : .65, 'gravity', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);

  for (let i = 0; i < 5; i++) {
    drawGravityAnchor(ctx, r, i * Math.PI * 2 / 5 + time * .08, st.color, st.disabled ? .06 : .44);
  }

  const pull = .5 + .5 * Math.sin(time * 3.1);
  ctx.globalAlpha = st.disabled ? .05 : .16 + pull * .08;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = .9;
  ctx.beginPath();
  ctx.arc(0, 0, coreR * (.98 + pull * .05), 0, Math.PI * 2);
  ctx.stroke();

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderGravitySphereRuntimeVfx;
