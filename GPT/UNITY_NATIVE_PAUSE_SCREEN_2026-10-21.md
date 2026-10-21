# Unified native pause screen and live UI ownership

Entry: unity-migration@4815395f78663f1c361a05446af8e887b8827d27.
Protected source new-desing@737ef2af4a877e0995db5f92331d536d2f29a934 unchanged.
Implementation is the commit containing this report; determine its CI from Actions.

## Delivered

- One retained UI Toolkit layout owns pause header, vertical ScrollView, footer,
  four existing native tab bodies and seven existing Buttons. A single diagnostic
  switch selects the complete candidate. Original resume/tab/exit closures remain
  authoritative. Original artifact-set detail stays above and blocks native input.
- Source header/body/footer layout anchors are removed only after native readiness;
  unrelated source children are retained. Existing source outer overlay/shell remain
  paint adapters for the original blur, gradient, border and inset/outer shadows.
  Seven sigma8 button backdrops and original footer shadows remain enabled.
  This is not the final native compositor and its production switch remains OFF.
- Paused data no longer forces a source tree/CSS rebuild every100ms. Pause/resume,
  pointer/actions, hook state and resize still invalidate the document. Motion,
  native preparation, layout, canvas paint and blur preparation continue per-frame.
  Active gameplay keeps its existing10Hz source data cadence. This fix is ON.
- Original pause actions now bind during geometry warmup, and chrome construction
  preserves those bindings. This removes a first-ready-frame input window in which
  Return could appear before its original callback existed. Original markup/actions
  and source-reference outputs are unchanged; provenance regenerated.
- Found an independent real Continue bug: RunSnapshotCodec restored saved UI owner
  flags, setting _nativeMainMenuWasActive=false while the live native Home stayed
  visible. Existing save selection is now also applied at root restore.33 explicit
  native/presentation fields are excluded from both directions, including legacy
  snapshots. No gameplay values, RNG, formulas or nested restore policy changed.

## Verification

Unity6000.3.24f1, Pipeline7800, real saved run. New implementation compiled in Editor
without C# errors/warnings. Native screen lifecycle covers all four native pointer
choices, original detail blocking/close, Resume/close, resumed Core time, repeated
pause and restoration (50 checks). A cold first-ready Return passes8 checks, invokes
the original map reload and disposes old controls. Original chrome oracle160cases/
3040checks also passes after warmup binding.7 sigma8 registrations
are present when owned and0 after disable. Source nodes144 ->83 with held native
Stats/HUD versus source chrome, then352 with all native pause ownership disabled.

Actual Continue from native Home preserves the live owner flag, then hides Home
(display None/root opacity0). Fresh capture contains none of33 presentation fields.
Original saved game preferences:6 keys restored and verified after diagnostic Play.
Final Editor Play/paused/Profiler/background all false; Console ground truth0 errors/
0 warnings (one historical cached warning retained).

Android player234 files, editor runtime234 files and separate editor assembly3 files
pass real Unity API checks with0 errors/warnings. Node272 tests and complete visual
provenance/preflight pass. Supplemental local .NET10 core execution passes1193
assertions and47339 source parity comparisons, including legacy root field selection
and nested gameplay restore. CI runs the repository's actual .NET8 contracts.

## Measured paused performance

Final post-Continue-fix A/B/C/A/B/C, same paused saved scene1260x542, native HUD held
constant.10 warm and20 measured frames per phase. A explicitly recreates prior
periodic source invalidation every100ms on the new implementation; it is a paired
counterfactual, not a checkout of an old commit. B retains the source pause; C owns
its layout/content natively. ProfilerRecorder; existing Timeline not cleared.

| Variant | Mean wall frame ms | FPS | GC decimal MB/frame |
| --- | ---: | ---: | ---: |
| Periodic source |373.226|2.68|48.696|
| Source, event invalidation |138.567|7.22|8.036|
| Unified native, event invalidation |56.005|17.86|3.183|

BuildTree/ResolveCss/Reconcile markers are0 in idle B/C samples; source A CSS averages
179.687ms (nested in BuildTree, do not add them). Blur preparation remains expensive.
These are bounded Editor results, not device/GPU, Home-menu FPS, active-world FPS,
production HUD measurements or complete pixel parity. Previous pre-ownership-fix
profile is excluded. Do not compare directly with the prior144-node chrome baseline.
Performance is still below target; do not declare P1/P2 DONE.

## Remaining

- Native screen full pixel/mobile/DPR/scrollbar and tab-warmup visual acceptance before production switch.
- Replace source paint/backdrop adapter with a verified same-frame native compositor.
- Migrate remaining gameplay HUD/overlays and actual world views; active-world CPU,
  allocation, rendering and input acceptance remain open. No APK built.
- User ProjectSettings and Recovery files are preserved.

Next chat: continue from the containing commit on unity-migration, read this report
and main progress; finish pause native compositor/mobile acceptance, then gameplay
views. Preserve formulas/quality and protected source branch; no APK yet.
