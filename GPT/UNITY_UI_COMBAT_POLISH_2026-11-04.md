# UI and combat presentation polish — 2026-11-04

Branch: unity-migration. Baseline: 575c2f1e5750d518841c1c0e8ecb1c9ff44a39d2.
Unity Editor 6000.6.1f1; Unity CLI beta.15 retained by explicit user decision. No CLI upgrade, ADB, device installation or phone save changes in this cycle.

## Implementation

1. Drones: ten cached family-specific mechanical hulls and mirrored moving side pieces. Existing sphere family colors; visual attack pulse originates from a real attack, not an idle repeating fire animation. Targeting/damage unchanged.
2. Shields: segmented thin membrane, charge indicators and impact/recast ring; passive charge-only shields are also visible. Gameplay shielding unchanged.
3. Lightning: pooled hot core, colored halo and connected secondary strikes; shared by ability and Chain presentation, with branch styles. No extra per-bolt blur passes.
4. Text: local wrapping mutation names, separate lock-button row, constrained vertical scrolling and explicit merchant price widths. A device smoke test found that global percentage-width/ellipsis rules clipped intrinsic menu labels; those rules were removed, preserving the existing shared label sizing. This is not exhaustive visual acceptance of every screen.
5. Archive: record enemy/boss encounter at all three actual registration paths, before an enemy can die between periodic scans. Discovery persistence also consumes event-recorded pending rewards. Correct section-specific locked copy and regional lore conditions. Existing IDs/reward rules retained.
6. Boss projectiles: already-fired bullets tick once in the independent phase while their boss is frozen. Global pause/time-stop rules retained; death cleanup unchanged.
7. Controls: 100% now maps to the former 75% control dimensions, including placement and joystick feedback; both handedness modes. Fresh preference default is 100%; existing saved preferences are not overwritten. HUD scale is not recalibrated by this change.
8–9. Icons: code-native vector motifs for all 90 catalog artifacts, used in rewards, pause inventory, merchant and archive. Related artifacts share a visual family with ID-specific engraved details; these are not 90 individually painted illustrations. Archive categories and unknown entries receive semantic/lock icons.
10. Mutation selection: safe-area-aware vertical ScrollView, no horizontal scrollbar, retained choices/reroll/locks. All three level-seven finals for every branch are checked in RU and EN at 280/360/640 logical width.
11. Merchant: neutral world presentation with six region silhouettes and moving inventory pieces. It is absent from enemy targeting and has no damageable EnemyAgent. Appears at the existing schedule, stays at a saved position, opens only within contact distance; timed remote auto-open removed. Compact four-column eight-offer stock, selected-item details and one buy action. Compact next-visit indicator moved to top-center; no lower-right remote open button. Existing prices, offers, rarity, cadence and purchase rules retained.

Merchant reference research: compact grid/selected-item presentation from public Magic Survival references, not an exact reproduction of a verified official shop screen:
https://w.atwiki.jp/magicsurvival/pages/172.html
https://w.atwiki.jp/magicsurvival/pages/275.html

## Verification

- Real Editor compile: PASS; actual Play Mode used.
- Actual runtime fixture: 377 checks PASS. Four frozen boss families; bullet single-step/global pause; discovery IDs for sphere branches/finals/abilities/artifacts; merchant position persistence/economy; 75/100 control scale and handedness. Isolated model fixture does not write PlayerPrefs.
- Attached UI Toolkit fixture: 184 layouts (180 mutation cases plus four merchant layouts), RU/EN, three widths; actual rendering to RenderTexture. Receipt and screenshots in Library/ui-polish; selected images archived alongside this report.
- Actual GPU sheet: ten drones, six merchant silhouettes, three shield states and three lightning styles; nonempty render checked and image inspected.
- Core checks: 265640 assertions and 47389 TypeScript comparisons PASS.
- Node: 272/272 PASS. Unity preflight and visual provenance PASS.
- Actual Unity API type compilation: five profiles PASS, zero errors/warnings (382 runtime files, six Editor files). The incomplete temporary Linux reference bundle failed its initialization-guard probe; using the installed Editor's actual Data directory passed. This was a test-reference setup issue, not a production-code fix.
- The first asynchronous UI fixture was interrupted during Play shutdown and reported a destroyed RenderTexture in its cleanup. Fresh complete fixture runs passed; this failure is not hidden as a production fix.

## Remaining acceptance

Physical Android visual/performance acceptance remains open, including safe areas, gestures, shop encounter/contact purchase, shields/lightning readability in motion, all menu/HUD/translated text combinations, and repeated region/pause transitions. No claim of device FPS or full all-screen text acceptance. Existing published APK predates this change; it must not be used as evidence for this source version. No new APK requested/built in this cycle.

CI and final commit are recorded in the cycle's final response. Source and report commit together; new-desing remains unchanged. Build/recovery leftovers are left untouched.

Continuation: ECHO SPHERE, unity-migration, use the HEAD containing this report. UI/combat polish implemented; read this report and the three mandatory GPT documents. Keep CLI beta.15 and Editor 6000.6.1f1. Next: Android acceptance of the new merchant, controls/mutation scrolling, visual effects and remaining text screens; build a fresh APK when requested. Do not use the phone without renewed authorization.
## Device follow-up
The user connected Xiaomi 11T by USB and authorized installation/testing. First release build from 0c7bada0 installed successfully with data retained, revealing global text-rule truncation on the real main menu. The correction removes that stylesheet and restores shared label sizing; mutation/shop constraints remain local. A fresh corrected APK replaces the intermediate build. The existing saved run at 20:02 must be preserved. Final device evidence is recorded below when verification completes.
