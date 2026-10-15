// Generated from src/networkRuntime.ts; sha256 af1a1d204c876801e1ffb221e383a4abe159e305c411bbccd067e4785d4e9a8b.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_networkRuntime {
public static readonly V Scope=VisualJs.Obj();
static Visual_networkRuntime() {
Scope[new V("buildRuntimeNetworkNodes")] = VisualJs.Fn(F_buildRuntimeNetworkNodes);
}
private static V F_buildRuntimeNetworkNodes(V[] args) {
V v_spheres_0 = VisualJs.Arg(args,0);
V v_minions_1 = VisualJs.Arg(args,1);
V v_includeMinions_2 = VisualJs.Arg(args,2);
V v_nodes_3 = v_spheres_0[new V("map")].Call(VisualJs.Fn(args_4 => { V v_sphere_5 = VisualJs.Arg(args_4, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_5[new V("pos")], new V("alive"), VisualJs.And(new V(v_sphere_5[new V("alive")] != new V(false)), () => new V((VisualJs.Or(v_sphere_5[new V("networkDisabledTimer")], () => new V(0d))) <= new V(0d))))); }));
if ((!v_includeMinions_2).Truth) return v_nodes_3;
foreach (V v_minion_6 in v_minions_1.Items) {
v_nodes_3[new V("push")].Call(VisualJs.Obj(new V("pos"), v_minion_6[new V("pos")], new V("alive"), new V(v_minion_6[new V("life")] > new V(0d))));
}
return v_nodes_3;
return default;
}
}
}
