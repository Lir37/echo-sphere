# ECHO SPHERE — regional combat and sphere rendering correction, 2026-11-03

Baseline: unity-migration d0516a9be01aac7cf270a2600a1b628f76002c1b.

User observations: fleet ships ram, region-one design changed without request, attacks are inconspicuous, placing a sphere slows combat, player-centred arena glow unwanted.

Implemented:
- Fleet profiles now give every regular hull an onboard weapon: short-range scouts, orbiting fighters, holding heavy hulls/snipers and spread volleys. Ranges 60–105 source pixels remain inside base standard sphere range. Separation cannot push a holding fleet ship toward the player; no retreat is introduced.
- Region-one bodies remain on the existing organic renderer. Removed generic added arms/emitter/ray overlay from region one. No new redesign of those enemies.
- Brood corruptors launch visible webs that settle into a bounded single-hit trap; spores retain their existing corrosive patch. Web drawing has six radial strands and concentric rings. Crystal regular shooters use three fragments; energy/void shooters retain distinct paired projectiles. Preparation for the changed ranged attacks is 0.5 seconds (fleet sniper 0.65); melee roles outside the fleet remain melee. Existing attack poses and muzzle feedback remain active.
- Removed the player-centred full-arena gradient. World-fixed nebulae, stars and region geometry remain.
- Standard sphere core no longer tessellates its pulsating disc each frame: a retained detailed mesh and shader coordinate scale preserve the authored gradient.
- Standard-family ring/arc Gaussian glow is prepared once per retained rig at full world density; rotation/translation/opacity reuse the premultiplied texture. Lost RT is regenerated; rig owns and releases the surface, textures and mesh. No MSAA/resolution/effect-count reduction.

Validation:
- Core contracts: 259533 assertions PASS. Node: 272 tests PASS. Unity preflight and visual provenance PASS.
- Unity compile and clean Play Console: 0 errors, 0 warnings after fresh Play start.
- Actual regional movement/snapshot checks: 159 PASS; actual attacks: 73 PASS including seven ordinary fleet roles firing and web settling; independent projectile flight: 35 PASS; faction render fixture 96 samples PASS.
- Retained sphere GPU check: 120 animated frames, zero repeated sphere shadow passes; comparison with old ring path mean RGB difference 0.150081/255 across the full image, max per-pixel RGB-sum difference 135. This is approximate visual equivalence, not bitwise identity. Local screenshots inspected.
- Release APK local BuildPipeline Succeeded; IL2CPP; regions open through ECHO_REGION_TEST_ACCESS, save unlock flags unchanged.
- APK: UnityProject/Builds/Android/ECHO-SPHERE-regional-combat-fix.apk, 57243546 bytes, SHA256 8f3a4bc08f46e5d18d4fb277daaab4b3567b4ba858bb3c51e9f3c0cdb4e29ad4.
- adb install -r: Success. Device profile freshly backed up privately before interaction. New APK gameplay/performance acceptance pending screen unlock. No FPS claim for this correction yet.

Remaining: confirm fleet shooting and traps in natural phone combat; measure sphere-placement cost in matched scene; confirm removal of requested glow and pause/HUD re-entry. Do not replace future player progress using an older profile backup.

## Follow-up — network cost confirmed after device check
The first Release correction still measured 27.599 FPS in a short region-one combat sample with five placed spheres (7.826 s, p95 49.722 ms, p99 65.318 ms). This is not a final performance pass.

Code inspection found three complete VectorGraphics tessellations of each 25-point animated network link per frame. The sine, 25 samples, three stroke widths, colors, opacity and travelling packets remain unchanged. Retained dynamic ribbons now update vertices directly, with joined nonoverlapping strips and 32-step round end caps. Generic formation contours retain their existing renderer. Ribbon and generic vector meshes have separate ownership to avoid stale tessellation-cache inputs.

Actual Editor comparison of six animated active/inactive link states: worst full-image RGB mean difference 0.000428/255. Warm 200-call CPU sample per link, same Editor fixture: 3.497986 ms vector versus 0.043893 ms ribbon. These are Editor submission costs, not phone frame times. New Release build/device validation follows.

## Device Profiler — retained UI hierarchy invalidated every frame
Actual Xiaomi Development player connected through adb-forwarded Unity Profiler. A warmed active-combat subset (48 frames, excluding loading frames) showed UIElementsRepaintPanels 44.418 ms/frame, Native run panel PrepareRepaint 43.936 ms/frame, RenderTree.UpdateVisuals 25.821 ms/frame. Development/profiling overhead is substantial; these numbers must not be compared directly with Release FPS.

A concrete cause was verified in the installed Unity 6000.6.1f1 implementation: VisualElement.PlaceInFront delegates to Hierarchy.MoveChildElement without checking whether the requested order already holds. MoveChildElement removes/reinserts the child and increments the hierarchy version unconditionally. Runtime called this every frame for the mobile layout and HUD, and repeatedly for pause/modal content. This invalidated retained UI rendering across broad subtrees despite unchanged layout.

NativeUiOrder now checks the actual sibling index before reordering; repeated BringToFront on bound modal views is similarly guarded. Semantics checked for all 30 distinct pairs among six siblings. Attached runtime-panel test: 100 repeated old calls increment hierarchyVersion 100 times; 100 guarded calls increment it zero times. All 31 checks PASS. No layouts, visual layers, filters or interaction targets are removed. New Release/device verification follows.

The earlier ribbon Release alone did not improve phone frame times (short five-sphere continuation 26.847 FPS, p95 49.836 ms). Performance completion remains open until the hierarchy fix is measured.

## Resumed phone session
The later resumed device session had a different current profile (saved at0:41, four placed spheres). A fresh private backup was made before testing. Never restore the earlier0:24 or0:53 archives over this newer player state.

Release with hierarchy guards installed successfully: APK57242162 bytes, SHA25666aa15c46a71c42e27f5bb98f5e6b932aa75ae5e71b63d288cac46bcd8a20714. Short natural combat on the newer four-sphere state:26.737 FPS over10.024s, p95 49.716ms, p99 66.205ms. It is a different saved state; no matched before/after FPS improvement is claimed. More attribution is needed.

Added five presentation-scope ProfilerMarker groups under UNITY_INCLUDE_INSTRUMENTATION only (standard/family/enemy/player/network). They are excluded from normal Release. A temporary ARM64 diagnostic build restores the user's target architectures in finally; normal Release remains dual-ABI. The remaining upgrade-modal order guard uses the same already checked helper.
