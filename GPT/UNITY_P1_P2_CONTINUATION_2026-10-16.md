# ECHO SPHERE Unity P1/P2 continuation — 2026-10-16

Source baseline: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934` (protected, unchanged).
Initial Unity baseline: `unity-migration@c5943873f753e056885fea17ce25e10048f4a672`.
Read alongside `ECHO_SPHERE_UNITY_MIGRATION_AUDIT_v1.0.txt` and the three authoritative GPT documents.

## Confirmed bounded change
- `SourceCanvasSurface.Call`'s `fillRect/strokeRect` previously allocated `new List<Contour>(_path)` for each rectangle call.
- The temporary path save now uses an owned reusable `_rectSavedPath` list. The original `_path` and `_contour` are restored in `finally`, even if painting throws.
- No change to stroke/fill selection, geometry, animation cadence, render order, gameplay data or authored effects.
- A Node structural guard verifies buffer reuse and restoration. It cannot measure Unity GC itself.

Implementation commit: `d7e5b8536f32f75f419dc627b1cf26f0065af46f`.
Regression guard commit: `a540f2b8ccea1d76db5d062f1b5e1121dbf299a9`.

## Remaining gates
P1/P2 remain PARTIAL and P3 OPEN. The current code is still hybrid; source-backed native screen/visual replacement must proceed incrementally. Use actual Unity Profiler captures (including EchoSphere.UI.* and EchoSphere.Canvas.*) to identify next hotspots. Check Editor focus/input, visual fidelity and retained memory. APK remains deferred until the user's final combined check. Green CI is not proof of Unity Editor compilation, FPS, or device parity.

CI state is determined by the actual GitHub Actions runs on each commit, not this note. Update the main audit once broader acceptance is available.
