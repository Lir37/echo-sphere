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

Desktop follow-up implementation: `e91dfe377a44aeeac3246ab5ebc50b198b2c1f76`.
Unity Migration Contracts **#850 SUCCESS**:
https://github.com/Lir37/echo-sphere/actions/runs/40137812178
All three jobs (real-API C# checks, gameplay/visual/source contracts and source
typecheck) passed. Final actual Editor GPU rerun also passed with Console 0/0;
validation readbacks restore their active target before resize/release. Fresh
Play: home -> characters -> mastery expansion -> home passed; visible-first
portrait loading and retained controls present, Console 0 errors / 0 warnings.
Play stopped. Only pre-existing user settings/SVG metadata/recovery files remain
uncommitted. The following documentation commit records this closure and requires
its own CI; it does not add a device acceptance result.

## First USB baseline and authorized diagnostic APK — 2026-10-25

Xiaomi 11T / Android 14 connected through authorized ADB. Previous installed
0.1.0 release IL2CPP ARM64 build predates this implementation. Its SurfaceFlinger
presentation history contained 126 valid frames over 49.595 seconds, including
menu/run transitions: 2.52 presented frames/s, p95 interval 1.720 s, maximum
4.282 s. This is not a steady-combat Unity Profiler measurement. At the symptom
capture, process PSS was 881,905 KiB, graphics accounting 559,896 KiB; a later
sample fell to 740,842 / 409,888 KiB. No process restart was observed; the still
capture showed combat, not the intermittent region overlay. The old build has
no route journal. Missing Noto Color Emoji font warnings are present; they do
not establish the cause of the stalls or region overlay.

The user then explicitly authorized a new diagnostic APK and an in-place update.
Built source HEAD: `f6bbafc6f91fb5e6d70fad0b4bea7872181be5f3` (CI #851 SUCCESS).
Actual Editor BuildPipeline: Development, IL2CPP, existing ARM64 + ARMv7 targets;
7m42s, SUCCESS, 0 errors / 4 warnings. APK size 104,368,757 bytes, SHA256:
`7828532254235fd83fbd6de4bfee7719225158837d9d94e36d5f0cb6ef1e11cc`.
Warnings: Pipeline runtime config absent (remote command pipeline disabled in
Player), and SourceUiSurfaceFilters Blur potentially uninitialized on d3d11,
gles3 and Vulkan. The shader already initializes its result; no speculative
shader rewrite or suppression was made. Investigate with actual frame evidence.

APK certificate matches the installed app and local debug key. `adb install -r`
succeeded; no uninstall/data clear was performed. Original APK is retained on
the laptop. Launch log confirms Development / ARM64 and actual EchoSphere.Route
events. Unity Profiler connected through the observed app's ADB socket and
received a 300-frame rolling capture; initial capture saved locally. Development
profiling overhead prevents treating its FPS as release acceptance. Device logs,
APK and profile are in the local temporary echo-device-build-f6bbafc6 directory;
raw device telemetry is not published to GitHub.

All five user render/settings files retained their pre-build SHA256. No gameplay,
visual-quality or source changes were required for this build. Next: reproduce
the same menu/tutorial symptoms on this installed build, correlate route events
with CPU Timeline / GC / rendering evidence. No new-build FPS, GC or intermittent
region cause is confirmed yet. P1/P2/device acceptance remain open.

### New-build symptom captured, Vulkan comparison pending

The user reproduced the symptom on the installed diagnostic APK. Actual phone
Profiler samples were saved locally (symptom-phone.data, slow-frames.txt).
Main-thread frames 1898-1905 ranged 2.001-5.423 seconds. In frame 1903,
Gfx.WaitForPresentOnGfxThread was 5.394 s; the render thread had Vulkan Present
3.359 s (pipeline threading means these durations must not be added). In frame
1899, Native run panel / UIElements.UpdateRenderData was 4.292 s, with GC.Collect
7.32 ms. These locate severe stalls in UI/render/presentation, not evidence that
gameplay simulation or GC explains the multi-second frames. Direct GPU timing
was unavailable (zero field means unavailable, not zero GPU cost).

At capture, PSS 1,279,128 KiB and graphics accounting 882,436 KiB. This is a
Development/profiling sample, not comparable release acceptance. After the
expected one-time region launch/scene reload, route records remained menu=False,
region=False on the same runtime/scene through the reported combat symptom;
pause commands were observed. A still capture showed combat, not the region
window. No repeated logical region transition is demonstrated in this capture;
an intermittent graphics artifact remains a hypothesis, not a proven cause.

For a same-APK API comparison, the app was backgrounded through the Home intent.
Actual application-pause/blur events and an updated preference file confirmed
the existing pause/save path; a local preference backup was taken. Then the app
was relaunched with Unity's supported `unity=-force-gles` intent argument.
No project graphics API list, effects, resolution or quality settings changed.
Profiler reconnected and receives frames; user was asked to Continue/resume the
same saved combat. GLES combat result is pending. Raw logs/profile/preferences
remain local; no telemetry/preferences are committed.

### GLES symptom captured and exact backdrop kernel reuse

The symptom starts immediately on combat launch according to the user. GLES
also stalled: most of the last 41 sampled main-thread frames took 1.45-1.78 s,
with peaks ~3.02 s. WaitForPresent dominates; an async profiler screenshot
readback added 1.465 s on one render-thread frame. This instrumentation overhead
is not attributed to gameplay. A 12-second device recording had four presented
images, all showing combat, with no region chooser visually captured. No repeated
region route appeared after launch. API switching is not a fix. Captures saved;
Frame Debugger disabled after inspection.

Confirmed expensive code path: SourceUiBackdrop computed Gaussian exp() weights
inside every pixel/tap. It now uses the existing bounded NativeGaussianWeights
CPU cache for supported kernels (radius <512), uploaded in native filter
property blocks. All taps, radius, normalization, mirror boundaries and color
conversion remain unchanged. Unsupported kernels keep the analytic shader path.
This reduces shader work; phone stalls/region symptom are NOT claimed fixed.

Actual native UIR GPU probes, sigma 2/6/12, compared cached weights with the
original analytic shader branch: seven phases each passed; full 140x80 filtered
panel patches had 0/255 channel difference. Covered live textures, text/style/
clip edits, off-region animation and owner removal. Shader messages: none.
Android player/runtime/editor typechecks: 0 errors/warnings; gameplay contracts:
1,383 assertions / 47,389 comparisons; Node: 272 passed. The installed f6bbafc6
APK predates this candidate; an updated APK is needed for phone verification.

### Full startup recording and candidate installation closure

The user closed the app and performed a fresh launch/settings/tutorial reset,
region launch and tutorial reproduction while screen video and a fresh Unity
log were recorded. The user then said the recording was sufficient and that
the tutorial itself was severely slow. Both files were saved locally. This full
startup run was video/log capture, not a new full-session Profiler trace; earlier
Vulkan/GLES profiler captures remain the measured CPU evidence.

Route timestamps: tutorial state 0 at 10:05:15.089, frame 369; state 1 at
10:05:49.234, frame 379. Thus only ten frames advanced during 34.145 seconds.
The video confirms multi-second held frames and delayed card/control layout.
Examined all recorded combat images from 62.07 through 162.80 seconds: tutorial
steps 1/6 and 2/6 are visible, with no repeated full region chooser. Region=true
occurs only before the expected launch/scene reload; combat retains menu=false,
region=false. The earlier intermittent overlay is not declared fixed or fully
diagnosed, but it was not reproduced visually in this complete recording.

Kernel-cache implementation `1db832f1e930ee359ecea6248a0eaca0c96f87c7` passed
Unity Migration Contracts **#854 SUCCESS**:
https://github.com/Lir37/echo-sphere/actions/runs/40171347711
Incremental Android Development build: SUCCESS, 1m42s, 0 errors / 4 known warnings.
Candidate APK: 122,872,268 bytes; SHA256:
`d86cb3765b84b0beb92c18daed066c844e4934d6f9e09cfefbbfd28d5b185192`.
After the user's capture finished, signature was verified and adb install -r
succeeded. Preference XML before/after update is byte-identical; all five user
render/settings files retain their original hashes. No new combat replay was
requested or performed, and no candidate phone performance improvement is
claimed. Installed app now contains 1db832f1; pending phone acceptance must not
be confused with the preceding f6bbafc6 recordings. No permanent GLES setting
was introduced. Raw logs, video, profiles and preference backups remain local.
