# Native HUD island checkpoint, 2026-10-19

Entry69d7de168f0e8bc1753e7eac1718465f5947f0ca, CI762 SUCCESS.
P1/P2 PARTIAL. Production native HUD flag remains false.

The candidate is now integrated in the actual game UIDocument/pane, between the
original mobile/field layer and combo/pause/modal/tutorial layers. No new pane
is created. After native geometry settles, the original HUD's logical content
is omitted before CSS resolution; two authored hidden boxes retain tutorial/query
anchors. Native controls use their existing authoritative factory callbacks.
The source Hud function still projects before filtering; this bridge does not
claim complete removal of DOM/V, CSS, pause/mobile or the source world renderer.
SourceUiDocument has a pre-paint native-island callback and forwards authored
custom painters into existing retained backdrop prefixes. Image/text copy owns
its built-in painter. Default false preserves the previous source-only path.

Actual saved-scene Editor check:26 assertions PASS. Same pane and UIDocument
count; mobile index2 < native HUD5 < pause6. Source HUD nodes73→2, whole paused
CSS tree442→371. Original network tutorial anchor remains queryable with its
box. Pause blocks native HUD picking; native custom vector is present in the
blur replica with a painter. The original resume button resumes authoritative
simulation; the original mobile pause button pauses it again. Mouse down/up
are paired. Disable restores visible source HUD; re-enable recreates the island.
Successful formation swapping in a real eligible live formation remains untested
here; earlier pointer fixtures use fake callbacks. No Android input claim.
Captures are actual source world + native HUD, with and without the pause overlay,
in a temporary stencil-backed target; original target/clear flags are restored.

## Measured paused-scene A/B/A/B

1260x542, existing saved scene, user pause ON throughout. Four phases each have
10 warm frames and20 samples. ProfilerRecorder retains the existing Timeline
frame range rather than clearing user captures. These are new paired Editor
counters, not active gameplay/device performance. CPU spike variation is visible
in raw phases. GC means are engine GC Allocated In Frame, decimal MB.

| Scope | Source HUD A1/A2 | Native HUD B1/B2 |
| --- | --- | --- |
| Editor FPS | 3.27 /3.74 | 3.91 /3.87 |
| Mean frame ms | 305.67 /267.19 | 255.54 /258.73 |
| GC MB/frame | 60.475 /60.475 | 52.596 /52.597 |
| ResolveCss ms | 142.51 /130.90 | 124.45 /122.56 |

Combined frame-time FPS is3.49→3.89; GC about13% lower. This remains far below
normal gameplay performance. ResolveCss is still dominant; native pause/mobile
and typed world presentation remain necessary. No effects or quality disabled.

Local actual-Unity-DLL player/editor checks zero errors/warnings; Editor compile
PASS; Node272/preflight/provenance PASS. Stop restores6 original preference keys,
leaves zero named HUD/brush RTs, recording/background false, current Console
0errors/0warnings. Recovery and protected source untouched.

Raw evidence: UNITY_NATIVE_HUD_ISLAND_EVIDENCE_2026-10-19.json. Reproduce from
a backed-up diagnostic Play + ContinueSavedRun with
check-unity-native-hud-island.cs and profile-unity-native-hud-island-ab.cs, then
Stop/restore prefs. No APK. Next: native PausePlanner four tabs and mobile controls
using actual source/Core outputs, then typed actors and active CPU/GC/rendering
measurements. Full visual and device gates still open.
