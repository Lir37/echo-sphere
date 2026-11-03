# Enemy dynamics, bosses and sphere arrangement — 2026-11-04

Baseline unity-migration de65798a0596e4fe24f4cfa0e962952b4a6987dc; Unity6000.6.1f1.
Scope: approved attached brief (regional attacks/summoners, movement tuning, transient drag freeze). No device connection or device save changes permitted in this cycle. No manual targeting, quality reduction or new rewards.

## Stage: arrangement simulation gate
Actual native drag state now gates the authoritative StepGameplay before any phase or game-time advance. IsGameplayPaused/IsPresentationPaused expose that state without setting ordinary pause or Time Stop. Tap/joystick/Follow do not activate it. Preview and UI remain unscaled. Active ability/dash buttons are blocked while arranging. Desktop fallback input is processed before the simulation gate so PointerUp still works.
PointerCancel previously called the same commit routine as PointerUp; now it aborts. Lost capture, screen/end/pause/focus transitions and inactive/destroyed spheres clear transient state. One sphere/placement gesture owns arrangement at a time; an independent joystick cannot latch the freeze. Restore cancels input; transient gesture fields remain excluded from snapshot.

Actual isolated Play acceptance20 checks PASS:120 blocked StepGameplay calls, unchanged time/core/cooldowns/Time Stop, cancellation without removal, lost capture, new placement, multitouch+joystick, deactivation, ordinary pause, tutorial focus loss and menu/end transitions. This does not yet constitute physical Android gesture acceptance or full network-preview/commit interaction acceptance. Existing five real-API compile profiles and272Node tests pass. Core259533assertions PASS. Full task remains OPEN.

## Baseline combat experiment
Added Threat entry to existing real-C# combat arena tool.1080actual-spawn arenas:6regions, times0/900/1700 (explicit wave=stage/20+1),6seeds,5player field locations, StandardI/VII, up to20s at60Hz. Observed12roles; this is not a full-wave, all-role or survival trial. Raw baseline is in ignored Library/combat-balance/threat/before.json. Ranged cases/fired/died before firing: Basin108/91/17; fleet180/159/21; brood72/60/12; crystal72/66/6; storm36/35/1; null36/34/2. Censored enemies alive at20s are not deaths. Later source tuning needs matched rerun. No performance or subjective-fun claim.

Initial arrangement CI passed both typechecks; its preflight expected the old exact IsPresentationPaused expression and rejected the additional arrangement pause. Updated that assertion to explicitly require the new arrangement term, retaining the main-menu/ordinary-pause guards.

## Regional mechanisms and summoner bosses — implementation candidate

All gameplay remains in existing C# simulation/agents. Sphere nearest-target rules, formulas, RNG algorithm, UI framework and quality settings are unchanged. New decisions use existing spawn draws or deterministic entity IDs; there are no new targeting priorities. No device was accessed.

- Basin: original organic bodies preserved; roles now close faster and ranged preparation starts at a useful distance. Healing/disruption have readable ring feedback and sniper preparation shows the locked shot line. Original15roles retained.
- Rift: scout/phase hulls flank, others orbit/hold; all retain onboard weapons and the no-retreat/no-intentional-ram rule. A disruptor command hull prepares up to3nearby escorts for a visibly warned shared volley. Leader death cancels their pending volley. Existing support effects remain.
- Bastion: web corruptor was absent from EcologyRoles; it now replaces a duplicate swarmer slot in the brood pocket. Anchor is the brood-bearing heavy, with articulated plates and one child per prepared release, up to3living children. Existing splitters/bombers/healers/spores/web puddles remain.
- Gravity: ranged/sniper/tank-guard crystals close their facets during preparation (incoming damage x0.55), then open for1.2s after attack (x1.20). Applies consistently to direct, sphere, protocol and status damage. No invulnerability; automatic spheres use the same damage pipeline. Other crystal roles remain melee.
- Storm: ranged/anchor/disruptor energy beings can form a short connection to a nearby faction ally.0.8s warning, then a10px-wide line hazard for8damage;4.5s cadence, up to6warning/flash connections total. Death of an endpoint cancels a pending discharge. Failed partner searches retry after0.25s rather than scanning every frame.
- Null: phase/leech roles can mark a locked old player position for a shadow strike after0.8s, radius38px. Movement avoids it; the mark does not retarget. Existing leech, disruption and tentacle handlers remain. Existing phase flag is a speed/role flag; no second phasing system was invented.

Numbers are authored tuning for the explicitly approved scope, not historical Blueprint numbers. RegionalCombatTuning and RegionalEnemyRules own them.

### Movement and firing parameters (source pixels / seconds)

Spawn speed multipliers: ordinary roles up to1.30; fast roles up to1.12; heavy roles up to1.45. Gains taper at ordinary180 / fast220 / heavy140px/s; an already faster historical enemy is never slowed. First regional recovery before a first preparation:1.20 ->0.25s. Every projectile still has at least0.50s warning; sniper0.65s.

| Region | Regular / sniper range, before -> after | Intervals before -> after | Projectile speed before -> after |
|---|---|---|---|
| Basin |80/110 ->145/200 |3.2/2.2 ->2.7/1.9 |245/320 ->270/350 |
| Rift |80/105 ->150/205; scouts65 ->125; heavy90 ->165 |2.4/3.2 ->2.1/2.7; scout1.8 ->1.6 |250/350 ->280/370 |
| Bastion |75/105 ->130/175; web80 ->135 |spore4 ->3.2; web3 ->2.8 |spore190 ->220; web150 ->180 |
| Gravity |85/110 ->155/200; guard125 |2.6/3 ->2.3/2.7 |280 ->310 |
| Storm |70/100 ->145/190 |2.4 ->2.1 |310 ->340 |
| Null |80/105 ->145/195 |3 ->2.6 |230 ->260 |

Brood melee preparation0.65 ->0.50s; preparation starts within100px, tentacles within their actual reach plus12px. No spawn-rate increase or guaranteed hit was added.

### New bosses and route ownership

Modern CampaignBossRules catalog now has10families; the source-compatible BossCombatRules catalog preserves the original8. Added Brood Queen / Матка Роя and Fleet Carrier / Носитель. Their first attack is a warned bodily release, followed by existing pulse/fan (Queen) or fan/cross (Carrier). At50%HP phaseII latches: release3 ->5 insects or2 ->4drones, higher cadence through the existing phase director. Plates/bays open with joint movement, not body rescaling. Carrier holds at180px rather than deliberately contacting the core.

Modern regional event mapping replaces Bastion's summoner with Queen and Rift's null with Carrier. Original summoner/null remain in other regional routes; all8old factories were checked. Existing event times/count and1800s stabilization remain unchanged. Convergence escalation includes both new families in its existing repeating slots. No additional mandatory boss event was added. Existing region-appropriate wave support is biased during an active boss using the same spawn roll; no extra escort spawn timer.

Offspring are born at0.25of the parent's radius, inside its body, through the existing minion factory. Existing minion reward rules remain. Child speed135px/s; HP12%of parent max, bounded12..90. Per-parent cap10 /14in phaseII; ordinary brood cap3; global cap24living brood. Children use swarmer or fleet swarmer/ranged roles, never a reproducing role. They remain enemies after parent death and do not count as active bosses or delay the existing completion gate. Ordinary run/scene cleanup removes them. No arbitrary despawn timer was introduced.

Integer model identities, parent IDs, prepared aims, warning timers and boss phase/emission state are persisted by existing snapshot adapters; no renderer stores the state and no save key/schema version changes. Actual entity JSON roundtrip restored new boss controllers and offspring ownership. Existing archive/discovery/reward rules are reused, with RU/EN boss archive entries and labels.

### Elite distribution

All10variants remain reachable. Regional favored variants are those in the brief, weighted4versus1 for other variants; Endless/Convergence use2versus1. Null has3favored variants, other regions2. One existing random draw is reused. Existing elite effect handlers are unchanged. Geometry Shifter still drains Resonance; no new geometry manipulation was invented. Stasis Warden still deals its existing area damage, with no false promise of a slow. Whether Geometry Shifter should truly alter geometry remains an explicit design question.

## Checks and evidence

- Actual arrangement state21checks, routed attached-UIDocument move/preview/commit/cancel11checks.
- Actual regional/new-boss acceptance114checks: first projectiles/speeds, leader/endpoint cancellation, closed/open mitigation, locked dodgeable shadow point, bounded brood, two-phase summons, JSON boss+child restore, restored summon frozen by arrangement, parent death/remaining children, actual regional factory mapping and all8old factories. New boss fan/cross/pulse also executed in both phases.
- Existing enemy roster646checks PASS (actual spawn reaches15roles, all10elite handlers); old bosses486checks PASS (all24patterns in both phases and legacy isolation).
- Actual GPU sheet24states (six regional cases and two summoners, idle/preparation/recovery), inspected. This is synthetic native-renderer output, not Android or designer acceptance.
- Core265640assertions /47389source comparisons,272Node tests and five real-Unity-API compile profiles PASS. Actual Unity compile PASS.
- Live Editor hot reload of the old menu owner produced Composition-null errors during development. Restarted Play and repeated the new fixture; fresh Console groundTruth0errors/0warnings. Do not label the entire earlier session error-free; raw Editor log retained. The hot-reload lifecycle issue was not masked or claimed fixed.

## Balance evidence and limits

Final matched original-shooter cases, fired/total before -> after:
Basin91/108 ->105/108; Rift159/180 ->180/180; Bastion60/72 ->66/72;
Gravity66/72 ->71/72; Storm35/36 ->36/36; Null34/36 ->36/36.
Common cases that fired in both runs fired earlier by about1.4..3.0s. Full per-region damage, censored kill times, role-category/elite summaries and raw hashes are in UNITY_ENEMY_DYNAMICS_RESULTS_2026-11-04.json. Increased threat readability does not imply every single-target fixture deals more damage.

1080before +1080after isolated20s arenas.216additional pack arenas use six actual spawns, legal Standard/Chain/Aura II orVII triangle placement, actual Network,100HP core, stationary/scripted movement. These include no acquisition budget, abilities, terrain, scheduled waves or complete30minute player input. Times0/900/1700 select regional contexts; assigned waves1/46/86 are explicit stress parameters, not a claim that wave number is a fixed function of time (production waves advance after clear). Kill means exclude surviving/censored enemies. No real-device FPS, full-run win rate or subjective fun is inferred.

Pack-only cosmetic acceleration reuses the existing tool policy; its production-HP/position/RNG/timer equivalence was rerun for all10sphere families (10checks PASS). It never ships as reduced graphics quality.

Full30minute, larger build diversity, dense rendered/mobile stress, every cancel/input platform combination, long-run resource trends and designer/Android acceptance remain OPEN. Build/CI receipts follow. No phone operations are authorized by this brief.

## Final input edge acceptance
Native dash now respects arrangement pause before consuming cooldown. Native pointer release cancels after entering main menu/death or losing its captured sphere; it cannot remove a neighboring sphere. Actual state fixture23checks PASS and attached UIDocument routed-pointer fixture13checks PASS, including menu change before PointerUp. Actual Unity compile PASS. Main implementation CI4d112720: SUCCESS (run41725198945).
## Delivery receipt
Source commit8db541d045d1ebe1c149801bdba3543fc50f9633: CI SUCCESS, https://github.com/Lir37/echo-sphere/actions/runs/41727240265 (core-contracts, unity-typecheck, source-typecheck).
Actual local Unity Android BuildPipeline Succeeded, Release BuildOptions.None, IL2CPP ARMv7+ARM64, ECHO_REGION_TEST_ACCESS retained. APK: UnityProject/Builds/Android/ECHO-SPHERE-enemy-dynamics-regions-test.apk;57305378bytes; SHA25625f3292248f2ac4267654cf4709819e02d259cf8c0c4decc84c8cd55cb6e1982. This receipt-only commit does not change packaged source. APK was not installed; no device or device save accessed.

Remaining acceptance: complete30minute production progression, broader actual builds/abilities, dense rendered stress and long-run resource trends, physical Android gestures/performance and designer visual acceptance. Controlled arenas are evidence of implementation and comparative threat, not proof of final balance. Real geometry-changing behavior for Geometry Shifter remains undefined; existing Resonance drain is preserved. Menu hot-reload Composition lifecycle remains a separate unresolved Editor issue.

APK publication: at the user's explicit request, the exact verified57305378-byte APK is tracked directly at UnityProject/Builds/Android/ECHO-SPHERE-enemy-dynamics-regions-test.apk in unity-migration. Packaged source remains8db541d045d1ebe1c149801bdba3543fc50f9633; SHA256 unchanged. Temporary build caches and local recovery scenes are excluded from publication.
