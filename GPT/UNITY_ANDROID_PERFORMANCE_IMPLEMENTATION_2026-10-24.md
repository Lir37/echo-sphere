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
3. Adjacent solid draws, retained world masks and run-backdrop layout reuse have
   now been implemented and checked in Editor (follow-up below). Truly deforming
   contours still use native tessellation. Animated overlapping/custom UI paint
   still conservatively invalidates its dependent backdrop layout.
4. Synchronous disk persistence and scene reload on starting a region remain.
   Any future asynchronous storage migration must preserve recovery guarantees.

User URP/quality/volume/settings/recovery changes are excluded from this work.
No graphics effects were disabled, no resolution/MSAA was reduced, and no gameplay,
balance, formulas, RNG, character progression or protected branch was changed.
P1/P2 and Android performance acceptance are NOT DONE.

## Verification closure — 2026-10-25

Implementation commit: `318a00fc771f71f9b228a04d8184ac6b8bb84cd0`.
GitHub Actions Unity Migration Contracts #848: **SUCCESS**:
https://github.com/Lir37/echo-sphere/actions/runs/40132214918

Final local semantic checks: 334 Android runtime source files, 0 errors/warnings
in player, editor-runtime and editor assemblies. Both modified shaders report
no compiler messages. Gameplay: 1,383 assertions and 47,389 source comparisons.
The actual live runtime snapshot also serialized identically with Unity
JsonUtility across three retained captures (273 nodes), without writing prefs.
Fresh final Play session: character entry/mastery control passed; Console 0
errors / 0 warnings. Play stopped; original Game View preset restored and the
temporary portrait preset removed. User render/settings/recovery changes remain
uncommitted and excluded. A documentation-only follow-up records this closure;
its CI result must be distinguished from implementation #848.

## Desktop-only completion — 2026-10-25

Starting HEAD: `1bc75337c87e3d001ae111a46cc6f50061f33af5`, CI #849 SUCCESS.

### Retained masks and ordered GPU draws

- Final blurred alpha masks for known native solid geometry are retained only
  after exact repeated input. Keys include mesh/version, transform, viewport,
  ROI, alpha, opacity and blur; tint, offset and blend are applied per draw.
  Changes to geometry, density or viewport invalidate reuse. Payload limits:
  16 MiB total, 128 entries, 1 MiB per mask. Moving/one-shot shapes need not
  allocate retained textures. Idle/removed/lost resources are evicted before
  recording commands; no texture referenced earlier in the current buffer is
  released. Unsupported CopyTexture keeps the ordinary temporary-target path.
- Consecutive compatible native solid commands now use Unity DrawMeshInstanced,
  in groups of at most 64, with per-instance color/opacity/transforms. Shadows,
  shader/mesh/blend changes are barriers. No transparency reordering or merging
  across another effect. Unsupported instancing retains individual draws.
- A Resources material and explicit instancing variant collection preserve the
  variant for builds without editing the user's global graphics settings.
  Project preflight checks their shader GUIDs and instancing flags.
- Tessellation snapshots and upload lists reuse buffers. Exact in-place curve
  mutation still causes real native Vector Graphics tessellation; algorithm,
  tolerance, vertices and triangle order are unchanged.

### Run backdrop preparation

Native run backdrops retain layout/style replicas while repainting custom
content and live render textures. Versions distinguish live pixels from layout,
include ownership/structure/resource changes, and restrict ordinary paint
invalidation to the panel's blur influence region. Ancestor transforms/opacity,
text overflow margins and root world scale are accounted for. Unknown custom
painters and filters remain conservative. Snapshot entries are kept alive while
their replicas are reused; disposing a filter owner invalidates dependent copies.
The legacy document path keeps its previous preparation policy.

### Actual verification and limits

- Unity Editor 6000.3.24f1, Direct3D12: retained mask comparison **8 scenarios,
  10 hits, pixel-identical** against uncached rendering; geometry mutation,
  alpha, tint/blur, viewport and density changes exercised.
- Resource pressure: 26 large / 128 small masks, peak payload **16,640,000 bytes**;
  byte/count limits, unused eviction, destroyed mesh and lost RT eviction pass.
- Ordered instancing: **24 GPU comparisons, maximum channel error 0/255**.
  Fixture geometry draws **3,360 -> 396**. A 140-primitive compatible run uses
  three calls. These are fixture draw counts, not measured device frame times.
- Tessellation: **48 exact fresh/reused geometry comparisons**, stable unchanged
  versions and in-place contour invalidation pass.
- Isolated live UIR capture: seven phases, full filtered 140x80 panel pixels
  **identical** to forced full preparation. Covered live texture updates,
  off-region opacity animation, text/opacity/clip edits and filter owner removal.
  Default FontDefinition without a legacy Font is covered (null guard added).
- Actual tutorial state 0: **178 frames, 0 full preparations / 356 repaints**.
  Actual Next submit advanced to state 1. Animated state 1 still performed
  **444 preparations over 222 frames**; this remaining work is explicitly not
  claimed eliminated. The transient paused setup did not invoke run-start,
  tutorial completion or save-writing commands. Console was 0 errors/0 warnings.
- Actual Android shader-only AssetBundle compilation/packaging with StrictMode
  succeeded, with no shader compiler messages. This is **not an APK or Android
  execution**. Temporary output is under UnityProject/Temp.
- An attempted explicit ShaderVariantCollection.WarmUp crashed Editor D3D12 in
  native PrewarmPipeline. That call was removed. Editor was reopened on the same
  backend, and the successful GPU comparisons above were rerun. No backend or
  quality downgrade was used to hide the failure.
- Final C# semantic checks: 335 runtime / 5 editor source files, zero errors and
  warnings in Android player/runtime/editor configurations. Gameplay contracts:
  1,383 assertions / 47,389 source comparisons. Node checks: 272 passed; project
  preflight passed. Containing commit CI is recorded separately after push.

No device FPS/GC, thermal stability, full tutorial or crowded combat acceptance
is claimed. The intermittent region window still needs a captured occurrence
and its journal. Synchronous persistence and fresh-region scene reload remain
unchanged to preserve recovery/start semantics. No APK, gameplay, effects,
resolution/MSAA, protected-branch or user render/settings changes are included.
