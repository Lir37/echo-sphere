import { SPHERE_TYPES } from './gameData';
import { playSound } from './audio';
import { dealDamageToEnemy } from './engineCombat';
import type { GameState, SphereEntity, EnemyEntity, Vec } from './engineTypes';

// Sphere lifecycle, derived stats, targeting, firing and projectile behaviour.
export const DEFAULT_MAX_SPHERES = 5;
export const MAX_SPHERES_CAP = 8;
export const BASE_SPHERE_RADIUS = 130;
export const BASE_SPHERE_DAMAGE = 12;
export const BASE_SPHERE_DELAY = 1.2;

export function createSphere(pos: Vec, type: SphereEntity['type'], visualTier = 0): SphereEntity {
  return {
    pos: { ...pos },
    radius: BASE_SPHERE_RADIUS,
    damage: BASE_SPHERE_DAMAGE,
    attackDelay: BASE_SPHERE_DELAY,
    attackTimer: 0,
    rotation: 0,
    alive: true,
    killsContribution: 0,
    visualTier,
    type,
    auraTimer: 0,
  };
}

export function getMaxSpheres(s: GameState): number {
  let m = DEFAULT_MAX_SPHERES + (s.player.abilities.maxspheres || 0) + (s.shopUpgrades.spheres || 0);
  return Math.min(m, MAX_SPHERES_CAP);
}

export function getSphereRadius(s: GameState, sphere: SphereEntity): number {
  let r = BASE_SPHERE_RADIUS;
  const lvl = s.player.abilities.radius || 0;
  r *= 1 + lvl * 0.15;
  r *= 1 + (s.shopUpgrades.radius || 0) * 0.05;
  if (s.player.artifacts.includes('radius_shard')) r *= 1.1;
  if (s.player.chaosOrbBuff === 'radius' && s.player.chaosOrbBuffTimer > 0) r *= 1.2;
  if (s.player.mutationStage >= 2) r *= 1.15;
  return r;
}

export function getSphereDamage(s: GameState, sphere: SphereEntity): number {
  let d = BASE_SPHERE_DAMAGE;
  const lvl = s.player.abilities.damage || 0;
  d *= 1 + lvl * 0.2;
  d *= 1 + (s.shopUpgrades.dmg || 0) * 0.05;
  if (s.player.mutationStage >= 1) d *= 1.1;
  if (s.player.chaosOrbBuff === 'dmg' && s.player.chaosOrbBuffTimer > 0) d *= 1.2;
  if (s.player.teleportDamageBuffTimer > 0) d *= 2;
  const sbLvl = s.player.abilities.sphereboost || 0;
  if (sbLvl > 0) {
    const per = Math.max(50, 100 - (sbLvl - 1) * 10);
    d += Math.floor(s.player.kills / per);
  }
  if (s.player.evolutions.includes('echoaccumulator')) {
    d += s.player.sphereXpAccumulator * 0.5;
  }
  return d;
}

export function getSphereDelay(s: GameState): number {
  let d = BASE_SPHERE_DELAY;
  const lvl = s.player.abilities.attackspeed || 0;
  d *= Math.pow(0.9, lvl);
  return d;
}

function updateSpheres(s: GameState, dt: number): void {
  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    const stype = SPHERE_TYPES[sphere.type];
    const radius = getSphereRadius(s, sphere) * stype.rangeMult;
    const damage = getSphereDamage(s, sphere) * stype.damageMult;
    const delay = getSphereDelay(s) * stype.delayMult;
    // aura type: continuous AoE damage — no barrel rotation
    if (stype.aura) {
      sphere.auraTimer -= dt;
      if (sphere.auraTimer <= 0) {
        sphere.auraTimer = 0.5;
        for (const e of s.enemies) {
          if (e.hp <= 0) continue;
          if (dist(e.pos, sphere.pos) < stype.auraRadius) {
            dealDamageToEnemy(s, e, damage);
          }
        }
      }
      continue;
    }
    // find nearest enemy for aiming (every frame, not just on fire)
    let nearest: EnemyEntity | null = null;
    let nd = Infinity;
    for (const e of s.enemies) {
      if (e.hp <= 0) continue;
      const d = dist(e.pos, sphere.pos);
      if (d < radius && d < nd) { nd = d; nearest = e; }
    }
    // aim turret at nearest enemy; stay static if no enemies
    if (nearest) {
      const adx = nearest.pos.x - sphere.pos.x;
      const ady = nearest.pos.y - sphere.pos.y;
      sphere.rotation = Math.atan2(ady, adx);
    }
    sphere.attackTimer -= dt;
    if (sphere.attackTimer <= 0) {
      if (nearest) {
        sphere.attackTimer = delay;
        const dx = nearest.pos.x - sphere.pos.x;
        const dy = nearest.pos.y - sphere.pos.y;
        const d = Math.hypot(dx, dy) || 1;
        const dirX = dx / d;
        const dirY = dy / d;
        const mods = s.player.towerMods;
        const shots = (1 + mods.multishot) * stype.pellets;
        for (let i = 0; i < shots; i++) {
          const spread = shots > 1 ? (i - (shots - 1) / 2) * (stype.spread / Math.max(1, shots - 1) || 0.15) : 0;
          const a = Math.atan2(dirY, dirX) + spread;
          let effect: 'none' | 'fire' | 'freeze' | 'poison' = 'none';
          if (mods.fire > 0) effect = 'fire';
          else if (mods.freeze > 0) effect = 'freeze';
          else if (mods.poison > 0) effect = 'poison';
          let color = stype.color;
          if (effect === 'fire') color = '#c46d3d';
          else if (effect === 'freeze') color = '#4a7a8a';
          else if (effect === 'poison') color = '#5a8c4a';
          const speed = 350 * stype.projectileSpeedMult;
          s.sphereProjectiles.push({
            pos: { ...sphere.pos },
            vel: { x: Math.cos(a) * speed, y: Math.sin(a) * speed },
            damage,
            radius: 5,
            alive: true,
            color,
            pierce: mods.pierce + (stype.chain ? 99 : 0),
            hitEnemies: new Set(),
            effect,
            ricochet: mods.ricochet,
            life: 2,
          });
          // chain lightning: instantly hit nearby enemies
          if (stype.chain) {
            const chainTargets: EnemyEntity[] = [];
            let current = nearest;
            const hitSet = new Set<EnemyEntity>([current]);
            for (let c = 0; c < 3; c++) {
              let next: EnemyEntity | null = null;
              let cd2 = Infinity;
              for (const e2 of s.enemies) {
                if (e2.hp <= 0 || hitSet.has(e2)) continue;
                const dd = dist(e2.pos, current.pos);
                if (dd < 150 && dd < cd2) { cd2 = dd; next = e2; }
              }
              if (!next) break;
              chainTargets.push(next);
              hitSet.add(next);
              current = next;
            }
            // apply damage to chain targets
            for (const ct of chainTargets) {
              dealDamageToEnemy(s, ct, damage * 0.7);
              s.lightnings.push({ from: { ...nearest.pos }, to: { ...ct.pos }, life: 0.3 });
            }
          }
        }
        playSound('shoot');
      }
    }
  }
  // update traveling projectiles
  for (let i = s.sphereProjectiles.length - 1; i >= 0; i--) {
    const p = s.sphereProjectiles[i];
    p.pos.x += p.vel.x * dt;
    p.pos.y += p.vel.y * dt;
    p.life -= dt;
    let hit = false;
    for (const e of s.enemies) {
      if (e.hp <= 0 || p.hitEnemies.has(e)) continue;
      if (dist(p.pos, e.pos) < p.radius + e.radius) {
        dealDamageToEnemy(s, e, p.damage);
        p.hitEnemies.add(e);
        hit = true;
        // impact effect particles
        for (let k = 0; k < 6; k++) {
          const a = Math.random() * Math.PI * 2;
          s.particles.push({ pos: { ...p.pos }, vel: { x: Math.cos(a) * 80, y: Math.sin(a) * 80 }, life: 0.3, maxLife: 0.3, color: p.color, size: 2 });
        }
        // apply status effects
        if (p.effect === 'fire') {
          e.fireTimer = (e.fireTimer || 0) + 3;
          e.fireDps = 5 + s.player.towerMods.fire * 3;
        } else if (p.effect === 'freeze') {
          e.freezeTimer = Math.max(e.freezeTimer || 0, 0.5 + s.player.towerMods.freeze * 0.3);
        } else if (p.effect === 'poison') {
          e.poisonTimer = (e.poisonTimer || 0) + 4;
          e.poisonDps = 3 + s.player.towerMods.poison * 2;
        }
        if (p.pierce > 0) {
          p.pierce--;
          hit = false;
        } else if (p.ricochet > 0) {
          p.ricochet--;
          // find new target
          let next: EnemyEntity | null = null;
          let nd = Infinity;
          for (const e2 of s.enemies) {
            if (e2.hp <= 0 || p.hitEnemies.has(e2)) continue;
            const d = dist(p.pos, e2.pos);
            if (d < nd) { nd = d; next = e2; }
          }
          if (next) {
            const dx = next.pos.x - p.pos.x;
            const dy = next.pos.y - p.pos.y;
            const d = Math.hypot(dx, dy) || 1;
            p.vel.x = (dx / d) * 350;
            p.vel.y = (dy / d) * 350;
            hit = false;
          } else { hit = true; }
        } else { hit = true; }
        break;
      }
    }
    if (hit || p.life <= 0) s.sphereProjectiles.splice(i, 1);
  }
}

export function setSphereType(s: GameState, type: SphereEntity['type']): void {
  s.selectedSphereType = type;
  playSound('place');
}

export function placeSphere(s: GameState, x: number, y: number): void {
  // toggle: if clicking near an existing sphere, remove it instead
  const existing = s.spheres.find(sp => sp.alive && Math.hypot(sp.pos.x - x, sp.pos.y - y) < 26);
  if (existing) {
    removeSphere(s, existing);
    return;
  }
  const max = getMaxSpheres(s);
  if (s.spheres.length >= max) return;
  s.spheres.push(createSphere({ x, y }, s.selectedSphereType, Math.floor(s.player.towerUpgradeCount / 5)));
  const stype = SPHERE_TYPES[s.selectedSphereType];
  for (let i = 0; i < 15; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({ pos: { x, y }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.5, maxLife: 0.5, color: stype.color, size: 3 });
  }
  playSound('place');
}

export function removeSphere(s: GameState, sphere: SphereEntity): void {
  sphere.alive = false;
  s.spheres = s.spheres.filter(sp => sp !== sphere);
  for (let i = 0; i < 15; i++) {
    const a = Math.random() * Math.PI * 2;
    s.particles.push({ pos: { ...sphere.pos }, vel: { x: Math.cos(a) * 120, y: Math.sin(a) * 120 }, life: 0.5, maxLife: 0.5, color: '#b8475a', size: 3 });
  }
}

export function syncSphereVisualTier(s: GameState): void {
  const newTier = Math.floor(s.player.towerUpgradeCount / 5);
  for (const sp of s.spheres) {
    sp.visualTier = Math.max(sp.visualTier, newTier);
  }
}
