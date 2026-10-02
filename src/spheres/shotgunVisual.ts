import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawShotgunTip(ctx: CanvasRenderingContext2D, r: number, y: number, color: string, disabled: boolean): void {
  ctx.save();
  ctx.translate(r * 1.18, y);
  ctx.globalAlpha = disabled ? .12 : .92;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.1;
  ctx.beginPath();
  ctx.moveTo(-r * .02, -r * .10);
  ctx.lineTo(r * .22, -r * .11);
  ctx.lineTo(r * .30, 0);
  ctx.lineTo(r * .22, r * .11);
  ctx.lineTo(-r * .02, r * .10);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.globalAlpha = disabled ? .07 : .72;
  ctx.strokeStyle = '#ffffff';
  ctx.beginPath();
  ctx.arc(r * .25, 0, r * .055, 0, Math.PI * 2);
  ctx.stroke();
  ctx.restore();
}

export function renderShotgunSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#ff8f3d', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.40, st.color, st.disabled ? .05 : .12);
  drawSphereOrbit(ctx, r * 1.02, st.color, time * .18, st.disabled ? .15 : .64, 'shotgun', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);
  ctx.rotate(sphere.rotation || 0);
  drawShotgunTip(ctx, r, -r * .25, st.color, st.disabled);
  drawShotgunTip(ctx, r, 0, st.color, st.disabled);
  drawShotgunTip(ctx, r, r * .25, st.color, st.disabled);

  for (let i = 0; i < 3; i++) {
    const a = time * (.34 + i * .035) + i * Math.PI * 2 / 3;
    const p = orbitPoint(r, a, .34, 1.12);
    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.globalAlpha = st.disabled ? .08 : .48 * p.depth;
    ctx.strokeStyle = st.color;
    ctx.lineWidth = .8;
    ctx.beginPath();
    ctx.moveTo(-r * .10, 0);
    ctx.lineTo(r * .10, 0);
    ctx.stroke();
    ctx.restore();
  }

  if (Math.sin(time * 8) > .6) {
    ctx.globalAlpha = st.disabled ? .05 : .22;
    ctx.strokeStyle = st.color;
    ctx.lineWidth = .9;
    ctx.beginPath();
    ctx.arc(r * 1.43, 0, r * .11, 0, Math.PI * 2);
    ctx.stroke();
  }

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderShotgunSphereRuntimeVfx;
