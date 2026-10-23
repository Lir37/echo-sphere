#if UNITY_EDITOR
// Generated from src/spheres/voidVisual.ts; sha256 f08f03434a25ec804f07608e518ecfadcb0cfc169059392469c6f567811ac142.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_voidVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_voidVisual() {
Scope[new V("drawVoidEmitter")] = VisualJs.Fn(F_drawVoidEmitter);
Scope[new V("renderVoidSphereRuntimeVfx")] = VisualJs.Fn(F_renderVoidSphereRuntimeVfx);
}
private static V F_drawVoidEmitter(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_color_2 = VisualJs.Arg(args,2);
V v_disabled_3 = VisualJs.Arg(args,3);
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call((v_r_1 * new V(1.2d)), new V(0d));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.12d) : new V(0.94d)));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#03070e"));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_2);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(1.1d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(((-v_r_1) * new V(0.01d)), ((-v_r_1) * new V(0.19d)));
v_ctx_0[new V("quadraticCurveTo")].Call((v_r_1 * new V(0.21d)), ((-v_r_1) * new V(0.08d)), (v_r_1 * new V(0.27d)), new V(0d));
v_ctx_0[new V("quadraticCurveTo")].Call((v_r_1 * new V(0.21d)), (v_r_1 * new V(0.08d)), ((-v_r_1) * new V(0.01d)), (v_r_1 * new V(0.19d)));
v_ctx_0[new V("quadraticCurveTo")].Call((v_r_1 * new V(0.07d)), new V(0d), ((-v_r_1) * new V(0.01d)), ((-v_r_1) * new V(0.19d)));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.06d) : new V(0.65d)));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",new V("#eadfff"));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("arc")].Call((v_r_1 * new V(0.18d)), new V(0d), (v_r_1 * new V(0.07d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderVoidSphereRuntimeVfx(V[] args) {
V v_ctx_4 = VisualJs.Arg(args,0);
V v_sphere_5 = VisualJs.Arg(args,1);
V v_player_6 = VisualJs.Arg(args,2);
V v_time_7 = VisualJs.Arg(args,3);
V v_scale_8 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_9 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_5, v_player_6, new V("#c28cff"), v_time_7);
V v_r_10 = (new V(24d) * v_scale_8);
V v_coreR_11 = v_r_10;
v_ctx_4[new V("save")].Call();
v_ctx_4[new V("translate")].Call(v_sphere_5[new V("pos")][new V("x")], v_sphere_5[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_4,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_4, (v_r_10 * new V(2.35d)), v_st_9[new V("color")], (v_st_9[new V("disabled")].Truth ? new V(0.04d) : new V(0.1d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_4, (v_r_10 * new V(1.03d)), v_st_9[new V("color")], (v_time_7 * new V(0.11d)), (v_st_9[new V("disabled")].Truth ? new V(0.1d) : new V(0.58d)), new V("void"), v_st_9[new V("pulse")]);
VisualJs.Assign(v_ctx_4,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_4, v_coreR_11, v_st_9[new V("color")], (v_st_9[new V("pulse")] * new V(0.92d)));
v_ctx_4[new V("rotate")].Call(VisualJs.Or(v_sphere_5[new V("rotation")], () => new V(0d)));
Scope[new V("drawVoidEmitter")].Call(v_ctx_4, v_r_10, v_st_9[new V("color")], v_st_9[new V("disabled")]);
for (V v_i_12 = new V(0d); new V(v_i_12 < new V(4d)).Truth; v_i_12++) {
V v_a_13 = (((-v_time_7) * ((new V(0.22d) + (v_i_12 * new V(0.025d))))) + ((v_i_12 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_p_14 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_10, v_a_13, new V(0.34d), new V(1.11d));
v_ctx_4[new V("save")].Call();
v_ctx_4[new V("translate")].Call(v_p_14[new V("x")], v_p_14[new V("y")]);
VisualJs.Assign(v_ctx_4,new V("globalAlpha"),"=",(v_st_9[new V("disabled")].Truth ? new V(0.05d) : (new V(0.32d) * v_p_14[new V("depth")])));
VisualJs.Assign(v_ctx_4,new V("strokeStyle"),"=",v_st_9[new V("color")]);
VisualJs.Assign(v_ctx_4,new V("lineWidth"),"=",new V(0.9d));
v_ctx_4[new V("beginPath")].Call();
v_ctx_4[new V("arc")].Call(new V(0d), new V(0d), (v_r_10 * ((new V(0.07d) + (v_i_12 * new V(0.012d))))), (VisualJs.MathObject[new V("PI")] * new V(0.2d)), (VisualJs.MathObject[new V("PI")] * new V(1.8d)));
v_ctx_4[new V("stroke")].Call();
v_ctx_4[new V("restore")].Call();
}
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_4, v_r_10, v_st_9[new V("color")], v_st_9[new V("disabled")]);
v_ctx_4[new V("restore")].Call();
return default;
}
}
}
#endif
