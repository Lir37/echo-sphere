import { CHARACTER_DEFS, getBerserkerFuryBonus, getSphereCountResonanceBonus, type CharacterId } from './characters';
import type { EnemyEntity, GameState, SphereEntity, Vec } from './engine';

export type CharacterFormation = 'none' | 'line' | 'triangle' | 'square' | 'cluster';

export const CHARACTER_LOCAL_RADIUS = 420;
export const CHARACTER_LOCAL_SPHERE_CAP = 8;

function distance(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

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

  return multiplier;
}

export function getCharacterRadiusMultiplier(s: GameState): number {
  const character = getCharacterId(s);
  const mastery = runtimePlayer(s).characterMasteryLevel || 1;
  let multiplier = 1 + CHARACTER_DEFS[character].baseModifiers.sphereRadius;

  if (character === 'spherist' && s.spheres.filter((sphere) => sphere.alive).length >= 8 && mastery >= 5) multiplier += 0.05;
  if (character === 'alchemist' && mastery >= 2) multiplier += 0.1;
  if (character === 'architect' && mastery >= 3) multiplier += 0.02;

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
  let multiplier = 1 + CHARACTER_DEFS[character].baseModifiers.statusDamage;
  if (character === 'alchemist' && (runtimePlayer(s).characterMasteryLevel || 1) >= 3) multiplier += 0.05;
  return multiplier;
}

export function getLocalCharacterSpheres(s: GameState, radius = CHARACTER_LOCAL_RADIUS): SphereEntity[] {
  return s.spheres
    .filter((sphere) => sphere.alive && distance(sphere.pos, s.player.pos) <= radius)
    .sort((a, b) => distance(a.pos, s.player.pos) - distance(b.pos, s.player.pos))
    .slice(0, CHARACTER_LOCAL_SPHERE_CAP);
}

export function getHunterMarkMultiplier(s: GameState): number {
  return getCharacterId(s) === 'hunter' && runtimePlayer(s).hunterMarkTarget ? 1.12 : 1;
}

export function shouldMarkHunterTarget(s: GameState, enemy: EnemyEntity): boolean {
  const character = getCharacterId(s);
  if (character !== 'hunter' || !['boss', 'elite', 'tank'].includes(enemy.type)) return false;
  return Boolean(runtimePlayer(s).hunterMarkTarget === enemy);
}

export function registerHunterHit(s: GameState, enemy: EnemyEntity): void {
  if (getCharacterId(s) !== 'hunter' || !['sniper', 'chain'].includes(sphereTypeForEnemyHit(s, enemy))) return;
  if (!['boss', 'elite', 'tank'].includes(enemy.type)) return;
  const p = runtimePlayer(s);
  const mastery = p.characterMasteryLevel || 1;
  if (p.hunterMarkTarget !== enemy) {
    p.hunterMarkTarget = enemy;
    p.hunterMarkTimer = mastery >= 2 ? 6 : 4;
    p.hunterHuntTarget = enemy;
    p.hunterHuntTimer = 6;
  }
}

function sphereTypeForEnemyHit(_s: GameState, _enemy: EnemyEntity): string {
  return 'sniper';
}

export function applyAlchemistReaction(s: GameState, enemy: EnemyEntity): number {
  if (getCharacterId(s) !== 'alchemist') return 0;
  const radius = (runtimePlayer(s).characterMasteryLevel || 1) >= 2 ? 60 : 55;
  const fire = Boolean(enemy.fireTimer && enemy.fireTimer > 0);
  const freeze = Boolean(enemy.freezeTimer && enemy.freezeTimer > 0);
  const poison = Boolean(enemy.poisonTimer && enemy.poisonTimer > 0);
  const statuses = Number(fire) + Number(freeze) + Number(poison);
  if (statuses < 2) return 0;

  let multiplier = 1;
  if (fire && poison) multiplier = 2;
  else if (freeze && poison) multiplier = 1.6;
  else if (fire && freeze) multiplier = 1.8;

  const burst = Math.max(0, enemy.maxHp * 0.02 * multiplier);
  enemy.hp -= burst;

  const target = s.enemies.find((other) => other !== enemy && other.hp > 0 && distance(other.pos, enemy.pos) <= radius);
  if (target && (runtimePlayer(s).characterMasteryLevel || 1) >= 4) target.hp -= burst * 0.5;

  if ((runtimePlayer(s).characterMasteryLevel || 1) >= 5) runtimePlayer(s).alchemistCatalystTimer = 2;
  return burst;
}

export function getEngineerNetworkRange(s: GameState): number {
  return (runtimePlayer(s).characterMasteryLevel || 1) >= 2 ? 240 : 220;
}

export function getSphereNeighbours(s: GameState, sphere: SphereEntity, range = getEngineerNetworkRange(s)): SphereEntity[] {
  return getLocalCharacterSpheres(s).filter((candidate) => candidate !== sphere && distance(candidate.pos, sphere.pos) <= range);
}

function getConnectedNetwork(s: GameState, start: SphereEntity, range: number): SphereEntity[] {
  const local = getLocalCharacterSpheres(s);
  const localSet = new Set(local);
  if (!localSet.has(start)) return [];

  const visited = new Set<SphereEntity>([start]);
  const queue = [start];
  for (let index = 0; index < queue.length; index++) {
    const current = queue[index];
    for (const neighbour of local) {
      if (visited.has(neighbour)) continue;
      if (distance(neighbour.pos, current.pos) > range) continue;
      visited.add(neighbour);
      queue.push(neighbour);
    }
  }
  return queue;
}

function getConnectedNetworkSize(s: GameState, start: SphereEntity, range: number): number {
  return getConnectedNetwork(s, start, range).length;
}

export function getEngineerNetworkSpheres(s: GameState): SphereEntity[] {
  const local = getLocalCharacterSpheres(s);
  const range = getEngineerNetworkRange(s);
  let largest: SphereEntity[] = [];
  for (const sphere of local) {
    const network = getConnectedNetwork(s, sphere, range);
    if (network.length > largest.length) largest = network;
  }
  return largest;
}

export function getEngineerNetworkSize(s: GameState): number {
  return getEngineerNetworkSpheres(s).length;
}

export function getCharacterFormation(s: GameState): CharacterFormationResult {
  const spheres = getLocalCharacterSpheres(s);
  const mastery = runtimePlayer(s).characterMasteryLevel || 1;
  const toleranceMultiplier = mastery >= 2 ? 1.15 : 1;

  if (spheres.length < 3) return { type: 'none', strength: 0 };

  const centroid = spheres.reduce((sum, sphere) => ({ x: sum.x + sphere.pos.x, y: sum.y + sphere.pos.y }), { x: 0, y: 0 });
  centroid.x /= spheres.length;
  centroid.y /= spheres.length;

  const vectors = spheres.map((sphere) => ({
    x: sphere.pos.x - centroid.x,
    y: sphere.pos.y - centroid.y,
  }));

  const radii = vectors.map((vector) => Math.hypot(vector.x, vector.y));
  const avgRadius = radii.reduce((sum, radius) => sum + radius, 0) / Math.max(1, radii.length);
  if (avgRadius < 1) return { type: 'cluster', strength: Math.min(1, spheres.length / 8) };

  const lineStrength = getLineStrength(vectors, avgRadius);
  if (lineStrength >= 0.78 / toleranceMultiplier) return { type: 'line', strength: lineStrength };

  const triangleStrength = getTriangleStrength(vectors, avgRadius);
  if (spheres.length === 3 && triangleStrength >= 0.8 / toleranceMultiplier) return { type: 'triangle', strength: triangleStrength };

  const squareStrength = getSquareStrength(spheres);
  if (spheres.length === 4 && squareStrength >= 0.8 / toleranceMultiplier) return { type: 'square', strength: squareStrength };

  return { type: 'cluster', strength: Math.min(1, spheres.length / 8) };
}

function getLineStrength(vectors: Vec[], avgRadius: number): number {
  let best = 0;
  const tolerance = Math.max(30, avgRadius * 0.25);
  for (const vector of vectors) {
    const angle = Math.atan2(vector.y, vector.x);
    const nx = -Math.sin(angle);
    const ny = Math.cos(angle);
    const sum = vectors.reduce((acc, other) => acc + Math.abs(other.x * nx + other.y * ny), 0);
    const score = 1 - sum / Math.max(1, vectors.length * avgRadius);
    best = Math.max(best, Math.max(0, Math.min(1, score + 1 / Math.max(1, tolerance))));
  }
  return best;
}

function getTriangleStrength(vectors: Vec[], avgRadius: number): number {
  if (vectors.length !== 3) return 0;
  const distances = [
    Math.hypot(vectors[0].x - vectors[1].x, vectors[0].y - vectors[1].y),
    Math.hypot(vectors[1].x - vectors[2].x, vectors[1].y - vectors[2].y),
    Math.hypot(vectors[2].x - vectors[0].x, vectors[2].y - vectors[0].y),
  ];
  const min = Math.min(...distances);
  const max = Math.max(...distances);
  return min / Math.max(max, avgRadius * 0.01);
}

function getSquareStrength(spheres: SphereEntity[]): number {
  if (spheres.length !== 4) return 0;
  const sorted = [...spheres].sort((a, b) => a.pos.y - b.pos.y || a.pos.x - b.pos.x);
  const top = sorted.slice(0, 2).sort((a, b) => a.pos.x - b.pos.x);
  const bottom = sorted.slice(2).sort((a, b) => a.pos.x - b.pos.x);
  const edges = [
    distance(top[0].pos, top[1].pos),
    distance(bottom[0].pos, bottom[1].pos),
    distance(top[0].pos, bottom[0].pos),
    distance(top[1].pos, bottom[1].pos),
  ];
  const min = Math.min(...edges);
  const max = Math.max(...edges);
  return min / Math.max(max, 1);
}

export function getFormationDamageTakenMultiplier(s: GameState, enemy: EnemyEntity): number {
  void enemy;
  const formation = getCharacterFormation(s);
  if (formation.type !== 'square') return 1;
  return 0.95;
}

export function getCharacterId(s: GameState): CharacterId {
  const id = runtimePlayer(s).characterId;
  if (id && id in CHARACTER_DEFS) return id;
  return 'spherist';
}
