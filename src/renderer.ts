import type { GameState, PlayerState, SphereEntity, EnemyEntity, DamageNumber, ChestEntity } from './engine';
import { PLAYER_RADIUS } from './engine';
import { SPHERE_TYPES, BOSS_TYPES } from './gameData';
import type { MapTheme } from './engine';

// ===== Origami / Paper Craft Style =====
// Warm backgrounds, faceted folded-paper shapes, fold lines, drop shadows.

const INK = '#3a2e1f';
const FOLD_LINE = 'rgba(58,46,31,0.18)';

const MUTATION_COLORS = ['#c46d3d', '#b85a30', '#a04830', '#d4943d', '#e8c878'];

interface Theme {
  bg: string;
  bgDark: string;
  grid: string;
  border: string;
  accent: string;
  textureDots: [string, string];
}

const THEMES: Record<MapTheme, Theme> = {
  parchment: {
    bg: '#f4ecd8', bgDark: '#e8dcc0', grid: 'rgba(58,46,31,0.06)', border: 'rgba(58,46,31,0.25)',
    accent: '#d4943d', textureDots: ['rgba(200,186,160,0.4)', 'rgba(120,100,70,0.15)'],
  },
  bamboo: {
    bg: '#e8e0c4', bgDark: '#d4ccaa', grid: 'rgba(80,60,30,0.06)', border: 'rgba(80,60,30,0.25)',
    accent: '#7a9a4a', textureDots: ['rgba(180,170,130,0.4)', 'rgba(90,80,40,0.12)'],
  },
  ocean: {
    bg: '#d8e0e4', bgDark: '#c0ccd4', grid: 'rgba(40,60,70,0.06)', border: 'rgba(40,60,70,0.25)',
    accent: '#4a7a8a', textureDots: ['rgba(160,180,190,0.4)', 'rgba(60,80,90,0.12)'],
  },
  sunset: {
    bg: '#f0d8c0', bgDark: '#e0c0a0', grid: 'rgba(100,50,30,0.06)', border: 'rgba(100,50,30,0.25)',
    accent: '#c46d3d', textureDots: ['rgba(200,170,140,0.4)', 'rgba(120,70,40,0.12)'],
  },
};

export function render(ctx: CanvasRenderingContext2D, s: GameState, canvasW: number, canvasH: number): void {
  const theme = THEMES[s.mapTheme] || THEMES.parchment;

  // ===== Static background (screen space) — reset transform to identity =====
  ctx.setTransform(1, 0, 0, 1, 0, 0);
  ctx.fillStyle = theme.bg;
  ctx.fillRect(0, 0, canvasW, canvasH);
  drawPaperTexture(ctx, canvasW, canvasH, theme);

  // ===== World space =====
  ctx.save();

  let shakeX = 0, shakeY = 0;
  if (s.screenShake > 0) {
    shakeX = (Math.random() - 0.5) * s.screenShake * 20;
    shakeY = (Math.random() - 0.5) * s.screenShake * 20;
  }
  ctx.translate(canvasW / 2 - s.camera.x + shakeX, canvasH / 2 - s.camera.y + shakeY);

  drawGrid(ctx, s, canvasW, canvasH, theme);

  // world bounds
  ctx.strokeStyle = theme.border;
  ctx.lineWidth = 3;
  ctx.setLineDash([12, 6]);
  ctx.strokeRect(-s.worldWidth / 2, -s.worldHeight / 2, s.worldWidth, s.worldHeight);
  ctx.setLineDash([]);

  // fire trails
  for (const ft of s.fireTrails) {
    const alpha = ft.life / ft.maxLife;
    ctx.fillStyle = `rgba(180,100,40,${alpha * 0.35})`;
    ctx.beginPath(); ctx.arc(ft.pos.x, ft.pos.y, 28, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = `rgba(120,60,20,${alpha * 0.2})`;
    ctx.beginPath(); ctx.arc(ft.pos.x, ft.pos.y, 18, 0, Math.PI * 2); ctx.fill();
  }

  // xp orbs
  for (const orb of s.xpOrbs) drawPaperDiamond(ctx, orb.pos.x, orb.pos.y, orb.radius, '#5a8c4a', '#7ab068');

  // health packs
  for (const hp of s.healthPacks) drawPaperCross(ctx, hp.pos.x, hp.pos.y, '#c4453d', '#e06b63');

  // chests
  for (const chest of s.chests) if (chest.alive) drawChest(ctx, chest);

  // spheres
  for (const sphere of s.spheres) drawSphere(ctx, s, sphere);

  // sphere projectiles
  for (const p of s.sphereProjectiles) drawPaperDart(ctx, p.pos.x, p.pos.y, p.vel.x, p.vel.y, p.radius, p.color);

  // minions
  for (const m of s.minions) drawPaperStar(ctx, m.pos.x, m.pos.y, m.radius, m.rotation, '#d4943d', '#e8b870');

  // enemies
  for (const e of s.enemies) drawEnemy(ctx, e);

  // boss projectiles
  for (const e of s.enemies) for (const bp of e.bossProjectiles) drawPaperDiamond(ctx, bp.pos.x, bp.pos.y, bp.radius, '#c4453d', '#e06b63');

  // player
  drawPlayer(ctx, s.player);

  // particles
  for (const p of s.particles) {
    const alpha = p.life / p.maxLife;
    ctx.fillStyle = p.color; ctx.globalAlpha = alpha;
    ctx.save(); ctx.translate(p.pos.x, p.pos.y);
    ctx.rotate(p.pos.x * 0.01 + Date.now() * 0.003);
    ctx.fillRect(-p.size, -p.size, p.size * 2, p.size * 2);
    ctx.restore();
  }
  ctx.globalAlpha = 1;

  // damage numbers
  for (const dn of s.damageNumbers) {
    const alpha = Math.min(1, dn.life / dn.maxLife * 1.5);
    ctx.globalAlpha = alpha;
    ctx.fillStyle = dn.crit ? '#c4453d' : INK;
    ctx.font = `bold ${dn.crit ? 20 : 14}px Georgia, serif`;
    ctx.textAlign = 'center';
    ctx.fillText(String(dn.value), dn.pos.x, dn.pos.y);
  }
  ctx.globalAlpha = 1;

  // lightnings
  for (const l of s.lightnings) {
    ctx.strokeStyle = INK; ctx.lineWidth = 2.5;
    ctx.globalAlpha = l.life / 0.3;
    drawLightning(ctx, l.from, l.to);
    ctx.globalAlpha = 1;
  }

  ctx.restore();

  // boss arrow (screen space)
  if (s.bossArrow) {
    const ax = canvasW / 2 + s.bossArrow.x * (canvasW / 2 - 40);
    const ay = canvasH / 2 + s.bossArrow.y * (canvasH / 2 - 40);
    ctx.save();
    ctx.fillStyle = '#c4453d';
    ctx.translate(ax, ay);
    ctx.rotate(Math.atan2(s.bossArrow.y, s.bossArrow.x));
    ctx.beginPath();
    ctx.moveTo(15, 0); ctx.lineTo(-5, -8); ctx.lineTo(-5, 8);
    ctx.closePath(); ctx.fill();
    ctx.restore();
  }

  // flash text
  if (s.flashText) {
    ctx.save();
    ctx.globalAlpha = Math.min(1, s.flashText.life);
    ctx.fillStyle = s.flashText.color;
    ctx.font = 'bold 42px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText(s.flashText.text, canvasW / 2, canvasH / 2 - 60);
    ctx.restore();
  }
}

// ===== Static paper texture (screen space) =====
let _textureCanvases: Partial<Record<MapTheme, HTMLCanvasElement>> = {};
function drawPaperTexture(ctx: CanvasRenderingContext2D, w: number, h: number, theme: Theme): void {
  if (!_textureCanvases[theme.bg as MapTheme]) {
    const tc = document.createElement('canvas');
    tc.width = 256; tc.height = 256;
    const tctx = tc.getContext('2d')!;
    tctx.fillStyle = theme.bg;
    tctx.fillRect(0, 0, 256, 256);
    for (let i = 0; i < 1000; i++) {
      const x = Math.random() * 256;
      const y = Math.random() * 256;
      const r = Math.random() * 1.5;
      tctx.fillStyle = Math.random() < 0.5 ? theme.textureDots[0] : theme.textureDots[1];
      tctx.beginPath();
      tctx.arc(x, y, r, 0, Math.PI * 2);
      tctx.fill();
    }
    // add some subtle fiber lines
    for (let i = 0; i < 30; i++) {
      tctx.strokeStyle = theme.textureDots[1];
      tctx.lineWidth = 0.5;
      tctx.beginPath();
      const x1 = Math.random() * 256, y1 = Math.random() * 256;
      tctx.moveTo(x1, y1);
      tctx.lineTo(x1 + (Math.random() - 0.5) * 40, y1 + (Math.random() - 0.5) * 40);
      tctx.stroke();
    }
    _textureCanvases[theme.bg as MapTheme] = tc;
  }
  const tile = _textureCanvases[theme.bg as MapTheme]!;
  // Tile manually in screen space — drawImage is not affected by pattern transform issues
  for (let x = 0; x < w; x += 256) {
    for (let y = 0; y < h; y += 256) {
      ctx.drawImage(tile, x, y);
    }
  }
}

function drawGrid(ctx: CanvasRenderingContext2D, s: GameState, canvasW: number, canvasH: number, theme: Theme): void {
  const grid = 80;
  const startX = Math.floor((s.camera.x - canvasW / 2) / grid) * grid;
  const startY = Math.floor((s.camera.y - canvasH / 2) / grid) * grid;
  ctx.strokeStyle = theme.grid;
  ctx.lineWidth = 1;
  ctx.setLineDash([4, 4]);
  ctx.beginPath();
  for (let x = startX; x < s.camera.x + canvasW / 2 + grid; x += grid) {
    ctx.moveTo(x, s.camera.y - canvasH / 2 - grid);
    ctx.lineTo(x, s.camera.y + canvasH / 2 + grid);
  }
  for (let y = startY; y < s.camera.y + canvasH / 2 + grid; y += grid) {
    ctx.moveTo(s.camera.x - canvasW / 2 - grid, y);
    ctx.lineTo(s.camera.x + canvasW / 2 + grid, y);
  }
  ctx.stroke();
  ctx.setLineDash([]);
}

// ===== Paper helpers =====
function drawShadow(ctx: CanvasRenderingContext2D, fn: () => void, dx = 3, dy = 4): void {
  ctx.save(); ctx.translate(dx, dy);
  ctx.fillStyle = 'rgba(58,46,31,0.12)';
  fn(); ctx.restore();
}

function shade(hex: string, amt: number): string {
  const c = hex.replace('#', '');
  const r = Math.max(0, Math.min(255, parseInt(c.slice(0, 2), 16) + amt));
  const g = Math.max(0, Math.min(255, parseInt(c.slice(2, 4), 16) + amt));
  const b = Math.max(0, Math.min(255, parseInt(c.slice(4, 6), 16) + amt));
  return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
}

function hexToRgb(hex: string): string {
  const h = hex.replace('#', '');
  return `${parseInt(h.substring(0, 2), 16)},${parseInt(h.substring(2, 4), 16)},${parseInt(h.substring(4, 6), 16)}`;
}

function foldAndOutline(ctx: CanvasRenderingContext2D, fill: string, foldLines: { from: [number, number]; to: [number, number] }[], outline: [number, number][]): void {
  ctx.fillStyle = fill;
  ctx.beginPath();
  outline.forEach((p, i) => i === 0 ? ctx.moveTo(p[0], p[1]) : ctx.lineTo(p[0], p[1]));
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  for (const fl of foldLines) { ctx.beginPath(); ctx.moveTo(fl.from[0], fl.from[1]); ctx.lineTo(fl.to[0], fl.to[1]); ctx.stroke(); }
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  outline.forEach((p, i) => i === 0 ? ctx.moveTo(p[0], p[1]) : ctx.lineTo(p[0], p[1]));
  ctx.closePath(); ctx.stroke();
}

// ===== Origami shapes =====

function drawPaperDiamond(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.moveTo(x, y - r); ctx.lineTo(x + r, y); ctx.lineTo(x, y + r); ctx.lineTo(x - r, y);
    ctx.closePath(); ctx.fill();
  });
  ctx.fillStyle = highlight;
  ctx.beginPath(); ctx.moveTo(x, y - r); ctx.lineTo(x + r, y); ctx.lineTo(x, y + r); ctx.closePath(); ctx.fill();
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(x, y - r); ctx.lineTo(x - r, y); ctx.lineTo(x, y + r); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(x, y - r); ctx.lineTo(x, y + r); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(x, y - r); ctx.lineTo(x + r, y); ctx.lineTo(x, y + r); ctx.lineTo(x - r, y); ctx.closePath(); ctx.stroke();
}

function drawPaperCross(ctx: CanvasRenderingContext2D, x: number, y: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => { ctx.fillRect(x - 3, y - 9, 6, 18); ctx.fillRect(x - 9, y - 3, 18, 6); });
  ctx.fillStyle = highlight; ctx.fillRect(x - 3, y - 9, 6, 18);
  ctx.fillStyle = fill; ctx.fillRect(x - 9, y - 3, 18, 6);
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.strokeRect(x - 3, y - 9, 6, 18); ctx.strokeRect(x - 9, y - 3, 18, 6);
}

function drawPaperStar(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, rot: number, fill: string, highlight: string): void {
  const spikes = 5, outer = r, inner = r * 0.45;
  const pts: { x: number; y: number }[] = [];
  for (let i = 0; i < spikes * 2; i++) {
    const rad = i % 2 === 0 ? outer : inner;
    const a = rot + (i / (spikes * 2)) * Math.PI * 2 - Math.PI / 2;
    pts.push({ x: x + Math.cos(a) * rad, y: y + Math.sin(a) * rad });
  }
  drawShadow(ctx, () => { ctx.beginPath(); pts.forEach((p, i) => i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y)); ctx.closePath(); ctx.fill(); });
  ctx.fillStyle = fill;
  ctx.beginPath(); pts.forEach((p, i) => i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y)); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.35;
  ctx.beginPath(); pts.forEach((p, i) => i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y)); ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); pts.forEach((p, i) => i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y)); ctx.closePath(); ctx.stroke();
}

function drawPaperDart(ctx: CanvasRenderingContext2D, x: number, y: number, vx: number, vy: number, r: number, color: string): void {
  const angle = Math.atan2(vy, vx);
  ctx.save(); ctx.translate(x, y); ctx.rotate(angle);
  ctx.fillStyle = 'rgba(58,46,31,0.12)';
  ctx.beginPath(); ctx.moveTo(r * 1.5 + 2, 2); ctx.lineTo(-r + 2, -r * 0.7 + 2); ctx.lineTo(-r + 2, r * 0.7 + 2); ctx.closePath(); ctx.fill();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.moveTo(r * 1.5, 0); ctx.lineTo(-r, -r * 0.7); ctx.lineTo(-r, r * 0.7); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r * 1.5, 0); ctx.lineTo(-r, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.2;
  ctx.beginPath(); ctx.moveTo(r * 1.5, 0); ctx.lineTo(-r, -r * 0.7); ctx.lineTo(-r, r * 0.7); ctx.closePath(); ctx.stroke();
  ctx.restore();
}

// ===== Origami Airplane (triangle-type enemy) =====
function drawOrigamiAirplane(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.moveTo(3, -r + 4); ctx.lineTo(r + 3, r + 4); ctx.lineTo(-r + 3, r + 4);
    ctx.closePath(); ctx.fill();
  });
  // main body — swept wings
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2);
  ctx.lineTo(r * 1.3, r * 0.8);
  ctx.lineTo(0, r * 0.4);
  ctx.lineTo(-r * 1.3, r * 0.8);
  ctx.closePath(); ctx.fill();
  // right wing highlight
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.35;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2); ctx.lineTo(r * 1.3, r * 0.8); ctx.lineTo(0, r * 0.4);
  ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  // fold lines — keel
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 1.2); ctx.lineTo(0, r * 0.4); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r * 1.3, r * 0.8); ctx.lineTo(0, r * 0.4); ctx.lineTo(r * 1.3, r * 0.8); ctx.stroke();
  // outline
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2); ctx.lineTo(r * 1.3, r * 0.8); ctx.lineTo(0, r * 0.4); ctx.lineTo(-r * 1.3, r * 0.8);
  ctx.closePath(); ctx.stroke();
}

// ===== Origami Boat (square-type enemy) =====
function drawOrigamiBoat(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.moveTo(-r + 3, -r * 0.3 + 4); ctx.lineTo(r + 3, -r * 0.3 + 4); ctx.lineTo(r * 0.6 + 3, r + 4); ctx.lineTo(-r * 0.6 + 3, r + 4);
    ctx.closePath(); ctx.fill();
  });
  // hull
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(-r, -r * 0.3); ctx.lineTo(r, -r * 0.3); ctx.lineTo(r * 0.6, r); ctx.lineTo(-r * 0.6, r);
  ctx.closePath(); ctx.fill();
  // sail
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.8, -r * 0.3); ctx.lineTo(0, -r * 1.2);
  ctx.closePath(); ctx.fill();
  // left sail
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(-r * 0.8, -r * 0.3); ctx.lineTo(0, -r * 1.2);
  ctx.closePath(); ctx.fill();
  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 1.2); ctx.lineTo(0, r); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r, -r * 0.3); ctx.lineTo(r, -r * 0.3); ctx.stroke();
  // outline
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(-r, -r * 0.3); ctx.lineTo(r, -r * 0.3); ctx.lineTo(r * 0.6, r); ctx.lineTo(-r * 0.6, r);
  ctx.closePath(); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.8, -r * 0.3); ctx.lineTo(0, -r * 1.2); ctx.lineTo(-r * 0.8, -r * 0.3);
  ctx.closePath(); ctx.stroke();
}

// ===== Origami Crane (circle-type enemy) =====
function drawOrigamiCrane(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.moveTo(3, -r + 4); ctx.lineTo(r + 3, r * 0.3 + 4); ctx.lineTo(-r + 3, r * 0.3 + 4);
    ctx.closePath(); ctx.fill();
  });
  // body
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r);
  ctx.lineTo(r * 0.3, -r * 0.2);
  ctx.lineTo(r, r * 0.5);
  ctx.lineTo(r * 0.2, r * 0.3);
  ctx.lineTo(0, r);
  ctx.lineTo(-r * 0.2, r * 0.3);
  ctx.lineTo(-r, r * 0.5);
  ctx.lineTo(-r * 0.3, -r * 0.2);
  ctx.closePath(); ctx.fill();
  // wing highlight
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r * 0.3, -r * 0.2); ctx.lineTo(r, r * 0.5); ctx.lineTo(0, r);
  ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r); ctx.lineTo(0, r); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r, r * 0.5); ctx.lineTo(r, r * 0.5); ctx.stroke();
  // outline
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r * 0.3, -r * 0.2); ctx.lineTo(r, r * 0.5); ctx.lineTo(r * 0.2, r * 0.3);
  ctx.lineTo(0, r); ctx.lineTo(-r * 0.2, r * 0.3); ctx.lineTo(-r, r * 0.5); ctx.lineTo(-r * 0.3, -r * 0.2);
  ctx.closePath(); ctx.stroke();
}

// ===== Origami Frog (fast triangle enemy variant) =====
function drawOrigamiFrog(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.ellipse(3, 4, r, r * 0.8, 0, 0, Math.PI * 2);
    ctx.fill();
  });
  // body — diamond with legs
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r, 0); ctx.lineTo(0, r); ctx.lineTo(-r, 0);
  ctx.closePath(); ctx.fill();
  // back legs
  ctx.fillStyle = shade(fill, -20);
  ctx.beginPath(); ctx.moveTo(-r, 0); ctx.lineTo(-r * 1.4, r * 0.8); ctx.lineTo(-r * 0.5, r * 0.3); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(r, 0); ctx.lineTo(r * 1.4, r * 0.8); ctx.lineTo(r * 0.5, r * 0.3); ctx.closePath(); ctx.fill();
  // highlight
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.moveTo(0, -r); ctx.lineTo(r, 0); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r); ctx.lineTo(0, r); ctx.moveTo(-r, 0); ctx.lineTo(r, 0); ctx.stroke();
  // outline
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, -r); ctx.lineTo(r, 0); ctx.lineTo(0, r); ctx.lineTo(-r, 0); ctx.closePath(); ctx.stroke();
}

// ===== Origami Butterfly (tank enemy variant) =====
function drawOrigamiButterfly(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.ellipse(3, 4, r * 1.2, r, 0, 0, Math.PI * 2);
    ctx.fill();
  });
  // upper wings
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(-r * 1.2, -r); ctx.lineTo(-r * 0.5, -r * 0.2); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(r * 1.2, -r); ctx.lineTo(r * 0.5, -r * 0.2); ctx.closePath(); ctx.fill();
  // lower wings
  ctx.fillStyle = shade(fill, -15);
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(-r, r * 0.8); ctx.lineTo(-r * 0.3, r * 0.2); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(r, r * 0.8); ctx.lineTo(r * 0.3, r * 0.2); ctx.closePath(); ctx.fill();
  // body
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.ellipse(0, 0, r * 0.1, r * 0.5, 0, 0, Math.PI * 2); ctx.fill();
  // highlight on upper wings
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(-r * 1.2, -r); ctx.lineTo(-r * 0.5, -r * 0.2); ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.5); ctx.lineTo(0, r * 0.5); ctx.stroke();
  // outline
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(-r * 1.2, -r); ctx.lineTo(-r * 0.5, -r * 0.2); ctx.closePath(); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(r * 1.2, -r); ctx.lineTo(r * 0.5, -r * 0.2); ctx.closePath(); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(-r, r * 0.8); ctx.lineTo(-r * 0.3, r * 0.2); ctx.closePath(); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(r, r * 0.8); ctx.lineTo(r * 0.3, r * 0.2); ctx.closePath(); ctx.stroke();
}

// ===== Origami Lotus (boss shapes) =====

function drawOrigamiLotus(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  const petals = 8;
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.arc(4, 5, r * 1.1, 0, Math.PI * 2); ctx.fill();
  });
  // outer petals
  for (let i = 0; i < petals; i++) {
    const a = (i / petals) * Math.PI * 2;
    ctx.fillStyle = i % 2 === 0 ? fill : shade(fill, -15);
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(Math.cos(a - 0.3) * r, Math.sin(a - 0.3) * r);
    ctx.lineTo(Math.cos(a) * r * 1.2, Math.sin(a) * r * 1.2);
    ctx.lineTo(Math.cos(a + 0.3) * r, Math.sin(a + 0.3) * r);
    ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.2;
    ctx.stroke();
  }
  // inner petals
  for (let i = 0; i < petals / 2; i++) {
    const a = (i / (petals / 2)) * Math.PI * 2 + Math.PI / petals;
    ctx.fillStyle = highlight;
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(Math.cos(a - 0.2) * r * 0.5, Math.sin(a - 0.2) * r * 0.5);
    ctx.lineTo(Math.cos(a) * r * 0.7, Math.sin(a) * r * 0.7);
    ctx.lineTo(Math.cos(a + 0.2) * r * 0.5, Math.sin(a + 0.2) * r * 0.5);
    ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1;
    ctx.stroke();
  }
  // center
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(0, 0, r * 0.12, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiDragon(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.ellipse(4, 5, r * 1.2, r * 0.8, 0, 0, Math.PI * 2); ctx.fill();
  });
  // serpentine body segments
  const segments = 7;
  for (let i = segments - 1; i >= 0; i--) {
    const phase = t * 2 + i * 0.4;
    const sx = Math.sin(phase) * r * 0.6;
    const sy = -r * 0.7 + (i / (segments - 1)) * r * 1.4;
    const sr = r * (1 - i * 0.08);
    ctx.fillStyle = i % 2 === 0 ? fill : shade(fill, -20);
    ctx.beginPath();
    ctx.moveTo(sx, sy - sr * 0.3);
    ctx.lineTo(sx + sr * 0.7, sy);
    ctx.lineTo(sx, sy + sr * 0.3);
    ctx.lineTo(sx - sr * 0.7, sy);
    ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.2;
    ctx.stroke();
  }
  // head — larger diamond at top
  const headX = Math.sin(t * 2) * r * 0.6;
  const headY = -r * 0.7;
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(headX, headY - r * 0.5); ctx.lineTo(headX + r * 0.6, headY); ctx.lineTo(headX, headY + r * 0.3); ctx.lineTo(headX - r * 0.6, headY);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  // eyes
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(headX - r * 0.2, headY - r * 0.1, 2, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(headX + r * 0.2, headY - r * 0.1, 2, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiPhoenix(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.ellipse(4, 5, r * 1.3, r, 0, 0, Math.PI * 2); ctx.fill();
  });
  // tail feathers
  const feathers = 7;
  for (let i = 0; i < feathers; i++) {
    const a = Math.PI * 0.3 + (i / (feathers - 1)) * Math.PI * 0.4;
    const len = r * (1.2 + Math.sin(t * 3 + i) * 0.15);
    ctx.fillStyle = i % 2 === 0 ? fill : shade(fill, 20);
    ctx.beginPath();
    ctx.moveTo(0, r * 0.3);
    ctx.lineTo(Math.cos(a) * len, Math.sin(a) * len + r * 0.3);
    ctx.lineTo(Math.cos(a + 0.1) * len * 0.8, Math.sin(a + 0.1) * len * 0.8 + r * 0.3);
    ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.2; ctx.stroke();
  }
  // body
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.5, 0); ctx.lineTo(0, r * 0.5); ctx.lineTo(-r * 0.5, 0);
  ctx.closePath(); ctx.fill();
  // wings spread
  ctx.fillStyle = shade(fill, -10);
  const flap = Math.sin(t * 4) * 0.2;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 1.3, -r * 0.3 + flap * r); ctx.lineTo(r * 0.8, r * 0.2); ctx.lineTo(0, 0);
  ctx.closePath(); ctx.fill();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(-r * 1.3, -r * 0.3 + flap * r); ctx.lineTo(-r * 0.8, r * 0.2); ctx.lineTo(0, 0);
  ctx.closePath(); ctx.fill();
  // head
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.3, -r * 1.1); ctx.lineTo(-r * 0.3, -r * 1.1);
  ctx.closePath(); ctx.fill();
  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.8); ctx.lineTo(0, r * 0.5); ctx.stroke();
  // outlines
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.5, 0); ctx.lineTo(0, r * 0.5); ctx.lineTo(-r * 0.5, 0); ctx.closePath(); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.3, -r * 1.1); ctx.lineTo(-r * 0.3, -r * 1.1); ctx.closePath(); ctx.stroke();
  // eye
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(0, -r * 0.9, 2, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiOctopus(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.arc(4, 5, r, 0, Math.PI * 2); ctx.fill();
  });
  // head
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.arc(0, -r * 0.2, r * 0.7, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  // tentacles
  ctx.strokeStyle = shade(fill, -10); ctx.lineWidth = 3; ctx.lineCap = 'round';
  for (let i = 0; i < 8; i++) {
    const a = (i / 8) * Math.PI * 2 + t * 0.5;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * r * 0.5, Math.sin(a) * r * 0.5 - r * 0.2);
    for (let k = 1; k <= 6; k++) {
      const wave = Math.sin(t * 2 + k * 0.5 + i) * 5;
      const tx = Math.cos(a) * (r * 0.5 + k * r * 0.12) + Math.cos(a + Math.PI / 2) * wave;
      const ty = Math.sin(a) * (r * 0.5 + k * r * 0.12) + Math.sin(a + Math.PI / 2) * wave - r * 0.2;
      ctx.lineTo(tx, ty);
    }
    ctx.stroke();
  }
  // eyes
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(-r * 0.2, -r * 0.3, 3, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.2, -r * 0.3, 3, 0, Math.PI * 2); ctx.fill();
  // highlight
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.arc(-r * 0.2, -r * 0.4, r * 0.3, 0, Math.PI * 2); ctx.fill();
  ctx.globalAlpha = 1;
}

// ===== Player — origami fox/wolf =====
function drawPlayer(ctx: CanvasRenderingContext2D, p: PlayerState): void {
  ctx.save();
  ctx.translate(p.pos.x, p.pos.y);
  const color = MUTATION_COLORS[p.mutationStage];
  const t = Date.now() / 1000;
  const pulse = 1 + Math.sin(t * 5) * 0.06;
  const r = PLAYER_RADIUS * pulse;

  // shield
  if (p.shieldCharges > 0 || p.shieldTimer > 0) {
    ctx.strokeStyle = '#4a7a8a'; ctx.lineWidth = 2.5;
    ctx.globalAlpha = 0.5 + Math.sin(t * 6) * 0.2;
    ctx.setLineDash([6, 4]);
    ctx.beginPath(); ctx.arc(0, 0, r + 10, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }
  if (p.invulnerableTimer > 0) {
    ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 3;
    ctx.beginPath(); ctx.arc(0, 0, r + 14, 0, Math.PI * 2); ctx.stroke();
  }

  // drop shadow
  ctx.fillStyle = 'rgba(58,46,31,0.15)';
  ctx.beginPath();
  ctx.ellipse(3, 4, r * 1.1, r * 0.8, 0, 0, Math.PI * 2); ctx.fill();

  // ===== Origami fox body — diamond head with ears =====
  // main head diamond
  const grad = ctx.createLinearGradient(0, -r * 1.4, 0, r);
  grad.addColorStop(0, shade(color, 40));
  grad.addColorStop(1, shade(color, -30));
  ctx.fillStyle = grad;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2); ctx.lineTo(r * 1.1, 0); ctx.lineTo(0, r * 0.9); ctx.lineTo(-r * 1.1, 0);
  ctx.closePath(); ctx.fill();

  // ears — two triangles at top
  ctx.fillStyle = shade(color, 20);
  ctx.beginPath();
  ctx.moveTo(-r * 0.6, -r * 0.7); ctx.lineTo(-r * 0.3, -r * 1.5); ctx.lineTo(-r * 0.1, -r * 0.8); ctx.closePath(); ctx.fill();
  ctx.beginPath();
  ctx.moveTo(r * 0.6, -r * 0.7); ctx.lineTo(r * 0.3, -r * 1.5); ctx.lineTo(r * 0.1, -r * 0.8); ctx.closePath(); ctx.fill();

  // inner ears
  ctx.fillStyle = shade(color, -20);
  ctx.beginPath();
  ctx.moveTo(-r * 0.45, -r * 0.8); ctx.lineTo(-r * 0.3, -r * 1.3); ctx.lineTo(-r * 0.2, -r * 0.85); ctx.closePath(); ctx.fill();
  ctx.beginPath();
  ctx.moveTo(r * 0.45, -r * 0.8); ctx.lineTo(r * 0.3, -r * 1.3); ctx.lineTo(r * 0.2, -r * 0.85); ctx.closePath(); ctx.fill();

  // snout — lighter triangle
  ctx.fillStyle = shade(color, 50);
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.4, r * 0.2); ctx.lineTo(-r * 0.4, r * 0.2); ctx.closePath(); ctx.fill();

  // fold lines
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2); ctx.lineTo(0, r * 0.9);
  ctx.moveTo(-r * 1.1, 0); ctx.lineTo(r * 1.1, 0); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.4, r * 0.2); ctx.lineTo(-r * 0.4, r * 0.2); ctx.closePath(); ctx.stroke();

  // outlines
  ctx.strokeStyle = INK; ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(0, -r * 1.2); ctx.lineTo(r * 1.1, 0); ctx.lineTo(0, r * 0.9); ctx.lineTo(-r * 1.1, 0); ctx.closePath(); ctx.stroke();
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(-r * 0.6, -r * 0.7); ctx.lineTo(-r * 0.3, -r * 1.5); ctx.lineTo(-r * 0.1, -r * 0.8); ctx.closePath(); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(r * 0.6, -r * 0.7); ctx.lineTo(r * 0.3, -r * 1.5); ctx.lineTo(r * 0.1, -r * 0.8); ctx.closePath(); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.4, r * 0.2); ctx.lineTo(-r * 0.4, r * 0.2); ctx.closePath(); ctx.stroke();

  // eyes
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(-r * 0.3, -r * 0.2, 2.5, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.3, -r * 0.2, 2.5, 0, Math.PI * 2); ctx.fill();
  // nose
  ctx.beginPath(); ctx.arc(0, r * 0.15, 2, 0, Math.PI * 2); ctx.fill();

  // mutations
  if (p.mutationStage >= 1) drawSpikes(ctx, color);
  if (p.mutationStage >= 2) drawWings(ctx, color);
  if (p.mutationStage >= 3) drawHalo(ctx);
  if (p.mutationStage >= 4) drawTentacles(ctx);

  ctx.restore();
}

function drawSpikes(ctx: CanvasRenderingContext2D, color: string): void {
  ctx.fillStyle = shade(color, 20); ctx.strokeStyle = INK; ctx.lineWidth = 1.2;
  for (let i = 0; i < 8; i++) {
    const a = (i / 8) * Math.PI * 2 + Date.now() / 1000;
    const r1 = PLAYER_RADIUS, r2 = PLAYER_RADIUS + 7;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a - 0.12) * r1, Math.sin(a - 0.12) * r1);
    ctx.lineTo(Math.cos(a) * r2, Math.sin(a) * r2);
    ctx.lineTo(Math.cos(a + 0.12) * r1, Math.sin(a + 0.12) * r1);
    ctx.closePath(); ctx.fill(); ctx.stroke();
  }
}
function drawWings(ctx: CanvasRenderingContext2D, color: string): void {
  ctx.strokeStyle = shade(color, 10); ctx.lineWidth = 2.5;
  const flap = Math.sin(Date.now() / 200) * 0.3;
  ctx.beginPath(); ctx.arc(-PLAYER_RADIUS - 4, 0, PLAYER_RADIUS - 2, -0.8 - flap, 0.8 + flap); ctx.stroke();
  ctx.beginPath(); ctx.arc(PLAYER_RADIUS + 4, 0, PLAYER_RADIUS - 2, Math.PI - 0.8 - flap, Math.PI + 0.8 + flap); ctx.stroke();
}
function drawHalo(ctx: CanvasRenderingContext2D): void {
  const pulse = 1 + Math.sin(Date.now() / 300) * 0.12;
  ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2.5;
  ctx.beginPath(); ctx.ellipse(0, -PLAYER_RADIUS - 12, (PLAYER_RADIUS + 6) * pulse, 5, 0, 0, Math.PI * 2); ctx.stroke();
}
function drawTentacles(ctx: CanvasRenderingContext2D): void {
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  const t = Date.now() / 400;
  for (let i = 0; i < 6; i++) {
    const a = (i / 6) * Math.PI * 2 + t * 0.5;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * PLAYER_RADIUS, Math.sin(a) * PLAYER_RADIUS);
    for (let k = 1; k <= 8; k++) {
      const r = PLAYER_RADIUS + k * 3;
      const wave = Math.sin(t + k * 0.5 + i) * 4;
      ctx.lineTo(Math.cos(a) * r + Math.cos(a + Math.PI / 2) * wave, Math.sin(a) * r + Math.sin(a + Math.PI / 2) * wave);
    }
    ctx.stroke();
  }
}

// ===== Sphere — origami turret/tower =====
function drawSphere(ctx: CanvasRenderingContext2D, s: GameState, sphere: SphereEntity): void {
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  const stype = SPHERE_TYPES[sphere.type];
  const baseColor = stype.color;
  const radius = sphere.radius * (1 + (s.player.abilities.radius || 0) * 0.15) * (s.player.artifacts.includes('radius_shard') ? 1.1 : 1) * stype.rangeMult;
  const t = Date.now() / 1000;

  // range indicator
  ctx.strokeStyle = `rgba(${hexToRgb(baseColor)},0.12)`;
  ctx.lineWidth = 1; ctx.setLineDash([5, 5]);
  ctx.beginPath(); ctx.arc(0, 0, radius, 0, Math.PI * 2); ctx.stroke();
  ctx.setLineDash([]);

  if (stype.aura) {
    ctx.strokeStyle = baseColor; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.25 + Math.sin(t * 3) * 0.08;
    ctx.setLineDash([8, 4]);
    ctx.beginPath(); ctx.arc(0, 0, stype.auraRadius, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }

  const tier = sphere.visualTier;

  // ===== Origami turret base =====
  // shadow
  ctx.fillStyle = 'rgba(58,46,31,0.15)';
  ctx.beginPath(); ctx.ellipse(2, 3, 16, 12, 0, 0, Math.PI * 2); ctx.fill();

  // base — octagonal platform
  ctx.fillStyle = shade(baseColor, -30);
  ctx.beginPath();
  for (let i = 0; i < 8; i++) {
    const a = (i / 8) * Math.PI * 2 + Math.PI / 8;
    const x = Math.cos(a) * 16, y = Math.sin(a) * 16;
    if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
  }
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();

  // fold lines on base
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  for (let i = 0; i < 4; i++) {
    const a = (i / 4) * Math.PI * 2 + Math.PI / 8;
    ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(Math.cos(a) * 16, Math.sin(a) * 16); ctx.stroke();
  }

  // turret barrel — type-specific origami shape
  ctx.save();
  ctx.rotate(sphere.rotation);

  if (sphere.type === 'sniper') {
    // Sniper: long barrel — elongated diamond
    ctx.fillStyle = baseColor;
    ctx.beginPath();
  ctx.moveTo(0, -4); ctx.lineTo(22, 0); ctx.lineTo(0, 4); ctx.lineTo(-6, 0);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-6, 0); ctx.lineTo(22, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(22, 0); ctx.lineTo(0, 4); ctx.lineTo(-6, 0); ctx.closePath(); ctx.stroke();
  // scope on top
  ctx.fillStyle = shade(baseColor, 30);
  ctx.beginPath(); ctx.arc(4, -6, 3, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    // Shotgun: wide triple barrel
    ctx.fillStyle = baseColor;
    for (let i = -1; i <= 1; i++) {
      ctx.beginPath();
      ctx.moveTo(i * 5 - 3, -3); ctx.lineTo(i * 5 + 14, i * 2); ctx.lineTo(i * 5 + 14, i * 2 + 3); ctx.lineTo(i * 5 - 3, 3);
      ctx.closePath(); ctx.fill();
      ctx.strokeStyle = INK; ctx.lineWidth = 1; ctx.stroke();
    }
  } else if (sphere.type === 'chain') {
    // Chain: forked antenna
    ctx.fillStyle = baseColor;
    ctx.beginPath();
  ctx.moveTo(-4, -3); ctx.lineTo(8, -8); ctx.lineTo(10, -6); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(-4, 3); ctx.lineTo(8, 8); ctx.lineTo(10, 6); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  // spark at tip
  ctx.fillStyle = '#d4a830';
  ctx.beginPath(); ctx.arc(10, -7, 2 + Math.sin(t * 8) * 1, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(10, 7, 2 + Math.sin(t * 8 + 1) * 1, 0, Math.PI * 2); ctx.fill();
  } else if (sphere.type === 'aura') {
    // Aura: paper lantern — no barrel, radiating folds
    ctx.fillStyle = baseColor;
    ctx.beginPath();
  ctx.moveTo(0, -10); ctx.lineTo(10, 0); ctx.lineTo(0, 10); ctx.lineTo(-10, 0);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(0, 10); ctx.moveTo(-10, 0); ctx.lineTo(10, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(10, 0); ctx.lineTo(0, 10); ctx.lineTo(-10, 0); ctx.closePath(); ctx.stroke();
  // glow
  ctx.fillStyle = `rgba(${hexToRgb(baseColor)},0.2)`;
  ctx.beginPath(); ctx.arc(0, 0, 14 + Math.sin(t * 4) * 2, 0, Math.PI * 2); ctx.fill();
  } else {
    // Standard: short barrel
    ctx.fillStyle = baseColor;
    ctx.beginPath();
  ctx.moveTo(0, -4); ctx.lineTo(14, -2); ctx.lineTo(14, 2); ctx.lineTo(0, 4); ctx.lineTo(-4, 0);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-4, 0); ctx.lineTo(14, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(14, -2); ctx.lineTo(14, 2); ctx.lineTo(0, 4); ctx.lineTo(-4, 0); ctx.closePath(); ctx.stroke();
  // muzzle tip
  ctx.fillStyle = shade(baseColor, 40);
  ctx.beginPath(); ctx.arc(14, 0, 2.5, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
  }

  ctx.restore();

  // tier mutations
  if (tier >= 1) {
    // spikes around base
    ctx.fillStyle = shade(baseColor, 30); ctx.strokeStyle = INK; ctx.lineWidth = 1;
    for (let i = 0; i < 8; i++) {
      const a = (i / 8) * Math.PI * 2 + t * 0.3;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a - 0.1) * 17, Math.sin(a - 0.1) * 17);
      ctx.lineTo(Math.cos(a) * 22, Math.sin(a) * 22);
      ctx.lineTo(Math.cos(a + 0.1) * 17, Math.sin(a + 0.1) * 17);
      ctx.closePath(); ctx.fill(); ctx.stroke();
    }
  }
  if (tier >= 2) {
    // ring
    ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2; ctx.globalAlpha = 0.5;
    ctx.beginPath(); ctx.arc(0, 0, 24, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1;
  }
  if (tier >= 3) {
    // rotating segments
    ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2;
    for (let i = 0; i < 4; i++) {
      const a = t * 2 + (i / 4) * Math.PI * 2;
      ctx.beginPath(); ctx.arc(0, 0, 28, a, a + 0.4); ctx.stroke();
    }
  }
  if (tier >= 4) {
    // core crystal
    ctx.fillStyle = '#d4943d';
    ctx.beginPath();
    ctx.moveTo(0, -5); ctx.lineTo(4, 0); ctx.lineTo(0, 5); ctx.lineTo(-4, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1; ctx.stroke();
  }
  ctx.restore();
}

// ===== Enemy — origami figures =====
// ===== Origami Mouse (normal circle enemy — distinct from boss crane) =====
function drawOrigamiMouse(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(3, 4, r, r * 0.7, 0, 0, Math.PI * 2); ctx.fill(); });
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.9, r * 0.5); ctx.lineTo(-r * 0.9, r * 0.5); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 10);
  ctx.beginPath(); ctx.arc(-r * 0.4, -r * 0.6, r * 0.3, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.4, -r * 0.6, r * 0.3, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = shade(fill, -20);
  ctx.beginPath(); ctx.arc(-r * 0.4, -r * 0.6, r * 0.15, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.4, -r * 0.6, r * 0.15, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -10); ctx.lineWidth = 2; ctx.lineCap = 'round';
  ctx.beginPath(); ctx.moveTo(0, r * 0.5); ctx.quadraticCurveTo(r * 0.6, r * 0.8, r * 0.8, r * 0.4); ctx.stroke();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.8); ctx.lineTo(0, r * 0.5); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.9, r * 0.5); ctx.lineTo(-r * 0.9, r * 0.5); ctx.closePath(); ctx.stroke();
  ctx.beginPath(); ctx.arc(-r * 0.4, -r * 0.6, r * 0.3, 0, Math.PI * 2); ctx.stroke();
  ctx.beginPath(); ctx.arc(r * 0.4, -r * 0.6, r * 0.3, 0, Math.PI * 2); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(-r * 0.2, -r * 0.1, 1.5, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.2, -r * 0.1, 1.5, 0, Math.PI * 2); ctx.fill();
}

// ===== Origami Fish (normal square enemy — distinct from boss boat) =====
function drawOrigamiFish(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(3, 4, r, r * 0.6, 0, 0, Math.PI * 2); ctx.fill(); });
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(r, 0); ctx.lineTo(0, -r * 0.7); ctx.lineTo(-r * 0.8, 0); ctx.lineTo(0, r * 0.7); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, -15);
  ctx.beginPath(); ctx.moveTo(-r * 0.8, 0); ctx.lineTo(-r * 1.3, -r * 0.4); ctx.lineTo(-r * 1.3, r * 0.4); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 10);
  ctx.beginPath(); ctx.moveTo(0, -r * 0.7); ctx.lineTo(-r * 0.2, -r * 1.1); ctx.lineTo(-r * 0.4, -r * 0.6); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.moveTo(r, 0); ctx.lineTo(0, -r * 0.7); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r, 0); ctx.lineTo(-r * 0.8, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(r, 0); ctx.lineTo(0, -r * 0.7); ctx.lineTo(-r * 0.8, 0); ctx.lineTo(0, r * 0.7); ctx.closePath(); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r * 0.8, 0); ctx.lineTo(-r * 1.3, -r * 0.4); ctx.lineTo(-r * 1.3, r * 0.4); ctx.closePath(); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.4, -r * 0.1, 2, 0, Math.PI * 2); ctx.fill();
}

function drawEnemy(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  ctx.save();
  ctx.translate(e.pos.x, e.pos.y);
  ctx.rotate(e.rotation);
  const color = e.hitFlash > 0 ? '#f4ecd8' : e.color;
  const highlight = e.hitFlash > 0 ? '#ffffff' : shade(e.color, 35);
  const frozen = e.freezeTimer > 0;
  const fc = '#6a9ab0', fh = '#8ac0d8';

  if (e.shape === 'triangle') {
    drawOrigamiAirplane(ctx, e.radius, frozen ? fc : color, frozen ? fh : highlight);
  } else if (e.shape === 'square') {
    if (e.type === 'tank') drawOrigamiButterfly(ctx, e.radius, frozen ? fc : color, frozen ? fh : highlight);
    else drawOrigamiFish(ctx, e.radius, frozen ? fc : color, frozen ? fh : highlight);
  } else if (e.shape === 'circle') {
    if (e.type === 'fast') drawOrigamiFrog(ctx, e.radius, frozen ? fc : color, frozen ? fh : highlight);
    else drawOrigamiMouse(ctx, e.radius, frozen ? fc : color, frozen ? fh : highlight);
  } else if (e.shape === 'hexagon') {
    drawBoss(ctx, e);
  }

  if (!e.isBoss && e.tier > 0) drawEnemyTierDetails(ctx, e);

  if (e.isElite) {
    ctx.strokeStyle = '#8a4a8a'; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.5 + Math.sin(Date.now() / 200) * 0.2;
    ctx.setLineDash([4, 3]);
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 8, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }

  if (e.fireTimer > 0) {
    ctx.fillStyle = 'rgba(180,80,30,0.25)';
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 2, 0, Math.PI * 2); ctx.fill();
  }
  if (e.poisonTimer > 0) {
    ctx.fillStyle = 'rgba(90,140,60,0.2)';
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 2, 0, Math.PI * 2); ctx.fill();
  }

  ctx.restore();

  if (e.isBoss) {
    const barW = 80, barH = 6;
    ctx.fillStyle = 'rgba(58,46,31,0.5)';
    ctx.fillRect(e.pos.x - barW / 2, e.pos.y - e.radius - 24, barW, barH);
    ctx.fillStyle = '#c4453d';
    ctx.fillRect(e.pos.x - barW / 2, e.pos.y - e.radius - 24, barW * (e.hp / e.maxHp), barH);
    ctx.strokeStyle = INK; ctx.lineWidth = 1;
    ctx.strokeRect(e.pos.x - barW / 2, e.pos.y - e.radius - 24, barW, barH);
  }
}

function drawEnemyTierDetails(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  const t = Date.now() / 1000;
  if (e.tier >= 1) {
    ctx.fillStyle = shade(e.color, 30);
    for (let i = 0; i < 3; i++) {
      const a = t * 2 + (i / 3) * Math.PI * 2;
      ctx.beginPath(); ctx.arc(Math.cos(a) * (e.radius + 5), Math.sin(a) * (e.radius + 5), 2, 0, Math.PI * 2); ctx.fill();
    }
  }
  if (e.tier >= 2) {
    ctx.strokeStyle = shade(e.color, 20); ctx.lineWidth = 1.5;
    ctx.globalAlpha = 0.4 + Math.sin(t * 4) * 0.15;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 4, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1;
  }
  if (e.tier >= 3) {
    ctx.strokeStyle = shade(e.color, 50); ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.moveTo(-e.radius * 0.5, 0); ctx.lineTo(e.radius * 0.5, 0);
    ctx.moveTo(0, -e.radius * 0.5); ctx.lineTo(0, e.radius * 0.5);
    ctx.stroke();
  }
}

// ===== Boss — beautiful diverse origami figures =====
function drawBoss(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  const t = Date.now() / 1000;
  const tier = e.tier;
  const color = e.freezeTimer > 0 ? '#6a9ab0' : e.color;
  const highlight = e.freezeTimer > 0 ? '#8ac0d8' : shade(e.color, 35);
  const r = e.radius;

  if (e.bossType === 'shooter') {
    drawOrigamiPhoenix(ctx, r, color, highlight, t);
  } else if (e.bossType === 'charger') {
    drawOrigamiDragon(ctx, r, color, highlight, t);
  } else if (e.bossType === 'summoner') {
    drawOrigamiLotus(ctx, r, color, highlight);
  } else if (e.bossType === 'aura') {
    drawOrigamiOctopus(ctx, r, color, highlight, t);
  } else {
    drawOrigamiLotus(ctx, r, color, highlight);
  }

  if (e.bossType === 'aura' && e.auraRadius) {
    ctx.strokeStyle = color; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.2 + Math.sin(t * 2) * 0.08;
    ctx.setLineDash([6, 4]);
    ctx.beginPath(); ctx.arc(0, 0, e.auraRadius, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }
  if (e.isCharging) {
    ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2.5;
    ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(e.chargeDir.x * 60, e.chargeDir.y * 60); ctx.stroke();
  }
  if (e.bossType === 'summoner') {
    ctx.strokeStyle = '#8a4a8a'; ctx.lineWidth = 1.5; ctx.globalAlpha = 0.4;
    for (let i = 0; i < 3; i++) {
      const a = t * 2 + (i / 3) * Math.PI * 2;
      ctx.beginPath(); ctx.arc(Math.cos(a) * 30, Math.sin(a) * 30, 8, 0, Math.PI * 2); ctx.stroke();
    }
    ctx.globalAlpha = 1;
  }

  if (tier >= 1) {
    ctx.fillStyle = shade(color, 30); ctx.strokeStyle = INK; ctx.lineWidth = 1;
    for (let i = 0; i < 6; i++) {
      const a = (i / 6) * Math.PI * 2 + t * 0.5;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a - 0.1) * r, Math.sin(a - 0.1) * r);
      ctx.lineTo(Math.cos(a) * (r + 10), Math.sin(a) * (r + 10));
      ctx.lineTo(Math.cos(a + 0.1) * r, Math.sin(a + 0.1) * r);
      ctx.closePath(); ctx.fill(); ctx.stroke();
    }
  }
  if (tier >= 2) {
    ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.4 + Math.sin(t * 3) * 0.2;
    ctx.beginPath(); ctx.arc(0, 0, r + 14, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1;
  }
  if (tier >= 3) {
    ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2;
    for (let i = 0; i < 8; i++) {
      const a = -t + (i / 8) * Math.PI * 2;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * (r + 18), Math.sin(a) * (r + 18));
      ctx.lineTo(Math.cos(a) * (r + 24), Math.sin(a) * (r + 24));
      ctx.stroke();
    }
  }
}

function drawLightning(ctx: CanvasRenderingContext2D, from: { x: number; y: number }, to: { x: number; y: number }): void {
  const segments = 8;
  ctx.beginPath(); ctx.moveTo(from.x, from.y);
  for (let i = 1; i < segments; i++) {
    const t = i / segments;
    const x = from.x + (to.x - from.x) * t + (Math.random() - 0.5) * 25;
    const y = from.y + (to.y - from.y) * t + (Math.random() - 0.5) * 25;
    ctx.lineTo(x, y);
  }
  ctx.lineTo(to.x, to.y); ctx.stroke();
}

// ===== Chest — paper box =====
function drawChest(ctx: CanvasRenderingContext2D, chest: ChestEntity): void {
  ctx.save();
  ctx.translate(chest.pos.x, chest.pos.y);
  const pulse = 1 + Math.sin(Date.now() / 300) * 0.08;
  ctx.scale(pulse, pulse);
  drawShadow(ctx, () => { ctx.fillRect(-12 + 3, -8 + 4, 24, 16); });
  ctx.fillStyle = '#b8854a'; ctx.fillRect(-12, -8, 24, 16);
  ctx.fillStyle = '#d4a06a'; ctx.fillRect(-12, -12, 24, 8);
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-12, -4); ctx.lineTo(12, -4); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(0, -12); ctx.lineTo(0, 8); ctx.stroke();
  ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2; ctx.strokeRect(-12, -12, 24, 20);
  ctx.fillStyle = '#d4943d'; ctx.fillRect(-3, -4, 6, 6);
  ctx.strokeStyle = INK; ctx.lineWidth = 1; ctx.strokeRect(-3, -4, 6, 6);
  ctx.restore();
}
