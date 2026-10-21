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
        private void PrepareNativeTutorial(VisualCssNode root)
        {
            if(!NativeTutorialRequested){DisposeNativeTutorial();return;}
            VisualCssNode overlay=null;void Find(VisualCssNode node){if(overlay==null&&node.HasClass("z-[80]")&&node.HasClass("pointer-events-none"))overlay=node;foreach(var child in node.Children)Find(child);}Find(root);
            if(!(overlay?.Native is SourceUiFrame frame)){DisposeNativeTutorial();return;}
            if(_nativeTutorial==null||_nativeTutorial.Root.parent!=frame.Content){DisposeNativeTutorial();_nativeTutorial=new NativeTutorialView(frame.Content,_sourceUi,NextNativeTutorial,SkipNativeTutorial);}
            _nativeTutorial.Bind(ReadNativeTutorialState());
            string key=_tutorialStep+"/"+IsLeftHanded+"/"+Screen.width+"/"+Screen.height;double now=Time.unscaledTimeAsDouble;
            if(key!=_nativeTutorialTargetKey||now>=_nativeTutorialTargetAt){
                _nativeTutorialTargetKey=key;_nativeTutorialTargetAt=now+.120;_nativeTutorialTarget=null;string target=NativeTutorialTargets[_tutorialStep];
                if(target!=null){var node=_sourceUi.Query("[data-tutorial-target=\""+target+"\"]");if(!node.Null&&!node.Undefined){var rect=node["getBoundingClientRect"].Call();_nativeTutorialTarget=new Rect((float)rect["left"].Number,(float)rect["top"].Number,(float)rect["width"].Number,(float)rect["height"].Number);}}
            }
            bool ready=_nativeTutorial.Ready;float scale=SourceDevicePixelRatio;_nativeTutorial.Prepare(Screen.width/scale,Screen.height/scale,scale,_nativeTutorialTarget);
            if(_nativeTutorial.Ready){foreach(var child in overlay.Children)if(child.Native is VisualElement e)e.style.visibility=Visibility.Hidden;if(!ready)_sourceUi.Dirty=true;}
        }
        private bool CanAdvanceNativeTutorial()
        {
            if(_tutorialStep==0||_tutorialStep==3||_tutorialStep==5)return true;
            if(_tutorialStep==2)return _sourceRenderState["spheres"]["length"].Number>=2;
            if(_tutorialStep!=1)return false;
            var player=_sourceRenderState["player"]["pos"];var start=SourcePoint((Vector2)_tutorialStartPosition);double dx=player["x"].Number-start["x"].Number,dy=player["y"].Number-start["y"].Number;return Math.Sqrt(dx*dx+dy*dy)>=72;
        }
        private NativeTutorialView.State ReadNativeTutorialState()
        {
            string lang=IsRussianUi?"ru":"en";string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;bool advance=CanAdvanceNativeTutorial(),action=_tutorialStep==1||_tutorialStep==2;
            string status=_tutorialStep==4?(IsRussianUi?"Выбери одну карту. Обучение продолжится после выбора.":"Choose one card. The tutorial continues after your choice."):advance&&action?T(_tutorialStep==1?"tutorialMoveDone":"tutorialPlaceDone"):(IsRussianUi?"Выполни действие, чтобы продолжить.":"Perform the action to continue.");
            return new NativeTutorialView.State {Step=_tutorialStep,CanAdvance=advance,ShowAction=action,Kicker=(IsRussianUi?"ОБУЧЕНИЕ · ":"TUTORIAL · ")+(_tutorialStep+1)+"/6",Title=T(NativeTutorialTitles[_tutorialStep]),Body=T(NativeTutorialBodies[_tutorialStep]),Status=status,Skip=T("tutorialSkip"),Next=T("tutorialNext")};
        }
        private void NextNativeTutorial(){if(!NativeTutorialRequested||!CanAdvanceNativeTutorial()||_tutorialStep==4)return;if(_tutorialStep==5)FinishTutorial(false);else SetTutorialStep(_tutorialStep+1);ApplySourceUiWrites();_sourceUi.Dirty=true;}
        private void SkipNativeTutorial(){if(!NativeTutorialRequested)return;FinishTutorial(true);ApplySourceUiWrites();_sourceUi.Dirty=true;}
        private V ProjectNativeTutorial(V tree)
        {
            if(!NativeTutorialRequested||_nativeTutorial==null||!_nativeTutorial.Ready)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var tokens=(node.Properties["className"].Text??"").Split(' ');if(Array.IndexOf(tokens,"z-[80]")>=0&&Array.IndexOf(tokens,"pointer-events-none")>=0){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
    }
}
