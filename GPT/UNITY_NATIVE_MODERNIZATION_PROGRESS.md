# ECHO SPHERE — native visual modernization

Current status entry point, 2026-10-20. Historical reports retain earlier evidence;
this file and UNITY_NATIVE_MENU_COMPOSITION_2026-10-18.md define this checkpoint.
Entry HEAD: `60fc8dedaab726b215d87c1164c14555e2a54479`, branch `unity-migration`.
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

Native HUD checkpoint (entry a625334, CI#757 SUCCESS): NativeHudView/RadarView/
PoiArrow now provide retained native controls, bars, telemetry, vector POI arrows,
Gaussian effects and pointer/timer behavior. This is a PILOT candidate and is
**not selected by production**. Existing SourceGameView still owns gameplay UI;
no full-scene FPS/GC improvement is claimed. Bounded Editor tests:4409 authored
radar comparisons,39 GPU/source brush cases,9 native vector/drop-shadow cases,
six viewport/DPR cases and actual pointer/timer checks. Shared menu brush defaults
remain unchanged; its12 existing GPU cases pass. Complete parity, pause/mobile
composition, gameplay integration and active measurements remain required.
See UNITY_NATIVE_HUD_CHECKPOINT_2026-10-19.md and its raw evidence JSON.

The inventory JSON now scans213 C# files, not only visual filenames. Discovery tags
and hashes are not semantic parity or completion claims. Keep it current when
adding native presentation classes.

Next work:
1. Complete/integrate the native HUD candidate with correct overlay ordering;
   continue native PausePlanner/mobile controls and typed world presentation.
   ResolveCss189-193ms remains the confirmed paused-interface bottleneck.
2. Equal-condition active scene captures, CPU Timeline/GC, Rendering/UI/Memory and
   portrait/landscape controls after each meaningful group. Keep effects intact.
3. Preserve native menu/Settings lifetime and navigation; retain explicit device
   gates. Do not build APK until the final modernization stage.
4. Commit each finished group and verify its GitHub Actions result independently.

Short continuation prompt: Continue ECHO SPHERE on unity-migration at the actual
HEAD containing this report. Read the three GPT gameplay documents and this file.
Native Home is selected: complete native composition and Editor controls/GPU/
responsive/navigation checks pass; paired Home capture is ~6.65→60.1 Editor FPS,
GC1.94MB→2.64KB/frame, not device proof. Native HUD/radar candidate has bounded
Editor/pixel/input checks but remains unselected. Read its 2026-10-19 checkpoint;
complete parity and native gameplay/pause/mobile composition, then active profiling.
Preserve visuals/gameplay/RNG and
new-desing. P1/P2 PARTIAL; no APK or browser experiments. Check CI for actual HEAD.

### HUD API correction, 2026-10-19

CI758 rejected two obsolete UI Toolkit calls (warnings, not C# errors).
SetDashPattern and FocusController.IgnoreEvent replace them without suppressing
checks. Local real-Unity-DLL typechecks zero errors/warnings; Editor recompile
and repeated six-viewport HUD pointer/timer fixture PASS. Raw evidence is in
UNITY_NATIVE_HUD_EDITOR_EVIDENCE_2026-10-19.json. Native HUD is still PILOT.
Next: full RU/EN parity and gameplay composition with pause/mobile/overlay order;
then typed gameplay views and actual CPU/GC measurements. No APK.

## HUD vectors in native blur prefixes, 2026-10-19

Entry47bac8b6, CI759 SUCCESS. NativeMenuGlass omitted custom Painter2D meshes
from the fast image prefix and supplied no custom painter to the retained prefix.
A real Editor GPU fixture with actual NativeHudPoiArrow/NativeHudDashBorder
reproduced max170/255 and mean1.499 byte error before correction.
The image path now delegates custom meshes to Unity's retained prefix and
forwards their authored generateVisualContent; native Image/Text callbacks stay
owned by their copied element to avoid double rendering. Blur stays enabled.
After correction: eight changing-image samples each at DPR1/1.75/3, max0/mean0;
existing text+changing-image regression32 samples max0/mean0. C# player/editor
checks zero warnings/errors; Editor compile PASS; Node272/preflight/provenance
PASS. Stop restores six original preference keys and leaves zero named prefix
RTs. No new FPS/GC/Android claim; HUD still PILOT and unselected.
Raw evidence: UNITY_NATIVE_HUD_VECTOR_BLUR_EVIDENCE_2026-10-19.json.
Next: full RU/EN model/pixel parity, native gameplay composition preserving
pause/mobile/modal order, then typed gameplay visuals and actual profiling.

## Explicit HUD display projection / RU-EN oracle

Entry0962dec02ca99839391e766a70237038471003ab, CI760 SUCCESS. The existing
ReadNativeHudState now delegates to an explicit internal display projection
(scene snapshot, language). This extracts the same expressions and source
helpers without changing formulas or connecting the pilot to production.
The Edit-mode oracle compares26 cases /420 checks:2 frozen original-source
HUD fixtures and24 generated original Hud cases. RU/EN text, HP rounding,
XP/HP/resonance clamping, resonance ready color/counter, sphere/wave counters,
time, formation names, boss labels/visibility, region modes and pocket fallback
match. Fixtures are cloned display data; no runtime GameState, preferences or
active source hooks are modified. Script refuses Play or an existing hook tree.
Real Editor compile and local player/editor C# typechecks pass without warnings.
Raw evidence: UNITY_NATIVE_HUD_MODEL_EVIDENCE_2026-10-19.json. Full pixel parity,
production gameplay composition and active performance measurements remain open.
No new FPS/GC or Android claims; HUD remains PILOT, P1/P2 PARTIAL.

### Native HUD pixel tracking / line boxes

Entry959b619 (CI761 SUCCESS): corrected standard TextCore tracking units locally
in NativeHudView, single-line boxes and source meta-counter0.13em spacing.
Measured2px tracking gives exact14px advance on8 chars at8/12/24px. Six original
full-game viewport cases give max geometry delta11.328→2.406px;48 authored
font-size/tracking checks pass, remaining pixel/font geometry is still open.
Six-viewport retained HUD pointer/timer fixture103checks and model420checks PASS.
Native HUD remains unselected; no FPS/GC/device improvement claim. Rejected
Advanced Text Generator and isolated source viewport experiments were reverted.
See UNITY_NATIVE_HUD_TYPOGRAPHY_2026-10-19.md and its raw evidence JSON.

### Native HUD in the real game pane (diagnostic opt-in)

Entry69d7de1 /CI762 SUCCESS. Actual saved-scene same-pane island checks26PASS:
source HUD73→2 nodes, paused tree442→371; source tutorial anchor, pause occlusion,
custom painter in blur prefix, original resume/mobile-pause callbacks and
disable/re-enable pass. Flag defaults false. Paused A/B/A/B: combined Editor
3.49→3.89FPS, GC60.5→52.6MB/frame; ResolveCss still about123ms. Not normal
gameplay performance or an active/device measurement. Source world, pause and
mobile CSS/V remain. See UNITY_NATIVE_HUD_ISLAND_2026-10-19.md and evidence JSON.

### Typed pause-stats data

Entry63b9b63: NativePauseStatsState /EchoSphereNativePauseStats now project8
character stats and10 sphere cards without DOM/CSS or copied IMGUI formulas.
18 original-source RU/EN cases /1760checks and actual saved-run17checks PASS.
This is data foundation only: native pause widgets/other3 tabs remain open.
See UNITY_NATIVE_PAUSE_STATS_2026-10-19.md and its raw evidence JSON.

## Native pause stats content, 2026-10-20

See UNITY_NATIVE_PAUSE_VIEW_2026-10-20.md and its evidence. Native retained
content replaces stats descendants inside the existing ScrollView; default false.
44 actual saved-scene checks and42 RU/EN viewport checks PASS. Paused A/B/A/B
with native HUD held enabled: CSS nodes352->144, combined3.27->5.45FPS,
GC50.8->27.6MB/frame. Still insufficient; no active/Android claim. Native HUD,
stats and remaining gameplay views require further acceptance/migration.
Editor reconnect/compile and new six-key prefs restoration confirmed; previous
interrupted-session restoration cannot be confirmed. No APK/quality changes.

## Native pause abilities, 2026-10-20

See UNITY_NATIVE_PAUSE_ABILITIES_2026-10-20.md. Verified original-helper model
(56 cases/5944 checks), retained native ability/passive cards in existing pane.
42 saved-scene,39 viewport and17 acquired/passive lifecycle checks PASS. Frozen
wide card/list dimensions109/983px matched. Paused A/B/A/B3.59->6.01FPS and
39.1->23.8MB/frame; system memory pressure limits absolute timing, no active or
Android inference.219 C# files; defaultfalse, P1/P2 PARTIAL. Artifacts/synergies,
mobile controls and world rendering remain. No APK or quality/gameplay changes.

## Artifact groundwork and current native Home, 2026-10-20

See UNITY_NATIVE_ARTIFACT_GROUNDWORK_2026-10-20.md. Original artifact/set/glyph
data40 cases/10192 checks PASS (all82 artifacts). Native angled gradient5 GPU
cases and12 existing menu brush cases PASS. NativeArtifactGlyph is an unwired
candidate: paired DPR1/2 RGB means1.86%/4.42%; full visual gate NOT passed.
Current production native Home:59.94FPS /202KB allocated-frame,120 samples at
1260x542, Timeline off, no source UI document; current Editor measurement only.
222 C# files, compile/typechecks/Node272/preflight/provenance PASS. Diagnostic
source-glyph comparison caused one Pipeline timeout error; game renderer fix is
not inferred. Artifact card/set badge composition, synergies/mobile/world remain.

## Native artifact cards/set badges, 2026-10-20

See UNITY_NATIVE_ARTIFACT_CONTENT_2026-10-20.md. Native retained cards/glyph
candidate/set Buttons now compose in existing ScrollView, defaultfalse. Badge
224checks, copied82-artifact/50-button seven-case862checks, abilities39-check
regression and actual empty saved-run21checks PASS. Empty CSS149->144->149.
Actual bound set callback opens original detail modal; Close/blocking verified,
callback invoked directly because saved inventory is empty.225 C# sources,
compile/typechecks/Node272/preflight/provenance PASS. NativeUiLineBoxes shared
without behavior change. Full glyph/device/populated pointer gates remain.

### 2026-10-20 — native pause synergies diagnostic candidate

Entry60fc8dedaab726b215d87c1164c14555e2a54479. All five source sections now
have a typed display model and retained native Yoga/TextCore card/tag view.
Original helper oracle64 cases/41,440 checks; seven native RU/EN DPR1/2 layouts
2,688 checks including201 CSS/native weighted pulse samples; actual saved-run
same-pane scroll/original tab/disposal/restoration20 checks PASS. Native synergies
body710x7360px; source stats352 -> native synergies144 -> source stats352 nodes.
This is a cross-tab lifecycle count, NOT a synergies A/B measurement. Cold source
synergies baseline stalled Editor requests >30sec; recovered by normal Stop and
verified6 preference keys. Diagnostic route removes compatibility body before
native warmup to avoid the cold source card rasterization. Flag defaults false;
full pixel/transition/input/device parity still open. Actual Editor compile and
real installed Unity APIs228C# checks PASS; Node272/preflight/provenance PASS.
See UNITY_NATIVE_PAUSE_SYNERGIES_2026-10-20.md and three evidence JSONs.
Native pause shell/mobile controls/world and artifact glyph gates remain next;
P1/P2 PARTIAL, no protected source/gameplay/quality changes, no APK.

The synergies group also removes confirmed discarded source-tree construction:
Unity passes optional native body ownership before the original component builds
its content; original GameScreen alwaysfalse. Mechanical C# regeneration and
8RU/EN/all-tab exact-shell checks24assertions PASS. No browser or pixel references
replaced. Sequential paused native captures before/after early skip:5.719->5.850FPS,
29.089->27.022MB/frame, ProjectTree5.426->.934ms. BuildTree50.781ms includes
ResolveCss49.702ms; do not add nested markers. Overall improvement limited and
performance still unacceptable. P1/P2 remain PARTIAL. See synergies report/evidence.

CI follow-up for5d5f065: #769 source and Unity typechecks succeeded, but CSS
provenance failed on the old App.tsx input hash. Regenerated with locked packages;
all1200 rules/22timelines identical, original CSS bytes retained. Complete visual
CI step now passes locally; follow-up commit updates only the manifest and reports.

### 2026-10-21 — native pause header/footer candidate, off by default

Entry608b30f929aaa3e0741a2c11e5532f63518f7130. Native labels/seven Buttons/five
source VectorImages and exact original callbacks replace only owned chrome children.
Fixed retained host reparenting when original detail changes footer source path.
Source144 ->86 ->144; model160cases/3040checks, view-only layout509checks,
actual blur-enabled lifecycle47checks, original Return/map reload7checks.

Full cascade inspection caught missing button blur8 and two footer shadows.
Restored seven filters through the existing backdrop pipeline and footer artwork
through existing NativeMenuBrush. This is PARTIAL, not a fully native compositor.
Earlier performance captures without blur or footer shadows are PROVISIONAL and
excluded from equal-quality acceptance. Isolated/actual-panel pixel capture failed;
no pixel parity claimed. Pilot remains false; Android/mobile, scrollbar, full
composition/effects and world views remain. No gameplay/protected branch/APK changes.
See UNITY_NATIVE_PAUSE_CHROME_2026-10-20.md and associated evidence for current gates.
Latest actual-panel contrast control now passes; sigma8 candidate still fails smoothing (.854902 contrast). Blur gate OPEN/FAIL, production off. Final Editor lifecycle47 and Return/reload7 PASS; six preferences restored; Console0errors/1warning. Candidate paused CPU5.04->6.29FPS, GC24.57->17.72MB/frame; no equal-quality acceptance.232 C# and272 Node checks pass. Next: fix/validate blur before pause activation, then native shell/world migration.

Implementation46e844e996702cf8b3da528abd8e43fc48f48d6b: CI#771 SUCCESS (39545733207); blur gate remains failed/open, pilot false. Next starts by diagnosing native effect host/crop preparation, then pause shell/world migration. No gameplay/APK changes.

### 2026-10-21 — shared backdrop mirror coordinates corrected

Entry814befe44f2d043a46ba877aac19e5708a4f81e3. The previous functional blur FAIL
is superseded: atlas UV normalization did not map to the CSS ROI. Use local PANEL
quad coordinates (already logical at DPR2), then reflect per-sample screen deltas
through the original matrices and convert to texels. No effect/kernel/gamma reduction.
Production shader corrected; pause chrome still off pending full-screen/device gates.

Actual-panel DPR1/1.5/2/3 plus rotated controls:12cases/42checks PASS; white edge
against black:4cases/16checks PASS. Original controls47 and Return/map reload7 PASS.
Fixture cleanup restores callback/runtime/scale/Profiler/background and0 filters.
Early native prefix32 changing-frame samples PASS(maxchannel1); known late-panel
negative control still fails first changed frame. This is not full compositor proof.

Paused A/B/A/B with corrected blur:7.70->9.65FPS, GC24.557->14.671MB/frame, still
unacceptable. CPU Timeline confirms source CSS52–67ms and backdrop preparation~14ms;
BuildTree includes CSS. API232+3, TS, Node272 and all8 provenance checks PASS.
Six preferences restored; Console0errors, Profiler pressure warnings retained.
P1/P2 PARTIAL. Read UNITY_NATIVE_PAUSE_BLUR_FIX_2026-10-21.md and BLUR_* evidence.
Next: native pause shell/compositor and complete quality/input gates, then active
world-view profiling/migration. Do not repeat the superseded empty-doc blur harness.

### 2026-10-21 — unified pause layout, event invalidation, Continue ownership fix

Entry4815395. One UI Toolkit screen now composes all four existing native pause
bodies, header/footer and ScrollView; original detail/actions retained. Source shell
is still a paint adapter, not a complete native compositor. Candidate OFF pending
mobile/pixel acceptance. Production paused data invalidates on events rather than
an unconditional100ms cadence; animations/layout/filters still tick every frame.

Found old snapshots overwriting live native Home ownership and leaving Home visible
over gameplay after Continue.33 UI fields now excluded on capture AND legacy restore;
original gameplay/RNG unchanged. Actual Continue/hide/exclusion verified. API234+3,
Node272, supplemental .NET10 core1193/source47339 PASS; CI must match containing HEAD.
Post-fix paired Editor pause: periodic source2.68FPS/48.696MB GC, retained source
7.22/8.036MB, unified native17.86/3.183MB; native HUD held constant, no device or full
pixel parity claim. CSS/reconcile0 in idle retained captures. P1/P2 PARTIAL.
See UNITY_NATIVE_PAUSE_SCREEN_2026-10-21.md and associated evidence; next compositor/
mobile acceptance then gameplay views. No source branch/APK/user-file changes.

Final pause lifecycle50 and cold first-ready Return/map reload8 checks PASS; original
chrome oracle160cases/3040checks PASS. Bind original actions during geometry warmup
and preserve them during construction. Editor Console current0errors/0warnings,
Play/Profiler/background off,6 original preference keys restored and verified.
