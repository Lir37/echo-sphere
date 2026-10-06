export interface NetworkPosition {
  x: number;
  y: number;
}

export interface NetworkNode {
  pos: NetworkPosition;
  alive?: boolean;
  networkDisabledTimer?: number;
}

export type NetworkFormation = 'none' | 'line' | 'triangle' | 'cluster' | 'square' | 'ring' | 'lattice' | 'fractal';

export interface NetworkLink {
  a: number;
  b: number;
  distance: number;
}

export interface NetworkShape {
  type: Exclude<NetworkFormation, 'none'>;
  strength: number;
  nodes: number[];
  /**
   * Deterministic geometry score used to decide which valid formation is
   * dominant. It is intentionally only a small refinement over raw shape
   * strength so a precise lower-order form can beat a sloppy higher-order one.
   */
  dominanceScore?: number;
  /** Temporarily disrupted formations retain slot identity while inactive. */
  active?: boolean;
  inactiveReason?: 'network-disabled';
}

export interface NetworkFormationSelection {
  dominant: NetworkFormation;
  secondary: NetworkFormation;
}

export interface SphereNetworkState {
  linkDistance: number;
  nodes: number[];
  links: NetworkLink[];
  /** All currently valid formations, before the two active combat layers are selected. */
  formationCandidates: NetworkShape[];
  /** Primary formation. Its Resonance event is the dominant Network event. */
  dominantFormation: NetworkShape | null;
  /** Optional secondary formation. Its local mechanics remain active, but it does not own Resonance. */
  secondaryFormation: NetworkShape | null;
  line: NetworkShape | null;
  triangle: NetworkShape | null;
  square: NetworkShape | null;
  cluster: NetworkShape | null;
  ring: NetworkShape | null;
  lattice: NetworkShape | null;
  fractal: NetworkShape | null;
  formationEfficiency?: number;
}

export interface SphereNetworkProfile {
  linkedNeighbours: number;
  line: boolean;
  triangle: boolean;
  square: boolean;
  cluster: boolean;
  ring: boolean;
  lattice: boolean;
  fractal: boolean;
}

const SECONDARY_SCORE_GAP = 14;
const SECONDARY_DISJOINT_SCORE_GAP = 18;
const DOMINANCE_SWITCH_MARGIN = 10;

const GEOMETRY_SPECIFICITY: Record<Exclude<NetworkFormation, 'none'>, number> = {
  line: 0,
  triangle: 1,
  cluster: 2,
  square: 5,
  ring: 3,
  lattice: 5,
  fractal: 7,
};

function getGeometryDominanceScore(shape: NetworkShape): number {
  const sizeBonus = Math.min(6, Math.max(0, shape.nodes.length - 3) * 1.5);
  return shape.strength * 100 + GEOMETRY_SPECIFICITY[shape.type] + sizeBonus;
}

function rankGeometryCandidates(
  candidates: Partial<Record<Exclude<NetworkFormation, 'none'>, NetworkShape | null>>,
  previousDominant: NetworkFormation = 'none',
): NetworkShape[] {
  const filtered = (Object.values(candidates) as Array<NetworkShape | null>)
    .filter((shape): shape is NetworkShape => Boolean(shape))
    .filter((shape) => shape.type !== 'fractal' || shape.nodes.length >= 6)
    .map((shape) => ({ ...shape, dominanceScore: getGeometryDominanceScore(shape) }))
    .sort((a, b) =>
      (b.dominanceScore || 0) - (a.dominanceScore || 0) ||
      b.strength - a.strength ||
      b.nodes.length - a.nodes.length ||
      a.type.localeCompare(b.type),
    );

  if (previousDominant === 'none') return filtered;

  const previous = filtered.find((shape) => shape.type === previousDominant);
  const top = filtered[0];
  if (!previous || !top || previous.type === top.type) return filtered;

  // Formation inertia: a small geometric fluctuation must not steal the
  // Network's identity. The player changes dominance by creating a real
  // structural advantage, or by breaking the current formation entirely.
  if ((top.dominanceScore || 0) < (previous.dominanceScore || 0) + DOMINANCE_SWITCH_MARGIN) {
    return [previous, ...filtered.filter((shape) => shape.type !== previous.type)];
  }

  return filtered;
}

function chooseSecondaryFormation(
  ranked: NetworkShape[],
  dominant: NetworkShape,
  selection?: NetworkFormationSelection | null,
): NetworkShape | null {
  const selectedSecondary = selection?.secondary && selection.secondary !== 'none' && selection.secondary !== dominant.type
    ? ranked.find((shape) => shape.type === selection.secondary) || null
    : null;
  if (selectedSecondary) return selectedSecondary;

  const dominantNodes = new Set(dominant.nodes);
  const scoreGap = (shape: NetworkShape): number =>
    (dominant.dominanceScore || 0) - (shape.dominanceScore || 0);

  const disjointCandidate = ranked.find((shape) =>
    shape.type !== dominant.type &&
    scoreGap(shape) <= SECONDARY_DISJOINT_SCORE_GAP &&
    shape.nodes.every((node) => !dominantNodes.has(node)),
  );

  const overlappingCandidate = ranked.find((shape) =>
    shape.type !== dominant.type &&
    scoreGap(shape) <= SECONDARY_SCORE_GAP,
  );

  return disjointCandidate || overlappingCandidate || null;
}

function selectActiveGeometry(
  ranked: NetworkShape[],
  selection?: NetworkFormationSelection | null,
  preservedDominant?: NetworkShape | null,
  preserveSuppressedWhenUnavailable = false,
): { dominant: NetworkShape | null; secondary: NetworkShape | null } {
  const selectedDominant = selection?.dominant && selection.dominant !== 'none'
    ? ranked.find((shape) => shape.type === selection.dominant) || null
    : null;

  const rememberedSuppressed = preservedDominant?.active === false &&
    preservedDominant.inactiveReason === 'network-disabled'
    ? preservedDominant
    : null;

  if (rememberedSuppressed && !selectedDominant &&
      (!selection?.dominant || selection.dominant === rememberedSuppressed.type)) {
    const restored = ranked.find((shape) => shape.type === rememberedSuppressed.type);
    if (restored) {
      return {
        dominant: restored,
        secondary: chooseSecondaryFormation(ranked, restored, selection),
      };
    }

    // Do not keep stale Dominance memory after a structural loss that is no
    // longer backed by an active temporary Network disable.
    if (!preserveSuppressedWhenUnavailable) return {
      dominant: selectedDominant || ranked[0] || null,
      secondary: selectedDominant
        ? chooseSecondaryFormation(ranked, selectedDominant, selection)
        : (ranked[1] || null),
    };

    const inactiveDominant: NetworkShape = {
      ...rememberedSuppressed,
      active: false,
      inactiveReason: 'network-disabled',
    };
    return {
      dominant: inactiveDominant,
      secondary: ranked.find((shape) => shape.type !== inactiveDominant.type) || null,
    };
  }

  const dominant = selectedDominant || ranked[0] || null;
  if (!dominant) return { dominant: null, secondary: null };

  return {
    dominant,
    secondary: chooseSecondaryFormation(ranked, dominant, selection),
  };
}

export function getFormationBonusMultiplier(
  network: SphereNetworkState,
  type: Exclude<NetworkFormation, 'none'>,
  nodeIndex?: number,
): number {
  const includesNode = (shape: NetworkShape | null): boolean =>
    Boolean(shape && (nodeIndex === undefined || nodeIndex < 0 || shape.nodes.includes(nodeIndex)));
  const efficiency = Math.max(0, Math.min(1, network.formationEfficiency ?? 1));
  if (network.dominantFormation?.active !== false && network.dominantFormation?.type === type && includesNode(network.dominantFormation)) return efficiency;
  if (network.secondaryFormation?.type === type && includesNode(network.secondaryFormation)) return 0.5 * efficiency;
  return 0;
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


function ringShape(nodes: NetworkNode[], indexes: number[], linkDistance: number): NetworkShape | null {
  if (indexes.length < 4) return null;
  const center = indexes.reduce((acc, index) => ({ x: acc.x + nodes[index].pos.x, y: acc.y + nodes[index].pos.y }), { x: 0, y: 0 });
  center.x /= indexes.length;
  center.y /= indexes.length;
  const ordered = [...indexes].sort((a, b) => Math.atan2(nodes[a].pos.y - center.y, nodes[a].pos.x - center.x) - Math.atan2(nodes[b].pos.y - center.y, nodes[b].pos.x - center.x));
  const distances: number[] = [];
  for (let i = 0; i < ordered.length; i++) {
    const a = ordered[i];
    const b = ordered[(i + 1) % ordered.length];
    const d = distance(nodes[a].pos, nodes[b].pos);
    if (d > linkDistance) return null;
    distances.push(d);
  }
  const mean = distances.reduce((a, b) => a + b, 0) / distances.length;
  if (mean < 55) return null;
  const variance = distances.reduce((sum, d) => sum + Math.abs(d - mean), 0) / (distances.length * mean);
  const strength = 1 - Math.min(1, variance * 2);
  return strength >= 0.68 ? { type: 'ring', strength, nodes: ordered } : null;
}

function isNodeSetConnected(indexes: number[], links: NetworkLink[]): boolean {
  if (indexes.length <= 1) return true;
  const allowed = new Set(indexes);
  const visited = new Set<number>([indexes[0]]);
  const queue = [indexes[0]];
  while (queue.length > 0) {
    const current = queue.shift()!;
    for (const link of links) {
      const next = link.a === current ? link.b : link.b === current ? link.a : -1;
      if (next >= 0 && allowed.has(next) && !visited.has(next)) {
        visited.add(next);
        queue.push(next);
      }
    }
  }
  return visited.size === allowed.size;
}

function latticeShape(nodes: NetworkNode[], indexes: number[], links: NetworkLink[], linkDistance: number): NetworkShape | null {
  if (indexes.length < 4) return null;
  const triangles: number[][] = [];
  const linked = (a: number, b: number) => links.some((link) => (link.a === a && link.b === b) || (link.a === b && link.b === a));
  for (let i = 0; i < indexes.length - 2; i++) for (let j = i + 1; j < indexes.length - 1; j++) for (let k = j + 1; k < indexes.length; k++) {
    const combo = [indexes[i], indexes[j], indexes[k]];
    if (linked(combo[0], combo[1]) && linked(combo[1], combo[2]) && linked(combo[0], combo[2]) && triangleStrength(nodes, combo, linkDistance) >= 0.82) triangles.push(combo);
  }
  if (triangles.length < 2) return null;
  let shared = false;
  outer: for (let i = 0; i < triangles.length; i++) for (let j = i + 1; j < triangles.length; j++) {
    const common = triangles[i].filter((value) => triangles[j].includes(value));
    if (common.length >= 2) { shared = true; break outer; }
  }
  if (!shared) return null;
  const strength = Math.min(1, 0.55 + Math.min(0.45, triangles.length * 0.08));
  const used = [...new Set(triangles.flat())];
  return { type: 'lattice', strength, nodes: used };
}

export function analyzeSphereNetwork(
  nodes: NetworkNode[],
  linkDistance = DEFAULT_LINK_DISTANCE,
  previousDominant: NetworkFormation = 'none',
  selection?: NetworkFormationSelection | null,
  preservedDominant?: NetworkShape | null,
  preserveSuppressedWhenUnavailable = false,
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
        const wholeStrength = lineStrength(nodes, indexes);
        if (wholeStrength >= 0.8) {
          return { type: 'line' as const, strength: wholeStrength, nodes: [...indexes] };
        }
        let best: NetworkShape | null = null;
        for (const combo of combinations3(indexes)) {
          const strength = lineStrength(nodes, combo);
          if (strength >= 0.8 && (!best || strength > best.strength)) {
            best = { type: 'line' as const, strength, nodes: [...combo] };
          }
        }
        return best;
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

  const ring = ringShape(nodes, indexes, linkDistance);
  const lattice = latticeShape(nodes, indexes, links, linkDistance);
  const fractalBase = [ring, lattice, square, triangle].filter(Boolean) as NetworkShape[];
  const fractalNodes = [...new Set(fractalBase.flatMap((shape) => shape.nodes))];
  const fractal = fractalBase.length >= 2 && isNodeSetConnected(fractalNodes, links)
    ? {
        type: 'fractal' as const,
        strength: Math.min(1, fractalBase.reduce((sum, shape) => sum + shape.strength, 0) / fractalBase.length),
        nodes: fractalNodes,
      }
    : null;

  // Physical Links are infrastructure. Geometry is a higher-level
  // interpretation of that infrastructure. Multiple valid formations may
  // overlap; only the two strongest layers are combat-active. The dominant
  // layer owns the player-level Resonance event.
  const candidates = {
    fractal,
    lattice,
    ring,
    square,
    triangle,
    cluster,
    line,
  };
  const ranked = rankGeometryCandidates(candidates, previousDominant);
  const active = selectActiveGeometry(ranked, selection, preservedDominant, preserveSuppressedWhenUnavailable);
  const activeTypes = new Set(
    [active.dominant, active.secondary]
      .filter((shape): shape is NetworkShape => Boolean(shape))
      .map((shape) => shape.type),
  );

  const byType = (type: Exclude<NetworkFormation, 'none'>): NetworkShape | null =>
    activeTypes.has(type) ? ranked.find((shape) => shape.type === type) || null : null;

  return {
    linkDistance,
    nodes: indexes,
    links,
    formationEfficiency: 1,
    formationCandidates: ranked,
    dominantFormation: active.dominant,
    secondaryFormation: active.secondary,
    line: byType('line'),
    triangle: byType('triangle'),
    square: byType('square'),
    cluster: byType('cluster'),
    ring: byType('ring'),
    lattice: byType('lattice'),
    fractal: byType('fractal'),
  };
}

export function getLinkedNodeIndexes(state: SphereNetworkState, nodeIndex: number): number[] {
  const result: number[] = [];
  for (const link of state.links) {
    if (link.a === nodeIndex) result.push(link.b);
    else if (link.b === nodeIndex) result.push(link.a);
  }
  return result;
}

export function areNetworkNodesLinked(state: SphereNetworkState, a: number, b: number): boolean {
  return state.links.some((link) =>
    (link.a === a && link.b === b) || (link.a === b && link.b === a)
  );
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
    ring: Boolean(state.ring?.nodes.includes(sphereIndex)),
    lattice: Boolean(state.lattice?.nodes.includes(sphereIndex)),
    fractal: Boolean(state.fractal?.nodes.includes(sphereIndex)),
  };
}
