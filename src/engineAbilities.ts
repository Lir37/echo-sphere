import { playSound } from './audio';
import type { AbilityType, GameState } from './engineTypes';
import {
  dealDamageToEnemy,
  dist,
  rand,
  getCooldownMult,
  getVampirePercent,
} from './engineCombat';

function clamp(v: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, v));
}


function activateBlast(s: GameState): void {
  const lvl = s.player.abilities.blast || 0;
  if (lvl === 0) return;
  const cd = (30 - (lvl - 1) * 2) * getCooldownMult(s);
  if (s.player.blastCooldown > 0) return;
  s.player.blastCooldown = cd;
  const radius = 200;
  for (const e of s.enemies) {
    if (e.hp > 0 && dist(e.pos, s.player.pos) < radius) {
      dealDamageToEnemy(s, e, 30 + lvl * 10);
    }
  }
  for (let i = 0; i < 30; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({
      pos: { ...s.player.pos },
      vel: { x: Math.cos(a) * rand(100, 300), y: Math.sin(a) * rand(100, 300) },
      life: 0.6, maxLife: 0.6, color: '#c46d3d', size: rand(3, 7),
    });
  }
  s.screenShake = 0.3;
  // synergy: blast + shield -> heal 10%
  if ((s.player.abilities.shield || 0) > 0) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + s.player.maxHp * 0.1);
  }
  // barrier evolution: shield for 5s
  if (s.player.evolutions.includes('barrier')) {
    s.player.shieldCharges = Math.max(s.player.shieldCharges, 3);
    s.player.shieldTimer = 5;
  }
}

function activateShield(s: GameState): void {
  const lvl = s.player.abilities.shield || 0;
  if (lvl === 0) return;
  const cd = 20 * getCooldownMult(s);
  if (s.player.shieldCooldown > 0) return;
  s.player.shieldCooldown = cd;
  s.player.shieldCharges = 1 + Math.floor((lvl - 1) / 2);
  s.player.shieldTimer = 10;
}

function activateTeleport(s: GameState): void {
  const lvl = s.player.abilities.teleport || 0;
  if (lvl === 0) return;
  // blink evolution: no CD but HP cost
  if (s.player.evolutions.includes('blink')) {
    if (s.player.hp <= s.player.maxHp * 0.1) return;
    s.player.hp -= s.player.maxHp * 0.1;
    doTeleport(s);
    return;
  }
  const cd = (15 - (lvl - 1) * 2) * getCooldownMult(s);
  if (s.player.teleportCooldown > 0) return;
  s.player.teleportCooldown = cd;
  doTeleport(s);
  // synergy: teleport + attackspeed -> double damage 3s
  if ((s.player.abilities.attackspeed || 0) > 0) {
    s.player.teleportDamageBuffTimer = 3;
  }
}

function doTeleport(s: GameState): void {
  for (let i = 0; i < 20; i++) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(-150, 150), y: rand(-150, 150) }, life: 0.5, maxLife: 0.5, color: '#5a8c4a', size: 3 });
  }
  s.player.pos.x = rand(s.player.pos.x - 300, s.player.pos.x + 300);
  s.player.pos.y = rand(s.player.pos.y - 300, s.player.pos.y + 300);
  s.player.pos.x = clamp(s.player.pos.x, -s.worldWidth / 2, s.worldWidth / 2);
  s.player.pos.y = clamp(s.player.pos.y, -s.worldHeight / 2, s.worldHeight / 2);
  for (let i = 0; i < 20; i++) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(-150, 150), y: rand(-150, 150) }, life: 0.5, maxLife: 0.5, color: '#5a8c4a', size: 3 });
  }
}

function activateFireTrail(s: GameState): void {
  const lvl = s.player.abilities.firetrail || 0;
  if (lvl === 0) return;
  const cd = 25 * getCooldownMult(s);
  if (s.player.fireTrailCooldown > 0) return;
  s.player.fireTrailCooldown = cd;
  s.player.fireTrailTimer = 5 + (lvl - 1);
}

function activateMinion(s: GameState): void {
  const lvl = s.player.abilities.minion || 0;
  if (lvl === 0) return;
  const cd = 30 * getCooldownMult(s);
  if (s.player.minionCooldown > 0) return;
  s.player.minionCooldown = cd;
  const count = 1 + Math.floor((lvl - 1) / 2);
  for (let i = 0; i < count; i++) {
    s.minions.push({
      pos: { x: s.player.pos.x + rand(-40, 40), y: s.player.pos.y + rand(-40, 40) },
      hp: 1, attackTimer: 0, life: 10, radius: 12, damage: 8 + lvl * 2, rotation: 0,
    });
  }
}

function activateLightning(s: GameState): void {
  const lvl = s.player.abilities.lightning || 0;
  if (lvl === 0) return;
  const cd = 20 * getCooldownMult(s);
  if (s.player.lightningCooldown > 0) return;
  s.player.lightningCooldown = cd;
  const targets = 1 + Math.floor((lvl - 1) / 2);
  const alive = s.enemies.filter(e => e.hp > 0);
  // thunderstorm evolution: hit all
  if (s.player.evolutions.includes('thunderstorm')) {
    for (const e of alive) {
      s.lightnings.push({ from: { ...s.player.pos }, to: { ...e.pos }, life: 0.3 });
      dealDamageToEnemy(s, e, (40 + lvl * 15) * 0.6);
      // synergy: lightning + vampire -> double heal
      const vPct = getVampirePercent(s);
      if (vPct > 0) s.player.hp = Math.min(s.player.maxHp, s.player.hp + (40 + lvl * 15) * 0.6 * vPct * 2);
    }
  } else {
    const shuffled = [...alive].sort(() => Math.random() - 0.5);
    for (let i = 0; i < Math.min(targets, shuffled.length); i++) {
      const e = shuffled[i];
      s.lightnings.push({ from: { ...s.player.pos }, to: { ...e.pos }, life: 0.3 });
      dealDamageToEnemy(s, e, 40 + lvl * 15);
      // synergy
      const vPct = getVampirePercent(s);
      if (vPct > 0) s.player.hp = Math.min(s.player.maxHp, s.player.hp + (40 + lvl * 15) * vPct * 2);
    }
  }
}

function activateTimeStop(s: GameState): void {
  const lvl = s.player.abilities.timestop || 0;
  if (lvl === 0) return;
  const cd = 40 * getCooldownMult(s);
  if (s.player.timestopCooldown > 0) return;
  s.player.timestopCooldown = cd;
  s.player.timestopTimer = 3 + (lvl - 1);
  for (const e of s.enemies) e.freezeTimer = s.player.timestopTimer;
  s.flashText = { text: 'TIME STOP!', life: 1.5, color: '#4a7a8a' };
}

function activateDarkRitual(s: GameState): void {
  const lvl = s.player.abilities.darkritual || 0;
  if (lvl === 0) return;
  const cd = 30 * getCooldownMult(s);
  if (s.player.darkritualCooldown > 0) return;
  if (s.player.hp <= s.player.maxHp * 0.2) return;
  s.player.darkritualCooldown = cd;
  s.player.hp -= s.player.maxHp * 0.2;
  const radius = 500;
  for (const e of s.enemies) {
    if (e.hp > 0 && dist(e.pos, s.player.pos) < radius) {
      dealDamageToEnemy(s, e, 50 + lvl * 25);
    }
  }
  for (let i = 0; i < 40; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({
      pos: { ...s.player.pos },
      vel: { x: Math.cos(a) * rand(150, 400), y: Math.sin(a) * rand(150, 400) },
      life: 0.7, maxLife: 0.7, color: '#8a5a8a', size: rand(3, 7),
    });
  }
  s.screenShake = 0.4;
}

export function activateByKey(s: GameState, key: string): void {
  const ability = s.activeKeyMap[key];
  if (!ability) return;
  switch (ability) {
    case 'blast': activateBlast(s); break;
    case 'shield': activateShield(s); break;
    case 'teleport': activateTeleport(s); break;
    case 'firetrail': activateFireTrail(s); break;
    case 'minion': activateMinion(s); break;
    case 'lightning': activateLightning(s); break;
    case 'timestop': activateTimeStop(s); break;
    case 'darkritual': activateDarkRitual(s); break;
  }
}
