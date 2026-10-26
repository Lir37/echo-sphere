# Unity 6.6 Android device acceptance — 2026-10-26

Branch: unity-migration. APK source checkpoint:
45d899dda3ef0be018ae3ff2774b12a370d9cecc; CI #868 SUCCESS:
https://github.com/Lir37/echo-sphere/actions/runs/40285243448.
User explicitly authorized building, installing and testing on the USB phone.
This supersedes the previous no-APK boundary for this acceptance cycle.

## Actual Android build

- Main UnityProject, Unity 6000.6.1f1, original ECHO SPHERE identity,
  com.lir37.echosphere; authored native entry is the sole enabled scene.
- Matching tools installed: NDK 27.2.12479018, SDK build-tools 36.0.0,
  Temurin JDK 17.0.18+8. No substitution of older engine tools for compilation.
- Existing BuildAndroidDevelopmentApk entry: IL2CPP, ARM64 + ARMv7,
  Development + AllowDebugging; version 0.1.0 / code 1, min SDK 26, target 36.
- Actual BuildReport: Succeeded, 0 errors, 5 warnings; 22m44s.
  APK size 177835552 bytes. BuildReport totalSize includes other outputs and
  must not be reported as the APK file size.
- APK: UnityProject/Builds/Android/ECHO-SPHERE-debug.apk.
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

## Phone and installation boundary

USB authorization confirmed: Xiaomi model 21081111RG (amber), Android 14,
ARM64, physical 1080x2400, density 440. Phone unlocked by user.
Before installation, no package matching echosphere or echo was installed;
run-as reported the main package unknown. No app uninstall, data clear or save
deletion was performed. Initial thermal status was 0; this is not a gameplay
thermal/performance result.

Two adb install -r attempts returned INSTALL_FAILED_USER_RESTRICTED:
Install canceled by user. The user reported no prompt and is enabling the
separate Xiaomi Install via USB switch. No security setting was automated.
The exact APK was also copied to Download/ECHO-SPHERE-Unity66-45d899dd.apk;
device-side SHA-256 matches. Copying is not successful installation.

**Device execution, menus, tutorial, physical input, FPS/GC and long combat
remain pending until installation succeeds.** Editor checks remain separately
documented in UNITY_NATIVE_66_EDITOR_ACCEPTANCE_2026-10-26.md.

Local build report/receipt are outside Git in
C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-device-20261026.
No private save contents are published.

## Continue

Confirm USB installation permission or manual installation of the already-copied
APK, then install the same artifact without rebuilding or clearing data. Verify
ordinary menu/characters, run the explicit development tutorial replay for actual
device frame/GC counters, then check tutorial control and unsolicited region UI.
Do not interpret build/CI success as Android performance acceptance.
