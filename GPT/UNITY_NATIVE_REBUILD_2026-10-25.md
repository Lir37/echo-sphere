# Native Unity presentation rebuild — 2026-10-25

## Decision and protected baseline

User approved replacing problematic presentation architecture in parallel,
retaining the verified simulation, formulas, RNG, save format and visual design.
This is not a new game implementation. Baseline: unity-migration at
de16e6537989a54c4e79e7f0db01c84fe1b3f660, CI #857 SUCCESS.
Protected new-desing remains 737ef2af4a877e0995db5f92331d536d2f29a934.

## Six work areas

1. **Retain core:** existing runtime/core read models, commands, calculations and
   storage remain authoritative. No simulation changes in this slice.
2. **Parallel scene:** EchoSphere_NativeBattleSlice contains the same runtime and
   a scene-local NativeBattleSceneRoot opt-in. Prototype remains first/default
   build scene; extra scene is appended without replacing existing entries.
3. **World views:** the scene owns serialized World presentation and Battle
   interface children; the runtime uses these bindings rather than constructing
   unrelated objects for this route. Existing validated meshes/shaders and the
   command-buffer compositor remain; replacing submission is not claimed done.
4. **Persistent native UI:** NativeBattleDocument loads NativeBattle.uxml/.uss.
   Named field/HUD/modal/tutorial hosts are authored assets; existing typed
   presenters bind to them. No duplicate legacy battle document is mounted.
5. **Effects infrastructure:** opt-in bounded prefix geometry in the parallel
   document restricts bitmap and flat background paint to the filter support
   region. Flat root backgrounds were another source of full-screen geometry.
   Bitmap density, blur radius, blend and sampling stay intact.
   Unknown/custom painters and transforms use the conservative existing path.
   The prototype uses the original unbounded path for comparison.
6. **Acceptance:** actual Editor compilation, GPU filtered-patch comparison,
   Android emulator execution, game contracts and CI are separate checks.
   Emulator timing cannot establish Xiaomi 11T performance.

## Supporting diagnosis before rebuild

The opt-in replay on the actual Xiaomi 11T (Vulkan, de16e653 diagnostic APK)
still stalled after CPU blur-weight caching: tutorial stage 0 recorded 6 frames
in 23.316 s, stage 1 recorded 4 frames in 21.443 s. GC counter means were
824916 and 812785 bytes/frame; render texture counters grew from 260 / 480304503
bytes to 320 / 702214843 bytes. Resource enumeration included many multi-megabyte
UI TempBuffer targets despite small tutorial button ROIs. This supports targeting
presentation/filter ownership; it does not prove every suspected cause.
No captured route showed an unsolicited region transition. The intermittent
region-window report remains unresolved, not silently marked fixed.

## Validation in this slice

- Unity Editor compiles the new document and scene. Play Mode loads the new
  scene marker; the document loads its UXML with a single native-run-root.
- Real UIR screen-panel fixtures compare full filtered patches against the
  original unbounded/analytic path. Cases include nonuniform live images,
  group opacity, padding/borders, translated groups, cropped UV, clipped rounded
  parent, resize, off-region animation and native text.
- Screen sigma 2 / density 1, sigma 8 / density 2.75 and sigma 12 / density 1:
  maximum channel difference 0/255 in the tested captures. These are fixtures,
  not proof of every production screen or backend.
- Final flat-background case: screen sigma 8 / density 2.75 and world-space
  sigma 12 / density 1 both pass with maximum channel difference 0/255.
  The fixture's signal-dominance threshold accounts for the linear world capture;
  exact reference comparisons still cover the full filtered patches.
- Actual authored-scene Editor Play: continued the pre-existing saved run,
  verified UXML/native world texture bindings, Console 0 errors / 0 warnings.
  Original ten game preference keys restored and verified after stopping.
- Core checks: 1383 assertions and 47389 direct TypeScript/C# comparisons pass.
  Node: 272 pass; Unity project preflight passes.
- Android 14 Google APIs x86_64 AVD installed in a separate local SDK; WHPX
  acceleration verified operational. Unity's bundled SDK is unchanged.
- Emulator APK build/execution and final CI: pending below.

## First emulator iteration

First native-scene x86_64 Development IL2CPP APK built successfully in 403.496 s,
0 errors / 5 warnings. Actual APK size 81589377 bytes (BuildReport's total size
also counts other build products). ProjectSettings hash restored unchanged.
Host Vulkan attempt exited before game UI; do not attribute this to a proven
game defect. The AVD boots with host OpenGL and emulated Vulkan disabled; the
game runs with explicit -force-gles, without changing project graphics settings.
The first full-screen Android notice was acknowledged before the replay.

The initial bounded-bitmap route (before flat-background restriction and scene
object binding) rendered both tutorial stages, with correctly oriented UI:

| Stage | Frames / seconds | GC mean / peak bytes | RT count / bytes |
|---|---|---|---|
| tutorial-0 | 21 / 20.083 | 288272 / 2557784 | 132 / 183234255 |
| tutorial-1 | 20 / 20.579 | 406941 / 995955 | 222 / 178874079 |

This remains very slow. It is not a before/after comparison with Xiaomi hardware
and does not establish an improvement. Logs/screenshots stay local. Final APK
adds scene-owned presentation, flat background cropping, and a development-only
tutorial-baseline replay option to compare the prototype on the same emulator.

## Continue

Validate the parallel scene on Android first. Then move world/view ownership and
explicit effect layers into the scene, followed by remaining menu/game screens.
Do not switch production/default entry until the parallel route passes visual,
input, save/resume and tutorial checks. P1/P2 and the rebuild remain NOT DONE.

## Final slice check

Final emulator APK: Development IL2CPP x86_64, build Succeeded in 115.811 s,
0 errors / 5 warnings. Warnings: optional tool RuntimePipelineConfig missing,
deprecated x86-64 target, and three existing SourceUiSurfaceFilters Blur warnings
for d3d11/GLES/Vulkan. The URP/game graphics settings were not changed.
All five original user settings/render asset hashes match after the scoped build.
APK SHA-256: 7989D1BC487283D3D2BABB26ED39224AE558E1BCD06C93027DBB0EF20476D4CC.
Actual local APK size: 121857206 bytes. Local-only diagnostic artifact, not release.

Same AVD, same APK, same GLES backend, fresh process for each replay:

| Route / stage | Frames / seconds | GC mean / peak bytes | RT count / bytes | Draw counter |
|---|---|---|---|---|
| Prototype / 0 | 18 / 20.001 | 370322 / 3381024 | 196 / 356578047 | 300 |
| Native scene / 0 | 17 / 20.170 | 330639 / 2564776 | 197 / 344026311 | 199 |
| Prototype / 1 | 17 / 21.772 | 388836 / 1098311 | 256 / 439955111 | 353 |
| Native scene / 1 | 17 / 20.247 | 416394 / 951345 | 250 / 437949759 | 259 |

Both replays completed through existing UI commands, no new exceptions or
unallocated-texture errors in their captured logs. The new route reduces the
draw counter in these samples but **does not establish a frame-time improvement**:
both remain around one frame/second. Large native UIR temporary targets remain.
Do not promote this bounded-prefix candidate as the Android stall fix.
No Xiaomi performance acceptance or full tutorial/input/long-battle acceptance.

This slice completes the parallel-scene foundation, serialized presentation
ownership and UXML host binding. It does not replace the world submission path
or the prefix-based filter architecture. Next substantial implementation:
explicitly owned native capture/effect layers replacing per-button prefix/filter
composition, then the remaining view groups. Preserve existing rendering as the
reference until the replacement passes actual visual and interaction checks.
CI for this implementation commit is pending publication/check below.
