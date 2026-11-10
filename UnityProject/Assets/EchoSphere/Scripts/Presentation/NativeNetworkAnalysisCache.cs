using System;using System.Collections.Generic;using EchoSphere.Core;
namespace EchoSphere.Presentation {
 // Geometry identity is separate from temporary network availability.
 internal sealed class NativeNetworkAnalysisCache {
  internal readonly List<SphereNetworkNode> Nodes=new List<SphereNetworkNode>();
  internal readonly List<int> Identities=new List<int>();internal int SphereCount;
  private readonly List<SphereNetworkNode> _previousNodes=new List<SphereNetworkNode>();
  private readonly List<int> _ids=new List<int>();private readonly List<SphereNetworkNode> _structural=new List<SphereNetworkNode>();
  internal bool PreserveDisplacedSelection;private bool _lastRecovery;
  private SphereNetworkFormation _previous,_dominant,_secondary;private int _sphereCount;private SphereNetworkState _state;private bool _refreshMain,_refreshSecondary;
  internal void RequestSelectionRefresh(bool main){if(main)_refreshMain=true;else _refreshSecondary=true;}
  internal SphereNetworkState Analyze(SphereNetworkState current,SphereNetworkFormation previous,SphereNetworkFormation dominant,SphereNetworkFormation secondary){
   bool identified=Identities.Count==Nodes.Count;int Identity(int i)=>identified?Identities[i]:i;
   bool same=PreserveDisplacedSelection==_lastRecovery&&!_refreshMain&&!_refreshSecondary&&ReferenceEquals(current,_state)&&_state!=null&&previous==_previous&&dominant==_dominant&&secondary==_secondary&&Nodes.Count==_previousNodes.Count;
   if(same)for(int i=0;i<Nodes.Count;i++){var a=Nodes[i];var b=_previousNodes[i];if(Identity(i)!=_ids[i]||a.Position.X!=b.Position.X||a.Position.Y!=b.Position.Y||a.Alive!=b.Alive||(a.DisabledTimer>0)!=(b.DisabledTimer>0)){same=false;break;}}
   if(same)return _state;
   // Addition/removal and uniform formation following do not count as repositioning an existing sphere.
   bool moved=false;int anchor=-1,oldAnchor=-1;int sphereCount=identified?SphereCount:Nodes.Count;
   for(int i=0;i<sphereCount;i++){int old=_ids.IndexOf(Identity(i));if(old<0||old>=_sphereCount||!Nodes[i].Alive||!_previousNodes[old].Alive)continue;if(anchor<0){anchor=i;oldAnchor=old;continue;}var a=Nodes[i].Position-Nodes[anchor].Position;var b=_previousNodes[old].Position-_previousNodes[oldAnchor].Position;if(Vec2.Distance(a,b)>.001f){moved=true;break;}}
   _structural.Clear();foreach(var node in Nodes)_structural.Add(new SphereNetworkNode(node.Position,node.Alive));
   var next=SphereNetworkRules.Analyze(_structural,SphereNetworkRules.DefaultLinkDistance,previous);
   SphereNetworkShape Retain(SphereNetworkShape shape){if(shape==null)return null;var indexes=new List<int>();foreach(int old in shape.NodeIndexes){int id=_ids.Count==0?old:old<_ids.Count?_ids[old]:int.MinValue;int at=_ids.Count==0?old:identified?Identities.IndexOf(id):id;if(at<0||at>=Nodes.Count||!Nodes[at].Alive)return null;indexes.Add(at);}return new SphereNetworkShape(shape.Type,shape.Strength,indexes){DominanceScore=shape.DominanceScore};}
   var selectedMain=_refreshMain?null:current?.DominantFormation;
   var selectedExtra=_refreshSecondary?null:current?.SecondaryFormation;
   var main=!moved||PreserveDisplacedSelection?Retain(selectedMain):null;
   var extra=!moved||PreserveDisplacedSelection?Retain(selectedExtra):null;
   _refreshMain=_refreshSecondary=false;
   bool Matches(SphereNetworkShape shape){var subset=new List<SphereNetworkNode>(Nodes.Count);for(int i=0;i<Nodes.Count;i++)subset.Add(new SphereNetworkNode(Nodes[i].Position,shape.Contains(i)&&Nodes[i].Alive));var analysis=SphereNetworkRules.Analyze(subset);return analysis.FormationCandidates.Exists(candidate=>candidate.Type==shape.Type&&candidate.NodeIndexes.Count==shape.NodeIndexes.Count&&candidate.NodeIndexes.TrueForAll(shape.Contains));}
   SphereNetworkState activeNetwork=null;
   SphereNetworkShape Recover(SphereNetworkShape selected,SphereNetworkShape retained,out bool valid){
    valid=true;
    if(selected==null)return null;
    if(retained!=null&&retained.NodeIndexes.Count>0&&Matches(retained))return retained;
    // A disrupted slot keeps its type. Only usable nodes can rebuild it.
    var candidates=next.FormationCandidates;
    if(Nodes.Exists(node=>node.Alive&&!node.IsActive)){
     activeNetwork??=SphereNetworkRules.Analyze(Nodes,next.LinkDistance,previous);
     candidates=activeNetwork.FormationCandidates;
    }
    var replacement=candidates.Find(candidate=>candidate.Type==selected.Type);
    if(replacement!=null)return replacement;
    valid=false;
    var pending=retained??new SphereNetworkShape(selected.Type,selected.Strength,new List<int>()){DominanceScore=selected.DominanceScore};
    pending.Active=false;
    return pending;
   }
   bool mainValid=true,extraValid=true;
   if(PreserveDisplacedSelection){
    main=Recover(selectedMain,main,out mainValid);
    extra=Recover(selectedExtra,extra,out extraValid);
   }
   // Replace candidate of the same type, retaining the exact existing members when a new node makes a stronger copy.
   void Candidate(SphereNetworkShape shape){if(shape==null)return;int at=next.FormationCandidates.FindIndex(s=>s.Type==shape.Type);if(at<0)next.FormationCandidates.Add(shape);else next.FormationCandidates[at]=shape;}
   Candidate(main);Candidate(extra);if(main!=null){next.DominantFormation=main;if(extra==null){next.SecondaryFormation=null;SphereNetworkRules.TrySetDominantFormation(next,main.Type);}}if(extra!=null&&extra.Type!=next.DominantFormation?.Type)next.SecondaryFormation=extra;
   if(dominant!=SphereNetworkFormation.None&&dominant!=_dominant)SphereNetworkRules.TrySetDominantFormation(next,dominant);
   else if(main==null&&dominant!=SphereNetworkFormation.None)SphereNetworkRules.TrySetDominantFormation(next,dominant);
   if(secondary!=SphereNetworkFormation.None&&(secondary!=_secondary||extra==null))SphereNetworkRules.TrySetSecondaryFormation(next,secondary);
   foreach(var shape in next.FormationCandidates){foreach(int index in shape.NodeIndexes)if(!Nodes[index].IsActive){shape.Active=false;break;}}
   // Disabled nodes have no live links/bonuses; selected shapes remain in their slots, marked inactive.
   next.Nodes.RemoveAll(index=>!Nodes[index].IsActive);next.Links.RemoveAll(link=>!Nodes[link.A].IsActive||!Nodes[link.B].IsActive);
   if(PreserveDisplacedSelection&&mainValid&&extraValid)PreserveDisplacedSelection=false;_lastRecovery=PreserveDisplacedSelection;
   _previousNodes.Clear();_previousNodes.AddRange(Nodes);_ids.Clear();for(int i=0;i<Nodes.Count;i++)_ids.Add(Identity(i));_sphereCount=sphereCount;_previous=previous;_dominant=dominant;_secondary=secondary;return _state=next;
  }
 }
}
