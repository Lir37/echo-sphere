// Generated from src/spheres/shotgunVisual.ts; sha256 74371764d179af03329a80d554a560e1d71cb8fe7af85b5dbf318d7cf90c5a6c.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_shotgunVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_shotgunVisual() {
Scope[new V("drawShotgunTip")] = VisualJs.Fn(F_drawShotgunTip);
Scope[new V("renderShotgunSphereRuntimeVfx")] = VisualJs.Fn(F_renderShotgunSphereRuntimeVfx);
}
private static V F_drawShotgunTip(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_y_2 = VisualJs.Arg(args,2);
V v_color_3 = VisualJs.Arg(args,3);
V v_disabled_4 = VisualJs.Arg(args,4);
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call((v_r_1 * new V(1.18d)), v_y_2);
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_4.Truth ? new V(0.12d) : new V(0.92d)));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_3);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(1.1d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(((-v_r_1) * new V(0.02d)), ((-v_r_1) * new V(0.1d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.22d)), ((-v_r_1) * new V(0.11d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.3d)), new V(0d));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.22d)), (v_r_1 * new V(0.11d)));
v_ctx_0[new V("lineTo")].Call(((-v_r_1) * new V(0.02d)), (v_r_1 * new V(0.1d)));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_4.Truth ? new V(0.07d) : new V(0.72d)));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",new V("#ffffff"));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("arc")].Call((v_r_1 * new V(0.25d)), new V(0d), (v_r_1 * new V(0.055d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderShotgunSphereRuntimeVfx(V[] args) {
V v_ctx_5 = VisualJs.Arg(args,0);
V v_sphere_6 = VisualJs.Arg(args,1);
V v_player_7 = VisualJs.Arg(args,2);
V v_time_8 = VisualJs.Arg(args,3);
V v_scale_9 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_10 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_6, v_player_7, new V("#ff8f3d"), v_time_8);
V v_r_11 = (new V(24d) * v_scale_9);
V v_coreR_12 = v_r_11;
v_ctx_5[new V("save")].Call();
v_ctx_5[new V("translate")].Call(v_sphere_6[new V("pos")][new V("x")], v_sphere_6[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_5,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_5, (v_r_11 * new V(2.4d)), v_st_10[new V("color")], (v_st_10[new V("disabled")].Truth ? new V(0.05d) : new V(0.12d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_5, (v_r_11 * new V(1.02d)), v_st_10[new V("color")], (v_time_8 * new V(0.18d)), (v_st_10[new V("disabled")].Truth ? new V(0.15d) : new V(0.64d)), new V("shotgun"), v_st_10[new V("pulse")]);
VisualJs.Assign(v_ctx_5,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_5, v_coreR_12, v_st_10[new V("color")], v_st_10[new V("pulse")]);
v_ctx_5[new V("rotate")].Call(VisualJs.Or(v_sphere_6[new V("rotation")], () => new V(0d)));
Scope[new V("drawShotgunTip")].Call(v_ctx_5, v_r_11, ((-v_r_11) * new V(0.25d)), v_st_10[new V("color")], v_st_10[new V("disabled")]);
Scope[new V("drawShotgunTip")].Call(v_ctx_5, v_r_11, new V(0d), v_st_10[new V("color")], v_st_10[new V("disabled")]);
Scope[new V("drawShotgunTip")].Call(v_ctx_5, v_r_11, (v_r_11 * new V(0.25d)), v_st_10[new V("color")], v_st_10[new V("disabled")]);
for (V v_i_13 = new V(0d); new V(v_i_13 < new V(3d)).Truth; v_i_13++) {
V v_a_14 = ((v_time_8 * ((new V(0.34d) + (v_i_13 * new V(0.035d))))) + (((v_i_13 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
V v_p_15 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_11, v_a_14, new V(0.34d), new V(1.12d));
v_ctx_5[new V("save")].Call();
v_ctx_5[new V("translate")].Call(v_p_15[new V("x")], v_p_15[new V("y")]);
VisualJs.Assign(v_ctx_5,new V("globalAlpha"),"=",(v_st_10[new V("disabled")].Truth ? new V(0.08d) : (new V(0.48d) * v_p_15[new V("depth")])));
VisualJs.Assign(v_ctx_5,new V("strokeStyle"),"=",v_st_10[new V("color")]);
VisualJs.Assign(v_ctx_5,new V("lineWidth"),"=",new V(0.8d));
v_ctx_5[new V("beginPath")].Call();
v_ctx_5[new V("moveTo")].Call(((-v_r_11) * new V(0.1d)), new V(0d));
v_ctx_5[new V("lineTo")].Call((v_r_11 * new V(0.1d)), new V(0d));
v_ctx_5[new V("stroke")].Call();
v_ctx_5[new V("restore")].Call();
}
if (new V(VisualJs.MathObject[new V("sin")].Call((v_time_8 * new V(8d))) > new V(0.6d)).Truth) {
VisualJs.Assign(v_ctx_5,new V("globalAlpha"),"=",(v_st_10[new V("disabled")].Truth ? new V(0.05d) : new V(0.22d)));
VisualJs.Assign(v_ctx_5,new V("strokeStyle"),"=",v_st_10[new V("color")]);
VisualJs.Assign(v_ctx_5,new V("lineWidth"),"=",new V(0.9d));
v_ctx_5[new V("beginPath")].Call();
v_ctx_5[new V("arc")].Call((v_r_11 * new V(1.43d)), new V(0d), (v_r_11 * new V(0.11d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_5[new V("stroke")].Call();
}
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_5, v_r_11, v_st_10[new V("color")], v_st_10[new V("disabled")]);
v_ctx_5[new V("restore")].Call();
return default;
}
}
}
