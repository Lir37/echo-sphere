# Native PausePlanner stats data, 2026-10-19

Entry63b9b63f2530a66ee484f101507274497a9d578c. HUD island is diagnostic opt-in;
native pause remains unselected. P1/P2 PARTIAL.

NativePauseStatsState provides typed header,8 character stats and10 ordered
sphere cards. EchoSphereNativePauseStats projects the existing display snapshot
using the exact source PausePlanner helpers and formatting; it does not build
a DOM/CSS tree or copy the older IMGUI stat formulas. Existing move/crit/dodge/
vampire/max-sphere/XP/damage/delay/range/DPS outputs remain authoritative.
No gameplay calculations, branch selections, RNG, balance or new rules changed.
The source's branch/final-name display lookup (including empty-string fallback)
is preserved, rather than silently adding a different level gate.

Original-source oracle:18 cases /1760checks PASS, RU/EN,2 frozen original stats
fixtures and16 generated original PausePlanner stats cases. It covers8 ordered
stats,10 sphere definitions/colors/names, live/dead/empty field conditions,
metrics/descriptions, selected/unknown branches and final-name lookup. Copies
are synthetic display inputs only, never applied to GameState or persistence.
An actual saved-run Editor pause reads the new public projection and matches
all8 displayed source stat labels/values:17 checks PASS,10 types,1 live type.
No native pause widgets were created by this group.

Actual Editor compile and local real-DLL player/editor typechecks zero warnings/
errors; Node272/preflight/provenance PASS (215 C# files). Diagnostic Stop restores
all6 original preference keys; current Console0errors/0warnings.

Raw evidence: UNITY_NATIVE_PAUSE_STATS_EVIDENCE_2026-10-19.json. Reproduce with
check-unity-native-pause-stats.cs in Edit Mode and
check-unity-native-pause-stats-live.cs after backed-up diagnostic Play +
ContinueSavedRun, then Stop/restore preferences.

Next: retained native PausePlanner stats widgets/grid/scroll composition using
this typed model and actual final source styles; then abilities/artifacts/
synergies tabs and native mobile controls. Remove remaining CSS via the proven
same-pane island hook, with visual/input/blur/lifecycle and active profiling
gates. The paused HUD-only3.49→3.89FPS /60.5→52.6MB-frame observation remains
insufficient performance. World presentation still uses the compatibility path.
No APK; protected new-desing unchanged.
