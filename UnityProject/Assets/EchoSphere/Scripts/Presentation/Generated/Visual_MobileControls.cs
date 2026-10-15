// Generated from src/MobileControls.tsx; sha256 0e11e0339075151f0e5f1e37254fcff045b2842b218c0da7cbf0262c1f9b8bad.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_MobileControls {
public static readonly V Scope=VisualJs.Obj();
static Visual_MobileControls() {
Scope[new V("haptic")] = VisualJs.Fn(F_haptic);
Scope[new V("isBlockedByControl")] = VisualJs.Fn(F_isBlockedByControl);
Scope[new V("MobileControls")] = VisualJs.Fn(F_MobileControls);
Scope[new V("SphereTypeGlyph")] = VisualJs.Fn(F_SphereTypeGlyph);
Scope[new V("worldToScreen")] = VisualJs.Fn(F_worldToScreen);
Scope[new V("GhostSnapOverlay")] = VisualJs.Fn(F_GhostSnapOverlay);
Scope[new V("FormationMemoryOverlay")] = VisualJs.Fn(F_FormationMemoryOverlay);
Scope[new V("getAbilityCooldown")] = VisualJs.Fn(F_getAbilityCooldown);
Scope[new V("getAbilityMaxCooldown")] = VisualJs.Fn(F_getAbilityMaxCooldown);
Scope[new V("stDashLabel")] = VisualJs.Fn(F_stDashLabel);
Scope[new V("JOYSTICK_DEADZONE")] = new V(12d);
Scope[new V("JOYSTICK_RADIUS")] = new V(58d);
Scope[new V("JOYSTICK_KNOB_RADIUS")] = new V(24d);
Scope[new V("sphere_TOUCH_TOLERANCE")] = new V(26d);
}
private static V F_haptic(V[] args) {
V v_duration_0 = VisualJs.Arg(args,0).Undefined?new V(8d):VisualJs.Arg(args,0);
if (VisualJs.And(new V(VisualJs.TypeOf(VisualUiBindings.Get("global","navigator")) != new V("undefined")), () => new V(VisualJs.TypeOf(VisualUiBindings.Get("global","navigator")[new V("vibrate")]) == new V("function"))).Truth) VisualUiBindings.Get("global","navigator")[new V("vibrate")].Call(v_duration_0);
return default;
}
private static V F_isBlockedByControl(V[] args) {
V v_target_1 = VisualJs.Arg(args,0);
return VisualJs.And(VisualUiBindings.Get("global","isElement").Call(v_target_1), () => new V(v_target_1[new V("closest")].Call(new V("[data-mobile-control=\"true\"]")).Truth));
return default;
}
private static V F_MobileControls(V[] args) {
V binding_2 = VisualJs.Arg(args,0);
V v_lang_3 = binding_2[new V("lang")];
V v_t_4 = binding_2[new V("t")];
V v_stateRef_5 = binding_2[new V("stateRef")];
V v_canvasRef_6 = binding_2[new V("canvasRef")];
V v_handedness_7 = binding_2[new V("handedness")];
V v_onPause_8 = binding_2[new V("onPause")];
V v_tutorialStep_9 = binding_2[new V("tutorialStep")];
V v_pointersRef_10 = VisualDom.React("useRef").Call(VisualJs.CreateMap(default(V),false));
V v_joystickIdRef_11 = VisualDom.React("useRef").Call(VisualJs.Null);
V v_lastDirectionRef_12 = VisualDom.React("useRef").Call(VisualJs.Obj(new V("x"), new V(0d), new V("y"), (-new V(1d))));
V binding_13 = VisualDom.React("useState").Call(VisualJs.Null);
V v_joystick_14 = binding_13[new V(0d)];
V v_setJoystick_15 = binding_13[new V(1d)];

V binding_16 = VisualDom.React("useState").Call(VisualJs.Null);
V v_placementFx_17 = binding_16[new V(0d)];
V v_setPlacementFx_18 = binding_16[new V(1d)];

V binding_19 = VisualDom.React("useState").Call(VisualJs.Null);
V v_ghostPreview_20 = binding_19[new V(0d)];
V v_setGhostPreview_21 = binding_19[new V(1d)];

V binding_22 = VisualDom.React("useState").Call(VisualJs.Null);
V v_formationMemory_23 = binding_22[new V(0d)];
V v_setFormationMemory_24 = binding_22[new V(1d)];

V v_interfaceScale_25 = Visual_interfaceScale.Scope[new V("loadInterfaceScale")].Call();
V v_joystickOnRight_26 = new V(v_handedness_7 == new V("right"));
V v_controlsOnRight_27 = (!v_joystickOnRight_26);
V v_controlsSide_28 = (v_controlsOnRight_27.Truth ? new V("right-3") : new V("left-3"));
V v_controlsAlign_29 = (v_controlsOnRight_27.Truth ? new V("items-end") : new V("items-start"));
V v_joystickZoneStart_30 = (v_joystickOnRight_26.Truth ? (VisualUiBindings.Get("global","window")[new V("innerWidth")] * new V(0.5d)) : new V(0d));
V v_joystickZoneEnd_31 = (v_joystickOnRight_26.Truth ? VisualUiBindings.Get("global","window")[new V("innerWidth")] : (VisualUiBindings.Get("global","window")[new V("innerWidth")] * new V(0.5d)));
V v_clearMovementKeys_32 = default; v_clearMovementKeys_32 = VisualJs.Fn(args_33 => { V v_st_34 = v_stateRef_5[new V("current")];
if ((!v_st_34).Truth) return default;
VisualJs.Assign(v_st_34[new V("keys")],new V("w"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("a"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("s"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("d"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("arrowup"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("arrowleft"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("arrowdown"),"=",new V(false));
VisualJs.Assign(v_st_34[new V("keys")],new V("arrowright"),"=",new V(false));
return default; });
V v_applyDirection_35 = default; v_applyDirection_35 = VisualJs.Fn(args_36 => { V v_dx_37 = VisualJs.Arg(args_36, 0);
V v_dy_38 = VisualJs.Arg(args_36, 1);
V v_st_39 = v_stateRef_5[new V("current")];
if ((!v_st_39).Truth) return default;
V v_length_40 = VisualJs.MathObject[new V("hypot")].Call(v_dx_37, v_dy_38);
if (new V(v_length_40 < Scope[new V("JOYSTICK_DEADZONE")]).Truth) {
v_clearMovementKeys_32.Call();
return default;
}
V v_nx_41 = (v_dx_37 / v_length_40);
V v_ny_42 = (v_dy_38 / v_length_40);
VisualJs.Assign(v_lastDirectionRef_12,new V("current"),"=",VisualJs.Obj(new V("x"), v_nx_41, new V("y"), v_ny_42));
VisualJs.Assign(v_st_39[new V("keys")],new V("w"),"=",new V(v_ny_42 < (-new V(0.2d))));
VisualJs.Assign(v_st_39[new V("keys")],new V("s"),"=",new V(v_ny_42 > new V(0.2d)));
VisualJs.Assign(v_st_39[new V("keys")],new V("a"),"=",new V(v_nx_41 < (-new V(0.2d))));
VisualJs.Assign(v_st_39[new V("keys")],new V("d"),"=",new V(v_nx_41 > new V(0.2d)));
return default; });
V v_touchToWorld_43 = default; v_touchToWorld_43 = VisualJs.Fn(args_44 => { V v_clientX_45 = VisualJs.Arg(args_44, 0);
V v_clientY_46 = VisualJs.Arg(args_44, 1);
V v_st_47 = v_stateRef_5[new V("current")];
V v_canvas_48 = v_canvasRef_6[new V("current")];
if (VisualJs.Or((!v_st_47), () => (!v_canvas_48)).Truth) return VisualJs.Null;
V v_rect_49 = v_canvas_48[new V("getBoundingClientRect")].Call();
if (VisualJs.Or(new V(v_rect_49[new V("width")] <= new V(0d)), () => new V(v_rect_49[new V("height")] <= new V(0d))).Truth) return VisualJs.Null;
V v_logicalX_50 = (v_clientX_45 - v_rect_49[new V("left")]);
V v_logicalY_51 = (v_clientY_46 - v_rect_49[new V("top")]);
return Visual_renderScale.Scope[new V("cssPointToWorld")].Call(v_clientX_45, v_clientY_46, v_rect_49, v_st_47[new V("camera")][new V("x")], v_st_47[new V("camera")][new V("y")]);
return default; });
V v_buildGhostPreview_52 = default; v_buildGhostPreview_52 = VisualJs.Fn(args_53 => { V v_st_54 = VisualJs.Arg(args_53, 0);
V v_sphere_55 = VisualJs.Arg(args_53, 1);
V v_sphereIndex_56 = VisualJs.Arg(args_53, 2);
V v_x_57 = VisualJs.Arg(args_53, 3);
V v_y_58 = VisualJs.Arg(args_53, 4);
V v_preview_59 = Visual_networkPreview.Scope[new V("buildGhostSnapPreview")].Call(v_st_54[new V("spheres")], VisualJs.Obj(new V("x"), v_x_57, new V("y"), v_y_58), default(V), v_sphereIndex_56);
V v_network_60 = VisualJs.Or(VisualJs.Chain(v_preview_59,optional_61=>optional_61[new V("network")]), () => Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_54[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_62 => { V v_current_63 = VisualJs.Arg(args_62, 0);
return (VisualJs.Obj(new V("pos"), v_current_63[new V("pos")], new V("alive"), v_current_63[new V("alive")], new V("networkDisabledTimer"), v_current_63[new V("networkDisabledTimer")])); }))));
return VisualJs.Obj(new V("pointerId"), (-new V(1d)), new V("sphereIndex"), v_sphereIndex_56, new V("x"), v_x_57, new V("y"), v_y_58, new V("valid"), Visual_spaceCollision.Scope[new V("canRepositionSphere")].Call(v_st_54, v_sphere_55, v_x_57, v_y_58), new V("network"), v_network_60, new V("formation"), (VisualJs.Chain(v_preview_59,optional_64=>optional_64[new V("formation")]).Truth ? (Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_network_60).Truth ? VisualJs.Obj(new V("type"), v_preview_59[new V("formation")][new V("type")], new V("nodes"), v_preview_59[new V("formation")][new V("nodes")], new V("strength"), v_preview_59[new V("formation")][new V("strength")]) : VisualJs.Null) : VisualJs.Null));
return default; });
V v_buildNewSphereGhostPreview_65 = default; v_buildNewSphereGhostPreview_65 = VisualJs.Fn(args_66 => { V v_st_67 = VisualJs.Arg(args_66, 0);
V v_x_68 = VisualJs.Arg(args_66, 1);
V v_y_69 = VisualJs.Arg(args_66, 2);
V v_preview_70 = Visual_networkPreview.Scope[new V("buildGhostSnapPreview")].Call(v_st_67[new V("spheres")], VisualJs.Obj(new V("x"), v_x_68, new V("y"), v_y_69));
if ((!v_preview_70).Truth) return VisualJs.Null;
return VisualJs.Obj(new V("pointerId"), (-new V(1d)), new V("sphereIndex"), v_preview_70[new V("candidateIndex")], new V("x"), v_x_68, new V("y"), v_y_69, new V("valid"), Visual_spaceCollision.Scope[new V("canPlaceSphere")].Call(v_st_67, v_x_68, v_y_69), new V("network"), v_preview_70[new V("network")], new V("formation"), (v_preview_70[new V("formation")].Truth ? VisualJs.Obj(new V("type"), v_preview_70[new V("formation")][new V("type")], new V("nodes"), v_preview_70[new V("formation")][new V("nodes")], new V("strength"), v_preview_70[new V("formation")][new V("strength")]) : VisualJs.Null));
return default; });
V v_captureFormationMemory_71 = default; v_captureFormationMemory_71 = VisualJs.Fn(args_72 => { V v_st_73 = VisualJs.Arg(args_72, 0);
V v_network_74 = VisualJs.Arg(args_72, 1);
V v_formation_75 = Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_network_74);
if ((!v_formation_75).Truth) return default;
V v_points_76 = v_formation_75[new V("nodes")][new V("map")].Call(VisualJs.Fn(args_77 => { V v_index_78 = VisualJs.Arg(args_77, 0);
return (VisualJs.Merge(v_st_73[new V("spheres")][v_index_78][new V("pos")])); }));
v_setFormationMemory_24.Call(VisualJs.Obj(new V("type"), v_formation_75[new V("type")], new V("points"), v_points_76, new V("life"), new V(0.9d), new V("id"), VisualUiBindings.Get("global","Date")[new V("now")].Call()));
return default; });
V v_handlesphereTap_79 = default; v_handlesphereTap_79 = VisualJs.Fn(args_80 => { V v_clientX_81 = VisualJs.Arg(args_80, 0);
V v_clientY_82 = VisualJs.Arg(args_80, 1);
V v_st_83 = v_stateRef_5[new V("current")];
if (VisualJs.Or(VisualJs.Or((!v_st_83), () => v_st_83[new V("gameOver")]), () => (VisualJs.And(VisualJs.And(v_st_83[new V("paused")], () => new V(v_tutorialStep_9 != new V(1d))), () => new V(v_tutorialStep_9 != new V(2d))))).Truth) return default;
if (VisualJs.Or(v_st_83[new V("pendingUpgrade")], () => v_st_83[new V("pendingArtifact")]).Truth) return default;
V v_world_84 = v_touchToWorld_43.Call(v_clientX_81, v_clientY_82);
if ((!v_world_84).Truth) return default;
V v_nearExistingsphere_85 = v_st_83[new V("spheres")][new V("some")].Call(VisualJs.Fn(args_86 => { V v_sphere_87 = VisualJs.Arg(args_86, 0);
return VisualJs.And(v_sphere_87[new V("alive")], () => new V(VisualJs.MathObject[new V("hypot")].Call((v_sphere_87[new V("pos")][new V("x")] - v_world_84[new V("x")]), (v_sphere_87[new V("pos")][new V("y")] - v_world_84[new V("y")])) < Scope[new V("sphere_TOUCH_TOLERANCE")])); }));
if (VisualJs.And(new V(v_tutorialStep_9 == new V(2d)), () => v_nearExistingsphere_85).Truth) return default;
if (v_nearExistingsphere_85.Truth) {
V v_existing_88 = v_st_83[new V("spheres")][new V("find")].Call(VisualJs.Fn(args_89 => { V v_sphere_90 = VisualJs.Arg(args_89, 0);
return VisualJs.And(v_sphere_90[new V("alive")], () => new V(VisualJs.MathObject[new V("hypot")].Call((v_sphere_90[new V("pos")][new V("x")] - v_world_84[new V("x")]), (v_sphere_90[new V("pos")][new V("y")] - v_world_84[new V("y")])) < Scope[new V("sphere_TOUCH_TOLERANCE")])); }));
if (v_existing_88.Truth) v_captureFormationMemory_71.Call(v_st_83, Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_83[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_91 => { V v_sphere_92 = VisualJs.Arg(args_91, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_92[new V("pos")], new V("alive"), v_sphere_92[new V("alive")], new V("networkDisabledTimer"), v_sphere_92[new V("networkDisabledTimer")])); }))));
V v_beforeCount_93 = v_st_83[new V("spheres")][new V("length")];
VisualUiBindings.Get("engineSpheres.ts","placeSphere").Call(v_st_83, v_world_84[new V("x")], v_world_84[new V("y")]);
if (new V(v_st_83[new V("spheres")][new V("length")] < v_beforeCount_93).Truth) Scope[new V("haptic")].Call(new V(10d));
return default;
}
if (new V(v_st_83[new V("spheres")][new V("length")] >= Visual_engineSpheres.Scope[new V("getMaxSpheres")].Call(v_st_83)).Truth) return default;
if ((!Visual_spaceCollision.Scope[new V("canPlaceSphere")].Call(v_st_83, v_world_84[new V("x")], v_world_84[new V("y")])).Truth) {
VisualJs.Assign(v_st_83,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("Путь перекрыт") : new V("Path blocked")), new V("life"), new V(0.8d), new V("color"), new V("#ff4d5d")));
Scope[new V("haptic")].Call(new V(35d));
return default;
}
V v_beforeCount_94 = v_st_83[new V("spheres")][new V("length")];
VisualUiBindings.Get("engineSpheres.ts","placeSphere").Call(v_st_83, v_world_84[new V("x")], v_world_84[new V("y")]);
if (new V(v_st_83[new V("spheres")][new V("length")] > v_beforeCount_94).Truth) {
Scope[new V("haptic")].Call(new V(8d));
v_setPlacementFx_18.Call(VisualJs.Obj(new V("x"), v_clientX_81, new V("y"), v_clientY_82, new V("color"), Visual_gameData.Scope[new V("SPHERE_TYPES")][v_st_83[new V("selectedSphereType")]][new V("color")], new V("id"), VisualUiBindings.Get("global","Date")[new V("now")].Call()));
}
return default; });
V v_startJoystick_95 = default; v_startJoystick_95 = VisualJs.Fn(args_96 => { V v_pointerId_97 = VisualJs.Arg(args_96, 0);
V v_x_98 = VisualJs.Arg(args_96, 1);
V v_y_99 = VisualJs.Arg(args_96, 2);
VisualJs.Assign(v_joystickIdRef_11,new V("current"),"=",v_pointerId_97);
v_setJoystick_15.Call(VisualJs.Obj(new V("pointerId"), v_pointerId_97, new V("x"), v_x_98, new V("y"), v_y_99, new V("dx"), new V(0d), new V("dy"), new V(0d), new V("active"), new V(false)));
return default; });
V v_endPointer_100 = default; v_endPointer_100 = VisualJs.Fn(args_101 => { V v_pointerId_102 = VisualJs.Arg(args_101, 0);
V v_clientX_103 = VisualJs.Arg(args_101, 1);
V v_clientY_104 = VisualJs.Arg(args_101, 2);
V v_pointer_105 = v_pointersRef_10[new V("current")][new V("get")].Call(v_pointerId_102);
v_pointersRef_10[new V("current")][new V("delete")].Call(v_pointerId_102);
if (VisualJs.Chain(v_pointer_105,optional_106=>optional_106[new V("draggingSphere")]).Truth) {
V v_st_107 = v_stateRef_5[new V("current")];
if (VisualJs.And(v_st_107, () => (!v_pointer_105[new V("moved")])).Truth) {
VisualUiBindings.Get("engineSpheres.ts","placeSphere").Call(v_st_107, v_pointer_105[new V("draggingSphere")][new V("pos")][new V("x")], v_pointer_105[new V("draggingSphere")][new V("pos")][new V("y")]);
v_setGhostPreview_21.Call(VisualJs.Null);
Scope[new V("haptic")].Call(new V(10d));
} else if (VisualJs.And(v_st_107, () => v_pointer_105[new V("repositioned")]).Truth) {
V v_oldNetwork_108 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_107[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_109 => { V v_sphere_110 = VisualJs.Arg(args_109, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_110[new V("pos")], new V("alive"), v_sphere_110[new V("alive")], new V("networkDisabledTimer"), v_sphere_110[new V("networkDisabledTimer")])); })));
V v_world_111 = v_touchToWorld_43.Call(v_clientX_103, v_clientY_104);
if (VisualJs.And(v_world_111, () => Visual_spaceCollision.Scope[new V("canRepositionSphere")].Call(v_st_107, v_pointer_105[new V("draggingSphere")], v_world_111[new V("x")], v_world_111[new V("y")])).Truth) {
VisualUiBindings.Get("spaceCollision.ts","repositionSphere").Call(v_st_107, v_pointer_105[new V("draggingSphere")], v_world_111[new V("x")], v_world_111[new V("y")]);
V v_newNetwork_112 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_107[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_113 => { V v_sphere_114 = VisualJs.Arg(args_113, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_114[new V("pos")], new V("alive"), v_sphere_114[new V("alive")], new V("networkDisabledTimer"), v_sphere_114[new V("networkDisabledTimer")])); })));
V v_oldFormation_115 = Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_oldNetwork_108);
V v_newFormation_116 = Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_newNetwork_112);
if (VisualJs.And(v_oldFormation_115, () => (VisualJs.Or(VisualJs.Or((!v_newFormation_116), () => new V(v_oldFormation_115[new V("type")] != v_newFormation_116[new V("type")])), () => new V(v_oldFormation_115[new V("nodes")][new V("join")].Call(new V(",")) != v_newFormation_116[new V("nodes")][new V("join")].Call(new V(",")))))).Truth) v_captureFormationMemory_71.Call(v_st_107, v_oldNetwork_108);
VisualJs.Assign(v_st_107,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("Сфера перемещена") : new V("Sphere moved")), new V("life"), new V(0.8d), new V("color"), Visual_gameData.Scope[new V("SPHERE_TYPES")][v_pointer_105[new V("draggingSphere")][new V("type")]][new V("color")]));
}
v_setGhostPreview_21.Call(VisualJs.Null);
Scope[new V("haptic")].Call(new V(12d));
} else if (VisualJs.And(v_st_107, () => v_pointer_105[new V("moved")]).Truth) {
VisualJs.Assign(v_st_107,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("Недоступная позиция") : new V("Invalid position")), new V("life"), new V(0.65d), new V("color"), new V("#ff4d5d")));
v_setGhostPreview_21.Call(VisualJs.Null);
Scope[new V("haptic")].Call(new V(22d));
}
return default;
}
if (VisualJs.Chain(v_pointer_105,optional_117=>optional_117[new V("placingSphere")]).Truth) {
V v_st_118 = v_stateRef_5[new V("current")];
if (VisualJs.And(v_st_118, () => (!v_pointer_105[new V("moved")])).Truth) {
v_handlesphereTap_79.Call(v_clientX_103, v_clientY_104);
v_setGhostPreview_21.Call(VisualJs.Null);
return default;
}
if (VisualJs.And(v_st_118, () => v_pointer_105[new V("repositioned")]).Truth) {
V v_world_119 = v_touchToWorld_43.Call(v_clientX_103, v_clientY_104);
if (VisualJs.And(v_world_119, () => Visual_spaceCollision.Scope[new V("canPlaceSphere")].Call(v_st_118, v_world_119[new V("x")], v_world_119[new V("y")])).Truth) {
V v_oldNetwork_120 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_118[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_121 => { V v_sphere_122 = VisualJs.Arg(args_121, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_122[new V("pos")], new V("alive"), v_sphere_122[new V("alive")], new V("networkDisabledTimer"), v_sphere_122[new V("networkDisabledTimer")])); })));
V v_beforeCount_123 = v_st_118[new V("spheres")][new V("length")];
VisualUiBindings.Get("engineSpheres.ts","placeSphere").Call(v_st_118, v_world_119[new V("x")], v_world_119[new V("y")]);
if (new V(v_st_118[new V("spheres")][new V("length")] > v_beforeCount_123).Truth) {
V v_newNetwork_124 = Visual_network.Scope[new V("analyzeSphereNetwork")].Call(v_st_118[new V("spheres")][new V("map")].Call(VisualJs.Fn(args_125 => { V v_sphere_126 = VisualJs.Arg(args_125, 0);
return (VisualJs.Obj(new V("pos"), v_sphere_126[new V("pos")], new V("alive"), v_sphere_126[new V("alive")], new V("networkDisabledTimer"), v_sphere_126[new V("networkDisabledTimer")])); })));
V v_oldFormation_127 = Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_oldNetwork_120);
V v_newFormation_128 = Visual_networkPreview.Scope[new V("getGhostSnapFormation")].Call(v_newNetwork_124);
if (VisualJs.And(v_oldFormation_127, () => (VisualJs.Or(VisualJs.Or((!v_newFormation_128), () => new V(v_oldFormation_127[new V("type")] != v_newFormation_128[new V("type")])), () => new V(v_oldFormation_127[new V("nodes")][new V("join")].Call(new V(",")) != v_newFormation_128[new V("nodes")][new V("join")].Call(new V(",")))))).Truth) v_captureFormationMemory_71.Call(v_st_118, v_oldNetwork_120);
Scope[new V("haptic")].Call(new V(8d));
}
}
v_setGhostPreview_21.Call(VisualJs.Null);
return default;
}
if (VisualJs.And(v_st_118, () => v_pointer_105[new V("moved")]).Truth) {
VisualJs.Assign(v_st_118,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("Недоступная позиция") : new V("Invalid position")), new V("life"), new V(0.65d), new V("color"), new V("#ff4d5d")));
v_setGhostPreview_21.Call(VisualJs.Null);
Scope[new V("haptic")].Call(new V(22d));
}
return default;
}
if (new V(v_joystickIdRef_11[new V("current")] == v_pointerId_102).Truth) {
VisualJs.Assign(v_joystickIdRef_11,new V("current"),"=",VisualJs.Null);
v_clearMovementKeys_32.Call();
v_setJoystick_15.Call(VisualJs.Null);
if (VisualJs.And(v_pointer_105, () => (!v_pointer_105[new V("moved")])).Truth) v_handlesphereTap_79.Call(v_clientX_103, v_clientY_104);
} else if (VisualJs.And(v_pointer_105, () => (!v_pointer_105[new V("moved")])).Truth) v_handlesphereTap_79.Call(v_clientX_103, v_clientY_104);
return default; });
VisualDom.React("useEffect").Call(VisualJs.Fn(args_129 => { v_clearMovementKeys_32.Call();
v_setJoystick_15.Call(VisualJs.Null);
VisualJs.Assign(v_joystickIdRef_11,new V("current"),"=",VisualJs.Null);
v_pointersRef_10[new V("current")][new V("clear")].Call();
return VisualJs.Fn(args_130 => { return v_clearMovementKeys_32.Call(); });
return default; }), VisualJs.Arr(v_handedness_7));
VisualDom.React("useEffect").Call(VisualJs.Fn(args_131 => { if ((!v_placementFx_17).Truth) return default;
V v_timer_132 = VisualUiBindings.Get("global","window")[new V("setTimeout")].Call(VisualJs.Fn(args_133 => { return v_setPlacementFx_18.Call(VisualJs.Null); }), new V(450d));
return VisualJs.Fn(args_134 => { return VisualUiBindings.Get("global","window")[new V("clearTimeout")].Call(v_timer_132); });
return default; }), VisualJs.Arr(v_placementFx_17));
VisualDom.React("useEffect").Call(VisualJs.Fn(args_135 => { if ((!v_formationMemory_23).Truth) return default;
V v_started_136 = VisualJs.PerformanceObject[new V("now")].Call();
V v_frame_137 = new V(0d);
V v_tick_138 = default; v_tick_138 = VisualJs.Fn(args_139 => { V v_elapsed_140 = (((VisualJs.PerformanceObject[new V("now")].Call() - v_started_136)) / new V(1000d));
v_setFormationMemory_24.Call(VisualJs.Fn(args_141 => { V v_current_142 = VisualJs.Arg(args_141, 0);
return (v_current_142.Truth ? VisualJs.Merge(v_current_142, VisualJs.Obj(new V("life"),VisualJs.MathObject[new V("max")].Call(new V(0d), (new V(0.9d) - v_elapsed_140)))) : VisualJs.Null); }));
if (new V(v_elapsed_140 < new V(0.9d)).Truth) v_frame_137 = VisualUiBindings.Get("global","requestAnimationFrame").Call(v_tick_138); else v_setFormationMemory_24.Call(VisualJs.Null);
return default; });
v_frame_137 = VisualUiBindings.Get("global","requestAnimationFrame").Call(v_tick_138);
return VisualJs.Fn(args_143 => { return VisualUiBindings.Get("global","cancelAnimationFrame").Call(v_frame_137); });
return default; }), VisualJs.Arr(VisualJs.Chain(v_formationMemory_23,optional_144=>optional_144[new V("id")])));
V v_activeAbilities_145 = (VisualJs.ObjectObject[new V("entries")].Call(VisualJs.Or(VisualJs.Chain(v_stateRef_5[new V("current")],optional_146=>optional_146[new V("activeKeyMap")]), () => VisualJs.Obj())));
V v_types_147 = (VisualJs.ObjectObject[new V("keys")].Call(Visual_gameData.Scope[new V("SPHERE_TYPES")]));
V v_selectedType_148 = VisualJs.Or(VisualJs.Chain(v_stateRef_5[new V("current")],optional_149=>optional_149[new V("selectedSphereType")]), () => new V("standard"));
V v_selectedDef_150 = Visual_gameData.Scope[new V("SPHERE_TYPES")][v_selectedType_148];
V v_preferredSphereTypes_151 = (v_stateRef_5[new V("current")].Truth ? VisualJs.Or(VisualJs.Chain(Visual_characters.Scope[new V("CHARACTER_DEFS")][v_stateRef_5[new V("current")][new V("player")][new V("characterId")]],optional_152=>optional_152[new V("preferredSphereTypes")]), () => VisualJs.Arr()) : VisualJs.Arr());
return (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("es-mobile-controls absolute inset-0 z-20 overflow-hidden select-none")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("touchAction"), new V("none"), new V("userSelect"), new V("none"), new V("WebkitUserSelect"), new V("none"))), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_153 => { V v_e_154 = VisualJs.Arg(args_153, 0);
if (Scope[new V("isBlockedByControl")].Call(v_e_154[new V("target")]).Truth) return default;
V v_st_155 = v_stateRef_5[new V("current")];
if (VisualJs.Or(VisualJs.Or((!v_st_155), () => v_st_155[new V("gameOver")]), () => (VisualJs.And(VisualJs.And(v_st_155[new V("paused")], () => new V(v_tutorialStep_9 != new V(1d))), () => new V(v_tutorialStep_9 != new V(2d))))).Truth) return default;
v_e_154[new V("currentTarget")][new V("setPointerCapture")].Call(v_e_154[new V("pointerId")]);
V v_pointer_156 = VisualJs.Obj(new V("startX"), v_e_154[new V("clientX")], new V("startY"), v_e_154[new V("clientY")], new V("moved"), new V(false), new V("joystickCandidate"), new V(false), new V("draggingSphere"), VisualJs.Null, new V("placingSphere"), new V(false), new V("repositioned"), new V(false), new V("lockedSphereTouch"), new V(false));
V v_world_157 = v_touchToWorld_43.Call(v_e_154[new V("clientX")], v_e_154[new V("clientY")]);
if (v_world_157.Truth) {
V v_draggable_158 = VisualJs.Or(v_st_155[new V("spheres")][new V("find")].Call(VisualJs.Fn(args_159 => { V v_sphere_160 = VisualJs.Arg(args_159, 0);
return VisualJs.And(v_sphere_160[new V("alive")], () => new V(VisualJs.MathObject[new V("hypot")].Call((v_sphere_160[new V("pos")][new V("x")] - v_world_157[new V("x")]), (v_sphere_160[new V("pos")][new V("y")] - v_world_157[new V("y")])) < new V(34d))); })), () => VisualJs.Null);
if (VisualJs.And(VisualJs.And(v_draggable_158, () => new V(v_tutorialStep_9 != new V(1d))), () => new V(v_tutorialStep_9 != new V(2d))).Truth) {
if (v_st_155[new V("player")][new V("sphereMovementLocked")].Truth) VisualJs.Assign(v_pointer_156,new V("lockedSphereTouch"),"=",new V(true)); else VisualJs.Assign(v_pointer_156,new V("draggingSphere"),"=",v_draggable_158);
} else {
V v_tutorialMovement_161 = new V(v_tutorialStep_9 == new V(1d));
V v_tutorialPlacement_162 = new V(v_tutorialStep_9 == new V(2d));
VisualJs.Assign(v_pointer_156,new V("joystickCandidate"),"=",VisualJs.And(VisualJs.And(VisualJs.And((!v_tutorialPlacement_162), () => new V(v_joystickIdRef_11[new V("current")] == VisualJs.Null)), () => new V(v_e_154[new V("clientX")] >= v_joystickZoneStart_30)), () => new V(v_e_154[new V("clientX")] < v_joystickZoneEnd_31)));
VisualJs.Assign(v_pointer_156,new V("placingSphere"),"=",VisualJs.And((VisualJs.Or(v_tutorialPlacement_162, () => (VisualJs.And((!v_tutorialMovement_161), () => (!v_pointer_156[new V("joystickCandidate")]))))), () => new V(v_st_155[new V("spheres")][new V("length")] < Visual_engineSpheres.Scope[new V("getMaxSpheres")].Call(v_st_155))));
}
}
v_pointersRef_10[new V("current")][new V("set")].Call(v_e_154[new V("pointerId")], v_pointer_156);
if (v_pointer_156[new V("joystickCandidate")].Truth) v_startJoystick_95.Call(v_e_154[new V("pointerId")], v_e_154[new V("clientX")], v_e_154[new V("clientY")]);
return default; })), VisualJs.Obj(new V("onPointerMove"),VisualJs.Fn(args_163 => { V v_e_164 = VisualJs.Arg(args_163, 0);
V v_pointer_165 = v_pointersRef_10[new V("current")][new V("get")].Call(v_e_164[new V("pointerId")]);
if ((!v_pointer_165).Truth) return default;
V v_dx_166 = (v_e_164[new V("clientX")] - v_pointer_165[new V("startX")]);
V v_dy_167 = (v_e_164[new V("clientY")] - v_pointer_165[new V("startY")]);
V v_distance_168 = VisualJs.MathObject[new V("hypot")].Call(v_dx_166, v_dy_167);
if (v_pointer_165[new V("lockedSphereTouch")].Truth) return default;
if (v_pointer_165[new V("draggingSphere")].Truth) {
if (new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]).Truth) VisualJs.Assign(v_pointer_165,new V("moved"),"=",new V(true));
if (new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]).Truth) {
V v_world_169 = v_touchToWorld_43.Call(v_e_164[new V("clientX")], v_e_164[new V("clientY")]);
V v_st_170 = v_stateRef_5[new V("current")];
if (VisualJs.And(v_world_169, () => v_st_170).Truth) {
V v_preview_171 = v_buildGhostPreview_52.Call(v_st_170, v_pointer_165[new V("draggingSphere")], v_st_170[new V("spheres")][new V("indexOf")].Call(v_pointer_165[new V("draggingSphere")]), v_world_169[new V("x")], v_world_169[new V("y")]);
VisualJs.Assign(v_preview_171,new V("pointerId"),"=",v_e_164[new V("pointerId")]);
VisualJs.Assign(v_pointer_165,new V("repositioned"),"=",v_preview_171[new V("valid")]);
v_setGhostPreview_21.Call(v_preview_171);
}
}
return default;
}
if (v_pointer_165[new V("placingSphere")].Truth) {
if (new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]).Truth) VisualJs.Assign(v_pointer_165,new V("moved"),"=",new V(true));
if (new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]).Truth) {
V v_world_172 = v_touchToWorld_43.Call(v_e_164[new V("clientX")], v_e_164[new V("clientY")]);
V v_st_173 = v_stateRef_5[new V("current")];
if (VisualJs.And(v_world_172, () => v_st_173).Truth) {
V v_preview_174 = v_buildNewSphereGhostPreview_65.Call(v_st_173, v_world_172[new V("x")], v_world_172[new V("y")]);
if (v_preview_174.Truth) {
VisualJs.Assign(v_preview_174,new V("pointerId"),"=",v_e_164[new V("pointerId")]);
VisualJs.Assign(v_pointer_165,new V("repositioned"),"=",v_preview_174[new V("valid")]);
v_setGhostPreview_21.Call(v_preview_174);
}
}
}
return default;
}
if (new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]).Truth) VisualJs.Assign(v_pointer_165,new V("moved"),"=",new V(true));
if (new V(v_joystickIdRef_11[new V("current")] == v_e_164[new V("pointerId")]).Truth) {
V v_length_175 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_dx_166, v_dy_167), () => new V(1d));
V v_movementScale_176 = VisualJs.MathObject[new V("min")].Call(new V(1d), VisualJs.MathObject[new V("max")].Call(new V(0d), (v_length_175 / Scope[new V("JOYSTICK_RADIUS")])));
v_setJoystick_15.Call(VisualJs.Fn(args_177 => { V v_current_178 = VisualJs.Arg(args_177, 0);
return (v_current_178.Truth ? VisualJs.Merge(v_current_178, VisualJs.Obj(new V("dx"),((((v_dx_166 / v_length_175)) * Scope[new V("JOYSTICK_RADIUS")]) * v_movementScale_176)), VisualJs.Obj(new V("dy"),((((v_dy_167 / v_length_175)) * Scope[new V("JOYSTICK_RADIUS")]) * v_movementScale_176)), VisualJs.Obj(new V("active"),new V(v_distance_168 > Scope[new V("JOYSTICK_DEADZONE")]))) : v_current_178); }));
v_applyDirection_35.Call(v_dx_166, v_dy_167);
}
return default; })), VisualJs.Obj(new V("onPointerUp"),VisualJs.Fn(args_179 => { V v_e_180 = VisualJs.Arg(args_179, 0);
return v_endPointer_100.Call(v_e_180[new V("pointerId")], v_e_180[new V("clientX")], v_e_180[new V("clientY")]); })), VisualJs.Obj(new V("onPointerCancel"),VisualJs.Fn(args_181 => { V v_e_182 = VisualJs.Arg(args_181, 0);
return v_endPointer_100.Call(v_e_182[new V("pointerId")], v_e_182[new V("clientX")], v_e_182[new V("clientY")]); }))), new V(""), VisualJs.And(new V(v_tutorialStep_9 == new V(1d)), () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("data-tutorial-target"),new V("movement")), VisualJs.Obj(new V("className"),new V("absolute pointer-events-none z-30 rounded-full border border-[#63e6ff]/20")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("width"), new V(116d), new V("height"), new V(116d), new V("left"), (v_joystickOnRight_26.Truth ? new V("calc(75% - 58px)") : new V("calc(25% - 58px)")), new V("bottom"), new V(44d))))))), new V(""), VisualJs.And(v_ghostPreview_20, () => VisualDom.Component("GhostSnapOverlay",Scope[new V("GhostSnapOverlay")],VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("canvasRef"),v_canvasRef_6), VisualJs.Obj(new V("stateRef"),v_stateRef_5), VisualJs.Obj(new V("preview"),v_ghostPreview_20), VisualJs.Obj(new V("lang"),v_lang_3), VisualJs.Obj(new V("sphereColor"),v_selectedDef_150[new V("color")]))))), new V(""), VisualJs.And(v_formationMemory_23, () => VisualDom.Component("FormationMemoryOverlay",Scope[new V("FormationMemoryOverlay")],VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("canvasRef"),v_canvasRef_6), VisualJs.Obj(new V("stateRef"),v_stateRef_5), VisualJs.Obj(new V("memory"),v_formationMemory_23))))), new V(""), VisualJs.And(v_placementFx_17, () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("key"),v_placementFx_17[new V("id")]), VisualJs.Obj(new V("className"),new V("absolute pointer-events-none")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("left"), (v_placementFx_17[new V("x")] - new V(26d)), new V("top"), (v_placementFx_17[new V("y")] - new V(26d)), new V("width"), new V(52d), new V("height"), new V(52d), new V("transform"), (new V("scale(") + v_interfaceScale_25 + new V(")")), new V("transformOrigin"), new V("center")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-0 rounded-full border-2 opacity-70 animate-ping")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_placementFx_17[new V("color")])))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-[9px] rounded-full border")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_placementFx_17[new V("color")], new V("opacity"), new V(0.55d))))), new V("")))), new V(""), VisualJs.And(v_joystick_14, () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute pointer-events-none")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("left"), (v_joystick_14[new V("x")] - Scope[new V("JOYSTICK_RADIUS")]), new V("top"), (v_joystick_14[new V("y")] - Scope[new V("JOYSTICK_RADIUS")]), new V("width"), (Scope[new V("JOYSTICK_RADIUS")] * new V(2d)), new V("height"), (Scope[new V("JOYSTICK_RADIUS")] * new V(2d)), new V("transform"), (new V("scale(") + v_interfaceScale_25 + new V(")")), new V("transformOrigin"), new V("center")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-0 rounded-full border border-white/20 bg-black/20 backdrop-blur-[2px]")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),(new V("absolute rounded-full border border-white/30 bg-white/25 ") + (v_joystick_14[new V("active")].Truth ? new V("opacity-90") : new V("opacity-70")) + new V(""))), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("width"), (Scope[new V("JOYSTICK_KNOB_RADIUS")] * new V(2d)), new V("height"), (Scope[new V("JOYSTICK_KNOB_RADIUS")] * new V(2d)), new V("left"), ((Scope[new V("JOYSTICK_RADIUS")] - Scope[new V("JOYSTICK_KNOB_RADIUS")]) + v_joystick_14[new V("dx")]), new V("top"), ((Scope[new V("JOYSTICK_RADIUS")] - Scope[new V("JOYSTICK_KNOB_RADIUS")]) + v_joystick_14[new V("dy")]))))), new V("")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),(new V("absolute bottom-4 ") + v_controlsSide_28 + new V(" flex flex-col ") + v_controlsAlign_29 + new V(" gap-2 pointer-events-none"))), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("transform"), (new V("scale(") + v_interfaceScale_25 + new V(")")), new V("transformOrigin"), (v_controlsOnRight_27.Truth ? new V("right bottom") : new V("left bottom"))))), new V(""), VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("className"),new V("pointer-events-auto w-12 h-12 rounded-full bg-[#0d1726]/90 border border-[#243b55] shadow-lg flex items-center justify-center text-[#b6c9de] active:scale-95")), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_183 => { V v_e_184 = VisualJs.Arg(args_183, 0);
v_e_184[new V("stopPropagation")].Call();
Scope[new V("haptic")].Call(new V(6d));
v_onPause_8.Call();
return default; })), VisualJs.Obj(new V("aria-label"),v_t_4.Call(new V("pause")))), VisualDom.Component("Pause",VisualUiBindings.Icon("Pause"),VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("size"),new V(18d)))))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("grid grid-cols-2 gap-1.5 pointer-events-auto"))), new V(""), VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("className"),new V("pointer-events-auto w-12 h-10 rounded-lg bg-[#0d1726]/90 border border-[#243b55] shadow-lg flex items-center justify-center gap-1 text-[9px] font-bold text-[#b6c9de] active:scale-95")), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_185 => { V v_e_186 = VisualJs.Arg(args_185, 0);
v_e_186[new V("stopPropagation")].Call();
V v_st_187 = v_stateRef_5[new V("current")];
if ((!v_st_187).Truth) return default;
VisualJs.Assign(v_st_187[new V("player")],new V("sphereMovementLocked"),"=",(!v_st_187[new V("player")][new V("sphereMovementLocked")]));
Scope[new V("haptic")].Call(new V(6d));
return default; })), VisualJs.Obj(new V("aria-label"),(new V(v_lang_3 == new V("ru")).Truth ? new V("Блокировка перемещения сфер") : new V("Lock sphere movement")))), new V(""), (VisualJs.Chain(v_stateRef_5[new V("current")],optional_188=>optional_188[new V("player")][new V("sphereMovementLocked")]).Truth ? VisualDom.Component("Lock",VisualUiBindings.Icon("Lock"),VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("size"),new V(14d))))) : VisualDom.Component("Unlock",VisualUiBindings.Icon("Unlock"),VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("size"),new V(14d)))))), new V(""), VisualDom.Element("span",VisualJs.Merge(), (VisualJs.Chain(v_stateRef_5[new V("current")],optional_189=>optional_189[new V("player")][new V("sphereMovementLocked")]).Truth ? ((new V(v_lang_3 == new V("ru")).Truth ? new V("ЗАБЛОКИРОВАНО") : new V("LOCKED"))) : ((new V(v_lang_3 == new V("ru")).Truth ? new V("ПЕРЕМЕЩЕНИЕ") : new V("MOVE"))))), new V("")), new V(""), VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("data-game-control"),new V("formation-follow")), VisualJs.Obj(new V("aria-pressed"),new V(VisualJs.Chain(v_stateRef_5[new V("current")],optional_190=>optional_190[new V("player")][new V("formationFollowActive")]).Truth)), VisualJs.Obj(new V("disabled"),new V(VisualJs.And(VisualJs.And(v_stateRef_5[new V("current")], () => (!v_stateRef_5[new V("current")][new V("player")][new V("formationFollowActive")])), () => (!VisualUiBindings.Get("formationFollow.ts","canActivateFormationFollow").Call(v_stateRef_5[new V("current")]))).Truth)), VisualJs.Obj(new V("className"),new V("pointer-events-auto w-12 h-10 rounded-lg bg-[#0d1726]/90 border border-[#243b55] shadow-lg flex items-center justify-center gap-1 text-[9px] font-bold text-[#b6c9de] active:scale-95 disabled:opacity-40")), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_191 => { V v_e_192 = VisualJs.Arg(args_191, 0);
v_e_192[new V("stopPropagation")].Call();
V v_st_193 = v_stateRef_5[new V("current")];
if (VisualJs.Or((!v_st_193), () => v_st_193[new V("gameOver")]).Truth) return default;
if (v_st_193[new V("player")][new V("formationFollowActive")].Truth) {
VisualUiBindings.Get("formationFollow.ts","setFormationFollow").Call(v_st_193, new V(false));
VisualJs.Assign(v_st_193,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("FOLLOW отключён") : new V("FOLLOW OFF")), new V("life"), new V(0.7d), new V("color"), new V("#7f9bb8")));
Scope[new V("haptic")].Call(new V(6d));
return default;
}
if (VisualJs.Or((!VisualUiBindings.Get("formationFollow.ts","canActivateFormationFollow").Call(v_st_193)), () => (!VisualUiBindings.Get("formationFollow.ts","setFormationFollow").Call(v_st_193, new V(true)))).Truth) {
VisualJs.Assign(v_st_193,new V("flashText"),"=",VisualJs.Obj(new V("text"), (new V(v_lang_3 == new V("ru")).Truth ? new V("Подойдите к центру формации") : new V("Move closer to Formation Centroid")), new V("life"), new V(1d), new V("color"), new V("#ffb84d")));
Scope[new V("haptic")].Call(new V(22d));
return default;
}
VisualJs.Assign(v_st_193,new V("flashText"),"=",VisualJs.Obj(new V("text"), new V("FORMATION FOLLOW"), new V("life"), new V(0.8d), new V("color"), new V("#63e6ff")));
Scope[new V("haptic")].Call(new V(12d));
return default; })), VisualJs.Obj(new V("aria-label"),(new V(v_lang_3 == new V("ru")).Truth ? new V("Следование формации") : new V("Formation Follow")))), new V(""), VisualDom.Component("Move",VisualUiBindings.Icon("Move"),VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("size"),new V(14d))))), new V(""), VisualDom.Element("span",VisualJs.Merge(), (VisualJs.Chain(v_stateRef_5[new V("current")],optional_194=>optional_194[new V("player")][new V("formationFollowActive")]).Truth ? ((new V(v_lang_3 == new V("ru")).Truth ? new V("СЛЕД") : new V("FOLLOWING"))) : new V("FOLLOW"))), new V("")), new V("")), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("grid grid-cols-2 gap-1.5 pointer-events-auto"))), new V(""), v_activeAbilities_145[new V("map")].Call(VisualJs.Fn(args_195 => { V binding_196 = VisualJs.Arg(args_195, 0);
V v_key_197 = binding_196[new V(0d)];
V v_ability_198 = binding_196[new V(1d)];
V v_def_199 = Visual_gameData.Scope[new V("ABILITIES")][v_ability_198];
V v_cd_200 = Scope[new V("getAbilityCooldown")].Call(v_stateRef_5[new V("current")], v_ability_198);
V v_followMovementLock_201 = new V(VisualJs.And(VisualJs.Chain(v_stateRef_5[new V("current")],optional_202=>optional_202[new V("player")][new V("formationFollowActive")]), () => Visual_formationFollow.Scope[new V("isFormationFollowMovementAbility")].Call(v_ability_198)).Truth);
return VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("key"),(new V("") + v_key_197 + new V("-") + v_ability_198 + new V(""))), VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("disabled"),v_followMovementLock_201), VisualJs.Obj(new V("className"),new V("relative w-16 h-12 rounded-xl bg-[#0d1726]/90 border border-[#243b55] shadow-lg px-1 overflow-hidden active:scale-95")), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_203 => { V v_e_204 = VisualJs.Arg(args_203, 0);
v_e_204[new V("stopPropagation")].Call();
V v_st_205 = v_stateRef_5[new V("current")];
if (VisualJs.Or(VisualJs.Or((!v_st_205), () => v_st_205[new V("gameOver")]), () => v_st_205[new V("paused")]).Truth) return default;
if (VisualJs.Or(v_st_205[new V("pendingUpgrade")], () => v_st_205[new V("pendingArtifact")]).Truth) return default;
Scope[new V("haptic")].Call(new V(12d));
VisualUiBindings.Get("engineAbilities.ts","activateByKey").Call(v_st_205, v_key_197);
return default; }))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("text-[9px] text-[#7f9bb8] block truncate"))), VisualJs.Or(Visual_sphereProgression.Scope[new V("getAbilityDisplayName")].Call(v_stateRef_5[new V("current")], v_ability_198, v_lang_3), () => v_def_199[new V("name")][v_lang_3])), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("text-[8px] text-[#b6c9de]/60"))), (new V(v_cd_200 > new V(0d)).Truth ? (new V("") + VisualJs.MathObject[new V("ceil")].Call(v_cd_200) + new V("s")) : v_t_4.Call(new V("ready")))), new V(""), VisualJs.And(new V(v_cd_200 > new V(0d)), () => VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-x-0 bottom-0 h-1 bg-[#ff4d5d]/70")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("width"), (new V("") + VisualJs.MathObject[new V("min")].Call(new V(100d), (((v_cd_200 / Scope[new V("getAbilityMaxCooldown")].Call(v_ability_198))) * new V(100d))) + new V("%"))))))), new V(""));
return default; })), new V("")), new V(""), VisualJs.And(VisualJs.Chain(v_stateRef_5[new V("current")],optional_206=>optional_206[new V("player")][new V("formationFollowActive")]), () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("pointer-events-none text-[8px] font-bold tracking-[0.16em] text-[#7f9bb8] bg-[#0d1726]/80 border border-[#243b55] rounded-md px-2 py-1"))), new V(""), (new V(v_lang_3 == new V("ru")).Truth ? new V("НАГРУЗКА СЕТИ") : new V("NETWORK STRAIN")), new V(" "), VisualJs.MathObject[new V("round")].Call(VisualJs.Or(VisualJs.Chain(v_stateRef_5[new V("current")],optional_207=>optional_207[new V("player")][new V("formationFollowStrain")]), () => new V(0d))), new V("%")))), new V(""), VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("disabled"),new V(VisualJs.Chain(v_stateRef_5[new V("current")],optional_208=>optional_208[new V("player")][new V("formationFollowActive")]).Truth)), VisualJs.Obj(new V("className"),new V("pointer-events-auto w-16 h-16 rounded-full bg-[#ffb84d]/85 border border-[#ff6b6b] shadow-lg flex flex-col items-center justify-center text-[#dcecff] active:scale-95")), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_209 => { V v_e_210 = VisualJs.Arg(args_209, 0);
v_e_210[new V("stopPropagation")].Call();
V v_st_211 = v_stateRef_5[new V("current")];
if (VisualJs.Or(VisualJs.Or(VisualJs.Or((!v_st_211), () => v_st_211[new V("gameOver")]), () => v_st_211[new V("paused")]), () => v_st_211[new V("player")][new V("formationFollowActive")]).Truth) return default;
V binding_212 = v_lastDirectionRef_12[new V("current")];
V v_x_213 = binding_212[new V("x")];
V v_y_214 = binding_212[new V("y")];

v_clearMovementKeys_32.Call();
if (new V(v_y_214 < (-new V(0.2d))).Truth) VisualJs.Assign(v_st_211[new V("keys")],new V("w"),"=",new V(true));
if (new V(v_y_214 > new V(0.2d)).Truth) VisualJs.Assign(v_st_211[new V("keys")],new V("s"),"=",new V(true));
if (new V(v_x_213 < (-new V(0.2d))).Truth) VisualJs.Assign(v_st_211[new V("keys")],new V("a"),"=",new V(true));
if (new V(v_x_213 > new V(0.2d)).Truth) VisualJs.Assign(v_st_211[new V("keys")],new V("d"),"=",new V(true));
Scope[new V("haptic")].Call(new V(16d));
VisualUiBindings.Get("engineLoop.ts","activateDash").Call(v_st_211);
v_clearMovementKeys_32.Call();
return default; })), VisualJs.Obj(new V("aria-label"),v_t_4.Call(new V("dashCooldown")))), new V(""), VisualDom.Component("Zap",VisualUiBindings.Icon("Zap"),VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("size"),new V(20d))))), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("text-[8px] font-bold"))), Scope[new V("stDashLabel")].Call(v_stateRef_5[new V("current")], v_lang_3, v_t_4)), new V("")), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("rounded-xl border border-[#243b55] bg-[#0d1726]/90 shadow-lg px-3 py-2 w-[220px] pointer-events-none text-center"))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("font-bold text-sm text-[#dcecff]"))), v_selectedDef_150[new V("name")][v_lang_3]), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("text-[10px] leading-tight text-[#7f9bb8] mt-0.5"))), v_selectedDef_150[new V("desc")][v_lang_3]), new V("")), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("grid grid-cols-5 gap-1.5 pointer-events-auto"))), new V(""), v_types_147[new V("map")].Call(VisualJs.Fn(args_215 => { V v_type_216 = VisualJs.Arg(args_215, 0);
V v_def_217 = Visual_gameData.Scope[new V("SPHERE_TYPES")][v_type_216];
V v_selected_218 = new V(v_selectedType_148 == v_type_216);
V v_preferred_219 = v_preferredSphereTypes_151[new V("includes")].Call(v_type_216);
return VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("key"),v_type_216), VisualJs.Obj(new V("data-mobile-control"),new V("true")), VisualJs.Obj(new V("className"),(new V("w-11 h-11 rounded-xl border bg-[#0d1726]/90 shadow-lg flex items-center justify-center active:scale-95 ") + (v_selected_218.Truth ? new V("border-[#7f9bb8]") : new V("border-[#243b55]")) + new V(" ") + (v_preferred_219.Truth ? new V("ring-2 ring-[#ffb84d]/45 ring-offset-1 ring-offset-[#0d1726]") : new V("")) + new V(""))), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), (v_selected_218.Truth ? v_def_217[new V("color")] : default(V)))), VisualJs.Obj(new V("onPointerDown"),VisualJs.Fn(args_220 => { V v_e_221 = VisualJs.Arg(args_220, 0);
v_e_221[new V("stopPropagation")].Call();
V v_st_222 = v_stateRef_5[new V("current")];
if ((!v_st_222).Truth) return default;
Scope[new V("haptic")].Call(new V(6d));
VisualUiBindings.Get("engineSpheres.ts","setSphereType").Call(v_st_222, v_type_216);
return default; })), VisualJs.Obj(new V("aria-label"),(new V("") + v_def_217[new V("name")][v_lang_3] + new V(" — ") + v_def_217[new V("desc")][v_lang_3] + new V("")))), new V(""), VisualDom.Component("SphereTypeGlyph",Scope[new V("SphereTypeGlyph")],VisualDom.Props(VisualJs.Merge(VisualJs.Obj(new V("type"),v_type_216), VisualJs.Obj(new V("color"),v_def_217[new V("color")])))), new V(""));
return default; })), new V("")), new V("")), new V("")));
return default;
}
private static V F_SphereTypeGlyph(V[] args) {
V binding_223 = VisualJs.Arg(args,0);
V v_type_224 = binding_223[new V("type")];
V v_color_225 = binding_223[new V("color")];
if (new V(v_type_224 == new V("sniper")).Truth) {
return VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-4 h-4 rotate-45 border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("33"))))));
}
if (new V(v_type_224 == new V("shotgun")).Truth) {
return VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("relative w-5 h-4")), VisualJs.Obj(new V("aria-hidden"),new V("true"))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute left-0 top-1 w-3 h-2 rounded-sm border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("33")))))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute left-3 top-0 w-2 h-4 rounded-r border-2 border-l-0")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225)))), new V(""));
}
if (new V(v_type_224 == new V("chain")).Truth) {
return VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("flex items-center gap-0.5")), VisualJs.Obj(new V("aria-hidden"),new V("true"))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-2.5 h-2.5 rounded-full border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("33")))))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-2.5 h-2.5 rounded-full border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("33")))))), new V(""), VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-2.5 h-2.5 rounded-full border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("33")))))), new V(""));
}
if (new V(v_type_224 == new V("aura")).Truth) {
return VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-5 h-5 rounded-full border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("boxShadow"), (new V("0 0 0 3px ") + v_color_225 + new V("22"))))));
}
return VisualDom.Element("span",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("w-4 h-4 rounded-full border-2")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("borderColor"), v_color_225, new V("backgroundColor"), (new V("") + v_color_225 + new V("44"))))));
return default;
}
private static V F_worldToScreen(V[] args) {
V v_canvas_226 = VisualJs.Arg(args,0);
V v_camera_227 = VisualJs.Arg(args,1);
V v_point_228 = VisualJs.Arg(args,2);
V v_rect_229 = v_canvas_226[new V("getBoundingClientRect")].Call();
return VisualJs.Obj(new V("x"), (v_rect_229[new V("left")] + ((((v_point_228[new V("x")] - v_camera_227[new V("x")]) + (v_canvas_226[new V("width")] / new V(2d)))) * ((v_rect_229[new V("width")] / v_canvas_226[new V("width")])))), new V("y"), (v_rect_229[new V("top")] + ((((v_point_228[new V("y")] - v_camera_227[new V("y")]) + (v_canvas_226[new V("height")] / new V(2d)))) * ((v_rect_229[new V("height")] / v_canvas_226[new V("height")])))));
return default;
}
private static V F_GhostSnapOverlay(V[] args) {
V binding_230 = VisualJs.Arg(args,0);
V v_canvasRef_231 = binding_230[new V("canvasRef")];
V v_stateRef_232 = binding_230[new V("stateRef")];
V v_preview_233 = binding_230[new V("preview")];
V v_lang_234 = binding_230[new V("lang")];
V v_sphereColor_235 = binding_230[new V("sphereColor")];
V v_canvas_236 = v_canvasRef_231[new V("current")];
V v_st_237 = v_stateRef_232[new V("current")];
if (VisualJs.Or((!v_canvas_236), () => (!v_st_237)).Truth) return VisualJs.Null;
V v_ghost_238 = Scope[new V("worldToScreen")].Call(v_canvas_236, v_st_237[new V("camera")], VisualJs.Obj(new V("x"), v_preview_233[new V("x")], new V("y"), v_preview_233[new V("y")]));
V v_scale_239 = (v_canvas_236[new V("getBoundingClientRect")].Call()[new V("width")] / v_canvas_236[new V("width")]);
V v_links_240 = v_preview_233[new V("network")][new V("links")][new V("filter")].Call(VisualJs.Fn(args_241 => { V v_link_242 = VisualJs.Arg(args_241, 0);
return VisualJs.Or(new V(v_link_242[new V("a")] == v_preview_233[new V("sphereIndex")]), () => new V(v_link_242[new V("b")] == v_preview_233[new V("sphereIndex")])); }));
V v_points_243 = VisualJs.Or(VisualJs.Chain(v_preview_233[new V("formation")],optional_244=>optional_244[new V("nodes")][new V("map")].Call(VisualJs.Fn(args_245 => { V v_index_246 = VisualJs.Arg(args_245, 0);
V v_node_247 = (new V(v_index_246 == v_preview_233[new V("sphereIndex")]).Truth ? VisualJs.Obj(new V("x"), v_preview_233[new V("x")], new V("y"), v_preview_233[new V("y")]) : VisualJs.Chain(v_st_237[new V("spheres")][v_index_246],optional_248=>optional_248[new V("pos")]));
return (v_node_247.Truth ? Scope[new V("worldToScreen")].Call(v_canvas_236, v_st_237[new V("camera")], v_node_247) : VisualJs.Null);
return default; }))[new V("filter")].Call(VisualJs.Fn(args_249 => { V v_x_250 = VisualJs.Arg(args_249, 0);
return new V(v_x_250.Truth); }))), () => VisualJs.Arr());
return VisualDom.Element("svg",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-0 pointer-events-none z-10 overflow-visible"))), new V(""), v_links_240[new V("map")].Call(VisualJs.Fn(args_251 => { V v_link_252 = VisualJs.Arg(args_251, 0);
V v_otherIndex_253 = (new V(v_link_252[new V("a")] == v_preview_233[new V("sphereIndex")]).Truth ? v_link_252[new V("b")] : v_link_252[new V("a")]);
V v_other_254 = v_st_237[new V("spheres")][v_otherIndex_253];
if ((!v_other_254).Truth) return VisualJs.Null;
V v_p_255 = Scope[new V("worldToScreen")].Call(v_canvas_236, v_st_237[new V("camera")], v_other_254[new V("pos")]);
return VisualDom.Element("line",VisualJs.Merge(VisualJs.Obj(new V("key"),(new V("ghost-link-") + v_otherIndex_253)), VisualJs.Obj(new V("x1"),v_ghost_238[new V("x")]), VisualJs.Obj(new V("y1"),v_ghost_238[new V("y")]), VisualJs.Obj(new V("x2"),v_p_255[new V("x")]), VisualJs.Obj(new V("y2"),v_p_255[new V("y")]), VisualJs.Obj(new V("stroke"),(v_preview_233[new V("valid")].Truth ? v_sphereColor_235 : new V("#ff4d5d"))), VisualJs.Obj(new V("strokeWidth"),(new V(2.2d) * v_scale_239)), VisualJs.Obj(new V("strokeDasharray"),new V("7 5")), VisualJs.Obj(new V("opacity"),new V(".9"))));
return default; })), new V(""), VisualJs.And(new V(v_points_243[new V("length")] >= new V(2d)), () => VisualDom.Element("polyline",VisualJs.Merge(VisualJs.Obj(new V("points"),v_points_243[new V("map")].Call(VisualJs.Fn(args_256 => { V v_p_257 = VisualJs.Arg(args_256, 0);
return ((v_p_257[new V("x")] + new V(",")) + v_p_257[new V("y")]); }))[new V("join")].Call(new V(" "))), VisualJs.Obj(new V("fill"),new V("none")), VisualJs.Obj(new V("stroke"),(v_preview_233[new V("valid")].Truth ? new V("#ffb84d") : new V("#ff4d5d"))), VisualJs.Obj(new V("strokeWidth"),(new V(2d) * v_scale_239)), VisualJs.Obj(new V("strokeDasharray"),new V("6 5")), VisualJs.Obj(new V("opacity"),new V(".82"))))), new V(""), VisualDom.Element("circle",VisualJs.Merge(VisualJs.Obj(new V("cx"),v_ghost_238[new V("x")]), VisualJs.Obj(new V("cy"),v_ghost_238[new V("y")]), VisualJs.Obj(new V("r"),(new V(25d) * v_scale_239)), VisualJs.Obj(new V("fill"),(v_preview_233[new V("valid")].Truth ? (v_sphereColor_235 + new V("18")) : new V("rgba(255,77,93,.12)"))), VisualJs.Obj(new V("stroke"),(v_preview_233[new V("valid")].Truth ? v_sphereColor_235 : new V("#ff4d5d"))), VisualJs.Obj(new V("strokeWidth"),(new V(2d) * v_scale_239)), VisualJs.Obj(new V("strokeDasharray"),new V("5 4")))), new V(""), VisualDom.Element("text",VisualJs.Merge(VisualJs.Obj(new V("x"),v_ghost_238[new V("x")]), VisualJs.Obj(new V("y"),(v_ghost_238[new V("y")] - (new V(31d) * v_scale_239))), VisualJs.Obj(new V("textAnchor"),new V("middle")), VisualJs.Obj(new V("fill"),(v_preview_233[new V("valid")].Truth ? new V("#dcecff") : new V("#ff7a86"))), VisualJs.Obj(new V("fontSize"),(new V(11d) * v_scale_239)), VisualJs.Obj(new V("fontWeight"),new V("700"))), (v_preview_233[new V("valid")].Truth ? ((v_preview_233[new V("formation")].Truth ? (new V(v_preview_233[new V("formation")][new V("type")] == new V("line")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("ЛИНИЯ") : new V("LINE"))) : (new V(v_preview_233[new V("formation")][new V("type")] == new V("triangle")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("ТРЕУГОЛЬНИК") : new V("TRIANGLE"))) : (new V(v_preview_233[new V("formation")][new V("type")] == new V("square")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("КВАДРАТ") : new V("SQUARE"))) : (new V(v_preview_233[new V("formation")][new V("type")] == new V("cluster")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("КЛАСТЕР") : new V("CLUSTER"))) : (new V(v_preview_233[new V("formation")][new V("type")] == new V("ring")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("КОЛЬЦО") : new V("RING"))) : (new V(v_preview_233[new V("formation")][new V("type")] == new V("lattice")).Truth ? ((new V(v_lang_234 == new V("ru")).Truth ? new V("РЕШЁТКА") : new V("LATTICE"))) : ((new V(v_lang_234 == new V("ru")).Truth ? new V("ФРАКТАЛ") : new V("FRACTAL"))))))))) : ((new V(v_lang_234 == new V("ru")).Truth ? new V("СЕТЬ") : new V("NETWORK"))))) : ((new V(v_lang_234 == new V("ru")).Truth ? new V("НЕДОСТУПНО") : new V("INVALID"))))), new V(""));
return default;
}
private static V F_FormationMemoryOverlay(V[] args) {
V binding_258 = VisualJs.Arg(args,0);
V v_canvasRef_259 = binding_258[new V("canvasRef")];
V v_stateRef_260 = binding_258[new V("stateRef")];
V v_memory_261 = binding_258[new V("memory")];
V v_canvas_262 = v_canvasRef_259[new V("current")];
V v_st_263 = v_stateRef_260[new V("current")];
if (VisualJs.Or((!v_canvas_262), () => (!v_st_263)).Truth) return VisualJs.Null;
V v_alpha_264 = VisualJs.MathObject[new V("min")].Call(new V(0.55d), (v_memory_261[new V("life")] / new V(0.9d)));
V v_points_265 = v_memory_261[new V("points")][new V("map")].Call(VisualJs.Fn(args_266 => { V v_point_267 = VisualJs.Arg(args_266, 0);
return Scope[new V("worldToScreen")].Call(v_canvas_262, v_st_263[new V("camera")], v_point_267); }));
V v_closed_268 = new V(v_memory_261[new V("type")] != new V("line"));
return VisualDom.Element("svg",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-0 pointer-events-none z-9 overflow-visible"))), new V(""), VisualDom.Element("polyline",VisualJs.Merge(VisualJs.Obj(new V("points"),v_points_265[new V("map")].Call(VisualJs.Fn(args_269 => { V v_p_270 = VisualJs.Arg(args_269, 0);
return ((v_p_270[new V("x")] + new V(",")) + v_p_270[new V("y")]); }))[new V("join")].Call(new V(" "))), VisualJs.Obj(new V("fill"),new V("none")), VisualJs.Obj(new V("stroke"),new V("#b6c9de")), VisualJs.Obj(new V("strokeWidth"),new V("2")), VisualJs.Obj(new V("strokeDasharray"),new V("4 6")), VisualJs.Obj(new V("opacity"),v_alpha_264))), new V(""), VisualJs.And(VisualJs.And(v_closed_268, () => new V(v_points_265[new V("length")] > new V(2d))), () => VisualDom.Element("line",VisualJs.Merge(VisualJs.Obj(new V("x1"),v_points_265[(v_points_265[new V("length")] - new V(1d))][new V("x")]), VisualJs.Obj(new V("y1"),v_points_265[(v_points_265[new V("length")] - new V(1d))][new V("y")]), VisualJs.Obj(new V("x2"),v_points_265[new V(0d)][new V("x")]), VisualJs.Obj(new V("y2"),v_points_265[new V(0d)][new V("y")]), VisualJs.Obj(new V("stroke"),new V("#b6c9de")), VisualJs.Obj(new V("strokeWidth"),new V("1.5")), VisualJs.Obj(new V("strokeDasharray"),new V("4 6")), VisualJs.Obj(new V("opacity"),(v_alpha_264 * new V(0.75d)))))), new V(""), v_points_265[new V("map")].Call(VisualJs.Fn(args_271 => { V v_p_272 = VisualJs.Arg(args_271, 0);
V v_index_273 = VisualJs.Arg(args_271, 1);
return VisualDom.Element("circle",VisualJs.Merge(VisualJs.Obj(new V("key"),v_index_273), VisualJs.Obj(new V("cx"),v_p_272[new V("x")]), VisualJs.Obj(new V("cy"),v_p_272[new V("y")]), VisualJs.Obj(new V("r"),new V("9")), VisualJs.Obj(new V("fill"),new V("none")), VisualJs.Obj(new V("stroke"),new V("#b6c9de")), VisualJs.Obj(new V("strokeWidth"),new V("1.5")), VisualJs.Obj(new V("opacity"),v_alpha_264))); })), new V(""));
return default;
}
private static V F_getAbilityCooldown(V[] args) {
V v_st_274 = VisualJs.Arg(args,0);
V v_ability_275 = VisualJs.Arg(args,1);
if ((!v_st_274).Truth) return new V(0d);
V v_cooldownMap_276 = VisualJs.Obj(new V("blast"), v_st_274[new V("player")][new V("blastCooldown")], new V("shield"), v_st_274[new V("player")][new V("shieldCooldown")], new V("teleport"), v_st_274[new V("player")][new V("teleportCooldown")], new V("firetrail"), v_st_274[new V("player")][new V("fireTrailCooldown")], new V("minion"), v_st_274[new V("player")][new V("minionCooldown")], new V("lightning"), v_st_274[new V("player")][new V("lightningCooldown")], new V("timestop"), v_st_274[new V("player")][new V("timestopCooldown")], new V("darkritual"), v_st_274[new V("player")][new V("darkritualCooldown")]);
return VisualJs.Or(v_cooldownMap_276[v_ability_275], () => new V(0d));
return default;
}
private static V F_getAbilityMaxCooldown(V[] args) {
V v_ability_277 = VisualJs.Arg(args,0);
V v_map_278 = VisualJs.Obj(new V("blast"), new V(30d), new V("shield"), new V(20d), new V("teleport"), new V(15d), new V("firetrail"), new V(25d), new V("minion"), new V(30d), new V("lightning"), new V(20d), new V("timestop"), new V(40d), new V("darkritual"), new V(30d));
return VisualJs.Or(v_map_278[v_ability_277], () => new V(1d));
return default;
}
private static V F_stDashLabel(V[] args) {
V v_st_279 = VisualJs.Arg(args,0);
V v_lang_280 = VisualJs.Arg(args,1);
V v_t_281 = VisualJs.Arg(args,2);
if ((!v_st_279).Truth) return v_t_281.Call(new V("dashCooldown"));
return (new V(v_st_279[new V("player")][new V("dashCooldown")] > new V(0d)).Truth ? v_t_281.Call(new V("dashOnCooldown"))[new V("replace")].Call(new V("{sec}"), new V(VisualJs.MathObject[new V("ceil")].Call(v_st_279[new V("player")][new V("dashCooldown")]).Text)) : ((new V(v_lang_280 == new V("ru")).Truth ? new V("Рывок") : new V("Dash"))));
return default;
}
}
}
