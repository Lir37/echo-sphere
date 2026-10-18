# ECHO SPHERE — native visual modernization

Current progress report, 2026-10-18. This is the single current status entry point;
older dated reports retain historical evidence rather than defining completion.
Entry HEAD: `ddfdb9d51cab64a0e444dee0853c33df0ff22131`, branch `unity-migration`.
Protected source: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation HEAD is the commit containing this report (`git log -1 --format=%H`).

## Acceptance status

**PARTIAL. P1/P2 and full native modernization are not DONE.** Menu performance
remains unacceptable. The replica-free composition experiment failed image parity;
it is Editor-only, opt-in, and is not connected to the production menu.
No gameplay, balance, formulas, RNG, visual quality settings, protected branch or
APK build changed in this group. User's preceding committed improvements remain.

The three gameplay documents, continuation notes and current migration audit
addenda were consulted. Seven recent project conversations were checked; some
assistant content was represented only by opaque references, so unavailable chat
content is not treated as implementation evidence. Current source and actual
Editor results take precedence over obsolete audit descriptions.

## Architecture actually in use

`EchoSphereSourcePresentation.LateUpdate` projects native game state into dynamic
`V` objects, executes generated drawing/UI code, then submits native meshes,
materials, render textures and UI Toolkit elements. The backend already uses Unity;
the costly compatibility projection, DOM/CSS evaluation, reconciliation, layout
and backdrop prefix replicas remain in front of it. Renaming Canvas or replacing
accurate meshes with generic particles would not remove those costs correctly.

The camera's `cullingMask=0` prevents legacy renderers from appearing but does not
stop their Updates or resource allocation. Confirmed redundant producers in this
group: SourcePlayerVisual2D, SourceElementalVisual2D,
StandardSphereAttackVfx2D, ProjectileVfx2D, BossPresentationVfx2D,
SourceBackdrop2D and legacy network line visuals. Active source rendering uses
runtime/agent state, not these generated meshes. Enemy appearance metadata and
combat state setters are retained. Network analysis/combat are unchanged.

`UNITY_NATIVE_VISUAL_INVENTORY_2026-10-18.json` records all 199 C# files below
Assets/EchoSphere/Scripts, including names without visual/UI/Canvas keywords,
with content hashes and resource/update/release discovery locations. Discovery
tags are not proof that every path has undergone semantic or image acceptance.

## Decisions by subsystem

MIGRATE = justified replacement/removal; PILOT = prove equivalence first;
KEEP = appropriate existing native/game logic; BLOCKED = missing acceptance proof.

| Subsystem | Actual path / proposed native target | Decision | Status and gate |
|---|---|---|---|
| Home menu, animated background, glass panels | TSX/CSS → retained UI Toolkit; native controls + shared native panel composition | PILOT | BLOCKED: composition pixels differ; full menu not switched |
| Settings | Existing SourceSettingsToolkitScreen native controls over old document | KEEP / MIGRATE hidden work | PARTIAL: opaque native root still has old document updating behind it; suspend only after lifecycle/input/resize proof |
| Map/region selection | Generated EchoMapScreen → source document | PILOT | NEXT: native retained view with original transitions and unlock state |
| Shop | Generated ShopScreen → source document | PILOT | NEXT: preserve purchases, prices, persistence and scroll |
| Characters | Generated CharacterSelect → source document | PILOT | NEXT: original selection, previews and persistence |
| Leaderboard, achievements, knowledge | Generated screens → source document | PILOT | NEXT: typed data and native controls; content/layout/input parity |
| HUD, touch controls, network dragging | Generated App/UI → source document | PILOT | NEXT: remove frame projection only with input, handedness and state parity |
| Pause planner, stats/skills/artifacts/synergies | Existing generated views | PILOT | NEXT: all tabs, resume and scroll acceptance |
| Tutorial, results, confirmation overlays | Existing generated views | PILOT | NEXT: exact trigger/lifecycle/event order |
| Player/shield/charges | Generated renderer → native mesh; hidden legacy producer | MIGRATE duplicate / PILOT typed view | PARTIAL: duplicate producer removed from active path; complete typed native view not accepted |
| Standard sphere | Generated renderer + native mesh; hidden charge producer | MIGRATE duplicate / PILOT | PARTIAL: hidden charge disabled; retain core/ring/charge timing |
| Orbital sphere | Generated contours → native mesh | PILOT | NEXT: all poses, effects and alpha parity |
| Sniper sphere | Generated contours → native mesh | PILOT | NEXT: aim/charge/projectile state parity |
| Shotgun sphere | Generated contours → native mesh | PILOT | NEXT: volleys, motion and visual lifetime parity |
| Chain sphere | Generated contours → native mesh | PILOT | NEXT: original curved links and pulses |
| Aura sphere | Generated contours → native mesh | PILOT | NEXT: aura shape, gradient and animation parity |
| Prism sphere | Generated contours → native mesh | PILOT | NEXT: ray/branch geometry and blend parity |
| Gravity sphere | Generated contours → native mesh | PILOT | NEXT: deformation and field animation parity |
| Pulse sphere | Generated contours → native mesh | PILOT | NEXT: wave timing, clipping and alpha parity |
| Void sphere | Generated contours → native mesh | PILOT | NEXT: distortion and compositing parity |
| Enemies/variants | Native agents + generated visual commands | KEEP simulation / PILOT view | PARTIAL: metadata retained; do not enable an approximate legacy view |
| Bosses/telegraphs | Native agents + generated visual commands | MIGRATE hidden duplicate / PILOT | PARTIAL: hidden producer guarded; visible telegraph/native typed replacement remains |
| Network topology/resonance | Native gameplay analysis | KEEP | Unchanged; no new connection rules |
| Wavy network links/comets | Source renderer → native mesh; hidden LineRenderer updates | MIGRATE duplicate / PILOT | PARTIAL: hidden updates skipped; straight lines are not an accepted substitute |
| Projectiles, hits, explosions, trails | Native simulation + generated drawing commands | KEEP simulation / PILOT view | PARTIAL: hidden projectile VFX guarded; exact lifecycle/trajectories required |
| Pickups/crystals/chests | Native state + generated views | PILOT | NEXT: preserve individual phase/rotation/collection effects |
| Generic ParticleSystem/VFX Graph replacement | Potential native view | BLOCKED | No demonstrated trajectory/blend/RNG/lifecycle and target-device equivalence |
| Background, POIs, region effects | Source state projection + native shader/mesh | PILOT | PARTIAL: unused legacy starfield skipped; typed view and POI projection work remain |
| Fallback star ownership | SourceBackdrop2D world-space SpriteRenderers | MIGRATE lifecycle | DONE bounded scope: 210 stars now owned/destroyed; world origin independent of moving backdrop |
| Mesh, HLSL, render textures, native UI filters | Existing Unity backend | KEEP | Appropriate engine APIs; audit ownership/invalidation instead of deleting by name |
| CSS/SVG/depth/backdrop resources | Native resources behind compatibility layer | KEEP resources / PILOT frontend | PARTIAL: previous reuse/cache fixes remain; no broad parity claim |
| DOM/CSS/V/hooks bridge | Generated compatibility infrastructure | PILOT → MIGRATE | NEXT only after every consumer has an accepted native path; retain reference meanwhile |

## Completed implementation in this group

- Guard five hidden presentation producers before creating meshes, materials and
  render children, only when UsesSourcePresentation is true. Their data references
  and safe combat/motion setters remain. Legacy fallback remains available.
- Skip inactive legacy starfield creation and network LineRenderer updates.
- Give the fallback backdrop's 210 world-space stars a separately owned root and
  destroy it on teardown, without parenting stars to the moving backdrop quad.
- Add an explicitly invoked Editor-only native panel composition probe and a
  bounded real-frame baseline recorder. Neither is selected by game runtime.

Native ownership check in the actual Editor: **PASS**, five disabled producers,
no allocated mesh/material/render children, safe setters; 210 stars remain at the
world origin when the backdrop moves and are destroyed with their owner.
The repeated diagnostic initially timed out while Editor was paused (deferred
Destroy cannot complete without player frames). The harness now temporarily
resumes and restores the prior pause state; the corrected rerun passed.

## Replica-free native composition experiment

`NativeBackdropPanelProbe` uses native panels, textures, labels and a native
button. Candidate background renders to one shared RT and is sampled by a second
native panel. Candidate has zero backdrop replica nodes; reference uses three.
The probe is a prerequisite fixture, not the complete proposed native menu.

Actual 12-frame animated-pattern comparison: **FAIL pixel parity**.
Maximum mean channel error 9.3716469/255; maximum channel error 73/255;
34327 pixels differ in the last comparison. Native button submit callback passed.
Visual inspection shows different blur coverage/clipping; the exact fault is not
yet isolated. No accepted workaround, effect removal or production switch.

Three explicit 256x256 RGBA RTs total 786432 bytes in the comparison fixture;
candidate's two explicit RTs are 524288 bytes. This excludes UI filter atlases,
readback/CPU buffers, driver and Editor memory. It is not a total memory claim.
Remaining gates: correct sampling/clipping/color space, pixel oracle, full menu
animation, first layout, actual pointer/touch/keyboard input, portrait/landscape,
screen transitions, resource teardown, hot reload and texture-index exception.

## Measured baseline and limits

Unity6000.3.24f1; local Pipeline7800; CLI beta.12; AMD Radeon760M;
Game View1260x542. No heavy contract suite ran during these captures. Harness
warms 12 actual player frames, records three seconds, reads main-thread Profiler
data and pauses. Inclusive marker times must not be added together. Editor and
Profiler overhead are included. GPU timings are unavailable.

| Current scenario | Frames | Frame ms / FPS | Main-thread GC bytes/frame | Selected inclusive work |
|---|---:|---:|---:|---|
| Home menu | 14 | 157.96 / 6.33 | 1940185 | Layout10.92ms; brushes/SVG8.49ms; depth2.46ms; backdrops33.78ms |
| Native Settings over source document | 70 | 40.25 / 24.84 | 1558379 | Layout4.23ms; brushes/SVG3.21ms; backdrops13.96ms |
| Active saved run, time5.37s, paused=false | 7 | 254.64 / 3.93 | 31467729 | Tessellate119.65ms; UI.BuildTree72.89ms; Reconcile16.79ms |
| Paused saved run/HUD, time4.22s | 5 | 322.59 / 3.10 | 55781687 | Tessellate0.73ms; UI.BuildTree154.12ms; Reconcile49.97ms |

Home record's harness label was unlabelled after a CLI timeout; recorded runtime
flags independently identify mainMenu=true, page=Home, regionSelect=false,
paused=false, runTime=0. Settings likewise records its actual runtime state.
The paused-run capture inherited the old Settings label after a timed-out label
request; it is classified by recorded mainMenu=false, paused=true, runTime4.22,
not its label. Active run was explicitly resumed and its label/state agree.
Production gameplay capture inspected: player, two spheres, wavy link, enemies,
background and HUD remain present. This does not establish complete visual parity.
Overall Editor allocated memory was approximately595MB/760MB respectively, not
scene GPU memory. These are current baselines, not controlled before/after proof
for the guards, nor evidence that menu performance has been fixed.

Previous active gameplay capture and exact geometry-cache benchmark are retained
in UNITY_MENU_GAMEPLAY_PERFORMANCE_2026-10-18.md: roughly2FPS active gameplay,
dynamic tessellation and UI.BuildTree still dominate. Frozen-state317.99→13.32ms
Canvas generation is not an active gameplay FPS claim. Player-only, each sphere
family alone, controlled enemy/boss counts, network/comets and heavy-VFX isolated
before/after scenarios have not been measured. Do not synthesize them by changing
game rules, RNG or removing visible effects.

## Verification and remaining sequence

Local Core checks PASS1193 assertions +47339 parity comparisons; Node272 PASS;
full VisualChecks PASS, including source drawing streams, UI trees, CSS/layout,
brush pixels, backdrop policy and events/hooks. These do not execute every GPU
path and do not replace image/control acceptance in Play Mode.
Editor recompile reports up_to_date, failed=false, zero compilation errors and
warnings. Actual native ownership fixture passed. Console has historical errors;
the latest inspected entries were Pipeline main-thread5000ms diagnostic timeouts,
not proof of a clean historical Console. The texture-index3 symptom is not marked
resolved. CI on the resulting commit must be checked independently.
Final Console interval after cursor1210 contained no new errors; current Editor
Console counts were zero errors/warnings. Play Mode and Profiler were stopped.
All six original game PlayerPrefs values were restored and verified after stop;
diagnostic captures were moved outside Assets. No user save changes are shipped.

Next work, in order:
1. Resolve replica-free panel composition mismatch without changing blur quality;
   then build the complete native menu pilot and compare all viewport/input states.
2. Eliminate old-document work behind opaque native Settings with explicit
   lifecycle/input/resize tests; preserve hook timers and return-to-menu behavior.
3. Replace dynamic gameplay V/Canvas projection with typed retained native views,
   starting with measured tessellation and HUD tree costs; preserve exact shapes.
4. Run equal-condition active scenarios, Rendering/UI Toolkit/Memory diagnostics
   and portrait/landscape checks. Keep blocked gates visible until measured.

Short continuation prompt: Continue ECHO SPHERE on unity-migration from the commit
containing this report. Read the three required GPT documents and this report;
preserve user changes and new-desing. Hidden duplicate producers/210-star cleanup
are verified; native panel pilot fails pixel parity and is not production. Start
with clipping/sampling/composition, then full native menu and dynamic gameplay
projection. P1/P2 remain PARTIAL. Check actual HEAD, Editor and CI; no APK.
