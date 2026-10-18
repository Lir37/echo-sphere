# Unity menu + gameplay performance verification — 2026-10-18

Starting branch unity-migration, HEAD db0768dd773f106a4485797fbe4a4ce1d4f08391. User commits since 38ca129 were present and the working tree was initially clean. Their replica text/state/callback/identity/sibling guards and logical motion scratch reuse were preserved. P1/P2 remain PARTIAL.

## Implemented and verified

- Connected the existing exact SourceCanvasGeometryCache to SourceCanvasSurface.Paint. It was already committed by the user but not used by the renderer. Normal capacity remains 512 records, with independent vertex/index/key budgets; coordinates are not rounded, tessellation options and drawing order are unchanged. A diagnostic Enabled switch permits a controlled uncached comparison; it is true in normal rendering.
- Native text and helper copies without custom paint callbacks now rely on Unity's own text/style/geometry invalidation. Custom callbacks continue MarkDirtyRepaint every pass because they can read changing shared Canvas/SVG/depth textures. No frame-rate throttling or effect removal.
- Updated two stale source guards: Canvas profiler now wraps the cache call, whose miss path still uses native VectorUtils; UI animation fallback assertion now recognizes the user's retained scratch dictionary. No working implementation was reverted to satisfy an old text pattern.

## Real Editor evidence

Unity 6000.3.24f1, CLI beta.12, port7800 confirmed ready. Compile SUCCESS with zero errors/warnings.

- Native geometry comparison: 41155 checks PASS against uncached Unity tessellation, including curves, compound fills, dashes, mutations, all options, eviction, oversize bypass and disabled-cache path.
- Frozen current gameplay render state, fixed diagnostic render random/time bindings restored in finally: full Canvas generation 317.99ms without cache, 13.32ms with 512 entries, 14.97ms with 2048. Exact mesh vertex/color/UV/data/index buffer digests matched. This is a repeated-state generation benchmark, NOT gameplay FPS, GPU timing or complete image parity. Repeatable diagnostic: scripts/check-unity-canvas-live-cache.cs.
- Active gameplay baseline from preceding cycle: 9 frames, 439.18ms/frame (~2.28 FPS), main-thread GC49.02MB/frame, Canvas.Tessellate303.08ms; time9.87s, five enemies, paused=false, gameOver=false.
- Current active 512-entry short sample: six frames, 472.16ms/frame (~2.12 FPS), GC31.47MB/frame, Tessellate221.52ms, UI.BuildTree142.17ms. Different scene population/time and host load mean these samples do NOT prove an overall speedup or regression. Do not present the GC/tessellation difference as a controlled before/after.
- A 2048-entry experiment did not solve dynamic misses (3289 cumulative hits / 6922 misses, 157727 retained vertices). It was not made the default. Many moving/evolving contours produce new exact keys. No coordinate quantization was introduced.
- Settled menu 1260x542: 25 frames, 142.77ms/frame (~7.00 FPS), GC2.087MB/frame; Layout11.02ms, PrepareBackdrops32.85ms. Not directly comparable with prior 480x854 measurements. Initial empty Profiler capture was discarded; startup requires several actual frames before resources/layout settle.
- Menu before/settled-after screenshots had identical PNG SHA256 DB72424B6DD2934D6E4EDD0E28B951346D3586ED5CE0EE2561B5562F5FB4F3C2 for the inspected frame. Active gameplay screenshot inspected with spheres, enemies, field and HUD. Full gameplay/animation pixel parity and physical input acceptance remain open.
- New live native text check PASS: source text then font/color changes caused native replica repaint (two callbacks), without unconditional text repaint. Initial detached-source diagnostic fixture was corrected to attach a measurable source to the native panel before drawing; its initial failure was a harness issue.
- Existing Editor menu checks PASS: 1206 order / 1780 styles / 7806 indices / 24 grid eligibility. Replica snapshot21360 comparisons PASS; visibility360 PASS.
- Node272 PASS; Core1193 assertions +47339 comparisons PASS; full VisualChecks PASS; preflight and metadata PASS.
- Pipeline main-thread diagnostic timeouts occurred and are not game/renderer exceptions. Final interval after cursor1207 had no new Console errors. Texture-index3 not reproduced; old repaint/hot-reload gates remain open. No globally clean historical Console claim.

## Local-state protection

Fresh gameplay PlayerPrefs backup was taken on 2026-10-18 before resuming the saved run; the older backup was retained separately. After testing, all six original game values were restored through Unity APIs and verified, with no additional game keys. No original save/RNG changes are shipped. Play Mode stopped; profiler stopped; own captures moved outside Assets. Existing tracked project/settings/import state was not staged. No APK; protected new-desing unchanged.

## Remaining priorities

1. Active Canvas dynamic tessellation still dominates. Investigate repeated world/local path conversion and native path backend options with exact/visual evidence; do not hide misses through approximate keys or lower tessellation quality.
2. UI.BuildTree/HUD projection remains expensive at low frame rates. Continue audit-guided native UI migration with source behavior/parity checks.
3. Menu backdrop preparation and native panel work remain far too slow. Verify retained invalidation across screen transitions, resizing and animated/custom surfaces.
4. Repeat controlled active-scene and menu captures after architectural changes; inspect CPU Timeline/UI Toolkit/Rendering/GPU as available. P1/P2 are not DONE. Check CI on this group's resulting HEAD separately.
