import { ARTIFACT_MAP, DIFFICULTIES } from './gameData';import { playSound } from './audio';
import type {
  ArtifactId,
  GameState,
  SphereEntity,
  EnemyEntity,
  Vec,
} from './engineTypes';

function dist(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function rand(min: number, max: number): number {
  return min + Math.random() * (max - min);
}

export function getCritChance(s: GameState): number {
  let c = (s.player.abilities.crit || 0) * 0.1;
  c += (s.shopUpgrades.crit || 0) * 0.05;
  if (s.player.artifacts.includes('luck_talisman')) c += 0.15;
  return c;
}

export function getDodgeChance(s: GameState): number {
  return (s.player.abilities.dodge || 0) * 0.1;
}

export function getVampirePercent(s: GameState): number {
  const lvl = s.player.abilities.vampire || 0;
  return lvl * 0.03; // 3% per level — 3% at lvl1, 15% at lvl5
}

export function getDamageTakenMult(s: GameState): number {
  let m = 1;
  if (s.player.artifacts.includes('defense_medallion')) m *= 0.85;
  return m;
}

function dealDamageToEnemy(s: GameState, enemy: EnemyEntity, dmg: number, fromSphere?: SphereEntity): void {
  let actual = dmg;
  let isCrit = false;
  // crit
  if (fromSphere && Math.random() < getCritChance(s)) { actual *= 2; isCrit = true; }
  // predator claw: every 5th hit
  if (fromSphere && s.player.artifacts.includes('predator_claw')) {
    fromSphere.killsContribution++;
    if (fromSphere.killsContribution % 5 === 0) { actual *= 2; isCrit = true; }
  }
  // buff from chest
  if (s.player.buffTimer > 0) actual *= 1.3;
  enemy.hp -= actual;
  enemy.hitFlash = 0.15;
  // damage number
  s.damageNumbers.push({
    pos: { x: enemy.pos.x + rand(-8, 8), y: enemy.pos.y - enemy.radius - 5 },
    value: Math.round(actual), life: 0.8, maxLife: 0.8, crit: isCrit,
    vel: { x: rand(-30, 30), y: -60 },
  });
  if (isCrit) playSound('crit'); else if (fromSphere) playSound('hit');
  // vampire
  if (fromSphere) {
    const vPct = getVampirePercent(s);
    if (vPct > 0) {
      const heal = actual * vPct;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + heal);
    }
  }
  // vampire ring artifact
  if (enemy.hp <= 0) {
    if (s.player.artifacts.includes('vampire_ring')) {
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + 5);
    }
    onEnemyDeath(s, enemy);
  }
}

function onEnemyDeath(s: GameState, enemy: EnemyEntity): void {
  if (!enemy.isBoss) {
    s.player.kills++;
    s.stats.enemiesKilled++;
    // combo system
    s.player.combo++;
    s.player.comboTimer = 3;
    if (s.player.combo >= 10) s.player.comboMult = 1.5;
    if (s.player.combo >= 25) s.player.comboMult = 2;
    if (s.player.combo >= 50) s.player.comboMult = 3;
    if (s.player.combo >= 100) s.player.comboMult = 4;
  }
  if (enemy.isElite) {
    s.player.eliteKills++;
    playSound('elite');
  }
  playSound(enemy.isBoss ? 'explosion' : 'kill');
  // particles
  for (let i = 0; i < (enemy.isBoss ? 40 : 8); i++) {
    s.particles.push({
      pos: { x: enemy.pos.x, y: enemy.pos.y },
      vel: { x: rand(-180, 180), y: rand(-180, 180) },
      life: rand(0.3, 0.8), maxLife: 0.8,
      color: enemy.color, size: rand(2, 5),
    });
  }
  // XP orb (combo multiplier applies)
  s.xpOrbs.push({
    pos: { x: enemy.pos.x, y: enemy.pos.y },
    value: Math.round(enemy.xpValue * s.player.comboMult), radius: 6, alive: true,
    vel: { x: rand(-40, 40), y: rand(-40, 40) },
  });
  // health pack drop chance
  const dropChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.5 : 0.04);
  if (Math.random() < dropChance) {
    s.healthPacks.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 10 });
  }
  // chest drop: 2% from normal, 20% from elite, 100% from boss
  const chestChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.2 : 0.02);
  if (Math.random() < chestChance) {
    s.chests.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 14 });
  }
  // swift boots
  if (s.player.artifacts.includes('swift_boots')) {
    s.player.swiftBootsTimer = 3;
  }
  // supernova evolution: sphere explodes on kill
  if (s.player.evolutions.includes('supernova')) {
    for (const e of s.enemies) {
      if (e !== enemy && e.hp > 0 && dist(e.pos, enemy.pos) < 90) {
        dealDamageToEnemy(s, e, 15 + s.player.level * 2);
      }
    }
    for (let i = 0; i < 15; i++) {
      s.particles.push({
        pos: { x: enemy.pos.x, y: enemy.pos.y },
        vel: { x: rand(-250, 250), y: rand(-250, 250) },
        life: 0.5, maxLife: 0.5, color: '#d4943d', size: rand(3, 6),
      });
    }
  }
  if (enemy.isBoss) {
    s.bossActive = false;
    s.bossDefeated++;
    const diff = DIFFICULTIES.find(d => d.id === s.difficulty)!;
    s.stats.goldEarned += Math.round((15 + s.wave * 1) * diff.goldMult);
    s.screenShake = 0.5;
    s.flashText = { text: 'BOSS DEFEATED!', life: 2, color: '#d4943d' };
    playSound('boss');
    // trigger artifact choice
    s.pendingArtifact = pickArtifacts(s);
  }
}

function pickArtifacts(s: GameState): ArtifactId[] {
  const owned = new Set(s.player.artifacts);
  const pool = (Object.keys(ARTIFACT_MAP) as ArtifactId[]).filter(a => !owned.has(a));
  const choices: ArtifactId[] = [];
  while (choices.length < 3 && pool.length > 0) {
    const idx = Math.floor(Math.random() * pool.length);
    choices.push(pool.splice(idx, 1)[0]);
  }
  return choices;
}

function damagePlayer(s: GameState, amount: number): void {
  if (s.player.invulnerableTimer > 0) return;
  // dodge
  if (Math.random() < getDodgeChance(s)) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: 0, y: -60 }, life: 0.5, maxLife: 0.5, color: '#e8dcc0', size: 3 });
    return;
  }
  // shield
  if (s.player.shieldCharges > 0) {
    s.player.shieldCharges--;
    for (let i = 0; i < 12; i++) {
      s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(-150, 150), y: rand(-150, 150) }, life: 0.4, maxLife: 0.4, color: '#4a7a8a', size: 3 });
    }
    return;
  }
  let dmg = amount * getDamageTakenMult(s);
  // mirror reflect
  if (s.player.artifacts.includes('mirror') && Math.random() < 0.2) {
    // reflect: find nearest enemy and damage
    const nearest = s.enemies.reduce((best, e) => {
      if (e.hp <= 0) return best;
      const d = dist(e.pos, s.player.pos);
      return d < (best?.dist ?? Infinity) ? { e, dist: d } : best;
    }, null as null | { e: EnemyEntity; dist: number });
    if (nearest) dealDamageToEnemy(s, nearest.e, dmg);
    return;
  }
  s.player.hp -= dmg;
  s.screenShake = Math.min(0.4, s.screenShake + 0.2);
  playSound('damage');
  // combo break on taking damage
  s.player.combo = 0;
  s.player.comboMult = 1;
  s.player.comboTimer = 0;
  // freeze amulet
  if (s.player.artifacts.includes('freeze_amulet')) {
    for (const e of s.enemies) {
      e.slowTimer = 2; e.slowFactor = 0.5;
    }
  }
  // invulnerability evolution
  if (s.player.evolutions.includes('invulnerability') && !s.player.invulnUsed && s.player.hp > 0 && s.player.hp / s.player.maxHp < 0.2) {
    s.player.invulnerableTimer = 3;
    s.player.invulnUsed = true;
    s.flashText = { text: 'INVULNERABLE!', life: 2, color: '#d4943d' };
  }
  if (s.player.hp <= 0) {
    s.player.hp = 0;
    s.gameOver = true;
    s.stats.time = s.time;
    playSound('gameover');
  }
}


export function getCooldownMult(s: GameState): number {
  let m = 1;
  if (s.player.artifacts.includes('mage_pendant')) m *= 0.9;
  return m;
}
