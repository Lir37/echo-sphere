# Native PausePlanner stats content checkpoint — 2026-10-20

Branch unity-migration; entry HEAD 402ecf291539624f7d223e4faa016706727abae2.
Implementation HEAD is the commit containing this report. P1/P2 remain PARTIAL.

NativePauseStatsView retains eight stat cells and ten sphere cards using native
UI Toolkit flex layout, Labels and existing typed GPU card paints. Data comes
from the verified original-helper projection, not duplicated gameplay formulas.
Content attaches to the existing source ScrollView in the same UIDocument.
Original header, tabs, resume/return and modal input/blur ordering remain owned
by the existing screen. Absolute warmup avoids doubling the scroll range; only
after geometry/paint readiness does native content take the flow slot and strip
the source stats descendants. Disable/tab changes dispose it and restore source.
Both stats and HUD diagnostics remain DEFAULT FALSE; no production selection.

Actual saved-run Editor acceptance:44 checks PASS. CSS nodes352->144->352 on
native enable/disable with native HUD held enabled; all8 stat labels/values and
10 sphere names match the live original model. Existing tab click selects
Abilities, disposes stats, and return recreates it. Same pane/document count,
vertical-only scroll, no horizontal overflow, scrollOffset820 and retained root
through scrolling verified. Actual source/native top and native bottom images
were inspected. Small vertical differences remain against the compatibility
view, so full screenshot parity and production activation are not approved.
Six real native Yoga layouts, RU/EN, portrait/wide, DPR1/2:42 checks PASS.
No Android touch/device or active-combat visual acceptance claim.

Paused saved-scene ProfilerRecorder A/B/A/B,1260x542; HUD held enabled;10 warm+
20 sampled frames per phase. Source stats frame317.005/295.166ms vs native
208.843/158.457ms. Combined frame-time FPS3.27->5.45. GC50.826->27.589MB/frame
(decimal MB); ResolveCss134.292->69.384ms, BuildTree140.805->74.569ms means.
Thus rebuilding the compatibility UI/CSS remains a confirmed major CPU/GC cost.
Performance is still unacceptable. These measurements do NOT describe running
combat, the main menu or Android. Existing Timeline was not cleared by scripts.

Editor restarted outside the test session after these successful captures.
The summary evidence was recovered from CLI results; Temp screenshots/raw JSON
were cleared by startup. The previous SessionState-only six-key preferences
backup was lost, so restoration of that interrupted session cannot be confirmed.
After reconnecting, compilation was idle/successful and a fresh six-key backup
was also saved privately outside Unity Temp. The42-check layout session ended
normally and all6 keys restored/verified. Editor stopped; background and Profiler
recording false. Console0 errors; warnings for deprecated Input Manager and
Profiler dropping frames under system memory pressure. No other apps closed.

Local real-Unity-DLL player/editor runtime/editor-assembly checks:216 C# files,
zero compiler errors/warnings; actual Editor compilation SUCCESS. Node272 PASS,
preflight/provenance PASS. CI is recorded by the matching commit's Actions run;
CI is separate from Editor/Play/visual/performance acceptance.

Evidence: UNITY_NATIVE_PAUSE_VIEW_EVIDENCE_2026-10-20.json.
Reproduce: back up prefs before Play, ContinueSavedRun + SetUserPaused(true),
check-unity-native-pause-view.cs, profile-unity-native-pause-view-ab.cs with HUD
held enabled, check-unity-native-pause-layout.cs; Stop and restore prefs.
Never print save payloads. Keep diagnostic backups outside Unity Temp if a
restart is possible. No APK. new-desing remains protected and unchanged.

Next: native pause abilities/artifacts/synergies and mobile controls; finish
visual/input gates before activating the HUD/stats islands. Then remove source
Canvas world presentation with verified native actor/effect views. Preserve
source gameplay/RNG/quality. Do not report the modernization DONE.
