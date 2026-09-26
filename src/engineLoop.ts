import { DIFFICULTIES, type ArtifactId } from './gameData';
import { playSound } from './audio';
import {
  getArtifactMaxHpBonus,
  getArtifactRegenPerSecond,
  pickArtifactChoices,
  pickStellaArtifactChoice, pickStellaArtifactChoices,
} from './artifactSystem';
import { RUNE_DEFS } from './runes';
import { nextRandom } from './rng';
import type { ResonanceSource } from './resonance';
import { BALANCE } from './engineBalance';
import { PLAYER_RADIUS, STELLA_LEGENDARY_CUTOFF_SECONDS, getXpToNextLevel } from './engineState';
import { getMoveSpeed, getXpMult, getMagnetRadius } from './engineStats';
import { updateSpheres } from './engineSpheres';
import { spawnEnemy, startWave, updateMinions, updateEnemies } from './engineEnemies';
import { dealDamageToEnemy } from './engineCombat';
import { generateUpgradeChoices } from './engineProgression';
import { chargeResonance as chargeResonanceRuntime } from './engineResonance';
import { dist, rand, clamp } from './engineRuntime';
import type { GameState, RuneEntity } from './engineTypes';

function pickArtifacts(s: GameState): ArtifactId[] {
  return pickArtifactChoices(s, 3, false, () => nextRandom(s));
}

function chargeResonance(s: GameState, source: ResonanceSource): void {
  chargeResonanceRuntime(s, source, dealDamageToEnemy);
}


export function claimStella(s: GameState): void {
  if (!s.pendingStella) return;

  s.pendingStella = false;
  const choices = s.time < STELLA_LEGENDARY_CUTOFF_SECONDS
    ? pickStellaArtifactChoices(s, s.player.artifacts.includes('quantum_fold') ? 4 : 3, () => nextRandom(s))
    : [];
  if (choices.length > 0) {
    s.stellaLegendaryClaims++;
    s.pendingArtifact = choices;
  } else {
    s.pendingArtifact = pickArtifacts(s);
  }
  playSound('chest');
}


export function applyArtifact(s: GameState, id: ArtifactId): void {
  if (s.player.artifacts.includes(id)) return;
  s.player.artifacts.push(id);
  const hpBonus = getArtifactMaxHpBonus(id);
  if (hpBonus > 0) { s.player.maxHp += hpBonus; s.player.hp += hpBonus; }
  playSound('chest');
}

// ===== Main update =====
export function update(s: GameState, dt: number): void {
  if (s.paused || s.gameOver) return;
  if (s.pendingUpgrade || s.pendingArtifact || s.pendingStella) return;

  s.time += dt;
  s.stats.time = s.time;
  s.networkFrameId += 1;
  s.networkFrame = null;
  // Progressive active slots: Dash is always free, then three non-Dash slots
  // open during the run. This keeps the active layer tactical rather than
  // turning the HUD into a keyboard.
  if (s.player.level >= 5) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 1);
  if (s.player.level >= 12) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 2);
  if (s.player.level >= 20) s.player.activeAbilitySlots = Math.max(s.player.activeAbilitySlots, 3);

  // character timers
  if (s.player.hunterMarkTimer > 0) {
    s.player.hunterMarkTimer -= dt;
    if (s.player.hunterMarkTimer <= 0) {
      s.player.hunterMarkTimer = 0;
      s.player.hunterMarkTarget = null;
      s.player.hunterHitCount = 0;
    }
  }
  if (s.player.hunterHuntTimer > 0) {
    s.player.hunterHuntTimer -= dt;
    if (s.player.hunterHuntTimer <= 0) {
      s.player.hunterHuntTimer = 0;
      s.player.hunterHuntTarget = null;
    }
  }
  if (s.player.hunterTrophyTimer > 0) s.player.hunterTrophyTimer = Math.max(0, s.player.hunterTrophyTimer - dt);
  if (s.player.engineerRelayTimer > 0) {
    s.player.engineerRelayTimer -= dt;
    if (s.player.engineerRelayTimer <= 0) {
      s.player.engineerRelayTimer = 0;
      s.player.engineerRelaySource = null;
    }
  }
  if (s.player.alchemistCatalystTimer > 0) s.player.alchemistCatalystTimer = Math.max(0, s.player.alchemistCatalystTimer - dt);

  // combo timer
  if (s.player.comboTimer > 0) {
    s.player.comboTimer -= dt;
    if (s.player.comboTimer <= 0) {
      s.player.combo = 0;
      s.player.comboMult = 1;
    }
  }
  // buff timer
  if (s.player.buffTimer > 0) s.player.buffTimer -= dt;
  // dash cooldown
  if (s.player.dashCooldown > 0) s.player.dashCooldown -= dt;
  // dash active
  if (s.player.dashTimer > 0) {
    s.player.dashTimer -= dt;
    s.player.pos.x += s.player.dashDir.x * 600 * dt;
    s.player.pos.y += s.player.dashDir.y * 600 * dt;
    s.player.pos.x = clamp(s.player.pos.x, -s.worldWidth / 2, s.worldWidth / 2);
    s.player.pos.y = clamp(s.player.pos.y, -s.worldHeight / 2, s.worldHeight / 2);
    // dash trail particles
    if (nextRandom(s) < 0.5) {
      s.particles.push({ pos: { ...s.player.pos }, vel: { x: 0, y: 0 }, life: 0.3, maxLife: 0.3, color: '#d4943d', size: 3 });
    }
  }

  // damage numbers
  for (let i = s.damageNumbers.length - 1; i >= 0; i--) {
    const dn = s.damageNumbers[i];
    dn.pos.x += dn.vel.x * dt;
    dn.pos.y += dn.vel.y * dt;
    dn.vel.y += 80 * dt;
    dn.life -= dt;
    if (dn.life <= 0) s.damageNumbers.splice(i, 1);
  }

  // Stella is a physical world chest. Picking it up opens the Legendary choice.
  for (let i = s.stellaChests.length - 1; i >= 0; i--) {
    const chest = s.stellaChests[i];
    if (!chest.alive) { s.stellaChests.splice(i, 1); continue; }
    if (dist(chest.pos, s.player.pos) < PLAYER_RADIUS + chest.radius) {
      chest.alive = false;
      s.stellaChests.splice(i, 1);
      const choiceCount = s.player.artifacts.includes('quantum_fold') ? 4 : 3;
      const choices = pickStellaArtifactChoices(s, choiceCount, () => nextRandom(s));
      s.stellaLegendaryClaims++;
      s.pendingArtifact = choices.length > 0 ? choices : pickArtifacts(s);
      s.flashText = { text: 'STELLA', life: 1.2, color: '#ffb84d' };
      playSound('chest');
    }
  }

  // ordinary artifact chests pickup
  for (let i = s.chests.length - 1; i >= 0; i--) {
    const chest = s.chests[i];
    if (!chest.alive) { s.chests.splice(i, 1); continue; }
    if (dist(chest.pos, s.player.pos) < PLAYER_RADIUS + chest.radius) {
      chest.alive = false;
      s.player.chestOpens++;
      s.pendingArtifact = pickArtifacts(s);
      s.chests.splice(i, 1);
      playSound('chest');
    }
  }

  // mutation check
  checkMutation(s);

  // player movement
  let mx = 0, my = 0;
  if (s.keys['w'] || s.keys['arrowup']) my -= 1;
  if (s.keys['s'] || s.keys['arrowdown']) my += 1;
  if (s.keys['a'] || s.keys['arrowleft']) mx -= 1;
  if (s.keys['d'] || s.keys['arrowright']) mx += 1;
  const len = Math.hypot(mx, my);
  if (len > 0) { mx /= len; my /= len; }
  const sp = getMoveSpeed(s);
  s.player.pos.x += mx * sp * dt;
  s.player.pos.y += my * sp * dt;
  s.player.pos.x = clamp(s.player.pos.x, -s.worldWidth / 2, s.worldWidth / 2);
  s.player.pos.y = clamp(s.player.pos.y, -s.worldHeight / 2, s.worldHeight / 2);

  // fire trail
  if (s.player.fireTrailTimer > 0) s.player.fireTrailTimer = Math.max(0, s.player.fireTrailTimer - dt);

  const artifactRegen = getArtifactRegenPerSecond(s);
  if (artifactRegen > 0) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + artifactRegen * dt);
  }
  // mutation stage 4: +25% regen
  if (s.player.mutationStage >= 4) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + dt * 1.25);
  }

  // chaos orb
  if (s.player.artifacts.includes('chaos_orb')) {
    s.player.chaosOrbTimer += dt;
    if (s.player.chaosOrbTimer >= 10) {
      s.player.chaosOrbTimer = 0;
      s.player.chaosOrbBuff = nextRandom(s) < 0.5 ? 'dmg' : 'radius';
      s.player.chaosOrbBuffTimer = 3;
    }
  }
  if (s.player.chaosOrbBuffTimer > 0) s.player.chaosOrbBuffTimer -= dt;

  // cooldowns
  const p = s.player;
  if (p.blastCooldown > 0) p.blastCooldown = Math.max(0, p.blastCooldown - dt);
  if (p.teleportCooldown > 0) p.teleportCooldown = Math.max(0, p.teleportCooldown - dt);
  if (p.shieldCooldown > 0) p.shieldCooldown = Math.max(0, p.shieldCooldown - dt);
  if (p.minionCooldown > 0) p.minionCooldown = Math.max(0, p.minionCooldown - dt);
  if (p.lightningCooldown > 0) p.lightningCooldown = Math.max(0, p.lightningCooldown - dt);
  if (p.timestopCooldown > 0) p.timestopCooldown = Math.max(0, p.timestopCooldown - dt);
  if (p.darkritualCooldown > 0) p.darkritualCooldown = Math.max(0, p.darkritualCooldown - dt);
  if (p.overloadTimer > 0) p.overloadTimer = Math.max(0, p.overloadTimer - dt);
  if (p.fireTrailCooldown > 0) p.fireTrailCooldown = Math.max(0, p.fireTrailCooldown - dt);
  if (s.player.shieldTimer > 0) s.player.shieldTimer -= dt;
  if (s.player.swiftBootsTimer > 0) s.player.swiftBootsTimer -= dt;
  if (s.player.teleportDamageBuffTimer > 0) s.player.teleportDamageBuffTimer = Math.max(0, s.player.teleportDamageBuffTimer - dt);
  if (s.player.fireCatalystTimer > 0) s.player.fireCatalystTimer = Math.max(0, s.player.fireCatalystTimer - dt);
  if (s.player.timestopTimer > 0) s.player.timestopTimer = Math.max(0, s.player.timestopTimer - dt);
  if (s.player.invulnerableTimer > 0) s.player.invulnerableTimer = Math.max(0, s.player.invulnerableTimer - dt);
  if (s.player.dodgeTimer > 0) s.player.dodgeTimer -= dt;
  for (const sphere of s.spheres) {
    if (sphere.networkDisabledTimer > 0) sphere.networkDisabledTimer = Math.max(0, sphere.networkDisabledTimer - dt);
  }
  if (s.player.contactDamageCooldown > 0) s.player.contactDamageCooldown = Math.max(0, s.player.contactDamageCooldown - dt);

  // spheres
  updateSpheres(s, dt);

  // minions
  updateMinions(s, dt);

  // waves
  s.waveTimer -= dt;
  if (s.waveTimer <= 0 && s.waveEnemiesToSpawn > 0) {
    s.enemies.push(spawnEnemy(s, false));
    s.waveEnemiesToSpawn--;
    s.waveTimer = Math.max(0.3, (1.2 - s.wave * 0.02) / (DIFFICULTIES.find(d => d.id === s.difficulty)?.spawnRateMult || 1));
  }
  if (s.waveEnemiesToSpawn <= 0 && s.enemies.filter(e => !e.isBoss).length === 0 && !s.bossActive) {
    s.waveTimer = 3;
    startWave(s);
  }
  // initial wave
  if (s.wave === 0) {
    startWave(s);
  }

  // enemies
  updateEnemies(s, dt);

  // xp orbs
  updateXpOrbs(s, dt);

  // health packs
  updateHealthPacks(s);
  updateRunes(s, dt);

  // particles
  for (let i = s.particles.length - 1; i >= 0; i--) {
    const p = s.particles[i];
    p.pos.x += p.vel.x * dt; p.pos.y += p.vel.y * dt;
    p.vel.x *= 0.95; p.vel.y *= 0.95;
    p.life -= dt;
    if (p.life <= 0) s.particles.splice(i, 1);
  }

  // fire trails
  for (let i = s.fireTrails.length - 1; i >= 0; i--) {
    const ft = s.fireTrails[i];
    ft.life -= dt;
    if (ft.life <= 0) { s.fireTrails.splice(i, 1); continue; }
    for (const e of s.enemies) {
      if (e.hp > 0 && dist(e.pos, ft.pos) < 25) {
        dealDamageToEnemy(s, e, ft.damage * dt * 3);
        // ice path evolution: freeze
        if (s.player.evolutions.includes('icepath')) {
          e.freezeTimer = 2;
        }
        // synergy: firetrail + slow -> extra slow
        if ((s.player.abilities.slow || 0) > 0) {
          e.slowTimer = 2; e.slowFactor = 0.7;
        }
      }
    }
  }

  // lightnings
  for (let i = s.lightnings.length - 1; i >= 0; i--) {
    s.lightnings[i].life -= dt;
    if (s.lightnings[i].life <= 0) s.lightnings.splice(i, 1);
  }

  // screen shake
  if (s.screenShake > 0) s.screenShake = Math.max(0, s.screenShake - dt);
  if (s.flashText) {
    s.flashText.life -= dt;
    if (s.flashText.life <= 0) s.flashText = null;
  }

  // camera
  s.camera.x = s.player.pos.x;
  s.camera.y = s.player.pos.y;

  // boss arrow
  s.bossArrow = null;
  if (s.player.artifacts.includes('foresight_eye')) {
    const boss = s.enemies.find(e => e.isBoss && e.hp > 0);
    if (boss) {
      const dx = boss.pos.x - s.player.pos.x;
      const dy = boss.pos.y - s.player.pos.y;
      const d = Math.hypot(dx, dy);
      if (d > 0) s.bossArrow = { x: dx / d, y: dy / d };
    }
  }
  // Network cache is valid only for this gameplay update.
  s.networkFrame = null;
}

function checkMutation(s: GameState): void {
  const lvl = s.player.level;
  let stage = 0;
  if (lvl >= 40) stage = 4;
  else if (lvl >= 30) stage = 3;
  else if (lvl >= 20) stage = 2;
  else if (lvl >= 10) stage = 1;
  if (stage > s.player.mutationStage) {
    s.player.mutationStage = stage;
    s.flashText = { text: 'MUTATION!', life: 2, color: '#b8475a' };
    for (let i = 0; i < 40; i++) {
      const a = nextRandom(s) * Math.PI * 2;
      s.particles.push({
        pos: { ...s.player.pos },
        vel: { x: Math.cos(a) * rand(s,100, 250), y: Math.sin(a) * rand(s,100, 250) },
        life: 1, maxLife: 1, color: ['#8a5a8a', '#c4453d', '#d4943d', '#e8dcc0'][stage - 1], size: rand(s,3, 6),
      });
    }
  }
}

function updateXpOrbs(s: GameState, dt: number): void {
  const magnetR = getMagnetRadius(s);
  for (let i = s.xpOrbs.length - 1; i >= 0; i--) {
    const orb = s.xpOrbs[i];
    orb.pos.x += orb.vel.x * dt; orb.pos.y += orb.vel.y * dt;
    orb.vel.x *= 0.9; orb.vel.y *= 0.9;
    const d = dist(orb.pos, s.player.pos);
    if (d < magnetR) {
      const dx = s.player.pos.x - orb.pos.x;
      const dy = s.player.pos.y - orb.pos.y;
      const pull = Math.min(1, 20 * dt);
      orb.pos.x += dx * pull;
      orb.pos.y += dy * pull;
    }
    if (d < PLAYER_RADIUS + 6) {
      gainXp(s, orb.value);
      playSound('pickup');
      s.xpOrbs.splice(i, 1);
    }
  }
}

function gainXp(s: GameState, amount: number): void {
  const mult = getXpMult(s);
  const gained = amount * mult;
  // echo accumulator: spheres absorb xp
  if (s.player.evolutions.includes('echoaccumulator')) {
    s.player.sphereXpAccumulator += gained * 0.3;
  }
  s.player.xp += gained;
  while (s.player.xp >= s.player.xpToNext) {
    s.player.xp -= s.player.xpToNext;
    s.player.level++;
    // HP per level: +8 max HP and heal 8
    s.player.maxHp += BALANCE.hpPerLevel;
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + BALANCE.hpPerLevel);
    s.player.xpToNext = getXpToNextLevel(s.player.level);
    s.pendingUpgrade = generateUpgradeChoices(s);
    playSound('levelup');
  }
}

function activateRune(s: GameState, rune: RuneEntity): void {
  const radius = 240;
  switch (rune.type) {
    case 'overdrive':
      for (const sphere of s.spheres) sphere.attackTimer = Math.max(0, sphere.attackTimer - sphere.attackDelay * 0.75);
      break;
    case 'phase':
      s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 1.0);
      break;
    case 'harvest':
      for (let i = s.xpOrbs.length - 1; i >= 0; i--) {
        const orb = s.xpOrbs[i];
        if (dist(orb.pos, rune.pos) <= 450) {
          gainXp(s, orb.value);
          s.xpOrbs.splice(i, 1);
        }
      }
      break;
    case 'purge':
      for (const enemy of s.enemies) {
        if (!enemy.isBoss && enemy.hp > 0 && dist(enemy.pos, rune.pos) <= radius) {
          dealDamageToEnemy(s, enemy, 45 + s.player.level * 4);
        }
      }
      break;
    case 'resonance':
      chargeResonance(s, 'rune');
      for (const sphere of s.spheres) {
        sphere.formationHitCount += 2;
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.5);
      }
      break;
    case 'fortify':
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 2);
      break;
    case 'hunt': {
      const target = s.enemies
        .filter((enemy) => enemy.hp > 0 && (enemy.isElite || enemy.isBoss))
        .sort((a, b) => dist(a.pos, rune.pos) - dist(b.pos, rune.pos))[0];
      if (target) {
        s.player.hunterMarkTarget = target;
        s.player.hunterMarkTimer = 8;
      }
      break;
    }
    case 'echo':
      s.player.buffTimer = Math.max(s.player.buffTimer, 4);
      break;
    case 'gravity':
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, rune.pos) > radius) continue;
        const dx = rune.pos.x - enemy.pos.x;
        const dy = rune.pos.y - enemy.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        const pull = Math.min(90, d * 0.55);
        enemy.pos.x += dx / d * pull;
        enemy.pos.y += dy / d * pull;
      }
      break;
  }
  const def = RUNE_DEFS[rune.type];
  s.flashText = { text: def.name.ru.toUpperCase(), life: 1.0, color: def.color };
  for (let i = 0; i < 18; i++) {
    const a = nextRandom(s) * Math.PI * 2;
    s.particles.push({ pos: { ...rune.pos }, vel: { x: Math.cos(a) * 150, y: Math.sin(a) * 150 }, life: 0.55, maxLife: 0.55, color: def.color, size: 3 });
  }
}

function updateRunes(s: GameState, dt: number): void {
  for (let i = s.runes.length - 1; i >= 0; i--) {
    const rune = s.runes[i];
    rune.life -= dt;
    if (rune.life <= 0 || !rune.alive) {
      s.runes.splice(i, 1);
      continue;
    }
    rune.pos.y += Math.sin((s.time + i) * 3) * dt * 3;
    if (dist(rune.pos, s.player.pos) <= rune.radius + PLAYER_RADIUS) {
      activateRune(s, rune);
      s.runes.splice(i, 1);
    }
  }
}

function updateHealthPacks(s: GameState): void {
  for (let i = s.healthPacks.length - 1; i >= 0; i--) {
    const hp = s.healthPacks[i];
    const d = dist(hp.pos, s.player.pos);
    if (d < PLAYER_RADIUS + hp.radius) {
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + 20);
      s.healthPacks.splice(i, 1);
      playSound('health');
      for (let k = 0; k < 10; k++) {
        s.particles.push({ pos: { ...hp.pos }, vel: { x: rand(s,-100, 100), y: rand(s,-100, 100) }, life: 0.5, maxLife: 0.5, color: '#5a8c4a', size: 3 });
      }
    }
  }
}

export function activateDash(s: GameState): void {
  if (s.player.dashCooldown > 0) return;
  let dx = 0, dy = 0;
  if (s.keys['w'] || s.keys['arrowup']) dy -= 1;
  if (s.keys['s'] || s.keys['arrowdown']) dy += 1;
  if (s.keys['a'] || s.keys['arrowleft']) dx -= 1;
  if (s.keys['d'] || s.keys['arrowright']) dx += 1;
  if (dx === 0 && dy === 0) { dx = 0; dy = -1; } // dash up by default
  const len = Math.hypot(dx, dy) || 1;
  s.player.dashDir = { x: dx / len, y: dy / len };
  s.player.dashTimer = 0.2;
  s.player.dashCooldown = 3;
  s.player.invulnerableTimer = Math.max(s.player.invulnerableTimer, 0.3);
  s.player.dashCount++;
  playSound('dash');
}
