# Vampirism and enemy time scaling — 2026-11-04

Branch unity-migration; baseline b9e3e4ae60e74434292a1ccf5e5b1a3b59807de7.
Authorized scope: user reports near-immortality with Vampire Ring and requests stronger enemy HP/damage over time. No new content, attacks, speed, RNG or reward changes.

## Confirmed causes and correction

- Active campaign ring provided 3% of sphere hit damage PLUS 5 HP per direct kill. The hit damage used for healing included damage beyond the target's remaining HP. A 1000-damage hit killing a 10-HP enemy could restore 35 HP.
- Campaign Vampire Ring now grants 1% lifesteal, with no flat kill heal. The lifesteal input is capped to the target's positive HP immediately before the actual sphere hit. That same example now restores 0.1 HP. Repeat damage to a dead target grants nothing. Healing still respects player MaxHp.
- The Vampire ability retains its 3% per level. It also uses actual HP removed instead of excess damage; mutation healing and separate artifact-set healing are outside this nerf.
- RU/EN reward/merchant/archive descriptions use the existing active ArtifactGameplayRules override. Historical catalog/source values remain readable for legacy/reference compatibility; the active campaign reads Effect(...,modern:true)=0.01. No duplicate editable balance catalog introduced.

## Time progression

Existing wave, difficulty, region/pocket, role and elite multipliers remain. New campaign enemies additionally use:

| Minutes | HP multiplier | Damage multiplier |
|---:|---:|---:|
| 0 | 1.00 | 1.00 |
| 5 | 1.10 | 1.075 |
| 15 | 1.30 | 1.225 |
| 30 | 1.60 | 1.45 |
| 60 | 2.20 | 1.90 |

Linear continuous clock: HP = 1 + 0.02 × minutes; damage = 1 + 0.015 × minutes. Multipliers compound the existing wave stats; enemy speed is unchanged. Clock only runs with the existing simulation run time, so pause cannot increase difficulty. Tutorial and pre-campaign reference paths stay at 1.

HP and contact damage are assigned at spawn, without refilling existing wounded enemies or rewriting saved enemy stats. Boss HP/contact, fixed regional/boss projectile attacks, elite periodic damage and regional energy links receive the clock factor. Summon HP derives from scaled parent HP with scaled min/max caps, or explicit boss summon base HP, without applying the factor twice; summon contact damage is scaled too. Already-fired projectiles retain their launch damage. No save format changes.

## Evidence

- Real Unity 6000.6.1f1 compile and Play fixture: 26 checks PASS. Actual nonlethal/overkill/dead-target/non-sphere-kill healing, MaxHp clamp, ability + ring composition, same-wave ordinary spawns in all six regions, boss HP/contact/projectile damage and ship projectile damage, tutorial/legacy exclusions. Isolated runtime objects; no PlayerPrefs/device save mutation.
- Core: 280054 assertions PASS, including historical source parity and new rate/clock guards. Node: 272/272 PASS. Unity preflight PASS.
- Sensitivity sweep: 40320 deterministic scenarios, six regions, four difficulties, three base archetypes, five independent wave values, seven time points, four kill rates, four excess-damage factors. Isolated ring healing falls 70.34–98.74% for equal HP/kill throughput over the tested grid. Separate samples include enemy HP/damage clock growth.
- This sweep uses actual stat formulas and assumes kill throughput/overkill. It does not simulate player controls, enemy AI, shield uptime, incoming hit frequency, build victory probability or enjoyment. It is not evidence of full combat balance or Android FPS.
- Reproduce: dotnet run --project tools/EchoSphere.BalanceSim --configuration Release -- survival UnityProject/Library/survival-sensitivity.json; Editor run_script scripts/check-unity-survival-balance.cs, entry SurvivalBalanceCheck.Main.

## Acceptance and continuation

Phone was no longer detected over USB at the end of this cycle; no APK installed with these balance changes. Device solo/late-run acceptance remains open. Previous installed APK still contains the older ring and enemy scaling until rebuilt/updated.

Next: fresh APK and on-device comparison of Vampire Ring versus no ring at early/mid/late run times; preserve user progress. Do not compensate by globally inflating enemy damage without evidence. Read this report plus the three mandatory GPT documents, keep Editor6000.6.1f1 and CLI beta.15. CI and final HEAD are recorded in the final cycle response.