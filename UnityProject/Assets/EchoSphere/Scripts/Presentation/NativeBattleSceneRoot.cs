using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Presentation
{
    // Scene-local opt-in. An additive scene cannot switch another runtime's UI.
    [DisallowMultipleComponent]
    public sealed class NativeBattleSceneRoot : MonoBehaviour
    {
        [SerializeField] private NativeBattleDocument _document=null;
        [SerializeField] private NativeWorldSurface _world=null;
        [SerializeField] private NativeWorldPresentation _presentation=null;
        [SerializeField] private NativeMainMenuScreen _mainMenu=null;
        [SerializeField] private NativeCharacterScreen _characters=null;
        [SerializeField] private NativeRegionScreen _regions=null;
        [SerializeField] private NativeProgressionScreen _progression=null;
        [SerializeField] private NativeArchiveScreen _archive=null;
        [SerializeField] private SourceSettingsToolkitScreen _settings=null;
        internal NativeBattleDocument Document=>_document!=null?_document:throw new InvalidOperationException("Native battle scene has no document binding");
        internal NativeWorldSurface World=>_world!=null?_world:throw new InvalidOperationException("Native battle scene has no world binding");
        internal NativeWorldPresentation Presentation=>_presentation!=null?_presentation:throw new InvalidOperationException("Native battle scene has no view resource binding");
        internal T Screen<T>() where T:Component
        {
            Component screen=typeof(T)==typeof(NativeMainMenuScreen)?_mainMenu:
                typeof(T)==typeof(NativeCharacterScreen)?_characters:
                typeof(T)==typeof(NativeRegionScreen)?_regions:
                typeof(T)==typeof(NativeProgressionScreen)?_progression:
                typeof(T)==typeof(NativeArchiveScreen)?_archive:
                typeof(T)==typeof(SourceSettingsToolkitScreen)?_settings:null;
            return screen as T??throw new InvalidOperationException("Native scene has no screen binding for "+typeof(T).Name);
        }
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        internal static bool PrototypeForDiagnosticReplay;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDiagnosticRoute()=>PrototypeForDiagnosticReplay=false;
#endif
        public static NativeBattleSceneRoot Find(Scene scene)
        {
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
            if(PrototypeForDiagnosticReplay)return null;
#endif
            if(!scene.IsValid()||!scene.isLoaded)return null;
            foreach(var root in scene.GetRootGameObjects())
            {
                var owner=root.GetComponentInChildren<NativeBattleSceneRoot>(true);
                if(owner!=null)return owner;
            }
            return null;
        }
#if UNITY_EDITOR
        public void AuthorPresentation()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author native presentation in Edit Mode");
            if(_world==null){var child=new GameObject("World presentation");child.transform.SetParent(transform,false);_world=child.AddComponent<NativeWorldSurface>();}
            if(_presentation==null)_presentation=_world.GetComponent<NativeWorldPresentation>()??_world.gameObject.AddComponent<NativeWorldPresentation>();
            if(_document==null){var child=new GameObject("Battle interface");child.transform.SetParent(transform,false);_document=child.AddComponent<NativeBattleDocument>();}
            T Author<T>(T existing,string label) where T:Component
            {if(existing!=null)return existing;var child=new GameObject(label);child.transform.SetParent(transform,false);return child.AddComponent<T>();}
            _mainMenu=Author(_mainMenu,"Main menu");_characters=Author(_characters,"Characters");_regions=Author(_regions,"Regions");
            _progression=Author(_progression,"Progression");_archive=Author(_archive,"Archive");_settings=Author(_settings,"Settings");
        }
#endif
    }
}
