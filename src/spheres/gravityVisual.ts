import type { PlayerState, SphereEntity } from '../engine';
import { core, finishDisabled, stateColor, glow } from './visualHelpers';

export function renderGravitySphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const st = stateColor(sphere, player, '#a58cff', time);
  const r = 24 * scale, rot = st.animationTime * -.12;
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.35, st.color, st.disabled ? .05 : .15);
  ctx.restore();

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.rotate(rot);
  ctx.globalAlpha = st.disabled ? .18 : .86;
  ctx.strokeStyle = st.color;
  ctx.lineWidth = 1.7;
  for (let i = 0; i < 3; i++) {
    const rr = r * (1.05 + i * .30);
    ctx.beginPath();
    ctx.arc(0, 0, rr, Math.PI * (.18 + i * .22), Math.PI * (1.56 + i * .22));
    ctx.stroke();
  }
  ctx.globalAlpha = st.disabled ? .10 : .58;
  for (let i = 0; i < 6; i++) {
    const a = i * Math.PI / 3;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * r * 1.28, Math.sin(a) * r * 1.28);
    ctx.quadraticCurveTo(Math.cos(a + .25) * r * .64, Math.sin(a + .25) * r * .42, 0, 0);
    ctx.stroke();
  }
  ctx.globalAlpha = st.disabled ? .12 : .42;
  ctx.beginPath();
  ctx.arc(0, 0, r * .74, 0, Math.PI * 2);
  ctx.stroke();
  core(ctx, r * .44, st.color, st.pulse * .7);
  finishDisabled(ctx, r, st.color, st.disabled);
  ctx.restore();
}
export default renderGravitySphereRuntimeVfx;
