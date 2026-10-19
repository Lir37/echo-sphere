# Gameplay native checkpoint, 2026-10-19

Entry: unity-migration@aba1b77211e1f069f2f4a5ca61155388cf810dc8.
Menu implementation CI #755: SUCCESS. Native gameplay modernization remains
PARTIAL. Protected new-desing remains 737ef2af4a877e0995db5f92331d536d2f29a934.

## Implemented

SourceCanvasSurface retains authored Unity BezierPathSegment coordinates alongside
the world-space path. When every path operation and paint share an exactly equal
matrix, Unity VectorGraphics tessellates the authored local coordinates directly.
Mixed construction transforms and changed paint transforms use the original
world/inverse path. Close decisions still use the original world-space threshold.
No quantization, cache enlargement, tessellation tolerance, shaders, effects,
gameplay, RNG or quality settings changed. The existing bounded exact cache and
native mesh/GPU backend remain authoritative. This does not remove the V drawing
adapter or complete typed native gameplay presentation.

Added ProjectTree, BuildCssNodes, CopyInteraction and ResolveCss child markers
inside EchoSphere.UI.BuildTree. Instrumentation distinguishes expensive layers
before the next native HUD/pause implementation.

## Evidence and limits

Real Editor synthetic path test: original world/inverse mode misses all 80 moving/
rotating curved strokes; local mode has 1 miss and 79 hits. Identity fill/stroke
meshes match exactly for lines, cubics and arcs; mixed construction, changed paint
and singular paint cases keep exact old mesh buffers. This is a microbenchmark,
not gameplay FPS. Existing native geometry-cache suite passes 41155 comparisons.

The existing saved run was restored through ContinueSavedRun, retaining its user
pause. Render-only random/time were frozen for a Canvas A/B raster check and
restored in finally. One pixel differs by 1/255; whole-image mean byte error is
0.0000003661. Frozen second Canvas draw was 10.43 vs 12.95 ms: this capture does
not establish a speed improvement. Repeated local geometry is stable under motion
in the dedicated microtest; the frozen scene already hits most old cache entries.

First paired Editor timeline, 1260x542, same paused saved-run simulation, 15 valid
frames/mode: world-roundtrip 389.694 ms / 2.566 FPS / 60,195,560 B main-thread
GC.Alloc per frame; retained-local 384.423 ms / 2.601 FPS / 60,184,961 B. These
short samples show no material full-scene improvement. BuildTree is 209-211 ms,
Reconcile 48-50 ms, Layout about20 ms; Canvas.Tessellate is below1 ms while paused.
Do not present this as active gameplay or Android performance. The earlier active
~2 FPS/221-303 ms tessellation evidence remains a separate capture.

Raw current results: UNITY_CANVAS_LOCAL_CONTOURS_EVIDENCE_2026-10-19.json.
Profiler contour flags label modes only; neither mode is fully native gameplay.

Second paired capture with child markers, 15 valid frames/mode: 379.607 vs
376.815 ms; GC60,185,373 vs60,185,745 B/frame. ResolveCss accounts for
192.807/188.879 ms, ProjectTree9.726/10.159 ms, BuildCssNodes0.587/0.627 ms,
CopyInteraction0.154/0.148 ms. Thus the full CSS cascade, rather than component
construction alone, dominates this paused interface. The next native UI work must
remove that cascade from gameplay presentation. Rechecked frozen raster still
differs in only one pixel by one byte; second draws9.918/11.217 ms.

Preferences were backed up before entry (raw registry safety copy in ignored
UnityProject/Temp plus typed Unity API values in Editor SessionState). Stop is
followed by restore-unity-gameplay-diagnostic-prefs.cs, which restores original
strings/ints/float, removes diagnostic-created game keys, saves and verifies all
six original game keys without printing values. Recovery assets remain untouched.
Background execution is restored false. Two Pipeline eval main-thread timeouts
appeared during the first slow scene capture; they are tooling errors, not a clean
Console claim or evidence that texture-index-3 is resolved.

Core1193/47339 PASS; full local VisualChecks PASS; project/provenance PASS; Unity
recompile SUCCESS without compiler errors/warnings. CI must be checked for the
actual resulting implementation commit separately.

## Next

Use the new child markers and original SourceGameView/Hud/MobileControls/pause
components to build retained typed native gameplay UI with matching visuals,
state and controls. Preserve all source transitions and callbacks. Do not enable
approximate legacy renderers or hide effects to obtain a faster capture. Then
measure an actual active saved-run state separately from paused UI and frozen
Canvas tests. Complete Android/device and APK gates only at the final stage.
