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

function drawOrbitalElementTrail(
  ctx: CanvasRenderingContext2D,
  element: NonNullable<ReturnType<typeof getSphereElementForBranch>>,
  size: number,
  angle: number,
  alpha: number,
  blade: boolean,
): void {
  const color = SPHERE_ELEMENT_META[element].color;
  const trailScale = blade ? 1.0 : 0.86;

  ctx.save();
  ctx.rotate(angle);
  ctx.globalCompositeOperation = 'lighter';
  ctx.strokeStyle = color;
  ctx.fillStyle = rgbaColor(color, alpha * 0.26);
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';

  // The trail is deliberately geometric and chunky enough to survive mobile
  // gameplay scale. Shape, not a faint blur, carries the elemental identity.
  if (element === 'fire') {
    ctx.globalAlpha = alpha * 0.82;
    ctx.lineWidth = Math.max(1.1, size * 0.12);
    ctx.beginPath();
    ctx.moveTo(-size * 0.34 * trailScale, 0);
    ctx.quadraticCurveTo(-size * 0.86 * trailScale, -size * 0.18, -size * 1.28 * trailScale, 0);
    ctx.quadraticCurveTo(-size * 0.86 * trailScale, size * 0.18, -size * 0.34 * trailScale, 0);
    ctx.stroke();
    ctx.globalAlpha = alpha * 0.68;
    ctx.lineWidth = Math.max(0.9, size * 0.08);
    ctx.beginPath();
    ctx.moveTo(-size * 0.50, -size * 0.08);
    ctx.lineTo(-size * 0.96, 0);
    ctx.lineTo(-size * 0.54, size * 0.10);
    ctx.stroke();
  } else if (element === 'freeze') {
    ctx.globalAlpha = alpha * 0.86;
    ctx.lineWidth = Math.max(1.0, size * 0.10);
    for (const side of [-1, 1]) {
      ctx.beginPath();
      ctx.moveTo(-size * 0.26 * trailScale, side * size * 0.05);
      ctx.lineTo(-size * 0.88 * trailScale, side * size * 0.18);
      ctx.lineTo(-size * 1.28 * trailScale, side * size * 0.03);
      ctx.stroke();
    }
    ctx.globalAlpha = alpha * 0.78;
    ctx.beginPath();
    ctx.moveTo(-size * 0.78, -size * 0.22);
    ctx.lineTo(-size * 1.02, 0);
    ctx.lineTo(-size * 0.78, size * 0.22);
    ctx.stroke();
  } else {
    ctx.globalAlpha = alpha * 0.84;
    ctx.lineWidth = Math.max(1.0, size * 0.095);
    ctx.beginPath();
    ctx.moveTo(-size * 0.30 * trailScale, 0);
    ctx.quadraticCurveTo(-size * 0.74, -size * 0.20, -size * 1.15, -size * 0.04);
    ctx.quadraticCurveTo(-size * 0.86, size * 0.20, -size * 0.30 * trailScale, 0);
    ctx.stroke();
    ctx.globalAlpha = alpha * 0.64;
    ctx.fillStyle = rgbaColor(color, alpha * 0.42);
    for (let i = 0; i < 3; i++) {
      const px = -size * (0.58 + i * 0.25);
      const py = (i - 1) * size * 0.14;
      ctx.beginPath();
      ctx.arc(px, py, Math.max(0.9, size * (0.08 - i * 0.015)), 0, TAU);
      ctx.fill();
    }
  }
  ctx.restore();
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
  orbitalRadiusScale = 1,
): void {
  const v = update(sphere, player, time);
  const disabled = sphere.networkDisabledTimer > 0 || !sphere.alive;
  const resonance = !disabled && v.resonance > 0;
  const color = disabled ? DISABLED : resonance ? RESONANCE : BASE;
  const r = 24 * scale;
  const orbitMultiplier = Number.isFinite(orbitalRadiusScale) && orbitalRadiusScale > 0
    ? orbitalRadiusScale
    : 1;
  const coreR = r * .70;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  glow(ctx, r * 2.55, color, disabled ? .05 : .13);
  drawSphereOrbit(ctx, r * 1.05 * orbitMultiplier, color, time * .18 + v.phase, disabled ? .12 : .66, 'orbital', .5 + .5 * Math.sin(time * 3.0 + v.phase));
  drawSphereOrbit(ctx, r * .88 * orbitMultiplier, color, -time * .22 + v.phase * .5, disabled ? .08 : .38, 'orbital', .5);
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
  orbitalRadiusScale = 1,
): void {
  if (!sphere.alive || sphere.networkDisabledTimer > 0) return;
  const v = update(sphere, player, time);
  const r = 24 * scale;
  const level = Math.max(1, Math.min(7, sphere.visualTier || 1));
  const resonance = v.resonance > 0;
  const branchElement = getSphereElementForBranch(branch);
  const color = resonance ? RESONANCE : BASE;
  const satelliteBaseColor = branchElement ? SPHERE_ELEMENT_META[branchElement].color : color;
  const branch = player?.sphereBranches?.orbital;
  const extraElements =
    (player?.artifacts?.includes('orbital_crown') ? 1 : 0)
    + (branch === 'orbital_dance' && (player?.evolutions || []).some((id: string) => id === 'sphere:orbital:7:orbital_dance:2') ? 1 : 0);
  const counts = getOrbitalRingCounts(level, extraElements);
  const bladeMutation = branch === 'orbital_blade';
  const orbitMultiplier = Number.isFinite(orbitalRadiusScale) && orbitalRadiusScale > 0 ? orbitalRadiusScale : 1;
  const innerOrbitRadius = r * 1.68 * orbitMultiplier;
  const outerOrbitRadius = r * 2.02 * orbitMultiplier;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';

  const drawRing = (ring: 'inner' | 'outer', count: number, radius: number): void => {
    for (let i = 0; i < count; i += 1) {
      const a = orbitalElementAngle(sphere.rotation, ring, i, count);
      const localX = Math.cos(a) * radius;
      const localY = Math.sin(a) * radius;
      const depth = .74 + .26 * ((Math.sin(a) + 1) * .5);
      drawSatellite(ctx, localX, localY, r * .20, satelliteBaseColor, depth, a, bladeMutation);

      const element = getSphereElementForBranch(branch);
      const elementColor = element ? SPHERE_ELEMENT_META[element].color : null;
      if (element) {
        const combatElementColor = elementColor || color;
        // Mutation I+ changes the combat elements themselves, not only the
        // outer Sphere. Make the elemental read obvious on every satellite.
        ctx.save();
        ctx.translate(localX, localY);
        ctx.globalCompositeOperation = 'lighter';
        ctx.globalAlpha = depth * .92;
        ctx.strokeStyle = combatElementColor;
        ctx.fillStyle = rgbaColor(combatElementColor, depth * .22);
        ctx.lineWidth = Math.max(1.25, r * .075);
        ctx.beginPath();
        ctx.arc(0, 0, r * .18, 0, TAU);
        ctx.stroke();
        ctx.beginPath();
        ctx.arc(0, 0, r * .095, 0, TAU);
        ctx.fill();
        ctx.restore();

        drawOrbitalElementTrail(ctx, element, r * .20, a, depth, bladeMutation);
      }

      if (ring === 'inner' && i === 0 && v.attack > 0) {
        const q = 1 - v.attack / .28;
        ctx.save();
        ctx.translate(localX, localY);
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
