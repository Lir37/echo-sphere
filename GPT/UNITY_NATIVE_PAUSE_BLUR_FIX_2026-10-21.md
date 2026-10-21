# Native pause / shared backdrop mirror fix — 2026-10-21

Entry HEAD: 814befe44f2d043a46ba877aac19e5708a4f81e3, unity-migration.
Implementation HEAD is the commit containing this report. P1/P2 remain PARTIAL.
Native pause chrome still defaults false; this is not full-screen/device acceptance.

## Confirmed cause and correction

The shared SourceUiBackdrop shader treated normalized atlas UV coordinates as the
CSS capture rectangle. In the actual Unity 6000.3.24f1 filter path, those domains
do not correspond. Mirroring therefore sampled the wrong part of the captured
prefix. GPU callbacks, sigma8, panel size and matrices were all correct; shader
support/compilation was not the cause. A constant-output shader proved the output
layer worked; bypassing mirroring proved the Gaussian sampler worked. Neither
diagnostic variant was enabled in production.

The filter quad supplies LOCAL PANEL coordinates. GPU readback verified that
these are already logical coordinates at DPR2; dividing by DPR again was wrong.
The corrected shader interpolates ROI origin + quad vertex coordinates, mirrors
each sample through the existing inverse/forward panel matrices, then converts
the reflected displacement to atlas texels. Sigma, full four-sigma support,
premultiplied color handling, gamma conversion, clipping and effects are retained.
No private Unity API or reflection was added to production rendering.

The shared shader is used by the source compatibility path as well as the native
button adapter. This fixes that renderer; it does not remove its replica system.
No gameplay/RNG/formula, source branch, visual design or APK changes.

## Editor evidence

- Actual-panel stripe fixture:12 cases /42 checks, DPR1/1.5/2/3 and rotations+15/-15.
  No-blur controls have red-channel spread .854902; every blur case has spread0.
  Mean brightness stays nonzero, preventing a blank-output false positive.
- White panel on exterior black:4 DPR cases /16 checks. Interior remains .886275,
  within the independent source-fill expectation; exterior black does not bleed in.
  A diagnostic no-mirror comparison gave .639216 instead, proving that keeping the
  mirror matters. The no-mirror material existed only in the isolated experiment.
- Fixture teardown restores the original callback, runtime enable state, panel
  scale, background/Profiler state and zero native registrations. Frozen run time
  is asserted/read back. The final one-case cleanup rerun records all restorations.
- Actual saved-run lifecycle:47 checks PASS again, including all native tabs,
  original detail blocking/close, Resume/close and restoration144 ->86 ->144.
  Actual original Return/map scene reload:7 checks PASS. Screenshots inspected.
- Existing early-prefix prototype, depth request24/actualD32_SFloat_S8_UInt,
  changing pattern every frame:32 samples PASS, maximum mean .000404358/255,
  maximum channel1, native click verified. Static-label limited fixture only.
  Late cross-panel negative control still fails on the first changed frame
  (maximum mean .666077/channel57), matching baseline0be716d; no new regression or
  full compositor acceptance is inferred. All owned probe render targets removed.

Fixtures hide the original screen only to isolate their owned pattern and freeze
the runtime while scaling its existing UIDocument. They are not performance modes
and do not change saved gameplay. The obsolete empty-source-document capture
harness was removed; its historical failure JSON remains. Use the panel and edge
scripts, not the old failing capture, to reproduce current functional gates.

## Performance and Timeline

Actual genuinely paused saved scene,1260x542, native HUD/stats held true, only
chrome toggled A/B/A/B;10 warm +20 sampled frames per phase. Correct mirror shader
and all original seven button filters/two footer shadows active in both branches.
Source frame means129.223/130.528ms; native101.237/105.975ms:
**7.70 ->9.65FPS; GC24.557 ->14.671MB/frame**. Still far below target; these are
candidate Editor paused measurements, not active-world/Android performance or
full visual-parity acceptance. Earlier pre-fix captures are not current baselines.

Fresh CPU Timeline data also captured:two available main-thread frames per variant
from bounded four-frame recording windows. Source ResolveCss52.20/66.96ms,
Reconcile20.03/23.10ms, PrepareBackdrops14.60ms in the first frame. Native includes
a frame without a rebuild and one with ResolveCss36.71ms/Reconcile10.92ms;
PrepareBackdrops11.27/10.71ms. BuildTree includes ResolveCss/ProjectTree: never add
nested markers. Canvas.EndFrame ~1.1–1.3ms is only its flush, not total world/GPU cost.
Remaining priority is the native pause shell/compositor and CSS work, then actual
active-world view profiling. No memory-leak or GPU-duration conclusion.

## Validation / remaining

Unity Editor recompile0 errors/warnings; real shader supported,0 compiler messages.
Real Unity API typecheck232 runtime files plus3 editor files:0 errors/warnings.
Locked TypeScript,272 Node tests, project preflight and all8 visual/provenance
checks PASS. Local pinned .NET8 SDK is absent; pure .NET8 contracts are verified
by the commit's CI, not represented as a local .NET8 execution. Read Actions for
the containing SHA; do not inherit baseline CI#772.

Stop restored all6 original preference keys. Final console ground truth0errors;
Profiler memory-pressure/drop warnings remain (27 at final Stop). Timeline limits
and retained Editor/Profiler memory matter; no logs/captures were explicitly cleared.
The earlier Pipeline30s diagnostic timeout is historical, not a shader/game error.
No texture-index3 error observed here; its complete resolution is not claimed.

Functional blur gates PASS. Full pixel/transition/mobile acceptance, original
scroll presentation, artifact glyph/details and native pause shell remain open.
Keep chrome off until those gates pass; then continue native world views. The early
prefix/static-label proof is not proof of dynamic real-menu composition.
