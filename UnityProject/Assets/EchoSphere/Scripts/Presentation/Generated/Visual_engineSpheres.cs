#if UNITY_EDITOR
// Generated from src/engineSpheres.ts; sha256 1af6fed80e20293361c8e0195053bf58846385c4079064b84a38382f22d05fbc.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_engineSpheres {
public static readonly V Scope=VisualJs.Obj();
static Visual_engineSpheres() {
Scope[new V("getMaxSpheres")] = VisualJs.Fn(F_getMaxSpheres);
Scope[new V("getSphereRadius")] = VisualJs.Fn(F_getSphereRadius);
Scope[new V("getSphereDamage")] = VisualJs.Fn(F_getSphereDamage);
Scope[new V("getSphereDpsEstimate")] = VisualJs.Fn(F_getSphereDpsEstimate);
Scope[new V("getSphereDelay")] = VisualJs.Fn(F_getSphereDelay);
Scope[new V("getSpecialSphereCadence")] = VisualJs.Fn(F_getSpecialSphereCadence);
}
private static V F_getMaxSpheres(V[] args) {
V v_s_0 = VisualJs.Arg(args,0);
V v_m_1 = ((Visual_engineBalanceConstants.Scope[new V("DEFAULT_MAX_SPHERES")] + (VisualJs.Or(v_s_0[new V("player")][new V("abilities")][new V("maxspheres")], () => new V(0d)))) + (VisualJs.Or(v_s_0[new V("shopUpgrades")][new V("spheres")], () => new V(0d))));
return VisualJs.MathObject[new V("min")].Call(v_m_1, Visual_engineBalanceConstants.Scope[new V("MAX_SPHERES_CAP")]);
}
private static V F_getSphereRadius(V[] args) {
V v_s_2 = VisualJs.Arg(args,0);
V v_sphere_3 = VisualJs.Arg(args,1);
V v_r_4 = Visual_engineBalanceConstants.Scope[new V("BASE_SPHERE_RADIUS")];
V v_lvl_5 = VisualJs.Or(v_s_2[new V("player")][new V("abilities")][new V("radius")], () => new V(0d));
v_r_4 *= (new V(1d) + (v_lvl_5 * new V(0.15d)));
v_r_4 *= (new V(1d) + ((VisualJs.Or(v_s_2[new V("shopUpgrades")][new V("radius")], () => new V(0d))) * new V(0.05d)));
v_r_4 *= Visual_artifactSystem.Scope[new V("getArtifactSphereRadiusMultiplier")].Call(v_s_2);
if (VisualJs.And(new V(v_s_2[new V("player")][new V("chaosOrbBuff")] == new V("radius")), () => new V(v_s_2[new V("player")][new V("chaosOrbBuffTimer")] > new V(0d))).Truth) v_r_4 *= new V(1.2d);
if (new V(v_s_2[new V("player")][new V("mutationStage")] >= new V(2d)).Truth) v_r_4 *= new V(1.15d);
v_r_4 *= Visual_characterRuntime.Scope[new V("getCharacterRadiusMultiplier")].Call(v_s_2);
v_r_4 *= VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_2, v_sphere_3[new V("type")])[new V("radius")];
if (VisualJs.And(new V(Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_2) == new V("architect")), () => new V(v_s_2[new V("player")][new V("characterMasteryLevel")] >= new V(3d))).Truth) v_r_4 *= new V(1.02d);
return v_r_4;
}
private static V F_getSphereDamage(V[] args) {
V v_s_6 = VisualJs.Arg(args,0);
V v_sphere_7 = VisualJs.Arg(args,1);
V v_network_8 = VisualJs.Arg(args,2);
V v_d_9 = Visual_engineBalanceConstants.Scope[new V("BASE_SPHERE_DAMAGE")];
V v_lvl_10 = VisualJs.Or(v_s_6[new V("player")][new V("abilities")][new V("damage")], () => new V(0d));
v_d_9 *= (new V(1d) + (v_lvl_10 * new V(0.15d)));
v_d_9 *= (new V(1d) + ((VisualJs.Or(v_s_6[new V("shopUpgrades")][new V("dmg")], () => new V(0d))) * new V(0.05d)));
if (new V(v_s_6[new V("player")][new V("mutationStage")] >= new V(1d)).Truth) v_d_9 *= new V(1.1d);
if (VisualJs.And(new V(v_s_6[new V("player")][new V("chaosOrbBuff")] == new V("dmg")), () => new V(v_s_6[new V("player")][new V("chaosOrbBuffTimer")] > new V(0d))).Truth) v_d_9 *= new V(1.2d);
if (new V(v_s_6[new V("player")][new V("teleportDamageBuffTimer")] > new V(0d)).Truth) v_d_9 *= new V(2d);
if (new V(v_s_6[new V("player")][new V("overloadTimer")] > new V(0d)).Truth) v_d_9 *= (new V(1.25d) + ((VisualJs.Or(v_s_6[new V("player")][new V("abilities")][new V("darkritual")], () => new V(0d))) * new V(0.04d)));
if (VisualJs.And(VisualJs.And(new V(v_s_6[new V("player")][new V("fireTrailTimer")] > new V(0d)), () => v_sphere_7), () => new V(VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_6, v_sphere_7[new V("type")], v_sphere_7)[new V("fire")] > new V(0d))).Truth) {
v_d_9 *= (new V(1.2d) + ((VisualJs.Or(v_s_6[new V("player")][new V("abilities")][new V("firetrail")], () => new V(0d))) * new V(0.025d)));
}
if (VisualJs.And(VisualJs.And(new V(v_s_6[new V("player")][new V("fireCatalystTimer")] > new V(0d)), () => v_sphere_7), () => new V(VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_6, v_sphere_7[new V("type")], v_sphere_7)[new V("fire")] > new V(0d))).Truth) v_d_9 *= new V(1.35d);
if (VisualJs.And(VisualJs.And(v_sphere_7, () => new V(VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_6, v_sphere_7[new V("type")], v_sphere_7)[new V("fire")] > new V(0d))), () => new V(Visual_engineRuntime.Scope[new V("getAbilityBranchId")].Call(v_s_6, new V("firetrail"), new V(4d)) == new V("firetrail_overdrive"))).Truth) v_d_9 *= new V(1.15d);
if (VisualJs.And(VisualJs.And(new V(v_s_6[new V("player")][new V("timestopTimer")] > new V(0d)), () => new V(v_s_6[new V("player")][new V("timestopTimer")] <= new V(1d))), () => new V(Visual_engineRuntime.Scope[new V("getAbilityBranchId")].Call(v_s_6, new V("timestop"), new V(7d)) == new V("timestop_temporal_core"))).Truth) v_d_9 *= new V(2d);
V v_sbLvl_11 = VisualJs.Or(v_s_6[new V("player")][new V("abilities")][new V("sphereboost")], () => new V(0d));
if (new V(v_sbLvl_11 > new V(0d)).Truth) {
V v_per_12 = VisualJs.MathObject[new V("max")].Call(new V(50d), (new V(100d) - (((v_sbLvl_11 - new V(1d))) * new V(10d))));
v_d_9 += VisualJs.MathObject[new V("floor")].Call((v_s_6[new V("player")][new V("kills")] / v_per_12));
}
if (v_s_6[new V("player")][new V("evolutions")][new V("includes")].Call(new V("echoaccumulator")).Truth) {
v_d_9 += (v_s_6[new V("player")][new V("sphereXpAccumulator")] * new V(0.5d));
}
v_d_9 *= Visual_artifactSystem.Scope[new V("getArtifactSphereDamageMultiplier")].Call(v_s_6);
v_d_9 *= Visual_artifactSystem.Scope[new V("getSphereArtifactDamageMultiplier")].Call(v_s_6, v_sphere_7);
v_d_9 *= VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_6, v_sphere_7[new V("type")], v_sphere_7)[new V("damage")];
v_d_9 *= Visual_characterRuntime.Scope[new V("getSphereCopyOutputMultiplier")].Call(v_s_6, v_sphere_7);
if (v_sphere_7.Truth) {
V v_networkState_13 = VisualJs.Coalesce(v_network_8, () => VisualBindings.Get(new V("engineRuntime.ts"),new V("getNetworkFrame")).Call(v_s_6));
V v_profile_14 = Visual_network.Scope[new V("getSphereNetworkProfile")].Call(v_networkState_13, v_s_6[new V("spheres")][new V("indexOf")].Call(v_sphere_7));
}
return v_d_9;
}
private static V F_getSphereDpsEstimate(V[] args) {
V v_s_15 = VisualJs.Arg(args,0);
V v_sphere_16 = VisualJs.Arg(args,1);
V v_def_17 = Visual_gameData.Scope[new V("SPHERE_TYPES")][v_sphere_16[new V("type")]];
V v_mods_18 = VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_15, v_sphere_16[new V("type")], v_sphere_16);
V v_network_19 = VisualBindings.Get(new V("engineRuntime.ts"),new V("getNetworkFrame")).Call(v_s_15);
V v_damage_20 = Scope[new V("getSphereDamage")].Call(v_s_15, v_sphere_16, v_network_19);
V v_delay_21 = VisualJs.MathObject[new V("max")].Call(new V(0.05d), ((Scope[new V("getSphereDelay")].Call(v_s_15, v_sphere_16, v_network_19) * v_def_17[new V("delayMult")]) * v_mods_18[new V("delay")]));
if (new V(v_sphere_16[new V("type")] == new V("orbital")).Truth) {
V v_level_22 = Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_15, new V("orbital"));
V v_branch_23 = VisualJs.Chain(v_s_15[new V("player")][new V("sphereBranches")],optional_24=>optional_24[new V("orbital")]);
V v_final_25 = Visual_engineRuntime.Scope[new V("getSphereFinalIndex")].Call(v_s_15, new V("orbital"));
V v_satelliteCounts_26 = Visual_orbitalGeometry.Scope[new V("getOrbitalRingCounts")].Call(v_level_22, (((v_s_15[new V("player")][new V("artifacts")][new V("includes")].Call(new V("orbital_crown")).Truth ? new V(1d) : new V(0d))) + ((VisualJs.And(new V(v_branch_23 == new V("orbital_dance")), () => new V(v_final_25 == new V(2d))).Truth ? new V(1d) : new V(0d)))));
V v_satellites_27 = (v_satelliteCounts_26[new V("inner")] + v_satelliteCounts_26[new V("outer")]);
V v_angularSpeed_28 = (((new V(1.8d) + VisualJs.MathObject[new V("min")].Call(new V(2.4d), (VisualJs.MathObject[new V("max")].Call(new V(1d), v_level_22) * new V(0.28d))))) * v_mods_18[new V("rotationSpeed")]);
V v_passRate_29 = ((v_satellites_27 * v_angularSpeed_28) / ((VisualJs.MathObject[new V("PI")] * new V(2d))));
return ((v_damage_20 * new V(0.95d)) * v_passRate_29);
}
if (new V(v_sphere_16[new V("type")] == new V("prism")).Truth) return (((v_damage_20 * VisualJs.MathObject[new V("max")].Call(new V(1d), (new V(1d) + v_mods_18[new V("multishot")])))) / v_delay_21);
if (new V(v_sphere_16[new V("type")] == new V("pulse")).Truth) {
V v_waves_30 = (new V(1d) + ((v_s_15[new V("player")][new V("artifacts")][new V("includes")].Call(new V("pulse_crown")).Truth ? new V(1d) : new V(0d))));
V v_interval_31 = Scope[new V("getSpecialSphereCadence")].Call(v_s_15, v_sphere_16, ((new V(1.15d) * v_mods_18[new V("auraPulse")]) * ((v_s_15[new V("player")][new V("artifacts")][new V("includes")].Call(new V("pulse_driver")).Truth ? new V(0.9d) : new V(1d)))), v_network_19);
return ((((v_damage_20 * v_waves_30) * new V(4d))) / v_interval_31);
}
if (new V(v_sphere_16[new V("type")] == new V("gravity")).Truth) {
V v_interval_32 = Scope[new V("getSpecialSphereCadence")].Call(v_s_15, v_sphere_16, (new V(0.8d) * v_mods_18[new V("auraPulse")]), v_network_19);
return (((v_damage_20 * new V(4d))) / v_interval_32);
}
if (new V(v_sphere_16[new V("type")] == new V("void")).Truth) return (((v_damage_20 * ((new V(Visual_sphereProgression.Scope[new V("sphereLevel")].Call(v_s_15, new V("void")) >= new V(2d)).Truth ? new V(1.3d) : new V(1.15d))))) / v_delay_21);
if (v_def_17[new V("aura")].Truth) return (v_damage_20 / v_delay_21);
V v_shots_33 = (((new V(1d) + v_mods_18[new V("multishot")])) * v_def_17[new V("pellets")]);
return (((v_damage_20 * v_shots_33)) / v_delay_21);
}
private static V F_getSphereDelay(V[] args) {
V v_s_34 = VisualJs.Arg(args,0);
V v_sphere_35 = VisualJs.Arg(args,1);
V v_network_36 = VisualJs.Arg(args,2);
V v_d_37 = Visual_engineBalanceConstants.Scope[new V("BASE_SPHERE_DELAY")];
V v_lvl_38 = VisualJs.Or(v_s_34[new V("player")][new V("abilities")][new V("attackspeed")], () => new V(0d));
v_d_37 *= VisualJs.MathObject[new V("pow")].Call(new V(0.93d), v_lvl_38);
v_d_37 /= VisualJs.MathObject[new V("max")].Call(new V(0.01d), Visual_characterRuntime.Scope[new V("getCharacterAttackSpeedMultiplier")].Call(v_s_34));
v_d_37 *= Visual_artifactSystem.Scope[new V("getArtifactSphereDelayMultiplier")].Call(v_s_34);
if (new V(v_s_34[new V("player")][new V("overloadTimer")] > new V(0d)).Truth) v_d_37 *= new V(0.72d);
if (VisualJs.And(VisualJs.And(new V(v_s_34[new V("player")][new V("fireTrailTimer")] > new V(0d)), () => v_sphere_35), () => new V(VisualBindings.Get(new V("sphereProgression.ts"),new V("sphereModifiers")).Call(v_s_34, v_sphere_35[new V("type")], v_sphere_35)[new V("fire")] > new V(0d))).Truth) v_d_37 *= new V(0.78d);
if (v_sphere_35.Truth) {
V v_networkState_39 = VisualJs.Coalesce(v_network_36, () => VisualBindings.Get(new V("engineRuntime.ts"),new V("getNetworkFrame")).Call(v_s_34));
V v_profile_40 = Visual_network.Scope[new V("getSphereNetworkProfile")].Call(v_networkState_39, v_s_34[new V("spheres")][new V("indexOf")].Call(v_sphere_35));
V v_clusterBonus_41 = Visual_network.Scope[new V("getFormationBonusMultiplier")].Call(v_networkState_39, new V("cluster"), v_s_34[new V("spheres")][new V("indexOf")].Call(v_sphere_35));
if (new V(v_clusterBonus_41 > new V(0d)).Truth) v_d_37 *= (new V(1d) - (new V(0.1d) * v_clusterBonus_41));
}
return v_d_37;
}
private static V F_getSpecialSphereCadence(V[] args) {
V v_s_42 = VisualJs.Arg(args,0);
V v_sphere_43 = VisualJs.Arg(args,1);
V v_baseInterval_44 = VisualJs.Arg(args,2);
V v_network_45 = VisualJs.Arg(args,3);
V v_delayRatio_46 = (Scope[new V("getSphereDelay")].Call(v_s_42, v_sphere_43, v_network_45) / Visual_engineBalanceConstants.Scope[new V("BASE_SPHERE_DELAY")]);
return VisualJs.MathObject[new V("max")].Call(new V(0.08d), (v_baseInterval_44 * v_delayRatio_46));
}
}
}
#endif
