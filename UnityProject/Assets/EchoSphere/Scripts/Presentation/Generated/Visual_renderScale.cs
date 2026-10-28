#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
// Generated from src/renderScale.ts; sha256 485ba817b4abe2101a849d138cf2b170725ac0b7b582dee2848fbe5b42d49a2d.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_renderScale {
public static readonly V Scope=VisualJs.Obj();
static Visual_renderScale() {
Scope[new V("clampDpr")] = VisualJs.Fn(F_clampDpr);
Scope[new V("calculateBackingSize")] = VisualJs.Fn(F_calculateBackingSize);
Scope[new V("cssPointToWorld")] = VisualJs.Fn(F_cssPointToWorld);
Scope[new V("DPR_CAP")] = new V(1.75d);
}
private static V F_clampDpr(V[] args) {
V v_value_0 = VisualJs.Arg(args,0);
return VisualJs.MathObject[new V("min")].Call(Scope[new V("DPR_CAP")], VisualJs.MathObject[new V("max")].Call(new V(1d), (VisualJs.NumberObject[new V("isFinite")].Call(v_value_0).Truth ? v_value_0 : new V(1d))));
}
private static V F_calculateBackingSize(V[] args) {
V v_cssWidth_1 = VisualJs.Arg(args,0);
V v_cssHeight_2 = VisualJs.Arg(args,1);
V v_dpr_3 = VisualJs.Arg(args,2);
V v_safeDpr_4 = Scope[new V("clampDpr")].Call(v_dpr_3);
return VisualJs.Obj(new V("width"), VisualJs.MathObject[new V("max")].Call(new V(1d), VisualJs.MathObject[new V("round")].Call((v_cssWidth_1 * v_safeDpr_4))), new V("height"), VisualJs.MathObject[new V("max")].Call(new V(1d), VisualJs.MathObject[new V("round")].Call((v_cssHeight_2 * v_safeDpr_4))), new V("dpr"), v_safeDpr_4);
}
private static V F_cssPointToWorld(V[] args) {
V v_clientX_5 = VisualJs.Arg(args,0);
V v_clientY_6 = VisualJs.Arg(args,1);
V v_rect_7 = VisualJs.Arg(args,2);
V v_cameraX_8 = VisualJs.Arg(args,3);
V v_cameraY_9 = VisualJs.Arg(args,4);
return VisualJs.Obj(new V("x"), (((v_clientX_5 - v_rect_7[new V("left")]) - (v_rect_7[new V("width")] / new V(2d))) + v_cameraX_8), new V("y"), (((v_clientY_6 - v_rect_7[new V("top")]) - (v_rect_7[new V("height")] / new V(2d))) + v_cameraY_9));
}
}
}
#endif
