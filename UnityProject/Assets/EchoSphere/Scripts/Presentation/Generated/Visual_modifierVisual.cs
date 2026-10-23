#if UNITY_EDITOR
// Generated from src/spheres/modifierVisual.ts; sha256 2e9e5cc4dce10a8d2c68c4c4b53c42ea58760f6b713da212d62e0a189eba7c9c.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_modifierVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_modifierVisual() {
Scope[new V("modifierValue")] = VisualJs.Fn(F_modifierValue);
Scope[new V("drawFlame")] = VisualJs.Fn(F_drawFlame);
Scope[new V("drawFreeze")] = VisualJs.Fn(F_drawFreeze);
Scope[new V("drawPoison")] = VisualJs.Fn(F_drawPoison);
Scope[new V("drawPierce")] = VisualJs.Fn(F_drawPierce);
Scope[new V("drawRicochet")] = VisualJs.Fn(F_drawRicochet);
Scope[new V("drawMultishot")] = VisualJs.Fn(F_drawMultishot);
Scope[new V("drawModifierGlyph")] = VisualJs.Fn(F_drawModifierGlyph);
Scope[new V("renderSphereModifierVfx")] = VisualJs.Fn(F_renderSphereModifierVfx);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
Scope[new V("MODIFIER_COLOR")] = VisualJs.Obj(new V("multishot"), new V("#dff8ff"), new V("pierce"), new V("#e9ffff"), new V("ricochet"), new V("#b897ff"), new V("fire"), new V("#ff743d"), new V("freeze"), new V("#bff4ff"), new V("poison"), new V("#6df0a9"));
Scope[new V("MODIFIER_ORDER")] = VisualJs.Arr(new V("multishot"), new V("pierce"), new V("ricochet"), new V("fire"), new V("freeze"), new V("poison"));
Scope[new V("PROJECTILE_ONLY")] = VisualJs.Arr(new V("multishot"), new V("pierce"), new V("ricochet"));
}
private static V F_modifierValue(V[] args) {
V v_mods_0 = VisualJs.Arg(args,0);
V v_kind_1 = VisualJs.Arg(args,1);
V v_value_2 = VisualJs.Number(VisualJs.Coalesce(VisualJs.Chain(v_mods_0,optional_3=>optional_3[v_kind_1]), () => new V(0d)));
return (VisualJs.NumberObject[new V("isFinite")].Call(v_value_2).Truth ? VisualJs.MathObject[new V("max")].Call(new V(0d), v_value_2) : new V(0d));
}
private static V F_drawFlame(V[] args) {
V v_ctx_4 = VisualJs.Arg(args,0);
V v_size_5 = VisualJs.Arg(args,1);
v_ctx_4[new V("beginPath")].Call();
v_ctx_4[new V("moveTo")].Call(new V(0d), (-v_size_5));
v_ctx_4[new V("quadraticCurveTo")].Call((v_size_5 * new V(0.95d)), ((-v_size_5) * new V(0.2d)), (v_size_5 * new V(0.55d)), (v_size_5 * new V(0.58d)));
v_ctx_4[new V("quadraticCurveTo")].Call(new V(0d), v_size_5, ((-v_size_5) * new V(0.55d)), (v_size_5 * new V(0.58d)));
v_ctx_4[new V("quadraticCurveTo")].Call(((-v_size_5) * new V(0.95d)), ((-v_size_5) * new V(0.2d)), new V(0d), (-v_size_5));
v_ctx_4[new V("stroke")].Call();
v_ctx_4[new V("beginPath")].Call();
v_ctx_4[new V("moveTo")].Call(new V(0d), ((-v_size_5) * new V(0.36d)));
v_ctx_4[new V("quadraticCurveTo")].Call((v_size_5 * new V(0.42d)), new V(0d), new V(0d), (v_size_5 * new V(0.52d)));
v_ctx_4[new V("quadraticCurveTo")].Call(((-v_size_5) * new V(0.42d)), new V(0d), new V(0d), ((-v_size_5) * new V(0.36d)));
v_ctx_4[new V("stroke")].Call();
return default;
}
private static V F_drawFreeze(V[] args) {
V v_ctx_6 = VisualJs.Arg(args,0);
V v_size_7 = VisualJs.Arg(args,1);
for (V v_i_8 = new V(0d); new V(v_i_8 < new V(6d)).Truth; v_i_8++) {
V v_a_9 = ((v_i_8 * VisualJs.MathObject[new V("PI")]) / new V(3d));
v_ctx_6[new V("beginPath")].Call();
v_ctx_6[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_9) * v_size_7) * new V(0.18d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_9) * v_size_7) * new V(0.18d)));
v_ctx_6[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_9) * v_size_7), (VisualJs.MathObject[new V("sin")].Call(v_a_9) * v_size_7));
VisualJs.Assign(v_ctx_6,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_size_7 * new V(0.18d))));
v_ctx_6[new V("stroke")].Call();
}
v_ctx_6[new V("beginPath")].Call();
v_ctx_6[new V("arc")].Call(new V(0d), new V(0d), (v_size_7 * new V(0.22d)), new V(0d), Scope[new V("TAU")]);
v_ctx_6[new V("stroke")].Call();
return default;
}
private static V F_drawPoison(V[] args) {
V v_ctx_10 = VisualJs.Arg(args,0);
V v_size_11 = VisualJs.Arg(args,1);
v_ctx_10[new V("beginPath")].Call();
v_ctx_10[new V("moveTo")].Call(new V(0d), (-v_size_11));
v_ctx_10[new V("quadraticCurveTo")].Call((v_size_11 * new V(0.86d)), ((-v_size_11) * new V(0.2d)), (v_size_11 * new V(0.72d)), (v_size_11 * new V(0.34d)));
v_ctx_10[new V("quadraticCurveTo")].Call((v_size_11 * new V(0.5d)), v_size_11, new V(0d), v_size_11);
v_ctx_10[new V("quadraticCurveTo")].Call(((-v_size_11) * new V(0.5d)), v_size_11, ((-v_size_11) * new V(0.72d)), (v_size_11 * new V(0.34d)));
v_ctx_10[new V("quadraticCurveTo")].Call(((-v_size_11) * new V(0.86d)), ((-v_size_11) * new V(0.2d)), new V(0d), (-v_size_11));
v_ctx_10[new V("stroke")].Call();
v_ctx_10[new V("beginPath")].Call();
v_ctx_10[new V("moveTo")].Call(new V(0d), ((-v_size_11) * new V(0.48d)));
v_ctx_10[new V("lineTo")].Call(new V(0d), (v_size_11 * new V(0.52d)));
v_ctx_10[new V("stroke")].Call();
return default;
}
private static V F_drawPierce(V[] args) {
V v_ctx_12 = VisualJs.Arg(args,0);
V v_size_13 = VisualJs.Arg(args,1);
v_ctx_12[new V("beginPath")].Call();
v_ctx_12[new V("moveTo")].Call(((-v_size_13) * new V(1.05d)), new V(0d));
v_ctx_12[new V("lineTo")].Call((v_size_13 * new V(1.05d)), new V(0d));
v_ctx_12[new V("stroke")].Call();
v_ctx_12[new V("beginPath")].Call();
v_ctx_12[new V("moveTo")].Call((v_size_13 * new V(0.55d)), ((-v_size_13) * new V(0.32d)));
v_ctx_12[new V("lineTo")].Call((v_size_13 * new V(1.05d)), new V(0d));
v_ctx_12[new V("lineTo")].Call((v_size_13 * new V(0.55d)), (v_size_13 * new V(0.32d)));
v_ctx_12[new V("moveTo")].Call(((-v_size_13) * new V(0.55d)), ((-v_size_13) * new V(0.32d)));
v_ctx_12[new V("lineTo")].Call(((-v_size_13) * new V(1.05d)), new V(0d));
v_ctx_12[new V("lineTo")].Call(((-v_size_13) * new V(0.55d)), (v_size_13 * new V(0.32d)));
v_ctx_12[new V("stroke")].Call();
return default;
}
private static V F_drawRicochet(V[] args) {
V v_ctx_14 = VisualJs.Arg(args,0);
V v_size_15 = VisualJs.Arg(args,1);
v_ctx_14[new V("beginPath")].Call();
v_ctx_14[new V("moveTo")].Call(((-v_size_15) * new V(0.95d)), (v_size_15 * new V(0.12d)));
v_ctx_14[new V("lineTo")].Call(((-v_size_15) * new V(0.58d)), ((-v_size_15) * new V(0.62d)));
v_ctx_14[new V("lineTo")].Call((v_size_15 * new V(0.12d)), ((-v_size_15) * new V(0.44d)));
v_ctx_14[new V("lineTo")].Call((v_size_15 * new V(0.82d)), ((-v_size_15) * new V(0.84d)));
v_ctx_14[new V("lineTo")].Call((v_size_15 * new V(0.98d)), ((-new V(0.08d)) * v_size_15));
v_ctx_14[new V("stroke")].Call();
v_ctx_14[new V("beginPath")].Call();
v_ctx_14[new V("arc")].Call((v_size_15 * new V(0.84d)), ((-v_size_15) * new V(0.06d)), (v_size_15 * new V(0.18d)), new V(0d), Scope[new V("TAU")]);
v_ctx_14[new V("stroke")].Call();
return default;
}
private static V F_drawMultishot(V[] args) {
V v_ctx_16 = VisualJs.Arg(args,0);
V v_size_17 = VisualJs.Arg(args,1);
V v_strength_18 = VisualJs.Arg(args,2);
V v_lanes_19 = VisualJs.MathObject[new V("min")].Call(new V(4d), (new V(2d) + VisualJs.MathObject[new V("floor")].Call((v_strength_18 * new V(2d)))));
V v_spread_20 = (v_size_17 * new V(0.46d));
for (V v_i_21 = new V(0d); new V(v_i_21 < v_lanes_19).Truth; v_i_21++) {
V v_y_22 = (((v_i_21 - (((v_lanes_19 - new V(1d))) / new V(2d)))) * v_spread_20);
v_ctx_16[new V("beginPath")].Call();
v_ctx_16[new V("moveTo")].Call(((-v_size_17) * new V(0.92d)), (v_y_22 * new V(0.42d)));
v_ctx_16[new V("quadraticCurveTo")].Call(new V(0d), v_y_22, (v_size_17 * new V(0.95d)), (v_y_22 * new V(0.34d)));
v_ctx_16[new V("stroke")].Call();
}
return default;
}
private static V F_drawModifierGlyph(V[] args) {
V v_ctx_23 = VisualJs.Arg(args,0);
V v_kind_24 = VisualJs.Arg(args,1);
V v_angle_25 = VisualJs.Arg(args,2);
V v_radius_26 = VisualJs.Arg(args,3);
V v_value_27 = VisualJs.Arg(args,4);
V v_time_28 = VisualJs.Arg(args,5);
V v_scale_29 = VisualJs.Arg(args,6);
if (new V(v_value_27 <= new V(0d)).Truth) return default;
V v_strength_30 = VisualJs.MathObject[new V("min")].Call(new V(1d), (v_value_27 / new V(2d)));
V v_size_31 = VisualJs.MathObject[new V("max")].Call(new V(2.8d), ((new V(3.8d) * v_scale_29) + ((v_strength_30 * new V(1.25d)) * v_scale_29)));
V v_x_32 = (VisualJs.MathObject[new V("cos")].Call(v_angle_25) * v_radius_26);
V v_y_33 = ((VisualJs.MathObject[new V("sin")].Call(v_angle_25) * v_radius_26) * new V(0.86d));
V v_color_34 = VisualJs.Coalesce(Scope[new V("MODIFIER_COLOR")][(v_kind_24)], () => new V("#dff8ff"));
v_ctx_23[new V("save")].Call();
v_ctx_23[new V("translate")].Call(v_x_32, v_y_33);
v_ctx_23[new V("rotate")].Call(((v_angle_25 + (VisualJs.MathObject[new V("PI")] / new V(2d))) + (v_time_28 * new V(0.18d))));
VisualJs.Assign(v_ctx_23,new V("globalAlpha"),"=",(new V(0.56d) + (v_strength_30 * new V(0.25d))));
VisualJs.Assign(v_ctx_23,new V("strokeStyle"),"=",v_color_34);
VisualJs.Assign(v_ctx_23,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.85d), (new V(1.05d) * v_scale_29)));
VisualJs.Assign(v_ctx_23,new V("lineJoin"),"=",new V("round"));
VisualJs.Assign(v_ctx_23,new V("lineCap"),"=",new V("round"));
if (new V(v_kind_24 == new V("multishot")).Truth) Scope[new V("drawMultishot")].Call(v_ctx_23, v_size_31, v_strength_30); else if (new V(v_kind_24 == new V("pierce")).Truth) Scope[new V("drawPierce")].Call(v_ctx_23, v_size_31); else if (new V(v_kind_24 == new V("ricochet")).Truth) Scope[new V("drawRicochet")].Call(v_ctx_23, v_size_31); else if (new V(v_kind_24 == new V("fire")).Truth) Scope[new V("drawFlame")].Call(v_ctx_23, v_size_31); else if (new V(v_kind_24 == new V("freeze")).Truth) Scope[new V("drawFreeze")].Call(v_ctx_23, v_size_31); else Scope[new V("drawPoison")].Call(v_ctx_23, v_size_31);
if (new V(v_value_27 >= new V(2d)).Truth) {
VisualJs.Assign(v_ctx_23,new V("globalAlpha"),"=",(new V(0.34d) + (v_strength_30 * new V(0.22d))));
for (V v_i_35 = new V(0d); new V(v_i_35 < VisualJs.MathObject[new V("min")].Call(new V(3d), VisualJs.MathObject[new V("floor")].Call(v_value_27))).Truth; v_i_35++) {
v_ctx_23[new V("beginPath")].Call();
v_ctx_23[new V("arc")].Call((((-v_size_31) * new V(0.82d)) + ((v_i_35 * v_size_31) * new V(0.42d))), (v_size_31 * new V(1.28d)), VisualJs.MathObject[new V("max")].Call(new V(0.65d), (new V(0.8d) * v_scale_29)), new V(0d), Scope[new V("TAU")]);
v_ctx_23[new V("stroke")].Call();
}
}
v_ctx_23[new V("restore")].Call();
return default;
}
private static V F_renderSphereModifierVfx(V[] args) {
V v_ctx_36 = VisualJs.Arg(args,0);
V v_sphere_37 = VisualJs.Arg(args,1);
V v_player_38 = VisualJs.Arg(args,2);
V v_time_39 = VisualJs.Arg(args,3);
V v_scale_40 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
if (VisualJs.Or(VisualJs.Or((!v_ctx_36), () => (!v_sphere_37)), () => (!VisualJs.Chain(v_player_38,optional_41=>optional_41[new V("sphereMods")]))).Truth) return default;
V v_disabled_42 = VisualJs.Or(new V(v_sphere_37[new V("networkDisabledTimer")] > new V(0d)), () => (!v_sphere_37[new V("alive")]));
V v_authored_43 = Visual_sphereProgression.Scope[new V("getAuthoredSphereModifierLevels")].Call(v_player_38, v_sphere_37[new V("type")]);
V v_radius_44 = ((new V(24d) * v_scale_40) * new V(1.38d));
V v_activeTime_45 = (v_disabled_42.Truth ? new V(0d) : v_time_39);
v_ctx_36[new V("save")].Call();
v_ctx_36[new V("translate")].Call(v_sphere_37[new V("pos")][new V("x")], v_sphere_37[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_36,new V("globalCompositeOperation"),"=",new V("lighter"));
for (V v_i_46 = new V(0d); new V(v_i_46 < Scope[new V("MODIFIER_ORDER")][new V("length")]).Truth; v_i_46++) {
V v_kind_47 = Scope[new V("MODIFIER_ORDER")][v_i_46];
if (VisualJs.And(Scope[new V("PROJECTILE_ONLY")][new V("includes")].Call(v_kind_47), () => (!Visual_gameData.Scope[new V("sphereUsesProjectileModifiers")].Call(v_sphere_37[new V("type")]))).Truth) continue;
V v_value_48 = VisualJs.MathObject[new V("max")].Call(Scope[new V("modifierValue")].Call(v_player_38[new V("sphereMods")], v_kind_47), VisualJs.Number(VisualJs.Coalesce(v_authored_43[v_kind_47], () => new V(0d))));
V v_angle_49 = ((((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + ((v_i_46 * Scope[new V("TAU")]) / Scope[new V("MODIFIER_ORDER")][new V("length")])) + (v_activeTime_45 * new V(0.11d)));
if (v_disabled_42.Truth) VisualJs.Assign(v_ctx_36,new V("globalAlpha"),"=",new V(0.38d));
Scope[new V("drawModifierGlyph")].Call(v_ctx_36, v_kind_47, v_angle_49, v_radius_44, v_value_48, v_activeTime_45, v_scale_40);
}
v_ctx_36[new V("restore")].Call();
return default;
}
}
}
#endif
