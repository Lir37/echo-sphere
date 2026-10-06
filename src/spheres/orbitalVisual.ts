import type { EnemyEntity, PlayerState, SphereEntity } from '../engine';
import { WHITE, core, drawSphereOrbit, finishDisabled, stateColor, glow } from './visualHelpers';
import { getSphereElementForBranch, SPHERE_ELEMENT_META } from '../sphereProgression';
import { getOrbitalRingCounts, orbitalElementAngle } from './orbitalGeometry';

const TAU = Math.PI * 2;
const BASE = '#8ef0ff';
const RESONANCE = '#ffd166';
const DISABLED = '#ff4d70';

type VisualState = { lastTime: number; lastAura: number; attack: number; resonance: number; phase: number };
const STATES = new WeakMap<SphereEntity, VisualState>();

function stateOf(s: SphereEntity): VisualState {
  let v = STATES.get(s);
  if (v) return v;
  v = {
    lastTime: 0,
    lastAura: Number.isFinite(s.auraTimer) ? s.auraTimer : 0,
    attack: 0,
    resonance: 0,
    phase: ((s.pos.x * .011 + s.pos.y * .007) % TAU + TAU) % TAU,
  };
  STATES.set(s, v);
  return v;
}

function update(s: SphereEntity, p: PlayerState, t: number): VisualState {
  const v = stateOf(s);
  if (v.lastTime === t && v.lastTime !== 0) return v;
  const dt = v.lastTime > 0 ? Math.min(.05, Math.max(0, t - v.lastTime)) : 0;
  v.lastTime = t;
  const disabled = s.networkDisabledTimer > 0 || !s.alive;
  if (!disabled) {
    const aura = Number.isFinite(s.auraTimer) ? s.auraTimer : 0;
    if (aura > v.lastAura + .12) v.attack = .28;
    v.lastAura = aura;
    v.attack = Math.max(0, v.attack - dt);
    if (p?.resonanceEventActive) v.resonance = 1.15;
    v.resonance = Math.max(0, v.resonance - dt);
  }
  return v;
}

function drawOrbitalTerminal(
  ctx: CanvasRenderingContext2D,
  r: number,
  y: number,
  color: string,
  alpha: number,
  phase: number,
): void {
  ctx.save();
  ctx.translate(0, y);
  ctx.rotate(phase * .08);
  ctx.globalAlpha = alpha;
  ctx.fillStyle = '#07111d';
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.0;
  ctx.beginPath();
  ctx.moveTo(0, -r * .28);
  ctx.lineTo(r * .16, -r * .05);
  ctx.lineTo(r * .11, r * .24);
  ctx.lineTo(0, r * .31);
  ctx.lineTo(-r * .11, r * .24);
  ctx.lineTo(-r * .16, -r * .05);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.globalAlpha = alpha * .72;
  ctx.fillStyle = '#ffffff';
  ctx.beginPath();
  ctx.moveTo(0, -r * .16);
  ctx.lineTo(r * .07, 0);
  ctx.lineTo(0, r * .14);
  ctx.lineTo(-r * .07, 0);
  ctx.closePath();
  ctx.fill();
  ctx.restore();
}

function rgbaColor(hex: string, alpha: number): string {
  const n = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${alpha})`;
}

function drawSatellite(
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  size: number,
  color: string,
  alpha: number,
  angle: number,
  blade: boolean,
): void {
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(blade ? angle : angle + Math.PI / 4);
  ctx.globalAlpha = alpha;

  if (blade) {
    ctx.fillStyle = '#07111d';
    ctx.strokeStyle = color;
    ctx.lineWidth = 1.05;
    ctx.beginPath();
    ctx.moveTo(-size * .56, -size * .14);
    ctx.lineTo(size * .78, 0);
    ctx.lineTo(-size * .34, size * .15);
    ctx.lineTo(-size * .02, 0);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    ctx.globalAlpha = alpha * .72;
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = .72;
    ctx.beginPath();
    ctx.moveTo(-size * .30, 0);
    ctx.lineTo(size * .54, 0);
    ctx.stroke();

    ctx.globalAlpha = alpha * .50;
    ctx.beginPath();
    ctx.moveTo(-size * .42, -size * .10);
    ctx.lineTo(-size * .62, 0);
    ctx.lineTo(-size * .42, size * .10);
    ctx.stroke();
  } else {
    // Keep the mobile hot path free of per-satellite radial-gradient creation.
    // The authored diamond and a compact additive disc retain the luminous read.
    ctx.globalCompositeOperation = 'lighter';
    ctx.fillStyle = rgbaColor(color, alpha * .30);
    ctx.beginPath();
    ctx.arc(0, 0, size * .62, 0, TAU);
    ctx.fill();

    ctx.globalCompositeOperation = 'source-over';
    ctx.globalAlpha = alpha * .92;
    ctx.fillStyle = '#07111d';
    ctx.strokeStyle = color;
    ctx.lineWidth = 1.0;
    ctx.beginPath();
    for (let i = 0; i < 8; i++) {
      const a = -Math.PI / 8 + i * TAU / 8;
      const rr = i % 2 === 0 ? size * .56 : size * .44;
      const px = Math.cos(a) * rr;
      const py = Math.sin(a) * rr;
      if (i === 0) ctx.moveTo(px, py);
      else ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    ctx.globalAlpha = alpha * .90;
    ctx.fillStyle = WHITE;
    ctx.beginPath();
    ctx.arc(-size * .10, -size * .12, size * .16, 0, TAU);
    ctx.fill();
  }

  ctx.restore();
}

export function renderOrbitalSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
  _enemies: EnemyEntity[] = [],
): void {
  const v = update(sphere, player, time);
  const disabled = sphere.networkDisabledTimer > 0 || !sphere.alive;
  const resonance = !disabled && v.resonance > 0;
  const color = disabled ? DISABLED : resonance ? RESONANCE : BASE;
  const r = 24 * scale;
  const coreR = r * .70;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.55, color, disabled ? .05 : .13);
  drawSphereOrbit(ctx, r * 1.05, color, time * .18 + v.phase, disabled ? .12 : .66, 'orbital', .5 + .5 * Math.sin(time * 3.0 + v.phase));
  drawSphereOrbit(ctx, r * .88, color, -time * .22 + v.phase * .5, disabled ? .08 : .38, 'orbital', .5);
  ctx.globalCompositeOperation = 'source-over';
  core(ctx, coreR, color, disabled ? .5 : .5 + .5 * Math.sin(time * 3.0 + v.phase));

  drawOrbitalTerminal(ctx, r, -r * 1.02, color, disabled ? .20 : .76, time);
  drawOrbitalTerminal(ctx, r, r * 1.02, color, disabled ? .16 : .62, -time);

  if (resonance) {
    ctx.globalAlpha = .42;
    ctx.strokeStyle = RESONANCE;
    ctx.lineWidth = 1.0;
    ctx.beginPath();
    ctx.arc(0, 0, r * 1.34, time * .65, time * .65 + Math.PI * 1.15);
    ctx.stroke();
  }
  if (disabled) {
    ctx.globalAlpha = .50;
    ctx.strokeStyle = DISABLED;
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.moveTo(-r * .72, -r * .72);
    ctx.lineTo(r * .72, r * .72);
    ctx.moveTo(r * .72, -r * .72);
    ctx.lineTo(-r * .72, r * .72);
    ctx.stroke();
  }
  finishDisabled(ctx, r, color, disabled);
  ctx.restore();
}

export function renderOrbitalSphereAttackersVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
  enemies: EnemyEntity[] = [],
): void {
  if (!sphere.alive || sphere.networkDisabledTimer > 0) return;
  const v = update(sphere, player, time);
  const r = 24 * scale;
  const level = Math.max(1, Math.min(7, sphere.visualTier || 1));
  const extraElements =
    (player?.artifacts?.includes('orbital_crown') ? 1 : 0)
    + ((player?.evolutions || []).some((id: string) => id.startsWith('sphere:orbital:7:') && id.endsWith(':2')) ? 1 : 0);
  const counts = getOrbitalRingCounts(level, extraElements);
  const resonance = v.resonance > 0;
  const color = resonance ? RESONANCE : BASE;
  const branch = player?.sphereBranches?.orbital;
  const bladeMutation = branch === 'orbital_blade';
  const innerOrbitRadius = r * 1.68;
  const outerOrbitRadius = r * 2.02;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';

  const drawRing = (ring: 'inner' | 'outer', count: number, radius: number): void => {
    for (let i = 0; i < count; i += 1) {
      const a = orbitalElementAngle(sphere.rotation, ring, i, count);
      const localX = Math.cos(a) * radius;
      const localY = Math.sin(a) * radius;
      const depth = .74 + .26 * ((Math.sin(a) + 1) * .5);
      drawSatellite(ctx, localX, localY, r * .20, color, depth, a, bladeMutation);

      const element = getSphereElementForBranch(branch);
      const elementColor = element ? SPHERE_ELEMENT_META[element].color : null;
      if (elementColor) {
        ctx.save();
        ctx.translate(localX, localY);
        ctx.rotate(a + Math.PI / 2);
        ctx.globalAlpha = depth * .72;
        ctx.strokeStyle = elementColor;
        ctx.lineWidth = 1.0;
        if (element === 'fire') {
          ctx.beginPath();
          ctx.moveTo(-r * .10, 0);
          ctx.quadraticCurveTo(0, -r * .16, r * .04, 0);
          ctx.quadraticCurveTo(0, r * .12, -r * .08, 0);
          ctx.stroke();
        } else if (element === 'freeze') {
          ctx.beginPath();
          ctx.moveTo(-r * .09, 0);
          ctx.lineTo(r * .09, 0);
          ctx.moveTo(0, -r * .09);
          ctx.lineTo(0, r * .09);
          ctx.stroke();
        } else {
          ctx.beginPath();
          ctx.arc(0, 0, r * .09, 0, TAU);
          ctx.stroke();
          ctx.beginPath();
          ctx.arc(r * .10, -r * .04, r * .035, 0, TAU);
          ctx.stroke();
        }
        ctx.restore();
      }

      if (ring === 'inner' && i === 0 && v.attack > 0) {
        const q = 1 - v.attack / .28;
        ctx.save();
        ctx.translate(local.x, local.y);
        ctx.rotate(a + Math.PI / 2);
        ctx.globalAlpha = (1 - q) * .32;
        ctx.strokeStyle = color;
        ctx.lineWidth = 1.0;
        ctx.beginPath();
        ctx.moveTo(-r * .32, 0);
        ctx.lineTo(-r * .04, 0);
        ctx.stroke();
        ctx.restore();
      }
    }
  };

  drawRing('inner', counts.inner, innerOrbitRadius);
  drawRing('outer', counts.outer, outerOrbitRadius);

  if (v.attack > 0) {
    const q = 1 - v.attack / .28;
    const strikePhase = sphere.rotation + (1 - Math.pow(1 - q, 3)) * .95;
    const sx = Math.cos(strikePhase) * innerOrbitRadius;
    const sy = Math.sin(strikePhase) * innerOrbitRadius;
    let ix = sx;
    let iy = sy;
    let near = Infinity;
    for (const e of enemies) {
      if (!e || e.hp <= 0) continue;
      const ex = e.pos.x - sphere.pos.x - sx;
      const ey = e.pos.y - sphere.pos.y - sy;
      const d = Math.hypot(ex, ey);
      if (d < near && d <= r * 1.30) {
        near = d;
        ix = sx + ex;
        iy = sy + ey;
      }
    }
    ctx.globalAlpha = .65 * (1 - q);
    ctx.fillStyle = '#ffffff';
    ctx.beginPath();
    ctx.arc(ix, iy, r * .07, 0, TAU);
    ctx.fill();
  }
  ctx.restore();
}

export default renderOrbitalSphereRuntimeVfx;
