// Generated from src/enemies/enemyVisual.ts; sha256 237da35ae87e4bc71f640107e1b8fa2d5d3050828a12d2a85b0ceae8846c2288.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_enemyVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_enemyVisual() {
Scope[new V("rgbOf")] = VisualJs.Fn(F_rgbOf);
Scope[new V("glow")] = VisualJs.Fn(F_glow);
Scope[new V("joint")] = VisualJs.Fn(F_joint);
Scope[new V("seg")] = VisualJs.Fn(F_seg);
Scope[new V("segmentBetween")] = VisualJs.Fn(F_segmentBetween);
Scope[new V("articulatedLeg")] = VisualJs.Fn(F_articulatedLeg);
Scope[new V("pairedLegs")] = VisualJs.Fn(F_pairedLegs);
Scope[new V("tail")] = VisualJs.Fn(F_tail);
Scope[new V("eye")] = VisualJs.Fn(F_eye);
Scope[new V("core")] = VisualJs.Fn(F_core);
Scope[new V("body")] = VisualJs.Fn(F_body);
Scope[new V("wing")] = VisualJs.Fn(F_wing);
Scope[new V("drawVeilRipper")] = VisualJs.Fn(F_drawVeilRipper);
Scope[new V("drawGraveLeech")] = VisualJs.Fn(F_drawGraveLeech);
Scope[new V("drawFangedCoil")] = VisualJs.Fn(F_drawFangedCoil);
Scope[new V("drawCarrionSkitter")] = VisualJs.Fn(F_drawCarrionSkitter);
Scope[new V("drawUmbralMoth")] = VisualJs.Fn(F_drawUmbralMoth);
Scope[new V("drawRiftScarab")] = VisualJs.Fn(F_drawRiftScarab);
Scope[new V("drawBonebackBrute")] = VisualJs.Fn(F_drawBonebackBrute);
Scope[new V("drawGlassHound")] = VisualJs.Fn(F_drawGlassHound);
Scope[new V("drawHollowStalker")] = VisualJs.Fn(F_drawHollowStalker);
Scope[new V("drawCableWidow")] = VisualJs.Fn(F_drawCableWidow);
Scope[new V("bossFrame")] = VisualJs.Fn(F_bossFrame);
Scope[new V("drawVoidLancerBoss")] = VisualJs.Fn(F_drawVoidLancerBoss);
Scope[new V("drawDreadChargerBoss")] = VisualJs.Fn(F_drawDreadChargerBoss);
Scope[new V("drawBroodMatriarchBoss")] = VisualJs.Fn(F_drawBroodMatriarchBoss);
Scope[new V("drawAbyssalLeviathanBoss")] = VisualJs.Fn(F_drawAbyssalLeviathanBoss);
Scope[new V("creature")] = VisualJs.Fn(F_creature);
Scope[new V("boss")] = VisualJs.Fn(F_boss);
Scope[new V("drawEnemyCreature")] = VisualJs.Fn(F_drawEnemyCreature);
Scope[new V("drawBossCreature")] = VisualJs.Fn(F_drawBossCreature);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
}
private static V F_rgbOf(V[] args) {
V v_hex_0 = VisualJs.Arg(args,0);
V v_n_1 = VisualJs.NumberObject[new V("parseInt")].Call(v_hex_0[new V("slice")].Call(new V(1d)), new V(16d));
return ((((((((v_n_1 >> (int)new V(16d).Number)) & new V(255d))) + new V(",")) + ((((v_n_1 >> (int)new V(8d).Number)) & new V(255d)))) + new V(",")) + ((v_n_1 & new V(255d))));
}
private static V F_glow(V[] args) {
V v_ctx_2 = VisualJs.Arg(args,0);
V v_r_3 = VisualJs.Arg(args,1);
V v_color_4 = VisualJs.Arg(args,2);
V v_a_5 = VisualJs.Arg(args,3).Undefined?new V(0.08d):VisualJs.Arg(args,3);
V v_g_6 = v_ctx_2[new V("createRadialGradient")].Call(new V(0d), new V(0d), new V(0d), new V(0d), new V(0d), v_r_3);
V v_c_7 = Scope[new V("rgbOf")].Call(v_color_4);
v_g_6[new V("addColorStop")].Call(new V(0d), ((((new V("rgba(") + v_c_7) + new V(",")) + ((v_a_5 * new V(2.8d)))) + new V(")")));
v_g_6[new V("addColorStop")].Call(new V(0.35d), ((((new V("rgba(") + v_c_7) + new V(",")) + ((v_a_5 * new V(0.9d)))) + new V(")")));
v_g_6[new V("addColorStop")].Call(new V(1d), ((new V("rgba(") + v_c_7) + new V(",0)")));
v_ctx_2[new V("save")].Call();
VisualJs.Assign(v_ctx_2,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_2,new V("fillStyle"),"=",v_g_6);
v_ctx_2[new V("beginPath")].Call();
v_ctx_2[new V("arc")].Call(new V(0d), new V(0d), v_r_3, new V(0d), Scope[new V("TAU")]);
v_ctx_2[new V("fill")].Call();
v_ctx_2[new V("restore")].Call();
return default;
}
private static V F_joint(V[] args) {
V v_ctx_8 = VisualJs.Arg(args,0);
V v_x_9 = VisualJs.Arg(args,1);
V v_y_10 = VisualJs.Arg(args,2);
V v_r_11 = VisualJs.Arg(args,3);
V v_color_12 = VisualJs.Arg(args,4);
v_ctx_8[new V("save")].Call();
VisualJs.Assign(v_ctx_8,new V("fillStyle"),"=",new V("rgba(4,8,14,.98)"));
VisualJs.Assign(v_ctx_8,new V("strokeStyle"),"=",v_color_12);
VisualJs.Assign(v_ctx_8,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_11 * new V(0.18d))));
v_ctx_8[new V("beginPath")].Call();
v_ctx_8[new V("arc")].Call(v_x_9, v_y_10, v_r_11, new V(0d), Scope[new V("TAU")]);
v_ctx_8[new V("fill")].Call();
v_ctx_8[new V("stroke")].Call();
v_ctx_8[new V("restore")].Call();
return default;
}
private static V F_seg(V[] args) {
V v_ctx_13 = VisualJs.Arg(args,0);
V v_x_14 = VisualJs.Arg(args,1);
V v_y_15 = VisualJs.Arg(args,2);
V v_len_16 = VisualJs.Arg(args,3);
V v_w_17 = VisualJs.Arg(args,4);
V v_ang_18 = VisualJs.Arg(args,5);
V v_color_19 = VisualJs.Arg(args,6);
V v_alpha_20 = VisualJs.Arg(args,7).Undefined?new V(0.9d):VisualJs.Arg(args,7);
v_ctx_13[new V("save")].Call();
v_ctx_13[new V("translate")].Call(v_x_14, v_y_15);
v_ctx_13[new V("rotate")].Call(v_ang_18);
VisualJs.Assign(v_ctx_13,new V("globalAlpha"),"=",v_alpha_20);
VisualJs.Assign(v_ctx_13,new V("fillStyle"),"=",new V("rgba(4,8,15,.98)"));
VisualJs.Assign(v_ctx_13,new V("strokeStyle"),"=",v_color_19);
VisualJs.Assign(v_ctx_13,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_w_17 * new V(0.09d))));
v_ctx_13[new V("beginPath")].Call();
v_ctx_13[new V("moveTo")].Call(new V(0d), ((-v_w_17) * new V(0.34d)));
v_ctx_13[new V("quadraticCurveTo")].Call((v_len_16 * new V(0.48d)), ((-v_w_17) * new V(0.48d)), v_len_16, ((-v_w_17) * new V(0.1d)));
v_ctx_13[new V("quadraticCurveTo")].Call((v_len_16 * new V(0.62d)), (v_w_17 * new V(0.42d)), new V(0d), (v_w_17 * new V(0.3d)));
v_ctx_13[new V("closePath")].Call();
v_ctx_13[new V("fill")].Call();
v_ctx_13[new V("stroke")].Call();
v_ctx_13[new V("restore")].Call();
return default;
}
private static V F_segmentBetween(V[] args) {
V v_ctx_21 = VisualJs.Arg(args,0);
V v_x1_22 = VisualJs.Arg(args,1);
V v_y1_23 = VisualJs.Arg(args,2);
V v_x2_24 = VisualJs.Arg(args,3);
V v_y2_25 = VisualJs.Arg(args,4);
V v_w_26 = VisualJs.Arg(args,5);
V v_color_27 = VisualJs.Arg(args,6);
V v_alpha_28 = VisualJs.Arg(args,7).Undefined?new V(0.9d):VisualJs.Arg(args,7);
V v_dx_29 = (v_x2_24 - v_x1_22);
V v_dy_30 = (v_y2_25 - v_y1_23);
Scope[new V("seg")].Call(v_ctx_21, v_x1_22, v_y1_23, VisualJs.MathObject[new V("hypot")].Call(v_dx_29, v_dy_30), v_w_26, VisualJs.MathObject[new V("atan2")].Call(v_dy_30, v_dx_29), v_color_27, v_alpha_28);
return default;
}
private static V F_articulatedLeg(V[] args) {
V v_ctx_31 = VisualJs.Arg(args,0);
V v_hipX_32 = VisualJs.Arg(args,1);
V v_hipY_33 = VisualJs.Arg(args,2);
V v_len_34 = VisualJs.Arg(args,3);
V v_side_35 = VisualJs.Arg(args,4);
V v_phase_36 = VisualJs.Arg(args,5);
V v_color_37 = VisualJs.Arg(args,6);
V v_thick_38 = VisualJs.Arg(args,7).Undefined?new V(1d):VisualJs.Arg(args,7);
V v_stride_39 = ((VisualJs.MathObject[new V("sin")].Call(v_phase_36) * v_len_34) * new V(0.3d));
V v_lift_40 = ((VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("cos")].Call(v_phase_36)) * v_len_34) * new V(0.12d));
V v_kneeX_41 = ((v_hipX_32 + (v_stride_39 * new V(0.46d))) + (v_len_34 * new V(0.05d)));
V v_kneeY_42 = (v_hipY_33 + (v_side_35 * (((v_len_34 * new V(0.34d)) - v_lift_40))));
V v_ankleX_43 = (v_hipX_32 + v_stride_39);
V v_ankleY_44 = (v_hipY_33 + (v_side_35 * (((v_len_34 * new V(0.76d)) - (v_lift_40 * new V(0.45d))))));
Scope[new V("segmentBetween")].Call(v_ctx_31, v_hipX_32, v_hipY_33, v_kneeX_41, v_kneeY_42, (v_thick_38 * new V(5.8d)), v_color_37, new V(0.94d));
Scope[new V("joint")].Call(v_ctx_31, v_kneeX_41, v_kneeY_42, (v_thick_38 * new V(3.5d)), v_color_37);
Scope[new V("segmentBetween")].Call(v_ctx_31, v_kneeX_41, v_kneeY_42, v_ankleX_43, v_ankleY_44, (v_thick_38 * new V(4d)), v_color_37, new V(0.9d));
Scope[new V("joint")].Call(v_ctx_31, v_ankleX_43, v_ankleY_44, (v_thick_38 * new V(1.9d)), v_color_37);
V v_footX_45 = (v_ankleX_43 + (v_len_34 * new V(0.15d)));
V v_footY_46 = (v_ankleY_44 + ((v_side_35 * v_len_34) * new V(0.035d)));
Scope[new V("segmentBetween")].Call(v_ctx_31, v_ankleX_43, v_ankleY_44, v_footX_45, v_footY_46, (v_thick_38 * new V(1.45d)), v_color_37, new V(0.92d));
return default;
}
private static V F_pairedLegs(V[] args) {
V v_ctx_47 = VisualJs.Arg(args,0);
V v_r_48 = VisualJs.Arg(args,1);
V v_xs_49 = VisualJs.Arg(args,2);
V v_bodySideY_50 = VisualJs.Arg(args,3);
V v_len_51 = VisualJs.Arg(args,4);
V v_phase_52 = VisualJs.Arg(args,5);
V v_color_53 = VisualJs.Arg(args,6);
V v_thick_54 = VisualJs.Arg(args,7).Undefined?new V(1d):VisualJs.Arg(args,7);
for (V v_i_55 = new V(0d); new V(v_i_55 < v_xs_49[new V("length")]).Truth; v_i_55++) {
V v_x_56 = (v_xs_49[v_i_55] * v_r_48);
foreach (V v_side_57 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
V v_sidePhase_58 = ((v_phase_52 + (v_i_55 * new V(1.05d))) + ((new V(v_side_57 > new V(0d)).Truth ? new V(0d) : VisualJs.MathObject[new V("PI")])));
Scope[new V("articulatedLeg")].Call(v_ctx_47, v_x_56, ((v_side_57 * v_bodySideY_50) * v_r_48), (v_len_51 * v_r_48), v_side_57, v_sidePhase_58, v_color_53, v_thick_54);
}
}
return default;
}
private static V F_tail(V[] args) {
V v_ctx_59 = VisualJs.Arg(args,0);
V v_sx_60 = VisualJs.Arg(args,1);
V v_sy_61 = VisualJs.Arg(args,2);
V v_len_62 = VisualJs.Arg(args,3);
V v_color_63 = VisualJs.Arg(args,4);
V v_t_64 = VisualJs.Arg(args,5);
V v_segments_65 = VisualJs.Arg(args,6).Undefined?new V(7d):VisualJs.Arg(args,6);
V v_amp_66 = VisualJs.Arg(args,7).Undefined?new V(0.18d):VisualJs.Arg(args,7);
V v_px_67 = v_sx_60;
V v_py_68 = v_sy_61;
for (V v_i_69 = new V(0d); new V(v_i_69 < v_segments_65).Truth; v_i_69++) {
V v_q0_70 = (v_i_69 / v_segments_65);
V v_q1_71 = (((v_i_69 + new V(1d))) / v_segments_65);
V v_x_72 = (v_sx_60 - (v_len_62 * v_q1_71));
V v_y_73 = (v_sy_61 + (((VisualJs.MathObject[new V("sin")].Call(((v_t_64 * new V(3d)) - (v_q1_71 * new V(4.2d)))) * v_len_62) * v_amp_66) * ((new V(0.3d) + (new V(0.7d) * v_q1_71)))));
V v_w_74 = (v_len_62 * ((new V(0.14d) - (new V(0.009d) * v_i_69))));
Scope[new V("segmentBetween")].Call(v_ctx_59, v_px_67, v_py_68, v_x_72, v_y_73, v_w_74, v_color_63, (new V(0.82d) - (new V(0.025d) * v_i_69)));
if (new V(v_i_69 < (v_segments_65 - new V(1d))).Truth) Scope[new V("joint")].Call(v_ctx_59, v_x_72, v_y_73, VisualJs.MathObject[new V("max")].Call(new V(0.9d), (v_w_74 * new V(0.22d))), v_color_63);
v_px_67 = v_x_72;
v_py_68 = v_y_73;
}
return default;
}
private static V F_eye(V[] args) {
V v_ctx_75 = VisualJs.Arg(args,0);
V v_x_76 = VisualJs.Arg(args,1);
V v_y_77 = VisualJs.Arg(args,2);
V v_r_78 = VisualJs.Arg(args,3);
V v_color_79 = VisualJs.Arg(args,4);
V v_look_80 = VisualJs.Arg(args,5).Undefined?new V(0d):VisualJs.Arg(args,5);
v_ctx_75[new V("save")].Call();
v_ctx_75[new V("translate")].Call(v_x_76, v_y_77);
v_ctx_75[new V("rotate")].Call(v_look_80);
VisualJs.Assign(v_ctx_75,new V("strokeStyle"),"=",v_color_79);
VisualJs.Assign(v_ctx_75,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_78 * new V(0.16d))));
VisualJs.Assign(v_ctx_75,new V("fillStyle"),"=",new V("rgba(245,252,255,.94)"));
v_ctx_75[new V("beginPath")].Call();
v_ctx_75[new V("ellipse")].Call(new V(0d), new V(0d), v_r_78, (v_r_78 * new V(0.62d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_75[new V("fill")].Call();
v_ctx_75[new V("stroke")].Call();
VisualJs.Assign(v_ctx_75,new V("fillStyle"),"=",v_color_79);
v_ctx_75[new V("beginPath")].Call();
v_ctx_75[new V("ellipse")].Call((v_r_78 * new V(0.18d)), new V(0d), (v_r_78 * new V(0.2d)), (v_r_78 * new V(0.42d)), new V(0d), new V(0d), Scope[new V("TAU")]);
v_ctx_75[new V("fill")].Call();
v_ctx_75[new V("restore")].Call();
return default;
}
private static V F_core(V[] args) {
V v_ctx_81 = VisualJs.Arg(args,0);
V v_x_82 = VisualJs.Arg(args,1);
V v_y_83 = VisualJs.Arg(args,2);
V v_r_84 = VisualJs.Arg(args,3);
V v_color_85 = VisualJs.Arg(args,4);
v_ctx_81[new V("save")].Call();
VisualJs.Assign(v_ctx_81,new V("globalCompositeOperation"),"=",new V("lighter"));
V v_g_86 = v_ctx_81[new V("createRadialGradient")].Call(v_x_82, v_y_83, new V(0d), v_x_82, v_y_83, (v_r_84 * new V(2.2d)));
V v_c_87 = Scope[new V("rgbOf")].Call(v_color_85);
v_g_86[new V("addColorStop")].Call(new V(0d), new V("rgba(255,255,255,.95)"));
v_g_86[new V("addColorStop")].Call(new V(0.24d), ((new V("rgba(") + v_c_87) + new V(",.95)")));
v_g_86[new V("addColorStop")].Call(new V(1d), ((new V("rgba(") + v_c_87) + new V(",0)")));
VisualJs.Assign(v_ctx_81,new V("fillStyle"),"=",v_g_86);
v_ctx_81[new V("beginPath")].Call();
v_ctx_81[new V("arc")].Call(v_x_82, v_y_83, (v_r_84 * new V(2.2d)), new V(0d), Scope[new V("TAU")]);
v_ctx_81[new V("fill")].Call();
v_ctx_81[new V("restore")].Call();
v_ctx_81[new V("save")].Call();
VisualJs.Assign(v_ctx_81,new V("fillStyle"),"=",new V("#07111b"));
VisualJs.Assign(v_ctx_81,new V("strokeStyle"),"=",v_color_85);
VisualJs.Assign(v_ctx_81,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_84 * new V(0.15d))));
v_ctx_81[new V("beginPath")].Call();
v_ctx_81[new V("arc")].Call(v_x_82, v_y_83, v_r_84, new V(0d), Scope[new V("TAU")]);
v_ctx_81[new V("fill")].Call();
v_ctx_81[new V("stroke")].Call();
v_ctx_81[new V("restore")].Call();
return default;
}
private static V F_body(V[] args) {
V v_ctx_88 = VisualJs.Arg(args,0);
V v_r_89 = VisualJs.Arg(args,1);
V v_color_90 = VisualJs.Arg(args,2);
V v_rot_91 = VisualJs.Arg(args,3).Undefined?new V(0d):VisualJs.Arg(args,3);
V v_scaleY_92 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
v_ctx_88[new V("save")].Call();
v_ctx_88[new V("rotate")].Call(v_rot_91);
v_ctx_88[new V("scale")].Call(new V(1d), v_scaleY_92);
VisualJs.Assign(v_ctx_88,new V("fillStyle"),"=",new V("rgba(3,7,13,.98)"));
VisualJs.Assign(v_ctx_88,new V("strokeStyle"),"=",v_color_90);
VisualJs.Assign(v_ctx_88,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_89 * new V(0.055d))));
v_ctx_88[new V("beginPath")].Call();
v_ctx_88[new V("moveTo")].Call(((-v_r_89) * new V(0.72d)), new V(0d));
v_ctx_88[new V("quadraticCurveTo")].Call(((-v_r_89) * new V(0.52d)), ((-v_r_89) * new V(0.62d)), new V(0d), ((-v_r_89) * new V(0.58d)));
v_ctx_88[new V("quadraticCurveTo")].Call((v_r_89 * new V(0.58d)), ((-v_r_89) * new V(0.52d)), (v_r_89 * new V(0.7d)), new V(0d));
v_ctx_88[new V("quadraticCurveTo")].Call((v_r_89 * new V(0.52d)), (v_r_89 * new V(0.56d)), new V(0d), (v_r_89 * new V(0.62d)));
v_ctx_88[new V("quadraticCurveTo")].Call(((-v_r_89) * new V(0.56d)), (v_r_89 * new V(0.55d)), ((-v_r_89) * new V(0.72d)), new V(0d));
v_ctx_88[new V("closePath")].Call();
v_ctx_88[new V("fill")].Call();
v_ctx_88[new V("stroke")].Call();
v_ctx_88[new V("restore")].Call();
return default;
}
private static V F_wing(V[] args) {
V v_ctx_93 = VisualJs.Arg(args,0);
V v_x_94 = VisualJs.Arg(args,1);
V v_y_95 = VisualJs.Arg(args,2);
V v_len_96 = VisualJs.Arg(args,3);
V v_w_97 = VisualJs.Arg(args,4);
V v_a_98 = VisualJs.Arg(args,5);
V v_color_99 = VisualJs.Arg(args,6);
V v_flap_100 = VisualJs.Arg(args,7);
v_ctx_93[new V("save")].Call();
v_ctx_93[new V("translate")].Call(v_x_94, v_y_95);
v_ctx_93[new V("rotate")].Call((v_a_98 + (VisualJs.MathObject[new V("sin")].Call(v_flap_100) * new V(0.14d))));
VisualJs.Assign(v_ctx_93,new V("fillStyle"),"=",new V("rgba(4,9,17,.96)"));
VisualJs.Assign(v_ctx_93,new V("strokeStyle"),"=",v_color_99);
VisualJs.Assign(v_ctx_93,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_w_97 * new V(0.07d))));
v_ctx_93[new V("beginPath")].Call();
v_ctx_93[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_93[new V("quadraticCurveTo")].Call((v_len_96 * new V(0.35d)), ((-v_w_97) * new V(0.85d)), v_len_96, ((-v_w_97) * new V(0.18d)));
v_ctx_93[new V("quadraticCurveTo")].Call((v_len_96 * new V(0.68d)), (v_w_97 * new V(0.55d)), (v_len_96 * new V(0.12d)), (v_w_97 * new V(0.36d)));
v_ctx_93[new V("closePath")].Call();
v_ctx_93[new V("fill")].Call();
v_ctx_93[new V("stroke")].Call();
VisualJs.Assign(v_ctx_93,new V("strokeStyle"),"=",new V("rgba(235,249,255,.25)"));
VisualJs.Assign(v_ctx_93,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.5d), (v_w_97 * new V(0.025d))));
for (V v_i_101 = new V(1d); new V(v_i_101 < new V(4d)).Truth; v_i_101++) {
v_ctx_93[new V("beginPath")].Call();
v_ctx_93[new V("moveTo")].Call((v_len_96 * new V(0.08d)), new V(0d));
v_ctx_93[new V("lineTo")].Call((v_len_96 * ((new V(0.24d) + (v_i_101 * new V(0.18d))))), ((-v_w_97) * ((new V(0.42d) - (v_i_101 * new V(0.08d))))));
v_ctx_93[new V("stroke")].Call();
}
v_ctx_93[new V("restore")].Call();
return default;
}
private static V F_drawVeilRipper(V[] args) {
V v_ctx_102 = VisualJs.Arg(args,0);
V v_r_103 = VisualJs.Arg(args,1);
V v_color_104 = VisualJs.Arg(args,2);
V v_t_105 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_102, v_r_103, v_color_104, (VisualJs.MathObject[new V("sin")].Call((v_t_105 * new V(2.4d))) * new V(0.04d)), new V(0.82d));
Scope[new V("wing")].Call(v_ctx_102, ((-v_r_103) * new V(0.18d)), ((-v_r_103) * new V(0.38d)), (v_r_103 * new V(0.82d)), (v_r_103 * new V(0.34d)), (-new V(1.15d)), v_color_104, (v_t_105 * new V(5d)));
Scope[new V("wing")].Call(v_ctx_102, ((-v_r_103) * new V(0.18d)), (v_r_103 * new V(0.38d)), (v_r_103 * new V(0.82d)), (v_r_103 * new V(0.34d)), new V(1.15d), v_color_104, (v_t_105 * new V(5d)));
Scope[new V("pairedLegs")].Call(v_ctx_102, v_r_103, VisualJs.Arr((-new V(0.28d)), new V(0.2d)), new V(0.3d), new V(0.58d), (v_t_105 * new V(3d)), v_color_104, new V(0.6d));
Scope[new V("eye")].Call(v_ctx_102, (v_r_103 * new V(0.3d)), ((-v_r_103) * new V(0.12d)), (v_r_103 * new V(0.13d)), v_color_104, (-new V(0.08d)));
Scope[new V("core")].Call(v_ctx_102, ((-v_r_103) * new V(0.16d)), (v_r_103 * new V(0.1d)), (v_r_103 * new V(0.09d)), v_color_104);
return default;
}
private static V F_drawGraveLeech(V[] args) {
V v_ctx_106 = VisualJs.Arg(args,0);
V v_r_107 = VisualJs.Arg(args,1);
V v_color_108 = VisualJs.Arg(args,2);
V v_t_109 = VisualJs.Arg(args,3);
for (V v_i_110 = new V(0d); new V(v_i_110 < new V(7d)).Truth; v_i_110++) {
V v_q_111 = (v_i_110 / new V(6d));
V v_x_112 = (((-v_r_107) * new V(0.62d)) + ((v_q_111 * v_r_107) * new V(1.22d)));
V v_y_113 = ((VisualJs.MathObject[new V("sin")].Call(((v_t_109 * new V(3.2d)) + (v_q_111 * new V(4.8d)))) * v_r_107) * new V(0.14d));
v_ctx_106[new V("save")].Call();
v_ctx_106[new V("translate")].Call(v_x_112, v_y_113);
v_ctx_106[new V("rotate")].Call((VisualJs.MathObject[new V("sin")].Call(((v_t_109 * new V(3d)) + (v_q_111 * new V(3.2d)))) * new V(0.14d)));
Scope[new V("body")].Call(v_ctx_106, (v_r_107 * ((new V(0.34d) - (new V(0.025d) * v_q_111)))), v_color_108, new V(0d), new V(0.72d));
v_ctx_106[new V("restore")].Call();
if (new V(v_i_110 < new V(6d)).Truth) Scope[new V("joint")].Call(v_ctx_106, (v_x_112 + (v_r_107 * new V(0.14d))), v_y_113, (v_r_107 * new V(0.07d)), v_color_108);
}
v_ctx_106[new V("save")].Call();
v_ctx_106[new V("translate")].Call((v_r_107 * new V(0.55d)), ((VisualJs.MathObject[new V("sin")].Call(((v_t_109 * new V(3.2d)) + new V(5d))) * v_r_107) * new V(0.1d)));
Scope[new V("body")].Call(v_ctx_106, (v_r_107 * new V(0.34d)), v_color_108, new V(0.05d), new V(0.66d));
Scope[new V("eye")].Call(v_ctx_106, (v_r_107 * new V(0.14d)), ((-v_r_107) * new V(0.1d)), (v_r_107 * new V(0.09d)), v_color_108);
Scope[new V("eye")].Call(v_ctx_106, (v_r_107 * new V(0.14d)), (v_r_107 * new V(0.1d)), (v_r_107 * new V(0.09d)), v_color_108);
v_ctx_106[new V("restore")].Call();
Scope[new V("pairedLegs")].Call(v_ctx_106, v_r_107, VisualJs.Arr((-new V(0.36d)), (-new V(0.02d))), new V(0.2d), new V(0.3d), (v_t_109 * new V(4d)), v_color_108, new V(0.34d));
return default;
}
private static V F_drawFangedCoil(V[] args) {
V v_ctx_114 = VisualJs.Arg(args,0);
V v_r_115 = VisualJs.Arg(args,1);
V v_color_116 = VisualJs.Arg(args,2);
V v_t_117 = VisualJs.Arg(args,3);
for (V v_i_118 = new V(0d); new V(v_i_118 < new V(8d)).Truth; v_i_118++) {
V v_q_119 = (v_i_118 / new V(7d));
V v_x_120 = ((v_r_115 * new V(0.46d)) - ((v_q_119 * v_r_115) * new V(1.1d)));
V v_y_121 = ((VisualJs.MathObject[new V("sin")].Call(((v_t_117 * new V(3.1d)) + (v_q_119 * new V(4.8d)))) * v_r_115) * ((new V(0.08d) + (new V(0.06d) * v_q_119))));
V v_dx_122 = (((-v_r_115) * new V(1.1d)) / new V(7d));
V v_dy_123 = ((((VisualJs.MathObject[new V("cos")].Call(((v_t_117 * new V(3.1d)) + (v_q_119 * new V(4.8d)))) * v_r_115) * ((new V(0.08d) + (new V(0.06d) * v_q_119)))) * new V(4.8d)) / new V(7d));
V v_a_124 = VisualJs.MathObject[new V("atan2")].Call(v_dy_123, v_dx_122);
v_ctx_114[new V("save")].Call();
v_ctx_114[new V("translate")].Call(v_x_120, v_y_121);
Scope[new V("body")].Call(v_ctx_114, (v_r_115 * ((new V(0.29d) - (new V(0.018d) * v_i_118)))), v_color_116, v_a_124, new V(0.64d));
v_ctx_114[new V("restore")].Call();
if (new V(v_i_118 < new V(7d)).Truth) Scope[new V("joint")].Call(v_ctx_114, (v_x_120 + (v_dx_122 * new V(0.5d))), (v_y_121 + (v_dy_123 * new V(0.5d))), (v_r_115 * new V(0.055d)), v_color_116);
}
v_ctx_114[new V("save")].Call();
v_ctx_114[new V("translate")].Call((v_r_115 * new V(0.5d)), ((-v_r_115) * new V(0.06d)));
Scope[new V("body")].Call(v_ctx_114, (v_r_115 * new V(0.36d)), v_color_116, (-new V(0.06d)), new V(0.62d));
Scope[new V("eye")].Call(v_ctx_114, (v_r_115 * new V(0.14d)), ((-v_r_115) * new V(0.1d)), (v_r_115 * new V(0.09d)), v_color_116);
Scope[new V("eye")].Call(v_ctx_114, (v_r_115 * new V(0.14d)), (v_r_115 * new V(0.1d)), (v_r_115 * new V(0.09d)), v_color_116);
VisualJs.Assign(v_ctx_114,new V("fillStyle"),"=",new V("#02050a"));
VisualJs.Assign(v_ctx_114,new V("strokeStyle"),"=",v_color_116);
VisualJs.Assign(v_ctx_114,new V("lineWidth"),"=",new V(1d));
v_ctx_114[new V("beginPath")].Call();
v_ctx_114[new V("moveTo")].Call((v_r_115 * new V(0.12d)), new V(0d));
v_ctx_114[new V("lineTo")].Call((v_r_115 * new V(0.34d)), (v_r_115 * new V(0.05d)));
v_ctx_114[new V("lineTo")].Call((v_r_115 * new V(0.12d)), (v_r_115 * new V(0.1d)));
v_ctx_114[new V("closePath")].Call();
v_ctx_114[new V("fill")].Call();
v_ctx_114[new V("stroke")].Call();
v_ctx_114[new V("restore")].Call();
return default;
}
private static V F_drawCarrionSkitter(V[] args) {
V v_ctx_125 = VisualJs.Arg(args,0);
V v_r_126 = VisualJs.Arg(args,1);
V v_color_127 = VisualJs.Arg(args,2);
V v_t_128 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_125, v_r_126, v_color_127, (VisualJs.MathObject[new V("sin")].Call((v_t_128 * new V(4d))) * new V(0.05d)), new V(0.58d));
Scope[new V("pairedLegs")].Call(v_ctx_125, v_r_126, VisualJs.Arr((-new V(0.4d)), (-new V(0.04d)), new V(0.32d)), new V(0.22d), new V(0.58d), (v_t_128 * new V(7d)), v_color_127, new V(0.62d));
v_ctx_125[new V("save")].Call();
v_ctx_125[new V("translate")].Call((v_r_126 * new V(0.45d)), new V(0d));
Scope[new V("body")].Call(v_ctx_125, (v_r_126 * new V(0.34d)), v_color_127, (-new V(0.05d)), new V(0.7d));
Scope[new V("eye")].Call(v_ctx_125, new V(0d), ((-v_r_126) * new V(0.1d)), (v_r_126 * new V(0.075d)), v_color_127);
Scope[new V("eye")].Call(v_ctx_125, new V(0d), (v_r_126 * new V(0.1d)), (v_r_126 * new V(0.075d)), v_color_127);
v_ctx_125[new V("restore")].Call();
return default;
}
private static V F_drawUmbralMoth(V[] args) {
V v_ctx_129 = VisualJs.Arg(args,0);
V v_r_130 = VisualJs.Arg(args,1);
V v_color_131 = VisualJs.Arg(args,2);
V v_t_132 = VisualJs.Arg(args,3);
Scope[new V("wing")].Call(v_ctx_129, ((-v_r_130) * new V(0.08d)), ((-v_r_130) * new V(0.4d)), (v_r_130 * new V(0.92d)), (v_r_130 * new V(0.62d)), (-new V(2.72d)), v_color_131, (v_t_132 * new V(8d)));
Scope[new V("wing")].Call(v_ctx_129, ((-v_r_130) * new V(0.08d)), (v_r_130 * new V(0.4d)), (v_r_130 * new V(0.92d)), (v_r_130 * new V(0.62d)), new V(2.72d), v_color_131, (v_t_132 * new V(8d)));
v_ctx_129[new V("save")].Call();
v_ctx_129[new V("scale")].Call(new V(0.45d), new V(1.25d));
Scope[new V("body")].Call(v_ctx_129, (v_r_130 * new V(0.72d)), v_color_131, new V(0d), new V(0.9d));
v_ctx_129[new V("restore")].Call();
Scope[new V("pairedLegs")].Call(v_ctx_129, v_r_130, VisualJs.Arr((-new V(0.16d)), new V(0.1d)), new V(0.28d), new V(0.48d), (v_t_132 * new V(8d)), v_color_131, new V(0.36d));
Scope[new V("eye")].Call(v_ctx_129, ((-v_r_130) * new V(0.07d)), ((-v_r_130) * new V(0.26d)), (v_r_130 * new V(0.09d)), v_color_131);
Scope[new V("eye")].Call(v_ctx_129, (v_r_130 * new V(0.07d)), ((-v_r_130) * new V(0.26d)), (v_r_130 * new V(0.09d)), v_color_131);
return default;
}
private static V F_drawRiftScarab(V[] args) {
V v_ctx_133 = VisualJs.Arg(args,0);
V v_r_134 = VisualJs.Arg(args,1);
V v_color_135 = VisualJs.Arg(args,2);
V v_t_136 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_133, v_r_134, v_color_135, (VisualJs.MathObject[new V("sin")].Call((v_t_136 * new V(1.6d))) * new V(0.03d)), new V(0.68d));
v_ctx_133[new V("save")].Call();
VisualJs.Assign(v_ctx_133,new V("strokeStyle"),"=",new V("rgba(235,249,255,.28)"));
VisualJs.Assign(v_ctx_133,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.6d), (v_r_134 * new V(0.025d))));
for (V v_i_137 = (-new V(1d)); new V(v_i_137 <= new V(1d)).Truth; v_i_137++) {
v_ctx_133[new V("beginPath")].Call();
v_ctx_133[new V("moveTo")].Call(((-v_r_134) * new V(0.52d)), ((v_i_137 * v_r_134) * new V(0.12d)));
v_ctx_133[new V("quadraticCurveTo")].Call(new V(0d), ((v_i_137 * v_r_134) * new V(0.22d)), (v_r_134 * new V(0.5d)), ((v_i_137 * v_r_134) * new V(0.08d)));
v_ctx_133[new V("stroke")].Call();
}
v_ctx_133[new V("restore")].Call();
Scope[new V("pairedLegs")].Call(v_ctx_133, v_r_134, VisualJs.Arr((-new V(0.34d)), new V(0.12d)), new V(0.24d), new V(0.55d), (v_t_136 * new V(3d)), v_color_135, new V(0.55d));
Scope[new V("eye")].Call(v_ctx_133, (v_r_134 * new V(0.48d)), ((-v_r_134) * new V(0.1d)), (v_r_134 * new V(0.07d)), v_color_135);
Scope[new V("eye")].Call(v_ctx_133, (v_r_134 * new V(0.48d)), (v_r_134 * new V(0.1d)), (v_r_134 * new V(0.07d)), v_color_135);
Scope[new V("core")].Call(v_ctx_133, ((-v_r_134) * new V(0.22d)), new V(0d), (v_r_134 * new V(0.08d)), v_color_135);
return default;
}
private static V F_drawBonebackBrute(V[] args) {
V v_ctx_138 = VisualJs.Arg(args,0);
V v_r_139 = VisualJs.Arg(args,1);
V v_color_140 = VisualJs.Arg(args,2);
V v_t_141 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_138, (v_r_139 * new V(1.05d)), v_color_140, (VisualJs.MathObject[new V("sin")].Call((v_t_141 * new V(1.7d))) * new V(0.025d)), new V(0.88d));
Scope[new V("pairedLegs")].Call(v_ctx_138, v_r_139, VisualJs.Arr((-new V(0.45d)), (-new V(0.12d)), new V(0.2d), new V(0.48d)), new V(0.3d), new V(0.72d), (v_t_141 * new V(6d)), v_color_140, new V(0.78d));
for (V v_i_142 = new V(0d); new V(v_i_142 < new V(3d)).Truth; v_i_142++) {
v_ctx_138[new V("save")].Call();
v_ctx_138[new V("translate")].Call((((-v_r_139) * new V(0.25d)) + ((v_i_142 * v_r_139) * new V(0.22d))), ((-v_r_139) * new V(0.4d)));
v_ctx_138[new V("rotate")].Call(((-new V(0.1d)) + (v_i_142 * new V(0.1d))));
VisualJs.Assign(v_ctx_138,new V("fillStyle"),"=",new V("rgba(6,10,16,.98)"));
VisualJs.Assign(v_ctx_138,new V("strokeStyle"),"=",new V("rgba(230,240,250,.46)"));
VisualJs.Assign(v_ctx_138,new V("lineWidth"),"=",new V(1d));
v_ctx_138[new V("beginPath")].Call();
v_ctx_138[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_138[new V("lineTo")].Call((v_r_139 * new V(0.1d)), ((-v_r_139) * new V(0.3d)));
v_ctx_138[new V("lineTo")].Call((v_r_139 * new V(0.2d)), new V(0d));
v_ctx_138[new V("closePath")].Call();
v_ctx_138[new V("fill")].Call();
v_ctx_138[new V("stroke")].Call();
v_ctx_138[new V("restore")].Call();
}
Scope[new V("eye")].Call(v_ctx_138, (v_r_139 * new V(0.36d)), ((-v_r_139) * new V(0.17d)), (v_r_139 * new V(0.11d)), v_color_140);
Scope[new V("eye")].Call(v_ctx_138, (v_r_139 * new V(0.36d)), (v_r_139 * new V(0.17d)), (v_r_139 * new V(0.11d)), v_color_140);
return default;
}
private static V F_drawGlassHound(V[] args) {
V v_ctx_143 = VisualJs.Arg(args,0);
V v_r_144 = VisualJs.Arg(args,1);
V v_color_145 = VisualJs.Arg(args,2);
V v_t_146 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_143, (v_r_144 * new V(0.92d)), v_color_145, (VisualJs.MathObject[new V("sin")].Call((v_t_146 * new V(2.5d))) * new V(0.04d)), new V(0.62d));
Scope[new V("pairedLegs")].Call(v_ctx_143, v_r_144, VisualJs.Arr((-new V(0.34d)), new V(0.1d)), new V(0.3d), new V(0.6d), (v_t_146 * new V(5d)), v_color_145, new V(0.48d));
v_ctx_143[new V("save")].Call();
v_ctx_143[new V("translate")].Call((v_r_144 * new V(0.48d)), ((-v_r_144) * new V(0.1d)));
Scope[new V("body")].Call(v_ctx_143, (v_r_144 * new V(0.42d)), v_color_145, (-new V(0.12d)), new V(0.62d));
Scope[new V("eye")].Call(v_ctx_143, (v_r_144 * new V(0.13d)), ((-v_r_144) * new V(0.1d)), (v_r_144 * new V(0.09d)), v_color_145);
v_ctx_143[new V("restore")].Call();
Scope[new V("tail")].Call(v_ctx_143, ((-v_r_144) * new V(0.96d)), new V(0d), (v_r_144 * new V(0.82d)), v_color_145, v_t_146, new V(6d), new V(0.2d));
return default;
}
private static V F_drawHollowStalker(V[] args) {
V v_ctx_147 = VisualJs.Arg(args,0);
V v_r_148 = VisualJs.Arg(args,1);
V v_color_149 = VisualJs.Arg(args,2);
V v_t_150 = VisualJs.Arg(args,3);
v_ctx_147[new V("save")].Call();
v_ctx_147[new V("translate")].Call(new V(0d), ((VisualJs.MathObject[new V("sin")].Call((v_t_150 * new V(4d))) * v_r_148) * new V(0.06d)));
Scope[new V("body")].Call(v_ctx_147, (v_r_148 * new V(0.72d)), v_color_149, new V(0d), new V(1.25d));
v_ctx_147[new V("restore")].Call();
Scope[new V("pairedLegs")].Call(v_ctx_147, v_r_148, VisualJs.Arr((-new V(0.22d)), new V(0.16d)), new V(0.24d), new V(0.9d), (v_t_150 * new V(5d)), v_color_149, new V(0.52d));
Scope[new V("eye")].Call(v_ctx_147, (v_r_148 * new V(0.1d)), ((-v_r_148) * new V(0.24d)), (v_r_148 * new V(0.12d)), v_color_149);
Scope[new V("core")].Call(v_ctx_147, ((-v_r_148) * new V(0.12d)), (v_r_148 * new V(0.2d)), (v_r_148 * new V(0.075d)), v_color_149);
return default;
}
private static V F_drawCableWidow(V[] args) {
V v_ctx_151 = VisualJs.Arg(args,0);
V v_r_152 = VisualJs.Arg(args,1);
V v_color_153 = VisualJs.Arg(args,2);
V v_t_154 = VisualJs.Arg(args,3);
Scope[new V("body")].Call(v_ctx_151, (v_r_152 * new V(0.92d)), v_color_153, new V(0d), new V(0.72d));
Scope[new V("pairedLegs")].Call(v_ctx_151, v_r_152, VisualJs.Arr((-new V(0.36d)), (-new V(0.12d)), new V(0.12d), new V(0.36d)), new V(0.18d), new V(0.58d), (v_t_154 * new V(4d)), v_color_153, new V(0.4d));
Scope[new V("eye")].Call(v_ctx_151, (v_r_152 * new V(0.26d)), ((-v_r_152) * new V(0.1d)), (v_r_152 * new V(0.085d)), new V("#ff6480"));
Scope[new V("eye")].Call(v_ctx_151, (v_r_152 * new V(0.26d)), (v_r_152 * new V(0.1d)), (v_r_152 * new V(0.085d)), new V("#ff6480"));
Scope[new V("core")].Call(v_ctx_151, ((-v_r_152) * new V(0.2d)), new V(0d), (v_r_152 * new V(0.08d)), new V("#ff6480"));
return default;
}
private static V F_bossFrame(V[] args) {
V v_ctx_155 = VisualJs.Arg(args,0);
V v_r_156 = VisualJs.Arg(args,1);
V v_color_157 = VisualJs.Arg(args,2);
V v_t_158 = VisualJs.Arg(args,3);
v_ctx_155[new V("save")].Call();
VisualJs.Assign(v_ctx_155,new V("strokeStyle"),"=",((new V("rgba(") + Scope[new V("rgbOf")].Call(v_color_157)) + new V(",.30)")));
VisualJs.Assign(v_ctx_155,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_r_156 * new V(0.025d))));
v_ctx_155[new V("beginPath")].Call();
v_ctx_155[new V("arc")].Call(new V(0d), new V(0d), (v_r_156 * new V(1.16d)), (v_t_158 * new V(0.2d)), ((v_t_158 * new V(0.2d)) + new V(4.9d)));
v_ctx_155[new V("stroke")].Call();
v_ctx_155[new V("restore")].Call();
return default;
}
private static V F_drawVoidLancerBoss(V[] args) {
V v_ctx_159 = VisualJs.Arg(args,0);
V v_r_160 = VisualJs.Arg(args,1);
V v_color_161 = VisualJs.Arg(args,2);
V v_t_162 = VisualJs.Arg(args,3);
Scope[new V("bossFrame")].Call(v_ctx_159, v_r_160, v_color_161, v_t_162);
Scope[new V("body")].Call(v_ctx_159, (v_r_160 * new V(1.12d)), v_color_161, (VisualJs.MathObject[new V("sin")].Call((v_t_162 * new V(1.7d))) * new V(0.025d)), new V(0.62d));
Scope[new V("wing")].Call(v_ctx_159, ((-v_r_160) * new V(0.18d)), ((-v_r_160) * new V(0.48d)), (v_r_160 * new V(1d)), (v_r_160 * new V(0.36d)), (-new V(1.15d)), v_color_161, (v_t_162 * new V(4d)));
Scope[new V("wing")].Call(v_ctx_159, ((-v_r_160) * new V(0.18d)), (v_r_160 * new V(0.48d)), (v_r_160 * new V(1d)), (v_r_160 * new V(0.36d)), new V(1.15d), v_color_161, ((v_t_162 * new V(4d)) + new V(0.35d)));
Scope[new V("pairedLegs")].Call(v_ctx_159, v_r_160, VisualJs.Arr((-new V(0.3d)), new V(0.18d)), new V(0.34d), new V(0.62d), (v_t_162 * new V(3d)), v_color_161, new V(0.82d));
v_ctx_159[new V("save")].Call();
v_ctx_159[new V("translate")].Call((v_r_160 * new V(0.62d)), new V(0d));
Scope[new V("body")].Call(v_ctx_159, (v_r_160 * new V(0.48d)), v_color_161, new V(0d), new V(0.58d));
Scope[new V("eye")].Call(v_ctx_159, (v_r_160 * new V(0.18d)), ((-v_r_160) * new V(0.12d)), (v_r_160 * new V(0.14d)), v_color_161);
Scope[new V("eye")].Call(v_ctx_159, (v_r_160 * new V(0.18d)), (v_r_160 * new V(0.12d)), (v_r_160 * new V(0.14d)), v_color_161);
v_ctx_159[new V("restore")].Call();
Scope[new V("tail")].Call(v_ctx_159, ((-v_r_160) * new V(0.9d)), new V(0d), (v_r_160 * new V(1.1d)), v_color_161, v_t_162, new V(8d), new V(0.16d));
Scope[new V("core")].Call(v_ctx_159, ((-v_r_160) * new V(0.12d)), new V(0d), (v_r_160 * new V(0.12d)), v_color_161);
return default;
}
private static V F_drawDreadChargerBoss(V[] args) {
V v_ctx_163 = VisualJs.Arg(args,0);
V v_r_164 = VisualJs.Arg(args,1);
V v_color_165 = VisualJs.Arg(args,2);
V v_t_166 = VisualJs.Arg(args,3);
Scope[new V("bossFrame")].Call(v_ctx_163, v_r_164, v_color_165, v_t_166);
Scope[new V("body")].Call(v_ctx_163, (v_r_164 * new V(1.14d)), v_color_165, (VisualJs.MathObject[new V("sin")].Call((v_t_166 * new V(2d))) * new V(0.025d)), new V(0.84d));
Scope[new V("pairedLegs")].Call(v_ctx_163, v_r_164, VisualJs.Arr((-new V(0.34d)), new V(0.2d)), new V(0.35d), new V(0.82d), (v_t_166 * new V(6d)), v_color_165, new V(0.9d));
v_ctx_163[new V("save")].Call();
v_ctx_163[new V("translate")].Call((v_r_164 * new V(0.5d)), new V(0d));
Scope[new V("body")].Call(v_ctx_163, (v_r_164 * new V(0.5d)), v_color_165, new V(0d), new V(0.72d));
Scope[new V("eye")].Call(v_ctx_163, (v_r_164 * new V(0.2d)), ((-v_r_164) * new V(0.14d)), (v_r_164 * new V(0.15d)), v_color_165);
Scope[new V("eye")].Call(v_ctx_163, (v_r_164 * new V(0.2d)), (v_r_164 * new V(0.14d)), (v_r_164 * new V(0.15d)), v_color_165);
v_ctx_163[new V("restore")].Call();
foreach (V v_side_167 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
Scope[new V("wing")].Call(v_ctx_163, ((-v_r_164) * new V(0.05d)), ((v_side_167 * v_r_164) * new V(0.5d)), (v_r_164 * new V(0.72d)), (v_r_164 * new V(0.25d)), (new V(v_side_167 > new V(0d)).Truth ? new V(0.72d) : (-new V(0.72d))), v_color_165, ((v_t_166 * new V(3d)) + ((new V(v_side_167 > new V(0d)).Truth ? new V(0d) : new V(0.35d)))));
}
Scope[new V("core")].Call(v_ctx_163, ((-v_r_164) * new V(0.3d)), new V(0d), (v_r_164 * new V(0.14d)), v_color_165);
return default;
}
private static V F_drawBroodMatriarchBoss(V[] args) {
V v_ctx_168 = VisualJs.Arg(args,0);
V v_r_169 = VisualJs.Arg(args,1);
V v_color_170 = VisualJs.Arg(args,2);
V v_t_171 = VisualJs.Arg(args,3);
Scope[new V("bossFrame")].Call(v_ctx_168, v_r_169, v_color_170, v_t_171);
Scope[new V("body")].Call(v_ctx_168, (v_r_169 * new V(1d)), v_color_170, (VisualJs.MathObject[new V("sin")].Call(v_t_171) * new V(0.02d)), new V(0.96d));
Scope[new V("pairedLegs")].Call(v_ctx_168, v_r_169, VisualJs.Arr((-new V(0.34d)), new V(0.18d)), new V(0.38d), new V(1d), (v_t_171 * new V(3d)), v_color_170, new V(0.95d));
v_ctx_168[new V("save")].Call();
v_ctx_168[new V("translate")].Call((v_r_169 * new V(0.34d)), ((-v_r_169) * new V(0.05d)));
Scope[new V("body")].Call(v_ctx_168, (v_r_169 * new V(0.52d)), v_color_170, (-new V(0.05d)), new V(0.76d));
Scope[new V("eye")].Call(v_ctx_168, (v_r_169 * new V(0.2d)), ((-v_r_169) * new V(0.12d)), (v_r_169 * new V(0.14d)), v_color_170);
Scope[new V("eye")].Call(v_ctx_168, (v_r_169 * new V(0.2d)), (v_r_169 * new V(0.12d)), (v_r_169 * new V(0.14d)), v_color_170);
v_ctx_168[new V("restore")].Call();
for (V v_i_172 = new V(0d); new V(v_i_172 < new V(5d)).Truth; v_i_172++) {
V v_a_173 = ((v_t_171 * new V(0.8d)) + ((v_i_172 * Scope[new V("TAU")]) / new V(5d)));
V v_x_174 = ((VisualJs.MathObject[new V("cos")].Call(v_a_173) * v_r_169) * new V(1.1d));
V v_y_175 = ((VisualJs.MathObject[new V("sin")].Call(v_a_173) * v_r_169) * new V(0.72d));
v_ctx_168[new V("save")].Call();
v_ctx_168[new V("translate")].Call(v_x_174, v_y_175);
v_ctx_168[new V("scale")].Call((new V(0.8d) + (new V(0.15d) * VisualJs.MathObject[new V("sin")].Call(((v_t_171 * new V(3d)) + v_i_172)))), new V(1d));
Scope[new V("body")].Call(v_ctx_168, (v_r_169 * new V(0.24d)), v_color_170, v_a_173, new V(0.9d));
Scope[new V("eye")].Call(v_ctx_168, new V(0d), ((-v_r_169) * new V(0.03d)), (v_r_169 * new V(0.055d)), v_color_170);
v_ctx_168[new V("restore")].Call();
}
Scope[new V("core")].Call(v_ctx_168, ((-v_r_169) * new V(0.2d)), (v_r_169 * new V(0.06d)), (v_r_169 * new V(0.13d)), v_color_170);
return default;
}
private static V F_drawAbyssalLeviathanBoss(V[] args) {
V v_ctx_176 = VisualJs.Arg(args,0);
V v_r_177 = VisualJs.Arg(args,1);
V v_color_178 = VisualJs.Arg(args,2);
V v_t_179 = VisualJs.Arg(args,3);
Scope[new V("bossFrame")].Call(v_ctx_176, v_r_177, v_color_178, v_t_179);
for (V v_i_180 = new V(0d); new V(v_i_180 < new V(8d)).Truth; v_i_180++) {
V v_q_181 = (v_i_180 / new V(7d));
V v_x_182 = (((-v_r_177) * new V(0.76d)) + ((v_i_180 * v_r_177) * new V(0.22d)));
V v_y_183 = ((VisualJs.MathObject[new V("sin")].Call(((v_t_179 * new V(2d)) + (v_i_180 * new V(0.72d)))) * v_r_177) * new V(0.16d));
v_ctx_176[new V("save")].Call();
v_ctx_176[new V("translate")].Call(v_x_182, v_y_183);
Scope[new V("body")].Call(v_ctx_176, (v_r_177 * ((new V(0.5d) - (new V(0.035d) * v_i_180)))), v_color_178, (VisualJs.MathObject[new V("sin")].Call((v_t_179 + v_i_180)) * new V(0.12d)), new V(0.7d));
v_ctx_176[new V("restore")].Call();
}
v_ctx_176[new V("save")].Call();
v_ctx_176[new V("translate")].Call((v_r_177 * new V(0.54d)), new V(0d));
Scope[new V("body")].Call(v_ctx_176, (v_r_177 * new V(0.58d)), v_color_178, (-new V(0.08d)), new V(0.68d));
Scope[new V("eye")].Call(v_ctx_176, (v_r_177 * new V(0.22d)), ((-v_r_177) * new V(0.16d)), (v_r_177 * new V(0.15d)), v_color_178);
Scope[new V("eye")].Call(v_ctx_176, (v_r_177 * new V(0.22d)), (v_r_177 * new V(0.16d)), (v_r_177 * new V(0.15d)), v_color_178);
v_ctx_176[new V("restore")].Call();
Scope[new V("pairedLegs")].Call(v_ctx_176, v_r_177, VisualJs.Arr((-new V(0.28d)), new V(0.1d)), new V(0.32d), new V(0.82d), (v_t_179 * new V(2d)), v_color_178, new V(0.7d));
Scope[new V("tail")].Call(v_ctx_176, ((-v_r_177) * new V(0.9d)), new V(0d), (v_r_177 * new V(1.16d)), v_color_178, v_t_179, new V(9d), new V(0.14d));
Scope[new V("core")].Call(v_ctx_176, ((-v_r_177) * new V(0.08d)), new V(0d), (v_r_177 * new V(0.12d)), v_color_178);
return default;
}
private static V F_creature(V[] args) {
V v_ctx_184 = VisualJs.Arg(args,0);
V v_r_185 = VisualJs.Arg(args,1);
V v_color_186 = VisualJs.Arg(args,2);
V v_t_187 = VisualJs.Arg(args,3);
V v_variant_188 = VisualJs.Arg(args,4);
if (new V(v_variant_188 == new V("wisp")).Truth) Scope[new V("drawVeilRipper")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("leech")).Truth) Scope[new V("drawGraveLeech")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("serpent")).Truth) Scope[new V("drawFangedCoil")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("skitter")).Truth) Scope[new V("drawCarrionSkitter")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("moth")).Truth) Scope[new V("drawUmbralMoth")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("beetle")).Truth) Scope[new V("drawRiftScarab")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("brute")).Truth) Scope[new V("drawBonebackBrute")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("prism")).Truth) Scope[new V("drawGlassHound")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else if (new V(v_variant_188 == new V("stalker")).Truth) Scope[new V("drawHollowStalker")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187); else Scope[new V("drawCableWidow")].Call(v_ctx_184, v_r_185, v_color_186, v_t_187);
return default;
}
private static V F_boss(V[] args) {
V v_ctx_189 = VisualJs.Arg(args,0);
V v_r_190 = VisualJs.Arg(args,1);
V v_color_191 = VisualJs.Arg(args,2);
V v_t_192 = VisualJs.Arg(args,3);
V v_type_193 = VisualJs.Arg(args,4);
if (new V(v_type_193 == new V("shooter")).Truth) Scope[new V("drawVoidLancerBoss")].Call(v_ctx_189, v_r_190, v_color_191, v_t_192); else if (new V(v_type_193 == new V("charger")).Truth) Scope[new V("drawDreadChargerBoss")].Call(v_ctx_189, v_r_190, v_color_191, v_t_192); else if (new V(v_type_193 == new V("summoner")).Truth) Scope[new V("drawBroodMatriarchBoss")].Call(v_ctx_189, v_r_190, v_color_191, v_t_192); else Scope[new V("drawAbyssalLeviathanBoss")].Call(v_ctx_189, v_r_190, v_color_191, v_t_192);
return default;
}
private static V F_drawEnemyCreature(V[] args) {
V v_ctx_194 = VisualJs.Arg(args,0);
V v_e_195 = VisualJs.Arg(args,1);
V v_t_196 = VisualJs.Arg(args,2);
V v_r_197 = v_e_195[new V("radius")];
V v_color_198 = v_e_195[new V("color")];
v_ctx_194[new V("save")].Call();
Scope[new V("glow")].Call(v_ctx_194, (v_r_197 * new V(2.15d)), v_color_198, new V(0.065d));
Scope[new V("creature")].Call(v_ctx_194, v_r_197, v_color_198, v_t_196, VisualJs.Or(v_e_195[new V("visualVariant")], () => new V("wisp")));
v_ctx_194[new V("restore")].Call();
return default;
}
private static V F_drawBossCreature(V[] args) {
V v_ctx_199 = VisualJs.Arg(args,0);
V v_e_200 = VisualJs.Arg(args,1);
V v_t_201 = VisualJs.Arg(args,2);
V v_r_202 = v_e_200[new V("radius")];
V v_color_203 = v_e_200[new V("color")];
v_ctx_199[new V("save")].Call();
Scope[new V("glow")].Call(v_ctx_199, (v_r_202 * new V(2.55d)), v_color_203, new V(0.085d));
Scope[new V("boss")].Call(v_ctx_199, v_r_202, v_color_203, v_t_201, v_e_200[new V("bossType")]);
v_ctx_199[new V("restore")].Call();
return default;
}
}
}
