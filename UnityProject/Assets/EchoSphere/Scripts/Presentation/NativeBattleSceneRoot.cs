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
        internal NativeBattleDocument Document=>_document!=null?_document:throw new InvalidOperationException("Native battle scene has no document binding");
        internal NativeWorldSurface World=>_world!=null?_world:throw new InvalidOperationException("Native battle scene has no world binding");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal static bool PrototypeForDiagnosticReplay;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDiagnosticRoute()=>PrototypeForDiagnosticReplay=false;
#endif
        public static NativeBattleSceneRoot Find(Scene scene)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
            if(_document==null){var child=new GameObject("Battle interface");child.transform.SetParent(transform,false);_document=child.AddComponent<NativeBattleDocument>();}
        }
#endif
    }
}
