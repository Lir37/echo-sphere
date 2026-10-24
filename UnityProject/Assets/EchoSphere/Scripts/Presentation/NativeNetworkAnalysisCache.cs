using System.Collections.Generic;
using EchoSphere.Core;

namespace EchoSphere.Presentation
{
    // Exact input cache only. Runtime bookkeeping still runs on every refresh.
    // This presentation-owned cache is intentionally excluded from run saves.
    internal sealed class NativeNetworkAnalysisCache
    {
        internal readonly List<SphereNetworkNode> Nodes=new List<SphereNetworkNode>();
        private readonly List<SphereNetworkNode> _previousNodes=new List<SphereNetworkNode>();
        private SphereNetworkFormation _previous,_dominant,_secondary;
        private SphereNetworkState _state;
        internal SphereNetworkState Analyze(SphereNetworkState current,SphereNetworkFormation previous,SphereNetworkFormation dominant,SphereNetworkFormation secondary)
        {
            bool same=ReferenceEquals(current,_state)&&_state!=null&&previous==_previous&&dominant==_dominant&&secondary==_secondary&&Nodes.Count==_previousNodes.Count;
            if(same)for(int i=0;i<Nodes.Count;i++){
                var a=Nodes[i];var b=_previousNodes[i];
                if(a.Position.X!=b.Position.X||a.Position.Y!=b.Position.Y||a.Alive!=b.Alive||a.DisabledTimer!=b.DisabledTimer){same=false;break;}
            }
            if(same)return _state;
            _previousNodes.Clear();_previousNodes.AddRange(Nodes);_previous=previous;_dominant=dominant;_secondary=secondary;
            return _state=SphereNetworkRules.Analyze(Nodes,SphereNetworkRules.DefaultLinkDistance,previous);
        }
    }
}
