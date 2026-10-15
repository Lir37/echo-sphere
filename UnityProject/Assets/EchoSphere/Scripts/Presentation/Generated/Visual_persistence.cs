// Generated from src/persistence.ts; sha256 3c0774dd38bb536b94f5a47b0e59b8d66a8ebbb1c244b7c9edeb39c8f81d7cd1.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_persistence {
public static readonly V Scope=VisualJs.Obj();
static Visual_persistence() {
Scope[new V("getCharacterMasteryNextThreshold")] = VisualJs.Fn(F_getCharacterMasteryNextThreshold);
Scope[new V("CHARACTER_MASTERY_THRESHOLDS")] = (VisualJs.Arr(new V(0d), new V(250d), new V(750d), new V(1500d), new V(2500d), new V(4000d), new V(6000d), new V(8500d), new V(11500d), new V(15000d)));
}
private static V F_getCharacterMasteryNextThreshold(V[] args) {
V v_level_0 = VisualJs.Arg(args,0);
return (new V(v_level_0 >= new V(10d)).Truth ? VisualJs.Null : Scope[new V("CHARACTER_MASTERY_THRESHOLDS")][v_level_0]);
return default;
}
}
}
