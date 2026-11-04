# ECHO SPHERE — menu, archive and rewards rework

Date: 2026-11-04. Branch: unity-migration. Baseline: 8bd27218a42c416cbd8f09de4bc2e975ea2607f0.

## Implemented

- Home: removed leaderboard navigation, central character/difficulty controls and redundant center labels. The animated sphere opens run selection. Character selection remains in footer; Continue remains accessible when a saved run exists. Campaign uses existing normal baseline; regional challenges remain available.
- Bestiary: 178 reachable authored identities, including 48 region/boss pairs, all regional elite profiles and actual regional enemy anatomy/roles. Unreachable boss-region combinations are excluded. Basin entries represent nine anatomy variants; random role combinations are not duplicate species.
- Unlocks now require the central death event. Encounter-only historical records are not inferred as kills. First defeated appearance is persisted with region/role/variant/color/elite/boss metadata. Character entries open after a non-tutorial completed run with positive elapsed time. Existing progress is retained; no retroactive invented unlocks.
- Enemy portraits use production creature geometry, baked lazily one per frame into owned cached textures. Open entries use the actual first defeated specimen. Character archive uses existing character portraits. Detail pages show behavior, individual RU/EN lore and regional histories. Added authored fiction for six regions, enemy families, elites, bosses and ten characters; editorial acceptance remains the user's.
- All 90 artifact pictures have explicit semantic motif pairs, distinct compositions and color palettes. The same vector artwork is used by existing archive/shop/reward/inventory icon consumers. No gameplay RNG is consumed by art.
- Merchant: compact shop icon and existing countdown inside upper-right HUD. Removed old separate status label; physical merchant contact unchanged.
- Combo placed below both HUD panes, including 125% control scale. Existing XP multipliers at 10/25/50/100 remain 1.5/2/3/4, with the original three-second window.
- Results: bounded scrolling layout, colored currencies and actual character mastery XP, readable run time. Double rewards is an explicitly disabled coming-soon ad placeholder.
- Reroll: one free per entire run, as explicitly confirmed by user. After consumption, disabled ad placeholder is shown. No ad SDK, simulated reward, additional reroll or RNG consumption was added.

## Verification

- Actual Unity 6000.6.1f1 Editor connected through Pipeline 7800, CLI beta.15 retained. Play Mode fixtures: 115 runtime behavior checks; 628 GPU art checks (178 portraits, 90 distinct artifact pictures, cache reuse/invalidation); 266 layout checks across ten RU/EN layouts; 377 existing UI/gameplay checks and 26 existing balance checks PASS.
- Core contracts: 281328 assertions PASS. Node: 272 tests PASS. Project preflight and source visual provenance PASS.
- Five compiler profiles against actual installed Unity API references: zero errors/warnings; 386 runtime source files. This compilation evidence is separate from executed Editor checks.
- Actual Game-view home screenshot passes dark-footer check. Offscreen UI Toolkit target captures of native filtered home/HUD can show white surfaces; those captures are not evidence of a production rendering fix or regression. Archive/results examples and actual home image retained in VisualReferences/MenuArchive-2026-11-04.
- Final live Console ground truth: compilationFailed=false, zero current errors/warnings. Historical Pipeline retained buffer includes earlier failed fixture attempts; it is not asserted empty.
- This cycle did not build/install an APK or validate on Android. Phone rendering, performance, final art/lore acceptance and full long-run regression remain open. No 60 FPS claim.
- Only two project chats were available among latest 50 tasks; reviewed available context, not five invented chats. Protected new-desing, user recovery files and unrelated .utmp changes were not modified/staged by this task.
- CI result and exact published commit are reported after push; a green contract workflow is not Android acceptance.

## Continue

Use the commit containing this report on unity-migration. Menu/archive/reward UI is implemented and Editor checked. Next: build current APK when requested; verify kill/character unlock persistence, mobile archive scrolling, merchant HUD and result/reroll placeholders on Xiaomi 11T. Ads remain stubs; one free reroll per run.

## Artifact build detail follow-up

User-requested artifact tap now opens a bounded scroll detail page for discovered artifacts. Lists every matching existing full set and artifact combination, their existing localized effects and all required artifacts with shared pictures. Selected artifact is highlighted. Artifacts with no special set explicitly say so. Full sets expand their existing synergy prerequisites through the same runtime resolver; the eight campaign sets use CampaignArtifactSets. No new combinations, bonuses, formulas or unlock rules added. Archive discovery is not confused with current-run ownership.

Actual Unity compilation and Play Mode: 1152 targeted checks PASS, covering all 90 artifacts in RU/EN, exact existing campaign and full-set memberships, pair requirements, actual click handling and horizontal text bounds. Current Console ground truth had zero errors/warnings. RU/EN detail images and receipt archived alongside previous evidence. Android acceptance remains open; APK not built in this follow-up. CI is checked after publishing the commit.

Continuation: use the commit containing this follow-up. Artifact detail now shows builds/effects/required items. Next is mobile acceptance with the rest of the menu/archive changes; one free reroll per run and ad stubs remain unchanged.

## Sphere and ability progression follow-up

Discovered sphere and ability cards now open the same bounded detail page. IV branches and VII finals are grouped by their actual parent using NativeRunCatalog, the same definitions used by the upgrade picker. Only choices recorded in existing knowledge keys expose names/descriptions. Unselected branches have generic locked text and no final contents; selected parents show locked placeholders for unselected finals. Abilities without mutation branches say so.

Both sides list existing SphereAbilityLinks and discovered partners. Synergy effects/names unlock only when the required sphere branch and actual ability final have been chosen and the partner is discovered; unknown mutation names remain masked in requirements. Historical link final aliases resolve to the actual choice ID using the existing runtime final-slot rule. No new synergies, combat rules or content definitions. Accepted ChooseEvolution now collects existing knowledge immediately, preserving the same batched persistence/reward mechanism and keys.

Actual Unity compile PASS; Play Mode2133 targeted checks PASS covering every sphere/ability branch and final individually, hidden sibling choices, reciprocal existing synergies, accepted choice discovery and RU/EN attached click/layout. Existing115 archive behavior checks and1152 artifact build checks also pass. Screens/receipt stored in VisualReferences/MenuArchive-2026-11-04. Android and full visual acceptance remain open, no APK built here. CI result reported after push.

Continuation: use the commit containing this follow-up. Sphere/ability archive pages now show previously chosen mutations and gated reciprocal synergies. Next: verify archive discovery across runs/relaunch and scrolling on Android with the prior menu/artifact updates. Ads remain stubs, free reroll one per run.

## Lore correction — environment

User clarified the game is not set in an aerial environment. Removed the invented reference to air from Glimmerwing lore in RU/EN; its wings now sense the existing field directly. Reviewed current ArchiveLore and CampaignRegionLore for explicit air, breathing, atmospheric and wind dependencies; no other occurrences found. No claim that the entire setting is literal vacuum is introduced. Existing dated screenshots preserve earlier verification evidence and may contain the old sentence. No gameplay or visuals changed. Existing core checks and CI are verified for this text correction.

## Android delivery — 2026-11-04

Built locally in Unity6000.6.1f1 from source commit620c53ad2d09a950b422c20231eafe4b76e6c15f (already on GitHub, contract CI SUCCESS41836831590). BuildResult Succeeded,335.6seconds. Release IL2CPP, ARM64 and ARMv7 as configured; ECHO_REGION_TEST_ACCESS remains enabled only through the test build's extra scripting define. APK: UnityProject/Builds/Android/ECHO-SPHERE-620c53ad-regions-test.apk;817250336bytes; SHA256 ef0d54032055fff2a684e454ef58cd412c0bd2c38dc0e2b0130724af150e4347.

ADB install -r returned Success on Xiaomi11T. Installed base.apk SHA256 matches the local artifact exactly; package com.lir37.echosphere, version0.1.0/code1. No uninstall, data clear, save replacement or unlock-save edits. Explicit launcher activity started successfully and the new home menu was observed on the phone. This is installation/startup verification, not full mobile archive or combat acceptance. Startup logs include Unity's nonfatal missing optional AssetPackManager class message; no fatal crash observed in this bounded launch. The Editor Pipeline request timed out at30seconds while synchronous BuildPlayer continued; the completed BuildReport/receipt and actual installed APK, not that request response, establish build success.

Documentation commit after this delivery does not alter APK source content. Continue from the current branch HEAD; current phone APK contains source620c53ad. Next: user acceptance of new archive/discovery pages and run UI. No performance or full-game acceptance claimed by this delivery.
