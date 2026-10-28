#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
// Generated from src/spheres/pulseVisual.ts; sha256 8eec4eeb885b50f2912e49c4a502bed3005978569ca8751a9f10aaba855c5337.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_pulseVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_pulseVisual() {
Scope[new V("renderPulseSphereRuntimeVfx")] = VisualJs.Fn(F_renderPulseSphereRuntimeVfx);
}
private static V F_renderPulseSphereRuntimeVfx(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_sphere_1 = VisualJs.Arg(args,1);
V v_player_2 = VisualJs.Arg(args,2);
V v_time_3 = VisualJs.Arg(args,3);
V v_scale_4 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_5 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_1, v_player_2, new V("#ffd35a"), v_time_3);
V v_r_6 = (new V(24d) * v_scale_4);
V v_coreR_7 = v_r_6;
V v_phase_8 = (v_st_5[new V("animationTime")] * new V(1.1d));
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call(v_sphere_1[new V("pos")][new V("x")], v_sphere_1[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_0,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_0, (v_r_6 * new V(2.45d)), v_st_5[new V("color")], (v_st_5[new V("disabled")].Truth ? new V(0.05d) : new V(0.12d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_0, (v_r_6 * new V(1.03d)), v_st_5[new V("color")], (v_phase_8 * new V(0.13d)), (v_st_5[new V("disabled")].Truth ? new V(0.1d) : new V(0.66d)), new V("pulse"), v_st_5[new V("pulse")]);
VisualJs.Assign(v_ctx_0,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_0, v_coreR_7, v_st_5[new V("color")], v_st_5[new V("pulse")]);
for (V v_i_9 = new V(0d); new V(v_i_9 < new V(4d)).Truth; v_i_9++) {
V v_a_10 = ((v_phase_8 * new V(0.28d)) + ((v_i_9 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_p_11 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_6, v_a_10, new V(0.34d), new V(1.1d));
V v_q_12 = (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call(((v_phase_8 * new V(2.8d)) + v_i_9))));
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call(v_p_11[new V("x")], v_p_11[new V("y")]);
v_ctx_0[new V("rotate")].Call((v_a_10 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_st_5[new V("disabled")].Truth ? new V(0.06d) : (((new V(0.36d) + (v_q_12 * new V(0.2d)))) * v_p_11[new V("depth")])));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_st_5[new V("color")]);
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(0.8d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(((-v_r_6) * new V(0.08d)), ((-v_r_6) * new V(0.1d)));
v_ctx_0[new V("lineTo")].Call((v_r_6 * new V(0.08d)), ((-v_r_6) * new V(0.06d)));
v_ctx_0[new V("lineTo")].Call((v_r_6 * new V(0.08d)), (v_r_6 * new V(0.06d)));
v_ctx_0[new V("lineTo")].Call(((-v_r_6) * new V(0.08d)), (v_r_6 * new V(0.1d)));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
}
V v_q_13 = ((((((v_phase_8 * new V(0.42d))) % new V(1d)) + new V(1d))) % new V(1d));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_st_5[new V("disabled")].Truth ? new V(0.06d) : (((new V(1d) - v_q_13)) * new V(0.36d))));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_st_5[new V("color")]);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(1d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("arc")].Call(new V(0d), new V(0d), (v_r_6 * ((new V(1.02d) + (v_q_13 * new V(0.86d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_0[new V("stroke")].Call();
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_0, v_r_6, v_st_5[new V("color")], v_st_5[new V("disabled")]);
v_ctx_0[new V("restore")].Call();
return default;
}
}
}
#endif
