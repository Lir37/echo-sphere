#if UNITY_EDITOR
// Generated from src/engineProgression.ts; sha256 d330c4ba740b03ba036a184f743661d39720dbb5ed01dd90d1b4279ddf374f05.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_engineProgression {
public static readonly V Scope=VisualJs.Obj();
static Visual_engineProgression() {
Scope[new V("getSphereEvolutionChoices")] = VisualJs.Fn(F_getSphereEvolutionChoices);
Scope[new V("getAbilityEvolutionChoices")] = VisualJs.Fn(F_getAbilityEvolutionChoices);
Scope[new V("getUpgradeChoiceKey")] = VisualJs.Fn(F_getUpgradeChoiceKey);
}
private static V F_getSphereEvolutionChoices(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_type_1 = VisualJs.Arg(args,1);
V v_level_2 = VisualJs.Arg(args,2);
V v_def_3 = Visual_sphereProgression.Scope[new V("SPHERE_PROGRESSION")][v_type_1];
if (new V(v_level_2 == new V(4d)).Truth) {
return v_def_3[new V("evolution4Choices")][new V("slice")].Call(new V(0d), new V(3d))[new V("map")].Call(VisualJs.Fn(args_4 => { V v_branch_5 = VisualJs.Arg(args_4, 0);
return (VisualJs.Obj(new V("type"), (new V("sphere")), new V("sphereType"), v_type_1, new V("sphereBranch"), v_branch_5[new V("id")], new V("sphereStage"), (new V("branch")), new V("name"), VisualJs.Obj(new V("ru"), v_branch_5[new V("name")][new V("ru")], new V("en"), v_branch_5[new V("name")][new V("en")]), new V("desc"), VisualJs.Obj(new V("ru"), (Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]).Truth ? (new V("") + v_branch_5[new V("desc")][new V("ru")] + new V(" ") + (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("fire")).Truth ? new V("🔥") : (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("freeze")).Truth ? new V("❄️") : new V("☠️"))) + new V(" Стихия: ") + (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("fire")).Truth ? new V("Огонь") : (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("freeze")).Truth ? new V("Заморозка") : new V("Яд"))) + new V(".")) : v_branch_5[new V("desc")][new V("ru")]), new V("en"), (Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]).Truth ? (new V("") + v_branch_5[new V("desc")][new V("en")] + new V(" ") + (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("fire")).Truth ? new V("🔥") : (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("freeze")).Truth ? new V("❄️") : new V("☠️"))) + new V(" Element: ") + (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("fire")).Truth ? new V("Fire") : (new V(Visual_sphereProgression.Scope[new V("getSphereElementForBranch")].Call(v_branch_5[new V("id")]) == new V("freeze")).Truth ? new V("Freeze") : new V("Poison"))) + new V(".")) : v_branch_5[new V("desc")][new V("en")])), new V("currentLevel"), new V(4d), new V("newLevel"), new V(4d))); }));
}
V v_branchId_6 = v_s_0[new V("player")][new V("sphereBranches")][v_type_1];
V v_branch_7 = v_def_3[new V("evolution4Choices")][new V("find")].Call(VisualJs.Fn(args_8 => { V v_x_9 = VisualJs.Arg(args_8, 0);
return new V(v_x_9[new V("id")] == v_branchId_6); }));
if ((!v_branch_7).Truth) return VisualJs.Arr();
return v_branch_7[new V("final")][new V("slice")].Call(new V(0d), new V(3d))[new V("map")].Call(VisualJs.Fn(args_10 => { V v_finalChoice_11 = VisualJs.Arg(args_10, 0);
V v_index_12 = VisualJs.Arg(args_10, 1);
return (VisualJs.Obj(new V("type"), (new V("sphere")), new V("sphereType"), v_type_1, new V("sphereBranch"), v_branch_7[new V("id")], new V("sphereFinalIndex"), v_index_12, new V("sphereStage"), (new V("final")), new V("name"), VisualJs.Obj(new V("ru"), (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12).Truth ? (new V("") + v_finalChoice_11[new V("name")][new V("ru")] + new V(" • ") + (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12))[new V("name")][new V("ru")] + new V("")) : v_finalChoice_11[new V("name")][new V("ru")]), new V("en"), (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12).Truth ? (new V("") + v_finalChoice_11[new V("name")][new V("en")] + new V(" • ") + (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12))[new V("name")][new V("en")] + new V("")) : v_finalChoice_11[new V("name")][new V("en")])), new V("desc"), VisualJs.Obj(new V("ru"), (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12).Truth ? (new V("") + v_finalChoice_11[new V("desc")][new V("ru")] + new V(" ") + (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12))[new V("desc")][new V("ru")] + new V("")) : v_finalChoice_11[new V("desc")][new V("ru")]), new V("en"), (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12).Truth ? (new V("") + v_finalChoice_11[new V("desc")][new V("en")] + new V(" ") + (Visual_sphereProgression.Scope[new V("getSphereElementMasteryForBranch")].Call(v_branch_7[new V("id")], v_index_12))[new V("desc")][new V("en")] + new V("")) : v_finalChoice_11[new V("desc")][new V("en")])), new V("currentLevel"), new V(7d), new V("newLevel"), new V(7d))); }));
}
private static V F_getAbilityEvolutionChoices(V[] args) {
V v_s_13 = VisualJs.Arg(args,0);
V v_ability_14 = VisualJs.Arg(args,1);
V v_level_15 = VisualJs.Arg(args,2);
V v_progression_16 = Visual_sphereProgression.Scope[new V("ABILITY_PROGRESSION")][v_ability_14];
if ((!v_progression_16).Truth) return VisualJs.Arr();
V v_pool_17 = (new V(v_level_15 == new V(4d)).Truth ? v_progression_16[new V("evolution4")] : Visual_sphereProgression.Scope[new V("getAbilityEvolutionPool")].Call(v_s_13, v_ability_14, new V(7d)));
return v_pool_17[new V("slice")].Call(new V(0d), new V(3d))[new V("map")].Call(VisualJs.Fn(args_18 => { V v_evolution_19 = VisualJs.Arg(args_18, 0);
V v_index_20 = VisualJs.Arg(args_18, 1);
return (VisualJs.Obj(new V("type"), (new V("ability")), new V("ability"), v_ability_14, new V("abilityEvolutionIndex"), v_index_20, new V("abilityStage"), (new V(v_level_15 == new V(4d)).Truth ? (new V("branch")) : (new V("final"))), new V("name"), v_evolution_19[new V("name")], new V("desc"), v_evolution_19[new V("desc")], new V("currentLevel"), v_level_15, new V("newLevel"), v_level_15)); }));
}
private static V F_getUpgradeChoiceKey(V[] args) {
V v_choice_21 = VisualJs.Arg(args,0);
if (new V(v_choice_21[new V("type")] == new V("modifier")).Truth) {
return (new V("modifier:") + VisualJs.Coalesce(v_choice_21[new V("modifier")], () => new V("unknown")) + new V(""));
}
if (new V(v_choice_21[new V("type")] == new V("sphere")).Truth) {
return VisualJs.Arr(new V("sphere"), VisualJs.Coalesce(v_choice_21[new V("sphereType")], () => new V("unknown")), VisualJs.Coalesce(v_choice_21[new V("sphereStage")], () => new V("upgrade")), v_choice_21[new V("currentLevel")], v_choice_21[new V("newLevel")], VisualJs.Coalesce(v_choice_21[new V("sphereBranch")], () => new V("")), VisualJs.Coalesce(v_choice_21[new V("sphereFinalIndex")], () => new V("")))[new V("join")].Call(new V(":"));
}
return VisualJs.Arr(new V("ability"), VisualJs.Coalesce(v_choice_21[new V("ability")], () => new V("unknown")), VisualJs.Coalesce(v_choice_21[new V("abilityStage")], () => new V("upgrade")), v_choice_21[new V("currentLevel")], v_choice_21[new V("newLevel")], VisualJs.Coalesce(v_choice_21[new V("abilityEvolutionIndex")], () => new V("")))[new V("join")].Call(new V(":"));
}
}
}
#endif
