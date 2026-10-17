# Unity menu flow and centered-grid diagnostics — 2026-10-17

Branch: unity-migration. Starting HEAD: 8be4435b051e35aaa0ed5a7d68fb4ce95eab67cd (CI #734 SUCCESS).
P1/P2 remain PARTIAL. No APK, gameplay, RNG, balance, authored effects or quality changes.

## Confirmed causes and changes

- Reconcile sorts native siblings for z-index painting. UI Toolkit also uses that hierarchy for flex layout, placing the map/sphere entry after controls. SourceUiFlow now projects measured single-line native spans into CSS order + source order, while retaining the native paint/picking hierarchy and composing authored translation. It does not calculate replacement flex sizes. Wrapping, preserve-3d, growth, distributed justification, negative/auto margins and relative main-axis insets remain on the existing path. Growing/distributed fixtures exposed native order-dependent rounding and are explicitly excluded.
- The main-stage grid has one centered item. Custom track projection fed its previously assigned height back into intrinsic sizing after resizing. At 480x854, the main core remained 352px tall and its loadout shrank to 64px. The narrow single-centered-cell case now uses native alignment/intrinsic measurement. CSS-expanded align-items/justify-items are recognized. Final main core is 475px and loadout 92px. No authored dimensions changed.
- EchoSphere.UI.FlowOrder records the new projection separately.

## Checks

- Editor 6000.3.24f1 / CLI beta.12: compile SUCCESS, zero errors/warnings. CLI was not upgraded.
- Native flow fixtures: 1200 comparisons PASS, 12 supported geometry fixtures and 28 excluded grow/justify fixtures, four directions, resize, text, transforms, hierarchy retention, wrapping reset.
- Actual menu versus temporary native source-order hierarchy: six elements, maximum delta 0 at 480x854 and 1260x542, including portrait after returning from landscape/settings. Test restores the original renderer and paint order.
- Editor menu checks: 1206 hierarchy, 1780 style, 7806 sprite indices, 24 grid eligibility checks PASS.
- Core: 1193 assertions + 47339 TS comparisons PASS. Full VisualChecks PASS; Node 272 PASS; project preflight and metadata PASS.
- Play Mode: portrait screenshots inspected; sphere precedes controls, loadout/text no longer compressed by the stale height. Settings/Back events worked. Native panel picking at map/continue centers returned descendants of the correct transformed frames. Physical touch and complete screenshot parity remain unverified.
- Hot recompilation while playing produced InvalidOperationException: Cannot set visual member current in DrawSourceDocument, and a diagnostic request timed out. Stopping/restarting Play Mode recovered. Final Console interval after cursor 1181 contained no new errors. Do not claim globally clean Console or hot-reload support.
- Original texture-index=3 error not reproduced; earlier repaint assertions remain open.
- Play Mode and Profiler stopped, previous Game View size restored, own temporary capture assets moved outside Assets. User package/settings/import/scene changes remain unstaged.

## Performance — not a controlled before/after comparison

Actual CPU Profiler, 480x854, four-second capture, excluding boundary frames: 40 frames, average 98.72ms (~10.13 FPS), main-thread GC.Alloc 5,476,644 bytes/frame. Average markers: Layout 19.27ms, PrepareBackdrops 25.38ms, PrepareBrushesAndSvg 6.06ms, PrepareDepth 2.06ms, FlowOrder 0.279ms. These inclusive marker values are not additive subsystem totals. No GPU timing claim. Menu performance is still unacceptable.

## Context and next step

Latest turns from five prior project chats were accessible this cycle: Продолжение P1 P2 Unity; Проверка модернизации Unity; Аудит миграции Unity; Исправление шейдера (two chats). Assistant bodies often return opaque content references; user observations confirm that lowering quality/removing effects is prohibited and native architecture is the priority.

Continue with backdrop replica/update architecture and Layout costs using the real Profiler; retain authored blur and visual semantics. Verify startup/resize and Console again, then remaining P1/P2 audit items. Check resulting commit CI separately; compile/Play/test PASS does not prove full game parity. Protected new-desing unchanged.
