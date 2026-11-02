# Production combat probes (Unity Editor)

`Program.cs` in this directory remains an **economy sensitivity model**. Its
assumed lifetimes are not used to judge combat balance.

Actual combat probes are in `scripts/simulate-unity-combat-balance.cs`. Run in
Play Mode with Unity CLI `command run_script --file <absolute-script-path>
--entry CombatBalanceSimulation.<entry> --timeout_ms 600000 --detach --json`.
Poll the returned job ID; inspect nested `result.success`, not just the CLI
envelope. Output is under `UnityProject/Library/combat-balance`.

Entries:

* `Main`: `mode.txt` = `full` gives 220 sphere states (including placeable level
  zero) × one/eight targets × free/anchored targets × three seeds = 2,640
  arenas, 20 seconds at 60 Hz. `smoke` tests level I only. `changed` reruns
  Shotgun/Pulse (528 arenas with level zero, historically 504 without it) and
  merges unaffected states from `before/arenas.json`; preserve that baseline
  explicitly before using this mode. Do not count the merged rows as new trials.
  `unlevelled` adds 120 level-zero trials to the existing nonzero arena rows.
* `Stats`: every singleton and unordered artifact pair, plus 1,024 distinct
  seeded subsets of 3–23 artifacts, for all 220 sphere states: 1,126,400
  production stat evaluations. This score excludes crit, procs and geometry.
* `Effects`: every active ability state individually in each region: 1,008
  activations and first two seconds. Ordinary sphere attack phases are omitted;
  a disabled network alone does not disable a sphere's weapon. This is not
  sustained DPS; buff/cooldown expiration is not advanced.
* `AbilityLoadouts`: all 43,164 unordered selections of one/two/three distinct
  active abilities at their nine final variants. One canonical activation
  order, one assigned region and fixed supporting spheres. Immediate burst and
  activation only; not all cast orders or all sphere/ability contexts.
  `ability-start.txt` selects batches of 4,096 (0,4096,...,40960).
* `Regions`: 156 arenas: six policies × thirteen roles × stationary/slow circular
  player movement (25 source pixels/s). A role-policy probe does not establish
  that the role actually spawns in every region. No waves or boss controllers.
* `Wounded`: 540 final-state arenas, at 10% target HP. Training targets carry a
  boss tag to exercise damage/execute modifiers without ordinary instant-kill
  reward hooks. They do not run actual boss AI.
* `ArtifactCombat`: `artifact-start.txt` selects a batch of up to 512 builds.
  Starts 0,512,...,4096 cover the empty build, 90 singles, 4,005 pairs and the
  first 128 larger subsets. Each has **one** assigned final sphere state,
  region and seed, eight targets and 20 seconds. This is not every pair in
  every context. `Stats` must run first to create `artifact-builds.json`.
  Each batch first checks `FastVisualIsolation`. Subsequent headless trials
  retire cosmetic particles after their first tick; production creation and RNG
  still run. Exact HP, position, RNG and attack-timer comparison covers all ten
  families in a stress fixture. This acceleration affects only the simulation
  harness; it is not a game quality setting or a performance measurement.
* `HailProbe`: regression check: source target excluded from the immediate
  campaign fragment hit, another target remains hittable; legacy preserved.
* `TuningIsolation`: actual campaign/legacy damage ratios for 220 states.
* `TriangleProbe`: actual damage-pipeline regression for bounded campaign
  Triangle secondary pulses, restored next-primary cadence, and unchanged
  sparse legacy behavior. Do not run a dense legacy cascade: its historical
  recursive path is retained for compatibility and can exhaust the stack.
* `SpherePairs`: all 3,735 compatible unordered final sphere pairs, including
  identical copies (a family cannot choose two conflicting mutation branches).
  Level-20 fixture, legal fixed positions, eight free targets, network/Resonance
  phases, first five seconds. `sphere-pair-start.txt` selects batches of 512.
* `Ensembles`: 128 seeded level-60 fixtures of 4–8 distinct final spheres plus
  3–12 artifacts, each on three seeds (384 arenas), twelve targets, ten seconds,
  network/Resonance phases. Existing persistent `spheres` upgrade III provides
  capacity, with all other meta upgrades empty. Open-arc positions and sphere
  pairs are validated by the actual placement/path/capacity rules. No active
  casts or acquisition-budget simulation. `ensemble-start.txt` selects 32-build
  batches (0,32,64,96); the last batch validates and merges all 384 rows.
  `ensemble-count.txt` defaults to 32; set it to 1 with start 9 to replay the
  formerly crashing case on all three seeds. Restore 32 for the full matrix.
* `Replays`: five suspicious artifact pairs with/without equipment on the same
  three seeds (30 arenas); use these comparisons instead of dividing unrelated
  trials with different seeds. Control and defensive value still need context.

Fixtures create a fresh disabled runtime: `Start`, `Update`, autosave,
meta progression and reward settlement are not invoked. Persistent upgrades
are explicitly empty except the ensemble capacity upgrade above. High-HP
targets avoid death hooks. Pools, agents and
fixture objects are owned and disposed locally. No actual scene instance is
borrowed. Existing Play state is paused only during `Main` and restored on exit;
synchronous entries block frame updates during execution. These are Editor
tools, not Android performance tests.

Single-sphere probes put a sphere at the origin and targets at a controlled
distance. This is a weapon test, not a legal player placement or a solo-run
scenario. Fixed high-HP targets do not reproduce crowds approaching, kill
triggers, acquisition timing, terrain, or actual bosses. Regional role-policy
probes separately exercise movement and attacks, without player weapons.

`scripts/analyze-unity-combat-balance.py INPUT_DIR OUTPUT_JSON` aggregates
the arenas and hashes raw outputs. Raw CSV/JSONL remain in ignored Library;
compact results and limits belong in GPT. Save the source revision and scope
with results. Never label a stat evaluation, activation or controlled arena as
a fully played run. Full-run win rates, economy-conditioned acquisition,
kill procs, boss/hazard interactions and player enjoyment need separate checks.
