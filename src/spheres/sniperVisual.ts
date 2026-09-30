import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawSniperTip(ctx: CanvasRenderingContext2D, r: number, color: string, disabled: boolean, time: number): void {
  ctx.save();
  ctx.translate(r * 1.22, 0);
  ctx.rotate(-Math.PI / 2);
  ctx.globalAlpha = disabled ? .14 : .94;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.15;
  ctx.beginPath();
  ctx.moveTo(0, -r * .24);
  ctx.lineTo(r * .18, -r * .05);
  ctx.lineTo(r * .13, r * .24);
  ctx.lineTo(0, r * .34);
  ctx.lineTo(-r * .13, r * .24);
  ctx.lineTo(-r * .18, -r * .05);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.globalAlpha = disabled ? .08 : .70;
  ctx.strokeStyle = '#ffffff';
  ctx.beginPath();
  ctx.moveTo(0, -r * .18);
  ctx.lineTo(0, r * .16);
  ctx.moveTo(-r * .10, -r * .02);
  ctx.lineTo(r * .10, -r * .02);
  ctx.stroke();
  const scan = .10 + .05 * Math.sin(time * 7);
  ctx.globalAlpha = disabled ? .04 : scan;
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.arc(0, 0, r * .13, 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();
}

export function renderSniperSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#e86cff', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.35, st.color, st.disabled ? .05 : .11);
  drawSphereOrbit(ctx, r * 1.02, st.color, time * .20, st.disabled ? .16 : .68, 'sniper', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse);
  ctx.rotate(sphere.rotation || 0);
  drawSniperTip(ctx, r, st.color, st.disabled, time);

  const a = time * .46 + Math.PI * .72;
  const p = orbitPoint(r, a, .34, 1.15);
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.globalAlpha = st.disabled ? .08 : .62 * p.depth;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = .9;
  ctx.beginPath();
  ctx.arc(0, 0, r * .10, 0, Math.PI * 2);
  ctx.moveTo(-r * .18, 0); ctx.lineTo(r * .18, 0);
  ctx.moveTo(0, -r * .18); ctx.lineTo(0, r * .18);
  ctx.stroke();
  ctx.restore();

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderSniperSphereRuntimeVfx;
