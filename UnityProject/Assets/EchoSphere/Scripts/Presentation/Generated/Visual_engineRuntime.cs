// Generated from src/engineRuntime.ts; sha256 baeaf14c2cc4905833d0e7423c17b03f50c03125935d731543eae6ec08b9463d.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_engineRuntime {
public static readonly V Scope=VisualJs.Obj();
static Visual_engineRuntime() {
Scope[new V("getAbilityBranchId")] = VisualJs.Fn(F_getAbilityBranchId);
Scope[new V("getSphereFinalIndex")] = VisualJs.Fn(F_getSphereFinalIndex);
}
private static V F_getAbilityBranchId(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_ability_1 = VisualJs.Arg(args,1);
V v_stage_2 = VisualJs.Arg(args,2);
V v_prefix_3 = ((((new V("ability:") + v_ability_1) + new V(":")) + v_stage_2) + new V(":"));
V v_marker_4 = (VisualJs.Or(v_s_0[new V("player")][new V("evolutions")], () => VisualJs.Arr()))[new V("find")].Call(VisualJs.Fn(args_5 => { V v_x_6 = VisualJs.Arg(args_5, 0);
return v_x_6[new V("startsWith")].Call(v_prefix_3); }));
return (v_marker_4.Truth ? v_marker_4[new V("slice")].Call(v_prefix_3[new V("length")]) : VisualJs.Null);
return default;
}
private static V F_getSphereFinalIndex(V[] args) {
V v_s_7 = VisualJs.Arg(args,0);
V v_type_8 = VisualJs.Arg(args,1);
V v_marker_9 = (VisualJs.Or(v_s_7[new V("player")][new V("evolutions")], () => VisualJs.Arr()))[new V("find")].Call(VisualJs.Fn(args_10 => { V v_x_11 = VisualJs.Arg(args_10, 0);
return v_x_11[new V("startsWith")].Call(((new V("sphere:") + v_type_8) + new V(":7:"))); }));
if ((!v_marker_9).Truth) return VisualJs.Null;
V v_value_12 = VisualJs.Number(v_marker_9[new V("slice")].Call((v_marker_9[new V("lastIndexOf")].Call(new V(":")) + new V(1d))));
return (VisualJs.NumberObject[new V("isFinite")].Call(v_value_12).Truth ? v_value_12 : VisualJs.Null);
return default;
}
}
}
