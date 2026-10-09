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
    private const string RendererPath = "Assets/Settings/EchoSphere_Renderer.asset";

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

        var pipelineObject = new SerializedObject(pipeline);
        var rendererList = pipelineObject.FindProperty("m_RendererDataList");
        var defaultRendererIndex = pipelineObject.FindProperty("m_DefaultRendererIndex");
        if (rendererList == null || defaultRendererIndex == null)
        {
            Debug.LogError("[ECHO SPHERE] URP renderer settings are unavailable. Check the installed URP package version.");
            return;
        }

        // A renderer can exist in the list while the default index is still -1.
        // Validate the selected index separately, not just whether the list has entries.
        var selectedRendererIsValid =
            rendererList.arraySize > 0 &&
            defaultRendererIndex.intValue >= 0 &&
            defaultRendererIndex.intValue < rendererList.arraySize &&
            rendererList.GetArrayElementAtIndex(defaultRendererIndex.intValue).objectReferenceValue is ScriptableRendererData;

        if (!selectedRendererIsValid)
        {
            ScriptableRendererData rendererData = null;
            for (var i = 0; i < rendererList.arraySize; i++)
            {
                if (rendererList.GetArrayElementAtIndex(i).objectReferenceValue is ScriptableRendererData existingRenderer)
                {
                    rendererData = existingRenderer;
                    break;
                }
            }

            if (rendererData == null)
            {
                rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
                if (rendererData == null)
                {
                    rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    rendererData.name = "ECHO SPHERE Renderer";
                    AssetDatabase.CreateAsset(rendererData, RendererPath);
                    Debug.Log("[ECHO SPHERE] Created the default Universal Renderer asset.");
                }

                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                Debug.Log("[ECHO SPHERE] Added ECHO SPHERE Renderer to the URP renderer list.");
            }

            // Always set a known-good default index, including when the list was populated
            // but m_DefaultRendererIndex was unset or pointed at a null entry.
            defaultRendererIndex.intValue = 0;
            pipelineObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(rendererData);
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
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationId);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
    }
}
