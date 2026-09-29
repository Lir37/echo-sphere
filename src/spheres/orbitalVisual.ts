import type { EnemyEntity, PlayerState, SphereEntity } from '../engine';

const TAU = Math.PI * 2;
const EPS = 0.0001;

export const ORBITAL_VISUAL = Object.freeze({
  BASE_RADIUS_PX: 24,
  BODY_COLOR: '#06111d', EDGE_COLOR: '#8ef0ff', EDGE_HOT: '#e9fcff',
  CORE_HOT: '#ffffff', CORE_COLOR: '#63d9ff', CORE_DEEP: '#063b8e',
  RESONANCE_COLOR: '#ffd166', DISABLED_COLOR: '#ff4d70',
  ORBIT_RX: 62, ORBIT_RY: 34.1, ORBIT_ROT: -0.04,
  IDLE_ORBIT_SPEED: 0.42, CORE_PULSE_SPEED: 3.2, BOB_SPEED: 1.4, BOB_AMPLITUDE: 0.9,
  ATTACK_DURATION: 0.19, IMPACT_FLASH_DURATION: 0.10, RESONANCE_DURATION: 0.76,
  RESONANCE_EXPANSION: 0.12, TIER_UP_DURATION: 0.52, DISABLED_GLITCH_FREQUENCY: 12,
  DISABLED_CORE_ALPHA: 0.34,
  SHELL_SPRITE_SIZE: 60, CORE_SPRITE_SIZE: 54, CRYSTAL_SPRITE_SIZE: 46, SATELLITE_SPRITE_SIZE: 48,
});

type SpriteKey = 'shell' | 'core' | 'crystal' | 'satellite';
type GradientStops = ReadonlyArray<readonly [number, string]>;
type OrbitalStateRecord = {
  lastTime: number; lastAuraTimer: number; lastTier: number;
  attackTimer: number; tierUpTimer: number; resonanceTimer: number; impactTimer: number; phaseOffset: number;
};
type Point = { x: number; y: number };

const SPRITES: Record<SpriteKey, string> = {
  shell: '/art/orbital-shell.svg', core: '/art/orbital-core.svg',
  crystal: '/art/orbital-crystal.svg', satellite: '/art/orbital-satellite.svg',
};
const SPRITE_CACHE = new Map<SpriteKey, HTMLImageElement>();
const GRADIENT_CACHE = new WeakMap<CanvasRenderingContext2D, Map<string, CanvasGradient>>();
const VISUAL_STATES = new WeakMap<SphereEntity, OrbitalStateRecord>();

function sprite(key: SpriteKey): HTMLImageElement | null {
  const cached = SPRITE_CACHE.get(key);
  if (cached) return cached.complete && cached.naturalWidth > 0 ? cached : null;
  const image = new Image(); image.decoding = 'async'; image.src = SPRITES[key]; SPRITE_CACHE.set(key, image); return null;
}
function drawSprite(ctx: CanvasRenderingContext2D, key: SpriteKey, x: number, y: number, size: number, rotation = 0, alpha = 1): boolean {
  const image = sprite(key); if (!image) return false;
  ctx.save(); ctx.translate(x, y); ctx.rotate(rotation); ctx.globalAlpha = alpha;
  ctx.drawImage(image, -size * 0.5, -size * 0.5, size, size); ctx.restore(); return true;
}
function cache(ctx: CanvasRenderingContext2D): Map<string, CanvasGradient> {
  let value = GRADIENT_CACHE.get(ctx); if (!value) { value = new Map(); GRADIENT_CACHE.set(ctx, value); } return value;
}
function gradient(ctx: CanvasRenderingContext2D, key: string, radius: number, stops: GradientStops): CanvasGradient {
  const cached = cache(ctx).get(key); if (cached) return cached;
  const g = ctx.createRadialGradient(0, 0, 0, 0, 0, radius);
  for (const [offset, color] of stops) g.addColorStop(offset, color);
  cache(ctx).set(key, g); return g;
}
function clamp(v: number): number { return Math.max(0, Math.min(1, v)); }
function ease(t: number): number { const x = clamp(t); return 1 - Math.pow(1 - x, 3); }
function ease2(t: number): number { const x = clamp(t); return x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2; }
function rgba(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.replace('#', ''), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${clamp(alpha)})`;
}
function orbitPoint(cx: number, cy: number, rx: number, ry: number, angle: number): Point {
  const x = Math.cos(angle) * rx, y = Math.sin(angle) * ry;
  const c = Math.cos(ORBITAL_VISUAL.ORBIT_ROT), s = Math.sin(ORBITAL_VISUAL.ORBIT_ROT);
  return { x: cx + x * c - y * s, y: cy + x * s + y * c };
}
function orbitArc(ctx: CanvasRenderingContext2D, cx: number, cy: number, rx: number, ry: number, a0: number, a1: number, color: string, alpha: number, width: number): void {
  ctx.save(); ctx.translate(cx, cy); ctx.rotate(ORBITAL_VISUAL.ORBIT_ROT); ctx.globalAlpha = alpha;
  ctx.strokeStyle = color; ctx.lineWidth = Math.max(0.65, width); ctx.beginPath(); ctx.ellipse(0, 0, rx, ry, 0, a0, a1); ctx.stroke(); ctx.restore();
}
function glow(ctx: CanvasRenderingContext2D, x: number, y: number, radius: number, color: string, alpha: number): void {
  const r = Math.max(8, radius);
  const g = gradient(ctx, `glow:${color}:${Math.round(r)}`, r, [[0, rgba('#ffffff', .82)], [.18, rgba(color, .42)], [.56, rgba(color, .14)], [1, rgba(color, 0)]]);
  ctx.save(); ctx.translate(x, y); ctx.globalAlpha = alpha; ctx.fillStyle = g; ctx.beginPath(); ctx.arc(0, 0, r, 0, TAU); ctx.fill(); ctx.restore();
}
function core(ctx: CanvasRenderingContext2D, cx: number, cy: number, scale: number, pulse: number, resonance: boolean, disabled: boolean): void {
  const color = disabled ? ORBITAL_VISUAL.DISABLED_COLOR : resonance ? ORBITAL_VISUAL.RESONANCE_COLOR : ORBITAL_VISUAL.CORE_COLOR;
  const size = ORBITAL_VISUAL.CORE_SPRITE_SIZE * scale * (.95 + pulse * .10);
  ctx.save(); ctx.globalCompositeOperation = 'lighter';
  glow(ctx, cx, cy, size * .62, color, disabled ? .18 : resonance ? .54 : .44);
  ctx.globalCompositeOperation = 'source-over';
  drawSprite(ctx, 'core', cx, cy, size, 0, disabled ? ORBITAL_VISUAL.DISABLED_CORE_ALPHA : 1);
  ctx.globalCompositeOperation = 'lighter';
  if (resonance || disabled) {
    const g = gradient(ctx, `state:${color}:${Math.round(size)}`, size * .52, [[0, rgba('#ffffff', .48)], [.34, rgba(color, .38)], [.78, rgba(color, .16)], [1, rgba(color, 0)]]);
    ctx.translate(cx, cy); ctx.globalAlpha = disabled ? .44 : .64; ctx.fillStyle = g; ctx.beginPath(); ctx.arc(0, 0, size * .52, 0, TAU); ctx.fill();
  }
  ctx.restore();
}
function crystals(ctx: CanvasRenderingContext2D, cx: number, cy: number, scale: number, pulse: number): void {
  const size = ORBITAL_VISUAL.CRYSTAL_SPRITE_SIZE * scale * (.99 + pulse * .018), offset = 40 * scale;
  drawSprite(ctx, 'crystal', cx, cy - offset, size); drawSprite(ctx, 'crystal', cx, cy + offset, size, Math.PI);
}
function satellites(ctx: CanvasRenderingContext2D, cx: number, cy: number, count: number, scale: number, phase: number, attack: number): void {
  const n = Math.max(0, Math.min(7, Math.round(count))); if (!n) return;
  const size = ORBITAL_VISUAL.SATELLITE_SPRITE_SIZE * scale;
  ctx.save(); ctx.globalCompositeOperation = 'source-over';
  for (let i = 0; i < n; i++) {
    const base = phase + i * TAU / n, a = base + (i === 0 ? ease(attack) * .82 : 0);
    const p = orbitPoint(cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale, ORBITAL_VISUAL.ORBIT_RY * scale, a);
    drawSprite(ctx, 'satellite', p.x, p.y, size, a + ORBITAL_VISUAL.ORBIT_ROT + Math.PI / 2, Math.sin(a) > 0 ? 1 : .42);
  }
  ctx.restore();
}
function impact(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, alpha: number): void {
  if (alpha <= 0) return; ctx.save(); ctx.globalCompositeOperation = 'lighter'; glow(ctx, x, y, r * 1.9, ORBITAL_VISUAL.EDGE_COLOR, alpha * .7);
  ctx.globalAlpha = alpha; ctx.strokeStyle = ORBITAL_VISUAL.EDGE_HOT; ctx.lineWidth = 1.05;
  ctx.beginPath(); ctx.arc(x, y, r * (.55 + alpha * .65), -.7, 1.8); ctx.stroke();
  ctx.beginPath(); ctx.arc(x, y, r * (.45 + alpha * .45), 2.1, 4.2); ctx.stroke(); ctx.restore();
}
function glitch(ctx: CanvasRenderingContext2D, cx: number, cy: number, scale: number, phase: number, intensity: number): void {
  if (intensity <= EPS) return; const color = ORBITAL_VISUAL.DISABLED_COLOR;
  orbitArc(ctx, cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale, ORBITAL_VISUAL.ORBIT_RY * scale, phase * 2.6, phase * 2.6 + .24, color, intensity * .44, 1.2);
  ctx.save(); ctx.globalAlpha = intensity * .55; ctx.strokeStyle = color; ctx.lineWidth = 1;
  for (let i = 0; i < 3; i++) { const y = cy + (i - 1) * 4, x0 = cx - 18 + Math.sin(phase * 9 + i) * 5, x1 = cx + 14 + Math.cos(phase * 7 + i) * 5;
    ctx.beginPath(); ctx.moveTo(x0, y); ctx.lineTo(x0 + 7, y + (i - 1) * 2); ctx.lineTo(x1 - 6, y - (i - 1) * 1.5); ctx.lineTo(x1, y); ctx.stroke(); }
  ctx.restore();
}
function chargeRing(ctx: CanvasRenderingContext2D, cx: number, cy: number, scale: number, color: string, progress: number): void {
  const p = clamp(progress); if (!p) return; const a0 = -Math.PI / 2 + .32, a1 = a0 + TAU * (.30 + .70 * ease2(p));
  ctx.save(); ctx.globalCompositeOperation = 'lighter'; ctx.strokeStyle = rgba(color, .72); ctx.lineWidth = 1.25 + p * .8; ctx.beginPath();
  ctx.ellipse(cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * 1.08, ORBITAL_VISUAL.ORBIT_RY * scale * 1.08, ORBITAL_VISUAL.ORBIT_ROT, a0, a1); ctx.stroke(); ctx.restore();
}
function tierPulse(ctx: CanvasRenderingContext2D, cx: number, cy: number, scale: number, progress: number): void {
  const q = clamp(progress); if (!q) return; ctx.save(); ctx.globalCompositeOperation = 'lighter'; ctx.globalAlpha = (1 - ease(q)) * .68; ctx.strokeStyle = ORBITAL_VISUAL.EDGE_HOT; ctx.lineWidth = 1.1;
  ctx.beginPath(); ctx.ellipse(cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * (1 + q * .4), ORBITAL_VISUAL.ORBIT_RY * scale * (1 + q * .4), ORBITAL_VISUAL.ORBIT_ROT, 0, TAU); ctx.stroke(); ctx.restore();
}
function state(sphere: SphereEntity): OrbitalStateRecord {
  let r = VISUAL_STATES.get(sphere); if (r) return r;
  r = { lastTime: 0, lastAuraTimer: sphere.auraTimer, lastTier: Math.max(1, sphere.visualTier || 1), attackTimer: 0, tierUpTimer: 0, resonanceTimer: 0, impactTimer: 0,
    phaseOffset: ((((sphere.pos && sphere.pos.x) || 0) * .011) + (((sphere.pos && sphere.pos.y) || 0) * .007)) % TAU };
  VISUAL_STATES.set(sphere, r); return r;
}
function update(sphere: SphereEntity, player: PlayerState, time: number): OrbitalStateRecord {
  const r = state(sphere), dt = r.lastTime > 0 ? Math.max(0, Math.min(.05, time - r.lastTime)) : 0; r.lastTime = time;
  const aura = Number.isFinite(sphere.auraTimer) ? sphere.auraTimer : 0;
  if (aura > r.lastAuraTimer + .12) { r.attackTimer = ORBITAL_VISUAL.ATTACK_DURATION; r.impactTimer = ORBITAL_VISUAL.IMPACT_FLASH_DURATION; }
  r.lastAuraTimer = aura;
  const tier = Math.max(1, Math.min(7, sphere.visualTier || 1)); if (tier > r.lastTier) r.tierUpTimer = ORBITAL_VISUAL.TIER_UP_DURATION; r.lastTier = tier;
  const charge = clamp(Number(player?.resonanceCharge || 0) / 100); if (player?.resonanceEventActive || charge >= .88) if (r.resonanceTimer <= 0) r.resonanceTimer = ORBITAL_VISUAL.RESONANCE_DURATION;
  r.attackTimer = Math.max(0, r.attackTimer - dt); r.impactTimer = Math.max(0, r.impactTimer - dt); r.tierUpTimer = Math.max(0, r.tierUpTimer - dt); r.resonanceTimer = Math.max(0, r.resonanceTimer - dt); return r;
}

export function renderOrbitalSphereRuntimeVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, player: PlayerState, time: number, scale = 1, enemies: EnemyEntity[] = []): void {
  if (!ctx || !sphere) return;
  const r = update(sphere, player, time), disabled = sphere.networkDisabledTimer > 0 || !sphere.alive, resonance = r.resonanceTimer > 0;
  const tier = Math.max(1, Math.min(7, sphere.visualTier || 1)), pulse = .5 + .5 * Math.sin(time * ORBITAL_VISUAL.CORE_PULSE_SPEED + r.phaseOffset);
  const resonanceProgress = resonance ? .65 + .35 * (.5 + .5 * Math.sin(time * 6)) : 0;
  const orbitScale = resonance ? 1 + ORBITAL_VISUAL.RESONANCE_EXPANSION * ease2(resonanceProgress) : 1;
  const attackProgress = r.attackTimer > 0 ? 1 - r.attackTimer / ORBITAL_VISUAL.ATTACK_DURATION : 0;
  const cx = sphere.pos.x, cy = sphere.pos.y + Math.sin(time * ORBITAL_VISUAL.BOB_SPEED + r.phaseOffset) * ORBITAL_VISUAL.BOB_AMPLITUDE * scale;
  const phase = time * ORBITAL_VISUAL.IDLE_ORBIT_SPEED + sphere.rotation * .06 + r.phaseOffset;
  const color = resonance ? ORBITAL_VISUAL.RESONANCE_COLOR : ORBITAL_VISUAL.EDGE_COLOR;
  const alpha = disabled ? .16 : resonance ? .72 : .34 + tier * .015;

  ctx.save(); ctx.globalCompositeOperation = 'lighter';
  orbitArc(ctx, cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * orbitScale, ORBITAL_VISUAL.ORBIT_RY * scale * orbitScale, Math.PI + .08, TAU - .08, color, alpha * .52, 1.25);
  orbitArc(ctx, cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * orbitScale, ORBITAL_VISUAL.ORBIT_RY * scale * orbitScale, .08, Math.PI - .08, color, alpha, 1.65);
  if (!disabled) satellites(ctx, cx, cy, tier, scale * orbitScale, phase, attackProgress);

  if (r.attackTimer > 0 && !disabled) {
    const p = ease(attackProgress), striker = orbitPoint(cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * orbitScale, ORBITAL_VISUAL.ORBIT_RY * scale * orbitScale, phase + p * .82);
    let ix = striker.x, iy = striker.y, nearest = Number.POSITIVE_INFINITY;
    for (const enemy of enemies) { if (!enemy || enemy.hp <= 0 || !enemy.pos) continue; const d = Math.hypot(enemy.pos.x - striker.x, enemy.pos.y - striker.y); if (d < nearest && d <= 18 * scale) { nearest = d; ix = enemy.pos.x; iy = enemy.pos.y; } }
    for (let i = 3; i >= 1; i--) { const tp = clamp(p - i * .10), t = orbitPoint(cx, cy, ORBITAL_VISUAL.ORBIT_RX * scale * orbitScale, ORBITAL_VISUAL.ORBIT_RY * scale * orbitScale, phase + tp * .82); drawSprite(ctx, 'satellite', t.x, t.y, ORBITAL_VISUAL.SATELLITE_SPRITE_SIZE * scale * (.90 - i * .04), phase + tp * .82 + ORBITAL_VISUAL.ORBIT_ROT + Math.PI / 2, .16 / i); }
    impact(ctx, ix, iy, 4.2 * scale, r.impactTimer > 0 ? r.impactTimer / ORBITAL_VISUAL.IMPACT_FLASH_DURATION : (1 - p) * .35);
  }

  ctx.globalCompositeOperation = 'source-over';
  drawSprite(ctx, 'shell', cx, cy, ORBITAL_VISUAL.SHELL_SPRITE_SIZE * scale, 0, disabled ? .72 : 1);
  crystals(ctx, cx, cy, scale, pulse);
  ctx.globalCompositeOperation = 'lighter';
  if (resonance) chargeRing(ctx, cx, cy, scale, color, resonanceProgress);
  if (r.tierUpTimer > 0) tierPulse(ctx, cx, cy, scale, 1 - r.tierUpTimer / ORBITAL_VISUAL.TIER_UP_DURATION);
  core(ctx, cx, cy, scale, pulse, resonance, disabled);
  if (disabled) glitch(ctx, cx, cy, scale, time + r.phaseOffset, clamp(.68 + (sphere.networkDisabledTimer > 0 ? sphere.networkDisabledTimer * .55 : .28)));
  ctx.restore();
}

export default renderOrbitalSphereRuntimeVfx;