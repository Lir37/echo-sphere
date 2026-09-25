import { analyzeSphereNetwork, type NetworkNode, type NetworkShape, type SphereNetworkState } from './network.ts';

export interface PreviewSphere extends NetworkNode {
  pos: { x: number; y: number };
  alive: boolean;
  networkDisabledTimer?: number;
}

export interface GhostSnapPreview {
  candidate: { x: number; y: number };
  candidateIndex: number;
  network: SphereNetworkState;
  nodes: NetworkNode[];
  linkedNodeIndexes: number[];
  formation: NetworkShape | null;
}

export function getGhostSnapFormation(network: SphereNetworkState): NetworkShape | null {
  return (
    network.fractal ??
    network.lattice ??
    network.ring ??
    network.square ??
    network.triangle ??
    network.cluster ??
    network.line ??
    null
  );
}

/**
 * Project a new Sphere placement or an existing Sphere reposition without
 * mutating live state. Node indexes deliberately stay aligned with the
 * caller's sphere array so the preview can be consumed directly by UI.
 */
export function buildGhostSnapPreview(
  spheres: PreviewSphere[],
  candidate: { x: number; y: number },
  linkDistance = 220,
  movingSphereIndex?: number,
): GhostSnapPreview | null {
  if (spheres.length === 0) return null;

  const moving =
    movingSphereIndex !== undefined &&
    Number.isInteger(movingSphereIndex) &&
    movingSphereIndex >= 0 &&
    movingSphereIndex < spheres.length
      ? movingSphereIndex
      : undefined;

  const nodes: NetworkNode[] = spheres.map((sphere, index) => ({
    pos: index === moving ? { ...candidate } : { ...sphere.pos },
    alive: sphere.alive && (sphere.networkDisabledTimer || 0) <= 0,
    networkDisabledTimer: 0,
  }));

  const candidateIndex = moving ?? nodes.length;
  if (moving === undefined) {
    nodes.push({ pos: { ...candidate }, alive: true, networkDisabledTimer: 0 });
  } else if (nodes[candidateIndex]) {
    // A Sphere being repositioned is the candidate itself. Keep its existing
    // Network participation state while previewing its new coordinates.
    nodes[candidateIndex] = {
      pos: { ...candidate },
      alive: spheres[candidateIndex].alive && (spheres[candidateIndex].networkDisabledTimer || 0) <= 0,
      networkDisabledTimer: 0,
    };
  }

  const network = analyzeSphereNetwork(nodes, linkDistance);
  const linkedNodeIndexes = network.links
    .filter((link) => link.a === candidateIndex || link.b === candidateIndex)
    .map((link) => link.a === candidateIndex ? link.b : link.a);

  const formation = getGhostSnapFormation(network);

  return {
    candidate: { ...candidate },
    candidateIndex,
    network,
    nodes,
    linkedNodeIndexes,
    formation,
  };
}
