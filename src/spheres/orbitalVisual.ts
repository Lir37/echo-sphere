import type { EnemyEntity, PlayerState, SphereEntity } from '../engine';

const TAU = Math.PI * 2;
const EPS = 0.0001;

export const ORBITAL_VISUAL = Object.freeze({
  BASE_RADIUS_PX: 24,
  BODY_COLOR: '#06111d',
  EDGE_COLOR: '#8ef0ff',
  EDGE_HOT: '#e9fcff',
  CORE_HOT: '#ffffff',
  CORE_COLOR: '#8ef0ff',
  CORE_DEEP: '#174a5c',
  RESONANCE_COLOR: '#ffd166',
  DISABLED_COLOR: '#ff4d70',

  ORBIT_A_RX: 46,
  ORBIT_A_RY: 16,
  ORBIT_B_RX: 39,
  ORBIT_B_RY: 13,
  ORBIT_A_ROT: -0.12,
  ORBIT_B_ROT: 0.98,

  IDLE_ORBIT_SPEED: 0.42,
  CORE_PULSE_SPEED: 3.2,
  BOB_SPEED: 1.4,
  BOB_AMPLITUDE: 0.9,

  ATTACK_DURATION: 0.19,
  IMPACT_FLASH_DURATION: 0.10,
  RESONANCE_DURATION: 0.76,
  RESONANCE_EXPANSION: 0.18,
  TIER_UP_DURATION: 0.52,
  DISABLED_GLITCH_FREQUENCY: 12.0,
  DISABLED_CORE_ALPHA: 0.34,
});

const GRADIENT_CACHE = new WeakMap();
const VISUAL_STATES = new WeakMap();

function getContextGradientCache(ctx) {
  let cache = GRADIENT_CACHE.get(ctx);
  if (!cache) {
    cache = new Map();
    GRADIENT_CACHE.set(ctx, cache);
  }
  return cache;
}

function getCachedRadialGradient(ctx, key, radius, stops) {
  const cache = getContextGradientCache(ctx);
  const cached = cache.get(key);
  if (cached) return cached;

  // Gradients are local-space objects. Translation happens at draw time, so
  // one gradient remains reusable at every Sphere position.
  const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, radius);
  for (const stop of stops) gradient.addColorStop(stop[0], stop[1]);
  cache.set(key, gradient);
  return gradient;
}

function clamp01(v) {
  return Math.max(0, Math.min(1, v));
}

function easeOutCubic(t) {
  const x = clamp01(t);
  return 1 - Math.pow(1 - x, 3);
}

function easeInOutQuad(t) {
  const x = clamp01(t);
  return x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2;
}

function rgba(hex, alpha) {
  const value = hex.replace('#', '');
  const normalized = value.length === 3
    ? value.split('').map((c) => c + c).join('')
    : value;
  const n = Number.parseInt(normalized, 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${clamp01(alpha)})`;
}

function orbitalPoint(cx, cy, rx, ry, angle, rotation) {
  const px = Math.cos(angle) * rx;
  const py = Math.sin(angle) * ry;
  const c = Math.cos(rotation);
  const s = Math.sin(rotation);
  return {
    x: cx + px * c - py * s,
    y: cy + px * s + py * c,
  };
}

function drawOrbitArc(ctx, cx, cy, rx, ry, rotation, start, end, color, alpha, width) {
  ctx.save();
  ctx.translate(cx, cy);
  ctx.rotate(rotation);
  ctx.globalAlpha = alpha;
  ctx.strokeStyle = color;
  ctx.lineWidth = Math.max(0.65, width);
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, start, end);
  ctx.stroke();
  ctx.restore();
}

function drawCachedCoreGlow(ctx, x, y, radius, color, alpha) {
  const r = Math.max(8, radius);
  const key = `coreGlow:${color}:${Math.round(r)}`;
  const gradient = getCachedRadialGradient(ctx, key, r, [
    [0, rgba('#ffffff', 0.86)],
    [0.18, rgba(color, 0.42)],
    [0.56, rgba(color, 0.13)],
    [1, rgba(color, 0)],
  ]);

  ctx.save();
  ctx.translate(x, y);
  ctx.globalAlpha = alpha;
  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(0, 0, r, 0, TAU);
  ctx.fill();
  ctx.restore();
}

function drawTierSatellites(ctx, cx, cy, count, orbitScale, phase, alpha, color, attackPulse) {
  const safeCount = Math.max(0, Math.min(7, Math.round(count)));
  if (safeCount <= 0) return;

  for (let i = 0; i < safeCount; i += 1) {
    const baseAngle = phase + (i / safeCount) * TAU;
    const usePrimary = i % 2 === 0;
    const rx = (usePrimary ? ORBITAL_VISUAL.ORBIT_A_RX : ORBITAL_VISUAL.ORBIT_B_RX) * orbitScale;
    const ry = (usePrimary ? ORBITAL_VISUAL.ORBIT_A_RY : ORBITAL_VISUAL.ORBIT_B_RY) * orbitScale;
    const rotation = usePrimary ? ORBITAL_VISUAL.ORBIT_A_ROT : ORBITAL_VISUAL.ORBIT_B_ROT;
    const strike = i === 0 && attackPulse > 0 ? easeOutCubic(attackPulse) : 0;
    const angle = baseAngle + strike * 0.82;
    const p = orbitalPoint(cx, cy, rx, ry, angle, rotation);
    const front = Math.sin(angle) > 0;
    const tangent = angle + rotation + Math.PI / 2;
    const size = 2.35;

    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.rotate(tangent);
    ctx.globalAlpha = alpha * (front ? 0.95 : 0.40);
    ctx.fillStyle = ORBITAL_VISUAL.BODY_COLOR;
    ctx.strokeStyle = color;
    ctx.lineWidth = 0.85;
    ctx.beginPath();
    ctx.moveTo(-size * 1.35, 0);
    ctx.lineTo(-size * 0.48, -size * 0.62);
    ctx.lineTo(size * 0.88, -size * 0.82);
    ctx.lineTo(size * 1.32, 0);
    ctx.lineTo(size * 0.88, size * 0.82);
    ctx.lineTo(-size * 0.48, size * 0.62);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    ctx.strokeStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, 0.58);
    ctx.lineWidth = 0.55;
    ctx.beginPath();
    ctx.moveTo(-size * 0.42, -size * 0.48);
    ctx.lineTo(size * 0.70, 0);
    ctx.stroke();

    ctx.fillStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, 0.62);
    ctx.fillRect(-0.65, -0.40, 1.3, 0.8);
    ctx.restore();
  }
}

function drawImpactFlash(ctx, x, y, radius, alpha) {
  if (alpha <= 0) return;

  drawCachedCoreGlow(ctx, x, y, radius * 1.9, ORBITAL_VISUAL.EDGE_COLOR, alpha * 0.70);
  ctx.save();
  ctx.globalAlpha = alpha;
  ctx.strokeStyle = ORBITAL_VISUAL.EDGE_HOT;
  ctx.lineWidth = 1.05;
  ctx.beginPath();
  ctx.arc(x, y, radius * (0.55 + alpha * 0.65), -0.7, 1.8);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(x, y, radius * (0.45 + alpha * 0.45), 2.1, 4.2);
  ctx.stroke();
  ctx.restore();
}

function drawBreakGlitch(ctx, cx, cy, orbitScale, phase, intensity) {
  if (intensity <= EPS) return;
  const color = ORBITAL_VISUAL.DISABLED_COLOR;
  const wave = 0.5 + 0.5 * Math.sin(phase * ORBITAL_VISUAL.DISABLED_GLITCH_FREQUENCY);

  for (let i = 0; i < 4; i += 1) {
    const center = phase * 2.6 + i * 1.47;
    const span = 0.18 + 0.10 * wave;
    drawOrbitArc(
      ctx, cx, cy,
      ORBITAL_VISUAL.ORBIT_A_RX * orbitScale,
      ORBITAL_VISUAL.ORBIT_A_RY * orbitScale,
      ORBITAL_VISUAL.ORBIT_A_ROT,
      center,
      center + span,
      color,
      intensity * (0.34 + i * 0.04),
      1.2,
    );
  }

  ctx.save();
  ctx.globalAlpha = intensity * 0.55;
  ctx.strokeStyle = color;
  ctx.lineWidth = 1;
  for (let i = 0; i < 3; i += 1) {
    const y = cy + (i - 1) * 4;
    const x0 = cx - 18 + Math.sin(phase * 9 + i) * 5;
    const x1 = cx + 14 + Math.cos(phase * 7 + i) * 5;
    ctx.beginPath();
    ctx.moveTo(x0, y);
    ctx.lineTo(x0 + 7, y + (i - 1) * 2);
    ctx.lineTo(x1 - 6, y - (i - 1) * 1.5);
    ctx.lineTo(x1, y);
    ctx.stroke();
  }
  ctx.restore();
}

function drawChargeRing(ctx, cx, cy, radius, color, progress) {
  const p = clamp01(progress);
  if (p <= EPS) return;
  const arc = 0.30 + 0.70 * easeInOutQuad(p);
  const a0 = -Math.PI / 2 + 0.32;
  const a1 = a0 + TAU * arc;

  ctx.save();
  ctx.strokeStyle = rgba(color, 0.72);
  ctx.lineWidth = 1.25 + p * 0.8;
  ctx.beginPath();
  ctx.ellipse(cx, cy, radius * 1.16, radius * 0.52, -0.10, a0, a1);
  ctx.stroke();

  ctx.strokeStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, 0.62);
  ctx.lineWidth = 0.55;
  ctx.beginPath();
  ctx.ellipse(cx, cy, radius * 1.23, radius * 0.56, -0.10, a1 - 0.42, a1);
  ctx.stroke();
  ctx.restore();
}

function drawTierPulse(ctx, cx, cy, radius, progress) {
  const q = clamp01(progress);
  if (q <= EPS) return;
  const fade = 1 - easeOutCubic(q);

  ctx.save();
  ctx.globalAlpha = fade * 0.68;
  ctx.strokeStyle = ORBITAL_VISUAL.EDGE_HOT;
  ctx.lineWidth = 1.1;
  ctx.beginPath();
  ctx.ellipse(cx, cy, radius * (1.02 + q * 0.95), radius * (0.48 + q * 0.48), -0.08, 0, TAU);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(cx, cy, radius * (0.54 + q * 0.46), 0, TAU);
  ctx.stroke();
  ctx.restore();
}

function drawTierContour(ctx, cx, cy, radius, progress) {
  const q = clamp01(progress);
  if (q <= 0.32) return;
  const alpha = 0.24 + 0.32 * clamp01((q - 0.32) / 0.68);
  ctx.save();
  ctx.strokeStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, alpha);
  ctx.lineWidth = 1.0;
  ctx.beginPath();
  ctx.ellipse(cx, cy, radius * 1.42, radius * 0.82, -0.10, 0, TAU);
  ctx.stroke();
  ctx.restore();
}

function drawCoreOverlay(ctx, cx, cy, radius, pulse, state) {
  let color = ORBITAL_VISUAL.CORE_COLOR;
  let alpha = 0.92;
  if (state === 'resonance') {
    color = ORBITAL_VISUAL.RESONANCE_COLOR;
    alpha = 1;
  } else if (state === 'disabled') {
    color = ORBITAL_VISUAL.DISABLED_COLOR;
    alpha = ORBITAL_VISUAL.DISABLED_CORE_ALPHA;
  }

  drawCachedCoreGlow(ctx, cx, cy, radius * (1.55 + pulse * 0.12), color, alpha * 0.36);

  const key = `core:${color}:${Math.round(radius)}`;
  const gradient = getCachedRadialGradient(ctx, key, radius, [
    [0, ORBITAL_VISUAL.CORE_HOT],
    [0.20, rgba(color, 0.92)],
    [0.60, rgba(color, 0.62)],
    [0.86, rgba(ORBITAL_VISUAL.CORE_DEEP, 0.88)],
    [1, rgba(ORBITAL_VISUAL.BODY_COLOR, 0.95)],
  ]);

  ctx.save();
  ctx.translate(cx, cy);
  ctx.globalAlpha = alpha;
  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(0, 0, radius * (0.92 + pulse * 0.035), 0, TAU);
  ctx.fill();

  ctx.strokeStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, state === 'disabled' ? 0.34 : 0.80 + pulse * 0.08);
  ctx.lineWidth = Math.max(0.8, radius * 0.055);
  ctx.beginPath();
  ctx.arc(0, 0, radius * 0.90, 0, TAU);
  ctx.stroke();

  // Inferred from the reference: inner energy arcs reinforce the spherical 2.5D core.
  ctx.strokeStyle = rgba(ORBITAL_VISUAL.EDGE_HOT, alpha * 0.46);
  ctx.lineWidth = 0.75;
  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 0.80, radius * 0.32, -0.20, 0.12, 0.94);
  ctx.stroke();
  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 0.72, radius * 0.28, -0.20, 3.28, 4.12);
  ctx.stroke();
  ctx.restore();
}

function getStateRecord(sphere) {
  let record = VISUAL_STATES.get(sphere);
  if (!record) {
    record = {
      lastTime: 0,
      lastAuraTimer: sphere.auraTimer,
      lastTier: Math.max(1, sphere.visualTier || 1),
      attackTimer: 0,
      tierUpTimer: 0,
      resonanceTimer: 0,
      impactTimer: 0,
      phaseOffset: ((((sphere.pos && sphere.pos.x) || 0) * 0.011) + (((sphere.pos && sphere.pos.y) || 0) * 0.007)) % TAU,
    };
    VISUAL_STATES.set(sphere, record);
  }
  return record;
}

function updateVisualState(sphere, player, time) {
  const record = getStateRecord(sphere);
  const dt = record.lastTime > 0 ? Math.max(0, Math.min(0.05, time - record.lastTime)) : 0;
  record.lastTime = time;

  const aura = Number.isFinite(sphere.auraTimer) ? sphere.auraTimer : 0;
  // The gameplay runtime resets auraTimer after an orbital hit cycle. The rising
  // edge gives us an attack event without adding a new gameplay field.
  if (aura > record.lastAuraTimer + 0.12) {
    record.attackTimer = ORBITAL_VISUAL.ATTACK_DURATION;
    record.impactTimer = ORBITAL_VISUAL.IMPACT_FLASH_DURATION;
  }
  record.lastAuraTimer = aura;

  const tier = Math.max(1, Math.min(7, sphere.visualTier || 1));
  if (tier > record.lastTier) record.tierUpTimer = ORBITAL_VISUAL.TIER_UP_DURATION;
  record.lastTier = tier;

  const charge = clamp01(Number(player && player.resonanceCharge || 0) / 100);
  if (Boolean(player && player.resonanceEventActive) || charge >= 0.88) {
    if (record.resonanceTimer <= 0) record.resonanceTimer = ORBITAL_VISUAL.RESONANCE_DURATION;
  }

  record.attackTimer = Math.max(0, record.attackTimer - dt);
  record.impactTimer = Math.max(0, record.impactTimer - dt);
  record.tierUpTimer = Math.max(0, record.tierUpTimer - dt);
  record.resonanceTimer = Math.max(0, record.resonanceTimer - dt);
  return record;
}

export function renderOrbitalSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale = 1,
  enemies: EnemyEntity[] = [],
): void {
  if (!ctx || !sphere) return;

  const record = updateVisualState(sphere, player, time);
  const disabled = sphere.networkDisabledTimer > 0 || !sphere.alive;
  const resonance = record.resonanceTimer > 0;
  const tier = Math.max(1, Math.min(7, sphere.visualTier || 1));
  const pulse = 0.5 + 0.5 * Math.sin(time * ORBITAL_VISUAL.CORE_PULSE_SPEED + record.phaseOffset);

  const resonanceProgress = resonance
    ? 0.65 + 0.35 * (0.5 + 0.5 * Math.sin(time * 6.0))
    : 0;
  const tierProgress = record.tierUpTimer > 0
    ? 1 - record.tierUpTimer / ORBITAL_VISUAL.TIER_UP_DURATION
    : 0;
  const orbitScale = resonance
    ? 1 + ORBITAL_VISUAL.RESONANCE_EXPANSION * easeInOutQuad(resonanceProgress)
    : 1;
  const attackProgress = record.attackTimer > 0
    ? 1 - record.attackTimer / ORBITAL_VISUAL.ATTACK_DURATION
    : 0;

  const cx = sphere.pos.x;
  const cy = sphere.pos.y + Math.sin(time * ORBITAL_VISUAL.BOB_SPEED + record.phaseOffset)
    * ORBITAL_VISUAL.BOB_AMPLITUDE * scale;
  const idlePhase = time * ORBITAL_VISUAL.IDLE_ORBIT_SPEED + sphere.rotation * 0.06 + record.phaseOffset;

  ctx.save();
  ctx.globalCompositeOperation = 'lighter';

  const mainColor = resonance
    ? ORBITAL_VISUAL.RESONANCE_COLOR
    : ORBITAL_VISUAL.EDGE_COLOR;
  const orbitAlpha = disabled ? 0.16 : (resonance ? 0.72 : 0.34 + tier * 0.015);

  // Front/back halves keep the 2.5D depth readable at mobile scale.
  drawOrbitArc(
    ctx, cx, cy,
    ORBITAL_VISUAL.ORBIT_A_RX * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_A_RY * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_A_ROT,
    Math.PI + 0.10, TAU - 0.10,
    mainColor, orbitAlpha * 0.58, 1.0,
  );
  drawOrbitArc(
    ctx, cx, cy,
    ORBITAL_VISUAL.ORBIT_A_RX * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_A_RY * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_A_ROT,
    0.10, Math.PI - 0.10,
    mainColor, orbitAlpha, 1.45,
  );
  drawOrbitArc(
    ctx, cx, cy,
    ORBITAL_VISUAL.ORBIT_B_RX * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_B_RY * scale * orbitScale,
    ORBITAL_VISUAL.ORBIT_B_ROT,
    0.12, Math.PI - 0.12,
    mainColor, orbitAlpha * 0.58, 1.0,
  );

  if (!disabled) {
    // The idle sheet contains Tier-1's single authored striker.
    // Runtime adds exactly one satellite per tier step, up to seven total.
    drawTierSatellites(
      ctx, cx, cy,
      Math.max(0, tier - 1),
      scale * orbitScale,
      idlePhase,
      1,
      ORBITAL_VISUAL.EDGE_COLOR,
      attackProgress,
    );
  }

  if (record.attackTimer > 0 && !disabled) {
    const p = easeOutCubic(attackProgress);
    const striker = orbitalPoint(
      cx, cy,
      ORBITAL_VISUAL.ORBIT_A_RX * scale * orbitScale,
      ORBITAL_VISUAL.ORBIT_A_RY * scale * orbitScale,
      idlePhase + p * 0.82,
      ORBITAL_VISUAL.ORBIT_A_ROT,
    );

    let impactX = striker.x;
    let impactY = striker.y;
    let closestDistance = Number.POSITIVE_INFINITY;
    for (const enemy of enemies) {
      if (!enemy || enemy.hp <= 0 || !enemy.pos) continue;
      const dx = enemy.pos.x - striker.x;
      const dy = enemy.pos.y - striker.y;
      const distance = Math.hypot(dx, dy);
      if (distance < closestDistance && distance <= 18 * scale) {
        closestDistance = distance;
        impactX = enemy.pos.x;
        impactY = enemy.pos.y;
      }
    }

    drawOrbitArc(
      ctx, cx, cy,
      ORBITAL_VISUAL.ORBIT_A_RX * scale * orbitScale,
      ORBITAL_VISUAL.ORBIT_A_RY * scale * orbitScale,
      ORBITAL_VISUAL.ORBIT_A_ROT,
      idlePhase - 0.38 * p,
      idlePhase + 0.03 * p,
      ORBITAL_VISUAL.EDGE_HOT,
      0.60 * (1 - p * 0.35),
      1.6,
    );

    drawImpactFlash(
      ctx,
      impactX,
      impactY,
      4.2 * scale,
      record.impactTimer > 0
        ? record.impactTimer / ORBITAL_VISUAL.IMPACT_FLASH_DURATION
        : (1 - p) * 0.35,
    );
  }

  if (resonance) {
    drawChargeRing(
      ctx,
      cx,
      cy,
      ORBITAL_VISUAL.BASE_RADIUS_PX * scale,
      mainColor,
      resonanceProgress,
    );
  }

  if (record.tierUpTimer > 0) {
    drawTierPulse(
      ctx, cx, cy,
      ORBITAL_VISUAL.BASE_RADIUS_PX * scale,
      tierProgress,
    );
    drawTierContour(
      ctx, cx, cy,
      ORBITAL_VISUAL.BASE_RADIUS_PX * scale,
      tierProgress,
    );
  }

  if (disabled) {
    drawBreakGlitch(
      ctx, cx, cy,
      scale * orbitScale,
      time + record.phaseOffset,
      clamp01(0.68 + (sphere.networkDisabledTimer > 0 ? sphere.networkDisabledTimer * 0.55 : 0.28)),
    );
  }

  drawCoreOverlay(
    ctx, cx, cy,
    ORBITAL_VISUAL.BASE_RADIUS_PX * scale,
    pulse,
    disabled ? 'disabled' : (resonance ? 'resonance' : 'idle'),
  );

  ctx.restore();
}

export default renderOrbitalSphereRuntimeVfx;
