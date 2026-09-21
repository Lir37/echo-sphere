# Authored 3D source assets

This directory is for user-supplied production/reference GLBs that must survive the
offline asset generator.

For ECHO SPHERE v4.0, place the raw Spherist model here as:

scripts/source_assets/player_spherist_raw.glb

The production generator copies that file to:

public/art3d/player_spherist.glb

and will not replace it with the procedural Spherist generator.

The raw source should remain a real GLB with authored geometry, UVs and PBR textures.
Do not replace it with a primitive placeholder.
