# ECHO SPHERE Unity migration

This directory is the new Unity runtime. The existing React/TypeScript/Capacitor app remains intact at the repository root as a reference and rollback baseline until the Unity build has passed parity and device acceptance gates.

## Engine baseline

- Unity 6.3 LTS, pinned initially to editor stream 6000.3.
- 2D-first rendering. No production GLB/large 3D asset migration.
- URP package is declared in `Packages/manifest.json`; all graphics remain 2D unless a later explicit design decision says otherwise.
- Android package id: `com.lir37.echosphere`.
- Target launch: landscape Android APK for manual device testing.

## Open the project

1. In Unity Hub, choose **Add > Add project from disk**.
2. Select this folder: `UnityProject` inside the repository checkout.
3. Open it with the installed Unity 6.3 LTS editor.
4. On first editor launch, the editor bootstrap creates `Assets/Scenes/EchoSphere_Prototype.unity` and registers it in Build Settings.
5. Open that scene if Unity does not open it automatically, then press **Play**.

The generated scene is a prototype harness, not the final art direction or a visual port of the current game. The protected `new-desing` branch is the only source of truth for current menu/UI, character, enemy, Sphere, HUD and VFX design. Do not use early 3D experiments or procedural placeholder silhouettes as production references. The harness currently verifies only a subset of movement, combat, Sphere attacks, Formation Follow and XP/level-up. Visual parity remains incomplete.

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
```

These checks validate project structure and deterministic core contracts. They do **not** replace opening the project in the actual Unity Editor, compiling all UnityEngine scripts, building an APK, or testing the APK on a phone.
