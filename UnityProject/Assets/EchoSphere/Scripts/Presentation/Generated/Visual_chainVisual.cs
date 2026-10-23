#if UNITY_EDITOR
// Generated from src/spheres/chainVisual.ts; sha256 d9958a454ad932d84d432f58770aef490b1714b481f29411af091e068eedd716.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_chainVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_chainVisual() {
Scope[new V("drawConductor")] = VisualJs.Fn(F_drawConductor);
Scope[new V("renderChainSphereRuntimeVfx")] = VisualJs.Fn(F_renderChainSphereRuntimeVfx);
}
private static V F_drawConductor(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_angle_2 = VisualJs.Arg(args,2);
V v_color_3 = VisualJs.Arg(args,3);
V v_alpha_4 = VisualJs.Arg(args,4);
V v_p_5 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_1, v_angle_2, new V(0.34d), new V(1.1d));
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call(v_p_5[new V("x")], v_p_5[new V("y")]);
v_ctx_0[new V("rotate")].Call((v_angle_2 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_alpha_4 * v_p_5[new V("depth")]));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_3);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_r_1 * new V(0.032d))));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(((-v_r_1) * new V(0.08d)), new V(0d));
v_ctx_0[new V("lineTo")].Call(((-v_r_1) * new V(0.01d)), ((-v_r_1) * new V(0.07d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.07d)), (v_r_1 * new V(0.03d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.14d)), ((-v_r_1) * new V(0.02d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.21d)), (v_r_1 * new V(0.06d)));
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderChainSphereRuntimeVfx(V[] args) {
V v_ctx_6 = VisualJs.Arg(args,0);
V v_sphere_7 = VisualJs.Arg(args,1);
V v_player_8 = VisualJs.Arg(args,2);
V v_time_9 = VisualJs.Arg(args,3);
V v_scale_10 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_11 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_7, v_player_8, new V("#ffe25b"), v_time_9);
V v_r_12 = (new V(24d) * v_scale_10);
V v_coreR_13 = v_r_12;
v_ctx_6[new V("save")].Call();
v_ctx_6[new V("translate")].Call(v_sphere_7[new V("pos")][new V("x")], v_sphere_7[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_6, (v_r_12 * new V(2.35d)), v_st_11[new V("color")], (v_st_11[new V("disabled")].Truth ? new V(0.05d) : new V(0.11d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_6, (v_r_12 * new V(1.02d)), v_st_11[new V("color")], (v_time_9 * new V(0.17d)), (v_st_11[new V("disabled")].Truth ? new V(0.12d) : new V(0.78d)), new V("lightning"), v_st_11[new V("pulse")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_6, v_coreR_13, v_st_11[new V("color")], v_st_11[new V("pulse")]);
for (V v_i_14 = new V(0d); new V(v_i_14 < new V(4d)).Truth; v_i_14++) {
Scope[new V("drawConductor")].Call(v_ctx_6, v_r_12, (((v_i_14 * VisualJs.MathObject[new V("PI")]) / new V(2d)) + (v_time_9 * new V(0.28d))), v_st_11[new V("color")], (v_st_11[new V("disabled")].Truth ? new V(0.1d) : new V(0.56d)));
}
V v_surge_15 = (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call((v_time_9 * new V(10d)))));
VisualJs.Assign(v_ctx_6,new V("globalAlpha"),"=",(v_st_11[new V("disabled")].Truth ? new V(0.06d) : (new V(0.2d) + (v_surge_15 * new V(0.16d)))));
VisualJs.Assign(v_ctx_6,new V("strokeStyle"),"=",v_st_11[new V("color")]);
VisualJs.Assign(v_ctx_6,new V("lineWidth"),"=",new V(1d));
v_ctx_6[new V("beginPath")].Call();
v_ctx_6[new V("arc")].Call(new V(0d), new V(0d), (v_coreR_13 * ((new V(1.04d) + (v_surge_15 * new V(0.08d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_6[new V("stroke")].Call();
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_6, v_r_12, v_st_11[new V("color")], v_st_11[new V("disabled")]);
v_ctx_6[new V("restore")].Call();
return default;
}
}
}
#endif
