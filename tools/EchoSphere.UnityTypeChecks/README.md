# Unity API typecheck without starting Unity

Requires .NET SDK 8 and Python 3.9+. Run from the repository root:

```sh
python3 scripts/prepare-unity-typecheck-references.py --output /tmp/echo-unity-references
dotnet run --project tools/EchoSphere.UnityTypeChecks --configuration Release -- UnityProject /tmp/echo-unity-references /tmp/unity-diagnostics.json
```

On Windows, use `python` and a writable Windows output directory. The preparer
fetches approximately 39 MiB of independent XZ blocks from the official Unity
6000.3.24f1 Linux archive and extracts only the pinned DLLs. Each DLL's SHA-256
is verified against `reference-manifest.json`. Unity is neither installed nor
executed. Reference binaries and archive blocks belong outside the repository.

The checker uses the actual Unity engine API DLLs and the render-pipeline DLLs
shipped in that editor's Universal template cache. It emits the project's C#
assemblies into memory, with C# 9, and checks:

1. Android player code, including its JNI branch, against .NET Standard 2.1.
2. Runtime code with Android and editor defines, against .NET Standard 2.1.
3. The separate editor assembly against the emitted runtime assembly and Unity's
   .NET Framework reference profile.

Any compiler error or warning fails the command. Project code and native Unity
DLLs are not executed. The .NET Standard references come from the official
targeting pack included with the .NET SDK.

An installed matching editor's `Editor/Data` directory can also be passed as the
second argument. Imported project package DLLs are read from
`UnityProject/Library/ScriptAssemblies` when `PackageReferences` is absent.

This check does **not** run Unity's asset importers, package resolution, shader
compilers, Unity-specific analyzers, IL2CPP conversion or the Android SDK/NDK.
It cannot establish GPU output, FPS, driver compatibility or visual equality.
