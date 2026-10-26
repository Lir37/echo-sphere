# ECHO SPHERE — battle UI shadow pipeline correction, 2026-10-26

Starting source: unity-migration / 826d0e57fb2c930e2dc96b2299b4b1c21d10a8f9.
Phone disconnected. No new APK or device performance acceptance in this cycle.
Gameplay, formulas, RNG, saves, source branch and user quality settings unchanged.

## Confirmed dominant layer in the controlled Editor experiment

Frozen existing run, native world, actual retained battle widgets, 1080x2400
owned panel target / density 2.75. Each stage has six warm frames and 16 measured
frames. Frame intervals include Editor overhead and a one-pixel GPU completion
readback. They are neither pure GPU timestamps nor Android FPS. Native brush
preparation retains Editor density; native UI filters run at panel density 2.75.

| Layer case | Before mean ms | After mean ms |
|---|---:|---:|
| Native world through field image, other UI hidden | 22.92 | 20.15 |
| Full native UI with alpha shadows temporarily excluded | 16.48 | 19.63 |
| Full native world + complete UI/effects | 343.18 | 21.46 |
| Complete UI/effects, world submission excluded | 338.37 | 18.79 |

Full-case GPU completion wait: 290.33 -> 8.46 ms. World command preparation:
0.98 -> 0.88 ms, 23 geometry draws / no new cached world-shadow blur regions.
The controls show the large stall follows UI alpha-shadow filters even without
world submission. This does not establish a device speedup or universal scene cost.
Excluding layers was diagnostic only; production keeps every authored effect.

## Implementation

The old SourceUiFilters alpha-shadow pass ran a full two-dimensional Gaussian,
including exp in the inner loop: at sigma 7 / density 2.75, radius 77 and 24025
loop/tap evaluations per output pixel. Native mobile sphere selectors and radar
arrows no longer use that pass.

NativeUiAlphaShadow now owns typed native shadow/content layers. Horizontal and
vertical UI Toolkit post-processing passes blur alpha with the same Gaussian,
ceil(4*sigma) support, colors and normalization. Existing bounded CPU weights are
reused. A separate one-pass content layer retains original sharp geometry, native
AA and the original capture margins. Nested child/parent shadows retain their
source-over order. Unity owns filter captures and temporary targets; no browser,
CSS parser, UI-prefix replica, custom capture camera or baked replacement bitmap.

The fixed chain row initially changed Yoga pixel rounding at density 2.75. Its
sharp content now owns intrinsic row sizing, preserving the original automatic
width and individual circle positions. No fixed-width redesign was accepted.
Radar triangle geometry remains the authored native 10px Painter2D triangle;
changing pooled-arrow color removes its old layer registration before rebuilding.

SourceRegionVisual2D is disabled before resource construction in the native route,
matching the other retired visual components. Native DrawNativeFrameOverlay
already renders boundary flashes/shards. RegionFieldVisuals and gameplay POIs are
retained. This removes duplicate culled mesh uploads; it is not presented as the
main source of the measured stall.

## Verification

- Actual main Unity 6000.6.1f1 compile PASS. Three pinned API C# configurations:
  348 runtime / 6 Editor files, all 0 errors / 0 warnings. Project preflight PASS.
- 49 actual production glyph/arrow GPU cases: all ten sphere families, panel
  densities 1 / 1.25 / 2.75, pressed scale .95, interface scale 1.25, three radar
  colors at 0 / 37 / 180 degrees. Original captures and the repeatable original
  single-pass Gaussian reference both compared. Maximum delta 3/255; only three
  pixels exceed 2/255 in the original-capture comparison; worst full-image mean
  0.0264/255. Tiny cumulative mask quantization, no radius/resolution/effect cut.
- Production panel picking/routed pointer check: 53 assertions PASS, 0 new errors.
- Nine populated modal stages / 420 checks / 270 frames PASS, 0 new errors.
- Pooled radar: 24 cycles of 12 arrows changing color and visibility, registrations
  stay at 24 and return to 0 on disposal; 0 new errors. Native legacy region
  component is actually disabled with no MeshFilter created in Play.
- Owned shadow materials after Stop: 0. Scene clean; ten original Windows
  preference keys restored and verified. No device preferences were changed.
- Actual Android compiler: 12 linked Vulkan/GLES3 shader programs, three passes
  and both output-linear keyword states, all Success with nonempty bytecode,
  0 compiler messages. Public ShaderData API; Vulkan/GLES return linked stages
  from ShaderType.Vertex. No player/APK build. The earlier auxiliary bundle probe
  emitted disabled-AssetBundle-module warnings; no module was enabled. Direct
  shader compilation is the final platform compilation evidence.
- Existing VisualChecks PASS; CI is checked independently on the published commit.

Repeatable Editor checks (back up diagnostic preferences before Play; stop/restore):
`check-unity-native-glyph-shadows.cs`, `check-unity-native-render-layers.cs`.
Shader-only check in Edit Mode: `check-unity-native-shadow-android.cs`.
Results are written under UnityProject/Library, not to saves or project sources.

## Remaining

Android performance/GC/memory, sustained battle and menu latency remain OPEN.
The narrow compressed strip at battle startup has not been reproduced or proven
fixed on the phone. The existing frame/layout/owner trace now also records actual scaledPixelsPerPoint,
not only requested panel.scale;
slow frames could prolong a transient layout, but this is an inference, not a
confirmed cause. Next device run must use a newly built APK with current saves
retained and compare real startup geometry, frame times, GC and rendering.
Do not reinstall the withdrawn 657e72ad... candidate or restore an older save.
P1/P2 are not DONE. A camera/world rewrite is not justified by this isolation.

Local evidence: UnityEvaluations/ECHO-SPHERE-retest-20261026T124823Z outside Git.
Before/after layer JSON, actual before/after glyph PNGs, pixel comparison and
Android compiler results are separate from private saved-game backups.
