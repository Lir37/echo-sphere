using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRouteJournal _nativeRouteJournal;
        private NativeRouteJournal NativeRoutes=>_nativeRouteJournal??(_nativeRouteJournal=new NativeRouteJournal());
        private void TraceNativeRoute(string reason)=>NativeRoutes.Observe(GetInstanceID(),gameObject.scene.handle,_mainMenu,_regionSelect,(int)_mainMenuPage,_tutorialStep,_userPaused,reason);
        private void OnNativeLowMemory()=>TraceNativeRoute("low-memory");
    }
}
