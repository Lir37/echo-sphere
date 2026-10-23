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
        // Temporary legacy tree adapter only. Native presenters consume typed
        // anchors; keep source placeholder/pruning policy out of their owners.
        private V ProjectNativeHudIsland(V tree)
        {
            if(!UsesNativeHud)return tree;
            void Visit(V value)
            {
                if(value.Raw is VisualDom.Node node){
                    var classes=node.Properties["className"].Text??"";
                    if(Array.IndexOf(classes.Split(' '),"es-hud-panel")>=0){
                        // Keep the authored box and tutorial/query anchor while
                        // the retained native island owns its pixels and input.
                        node.Children.Clear();
                        node.Properties["style"]=VisualJs.Merge(node.Properties["style"],VisualJs.Obj("visibility","hidden","pointerEvents","none"));
                        return;
                    }
                    foreach(var child in node.Children)Visit(child);
                }else if(value.Raw is System.Collections.Generic.List<V>)foreach(var child in value.Items)Visit(child);
            }
            Visit(tree);return tree;
        }
        private V ProjectNativePauseScreen(V tree)
        {
            if (!UsesNativePauseScreen) return tree;
            void Visit(V value, bool pause)
            {
                if (value.Raw is VisualDom.Node node)
                {
                    var classes = (node.Properties["className"].Text ?? "").Split(' ');
                    pause |= Array.IndexOf(classes, "es-pause-overlay") >= 0;
                    if (pause && Array.IndexOf(classes, "es-modal-shell") >= 0)
                    {
                        // Retain the original detail modal and shell artwork.
                        node.Children.RemoveAll(child => {
                            if (!(child.Raw is VisualDom.Node element)) return false;
                            var tokens = (element.Properties["className"].Text ?? "").Split(' ');
                            bool Has(string token) => Array.IndexOf(tokens, token) >= 0;
                            return Has("px-4") && ((Has("pt-4") && Has("pb-3") && Has("border-b")) ||
                                (Has("overflow-y-auto") && Has("py-4") && Has("min-h-0")) || (Has("py-3") && Has("border-t")));
                        });
                        return;
                    }
                    foreach (var child in node.Children) Visit(child, pause);
                }
                else if (value.Raw is System.Collections.Generic.List<V>) foreach (var child in value.Items) Visit(child, pause);
            }
            Visit(tree, false); return tree;
        }
        private V ProjectNativeRunResult(V tree)
        {
            if(!_gameOver||_nativeRunResult==null||!_nativeRunResult.Ready)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var classes=(node.Properties["className"].Text??"").Split(' ');bool Has(string c)=>System.Array.IndexOf(classes,c)>=0;if(Has("bg-black/80")&&Has("absolute")&&Has("inset-0")&&Has("z-50")){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private V ProjectNativeUpgrade(V tree)
        {
            if(!NativeUpgradeRequested||_nativeUpgradeView==null||!_nativeUpgradeView.Ready)return tree;bool found=false;
            void Visit(V value){if(found)return;if(value.Raw is VisualDom.Node node){var classes=(node.Properties["className"].Text??"").Split(' ');bool Has(string c)=>Array.IndexOf(classes,c)>=0;if(Has("bg-black/70")&&Has("absolute")&&Has("inset-0")&&Has("z-50")){node.Children.Clear();found=true;return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private V ProjectNativeArtifactRewards(V tree)
        {
            bool reward=NativeArtifactRewardRequested&&_nativeArtifactRewards!=null&&_nativeArtifactRewards.Ready,pause=_nativePauseSetDetail!=null&&_nativePauseSetDetail.Ready;
            if(!reward&&!pause)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var tokens=(node.Properties["className"].Text??"").Split(' ');bool Has(string s)=>Array.IndexOf(tokens,s)>=0;if(reward&&Has("bg-black/70")&&Has("backdrop-blur-sm")&&Has("z-50")||pause&&Has("z-[70]")&&Has("bg-black/75")){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private V ProjectNativeTutorial(V tree)
        {
            if(!NativeTutorialRequested||_nativeTutorial==null||!_nativeTutorial.Ready)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var tokens=(node.Properties["className"].Text??"").Split(' ');if(Array.IndexOf(tokens,"z-[80]")>=0&&Array.IndexOf(tokens,"pointer-events-none")>=0){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private V BuildNativeSourceUiTree()
        {
            _nativeMobilePresent=false;var tree=SourceUiTree();
            if(!_nativeMobilePresent){if(_nativeMobileMounted)ResetNativeMobileGestures(true);_nativeMobileDockState=null;_nativeJoystickState=_nativePlacementState=null;_nativeGhostNetworkState=_nativeFormationMemoryState=null;}
            return ProjectNativeTutorial(ProjectNativeArtifactRewards(ProjectNativeUpgrade(ProjectNativeRunResult(ProjectNativePauseScreen(ProjectNativeHudIsland(tree))))));
        }
        private V BuildNativeMobileComponent(V[] args)
        {
            _nativeMobilePresent=true;
            if(!_nativeMobileMounted){_nativeMobileMounted=true;_nativeMobileHandedness=IsLeftHanded;NativeMobileClearKeys();}
            else if(_nativeMobileHandedness!=IsLeftHanded){ResetNativeMobileGestures(false);_nativeMobileHandedness=IsLeftHanded;}
            ReadNativeMobileState();var children=new List<V>();
            V Box(string css,V style,params V[] content)=>VisualDom.Element("div",VisualJs.Obj("className",css,"style",style),content);
            double scale=Visual_interfaceScale.Scope["loadInterfaceScale"].Call().Number;
            V FeedbackStyle(double left,double top,int size)=>VisualJs.Obj("left",left,"top",top,"width",size,"height",size,"transform","scale("+VisualJs.NumberText(scale)+")","transformOrigin","center");
            if(_tutorialStep==1)children.Add(VisualDom.Element("div",VisualJs.Obj("data-tutorial-target","movement","className","absolute pointer-events-none z-30 rounded-full border border-[#63e6ff]/20","style",VisualJs.Obj("width",116,"height",116,"left",IsLeftHanded?"calc(25% - 58px)":"calc(75% - 58px)","bottom",44))));
            if(_nativeGhostNetworkState!=null)children.Add(Box("absolute inset-0 pointer-events-none z-10 overflow-visible",VisualJs.Obj()));
            if(_nativeFormationMemoryState!=null)children.Add(Box("absolute inset-0 pointer-events-none z-9 overflow-visible",VisualJs.Obj()));
            if(_nativePlacementState!=null)children.Add(VisualDom.Element("div",VisualJs.Obj("key",_nativeMobilePlacementAt,"className","absolute pointer-events-none","style",FeedbackStyle(_nativeMobilePlacement.x-26,_nativeMobilePlacement.y-26,52))));
            if(_nativeJoystickState!=null)children.Add(Box("absolute pointer-events-none",FeedbackStyle(_nativeMobileJoystickX-58,_nativeMobileJoystickY-58,116)));
            children.Add(Box("absolute bottom-4 "+(IsLeftHanded?"right-3 flex flex-col items-end":"left-3 flex flex-col items-start")+" gap-2 pointer-events-none",VisualJs.Obj("transform","scale("+VisualJs.NumberText(scale)+")","transformOrigin",IsLeftHanded?"right bottom":"left bottom")));
            return VisualDom.Element("div",VisualJs.Obj("className","es-mobile-controls absolute inset-0 z-20 overflow-hidden select-none","style",VisualJs.Obj("touchAction","none","userSelect","none","WebkitUserSelect","none")),VisualJs.Arr(children.ToArray()));
        }
        private static bool NativeDockClass(string text)=>NativeClasses(text,"bottom-4","flex-col","pointer-events-none");
        private static bool NativeClasses(string text,params string[] required){var tokens=(text??"").Split(' ');return required.All(t=>Array.IndexOf(tokens,t)>=0);}
        private static bool NativeFeedbackBox(V props,float size)=>NativeClasses(props["className"].Text,"absolute","pointer-events-none")&&props["style"]["width"].Number==size&&props["style"]["height"].Number==size;
        private static bool NativeNetworkOverlay(string tag,V props,bool memory)=>tag=="div"&&NativeClasses(props["className"].Text,"absolute","inset-0","pointer-events-none",memory?"z-9":"z-10");
    }
}
