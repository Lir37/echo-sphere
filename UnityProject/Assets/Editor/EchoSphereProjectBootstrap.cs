using System.Collections.Generic;
using System.IO;
using EchoSphere.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
internal static class EchoSphereProjectBootstrap
{
    private const string ScenePath = "Assets/Scenes/EchoSphere_NativeBattleSlice.unity";
    private const string ApplicationId = "com.lir37.echosphere";
    private const string PipelinePath = "Assets/Settings/EchoSphere_URP.asset";
    private const string RendererPath = "Assets/Settings/EchoSphere_Renderer.asset";

    static EchoSphereProjectBootstrap() { EditorApplication.delayCall += EnsureNativeScene; }

    internal static void EnsureNativeScene()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureNativeScene;
            return;
        }
        ConfigureProjectSettings();
        ConfigureUniversalRenderPipeline();
        Directory.CreateDirectory("Assets/Scenes");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            throw new System.InvalidOperationException("The authored native scene is required; do not generate a prototype fallback.");
        var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.path != ScenePath && scene.path != "Assets/Scenes/EchoSphere_Prototype.unity") scenes.Add(scene);
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

        var pipelineObject = new SerializedObject(pipeline);
        var rendererList = pipelineObject.FindProperty("m_RendererDataList");
        var defaultRendererIndex = pipelineObject.FindProperty("m_DefaultRendererIndex");
        if (rendererList == null || defaultRendererIndex == null)
        {
            Debug.LogError("[ECHO SPHERE] URP renderer settings are unavailable. Check the installed URP package version.");
            return;
        }

        // The list may contain valid renderer data while the default index is -1,
        // out of range, or pointing to an empty slot. Validate the selected entry.
        var selectedRendererIsValid =
            rendererList.arraySize > 0 &&
            defaultRendererIndex.intValue >= 0 &&
            defaultRendererIndex.intValue < rendererList.arraySize &&
            rendererList.GetArrayElementAtIndex(defaultRendererIndex.intValue).objectReferenceValue is ScriptableRendererData;

        if (!selectedRendererIsValid)
        {
            var validRendererIndex = -1;
            for (var i = 0; i < rendererList.arraySize; i++)
            {
                if (rendererList.GetArrayElementAtIndex(i).objectReferenceValue is ScriptableRendererData)
                {
                    validRendererIndex = i;
                    break;
                }
            }

            if (validRendererIndex < 0)
            {
                var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
                if (rendererData == null)
                {
                    rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    rendererData.name = "ECHO SPHERE Renderer";
                    AssetDatabase.CreateAsset(rendererData, RendererPath);
                    Debug.Log("[ECHO SPHERE] Created the default Universal Renderer asset.");
                }

                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                validRendererIndex = 0;
            }

            defaultRendererIndex.intValue = validRendererIndex;
            pipelineObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            Debug.Log("[ECHO SPHERE] Repaired the default URP Renderer assignment.");
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        EditorUtility.SetDirty(pipeline);
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureProjectSettings()
    {
        PlayerSettings.companyName = "Lir37";
        PlayerSettings.productName = "ECHO SPHERE";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
        // Keep 32-bit device support and include the required 64-bit player.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        if ((int)PlayerSettings.Android.minSdkVersion < 26)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        // ECHO SPHERE is designed for upright portrait mobile play.
        // Keep Android from rotating into a landscape layout.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
    }
}
