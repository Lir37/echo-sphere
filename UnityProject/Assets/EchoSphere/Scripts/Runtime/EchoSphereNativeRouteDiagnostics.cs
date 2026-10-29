using EchoSphere;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRouteJournal _nativeRouteJournal;
        private NativeRouteJournal NativeRoutes=>_nativeRouteJournal??(_nativeRouteJournal=new NativeRouteJournal());
        private void TraceNativeRoute(string reason)=>NativeRoutes.Observe(this.ExistingObjectId(),gameObject.scene.handle,_mainMenu,_regionSelect,(int)_mainMenuPage,_tutorialStep,_userPaused,reason,
            // Bit mask: simulation pause, user pause, upgrade, evolution, reward,
            // game over and player death. Record changes, never every frame.
            (_runEconomy!=null&&_runEconomy.Open?128:0)|(_paused?1:0)|(_userPaused?2:0)|(_levelUp?4:0)|(_choosingEvolution?8:0)|(_stellaRewardPending?16:0)|(_gameOver?32:0)|(_playerCore!=null&&_playerCore.IsDead?64:0),
            _runTime,_choosingEvolution?_evolutionChoices.Count:_stellaRewardPending?_stellaRewardChoices.Count:_levelUp?_levelUpChoices.Count:0);
        private void OnNativeLowMemory()=>TraceNativeRoute("low-memory");
    }
}
