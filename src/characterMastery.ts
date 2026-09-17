import { getCharacterFormation, getCharacterId, getEngineerNetworkSpheres } from './characterRuntime';
import type { GameState } from './engine';

export interface MasteryRunTracker {
  xp: number;
  lastEliteKills: number;
  lastBosses: number;
}

export function createMasteryRunTracker(): MasteryRunTracker {
  return {
    xp: 0,
    lastEliteKills: 0,
    lastBosses: 0,
  };
}

export function tickCharacterMastery(
  s: GameState,
  dt: number,
  tracker: MasteryRunTracker,
): void {
  if (
    dt <= 0 ||
    s.gameOver ||
    s.paused ||
    s.pendingUpgrade ||
    s.pendingArtifact ||
    s.pendingEvolution ||
    s.pendingTowerUpgrade ||
    s.pendingChest
  ) return;

  const character = getCharacterId(s);
  const p = s.player;

  // Discrete achievements inside the run.
  const eliteDelta = Math.max(0, p.eliteKills - tracker.lastEliteKills);
  const bossDelta = Math.max(0, s.bossDefeated - tracker.lastBosses);
  tracker.lastEliteKills = p.eliteKills;
  tracker.lastBosses = s.bossDefeated;

  if (character === 'hunter') {
    tracker.xp += eliteDelta * 20 + bossDelta * 45;
    if (p.hunterMarkTimer > 0) tracker.xp += dt * 1.5;
    if (p.hunterHuntTimer > 0) tracker.xp += dt * 2.25;
  }

  switch (character) {
    case 'spherist': {
      const spheres = s.spheres.filter((sphere) => sphere.alive).length;
      if (spheres >= 4) tracker.xp += dt * 0.85;
      if (spheres >= 6) tracker.xp += dt * 0.65;
      if (spheres >= 8) tracker.xp += dt * 0.5;
      break;
    }

    case 'engineer': {
      const networkSize = getEngineerNetworkSpheres(s).length;
      if (networkSize >= 3) tracker.xp += dt * 1.0;
      if (networkSize >= 5) tracker.xp += dt * 0.65;
      if (p.engineerRelayTimer > 0) tracker.xp += dt * 0.75;
      break;
    }

    case 'berserker': {
      const hpRatio = p.hp / Math.max(1, p.maxHp);
      if (hpRatio <= 0.6) tracker.xp += dt * 0.85;
      if (hpRatio <= 0.3) tracker.xp += dt * 0.8;
      const closeEnemies = s.enemies.some(
        (enemy) => enemy.hp > 0 && Math.hypot(enemy.pos.x - p.pos.x, enemy.pos.y - p.pos.y) <= 110,
      );
      if (closeEnemies) tracker.xp += dt * 0.55;
      break;
    }

    case 'alchemist': {
      const statusTargets = s.enemies.filter((enemy) => enemy.hp > 0 && (
        enemy.fireTimer > 0 || enemy.freezeTimer > 0 || enemy.poisonTimer > 0
      )).length;
      const multiStatusTargets = s.enemies.filter((enemy) => enemy.hp > 0 && (
        Number(enemy.fireTimer > 0) + Number(enemy.freezeTimer > 0) + Number(enemy.poisonTimer > 0) >= 2
      )).length;
      tracker.xp += statusTargets * dt * 0.07;
      tracker.xp += multiStatusTargets * dt * 0.12;
      if (p.alchemistCatalystTimer > 0) tracker.xp += dt * 2;
      break;
    }

    case 'architect': {
      const formation = getCharacterFormation(s);
      if (formation.type !== 'none') tracker.xp += dt * 1.0;
      if (formation.strength >= 0.9) tracker.xp += dt * 0.75;
      break;
    }
  }

  tracker.xp = Math.max(0, tracker.xp);
}

export function getMasteryRunXp(tracker: MasteryRunTracker): number {
  return Math.floor(tracker.xp);
}
