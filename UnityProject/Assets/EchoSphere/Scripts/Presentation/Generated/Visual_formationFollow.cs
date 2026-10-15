// Generated from src/formationFollow.ts; sha256 d4d0fdbf1db1a5d2a89ddc68f4f2fe1f9edf538bbce8c85fe016590c89756e40.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_formationFollow {
public static readonly V Scope=VisualJs.Obj();
static Visual_formationFollow() {
Scope[new V("isFormationFollowMovementAbility")] = VisualJs.Fn(F_isFormationFollowMovementAbility);
Scope[new V("FOLLOW_MOVEMENT_ABILITIES")] = VisualJs.NewSet(VisualJs.Arr(new V("teleport")));
}
private static V F_isFormationFollowMovementAbility(V[] args) {
V v_ability_0 = VisualJs.Arg(args,0);
return Scope[new V("FOLLOW_MOVEMENT_ABILITIES")][new V("has")].Call(v_ability_0);
return default;
}
}
}
