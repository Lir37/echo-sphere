using System;
using System.Collections.Generic;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeTutorialView _nativeTutorial;
        private Rect? _nativeTutorialTarget;
        private double _nativeTutorialTargetAt;
        private string _nativeTutorialTargetKey;
        private static readonly string[] NativeTutorialTitles={"tutorialWelcomeTitle","tutorialMoveTitle","tutorialPlaceTitle","tutorialNetworkTitle","tutorialUpgradeTitle","tutorialDoneTitle"};
        private static readonly string[] NativeTutorialBodies={"tutorialWelcomeBody","tutorialMoveBody","tutorialPlaceBody","tutorialNetworkBody","tutorialUpgradeBody","tutorialDoneBody"};
        private static readonly string[] NativeTutorialTargets={null,"movement","field","network","upgrade",null};
        private bool NativeTutorialRequested=>!_mainMenu&&!_gameOver&&_tutorialStep>=0;
        private void DisposeNativeTutorial(){_nativeTutorial?.Dispose();_nativeTutorial=null;_nativeTutorialTarget=null;_nativeTutorialTargetKey=null;}
        private void PrepareNativeTutorial(NativeRunUiAnchors anchors)
        {
            if(!NativeTutorialRequested){DisposeNativeTutorial();return;}
            var slot=anchors.Tutorial;var host=slot.Content;if(host==null){DisposeNativeTutorial();return;}
            if(_nativeTutorial==null||_nativeTutorial.Root.parent!=host){DisposeNativeTutorial();_nativeTutorial=new NativeTutorialView(host,anchors.Backdrops,NextNativeTutorial,SkipNativeTutorial);}
            _nativeTutorial.Bind(ReadNativeTutorialState());
            string key=_tutorialStep+"/"+IsLeftHanded+"/"+Screen.width+"/"+Screen.height;double now=Time.unscaledTimeAsDouble;
            if(key!=_nativeTutorialTargetKey||now>=_nativeTutorialTargetAt){
                _nativeTutorialTargetKey=key;_nativeTutorialTargetAt=now+.120;_nativeTutorialTarget=null;string target=NativeTutorialTargets[_tutorialStep];
                if(target!=null)_nativeTutorialTarget=anchors.TutorialTarget?.Invoke(target);
            }
            bool ready=_nativeTutorial.Ready;float scale=SourceDevicePixelRatio;_nativeTutorial.Prepare(Screen.width/scale,Screen.height/scale,scale,_nativeTutorialTarget);
            if(_nativeTutorial.Ready){slot.HideLegacy();if(!ready)anchors.Invalidate?.Invoke();}
        }
        private bool CanAdvanceNativeTutorial()
        {
            if(_tutorialStep==0||_tutorialStep==3||_tutorialStep==5)return true;
            if(_tutorialStep==2)return NativeMobileSphereCount>=2;
            if(_tutorialStep!=1)return false;
            var player=NativeSourcePixels(PlayerPosition);var start=NativeSourcePixels((Vector2)_tutorialStartPosition);double dx=player.x-start.x,dy=player.y-start.y;return Math.Sqrt(dx*dx+dy*dy)>=72;
        }
        private NativeTutorialView.State ReadNativeTutorialState()
        {
            string lang=IsRussianUi?"ru":"en";string T(string key)=>NativeText(key,lang=="ru");bool advance=CanAdvanceNativeTutorial(),action=_tutorialStep==1||_tutorialStep==2;
            string status=_tutorialStep==4?(IsRussianUi?"Выбери одну карту. Обучение продолжится после выбора.":"Choose one card. The tutorial continues after your choice."):advance&&action?T(_tutorialStep==1?"tutorialMoveDone":"tutorialPlaceDone"):(IsRussianUi?"Выполни действие, чтобы продолжить.":"Perform the action to continue.");
            return new NativeTutorialView.State {Step=_tutorialStep,CanAdvance=advance,ShowAction=action,Kicker=(IsRussianUi?"ОБУЧЕНИЕ · ":"TUTORIAL · ")+(_tutorialStep+1)+"/6",Title=T(NativeTutorialTitles[_tutorialStep]),Body=T(NativeTutorialBodies[_tutorialStep]),Status=status,Skip=T("tutorialSkip"),Next=T("tutorialNext")};
        }
        private void NextNativeTutorial(){if(!NativeTutorialRequested||!CanAdvanceNativeTutorial()||_tutorialStep==4)return;if(_tutorialStep==5)FinishTutorial(false);else SetTutorialStep(_tutorialStep+1);RefreshNativeCommandModels();_sourceUi.Dirty=true;}
        private void SkipNativeTutorial(){if(!NativeTutorialRequested)return;FinishTutorial(true);RefreshNativeCommandModels();_sourceUi.Dirty=true;}

    }
}
