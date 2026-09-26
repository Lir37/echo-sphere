import type { GameState, PlayerState, SphereEntity, EnemyEntity, DamageNumber, ChestEntity } from './engine';
import { PLAYER_RADIUS, getMaxSpheres } from './engine';
import { SPHERE_TYPES, BOSS_TYPES } from './gameData';
import type { MapTheme, Vec } from './engine';
import { CHARACTER_DEFS } from './characters';
import { getCharacterId, getCharacterFormation, getEngineerNetworkRange } from './characterRuntime';
import { analyzeSphereNetwork, type SphereNetworkState } from './network';
import { buildRuntimeNetworkNodes } from './networkRuntime';
import { RUNE_DEFS } from './runes';
import { BOSS_TELEGRAPH_WINDOWS } from './bossBalance';
import { LINK_BREAKER_TELEGRAPH_SECONDS, LINK_BREAKER_DISABLED_SECONDS } from './eliteBalance';

// ===== Origami / Paper Craft Style =====
// Warm backgrounds, faceted folded-paper shapes, fold lines, drop shadows.

const INK = '#eaf6ff';
const FOLD_LINE = 'rgba(120,190,255,0.16)';
const VOID_BG = '#02040b';
const VOID_PANEL = '#080d1b';

const MUTATION_COLORS = ['#6eeaff', '#9b7cff', '#d86cff', '#55e6c1', '#b9a7ff'];

type ArtKey =
  | 'player'
  | 'sphere-standard' | 'sphere-sniper' | 'sphere-shotgun' | 'sphere-chain' | 'sphere-aura'
  | 'sphere-orbital' | 'sphere-prism' | 'sphere-gravity' | 'sphere-pulse' | 'sphere-void'
  | 'enemy-skitter' | 'enemy-fast' | 'enemy-tank' | 'enemy-moth'
  | 'boss';

const ART_PATHS: Record<ArtKey, string> = {
  player: '/art/player.svg',
  'sphere-standard': '/art/standard.svg',
  'sphere-sniper': '/art/sniper.svg',
  'sphere-shotgun': '/art/shotgun.svg',
  'sphere-chain': '/art/chain.svg',
  'sphere-aura': '/art/aura.svg',
  'sphere-orbital': '/art/orbital.svg',
  'sphere-prism': '/art/prism.svg',
  'sphere-gravity': '/art/gravity.svg',
  'sphere-pulse': '/art/pulse.svg',
  'sphere-void': '/art/void.svg',
  'enemy-skitter': '/art/enemy-skitter.svg',
  'enemy-fast': '/art/enemy-fast.svg',
  'enemy-tank': '/art/enemy-tank.svg',
  'enemy-moth': '/art/enemy-moth.svg',
  boss: '/art/boss.svg',
};

const ART_CACHE = new Map<ArtKey, HTMLImageElement>();

function getReferenceArt(key: ArtKey): HTMLImageElement | null {
  const cached = ART_CACHE.get(key);
  if (cached) return cached.complete && cached.naturalWidth > 0 ? cached : null;

  const image = new Image();
  image.decoding = 'async';
  image.src = ART_PATHS[key];
  ART_CACHE.set(key, image);
  return null;
}

function drawReferenceSprite(
  ctx: CanvasRenderingContext2D,
  key: ArtKey,
  x: number,
  y: number,
  size: number,
  color: string,
  rotation = 0,
  opacity = 1,
): boolean {
  const image = getReferenceArt(key);
  if (!image) return false;

  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(rotation);
  ctx.globalAlpha = opacity;

  // The artwork carries the silhouette. Canvas only supplies a restrained light halo.
  ctx.shadowColor = color;
  ctx.shadowBlur = Math.max(4, size * 0.09);
  ctx.drawImage(image, -size / 2, -size / 2, size, size);
  ctx.shadowBlur = 0;

  ctx.restore();
  return true;
}

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

  drawVoidField(ctx, s.worldWidth, s.worldHeight, theme, -s.worldWidth / 2, -s.worldHeight / 2, s.player.pos.x, s.player.pos.y);

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
    const alpha = Math.max(0, Math.min(1, ft.life / ft.maxLife));
    ctx.save();
    ctx.translate(ft.pos.x, ft.pos.y);
    glowCircle(ctx, 34, '#ff613d', alpha * 0.24);
    ctx.fillStyle = `rgba(255,91,56,${alpha * 0.16})`;
    ctx.beginPath(); ctx.arc(0, 0, 22, 0, Math.PI * 2); ctx.fill();
    ctx.strokeStyle = `rgba(255,180,70,${alpha * 0.75})`;
    ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.arc(0, 0, 15 + Math.sin(Date.now() * 0.02 + ft.pos.x) * 2, 0, Math.PI * 2); ctx.stroke();
    ctx.restore();
  }

  // The network is part of the battlefield, not a hidden calculation. Draw it
  // before the Spheres so links stay behind the authored sphere silhouettes.
  drawFormationMemory(ctx, s);
  drawSphereNetwork(ctx, s, analyzeSphereNetwork(buildRuntimeNetworkNodes(s.spheres, s.minions, (s.player.abilities.minion || 0) >= 3)));

function drawRune(ctx: CanvasRenderingContext2D, rune: GameState['runes'][number]): void {
  const def=RUNE_DEFS[rune.type], t=Date.now()/1000, pulse=1+Math.sin(t*4+rune.pos.x*.01)*.08;
  ctx.save();ctx.translate(rune.pos.x,rune.pos.y);ctx.scale(pulse,pulse);ctx.globalCompositeOperation='lighter';
  ctx.shadowColor=def.color;ctx.shadowBlur=18;
  ctx.fillStyle='rgba(5,12,22,.92)';ctx.strokeStyle=def.color;ctx.lineWidth=1.6;
  ctx.beginPath();ctx.moveTo(0,-15);ctx.lineTo(11,-7);ctx.lineTo(13,7);ctx.lineTo(0,15);ctx.lineTo(-13,7);ctx.lineTo(-11,-7);ctx.closePath();ctx.fill();ctx.stroke();
  for(let i=0;i<3;i++){const a=t*.9+i*Math.PI*2/3;ctx.strokeStyle=`rgba(255,255,255,${.28+.12*Math.sin(t*3+i)})`;ctx.beginPath();ctx.moveTo(Math.cos(a)*3,Math.sin(a)*3);ctx.lineTo(Math.cos(a)*10,Math.sin(a)*7);ctx.stroke();}
  ctx.fillStyle=def.color;ctx.beginPath();ctx.arc(0,0,3.2,0,Math.PI*2);ctx.fill();ctx.restore();
}

  // pickups
  for (const orb of s.xpOrbs) drawModernXp(ctx, orb.pos.x, orb.pos.y, orb.radius, '#63e6ff');
  for (const hp of s.healthPacks) drawModernHealth(ctx, hp.pos.x, hp.pos.y, '#ff5c72');
  for (const rune of s.runes) if (rune.alive) drawRune(ctx, rune);

  // chests
  for (const chest of s.chests) if (chest.alive) drawChest(ctx, chest);
  for (const chest of s.stellaChests) if (chest.alive) drawStellaChest(ctx, chest);

  // spheres
  for (const sphere of s.spheres) drawModernSphere(ctx, s, sphere);

  // sphere projectiles
  for (const p of s.sphereProjectiles) drawModernProjectile(ctx, p.pos.x, p.pos.y, p.vel.x, p.vel.y, p.radius, p.color);

  // Minion identity follows its anchor Sphere.
  for (const m of s.minions) drawModernMinion(ctx, m.pos.x, m.pos.y, m.radius, m.rotation, SPHERE_TYPES[m.anchorType]?.color || '#ffb84d');

  // enemies
  for (const e of s.enemies) drawModernEnemy(ctx, e, s.player.pos);

  drawPlayerShield(ctx, s);

  // Player is part of the same Canvas/2.5D visual family as the Spheres.
  // It is deliberately rendered after enemies so the hero remains readable.
  drawPlayer(ctx, s.player);

  // hunter mark and alchemist reaction indicators sit above enemies
  drawCharacterTargetIndicators(ctx, s);

  // boss projectiles
  for (const e of s.enemies) for (const bp of e.bossProjectiles) drawBossProjectile(ctx, bp.pos.x, bp.pos.y, bp.radius, e.color);


  // particles
  for (const p of s.particles) {
    const alpha = p.life / p.maxLife;
    const rgb = hexToRgb(p.color);
    ctx.fillStyle = `rgba(${rgb},${alpha * 0.82})`;
    ctx.shadowColor = p.color;
    ctx.shadowBlur = 7;
    ctx.save();
    ctx.translate(p.pos.x, p.pos.y);
    const s2 = p.size * (0.55 + alpha * 0.65);
    ctx.rotate(Math.atan2(p.vel?.y || 0, p.vel?.x || 1));
    ctx.beginPath();
    ctx.moveTo(s2 * 1.8, 0);
    ctx.lineTo(0, -s2 * 0.55);
    ctx.lineTo(-s2 * 1.2, 0);
    ctx.lineTo(0, s2 * 0.55);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
    ctx.shadowBlur = 0;
  }
  ctx.globalAlpha = 1;

  // damage numbers
  for (const dn of s.damageNumbers) {
    const alpha = Math.min(1, dn.life / dn.maxLife * 1.5);
    ctx.globalAlpha = alpha;
    ctx.fillStyle = dn.crit ? '#ffd166' : '#e8f6ff';
    ctx.shadowColor = dn.crit ? '#ff7a3d' : '#39d8ff';
    ctx.shadowBlur = dn.crit ? 12 : 7;
    ctx.font = `bold ${dn.crit ? 20 : 14}px system-ui, sans-serif`;
    ctx.textAlign = 'center';
    ctx.fillText(String(dn.value), dn.pos.x, dn.pos.y);
    ctx.shadowBlur = 0;
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
    ctx.translate(ax, ay);
    ctx.rotate(Math.atan2(s.bossArrow.y, s.bossArrow.x));
    ctx.shadowColor = '#ff4d70';
    ctx.shadowBlur = 14;
    ctx.fillStyle = '#ff5c79';
    ctx.beginPath();
    ctx.moveTo(16, 0); ctx.lineTo(-6, -9); ctx.lineTo(-2, 0); ctx.lineTo(-6, 9);
    ctx.closePath(); ctx.fill();
    ctx.shadowBlur = 0;
    ctx.strokeStyle = '#ffdbe4';
    ctx.lineWidth = 1;
    ctx.stroke();
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

function drawCharacterHud(_ctx: CanvasRenderingContext2D, _s: GameState, _canvasW: number, _canvasH: number): void {
  // Character state is communicated by world indicators and the main HUD.
}

function drawPlayerShield(ctx: CanvasRenderingContext2D, s: GameState): void {
  const charges = s.player.shieldCharges || 0;
  if (charges <= 0 || s.player.shieldTimer <= 0) return;
  const pulse = 1 + Math.sin(Date.now() * 0.008) * 0.035;
  ctx.save(); ctx.translate(s.player.pos.x, s.player.pos.y); ctx.scale(pulse, pulse);
  ctx.shadowColor = '#63e6ff'; ctx.shadowBlur = 18; ctx.strokeStyle = 'rgba(99,230,255,0.72)';
  ctx.lineWidth = 2.2; ctx.setLineDash([10,7]); ctx.beginPath(); ctx.arc(0,0,34,0,Math.PI*2); ctx.stroke(); ctx.setLineDash([]);
  ctx.strokeStyle = 'rgba(225,251,255,0.82)'; ctx.lineWidth = 1;
  for(let i=0;i<charges;i++){const a=-Math.PI/2+i*Math.PI*2/Math.max(1,charges);ctx.beginPath();ctx.moveTo(Math.cos(a)*29,Math.sin(a)*29);ctx.lineTo(Math.cos(a)*38,Math.sin(a)*38);ctx.stroke();}
  ctx.shadowBlur=0; ctx.fillStyle='#dffaff'; ctx.font='bold 9px system-ui,sans-serif'; ctx.textAlign='center'; ctx.fillText(String(charges),0,3); ctx.restore();
}

function drawStellaChest(ctx: CanvasRenderingContext2D, chest: ChestEntity): void {
  const t=Date.now()/1000,p=1+Math.sin(t*2.6)*.08,rot=t*.25;
  ctx.save();ctx.translate(chest.pos.x,chest.pos.y);ctx.scale(p,p);ctx.globalCompositeOperation='lighter';drawGroundShadow(ctx,24,7,7);
  ctx.shadowColor='#ffb84d';ctx.shadowBlur=28;ctx.fillStyle='rgba(24,12,2,.96)';ctx.strokeStyle='#ffb84d';ctx.lineWidth=2;
  ctx.beginPath();ctx.moveTo(0,-23);ctx.lineTo(18,-9);ctx.lineTo(20,10);ctx.lineTo(0,23);ctx.lineTo(-20,10);ctx.lineTo(-18,-9);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.save();ctx.rotate(rot);ctx.strokeStyle='#fff0bf';ctx.lineWidth=1.4;for(let i=0;i<4;i++){ctx.beginPath();ctx.moveTo(-17+i*4,-8);ctx.lineTo(17-i*4,8);ctx.stroke();}ctx.restore();
  ctx.fillStyle='#fff8df';ctx.beginPath();ctx.arc(0,0,5,0,Math.PI*2);ctx.fill();
  ctx.font='bold 8px system-ui,sans-serif';ctx.textAlign='center';ctx.fillStyle='#ffd27e';ctx.fillText('STELLA',0,37);ctx.restore();
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

function drawVoidField(ctx: CanvasRenderingContext2D, w: number, h: number, theme: Theme, ox = 0, oy = 0, focusX = 0, focusY = 0): void {
  const cx = focusX;
  const cy = focusY;
  const outer = Math.max(w, h);
  const g = ctx.createRadialGradient(cx, cy - h * 0.14, 10, cx, cy, outer * 0.78);
  g.addColorStop(0, 'rgba(41,84,142,0.18)');
  g.addColorStop(0.26, 'rgba(58,37,121,0.10)');
  g.addColorStop(0.62, 'rgba(3,10,23,0.18)');
  g.addColorStop(1, 'rgba(0,0,0,0.82)');
  ctx.fillStyle = g; ctx.fillRect(ox, oy, w, h);

  ctx.save();
  const nebulae: Array<[number,number,number,string]> = [
    [0.18,0.30,0.24,'rgba(44,168,255,0.055)'],
    [0.76,0.28,0.22,'rgba(178,75,255,0.045)'],
    [0.52,0.69,0.30,'rgba(70,117,255,0.045)'],
  ];
  for (const [nx,ny,nr,color] of nebulae) {
    const x=ox+w*nx,y=oy+h*ny,rg=ctx.createRadialGradient(x,y,0,x,y,w*nr);
    rg.addColorStop(0,color); rg.addColorStop(1,'rgba(0,0,0,0)');
    ctx.fillStyle=rg;ctx.beginPath();ctx.arc(x,y,w*nr,0,Math.PI*2);ctx.fill();
  }
  ctx.restore();

  // Circular resonance arena, matching the reference's layered combat geometry.\n  ctx.save();\n  ctx.translate(cx, cy);\n  const arenaRadius = outer * 0.43;\n  const coreGlow = ctx.createRadialGradient(0, 0, 0, 0, 0, arenaRadius);\n  coreGlow.addColorStop(0, 'rgba(75,190,255,0.16)');\n  coreGlow.addColorStop(0.16, 'rgba(79,111,255,0.08)');\n  coreGlow.addColorStop(0.52, 'rgba(9,24,48,0.055)');\n  coreGlow.addColorStop(1, 'rgba(0,0,0,0)');\n  ctx.fillStyle = coreGlow;\n  ctx.beginPath(); ctx.arc(0, 0, arenaRadius, 0, Math.PI * 2); ctx.fill();\n  for (let i = 1; i <= 8; i++) {\n    const rr = arenaRadius * (i / 8);\n    ctx.strokeStyle = i === 1 ? 'rgba(119,224,255,0.16)' : 'rgba(102,202,255,0.065)';\n    ctx.lineWidth = i === 1 ? 1.2 : 0.8;\n    ctx.beginPath(); ctx.arc(0, 0, rr, 0, Math.PI * 2); ctx.stroke();\n  }\n  ctx.strokeStyle = 'rgba(130,218,255,0.055)';\n  ctx.lineWidth = 0.8;\n  for (let i = 0; i < 20; i++) {\n    const a = (Math.PI * 2 * i) / 20;\n    const inner = arenaRadius * 0.08;\n    ctx.beginPath();\n    ctx.moveTo(Math.cos(a) * inner, Math.sin(a) * inner);\n    ctx.lineTo(Math.cos(a) * arenaRadius, Math.sin(a) * arenaRadius);\n    ctx.stroke();\n  }\n  const arcs = [\n    { radius: arenaRadius * 0.38, start: -0.35, end: 2.15, color: 'rgba(92,223,255,0.20)' },\n    { radius: arenaRadius * 0.62, start: 1.55, end: 4.70, color: 'rgba(166,112,255,0.16)' },\n    { radius: arenaRadius * 0.82, start: -2.35, end: 0.55, color: 'rgba(255,119,188,0.12)' },\n  ];\n  for (const arc of arcs) {\n    ctx.strokeStyle = arc.color;\n    ctx.lineWidth = 1.1;\n    ctx.beginPath(); ctx.arc(0, 0, arc.radius, arc.start, arc.end); ctx.stroke();\n  }\n  ctx.restore();\n\n  // A few near-field plates create scale and depth without becoming a grid.
  ctx.save();
  ctx.strokeStyle='rgba(96,184,255,0.032)';
  ctx.lineWidth=1;
  for(let i=1;i<5;i++){
    const y=cy+h*0.22+i*i*h*0.045;
    ctx.beginPath();ctx.moveTo(ox+w*.08,y);ctx.lineTo(ox+w*.92,y);ctx.stroke();
  }
  ctx.restore();

  ctx.save();
  const horizon=oy+h*0.40;ctx.strokeStyle='rgba(115,206,255,0.055)';ctx.lineWidth=1;
  for(let i=0;i<8;i++){const yy=horizon+Math.pow(i/8,1.9)*h*0.48;ctx.beginPath();ctx.moveTo(ox+w*0.06,yy);ctx.lineTo(ox+w*0.94,yy);ctx.stroke();}
  ctx.restore();

  ctx.save();
  for(let i=0;i<120;i++){
    const sx=ox+((i*137.31)%w),sy=oy+((i*71.93)%h),alpha=0.12+((i*17)%60)/600;
    ctx.fillStyle=`rgba(190,230,255,${alpha})`;ctx.beginPath();ctx.arc(sx,sy,0.35+(i%3)*0.18,0,Math.PI*2);ctx.fill();
  }
  ctx.restore();
}

function drawModernXp(ctx: CanvasRenderingContext2D,x:number,y:number,r:number,color:string):void{
  const t=Date.now()/1000,rot=t*1.4+(x+y)*.002,rgb=hexToRgb(color);
  ctx.save();ctx.translate(x,y);ctx.rotate(rot);ctx.globalCompositeOperation='lighter';ctx.shadowColor=color;ctx.shadowBlur=14;
  const g=ctx.createLinearGradient(-r,-r,r,r);g.addColorStop(0,'#fff');g.addColorStop(.24,color);g.addColorStop(.72,`rgba(${rgb},.40)`);g.addColorStop(1,'#06101e');ctx.fillStyle=g;
  ctx.beginPath();ctx.moveTo(0,-r*1.1);ctx.lineTo(r*.62,-r*.35);ctx.lineTo(r*.52,r*.68);ctx.lineTo(0,r*1.05);ctx.lineTo(-r*.52,r*.68);ctx.lineTo(-r*.62,-r*.35);ctx.closePath();ctx.fill();
  ctx.strokeStyle='rgba(230,253,255,.72)';ctx.lineWidth=Math.max(.7,r*.10);ctx.stroke();
  ctx.strokeStyle=`rgba(${rgb},.40)`;ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(-r*.35,-r*.55);ctx.lineTo(r*.30,r*.58);ctx.stroke();ctx.restore();
}

function drawModernHealth(ctx: CanvasRenderingContext2D,x:number,y:number,color:string):void{
  const t=Date.now()/1000,p=1+Math.sin(t*5)*.07;
  ctx.save();ctx.translate(x,y);ctx.scale(p,p);ctx.globalCompositeOperation='lighter';ctx.shadowColor=color;ctx.shadowBlur=15;
  ctx.fillStyle='rgba(7,13,22,.94)';ctx.strokeStyle='rgba(255,125,150,.90)';ctx.lineWidth=1.4;
  ctx.beginPath();ctx.moveTo(-4,-12);ctx.lineTo(4,-12);ctx.lineTo(4,-4);ctx.lineTo(12,-4);ctx.lineTo(12,4);ctx.lineTo(4,4);ctx.lineTo(4,12);ctx.lineTo(-4,12);ctx.lineTo(-4,4);ctx.lineTo(-12,4);ctx.lineTo(-12,-4);ctx.lineTo(-4,-4);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='#fff';ctx.fillRect(-2,-7,4,14);ctx.fillRect(-7,-2,14,4);ctx.restore();
}

function drawModernProjectile(ctx: CanvasRenderingContext2D,x:number,y:number,vx:number,vy:number,r:number,color:string):void{
  const a=Math.atan2(vy,vx),speed=Math.hypot(vx,vy)||1,t=Date.now()/1000,trail=Math.min(52,15+speed*.05),rgb=hexToRgb(color);
  ctx.save();ctx.translate(x,y);ctx.rotate(a);ctx.globalCompositeOperation='lighter';
  ctx.strokeStyle=`rgba(${rgb},.18)`;ctx.lineWidth=Math.max(4,r*1.8);ctx.shadowColor=color;ctx.shadowBlur=16;ctx.beginPath();ctx.moveTo(-trail,0);ctx.lineTo(0,0);ctx.stroke();ctx.shadowBlur=0;
  ctx.fillStyle='#06111e';ctx.strokeStyle=`rgba(${rgb},.95)`;ctx.lineWidth=1;
  const pulse=1+Math.sin(t*12)*.08;
  ctx.beginPath();ctx.moveTo(r*2.2,0);ctx.lineTo(-r*.55,-r*.72*pulse);ctx.lineTo(-r*.85,0);ctx.lineTo(-r*.55,r*.72*pulse);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='#fff';ctx.beginPath();ctx.arc(0,0,r*.46,0,Math.PI*2);ctx.fill();
  ctx.fillStyle=color;ctx.beginPath();ctx.arc(0,0,r*.34,0,Math.PI*2);ctx.fill();ctx.restore();
}

function drawModernMinion(ctx: CanvasRenderingContext2D,x:number,y:number,r:number,rotation:number,color:string):void{
  const t=Date.now()/1000,bob=Math.sin(t*5+x*.01)*r*.08,rgb=hexToRgb(color);
  ctx.save();ctx.translate(x,y+bob);ctx.rotate(rotation+t*.12);ctx.globalCompositeOperation='lighter';drawGroundShadow(ctx,r*.72,r*.22,4);
  ctx.shadowColor=color;ctx.shadowBlur=11;ctx.fillStyle='#06101c';ctx.strokeStyle=`rgba(${rgb},.86)`;ctx.lineWidth=1.1;
  ctx.beginPath();ctx.moveTo(0,-r);ctx.lineTo(r*.72,-r*.34);ctx.lineTo(r*.52,r*.62);ctx.lineTo(0,r*.72);ctx.lineTo(-r*.52,r*.62);ctx.lineTo(-r*.72,-r*.34);ctx.closePath();ctx.fill();ctx.stroke();
  for(let i=0;i<3;i++){const a=t*1.7+i*Math.PI*2/3;ctx.strokeStyle=`rgba(${rgb},.42)`;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.25,Math.sin(a)*r*.20);ctx.lineTo(Math.cos(a)*r*.88,Math.sin(a)*r*.52);ctx.stroke();}
  ctx.shadowBlur=0;ctx.fillStyle='#fff';ctx.beginPath();ctx.arc(0,-r*.10,r*.24,0,Math.PI*2);ctx.fill();ctx.fillStyle=color;ctx.beginPath();ctx.arc(0,-r*.10,r*.16,0,Math.PI*2);ctx.fill();ctx.restore();
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
const _textureCanvases: Partial<Record<MapTheme, HTMLCanvasElement>> = {};
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
  const grid = 160;
  const startX = Math.floor((s.camera.x - canvasW / 2) / grid) * grid;
  const startY = Math.floor((s.camera.y - canvasH / 2) / grid) * grid;
  ctx.save();
  ctx.strokeStyle = 'rgba(85,130,255,0.045)';
  ctx.lineWidth = 1;
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
  ctx.restore();
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

// ===== Player — authored 2.5D core, no containment sphere =====
function drawPlayer(ctx: CanvasRenderingContext2D, p: PlayerState): void {
  const r = PLAYER_RADIUS;
  const id = getCharacterId({ player: p } as GameState);
  const color = CHARACTER_DEFS[id].color;
  const rgb = hexToRgb(color);
  const t = Date.now() / 1000;
  const bob = Math.sin(t * 2.4) * 0.8;
  const pulse = 1 + Math.sin(t * 5.5) * 0.025;

  ctx.save();
  ctx.translate(p.pos.x, p.pos.y - r * 0.1 + bob);
  ctx.scale(pulse, pulse);
  drawGroundShadow(ctx, r * .95, r * .25, 8);
  ctx.globalCompositeOperation = 'lighter';

  // Each character has a distinct folded silhouette while sharing the same 2.5D material language.
  ctx.shadowColor = color;
  ctx.shadowBlur = 14;
  ctx.fillStyle = '#071321';
  ctx.strokeStyle = `rgba(${rgb},.90)`;
  ctx.lineWidth = 1.5;

  if (id === 'hunter') {
    ctx.beginPath(); ctx.moveTo(0,-r*1.0); ctx.lineTo(r*.48,-r*.12); ctx.lineTo(r*.34,r*.72); ctx.lineTo(0,r*.44); ctx.lineTo(-r*.34,r*.72); ctx.lineTo(-r*.48,-r*.12); ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.strokeStyle=`rgba(${rgb},.46)`;ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(-r*.92,-r*.34);ctx.lineTo(0,-r*.72);ctx.lineTo(r*.92,-r*.34);ctx.stroke();
  } else if (id === 'engineer') {
    ctx.beginPath();ctx.moveTo(-r*.72,-r*.56);ctx.lineTo(r*.72,-r*.56);ctx.lineTo(r*.72,r*.56);ctx.lineTo(-r*.72,r*.56);ctx.closePath();ctx.fill();ctx.stroke();
    ctx.strokeStyle=`rgba(${rgb},.52)`;ctx.beginPath();ctx.moveTo(-r*.72,0);ctx.lineTo(r*.72,0);ctx.moveTo(0,-r*.56);ctx.lineTo(0,r*.56);ctx.stroke();
    for(let i=0;i<4;i++){const a=t*.6+i*Math.PI/2;ctx.fillStyle=color;ctx.fillRect(Math.cos(a)*r*.86-2,Math.sin(a)*r*.62-2,4,4);}
  } else if (id === 'berserker') {
    ctx.beginPath();ctx.moveTo(-r*.82,r*.40);ctx.lineTo(-r*.58,-r*.62);ctx.lineTo(-r*.18,-r*.92);ctx.lineTo(0,-r*.56);ctx.lineTo(r*.18,-r*.92);ctx.lineTo(r*.58,-r*.62);ctx.lineTo(r*.82,r*.40);ctx.lineTo(r*.34,r*.78);ctx.lineTo(-r*.34,r*.78);ctx.closePath();ctx.fill();ctx.stroke();
  } else if (id === 'alchemist') {
    ctx.beginPath();ctx.moveTo(-r*.48,-r*.82);ctx.lineTo(r*.48,-r*.82);ctx.lineTo(r*.62,-r*.28);ctx.lineTo(r*.42,r*.70);ctx.lineTo(0,r*.90);ctx.lineTo(-r*.42,r*.70);ctx.lineTo(-r*.62,-r*.28);ctx.closePath();ctx.fill();ctx.stroke();
    for(let i=0;i<3;i++){const a=t*.9+i*2.1;ctx.fillStyle=`rgba(${rgb},.55)`;ctx.beginPath();ctx.arc(Math.cos(a)*r*.9,Math.sin(a)*r*.45,2+i,0,Math.PI*2);ctx.fill();}
  } else if (id === 'architect') {
    ctx.beginPath();ctx.moveTo(0,-r*1.0);ctx.lineTo(r*.78,-r*.30);ctx.lineTo(r*.58,r*.72);ctx.lineTo(0,r*.46);ctx.lineTo(-r*.58,r*.72);ctx.lineTo(-r*.78,-r*.30);ctx.closePath();ctx.fill();ctx.stroke();
    ctx.strokeStyle=`rgba(${rgb},.52)`;ctx.lineWidth=1.1;ctx.beginPath();ctx.moveTo(0,-r);ctx.lineTo(0,r*.46);ctx.moveTo(-r*.78,-r*.30);ctx.lineTo(r*.58,r*.72);ctx.moveTo(r*.78,-r*.30);ctx.lineTo(-r*.58,r*.72);ctx.stroke();
  } else {
    ctx.beginPath();ctx.moveTo(0,-r*1.02);ctx.lineTo(r*.74,-r*.42);ctx.lineTo(r*.62,r*.48);ctx.lineTo(0,r*.78);ctx.lineTo(-r*.62,r*.48);ctx.lineTo(-r*.74,-r*.42);ctx.closePath();ctx.fill();ctx.stroke();
    for(let i=0;i<3;i++){const a=t*1.3+i*Math.PI*2/3;ctx.strokeStyle=`rgba(${rgb},.40)`;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.35,Math.sin(a)*r*.30);ctx.lineTo(Math.cos(a)*r*1.0,Math.sin(a)*r*.62);ctx.stroke();}
  }

  ctx.shadowBlur = 0;
  const core = ctx.createRadialGradient(-r*.18,-r*.20,1,0,0,r*.52);
  core.addColorStop(0,'#ffffff'); core.addColorStop(.28,color); core.addColorStop(.68,`rgba(${rgb},.55)`); core.addColorStop(1,'rgba(0,0,0,0)');
  ctx.fillStyle=core; ctx.beginPath();ctx.arc(0,0,r*.48,0,Math.PI*2);ctx.fill();
  ctx.restore();

  drawCharacterVfx(ctx,p);
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
function drawTowerPaperFrame(ctx:CanvasRenderingContext2D,sphere:SphereEntity,color:string,t:number):void{}
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

function drawGroundShadow(ctx:CanvasRenderingContext2D,rx:number,ry:number,blur:number):void{ctx.save();ctx.shadowColor='rgba(0,0,0,.48)';ctx.shadowBlur=blur;ctx.fillStyle='rgba(0,0,0,.34)';ctx.beginPath();ctx.ellipse(0,5,rx,ry,0,0,Math.PI*2);ctx.fill();ctx.restore();}
function drawSphereBase(ctx:CanvasRenderingContext2D,r:number,color:string):void{
  const rgb=hexToRgb(color);
  ctx.save();
  ctx.translate(0,r*.55);
  const base=ctx.createLinearGradient(0,-r*.20,0,r*.32);
  base.addColorStop(0,'#40576e');base.addColorStop(.25,'#172a3d');base.addColorStop(1,'#02050b');
  ctx.fillStyle=base;ctx.strokeStyle=`rgba(${rgb},.34)`;ctx.lineWidth=Math.max(1,r*.055);
  ctx.beginPath();ctx.ellipse(0,0,r*.78,r*.22,0,0,Math.PI*2);ctx.fill();ctx.stroke();
  ctx.fillStyle='rgba(170,220,255,.07)';
  ctx.beginPath();ctx.ellipse(-r*.16,-r*.08,r*.48,r*.10,0,0,Math.PI*2);ctx.fill();
  ctx.restore();
}


function drawSphereVfx(ctx: CanvasRenderingContext2D, sphere: SphereEntity, color: string, r: number, time: number): void {
  const rgb = hexToRgb(color);
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y);
  ctx.globalCompositeOperation = 'lighter';
  const pulse = 0.78 + Math.sin(time * 4.2 + sphere.pos.x * 0.01) * 0.22;

  if (sphere.type === 'standard') {
    for (let i = 0; i < 3; i++) {
      const a = time * 1.2 + i * Math.PI * 2 / 3;
      const x = Math.cos(a) * r * 1.35, y = Math.sin(a) * r * 0.72;
      ctx.fillStyle = `rgba(${rgb},${0.38 * pulse})`;
      ctx.beginPath(); ctx.moveTo(x, y - 3); ctx.lineTo(x + 3, y); ctx.lineTo(x, y + 3); ctx.lineTo(x - 3, y); ctx.closePath(); ctx.fill();
    }
  } else if (sphere.type === 'sniper') {
    const sweep = ((time * 0.9) % 1) * 2 - 1;
    ctx.strokeStyle = `rgba(${rgb},${0.30 * pulse})`;
    ctx.lineWidth = 1.2;
    ctx.beginPath(); ctx.moveTo(sweep * r * 1.7, -r * 0.75); ctx.lineTo(sweep * r * 1.7, r * 0.55); ctx.stroke();
    ctx.fillStyle = `rgba(${rgb},${0.20 * pulse})`;
    ctx.fillRect(r * 0.78, -2, r * 0.58, 4);
  } else if (sphere.type === 'shotgun') {
    for (let i = -1; i <= 1; i++) {
      ctx.fillStyle = `rgba(255,170,90,${0.30 * pulse})`;
      ctx.beginPath(); ctx.arc(r * 0.9, i * r * 0.23, r * 0.09 + pulse * 1.1, 0, Math.PI * 2); ctx.fill();
    }
  } else if (sphere.type === 'chain') {
    for (let i = 0; i < 3; i++) {
      const a = time * 2.8 + i * Math.PI * 2 / 3;
      const x = Math.cos(a) * r * 1.25, y = Math.sin(a) * r * 0.62;
      ctx.strokeStyle = `rgba(255,242,150,${0.45 * pulse})`;
      ctx.lineWidth = 1.4;
      ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(x, y); ctx.stroke();
      ctx.fillStyle = '#fff6b0'; ctx.beginPath(); ctx.arc(x, y, 2.1, 0, Math.PI * 2); ctx.fill();
    }
  } else if (sphere.type === 'aura') {
    for (let i = 0; i < 7; i++) {
      const a = time * 0.8 + i * Math.PI * 2 / 7;
      const rr = r * (1.0 + 0.18 * Math.sin(time * 2 + i));
      ctx.fillStyle = `rgba(87,230,180,${0.18 + 0.10 * pulse})`;
      ctx.beginPath(); ctx.arc(Math.cos(a) * rr, Math.sin(a) * rr * 0.45, 2.2, 0, Math.PI * 2); ctx.fill();
    }
  } else if (sphere.type === 'orbital') {
    const n = Math.max(1, 1 + (sphere.visualTier || 0));
    for (let i = 0; i < Math.min(n, 7); i++) {
      const a = sphere.rotation + time * 0.5 + i * Math.PI * 2 / Math.min(n, 7);
      ctx.fillStyle = `rgba(142,240,255,${0.18 + 0.08 * pulse})`;
      ctx.beginPath(); ctx.arc(Math.cos(a) * r * 2.1, Math.sin(a) * r * 1.05, 1.8, 0, Math.PI * 2); ctx.fill();
    }
  } else if (sphere.type === 'prism') {
    for (let i = 0; i < 3; i++) {
      const a = time * 1.5 + i * Math.PI * 2 / 3;
      ctx.strokeStyle = `rgba(255,141,225,${0.22 + 0.12 * pulse})`;
      ctx.lineWidth = 1;
      ctx.beginPath(); ctx.moveTo(Math.cos(a) * r * 0.35, Math.sin(a) * r * 0.35); ctx.lineTo(Math.cos(a) * r * 1.55, Math.sin(a) * r * 0.95); ctx.stroke();
    }
  } else if (sphere.type === 'gravity') {
    for (let i = 0; i < 8; i++) {
      const a = time * -0.7 + i * Math.PI * 2 / 8;
      const rr = r * (1.8 - ((time * 0.55 + i * 0.22) % 1) * 0.95);
      ctx.fillStyle = `rgba(165,140,255,${0.26 * pulse})`;
      ctx.beginPath(); ctx.arc(Math.cos(a) * rr, Math.sin(a) * rr * 0.55, 1.7, 0, Math.PI * 2); ctx.fill();
    }
  } else if (sphere.type === 'pulse') {
    const wave = ((time * 1.8) % 1);
    ctx.strokeStyle = `rgba(255,211,90,${(1 - wave) * 0.42})`;
    ctx.lineWidth = 1.4;
    ctx.beginPath(); ctx.ellipse(0, 0, r * (0.8 + wave * 1.9), r * (0.32 + wave * 0.78), 0, 0, Math.PI * 2); ctx.stroke();
  } else if (sphere.type === 'void') {
    for (let i = 0; i < 5; i++) {
      const a = time * -1.1 + i * Math.PI * 2 / 5;
      const rr = r * (1.0 + 0.55 * Math.sin(time * 1.7 + i));
      ctx.fillStyle = `rgba(194,140,255,${0.22 + 0.10 * pulse})`;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * rr, Math.sin(a) * rr * 0.55 - 4);
      ctx.lineTo(Math.cos(a + 0.18) * rr, Math.sin(a + 0.18) * rr * 0.55);
      ctx.lineTo(Math.cos(a) * rr, Math.sin(a) * rr * 0.55 + 4);
      ctx.closePath(); ctx.fill();
    }
  }
  ctx.restore();
}

function drawCharacterVfx(ctx: CanvasRenderingContext2D, p: PlayerState): void {
  const id = getCharacterId({ player: p } as GameState);
  const color = CHARACTER_DEFS[id].color;
  const rgb = hexToRgb(color);
  const t = Date.now() / 1000;
  const r = PLAYER_RADIUS;
  ctx.save();
  ctx.translate(p.pos.x, p.pos.y - r * 0.1);
  ctx.globalCompositeOperation = 'lighter';
  if (id === 'spherist') {
    for (let i = 0; i < 3; i++) {
      const a = t * 1.4 + i * Math.PI * 2 / 3;
      ctx.strokeStyle = `rgba(${rgb},.42)`; ctx.lineWidth = 1.3;
      ctx.beginPath(); ctx.moveTo(Math.cos(a) * r * .45, Math.sin(a) * r * .35); ctx.lineTo(Math.cos(a) * r * 1.15, Math.sin(a) * r * .72); ctx.stroke();
    }
  } else if (id === 'hunter') {
    const scan = Math.sin(t * 3.5) * .5 + .5;
    ctx.strokeStyle = `rgba(${rgb},${.25 + scan * .3})`; ctx.lineWidth = 1.2;
    ctx.beginPath(); ctx.moveTo(-r * .95, r * (.15 - scan * .25)); ctx.lineTo(r * .95, r * (.15 - scan * .25)); ctx.stroke();
  } else if (id === 'engineer') {
    for (let i = 0; i < 4; i++) {
      const a = t * .6 + i * Math.PI / 2;
      const x = Math.cos(a) * r * 1.05, y = Math.sin(a) * r * .72;
      ctx.fillStyle = `rgba(${rgb},.34)`; ctx.fillRect(x - 3, y - 3, 6, 6);
    }
  } else if (id === 'berserker') {
    const pulse = 1 + Math.sin(t * 7) * .08;
    ctx.strokeStyle = `rgba(${rgb},.58)`; ctx.lineWidth = 2;
    ctx.beginPath(); ctx.moveTo(-r * .9, r * .55 * pulse); ctx.lineTo(0, r * .85 * pulse); ctx.lineTo(r * .9, r * .55 * pulse); ctx.stroke();
  } else if (id === 'alchemist') {
    for (let i = 0; i < 5; i++) {
      const a = t * .8 + i * 1.2;
      const rr = r * (.85 + .15 * Math.sin(t * 2 + i));
      ctx.fillStyle = `rgba(${rgb},.38)`; ctx.beginPath(); ctx.arc(Math.cos(a) * rr, Math.sin(a) * rr * .6, 2.2 + (i % 2), 0, Math.PI * 2); ctx.fill();
    }
  } else if (id === 'architect') {
    for (let i = 0; i < 4; i++) {
      const a = t * .7 + i * Math.PI / 2;
      const x = Math.cos(a) * r * 1.15, y = Math.sin(a) * r * .72;
      ctx.strokeStyle = `rgba(${rgb},.46)`; ctx.lineWidth = 1.2;
      ctx.beginPath(); ctx.moveTo(x - 5, y); ctx.lineTo(x + 5, y); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(x, y - 5); ctx.lineTo(x, y + 5); ctx.stroke();
    }
  }
  ctx.restore();
}

function drawEnemySilhouette(ctx: CanvasRenderingContext2D, e: EnemyEntity, color: string, t: number): void {
  const r = e.radius;
  const rgb = hexToRgb(color);
  const v = e.visualVariant || (e.type === 'fast' ? 'moth' : e.type === 'tank' ? 'beetle' : 'wisp');
  const bob = Math.sin(t * (v === 'moth' ? 5 : 3) + e.pos.x * .01) * (v === 'moth' ? r * .10 : r * .04);
  ctx.save();
  ctx.translate(0, bob);
  ctx.shadowColor = color;
  ctx.shadowBlur = 10;
  ctx.fillStyle = '#071321';
  ctx.strokeStyle = `rgba(${rgb},.84)`;
  ctx.lineWidth = Math.max(1, r * .055);

  if (v === 'moth') {
    const flap = Math.sin(t * 7) * .18;
    for (const side of [-1,1]) {
      ctx.beginPath();
      ctx.moveTo(side*r*.12,0);
      ctx.quadraticCurveTo(side*r*(.70+flap),-r*.78,side*r*1.15,-r*.28);
      ctx.quadraticCurveTo(side*r*.78,r*.14,side*r*.16,r*.20);
      ctx.closePath(); ctx.fill(); ctx.stroke();
    }
    ctx.beginPath(); ctx.moveTo(0,-r*.75); ctx.lineTo(r*.22,-r*.12); ctx.lineTo(0,r*.72); ctx.lineTo(-r*.22,-r*.12); ctx.closePath(); ctx.fill(); ctx.stroke();
  } else if (v === 'beetle') {
    ctx.beginPath(); ctx.moveTo(0,-r); ctx.lineTo(r*.74,-r*.58); ctx.lineTo(r*.92,r*.18); ctx.lineTo(r*.52,r*.82); ctx.lineTo(0,r); ctx.lineTo(-r*.52,r*.82); ctx.lineTo(-r*.92,r*.18); ctx.lineTo(-r*.74,-r*.58); ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.strokeStyle = 'rgba(220,244,255,.24)';
    ctx.beginPath(); ctx.moveTo(-r*.55,-r*.45); ctx.lineTo(0,-r*.78); ctx.lineTo(r*.55,-r*.45); ctx.moveTo(-r*.64,r*.20); ctx.lineTo(0,r*.55); ctx.lineTo(r*.64,r*.20); ctx.stroke();
  } else if (v === 'brute') {
    ctx.beginPath(); ctx.moveTo(-r*.85,r*.40); ctx.lineTo(-r*.65,-r*.52); ctx.lineTo(-r*.22,-r*.82); ctx.lineTo(0,-r*.52); ctx.lineTo(r*.22,-r*.82); ctx.lineTo(r*.65,-r*.52); ctx.lineTo(r*.85,r*.40); ctx.lineTo(r*.42,r*.88); ctx.lineTo(-r*.42,r*.88); ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(-r*.55,-r*.42); ctx.lineTo(-r*.95,-r*.88); ctx.moveTo(r*.55,-r*.42); ctx.lineTo(r*.95,-r*.88); ctx.stroke();
  } else if (v === 'prism') {
    ctx.beginPath(); ctx.moveTo(0,-r*.95); ctx.lineTo(r*.75,-r*.18); ctx.lineTo(r*.46,r*.78); ctx.lineTo(-r*.46,r*.78); ctx.lineTo(-r*.75,-r*.18); ctx.closePath(); ctx.fill(); ctx.stroke();
    for(let i=0;i<3;i++){const yy=(-.38+i*.34)*r;ctx.strokeStyle=`rgba(${rgb},.35)`;ctx.beginPath();ctx.moveTo(-r*.42,yy);ctx.lineTo(r*.42,yy);ctx.stroke();}
  } else if (v === 'linkbreaker') {
    ctx.beginPath(); ctx.moveTo(0,-r*.96); ctx.lineTo(r*.78,-r*.34); ctx.lineTo(r*.62,r*.66); ctx.lineTo(0,r*.92); ctx.lineTo(-r*.62,r*.66); ctx.lineTo(-r*.78,-r*.34); ctx.closePath(); ctx.fill(); ctx.stroke();
    ctx.strokeStyle = '#ff4d70'; ctx.lineWidth = Math.max(1.4,r*.08);
    ctx.beginPath(); ctx.moveTo(-r*.60,-r*.50); ctx.lineTo(r*.10,r*.10); ctx.lineTo(-r*.08,r*.48); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(r*.56,-r*.62); ctx.lineTo(r*.16,-r*.14); ctx.stroke();
  } else if (v === 'skitter') {
    ctx.beginPath(); ctx.moveTo(0,-r*.78); ctx.lineTo(r*.72,-r*.30); ctx.lineTo(r*.82,r*.40); ctx.lineTo(r*.34,r*.80); ctx.lineTo(-r*.34,r*.80); ctx.lineTo(-r*.82,r*.40); ctx.lineTo(-r*.72,-r*.30); ctx.closePath(); ctx.fill(); ctx.stroke();
    for(let i=0;i<3;i++){const yy=(-.35+i*.32)*r;ctx.strokeStyle=`rgba(${rgb},.32)`;ctx.beginPath();ctx.moveTo(-r*.50,yy);ctx.lineTo(r*.50,yy);ctx.stroke();}
    for(const side of [-1,1]){const leg= Math.sin(t*8+side)*r*.10;ctx.beginPath();ctx.moveTo(side*r*.42,0);ctx.lineTo(side*r*(1.0+leg),-r*.48);ctx.moveTo(side*r*.42,r*.18);ctx.lineTo(side*r*(1.08-leg),r*.58);ctx.stroke();}
  } else {
    const phase = Math.sin(t*2.6+e.pos.y*.01)*.10;
    ctx.beginPath(); ctx.moveTo(0,-r*.82); ctx.lineTo(r*.58,-r*.32); ctx.lineTo(r*.44,r*.58); ctx.lineTo(0,r*.82); ctx.lineTo(-r*.44,r*.58); ctx.lineTo(-r*.58,-r*.32); ctx.closePath(); ctx.fill(); ctx.stroke();
    for(let i=0;i<4;i++){const a=t*.9+i*Math.PI/2;ctx.strokeStyle=`rgba(${rgb},.30)`;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.42,Math.sin(a)*r*.42);ctx.lineTo(Math.cos(a)*r*(.95+phase),Math.sin(a)*r*.55);ctx.stroke();}
  }
  ctx.shadowBlur = 0;
  const core = ctx.createRadialGradient(-r*.16,-r*.18,1,0,0,r*.62);
  core.addColorStop(0,'#fff'); core.addColorStop(.28,color); core.addColorStop(.75,`rgba(${rgb},.18)`); core.addColorStop(1,'rgba(0,0,0,0)');
  ctx.fillStyle=core; ctx.beginPath(); ctx.arc(0,0,r*.52,0,Math.PI*2); ctx.fill();
  ctx.restore();
}

function drawEliteCrest(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void {
  const r=e.radius, pulse=.72+.28*Math.sin(t*6);
  ctx.save();
  ctx.globalCompositeOperation='lighter';
  ctx.translate(0,-r*1.08);
  ctx.rotate(t*.55);
  ctx.strokeStyle=`rgba(216,121,255,${.48+.30*pulse})`;
  ctx.lineWidth=1.8;
  for(let i=0;i<4;i++){
    const a=i*Math.PI/2;
    const x=Math.cos(a)*r*.55, y=Math.sin(a)*r*.55;
    ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.18,Math.sin(a)*r*.18);ctx.lineTo(x,y);ctx.stroke();
  }
  ctx.fillStyle='#f0c9ff'; ctx.shadowColor='#d879ff';ctx.shadowBlur=12;
  ctx.beginPath();ctx.moveTo(0,-r*.48);ctx.lineTo(r*.20,-r*.08);ctx.lineTo(0,r*.26);ctx.lineTo(-r*.20,-r*.08);ctx.closePath();ctx.fill();
  ctx.restore();
}

function drawEnemyStatusVfx(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void {
  const r=e.radius;
  ctx.save(); ctx.globalCompositeOperation='lighter';
  if(e.fireTimer>0){
    const p=.5+.5*Math.sin(t*10);
    for(let i=0;i<4;i++){const a=t*2.4+i*Math.PI/2;const rr=r*(.8+.18*Math.sin(t*5+i));ctx.fillStyle=`rgba(255,102,61,${.28+.18*p})`;ctx.beginPath();ctx.arc(Math.cos(a)*rr,Math.sin(a)*rr*.65-r*.18,2.2+p*1.4,0,Math.PI*2);ctx.fill();}
    ctx.strokeStyle=`rgba(255,190,90,${.25+.2*p})`;ctx.lineWidth=1.4;ctx.beginPath();ctx.moveTo(-r*.35,-r*.65);ctx.quadraticCurveTo(0,-r*(1.15+p*.15),r*.35,-r*.65);ctx.stroke();
  }
  if(e.poisonTimer>0){
    for(let i=0;i<5;i++){const a=t*.65+i*1.23;const rr=r*(.85+.18*((t*.7+i*.2)%1));ctx.fillStyle='rgba(101,227,143,.38)';ctx.beginPath();ctx.arc(Math.cos(a)*rr,Math.sin(a)*rr*.55-r*.12,2+i%2,0,Math.PI*2);ctx.fill();}
    ctx.strokeStyle='rgba(113,245,157,.35)';ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(-r*.7,r*.35);ctx.quadraticCurveTo(0,r*.72,r*.7,r*.35);ctx.stroke();
  }
  if(e.freezeTimer>0){
    const spin=t*.8;
    ctx.strokeStyle='rgba(140,233,255,.72)';ctx.lineWidth=1.3;
    for(let i=0;i<6;i++){const a=spin+i*Math.PI/3;const x=Math.cos(a)*r*1.12,y=Math.sin(a)*r*.72;ctx.beginPath();ctx.moveTo(x,y);ctx.lineTo(x+Math.cos(a)*r*.22,y+Math.sin(a)*r*.22);ctx.stroke();}
    ctx.fillStyle='rgba(210,250,255,.32)';ctx.beginPath();ctx.arc(0,0,r*.22,0,Math.PI*2);ctx.fill();
  }
  ctx.restore();
}

function drawModernSphere(ctx: CanvasRenderingContext2D, s: GameState, sphere: SphereEntity): void {
  const def = SPHERE_TYPES[sphere.type];
  const color = def.color;
  const rgb = hexToRgb(color);
  const time = Date.now() / 1000;
  const tier = sphere.visualTier;
  const range = sphere.radius
    * (1 + (s.player.abilities.radius || 0) * 0.15)
    * (s.player.artifacts.includes('radius_shard') ? 1.1 : 1)
    * def.rangeMult;

  // New-desing pass: real illustrated sprite first, procedural geometry only as fallback.
  const r = Math.max(13, Math.min(21, sphere.radius * 0.17 + tier * 0.7));
  const bob = Math.sin(time * 2.2 + sphere.pos.x * 0.01) * 0.7;
  const artKey: ArtKey = `sphere-${sphere.type}` as ArtKey;

  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y + bob);
  drawGroundShadow(ctx, r * 0.90, r * 0.24, 6);
  drawNetworkDisabledIndicator(ctx, sphere, time);

  const drawn = drawReferenceSprite(ctx, artKey, 0, -r * 0.14, r * 3.15, color, Math.sin(time * 0.7 + sphere.pos.x * 0.01) * 0.025);
  if (drawn) {
    // Orbital satellites are damage emitters, not physics bodies. They are
    // rendered from the same angular state used by engine.ts and never enter
    // player/enemy collision resolution.
    if (sphere.type === 'orbital') {
      const satelliteCount = Math.max(
        1,
        1 + (s.player.sphereMods?.multishot || 0)
          + (s.player.artifacts.includes('orbital_crown') ? 1 : 0)
          + (tier >= 7 && s.player.sphereBranches?.orbital === 'orbital_blade' ? 1 : 0),
      );
      const orbitR = r * (2.25 + tier * 0.08);
      for (let i = 0; i < satelliteCount; i++) {
        const a = sphere.rotation + i * Math.PI * 2 / satelliteCount;
        const sx = Math.cos(a) * orbitR;
        const sy = Math.sin(a) * orbitR * 0.52;
        ctx.save();
        ctx.translate(sx, sy);
        ctx.rotate(a + Math.PI / 2);
        ctx.shadowColor = color;
        ctx.shadowBlur = 9;
        ctx.fillStyle = color;
        ctx.strokeStyle = '#effcff';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(0, -r * 0.18);
        ctx.lineTo(r * 0.14, 0);
        ctx.lineTo(0, r * 0.18);
        ctx.lineTo(-r * 0.14, 0);
        ctx.closePath();
        ctx.fill();
        ctx.stroke();
        ctx.restore();
      }
    }
    // Tier/evolution information stays in the game renderer, but is deliberately secondary to the art.
    if (tier >= 4) {
      ctx.save();
      ctx.translate(0,-r*1.05);
      ctx.rotate(time*.7);
      ctx.strokeStyle = `rgba(${rgb},0.55)`;
      ctx.lineWidth = 1.2;
      for(let i=0;i<4;i++){const a=i*Math.PI/2;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.18,Math.sin(a)*r*.18);ctx.lineTo(Math.cos(a)*r*.52,Math.sin(a)*r*.52);ctx.stroke();}
      ctx.restore();
    }
    ctx.restore();
    drawSphereVfx(ctx,sphere,color,r,time);
    return;
  }

  // Contact shadow anchors the object to the arena.
  drawGroundShadow(ctx, r * 0.78, r * 0.22, 5);

  // Small physical socket under every sphere.
  const base = ctx.createLinearGradient(-r, 0, r, r * 0.65);
  base.addColorStop(0, '#263b4f');
  base.addColorStop(0.42, '#102235');
  base.addColorStop(1, '#030812');
  ctx.fillStyle = base;
  ctx.strokeStyle = 'rgba(190,225,245,0.24)';
  ctx.lineWidth = 0.8;
  ctx.beginPath();
  ctx.ellipse(0, r * 0.40, r * 0.72, r * 0.23, 0, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();

  // Dark volumetric outer shell.
  const shell = ctx.createRadialGradient(-r * 0.34, -r * 0.48, r * 0.08, 0, 0, r * 1.05);
  shell.addColorStop(0, '#6d879c');
  shell.addColorStop(0.13, '#344c61');
  shell.addColorStop(0.34, '#152a3e');
  shell.addColorStop(0.72, '#07121f');
  shell.addColorStop(1, '#01050b');

  ctx.save();
  ctx.scale(1, 0.94);
  ctx.fillStyle = shell;
  ctx.strokeStyle = 'rgba(205,233,247,0.30)';
  ctx.lineWidth = 0.85;
  ctx.shadowColor = color;
  ctx.shadowBlur = 5;

  if (sphere.type === 'sniper') {
    // Purple precision sphere with a long, asymmetric emitter spine.
    ctx.beginPath();
    ctx.moveTo(-r * 0.54, -r * 0.20);
    ctx.lineTo(-r * 0.28, -r * 0.72);
    ctx.lineTo(r * 0.22, -r * 0.86);
    ctx.lineTo(r * 0.68, -r * 0.34);
    ctx.lineTo(r * 0.55, r * 0.38);
    ctx.lineTo(0, r * 0.67);
    ctx.lineTo(-r * 0.58, r * 0.34);
    ctx.closePath();
    ctx.fill(); ctx.stroke();

    const barrel = ctx.createLinearGradient(-r * 0.25, -r, r * 0.95, -r * 0.25);
    barrel.addColorStop(0, '#6b8093');
    barrel.addColorStop(0.35, '#24394e');
    barrel.addColorStop(1, '#050b14');
    ctx.fillStyle = barrel;
    ctx.beginPath();
    ctx.moveTo(-r * 0.06, -r * 0.48);
    ctx.lineTo(r * 0.94, -r * 0.92);
    ctx.lineTo(r * 1.10, -r * 0.74);
    ctx.lineTo(r * 0.10, -r * 0.25);
    ctx.closePath();
    ctx.fill();
    ctx.strokeStyle = 'rgba(226,237,246,0.28)';
    ctx.stroke();
  } else if (sphere.type === 'shotgun') {
    // Red spread sphere with three short physical emitters.
    ctx.beginPath();
    ctx.moveTo(-r * 0.72, -r * 0.28);
    ctx.lineTo(-r * 0.46, -r * 0.68);
    ctx.lineTo(r * 0.42, -r * 0.62);
    ctx.lineTo(r * 0.74, -r * 0.12);
    ctx.lineTo(r * 0.50, r * 0.58);
    ctx.lineTo(-r * 0.58, r * 0.62);
    ctx.closePath();
    ctx.fill(); ctx.stroke();

    for (const [yy, tilt] of [[-0.28, -0.12], [0, 0], [0.28, 0.12]] as const) {
      ctx.save();
      ctx.rotate(tilt);
      const tube = ctx.createLinearGradient(0, yy * r, r, yy * r);
      tube.addColorStop(0, '#61788c');
      tube.addColorStop(0.30, '#243a4e');
      tube.addColorStop(1, '#040a12');
      ctx.fillStyle = tube;
      ctx.beginPath();
      ctx.roundRect(r * 0.22, yy * r - r * 0.075, r * 0.72, r * 0.15, r * 0.045);
      ctx.fill();
      ctx.strokeStyle = 'rgba(' + rgb + ',0.46)';
      ctx.stroke();
      ctx.restore();
    }
  } else if (sphere.type === 'chain') {
    // Gold chain sphere with articulated conductor arms.
    ctx.beginPath();
    ctx.moveTo(0, -r * 0.78);
    ctx.lineTo(r * 0.58, -r * 0.42);
    ctx.lineTo(r * 0.66, r * 0.30);
    ctx.lineTo(r * 0.18, r * 0.68);
    ctx.lineTo(-r * 0.56, r * 0.46);
    ctx.lineTo(-r * 0.68, -r * 0.24);
    ctx.closePath();
    ctx.fill(); ctx.stroke();

    ctx.strokeStyle = '#31495d';
    ctx.lineWidth = Math.max(1.8, r * 0.095);
    ctx.lineCap = 'round';
    for (const side of [-1, 1]) {
      ctx.beginPath();
      ctx.moveTo(side * r * 0.34, -side * r * 0.04);
      ctx.lineTo(side * r * 0.92, -side * r * 0.40);
      ctx.lineTo(side * r * 1.08, -side * r * 0.10);
      ctx.stroke();
    }
    ctx.lineCap = 'butt';

    ctx.fillStyle = color;
    ctx.shadowColor = color;
    ctx.shadowBlur = 8;
    for (const side of [-1, 1]) {
      ctx.beginPath();
      ctx.arc(side * r * 1.03, -side * r * 0.12, r * 0.10, 0, Math.PI * 2);
      ctx.fill();
    }
  } else if (sphere.type === 'aura') {
    // Teal support sphere. The ring is a physical emitter around the core.
    ctx.beginPath();
    ctx.moveTo(-r * 0.72, -r * 0.16);
    ctx.lineTo(-r * 0.42, -r * 0.68);
    ctx.lineTo(r * 0.42, -r * 0.68);
    ctx.lineTo(r * 0.72, -r * 0.16);
    ctx.lineTo(r * 0.52, r * 0.56);
    ctx.lineTo(-r * 0.52, r * 0.56);
    ctx.closePath();
    ctx.fill(); ctx.stroke();

    ctx.save();
    ctx.rotate(-0.14 + Math.sin(time * 0.7) * 0.03);
    const ring = ctx.createLinearGradient(-r, 0, r, 0);
    ring.addColorStop(0, '#06101b');
    ring.addColorStop(0.22, '#486377');
    ring.addColorStop(0.50, color);
    ring.addColorStop(0.78, '#365064');
    ring.addColorStop(1, '#06101b');
    ctx.fillStyle = ring;
    ctx.strokeStyle = 'rgba(214,242,255,0.36)';
    ctx.lineWidth = 0.85;
    ctx.beginPath();
    ctx.ellipse(0, -r * 0.18, r * 1.00, r * 0.27, 0, 0, Math.PI * 2);
    ctx.fill(); ctx.stroke();
    ctx.restore();
  } else {
    // Blue standard sphere, the cleanest and most orb-like silhouette.
    ctx.beginPath();
    ctx.moveTo(0, -r * 0.82);
    ctx.lineTo(r * 0.62, -r * 0.48);
    ctx.lineTo(r * 0.74, r * 0.20);
    ctx.lineTo(r * 0.30, r * 0.66);
    ctx.lineTo(-r * 0.40, r * 0.62);
    ctx.lineTo(-r * 0.72, r * 0.18);
    ctx.lineTo(-r * 0.60, -r * 0.52);
    ctx.closePath();
    ctx.fill(); ctx.stroke();

    ctx.fillStyle = 'rgba(116,145,166,0.20)';
    ctx.beginPath();
    ctx.moveTo(-r * 0.44, -r * 0.50);
    ctx.lineTo(0, -r * 0.72);
    ctx.lineTo(r * 0.44, -r * 0.44);
    ctx.lineTo(r * 0.16, -r * 0.18);
    ctx.lineTo(-r * 0.36, -r * 0.24);
    ctx.closePath();
    ctx.fill();
  }

  ctx.shadowBlur = 0;

  // Recessed reactor. The colored light should sit inside the shell.
  const coreSize = sphere.type === 'aura' ? r * 0.30 : r * 0.34;
  ctx.fillStyle = 'rgba(1,5,11,0.88)';
  ctx.strokeStyle = 'rgba(205,233,247,0.26)';
  ctx.lineWidth = 0.75;
  ctx.beginPath();
  ctx.ellipse(0, r * 0.02, coreSize * 1.10, coreSize * 0.80, 0, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();

  const core = ctx.createRadialGradient(-coreSize * 0.25, -coreSize * 0.24, 0.5, 0, 0, coreSize * 1.20);
  core.addColorStop(0, '#ffffff');
  core.addColorStop(0.15, '#f4ffff');
  core.addColorStop(0.38, color);
  core.addColorStop(0.70, 'rgba(' + rgb + ',0.46)');
  core.addColorStop(1, 'rgba(0,0,0,0)');
  ctx.fillStyle = core;
  ctx.shadowColor = color;
  ctx.shadowBlur = 8 + tier * 1.4;
  ctx.beginPath();
  ctx.ellipse(0, r * 0.02, coreSize, coreSize * 0.72, 0, 0, Math.PI * 2);
  ctx.fill();
  ctx.shadowBlur = 0;

  ctx.fillStyle = 'rgba(0,6,14,0.34)';
  ctx.beginPath();
  ctx.ellipse(coreSize * 0.10, coreSize * 0.06, coreSize * 0.34, coreSize * 0.24, 0, 0, Math.PI * 2);
  ctx.fill();

  if (tier >= 2) {
    ctx.strokeStyle = 'rgba(' + rgb + ',0.54)';
    ctx.lineWidth = 0.9;
    ctx.beginPath();
    ctx.moveTo(-r * 0.62, r * 0.38);
    ctx.lineTo(-r * 0.86, r * 0.54);
    ctx.lineTo(-r * 0.52, r * 0.54);
    ctx.moveTo(r * 0.62, r * 0.38);
    ctx.lineTo(r * 0.86, r * 0.54);
    ctx.lineTo(r * 0.52, r * 0.54);
    ctx.stroke();
  }

  if (tier >= 4) {
    ctx.save();
    ctx.rotate(time * 0.18);
    ctx.strokeStyle = 'rgba(' + rgb + ',0.34)';
    ctx.lineWidth = 0.9;
    ctx.beginPath();
    ctx.ellipse(0, 0, r * 1.02, r * 0.29, 0, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();
  }

  ctx.restore();

  drawSphereVfx(ctx,sphere,color,r,time);

  // Combat footprint remains subtle. The arena should dominate the frame.
  ctx.save();
  ctx.translate(sphere.pos.x, sphere.pos.y + bob);
  ctx.strokeStyle = 'rgba(' + rgb + ',0.025)';
  ctx.lineWidth = 0.8;
  ctx.beginPath();
  ctx.ellipse(0, r * 0.50, Math.min(range * 0.52, 120), Math.min(range * 0.10, 22), 0, 0, Math.PI * 2);
  ctx.stroke();

  if (def.aura) {
    const pulse = 1 + Math.sin(time * 4) * 0.035;
    ctx.strokeStyle = 'rgba(' + rgb + ',0.11)';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.ellipse(0, 4, def.auraRadius * pulse, def.auraRadius * pulse * 0.22, 0, 0, Math.PI * 2);
    ctx.stroke();
  }

  const targetNearby = s.enemies.some((enemy) =>
    enemy.hp > 0 && Math.hypot(enemy.pos.x - sphere.pos.x, enemy.pos.y - sphere.pos.y) < range
  );
  if (targetNearby) {
    const pulse = 0.35 + 0.30 * Math.sin(time * 8);
    ctx.strokeStyle = 'rgba(' + rgb + ',' + (0.055 + pulse * 0.055) + ')';
    ctx.lineWidth = 0.7;
    ctx.beginPath();
    ctx.ellipse(0, r * 0.50, r * 0.88, r * 0.18, 0, 0, Math.PI * 2);
    ctx.stroke();
  }
  ctx.restore();
}


function drawFormationMemory(ctx: CanvasRenderingContext2D, s: GameState): void {
  const memory = s.formationMemory;
  if (!memory || memory.expiresAt <= s.time || memory.nodes.length < 2) return;

  const alpha = Math.max(0, Math.min(1, (memory.expiresAt - s.time) / 0.9));
  const color =
    memory.type === 'triangle' ? '#ffb84d' :
    memory.type === 'square' ? '#69b7ff' :
    memory.type === 'cluster' ? '#b38cff' :
    '#63e6ff';

  ctx.save();
  ctx.strokeStyle = color;
  ctx.globalAlpha = 0.34 * alpha;
  ctx.lineWidth = 1.4;
  ctx.shadowColor = color;
  ctx.shadowBlur = 10;
  ctx.setLineDash([5, 7]);

  ctx.beginPath();
  memory.nodes.forEach((node, index) => {
    if (index === 0) ctx.moveTo(node.x, node.y);
    else ctx.lineTo(node.x, node.y);
  });
  if (memory.type === 'triangle' || memory.type === 'square') ctx.closePath();
  ctx.stroke();

  for (const node of memory.nodes) {
    ctx.globalAlpha = 0.22 * alpha;
    ctx.beginPath();
    ctx.arc(node.x, node.y, 10 + Math.sin(s.time * 7) * 1.5, 0, Math.PI * 2);
    ctx.stroke();
  }

  ctx.setLineDash([]);
  ctx.restore();
}

function drawSphereNetwork(ctx: CanvasRenderingContext2D, s: GameState, network: SphereNetworkState): void {
  if (network.nodes.length < 2) return;

  const t = s.time;
  const pulse = 0.55 + Math.sin(t * 4.2) * 0.12;

  const hasNode = (shape: { nodes: number[] } | null, index: number): boolean =>
    Boolean(shape?.nodes.includes(index));

  const drawFormationTag = (
    x: number,
    y: number,
    label: string,
    color: string,
    width = 96,
  ): void => {
    ctx.save();
    ctx.translate(x, y);
    ctx.fillStyle = 'rgba(3,8,18,0.86)';
    ctx.strokeStyle = color;
    ctx.globalAlpha = 0.92;
    ctx.lineWidth = 1;
    ctx.shadowColor = color;
    ctx.shadowBlur = 8;
    ctx.beginPath();
    ctx.roundRect(-width / 2, -10, width, 20, 7);
    ctx.fill();
    ctx.stroke();
    ctx.shadowBlur = 0;
    ctx.fillStyle = '#eef9ff';
    ctx.font = 'bold 8px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(label, 0, 0.5);
    ctx.restore();
  };

  for (const link of network.links) {
    const lineNode = hasNode(network.line, link.a) && hasNode(network.line, link.b);
    const triangleNode = hasNode(network.triangle, link.a) && hasNode(network.triangle, link.b);
    const clusterNode = hasNode(network.cluster, link.a) && hasNode(network.cluster, link.b);
    const squareNode = hasNode(network.square, link.a) && hasNode(network.square, link.b);

    let alpha = 0.18;
    let color = '#63b9ff';
    let active = false;
    if (lineNode) {
      alpha = 0.58;
      color = '#63e6ff';
      active = true;
    } else if (squareNode) {
      alpha = 0.58;
      color = '#69b7ff';
      active = true;
    } else if (triangleNode) {
      alpha = 0.62;
      color = '#ffb84d';
      active = true;
    } else if (clusterNode) {
      alpha = 0.46;
      color = '#b38cff';
      active = true;
    }

    const a = s.spheres[link.a].pos;
    const b = s.spheres[link.b].pos;
    ctx.save();
    ctx.strokeStyle = color;
    ctx.globalAlpha = alpha * pulse;
    ctx.lineWidth = active ? 1.6 : 1;
    ctx.shadowColor = color;
    ctx.shadowBlur = active ? 11 : 5;
    ctx.setLineDash(lineNode ? [8, 6] : squareNode ? [6, 5] : clusterNode ? [3, 7] : [4, 8]);
    ctx.beginPath();
    ctx.moveTo(a.x, a.y);
    ctx.lineTo(b.x, b.y);
    ctx.stroke();

    if (active) {
      const speed = lineNode ? 0.42 : triangleNode ? 0.50 : 0.34;
      const phase = ((t * speed) + link.a * 0.17 + link.b * 0.11) % 1;
      const px = a.x + (b.x - a.x) * phase;
      const py = a.y + (b.y - a.y) * phase;
      ctx.fillStyle = color;
      ctx.globalAlpha = 0.85;
      ctx.shadowColor = color;
      ctx.shadowBlur = 9;
      ctx.beginPath();
      ctx.arc(px, py, 2.2, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.setLineDash([]);
    ctx.restore();
  }

  if (network.line) {
    let cx = 0;
    let cy = 0;
    for (const index of network.line.nodes) {
      cx += s.spheres[index].pos.x;
      cy += s.spheres[index].pos.y;
    }
    cx /= network.line.nodes.length;
    cy /= network.line.nodes.length;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.strokeStyle = '#63e6ff';
    ctx.globalAlpha = 0.20 + 0.05 * Math.sin(t * 5);
    ctx.lineWidth = 1;
    ctx.setLineDash([2, 8]);
    const spread = 34 + network.line.nodes.length * 5;
    ctx.beginPath();
    ctx.moveTo(-spread, 0);
    ctx.lineTo(spread, 0);
    ctx.stroke();
    ctx.restore();
  }

  if (network.triangle) {
    const points = network.triangle.nodes.map((index) => s.spheres[index].pos);
    ctx.save();
    ctx.fillStyle = '#ffb84d';
    ctx.globalAlpha = 0.028 + 0.018 * Math.sin(t * 3);
    ctx.beginPath();
    ctx.moveTo(points[0].x, points[0].y);
    ctx.lineTo(points[1].x, points[1].y);
    ctx.lineTo(points[2].x, points[2].y);
    ctx.closePath();
    ctx.fill();
    ctx.strokeStyle = '#ffcf73';
    ctx.globalAlpha = 0.24 + 0.08 * Math.sin(t * 4);
    ctx.lineWidth = 1;
    ctx.shadowColor = '#ffb84d';
    ctx.shadowBlur = 10;
    ctx.stroke();
    ctx.restore();

    let cx = 0;
    let cy = 0;
    let totalCharge = 0;
    let activePulse = false;
    for (const index of network.triangle.nodes) {
      const sphere = s.spheres[index];
      cx += sphere.pos.x;
      cy += sphere.pos.y;
      totalCharge += sphere.formationHitCount % 3;
      activePulse ||= sphere.resonancePulseTimer > 0;
    }
    cx /= network.triangle.nodes.length;
    cy /= network.triangle.nodes.length;
    const averageCharge = Math.round(totalCharge / network.triangle.nodes.length);

    for (const index of network.triangle.nodes) {
      const sphere = s.spheres[index];
      const charge = sphere.formationHitCount % 3;
      const progress = charge / 3;
      const radius = 25 + Math.sin(t * 5 + index) * 1.5;

      ctx.save();
      ctx.translate(sphere.pos.x, sphere.pos.y);
      ctx.strokeStyle = '#ffb84d';
      ctx.lineWidth = 1.4;
      ctx.globalAlpha = 0.20;
      ctx.beginPath();
      ctx.arc(0, 0, radius, -Math.PI / 2, Math.PI * 1.5);
      ctx.stroke();

      if (charge > 0) {
        ctx.globalAlpha = 0.86;
        ctx.shadowColor = '#ffb84d';
        ctx.shadowBlur = 8;
        ctx.beginPath();
        ctx.arc(0, 0, radius, -Math.PI / 2, -Math.PI / 2 + Math.PI * 2 * progress);
        ctx.stroke();
      }

      if (sphere.resonancePulseTimer > 0) {
        const flash = sphere.resonancePulseTimer / 0.45;
        ctx.globalAlpha = 0.45 + 0.4 * flash;
        ctx.shadowColor = '#fff0c2';
        ctx.shadowBlur = 18;
        ctx.lineWidth = 2.6;
        ctx.beginPath();
        ctx.arc(0, 0, radius + 5 + (1 - flash) * 10, -Math.PI / 2, Math.PI * 1.5);
        ctx.stroke();
      }

      for (let tick = 0; tick < 3; tick++) {
        const a = -Math.PI / 2 + (tick / 3) * Math.PI * 2;
        const inner = radius + 2;
        const outer = inner + (tick < charge ? 5 : 2);
        ctx.globalAlpha = tick < charge ? 0.95 : 0.30;
        ctx.beginPath();
        ctx.moveTo(Math.cos(a) * inner, Math.sin(a) * inner);
        ctx.lineTo(Math.cos(a) * outer, Math.sin(a) * outer);
        ctx.stroke();
      }
      ctx.restore();
    }

    ctx.save();
    ctx.translate(cx, cy);
    const corePulse = 1 + (activePulse ? 0.28 + 0.08 * Math.sin(t * 24) : 0.04 * Math.sin(t * 6));
    ctx.scale(corePulse, corePulse);
    ctx.fillStyle = '#ffb84d';
    ctx.globalAlpha = activePulse ? 0.9 : 0.26;
    ctx.shadowColor = activePulse ? '#fff0c2' : '#ffb84d';
    ctx.shadowBlur = activePulse ? 18 : 8;
    ctx.beginPath();
    ctx.arc(0, 0, activePulse ? 4.5 : 2.5, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Resonance is communicated by the triangle geometry, charge rings/ticks
    // and the dedicated HUD. Do not add a persistent field text label here.
  }

  if (network.square) {
    const points = network.square.nodes.map((index) => s.spheres[index].pos);
    let cx = 0;
    let cy = 0;
    for (const p of points) { cx += p.x; cy += p.y; }
    cx /= points.length;
    cy /= points.length;

    ctx.save();
    const shieldPulse = 1 + Math.sin(t * 4.8) * 0.035;
    ctx.translate(cx, cy);
    ctx.scale(shieldPulse, shieldPulse);
    ctx.strokeStyle = '#69b7ff';
    ctx.globalAlpha = 0.28 + 0.08 * Math.sin(t * 4);
    ctx.lineWidth = 1.8;
    ctx.shadowColor = '#69b7ff';
    ctx.shadowBlur = 16;
    ctx.beginPath();
    points.forEach((p, i) => {
      const x = p.x - cx;
      const y = p.y - cy;
      if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
    });
    ctx.closePath();
    ctx.stroke();

    const radius = Math.max(...points.map((p) => Math.hypot(p.x - cx, p.y - cy))) + 22;
    ctx.globalAlpha = 0.16 + 0.05 * Math.sin(t * 6);
    ctx.setLineDash([5, 7]);
    ctx.beginPath();
    ctx.arc(0, 0, radius, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);

    ctx.fillStyle = '#69b7ff';
    ctx.globalAlpha = 0.30 + 0.10 * Math.sin(t * 8);
    ctx.shadowBlur = 20;
    ctx.beginPath();
    ctx.arc(0, 0, 4.5, 0, Math.PI * 2);
    ctx.fill();

    for (let i = 0; i < 4; i++) {
      const a = t * 0.8 + i * Math.PI / 2;
      const x = Math.cos(a) * radius;
      const y = Math.sin(a) * radius;
      ctx.globalAlpha = 0.65;
      ctx.beginPath();
      ctx.arc(x, y, 2.2, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.restore();
  }

  if (network.cluster) {
    let cx = 0;
    let cy = 0;
    for (const index of network.cluster.nodes) {
      cx += s.spheres[index].pos.x;
      cy += s.spheres[index].pos.y;
    }
    cx /= network.cluster.nodes.length;
    cy /= network.cluster.nodes.length;

    let radius = 0;
    for (const index of network.cluster.nodes) {
      radius = Math.max(radius, Math.hypot(s.spheres[index].pos.x - cx, s.spheres[index].pos.y - cy));
    }

    ctx.save();
    const clusterPulse = 1 + Math.sin(t * 3.4) * 0.035;
    ctx.strokeStyle = '#b38cff';
    ctx.globalAlpha = 0.20 + 0.07 * Math.sin(t * 3.2);
    ctx.lineWidth = 1.4;
    ctx.shadowColor = '#b38cff';
    ctx.shadowBlur = 15;
    ctx.setLineDash([3, 7]);
    ctx.beginPath();
    ctx.arc(cx, cy, radius + 18 + Math.sin(t * 3) * 3, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);

    ctx.strokeStyle = 'rgba(206,178,255,0.42)';
    ctx.globalAlpha = 0.55;
    ctx.lineWidth = 0.8;
    for (const index of network.cluster.nodes) {
      const p = s.spheres[index].pos;
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.lineTo(p.x, p.y);
      ctx.stroke();
    }

    ctx.fillStyle = '#b38cff';
    ctx.globalAlpha = 0.42 + 0.1 * Math.sin(t * 7);
    ctx.shadowColor = '#b38cff';
    ctx.shadowBlur = 11;
    ctx.beginPath();
    ctx.arc(cx, cy, 4.2 * clusterPulse, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

  }

  if (network.ring) {
    const points = network.ring.nodes.map((index) => s.spheres[index].pos);
    ctx.save();
    ctx.strokeStyle = '#55e69a';
    ctx.globalAlpha = 0.24 + 0.08 * Math.sin(t * 5);
    ctx.lineWidth = 1.6;
    ctx.shadowColor = '#55e69a';
    ctx.shadowBlur = 14;
    ctx.setLineDash([5, 6]);
    ctx.beginPath();
    points.forEach((p, i) => i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y));
    ctx.closePath();
    ctx.stroke();
    ctx.setLineDash([]);
    ctx.restore();
  }

  if (network.lattice) {
    let cx = 0;
    let cy = 0;
    for (const index of network.lattice.nodes) {
      cx += s.spheres[index].pos.x;
      cy += s.spheres[index].pos.y;
    }
    cx /= network.lattice.nodes.length;
    cy /= network.lattice.nodes.length;
    ctx.save();
    ctx.strokeStyle = '#39d8ff';
    ctx.globalAlpha = 0.20 + 0.06 * Math.sin(t * 6);
    ctx.lineWidth = 1;
    for (const index of network.lattice.nodes) {
      const p = s.spheres[index].pos;
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.lineTo(p.x, p.y);
      ctx.stroke();
      ctx.globalAlpha = 0.62;
      ctx.beginPath();
      ctx.arc(p.x, p.y, 2.8 + Math.sin(t * 7 + index) * 0.7, 0, Math.PI * 2);
      ctx.fillStyle = '#39d8ff';
      ctx.fill();
      ctx.globalAlpha = 0.20 + 0.06 * Math.sin(t * 6);
    }
    ctx.restore();
  }

  if (network.fractal) {
    const points = network.fractal.nodes.map((index) => s.spheres[index].pos);
    let cx = 0;
    let cy = 0;
    for (const p of points) { cx += p.x; cy += p.y; }
    cx /= points.length;
    cy /= points.length;
    let radius = 0;
    for (const p of points) radius = Math.max(radius, Math.hypot(p.x - cx, p.y - cy));
    ctx.save();
    ctx.strokeStyle = '#ff6b9d';
    ctx.globalAlpha = 0.24 + 0.10 * Math.sin(t * 8);
    ctx.lineWidth = 1.2;
    ctx.shadowColor = '#ff6b9d';
    ctx.shadowBlur = 18;
    ctx.setLineDash([3, 5]);
    for (const scale of [1, 0.62, 0.34]) {
      ctx.beginPath();
      ctx.arc(cx, cy, Math.max(8, radius * scale), 0, Math.PI * 2);
      ctx.stroke();
    }
    ctx.setLineDash([]);
    ctx.restore();
  }
}
function drawVoidCore(ctx:CanvasRenderingContext2D,r:number,color:string,pulse=1):void{
  const rgb=hexToRgb(color);
  const glow=ctx.createRadialGradient(0,-r*.12,0,0,0,r*1.35);
  glow.addColorStop(0,'rgba(255,255,255,.96)');
  glow.addColorStop(.10,'rgba(215,250,255,.82)');
  glow.addColorStop(.30,`rgba(${rgb},.58)`);
  glow.addColorStop(.68,`rgba(${rgb},.12)`);
  glow.addColorStop(1,'rgba(0,0,0,0)');
  ctx.save();
  ctx.scale(pulse,pulse);
  ctx.shadowColor=color;ctx.shadowBlur=r*.9;
  ctx.fillStyle=glow;ctx.beginPath();ctx.arc(0,-r*.08,r*.92,0,Math.PI*2);ctx.fill();
  ctx.shadowBlur=0;
  ctx.fillStyle='rgba(1,7,15,.76)';ctx.beginPath();ctx.arc(0,0,r*.43,0,Math.PI*2);ctx.fill();
  ctx.strokeStyle=`rgba(${rgb},.80)`;ctx.lineWidth=Math.max(1,r*.055);ctx.stroke();
  ctx.restore();
}

function drawVoidMantis(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  const rgb=hexToRgb(color);
  ctx.save();
  ctx.strokeStyle=`rgba(${rgb},.82)`;ctx.lineWidth=Math.max(1.2,r*.10);ctx.lineCap='round';
  // Razor wings / forelegs create the angular silhouette from the reference.
  for(const s of [-1,1]){
    ctx.beginPath();ctx.moveTo(s*r*.18,-r*.05);ctx.lineTo(s*r*1.18,-r*.72);ctx.lineTo(s*r*.82,r*.05);ctx.lineTo(s*r*1.20,r*.58);ctx.stroke();
    ctx.beginPath();ctx.moveTo(s*r*.30,r*.12);ctx.lineTo(s*r*.92,r*.30);ctx.lineTo(s*r*1.12,r*.82);ctx.stroke();
  }
  const body=ctx.createLinearGradient(-r*.5,-r,r*.5,r);
  body.addColorStop(0,'#8299aa');body.addColorStop(.18,'#263d51');body.addColorStop(.55,'#071525');body.addColorStop(1,'#01050b');
  ctx.fillStyle=body;ctx.strokeStyle=`rgba(${rgb},.76)`;ctx.lineWidth=Math.max(1,r*.05);
  ctx.beginPath();ctx.moveTo(0,-r*.92);ctx.lineTo(r*.48,-r*.28);ctx.lineTo(r*.38,r*.62);ctx.lineTo(0,r*.86);ctx.lineTo(-r*.38,r*.62);ctx.lineTo(-r*.48,-r*.28);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle=`rgba(${rgb},.28)`;ctx.lineWidth=1;
  for(let i=0;i<3;i++){ctx.beginPath();ctx.moveTo(-r*.28,(i-.7)*r*.32);ctx.lineTo(r*.28,(i-.7)*r*.32);ctx.stroke();}
  drawVoidCore(ctx,r*.72,color,1+Math.sin(t*7)*.035);
  ctx.restore();
}

function drawVoidBeetle(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  const rgb=hexToRgb(color);
  ctx.save();
  ctx.rotate(Math.sin(t*1.5)*.025);
  ctx.fillStyle='#071321';ctx.strokeStyle=`rgba(${rgb},.78)`;ctx.lineWidth=Math.max(1.4,r*.06);
  ctx.beginPath();ctx.moveTo(0,-r);ctx.lineTo(r*.70,-r*.62);ctx.lineTo(r*.92,r*.15);ctx.lineTo(r*.54,r*.78);ctx.lineTo(0,r);ctx.lineTo(-r*.54,r*.78);ctx.lineTo(-r*.92,r*.15);ctx.lineTo(-r*.70,-r*.62);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle='rgba(193,229,245,.22)';ctx.lineWidth=Math.max(1,r*.035);
  ctx.beginPath();ctx.moveTo(-r*.56,-r*.45);ctx.lineTo(0,-r*.78);ctx.lineTo(r*.56,-r*.45);ctx.moveTo(-r*.64,r*.20);ctx.lineTo(0,r*.52);ctx.lineTo(r*.64,r*.20);ctx.stroke();
  for(const s of [-1,1]){ctx.beginPath();ctx.moveTo(s*r*.48,-r*.05);ctx.lineTo(s*r*1.10,r*.38);ctx.lineTo(s*r*1.18,r*.70);ctx.stroke();}
  drawVoidCore(ctx,r*.74,color,1+Math.sin(t*4)*.025);
  ctx.restore();
}

function drawVoidMoth(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  const rgb=hexToRgb(color),flap=Math.sin(t*5)*.12;
  ctx.save();
  ctx.strokeStyle=`rgba(${rgb},.70)`;ctx.lineWidth=Math.max(1,r*.045);
  ctx.fillStyle='rgba(7,18,31,.88)';
  for(const s of [-1,1]){
    ctx.beginPath();ctx.moveTo(s*r*.12,0);ctx.quadraticCurveTo(s*r*.72,-r*(.75+flap),s*r*1.15,-r*.35);ctx.quadraticCurveTo(s*r*.86,r*.10,s*r*.16,r*.18);ctx.closePath();ctx.fill();ctx.stroke();
  }
  ctx.beginPath();ctx.moveTo(0,-r*.78);ctx.lineTo(r*.24,-r*.18);ctx.lineTo(0,r*.70);ctx.lineTo(-r*.24,-r*.18);ctx.closePath();ctx.fill();ctx.stroke();
  drawVoidCore(ctx,r*.55,color,1+Math.sin(t*6)*.04);
  ctx.restore();
}

function drawVoidSkitter(ctx:CanvasRenderingContext2D,r:number,color:string,t:number):void{
  const rgb=hexToRgb(color);
  ctx.save();
  ctx.fillStyle='#06111e';ctx.strokeStyle=`rgba(${rgb},.74)`;ctx.lineWidth=Math.max(1,r*.055);
  ctx.beginPath();ctx.moveTo(0,-r*.78);ctx.lineTo(r*.72,-r*.35);ctx.lineTo(r*.82,r*.38);ctx.lineTo(r*.35,r*.78);ctx.lineTo(-r*.35,r*.78);ctx.lineTo(-r*.82,r*.38);ctx.lineTo(-r*.72,-r*.35);ctx.closePath();ctx.fill();ctx.stroke();
  for(let i=0;i<4;i++){const y=(-.45+i*.30)*r;ctx.beginPath();ctx.moveTo(-r*.55,y);ctx.lineTo(r*.55,y);ctx.stroke();}
  for(const s of [-1,1]){ctx.beginPath();ctx.moveTo(s*r*.52,-r*.05);ctx.lineTo(s*r*1.05,-r*.58);ctx.moveTo(s*r*.52,r*.12);ctx.lineTo(s*r*1.12,r*.60);ctx.stroke();}
  drawVoidCore(ctx,r*.58,color,1+Math.sin(t*8)*.05);
  ctx.restore();
}

function drawBossAttackTelegraph(
  ctx: CanvasRenderingContext2D,
  e: EnemyEntity,
  playerPos: { x: number; y: number },
  t: number,
): void {
  if (!e.isBoss) return;

  const r = e.radius;
  const pulse = 0.55 + Math.sin(t * 14) * 0.16;
  ctx.save();

  if (e.bossType === 'charger' && e.isCharging) {
    const windup = e.chargeTimer > 0.35;
    const reach = windup ? r * 2.7 : r * 4.2;
    ctx.strokeStyle = windup
      ? `rgba(255,184,77,${0.58 + pulse * 0.25})`
      : `rgba(255,98,87,${0.70 + pulse * 0.20})`;
    ctx.lineWidth = windup ? Math.max(2, r * 0.025) : Math.max(3, r * 0.04);
    ctx.setLineDash(windup ? [r * 0.22, r * 0.16] : []);
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(e.chargeDir.x * reach, e.chargeDir.y * reach);
    ctx.stroke();
    ctx.setLineDash([]);

    if (windup) {
      const nx = -e.chargeDir.y;
      const ny = e.chargeDir.x;
      const tip = r * 2.05;
      const half = r * 0.55;
      ctx.fillStyle = `rgba(255,184,77,${0.08 + pulse * 0.05})`;
      ctx.beginPath();
      ctx.moveTo(e.chargeDir.x * r * 0.82, e.chargeDir.y * r * 0.82);
      ctx.lineTo(e.chargeDir.x * tip + nx * half, e.chargeDir.y * tip + ny * half);
      ctx.lineTo(e.chargeDir.x * tip - nx * half, e.chargeDir.y * tip - ny * half);
      ctx.closePath();
      ctx.fill();
    }
  }

  if (e.bossType === 'shooter' && e.bossShootTimer <= BOSS_TELEGRAPH_WINDOWS.shooter) {
    const dx = playerPos.x - e.pos.x;
    const dy = playerPos.y - e.pos.y;
    const angle = Math.atan2(dy, dx);
    ctx.strokeStyle = `rgba(255,106,95,${0.46 + pulse * 0.22})`;
    ctx.lineWidth = Math.max(1.5, r * 0.018);
    ctx.setLineDash([r * 0.18, r * 0.13]);
    for (let k = -1; k <= 1; k++) {
      const a = angle + k * 0.11;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * r * 1.05, Math.sin(a) * r * 1.05);
      ctx.lineTo(Math.cos(a) * r * 4.8, Math.sin(a) * r * 4.8);
      ctx.stroke();
    }
    ctx.setLineDash([]);
  }

  if (e.bossType === 'summoner' && e.summonTimer <= BOSS_TELEGRAPH_WINDOWS.summoner) {
    const progress = 1 - Math.max(0, e.summonTimer) / BOSS_TELEGRAPH_WINDOWS.summoner;
    const ringRadius = r * (1.15 + progress * 2.2);
    ctx.strokeStyle = `rgba(162,124,255,${0.34 + pulse * 0.18})`;
    ctx.lineWidth = Math.max(1.5, r * 0.02);
    ctx.setLineDash([r * 0.2, r * 0.14]);
    ctx.beginPath();
    ctx.arc(0, 0, ringRadius, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);
    for (let i = 0; i < 3; i++) {
      const a = t * 2.2 + (i / 3) * Math.PI * 2;
      const nx = Math.cos(a) * (r * (0.95 + progress * 0.8));
      const ny = Math.sin(a) * (r * (0.95 + progress * 0.8));
      ctx.fillStyle = `rgba(201,181,255,${0.48 + pulse * 0.18})`;
      ctx.beginPath();
      ctx.arc(nx, ny, Math.max(2, r * 0.045), 0, Math.PI * 2);
      ctx.fill();
    }
  }

  if (e.bossType === 'aura' && e.bossShootTimer <= BOSS_TELEGRAPH_WINDOWS.aura) {
    const ringRadius = e.auraRadius + r * (0.12 + (0.10 + pulse * 0.05));
    ctx.strokeStyle = `rgba(255,98,185,${0.42 + pulse * 0.20})`;
    ctx.lineWidth = Math.max(1.5, r * 0.02);
    ctx.setLineDash([r * 0.2, r * 0.14]);
    ctx.beginPath();
    ctx.arc(0, 0, ringRadius, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);
  }

  ctx.restore();
}

function drawModernEnemy(ctx: CanvasRenderingContext2D, e: EnemyEntity, playerPos: { x: number; y: number function drawLinkBreakerTelegraph(ctx: CanvasRenderingContext2D, e: EnemyEntity, t: number): void {
  if (!e.isElite || !e.elitePulseTarget || (e.elitePulseTelegraphTimer || 0) <= 0) return;

  const target = e.elitePulseTarget;
  const dx = target.pos.x - e.pos.x;
  const dy = target.pos.y - e.pos.y;
  const remaining = Math.max(0, Math.min(1, (e.elitePulseTelegraphTimer || 0) / LINK_BREAKER_TELEGRAPH_SECONDS));
  const pulse = 0.60 + Math.sin(t * 18) * 0.22;

  ctx.save();
  ctx.strokeStyle = '#ff4d70';
  ctx.shadowColor = '#ff4d70';
  ctx.shadowBlur = 14;
  ctx.globalAlpha = pulse * (0.55 + (1 - remaining) * 0.45);
  ctx.lineWidth = 2.4;
  ctx.setLineDash([9, 6]);
  ctx.beginPath();
  ctx.moveTo(0, 0);
  ctx.lineTo(dx, dy);
  ctx.stroke();
  ctx.setLineDash([]);

  ctx.translate(dx, dy);
  ctx.globalAlpha = 0.78 + (1 - remaining) * 0.18;
  ctx.lineWidth = 2.2;
  ctx.beginPath();
  ctx.arc(0, 0, 16 + Math.sin(t * 20) * 2, 0, Math.PI * 2);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(-22, 0); ctx.lineTo(22, 0);
  ctx.moveTo(0, -22); ctx.lineTo(0, 22);
  ctx.stroke();

  ctx.globalAlpha = 0.35;
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.arc(0, 0, 28, 0, Math.PI * 2);
  ctx.stroke();
  ctx.restore();
}

function drawNetworkDisabledIndicator(ctx: CanvasRenderingContext2D, sphere: SphereEntity, t: number): void {
  if ((sphere.networkDisabledTimer || 0) <= 0) return;

  const remaining = Math.max(0, Math.min(1, (sphere.networkDisabledTimer || 0) / LINK_BREAKER_DISABLED_SECONDS));
  const pulse = 0.72 + Math.sin(t * 16) * 0.22;

  ctx.save();
  ctx.shadowColor = '#ff4d70';
  ctx.shadowBlur = 15;
  ctx.strokeStyle = '#ff4d70';
  ctx.globalAlpha = pulse;
  ctx.lineWidth = 2.2;
  ctx.setLineDash([7, 5]);

  ctx.beginPath();
  ctx.arc(0, 0, 24 + Math.sin(t * 13) * 1.5, 0, Math.PI * 2);
  ctx.stroke();

  ctx.globalAlpha = 0.88;
  ctx.setLineDash([3, 7]);
  ctx.beginPath();
  ctx.arc(0, 0, 31, -Math.PI / 3, Math.PI / 3);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(-14, -14); ctx.lineTo(14, 14);
  ctx.moveTo(14, -14); ctx.lineTo(-14, 14);
  ctx.stroke();

  ctx.globalAlpha = 0.50;
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.arc(0, 0, 36 - remaining * 8, -Math.PI / 2, -Math.PI / 2 + Math.PI * 2 * remaining);
  ctx.stroke();

  ctx.setLineDash([]);
  ctx.restore();
}

function drawModernBossBody(ctx:CanvasRenderingContext2D,e:EnemyEntity,color:string,t:number):void{
  const r=e.radius,boss=e.bossType,rgb=hexToRgb(color),pulse=1+Math.sin(t*2.8)*.04;
  ctx.save();
  drawGroundShadow(ctx,r*1.65,r*.50,20);
  ctx.globalCompositeOperation='lighter';
  ctx.shadowColor=color;ctx.shadowBlur=22;
  ctx.fillStyle='rgba(3,8,16,.94)';ctx.strokeStyle=`rgba(${rgb},.82)`;ctx.lineWidth=Math.max(2,r*.024);

  if(boss==='charger'){
    ctx.beginPath();ctx.moveTo(-r*1.05,r*.36);ctx.lineTo(-r*.72,-r*.56);ctx.lineTo(-r*.18,-r*.92);ctx.lineTo(r*.18,-r*.58);ctx.lineTo(r*.72,-r*.92);ctx.lineTo(r*1.05,r*.30);ctx.lineTo(r*.56,r*.82);ctx.lineTo(0,r*.66);ctx.lineTo(-r*.56,r*.82);ctx.closePath();ctx.fill();ctx.stroke();
    for(const side of [-1,1]){ctx.strokeStyle=`rgba(${rgb},.54)`;ctx.beginPath();ctx.moveTo(side*r*.55,-r*.48);ctx.lineTo(side*r*1.25,-r*.88);ctx.lineTo(side*r*1.02,-r*.18);ctx.stroke();}
    ctx.strokeStyle=`rgba(255,210,110,.55)`;ctx.lineWidth=2;ctx.beginPath();ctx.moveTo(-r*.30,r*.20);ctx.lineTo(r*.30,r*.20);ctx.stroke();
  } else if(boss==='shooter'){
    ctx.beginPath();ctx.moveTo(0,-r*1.05);ctx.lineTo(r*.55,-r*.62);ctx.lineTo(r*1.10,-r*.18);ctx.lineTo(r*.72,r*.60);ctx.lineTo(0,r*.88);ctx.lineTo(-r*.72,r*.60);ctx.lineTo(-r*1.10,-r*.18);ctx.lineTo(-r*.55,-r*.62);ctx.closePath();ctx.fill();ctx.stroke();
    for(let i=-1;i<=1;i++){ctx.strokeStyle=`rgba(${rgb},.46)`;ctx.lineWidth=1.2;ctx.beginPath();ctx.moveTo(i*r*.20,-r*.72);ctx.lineTo(i*r*.20,r*.54);ctx.stroke();}
    const scan=Math.sin(t*3)*.5+.5;ctx.strokeStyle=`rgba(255,240,180,${.35+.25*scan})`;ctx.beginPath();ctx.moveTo(-r*.82,-r*.06+scan*r*.32);ctx.lineTo(r*.82,-r*.06+scan*r*.32);ctx.stroke();
  } else if(boss==='summoner'){
    ctx.beginPath();ctx.ellipse(0,r*.08,r*.92,r*.80,0,0,Math.PI*2);ctx.fill();ctx.stroke();
    for(const side of [-1,1]){ctx.beginPath();ctx.moveTo(side*r*.46,-r*.34);ctx.lineTo(side*r*.98,-r*1.08);ctx.lineTo(side*r*.22,-r*.72);ctx.closePath();ctx.fill();ctx.stroke();}
    for(let i=0;i<5;i++){const a=t*1.6+i*Math.PI*2/5;const x=Math.cos(a)*r*1.18,y=Math.sin(a)*r*.68;ctx.fillStyle=`rgba(220,195,255,${.28+.15*pulse})`;ctx.beginPath();ctx.arc(x,y,3,0,Math.PI*2);ctx.fill();}
  } else {
    ctx.beginPath();ctx.moveTo(0,-r*1.02);ctx.lineTo(r*.84,-r*.56);ctx.lineTo(r*1.02,0);ctx.lineTo(r*.70,r*.70);ctx.lineTo(0,r*.88);ctx.lineTo(-r*.70,r*.70);ctx.lineTo(-r*1.02,0);ctx.lineTo(-r*.84,-r*.56);ctx.closePath();ctx.fill();ctx.stroke();
    for(let i=0;i<6;i++){const a=t*.65+i*Math.PI/3;ctx.strokeStyle=`rgba(${rgb},.35)`;ctx.beginPath();ctx.moveTo(Math.cos(a)*r*.62,Math.sin(a)*r*.46);ctx.lineTo(Math.cos(a)*r*1.22,Math.sin(a)*r*.82);ctx.stroke();}
  }

  ctx.shadowBlur=0;
  const coreR=r*.20*pulse,core=ctx.createRadialGradient(-r*.06,-r*.08,1,0,0,coreR*2.8);
  core.addColorStop(0,'#fff');core.addColorStop(.20,'#eaffff');core.addColorStop(.44,color);core.addColorStop(.78,`rgba(${rgb},.16)`);core.addColorStop(1,'rgba(0,0,0,0)');
  ctx.fillStyle=core;ctx.beginPath();ctx.arc(0,0,coreR*2.8,0,Math.PI*2);ctx.fill();
  ctx.fillStyle='rgba(0,4,11,.84)';ctx.beginPath();ctx.arc(0,0,coreR*1.12,0,Math.PI*2);ctx.fill();
  ctx.strokeStyle='rgba(230,251,255,.78)';ctx.lineWidth=1.2;ctx.beginPath();ctx.arc(0,0,coreR,0,Math.PI*2);ctx.stroke();
  ctx.restore();
}

function drawBossProjectile(ctx:CanvasRenderingContext2D,x:number,y:number,r:number,color:string):void{
  const t=Date.now()/1000,rgb=hexToRgb(color);
  ctx.save();ctx.translate(x,y);ctx.rotate(t*1.8);ctx.globalCompositeOperation='lighter';ctx.shadowColor=color;ctx.shadowBlur=16;
  ctx.fillStyle='#06101c';ctx.strokeStyle=`rgba(${rgb},.92)`;ctx.lineWidth=Math.max(1,r*.11);
  ctx.beginPath();for(let i=0;i<8;i++){const a=i*Math.PI/4,rr=i%2===0?r*1.45:r*.82;const px=Math.cos(a)*rr,py=Math.sin(a)*rr;if(i===0)ctx.moveTo(px,py);else ctx.lineTo(px,py);}ctx.closePath();ctx.fill();ctx.stroke();
  ctx.fillStyle='#fff';ctx.beginPath();ctx.arc(0,0,r*.38,0,Math.PI*2);ctx.fill();ctx.fillStyle=color;ctx.beginPath();ctx.arc(0,0,r*.26,0,Math.PI*2);ctx.fill();ctx.restore();
}

function drawLightning(ctx: CanvasRenderingContext2D, from: { x: number; y: number }, to: { x: number; y: number }): void {
  const segments = 8;
  ctx.beginPath(); ctx.moveTo(from.x, from.y);
  for (let i = 1; i < segments; i++) { const t = i / segments; const x = from.x + (to.x - from.x) * t + (Math.random() - 0.5) * 25; const y = from.y + (to.y - from.y) * t + (Math.random() - 0.5) * 25; ctx.lineTo(x, y); }
  ctx.lineTo(to.x, to.y); ctx.stroke();
}

// ===== Chest — paper box =====
function drawChest(ctx:CanvasRenderingContext2D,chest:ChestEntity):void{
  const t=Date.now()/1000,p=1+Math.sin(t*3.2)*.05;
  ctx.save();ctx.translate(chest.pos.x,chest.pos.y);ctx.scale(p,p);ctx.globalCompositeOperation='lighter';drawGroundShadow(ctx,18,6,5);
  ctx.shadowColor='#a98aff';ctx.shadowBlur=16;ctx.fillStyle='rgba(4,10,19,.96)';ctx.strokeStyle='rgba(194,160,255,.86)';ctx.lineWidth=1.4;
  ctx.beginPath();ctx.moveTo(-17,7);ctx.lineTo(-13,-8);ctx.lineTo(0,-14);ctx.lineTo(13,-8);ctx.lineTo(17,7);ctx.lineTo(0,14);ctx.closePath();ctx.fill();ctx.stroke();
  ctx.strokeStyle='rgba(111,224,255,.55)';ctx.beginPath();ctx.moveTo(-13,-8);ctx.lineTo(0,-2);ctx.lineTo(13,-8);ctx.moveTo(0,-2);ctx.lineTo(0,10);ctx.stroke();
  ctx.fillStyle='#fff';ctx.beginPath();ctx.arc(0,-2,3,0,Math.PI*2);ctx.fill();ctx.restore();
}

