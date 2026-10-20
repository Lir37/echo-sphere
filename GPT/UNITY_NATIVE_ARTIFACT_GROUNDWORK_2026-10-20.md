# Native artifact groundwork and current Home measurement — 2026-10-20

Entry HEAD5c881b224e5061b15dd9a10a83cb258d817b8876, CI#766 SUCCESS.
Implementation HEAD is the commit containing this report; unity-migration.
Full modernization and P1/P2 remain PARTIAL. No APK/gameplay/quality change.

NativePauseArtifactsState projects original artifact order/names/descriptions,
rarity labels/card borders, exact presentation-only ArtifactGlyph symbol/color,
and set badge names/owned/total/complete/status from the existing original
artifactSystem helpers. Original source oracle40 cases /10192 checks PASS:
RU/EN,2 frozen and38 generated original PausePlanner views, empty inventory,
all82 artifacts, partial inventory and complete concrete set. Glyph selection
reproduces the source ASCII-id character sum; it does not use/change RNG.
No native artifact tab selection or new game rules were introduced.

NativeMenuBrush now accepts an optional explicit linear angle. Existing default
linear/radial behavior is unchanged. Five GPU/source premultiplied-pixel cases
(145/0/90/225deg, square/rectangle, DPR1/1.75/2) PASS, visible raw channel delta
<=1/255. All12 existing native menu brush regression cases PASS. Real shader
rendering was exercised in the installed Editor; no shader compile error seen.

NativeArtifactGlyph is a CANDIDATE only, not wired into the artifact tab. It uses
the existing GPU ellipse/gradient/Gaussian shadow brush, native Painter2D inner
squares/dashed orbit/cut, native TextCore glyph and text shadow. Inherited font
size is explicit; it preserves the source1.45em relationship rather than making
text proportional to icon size. Twelve original symbols rendered beside the
original component in the same Unity pane at DPR1/2. Original reference bounds
are used for pixel comparisons (the source modal border adds1px). Inspected
paired images; mean RGB error approximately1.86%/4.42%, worst local errors remain
large. Dash/edge/text/placement agreement is NOT fully accepted, so do not activate
or describe this candidate as visually identical. Initial unaligned comparison
was corrected to measured source bounds; no browser was launched.
The expensive source side of this diagnostic blocked a CLI observation >30sec,
producing one Pipeline main-thread-timeout Console error. The render itself
completed both scales and cleaned up. This is not the texture-index3 error.

After removing diagnostic documents/glyphs, current PRODUCTION native Home was
measured in the real Play session:1260x542,30 warm+120 sampled frames,59.940FPS,
16.683ms/frame and202099bytes allocated/frame, Main Thread16.656ms (includes
frame pacing). No SourceUiDocument and Timeline recording stayedfalse. Game tab
was selected and native menu visible when inspected in the actual Editor.
This is a current Editor measurement; no measured old-menu baseline or Android
FPS claim. No new texture-index3 error was observed, but universal resolution is
not claimed. Paused abilities/stats results in their own reports are separate.

Actual Editor compile SUCCESS; real Unity DLL Android player/editor/runtime
checks222 C# sources,0 errors/0 compiler warnings. Node272/preflight/provenance
PASS. CI is separate and must be checked for the code commit. Play stopped;
all6 original prefs restored/verified, background and Profiler recordingfalse.
Console retains the diagnostic Pipeline timeout and deprecation/memory-pressure
warnings; no user capture/logs cleared. Recovery files untouched.

Evidence: UNITY_NATIVE_PAUSE_ARTIFACTS_MODEL_EVIDENCE,
UNITY_NATIVE_ARTIFACT_GRADIENT_EVIDENCE, UNITY_NATIVE_ARTIFACT_GLYPH_EVIDENCE,
UNITY_NATIVE_ARTIFACT_MENU_BRUSH_REGRESSION and UNITY_NATIVE_HOME_CURRENT_PROFILE
(all2026-10-20.json). Reproduction scripts under scripts/.
Next: finish glyph quality checks, native set badges + existing detail callback,
artifact cards in the same-pane ScrollView, then synergies/mobile/world views.
HUD/stats/abilities pilots remain DEFAULT FALSE pending full acceptance.
Protected new-desing unchanged; no APK.
