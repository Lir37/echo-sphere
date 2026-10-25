using System;
using System.Collections.Generic;
using EchoSphere.Presentation;
using EchoSphere.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoSphereNativeBattleScene
{
    public const string Path="Assets/Scenes/EchoSphere_NativeBattleSlice.unity";
    [MenuItem("Echo Sphere/Native Battle/Create parallel scene")]
    public static void Create()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before creating the scene");
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path)==null)
        {
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var root=new GameObject("ECHO SPHERE Native Battle");
                root.AddComponent<NativeBattleSceneRoot>().AuthorPresentation();root.AddComponent<EchoSphereRuntime>();
                if(!EditorSceneManager.SaveScene(scene,Path))throw new InvalidOperationException("Could not save native battle scene");
            }
            finally
            {
                if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene,true);
            }
        }
        var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if(!scenes.Exists(s=>s.path==Path)){scenes.Add(new EditorBuildSettingsScene(Path,true));EditorBuildSettings.scenes=scenes.ToArray();}
    }
    // The emulator's ABI is a scoped build choice, never the release project
    // configuration. A scene reload still works because this scene is index 0.
    public static object BuildEmulator(string output)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before building");
        Create();
        var architectures=PlayerSettings.Android.targetArchitectures;
        try
        {
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.X86_64;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{Path},locationPathName=output,target=BuildTarget.Android,
                options=BuildOptions.Development|BuildOptions.CompressWithLz4
            });
            var notices=new List<string>();foreach(var step in report.steps)foreach(var message in step.messages)
                if(message.type==LogType.Warning||message.type==LogType.Error||message.type==LogType.Exception)notices.Add(message.content);
            return new {result=report.summary.result.ToString(),errors=report.summary.totalErrors,
                warnings=report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds,
                size=report.summary.totalSize,output,notices};
        }
        finally{PlayerSettings.Android.targetArchitectures=architectures;AssetDatabase.SaveAssets();}
    }
}
