#if UNITY_EDITOR
// Generated from src/network.ts; sha256 b3220bdab74739b11e247ebbe987954e323a1aba2f059ca17d5cdcccb455c177.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_network {
public static readonly V Scope=VisualJs.Obj();
static Visual_network() {
Scope[new V("getGeometryDominanceScore")] = VisualJs.Fn(F_getGeometryDominanceScore);
Scope[new V("rankGeometryCandidates")] = VisualJs.Fn(F_rankGeometryCandidates);
Scope[new V("chooseSecondaryFormation")] = VisualJs.Fn(F_chooseSecondaryFormation);
Scope[new V("selectActiveGeometry")] = VisualJs.Fn(F_selectActiveGeometry);
Scope[new V("getFormationBonusMultiplier")] = VisualJs.Fn(F_getFormationBonusMultiplier);
Scope[new V("distance")] = VisualJs.Fn(F_distance);
Scope[new V("aliveIndexes")] = VisualJs.Fn(F_aliveIndexes);
Scope[new V("combinations3")] = VisualJs.Fn(F_combinations3);
Scope[new V("lineStrength")] = VisualJs.Fn(F_lineStrength);
Scope[new V("triangleStrength")] = VisualJs.Fn(F_triangleStrength);
Scope[new V("squareStrength")] = VisualJs.Fn(F_squareStrength);
Scope[new V("clusterStrength")] = VisualJs.Fn(F_clusterStrength);
Scope[new V("ringShape")] = VisualJs.Fn(F_ringShape);
Scope[new V("isNodeSetConnected")] = VisualJs.Fn(F_isNodeSetConnected);
Scope[new V("latticeShape")] = VisualJs.Fn(F_latticeShape);
Scope[new V("analyzeSphereNetwork")] = VisualJs.Fn(F_analyzeSphereNetwork);
Scope[new V("getSphereNetworkProfile")] = VisualJs.Fn(F_getSphereNetworkProfile);
Scope[new V("SECONDARY_SCORE_GAP")] = new V(14d);
Scope[new V("SECONDARY_DISJOINT_SCORE_GAP")] = new V(18d);
Scope[new V("DOMINANCE_SWITCH_MARGIN")] = new V(10d);
Scope[new V("GEOMETRY_SPECIFICITY")] = VisualJs.Obj(new V("line"), new V(0d), new V("triangle"), new V(1d), new V("cluster"), new V(2d), new V("square"), new V(5d), new V("ring"), new V(3d), new V("lattice"), new V(5d), new V("fractal"), new V(7d));
Scope[new V("DEFAULT_LINK_DISTANCE")] = new V(220d);
}
private static V F_getGeometryDominanceScore(V[] args) {
V v_shape_0 = VisualJs.Arg(args,0);
V v_sizeBonus_1 = VisualJs.MathObject[new V("min")].Call(new V(6d), (VisualJs.MathObject[new V("max")].Call(new V(0d), (v_shape_0[new V("nodes")][new V("length")] - new V(3d))) * new V(1.5d)));
return (((v_shape_0[new V("strength")] * new V(100d)) + Scope[new V("GEOMETRY_SPECIFICITY")][v_shape_0[new V("type")]]) + v_sizeBonus_1);
}
private static V F_rankGeometryCandidates(V[] args) {
V v_candidates_2 = VisualJs.Arg(args,0);
V v_previousDominant_3 = VisualJs.Arg(args,1).Undefined?new V("none"):VisualJs.Arg(args,1);
V v_filtered_4 = ((VisualJs.ObjectObject[new V("values")].Call(v_candidates_2)))[new V("filter")].Call(VisualJs.Fn(args_5 => { V v_shape_6 = VisualJs.Arg(args_5, 0);
return new V(v_shape_6.Truth); }))[new V("filter")].Call(VisualJs.Fn(args_7 => { V v_shape_8 = VisualJs.Arg(args_7, 0);
return VisualJs.Or(new V(v_shape_8[new V("type")] != new V("fractal")), () => new V(v_shape_8[new V("nodes")][new V("length")] >= new V(6d))); }))[new V("map")].Call(VisualJs.Fn(args_9 => { V v_shape_10 = VisualJs.Arg(args_9, 0);
return (VisualJs.Merge(v_shape_10, VisualJs.Obj(new V("dominanceScore"),Scope[new V("getGeometryDominanceScore")].Call(v_shape_10)))); }))[new V("sort")].Call(VisualJs.Fn(args_11 => { V v_a_12 = VisualJs.Arg(args_11, 0);
V v_b_13 = VisualJs.Arg(args_11, 1);
return VisualJs.Or(VisualJs.Or(VisualJs.Or(((VisualJs.Or(v_b_13[new V("dominanceScore")], () => new V(0d))) - (VisualJs.Or(v_a_12[new V("dominanceScore")], () => new V(0d)))), () => (v_b_13[new V("strength")] - v_a_12[new V("strength")])), () => (v_b_13[new V("nodes")][new V("length")] - v_a_12[new V("nodes")][new V("length")])), () => v_a_12[new V("type")][new V("localeCompare")].Call(v_b_13[new V("type")])); }));
if (new V(v_previousDominant_3 == new V("none")).Truth) return v_filtered_4;
V v_previous_14 = v_filtered_4[new V("find")].Call(VisualJs.Fn(args_15 => { V v_shape_16 = VisualJs.Arg(args_15, 0);
return new V(v_shape_16[new V("type")] == v_previousDominant_3); }));
V v_top_17 = v_filtered_4[new V(0d)];
if (VisualJs.Or(VisualJs.Or((!v_previous_14), () => (!v_top_17)), () => new V(v_previous_14[new V("type")] == v_top_17[new V("type")])).Truth) return v_filtered_4;
if (new V((VisualJs.Or(v_top_17[new V("dominanceScore")], () => new V(0d))) < ((VisualJs.Or(v_previous_14[new V("dominanceScore")], () => new V(0d))) + Scope[new V("DOMINANCE_SWITCH_MARGIN")])).Truth) {
return VisualJs.Concat(VisualJs.Arr(v_previous_14), v_filtered_4[new V("filter")].Call(VisualJs.Fn(args_18 => { V v_shape_19 = VisualJs.Arg(args_18, 0);
return new V(v_shape_19[new V("type")] != v_previous_14[new V("type")]); })));
}
return v_filtered_4;
}
private static V F_chooseSecondaryFormation(V[] args) {
V v_ranked_20 = VisualJs.Arg(args,0);
V v_dominant_21 = VisualJs.Arg(args,1);
V v_selection_22 = VisualJs.Arg(args,2);
V v_selectedSecondary_23 = (VisualJs.And(VisualJs.And(VisualJs.Chain(v_selection_22,optional_24=>optional_24[new V("secondary")]), () => new V(v_selection_22[new V("secondary")] != new V("none"))), () => new V(v_selection_22[new V("secondary")] != v_dominant_21[new V("type")])).Truth ? VisualJs.Or(v_ranked_20[new V("find")].Call(VisualJs.Fn(args_25 => { V v_shape_26 = VisualJs.Arg(args_25, 0);
return new V(v_shape_26[new V("type")] == v_selection_22[new V("secondary")]); })), () => VisualJs.Null) : VisualJs.Null);
if (v_selectedSecondary_23.Truth) return v_selectedSecondary_23;
V v_dominantNodes_27 = VisualJs.NewSet(v_dominant_21[new V("nodes")]);
V v_scoreGap_28 = default; v_scoreGap_28 = VisualJs.Fn(args_29 => { V v_shape_30 = VisualJs.Arg(args_29, 0);
return ((VisualJs.Or(v_dominant_21[new V("dominanceScore")], () => new V(0d))) - (VisualJs.Or(v_shape_30[new V("dominanceScore")], () => new V(0d)))); });
V v_disjointCandidate_31 = v_ranked_20[new V("find")].Call(VisualJs.Fn(args_32 => { V v_shape_33 = VisualJs.Arg(args_32, 0);
return VisualJs.And(VisualJs.And(new V(v_shape_33[new V("type")] != v_dominant_21[new V("type")]), () => new V(v_scoreGap_28.Call(v_shape_33) <= Scope[new V("SECONDARY_DISJOINT_SCORE_GAP")])), () => v_shape_33[new V("nodes")][new V("every")].Call(VisualJs.Fn(args_34 => { V v_node_35 = VisualJs.Arg(args_34, 0);
return (!v_dominantNodes_27[new V("has")].Call(v_node_35)); }))); }));
V v_overlappingCandidate_36 = v_ranked_20[new V("find")].Call(VisualJs.Fn(args_37 => { V v_shape_38 = VisualJs.Arg(args_37, 0);
return VisualJs.And(new V(v_shape_38[new V("type")] != v_dominant_21[new V("type")]), () => new V(v_scoreGap_28.Call(v_shape_38) <= Scope[new V("SECONDARY_SCORE_GAP")])); }));
return VisualJs.Or(VisualJs.Or(v_disjointCandidate_31, () => v_overlappingCandidate_36), () => VisualJs.Null);
}
private static V F_selectActiveGeometry(V[] args) {
V v_ranked_39 = VisualJs.Arg(args,0);
V v_selection_40 = VisualJs.Arg(args,1);
V v_preservedDominant_41 = VisualJs.Arg(args,2);
V v_preserveSuppressedWhenUnavailable_42 = VisualJs.Arg(args,3).Undefined?new V(false):VisualJs.Arg(args,3);
V v_selectedDominant_43 = (VisualJs.And(VisualJs.Chain(v_selection_40,optional_44=>optional_44[new V("dominant")]), () => new V(v_selection_40[new V("dominant")] != new V("none"))).Truth ? VisualJs.Or(v_ranked_39[new V("find")].Call(VisualJs.Fn(args_45 => { V v_shape_46 = VisualJs.Arg(args_45, 0);
return new V(v_shape_46[new V("type")] == v_selection_40[new V("dominant")]); })), () => VisualJs.Null) : VisualJs.Null);
V v_rememberedSuppressed_47 = (VisualJs.And(new V(VisualJs.Chain(v_preservedDominant_41,optional_48=>optional_48[new V("active")]) == new V(false)), () => new V(v_preservedDominant_41[new V("inactiveReason")] == new V("network-disabled"))).Truth ? v_preservedDominant_41 : VisualJs.Null);
if (VisualJs.And(VisualJs.And(v_rememberedSuppressed_47, () => (!v_selectedDominant_43)), () => (VisualJs.Or((!VisualJs.Chain(v_selection_40,optional_49=>optional_49[new V("dominant")])), () => new V(v_selection_40[new V("dominant")] == v_rememberedSuppressed_47[new V("type")])))).Truth) {
V v_restored_50 = v_ranked_39[new V("find")].Call(VisualJs.Fn(args_51 => { V v_shape_52 = VisualJs.Arg(args_51, 0);
return new V(v_shape_52[new V("type")] == v_rememberedSuppressed_47[new V("type")]); }));
if (v_restored_50.Truth) {
return VisualJs.Obj(new V("dominant"), v_restored_50, new V("secondary"), Scope[new V("chooseSecondaryFormation")].Call(v_ranked_39, v_restored_50, v_selection_40));
}
if ((!v_preserveSuppressedWhenUnavailable_42).Truth) return VisualJs.Obj(new V("dominant"), VisualJs.Or(VisualJs.Or(v_selectedDominant_43, () => v_ranked_39[new V(0d)]), () => VisualJs.Null), new V("secondary"), (v_selectedDominant_43.Truth ? Scope[new V("chooseSecondaryFormation")].Call(v_ranked_39, v_selectedDominant_43, v_selection_40) : (VisualJs.Or(v_ranked_39[new V(1d)], () => VisualJs.Null))));
V v_inactiveDominant_53 = VisualJs.Merge(v_rememberedSuppressed_47, VisualJs.Obj(new V("active"),new V(false)), VisualJs.Obj(new V("inactiveReason"),new V("network-disabled")));
return VisualJs.Obj(new V("dominant"), v_inactiveDominant_53, new V("secondary"), VisualJs.Or(v_ranked_39[new V("find")].Call(VisualJs.Fn(args_54 => { V v_shape_55 = VisualJs.Arg(args_54, 0);
return new V(v_shape_55[new V("type")] != v_inactiveDominant_53[new V("type")]); })), () => VisualJs.Null));
}
V v_dominant_56 = VisualJs.Or(VisualJs.Or(v_selectedDominant_43, () => v_ranked_39[new V(0d)]), () => VisualJs.Null);
if ((!v_dominant_56).Truth) return VisualJs.Obj(new V("dominant"), VisualJs.Null, new V("secondary"), VisualJs.Null);
return VisualJs.Obj(new V("dominant"), v_dominant_56, new V("secondary"), Scope[new V("chooseSecondaryFormation")].Call(v_ranked_39, v_dominant_56, v_selection_40));
}
private static V F_getFormationBonusMultiplier(V[] args) {
V v_network_57 = VisualJs.Arg(args,0);
V v_type_58 = VisualJs.Arg(args,1);
V v_nodeIndex_59 = VisualJs.Arg(args,2);
V v_includesNode_60 = default; v_includesNode_60 = VisualJs.Fn(args_61 => { V v_shape_62 = VisualJs.Arg(args_61, 0);
return new V(VisualJs.And(v_shape_62, () => (VisualJs.Or(VisualJs.Or(new V(v_nodeIndex_59 == default(V)), () => new V(v_nodeIndex_59 < new V(0d))), () => v_shape_62[new V("nodes")][new V("includes")].Call(v_nodeIndex_59)))).Truth); });
V v_efficiency_63 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), VisualJs.Coalesce(v_network_57[new V("formationEfficiency")], () => new V(1d))));
if (VisualJs.And(VisualJs.And(new V(VisualJs.Chain(v_network_57[new V("dominantFormation")],optional_64=>optional_64[new V("active")]) != new V(false)), () => new V(VisualJs.Chain(v_network_57[new V("dominantFormation")],optional_65=>optional_65[new V("type")]) == v_type_58)), () => v_includesNode_60.Call(v_network_57[new V("dominantFormation")])).Truth) return v_efficiency_63;
if (VisualJs.And(new V(VisualJs.Chain(v_network_57[new V("secondaryFormation")],optional_66=>optional_66[new V("type")]) == v_type_58), () => v_includesNode_60.Call(v_network_57[new V("secondaryFormation")])).Truth) return (new V(0.5d) * v_efficiency_63);
return new V(0d);
}
private static V F_distance(V[] args) {
V v_a_67 = VisualJs.Arg(args,0);
V v_b_68 = VisualJs.Arg(args,1);
return VisualJs.MathObject[new V("hypot")].Call((v_a_67[new V("x")] - v_b_68[new V("x")]), (v_a_67[new V("y")] - v_b_68[new V("y")]));
}
private static V F_aliveIndexes(V[] args) {
V v_nodes_69 = VisualJs.Arg(args,0);
V v_result_70 = VisualJs.Arr();
for (V v_i_71 = new V(0d); new V(v_i_71 < v_nodes_69[new V("length")]).Truth; v_i_71++) {
if (new V(v_nodes_69[v_i_71][new V("alive")] != new V(false)).Truth) v_result_70[new V("push")].Call(v_i_71);
}
return v_result_70;
}
private static V F_combinations3(V[] args) {
V v_values_72 = VisualJs.Arg(args,0);
V v_result_73 = VisualJs.Arr();
for (V v_i_74 = new V(0d); new V(v_i_74 < (v_values_72[new V("length")] - new V(2d))).Truth; v_i_74++) {
for (V v_j_75 = (v_i_74 + new V(1d)); new V(v_j_75 < (v_values_72[new V("length")] - new V(1d))).Truth; v_j_75++) {
for (V v_k_76 = (v_j_75 + new V(1d)); new V(v_k_76 < v_values_72[new V("length")]).Truth; v_k_76++) {
v_result_73[new V("push")].Call(VisualJs.Arr(v_values_72[v_i_74], v_values_72[v_j_75], v_values_72[v_k_76]));
}
}
}
return v_result_73;
}
private static V F_lineStrength(V[] args) {
V v_nodes_77 = VisualJs.Arg(args,0);
V v_indexes_78 = VisualJs.Arg(args,1);
if (new V(v_indexes_78[new V("length")] < new V(3d)).Truth) return new V(0d);
V v_best_79 = new V(0d);
V v_centroid_80 = v_indexes_78[new V("reduce")].Call(VisualJs.Fn(args_81 => { V v_acc_82 = VisualJs.Arg(args_81, 0);
V v_index_83 = VisualJs.Arg(args_81, 1);
return (VisualJs.Obj(new V("x"), (v_acc_82[new V("x")] + v_nodes_77[v_index_83][new V("pos")][new V("x")]), new V("y"), (v_acc_82[new V("y")] + v_nodes_77[v_index_83][new V("pos")][new V("y")]))); }), VisualJs.Obj(new V("x"), new V(0d), new V("y"), new V(0d)));
VisualJs.Assign(v_centroid_80,new V("x"),"/=",v_indexes_78[new V("length")]);
VisualJs.Assign(v_centroid_80,new V("y"),"/=",v_indexes_78[new V("length")]);
for (V v_step_84 = new V(0d); new V(v_step_84 < new V(24d)).Truth; v_step_84++) {
V v_angle_85 = (((v_step_84 / new V(24d))) * VisualJs.MathObject[new V("PI")]);
V v_dirX_86 = VisualJs.MathObject[new V("cos")].Call(v_angle_85);
V v_dirY_87 = VisualJs.MathObject[new V("sin")].Call(v_angle_85);
V v_maxError_88 = new V(0d);
foreach (V v_index_89 in v_indexes_78.Items) {
V v_dx_90 = (v_nodes_77[v_index_89][new V("pos")][new V("x")] - v_centroid_80[new V("x")]);
V v_dy_91 = (v_nodes_77[v_index_89][new V("pos")][new V("y")] - v_centroid_80[new V("y")]);
v_maxError_88 = VisualJs.MathObject[new V("max")].Call(v_maxError_88, VisualJs.MathObject[new V("abs")].Call(((v_dx_90 * v_dirY_87) - (v_dy_91 * v_dirX_86))));
}
V v_strength_92 = (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (v_maxError_88 / new V(72d))));
v_best_79 = VisualJs.MathObject[new V("max")].Call(v_best_79, v_strength_92);
}
return v_best_79;
}
private static V F_triangleStrength(V[] args) {
V v_nodes_93 = VisualJs.Arg(args,0);
V v_indexes_94 = VisualJs.Arg(args,1);
V v_linkDistance_95 = VisualJs.Arg(args,2);
if (new V(v_indexes_94[new V("length")] < new V(3d)).Truth) return new V(0d);
V binding_96 = v_indexes_94;
V v_a_97 = binding_96[new V(0d)];
V v_b_98 = binding_96[new V(1d)];
V v_c_99 = binding_96[new V(2d)];

V v_ab_100 = Scope[new V("distance")].Call(v_nodes_93[v_a_97][new V("pos")], v_nodes_93[v_b_98][new V("pos")]);
V v_bc_101 = Scope[new V("distance")].Call(v_nodes_93[v_b_98][new V("pos")], v_nodes_93[v_c_99][new V("pos")]);
V v_ca_102 = Scope[new V("distance")].Call(v_nodes_93[v_c_99][new V("pos")], v_nodes_93[v_a_97][new V("pos")]);
if (VisualJs.Or(VisualJs.Or(new V(v_ab_100 > v_linkDistance_95), () => new V(v_bc_101 > v_linkDistance_95)), () => new V(v_ca_102 > v_linkDistance_95)).Truth) return new V(0d);
V v_mean_103 = ((((v_ab_100 + v_bc_101) + v_ca_102)) / new V(3d));
if (VisualJs.Or(new V(v_mean_103 < new V(65d)), () => new V(v_mean_103 > v_linkDistance_95)).Truth) return new V(0d);
V v_variance_104 = ((((VisualJs.MathObject[new V("abs")].Call((v_ab_100 - v_mean_103)) + VisualJs.MathObject[new V("abs")].Call((v_bc_101 - v_mean_103))) + VisualJs.MathObject[new V("abs")].Call((v_ca_102 - v_mean_103)))) / ((new V(3d) * v_mean_103)));
return (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (v_variance_104 * new V(2.2d))));
}
private static V F_squareStrength(V[] args) {
V v_nodes_105 = VisualJs.Arg(args,0);
V v_indexes_106 = VisualJs.Arg(args,1);
V v_linkDistance_107 = VisualJs.Arg(args,2);
if (new V(v_indexes_106[new V("length")] != new V(4d)).Truth) return new V(0d);
V v_points_108 = v_indexes_106[new V("map")].Call(VisualJs.Fn(args_109 => { V v_index_110 = VisualJs.Arg(args_109, 0);
return (VisualJs.Obj(new V("index"), v_index_110, new V("pos"), v_nodes_105[v_index_110][new V("pos")])); }));
V v_center_111 = v_points_108[new V("reduce")].Call(VisualJs.Fn(args_112 => { V v_acc_113 = VisualJs.Arg(args_112, 0);
V v_p_114 = VisualJs.Arg(args_112, 1);
return (VisualJs.Obj(new V("x"), (v_acc_113[new V("x")] + v_p_114[new V("pos")][new V("x")]), new V("y"), (v_acc_113[new V("y")] + v_p_114[new V("pos")][new V("y")]))); }), VisualJs.Obj(new V("x"), new V(0d), new V("y"), new V(0d)));
VisualJs.Assign(v_center_111,new V("x"),"/=",new V(4d));
VisualJs.Assign(v_center_111,new V("y"),"/=",new V(4d));
V v_ordered_115 = VisualJs.Concat(v_points_108)[new V("sort")].Call(VisualJs.Fn(args_116 => { V v_a_117 = VisualJs.Arg(args_116, 0);
V v_b_118 = VisualJs.Arg(args_116, 1);
return (VisualJs.MathObject[new V("atan2")].Call((v_a_117[new V("pos")][new V("y")] - v_center_111[new V("y")]), (v_a_117[new V("pos")][new V("x")] - v_center_111[new V("x")])) - VisualJs.MathObject[new V("atan2")].Call((v_b_118[new V("pos")][new V("y")] - v_center_111[new V("y")]), (v_b_118[new V("pos")][new V("x")] - v_center_111[new V("x")]))); }));
V v_sides_119 = VisualJs.Arr(new V(0d), new V(1d), new V(2d), new V(3d))[new V("map")].Call(VisualJs.Fn(args_120 => { V v_i_121 = VisualJs.Arg(args_120, 0);
return Scope[new V("distance")].Call(v_ordered_115[v_i_121][new V("pos")], v_ordered_115[(((v_i_121 + new V(1d))) % new V(4d))][new V("pos")]); }));
V v_diagonals_122 = VisualJs.Arr(Scope[new V("distance")].Call(v_ordered_115[new V(0d)][new V("pos")], v_ordered_115[new V(2d)][new V("pos")]), Scope[new V("distance")].Call(v_ordered_115[new V(1d)][new V("pos")], v_ordered_115[new V(3d)][new V("pos")]));
V v_sideMean_123 = (v_sides_119[new V("reduce")].Call(VisualJs.Fn(args_124 => { V v_a_125 = VisualJs.Arg(args_124, 0);
V v_b_126 = VisualJs.Arg(args_124, 1);
return (v_a_125 + v_b_126); }), new V(0d)) / new V(4d));
if (VisualJs.Or(VisualJs.Or(new V(v_sideMean_123 < new V(60d)), () => new V(v_sideMean_123 > v_linkDistance_107)), () => v_sides_119[new V("some")].Call(VisualJs.Fn(args_127 => { V v_side_128 = VisualJs.Arg(args_127, 0);
return new V(v_side_128 > v_linkDistance_107); }))).Truth) return new V(0d);
V v_sideVariance_129 = (v_sides_119[new V("reduce")].Call(VisualJs.Fn(args_130 => { V v_sum_131 = VisualJs.Arg(args_130, 0);
V v_side_132 = VisualJs.Arg(args_130, 1);
return (v_sum_131 + VisualJs.MathObject[new V("abs")].Call((v_side_132 - v_sideMean_123))); }), new V(0d)) / ((new V(4d) * v_sideMean_123)));
V v_diagonalMean_133 = (((v_diagonals_122[new V(0d)] + v_diagonals_122[new V(1d)])) / new V(2d));
V v_diagonalVariance_134 = (VisualJs.MathObject[new V("abs")].Call((v_diagonals_122[new V(0d)] - v_diagonals_122[new V(1d)])) / VisualJs.MathObject[new V("max")].Call(new V(1d), v_diagonalMean_133));
V v_rightAngleError_135 = (VisualJs.MathObject[new V("abs")].Call(((v_diagonalMean_133 / v_sideMean_123) - VisualJs.MathObject[new V("SQRT2")])) / VisualJs.MathObject[new V("SQRT2")]);
return VisualJs.MathObject[new V("max")].Call(new V(0d), (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (((v_sideVariance_129 * new V(2d)) + v_diagonalVariance_134) + v_rightAngleError_135))));
}
private static V F_clusterStrength(V[] args) {
V v_nodes_136 = VisualJs.Arg(args,0);
V v_indexes_137 = VisualJs.Arg(args,1);
V v_linkDistance_138 = VisualJs.Arg(args,2);
if (new V(v_indexes_137[new V("length")] < new V(4d)).Truth) return new V(0d);
V v_sum_139 = new V(0d);
V v_pairs_140 = new V(0d);
V v_linkedPairs_141 = new V(0d);
for (V v_i_142 = new V(0d); new V(v_i_142 < v_indexes_137[new V("length")]).Truth; v_i_142++) {
for (V v_j_143 = (v_i_142 + new V(1d)); new V(v_j_143 < v_indexes_137[new V("length")]).Truth; v_j_143++) {
V v_d_144 = Scope[new V("distance")].Call(v_nodes_136[v_indexes_137[v_i_142]][new V("pos")], v_nodes_136[v_indexes_137[v_j_143]][new V("pos")]);
v_sum_139 += v_d_144;
v_pairs_140++;
if (new V(v_d_144 <= v_linkDistance_138).Truth) v_linkedPairs_141++;
}
}
V v_average_145 = (v_sum_139 / VisualJs.MathObject[new V("max")].Call(new V(1d), v_pairs_140));
V v_compactness_146 = (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (VisualJs.MathObject[new V("max")].Call(new V(0d), (v_average_145 - new V(90d))) / new V(90d))));
V v_connectivity_147 = (v_linkedPairs_141 / VisualJs.MathObject[new V("max")].Call(new V(1d), v_pairs_140));
return ((v_compactness_146 * new V(0.65d)) + (v_connectivity_147 * new V(0.35d)));
}
private static V F_ringShape(V[] args) {
V v_nodes_148 = VisualJs.Arg(args,0);
V v_indexes_149 = VisualJs.Arg(args,1);
V v_linkDistance_150 = VisualJs.Arg(args,2);
if (new V(v_indexes_149[new V("length")] < new V(4d)).Truth) return VisualJs.Null;
V v_center_151 = v_indexes_149[new V("reduce")].Call(VisualJs.Fn(args_152 => { V v_acc_153 = VisualJs.Arg(args_152, 0);
V v_index_154 = VisualJs.Arg(args_152, 1);
return (VisualJs.Obj(new V("x"), (v_acc_153[new V("x")] + v_nodes_148[v_index_154][new V("pos")][new V("x")]), new V("y"), (v_acc_153[new V("y")] + v_nodes_148[v_index_154][new V("pos")][new V("y")]))); }), VisualJs.Obj(new V("x"), new V(0d), new V("y"), new V(0d)));
VisualJs.Assign(v_center_151,new V("x"),"/=",v_indexes_149[new V("length")]);
VisualJs.Assign(v_center_151,new V("y"),"/=",v_indexes_149[new V("length")]);
V v_ordered_155 = VisualJs.Concat(v_indexes_149)[new V("sort")].Call(VisualJs.Fn(args_156 => { V v_a_157 = VisualJs.Arg(args_156, 0);
V v_b_158 = VisualJs.Arg(args_156, 1);
return (VisualJs.MathObject[new V("atan2")].Call((v_nodes_148[v_a_157][new V("pos")][new V("y")] - v_center_151[new V("y")]), (v_nodes_148[v_a_157][new V("pos")][new V("x")] - v_center_151[new V("x")])) - VisualJs.MathObject[new V("atan2")].Call((v_nodes_148[v_b_158][new V("pos")][new V("y")] - v_center_151[new V("y")]), (v_nodes_148[v_b_158][new V("pos")][new V("x")] - v_center_151[new V("x")]))); }));
V v_distances_159 = VisualJs.Arr();
for (V v_i_160 = new V(0d); new V(v_i_160 < v_ordered_155[new V("length")]).Truth; v_i_160++) {
V v_a_161 = v_ordered_155[v_i_160];
V v_b_162 = v_ordered_155[(((v_i_160 + new V(1d))) % v_ordered_155[new V("length")])];
V v_d_163 = Scope[new V("distance")].Call(v_nodes_148[v_a_161][new V("pos")], v_nodes_148[v_b_162][new V("pos")]);
if (new V(v_d_163 > v_linkDistance_150).Truth) return VisualJs.Null;
v_distances_159[new V("push")].Call(v_d_163);
}
V v_mean_164 = (v_distances_159[new V("reduce")].Call(VisualJs.Fn(args_165 => { V v_a_166 = VisualJs.Arg(args_165, 0);
V v_b_167 = VisualJs.Arg(args_165, 1);
return (v_a_166 + v_b_167); }), new V(0d)) / v_distances_159[new V("length")]);
if (new V(v_mean_164 < new V(55d)).Truth) return VisualJs.Null;
V v_variance_168 = (v_distances_159[new V("reduce")].Call(VisualJs.Fn(args_169 => { V v_sum_170 = VisualJs.Arg(args_169, 0);
V v_d_171 = VisualJs.Arg(args_169, 1);
return (v_sum_170 + VisualJs.MathObject[new V("abs")].Call((v_d_171 - v_mean_164))); }), new V(0d)) / ((v_distances_159[new V("length")] * v_mean_164)));
V v_strength_172 = (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (v_variance_168 * new V(2d))));
return (new V(v_strength_172 >= new V(0.68d)).Truth ? VisualJs.Obj(new V("type"), new V("ring"), new V("strength"), v_strength_172, new V("nodes"), v_ordered_155) : VisualJs.Null);
}
private static V F_isNodeSetConnected(V[] args) {
V v_indexes_173 = VisualJs.Arg(args,0);
V v_links_174 = VisualJs.Arg(args,1);
if (new V(v_indexes_173[new V("length")] <= new V(1d)).Truth) return new V(true);
V v_allowed_175 = VisualJs.NewSet(v_indexes_173);
V v_visited_176 = VisualJs.NewSet(VisualJs.Arr(v_indexes_173[new V(0d)]));
V v_queue_177 = VisualJs.Arr(v_indexes_173[new V(0d)]);
while (new V(v_queue_177[new V("length")] > new V(0d)).Truth) {
V v_current_178 = (v_queue_177[new V("shift")].Call());
foreach (V v_link_179 in v_links_174.Items) {
V v_next_180 = (new V(v_link_179[new V("a")] == v_current_178).Truth ? v_link_179[new V("b")] : (new V(v_link_179[new V("b")] == v_current_178).Truth ? v_link_179[new V("a")] : (-new V(1d))));
if (VisualJs.And(VisualJs.And(new V(v_next_180 >= new V(0d)), () => v_allowed_175[new V("has")].Call(v_next_180)), () => (!v_visited_176[new V("has")].Call(v_next_180))).Truth) {
v_visited_176[new V("add")].Call(v_next_180);
v_queue_177[new V("push")].Call(v_next_180);
}
}
}
return new V(v_visited_176[new V("size")] == v_allowed_175[new V("size")]);
}
private static V F_latticeShape(V[] args) {
V v_nodes_181 = VisualJs.Arg(args,0);
V v_indexes_182 = VisualJs.Arg(args,1);
V v_links_183 = VisualJs.Arg(args,2);
V v_linkDistance_184 = VisualJs.Arg(args,3);
if (new V(v_indexes_182[new V("length")] < new V(4d)).Truth) return VisualJs.Null;
V v_triangles_185 = VisualJs.Arr();
V v_linked_186 = default; v_linked_186 = VisualJs.Fn(args_187 => { V v_a_188 = VisualJs.Arg(args_187, 0);
V v_b_189 = VisualJs.Arg(args_187, 1);
return v_links_183[new V("some")].Call(VisualJs.Fn(args_190 => { V v_link_191 = VisualJs.Arg(args_190, 0);
return VisualJs.Or((VisualJs.And(new V(v_link_191[new V("a")] == v_a_188), () => new V(v_link_191[new V("b")] == v_b_189))), () => (VisualJs.And(new V(v_link_191[new V("a")] == v_b_189), () => new V(v_link_191[new V("b")] == v_a_188)))); })); });
for (V v_i_192 = new V(0d); new V(v_i_192 < (v_indexes_182[new V("length")] - new V(2d))).Truth; v_i_192++) for (V v_j_193 = (v_i_192 + new V(1d)); new V(v_j_193 < (v_indexes_182[new V("length")] - new V(1d))).Truth; v_j_193++) for (V v_k_194 = (v_j_193 + new V(1d)); new V(v_k_194 < v_indexes_182[new V("length")]).Truth; v_k_194++) {
V v_combo_195 = VisualJs.Arr(v_indexes_182[v_i_192], v_indexes_182[v_j_193], v_indexes_182[v_k_194]);
if (VisualJs.And(VisualJs.And(VisualJs.And(v_linked_186.Call(v_combo_195[new V(0d)], v_combo_195[new V(1d)]), () => v_linked_186.Call(v_combo_195[new V(1d)], v_combo_195[new V(2d)])), () => v_linked_186.Call(v_combo_195[new V(0d)], v_combo_195[new V(2d)])), () => new V(Scope[new V("triangleStrength")].Call(v_nodes_181, v_combo_195, v_linkDistance_184) >= new V(0.82d))).Truth) v_triangles_185[new V("push")].Call(v_combo_195);
}
if (new V(v_triangles_185[new V("length")] < new V(2d)).Truth) return VisualJs.Null;
V v_shared_196 = new V(false);
for (V v_i_197 = new V(0d); new V(v_i_197 < v_triangles_185[new V("length")]).Truth; v_i_197++) for (V v_j_198 = (v_i_197 + new V(1d)); new V(v_j_198 < v_triangles_185[new V("length")]).Truth; v_j_198++) {
V v_common_199 = v_triangles_185[v_i_197][new V("filter")].Call(VisualJs.Fn(args_200 => { V v_value_201 = VisualJs.Arg(args_200, 0);
return v_triangles_185[v_j_198][new V("includes")].Call(v_value_201); }));
if (new V(v_common_199[new V("length")] >= new V(2d)).Truth) {
v_shared_196 = new V(true);
goto label_outer;
}
}
label_outer: ;
if ((!v_shared_196).Truth) return VisualJs.Null;
V v_strength_202 = VisualJs.MathObject[new V("min")].Call(new V(1d), (new V(0.55d) + VisualJs.MathObject[new V("min")].Call(new V(0.45d), (v_triangles_185[new V("length")] * new V(0.08d)))));
V v_used_203 = VisualJs.Concat(VisualJs.NewSet(v_triangles_185[new V("flat")].Call()));
return VisualJs.Obj(new V("type"), new V("lattice"), new V("strength"), v_strength_202, new V("nodes"), v_used_203);
}
private static V F_analyzeSphereNetwork(V[] args) {
V v_nodes_204 = VisualJs.Arg(args,0);
V v_linkDistance_205 = VisualJs.Arg(args,1).Undefined?Scope[new V("DEFAULT_LINK_DISTANCE")]:VisualJs.Arg(args,1);
V v_previousDominant_206 = VisualJs.Arg(args,2).Undefined?new V("none"):VisualJs.Arg(args,2);
V v_selection_207 = VisualJs.Arg(args,3);
V v_preservedDominant_208 = VisualJs.Arg(args,4);
V v_preserveSuppressedWhenUnavailable_209 = VisualJs.Arg(args,5).Undefined?new V(false):VisualJs.Arg(args,5);
V v_indexes_210 = Scope[new V("aliveIndexes")].Call(v_nodes_204);
V v_links_211 = VisualJs.Arr();
for (V v_i_212 = new V(0d); new V(v_i_212 < v_indexes_210[new V("length")]).Truth; v_i_212++) {
for (V v_j_213 = (v_i_212 + new V(1d)); new V(v_j_213 < v_indexes_210[new V("length")]).Truth; v_j_213++) {
V v_a_214 = v_indexes_210[v_i_212];
V v_b_215 = v_indexes_210[v_j_213];
V v_d_216 = Scope[new V("distance")].Call(v_nodes_204[v_a_214][new V("pos")], v_nodes_204[v_b_215][new V("pos")]);
if (new V(v_d_216 <= v_linkDistance_205).Truth) v_links_211[new V("push")].Call(VisualJs.Obj(new V("a"), v_a_214, new V("b"), v_b_215, new V("distance"), v_d_216));
}
}
V v_line_217 = (new V(v_indexes_210[new V("length")] >= new V(3d)).Truth ? (VisualJs.Fn(args_218 => { V v_wholeStrength_219 = Scope[new V("lineStrength")].Call(v_nodes_204, v_indexes_210);
if (new V(v_wholeStrength_219 >= new V(0.8d)).Truth) {
return VisualJs.Obj(new V("type"), (new V("line")), new V("strength"), v_wholeStrength_219, new V("nodes"), VisualJs.Concat(v_indexes_210));
}
V v_best_220 = VisualJs.Null;
foreach (V v_combo_221 in Scope[new V("combinations3")].Call(v_indexes_210).Items) {
V v_strength_222 = Scope[new V("lineStrength")].Call(v_nodes_204, v_combo_221);
if (VisualJs.And(new V(v_strength_222 >= new V(0.8d)), () => (VisualJs.Or((!v_best_220), () => new V(v_strength_222 > v_best_220[new V("strength")])))).Truth) {
v_best_220 = VisualJs.Obj(new V("type"), (new V("line")), new V("strength"), v_strength_222, new V("nodes"), VisualJs.Concat(v_combo_221));
}
}
return v_best_220; })).Call() : VisualJs.Null);
V v_triangle_223 = VisualJs.Null;
foreach (V v_combo_224 in Scope[new V("combinations3")].Call(v_indexes_210).Items) {
V v_strength_225 = Scope[new V("triangleStrength")].Call(v_nodes_204, v_combo_224, v_linkDistance_205);
if (new V(v_strength_225 < new V(0.82d)).Truth) continue;
if (VisualJs.Or((!v_triangle_223), () => new V(v_strength_225 > v_triangle_223[new V("strength")])).Truth) {
v_triangle_223 = VisualJs.Obj(new V("type"), new V("triangle"), new V("strength"), v_strength_225, new V("nodes"), VisualJs.Concat(v_combo_224));
}
}
V v_square_226 = (VisualJs.Fn(args_227 => { V v_best_228 = VisualJs.Null;
for (V v_i_229 = new V(0d); new V(v_i_229 < (v_indexes_210[new V("length")] - new V(3d))).Truth; v_i_229++) for (V v_j_230 = (v_i_229 + new V(1d)); new V(v_j_230 < (v_indexes_210[new V("length")] - new V(2d))).Truth; v_j_230++) for (V v_k_231 = (v_j_230 + new V(1d)); new V(v_k_231 < (v_indexes_210[new V("length")] - new V(1d))).Truth; v_k_231++) for (V v_l_232 = (v_k_231 + new V(1d)); new V(v_l_232 < v_indexes_210[new V("length")]).Truth; v_l_232++) {
V v_combo_233 = VisualJs.Arr(v_indexes_210[v_i_229], v_indexes_210[v_j_230], v_indexes_210[v_k_231], v_indexes_210[v_l_232]);
V v_strength_234 = Scope[new V("squareStrength")].Call(v_nodes_204, v_combo_233, v_linkDistance_205);
if (VisualJs.And(new V(v_strength_234 >= new V(0.84d)), () => (VisualJs.Or((!v_best_228), () => new V(v_strength_234 > v_best_228[new V("strength")])))).Truth) v_best_228 = VisualJs.Obj(new V("type"), new V("square"), new V("strength"), v_strength_234, new V("nodes"), v_combo_233);
}
return v_best_228; })).Call();
V v_cluster_235 = (VisualJs.Fn(args_236 => { V v_strength_237 = Scope[new V("clusterStrength")].Call(v_nodes_204, v_indexes_210, v_linkDistance_205);
return (new V(v_strength_237 >= new V(0.78d)).Truth ? VisualJs.Obj(new V("type"), (new V("cluster")), new V("strength"), v_strength_237, new V("nodes"), VisualJs.Concat(v_indexes_210)) : VisualJs.Null); })).Call();
V v_ring_238 = Scope[new V("ringShape")].Call(v_nodes_204, v_indexes_210, v_linkDistance_205);
V v_lattice_239 = Scope[new V("latticeShape")].Call(v_nodes_204, v_indexes_210, v_links_211, v_linkDistance_205);
V v_fractalBase_240 = (VisualJs.Arr(v_ring_238, v_lattice_239, v_square_226, v_triangle_223)[new V("filter")].Call(VisualJs.Fn(a=>new V(VisualJs.Arg(a,0).Truth))));
V v_fractalNodes_241 = VisualJs.Concat(VisualJs.NewSet(v_fractalBase_240[new V("flatMap")].Call(VisualJs.Fn(args_242 => { V v_shape_243 = VisualJs.Arg(args_242, 0);
return v_shape_243[new V("nodes")]; }))));
V v_fractal_244 = (VisualJs.And(new V(v_fractalBase_240[new V("length")] >= new V(2d)), () => Scope[new V("isNodeSetConnected")].Call(v_fractalNodes_241, v_links_211)).Truth ? VisualJs.Obj(new V("type"), (new V("fractal")), new V("strength"), VisualJs.MathObject[new V("min")].Call(new V(1d), (v_fractalBase_240[new V("reduce")].Call(VisualJs.Fn(args_245 => { V v_sum_246 = VisualJs.Arg(args_245, 0);
V v_shape_247 = VisualJs.Arg(args_245, 1);
return (v_sum_246 + v_shape_247[new V("strength")]); }), new V(0d)) / v_fractalBase_240[new V("length")])), new V("nodes"), v_fractalNodes_241) : VisualJs.Null);
V v_candidates_248 = VisualJs.Obj(new V("fractal"), v_fractal_244, new V("lattice"), v_lattice_239, new V("ring"), v_ring_238, new V("square"), v_square_226, new V("triangle"), v_triangle_223, new V("cluster"), v_cluster_235, new V("line"), v_line_217);
V v_ranked_249 = Scope[new V("rankGeometryCandidates")].Call(v_candidates_248, v_previousDominant_206);
V v_active_250 = Scope[new V("selectActiveGeometry")].Call(v_ranked_249, v_selection_207, v_preservedDominant_208, v_preserveSuppressedWhenUnavailable_209);
V v_activeTypes_251 = VisualJs.NewSet(VisualJs.Arr(v_active_250[new V("dominant")], v_active_250[new V("secondary")])[new V("filter")].Call(VisualJs.Fn(args_252 => { V v_shape_253 = VisualJs.Arg(args_252, 0);
return new V(v_shape_253.Truth); }))[new V("map")].Call(VisualJs.Fn(args_254 => { V v_shape_255 = VisualJs.Arg(args_254, 0);
return v_shape_255[new V("type")]; })));
V v_byType_256 = default; v_byType_256 = VisualJs.Fn(args_257 => { V v_type_258 = VisualJs.Arg(args_257, 0);
return (v_activeTypes_251[new V("has")].Call(v_type_258).Truth ? VisualJs.Or(v_ranked_249[new V("find")].Call(VisualJs.Fn(args_259 => { V v_shape_260 = VisualJs.Arg(args_259, 0);
return new V(v_shape_260[new V("type")] == v_type_258); })), () => VisualJs.Null) : VisualJs.Null); });
return VisualJs.Obj(new V("linkDistance"), v_linkDistance_205, new V("nodes"), v_indexes_210, new V("links"), v_links_211, new V("formationEfficiency"), new V(1d), new V("formationCandidates"), v_ranked_249, new V("dominantFormation"), v_active_250[new V("dominant")], new V("secondaryFormation"), v_active_250[new V("secondary")], new V("line"), v_byType_256.Call(new V("line")), new V("triangle"), v_byType_256.Call(new V("triangle")), new V("square"), v_byType_256.Call(new V("square")), new V("cluster"), v_byType_256.Call(new V("cluster")), new V("ring"), v_byType_256.Call(new V("ring")), new V("lattice"), v_byType_256.Call(new V("lattice")), new V("fractal"), v_byType_256.Call(new V("fractal")));
}
private static V F_getSphereNetworkProfile(V[] args) {
V v_state_261 = VisualJs.Arg(args,0);
V v_sphereIndex_262 = VisualJs.Arg(args,1);
V v_linkedNeighbours_263 = new V(0d);
foreach (V v_link_264 in v_state_261[new V("links")].Items) {
if (VisualJs.Or(new V(v_link_264[new V("a")] == v_sphereIndex_262), () => new V(v_link_264[new V("b")] == v_sphereIndex_262)).Truth) v_linkedNeighbours_263++;
}
return VisualJs.Obj(new V("linkedNeighbours"), v_linkedNeighbours_263, new V("line"), new V(VisualJs.Chain(v_state_261[new V("line")],optional_265=>optional_265[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("triangle"), new V(VisualJs.Chain(v_state_261[new V("triangle")],optional_266=>optional_266[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("square"), new V(VisualJs.Chain(v_state_261[new V("square")],optional_267=>optional_267[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("cluster"), new V(VisualJs.Chain(v_state_261[new V("cluster")],optional_268=>optional_268[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("ring"), new V(VisualJs.Chain(v_state_261[new V("ring")],optional_269=>optional_269[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("lattice"), new V(VisualJs.Chain(v_state_261[new V("lattice")],optional_270=>optional_270[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth), new V("fractal"), new V(VisualJs.Chain(v_state_261[new V("fractal")],optional_271=>optional_271[new V("nodes")][new V("includes")].Call(v_sphereIndex_262)).Truth));
}
}
}
#endif
