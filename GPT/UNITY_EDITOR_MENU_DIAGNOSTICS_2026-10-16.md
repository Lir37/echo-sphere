# Unity Editor menu investigation — 2026-10-16

Entry HEAD: `6fdce69c6940e638c311c0fb89efc172db11a2e2`, `unity-migration`.
Protected remote source: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`, unchanged.
Editor connection verified through Unity MCP: local `UnityProject`, Unity 6000.3.24f1.
Existing user package/settings/import/scene changes were preserved and excluded from this commit.
Recent chat tools were not exposed in this session. Used the supplied conversation preview,
three authoritative documents and migration/continuation audits; no claim of reading 5–15 chats.

## Confirmed causes and changes

1. `SourcePlayerVisual2D` read `sprite.triangles` inside the index loop, copying the complete
   native array for each index. The front sprite has 1477 vertices / 4206 indices.
   Read the native array once per frame, preserving every index, vertex, UV and effect.
   An Editor microbenchmark measured 38.1049 ms for the old loop and 0.0255 ms for the
   single-read loop, with equal output. This is a microbenchmark, not an FPS comparison.
2. Grid projection ran before the first native Yoga geometry, turning unresolved auto rows
   into explicit zero-height absolute children. Wait for valid native geometry before
   projecting those tracks. No visibility gate or arbitrary frame delay was added.
   Observed `ECHO SPHERE` native box height changed from 0 to 41 px in the landscape check.
3. Intrinsic single-column grids fed previously assigned child heights back into auto sizing.
   The menu difficulty row was 907 px tall; the loadout panel was 421 px.
   A narrowly eligible `1fr` column with auto rows now uses native vertical layout and the
   existing gap projection. Explicit/fractional rows, constrained height, special item
   alignment/placement and z-index use the existing grid adapter. In 480x854 Play Mode,
   the loadout panel measured 92 px and difficulty row 30 px.
4. Backdrop replicas moved every sibling to the end of its hierarchy on each update even
   when the final paint order was unchanged. Retain the expected child position and only
   insert when required. Stale-node removal and paint-prefix traversal remain unchanged.
5. Helper replicas applied 51 resolved appearance properties, immediately overwriting all
   of them from inline styles. Removed the first pass for helpers; ordinary nodes retain it.
   All 89 resulting IStyle properties matched in native Editor comparisons.

## Actual validation

- Unity MCP recompile: completed, no C# compilation errors; repeated Play Mode starts.
- Real Unity diagnostics (`scripts/check-unity-editor-menu.cs`, via MCP `eval_file`):
  1206 native hierarchy order checks, 1780 style comparisons, 7806 imported sprite index
  checks, 14 native-grid eligibility checks: PASS. These are not screenshot parity tests.
- .NET 8 core: 1193 assertions and 47339 TypeScript comparisons: PASS.
- Full portable VisualChecks: PASS (including 2663 command streams, 76 UI trees,
  674640 CSS properties, grid/brush/backdrop/native-layer/event fixtures).
- Node: 272 tests PASS. Project preflight PASS.
- Settings opened via a UI Toolkit ClickEvent and returned via NavigationSubmitEvent;
  screenshots inspected. Physical pointer/touch input was not validated: MCP pointer
  injection requires Input System, which is not installed; no input package was added.
- No new error entries in the final menu/settings interval. Earlier transitions produced
  two `UIElementsRuntimeUtilityNative.RepaintPanels` assertions; full Console cleanliness
  is NOT claimed. The reported unallocated-texture index=3 error was not reproduced.

## Profiling evidence and limitations

Initial warmed landscape capture: 31 frames, 123–185 ms main-thread frame time;
backdrop preparation about 36–45 ms; layout about 16–25 ms. Player visual frequently
cost tens of milliseconds (roughly 34–82 ms across sampled frames).
After the triangle read fix, a 46-frame sample averaged 0.281 ms in player LateUpdate.

Final short capture at **480x854**: 28 sampled game frames; mean frame time 118.312 ms,
backdrop 43.925 ms, layout 18.141 ms, player LateUpdate 0.288 ms,
GC.Alloc metadata total average 5,507,224 bytes/frame on the sampled main thread.
This corresponds to approximately 8.45 frames/s within that capture. Resolution/layout
and sampling conditions differ from the initial recording; do NOT present a controlled
before/after FPS or GC improvement. The old `GC.GetAllocatedBytesForCurrentThread`
microbenchmark returned zero in Unity and was not used as allocation evidence.

Long recordings emitted memory-pressure warnings and discarded frames. Recording was
stopped/cleared; final sampling was bounded to three seconds. Use short captures or
ProfilerRecorder for follow-up. Editor/Profiler overhead is included; GPU timing and
startup FontResolve timing were not established.

## Still open — P1/P2 remain PARTIAL

- The menu is still too slow; backdrop replicas remain expensive (over 1000 copies were
  observed for 74 source UI entries). No blur/effects were disabled.
- Existing Reconcile sorts native in-flow children by CSS z-index through BringToFront.
  Unity uses that same hierarchy for flex layout, so the map/sphere entry moves below
  the loadout controls. The source TSX puts it above them. Separate flow order and paint
  stacking before accepting menu layout; do not remove z-index semantics as a workaround.
- Initial-position transitions, resize, text wrapping, navigation at multiple scales,
  full visual parity, texture lifetimes and the native Settings pilot require further checks.
- Remaining native screen migration and renderer modernization are not complete.
- No gameplay/balance/RNG/source branch changes. No APK build.
- GitHub Actions outcome must be read on the resulting commit; local PASS is not CI SUCCESS.