using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldNetwork _nativeWorldNetwork;
        private static NativeWorldNetwork.Node NativeNetworkPoint(V position)=>new NativeWorldNetwork.Node {X=position["x"].Number,Y=position["y"].Number};
        private void DrawNativeNetwork(V scene)
        {
            var spheres=scene["spheres"].Items.Select(s=>new NativeWorldNetwork.Node {X=s["pos"]["x"].Number,Y=s["pos"]["y"].Number,Charge=s["formationHitCount"].Number,Pulse=s["resonancePulseTimer"].Number}).ToArray();
            var positions=scene["spheres"].Items.Select(s=>s["pos"]).ToList();if(scene["player"]["abilities"]["minion"].Number>=3)positions.AddRange(scene["minions"].Items.Select(minion=>minion["pos"]));
            var projected=ProjectNativeNetworkState(_networkState,spheres);
            _nativeWorldNetwork.Draw(_worldSurface,projected.Network,positions.Select(position=>position.Truth?(NativeWorldNetwork.Node?)NativeNetworkPoint(position):null).ToArray(),projected.ExternalLinks,scene["time"].Number);
        }
        internal static (NativeWorldNetwork.Network Network,NativeWorldNetwork.Link[] ExternalLinks) ProjectNativeNetworkState(SphereNetworkState state,NativeWorldNetwork.Node[] spheres)
        {
            state=state??new SphereNetworkState();int count=spheres.Length;
            bool SphereIndex(int index)=>index>=0&&index<count;
            bool SphereLink(SphereNetworkLink link)=>SphereIndex(link.A)&&SphereIndex(link.B);
            NativeWorldNetwork.Link Link(SphereNetworkLink link)=>new NativeWorldNetwork.Link {A=link.A,B=link.B};
            NativeWorldNetwork.Formation Formation(SphereNetworkFormation kind){var shape=state.GetFormation(kind);if(shape==null)return null;var indexes=shape.NodeIndexes.Where(SphereIndex).ToArray();return indexes.Length>=2?new NativeWorldNetwork.Formation {Nodes=indexes,Active=shape.Active}:null;}
            return (new NativeWorldNetwork.Network {NodeCount=state.Nodes.Count(SphereIndex),Spheres=spheres,Links=state.Links.Where(SphereLink).Select(Link).ToArray(),Line=Formation(SphereNetworkFormation.Line),Triangle=Formation(SphereNetworkFormation.Triangle),Square=Formation(SphereNetworkFormation.Square),Cluster=Formation(SphereNetworkFormation.Cluster),Ring=Formation(SphereNetworkFormation.Ring),Lattice=Formation(SphereNetworkFormation.Lattice),Fractal=Formation(SphereNetworkFormation.Fractal)},state.Links.Where(link=>!SphereLink(link)).Select(Link).ToArray());
        }
#if UNITY_EDITOR
        internal static (NativeWorldNetwork.Network Network,NativeWorldNetwork.Node?[] RuntimeNodes,NativeWorldNetwork.Link[] ExternalLinks) ProjectNativeNetwork(V scene,V source)
        {
            int sphereCount=(int)scene["spheres"]["length"].Number;
            bool SphereIndex(V index)=>index.Number>=0&&index.Number<sphereCount;
            bool SphereLink(V link)=>SphereIndex(link["a"])&&SphereIndex(link["b"]);
            NativeWorldNetwork.Formation Formation(string key){var shape=source[key];if(!shape.Truth)return null;var indexes=shape["nodes"].Items.Where(SphereIndex).Select(index=>(int)index.Number).ToArray();return indexes.Length>=2?new NativeWorldNetwork.Formation {Nodes=indexes,Active=!(shape["active"].Raw is bool active)||active}:null;}
            NativeWorldNetwork.Link Link(V link)=>new NativeWorldNetwork.Link {A=(int)link["a"].Number,B=(int)link["b"].Number};
            var nodes=scene["spheres"].Items.Select(s=>new NativeWorldNetwork.Node {X=s["pos"]["x"].Number,Y=s["pos"]["y"].Number,Charge=s["formationHitCount"].Number,Pulse=s["resonancePulseTimer"].Number}).ToArray();
            var positions=scene["spheres"].Items.Select(s=>s["pos"]).ToList();if(scene["player"]["abilities"]["minion"].Number>=3)positions.AddRange(scene["minions"].Items.Select(minion=>minion["pos"]));
            return (new NativeWorldNetwork.Network {NodeCount=source["nodes"].Items.Count(SphereIndex),Spheres=nodes,Links=source["links"].Items.Where(SphereLink).Select(Link).ToArray(),Line=Formation("line"),Triangle=Formation("triangle"),Square=Formation("square"),Cluster=Formation("cluster"),Ring=Formation("ring"),Lattice=Formation("lattice"),Fractal=Formation("fractal")},positions.Select(position=>position.Truth?(NativeWorldNetwork.Node?)NativeNetworkPoint(position):null).ToArray(),source["links"].Items.Where(link=>!SphereLink(link)).Select(Link).ToArray());
        }
#endif
        private void DrawNativeFormationMemory(V scene)
        {
            var memory=scene["formationMemory"];if(!memory.Truth)return;
            _nativeWorldNetwork.DrawMemory(_worldSurface,memory["type"].Text,memory["nodes"].Items.Select(NativeNetworkPoint).ToArray(),memory["expiresAt"].Number,scene["time"].Number);
        }
    }
}
