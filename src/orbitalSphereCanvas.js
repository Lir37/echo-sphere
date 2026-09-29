/**
 * ECHO SPHERE - Orbital Sphere Canvas renderer
 * 2.5D / Canvas API / transparent background.
 *
 * The renderer is intentionally self-contained: no DOM, no assets, no WebGL.
 * It can be plugged into the existing renderer without changing gameplay logic.
 */

const TAU = Math.PI * 2;

export const ORBITAL_SPHERE_DEFAULTS = {
  size: 180,
  coreRadius: 34,
  orbitX: 68,
  orbitY: 24,
  crystalSize: 18,
  satelliteSize: 10,
  colors: {
    core: '#8ed8ff',
    coreHot: '#ffffff',
    orbit: '#39a9ff',
    orbitAlt: '#7b5cff',
    crystal: '#73c8ff',
    crystalEdge: '#dff6ff',
    satellite: '#9ad8ff',
    satelliteEdge: '#ffffff',
    hit: '#ffffff',
    death: '#6fb8ff',
  },
  speeds: {
    orbitA: 0.65,
    orbitB: -0.42,
    bob: 0.8,
  },
};

const clamp01 = (v) => Math.max(0, Math.min(1, v));
const lerp = (a, b, t) => a + (b - a) * t;

function rgba(hex, alpha) {
  const value = hex.replace('#', '');
  const n = parseInt(value.length === 3
    ? value.split('').map((c) => c + c).join('')
    : value, 16);

  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${clamp01(alpha)})`;
}

function diamondPath(ctx, x, y, w, h, rotation = 0) {
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(rotation);
  ctx.beginPath();
  ctx.moveTo(0, -h);
  ctx.lineTo(w, 0);
  ctx.lineTo(0, h);
  ctx.lineTo(-w, 0);
  ctx.closePath();
  ctx.restore();
}

function drawGlowDot(ctx, x, y, radius, color, alpha = 1) {
  const g = ctx.createRadialGradient(x, y, 0, x, y, radius);
  g.addColorStop(0, rgba('#ffffff', alpha * 0.95));
  g.addColorStop(0.18, rgba(color, alpha * 0.8));
  g.addColorStop(0.55, rgba(color, alpha * 0.25));
  g.addColorStop(1, rgba(color, 0));

  ctx.fillStyle = g;
  ctx.beginPath();
  ctx.arc(x, y, radius, 0, TAU);
  ctx.fill();
}

/**
 * Draw the emissive core.
 * The hard silhouette and the soft halo are separate so the glow never
 * destroys the readable spherical form.
 */
export function drawOrbitalCore(ctx, x, y, radius, colors) {
  drawGlowDot(ctx, x, y, radius * 1.85, colors.core, 0.9);

  const g = ctx.createRadialGradient(
    x - radius * 0.3,
    y - radius * 0.35,
    radius * 0.08,
    x,
    y,
    radius,
  );
  g.addColorStop(0, colors.coreHot);
  g.addColorStop(0.22, colors.core);
  g.addColorStop(0.58, '#2677ff');
  g.addColorStop(0.86, '#1036a4');
  g.addColorStop(1, '#071337');

  ctx.fillStyle = g;
  ctx.beginPath();
  ctx.arc(x, y, radius, 0, TAU);
  ctx.fill();

  ctx.strokeStyle = rgba(colors.crystalEdge, 0.8);
  ctx.lineWidth = Math.max(1, radius * 0.045);
  ctx.beginPath();
  ctx.arc(x, y, radius * 0.92, 0, TAU);
  ctx.stroke();

  // Small inner energy arcs create the authored 2.5D feel.
  ctx.strokeStyle = rgba(colors.orbit, 0.45);
  ctx.lineWidth = Math.max(1, radius * 0.025);
  ctx.beginPath();
  ctx.arc(x, y, radius * 0.68, -1.15, 0.55);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(x, y, radius * 0.72, 1.85, 3.8);
  ctx.stroke();

  drawGlowDot(ctx, x - radius * 0.28, y - radius * 0.32, radius * 0.25, '#ffffff', 0.65);
}

/**
 * Elliptical orbital path. perspective is faked by squash + alpha:
 * back half is weaker, front half is stronger.
 */
export function drawOrbitRing(ctx, x, y, rx, ry, rotation, color, alpha = 1, width = 2) {
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(rotation);

  ctx.shadowColor = color;
  ctx.shadowBlur = 8;
  ctx.strokeStyle = rgba(color, alpha * 0.25);
  ctx.lineWidth = width * 3;
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, 0, Math.PI);
  ctx.stroke();

  ctx.shadowBlur = 0;
  ctx.strokeStyle = rgba(color, alpha);
  ctx.lineWidth = width;
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, Math.PI, TAU);
  ctx.stroke();

  ctx.strokeStyle = rgba(color, alpha * 0.34);
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, 0, Math.PI);
  ctx.stroke();

  ctx.restore();
}

/**
 * Four axial crystals: top, right, bottom, left.
 * They are separate objects, not a single sprite.
 */
export function drawCrystals(ctx, x, y, orbitX, orbitY, size, colors, time = 0) {
  const points = [
    { x, y: y - orbitY - size * 0.8, rot: 0 },
    { x: x + orbitX + size * 0.8, y, rot: Math.PI / 2 },
    { x, y: y + orbitY + size * 0.8, rot: Math.PI },
    { x: x - orbitX - size * 0.8, y, rot: -Math.PI / 2 },
  ];

  points.forEach((p, i) => {
    const pulse = 1 + Math.sin(time * 2.2 + i * 1.7) * 0.045;
    const w = size * 0.55 * pulse;
    const h = size * 1.25 * pulse;

    drawGlowDot(ctx, p.x, p.y, size * 1.15, colors.crystal, 0.28);

    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.rotate(p.rot);
    ctx.beginPath();
    ctx.moveTo(0, -h);
    ctx.lineTo(w, 0);
    ctx.lineTo(0, h);
    ctx.lineTo(-w, 0);
    ctx.closePath();

    const g = ctx.createLinearGradient(-w, -h, w, h);
    g.addColorStop(0, '#ffffff');
    g.addColorStop(0.25, colors.crystal);
    g.addColorStop(0.65, '#2d65db');
    g.addColorStop(1, '#101d6d');

    ctx.fillStyle = g;
    ctx.fill();
    ctx.strokeStyle = colors.crystalEdge;
    ctx.lineWidth = Math.max(1, size * 0.08);
    ctx.stroke();

    // Facet line.
    ctx.strokeStyle = rgba('#ffffff', 0.38);
    ctx.lineWidth = Math.max(1, size * 0.035);
    ctx.beginPath();
    ctx.moveTo(0, -h * 0.78);
    ctx.lineTo(0, h * 0.72);
    ctx.stroke();
    ctx.restore();
  });
}

function drawSatellite(ctx, x, y, angle, size, colors, alpha = 1, behavior = 'idle') {
  const scale = behavior === 'blade' ? 1.18 : behavior === 'eagle' ? 1.08 : 1;
  const w = size * 1.5 * scale;
  const h = size * 0.58 * scale;

  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle + (behavior === 'blade' ? Math.PI / 4 : 0));
  ctx.globalAlpha = alpha;

  if (behavior === 'blade') {
    ctx.shadowColor = '#ff4d7d';
    ctx.shadowBlur = 12;
  } else {
    ctx.shadowColor = colors.satellite;
    ctx.shadowBlur = 8;
  }

  ctx.fillStyle = behavior === 'blade' ? '#ff527f' : colors.satellite;
  ctx.strokeStyle = colors.satelliteEdge;
  ctx.lineWidth = Math.max(1, size * 0.12);

  ctx.beginPath();
  ctx.moveTo(-w, 0);
  ctx.lineTo(-w * 0.25, -h);
  ctx.lineTo(w, 0);
  ctx.lineTo(-w * 0.25, h);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();

  ctx.shadowBlur = 0;
  ctx.fillStyle = behavior === 'eagle' ? '#ffffff' : rgba('#ffffff', 0.8);
  ctx.fillRect(-size * 0.18, -size * 0.13, size * 0.36, size * 0.26);
  ctx.restore();
}

function orbitPoint(cx, cy, rx, ry, angle, rotation = 0) {
  const px = Math.cos(angle) * rx;
  const py = Math.sin(angle) * ry;

  return {
    x: cx + px * Math.cos(rotation) - py * Math.sin(rotation),
    y: cy + px * Math.sin(rotation) + py * Math.cos(rotation),
  };
}

/**
 * Satellites share the orbital math but are rendered in the correct depth
 * order. The front half gets full alpha, the rear half is subdued.
 */
export function drawSatellites(ctx, x, y, rx, ry, rotation, count, time, colors, mutation) {
  for (let i = 0; i < count; i += 1) {
    const angle = time * 0.95 + (TAU * i) / count;
    const p = orbitPoint(x, y, rx, ry, angle, rotation);
    const front = Math.sin(angle) > 0;
    const alpha = front ? 1 : 0.38;

    drawSatellite(
      ctx,
      p.x,
      p.y,
      angle + rotation,
      9,
      colors,
      alpha,
      mutation,
    );
  }
}

/**
 * Particle state used only by Death. No external particle system required.
 */
function createDeathParticles(x, y, count, color) {
  return Array.from({ length: count }, (_, i) => {
    const a = (TAU * i) / count + Math.random() * 0.35;
    const speed = 45 + Math.random() * 100;
    return {
      x,
      y,
      vx: Math.cos(a) * speed,
      vy: Math.sin(a) * speed,
      life: 0.65 + Math.random() * 0.45,
      size: 2 + Math.random() * 4,
      color,
    };
  });
}

export function createOrbitalSphere(config = {}) {
  const cfg = {
    ...ORBITAL_SPHERE_DEFAULTS,
    ...config,
    colors: { ...ORBITAL_SPHERE_DEFAULTS.colors, ...(config.colors || {}) },
    speeds: { ...ORBITAL_SPHERE_DEFAULTS.speeds, ...(config.speeds || {}) },
  };

  const state = {
    time: 0,
    hitTimer: 0,
    deathTimer: 0,
    deathDuration: 0.95,
    particles: [],
    mutation: 'base',
    orbitPhaseA: 0,
    orbitPhaseB: Math.PI * 0.5,
  };

  function setMutation(mutation) {
    state.mutation = mutation;
  }

  function hit(power = 1) {
    state.hitTimer = Math.max(state.hitTimer, 0.18 * Math.max(0.5, power));
  }

  function death() {
    if (state.deathTimer > 0) return;
    state.deathTimer = state.deathDuration;
    state.particles = createDeathParticles(0, 0, 28, cfg.colors.death);
  }

  function update(dt) {
    const step = Math.max(0, Math.min(dt, 0.05));
    state.time += step;
    state.orbitPhaseA += step * cfg.speeds.orbitA;
    state.orbitPhaseB += step * cfg.speeds.orbitB;
    state.hitTimer = Math.max(0, state.hitTimer - step);

    if (state.deathTimer > 0) {
      state.deathTimer = Math.max(0, state.deathTimer - step);
      state.particles.forEach((p) => {
        p.x += p.vx * step;
        p.y += p.vy * step;
        p.vx *= Math.pow(0.05, step);
        p.vy *= Math.pow(0.05, step);
        p.life -= step;
      });
    }
  }

  function draw(ctx, x, y, scale = 1) {
    const bob = Math.sin(state.time * cfg.speeds.bob) * 3 * scale;
    const hit = clamp01(state.hitTimer / 0.18);
    const dead = state.deathTimer > 0;
    const deathProgress = 1 - clamp01(state.deathTimer / state.deathDuration);

    const shakeX = hit > 0 ? (Math.random() - 0.5) * 7 * hit : 0;
    const shakeY = hit > 0 ? (Math.random() - 0.5) * 7 * hit : 0;

    const cx = x + shakeX;
    const cy = y + bob + shakeY;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    if (!dead || deathProgress < 0.55) {
      const fade = dead ? 1 - deathProgress / 0.55 : 1;
      const rx = cfg.orbitX * scale;
      const ry = cfg.orbitY * scale;

      // BACK layer.
      drawOrbitRing(ctx, cx, cy, rx, ry, state.orbitPhaseA, cfg.colors.orbit, 0.36 * fade, 1.5 * scale);
      drawSatellites(ctx, cx, cy, rx, ry, state.orbitPhaseA, 4, state.time, cfg.colors, state.mutation);

      // MIDDLE layer.
      drawOrbitRing(ctx, cx, cy, rx * 0.78, ry * 1.35, state.orbitPhaseB, cfg.colors.orbitAlt, 0.55 * fade, 1.2 * scale);

      drawCrystals(
        ctx,
        cx,
        cy,
        rx * 0.62,
        ry * 1.15,
        cfg.crystalSize * scale,
        cfg.colors,
        state.time,
      );

      // CORE layer.
      drawOrbitalCore(
        ctx,
        cx,
        cy,
        cfg.coreRadius * scale * (1 + hit * 0.1),
        cfg.colors,
      );

      if (hit > 0) {
        drawGlowDot(ctx, cx, cy, cfg.coreRadius * (1.8 + hit * 1.4) * scale, cfg.colors.hit, hit * 0.85);
      }
    }

    // DEATH layer: particles stay above the fading sphere.
    if (dead) {
      state.particles.forEach((p) => {
        if (p.life <= 0) return;
        const alpha = clamp01(p.life / 0.55);
        drawGlowDot(ctx, cx + p.x * scale, cy + p.y * scale, p.size * scale * 2, p.color, alpha * 0.65);
        ctx.fillStyle = rgba(p.color, alpha);
        ctx.beginPath();
        ctx.arc(cx + p.x * scale, cy + p.y * scale, p.size * scale, 0, TAU);
        ctx.fill();
      });
    }

    ctx.restore();
  }

  return {
    config: cfg,
    state,
    update,
    draw,
    hit,
    death,
    setMutation,
  };
}

/**
 * Mutations are deliberately behavior-oriented, not only palette swaps.
 * These functions can be called by gameplay code without knowing renderer
 * internals.
 */
export function mutateBase(sphere) {
  sphere.setMutation('base');
  return sphere;
}

export function mutateDance(sphere) {
  sphere.setMutation('dance');
  sphere.config.speeds.orbitA *= 1.45;
  sphere.config.speeds.orbitB *= 1.30;
  return sphere;
}

export function mutateEagle(sphere) {
  sphere.setMutation('eagle');
  sphere.config.colors.orbit = '#67c7ff';
  sphere.config.colors.orbitAlt = '#9be7ff';
  sphere.config.speeds.orbitA *= 0.72;
  return sphere;
}

export function mutateBlade(sphere) {
  sphere.setMutation('blade');
  sphere.config.colors.orbit = '#ff426f';
  sphere.config.colors.orbitAlt = '#ff8a45';
  sphere.config.colors.crystal = '#ff477f';
  sphere.config.speeds.orbitA *= 1.18;
  return sphere;
}

/**
 * Convenience demo loop. The production game should own requestAnimationFrame
 * and call update/draw from its existing loop instead.
 */
export function mountOrbitalSphereCanvas(canvas, options = {}) {
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('Canvas 2D context is unavailable.');

  canvas.style.background = 'transparent';
  const sphere = createOrbitalSphere(options);
  let last = performance.now();
  let raf = 0;

  const frame = (now) => {
    const dt = (now - last) / 1000;
    last = now;

    const dpr = Math.max(1, window.devicePixelRatio || 1);
    const rect = canvas.getBoundingClientRect();
    const width = Math.max(1, Math.round(rect.width * dpr));
    const height = Math.max(1, Math.round(rect.height * dpr));

    if (canvas.width !== width || canvas.height !== height) {
      canvas.width = width;
      canvas.height = height;
    }

    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, rect.width, rect.height);

    sphere.update(dt);
    sphere.draw(ctx, rect.width / 2, rect.height / 2, Math.min(1, rect.width / 220));

    raf = requestAnimationFrame(frame);
  };

  raf = requestAnimationFrame(frame);

  return {
    sphere,
    destroy() {
      cancelAnimationFrame(raf);
    },
  };
}
