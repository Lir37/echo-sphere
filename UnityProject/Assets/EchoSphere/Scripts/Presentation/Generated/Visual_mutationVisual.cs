// Generated from src/spheres/mutationVisual.ts; sha256 1a770c3b29ff7ab749c951a8737e5a265954fc2e8dc586e4474130351ce4b654.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_mutationVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_mutationVisual() {
Scope[new V("rgbOf")] = VisualJs.Fn(F_rgbOf);
Scope[new V("branchOf")] = VisualJs.Fn(F_branchOf);
Scope[new V("finalOf")] = VisualJs.Fn(F_finalOf);
Scope[new V("glow")] = VisualJs.Fn(F_glow);
Scope[new V("fin")] = VisualJs.Fn(F_fin);
Scope[new V("shard")] = VisualJs.Fn(F_shard);
Scope[new V("arc")] = VisualJs.Fn(F_arc);
Scope[new V("tether")] = VisualJs.Fn(F_tether);
Scope[new V("eye")] = VisualJs.Fn(F_eye);
Scope[new V("pulseRing")] = VisualJs.Fn(F_pulseRing);
Scope[new V("attackPeak")] = VisualJs.Fn(F_attackPeak);
Scope[new V("emit")] = VisualJs.Fn(F_emit);
Scope[new V("drawFinalExpansion")] = VisualJs.Fn(F_drawFinalExpansion);
Scope[new V("mutationSilhouette")] = VisualJs.Fn(F_mutationSilhouette);
Scope[new V("emitter")] = VisualJs.Fn(F_emitter);
Scope[new V("projectileCore")] = VisualJs.Fn(F_projectileCore);
Scope[new V("projectileTail")] = VisualJs.Fn(F_projectileTail);
Scope[new V("renderSphereMutationVfx")] = VisualJs.Fn(F_renderSphereMutationVfx);
Scope[new V("renderSphereProjectileVfx")] = VisualJs.Fn(F_renderSphereProjectileVfx);
Scope[new V("renderChainLightningVfx")] = VisualJs.Fn(F_renderChainLightningVfx);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
}
private static V F_rgbOf(V[] args) {
V v_hex_0 = VisualJs.Arg(args,0);
V v_n_1 = VisualJs.NumberObject[new V("parseInt")].Call(v_hex_0[new V("slice")].Call(new V(1d)), new V(16d));
return ((((((((v_n_1 >> (int)new V(16d).Number)) & new V(255d))) + new V(",")) + ((((v_n_1 >> (int)new V(8d).Number)) & new V(255d)))) + new V(",")) + ((v_n_1 & new V(255d))));
}
private static V F_branchOf(V[] args) {
V v_p_2 = VisualJs.Arg(args,0);
V v_t_3 = VisualJs.Arg(args,1);
return VisualJs.Coalesce(((VisualJs.Chain(v_p_2[new V("sphereBranches")],optional_4=>optional_4[v_t_3]))), () => VisualJs.Null);
}
private static V F_finalOf(V[] args) {
V v_p_5 = VisualJs.Arg(args,0);
V v_t_6 = VisualJs.Arg(args,1);
V v_id_7 = (VisualJs.Or(v_p_5[new V("evolutions")], () => VisualJs.Arr()))[new V("find")].Call(VisualJs.Fn(args_8 => { V v_x_9 = VisualJs.Arg(args_8, 0);
return v_x_9[new V("startsWith")].Call(((new V("sphere:") + v_t_6) + new V(":7:"))); }));
if ((!v_id_7).Truth) return (-new V(1d));
V v_n_10 = VisualJs.Number(v_id_7[new V("split")].Call(new V(":"))[new V("pop")].Call());
return (VisualJs.NumberObject[new V("isFinite")].Call(v_n_10).Truth ? v_n_10 : (-new V(1d)));
}
private static V F_glow(V[] args) {
V v_ctx_11 = VisualJs.Arg(args,0);
V v_r_12 = VisualJs.Arg(args,1);
V v_color_13 = VisualJs.Arg(args,2);
V v_a_14 = VisualJs.Arg(args,3).Undefined?new V(0.07d):VisualJs.Arg(args,3);
V v_rgb_15 = Scope[new V("rgbOf")].Call(v_color_13);
V v_g_16 = v_ctx_11[new V("createRadialGradient")].Call(new V(0d), new V(0d), new V(0d), new V(0d), new V(0d), v_r_12);
v_g_16[new V("addColorStop")].Call(new V(0d), ((((new V("rgba(") + v_rgb_15) + new V(",")) + ((v_a_14 * new V(3.4d)))[new V("toFixed")].Call(new V(3d))) + new V(")")));
v_g_16[new V("addColorStop")].Call(new V(0.28d), ((((new V("rgba(") + v_rgb_15) + new V(",")) + ((v_a_14 * new V(1.15d)))[new V("toFixed")].Call(new V(3d))) + new V(")")));
v_g_16[new V("addColorStop")].Call(new V(0.72d), ((((new V("rgba(") + v_rgb_15) + new V(",")) + ((v_a_14 * new V(0.25d)))[new V("toFixed")].Call(new V(3d))) + new V(")")));
v_g_16[new V("addColorStop")].Call(new V(1d), ((new V("rgba(") + v_rgb_15) + new V(",0)")));
v_ctx_11[new V("save")].Call();
VisualJs.Assign(v_ctx_11,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_11,new V("fillStyle"),"=",v_g_16);
v_ctx_11[new V("beginPath")].Call();
v_ctx_11[new V("arc")].Call(new V(0d), new V(0d), v_r_12, new V(0d), Scope[new V("TAU")]);
v_ctx_11[new V("fill")].Call();
v_ctx_11[new V("restore")].Call();
return default;
}
private static V F_fin(V[] args) {
V v_ctx_17 = VisualJs.Arg(args,0);
V v_x_18 = VisualJs.Arg(args,1);
V v_y_19 = VisualJs.Arg(args,2);
V v_l_20 = VisualJs.Arg(args,3);
V v_w_21 = VisualJs.Arg(args,4);
V v_c_22 = VisualJs.Arg(args,5);
V v_a_23 = VisualJs.Arg(args,6);
V v_alpha_24 = VisualJs.Arg(args,7).Undefined?new V(0.75d):VisualJs.Arg(args,7);
v_ctx_17[new V("save")].Call();
v_ctx_17[new V("translate")].Call(v_x_18, v_y_19);
v_ctx_17[new V("rotate")].Call(v_a_23);
VisualJs.Assign(v_ctx_17,new V("globalAlpha"),"=",v_alpha_24);
VisualJs.Assign(v_ctx_17,new V("fillStyle"),"=",new V("rgba(3,8,15,.92)"));
VisualJs.Assign(v_ctx_17,new V("strokeStyle"),"=",v_c_22);
VisualJs.Assign(v_ctx_17,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_w_21 * new V(0.07d))));
v_ctx_17[new V("beginPath")].Call();
v_ctx_17[new V("moveTo")].Call(new V(0d), ((-v_w_21) * new V(0.2d)));
v_ctx_17[new V("quadraticCurveTo")].Call((v_l_20 * new V(0.42d)), ((-v_w_21) * new V(0.82d)), v_l_20, ((-v_w_21) * new V(0.06d)));
v_ctx_17[new V("quadraticCurveTo")].Call((v_l_20 * new V(0.48d)), (v_w_21 * new V(0.32d)), new V(0d), (v_w_21 * new V(0.22d)));
v_ctx_17[new V("closePath")].Call();
v_ctx_17[new V("fill")].Call();
v_ctx_17[new V("stroke")].Call();
v_ctx_17[new V("restore")].Call();
return default;
}
private static V F_shard(V[] args) {
V v_ctx_25 = VisualJs.Arg(args,0);
V v_x_26 = VisualJs.Arg(args,1);
V v_y_27 = VisualJs.Arg(args,2);
V v_s_28 = VisualJs.Arg(args,3);
V v_c_29 = VisualJs.Arg(args,4);
V v_a_30 = VisualJs.Arg(args,5);
V v_alpha_31 = VisualJs.Arg(args,6).Undefined?new V(0.8d):VisualJs.Arg(args,6);
v_ctx_25[new V("save")].Call();
v_ctx_25[new V("translate")].Call(v_x_26, v_y_27);
v_ctx_25[new V("rotate")].Call(v_a_30);
VisualJs.Assign(v_ctx_25,new V("globalAlpha"),"=",v_alpha_31);
VisualJs.Assign(v_ctx_25,new V("fillStyle"),"=",new V("rgba(4,9,16,.94)"));
VisualJs.Assign(v_ctx_25,new V("strokeStyle"),"=",v_c_29);
VisualJs.Assign(v_ctx_25,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.55d), (v_s_28 * new V(0.11d))));
v_ctx_25[new V("beginPath")].Call();
v_ctx_25[new V("moveTo")].Call(v_s_28, new V(0d));
v_ctx_25[new V("lineTo")].Call(new V(0d), ((-v_s_28) * new V(0.54d)));
v_ctx_25[new V("lineTo")].Call(((-v_s_28) * new V(0.68d)), ((-v_s_28) * new V(0.25d)));
v_ctx_25[new V("lineTo")].Call(((-v_s_28) * new V(0.44d)), (v_s_28 * new V(0.28d)));
v_ctx_25[new V("closePath")].Call();
v_ctx_25[new V("fill")].Call();
v_ctx_25[new V("stroke")].Call();
v_ctx_25[new V("restore")].Call();
return default;
}
private static V F_arc(V[] args) {
V v_ctx_32 = VisualJs.Arg(args,0);
V v_r_33 = VisualJs.Arg(args,1);
V v_c_34 = VisualJs.Arg(args,2);
V v_w_35 = VisualJs.Arg(args,3);
V v_a0_36 = VisualJs.Arg(args,4);
V v_a1_37 = VisualJs.Arg(args,5);
V v_alpha_38 = VisualJs.Arg(args,6).Undefined?new V(0.65d):VisualJs.Arg(args,6);
v_ctx_32[new V("save")].Call();
VisualJs.Assign(v_ctx_32,new V("strokeStyle"),"=",v_c_34);
VisualJs.Assign(v_ctx_32,new V("lineWidth"),"=",v_w_35);
VisualJs.Assign(v_ctx_32,new V("globalAlpha"),"=",v_alpha_38);
VisualJs.Assign(v_ctx_32,new V("lineCap"),"=",new V("round"));
v_ctx_32[new V("beginPath")].Call();
v_ctx_32[new V("arc")].Call(new V(0d), new V(0d), v_r_33, v_a0_36, v_a1_37);
v_ctx_32[new V("stroke")].Call();
v_ctx_32[new V("restore")].Call();
return default;
}
private static V F_tether(V[] args) {
V v_ctx_39 = VisualJs.Arg(args,0);
V v_sx_40 = VisualJs.Arg(args,1);
V v_sy_41 = VisualJs.Arg(args,2);
V v_ex_42 = VisualJs.Arg(args,3);
V v_ey_43 = VisualJs.Arg(args,4);
V v_b_44 = VisualJs.Arg(args,5);
V v_c_45 = VisualJs.Arg(args,6);
V v_w_46 = VisualJs.Arg(args,7);
V v_alpha_47 = VisualJs.Arg(args,8).Undefined?new V(0.65d):VisualJs.Arg(args,8);
V v_phase_48 = VisualJs.Arg(args,9).Undefined?new V(0d):VisualJs.Arg(args,9);
V v_dx_49 = (v_ex_42 - v_sx_40);
V v_dy_50 = (v_ey_43 - v_sy_41);
V v_len_51 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_dx_49, v_dy_50), () => new V(1d));
V v_nx_52 = ((-v_dy_50) / v_len_51);
V v_ny_53 = (v_dx_49 / v_len_51);
v_ctx_39[new V("save")].Call();
VisualJs.Assign(v_ctx_39,new V("strokeStyle"),"=",v_c_45);
VisualJs.Assign(v_ctx_39,new V("lineWidth"),"=",v_w_46);
VisualJs.Assign(v_ctx_39,new V("lineCap"),"=",new V("round"));
VisualJs.Assign(v_ctx_39,new V("globalAlpha"),"=",v_alpha_47);
v_ctx_39[new V("beginPath")].Call();
v_ctx_39[new V("moveTo")].Call(v_sx_40, v_sy_41);
v_ctx_39[new V("quadraticCurveTo")].Call(((((v_sx_40 + v_ex_42)) / new V(2d)) + (v_nx_52 * ((v_b_44 + ((VisualJs.MathObject[new V("sin")].Call(v_phase_48) * v_w_46) * new V(2d)))))), ((((v_sy_41 + v_ey_43)) / new V(2d)) + (v_ny_53 * ((v_b_44 + ((VisualJs.MathObject[new V("sin")].Call(v_phase_48) * v_w_46) * new V(2d)))))), v_ex_42, v_ey_43);
v_ctx_39[new V("stroke")].Call();
v_ctx_39[new V("restore")].Call();
return default;
}
private static V F_eye(V[] args) {
V v_ctx_54 = VisualJs.Arg(args,0);
V v_x_55 = VisualJs.Arg(args,1);
V v_y_56 = VisualJs.Arg(args,2);
V v_s_57 = VisualJs.Arg(args,3);
V v_c_58 = VisualJs.Arg(args,4);
V v_a_59 = VisualJs.Arg(args,5).Undefined?new V(0d):VisualJs.Arg(args,5);
v_ctx_54[new V("save")].Call();
v_ctx_54[new V("translate")].Call(v_x_55, v_y_56);
v_ctx_54[new V("rotate")].Call(v_a_59);
VisualJs.Assign(v_ctx_54,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_54,new V("strokeStyle"),"=",v_c_58);
VisualJs.Assign(v_ctx_54,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.6d), (v_s_57 * new V(0.14d))));
VisualJs.Assign(v_ctx_54,new V("globalAlpha"),"=",new V(0.9d));
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("moveTo")].Call((-v_s_57), new V(0d));
v_ctx_54[new V("quadraticCurveTo")].Call(new V(0d), ((-v_s_57) * new V(0.55d)), v_s_57, new V(0d));
v_ctx_54[new V("quadraticCurveTo")].Call(new V(0d), (v_s_57 * new V(0.55d)), (-v_s_57), new V(0d));
v_ctx_54[new V("stroke")].Call();
VisualJs.Assign(v_ctx_54,new V("fillStyle"),"=",new V("#ffffff"));
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("ellipse")].Call(new V(0d), new V(0d), (v_s_57 * new V(0.23d)), (v_s_57 * new V(0.48d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_54[new V("fill")].Call();
VisualJs.Assign(v_ctx_54,new V("fillStyle"),"=",v_c_58);
v_ctx_54[new V("beginPath")].Call();
v_ctx_54[new V("ellipse")].Call(new V(0d), new V(0d), (v_s_57 * new V(0.1d)), (v_s_57 * new V(0.32d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_54[new V("fill")].Call();
v_ctx_54[new V("restore")].Call();
return default;
}
private static V F_pulseRing(V[] args) {
V v_ctx_60 = VisualJs.Arg(args,0);
V v_r_61 = VisualJs.Arg(args,1);
V v_c_62 = VisualJs.Arg(args,2);
V v_t_63 = VisualJs.Arg(args,3);
V v_scale_64 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_q_65 = (((v_t_63 * new V(0.62d))) % new V(1d));
Scope[new V("arc")].Call(v_ctx_60, (v_r_61 * ((new V(0.72d) + (v_q_65 * new V(1.02d))))), v_c_62, VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_61 * new V(0.028d))), new V(0d), Scope[new V("TAU")], (((new V(0.44d) * ((new V(1d) - v_q_65)))) * v_scale_64));
return default;
}
private static V F_attackPeak(V[] args) {
V v_s_66 = VisualJs.Arg(args,0);
V v_d_67 = VisualJs.MathObject[new V("max")].Call(new V(0.15d), VisualJs.Or(v_s_66[new V("attackDelay")], () => new V(1d)));
V v_p_68 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (new V(1d) - (v_s_66[new V("attackTimer")] / v_d_67))));
return VisualJs.MathObject[new V("exp")].Call((-VisualJs.MathObject[new V("pow")].Call((((v_p_68 - new V(0.95d))) / new V(0.07d)), new V(2d))));
}
private static V F_emit(V[] args) {
V v_ctx_69 = VisualJs.Arg(args,0);
V v_r_70 = VisualJs.Arg(args,1);
V v_c_71 = VisualJs.Arg(args,2);
V v_t_72 = VisualJs.Arg(args,3);
V v_branch_73 = VisualJs.Arg(args,4);
V v_final_74 = VisualJs.Arg(args,5);
V v_sphere_75 = VisualJs.Arg(args,6);
V v_p_76 = Scope[new V("attackPeak")].Call(v_sphere_75);
if (VisualJs.Or((!VisualJs.NumberObject[new V("isFinite")].Call(v_p_76)), () => new V(v_p_76 < new V(0.03d))).Truth) return default;
v_ctx_69[new V("save")].Call();
VisualJs.Assign(v_ctx_69,new V("globalCompositeOperation"),"=",new V("lighter"));
V v_strength_77 = (new V(0.28d) + (new V(0.72d) * v_p_76));
if (new V(v_branch_73 == new V("standard_resonator")).Truth) {
for (V v_i_78 = new V(0d); new V(v_i_78 < new V(3d)).Truth; v_i_78++) {
V v_a_79 = ((v_t_72 * new V(0.55d)) + ((v_i_78 * Scope[new V("TAU")]) / new V(3d)));
Scope[new V("fin")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_79) * v_r_70) * new V(1.18d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_79) * v_r_70) * new V(0.72d)), (v_r_70 * new V(0.44d)), (v_r_70 * new V(0.13d)), v_c_71, (v_a_79 + new V(0.2d)), (new V(0.45d) + (new V(0.4d) * v_p_76)));
}
Scope[new V("pulseRing")].Call(v_ctx_69, (v_r_70 * new V(1.25d)), v_c_71, v_t_72, new V(1d));
} else if (new V(v_branch_73 == new V("standard_singularity")).Truth) {
for (V v_i_80 = new V(0d); new V(v_i_80 < new V(4d)).Truth; v_i_80++) {
V v_a_81 = (((-v_t_72) * new V(0.35d)) + ((v_i_80 * Scope[new V("TAU")]) / new V(4d)));
Scope[new V("tether")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_81) * v_r_70) * new V(1.15d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_81) * v_r_70) * new V(0.7d)), ((VisualJs.MathObject[new V("cos")].Call((v_a_81 + new V(0.6d))) * v_r_70) * new V(0.42d)), ((VisualJs.MathObject[new V("sin")].Call((v_a_81 + new V(0.6d))) * v_r_70) * new V(0.25d)), (v_r_70 * new V(0.16d)), v_c_71, new V(1.1d), (new V(0.35d) + (new V(0.4d) * v_p_76)), (v_t_72 + v_i_80));
}
Scope[new V("arc")].Call(v_ctx_69, (v_r_70 * ((new V(1.08d) + (v_p_76 * new V(0.42d))))), new V("#d8b9ff"), new V(1.2d), new V(0.4d), new V(2.55d), new V(0.8d));
} else if (new V(v_branch_73 == new V("standard_swarm")).Truth) {
for (V v_i_82 = new V(0d); new V(v_i_82 < new V(5d)).Truth; v_i_82++) {
V v_a_83 = ((v_t_72 * new V(0.48d)) + ((v_i_82 * Scope[new V("TAU")]) / new V(5d)));
Scope[new V("shard")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_83) * v_r_70) * ((new V(1d) + (v_strength_77 * new V(0.5d))))), ((VisualJs.MathObject[new V("sin")].Call(v_a_83) * v_r_70) * new V(0.67d)), (v_r_70 * new V(0.16d)), v_c_71, (v_a_83 + new V(0.7d)), (new V(0.45d) + (new V(0.4d) * v_p_76)));
}
} else if (new V(v_branch_73 == new V("sniper_oracle")).Truth) {
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",new V("#f5ddff"));
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.2d));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.45d) + (new V(0.45d) * v_p_76)));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("arc")].Call((v_r_70 * new V(1.15d)), new V(0d), (v_r_70 * ((new V(0.25d) + (new V(0.07d) * v_p_76)))), (-new V(1d)), new V(1d));
v_ctx_69[new V("stroke")].Call();
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.75d)), ((-v_r_70) * new V(0.06d)));
v_ctx_69[new V("lineTo")].Call((v_r_70 * ((new V(1.62d) + (new V(0.2d) * v_p_76)))), new V(0d));
v_ctx_69[new V("lineTo")].Call((v_r_70 * new V(0.75d)), (v_r_70 * new V(0.06d)));
v_ctx_69[new V("stroke")].Call();
} else if (new V(v_branch_73 == new V("sniper_assassin")).Truth) {
foreach (V v_y_84 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",v_c_71);
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.2d));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.35d) + (new V(0.45d) * v_p_76)));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.65d)), ((v_y_84 * v_r_70) * new V(0.16d)));
v_ctx_69[new V("quadraticCurveTo")].Call((v_r_70 * ((new V(1.2d) + (v_p_76 * new V(0.25d))))), ((v_y_84 * v_r_70) * new V(0.3d)), (v_r_70 * ((new V(1.72d) + (v_p_76 * new V(0.2d))))), ((v_y_84 * v_r_70) * new V(0.05d)));
v_ctx_69[new V("stroke")].Call();
}
} else if (new V(v_branch_73 == new V("sniper_beacon")).Truth) {
for (V v_i_85 = new V(0d); new V(v_i_85 < new V(3d)).Truth; v_i_85++) Scope[new V("pulseRing")].Call(v_ctx_69, (v_r_70 * ((new V(1.05d) + (v_i_85 * new V(0.25d))))), v_c_71, (v_t_72 + (v_i_85 * new V(0.17d))), new V(1d));
Scope[new V("fin")].Call(v_ctx_69, new V(0d), ((-v_r_70) * new V(1.05d)), (v_r_70 * new V(0.4d)), (v_r_70 * new V(0.13d)), v_c_71, ((-VisualJs.MathObject[new V("PI")]) / new V(2d)), new V(0.8d));
} else if (new V(v_branch_73 == new V("shotgun_burst")).Truth) {
for (V v_i_86 = new V(0d); new V(v_i_86 < new V(3d)).Truth; v_i_86++) {
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",v_c_71);
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.3d));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.4d) + (new V(0.3d) * v_p_76)));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.72d)), ((((v_i_86 - new V(1d))) * v_r_70) * new V(0.15d)));
v_ctx_69[new V("lineTo")].Call((v_r_70 * ((new V(1.58d) + (v_p_76 * new V(0.3d))))), ((((v_i_86 - new V(1d))) * v_r_70) * new V(0.22d)));
v_ctx_69[new V("stroke")].Call();
}
} else if (new V(v_branch_73 == new V("shotgun_cataclysm")).Truth) {
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",new V("#ffd29c"));
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.4d));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.4d) + (new V(0.5d) * v_p_76)));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.65d)), ((-v_r_70) * new V(0.2d)));
v_ctx_69[new V("lineTo")].Call((v_r_70 * ((new V(1.28d) + (v_p_76 * new V(0.3d))))), new V(0d));
v_ctx_69[new V("lineTo")].Call((v_r_70 * new V(0.65d)), (v_r_70 * new V(0.2d)));
v_ctx_69[new V("stroke")].Call();
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("arc")].Call((v_r_70 * ((new V(1.35d) + (v_p_76 * new V(0.2d))))), new V(0d), (v_r_70 * ((new V(0.08d) + (new V(0.06d) * v_p_76)))), new V(0d), Scope[new V("TAU")]);
v_ctx_69[new V("stroke")].Call();
} else if (new V(v_branch_73 == new V("shotgun_hail")).Truth) {
for (V v_i_87 = new V(0d); new V(v_i_87 < new V(6d)).Truth; v_i_87++) {
V v_a_88 = ((-new V(0.75d)) + (v_i_87 * new V(0.3d)));
V v_rr_89 = (v_r_70 * ((new V(1.05d) + (v_p_76 * new V(0.55d)))));
Scope[new V("shard")].Call(v_ctx_69, (VisualJs.MathObject[new V("cos")].Call(v_a_88) * v_rr_89), ((VisualJs.MathObject[new V("sin")].Call(v_a_88) * v_rr_89) * new V(0.55d)), (v_r_70 * new V(0.1d)), v_c_71, (v_a_88 + new V(0.7d)), (new V(0.35d) + (new V(0.4d) * v_p_76)));
}
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("chain_web")), () => new V(v_branch_73 == new V("chain_storm"))), () => new V(v_branch_73 == new V("chain_leech"))).Truth) {
V v_count_90 = (new V(v_branch_73 == new V("chain_storm")).Truth ? new V(7d) : new V(8d));
for (V v_i_91 = new V(0d); new V(v_i_91 < v_count_90).Truth; v_i_91++) {
V v_a_92 = ((v_t_72 * new V(1.3d)) + ((v_i_91 * Scope[new V("TAU")]) / v_count_90));
V v_rr_93 = (v_r_70 * ((new V(0.8d) + (v_strength_77 * new V(0.9d)))));
Scope[new V("tether")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_92) * v_r_70) * new V(0.18d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_92) * v_r_70) * new V(0.13d)), (VisualJs.MathObject[new V("cos")].Call(v_a_92) * v_rr_93), ((VisualJs.MathObject[new V("sin")].Call(v_a_92) * v_rr_93) * new V(0.62d)), (v_r_70 * new V(0.1d)), v_c_71, (new V(v_branch_73 == new V("chain_storm")).Truth ? new V(1.35d) : new V(1d)), (new V(0.3d) + (new V(0.45d) * v_p_76)), v_a_92);
}
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("aura_sanctum")), () => new V(v_branch_73 == new V("aura_gravity"))), () => new V(v_branch_73 == new V("aura_overgrowth"))).Truth) {
for (V v_i_94 = new V(0d); new V(v_i_94 < new V(6d)).Truth; v_i_94++) {
V v_a_95 = ((v_t_72 * new V(0.3d)) + ((v_i_94 * Scope[new V("TAU")]) / new V(6d)));
V v_rr_96 = (v_r_70 * ((new V(1d) + (v_strength_77 * new V(0.8d)))));
Scope[new V("tether")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_95) * v_r_70) * new V(0.25d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_95) * v_r_70) * new V(0.16d)), (VisualJs.MathObject[new V("cos")].Call(v_a_95) * v_rr_96), ((VisualJs.MathObject[new V("sin")].Call(v_a_95) * v_rr_96) * new V(0.58d)), (v_r_70 * new V(0.18d)), v_c_71, new V(1d), (new V(0.22d) + (new V(0.5d) * v_p_76)), v_a_95);
}
Scope[new V("arc")].Call(v_ctx_69, (v_r_70 * ((new V(1.15d) + (v_strength_77 * new V(0.55d))))), v_c_71, new V(1d), ((-VisualJs.MathObject[new V("PI")]) * new V(0.75d)), (VisualJs.MathObject[new V("PI")] * new V(0.75d)), (new V(0.35d) + (new V(0.35d) * v_p_76)));
} else if (new V(v_branch_73 == new V("orbital_dance")).Truth) {
V v_n_97 = (new V(v_final_74 == new V(2d)).Truth ? new V(6d) : new V(4d));
for (V v_i_98 = new V(0d); new V(v_i_98 < v_n_97).Truth; v_i_98++) {
V v_a_99 = ((v_t_72 * new V(1.8d)) + ((v_i_98 * Scope[new V("TAU")]) / v_n_97));
Scope[new V("fin")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_99) * v_r_70) * ((new V(1.04d) + (v_strength_77 * new V(0.5d))))), ((VisualJs.MathObject[new V("sin")].Call(v_a_99) * v_r_70) * new V(0.62d)), (v_r_70 * ((new V(0.3d) + (new V(0.18d) * v_p_76)))), (v_r_70 * new V(0.12d)), v_c_71, v_a_99, (new V(0.32d) + (new V(0.48d) * v_p_76)));
}
} else if (new V(v_branch_73 == new V("orbital_halo")).Truth) {
for (V v_i_100 = new V(0d); new V(v_i_100 < new V(3d)).Truth; v_i_100++) {
V v_rr_101 = (v_r_70 * ((new V(1.08d) + (v_i_100 * new V(0.18d)))));
Scope[new V("pulseRing")].Call(v_ctx_69, v_rr_101, v_c_71, (v_t_72 + (v_i_100 * new V(0.16d))), (new V(0.75d) + (new V(0.18d) * v_p_76)));
}
Scope[new V("arc")].Call(v_ctx_69, (v_r_70 * new V(1.48d)), v_c_71, new V(1.2d), (-new V(0.72d)), new V(0.72d), (new V(0.55d) + (new V(0.25d) * v_p_76)));
} else if (new V(v_branch_73 == new V("orbital_blade")).Truth) {
for (V v_i_102 = new V(0d); new V(v_i_102 < new V(4d)).Truth; v_i_102++) {
V v_a_103 = ((v_t_72 * new V(0.72d)) + ((v_i_102 * Scope[new V("TAU")]) / new V(4d)));
V v_x_104 = ((VisualJs.MathObject[new V("cos")].Call(v_a_103) * v_r_70) * new V(1.14d));
V v_y_105 = ((VisualJs.MathObject[new V("sin")].Call(v_a_103) * v_r_70) * new V(0.68d));
Scope[new V("fin")].Call(v_ctx_69, v_x_104, v_y_105, (v_r_70 * ((new V(0.38d) + (new V(0.12d) * v_p_76)))), (v_r_70 * new V(0.1d)), v_c_71, v_a_103, (new V(0.45d) + (new V(0.45d) * v_p_76)));
}
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("prism_split")), () => new V(v_branch_73 == new V("prism_spectrum"))), () => new V(v_branch_73 == new V("prism_mirror"))).Truth) {
V v_n_106 = (new V(v_branch_73 == new V("prism_spectrum")).Truth ? new V(5d) : (new V(v_branch_73 == new V("prism_split")).Truth ? new V(3d) : new V(2d)));
for (V v_i_107 = new V(0d); new V(v_i_107 < v_n_106).Truth; v_i_107++) {
V v_y_108 = ((((v_i_107 - (((v_n_106 - new V(1d))) / new V(2d)))) * v_r_70) * new V(0.28d));
Scope[new V("shard")].Call(v_ctx_69, (v_r_70 * ((new V(1d) + (v_strength_77 * new V(0.25d))))), v_y_108, (v_r_70 * new V(0.13d)), v_c_71, new V(0d), (new V(0.38d) + (new V(0.45d) * v_p_76)));
}
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",new V("#ffffff"));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.3d) + (new V(0.45d) * v_p_76)));
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1d));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.72d)), new V(0d));
v_ctx_69[new V("lineTo")].Call((v_r_70 * ((new V(1.68d) + (v_p_76 * new V(0.2d))))), new V(0d));
v_ctx_69[new V("stroke")].Call();
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("gravity_well")), () => new V(v_branch_73 == new V("gravity_tide"))), () => new V(v_branch_73 == new V("gravity_collapse"))).Truth) {
for (V v_i_109 = new V(0d); new V(v_i_109 < new V(3d)).Truth; v_i_109++) {
V v_q_110 = (v_i_109 / new V(3d));
V v_a_111 = (((-v_t_72) * new V(0.3d)) + (v_q_110 * new V(1.5d)));
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",v_c_71);
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.22d) + (new V(0.46d) * v_p_76)));
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.1d));
v_ctx_69[new V("beginPath")].Call();
for (V v_k_112 = new V(0d); new V(v_k_112 <= new V(14d)).Truth; v_k_112++) {
V v_u_113 = (v_k_112 / new V(14d));
V v_rr_114 = (v_r_70 * ((new V(0.35d) + (v_u_113 * ((new V(0.85d) + (v_strength_77 * new V(0.7d))))))));
V v_ang_115 = (v_a_111 + (v_u_113 * new V(2.15d)));
V v_x_116 = (VisualJs.MathObject[new V("cos")].Call(v_ang_115) * v_rr_114);
V v_y_117 = ((VisualJs.MathObject[new V("sin")].Call(v_ang_115) * v_rr_114) * new V(0.56d));
if (new V(v_k_112 == new V(0d)).Truth) v_ctx_69[new V("moveTo")].Call(v_x_116, v_y_117); else v_ctx_69[new V("lineTo")].Call(v_x_116, v_y_117);
}
v_ctx_69[new V("stroke")].Call();
}
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("pulse_wave")), () => new V(v_branch_73 == new V("pulse_resonator"))), () => new V(v_branch_73 == new V("pulse_burst"))).Truth) {
V v_lobes_118 = (new V(v_branch_73 == new V("pulse_resonator")).Truth ? new V(8d) : (new V(v_branch_73 == new V("pulse_wave")).Truth ? new V(6d) : new V(5d)));
V v_rr_119 = (v_r_70 * ((new V(0.9d) + (v_strength_77 * new V(0.85d)))));
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",v_c_71);
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.25d) + (new V(0.55d) * v_p_76)));
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.1d));
v_ctx_69[new V("beginPath")].Call();
for (V v_i_120 = new V(0d); new V(v_i_120 <= new V(48d)).Truth; v_i_120++) {
V v_a_121 = ((v_i_120 / new V(48d)) * Scope[new V("TAU")]);
V v_q_122 = ((new V(1d) + (new V(0.16d) * VisualJs.MathObject[new V("sin")].Call(((v_a_121 * v_lobes_118) + (v_t_72 * new V(4.8d)))))) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call(((v_a_121 * ((v_lobes_118 + new V(2d)))) - (v_t_72 * new V(3.4d))))));
V v_x_123 = ((VisualJs.MathObject[new V("cos")].Call(v_a_121) * v_rr_119) * v_q_122);
V v_y_124 = (((VisualJs.MathObject[new V("sin")].Call(v_a_121) * v_rr_119) * new V(0.54d)) * v_q_122);
if (new V(v_i_120 == new V(0d)).Truth) v_ctx_69[new V("moveTo")].Call(v_x_123, v_y_124); else v_ctx_69[new V("lineTo")].Call(v_x_123, v_y_124);
}
v_ctx_69[new V("stroke")].Call();
if (new V(v_branch_73 == new V("pulse_burst")).Truth) Scope[new V("arc")].Call(v_ctx_69, (v_rr_119 * new V(0.46d)), new V("#fff0a9"), new V(1d), new V(0d), Scope[new V("TAU")], (new V(0.4d) + (new V(0.4d) * v_p_76)));
} else if (VisualJs.Or(VisualJs.Or(new V(v_branch_73 == new V("void_hunger")), () => new V(v_branch_73 == new V("void_reaper"))), () => new V(v_branch_73 == new V("void_execution"))).Truth) {
for (V v_i_125 = new V(0d); new V(v_i_125 < new V(4d)).Truth; v_i_125++) {
V v_a_126 = ((v_t_72 * new V(0.24d)) + ((v_i_125 * Scope[new V("TAU")]) / new V(4d)));
Scope[new V("tether")].Call(v_ctx_69, ((VisualJs.MathObject[new V("cos")].Call(v_a_126) * v_r_70) * new V(0.3d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_126) * v_r_70) * new V(0.2d)), ((VisualJs.MathObject[new V("cos")].Call((v_a_126 + new V(0.55d))) * v_r_70) * ((new V(1.1d) + (v_strength_77 * new V(0.55d))))), ((VisualJs.MathObject[new V("sin")].Call((v_a_126 + new V(0.55d))) * v_r_70) * new V(0.65d)), (v_r_70 * new V(0.18d)), v_c_71, new V(1.25d), (new V(0.3d) + (new V(0.46d) * v_p_76)), v_a_126);
}
if (new V(v_branch_73 == new V("void_reaper")).Truth) Scope[new V("fin")].Call(v_ctx_69, (v_r_70 * new V(1.02d)), new V(0d), (v_r_70 * ((new V(0.62d) + (new V(0.18d) * v_p_76)))), (v_r_70 * new V(0.18d)), v_c_71, new V(0d), new V(0.8d)); else if (new V(v_branch_73 == new V("void_execution")).Truth) {
VisualJs.Assign(v_ctx_69,new V("strokeStyle"),"=",v_c_71);
VisualJs.Assign(v_ctx_69,new V("lineWidth"),"=",new V(1.25d));
VisualJs.Assign(v_ctx_69,new V("globalAlpha"),"=",(new V(0.4d) + (new V(0.4d) * v_p_76)));
v_ctx_69[new V("beginPath")].Call();
v_ctx_69[new V("moveTo")].Call((v_r_70 * new V(0.8d)), ((-v_r_70) * new V(0.28d)));
v_ctx_69[new V("lineTo")].Call((v_r_70 * ((new V(1.55d) + (new V(0.25d) * v_p_76)))), new V(0d));
v_ctx_69[new V("lineTo")].Call((v_r_70 * new V(0.8d)), (v_r_70 * new V(0.28d)));
v_ctx_69[new V("stroke")].Call();
}
}
v_ctx_69[new V("restore")].Call();
return default;
}
private static V F_drawFinalExpansion(V[] args) {
V v_ctx_127 = VisualJs.Arg(args,0);
V v_r_128 = VisualJs.Arg(args,1);
V v_c_129 = VisualJs.Arg(args,2);
V v_t_130 = VisualJs.Arg(args,3);
V v_branch_131 = VisualJs.Arg(args,4);
V v_final_132 = VisualJs.Arg(args,5);
V v_sphere_133 = VisualJs.Arg(args,6);
if (new V(v_final_132 < new V(0d)).Truth) return default;
V v_mode_134 = (new V(v_final_132 == new V(0d)).Truth ? new V("core") : (new V(v_final_132 == new V(1d)).Truth ? new V("cascade") : new V("apex")));
V v_family_135 = v_branch_131[new V("split")].Call(new V("_"))[new V(0d)];
V v_dir_136 = (VisualJs.NumberObject[new V("isFinite")].Call(v_sphere_133[new V("rotation")]).Truth ? v_sphere_133[new V("rotation")] : new V(0d));
V v_phase_137 = (v_t_130 * ((new V(v_mode_134 == new V("core")).Truth ? new V(0.72d) : (new V(v_mode_134 == new V("cascade")).Truth ? new V(1.05d) : new V(1.35d)))));
V v_rgb_138 = Scope[new V("rgbOf")].Call(v_c_129);
v_ctx_127[new V("save")].Call();
VisualJs.Assign(v_ctx_127,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_127,new V("lineCap"),"=",new V("round"));
VisualJs.Assign(v_ctx_127,new V("lineJoin"),"=",new V("round"));
if (new V(v_family_135 == new V("orbital")).Truth) {
V v_count_139 = (new V(v_mode_134 == new V("core")).Truth ? new V(3d) : (new V(v_mode_134 == new V("cascade")).Truth ? new V(4d) : new V(5d)));
V v_orbitR_140 = (v_r_128 * ((new V(1.36d) + ((new V(v_mode_134 == new V("apex")).Truth ? new V(0.1d) : new V(0d))))));
for (V v_i_141 = new V(0d); new V(v_i_141 < v_count_139).Truth; v_i_141++) {
V v_a_142 = (v_phase_137 + ((v_i_141 * Scope[new V("TAU")]) / v_count_139));
Scope[new V("shard")].Call(v_ctx_127, (VisualJs.MathObject[new V("cos")].Call(v_a_142) * v_orbitR_140), ((VisualJs.MathObject[new V("sin")].Call(v_a_142) * v_orbitR_140) * new V(0.66d)), (v_r_128 * ((new V(v_mode_134 == new V("apex")).Truth ? new V(0.16d) : new V(0.12d)))), v_c_129, v_a_142, new V(0.72d));
}
if (new V(v_mode_134 == new V("cascade")).Truth) Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.18d)), v_c_129, v_t_130, new V(0.8d));
if (new V(v_mode_134 == new V("apex")).Truth) Scope[new V("arc")].Call(v_ctx_127, (v_r_128 * new V(1.52d)), v_c_129, new V(1.4d), (-new V(0.52d)), new V(0.52d), new V(0.72d));
} else if (VisualJs.Or(VisualJs.Or(new V(v_family_135 == new V("chain")), () => new V(v_family_135 == new V("aura"))), () => new V(v_family_135 == new V("gravity"))).Truth) {
if (new V(v_mode_134 == new V("core")).Truth) {
for (V v_i_143 = new V(0d); new V(v_i_143 < new V(3d)).Truth; v_i_143++) {
V v_a_144 = (v_phase_137 + ((v_i_143 * Scope[new V("TAU")]) / new V(3d)));
Scope[new V("tether")].Call(v_ctx_127, ((VisualJs.MathObject[new V("cos")].Call(v_a_144) * v_r_128) * new V(0.34d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_144) * v_r_128) * new V(0.22d)), ((VisualJs.MathObject[new V("cos")].Call(v_a_144) * v_r_128) * new V(1.25d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_144) * v_r_128) * new V(0.7d)), (v_r_128 * new V(0.08d)), v_c_129, new V(1.1d), new V(0.62d), v_a_144);
}
Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.18d)), v_c_129, v_t_130, new V(0.72d));
} else if (new V(v_mode_134 == new V("cascade")).Truth) {
for (V v_i_145 = new V(0d); new V(v_i_145 < new V(4d)).Truth; v_i_145++) {
V v_a_146 = ((-new V(0.92d)) + (v_i_145 * new V(0.62d)));
V v_q_147 = (new V(0.55d) + (new V(0.22d) * VisualJs.MathObject[new V("sin")].Call((v_phase_137 + v_i_145))));
Scope[new V("tether")].Call(v_ctx_127, (v_r_128 * new V(0.1d)), new V(0d), ((VisualJs.MathObject[new V("cos")].Call(v_a_146) * v_r_128) * ((new V(0.92d) + v_q_147))), ((VisualJs.MathObject[new V("sin")].Call(v_a_146) * v_r_128) * ((new V(0.52d) + (v_q_147 * new V(0.35d))))), (v_r_128 * new V(0.1d)), v_c_129, new V(1.05d), (new V(0.42d) + (new V(0.08d) * v_i_145)), (v_phase_137 + v_i_145));
}
Scope[new V("arc")].Call(v_ctx_127, (v_r_128 * new V(1.42d)), v_c_129, new V(1.25d), (-new V(1.05d)), new V(1.05d), new V(0.7d));
} else {
v_ctx_127[new V("save")].Call();
v_ctx_127[new V("rotate")].Call(v_dir_136);
Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * new V(0.62d)), new V(0d), (v_r_128 * new V(0.72d)), (v_r_128 * new V(0.18d)), v_c_129, new V(0d), new V(0.9d));
Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * new V(0.96d)), new V(0d), (v_r_128 * new V(0.48d)), (v_r_128 * new V(0.13d)), v_c_129, new V(0d), new V(0.62d));
v_ctx_127[new V("restore")].Call();
Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.32d)), v_c_129, v_t_130, new V(1d));
}
} else if (new V(v_family_135 == new V("pulse")).Truth) {
V v_lobes_148 = (new V(v_mode_134 == new V("core")).Truth ? new V(6d) : (new V(v_mode_134 == new V("cascade")).Truth ? new V(8d) : new V(10d)));
V v_rr_149 = (v_r_128 * ((new V(1.24d) + ((new V(v_mode_134 == new V("apex")).Truth ? new V(0.12d) : new V(0d))))));
VisualJs.Assign(v_ctx_127,new V("strokeStyle"),"=",v_c_129);
VisualJs.Assign(v_ctx_127,new V("globalAlpha"),"=",new V(0.62d));
VisualJs.Assign(v_ctx_127,new V("lineWidth"),"=",(new V(v_mode_134 == new V("apex")).Truth ? new V(1.55d) : new V(1.15d)));
v_ctx_127[new V("beginPath")].Call();
for (V v_i_150 = new V(0d); new V(v_i_150 <= new V(60d)).Truth; v_i_150++) {
V v_a_151 = ((v_i_150 / new V(60d)) * Scope[new V("TAU")]);
V v_q_152 = ((new V(1d) + (new V(0.12d) * VisualJs.MathObject[new V("sin")].Call(((v_a_151 * v_lobes_148) + v_phase_137)))) + (new V(0.05d) * VisualJs.MathObject[new V("sin")].Call(((v_a_151 * ((v_lobes_148 + new V(2d)))) - (v_phase_137 * new V(1.4d))))));
V v_x_153 = ((VisualJs.MathObject[new V("cos")].Call(v_a_151) * v_rr_149) * v_q_152);
V v_y_154 = (((VisualJs.MathObject[new V("sin")].Call(v_a_151) * v_rr_149) * new V(0.56d)) * v_q_152);
if (new V(v_i_150 == new V(0d)).Truth) v_ctx_127[new V("moveTo")].Call(v_x_153, v_y_154); else v_ctx_127[new V("lineTo")].Call(v_x_153, v_y_154);
}
v_ctx_127[new V("stroke")].Call();
if (new V(v_mode_134 == new V("cascade")).Truth) Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.48d)), v_c_129, v_t_130, new V(0.75d));
if (new V(v_mode_134 == new V("apex")).Truth) Scope[new V("arc")].Call(v_ctx_127, (v_r_128 * new V(1.58d)), ((new V("rgba(") + v_rgb_138) + new V(",.95)")), new V(1.6d), (-new V(0.28d)), new V(0.28d), new V(0.85d));
} else if (new V(v_family_135 == new V("prism")).Truth) {
V v_count_155 = (new V(v_mode_134 == new V("core")).Truth ? new V(3d) : (new V(v_mode_134 == new V("cascade")).Truth ? new V(4d) : new V(5d)));
for (V v_i_156 = new V(0d); new V(v_i_156 < v_count_155).Truth; v_i_156++) {
V v_a_157 = ((((v_i_156 - (((v_count_155 - new V(1d))) / new V(2d)))) * new V(0.22d)) + (VisualJs.MathObject[new V("sin")].Call((v_phase_137 + v_i_156)) * new V(0.03d)));
V v_rr_158 = (v_r_128 * ((new V(1.1d) + ((new V(v_mode_134 == new V("apex")).Truth ? new V(0.18d) : new V(0d))))));
Scope[new V("shard")].Call(v_ctx_127, (VisualJs.MathObject[new V("cos")].Call(v_a_157) * v_rr_158), ((VisualJs.MathObject[new V("sin")].Call(v_a_157) * v_rr_158) * new V(0.38d)), (v_r_128 * ((new V(v_mode_134 == new V("apex")).Truth ? new V(0.15d) : new V(0.11d)))), v_c_129, v_a_157, new V(0.78d));
}
if (new V(v_mode_134 == new V("cascade")).Truth) Scope[new V("tether")].Call(v_ctx_127, (v_r_128 * new V(0.62d)), new V(0d), (v_r_128 * new V(1.58d)), new V(0d), (v_r_128 * new V(0.08d)), new V("#ffffff"), new V(0.8d), new V(0.52d), v_phase_137);
if (new V(v_mode_134 == new V("apex")).Truth) {
v_ctx_127[new V("save")].Call();
v_ctx_127[new V("rotate")].Call(v_dir_136);
Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * new V(0.72d)), new V(0d), (v_r_128 * new V(0.88d)), (v_r_128 * new V(0.17d)), v_c_129, new V(0d), new V(0.92d));
v_ctx_127[new V("restore")].Call();
}
} else {
v_ctx_127[new V("save")].Call();
if (VisualJs.Or(VisualJs.Or(VisualJs.Or(new V(v_family_135 == new V("sniper")), () => new V(v_family_135 == new V("shotgun"))), () => new V(v_family_135 == new V("standard"))), () => new V(v_family_135 == new V("void"))).Truth) v_ctx_127[new V("rotate")].Call(v_dir_136);
if (new V(v_mode_134 == new V("core")).Truth) {
foreach (V v_y_159 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * new V(0.62d)), ((v_y_159 * v_r_128) * new V(0.28d)), (v_r_128 * new V(0.56d)), (v_r_128 * new V(0.14d)), v_c_129, (new V(v_y_159 > new V(0d)).Truth ? new V(0.18d) : (-new V(0.18d))), new V(0.72d));
Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.22d)), v_c_129, v_t_130, new V(0.72d));
} else if (new V(v_mode_134 == new V("cascade")).Truth) {
for (V v_i_160 = new V(0d); new V(v_i_160 < new V(4d)).Truth; v_i_160++) {
V v_q_161 = (v_i_160 / new V(3d));
Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * ((new V(0.52d) + (v_q_161 * new V(0.58d))))), ((VisualJs.MathObject[new V("sin")].Call((v_phase_137 + v_i_160)) * v_r_128) * new V(0.34d)), (v_r_128 * ((new V(0.34d) + (new V(0.1d) * v_q_161)))), (v_r_128 * ((new V(0.11d) + (new V(0.02d) * v_q_161)))), v_c_129, (v_phase_137 + v_i_160), (new V(0.72d) - (new V(0.08d) * v_q_161)));
}
Scope[new V("arc")].Call(v_ctx_127, (v_r_128 * new V(1.46d)), v_c_129, new V(1.15d), (-new V(0.7d)), new V(0.7d), new V(0.7d));
} else {
Scope[new V("fin")].Call(v_ctx_127, (v_r_128 * new V(0.54d)), new V(0d), (v_r_128 * new V(1.02d)), (v_r_128 * new V(0.22d)), v_c_129, new V(0d), new V(0.94d));
VisualJs.Assign(v_ctx_127,new V("strokeStyle"),"=",v_c_129);
VisualJs.Assign(v_ctx_127,new V("lineWidth"),"=",new V(1.35d));
VisualJs.Assign(v_ctx_127,new V("globalAlpha"),"=",new V(0.82d));
v_ctx_127[new V("beginPath")].Call();
v_ctx_127[new V("moveTo")].Call((v_r_128 * new V(0.82d)), ((-v_r_128) * new V(0.3d)));
v_ctx_127[new V("lineTo")].Call((v_r_128 * new V(1.62d)), new V(0d));
v_ctx_127[new V("lineTo")].Call((v_r_128 * new V(0.82d)), (v_r_128 * new V(0.3d)));
v_ctx_127[new V("stroke")].Call();
Scope[new V("pulseRing")].Call(v_ctx_127, (v_r_128 * new V(1.52d)), v_c_129, v_t_130, new V(0.82d));
}
v_ctx_127[new V("restore")].Call();
}
v_ctx_127[new V("restore")].Call();
return default;
}
private static V F_mutationSilhouette(V[] args) {
V v_ctx_162 = VisualJs.Arg(args,0);
V v_r_163 = VisualJs.Arg(args,1);
V v_c_164 = VisualJs.Arg(args,2);
V v_t_165 = VisualJs.Arg(args,3);
V v_branch_166 = VisualJs.Arg(args,4);
V v_final_167 = VisualJs.Arg(args,5);
v_ctx_162[new V("save")].Call();
VisualJs.Assign(v_ctx_162,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_162,new V("lineJoin"),"=",new V("round"));
VisualJs.Assign(v_ctx_162,new V("lineCap"),"=",new V("round"));
V v_pulse_168 = (new V(0.92d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call((v_t_165 * new V(2.4d)))));
if (new V(v_branch_166 == new V("standard_resonator")).Truth) {
foreach (V v_y_169 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, ((-v_r_163) * new V(0.1d)), ((v_y_169 * v_r_163) * new V(0.62d)), (v_r_163 * new V(0.72d)), (v_r_163 * new V(0.16d)), v_c_164, (new V(v_y_169 > new V(0d)).Truth ? new V(0.55d) : (-new V(0.55d))), new V(0.7d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.22d)), v_c_164, new V(1.1d), (-new V(0.9d)), new V(0.9d), new V(0.6d));
} else if (new V(v_branch_166 == new V("standard_singularity")).Truth) {
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * ((new V(1.16d) + (v_final_167 * new V(0.08d))))), new V("#d8b9ff"), new V(1.4d), new V(0.2d), new V(5.9d), new V(0.72d));
foreach (V v_a_170 in VisualJs.Arr((-new V(0.55d)), new V(0.55d)).Items) Scope[new V("tether")].Call(v_ctx_162, ((VisualJs.MathObject[new V("cos")].Call(v_a_170) * v_r_163) * new V(0.92d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_170) * v_r_163) * new V(0.52d)), new V(0d), new V(0d), (v_r_163 * new V(0.18d)), v_c_164, new V(1.2d), new V(0.55d), v_a_170);
} else if (new V(v_branch_166 == new V("standard_swarm")).Truth) {
foreach (V v_y_171 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
Scope[new V("shard")].Call(v_ctx_162, (v_r_163 * new V(1.02d)), ((v_y_171 * v_r_163) * new V(0.56d)), (v_r_163 * new V(0.18d)), v_c_164, (new V(v_y_171 > new V(0d)).Truth ? new V(0.35d) : (-new V(0.35d))), new V(0.78d));
Scope[new V("shard")].Call(v_ctx_162, (v_r_163 * new V(0.72d)), ((v_y_171 * v_r_163) * new V(0.76d)), (v_r_163 * new V(0.13d)), v_c_164, (new V(v_y_171 > new V(0d)).Truth ? new V(0.8d) : (-new V(0.8d))), new V(0.64d));
}
} else if (new V(v_branch_166 == new V("sniper_oracle")).Truth) {
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.18d)), new V("#f5ddff"), new V(1.5d), (-new V(0.95d)), new V(0.95d), new V(0.82d));
foreach (V v_y_172 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, (v_r_163 * new V(0.42d)), ((v_y_172 * v_r_163) * new V(0.3d)), (v_r_163 * new V(0.72d)), (v_r_163 * new V(0.13d)), v_c_164, (new V(v_y_172 > new V(0d)).Truth ? new V(0.12d) : (-new V(0.12d))), new V(0.88d));
VisualJs.Assign(v_ctx_162,new V("strokeStyle"),"=",new V("#f5ddff"));
VisualJs.Assign(v_ctx_162,new V("lineWidth"),"=",new V(1d));
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",new V(0.78d));
v_ctx_162[new V("beginPath")].Call();
v_ctx_162[new V("moveTo")].Call((v_r_163 * new V(0.78d)), ((-v_r_163) * new V(0.08d)));
v_ctx_162[new V("lineTo")].Call((v_r_163 * new V(1.72d)), new V(0d));
v_ctx_162[new V("lineTo")].Call((v_r_163 * new V(0.78d)), (v_r_163 * new V(0.08d)));
v_ctx_162[new V("stroke")].Call();
} else if (new V(v_branch_166 == new V("sniper_assassin")).Truth) {
foreach (V v_y_173 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, (v_r_163 * new V(0.4d)), ((v_y_173 * v_r_163) * new V(0.28d)), (v_r_163 * new V(0.82d)), (v_r_163 * new V(0.16d)), v_c_164, (new V(v_y_173 > new V(0d)).Truth ? new V(0.34d) : (-new V(0.34d))), new V(0.92d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.42d)), v_c_164, new V(1.1d), (-new V(0.42d)), new V(0.42d), new V(0.7d));
} else if (new V(v_branch_166 == new V("sniper_beacon")).Truth) {
foreach (V v_y_174 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, ((-v_r_163) * new V(0.18d)), ((v_y_174 * v_r_163) * new V(0.48d)), (v_r_163 * new V(0.58d)), (v_r_163 * new V(0.15d)), v_c_164, (new V(v_y_174 > new V(0d)).Truth ? (-new V(0.95d)) : new V(0.95d)), new V(0.82d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.34d)), v_c_164, new V(1.3d), (-new V(0.65d)), new V(0.65d), new V(0.72d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.56d)), v_c_164, new V(0.8d), (-new V(0.28d)), new V(0.28d), new V(0.55d));
} else if (new V(v_branch_166 == new V("shotgun_burst")).Truth) {
foreach (V v_y_175 in VisualJs.Arr((-new V(1d)), new V(0d), new V(1d)).Items) {
V v_yy_176 = ((v_y_175 * v_r_163) * new V(0.22d));
VisualJs.Assign(v_ctx_162,new V("strokeStyle"),"=",v_c_164);
VisualJs.Assign(v_ctx_162,new V("lineWidth"),"=",new V(2d));
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",new V(0.78d));
v_ctx_162[new V("beginPath")].Call();
v_ctx_162[new V("moveTo")].Call((v_r_163 * new V(0.34d)), v_yy_176);
v_ctx_162[new V("lineTo")].Call((v_r_163 * new V(1.16d)), (v_yy_176 * ((new V(1d) + (new V(0.08d) * v_final_167)))));
v_ctx_162[new V("stroke")].Call();
}
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.26d)), v_c_164, new V(1.2d), (-new V(0.42d)), new V(0.42d), new V(0.55d));
} else if (new V(v_branch_166 == new V("shotgun_cataclysm")).Truth) {
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.3d)), new V("#ffd29c"), new V(2d), (-new V(0.34d)), new V(0.34d), new V(0.86d));
foreach (V v_y_177 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, (v_r_163 * new V(0.3d)), ((v_y_177 * v_r_163) * new V(0.32d)), (v_r_163 * new V(0.72d)), (v_r_163 * new V(0.18d)), v_c_164, (new V(v_y_177 > new V(0d)).Truth ? new V(0.18d) : (-new V(0.18d))), new V(0.9d));
VisualJs.Assign(v_ctx_162,new V("fillStyle"),"=",new V("rgba(255,169,77,.28)"));
v_ctx_162[new V("beginPath")].Call();
v_ctx_162[new V("arc")].Call((v_r_163 * new V(1.3d)), new V(0d), (v_r_163 * ((new V(0.18d) + (new V(0.04d) * v_final_167)))), new V(0d), Scope[new V("TAU")]);
v_ctx_162[new V("fill")].Call();
} else if (new V(v_branch_166 == new V("shotgun_hail")).Truth) {
for (V v_i_178 = new V(0d); new V(v_i_178 < new V(5d)).Truth; v_i_178++) {
V v_y_179 = ((((v_i_178 - new V(2d))) * v_r_163) * new V(0.24d));
Scope[new V("shard")].Call(v_ctx_162, (v_r_163 * ((new V(0.78d) + (new V(0.06d) * ((v_i_178 % new V(2d))))))), v_y_179, (v_r_163 * new V(0.12d)), v_c_164, new V(0d), new V(0.72d));
}
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.12d)), v_c_164, new V(1d), (-new V(0.72d)), new V(0.72d), new V(0.56d));
} else if (new V(v_branch_166 == new V("chain_web")).Truth) {
foreach (V v_y_180 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("tether")].Call(v_ctx_162, ((-v_r_163) * new V(0.25d)), ((v_y_180 * v_r_163) * new V(0.2d)), (v_r_163 * new V(1.1d)), ((v_y_180 * v_r_163) * new V(0.72d)), (v_r_163 * new V(0.18d)), v_c_164, new V(1.1d), new V(0.62d), (v_y_180 * new V(0.8d)));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.15d)), v_c_164, new V(1d), (-new V(0.9d)), new V(0.9d), new V(0.58d));
} else if (new V(v_branch_166 == new V("chain_storm")).Truth) {
for (V v_i_181 = new V(0d); new V(v_i_181 < new V(5d)).Truth; v_i_181++) {
V v_a_182 = ((-new V(0.8d)) + (v_i_181 * new V(0.4d)));
Scope[new V("shard")].Call(v_ctx_162, (v_r_163 * ((new V(0.86d) + (new V(0.12d) * ((v_i_181 % new V(2d))))))), ((VisualJs.MathObject[new V("sin")].Call(v_a_182) * v_r_163) * new V(0.55d)), (v_r_163 * new V(0.12d)), v_c_164, v_a_182, new V(0.72d));
}
} else if (new V(v_branch_166 == new V("chain_leech")).Truth) {
foreach (V v_y_183 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, (v_r_163 * new V(0.15d)), ((v_y_183 * v_r_163) * new V(0.45d)), (v_r_163 * new V(0.62d)), (v_r_163 * new V(0.16d)), v_c_164, (new V(v_y_183 > new V(0d)).Truth ? new V(0.7d) : (-new V(0.7d))), new V(0.76d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * new V(1.12d)), v_c_164, new V(1d), (-new V(1d)), new V(1d), new V(0.58d));
} else if (v_branch_166[new V("startsWith")].Call(new V("aura_")).Truth) {
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * ((new V(1.18d) + (v_final_167 * new V(0.08d))))), v_c_164, new V(1.5d), ((-VisualJs.MathObject[new V("PI")]) * new V(0.82d)), (VisualJs.MathObject[new V("PI")] * new V(0.82d)), new V(0.7d));
foreach (V v_y_184 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("tether")].Call(v_ctx_162, new V(0d), ((v_y_184 * v_r_163) * new V(0.2d)), (v_r_163 * new V(0.96d)), ((v_y_184 * v_r_163) * new V(0.66d)), (v_r_163 * new V(0.16d)), v_c_164, new V(1d), new V(0.48d), (v_y_184 * new V(0.7d)));
} else if (v_branch_166[new V("startsWith")].Call(new V("orbital_")).Truth) {
V v_n_185 = (new V(v_final_167 == new V(2d)).Truth ? new V(5d) : new V(4d));
for (V v_i_186 = new V(0d); new V(v_i_186 < v_n_185).Truth; v_i_186++) {
V v_a_187 = ((v_t_165 * ((new V(v_branch_166 == new V("orbital_dance")).Truth ? new V(1.4d) : new V(0.55d)))) + ((v_i_186 * Scope[new V("TAU")]) / v_n_185));
Scope[new V("shard")].Call(v_ctx_162, ((VisualJs.MathObject[new V("cos")].Call(v_a_187) * v_r_163) * new V(1.18d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_187) * v_r_163) * new V(0.72d)), (v_r_163 * new V(0.13d)), v_c_164, v_a_187, new V(0.74d));
}
} else if (v_branch_166[new V("startsWith")].Call(new V("prism_")).Truth) {
V v_n_188 = (new V(v_branch_166 == new V("prism_spectrum")).Truth ? new V(5d) : (new V(v_branch_166 == new V("prism_split")).Truth ? new V(3d) : new V(2d)));
for (V v_i_189 = new V(0d); new V(v_i_189 < v_n_188).Truth; v_i_189++) Scope[new V("shard")].Call(v_ctx_162, (v_r_163 * new V(1.12d)), ((((v_i_189 - (((v_n_188 - new V(1d))) / new V(2d)))) * v_r_163) * new V(0.28d)), (v_r_163 * new V(0.12d)), v_c_164, new V(0d), new V(0.74d));
} else if (v_branch_166[new V("startsWith")].Call(new V("gravity_")).Truth) {
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * ((new V(1.12d) + (v_final_167 * new V(0.1d))))), v_c_164, new V(1.25d), new V(0d), Scope[new V("TAU")], new V(0.64d));
for (V v_i_190 = new V(0d); new V(v_i_190 < new V(3d)).Truth; v_i_190++) Scope[new V("tether")].Call(v_ctx_162, (v_r_163 * new V(0.18d)), new V(0d), (v_r_163 * ((new V(0.72d) + (v_i_190 * new V(0.18d))))), new V(0d), (v_r_163 * new V(0.1d)), v_c_164, new V(1d), new V(0.46d), (v_t_165 + v_i_190));
} else if (v_branch_166[new V("startsWith")].Call(new V("pulse_")).Truth) {
V v_lobes_191 = (new V(v_branch_166 == new V("pulse_resonator")).Truth ? new V(8d) : (new V(v_branch_166 == new V("pulse_wave")).Truth ? new V(6d) : new V(5d)));
VisualJs.Assign(v_ctx_162,new V("strokeStyle"),"=",v_c_164);
VisualJs.Assign(v_ctx_162,new V("lineWidth"),"=",new V(1.25d));
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",new V(0.62d));
v_ctx_162[new V("beginPath")].Call();
for (V v_i_192 = new V(0d); new V(v_i_192 <= new V(36d)).Truth; v_i_192++) {
V v_a_193 = ((v_i_192 / new V(36d)) * Scope[new V("TAU")]);
V v_rr_194 = (v_r_163 * ((new V(1.02d) + (new V(0.12d) * VisualJs.MathObject[new V("sin")].Call(((v_a_193 * v_lobes_191) + (v_t_165 * new V(2.4d))))))));
V v_x_195 = (VisualJs.MathObject[new V("cos")].Call(v_a_193) * v_rr_194);
V v_y_196 = ((VisualJs.MathObject[new V("sin")].Call(v_a_193) * v_rr_194) * new V(0.54d));
if (new V(v_i_192 == new V(0d)).Truth) v_ctx_162[new V("moveTo")].Call(v_x_195, v_y_196); else v_ctx_162[new V("lineTo")].Call(v_x_195, v_y_196);
}
v_ctx_162[new V("stroke")].Call();
} else if (v_branch_166[new V("startsWith")].Call(new V("void_")).Truth) {
foreach (V v_y_197 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) Scope[new V("fin")].Call(v_ctx_162, (v_r_163 * new V(0.36d)), ((v_y_197 * v_r_163) * new V(0.24d)), (v_r_163 * new V(0.72d)), (v_r_163 * new V(0.18d)), v_c_164, (new V(v_y_197 > new V(0d)).Truth ? new V(0.22d) : (-new V(0.22d))), new V(0.78d));
Scope[new V("arc")].Call(v_ctx_162, (v_r_163 * ((new V(1.15d) + (v_final_167 * new V(0.08d))))), v_c_164, new V(1.2d), new V(0.15d), (Scope[new V("TAU")] - new V(0.15d)), new V(0.64d));
}
if (new V(v_final_167 >= new V(0d)).Truth) {
V v_fs_198 = (new V(1d) + (v_final_167 * new V(0.08d)));
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",new V(0.42d));
VisualJs.Assign(v_ctx_162,new V("strokeStyle"),"=",v_c_164);
VisualJs.Assign(v_ctx_162,new V("lineWidth"),"=",new V(1d));
v_ctx_162[new V("beginPath")].Call();
v_ctx_162[new V("ellipse")].Call((v_r_163 * new V(0.92d)), new V(0d), ((v_r_163 * new V(0.3d)) * v_fs_198), ((v_r_163 * new V(0.62d)) * v_fs_198), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_162[new V("stroke")].Call();
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",new V(0.9d));
V v_a_199 = (v_t_165 * ((new V(0.35d) + (v_final_167 * new V(0.08d)))));
Scope[new V("shard")].Call(v_ctx_162, ((v_r_163 * new V(1.2d)) * VisualJs.MathObject[new V("cos")].Call(v_a_199)), ((v_r_163 * new V(0.72d)) * VisualJs.MathObject[new V("sin")].Call(v_a_199)), (v_r_163 * new V(0.11d)), v_c_164, v_a_199, new V(0.76d));
}
VisualJs.Assign(v_ctx_162,new V("globalAlpha"),"=",v_pulse_168);
v_ctx_162[new V("restore")].Call();
return default;
}
private static V F_emitter(V[] args) {
V v_ctx_200 = VisualJs.Arg(args,0);
V v_r_201 = VisualJs.Arg(args,1);
V v_c_202 = VisualJs.Arg(args,2);
V v_t_203 = VisualJs.Arg(args,3);
V v_branch_204 = VisualJs.Arg(args,4);
V v_final_205 = VisualJs.Arg(args,5);
if ((!Visual_gameData.Scope[new V("sphereUsesProjectileModifiers")].Call((v_branch_204[new V("split")].Call(new V("_"))[new V(0d)]))).Truth) return default;
v_ctx_200[new V("save")].Call();
v_ctx_200[new V("translate")].Call((v_r_201 * new V(1.03d)), new V(0d));
VisualJs.Assign(v_ctx_200,new V("globalAlpha"),"=",new V(0.86d));
VisualJs.Assign(v_ctx_200,new V("strokeStyle"),"=",v_c_202);
VisualJs.Assign(v_ctx_200,new V("fillStyle"),"=",new V("rgba(5,11,19,.96)"));
VisualJs.Assign(v_ctx_200,new V("lineWidth"),"=",new V(1.05d));
V v_rgb_206 = Scope[new V("rgbOf")].Call(v_c_202);
if (v_branch_204[new V("startsWith")].Call(new V("standard_")).Truth) {
V v_n_207 = (v_final_205 + new V(2d));
for (V v_i_208 = new V(0d); new V(v_i_208 < v_n_207).Truth; v_i_208++) {
V v_y_209 = ((((v_i_208 - (((v_n_207 - new V(1d))) / new V(2d)))) * v_r_201) * new V(0.15d));
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call(((-v_r_201) * new V(0.15d)), v_y_209);
v_ctx_200[new V("lineTo")].Call((v_r_201 * new V(0.34d)), (v_y_209 * new V(0.82d)));
v_ctx_200[new V("stroke")].Call();
}
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("arc")].Call((v_r_201 * new V(0.33d)), new V(0d), (v_r_201 * ((new V(0.07d) + (new V(0.02d) * v_final_205)))), new V(0d), Scope[new V("TAU")]);
v_ctx_200[new V("fill")].Call();
v_ctx_200[new V("stroke")].Call();
} else if (v_branch_204[new V("startsWith")].Call(new V("sniper_")).Truth) {
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call(((-v_r_201) * new V(0.14d)), new V(0d));
v_ctx_200[new V("lineTo")].Call((v_r_201 * new V(0.4d)), new V(0d));
v_ctx_200[new V("stroke")].Call();
if (new V(v_branch_204 == new V("sniper_oracle")).Truth) Scope[new V("arc")].Call(v_ctx_200, (v_r_201 * new V(0.2d)), v_c_202, new V(1d), (-new V(0.9d)), new V(0.9d), new V(0.8d)); else if (new V(v_branch_204 == new V("sniper_assassin")).Truth) Scope[new V("fin")].Call(v_ctx_200, (v_r_201 * new V(0.12d)), new V(0d), (v_r_201 * ((new V(0.46d) + (new V(0.08d) * v_final_205)))), (v_r_201 * new V(0.1d)), v_c_202, new V(0d), new V(0.88d)); else {
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call((v_r_201 * new V(0.06d)), ((-v_r_201) * new V(0.16d)));
v_ctx_200[new V("lineTo")].Call((v_r_201 * new V(0.06d)), (v_r_201 * new V(0.16d)));
v_ctx_200[new V("stroke")].Call();
}
} else if (v_branch_204[new V("startsWith")].Call(new V("shotgun_")).Truth) {
V v_n_210 = (new V(v_branch_204 == new V("shotgun_cataclysm")).Truth ? new V(1d) : (new V(v_branch_204 == new V("shotgun_hail")).Truth ? new V(4d) : new V(3d)));
for (V v_i_211 = new V(0d); new V(v_i_211 < v_n_210).Truth; v_i_211++) {
V v_y_212 = ((((v_i_211 - (((v_n_210 - new V(1d))) / new V(2d)))) * v_r_201) * new V(0.14d));
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call(((-v_r_201) * new V(0.16d)), v_y_212);
v_ctx_200[new V("lineTo")].Call((v_r_201 * new V(0.38d)), v_y_212);
v_ctx_200[new V("stroke")].Call();
}
if (new V(v_branch_204 == new V("shotgun_cataclysm")).Truth) {
VisualJs.Assign(v_ctx_200,new V("fillStyle"),"=",new V("rgba(255,169,77,.78)"));
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("arc")].Call((v_r_201 * new V(0.35d)), new V(0d), (v_r_201 * new V(0.09d)), new V(0d), Scope[new V("TAU")]);
v_ctx_200[new V("fill")].Call();
}
} else if (v_branch_204[new V("startsWith")].Call(new V("prism_")).Truth) {
V v_n_213 = (new V(v_branch_204 == new V("prism_split")).Truth ? new V(3d) : (new V(v_branch_204 == new V("prism_spectrum")).Truth ? new V(5d) : new V(2d)));
for (V v_i_214 = new V(0d); new V(v_i_214 < v_n_213).Truth; v_i_214++) {
V v_y_215 = ((((v_i_214 - (((v_n_213 - new V(1d))) / new V(2d)))) * v_r_201) * new V(0.13d));
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call(((-v_r_201) * new V(0.12d)), new V(0d));
v_ctx_200[new V("quadraticCurveTo")].Call((v_r_201 * new V(0.15d)), (v_y_215 * new V(0.2d)), (v_r_201 * new V(0.38d)), v_y_215);
v_ctx_200[new V("stroke")].Call();
}
Scope[new V("shard")].Call(v_ctx_200, (v_r_201 * new V(0.37d)), new V(0d), (v_r_201 * ((new V(0.12d) + (new V(0.02d) * v_final_205)))), v_c_202, new V(0d), new V(0.82d));
} else if (v_branch_204[new V("startsWith")].Call(new V("void_")).Truth) {
if (new V(v_branch_204 == new V("void_hunger")).Truth) {
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("ellipse")].Call((v_r_201 * new V(0.25d)), new V(0d), (v_r_201 * new V(0.16d)), (v_r_201 * new V(0.11d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_200[new V("stroke")].Call();
} else if (new V(v_branch_204 == new V("void_reaper")).Truth) Scope[new V("fin")].Call(v_ctx_200, (v_r_201 * new V(0.2d)), new V(0d), (v_r_201 * ((new V(0.48d) + (new V(0.08d) * v_final_205)))), (v_r_201 * new V(0.13d)), v_c_202, new V(0d), new V(0.9d)); else {
v_ctx_200[new V("beginPath")].Call();
v_ctx_200[new V("moveTo")].Call((v_r_201 * new V(0.02d)), ((-v_r_201) * new V(0.18d)));
v_ctx_200[new V("lineTo")].Call((v_r_201 * ((new V(0.42d) + (new V(0.08d) * v_final_205)))), new V(0d));
v_ctx_200[new V("lineTo")].Call((v_r_201 * new V(0.02d)), (v_r_201 * new V(0.18d)));
v_ctx_200[new V("stroke")].Call();
}
}
v_ctx_200[new V("restore")].Call();
return default;
}
private static V F_projectileCore(V[] args) {
V v_ctx_216 = VisualJs.Arg(args,0);
V v_p_217 = VisualJs.Arg(args,1);
V v_player_218 = VisualJs.Arg(args,2);
V v_t_219 = (VisualJs.Chain(v_p_217[new V("sourceSphere")],optional_220=>optional_220[new V("type")]));
V v_b_221 = (v_t_219.Truth ? Scope[new V("branchOf")].Call(v_player_218, v_t_219) : VisualJs.Null);
V v_f_222 = (v_t_219.Truth ? Scope[new V("finalOf")].Call(v_player_218, v_t_219) : (-new V(1d)));
V v_r_223 = VisualJs.MathObject[new V("max")].Call(new V(2d), v_p_217[new V("radius")]);
V v_c_224 = v_p_217[new V("color")];
v_ctx_216[new V("save")].Call();
VisualJs.Assign(v_ctx_216,new V("globalCompositeOperation"),"=",new V("lighter"));
if (new V(v_t_219 == new V("standard")).Truth) {
VisualJs.Assign(v_ctx_216,new V("strokeStyle"),"=",v_c_224);
VisualJs.Assign(v_ctx_216,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_223 * new V(0.17d))));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call(((-v_r_223) * new V(0.9d)), new V(0d));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(1.35d)), new V(0d));
v_ctx_216[new V("stroke")].Call();
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("arc")].Call((v_r_223 * new V(0.22d)), new V(0d), (v_r_223 * new V(0.3d)), new V(0d), Scope[new V("TAU")]);
v_ctx_216[new V("stroke")].Call();
if (new V(v_b_221 == new V("standard_singularity")).Truth) {
VisualJs.Assign(v_ctx_216,new V("fillStyle"),"=",new V("#05020b"));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("arc")].Call(new V(0d), new V(0d), (v_r_223 * new V(0.62d)), new V(0d), Scope[new V("TAU")]);
v_ctx_216[new V("fill")].Call();
}
if (new V(v_b_221 == new V("standard_swarm")).Truth) {
foreach (V v_q_225 in VisualJs.Arr((-new V(1d)), new V(0d), new V(1d)).Items) Scope[new V("shard")].Call(v_ctx_216, ((v_q_225 * v_r_223) * new V(0.42d)), new V(0d), (v_r_223 * new V(0.25d)), v_c_224, (v_q_225 * new V(0.2d)), new V(0.8d));
}
} else if (new V(v_t_219 == new V("sniper")).Truth) {
VisualJs.Assign(v_ctx_216,new V("strokeStyle"),"=",v_c_224);
VisualJs.Assign(v_ctx_216,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_223 * new V(0.14d))));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call(((-v_r_223) * new V(0.75d)), new V(0d));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(1.35d)), new V(0d));
v_ctx_216[new V("stroke")].Call();
if (new V(v_b_221 == new V("sniper_oracle")).Truth) {
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("arc")].Call((v_r_223 * new V(0.35d)), new V(0d), (v_r_223 * new V(0.5d)), new V(0d), Scope[new V("TAU")]);
v_ctx_216[new V("stroke")].Call();
} else if (new V(v_b_221 == new V("sniper_assassin")).Truth) Scope[new V("fin")].Call(v_ctx_216, (v_r_223 * new V(0.18d)), new V(0d), (v_r_223 * ((new V(0.85d) + (new V(0.15d) * v_f_222)))), (v_r_223 * new V(0.18d)), v_c_224, new V(0d), new V(0.9d)); else {
Scope[new V("arc")].Call(v_ctx_216, (v_r_223 * new V(1d)), v_c_224, new V(0.8d), (-new V(0.8d)), new V(0.8d), new V(0.9d));
}
} else if (new V(v_t_219 == new V("shotgun")).Truth) {
V v_n_226 = (new V(v_b_221 == new V("shotgun_cataclysm")).Truth ? new V(1d) : (new V(v_b_221 == new V("shotgun_hail")).Truth ? new V(4d) : new V(3d)));
for (V v_i_227 = new V(0d); new V(v_i_227 < v_n_226).Truth; v_i_227++) {
V v_y_228 = ((((v_i_227 - (((v_n_226 - new V(1d))) / new V(2d)))) * v_r_223) * new V(0.34d));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call(((-v_r_223) * new V(0.7d)), v_y_228);
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(0.92d)), v_y_228);
v_ctx_216[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_216,new V("fillStyle"),"=",new V("#fff"));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("arc")].Call((v_r_223 * new V(0.58d)), new V(0d), (v_r_223 * new V(0.18d)), new V(0d), Scope[new V("TAU")]);
v_ctx_216[new V("fill")].Call();
} else if (new V(v_t_219 == new V("prism")).Truth) {
V v_n_229 = (new V(v_b_221 == new V("prism_split")).Truth ? new V(3d) : (new V(v_b_221 == new V("prism_spectrum")).Truth ? new V(5d) : new V(2d)));
for (V v_i_230 = new V(0d); new V(v_i_230 < v_n_229).Truth; v_i_230++) {
V v_y_231 = ((((v_i_230 - (((v_n_229 - new V(1d))) / new V(2d)))) * v_r_223) * new V(0.38d));
Scope[new V("shard")].Call(v_ctx_216, new V(0d), v_y_231, (v_r_223 * new V(0.34d)), v_c_224, new V(0d), new V(0.75d));
}
VisualJs.Assign(v_ctx_216,new V("strokeStyle"),"=",new V("#ffffff"));
VisualJs.Assign(v_ctx_216,new V("lineWidth"),"=",new V(0.7d));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call(((-v_r_223) * new V(0.28d)), new V(0d));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(1.04d)), new V(0d));
v_ctx_216[new V("stroke")].Call();
} else if (new V(v_t_219 == new V("void")).Truth) {
if (new V(v_b_221 == new V("void_hunger")).Truth) {
VisualJs.Assign(v_ctx_216,new V("fillStyle"),"=",new V("#08030f"));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("arc")].Call(new V(0d), new V(0d), (v_r_223 * new V(0.68d)), new V(0d), Scope[new V("TAU")]);
v_ctx_216[new V("fill")].Call();
Scope[new V("arc")].Call(v_ctx_216, (v_r_223 * new V(0.42d)), v_c_224, new V(0.9d), new V(0.2d), new V(5.4d), new V(0.9d));
} else if (new V(v_b_221 == new V("void_reaper")).Truth) Scope[new V("fin")].Call(v_ctx_216, new V(0d), new V(0d), (v_r_223 * new V(0.95d)), (v_r_223 * new V(0.24d)), v_c_224, new V(0d), new V(0.9d)); else {
VisualJs.Assign(v_ctx_216,new V("strokeStyle"),"=",v_c_224);
VisualJs.Assign(v_ctx_216,new V("lineWidth"),"=",new V(1d));
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call(((-v_r_223) * new V(0.44d)), new V(0d));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(0.9d)), new V(0d));
v_ctx_216[new V("stroke")].Call();
v_ctx_216[new V("beginPath")].Call();
v_ctx_216[new V("moveTo")].Call((v_r_223 * new V(0.55d)), ((-v_r_223) * new V(0.25d)));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(0.9d)), new V(0d));
v_ctx_216[new V("lineTo")].Call((v_r_223 * new V(0.55d)), (v_r_223 * new V(0.25d)));
v_ctx_216[new V("stroke")].Call();
}
}
v_ctx_216[new V("restore")].Call();
return default;
}
private static V F_projectileTail(V[] args) {
V v_ctx_232 = VisualJs.Arg(args,0);
V v_r_233 = VisualJs.Arg(args,1);
V v_c_234 = VisualJs.Arg(args,2);
V v_speed_235 = VisualJs.Arg(args,3);
V v_t_236 = VisualJs.Arg(args,4);
V v_len_237 = VisualJs.MathObject[new V("min")].Call(new V(58d), (new V(14d) + (v_speed_235 * new V(0.045d))));
V v_rgb_238 = Scope[new V("rgbOf")].Call(v_c_234);
VisualJs.Assign(v_ctx_232,new V("globalAlpha"),"=",new V(0.17d));
VisualJs.Assign(v_ctx_232,new V("fillStyle"),"=",v_c_234);
v_ctx_232[new V("beginPath")].Call();
v_ctx_232[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_232[new V("quadraticCurveTo")].Call(((-v_len_237) * new V(0.35d)), ((-v_r_233) * new V(0.82d)), ((-v_len_237) * new V(0.86d)), ((-v_r_233) * new V(0.12d)));
v_ctx_232[new V("quadraticCurveTo")].Call((-v_len_237), new V(0d), ((-v_len_237) * new V(0.86d)), (v_r_233 * new V(0.12d)));
v_ctx_232[new V("quadraticCurveTo")].Call(((-v_len_237) * new V(0.35d)), (v_r_233 * new V(0.82d)), new V(0d), new V(0d));
v_ctx_232[new V("fill")].Call();
VisualJs.Assign(v_ctx_232,new V("globalAlpha"),"=",new V(0.1d));
VisualJs.Assign(v_ctx_232,new V("strokeStyle"),"=",((new V("rgba(") + v_rgb_238) + new V(",.80)")));
VisualJs.Assign(v_ctx_232,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_233 * new V(0.65d))));
v_ctx_232[new V("beginPath")].Call();
v_ctx_232[new V("moveTo")].Call(((-v_len_237) * new V(0.88d)), new V(0d));
v_ctx_232[new V("lineTo")].Call(((-v_r_233) * new V(0.1d)), new V(0d));
v_ctx_232[new V("stroke")].Call();
return default;
}
private static V F_renderSphereMutationVfx(V[] args) {
V v_ctx_239 = VisualJs.Arg(args,0);
V v_sphere_240 = VisualJs.Arg(args,1);
V v_player_241 = VisualJs.Arg(args,2);
V v_time_242 = VisualJs.Arg(args,3);
V v_scale_243 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_branch_244 = Scope[new V("branchOf")].Call(v_player_241, v_sphere_240[new V("type")]);
if ((!v_branch_244).Truth) return default;
V v_final_245 = Scope[new V("finalOf")].Call(v_player_241, v_sphere_240[new V("type")]);
V v_r_246 = (new V(24d) * v_scale_243);
V v_c_247 = Visual_gameData.Scope[new V("SPHERE_TYPES")][v_sphere_240[new V("type")]][new V("color")];
V v_directedWeapon_248 = Visual_gameData.Scope[new V("sphereUsesProjectileModifiers")].Call(v_sphere_240[new V("type")]);
v_ctx_239[new V("save")].Call();
v_ctx_239[new V("translate")].Call(v_sphere_240[new V("pos")][new V("x")], v_sphere_240[new V("pos")][new V("y")]);
if (v_directedWeapon_248.Truth) v_ctx_239[new V("rotate")].Call(VisualJs.Or(v_sphere_240[new V("rotation")], () => new V(0d)));
Scope[new V("glow")].Call(v_ctx_239, (v_r_246 * new V(1.75d)), v_c_247, new V(0.055d));
Scope[new V("mutationSilhouette")].Call(v_ctx_239, v_r_246, v_c_247, v_time_242, v_branch_244, v_final_245);
Scope[new V("drawFinalExpansion")].Call(v_ctx_239, v_r_246, v_c_247, v_time_242, v_branch_244, v_final_245, v_sphere_240);
Scope[new V("emitter")].Call(v_ctx_239, v_r_246, v_c_247, v_time_242, v_branch_244, v_final_245);
Scope[new V("emit")].Call(v_ctx_239, v_r_246, v_c_247, v_time_242, v_branch_244, v_final_245, v_sphere_240);
v_ctx_239[new V("restore")].Call();
return default;
}
private static V F_renderSphereProjectileVfx(V[] args) {
V v_ctx_249 = VisualJs.Arg(args,0);
V v_p_250 = VisualJs.Arg(args,1);
V v_player_251 = VisualJs.Arg(args,2);
V v_time_252 = VisualJs.Arg(args,3);
V v_a_253 = VisualJs.MathObject[new V("atan2")].Call(v_p_250[new V("vel")][new V("y")], v_p_250[new V("vel")][new V("x")]);
V v_speed_254 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_p_250[new V("vel")][new V("x")], v_p_250[new V("vel")][new V("y")]), () => new V(1d));
V v_r_255 = VisualJs.MathObject[new V("max")].Call(new V(2d), v_p_250[new V("radius")]);
V v_rgb_256 = Scope[new V("rgbOf")].Call(v_p_250[new V("color")]);
v_ctx_249[new V("save")].Call();
v_ctx_249[new V("translate")].Call(v_p_250[new V("pos")][new V("x")], v_p_250[new V("pos")][new V("y")]);
v_ctx_249[new V("rotate")].Call(v_a_253);
Scope[new V("projectileTail")].Call(v_ctx_249, v_r_255, v_p_250[new V("color")], v_speed_254, v_time_252);
VisualJs.Assign(v_ctx_249,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_249,new V("globalAlpha"),"=",new V(0.18d));
VisualJs.Assign(v_ctx_249,new V("fillStyle"),"=",((new V("rgba(") + v_rgb_256) + new V(",1)")));
v_ctx_249[new V("beginPath")].Call();
v_ctx_249[new V("arc")].Call(new V(0d), new V(0d), (v_r_255 * new V(1.65d)), new V(0d), Scope[new V("TAU")]);
v_ctx_249[new V("fill")].Call();
VisualJs.Assign(v_ctx_249,new V("globalAlpha"),"=",new V(1d));
Scope[new V("projectileCore")].Call(v_ctx_249, v_p_250, v_player_251);
v_ctx_249[new V("restore")].Call();
return default;
}
private static V F_renderChainLightningVfx(V[] args) {
V v_ctx_257 = VisualJs.Arg(args,0);
V v_bolt_258 = VisualJs.Arg(args,1);
V v_player_259 = VisualJs.Arg(args,2);
V v_alpha_260 = VisualJs.Arg(args,3);
V v_time_261 = VisualJs.Arg(args,4);
V v_sphere_262 = v_bolt_258[new V("sourceSphere")];
if (VisualJs.Or((!v_sphere_262), () => new V(v_sphere_262[new V("type")] != new V("chain"))).Truth) return new V(false);
V v_b_263 = Scope[new V("branchOf")].Call(v_player_259, new V("chain"));
V v_f_264 = Scope[new V("finalOf")].Call(v_player_259, new V("chain"));
if ((!v_b_263).Truth) return new V(false);
V v_c_265 = Visual_gameData.Scope[new V("SPHERE_TYPES")][new V("chain")][new V("color")];
V v_rgb_266 = Scope[new V("rgbOf")].Call(v_c_265);
V v_dx_267 = (v_bolt_258[new V("to")][new V("x")] - v_bolt_258[new V("from")][new V("x")]);
V v_dy_268 = (v_bolt_258[new V("to")][new V("y")] - v_bolt_258[new V("from")][new V("y")]);
V v_len_269 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_dx_267, v_dy_268), () => new V(1d));
V v_nx_270 = ((-v_dy_268) / v_len_269);
V v_ny_271 = (v_dx_267 / v_len_269);
V v_pieces_272 = (new V(v_b_263 == new V("chain_web")).Truth ? new V(10d) : (new V(v_b_263 == new V("chain_storm")).Truth ? new V(8d) : new V(12d)));
v_ctx_257[new V("save")].Call();
VisualJs.Assign(v_ctx_257,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_257,new V("lineCap"),"=",new V("round"));
for (V v_pass_273 = new V(0d); new V(v_pass_273 < new V(2d)).Truth; v_pass_273++) {
VisualJs.Assign(v_ctx_257,new V("strokeStyle"),"=",(new V(v_pass_273 == new V(0d)).Truth ? ((((new V("rgba(") + v_rgb_266) + new V(",")) + ((new V(0.18d) * v_alpha_260))) + new V(")")) : ((new V("rgba(245,255,255,") + ((new V(0.88d) * v_alpha_260))) + new V(")"))));
VisualJs.Assign(v_ctx_257,new V("lineWidth"),"=",(new V(v_pass_273 == new V(0d)).Truth ? VisualJs.MathObject[new V("max")].Call(new V(4d), (v_len_269 * new V(0.015d))) : VisualJs.MathObject[new V("max")].Call(new V(1.2d), (v_len_269 * new V(0.0045d)))));
v_ctx_257[new V("beginPath")].Call();
v_ctx_257[new V("moveTo")].Call(v_bolt_258[new V("from")][new V("x")], v_bolt_258[new V("from")][new V("y")]);
for (V v_i_274 = new V(1d); new V(v_i_274 < v_pieces_272).Truth; v_i_274++) {
V v_q_275 = (v_i_274 / v_pieces_272);
V v_amp_276 = (v_len_269 * ((new V(v_b_263 == new V("chain_storm")).Truth ? new V(0.12d) : (new V(v_b_263 == new V("chain_leech")).Truth ? new V(0.06d) : new V(0.09d)))));
V v_zig_277 = (((((v_i_274 % new V(2d)).Truth ? (-new V(1d)) : new V(1d))) * v_amp_276) * ((new V(0.7d) + (new V(0.3d) * VisualJs.MathObject[new V("sin")].Call(((v_time_261 * new V(22d)) + v_i_274))))));
v_ctx_257[new V("lineTo")].Call(((v_bolt_258[new V("from")][new V("x")] + (v_dx_267 * v_q_275)) + (v_nx_270 * v_zig_277)), ((v_bolt_258[new V("from")][new V("y")] + (v_dy_268 * v_q_275)) + (v_ny_271 * v_zig_277)));
}
v_ctx_257[new V("lineTo")].Call(v_bolt_258[new V("to")][new V("x")], v_bolt_258[new V("to")][new V("y")]);
v_ctx_257[new V("stroke")].Call();
}
if (new V(v_b_263 == new V("chain_web")).Truth) {
VisualJs.Assign(v_ctx_257,new V("strokeStyle"),"=",((new V("rgba(255,244,150,") + ((new V(0.68d) * v_alpha_260))) + new V(")")));
VisualJs.Assign(v_ctx_257,new V("lineWidth"),"=",new V(1d));
for (V v_i_278 = new V(0d); new V(v_i_278 < new V(4d)).Truth; v_i_278++) {
V v_a_279 = ((v_time_261 * new V(2d)) + ((v_i_278 * Scope[new V("TAU")]) / new V(4d)));
v_ctx_257[new V("beginPath")].Call();
v_ctx_257[new V("moveTo")].Call(v_bolt_258[new V("to")][new V("x")], v_bolt_258[new V("to")][new V("y")]);
v_ctx_257[new V("lineTo")].Call((v_bolt_258[new V("to")][new V("x")] + (VisualJs.MathObject[new V("cos")].Call(v_a_279) * ((new V(8d) + (v_f_264 * new V(2d)))))), (v_bolt_258[new V("to")][new V("y")] + (VisualJs.MathObject[new V("sin")].Call(v_a_279) * ((new V(6d) + v_f_264)))));
v_ctx_257[new V("stroke")].Call();
}
} else if (new V(v_b_263 == new V("chain_storm")).Truth) {
VisualJs.Assign(v_ctx_257,new V("fillStyle"),"=",((new V("rgba(255,249,190,") + ((new V(0.9d) * v_alpha_260))) + new V(")")));
v_ctx_257[new V("beginPath")].Call();
v_ctx_257[new V("arc")].Call(v_bolt_258[new V("to")][new V("x")], v_bolt_258[new V("to")][new V("y")], (new V(3d) + (v_f_264 * new V(0.8d))), new V(0d), Scope[new V("TAU")]);
v_ctx_257[new V("fill")].Call();
} else {
VisualJs.Assign(v_ctx_257,new V("strokeStyle"),"=",((new V("rgba(255,255,255,") + ((new V(0.7d) * v_alpha_260))) + new V(")")));
VisualJs.Assign(v_ctx_257,new V("lineWidth"),"=",new V(1d));
v_ctx_257[new V("beginPath")].Call();
v_ctx_257[new V("arc")].Call(v_bolt_258[new V("to")][new V("x")], v_bolt_258[new V("to")][new V("y")], (new V(3d) + (VisualJs.MathObject[new V("sin")].Call((v_time_261 * new V(16d))) * new V(0.8d))), new V(0d), Scope[new V("TAU")]);
v_ctx_257[new V("stroke")].Call();
}
v_ctx_257[new V("restore")].Call();
return new V(true);
}
}
}
