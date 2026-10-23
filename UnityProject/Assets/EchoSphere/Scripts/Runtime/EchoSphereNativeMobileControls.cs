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


        private void DisposeNativeMobileDock(){_nativeMobileDock?.Dispose();_nativeMobileDock=null;}

        private void BindNativeMobileInput(Button button,string id)
        {
            button.AddToClassList("native-mobile-control");
            button.RegisterCallback<PointerDownEvent>(evt=>{evt.StopPropagation();if(button.enabledInHierarchy)NativeMobileControl(id);});
        }
        private void PrepareNativeMobileDock(NativeRunUiAnchors anchors)
        {
            if(_nativeMobileDockState==null){DisposeNativeMobileDock();return;}
            var slot=anchors.Dock;var host=slot.Content;if(host==null){DisposeNativeMobileDock();return;}
            if(_nativeMobileDock==null||_nativeMobileDock.Root.parent!=host){DisposeNativeMobileDock();_nativeMobileDock=new NativeMobileControlsView(host,BindNativeMobileInput);}
            _nativeMobileDock.Bind(_nativeMobileDockState);bool ready=_nativeMobileDock.Ready;_nativeMobileDock.Prepare(SourceDevicePixelRatio);
            if(_nativeMobileDock.Ready){slot.HideLegacy(true);if(!ready)anchors.Invalidate?.Invoke();}
        }
    }
}
