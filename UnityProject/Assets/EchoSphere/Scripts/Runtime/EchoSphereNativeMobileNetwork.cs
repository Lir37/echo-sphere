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
        private static bool NativeNetworkOverlay(string tag,V props,bool memory)=>tag=="svg"&&NativeClasses(props["className"].Text,"absolute","inset-0","pointer-events-none",memory?"z-9":"z-10");
        private void CaptureNativeMobileNetwork(VisualDom.Node mobile)
        {
            _nativeGhostNetworkState=_nativeFormationMemoryState=null;if(mobile==null)return;
            foreach(var value in mobile.Children){if(!(value.Raw is VisualDom.Node overlay))continue;bool memory=NativeNetworkOverlay(overlay.Tag,overlay.Properties,true);if(!memory&&!NativeNetworkOverlay(overlay.Tag,overlay.Properties,false))continue;
                var state=new NativeMobileNetworkView.State {Memory=memory};
                Vector2 Point(V p,string x,string y)=>new Vector2((float)p[x].Number,(float)p[y].Number);
                foreach(var child in overlay.Children){if(!(child.Raw is VisualDom.Node node))continue;var p=node.Properties;
                    if(node.Tag=="polyline"){
                        // Only the explicit display-point list from the original
                        // ghost/memory component; no runtime SVG path parser.
                        state.Points=p["points"].Text.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries).Select(pair=>{var xy=pair.Split(',');return new Vector2(float.Parse(xy[0],CultureInfo.InvariantCulture),float.Parse(xy[1],CultureInfo.InvariantCulture));}).ToArray();
                        if(memory)state.Alpha=(float)p["opacity"].Number;else state.FormationWidth=(float)p["strokeWidth"].Number;
                    }else if(node.Tag=="line"){
                        if(memory)state.Closed=true;else state.Links.Add(new NativeMobileNetworkView.Link(Point(p,"x1","y1"),Point(p,"x2","y2"),(float)p["strokeWidth"].Number));
                    }else if(node.Tag=="circle"&&!memory){state.Center=Point(p,"cx","cy");state.Radius=(float)p["r"].Number;state.RingWidth=(float)p["strokeWidth"].Number;ColorUtility.TryParseHtmlString(p["stroke"].Text,out state.SphereColor);}
                    else if(node.Tag=="text"){state.Caption=string.Concat(node.Children.Select(c=>c.Text));state.CaptionPosition=Point(p,"x","y");state.FontSize=(float)p["fontSize"].Number;state.Valid=p["fill"].Text=="#dcecff";}
                }
                if(memory)_nativeFormationMemoryState=state;else _nativeGhostNetworkState=state;
            }
        }
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
        private V ProjectNativeMobileNetwork(V tree)
        {
            bool ghost=_nativeGhostNetworkState!=null&&_nativeGhostNetwork!=null&&_nativeGhostNetwork.Ready,memory=_nativeFormationMemoryState!=null&&_nativeFormationMemory!=null&&_nativeFormationMemory.Ready;
            void Visit(V v,bool mobile){if(v.Raw is VisualDom.Node n){mobile|=NativeClasses(n.Properties["className"].Text,"es-mobile-controls");if(mobile&&(ghost&&NativeNetworkOverlay(n.Tag,n.Properties,false)||memory&&NativeNetworkOverlay(n.Tag,n.Properties,true))){n.Children.Clear();return;}foreach(var c in n.Children)Visit(c,mobile);}else if(v.Raw is System.Collections.Generic.List<V>)foreach(var c in v.Items)Visit(c,mobile);}if(ghost||memory)Visit(tree,false);return tree;
        }
    }
}
