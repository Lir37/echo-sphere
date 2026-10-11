using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Explicit cloud build entry point. The regular Unity Editor bootstrap also
// calls EnsurePrototypeScene after startup; CI calls it synchronously so that
// a generated scene is registered before BuildPipeline.BuildPlayer starts.
public static class EchoSphereCiBuild
{
    private const string ApplicationId = "com.lir37.echosphere";
    private const string OutputRelativePath = "Builds/Android/ECHO-SPHERE-debug.apk";

    public static void BuildAndroidDevelopmentApk()
    {
        EchoSphereProjectBootstrap.EnsurePrototypeScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

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

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationId);
        EditorUserBuildSettings.buildAppBundle = false;

        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            throw new InvalidOperationException("[ECHO SPHERE CI] Could not resolve the Unity project root.");
        }

        var outputPath = Path.Combine(projectRoot, OutputRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        Debug.Log("[ECHO SPHERE CI] Building Android debug APK: " + outputPath);
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
