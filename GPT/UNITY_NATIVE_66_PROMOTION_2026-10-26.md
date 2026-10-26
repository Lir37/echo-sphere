# Unity 6.6 native presentation promotion — 2026-10-26

User explicitly approved promotion after the separate-copy evaluation.
Starting unity-migration HEAD: c929033a88000654044af960a59c0b3c5fffcb6a.
Published implementation: 4ad3c803ef46934ec1db92a4f695c4aca2c00043,
CI #866 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40280175581
Main UnityProject now targets 6000.6.1f1 (7efac9f6c10e), URP 17.6.0,
Android minimum SDK 26, ARMv7 + ARM64. Original company/product/application
identity retained; the evaluation product/save namespace was not promoted.
No APK was built. Android x86_64 is unsupported in this engine version.

## Implementation boundary

Authored NativeBattleSlice is the sole enabled build entry, index 0. Bootstrap
and future build entry require that scene instead of generating the prototype.
The prototype asset remains available as a reference, outside enabled scenes.
Existing seven scene-owned documents/panel templates and sixteen world-view
resource groups remain; formulas, RNG, save format and Core sources unchanged.

Production glass uses native UI Toolkit backdropFilter. Unity owns capture,
clipping, ordering and temporary resources. The existing Gaussian appearance is
supplied through FilterFunctionDefinition; no prefix-tree duplication or capture
cameras are used. SourceUiBackdrop/Replica/Frame/snapshot helpers, old capture
bridge and NativeUiCapturePanel are Editor-only. Typechecks reject these types
in the Android player. Reference probes and legacy comparators also stay Editor-only.

6.6 object-identity API adaptation preserves the previous public integer-ID
contract and ordering, including hit/ignore sets. No new gameplay behavior.
Project/package/URP serialization was upgraded; existing artwork postprocessor
reapplied its already-authored sprite settings. Its import policy was not changed.
No effects, authored artwork, animation, resolution or quality were deliberately
reduced. Remaining visual acceptance is listed below.

The two generated 6.6 project settings (empty PhysicsCore 2D binding and default
Editor ProjectAuditor settings) are tracked in the closing checkpoint. Existing
Physics2DSettings/physics simulation parameters are unchanged.

## Preservation and verification

- Candidate and main baseline hashes checked before copying. Prior working diff
  and affected originals backed up outside Git at
  C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-before-66-promotion-20261026-060734.
- Five user project/render files were not replaced by evaluation settings.
  Engine-only serialized changes are staged separately from user overrides.
  Local quality/URP/default-volume settings, icon metadata, recovery files and
  the earlier uncommitted probe/shader experiments remain preserved.
- Actual main Editor: 6000.6.1f1, Android target, authored native entry index 0.
  Fourteen menu transitions/submissions/picking PASS. Actual saved-run Continue
  loads native battle/world owners; no unsolicited region screen.
- Four populated pause tabs and Resume PASS on the existing save. The checker
  was corrected to select the document containing the requested control: Continue
  retains a hidden menu UIDocument, so the first enabled document is insufficient.
- Play stopped; all ten original preference/save keys restored and checked.
  Console ground truth 0 errors / 0 warnings after checks. Startup deprecation
  notices are separate; no claim of a warning-free initial engine import.
- Pure C# contracts: 1,383 assertions / 47,389 TS-to-C# comparisons PASS.
  Existing VisualChecks PASS; 272 Node tests PASS. Source typechecks and project,
  metadata/provenance guards pass.
- C# API checks against installed Windows 6.6 and official Linux 6.6 references:
  all three Android configurations 0 errors / 0 compiler warnings. A clean index
  snapshot of 348 runtime / 6 Editor sources passes independently of unstaged
  local experiments. Official Linux archive pins cover 438 DLLs / seven blocks,
  approximately 37.64 MiB; each DLL SHA-256 verified. CI now checks the 6.6 APIs.

Separate-copy pixel results and transparent-edge AA limits remain documented in
UNITY_NATIVE_66_EVALUATION_2026-10-26.md. Those results are not phone measurements.

## Remaining acceptance

Later Editor checkpoint covers the full tutorial, run/reward/result transitions,
populated modals and handedness/scale controls; it also fixes legacy scale storage.
See UNITY_NATIVE_66_EDITOR_ACCEPTANCE_2026-10-26.md. Device/performance acceptance
is still open; the original promotion-stage list below is historical.

Replica-free glass and native entry are now promoted implementation, not pending
architecture work. Do not restart the transfer or rewrite working Unity mesh,
material, shader or procedural-geometry code merely because it is authored C#.
Full tutorial, all populated reward/result/detail variants, touch controls and
placement/drag behavior still need acceptance on the promoted project/device.
Transparent rounded-edge AA, arbitrary transformed filters and Linear/HDR have
not received universal parity acceptance. Performance work remains a separate
stage: no current FPS/GC/device GPU or long-combat result is asserted here.
P1/P2/full visual and device acceptance remain NOT DONE. new-desing is protected.

Continuation baseline: implementation 4ad3c803ef46934ec1db92a4f695c4aca2c00043,
CI #866 SUCCESS. Read the three mandatory GPT documents, modernization progress
and this report; verify actual HEAD/CI. Main is on Unity 6.6 with native glass and
entry promoted. Finish remaining visual/control/device acceptance, then address
measured Android bottlenecks. Preserve user settings/saves; do not build an APK
without the agreed final-stage decision.
