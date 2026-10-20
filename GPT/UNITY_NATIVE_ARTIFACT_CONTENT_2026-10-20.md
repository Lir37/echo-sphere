# Native artifact cards and set badges — 2026-10-20

Entry HEAD9da5bef9915fe92ae7c7e35670bce3d3fe2363b2, CI#767 SUCCESS.
Implementation HEAD is the commit containing this report; unity-migration.
Full modernization/P1/P2 remain PARTIAL. Artifacts pilot DEFAULT FALSE because
full glyph visual parity remains unaccepted (see artifact groundwork report).

NativePauseArtifactsView composes the verified typed projection into retained
native Labels, Yoga cards, existing GPU rarity borders, NativeArtifactGlyph
candidate and native Button set badges. It owns only scroll content; original
pause header/footer/tabs/detail modal stay with the existing screen. Ready/flow
handoff replaces source body descendants only after stable geometry and paint.
Disable/tab changes dispose it and restore source. Native badge selection sends
the same set id to the existing _pauseArtifactSetId presentation state and marks
UI dirty; it does not add inventory, alter Core or change discovery/progress.

NativeArtifactSetBadge uses Unity Button activation, native USS hover transitions
(.15s ease), Painter2D conic ring and native circular hole/text. Visual inspection
caught Unity's999px radius becoming an ellipse and standard Button centering its
labels. Explicit19.5px capsule corners and left labels restore the authored
39px-high shape; ring31px/hole25px. Four RU/EN DPR1/2 native layout, ring-pixel,
Button submit/id and stopped-click propagation cases:224 checks PASS. Corner
and line-box pixels are sampled away from conic boundaries, in actual texels.

Wrapped line-box measurement was extracted unchanged from abilities into
NativeUiLineBoxes for shared artifact descriptions. Abilities regression39
checks PASS, including original109px cards /983px wide list. Existing stats
behavior was not changed by this extraction.

Populated/empty standalone Editor composition on COPIED original data:7 cases,
862 checks PASS; all82 artifacts,50 native set Buttons, RU/EN, portrait/wide,
DPR1/2, empty and repopulated lifecycle. Native Button routes the original set
id. Empty box62px and full empty tab89px match authored geometry. Screenshots
inspected. The82-card/50-button inventory was never installed into gameplay.
No populated-view FPS/device/full glyph parity claim.

Actual saved run contains0 artifacts. Real native empty-tab acceptance21 checks
PASS: CSS nodes149->144->149, same pane/document count, width734/height89;
existing tab controls dispose/recreate content. The actual bound set callback
was invoked DIRECTLY (no real badge exists in this save): original set-detail
modal opens, blocks native content picking, and its original Close button
closes it. This is not a claim of actual populated-inventory pointer input.

Installed Editor compile SUCCESS; real Unity DLL player/editor/runtime checks
225 C# sources,0 errors/0 compiler warnings. Node272/preflight/provenance PASS.
After Stop all6 original prefs restored/verified. Current Console0errors/0warnings
at the final sample; Pipeline history still records the earlier source-glyph
observation timeout, with no new render error during this group. No explicit
Console/Timeline clearing, no texture-index3 fix inferred, no APK, quality or
source-branch changes. User Recovery assets remain untouched.

Evidence: UNITY_NATIVE_ARTIFACT_SET_BADGE_EVIDENCE,
UNITY_NATIVE_PAUSE_ARTIFACTS_LAYOUT_EVIDENCE,
UNITY_NATIVE_PAUSE_ARTIFACTS_VIEW_EVIDENCE and
UNITY_NATIVE_ARTIFACTS_ABILITIES_REGRESSION (all2026-10-20.json).
Reproduce using the corresponding scripts; back up before Play, Stop/restore.
Current native Home59.94FPS/202KB-frame remains the separate current Editor
measurement in UNITY_NATIVE_HOME_CURRENT_PROFILE_2026-10-20.json.

Next: synergies tab, full glyph quality/detail/gesture gates, then native pause
shell/mobile controls/world views. Keep diagnostic islands off until accepted.
