import {
  getCharacterId,
  getCharacterMoveSpeedMultiplier,
} from './characterRuntime';
import {
  getArtifactMoveSpeedMultiplier,
  getArtifactXpMultiplier,
} from './artifactSystem';
import { BASE_PLAYER_SPEED } from './engineState';
import type { GameState } from './engineTypes';

export function getMoveSpeed(s: GameState): number {
  let sp = BASE_PLAYER_SPEED;
  const lvl = s.player.abilities.movespeed || 0;
  sp *= 1 + lvl * 0.1;
  sp *= 1 + (s.shopUpgrades.speed || 0) * 0.05;
  sp *= getArtifactMoveSpeedMultiplier(s);
  if (s.player.swiftBootsTimer > 0) sp *= 1.1;
  if (s.player.mutationStage >= 3) sp *= 1.2;
  sp *= getCharacterMoveSpeedMultiplier(s);
  if (s.player.hunterTrophyTimer > 0 && getCharacterId(s) === 'hunter') sp *= 1.1;
  if (getCharacterId(s) === 'berserker' && s.player.characterMasteryLevel >= 5 && s.player.buffTimer > 0) sp *= 1.05;
  return sp;
}

export function getCooldownMult(s: GameState): number {
  return getArtifactCooldownMultiplier(s);
}

export function getXpMult(s: GameState): number {
  let m = 1;
  m *= 1 + (s.shopUpgrades.xp || 0) * 0.05;
  m *= getArtifactXpMultiplier(s);
  return m;
}

export function getMagnetRadius(s: GameState): number {
  let r = 60;
  const lvl = s.player.abilities.magnet || 0;
  r *= 1 + lvl * 0.2;
  return r;
}
