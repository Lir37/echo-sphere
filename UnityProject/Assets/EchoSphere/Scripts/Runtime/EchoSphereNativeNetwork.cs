using System.Linq;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldNetwork _nativeWorldNetwork;
        private static NativeWorldNetwork.Node NativeNetworkPoint(V position)=>new NativeWorldNetwork.Node {X=position["x"].Number,Y=position["y"].Number};
        private void DrawNativeNetwork(V scene,V source)
        {
            var projected=ProjectNativeNetwork(scene,source);_nativeWorldNetwork.Draw(_worldSurface,projected.Network,projected.RuntimeNodes,projected.ExternalLinks,scene["time"].Number);
        }
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
        private void DrawNativeFormationMemory(V scene)
        {
            var memory=scene["formationMemory"];if(!memory.Truth)return;
            _nativeWorldNetwork.DrawMemory(_worldSurface,memory["type"].Text,memory["nodes"].Items.Select(NativeNetworkPoint).ToArray(),memory["expiresAt"].Number,scene["time"].Number);
        }
    }
}
