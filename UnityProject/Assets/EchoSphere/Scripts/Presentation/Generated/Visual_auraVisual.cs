// Generated from src/spheres/auraVisual.ts; sha256 397363af140f3e82fff9c26b20517d12ad013c919fca687cf80e3b5779c90b51.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_auraVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_auraVisual() {
Scope[new V("drawFieldBlade")] = VisualJs.Fn(F_drawFieldBlade);
Scope[new V("renderAuraSphereRuntimeVfx")] = VisualJs.Fn(F_renderAuraSphereRuntimeVfx);
}
private static V F_drawFieldBlade(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_angle_2 = VisualJs.Arg(args,2);
V v_color_3 = VisualJs.Arg(args,3);
V v_alpha_4 = VisualJs.Arg(args,4);
V v_p_5 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_1, v_angle_2, new V(0.34d), new V(1.08d));
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call(v_p_5[new V("x")], v_p_5[new V("y")]);
v_ctx_0[new V("rotate")].Call((v_angle_2 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_alpha_4 * v_p_5[new V("depth")]));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_3);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(0.9d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(((-v_r_1) * new V(0.1d)), new V(0d));
v_ctx_0[new V("lineTo")].Call(new V(0d), ((-v_r_1) * new V(0.14d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.11d)), new V(0d));
v_ctx_0[new V("lineTo")].Call(new V(0d), (v_r_1 * new V(0.14d)));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderAuraSphereRuntimeVfx(V[] args) {
V v_ctx_6 = VisualJs.Arg(args,0);
V v_sphere_7 = VisualJs.Arg(args,1);
V v_player_8 = VisualJs.Arg(args,2);
V v_time_9 = VisualJs.Arg(args,3);
V v_scale_10 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_11 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_7, v_player_8, new V("#57e6b4"), v_time_9);
V v_r_12 = (new V(24d) * v_scale_10);
V v_coreR_13 = v_r_12;
v_ctx_6[new V("save")].Call();
v_ctx_6[new V("translate")].Call(v_sphere_7[new V("pos")][new V("x")], v_sphere_7[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_6, (v_r_12 * new V(2.45d)), v_st_11[new V("color")], (v_st_11[new V("disabled")].Truth ? new V(0.05d) : new V(0.12d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_6, (v_r_12 * new V(1.03d)), v_st_11[new V("color")], (v_time_9 * new V(0.14d)), (v_st_11[new V("disabled")].Truth ? new V(0.12d) : new V(0.68d)), new V("aura"), v_st_11[new V("pulse")]);
VisualJs.Assign(v_ctx_6,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_6, v_coreR_13, v_st_11[new V("color")], v_st_11[new V("pulse")]);
for (V v_i_14 = new V(0d); new V(v_i_14 < new V(6d)).Truth; v_i_14++) {
Scope[new V("drawFieldBlade")].Call(v_ctx_6, v_r_12, (((v_i_14 * VisualJs.MathObject[new V("PI")]) / new V(3d)) + (v_time_9 * new V(0.1d))), v_st_11[new V("color")], (v_st_11[new V("disabled")].Truth ? new V(0.07d) : new V(0.48d)));
}
V v_q_15 = (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call((v_time_9 * new V(2.7d)))));
VisualJs.Assign(v_ctx_6,new V("globalAlpha"),"=",(v_st_11[new V("disabled")].Truth ? new V(0.05d) : (new V(0.16d) + (v_q_15 * new V(0.08d)))));
VisualJs.Assign(v_ctx_6,new V("strokeStyle"),"=",v_st_11[new V("color")]);
VisualJs.Assign(v_ctx_6,new V("lineWidth"),"=",new V(1d));
v_ctx_6[new V("beginPath")].Call();
v_ctx_6[new V("arc")].Call(new V(0d), new V(0d), (v_r_12 * ((new V(1.04d) + (v_q_15 * new V(0.06d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_6[new V("stroke")].Call();
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_6, v_r_12, v_st_11[new V("color")], v_st_11[new V("disabled")]);
v_ctx_6[new V("restore")].Call();
return default;
}
}
}
