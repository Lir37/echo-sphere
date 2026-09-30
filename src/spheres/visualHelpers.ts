import type { PlayerState, SphereEntity } from '../engine';

export const WHITE = '#ffffff';
export const RESONANCE = '#ffd166';
export const DISABLED = '#ff4d70';
const GRADIENTS = new WeakMap<CanvasRenderingContext2D, Map<string, CanvasGradient>>();
const CORE_GRADIENTS = new WeakMap<CanvasRenderingContext2D, Map<string, CanvasGradient>>();

function gradientsOf(ctx: CanvasRenderingContext2D): Map<string, CanvasGradient> {
  let map = GRADIENTS.get(ctx);
  if (map) return map;
  map = new Map<string, CanvasGradient>();
  GRADIENTS.set(ctx, map);
  return map;
}

function coreGradientsOf(ctx: CanvasRenderingContext2D): Map<string, CanvasGradient> {
  let map = CORE_GRADIENTS.get(ctx);
  if (map) return map;
  map = new Map<string, CanvasGradient>();
  CORE_GRADIENTS.set(ctx, map);
  return map;
}

function rgbaFromHex(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${alpha})`;
}

export function stateColor(
  sphere: SphereEntity,
  player: PlayerState,
  base: string,
  time: number,
): {
  color: string;
  disabled: boolean;
  resonance: boolean;
  pulse: number;
  animationTime: number;
} {
  const disabled = sphere.networkDisabledTimer > 0 || !sphere.alive;
  const resonance = !disabled && !!player?.resonanceEventActive;
  return {
    color: disabled ? DISABLED : resonance ? RESONANCE : base,
    disabled,
    resonance,
    pulse: disabled
      ? .5
      : .5 + .5 * Math.sin(time * 3.4 + sphere.pos.x * .008 + sphere.pos.y * .006),
    animationTime: disabled ? 0 : time,
  };
}

export function glow(ctx: CanvasRenderingContext2D, radius: number, color: string, alpha = .18): void {
  const map = gradientsOf(ctx);
  let g = map.get(color);
  if (!g) {
    g = ctx.createRadialGradient(0, 0, 0, 0, 0, 1);
    g.addColorStop(0, rgbaFromHex(color, .72));
    g.addColorStop(.20, rgbaFromHex(color, .38));
    g.addColorStop(.55, rgbaFromHex(color, .12));
    g.addColorStop(1, rgbaFromHex(color, 0));
    map.set(color, g);
  }
  ctx.save();
  ctx.globalAlpha = alpha;
  ctx.fillStyle = g;
  ctx.scale(radius, radius);
  ctx.beginPath();
  ctx.arc(0, 0, 1, 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();
}

/**
 * Family 2.5D core based on the approved Standard Sphere core:
 * luminous asymmetric fill, bright upper-left highlight and dark lower rim.
 * This is intentionally a soft volumetric sphere, not a flat polygon.
 */
export function core(
  ctx: CanvasRenderingContext2D,
  radius: number,
  color: string,
  pulse: number,
  scale = 1,
): void {
  ctx.save();
  ctx.scale(scale, scale);

  glow(ctx, radius * 1.85, color, .14 + pulse * .08);

  const map = coreGradientsOf(ctx);
  const key = `${color}:${Math.round(radius * 10)}`;
  let body = map.get(key);
  if (!body) {
    body = ctx.createRadialGradient(
      -radius * .20,
      -radius * .22,
      radius * .04,
      radius * .06,
      radius * .10,
      radius * 1.05,
    );
    body.addColorStop(0, 'rgba(255,255,255,.98)');
    body.addColorStop(.14, rgbaFromHex(color, .97));
    body.addColorStop(.38, rgbaFromHex(color, .80));
    body.addColorStop(.68, 'rgba(15,43,70,.90)');
    body.addColorStop(.88, 'rgba(5,17,31,.98)');
    body.addColorStop(1, 'rgba(1,7,15,1)');
    map.set(key, body);
  }

  const breathing = 1 + pulse * .028;
  ctx.save();
  ctx.scale(breathing, breathing);
  ctx.globalCompositeOperation = 'source-over';
  ctx.globalAlpha = .96;
  ctx.fillStyle = body;
  ctx.strokeStyle = rgbaFromHex(color, .88);
  ctx.lineWidth = Math.max(.9, radius * .035);
  ctx.beginPath();
  ctx.arc(0, 0, radius, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();

  // Soft 2.5D specular patch. It has no discrete point, so the center reads
  // as a volume instead of a blinking LED.
  const hi = ctx.createRadialGradient(
    -radius * .34,
    -radius * .38,
    0,
    -radius * .34,
    -radius * .38,
    radius * .52,
  );
  hi.addColorStop(0, 'rgba(255,255,255,.32)');
  hi.addColorStop(.45, 'rgba(220,250,255,.10)');
  hi.addColorStop(1, 'rgba(220,250,255,0)');
  ctx.globalAlpha = .95;
  ctx.fillStyle = hi;
  ctx.beginPath();
  ctx.arc(0, 0, radius * .92, 0, Math.PI * 2);
  ctx.fill();

  ctx.globalAlpha = .42;
  ctx.strokeStyle = 'rgba(2,8,16,.92)';
  ctx.lineWidth = Math.max(.8, radius * .032);
  ctx.beginPath();
  ctx.arc(0, radius * .08, radius * .90, Math.PI * .08, Math.PI * .92);
  ctx.stroke();
  ctx.restore();

  ctx.restore();
}

/**
 * Standard-derived double orbital language.
 * mode changes material/segmentation only; the family keeps the same
 * readable elliptical orbit proportions and front/back separation.
 */
export type SphereOrbitMode =
  | 'standard'
  | 'lightning'
  | 'sniper'
  | 'shotgun'
  | 'aura'
  | 'prism'
  | 'gravity'
  | 'pulse'
  | 'void'
  | 'orbital';

function ellipsePoint(radiusX: number, radiusY: number, a: number): { x: number; y: number } {
  return { x: Math.cos(a) * radiusX, y: Math.sin(a) * radiusY };
}

function drawEllipseTrace(
  ctx: CanvasRenderingContext2D,
  radiusX: number,
  radiusY: number,
  start: number,
  end: number,
  wobble = 0,
): void {
  const steps = 42;
  ctx.beginPath();
  for (let i = 0; i <= steps; i++) {
    const p = i / steps;
    const a = start + (end - start) * p;
    const n = wobble ? Math.sin(a * 7.0 + 0.35) * wobble : 0;
    const x = Math.cos(a) * (radiusX + n);
    const y = Math.sin(a) * (radiusY + n * .34);
    if (i === 0) ctx.moveTo(x, y);
    else ctx.lineTo(x, y);
  }
  ctx.stroke();
}

function drawLightningEllipse(
  ctx: CanvasRenderingContext2D,
  radiusX: number,
  radiusY: number,
  rotation: number,
  color: string,
  alpha: number,
  pulse: number,
): void {
  ctx.save();
  ctx.rotate(rotation);
  ctx.strokeStyle = color;
  ctx.lineWidth = Math.max(.8, radiusX * .028);
  ctx.globalAlpha = alpha;
  const segments = 20;
  ctx.beginPath();
  for (let i = 0; i <= segments; i++) {
    const a = (i / segments) * Math.PI * 2;
    const jitter = Math.sin(i * 3.7 + pulse * 5.2) * radiusX * .025;
    const p = ellipsePoint(radiusX + jitter, radiusY + jitter * .34, a);
    if (i === 0) ctx.moveTo(p.x, p.y);
    else ctx.lineTo(p.x, p.y);
  }
  ctx.stroke();
  ctx.restore();
}

export function drawSphereOrbit(
  ctx: CanvasRenderingContext2D,
  radius: number,
  color: string,
  rotation: number,
  opacity = .72,
  mode: SphereOrbitMode = 'standard',
  pulse = .5,
): void {
  const rx = radius * 1.16;
  const ry = radius * .34;
  ctx.save();
  ctx.rotate(rotation);

  if (mode === 'lightning') {
    drawLightningEllipse(ctx, rx, ry, 0, color, opacity, pulse);
    drawLightningEllipse(ctx, radius * .98, radius * .28, 0, color, opacity * .36, pulse + .5);
    ctx.restore();
    return;
  }

  ctx.globalCompositeOperation = 'lighter';
  ctx.strokeStyle = color;
  ctx.lineCap = 'round';

  if (mode === 'void') {
    ctx.globalAlpha = opacity * .52;
    ctx.lineWidth = Math.max(.7, radius * .032);
    for (let i = 0; i < 6; i++) {
      const a = i * Math.PI / 3 + .18;
      const span = .32 + (i % 2) * .10;
      drawEllipseTrace(ctx, rx, ry, a, a + span, .7);
    }
    ctx.restore();
    return;
  }

  if (mode === 'pulse') {
    ctx.globalAlpha = opacity * .76;
    ctx.lineWidth = Math.max(.9, radius * .035);
    ctx.setLineDash([radius * .12, radius * .10]);
    drawEllipseTrace(ctx, rx, ry, 0, Math.PI * 2);
    ctx.setLineDash([]);
    ctx.globalAlpha = opacity * .34;
    ctx.lineWidth = Math.max(.65, radius * .024);
    drawEllipseTrace(ctx, radius * .96, radius * .27, Math.PI * .16, Math.PI * 1.86);
    ctx.restore();
    return;
  }

  ctx.globalAlpha = opacity;
  ctx.lineWidth = Math.max(.9, radius * .040);
  drawEllipseTrace(ctx, rx, ry, 0, Math.PI * 2);

  ctx.globalAlpha = opacity * .44;
  ctx.lineWidth = Math.max(.55, radius * .022);
  drawEllipseTrace(ctx, radius * 1.00, radius * .285, Math.PI * .10, Math.PI * 1.92);

  if (mode === 'sniper') {
    ctx.globalAlpha = opacity * .68;
    ctx.lineWidth = Math.max(.6, radius * .020);
    for (const a of [-.56, -.28, 0, .28, .56]) {
      const p = ellipsePoint(rx, ry, a);
      const nx = Math.cos(a);
      const ny = Math.sin(a);
      ctx.beginPath();
      ctx.moveTo(p.x - nx * radius * .045, p.y - ny * radius * .045);
      ctx.lineTo(p.x + nx * radius * .075, p.y + ny * radius * .075);
      ctx.stroke();
    }
  } else if (mode === 'shotgun') {
    ctx.globalAlpha = opacity * .72;
    ctx.lineWidth = Math.max(.7, radius * .024);
    for (let i = 0; i < 3; i++) {
      const a = -.44 + i * .44;
      const p = ellipsePoint(rx, ry, a);
      ctx.beginPath();
      ctx.arc(p.x, p.y, radius * .065, 0, Math.PI * 2);
      ctx.stroke();
    }
  } else if (mode === 'aura') {
    ctx.globalAlpha = opacity * .64;
    ctx.lineWidth = Math.max(.6, radius * .018);
    for (let i = 0; i < 6; i++) {
      const a = i * Math.PI / 3 + pulse * .10;
      drawEllipseTrace(ctx, rx * .96, ry * .96, a - .26, a + .26);
    }
  } else if (mode === 'prism') {
    ctx.globalAlpha = opacity * .74;
    ctx.lineWidth = Math.max(.75, radius * .024);
    ctx.setLineDash([radius * .15, radius * .07]);
    drawEllipseTrace(ctx, rx * 1.01, ry * 1.02, .08, Math.PI * 1.95);
    ctx.setLineDash([]);
  } else if (mode === 'gravity') {
    ctx.globalAlpha = opacity * .56;
    ctx.lineWidth = Math.max(.7, radius * .026);
    drawEllipseTrace(ctx, rx * .88, ry * .78, .25, Math.PI * 1.78);
    drawEllipseTrace(ctx, rx * 1.05, ry * .86, Math.PI * 1.08, Math.PI * 2.64);
  } else if (mode === 'orbital') {
    ctx.globalAlpha = opacity * .52;
    ctx.lineWidth = Math.max(.65, radius * .022);
    drawEllipseTrace(ctx, rx * .92, ry * .84, Math.PI * .12, Math.PI * 1.86);
  }

  ctx.restore();
}

export function orbitPoint(
  radius: number,
  angle: number,
  flatten = .34,
  radiusScale = 1.16,
): { x: number; y: number; depth: number } {
  const s = Math.sin(angle);
  return {
    x: Math.cos(angle) * radius * radiusScale,
    y: s * radius * flatten * radiusScale,
    depth: .64 + .36 * ((s + 1) * .5),
  };
}

export function finishDisabled(
  ctx: CanvasRenderingContext2D,
  radius: number,
  color: string,
  disabled: boolean,
): void {
  if (!disabled) return;
  ctx.save();
  ctx.globalAlpha = .42;
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.arc(0, 0, radius * 1.18, Math.PI * .20, Math.PI * .80);
  ctx.stroke();
  ctx.restore();
}
