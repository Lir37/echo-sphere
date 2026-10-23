using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldNetwork _nativeWorldNetwork;
        #if UNITY_EDITOR
        private static NativeWorldNetwork.Node NativeNetworkPoint(V position)=>new NativeWorldNetwork.Node {X=position["x"].Number,Y=position["y"].Number};
        #endif
        internal static SphereNetworkState NativeAnalyzeSphereNetwork(V spheres)
        {
            // Logical source axes keep preview perimeter ordering; lengths use
            // the same calibrated units as the authoritative native analyzer.
            var nodes=spheres.Items.Select(sphere=>new SphereNetworkNode(new Vec2((float)sphere["pos"]["x"].Number*SpherePlacementRules.SourcePixelsToWorld,(float)sphere["pos"]["y"].Number*SpherePlacementRules.SourcePixelsToWorld),!(sphere["alive"].Raw is bool alive&&!alive))).ToList();
            return SphereNetworkRules.Analyze(nodes,SphereNetworkRules.DefaultLinkDistance);
        }
        internal static double NativeOrbitalNetworkMultiplier(SphereNetworkState state,int index)
        {
            double cluster=SphereNetworkRules.GetFormationBonusMultiplier(state,SphereNetworkFormation.Cluster,index),ring=SphereNetworkRules.GetFormationBonusMultiplier(state,SphereNetworkFormation.Ring,index);int neighbours=state==null?0:state.Links.Count(link=>link.A==index||link.B==index);
            return 1+(cluster>0?.08*cluster:0)+(ring>0?.12*ring:0)+System.Math.Min(.20,neighbours*.03);
        }
        internal NativeWorldNetwork.Node[] ProjectNativeWorldNetworkNodes()=>_spheres.Where(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy).Select(sphere=>{var point=NativeSourcePixels(sphere.transform.position);return new NativeWorldNetwork.Node {X=point.x,Y=point.y,Charge=sphere.FormationHitCount,Pulse=sphere.ResonancePulseTimer};}).ToArray();
        private void DrawNativeNetwork()
        {
            var spheres=ProjectNativeWorldNetworkNodes();
            var positions=spheres.Select(sphere=>(NativeWorldNetwork.Node?)new NativeWorldNetwork.Node {X=sphere.X,Y=sphere.Y}).ToList();if(GetAbilityLevel(RunUpgradeKind.Minion)>=3)foreach(var agent in _simulationAgents)if(agent is AbilityDroneAgent2D drone&&drone!=null&&drone.gameObject.activeInHierarchy){var point=NativeSourcePixels(drone.transform.position);positions.Add(new NativeWorldNetwork.Node {X=point.x,Y=point.y});}
            var projected=ProjectNativeNetworkState(_networkState,spheres);
            _nativeWorldNetwork.Draw(_worldSurface,projected.Network,positions.ToArray(),projected.ExternalLinks,_runTime);
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
        private void DrawNativeFormationMemory()
        {
            if(!(_formationMemoryTimer>0)||_formationMemoryPoints==null)return;var nodes=_formationMemoryPoints.Select(position=>{var point=NativeSourcePixels(position);return new NativeWorldNetwork.Node {X=point.x,Y=point.y};}).ToArray();
            _nativeWorldNetwork.DrawMemory(_worldSurface,_formationMemoryType.ToString().ToLowerInvariant(),nodes,_runTime+_formationMemoryTimer,_runTime);
        }
    }
}
