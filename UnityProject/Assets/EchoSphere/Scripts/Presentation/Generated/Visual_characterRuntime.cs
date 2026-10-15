// Generated from src/characterRuntime.ts; sha256 369ee8da88dda85bdfd10f517b53427ee238997230e457bb682bee96ff648e2c.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_characterRuntime {
public static readonly V Scope=VisualJs.Obj();
static Visual_characterRuntime() {
Scope[new V("distance")] = VisualJs.Fn(F_distance);
Scope[new V("runtimePlayer")] = VisualJs.Fn(F_runtimePlayer);
Scope[new V("getCharacterAttackSpeedMultiplier")] = VisualJs.Fn(F_getCharacterAttackSpeedMultiplier);
Scope[new V("getCharacterRadiusMultiplier")] = VisualJs.Fn(F_getCharacterRadiusMultiplier);
Scope[new V("getCharacterMoveSpeedMultiplier")] = VisualJs.Fn(F_getCharacterMoveSpeedMultiplier);
Scope[new V("getLocalCharacterSpheres")] = VisualJs.Fn(F_getLocalCharacterSpheres);
Scope[new V("getFormationAttackSpeedBonus")] = VisualJs.Fn(F_getFormationAttackSpeedBonus);
Scope[new V("getArchitectFormationBonusScale")] = VisualJs.Fn(F_getArchitectFormationBonusScale);
Scope[new V("getEngineerNetworkRange")] = VisualJs.Fn(F_getEngineerNetworkRange);
Scope[new V("getEngineerNetworkSpheres")] = VisualJs.Fn(F_getEngineerNetworkSpheres);
Scope[new V("getCharacterSphereCopyCap")] = VisualJs.Fn(F_getCharacterSphereCopyCap);
Scope[new V("getSphereCopyOutputMultiplier")] = VisualJs.Fn(F_getSphereCopyOutputMultiplier);
Scope[new V("getCharacterId")] = VisualJs.Fn(F_getCharacterId);
Scope[new V("getConnectedNetworkSizeFromPool")] = VisualJs.Fn(F_getConnectedNetworkSizeFromPool);
Scope[new V("CHARACTER_LOCAL_RADIUS")] = new V(420d);
Scope[new V("CHARACTER_LOCAL_SPHERE_CAP")] = new V(8d);
}
private static V F_distance(V[] args) {
V v_a_0 = VisualJs.Arg(args,0);
V v_b_1 = VisualJs.Arg(args,1);
return VisualJs.MathObject[new V("hypot")].Call((v_a_0[new V("x")] - v_b_1[new V("x")]), (v_a_0[new V("y")] - v_b_1[new V("y")]));
}
private static V F_runtimePlayer(V[] args) {
V v_s_2 = VisualJs.Arg(args,0);
return (v_s_2[new V("player")]);
}
private static V F_getCharacterAttackSpeedMultiplier(V[] args) {
V v_s_3 = VisualJs.Arg(args,0);
V v_character_4 = Scope[new V("getCharacterId")].Call(v_s_3);
V v_mastery_5 = VisualJs.Or(Scope[new V("runtimePlayer")].Call(v_s_3)[new V("characterMasteryLevel")], () => new V(1d));
V v_multiplier_6 = (new V(1d) + Visual_characters.Scope[new V("CHARACTER_DEFS")][v_character_4][new V("baseModifiers")][new V("sphereAttackSpeed")]);
if (VisualJs.And(new V(v_character_4 == new V("conductor")), () => new V((VisualJs.Or(Scope[new V("runtimePlayer")].Call(v_s_3)[new V("conductorOverdriveTimer")], () => new V(0d))) > new V(0d))).Truth) v_multiplier_6 += (new V(v_mastery_5 >= new V(8d)).Truth ? new V(0.28d) : (new V(v_mastery_5 >= new V(4d)).Truth ? new V(0.23d) : new V(0.18d)));
if (new V(v_character_4 == new V("spherist")).Truth) {
V v_sphereCount_7 = v_s_3[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_8 => { V v_sphere_9 = VisualJs.Arg(args_8, 0);
return v_sphere_9[new V("alive")]; }))[new V("length")];
v_multiplier_6 += Visual_characters.Scope[new V("getSphereCountResonanceBonus")].Call(v_character_4, v_sphereCount_7);
if (new V(v_mastery_5 >= new V(4d)).Truth) v_multiplier_6 += (VisualJs.MathObject[new V("max")].Call(new V(0d), (v_sphereCount_7 - new V(1d))) * new V(0.005d));
if (VisualJs.And(new V(v_sphereCount_7 >= new V(8d)), () => new V(v_mastery_5 >= new V(5d))).Truth) v_multiplier_6 += new V(0.05d);
if (VisualJs.And(new V(v_sphereCount_7 >= new V(7d)), () => new V(v_mastery_5 >= new V(7d))).Truth) v_multiplier_6 += new V(0.02d);
if (VisualJs.And(new V(v_sphereCount_7 >= new V(8d)), () => new V(v_mastery_5 >= new V(10d))).Truth) v_multiplier_6 += new V(0.03d);
}
if (new V(v_character_4 == new V("berserker")).Truth) {
v_multiplier_6 += Visual_characters.Scope[new V("getBerserkerFuryBonus")].Call(v_character_4, (v_s_3[new V("player")][new V("hp")] / VisualJs.MathObject[new V("max")].Call(new V(1d), v_s_3[new V("player")][new V("maxHp")])))[new V("attackSpeed")];
if (new V(v_mastery_5 >= new V(3d)).Truth) v_multiplier_6 += new V(0.02d);
}
if (VisualJs.And(new V(v_character_4 == new V("architect")), () => new V(Scope[new V("getLocalCharacterSpheres")].Call(v_s_3)[new V("length")] >= new V(5d))).Truth) {
v_multiplier_6 += Scope[new V("getFormationAttackSpeedBonus")].Call(v_s_3);
}
return v_multiplier_6;
}
private static V F_getCharacterRadiusMultiplier(V[] args) {
V v_s_10 = VisualJs.Arg(args,0);
V v_character_11 = Scope[new V("getCharacterId")].Call(v_s_10);
V v_mastery_12 = VisualJs.Or(Scope[new V("runtimePlayer")].Call(v_s_10)[new V("characterMasteryLevel")], () => new V(1d));
V v_multiplier_13 = (new V(1d) + Visual_characters.Scope[new V("CHARACTER_DEFS")][v_character_11][new V("baseModifiers")][new V("sphereRadius")]);
if (VisualJs.And(VisualJs.And(new V(v_character_11 == new V("spherist")), () => new V(v_mastery_12 >= new V(5d))), () => new V(v_s_10[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_14 => { V v_sphere_15 = VisualJs.Arg(args_14, 0);
return v_sphere_15[new V("alive")]; }))[new V("length")] >= new V(8d))).Truth) {
v_multiplier_13 += new V(0.05d);
}
if (new V(v_character_11 == new V("architect")).Truth) {
if (new V(Scope[new V("getLocalCharacterSpheres")].Call(v_s_10)[new V("length")] >= new V(4d)).Truth) v_multiplier_13 += new V(0.05d);
if (new V(v_mastery_12 >= new V(3d)).Truth) v_multiplier_13 += new V(0.02d);
}
if (VisualJs.And(new V(v_character_11 == new V("engineer")), () => new V(Scope[new V("getEngineerNetworkSpheres")].Call(v_s_10)[new V("length")] >= new V(3d))).Truth) v_multiplier_13 += new V(0.08d);
return v_multiplier_13;
}
private static V F_getCharacterMoveSpeedMultiplier(V[] args) {
V v_s_16 = VisualJs.Arg(args,0);
V v_character_17 = Scope[new V("getCharacterId")].Call(v_s_16);
return (new V(1d) + Visual_characters.Scope[new V("CHARACTER_DEFS")][v_character_17][new V("baseModifiers")][new V("moveSpeed")]);
}
private static V F_getLocalCharacterSpheres(V[] args) {
V v_s_18 = VisualJs.Arg(args,0);
V v_radius_19 = VisualJs.Arg(args,1).Undefined?Scope[new V("CHARACTER_LOCAL_RADIUS")]:VisualJs.Arg(args,1);
return v_s_18[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_20 => { V v_sphere_21 = VisualJs.Arg(args_20, 0);
return VisualJs.And(v_sphere_21[new V("alive")], () => new V(Scope[new V("distance")].Call(v_sphere_21[new V("pos")], v_s_18[new V("player")][new V("pos")]) <= v_radius_19)); }))[new V("sort")].Call(VisualJs.Fn(args_22 => { V v_a_23 = VisualJs.Arg(args_22, 0);
V v_b_24 = VisualJs.Arg(args_22, 1);
return (Scope[new V("distance")].Call(v_a_23[new V("pos")], v_s_18[new V("player")][new V("pos")]) - Scope[new V("distance")].Call(v_b_24[new V("pos")], v_s_18[new V("player")][new V("pos")])); }))[new V("slice")].Call(new V(0d), Scope[new V("CHARACTER_LOCAL_SPHERE_CAP")]);
}
private static V F_getFormationAttackSpeedBonus(V[] args) {
V v_s_25 = VisualJs.Arg(args,0);
if (new V(Scope[new V("getCharacterId")].Call(v_s_25) != new V("architect")).Truth) return new V(0d);
V v_formation_26 = VisualBindings.Get(new V("characterRuntime.ts"),new V("getCharacterFormation")).Call(v_s_25);
if (new V(v_formation_26[new V("type")] != new V("cluster")).Truth) return new V(0d);
return (new V(0.15d) * Scope[new V("getArchitectFormationBonusScale")].Call(v_s_25, v_formation_26[new V("type")]));
}
private static V F_getArchitectFormationBonusScale(V[] args) {
V v_s_27 = VisualJs.Arg(args,0);
V v_type_28 = VisualJs.Arg(args,1);
V v_p_29 = Scope[new V("runtimePlayer")].Call(v_s_27);
V v_scale_30 = new V(1d);
V v_mastery_31 = VisualJs.Or(v_p_29[new V("characterMasteryLevel")], () => new V(1d));
if (VisualJs.And(VisualJs.And(new V(v_mastery_31 >= new V(4d)), () => new V(v_p_29[new V("architectFormationType")] == v_type_28)), () => new V(VisualJs.TypeOf(v_p_29[new V("architectFormationChangedAt")]) == new V("number"))).Truth) {
if (new V((v_s_27[new V("time")] - v_p_29[new V("architectFormationChangedAt")]) <= new V(2d)).Truth) v_scale_30 *= new V(1.25d);
}
if (VisualJs.And(new V(v_mastery_31 >= new V(5d)), () => new V(Scope[new V("getLocalCharacterSpheres")].Call(v_s_27)[new V("length")] >= new V(5d))).Truth) v_scale_30 *= new V(1.05d);
if (new V(v_mastery_31 >= new V(7d)).Truth) v_scale_30 *= new V(1.05d);
if (VisualJs.And(new V(v_mastery_31 >= new V(10d)), () => new V(v_type_28 != new V("none"))).Truth) v_scale_30 *= new V(1.1d);
return v_scale_30;
}
private static V F_getEngineerNetworkRange(V[] args) {
V v_s_32 = VisualJs.Arg(args,0);
V v_mastery_33 = VisualJs.Or(Scope[new V("runtimePlayer")].Call(v_s_32)[new V("characterMasteryLevel")], () => new V(1d));
return (new V(v_mastery_33 >= new V(6d)).Truth ? new V(260d) : (new V(v_mastery_33 >= new V(2d)).Truth ? new V(240d) : new V(220d)));
}
private static V F_getEngineerNetworkSpheres(V[] args) {
V v_s_34 = VisualJs.Arg(args,0);
V v_local_35 = Scope[new V("getLocalCharacterSpheres")].Call(v_s_34);
if (new V(v_local_35[new V("length")] == new V(0d)).Truth) return VisualJs.Arr();
V v_range_36 = Scope[new V("getEngineerNetworkRange")].Call(v_s_34);
V v_best_37 = VisualJs.Arr();
foreach (V v_start_38 in v_local_35.Items) {
V v_connected_39 = Scope[new V("getConnectedNetworkSizeFromPool")].Call(v_start_38, v_local_35, v_range_36);
if (new V(v_connected_39[new V("length")] > v_best_37[new V("length")]).Truth) v_best_37 = v_connected_39;
}
return v_best_37;
}
private static V F_getCharacterSphereCopyCap(V[] args) {
V v_s_40 = VisualJs.Arg(args,0);
V v_sphereType_41 = VisualJs.Arg(args,1);
return Visual_characters.Scope[new V("getCharacterSphereCopyCap")].Call(Scope[new V("getCharacterId")].Call(v_s_40), v_sphereType_41, VisualJs.Or(Scope[new V("runtimePlayer")].Call(v_s_40)[new V("characterMasteryLevel")], () => new V(1d)));
}
private static V F_getSphereCopyOutputMultiplier(V[] args) {
V v_s_42 = VisualJs.Arg(args,0);
V v_sphere_43 = VisualJs.Arg(args,1);
if (new V(Scope[new V("getCharacterSphereCopyCap")].Call(v_s_42, v_sphere_43[new V("type")]) <= new V(2d)).Truth) return new V(1d);
V v_copies_44 = v_s_42[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_45 => { V v_item_46 = VisualJs.Arg(args_45, 0);
return VisualJs.And(v_item_46[new V("alive")], () => new V(v_item_46[new V("type")] == v_sphere_43[new V("type")])); }));
V v_index_47 = v_copies_44[new V("indexOf")].Call(v_sphere_43);
return (new V(v_index_47 < new V(2d)).Truth ? new V(1d) : (new V(v_index_47 == new V(2d)).Truth ? new V(0.85d) : new V(0.7d)));
}
private static V F_getCharacterId(V[] args) {
V v_s_48 = VisualJs.Arg(args,0);
V v_raw_49 = Scope[new V("runtimePlayer")].Call(v_s_48)[new V("characterId")];
return (VisualJs.And(v_raw_49, () => VisualJs.Has(Visual_characters.Scope[new V("CHARACTER_DEFS")],v_raw_49)).Truth ? v_raw_49 : new V("spherist"));
}
private static V F_getConnectedNetworkSizeFromPool(V[] args) {
V v_start_50 = VisualJs.Arg(args,0);
V v_pool_51 = VisualJs.Arg(args,1);
V v_range_52 = VisualJs.Arg(args,2);
if ((!v_pool_51[new V("includes")].Call(v_start_50)).Truth) return VisualJs.Arr();
V v_visited_53 = VisualJs.NewSet(VisualJs.Arr(v_start_50));
V v_queue_54 = VisualJs.Arr(v_start_50);
while (new V(v_queue_54[new V("length")] > new V(0d)).Truth) {
V v_current_55 = (v_queue_54[new V("shift")].Call());
foreach (V v_neighbour_56 in v_pool_51.Items) {
if (v_visited_53[new V("has")].Call(v_neighbour_56).Truth) continue;
if (new V(Scope[new V("distance")].Call(v_neighbour_56[new V("pos")], v_current_55[new V("pos")]) > v_range_52).Truth) continue;
v_visited_53[new V("add")].Call(v_neighbour_56);
v_queue_54[new V("push")].Call(v_neighbour_56);
}
}
return VisualJs.Concat(v_visited_53);
}
}
}
