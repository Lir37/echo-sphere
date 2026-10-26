# ECHO SPHERE — complete battle render path audit, 2026-10-26

Latest user observation: Android battle initially appears as a narrow compressed
strip and expands after several seconds. Phone disconnected. This audit does NOT
claim to have reproduced or fixed that symptom on Android.

## Actual active pipeline

1. EchoSphereSourcePresentation.LateUpdate reads Screen dimensions and Android
   source density, derives the original capped logical field (1280 x 800 maximum).
2. NativeWorldPresentation supplies typed native entity/frame layers. Each frame
   they submit Unity meshes/material properties and ordering to NativeWorldSurface.
3. NativeWorldSurface executes a custom Unity CommandBuffer into its ARGB32 4xMSAA
   world target, performs bounded shadow passes, then Blits into a separate UI image.
   Source backing density is capped at 1.75, matching the existing source policy.
4. NativeRunDocument places that image under the retained native HUD in UI Toolkit,
   using the platform density (2.75 on this Xiaomi), the same logical field caps,
   and then native panel/backdrop/filter composition to the display.
5. An enabled URP camera still owns the display frame. Its cullingMask becomes 0
   when the world compositor initializes, so ordinary scene Renderers are culled.

This uses real Unity APIs and native shaders, not a production browser/JS renderer.
It still has a custom offscreen world compositor plus UI texture presentation;
calling it equivalent to normal camera-owned SpriteRenderer scene presentation
would be inaccurate. An extra full-frame copy exists; its cost has NOT been isolated
as the main stall. A render texture itself is not proof of a non-native defect.

Relevant code: EchoSphereSourcePresentation.cs, EchoSphereNativeWorld.cs,
NativeWorldSurface.cs, NativeRunDocument.cs, NativePanelLease.cs,
Resources/EchoSphere/NativeBattle/NativeBattle.uss.

## Parallel/old work: verified scope

Actual main Editor Play inventory: 1 camera, full normalized viewport, no camera
target texture, cullingMask=0; 1 NativeWorldSurface; 0 SourceCanvasSurface.
SourcePlayerVisual2D / SourceElementalVisual2D disabled. Android metadata checks
confirm old Canvas/UI translator and scene snapshots are absent from the player.
Therefore two full old/new battlefield draws are NOT established by current evidence.

Residual old work DOES exist: SourceRegionVisual2D remains enabled, clears/uploads
its mesh each LateUpdate and can rebuild boundary-flash geometry. That effect also
has a typed native implementation in DrawNativeFrameOverlay. Its old mesh is culled
by the main camera; this is duplicate CPU/resource preparation, not a proven second
visible pass or the cause of 1.5-second graphics waits. Other legacy SpriteRenderer
objects are still created as entity shells. Their data consumers must be checked
before removal; RegionFieldVisuals also owns gameplay POIs and cannot be deleted.

## Startup size evidence and limits

Main Editor 1260x542, fresh Play, simulation/save commands disabled for the probe:
20 successive startup frames captured. On the first submission (frame 2) the world
texture is already 1260x542 and logical viewport matches. UI root/field bounds are
NaN before the first layout, then 1260x542 from frame 3 onward. The first world
submission has 22 geometry draws, two small shadow regions totaling 5200 pixels;
settled pause has 23 world draws and cached shadows (0 new blur regions).
This is not a phone timing or first-render pixel proof. Pre-layout NaN is not itself
proof that a malformed frame was displayed. The compressed strip is not reproduced.

The 1280x800 cap is present in original src/App.tsx canvas classes and is retained
by native USS and runtime. It must not be removed as a guessed fix. Android Screen
size/density transitions, UI first-layout geometry, image bounds and RT allocation
must be recorded together during the actual symptom.

## Changes in this cycle

Added NativeBattleFrameJournal in existing presentation diagnostics: development/
Editor only, first four submitted frames and geometry changes, hard cap 64 records.
Records screen, density, panel scale, root/field rectangles, actual texture/logical
size, camera viewport/pixels/mask, owner counts, world draws and shadow regions.
Captures both submitted and layout phases. No render override, camera switch,
replay, save field or ordinary release behavior is introduced. Static frames do not
repeat logs. NativeRunDocument owns the journal, outside the saved runtime schema.

The previous shadow-weight candidate is withdrawn after actual device acceptance
failed; see UNITY_NATIVE_66_ANDROID_RETEST_2026-10-26.md. Existing user changes remain
separate. No new APK after the phone disconnected.

## Next work in order

1. Reproduce cold/continued start, portrait density and size changes in Editor;
   correlate actual display bounds with the new trace. On the next device capture,
   find exactly which dimension/owner changes when the strip expands.
2. Isolate the same paused frame by world submission, world-image presentation,
   native HUD/glyph filters and visible backdrop composition, using diagnostic
   captures only. Do not permanently disable effects or reduce quality.
3. Replace the confirmed dominant algorithm/submission path as a complete unit.
   Full 2D glyph Gaussian kernels remain a substantial architectural candidate;
   caching weights while retaining quadratic work was not a sufficient fix.
   A switch to direct camera-owned world rendering requires evidence and visual/
   alpha/order parity, rather than assuming that another rewrite guarantees FPS.
4. Retire proven duplicate legacy visual preparation without touching POI gameplay,
   then verify Console, visual/input parity and CI. Phone 60 FPS remains unaccepted.

Local startup probe/evidence: UnityEvaluations/ECHO-SPHERE-retest-20261026T124823Z
outside Git. Ten Windows diagnostic preference keys restored after each Play check.

## Validation of the diagnostic change

Actual main Editor compile PASS. The fresh 20-frame probe produced exactly five
frame records: first submission with pre-layout NaN bounds, one settled layout
record, then the remaining initial submissions. No repeated logs in steady state.
One camera / one world confirmed by the trace; Console ground truth 0 errors /
0 warnings after the check. Three pinned API C# configurations pass, 0 errors /
0 warnings; project preflight PASS. Editor stopped, original ten preference keys
restored and verified. This validates the observation tool, not an Android fix.
