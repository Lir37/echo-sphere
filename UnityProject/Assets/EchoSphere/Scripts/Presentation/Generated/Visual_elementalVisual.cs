// Generated from src/spheres/elementalVisual.ts; sha256 bf1eb3f594fb42019064c227d0b690aede810df4c2391a72f958add6b526cee5.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_elementalVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_elementalVisual() {
Scope[new V("rgba")] = VisualJs.Fn(F_rgba);
Scope[new V("drawFlame")] = VisualJs.Fn(F_drawFlame);
Scope[new V("drawSnowflake")] = VisualJs.Fn(F_drawSnowflake);
Scope[new V("drawPoisonCloud")] = VisualJs.Fn(F_drawPoisonCloud);
Scope[new V("drawElementParticle")] = VisualJs.Fn(F_drawElementParticle);
Scope[new V("ellipsePoint")] = VisualJs.Fn(F_ellipsePoint);
Scope[new V("drawElementRing")] = VisualJs.Fn(F_drawElementRing);
Scope[new V("drawElementWeaponAccent")] = VisualJs.Fn(F_drawElementWeaponAccent);
Scope[new V("drawSpecialElementalSignature")] = VisualJs.Fn(F_drawSpecialElementalSignature);
Scope[new V("renderSphereElementalVfx")] = VisualJs.Fn(F_renderSphereElementalVfx);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
}
private static V F_rgba(V[] args) {
V v_hex_0 = VisualJs.Arg(args,0);
V v_alpha_1 = VisualJs.Arg(args,1);
V v_n_2 = VisualJs.NumberObject[new V("parseInt")].Call(v_hex_0[new V("slice")].Call(new V(1d)), new V(16d));
return (new V("rgba(") + (((v_n_2 >> (int)new V(16d).Number)) & new V(255d)) + new V(",") + (((v_n_2 >> (int)new V(8d).Number)) & new V(255d)) + new V(",") + (v_n_2 & new V(255d)) + new V(",") + v_alpha_1 + new V(")"));
}
private static V F_drawFlame(V[] args) {
V v_ctx_3 = VisualJs.Arg(args,0);
V v_size_4 = VisualJs.Arg(args,1);
V v_variant_5 = VisualJs.Arg(args,2).Undefined?new V(0d):VisualJs.Arg(args,2);
V v_lean_6 = ((((v_variant_5 - new V(1d))) * v_size_4) * new V(0.12d));
v_ctx_3[new V("beginPath")].Call();
v_ctx_3[new V("moveTo")].Call(v_lean_6, (-v_size_4));
v_ctx_3[new V("quadraticCurveTo")].Call((v_size_4 * ((new V(0.92d) + (v_variant_5 * new V(0.06d))))), ((-v_size_4) * new V(0.2d)), (v_size_4 * ((new V(0.44d) - (v_variant_5 * new V(0.05d))))), (v_size_4 * new V(0.55d)));
v_ctx_3[new V("quadraticCurveTo")].Call(v_lean_6, (v_size_4 * new V(0.92d)), ((-v_size_4) * ((new V(0.46d) + (v_variant_5 * new V(0.03d))))), (v_size_4 * new V(0.58d)));
v_ctx_3[new V("quadraticCurveTo")].Call(((-v_size_4) * ((new V(0.9d) - (v_variant_5 * new V(0.04d))))), ((-v_size_4) * new V(0.18d)), v_lean_6, (-v_size_4));
v_ctx_3[new V("closePath")].Call();
v_ctx_3[new V("stroke")].Call();
v_ctx_3[new V("beginPath")].Call();
v_ctx_3[new V("moveTo")].Call(v_lean_6, ((-v_size_4) * new V(0.34d)));
v_ctx_3[new V("quadraticCurveTo")].Call((v_size_4 * new V(0.4d)), ((-v_size_4) * new V(0.02d)), (v_lean_6 + (v_size_4 * new V(0.04d))), (v_size_4 * new V(0.46d)));
v_ctx_3[new V("quadraticCurveTo")].Call(((-v_size_4) * new V(0.3d)), (v_size_4 * new V(0.18d)), v_lean_6, ((-v_size_4) * new V(0.34d)));
v_ctx_3[new V("stroke")].Call();
return default;
}
private static V F_drawSnowflake(V[] args) {
V v_ctx_7 = VisualJs.Arg(args,0);
V v_size_8 = VisualJs.Arg(args,1);
V v_variant_9 = VisualJs.Arg(args,2).Undefined?new V(0d):VisualJs.Arg(args,2);
V v_arms_10 = (new V(v_variant_9 == new V(1d)).Truth ? new V(4d) : new V(6d));
for (V v_i_11 = new V(0d); new V(v_i_11 < v_arms_10).Truth; v_i_11++) {
V v_a_12 = ((v_i_11 * Scope[new V("TAU")]) / v_arms_10);
v_ctx_7[new V("beginPath")].Call();
v_ctx_7[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_12) * v_size_8) * new V(0.14d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_12) * v_size_8) * new V(0.14d)));
v_ctx_7[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_12) * v_size_8), (VisualJs.MathObject[new V("sin")].Call(v_a_12) * v_size_8));
v_ctx_7[new V("stroke")].Call();
if (new V(v_variant_9 != new V(2d)).Truth) {
V v_tip_13 = (new V(v_variant_9 == new V(1d)).Truth ? new V(0.68d) : new V(0.62d));
V v_branch_14 = (new V(v_variant_9 == new V(1d)).Truth ? new V(0.42d) : new V(0.34d));
v_ctx_7[new V("beginPath")].Call();
v_ctx_7[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_12) * v_size_8) * v_tip_13), ((VisualJs.MathObject[new V("sin")].Call(v_a_12) * v_size_8) * v_tip_13));
v_ctx_7[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call((v_a_12 + v_branch_14)) * v_size_8) * new V(0.88d)), ((VisualJs.MathObject[new V("sin")].Call((v_a_12 + v_branch_14)) * v_size_8) * new V(0.88d)));
v_ctx_7[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_12) * v_size_8) * v_tip_13), ((VisualJs.MathObject[new V("sin")].Call(v_a_12) * v_size_8) * v_tip_13));
v_ctx_7[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call((v_a_12 - v_branch_14)) * v_size_8) * new V(0.88d)), ((VisualJs.MathObject[new V("sin")].Call((v_a_12 - v_branch_14)) * v_size_8) * new V(0.88d)));
v_ctx_7[new V("stroke")].Call();
}
}
v_ctx_7[new V("beginPath")].Call();
v_ctx_7[new V("arc")].Call(new V(0d), new V(0d), (v_size_8 * ((new V(v_variant_9 == new V(2d)).Truth ? new V(0.28d) : new V(0.2d)))), new V(0d), Scope[new V("TAU")]);
v_ctx_7[new V("stroke")].Call();
return default;
}
private static V F_drawPoisonCloud(V[] args) {
V v_ctx_15 = VisualJs.Arg(args,0);
V v_size_16 = VisualJs.Arg(args,1);
V v_variant_17 = VisualJs.Arg(args,2).Undefined?new V(0d):VisualJs.Arg(args,2);
V v_drift_18 = ((((v_variant_17 - new V(1d))) * v_size_16) * new V(0.08d));
v_ctx_15[new V("beginPath")].Call();
v_ctx_15[new V("arc")].Call((((-v_size_16) * new V(0.34d)) + v_drift_18), (v_size_16 * new V(0.1d)), (v_size_16 * new V(0.34d)), new V(0d), Scope[new V("TAU")]);
v_ctx_15[new V("arc")].Call(new V(0d), ((-v_size_16) * ((new V(0.12d) + (v_variant_17 * new V(0.04d))))), (v_size_16 * ((new V(0.42d) - (v_variant_17 * new V(0.03d))))), new V(0d), Scope[new V("TAU")]);
v_ctx_15[new V("arc")].Call(((v_size_16 * new V(0.34d)) + v_drift_18), (v_size_16 * new V(0.08d)), (v_size_16 * ((new V(0.29d) + (v_variant_17 * new V(0.02d))))), new V(0d), Scope[new V("TAU")]);
v_ctx_15[new V("stroke")].Call();
v_ctx_15[new V("beginPath")].Call();
v_ctx_15[new V("arc")].Call((v_size_16 * ((new V(0.1d) - (v_variant_17 * new V(0.04d))))), ((-v_size_16) * ((new V(0.5d) + (v_variant_17 * new V(0.04d))))), (v_size_16 * ((new V(0.11d) + (v_variant_17 * new V(0.02d))))), new V(0d), Scope[new V("TAU")]);
v_ctx_15[new V("arc")].Call(((-v_size_16) * new V(0.22d)), ((-v_size_16) * new V(0.42d)), (v_size_16 * new V(0.07d)), new V(0d), Scope[new V("TAU")]);
v_ctx_15[new V("stroke")].Call();
return default;
}
private static V F_drawElementParticle(V[] args) {
V v_ctx_19 = VisualJs.Arg(args,0);
V v_element_20 = VisualJs.Arg(args,1);
V v_x_21 = VisualJs.Arg(args,2);
V v_y_22 = VisualJs.Arg(args,3);
V v_size_23 = VisualJs.Arg(args,4);
V v_angle_24 = VisualJs.Arg(args,5);
V v_alpha_25 = VisualJs.Arg(args,6);
V v_variant_26 = VisualJs.Arg(args,7).Undefined?new V(0d):VisualJs.Arg(args,7);
v_ctx_19[new V("save")].Call();
v_ctx_19[new V("translate")].Call(v_x_21, v_y_22);
v_ctx_19[new V("rotate")].Call((v_angle_24 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",v_alpha_25);
if (new V(v_element_20 == new V("fire")).Truth) Scope[new V("drawFlame")].Call(v_ctx_19, v_size_23, v_variant_26); else if (new V(v_element_20 == new V("freeze")).Truth) Scope[new V("drawSnowflake")].Call(v_ctx_19, v_size_23, v_variant_26); else Scope[new V("drawPoisonCloud")].Call(v_ctx_19, v_size_23, v_variant_26);
v_ctx_19[new V("restore")].Call();
return default;
}
private static V F_ellipsePoint(V[] args) {
V v_radiusX_27 = VisualJs.Arg(args,0);
V v_radiusY_28 = VisualJs.Arg(args,1);
V v_angle_29 = VisualJs.Arg(args,2);
return VisualJs.Obj(new V("x"), (VisualJs.MathObject[new V("cos")].Call(v_angle_29) * v_radiusX_27), new V("y"), (VisualJs.MathObject[new V("sin")].Call(v_angle_29) * v_radiusY_28));
}
private static V F_drawElementRing(V[] args) {
V v_ctx_30 = VisualJs.Arg(args,0);
V v_element_31 = VisualJs.Arg(args,1);
V v_radius_32 = VisualJs.Arg(args,2);
V v_time_33 = VisualJs.Arg(args,3);
V v_active_34 = VisualJs.Arg(args,4);
V v_scale_35 = VisualJs.Arg(args,5);
V v_color_36 = Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_element_31][new V("color")];
V v_ringRx_37 = (v_radius_32 * new V(1.16d));
V v_ringRy_38 = (v_radius_32 * new V(0.34d));
V v_ringSpin_39 = ((-v_time_33) * new V(0.18d));
V v_particleSpin_40 = ((-v_time_33) * new V(0.34d));
V v_ringAlpha_41 = (v_active_34.Truth ? new V(0.78d) : new V(0.2d));
V v_mainWidth_42 = VisualJs.MathObject[new V("max")].Call(new V(1d), (v_radius_32 * new V(0.04d)));
v_ctx_30[new V("save")].Call();
VisualJs.Assign(v_ctx_30,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_30,new V("strokeStyle"),"=",Scope[new V("rgba")].Call(v_color_36, (v_ringAlpha_41 * new V(0.18d))));
VisualJs.Assign(v_ctx_30,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(2d), (v_mainWidth_42 * new V(3d))));
v_ctx_30[new V("beginPath")].Call();
v_ctx_30[new V("ellipse")].Call(new V(0d), new V(0d), v_ringRx_37, v_ringRy_38, v_ringSpin_39, new V(0d), Scope[new V("TAU")]);
v_ctx_30[new V("stroke")].Call();
VisualJs.Assign(v_ctx_30,new V("strokeStyle"),"=",Scope[new V("rgba")].Call(v_color_36, (v_ringAlpha_41 * new V(0.58d))));
VisualJs.Assign(v_ctx_30,new V("lineWidth"),"=",v_mainWidth_42);
v_ctx_30[new V("beginPath")].Call();
v_ctx_30[new V("ellipse")].Call(new V(0d), new V(0d), v_ringRx_37, v_ringRy_38, v_ringSpin_39, VisualJs.MathObject[new V("PI")], Scope[new V("TAU")]);
v_ctx_30[new V("stroke")].Call();
VisualJs.Assign(v_ctx_30,new V("strokeStyle"),"=",Scope[new V("rgba")].Call(v_color_36, v_ringAlpha_41));
VisualJs.Assign(v_ctx_30,new V("lineWidth"),"=",v_mainWidth_42);
v_ctx_30[new V("beginPath")].Call();
v_ctx_30[new V("ellipse")].Call(new V(0d), new V(0d), v_ringRx_37, v_ringRy_38, v_ringSpin_39, new V(0d), VisualJs.MathObject[new V("PI")]);
v_ctx_30[new V("stroke")].Call();
VisualJs.Assign(v_ctx_30,new V("strokeStyle"),"=",Scope[new V("rgba")].Call(v_color_36, (v_ringAlpha_41 * new V(0.72d))));
VisualJs.Assign(v_ctx_30,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.75d), (v_mainWidth_42 * new V(0.55d))));
v_ctx_30[new V("beginPath")].Call();
v_ctx_30[new V("ellipse")].Call(new V(0d), new V(0d), (v_ringRx_37 * new V(0.995d)), (v_ringRy_38 * new V(0.995d)), v_ringSpin_39, (v_particleSpin_40 % Scope[new V("TAU")]), ((v_particleSpin_40 % Scope[new V("TAU")]) + new V(0.72d)));
v_ctx_30[new V("stroke")].Call();
V v_particleCount_43 = new V(6d);
for (V v_i_44 = new V(0d); new V(v_i_44 < v_particleCount_43).Truth; v_i_44++) {
V v_a_45 = (v_particleSpin_40 + ((v_i_44 * Scope[new V("TAU")]) / v_particleCount_43));
V v_p_46 = Scope[new V("ellipsePoint")].Call(v_ringRx_37, v_ringRy_38, v_a_45);
V v_depth_47 = (new V(0.38d) + (new V(0.62d) * ((((VisualJs.MathObject[new V("sin")].Call(v_a_45) + new V(1d))) * new V(0.5d)))));
V v_bob_48 = ((VisualJs.MathObject[new V("sin")].Call(((v_time_33 * new V(2.8d)) + (v_i_44 * new V(1.73d)))) * v_radius_32) * new V(0.018d));
V v_size_49 = VisualJs.MathObject[new V("max")].Call(new V(2.2d), (v_radius_32 * ((new V(0.055d) + (((v_i_44 % new V(3d))) * new V(0.008d))))));
Scope[new V("drawElementParticle")].Call(v_ctx_30, v_element_31, v_p_46[new V("x")], (v_p_46[new V("y")] + v_bob_48), v_size_49, v_a_45, (v_ringAlpha_41 * ((new V(0.72d) + (v_depth_47 * new V(0.28d))))), (v_i_44 % new V(3d)));
if (new V((v_i_44 % new V(2d)) == new V(0d)).Truth) {
V v_side_50 = (new V((v_i_44 % new V(4d)) == new V(0d)).Truth ? new V(1d) : (-new V(1d)));
V v_moteA_51 = (v_a_45 + (v_side_50 * new V(0.16d)));
V v_mote_52 = Scope[new V("ellipsePoint")].Call((v_ringRx_37 * new V(1.035d)), (v_ringRy_38 * new V(1.18d)), v_moteA_51);
v_ctx_30[new V("save")].Call();
VisualJs.Assign(v_ctx_30,new V("globalAlpha"),"=",(v_ringAlpha_41 * ((new V(0.28d) + (v_depth_47 * new V(0.24d))))));
VisualJs.Assign(v_ctx_30,new V("fillStyle"),"=",v_color_36);
v_ctx_30[new V("beginPath")].Call();
v_ctx_30[new V("arc")].Call(v_mote_52[new V("x")], v_mote_52[new V("y")], VisualJs.MathObject[new V("max")].Call(new V(0.65d), (v_radius_32 * new V(0.018d))), new V(0d), Scope[new V("TAU")]);
v_ctx_30[new V("fill")].Call();
v_ctx_30[new V("restore")].Call();
}
}
v_ctx_30[new V("restore")].Call();
return default;
}
private static V F_drawElementWeaponAccent(V[] args) {
V v_ctx_53 = VisualJs.Arg(args,0);
V v_element_54 = VisualJs.Arg(args,1);
V v_sphere_55 = VisualJs.Arg(args,2);
V v_radius_56 = VisualJs.Arg(args,3);
V v_active_57 = VisualJs.Arg(args,4);
V v_scale_58 = VisualJs.Arg(args,5);
V v_time_59 = VisualJs.Arg(args,6);
if (new V(v_sphere_55[new V("type")] == new V("orbital")).Truth) return default;
V v_color_60 = Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_element_54][new V("color")];
V v_direction_61 = (VisualJs.NumberObject[new V("isFinite")].Call(v_sphere_55[new V("rotation")]).Truth ? v_sphere_55[new V("rotation")] : new V(0d));
V v_pulse_62 = (new V(0.75d) + (new V(0.25d) * VisualJs.MathObject[new V("sin")].Call(((v_time_59 * new V(8d)) + (v_sphere_55[new V("pos")][new V("x")] * new V(0.01d))))));
V v_reach_63 = (v_radius_56 * new V(1.05d));
v_ctx_53[new V("save")].Call();
v_ctx_53[new V("translate")].Call((v_sphere_55[new V("pos")][new V("x")] + (VisualJs.MathObject[new V("cos")].Call(v_direction_61) * v_reach_63)), (v_sphere_55[new V("pos")][new V("y")] + (VisualJs.MathObject[new V("sin")].Call(v_direction_61) * v_reach_63)));
v_ctx_53[new V("rotate")].Call(v_direction_61);
VisualJs.Assign(v_ctx_53,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_53,new V("globalAlpha"),"=",(((v_active_57.Truth ? new V(0.72d) : new V(0.18d))) * v_pulse_62));
VisualJs.Assign(v_ctx_53,new V("strokeStyle"),"=",v_color_60);
VisualJs.Assign(v_ctx_53,new V("fillStyle"),"=",Scope[new V("rgba")].Call(v_color_60, new V(0.16d)));
VisualJs.Assign(v_ctx_53,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.85d), (new V(1.1d) * v_scale_58)));
if (new V(v_element_54 == new V("fire")).Truth) Scope[new V("drawFlame")].Call(v_ctx_53, (v_radius_56 * new V(0.12d))); else if (new V(v_element_54 == new V("freeze")).Truth) Scope[new V("drawSnowflake")].Call(v_ctx_53, (v_radius_56 * new V(0.11d))); else Scope[new V("drawPoisonCloud")].Call(v_ctx_53, (v_radius_56 * new V(0.1d)));
VisualJs.Assign(v_ctx_53,new V("globalAlpha"),"*=",new V(0.56d));
v_ctx_53[new V("beginPath")].Call();
v_ctx_53[new V("arc")].Call((v_radius_56 * new V(0.1d)), new V(0d), (v_radius_56 * new V(0.1d)), new V(0d), Scope[new V("TAU")]);
v_ctx_53[new V("fill")].Call();
v_ctx_53[new V("restore")].Call();
return default;
}
private static V F_drawSpecialElementalSignature(V[] args) {
V v_ctx_64 = VisualJs.Arg(args,0);
V v_sphere_65 = VisualJs.Arg(args,1);
V v_element_66 = VisualJs.Arg(args,2);
V v_radius_67 = VisualJs.Arg(args,3);
V v_time_68 = VisualJs.Arg(args,4);
V v_active_69 = VisualJs.Arg(args,5);
if ((!VisualJs.Arr(new V("chain"), new V("aura"), new V("gravity"), new V("pulse"))[new V("includes")].Call(v_sphere_65[new V("type")])).Truth) return default;
V v_color_70 = Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_element_66][new V("color")];
V v_alpha_71 = (v_active_69.Truth ? new V(0.42d) : new V(0.1d));
v_ctx_64[new V("save")].Call();
VisualJs.Assign(v_ctx_64,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_64,new V("strokeStyle"),"=",v_color_70);
VisualJs.Assign(v_ctx_64,new V("fillStyle"),"=",Scope[new V("rgba")].Call(v_color_70, (v_alpha_71 * new V(0.18d))));
VisualJs.Assign(v_ctx_64,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_radius_67 * new V(0.024d))));
if (new V(v_sphere_65[new V("type")] == new V("chain")).Truth) {
for (V v_i_72 = new V(0d); new V(v_i_72 < new V(3d)).Truth; v_i_72++) {
V v_a_73 = ((v_time_68 * new V(0.9d)) + ((v_i_72 * Scope[new V("TAU")]) / new V(3d)));
V v_x_74 = ((VisualJs.MathObject[new V("cos")].Call(v_a_73) * v_radius_67) * new V(1.02d));
V v_y_75 = ((VisualJs.MathObject[new V("sin")].Call(v_a_73) * v_radius_67) * new V(0.3d));
v_ctx_64[new V("beginPath")].Call();
v_ctx_64[new V("moveTo")].Call(v_x_74, v_y_75);
v_ctx_64[new V("lineTo")].Call((v_x_74 + ((VisualJs.MathObject[new V("cos")].Call((v_a_73 + new V(1.2d))) * v_radius_67) * new V(0.18d))), (v_y_75 + ((VisualJs.MathObject[new V("sin")].Call((v_a_73 + new V(1.2d))) * v_radius_67) * new V(0.12d))));
v_ctx_64[new V("stroke")].Call();
}
} else if (new V(v_sphere_65[new V("type")] == new V("aura")).Truth) {
VisualJs.Assign(v_ctx_64,new V("globalAlpha"),"=",(v_alpha_71 * new V(0.72d)));
v_ctx_64[new V("beginPath")].Call();
v_ctx_64[new V("ellipse")].Call(new V(0d), new V(0d), (v_radius_67 * new V(1.85d)), (v_radius_67 * new V(0.58d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_64[new V("stroke")].Call();
for (V v_i_76 = new V(0d); new V(v_i_76 < new V(3d)).Truth; v_i_76++) {
V v_a_77 = (((-v_time_68) * new V(0.45d)) + ((v_i_76 * Scope[new V("TAU")]) / new V(3d)));
V v_p_78 = Scope[new V("ellipsePoint")].Call((v_radius_67 * new V(1.55d)), (v_radius_67 * new V(0.48d)), v_a_77);
Scope[new V("drawElementParticle")].Call(v_ctx_64, v_element_66, v_p_78[new V("x")], v_p_78[new V("y")], (v_radius_67 * new V(0.065d)), v_a_77, (v_alpha_71 * new V(0.65d)), v_i_76);
}
} else if (new V(v_sphere_65[new V("type")] == new V("gravity")).Truth) {
for (V v_i_79 = new V(0d); new V(v_i_79 < new V(3d)).Truth; v_i_79++) {
V v_q_80 = (((((((v_time_68 * new V(0.35d)) + (v_i_79 / new V(3d)))) % new V(1d)) + new V(1d))) % new V(1d));
V v_rr_81 = (v_radius_67 * ((new V(1.72d) - (v_q_80 * new V(0.72d)))));
VisualJs.Assign(v_ctx_64,new V("globalAlpha"),"=",((v_alpha_71 * ((new V(1d) - v_q_80))) * new V(0.8d)));
v_ctx_64[new V("beginPath")].Call();
v_ctx_64[new V("arc")].Call(new V(0d), new V(0d), v_rr_81, ((-new V(0.55d)) + (v_i_79 * new V(2d))), (new V(0.55d) + (v_i_79 * new V(2d))));
v_ctx_64[new V("stroke")].Call();
}
} else {
V v_q_82 = ((((((v_time_68 * new V(0.72d))) % new V(1d)) + new V(1d))) % new V(1d));
VisualJs.Assign(v_ctx_64,new V("globalAlpha"),"=",(v_alpha_71 * ((new V(1d) - v_q_82))));
v_ctx_64[new V("beginPath")].Call();
v_ctx_64[new V("arc")].Call(new V(0d), new V(0d), (v_radius_67 * ((new V(0.72d) + (v_q_82 * new V(1.15d))))), new V(0d), Scope[new V("TAU")]);
v_ctx_64[new V("stroke")].Call();
}
v_ctx_64[new V("restore")].Call();
return default;
}
private static V F_renderSphereElementalVfx(V[] args) {
V v_ctx_83 = VisualJs.Arg(args,0);
V v_sphere_84 = VisualJs.Arg(args,1);
V v_player_85 = VisualJs.Arg(args,2);
V v_time_86 = VisualJs.Arg(args,3);
V v_scale_87 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_branch_88 = VisualJs.Chain(v_player_85,optional_89=>VisualJs.Chain(optional_89[new V("sphereBranches")],optional_90=>optional_90[v_sphere_84[new V("type")]]));
V v_element_91 = Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_88);
if (VisualJs.And(new V(v_sphere_84[new V("type")] == new V("prism")), () => new V(v_branch_88 == new V("prism_spectrum"))).Truth) {
V v_finalId_92 = (VisualJs.Or(VisualJs.Chain(v_player_85,optional_93=>optional_93[new V("evolutions")]), () => VisualJs.Arr()))[new V("find")].Call(VisualJs.Fn(args_94 => { V v_id_95 = VisualJs.Arg(args_94, 0);
return v_id_95[new V("startsWith")].Call(new V("sphere:prism:7:")); }));
V v_finalIndex_96 = (v_finalId_92.Truth ? VisualJs.Number(v_finalId_92[new V("split")].Call(new V(":"))[new V("pop")].Call()) : VisualJs.Null);
v_element_91 = (new V(v_finalIndex_96 == new V(1d)).Truth ? new V("freeze") : (new V(v_finalIndex_96 == new V(2d)).Truth ? new V("poison") : new V("fire")));
}
if ((!v_element_91).Truth) return default;
V v_active_97 = VisualJs.And(v_sphere_84[new V("alive")], () => new V(v_sphere_84[new V("networkDisabledTimer")] <= new V(0d)));
V v_radius_98 = ((new V(24d) * v_scale_87) * ((new V(v_sphere_84[new V("type")] == new V("orbital")).Truth ? new V(1.46d) : new V(1.34d))));
v_ctx_83[new V("save")].Call();
v_ctx_83[new V("translate")].Call(v_sphere_84[new V("pos")][new V("x")], v_sphere_84[new V("pos")][new V("y")]);
Scope[new V("drawElementRing")].Call(v_ctx_83, v_element_91, v_radius_98, v_time_86, v_active_97, v_scale_87);
Scope[new V("drawSpecialElementalSignature")].Call(v_ctx_83, v_sphere_84, v_element_91, v_radius_98, v_time_86, v_active_97);
v_ctx_83[new V("restore")].Call();
Scope[new V("drawElementWeaponAccent")].Call(v_ctx_83, v_element_91, v_sphere_84, (new V(24d) * v_scale_87), v_active_97, v_scale_87, v_time_86);
return default;
}
}
}
