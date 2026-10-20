# Native pause synergies diagnostic candidate — 2026-10-20

Entry HEAD: 60fc8dedaab726b215d87c1164c14555e2a54479, unity-migration.
Implementation HEAD is the commit containing this report.

## Scope and evidence

Adds typed display projection for all five original PausePlanner synergy sections:
sphere/branch/ability links, build diagnostics, artifact sets, protocols and artifact
synergies. Existing Visual_sphereProgression / Visual_engineStats /
Visual_artifactSystem helpers remain authoritative. No Core, gameplay conditions,
RNG, formulas, save schema or authored definitions are changed.

Original component oracle: 64 RU/EN copied states, 41,440 checks, 82 cards per state.
Covers locked/eligible/in-progress/active sphere status, unknown/discovered/active
protocols, empty/partial/full artifact ownership, names/descriptions/hints, signal
order, tag ownership, counts, card tones and completed-set pulse flag. Synthetic
states are display copies only, never applied to the actual simulation or saves.

Native candidate uses UI Toolkit Yoga, TextCore Labels, native rounded fills/borders,
retained card/tag pools, and shared measured native line boxes. Source pulse is
2 seconds, cubic-bezier(.4,0,.6,1), opacity .5 at50%; Unity weighted AnimationCurve
matches an independent parametric CSS oracle at201 samples within .0002 opacity.
Seven standalone Play layouts pass2,688 checks: RU/EN, portrait/wide, DPR1/2,
partial/full inventory and return to the first state. Card count82, content width
324/734px, finite layout, all labels/statuses/tag boxes, retained root and pulse
range verified. These are bounded layout checks, not full pixel/transition parity.
Screenshots inspected from the actual rendered native panel.

Integration targets the existing pause ScrollView and preserves its header, tabs,
footer and UIDocument. The diagnostic flag defaults false. Unlike older islands,
this pilot removes source body children as soon as requested, before native warmup,
so it can prepare without rasterizing all82 compatibility cards first. Native
content takes normal flow only after stable measured geometry. Source other tabs
and disable continue to use the original route.

## Limits

An initial same-pane test attempted a cold compatibility synergies baseline and
encountered main-thread stalls exceeding30 seconds. That run was interrupted with
normal Editor Stop; the six original preference keys were restored and verified.
This does not prove a specific CPU/GPU cause, and its timing is not an FPS result.
The interrupted run is not a successful lifecycle/visual acceptance. Standalone
native layout success does not substitute for actual same-pane acceptance.

Production activation remains blocked by full visual/transition/input review and
actual game/device checks. Existing artifact glyph quality gate is still open.
No APK, protected branch changes, effects removed or quality settings reduced.
No new FPS/GC or Android performance claim in this group.

Reproduce: scripts/check-unity-native-pause-synergies.cs (Edit Mode),
check-unity-native-pause-synergies-layout.cs (Play, copied display states),
check-unity-native-pause-synergies-view.cs (paused saved run, diagnostic island).
Back up diagnostic preferences before Play and restore/verify after Stop.

## Same-pane acceptance and build validation

A second actual saved-run Play test passed20 checks: enter from original statistics,
five sections/82 cards, same panel and UIDocument count, original vertical scroll,
710x7360px content, actual source tab click to statistics, disposal of native content,
return/recreation, then disable and original statistics restoration. Source stats
nodes352 -> native synergies144 -> source stats352; this is NOT a synergies source
A/B performance comparison. The cold compatibility synergies route was avoided
following the stall above. Evidence: UNITY_NATIVE_PAUSE_SYNERGIES_VIEW_EVIDENCE.

Actual Editor compilation: SUCCESS, no compile warnings. Installed Unity API
checks: Android player228 files, Android Editor runtime228, Editor assembly3;
zero errors/compiler warnings. Node272 tests, Unity preflight and visual reference
provenance checks: PASS. The new pilot remains off by default. Full CSS transition,
pixel, gesture and Android acceptance remain incomplete.

## Confirmed discarded-tree cost and early ownership

The first measured native HUD+synergies paused configuration still constructed
PausePlanner's entire source body before pruning it. At1260x542, Timeline recording
off,30 warm/120 samples:5.719FPS,174.849ms/frame,29,089,341allocated bytes/frame;
BuildTree54.235ms, ResolveCss48.680ms, ProjectTree5.426ms, Canvas.EndFrame1.013ms.
This exposes real remaining source-UI work; it does not attribute unmeasured
GPU, active simulation or Android costs. Evidence PROFILE_BEFORE_EARLY_SKIP.

Adds an optional presentation ownership flag to PausePlanner and a false constant
in the original GameScreen. The Unity-hoisted view gets its flag from the existing
bridge only for native-owned content. Short-circuit guards now skip the old body
before construction, and the redundant post-build pause tree walk is removed.
The original renderer always passes false. Gameplay/conditions/text remain intact.
C# was regenerated mechanically with integrity-verified locked TypeScript5.6.3;
only Visual_App and the corresponding manifest hashes change. No browser launched
or reference pixels/fixtures replaced. The reference manifest updates only App.tsx
input hash for this opt-in bridge change, preserving all captured output hashes.
An additional8case/24check original-shell test proves the returned native tree is
identical to the source tree with just body children removed (RU/EN all4tabs).
The64case/41,440check original-content oracle passes again with ownership off.
Pinned TS5.6.3 typecheck, Node272 and preflight/provenance pass.

After early ownership, same saved run/resolution/paused diagnostic configuration,
Timeline off,30 warm/120samples:5.850FPS,170.936ms/frame,27,022,267bytes/frame;
ProjectTree0.934ms, ResolveCss49.702ms, BuildTree50.781ms, Canvas.EndFrame0.977ms.
ProjectTree fell82.8%, GC7.1%; total frame time fell2.2%. These two sequential
samples are bounded observations, not repeated A/B or Android acceptance.
**BuildTree INCLUDES ProjectTree and ResolveCss** (see SourceUiDocument.Draw),
so those timings must not be added. ResolveCss remains the main measured nested
BuildTree cost. This is not a performance fix completion: paused5.85FPS and27MB
per frame remain unacceptable. Native pause shell/mobile and the compatibility
UI path remain architectural priorities; world performance needs separate traces.
Same-pane20check lifecycle passed again after the early-skip change. Evidence
PROFILE_AFTER_EARLY_SKIP and PAUSE_EARLY_SKIP_EVIDENCE accompany the first capture.

Final regression after early skip: actual stats44, abilities42, artifacts21 (empty
saved inventory; directly invoked bound set callback opens/blocks/closes the
original modal) checks PASS. The existing tests now wait for the first projected
empty source body after native Ready rather than assuming both happen in the same
Editor update; all waits retain the180sec deadline and full assertions. Six original
preference keys restored/verified after each final Play session. Final Console
sample13:53UTC:0errors,1warning retained (Profiler memory pressure). Pipeline
history retains request timeouts from the cold source stall. No texture-index3
error observed; that is not proof the original issue is resolved. Memory pressure
was reported at13:46 before the later capture, so the small FPS difference must
not be treated as a robust benchmark or general speedup. No logs/captures cleared.

Visual inspection of the actual native-body pause capture also shows that the
remaining source tab icons/text overlap and the shared ScrollView retains its
Editor theme scrollbar. These are existing shell/scroll presentation limitations,
not accepted full-pause parity. Native shell/control work must address the original
authored appearance; current content-only pilots do not claim it is complete.
