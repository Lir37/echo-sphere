import type { GameState, PlayerState, SphereEntity, EnemyEntity, DamageNumber, ChestEntity } from './engine';
import { PLAYER_RADIUS } from './engine';
import { SPHERE_TYPES, BOSS_TYPES } from './gameData';
import type { MapTheme } from './engine';
import { CHARACTER_DEFS } from './characters';
import { getCharacterId, getCharacterFormation, getEngineerNetworkRange } from './characterRuntime';

// ===== Origami / Paper Craft Style =====
// Warm backgrounds, faceted folded-paper shapes, fold lines, drop shadows.

const INK = '#d8e7ff';
const FOLD_LINE = 'rgba(190,215,245,0.18)';
const VOID_BG = '#070d18';
const VOID_PANEL = '#0d1726';

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
    bg: '#07101d', bgDark: '#040912', grid: 'rgba(120,170,220,0.075)', border: 'rgba(100,180,255,0.24)',
    accent: '#39d8ff', textureDots: ['rgba(80,150,220,0.16)', 'rgba(160,200,255,0.07)'],
  },
  bamboo: {
    bg: '#07140f', bgDark: '#040b08', grid: 'rgba(90,210,150,0.07)', border: 'rgba(90,220,170,0.23)',
    accent: '#55e69a', textureDots: ['rgba(80,200,140,0.14)', 'rgba(170,255,210,0.06)'],
  },
  ocean: {
    bg: '#06131a', bgDark: '#030a10', grid: 'rgba(70,190,240,0.075)', border: 'rgba(70,210,255,0.24)',
    accent: '#54dfff', textureDots: ['rgba(60,180,230,0.15)', 'rgba(160,230,255,0.06)'],
  },
  sunset: {
    bg: '#160b12', bgDark: '#0b050a', grid: 'rgba(240,110,170,0.075)', border: 'rgba(240,120,180,0.24)',
    accent: '#ff6da8', textureDots: ['rgba(230,90,150,0.15)', 'rgba(255,180,210,0.06)'],
  },
};

export function render(ctx: CanvasRenderingContext2D, s: GameState, canvasW: number, canvasH: number): void {
  const theme = THEMES[s.mapTheme] || THEMES.parchment;

  // ===== Base background =====
  ctx.setTransform(1, 0, 0, 1, 0, 0);
  const bg = ctx.createRadialGradient(canvasW * 0.5, canvasH * 0.46, 0, canvasW * 0.5, canvasH * 0.46, Math.max(canvasW, canvasH) * 0.72);
  bg.addColorStop(0, theme.bg);
  bg.addColorStop(0.68, theme.bgDark);
  bg.addColorStop(1, '#02050b');
  ctx.fillStyle = bg;
  ctx.fillRect(0, 0, canvasW, canvasH);
  drawVoidAmbient(ctx, canvasW, canvasH, s.time);

  // ===== World space =====
  ctx.save();

  let shakeX = 0, shakeY = 0;
  if (s.screenShake > 0) {
    shakeX = (Math.random() - 0.5) * s.screenShake * 20;
    shakeY = (Math.random() - 0.5) * s.screenShake * 20;
  }
  ctx.translate(canvasW / 2 - s.camera.x + shakeX, canvasH / 2 - s.camera.y + shakeY);

  drawVoidField(ctx, s.worldWidth, s.worldHeight, theme, -s.worldWidth / 2, -s.worldHeight / 2);
  drawGrid(ctx, s, canvasW, canvasH, theme);

  // world bounds
  ctx.strokeStyle = theme.border;
  ctx.shadowColor = `rgba(${hexToRgb(theme.accent)},0.22)`;
  ctx.shadowBlur = 10;
  ctx.lineWidth = 1.5;
  ctx.setLineDash([10, 8]);
  ctx.strokeRect(-s.worldWidth / 2, -s.worldHeight / 2, s.worldWidth, s.worldHeight);
  ctx.setLineDash([]);
  ctx.shadowBlur = 0;

  // character-specific world indicators
  drawCharacterWorldIndicators(ctx, s);

  // fire trails
  for (const ft of s.fireTrails) {
    const alpha = ft.life / ft.maxLife;
    ctx.fillStyle = `rgba(180,100,40,${alpha * 0.35})`;
    ctx.beginPath(); ctx.arc(ft.pos.x, ft.pos.y, 28, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = `rgba(120,60,20,${alpha * 0.2})`;
    ctx.beginPath(); ctx.arc(ft.pos.x, ft.pos.y, 18, 0, Math.PI * 2); ctx.fill();
  }

  // pickups
  for (const orb of s.xpOrbs) drawModernXp(ctx, orb.pos.x, orb.pos.y, orb.radius, '#63e6ff');
  for (const hp of s.healthPacks) drawModernHealth(ctx, hp.pos.x, hp.pos.y, '#ff5c72');

  // chests
  for (const chest of s.chests) if (chest.alive) drawChest(ctx, chest);

  // spheres
  for (const sphere of s.spheres) drawModernSphere(ctx, s, sphere);

  // sphere projectiles
  for (const p of s.sphereProjectiles) drawModernProjectile(ctx, p.pos.x, p.pos.y, p.vel.x, p.vel.y, p.radius, p.color);

  // minions
  for (const m of s.minions) drawModernMinion(ctx, m.pos.x, m.pos.y, m.radius, m.rotation, '#ffb84d');

  // enemies
  for (const e of s.enemies) drawModernEnemy(ctx, e);

  // hunter mark and alchemist reaction indicators sit above enemies
  drawCharacterTargetIndicators(ctx, s);

  // boss projectiles
  for (const e of s.enemies) for (const bp of e.bossProjectiles) drawPaperDiamond(ctx, bp.pos.x, bp.pos.y, bp.radius, '#c4453d', '#e06b63');

  // Legacy fox/wolf player body is intentionally disabled. The mobile overlay owns the character visual.
  drawPlayer(ctx, s.player);

  // particles
  for (const p of s.particles) {
    const alpha = p.life / p.maxLife;
    const rgb = hexToRgb(p.color);
    ctx.fillStyle = `rgba(${rgb},${alpha * 0.9})`;
    ctx.shadowColor = p.color;
    ctx.shadowBlur = 10;
    ctx.save(); ctx.translate(p.pos.x, p.pos.y);
    ctx.rotate(p.pos.x * 0.01 + Date.now() * 0.003);
    const s2 = p.size * (0.7 + alpha * 0.8);
    ctx.fillRect(-s2, -s2, s2 * 2, s2 * 2);
    ctx.restore();
    ctx.shadowBlur = 0;
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
    const a = Math.max(0, Math.min(1, l.life / 0.3));
    drawEnergyBolt(ctx, l.from, l.to, a);
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

  // character HUD (screen space)
  drawCharacterHud(ctx, s, canvasW, canvasH);

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

function drawCharacterHud(ctx: CanvasRenderingContext2D, s: GameState, canvasW: number, _canvasH: number): void {
  const characterId = getCharacterId(s);
  const def = CHARACTER_DEFS[characterId];
  const mastery = s.player.characterMasteryLevel || 1;
  const x = 12;
  const y = 78;
  const width = Math.min(210, Math.max(165, canvasW * 0.34));
  const lineH = 14;
  const lines: string[] = [];

  switch (characterId) {
    case 'spherist': {
      const afterFirst = Math.max(0, s.spheres.length - 1);
      lines.push(`RESONANCE  +${Math.round(afterFirst * 3 + (mastery >= 4 ? afterFirst * 0.5 : 0))}% AS`);
      if (s.spheres.length >= (mastery >= 2 ? 4 : 5)) lines.push(`CHORUS  +${mastery >= 3 ? 7 : 5}% DMG`);
      break;
    }
    case 'hunter': {
      const marked = s.player.hunterMarkTarget !== null && s.player.hunterMarkTimer > 0;
      const hunt = s.player.hunterHuntTimer > 0;
      if (hunt) lines.push(`HUNT  ${Math.ceil(s.player.hunterHuntTimer)}s  +30%`);
      else if (marked) lines.push(`MARK  ${Math.ceil(s.player.hunterMarkTimer)}s  ${s.player.hunterHitCount}/5`);
      else lines.push('MARK  —');
      if (s.player.hunterTrophyTimer > 0) lines.push(`TROPHY  ${Math.ceil(s.player.hunterTrophyTimer)}s`);
      break;
    }
    case 'engineer': {
      const range = getEngineerNetworkRange(s);
      const linked = s.spheres.filter((sphere) => s.spheres.some((other) => other !== sphere && other.alive && sphere.alive && dist2D(sphere.pos, other.pos) <= range)).length;
      const formationSize = getEngineerFormationSize(s, range);
      lines.push(`LINKS  ${linked}/${s.spheres.length}`);
      if (formationSize >= 3) lines.push(`NETWORK  ${formationSize}`);
      if (s.player.engineerRelayTimer > 0) lines.push(`RELAY  ${s.player.engineerRelayTimer.toFixed(1)}s`);
      break;
    }
    case 'berserker': {
      const missing = Math.max(0, 1 - s.player.hp / Math.max(1, s.player.maxHp));
      const steps = Math.min(4, Math.floor(missing / 0.2));
      lines.push(`FURY  ${steps}/4  +${steps * 7}% DMG`);
      lines.push(`CLOSE  ${hasCloseEnemy(s) ? '+12% DMG' : 'READY'}`);
      if (mastery >= 5 && s.player.buffTimer > 0) lines.push(`BLOOD TRAIL  ${Math.ceil(s.player.buffTimer)}s`);
      break;
    }
    case 'alchemist': {
      const reaction = s.enemies.some((enemy) => enemy.hp > 0 && countStatusEffects(enemy) >= 2);
      lines.push(reaction ? 'REACTION  READY' : 'REACTION  —');
      if (s.player.alchemistCatalystTimer > 0) lines.push(`CATALYST  ${s.player.alchemistCatalystTimer.toFixed(1)}s`);
      break;
    }
    case 'architect': {
      const formation = getCharacterFormation(s);
      lines.push(`FORM  ${formation.type.toUpperCase()}`);
      lines.push(`STRENGTH  ${Math.round(formation.strength * 100)}%`);
      break;
    }
  }

  ctx.save();
  ctx.fillStyle = 'rgba(232,220,192,0.88)';
  ctx.strokeStyle = 'rgba(58,46,31,0.2)';
  ctx.lineWidth = 1;
  const height = 30 + lines.length * lineH;
  ctx.beginPath();
  ctx.roundRect(x, y, width, height, 8);
  ctx.fill();
  ctx.stroke();

  ctx.fillStyle = def.color;
  ctx.font = 'bold 11px Georgia, serif';
  ctx.textAlign = 'left';
  ctx.fillText(`${def.name.en.toUpperCase()}  •  M${mastery}`, x + 9, y + 15);

  ctx.fillStyle = '#5a4a32';
  ctx.font = '10px Georgia, serif';
  lines.forEach((line, index) => {
    ctx.fillText(line, x + 9, y + 29 + index * lineH);
  });
  ctx.restore();
}

function drawCharacterWorldIndicators(ctx: CanvasRenderingContext2D, s: GameState): void {
  const characterId = getCharacterId(s);
  if (characterId === 'engineer') drawEngineerLinks(ctx, s);
  if (characterId === 'architect') drawArchitectFormation(ctx, s);
  if (characterId === 'berserker') drawBerserkerRange(ctx, s);
}

function drawCharacterTargetIndicators(ctx: CanvasRenderingContext2D, s: GameState): void {
  const characterId = getCharacterId(s);
  if (characterId === 'hunter') drawHunterTarget(ctx, s);
  if (characterId === 'alchemist') drawAlchemistReactions(ctx, s);
}

function drawEngineerLinks(ctx: CanvasRenderingContext2D, s: GameState): void {
  const range = getEngineerNetworkRange(s);
  ctx.save();
  ctx.strokeStyle = 'rgba(74,122,138,0.42)';
  ctx.lineWidth = 1.5;
  ctx.setLineDash([5, 4]);
  for (let i = 0; i < s.spheres.length; i++) {
    const a = s.spheres[i];
    if (!a.alive) continue;
    for (let j = i + 1; j < s.spheres.length; j++) {
      const b = s.spheres[j];
      if (!b.alive || dist2D(a.pos, b.pos) > range) continue;
      ctx.beginPath();
      ctx.moveTo(a.pos.x, a.pos.y);
      ctx.lineTo(b.pos.x, b.pos.y);
      ctx.stroke();
    }
  }
  ctx.setLineDash([]);
  ctx.restore();
}

function drawArchitectFormation(ctx: CanvasRenderingContext2D, s: GameState): void {
  const result = getCharacterFormation(s);
  if (result.type === 'none') return;
  const spheres = s.spheres.filter((sphere) => sphere.alive).slice(0, 8);
  ctx.save();
  ctx.strokeStyle = 'rgba(212,148,61,0.55)';
  ctx.fillStyle = 'rgba(212,148,61,0.08)';
  ctx.lineWidth = 2;
  ctx.setLineDash([8, 5]);

  if (result.type === 'line') {
    const ordered = [...spheres].sort((a, b) => a.pos.x === b.pos.x ? a.pos.y - b.pos.y : a.pos.x - b.pos.x);
    drawPolyline(ctx, ordered);
  } else if (result.type === 'triangle') {
    drawPolygon(ctx, spheres.slice(0, 3));
  } else if (result.type === 'square') {
    const center = spheres.reduce((acc, sphere) => ({ x: acc.x + sphere.pos.x, y: acc.y + sphere.pos.y }), { x: 0, y: 0 });
    center.x /= spheres.length; center.y /= spheres.length;
    const ordered = [...spheres.slice(0, 4)].sort((a, b) => Math.atan2(a.pos.y - center.y, a.pos.x - center.x) - Math.atan2(b.pos.y - center.y, b.pos.x - center.x));
    drawPolygon(ctx, ordered);
  } else if (result.type === 'cluster') {
    for (let i = 0; i < spheres.length; i++) {
      const a = spheres[i];
      if (dist2D(a.pos, s.player.pos) > 220) continue;
      ctx.beginPath(); ctx.arc(a.pos.x, a.pos.y, 20, 0, Math.PI * 2); ctx.stroke();
    }
  }
  ctx.setLineDash([]);
  ctx.restore();
}

function drawHunterTarget(ctx: CanvasRenderingContext2D, s: GameState): void {
  const target = s.player.hunterMarkTarget;
  if (target && target.hp > 0 && s.player.hunterMarkTimer > 0) {
    ctx.save();
    const hunt = s.player.hunterHuntTarget === target && s.player.hunterHuntTimer > 0;
    ctx.strokeStyle = hunt ? '#d4943d' : '#c46d3d';
    ctx.lineWidth = hunt ? 3 : 2;
    ctx.setLineDash(hunt ? [10, 5] : [6, 4]);
    ctx.beginPath(); ctx.arc(target.pos.x, target.pos.y, target.radius + (hunt ? 14 : 9), 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]);
    ctx.fillStyle = hunt ? '#d4943d' : '#c46d3d';
    ctx.font = 'bold 10px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText(hunt ? 'HUNT' : 'MARK', target.pos.x, target.pos.y - target.radius - 12);
    ctx.restore();
  }
}

function drawAlchemistReactions(ctx: CanvasRenderingContext2D, s: GameState): void {
  for (const enemy of s.enemies) {
    if (enemy.hp <= 0) continue;
    const count = countStatusEffects(enemy);
    if (count < 2) continue;
    let symbol = '✦';
    if (enemy.fireTimer > 0 && enemy.poisonTimer > 0) symbol = 'TP';
    else if (enemy.freezeTimer > 0 && enemy.poisonTimer > 0) symbol = 'CP';
    else if (enemy.fireTimer > 0 && enemy.freezeTimer > 0) symbol = 'TS';
    ctx.save();
    ctx.fillStyle = '#8a5a8a';
    ctx.font = 'bold 10px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText(symbol, enemy.pos.x, enemy.pos.y - enemy.radius - 6);
    ctx.restore();
  }
}

function drawBerserkerRange(ctx: CanvasRenderingContext2D, s: GameState): void {
  ctx.save();
  const missing = Math.max(0, 1 - s.player.hp / Math.max(1, s.player.maxHp));
  const steps = Math.min(4, Math.floor(missing / 0.2));
  ctx.strokeStyle = steps > 0 ? 'rgba(196,69,61,0.22)' : 'rgba(196,69,61,0.10)';
  ctx.lineWidth = steps > 0 ? 2 : 1;
  ctx.setLineDash([6, 5]);
  ctx.beginPath(); ctx.arc(s.player.pos.x, s.player.pos.y, 110, 0, Math.PI * 2); ctx.stroke();
  ctx.setLineDash([]);
  ctx.restore();
}

function getEngineerFormationSize(s: GameState, range: number): number {
  let best = 0;
  for (const start of s.spheres) {
    if (!start.alive) continue;
    const visited = new Set<SphereEntity>([start]);
    const queue: SphereEntity[] = [start];
    while (queue.length > 0) {
      const current = queue.shift()!;
      for (const neighbour of s.spheres) {
        if (!neighbour.alive || visited.has(neighbour)) continue;
        if (dist2D(current.pos, neighbour.pos) <= range) {
          visited.add(neighbour);
          queue.push(neighbour);
        }
      }
    }
    best = Math.max(best, visited.size);
  }
  return best;
}

function countStatusEffects(enemy: EnemyEntity): number {
  return Number(enemy.fireTimer > 0) + Number(enemy.freezeTimer > 0) + Number(enemy.poisonTimer > 0);
}

function hasCloseEnemy(s: GameState): boolean {
  return s.enemies.some((enemy) => enemy.hp > 0 && dist2D(enemy.pos, s.player.pos) <= 110);
}

function dist2D(a: { x: number; y: number }, b: { x: number; y: number }): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function drawPolyline(ctx: CanvasRenderingContext2D, spheres: SphereEntity[]): void {
  if (spheres.length < 2) return;
  ctx.beginPath();
  spheres.forEach((sphere, index) => index === 0 ? ctx.moveTo(sphere.pos.x, sphere.pos.y) : ctx.lineTo(sphere.pos.x, sphere.pos.y));
  ctx.stroke();
}

function drawPolygon(ctx: CanvasRenderingContext2D, spheres: SphereEntity[]): void {
  if (spheres.length < 3) return;
  ctx.beginPath();
  spheres.forEach((sphere, index) => index === 0 ? ctx.moveTo(sphere.pos.x, sphere.pos.y) : ctx.lineTo(sphere.pos.x, sphere.pos.y));
  ctx.closePath();
  ctx.stroke();
  ctx.fill();
}


function drawVoidAmbient(ctx: CanvasRenderingContext2D, w: number, h: number, time: number): void {
  ctx.save();
  const count = Math.min(90, Math.floor((w * h) / 11000));
  for (let i = 0; i < count; i++) {
    const x = ((i * 97.13) % w);
    const y = ((i * 53.71 + time * (2 + (i % 3))) % h);
    const pulse = 0.28 + 0.22 * Math.sin(time * 1.4 + i);
    ctx.fillStyle = `rgba(120,190,255,${pulse * 0.16})`;
    ctx.beginPath();
    ctx.arc(x, y, 0.7 + (i % 3) * 0.35, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.restore();
}

function drawVoidField(ctx: CanvasRenderingContext2D, w: number, h: number, theme: Theme, ox = 0, oy = 0): void {
  const tileKey = `void-${theme.bg}`;
  if (!_textureCanvases[tileKey as MapTheme]) {
    const tc = document.createElement('canvas');
    tc.width = 320; tc.height = 320;
    const tctx = tc.getContext('2d')!;
    tctx.fillStyle = theme.bg;
    tctx.fillRect(0, 0, 320, 320);
    for (let i = 0; i < 120; i++) {
      const x = (i * 73.17) % 320;
      const y = (i * 127.41) % 320;
      const r = 0.35 + (i % 4) * 0.25;
      tctx.fillStyle = `rgba(150,205,255,${0.03 + (i % 5) * 0.012})`;
      tctx.beginPath(); tctx.arc(x, y, r, 0, Math.PI * 2); tctx.fill();
    }
    for (let i = 0; i <= 4; i++) {
      const inset = 8 + i * 64;
      tctx.strokeStyle = `rgba(${hexToRgb(theme.accent)},${0.018 + i * 0.008})`;
      tctx.lineWidth = 1;
      tctx.strokeRect(inset, inset, 320 - inset * 2, 320 - inset * 2);
    }
    _textureCanvases[tileKey as MapTheme] = tc;
  }
  const tile = _textureCanvases[tileKey as MapTheme]!;
  for (let x = -256; x < w + 256; x += 320) {
    for (let y = -256; y < h + 256; y += 320) {
      ctx.drawImage(tile, x + ox, y + oy);
    }
  }
}

function drawModernXp(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, color: string): void {
  ctx.save(); ctx.translate(x, y);
  glowCircle(ctx, r * 3.5, color, 0.16);
  ctx.rotate(Math.PI / 4);
  ctx.fillStyle = color;
  ctx.globalAlpha = 0.9;
  ctx.fillRect(-r * 0.65, -r * 0.65, r * 1.3, r * 1.3);
  ctx.globalAlpha = 1;
  ctx.strokeStyle = '#dffaff'; ctx.lineWidth = Math.max(0.8, r * 0.16);
  ctx.strokeRect(-r * 0.65, -r * 0.65, r * 1.3, r * 1.3);
  ctx.restore();
}

function drawModernHealth(ctx: CanvasRenderingContext2D, x: number, y: number, color: string): void {
  ctx.save(); ctx.translate(x, y);
  glowCircle(ctx, 22, color, 0.10);
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.moveTo(-4, -11); ctx.lineTo(4, -11); ctx.lineTo(4, -4); ctx.lineTo(11, -4);
  ctx.lineTo(11, 4); ctx.lineTo(4, 4); ctx.lineTo(4, 11); ctx.lineTo(-4, 11);
  ctx.lineTo(-4, 4); ctx.lineTo(-11, 4); ctx.lineTo(-11, -4); ctx.lineTo(-4, -4); ctx.closePath();
  ctx.fill();
  ctx.strokeStyle = '#ffeaf0'; ctx.lineWidth = 1;
  ctx.stroke();
  ctx.restore();
}

function drawModernProjectile(ctx: CanvasRenderingContext2D, x: number, y: number, vx: number, vy: number, r: number, color: string): void {
  const angle = Math.atan2(vy, vx);
  const speed = Math.hypot(vx, vy) || 1;
  const trail = Math.min(28, 8 + speed * 0.045);
  ctx.save();
  ctx.translate(x, y); ctx.rotate(angle);
  const rgb = hexToRgb(color);
  ctx.strokeStyle = `rgba(${rgb},0.25)`;
  ctx.lineWidth = Math.max(1, r * 0.7);
  ctx.shadowColor = color; ctx.shadowBlur = 12;
  ctx.beginPath(); ctx.moveTo(-trail, 0); ctx.lineTo(-r, 0); ctx.stroke();
  ctx.fillStyle = '#ffffff';
  ctx.beginPath(); ctx.ellipse(0, 0, r * 1.15, r * 0.55, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.moveTo(r * 1.8, 0); ctx.lineTo(-r * 0.8, -r * 0.72); ctx.lineTo(-r * 0.35, 0); ctx.lineTo(-r * 0.8, r * 0.72); ctx.closePath(); ctx.fill();
  ctx.shadowBlur = 0; ctx.restore();
}

function drawModernMinion(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, rotation: number, color: string): void {
  ctx.save(); ctx.translate(x, y); ctx.rotate(rotation);
  glowCircle(ctx, r * 2.5, color, 0.14);
  ctx.fillStyle = '#070d18';
  ctx.strokeStyle = color; ctx.lineWidth = Math.max(1.2, r * 0.12);
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r * 0.9, 0); ctx.lineTo(0, r); ctx.lineTo(-r * 0.9, 0); ctx.closePath();
  ctx.fill(); ctx.stroke();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.arc(r * 0.18, -r * 0.18, r * 0.22, 0, Math.PI * 2); ctx.fill();
  ctx.restore();
}

function drawEnergyBolt(ctx: CanvasRenderingContext2D, from: Vec, to: Vec, alpha: number): void {
  const dx = to.x - from.x, dy = to.y - from.y;
  const len = Math.hypot(dx, dy) || 1;
  const nx = -dy / len, ny = dx / len;
  const points = 7;
  const wobble = Math.min(26, len * 0.08);
  ctx.save();
  ctx.lineCap = 'round';
  ctx.shadowColor = '#6eeaff';
  ctx.shadowBlur = 16;
  ctx.strokeStyle = `rgba(105,235,255,${0.22 * alpha})`;
  ctx.lineWidth = 6;
  ctx.beginPath(); ctx.moveTo(from.x, from.y);
  for (let i = 1; i < points; i++) {
    const p = i / points;
    const wave = Math.sin(i * 3.7 + Date.now() * 0.03) * wobble * 0.35;
    ctx.lineTo(from.x + dx * p + nx * wave, from.y + dy * p + ny * wave);
  }
  ctx.lineTo(to.x, to.y); ctx.stroke();
  ctx.shadowBlur = 0;
  ctx.strokeStyle = `rgba(235,252,255,${0.9 * alpha})`;
  ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(from.x, from.y);
  for (let i = 1; i < points; i++) {
    const p = i / points;
    const wave = Math.sin(i * 3.7 + Date.now() * 0.03) * wobble * 0.35;
    ctx.lineTo(from.x + dx * p + nx * wave, from.y + dy * p + ny * wave);
  }
  ctx.lineTo(to.x, to.y); ctx.stroke();
  ctx.restore();
}

// ===== Paper texture (world space) =====
let _textureCanvases: Partial<Record<MapTheme, HTMLCanvasElement>> = {};
function drawPaperTexture(ctx: CanvasRenderingContext2D, w: number, h: number, theme: Theme, offsetX = 0, offsetY = 0): void {
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
  for (let x = 0; x < w; x += 256) {
    for (let y = 0; y < h; y += 256) {
      ctx.drawImage(tile, x + offsetX, y + offsetY);
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
  const speed = Math.hypot(vx, vy) || 1;
  const trail = Math.min(18, 7 + speed * 0.035);
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle);
  ctx.strokeStyle = `rgba(${hexToRgb(color)},0.28)`;
  ctx.lineWidth = Math.max(1, r * 0.45);
  ctx.beginPath();
  ctx.moveTo(-trail, 0);
  ctx.lineTo(-r * 0.7, 0);
  ctx.stroke();
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
function drawOrigamiAirplane(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const flap = Math.sin(t * 12) * 0.16;
  const bob = Math.sin(t * 6) * r * 0.08;
  ctx.save(); ctx.translate(bob, Math.sin(t * 8) * r * 0.035);
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(4, 6, r * 0.62, r * 0.34, 0, 0, Math.PI * 2); ctx.fill(); });

  // Folded moth: the body points forward, while wings flap around it.
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.55, -r * 0.18); ctx.lineTo(r * 0.7, 0); ctx.lineTo(-r * 0.55, r * 0.18); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.4; ctx.stroke();

  const wingLift = flap * r;
  ctx.fillStyle = shade(fill, 18); ctx.globalAlpha = 0.95;
  ctx.beginPath(); ctx.moveTo(-r * 0.08, -r * 0.1); ctx.lineTo(-r * 0.55, -r * 0.82 - wingLift); ctx.lineTo(r * 0.48, -r * 0.55 - wingLift * 0.55); ctx.lineTo(r * 0.3, -r * 0.08); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(-r * 0.08, r * 0.1); ctx.lineTo(-r * 0.55, r * 0.82 + wingLift); ctx.lineTo(r * 0.48, r * 0.55 + wingLift * 0.55); ctx.lineTo(r * 0.3, r * 0.08); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.1, 0); ctx.lineTo(r * 0.65, 0); ctx.moveTo(0, -r * 0.12); ctx.lineTo(-r * 0.46, -r * 0.62 - wingLift); ctx.moveTo(0, r * 0.12); ctx.lineTo(-r * 0.46, r * 0.62 + wingLift); ctx.stroke();

  ctx.strokeStyle = shade(fill, -25); ctx.lineWidth = Math.max(1.2, r * 0.08); ctx.lineCap = 'round';
  const leg = Math.sin(t * 12) * r * 0.08;
  ctx.beginPath(); ctx.moveTo(-r * 0.12, -r * 0.12); ctx.lineTo(-r * 0.3, -r * 0.46 + leg); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r * 0.12, r * 0.12); ctx.lineTo(-r * 0.3, r * 0.46 - leg); ctx.stroke();
  ctx.lineCap = 'butt';

  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.44, -r * 0.08, Math.max(1.6, r * 0.08), 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.44, r * 0.08, Math.max(1.6, r * 0.08), 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -22); ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r * 0.62, -r * 0.08); ctx.lineTo(r * 0.95, -r * 0.24); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.62, r * 0.08); ctx.lineTo(r * 0.95, r * 0.24); ctx.stroke();
  ctx.restore();
}


function drawOrigamiBoat(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  drawShadow(ctx, () => {
    ctx.beginPath();
    ctx.moveTo(-r + 3, -r * 0.3 + 4); ctx.lineTo(r + 3, -r * 0.3 + 4); ctx.lineTo(r * 0.6 + 3, r + 4); ctx.lineTo(-r * 0.6 + 3, r + 4);
    ctx.closePath(); ctx.fill();
  });
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(-r, -r * 0.3); ctx.lineTo(r, -r * 0.3); ctx.lineTo(r * 0.6, r); ctx.lineTo(-r * 0.6, r);
  ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 0.8, -r * 0.3); ctx.lineTo(0, -r * 1.2);
  ctx.closePath(); ctx.fill();
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(-r * 0.8, -r * 0.3); ctx.lineTo(0, -r * 1.2);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 1.2); ctx.lineTo(0, r); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r, -r * 0.3); ctx.lineTo(r, -r * 0.3); ctx.stroke();
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
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r * 0.3, -r * 0.2); ctx.lineTo(r, r * 0.5); ctx.lineTo(0, r);
  ctx.closePath(); ctx.fill();
  ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r); ctx.lineTo(0, r); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r, r * 0.5); ctx.lineTo(r, r * 0.5); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(0, -r); ctx.lineTo(r * 0.3, -r * 0.2); ctx.lineTo(r, r * 0.5); ctx.lineTo(r * 0.2, r * 0.3);
  ctx.lineTo(0, r); ctx.lineTo(-r * 0.2, r * 0.3); ctx.lineTo(-r, r * 0.5); ctx.lineTo(-r * 0.3, -r * 0.2);
  ctx.closePath(); ctx.stroke();
}

// ===== Origami Frog (fast triangle enemy variant) =====
function drawOrigamiFrog(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const walk = t * 13;
  const crouch = Math.sin(t * 6.5) * r * 0.04;
  ctx.save(); ctx.translate(Math.sin(t * 13) * r * 0.03, crouch);
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(4, 6, r * 0.8, r * 0.46, 0, 0, Math.PI * 2); ctx.fill(); });

  // Fast folded mantis: long legs make the speed readable.
  ctx.strokeStyle = shade(fill, -28); ctx.lineWidth = Math.max(1.5, r * 0.1); ctx.lineCap = 'round';
  for (let i = 0; i < 2; i++) {
    const phase = walk + i * Math.PI;
    const swing = Math.sin(phase) * r * 0.22;
    const x = -r * 0.22 + i * r * 0.42;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.15); ctx.lineTo(x + r * 0.28, -r * 0.66 + swing); ctx.lineTo(x + r * 0.58, -r * 0.42 + swing); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.15); ctx.lineTo(x + r * 0.42, r * 0.7 - swing); ctx.lineTo(x + r * 0.75, r * 0.4 - swing); ctx.stroke();
  }
  ctx.lineCap = 'butt';
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.68, -r * 0.34); ctx.lineTo(r * 0.5, -r * 0.5); ctx.lineTo(r * 0.82, 0); ctx.lineTo(r * 0.5, r * 0.5); ctx.lineTo(-r * 0.68, r * 0.34); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 22);
  ctx.beginPath(); ctx.moveTo(-r * 0.25, -r * 0.34); ctx.lineTo(r * 0.5, -r * 0.5); ctx.lineTo(r * 0.2, 0); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.5;
  ctx.beginPath(); ctx.moveTo(r * 0.15, -r * 0.42); ctx.lineTo(r * 0.58, -r * 0.12); ctx.lineTo(r * 0.1, -r * 0.02); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1; ctx.beginPath(); ctx.moveTo(-r * 0.45, 0); ctx.lineTo(r * 0.62, 0); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(-r * 0.68, -r * 0.34); ctx.lineTo(r * 0.5, -r * 0.5); ctx.lineTo(r * 0.82, 0); ctx.lineTo(r * 0.5, r * 0.5); ctx.lineTo(-r * 0.68, r * 0.34); ctx.closePath(); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.56, -r * 0.11, Math.max(2, r * 0.1), 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.56, r * 0.11, Math.max(2, r * 0.1), 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -20); ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r * 0.82, -r * 0.05); ctx.lineTo(r * 1.08, -r * 0.18); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.82, r * 0.05); ctx.lineTo(r * 1.08, r * 0.18); ctx.stroke();
  ctx.restore();
}


function drawOrigamiButterfly(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const walk = t * 9;
  const bob = Math.sin(t * 9) * r * 0.025;
  ctx.save(); ctx.translate(0, bob);
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(4, 7, r * 1.05, r * 0.52, 0, 0, Math.PI * 2); ctx.fill(); });

  // Heavy armored insect with broad folded shell and eight short legs.
  ctx.strokeStyle = shade(fill, -32); ctx.lineWidth = Math.max(1.8, r * 0.095); ctx.lineCap = 'round';
  for (let i = 0; i < 4; i++) {
    const phase = walk + i * 1.55;
    const swing = Math.sin(phase) * r * 0.12;
    const x = -r * 0.58 + i * r * 0.38;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.24); ctx.lineTo(x - r * 0.2, -r * 0.56 - swing); ctx.lineTo(x + r * 0.03, -r * 0.82 - swing); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.24); ctx.lineTo(x + r * 0.2, r * 0.56 + swing); ctx.lineTo(x + r * 0.03, r * 0.82 + swing); ctx.stroke();
  }
  ctx.lineCap = 'butt';

  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.86, -r * 0.48); ctx.lineTo(r * 0.54, -r * 0.58); ctx.lineTo(r * 0.92, -r * 0.2); ctx.lineTo(r * 0.92, r * 0.2); ctx.lineTo(r * 0.54, r * 0.58); ctx.lineTo(-r * 0.86, r * 0.48); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 20);
  ctx.beginPath(); ctx.moveTo(-r * 0.65, -r * 0.43); ctx.lineTo(-r * 0.05, -r * 0.52); ctx.lineTo(r * 0.15, -r * 0.02); ctx.lineTo(-r * 0.15, r * 0.02); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, -22);
  ctx.beginPath(); ctx.moveTo(-r * 0.02, -r * 0.48); ctx.lineTo(r * 0.55, -r * 0.58); ctx.lineTo(r * 0.86, -r * 0.16); ctx.lineTo(r * 0.18, -r * 0.05); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.38;
  ctx.beginPath(); ctx.moveTo(-r * 0.58, -r * 0.36); ctx.lineTo(r * 0.48, -r * 0.45); ctx.lineTo(r * 0.58, -r * 0.2); ctx.lineTo(-r * 0.05, -r * 0.08); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.55, 0); ctx.lineTo(r * 0.75, 0); ctx.moveTo(r * 0.08, -r * 0.52); ctx.lineTo(r * 0.08, r * 0.52); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.7;
  ctx.beginPath(); ctx.moveTo(-r * 0.86, -r * 0.48); ctx.lineTo(r * 0.54, -r * 0.58); ctx.lineTo(r * 0.92, -r * 0.2); ctx.lineTo(r * 0.92, r * 0.2); ctx.lineTo(r * 0.54, r * 0.58); ctx.lineTo(-r * 0.86, r * 0.48); ctx.closePath(); ctx.stroke();

  ctx.fillStyle = shade(fill, 26);
  ctx.beginPath(); ctx.moveTo(r * 0.72, -r * 0.32); ctx.lineTo(r * 1.12, -r * 0.46); ctx.lineTo(r * 0.96, -r * 0.08); ctx.closePath(); ctx.fill(); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.74, -r * 0.12, Math.max(2, r * 0.08), 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.74, r * 0.12, Math.max(2, r * 0.08), 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -22); ctx.lineWidth = 1.2;
  const feeler = Math.sin(t * 5) * r * 0.05;
  ctx.beginPath(); ctx.moveTo(r * 0.85, -r * 0.16); ctx.lineTo(r * 1.18, -r * 0.34 + feeler); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.85, r * 0.16); ctx.lineTo(r * 1.18, r * 0.34 - feeler); ctx.stroke();
  ctx.restore();
}


// ===== Origami Lotus (boss shapes) =====

function drawOrigamiLotus(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string): void {
  const petals = 8;
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.arc(4, 5, r * 1.1, 0, Math.PI * 2); ctx.fill();
  });
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
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(0, 0, r * 0.12, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiDragon(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.ellipse(4, 5, r * 1.2, r * 0.8, 0, 0, Math.PI * 2); ctx.fill();
  });
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
  const headX = Math.sin(t * 2) * r * 0.6;
  const headY = -r * 0.7;
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(headX, headY - r * 0.5); ctx.lineTo(headX + r * 0.6, headY); ctx.lineTo(headX, headY + r * 0.3); ctx.lineTo(headX - r * 0.6, headY);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(headX - r * 0.2, headY - r * 0.1, 2, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(headX + r * 0.2, headY - r * 0.1, 2, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiPhoenix(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.ellipse(4, 5, r * 1.3, r, 0, 0, Math.PI * 2); ctx.fill();
  });
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
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.5, 0); ctx.lineTo(0, r * 0.5); ctx.lineTo(-r * 0.5, 0);
  ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, -10);
  const flap = Math.sin(t * 4) * 0.2;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(r * 1.3, -r * 0.3 + flap * r); ctx.lineTo(r * 0.8, r * 0.2); ctx.lineTo(0, 0);
  ctx.closePath(); ctx.fill();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.3); ctx.lineTo(-r * 1.3, -r * 0.3 + flap * r); ctx.lineTo(-r * 0.8, r * 0.2); ctx.lineTo(0, 0);
  ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.3, -r * 1.1); ctx.lineTo(-r * 0.3, -r * 1.1);
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.8); ctx.lineTo(0, r * 0.5); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.5, 0); ctx.lineTo(0, r * 0.5); ctx.lineTo(-r * 0.5, 0); ctx.closePath(); ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, -r * 0.8); ctx.lineTo(r * 0.3, -r * 1.1); ctx.lineTo(-r * 0.3, -r * 1.1); ctx.closePath(); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(0, -r * 0.9, 2, 0, Math.PI * 2); ctx.fill();
}

function drawOrigamiOctopus(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawShadow(ctx, () => {
    ctx.beginPath(); ctx.arc(4, 5, r, 0, Math.PI * 2); ctx.fill();
  });
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.arc(0, -r * 0.2, r * 0.7, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
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
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(-r * 0.2, -r * 0.3, 3, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.2, -r * 0.3, 3, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.arc(-r * 0.2, -r * 0.4, r * 0.3, 0, Math.PI * 2); ctx.fill();
  ctx.globalAlpha = 1;
}

// ===== Player — status effects only =====
function drawPlayer(ctx: CanvasRenderingContext2D, p: PlayerState): void {
  ctx.save();
  ctx.translate(p.pos.x, p.pos.y);
  const t = Date.now() / 1000;
  const r = PLAYER_RADIUS;

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
function drawTowerPaperFrame(ctx: CanvasRenderingContext2D, sphere: SphereEntity, color: string, t: number): void {
  const pulse = 0.5 + Math.sin(t * 5 + sphere.pos.x * 0.01) * 0.5;
  const lift = Math.sin(t * 3.5 + sphere.pos.y * 0.008) * 0.8;
  ctx.save();
  ctx.translate(0, lift);

  ctx.fillStyle = 'rgba(58,46,31,0.13)';
  ctx.beginPath(); ctx.ellipse(3, 9, 21, 9, 0, 0, Math.PI * 2); ctx.fill();

  // Layered folded base.
  const points: Array<{ x: number; y: number }> = [];
  for (let i = 0; i < 8; i++) {
    const a = (i / 8) * Math.PI * 2 + Math.PI / 8;
    points.push({ x: Math.cos(a) * 19, y: Math.sin(a) * 19 });
  }
  ctx.fillStyle = shade(color, -48);
  ctx.beginPath(); points.forEach((p, i) => i ? ctx.lineTo(p.x, p.y + 3) : ctx.moveTo(p.x, p.y + 3)); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  ctx.fillStyle = shade(color, -16);
  ctx.beginPath(); points.forEach((p, i) => i ? ctx.lineTo(p.x, p.y) : ctx.moveTo(p.x, p.y)); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  for (let i = 0; i < 8; i += 2) { ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(points[i].x, points[i].y); ctx.stroke(); }

  // Distinct folded-paper body silhouette for each tower role.
  // The silhouette is intentionally compact so the existing tower frame remains visible.
  ctx.save();
  ctx.rotate(sphere.rotation * 0.35);
  ctx.fillStyle = shade(color, -24);
  ctx.strokeStyle = INK;
  ctx.lineWidth = 1.4;
  if (sphere.type === 'sniper') {
    // Tall crystal/periscope: visually points forward.
    ctx.beginPath();
    ctx.moveTo(-8, 13); ctx.lineTo(-6, -12); ctx.lineTo(2, -18);
    ctx.lineTo(13, -10); ctx.lineTo(10, 15); ctx.lineTo(0, 20);
    ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.fillStyle = shade(color, 22);
    ctx.beginPath(); ctx.moveTo(-6, -12); ctx.lineTo(2, -18); ctx.lineTo(2, -1); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = FOLD_LINE;
    ctx.beginPath(); ctx.moveTo(2, -1); ctx.lineTo(0, 20); ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    // Broad folded receiver with three short barrels.
    ctx.beginPath();
    ctx.moveTo(-15, -10); ctx.lineTo(4, -13); ctx.lineTo(14, -7);
    ctx.lineTo(14, 7); ctx.lineTo(4, 13); ctx.lineTo(-15, 10);
    ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.strokeStyle = shade(color, 24); ctx.lineWidth = 2;
    for (let i = -1; i <= 1; i++) {
      ctx.beginPath(); ctx.moveTo(0, i * 4); ctx.lineTo(19, i * 6); ctx.stroke();
    }
  } else if (sphere.type === 'chain') {
    // Two interlocking paper loops, animated against each other.
    const pulse = 0.5 + Math.sin(t * 7 + sphere.pos.y * 0.01) * 0.5;
    ctx.strokeStyle = shade(color, 20); ctx.lineWidth = 4;
    ctx.beginPath(); ctx.arc(-7, 0, 8 + pulse, -0.8, Math.PI + 0.8); ctx.stroke();
    ctx.beginPath(); ctx.arc(7, 0, 8 + (1 - pulse), Math.PI - 0.8, Math.PI * 2 - 0.8); ctx.stroke();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.4;
    ctx.beginPath(); ctx.arc(-7, 0, 8 + pulse, -0.8, Math.PI + 0.8); ctx.stroke();
    ctx.beginPath(); ctx.arc(7, 0, 8 + (1 - pulse), Math.PI - 0.8, Math.PI * 2 - 0.8); ctx.stroke();
  } else if (sphere.type === 'aura') {
    // Hexagonal amplifier/basin.
    ctx.beginPath();
    ctx.moveTo(0, -18); ctx.lineTo(14, -8); ctx.lineTo(12, 9);
    ctx.lineTo(0, 18); ctx.lineTo(-12, 9); ctx.lineTo(-14, -8);
    ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.strokeStyle = color; ctx.lineWidth = 2;
    ctx.beginPath(); ctx.arc(0, 2, 9 + Math.sin(t * 5) * 1.4, 0, Math.PI * 2); ctx.stroke();
    ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
    ctx.beginPath(); ctx.moveTo(0, -18); ctx.lineTo(0, 18); ctx.stroke();
  } else {
    // Default folded emitter/node.
    ctx.beginPath();
    ctx.moveTo(0, -17); ctx.lineTo(15, -7); ctx.lineTo(12, 11);
    ctx.lineTo(0, 17); ctx.lineTo(-12, 11); ctx.lineTo(-15, -7);
    ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.fillStyle = shade(color, 26);
    ctx.beginPath(); ctx.moveTo(0, -17); ctx.lineTo(15, -7); ctx.lineTo(0, 2); ctx.closePath(); ctx.fill();
  }
  ctx.restore();

  // Animated paper braces.
  ctx.save();
  ctx.rotate(t * 0.32 + sphere.rotation * 0.15);
  ctx.strokeStyle = 'rgba(' + hexToRgb(color) + ',0.48)';
  ctx.lineWidth = 1.6;
  ctx.setLineDash([6, 4]);
  ctx.beginPath(); ctx.arc(0, 0, 23 + pulse * 2, -0.55, 0.95); ctx.stroke();
  ctx.beginPath(); ctx.arc(0, 0, 23 + pulse * 2, Math.PI - 0.95, Math.PI + 0.55); ctx.stroke();
  ctx.setLineDash([]);
  ctx.restore();

  // Folded directional crown, unique to the tower type.
  ctx.save(); ctx.rotate(sphere.rotation);
  ctx.strokeStyle = INK; ctx.lineWidth = 1.2;
  if (sphere.type === 'sniper') {
    ctx.fillStyle = shade(color, 22);
    ctx.beginPath(); ctx.moveTo(0, -9); ctx.lineTo(10, -2); ctx.lineTo(0, 2); ctx.lineTo(-10, -2); ctx.closePath(); ctx.fill(); ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    ctx.fillStyle = shade(color, 18);
    for (let i = -1; i <= 1; i++) { ctx.beginPath(); ctx.moveTo(-7, i * 5 - 2); ctx.lineTo(2, i * 5 - 5); ctx.lineTo(4, i * 5); ctx.lineTo(2, i * 5 + 5); ctx.lineTo(-7, i * 5 + 2); ctx.closePath(); ctx.fill(); ctx.stroke(); }
  } else if (sphere.type === 'chain') {
    ctx.strokeStyle = '#d4a830'; ctx.lineWidth = 2;
    ctx.beginPath(); ctx.arc(0, -7, 4 + pulse, 0.4, Math.PI * 1.6); ctx.stroke();
    ctx.beginPath(); ctx.arc(0, 7, 4 + (1 - pulse), Math.PI * 1.4, Math.PI * 0.4); ctx.stroke();
  } else if (sphere.type === 'aura') {
    ctx.strokeStyle = color; ctx.lineWidth = 2;
    ctx.beginPath(); ctx.arc(0, 0, 12 + pulse * 2, 0, Math.PI * 2); ctx.stroke();
  } else {
    ctx.fillStyle = shade(color, 28);
    ctx.beginPath(); ctx.moveTo(0, -8); ctx.lineTo(7, 0); ctx.lineTo(0, 8); ctx.lineTo(-7, 0); ctx.closePath(); ctx.fill(); ctx.stroke();
  }
  ctx.restore();
  ctx.restore();
}

function drawSphereLegacy(ctx: CanvasRenderingContext2D, s: GameState, sphere: SphereEntity): void {
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  const stype = SPHERE_TYPES[sphere.type];
  const baseColor = stype.color;
  const radius = sphere.radius * (1 + (s.player.abilities.radius || 0) * 0.15) * (s.player.artifacts.includes('radius_shard') ? 1.1 : 1) * stype.rangeMult;
  const t = Date.now() / 1000;
  drawTowerPaperFrame(ctx, sphere, baseColor, t);
  ctx.strokeStyle = `rgba(${hexToRgb(baseColor)},0.12)`;
  ctx.lineWidth = 1; ctx.setLineDash([5, 5]);
  ctx.beginPath(); ctx.arc(0, 0, radius, 0, Math.PI * 2); ctx.stroke();
  ctx.setLineDash([]);
  if (stype.aura) {
    const auraPulse = 1 + Math.sin(t * 4) * 0.06;
    ctx.strokeStyle = baseColor; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.25 + Math.sin(t * 3) * 0.08;
    ctx.setLineDash([8, 4]);
    ctx.beginPath(); ctx.arc(0, 0, stype.auraRadius * auraPulse, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }
  const tier = sphere.visualTier;
  const hasTarget = s.enemies.some((enemy) => enemy.hp > 0 && Math.hypot(enemy.pos.x - sphere.pos.x, enemy.pos.y - sphere.pos.y) < radius);
  if (hasTarget) {
    const pulse = 0.5 + Math.sin(t * 10) * 0.5;
    ctx.fillStyle = `rgba(${hexToRgb(baseColor)},${0.08 + pulse * 0.09})`;
    ctx.beginPath();
    ctx.arc(0, 0, 21 + pulse * 4, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.fillStyle = 'rgba(58,46,31,0.15)';
  ctx.beginPath(); ctx.ellipse(2, 3, 16, 12, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = shade(baseColor, -30);
  ctx.beginPath();
  for (let i = 0; i < 8; i++) { const a = (i / 8) * Math.PI * 2 + Math.PI / 8; const x = Math.cos(a) * 16, y = Math.sin(a) * 16; if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y); }
  ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  for (let i = 0; i < 4; i++) { const a = (i / 4) * Math.PI * 2 + Math.PI / 8; ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(Math.cos(a) * 16, Math.sin(a) * 16); ctx.stroke(); }
  ctx.save(); ctx.rotate(sphere.rotation);
  if (sphere.type === 'sniper') {
    ctx.fillStyle = baseColor;
    ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(22, 0); ctx.lineTo(0, 4); ctx.lineTo(-6, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1; ctx.beginPath(); ctx.moveTo(-6, 0); ctx.lineTo(22, 0); ctx.stroke();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(22, 0); ctx.lineTo(0, 4); ctx.lineTo(-6, 0); ctx.closePath(); ctx.stroke();
    ctx.fillStyle = shade(baseColor, 30); ctx.beginPath(); ctx.arc(4, -6, 3, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    ctx.fillStyle = baseColor;
    for (let i = -1; i <= 1; i++) { ctx.beginPath(); ctx.moveTo(i * 5 - 3, -3); ctx.lineTo(i * 5 + 14, i * 2); ctx.lineTo(i * 5 + 14, i * 2 + 3); ctx.lineTo(i * 5 - 3, 3); ctx.closePath(); ctx.fill(); ctx.strokeStyle = INK; ctx.lineWidth = 1; ctx.stroke(); }
  } else if (sphere.type === 'chain') {
    ctx.fillStyle = baseColor;
    ctx.beginPath(); ctx.moveTo(-4, -3); ctx.lineTo(8, -8); ctx.lineTo(10, -6); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
    ctx.beginPath(); ctx.moveTo(-4, 3); ctx.lineTo(8, 8); ctx.lineTo(10, 6); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
    ctx.fillStyle = '#d4a830';
    ctx.beginPath(); ctx.arc(10, -7, 2 + Math.sin(t * 8), 0, Math.PI * 2); ctx.fill();
    ctx.beginPath(); ctx.arc(10, 7, 2 + Math.sin(t * 8 + 1), 0, Math.PI * 2); ctx.fill();
  } else if (sphere.type === 'aura') {
    ctx.fillStyle = baseColor;
    ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(10, 0); ctx.lineTo(0, 10); ctx.lineTo(-10, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1; ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(0, 10); ctx.moveTo(-10, 0); ctx.lineTo(10, 0); ctx.stroke();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(10, 0); ctx.lineTo(0, 10); ctx.lineTo(-10, 0); ctx.closePath(); ctx.stroke();
    ctx.fillStyle = `rgba(${hexToRgb(baseColor)},0.2)`; ctx.beginPath(); ctx.arc(0, 0, 14 + Math.sin(t * 4) * 2, 0, Math.PI * 2); ctx.fill();
  } else {
    ctx.fillStyle = baseColor;
    ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(14, -2); ctx.lineTo(14, 2); ctx.lineTo(0, 4); ctx.lineTo(-4, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1; ctx.beginPath(); ctx.moveTo(-4, 0); ctx.lineTo(14, 0); ctx.stroke();
    ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.beginPath(); ctx.moveTo(0, -4); ctx.lineTo(14, -2); ctx.lineTo(14, 2); ctx.lineTo(0, 4); ctx.lineTo(-4, 0); ctx.closePath(); ctx.stroke();
    ctx.fillStyle = shade(baseColor, 40); ctx.beginPath(); ctx.arc(14, 0, 2.5, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
  }
  ctx.restore();
  if (tier >= 1) {
    ctx.fillStyle = shade(baseColor, 30); ctx.strokeStyle = INK; ctx.lineWidth = 1;
    for (let i = 0; i < 8; i++) { const a = (i / 8) * Math.PI * 2 + t * 0.3; ctx.beginPath(); ctx.moveTo(Math.cos(a - 0.1) * 17, Math.sin(a - 0.1) * 17); ctx.lineTo(Math.cos(a) * 22, Math.sin(a) * 22); ctx.lineTo(Math.cos(a + 0.1) * 17, Math.sin(a + 0.1) * 17); ctx.closePath(); ctx.fill(); ctx.stroke(); }
  }
  if (tier >= 2) { ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2; ctx.globalAlpha = 0.5; ctx.beginPath(); ctx.arc(0, 0, 24, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1; }
  if (tier >= 3) {
    ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2;
    for (let i = 0; i < 4; i++) { const a = t * 2 + (i / 4) * Math.PI * 2; ctx.beginPath(); ctx.arc(0, 0, 28, a, a + 0.4); ctx.stroke(); }
  }
  if (tier >= 4) { ctx.fillStyle = '#d4943d'; ctx.beginPath(); ctx.moveTo(0, -5); ctx.lineTo(4, 0); ctx.lineTo(0, 5); ctx.lineTo(-4, 0); ctx.closePath(); ctx.fill(); ctx.strokeStyle = INK; ctx.lineWidth = 1; ctx.stroke(); }
  ctx.restore();
}

// Renderer-only facing state. Enemies keep their last travel direction while moving,
// instead of exposing the simulation's spin/turn value to the visual body.
const ENEMY_LAST_POS = new WeakMap<EnemyEntity, { x: number; y: number }>();
const ENEMY_FACING = new WeakMap<EnemyEntity, number>();

function getEnemyFacingAngle(e: EnemyEntity): number {
  const previous = ENEMY_LAST_POS.get(e);
  if (previous) {
    const dx = e.pos.x - previous.x;
    const dy = e.pos.y - previous.y;
    if (dx * dx + dy * dy > 0.04) ENEMY_FACING.set(e, Math.atan2(dy, dx));
  } else if (typeof e.rotation === 'number') {
    ENEMY_FACING.set(e, e.rotation);
  }
  ENEMY_LAST_POS.set(e, { x: e.pos.x, y: e.pos.y });
  return ENEMY_FACING.get(e) ?? 0;
}

// ===== Enemy — folded creatures =====
function drawOrigamiMouse(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const walk = t * 11;
  const bob = Math.sin(t * 11) * r * 0.035;
  ctx.save(); ctx.translate(0, bob);

  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(2, 5, r * 0.78, r * 0.42, 0, 0, Math.PI * 2); ctx.fill(); });

  // Six little paper legs, phase-shifted like a real crawler.
  ctx.strokeStyle = shade(fill, -30); ctx.lineWidth = Math.max(1.4, r * 0.1); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const phase = walk + i * 2.1;
    const swing = Math.sin(phase) * r * 0.16;
    const x = -r * 0.38 + i * r * 0.34;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.18); ctx.lineTo(x - r * 0.2, -r * 0.55 - swing); ctx.lineTo(x + r * 0.02, -r * 0.86 - swing * 0.55); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.18); ctx.lineTo(x + r * 0.2, r * 0.55 + swing); ctx.lineTo(x + r * 0.02, r * 0.86 + swing * 0.55); ctx.stroke();
  }
  ctx.lineCap = 'butt';

  // Folded body and forward-facing wedge head.
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.ellipse(-r * 0.08, 0, r * 0.72, r * 0.43, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.6;
  ctx.beginPath(); ctx.moveTo(-r * 0.54, -r * 0.22); ctx.lineTo(r * 0.22, -r * 0.36); ctx.lineTo(r * 0.57, -r * 0.05); ctx.lineTo(-r * 0.15, -r * 0.02); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.52, 0); ctx.lineTo(r * 0.54, 0); ctx.moveTo(-r * 0.04, -r * 0.42); ctx.lineTo(-r * 0.04, r * 0.42); ctx.stroke();

  ctx.fillStyle = shade(fill, 18);
  ctx.beginPath(); ctx.moveTo(r * 0.25, -r * 0.34); ctx.lineTo(r * 0.9, 0); ctx.lineTo(r * 0.25, r * 0.34); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.58, -r * 0.12, Math.max(1.7, r * 0.1), 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.58, r * 0.12, Math.max(1.7, r * 0.1), 0, Math.PI * 2); ctx.fill();

  // Ears and twitching antenna-like whiskers.
  ctx.fillStyle = shade(fill, 8);
  ctx.beginPath(); ctx.moveTo(r * 0.35, -r * 0.27); ctx.lineTo(r * 0.48, -r * 0.7); ctx.lineTo(r * 0.72, -r * 0.3); ctx.closePath(); ctx.fill(); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.35, r * 0.27); ctx.lineTo(r * 0.48, r * 0.7); ctx.lineTo(r * 0.72, r * 0.3); ctx.closePath(); ctx.fill(); ctx.stroke();
  ctx.strokeStyle = shade(fill, -25); ctx.lineWidth = 1;
  const whisker = Math.sin(t * 7) * r * 0.05;
  ctx.beginPath(); ctx.moveTo(r * 0.82, -r * 0.06); ctx.lineTo(r * 1.12, -r * 0.18 + whisker); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.82, r * 0.06); ctx.lineTo(r * 1.12, r * 0.18 - whisker); ctx.stroke();
  ctx.restore();
}


function drawOrigamiFish(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const walk = t * 9;
  ctx.save(); ctx.translate(Math.sin(t * 9) * r * 0.025, Math.sin(t * 9) * r * 0.035);
  drawShadow(ctx, () => { ctx.beginPath(); ctx.ellipse(4, 6, r * 0.82, r * 0.5, 0, 0, Math.PI * 2); ctx.fill(); });

  // Six insect legs under an angular origami beetle.
  ctx.strokeStyle = shade(fill, -28); ctx.lineWidth = Math.max(1.5, r * 0.1); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const phase = walk + i * 1.9;
    const swing = Math.sin(phase) * r * 0.18;
    const x = -r * 0.38 + i * r * 0.35;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.18); ctx.lineTo(x - r * 0.28, -r * 0.58 + swing); ctx.lineTo(x - r * 0.12, -r * 0.9 + swing); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.18); ctx.lineTo(x - r * 0.28, r * 0.58 - swing); ctx.lineTo(x - r * 0.12, r * 0.9 - swing); ctx.stroke();
  }
  ctx.lineCap = 'butt';

  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.7, -r * 0.4); ctx.lineTo(r * 0.54, -r * 0.48); ctx.lineTo(r * 0.88, 0); ctx.lineTo(r * 0.54, r * 0.48); ctx.lineTo(-r * 0.7, r * 0.4); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.55;
  ctx.beginPath(); ctx.moveTo(-r * 0.52, -r * 0.35); ctx.lineTo(r * 0.48, -r * 0.42); ctx.lineTo(r * 0.65, -r * 0.06); ctx.lineTo(-r * 0.1, -r * 0.02); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.fillStyle = shade(fill, -18);
  ctx.beginPath(); ctx.moveTo(-r * 0.52, 0); ctx.lineTo(-r * 0.96, -r * 0.5); ctx.lineTo(-r * 0.78, 0); ctx.lineTo(-r * 0.96, r * 0.5); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.58, 0); ctx.lineTo(r * 0.7, 0); ctx.moveTo(-r * 0.08, -r * 0.4); ctx.lineTo(-r * 0.08, r * 0.4); ctx.stroke();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.moveTo(-r * 0.7, -r * 0.4); ctx.lineTo(r * 0.54, -r * 0.48); ctx.lineTo(r * 0.88, 0); ctx.lineTo(r * 0.54, r * 0.48); ctx.lineTo(-r * 0.7, r * 0.4); ctx.closePath(); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.55, -r * 0.11, Math.max(1.8, r * 0.09), 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.55, r * 0.11, Math.max(1.8, r * 0.09), 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -20); ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r * 0.78, -r * 0.2); ctx.lineTo(r * 0.98, -r * 0.34); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.78, r * 0.2); ctx.lineTo(r * 0.98, r * 0.34); ctx.stroke();
  ctx.restore();
}


function drawCreatureShadow(ctx: CanvasRenderingContext2D, r: number): void {
  ctx.fillStyle = 'rgba(58,46,31,0.13)';
  ctx.beginPath();
  ctx.ellipse(3, r * 0.55, r * 0.82, r * 0.28, 0, 0, Math.PI * 2);
  ctx.fill();
}

function drawPaperLeg(ctx: CanvasRenderingContext2D, r: number, x: number, side: number, phase: number, reach = 0.8): void {
  const swing = Math.sin(phase) * r * 0.16;
  ctx.beginPath();
  ctx.moveTo(x, side * r * 0.18);
  ctx.lineTo(x - r * 0.18, side * (r * 0.5 + swing));
  ctx.lineTo(x + r * 0.08, side * r * reach + swing * 0.55);
  ctx.stroke();
}

function drawPaperMouseCreature(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawCreatureShadow(ctx, r);
  ctx.strokeStyle = shade(fill, -34); ctx.lineWidth = Math.max(1.6, r * 0.09); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const x = -r * 0.34 + i * r * 0.34;
    drawPaperLeg(ctx, r, x, -1, t * 12 + i * 2.1, 0.9);
    drawPaperLeg(ctx, r, x, 1, t * 12 + i * 2.1 + Math.PI, 0.9);
  }
  ctx.lineCap = 'butt';
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.ellipse(-r * 0.08, 0, r * 0.72, r * 0.46, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.52;
  ctx.beginPath(); ctx.moveTo(-r * 0.55, -r * 0.2); ctx.lineTo(r * 0.28, -r * 0.38); ctx.lineTo(r * 0.58, -r * 0.04); ctx.lineTo(-r * 0.15, -r * 0.02); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.fillStyle = shade(fill, 22);
  ctx.beginPath(); ctx.moveTo(r * 0.22, -r * 0.34); ctx.lineTo(r * 0.9, 0); ctx.lineTo(r * 0.22, r * 0.34); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.5, 0); ctx.lineTo(r * 0.65, 0); ctx.moveTo(-r * 0.08, -r * 0.43); ctx.lineTo(-r * 0.08, r * 0.43); ctx.stroke();
  ctx.fillStyle = shade(fill, 8);
  ctx.beginPath(); ctx.moveTo(r * 0.3, -r * 0.25); ctx.lineTo(r * 0.44, -r * 0.66); ctx.lineTo(r * 0.7, -r * 0.28); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(r * 0.3, r * 0.25); ctx.lineTo(r * 0.44, r * 0.66); ctx.lineTo(r * 0.7, r * 0.28); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.5; ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.56, -r * 0.1, 2.3, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.56, r * 0.1, 2.3, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -28); ctx.lineWidth = 1;
  const whisker = Math.sin(t * 8) * r * 0.04;
  ctx.beginPath(); ctx.moveTo(r * 0.78, -r * 0.05); ctx.lineTo(r * 1.08, -r * 0.2 + whisker); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.78, r * 0.05); ctx.lineTo(r * 1.08, r * 0.2 - whisker); ctx.stroke();
}

function drawPaperBeetle(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  drawCreatureShadow(ctx, r);
  ctx.strokeStyle = shade(fill, -34); ctx.lineWidth = Math.max(1.7, r * 0.085); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const x = -r * 0.42 + i * r * 0.38;
    drawPaperLeg(ctx, r, x, -1, t * 10 + i * 1.8, 0.86);
    drawPaperLeg(ctx, r, x, 1, t * 10 + i * 1.8 + Math.PI, 0.86);
  }
  ctx.lineCap = 'butt';
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.78, -r * 0.42); ctx.lineTo(r * 0.5, -r * 0.5); ctx.lineTo(r * 0.84, 0); ctx.lineTo(r * 0.5, r * 0.5); ctx.lineTo(-r * 0.78, r * 0.42); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, -18);
  ctx.beginPath(); ctx.moveTo(-r * 0.3, -r * 0.4); ctx.lineTo(r * 0.65, -r * 0.25); ctx.lineTo(r * 0.65, 0); ctx.lineTo(-r * 0.3, 0); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.34;
  ctx.beginPath(); ctx.moveTo(-r * 0.55, -r * 0.32); ctx.lineTo(r * 0.42, -r * 0.42); ctx.lineTo(r * 0.18, -r * 0.06); ctx.lineTo(-r * 0.25, -r * 0.08); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.55, 0); ctx.lineTo(r * 0.65, 0); ctx.moveTo(0, -r * 0.45); ctx.lineTo(0, r * 0.45); ctx.stroke();
  ctx.fillStyle = shade(fill, 24);
  ctx.beginPath(); ctx.moveTo(r * 0.5, -r * 0.3); ctx.lineTo(r * 1.02, 0); ctx.lineTo(r * 0.5, r * 0.3); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1.6; ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.66, -r * 0.11, 2.4, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.66, r * 0.11, 2.4, 0, Math.PI * 2); ctx.fill();
}

function drawPaperMantis(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const bob = Math.sin(t * 10) * r * 0.035;
  ctx.save(); ctx.translate(0, bob);
  drawCreatureShadow(ctx, r);
  ctx.strokeStyle = shade(fill, -34); ctx.lineWidth = Math.max(1.5, r * 0.08); ctx.lineCap = 'round';
  for (let i = 0; i < 2; i++) {
    const x = -r * 0.22 + i * r * 0.44;
    const swing = Math.sin(t * 13 + i * Math.PI) * r * 0.22;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.12); ctx.lineTo(x + r * 0.28, -r * 0.66 + swing); ctx.lineTo(x + r * 0.62, -r * 0.4 + swing); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.12); ctx.lineTo(x + r * 0.42, r * 0.68 - swing); ctx.lineTo(x + r * 0.78, r * 0.42 - swing); ctx.stroke();
  }
  ctx.lineCap = 'butt';
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.66, -r * 0.34); ctx.lineTo(r * 0.48, -r * 0.5); ctx.lineTo(r * 0.86, 0); ctx.lineTo(r * 0.48, r * 0.5); ctx.lineTo(-r * 0.66, r * 0.34); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 22);
  ctx.beginPath(); ctx.moveTo(-r * 0.18, -r * 0.35); ctx.lineTo(r * 0.48, -r * 0.5); ctx.lineTo(r * 0.18, 0); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.42;
  ctx.beginPath(); ctx.moveTo(r * 0.04, -r * 0.42); ctx.lineTo(r * 0.58, -r * 0.1); ctx.lineTo(r * 0.08, -r * 0.02); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.45, 0); ctx.lineTo(r * 0.65, 0); ctx.stroke();
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.57, -r * 0.11, 2.4, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.57, r * 0.11, 2.4, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = shade(fill, -22); ctx.lineWidth = 1.1;
  const feeler = Math.sin(t * 7) * r * 0.05;
  ctx.beginPath(); ctx.moveTo(r * 0.76, -r * 0.05); ctx.lineTo(r * 1.12, -r * 0.18 + feeler); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.76, r * 0.05); ctx.lineTo(r * 1.12, r * 0.18 - feeler); ctx.stroke();
  ctx.restore();
}

function drawPaperMoth(ctx: CanvasRenderingContext2D, r: number, fill: string, highlight: string, t: number): void {
  const flap = Math.sin(t * 10) * r * 0.13;
  drawCreatureShadow(ctx, r);
  ctx.fillStyle = fill;
  ctx.beginPath(); ctx.moveTo(-r * 0.1, 0); ctx.lineTo(r * 0.76, -r * 0.14); ctx.lineTo(-r * 0.1, r * 0.14); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(fill, 20);
  ctx.beginPath(); ctx.moveTo(-r * 0.05, -r * 0.08); ctx.lineTo(-r * 0.62, -r * 0.84 - flap); ctx.lineTo(r * 0.45, -r * 0.54 - flap * 0.5); ctx.lineTo(r * 0.25, -r * 0.06); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(-r * 0.05, r * 0.08); ctx.lineTo(-r * 0.62, r * 0.84 + flap); ctx.lineTo(r * 0.45, r * 0.54 + flap * 0.5); ctx.lineTo(r * 0.25, r * 0.06); ctx.closePath(); ctx.fill();
  ctx.fillStyle = highlight; ctx.globalAlpha = 0.3;
  ctx.beginPath(); ctx.moveTo(-r * 0.1, -r * 0.2); ctx.lineTo(r * 0.36, -r * 0.48); ctx.lineTo(r * 0.2, -r * 0.05); ctx.closePath(); ctx.fill(); ctx.globalAlpha = 1;
  ctx.strokeStyle = FOLD_LINE; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.05, 0); ctx.lineTo(r * 0.62, 0); ctx.stroke();
  ctx.strokeStyle = shade(fill, -28); ctx.lineWidth = 1.4; ctx.lineCap = 'round';
  const swing = Math.sin(t * 11) * r * 0.09;
  ctx.beginPath(); ctx.moveTo(-r * 0.05, -r * 0.1); ctx.lineTo(-r * 0.28, -r * 0.38 + swing); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(-r * 0.05, r * 0.1); ctx.lineTo(-r * 0.28, r * 0.38 - swing); ctx.stroke();
  ctx.lineCap = 'butt';
  ctx.fillStyle = INK;
  ctx.beginPath(); ctx.arc(r * 0.48, -r * 0.07, 2.2, 0, Math.PI * 2); ctx.fill();
  ctx.beginPath(); ctx.arc(r * 0.48, r * 0.07, 2.2, 0, Math.PI * 2); ctx.fill();
  ctx.strokeStyle = INK; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(r * 0.58, -r * 0.05); ctx.lineTo(r * 0.9, -r * 0.2); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.58, r * 0.05); ctx.lineTo(r * 0.9, r * 0.2); ctx.stroke();
}

function drawEnemyLegacy(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  const t = Date.now() / 1000;
  const facing = e.isBoss ? 0 : getEnemyFacingAngle(e);
  ctx.save();
  ctx.translate(e.pos.x, e.pos.y);
  ctx.rotate(facing);

  const hitStrength = Math.max(0, Math.min(1, e.hitFlash / 0.15));
  if (hitStrength > 0) ctx.scale(1 + hitStrength * 0.1, 1 + hitStrength * 0.1);
  if (hitStrength > 0) {
    ctx.strokeStyle = 'rgba(196,69,61,' + (hitStrength * 0.45) + ')';
    ctx.lineWidth = 2;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 7 + (1 - hitStrength) * 8, 0, Math.PI * 2); ctx.stroke();
  }

  const color = hitStrength > 0 ? '#f4ecd8' : e.color;
  const highlight = hitStrength > 0 ? '#ffffff' : shade(e.color, 35);
  const frozen = e.freezeTimer > 0;
  const bodyColor = frozen ? '#6a9ab0' : color;
  const bodyHighlight = frozen ? '#8ac0d8' : highlight;

  if (e.shape === 'circle') {
    if (e.type === 'fast') drawPaperMantis(ctx, e.radius, bodyColor, bodyHighlight, t);
    else drawPaperMouseCreature(ctx, e.radius, bodyColor, bodyHighlight, t);
  } else if (e.shape === 'square') {
    if (e.type === 'tank') drawPaperBeetle(ctx, e.radius * 1.1, bodyColor, bodyHighlight, t);
    else drawPaperBeetle(ctx, e.radius, bodyColor, bodyHighlight, t);
  } else if (e.shape === 'triangle') {
    drawPaperMoth(ctx, e.radius, bodyColor, bodyHighlight, t);
  } else if (e.shape === 'hexagon') {
    drawBossLegacy(ctx, e);
  }

  if (!e.isBoss && e.tier > 0) drawEnemyTierDetails(ctx, e);
  if (e.isElite) {
    ctx.strokeStyle = '#8a4a8a'; ctx.lineWidth = 2;
    ctx.globalAlpha = 0.5 + Math.sin(t * 5) * 0.2;
    ctx.setLineDash([4, 3]);
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 8, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]); ctx.globalAlpha = 1;
  }
  if (e.fireTimer > 0) { ctx.fillStyle = 'rgba(180,80,30,0.25)'; ctx.beginPath(); ctx.arc(0, 0, e.radius + 2, 0, Math.PI * 2); ctx.fill(); }
  if (e.poisonTimer > 0) { ctx.fillStyle = 'rgba(90,140,60,0.2)'; ctx.beginPath(); ctx.arc(0, 0, e.radius + 2, 0, Math.PI * 2); ctx.fill(); }
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
    for (let i = 0; i < 3; i++) { const a = t * 2 + (i / 3) * Math.PI * 2; ctx.beginPath(); ctx.arc(Math.cos(a) * (e.radius + 5), Math.sin(a) * (e.radius + 5), 2, 0, Math.PI * 2); ctx.fill(); }
  }
  if (e.tier >= 2) { ctx.strokeStyle = shade(e.color, 20); ctx.lineWidth = 1.5; ctx.globalAlpha = 0.4 + Math.sin(t * 4) * 0.15; ctx.beginPath(); ctx.arc(0, 0, e.radius + 4, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1; }
  if (e.tier >= 3) { ctx.strokeStyle = shade(e.color, 50); ctx.lineWidth = 1.5; ctx.beginPath(); ctx.moveTo(-e.radius * 0.5, 0); ctx.lineTo(e.radius * 0.5, 0); ctx.moveTo(0, -e.radius * 0.5); ctx.lineTo(0, e.radius * 0.5); ctx.stroke(); }
}

// ===== Boss — beautiful diverse origami figures =====
function drawBossLegacy(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  const t = Date.now() / 1000;
  const tier = e.tier;
  const color = e.freezeTimer > 0 ? '#6a9ab0' : e.color;
  const highlight = e.freezeTimer > 0 ? '#8ac0d8' : shade(e.color, 35);
  const r = e.radius;
  if (e.bossType === 'shooter') drawOrigamiPhoenix(ctx, r, color, highlight, t);
  else if (e.bossType === 'charger') drawOrigamiDragon(ctx, r, color, highlight, t);
  else if (e.bossType === 'summoner') drawOrigamiLotus(ctx, r, color, highlight);
  else if (e.bossType === 'aura') drawOrigamiOctopus(ctx, r, color, highlight, t);
  else drawOrigamiLotus(ctx, r, color, highlight);
  if (e.bossType === 'aura' && e.auraRadius) { ctx.strokeStyle = color; ctx.lineWidth = 2; ctx.globalAlpha = 0.2 + Math.sin(t * 2) * 0.08; ctx.setLineDash([6, 4]); ctx.beginPath(); ctx.arc(0, 0, e.auraRadius, 0, Math.PI * 2); ctx.stroke(); ctx.setLineDash([]); ctx.globalAlpha = 1; }
  if (e.isCharging) { ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2.5; ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(e.chargeDir.x * 60, e.chargeDir.y * 60); ctx.stroke(); }
  if (e.bossType === 'summoner') { ctx.strokeStyle = '#8a4a8a'; ctx.lineWidth = 1.5; ctx.globalAlpha = 0.4; for (let i = 0; i < 3; i++) { const a = t * 2 + (i / 3) * Math.PI * 2; ctx.beginPath(); ctx.arc(Math.cos(a) * 30, Math.sin(a) * 30, 8, 0, Math.PI * 2); ctx.stroke(); } ctx.globalAlpha = 1; }
  if (tier >= 1) { ctx.fillStyle = shade(color, 30); ctx.strokeStyle = INK; ctx.lineWidth = 1; for (let i = 0; i < 6; i++) { const a = (i / 6) * Math.PI * 2 + t * 0.5; ctx.beginPath(); ctx.moveTo(Math.cos(a - 0.1) * r, Math.sin(a - 0.1) * r); ctx.lineTo(Math.cos(a) * (r + 10), Math.sin(a) * (r + 10)); ctx.lineTo(Math.cos(a + 0.1) * r, Math.sin(a + 0.1) * r); ctx.closePath(); ctx.fill(); ctx.stroke(); } }
  if (tier >= 2) { ctx.strokeStyle = '#c4453d'; ctx.lineWidth = 2; ctx.globalAlpha = 0.4 + Math.sin(t * 3) * 0.2; ctx.beginPath(); ctx.arc(0, 0, r + 14, 0, Math.PI * 2); ctx.stroke(); ctx.globalAlpha = 1; }
  if (tier >= 3) { ctx.strokeStyle = '#d4943d'; ctx.lineWidth = 2; for (let i = 0; i < 8; i++) { const a = -t + (i / 8) * Math.PI * 2; ctx.beginPath(); ctx.moveTo(Math.cos(a) * (r + 18), Math.sin(a) * (r + 18)); ctx.lineTo(Math.cos(a) * (r + 24), Math.sin(a) * (r + 24)); ctx.stroke(); } }
}


// ============================================================================
// ECHO SPHERE — VOID / ENERGY VISUAL PASS
// The world is intentionally dark and geometric. Spheres are luminous nodes;
// enemies are designed insectoid silhouettes with recognizable anatomy.
// ============================================================================

const MODERN_INK = '#dcecff';

function glowCircle(ctx: CanvasRenderingContext2D, radius: number, color: string, alpha = 0.18): void {
  const rgb = hexToRgb(color);
  const g = ctx.createRadialGradient(0, 0, 0, 0, 0, radius);
  g.addColorStop(0, `rgba(${rgb},${alpha})`);
  g.addColorStop(0.45, `rgba(${rgb},${alpha * 0.32})`);
  g.addColorStop(1, `rgba(${rgb},0)`);
  ctx.fillStyle = g;
  ctx.beginPath();
  ctx.arc(0, 0, radius, 0, Math.PI * 2);
  ctx.fill();
}

function drawModernSphere(ctx: CanvasRenderingContext2D, s: GameState, sphere: SphereEntity): void {
  const def = SPHERE_TYPES[sphere.type];
  const color = def.color;
  const t = Date.now() / 1000;
  const tier = sphere.visualTier;
  const radius = sphere.radius * (1 + (s.player.abilities.radius || 0) * 0.15) *
    (s.player.artifacts.includes('radius_shard') ? 1.1 : 1) * def.rangeMult;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);

  glowCircle(ctx, 42 + tier * 6, color, 0.20 + tier * 0.025);

  // Range is a quiet field, not a hard UI circle.
  ctx.strokeStyle = `rgba(${hexToRgb(color)},0.08)`;
  ctx.lineWidth = 1;
  ctx.beginPath(); ctx.arc(0, 0, radius, 0, Math.PI * 2); ctx.stroke();

  if (def.aura) {
    const ar = def.auraRadius * (1 + Math.sin(t * 3.5) * 0.035);
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.22)`;
    ctx.lineWidth = 1.5;
    ctx.setLineDash([10, 8]);
    ctx.beginPath(); ctx.arc(0, 0, ar, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]);
    ctx.fillStyle = `rgba(${hexToRgb(color)},0.035)`;
    ctx.beginPath(); ctx.arc(0, 0, ar, 0, Math.PI * 2); ctx.fill();
  }

  // Orbiting evolution geometry.
  if (tier >= 1) {
    ctx.save();
    ctx.rotate(t * 0.45);
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.55)`;
    ctx.lineWidth = 1.2;
    ctx.beginPath(); ctx.arc(0, 0, 24, -0.9, 1.15); ctx.stroke();
    ctx.beginPath(); ctx.arc(0, 0, 24, 2.25, 4.3); ctx.stroke();
    ctx.restore();
  }
  if (tier >= 2) {
    ctx.save();
    ctx.rotate(-t * 0.28);
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.42)`;
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(0, -28); ctx.lineTo(24, 14); ctx.lineTo(-24, 14); ctx.closePath();
    ctx.stroke();
    ctx.restore();
  }
  if (tier >= 3) {
    ctx.save();
    ctx.rotate(t * 0.18);
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.7)`;
    ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.arc(0, 0, 30, 0, Math.PI * 2); ctx.stroke();
    ctx.beginPath(); ctx.arc(0, 0, 34, 0.35, 2.1); ctx.stroke();
    ctx.restore();
  }
  if (tier >= 4) {
    ctx.save();
    ctx.rotate(-t * 0.65);
    ctx.strokeStyle = '#ffffff';
    ctx.globalAlpha = 0.72;
    ctx.lineWidth = 1.2;
    ctx.beginPath(); ctx.moveTo(0, -38); ctx.lineTo(0, -30); ctx.moveTo(0, 30); ctx.lineTo(0, 38);
    ctx.moveTo(-38, 0); ctx.lineTo(-30, 0); ctx.moveTo(30, 0); ctx.lineTo(38, 0); ctx.stroke();
    ctx.restore();
  }

  // Physical luminous core.
  const core = ctx.createRadialGradient(-4, -5, 1, 0, 0, 18);
  core.addColorStop(0, '#ffffff');
  core.addColorStop(0.16, shade(color, 65));
  core.addColorStop(0.55, color);
  core.addColorStop(1, shade(color, -45));
  ctx.fillStyle = core;
  ctx.beginPath(); ctx.arc(0, 0, 12 + Math.min(4, tier), 0, Math.PI * 2); ctx.fill();

  ctx.strokeStyle = `rgba(255,255,255,0.82)`;
  ctx.lineWidth = 1.1;
  ctx.beginPath(); ctx.arc(0, 0, 12 + Math.min(4, tier), 0, Math.PI * 2); ctx.stroke();

  // Role-specific inner geometry.
  ctx.save();
  ctx.rotate(sphere.rotation);
  ctx.strokeStyle = `rgba(255,255,255,0.72)`;
  ctx.fillStyle = `rgba(${hexToRgb(color)},0.55)`;
  ctx.lineWidth = 1.1;
  if (sphere.type === 'sniper') {
    ctx.beginPath(); ctx.moveTo(0, -14); ctx.lineTo(3, -3); ctx.lineTo(14, 0); ctx.lineTo(3, 3); ctx.lineTo(0, 14); ctx.lineTo(-3, 3); ctx.lineTo(-14, 0); ctx.lineTo(-3, -3); ctx.closePath(); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(4, 0); ctx.lineTo(19, 0); ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    for (let i = -1; i <= 1; i++) { ctx.beginPath(); ctx.moveTo(-8, i * 4); ctx.lineTo(16, i * 7); ctx.stroke(); }
  } else if (sphere.type === 'chain') {
    ctx.beginPath(); ctx.arc(-6, 0, 7, -0.9, Math.PI + 0.9); ctx.stroke();
    ctx.beginPath(); ctx.arc(6, 0, 7, Math.PI - 0.9, Math.PI * 2 - 0.9); ctx.stroke();
    ctx.fillStyle = '#fff2a6'; ctx.beginPath(); ctx.arc(13, 0, 2.2 + Math.sin(t * 8), 0, Math.PI * 2); ctx.fill();
  } else if (sphere.type === 'aura') {
    ctx.beginPath(); ctx.arc(0, 0, 8 + Math.sin(t * 5) * 1.5, 0, Math.PI * 2); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(-8, 0); ctx.lineTo(8, 0); ctx.moveTo(0, -8); ctx.lineTo(0, 8); ctx.stroke();
  } else {
    ctx.beginPath(); ctx.moveTo(0, -12); ctx.lineTo(12, 0); ctx.lineTo(0, 12); ctx.lineTo(-12, 0); ctx.closePath(); ctx.stroke();
  }
  ctx.restore();

  if (s.enemies.some((enemy) => enemy.hp > 0 && Math.hypot(enemy.pos.x - sphere.pos.x, enemy.pos.y - sphere.pos.y) < radius)) {
    const p = 0.5 + Math.sin(t * 11) * 0.5;
    ctx.strokeStyle = `rgba(${hexToRgb(color)},${0.22 + p * 0.22})`;
    ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.arc(0, 0, 20 + p * 3, 0, Math.PI * 2); ctx.stroke();
  }

  ctx.restore();
}

function insectLeg(ctx: CanvasRenderingContext2D, r: number, x: number, side: number, phase: number, length = 1): void {
  const swing = Math.sin(phase) * r * 0.12;
  ctx.beginPath();
  ctx.moveTo(x, side * r * 0.16);
  ctx.lineTo(x + r * 0.12, side * r * 0.48 + swing);
  ctx.lineTo(x + r * 0.28, side * r * length - swing * 0.45);
  ctx.stroke();
}

function drawInsectEye(ctx: CanvasRenderingContext2D, x: number, y: number, r: number, color: string): void {
  ctx.fillStyle = '#02040a';
  ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color;
  ctx.globalAlpha = 0.95;
  ctx.beginPath(); ctx.arc(x - r * 0.25, y - r * 0.25, r * 0.42, 0, Math.PI * 2); ctx.fill();
  ctx.globalAlpha = 1;
}

function drawVoidSkitter(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  ctx.strokeStyle = shade(color, -45); ctx.lineWidth = Math.max(1.3, r * 0.09); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const x = -r * 0.42 + i * r * 0.38;
    insectLeg(ctx, r, x, -1, t * 12 + i * 1.7, 0.95);
    insectLeg(ctx, r, x, 1, t * 12 + i * 1.7 + Math.PI, 0.95);
  }
  ctx.lineCap = 'butt';

  ctx.fillStyle = shade(color, -35);
  ctx.beginPath(); ctx.ellipse(-r * 0.18, 0, r * 0.62, r * 0.42, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.ellipse(-r * 0.02, 0, r * 0.5, r * 0.34, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = shade(color, 18);
  ctx.beginPath(); ctx.ellipse(r * 0.42, 0, r * 0.36, r * 0.29, 0, 0, Math.PI * 2); ctx.fill();

  ctx.strokeStyle = 'rgba(255,255,255,0.25)'; ctx.lineWidth = 0.8;
  ctx.beginPath(); ctx.moveTo(-r * 0.45, 0); ctx.lineTo(r * 0.52, 0); ctx.stroke();
  drawInsectEye(ctx, r * 0.58, -r * 0.11, Math.max(1.6, r * 0.11), '#ff526d');
  drawInsectEye(ctx, r * 0.58, r * 0.11, Math.max(1.6, r * 0.11), '#ff526d');

  ctx.strokeStyle = shade(color, -35); ctx.lineWidth = Math.max(1, r * 0.045);
  const a = Math.sin(t * 7) * 0.08;
  ctx.beginPath(); ctx.moveTo(r * 0.62, -r * 0.08); ctx.quadraticCurveTo(r * 0.92, -r * 0.4 + a * r, r * 1.18, -r * 0.28); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.62, r * 0.08); ctx.quadraticCurveTo(r * 0.92, r * 0.4 - a * r, r * 1.18, r * 0.28); ctx.stroke();
}

function drawVoidBeetle(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  ctx.strokeStyle = shade(color, -48); ctx.lineWidth = Math.max(1.5, r * 0.085); ctx.lineCap = 'round';
  for (let i = 0; i < 3; i++) {
    const x = -r * 0.45 + i * r * 0.42;
    insectLeg(ctx, r, x, -1, t * 9 + i * 1.9, 0.86);
    insectLeg(ctx, r, x, 1, t * 9 + i * 1.9 + Math.PI, 0.86);
  }
  ctx.lineCap = 'butt';

  ctx.fillStyle = shade(color, -42);
  ctx.beginPath(); ctx.ellipse(-r * 0.08, 0, r * 0.82, r * 0.55, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.ellipse(-r * 0.08, 0, r * 0.7, r * 0.47, 0, 0, Math.PI * 2); ctx.fill();

  // Hard wing cases and central seam.
  ctx.fillStyle = shade(color, 20);
  ctx.beginPath(); ctx.moveTo(-r * 0.68, -r * 0.42); ctx.quadraticCurveTo(0, -r * 0.7, r * 0.6, -r * 0.28); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.fillStyle = shade(color, -12);
  ctx.beginPath(); ctx.moveTo(-r * 0.68, r * 0.42); ctx.quadraticCurveTo(0, r * 0.7, r * 0.6, r * 0.28); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = 'rgba(255,255,255,0.26)'; ctx.lineWidth = 0.9;
  ctx.beginPath(); ctx.moveTo(0, -r * 0.48); ctx.lineTo(0, r * 0.48); ctx.stroke();

  ctx.fillStyle = shade(color, 28);
  ctx.beginPath(); ctx.moveTo(r * 0.42, -r * 0.28); ctx.lineTo(r * 0.95, -r * 0.16); ctx.lineTo(r * 0.8, r * 0.16); ctx.lineTo(r * 0.42, r * 0.28); ctx.closePath(); ctx.fill();
  drawInsectEye(ctx, r * 0.68, -r * 0.1, Math.max(1.8, r * 0.1), '#ff7b3f');
  drawInsectEye(ctx, r * 0.68, r * 0.1, Math.max(1.8, r * 0.1), '#ff7b3f');
}

function drawVoidMantis(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  ctx.strokeStyle = shade(color, -50); ctx.lineWidth = Math.max(1.3, r * 0.075); ctx.lineCap = 'round';
  // Long rear legs.
  for (let i = 0; i < 2; i++) {
    const x = -r * 0.28 + i * r * 0.48;
    const phase = t * 13 + i * Math.PI;
    ctx.beginPath(); ctx.moveTo(x, -r * 0.1); ctx.lineTo(x - r * 0.32, -r * 0.7 + Math.sin(phase) * r * 0.18); ctx.lineTo(x - r * 0.72, -r * 0.5); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(x, r * 0.1); ctx.lineTo(x - r * 0.32, r * 0.7 - Math.sin(phase) * r * 0.18); ctx.lineTo(x - r * 0.72, r * 0.5); ctx.stroke();
  }
  // Raptorial arms.
  ctx.lineWidth = Math.max(1.5, r * 0.09);
  ctx.beginPath(); ctx.moveTo(r * 0.15, -r * 0.18); ctx.lineTo(r * 0.52, -r * 0.58); ctx.lineTo(r * 0.8, -r * 0.32); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(r * 0.15, r * 0.18); ctx.lineTo(r * 0.52, r * 0.58); ctx.lineTo(r * 0.8, r * 0.32); ctx.stroke();

  ctx.fillStyle = shade(color, -36);
  ctx.beginPath(); ctx.ellipse(-r * 0.1, 0, r * 0.56, r * 0.7, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color;
  ctx.beginPath(); ctx.ellipse(r * 0.28, 0, r * 0.48, r * 0.42, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = shade(color, 24);
  ctx.beginPath(); ctx.moveTo(-r * 0.28, -r * 0.58); ctx.lineTo(r * 0.48, -r * 0.4); ctx.lineTo(r * 0.08, 0); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(-r * 0.28, r * 0.58); ctx.lineTo(r * 0.48, r * 0.4); ctx.lineTo(r * 0.08, 0); ctx.closePath(); ctx.fill();
  drawInsectEye(ctx, r * 0.56, -r * 0.12, Math.max(1.8, r * 0.105), '#a875ff');
  drawInsectEye(ctx, r * 0.56, r * 0.12, Math.max(1.8, r * 0.105), '#a875ff');
}

function drawVoidMoth(ctx: CanvasRenderingContext2D, r: number, color: string, t: number): void {
  const flap = Math.sin(t * 12) * 0.12;
  ctx.fillStyle = shade(color, -25);
  ctx.beginPath(); ctx.ellipse(0, 0, r * 0.22, r * 0.7, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = shade(color, 12);
  ctx.beginPath(); ctx.moveTo(-r * 0.05, -r * 0.1); ctx.lineTo(-r * 0.88, -r * 0.72 - flap * r); ctx.lineTo(-r * 0.4, 0); ctx.lineTo(-r * 0.88, r * 0.72 + flap * r); ctx.closePath(); ctx.fill();
  ctx.beginPath(); ctx.moveTo(r * 0.05, -r * 0.1); ctx.lineTo(r * 0.88, -r * 0.72 + flap * r); ctx.lineTo(r * 0.4, 0); ctx.lineTo(r * 0.88, r * 0.72 - flap * r); ctx.closePath(); ctx.fill();
  ctx.strokeStyle = 'rgba(255,255,255,0.3)'; ctx.lineWidth = 1;
  ctx.beginPath(); ctx.moveTo(-r * 0.06, -r * 0.1); ctx.lineTo(-r * 0.75, -r * 0.62); ctx.moveTo(r * 0.06, -r * 0.1); ctx.lineTo(r * 0.75, -r * 0.62); ctx.stroke();
  drawInsectEye(ctx, r * 0.12, -r * 0.2, Math.max(1.5, r * 0.08), '#ffcf55');
  drawInsectEye(ctx, r * 0.12, r * 0.2, Math.max(1.5, r * 0.08), '#ffcf55');
}

function drawModernEnemy(ctx: CanvasRenderingContext2D, e: EnemyEntity): void {
  const t = Date.now() / 1000;
  const facing = e.isBoss ? 0 : getEnemyFacingAngle(e);
  const color = e.freezeTimer > 0 ? '#69d6ff' : e.color;
  const hit = Math.max(0, Math.min(1, e.hitFlash / 0.15));

  ctx.save();
  ctx.translate(e.pos.x, e.pos.y);
  ctx.rotate(facing);
  if (hit > 0) {
    ctx.globalAlpha = 0.92 + hit * 0.08;
    ctx.scale(1 + hit * 0.07, 1 + hit * 0.07);
  }

  glowCircle(ctx, e.radius * (e.isBoss ? 1.8 : 1.25), color, e.isBoss ? 0.22 : 0.10);

  if (e.isBoss) {
    drawModernBossBody(ctx, e, color, t);
  } else if (e.type === 'fast') {
    drawVoidMantis(ctx, e.radius, color, t);
  } else if (e.type === 'tank') {
    drawVoidBeetle(ctx, e.radius * 1.08, color, t);
  } else if (e.shape === 'triangle') {
    drawVoidMoth(ctx, e.radius, color, t);
  } else {
    drawVoidSkitter(ctx, e.radius, color, t);
  }

  // Status and elite geometry.
  if (e.tier > 0) {
    ctx.save();
    ctx.rotate(t * (e.tier >= 3 ? -0.8 : 0.45));
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.62)`;
    ctx.lineWidth = e.tier >= 2 ? 1.6 : 1;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 5 + e.tier * 2, 0, Math.PI * 2); ctx.stroke();
    if (e.tier >= 3) {
      ctx.beginPath(); ctx.moveTo(0, -e.radius - 8); ctx.lineTo(0, -e.radius - 15);
      ctx.moveTo(0, e.radius + 8); ctx.lineTo(0, e.radius + 15); ctx.stroke();
    }
    ctx.restore();
  }
  if (e.isElite) {
    ctx.strokeStyle = '#d879ff'; ctx.lineWidth = 1.5;
    ctx.globalAlpha = 0.75 + Math.sin(t * 5) * 0.2;
    ctx.setLineDash([3, 5]);
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 9, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]);
    ctx.globalAlpha = 1;
  }
  if (e.fireTimer > 0) {
    ctx.strokeStyle = '#ff7048'; ctx.lineWidth = 2; ctx.globalAlpha = 0.7;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 3 + Math.sin(t * 8) * 2, 0, Math.PI * 2); ctx.stroke();
    ctx.globalAlpha = 1;
  }
  if (e.poisonTimer > 0) {
    ctx.strokeStyle = '#65e38f'; ctx.lineWidth = 1.8; ctx.globalAlpha = 0.7;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 4, 0, Math.PI * 2); ctx.stroke();
    ctx.globalAlpha = 1;
  }
  if (e.freezeTimer > 0) {
    ctx.strokeStyle = '#8ce9ff'; ctx.lineWidth = 1.2; ctx.globalAlpha = 0.75;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 6, 0, Math.PI * 2); ctx.stroke();
    ctx.globalAlpha = 1;
  }
  if (hit > 0) {
    ctx.strokeStyle = '#ffffff'; ctx.lineWidth = 2;
    ctx.globalAlpha = hit * 0.8;
    ctx.beginPath(); ctx.arc(0, 0, e.radius + 4, 0, Math.PI * 2); ctx.stroke();
    ctx.globalAlpha = 1;
  }
  ctx.restore();

  if (e.isBoss) {
    const barW = Math.max(90, e.radius * 1.45);
    const barH = 6;
    const ratio = Math.max(0, Math.min(1, e.hp / Math.max(1, e.maxHp)));
    ctx.fillStyle = 'rgba(0,0,0,0.55)';
    ctx.fillRect(e.pos.x - barW / 2, e.pos.y - e.radius - 28, barW, barH);
    ctx.fillStyle = color;
    ctx.fillRect(e.pos.x - barW / 2, e.pos.y - e.radius - 28, barW * ratio, barH);
    ctx.strokeStyle = 'rgba(190,220,255,0.45)';
    ctx.lineWidth = 1;
    ctx.strokeRect(e.pos.x - barW / 2, e.pos.y - e.radius - 28, barW, barH);
  }
}

function drawModernBossBody(ctx: CanvasRenderingContext2D, e: EnemyEntity, color: string, t: number): void {
  const r = e.radius;
  const boss = e.bossType;

  if (boss === 'charger') {
    // Massive horned beetle: low silhouette, armored plates, forward mandibles.
    ctx.strokeStyle = shade(color, -55); ctx.lineWidth = Math.max(3, r * 0.07); ctx.lineCap = 'round';
    for (let i = 0; i < 4; i++) {
      const x = -r * 0.55 + i * r * 0.36;
      insectLeg(ctx, r, x, -1, t * 6 + i, 0.92);
      insectLeg(ctx, r, x, 1, t * 6 + i + Math.PI, 0.92);
    }
    ctx.lineCap = 'butt';
    ctx.fillStyle = shade(color, -50);
    ctx.beginPath(); ctx.ellipse(-r * 0.1, 0, r * 0.9, r * 0.62, 0, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = color;
    ctx.beginPath(); ctx.ellipse(-r * 0.08, 0, r * 0.78, r * 0.52, 0, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = shade(color, 24);
    ctx.beginPath(); ctx.moveTo(-r * 0.62, -r * 0.45); ctx.lineTo(r * 0.52, -r * 0.36); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
    ctx.beginPath(); ctx.moveTo(-r * 0.62, r * 0.45); ctx.lineTo(r * 0.52, r * 0.36); ctx.lineTo(0, 0); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = 'rgba(255,255,255,0.3)'; ctx.lineWidth = Math.max(1, r * 0.025);
    ctx.beginPath(); ctx.moveTo(-r * 0.55, 0); ctx.lineTo(r * 0.55, 0); ctx.stroke();
    ctx.strokeStyle = color; ctx.lineWidth = Math.max(2, r * 0.045);
    ctx.beginPath(); ctx.moveTo(r * 0.4, -r * 0.12); ctx.quadraticCurveTo(r * 0.85, -r * 0.55, r * 0.72, -r * 0.9); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(r * 0.4, r * 0.12); ctx.quadraticCurveTo(r * 0.85, r * 0.55, r * 0.72, r * 0.9); ctx.stroke();
    drawInsectEye(ctx, r * 0.62, -r * 0.15, Math.max(3, r * 0.08), '#ff5b48');
    drawInsectEye(ctx, r * 0.62, r * 0.15, Math.max(3, r * 0.08), '#ff5b48');
  } else if (boss === 'shooter') {
    // Void moth: huge wings, a luminous thorax and multiple eyes.
    const flap = Math.sin(t * 4.5) * r * 0.08;
    ctx.fillStyle = shade(color, -30);
    ctx.beginPath(); ctx.ellipse(0, 0, r * 0.23, r * 0.7, 0, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = shade(color, 18);
    ctx.beginPath(); ctx.moveTo(-r * 0.08, -r * 0.08); ctx.quadraticCurveTo(-r * 1.2, -r * 0.8 - flap, -r * 1.05, -r * 0.05); ctx.quadraticCurveTo(-r * 0.9, r * 0.3, -r * 0.08, r * 0.12); ctx.closePath(); ctx.fill();
    ctx.beginPath(); ctx.moveTo(r * 0.08, -r * 0.08); ctx.quadraticCurveTo(r * 1.2, -r * 0.8 + flap, r * 1.05, -r * 0.05); ctx.quadraticCurveTo(r * 0.9, r * 0.3, r * 0.08, r * 0.12); ctx.closePath(); ctx.fill();
    ctx.strokeStyle = 'rgba(255,255,255,0.28)'; ctx.lineWidth = Math.max(1, r * 0.022);
    for (let i = 0; i < 3; i++) {
      ctx.beginPath(); ctx.moveTo(-r * 0.12, -r * 0.15); ctx.lineTo(-r * (0.45 + i * 0.22), -r * (0.25 + i * 0.18)); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(r * 0.12, -r * 0.15); ctx.lineTo(r * (0.45 + i * 0.22), -r * (0.25 + i * 0.18)); ctx.stroke();
    }
    ctx.fillStyle = color;
    ctx.beginPath(); ctx.ellipse(0, 0, r * 0.3, r * 0.58, 0, 0, Math.PI * 2); ctx.fill();
    for (const y of [-0.25, 0, 0.25]) drawInsectEye(ctx, r * 0.22, y * r, Math.max(2.4, r * 0.055), '#ffe06b');
  } else if (boss === 'summoner') {
    // Brood queen: spider-like abdomen, crown mandibles and orbiting eggs.
    ctx.fillStyle = shade(color, -48);
    ctx.beginPath(); ctx.ellipse(-r * 0.12, r * 0.12, r * 0.72, r * 0.68, 0, 0, Math.PI * 2); ctx.fill();
    ctx.fillStyle = color;
    ctx.beginPath(); ctx.ellipse(-r * 0.08, r * 0.08, r * 0.58, r * 0.56, 0, 0, Math.PI * 2); ctx.fill();
    ctx.strokeStyle = shade(color, -52); ctx.lineWidth = Math.max(2, r * 0.045); ctx.lineCap = 'round';
    for (let i = 0; i < 4; i++) {
      const a = -1.15 + i * 0.76;
      ctx.beginPath(); ctx.moveTo(-r * 0.35, -r * 0.1); ctx.lineTo(Math.cos(a) * r * 1.05, Math.sin(a) * r * 1.05); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(-r * 0.35, r * 0.1); ctx.lineTo(Math.cos(-a) * r * 1.05, Math.sin(-a) * r * 1.05); ctx.stroke();
    }
    ctx.lineCap = 'butt';
    ctx.fillStyle = shade(color, 26);
    ctx.beginPath(); ctx.moveTo(r * 0.3, -r * 0.35); ctx.lineTo(r * 0.78, -r * 0.15); ctx.lineTo(r * 0.3, 0); ctx.closePath(); ctx.fill();
    ctx.beginPath(); ctx.moveTo(r * 0.3, r * 0.35); ctx.lineTo(r * 0.78, r * 0.15); ctx.lineTo(r * 0.3, 0); ctx.closePath(); ctx.fill();
    for (let i = 0; i < 4; i++) {
      const a = t * 0.8 + i * Math.PI / 2;
      ctx.fillStyle = i % 2 ? '#d879ff' : color;
      ctx.beginPath(); ctx.arc(Math.cos(a) * r * 0.92, Math.sin(a) * r * 0.92, Math.max(4, r * 0.07), 0, Math.PI * 2); ctx.fill();
    }
    drawInsectEye(ctx, r * 0.55, -r * 0.12, Math.max(3, r * 0.07), '#d879ff');
    drawInsectEye(ctx, r * 0.55, r * 0.12, Math.max(3, r * 0.07), '#d879ff');
  } else {
    // Aura boss: enormous spider with a glowing core and eight articulated legs.
    const coreR = r * 0.42;
    ctx.strokeStyle = shade(color, -55); ctx.lineWidth = Math.max(2.5, r * 0.055); ctx.lineCap = 'round';
    for (let i = 0; i < 8; i++) {
      const a = i * Math.PI / 4 + Math.sin(t * 1.8 + i) * 0.06;
      const x0 = Math.cos(a) * coreR * 0.65, y0 = Math.sin(a) * coreR * 0.65;
      const x1 = Math.cos(a) * r * 0.8, y1 = Math.sin(a) * r * 0.8;
      const bend = a + (i % 2 ? 0.28 : -0.28);
      ctx.beginPath(); ctx.moveTo(x0, y0);
      ctx.lineTo(Math.cos(bend) * r * 0.58, Math.sin(bend) * r * 0.58);
      ctx.lineTo(x1, y1); ctx.stroke();
    }
    ctx.lineCap = 'butt';
    ctx.fillStyle = shade(color, -42);
    ctx.beginPath(); ctx.arc(0, 0, coreR * 1.15, 0, Math.PI * 2); ctx.fill();
    const g = ctx.createRadialGradient(-r * 0.1, -r * 0.12, 2, 0, 0, coreR);
    g.addColorStop(0, '#ffffff'); g.addColorStop(0.18, shade(color, 50)); g.addColorStop(0.7, color); g.addColorStop(1, shade(color, -30));
    ctx.fillStyle = g; ctx.beginPath(); ctx.arc(0, 0, coreR, 0, Math.PI * 2); ctx.fill();
    ctx.strokeStyle = 'rgba(255,255,255,0.55)'; ctx.lineWidth = Math.max(1, r * 0.02);
    ctx.beginPath(); ctx.arc(0, 0, coreR * 0.7, 0, Math.PI * 2); ctx.stroke();
    for (let i = 0; i < 4; i++) drawInsectEye(ctx, Math.cos(i * Math.PI / 2) * coreR * 0.62, Math.sin(i * Math.PI / 2) * coreR * 0.62, Math.max(2.5, r * 0.045), '#ff67b7');
  }

  if (e.isCharging) {
    ctx.strokeStyle = '#ff4d58'; ctx.lineWidth = Math.max(2, r * 0.025);
    ctx.globalAlpha = 0.85;
    ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(e.chargeDir.x * r * 1.5, e.chargeDir.y * r * 1.5); ctx.stroke();
    ctx.globalAlpha = 1;
  }

  if (e.bossType === 'aura' && e.auraRadius) {
    ctx.strokeStyle = `rgba(${hexToRgb(color)},0.16)`;
    ctx.lineWidth = 1.5;
    ctx.setLineDash([14, 9]);
    ctx.beginPath(); ctx.arc(0, 0, e.auraRadius, 0, Math.PI * 2); ctx.stroke();
    ctx.setLineDash([]);
  }

  // Boss core pulse.
  const p = 0.5 + Math.sin(t * 3.2) * 0.5;
  ctx.strokeStyle = `rgba(255,255,255,${0.18 + p * 0.22})`;
  ctx.lineWidth = 1.5;
  ctx.beginPath(); ctx.arc(0, 0, r + 8 + p * 5, 0, Math.PI * 2); ctx.stroke();
}

// Override the world draw calls without touching gameplay simulation.
const renderLegacyWorld = render;
void renderLegacyWorld;

function drawLightning(ctx: CanvasRenderingContext2D, from: { x: number; y: number }, to: { x: number; y: number }): void {
  const segments = 8;
  ctx.beginPath(); ctx.moveTo(from.x, from.y);
  for (let i = 1; i < segments; i++) { const t = i / segments; const x = from.x + (to.x - from.x) * t + (Math.random() - 0.5) * 25; const y = from.y + (to.y - from.y) * t + (Math.random() - 0.5) * 25; ctx.lineTo(x, y); }
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
