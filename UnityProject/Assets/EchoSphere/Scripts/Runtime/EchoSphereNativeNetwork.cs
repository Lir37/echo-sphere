using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        #if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        private static NativeWorldNetwork.Node NativeNetworkPoint(V position)=>new NativeWorldNetwork.Node {X=position["x"].Number,Y=position["y"].Number};
        #endif
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal static SphereNetworkState NativeAnalyzeSphereNetwork(V spheres)
        {
            // Logical source axes keep preview perimeter ordering; lengths use
            // the same calibrated units as the authoritative native analyzer.
            var nodes=spheres.Items.Select(sphere=>new SphereNetworkNode(new Vec2((float)sphere["pos"]["x"].Number*SpherePlacementRules.SourcePixelsToWorld,(float)sphere["pos"]["y"].Number*SpherePlacementRules.SourcePixelsToWorld),!(sphere["alive"].Raw is bool alive&&!alive))).ToList();
            return SphereNetworkRules.Analyze(nodes,SphereNetworkRules.DefaultLinkDistance);
        }
#endif
        private double NativeLiveOrbitalNetworkMultiplier(SphereAttackAgent sphere)
        {
            int index=GetSphereIndex(sphere);double cluster=GetFormationBonus(SphereNetworkFormation.Cluster,index),ring=GetFormationBonus(SphereNetworkFormation.Ring,index);int neighbours=0;
            if(_networkState!=null)foreach(var link in _networkState.Links)if(link.A==index||link.B==index)neighbours++;
            return 1+(cluster>0?.08*cluster:0)+(ring>0?.12*ring:0)+System.Math.Min(.20,neighbours*.03);
        }
        internal static double NativeOrbitalNetworkMultiplier(SphereNetworkState state,int index)
        {
            double cluster=SphereNetworkRules.GetFormationBonusMultiplier(state,SphereNetworkFormation.Cluster,index),ring=SphereNetworkRules.GetFormationBonusMultiplier(state,SphereNetworkFormation.Ring,index);int neighbours=state==null?0:state.Links.Count(link=>link.A==index||link.B==index);
            return 1+(cluster>0?.08*cluster:0)+(ring>0?.12*ring:0)+System.Math.Min(.20,neighbours*.03);
        }
        internal NativeWorldNetwork.Node[] ProjectNativeWorldNetworkNodes()=>_spheres.Where(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy).Select(sphere=>{var point=NativeSourcePixels(sphere.transform.position);return new NativeWorldNetwork.Node {X=point.x,Y=point.y,Charge=sphere.FormationHitCount,Pulse=sphere.ResonancePulseTimer};}).ToArray();
        // Exact-sized arrays retain the original renderer contract (Length is
        // the live count), while stable populations allocate nothing per frame.
        private sealed class NativeNetworkProjectionBuffers
        {
            internal readonly NativeWorldNetwork.Network Network=new NativeWorldNetwork.Network();
            internal NativeWorldNetwork.Node?[] Positions=System.Array.Empty<NativeWorldNetwork.Node?>();
            internal NativeWorldNetwork.Link[] External=System.Array.Empty<NativeWorldNetwork.Link>();
            private readonly NativeWorldNetwork.Formation[] _formations=new NativeWorldNetwork.Formation[7];
            internal static void Size<T>(ref T[] values,int length){if(values==null||values.Length!=length)values=new T[length];}
            private NativeWorldNetwork.Formation Shape(SphereNetworkState state,SphereNetworkFormation kind,int slot,int count)
            {
                var source=state?.GetFormation(kind);if(source==null)return null;
                int length=0;foreach(int index in source.NodeIndexes)if(index>=0&&index<count)length++;
                if(length<2)return null;
                var target=_formations[slot]??(_formations[slot]=new NativeWorldNetwork.Formation());Size(ref target.Nodes,length);
                int at=0;foreach(int index in source.NodeIndexes)if(index>=0&&index<count)target.Nodes[at++]=index;
                target.Active=source.Active;return target;
            }
            internal void Topology(SphereNetworkState state,int count)
            {
                int internalCount=0,externalCount=0;Network.NodeCount=0;
                if(state!=null){foreach(int index in state.Nodes)if(index>=0&&index<count)Network.NodeCount++;
                    foreach(var link in state.Links)if(link.A>=0&&link.A<count&&link.B>=0&&link.B<count)internalCount++;else externalCount++;}
                Size(ref Network.Links,internalCount);Size(ref External,externalCount);int a=0,b=0;
                if(state!=null)foreach(var link in state.Links){var value=new NativeWorldNetwork.Link {A=link.A,B=link.B};if(link.A>=0&&link.A<count&&link.B>=0&&link.B<count)Network.Links[a++]=value;else External[b++]=value;}
                Network.Line=Shape(state,SphereNetworkFormation.Line,0,count);Network.Triangle=Shape(state,SphereNetworkFormation.Triangle,1,count);
                Network.Square=Shape(state,SphereNetworkFormation.Square,2,count);Network.Cluster=Shape(state,SphereNetworkFormation.Cluster,3,count);
                Network.Ring=Shape(state,SphereNetworkFormation.Ring,4,count);Network.Lattice=Shape(state,SphereNetworkFormation.Lattice,5,count);Network.Fractal=Shape(state,SphereNetworkFormation.Fractal,6,count);
            }
        }
        private readonly NativeNetworkProjectionBuffers _nativeNetworkProjection=new NativeNetworkProjectionBuffers();
        private void DrawNativeNetwork()
        {
            int count=0,drones=0;bool includeDrones=GetAbilityLevel(RunUpgradeKind.Minion)>=3;
            foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)count++;
            if(includeDrones)foreach(var agent in _simulationAgents)if(agent is AbilityDroneAgent2D drone&&drone!=null&&drone.gameObject.activeInHierarchy)drones++;
            var buffer=_nativeNetworkProjection;NativeNetworkProjectionBuffers.Size(ref buffer.Network.Spheres,count);NativeNetworkProjectionBuffers.Size(ref buffer.Positions,count+drones);
            int at=0;foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy){var point=NativeSourcePixels(sphere.transform.position);buffer.Network.Spheres[at]=new NativeWorldNetwork.Node {X=point.x,Y=point.y,Charge=sphere.FormationHitCount,Pulse=sphere.ResonancePulseTimer};buffer.Positions[at++]=new NativeWorldNetwork.Node {X=point.x,Y=point.y};}
            if(includeDrones)foreach(var agent in _simulationAgents)if(agent is AbilityDroneAgent2D drone&&drone!=null&&drone.gameObject.activeInHierarchy){var point=NativeSourcePixels(drone.transform.position);buffer.Positions[at++]=new NativeWorldNetwork.Node {X=point.x,Y=point.y};}
            buffer.Topology(_networkState,count);
            _nativeWorldNetwork.Draw(_worldSurface,buffer.Network,buffer.Positions,buffer.External,_runTime);
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
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
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
