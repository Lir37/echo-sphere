using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Explicit cloud build entry point. The regular Unity Editor bootstrap also
// calls EnsureNativeScene after startup; CI calls it synchronously so that
// the authored native entry is registered before BuildPipeline.BuildPlayer starts.
public static class EchoSphereCiBuild
{
    private const string ApplicationId = "com.lir37.echosphere";
    private const string OutputRelativePath = "Builds/Android/ECHO-SPHERE-debug.apk";

    public static void BuildAndroidDevelopmentApk()
        => BuildAndroid(BuildOptions.Development | BuildOptions.AllowDebugging, OutputRelativePath);

    // Manual repository builds use ordinary IL2CPP settings for phone testing.
    public static void BuildAndroidApk()
        => BuildAndroid(BuildOptions.None, "Builds/Android/ECHO-SPHERE.apk");

    public static void BuildAndroidReleaseApk()
    {
        var keystore = Path.GetFullPath(RequireEnvironment("ECHO_ANDROID_KEYSTORE"));
        if (!File.Exists(keystore))
            throw new FileNotFoundException("[ECHO SPHERE] Release keystore is missing.");

        var alias = RequireEnvironment("ECHO_ANDROID_KEY_ALIAS");
        var storePassword = RequireEnvironment("ECHO_ANDROID_KEYSTORE_PASSWORD");
        var keyPassword = RequireEnvironment("ECHO_ANDROID_KEY_PASSWORD");
        var version = RequireEnvironment("ECHO_ANDROID_VERSION");
        if (!int.TryParse(RequireEnvironment("ECHO_ANDROID_VERSION_CODE"), out var versionCode) ||
            versionCode <= 1)
            throw new InvalidOperationException("[ECHO SPHERE] Release version code must be greater than the test APK's version code (1).");

        var symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android).Split(';');
        if (symbols.Any(symbol => symbol.Trim() == "ECHO_SPHERE_TEST_CONTROLS"))
            throw new InvalidOperationException("[ECHO SPHERE] Remove ECHO_SPHERE_TEST_CONTROLS before building a release.");

        PrepareAndroidBuild();
        var previousVersion = PlayerSettings.bundleVersion;
        var previousCode = PlayerSettings.Android.bundleVersionCode;
        var previousTarget = PlayerSettings.Android.targetSdkVersion;
        var previousCustom = PlayerSettings.Android.useCustomKeystore;
        var previousKeystore = PlayerSettings.Android.keystoreName;
        var previousAlias = PlayerSettings.Android.keyaliasName;
        var previousStorePassword = PlayerSettings.Android.keystorePass;
        var previousKeyPassword = PlayerSettings.Android.keyaliasPass;
        try
        {
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = versionCode;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keystorePass = storePassword;
            PlayerSettings.Android.keyaliasPass = keyPassword;
            BuildPreparedAndroid(BuildOptions.None, "Builds/Android/ECHO-SPHERE-release.apk");
        }
        finally
        {
            PlayerSettings.bundleVersion = previousVersion;
            PlayerSettings.Android.bundleVersionCode = previousCode;
            PlayerSettings.Android.targetSdkVersion = previousTarget;
            PlayerSettings.Android.keystoreName = previousKeystore;
            PlayerSettings.Android.keyaliasName = previousAlias;
            PlayerSettings.Android.keystorePass = previousStorePassword;
            PlayerSettings.Android.keyaliasPass = previousKeyPassword;
            PlayerSettings.Android.useCustomKeystore = previousCustom;
        }
    }

    private static string RequireEnvironment(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("[ECHO SPHERE] Set " + name + " for the signed release build.");
        return value;
    }

    private static void BuildAndroid(BuildOptions buildOptions, string relativePath)
    {
        PrepareAndroidBuild();
        BuildPreparedAndroid(buildOptions, relativePath);
    }

    private static void PrepareAndroidBuild()
    {
        EchoSphereProjectBootstrap.EnsureNativeScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildPreparedAndroid(BuildOptions buildOptions, string relativePath)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException(
                "[ECHO SPHERE CI] No enabled scenes were registered by the project bootstrap.");
        }

        foreach (var scenePath in scenes)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                throw new InvalidOperationException(
                    "[ECHO SPHERE CI] Registered build scene is missing: " + scenePath);
            }
        }

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
        EditorUserBuildSettings.buildAppBundle = false;

        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            throw new InvalidOperationException("[ECHO SPHERE CI] Could not resolve the Unity project root.");
        }

        var outputPath = Path.Combine(projectRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        Debug.Log("[ECHO SPHERE CI] Building Android APK: " + outputPath);
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                "[ECHO SPHERE CI] Android build failed: " + report.summary.result +
                ". See Unity Editor logs in this workflow run.");
        }

        if (!File.Exists(outputPath))
        {
            throw new FileNotFoundException("[ECHO SPHERE CI] BuildPipeline reported success but the APK is missing.", outputPath);
        }

        Debug.Log(
            "[ECHO SPHERE CI] APK created successfully. Size: " +
            new FileInfo(outputPath).Length + " bytes.");
    }
}
