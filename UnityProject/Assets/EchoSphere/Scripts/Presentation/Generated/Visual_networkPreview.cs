// Generated from src/networkPreview.ts; sha256 5e0286db542c85de38635a79d7491bd620bd78092208fdcb65941933e1c21074.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_networkPreview {
public static readonly V Scope=VisualJs.Obj();
static Visual_networkPreview() {
Scope[new V("getGhostSnapFormation")] = VisualJs.Fn(F_getGhostSnapFormation);
Scope[new V("buildGhostSnapPreview")] = VisualJs.Fn(F_buildGhostSnapPreview);
}
private static V F_getGhostSnapFormation(V[] args) {
V v_network_0 = VisualJs.Arg(args,0);
return VisualJs.Coalesce(v_network_0[new V("dominantFormation")], () => VisualJs.Null);
}
private static V F_buildGhostSnapPreview(V[] args) {
V v_spheres_1 = VisualJs.Arg(args,0);
V v_candidate_2 = VisualJs.Arg(args,1);
V v_linkDistance_3 = VisualJs.Arg(args,2).Undefined?new V(220d):VisualJs.Arg(args,2);
V v_movingSphereIndex_4 = VisualJs.Arg(args,3);
if (new V(v_spheres_1[new V("length")] == new V(0d)).Truth) return VisualJs.Null;
V v_moving_5 = (VisualJs.And(VisualJs.And(VisualJs.And(new V(v_movingSphereIndex_4 != default(V)), () => VisualJs.NumberObject[new V("isInteger")].Call(v_movingSphereIndex_4)), () => new V(v_movingSphereIndex_4 >= new V(0d))), () => new V(v_movingSphereIndex_4 < v_spheres_1[new V("length")])).Truth ? v_movingSphereIndex_4 : default(V));
V v_nodes_6 = v_spheres_1[new V("map")].Call(VisualJs.Fn(args_7 => { V v_sphere_8 = VisualJs.Arg(args_7, 0);
V v_index_9 = VisualJs.Arg(args_7, 1);
return (VisualJs.Obj(new V("pos"), (new V(v_index_9 == v_moving_5).Truth ? VisualJs.Merge(v_candidate_2) : VisualJs.Merge(v_sphere_8[new V("pos")])), new V("alive"), VisualJs.And(v_sphere_8[new V("alive")], () => new V((VisualJs.Or(v_sphere_8[new V("networkDisabledTimer")], () => new V(0d))) <= new V(0d))), new V("networkDisabledTimer"), new V(0d))); }));
V v_candidateIndex_10 = VisualJs.Coalesce(v_moving_5, () => v_nodes_6[new V("length")]);
if (new V(v_moving_5 == default(V)).Truth) {
v_nodes_6[new V("push")].Call(VisualJs.Obj(new V("pos"), VisualJs.Merge(v_candidate_2), new V("alive"), new V(true), new V("networkDisabledTimer"), new V(0d)));
} else if (v_nodes_6[v_candidateIndex_10].Truth) {
VisualJs.Assign(v_nodes_6,v_candidateIndex_10,"=",VisualJs.Obj(new V("pos"), VisualJs.Merge(v_candidate_2), new V("alive"), VisualJs.And(v_spheres_1[v_candidateIndex_10][new V("alive")], () => new V((VisualJs.Or(v_spheres_1[v_candidateIndex_10][new V("networkDisabledTimer")], () => new V(0d))) <= new V(0d))), new V("networkDisabledTimer"), new V(0d)));
}
V v_network_11 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_nodes_6, v_linkDistance_3);
V v_linkedNodeIndexes_12 = v_network_11[new V("links")][new V("filter")].Call(VisualJs.Fn(args_13 => { V v_link_14 = VisualJs.Arg(args_13, 0);
return VisualJs.Or(new V(v_link_14[new V("a")] == v_candidateIndex_10), () => new V(v_link_14[new V("b")] == v_candidateIndex_10)); }))[new V("map")].Call(VisualJs.Fn(args_15 => { V v_link_16 = VisualJs.Arg(args_15, 0);
return (new V(v_link_16[new V("a")] == v_candidateIndex_10).Truth ? v_link_16[new V("b")] : v_link_16[new V("a")]); }));
V v_formation_17 = Scope[new V("getGhostSnapFormation")].Call(v_network_11);
return VisualJs.Obj(new V("candidate"), VisualJs.Merge(v_candidate_2), new V("candidateIndex"), v_candidateIndex_10, new V("network"), v_network_11, new V("nodes"), v_nodes_6, new V("linkedNodeIndexes"), v_linkedNodeIndexes_12, new V("formation"), v_formation_17);
}
}
}
