# Point 4 — enemy roster, elites and boss combat

Implementation baseline: unity-migration @31f4ae60116f4e91972706e36b09eb6e7d3dba5f.
Unity6000.6.1f1 / CLI1.0.0-beta.15. Scope is point4 only; ability expansion remains removed.

## Implemented

15 existing enemy roles and10 existing elite variants are active, reachable and retained.
They did not need replacing. The missing work was boss attack/phase completeness.
New campaign runs now use a deterministic three-pattern cycle for all8boss families.
Each attack has a .8–.9second warning; no attack can skip its warning on a large step.
Target direction locks at warning start. Casting holds the boss in place.
At50%HP, phaseII latches permanently. An already warned attack retains its phaseI
parameters, so a last-moment HP crossing cannot add unannounced projectiles.
PhaseII shortens recovery20%, increases walking speed12%, adds two fan/radial
projectiles, one summoned minion or a second disabled sphere as applicable.
Projectile speed rises10%; pulse/disable/shift/null radii rise15%.
Summons stay on their actual60px ring. Dash is600/720px/s for .5s; cue reach300/360px.
Boss HP/contact/reward spawn formulas and scheduled spawn times are unchanged.

These are authored starting balance values for the approved completion scope,
not values claimed to exist in the original source. Campaign boss attacks replace
the old independent timers with one readable warning/recovery sequence.
In particular modern Aura uses an avoidable warned pulse rather than constant auraDPS.
Pre-campaign saves (_runEconomy==null) retain the entire old boss path and RNG order.
New scheduling/summon placement consumes no random numbers; hits still use existing
player damage resolution/RNG. Other existing systems retain their own RNG draws.

| Family | Attack1 | Attack2 | Attack3 |
|---|---|---|---|
| Void Lancer/shooter | aimed fan | radial volley | four-direction cross |
| Breacher/charger | locked-direction dash | radial volley | area pulse |
| Brood Mind/summoner | summon brood | aimed fan | radial volley |
| Aura Titan/aura | area pulse | radial volley | aimed fan |
| Conductor | aimed fan | nearby sphere disable | area pulse |
| Architect | radial volley | relocation/resonance drain | nearby sphere disable |
| Null | aimed fan | bounded null field/resonance drain/attack delay | area pulse |
| Stella Warden | aimed fan | summon guards | area pulse |

Standard recovery3.3s; summon recovery4.5s, phaseII multiplier.8.
Fan5/7shots, radial8/10, cross4; regular summon3/4, Warden2/3.
Volley damage22 (Null28), pulse1.2×spawn contact damage. Disable lasts1.6s,
using existing Stability duration adjustment; only spheres inside the cue radius
are eligible. Null field drains25Resonance and delays attacks.45s only when player
is inside the indicated area. Shift retains12Resonance drain.
Native world draws actual directions/counts/radii in source pixels, independent
of facing/body scale. PhaseII ring and RU/EN phase message; all8family labels now
resolve correctly for campaign bosses instead of four families reading Aura Titan.

## Existing enemy mechanisms verified

- grunt: baseline contact/movement; swarmer: faster small body; charger: faster contact attacker;
  tank_guard: HP/body/speed tank; anchor: slower heavy body. These are stat roles, not invented abilities.
- ranged/sniper: actual distinct projectile speed/damage/cadence; healer: heals nearby living allies;
  disruptor: disables a nearby active sphere; phase: existing phase flag and movement behavior;
  leech: short-range self-heal; splitter: two children on death; bomber: death explosion;
  scavenger: lowHP speed; corruptor: contact Resonance drain.
- linkbreaker: target warning then network break; resonance_leech: proximity drain;
  phantom_hunter: distant acceleration; geometry_shifter: periodic Resonance drain;
  splitter_prime: two stronger children; mirror_warden: mitigation and reflection;
  stasis_warden: existing area damage (not falsely claimed to slow movement);
  nullifier: nearby sphere disable; pyroclast: short-range area damage;
  scavenger_prime: lowHP acceleration.

No extra behaviors were invented to make the existing names sound more elaborate.

## Ownership and persistence

CampaignBossRules owns pattern definitions and deterministic state progression.
BossCampaignCombat executes through existing projectiles/minions/damage/network APIs.
CampaignBossState owns phase, current attack, warned phase, timer and locked aim.
Remaining dash duration/speed live in the active boss model. Existing boss auxiliary
snapshot automatically stores them; no save key/schema changes or renderer dependency.
One active path per run policy; ordinary legacy boss Tick is preserved.
Native geometry/material ownership remains in NativeWorldCreatures; fixed warning
meshes are retained in its existing cache and released by its owner.

## Verification and limits

- Actual Editor fixture check-unity-enemy-roster.cs:646PASS. 300 real region spawns
  reach all15roles; serialized identity/stats/movement, special role actions,
  all10elite effects and elite snapshot identities. Working handlers preserved.
- Actual Editor check-unity-campaign-bosses.cs:486PASS,all24attacks in both phases,
  warning/aim/dash auxiliary snapshots, moved-target aim lock, damage/projectiles/
  summon/network/shift side effects, outside-radius immunity, pause and legacy isolation.
- check-unity-campaign-boss-render.cs:6GPU sheets/48warning states. Thin warning
  rays/rings, phase ring and bodies rendered; this is a synthetic layout of actual
  renderer output, not an Android screenshot or a full battle performance sample.
- Core38999new boss checks /59193total assertions and47389source comparisons PASS.
  Project preflight355runtime C#files, five profiles (355runtime/6Editor) no compile
  errors/warnings, Node272PASS, source catalog305exact strings. Unity actual compile
  and Play PASS; current Console groundTruth0errors/0warnings (historical CLI log
  counters include prior tool/obsolete-API messages and are not hidden).
- Evidence is under GPT/VisualReferences/EnemyBossCompletion-2026-11-01/.

Implementation target15roles/10elites/8families×3attacks+phase reached.
Android acceptance, encounter fairness/full30minute balance and designer visual
acceptance remain OPEN. No phone FPS or GPU/performance improvement is claimed.
No APK built. User QualitySettings/recovery files are excluded from this change.
Point5regional content, run economy/meta/localization/audio and other remaining
plan sections are not closed by completing this implementation milestone.
