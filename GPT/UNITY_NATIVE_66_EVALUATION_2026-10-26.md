# ECHO SPHERE — separate Unity 6.6 evaluation

Status update: user subsequently approved promotion. Main now uses 6.6/native
glass and entry. See UNITY_NATIVE_66_PROMOTION_2026-10-26.md. The copy-only and
pending-approval statements below describe the evaluation stage historically.

User authorization: evaluate Unity 6.6 in a separate copy. Main project promotion
is not yet authorized. Main baseline: a9766ade3d392453af7ef51bdf2fa577ac181f52,
unity-migration, CI #864 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40237025849

## Isolated candidate

Project: C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-6000.6.1f1.
Unity 6000.6.1f1 (7efac9f6c10e), bundled URP 17.6.0, installed beside 6000.3.24f1.
Actual Assets/Packages/ProjectSettings copied; no Library or Git checkout copied.
Isolated product: ECHO SPHERE Unity66 Evaluation; Android identifier:
com.lir37.echosphere.evaluation66. Bootstrap and CI entry preserve this isolation.
Do not transplant this evaluation identity into the production game.

Unity owns backdrop capture, clipping, ordering and temporary textures through
UI Toolkit backdropFilter. NativeEngineBackdrop attaches/restores that style.
NativeUiBackdropRegistry and NativeMenuGlass no longer copy UI prefixes or create
capture cameras in the player. Original Gaussian kernel, sigma and gamma behavior
use a native custom FilterFunctionDefinition. Stock Blur was tested but rejected
because its output differed. No effects, resolution or quality were reduced.

Replica, prefix/snapshot and old capture classes remain Editor-only references;
Android metadata checks reject these classes and the validation probes.
Coordinate-debug shader branches removed after resolving physical-pixel/DPI
mapping and straight-alpha output. Existing procedural Unity meshes/materials
remain: these do not constitute a second UI/layout engine.

Evaluation bootstrap makes the authored NativeBattleSlice the only enabled entry,
index 0. Missing scene fails explicitly; no prototype is generated. Prototype
asset and main working repo entry remain untouched.

## Verified

- Actual Editor recompile succeeds. Windows/DX12 Play: all 14 menu navigation,
  native submissions and picking pass, repeated after changing entry index.
- Region start via its actual button reloads the native scene; battle document
  and world presentation bind, mainMenu=false, regionSelect=false. Tutorial
  renders; Skip passes picking/submission. Pause pointer event, four pause tabs
  and Resume pass. This is not a full tutorial/control acceptance run.
- Same-frame real menu render targets, old versus native filter: density 1,
  640x960, max channel delta 2/255, mean 0.0000317383, 2 pixels above delta 1.
  Density 2.75, 1760x2640: max 2/255, mean 0.0000442278, 1 pixel above delta 1.
  After debug-branch removal: density 2.75 max 2/255, mean 0.000165451,
  1 pixel above delta 1. Comparisons are not performance measurements.
- Transparent synthetic rounded fixture still exposes edge-AA differences:
  density 2.75 max 77/255, mean 0.0571975, 223 pixels above delta 1. Excluding
  one physical pixel at the boundary: max 2/255, 23 pixels above delta 1.
  Do not claim universal pixel parity. Arbitrary transforms, Linear/HDR and
  every modal remain unverified.
- Console ground truth during menu/battle/pause: 0 errors / 0 warnings. Startup
  warns that legacy Input Manager is deprecated; it is not migrated here.
- Actual 6.6 DLL typechecks: 348 runtime / 6 Editor sources, all three Android
  configurations 0 errors / 0 compiler warnings. These are API checks, not an APK.
- Object-identity adapter matches the public obsolete GetInstanceID low 32 bits,
  proven on 32 actual GameObjects. Integer hit/ignore sets, ordering and save
  contracts remain. Core folder is byte-identical to main; no formulas/RNG edits.

Android Build Support installed. Actual Android target import and compile pass;
Play starts from native entry index 0. A separately keyed Android Editor check
passes all 14 menu submissions/picking; region start reloads native index 0 with
mainMenu=false, regionSelect=false and scene-owned world presentation. No APK
built, installed or timed. Windows/DX12 Editor rendering with Android target is
not Android GPU execution.

## Compatibility and remaining acceptance

Unity 6.6 removes Android x86_64 support. Existing x86 emulator build command now
fails explicitly. A supported ARM Android player/device path is needed. Minimum
SDK is 26. Phone performance, thermals, GPU timing, long combat, full tutorial and
visual/control acceptance remain OPEN.

Review material is in the copy's Validation folder: native66-source.patch,
source-manifest.json, typechecks.json, replay scripts and native66-review.zip.
Baseline hashes include actual local source state and earlier capture experiments;
never apply the patch blindly or overwrite main user changes. Imported 6.6 project
and URP settings must not be copied wholesale into main.

Main remains Unity 6000.3.24f1 / URP 17.3. Five original user project/render file
hashes match pre-evaluation values. new-desing stays protected. Four earlier
capture-candidate files remain uncommitted and OFF in main. Main CI does not
validate the separate 6.6 candidate. Full migration/P1/P2 acceptance NOT DONE.

Recommendation: 6.6 is the practical native backdrop/entry path. Request approval
to promote only after presenting these concrete results and compatibility limits.
