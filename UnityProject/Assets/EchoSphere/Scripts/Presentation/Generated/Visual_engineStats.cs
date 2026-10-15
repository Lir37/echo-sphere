// Generated from src/engineStats.ts; sha256 ea07863b529fa8531ff5b5f56f529200d8edf3b2473ceff1d3c0f7c8bbd09800.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_engineStats {
public static readonly V Scope=VisualJs.Obj();
static Visual_engineStats() {
Scope[new V("getMoveSpeed")] = VisualJs.Fn(F_getMoveSpeed);
Scope[new V("getBuildDiagnostics")] = VisualJs.Fn(F_getBuildDiagnostics);
Scope[new V("diagnosticSphereNames")] = VisualJs.Fn(args_33 => { V v_s_34 = VisualJs.Arg(args_33, 0);
V v_types_35 = VisualJs.Arg(args_33, 1);
V v_picked_36 = v_types_35[new V("filter")].Call(VisualJs.Fn(args_37 => { V v_type_38 = VisualJs.Arg(args_37, 0);
return new V(Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_34, v_type_38) > new V(0d)); }));
return VisualJs.Obj(new V("ru"), v_picked_36[new V("map")].Call(VisualJs.Fn(args_39 => { V v_type_40 = VisualJs.Arg(args_39, 0);
return (new V("") + Visual_gameData.Scope[new V("SPHERE_TYPES")][v_type_40][new V("name")][new V("ru")] + new V(" VII"))[new V("replace")].Call(new V(" VII"), (new V(" ") + Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_34, v_type_40) + new V(""))); })), new V("en"), v_picked_36[new V("map")].Call(VisualJs.Fn(args_41 => { V v_type_42 = VisualJs.Arg(args_41, 0);
return (new V("") + Visual_gameData.Scope[new V("SPHERE_TYPES")][v_type_42][new V("name")][new V("en")] + new V(" ") + Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_34, v_type_42) + new V("")); }))); });
Scope[new V("hasAbility")] = VisualJs.Fn(args_43 => { V v_s_44 = VisualJs.Arg(args_43, 0);
V v_id_45 = VisualJs.Arg(args_43, 1);
return new V((VisualJs.Or(v_s_44[new V("player")][new V("abilities")][v_id_45], () => new V(0d))) > new V(0d)); });
}
private static V F_getMoveSpeed(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_sp_1 = Visual_engineState.Scope[new V("BASE_PLAYER_SPEED")];
V v_lvl_2 = VisualJs.Or(v_s_0[new V("player")][new V("abilities")][new V("movespeed")], () => new V(0d));
v_sp_1 *= (new V(1d) + (v_lvl_2 * new V(0.1d)));
v_sp_1 *= (new V(1d) + ((VisualJs.Or(v_s_0[new V("shopUpgrades")][new V("speed")], () => new V(0d))) * new V(0.05d)));
v_sp_1 *= Visual_artifactSystem.Scope[new V("getArtifactMoveSpeedMultiplier")].Call(v_s_0);
if (new V(v_s_0[new V("player")][new V("swiftBootsTimer")] > new V(0d)).Truth) v_sp_1 *= new V(1.1d);
if (new V(v_s_0[new V("player")][new V("mutationStage")] >= new V(3d)).Truth) v_sp_1 *= new V(1.2d);
v_sp_1 *= Visual_characterRuntime.Scope[new V("getCharacterMoveSpeedMultiplier")].Call(v_s_0);
if (VisualJs.And(new V(v_s_0[new V("player")][new V("hunterTrophyTimer")] > new V(0d)), () => new V(Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_0) == new V("hunter"))).Truth) v_sp_1 *= new V(1.1d);
if (VisualJs.And(VisualJs.And(new V(Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_0) == new V("berserker")), () => new V(v_s_0[new V("player")][new V("characterMasteryLevel")] >= new V(5d))), () => new V(v_s_0[new V("player")][new V("buffTimer")] > new V(0d))).Truth) v_sp_1 *= new V(1.05d);
if (VisualJs.And(v_s_0[new V("player")][new V("abilities")][new V("momentum")], () => new V(v_s_0[new V("player")][new V("dashTimer")] > new V(0d))).Truth) v_sp_1 *= (new V(1d) + ((v_s_0[new V("player")][new V("abilities")][new V("momentum")] * new V(0.05d))));
return v_sp_1;
}
private static V F_getBuildDiagnostics(V[] args) {
V v_s_3 = VisualJs.Arg(args,0);
V v_network_4 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_s_3[new V("spheres")]);
V v_activeSets_5 = Visual_artifactSystem.Scope[new V("getArtifactSetProgress")].Call(v_s_3)[new V("filter")].Call(VisualJs.Fn(args_6 => { V v_set_7 = VisualJs.Arg(args_6, 0);
return v_set_7[new V("complete")]; }));
V v_activeProtocols_8 = Visual_artifactSystem.Scope[new V("getArtifactProtocolStates")].Call(v_s_3)[new V("filter")].Call(VisualJs.Fn(args_9 => { V v_protocol_10 = VisualJs.Arg(args_9, 0);
return v_protocol_10[new V("active")]; }));
V v_charSynergies_11 = Visual_sphereProgression.Scope[new V("getActiveSphereAbilitySynergies")].Call(v_s_3);
V v_offense_12 = Scope[new V("diagnosticSphereNames")].Call(v_s_3, VisualJs.Arr(new V("standard"), new V("sniper"), new V("shotgun"), new V("chain"), new V("pulse"), new V("void")));
V v_control_13 = Scope[new V("diagnosticSphereNames")].Call(v_s_3, VisualJs.Arr(new V("aura"), new V("gravity"), new V("prism")));
V v_survivalSignalsRu_14 = VisualJs.Arr();
V v_survivalSignalsEn_15 = VisualJs.Arr();
if (Scope[new V("hasAbility")].Call(v_s_3, new V("vitality")).Truth) {
v_survivalSignalsRu_14[new V("push")].Call(new V("Живучесть"));
v_survivalSignalsEn_15[new V("push")].Call(new V("Vitality"));
}
if (Scope[new V("hasAbility")].Call(v_s_3, new V("shield")).Truth) {
v_survivalSignalsRu_14[new V("push")].Call(new V("Барьер"));
v_survivalSignalsEn_15[new V("push")].Call(new V("Barrier"));
}
if (Scope[new V("hasAbility")].Call(v_s_3, new V("dodge")).Truth) {
v_survivalSignalsRu_14[new V("push")].Call(new V("Уклонение"));
v_survivalSignalsEn_15[new V("push")].Call(new V("Dodge"));
}
if (Scope[new V("hasAbility")].Call(v_s_3, new V("vampire")).Truth) {
v_survivalSignalsRu_14[new V("push")].Call(new V("Вампиризм"));
v_survivalSignalsEn_15[new V("push")].Call(new V("Vampirism"));
}
if (new V((VisualJs.Or(v_s_3[new V("player")][new V("abilities")][new V("movespeed")], () => new V(0d))) > new V(0d)).Truth) {
v_survivalSignalsRu_14[new V("push")].Call(new V("Мобильность"));
v_survivalSignalsEn_15[new V("push")].Call(new V("Mobility"));
}
V v_resonanceGeometry_16 = new V(VisualJs.Or(((v_s_3[new V("player")]))[new V("resonanceGeometryKey")], () => new V("none")).Text);
return VisualJs.Arr(VisualJs.Obj(new V("id"), new V("offense"), new V("ru"), VisualJs.Concat(v_offense_12[new V("ru")], ((new V((VisualJs.Or(v_s_3[new V("player")][new V("abilities")][new V("damage")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr((new V("") + Visual_gameData.Scope[new V("ABILITIES")][new V("damage")][new V("name")][new V("ru")] + new V(" ") + v_s_3[new V("player")][new V("abilities")][new V("damage")] + new V(""))) : VisualJs.Arr()))), new V("en"), VisualJs.Concat(v_offense_12[new V("en")], ((new V((VisualJs.Or(v_s_3[new V("player")][new V("abilities")][new V("damage")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr((new V("") + Visual_gameData.Scope[new V("ABILITIES")][new V("damage")][new V("name")][new V("en")] + new V(" ") + v_s_3[new V("player")][new V("abilities")][new V("damage")] + new V(""))) : VisualJs.Arr())))), VisualJs.Obj(new V("id"), new V("control"), new V("ru"), VisualJs.Concat(v_control_13[new V("ru")], ((new V((VisualJs.Or(v_s_3[new V("player")][new V("sphereMods")][new V("freeze")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr(new V("Freeze")) : VisualJs.Arr())), ((new V((VisualJs.Or(v_s_3[new V("player")][new V("sphereMods")][new V("gravitic")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr(new V("Gravity")) : VisualJs.Arr()))), new V("en"), VisualJs.Concat(v_control_13[new V("en")], ((new V((VisualJs.Or(v_s_3[new V("player")][new V("sphereMods")][new V("freeze")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr(new V("Freeze")) : VisualJs.Arr())), ((new V((VisualJs.Or(v_s_3[new V("player")][new V("sphereMods")][new V("gravitic")], () => new V(0d))) > new V(0d)).Truth ? VisualJs.Arr(new V("Gravity")) : VisualJs.Arr())))), VisualJs.Obj(new V("id"), new V("survival"), new V("ru"), v_survivalSignalsRu_14, new V("en"), v_survivalSignalsEn_15), VisualJs.Obj(new V("id"), new V("network"), new V("ru"), VisualJs.Arr((new V("Типов сфер: ") + VisualJs.NewSet(v_s_3[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_17 => { V v_x_18 = VisualJs.Arg(args_17, 0);
return v_x_18[new V("alive")]; }))[new V("map")].Call(VisualJs.Fn(args_19 => { V v_x_20 = VisualJs.Arg(args_19, 0);
return v_x_20[new V("type")]; })))[new V("size")] + new V("")), (new V("Связей: ") + v_network_4[new V("links")][new V("length")] + new V("")), (new V("Геометрия: ") + v_resonanceGeometry_16 + new V(""))), new V("en"), VisualJs.Arr((new V("Sphere types: ") + VisualJs.NewSet(v_s_3[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_21 => { V v_x_22 = VisualJs.Arg(args_21, 0);
return v_x_22[new V("alive")]; }))[new V("map")].Call(VisualJs.Fn(args_23 => { V v_x_24 = VisualJs.Arg(args_23, 0);
return v_x_24[new V("type")]; })))[new V("size")] + new V("")), (new V("Links: ") + v_network_4[new V("links")][new V("length")] + new V("")), (new V("Geometry: ") + v_resonanceGeometry_16 + new V("")))), VisualJs.Obj(new V("id"), new V("resonance"), new V("ru"), VisualJs.Arr((new V("Резонанс: ") + VisualJs.MathObject[new V("floor")].Call(v_s_3[new V("player")][new V("resonanceCharge")]) + new V("/100")), (new V("Геометрия: ") + v_resonanceGeometry_16 + new V(""))), new V("en"), VisualJs.Arr((new V("Resonance: ") + VisualJs.MathObject[new V("floor")].Call(v_s_3[new V("player")][new V("resonanceCharge")]) + new V("/100")), (new V("Geometry: ") + v_resonanceGeometry_16 + new V("")))), VisualJs.Obj(new V("id"), new V("synergy"), new V("ru"), VisualJs.Concat(v_activeSets_5[new V("map")].Call(VisualJs.Fn(args_25 => { V v_set_26 = VisualJs.Arg(args_25, 0);
return v_set_26[new V("name")][new V("ru")]; })), v_activeProtocols_8[new V("map")].Call(VisualJs.Fn(args_27 => { V v_protocol_28 = VisualJs.Arg(args_27, 0);
return v_protocol_28[new V("name")][new V("ru")]; })), ((v_charSynergies_11[new V("length")].Truth ? VisualJs.Arr((new V("Активных синергий сфер: ") + v_charSynergies_11[new V("length")] + new V(""))) : VisualJs.Arr()))), new V("en"), VisualJs.Concat(v_activeSets_5[new V("map")].Call(VisualJs.Fn(args_29 => { V v_set_30 = VisualJs.Arg(args_29, 0);
return v_set_30[new V("name")][new V("en")]; })), v_activeProtocols_8[new V("map")].Call(VisualJs.Fn(args_31 => { V v_protocol_32 = VisualJs.Arg(args_31, 0);
return v_protocol_32[new V("name")][new V("en")]; })), ((v_charSynergies_11[new V("length")].Truth ? VisualJs.Arr((new V("Active Sphere synergies: ") + v_charSynergies_11[new V("length")] + new V(""))) : VisualJs.Arr())))));
}
}
}
