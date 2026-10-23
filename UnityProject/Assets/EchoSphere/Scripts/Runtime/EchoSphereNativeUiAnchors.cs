using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRunUiAnchors _nativeRunUiAnchors;
        private NativeMobileLayout _nativeMobileLayout;
        private VisualElement _nativeRunField;
        // The only legacy layout adapter. Preserve original first/last DFS match
        // rules and existing frame Content hosts rather than choosing new layout.
        private NativeRunUiAnchors CaptureLegacyRunUiAnchors(VisualCssNode root)
        {
            if(_nativeRunUiAnchors==null)_nativeRunUiAnchors=new NativeRunUiAnchors();var a=_nativeRunUiAnchors;a.Clear();a.ReadVersion=ReadLegacyUiVersion;a.Backdrops=_sourceUi;a.Invalidate=InvalidateNativeUiAnchors;a.TutorialTarget=ReadLegacyTutorialTarget;
            void Bind(NativeRunUiAnchors.Slot slot,VisualCssNode node)=>slot.Bind(node.Native as VisualElement,(node.Native as SourceUiFrame)?.Content,node.Children.Select(c=>c.Native as VisualElement));
            VisualElement gameHost=null,controlsAfter=null;VisualCssNode gameNode=null;_nativeRunField=null;
            void Visit(VisualCssNode n,bool pause)
            {
                if(n.HasClass("es-game-screen")){gameNode=n;gameHost=(n.Native as SourceUiFrame)?.Content;}
                if(n.Tag=="canvas")_nativeRunField=(n.Native as SourceUiFrame)?.Content;
                pause|=n.HasClass("es-pause-overlay");
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
                foreach(var child in n.Children)Visit(child,pause);
            }
            Visit(root,false);
            if(_nativeMobilePresent&&gameHost!=null){
                if(_nativeMobileLayout==null||_nativeMobileLayout.Root.parent!=gameHost){
                    DetachNativeMobileGestures();_nativeMobileLayout?.Dispose();_nativeMobileLayout=new NativeMobileLayout(gameHost);
                }
                // Preserve z-20 against every authored sibling, including the z-4
                // vignette pseudo-element. Placing directly after canvas would
                // incorrectly multiply that vignette over the controls.
                int last=-1;
                foreach(var child in gameNode.Children){
                    if(!(child.Native is VisualElement element)||element.parent!=gameHost)continue;
                    child.Style.TryGetValue("z-index",out var text);
                    double.TryParse(text,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var z);
                    int index=gameHost.IndexOf(element);if(z<=20&&index>last){last=index;controlsAfter=element;}
                }
                if(controlsAfter!=null)_nativeMobileLayout.Root.PlaceInFront(controlsAfter);
                _nativeMobileLayout.Bind(a,IsLeftHanded,(float)Visual_interfaceScale.Scope["loadInterfaceScale"].Call().Number,
                    Screen.width/SourceDevicePixelRatio,_tutorialStep==1,
                    _nativeJoystickState==null?(Vector2?)null:new Vector2((float)_nativeMobileJoystickX,(float)_nativeMobileJoystickY),
                    _nativePlacementState==null?(Vector2?)null:_nativeMobilePlacement,
                    _nativeGhostNetworkState!=null,_nativeFormationMemoryState!=null,SourceDevicePixelRatio);
            }else{DetachNativeMobileGestures();_nativeMobileLayout?.Dispose();_nativeMobileLayout=null;}
            return a;
        }
        private int ReadLegacyUiVersion()=>_sourceUi.FrameVersion;
        private void InvalidateNativeUiAnchors(){if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private Rect? ReadLegacyTutorialTarget(string target)
        {
            if(target=="movement"&&_nativeMobileLayout!=null)return _nativeMobileLayout.MovementBounds;
            var node=_sourceUi.Query("[data-tutorial-target=\""+target+"\"]");if(node.Null||node.Undefined)return null;var rect=node["getBoundingClientRect"].Call();return new Rect((float)rect["left"].Number,(float)rect["top"].Number,(float)rect["width"].Number,(float)rect["height"].Number);
        }
    }
}
