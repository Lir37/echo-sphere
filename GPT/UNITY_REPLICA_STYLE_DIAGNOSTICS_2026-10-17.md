# Unity backdrop style writes — 2026-10-17

Entry HEAD: `f54dd70a39737ef2372016a51bdf868b08d2b5f0`, `unity-migration`.
Local Unity 6000.3.24f1 verified via MCP, then Unity CLI beta.12 / port 7800 after
the session's MCP tools became unavailable. No CLI upgrade or APK build.

## Change and reason

Backdrop replicas resubmitted all inline style values each frame, even when equal.
`SourceUiReplicaStyle` retains the last requested scalar/value properties of each
owned replica and only forwards changed requests to Unity. Native getters remain
authoritative because Unity canonicalizes some values (empty background vs Null).
Mutable style lists pass through; Clear resets the cache; the legacy background
alias refreshes all cached requests. Geometry, transform, appearance and root
overrides use the same writer. Paint callbacks still run at the original cadence.
Unity retains layout, filter/render-target ownership, effects and rendering.

## Measured evidence

Short warmed 480x854 Editor Profiler captures during the October 16 portion of
this cycle (main thread, profiling overhead included):

| | Before, 19 frames | After initial cache, 29 frames |
|---|---:|---:|
| Mean frame | 139.701 ms | 102.932 ms |
| Derived capture FPS | 7.16 | 9.71 |
| PrepareBackdrops | 56.945 ms | 28.308 ms |
| Layout | 23.097 ms | 18.345 ms |
| GC.Alloc bytes/frame | 5,939,933 | 5,506,100 |

These are short sequential samples, not a device benchmark or proof of a GC
improvement. Layout also varied despite being unchanged by this patch.

After the getter correction on October 17, an alternating same-frame A/B test
used 1979 copies at 1260x542 and the same live source state. Five warmed full
PreparePaintResources calls per round:
- Native writer: 52.477 / 61.150 / 51.225 ms per call.
- Retained writer: 33.634 / 33.863 / 32.270 ms per call.
Both paths execute the same preparation and paint-prefix traversal. This isolates
writer cost more closely; it is not an FPS measurement. Cache construction was
outside the timed loops and all copies were restored to retained writers afterward.

## Validation

- Actual Editor recompile: SUCCESS, zero errors/warnings on final compile.
- `scripts/check-unity-replica-style.cs`: 66,305 native property comparisons PASS,
  including repeated writes, keywords, changing source values and Clear(true/false).
- Existing Editor menu checks: 1206 hierarchy, 1780 styles, 7806 indices and
  14 grid eligibility checks PASS.
- Core: 1193 assertions + 47339 TS comparisons PASS. Full VisualChecks PASS.
- Node: 272 tests PASS. Unity project preflight PASS.
- Play Mode entered repeatedly; portrait menu and final landscape screenshot
  inspected. Settings opened and returned via actual UI Toolkit events.
- Physical pointer/touch and complete screenshot parity remain unverified.
- Final transition interval had no new game exceptions or texture-index error.
  CLI diagnostics did log a main-thread timeout and a rejected screenshot output
  path. These are recorded separately from game/renderer errors; Console-wide
  cleanliness is not claimed. The earlier repaint assertions remain unresolved.
- Temporary screenshot assets moved outside Assets. Play Mode stopped.

## Remaining gates

P1/P2 remain PARTIAL. The menu is still too slow. Z-index sorting still changes
native flex flow: the map/sphere entry appears below the controls. Do not drop
z-index semantics to hide this. Remaining work includes separating paint and
layout order, reducing backdrop replica work, startup/resize behavior, native
screen migration and further Profiler/Console/visual verification.
Gameplay, balance, RNG, authored visual assets and protected new-desing unchanged.
Existing user package/settings/import/scene changes remain outside the commit.
Recent 5–15 project chats were not accessible through available tools.
Check CI on the resulting commit; local/Editor PASS is separate from CI SUCCESS.

CI #733 on b72ccbed65cce0bd2806433837bb8aaeea055150 passed source typecheck
and core contracts, but the strict Unity API job failed on two obsolete-member
warnings (zero compile errors). The required legacy IStyle alias implementation
now carries Obsolete like Unity's interface, without disabling warning checks.
The corrected commit requires a new CI result.
