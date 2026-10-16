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


## Follow-up: native Canvas allocations
Starting HEAD: `5d8d0aa3b543dad702a370104dcc4b67b6ab7d7c`.
- `SourceCanvasSurface.Quad` no longer allocates temporary four-element point and UV arrays. It emits the same four vertices in the same order, preserving the original UV assignments and triangles.
- `setLineDash` duplicates an odd dash sequence into the already allocated dash list instead of allocating a second intermediate list. This retains the original doubled pattern and state assignment.
- A Node structural regression guard protects ordering and the absence of these two allocation patterns.
- Code: `f166e5bfc79e880107165ef5745a24c2d98045bf`; test: `b2d50836f0510d0910e2725790847e90e9f8f7e8`.
- No effects removed, no gameplay changes, no source branch modifications. Remaining P1/P2 and Profiler acceptance gates are unchanged. CI status must be verified from Actions.

## Quad allocation hardening
Replaced the quad-local corner writer with a noncapturing instance method `AddQuadCorner`, avoiding a possible compiler-generated closure in addition to the former two temporary arrays. Corner order, transforms, colors, UVs and triangle indices are unchanged. Fix commit `843e36d756aeaf10858837045a42dcc39f83746e`; test adjustment `095c5f2334ce1d3386c0a08407e941d3db85a202`. Final CI must be checked independently.


## 2026-10-16 continuation: Canvas roundRect radius allocations

Entry HEAD: `8615aec42b52b090d586b8d2cbbcfebd169f883e`. Baseline `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934` unchanged.
- SourceCanvasSurface.RoundRectangle previously allocated a List<float> on each call for one to four radii. It now reads existing List<V> entries directly, or the scalar V.Number, maintaining the original CSS-style radius fallback mapping (a, b, c, d), invalid-count guard, negative dimensions, shrink calculation and ellipse sequence.
- The intermediate captured local helper was removed to avoid introducing a possible closure allocation.
- Regression test `tests/unity-canvas-roundrect-allocation.test.mjs` guards these invariants.
- Code commits: `038b5b84076be1e6a88f3afe83364ee5b222da99`, `c54e6537c129846d31ed455b7a438589cc5e0ea2`; test `7178cc24d15ac978c5c9d4a14b3caec6a6c6de63`.
- Static/code tests are not evidence of reduced measured GC, Editor correctness or APK parity. P1/P2 PARTIAL, P3 OPEN. Profiler sampling remains the next decision gate; APK is deferred.

### CI correction for RoundRectangle
CI #702 revealed C# CS0173/CS0021: `V.Items` is an `IEnumerable<V>`, not an indexable list. Corrected the direct access to use `radii.Raw as List<V>`, matching the original source type guard. Fixed code `e7a2f87ee08be6fef9230a94b7d67e661f65cf16`, corrected test `86b55c99d33cb87b3e94be74eddef31bc004b355`. Re-run CI required. No changes to radius values or rendering behavior.


## 2026-10-16 — Canvas shader property and profiler scope slice
- Entry HEAD `f6a4bde9c41dad6c4419ca0f3e75591a2f8f277b`; protected source unchanged.
- Converted eight repeatedly assigned Canvas shader properties from string identifiers to static `Shader.PropertyToID` integers, using Unity's existing integer overloads in MaterialPropertyBlock, Material and CommandBuffer. Shadow temporary RenderTexture IDs remain unchanged.
- Added `EchoSphere.Canvas.Tessellate`, `EchoSphere.Canvas.ShadowCommands` and `EchoSphere.Canvas.ExecuteCommands` markers so an Editor capture can distinguish path tessellation, shadow command generation and command buffer execution from prior EndFrame/DrawBatches markers. These do not establish GPU timings on their own.
- Original shader property names, rendering arguments, command order, mesh data, visuals and gameplay are unchanged.
- Code commit `cc89be7494c88674599f7348409b55b24bf512dc`; structural guard `945e104fa0709ee7ca9c4162f31ab74f2507ace9`. CI status must be verified against Actions, including Unity typecheck. P1/P2 PARTIAL; P3 OPEN; APK deferred.


## 2026-10-16 — P1 UI interaction carry-over allocation
- `SourceUiDocument.CopyInteraction` constructed a filtered child list via `Where(...).ToList()` every recursive invocation. Replaced it with direct filtered iteration over the pre-existing child collection. For unkeyed children the previous after-index mapping (including index gaps for pseudo children) is preserved; keyed children still select the first matching non-pseudo source child. Hover/Active/Focused copying and recursive walk remain unchanged.
- Code `ede508af1cad9e3a307c5821964816330d65ffe1`, structural guard `10529ff2a6c894a8eeffc88eb627f8cbb1f5af44`. Runtime state parity and reduction of GC remain subject to Editor validation.
- CI needs to be verified for this code/test checkpoint. No native Settings, source gameplay or APK changes. P1/P2 remain PARTIAL, P3 remains OPEN.


## 2026-10-16 — Retained Canvas dash scratch (P2)

Entry HEAD: `e492ebbc117ac7eb20a4df835c12f5ff527db850`. Source baseline remains protected.
- Verified `SourceCanvasSurface.Call("setLineDash")` was allocating a temporary `List<float>` each call and then an independent `float[]` snapshot. Kept `State.Dash` as a distinct owned array, preserving the behavior of `save/restore` and prior snapshots, while reusing an instance-owned `_dashScratch` for collecting source values and doubling odd sequences.
- This is an allocation-only source-backed change, not a renderer replacement, shader-effect reduction, or new gameplay behavior.
- Code commit `78f3b0721f3b66c144a89fe330d02fc0fa66973a`, updated test `2859f73919068b840d145a1b5f9d1dda779676c1`.
- Confirm via current CI and Editor Profiler. P1/P2 remain PARTIAL; P3 remains OPEN. APK deferred.


## 2026-10-16 — P1 retained UI paint-order sorting

Starting HEAD: `ab07259c02d691f49de185dc29aa0f18e6192858`; source `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934` not changed.

`SourceUiDocument.Reconcile` previously ran `Where(...).OrderBy(...).ThenBy(...)` for each element's native paint/picking reordering. It now fills and clears a document-owned `List<(VisualElement, double, double, int)>`, sorts by the same CSS `order` and `z-index` values with original child index as an explicit stable tie-break, and calls `BringToFront` in the same order. The scratch list is filled only after recursive children have been reconciled. No change to CSS property formulas, layout, input event handlers, gameplay, source or visual effects is intended.

Code commit: `50d9d5931490b4ac041b1a6be30a5610b64fd47c`. Structural guard: `9ec588d611b1f20f3c761f9fa36b87bb92186faa`. Unity Editor CPU/GC impact and resulting pick/render-order parity remain to be confirmed; static CI does not prove either. P1/P2 partial, P3 open, APK deferred.


## 2026-10-16 — main-menu first layout jump and severe startup stutter (user observation)

Baseline `unity-migration@dd8d46280780d31344b1b6ef92624237d4aec97c`, protected `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`.
User observed in Unity Editor: first menu text appears clustered at top-left and then moves to its intended coordinates, together with approximately 1–2 FPS at startup. This is a user-reported runtime observation, not a captured Profiler trace.

Source-level finding: `SourceUiDocument.Draw` builds a new UI Toolkit tree and computes positioned-element offsets in `SourceUiLayout.Apply` before UI Toolkit's asynchronous first Yoga geometry pass. Existing `GeometryChangedEvent` callbacks then update element bounds in later frames. Initial unresolved coordinates can therefore be displayed while layout converges.

Change: UI root starts with `Visibility.Hidden`, which preserves layout computation rather than using `DisplayStyle.None`. The existing child `GeometryChangedEvent` observes initial geometry. Once no further geometry changes are observed for two document update passes, `RevealAfterFirstLayout` switches the document root to `Visibility.Visible`. UI update/layout, source animation logic, painting, source effects and input handlers remain in place. This specifically prevents unresolved initial placement from being drawn; actual Editor validation is necessary to check that the menu becomes visible promptly and navigation works correctly. It is NOT a proven fix for 1–2 FPS.

Additional diagnostic work: `EchoSphere.UI.BuildTree`, `EchoSphere.UI.Reconcile` and `EchoSphere.UI.Layout` Profiler markers split the first UI update work from existing `PrepareBrushesAndSvg`, `PrepareDepth`, `PrepareBackdrops`, `RasterizeCss`, and `RasterizeSvg`. Original operation order is preserved. Capture first-menu CPU Timeline/GC Alloc in Unity Editor to determine the cause of severe stutter before attempting broad renderer replacement.

Commits: `e53ec991f739498b6f0bbce20dddf0c54206c05e` initial presentation gate; `d69e56b13f0f85e5fa55ac5066475d316aa1552f` gate test; `b7ccb452dc4d040bb8fcbd5863245c1be4e69447` new markers; `4dbd834c00e8b42914ab0356087a32ee828aa8ca` marker guard.
Code-level CI must be checked for these commits. P1/P2 remain PARTIAL and P3 OPEN; actual Unity Editor initial-menu behavior and FPS are not yet verified; APK remains deferred.
