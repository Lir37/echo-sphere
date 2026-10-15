// Generated from src/spheres/orbitalVisual.ts; sha256 5f6a998ffb026a4c89085cf636725b199c3d3ce5c31a039fe1fbfd20ce49a101.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_orbitalVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_orbitalVisual() {
Scope[new V("stateOf")] = VisualJs.Fn(F_stateOf);
Scope[new V("update")] = VisualJs.Fn(F_update);
Scope[new V("drawOrbitalTerminal")] = VisualJs.Fn(F_drawOrbitalTerminal);
Scope[new V("rgbaColor")] = VisualJs.Fn(F_rgbaColor);
Scope[new V("drawOrbitalElementTrail")] = VisualJs.Fn(F_drawOrbitalElementTrail);
Scope[new V("drawSatellite")] = VisualJs.Fn(F_drawSatellite);
Scope[new V("renderOrbitalSphereRuntimeVfx")] = VisualJs.Fn(F_renderOrbitalSphereRuntimeVfx);
Scope[new V("renderOrbitalSphereAttackersVfx")] = VisualJs.Fn(F_renderOrbitalSphereAttackersVfx);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
Scope[new V("BASE")] = new V("#8ef0ff");
Scope[new V("RESONANCE")] = new V("#ffd166");
Scope[new V("DISABLED")] = new V("#ff4d70");
Scope[new V("STATES")] = VisualJs.CreateMap(default(V),true);
}
private static V F_stateOf(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_v_1 = Scope[new V("STATES")][new V("get")].Call(v_s_0);
if (v_v_1.Truth) return v_v_1;
v_v_1 = VisualJs.Obj(new V("lastTime"), new V(0d), new V("lastAura"), (VisualJs.NumberObject[new V("isFinite")].Call(v_s_0[new V("auraTimer")]).Truth ? v_s_0[new V("auraTimer")] : new V(0d)), new V("attack"), new V(0d), new V("resonance"), new V(0d), new V("phase"), (((((((v_s_0[new V("pos")][new V("x")] * new V(0.011d)) + (v_s_0[new V("pos")][new V("y")] * new V(0.007d)))) % Scope[new V("TAU")]) + Scope[new V("TAU")])) % Scope[new V("TAU")]));
Scope[new V("STATES")][new V("set")].Call(v_s_0, v_v_1);
return v_v_1;
}
private static V F_update(V[] args) {
V v_s_2 = VisualJs.Arg(args,0);
V v_p_3 = VisualJs.Arg(args,1);
V v_t_4 = VisualJs.Arg(args,2);
V v_v_5 = Scope[new V("stateOf")].Call(v_s_2);
if (VisualJs.And(new V(v_v_5[new V("lastTime")] == v_t_4), () => new V(v_v_5[new V("lastTime")] != new V(0d))).Truth) return v_v_5;
V v_dt_6 = (new V(v_v_5[new V("lastTime")] > new V(0d)).Truth ? VisualJs.MathObject[new V("min")].Call(new V(0.05d), VisualJs.MathObject[new V("max")].Call(new V(0d), (v_t_4 - v_v_5[new V("lastTime")]))) : new V(0d));
VisualJs.Assign(v_v_5,new V("lastTime"),"=",v_t_4);
V v_disabled_7 = VisualJs.Or(new V(v_s_2[new V("networkDisabledTimer")] > new V(0d)), () => (!v_s_2[new V("alive")]));
if ((!v_disabled_7).Truth) {
V v_aura_8 = (VisualJs.NumberObject[new V("isFinite")].Call(v_s_2[new V("auraTimer")]).Truth ? v_s_2[new V("auraTimer")] : new V(0d));
if (new V(v_aura_8 > (v_v_5[new V("lastAura")] + new V(0.12d))).Truth) VisualJs.Assign(v_v_5,new V("attack"),"=",new V(0.28d));
VisualJs.Assign(v_v_5,new V("lastAura"),"=",v_aura_8);
VisualJs.Assign(v_v_5,new V("attack"),"=",VisualJs.MathObject[new V("max")].Call(new V(0d), (v_v_5[new V("attack")] - v_dt_6)));
if (VisualJs.Chain(v_p_3,optional_9=>optional_9[new V("resonanceEventActive")]).Truth) VisualJs.Assign(v_v_5,new V("resonance"),"=",new V(1.15d));
VisualJs.Assign(v_v_5,new V("resonance"),"=",VisualJs.MathObject[new V("max")].Call(new V(0d), (v_v_5[new V("resonance")] - v_dt_6)));
}
return v_v_5;
}
private static V F_drawOrbitalTerminal(V[] args) {
V v_ctx_10 = VisualJs.Arg(args,0);
V v_r_11 = VisualJs.Arg(args,1);
V v_y_12 = VisualJs.Arg(args,2);
V v_color_13 = VisualJs.Arg(args,3);
V v_alpha_14 = VisualJs.Arg(args,4);
V v_phase_15 = VisualJs.Arg(args,5);
v_ctx_10[new V("save")].Call();
v_ctx_10[new V("translate")].Call(new V(0d), v_y_12);
v_ctx_10[new V("rotate")].Call((v_phase_15 * new V(0.08d)));
VisualJs.Assign(v_ctx_10,new V("globalAlpha"),"=",v_alpha_14);
VisualJs.Assign(v_ctx_10,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_10,new V("strokeStyle"),"=",v_color_13);
VisualJs.Assign(v_ctx_10,new V("lineWidth"),"=",new V(1d));
v_ctx_10[new V("beginPath")].Call();
v_ctx_10[new V("moveTo")].Call(new V(0d), ((-v_r_11) * new V(0.28d)));
v_ctx_10[new V("lineTo")].Call((v_r_11 * new V(0.16d)), ((-v_r_11) * new V(0.05d)));
v_ctx_10[new V("lineTo")].Call((v_r_11 * new V(0.11d)), (v_r_11 * new V(0.24d)));
v_ctx_10[new V("lineTo")].Call(new V(0d), (v_r_11 * new V(0.31d)));
v_ctx_10[new V("lineTo")].Call(((-v_r_11) * new V(0.11d)), (v_r_11 * new V(0.24d)));
v_ctx_10[new V("lineTo")].Call(((-v_r_11) * new V(0.16d)), ((-v_r_11) * new V(0.05d)));
v_ctx_10[new V("closePath")].Call();
v_ctx_10[new V("fill")].Call();
v_ctx_10[new V("stroke")].Call();
VisualJs.Assign(v_ctx_10,new V("globalAlpha"),"=",(v_alpha_14 * new V(0.72d)));
VisualJs.Assign(v_ctx_10,new V("fillStyle"),"=",new V("#ffffff"));
v_ctx_10[new V("beginPath")].Call();
v_ctx_10[new V("moveTo")].Call(new V(0d), ((-v_r_11) * new V(0.16d)));
v_ctx_10[new V("lineTo")].Call((v_r_11 * new V(0.07d)), new V(0d));
v_ctx_10[new V("lineTo")].Call(new V(0d), (v_r_11 * new V(0.14d)));
v_ctx_10[new V("lineTo")].Call(((-v_r_11) * new V(0.07d)), new V(0d));
v_ctx_10[new V("closePath")].Call();
v_ctx_10[new V("fill")].Call();
v_ctx_10[new V("restore")].Call();
return default;
}
private static V F_rgbaColor(V[] args) {
V v_hex_16 = VisualJs.Arg(args,0);
V v_alpha_17 = VisualJs.Arg(args,1);
V v_n_18 = VisualJs.NumberObject[new V("parseInt")].Call(v_hex_16[new V("slice")].Call(new V(1d)), new V(16d));
return (new V("rgba(") + (((v_n_18 >> (int)new V(16d).Number)) & new V(255d)) + new V(",") + (((v_n_18 >> (int)new V(8d).Number)) & new V(255d)) + new V(",") + (v_n_18 & new V(255d)) + new V(",") + v_alpha_17 + new V(")"));
}
private static V F_drawOrbitalElementTrail(V[] args) {
V v_ctx_19 = VisualJs.Arg(args,0);
V v_element_20 = VisualJs.Arg(args,1);
V v_size_21 = VisualJs.Arg(args,2);
V v_angle_22 = VisualJs.Arg(args,3);
V v_alpha_23 = VisualJs.Arg(args,4);
V v_blade_24 = VisualJs.Arg(args,5);
V v_color_25 = Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_element_20][new V("color")];
V v_trailScale_26 = (v_blade_24.Truth ? new V(1d) : new V(0.86d));
v_ctx_19[new V("save")].Call();
v_ctx_19[new V("rotate")].Call(v_angle_22);
VisualJs.Assign(v_ctx_19,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_19,new V("strokeStyle"),"=",v_color_25);
VisualJs.Assign(v_ctx_19,new V("fillStyle"),"=",Scope[new V("rgbaColor")].Call(v_color_25, (v_alpha_23 * new V(0.26d))));
VisualJs.Assign(v_ctx_19,new V("lineCap"),"=",new V("round"));
VisualJs.Assign(v_ctx_19,new V("lineJoin"),"=",new V("round"));
if (new V(v_element_20 == new V("fire")).Truth) {
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.82d)));
VisualJs.Assign(v_ctx_19,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1.1d), (v_size_21 * new V(0.12d))));
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("moveTo")].Call((((-v_size_21) * new V(0.34d)) * v_trailScale_26), new V(0d));
v_ctx_19[new V("quadraticCurveTo")].Call((((-v_size_21) * new V(0.86d)) * v_trailScale_26), ((-v_size_21) * new V(0.18d)), (((-v_size_21) * new V(1.28d)) * v_trailScale_26), new V(0d));
v_ctx_19[new V("quadraticCurveTo")].Call((((-v_size_21) * new V(0.86d)) * v_trailScale_26), (v_size_21 * new V(0.18d)), (((-v_size_21) * new V(0.34d)) * v_trailScale_26), new V(0d));
v_ctx_19[new V("stroke")].Call();
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.68d)));
VisualJs.Assign(v_ctx_19,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.9d), (v_size_21 * new V(0.08d))));
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("moveTo")].Call(((-v_size_21) * new V(0.5d)), ((-v_size_21) * new V(0.08d)));
v_ctx_19[new V("lineTo")].Call(((-v_size_21) * new V(0.96d)), new V(0d));
v_ctx_19[new V("lineTo")].Call(((-v_size_21) * new V(0.54d)), (v_size_21 * new V(0.1d)));
v_ctx_19[new V("stroke")].Call();
} else if (new V(v_element_20 == new V("freeze")).Truth) {
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.86d)));
VisualJs.Assign(v_ctx_19,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_size_21 * new V(0.1d))));
foreach (V v_side_27 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("moveTo")].Call((((-v_size_21) * new V(0.26d)) * v_trailScale_26), ((v_side_27 * v_size_21) * new V(0.05d)));
v_ctx_19[new V("lineTo")].Call((((-v_size_21) * new V(0.88d)) * v_trailScale_26), ((v_side_27 * v_size_21) * new V(0.18d)));
v_ctx_19[new V("lineTo")].Call((((-v_size_21) * new V(1.28d)) * v_trailScale_26), ((v_side_27 * v_size_21) * new V(0.03d)));
v_ctx_19[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.78d)));
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("moveTo")].Call(((-v_size_21) * new V(0.78d)), ((-v_size_21) * new V(0.22d)));
v_ctx_19[new V("lineTo")].Call(((-v_size_21) * new V(1.02d)), new V(0d));
v_ctx_19[new V("lineTo")].Call(((-v_size_21) * new V(0.78d)), (v_size_21 * new V(0.22d)));
v_ctx_19[new V("stroke")].Call();
} else {
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.84d)));
VisualJs.Assign(v_ctx_19,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_size_21 * new V(0.095d))));
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("moveTo")].Call((((-v_size_21) * new V(0.3d)) * v_trailScale_26), new V(0d));
v_ctx_19[new V("quadraticCurveTo")].Call(((-v_size_21) * new V(0.74d)), ((-v_size_21) * new V(0.2d)), ((-v_size_21) * new V(1.15d)), ((-v_size_21) * new V(0.04d)));
v_ctx_19[new V("quadraticCurveTo")].Call(((-v_size_21) * new V(0.86d)), (v_size_21 * new V(0.2d)), (((-v_size_21) * new V(0.3d)) * v_trailScale_26), new V(0d));
v_ctx_19[new V("stroke")].Call();
VisualJs.Assign(v_ctx_19,new V("globalAlpha"),"=",(v_alpha_23 * new V(0.64d)));
VisualJs.Assign(v_ctx_19,new V("fillStyle"),"=",Scope[new V("rgbaColor")].Call(v_color_25, (v_alpha_23 * new V(0.42d))));
for (V v_i_28 = new V(0d); new V(v_i_28 < new V(3d)).Truth; v_i_28++) {
V v_px_29 = ((-v_size_21) * ((new V(0.58d) + (v_i_28 * new V(0.25d)))));
V v_py_30 = ((((v_i_28 - new V(1d))) * v_size_21) * new V(0.14d));
v_ctx_19[new V("beginPath")].Call();
v_ctx_19[new V("arc")].Call(v_px_29, v_py_30, VisualJs.MathObject[new V("max")].Call(new V(0.9d), (v_size_21 * ((new V(0.08d) - (v_i_28 * new V(0.015d)))))), new V(0d), Scope[new V("TAU")]);
v_ctx_19[new V("fill")].Call();
}
}
v_ctx_19[new V("restore")].Call();
return default;
}
private static V F_drawSatellite(V[] args) {
V v_ctx_31 = VisualJs.Arg(args,0);
V v_x_32 = VisualJs.Arg(args,1);
V v_y_33 = VisualJs.Arg(args,2);
V v_size_34 = VisualJs.Arg(args,3);
V v_color_35 = VisualJs.Arg(args,4);
V v_alpha_36 = VisualJs.Arg(args,5);
V v_angle_37 = VisualJs.Arg(args,6);
V v_blade_38 = VisualJs.Arg(args,7);
v_ctx_31[new V("save")].Call();
v_ctx_31[new V("translate")].Call(v_x_32, v_y_33);
v_ctx_31[new V("rotate")].Call((v_blade_38.Truth ? v_angle_37 : (v_angle_37 + (VisualJs.MathObject[new V("PI")] / new V(4d)))));
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",v_alpha_36);
if (v_blade_38.Truth) {
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_31,new V("strokeStyle"),"=",v_color_35);
VisualJs.Assign(v_ctx_31,new V("lineWidth"),"=",new V(1.05d));
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("moveTo")].Call(((-v_size_34) * new V(0.56d)), ((-v_size_34) * new V(0.14d)));
v_ctx_31[new V("lineTo")].Call((v_size_34 * new V(0.78d)), new V(0d));
v_ctx_31[new V("lineTo")].Call(((-v_size_34) * new V(0.34d)), (v_size_34 * new V(0.15d)));
v_ctx_31[new V("lineTo")].Call(((-v_size_34) * new V(0.02d)), new V(0d));
v_ctx_31[new V("closePath")].Call();
v_ctx_31[new V("fill")].Call();
v_ctx_31[new V("stroke")].Call();
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",(v_alpha_36 * new V(0.72d)));
VisualJs.Assign(v_ctx_31,new V("strokeStyle"),"=",new V("#ffffff"));
VisualJs.Assign(v_ctx_31,new V("lineWidth"),"=",new V(0.72d));
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("moveTo")].Call(((-v_size_34) * new V(0.3d)), new V(0d));
v_ctx_31[new V("lineTo")].Call((v_size_34 * new V(0.54d)), new V(0d));
v_ctx_31[new V("stroke")].Call();
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",(v_alpha_36 * new V(0.5d)));
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("moveTo")].Call(((-v_size_34) * new V(0.42d)), ((-v_size_34) * new V(0.1d)));
v_ctx_31[new V("lineTo")].Call(((-v_size_34) * new V(0.62d)), new V(0d));
v_ctx_31[new V("lineTo")].Call(((-v_size_34) * new V(0.42d)), (v_size_34 * new V(0.1d)));
v_ctx_31[new V("stroke")].Call();
} else {
VisualJs.Assign(v_ctx_31,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",Scope[new V("rgbaColor")].Call(v_color_35, (v_alpha_36 * new V(0.3d))));
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("arc")].Call(new V(0d), new V(0d), (v_size_34 * new V(0.62d)), new V(0d), Scope[new V("TAU")]);
v_ctx_31[new V("fill")].Call();
VisualJs.Assign(v_ctx_31,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",(v_alpha_36 * new V(0.92d)));
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_31,new V("strokeStyle"),"=",v_color_35);
VisualJs.Assign(v_ctx_31,new V("lineWidth"),"=",new V(1d));
v_ctx_31[new V("beginPath")].Call();
for (V v_i_39 = new V(0d); new V(v_i_39 < new V(8d)).Truth; v_i_39++) {
V v_a_40 = (((-VisualJs.MathObject[new V("PI")]) / new V(8d)) + ((v_i_39 * Scope[new V("TAU")]) / new V(8d)));
V v_rr_41 = (new V((v_i_39 % new V(2d)) == new V(0d)).Truth ? (v_size_34 * new V(0.56d)) : (v_size_34 * new V(0.44d)));
V v_px_42 = (VisualJs.MathObject[new V("cos")].Call(v_a_40) * v_rr_41);
V v_py_43 = (VisualJs.MathObject[new V("sin")].Call(v_a_40) * v_rr_41);
if (new V(v_i_39 == new V(0d)).Truth) v_ctx_31[new V("moveTo")].Call(v_px_42, v_py_43); else v_ctx_31[new V("lineTo")].Call(v_px_42, v_py_43);
}
v_ctx_31[new V("closePath")].Call();
v_ctx_31[new V("fill")].Call();
v_ctx_31[new V("stroke")].Call();
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",(v_alpha_36 * new V(0.9d)));
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",Visual_visualHelpers.Scope[new V("WHITE")]);
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("arc")].Call(((-v_size_34) * new V(0.1d)), ((-v_size_34) * new V(0.12d)), (v_size_34 * new V(0.16d)), new V(0d), Scope[new V("TAU")]);
v_ctx_31[new V("fill")].Call();
}
v_ctx_31[new V("restore")].Call();
return default;
}
private static V F_renderOrbitalSphereRuntimeVfx(V[] args) {
V v_ctx_44 = VisualJs.Arg(args,0);
V v_sphere_45 = VisualJs.Arg(args,1);
V v_player_46 = VisualJs.Arg(args,2);
V v_time_47 = VisualJs.Arg(args,3);
V v_scale_48 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v__enemies_49 = VisualJs.Arg(args,5).Undefined?VisualJs.Arr():VisualJs.Arg(args,5);
V v_orbitalRadiusScale_50 = VisualJs.Arg(args,6).Undefined?new V(1d):VisualJs.Arg(args,6);
V v_v_51 = Scope[new V("update")].Call(v_sphere_45, v_player_46, v_time_47);
V v_disabled_52 = VisualJs.Or(new V(v_sphere_45[new V("networkDisabledTimer")] > new V(0d)), () => (!v_sphere_45[new V("alive")]));
V v_resonance_53 = VisualJs.And((!v_disabled_52), () => new V(v_v_51[new V("resonance")] > new V(0d)));
V v_color_54 = (v_disabled_52.Truth ? Scope[new V("DISABLED")] : (v_resonance_53.Truth ? Scope[new V("RESONANCE")] : Scope[new V("BASE")]));
V v_r_55 = (new V(24d) * v_scale_48);
V v_orbitMultiplier_56 = (VisualJs.And(VisualJs.NumberObject[new V("isFinite")].Call(v_orbitalRadiusScale_50), () => new V(v_orbitalRadiusScale_50 > new V(0d))).Truth ? v_orbitalRadiusScale_50 : new V(1d));
V v_coreR_57 = (v_r_55 * new V(0.7d));
v_ctx_44[new V("save")].Call();
v_ctx_44[new V("translate")].Call(v_sphere_45[new V("pos")][new V("x")], v_sphere_45[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_44,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_44, (v_r_55 * new V(2.55d)), v_color_54, (v_disabled_52.Truth ? new V(0.05d) : new V(0.13d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_44, ((v_r_55 * new V(1.05d)) * v_orbitMultiplier_56), v_color_54, ((v_time_47 * new V(0.18d)) + v_v_51[new V("phase")]), (v_disabled_52.Truth ? new V(0.12d) : new V(0.66d)), new V("orbital"), (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call(((v_time_47 * new V(3d)) + v_v_51[new V("phase")])))));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_44, ((v_r_55 * new V(0.88d)) * v_orbitMultiplier_56), v_color_54, (((-v_time_47) * new V(0.22d)) + (v_v_51[new V("phase")] * new V(0.5d))), (v_disabled_52.Truth ? new V(0.08d) : new V(0.38d)), new V("orbital"), new V(0.5d));
VisualJs.Assign(v_ctx_44,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_44, v_coreR_57, v_color_54, (v_disabled_52.Truth ? new V(0.5d) : (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call(((v_time_47 * new V(3d)) + v_v_51[new V("phase")]))))));
Scope[new V("drawOrbitalTerminal")].Call(v_ctx_44, v_r_55, ((-v_r_55) * new V(1.02d)), v_color_54, (v_disabled_52.Truth ? new V(0.2d) : new V(0.76d)), v_time_47);
Scope[new V("drawOrbitalTerminal")].Call(v_ctx_44, v_r_55, (v_r_55 * new V(1.02d)), v_color_54, (v_disabled_52.Truth ? new V(0.16d) : new V(0.62d)), (-v_time_47));
if (v_resonance_53.Truth) {
VisualJs.Assign(v_ctx_44,new V("globalAlpha"),"=",new V(0.42d));
VisualJs.Assign(v_ctx_44,new V("strokeStyle"),"=",Scope[new V("RESONANCE")]);
VisualJs.Assign(v_ctx_44,new V("lineWidth"),"=",new V(1d));
v_ctx_44[new V("beginPath")].Call();
v_ctx_44[new V("arc")].Call(new V(0d), new V(0d), (v_r_55 * new V(1.34d)), (v_time_47 * new V(0.65d)), ((v_time_47 * new V(0.65d)) + (VisualJs.MathObject[new V("PI")] * new V(1.15d))));
v_ctx_44[new V("stroke")].Call();
}
if (v_disabled_52.Truth) {
VisualJs.Assign(v_ctx_44,new V("globalAlpha"),"=",new V(0.5d));
VisualJs.Assign(v_ctx_44,new V("strokeStyle"),"=",Scope[new V("DISABLED")]);
VisualJs.Assign(v_ctx_44,new V("lineWidth"),"=",new V(1.5d));
v_ctx_44[new V("beginPath")].Call();
v_ctx_44[new V("moveTo")].Call(((-v_r_55) * new V(0.72d)), ((-v_r_55) * new V(0.72d)));
v_ctx_44[new V("lineTo")].Call((v_r_55 * new V(0.72d)), (v_r_55 * new V(0.72d)));
v_ctx_44[new V("moveTo")].Call((v_r_55 * new V(0.72d)), ((-v_r_55) * new V(0.72d)));
v_ctx_44[new V("lineTo")].Call(((-v_r_55) * new V(0.72d)), (v_r_55 * new V(0.72d)));
v_ctx_44[new V("stroke")].Call();
}
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_44, v_r_55, v_color_54, v_disabled_52);
v_ctx_44[new V("restore")].Call();
return default;
}
private static V F_renderOrbitalSphereAttackersVfx(V[] args) {
V v_ctx_58 = VisualJs.Arg(args,0);
V v_sphere_59 = VisualJs.Arg(args,1);
V v_player_60 = VisualJs.Arg(args,2);
V v_time_61 = VisualJs.Arg(args,3);
V v_scale_62 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_enemies_63 = VisualJs.Arg(args,5).Undefined?VisualJs.Arr():VisualJs.Arg(args,5);
V v_orbitalRadiusScale_64 = VisualJs.Arg(args,6).Undefined?new V(1d):VisualJs.Arg(args,6);
if (VisualJs.Or((!v_sphere_59[new V("alive")]), () => new V(v_sphere_59[new V("networkDisabledTimer")] > new V(0d))).Truth) return default;
V v_v_65 = Scope[new V("update")].Call(v_sphere_59, v_player_60, v_time_61);
V v_r_66 = (new V(24d) * v_scale_62);
V v_level_67 = VisualJs.MathObject[new V("max")].Call(new V(1d), VisualJs.MathObject[new V("min")].Call(new V(7d), VisualJs.Or(v_sphere_59[new V("visualTier")], () => new V(1d))));
V v_resonance_68 = new V(v_v_65[new V("resonance")] > new V(0d));
V v_branch_69 = VisualJs.Chain(v_player_60,optional_70=>VisualJs.Chain(optional_70[new V("sphereBranches")],optional_71=>optional_71[new V("orbital")]));
V v_branchElement_72 = Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_69);
V v_color_73 = (v_resonance_68.Truth ? Scope[new V("RESONANCE")] : Scope[new V("BASE")]);
V v_satelliteBaseColor_74 = (v_branchElement_72.Truth ? Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_branchElement_72][new V("color")] : v_color_73);
V v_extraElements_75 = (((VisualJs.Chain(v_player_60,optional_76=>VisualJs.Chain(optional_76[new V("artifacts")],optional_77=>optional_77[new V("includes")].Call(new V("orbital_crown")))).Truth ? new V(1d) : new V(0d))) + ((VisualJs.And(new V(v_branch_69 == new V("orbital_dance")), () => (VisualJs.Or(VisualJs.Chain(v_player_60,optional_78=>optional_78[new V("evolutions")]), () => VisualJs.Arr()))[new V("some")].Call(VisualJs.Fn(args_79 => { V v_id_80 = VisualJs.Arg(args_79, 0);
return new V(v_id_80 == new V("sphere:orbital:7:orbital_dance:2")); }))).Truth ? new V(1d) : new V(0d))));
V v_counts_81 = Visual_orbitalGeometry.Scope[new V("getOrbitalRingCounts")].Call(v_level_67, v_extraElements_75);
V v_bladeMutation_82 = new V(v_branch_69 == new V("orbital_blade"));
V v_orbitMultiplier_83 = (VisualJs.And(VisualJs.NumberObject[new V("isFinite")].Call(v_orbitalRadiusScale_64), () => new V(v_orbitalRadiusScale_64 > new V(0d))).Truth ? v_orbitalRadiusScale_64 : new V(1d));
V v_innerOrbitRadius_84 = ((v_r_66 * new V(1.68d)) * v_orbitMultiplier_83);
V v_outerOrbitRadius_85 = ((v_r_66 * new V(2.02d)) * v_orbitMultiplier_83);
v_ctx_58[new V("save")].Call();
v_ctx_58[new V("translate")].Call(v_sphere_59[new V("pos")][new V("x")], v_sphere_59[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_58,new V("globalCompositeOperation"),"=",new V("lighter"));
V v_drawRing_86 = default; v_drawRing_86 = VisualJs.Fn(args_87 => { V v_ring_88 = VisualJs.Arg(args_87, 0);
V v_count_89 = VisualJs.Arg(args_87, 1);
V v_radius_90 = VisualJs.Arg(args_87, 2);
for (V v_i_91 = new V(0d); new V(v_i_91 < v_count_89).Truth; v_i_91 += new V(1d)) {
V v_a_92 = Visual_orbitalGeometry.Scope[new V("orbitalElementAngle")].Call(v_sphere_59[new V("rotation")], v_ring_88, v_i_91, v_count_89);
V v_localX_93 = (VisualJs.MathObject[new V("cos")].Call(v_a_92) * v_radius_90);
V v_localY_94 = (VisualJs.MathObject[new V("sin")].Call(v_a_92) * v_radius_90);
V v_depth_95 = (new V(0.74d) + (new V(0.26d) * ((((VisualJs.MathObject[new V("sin")].Call(v_a_92) + new V(1d))) * new V(0.5d)))));
Scope[new V("drawSatellite")].Call(v_ctx_58, v_localX_93, v_localY_94, (v_r_66 * new V(0.2d)), v_satelliteBaseColor_74, v_depth_95, v_a_92, v_bladeMutation_82);
V v_element_96 = Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_69);
V v_elementColor_97 = (v_element_96.Truth ? Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_element_96][new V("color")] : VisualJs.Null);
if (v_element_96.Truth) {
V v_combatElementColor_98 = VisualJs.Or(v_elementColor_97, () => v_color_73);
v_ctx_58[new V("save")].Call();
v_ctx_58[new V("translate")].Call(v_localX_93, v_localY_94);
VisualJs.Assign(v_ctx_58,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_58,new V("globalAlpha"),"=",(v_depth_95 * new V(0.92d)));
VisualJs.Assign(v_ctx_58,new V("strokeStyle"),"=",v_combatElementColor_98);
VisualJs.Assign(v_ctx_58,new V("fillStyle"),"=",Scope[new V("rgbaColor")].Call(v_combatElementColor_98, (v_depth_95 * new V(0.22d))));
VisualJs.Assign(v_ctx_58,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1.25d), (v_r_66 * new V(0.075d))));
v_ctx_58[new V("beginPath")].Call();
v_ctx_58[new V("arc")].Call(new V(0d), new V(0d), (v_r_66 * new V(0.18d)), new V(0d), Scope[new V("TAU")]);
v_ctx_58[new V("stroke")].Call();
v_ctx_58[new V("beginPath")].Call();
v_ctx_58[new V("arc")].Call(new V(0d), new V(0d), (v_r_66 * new V(0.095d)), new V(0d), Scope[new V("TAU")]);
v_ctx_58[new V("fill")].Call();
v_ctx_58[new V("restore")].Call();
Scope[new V("drawOrbitalElementTrail")].Call(v_ctx_58, v_element_96, (v_r_66 * new V(0.2d)), v_a_92, v_depth_95, v_bladeMutation_82);
}
if (VisualJs.And(VisualJs.And(new V(v_ring_88 == new V("inner")), () => new V(v_i_91 == new V(0d))), () => new V(v_v_65[new V("attack")] > new V(0d))).Truth) {
V v_q_99 = (new V(1d) - (v_v_65[new V("attack")] / new V(0.28d)));
v_ctx_58[new V("save")].Call();
v_ctx_58[new V("translate")].Call(v_localX_93, v_localY_94);
v_ctx_58[new V("rotate")].Call((v_a_92 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_58,new V("globalAlpha"),"=",(((new V(1d) - v_q_99)) * new V(0.32d)));
VisualJs.Assign(v_ctx_58,new V("strokeStyle"),"=",v_color_73);
VisualJs.Assign(v_ctx_58,new V("lineWidth"),"=",new V(1d));
v_ctx_58[new V("beginPath")].Call();
v_ctx_58[new V("moveTo")].Call(((-v_r_66) * new V(0.32d)), new V(0d));
v_ctx_58[new V("lineTo")].Call(((-v_r_66) * new V(0.04d)), new V(0d));
v_ctx_58[new V("stroke")].Call();
v_ctx_58[new V("restore")].Call();
}
}
return default; });
v_drawRing_86.Call(new V("inner"), v_counts_81[new V("inner")], v_innerOrbitRadius_84);
v_drawRing_86.Call(new V("outer"), v_counts_81[new V("outer")], v_outerOrbitRadius_85);
if (new V(v_v_65[new V("attack")] > new V(0d)).Truth) {
V v_q_100 = (new V(1d) - (v_v_65[new V("attack")] / new V(0.28d)));
V v_strikePhase_101 = (v_sphere_59[new V("rotation")] + (((new V(1d) - VisualJs.MathObject[new V("pow")].Call((new V(1d) - v_q_100), new V(3d)))) * new V(0.95d)));
V v_sx_102 = (VisualJs.MathObject[new V("cos")].Call(v_strikePhase_101) * v_innerOrbitRadius_84);
V v_sy_103 = (VisualJs.MathObject[new V("sin")].Call(v_strikePhase_101) * v_innerOrbitRadius_84);
V v_ix_104 = v_sx_102;
V v_iy_105 = v_sy_103;
V v_near_106 = new V(double.PositiveInfinity);
foreach (V v_e_107 in v_enemies_63.Items) {
if (VisualJs.Or((!v_e_107), () => new V(v_e_107[new V("hp")] <= new V(0d))).Truth) continue;
V v_ex_108 = ((v_e_107[new V("pos")][new V("x")] - v_sphere_59[new V("pos")][new V("x")]) - v_sx_102);
V v_ey_109 = ((v_e_107[new V("pos")][new V("y")] - v_sphere_59[new V("pos")][new V("y")]) - v_sy_103);
V v_d_110 = VisualJs.MathObject[new V("hypot")].Call(v_ex_108, v_ey_109);
if (VisualJs.And(new V(v_d_110 < v_near_106), () => new V(v_d_110 <= (v_r_66 * new V(1.3d)))).Truth) {
v_near_106 = v_d_110;
v_ix_104 = (v_sx_102 + v_ex_108);
v_iy_105 = (v_sy_103 + v_ey_109);
}
}
VisualJs.Assign(v_ctx_58,new V("globalAlpha"),"=",(new V(0.65d) * ((new V(1d) - v_q_100))));
VisualJs.Assign(v_ctx_58,new V("fillStyle"),"=",new V("#ffffff"));
v_ctx_58[new V("beginPath")].Call();
v_ctx_58[new V("arc")].Call(v_ix_104, v_iy_105, (v_r_66 * new V(0.07d)), new V(0d), Scope[new V("TAU")]);
v_ctx_58[new V("fill")].Call();
}
v_ctx_58[new V("restore")].Call();
return default;
}
}
}
