using System.Collections.Generic;
using System.IO;
using EchoSphere.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
internal static class EchoSphereProjectBootstrap
{
    private const string ScenePath = "Assets/Scenes/EchoSphere_Prototype.unity";
    private const string ApplicationId = "com.lir37.echosphere";
    private const string PipelinePath = "Assets/Settings/EchoSphere_URP.asset";

    static EchoSphereProjectBootstrap() { EditorApplication.delayCall += EnsurePrototypeScene; }

    private static void EnsurePrototypeScene()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsurePrototypeScene;
            return;
        }
        ConfigureProjectSettings();
        ConfigureUniversalRenderPipeline();
        Directory.CreateDirectory("Assets/Scenes");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("ECHO SPHERE Runtime").AddComponent<EchoSphereRuntime>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[ECHO SPHERE] Created EchoSphere_Prototype. Open the scene and press Play.");
        }
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var registered = false;
        for (var i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path != ScenePath) continue;
            scenes[i] = new EditorBuildSettingsScene(ScenePath, true);
            registered = true;
            break;
        }
        if (!registered) scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureUniversalRenderPipeline()
    {
        Directory.CreateDirectory("Assets/Settings");
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create();
            pipeline.name = "ECHO SPHERE URP";
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
            Debug.Log("[ECHO SPHERE] Created the URP pipeline asset.");
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureProjectSettings()
    {
        PlayerSettings.companyName = "Lir37";
        PlayerSettings.productName = "ECHO SPHERE";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationId);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
    }
}
