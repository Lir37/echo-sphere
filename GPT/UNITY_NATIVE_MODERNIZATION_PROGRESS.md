# ECHO SPHERE — native visual modernization

Current progress report, 2026-10-18. This is the single current status entry point;
older dated reports retain historical evidence rather than defining completion.
Entry HEAD: `9dac7200454494423a6a5007e4c9e926063f4042`, branch `unity-migration`.
Protected source: `new-desing@737ef2af4a877e0995db5f92331d536d2f29a934`.
Implementation HEAD is the commit containing this report (`git log -1 --format=%H`).

## Acceptance status

**PARTIAL. P1/P2 and full native modernization are not DONE.** Menu performance
remains unacceptable. The original cross-panel experiment fails transition parity;
an early-prefix variant now passes the bounded animated-pattern/static-text
fixture. Full menu fidelity is not accepted. Both variants are Editor-only,
opt-in, and are not connected to the production menu.
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
| Home menu, animated background, glass panels | TSX/CSS → retained UI Toolkit; native controls + shared native composition | PILOT | PARTIAL: early prefix passes bounded pattern/static-text fixture; full menu and live animated layers remain unaccepted, not switched |
| Settings | Existing SourceSettingsToolkitScreen native controls over old document | KEEP / MIGRATE hidden work | DONE bounded hidden-work removal: suspend paint/layout, keep timers/viewport; actual Editor checks and identical screenshot; complete original-screen acceptance remains PARTIAL |
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

Initial 12-frame animated-pattern comparison: **FAIL pixel parity**.
Maximum mean channel error 9.3716469/255; maximum channel error 73/255;
34327 pixels differ in the last comparison. Native button submit callback passed.
Visual inspection shows different blur coverage/clipping; the exact fault is not
yet isolated. No accepted workaround, effect removal or production switch.

Continuation experiment (same 256x256 surfaces): the candidate incorrectly
filtered the entire 256x256 input instead of a cropped screen-space region with
four-sigma support. It now filters a 224x184 crop at (16,32), inside the rounded
160x120 border at (48,64), using the same ROI mapping as SourceUiBackdrop.
No production shader or menu changes. Settled-frame error falls to at most five
pixels differing by one channel level (maximum mean0.0000191/255).

The extended 32-frame animated comparison still **FAILS**. At every phase change,
the first frame has mean1.79860–1.79863/255 and maximum73/255; subsequent three
frames at that phase return to mean0.0000114–0.0000191/255, maximum1. All eight
recorded transitions show this pattern. This establishes a one-frame transition
discrepancy in this inter-panel/filter arrangement; the engine's internal pass
responsible has not been isolated. MarkDirtyRepaint on the native texture image
was tested and did not remove the discrepancy; that ineffective change was
removed. Do not hide the failing first frame by extending warmup or delaying
animation. Candidate remains zero replicas; native button callback still passes.

The driver now records all comparisons, tests32 frames rather than12, requires
maximum mean<=0.01 AND maximum channel error<=1 plus the input callback, focuses
Game View and temporarily enables background execution with restoration. It has
a120-second deadline. A mean-only threshold could conceal important local errors.
Static acceptance does not imply animation, full-menu or viewport acceptance.

### Inter-panel scheduling investigation (continuation)

Production UI/gameplay remains unchanged in this group. Tested in the actual
Editor with32-frame comparisons, unchanged sigma8/four-sigma kernel, native
label, rounded clip and input fixture:

| Experiment | Result at transitions | Decision |
|---|---|---|
| Notify both mutable Texture2D pattern panels with RuntimePanelUtils.SetTextureDirty | Same maximum mean1.79863, channel73 | Does not resolve this fixture; ineffective change removed |
| Background sorting2101, reference2100, composite2102 | Existing one-frame mismatch | Baseline remains rejected |
| Background sorting2099, before both other panels | Same mean1.79863/channel73 | Earlier sorting alone does not resolve it |
| Background sorting2199, after composite | Mean18.27803/channel89;46160 pixels differ at transitions | Ordinary composition also becomes stale; retain causal ordering |
| First filter pass directly samples shared RT instead of its UI filter input copy; sorting2101 | Same mean1.79863/channel73 | Bypassing the input copy alone does not resolve it |

In all variants, settled frames still agree within one channel level on3–5pixels.
Native button submission passes. No FPS improvement or full-menu acceptance is
claimed. These comparisons separate ordinary panel ordering from the remaining
filter dependency, but do not identify the engine's exact internal pass. Public
RuntimePanelUtils and filter APIs were inspected in installed6000.3.24f1; no
private render invocation, internal reflection workaround or frame delay ships.

The Editor-only probe now accepts backgroundOrder/externalPrefix parameters;
the driver reads EchoNativeBackdropBackgroundOrder (default2101) and
EchoNativeBackdropExternalPrefix (defaultfalse) from Editor SessionState and
records both in results. Defaults were restored after experiments. The external
input shader resides under Assets/Editor, uses the existing Gaussian support,
mirror mapping and color conversion, and is never selected by production UI.
It is a diagnostic alternative, not an accepted fallback or a second game renderer.
CI749 rejected the initial diagnostic shader loader's UnityEditor.AssetDatabase
reference in Presentation (despite the probe's Editor-only compilation). The
existing runtime-boundary check was kept intact; loading now uses Shader.Find,
with explicit availability checking. Native Editor lookup/support and local
visual provenance checks pass. Recheck CI on the corrective commit separately.

Next architecture experiment must establish same-frame production of the shared
prefix before native filtering, with explicit frame/phase evidence. Do not repeat
sorting/atlas-invalidation experiments as assumed fixes, relax pixel thresholds,
delay animation or switch the complete menu based on settled frames alone.

### Early native prefix producer — bounded temporal gate passed

New Editor-only variant produces the changing background RT in LateUpdate, before
native UI filtering, using Graphics.Blit and a native material. Static label pixels
come from a separate native UI Toolkit target. The output RT has a single writer;
it is no longer simultaneously the target of a later background UIDocument.
Graphics state (active target/sRGB writes) is restored in finally. Rendering is
not issued from generateVisualContent, filter callbacks or UI repaint.

Actual Editor comparison, unchanged threshold(mean<=0.01, every channel<=1),
sigma8 and four-sigma support, no replicas on candidate:

| Native early prefix | Samples | Maximum mean /255 | Maximum channel error |
|---|---:|---:|---:|
| Pattern changes every4 actual frames | 32 | 0.000354766846 | 1 |
| Pattern changes every actual frame, phases12–43 | 32 | 0.000354766846 | 1 |

Both runs PASS the limited fixture including transitions; button submit passes.
Negative control with earlyPrefix=false and a phase change every frame FAILS all
32 samples: maximum mean1.798633575/channel73,21410–21411 differing pixels.
The unchanged oracle therefore still detects the original temporal problem.
81–91 pixels differ by at most one level, rather than21410+ transition pixels
with maximum73 in the old arrangement. This demonstrates that producing the
changing RT before filtering resolves the tested temporal mismatch; it does not
identify every internal Unity pass or establish full-game parity/performance.
The driver exposes EchoNativeBackdropEarlyPrefix(defaultfalse) and
EchoNativeBackdropPhaseFrames(default4). The early variant owns four explicit
256x256 RGBA targets total1048576bytes including reference; candidate uses three.
Native filter atlases, CPU/readback textures and Editor memory are excluded.
After positive/negative runs and Play Mode teardown, native resource lookup found
zero RenderTextures with the probe's ownership prefix. No new Console errors
after cursor1216; current Console counts zero errors/warnings. Probe defaults
restored, Profiler/Play Mode stopped, screenshot moved outside Assets. Local
Core1193/47339, Node272, project preflight, visual provenance and metadata pass;
actual Editor C# compilation and shader execution passed. CI remains a separate
check on the containing commit.

Limits: the label is static and warmed before comparison. Dynamic text, moving
native UI, multiple/transformed panels, real menu orb/background, opacity groups,
resize/DPI and physical input are not accepted by these runs. The screenshot
shows matching grey-looking panels in both fixture arms; comparison against this
Unity reference does not validate original browser/CSS appearance. Preserve
browser-oracle and full-menu visual gates. No production renderer or FPS claim.
Do not reuse a previous-frame label cache for changing text without new evidence.

Three explicit 256x256 RGBA RTs total 786432 bytes in the comparison fixture;
candidate's two explicit RTs are 524288 bytes. This excludes UI filter atlases,
readback/CPU buffers, driver and Editor memory. It is not a total memory claim.
Remaining gates: correct sampling/clipping/color space, pixel oracle, full menu
animation, first layout, actual pointer/touch/keyboard input, portrait/landscape,
screen transitions, resource teardown, hot reload and texture-index exception.

## Measured baseline and limits

### Native Settings hidden-work removal (continuation)

SourceSettingsToolkitScreen has an existing opaque fullscreen #02050d root above
the source document. Previously the hidden document still reconciled/laid out and
prepared brushes/backdrop replicas. The bridge now renders the source Settings
page once on entry, retaining its hook transition/unmount behavior, then suspends
only source presentation. SourceUiDocument keeps Environment.Tick, viewport/safe
area and resize dispatch live. The source root becomes transparent and disabled
without detaching/recreating its UIDocument or collapsing layout. On exit, exact
inline opacity/enabled state is restored and the current page is rebuilt dirty.
Active pointer style is cleared as on focus loss. Native controls, graphics,
gameplay/state projection and all other pages remain on their existing paths.

Controlled short Editor captures in this continuation, both1260x542, same harness,
hardware and production Settings state, no heavy local suite running:

| Settings | Frames | Frame ms / FPS | Main-thread GC bytes/frame | PlayerLoop |
|---|---:|---:|---:|---:|
| Before suspension | 67 | 41.7267 / 23.9655 | 1558392 | 39.4479ms |
| After suspension | 177 | 16.6851 / 59.9336 | 105231 | 2.13245ms |

Before: Layout4.6726ms, brushes/SVG3.5884ms, backdrops14.1228ms.
After: those EchoSphere.UI preparation markers are absent. Roughly60FPS is this
Editor's observed frame pacing, not a measured uncapped/device limit. Renderer
count remains8. Editor memory still includes retained resources; no total memory
or GPU timing improvement is claimed. This fixes Settings hidden work, not Home
menu or active gameplay performance.

Before/after1260x542 screenshots are byte-identical: SHA256
`74288A735AC777811B267CD94D3D9D8329BBEA15F3350686DA77F014D4349F2C`.
Native Editor diagnostic PASS in landscape and480x854 portrait: no hidden Render
callback even when dirty; scheduled timer executes through actual player frames;
viewport stays current; resume restores root and rebuilds. Back navigation,
portrait-to-landscape return and reopening Settings were checked; screenshots
inspected. Events were dispatched through native UI events, not physical touch.
Language/sound/handedness/scale persistence callbacks are unchanged and were not
written during this group. Full device/touch acceptance is still open.

Harness corrections: one initial synchronous timer assertion ran while Editor
was paused; it now waits three actual player frames. A separate return check used
the numeric payload of inline opacity=Null; resolved opacity was1 and enabled was
true, so that assertion did not indicate a production failure. An interrupted
capture was discarded; the table uses completed67/177-frame captures only.

Unity6000.3.24f1; local Pipeline7800; CLI beta.12; AMD Radeon760M;
Game View1260x542. No heavy contract suite ran during these captures. Harness
warms 12 actual player frames, records three seconds, reads main-thread Profiler
data and pauses. Inclusive marker times must not be added together. Editor and
Profiler overhead are included. GPU timings are unavailable.

| Earlier baseline, before Settings suspension | Frames | Frame ms / FPS | Main-thread GC bytes/frame | Selected inclusive work |
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
scene GPU memory. These earlier baselines are not controlled before/after proof
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
1. Validate the fixture against the browser/CSS oracle and resolve the grey panel
   before claiming appearance acceptance. Then extend early native production to
   the real menu background/orb and prove dynamic text/layers update in the same
   frame. Retain the failing cross-panel control; check viewport/input/ownership
   before building/switching the complete native menu.
2. Settings hidden work is removed and checked; retain lifecycle/resize checks
   while extending native migration to other screens. Do not suspend transparent
   overlays or visible animated backgrounds by analogy.
3. Replace dynamic gameplay V/Canvas projection with typed retained native views,
   starting with measured tessellation and HUD tree costs; preserve exact shapes.
4. Run equal-condition active scenarios, Rendering/UI Toolkit/Memory diagnostics
   and portrait/landscape checks. Keep blocked gates visible until measured.

Short continuation prompt: Continue ECHO SPHERE on unity-migration from the commit
containing this report. Read the three required GPT documents and this report;
preserve user changes and new-desing. Hidden duplicate producers/210-star cleanup
are verified; Settings now ~60FPS vs24FPS in short Editor captures with identical
screenshots and live timers/resize. Early native prefix production passes32-frame
animated-pattern/static-label checks, including a change every frame; full menu
and browser appearance are NOT accepted, production remains unchanged. Start with
browser/CSS validation of the grey fixture, then real native background/orb and
dynamic text/layer fidelity, then dynamic gameplay
projection. P1/P2 remain PARTIAL. Check actual HEAD, Editor and CI; no APK.
