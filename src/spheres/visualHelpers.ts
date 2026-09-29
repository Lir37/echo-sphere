import type { PlayerState, SphereEntity } from '../engine';

export const WHITE = '#ffffff';
export const RESONANCE = '#ffd166';
export const DISABLED = '#ff4d70';
const GRADIENTS = new WeakMap<CanvasRenderingContext2D, Map<string, CanvasGradient>>();

function gradientsOf(ctx: CanvasRenderingContext2D): Map<string, CanvasGradient> {
  let map = GRADIENTS.get(ctx);
  if (map) return map;
  map = new Map<string, CanvasGradient>();
  GRADIENTS.set(ctx, map);
  return map;
}

function rgbaFromHex(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${alpha})`;
}

export function stateColor(sphere: SphereEntity, player: PlayerState, base: string, time: number): {
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
    pulse: disabled ? .5 : .5 + .5 * Math.sin(time * 3.4 + sphere.pos.x * .008 + sphere.pos.y * .006),
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

export function core(ctx: CanvasRenderingContext2D, radius: number, color: string, pulse: number, scale = 1): void {
  ctx.save();
  ctx.scale(scale, scale);
  glow(ctx, radius * 1.7, color, .17 + pulse * .08);
  ctx.globalAlpha = .20;
  ctx.fillStyle = color;
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.arc(0, 0, radius, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();
  ctx.globalAlpha = .95;
  ctx.fillStyle = WHITE;
  ctx.beginPath();
  ctx.arc(-radius * .08, -radius * .06, radius * (.14 + pulse * .025), 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();
}

export function drawCrescent(ctx: CanvasRenderingContext2D, radius: number, color: string, rotation: number, alpha: number, squash = .62): void {
  ctx.save();
  ctx.rotate(rotation);
  ctx.globalAlpha = alpha;
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.7;
  ctx.beginPath();
  ctx.moveTo(radius, 0);
  ctx.quadraticCurveTo(radius * .18, -radius * squash, -radius * .58, -radius * .12);
  ctx.quadraticCurveTo(-radius * .06, 0, -radius * .58, radius * .12);
  ctx.quadraticCurveTo(radius * .18, radius * squash, radius, 0);
  ctx.stroke();
  ctx.restore();
}

export function finishDisabled(ctx: CanvasRenderingContext2D, radius: number, color: string, disabled: boolean): void {
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
