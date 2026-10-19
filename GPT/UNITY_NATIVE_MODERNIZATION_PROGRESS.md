# ECHO SPHERE — native visual modernization

Current status entry point, 2026-10-19. Historical reports retain earlier evidence;
this file and UNITY_NATIVE_MENU_COMPOSITION_2026-10-18.md define this checkpoint.
Entry HEAD: `26765b508a15ad3155b62e681af91d505e5cf7e8`, branch `unity-migration`.
Protected source: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation HEAD is the commit containing this report (`git log -1 --format=%H`).

## Acceptance status

**P1/P2 and full native modernization remain PARTIAL. Native Home is now selected
by production and passes its bounded Editor acceptance.** Cold Home creates no
source DOM/CSS document or source Canvas. Gameplay still uses the source drawing/
UI projection and remains the next performance/migration priority. Android device
performance and complete device visual/input parity have not been established.
No APK, gameplay, formulas, balance, RNG or quality setting changed in this group.

The three gameplay GPT documents, continuation notes and migration audits were
consulted. Seven recent project chats were checked; opaque/unavailable content is
not implementation evidence. The user's current target is native Unity, not a
browser/Edge comparison project. No browser was launched for this work.

## Native menu checkpoint

- NativeMainMenuView owns retained UXML/USS Button/TextField layout and a typed
  change-only state. NativeMainMenuScreen/Composition now supply and select the
  complete background, core/orbits, avatar/glyph, typography, shadows and glass.
- Six original Lucide VectorImages use their source geometry; no runtime SVG
  rasterization. GPU background preserves gradients, grid, alpha and radial mask.
- NativeMenuBrush supplies typed GPU fills, borders, gradients and outer/inset
  shadows. NativeMenuArtwork retains five orbit planes/dots, the core and blurred
  highlight, using the existing Unity mesh/depth backend. Source important
  transforms suppress the earlier orbit tracks; no invented rotation was added.
- NativeMenuGlass prepares image/solid prefixes and mirrored separable Gaussian
  passes before UI rendering. Cropped targets cache unchanged output. Text,
  text-shadow and vector overlap use the complete retained UI Toolkit prefix;
  visible layers are never omitted. Those filters are disposed when hidden.
- Rejected separate UIDocument prefix targets read an older frame and mishandled
  premultiplied/straight interchange. The accepted direct GPU path passes actual
  phase and independent frozen-pixel tests; it does not capture a live render pass.
- Native layout retains source grid intrinsic sizes, gap/width behavior, safe
  insets and the surviving stage height `calc(100% - 50px)`. Initial presentation
  waits for stable layout. No unbound text tree is shown at the screen origin.
- Existing map/page/continue actions, character data, preference keys, difficulty
  IDs and UTF-16 name limits remain authoritative. The source Menu name mount
  persistence is retained even when no source document exists.
- Hidden source UIDocument chains continued GPU filtering with opacity zero or
  display:none. Suspension now disables that document while environment hooks/
  timers tick. Resume configures its new root and reconciles retained content,
  including root pointer callbacks. Actual page and Back controls verify resume.

Details and measurement limits: `GPT/UNITY_NATIVE_MENU_COMPOSITION_2026-10-18.md`.

## Measured Editor result

Unity 6000.3.24f1, Radeon 760M, Direct3D12, Game View 1260x542. Paired warmed
Profiler capture in the same run/state; FPS is reciprocal mean frame time:

| Home renderer | Valid frames | Mean frame | FPS | Main-thread GC.Alloc/frame |
| --- | ---: | ---: | ---: | ---: |
| Source projection | 14 | 150.3738 ms | 6.6501 | 1,944,778 B |
| Selected native composition | 17 | 16.6375 ms | 60.1052 | 2,640.6 B |

Native composition preparation: 0.01328 ms average. This is a short Editor result,
not Android FPS or a guarantee of every device/resolution. Complete scene input,
resource lifetime and device parity remain separate from compile/CI success.

## Verification

Unity recompile: SUCCESS, zero compiler errors/warnings. Core checks PASS:
1193 assertions and 47339 TypeScript/C# comparisons. Node272 PASS. Full local
VisualChecks PASS against existing drawing/UI/CSS/animation/brush/backdrop/mask/
image/interaction references. Those CPU checks do not replace GPU tests.
Project, provenance, script/icon metadata and VectorImage export checks PASS.

Actual Editor fixtures pass:
- 9 native navigation callbacks, 4 difficulties, name editing/limit, silent model
  binding, RU/EN, saved-run visibility, retained tree and viewport restoration.
- Complete composition at 1260x542@1, 480x854@1, 360x800@1.75 with safe insets,
  854x480@3, 800x360@3 and 320x568@2. Character/difficulty overlap is checked.
- 7 real native destination buttons plus authored Back controls. Legacy roots
  recreate/populate on six source pages; Settings keeps its existing native owner.
- 24 independent GPU glass references: sigma4/6/8/12, DPR1/1.75/3, opaque and
  transparent; worst mean .5635/255, max4/255 in premultiplied RGBA. Existing
  backdrop tolerances remain mean<2, max<10; no acceptance threshold was widened.
- 32 image phase frames and 32 overlapping-text prefix frames: zero pixel error
  against their corresponding complete/fixed native frames.
- 12 artwork GPU/source brush cases; 12 menu-surface normal/hover/active cases;
  artwork geometry/depth/blur/cache/resize/DPR/disposal; 192 native Image replica
  transitions and 360 repeated subtree visibility checks.

Clean Play/Stop resource check on 2026-10-19: 78 owned native targets while open,
zero after Stop. Earlier hot-reload orphans were released separately.

Historical Console logs contain CLI timeouts and unsupported in-Play assembly
reload experiments. Acceptance uses cold Play entry; do not label historical logs
clean or claim the intermittent texture-index-3 issue universally resolved.
CI SUCCESS, C# compilation, Play Mode, measured performance and device parity are
independent gates. Check Actions for the actual resulting HEAD.

## Remaining gameplay architecture

Outside native Home and existing native Settings, LateUpdate still projects game
state into dynamic V objects, executes generated drawing/UI code and submits
Unity meshes/materials/targets/UI Toolkit elements. Native backend use alone does
not remove boxing, DOM/CSS evaluation, reconciliation or dynamic tessellation.

Earlier redundant legacy visual producers were guarded while metadata/combat
setters were retained; do not repeat that work or turn off source effects. The
active renderer still consumes runtime/agent state. Existing exact bounded
geometry cache preserves topology/options, but active motion still misses it.
Earlier frozen-state Canvas generation 317.99→13.32 ms is not active gameplay FPS.

`GPT/UNITY_MENU_GAMEPLAY_PERFORMANCE_2026-10-18.md` retains the actual active-run
~2 FPS evidence and original markers (Canvas.Tessellate and UI.BuildTree dominate).
The world→inverse-local roundtrip is now bypassed when construction/paint matrices
are exactly equal. Authored local Bezier coordinates feed the existing native
VectorGraphics cache; mixed transforms retain the previous path. Actual Editor
moving-curve check gives79hits/1miss vs0hits/80misses; paused saved-run raster
differs in only one pixel by1/255. This is not complete typed native gameplay.

Actual saved-run **paused simulation** A/B remains about2.6Editor FPS, GC60MB/frame;
no material scene improvement is claimed from this contour change. Child markers
identify the dominant cost: full ResolveCss189-193ms, ProjectTree about10ms,
BuildCssNodes below1ms. Native retained HUD/pause UI, removing the CSS cascade,
is the next architectural priority. Active simulation and Android need separate
measurements. Menu implementation commitaba1b77 has CI#755SUCCESS.
Details/raw evidence: UNITY_GAMEPLAY_NATIVE_CHECKPOINT_2026-10-19.md and
UNITY_CANVAS_LOCAL_CONTOURS_EVIDENCE_2026-10-19.json. Preferences were restored
and verified through Unity APIs after diagnostic Stop; recovery assets untouched.

The inventory JSON scans every C# file, not only visual filenames. Discovery tags
and hashes are not semantic parity or completion claims. Keep it current when
adding native presentation classes.

Next work:
1. Typed native gameplay/path/mesh presentation, then retained native HUD bindings,
   starting with the confirmed tessellation and UI tree costs.
2. Equal-condition active scene captures, CPU Timeline/GC, Rendering/UI/Memory and
   portrait/landscape controls after each meaningful group. Keep effects intact.
3. Preserve native menu/Settings lifetime and navigation; retain explicit device
   gates. Do not build APK until the final modernization stage.
4. Commit each finished group and verify its GitHub Actions result independently.

Short continuation prompt: Continue ECHO SPHERE on unity-migration at the actual
HEAD containing this report. Read the three GPT gameplay documents and this file.
Native Home is selected: complete native composition and Editor controls/GPU/
responsive/navigation checks pass; paired Home capture is ~6.65→60.1 Editor FPS,
GC1.94MB→2.64KB/frame, not device proof. Continue typed native gameplay/path and
HUD migration from actual source and Profiler; preserve visuals/gameplay/RNG and
new-desing. P1/P2 PARTIAL; no APK or browser experiments. Check CI for actual HEAD.
