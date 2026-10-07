import { ARTIFACT_MAP, DIFFICULTIES } from './gameData';
import { playSound } from './audio';
import {
  getCharacterId,
  getCharacterDamageMultiplier,
  getCharacterDamageTakenMultiplier,
  getEngineerNetworkRange,
  getCharacterStatusDamageMultiplier,
  getHunterMarkMultiplier,
  shouldMarkHunterTarget,
  getCharacterFormation,
  getFormationDamageTakenMultiplier,
} from './characterRuntime';
import {
  getArtifactDamageTakenMultiplier,
  getArtifactCooldownMultiplier,
  getArtifactCritChanceBonus,
  getArtifactDodgeChanceBonus,
  getArtifactVampireBonus,
  getArtifactReflectChance,
  getArtifactSetBehavior,
  pickArtifactChoices,
} from './artifactSystem';
import { sphereLevel, sphereModifiers, getSphereElementForBranch, getAbilityFinalArchetype } from './sphereProgression';
import { getSphereNetworkProfile, getLinkedNodeIndexes, getFormationBonusMultiplier } from './network';
import { nextRandom } from './rng';
import { RUNE_DEFS, type RuneType } from './runes';
import { canReceivePlayerDamage, canReceivePlayerDoTDamage, CRIT_BASE, CRIT_MULTIPLIER_BASE, getContextualCritChance } from './combatRules';
import type { GameState, SphereEntity, EnemyEntity, Vec } from './engineTypes';
import type { ArtifactId } from './gameData';
import { dist, rand, getNetworkFrame, getAbilityBranchId, getNearestSphere, getSphereFinalIndex } from './engineRuntime';
import { chargeResonance } from './engineResonance';
import { getRegionRepulsionMultiplier } from './region';

function preFinalBranchPower(s: GameState, type: SphereEntity['type']): number {
  const level = sphereLevel(s, type);
  return level >= 6 ? 1.30 : level >= 5 ? 1.15 : 1;
}

function registerHunterHit(s: GameState, enemy: EnemyEntity, sphere: SphereEntity): void {
  if (getCharacterId(s) !== 'hunter') return;
  if (!['sniper', 'chain', 'prism', 'void'].includes(sphere.type)) return;
  if (!shouldMarkHunterTarget(enemy)) return;

  const p = s.player;
  const duration = p.characterMasteryLevel >= 2 ? 6 : 5;
  const threshold = p.characterMasteryLevel >= 4 ? 4 : 5;
  const activeSameTarget = p.hunterMarkTarget === enemy && p.hunterMarkTimer > 0;

  if (!activeSameTarget) {
    p.hunterMarkTarget = enemy;
    p.hunterMarkTimer = duration;
    p.hunterHitCount = 1;
    for (let i = 0; i < 10; i++) {
      const a = nextRandom(s) * Math.PI * 2;
      const speed = 50 + nextRandom(s) * 80;
      s.particles.push({
        pos: { ...enemy.pos },
        vel: { x: Math.cos(a) * speed, y: Math.sin(a) * speed },
        life: 0.35, maxLife: 0.35, color: '#b8475a', size: 2.5,
      });
    }
    return;
  }

  p.hunterMarkTimer = duration;
  p.hunterHitCount++;
  if (p.hunterHitCount >= threshold) {
    p.hunterHuntTarget = enemy;
    p.hunterHuntTimer = 3;
    p.hunterHitCount = 0;
    for (let i = 0; i < 18; i++) {
      const a = nextRandom(s) * Math.PI * 2;
      const r = 18 + nextRandom(s) * 24;
      s.particles.push({
        pos: { x: enemy.pos.x + Math.cos(a) * r, y: enemy.pos.y + Math.sin(a) * r },
        vel: { x: 0, y: -25 },
        life: 0.5, maxLife: 0.5, color: '#c4453d', size: 3,
      });
    }
    s.screenShake = Math.min(0.16, s.screenShake + 0.04);
  }
}


export function consumeEngineerRelayBonus(s: GameState, sphere: SphereEntity): number {
  if (getCharacterId(s) !== 'engineer') return 1;
  const source = s.player.engineerRelaySource;
  if (!source || s.player.engineerRelayTimer <= 0 || source === sphere) return 1;
  const range = getEngineerNetworkRange(s);
  if (dist(source.pos, sphere.pos) > range) return 1;
  s.player.engineerRelaySource = null;
  s.player.engineerRelayTimer = 0;
  return 1.25;
}


export function triggerEngineerRelay(s: GameState, sphere: SphereEntity): void {
  if (getCharacterId(s) !== 'engineer') return;
  s.player.engineerRelaySource = sphere;
  s.player.engineerRelayTimer = s.player.characterMasteryLevel >= 4 ? 0.55 : 0.4;
  for (const neighbour of s.spheres) {
    if (neighbour === sphere || !neighbour.alive) continue;
    if (dist(neighbour.pos, sphere.pos) <= getEngineerNetworkRange(s)) {
      s.lightnings.push({ from: { ...sphere.pos }, to: { ...neighbour.pos }, life: 0.12 });
    }
  }
}

export function getCritChance(s: GameState, sphere?: SphereEntity): number {
  let c = CRIT_BASE;
  c += (s.player.abilities.crit || 0) * 0.1;
  c += (s.shopUpgrades.crit || 0) * 0.05;
  c += getArtifactCritChanceBonus(s);
  if (sphere?.type === 'sniper' && sphereLevel(s, 'sniper') >= 3) c += 0.15;
  return Math.min(0.75, c);
}

export function getDodgeChance(s: GameState): number {
  return Math.min(0.75, (s.player.abilities.dodge || 0) * 0.1 + getArtifactDodgeChanceBonus(s));
}

export function getVampirePercent(s: GameState): number {
  const lvl = s.player.abilities.vampire || 0;
  return lvl * 0.03 + getArtifactVampireBonus(s);
}

export function getCooldownMult(s: GameState): number {
  return getArtifactCooldownMultiplier(s) * Math.max(0.55, 1 - (s.player.abilities.cooldown || 0) * 0.04);
}

export function getDamageTakenMult(s: GameState): number {
  let m = 1;
  m *= getCharacterDamageTakenMultiplier(s);
  m *= Math.max(0.45, 1 - (s.player.abilities.armor || 0) * 0.04);
  m *= getFormationDamageTakenMultiplier(s);
  return m;
}

// ===== Damage application =====

export function dealDamageToEnemy(s: GameState, enemy: EnemyEntity, dmg: number, fromSphere?: SphereEntity, allowSphereProc = true): void {
  if (enemy.hp <= 0) return;
  if (fromSphere) registerHunterHit(s, enemy, fromSphere);

  if (s.player.timestopTimer > 0 && getAbilityBranchId(s, 'timestop', 7) === 'timestop_outside_time') {
    enemy.freezeTimer = Math.max(enemy.freezeTimer, 0.35);
  }

  let actual = dmg;
  if (enemy.isElite && enemy.eliteVariant === 'mirror_warden' && fromSphere) actual *= 0.88;
  if (fromSphere) {
    actual *= getCharacterDamageMultiplier(s, fromSphere);
    actual *= getHunterMarkMultiplier(s, enemy);
    const mods = sphereModifiers(s, fromSphere.type, fromSphere);
    const hpRatio = enemy.hp / Math.max(1, enemy.maxHp);

    // Final modifier behaviors are intentionally local to the authoritative
    // damage path so projectile, Aura and Chain hits share the same rules.
    if (mods.execute > 0 && hpRatio <= 0.30) actual *= 1 + 0.18 * mods.execute;
    if (mods.mark > 0 && getCharacterId(s) !== 'hunter' && s.player.hunterMarkTarget === enemy && s.player.hunterMarkTimer > 0) {
      actual *= 1 + 0.15 * mods.mark;
      s.player.hunterMarkTimer = 0;
    }
    if (mods.corrupt > 0) {
      enemy.corruptStacks = Math.min(5, (enemy.corruptStacks ?? 0) + 1);
      actual *= 1 + 0.04 * enemy.corruptStacks;
    }
    if (
      enemy.freezeVulnerabilityTimer && enemy.freezeVulnerabilityTimer > 0 &&
      enemy.freezeTimer > 0 &&
      fromSphere?.type === enemy.freezeVulnerabilitySource
    ) {
      actual *= 1.22;
    }
  }
  if (fromSphere && s.player.timestopTimer > 0 && getAbilityBranchId(s, 'timestop', 4) === 'timestop_echo_phase') actual *= 1.18;
  const contextualCritChance = getContextualCritChance(getCritChance(s, fromSphere), {
    hunterMarked: Boolean(
      fromSphere
      && getCharacterId(s) === 'hunter'
      && s.player.hunterMarkTarget === enemy
      && s.player.hunterMarkTimer > 0
      && s.player.characterMasteryLevel >= 3
    ),
    architectTriangle: Boolean(
      fromSphere
      && getCharacterId(s) === 'architect'
      && getCharacterFormation(s).type === 'triangle'
    ),
  });
  let isCrit = false;
  // crit
  if (fromSphere && nextRandom(s) < contextualCritChance) { actual *= CRIT_MULTIPLIER_BASE; isCrit = true; }
  if (fromSphere) {
    const squareNetwork = getNetworkFrame(s);
    const sphereIndex = s.spheres.indexOf(fromSphere);
    const squareProfile = getSphereNetworkProfile(squareNetwork, sphereIndex);
    const squareBonus = getFormationBonusMultiplier(squareNetwork, 'square', sphereIndex);
    if (squareProfile.square && squareBonus > 0) {
      // Each active Geometry keeps an independent cadence so overlapping
      // Triangle/Square formations cannot consume one another's counters.
      const counts = fromSphere.formationHitCounts || (fromSphere.formationHitCounts = {});
      counts.square = (counts.square || 0) + 1;
      const cadence = squareBonus >= 1 ? 4 : 8;
      if (counts.square % cadence === 0) {
        fromSphere.resonancePulseTimer = 0.55;
        s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
      }
    }
  }

  // predator claw: every 5th hit
  if (fromSphere && allowSphereProc && getArtifactSetBehavior(s).singularityPath && s.spheres.filter((x) => x.alive).every((x) => x.type === fromSphere.type)) {
    if (isCrit && enemy.hp > 0) {
      const nearby = s.enemies.filter((other) => other !== enemy && other.hp > 0 && dist(other.pos, enemy.pos) <= 72);
      for (const other of nearby.slice(0, 3)) dealDamageToEnemy(s, other, actual * 0.20, fromSphere, false);
    }
  }

  if (fromSphere && s.player.artifacts.includes('predator_claw')) {
    fromSphere.killsContribution++;
    if (fromSphere.killsContribution % 5 === 0) { actual *= 2; isCrit = true; }
  }

  if (fromSphere) {
    const mods = sphereModifiers(s, fromSphere.type, fromSphere);
    if (allowSphereProc && mods.shatter > 0 && enemy.freezeTimer > 0) {
      const burst = actual * (0.20 + 0.08 * mods.shatter);
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) <= 48) {
          dealDamageToEnemy(s, nearby, burst, fromSphere, false);
        }
      }
    }
    if (mods.impact > 0) {
      const dx = enemy.pos.x - fromSphere.pos.x;
      const dy = enemy.pos.y - fromSphere.pos.y;
      const d = Math.hypot(dx, dy) || 1;
      const push = 18 * mods.impact * (s.region ? getRegionRepulsionMultiplier(s) : 1);
      enemy.pos.x += dx / d * push;
      enemy.pos.y += dy / d * push;
    }
    if (mods.gravitic > 0) {
      for (const other of s.enemies) {
        if (other === enemy || other.hp <= 0) continue;
        const dx = enemy.pos.x - other.pos.x;
        const dy = enemy.pos.y - other.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        if (d <= 72) {
          const pull = Math.min(10, 4 * mods.gravitic);
          other.pos.x += dx / d * pull;
          other.pos.y += dy / d * pull;
        }
      }
    }
    if (mods.anchor > 0) {
      enemy.slowTimer = Math.max(enemy.slowTimer, 0.8 + 0.25 * mods.anchor);
      enemy.slowFactor = Math.min(enemy.slowFactor, Math.max(0.35, 0.65 - 0.05 * mods.anchor));
    }
    if (mods.vampiric > 0) {
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + actual * mods.healOnHit);
    }
    if (mods.resonantCharge > 0 && s.player.sphereBranches?.[fromSphere.type] !== 'pulse_resonator') {
      chargeResonance(
        s,
        'sphereHit',
        dealDamageToEnemy,
        Math.max(1, mods.resonantCharge / 2),
      );
    }
    if (allowSphereProc && mods.echoChance > 0 && nextRandom(s) < mods.echoChance) {
      dealDamageToEnemy(s, enemy, actual * 0.22, fromSphere, false);
    }
    if (allowSphereProc && mods.afterimage > 0 && nextRandom(s) < Math.min(0.40, 0.16 * mods.afterimage)) {
      const angle = Math.atan2(enemy.pos.y - fromSphere.pos.y, enemy.pos.x - fromSphere.pos.x);
      s.sphereProjectiles.push({
        pos: { ...enemy.pos },
        vel: { x: Math.cos(angle) * 250, y: Math.sin(angle) * 250 },
        damage: actual * Math.min(0.32, 0.16 + 0.04 * mods.afterimage),
        radius: 4,
        alive: true,
        color: '#d8c5ff',
        pierce: 0,
        hitEnemies: new Set([enemy]),
        effect: 'none',
        ricochet: 0,
        life: 0.42,
        sourceSphere: fromSphere,
        procOnHit: false,
      });
    }
    if (allowSphereProc && mods.staticChance > 0 && nextRandom(s) < mods.staticChance) {
      let next: EnemyEntity | undefined;
      let nextDistance = Infinity;
      for (const candidate of s.enemies) {
        if (candidate === enemy || candidate.hp <= 0) continue;
        const distance = dist(candidate.pos, enemy.pos);
        if (distance <= 110 && distance < nextDistance) {
          next = candidate;
          nextDistance = distance;
        }
      }
      if (next) {
        s.lightnings.push({ from: { ...enemy.pos }, to: { ...next.pos }, life: 0.22, sourceSphere: fromSphere });
        dealDamageToEnemy(s, next, actual * 0.35, fromSphere, false);
      }
    }
    if (mods.mark > 0 && getCharacterId(s) !== 'hunter') {
      s.player.hunterMarkTarget = enemy;
      s.player.hunterMarkTimer = Math.max(s.player.hunterMarkTimer, 2.5);
    }
  }
  // Basic Shotgun II: +20% damage at close range.
  if (fromSphere?.type === 'shotgun' && sphereLevel(s, 'shotgun') >= 2 && dist(enemy.pos, fromSphere.pos) < 110) {
    actual *= 1.20;
  }
  if (fromSphere?.type === 'void') {
    const voidLevel = sphereLevel(s, 'void');
    const voidBranch = s.player.sphereBranches?.void;
    const voidFinal = getSphereFinalIndex(s, 'void');
    const hpRatio = enemy.hp / Math.max(1, enemy.maxHp);
    if (voidLevel >= 1 && hpRatio <= 0.50) actual *= 1.20;
    const network = getNetworkFrame(s);
    const profile = getSphereNetworkProfile(network, s.spheres.indexOf(fromSphere));
    if (hpRatio <= 0.20) actual *= 2.25;
    if (hpRatio <= 0.35 && s.player.artifacts.includes('void_mark')) actual *= 1.10;
    if (hpRatio <= 0.35 && s.player.artifacts.includes('void_lantern')) actual *= 1.20;
    if (voidBranch === 'void_hunger') {
      const branchPower = voidFinal === null ? preFinalBranchPower(s, 'void') : 1;
      actual *= 1 + Math.min(0.55, (1 - hpRatio) * (voidFinal === 2 ? 0.72 : 0.42) * branchPower);
      if (voidFinal === 2 && hpRatio <= 0.30) actual *= 1.12;
    }
    if (voidBranch === 'void_reaper' && hpRatio <= 0.25) actual *= voidFinal === 1 ? 1.25 : 1.12;
    const baseExecuteChance = voidLevel >= 2 ? 0.10 : 0;
    if (voidBranch === 'void_execution') {
      const threshold = voidFinal === 2
        ? 0.30
        : voidFinal === 0 || (voidFinal === null && voidLevel >= 6)
          ? 0.28
          : voidFinal === null && voidLevel >= 5
            ? 0.27
            : voidLevel >= 3
              ? 0.25
              : 0.20;
      let executeChance = baseExecuteChance;
      executeChance += voidFinal === 2 ? 0.08 : voidFinal === 1 ? 0.04 : 0;
      if (s.player.artifacts.includes('void_star')) executeChance += 0.08;
      if (hpRatio <= threshold && nextRandom(s) < Math.min(0.45, executeChance)) {
        if (!enemy.isBoss) actual = Math.max(actual, enemy.hp + 1);
        else actual *= 2.5;
      }
    } else if (baseExecuteChance > 0 && hpRatio <= 0.20 && nextRandom(s) < baseExecuteChance) {
      if (!enemy.isBoss) actual = Math.max(actual, enemy.hp + 1);
      else actual *= 1.35;
    }
    const sphereIndex = s.spheres.indexOf(fromSphere);
    const lineBonus = getFormationBonusMultiplier(getNetworkFrame(s), 'line', sphereIndex);
    const fractalBonus = getFormationBonusMultiplier(getNetworkFrame(s), 'fractal', sphereIndex);
    if (lineBonus > 0) actual *= 1 + 0.10 * lineBonus;
    if (fractalBonus > 0) actual *= 1 + 0.15 * fractalBonus;
  }

  if (fromSphere && allowSphereProc) {
    const branch = s.player.sphereBranches?.[fromSphere.type];
    const finalId = (s.player.evolutions || []).find((x: string) => x.startsWith('sphere:' + fromSphere.type + ':7:'));
    const finalIndex = finalId ? Number(finalId.split(':').pop()) : null;

    if (branch === 'standard_resonator') {
      const hits = ((fromSphere as any).evolutionHits || 0) + 1;
      (fromSphere as any).evolutionHits = hits;
      if (hits % 3 === 0) {
        const branchPower = finalIndex === null ? preFinalBranchPower(s, 'standard') : 1;
        const shockDamage = (finalIndex === 0 ? actual * 0.65 : finalIndex === 1 ? actual * 0.45 : actual * 0.35) * branchPower;
        const shockRadius = finalIndex === 0 ? 115 : finalIndex === 1 ? 100 : 90;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < shockRadius) {
            dealDamageToEnemy(s, nearby, shockDamage, fromSphere, false);
            if (finalIndex === 2) {
              nearby.slowTimer = Math.max(nearby.slowTimer, 0.8);
              nearby.slowFactor = Math.min(nearby.slowFactor, 0.7);
            }
          }
        }
        if (finalIndex === 1) {
          for (const nearby of s.enemies) {
            if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < 100) {
              const dx = nearby.pos.x - enemy.pos.x, dy = nearby.pos.y - enemy.pos.y;
              const d = Math.hypot(dx, dy) || 1;
              nearby.pos.x += dx / d * 18;
              nearby.pos.y += dy / d * 18;
            }
          }
        }
        s.screenShake = Math.min(0.14, s.screenShake + (finalIndex === 0 ? 0.04 : 0.025));
      }
    } else if (branch === 'standard_singularity') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'standard') : 1;
      const slowDuration = (finalIndex === 0 ? 1.2 : finalIndex === 1 ? 1.8 : 0.9) * branchPower;
      enemy.slowTimer = Math.max(enemy.slowTimer, slowDuration);
      enemy.slowFactor = Math.min(enemy.slowFactor, Math.max(0.45, (finalIndex === 0 ? 0.5 : finalIndex === 1 ? 0.58 : 0.68) - (branchPower - 1) * 0.08));
      if (finalIndex === null || finalIndex === 0 || finalIndex === 2) {
        const branchPower = finalIndex === null ? preFinalBranchPower(s, 'standard') : 1;
        const pull = (finalIndex === 0 ? 24 : finalIndex === 2 ? 40 : 20) * branchPower;
        const pullRadius = (finalIndex === 0 ? 75 : finalIndex === 2 ? 100 : 65) * branchPower;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < pullRadius) {
            const dx = enemy.pos.x - nearby.pos.x, dy = enemy.pos.y - nearby.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            nearby.pos.x += dx / d * pull;
            nearby.pos.y += dy / d * pull;
          }
        }
      }
      if (finalIndex === 2 && enemy.hp < enemy.maxHp * 0.5) actual *= 1.15;
    } else if (branch === 'standard_swarm') {
      // Level VII Swarm finals are three different shard patterns:
      // guided hunter shard, crossfire pair, or fan-shaped shard web.
      const preFinalLevel = sphereLevel(s, 'standard');
      const count = finalIndex === null
        ? (preFinalLevel >= 6 ? 2 : 1)
        : finalIndex === 0 ? 1 : finalIndex === 1 ? 2 : 3;
      const chance = finalIndex === null ? 1 : finalIndex === 0 ? 0.75 : finalIndex === 1 ? 0.70 : 1;
      if (nextRandom(s) < chance) {
        const shardDamageMultiplier = finalIndex === null
          ? preFinalLevel >= 6 ? 0.65 : preFinalLevel >= 5 ? 0.58 : 0.50
          : finalIndex === 0 ? 0.72 : finalIndex === 1 ? 0.56 : 0.44;
        const otherTarget = s.enemies
          .filter((candidate) => candidate !== enemy && candidate.hp > 0)
          .sort((a, b) => dist(a.pos, enemy.pos) - dist(b.pos, enemy.pos))[0];
        const baseAngle = otherTarget
          ? Math.atan2(otherTarget.pos.y - enemy.pos.y, otherTarget.pos.x - enemy.pos.x)
          : nextRandom(s) * Math.PI * 2;
        const angles = finalIndex === 0
          ? [baseAngle]
          : finalIndex === 1
            ? [baseAngle - 0.55, baseAngle + 0.55]
            : [baseAngle - 0.7, baseAngle, baseAngle + 0.7];
        for (let i = 0; i < count; i++) {
          const a = angles[i] ?? (nextRandom(s) * Math.PI * 2);
          s.sphereProjectiles.push({
            pos: { ...enemy.pos }, vel: { x: Math.cos(a) * 320, y: Math.sin(a) * 320 },
            damage: actual * shardDamageMultiplier, radius: 4, alive: true, color: '#d4943d',
            pierce: finalIndex === 2 ? 1 : 0,
            hitEnemies: new Set([enemy]), effect: 'none', ricochet: 0, life: 0.55,
            sourceSphere: fromSphere, procOnHit: false,
          });
        }
      }
    } else if (branch === 'sniper_oracle' && s.player.hunterMarkTarget === enemy && finalIndex === null) {
      actual *= preFinalBranchPower(s, 'sniper');
    } else if (branch === 'sniper_oracle' && s.player.hunterMarkTarget === enemy && isCrit) {
      actual *= finalIndex === 0 ? 1.5 : finalIndex === 1 ? 1.3 : 1.22;
      if (finalIndex === 2) {
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < 65) {
            dealDamageToEnemy(s, nearby, actual * 0.25, fromSphere, false);
          }
        }
      }
    } else if (branch === 'sniper_assassin') {
      const hpRatio = enemy.hp / Math.max(1, enemy.maxHp);
      if (finalIndex === null) {
        if (hpRatio < 0.35) actual *= preFinalBranchPower(s, 'sniper');
      } else if (finalIndex === 0) {
        if (hpRatio < 0.45) actual *= 1.35;
        if (hpRatio < 0.45) {
          s.player.hunterMarkTarget = enemy;
          s.player.hunterMarkTimer = Math.max(s.player.hunterMarkTimer, 0.9);
        }
      } else if (finalIndex === 1) {
        if (hpRatio < 0.20) actual *= 2.0;
        if (hpRatio < 0.20) {
          s.player.hunterMarkTarget = enemy;
          s.player.hunterMarkTimer = Math.max(s.player.hunterMarkTimer, 1.0);
        }
      } else {
        if (hpRatio < 0.35) {
          actual *= 1.5;
          s.player.hp = Math.min(s.player.maxHp, s.player.hp + actual * 0.012);
        }
      }
    } else if (branch === 'sniper_beacon') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'sniper') : 1;
      s.player.hunterMarkTarget = enemy;
      s.player.hunterMarkTimer = Math.max(s.player.hunterMarkTimer, (finalIndex === 1 ? 5 : 3) * branchPower);
      enemy.slowTimer = Math.max(enemy.slowTimer, (finalIndex === 0 ? 0.9 : finalIndex === 1 ? 1.2 : 0.7) * branchPower);
      enemy.slowFactor = Math.min(enemy.slowFactor, Math.max(0.42, (finalIndex === 0 ? 0.65 : finalIndex === 1 ? 0.7 : 0.6) - (branchPower - 1) * 0.06));
      const radius = (finalIndex === 0 ? 90 : finalIndex === 1 ? 140 : 110) * branchPower;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          nearby.slowTimer = Math.max(nearby.slowTimer, finalIndex === 1 ? 1 : 0.5);
          nearby.slowFactor = Math.min(nearby.slowFactor, finalIndex === 2 ? 0.65 : 0.8);
        }
      }
    } else if (branch === 'shotgun_burst') {
      const shotgunBranchLevel = sphereLevel(s, 'shotgun');
      const closeRange = finalIndex === null
        ? shotgunBranchLevel >= 6 ? 150 : shotgunBranchLevel >= 5 ? 135 : 120
        : finalIndex === 1 ? 180 : 150;
      if (dist(enemy.pos, fromSphere.pos) < closeRange) {
        actual *= finalIndex === null
          ? preFinalBranchPower(s, 'shotgun')
          : finalIndex === 0 ? 1.3 : finalIndex === 1 ? 1.5 : 1.22;
      }
      if (finalIndex === 2 && dist(enemy.pos, fromSphere.pos) < 90) enemy.slowTimer = Math.max(enemy.slowTimer, 0.4);
    } else if (branch === 'shotgun_cataclysm') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'shotgun') : 1;
      const radius = (finalIndex === 0 ? 60 : finalIndex === 1 ? 85 : 55) * branchPower;
      const splash = (finalIndex === 0 ? 0.45 : finalIndex === 1 ? 0.65 : 0.35) * branchPower;
      for (let i = 0; i < 14; i++) {
        const a = nextRandom(s) * Math.PI * 2;
        s.particles.push({ pos: { ...enemy.pos }, vel: { x: Math.cos(a) * 90, y: Math.sin(a) * 90 }, life: 0.35, maxLife: 0.35, color: '#c4453d', size: 3 });
      }
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          dealDamageToEnemy(s, nearby, actual * splash, fromSphere, false);
        }
      }
      if (finalIndex === 2) {
        enemy.slowTimer = Math.max(enemy.slowTimer, 0.8);
        enemy.slowFactor = Math.min(enemy.slowFactor, 0.65);
      }
    } else if (branch === 'shotgun_hail') {
      const branchLevel = sphereLevel(s, 'shotgun');
      const chance = finalIndex === null
        ? (branchLevel >= 6 ? 0.60 : branchLevel >= 5 ? 0.45 : 0.32)
        : finalIndex === 0 ? 0.25 : finalIndex === 1 ? 0.4 : 0.32;
      if (finalIndex === 2) actual *= 1.08;
      if (nextRandom(s) < chance) {
        const radius = finalIndex === 1
          ? 65
          : finalIndex === null
            ? branchLevel >= 6 ? 60 : branchLevel >= 5 ? 52 : 45
            : 45;
        const shardCount = finalIndex === 1 ? 8 : finalIndex === null && branchLevel >= 6 ? 7 : 6;
        const guidedTargets = finalIndex === 2
          ? s.enemies
            .filter((candidate) => candidate !== enemy && candidate.hp > 0)
            .sort((a, b) => dist(a.pos, enemy.pos) - dist(b.pos, enemy.pos))
            .slice(0, shardCount)
          : [];
        for (let i = 0; i < shardCount; i++) {
          const guided = guidedTargets[i];
          const angle = guided
            ? Math.atan2(guided.pos.y - enemy.pos.y, guided.pos.x - enemy.pos.x)
            : (i / shardCount) * Math.PI * 2 + nextRandom(s) * 0.18;
          s.sphereProjectiles.push({
            pos: { ...enemy.pos },
            vel: { x: Math.cos(angle) * 300, y: Math.sin(angle) * 300 },
            damage: actual * (finalIndex === 1 ? 0.22 : finalIndex === 2 ? 0.19 : 0.16),
            radius: 5, alive: true, color: '#f0b35a', pierce: finalIndex === 2 ? 1 : 0,
            hitEnemies: new Set(), effect: 'none', ricochet: 0, life: 0.45, sourceSphere: fromSphere, procOnHit: false,
          });
        }
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
            dealDamageToEnemy(s, nearby, actual * (finalIndex === 1 ? 0.18 : 0.12), fromSphere, false);
          }
        }
        for (let i = 0; i < shardCount; i++) {
          const angle = (i / shardCount) * Math.PI * 2;
          s.particles.push({ pos: { ...enemy.pos }, vel: { x: Math.cos(angle) * 110, y: Math.sin(angle) * 110 }, life: 0.35, maxLife: 0.35, color: '#f0b35a', size: 3 });
        }
      }
    } else if (branch === 'chain_web') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'chain') : 1;
      enemy.slowTimer = Math.max(enemy.slowTimer, (finalIndex === 0 ? 0.7 : finalIndex === 1 ? 1.4 : 0.5) * branchPower);
      enemy.slowFactor = Math.min(enemy.slowFactor, Math.max(0.42, (finalIndex === 0 ? 0.7 : finalIndex === 1 ? 0.55 : 0.72) - (branchPower - 1) * 0.08));
      if (finalIndex === 2) actual *= 1.25;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < 90) {
          s.lightnings.push({ from: { ...enemy.pos }, to: { ...nearby.pos }, life: 0.12 });
        }
      }
    } else if (branch === 'chain_storm') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'chain') : 1;
      const radius = (finalIndex === 0 ? 70 : finalIndex === 1 ? 100 : 55) * branchPower;
      const splash = (finalIndex === 0 ? 0.25 : finalIndex === 1 ? 0.4 : 0.2) * branchPower;
      for (const nearby of s.enemies) {
        if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) < radius) {
          dealDamageToEnemy(s, nearby, actual * splash, fromSphere, false);
          s.lightnings.push({ from: { ...enemy.pos }, to: { ...nearby.pos }, life: 0.2 });
        }
      }
      if (finalIndex === 1) {
        const linkedCharge = ((fromSphere as any).chainStormCharge || 0) + 1;
        (fromSphere as any).chainStormCharge = linkedCharge;
        if (linkedCharge % 2 === 0) fromSphere.resonancePulseTimer = Math.max(fromSphere.resonancePulseTimer, 0.45);
      }
      if (finalIndex === 2) {
        const cycle = ((fromSphere as any).chainStormCycle || 0) + 1;
        (fromSphere as any).chainStormCycle = cycle;
        if (cycle % 3 === 0) {
          const extra = s.enemies
            .filter((candidate) => candidate !== enemy && candidate.hp > 0)
            .sort((a, b) => dist(a.pos, enemy.pos) - dist(b.pos, enemy.pos))[0];
          if (extra) {
            s.lightnings.push({ from: { ...enemy.pos }, to: { ...extra.pos }, life: 0.24, sourceSphere: fromSphere });
            dealDamageToEnemy(s, extra, actual * 0.38, fromSphere, false);
          }
        }
      }
    } else if (branch === 'chain_leech') {
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'chain') : 1;
      const heal = (finalIndex === 0 ? 0.025 : finalIndex === 1 ? 0.045 : 0.018) * branchPower;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + actual * heal);
      if (finalIndex === 2 && enemy.hp < enemy.maxHp * 0.4) actual *= 1.2;
    } else if (branch === 'aura_sanctum') {
      enemy.slowTimer = Math.max(enemy.slowTimer, finalIndex === 0 ? 1.4 : finalIndex === 1 ? 2 : 0.9);
      enemy.slowFactor = Math.min(enemy.slowFactor, finalIndex === 0 ? 0.5 : finalIndex === 1 ? 0.42 : 0.62);
      if (finalIndex === 2) actual *= 1.12;
    } else if (branch === 'aura_gravity') {
      if (finalIndex === 0 || finalIndex === 1 || finalIndex === 2) {
        const pull = finalIndex === 0 ? 55 : 80;
        const pullRadius = finalIndex === 0 ? 150 : 190;
        for (const nearby of s.enemies) {
          if (nearby.hp > 0 && dist(nearby.pos, fromSphere.pos) < pullRadius) {
            const dx = fromSphere.pos.x - nearby.pos.x, dy = fromSphere.pos.y - nearby.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            nearby.pos.x += dx / d * pull;
            nearby.pos.y += dy / d * pull;
            if (finalIndex === 1) {
              nearby.slowTimer = Math.max(nearby.slowTimer, 0.8);
              nearby.slowFactor = Math.min(nearby.slowFactor, 0.72);
            }
          }
        }
      }
      if (finalIndex === 2) actual *= 1.18;
    } else if (branch === 'gravity_tide') {
      const hitCount = ((fromSphere as any).gravityTideHits || 0) + 1;
      (fromSphere as any).gravityTideHits = hitCount;
      const mode = finalIndex === null ? 0 : finalIndex;
      const radius = mode === 1 ? 170 : 150;
      for (const nearby of s.enemies) {
        if (nearby === enemy || nearby.hp <= 0) continue;
        const d = dist(nearby.pos, fromSphere.pos);
        if (d >= radius) continue;
        const dx = nearby.pos.x - fromSphere.pos.x;
        const dy = nearby.pos.y - fromSphere.pos.y;
        const len = Math.hypot(dx,dy) || 1;
        const direction = mode === 0 ? 1 : mode === 1 ? -1 : hitCount % 2 === 0 ? 1 : -1;
        nearby.pos.x += (dx / len) * 52 * direction;
        nearby.pos.y += (dy / len) * 52 * direction;
      }
    } else if (branch === 'aura_overgrowth') {
      const bonus = finalIndex === 0 ? 0.18 : finalIndex === 1 ? 0.3 : 0.1;
      for (const ally of s.spheres) {
        if (ally !== fromSphere && ally.alive && dist(ally.pos, fromSphere.pos) < (finalIndex === 1 ? 180 : 140)) {
          ally.attackTimer = Math.max(0, ally.attackTimer - bonus);
        }
      }
      if (finalIndex === 2) actual *= 1.12;
    }
  }

  // buff from chest
  if (s.player.buffTimer > 0) actual *= 1.3;
  enemy.hp -= actual;
  if (enemy.isElite && enemy.eliteVariant === 'mirror_warden' && fromSphere && actual > 0) damagePlayer(s, actual * 0.12);
  if (fromSphere) {
    chargeResonance(s, 'sphereHit', dealDamageToEnemy);
    if (allowSphereProc && actual > 0) {
      const character = getCharacterId(s);
      const mastery = s.player.characterMasteryLevel || 1;
      if (character === 'spherist') {
        const sphereCount = s.spheres.filter((item) => item.alive).length;
        if (mastery >= 9 && sphereCount >= 8) {
          s.player.spheristChorusHits++;
          const threshold = mastery >= 10 ? 8 : 10;
          if (s.player.spheristChorusHits >= threshold) {
            s.player.spheristChorusHits = 0;
            const companions = s.spheres.filter((item) => item.alive && item !== fromSphere).sort((a, b) => b.attackTimer - a.attackTimer).slice(0, mastery >= 10 ? 2 : 1);
            for (const companion of companions) {
              companion.attackTimer = 0;
              companion.resonancePulseTimer = Math.max(companion.resonancePulseTimer, 0.45);
            }
            s.screenShake = Math.min(0.16, s.screenShake + 0.025);
            s.flashText = { text: mastery >= 10 ? 'CHORUS SYNC' : 'CHORUS PULSE', life: 0.7, color: '#55dfff' };
          }
        } else {
          s.player.spheristChorusHits = 0;
        }
      }
      if (character === 'berserker' && mastery >= 7) {
        const hpRatio = s.player.hp / Math.max(1, s.player.maxHp);
        if (hpRatio <= 0.30) {
          s.player.berserkerRedlineHits++;
          const threshold = mastery >= 10 ? 4 : 5;
          if (s.player.berserkerRedlineHits >= threshold) {
            s.player.berserkerRedlineHits = 0;
            const pulseDamage = actual * (mastery >= 10 ? 0.40 : mastery >= 8 ? 0.35 : 0.30);
            for (const target of s.enemies) if (target.hp > 0 && dist(target.pos, s.player.pos) <= 105) dealDamageToEnemy(s, target, pulseDamage, undefined, false);
            for (let i = 0; i < 16; i++) { const a = nextRandom(s) * Math.PI * 2; s.particles.push({ pos: { ...s.player.pos }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.35, maxLife: 0.35, color: '#ff625d', size: 3 }); }
            s.screenShake = Math.min(0.20, s.screenShake + 0.035);
            s.flashText = { text: 'REDLINE', life: 0.65, color: '#ff625d' };
          }
        } else s.player.berserkerRedlineHits = 0;
      }
    }
  }
  enemy.hitFlash = 0.15;

  // Juicier impact: a short, directional burst makes every sphere hit readable.
  // formationHitCount remains local to preserve formation cadence mechanics.
  if (fromSphere) {
    const sphereIndex = s.spheres.indexOf(fromSphere);
    const network = getNetworkFrame(s);
    const profile = getSphereNetworkProfile(network, sphereIndex);
    const triangleBonus = getFormationBonusMultiplier(network, 'triangle', sphereIndex);
    if (profile.triangle && triangleBonus > 0) {
      const counts = fromSphere.formationHitCounts || (fromSphere.formationHitCounts = {});
      counts.triangle = (counts.triangle || 0) + 1;
      const setBehavior = getArtifactSetBehavior(s);
      const trianglePulseEvery = setBehavior.resonanceGrid ? 2 : 3;
      if (counts.triangle % trianglePulseEvery === 0) {
        fromSphere.resonancePulseTimer = 0.45;
        const pulseDamage = actual * 0.35 * triangleBonus;
        const pulseRadius = 88;
        for (const nearby of s.enemies) {
          if (nearby !== enemy && nearby.hp > 0 && dist(nearby.pos, enemy.pos) <= pulseRadius) {
            dealDamageToEnemy(s, nearby, pulseDamage, fromSphere, false);
          }
        }
        s.lightnings.push({ from: { ...fromSphere.pos }, to: { ...enemy.pos }, life: 0.24 });
        s.screenShake = Math.min(0.16, s.screenShake + 0.025);
      }
    }

    // Echo Architecture completion creates a real relay event between linked Spheres.
    const setBehavior = getArtifactSetBehavior(s);
    if (setBehavior.echoArchitecture && fromSphere.formationHitCount > 0 && fromSphere.formationHitCount % 5 === 0) {
      const relayNetwork = getNetworkFrame(s);
      const linked = getLinkedNodeIndexes(relayNetwork, s.spheres.indexOf(fromSphere));
      const targetIndex = linked.find((index) => s.spheres[index]?.alive);
      if (targetIndex !== undefined) {
        const relay = s.spheres[targetIndex];
        relay.formationHitCount += 2;
        relay.resonancePulseTimer = Math.max(relay.resonancePulseTimer, 0.35);
        s.lightnings.push({ from: { ...fromSphere.pos }, to: { ...relay.pos }, life: 0.20 });
      }
    }
  }

  const impactCount = !fromSphere ? 1 : allowSphereProc ? (enemy.isBoss ? 10 : isCrit ? 9 : enemy.isElite ? 7 : 4) : 1;
  for (let i = 0; i < impactCount; i++) {
    const angle = nextRandom(s) * Math.PI * 2;
    const speed = rand(s,isCrit ? 120 : 80, isCrit ? 260 : 180);
    s.particles.push({
      pos: { x: enemy.pos.x, y: enemy.pos.y },
      vel: { x: Math.cos(angle) * speed, y: Math.sin(angle) * speed },
      life: rand(s,0.16, isCrit ? 0.42 : 0.3),
      maxLife: 0.42,
      color: isCrit ? '#c4453d' : enemy.color,
      size: rand(s,isCrit ? 2.5 : 1.5, isCrit ? 4.5 : 3.2),
    });
  }
  if (isCrit) {
    s.screenShake = Math.min(0.24, s.screenShake + 0.06);
  }

  // Primary/authoritative hits keep full feedback; recursive secondary damage stays real
  // but does not create an audio, particle and number storm.
  if (fromSphere && allowSphereProc) {
    const branchElement = getSphereElementForBranch(s.player.sphereBranches?.[fromSphere.type]);
    const mods = sphereModifiers(s, fromSphere.type, fromSphere);
    const element = branchElement ?? (mods.fire > 0 ? 'fire' : mods.freeze > 0 ? 'freeze' : mods.poison > 0 ? 'poison' : undefined);
    if (s.damageNumbers.length >= 72) s.damageNumbers.splice(0, s.damageNumbers.length - 71);
    s.damageNumbers.push({
      pos: { x: enemy.pos.x + rand(s,-8, 8), y: enemy.pos.y - enemy.radius - 5 },
      value: Math.round(actual), life: 0.8, maxLife: 0.8, crit: isCrit,
      vel: { x: rand(s,-30, 30), y: -60 },
      sourceSphereType: fromSphere?.type,
      element,
    });
    if (isCrit) playSound('crit'); else playSound('hit');
  }
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
    if (fromSphere?.type === 'void' && s.player.sphereBranches?.void === 'void_reaper') {
      const finalIndex = getSphereFinalIndex(s, 'void');
      const branchPower = finalIndex === null ? preFinalBranchPower(s, 'void') : 1;
      s.player.hp = Math.min(s.player.maxHp, s.player.hp + (finalIndex === 2 ? 6 : 3) * branchPower);
      const shardCount = finalIndex === 2 ? 3 : finalIndex === null && sphereLevel(s, 'void') >= 6 ? 3 : 2;
      for (let shard = 0; shard < shardCount; shard++) {
        const angle = shard * (Math.PI * 2 / shardCount);
        s.sphereProjectiles.push({
          pos: { ...enemy.pos },
          vel: { x: Math.cos(angle) * 320, y: Math.sin(angle) * 320 },
          damage: Math.max(4, actual * (finalIndex === 1 ? 0.24 : 0.18) * branchPower),
          radius: 4, alive: true, color: '#8f63ff', pierce: 0,
          hitEnemies: new Set(), effect: 'none', ricochet: 0, life: 0.65,
          sourceSphere: fromSphere, procOnHit: false,
        });
      }
    }
    onEnemyDeath(s, enemy, fromSphere);
  }
}


export function onEnemyDeath(s: GameState, enemy: EnemyEntity, fromSphere?: SphereEntity): void {
  if (fromSphere) {
    const healOnKill = sphereModifiers(s, fromSphere.type, fromSphere).healOnKill;
    if (healOnKill > 0) s.player.hp = Math.min(s.player.maxHp, s.player.hp + s.player.maxHp * healOnKill);
  } else if (s.player.sphereMods.drain > 0) {
    s.player.hp = Math.min(s.player.maxHp, s.player.hp + s.player.maxHp * 0.03 * s.player.sphereMods.drain);
  }
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
    if (getCharacterId(s) === 'hunter' && s.player.characterMasteryLevel >= 5 && s.player.hunterMarkTarget === enemy) {
      s.player.hunterTrophyTimer = 4;
    }
    playSound('elite');
  }
  if (getCharacterId(s) === 'hunter' && s.player.hunterMarkTarget === enemy) {
    s.player.hunterMarkTarget = null;
    s.player.hunterMarkTimer = 0;
    s.player.hunterHitCount = 0;
  }
  if (getCharacterId(s) === 'hunter' && s.player.hunterHuntTarget === enemy) {
    s.player.hunterHuntTarget = null;
    s.player.hunterHuntTimer = 0;
  }
  if (enemy.isElite && enemy.eliteVariant === 'splitter_prime') {
    for (let k = 0; k < 2; k++) {
      const angle = nextRandom(s) * Math.PI * 2;
      const childHp = Math.max(8, enemy.maxHp * 0.18);
      s.enemies.push({
        pos: { x: enemy.pos.x + Math.cos(angle) * 18, y: enemy.pos.y + Math.sin(angle) * 18 },
        hp: childHp,
        maxHp: childHp,
        speed: enemy.speed * 1.15,
        radius: 10,
        damage: enemy.damage * 0.45,
        type: 'fast',
        color: '#8a5a8a',
        shape: 'triangle',
        slowTimer: 0,
        slowFactor: 1,
        freezeTimer: 0,
        hitFlash: 0,
        isBoss: false,
        bossShootTimer: 0,
        bossProjectiles: [],
        xpValue: 2,
        rotation: 0,
        tier: enemy.tier,
        trailTimer: 0,
        fireTimer: 0,
        fireDps: 0,
        poisonTimer: 0,
        poisonDps: 0,
        isElite: false,
        elitePulseTimer: 0,
        bossType: 'shooter',
        chargeTimer: 0,
        isCharging: false,
        chargeDir: { x: 0, y: 0 },
        summonTimer: 0,
        auraRadius: 0,
        auraDps: 0,
        visualVariant: 'skitter',
      });
    }
    s.flashText = { text: 'SPLITTER', life: 0.6, color: '#b8475a' };
  }
  if (enemy.role === 'bomber' && !enemy.isBoss) {
    const blastRadius = 95;
    for (const target of s.enemies) {
      if (target !== enemy && target.hp > 0 && dist(target.pos, enemy.pos) <= blastRadius) {
        dealDamageToEnemy(s, target, 12 + s.player.level, undefined, false);
      }
    }
    if (dist(enemy.pos, s.player.pos) <= blastRadius) damagePlayer(s, 18 + s.player.level * 0.5);
    s.screenShake = Math.min(0.16, s.screenShake + 0.035);
  }
  if (enemy.role === 'splitter' && !enemy.isBoss) {
    for (let k = 0; k < 2; k++) {
      const angle = nextRandom(s) * Math.PI * 2;
      const hp = Math.max(5, enemy.maxHp * 0.16);
      s.enemies.push({
        pos: { x: enemy.pos.x + Math.cos(angle) * 14, y: enemy.pos.y + Math.sin(angle) * 14 },
        hp, maxHp: hp,
        speed: enemy.speed * 1.15,
        radius: 9,
        damage: enemy.damage * 0.45,
        type: 'fast',
        role: 'swarmer',
        color: '#8a5a8a',
        shape: 'triangle',
        slowTimer: 0,
        slowFactor: 1,
        freezeTimer: 0,
        hitFlash: 0,
        isBoss: false,
        bossShootTimer: 0,
        bossProjectiles: [],
        xpValue: 1,
        rotation: 0,
        tier: enemy.tier,
        trailTimer: 0,
        fireTimer: 0,
        fireDps: 0,
        poisonTimer: 0,
        poisonDps: 0,
        isElite: false,
        elitePulseTimer: 0,
        bossType: 'shooter',
        chargeTimer: 0,
        isCharging: false,
        chargeDir: { x: 0, y: 0 },
        summonTimer: 0,
        auraRadius: 0,
        auraDps: 0,
        visualVariant: 'skitter',
      });
    }
  }
  playSound(enemy.isBoss ? 'explosion' : 'kill');
  // Stronger death burst, scaled by enemy importance.
  const deathParticles = enemy.isBoss ? 56 : enemy.isElite ? 18 : 10;
  for (let i = 0; i < deathParticles; i++) {
    s.particles.push({
      pos: { x: enemy.pos.x, y: enemy.pos.y },
      vel: { x: rand(s,-180, 180), y: rand(s,-180, 180) },
      life: rand(s,0.3, 0.8), maxLife: 0.8,
      color: enemy.color, size: rand(s,2, 5),
    });
  }
  // XP orb (combo multiplier applies)
  s.xpOrbs.push({
    pos: { x: enemy.pos.x, y: enemy.pos.y },
    value: Math.round(enemy.xpValue * s.player.comboMult), radius: 6, alive: true,
    vel: { x: rand(s,-40, 40), y: rand(s,-40, 40) },
    rotation: nextRandom(s) * Math.PI * 2,
  });
  // health pack drop chance
  const dropChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.5 : 0.04);
  if (nextRandom(s) < dropChance) {
    s.healthPacks.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 10 });
  }
  // Runes are temporary tactical field drops. They are deliberately rarer than XP
  // and independent from the persistent Artifact inventory.
  const runeChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.35 : 0);
  if (nextRandom(s) < runeChance) {
    const runeTypes = Object.keys(RUNE_DEFS) as RuneType[];
    const type = runeTypes[Math.floor(nextRandom(s) * runeTypes.length)];
    s.runes.push({ pos: { ...enemy.pos }, alive: true, radius: 15, type, life: 22 });
  }
  // chest drop: 2% from normal, 20% from elite, 100% from boss
  const chestChance = enemy.isBoss ? 1 : (enemy.isElite ? 0.2 : 0.02);
  if (nextRandom(s) < chestChance) {
    s.chests.push({ pos: { x: enemy.pos.x, y: enemy.pos.y }, alive: true, radius: 14 });
  }
  // swift boots
  if (s.player.artifacts.includes('swift_boots')) {
    s.player.swiftBootsTimer = 3;
  }
  // berserker mastery 5: close-range kill gives temporary speed via buffTimer
  if (getCharacterId(s) === 'berserker' && s.player.characterMasteryLevel >= 5 && dist(enemy.pos, s.player.pos) <= 110) {
    s.player.buffTimer = Math.max(s.player.buffTimer, 2);
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
        vel: { x: rand(s,-250, 250), y: rand(s,-250, 250) },
        life: 0.5, maxLife: 0.5, color: '#d4943d', size: rand(s,3, 6),
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
    // Bosses create a distinct Stella event. Stella is the only gateway to Legendary rewards.
    s.pendingStella = false;
    s.stellaClaims++;
    s.pendingArtifact = null;
    s.stellaChests.push({ pos: { ...enemy.pos }, alive: true, radius: 28, kind: 'stella' });
    s.particles.push({ pos: { ...enemy.pos }, vel: { x: 0, y: 0 }, life: 1.2, maxLife: 1.2, color: '#ffb84d', size: 9 });
  }
}


function pickArtifacts(s: GameState): ArtifactId[] {
  return pickArtifactChoices(s, 3, false, () => nextRandom(s));
}


export function damagePlayer(s: GameState, amount: number): void {
  if (!canReceivePlayerDamage(s.player.invulnerableTimer, s.player.contactDamageCooldown)) return;
  // dodge
  if (nextRandom(s) < getDodgeChance(s)) {
    s.particles.push({ pos: { ...s.player.pos }, vel: { x: 0, y: -60 }, life: 0.5, maxLife: 0.5, color: '#e8dcc0', size: 3 });
    return;
  }
  amount *= getArtifactDamageTakenMultiplier(s);
  if (nextRandom(s) < getArtifactReflectChance(s)) {
    const nearest = s.enemies.reduce((best: EnemyEntity | null, enemy) => {
      if (enemy.hp <= 0) return best;
      const d = Math.hypot(enemy.pos.x - s.player.pos.x, enemy.pos.y - s.player.pos.y);
      if (d > 180) return best;
      if (!best) return enemy;
      const bestD = Math.hypot(best.pos.x - s.player.pos.x, best.pos.y - s.player.pos.y);
      return d < bestD ? enemy : best;
    }, null);
    if (nearest) dealDamageToEnemy(s, nearest, amount * 1.25);
  }
  // shield
  const shieldBranch = getAbilityBranchId(s, 'shield', 4);
  const shieldFinal = getAbilityFinalArchetype(s, 'shield');
  if (s.player.shieldCharges > 0) {
    s.player.shieldCharges--;
    if (shieldBranch === 'shield_reflector') {
      const nearest = s.enemies
        .filter((enemy) => enemy.hp > 0)
        .sort((a, b) => dist(a.pos, s.player.pos) - dist(b.pos, s.player.pos))[0];
      if (nearest && dist(nearest.pos, s.player.pos) < 220) {
        dealDamageToEnemy(s, nearest, amount * 0.75);
      }
    }
    if (shieldFinal === 'shield_resonant_guard') {
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 1);
    }
    if (shieldBranch === 'shield_bastion') {
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0) continue;
        const d = dist(enemy.pos, s.player.pos);
        if (d > 150) continue;
        const dx = enemy.pos.x - s.player.pos.x;
        const dy = enemy.pos.y - s.player.pos.y;
        const len = Math.hypot(dx,dy) || 1;
        enemy.pos.x += (dx / len) * 36;
        enemy.pos.y += (dy / len) * 36;
      }
    }
    for (let i = 0; i < 12; i++) {
      s.particles.push({ pos: { ...s.player.pos }, vel: { x: rand(s,-150, 150), y: rand(s,-150, 150) }, life: 0.4, maxLife: 0.4, color: '#4a7a8a', size: 3 });
    }
    return;
  }
  const bastionReduction = shieldBranch === 'shield_bastion' && s.player.shieldTimer > 0 ? 0.70 : 1;
  const dmg = amount * getDamageTakenMult(s) * bastionReduction;
  // mirror reflect
  if (s.player.artifacts.includes('mirror') && nextRandom(s) < 0.2) {
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
  // Contact and repeated close-range hits get a short post-hit grace period.
  // This is intentionally separate from Dash invulnerability.
  s.player.contactDamageCooldown = 0.60;
  if (getAbilityBranchId(s, 'darkritual', 7) === 'darkritual_sacrifice_core') {
    const sacrificeSphere = getNearestSphere(s, s.player.pos);
    if (sacrificeSphere) {
      emitSpherePulse(s, sacrificeSphere, Math.max(8, dmg * 0.6), 95, '#8a5a8a');
    }
  }
  s.screenShake = Math.min(0.4, s.screenShake + 0.2);
  playSound('damage');
  // combo break on taking damage
  s.player.combo = 0;
  s.player.comboMult = 1;
  s.player.comboTimer = 0;
  // freeze amulet
  if (s.player.artifacts.includes('stasis_core')) {
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
    s.player.deathTimer = 0.90;
    s.stats.time = s.time;
  }
}


export function damagePlayerDoT(s: GameState, amount: number): void {
  if (!canReceivePlayerDoTDamage(s.player.invulnerableTimer)) return;
  if (!Number.isFinite(amount) || amount <= 0) return;

  const mitigated = amount * getArtifactDamageTakenMultiplier(s) * getDamageTakenMult(s);
  if (mitigated <= 0) return;

  s.player.hp -= mitigated;
  s.screenShake = Math.min(0.25, s.screenShake + 0.04);

  if (s.player.hp <= 0) {
    s.player.hp = 0;
    s.player.deathTimer = 0.90;
    s.stats.time = s.time;
  }
}

// ===== Active abilities =====

export function emitSpherePulse(s: GameState, sphere: SphereEntity, damage: number, radius: number, color: string, slow = false, sourceSphere?: SphereEntity): void {
  for (const e of s.enemies) {
    if (e.hp <= 0 || dist(e.pos, sphere.pos) > radius) continue;
    dealDamageToEnemy(s, e, damage, sourceSphere);
    if (slow) {
      e.slowTimer = Math.max(e.slowTimer, 1.2);
      e.slowFactor = Math.min(e.slowFactor, 0.6);
    }
  }
  for (let i = 0; i < 14; i++) {
    const a = nextRandom(s) * Math.PI * 2;
    s.particles.push({
      pos: { ...sphere.pos },
      vel: { x: Math.cos(a) * 90, y: Math.sin(a) * 90 },
      life: 0.35, maxLife: 0.35, color, size: 3,
    });
  }
}