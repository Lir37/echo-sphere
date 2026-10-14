# ECHO SPHERE Unity migration

This directory is the new Unity runtime. The existing React/TypeScript/Capacitor app remains intact at the repository root as a reference and rollback baseline until the Unity build has passed parity and device acceptance gates.

## Engine baseline

- Unity 6.3 LTS, pinned to **6000.3.24f1**.
- 2D-first rendering. No production GLB/large 3D asset migration.
- URP package is declared in `Packages/manifest.json`; all graphics remain 2D unless a later explicit design decision says otherwise.
- Android package id: `com.lir37.echosphere`.
- Portrait orientation, matching the source mobile controls. Editor bootstrap configures Android settings.

## Open the project

1. In Unity Hub, choose **Add > Add project from disk**.
2. Select this folder: `UnityProject` inside the repository checkout.
3. Open it with the installed Unity 6.3 LTS editor.
4. On first editor launch, the editor bootstrap creates `Assets/Scenes/EchoSphere_Prototype.unity` and registers it in Build Settings.
5. Open that scene if Unity does not open it automatically, then press **Play**.

The scene keeps its historical `EchoSphere_Prototype` filename. It starts the migrated runtime, including combat, progression, characters, regions, artifacts, run continuation and the game screens. The reference is `new-desing` commit `737ef2af4a877e0995db5f92331d536d2f29a934`; see [the before/after audit](../GPT/UNITY_SOURCE_PARITY_AUDIT_2026-10-14.md).

Gameplay entities run through an ordered simulation frame instead of independent `Update` methods. Source coordinates use a downward Y axis and are converted to Unity world coordinates. The port includes source collision separation, dash/death timers, player mutations, fractional XP, Chaos Orb, Sphere branch/final hit effects, projectile ownership, field/network interactions and JSON run snapshots. Object references discarded by `src/runPersistence.ts` are also discarded on resume.

The menu, bilingual Knowledge Base, HUD/radar/tooltips, upgrade cards, pause planner, artifact/set dialogs, regions, settings, tutorial and result screen use source content and layout. Catalog text and all 290 authored Sphere modifier states are exported directly from the TypeScript reference. Background, player silhouettes, regional seams and elemental particle paths use source geometry; existing authored art is imported by the editor bootstrap. Corrupt source PNG files are not replaced with newly designed images: the source's procedural core/ring/shield behavior is retained.

Unity Editor compilation, shader execution, visual comparison, APK generation and device testing **have not been run** in this cycle, at the user's request. Passing the checks below establishes deterministic Core/data contracts and static project checks; it does not establish complete visual or device parity. Manual Unity inspection remains necessary.

## Development rules

- Do not copy `node_modules`, `dist`, Capacitor's `android/`, or the old Canvas renderer into this project.
- Preserve deterministic gameplay contracts, but do not mistake that work for a completed game port. Visual/UI work must map directly to the active `new-desing` source (`src/App.tsx`, `src/conceptStyle.css`, `src/renderer.ts`, `src/spheres/*Visual.ts`, `src/enemies/*`, and screen-specific components).
- Runtime gameplay code must not depend on `UnityEditor`.
- Keep the protected branch `new-desing` intact. Work only on `unity-migration` until functional and visual parity, Unity compilation, and human APK checks pass.
- Do not add invented placeholder art or resurrect assets from early 3D experiments to fill gaps. Translate the current authored Canvas/2.5D visuals and UI into Unity-native rendering and UI.
- Never commit `Library/`, `Temp/`, generated IDE projects, local keystores, ad credentials, or Unity licensing data.

## Validation

From repository root:
```bash
node scripts/check-unity-project.mjs
dotnet run --project tools/EchoSphere.CoreChecks/EchoSphere.CoreChecks.csproj --configuration Release
node --experimental-strip-types scripts/export-unity-source-catalog.mjs --check
node --experimental-strip-types scripts/export-unity-sphere-modifiers.mjs --check
node --experimental-strip-types scripts/export-unity-parity-fixtures.mjs --check
node --experimental-strip-types --test tests/*.test.mjs
```

These checks validate project structure and deterministic core contracts. They do **not** replace opening the project in the actual Unity Editor, compiling all UnityEngine scripts, building an APK, or testing the APK on a phone.

The exporter scripts require Node.js 22.15 or newer. `Unity Migration Contracts` runs these checks on pushes to `unity-migration`; it does not invoke Unity or build an APK.
