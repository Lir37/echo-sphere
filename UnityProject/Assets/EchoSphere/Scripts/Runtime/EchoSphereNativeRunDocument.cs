using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRunDocument _nativeRunDocument;
        private double _nativeRunNextModel;
        private void DrawNativeRunDocument()
        {
            if(_nativeRunDocument==null){_nativeRunDocument=new GameObject("Native Run UI Toolkit").AddComponent<NativeRunDocument>();_nativeRunDocument.Initialize(InvalidateNativeUiAnchors,()=>_pauseArtifactSetId=null);}

            var now=Time.unscaledTimeAsDouble;bool update=_sourceUi.Dirty||(!_userPaused&&now>=_nativeRunNextModel);
            if(update){_sourceUi.Dirty=false;_nativeRunNextModel=now+.1;}
            bool active=!_gameOver,upgrade=_levelUp,reward=_stellaRewardPending;
            _nativeMobilePresent=active;
            if(active){
                if(!_nativeMobileMounted){_nativeMobileMounted=true;_nativeMobileHandedness=IsLeftHanded;NativeMobileClearKeys();}
                else if(_nativeMobileHandedness!=IsLeftHanded){ResetNativeMobileGestures(false);_nativeMobileHandedness=IsLeftHanded;}
                if(update)ReadNativeMobileState();
            }else{if(_nativeMobileMounted)ResetNativeMobileGestures(true);_nativeMobileDockState=null;_nativeJoystickState=_nativePlacementState=null;_nativeGhostNetworkState=_nativeFormationMemoryState=null;}
            _nativeRunDocument.Begin(_worldSurface.UiTexture,SourceDevicePixelRatio,update,active,
                NativePauseChromeRequested,!string.IsNullOrEmpty(_pauseArtifactSetId),upgrade,reward,NativeTutorialRequested,
                _combo,_comboTimer,NativeText("combo",IsRussianUi)+" x"+NativePresentationMath.NumberText(_combo),
                NativeText("comboMultiplier",IsRussianUi).Replace("{mult}",NativePresentationMath.NumberText(_comboMultiplier)));
            _nativeRunField=_nativeRunDocument.Field;
            var anchors=_nativeRunDocument.Anchors;anchors.TutorialTarget=ReadNativeRunTutorialTarget;
            if(active){
                var parent=_nativeRunDocument.ControlsParent;
                if(_nativeMobileLayout==null||_nativeMobileLayout.Root.parent!=parent){DetachNativeMobileGestures();_nativeMobileLayout?.Dispose();_nativeMobileLayout=new NativeMobileLayout(parent);}
                _nativeMobileLayout.Root.PlaceInFront(_nativeRunDocument.ControlsPredecessor);
                _nativeMobileLayout.Bind(anchors,IsLeftHanded,(float)NativeInterfaceScale,Screen.width/SourceDevicePixelRatio,_tutorialStep==1,
                    _nativeJoystickState==null?(Vector2?)null:new Vector2((float)_nativeMobileJoystickX,(float)_nativeMobileJoystickY),_nativePlacementState==null?(Vector2?)null:_nativeMobilePlacement,
                    _nativeGhostNetworkState!=null,_nativeFormationMemoryState!=null,SourceDevicePixelRatio);
            }else{DetachNativeMobileGestures();_nativeMobileLayout?.Dispose();_nativeMobileLayout=null;}
            PrepareNativeRunPresenters(anchors);_nativeRunDocument.End(SourceDevicePixelRatio);
        }
        private Rect? ReadNativeRunTutorialTarget(string key)
        {
            if(key=="movement")return _nativeMobileLayout?.MovementBounds;
            if(key=="field")return _nativeRunField?.worldBound;
            if(key=="network")return _nativeHudIsland?.NetworkBounds;
            if(key=="upgrade")return _nativeUpgradeView?.Root.worldBound;
            return null;
        }
        private void PrepareNativeRunPresenters(NativeRunUiAnchors anchors)
        {
            PrepareNativeHudIsland(anchors);PrepareNativePauseScreen(anchors);PrepareNativePauseChromeIsland(anchors);PrepareNativePauseStatsIsland(anchors);PrepareNativePauseAbilitiesIsland(anchors);PrepareNativePauseArtifactsIsland(anchors);PrepareNativePauseSynergiesIsland(anchors);CompleteNativePauseScreen(anchors);PrepareNativeRunResult(anchors);PrepareNativeUpgrade(anchors);PrepareNativeArtifactRewards(anchors);PrepareNativePauseSetDetail(anchors);PrepareNativeTutorial(anchors);PrepareNativeMobileGestures(anchors);PrepareNativeMobileDock(anchors);PrepareNativeMobileFeedback(anchors);PrepareNativeMobileNetwork(anchors);
        }
    }
}
