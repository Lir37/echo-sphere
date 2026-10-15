// Generated from src/interfaceScale.ts; sha256 b2993563fc50dcf4658203826b13a864385ac9b3aac8889d9f7aa406c4c8368a.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_interfaceScale {
public static readonly V Scope=VisualJs.Obj();
static Visual_interfaceScale() {
Scope[new V("loadInterfaceScale")] = VisualJs.Fn(F_loadInterfaceScale);
Scope[new V("saveInterfaceScale")] = VisualJs.Fn(F_saveInterfaceScale);
Scope[new V("formatInterfaceScale")] = VisualJs.Fn(F_formatInterfaceScale);
Scope[new V("INTERFACE_SCALE_KEY")] = new V("echosphere_interface_scale");
Scope[new V("DEFAULT_SCALE")] = new V(1d);
Scope[new V("MIN_SCALE")] = new V(0.75d);
Scope[new V("MAX_SCALE")] = new V(1.25d);
}
private static V F_loadInterfaceScale(V[] args) {
V v_raw_0 = VisualJs.Number(VisualUiBindings.Get("global","localStorage")[new V("getItem")].Call(Scope[new V("INTERFACE_SCALE_KEY")]));
if ((!VisualJs.NumberObject[new V("isFinite")].Call(v_raw_0)).Truth) return Scope[new V("DEFAULT_SCALE")];
return VisualJs.MathObject[new V("min")].Call(Scope[new V("MAX_SCALE")], VisualJs.MathObject[new V("max")].Call(Scope[new V("MIN_SCALE")], v_raw_0));
return default;
}
private static V F_saveInterfaceScale(V[] args) {
V v_value_1 = VisualJs.Arg(args,0);
V v_clamped_2 = VisualJs.MathObject[new V("min")].Call(Scope[new V("MAX_SCALE")], VisualJs.MathObject[new V("max")].Call(Scope[new V("MIN_SCALE")], v_value_1));
VisualUiBindings.Get("global","localStorage")[new V("setItem")].Call(Scope[new V("INTERFACE_SCALE_KEY")], new V(v_clamped_2.Text));
return default;
}
private static V F_formatInterfaceScale(V[] args) {
V v_value_3 = VisualJs.Arg(args,0);
return (new V("") + VisualJs.MathObject[new V("round")].Call((v_value_3 * new V(100d))) + new V("%"));
return default;
}
}
}
