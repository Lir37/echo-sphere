import type { PlayerState, SphereEntity } from '../engine';
import { core, drawSphereOrbit, finishDisabled, orbitPoint, stateColor, glow } from './visualHelpers';

function drawVoidEmitter(ctx: CanvasRenderingContext2D, r: number, color: string, disabled: boolean): void {
  ctx.save();
  ctx.translate(r * 1.20, 0);
  ctx.globalAlpha = disabled ? .12 : .94;
  ctx.fillStyle = '#03070e';
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.1;
  ctx.beginPath();
  ctx.moveTo(-r * .01, -r * .19);
  ctx.quadraticCurveTo(r * .21, -r * .08, r * .27, 0);
  ctx.quadraticCurveTo(r * .21, r * .08, -r * .01, r * .19);
  ctx.quadraticCurveTo(r * .07, 0, -r * .01, -r * .19);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.globalAlpha = disabled ? .06 : .65;
  ctx.strokeStyle = '#eadfff';
  ctx.beginPath();
  ctx.arc(r * .18, 0, r * .07, 0, Math.PI * 2);
  ctx.stroke();
  ctx.restore();
}

export function renderVoidSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  const st = stateColor(sphere, player, '#c28cff', time);
  const r = 24 * scale;
  const coreR = r;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.35, st.color, st.disabled ? .04 : .10);
  drawSphereOrbit(ctx, r * 1.03, st.color, time * .11, st.disabled ? .10 : .58, 'void', st.pulse);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, st.color, st.pulse * .92);

  ctx.rotate(sphere.rotation || 0);
  drawVoidEmitter(ctx, r, st.color, st.disabled);

  for (let i = 0; i < 4; i++) {
    const a = -time * (.22 + i * .025) + i * Math.PI / 2;
    const p = orbitPoint(r, a, .34, 1.11);
    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.globalAlpha = st.disabled ? .05 : .32 * p.depth;
    ctx.strokeStyle = st.color;
    ctx.lineWidth = .9;
    ctx.beginPath();
    ctx.arc(0, 0, r * (.07 + i * .012), Math.PI * .2, Math.PI * 1.8);
    ctx.stroke();
    ctx.restore();
  }

  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderVoidSphereRuntimeVfx;
