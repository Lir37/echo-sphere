import type { AbilityType, SphereType } from './gameData';
import type { GameState, SphereEntity, Vec } from './engineTypes';
import { buildRuntimeNetworkNodes } from './networkRuntime';
import { analyzeSphereNetwork, type SphereNetworkState, type NetworkShape } from './network';
import { nextRandom } from './rng';

export function dist(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

export function rand(s: GameState, min: number, max: number): number {
  return min + nextRandom(s) * (max - min);
}

export function clamp(v: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, v));
}

export function getNetworkNodes(s: GameState) {
  return buildRuntimeNetworkNodes(s.spheres, s.minions, (s.player.abilities.minion || 0) >= 3);
}

export function getAbilityBranchId(s: GameState, ability: AbilityType, stage: 4 | 7): string | null {
  const prefix = 'ability:' + ability + ':' + stage + ':';
  const marker = (s.player.evolutions || []).find((x: string) => x.startsWith(prefix));
  return marker ? marker.slice(prefix.length) : null;
}


export function getNearestSphere(s: GameState, origin: Vec, predicate?: (sphere: SphereEntity) => boolean): SphereEntity | null {
  let nearest: SphereEntity | null = null;
  let best = Infinity;
  for (const sphere of s.spheres) {
    if (!sphere.alive || (predicate && !predicate(sphere))) continue;
    const d = dist(origin, sphere.pos);
    if (d < best) {
      best = d;
      nearest = sphere;
    }
  }
  return nearest;
}


export function getSphereFinalIndex(s: GameState, type: SphereType): number | null {
  const marker = (s.player.evolutions || []).find((x: string) => x.startsWith('sphere:' + type + ':7:'));
  if (!marker) return null;
  const value = Number(marker.slice(marker.lastIndexOf(':') + 1));
  return Number.isFinite(value) ? value : null;
}



export function getNetworkFrame(s: GameState): SphereNetworkState {
  if (s.networkFrame?.frameId === s.networkFrameId) return s.networkFrame.network;

  // Preserve formation identity through temporary Network disruption.
  const previousNetwork = s.networkFrame?.network;
  const previousDominantShape = previousNetwork?.dominantFormation || null;
  const previousDominant = previousDominantShape?.type || 'none';

  let preservedDominant: NetworkShape | null = null;
  if (previousDominantShape) {
    const allNodesArePersistentSpheres = previousDominantShape.nodes.length > 0 &&
      previousDominantShape.nodes.every((index) =>
        index >= 0 &&
        index < s.spheres.length &&
        s.spheres[index]?.alive !== false
      );
    const currentlyDisabledNode = previousDominantShape.nodes.some((index) =>
      index >= 0 &&
      index < s.spheres.length &&
      s.spheres[index]?.alive !== false &&
      (s.spheres[index]?.networkDisabledTimer || 0) > 0
    );

    if (previousDominantShape.active === false ||
        (allNodesArePersistentSpheres && currentlyDisabledNode)) {
      preservedDominant = {
        ...previousDominantShape,
        active: false,
        inactiveReason: 'network-disabled',
      };
    }
  }

  const preserveSuppressedWhenUnavailable = Boolean(
    previousDominantShape &&
    previousDominantShape.nodes.length > 0 &&
    previousDominantShape.nodes.every((index) =>
      index >= 0 &&
      index < s.spheres.length &&
      s.spheres[index]?.alive !== false
    ) &&
    previousDominantShape.nodes.some((index) =>
      index >= 0 &&
      index < s.spheres.length &&
      s.spheres[index]?.alive !== false &&
      (s.spheres[index]?.networkDisabledTimer || 0) > 0
    )
  );

  const network = analyzeSphereNetwork(
    getNetworkNodes(s),
    220,
    previousDominant,
    s.networkFormationSelection,
    preservedDominant,
    preserveSuppressedWhenUnavailable,
  );

  // Do not rewrite the selected slot order merely because the Dominant is
  // temporarily unavailable. Recovery restores the same Dominant identity.
  s.networkFrame = { frameId: s.networkFrameId, network };
  return network;
}
