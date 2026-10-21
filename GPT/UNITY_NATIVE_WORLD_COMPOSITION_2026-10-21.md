# Native UI capture and blur composition — 2026-10-21

Entry HEAD: `1ba7359a12120f0b27e86a675e54ead319dcc004`, unity-migration.
P1/P2 remain PARTIAL. This change supplies a runtime capture component and an
Editor acceptance fixture; it does not switch the actual menu or pause renderer.
Protected new-desing, gameplay, RNG, formulas and graphical quality are unchanged.
No APK was built.

## Confirmed blockers and implementation

Capturing before native text preparation produced previous-frame text; submitting
the texture after the consumer panel had prepared its filter produced previous-frame
background. The accepted ordering captures native UIR at Application.onBeforeRender,
then runs the existing Gaussian and renders the native foreground into an owned
target. It uses public URP SingleCameraRequest, outside the URP rendering loop.

NativeUiCapturePanel is a runtime IDisposable wrapper for a world-space UIDocument
and a disabled capture camera. It requires an identity owner and an exclusively
reserved layer, validates target/aspect/readiness, disables generated colliders,
and disposes its owned objects. It performs no tree cloning or GPU readback.
The acceptance fixture reserves layers 30 and 31. Production routing does not yet
instantiate this component.

World rounded clipping alone differed at the corners. A native stencil mask rendered
by UI Toolkit preserves the original edge coverage; NativeMenuPrefix shader pass 3
multiplies blur alpha by this mask. Existing shader passes 0–2 and the sigma8,
four-sigma Gaussian support are unchanged. Layout pivot prevents overflowing input
geometry from shifting capture coordinates. Native background-image rendering
preserves RenderTexture sampling where the Image path did not match.

## Current Editor verification

Final runtime-resource mask pass, actual Unity 6000.3.24f1, Play Mode:

| DPR | Changing-frame samples | Maximum mean error (0–255) | Maximum channel error |
| --- | --- | --- | --- |
| 1 | 32 | 0.0000495911 | 1 |
| 2 | 32 | 0.0002079010 | 1 |
| 3 | 32 | 0.0000258552 | 1 |

All 96 samples PASS the unchanged mean <=0.01 / channel <=1 gate. The fixture uses
authored Inter bold Cyrillic text and a pattern changing every frame, native blur
mask and foreground. Native navigation submission passes in all three cases.
Candidate prefix tree replicas: 0; reference copies: 3. Evidence is in the adjacent
JSON. These are isolated 256-logical-pixel comparisons, not full-screen acceptance.

Actual Editor compile: 0 errors / 0 warnings. Shader supported, 0 shader messages.
Console ground truth: 0 errors / 0 warnings; cached warning history was retained.
After stopping Play: capture targets 0, capture documents 0, Profiler off; all six
original preference keys restored and verified.

Real Unity API checks: 235 runtime files for Android player and editor-runtime,
plus 3 Editor files, all 0 errors / 0 warnings. This is compilation against installed
Unity references, not an Android execution test. TypeScript check and 272 Node tests
PASS; generated visual assets, metadata, provenance and project preflight PASS.
CI must be checked against the containing implementation commit.

Target descriptor bytes (including reference fixture targets): color/depth-stencil
1,835,008 / 2,097,152 at DPR1; 7,340,032 / 8,388,608 at DPR2;
16,515,072 / 18,874,368 at DPR3. Depth format D32_SFloat_S8_UInt. These are descriptor
counts, not measured GPU residency or performance.

## Remaining acceptance and next implementation

Actual pause/menu composition has not been connected. Their candidate remains OFF.
No new game FPS or GC measurement was made; the previous bounded pause measurements
in UNITY_NATIVE_PAUSE_SCREEN_2026-10-21.md remain the latest and are not superseded.

Next: prove multiple dependent glass layers and transparent native target alpha
composition, then connect actual underlay/HUD/pause factories and complete full-screen
visual, input, rotation and performance checks. The present opaque fixture does not
prove that premultiplied native UI targets can be fed into a straight-alpha blending
pass. Do not infer that support or activate the candidate prematurely. Gameplay world
views and Android acceptance also remain open.

Short continuation: continue unity-migration from the containing commit; read three
GPT documents and modernization progress. Native capture/blur composition passes
96 authored-text samples at DPR1/2/3. Next multiple dependent layers and transparent
alpha, then actual pause/menu integration and world views. Keep quality/gameplay,
new-desing untouched; no APK.
