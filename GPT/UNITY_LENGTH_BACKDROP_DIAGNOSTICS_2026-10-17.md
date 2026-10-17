# Unity length parsing and backdrop snapshots — 2026-10-17

Branch unity-migration; starting HEAD c60d63f3ef6f82ebb8d521397f77ec67e0100165 (CI #735 SUCCESS). P1/P2 remain PARTIAL.

## Confirmed repeated work and changes

1. VisualCss.Length reparsed unchanged numeric literals with regex on every use. A per-sheet cache now retains only number/unit syntax, bounded at 4096 entries. Percentage reference, font size, viewport and safe-area values remain live; calc/min/max/clamp arithmetic and parsing behavior are unchanged. This is not a cache of evaluated dimensions.
2. Multiple backdrop prefixes repeatedly read the same native style properties. SourceUiReplicaSnapshotCache shares canonical native property reads within one ordered backdrop pass only. Each new pass recaptures values; unused records are pruned. Inline helpers and resolved regular elements keep their distinct read paths. Native hierarchy, render targets, blur passes, texture ownership and visual quality are unchanged. Cache records trade retained memory for fewer native style reads; total process-memory improvement is not claimed.

## Measurements in real Editor

Unity 6000.3.24f1, CLI beta.12, 480x854 menu. Four-second CPU Profiler captures; first two and last boundary frames excluded. Short Editor samples, not device benchmarks or stable FPS guarantees.

| Stage | Frames | Average frame | FPS | Main-thread GC.Alloc/frame | Layout |
|---|---:|---:|---:|---:|---:|
| Before this group | 40 | 96.67 ms | 10.34 | 5,477,203 bytes | 19.45 ms |
| Length cache only | 41 | 91.16 ms | 10.97 | 2,391,572 bytes | 8.41 ms |
| Both changes | 45 | 89.64 ms | 11.16 | 2,392,253 bytes | 8.19 ms |

Final PrepareBackdrops 25.81 ms, PrepareBrushesAndSvg 6.64 ms, PrepareDepth 1.87 ms. Whole-frame backdrop samples fluctuate and alone do not establish its speedup. Same-frame alternating A/B full paint preparation with 1712 copies measured raw 28.72 / 29.52 / 27.59 ms versus shared snapshots 24.47 / 25.20 / 24.35 ms. A/B restores shared mode in finally. No GPU timing collected. One empty Profiler capture was discarded, not treated as a result.

## Verification

- Real Unity compile SUCCESS, zero errors/warnings; Play Mode entered and stopped normally (no hot recompilation while playing this group).
- 8741 comparisons against frozen pre-cache length evaluator PASS, including changed reference/font/viewport/safe-area, invalid/NaN/Infinity inputs and cache eviction.
- Full VisualChecks PASS; Core 1193 assertions + 47339 TS comparisons PASS; Node 272 PASS; project preflight and metadata PASS.
- Native snapshot tests: 21360 IStyle comparisons PASS, both source kinds, changed passes and stale-record pruning.
- Actual menu raw/shared backdrop styles: 152368 comparisons / 1712 copies at 480x854; 176131 / 1979 copies after landscape resize; PASS.
- Actual menu/native source flow: six elements, maximum delta 0 at 480x854 and 1260x542. Settings/Back UI Toolkit events worked. Portrait screenshot inspected; effects and layout preserved within this inspection, not exhaustive pixel/gameplay parity. Physical input not retested.
- Console interval contained Pipeline main-thread timeout errors during diagnostics; requests were repeated only after completion. No new game/renderer exception observed. Original texture index=3 remains unreproduced; earlier repaint/hot-reload issues remain open. Do not claim globally clean Console.
- Prior diagnostic Game View removal used a wrong index convention and had logged Invalid index; this cycle corrected the diagnostic cleanup and removed both exact named temporary sizes. Previous selected size restored. Profiler and Play Mode stopped. Own capture moved outside Assets.
- Existing user package/settings/import/scene changes remain outside commit; protected new-desing unchanged; no APK.

## Next

Menu remains too slow. Continue architectural reduction of backdrop prefix replication/native UI update costs without removing blur/effects; record CPU Timeline/UI Toolkit/Rendering evidence and memory tradeoffs. Continue remaining P1/P2 audit gates only with real Editor verification. Current group requires its own CI SUCCESS; local tests are not CI or full gameplay acceptance. Five recent project chats were inspected in the preceding cycle; constraints remain unchanged.

## Follow-up static allocation cleanup (2026-10-17)

- `SourceUiReplica.Update` previously ran LINQ `Where(...).ToArray()` to collect stale backdrop copies each pass. It now reuses a retained `List<VisualElement>` for the stale source keys and removes copies in the same order. Paint order, blur, child traversal, and per-frame copy semantics are unchanged.
- This is source-level allocation removal only. No Editor FPS/GC measurement or visual parity confirmation was performed for this follow-up; P1/P2 remain PARTIAL.
- It does not address the dominant cost of thousands of active replica updates. Retain that task for source-guided architectural redesign and Editor validation.

## Static native text update guard (2026-10-17)

- In `SourceUiReplica.CopyNode`, retained text copies now update `TextElement.text`, `enableRichText`, and text selection only when values differ. Previously these native properties were assigned on every backdrop pass, even for unchanged menu labels. `MarkDirtyRepaint` is deliberately preserved for animated/custom paint paths; this change does not freeze moving visuals or alter blur passes.
- Source-only change. Unity compile, layout parity and CPU/GC benefit require Editor verification. No FPS improvement is claimed; P1/P2 remain PARTIAL.

## Retained UI Toolkit callback and enable-state guard (2026-10-17)

- In `SourceUiReplica.CopyNode`, the native `SetEnabled` propagation is now invoked only when `enabledSelf` changes. Backdrop copies are non-interactive, but the hierarchy remains native and unchanged.
- `generateVisualContent` now keeps the previously assigned `copy.Painter` delegate when it is still the selected callback, instead of reassigning that property for every replicated source on every pass. Its mutable `PaintTag` and existing `MarkDirtyRepaint` behavior remain intact, preserving per-frame custom drawing and animation.
- Neither change reduces the number of replicas, suppresses blur or changes layout order. Source-only review is complete; Unity Editor compilation, Play Mode visuals and measured CPU/GC remain acceptance gates. P1/P2 stay PARTIAL.

## Logical-node animation scratch reuse (2026-10-17)

- `SourceUiDocument.AdvanceMotion` created a new `Dictionary<string,string>` every animation tick for each logical UI node without a retained native entry. This is now one reused scratch dictionary owned by the document. The traversal completes each node's compare before descending to children, so successive logical nodes never need concurrent snapshots. Retained entries continue using their own per-entry dictionaries. Key/value comparison and frame invalidation are unchanged.
- Source-level allocation reduction, not a switch in animation cadence or UI output. Editor compilation, CPU/GC measurements and visual checks remain pending; this does not close the architecture/backend replica gate or prove FPS improvement.

## Ordered native sibling lookup (2026-10-17)

- `SourceUiReplica.Update` previously called `container.hierarchy.IndexOf(target)` for every retained replica, a linear scan over sibling elements on each backdrop pass. The paint-prefix traversal already tracks each sibling's expected index; the implementation now tests `container.hierarchy[childIndex]` directly (with a child-count guard) and performs the same `Insert` if parent or position differs. This keeps native UI Toolkit hierarchy and exact order semantics while eliminating repeated sibling scans in the unchanged-order case.
- No blur, layout, paint callback, source state, or replica lifetime rules changed. This is a source-level algorithmic improvement; Unity Editor compilation, repaint/layout acceptance and CPU measurements are outstanding. P1/P2 remain PARTIAL; reducing active replica count requires a separate evidence-backed design.
