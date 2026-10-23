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

        private void DisposeNativeMobileNetwork(){_nativeGhostNetwork?.Dispose();_nativeGhostNetwork=null;_nativeFormationMemory?.Dispose();_nativeFormationMemory=null;}
        private void PrepareNativeMobileNetwork(NativeRunUiAnchors anchors)
        {
            void Prepare(NativeRunUiAnchors.Slot slot,NativeMobileNetworkView.State state,ref NativeMobileNetworkView view){
                if(state==null||slot.Content==null){view?.Dispose();view=null;return;}
                if(view==null||view.Root.parent!=slot.Content){view?.Dispose();view=new NativeMobileNetworkView(slot.Content);}
                view.Bind(state);bool ready=view.Ready;view.Prepare();if(view.Ready){foreach(var child in slot.Content.Children())if(child!=view.Root)child.style.display=DisplayStyle.None;if(!ready)anchors.Invalidate?.Invoke();}
            }
            Prepare(anchors.Ghost,_nativeGhostNetworkState,ref _nativeGhostNetwork);Prepare(anchors.Memory,_nativeFormationMemoryState,ref _nativeFormationMemory);
        }
    }
}
