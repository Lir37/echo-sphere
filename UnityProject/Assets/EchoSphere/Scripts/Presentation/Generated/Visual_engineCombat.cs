#if UNITY_EDITOR
// Generated from src/engineCombat.ts; sha256 a4129556873bcf8d6afb05aad0cc7145edc1431e6bf9c14af3dd619fd5f9cf5b.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_engineCombat {
public static readonly V Scope=VisualJs.Obj();
static Visual_engineCombat() {
Scope[new V("getCritChance")] = VisualJs.Fn(F_getCritChance);
Scope[new V("getDodgeChance")] = VisualJs.Fn(F_getDodgeChance);
Scope[new V("getVampirePercent")] = VisualJs.Fn(F_getVampirePercent);
}
private static V F_getCritChance(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_sphere_1 = VisualJs.Arg(args,1);
V v_c_2 = Visual_combatRules.Scope[new V("CRIT_BASE")];
v_c_2 += ((VisualJs.Or(v_s_0[new V("player")][new V("abilities")][new V("crit")], () => new V(0d))) * new V(0.1d));
v_c_2 += ((VisualJs.Or(v_s_0[new V("shopUpgrades")][new V("crit")], () => new V(0d))) * new V(0.05d));
v_c_2 += Visual_artifactSystem.Scope[new V("getArtifactCritChanceBonus")].Call(v_s_0);
if (VisualJs.And(new V(VisualJs.Chain(v_sphere_1,optional_3=>optional_3[new V("type")]) == new V("sniper")), () => new V(Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_0, new V("sniper")) >= new V(3d))).Truth) v_c_2 += new V(0.15d);
return VisualJs.MathObject[new V("min")].Call(new V(0.75d), v_c_2);
}
private static V F_getDodgeChance(V[] args) {
V v_s_4 = VisualJs.Arg(args,0);
return VisualJs.MathObject[new V("min")].Call(new V(0.75d), (((VisualJs.Or(v_s_4[new V("player")][new V("abilities")][new V("dodge")], () => new V(0d))) * new V(0.1d)) + Visual_artifactSystem.Scope[new V("getArtifactDodgeChanceBonus")].Call(v_s_4)));
}
private static V F_getVampirePercent(V[] args) {
V v_s_5 = VisualJs.Arg(args,0);
V v_lvl_6 = VisualJs.Or(v_s_5[new V("player")][new V("abilities")][new V("vampire")], () => new V(0d));
return ((v_lvl_6 * new V(0.03d)) + Visual_artifactSystem.Scope[new V("getArtifactVampireBonus")].Call(v_s_5));
}
}
}
#endif
