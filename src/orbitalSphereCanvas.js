/**
 * ECHO SPHERE - Orbital Sphere
 * 2.5D Canvas renderer based on the project reference sheet.
 *
 * Visual structure:
 *   BACK    -> rear halves of orbital tracks + rear satellites
 *   MIDDLE  -> orbit tracks + satellite modules
 *   FRONT   -> front satellites + top/bottom crystals
 *   CORE    -> emissive nucleus
 *   FX      -> Hit / Death
 *
 * No WebGL, GLB or external assets are required.
 */

const TAU = Math.PI * 2;

export const ORBITAL_SPHERE_DEFAULTS = {
  coreRadius: 34,

  // Two crossing orbital tracks. Values are intentionally editable.
  orbitA: { rx: 76, ry: 25, rotation: -0.10 },
  orbitB: { rx: 64, ry: 21, rotation: Math.PI * 0.47 },

  crystal: {
    width: 10,
    height: 31,
    offsetY: 69,
  },

  satellite: {
    size: 9,
    orbitA: 4,
    orbitB: 4,
  },

  colors: {
    core: '#65cfff',
    coreHot: '#ffffff',
    coreDeep: '#0d4fd1',

    orbitA: '#20a8ff',
    orbitB: '#635cff',
    orbitGlow: '#55c7ff',

    satellite: '#b7e7ff',
    satelliteHot: '#ffffff',
    satelliteEdge: '#79caff',

    crystal: '#67bfff',
    crystalHot: '#ffffff',
    crystalDeep: '#3157e6',
    crystalEdge: '#e4f8ff',

    hit: '#ffffff',
    death: '#59baff',
  },

  speeds: {
    orbitA: 0.62,
    orbitB: -0.44,
    satelliteA: 0.90,
    satelliteB: -0.72,
    bob: 0.75,
  },
};

const MUTATIONS = {
  base: {
    orbitSpeed: 1,
    satelliteSpeed: 1,
    satelliteCountMultiplier: 1,
    satelliteColor: null,
    trail: 0.18,
    bladeShape: false,
    protectiveContour: false,
  },
  dance: {
    orbitSpeed: 1.15,
    satelliteSpeed: 1.72,
    satelliteCountMultiplier: 1.25,
    satelliteColor: '#b76cff',
    trail: 0.34,
    bladeShape: false,
    protectiveContour: false,
  },
  eagle: {
    orbitSpeed: 0.82,
    satelliteSpeed: 0.78,
    satelliteCountMultiplier: 1,
    satelliteColor: '#a8edff',
    trail: 0.24,
    bladeShape: false,
    protectiveContour: true,
  },
  blade: {
    orbitSpeed: 1.18,
    satelliteSpeed: 1.25,
    satelliteCountMultiplier: 1,
    satelliteColor: '#ff4b73',
    trail: 0.48,
    bladeShape: true,
    protectiveContour: false,
  },
};

const clamp01 = (value) => Math.max(0, Math.min(1, value));

function rgba(hex, alpha) {
  const value = hex.replace('#', '');
  const normalized = value.length === 3
    ? value.split('').map((c) => c + c).join('')
    : value;
  const n = Number.parseInt(normalized, 16);

  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${clamp01(alpha)})`;
}

function drawGlow(ctx, x, y, radius, color, alpha = 1) {
  const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
  gradient.addColorStop(0, rgba('#ffffff', alpha * 0.95));
  gradient.addColorStop(0.16, rgba(color, alpha * 0.72));
  gradient.addColorStop(0.52, rgba(color, alpha * 0.20));
  gradient.addColorStop(1, rgba(color, 0));

  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(x, y, radius, 0, TAU);
  ctx.fill();
}

function orbitPoint(cx, cy, rx, ry, angle, rotation) {
  const px = Math.cos(angle) * rx;
  const py = Math.sin(angle) * ry;

  return {
    x: cx + px * Math.cos(rotation) - py * Math.sin(rotation),
    y: cy + px * Math.sin(rotation) + py * Math.cos(rotation),
  };
}

/**
 * One half of an orbital track. Drawing halves separately is what gives
 * the sphere a real front/back ordering instead of a flat ellipse.
 */
function drawOrbitHalf(ctx, cx, cy, rx, ry, rotation, color, alpha, front, width) {
  ctx.save();
  ctx.translate(cx, cy);
  ctx.rotate(rotation);

  const start = front ? 0 : Math.PI;
  const end = front ? Math.PI : TAU;

  ctx.shadowColor = color;
  ctx.shadowBlur = 10;

  ctx.strokeStyle = rgba(color, alpha * 0.24);
  ctx.lineWidth = width * 3.2;
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, start, end);
  ctx.stroke();

  ctx.shadowBlur = 0;
  ctx.strokeStyle = rgba(color, alpha);
  ctx.lineWidth = width;
  ctx.beginPath();
  ctx.ellipse(0, 0, rx, ry, 0, start, end);
  ctx.stroke();

  ctx.restore();
}

function drawOrbitalTrack(ctx, cx, cy, orbit, color, alpha, width) {
  drawOrbitHalf(ctx, cx, cy, orbit.rx, orbit.ry, orbit.rotation, color, alpha * 0.42, false, width);
  drawOrbitHalf(ctx, cx, cy, orbit.rx, orbit.ry, orbit.rotation, color, alpha, true, width);
}

/**
 * The nucleus is a luminous sphere, not a flat blue circle.
 * Small latitude arcs reinforce the 2.5D construction visible in the sheet.
 */
export function drawOrbitalCore(ctx, x, y, radius, colors, pulse = 0) {
  drawGlow(ctx, x, y, radius * 2.05, colors.core, 0.88 + pulse * 0.2);

  const gradient = ctx.createRadialGradient(
    x - radius * 0.28,
    y - radius * 0.34,
    radius * 0.04,
    x,
    y,
    radius,
  );

  gradient.addColorStop(0, colors.coreHot);
  gradient.addColorStop(0.20, '#c9f5ff');
  gradient.addColorStop(0.48, colors.core);
  gradient.addColorStop(0.76, colors.coreDeep);
  gradient.addColorStop(1, '#06143e');

  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(x, y, radius, 0, TAU);
  ctx.fill();

  ctx.strokeStyle = rgba(colors.coreHot, 0.74);
  ctx.lineWidth = Math.max(1, radius * 0.045);
  ctx.beginPath();
  ctx.arc(x, y, radius * 0.90, 0, TAU);
  ctx.stroke();

  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(-0.20);

  ctx.strokeStyle = rgba('#ffffff', 0.42);
  ctx.lineWidth = Math.max(1, radius * 0.035);

  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 0.82, radius * 0.36, 0, Math.PI * 0.12, Math.PI * 0.92);
  ctx.stroke();

  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 0.72, radius * 0.30, 0, Math.PI * 1.12, Math.PI * 1.92);
  ctx.stroke();

  ctx.restore();

  drawGlow(ctx, x - radius * 0.30, y - radius * 0.34, radius * 0.28, '#ffffff', 0.68);
}

/**
 * Top and bottom crystals are the only axial crystals in the reference.
 * The left/right silhouettes are satellite modules, not extra crystals.
 */
export function drawAxialCrystals(ctx, x, y, config, colors, time, alpha = 1) {
  const pulse = 1 + Math.sin(time * 2.4) * 0.035;

  [-1, 1].forEach((direction) => {
    const px = x;
    const py = y + direction * config.offsetY;
    const w = config.width * pulse;
    const h = config.height * pulse;

    drawGlow(ctx, px, py, h * 0.95, colors.crystal, 0.22 * alpha);

    ctx.save();
    ctx.translate(px, py);
    if (direction > 0) ctx.rotate(Math.PI);

    ctx.beginPath();
    ctx.moveTo(0, -h);
    ctx.lineTo(w, -h * 0.18);
    ctx.lineTo(w * 0.68, h * 0.64);
    ctx.lineTo(0, h);
    ctx.lineTo(-w * 0.68, h * 0.64);
    ctx.lineTo(-w, -h * 0.18);
    ctx.closePath();

    const gradient = ctx.createLinearGradient(-w, -h, w, h);
    gradient.addColorStop(0, colors.crystalHot);
    gradient.addColorStop(0.22, colors.crystal);
    gradient.addColorStop(0.58, '#4179f4');
    gradient.addColorStop(1, colors.crystalDeep);

    ctx.globalAlpha = alpha;
    ctx.fillStyle = gradient;
    ctx.fill();
    ctx.strokeStyle = colors.crystalEdge;
    ctx.lineWidth = Math.max(1, w * 0.17);
    ctx.stroke();

    // Facet split gives the crystal the machined sci-fi appearance.
    ctx.strokeStyle = rgba('#ffffff', 0.40);
    ctx.lineWidth = Math.max(1, w * 0.07);
    ctx.beginPath();
    ctx.moveTo(0, -h * 0.76);
    ctx.lineTo(0, h * 0.72);
    ctx.stroke();

    ctx.restore();
  });
}

function drawSatellite(ctx, x, y, angle, size, colors, mutation, alpha) {
  const profile = MUTATIONS[mutation] || MUTATIONS.base;
  const moduleColor = profile.satelliteColor || colors.satellite;

  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle + (profile.bladeShape ? Math.PI * 0.25 : 0));
  ctx.globalAlpha = alpha;

  drawGlow(ctx, 0, 0, size * 1.65, moduleColor, 0.20);

  const w = size * (profile.bladeShape ? 1.65 : 1.48);
  const h = size * (profile.bladeShape ? 0.44 : 0.68);

  ctx.shadowColor = moduleColor;
  ctx.shadowBlur = profile.bladeShape ? 13 : 8;

  ctx.beginPath();
  if (profile.bladeShape) {
    ctx.moveTo(-w, 0);
    ctx.lineTo(-w * 0.15, -h);
    ctx.lineTo(w, 0);
    ctx.lineTo(-w * 0.15, h);
  } else {
    ctx.moveTo(-w, 0);
    ctx.lineTo(-w * 0.35, -h);
    ctx.lineTo(w * 0.62, -h * 0.56);
    ctx.lineTo(w, 0);
    ctx.lineTo(w * 0.62, h * 0.56);
    ctx.lineTo(-w * 0.35, h);
  }
  ctx.closePath();

  const gradient = ctx.createLinearGradient(-w, -h, w, h);
  gradient.addColorStop(0, colors.satelliteHot);
  gradient.addColorStop(0.30, moduleColor);
  gradient.addColorStop(0.72, colors.satelliteEdge);
  gradient.addColorStop(1, '#1740a9');

  ctx.fillStyle = gradient;
  ctx.fill();
  ctx.strokeStyle = colors.satelliteHot;
  ctx.lineWidth = Math.max(1, size * 0.12);
  ctx.stroke();

  ctx.shadowBlur = 0;
  ctx.fillStyle = rgba('#ffffff', 0.85);
  ctx.fillRect(-size * 0.16, -size * 0.12, size * 0.32, size * 0.24);

  // Energy tail is stronger for Dance and Blade.
  if (profile.trail > 0) {
    ctx.strokeStyle = rgba(moduleColor, profile.trail);
    ctx.lineWidth = Math.max(1, size * 0.18);
    ctx.beginPath();
    ctx.moveTo(-w * 0.75, 0);
    ctx.lineTo(-w * 1.8, 0);
    ctx.stroke();
  }

  ctx.restore();
}

function drawProtectiveContour(ctx, cx, cy, radius, color, time) {
  ctx.save();
  ctx.translate(cx, cy);
  ctx.rotate(time * 0.15);
  ctx.strokeStyle = rgba(color, 0.72);
  ctx.shadowColor = color;
  ctx.shadowBlur = 12;
  ctx.lineWidth = 2;

  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 1.38, radius * 0.64, -0.16, 0, TAU);
  ctx.stroke();

  ctx.beginPath();
  ctx.ellipse(0, 0, radius * 1.12, radius * 0.88, 0.74, 0, TAU);
  ctx.stroke();

  ctx.restore();
}

function drawSatellites(ctx, cx, cy, orbit, count, phase, speed, time, colors, mutation) {
  const profile = MUTATIONS[mutation] || MUTATIONS.base;
  const actualCount = Math.max(3, Math.round(count * profile.satelliteCountMultiplier));
  const angleStep = TAU / actualCount;

  for (let i = 0; i < actualCount; i += 1) {
    const angle = phase + i * angleStep;
    const point = orbitPoint(cx, cy, orbit.rx, orbit.ry, angle, orbit.rotation);
    const front = Math.sin(angle) > 0;

    drawSatellite(
      ctx,
      point.x,
      point.y,
      angle + orbit.rotation,
      8.5,
      colors,
      mutation,
      front ? 1 : 0.40,
    );
  }

  // Eagle's identity is a defensive orbital contour, not just a palette swap.
  if (mutation === 'eagle') {
    drawProtectiveContour(ctx, cx, cy, 48, profile.satelliteColor, time);
  }
}

function createDeathParticles(count, color) {
  return Array.from({ length: count }, (_, i) => {
    const angle = (TAU * i) / count + Math.random() * 0.30;
    const speed = 55 + Math.random() * 125;

    return {
      x: 0,
      y: 0,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      size: 1.5 + Math.random() * 3.5,
      life: 0.55 + Math.random() * 0.55,
      color,
    };
  });
}

export function createOrbitalSphere(config = {}) {
  const cfg = {
    ...ORBITAL_SPHERE_DEFAULTS,
    ...config,
    orbitA: { ...ORBITAL_SPHERE_DEFAULTS.orbitA, ...(config.orbitA || {}) },
    orbitB: { ...ORBITAL_SPHERE_DEFAULTS.orbitB, ...(config.orbitB || {}) },
    crystal: { ...ORBITAL_SPHERE_DEFAULTS.crystal, ...(config.crystal || {}) },
    satellite: { ...ORBITAL_SPHERE_DEFAULTS.satellite, ...(config.satellite || {}) },
    colors: { ...ORBITAL_SPHERE_DEFAULTS.colors, ...(config.colors || {}) },
    speeds: { ...ORBITAL_SPHERE_DEFAULTS.speeds, ...(config.speeds || {}) },
  };

  const baseSpeeds = { ...cfg.speeds };

  const state = {
    time: 0,
    hitTimer: 0,
    deathTimer: 0,
    deathDuration: 0.95,
    particles: [],
    mutation: 'base',
    phaseA: 0,
    phaseB: Math.PI * 0.35,
  };

  function setMutation(name) {
    state.mutation = MUTATIONS[name] ? name : 'base';
  }

  function hit(power = 1) {
    state.hitTimer = Math.max(state.hitTimer, 0.20 * Math.max(0.5, power));
  }

  function death() {
    if (state.deathTimer > 0) return;
    state.deathTimer = state.deathDuration;
    state.particles = createDeathParticles(34, cfg.colors.death);
  }

  function update(dt) {
    const step = Math.max(0, Math.min(dt, 0.05));
    const profile = MUTATIONS[state.mutation];

    state.time += step;
    state.phaseA += step * baseSpeeds.orbitA * profile.orbitSpeed;
    state.phaseB += step * baseSpeeds.orbitB * profile.orbitSpeed;
    state.hitTimer = Math.max(0, state.hitTimer - step);

    if (state.deathTimer > 0) {
      state.deathTimer = Math.max(0, state.deathTimer - step);
      state.particles.forEach((particle) => {
        particle.x += particle.vx * step;
        particle.y += particle.vy * step;
        particle.vx *= Math.pow(0.12, step);
        particle.vy *= Math.pow(0.12, step);
        particle.life -= step;
      });
    }
  }

  function draw(ctx, x, y, scale = 1) {
    const profile = MUTATIONS[state.mutation];
    const hit = clamp01(state.hitTimer / 0.20);
    const dead = state.deathTimer > 0;
    const deathProgress = 1 - clamp01(state.deathTimer / state.deathDuration);

    const bob = Math.sin(state.time * baseSpeeds.bob) * 2.4 * scale;
    const shake = hit * 5.5 * scale;

    const cx = x + (Math.random() - 0.5) * shake;
    const cy = y + bob + (Math.random() - 0.5) * shake;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    if (!dead || deathProgress < 0.52) {
      const fade = dead ? 1 - deathProgress / 0.52 : 1;

      // BACK: both tracks begin behind the core.
      drawOrbitHalf(ctx, cx, cy, cfg.orbitA.rx * scale, cfg.orbitA.ry * scale,
        cfg.orbitA.rotation, cfg.colors.orbitA, 0.40 * fade, false, 1.6 * scale);
      drawOrbitHalf(ctx, cx, cy, cfg.orbitB.rx * scale, cfg.orbitB.ry * scale,
        cfg.orbitB.rotation, cfg.colors.orbitB, 0.34 * fade, false, 1.35 * scale);

      // MIDDLE: complete track glow and rear/front satellite distribution.
      drawOrbitalTrack(ctx, cx, cy, {
        rx: cfg.orbitA.rx * scale,
        ry: cfg.orbitA.ry * scale,
        rotation: cfg.orbitA.rotation,
      }, cfg.colors.orbitA, 0.66 * fade, 1.45 * scale);

      drawOrbitalTrack(ctx, cx, cy, {
        rx: cfg.orbitB.rx * scale,
        ry: cfg.orbitB.ry * scale,
        rotation: cfg.orbitB.rotation,
      }, cfg.colors.orbitB, 0.52 * fade, 1.25 * scale);

      drawSatellites(
        ctx, cx, cy,
        { rx: cfg.orbitA.rx * scale, ry: cfg.orbitA.ry * scale, rotation: cfg.orbitA.rotation },
        cfg.satellite.orbitA,
        state.phaseA,
        baseSpeeds.satelliteA * profile.satelliteSpeed,
        state.time,
        cfg.colors,
        state.mutation,
      );

      drawSatellites(
        ctx, cx, cy,
        { rx: cfg.orbitB.rx * scale, ry: cfg.orbitB.ry * scale, rotation: cfg.orbitB.rotation },
        cfg.satellite.orbitB,
        state.phaseB,
        baseSpeeds.satelliteB * profile.satelliteSpeed,
        state.time,
        cfg.colors,
        state.mutation,
      );

      // FRONT: axial crystals sit outside the core and stay visually readable.
      drawAxialCrystals(ctx, cx, cy, {
        ...cfg.crystal,
        width: cfg.crystal.width * scale,
        height: cfg.crystal.height * scale,
        offsetY: cfg.crystal.offsetY * scale,
      }, cfg.colors, state.time, fade);

      // CORE is intentionally rendered after rear orbital geometry.
      drawOrbitalCore(
        ctx,
        cx,
        cy,
        cfg.coreRadius * scale * (1 + hit * 0.10),
        cfg.colors,
        hit,
      );

      if (hit > 0) {
        drawGlow(
          ctx,
          cx,
          cy,
          cfg.coreRadius * (1.7 + hit * 1.5) * scale,
          cfg.colors.hit,
          hit * 0.90,
        );
      }
    }

    if (dead) {
      state.particles.forEach((particle) => {
        if (particle.life <= 0) return;

        const alpha = clamp01(particle.life / 0.55);
        drawGlow(
          ctx,
          cx + particle.x * scale,
          cy + particle.y * scale,
          particle.size * 2.4 * scale,
          particle.color,
          alpha * 0.60,
        );

        ctx.fillStyle = rgba(particle.color, alpha);
        ctx.beginPath();
        ctx.arc(
          cx + particle.x * scale,
          cy + particle.y * scale,
          particle.size * scale,
          0,
          TAU,
        );
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
 * Public mutation functions.
 * They reset to a stable profile, so repeatedly switching mutations never
 * compounds speed/color changes.
 */
export function mutateBase(sphere) {
  sphere.setMutation('base');
  return sphere;
}

export function mutateDance(sphere) {
  sphere.setMutation('dance');
  return sphere;
}

export function mutateEagle(sphere) {
  sphere.setMutation('eagle');
  return sphere;
}

export function mutateBlade(sphere) {
  sphere.setMutation('blade');
  return sphere;
}

/**
 * Optional isolated preview loop.
 * The game runtime can instead call sphere.update() / sphere.draw() itself.
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
