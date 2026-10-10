ECHO SPHERE / production 2D art import

SOURCE OF TRUTH
All imported art on this folder is mapped from new-desing at source HEAD:
737ef2af4a877e0995db5f92331d536d2f29a934
The upstream Canvas art files are in public/art/. Do not replace them with
earlier 3D experiment art or invented flattened sprites.

CHARACTER
- character-spherist-3q.svg maps from public/art/spherist-hybrid-3q.svg.
- character-spherist-front.svg maps from public/art/spherist-hybrid-front.svg.
- character-spherist-shield.png maps byte-for-byte from public/art/spherist-shield-96.png.
- player.svg is the Core glyph, not the playable Spherist character.

SPHERES
- sphere-aura/chain/gravity/prism/pulse/shotgun/sniper/void.svg map from
  public/art/aura/chain/gravity/prism/pulse/shotgun/sniper/void.svg.
- Standard is intentionally assembled from the original transparent files:
  standard-sphere/energy-core.png, stabilization-ring.png,
  upper-crystal.png and lower-crystal.png.
- The source project does not define an orbital.svg image. Orbital is rendered
  as a 2.5D core, crossing rings, moving terminals and level-dependent effects.

ENEMIES
- enemy-boss.svg maps from public/art/boss.svg. Ordinary enemy creatures are
  procedurally authored in src/enemies/enemyVisual.ts and still require a
  direct Unity 2.5D renderer port; do not invent GLB substitutes.

IMPORT COMPATIBILITY
Unity Vector Graphics (3.0.0-preview.7) cannot import currentColor and does not
support the source SVG glow filters. The SVG copies preserve original paths,
geometry, gradients and explicit authored color; only unsupported filter
definitions/references are removed and inherited currentColor is resolved to
the source group's existing color. No polygon or path was redrawn here.
The importer sets SVG/PNG settings before import, without SaveAndReimport loops.

This asset synchronization is not visual acceptance by itself. Open the project
in the pinned Unity 6.3 editor, inspect the generated scene and built APK.
