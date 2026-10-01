import type { PlayerState, SphereEntity, SphereMods } from '../engine';
import { sphereUsesProjectileModifiers } from '../gameData';
import { getAuthoredSphereModifierLevels } from '../sphereProgression';

const TAU = Math.PI * 2;

type ModifierKind = keyof SphereMods;
type CoreModifierKind = 'multishot' | 'pierce' | 'ricochet' | 'fire' | 'freeze' | 'poison';

const MODIFIER_COLOR: Record<CoreModifierKind, string> = {
  multishot: '#dff8ff',
  pierce: '#e9ffff',
  ricochet: '#b897ff',
  fire: '#ff743d',
  freeze: '#bff4ff',
  poison: '#6df0a9',
};

const MODIFIER_ORDER: CoreModifierKind[] = ['multishot', 'pierce', 'ricochet', 'fire', 'freeze', 'poison'];
const PROJECTILE_ONLY: ModifierKind[] = ['multishot', 'pierce', 'ricochet'];

function modifierValue(mods: SphereMods, kind: ModifierKind): number {
  const value = Number(mods?.[kind] ?? 0);
  return Number.isFinite(value) ? Math.max(0, value) : 0;
}

function drawFlame(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.moveTo(0, -size);
  ctx.quadraticCurveTo(size * .95, -size * .20, size * .55, size * .58);
  ctx.quadraticCurveTo(0, size, -size * .55, size * .58);
  ctx.quadraticCurveTo(-size * .95, -size * .20, 0, -size);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -size * .36);
  ctx.quadraticCurveTo(size * .42, 0, 0, size * .52);
  ctx.quadraticCurveTo(-size * .42, 0, 0, -size * .36);
  ctx.stroke();
}

function drawFreeze(ctx: CanvasRenderingContext2D, size: number): void {
  for (let i = 0; i < 6; i++) {
    const a = i * Math.PI / 3;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * size * .18, Math.sin(a) * size * .18);
    ctx.lineTo(Math.cos(a) * size, Math.sin(a) * size);
    ctx.lineWidth = Math.max(.7, size * .18);
    ctx.stroke();
  }
  ctx.beginPath();
  ctx.arc(0, 0, size * .22, 0, TAU);
  ctx.stroke();
}

function drawPoison(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.moveTo(0, -size);
  ctx.quadraticCurveTo(size * .86, -size * .20, size * .72, size * .34);
  ctx.quadraticCurveTo(size * .50, size, 0, size);
  ctx.quadraticCurveTo(-size * .50, size, -size * .72, size * .34);
  ctx.quadraticCurveTo(-size * .86, -size * .20, 0, -size);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -size * .48);
  ctx.lineTo(0, size * .52);
  ctx.stroke();
}

function drawPierce(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.moveTo(-size * 1.05, 0);
  ctx.lineTo(size * 1.05, 0);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(size * .55, -size * .32);
  ctx.lineTo(size * 1.05, 0);
  ctx.lineTo(size * .55, size * .32);
  ctx.moveTo(-size * .55, -size * .32);
  ctx.lineTo(-size * 1.05, 0);
  ctx.lineTo(-size * .55, size * .32);
  ctx.stroke();
}

function drawRicochet(ctx: CanvasRenderingContext2D, size: number): void {
  ctx.beginPath();
  ctx.moveTo(-size * .95, size * .12);
  ctx.lineTo(-size * .58, -size * .62);
  ctx.lineTo(size * .12, -size * .44);
  ctx.lineTo(size * .82, -size * .84);
  ctx.lineTo(size * .98, -.08 * size);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(size * .84, -size * .06, size * .18, 0, TAU);
  ctx.stroke();
}

function drawMultishot(ctx: CanvasRenderingContext2D, size: number, strength: number): void {
  const lanes = Math.min(4, 2 + Math.floor(strength * 2));
  const spread = size * .46;
  for (let i = 0; i < lanes; i++) {
    const y = (i - (lanes - 1) / 2) * spread;
    ctx.beginPath();
    ctx.moveTo(-size * .92, y * .42);
    ctx.quadraticCurveTo(0, y, size * .95, y * .34);
    ctx.stroke();
  }
}

function drawModifierGlyph(
  ctx: CanvasRenderingContext2D,
  kind: ModifierKind,
  angle: number,
  radius: number,
  value: number,
  time: number,
  scale: number,
): void {
  if (value <= 0) return;

  const strength = Math.min(1, value / 2);
  const size = Math.max(2.8, 3.8 * scale + strength * 1.25 * scale);
  const x = Math.cos(angle) * radius;
  const y = Math.sin(angle) * radius * .86;
  const color = MODIFIER_COLOR[kind];

  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle + Math.PI / 2 + time * .18);
  ctx.globalAlpha = .56 + strength * .25;
  ctx.strokeStyle = color;
  ctx.lineWidth = Math.max(.85, 1.05 * scale);
  ctx.lineJoin = 'round';
  ctx.lineCap = 'round';

  if (kind === 'multishot') drawMultishot(ctx, size, strength);
  else if (kind === 'pierce') drawPierce(ctx, size);
  else if (kind === 'ricochet') drawRicochet(ctx, size);
  else if (kind === 'fire') drawFlame(ctx, size);
  else if (kind === 'freeze') drawFreeze(ctx, size);
  else drawPoison(ctx, size);

  if (value >= 2) {
    ctx.globalAlpha = .34 + strength * .22;
    for (let i = 0; i < Math.min(3, Math.floor(value)); i++) {
      ctx.beginPath();
      ctx.arc(-size * .82 + i * size * .42, size * 1.28, Math.max(.65, .8 * scale), 0, TAU);
      ctx.stroke();
    }
  }
  ctx.restore();
}

export function renderSphereModifierVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
): void {
  if (!ctx || !sphere || !player?.sphereMods) return;

  const disabled = sphere.networkDisabledTimer > 0 || !sphere.alive;
  const authored = getAuthoredSphereModifierLevels(player, sphere.type);
  const radius = 24 * scale * 1.38;
  const activeTime = disabled ? 0 : time;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';

  for (let i = 0; i < MODIFIER_ORDER.length; i++) {
    const kind = MODIFIER_ORDER[i];
    if (PROJECTILE_ONLY.includes(kind) && !sphereUsesProjectileModifiers(sphere.type)) continue;
    const value = Math.max(modifierValue(player.sphereMods, kind), Number(authored[kind] ?? 0));
    const angle = -Math.PI / 2 + i * TAU / MODIFIER_ORDER.length + activeTime * .11;
    if (disabled) ctx.globalAlpha = .38;
    drawModifierGlyph(ctx, kind, angle, radius, value, activeTime, scale);
  }

  ctx.restore();
}

export default renderSphereModifierVfx;
