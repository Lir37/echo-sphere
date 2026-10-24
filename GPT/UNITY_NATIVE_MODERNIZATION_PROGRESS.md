# ECHO SPHERE — native visual modernization

## Current implementation boundary — 2026-10-24

The active visual/UI migration is implemented end to end: retained UI Toolkit
screens, typed native run models/controllers, Unity meshes/materials/CommandBuffer
world composition and GPU effects. No production consumer uses translated
renderer/UI functions, dynamic V scene data, JS callables, DOM/CSS cascade,
reconciliation or SourceCanvasSurface. Android compilation now explicitly rejects
those types and snapshot members. Editor references remain for regression checks.

| Subsystem | Native implementation | Decision / implementation status |
| --- | --- | --- |
| Home and all menu destinations | Retained UI Toolkit, VectorImage, native font assets and materials | MIGRATE / implemented |
| HUD, pause, upgrade/reward, tutorial and results | Retained UI Toolkit and typed runtime models/actions | MIGRATE / implemented |
| Mobile controls, gestures and Ghost Snap | UI Toolkit pointer events, native components and Core graph data | MIGRATE / implemented |
| Player, spheres, enemies/bosses and pickups | Typed component views, Unity mesh/material composition | MIGRATE / implemented |
| Links, particles, combat feedback and region world | Native meshes/shaders/component state, authored order and curves | MIGRATE / implemented |
| Shadows, blur, gradients and depth | Unity materials, render targets and ordered passes | KEEP / native fidelity mechanisms |
| Procedural geometry and exact scalar formatting | Authored C# emitting Unity geometry, original numeric order | KEEP / no duplicated UI engine |
| Legacy translated render/UI and V snapshots | Editor-only reference code, absent from Android player | MIGRATE / runtime retirement implemented |
| Final visual/control/device and performance acceptance | Actual Editor/device verification | OPEN / no broad parity or FPS claim |

Implementation completion does not mark P1/P2 acceptance DONE. No APK has been
built. Keep new-desing protected; preserve local project/quality/recovery changes.
Later historical entries describe earlier intermediate states.


2026-10-24 pointer dispatch and Follow actor lifecycle: exercised actual Unity
UI Toolkit PointerDown/Move/Up/Cancel events on a temporary laid-out UIDocument,
through the production native gesture handlers and BindNativeMobileInput. 22
checks PASS: native/disabled button isolation; handed joystick capture; 11.99px,
12px and >12px original deadzone/key/feedback boundaries; Up clears capture/keys;
pause/game-over blocking; real Ghost Snap preview and placement commit; tutorial
existing-Sphere protection; retained Sphere drag/reposition; movement lock and
original same-position tap removal. PointerCancel intentionally retains source
endPointer commit semantics; no new cancellation behavior was invented. A real
seed Sphere is required for original Ghost Snap previews; empty-network null
preview behavior was preserved. Tests used a temporary panel, not physical touch
or continuous combat on the production document; broader input acceptance OPEN.
Actual SphereAttackAgent lifecycle comparison then reproduced three failed
assertions: inactive Spheres moved during Follow, and missing offsets were applied
as pre-existing offsets when a Sphere reactivated. Source formationFollow.ts
skips !alive nodes and captures a missing offset on first live sync without moving.
SyncFollowPosition now skips inactive GameObjects; a private validity flag maps
source's optional formationFollowOffset. Active enable captures normally; inactive
enable defers capture; first active sync captures only, subsequent sync follows.
All 7 actor lifecycle checks PASS, including ordinary offsets, inactive freeze,
reactivation, free mode and newly captured offsets. Alive but network-disabled
Spheres continue an already active Follow, matching source (activation eligibility
is separately filtered); this distinction is explicitly checked. No balance,
RNG, source distance/strain, input policy, graphic/effect or APK change.
Actual Editor compile and Android player/editor typechecks: 0 errors/warnings;
Console 0 errors/0 warnings. Repeated 78 mobile-command checks and 272 Node tests,
preflight/provenance PASS. Simulation stayed paused during each transaction;
player/runtime fields, collections and RNG restored; created fixtures cleaned up,
Play stopped; no preference save invoked. Added reusable pointer-panel/dispatch
and actor-lifecycle checks. Containing-commit CI must pass. Remaining: continuous
live combat and production-document gestures/effects across devices/resolutions,
Android performance and full P1/P2 acceptance; no measured FPS/GC change.

2026-10-24 native combat-control command acceptance: actual native command
transaction passed 78 checks: all catalog Sphere selectors, Pause/unpause and
modal/tutorial guards, movement Lock, nine Dash directions/default direction,
source Dash timing/cooldown/repeat/blocked gates, Shield hotkey/cooldown/blocked
gates, actual Sphere creation, source-pixel placement coordinates, reposition,
removal and Follow enable/disable/denial. Newly created Sphere/particle/audio
fixture objects were disabled/destroyed; runtime/player fields, collections and
exact RNG restored in finally. No simulation frame advanced or preference save
was invoked. This does not replace live pointer/combat/device acceptance.
The run exposed two confirmed Follow parity defects: FormationFollowRules still
used 1.6 old world units despite shared source calibration 12.6/720. Source
formationFollow.ts specifies 160 logical pixels. ActivateRadius now uses exactly
160 * SpherePlacementRules.SourcePixelsToWorld (2.8 world units at current scale).
Also both NativeCanActivateFollow and the actual SetFormationFollowMode guard now
exclude inactive/network-disabled Sphere nodes, matching source centroid filtering.
No source activation distance, strain, movement, network/resonance multipliers,
pool weights, RNG algorithm, artwork or effects were redesigned. Updated the old
C# checks that incorrectly hardcoded 1.6; checks now cover source 100px, inclusive
160px and outside 160.01px. Actual Editor checks cover 159.99/160.01px and reject
inactive-only/disabled-only formations in both UI eligibility and commit paths.
Unity 6000.3.24f1 compile and Android player/editor semantic typechecks: 0 errors/
warnings; actual Console 0 errors/0 warnings. Core contracts PASS 1383 assertions,
47339 direct TS/C# comparisons; 272 Node tests and preflight/provenance PASS.
Play stopped; user settings/recovery edits preserved. Containing-commit CI must
pass. Still OPEN: full live combat/gesture/visual flows and device performance;
no new FPS/GC measurements and no APK. Previous command-application commit
5fa52d93f44279462ab53bff65b6595fb84d4ffb passed CI #838 SUCCESS.

2026-10-24 command application acceptance: found a source-proven handler gap.
Native cards can represent Resonance, CritPower, ProjectileSpeed (proj_speed)
and Fortune, but ChooseUpgrade consumed those choices without assigning their
level. src/engineProgression.ts applyUpgrade stores the level for any ability.
Actual PickNativeUpgrade reproduced 28/28 missing increments (levels 0->1 through
6->7 for all four kinds). Added only the four missing passive switch labels;
the same 28 actual Editor command cases now PASS, with dictionaries/flags restored.
Routine Level-Up deliberately excludes passivePool in both original and Unity.
This change does not add passive offers, alter RNG/pool weights or implement
bonus formulas merely promised by metadata. In particular no projectile-speed,
critical-power or Fortune effect was invented; existing source behavior remains.
Actual native command transaction also passed 33 checks: active Blast acquisition
and hotkey; Sphere IV -> existing branch decision and mutation count; Lock token
and locked identity; Reroll token and preserved lock; denied reroll consumes no
RNG; common crystal_speed reward acquisition/consumption; duplicate and unknown
reward rejection. No simulation frame advanced. Scalars, mutable collections and
exact run RNG state restored in finally. Common-reward fixture was checked not
to complete any protocol, so no discovery/preference save was invoked. These are
actual production native command calls, not full pointer/combat/device acceptance.
Unity 6000.3.24f1 compile and Android player/editor semantic typechecks:
0 errors/warnings; Unity LogEntries after commands 0 errors/0 warnings; 272 Node
tests and preflight/provenance PASS. Added reusable upgrade/application command
checks. Play stopped, user project/quality/recovery edits preserved. Containing-
commit CI must pass. Remaining acceptance: complete live combat/control/effect
flows and character/evolution combinations, plus Android/device performance.
No measured FPS/GC changes, no APK; P1/P2 acceptance remains OPEN.

2026-10-24 populated progression follow-up: the empty-modal pass did not cover
one remaining texture-registry failure. Three populated upgrade cards plus the
upgrade tutorial step reproduced 33 actual Console errors when that step closed.
Registry inspection identified Unity-owned dynamic filter TempBuffers, not source
Sprite/RenderTexture artwork. Two lifecycle defects were corrected: backdrop
prefix traversal now immediately skips explicitly zero-opacity subtrees instead
of copying their previous resolved appearance; the tutorial Next button retires
its nested backdrop before leaving layout on the choice step and registers a
fresh backdrop when shown again. Filter parameters, graphics, authored layout,
animations, gameplay formulas and RNG remain unchanged.
Actual Editor 6000.3.24f1: nine populated modal stages / 270 frames / 420 checks
PASS; all six tutorial steps, three real upgrade kinds, three real set-member
artifacts, card hover and hidden-overlay picks exercised. Each stage and the
final Unity LogEntries had 0 errors / 0 warnings. Real Game View screenshots of
populated upgrades/rewards and set detail reviewed. Actual nested badge pointer
check: set callback=1, parent card callback=0. Actual runtime reward badge -> set
detail -> native Close: 11 checks PASS, reward unconsumed, owned artifacts and
choice lists unchanged. Gameplay picker was intercepted during this input check;
this does not claim applying a reward/upgrade or combat formula acceptance.
Flags/choices/callback restored, Play stopped. Unity recompile and Android
player/editor semantic typechecks: 0 errors/warnings; 272 Node tests and project/
provenance guards PASS. Added populated-modal and real reward-set-detail Editor
checks; containing-commit CI must be green. Remaining: actual upgrade/reward/
lock/reroll outcomes, complete live combat/effect/control and device/performance
acceptance. No current FPS/GC before/after samples, no APK; P1/P2 acceptance OPEN.

2026-10-24 final modal lifecycle acceptance: reproduced the reported UI Toolkit
"Attempted to get a texture ... that is not allocated" during the retained
upgrade/reward/tutorial/result transition sequence (128 errors on a repeat).
Absolute NativeRunDocument overlays now retain their filtered render trees when
hidden: Visibility.Hidden plus subtree opacity zero, instead of display:none.
Hidden overlays are excluded from backdrop preparation. Opacity also suppresses
replica descendants with explicit visibility. No shader, blur radius, artwork,
animation, simulation formula, RNG or gameplay command changed.
Actual Editor 6000.3.24f1: nine modal stages / 150 frames / 414 activation,
layout and grid hit-test checks PASS, flags restored and Play stopped. Six tutorial
steps, upgrade/reward hosts and result activated; upgrade/reward choices were
empty fixtures, so this does not accept choosing actual cards/artifacts. Hidden
overlays intercepted none of the sampled panel picks. Real Game View result
screenshot reviewed. Actual Unity LogEntries: 0 errors / 0 warnings after fixed
sequence. Existing original baseline reproduced 128 texture errors; no current
FPS/GC before/after measurement or Android/device/full-effect acceptance claim.
Unity compile and Android player/editor semantic typechecks: 0 errors/warnings;
272 Node tests and preflight/provenance/code export guards PASS. Added reusable
scripts/check-unity-native-modal-lifecycle.cs. CI must pass on containing commit.
Remaining acceptance: populated card/reward actions, complete live input and
combat/effect/device parity and performance. P1/P2 acceptance remains OPEN.

2026-10-23: Native world resources and localization no longer initialize or call
the translated renderer/i18n/canvasLocale/orbitalGeometry modules. Added actual
Unity ScriptableObject catalogs: 17 existing Sprite references with source natural
sizes and original first-request fallback, 127 RU/EN translations, 25 ordered world
phrases and 75 ordered regex replacements. Native sprite submission consumes typed
INativeWorldSprite dimensions rather than browser image properties. Canonical
world palette/render log state is authored C#; runtime ImageFactory removed.
Orbital display counts/angles are direct double C#; character metadata uses existing
CharacterRules and EngineerRelayRules. No combat/RNG/quality/artwork change.
Editor comparisons matched 254 translations, 624 canvas-localization cases, 17
sprite identities/dimensions and 365 orbital angles/counts. Real native pause UI
rendered after the change: legacy documents=0, stats Ready=true, flags restored;
Console current errors/warnings=0, Play stopped. Editor compile zero errors/warnings;
272 Node tests and metadata/preflight/provenance pass. Added retirement guards and
reusable Editor catalog import/check scripts. CI checked on containing commit.
Remaining: gameplay presentation catalog/read helpers, mobile/network V adapters,
world snapshot/dispatch V and comprehensive control/modal/device acceptance.
This is an implementation checkpoint within the continuing full-transfer cycle,
not a declaration that P1/P2 or full modernization is DONE. No APK.

2026-10-23 follow-up: All active runtime Visual_App.Scope calls removed.
Formation names, network tooltip text/scaling and displayed XP planner multiplier
are direct C# read helpers, preserving the original display quirks and formula.
Actual Editor comparison against the original helpers matched 112 tooltip cases
(7 forms, RU/EN, empty/populated sphere types, four active/secondary/inactive
combinations), 16 name cases and 12 XP cases. This is read-only model parity, not
an input/gameplay/device acceptance claim. Editor compile zero errors/warnings;
272/272 local Node regression tests pass. CI#806 correctly rejected the previous
commit: CS0649 on unused legacy BeforePaint field and two tests requiring retired
reset/cadence wiring. The optional legacy hook is now a property; tests verify
direct native reset and the native 10Hz/dirty model cadence with per-frame effects.
A containing-commit CI rerun is required. Data/catalogue/helper V adapters outside
App remain. Overall modernization/acceptance still PARTIAL; no APK.

2026-10-23: Production run UI now belongs to NativeRunDocument (retained Unity
UIDocument). World Image/filter, vignette, HUD hosts, combo, pause shell/highlight,
result/upgrade/reward/tutorial hosts and ordered blur prefixes are authored native
hierarchy/brushes; existing native presenters bind directly. Active game no longer
constructs SourceUiDocument, VisualDom trees, CSS nodes, hook/reconciliation trees
or Canvas command surfaces. EchoSphereLegacyRunUiProjection and the CSS anchor
capture were deleted. NativeUiDataContext is a plain storage/date compatibility
object with no GameObject, renderer, timers or UI environment. Main menu no longer
draws hidden generated pages. Interface scale reads PlayerPrefs directly with the
original Number conversion/clamp; reset command extracted unchanged and called
directly. Read-model V/catalogue/calculation helpers remain; they do not own UI.
Actual Editor compile zero errors/warnings. Paused real-runtime UI checks rendered
all four tabs: Ready true for each across 48 bounded stepped frames. Screenshot
inspected; fixed native shell brush covering its content, now ordered behind it.
Save accumulator and menu/pause/tab flags restored; no save replacement or inputs.
After deleting legacy runtime bindings, repeated native run: SourceUiDocument
instances=0, NativeRunDocument=1, NativeUiDataContext, native stats Ready=true;
menu flags restored. Current Console errors=0/warnings=0; retained Pipeline stream
still contains earlier diagnostic records. Play stopped. Structural preflight,
metadata/provenance and native route guard pass. CI on containing commit checked
separately. Production visual ownership is native; overall acceptance remains
PARTIAL: V read helpers and legacy reference code remain, full visual/input/effect
cases, Android and performance have not all been accepted. No APK, protected
baseline/gameplay/quality settings changes included.

2026-10-23: Mobile host geometry now belongs to retained NativeMobileLayout,
not generated DOM/CSS placeholders. Dock, joystick, placement, ghost network,
formation memory and movement tutorial target bind directly to typed anchors.
Original 12/16px dock insets, hand-dependent origins, interface scaling, 116/52px
feedback boxes, movement target quarter positions/bottom44, overflow and z-order
are preserved. The z-20 native root is inserted above the authored z-4 vignette,
below HUD and overlays. Mobile world/preview coordinates read the same canvas
Content.worldBound and texture dimensions directly, removing their V canvas-ref
bridge without changing the existing formulas. Mount/handedness/state reads and
gameplay commands are unchanged. Native layout/field handles excluded from saves.
Actual Editor compile zero errors/warnings. Real-runtime temporary pause-frame
construction: native mobile root six hosts, sibling index2, HUD index5, same parent;
flags restored and Play stopped. Isolated actual UI Toolkit geometry checks at
1260x542 and interfaceScale1.25 verified both hands, feedback dimensions/origins,
movement target positions and a sized dock child. Not positive input, all pixels,
Android acceptance or measured FPS/GC. Diagnostic-only probe emitted a missing
ThemeStyleSheet warning; an unsupported console_get command logged a Pipeline
error. Both are identified as diagnostics, not suppressed or called a clean Console.
Preflight/provenance/metadata pass; containing-commit CI checked separately.
Legacy SourceUiDocument/game root, modal shells/prefix resolver and V data remain;
full migration PARTIAL. No APK, gameplay/source-baseline/user-file changes.

2026-10-23: All native run-panel preparation now consumes NativeRunUiAnchors rather
than traversing VisualCssNode trees. Seventeen typed UI Toolkit slots, a live model
version provider, invalidation, backdrop provider and tutorial target resolver form
the host contract. One CaptureLegacyRunUiAnchors adapter supplies current hosts with
the original first/last DFS rules and Content containers. Legacy tree pruning and
mobile placeholder construction moved unchanged into EchoSphereLegacyRunUiProjection;
native presenter preparation no longer owns DOM/CSS selection rules. Existing layout,
parents, order, warmup gates, hidden-vs-collapsed legacy content and version reads
are retained. Actual Editor compile zero errors/warnings. Real-runtime Play checks
rendered normal plus pause tabs 0..3, with HUD/mobile anchors present and all pause
hosts present in pause; seven native backdrop registrations, Console zero errors/
warnings, temporary flags restored, Play stopped. Not full visual/input/device
acceptance. CI checked on containing commit. Legacy root/CSS/prefix resolver, V read
models and remaining gesture canvas bridge still active. Migration remains PARTIAL.
No APK or gameplay/quality change.

2026-10-23: Seven native UI view/composition classes no longer depend on concrete
SourceUiDocument for blur registration. INativeUiBackdrops and NativeUiBackdropRegistry
own registration/prepare/disposal independently; the legacy document supplies the
existing paint-prefix resolver and shared snapshot cache. Traversal order, prefix
root selection, sigma, replica-skip and blur renderer are unchanged. Actual Editor
compile zero errors/warnings. Play Mode rendered an actual-runtime pause frame with
7 registered native backdrops; temporary menu/pause flags restored, Play stopped,
current Console zero errors/warnings. This does not prove all blur pixels/input or
remove the old UI root: DOM anchors, prefix resolution and V model adapters remain.
CI checked on containing commit; no APK or gameplay/quality changes.

2026-10-23: Native mobile cooldown/max-cooldown/dash-label/strain display logic
now runs directly in C# with the original constants, fallback and rounding. Haptics
call Unity Handheld.Vibrate under the same Android/iOS gate as the former navigator
adapter (duration was already ignored). No active mobile read/haptic calls remain
into the translated MobileControls module. Ability-name/catalogue, V data and root
layout adapters still remain. Actual Editor compile zero errors/warnings; CI checked
on containing commit. No gameplay/cooldown formula/quality change or APK.

2026-10-23: Native run controls now call typed C# commands directly. Pause chrome
resume/exit/tab actions no longer retain generated V callbacks or a binder. Mobile
follow/dash/type/hotkey/place/reposition actions no longer route through SourceUiBinding
or allocate a generated action closure per call. HUD slot swapping applies the
existing native network preferences directly, using the same projected formation
selection as before. The remaining source UI action adapter calls the same typed
commands. Original gameplay methods, guards, sound/save/refresh order, coordinates,
follow checks and keyboard direction quantization are preserved. Model reads and
legacy flash/keyboard/query bridges remain; this does not remove the UI root.
Actual Editor compile zero errors/warnings; initialized Play runtime query/empty-slot
guards exercised, current Console zero errors/warnings, Play stopped. This is not
positive-control/all-input/device acceptance. CI checked on containing commit.
Full modernization remains PARTIAL: V model/read helpers, UI DOM/hooks/layout anchors
and shared UI backdrop/prefix ownership remain. No APK or gameplay/quality changes.

2026-10-23 CI follow-up: run #799 found no C# errors but rejected three CS0649
warnings for the native compositor zero shadow/dash offsets. These fixed defaults
are now explicitly initialized readonly fields; values/paint behavior are unchanged.
A fresh containing-commit CI run is required and is checked separately.

2026-10-23: The active world now renders through NativeWorldSurface, a dedicated
Unity CommandBuffer mesh compositor. SourceCanvasSurface is no longer instantiated
by the game frame. Native world owners no longer construct/retain Canvas current
paths, generic paint tables or Canvas calls; typed transforms/opacity/dashes replace
frame command dispatch. Existing native meshes/materials, shared glyph stabilization,
MSAA4, ordered alpha/additive/screen, shadow masks/blur/padding and presentation
color conversion are retained. Backing size uses the original finite DPR cap and
rounding. Reflection field discovery/value reads were removed from presentation;
remaining profile/leaderboard/shop records have explicit typed mappings.
Actual Editor compile zero errors/warnings. Actual-state Play Mode frame rendered
through NativeWorldSurface; 1260x542 texture captured and inspected, no blank/error
output. Current Console zero errors/warnings; menu restored, Play Mode stopped.
This does NOT establish all-effect/device/visual/performance acceptance. CI checked
on containing commit. Remaining: V snapshot/helper-data adapters and source UI
DOM/model/hooks/slot-layout bridge. Full migration remains PARTIAL, no APK.

2026-10-23: Active world frame orchestration is now authored C# rather than
Visual_renderer.render. Sphere layer ordering for all ten catalogue types is native
as well. Per-frame capture/replacement/restoration of translated drawing functions
has been removed; native views are dispatched directly. All formerly inline world
feedback is native: fire trails, particles, both damage-number streams, boss arrow,
boundary waves/shards, wrapped localized flash banner, world border and radial frame
background shader. Original render-load strides, Orbital damage priority, shake RNG
calls, display formulas, inherited shadows/baselines and paint order are retained.
Actual Editor compile zero errors/warnings; new background shader supported, empty
diagnostics, three composition passes. A single actual-state frame was rendered in
Play Mode via LateUpdate with the menu guard temporarily bypassed and restored in
finally; no simulation run or saved-run replacement. Current Console 0 errors/
warnings. This is a smoke check, NOT all-effect/visual/device/performance acceptance.
CI checked separately on containing commit. Remaining migration: V/reflection data
snapshot, source UI document/model hooks, Canvas state/ordered compositor and
noncatalogue fallback. P1/P2 remain PARTIAL. No APK or gameplay/quality changes.

2026-10-23: Sphere network rendering is now a typed native Unity mesh owner:
25-point wavy links and three moving packets, external Echo Drone links, all seven
formation views, triangle charge/pulse ticks and fading formation memory. Existing
network/frame/filter helpers still own membership and gameplay. Paint order,
per-layer shadows/blur, dashes, alpha, source double display math and final path
continuation are retained; no unused formation tags added. Actual Editor compile:
zero errors/warnings. CI is checked on containing commit. Full migration remains
PARTIAL: inline combat feedback and source renderer/UI/data/compositor bridges.
Play/device/FPS/GC deferred; no APK or quality/gameplay change.

2026-10-23: Player shield, Engineer links, Architect formations, Hunter target marks,
Alchemist reaction labels and Berserker range are now native world views. Existing
formation/range/target data remain authoritative. Original dashes, colors, shadow and
stroke-before-fill order are retained. Native world text now supports source font
family/size/style and additive composition (Georgia 10px labels, 9px shield count);
existing 8px labels keep their defaults. Editor compilation and label shader pass
checks succeed. Gameplay/quality unchanged; Play/device/FPS/GC still deferred.
Network formations/links, inline combat feedback and source renderer/UI/data/
compositor bridges remain incomplete. No APK.

2026-10-23: The complete renderSphereMutationVfx entry point now has a typed native
C# owner: all original branch silhouettes, three final expansion modes, persistent
emitters and attack-peak accents. It retains source paint order, caps/joins, inherited
opacity, branch/final metadata and the extra final-form rotation where authored.
The display attack-peak calculation and .03 gate are unchanged. Legacy mutation
primitive adapters now parse CSS rgba as well as HEX (the Pulse apex arc uses rgba).
Actual Editor compile: zero errors/warnings. Play/device/FPS/GC acceptance remains
deferred. Other combat/network/character-world effects and source renderer/UI/data/
compositor bridges remain; full modernization is still PARTIAL. No APK/gameplay change.

2026-10-23: Sphere projectile tails/energy cores and all original family/branch
projectile variants now have a native owner. Specialized Chain Lightning (web/storm/
leech and original branch fallback) is native and retains the original false return
for the existing fallback bolt. Mutation fin/shard/arc/tether/eye/glow/pulse-ring
primitives also submit native meshes/materials. Original mutation silhouette/final
expansion/emitter/transient branch coordination and its inline paints remain partial.
Editor compile passes; mutation glow shader is supported, diagnostics empty, all
three composition passes present. No Play/device/FPS/GC claim. Source coordinator/
UI/data/compositor and other combat/network effects remain. No APK or gameplay change.

2026-10-23: Elemental Fire/Freeze/Poison rings, glyph variants, weapon accents and
special family signatures are native, including Prism Spectrum resolution through
existing branch metadata. Orbital body and both attacker rings are native too, with
weak animation memory, original attack/resonance decay, terminals, satellites,
blade/element trails and nearest-enemy display flash. Counts/angles come from the
existing orbitalGeometry rules. Original composite/alpha/path quirks are preserved;
no gameplay correction or new rule was introduced. Editor compilation passes with
zero errors/warnings. Mutation silhouettes/emitters/projectiles/lightning and source
renderer/UI/data/compositor bridges remain. No Play/FPS/GC/device parity claim.

2026-10-23: Eight specialized family views (Sniper, Shotgun, Chain, Aura, Prism,
Gravity, Pulse, Void) now use native C# owners and Unity Vector Graphics meshes.
Shared family cores, all ten authored orbit modes, glow and disabled arc are native
(including use by remaining source Orbital code). The source 43-point traces,
lightning jitter, Prism dash and inherited alpha/blend/gradient creation-space rules
are retained. All six modifier glyphs and level marks are native as well; existing
authored modifier levels/projectile applicability remain authoritative. Actual Editor
compile and shader diagnostics pass. Orbital-specific body/attackers, elemental and
mutation/projectile/lightning VFX plus source coordinator/UI/data/compositor bridges
remain incomplete. No Play/device/FPS/GC acceptance is claimed.

2026-10-23: The active player representation is now native C#: original Spherist
animation-state priority, front/side selection, movement mirror, double pulse,
death fold/shear/fragments and sprite availability fallback, plus all original
procedural character silhouettes/cores and character-specific accents. Imported
Sprite art is unchanged. Standard Sphere assembly is also native, retaining target
pulse/range display predicate, breathing, the original upper crystal mirrored for
the lower part and accent. Source specialized sphere mutation/element/modifier/
family VFX, overall renderer/UI/data/compositor bridges remain incomplete. Actual
Editor compile and all three new world shader diagnostics pass; no Play/FPS/GC/
device parity claim. No APK, source-branch or user-file changes.

2026-10-23: Standard Sphere core, full/partial orbital rings and attack charge VFX
now have a native Unity owner. The original focal radial gradient, additive passes,
three-pixel ring shadows and charge curves remain. All original rune presentations
and ordinary/Stella chests also use retained native meshes/materials and Unity world
labels. Collection, reward and sphere attack rules remain unchanged. Editor compile
passes; Standard core shader is supported with no diagnostics. No Play/FPS/GC/device
acceptance is claimed. Assembly/state dispatch, other sphere/player VFX and source
bridges remain explicit work.

2026-10-23: The complete active drawModernEnemy representation now has a native
C# owner: all ten creature anatomies and four bosses, movement-facing memory,
pose/hit pulse, tier/elite crests, fire/poison/freeze, Link Breaker, all original boss
attack telegraphs and boss health/name labels. Shared additive discs, ground shadows
and the disabled-network glyph/ring also submit native meshes. Unity Vector Graphics
owns curves; materials own radial creature/core glows. Simulation/RNG/balance remain
unchanged. Dynamic anatomy/effect meshes are retained but still retessellated when
shape changes; this is migration work, not a measured performance result. Actual
Editor compile and glow shader validation pass. Play/device acceptance is deferred.
Player/sphere procedural views, other VFX and source coordinator/UI/data/compositor
bridges remain incomplete. See the latest production-routes addendum.

2026-10-23: All six hostile projectile presentations are now selected through a
native C# owner: ranged/sniper and shooter/charger/summoner/aura boss variants.
Retained Unity Vector Graphics meshes preserve authored bodies, tails, outlines,
orbiting satellites and animated arcs; native materials preserve over/additive
passes and existing shadows. Projectile simulation is unchanged. Actual Editor
compile passes; Play/device/performance acceptance remains deferred. Enemy creatures,
other combat VFX and source renderer/UI/data/compositor bridges remain incomplete.

## Latest implementation direction — 2026-10-21

User explicitly prioritizes completing native Unity migration before further
measurement or standalone acceptance fixtures. Older performance-gated pilot notes
below are historical. Current production now selects native HUD and the unified
four-tab pause content/chrome. Native Home, region selection, shop, leaderboard,
achievements, character selection, archive and Settings route directly to UI Toolkit owners. Settings no longer
constructs a source document/Canvas first. Achievements use a native ScriptableObject
catalog imported from the locked original definitions. Existing prices, progression,
region reload handoff, saves and gameplay remain authoritative.

Actual Editor compilation passes with zero errors/warnings. No new profiling,
pixel comparison, Play acceptance or device validation was performed for this group
at the user's direction. CI is a separate check on the containing commit. Full native
migration remains incomplete: gameplay drawing, and the remaining CSS shell/compositor
and retained-prefix fallback must still be replaced. Native controls being selected
does not mean the whole game is native or visual/device parity has been accepted.
See UNITY_NATIVE_PRODUCTION_ROUTES_2026-10-21.md.

Native result content and upgrade/evolution picker controls are also selected now.
They retain the original game overlay frame and registered backdrop order. Lock,
Reroll, evolution, Oracle and mutation-hint display use existing state/actions;
the native result uses Unity AnimationCurve for the original record pulse. This is
implementation, not combined Play/device acceptance. See the latest report addendum.

Artifact rewards, artifact set detail (reward and pause), six-step tutorial and the
mobile combat button dock now have retained native Unity views. The dock now calls a native C# gesture/action owner preserving direction, haptics
and guards. Native joystick, placement pulse, ghost network and formation-memory geometry are
also implemented. Gesture recognition, pointer capture, timers and typed mobile view models now have
a native C# owner. The generated MobileControls component and its hook/DOM-event
state are bypassed. Outer source overlay/layout and shared game-data projection
remain explicit migration work. Native alpha shadows and
brush spread support preserve authored glyph and panel shadows.

Native gameplay-world migration has started: the active field gradient, nebulae,
near-field/horizon lines,120 static stars and animated ambient motes now use retained
Unity meshes/materials. A typed ordered-mesh seam retains the current compositor and
camera/shake order. Entity/region/effect/text drawing and the overall renderer are
still source-projected; this is a partial world migration, not a finished scene.

The active regional world layer is now native as well: pocket rings/seams/markers,
POI pulses/diamonds and localized world labels use Unity meshes, materials and
Font/TextGenerator. Native and remaining source text share atlas stabilization to
preserve glyph UVs and source interleaving. Entity/attack/effect migration remains.

2026-10-23: Native XP crystals, health packs, summoned minions and fallback energy
bolts are now selected. Static art uses retained Unity Vector Graphics meshes;
animated bolts update pooled Unity meshes. Ordered native shadow submissions preserve
all four health-pack paints, blur and overlaps. Special mutation-specific lightning,
player/enemy/sphere representations and other combat effects remain unfinished.

2026-10-23: The shared artwork layer for player/sphere sprites and Standard Sphere
parts now uses native imported Sprite UV meshes/materials. Original availability,
aspect/fit, mirrors, alpha assignment, shadows and blend order remain. Animation
selection, procedural fallback and surrounding player/sphere effects are still source-
coordinated; this does not complete those entire representations.

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

### 2026-10-21 — runtime native UIR capture and current-frame composition

Entry1ba7359. NativeUiCapturePanel now provides public-API world UIDocument/camera
capture, without prefix tree cloning. Native stencil alpha mask is a runtime shader
pass; existing Gaussian/quality unchanged. Actual Editor authored Inter bold Cyrillic
text and pattern changing every frame:96 samples at DPR1/2/3 PASS, maxchannel1,
meanmax.000207901; candidate replicas0. Capture ordering resolves observed text and
background frame lag. This is an isolated fixture, not real pause/menu acceptance.

Editor compile0/0, shader supported/messages0, Console ground truth0/0; capture
resources0 after cleanup and6 prefs restored. API235+3, TS, Node272 and provenance/
preflight PASS. No new game FPS/GC measurement. Production candidate remains OFF.
P1/P2 PARTIAL. Next multiple dependent glass layers and transparent alpha proof,
then actual pause/menu compositor wiring, mobile/input/performance and world views.
See UNITY_NATIVE_WORLD_COMPOSITION_2026-10-21.md and adjacent JSON evidence.

## Native run catalog integration (2026-10-23)

Unity ScriptableObject NativeRunCatalog now supplies 10 spheres, 30 abilities,
82 artifacts, 22 artifact synergies, 12 sets, element metadata and mutation links.
Pause abilities/artifacts/stats, artifact rewards and set details, mobile labels,
upgrade hints and world sphere colors/ranges use typed catalog entries instead of
translated metadata helpers. Ability evolution selection and descriptions are
native C# read models, preserving first-marker selection and source fallback order.
Artifact ownership/set membership and reward synergy previews use native collections.
The serializer schema has finite branch/final depth; no recursive Choice list.

Validation: Editor compile 0 errors/warnings; catalog/source 6140 comparisons;
existing pause ability/artifact/stats/synergy projection checks PASS; 272 Node tests
PASS. Bounded paused Play Mode exercised four tabs, SourceUiDocument instances 0;
after stop current Editor Console 0 errors/0 warnings. No FPS/GC/device claim.

Remaining: translated build diagnostics/protocol-state reads, numerical stat reads,
mobile collision/network preview adapters and V snapshot/world dispatch boundaries.
Full native migration and Android acceptance remain open. No APK built.

### Native build read models follow-up (2026-10-23)

All EchoSphereNative runtime files now avoid translated gameData,
sphereProgression and artifactSystem scopes. Build diagnostics and protocol states
use native typed read models with existing Core protocol definitions. World
modifier overlays reuse existing Core SphereAuthoredModifierRules rather than a
second implementation. Numerical sphere/combat stats and network analysis remain
separate translated read boundaries; mobile collision/ghost preview and snapshot
world dispatch migration are still open.

Validation: 64 pause-synergy scenarios / 41440 comparisons PASS; all 2880 authored
modifier entries matched original; compile 0 errors/warnings and 272 Node tests
PASS. Previous catalog commit 2698e73f69eda61ac003426c8d7daebaa9ad27a2 CI #809 SUCCESS.
These checks do not establish Android acceptance or measured performance.

### Evolution card production bridge retired (2026-10-23)

Evolution UI cards now come from NativeRunCatalog and native C# selectors. The
Editor importer captures exact source elemental/mastery text, preserving source
card order, three-choice cap, branch/final IDs and indices. Upgrade choice keys
are native; ability choice names use the typed catalog. Character pause titles
and roles reuse existing Core CharacterRules. Runtime no longer directly calls
translated gameData, sphereProgression, artifactSystem, engineProgression or
characters scopes; provenance guards enforce this retirement.

Validation: 3240 evolution-card/key comparisons PASS, native pause chrome/stats
projection checks PASS, Editor compile 0 errors/warnings, 272 Node tests PASS.
Build-read-model commit e098a64dbc640057d970db70b5f50791a2f9cd5d CI #810 SUCCESS.
Remaining: numerical combat/stat and world entity read adapters; collision/network
preview helpers; V snapshots and named world dispatch. Native migration remains
incomplete; no new performance measurement or Android/APK acceptance.

### Native network rendering projection (2026-10-23)

World network drawing now builds NativeWorldNetwork.Network directly. Sphere-only
links/formations are filtered in native C#; external drone links keep source index
order and positions. The runtime no longer calls translated networkRuntime or
networkRender scopes, and guards enforce that. Existing network analysis and
mobile ghost/collision helpers remain open; this is not full V snapshot retirement.

Validation: 1440 network projection comparisons PASS, Editor compile 0 errors /
0 warnings, 272 Node tests and project/provenance/metadata checks PASS.
Evolution-card commit a4e92fadbfb61fff7dd17bf466166b91b91a2389 CI #811 SUCCESS.
No gameplay, graph quality, RNG or APK changes; FPS/GC not measured in this block.

### Direct world calls and native Ghost Snap projection (2026-10-23)

Removed the string-based NativeFrameView/DrawNativeWorldView dispatcher: 22 direct
C# draw methods retain the exact call order/arguments while avoiding both the
params array and leading-null array copy at each invocation. Entity snapshots
remain V at this boundary and are not claimed retired.

Ghost Snap candidate projection is now native C#: sphere order, moving index,
participation, disabled state and empty-input behavior are preserved. The graph
analyzer is still the existing translated network calculation; collision preview
also remains open. Runtime no longer calls translated networkPreview.

Validation: Editor compile 0 errors/warnings; bounded paused Play Mode after draw
refactor, stopped with current Console 0 errors/0 warnings. Ghost network outputs
matched source in 105 cases, including disabled and omitted alive flags. 272 Node
tests PASS. Network projection commit f841c86b6450811388ab65c71aa3bd5a605988b4 CI
#812 SUCCESS. No measured FPS/GC or Android/full-game acceptance claimed.

### Native cached network and placement gate (2026-10-23)

World network drawing now consumes authoritative Core SphereNetworkState directly,
without serializing the graph into V and reading it back for that draw. The old
V-based projection remains Editor-only for equivalence checks. Orbital cached
network lookup no longer calls VisualBindings.getNetworkFrame.

Mobile place/move validity now calls the existing CanPlaceSphereAt method used by
native placement commits, using the identical source-to-world conversion already
used by NativeRunCommands. Gameplay placement rules were not changed. This removes
the separate translated spaceCollision check from the production runtime.

Validation: 810 typed-Core/previous-projection comparisons PASS; real paused Editor
with refreshed runtime snapshot: 45 finite placement candidates, 39 accepted / 6
rejected, all matching the prior source gate. Initial unrefreshed menu-snapshot
attempt was invalid and is excluded. Compile 0 errors/warnings, 272 Node tests PASS.
Remaining graph analysis for Ghost Snap/read-only diagnostics, numerical stat
adapters and broad V entity/player snapshots are still open. No completion claim.

### Direct Unity entity layers (2026-10-23)

Enemies/bosses now project NativeWorldCreatures.EnemyView directly from
EnemyAgent2D/BossCombatAgent2D; the V-based comparator is Editor-only. XP, health
packs, both chest layers, runes, drones and particles draw directly from active
Unity simulation components in their original layer and entity order. Nine rune
metadata definitions are now in NativeRunCatalog. Particle source-pixel conversion
keeps double intermediates, exact previous RGB quantization and zero-X fallback.
Orbital visual network multiplier now reuses cached Core state directly as well.

Validation: 1480 enemy/boss view field comparisons; 189 particle field comparisons;
1215 native cached-network/orbital comparisons PASS. Compile 0 errors/warnings;
272 Node tests PASS. Prior commit 9313e834df57dfd7e62e817bed6eb34d4d522688 CI #814
SUCCESS. No device/FPS/GC/full gameplay identity claim.

Important open finding: see UNITY_NATIVE_NETWORK_PARITY_GAP_2026-10-23.md.
Existing Core graph classification does not fully match original source; attempted
analyzer replacement was reverted. Native gameplay network/formulas were not
changed. Broad sphere/projectile/player/overlay V snapshots and numerical adapters
remain open; this is not a full migration completion statement.

### Unified native graph analysis after source parity correction (2026-10-23)

Resolved the recorded network gap: source pixel distance thresholds now use the
actual Unity camera conversion, Fractal requires six nodes in ranking, and native
node union order follows original insertion order. These are corrections to the
Unity port, so formation selection can differ from its incorrect prior baseline;
original TypeScript formulas and source data are unchanged.

Ghost Snap and read-only build diagnostics now reuse Core SphereNetworkRules;
Runtime has no direct Visual_network.Scope calls. Native graph snapshots use one
shared serializer. Source numerical stat adapters and broad V snapshots remain.

Validation: 10 original TS network regression fixtures; CoreChecks PASS (1382
assertions), existing direct TS/C# parity PASS (47339 comparisons), local temporary
.NET 10 harness; .NET 8 result is left to CI. 105 Ghost Snap UI-geometry cases PASS
(all UI-consumed ordered fields; unused float strengths/distances not bitwise
identical). Existing 64 pause synergy scenarios / 41440 comparisons and 1215 typed
network/orbital checks PASS. No Android/full-run/FPS/GC acceptance claimed.

### Native derived stats and projectile component views (2026-10-23)

Pause speed/crit/dodge/vampire reads are native C# using typed artifact effects and
10 character modifier records in NativeRunCatalog. Runtime no longer directly
calls translated engineStats or engineCombat. The original evaluation order and
numeric constants are unchanged; these are read-only presentation calculations.

Sphere projectiles now project their native view directly from ProjectileAgent,
its native evolution owner and existing native branch/final catalogs. The V-based
projector remains Editor-only for comparison. Removed the unused mutation-primitive
string dispatcher and unused sprite V adapter after confirming no call sites.

Validation: 1280 exact derived-stat comparisons and existing 18 pause-stat model
cases PASS; 297 projectile-view comparisons PASS (native null / legacy undefined
both keep the same default unbranched projectile appearance). Compile 0 errors /
warnings, 272 Node tests and preflight/metadata PASS. Network unification commit
b90ebdd5a08290eaa822178e27311c339a4d1f66 CI #816 SUCCESS (.NET 8 checks included).
Remaining: sphere radius/damage/delay/DPS read adapters, player/sphere/overlay V
models and remaining production compatibility bindings. No full completion claim.

### Native sphere statistics and transient snapshot retirement (2026-10-23)

Radius/damage/delay/DPS presentation calculations are now authored C#. Runtime
has no direct translated engineSpheres scope calls. Original source operation
order/constants, orbital pass-rate DPS and special cadence floors are preserved.
The native cached modifier input remains authoritative at this transition boundary.

Found an actual retired-adapter defect: evolved Orbital DPS called unsupported
V-string lastIndexOf and threw. The native reader parses the existing marker and
is checked against original TypeScript for that case, rather than the broken adapter.

All transient component layers now bypass V dictionaries in UpdateSourceRenderState.
XP/health/runes/chests/drones/particles/sphere projectiles and enemy bullets are not
serialized into duplicate per-frame model arrays. Native counts keep the original
load/stride rules. Enemy bullets are grouped in reusable native lists in owner order,
not scanned quadratically and not reordered. Native network positions read drones
directly. Lists release entity references on world disposal.

Validation: 800 radius/delay comparisons; 1170 legacy stat comparisons with artifact
inputs; 30 broken legacy Orbital DPS comparisons explicitly excluded and covered by
original TS instead. Original TS artifact-free scenes: 1200 comparisons, max relative
error 1.54e-16. This does not claim full artifact-instance modifier payload identity.
Real paused Editor: 35 transient-retirement/count/order/projectile-field checks PASS;
12 bounded native frames, SourceUiDocument instances 0, stop with current Console
0 errors/0 warnings. No saved game or input mutation. No FPS/GC/device claim.
Remaining: player/sphere/overlay V models and remaining compatibility bindings;
production modernization is not marked complete.

### Production reference-engine retirement and native regions (2026-10-23)

All 52 translated Visual_* modules, generated legacy initial state, React/DOM/hooks,
SourceUiDocument/layout/range/environment and SourceCanvasSurface are Editor-only.
The original Canvas command sink/trace and browser date host are also Editor-only.
NativeRunSnapshotDefaults preserves initial display values without a production
reference-module dependency. Native menu/run controllers remain the active path.
The reference .NET harness explicitly defines UNITY_EDITOR; its source expressions
and frozen reference outputs are unchanged. Exporters preserve the same guards.

Region names, pocket geometry/colors, original Basin world-pocket alias and six
phase names now live in the native ScriptableObject catalog. Native phase reads
preserve the exact source boundary comparisons. Architect formation and network
indicator reads no longer consult compatibility bindings. Production Runtime has
no translated Scope invocations and no dynamic .Call invocations (guard enforced).
Cosmetic shake uses the same existing cosmetic Random stream directly; gameplay
RNG is untouched. Pause formatting calls the equivalent C# math/format functions.
A native ProfilerMarker replaces the old timing/no-op console dispatch.

Damage numbers and lightning effects now read their existing native state lists
without per-frame V dictionaries/arrays. Original draw order, Orbital damage pass,
stride thresholds, alpha math and chain fallback remain unchanged.

Validation: region catalog 154 exact comparisons; HUD 420 comparisons / 26 cases;
pause stats 1760 comparisons / 18 cases PASS. Full reference .NET 10 harness PASS
(2663 drawing scenarios, 76 UI trees and existing CSS/animation/paint contracts).
272 Node tests PASS; preflight/provenance/meta PASS. Editor compile 0 errors/warnings.
Real paused Play Mode: 12 native frame constructions, 0 legacy UI documents; this
is a frame-construction smoke check, not 12 visually accepted rendered frames.
Unity Android C# compilation PASS (20 assemblies), no APK/player package build.
Inspected Android Assembly-CSharp: all 10 retired renderer/DOM/Canvas/date types
absent. Initial Windows C# compilation also produced its assembly; its first CLI
response timed out, so final Android compilation is the authoritative check.
Previous aec23928fb9ca7f5193957f8ac7b160c09e50f58 CI #818 SUCCESS.

Remaining: typed player/sphere/region/character overlay read models and retirement
of remaining plain V display data. Some stateless authored CSS/geometry math remains
shared by native graphics. Full visual/control/device acceptance remains open;
no FPS/GC measurement or full migration completion is claimed.

### Direct player and character indicator views (2026-10-23)

Player artwork now reads a typed State directly from PlayerCore/runtime, bypassing
its V dictionary projector. Engineer/Architect/Berserker world indicators and
Hunter/Alchemist target indicators read existing Unity sphere/enemy components.
Removed the string-dispatched character drawing bridge. Shield reads native
charges directly. Previous shieldTimer/shieldVisualPulse/deathTimer presentation
values were never updated by the old snapshot path; their zero behavior is
explicitly preserved, without adding new gameplay or animation wiring.

Validation: all 10 character player views, 120 exact field comparisons PASS in
paused real Editor, with character field restored. Editor compile 0 errors/warnings.
The legacy player projector remains Editor-only for read-model comparisons.
CI #819 on 65465fc00eee0fd8342efc55e9566f7b91a19287 failed its warnings-as-errors
policy: production exclusion exposed two never-assigned depth-plane clip fields.
Native menu plane creation now explicitly initializes their existing null/default
values. Local actual-Unity-API typecheck: Android player 0 errors/0 warnings;
Editor configurations checked separately below/on CI. No visual behavior change.
Remaining: sphere/region and remaining run UI plain V data boundaries, plus final
visual/control/device acceptance. Full migration is still not marked DONE.

### Retired CSS engine and explicit sphere drawing calls (2026-10-24)

CSS node/cascade/selector/length layout, grid, animation, CPU paint, CSS transform,
SVG DOM serialization, old range/timer hosts and old inline/flow layout are now
Editor-only. Native graphics retain only required stateless token/color/math helpers.
Legacy sheet-based overloads of typography/depth/filter adapters are Editor-only;
shared native FontAsset, GPU blur, depth composition and UI Toolkit filters remain.

Sphere drawing no longer allocates JS-style V[] argument lists or uses VisualJs.Arg.
Mutation/element/modifier/family/orbital calls have explicit C# parameters. Standard
sphere target checks and Orbital attack points read alive native EnemyAgent2D
components directly, preserving list order and source coordinate precision.

Preceding a606631ecb320db399b3765c770555e1901677b1 CI #820 SUCCESS. Local real
Unity API typecheck after CSS retirement: Android player, Editor runtime and Editor
assembly all 0 errors/0 warnings. Real Android C# compilation: 20 assemblies PASS,
no APK. Editor compile after sphere calling-convention changes: 0 errors/warnings.
Remaining plain V scene/sphere/run-UI snapshots are data adapters; their complete
retirement and final visual/control/device acceptance are still open.

Final bounded checks for this group: Android C# compile 20 assemblies PASS; 12
more native frame constructions in paused Play Mode, legacy documents 0; current
Unity Console errors/warnings 0 (historical Pipeline entries are retained). Inspected
Android assembly: 12 retired CSS/layout/SVG-host types absent. 272 Node tests and
preflight/provenance/meta PASS. No FPS/GC or full visual acceptance claim.

### Confirmed stale player display bindings corrected (2026-10-24)

Source-authoritative follow-up supersedes the earlier bounded decision to preserve
zero display timers. Inspection confirms original engineLoop/engineAbilities update
level, shieldTimer/shieldVisualPulse/deathTimer, mutation/buff/Chaos Orb/teleport/fire
catalyst values, and native runtime already updates their exact counterparts.
The presentation snapshot omitted them and silently retained initial defaults.
Architect formation type/change time was omitted as well. Added 12 explicit live
bindings. HUD and pause level, displayed sphere statistics and existing shield/death
artwork now consume current native state. Typed player/shield views read actual
shield/death timers, preserving original renderer.ts behavior rather than the
broken stale adapter. No simulation, formulas, RNG or new effects were added.

Real paused Editor: 50 checks for 12 live inputs, typed player fields and HUD level
PASS with all temporary native field values restored. Editor compile 0 errors /
warnings. Previous 1e26a1924c64762e69e49fd96e9f6385209db626 CI #821 SUCCESS.
Still no complete device/visual/performance acceptance claim.

### Direct native HUD/region views and reusable POI presentation (2026-10-24)

Production HUD values now project directly from PlayerCore, runtime counters,
Core network shapes and native enemy/POI records. The V-based HUD projector remains
Editor-only as a reference. Region world composition reads native region state,
not V dictionaries. POI presentation uses a reusable struct list copied from the
existing RegionFieldVisuals data; it no longer allocates a save-game PoiSnapshot
on every display update. Persistence CapturePoiState/RestorePoiState is unchanged.

Confirmed another stale adapter input: MaxSpheres is stored in the dedicated
_maxSphereAbilityLevel field, but the old UI copied only _abilityLevels. Explicitly
projecting the existing GetAbilityLevel(MaxSpheres) restores the correct existing
HUD/mobile maximum; no sphere limit rule or balance changed.

Real paused Editor comparisons: 24 live HUD scenarios / 576 checks PASS, RU/EN,
both regions and modes, three phase times, unknown-pocket fallback and three
max-sphere levels. All temporary runtime fields restored. Editor compile 0 errors /
warnings; Android C# compile 20 assemblies PASS without APK. Preflight/provenance /
metadata PASS. Previous 95173ab58adff787c5c52c3b0298847ddf068bbf CI #822 SUCCESS.
Remaining: sphere/read-only planner/modal/mobile plain V data seams; final device,
visual and control acceptance. No full modernization DONE or FPS/GC claim.

### Persistence boundary follow-up (2026-10-24)

Explicitly exclude new region-POI/display-count/projectile-group caches from both
run snapshot capture and restore. They are derived presentation data, not run
state. Generic Unity-object filtering already excludes projectile component lists,
but primitive display counts and the new POI struct list also require this explicit
boundary. Persistence format and authoritative run fields remain unchanged.

Paused Editor snapshot capture: 6 checks PASS; 273 nodes, no PlayerPrefs write,
all five caches absent and authoritative level retained. Previous
e8f67d09b51d9a59456193322be6468a719cbc2e CI #823 SUCCESS.

### Typed sphere presentation and native mobile dock/controller (2026-10-24)

All sphere frame/family/mutation/element/modifier/Orbital drawing now receives a
NativeSphereVisual projected directly from SphereAttackAgent and native evolution
state. No V sphere/player dictionaries are passed between these drawing functions.
Identity uses the stable native component. Existing draw ordering, source logical
coordinate conversion, rotation signs, timer projection, base presentation radius /
delay, branch/final selection and RingCounts formulas are preserved. Orbital's
read-only radius calculator remains one explicit legacy-data boundary; it is not
replaced with an approximately equivalent gameplay float formula.

Mobile dock reads native cooldowns, follow/lock/strain, dash, selected sphere and
ability evolution names. The controller gates actions using actual native modal /
pause/game-over flags. Existing keys/placement/Ghost preview seams remain.
Found four additional stale display bindings: shield/firetrail/minion/lightning
cooldowns never left their initial zeros in the old snapshot. The native dock now
reads GetActiveAbilityCooldown directly, and those snapshot bindings are corrected
for remaining data consumers. No ability timing/rule/balance changes.

Real paused Editor: 390 sphere family/branch/final/angle/tier cases, 6630 exact
projection-field comparisons PASS. Native absent branch canonicalizes null to empty
string, preserving the same existing unbranched drawing path. Mobile ability names:
624 RU/EN stage/branch/final cases PASS; cooldown input/text/fill checks included,
696 comparisons total. Runtime dictionaries/timers restored; temporary sphere and
its dictionary entry destroyed. No ability activation, simulation or save mutation.
12 additional native frame constructions PASS with legacy documents 0; this is not
full visual acceptance or a measured FPS/GC run. Editor compile 0 errors/warnings.
Remaining: Orbital radius/planner/modal data adapters, gesture/Ghost preview models
and final visual/control/device acceptance. Full migration not marked DONE.

Android C# compile after sphere/mobile changes: PASS (20 assemblies), no APK.
272 Node tests plus preflight/provenance/metadata PASS. CI checked on containing
commit separately from Unity Editor/Android script compilation.

### Typed Ghost Snap and formation-memory graphs (2026-10-24)

Production preview/current graphs and captured formation selection now use Core
SphereNetworkState/SphereNetworkShape and native node records. No graph serialization
into V dictionaries is needed for Ghost Snap or formation-memory comparisons.
Current-graph disabled behavior and preview-only disabled filtering are preserved
as separate existing source policies; logical axes, float conversion order, candidate
selection, ordered membership and first-sphere empty-preview behavior are unchanged.
Native graph rendering reads sphere/camera positions directly and keeps existing
pointer geometry/thresholds/placement gates. V pointer handles/keys remain for the
next controller boundary; their action semantics are not guessed or rewritten.

Checks: typed graph vs previous Core-backed adapter, 231 complete exact graph
comparisons PASS. Existing original TypeScript UI geometry parity: 105 cases PASS;
99 unused strength/distance float differences remain explicitly acknowledged, not
claimed bitwise original-TS identity. Editor compile 0 errors/warnings; Android C#
compile 20 assemblies PASS without APK. Previous sphere/mobile commit
387ab14493d7150a2442622e4f9b694d9b246897 CI #825 SUCCESS.
Remaining: gesture pointer/keys, planner/modal data and Orbital radius adapter;
final visual/control/device acceptance. Full modernization remains incomplete.

### Native movement input and Orbital radius boundary retirement (2026-10-24)

Mobile joystick/dash direction is a native Vector2. Removed all production Runtime
keys-dictionary reads/writes and SourceUiMovement. Original .2 directional thresholds,
12-unit dead zone, transient dash direction and keyboard Input.GetAxisRaw addition
remain unchanged. New transient input field is excluded from run capture/restore.

Orbital radius now reads native player/ability/artifact/character/sphere data in the
same double operation order as the established source-stat reader, without a V
snapshot. Preserves nearest-eight stable local selection, Engineer component size
(with distinct coincident spheres), mastery thresholds, mutation/buff multipliers
and native source modifier input. No approximate float gameplay helper substituted.
All sphere drawing is now independent of V entity/player data, including radius.

Real paused Editor: 144 direction/dead-zone/NaN/Infinity comparisons PASS, clear and
save exclusion PASS. Exact radius comparisons: 540 cases PASS across all characters,
six mastery levels, three geometry layouts and three radius-artifact loadouts;
mutation/radius buff covered. Temporary fields/lists/artifacts fully restored,
no ability activation or PlayerPrefs write. 12 native frame constructions PASS,
legacy documents 0. Editor compile 0 errors/warnings; Android C# 20 assemblies PASS.
Previous fdc31d2eeba7722090c1440a2b80554bd52dd8ed CI #826 SUCCESS.
Remaining: gesture sphere handles, read-only planner/modal models and final visual /
control/device acceptance. Full migration still not declared complete.

### Native modal models, artifact collections and direct controller commands (2026-10-24)

Upgrade/evolution cards now build typed inputs from RunUpgradeChoice, native
pending-evolution state and ScriptableObject text/card metadata. All sphere and
active-ability branch/final ordering, categories, levels, element badges and hints
are preserved. Regular card identity now uses GetUpgradeChoiceKey, the same key
actually written by LockLevelUpChoice and Oracle forecast selection. Confirmed old
adapter mismatch: source-format keys never equalled native lock/forecast keys,
so the UI could omit locked/forecast markers. This corrects display identity only,
not choice selection, lock/reroll rules, costs or RNG.

Owned artifact cards, reward candidates and set details now use native string lists
and catalog records. Typed synergy reads preserve source requirement/order behavior.
Pause chrome and modal visibility/priority read native flags/counters; combo text
reads actual runtime values. Mutable pending-card/lock/reroll UI snapshots are now
Editor-only. Upgrade/reward/tutorial actions call existing native commands and
refresh read models; production no longer calls ApplySourceUiWrites. Mobile flash
uses the existing native message method and preserves the old repeated-text guard.

Validation: 268 upgrade/evolution states, 9636 comparisons PASS (RU/EN, all sphere /
active-ability branches/finals and ordinary kinds). Routine lock/Oracle keys are
intentionally compared against actual Core keys, not the broken old identity.
Artifact/reward model integration: 90 states / 16518 comparisons PASS. Independent
original PausePlanner artifact check: 40 cases / 10192 comparisons PASS. Modal
visibility matrix: 128 cases; live pause chrome 80 cases; 1696 checks PASS.
Temporary flags/dictionaries/lists restored; no pick/reroll/grant/roll/save invoked.

Actual Editor Play Mode: 12 stepped frames (not manual LateUpdate-only calls),
UsesNativePauseChrome=true, UsesNativePauseScreen=true, UsesNativePauseStats=true,
legacy UI documents=0; flags and autosave accumulator restored. Current Console
errors/warnings=0. This is bounded activation/layout smoke, not full visual or
interactive acceptance for every modal. Editor compile 0 errors/warnings; Android
C# compile 20 assemblies PASS, no APK. Previous 8baecffe90821681a972e5e45e7b7f6d1f2d6f99
CI #827 SUCCESS. Remaining: gesture sphere handles, remaining scene/overlay and
read-only stats/abilities/synergy models, final visual/control/device acceptance.
Full migration not marked DONE; no FPS/GC values claimed.

### Native world-frame and overlay inputs (2026-10-24)

RenderNativeFrameContents/overlay orchestration no longer references V or the
compatibility scene snapshot. Camera/player positions, world bounds, shake, alive
enemy count, message/banner and boss-arrow inputs are typed native data. World
network nodes read sphere components directly; formation memory reads its native
points/timer/type; Architect world geometry reads the cached Core formation.
Existing mesh/shader/opacity/ordering and cosmetic Random stream are unchanged.

Removed unreachable iteration over the old fireTrails segment array after checking
all source/native references: current source only initializes an empty array and
ages/removes entries; no producer exists. Native source simulation likewise never
writes that list. The native segment drawing primitive remains available; a guard
rejects added direct source segment producers until their live layer is ported.
Existing FireTrail ability/buff rules and their visuals are untouched.

Checks: 32 frame-input scenarios / 560 exact scalar/camera/shake/message/arrow and
world-network input comparisons PASS, including zero-direction boss arrow and NaN
message-timer truth behavior. Temporary boss, flags, camera and artifact list restored.
Android C# compile 20 assemblies PASS. Actual paused Editor: 12 stepped frames,
native pause/chrome/stats Ready=true, legacy documents 0; original flags/autosave
accumulator restored. Bounded layout/activation smoke only, not full visual/control
acceptance or FPS/GC measurement. Previous 024160ceb13b5e8e28debe7aad18b43853a23e02
CI #828 SUCCESS. Remaining: gesture sphere handles and stats/abilities/synergy read
models, final visual/control/device acceptance. Full modernization not marked DONE.

### Native pause read models and formation tooltip (2026-10-24)

All four pause tabs now build their display models directly from native runtime
fields/components and imported Unity catalogs. Abilities, sphere/character stats,
synergies, build diagnostics and protocol visibility no longer consume V scene
snapshots. The formation HUD tooltip likewise uses typed Core shapes and active
native sphere types. Old V projections/formulas are Editor-only reference oracles.
Existing double display operation order, local-distance tests, duplicate-sphere
identity, cached formation efficiency, artifact ordering, branch/final labels and
unselected-final display fallback are preserved. No combat formula was changed.
The bare echoaccumulator evolution marker has no producer in the native adapter;
its unreachable display branch stays in the Editor oracle, with no invented rule.

Checks: live abilities 60 complete model comparisons; live synergy/diagnostics/
protocols 216 complete model comparisons; live stats 2160 complete model comparisons
plus exact damage/delay/DPS equality for every placed sphere, all ten characters,
mastery thresholds, artifact loadouts, branches/finals, timed buffs and cached
formation conditions. Temporary native state restored. Independent original
PausePlanner checks: abilities 56 cases/5944 checks; stats 18/1760; synergies
64/41440 PASS. Android player/editor C# typechecks, 0 errors/warnings;
272 Node tests and project/provenance guards PASS. CI #829 on previous
adfbc56a7861b4dde3cad08e7a8bcaa1a944eeca SUCCESS.

Remaining: mobile pointer sphere handles, the residual compatibility snapshot
producer/controller seams, final visual/control/device acceptance. Full migration
not marked DONE; no FPS or GC measurements claimed.
Real Editor activation/layout smoke: all four pause tabs individually Ready=true
with native chrome after 12 stepped frames each (48 total); original pause/menu/
tab/autosave fields restored, Play Mode stopped. Current Console 0 errors and
0 warnings. This does not establish complete visual/control/device acceptance.


### Production compatibility retirement (2026-10-24)

Native sphere pointer handles replace V dictionary identities. Captured positions
retain the old inactive/destroyed-pointer behavior, including the position refresh
performed by a move validity lookup. Hit radii, strict boundary comparisons,
placement/reposition gates, haptics and tutorial restrictions are unchanged.
HUD slot swaps preserve the last presented native formation types. Tutorial gates
and all native command refreshes read native state. Native POIs still refresh at
the same presentation boundary, without constructing duplicate generic dictionaries.

LateUpdate and native actions no longer call UpdateSourceRenderState or
UpdateSourceUiState, including in the real Editor. Those methods, generic object
projection, source snapshot defaults, image/JSON compatibility and the complete
V/VisualJs host/callable implementation now compile only under UNITY_EDITOR.
NativePresentationMath retains the exact number formatting and shared cosmetic
random stream; persistent interface-scale parsing uses the equivalent direct
invariant numeric conversion. The Android C# semantic guard proves absence of
V, VisualJs, VisualJson, IVisualHost/IVisualCallable, SourceCanvasImage,
NativeRunSnapshotDefaults, SourceUiDocument/SourceCanvasSurface and all five
snapshot/update/writeback members from the compiled player.

Verification: Editor compile 0 errors/warnings; Android player/editor runtime/editor
assembly typechecks 0 errors/warnings, retirement guard PASS; 272 Node tests,
project/provenance/asset metadata guards PASS. Full portable visual/source contract
suite PASS, including 38694 poses, 1580 shear cases, 5000 matrices, 934 clipped
brushes, 1230 SVGs and 68 original React-event frames. Native gesture lifetime/hit
check: 60 checks PASS, no gameplay command/save invoked; temporary actors restored.

Actual Editor after retirement: 12 stepped frames for each of the four pause tabs,
all respective native views and chrome Ready=true, legacy documents=0 and the
Editor reference snapshot remained Undefined throughout all 48 frames. Real Game
View menu and native world/HUD PNG captures inspected at 1260x542, with no panel
redirection in the successful captures. Native menu Ready=true. World capture used
the existing scene with paused simulation, not a combat stress or device test.
Menu/pause/tab/autosave/background fields restored; Play Mode stopped. Captures are
local diagnostic files under UnityProject/Library/native-*-screen-review.png, not
an assertion of all-effect visual parity. Current Console checked separately.
An earlier offscreen-panel diagnostic produced an empty target after UIDocument
root recreation; it was discarded, its temporary target destroyed, and the Editor
Play session restarted before the successful Game View captures. No production
render-target workaround was added.

Previous implementation 9d9ae8b0cd45d66d5d2f1c671ab0f99d6253357d: CI #830 SUCCESS.
Containing-commit CI must be checked separately. Remaining work is final broad
visual/control/device acceptance and measured performance; no remaining active
V/DOM/Canvas compatibility migration task was found. No APK, no gameplay/RNG/
balance/quality changes; new-desing remains unchanged.

### Shared native planner catalog and Editor reference asset isolation (2026-10-24)

A final dependency audit found that native region selection and upgrade/evolution
texts still entered EnsureKnowledgeLoaded, which parsed the old combined
SourceCatalog JSON in the player. Its 822 planner rows now live alongside the
existing 305 archive entries in NativeArchiveCatalog, a Unity ScriptableObject.
Knowledge discovery/save IDs, planner lookups, choices, source text/order, region
availability/challenges and all gameplay rules are unchanged. The pre-existing
305 serialized archive entries are identical after YAML whitespace normalization.
The importer preserves source commit/digest and every planner field.

SourceCatalog.json, SourceUiStyles.json and SourceUiIcons.json plus their original
GUID metadata moved from Resources into Assets/Editor/EchoSphereReference.
They are now Editor reference/import inputs, not player Resources. Their combined
raw size is 850298 bytes; this is not an APK size measurement or a claimed net
build-size reduction. No scene/prefab/material/asset serialized references point
to these TextAssets. Exporters, native icon import, portable checks and Editor
oracles use the new paths. Locked reference payloads are unchanged; provenance
updates are limited to the changed exporter paths/Editor-only generation guard.
The retired SourceCanvasArtIndex host table is also Editor-only, while the native
art catalog continues to reference the same Sprite assets and dimensions.

Checks: native planner 822 rows / 11508 exact field/order comparisons PASS, with
Unity's equivalent null/empty optional-string/array serialization normalization.
Actual Editor confirms NativeArchiveCatalog is the live data owner and all three
old Resources.Load<TextAsset> paths return null. Seven native navigation routes
(shop, leaderboard, achievements, characters, archive, settings, map) exercised
for 12 stepped frames each: native content present, no generated UI documents or
V snapshot creation. Navigation callbacks were invoked directly; this is bounded
route activation/layout coverage, not complete pointer or device acceptance.
Original menu/region route restored and Play stopped. Current Console 0 errors /
0 warnings, Editor compile clean. Android player/editor typechecks, 272 Node tests,
portable visual suite, metadata, source catalog/CSS/art and provenance guards PASS.
No APK, gameplay, quality or protected-branch changes. Check containing commit CI.

CI correction: #832/#833 passed source and Unity C# checks but the core job found
an old SourceUiStyles path in the range-control reference manifest. Updated that
manifest and all eight .cjs fixture/exporter path consumers; reference payloads and
image hashes are unchanged. Re-ran every command in the exact failed CI visual/
styles/artwork/provenance step locally: all eight checks PASS. A fresh HEAD CI is
required; no browser/reference artwork regeneration was performed.

### Complete source ability-ID coverage correction (2026-10-24)

The final identifier audit found a real typed-model mismatch: source proj_speed
maps to RunUpgradeKind.ProjectileSpeed, so generic Enum.TryParse skipped it in the
pause ability list. Native readers now reuse the existing PlannerAbility inverse
of SourceAbilityId (also used by the original planner), including branch/final
lookups. No projectile-speed formula, upgrade rule or gameplay state was changed.

The earlier native comparison fixtures had the same generic enum filter and thus
omitted this alias. They now resolve every source ID independently through the
forward SourceAbilityId mapping and fail if any catalog ID has no unique native
kind. Actual Editor comparisons re-run with all 30 ability IDs: abilities 60 full
models, stats 2160 full models plus exact sphere metrics, synergies 216 full models
PASS. Temporary state restored, Play stopped, Console 0 errors/warnings. Android
player/editor typechecks and 272 Node tests PASS. Prior d14c6f6b8a1a3936b85cd10e9423e6ffd8007f33
CI #834 SUCCESS; check the containing correction commit's CI separately.
