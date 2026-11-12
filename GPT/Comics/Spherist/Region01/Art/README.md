# Spherist / Resonance Basin: fixed-source compositing pilot

This is a **visual proof of the approved layered workflow**, not yet a Unity-ready full comic and not a replacement for the 16-page production bible.

- `S0_p01.svg`: independently rendered 3-panel page, from **original exact source bitmaps** in the game embedded directly in the image. The Spherist PNG is NOT regenerated and retains pixel identity; the backdrop is the exact Region 1 JPG, with a comic-book color/stipple overlay.
- `S0_p01.slots.json`: overlay-only RU/EN dialogue, pixel rectangles. Words do NOT appear inside the art.
- No new enemy, weapon, boss, spell, or gameplay logic has been added.
- The relay is taken from the real Region 1 architecture; narrative line glow is an illustration, **not a new game mechanic**.
- To see the artwork, open the SVG in a browser or an SVG-compatible image viewer. Unity integration / PNG conversion is a later validation step.
- This deliberately validates only the first page (S0-p01) without enemy composites. For S0-p02, official enemy source JPGs will be used without inventing alternative bodies.

**Reference checks:** source branch `unity-migration`, sprite `UnityProject/Assets/Resources/EchoSphere/CharacterArt/spherist.png`, environment `UnityProject/Assets/Resources/EchoSphere/RegionBackdrops/resonance_basin.jpg`, story `GPT/Comics/Spherist/REGION_01_RESONANCE_BASIN_PRODUCTION_BRIEF_v1.0.md`.

- `S0_p02.svg`: second pilot page using original Archive illustrations of `defeat-enemy-0-wisp` and `defeat-enemy-0-beetle` as image layers (with feathered edges), plus original Spherist core PNG and source `sphere-standard` JPG. No AI-redrawn monsters. `S0_p02.slots.json` contains the only thought overlay.
- **Visual-quality limitation:** cutouts are feathered archive photograph/illustration layers, not yet individually hand-redrawn comic silhouettes. If visual montage seams are obvious or the panels are unclear, this is a compositing test, not production acceptance.
