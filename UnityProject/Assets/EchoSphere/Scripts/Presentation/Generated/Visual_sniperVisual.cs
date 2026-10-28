#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
// Generated from src/spheres/sniperVisual.ts; sha256 0f9d191bdeac6be5a3403bbece3d3f6e904ad78a8dd69257dec8e6436533f86a.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_sniperVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_sniperVisual() {
Scope[new V("drawSniperTip")] = VisualJs.Fn(F_drawSniperTip);
Scope[new V("renderSniperSphereRuntimeVfx")] = VisualJs.Fn(F_renderSniperSphereRuntimeVfx);
}
private static V F_drawSniperTip(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_color_2 = VisualJs.Arg(args,2);
V v_disabled_3 = VisualJs.Arg(args,3);
V v_time_4 = VisualJs.Arg(args,4);
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call((v_r_1 * new V(1.22d)), new V(0d));
v_ctx_0[new V("rotate")].Call(((-VisualJs.MathObject[new V("PI")]) / new V(2d)));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.14d) : new V(0.94d)));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_2);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(1.15d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(new V(0d), ((-v_r_1) * new V(0.24d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.18d)), ((-v_r_1) * new V(0.05d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.13d)), (v_r_1 * new V(0.24d)));
v_ctx_0[new V("lineTo")].Call(new V(0d), (v_r_1 * new V(0.34d)));
v_ctx_0[new V("lineTo")].Call(((-v_r_1) * new V(0.13d)), (v_r_1 * new V(0.24d)));
v_ctx_0[new V("lineTo")].Call(((-v_r_1) * new V(0.18d)), ((-v_r_1) * new V(0.05d)));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.08d) : new V(0.7d)));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",new V("#ffffff"));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(new V(0d), ((-v_r_1) * new V(0.18d)));
v_ctx_0[new V("lineTo")].Call(new V(0d), (v_r_1 * new V(0.16d)));
v_ctx_0[new V("moveTo")].Call(((-v_r_1) * new V(0.1d)), ((-v_r_1) * new V(0.02d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.1d)), ((-v_r_1) * new V(0.02d)));
v_ctx_0[new V("stroke")].Call();
V v_scan_5 = (new V(0.1d) + (new V(0.05d) * VisualJs.MathObject[new V("sin")].Call((v_time_4 * new V(7d)))));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.04d) : v_scan_5));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",v_color_2);
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("arc")].Call(new V(0d), new V(0d), (v_r_1 * new V(0.13d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderSniperSphereRuntimeVfx(V[] args) {
V v_ctx_6 = VisualJs.Arg(args,0);
V v_sphere_7 = VisualJs.Arg(args,1);
V v_player_8 = VisualJs.Arg(args,2);
V v_time_9 = VisualJs.Arg(args,3);
V v_scale_10 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_11 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_7, v_player_8, new V("#e86cff"), v_time_9);
V v_r_12 = (new V(24d) * v_scale_10);
V v_coreR_13 = v_r_12;
v_ctx_6[new V("save")].Call();
v_ctx_6[new V("translate")].Call(v_sphere_7[new V("pos")][new V("x")], v_sphere_7[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_6, (v_r_12 * new V(2.35d)), v_st_11[new V("color")], (v_st_11[new V("disabled")].Truth ? new V(0.05d) : new V(0.11d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_6, (v_r_12 * new V(1.02d)), v_st_11[new V("color")], (v_time_9 * new V(0.2d)), (v_st_11[new V("disabled")].Truth ? new V(0.16d) : new V(0.68d)), new V("sniper"), v_st_11[new V("pulse")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_6, v_coreR_13, v_st_11[new V("color")], v_st_11[new V("pulse")]);
v_ctx_6[new V("rotate")].Call(VisualJs.Or(v_sphere_7[new V("rotation")], () => new V(0d)));
Scope[new V("drawSniperTip")].Call(v_ctx_6, v_r_12, v_st_11[new V("color")], v_st_11[new V("disabled")], v_time_9);
V v_a_14 = ((v_time_9 * new V(0.46d)) + (VisualJs.MathObject[new V("PI")] * new V(0.72d)));
V v_p_15 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_12, v_a_14, new V(0.34d), new V(1.15d));
v_ctx_6[new V("save")].Call();
v_ctx_6[new V("translate")].Call(v_p_15[new V("x")], v_p_15[new V("y")]);
VisualJs.Assign(v_ctx_6,new V("globalAlpha"),"=",(v_st_11[new V("disabled")].Truth ? new V(0.08d) : (new V(0.62d) * v_p_15[new V("depth")])));
VisualJs.Assign(v_ctx_6,new V("strokeStyle"),"=",v_st_11[new V("color")]);
VisualJs.Assign(v_ctx_6,new V("lineWidth"),"=",new V(0.9d));
v_ctx_6[new V("beginPath")].Call();
v_ctx_6[new V("arc")].Call(new V(0d), new V(0d), (v_r_12 * new V(0.1d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_6[new V("moveTo")].Call(((-v_r_12) * new V(0.18d)), new V(0d));
v_ctx_6[new V("lineTo")].Call((v_r_12 * new V(0.18d)), new V(0d));
v_ctx_6[new V("moveTo")].Call(new V(0d), ((-v_r_12) * new V(0.18d)));
v_ctx_6[new V("lineTo")].Call(new V(0d), (v_r_12 * new V(0.18d)));
v_ctx_6[new V("stroke")].Call();
v_ctx_6[new V("restore")].Call();
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_6, v_r_12, v_st_11[new V("color")], v_st_11[new V("disabled")]);
v_ctx_6[new V("restore")].Call();
return default;
}
}
}
#endif
