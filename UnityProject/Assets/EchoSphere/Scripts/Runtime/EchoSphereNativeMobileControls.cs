using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeMobileControlsView _nativeMobileDock;
        private NativeMobileControlsView.State _nativeMobileDockState;
        private static bool NativeDockClass(string text)=>NativeClasses(text,"bottom-4","flex-col","pointer-events-none");
        private static bool NativeClasses(string text,params string[] required){var tokens=(text??"").Split(' ');return required.All(t=>Array.IndexOf(tokens,t)>=0);}
        private void DisposeNativeMobileDock(){_nativeMobileDock?.Dispose();_nativeMobileDock=null;}
        private V BuildNativeSourceUiTree()
        {
            _nativeMobilePresent=false;var tree=SourceUiTree();
            if(!_nativeMobilePresent){if(_nativeMobileMounted)ResetNativeMobileGestures(true);_nativeMobileDockState=null;_nativeJoystickState=_nativePlacementState=null;_nativeGhostNetworkState=_nativeFormationMemoryState=null;}
            return ProjectNativeTutorial(ProjectNativeArtifactRewards(ProjectNativeUpgrade(ProjectNativeRunResult(ProjectNativePauseScreen(ProjectNativeHudIsland(tree))))));
        }
        private void BindNativeMobileInput(Button button,string id)
        {
            button.AddToClassList("native-mobile-control");
            button.RegisterCallback<PointerDownEvent>(evt=>{evt.StopPropagation();if(button.enabledInHierarchy)NativeMobileControl(id);});
        }
        private void PrepareNativeMobileDock(VisualCssNode root)
        {
            if(_nativeMobileDockState==null){DisposeNativeMobileDock();return;}
            VisualCssNode dock=null;void Find(VisualCssNode n){if(dock==null&&NativeDockClass(n.Attribute("className")))dock=n;foreach(var c in n.Children)Find(c);}Find(root);
            if(!(dock?.Native is SourceUiFrame frame)){DisposeNativeMobileDock();return;}
            if(_nativeMobileDock==null||_nativeMobileDock.Root.parent!=frame.Content){DisposeNativeMobileDock();_nativeMobileDock=new NativeMobileControlsView(frame.Content,BindNativeMobileInput);}
            _nativeMobileDock.Bind(_nativeMobileDockState);bool ready=_nativeMobileDock.Ready;_nativeMobileDock.Prepare(SourceDevicePixelRatio);
            if(_nativeMobileDock.Ready){foreach(var c in dock.Children)if(c.Native is VisualElement e)e.style.display=DisplayStyle.None;if(!ready)_sourceUi.Dirty=true;}
        }
    }
}
