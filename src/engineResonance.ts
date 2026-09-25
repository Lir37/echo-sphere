import { addResonanceChargeFromSource, type ResonanceSource } from './resonance';
import type { SphereNetworkState } from './network';
import { getLinkedNodeIndexes } from './network';
import { dist, getNetworkNodes, getNetworkFrame } from './engineRuntime';
import type { GameState, EnemyEntity, SphereEntity, Vec } from './engineTypes';

export type ResonanceDamageHandler = (
  s: GameState,
  enemy: EnemyEntity,
  dmg: number,
  fromSphere?: SphereEntity,
  allowSphereProc?: boolean,
) => void;

function getResonanceFormation(network: SphereNetworkState) {
  // Prefer the most structurally expressive active geometry for the event.
  return network.fractal ?? network.lattice ?? network.ring ?? network.square ?? network.triangle ?? network.cluster ?? network.line;
}

function resonanceFormationCenter(s: GameState, nodes: number[], networkNodes = getNetworkNodes(s)): Vec {
  const positions = nodes.map((index) => networkNodes[index]?.pos).filter((pos): pos is Vec => Boolean(pos));
  if (positions.length === 0) return { ...s.player.pos };
  return positions.reduce((acc, pos) => ({ x: acc.x + pos.x / positions.length, y: acc.y + pos.y / positions.length }), { x: 0, y: 0 });
}

export function triggerResonanceEvent(s: GameState, dealDamage: ResonanceDamageHandler, network?: SphereNetworkState): void {
  const networkState = network ?? getNetworkFrame(s);
  const networkNodes = getNetworkNodes(s);
  const formation = getResonanceFormation(networkState);
  const type = formation?.type ?? 'none';
  const center = formation ? resonanceFormationCenter(s, formation.nodes, networkNodes) : { ...s.player.pos };
  const baseDamage = 16 + s.player.level * 2;
  s.player.resonanceEventActive = true;
  try {
    if (type === 'fractal') {
      // Fractal Echo replays the strongest lower-order geometry and then
      // emits a secondary echo around the whole fractal.
      const replay = network.lattice ?? network.ring ?? network.square ?? network.triangle;
      const replayNodes = (replay?.nodes || []).filter((index) => index < s.spheres.length);
      for (const index of replayNodes) {
        const sphere = s.spheres[index];
        if (!sphere?.alive) continue;
        sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.35);
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.45);
        s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.45, maxLife: 0.45, color: '#ffb84d', size: 4 });
        for (const enemy of s.enemies) {
          if (enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= 95) {
            dealDamage(s, enemy, baseDamage * 0.55, sphere, false);
          }
        }
      }
      for (const enemy of s.enemies) {
        if (enemy.hp > 0 && dist(enemy.pos, center) <= 155) {
          dealDamage(s, enemy, baseDamage * 0.65, undefined, false);
        }
      }
      s.flashText = { text: 'FRACTAL ECHO', life: 0.9, color: '#ffb84d' };
    } else if (type === 'lattice') {
      // Lattice Cascade synchronizes attack timing across the formation.
      for (const index of (formation?.nodes || []).filter((value) => value < s.spheres.length)) {
        const sphere = s.spheres[index];
        if (sphere?.alive) {
          sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.55);
          sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.40);
          s.particles.push({ pos: { ...sphere.pos }, vel: { x: 0, y: 0 }, life: 0.35, maxLife: 0.35, color: '#39d8ff', size: 4 });
        }
      }
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, center) <= 145) dealDamage(s, enemy, baseDamage * 0.95, undefined, false);
      s.flashText = { text: 'LATTICE CASCADE', life: 0.9, color: '#39d8ff' };
    } else if (type === 'ring') {
      // Ring Loop continues for a short duration and advances node by node.
      s.player.resonanceRingTimer = 2.4;
      s.player.resonanceRingPulseTimer = 0;
      s.player.resonanceRingCursor = 0;
      s.flashText = { text: 'RING LOOP', life: 0.9, color: '#55e69a' };
    } else if (type === 'square') {
      // Square Shell uses the existing shield-charge gate, not global i-frames.
      s.player.shieldCharges = Math.min(5, s.player.shieldCharges + 2);
      s.player.shieldTimer = Math.max(s.player.shieldTimer, 2.5);
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, center) <= 150) dealDamage(s, enemy, baseDamage * 1.2, undefined, false);
      s.flashText = { text: 'SQUARE RESONANCE', life: 0.9, color: '#d4943d' };
    } else if (type === 'triangle') {
      // Triangle Arc is explicitly routed through the active linked nodes.
      const triangleNodes = (formation?.nodes || []).filter((index) => index < s.spheres.length && s.spheres[index]?.alive);
      const targets = s.enemies
        .filter((enemy) => enemy.hp > 0)
        .sort((a, b) => dist(a.pos, center) - dist(b.pos, center));
      const usedTargets = new Set<EnemyEntity>();
      for (let i = 0; i < triangleNodes.length; i++) {
        const nodeIndex = triangleNodes[i];
        const sphere = s.spheres[nodeIndex];
        if (!sphere) continue;
        const linked = getLinkedNodeIndexes(networkState, nodeIndex).find((index) => triangleNodes.includes(index));
        const nextIndex = linked ?? triangleNodes[(i + 1) % triangleNodes.length];
        const nextSphere = s.spheres[nextIndex];
        if (nextSphere) s.lightnings.push({ from: { ...sphere.pos }, to: { ...nextSphere.pos }, life: 0.22 });
        sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.38);
        const target = targets.find((enemy) => !usedTargets.has(enemy)) ?? targets[i % Math.max(1, targets.length)];
        if (target) {
          usedTargets.add(target);
          dealDamage(s, target, baseDamage * 0.65, sphere, false);
        }
      }
      s.flashText = { text: 'TRIANGLE RESONANCE', life: 0.9, color: '#8a5a8a' };
    } else if (type === 'cluster') {
      for (const enemy of s.enemies) {
        if (enemy.hp <= 0 || dist(enemy.pos, center) > 135) continue;
        const dx = enemy.pos.x - center.x, dy = enemy.pos.y - center.y, d = Math.hypot(dx, dy) || 1;
        enemy.pos.x += dx / d * 36;
        enemy.pos.y += dy / d * 36;
        dealDamage(s, enemy, baseDamage * 0.9, undefined, false);
      }
      s.flashText = { text: 'CLUSTER RESONANCE', life: 0.9, color: '#c4453d' };
    } else if (type === 'line') {
      // Line Surge is consumed by the next actual Sphere attack.
      s.player.resonanceLineBurst = Math.max(s.player.resonanceLineBurst, 1);
      s.flashText = { text: 'LINE RESONANCE', life: 0.9, color: '#4a7a8a' };
    } else {
      for (const enemy of s.enemies) if (enemy.hp > 0 && dist(enemy.pos, s.player.pos) <= 90) dealDamage(s, enemy, baseDamage * 0.6, undefined, false);
      s.flashText = { text: 'RESONANCE', life: 0.9, color: '#d4943d' };
    }
    s.screenShake = Math.min(0.18, s.screenShake + 0.06);
  } finally {
    s.player.resonanceEventActive = false;
  }
}

export function updateResonanceRing(
  s: GameState,
  dt: number,
  network: ReturnType<typeof analyzeSphereNetwork>,
  dealDamage: ResonanceDamageHandler,
): void {
  if (s.player.resonanceRingTimer <= 0) return;
  s.player.resonanceRingTimer = Math.max(0, s.player.resonanceRingTimer - dt);
  s.player.resonanceRingPulseTimer -= dt;
  if (s.player.resonanceRingPulseTimer > 0) return;

  const ringNodes = (network.ring?.nodes || []).filter((index) => index < s.spheres.length && s.spheres[index]?.alive);
  if (ringNodes.length === 0) {
    s.player.resonanceRingTimer = 0;
    return;
  }

  s.player.resonanceRingPulseTimer = 0.42;
  const cursor = s.player.resonanceRingCursor % ringNodes.length;
  const nodeIndex = ringNodes[cursor];
  const nextIndex = ringNodes[(cursor + 1) % ringNodes.length];
  s.player.resonanceRingCursor = (cursor + 1) % ringNodes.length;

  const sphere = s.spheres[nodeIndex];
  const nextSphere = s.spheres[nextIndex];
  if (!sphere) return;

  sphere.resonancePulseTimer = Math.max(sphere.resonancePulseTimer, 0.40);
  sphere.attackTimer = Math.max(0, sphere.attackTimer - 0.30);
  if (nextSphere) s.lightnings.push({ from: { ...sphere.pos }, to: { ...nextSphere.pos }, life: 0.24 });

  for (const enemy of s.enemies) {
    if (enemy.hp > 0 && dist(enemy.pos, sphere.pos) <= 105) {
      dealDamage(s, enemy, (16 + s.player.level * 2) * 0.42, sphere, false);
    }
  }
}

export function chargeResonance(s: GameState, source: ResonanceSource, dealDamage: ResonanceDamageHandler): void {
  if (s.player.resonanceEventActive) return;
  const events = addResonanceChargeFromSource(s.player, source);
  const network = getNetworkFrame(s);
  for (let i = 0; i < events; i++) triggerResonanceEvent(s, dealDamage, network);
}

export function syncResonanceGeometry(s: GameState, network: SphereNetworkState | undefined, dealDamage: ResonanceDamageHandler): void {
  const resolvedNetwork = network ?? getNetworkFrame(s);
  const formation = getResonanceFormation(resolvedNetwork);
  const key = formation ? formation.type + ':' + formation.nodes.join(',') : 'none';
  if (key === s.player.resonanceGeometryKey) return;

  if (s.player.resonanceGeometryKey !== 'none' && s.player.resonanceGeometryNodes.length > 0) {
    const previousNodes = s.player.resonanceGeometryNodes
      .map((index) => s.spheres[index]?.pos)
      .filter((pos): pos is Vec => Boolean(pos))
      .map((pos) => ({ ...pos }));
    if (previousNodes.length >= 2) {
      s.formationMemory = {
        type: s.player.resonanceGeometryKey.split(':', 1)[0],
        nodes: previousNodes,
        expiresAt: s.time + 0.9,
      };
    }
  }

  // Geometry charge is earned only when a genuinely new formation appears.
  // Losing a formation and recovering the same key (e.g. Link Breaker) does
  // not recharge the resource.
  const gainedFormation = Boolean(formation) && key !== s.player.resonanceLastActiveFormationKey;
  s.player.resonanceGeometryKey = key;
  s.player.resonanceGeometryNodes = formation ? [...formation.nodes] : [];
  if (formation) s.player.resonanceLastActiveFormationKey = key;
  if (gainedFormation) chargeResonance(s, 'geometry', dealDamage);
}


