# Android performance: findings, implementation and acceptance

Branch: `unity-migration`. Implementation baseline:
`b4a4ff4596727dfb8e08391a757f46ee426ba43d`.
Protected `new-desing`: `737ef2af4a877e0995db5f92331d536d2f29a934` (unchanged).
Target reported by user: Xiaomi 11T, latest manually built local APK.
No ADB device is connected. This work does not build an APK.

## Confirmed expensive paths and changes

| Path | Implementation | Limits |
| --- | --- | --- |
| Each world shadow used a full padded viewport | Integer-aligned transformed mesh bounds, full four-sigma/offset margins, label vertex offset, conservative full viewport for unknown vertex shaders | Preserves source-over/additive/screen order and 4x MSAA; moving shader geometry retains full viewport |
| Gaussian exponentials and individual texture taps | Shared bounded CPU weight cache; paired bilinear taps; analytic fallback for large kernels and non-filterable RFloat UI masks | Same Gaussian support/weights; Android driver execution still unverified |
| Repeated glass prefix preparation | One appearance traversal per root/frame, retained glass results until geometry/text/style/image/paint changes | Unknown custom native painters invalidate conservatively; moving run backdrops remain separate |
| Character screen constructed all heavy graphics and loaded all art synchronously | Resources.LoadAsync with one outstanding load, viewport-first requests then neighbouring cards; clipped surface preparation | All ten original-quality portraits can still occupy memory after visiting all cards; no compression/downscaling introduced |
| Screen teardown/rebuild on navigation | Retain character/region/progression/archive screens; compare display DTO contents on entry | Content changes still invalidate appropriate UI; archive language changes rebuild chrome |
| Whole character list rebuilt after expanding/selecting | Replace only changed cards, preserve unchanged controls, ordering and scroll offset | Structural/language/layout changes still rebuild the screen |
| Independent identical brushes | Exact immutable skin keys, shared textures/materials, reference-counted release by live owners | No orphaned global texture cache; filters remain attached while screens are hidden |
| Per-frame world objects, repeated text shaping | Draw/state pools; glyph preparation only when dirty; retain label delegates and meshes | Dynamic authored contours still need tessellation |
| Walking bones repeatedly tesselated from rounded endpoints | Share stroke geometry using invariant lengths returned by the existing IK solver | Tail/deforming contours keep their actual geometry; no stroke-width scaling or animation quantization |
| Per-phase FindAll lists | Reused phase snapshots preserving forward/reverse order, activation and spawn timing | No agent-order/RNG changes |
| Network reconstruction every frame | Exact position/alive/disabled/preference cache; reused node lists | All resonance/architect bookkeeping still executes; moving nodes invalidate immediately |
| Snapshot reflection and node allocation each second | Cached field metadata, retained player/runtime/entity node buffers, stale entity-buffer release | Same JSON fields/version and synchronous PlayerPrefs persistence; one-second cadence and lifecycle saves unchanged |

## Region overlay diagnosis

The tutorial does not normally set the flags needed to show the region screen.
The underlying Android cause is NOT confirmed fixed. A bounded 64-event journal
logs route commands/state, runtime and scene IDs, tutorial/pause/focus changes,
low-memory events and documents with visible content. No per-frame log spam.
Stale home commands and duplicate region launches are rejected. Hidden native
screens retain filter ownership with visibility/opacity/input disabled, avoiding
the previously observed Unity 6.3 filter texture-ID lifetime hazard.

In a controlled Editor transition from the region view to tutorial state 0, the
region root was hidden/opacity zero, the native run document visible, and the
actual tutorial Next control advanced to state 1. This is NOT a complete Android
tutorial or a reproduction/diagnosis of the intermittent device overlay.

## Verification

- Actual Unity 6000.3.24f1 compilation succeeds.
- Android player/editor-runtime/editor-assembly semantic C# checks: 0 errors and
  0 compiler warnings; gameplay contracts and source parity pass.
- Retained snapshot tests compare serialized output and restore results over 50
  collection grow/shrink/null/field-selection changes; schema and node reuse pass.
- GPU world comparison: 180 cases, two densities, rotated/nonuniform geometry,
  viewport edges/offscreen shapes, label vertex offset and three blend modes.
  ROI shadow area total: 4,320,471 vs 36,745,680 pixels (about 8.5x less).
  Paired taps differ from analytic reference by at most 2/255 per channel.
- GPU UI brush comparison: 24 cases, DPR 1/1.75/3, outer/inset shadows, spread,
  offsets and very small kernels; maximum channel difference 1/255.
  These GPU checks execute on Editor Direct3D12, not Android.
- Exact-input network cache: 160 cases; simulation spawn/activation/reverse-order
  snapshots: three scenarios; shared skin ownership/immutable keys pass.
- Play Mode navigation through characters/shop/scores/achievements/archive/settings,
  return to home and region selection checked using actual submit events.
  Portrait 360x800 and landscape checked. An unchanged character card retained
  its identity across exit/reentry; portraits loaded on viewport/scroll demand.
- 272 Node checks and Unity project preflight pass. CI must pass for the containing
  commit separately; neither CI nor Editor checks establish device FPS.
- Actual Editor build flags inspected: Android target; Development, debugger and
  profiler connection disabled. This describes current settings, not an APK stamp.

## Remaining acceptance and follow-up

1. Reproduce and classify the region overlay using the new event journal on the
   actual device; distinguish route changes, lifecycle restart and texture ghosts.
2. Xiaomi 11T: cold/warm menus, complete tutorial, touch placement/movement,
   crowded combat, CPU/GPU/GC/memory and sustained thermal behaviour. No device
   before/after FPS or GC figures are available. Stable 60 FPS is not claimed.
3. Further draw-call aggregation, immutable world shadow-mask reuse and remaining
   dynamic contour/run-backdrop work need targeted profiling and visual checks;
   they have not been replaced by an unverified renderer rewrite.
4. Synchronous disk persistence and scene reload on starting a region remain.
   Any future asynchronous storage migration must preserve recovery guarantees.

User URP/quality/volume/settings/recovery changes are excluded from this work.
No graphics effects were disabled, no resolution/MSAA was reduced, and no gameplay,
balance, formulas, RNG, character progression or protected branch was changed.
P1/P2 and Android performance acceptance are NOT DONE.
