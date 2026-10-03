import type { PlayerState, SphereEntity } from '../engine';
import { getSphereElementForBranch, SPHERE_ELEMENT_META, type SphereElement } from '../sphereProgression';

const TAU = Math.PI * 2;

function rgba(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${alpha})`;
}

function drawFlame(ctx: CanvasRenderingContext2D, size: number, variant = 0): void {
  const lean = (variant - 1) * size * .12;
  ctx.beginPath();
  ctx.moveTo(lean, -size);
  ctx.quadraticCurveTo(size * (.92 + variant * .06), -size * .20, size * (.44 - variant * .05), size * .55);
  ctx.quadraticCurveTo(lean, size * .92, -size * (.46 + variant * .03), size * .58);
  ctx.quadraticCurveTo(-size * (.90 - variant * .04), -size * .18, lean, -size);
  ctx.closePath();
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(lean, -size * .34);
  ctx.quadraticCurveTo(size * .40, -size * .02, lean + size * .04, size * .46);
  ctx.quadraticCurveTo(-size * .30, size * .18, lean, -size * .34);
  ctx.stroke();
}

function drawSnowflake(ctx: CanvasRenderingContext2D, size: number, variant = 0): void {
  const arms = variant === 1 ? 4 : 6;
  for (let i = 0; i < arms; i++) {
    const a = i * TAU / arms;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * size * .14, Math.sin(a) * size * .14);
    ctx.lineTo(Math.cos(a) * size, Math.sin(a) * size);
    ctx.stroke();
    if (variant !== 2) {
      const tip = variant === 1 ? .68 : .62;
      const branch = variant === 1 ? .42 : .34;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * size * tip, Math.sin(a) * size * tip);
      ctx.lineTo(Math.cos(a + branch) * size * .88, Math.sin(a + branch) * size * .88);
      ctx.moveTo(Math.cos(a) * size * tip, Math.sin(a) * size * tip);
      ctx.lineTo(Math.cos(a - branch) * size * .88, Math.sin(a - branch) * size * .88);
      ctx.stroke();
    }
  }
  ctx.beginPath();
  ctx.arc(0, 0, size * (variant === 2 ? .28 : .20), 0, TAU);
  ctx.stroke();
}

function drawPoisonCloud(ctx: CanvasRenderingContext2D, size: number, variant = 0): void {
  const drift = (variant - 1) * size * .08;
  ctx.beginPath();
  ctx.arc(-size * .34 + drift, size * .10, size * .34, 0, TAU);
  ctx.arc(0, -size * (.12 + variant * .04), size * (.42 - variant * .03), 0, TAU);
  ctx.arc(size * .34 + drift, size * .08, size * (.29 + variant * .02), 0, TAU);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(size * (.10 - variant * .04), -size * (.50 + variant * .04), size * (.11 + variant * .02), 0, TAU);
  ctx.arc(-size * .22, -size * .42, size * .07, 0, TAU);
  ctx.stroke();
}

function drawElementParticle(
  ctx: CanvasRenderingContext2D,
  element: SphereElement,
  x: number,
  y: number,
  size: number,
  angle: number,
  alpha: number,
  variant = 0,
): void {
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle + Math.PI / 2);
  ctx.globalAlpha = alpha;
  if (element === 'fire') drawFlame(ctx, size, variant);
  else if (element === 'freeze') drawSnowflake(ctx, size, variant);
  else drawPoisonCloud(ctx, size, variant);
  ctx.restore();
}

function ellipsePoint(radiusX: number, radiusY: number, angle: number): { x: number; y: number } {
  return { x: Math.cos(angle) * radiusX, y: Math.sin(angle) * radiusY };
}

function drawElementRing(
  ctx: CanvasRenderingContext2D,
  element: SphereElement,
  radius: number,
  time: number,
  active: boolean,
  scale: number,
): void {
  const color = SPHERE_ELEMENT_META[element].color;
  const ringRx = radius * 1.16;
  const ringRy = radius * .34;

  // Standard Sphere's primary orbit advances clockwise. The elemental
  // orbit intentionally counters it and keeps the same flattened 2.5D
  // proportions and line-weight grammar.
  const ringSpin = -time * .18;
  const particleSpin = -time * .34;
  const ringAlpha = active ? .78 : .20;
  const mainWidth = Math.max(1.0, radius * .040);

  ctx.save();
  ctx.globalCompositeOperation = 'lighter';

  // Soft luminous bed keeps the ring substantial at gameplay scale.
  ctx.strokeStyle = rgba(color, ringAlpha * .18);
  ctx.lineWidth = Math.max(2.0, mainWidth * 3.0);
  ctx.beginPath();
  ctx.ellipse(0, 0, ringRx, ringRy, ringSpin, 0, TAU);
  ctx.stroke();

  // Back half first, then the brighter front half. The ring stays continuous,
  // avoiding the thin/dashed look of the previous implementation.
  ctx.strokeStyle = rgba(color, ringAlpha * .58);
  ctx.lineWidth = mainWidth;
  ctx.beginPath();
  ctx.ellipse(0, 0, ringRx, ringRy, ringSpin, Math.PI, TAU);
  ctx.stroke();

  ctx.strokeStyle = rgba(color, ringAlpha);
  ctx.lineWidth = mainWidth;
  ctx.beginPath();
  ctx.ellipse(0, 0, ringRx, ringRy, ringSpin, 0, Math.PI);
  ctx.stroke();

  // Restrained moving highlight gives the orbit a live energy flow without
  // converting the main ring into a segmented line.
  ctx.strokeStyle = rgba(color, ringAlpha * .72);
  ctx.lineWidth = Math.max(.75, mainWidth * .55);
  ctx.beginPath();
  ctx.ellipse(0, 0, ringRx * .995, ringRy * .995, ringSpin, particleSpin % TAU, particleSpin % TAU + .72);
  ctx.stroke();

  // Elemental motes travel along the same flattened orbit, opposite to the
  // standard Sphere orbit. Each type has authored glyph variants.
  const particleCount = 6;
  for (let i = 0; i < particleCount; i++) {
    const a = particleSpin + i * TAU / particleCount;
    const p = ellipsePoint(ringRx, ringRy, a);
    const depth = .38 + .62 * ((Math.sin(a) + 1) * .5);
    const bob = Math.sin(time * 2.8 + i * 1.73) * radius * .018;
    const size = Math.max(2.2, radius * (.055 + (i % 3) * .008));

    drawElementParticle(
      ctx,
      element,
      p.x,
      p.y + bob,
      size,
      a,
      ringAlpha * (.72 + depth * .28),
      i % 3,
    );

    // Small secondary motes make the elemental layer feel alive without
    // making the ring visually noisy.
    if (i % 2 === 0) {
      const side = i % 4 === 0 ? 1 : -1;
      const moteA = a + side * .16;
      const mote = ellipsePoint(ringRx * 1.035, ringRy * 1.18, moteA);
      ctx.save();
      ctx.globalAlpha = ringAlpha * (.28 + depth * .24);
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.arc(mote.x, mote.y, Math.max(.65, radius * .018), 0, TAU);
      ctx.fill();
      ctx.restore();
    }
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


function drawSpecialElementalSignature(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  element: SphereElement,
  radius: number,
  time: number,
  active: boolean,
): void {
  if (!['chain', 'aura', 'gravity', 'pulse'].includes(sphere.type)) return;
  const color = SPHERE_ELEMENT_META[element].color;
  const alpha = active ? 0.42 : 0.10;
  ctx.save();
  ctx.globalCompositeOperation = 'lighter';
  ctx.strokeStyle = color;
  ctx.fillStyle = rgba(color, alpha * 0.18);
  ctx.lineWidth = Math.max(0.8, radius * 0.024);

  if (sphere.type === 'chain') {
    for (let i = 0; i < 3; i++) {
      const a = time * 0.9 + i * TAU / 3;
      const x = Math.cos(a) * radius * 1.02;
      const y = Math.sin(a) * radius * 0.30;
      ctx.beginPath();
      ctx.moveTo(x, y);
      ctx.lineTo(x + Math.cos(a + 1.2) * radius * 0.18, y + Math.sin(a + 1.2) * radius * 0.12);
      ctx.stroke();
    }
  } else if (sphere.type === 'aura') {
    ctx.globalAlpha = alpha * 0.72;
    ctx.beginPath();
    ctx.ellipse(0, 0, radius * 1.85, radius * 0.58, 0, 0, TAU);
    ctx.stroke();
    for (let i = 0; i < 3; i++) {
      const a = -time * 0.45 + i * TAU / 3;
      const p = ellipsePoint(radius * 1.55, radius * 0.48, a);
      drawElementParticle(ctx, element, p.x, p.y, radius * 0.065, a, alpha * 0.65, i);
    }
  } else if (sphere.type === 'gravity') {
    for (let i = 0; i < 3; i++) {
      const q = ((time * 0.35 + i / 3) % 1 + 1) % 1;
      const rr = radius * (1.72 - q * 0.72);
      ctx.globalAlpha = alpha * (1 - q) * 0.8;
      ctx.beginPath();
      ctx.arc(0, 0, rr, -0.55 + i * 2.0, 0.55 + i * 2.0);
      ctx.stroke();
    }
  } else {
    const q = ((time * 0.72) % 1 + 1) % 1;
    ctx.globalAlpha = alpha * (1 - q);
    ctx.beginPath();
    ctx.arc(0, 0, radius * (0.72 + q * 1.15), 0, TAU);
    ctx.stroke();
  }
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
  drawSpecialElementalSignature(ctx, sphere, element, radius, time, active);
  ctx.restore();

  drawElementWeaponAccent(ctx, element, sphere, 24 * scale, active, scale, time);
}
