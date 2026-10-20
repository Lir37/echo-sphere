# Native pause abilities checkpoint — 2026-10-20

Entry HEAD56a1c69a0cfd3252f36111ae49674748058817f4; that checkpoint CI#765 SUCCESS.
Implementation HEAD is the commit containing this report; branch unity-migration.
P1/P2 remain PARTIAL, diagnostic abilities/stats/HUD selection DEFAULT FALSE.

NativePauseAbilitiesState reads the existing ABILITIES definitions and original
sphereProgression display/evolution helpers. All8 active abilities retain source
order, learned/unlearned conditions, level/7, descriptions and IV/VII fallback
lists. Only acquired passive abilities appear (22 definitions). No gameplay,
RNG, evolution gating, values or formulas changed. No generated source edited.

NativePauseAbilitiesView uses retained native Labels, Yoga and typed GPU card
fills/borders in the original ScrollView/pane. Original tabs, header, footer,
modal blocking and callbacks stay with their existing owner. First-frame warmup
and source-child replacement use the same readiness rule as native stats.
Disable/tab changes dispose resources and restore the source content. Shared
stats text/box helpers became internal without changing stats behavior.

Confirmed typography issue: TextCore intrinsic multiline extents include extra
leading beyond the authored CSS line boxes. Abilities now uses native text
measurement to count wrapped lines, applies original20/16/15px boxes and caches
by text/width. It waits for stable geometry before taking the source slot.
Frozen original wide geometry:734px content,109px cards,983px full list. Native
matches these dimensions. No browser launched; existing reference read only.

Verification:
- Original PausePlanner oracle56 cases /5944 checks PASS,2 frozen and54 generated
  original cases RU/EN; zero/all acquisition, levels1/4/5/7, selected branch and
  final pools and unknown evolution markers; source copies never applied to Core.
- Actual saved-run Play:42 checks PASS after line-box correction;8 names,
  descriptions and levels match. CSS nodes307->144->307. Vertical scroll730px,
  same pane/document count, existing Stats tab click disposes the abilities island,
  return recreates it; disable restores source. Source/native captures inspected.
- Six portrait/wide RU/EN DPR1/2 Yoga cases:39 checks PASS. Acquired level7/all22
  passive cards followed by passive hiding:17 further checks PASS. Remaining
  pixel/Android/input gates are not inferred from these bounded checks.
- Actual Editor compile SUCCESS; local real-DLL Android player/runtime/editor
  assemblies219 C# sources,0 errors/0 compiler warnings. Preflight/provenance PASS.
  Node272 PASS at the stats checkpoint; full CI runs for this code commit.
- Stop and6 original preference keys restored/verified after each session. Editor
  idle, background false, recording false at cleanup; Recovery files untouched.

Paused A/B/A/B saved scene1260x542, native HUD held enabled,10 warm+20 sampled
frames per phase. Source abilities290.728/265.627ms vs native170.322/162.627ms:
combined3.59->6.01FPS. GC39.073->23.821MB/frame(decimal), ResolveCss128.328->
66.029ms, BuildTree130.719->68.291ms. Still insufficient performance. No running
combat, menu or Android FPS inference. Original Timeline was not cleared by the
scripts; Editor auto-recording was found enabled again and repeatedly warned
about system memory pressure/dropped frames. Absolute timing is affected by that
Editor condition; both A/B paths used the same session. Recording stopped at
cleanup. Console0 errors; deprecated Input Manager and memory-pressure warnings.

Evidence: UNITY_NATIVE_PAUSE_ABILITIES_{MODEL,VIEW,LAYOUT,ACQUIRED}_EVIDENCE and
UNITY_NATIVE_PAUSE_ABILITIES_PROFILE_2026-10-20.json. Scripts under scripts/
check-unity-native-pause-abilities*.cs and profile-unity-native-pause-abilities-ab.cs.
No production activation, APK, new-desing change or lowered quality.
Next: native artifact content/glyph/set badges and remaining synergies, mobile
controls, then world views; complete acceptance before activating diagnostic
islands. Full native migration remains unfinished.
