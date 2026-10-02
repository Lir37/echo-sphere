import type { NetworkNode } from './network';

export interface RuntimeNetworkMinion {
  pos: { x: number; y: number };
  life: number;
}

export function buildRuntimeNetworkNodes(
  spheres: NetworkNode[],
  minions: RuntimeNetworkMinion[],
  includeMinions: boolean,
): NetworkNode[] {
  const nodes = spheres.map((sphere) => ({
    pos: sphere.pos,
    alive: sphere.alive !== false && (sphere.networkDisabledTimer || 0) <= 0,
  }));

  if (!includeMinions) return nodes;

  for (const minion of minions) {
    nodes.push({
      pos: minion.pos,
      alive: minion.life > 0,
    });
  }

  return nodes;
}
