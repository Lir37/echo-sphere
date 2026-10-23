using System;
using System.Collections.Generic;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeMobileFeedbackView _nativeJoystickFeedback,_nativePlacementFeedback;
        private NativeMobileFeedbackView.State _nativeJoystickState,_nativePlacementState;

        private void DisposeNativeMobileFeedback(){_nativeJoystickFeedback?.Dispose();_nativeJoystickFeedback=null;_nativePlacementFeedback?.Dispose();_nativePlacementFeedback=null;}
        private void PrepareNativeMobileFeedback(NativeRunUiAnchors anchors)
        {
            void Prepare(NativeRunUiAnchors.Slot slot,NativeMobileFeedbackView.State state,ref NativeMobileFeedbackView view){
                if(state==null||slot.Content==null){view?.Dispose();view=null;return;}
                if(view==null||view.Root.parent!=slot.Content){view?.Dispose();view=new NativeMobileFeedbackView(slot.Content,anchors.Backdrops,state.Joystick);}
                view.Bind(state);bool ready=view.Ready;view.Prepare(SourceDevicePixelRatio);if(view.Ready){slot.HideLegacy(true);if(!ready)anchors.Invalidate?.Invoke();}
            }
            Prepare(anchors.Joystick,_nativeJoystickState,ref _nativeJoystickFeedback);Prepare(anchors.Placement,_nativePlacementState,ref _nativePlacementFeedback);
        }
    }
}
