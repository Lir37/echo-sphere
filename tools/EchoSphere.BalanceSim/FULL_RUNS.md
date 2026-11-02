# Actual Unity campaign runs

These probes use production `EchoSphereRuntime.StepGameplay`: the same ordered
phases called by `Update`, not a second combat implementation. Waves, regional
enemies and bosses, experience, runes, drops, offered upgrades/evolutions,
formation movement, placement and merchant budgets execute their real handlers.
The economy sensitivity model and fixed-target arenas remain separate tools.

Use Unity 6000.6.1f1, the current Unity CLI and Play Mode with the live runtime
at the main menu. Do not operate the live game while probes are running. No APK
or phone is required; these are not Android performance or visual acceptance.

First run each entry from `scripts/simulate-unity-full-runs.cs` using
`unity command run_script --file <absolute-path> --entry FullRunSimulation.<entry>
--timeout_ms 600000 --detach --json`, and poll `unity job status <id> --json`.
Check **nested `data.result.success`**, including script exceptions.

* `IsolationProbe`: typed sandbox preferences, nested scope/exception cleanup,
  unchanged real active-run preference, deferred and duplicate object destruction.
* `FrameEquivalence`: actual `Update` versus the shared step on three seeds,
  idle input, up to 2,048 frames each or player death. Compares per-frame combat,
  progression, wave timers, RNG, sphere cadence and active agent state. Camera
  effects, autosave buffers and presentation are excluded from the projection.
* `FastEquivalence`: collector profile with ordinary particles versus cosmetic
  retirement after their real first tick, up to 300 virtual seconds or death.
  Compares complete results except wall-clock duration and acceleration flag.
  This is a harness optimization, never a player graphics setting.
* `SnapshotGuardProbe`: production snapshot capture excludes the temporary
  Triangle reentrancy guard; the restoration filter ignores historical copies.
* `BossScalingProbe`: nine actual campaign boss creations, holding wave fixed
  across three run times and holding time fixed across waves 2/20/40. Records
  HP, contact damage and speed to distinguish clock rewards from wave scaling.
* `Main`: executes `UnityProject/Library/full-run-balance/request.json`; writes
  `last-run.json` and checkpoints `progress.json`. Input has `region` (0–5),
  `character` (0–9), `family` (0–9), `seed`, `seconds` (up to 2100), `policy`
  (`anchor`, `collector`, `kite`, `tactical`) and `fastCosmetics`.

All requests use normal difficulty, mastery I and an empty meta-shop. Regional
clear flags are isolated access fixtures, not rewards granted to the player.
Preferences are scoped for both reads and writes, including death/clear receipts.
Scopes restore even after exceptions. Production preference and destruction
forwarders call the ordinary Unity APIs outside these Editor-only scopes.
Each request owns a separate scene and runtime; no live game model is borrowed.
Audio enablement and previous active scene are restored during cleanup.

Controller policies are deliberately limited:

* `anchor`: remains near the initial formation and seeks nearby pickups.
* `collector`: approaches visible enemies/pickups, repels close threats more
  strongly when wounded.
* `kite`: broad rotating movement with enemy repulsion and opportunistic pickups.
* `tactical`: selects among 24 directions and standing still using visible
  pickups/enemies and nearby boundaries; holds direction for 100 ms.

All use the same greedy offered-choice scoring, real cooldown-limited casts,
legal capacity/unlock/copy/path-constrained placement and earned merchant funds.
They never teleport, grant levels/items, inspect future RNG or bypass attacks.
They do not dodge projectiles intelligently, rearrange a complete formation,
use rerolls or optimize artifact special effects. Outcomes describe these bots,
not human win rates, optimal play or subjective enjoyment. Requested preferred
family is a choice priority, not a guaranteed or forced build.

Sequential orchestration (do not run multiple orchestrators against one Editor):

```
python scripts/run-unity-full-runs.py --cli <unity-cli.exe>
python scripts/run-unity-full-runs.py --cli <unity-cli.exe> --policies tactical --output UnityProject/Library/full-run-balance/tactical
python scripts/run-unity-full-runs.py --cli <unity-cli.exe> --characters --output UnityProject/Library/full-run-balance/characters
```

The first command performs 54 runs (six regions, three seeds, three policies).
The second adds 18 tactical runs. The third performs 60 matched character runs:
same seed/policy/preferred family within each region. Existing request-matching
completed files resume a series; select a **new output directory** after changing
gameplay or bot behavior. Two character requests also occur in the tactical
series; keep these as explicit replays rather than inflating unique coverage.

The cap is 1,800 **simulation** seconds or death/stabilization, excluding time
spent in upgrade/shop dialogs. A `time_limit` is not a win. Fixed 1/60 steps use
the same float accumulation as the game; 1,800 can end at 1800.004. Death settles
the real run reward, skipping only the non-simulating cosmetic death animation.
Autosave I/O, focus/UI transitions and human reaction time are not simulated.

Aggregate completed JSON with `scripts/analyze-unity-full-runs.py <directories>
--output <report.json>`. It validates outcomes/budgets, preserves histories and
samples, lists acquired content and hashes original raw outputs. Never merge
calibration runs with the accepted series or label the finite matrix exhaustive.
