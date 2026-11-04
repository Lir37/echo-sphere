# ECHO SPHERE — archive icons, progression economy, geometry retention and hostile ammunition

2026-11-04. Branch unity-migration. Baseline cc4606149768324a291a71d67fb96d61c59ce38f. User authorized all four scopes in successive messages. Protected new-desing and recovery/.utmp files untouched.

## Archive art

NativeArchiveIllustrations supplies302 code-native vector emblems: two core concepts, four currencies, ten sphere families,30 abilities, their IV/VII mutation compositions,30 regional lore entries and ten runes. Sphere silhouettes reflect core/weapon/ring anatomy. Mutations retain the parent with a mechanic emblem, branch structure and distinct final appendages. Runes follow the production pickup silhouettes/colors. Region icons use six distinct regional motifs and chapter marks. The same icon component is reused inside discovered mutation detail pages. Unknown mutations still render only the lock, never future art/names/effects. Existing artifact illustrations and actual enemy/boss/character pictures are retained.

## Mastery and economy

Removed mastery purchase handler, price API, button/state fields and callback. Future mastery growth uses existing run XP only. Already earned mastery and historical paid-receipt recovery are preserved; no refunds/reset/save edits. Shards purpose text updated RU/EN. XP rates, mastery thresholds, character combat properties and shop upgrade effects/caps did not change.

Old total character unlock cost was approximately200 Shards; full shop cost17337700Credits. These two extremes did not form one coherent progression. New character tiers use existing character cost identities:24,72,72,144,256,448,704,960,1200Shards, total3880. First Hunter costs24 (four successful six-Shard campaign runs). Shop starts at existing base prices, then uses base*(level+1)^2*(level+4)/4, total885500Credits across54 levels. Old exponential late prices are replaced, not increased indiscriminately. Prior balances, unlocks, levels and receipts remain valid.

Run Credits formula was extracted unchanged into MetaCurrencyRules.RunCredits so the model and runtime share it. Boss/stabilization Shards income also unchanged. MetaProgression balance mode executes512seeds x6regions x4assumed enemy-lifetime/collection profiles x3run durations =36864 income comparisons. It verifies exact old/new income equivalence and evaluates the new spending curves. It is an economy throughput model, NOT thousands of played combat sessions, survival predictions, fun testing or Android measurements.

With60seconds between runs, full campaign win profiles imply complete shop times approximately328/218/166/145hours (slow/regular/fast/ceiling). Fastest sampled win gives129hours, about64.5days at2h/day. Optimistic Endless boss ceiling is24Shards/hour (every150s boss instantly killed, initial delay ignored), giving161.7hours to all characters,80.8days at2h/day. Ordinary successful campaign at12Shards/hour takes323.3hours; losses take longer. These are conditional fresh-account estimates, not a calendar gate or promise of player behavior. Existing wealthy saves naturally finish sooner. Future rewarded ads/economic content need recalibration; stubs currently pay nothing. Meta JSON records all assumptions and prices.

## Formation regression

Confirmed failure: runtime passed disabled spheres as dead; analysis dropped their candidates and TrySet cleared manual preference when a candidate was unavailable. New nodes could also replace the selected members/type through score ranking.

NativeNetworkAnalysisCache now distinguishes physical membership from temporary availability. It retains primary/secondary shapes and exact node identities across new-node additions and disable/recovery. Disabled shapes keep their slot but give no bonus; disabled links remain absent. A disabled timer countdown does not rebuild the same geometry. Genuine relative movement permits a fresh structural selection. Following translation and cache recreation after restoration preserve membership. Manual role selection and explicit AUTO reset remain supported; secondary AUTO cannot replace the main slot. Existing integer identity helper uses Unity EntityId. No formation damage/efficiency multipliers changed.

## Enemy ammunition

Production enemy bullets route to retained NativeRegionalProjectileArt using region/attack style. Region0 organic barbs/needles; fleet rocket bodies/fins and moving laser packets; brood slime/web darts and original stationary pools/webs; crystal shards with orbiting splinters; storm zigzag/plasma fragments; archive hollow violet glyphs/traces. Role/sniper and web variants differ. These are enemy-only meshes/palette; allied projectile paths stay unchanged. Moving laser packets are not new hitscan attacks. Speed, lifetime, collision radius, damage, firing schedule, status behavior and existing area effects are unchanged. Region is retained with projectile data for owner death/restoration. Geometry and property blocks are pooled; no extra per-piece blur passes.

## Evidence and remaining acceptance

- Actual Unity6000.6.1f1 compile PASS. Five type-check profiles against installed Unity DLLs: zero errors/warnings,388runtime files.
- Core281432 assertions PASS; preflight/provenance PASS. Static preflight updated to the approved new price curve and extracted equivalent reward formula.
- Actual native-cache14 regression checks PASS: original triangle/line, disable/recovery, exact members, addition, following, manual swap/AUTO, new cache/restored IDs and real movement.
- Actual Editor commerce12checks and RU/EN UI105checks PASS: no mastery purchase handler/button, existing paid progress recovery, character/shop purchases and persistent balances.
- GPU1232checks PASS for302 distinct icon images and24 projectile sample variants. Readbacks exist only in the fixture. Sheets/JSON in VisualReferences/ArchiveEconomyFormation-2026-11-04.
- Long combat, all mutations/regions in motion, real economy retention/feel and Android performance are not established by these bounded checks. Fresh APK install is recorded after successful build; previous phone APK was source620c53ad.

Continue from the commit containing this report: four requested scopes implemented; finish Android delivery/acceptance, then respond to actual player observations. No new mechanics or gameplay content beyond the user-authorized economy retuning.
