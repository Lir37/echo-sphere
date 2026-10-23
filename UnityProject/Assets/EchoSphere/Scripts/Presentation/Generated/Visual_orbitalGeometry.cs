#if UNITY_EDITOR
// Generated from src/spheres/orbitalGeometry.ts; sha256 19a7e978d861afeed8c7d27f3275842b5d595bdfd3e3988b7e4fff17cfe1fcaf.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_orbitalGeometry {
public static readonly V Scope=VisualJs.Obj();
static Visual_orbitalGeometry() {
Scope[new V("getOrbitalRingCounts")] = VisualJs.Fn(F_getOrbitalRingCounts);
Scope[new V("orbitalElementAngle")] = VisualJs.Fn(F_orbitalElementAngle);
Scope[new V("TAU")] = (VisualJs.MathObject[new V("PI")] * new V(2d));
}
private static V F_getOrbitalRingCounts(V[] args) {
V v_level_0 = VisualJs.Arg(args,0);
V v_extraElements_1 = VisualJs.Arg(args,1).Undefined?new V(0d):VisualJs.Arg(args,1);
V v_effectiveLevel_2 = VisualJs.MathObject[new V("max")].Call(new V(1d), VisualJs.MathObject[new V("min")].Call(new V(7d), VisualJs.MathObject[new V("floor")].Call(v_level_0)));
V v_inner_3 = new V(1d);
V v_outer_4 = new V(1d);
for (V v_currentLevel_5 = new V(2d); new V(v_currentLevel_5 <= v_effectiveLevel_2).Truth; v_currentLevel_5 += new V(1d)) {
if (new V((v_currentLevel_5 % new V(2d)) == new V(0d)).Truth) v_outer_4 += new V(1d); else v_inner_3 += new V(1d);
}
for (V v_extra_6 = new V(0d); new V(v_extra_6 < VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("floor")].Call(v_extraElements_1))).Truth; v_extra_6 += new V(1d)) {
if (new V(v_outer_4 < v_inner_3).Truth) v_outer_4 += new V(1d); else if (new V(v_inner_3 < v_outer_4).Truth) v_inner_3 += new V(1d); else if (new V((v_extra_6 % new V(2d)) == new V(0d)).Truth) v_outer_4 += new V(1d); else v_inner_3 += new V(1d);
}
return VisualJs.Obj(new V("inner"), v_inner_3, new V("outer"), v_outer_4);
}
private static V F_orbitalElementAngle(V[] args) {
V v_rotation_7 = VisualJs.Arg(args,0);
V v_ring_8 = VisualJs.Arg(args,1);
V v_index_9 = VisualJs.Arg(args,2);
V v_count_10 = VisualJs.Arg(args,3);
V v_safeCount_11 = VisualJs.MathObject[new V("max")].Call(new V(1d), v_count_10);
V v_direction_12 = (new V(v_ring_8 == new V("inner")).Truth ? new V(1d) : (-new V(1d)));
return ((v_rotation_7 * v_direction_12) + (v_index_9 * ((Scope[new V("TAU")] / v_safeCount_11))));
}
}
}
#endif
