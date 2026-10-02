import type { PlayerState, SphereEntity } from '../engine';
import { getSphereElementForBranch, SPHERE_ELEMENT_META, type SphereElement } from '../sphereProgression';

const TAU = Math.PI * 2;

function rgba(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${alpha})`;
}

function drawFlame(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.moveTo(0, -size);
  ctx.quadraticCurveTo(size * .9, -size * .20, size * .42, size * .58);
  ctx.quadraticCurveTo(0, size, -size * .42, size * .58);
  ctx.quadraticCurveTo(-size * .9, -size * .20, 0, -size);
  ctx.stroke();
}

function drawSnowflake(ctx: CanvasRenderingContext2D, size: number): void {
  for (let i = 0; i < 3; i++) {
    const a = i * Math.PI / 3;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * size * .15, Math.sin(a) * size * .15);
    ctx.lineTo(Math.cos(a) * size, Math.sin(a) * size);
    ctx.moveTo(Math.cos(a) * size * .62, Math.sin(a) * size * .62);
    ctx.lineTo(Math.cos(a + .34) * size * .84, Math.sin(a + .34) * size * .84);
    ctx.moveTo(Math.cos(a) * size * .62, Math.sin(a) * size * .62);
    ctx.lineTo(Math.cos(a - .34) * size * .84, Math.sin(a - .34) * size * .84);
    ctx.stroke();
  }
}

function drawPoisonCloud(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.arc(-size * .30, size * .08, size * .34, 0, TAU);
  ctx.arc(0, -size * .10, size * .42, 0, TAU);
  ctx.arc(size * .34, size * .08, size * .30, 0, TAU);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(size * .10, -size * .48, size * .11, 0, TAU);
  ctx.stroke();
}

function drawElementParticle(ctx: CanvasRenderingContext2D, element: SphereElement, x: number, y: number, size: number, angle: number, alpha: number): void {
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle + Math.PI / 2);
  ctx.globalAlpha = alpha;
  if (element === 'fire') drawFlame(ctx, size);
  else if (element === 'freeze') drawSnowflake(ctx, size);
  else drawPoisonCloud(ctx, size);
  ctx.restore();
}

function drawElementRing(ctx: CanvasRenderingContext2D, element: SphereElement, radius: number, time: number, active: boolean, scale: number): void {
  const color = SPHERE_ELEMENT_META[element].color;
  const spin = time * (element === 'fire' ? .82 : element === 'freeze' ? -.68 : .72);
  const ringAlpha = active ? .62 : .18;

  ctx.save();
  ctx.globalCompositeOperation = 'lighter';
  ctx.strokeStyle = rgba(color, ringAlpha * .36);
  ctx.lineWidth = Math.max(.7, 1.0 * scale);
  ctx.beginPath();
  ctx.ellipse(0, 0, radius, radius * .66, 0, 0, TAU);
  ctx.stroke();

  ctx.strokeStyle = rgba(color, ringAlpha);
  ctx.lineWidth = Math.max(.9, 1.35 * scale);
  ctx.beginPath();
  ctx.ellipse(0, 0, radius, radius * .66, 0, spin + .32, spin + 1.92);
  ctx.stroke();
  ctx.beginPath();
  ctx.ellipse(0, 0, radius, radius * .66, 0, spin + Math.PI + .32, spin + Math.PI + 1.92);
  ctx.stroke();

  for (let i = 0; i < 6; i++) {
    const a = spin + i * TAU / 6;
    const x = Math.cos(a) * radius;
    const y = Math.sin(a) * radius * .66;
    const depth = .34 + .66 * ((Math.sin(a) + 1) * .5);
    drawElementParticle(ctx, element, x, y, Math.max(1.8, radius * .055), a, ringAlpha * depth);
  }
  ctx.restore();
}

function drawElementWeaponAccent(ctx: CanvasRenderingContext2D, element: SphereElement, sphere: SphereEntity, radius: number, active: boolean, scale: number, time: number): void {
  if (sphere.type === 'orbital') return;
  const color = SPHERE_ELEMENT_META[element].color;
  const direction = Number.isFinite(sphere.rotation) ? sphere.rotation : 0;
  const pulse = .75 + .25 * Math.sin(time * 8 + sphere.pos.x * .01);
  const reach = radius * 1.05;

  ctx.save();
  ctx.translate(sphere.pos.x + Math.cos(direction) * reach, sphere.pos.y + Math.sin(direction) * reach);
  ctx.rotate(direction);
  ctx.globalCompositeOperation = 'lighter';
  ctx.globalAlpha = (active ? .72 : .18) * pulse;
  ctx.strokeStyle = color;
  ctx.fillStyle = rgba(color, .16);
  ctx.lineWidth = Math.max(.85, 1.1 * scale);

  if (element === 'fire') drawFlame(ctx, radius * .12);
  else if (element === 'freeze') drawSnowflake(ctx, radius * .11);
  else drawPoisonCloud(ctx, radius * .10);

  ctx.globalAlpha *= .56;
  ctx.beginPath();
  ctx.arc(radius * .10, 0, radius * .10, 0, TAU);
  ctx.fill();
  ctx.restore();
}

export function renderSphereElementalVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1): void {
  const branch = player?.sphereBranches?.[sphere.type];
  const element = getSphereElementForBranch(branch);
  if (!element) return;

  const active = sphere.alive && sphere.networkDisabledTimer <= 0;
  const radius = 24 * scale * (sphere.type === 'orbital' ? 1.46 : 1.34);

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  drawElementRing(ctx, element, radius, time, active, scale);
  ctx.restore();

  drawElementWeaponAccent(ctx, element, sphere, 24 * scale, active, scale, time);
}
