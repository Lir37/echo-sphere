import { analyzeSphereNetwork, type NetworkNode, type SphereNetworkState } from './network';

export interface PreviewSphere extends NetworkNode {
  pos: { x: number; y: number };
  alive: boolean;
  networkDisabledTimer?: number;
}

export interface GhostSnapPreview {
  candidate: { x: number; y: number };
  candidateIndex: number;
  network: SphereNetworkState;
  linkedNodeIndexes: number[];
  formation: string | null;
}

export function buildGhostSnapPreview(
  spheres: PreviewSphere[],
  candidate: { x: number; y: number },
  linkDistance = 220,
): GhostSnapPreview | null {
  const active = spheres.filter((sphere) => sphere.alive && (sphere.networkDisabledTimer || 0) <= 0);
  if (active.length === 0) return null;

  const nodes: NetworkNode[] = active.map((sphere) => ({
    pos: { ...sphere.pos },
    alive: true,
    networkDisabledTimer: 0,
  }));
  const candidateIndex = nodes.length;
  nodes.push({ pos: { ...candidate }, alive: true, networkDisabledTimer: 0 });

  const network = analyzeSphereNetwork(nodes, linkDistance);
  const linkedNodeIndexes = network.links
    .filter((link) => link.a === candidateIndex || link.b === candidateIndex)
    .map((link) => link.a === candidateIndex ? link.b : link.a);

  const formation =
    network.fractal?.type ??
    network.lattice?.type ??
    network.ring?.type ??
    network.square?.type ??
    network.triangle?.type ??
    network.cluster?.type ??
    network.line?.type ??
    null;

  return { candidate: { ...candidate }, candidateIndex, network, linkedNodeIndexes, formation };
}
