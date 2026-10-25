using System;
using System.Collections.Generic;
using EchoSphere.Presentation;
using EchoSphere.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class EchoSphereNativeBattleScene
{
    public const string Path="Assets/Scenes/EchoSphere_NativeBattleSlice.unity";
    private const string PanelFolder="Assets/Resources/EchoSphere/NativeBattle/";
    // Idempotent authoring; existing panel settings are never overwritten.
    [MenuItem("Echo Sphere/Native Battle/Author scene documents")]
    public static void AuthorDocuments()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before authoring documents");
        var scene=SceneManager.GetSceneByPath(Path);bool opened=!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(Path,OpenSceneMode.Additive);
        try{
            var root=NativeBattleSceneRoot.Find(scene)??throw new InvalidOperationException("Native scene root is missing");
            root.AuthorPresentation();
            PanelSettings Panel(string name,int order){
                var path=PanelFolder+name+".asset";var panel=AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
                if(panel!=null)return panel;
                panel=ScriptableObject.CreateInstance<PanelSettings>();panel.name=name;panel.scaleMode=PanelScaleMode.ConstantPixelSize;
                panel.sortingOrder=order;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
                AssetDatabase.CreateAsset(panel,path);return panel;
            }
            var runPanel=Panel("NativeBattlePanel",1000);var menuPanel=Panel("NativeMenuPanel",1001);
            void Bind(Component owner,PanelSettings panel,VisualTreeAsset layout=null){
                var doc=owner.GetComponent<UIDocument>()??owner.gameObject.AddComponent<UIDocument>();
                doc.enabled=false;doc.panelSettings=panel;if(layout!=null)doc.visualTreeAsset=layout;EditorUtility.SetDirty(doc);
            }
            foreach(var owner in root.GetComponentsInChildren<MonoBehaviour>(true)){
                if(owner is NativeBattleDocument)Bind(owner,runPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeBattle"));
                else if(owner is SourceSettingsToolkitScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeSettings"));
                else if(owner is NativeMainMenuScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeMainMenuDocument"));
                else if(owner is NativeCharacterScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeCharacters"));
                else if(owner is NativeRegionScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeRegions"));
                else if(owner is NativeProgressionScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeProgressionShell"));
                else if(owner is NativeArchiveScreen)Bind(owner,menuPanel,Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeArchiveShell"));
            }
            EditorUtility.SetDirty(root);EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Could not save authored native documents");AssetDatabase.SaveAssets();
        }finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
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
        AuthorDocuments();
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
