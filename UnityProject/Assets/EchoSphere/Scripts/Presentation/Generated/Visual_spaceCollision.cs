#if UNITY_EDITOR
// Generated from src/spaceCollision.ts; sha256 dd3b9ab788a6210a95af074d45cdb35d8394b6771a7ed9e5db23cf8131aaed6e.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_spaceCollision {
public static readonly V Scope=VisualJs.Obj();
static Visual_spaceCollision() {
Scope[new V("distance")] = VisualJs.Fn(F_distance);
Scope[new V("clampScalar")] = VisualJs.Fn(F_clampScalar);
Scope[new V("cellKey")] = VisualJs.Fn(F_cellKey);
Scope[new V("canPlaceSphere")] = VisualJs.Fn(F_canPlaceSphere);
Scope[new V("canRepositionSphere")] = VisualJs.Fn(F_canRepositionSphere);
Scope[new V("hasRouteToOutside")] = VisualJs.Fn(F_hasRouteToOutside);
Scope[new V("TOWER_BODY_RADIUS")] = new V(20d);
Scope[new V("TOWER_PLACEMENT_GAP")] = new V(8d);
Scope[new V("PLAYER_BUILD_GAP")] = new V(48d);
Scope[new V("ENEMY_PLACEMENT_GAP")] = new V(10d);
Scope[new V("ENEMY_NAV_RADIUS")] = new V(22d);
Scope[new V("NAV_CELL_SIZE")] = new V(40d);
}
private static V F_distance(V[] args) {
V v_a_0 = VisualJs.Arg(args,0);
V v_b_1 = VisualJs.Arg(args,1);
return VisualJs.MathObject[new V("hypot")].Call((v_a_0[new V("x")] - v_b_1[new V("x")]), (v_a_0[new V("y")] - v_b_1[new V("y")]));
}
private static V F_clampScalar(V[] args) {
V v_value_2 = VisualJs.Arg(args,0);
V v_min_3 = VisualJs.Arg(args,1);
V v_max_4 = VisualJs.Arg(args,2);
return VisualJs.MathObject[new V("max")].Call(v_min_3, VisualJs.MathObject[new V("min")].Call(v_max_4, v_value_2));
}
private static V F_cellKey(V[] args) {
V v_x_5 = VisualJs.Arg(args,0);
V v_y_6 = VisualJs.Arg(args,1);
return (new V("") + v_x_5 + new V(",") + v_y_6 + new V(""));
}
private static V F_canPlaceSphere(V[] args) {
V v_s_7 = VisualJs.Arg(args,0);
V v_x_8 = VisualJs.Arg(args,1);
V v_y_9 = VisualJs.Arg(args,2);
V v_ignoreSphere_10 = VisualJs.Arg(args,3);
if (new V(Scope[new V("distance")].Call(VisualJs.Obj(new V("x"), v_x_8, new V("y"), v_y_9), v_s_7[new V("player")][new V("pos")]) < (Scope[new V("TOWER_BODY_RADIUS")] + Scope[new V("PLAYER_BUILD_GAP")])).Truth) return new V(false);
foreach (V v_sphere_11 in v_s_7[new V("spheres")].Items) {
if (VisualJs.Or((!v_sphere_11[new V("alive")]), () => new V(v_sphere_11 == v_ignoreSphere_10)).Truth) continue;
if (new V(Scope[new V("distance")].Call(VisualJs.Obj(new V("x"), v_x_8, new V("y"), v_y_9), v_sphere_11[new V("pos")]) < ((Scope[new V("TOWER_BODY_RADIUS")] * new V(2d)) + Scope[new V("TOWER_PLACEMENT_GAP")])).Truth) return new V(false);
}
foreach (V v_enemy_12 in v_s_7[new V("enemies")].Items) {
if (new V(v_enemy_12[new V("hp")] <= new V(0d)).Truth) continue;
if (new V(Scope[new V("distance")].Call(VisualJs.Obj(new V("x"), v_x_8, new V("y"), v_y_9), v_enemy_12[new V("pos")]) < ((Scope[new V("TOWER_BODY_RADIUS")] + v_enemy_12[new V("radius")]) + Scope[new V("ENEMY_PLACEMENT_GAP")])).Truth) return new V(false);
}
return Scope[new V("hasRouteToOutside")].Call(v_s_7, VisualJs.Obj(new V("x"), v_x_8, new V("y"), v_y_9), v_ignoreSphere_10);
}
private static V F_canRepositionSphere(V[] args) {
V v_s_13 = VisualJs.Arg(args,0);
V v_sphere_14 = VisualJs.Arg(args,1);
V v_x_15 = VisualJs.Arg(args,2);
V v_y_16 = VisualJs.Arg(args,3);
return Scope[new V("canPlaceSphere")].Call(v_s_13, v_x_15, v_y_16, v_sphere_14);
}
private static V F_hasRouteToOutside(V[] args) {
V v_s_17 = VisualJs.Arg(args,0);
V v_candidate_18 = VisualJs.Arg(args,1);
V v_ignoreSphere_19 = VisualJs.Arg(args,2);
V v_obstacles_20 = v_s_17[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_21 => { V v_sphere_22 = VisualJs.Arg(args_21, 0);
return VisualJs.And(v_sphere_22[new V("alive")], () => new V(v_sphere_22 != v_ignoreSphere_19)); }))[new V("map")].Call(VisualJs.Fn(args_23 => { V v_sphere_24 = VisualJs.Arg(args_23, 0);
return v_sphere_24[new V("pos")]; }))[new V("concat")].Call(VisualJs.Arr(v_candidate_18));
V v_minX_25 = ((-v_s_17[new V("worldWidth")]) / new V(2d));
V v_minY_26 = ((-v_s_17[new V("worldHeight")]) / new V(2d));
V v_cols_27 = VisualJs.MathObject[new V("ceil")].Call((v_s_17[new V("worldWidth")] / Scope[new V("NAV_CELL_SIZE")]));
V v_rows_28 = VisualJs.MathObject[new V("ceil")].Call((v_s_17[new V("worldHeight")] / Scope[new V("NAV_CELL_SIZE")]));
V v_toCell_29 = default; v_toCell_29 = VisualJs.Fn(args_30 => { V v_pos_31 = VisualJs.Arg(args_30, 0);
return (VisualJs.Obj(new V("x"), Scope[new V("clampScalar")].Call(VisualJs.MathObject[new V("floor")].Call((((v_pos_31[new V("x")] - v_minX_25)) / Scope[new V("NAV_CELL_SIZE")])), new V(0d), (v_cols_27 - new V(1d))), new V("y"), Scope[new V("clampScalar")].Call(VisualJs.MathObject[new V("floor")].Call((((v_pos_31[new V("y")] - v_minY_26)) / Scope[new V("NAV_CELL_SIZE")])), new V(0d), (v_rows_28 - new V(1d))))); });
V v_isBlocked_32 = default; v_isBlocked_32 = VisualJs.Fn(args_33 => { V v_cx_34 = VisualJs.Arg(args_33, 0);
V v_cy_35 = VisualJs.Arg(args_33, 1);
V v_center_36 = VisualJs.Obj(new V("x"), (v_minX_25 + (((v_cx_34 + new V(0.5d))) * Scope[new V("NAV_CELL_SIZE")])), new V("y"), (v_minY_26 + (((v_cy_35 + new V(0.5d))) * Scope[new V("NAV_CELL_SIZE")])));
return v_obstacles_20[new V("some")].Call(VisualJs.Fn(args_37 => { V v_obstacle_38 = VisualJs.Arg(args_37, 0);
return new V(Scope[new V("distance")].Call(v_center_36, v_obstacle_38) < (Scope[new V("TOWER_BODY_RADIUS")] + Scope[new V("ENEMY_NAV_RADIUS")])); })); });
V v_start_39 = v_toCell_29.Call(v_s_17[new V("player")][new V("pos")]);
if (v_isBlocked_32.Call(v_start_39[new V("x")], v_start_39[new V("y")]).Truth) return new V(false);
V v_queue_40 = VisualJs.Arr(v_start_39);
V v_visited_41 = VisualJs.NewSet(VisualJs.Arr(Scope[new V("cellKey")].Call(v_start_39[new V("x")], v_start_39[new V("y")])));
V v_cursor_42 = new V(0d);
while (new V(v_cursor_42 < v_queue_40[new V("length")]).Truth) {
V v_current_43 = v_queue_40[v_cursor_42++];
if (VisualJs.Or(VisualJs.Or(VisualJs.Or(new V(v_current_43[new V("x")] == new V(0d)), () => new V(v_current_43[new V("y")] == new V(0d))), () => new V(v_current_43[new V("x")] == (v_cols_27 - new V(1d)))), () => new V(v_current_43[new V("y")] == (v_rows_28 - new V(1d)))).Truth) return new V(true);
for (V v_dx_44 = (-new V(1d)); new V(v_dx_44 <= new V(1d)).Truth; v_dx_44++) {
for (V v_dy_45 = (-new V(1d)); new V(v_dy_45 <= new V(1d)).Truth; v_dy_45++) {
if (VisualJs.And(new V(v_dx_44 == new V(0d)), () => new V(v_dy_45 == new V(0d))).Truth) continue;
V v_nx_46 = (v_current_43[new V("x")] + v_dx_44);
V v_ny_47 = (v_current_43[new V("y")] + v_dy_45);
if (VisualJs.Or(VisualJs.Or(VisualJs.Or(new V(v_nx_46 < new V(0d)), () => new V(v_ny_47 < new V(0d))), () => new V(v_nx_46 >= v_cols_27)), () => new V(v_ny_47 >= v_rows_28)).Truth) continue;
V v_key_48 = Scope[new V("cellKey")].Call(v_nx_46, v_ny_47);
if (VisualJs.Or(v_visited_41[new V("has")].Call(v_key_48), () => v_isBlocked_32.Call(v_nx_46, v_ny_47)).Truth) continue;
if (VisualJs.And(new V(v_dx_44 != new V(0d)), () => new V(v_dy_45 != new V(0d))).Truth) {
if (VisualJs.Or(v_isBlocked_32.Call((v_current_43[new V("x")] + v_dx_44), v_current_43[new V("y")]), () => v_isBlocked_32.Call(v_current_43[new V("x")], (v_current_43[new V("y")] + v_dy_45))).Truth) continue;
}
v_visited_41[new V("add")].Call(v_key_48);
v_queue_40[new V("push")].Call(VisualJs.Obj(new V("x"), v_nx_46, new V("y"), v_ny_47));
}
}
}
return new V(false);
}
}
}
#endif
