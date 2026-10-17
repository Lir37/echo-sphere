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

### CI regression guard correction
CI #717 and #718 failed in core-contracts because the existing `unity-ui-refresh-cadence.test.mjs` required a literal `AdvanceMotion;UpdateLayout;...` sequence, now separated only by the new `UiLayoutMarker.Auto()` scope. Source and Unity typechecks passed. Updated the regex guard to verify the identical call ordering with the measurement scope included; implementation unchanged. Test correction commit `68d40f412df1fc14b0bba5713a00f90e49cfb794`. Await CI for the corrected checkpoint. Editor run remains the required acceptance check for visible startup and reported severe FPS loss.


## 2026-10-16 — UI Toolkit unallocated texture diagnostic and brush handle reuse
Starting HEAD `ff6781c3f9a406c8894e9bb597e7738a244563d5`; source branch unchanged. User reports Unity UI Toolkit error `Attempted to get a texture (index=3) that is not allocated`, continued top-left first-menu text, and ~1–2 FPS. The previous initial visibility gate has **not** resolved the user's symptoms. This is a user-confirmed observation, not a Profiler measurement.

Confirmed code-level lifetime risk: `SourceUiDocument.EnsureBrush` destroyed/recreated every CSS brush `Texture2D` when the paint key changed, even when dimensions stayed identical. Native UI Toolkit meshes could still reference the previous brush handle. Change: retain the same `Texture2D` at identical width and height, rewrite the existing pixel storage via `LoadRawTextureData`, use `Apply(false,false)` so its CPU storage remains available for later updates. On dimension changes a replacement is still necessary; this and other SVG/depth texture lifetimes require separate Editor confirmation. The possibility of incremental repaints, retained UI meshes and their texture references motivates this targeted change; **it is not yet proven to resolve the exact error**.

Commits `dec224f07f8a6eca0c8b0321895ef9431fd3eb74` and regression test `de85616e394c8b627481117d6a01e1029728fcad`. Check Actions CI separately. Severe menu FPS issue remains OPEN; gather Unity Profiler CPU Timeline, GC Alloc and render texture diagnostics; do not claim FPS gains. P1/P2 partial, P3 open, APK deferred.

### Removing ineffective initial visibility workaround
User's new Editor observation confirms that the initial `Visibility.Hidden`/two-stable-pass gate introduced earlier did not fix the top-left initial placement or severe menu stutter. Removed that gate rather than layering another speculative delay; the existing Unity Toolkit layout, source CSS projection, and profiling remain active. Code `b8f7ea76a4bb26f0bf4eb1efb7f3d40797366bac`, guard `b70b67fbd0b4e39198e2f4de79fb9a7890149f19`. Startup placement and performance are **still OPEN** and must be diagnosed using actual Editor visual observations and `EchoSphere.UI.*` Profiler timing. CI must be verified for the new checkpoint.


### Cold native font startup profiling
Inspection of `SourceUiTypography.Resolve` confirms first-use `FontAsset.CreateFontAsset` calls for primary/fallback faces, including up to nine inline font-weight variations of the primary family. This is a plausible significant Editor startup cost, but remains **unmeasured**. Added `EchoSphere.UI.FontResolve` ProfilerMarker only on an uncached resolution path (after the existing cache lookup); the original font assets, family matching, weights, fallback, rendering, glyph appearance and caching remain identical. Code `423c592be02908bce998b0a31a302b60632d4eb8`, test `bcb5895c9dd101e4689b83e31d4e2c40d31dd098`. Use this marker together with UI BuildTree/Reconcile/Layout/Brushes/SVG markers in Editor to identify actual 1–2 FPS root causes before deciding on font-init refactoring. CI to be checked separately.


### SVG native RenderTexture handle reuse
The source `PrepareSvg` previously destroyed and replaced its `RenderTexture` on every changed SVG fingerprint, even when output dimensions stayed equal. Added an optional existing target parameter to `SourceUiSvg.Rasterize`, preserving the previous RenderTexture object at equal width/height while rerendering the identical geometry and `Graphics.Blit` pass. If dimensions differ the original creation/replacement path remains, and the caller only destroys the previous texture if a new object was returned. On exception the rasterizer destroys only newly created targets, not caller-owned retained ones. Existing SVG appearance, atlas, overflow bounds and shader passes are unchanged.
Code `1725f2a5fca6cca27ddde2aa695a4768548b5797`, caller `67ce3746281be5a3f347dabfa1997f68a286a62a`, guard `f6ca707308b2e91a9cdb4dc25ac35a96b3301acf`. Requires CI and Unity Editor validation. This reduces one source-backed texture-handle churn risk but does not prove the specific index=3 UI Toolkit error is resolved.

## 2026-10-16 — Geometry-event CSS repaint invalidation
Baseline `a4591e37df5b27482cf2bbc232d1b8b563a8f662`; protected source unchanged. Verified source `SourceUiDocument` unconditionally reset `entry.PaintKey=null` on every `GeometryChangedEvent`, even though `EnsureBrush` already derives its fingerprint from dimensions, scale, viewport clip and relevant CSS style. This caused redundant CSS texture rasterization after geometry callbacks for unchanged brush inputs. The event still updates node bounds and increments FrameVersion, but no longer forces PaintKey null. Fingerprint-driven redraw, generated visual content, effects, layout and render order remain in place. Code `3893ab16b37e346f039f4cc522443c35a3c0ace6`, regression guard `f4419caafe55a6106bea4bd0a3a9d97c92dc7dd3`. CI and Editor performance/visual verification required. P1/P2 NOT accepted, menu 1–2 FPS and initial label placement still unverified/open; no APK.


## 2026-10-16 — Actual Windows Unity Editor menu diagnosis

Entry unity-migration HEAD: 6fdce69c6940e638c311c0fb89efc172db11a2e2.
Unity MCP connection to the local 6000.3.24f1 Editor verified; compile and Play Mode exercised.
See GPT/UNITY_EDITOR_MENU_DIAGNOSTICS_2026-10-16.md for measured evidence and remaining gates.
Fixed repeated native sprite-index copies, unresolved Grid auto-row projection, the menu's
self-reinforcing intrinsic-column heights, and redundant backdrop hierarchy/style writes.
Native Editor checks and full portable C#/Node tests passed. Performance remains unacceptable;
final 480x854 sample averaged 118.312 ms/frame and about 5.51 MB main-thread GC.Alloc/frame.
The existing z-index/flex-flow coupling still places the sphere entry below the controls.
P1/P2 remain PARTIAL. Console has earlier repaint assertions; texture index=3 was not reproduced.
No gameplay changes or APK. User local settings/import changes preserved. Check resulting CI.
## 2026-10-17 — Measured backdrop style-write reduction
Entry HEAD f54dd70a39737ef2372016a51bdf868b08d2b5f0 (CI #732 SUCCESS).
Added a retained inline-style writer only for owned backdrop replicas; unchanged
value requests no longer repeat native style writes. Mutable lists still pass
through and getters preserve Unity canonicalization. No effects/cadence removed.
Same-state Editor A/B: full paint preparation 51–61 ms native vs 32–34 ms retained
at 1260x542 with 1979 copies. This is not an FPS measurement.
Final Editor compile, 66305 property comparisons, previous Editor checks, full
portable core/visual checks and 272 Node tests PASS. Settings event navigation
exercised. See GPT/UNITY_REPLICA_STYLE_DIAGNOSTICS_2026-10-17.md for captures,
limits and Console diagnostics. P1/P2 remain PARTIAL: menu performance and
z-index/flex-order bug still open. No APK. Check resulting commit CI separately.