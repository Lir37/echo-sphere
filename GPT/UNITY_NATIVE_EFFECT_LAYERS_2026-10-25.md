# Explicit native UI effect layers — prepared, not accepted

Baseline: unity-migration dd03db4addc8697204f71190e594e8474ad69f42.
Continue the parallel-scene rebuild described in UNITY_NATIVE_REBUILD_2026-10-25.md.

## Implementation

- NativeCaptureBackdrop replaces UIR custom filter passes with an explicit native
  world-space UI capture camera, a depth/stencil capture texture and two owned
  color textures. Their dimensions are the effect support region at full DPI,
  including the full four-sigma kernel. No resolution/effect/quality reduction.
- Gaussian passes run outside repaint callbacks, through native Graphics.Blit.
  Blur sigma, mirror boundary, transform and premultiplied/straight-alpha handling
  are implemented, but pixel correctness is NOT established yet.
- Registry schedules captures in paint order on Application.onBeforeRender so
  later effects can consume earlier output textures. Hiding the run suspends
  its submission; owner destruction releases cameras, panels, layers and targets.
- Capture preserves RenderTexture.active and GL.sRGBWrite across the camera and
  both passes. Resizing has explicit allocation cleanup. Capture layers avoid
  occupied renderer layers and are reserved/released by their effect owners.
- The existing NativeUiCapturePanel gains validated logical-size resizing; old
  callers and the prototype retain their original path.
- This is a transitional producer bridge: SourceUiReplica still prepares the
  paint prefix. It does NOT yet remove tree replication or author all UI into
  shared semantic producer layers. Do not call the whole rebuild complete.

## Gate and tests

NativeBattleDocument._explicitBackdropLayers is serialized and defaults FALSE.
Neither the default prototype nor the authored battle scene enables this
unverified backend automatically. Enable only after visual/order checks pass.

The Editor capture probe now accepts explicitLayers and translucent arguments.
Reference phases switch to the original UIR filter backend, not to another copy
of the candidate. The full filtered patches compare colors, text, clips, resize,
UV/opacity and owner removal. Added transparent-root coverage for alpha handling.
Profiler markers: EchoSphere.UI.NativeEffectCapture / NativeEffectBlur.

Static verification: game core 1383 assertions / 47389 TS-C# comparisons PASS;
Node 272 PASS; project preflight PASS. Android player/editor API typechecks:
339 runtime C# files and 6 Editor files, zero errors/warnings before final review;
final checkpoint typecheck also passed all three configurations with zero errors
and zero warnings. Implementation 9286cee102be83ca4ddea65d4744bd5f9f0c762e:
CI #860 SUCCESS, https://github.com/Lir37/echo-sphere/actions/runs/40222782599.
All five original user render/Android settings file hashes remain unchanged.

## Blocking live verification

Windows was visibly locked during the attempted Editor check. Unity pipeline
requests timed out; no successful GPU probe, new APK, emulator run or performance
measurement is claimed for this backend. The user was asked to unlock Windows.
No forced Editor shutdown, authentication automation or lock-screen interaction.
Original settings/user edits and new-desing are unchanged.

After unlocking: inspect Editor state, stop pending Play, recompile, then run
EchoSphereBackdropCacheProbe.StartProbe(2,true,1,true), followed by sigma8/DPI2.75
and translucent=true. Diagnose any gamma/alpha/pixel-phase discrepancy before
enabling the scene flag. Verify dynamic text, ordered overlapping glass and
resize/disposal, then actual battle/tutorial in Editor and the GLES emulator.
Prior same-emulator ~0.8-0.9 FPS remains the baseline, not a new measurement.

Short continuation: read the three GPT documents and this report. Native ROI
capture/blur backend is implemented, flag OFF; code/API/contracts/CI pass.
After unlocking Windows, finish Editor pixel/alpha/order validation, enable the
experimental scene flag only on success, then test the same APK in the emulator.
