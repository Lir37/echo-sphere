// Generated from src/renderScale.ts; sha256 485ba817b4abe2101a849d138cf2b170725ac0b7b582dee2848fbe5b42d49a2d.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_renderScale {
public static readonly V Scope=VisualJs.Obj();
static Visual_renderScale() {
Scope[new V("cssPointToWorld")] = VisualJs.Fn(F_cssPointToWorld);
}
private static V F_cssPointToWorld(V[] args) {
V v_clientX_0 = VisualJs.Arg(args,0);
V v_clientY_1 = VisualJs.Arg(args,1);
V v_rect_2 = VisualJs.Arg(args,2);
V v_cameraX_3 = VisualJs.Arg(args,3);
V v_cameraY_4 = VisualJs.Arg(args,4);
return VisualJs.Obj(new V("x"), (((v_clientX_0 - v_rect_2[new V("left")]) - (v_rect_2[new V("width")] / new V(2d))) + v_cameraX_3), new V("y"), (((v_clientY_1 - v_rect_2[new V("top")]) - (v_rect_2[new V("height")] / new V(2d))) + v_cameraY_4));
}
}
}
