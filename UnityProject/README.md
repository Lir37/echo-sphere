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

The active presentation now translates the original Canvas drawing functions and TSX screen functions into native C#. It retains the source drawing order, per-family Sphere and mutation paths, articulated enemies/bosses, character animation state, original screen trees, Lucide icons, CSS cascade, gradients, masks, shadows, keyframes and transitions. The old procedural migration renderers and immediate-mode menu are disabled. See [the presentation audit](../GPT/UNITY_VISUAL_PORT_AUDIT_2026-10-15.md) for the source inventory, verification and remaining manual acceptance gates.

The browser can decode all 17 referenced source images. Four malformed upstream PNGs decode to entirely transparent pixels in the reference browser; the port includes validated PNG derivatives of that exact visible result. No replacement crystal or shield illustration was invented. Earlier audit entries describing these images as browser decode failures are superseded by this measured result.

Unity Editor compilation, shader execution, Unity screenshot comparison, APK generation and device testing **have not been run** in this cycle, at the user's request. Portable checks compare translated drawing commands, complete screen trees, computed CSS, animation timelines, painted CSS pixels and actual React interaction sequences with the source. These checks do not compile the Unity-facing adapters or certify their final appearance/performance. Manual Unity inspection remains necessary.

## Development rules

- Do not copy `node_modules`, `dist` or Capacitor's `android/` into this project. The authored Canvas and TSX expressions are mechanically translated to C#; the player does not embed a browser or JavaScript runtime.
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
dotnet run --project tools/EchoSphere.VisualChecks/EchoSphere.VisualChecks.csproj --configuration Release
node scripts/check-unity-visual-provenance.mjs
node scripts/export-unity-visual-code.mjs --check
node scripts/export-unity-ui-styles.mjs --check
node scripts/export-unity-canvas-art.cjs --check
node scripts/export-unity-control-art.cjs --check
node scripts/export-unity-presentation-meta.mjs --check
node --experimental-strip-types scripts/export-unity-source-catalog.mjs --check
node --experimental-strip-types scripts/export-unity-sphere-modifiers.mjs --check
node --experimental-strip-types scripts/export-unity-parity-fixtures.mjs --check
node --experimental-strip-types --test tests/*.test.mjs
```

These checks validate project structure, deterministic core contracts and the portable presentation against recorded original-source references. They do **not** replace opening the project in the actual Unity Editor, compiling all UnityEngine scripts, building an APK, or testing the APK on a phone. A failed portable visual comparison exits with code 1 and writes `visual-checks-error.log` beside the checker executable; it no longer opens a Windows unhandled-exception dialog.

The exporter scripts require Node.js 22.15 or newer. `Unity Migration Contracts` runs these checks on pushes to `unity-migration`; it does not invoke Unity or build an APK.

Regenerating visual references requires Node.js 24.19+, the source-locked TypeScript 5.6.3 / React 18.3.1 / Tailwind 3.4.17 dependencies, React Test Renderer 18.3.1 and Playwright with an installed Chromium browser. Normal CI uses checked-in reference data and needs neither a browser nor npm dependencies. Set `ECHO_TYPESCRIPT_PATH`, `ECHO_UI_DEPENDENCIES`, `ECHO_PLAYWRIGHT_PATH` and `ECHO_ART_BROWSER` only when these dependencies live outside the checkout. Run the corresponding `export-unity-*-fixtures` scripts against the unchanged reference, then record their provenance with `node scripts/check-unity-visual-provenance.mjs --record`. Do not record provenance to hide an unexplained comparison failure.
