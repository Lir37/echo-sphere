# Native HUD checkpoint, 2026-10-19

Entry HEAD: a625334d1f1f4bdc37155b8f30e9596ac8eca37d, unity-migration,
CI#757 SUCCESS. Implementation HEAD is the commit containing this report.
Protected new-desing remains737ef2af4a877e0995db5f92331d536d2f29a934.

**P1/P2 PARTIAL. Native Home remains selected. Native HUD is a PILOT candidate,
not selected by production. No gameplay-scene FPS or GC improvement is claimed.**

| Subsystem | Current production | Native alternative | Gain | Risk | Decision/status |
| --- | --- | --- | --- | --- | --- |
| Gameplay HUD | SourceGameView/Hud V/DOM/CSS | NativeHudView retained UI Toolkit | No cascade/tree projection inside this view | Full visual/overlay/input parity still required | PILOT, bounded Editor tests pass |
| Radar/POI | Source RadarHud projection | NativeHudRadarView, Painter2D arrows, typed GPU fills/filters | Retained pooled dots, native vector geometry | Parent clipping and fractional DPR | MIGRATE component verified, runtime binding pending |
| Pause/mobile/other overlays | Existing source components | Retained native views in gameplay composition | Remove dominant ResolveCss cost | Source formulas, modal order and controls must remain exact | PILOT, next work |

## Implementation

- NativeHudView retains health/XP/resonance bars, counters, formations, tooltip,
  timer, radar, region text and existing boss telemetry. Native layout, text,
  pointer capture, scheduled five-second tooltip hide, transitions, filtering and
  meshes belong to Unity. No CSS/DOM parser is used inside the candidate view.
- EchoSphereNativeHud reads display values from the existing render snapshot and
  calls existing authoritative helpers. It does not duplicate combat calculations.
  Its factory preserves the source swap callback's success/failure contract.
  This compatibility model reader still uses V; it is not a complete typed world.
- Drag state identifies dominant/secondary slots, not mutable formation types.
  The default Button Clickable is removed because the source uses capture/hold;
  it must not release capture before the drag handler. Cancellation, rejected
  swaps and silent rebinding retain source behavior.
- NativeHudSurface and NativeMenuBrush supply typed GPU fills/shadows. The shared
  brush gains only horizontal gradients; existing menu defaults stay unchanged.
- Separate native clip containers retain contents while preserving each frame's
  border and external shadow. Offscreen rounded clipping requires a stencil-backed
  target and clearing stencil/color; these are fixture settings, not game quality
  changes. First presentation waits for binding and settled geometry.
- A triangle SDF pilot was rejected at max14/255 versus the existing10/255 mask
  gate. It was removed, not accepted with a larger threshold. POI arrows now use
  Painter2D plus UI Toolkit post-processing and the established Gaussian shader.
  Authored vertices remain10x10 logical pixels: using Yoga's snapped host size
  changed edges at fractional DPR. Native vector/drop-shadow comparison now
  passes all three colors at DPR1/1.75/3 with zero pixel difference.

## Bounded verification

- Unity6000.3.24f1 recompiles without compiler errors/warnings.
- Original RadarHud tree oracle:4409 comparisons across24 states, alive filtering,
  caps40/12, positions, boss/elite precedence, POI color/orientation and reuse.
- 39 real GPU/source CSS brush cases at DPR1/1.75/3, using unchanged limits:
  visible RGB<=1byte, alpha<=1byte, premultiplied RGBA<=2bytes.
- 12 existing menu-surface GPU/source cases continue to pass after the shared
  shader change. 9 native arrow/independent primitive cases pass at zero error.
- Actual retained HUD panel fixtures cover1260x542@1,360x800@2 (safe insets),
  800x360@3,320x568@2,480x854@1,854x480@3. Pixel-presence checks reject white
  placeholders, missing player dots, clipped-away frame borders and HP gradients.
- Pointer tests use actual UI Toolkit events with fake external callbacks:
  drag success, cancellation, rejection, and120 silent binds. A native timer
  is observed against elapsed Editor time. They do not prove real gameplay drag
  parity or Android touch behavior. The initial model is the existing saved run;
  later states exercise display-only formations/POIs without modifying GameState.
- Core1193/47339, Node272, full local VisualChecks, project/provenance checks pass.
  Frozen browser-produced fixtures are local references; no browser was launched.
- Preferences are backed up before diagnostic Play and restored/verified after
  Stop. Original recovery assets remain untouched. Profiler recording was found
  enabled with low-memory warnings and stopped; no new FPS measurements are
  inferred from these visual fixtures. Historical tool/compile logs are retained.
- Final clean Stop leaves zero named native brush/HUD targets and zero HUD/radar
  shadow definitions. One interrupted pre-Play diagnostic's orphan target was
  released separately; it is not counted as a successful natural lifecycle.

Raw bounded evidence: UNITY_NATIVE_HUD_EDITOR_EVIDENCE_2026-10-19.json.
Inventory now scans213 C# files, including files outside visual/UI filename tags.

## Reproduce / next

Use Unity CLI eval/eval_file's positional30000ms dispatcher budget with a45s CLI
timeout. Scripts live outside Assets. Back up with
backup-unity-gameplay-diagnostic-prefs.cs, enter Play, restore the existing saved
run through ContinueSavedRun (it stays user-paused), then run the HUD/radar checks.
HUD brush checks need export-unity-gameplay-ui-reference.cs first. Wait for each
async SessionState result before Stop/recompile; restore preferences afterward.
Captures use unique directories under UnityProject/Temp/native-hud.

Complete RU/EN and full visual/input/model parity, then integrate HUD in a native
gameplay composition with correct pause/modal/mobile-control ordering. Preserve
native custom vector painters when preparing any retained backdrop prefix.
Port PausePlanner's four tabs from actual source, without recreating its formulas.
Then capture comparable active gameplay CPU Timeline/GC/rendering/memory. No APK
until the final modernization stage. CI, compilation, Play, pixels and performance
remain independent gates. Check Actions for the actual resulting HEAD.

## API correction after CI758

CI758 on72dad445 failed unity-typecheck because two obsolete API warnings are
errors in this gate; core/source/visual jobs passed. Replaced dashPattern with
SetDashPattern and PreventDefault with FocusController.IgnoreEvent. Pointer
propagation and capture remain available. Local Android player/editor/runtime
typechecks against installed real6000.3.24f1 DLLs now have zero errors/warnings
(local harness copied to Temp/net10 because this laptop lacks the pinned SDK8;
GitHub remains the pinned SDK8 gate). Editor recompile and repeated six-viewport
HUD layout/pixel/pointer/cancel/rejection/timer fixture PASS,98 checks,2 fake
callback attempts, auto-hide5.418s. Stop and restoration of six original game
preference keys PASS. Native HUD remains unselected; no new FPS/GC claims.

## HUD vectors in native blur prefixes, 2026-10-19

Entry47bac8b6, CI759 SUCCESS. NativeMenuGlass omitted custom Painter2D meshes
from the fast image prefix and supplied no custom painter to the retained prefix.
A real Editor GPU fixture with actual NativeHudPoiArrow/NativeHudDashBorder
reproduced max170/255 and mean1.499 byte error before correction.
The image path now delegates custom meshes to Unity's retained prefix and
forwards their authored generateVisualContent; native Image/Text callbacks stay
owned by their copied element to avoid double rendering. Blur stays enabled.
After correction: eight changing-image samples each at DPR1/1.75/3, max0/mean0;
existing text+changing-image regression32 samples max0/mean0. C# player/editor
checks zero warnings/errors; Editor compile PASS; Node272/preflight/provenance
PASS. Stop restores six original preference keys and leaves zero named prefix
RTs. No new FPS/GC/Android claim; HUD still PILOT and unselected.
Raw evidence: UNITY_NATIVE_HUD_VECTOR_BLUR_EVIDENCE_2026-10-19.json.
Next: full RU/EN model/pixel parity, native gameplay composition preserving
pause/mobile/modal order, then typed gameplay visuals and actual profiling.

## Explicit HUD display projection / RU-EN oracle

Entry0962dec02ca99839391e766a70237038471003ab, CI760 SUCCESS. The existing
ReadNativeHudState now delegates to an explicit internal display projection
(scene snapshot, language). This extracts the same expressions and source
helpers without changing formulas or connecting the pilot to production.
The Edit-mode oracle compares26 cases /420 checks:2 frozen original-source
HUD fixtures and24 generated original Hud cases. RU/EN text, HP rounding,
XP/HP/resonance clamping, resonance ready color/counter, sphere/wave counters,
time, formation names, boss labels/visibility, region modes and pocket fallback
match. Fixtures are cloned display data; no runtime GameState, preferences or
active source hooks are modified. Script refuses Play or an existing hook tree.
Real Editor compile and local player/editor C# typechecks pass without warnings.
Raw evidence: UNITY_NATIVE_HUD_MODEL_EVIDENCE_2026-10-19.json. Full pixel parity,
production gameplay composition and active performance measurements remain open.
No new FPS/GC or Android claims; HUD remains PILOT, P1/P2 PARTIAL.

### Native HUD pixel tracking / line boxes

Entry959b619 (CI761 SUCCESS): corrected standard TextCore tracking units locally
in NativeHudView, single-line boxes and source meta-counter0.13em spacing.
Measured2px tracking gives exact14px advance on8 chars at8/12/24px. Six original
full-game viewport cases give max geometry delta11.328→2.406px;48 authored
font-size/tracking checks pass, remaining pixel/font geometry is still open.
Six-viewport retained HUD pointer/timer fixture103checks and model420checks PASS.
Native HUD remains unselected; no FPS/GC/device improvement claim. Rejected
Advanced Text Generator and isolated source viewport experiments were reverted.
See UNITY_NATIVE_HUD_TYPOGRAPHY_2026-10-19.md and its raw evidence JSON.

### Native HUD in the real game pane (diagnostic opt-in)

Entry69d7de1 /CI762 SUCCESS. Actual saved-scene same-pane island checks26PASS:
source HUD73→2 nodes, paused tree442→371; source tutorial anchor, pause occlusion,
custom painter in blur prefix, original resume/mobile-pause callbacks and
disable/re-enable pass. Flag defaults false. Paused A/B/A/B: combined Editor
3.49→3.89FPS, GC60.5→52.6MB/frame; ResolveCss still about123ms. Not normal
gameplay performance or an active/device measurement. Source world, pause and
mobile CSS/V remain. See UNITY_NATIVE_HUD_ISLAND_2026-10-19.md and evidence JSON.
