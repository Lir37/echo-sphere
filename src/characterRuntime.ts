import { CHARACTER_DEFS, getBerserkerFuryBonus, getSphereCountResonanceBonus, type CharacterId } from './characters';
import type { EnemyEntity, GameState, SphereEntity, Vec } from './engine';

export type CharacterFormation = 'none' | 'line' | 'triangle' | 'square' | 'cluster';

export const CHARACTER_LOCAL_RADIUS = 420;
export const CHARACTER_LOCAL_SPHERE_CAP = 8;

interface CharacterRuntimePlayer {
  characterId?: CharacterId;
  characterMasteryLevel?: number;
  hunterMarkTarget?: EnemyEntity | null;
  hunterMarkTimer?: number;
  hunterHuntTarget?: EnemyEntity | null;
  hunterHuntTimer?: number;
  alchemistCatalystTimer?: number;
  architectFormationType?: CharacterFormation;
  architectFormationChangedAt?: number;
}

export interface CharacterFormationResult {
  type: CharacterFormation;
  strength: number;
}

function runtimePlayer(s: GameState): CharacterRuntimePlayer {
  return s.player as GameState['player'] & CharacterRuntimePlayer;
}

export function getCharacterDamageMultiplier(s: GameState, sphere: SphereEntity): number {
  const character = getCharacterId(s);
  const mastery = runtimePlayer(s).characterMasteryLevel || 1;
  let multiplier = 1 + CHARACTER_DEFS[character].baseModifiers.sphereDamage;

  if (character === 'spherist') {
    const fullResonanceThreshold = mastery >= 2 ? 4 : 5;
    if (s.spheres.filter((item) => item.alive).length >= fullResonanceThreshold) multiplier += 0.05;
    if (mastery >= 3) multiplier += 0.02;
  }

  if (character === 'berserker') {
    multiplier += getBerserkerFuryBonus(character, s.player.hp / Math.max(1, s.player.maxHp)).damage;
    const closeEnemy = s.enemies.some((enemy) => enemy.hp > 0 && distance(enemy.pos, s.player.pos) <= 110);
    if (closeEnemy) multiplier += mastery >= 2 ? 0.15 : 0.12;
  }

  if (character === 'engineer') {
    const range = getEngineerNetworkRange(s);
    const neighbours = getSphereNeighbours(s, sphere, range);
    multiplier += Math.min(2, neighbours.length) * 0.06;

    if (mastery >= 5) {
      const connectedToMasterNode = neighbours.some((neighbour) => getSphereNeighbours(s, neighbour, range).length >= 2);
      if (connectedToMasterNode) multiplier += 0.02;
    }

    const networkSize = getConnectedNetworkSize(s, sphere, range);
    if (networkSize >= 4) multiplier += 0.03;
  }

  return multiplier;
}

export function getCharacterAttackSpeedMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  const mastery = runtimePlayer(s).characterMasteryLevel || 1;
  let multiplier = 1 + CHARACTER_DEFS[character].baseModifiers.sphereAttackSpeed;

  if (character === 'spherist') {
    const sphereCount = s.spheres.filter((sphere) => sphere.alive).length;
    multiplier += getSphereCountResonanceBonus(character, sphereCount);
    if (mastery >= 4) multiplier += Math.max(0, sphereCount - 1) * 0.005;
    if (sphereCount >= 8 && mastery >= 5) multiplier += 0.05;
  }

  if (character === 'berserker') {
    multiplier += getBerserkerFuryBonus(character, s.player.hp / Math.max(1, s.player.maxHp)).attackSpeed;
    if (mastery >= 3) multiplier += 0.02;
  }

  if (character === 'architect' && getLocalCharacterSpheres(s).length >= 5) {
    multiplier += getFormationAttackSpeedBonus(s);
  }

  return multiplier;
}

export function getCharacterRadiusMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  const mastery = runtimePlayer(s).characterMasteryLevel || 1;
  let multiplier = 1 + CHARACTER_DEFS[character].baseModifiers.sphereRadius;

  if (character === 'spherist' && mastery >= 5 && s.spheres.filter((sphere) => sphere.alive).length >= 8) {
    multiplier += 0.05;
  }

  if (character === 'architect' && getLocalCharacterSpheres(s).length >= 4) multiplier += 0.05;

  if (character === 'engineer' && getEngineerNetworkSpheres(s).length >= 3) multiplier += 0.08;

  return multiplier;
}

export function getCharacterMoveSpeedMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  return 1 + CHARACTER_DEFS[character].baseModifiers.moveSpeed;
}

export function getCharacterMaxHpMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  return 1 + CHARACTER_DEFS[character].baseModifiers.maxHp;
}

export function getCharacterDamageTakenMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  return 1 + CHARACTER_DEFS[character].baseModifiers.damageTaken;
}

export function getCharacterStatusDurationMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  return 1 + CHARACTER_DEFS[character].baseModifiers.statusDuration;
}

export function getCharacterStatusDamageMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  return 1 + CHARACTER_DEFS[character].baseModifiers.statusDamage;
}

export function getLocalCharacterSpheres(s: GameState, radius = CHARACTER_LOCAL_RADIUS): SphereEntity[] {
  return s.spheres
    .filter((sphere) => sphere.alive && distance(sphere.pos, s.player.pos) <= radius)
    .sort((a, b) => distance(a.pos, s.player.pos) - distance(b.pos, s.player.pos))
    .slice(0, CHARACTER_LOCAL_SPHERE_CAP);
}

export function getCharacterFormation(s: GameState): CharacterFormationResult {
  if (getCharacterId(s) !== 'architect') return { type: 'none', strength: 0 };

  const spheres = getLocalCharacterSpheres(s);
  if (spheres.length < 3) return { type: 'none', strength: 0 };

  const clusterStrength = getClusterStrength(spheres);
  const lineStrength = getLineStrength(spheres);
  const squareStrength = getSquareStrength(spheres);
  const triangleStrength = getTriangleStrength(spheres);

  const candidates: CharacterFormationResult[] = [];
  if (spheres.length >= 4 && clusterStrength >= 0.78) candidates.push({ type: 'cluster', strength: clusterStrength });
  if (spheres.length >= 4 && squareStrength >= 0.78) candidates.push({ type: 'square', strength: squareStrength });
  if (spheres.length >= 3 && triangleStrength >= 0.78) candidates.push({ type: 'triangle', strength: triangleStrength });
  if (spheres.length >= 3 && lineStrength >= 0.80) candidates.push({ type: 'line', strength: lineStrength });

  candidates.sort((a, b) => b.strength - a.strength);
  const result = candidates[0] || { type: 'none', strength: 0 };
  trackArchitectFormationChange(s, result.type);
  return result;
}

export function getFormationDamageMultiplier(s: GameState): number {
  if (getCharacterId(s) !== 'architect') return 1;
  const formation = getCharacterFormation(s);
  if (formation.type !== 'line') return 1;
  return 1 + 0.10 * getArchitectFormationBonusScale(s, formation.type);
}

export function getFormationCritBonus(s: GameState): number {
  if (getCharacterId(s) !== 'architect') return 0;
  const formation = getCharacterFormation(s);
  if (formation.type !== 'triangle') return 0;
  return 0.10 * getArchitectFormationBonusScale(s, formation.type);
}

export function getFormationDamageTakenMultiplier(s: GameState): number {
  if (getCharacterId(s) !== 'architect') return 1;
  const formation = getCharacterFormation(s);
  if (formation.type !== 'square') return 1;
  const reduction = 0.12 * getArchitectFormationBonusScale(s, formation.type);
  return Math.max(0.70, 1 - reduction);
}

export function getFormationAttackSpeedBonus(s: GameState): number {
  if (getCharacterId(s) !== 'architect') return 0;
  const formation = getCharacterFormation(s);
  if (formation.type !== 'cluster') return 0;
  return 0.15 * getArchitectFormationBonusScale(s, formation.type);
}

function getArchitectFormationBonusScale(s: GameState, type: CharacterFormation): number {
  const p = runtimePlayer(s);
  let scale = 1;
  const mastery = p.characterMasteryLevel || 1;

  if (mastery >= 4 && p.architectFormationType === type && typeof p.architectFormationChangedAt === 'number') {
    if (s.time - p.architectFormationChangedAt <= 2) scale *= 1.25;
  }

  if (mastery >= 5 && getLocalCharacterSpheres(s).length >= 5) scale *= 1.05;
  return scale;
}

function trackArchitectFormationChange(s: GameState, nextType: CharacterFormation): void {
  const p = runtimePlayer(s);
  if (p.architectFormationType === undefined) {
    p.architectFormationType = nextType;
    p.architectFormationChangedAt = Number.NEGATIVE_INFINITY;
    return;
  }
  if (p.architectFormationType !== nextType) {
    p.architectFormationType = nextType;
    p.architectFormationChangedAt = s.time;
  }
}

export function getHunterMarkMultiplier(s: GameState, enemy: EnemyEntity): number {
  if (getCharacterId(s) !== 'hunter') return 1;
  const p = runtimePlayer(s);
  const markedUntil = p.hunterMarkTarget === enemy ? (p.hunterMarkTimer || 0) : 0;
  if (markedUntil <= 0) return 1;
  const huntActive = (p.hunterHuntTimer || 0) > 0;
  return 1.20 * (huntActive && p.hunterHuntTarget === enemy ? 1.30 : 1);
}

export function shouldMarkHunterTarget(enemy: EnemyEntity): boolean {
  return enemy.isBoss || enemy.isElite || enemy.type === 'tank';
}

export function applyAlchemistReaction(s: GameState, enemy: EnemyEntity): boolean {
  if (getCharacterId(s) !== 'alchemist') return false;
  const fire = enemy.fireTimer > 0;
  const freeze = enemy.freezeTimer > 0;
  const poison = enemy.poisonTimer > 0;
  if (!((fire && poison) || (freeze && poison) || (fire && freeze))) return false;

  const p = runtimePlayer(s);
  const reactionRadius = (p.characterMasteryLevel || 1) >= 2 ? 60 : 55;
  const baseDamage = 20 + s.player.level * 2;
  let burstMultiplier = 1;
  if (fire && poison) burstMultiplier = 2;
  else if (freeze && poison) burstMultiplier = 1.6;
  else if (fire && freeze) burstMultiplier = 1.8;

  enemy.hp -= baseDamage * burstMultiplier;
  if (fire && poison) {
    enemy.fireTimer = 0;
    enemy.poisonTimer = 0;
  } else if (freeze && poison) {
    enemy.freezeTimer = 0;
    enemy.poisonTimer = 0;
    enemy.slowTimer = 2;
    enemy.slowFactor = 0.35;
  } else {
    enemy.fireTimer = 0;
    enemy.freezeTimer = 0;
    enemy.slowTimer = 2;
    enemy.slowFactor = 0.4;
  }

  if ((p.characterMasteryLevel || 1) >= 4) {
    const target = s.enemies.find((other) => other !== enemy && other.hp > 0 && distance(other.pos, enemy.pos) <= reactionRadius);
    if (target) target.hp -= baseDamage * burstMultiplier * 0.5;
  }

  if ((p.characterMasteryLevel || 1) >= 5) p.alchemistCatalystTimer = 2;
  return true;
}

export function getEngineerNetworkRange(s: GameState): number {
  return (runtimePlayer(s).characterMasteryLevel || 1) >= 2 ? 240 : 220;
}

export function getEngineerNetworkSize(s: GameState, sphere: SphereEntity): number {
  return getConnectedNetworkSize(s, sphere, getEngineerNetworkRange(s));
}

export function getEngineerNetworkSpheres(s: GameState): SphereEntity[] {
  const local = getLocalCharacterSpheres(s);
  if (local.length === 0) return [];

  const range = getEngineerNetworkRange(s);
  let best: SphereEntity[] = [];
  for (const start of local) {
    const connected = getConnectedNetworkSizeFromPool(start, local, range);
    if (connected.length > best.length) best = connected;
  }
  return best;
}

export function getCharacterId(s: GameState): CharacterId {
  const raw = runtimePlayer(s).characterId;
  return raw && raw in CHARACTER_DEFS ? raw : 'spherist';
}

function getSphereNeighbours(s: GameState, sphere: SphereEntity, range: number): SphereEntity[] {
  return getLocalCharacterSpheres(s).filter((candidate) => candidate !== sphere && distance(candidate.pos, sphere.pos) <= range);
}

function getConnectedNetworkSize(s: GameState, start: SphereEntity, range: number): number {
  return getConnectedNetworkSizeFromPool(start, getLocalCharacterSpheres(s), range).length;
}

function getConnectedNetworkSizeFromPool(start: SphereEntity, pool: SphereEntity[], range: number): SphereEntity[] {
  if (!pool.includes(start)) return [];

  const visited = new Set<SphereEntity>([start]);
  const queue: SphereEntity[] = [start];
  while (queue.length > 0) {
    const current = queue.shift()!;
    for (const neighbour of pool) {
      if (visited.has(neighbour)) continue;
      if (distance(neighbour.pos, current.pos) > range) continue;
      visited.add(neighbour);
      queue.push(neighbour);
    }
  }
  return [...visited];
}

function getClusterStrength(spheres: SphereEntity[]): number {
  if (spheres.length < 4) return 0;
  let sum = 0;
  let pairs = 0;
  for (let i = 0; i < spheres.length; i++) {
    for (let j = i + 1; j < spheres.length; j++) {
      sum += distance(spheres[i].pos, spheres[j].pos);
      pairs++;
    }
  }
  const average = sum / Math.max(1, pairs);
  return Math.max(0, 1 - Math.max(0, average - 140) / 140);
}

function getLineStrength(spheres: SphereEntity[]): number {
  if (spheres.length < 3) return 0;
  const centroid = spheres.reduce((acc, sphere) => ({ x: acc.x + sphere.pos.x, y: acc.y + sphere.pos.y }), { x: 0, y: 0 });
  centroid.x /= spheres.length;
  centroid.y /= spheres.length;
  let best = 0;
  for (let angle = 0; angle < Math.PI; angle += Math.PI / 16) {
    const dir = { x: Math.cos(angle), y: Math.sin(angle) };
    let maxError = 0;
    for (const sphere of spheres) {
      const dx = sphere.pos.x - centroid.x;
      const dy = sphere.pos.y - centroid.y;
      const perpendicular = Math.abs(dx * dir.y - dy * dir.x);
      maxError = Math.max(maxError, perpendicular);
    }
    best = Math.max(best, 1 - Math.min(1, maxError / 80));
  }
  return best;
}

function getTriangleStrength(spheres: SphereEntity[]): number {
  if (spheres.length < 3) return 0;
  let best = 0;
  for (let i = 0; i < spheres.length; i++) {
    for (let j = i + 1; j < spheres.length; j++) {
      for (let k = j + 1; k < spheres.length; k++) {
        const a = distance(spheres[i].pos, spheres[j].pos);
        const b = distance(spheres[j].pos, spheres[k].pos);
        const c = distance(spheres[k].pos, spheres[i].pos);
        const mean = (a + b + c) / 3;
        if (mean < 70 || mean > 280) continue;
        const variance = (Math.abs(a - mean) + Math.abs(b - mean) + Math.abs(c - mean)) / (3 * mean);
        best = Math.max(best, 1 - Math.min(1, variance * 2));
      }
    }
  }
  return best;
}

function getSquareStrength(spheres: SphereEntity[]): number {
  if (spheres.length < 4) return 0;
  const points = [...spheres].sort((a, b) => a.pos.x - b.pos.x);
  const left = points.slice(0, 2).sort((a, b) => a.pos.y - b.pos.y);
  const right = points.slice(-2).sort((a, b) => a.pos.y - b.pos.y);
  if (left.length !== 2 || right.length !== 2) return 0;
  const top = [left[0], right[0]];
  const bottom = [left[1], right[1]];
  const sides = [
    distance(left[0].pos, left[1].pos),
    distance(right[0].pos, right[1].pos),
    distance(top[0].pos, top[1].pos),
    distance(bottom[0].pos, bottom[1].pos),
  ];
  const mean = sides.reduce((a, b) => a + b, 0) / 4;
  if (mean < 60 || mean > 300) return 0;
  const variance = sides.reduce((sum, side) => sum + Math.abs(side - mean), 0) / (4 * mean);
  return 1 - Math.min(1, variance * 2.5);
}
