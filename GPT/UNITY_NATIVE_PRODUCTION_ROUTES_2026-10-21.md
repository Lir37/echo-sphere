# Native production UI routes — 2026-10-21

Entry HEAD: 84c2c6533ce5b582ec12cb8795c7a5c454c3beb2, unity-migration.
Implementation HEAD is the containing commit. Source new-desing remains protected.

The user changed the work order: finish the native Unity implementation first;
measurement and dedicated acceptance campaigns follow later. This supersedes older
notes requiring more isolated compositor tests before selecting native components.
Compilation and CI remain technical gates; neither proves full game equivalence.

## Implemented and selected

- Shop, leaderboard and achievements now have native retained UI Toolkit trees,
  ScrollView/Button/Label input, typed display models and direct runtime callbacks.
  Their normal routes bypass SourceUiDocument, generated TSX, CSS reconciliation and
  SourceCanvasSurface creation. Existing Home remains the native owner.
- Shop reads PersistentUpgradeRules and existing save data; purchase retains the
  original affordability/max-level guards, price, level increment, shop/gold schema
  and next-run application. No extra message, bonus or stat rule was added. Labels
  and descriptions reuse the matching existing C# definitions.
- Leaderboard reads the existing sorted saved entries; achievements read the same
  unlocked IDs. NativeProgressionCatalog.asset is a native ScriptableObject containing
  the 12 exact original achievement IDs and RU/EN texts. Its Editor importer reads
  the locked generated definitions once; the player UI does not execute their TSX.
- Region selection is a native UI Toolkit screen: two region cards, stabilization,
  six challenge actions, completion/lock states, Endless availability and original
  descriptions. Data comes from the existing source catalog and progress functions.
  The six challenge accents are copied explicitly from EchoMapScreen.tsx, since the
  shared planner catalog does not contain those colors.
- Region start writes exactly the existing selected-region, pending challenge,
  pending Endless and reload-start preferences, then reloads the active scene so
  player/spheres/character/shop stats are constructed by the original run path.
- Settings is dispatched before source Canvas/document creation. Scale storage now
  uses PlayerPrefs directly, retaining the source .75–1.25 limits and absent/empty
  storage conversion. Existing language, sound, handedness, replay and reset actions
  are retained. No DOM Settings screen is mounted as a prerequisite.
- Native HUD and the already implemented unified pause screen, chrome and all four
  content views are now the default production selection. Existing diagnostic APIs
  retain their names; their default-off policy is superseded by the user request.
- Authored backgrounds, vector icons, gradients, borders, inset/outer shadows and
  blur are expressed through native GPU/UI Toolkit components. The shared brush now
  supports the second authored inset shadow; its unset default leaves prior paints
  unchanged. Six additional Lucide geometries are imported as VectorImage assets.
- New UI owners/state are excluded from run snapshots; screen resources are disposed
  on runtime destruction and native temporary card/filter resources on page hiding.

## Scope and technical status

Actual Unity 6000.3.24f1 compilation: zero errors / zero warnings. ScriptableObject
import completed in Edit Mode with 12 achievements. No game preferences were changed
by the content importer. CI must match the containing implementation commit.

No new manual test campaign, benchmark, Profiler capture, screenshot comparison or
device acceptance was run. No FPS/GC improvement is claimed. The new selected pages
and default HUD/pause are implementation, not an assertion of completed visual/input
acceptance. Existing historical measurements are not current-route measurements.

The pause still attaches to the source outer shell; HUD still shares the source
game screen. NativeMenuGlass can still use the existing retained-prefix snapshot
fallback when overlapping native text/vector paint needs it. These dependencies are
explicit remaining migration work, not reclassified as fully native architecture.

## Continue implementation

1. Top-level menu screens are implemented natively, including characters and archive in following groups; continue with gameplay overlays.
2. Move remaining upgrade/artifact/detail/result/tutorial/mobile controls out of TSX.
3. Replace pause outer shell and retained-prefix fallback with the native compositor.
4. Replace generated gameplay Canvas execution with native entity views and Unity
   graphics/VFX, retaining every authored effect and the authoritative simulation.
5. Then undertake the combined visual/input/performance/device acceptance pass.

No gameplay/balance/RNG changes, no new mechanics, no protected branch edits, no APK.
ProjectSettings.asset and _Recovery remain user-owned and outside this change.

Short handoff: continue unity-migration at the containing HEAD. Native Home/map/shop/
records/achievements/characters/archive/Settings routes are selected; native HUD/four-tab pause now
default on. Complete overlays and gameplay views, then native
shell/compositor. Implementation first; defer profiling/standalone fixtures. Preserve
gameplay, visuals, new-desing and user changes; no APK.

## Following implementation group — character selection

Entry bccdfd283ccf6ddc38ba420912fd7790cda570e5. NativeCharacterScreen now owns
character selection, unlock error, signature/partner tags, mastery progress, expandable
ten-level descriptions, next-level XP and completion display. Typed CharacterRules/
GameCatalog definitions and existing profile saves supply the model; no generated
CharacterSelect component is invoked by this route. Native glyph/vector, original
card paints/shadows, hover movement and global button blur remain implemented.

Selection/unlock follows the actual TSX + persistence binding. The old IMGUI helper
was deliberately not reused: it additionally rewrites prepared HP/starter spheres,
while the actual source CharacterSelect does not. Unlock charges the original cost
once; selection persists the same ID and mastery level. The source previous-threshold
calculation is retained for the displayed progress bar. State changes rebuild native
controls only on actions; normal frames retain them, including scroll position.

Editor compilation passes with zero errors/warnings; no new Play/Profiler or pixel
campaign was run. CI776 had passed API and TypeScript checks but failed an old Settings
structural test that searched the former bridge file for moved callbacks. The same
assertions now inspect EchoSphereNativeSettings.cs; the targeted existing test passes.
The containing commit still requires a new complete CI result. No mechanics/APK changes.

## Following implementation group — archive

Entry b8bd6865b66b5715951d5c5a690f932619707c6c, CI777 SUCCESS. NativeArchiveScreen
now owns all seven archive sections, native TextField search without rebuilding the
focused field, responsive navigation/two-column cards, locked entries, concept cards,
Resonance paragraphs and the three evolution groups. Existing knowledge IDs and
unlock-count semantics remain authoritative. Evolution queries intentionally retain
the source behavior (the source does not filter that section).

NativeArchiveCatalog.asset contains 305 typed native entries. The content importer
uses exact source strings, and corrects the generic catalog's ability descriptions
from the original progression level-I descriptions, matching KnowledgeBase.tsx's
actual fallback order. Original seven-level/evolution explanatory copy is retained.
The player archive does not execute generated TSX or dynamic progression functions.
Its main surface belongs to the ScrollView viewport, not a texture spanning the full
height of every archive entry. Native button blur still shares the documented prefix
fallback; its removal remains separate native compositor work.

Editor compilation passes with zero errors/warnings; import completed with 305 entries.
No Play/Profiler/pixel campaign was run. CI must match the containing archive commit.
No new gameplay rules, unlock conditions, source branch edits or APK.

## Following implementation group — results and upgrade picker

Entry c92e2b082547bd76e4f859c2b966e1802f93805a, CI778 SUCCESS.
NativeRunResultView now owns final statistics, record text/pulse and Return input.
Original result values and its default Basin challenge/Endless accessors are retained.
Return preserves the existing clear-snapshot/open-region-select/reload handoff.
The record pulse uses native weighted AnimationCurve tangents for the original
cubic-bezier(.4,0,.6,1), including the original two-second timing.

NativeUpgradeView now owns upgrade/evolution cards, element badges, Oracle forecast,
Lock/Reroll controls, future-synergy hints and their orbit artwork. Existing runtime
ChooseUpgrade/ChooseEvolution/LockLevelUpChoice/RerollLevelUpChoices handlers remain
authoritative. Their source-write synchronization is retained. Presentation helpers
still supply original names, descriptions and compatibility hints; this group does
not rewrite gameplay progression or the remaining data projection.

Card state is retained while its signature is unchanged. Input is consumed once per
bound choice update; a no-op action can rebind on the next source frame rather than
leave controls permanently disabled. Original responsive 1/3-column choice layout and
disabled/locked visibility semantics are implemented. Existing tutorial wrapper target
remains available. Native buttons register with the existing ordered backdrop owner.

These controls are attached to the existing game overlay frame. The source black
overlay, world prefix, CSS shell and their compositor dependencies are explicitly
still present. Original generated content is removed after native geometry is ready;
fallback content supplies initial layout warmup. This is not a completed source-free
game UI or complete gameplay renderer migration. Artifact/detail/tutorial/mobile and
world presentation remain next.

Editor compilation: zero errors/warnings. No new benchmark or dedicated Play/pixel
campaign; no visual/device acceptance claim. CI must match the containing commit.


## Following implementation group — artifacts, tutorial and mobile dock

Entry 097b196feb4f82688d7f118e9483b58b3adbd577, CI779 SUCCESS.
NativeArtifactRewardView owns reward cards, mechanic labels, newly activated synergy
hints and pick input. NativeArtifactSetDetailView owns the set popup in rewards and
pause: conic header, progress count, owned/unowned rows, native glyphs, scroll and
close/outside input. The shared artifact display projection now accepts candidate
IDs while retaining actual inventory for set progress. Existing ChooseStellaReward
and save/write synchronization remain authoritative.

NativeTutorialView implements all six original steps/translations/advance guards,
target sampling every120ms, native spotlight shader, native card layout and original
two-second ring pulse via AnimationCurve. Movement distance uses the same72 source
pixels; placement checks the same spheres.length. Upgrade step still advances only
through the actual choice handler. No extra tutorial rules were introduced.

NativeMobileControlsView owns Pause, movement Lock, Follow, ability/cooldown buttons,
Dash, selected sphere information and the ten sphere selectors as UI Toolkit controls.
Source-important visual rules remain: rectangular controls, circular selectors,
normal/pressed gradient and shadows, native VectorImage icons and native glyph alpha
shadows. Brush shadow spread now supports the original aura and negative-spread
information panel shadows, preserving the previous three-argument API.

The mobile dock captures the original pointer callbacks before source-tree projection.
This intentionally preserves lastDirectionRef, original haptics, guards and existing
follow/dash/ability handlers rather than approximating input. A semantic native-button
marker preserves the original closest(data-mobile-control) placement guard, including
disabled buttons. Original dock positioning/scale frame and outer gesture root remain;
this is native button presentation, not source-free input ownership. Source joystick,
ghost preview, formation-memory and placement effects still need native owners.

All replaced source children are removed after native geometry warmup. Outer source
black/blur shells, source tree/data projection and ordered compositor remain explicit
dependencies. Upgrade title spacing and original text shadow are also retained.

Editor recompilation: zero errors/warnings. No dedicated tests, Play/profiling campaign,
FPS/GC or device parity claims. No APK; protected source and user ProjectSettings /
Recovery files untouched. CI must match the containing commit.

Next: replace mobile feedback and gesture ownership, gameplay/world drawing and remaining
source overlay/compositor dependencies. Full migration and P1/P2 remain PARTIAL.
Short handoff: Continue ECHO SPHERE on unity-migration at the containing commit
(git log -1 --format=%H). Read the three GPT documents and this report. Native menu,
pause/HUD, results/upgrade, artifact rewards/detail, tutorial and combat dock are
selected; continue mobile feedback/input and world rendering, then remove source-shell
and projection dependencies. Preserve original behavior/quality, no APK. Compile/CI;
performance and dedicated acceptance follow implementation per user instruction.


## Following implementation group — complete mobile feedback geometry

Entry 5adcaeffd59ea9724b53b6c3adb2b8d7d64add9e, CI780 SUCCESS
(run39643220694). NativeMobileFeedbackView owns the joystick base/knob and placement
rings, including the original ordered2px backdrop blur, knob opacity/glow and
placement ping via a native weighted AnimationCurve. The existing450ms placement
lifetime and interface-scale/position frame remain authoritative. Source-important
border specificity is preserved: the thin inner ring and joystick circles use the
authored cyan border override, while the thick ping retains the selected sphere tint.

NativeMobileNetworkView owns the ghost network links, formation polyline, valid/
invalid marker and localized caption, plus formation-memory paths and circles.
Unity Painter2D performs native path tessellation, exact dash patterns, circle arcs,
fill and stroke. A native TextCore Label retains the original font/anchor/baseline.
The view consumes explicit computed display coordinates from the existing component,
not a runtime SVG/path parser. Original preview validity, topology, world projection,
colors, widths and memory alpha remain unchanged. Replaced generated shapes/texts
are removed after native warmup.

The remaining source mobile root still owns gesture state, timers and display-data
projection. Layout/order frames remain source-owned; native geometry does not yet
mean source-free controls. The world renderer, gesture/controller owner and remaining
source UI shell/compositor are still unfinished. P1/P2/full native migration PARTIAL.
Editor compilation passes without errors/warnings. No new tests, profiling, Play or
Android acceptance campaign; no measured FPS/GC claim. No APK or source-branch edits.

Short handoff: Continue ECHO SPHERE unity-migration at the containing commit
(git log -1 --format=%H). Read the three GPT documents, modernization progress and
this report. Native menus, pause/HUD, result/upgrade, artifacts/detail, tutorial,
mobile button dock and all mobile feedback geometry are selected. Continue with
native gesture/controller ownership, gameplay/world drawing and removing the remaining
source-shell/data-projection/compositor dependencies. Keep gameplay and visual quality;
no APK. Compile/CI only; dedicated measurement/acceptance follows implementation.


## Native mobile gesture/controller ownership

Entry d69cd0bb6ffe68757f782077c57f1fc42cd54bc0, CI781 SUCCESS.
EchoSphereNativeMobileGestures now owns the pointer dictionary, Unity pointer capture,
joystick direction, dragging/placement decisions, locked touches, tap completion,
formation-memory and placement timers. NativeMobileControl handles the actual dock
input. Existing SourceUiBinding actions, collision validation and network preview
helpers remain authoritative; their gameplay formulas and Core implementations are
not rewritten. Original12px deadzone,26px tap and34px drag tolerances, joystick side,
tutorial exceptions, pending guards, haptics and last-direction dash are preserved.
PointerCancel deliberately retains the original completion behavior.

EchoSphereNativeMobileState supplies typed dock/feedback/network models directly.
The original generated MobileControls component is replaced only for the synchronous
SourceGameView composition call, restoring the module entry in finally. It no longer
runs its React-style hooks, timers, generated button callbacks or SVG construction.
The remaining thin layout placeholders preserve source scale, position, z-order and
tutorial anchor while native views own their contents. Ghost and memory containers
are now ordinary layout boxes; no empty SVG parser/rasterization dependency remains.
DOM scans, callback capture, semantic pseudo-DOM buttons and source mobile projection
passes have been removed. Native button class markers protect placement even when a
control is disabled. Transient controller fields are excluded from run snapshots.

Still incomplete: native gameplay/world rendering, outer game/pause/overlay layout,
shared V/data projection and ordered shell/compositor fallback. Pure source display
helpers/catalogs remain transitional. No APK or protected-source change. Dedicated
Play, visual/device acceptance and performance measurement remain deferred by user.
Editor compile and CI are technical gates only; full migration/P1/P2 remain PARTIAL.

Short handoff: Continue ECHO SPHERE unity-migration at the containing commit
(git log -1 --format=%H). Read the three GPT documents and native modernization reports.
Native mobile input/state no longer runs the generated MobileControls component.
Continue gameplay/world drawing and remaining source layout/data/compositor removal;
preserve gameplay/quality, no APK. Compile/CI now, measurement/acceptance afterwards.


## Native gameplay-world field and ambient layers

Entry f27fa20189a100b48b0c9d5358bf0554aef7cbd3. CI782 exposed one CS0649 warning
for the now-unused mobile Tooltip field; it is removed in this group. CI otherwise
reported zero C# errors and green core/source checks. CI must match the new containing
commit before claiming SUCCESS.

NativeWorldField now owns the active drawVoidField artwork as retained Unity meshes
and shaders: exact two-circle gradient stops, three nebula disks, four near-field
plates, eight horizon lines and120 stars. Player focus updates shader uniforms;
geometry rebuilds only on field dimensions/origin changes. NativeWorldAmbient owns
the existing capped mote count, positions, wrap, sizes and pulse in a Unity vertex/
fragment shader. No RNG or gameplay state is introduced. Source's apparent circular
arena code is one commented line containing literal backslash-n sequences; it is
inactive and is intentionally not invented as visible geometry.

SourceCanvasSurface.DrawNativeMesh is the temporary ordered submission seam. Native
Unity meshes/materials are inserted between the original layers with the exact source
transform, camera shake and inherited opacity. Native entries are protected from legacy
paint-batch merging and bypass Canvas path tessellation/gradient-stop upload. The
source backend still owns the final target/composition. A minimal context continuation
preserves each replaced helper's final current path/fill for remaining source layers;
it emits no Canvas paint for the native world field or ambient drawing.

Premultiplied blending and native circle antialias fringes are retained. No quality
settings, effects, density, gameplay formulas or source branch are changed. Both new
shaders were checked through the actual Editor ShaderUtil messages after import:
no diagnostics. Editor C# recompile: zero errors/warnings and scriptCompilationFailed
false. This is compile/import confirmation, not Play/device or measured performance
acceptance. No APK and no new standalone measurement/test campaign.

Next: region/entity/attack/effect/text rendering, then remove the remaining source
render coordinator, UI shell, V projection and compositor fallback. Full native/P1/P2
remain PARTIAL. Short handoff: Continue ECHO SPHERE unity-migration at the containing
commit (git log -1 --format=%H). Read the three GPT docs and modernization reports.
Native mobile input/state and world field/ambient are implemented. Continue the actual
region/entity/effect renderer; preserve behavior/quality, no APK. Compile/CI now;
performance and dedicated acceptance after implementation.


CI wiring follow-up: CI783 passed Unity C# typecheck but its legacy provenance
assertion still searched for the source-render call in EchoSphereSourcePresentation.
That call now lives inside EchoSphereNativeWorld. The same active-frame guard now
checks the delegation, both native layers, remaining source render and source UI.
Source reference hashes and the remaining validation requirements are unchanged.


## Native region layer and world-label ownership

Entry a4ee54b257c16699484cbe1856a9d8194b17e8eb, CI784 SUCCESS.
NativeWorldRegion now owns the actual renderer.ts region layer: original REGION_POCKETS
alias, active/inactive ring width/alpha, six moving seams and eight moving triangular
markers per pocket. POI glow pulses and2px miter diamonds retain their original colors,
sizes, timing and per-object paint order. Existing region gameplay/spawn state is read
only; the dormant scene approximation is not enabled in place of the authored drawing.

NativeWorldLabel owns localized POI captions through native Unity Font/TextGenerator,
retained Mesh and glyph shader. It preserves bold8px sizing, centre/alphabetic baseline,
platform family/fallback, raster density and atlas sampling. Typed font resolution shares
the existing font owner without parsing a Canvas declaration. Atlas requests from native
labels and remaining source text stabilize together before either emits geometry, so a
later glyph request cannot silently invalidate the earlier layer's UVs. POI geometry and
text stay interleaved per object; moving all text after all POIs would change overlaps.

The original source region helper is bypassed during the ordered world frame and restored
in finally. Only a non-painted current-path continuation remains for downstream source
layers. Region labels no longer submit Canvas text commands. Native ring/arc geometry
uses analytic antialiased shapes; diamond/triangle edges retain the shared4x target.
No gameplay rules, density, visual effects, quality settings or protected source changes.

Actual Editor C# compilation: zero errors/warnings. New region/label shader messages:
empty; scriptCompilationFailed=false. These are compile/import checks, not Play/device
or FPS/GC acceptance. No new profiling/test campaign or APK. Remaining work: entity,
attack/effect/text layers, source render coordinator, UI shell and data/compositor bridge.
Full native migration and P1/P2 remain PARTIAL; CI must match the containing commit.


## Native pickups, summoned minions and fallback energy bolts —2026-10-23

Entry a83295e146b3877fdc2a282bbfacc37db33f381b, CI785 SUCCESS.
Completed the previously uncommitted pickup/minion group, including its C# declaration
fix. NativeWorldPickups owns the original XP crystal silhouette, outline, highlight,
radius and rotation. Health packs retain their cross, pulse, stroke and white bars.
Their four separate4px shadows are preserved in the original per-paint order, including
both overlapping white rectangles. Native shadow batches share the existing exact
padding/blur/composition owner rather than flattening or omitting those effects.

NativeWorldMinions owns the actual drawModernMinion artwork: two source ground-shadow
ellipses, bob, rotation, body/outline, three animated rays and two core circles. The
original ground helper's blur argument is unused; no new blur is invented. Radius-
dependent immutable art is retained. NativeWorldShapes uses Unity Vector Graphics
with the same contour/stroke/tessellation parameters, not a Canvas-command interpreter.

NativeWorldEnergyBolts owns only drawEnergyBolt (the fallback when the mutation-specific
chain renderer declines). The original seven-segment wave, timing, wobble, round caps,
inherited corners/miter and7px/1.5px paint layers remain. Meshes/property blocks are
pooled; inherited source-over/lighter/screen composition is retained by native shader
passes. Special mutation-specific lightning and the existing draw-stride policy are
unchanged. This is not a completed migration of all lightning or combat effects.

All these helpers are bypassed in the active synchronous render coordinator and restored
in finally. No spawning, pickups, summon logic, damage, RNG or gameplay formula changed.
The shared final render target and original per-layer ordering remain transitional;
non-painted current-path continuation is retained for downstream source drawing.

Actual Unity Editor recompilation reports zero errors/warnings; EditorUtility confirms
scriptCompilationFailed=false and NativeWorldSolid shader messages are empty. No new
Play/Profiler/device acceptance, FPS/GC numbers or APK. CI must match the containing
commit. Full native migration and P1/P2 remain PARTIAL.

Short handoff: Continue ECHO SPHERE unity-migration at the containing commit
(git log -1 --format=%H). Read three GPT docs and native modernization reports. Native
menus/input, field/region/world labels, XP/health, summons and fallback energy bolts
are implemented. Next: player/enemy/sphere views, projectiles, remaining combat VFX
and source renderer/UI/data/compositor removal. Preserve gameplay and visual quality,
no APK; compile/CI now, dedicated measurement/acceptance after implementation.


## Native imported artwork layer —2026-10-23

Entry89ae4344a40d9c803058ded9d07e17bd5edc7ea3, CI786 SUCCESS.
NativeWorldSprites replaces the actual drawArtSprite and drawStandardSpherePart
painting path with Unity imported Sprite assets, retained UV meshes, pooled property
blocks and a native image material. It covers the shared player/Spherist/shield art,
sphere silhouettes and Standard Sphere production parts at their existing call sites.
No artwork was regenerated, decoded differently or substituted.

The original getArtImage availability gate remains, including first-call false and
procedural fallback. Aspect fitting uses the same original image dimensions; sprite UVs
use the existing textureRect. Local rotation, parent/shear/scale, mirrored part axes,
alpha assignment (including invalid-value semantics), inherited shadow blur/offset,
source-over/lighter and forced-screen part blending are retained. The native shader
uses the same premultiplied bilinear sampling and gamma conversion as the current
image compositor to preserve transparent edges. Sprite drawing does not alter the
current Canvas path. Imported asset ownership remains with Unity Resources; the view
disposes only its own meshes/material.

This removes Canvas drawImage/transform command submission for the shared artwork,
not all player/sphere presentation. Original animation/death selection, procedural
fallback, standard core/rings and surrounding VFX still need native owners. The source
renderer coordinator, data projection and final compositor remain transitional.
No gameplay, graphics-quality setting, source branch or APK changes. Editor compile:
zero errors/warnings; scriptCompilationFailed=false and sprite shader diagnostics empty.
Play/device/performance acceptance remains deferred; CI is a separate technical gate.

Short handoff: Continue ECHO SPHERE unity-migration at the containing commit
(git log -1 --format=%H). Read three GPT docs and modernization reports. Native menus,
input, field/region/labels, pickups, summons, fallback energy bolts and the shared
player/sphere artwork painting layer are implemented. Continue actual player/enemy/
sphere representation and combat VFX, then remove source UI/render/data/compositor
bridges. Preserve behavior/quality, no APK. Dedicated measurement after implementation.


### 2026-10-23 — native hostile projectile presentation

Entry HEAD 1b216fbec576c99c265c1c8b9910ca34f69c0d00 (CI #787 SUCCESS).
NativeWorldProjectiles replaces drawEnemyProjectile and drawBossProjectile paint
submission for both enemy variants and all four original boss variants. Typed
Unity Vector Graphics meshes retain body/outline/tail geometry by radius, variant
and inherited stroke settings. Pooled property blocks carry colors and transforms;
rotating arcs, five summoner satellites and pulsing additive cores retain the source
constants/order. Source-over/lighter, seven-pixel enemy shadows and inherited boss
shadows/offsets remain ordered through the existing compositor. Final current-path
circle is continued without repainting. Owners are disposed and excluded from saves;
helper replacements are restored in finally. No projectile simulation, collisions,
RNG, damage or spawn changes. No approximate creature renderers were enabled.

Editor C# compilation: zero errors and warnings. This is implementation validation;
no Play/device acceptance or FPS/GC claim. Full migration remains PARTIAL: creature,
player/sphere procedural representations, telegraphs/VFX and source UI/data/render/
compositor bridges still need native owners. No APK or protected-branch changes.
CI status belongs to the containing commit and is checked separately after push.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT documents and native modernization reports. Hostile
projectile painting is native for all six variants. Continue actual enemy creature
geometry and remaining player/sphere/telegraph/VFX owners, then remove transitional
source bridges. Preserve gameplay and quality; no APK; measurements after migration.


### 2026-10-23 — complete active enemy representation and associated effects

Entry HEAD31f27283a0cf45bf4fa01d7e0eff80447f202f1f, CI #788 SUCCESS.
NativeWorldCreatures now owns the production drawModernEnemy route rather than
enabling the older scene approximation. Typed anatomy retains all ten enemy variants
and four bosses from enemyVisual.ts: quadratic body/wing/segment contours, articulated
legs, tails, eyes, cores and boss frame arcs. Unity Vector Graphics owns tessellation,
Unity meshes/materials own drawing, a dedicated native radial shader retains original
creature/core gradient stops and additive composition. Static art is retained;
animated segments/effects reuse mesh instances and retessellate when needed. This
has not established FPS/GC improvement and can still need native batching work.

Native movement-facing memory uses weak object identity with the original .04
threshold and initial rotation rule. Freeze affects the original outer/tier/bar
colors without recoloring the original creature body. Hit scale/cross, tier rays,
elite crest, all fire/poison/freeze paints, Link Breaker target warning and disabled
network glyph/ring preserve original constants, paint order, alpha assignment,
shadow offsets/blur, caps/joins and multi-contour strokes. Boss telegraph windows
come from the existing source balance constants; no new rules or timing values.
Boss health bars/name labels retain unrotated placement, health ratio, original
names/localization and Unity Font atlas synchronization. Native helper interception
is restored in finally; view ownership is excluded from saves and disposed.

Shared glowCircle (two additive discs, not a new gradient) and drawGroundShadow
(two solid ellipses; original unused blur argument stays unused) are also native.
Current-path continuation preserves the source painter state between remaining
layers, including multi-contour glyphs and rectangle/text path preservation.
No gameplay, visual-quality settings, source branch or APK modifications.

Actual Editor compilation: zero errors/warnings; scriptCompilationFailed=false;
NativeWorldCreatureGlow shader supported with no diagnostics. Play/device visual
acceptance and measurements remain deferred at the user's implementation-first
direction. CI for the containing commit is checked independently after push.
Full migration remains PARTIAL: player/sphere procedural views, other VFX and the
source renderer/UI/data/compositor bridges remain.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT documents and native modernization reports. Complete
active enemy representation, all 14 anatomies, associated statuses/telegraphs/bars,
shared glow/shadow helpers and disabled-network indicator are now native. Continue
player/sphere procedural views and remaining world effects, then remove transitional
source bridges. Preserve behavior/quality; no APK; measurements after implementation.


### 2026-10-23 — Standard Sphere procedural details and world pickables

Entry HEADf04cb64858882be638c2591d5e6e3dc92433bad7, CI #789 SUCCESS.
NativeWorldStandardSphere replaces drawStandardSphereCore, drawStandardSphereRing
and drawStandardAttackVfx. Native retained Vector Graphics meshes and pooled animated
meshes retain the original body, two elliptical ring contours, caps/joins, three-pixel
shadows, alpha assignments and additive/over ordering. NativeWorldStandardCore shader
uses the original two-circle focal radial solution and five authored gradient stops;
no substitute flat glow or quality reduction. Attack charging uses the existing
1.2-second display calculation and original three curves/arc/white core; gameplay
attack timers, damage, range and targeting are untouched. Source assembly and other
families' specialized VFX remain migration work.

NativeWorldPickableViews replaces all rune drawing branches (phase, harvest, purge,
resonance, fortify, hunt, echo, gravity, original fallback) and both ordinary/Stella
chests. Native Vector Graphics retains paths, source open/closed contour semantics,
animated rings/rays, multi-contour strokes, pulse, blur and per-paint ordering.
Stella text uses native Font/TextGenerator with original local transform, raster
scale, shadow and atlas synchronization. Existing rune definitions supply colors;
collection/spawn/reward behavior is unchanged. Shared text submission now accepts
an optional local transform and shadow settings, keeping old callers unchanged.
View owners are disposed and excluded from saves; scopes restored in finally.

Actual Editor compile: zero errors/warnings. Standard core shader supported with
empty diagnostics; scriptCompilationFailed=false. No Play/device or FPS/GC claim.
No source branch, user settings/Recovery or APK changes. Full migration remains
PARTIAL: player views, specialized sphere and remaining combat effects, source
assembly/coordinator/UI/data/compositor bridges. CI is verified on containing commit.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. Full enemy views,
Standard Sphere core/rings/attack, all rune graphics and both chest views are native.
Continue player views and remaining sphere/combat VFX, then remove source bridges.
Preserve original gameplay/quality; no APK; measurements after implementation.


### 2026-10-23 — complete active player view and Standard Sphere assembly

Entry HEADd764c2ebba3532fa0916a96c8121af353489ced2, CI #790 SUCCESS.
NativeWorldPlayer replaces drawPlayer and drawCharacterVfx. The native owner retains
the original Spherist dt/velocity history and death > hit > shield > move > idle
priority, authored front/side selection, movement mirror, original double pulse,
death fold/shear/opacity and eight fragments. It keeps getArtImage availability
(including first-call fallback), approved Sprite art and aspect. Native sprite
submission now accepts an optional parent matrix and explicit over mode; existing
callers retain their defaults. No replacement artwork or new animation behavior.

All original procedural player silhouettes (hunter, engineer, berserker, alchemist,
architect and existing default) retain their source paths/colors and 5px shadows;
the native player core shader retains its focal two-circle gradient, four stops
and 6px shadow. Native per-character scan, squares, chevron, motes and cross marks
retain placement and source alpha. fillRect-only accents preserve the current path.
Animated fallback rays/chevrons reuse pooled meshes, disposed only with the view.
No containment/orbit/status decorations were added to the player.

Standard Sphere assembly now also has a typed native owner: original visual target
predicate (living enemy strictly inside existing display range), breathing and
target/energy pulses, local anisotropic scale, core/rings, two original upper-crystal
Sprite draws (lower mirrored vertically), and target accent. Existing mutation,
elemental/modifier and specialized family VFX dispatch still needs migration. This
changes display ownership, not sphere targeting, damage, RNG or attack rules.

Actual Editor compilation: zero errors/warnings; scriptCompilationFailed=false.
NativeWorldPlayerCore is supported with empty diagnostics; Standard core and creature
glow shader diagnostics are also empty. No Play/device acceptance or FPS/GC claim.
No APK, protected-branch or user settings/Recovery changes. CI for this containing
commit is checked independently. Overall migration remains PARTIAL: specialized
sphere and remaining combat/network/character-world effects, source renderer/UI/data/
compositor bridges remain. Implementation-first direction remains in force.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT documents and modernization reports. Native active
player and enemy views, Standard Sphere assembly/core/rings/attack, runes/chests and
associated enemy indicators are implemented. Continue specialized sphere mutation/
element/modifier/family and remaining combat/network VFX, then remove source renderer,
UI/data/compositor bridges. Preserve behavior/quality; no APK; measurements later.


### 2026-10-23 — specialized sphere families, shared family art and modifiers

Entry HEAD2523b000577464103a77bf80bb40ecfa9fbac00d, CI #791 SUCCESS.
NativeWorldSphereFamilies replaces the complete specialized render entry points for
Sniper, Shotgun, Chain, Aura, Prism, Gravity, Pulse and Void. It retains source state
colors, disabled/resonance priority, pulses, rotation behavior, emitter/anchor/plate/
conductor geometry, depths, timing and over/additive ordering. Prism Spectrum color
uses the original branch/evolution lookup and element catalog; no new branch rules.

Shared visualHelpers core/drawSphereOrbit/glow/finishDisabled also route to native
owners, including calls from remaining Orbital code. Family core uses original focal
Standard-derived geometry/five stops with parameterized family color. All ten orbit
modes retain authored 43-point traces, lightning jitter, Void wobble/fragments, Prism
pattern and caps/joins. Cached glow preserves the original first-creation transform
per color, rather than silently changing its anchoring. Native materials/MPBs own
rendering; static art is retained and variable art reuses meshes. Performance is
unmeasured and dynamic tessellation/batching remains follow-up work.

All six modifier glyphs (multishot, pierce, ricochet, fire, freeze, poison), level
marks and original round stroke settings are now native. Existing authored levels
and sphereUsesProjectileModifiers remain the data authority. Disabled state freezes
motion while glyph alpha still follows its own original assignment; no extra fade
was invented. Native fields are excluded from saves/disposed; scoped interception
is restored in finally. Source gameplay/RNG/formulas/quality and protected branch
are unchanged; no APK. Editor compile: zero errors/warnings; family glow and updated
core shader diagnostics empty. Play/device/FPS/GC acceptance deferred. CI is checked
independently on the containing commit. Full migration remains PARTIAL: Orbital
specific body/attackers, elemental, mutation/projectile/lightning VFX and source
renderer/UI/data/compositor bridges remain.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. Eight family views,
shared core/orbits/glow/disabled arc and six modifier glyphs are native. Continue
Orbital-specific views, elemental and mutation/projectile/lightning effects, then
remove source bridges. Preserve behavior/quality; no APK; measurements later.


### 2026-10-23 — elemental sphere layers and Orbital body/attackers

Entry HEAD3355bef040870c8d26e21e5f4952b80c44adbd87, CI #792 SUCCESS.
Native elemental representation retains three elements/all glyph variants, continuous
front/back ring layers, highlight, six primary/secondary motes, weapon accents and
chain/aura/gravity/pulse signatures. Original compounded ring/glyph alpha, stroke
widths, the connected arcs inside poison clouds and chain signature's inherited
opacity are kept. Existing getSphereElementForBranch and element metadata remain
authoritative, including Prism Spectrum's original final-form override.

Orbital runtime and attacker entry points now use native C# ownership. Weak identity
animation memory retains original dt cap, same-time early return, aura-triggered
attack and resonance decay/freeze. Body retains two orbital traces, original core,
terminals and indicators. Attackers retain inner/outer satellite ordering, blade or
octagonal bodies, element overlays/trails and nearest living enemy display impact.
Original orbitalGeometry supplies counts/angles and source artifact/final-form gates
are retained; no hit detection, damage, RNG, spawn or progression changes. The source
trail call is sphere-local after restoring satellite translation; that placement is
preserved rather than inventing a relocation. Source alpha/blend/multi-contour and
current-path semantics remain. Pooled meshes are disposed only with their owner.

Actual Editor compilation: zero errors/warnings. Play/device/performance acceptance
remains deferred; no APK, protected branch or user settings/Recovery changes. CI is
verified separately on containing commit. Full migration remains PARTIAL: mutation
silhouette/final expansion/emitters/projectiles/lightning and other combat/network
layers plus source renderer/UI/data/compositor bridges remain.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. All nine specialized
family bodies, Orbital attackers, shared family art, modifier and elemental layers
are native. Continue mutation/projectile/lightning and remaining combat/network
views, then remove source bridges. Preserve gameplay/quality; no APK; measure later.


### 2026-10-23 — sphere projectiles, specialized chain lightning and mutation primitives

Entry HEADa5e6e7244bee70ad5ab256912ddf1829e4c6f0a2, CI #793 SUCCESS.
Native sphere projectile presentation retains velocity orientation, the original
tail contour/length/alpha, additive energy disc, Standard/Sniper/Shotgun/Prism/Void
branch-specific cores, fins/shards and final-form variations. Shotgun's source core
inherits tail stroke color/width; that behavior is preserved, not replaced by an
assumed default. Existing first matching evolution suffix determines visual final
index. Collision/targeting/damage/RNG/projectile simulation are unchanged.

Specialized Chain Lightning retains original source/branch guards and false return
for the existing native fallback bolt, two zigzag passes, per-branch piece count,
width/alpha/caps and web/storm/leech terminal accents. Existing source render stride
and lifetime decisions stay authoritative. No lightning gameplay change.

Shared mutation fin/shard/arc/tether/eye/glow/pulseRing helpers now submit retained or
pooled Unity meshes and native materials. Fins and tethers retessellate pooled meshes
for changing geometry; no unbounded time-varying geometry-key cache was added.
Mutation glow shader retains original four radial stops and the current runtime's
F3 alpha rounding, inherited opacity, shadows and over/add/screen pass indexing.
Scoped interception restores originals in finally; resources disposed with owner.

Actual Editor compilation zero errors/warnings; scriptCompilationFailed=false.
NativeWorldMutationGlow supported, empty shader diagnostics, three passes confirmed.
No Play/device/FPS/GC claim; no APK, protected branch or user settings/Recovery edits.
CI is checked separately on containing commit. Full migration remains PARTIAL:
mutation silhouette/final-expansion/emitter/transient branch coordinator and inline
paints, other combat/network/character-world effects and source renderer/UI/data/
compositor bridges remain. Native primitives do not mean that entire mutation layer
is complete.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT documents and modernization reports. All nine specialized
family views, Orbital attackers, modifiers/elements, sphere projectiles, specialized
chain lightning and shared mutation primitives are native. Continue mutation branch
silhouette/final-expansion/emitter/emit inline views and remaining combat/network
layers, then remove source coordinator/UI/data/compositor bridges. Preserve gameplay/
quality; no APK; measurements after implementation.


### 2026-10-23 — complete mutation coordinator and inline authored views

Entry HEADa2f888ffead89049949680acdb7b3e11ff006f22, CI #794 SUCCESS.
renderSphereMutationVfx now routes directly to a native typed view. Native layers
retain the original silhouette, final-expansion, persistent-emitter and attack emit
order for all source branches. Round versus inherited caps/joins and over versus
additive/inherited composition are explicit per authored layer. Unity Vector Graphics
owns contours and tessellation; pooled meshes own variable curves, rings and lobed
paths. Final-mode core/cascade/apex thresholds, counts, source extra direction turn,
original inherited fill alpha and F3 glow stops are retained. No new designer rules.
The exact attackPeak display expression and finite/.03 guard are unchanged; attack
simulation, delay/timer updates, damage/RNG/balance are untouched.

The remaining legacy helper adapter also now accepts CSS rgba via the existing CSS
color parser: source Pulse apex uses an rgba stroke color, which HTML-only parsing
could not retain. Main native view carries that same .95 color alpha directly.
No source TS/generated code, protected branch, user settings/Recovery or APK edits.
Editor compile zero errors/warnings. No Play/device/performance claim. CI is checked
separately on containing commit. Overall migration remains PARTIAL: remaining combat,
network/character-world overlays and source renderer/UI/data/compositor bridges.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. Full mutation coordinator,
all branch silhouettes/final modes/emitters/attack accents are native, alongside the
previous sphere/player/enemy views. Continue remaining combat/network/character-world
effects, then source coordinator/UI/data/compositor removal. Preserve behavior and
quality; no APK; measurements later.


### 2026-10-23 — shield and character world/target indicators

Entry HEADee6b45eb8c78db648f4fd365789730d7a56a2596, CI #795 SUCCESS.
NativeWorldCharacterIndicators owns active world/target indicator dispatch and all
Engineer/Architect/Hunter/Alchemist/Berserker views, plus player shield. Existing
getEngineerNetworkRange/getCharacterFormation and current target identity/timers
remain authoritative. Source alive selection, first-eight formation list, stable
ordering, square center from that full list, cluster distance and exact status-pair
symbol priority are retained. Architect polygons still stroke before fill. Original
colors, line widths/dashes/offsets, inherited composition/shadows and path continuation
remain; no new gameplay formation or target rule.

Shield retains original guards, Spherist asset gate, pulse, octagon/rings, recast wave,
charge motes and text. Native sprites accept additive composition without changing
old callers. Native world labels now parameterize original family/size/style and
composition/inherited shadow/alpha while retaining old defaults and shared Font atlas
stabilization. Hunter/Alchemist stay bold Georgia 10px; shield count stays bold sans
9px/additive/alpha one. Native label shader now has all three ordered composition
passes; glyph sampling is unchanged. No substitute font or added decoration.

Actual Editor compile zero errors/warnings. Label shader supported, diagnostics empty,
three passes confirmed. No Play/device/FPS/GC acceptance, APK, protected branch or user
settings/Recovery changes. CI is verified separately on containing commit. Overall
migration remains PARTIAL: network formations/links, inline combat feedback and
source renderer/UI/data/compositor bridges.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. Mutation layers, shield
and character indicators are native. Continue network/formation links and remaining
combat feedback, then source renderer/UI/data/compositor removal. Preserve gameplay/
quality; no APK; measurements later.


## 2026-10-23: Complete native sphere network views

NativeWorldNetwork / NativeWorldNetworkFormations own wavy connections (three stroke
layers and three animated packet pairs), seven formation bodies, charge and resonance
indicators and formation memory. External drone positions still come from the
existing buildRuntimeNetworkNodes; getSphereVisualNetwork still filters sphere
membership. Runtime adapters preserve strict active !== false behavior and existing
link color priority. No graph, range, charge, bonus or gameplay rule is replaced.

Original triangle first-three hull vs all-node center, inactive triangle fade,
per-node shadow/width state through charge/flash/ticks, square scaling, cluster
spoke alpha, raw inherited shadows and explicit zero-blur ring/fractal shadows are
preserved. Original drawFormationTag is unused, so no new labels were introduced.
Native owners are frame-pooled, disposed and excluded from run persistence. The
three source helper overrides are always restored in finally. Editor compile is
zero errors/warnings; Play/device/performance acceptance remains deferred. Full
modernization remains PARTIAL. CI success is tracked on containing commit.

Short handoff: Continue ECHO SPHERE unity-migration at containing HEAD (git log -1
--format=%H). Read three GPT docs and modernization reports. Mutations, shield,
character indicators and complete network views are native. Continue inline combat
feedback and source renderer/UI/data/compositor removal. Preserve gameplay/quality;
no APK, measurements later.
