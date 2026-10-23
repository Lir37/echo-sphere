using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRunUiAnchors _nativeRunUiAnchors;
        // The only legacy layout adapter. Preserve original first/last DFS match
        // rules and existing frame Content hosts rather than choosing new layout.
        private NativeRunUiAnchors CaptureLegacyRunUiAnchors(VisualCssNode root)
        {
            if(_nativeRunUiAnchors==null)_nativeRunUiAnchors=new NativeRunUiAnchors();var a=_nativeRunUiAnchors;a.Clear();a.ReadVersion=ReadLegacyUiVersion;a.Backdrops=_sourceUi;a.Invalidate=InvalidateNativeUiAnchors;a.TutorialTarget=ReadLegacyTutorialTarget;
            void Bind(NativeRunUiAnchors.Slot slot,VisualCssNode node)=>slot.Bind(node.Native as VisualElement,(node.Native as SourceUiFrame)?.Content,node.Children.Select(c=>c.Native as VisualElement));
            void Visit(VisualCssNode n,bool pause,bool mobile)
            {
                pause|=n.HasClass("es-pause-overlay");mobile|=n.HasClass("es-mobile-controls");
                if(n.HasClass("es-hud-panel")){if(n.HasClass("es-top-left"))Bind(a.HudLeft,n);else if(n.HasClass("es-top-right"))Bind(a.HudRight,n);}
                if(pause){
                    if(!a.PauseShell.IsBound&&n.HasClass("es-modal-shell"))Bind(a.PauseShell,n);
                    if(n.HasClass("px-4")&&n.HasClass("pt-4")&&n.HasClass("pb-3")&&n.HasClass("border-b"))Bind(a.PauseHeader,n);
                    if(n.HasClass("px-4")&&n.HasClass("py-3")&&n.HasClass("border-t"))Bind(a.PauseFooter,n);
                    if(!a.PauseBody.IsBound&&n.HasClass("overflow-y-auto")&&n.HasClass("px-4")&&n.HasClass("py-4")&&n.HasClass("min-h-0"))Bind(a.PauseBody,n);
                    if(!a.PauseDetail.IsBound&&n.HasClass("z-[70]")&&n.HasClass("bg-black/75"))Bind(a.PauseDetail,n);
                }
                if(!a.Result.IsBound&&n.HasClass("bg-black/80")&&n.HasClass("absolute")&&n.HasClass("inset-0")&&n.HasClass("z-50"))Bind(a.Result,n);
                if(!a.Upgrade.IsBound&&n.HasClass("bg-black/70")&&n.HasClass("absolute")&&n.HasClass("inset-0")&&n.HasClass("z-50"))Bind(a.Upgrade,n);
                if(!a.Rewards.IsBound&&n.HasClass("bg-black/70")&&n.HasClass("backdrop-blur-sm")&&n.HasClass("z-50"))Bind(a.Rewards,n);
                if(!a.Tutorial.IsBound&&n.HasClass("z-[80]")&&n.HasClass("pointer-events-none"))Bind(a.Tutorial,n);
                if(n.HasClass("es-mobile-controls"))Bind(a.Mobile,n);
                // Original dock search is global; other mobile slots are scoped.
                if(!a.Dock.IsBound&&NativeDockClass(n.Attribute("className")))Bind(a.Dock,n);
                if(mobile){
                    if(NativeFeedbackBox(n.Properties,116)&&n.Attribute("data-tutorial-target")!="movement")Bind(a.Joystick,n);
                    if(NativeFeedbackBox(n.Properties,52))Bind(a.Placement,n);
                    if(NativeNetworkOverlay(n.Tag,n.Properties,false))Bind(a.Ghost,n);
                    if(NativeNetworkOverlay(n.Tag,n.Properties,true))Bind(a.Memory,n);
                }
                foreach(var child in n.Children)Visit(child,pause,mobile);
            }
            Visit(root,false,false);return a;
        }
        private int ReadLegacyUiVersion()=>_sourceUi.FrameVersion;
        private void InvalidateNativeUiAnchors(){if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private Rect? ReadLegacyTutorialTarget(string target)
        {
            var node=_sourceUi.Query("[data-tutorial-target=\""+target+"\"]");if(node.Null||node.Undefined)return null;var rect=node["getBoundingClientRect"].Call();return new Rect((float)rect["left"].Number,(float)rect["top"].Number,(float)rect["width"].Number,(float)rect["height"].Number);
        }
    }
}
