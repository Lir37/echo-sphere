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

1. Replace character selection and archive with typed native Unity screens.
2. Move remaining upgrade/artifact/detail/result/tutorial/mobile controls out of TSX.
3. Replace pause outer shell and retained-prefix fallback with the native compositor.
4. Replace generated gameplay Canvas execution with native entity views and Unity
   graphics/VFX, retaining every authored effect and the authoritative simulation.
5. Then undertake the combined visual/input/performance/device acceptance pass.

No gameplay/balance/RNG changes, no new mechanics, no protected branch edits, no APK.
ProjectSettings.asset and _Recovery remain user-owned and outside this change.

Short handoff: continue unity-migration at the containing HEAD. Native Home/map/shop/
records/achievements/Settings routes are selected; native HUD/four-tab pause now
default on. Complete characters/archive, overlays and gameplay views, then native
shell/compositor. Implementation first; defer profiling/standalone fixtures. Preserve
gameplay, visuals, new-desing and user changes; no APK.
