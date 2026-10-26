# Native presentation ownership — 2026-10-26

Promotion update: user approved Unity 6.6; main project now uses native
backdropFilter and authored entry index 0. Prior pending engine/replica retirement
statements below are historical. Current verification/remaining acceptance:
GPT/UNITY_NATIVE_66_PROMOTION_2026-10-26.md.

Latest update: user approved separate-copy Unity 6.6 evaluation. Native engine
backdropFilter replaces production replicas in that copy, authored entry is index
0, actual Editor menu/battle/pause checks pass. Main stays on 6.3; no promotion
decision yet. Earlier pending/unauthorized evaluation statements below are history.
Exact results/limitations: GPT/UNITY_NATIVE_66_EVALUATION_2026-10-26.md.

Branch unity-migration; starting HEAD 568aefb49395ea902549875a9774f1956b0727df.
User priority: complete presentation migration before separate performance work.

## Implemented

- The parallel scene owns all seven screen UIDocuments: battle, home, characters,
  regions, progression, archive and settings. Serialized bindings replace runtime
  creation of unrelated screen GameObjects on this route. Documents start disabled.
- Two authored PanelSettings assets provide menu/run templates. NativePanelLease
  makes one owned runtime instance per initialized screen; density changes cannot
  mutate the shared asset or another screen. Disposal releases only that instance.
- Battle shell, combo track, pause shell/detail and modal shade hierarchy/styles
  live in UXML/USS. Settings layout, controls, confirmation and fixed styling are
  also UXML/USS. Other menu page/scroll shells and main-menu template instances
  are authored assets. Dynamic cards and existing presenters remain typed C# UI.
- NativeMainMenuBackground inserts behind authored content, preserving paint order.
- NativeWorldPresentation is a scene component owning all 16 existing world-view
  groups and their mesh/material/text resources. Runtime supplies the same state
  and paint order. Drawing geometry, animation, simulation and formulas unchanged.
- Development replay tolerates disabled, not-yet-initialized authored documents.
  No release behavior or input/gameplay commands added.

## Verification

- Actual Editor compilation succeeds. All seven serialized documents have their
  panel/layout references and are disabled in Edit Mode.
- Temporary reference classes were compiled directly from starting HEAD, compared
  in the actual Editor, then removed. Battle and settings outputs at density 1 and
  2.75: full target pixel difference 0/255 and no shared-name geometry differences.
  Nonempty images verified; tests cover pause/detail, combo and settings state
  changes, not every gameplay screen or device/backend.
- Actual Play Mode: 14 button submissions across characters, shop, leaderboard,
  achievements, archive, settings, region selection and return. Native panel
  picking reaches each button. All owners remain children of the scene root.
- Continued the pre-existing saved run in Editor: scene document bound, all 16
  view groups initialized under scene ownership. Console 0 errors / 0 warnings.
- Original 10 preference keys restored and verified after stopping Play. Five
  pre-existing user graphics/settings asset hashes unchanged. Recovery copies and
  user SVG importer/settings changes excluded from commits.
- Game contracts: 1383 assertions / 47389 TS-C# comparisons PASS; Node 272 PASS.
  C# actual-API typechecks: 342 runtime / 6 Editor sources, 0 errors/warnings in
  Android player, Editor runtime and Editor assembly configurations.
- No APK, phone/emulator measurements or performance improvement claimed here.

## Remaining migration

This closes scene/document/view-resource ownership, not the entire migration.
The default prototype entry is retained. Native meshes, materials and CommandBuffer
submission are Unity APIs; their mere presence is not a browser compatibility layer.
The genuine remaining bridge is SourceUiReplica/prefix-based backdrop composition
and its fallback users. Explicit camera capture is still OFF and still has that
prefix bridge; it must not be described as completed replica-free presentation.

After unlocking, the separate capture candidate was tested: complex intersecting
clips need compatibility handling; a strict fractional-density case still differed
by up to 3/255. No acceptance or production switch is claimed. Earlier temporary
relaxed comparisons are not acceptance evidence. Stock UIR filter targets observed
as RGBA8 plus depth/stencil, not floating-point color.

Installed Unity 6000.3.24f1 has no IStyle.backdropFilter property (actual API checked).
Later Unity documentation describes it, but no engine upgrade is authorized or
performed. Remaining work uses explicit Unity capture/composition under the current
version, retaining visual/input behavior. Separate optimization stays deferred.

Continue: finish replica-free producer/effect ownership and remaining modal/view
authoring, validate behavior, then switch the primary entry and retire the bridge.
P1/P2/full migration remain NOT DONE. CI result is recorded after publication.

## Ownership checkpoint and authored tutorial

Ownership implementation 734e2776bb123c271ca60fee810f763435f0cb52:
CI #862 SUCCESS, https://github.com/Lir37/echo-sphere/actions/runs/40236238165.

Tutorial fixed hierarchy, label geometry, footer and button styling now live in
NativeTutorial.uxml/.uss. The existing state/actions, spotlight shader, animation
curve, paint parameters and backdrop registrations remain unchanged. CloneTree
loads directly into the authored host, preserving the stylesheet scope; no second
control tree or input forwarding layer is introduced.

Actual Editor paired-reference checks: density 1 without focus, density 2.75 with
spotlight and step 4, then step 4 -> 0 restoring Next. Full nonempty render targets
match at 0/255 with no named-node geometry differences. Next/Skip native picking
and submissions both pass. Final Console 0 errors / 0 warnings; ten preferences
restored and verified after Play; temporary reference/probe classes removed.
These are presentation correctness checks, not performance measurements.

A change of engine version is a separate user decision: current 6000.3.24f1 lacks
backdropFilter; Unity 6000.6 documents a native URP screen-space implementation:
https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/uss-filter/backdrop-filter
The user was asked whether to evaluate 6000.6.1f1 in a separate project copy.
No engine upgrade, installation or change to ProjectVersion/packages was made.
Without that decision, do not silently choose a new engine or declare the existing
replica-based glass pipeline retired. The experimental camera bridge stays OFF.

## Published verification closure

Tutorial implementation b556824f92ebfed20abafc1e406abb1b75f8871d:
CI #863 SUCCESS, https://github.com/Lir37/echo-sphere/actions/runs/40236864927.
Final retained C# sources: 342 runtime / 6 Editor; all three API configurations
0 errors / 0 warnings. Editor recompile and Console ground truth clean; Play
stopped. Original five user settings/render hashes match; protected new-desing
remains 737ef2af4a877e0995db5f92331d536d2f29a934. Four earlier capture-candidate
files remain uncommitted/off and were excluded from these published changes.

Short continuation: unity-migration, implementation b556824f92ebfed20abafc1e406abb1b75f8871d,
CI #863 SUCCESS. Read the three required GPT documents, this report and progress.
Seven scene-owned documents/panel templates, 16 world-view resource groups and
battle/menu/tutorial authored layouts are in place; Editor correctness passed.
Finish replica-free glass and primary-entry retirement before separate performance
work. Resolve the pending separate-copy Unity 6.6 evaluation decision; do not change
the fixed 6.3 engine silently. Preserve user settings, saves, gameplay and quality.
