import { BOSS_TYPES, DIFFICULTIES } from './gameData';
import { playSound } from './audio';
import {
  dealDamageToEnemy,
  damagePlayer,
  getCritChance,
  onEnemyDeath,
} from './engineCombat';
import type { BossType, EnemyEntity, GameState, Vec } from './engineTypes';

export const PLAYER_RADIUS = 16;
const SLOW_RADIUS = 300;

function dist(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function rand(min: number, max: number): number {
  return min + Math.random() * (max - min);
}

function getSlowRadius(): number {
  return SLOW_RADIUS;
}

function getSlowFactor(s: GameState): number {
  const lvl = s.player.abilities.slow || 0;
  return lvl > 0 ? 1 - (0.1 + (lvl - 1) * 0.05) : 1;
}

// ===== Wave spawning =====
function spawnEnemy(s: GameState, isBoss: boolean): EnemyEntity {
  const wave = s.wave;
  const angle = Math.random() * Math.PI * 2;
  const spawnDist = 700;
  const px = s.player.pos.x + Math.cos(angle) * spawnDist;
  const py = s.player.pos.y + Math.sin(angle) * spawnDist;
  const diff = DIFFICULTIES.find(d => d.id === s.difficulty)!;
  if (isBoss) {
    const hp = (400 + wave * 60) * diff.enemyHpMult;
    // pick boss type based on boss count
    const bossTypes: BossType[] = ['shooter', 'charger', 'summoner', 'aura'];
    const bt = bossTypes[s.bossDefeated % bossTypes.length];
    const bdef = BOSS_TYPES[bt];
    const baseSpeed = bt === 'charger' ? 80 + wave * 2 : 50 + wave * 1.5;
    return {
      pos: { x: px, y: py },
      hp, maxHp: hp,
      speed: baseSpeed * diff.enemySpeedMult,
      radius: 42,
      damage: 25 * diff.enemyDamageMult,
      type: 'boss',
      color: bdef.color,
      shape: 'hexagon',
      slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
      isBoss: true,
      bossShootTimer: 5,
      bossProjectiles: [],
      xpValue: 50 + wave * 5,
      rotation: 0,
      tier: Math.floor(wave / 10),
      trailTimer: 0,
      fireTimer: 0, fireDps: 0,
      poisonTimer: 0, poisonDps: 0,
      isElite: false,
      bossType: bt,
      chargeTimer: 3,
      isCharging: false,
      chargeDir: { x: 0, y: 0 },
      summonTimer: 4,
      auraRadius: bt === 'aura' ? 120 : 0,
      auraDps: bt === 'aura' ? 10 + wave * 2 : 0,
    };
  }
  const r = Math.random();
  let type: EnemyEntity['type'] = 'normal';
  let hp = (20 + wave * 6) * diff.enemyHpMult;
  let speed = (70 + wave * 2) * diff.enemySpeedMult;
  let radius = 14;
  let dmg = 10 * diff.enemyDamageMult;
  let color = '#4a7a8a';
  let shape: EnemyEntity['shape'] = 'circle';
  if (r < 0.2 && wave > 2) { type = 'fast'; hp = (12 + wave * 4) * diff.enemyHpMult; speed = (130 + wave * 3) * diff.enemySpeedMult; radius = 10; color = '#d4a830'; shape = 'triangle'; }
  else if (r < 0.35 && wave > 4) { type = 'tank'; hp = (50 + wave * 14) * diff.enemyHpMult; speed = (45 + wave) * diff.enemySpeedMult; radius = 20; color = '#8a5a8a'; shape = 'square'; dmg = 10 * diff.enemyDamageMult; }
  // elite chance: 5% after wave 5, scales up
  const isElite = wave > 5 && Math.random() < Math.min(0.12, 0.03 + wave * 0.005);
  if (isElite) {
    hp *= 3;
    radius += 4;
    dmg *= 1.5;
    color = '#b8475a';
  }
  return {
    pos: { x: px, y: py },
    hp, maxHp: hp,
    speed, radius, damage: dmg, type,
    color, shape,
    slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
    isBoss: false, bossShootTimer: 0, bossProjectiles: [],
    xpValue: (type === 'tank' ? 4 : type === 'fast' ? 2 : 1) * (isElite ? 5 : 1),
    rotation: 0,
    tier: s.bossDefeated,
    trailTimer: 0,
    fireTimer: 0, fireDps: 0,
    poisonTimer: 0, poisonDps: 0,
    isElite,
    bossType: 'shooter',
    chargeTimer: 0, isCharging: false, chargeDir: { x: 0, y: 0 },
    summonTimer: 0, auraRadius: 0, auraDps: 0,
  };
}

function startWave(s: GameState): void {
  s.wave++;
  s.stats.wave = s.wave;
  const isBossWave = s.wave % 10 === 0;
  if (isBossWave) {
    s.bossActive = true;
    s.enemies.push(spawnEnemy(s, true));
    // also spawn some minions
    s.waveEnemiesToSpawn = 3 + Math.floor(s.wave / 10);
    playSound('boss');
  } else {
    s.waveEnemiesToSpawn = 4 + Math.floor(s.wave * 1.4);
    playSound('wave');
  }
  s.waveSpawnTimer = 0.5;
}

export function updateWaves(s: GameState, dt: number): void {
  s.waveTimer -= dt;
  if (s.waveTimer <= 0 && s.waveEnemiesToSpawn > 0) {
    s.enemies.push(spawnEnemy(s, false));
    s.waveEnemiesToSpawn--;
    s.waveTimer = Math.max(
      0.3,
      (1.2 - s.wave * 0.02) /
        (DIFFICULTIES.find(d => d.id === s.difficulty)?.spawnRateMult || 1),
    );
  }
  if (
    s.waveEnemiesToSpawn <= 0 &&
    s.enemies.filter(e => !e.isBoss).length === 0 &&
    !s.bossActive
  ) {
    s.waveTimer = 3;
    startWave(s);
  }
  if (s.wave === 0) {
    startWave(s);
  }
}

export function updateMinions(s: GameState, dt: number): void {
  for (let i = s.minions.length - 1; i >= 0; i--) {
    const m = s.minions[i];
    m.life -= dt;
    m.rotation += dt * 3;
    if (m.life <= 0) { s.minions.splice(i, 1); continue; }
    m.attackTimer -= dt;
    if (m.attackTimer <= 0) {
      let nearest: EnemyEntity | null = null;
      let nd = Infinity;
      for (const e of s.enemies) {
        if (e.hp <= 0) continue;
        const d = dist(e.pos, m.pos);
        if (d < 200 && d < nd) { nd = d; nearest = e; }
      }
      if (nearest) {
        let dmg = m.damage;
        // synergy: minion + crit -> minions can crit
        if ((s.player.abilities.crit || 0) > 0 && Math.random() < getCritChance(s)) dmg *= 2;
        dealDamageToEnemy(s, nearest, dmg);
        m.attackTimer = 0.6;
        // devourers evolution: heal on kill
        if (s.player.evolutions.includes('devourers') && nearest.hp <= 0) {
          s.player.hp = Math.min(s.player.maxHp, s.player.hp + 5);
        }
      }
    }
    // move minion toward nearest enemy
    let nearest: EnemyEntity | null = null;
    let nd = Infinity;
    for (const e of s.enemies) {
      if (e.hp <= 0) continue;
      const d = dist(e.pos, m.pos);
      if (d < nd) { nd = d; nearest = e; }
    }
    if (nearest && nd > 80) {
      const dx = nearest.pos.x - m.pos.x;
      const dy = nearest.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      m.pos.x += dx / d * 120 * dt;
      m.pos.y += dy / d * 120 * dt;
    } else {
      // follow player
      const dx = s.player.pos.x - m.pos.x;
      const dy = s.player.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      if (d > 60) { m.pos.x += dx / d * 100 * dt; m.pos.y += dy / d * 100 * dt; }
    }
  }
}

export function updateEnemies(s: GameState, dt: number): void {
  const slowLvl = s.player.abilities.slow || 0;
  for (let i = s.enemies.length - 1; i >= 0; i--) {
    const e = s.enemies[i];
    if (e.hp <= 0) { s.enemies.splice(i, 1); continue; }
    e.rotation += dt;
    if (e.hitFlash > 0) e.hitFlash -= dt;
    // DoT: fire
    if (e.fireTimer > 0) {
      e.fireTimer -= dt;
      e.hp -= e.fireDps * dt;
      if (Math.random() < 0.3) {
        s.particles.push({ pos: { x: e.pos.x + rand(-e.radius, e.radius), y: e.pos.y + rand(-e.radius, e.radius) }, vel: { x: 0, y: -30 }, life: 0.3, maxLife: 0.3, color: '#c46d3d', size: 2 });
      }
      if (e.hp <= 0) { onEnemyDeath(s, e); s.enemies.splice(i, 1); continue; }
    }
    // DoT: poison
    if (e.poisonTimer > 0) {
      e.poisonTimer -= dt;
      e.hp -= e.poisonDps * dt;
      if (Math.random() < 0.2) {
        s.particles.push({ pos: { x: e.pos.x + rand(-e.radius, e.radius), y: e.pos.y + rand(-e.radius, e.radius) }, vel: { x: 0, y: -20 }, life: 0.4, maxLife: 0.4, color: '#5a8c4a', size: 2 });
      }
      if (e.hp <= 0) { onEnemyDeath(s, e); s.enemies.splice(i, 1); continue; }
    }
    // tier-based trailing particles for tougher enemies
    if (e.tier > 0 && !e.isBoss) {
      e.trailTimer -= dt;
      if (e.trailTimer <= 0) {
        e.trailTimer = 0.15;
        s.particles.push({ pos: { ...e.pos }, vel: { x: 0, y: 0 }, life: 0.4, maxLife: 0.4, color: e.color, size: 2 });
      }
    }
    if (e.freezeTimer > 0) { e.freezeTimer -= dt; continue; }
    if (e.slowTimer > 0) e.slowTimer -= dt;
    else e.slowFactor = 1;

    // slow aura
    let speedMult = e.slowFactor;
    if (slowLvl > 0 && dist(e.pos, s.player.pos) < getSlowRadius()) {
      speedMult = Math.min(speedMult, getSlowFactor(s));
    }
    // invisibility cloak: lower aggression
    let aggro = 1;
    if (s.player.artifacts.includes('invisibility_cloak') && s.player.hp / s.player.maxHp < 0.3) {
      aggro = 0.5;
    }

    const dx = s.player.pos.x - e.pos.x;
    const dy = s.player.pos.y - e.pos.y;
    const d = Math.hypot(dx, dy) || 1;
    e.pos.x += (dx / d) * e.speed * speedMult * aggro * dt;
    e.pos.y += (dy / d) * e.speed * speedMult * aggro * dt;

    // collision with player
    if (d < e.radius + PLAYER_RADIUS) {
      damagePlayer(s, e.damage);
      // boss projectile enemies don't self-damage on contact; normal enemies bounce
      if (!e.isBoss) {
        // knockback
        e.pos.x -= (dx / d) * 10;
        e.pos.y -= (dy / d) * 10;
      }
    }

    // boss attacks
    if (e.isBoss) {
      if (e.bossType === 'charger') {
        e.chargeTimer -= dt;
        if (e.isCharging) {
          e.pos.x += e.chargeDir.x * e.speed * 3 * dt;
          e.pos.y += e.chargeDir.y * e.speed * 3 * dt;
          e.isCharging = false; // will re-check below
          // damage on contact during charge
          if (dist(e.pos, s.player.pos) < e.radius + PLAYER_RADIUS) {
            damagePlayer(s, e.damage * 1.5);
            e.isCharging = false;
            e.chargeTimer = 4;
          }
          // stop charging after some distance
          if (!e.isCharging) e.chargeTimer = 4;
        } else if (e.chargeTimer <= 0) {
          // start charge
          const cdx = s.player.pos.x - e.pos.x;
          const cdy = s.player.pos.y - e.pos.y;
          const cd = Math.hypot(cdx, cdy) || 1;
          e.chargeDir = { x: cdx / cd, y: cdy / cd };
          e.isCharging = true;
          e.chargeTimer = 0.5; // charge duration
          playSound('bosshit');
        }
      } else if (e.bossType === 'summoner') {
        e.summonTimer -= dt;
        if (e.summonTimer <= 0) {
          e.summonTimer = 5;
          // spawn 3 minions
          for (let k = 0; k < 3; k++) {
            const a = Math.random() * Math.PI * 2;
            const sx = e.pos.x + Math.cos(a) * 60;
            const sy = e.pos.y + Math.sin(a) * 60;
            s.enemies.push({
              pos: { x: sx, y: sy },
              hp: 20 + s.wave * 4, maxHp: 20 + s.wave * 4,
              speed: 100, radius: 10, damage: 8, type: 'normal',
              color: '#8a5a8a', shape: 'circle',
              slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
              isBoss: false, bossShootTimer: 0, bossProjectiles: [],
              xpValue: 2, rotation: 0, tier: 0, trailTimer: 0,
              fireTimer: 0, fireDps: 0, poisonTimer: 0, poisonDps: 0,
              isElite: false, bossType: 'shooter',
              chargeTimer: 0, isCharging: false, chargeDir: { x: 0, y: 0 },
              summonTimer: 0, auraRadius: 0, auraDps: 0,
            });
          }
        }
        // also shoot
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 7;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 20, radius: 8, alive: true });
          }
        }
      } else if (e.bossType === 'aura') {
        // aura damage to player
        if (dist(e.pos, s.player.pos) < e.auraRadius) {
          damagePlayer(s, e.auraDps * dt);
        }
        // also shoot occasionally
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 6;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 20, radius: 8, alive: true });
          }
        }
      } else {
        // shooter (default)
        e.bossShootTimer -= dt;
        if (e.bossShootTimer <= 0) {
          e.bossShootTimer = 5;
          for (let k = -1; k <= 1; k++) {
            const angle = Math.atan2(dy, dx) + k * 0.3;
            e.bossProjectiles.push({ pos: { ...e.pos }, vel: { x: Math.cos(angle) * 180, y: Math.sin(angle) * 180 }, damage: 25, radius: 8, alive: true });
          }
        }
      }
      // update boss projectiles
      for (let j = e.bossProjectiles.length - 1; j >= 0; j--) {
        const bp = e.bossProjectiles[j];
        bp.pos.x += bp.vel.x * dt;
        bp.pos.y += bp.vel.y * dt;
        const pd = dist(bp.pos, s.player.pos);
        if (pd < bp.radius + PLAYER_RADIUS) {
          damagePlayer(s, bp.damage);
          bp.alive = false;
        }
        if (Math.abs(bp.pos.x - s.player.pos.x) > 1000 || Math.abs(bp.pos.y - s.player.pos.y) > 1000) bp.alive = false;
        if (!bp.alive) e.bossProjectiles.splice(j, 1);
      }
    }
  }
}

