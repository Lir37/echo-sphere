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
        private static bool NativeFeedbackBox(V props,float size)=>NativeClasses(props["className"].Text,"absolute","pointer-events-none")&&props["style"]["width"].Number==size&&props["style"]["height"].Number==size;
        private void DisposeNativeMobileFeedback(){_nativeJoystickFeedback?.Dispose();_nativeJoystickFeedback=null;_nativePlacementFeedback?.Dispose();_nativePlacementFeedback=null;}
        private void PrepareNativeMobileFeedback(VisualCssNode root)
        {
            VisualCssNode joystick=null,placement=null;void Find(VisualCssNode n,bool mobile){mobile|=n.HasClass("es-mobile-controls");if(mobile){if(NativeFeedbackBox(n.Properties,116)&&n.Attribute("data-tutorial-target")!="movement")joystick=n;if(NativeFeedbackBox(n.Properties,52))placement=n;}foreach(var c in n.Children)Find(c,mobile);}Find(root,false);
            void Prepare(VisualCssNode node,NativeMobileFeedbackView.State state,ref NativeMobileFeedbackView view){
                if(state==null||!(node?.Native is SourceUiFrame frame)){view?.Dispose();view=null;return;}
                if(view==null||view.Root.parent!=frame.Content){view?.Dispose();view=new NativeMobileFeedbackView(frame.Content,_sourceUi,state.Joystick);}
                view.Bind(state);bool ready=view.Ready;view.Prepare(SourceDevicePixelRatio);if(view.Ready){foreach(var c in node.Children)if(c.Native is VisualElement e)e.style.display=DisplayStyle.None;if(!ready)_sourceUi.Dirty=true;}
            }
            Prepare(joystick,_nativeJoystickState,ref _nativeJoystickFeedback);Prepare(placement,_nativePlacementState,ref _nativePlacementFeedback);
        }
    }
}
