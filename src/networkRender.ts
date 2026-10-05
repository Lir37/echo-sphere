import type { NetworkShape, SphereNetworkState } from './network';

function filterShapeToSpheres(shape: NetworkShape | null, sphereCount: number): NetworkShape | null {
  if (!shape) return null;
  const nodes = shape.nodes.filter((index) => index >= 0 && index < sphereCount);
  return nodes.length >= 2 ? { ...shape, nodes } : null;
}

/**
 * Renderer-facing Network projection.
 * Gameplay may contain runtime nodes such as Echo Drones, whose indexes are
 * intentionally outside the Sphere array. Keep those links out of geometry
 * routines that read Sphere-specific state, while returning them separately
 * for generic position-based rendering.
 */
export function getSphereVisualNetwork(
  source: SphereNetworkState,
  sphereCount: number,
): { network: SphereNetworkState; externalLinks: SphereNetworkState['links'] } {
  const isSphereLink = (link: SphereNetworkState['links'][number]) =>
    link.a >= 0 && link.a < sphereCount && link.b >= 0 && link.b < sphereCount;

  const filterCandidates = source.formationCandidates
    .map((shape) => filterShapeToSpheres(shape, sphereCount))
    .filter((shape): shape is NetworkShape => Boolean(shape));

  return {
    network: {
      ...source,
      nodes: source.nodes.filter((index) => index >= 0 && index < sphereCount),
      links: source.links.filter(isSphereLink),
      formationCandidates: filterCandidates,
      dominantFormation: filterShapeToSpheres(source.dominantFormation, sphereCount),
      secondaryFormation: filterShapeToSpheres(source.secondaryFormation, sphereCount),
      line: filterShapeToSpheres(source.line, sphereCount),
      triangle: filterShapeToSpheres(source.triangle, sphereCount),
      square: filterShapeToSpheres(source.square, sphereCount),
      cluster: filterShapeToSpheres(source.cluster, sphereCount),
      ring: filterShapeToSpheres(source.ring, sphereCount),
      lattice: filterShapeToSpheres(source.lattice, sphereCount),
      fractal: filterShapeToSpheres(source.fractal, sphereCount),
    },
    externalLinks: source.links.filter((link) => !isSphereLink(link)),
  };
}
