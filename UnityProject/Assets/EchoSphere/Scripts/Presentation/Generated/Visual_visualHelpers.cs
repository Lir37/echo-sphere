#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
// Generated from src/spheres/visualHelpers.ts; sha256 caf1d9731fd4c76c955e329fd0c8ac96dd42bbb3bfd997f03a33ba95b2423e4d.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_visualHelpers {
public static readonly V Scope=VisualJs.Obj();
static Visual_visualHelpers() {
Scope[new V("gradientsOf")] = VisualJs.Fn(F_gradientsOf);
Scope[new V("coreGradientsOf")] = VisualJs.Fn(F_coreGradientsOf);
Scope[new V("rgbaFromHex")] = VisualJs.Fn(F_rgbaFromHex);
Scope[new V("stateColor")] = VisualJs.Fn(F_stateColor);
Scope[new V("glow")] = VisualJs.Fn(F_glow);
Scope[new V("core")] = VisualJs.Fn(F_core);
Scope[new V("ellipsePoint")] = VisualJs.Fn(F_ellipsePoint);
Scope[new V("drawEllipseTrace")] = VisualJs.Fn(F_drawEllipseTrace);
Scope[new V("drawLightningEllipse")] = VisualJs.Fn(F_drawLightningEllipse);
Scope[new V("drawSphereOrbit")] = VisualJs.Fn(F_drawSphereOrbit);
Scope[new V("orbitPoint")] = VisualJs.Fn(F_orbitPoint);
Scope[new V("finishDisabled")] = VisualJs.Fn(F_finishDisabled);
Scope[new V("WHITE")] = new V("#ffffff");
Scope[new V("RESONANCE")] = new V("#ffd166");
Scope[new V("DISABLED")] = new V("#ff4d70");
Scope[new V("GRADIENTS")] = VisualJs.CreateMap(default(V),true);
Scope[new V("CORE_GRADIENTS")] = VisualJs.CreateMap(default(V),true);
}
private static V F_gradientsOf(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_map_1 = Scope[new V("GRADIENTS")][new V("get")].Call(v_ctx_0);
if (v_map_1.Truth) return v_map_1;
v_map_1 = VisualJs.CreateMap(default(V),false);
Scope[new V("GRADIENTS")][new V("set")].Call(v_ctx_0, v_map_1);
return v_map_1;
}
private static V F_coreGradientsOf(V[] args) {
V v_ctx_2 = VisualJs.Arg(args,0);
V v_map_3 = Scope[new V("CORE_GRADIENTS")][new V("get")].Call(v_ctx_2);
if (v_map_3.Truth) return v_map_3;
v_map_3 = VisualJs.CreateMap(default(V),false);
Scope[new V("CORE_GRADIENTS")][new V("set")].Call(v_ctx_2, v_map_3);
return v_map_3;
}
private static V F_rgbaFromHex(V[] args) {
V v_hex_4 = VisualJs.Arg(args,0);
V v_alpha_5 = VisualJs.Arg(args,1);
V v_n_6 = VisualJs.NumberObject[new V("parseInt")].Call(v_hex_4[new V("slice")].Call(new V(1d)), new V(16d));
return (new V("rgba(") + (((v_n_6 >> (int)new V(16d).Number)) & new V(255d)) + new V(",") + (((v_n_6 >> (int)new V(8d).Number)) & new V(255d)) + new V(",") + (v_n_6 & new V(255d)) + new V(",") + v_alpha_5 + new V(")"));
}
private static V F_stateColor(V[] args) {
V v_sphere_7 = VisualJs.Arg(args,0);
V v_player_8 = VisualJs.Arg(args,1);
V v_base_9 = VisualJs.Arg(args,2);
V v_time_10 = VisualJs.Arg(args,3);
V v_disabled_11 = VisualJs.Or(new V(v_sphere_7[new V("networkDisabledTimer")] > new V(0d)), () => (!v_sphere_7[new V("alive")]));
V v_resonance_12 = VisualJs.And((!v_disabled_11), () => (!(!VisualJs.Chain(v_player_8,optional_13=>optional_13[new V("resonanceEventActive")]))));
return VisualJs.Obj(new V("color"), (v_disabled_11.Truth ? Scope[new V("DISABLED")] : (v_resonance_12.Truth ? Scope[new V("RESONANCE")] : v_base_9)), new V("disabled"), v_disabled_11, new V("resonance"), v_resonance_12, new V("pulse"), (v_disabled_11.Truth ? new V(0.5d) : (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call((((v_time_10 * new V(3.4d)) + (v_sphere_7[new V("pos")][new V("x")] * new V(0.008d))) + (v_sphere_7[new V("pos")][new V("y")] * new V(0.006d))))))), new V("animationTime"), (v_disabled_11.Truth ? new V(0d) : v_time_10));
}
private static V F_glow(V[] args) {
V v_ctx_14 = VisualJs.Arg(args,0);
V v_radius_15 = VisualJs.Arg(args,1);
V v_color_16 = VisualJs.Arg(args,2);
V v_alpha_17 = VisualJs.Arg(args,3).Undefined?new V(0.18d):VisualJs.Arg(args,3);
V v_map_18 = Scope[new V("gradientsOf")].Call(v_ctx_14);
V v_g_19 = v_map_18[new V("get")].Call(v_color_16);
if ((!v_g_19).Truth) {
v_g_19 = v_ctx_14[new V("createRadialGradient")].Call(new V(0d), new V(0d), new V(0d), new V(0d), new V(0d), new V(1d));
v_g_19[new V("addColorStop")].Call(new V(0d), Scope[new V("rgbaFromHex")].Call(v_color_16, new V(0.72d)));
v_g_19[new V("addColorStop")].Call(new V(0.2d), Scope[new V("rgbaFromHex")].Call(v_color_16, new V(0.38d)));
v_g_19[new V("addColorStop")].Call(new V(0.55d), Scope[new V("rgbaFromHex")].Call(v_color_16, new V(0.12d)));
v_g_19[new V("addColorStop")].Call(new V(1d), Scope[new V("rgbaFromHex")].Call(v_color_16, new V(0d)));
v_map_18[new V("set")].Call(v_color_16, v_g_19);
}
v_ctx_14[new V("save")].Call();
VisualJs.Assign(v_ctx_14,new V("globalAlpha"),"=",v_alpha_17);
VisualJs.Assign(v_ctx_14,new V("fillStyle"),"=",v_g_19);
v_ctx_14[new V("scale")].Call(v_radius_15, v_radius_15);
v_ctx_14[new V("beginPath")].Call();
v_ctx_14[new V("arc")].Call(new V(0d), new V(0d), new V(1d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_14[new V("fill")].Call();
v_ctx_14[new V("restore")].Call();
return default;
}
private static V F_core(V[] args) {
V v_ctx_20 = VisualJs.Arg(args,0);
V v_radius_21 = VisualJs.Arg(args,1);
V v_color_22 = VisualJs.Arg(args,2);
V v_pulse_23 = VisualJs.Arg(args,3);
V v_scale_24 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
v_ctx_20[new V("save")].Call();
v_ctx_20[new V("scale")].Call(v_scale_24, v_scale_24);
V v_glow_25 = v_ctx_20[new V("createRadialGradient")].Call(((-v_radius_21) * new V(0.08d)), ((-v_radius_21) * new V(0.1d)), new V(1d), new V(0d), new V(0d), (v_radius_21 * new V(0.62d)));
v_glow_25[new V("addColorStop")].Call(new V(0d), new V("rgba(255,255,255,0.98)"));
v_glow_25[new V("addColorStop")].Call(new V(0.2d), Scope[new V("rgbaFromHex")].Call(v_color_22, new V(0.96d)));
v_glow_25[new V("addColorStop")].Call(new V(0.48d), Scope[new V("rgbaFromHex")].Call(v_color_22, new V(0.78d)));
v_glow_25[new V("addColorStop")].Call(new V(0.82d), Scope[new V("rgbaFromHex")].Call(v_color_22, new V(0.34d)));
v_glow_25[new V("addColorStop")].Call(new V(1d), Scope[new V("rgbaFromHex")].Call(v_color_22, new V(0d)));
v_ctx_20[new V("save")].Call();
VisualJs.Assign(v_ctx_20,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_20,new V("globalAlpha"),"=",(new V(0.82d) + (v_pulse_23 * new V(0.14d))));
VisualJs.Assign(v_ctx_20,new V("fillStyle"),"=",v_glow_25);
v_ctx_20[new V("beginPath")].Call();
v_ctx_20[new V("arc")].Call(new V(0d), new V(0d), ((v_radius_21 * new V(0.58d)) * ((new V(1d) + (v_pulse_23 * new V(0.035d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_20[new V("fill")].Call();
VisualJs.Assign(v_ctx_20,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_20,new V("fillStyle"),"=",Scope[new V("rgbaFromHex")].Call(v_color_22, new V(0.72d)));
VisualJs.Assign(v_ctx_20,new V("strokeStyle"),"=",new V("rgba(182,247,255,0.90)"));
VisualJs.Assign(v_ctx_20,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_radius_21 * new V(0.035d))));
v_ctx_20[new V("beginPath")].Call();
v_ctx_20[new V("arc")].Call(new V(0d), new V(0d), (v_radius_21 * new V(0.42d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_20[new V("fill")].Call();
v_ctx_20[new V("stroke")].Call();
v_ctx_20[new V("restore")].Call();
v_ctx_20[new V("restore")].Call();
return default;
}
private static V F_ellipsePoint(V[] args) {
V v_radiusX_26 = VisualJs.Arg(args,0);
V v_radiusY_27 = VisualJs.Arg(args,1);
V v_a_28 = VisualJs.Arg(args,2);
return VisualJs.Obj(new V("x"), (VisualJs.MathObject[new V("cos")].Call(v_a_28) * v_radiusX_26), new V("y"), (VisualJs.MathObject[new V("sin")].Call(v_a_28) * v_radiusY_27));
}
private static V F_drawEllipseTrace(V[] args) {
V v_ctx_29 = VisualJs.Arg(args,0);
V v_radiusX_30 = VisualJs.Arg(args,1);
V v_radiusY_31 = VisualJs.Arg(args,2);
V v_start_32 = VisualJs.Arg(args,3);
V v_end_33 = VisualJs.Arg(args,4);
V v_wobble_34 = VisualJs.Arg(args,5).Undefined?new V(0d):VisualJs.Arg(args,5);
V v_steps_35 = new V(42d);
v_ctx_29[new V("beginPath")].Call();
for (V v_i_36 = new V(0d); new V(v_i_36 <= v_steps_35).Truth; v_i_36++) {
V v_p_37 = (v_i_36 / v_steps_35);
V v_a_38 = (v_start_32 + (((v_end_33 - v_start_32)) * v_p_37));
V v_n_39 = (v_wobble_34.Truth ? (VisualJs.MathObject[new V("sin")].Call(((v_a_38 * new V(7d)) + new V(0.35d))) * v_wobble_34) : new V(0d));
V v_x_40 = (VisualJs.MathObject[new V("cos")].Call(v_a_38) * ((v_radiusX_30 + v_n_39)));
V v_y_41 = (VisualJs.MathObject[new V("sin")].Call(v_a_38) * ((v_radiusY_31 + (v_n_39 * new V(0.34d)))));
if (new V(v_i_36 == new V(0d)).Truth) v_ctx_29[new V("moveTo")].Call(v_x_40, v_y_41); else v_ctx_29[new V("lineTo")].Call(v_x_40, v_y_41);
}
v_ctx_29[new V("stroke")].Call();
return default;
}
private static V F_drawLightningEllipse(V[] args) {
V v_ctx_42 = VisualJs.Arg(args,0);
V v_radiusX_43 = VisualJs.Arg(args,1);
V v_radiusY_44 = VisualJs.Arg(args,2);
V v_rotation_45 = VisualJs.Arg(args,3);
V v_color_46 = VisualJs.Arg(args,4);
V v_alpha_47 = VisualJs.Arg(args,5);
V v_pulse_48 = VisualJs.Arg(args,6);
v_ctx_42[new V("save")].Call();
v_ctx_42[new V("rotate")].Call(v_rotation_45);
VisualJs.Assign(v_ctx_42,new V("strokeStyle"),"=",v_color_46);
VisualJs.Assign(v_ctx_42,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_radiusX_43 * new V(0.028d))));
VisualJs.Assign(v_ctx_42,new V("globalAlpha"),"=",v_alpha_47);
V v_segments_49 = new V(20d);
v_ctx_42[new V("beginPath")].Call();
for (V v_i_50 = new V(0d); new V(v_i_50 <= v_segments_49).Truth; v_i_50++) {
V v_a_51 = ((((v_i_50 / v_segments_49)) * VisualJs.MathObject[new V("PI")]) * new V(2d));
V v_jitter_52 = ((VisualJs.MathObject[new V("sin")].Call(((v_i_50 * new V(3.7d)) + (v_pulse_48 * new V(5.2d)))) * v_radiusX_43) * new V(0.025d));
V v_p_53 = Scope[new V("ellipsePoint")].Call((v_radiusX_43 + v_jitter_52), (v_radiusY_44 + (v_jitter_52 * new V(0.34d))), v_a_51);
if (new V(v_i_50 == new V(0d)).Truth) v_ctx_42[new V("moveTo")].Call(v_p_53[new V("x")], v_p_53[new V("y")]); else v_ctx_42[new V("lineTo")].Call(v_p_53[new V("x")], v_p_53[new V("y")]);
}
v_ctx_42[new V("stroke")].Call();
v_ctx_42[new V("restore")].Call();
return default;
}
private static V F_drawSphereOrbit(V[] args) {
V v_ctx_54 = VisualJs.Arg(args,0);
V v_radius_55 = VisualJs.Arg(args,1);
V v_color_56 = VisualJs.Arg(args,2);
V v_rotation_57 = VisualJs.Arg(args,3);
V v_opacity_58 = VisualJs.Arg(args,4).Undefined?new V(0.72d):VisualJs.Arg(args,4);
V v_mode_59 = VisualJs.Arg(args,5).Undefined?new V("standard"):VisualJs.Arg(args,5);
V v_pulse_60 = VisualJs.Arg(args,6).Undefined?new V(0.5d):VisualJs.Arg(args,6);
V v_rx_61 = (v_radius_55 * new V(1.16d));
V v_ry_62 = (v_radius_55 * new V(0.34d));
v_ctx_54[new V("save")].Call();
v_ctx_54[new V("rotate")].Call(v_rotation_57);
if (new V(v_mode_59 == new V("lightning")).Truth) {
Scope[new V("drawLightningEllipse")].Call(v_ctx_54, v_rx_61, v_ry_62, new V(0d), v_color_56, v_opacity_58, v_pulse_60);
Scope[new V("drawLightningEllipse")].Call(v_ctx_54, (v_radius_55 * new V(0.98d)), (v_radius_55 * new V(0.28d)), new V(0d), v_color_56, (v_opacity_58 * new V(0.36d)), (v_pulse_60 + new V(0.5d)));
v_ctx_54[new V("restore")].Call();
return default;
}
VisualJs.Assign(v_ctx_54,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_54,new V("strokeStyle"),"=",v_color_56);
VisualJs.Assign(v_ctx_54,new V("lineCap"),"=",new V("round"));
if (new V(v_mode_59 == new V("void")).Truth) {
for (V v_i_63 = new V(0d); new V(v_i_63 < new V(7d)).Truth; v_i_63++) {
V v_angle_64 = ((((v_i_63 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(7d)) + new V(0.08d));
V v_span_65 = (new V((v_i_63 % new V(2d)) == new V(0d)).Truth ? new V(0.58d) : new V(0.34d));
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * ((new V((v_i_63 % new V(3d)) == new V(0d)).Truth ? new V(0.94d) : new V(0.72d)))));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_radius_55 * new V(0.03d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, v_rx_61, v_ry_62, v_angle_64, (v_angle_64 + v_span_65), new V(0.7d));
}
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.42d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.55d), (v_radius_55 * new V(0.018d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_radius_55 * new V(1.03d)), (v_radius_55 * new V(0.295d)), (VisualJs.MathObject[new V("PI")] * new V(0.16d)), (VisualJs.MathObject[new V("PI")] * new V(1.84d)), new V(0.35d));
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.72d)));
VisualJs.Assign(v_ctx_54,new V("fillStyle"),"=",v_color_56);
for (V v_i_66 = new V(0d); new V(v_i_66 < new V(3d)).Truth; v_i_66++) {
V v_angle_67 = (((-v_rotation_57) * new V(0.72d)) + (((v_i_66 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
V v_p_68 = Scope[new V("ellipsePoint")].Call((v_rx_61 * new V(0.99d)), (v_ry_62 * new V(0.99d)), v_angle_67);
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("moveTo")].Call((v_p_68[new V("x")] + (v_radius_55 * new V(0.1d))), v_p_68[new V("y")]);
v_ctx_54[new V("lineTo")].Call(v_p_68[new V("x")], (v_p_68[new V("y")] + (v_radius_55 * new V(0.055d))));
v_ctx_54[new V("lineTo")].Call((v_p_68[new V("x")] - (v_radius_55 * new V(0.1d))), v_p_68[new V("y")]);
v_ctx_54[new V("lineTo")].Call(v_p_68[new V("x")], (v_p_68[new V("y")] - (v_radius_55 * new V(0.055d))));
v_ctx_54[new V("closePath")].Call();
v_ctx_54[new V("fill")].Call();
}
v_ctx_54[new V("restore")].Call();
return default;
}
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",v_opacity_58);
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.9d), (v_radius_55 * new V(0.04d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, v_rx_61, v_ry_62, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.44d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.55d), (v_radius_55 * new V(0.022d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_radius_55 * new V(1d)), (v_radius_55 * new V(0.285d)), (VisualJs.MathObject[new V("PI")] * new V(0.1d)), (VisualJs.MathObject[new V("PI")] * new V(1.92d)));
if (new V(v_mode_59 == new V("sniper")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.68d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.6d), (v_radius_55 * new V(0.02d))));
foreach (V v_a_69 in VisualJs.Arr((-new V(0.56d)), (-new V(0.28d)), new V(0d), new V(0.28d), new V(0.56d)).Items) {
V v_p_70 = Scope[new V("ellipsePoint")].Call(v_rx_61, v_ry_62, v_a_69);
V v_nx_71 = VisualJs.MathObject[new V("cos")].Call(v_a_69);
V v_ny_72 = VisualJs.MathObject[new V("sin")].Call(v_a_69);
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("moveTo")].Call((v_p_70[new V("x")] - ((v_nx_71 * v_radius_55) * new V(0.045d))), (v_p_70[new V("y")] - ((v_ny_72 * v_radius_55) * new V(0.045d))));
v_ctx_54[new V("lineTo")].Call((v_p_70[new V("x")] + ((v_nx_71 * v_radius_55) * new V(0.075d))), (v_p_70[new V("y")] + ((v_ny_72 * v_radius_55) * new V(0.075d))));
v_ctx_54[new V("stroke")].Call();
}
} else if (new V(v_mode_59 == new V("shotgun")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.72d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_radius_55 * new V(0.024d))));
for (V v_i_73 = new V(0d); new V(v_i_73 < new V(3d)).Truth; v_i_73++) {
V v_a_74 = ((-new V(0.44d)) + (v_i_73 * new V(0.44d)));
V v_p_75 = Scope[new V("ellipsePoint")].Call(v_rx_61, v_ry_62, v_a_74);
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("arc")].Call(v_p_75[new V("x")], v_p_75[new V("y")], (v_radius_55 * new V(0.065d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_54[new V("stroke")].Call();
}
} else if (new V(v_mode_59 == new V("aura")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.64d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.6d), (v_radius_55 * new V(0.018d))));
for (V v_i_76 = new V(0d); new V(v_i_76 < new V(6d)).Truth; v_i_76++) {
V v_a_77 = (((v_i_76 * VisualJs.MathObject[new V("PI")]) / new V(3d)) + (v_pulse_60 * new V(0.1d)));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_rx_61 * new V(0.96d)), (v_ry_62 * new V(0.96d)), (v_a_77 - new V(0.26d)), (v_a_77 + new V(0.26d)));
}
} else if (new V(v_mode_59 == new V("prism")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.74d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.75d), (v_radius_55 * new V(0.024d))));
v_ctx_54[new V("setLineDash")].Call(VisualJs.Arr((v_radius_55 * new V(0.15d)), (v_radius_55 * new V(0.07d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_rx_61 * new V(1.01d)), (v_ry_62 * new V(1.02d)), new V(0.08d), (VisualJs.MathObject[new V("PI")] * new V(1.95d)));
v_ctx_54[new V("setLineDash")].Call(VisualJs.Arr());
} else if (new V(v_mode_59 == new V("gravity")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.56d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_radius_55 * new V(0.026d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_rx_61 * new V(0.88d)), (v_ry_62 * new V(0.78d)), new V(0.25d), (VisualJs.MathObject[new V("PI")] * new V(1.78d)));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_rx_61 * new V(1.05d)), (v_ry_62 * new V(0.86d)), (VisualJs.MathObject[new V("PI")] * new V(1.08d)), (VisualJs.MathObject[new V("PI")] * new V(2.64d)));
} else if (new V(v_mode_59 == new V("orbital")).Truth) {
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",(v_opacity_58 * new V(0.52d)));
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.65d), (v_radius_55 * new V(0.022d))));
Scope[new V("drawEllipseTrace")].Call(v_ctx_54, (v_rx_61 * new V(0.92d)), (v_ry_62 * new V(0.84d)), (VisualJs.MathObject[new V("PI")] * new V(0.12d)), (VisualJs.MathObject[new V("PI")] * new V(1.86d)));
}
v_ctx_54[new V("restore")].Call();
return default;
}
private static V F_orbitPoint(V[] args) {
V v_radius_78 = VisualJs.Arg(args,0);
V v_angle_79 = VisualJs.Arg(args,1);
V v_flatten_80 = VisualJs.Arg(args,2).Undefined?new V(0.34d):VisualJs.Arg(args,2);
V v_radiusScale_81 = VisualJs.Arg(args,3).Undefined?new V(1.16d):VisualJs.Arg(args,3);
V v_s_82 = VisualJs.MathObject[new V("sin")].Call(v_angle_79);
return VisualJs.Obj(new V("x"), ((VisualJs.MathObject[new V("cos")].Call(v_angle_79) * v_radius_78) * v_radiusScale_81), new V("y"), (((v_s_82 * v_radius_78) * v_flatten_80) * v_radiusScale_81), new V("depth"), (new V(0.64d) + (new V(0.36d) * ((((v_s_82 + new V(1d))) * new V(0.5d))))));
}
private static V F_finishDisabled(V[] args) {
V v_ctx_83 = VisualJs.Arg(args,0);
V v_radius_84 = VisualJs.Arg(args,1);
V v_color_85 = VisualJs.Arg(args,2);
V v_disabled_86 = VisualJs.Arg(args,3);
if ((!v_disabled_86).Truth) return default;
v_ctx_83[new V("save")].Call();
VisualJs.Assign(v_ctx_83,new V("globalAlpha"),"=",new V(0.42d));
VisualJs.Assign(v_ctx_83,new V("strokeStyle"),"=",v_color_85);
VisualJs.Assign(v_ctx_83,new V("lineWidth"),"=",new V(1.7d));
v_ctx_83[new V("beginPath")].Call();
v_ctx_83[new V("arc")].Call(new V(0d), new V(0d), (v_radius_84 * new V(1.18d)), (VisualJs.MathObject[new V("PI")] * new V(0.2d)), (VisualJs.MathObject[new V("PI")] * new V(0.8d)));
v_ctx_83[new V("stroke")].Call();
v_ctx_83[new V("restore")].Call();
return default;
}
}
}
#endif
