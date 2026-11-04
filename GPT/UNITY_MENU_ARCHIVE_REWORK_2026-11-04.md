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
