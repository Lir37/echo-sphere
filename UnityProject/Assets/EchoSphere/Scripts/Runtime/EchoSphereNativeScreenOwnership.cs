using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private T AcquireNativeScreen<T>(string label) where T:Component
        {
            var scene=NativeBattleSceneRoot.Find(gameObject.scene);
            if(scene!=null)return scene.Screen<T>();
            var owner=new GameObject(label);SceneManager.MoveGameObjectToScene(owner,gameObject.scene);
            return owner.AddComponent<T>();
        }
    }
}
