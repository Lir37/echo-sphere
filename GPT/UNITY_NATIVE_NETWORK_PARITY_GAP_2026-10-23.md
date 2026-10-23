# Native network parity gap — 2026-10-23

Status: OPEN. No Core gameplay formulas changed in this investigation.

A direct substitution of the translated network analyzer with existing
SphereNetworkRules.Analyze was tried locally and reverted before commit because
it changed the UI-consumed formation topology. The strict Ghost Snap test remains
unchanged; a weakened numerical-only comparison was not retained.

Reproduction: four source-pixel nodes
`(cos(i*1.17)*140, sin(i*1.17)*140)` for i=0..3, plus candidate `(43,-28)`;
link distance 220 pixels. Original src/network.ts executed with Node selects
Line nodes [0,3,4], secondary Triangle [2,3,4]. Existing native Core after its
normal world-unit conversion selects Fractal over all five nodes.

Confirmed source differences to resolve before unifying analyzers:
- Source rankGeometryCandidates excludes Fractal with fewer than six nodes;
  native RankCandidates currently has no equivalent filter.
- Native distance tolerances still use the earlier 0.01 units/pixel scale:
  line error 0.72 vs source 72 pixels; triangle minimum 0.65 vs 65 pixels;
  square minimum 0.60 vs 60 pixels; ring minimum 0.55 vs 55 pixels;
  cluster compactness 0.90 vs 90 pixels. Current camera calibration is
  SpherePlacementRules.SourcePixelsToWorld = 12.6/720 = 0.0175.
- Square diagonal denominator also has a world-distance floor 1 instead of
  source one pixel. Fractal node ordering is explicitly sorted in Core while
  original source retains ordered Set insertion.

Do not silently enable the attempted analyzer substitution or call it source
parity. Any correction must be based on original source fixtures and preserve
source formulas, not invent new balance. Existing native gameplay network stayed
unchanged during this cycle. World drawing and orbital bonuses can safely read
its cached Core state because that is exactly what their previous V snapshot
represented; that separate projection parity passed.
