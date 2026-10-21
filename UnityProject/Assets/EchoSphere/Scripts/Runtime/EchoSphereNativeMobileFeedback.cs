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
        private void CaptureNativeMobileFeedback(VisualDom.Node mobile)
        {
            _nativeJoystickState=_nativePlacementState=null;if(mobile==null)return;
            foreach(var child in mobile.Children){if(!(child.Raw is VisualDom.Node n))continue;
                if(NativeFeedbackBox(n.Properties,116)){
                    foreach(var c in n.Children)if(c.Raw is VisualDom.Node knob&&NativeClasses(knob.Properties["className"].Text,"bg-white/25"))_nativeJoystickState=new NativeMobileFeedbackView.State {Joystick=true,KnobX=(float)knob.Properties["style"]["left"].Number,KnobY=(float)knob.Properties["style"]["top"].Number,Active=NativeClasses(knob.Properties["className"].Text,"opacity-90")};
                }else if(NativeFeedbackBox(n.Properties,52))foreach(var c in n.Children)if(c.Raw is VisualDom.Node ring&&NativeClasses(ring.Properties["className"].Text,"animate-ping"))_nativePlacementState=new NativeMobileFeedbackView.State {Id=n.Properties["key"].Text,Color=ring.Properties["style"]["borderColor"].Text};
            }
        }
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
        private V ProjectNativeMobileFeedback(V tree)
        {
            bool joystick=_nativeJoystickState!=null&&_nativeJoystickFeedback!=null&&_nativeJoystickFeedback.Ready,placement=_nativePlacementState!=null&&_nativePlacementFeedback!=null&&_nativePlacementFeedback.Ready;if(!joystick&&!placement)return tree;
            void Visit(V v,bool mobile){if(v.Raw is VisualDom.Node n){mobile|=NativeClasses(n.Properties["className"].Text,"es-mobile-controls");if(mobile&&(joystick&&NativeFeedbackBox(n.Properties,116)&&n.Properties["data-tutorial-target"].Text!="movement"||placement&&NativeFeedbackBox(n.Properties,52))){n.Children.Clear();return;}foreach(var c in n.Children)Visit(c,mobile);}else if(v.Raw is List<V>)foreach(var c in v.Items)Visit(c,mobile);}Visit(tree,false);return tree;
        }
    }
}
