# Native HUD typography checkpoint, 2026-10-19

Entry959b619f3bca40fb886ed82dd5064a65d1c8f4f5, CI761 SUCCESS.
Native HUD remains PILOT/unselected; P1/P2 PARTIAL.

Full retained native HUD comparison now reads existing original full-game
geometry for RU/EN at390x844,1280x720,360x520. No browser was launched or
reference regenerated. Panel/radar/track geometry matches; typography exposed
a standard TextCore character-spacing unit mismatch. The source CSS supplies
pixels, whereas this engine path scales characterSpacing as hundredths of em.
NativeHudView now converts pixel tracking using100/fontSize and keeps authored
line-box heights on single-line labels. Responsive formation labels update both
font size and tracking. Wrapped region/tooltip text retains automatic height.
Source meta counters explicitly retain their authored0.13em (0.91px at7px).

Independent native-label test:2px tracking on8 characters adds14px at8/12/24px
font sizes, exactly, at DPR2. A rejected pointSize-based prototype added12.5px
instead and was corrected. The test checks measured native advances.
Full-game original geometry: max delta11.328px before to2.406px after;48 source
font-size/tracking checks pass. Remaining text widths/fractional line geometry
are not declared pixel-identical or attributed to a proven cause. Full visual
acceptance and device font behavior remain open.

Actual retained HUD Play fixture: six viewport/DPR cases,103 checks,2 fake
callback attempts, drag/cancel/reject/timer PASS. Original model oracle26cases/
420checks PASS. Local real-Unity-DLL player/editor checks zero errors/warnings;
Editor compile PASS; Node272, preflight, provenance PASS. Latest diagnostic
Play Console:0errors/0warnings. Stop restores all6 original preference keys and
leaves no named HUD RTs, recording/background false. Recovery untouched.

Rejected diagnostics: isolated source CSS subtree was not a reliable full-game
pixel oracle (grid/painter instability). Its temporary viewport adapter was
reverted. Advanced Text Generator with the current dynamic SDF assets produced
corrupt glyphs; original project flag false restored and own diagnostic settings
file removed. Neither was enabled in production or accepted as parity.

Raw: UNITY_NATIVE_HUD_TYPOGRAPHY_EVIDENCE_2026-10-19.json. Reproduce with
check-unity-native-hud-tracking.cs and check-unity-native-hud-original-geometry.cs
in diagnostic Play, then Stop/restore prefs. Existing model/HUD fixtures remain.

Next: preserve HUD + pause/mobile/modal ordering in native gameplay composition,
port PausePlanner's four tabs using existing source/Core outputs, then typed
world visuals and comparable active profiling. No new FPS/GC claim or APK.
