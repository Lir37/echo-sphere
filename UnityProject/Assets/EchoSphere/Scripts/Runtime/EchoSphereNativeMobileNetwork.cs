using System;
using System.Globalization;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeMobileNetworkView _nativeGhostNetwork,_nativeFormationMemory;
        private NativeMobileNetworkView.State _nativeGhostNetworkState,_nativeFormationMemoryState;
        private static bool NativeNetworkOverlay(string tag,V props,bool memory)=>tag=="div"&&NativeClasses(props["className"].Text,"absolute","inset-0","pointer-events-none",memory?"z-9":"z-10");
        private void DisposeNativeMobileNetwork(){_nativeGhostNetwork?.Dispose();_nativeGhostNetwork=null;_nativeFormationMemory?.Dispose();_nativeFormationMemory=null;}
        private void PrepareNativeMobileNetwork(VisualCssNode root)
        {
            VisualCssNode ghost=null,memory=null;void Find(VisualCssNode n,bool mobile){mobile|=n.HasClass("es-mobile-controls");if(mobile){if(NativeNetworkOverlay(n.Tag,n.Properties,false))ghost=n;if(NativeNetworkOverlay(n.Tag,n.Properties,true))memory=n;}foreach(var c in n.Children)Find(c,mobile);}Find(root,false);
            void Prepare(VisualCssNode node,NativeMobileNetworkView.State state,ref NativeMobileNetworkView view){
                if(state==null||!(node?.Native is SourceUiFrame frame)){view?.Dispose();view=null;return;}
                if(view==null||view.Root.parent!=frame.Content){view?.Dispose();view=new NativeMobileNetworkView(frame.Content);}
                view.Bind(state);bool ready=view.Ready;view.Prepare();if(view.Ready){foreach(var child in frame.Content.Children())if(child!=view.Root)child.style.display=DisplayStyle.None;if(!ready)_sourceUi.Dirty=true;}
            }
            Prepare(ghost,_nativeGhostNetworkState,ref _nativeGhostNetwork);Prepare(memory,_nativeFormationMemoryState,ref _nativeFormationMemory);
        }
    }
}
