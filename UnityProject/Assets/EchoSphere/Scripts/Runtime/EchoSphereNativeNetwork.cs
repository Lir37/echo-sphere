using System.Linq;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldNetwork _nativeWorldNetwork;
        private static NativeWorldNetwork.Node NativeNetworkPoint(V position)=>new NativeWorldNetwork.Node {X=position["x"].Number,Y=position["y"].Number};
        private static NativeWorldNetwork.Formation NativeNetworkFormation(V source)
            =>source.Truth?new NativeWorldNetwork.Formation {Nodes=source["nodes"].Items.Select(i=>(int)i.Number).ToArray(),Active=!(source["active"].Raw is bool active)||active}:null;
        private static NativeWorldNetwork.Link[] NativeNetworkLinks(V source)=>source.Items.Select(l=>new NativeWorldNetwork.Link {A=(int)l["a"].Number,B=(int)l["b"].Number}).ToArray();
        private void DrawNativeNetwork(V scene,V source)
        {
            var runtime=Visual_networkRuntime.Scope["buildRuntimeNetworkNodes"].Call(scene["spheres"],scene["minions"],scene["player"]["abilities"]["minion"].Number>=3);
            var visual=Visual_networkRender.Scope["getSphereVisualNetwork"].Call(source,scene["spheres"]["length"]);var network=visual["network"];
            var nodes=scene["spheres"].Items.Select(s=>new NativeWorldNetwork.Node {X=s["pos"]["x"].Number,Y=s["pos"]["y"].Number,Charge=s["formationHitCount"].Number,Pulse=s["resonancePulseTimer"].Number}).ToArray();
            _nativeWorldNetwork.Draw(_sourceCanvas,new NativeWorldNetwork.Network {NodeCount=(int)network["nodes"]["length"].Number,Spheres=nodes,Links=NativeNetworkLinks(network["links"]),Line=NativeNetworkFormation(network["line"]),Triangle=NativeNetworkFormation(network["triangle"]),Square=NativeNetworkFormation(network["square"]),Cluster=NativeNetworkFormation(network["cluster"]),Ring=NativeNetworkFormation(network["ring"]),Lattice=NativeNetworkFormation(network["lattice"]),Fractal=NativeNetworkFormation(network["fractal"])},runtime.Items.Select(n=>n["pos"].Truth?(NativeWorldNetwork.Node?)NativeNetworkPoint(n["pos"]):null).ToArray(),NativeNetworkLinks(visual["externalLinks"]),scene["time"].Number);
        }
        private void DrawNativeFormationMemory(V scene)
        {
            var memory=scene["formationMemory"];if(!memory.Truth)return;
            _nativeWorldNetwork.DrawMemory(_sourceCanvas,memory["type"].Text,memory["nodes"].Items.Select(NativeNetworkPoint).ToArray(),memory["expiresAt"].Number,scene["time"].Number);
        }
    }
}
