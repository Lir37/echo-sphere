# Native pause header/footer candidate — 2026-10-21

Entry HEAD: 608b30f929aaa3e0741a2c11e5532f63518f7130, unity-migration.
Implementation HEAD is the commit containing this report. P1/P2 remain PARTIAL.
The pilot defaults false. No gameplay/RNG/formula, protected new-desing or APK changes.

## Implementation

Retained UI Toolkit labels and seven Buttons replace header/footer children only.
Five additional VectorImages reuse exact locked SourceUiIcons geometry. Original
PausePlanner onResume/onExit/setTab closures are forwarded through the existing
write bridge; no duplicate save/exit or gameplay rules. Original GameScreen opts
out; Unity opts in only when the native view is ready. Source shell, ScrollView,
body/detail ordering and modal remain. This is not a fully native pause compositor.

Opening source set detail changes the footer reconciliation host. Fixed the real
lifecycle defect by reparenting the same retained header/footer/buttons, preserving
callbacks and modal input blocking. Source tree decreases 144 ->86 and restores144.
Native line boxes follow authored CSS; source TextCore extra leading is excluded.
Tab transitions use the source 150ms cubic-bezier and premultiplied color blending.

Full computed-style inspection identified two initially missed effects:
- all seven buttons have blur(8px), even though the modal shell disables its own blur;
- Resume/Return have inset white .018 at y1 and black .14 shadow y10/blur24.
Native effect hosts now register seven sigma8 filters in the existing ordered
SourceUiDocument backdrop/snapshot pipeline. Footer shadows/fill/border use the
existing NativeMenuBrush GPU implementation, cached by geometry/DPR. Resources
and registrations are disposed on disable, resume and scene reload. This still
uses the compatibility backdrop pipeline and is not its architectural replacement.

## Evidence and limits

Source model oracle:160 cases/3040 checks. View-only layout:6 RU/EN/width/DPR cases,
509 checks including151 easing and202 premultiplied-color samples. Actual saved-run
lifecycle with blur:47 checks, all four native pointer tabs, original detail blocking
and close, Resume/close advancing simulation, re-pause and registration cleanup.
Original Return/map scene reload:7 checks. These are synthetic native input events,
not physical touchscreen acceptance. Latest reruns and compile results are in the
associated JSON evidence; do not infer Android execution from API compilation.

The isolated blur pixel oracle FAILS to capture its fixture (blank/background image
with attached geometry). The first actual-panel variant also failed its no-blur control. After moving the owned fixture above the source overlay, the control passes, but blur contrast remains .854902 (expected <.04): effect acceptance FAIL.
The later failure is a real open blur-quality gate; filter parameters/registrations alone do not prove working blur. They are retained as failed
capture diagnostics, not acceptance. Actual game screenshots show the controls,
but full pixel/effect/transition parity and DPR/device acceptance remain OPEN.

All PROVISIONAL_NO_BUTTON_BLUR and PROVISIONAL_NO_FOOTER_SHADOW captures are excluded
from equal-quality performance acceptance. Earlier 6.86->14.75FPS and later
7.32->10.79FPS claims describe incomplete effects and MUST NOT be used as the final
same-quality result. Final captures, if present, explicitly follow footer restoration.
BuildTree includes ResolveCss/ProjectTree; never add nested marker durations.
Editor render/memory counters include other panels; UsedTexturesCount0 is unsupported,
not zero textures. No active-game, Android, GPU timing or memory-leak conclusion.

## Remaining

Keep pilot off. Resolve pixel capture and compare source/native effects; validate
narrow/mobile input, retained scrollbar parity and complete pause compositor.
Artifact glyph/detail gates and active world views remain; no P1/P2 DONE claim.
Preserve all source quality and gameplay. Stop Play and restore the six backed-up
preferences after diagnostics. Next continuation begins at the committed HEAD,
reads this report plus main progress and the three mandatory GPT documents.
## 2026-10-21 rerun after footer effect restoration

New Editor session PID13372 connected on7800. Actual lifecycle47 checks PASS again;
source144/native86/restored144, header106/footer46. Both footer GPU paints have
PaintCount1 and cached shadow bounds padding58 (10+2*24), confirming retained
resources rather than per-frame rerasterization. Screenshot inspected in Editor RT.
Real Unity API compile232 runtime C# files and3 editor files:0 errors/warnings.
Locked TypeScript,272 Node tests and all eight visual/provenance checks PASS.

Final CPU A/B/A/B capture (all effects configured) at1260x542:
source192.057/205.106ms (5.207/4.876FPS), native169.005/149.040ms (5.917/6.710FPS).
Combined5.04->6.29FPS; GC24.570->17.717MB/frame. Still unacceptable performance.
Different Editor session/load: do not compare these with prior-session provisional
captures as a controlled change. Pixel parity remains open, so this is measured
candidate performance, not accepted equal-quality or Android improvement.

Final actual Return/reload7 checks PASS; Stop restored six preferences. Console ground truth0 errors/1 warning. Latest actual-panel blur fixture: FAIL contrast .854902 with sigma8 configured; native pilot MUST remain false. Implementation commit46e844e996702cf8b3da528abd8e43fc48f48d6b: GitHub Actions#771 SUCCESS, run39545733207. CI does not close the blur/pixel/device gates.
