import { BOSS_TYPES, DIFFICULTIES, SPHERE_TYPES } from './gameData';
import { playSound } from './audio';
import {
  dealDamageToEnemy, damagePlayer, damagePlayerDoT, getCritChance, onEnemyDeath
} from './engineCombat';
import {
  dist, rand, getAbilityBranchId, getNearestSphere, getNetworkNodes, getSphereFinalIndex
} from './engineRuntime';
import { sphereModifiers, sphereLevel } from './sphereProgression';
import { analyzeSphereNetwork, getSphereNetworkProfile } from './network';
import { nextRandom } from './rng';
import { BALANCE } from './engineBalance';
import { BOSS_CHARGER_COMMIT_SECONDS, BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS } from './bossBalance';
import { LINK_BREAKER_COOLDOWN_SECONDS, LINK_BREAKER_DISABLED_SECONDS, LINK_BREAKER_TARGET_RANGE, LINK_BREAKER_TELEGRAPH_SECONDS } from './eliteBalance';
import type { GameState, EnemyEntity, SphereEntity, Vec } from './engineTypes';
import type { BossType } from './gameData';

export const PLAYER_RADIUS = 16;

export function getSlowRadius(): number {
  return 300;
}

export function getSlowFactor(s: GameState): number {
  const lvl = s.player.abilities.slow || 0;
  return lvl > 0 ? 1 - (0.1 + (lvl - 1) * 0.05) : 1;
}

export function spawnEnemy(s: GameState, isBoss: boolean): EnemyEntity {
  const wave = s.wave;
  const angle = nextRandom(s) * Math.PI * 2;
  const spawnDist = 700;
  const px = s.player.pos.x + Math.cos(angle) * spawnDist;
  const py = s.player.pos.y + Math.sin(angle) * spawnDist;
  const diff = DIFFICULTIES.find(d => d.id === s.difficulty)!;
  if (isBoss) {
    const hp = (BALANCE.bossHpBase + wave * BALANCE.bossHpPerWave) * diff.enemyHpMult;
    // pick boss type based on boss count
    const bossTypes: BossType[] = ['shooter', 'charger', 'summoner', 'aura'];
    const bt = bossTypes[s.bossDefeated % bossTypes.length];
    const bdef = BOSS_TYPES[bt];
    const baseSpeed = bt === 'charger'
      ? BALANCE.bossChargerSpeedBase + wave * BALANCE.bossChargerSpeedPerWave
      : BALANCE.bossSpeedBase + wave * BALANCE.bossSpeedPerWave;
    return {
      pos: { x: px, y: py },
      hp, maxHp: hp,
      speed: baseSpeed * diff.enemySpeedMult,
      radius: 42,
      damage: (BALANCE.bossDamageBase + wave * BALANCE.bossDamagePerWave) * diff.enemyDamageMult,
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
      elitePulseTimer: 0,
      bossType: bt,
      chargeTimer: 3,
      isCharging: false,
      chargeDir: { x: 0, y: 0 },
      summonTimer: 4,
      auraRadius: bt === 'aura' ? 120 : 0,
      auraDps: bt === 'aura' ? 10 + wave * 2 : 0,
    };
  }
  const r = nextRandom(s);
  let type: EnemyEntity['type'] = 'normal';
  let hp = (BALANCE.normalHpBase + wave * BALANCE.normalHpPerWave) * diff.enemyHpMult;
  let speed = (BALANCE.normalSpeedBase + wave * BALANCE.normalSpeedPerWave) * diff.enemySpeedMult;
  let radius = 14;
  let dmg = (BALANCE.normalDamageBase + wave * BALANCE.normalDamagePerWave) * diff.enemyDamageMult;
  let color = '#4a7a8a';
  let shape: EnemyEntity['shape'] = 'circle';
  if (r < 0.2 && wave > 2) { type = 'fast'; hp = (BALANCE.fastHpBase + wave * BALANCE.fastHpPerWave) * diff.enemyHpMult; speed = (BALANCE.fastSpeedBase + wave * BALANCE.fastSpeedPerWave) * diff.enemySpeedMult; radius = 10; dmg = (BALANCE.fastDamageBase + wave * BALANCE.fastDamagePerWave) * diff.enemyDamageMult; color = '#d4a830'; shape = 'triangle'; }
  else if (r < 0.35 && wave > 4) { type = 'tank'; hp = (BALANCE.tankHpBase + wave * BALANCE.tankHpPerWave) * diff.enemyHpMult; speed = (BALANCE.tankSpeedBase + wave * BALANCE.tankSpeedPerWave) * diff.enemySpeedMult; radius = 20; dmg = (BALANCE.tankDamageBase + wave * BALANCE.tankDamagePerWave) * diff.enemyDamageMult; color = '#8a5a8a'; shape = 'square'; }
  // elite chance: 5% after wave 5, scales up
  const baseType = type;
  const isElite = wave > 5 && nextRandom(s) < Math.min(0.12, 0.03 + wave * 0.005);
  if (isElite) {
    hp *= 3;
    radius += 4;
    dmg *= 1.5;
    color = '#b8475a';
    type = 'elite';
  }
  return {
    pos: { x: px, y: py },
    hp, maxHp: hp,
    speed, radius, damage: dmg, type,
    color, shape,
    slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
    isBoss: false, bossShootTimer: 0, bossProjectiles: [],
    xpValue: (baseType === 'tank' ? 4 : baseType === 'fast' ? 2 : 1) * (isElite ? 5 : 1),
    rotation: 0,
    tier: s.bossDefeated,
    trailTimer: 0,
    fireTimer: 0, fireDps: 0,
    poisonTimer: 0, poisonDps: 0,
    isElite,
    elitePulseTimer: isElite ? 5 : 0,
    bossType: 'shooter',
    chargeTimer: 0, isCharging: false, chargeDir: { x: 0, y: 0 },
    summonTimer: 0, auraRadius: 0, auraDps: 0,
  };
}

export function startWave(s: GameState): void {
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
    s.waveEnemiesToSpawn = BALANCE.enemiesPerWaveBase + Math.floor(s.wave * BALANCE.enemiesPerWaveGrowth);
    playSound('wave');
  }
  s.waveSpawnTimer = 0.5;
}

export function updateMinions(s: GameState, dt: number): void {
  for (let i = s.minions.length - 1; i >= 0; i--) {
    const m = s.minions[i];
    m.life -= dt;
    m.rotation += dt * 3;
    if (m.life <= 0) { s.minions.splice(i, 1); continue; }

    const anchor = getNearestSphere(s, m.pos);
    if (anchor) {
      const angle = m.rotation * 0.7 + i * 2.1;
      const targetX = anchor.pos.x + Math.cos(angle) * 48;
      const targetY = anchor.pos.y + Math.sin(angle) * 48;
      m.pos.x += (targetX - m.pos.x) * Math.min(1, dt * 4);
      m.pos.y += (targetY - m.pos.y) * Math.min(1, dt * 4);

      const abilityBranch = getAbilityBranchId(s, 'minion', 4);
      const abilityFinal = getAbilityBranchId(s, 'minion', 7);
      const nearby = getNearestSphere(s, m.pos, (sphere) => sphere !== anchor && dist(sphere.pos, m.pos) < 240);
      if (abilityBranch === 'minion_relay_drone' && nearby && nextRandom(s) < dt * 4) {
        s.lightnings.push({ from: { ...anchor.pos }, to: { ...nearby.pos }, life: 0.1 });
        anchor.attackTimer = Math.max(0, anchor.attackTimer - 0.16);
        nearby.attackTimer = Math.max(0, nearby.attackTimer - 0.08);
      } else if (abilityBranch === 'minion_guardian') {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - dt * 0.18);
      } else if (nearby && nextRandom(s) < dt * 2) {
        s.lightnings.push({ from: { ...anchor.pos }, to: { ...nearby.pos }, life: 0.08 });
      }
      if (abilityFinal === 'minion_sphere_guard') {
        anchor.attackTimer = Math.max(0, anchor.attackTimer - dt * 0.12);
      }
    } else {
      const dx = s.player.pos.x - m.pos.x;
      const dy = s.player.pos.y - m.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      m.pos.x += dx / d * 90 * dt;
      m.pos.y += dy / d * 90 * dt;
    }

    m.attackTimer -= dt;
    if (m.attackTimer <= 0) {
      let nearest: EnemyEntity | null = null;
      let nd = Infinity;
      for (const e of s.enemies) {
        if (e.hp <= 0) continue;
        const d = dist(e.pos, m.pos);
        if (d < 180 && d < nd) { nd = d; nearest = e; }
      }
      if (nearest) {
        const dmg = (5 + (s.player.abilities.minion || 0) * 2) * (s.player.overloadTimer > 0 ? 1.25 : 1);
        dealDamageToEnemy(s, nearest, dmg);
        m.attackTimer = 0.8;
      }
    }
  }
}

function applyGravityFields(s: GameState, dt: number): void {
  const networkState = analyzeSphereNetwork(getNetworkNodes(s));
  for (const sphere of s.spheres) {
    if (!sphere.alive || sphere.type !== 'gravity') continue;
    const mods = sphereModifiers(s, 'gravity', sphere);
    const branch = s.player.sphereBranches?.gravity;
    const finalIndex = getSphereFinalIndex(s, 'gravity');
    const radius = SPHERE_TYPES.gravity.auraRadius * mods.radius * mods.auraRadius;

    let strength = 34 * Math.min(1.6, sphereLevel(s, 'gravity') * 0.18 + 0.5);
    if (s.player.artifacts.includes('gravity_bead')) strength *= 1.12;
    if (s.player.artifacts.includes('gravity_hook')) strength *= 1.10;
    if (getSphereNetworkProfile(networkState, s.spheres.indexOf(sphere)).cluster) strength *= 1.20;
    if (branch === 'gravity_well') strength *= finalIndex === 1 ? 1.35 : 1.15;
    if (branch === 'gravity_tide') strength *= 1.05;
    if (branch === 'gravity_collapse') strength *= 0.90;

    for (const enemy of s.enemies) {
      if (enemy.hp <= 0) continue;
      const dx = sphere.pos.x - enemy.pos.x;
      const dy = sphere.pos.y - enemy.pos.y;
      const d = Math.hypot(dx, dy);
      if (d <= 1 || d >= radius) continue;

      // Continuous inverse-distance-style falloff: far enemies are only nudged
      // off course, while the force rises sharply as they approach the core.
      const proximity = 1 - d / radius;
      let pullSpeed = 8 + strength * proximity * proximity * 3.4;
      if (branch === 'gravity_tide') {
        const phase = Math.sin(sphere.rotation);
        if (phase < -0.25) pullSpeed *= -0.28;
      }
      pullSpeed = Math.max(-80, Math.min(220, pullSpeed));
      enemy.pos.x += (dx / d) * pullSpeed * dt;
      enemy.pos.y += (dy / d) * pullSpeed * dt;
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
      if (nextRandom(s) < 0.3) {
        s.particles.push({ pos: { x: e.pos.x + rand(s,-e.radius, e.radius), y: e.pos.y + rand(s,-e.radius, e.radius) }, vel: { x: 0, y: -30 }, life: 0.3, maxLife: 0.3, color: '#c46d3d', size: 2 });
      }
      if (e.hp <= 0) { onEnemyDeath(s, e); s.enemies.splice(i, 1); continue; }
    }
    // DoT: poison
    if (e.poisonTimer > 0) {
      e.poisonTimer -= dt;
      e.hp -= e.poisonDps * dt;
      if (nextRandom(s) < 0.2) {
        s.particles.push({ pos: { x: e.pos.x + rand(s,-e.radius, e.radius), y: e.pos.y + rand(s,-e.radius, e.radius) }, vel: { x: 0, y: -20 }, life: 0.4, maxLife: 0.4, color: '#5a8c4a', size: 2 });
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
    if (s.player.artifacts.includes('veil_cloak') && s.player.hp / s.player.maxHp < 0.3) {
      aggro = 0.5;
    }

    const dx = s.player.pos.x - e.pos.x;
    const dy = s.player.pos.y - e.pos.y;
    const d = Math.hypot(dx, dy) || 1;
    e.pos.x += (dx / d) * e.speed * speedMult * aggro * dt;
    e.pos.y += (dy / d) * e.speed * speedMult * aggro * dt;

    // Elite Link Breaker uses a readable telegraph before removing a Sphere from Network participation.
    if (e.isElite) {
      const telegraphTimer = e.elitePulseTelegraphTimer || 0;
      if (telegraphTimer > 0) {
        e.elitePulseTelegraphTimer = Math.max(0, telegraphTimer - dt);
        if (e.elitePulseTelegraphTimer <= 0) {
          const target = e.elitePulseTarget;
          e.elitePulseTarget = undefined;
          if (target?.alive && dist(target.pos, e.pos) <= LINK_BREAKER_TARGET_RANGE + 60) {
            target.networkDisabledTimer = LINK_BREAKER_DISABLED_SECONDS;
            s.flashText = { text: 'NETWORK BREAK', life: 0.8, color: '#b8475a' };
            s.lightnings.push({ from: { ...e.pos }, to: { ...target.pos }, life: 0.30 });
          }
        }
      } else {
        e.elitePulseTimer -= dt;
        if (e.elitePulseTimer <= 0) {
          e.elitePulseTimer = LINK_BREAKER_COOLDOWN_SECONDS;
          let target: SphereEntity | null = null;
          let best = LINK_BREAKER_TARGET_RANGE;
          for (const sphere of s.spheres) {
            if (!sphere.alive || (sphere.networkDisabledTimer || 0) > 0) continue;
            const sd = dist(sphere.pos, e.pos);
            if (sd < best) {
              best = sd;
              target = sphere;
            }
          }
          if (target) {
            e.elitePulseTarget = target;
            e.elitePulseTelegraphTimer = LINK_BREAKER_TELEGRAPH_SECONDS;
          }
        }
      }
    }

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
          // Keep a readable wind-up window, then convert it into a short committed dash.
          if (e.chargeTimer <= BOSS_CHARGER_COMMIT_SECONDS) {
            e.pos.x += e.chargeDir.x * e.speed * 3 * dt;
            e.pos.y += e.chargeDir.y * e.speed * 3 * dt;
            // damage on contact during the committed dash
            if (dist(e.pos, s.player.pos) < e.radius + PLAYER_RADIUS) {
              damagePlayer(s, e.damage * 1.5);
              e.isCharging = false;
              e.chargeTimer = 4;
            }
          }
          if (e.isCharging && e.chargeTimer <= 0) {
            e.isCharging = false;
            e.chargeTimer = 4;
          }
        } else if (e.chargeTimer <= 0) {
          // start charge with an explicit telegraph before the dash begins
          const cdx = s.player.pos.x - e.pos.x;
          const cdy = s.player.pos.y - e.pos.y;
          const cd = Math.hypot(cdx, cdy) || 1;
          e.chargeDir = { x: cdx / cd, y: cdy / cd };
          e.isCharging = true;
          e.chargeTimer = BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS;
          playSound('bosshit');
        }
      } else if (e.bossType === 'summoner') {
        e.summonTimer -= dt;
        if (e.summonTimer <= 0) {
          e.summonTimer = 5;
          // spawn 3 minions
          for (let k = 0; k < 3; k++) {
            const a = nextRandom(s) * Math.PI * 2;
            const sx = e.pos.x + Math.cos(a) * 60;
            const sy = e.pos.y + Math.sin(a) * 60;
            s.enemies.push({
              pos: { x: sx, y: sy },
              hp: 20 + s.wave * 4, maxHp: 20 + s.wave * 4,
              speed: 100, radius: 10, damage: 8, type: 'normal',
              color: '#8a5a8a', shape: 'circle',
              slowTimer: 0, slowFactor: 1, freezeTimer: 0, hitFlash: 0,
              isBoss: false, bossShootTimer: 0, bossProjectiles: [],
              xpValue: 2, rotation: 0, tier: 0, trailTimer: 0, elitePulseTimer: 0,
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
          damagePlayerDoT(s, e.auraDps * dt);
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

  // Apply gravity once after all enemies have completed their normal steering.
  // This bends trajectories smoothly without multiplying the force by enemy count.
  applyGravityFields(s, dt);
}

