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

The generated scene is a prototype harness, not the final art direction. It exists to verify movement, combat, Sphere attacks, Formation Follow, XP/level-up, and runtime composition before the full content port.

## Development rules

- Do not copy `node_modules`, `dist`, Capacitor's `android/`, or the old Canvas renderer into this project.
- Port deterministic rules before UI/visual code; preserve behaviour by adding parity tests against the current TypeScript contracts.
- Runtime gameplay code must not depend on `UnityEditor`.
- Keep the old branch `new-desing` intact. Work only on `unity-migration` until the Unity build and human APK checks pass.
- Never commit `Library/`, `Temp/`, generated IDE projects, local keystores, ad credentials, or Unity licensing data.

## Validation

From repository root:
```bash
node scripts/check-unity-project.mjs
dotnet run --project tools/EchoSphere.CoreChecks/EchoSphere.CoreChecks.csproj --configuration Release
```

These checks validate project structure and deterministic core contracts. They do **not** replace opening the project in the actual Unity Editor, compiling all UnityEngine scripts, building an APK, or testing the APK on a phone.
