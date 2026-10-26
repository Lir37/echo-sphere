# ECHO SPHERE — Xiaomi USB retest, 2026-10-26

Branch: unity-migration. Starting HEAD: 705fe8d1d6b35361ed0301234dfd304c37c74c55.
Actual phone: Xiaomi 11T / 21081111RG, Android 14, 1080x2400, density 440
(UI Toolkit source density 2.75). Main Unity 6000.6.1f1, native scene, Vulkan.
No graphics API, resolution, MSAA, authored effect or gameplay rule changed.

## Corrected baseline installed and measured

ADB replacement installation succeeded, retaining the existing application data.
APK SHA-256: 003fb912e6ca2e531069435578d745614f36d6150cee47a7963d9665316cb981
(232093576 bytes). Installed base.apk hash matches. Before installation a fresh
private backup was taken; the user's tutorial is now completed and a saved run
exists. The earlier pre-tutorial backup must NOT overwrite this newer progress.

Actual USB Unity Profiler capture, PlayerLoop / Android render threads, valid
GC Allocated In Frame counters:

| State | Frames | Mean frame ms | FPS (1000 / mean ms) | Mean GC bytes/frame |
|---|---:|---:|---:|---:|
| Home menu | 60 | 134.32 | 7.445 | 94 |
| Continued run, pause | 20 | 1886.25 | 0.530 | 148216 |
| Same run, early battle | 20 | 1718.91 | 0.582 | 402497 |

A warmed battle frame was about 1697 ms, including 1644.74 ms in main-thread
Gfx.WaitForPresentOnGfxThread; LateUpdate about 23.9 ms. GPU timing is unavailable
(zero), so presentation waiting is not a measurement of pure GPU execution.
These are a continued normal run, not the older fresh-tutorial scenario; do not
claim a matched performance ratio against the older 0.61 FPS tutorial capture.
The hidden-backdrop and tutorial-layout fixes alone did NOT solve phone stalls.
No unsolicited region screen was observed in this short retest. One low-memory
notification occurred; its cause and significance remain unresolved. Nonfatal
missing AssetPackManager startup message persists; no crash was observed.

## Confirmed remaining shadow cost and implementation

SourceUiFilters pass 2 evaluated a two-dimensional Gaussian including exponential
weights in the inner pixel loop. At sigma 7 and device density 2.75 its support
radius is 77: 155 x 155 = 24025 loop/tap evaluations per output pixel. Native mobile
sphere glyphs include nested shadows; radar arrows use the same pass at sigma 4.
Actual cost is affected by atlas clipping; this count is not GPU timing.

NativeGaussianWeights now supplies bounded cached weights and normalization to
the native glyph and radar filters. For aligned texel centers, four adjacent
weighted texels use one bilinear read with the same Gaussian. Atlas boundaries
fall back to individual reads; unsupported/nonaligned inputs use the exact cached
point kernel. Tiny-sigma zero sums are guarded. The uncached original shader
branch remains the fallback and regression reference. Original sharp content,
support, color, alpha composition, nested order, resolution and engine ownership
remain unchanged. This is a reduction of repeated work, not reduced visual quality.

Stock Unity DropShadow was evaluated and rejected: at phone density it softened
the sharp glyph edges (maximum channel differences up to 105/255 in the fixture).
It was not adopted.

## Verification of the new kernel

Actual Editor GPU comparison against the original two-dimensional pass:
8 cases (density 1 / 1.25 / 2.75, sigma 7 / 4 / 0.01, nested glows, forced point
fallback and actual rotated radar arrow), all PASS, maximum channel error 1/255,
0 new Console errors. Texture declaration conversion alone matched 0/255 in four
reference cases. Repeatable check: scripts/check-unity-native-alpha-shadow-kernel.cs.
It owns offscreen panels and textures, issues no game/save command, and requires
diagnostic preference backup before Play. Original Windows preferences restored.
Actual Editor recompilation PASS; pinned Unity 6.6 API C# checks PASS for Android
runtime, Editor runtime and separate Editor sources: 0 errors / 0 warnings.

Device acceptance of this new kernel is pending the in-progress Android build.
Full tutorial, dense battle, prolonged play and stable 60 FPS are not accepted.

## Evidence and continuation

Local evidence (private saves excluded from Git):
C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-retest-20261026T124823Z
Includes baseline profiler JSON, raw capture, app-only Unity logs, screenshots,
installed baseline APK and shader comparison output. Preserve the current saved
run. Install the new signed candidate as an update, then repeat pause and early
battle measurements; inspect remaining visible filters/rendering if stalls persist.
P1/P2 are not DONE. CI, Editor, GPU fixtures and physical-device acceptance remain
separate results.
