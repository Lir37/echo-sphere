// Generated from src/networkRender.ts; sha256 a340d4e26bf4da771f7ff73b90d35901331eb35727ff5a36f67cf8550141ece9.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_networkRender {
public static readonly V Scope=VisualJs.Obj();
static Visual_networkRender() {
Scope[new V("filterShapeToSpheres")] = VisualJs.Fn(F_filterShapeToSpheres);
Scope[new V("getSphereVisualNetwork")] = VisualJs.Fn(F_getSphereVisualNetwork);
}
private static V F_filterShapeToSpheres(V[] args) {
V v_shape_0 = VisualJs.Arg(args,0);
V v_sphereCount_1 = VisualJs.Arg(args,1);
if ((!v_shape_0).Truth) return VisualJs.Null;
V v_nodes_2 = v_shape_0[new V("nodes")][new V("filter")].Call(VisualJs.Fn(args_3 => { V v_index_4 = VisualJs.Arg(args_3, 0);
return VisualJs.And(new V(v_index_4 >= new V(0d)), () => new V(v_index_4 < v_sphereCount_1)); }));
return (new V(v_nodes_2[new V("length")] >= new V(2d)).Truth ? VisualJs.Merge(v_shape_0, VisualJs.Obj(new V("nodes"),v_nodes_2)) : VisualJs.Null);
return default;
}
private static V F_getSphereVisualNetwork(V[] args) {
V v_source_5 = VisualJs.Arg(args,0);
V v_sphereCount_6 = VisualJs.Arg(args,1);
V v_isSphereLink_7 = default; v_isSphereLink_7 = VisualJs.Fn(args_8 => { V v_link_9 = VisualJs.Arg(args_8, 0);
return VisualJs.And(VisualJs.And(VisualJs.And(new V(v_link_9[new V("a")] >= new V(0d)), () => new V(v_link_9[new V("a")] < v_sphereCount_6)), () => new V(v_link_9[new V("b")] >= new V(0d))), () => new V(v_link_9[new V("b")] < v_sphereCount_6)); });
V v_filterCandidates_10 = v_source_5[new V("formationCandidates")][new V("map")].Call(VisualJs.Fn(args_11 => { V v_shape_12 = VisualJs.Arg(args_11, 0);
return Scope[new V("filterShapeToSpheres")].Call(v_shape_12, v_sphereCount_6); }))[new V("filter")].Call(VisualJs.Fn(args_13 => { V v_shape_14 = VisualJs.Arg(args_13, 0);
return new V(v_shape_14.Truth); }));
return VisualJs.Obj(new V("network"), VisualJs.Merge(v_source_5, VisualJs.Obj(new V("nodes"),v_source_5[new V("nodes")][new V("filter")].Call(VisualJs.Fn(args_15 => { V v_index_16 = VisualJs.Arg(args_15, 0);
return VisualJs.And(new V(v_index_16 >= new V(0d)), () => new V(v_index_16 < v_sphereCount_6)); }))), VisualJs.Obj(new V("links"),v_source_5[new V("links")][new V("filter")].Call(v_isSphereLink_7)), VisualJs.Obj(new V("formationCandidates"),v_filterCandidates_10), VisualJs.Obj(new V("dominantFormation"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("dominantFormation")], v_sphereCount_6)), VisualJs.Obj(new V("secondaryFormation"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("secondaryFormation")], v_sphereCount_6)), VisualJs.Obj(new V("line"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("line")], v_sphereCount_6)), VisualJs.Obj(new V("triangle"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("triangle")], v_sphereCount_6)), VisualJs.Obj(new V("square"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("square")], v_sphereCount_6)), VisualJs.Obj(new V("cluster"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("cluster")], v_sphereCount_6)), VisualJs.Obj(new V("ring"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("ring")], v_sphereCount_6)), VisualJs.Obj(new V("lattice"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("lattice")], v_sphereCount_6)), VisualJs.Obj(new V("fractal"),Scope[new V("filterShapeToSpheres")].Call(v_source_5[new V("fractal")], v_sphereCount_6))), new V("externalLinks"), v_source_5[new V("links")][new V("filter")].Call(VisualJs.Fn(args_17 => { V v_link_18 = VisualJs.Arg(args_17, 0);
return (!v_isSphereLink_7.Call(v_link_18)); })));
return default;
}
}
}
