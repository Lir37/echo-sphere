// Generated from src/spheres/prismVisual.ts; sha256 f596abdee3d79802a2692ae220ca109285ff9f966db606a6cf1cda6cf93539c3.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_prismVisual {
public static readonly V Scope=VisualJs.Obj();
static Visual_prismVisual() {
Scope[new V("drawPrismEmitter")] = VisualJs.Fn(F_drawPrismEmitter);
Scope[new V("renderPrismSphereRuntimeVfx")] = VisualJs.Fn(F_renderPrismSphereRuntimeVfx);
}
private static V F_drawPrismEmitter(V[] args) {
V v_ctx_0 = VisualJs.Arg(args,0);
V v_r_1 = VisualJs.Arg(args,1);
V v_color_2 = VisualJs.Arg(args,2);
V v_disabled_3 = VisualJs.Arg(args,3);
v_ctx_0[new V("save")].Call();
v_ctx_0[new V("translate")].Call((v_r_1 * new V(1.18d)), new V(0d));
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.12d) : new V(0.94d)));
VisualJs.Assign(v_ctx_0,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",v_color_2);
VisualJs.Assign(v_ctx_0,new V("lineWidth"),"=",new V(1.05d));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call(new V(0d), ((-v_r_1) * new V(0.2d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.17d)), ((-v_r_1) * new V(0.06d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.27d)), new V(0d));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.17d)), (v_r_1 * new V(0.06d)));
v_ctx_0[new V("lineTo")].Call(new V(0d), (v_r_1 * new V(0.2d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.08d)), new V(0d));
v_ctx_0[new V("closePath")].Call();
v_ctx_0[new V("fill")].Call();
v_ctx_0[new V("stroke")].Call();
VisualJs.Assign(v_ctx_0,new V("globalAlpha"),"=",(v_disabled_3.Truth ? new V(0.06d) : new V(0.68d)));
VisualJs.Assign(v_ctx_0,new V("strokeStyle"),"=",new V("#ffffff"));
v_ctx_0[new V("beginPath")].Call();
v_ctx_0[new V("moveTo")].Call((v_r_1 * new V(0.09d)), ((-v_r_1) * new V(0.1d)));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.21d)), new V(0d));
v_ctx_0[new V("lineTo")].Call((v_r_1 * new V(0.09d)), (v_r_1 * new V(0.1d)));
v_ctx_0[new V("stroke")].Call();
v_ctx_0[new V("restore")].Call();
return default;
}
private static V F_renderPrismSphereRuntimeVfx(V[] args) {
V v_ctx_4 = VisualJs.Arg(args,0);
V v_sphere_5 = VisualJs.Arg(args,1);
V v_player_6 = VisualJs.Arg(args,2);
V v_time_7 = VisualJs.Arg(args,3);
V v_scale_8 = VisualJs.Arg(args,4).Undefined?new V(1d):VisualJs.Arg(args,4);
V v_st_9 = Visual_visualHelpers.Scope[new V("stateColor")].Call(v_sphere_5, v_player_6, new V("#ff8de1"), v_time_7);
V v_branch_10 = VisualJs.Chain(v_player_6,optional_11=>VisualJs.Chain(optional_11[new V("sphereBranches")],optional_12=>optional_12[new V("prism")]));
V v_finalId_13 = (VisualJs.Or(VisualJs.Chain(v_player_6,optional_14=>optional_14[new V("evolutions")]), () => VisualJs.Arr()))[new V("find")].Call(VisualJs.Fn(args_15 => { V v_id_16 = VisualJs.Arg(args_15, 0);
return v_id_16[new V("startsWith")].Call(new V("sphere:prism:7:")); }));
V v_finalIndex_17 = (v_finalId_13.Truth ? VisualJs.Number(v_finalId_13[new V("split")].Call(new V(":"))[new V("pop")].Call()) : VisualJs.Null);
V v_spectrumElement_18 = (new V(v_branch_10 == new V("prism_spectrum")).Truth ? (new V(v_finalIndex_17 == new V(1d)).Truth ? new V("freeze") : (new V(v_finalIndex_17 == new V(2d)).Truth ? new V("poison") : new V("fire"))) : VisualJs.Null);
V v_elementColor_19 = (v_spectrumElement_18.Truth ? Visual_sphereProgression.Scope[new V("SPHERE_ELEMENT_META")][v_spectrumElement_18][new V("color")] : VisualJs.Null);
V v_visualColor_20 = (VisualJs.And(VisualJs.And(v_elementColor_19, () => (!v_st_9[new V("resonance")])), () => (!v_st_9[new V("disabled")])).Truth ? v_elementColor_19 : v_st_9[new V("color")]);
V v_r_21 = (new V(24d) * v_scale_8);
V v_coreR_22 = v_r_21;
v_ctx_4[new V("save")].Call();
v_ctx_4[new V("translate")].Call(v_sphere_5[new V("pos")][new V("x")], v_sphere_5[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_4,new V("globalCompositeOperation"),"=",new V("lighter"));
Visual_visualHelpers.Scope[new V("glow")].Call(v_ctx_4, (v_r_21 * new V(2.4d)), v_visualColor_20, (v_st_9[new V("disabled")].Truth ? new V(0.05d) : new V(0.12d)));
Visual_visualHelpers.Scope[new V("drawSphereOrbit")].Call(v_ctx_4, (v_r_21 * new V(1.03d)), v_visualColor_20, (v_time_7 * new V(0.16d)), (v_st_9[new V("disabled")].Truth ? new V(0.12d) : new V(0.66d)), new V("prism"), v_st_9[new V("pulse")]);
VisualJs.Assign(v_ctx_4,new V("globalCompositeOperation"),"=",new V("source-over"));
Visual_visualHelpers.Scope[new V("core")].Call(v_ctx_4, v_coreR_22, v_visualColor_20, v_st_9[new V("pulse")]);
v_ctx_4[new V("rotate")].Call(VisualJs.Or(v_sphere_5[new V("rotation")], () => new V(0d)));
Scope[new V("drawPrismEmitter")].Call(v_ctx_4, v_r_21, v_visualColor_20, v_st_9[new V("disabled")]);
for (V v_i_23 = new V(0d); new V(v_i_23 < new V(2d)).Truth; v_i_23++) {
V v_a_24 = ((v_time_7 * ((new V(0.3d) + (v_i_23 * new V(0.05d))))) + (v_i_23 * VisualJs.MathObject[new V("PI")]));
V v_p_25 = Visual_visualHelpers.Scope[new V("orbitPoint")].Call(v_r_21, v_a_24, new V(0.34d), new V(1.1d));
v_ctx_4[new V("save")].Call();
v_ctx_4[new V("translate")].Call(v_p_25[new V("x")], v_p_25[new V("y")]);
v_ctx_4[new V("rotate")].Call((v_a_24 + (VisualJs.MathObject[new V("PI")] / new V(2d))));
VisualJs.Assign(v_ctx_4,new V("globalAlpha"),"=",(v_st_9[new V("disabled")].Truth ? new V(0.06d) : (new V(0.42d) * v_p_25[new V("depth")])));
VisualJs.Assign(v_ctx_4,new V("fillStyle"),"=",new V("#07111d"));
VisualJs.Assign(v_ctx_4,new V("strokeStyle"),"=",v_visualColor_20);
VisualJs.Assign(v_ctx_4,new V("lineWidth"),"=",new V(0.85d));
v_ctx_4[new V("beginPath")].Call();
v_ctx_4[new V("moveTo")].Call(new V(0d), ((-v_r_21) * new V(0.09d)));
v_ctx_4[new V("lineTo")].Call((v_r_21 * new V(0.07d)), new V(0d));
v_ctx_4[new V("lineTo")].Call(new V(0d), (v_r_21 * new V(0.09d)));
v_ctx_4[new V("lineTo")].Call(((-v_r_21) * new V(0.07d)), new V(0d));
v_ctx_4[new V("closePath")].Call();
v_ctx_4[new V("fill")].Call();
v_ctx_4[new V("stroke")].Call();
v_ctx_4[new V("restore")].Call();
}
Visual_visualHelpers.Scope[new V("finishDisabled")].Call(v_ctx_4, v_r_21, v_visualColor_20, v_st_9[new V("disabled")]);
v_ctx_4[new V("restore")].Call();
return default;
}
}
}
