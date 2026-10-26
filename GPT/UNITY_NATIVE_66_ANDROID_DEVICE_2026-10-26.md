# Unity 6.6 Android device acceptance — 2026-10-26

Branch: unity-migration. Measured phone APK source checkpoint:
45d899dda3ef0be018ae3ff2774b12a370d9cecc; CI #868 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40285243448.
User explicitly authorized building, installing and testing on the USB phone.
This supersedes the previous no-APK boundary for this acceptance cycle.

## Measured baseline Android build

- Main UnityProject, Unity 6000.6.1f1, original ECHO SPHERE identity,
  com.lir37.echosphere; authored native entry is the sole enabled scene.
- Matching tools installed: NDK 27.2.12479018, SDK build-tools 36.0.0,
  Temurin JDK 17.0.18+8. No substitution of older engine tools for compilation.
- Existing BuildAndroidDevelopmentApk entry: IL2CPP, ARM64 + ARMv7,
  Development + AllowDebugging; version 0.1.0 / code 1, min SDK 26, target 36.
- Actual BuildReport: Succeeded, 0 errors, 5 warnings; 22m44s.
  APK size 177835552 bytes. BuildReport totalSize includes other outputs and
  must not be reported as the APK file size.
- Original output: UnityProject/Builds/Android/ECHO-SPHERE-debug.apk;
  that path now contains the corrected candidate documented below.
  SHA-256: a9916c6a29ed96b2cbd5e4504a1b898bc6a65f74e86e3200d17794be100fb6e0.
- Warnings: Pipeline runtime config absent (player connection disabled), three
  potentially-uninitialized Blur shader warnings across compiler variants,
  and one large Core static constructor split into its own C++ file.
  These are recorded, not claimed resolved or proven to cause device stalls.

The APK uses the actual working tree, including preserved user render/quality
overrides; CI for the committed checkpoint does not validate those local overrides.
No gameplay, formulas, RNG, save format, artwork or quality settings changed.
Two build-generated serialization-only edits (URP runtime settings cache and an
empty-setting whitespace change) were restored; pre-existing local changes remain.

## Actual phone execution

USB authorization confirmed: Xiaomi model 21081111RG (amber), Android 14,
ARM64, physical 1080x2400, density 440. Phone unlocked by user.
Before installation, no package matching echosphere or echo was installed;
run-as reported the main package unknown. No app uninstall, data clear or save
deletion was performed. Initial thermal status was 0; this is not a gameplay
thermal/performance result.

Two adb install -r attempts returned INSTALL_FAILED_USER_RESTRICTED. The user
then installed Download/ECHO-SPHERE-Unity66-45d899dd.apk manually. The copied
APK's device-side SHA-256 matches. Main package, version/code, ARM64 ABI and
debuggable flag verified through Android after installation. No security setting
was automated. Private pre-test files/preferences backed up before first launch.

Ordinary home/characters/Back, native region selection/start, welcome, movement,
sphere placement, network explanation and generated upgrade choice screen ran
on the phone. Held ADB touch input completed the real movement gate; a sphere
appeared approximately at the tested (300,1100) screen tap. This is one device
input case, not full finger/multitouch or coordinate acceptance. On tutorial
step 3/6, the card/Next button extended below the viewport. Advancing required
tapping its remaining visible edge. User disconnected the phone at the generated
upgrade screen (5/6); full tutorial, sustained combat and save restoration were
not completed. The pre-install package was absent; no earlier main-game save was
overwritten or cleared. No unsolicited region screen appeared in this run;
the intermittent symptom is not declared fixed.

Actual native-scene replay, Vulkan, before external Profiler connection:

| Stage | Frames / seconds | FPS | GC mean / peak bytes per frame |
|---|---|---|---|
| tutorial-0 | 14 / 22.761 | 0.615 | 67491 / 202248 |
| tutorial-1 | 13 / 21.202 | 0.613 | 220909 / 254552 |

Both GC counters were valid. SurfaceFlinger independently reported 126 valid
presents over 164.096 seconds, approximately 0.762 FPS, spanning a longer tutorial
interval. This is not a steady-state 60 FPS result. A single meminfo snapshot
after characters showed 944930 KiB PSS; tutorial later showed 760256 KiB PSS.
Neither is a peak or a proven leak. Thermal status remained 0 during sampling.

USB Unity Profiler connection confirmed through a fresh Android PlayerLoop /
Render Thread capture. Ten warmed tutorial frames averaged 1719.23 ms on the main
thread, including 1657.84 ms in Gfx.WaitForPresentOnGfxThread. Render Thread averaged
1719.69 ms, with 1703.68 ms in GfxDeviceVK.Present. UIElementsRepaintPanels averaged
30.47 ms. GPU timing was unavailable (zero), so Present wait is not a measurement
of pure GPU execution. The stall is primarily in frame presentation/graphics
waiting, not explained by simulation/GC alone. A nonfatal startup log also reports
missing AssetPackManager; the app continued. Its cause is not claimed resolved.

## Confirmed fixes prepared after phone disconnection

1. Retained hidden pause and reward backdrops still executed both Gaussian passes
   every frame. Actual Editor callback probe measured 39-40 calls per pass in
   40-frame cases even when the respective overlay was hidden. NativeEngineBackdrop
   now synchronizes filter attachment with effective hierarchy visibility after
   each runtime presentation update, including early-return menu paths. Owner
   requests, visible fades, shader parameters and native Unity texture ownership
   remain. Hidden filters detach; visible ones reattach. This also covers retained
   menu glass. No visible effect, resolution, MSAA or quality reduction.
2. NativeTutorialView used a fixed 188-unit bottom clamp despite text-determined
   card height. At 1080x2400 / density 2.75, measured card y=684.7273, height=225.8182
   extended beyond logical height 872.7273. Position now clamps using measured
   height, including GeometryChangedEvent during layout. Existing preferred target
   placement remains; text, fonts, sizes, artwork, callbacks and tutorial rules
   are unchanged. Fixed card bottom=860.7273 leaves the authored 12-unit inset.

Editor verification after the fixes:

- Six tutorial/pause/return/rewards/main-menu/run-return cases count actual engine
  callbacks after settling: hidden fullscreen filter calls 0; visible pause and
  rewards each 68 calls (34 per pass); visible tutorial 136 (34 per button/pass).
- Eighteen actual tutorial viewport cases / 93 checks, including density 2.75,
  portrait/landscape, localized text and spotlight resizing: PASS, 0 new errors.
- Repeatable Editor checks: scripts/check-unity-native-backdrop-visibility.cs
  and scripts/check-unity-native-tutorial-viewport.cs. Run after diagnostic
  preference backup; stop Play and restore afterwards.
- Nine populated modal stages / 420 checks, including real catalog cards, hover,
  hit testing and reopen: PASS, 0 new errors. Visible reward blur screenshot checked.
- Actual Unity compile PASS; pinned Unity API C# checks for all three configurations
  0 errors / warnings; project preflight and VisualChecks PASS.
- Windows diagnostic preferences restored and verified, all ten original keys.
  Scene is clean/stopped. Pre-existing local user files/settings remain separate.

**No post-fix Android FPS/GC or phone acceptance is available.** The installed phone
APK is the measured baseline above, not these source fixes. Editor counts prove
removed hidden work, not the size of the device performance improvement.

## Corrected APK candidate

The corrected main development APK was built through the actual Editor after
the checks: Succeeded, 0 errors, 5 warnings, 6m17.54s; signature verification PASS.
Same application/version/code, both ARM ABIs and existing user quality overrides.
File: UnityProject/Builds/Android/ECHO-SPHERE-debug.apk, 232093576 bytes.
SHA-256: 003fb912e6ca2e531069435578d745614f36d6150cee47a7963d9665316cb981.
It has not been installed on the disconnected phone. The earlier measured APK is
preserved outside Git as baseline-45d899dd.apk. Local corrected build report and
source-file hashes are in the device evidence folder. The 32 pre-existing tracked
local edits match their pre-test SHA-256 values; build-only serialization is
cleaned separately, without resetting user changes.

Local build report/receipt are outside Git in
C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-device-20261026.
No private save contents are published.

## Continue

Install the corrected candidate after the phone reconnects, preserving app data.
Repeat comparable native replay/frame/GC captures; complete the generated upgrade,
tutorial, battle, pause/focus and intermittent region checks. Use a fresh diagnostic
profile only with an appropriate backup/restore boundary; never clear real progress
to bypass the replay's saved-run/tutorial guards. Inspect remaining visible filter,
shadow and rendering costs if presentation stalls remain. Do not interpret build,
Editor callback reductions or CI success as Android performance acceptance.
