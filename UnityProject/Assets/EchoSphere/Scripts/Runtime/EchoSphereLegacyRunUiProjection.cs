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
            ReadNativeMobileState();
            // The retained native layout now owns all mobile host geometry.
            return VisualJs.Null;
        }

    }
}
