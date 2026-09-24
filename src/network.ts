export interface NetworkPosition {
  x: number;
  y: number;
}

export interface NetworkNode {
  pos: NetworkPosition;
  alive?: boolean;
}

export type NetworkFormation = 'none' | 'line' | 'triangle' | 'square' | 'cluster';

export interface NetworkLink {
  a: number;
  b: number;
  distance: number;
}

export interface NetworkShape {
  type: Exclude<NetworkFormation, 'none'>;
  strength: number;
  nodes: number[];
}

export interface SphereNetworkState {
  linkDistance: number;
  nodes: number[];
  links: NetworkLink[];
  line: NetworkShape | null;
  triangle: NetworkShape | null;
  square: NetworkShape | null;
  cluster: NetworkShape | null;
}

export interface SphereNetworkProfile {
  linkedNeighbours: number;
  line: boolean;
  triangle: boolean;
  square: boolean;
  cluster: boolean;
}

const DEFAULT_LINK_DISTANCE = 220;

function distance(a: NetworkPosition, b: NetworkPosition): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function aliveIndexes(nodes: NetworkNode[]): number[] {
  const result: number[] = [];
  for (let i = 0; i < nodes.length; i++) {
    if (nodes[i].alive !== false) result.push(i);
  }
  return result;
}

function combinations3(values: number[]): Array<[number, number, number]> {
  const result: Array<[number, number, number]> = [];
  for (let i = 0; i < values.length - 2; i++) {
    for (let j = i + 1; j < values.length - 1; j++) {
      for (let k = j + 1; k < values.length; k++) {
        result.push([values[i], values[j], values[k]]);
      }
    }
  }
  return result;
}

function lineStrength(nodes: NetworkNode[], indexes: number[]): number {
  if (indexes.length < 3) return 0;

  let best = 0;
  const centroid = indexes.reduce(
    (acc, index) => ({ x: acc.x + nodes[index].pos.x, y: acc.y + nodes[index].pos.y }),
    { x: 0, y: 0 },
  );
  centroid.x /= indexes.length;
  centroid.y /= indexes.length;

  for (let step = 0; step < 24; step++) {
    const angle = (step / 24) * Math.PI;
    const dirX = Math.cos(angle);
    const dirY = Math.sin(angle);
    let maxError = 0;
    for (const index of indexes) {
      const dx = nodes[index].pos.x - centroid.x;
      const dy = nodes[index].pos.y - centroid.y;
      maxError = Math.max(maxError, Math.abs(dx * dirY - dy * dirX));
    }
    const strength = 1 - Math.min(1, maxError / 72);
    best = Math.max(best, strength);
  }

  return best;
}

function triangleStrength(nodes: NetworkNode[], indexes: number[], linkDistance: number): number {
  if (indexes.length < 3) return 0;
  const [a, b, c] = indexes;
  const ab = distance(nodes[a].pos, nodes[b].pos);
  const bc = distance(nodes[b].pos, nodes[c].pos);
  const ca = distance(nodes[c].pos, nodes[a].pos);
  if (ab > linkDistance || bc > linkDistance || ca > linkDistance) return 0;

  const mean = (ab + bc + ca) / 3;
  if (mean < 65 || mean > linkDistance) return 0;

  const variance = (Math.abs(ab - mean) + Math.abs(bc - mean) + Math.abs(ca - mean)) / (3 * mean);
  return 1 - Math.min(1, variance * 2.2);
}


function squareStrength(nodes: NetworkNode[], indexes: number[], linkDistance: number): number {
  if (indexes.length !== 4) return 0;
  const points = indexes.map((index) => ({ index, pos: nodes[index].pos }));
  const center = points.reduce((acc, p) => ({ x: acc.x + p.pos.x, y: acc.y + p.pos.y }), { x: 0, y: 0 });
  center.x /= 4; center.y /= 4;
  const ordered = [...points].sort((a, b) => Math.atan2(a.pos.y - center.y, a.pos.x - center.x) - Math.atan2(b.pos.y - center.y, b.pos.x - center.x));
  const sides = [0, 1, 2, 3].map((i) => distance(ordered[i].pos, ordered[(i + 1) % 4].pos));
  const diagonals = [distance(ordered[0].pos, ordered[2].pos), distance(ordered[1].pos, ordered[3].pos)];
  const sideMean = sides.reduce((a, b) => a + b, 0) / 4;
  if (sideMean < 60 || sideMean > linkDistance || sides.some((side) => side > linkDistance)) return 0;
  const sideVariance = sides.reduce((sum, side) => sum + Math.abs(side - sideMean), 0) / (4 * sideMean);
  const diagonalMean = (diagonals[0] + diagonals[1]) / 2;
  const diagonalVariance = Math.abs(diagonals[0] - diagonals[1]) / Math.max(1, diagonalMean);
  const rightAngleError = Math.abs(diagonalMean / sideMean - Math.SQRT2) / Math.SQRT2;
  return Math.max(0, 1 - Math.min(1, sideVariance * 2 + diagonalVariance + rightAngleError));
}

function clusterStrength(nodes: NetworkNode[], indexes: number[], linkDistance: number): number {
  if (indexes.length < 4) return 0;

  let sum = 0;
  let pairs = 0;
  let linkedPairs = 0;
  for (let i = 0; i < indexes.length; i++) {
    for (let j = i + 1; j < indexes.length; j++) {
      const d = distance(nodes[indexes[i]].pos, nodes[indexes[j]].pos);
      sum += d;
      pairs++;
      if (d <= linkDistance) linkedPairs++;
    }
  }

  const average = sum / Math.max(1, pairs);
  const compactness = 1 - Math.min(1, Math.max(0, average - 90) / 90);
  const connectivity = linkedPairs / Math.max(1, pairs);
  return compactness * 0.65 + connectivity * 0.35;
}

export function analyzeSphereNetwork(
  nodes: NetworkNode[],
  linkDistance = DEFAULT_LINK_DISTANCE,
): SphereNetworkState {
  const indexes = aliveIndexes(nodes);
  const links: NetworkLink[] = [];

  for (let i = 0; i < indexes.length; i++) {
    for (let j = i + 1; j < indexes.length; j++) {
      const a = indexes[i];
      const b = indexes[j];
      const d = distance(nodes[a].pos, nodes[b].pos);
      if (d <= linkDistance) links.push({ a, b, distance: d });
    }
  }

  const line = indexes.length >= 3
    ? (() => {
        const strength = lineStrength(nodes, indexes);
        return strength >= 0.8
          ? { type: 'line' as const, strength, nodes: [...indexes] }
          : null;
      })()
    : null;

  let triangle: NetworkShape | null = null;
  for (const combo of combinations3(indexes)) {
    const strength = triangleStrength(nodes, combo, linkDistance);
    if (strength < 0.82) continue;
    if (!triangle || strength > triangle.strength) {
      triangle = { type: 'triangle', strength, nodes: [...combo] };
    }
  }

  const square = (() => {
    let best: NetworkShape | null = null;
    for (let i = 0; i < indexes.length - 3; i++) for (let j = i + 1; j < indexes.length - 2; j++) for (let k = j + 1; k < indexes.length - 1; k++) for (let l = k + 1; l < indexes.length; l++) {
      const combo = [indexes[i], indexes[j], indexes[k], indexes[l]];
      const strength = squareStrength(nodes, combo, linkDistance);
      if (strength >= 0.84 && (!best || strength > best.strength)) best = { type: 'square', strength, nodes: combo };
    }
    return best;
  })();

  const cluster = (() => {
    const strength = clusterStrength(nodes, indexes, linkDistance);
    return strength >= 0.78 ? { type: 'cluster' as const, strength, nodes: [...indexes] } : null;
  })();

  // Readability priority: Square > Triangle > Cluster > Line when formations overlap.
  const resolvedTriangle = square ? null : triangle;
  const resolvedCluster = square || triangle ? null : cluster;
  const resolvedLine = square || triangle || cluster ? null : line;

  return {
    linkDistance: linkDistance,
    nodes: indexes,
    links,
    line: resolvedLine,
    triangle: resolvedTriangle,
    square,
    cluster: resolvedCluster,
  };
}

export function getSphereNetworkProfile(
  state: SphereNetworkState,
  sphereIndex: number,
): SphereNetworkProfile {
  let linkedNeighbours = 0;
  for (const link of state.links) {
    if (link.a === sphereIndex || link.b === sphereIndex) linkedNeighbours++;
  }

  return {
    linkedNeighbours,
    line: Boolean(state.line?.nodes.includes(sphereIndex)),
    triangle: Boolean(state.triangle?.nodes.includes(sphereIndex)),
    square: Boolean(state.square?.nodes.includes(sphereIndex)),
    cluster: Boolean(state.cluster?.nodes.includes(sphereIndex)),
  };
}
