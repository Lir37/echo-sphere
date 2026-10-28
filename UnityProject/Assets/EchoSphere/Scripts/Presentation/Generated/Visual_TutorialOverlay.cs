#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
// Generated from src/TutorialOverlay.tsx; sha256 fcbaf8e953003e58fee957fec589c713b2f983b764ea68f9df42be7651af2ac4.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_TutorialOverlay {
public static readonly V Scope=VisualJs.Obj();
static Visual_TutorialOverlay() {
Scope[new V("clamp")] = VisualJs.Fn(F_clamp);
Scope[new V("TutorialOverlay")] = VisualJs.Fn(F_TutorialOverlay);
Scope[new V("STEP_CONFIG")] = VisualJs.Obj(new V("0"), VisualJs.Obj(new V("title"), new V("tutorialWelcomeTitle"), new V("body"), new V("tutorialWelcomeBody"), new V("target"), VisualJs.Null), new V("1"), VisualJs.Obj(new V("title"), new V("tutorialMoveTitle"), new V("body"), new V("tutorialMoveBody"), new V("target"), new V("movement"), new V("action"), new V("tutorialMoveDone")), new V("2"), VisualJs.Obj(new V("title"), new V("tutorialPlaceTitle"), new V("body"), new V("tutorialPlaceBody"), new V("target"), new V("field"), new V("action"), new V("tutorialPlaceDone")), new V("3"), VisualJs.Obj(new V("title"), new V("tutorialNetworkTitle"), new V("body"), new V("tutorialNetworkBody"), new V("target"), new V("network")), new V("4"), VisualJs.Obj(new V("title"), new V("tutorialUpgradeTitle"), new V("body"), new V("tutorialUpgradeBody"), new V("target"), new V("upgrade")), new V("5"), VisualJs.Obj(new V("title"), new V("tutorialDoneTitle"), new V("body"), new V("tutorialDoneBody"), new V("target"), VisualJs.Null));
}
private static V F_clamp(V[] args) {
V v_value_0 = VisualJs.Arg(args,0);
V v_min_1 = VisualJs.Arg(args,1);
V v_max_2 = VisualJs.Arg(args,2);
return VisualJs.MathObject[new V("min")].Call(v_max_2, VisualJs.MathObject[new V("max")].Call(v_min_1, v_value_0));
}
private static V F_TutorialOverlay(V[] args) {
V binding_3 = VisualJs.Arg(args,0);
V v_step_4 = binding_3[new V("step")];
V v_lang_5 = binding_3[new V("lang")];
V v_t_6 = binding_3[new V("t")];
V v_handedness_7 = binding_3[new V("handedness")];
V v_canAdvance_8 = binding_3[new V("canAdvance")];
V v_onNext_9 = binding_3[new V("onNext")];
V v_onSkip_10 = binding_3[new V("onSkip")];
V v_config_11 = Scope[new V("STEP_CONFIG")][v_step_4];
V binding_12 = VisualDom.React("useState").Call(VisualJs.Null);
V v_targetRect_13 = binding_12[new V(0d)];
V v_setTargetRect_14 = binding_12[new V(1d)];

VisualDom.React("useEffect").Call(VisualJs.Fn(args_15 => { V v_update_16 = default; v_update_16 = VisualJs.Fn(args_17 => { if ((!v_config_11[new V("target")]).Truth) {
v_setTargetRect_14.Call(VisualJs.Null);
return default;
}
V v_node_18 = VisualUiBindings.Get("global","document")[new V("querySelector")].Call((new V("[data-tutorial-target=\"") + v_config_11[new V("target")] + new V("\"]")));
v_setTargetRect_14.Call(VisualJs.Coalesce(VisualJs.Chain(v_node_18,optional_19=>optional_19[new V("getBoundingClientRect")].Call()), () => VisualJs.Null));
return default; });
v_update_16.Call();
V v_id_20 = VisualUiBindings.Get("global","window")[new V("setInterval")].Call(v_update_16, new V(120d));
VisualUiBindings.Get("global","window")[new V("addEventListener")].Call(new V("resize"), v_update_16);
VisualUiBindings.Get("global","window")[new V("addEventListener")].Call(new V("scroll"), v_update_16, new V(true));
return VisualJs.Fn(args_21 => { VisualUiBindings.Get("global","window")[new V("clearInterval")].Call(v_id_20);
VisualUiBindings.Get("global","window")[new V("removeEventListener")].Call(new V("resize"), v_update_16);
VisualUiBindings.Get("global","window")[new V("removeEventListener")].Call(new V("scroll"), v_update_16, new V(true));
return default; }); }), VisualJs.Arr(v_config_11[new V("target")], v_handedness_7, v_step_4));
V v_cardStyle_22 = VisualDom.React("useMemo").Call(VisualJs.Fn(args_23 => { V v_width_24 = VisualJs.MathObject[new V("min")].Call(new V(340d), (VisualUiBindings.Get("global","window")[new V("innerWidth")] - new V(24d)));
if ((!v_targetRect_13).Truth) {
return (VisualJs.Obj(new V("width"), (new V("") + v_width_24 + new V("px")), new V("left"), (new V("") + VisualJs.MathObject[new V("max")].Call(new V(12d), (((VisualUiBindings.Get("global","window")[new V("innerWidth")] - v_width_24)) / new V(2d))) + new V("px")), new V("top"), new V("50%"), new V("transform"), new V("translateY(-50%)")));
}
V v_above_25 = new V(v_targetRect_13[new V("top")] > (VisualUiBindings.Get("global","window")[new V("innerHeight")] * new V(0.48d)));
V v_top_26 = (v_above_25.Truth ? (v_targetRect_13[new V("top")] - new V(170d)) : (v_targetRect_13[new V("bottom")] + new V(14d)));
return (VisualJs.Obj(new V("width"), (new V("") + v_width_24 + new V("px")), new V("left"), (new V("") + Scope[new V("clamp")].Call(((v_targetRect_13[new V("left")] + (v_targetRect_13[new V("width")] / new V(2d))) - (v_width_24 / new V(2d))), new V(12d), ((VisualUiBindings.Get("global","window")[new V("innerWidth")] - v_width_24) - new V(12d))) + new V("px")), new V("top"), (new V("") + Scope[new V("clamp")].Call(v_top_26, new V(12d), (VisualUiBindings.Get("global","window")[new V("innerHeight")] - new V(188d))) + new V("px")))); }), VisualJs.Arr(v_targetRect_13));
V v_cutoutStyle_27 = (v_targetRect_13.Truth ? VisualJs.Obj(new V("left"), VisualJs.MathObject[new V("max")].Call(new V(4d), (v_targetRect_13[new V("left")] - new V(8d))), new V("top"), VisualJs.MathObject[new V("max")].Call(new V(4d), (v_targetRect_13[new V("top")] - new V(8d))), new V("width"), (v_targetRect_13[new V("width")] + new V(16d)), new V("height"), (v_targetRect_13[new V("height")] + new V(16d))) : VisualJs.Null);
V v_ru_28 = new V(v_lang_5 == new V("ru"));
return (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute inset-0 z-[80] pointer-events-none")), VisualJs.Obj(new V("aria-live"),new V("polite"))), new V(""), VisualJs.And(v_cutoutStyle_27, () => (VisualDom.Element("fragment",VisualJs.Obj(), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute rounded-xl border-2 border-[#63e6ff] shadow-[0_0_24px_rgba(57,216,255,.28)]")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("left"), v_cutoutStyle_27[new V("left")], new V("top"), v_cutoutStyle_27[new V("top")], new V("width"), v_cutoutStyle_27[new V("width")], new V("height"), v_cutoutStyle_27[new V("height")], new V("boxShadow"), new V("0 0 0 9999px rgba(0,0,0,.72), 0 0 24px rgba(57,216,255,.30)"))))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute rounded-xl border border-[#dcecff]/30 animate-pulse")), VisualJs.Obj(new V("style"),VisualJs.Obj(new V("left"), (v_cutoutStyle_27[new V("left")] - new V(3d)), new V("top"), (v_cutoutStyle_27[new V("top")] - new V(3d)), new V("width"), (v_cutoutStyle_27[new V("width")] + new V(6d)), new V("height"), (v_cutoutStyle_27[new V("height")] + new V(6d)))))), new V("")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("absolute rounded-2xl border border-[#63e6ff]/30 bg-[#030a14]/95 px-4 py-4 text-[#dcecff] shadow-[0_16px_50px_rgba(0,0,0,.55)] pointer-events-auto")), VisualJs.Obj(new V("style"),v_cardStyle_22)), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("text-[9px] uppercase tracking-[.22em] font-black text-[#63e6ff]"))), new V(""), (v_ru_28.Truth ? (new V("ОБУЧЕНИЕ · ") + (v_step_4 + new V(1d)) + new V("/6")) : (new V("TUTORIAL · ") + (v_step_4 + new V(1d)) + new V("/6"))), new V("")), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("mt-1 text-lg font-bold"))), v_t_6.Call(v_config_11[new V("title")])), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("mt-2 text-xs leading-relaxed text-[#a9c0d2]"))), v_t_6.Call(v_config_11[new V("body")])), new V(""), VisualJs.And(VisualJs.And(v_config_11[new V("action")], () => new V(v_step_4 != new V(4d))), () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),(new V("mt-3 rounded-lg border px-3 py-2 text-[10px] font-bold ") + (v_canAdvance_8.Truth ? new V("border-[#55e6c1]/40 bg-[#55e6c1]/10 text-[#b9ffe8]") : new V("border-[#243b55] bg-[#09121f] text-[#7089a0]")) + new V("")))), new V(""), (v_canAdvance_8.Truth ? v_t_6.Call(v_config_11[new V("action")]) : ((v_ru_28.Truth ? new V("Выполни действие, чтобы продолжить.") : new V("Perform the action to continue.")))), new V("")))), new V(""), VisualJs.And(new V(v_step_4 == new V(4d)), () => (VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("mt-3 rounded-lg border border-[#b68cff]/30 bg-[#b68cff]/10 px-3 py-2 text-[10px] font-bold text-[#dbcfff]"))), new V(""), (v_ru_28.Truth ? new V("Выбери одну карту. Обучение продолжится после выбора.") : new V("Choose one card. The tutorial continues after your choice.")), new V("")))), new V(""), VisualDom.Element("div",VisualJs.Merge(VisualJs.Obj(new V("className"),new V("mt-4 flex items-center justify-between gap-2"))), new V(""), VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("type"),new V("button")), VisualJs.Obj(new V("onClick"),v_onSkip_10), VisualJs.Obj(new V("className"),new V("rounded-lg px-2 py-2 text-[9px] font-bold uppercase tracking-wider text-[#718aa1] pointer-events-auto"))), new V(""), v_t_6.Call(new V("tutorialSkip")), new V("")), new V(""), (VisualJs.And(VisualJs.And(new V(v_step_4 != new V(1d)), () => new V(v_step_4 != new V(2d))), () => new V(v_step_4 != new V(4d))).Truth ? (VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("type"),new V("button")), VisualJs.Obj(new V("onClick"),v_onNext_9), VisualJs.Obj(new V("disabled"),(!v_canAdvance_8)), VisualJs.Obj(new V("className"),new V("rounded-xl border border-[#63e6ff]/40 bg-[#63e6ff]/10 px-4 py-2.5 text-[10px] font-black uppercase tracking-wider text-[#dffaff] disabled:opacity-30"))), new V(""), v_t_6.Call(new V("tutorialNext")), new V(""))) : ((VisualJs.Or(new V(v_step_4 == new V(1d)), () => new V(v_step_4 == new V(2d))).Truth ? (VisualDom.Element("button",VisualJs.Merge(VisualJs.Obj(new V("type"),new V("button")), VisualJs.Obj(new V("onClick"),v_onNext_9), VisualJs.Obj(new V("disabled"),(!v_canAdvance_8)), VisualJs.Obj(new V("className"),new V("rounded-xl border border-[#63e6ff]/40 bg-[#63e6ff]/10 px-4 py-2.5 text-[10px] font-black uppercase tracking-wider text-[#dffaff] disabled:opacity-30"))), new V(""), v_t_6.Call(new V("tutorialNext")), new V(""))) : VisualJs.Null))), new V("")), new V("")), new V("")));
}
}
}
#endif
