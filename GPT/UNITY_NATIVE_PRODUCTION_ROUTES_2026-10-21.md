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
