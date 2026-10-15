// Generated from src/renderer.ts; sha256 291bf47ec7e2408e4aa0acac7e70a9bcdaedc95936c59adc9f8ca95d711ec29d.
// Do not hand-edit authored drawing expressions. Run export-unity-visual-code.mjs.
using System;
using System.Linq;
namespace EchoSphere.Presentation {
internal static class Visual_renderer {
public static readonly V Scope=VisualJs.Obj();
static Visual_renderer() {
Scope[new V("getArtImage")] = VisualJs.Fn(F_getArtImage);
Scope[new V("drawArtSprite")] = VisualJs.Fn(F_drawArtSprite);
Scope[new V("drawStandardSpherePart")] = VisualJs.Fn(F_drawStandardSpherePart);
Scope[new V("drawStandardSphereCore")] = VisualJs.Fn(F_drawStandardSphereCore);
Scope[new V("drawStandardSphereRing")] = VisualJs.Fn(F_drawStandardSphereRing);
Scope[new V("drawStandardSphereAssembly")] = VisualJs.Fn(F_drawStandardSphereAssembly);
Scope[new V("getSpheristAnimationState")] = VisualJs.Fn(F_getSpheristAnimationState);
Scope[new V("render")] = VisualJs.Fn(F_render);
Scope[new V("drawRegionLayer")] = VisualJs.Fn(F_drawRegionLayer);
Scope[new V("drawCharacterHud")] = VisualJs.Fn(F_drawCharacterHud);
Scope[new V("drawPlayerShield")] = VisualJs.Fn(F_drawPlayerShield);
Scope[new V("drawStellaChest")] = VisualJs.Fn(F_drawStellaChest);
Scope[new V("drawCharacterWorldIndicators")] = VisualJs.Fn(F_drawCharacterWorldIndicators);
Scope[new V("drawCharacterTargetIndicators")] = VisualJs.Fn(F_drawCharacterTargetIndicators);
Scope[new V("drawEngineerLinks")] = VisualJs.Fn(F_drawEngineerLinks);
Scope[new V("drawArchitectFormation")] = VisualJs.Fn(F_drawArchitectFormation);
Scope[new V("drawHunterTarget")] = VisualJs.Fn(F_drawHunterTarget);
Scope[new V("drawAlchemistReactions")] = VisualJs.Fn(F_drawAlchemistReactions);
Scope[new V("drawBerserkerRange")] = VisualJs.Fn(F_drawBerserkerRange);
Scope[new V("countStatusEffects")] = VisualJs.Fn(F_countStatusEffects);
Scope[new V("dist2D")] = VisualJs.Fn(F_dist2D);
Scope[new V("drawPolyline")] = VisualJs.Fn(F_drawPolyline);
Scope[new V("drawPolygon")] = VisualJs.Fn(F_drawPolygon);
Scope[new V("drawVoidAmbient")] = VisualJs.Fn(F_drawVoidAmbient);
Scope[new V("drawVoidField")] = VisualJs.Fn(F_drawVoidField);
Scope[new V("drawModernXp")] = VisualJs.Fn(F_drawModernXp);
Scope[new V("drawModernHealth")] = VisualJs.Fn(F_drawModernHealth);
Scope[new V("drawModernMinion")] = VisualJs.Fn(F_drawModernMinion);
Scope[new V("drawEnergyBolt")] = VisualJs.Fn(F_drawEnergyBolt);
Scope[new V("hexToRgb")] = VisualJs.Fn(F_hexToRgb);
Scope[new V("drawPlayer")] = VisualJs.Fn(F_drawPlayer);
Scope[new V("getEnemyFacingAngle")] = VisualJs.Fn(F_getEnemyFacingAngle);
Scope[new V("glowCircle")] = VisualJs.Fn(F_glowCircle);
Scope[new V("drawGroundShadow")] = VisualJs.Fn(F_drawGroundShadow);
Scope[new V("drawSphereVfx")] = VisualJs.Fn(F_drawSphereVfx);
Scope[new V("drawCharacterVfx")] = VisualJs.Fn(F_drawCharacterVfx);
Scope[new V("drawEliteCrest")] = VisualJs.Fn(F_drawEliteCrest);
Scope[new V("drawEnemyStatusVfx")] = VisualJs.Fn(F_drawEnemyStatusVfx);
Scope[new V("drawStandardAttackVfx")] = VisualJs.Fn(F_drawStandardAttackVfx);
Scope[new V("drawModernSphere")] = VisualJs.Fn(F_drawModernSphere);
Scope[new V("drawFormationMemory")] = VisualJs.Fn(F_drawFormationMemory);
Scope[new V("drawWavyNetworkLink")] = VisualJs.Fn(F_drawWavyNetworkLink);
Scope[new V("drawSphereNetwork")] = VisualJs.Fn(F_drawSphereNetwork);
Scope[new V("drawBossAttackTelegraph")] = VisualJs.Fn(F_drawBossAttackTelegraph);
Scope[new V("drawModernEnemy")] = VisualJs.Fn(F_drawModernEnemy);
Scope[new V("drawLinkBreakerTelegraph")] = VisualJs.Fn(F_drawLinkBreakerTelegraph);
Scope[new V("drawNetworkDisabledIndicator")] = VisualJs.Fn(F_drawNetworkDisabledIndicator);
Scope[new V("drawEnemyProjectile")] = VisualJs.Fn(F_drawEnemyProjectile);
Scope[new V("drawBossProjectile")] = VisualJs.Fn(F_drawBossProjectile);
Scope[new V("drawChest")] = VisualJs.Fn(F_drawChest);
Scope[new V("drawRune")] = VisualJs.Fn(F_drawRune);
Scope[new V("ART_PATHS")] = VisualJs.Obj(new V("player"), new V("/art/player.svg"), new V("player-spherist-shield"), new V("/art/spherist-shield-96.png"), new V("player-spherist"), new V("/art/spherist-hybrid-front.svg"), new V("player-spherist-side"), new V("/art/spherist-hybrid-3q.svg"), new V("sphere-standard-upper-crystal"), new V("/art/standard-sphere/upper-crystal.png"), new V("sphere-standard-core"), new V("/art/standard-sphere/energy-core.png"), new V("sphere-standard-ring"), new V("/art/standard-sphere/stabilization-ring.png"), new V("sphere-standard-lower-crystal"), new V("/art/standard-sphere/lower-crystal.png"), new V("sphere-sniper"), new V("/art/sniper.svg"), new V("sphere-shotgun"), new V("/art/shotgun.svg"), new V("sphere-chain"), new V("/art/chain.svg"), new V("sphere-aura"), new V("/art/aura.svg"), new V("sphere-prism"), new V("/art/prism.svg"), new V("sphere-gravity"), new V("/art/gravity.svg"), new V("sphere-pulse"), new V("/art/pulse.svg"), new V("sphere-void"), new V("/art/void.svg"), new V("boss"), new V("/art/boss.svg"));
Scope[new V("ART_CACHE")] = VisualJs.CreateMap(default(V),false);
Scope[new V("RENDER_TIME")] = new V(0d);
Scope[new V("RENDER_COST_MS")] = new V(0d);
Scope[new V("RENDER_LAST_LOG")] = (-new V(10d));
Scope[new V("SPHERIST_LAST_X")] = new V(0d);
Scope[new V("SPHERIST_LAST_Y")] = new V(0d);
Scope[new V("SPHERIST_LAST_TIME")] = (-new V(1d));
Scope[new V("SPHERIST_VX")] = new V(0d);
Scope[new V("SPHERIST_VY")] = new V(0d);
Scope[new V("SPHERIST_ANIMATION_STATE")] = new V("idle");
Scope[new V("CANONICAL_THEME")] = VisualJs.Obj(new V("bg"), new V("#07101d"), new V("bgDark"), new V("#040912"), new V("grid"), new V("rgba(120,170,220,0.075)"), new V("border"), new V("rgba(100,180,255,0.24)"), new V("accent"), new V("#39d8ff"), new V("textureDots"), VisualJs.Arr(new V("rgba(80,150,220,0.16)"), new V("rgba(160,200,255,0.07)")));
Scope[new V("ENEMY_LAST_POS")] = VisualJs.CreateMap(default(V),true);
Scope[new V("ENEMY_FACING")] = VisualJs.CreateMap(default(V),true);
}
private static V F_getArtImage(V[] args) {
V v_key_0 = VisualJs.Arg(args,0);
V v_cached_1 = Scope[new V("ART_CACHE")][new V("get")].Call(v_key_0);
if (v_cached_1.Truth) return (VisualJs.And(v_cached_1[new V("complete")], () => new V(v_cached_1[new V("naturalWidth")] > new V(0d))).Truth ? v_cached_1 : VisualJs.Null);
V v_image_2 = VisualJs.CreateImage();
VisualJs.Assign(v_image_2,new V("decoding"),"=",new V("async"));
Scope[new V("ART_CACHE")][new V("set")].Call(v_key_0, v_image_2);
VisualJs.Assign(v_image_2,new V("src"),"=",Scope[new V("ART_PATHS")][v_key_0]);
return VisualJs.Null;
}
private static V F_drawArtSprite(V[] args) {
V v_ctx_3 = VisualJs.Arg(args,0);
V v_key_4 = VisualJs.Arg(args,1);
V v_x_5 = VisualJs.Arg(args,2);
V v_y_6 = VisualJs.Arg(args,3);
V v_size_7 = VisualJs.Arg(args,4);
V v_rotation_8 = VisualJs.Arg(args,5).Undefined?new V(0d):VisualJs.Arg(args,5);
V v_opacity_9 = VisualJs.Arg(args,6).Undefined?new V(1d):VisualJs.Arg(args,6);
V v_image_10 = Scope[new V("getArtImage")].Call(v_key_4);
if ((!v_image_10).Truth) return new V(false);
v_ctx_3[new V("save")].Call();
v_ctx_3[new V("translate")].Call(v_x_5, v_y_6);
v_ctx_3[new V("rotate")].Call(v_rotation_8);
VisualJs.Assign(v_ctx_3,new V("globalAlpha"),"=",v_opacity_9);
V v_iw_11 = VisualJs.MathObject[new V("max")].Call(new V(1d), v_image_10[new V("naturalWidth")]);
V v_ih_12 = VisualJs.MathObject[new V("max")].Call(new V(1d), v_image_10[new V("naturalHeight")]);
V v_imageScale_13 = VisualJs.MathObject[new V("min")].Call((v_size_7 / v_iw_11), (v_size_7 / v_ih_12));
V v_dw_14 = (v_iw_11 * v_imageScale_13);
V v_dh_15 = (v_ih_12 * v_imageScale_13);
v_ctx_3[new V("drawImage")].Call(v_image_10, ((-v_dw_14) / new V(2d)), ((-v_dh_15) / new V(2d)), v_dw_14, v_dh_15);
v_ctx_3[new V("restore")].Call();
return new V(true);
}
private static V F_drawStandardSpherePart(V[] args) {
V v_ctx_16 = VisualJs.Arg(args,0);
V v_key_17 = VisualJs.Arg(args,1);
V v_x_18 = VisualJs.Arg(args,2);
V v_y_19 = VisualJs.Arg(args,3);
V v_size_20 = VisualJs.Arg(args,4);
V v_rotation_21 = VisualJs.Arg(args,5);
V v_opacity_22 = VisualJs.Arg(args,6).Undefined?new V(1d):VisualJs.Arg(args,6);
V v_mirrorX_23 = VisualJs.Arg(args,7).Undefined?new V(false):VisualJs.Arg(args,7);
V v_mirrorY_24 = VisualJs.Arg(args,8).Undefined?new V(false):VisualJs.Arg(args,8);
V v_image_25 = Scope[new V("getArtImage")].Call(v_key_17);
if ((!v_image_25).Truth) return new V(false);
V v_iw_26 = VisualJs.MathObject[new V("max")].Call(new V(1d), v_image_25[new V("naturalWidth")]);
V v_ih_27 = VisualJs.MathObject[new V("max")].Call(new V(1d), v_image_25[new V("naturalHeight")]);
V v_scale_28 = VisualJs.MathObject[new V("min")].Call((v_size_20 / v_iw_26), (v_size_20 / v_ih_27));
V v_dw_29 = (v_iw_26 * v_scale_28);
V v_dh_30 = (v_ih_27 * v_scale_28);
v_ctx_16[new V("save")].Call();
v_ctx_16[new V("translate")].Call(v_x_18, v_y_19);
v_ctx_16[new V("rotate")].Call(v_rotation_21);
v_ctx_16[new V("scale")].Call((v_mirrorX_23.Truth ? (-new V(1d)) : new V(1d)), (v_mirrorY_24.Truth ? (-new V(1d)) : new V(1d)));
VisualJs.Assign(v_ctx_16,new V("globalAlpha"),"=",v_opacity_22);
VisualJs.Assign(v_ctx_16,new V("globalCompositeOperation"),"=",new V("screen"));
v_ctx_16[new V("drawImage")].Call(v_image_25, ((-v_dw_29) / new V(2d)), ((-v_dh_30) / new V(2d)), v_dw_29, v_dh_30);
v_ctx_16[new V("restore")].Call();
return new V(true);
}
private static V F_drawStandardSphereCore(V[] args) {
V v_ctx_31 = VisualJs.Arg(args,0);
V v_r_32 = VisualJs.Arg(args,1);
V v_pulse_33 = VisualJs.Arg(args,2);
V v_glow_34 = v_ctx_31[new V("createRadialGradient")].Call(((-v_r_32) * new V(0.08d)), ((-v_r_32) * new V(0.1d)), new V(1d), new V(0d), new V(0d), (v_r_32 * new V(0.62d)));
v_glow_34[new V("addColorStop")].Call(new V(0d), new V("rgba(255,255,255,0.98)"));
v_glow_34[new V("addColorStop")].Call(new V(0.2d), new V("rgba(176,244,255,0.96)"));
v_glow_34[new V("addColorStop")].Call(new V(0.48d), new V("rgba(38,170,255,0.78)"));
v_glow_34[new V("addColorStop")].Call(new V(0.82d), new V("rgba(20,92,220,0.34)"));
v_glow_34[new V("addColorStop")].Call(new V(1d), new V("rgba(0,30,90,0)"));
v_ctx_31[new V("save")].Call();
VisualJs.Assign(v_ctx_31,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_31,new V("globalAlpha"),"=",(new V(0.82d) + (v_pulse_33 * new V(0.14d))));
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",v_glow_34);
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("arc")].Call(new V(0d), new V(0d), ((v_r_32 * new V(0.58d)) * ((new V(1d) + (v_pulse_33 * new V(0.035d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_31[new V("fill")].Call();
VisualJs.Assign(v_ctx_31,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_31,new V("fillStyle"),"=",new V("rgba(30,135,232,0.72)"));
VisualJs.Assign(v_ctx_31,new V("strokeStyle"),"=",new V("rgba(182,247,255,0.90)"));
VisualJs.Assign(v_ctx_31,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_r_32 * new V(0.035d))));
v_ctx_31[new V("beginPath")].Call();
v_ctx_31[new V("arc")].Call(new V(0d), new V(0d), (v_r_32 * new V(0.42d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_31[new V("fill")].Call();
v_ctx_31[new V("stroke")].Call();
v_ctx_31[new V("restore")].Call();
return default;
}
private static V F_drawStandardSphereRing(V[] args) {
V v_ctx_35 = VisualJs.Arg(args,0);
V v_r_36 = VisualJs.Arg(args,1);
V v_rotation_37 = VisualJs.Arg(args,2);
V v_opacity_38 = VisualJs.Arg(args,3);
v_ctx_35[new V("save")].Call();
v_ctx_35[new V("rotate")].Call(v_rotation_37);
VisualJs.Assign(v_ctx_35,new V("globalAlpha"),"=",v_opacity_38);
VisualJs.Assign(v_ctx_35,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_35,new V("strokeStyle"),"=",new V("rgba(48,214,255,0.82)"));
VisualJs.Assign(v_ctx_35,new V("shadowColor"),"=",new V("#36dfff"));
VisualJs.Assign(v_ctx_35,new V("shadowBlur"),"=",new V(3d));
VisualJs.Assign(v_ctx_35,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.9d), (v_r_36 * new V(0.045d))));
v_ctx_35[new V("beginPath")].Call();
v_ctx_35[new V("ellipse")].Call(new V(0d), new V(0d), (v_r_36 * new V(0.98d)), (v_r_36 * new V(0.34d)), new V(0d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_35[new V("stroke")].Call();
VisualJs.Assign(v_ctx_35,new V("globalAlpha"),"=",(v_opacity_38 * new V(0.55d)));
VisualJs.Assign(v_ctx_35,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.55d), (v_r_36 * new V(0.022d))));
v_ctx_35[new V("beginPath")].Call();
v_ctx_35[new V("ellipse")].Call(new V(0d), new V(0d), (v_r_36 * new V(0.88d)), (v_r_36 * new V(0.27d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(0.1d)), (VisualJs.MathObject[new V("PI")] * new V(1.92d)));
v_ctx_35[new V("stroke")].Call();
v_ctx_35[new V("restore")].Call();
return default;
}
private static V F_drawStandardSphereAssembly(V[] args) {
V v_ctx_39 = VisualJs.Arg(args,0);
V v_s_40 = VisualJs.Arg(args,1);
V v_sphere_41 = VisualJs.Arg(args,2);
V v_r_42 = VisualJs.Arg(args,3);
V v_time_43 = VisualJs.Arg(args,4);
V v_tier_44 = VisualJs.Or(v_sphere_41[new V("visualTier")], () => new V(0d));
V v_target_45 = v_s_40[new V("enemies")][new V("some")].Call(VisualJs.Fn(args_46 => { V v_enemy_47 = VisualJs.Arg(args_46, 0);
return (VisualJs.And(new V(v_enemy_47[new V("hp")] > new V(0d)), () => new V(VisualJs.MathObject[new V("hypot")].Call((v_enemy_47[new V("pos")][new V("x")] - v_sphere_41[new V("pos")][new V("x")]), (v_enemy_47[new V("pos")][new V("y")] - v_sphere_41[new V("pos")][new V("y")])) < (v_sphere_41[new V("radius")] * Visual_gameData.Scope[new V("SPHERE_TYPES")][v_sphere_41[new V("type")]][new V("rangeMult")])))); }));
V v_breathe_48 = (VisualJs.MathObject[new V("sin")].Call(((v_time_43 * new V(2.2d)) + (v_sphere_41[new V("pos")][new V("x")] * new V(0.013d)))) * new V(0.028d));
V v_energyPulse_49 = (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call(((v_time_43 * new V(4.6d)) + (v_sphere_41[new V("pos")][new V("y")] * new V(0.009d))))));
V v_targetPulse_50 = (v_target_45.Truth ? (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call((v_time_43 * new V(11d))))) : new V(0d));
V v_attackScale_51 = (new V(1d) + (v_targetPulse_50 * new V(0.025d)));
V v_ringRotation_52 = (v_time_43 * ((new V(0.22d) + (v_tier_44 * new V(0.018d)))));
V v_crystalPulse_53 = (new V(1d) + (v_energyPulse_49 * new V(0.022d)));
v_ctx_39[new V("save")].Call();
v_ctx_39[new V("translate")].Call(v_sphere_41[new V("pos")][new V("x")], v_sphere_41[new V("pos")][new V("y")]);
v_ctx_39[new V("scale")].Call((v_attackScale_51 + (v_breathe_48 * new V(0.18d))), (v_attackScale_51 - (v_breathe_48 * new V(0.12d))));
V v_coreRadius_54 = v_r_42;
V v_ringRadius_55 = (v_r_42 * new V(1.18d));
V v_crystalSize_56 = (v_r_42 * new V(1.34d));
Scope[new V("drawStandardSphereCore")].Call(v_ctx_39, v_coreRadius_54, v_energyPulse_49);
Scope[new V("drawStandardSphereRing")].Call(v_ctx_39, v_ringRadius_55, v_ringRotation_52, (new V(0.72d) + (v_targetPulse_50 * new V(0.2d))));
Scope[new V("drawStandardSpherePart")].Call(v_ctx_39, new V("sphere-standard-upper-crystal"), new V(0d), (((-v_r_42) * new V(0.62d)) - ((v_energyPulse_49 * v_r_42) * new V(0.01d))), (v_crystalSize_56 * v_crystalPulse_53), new V(0d), new V(1d));
Scope[new V("drawStandardSpherePart")].Call(v_ctx_39, new V("sphere-standard-upper-crystal"), new V(0d), ((v_r_42 * new V(0.66d)) + ((v_energyPulse_49 * v_r_42) * new V(0.01d))), (v_crystalSize_56 * v_crystalPulse_53), new V(0d), new V(1d), new V(false), new V(true));
if (new V(v_targetPulse_50 > new V(0.55d)).Truth) {
v_ctx_39[new V("save")].Call();
VisualJs.Assign(v_ctx_39,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_39,new V("globalAlpha"),"=",(new V(0.14d) + (v_targetPulse_50 * new V(0.12d))));
VisualJs.Assign(v_ctx_39,new V("strokeStyle"),"=",new V("#d9fbff"));
VisualJs.Assign(v_ctx_39,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_42 * new V(0.03d))));
v_ctx_39[new V("beginPath")].Call();
v_ctx_39[new V("moveTo")].Call(((-v_r_42) * new V(0.44d)), ((-v_r_42) * new V(0.22d)));
v_ctx_39[new V("lineTo")].Call(((-v_r_42) * new V(0.08d)), ((-v_r_42) * new V(0.48d)));
v_ctx_39[new V("lineTo")].Call((v_r_42 * new V(0.36d)), ((-v_r_42) * new V(0.18d)));
v_ctx_39[new V("stroke")].Call();
v_ctx_39[new V("restore")].Call();
}
v_ctx_39[new V("restore")].Call();
return new V(true);
}
private static V F_getSpheristAnimationState(V[] args) {
V v_p_57 = VisualJs.Arg(args,0);
V v_dt_58 = (new V(Scope[new V("SPHERIST_LAST_TIME")] >= new V(0d)).Truth ? VisualJs.MathObject[new V("max")].Call(new V(0.001d), (Scope[new V("RENDER_TIME")] - Scope[new V("SPHERIST_LAST_TIME")])) : new V(0.016d));
V v_vx_59 = (((v_p_57[new V("pos")][new V("x")] - Scope[new V("SPHERIST_LAST_X")])) / v_dt_58);
V v_vy_60 = (((v_p_57[new V("pos")][new V("y")] - Scope[new V("SPHERIST_LAST_Y")])) / v_dt_58);
VisualJs.Assign(Scope,new V("SPHERIST_VX"),"=",v_vx_59);
VisualJs.Assign(Scope,new V("SPHERIST_VY"),"=",v_vy_60);
VisualJs.Assign(Scope,new V("SPHERIST_LAST_X"),"=",v_p_57[new V("pos")][new V("x")]);
VisualJs.Assign(Scope,new V("SPHERIST_LAST_Y"),"=",v_p_57[new V("pos")][new V("y")]);
VisualJs.Assign(Scope,new V("SPHERIST_LAST_TIME"),"=",Scope[new V("RENDER_TIME")]);
V v_next_61 = default(V);
if (new V(v_p_57[new V("hp")] <= new V(0d)).Truth) v_next_61 = new V("death"); else if (new V(v_p_57[new V("contactDamageCooldown")] > new V(0d)).Truth) v_next_61 = new V("hit"); else if (VisualJs.Or(new V(v_p_57[new V("shieldTimer")] > new V(0d)), () => new V(v_p_57[new V("shieldCharges")] > new V(0d))).Truth) v_next_61 = new V("shield"); else if (VisualJs.Or(new V(v_p_57[new V("dashTimer")] > new V(0d)), () => new V(VisualJs.MathObject[new V("hypot")].Call(v_vx_59, v_vy_60) > new V(0.06d))).Truth) v_next_61 = new V("move"); else v_next_61 = new V("idle");
VisualJs.Assign(Scope,new V("SPHERIST_ANIMATION_STATE"),"=",v_next_61);
return Scope[new V("SPHERIST_ANIMATION_STATE")];
}
private static V F_render(V[] args) {
V v_ctx_62 = VisualJs.Arg(args,0);
V v_s_63 = VisualJs.Arg(args,1);
V v_backingWidth_64 = VisualJs.Arg(args,2);
V v_backingHeight_65 = VisualJs.Arg(args,3);
V v_viewport_66 = VisualBindings.Get(new V("renderScale.ts"),new V("getRenderViewport")).Call(v_ctx_62[new V("canvas")], v_backingWidth_64, v_backingHeight_65);
V v_canvasW_67 = v_viewport_66[new V("cssWidth")];
V v_canvasH_68 = v_viewport_66[new V("cssHeight")];
V v_renderStarted_69 = VisualJs.PerformanceObject[new V("now")].Call();
VisualJs.Assign(Scope,new V("RENDER_TIME"),"=",v_s_63[new V("time")]);
V v_theme_70 = Scope[new V("CANONICAL_THEME")];
v_ctx_62[new V("setTransform")].Call(v_viewport_66[new V("dpr")], new V(0d), new V(0d), v_viewport_66[new V("dpr")], new V(0d), new V(0d));
V v_bg_71 = v_ctx_62[new V("createRadialGradient")].Call((v_canvasW_67 * new V(0.5d)), (v_canvasH_68 * new V(0.46d)), new V(0d), (v_canvasW_67 * new V(0.5d)), (v_canvasH_68 * new V(0.46d)), (VisualJs.MathObject[new V("max")].Call(v_canvasW_67, v_canvasH_68) * new V(0.72d)));
v_bg_71[new V("addColorStop")].Call(new V(0d), v_theme_70[new V("bg")]);
v_bg_71[new V("addColorStop")].Call(new V(0.68d), v_theme_70[new V("bgDark")]);
v_bg_71[new V("addColorStop")].Call(new V(1d), new V("#02050b"));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",v_bg_71);
v_ctx_62[new V("fillRect")].Call(new V(0d), new V(0d), v_canvasW_67, v_canvasH_68);
Scope[new V("drawVoidAmbient")].Call(v_ctx_62, v_canvasW_67, v_canvasH_68, v_s_63[new V("time")]);
V v_renderLoad_72 = ((((v_s_63[new V("enemies")][new V("length")] + (v_s_63[new V("sphereProjectiles")][new V("length")] * new V(0.55d))) + (v_s_63[new V("particles")][new V("length")] * new V(0.28d))) + (v_s_63[new V("damageNumbers")][new V("length")] * new V(0.42d))) + (v_s_63[new V("lightnings")][new V("length")] * new V(0.75d)));
V v_fxStride_73 = (new V(v_renderLoad_72 > new V(220d)).Truth ? new V(3d) : (new V(v_renderLoad_72 > new V(150d)).Truth ? new V(2d) : new V(1d)));
V v_damageStride_74 = (new V(v_s_63[new V("damageNumbers")][new V("length")] > new V(36d)).Truth ? new V(2d) : new V(1d));
V v_lightningStride_75 = (new V(v_s_63[new V("lightnings")][new V("length")] > new V(18d)).Truth ? new V(2d) : new V(1d));
v_ctx_62[new V("save")].Call();
V v_shakeX_76 = new V(0d);
V v_shakeY_77 = new V(0d);
if (new V(v_s_63[new V("screenShake")] > new V(0d)).Truth) {
v_shakeX_76 = ((((VisualJs.MathObject[new V("random")].Call() - new V(0.5d))) * v_s_63[new V("screenShake")]) * new V(20d));
v_shakeY_77 = ((((VisualJs.MathObject[new V("random")].Call() - new V(0.5d))) * v_s_63[new V("screenShake")]) * new V(20d));
}
v_ctx_62[new V("translate")].Call((((v_canvasW_67 / new V(2d)) - v_s_63[new V("camera")][new V("x")]) + v_shakeX_76), (((v_canvasH_68 / new V(2d)) - v_s_63[new V("camera")][new V("y")]) + v_shakeY_77));
Scope[new V("drawVoidField")].Call(v_ctx_62, v_s_63[new V("worldWidth")], v_s_63[new V("worldHeight")], v_theme_70, ((-v_s_63[new V("worldWidth")]) / new V(2d)), ((-v_s_63[new V("worldHeight")]) / new V(2d)), v_s_63[new V("player")][new V("pos")][new V("x")], v_s_63[new V("player")][new V("pos")][new V("y")]);
Scope[new V("drawRegionLayer")].Call(v_ctx_62, v_s_63);
VisualJs.Assign(v_ctx_62,new V("strokeStyle"),"=",v_theme_70[new V("border")]);
VisualJs.Assign(v_ctx_62,new V("shadowColor"),"=",(new V("rgba(") + Scope[new V("hexToRgb")].Call(v_theme_70[new V("accent")]) + new V(",0.22)")));
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(1.5d));
v_ctx_62[new V("setLineDash")].Call(VisualJs.Arr(new V(10d), new V(8d)));
v_ctx_62[new V("strokeRect")].Call(((-v_s_63[new V("worldWidth")]) / new V(2d)), ((-v_s_63[new V("worldHeight")]) / new V(2d)), v_s_63[new V("worldWidth")], v_s_63[new V("worldHeight")]);
v_ctx_62[new V("setLineDash")].Call(VisualJs.Arr());
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
Scope[new V("drawCharacterWorldIndicators")].Call(v_ctx_62, v_s_63);
foreach (V v_ft_78 in v_s_63[new V("fireTrails")].Items) {
V v_alpha_79 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (v_ft_78[new V("life")] / v_ft_78[new V("maxLife")])));
v_ctx_62[new V("save")].Call();
v_ctx_62[new V("translate")].Call(v_ft_78[new V("pos")][new V("x")], v_ft_78[new V("pos")][new V("y")]);
Scope[new V("glowCircle")].Call(v_ctx_62, new V(34d), new V("#ff613d"), (v_alpha_79 * new V(0.24d)));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",(new V("rgba(255,91,56,") + (v_alpha_79 * new V(0.16d)) + new V(")")));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("arc")].Call(new V(0d), new V(0d), new V(22d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_62[new V("fill")].Call();
VisualJs.Assign(v_ctx_62,new V("strokeStyle"),"=",(new V("rgba(255,180,70,") + (v_alpha_79 * new V(0.75d)) + new V(")")));
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(1.5d));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("arc")].Call(new V(0d), new V(0d), (new V(15d) + (VisualJs.MathObject[new V("sin")].Call(((Scope[new V("RENDER_TIME")] * new V(50d)) + v_ft_78[new V("pos")][new V("x")])) * new V(2d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_62[new V("stroke")].Call();
v_ctx_62[new V("restore")].Call();
}
Scope[new V("drawFormationMemory")].Call(v_ctx_62, v_s_63);
Scope[new V("drawSphereNetwork")].Call(v_ctx_62, v_s_63, VisualBindings.Get(new V("engineRuntime.ts"),new V("getNetworkFrame")).Call(v_s_63));

foreach (V v_orb_80 in v_s_63[new V("xpOrbs")].Items) Scope[new V("drawModernXp")].Call(v_ctx_62, v_orb_80[new V("pos")][new V("x")], v_orb_80[new V("pos")][new V("y")], v_orb_80[new V("radius")], new V("#63e6ff"), v_orb_80[new V("rotation")]);
foreach (V v_hp_81 in v_s_63[new V("healthPacks")].Items) Scope[new V("drawModernHealth")].Call(v_ctx_62, v_hp_81[new V("pos")][new V("x")], v_hp_81[new V("pos")][new V("y")], new V("#ff5c72"));
foreach (V v_rune_82 in v_s_63[new V("runes")].Items) if (v_rune_82[new V("alive")].Truth) Scope[new V("drawRune")].Call(v_ctx_62, v_rune_82);
foreach (V v_chest_83 in v_s_63[new V("chests")].Items) if (v_chest_83[new V("alive")].Truth) Scope[new V("drawChest")].Call(v_ctx_62, v_chest_83);
foreach (V v_chest_84 in v_s_63[new V("stellaChests")].Items) if (v_chest_84[new V("alive")].Truth) Scope[new V("drawStellaChest")].Call(v_ctx_62, v_chest_84);
foreach (V v_sphere_85 in v_s_63[new V("spheres")].Items) Scope[new V("drawModernSphere")].Call(v_ctx_62, v_s_63, v_sphere_85, v_s_63[new V("time")]);
foreach (V v_p_86 in v_s_63[new V("sphereProjectiles")].Items) Visual_mutationVisual.Scope[new V("renderSphereProjectileVfx")].Call(v_ctx_62, v_p_86, v_s_63[new V("player")], v_s_63[new V("time")]);
foreach (V v_m_87 in v_s_63[new V("minions")].Items) Scope[new V("drawModernMinion")].Call(v_ctx_62, v_m_87[new V("pos")][new V("x")], v_m_87[new V("pos")][new V("y")], v_m_87[new V("radius")], v_m_87[new V("rotation")], VisualJs.Or(VisualJs.Chain(Visual_gameData.Scope[new V("SPHERE_TYPES")][v_m_87[new V("anchorType")]],optional_88=>optional_88[new V("color")]), () => new V("#ffb84d")));
foreach (V v_e_89 in v_s_63[new V("enemies")].Items) Scope[new V("drawModernEnemy")].Call(v_ctx_62, v_e_89, v_s_63[new V("player")][new V("pos")]);
foreach (V v_sphere_90 in v_s_63[new V("spheres")].Items) {
if (VisualJs.And(v_sphere_90[new V("alive")], () => new V(v_sphere_90[new V("type")] == new V("orbital"))).Truth) {
V v_orbitScale_91 = (VisualJs.MathObject[new V("max")].Call(new V(15d), VisualJs.MathObject[new V("min")].Call(new V(25d), ((v_sphere_90[new V("radius")] * new V(0.19d)) + ((VisualJs.Or(v_sphere_90[new V("visualTier")], () => new V(0d))) * new V(0.8d))))) / new V(24d));
V v_network_92 = VisualBindings.Get(new V("engineRuntime.ts"),new V("getNetworkFrame")).Call(v_s_63);
V v_sphereIndex_93 = v_s_63[new V("spheres")][new V("indexOf")].Call(v_sphere_90);
V v_networkProfile_94 = Visual_network.Scope[new V("getSphereNetworkProfile")].Call(v_network_92, v_sphereIndex_93);
V v_clusterBonus_95 = Visual_network.Scope[new V("getFormationBonusMultiplier")].Call(v_network_92, new V("cluster"), v_sphereIndex_93);
V v_ringBonus_96 = Visual_network.Scope[new V("getFormationBonusMultiplier")].Call(v_network_92, new V("ring"), v_sphereIndex_93);
V v_networkRadiusMultiplier_97 = (((new V(1d) + ((new V(v_clusterBonus_95 > new V(0d)).Truth ? (new V(0.08d) * v_clusterBonus_95) : new V(0d)))) + ((new V(v_ringBonus_96 > new V(0d)).Truth ? (new V(0.12d) * v_ringBonus_96) : new V(0d)))) + VisualJs.MathObject[new V("min")].Call(new V(0.2d), (v_networkProfile_94[new V("linkedNeighbours")] * new V(0.03d))));
V v_orbitalRadiusScale_98 = (((Visual_engineSpheres.Scope[new V("getSphereRadius")].Call(v_s_63, v_sphere_90) / VisualJs.MathObject[new V("max")].Call(new V(1d), v_sphere_90[new V("radius")]))) * v_networkRadiusMultiplier_97);
Visual_orbitalVisual.Scope[new V("renderOrbitalSphereAttackersVfx")].Call(v_ctx_62, v_sphere_90, v_s_63[new V("player")], v_s_63[new V("time")], v_orbitScale_91, v_s_63[new V("enemies")], v_orbitalRadiusScale_98);
}
}
Scope[new V("drawPlayerShield")].Call(v_ctx_62, v_s_63);
Scope[new V("drawPlayer")].Call(v_ctx_62, v_s_63[new V("player")]);
Scope[new V("drawCharacterTargetIndicators")].Call(v_ctx_62, v_s_63);
foreach (V v_e_99 in v_s_63[new V("enemies")].Items) foreach (V v_bp_100 in v_e_99[new V("bossProjectiles")].Items) {
if (new V(v_bp_100[new V("visualType")] == new V("enemy_ranged")).Truth) Scope[new V("drawEnemyProjectile")].Call(v_ctx_62, v_bp_100[new V("pos")][new V("x")], v_bp_100[new V("pos")][new V("y")], v_bp_100[new V("radius")], v_bp_100[new V("vel")], new V(false)); else if (new V(v_bp_100[new V("visualType")] == new V("enemy_sniper")).Truth) Scope[new V("drawEnemyProjectile")].Call(v_ctx_62, v_bp_100[new V("pos")][new V("x")], v_bp_100[new V("pos")][new V("y")], v_bp_100[new V("radius")], v_bp_100[new V("vel")], new V(true)); else Scope[new V("drawBossProjectile")].Call(v_ctx_62, v_bp_100[new V("pos")][new V("x")], v_bp_100[new V("pos")][new V("y")], v_bp_100[new V("radius")], v_e_99[new V("color")], v_e_99[new V("bossType")], v_bp_100[new V("vel")]);
}
for (V v_particleIndex_101 = new V(0d); new V(v_particleIndex_101 < v_s_63[new V("particles")][new V("length")]).Truth; v_particleIndex_101 += v_fxStride_73) {
V v_p_102 = v_s_63[new V("particles")][v_particleIndex_101];
V v_alpha_103 = (v_p_102[new V("life")] / v_p_102[new V("maxLife")]);
V v_rgb_104 = Scope[new V("hexToRgb")].Call(v_p_102[new V("color")]);
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_104 + new V(",") + (v_alpha_103 * new V(0.82d)) + new V(")")));
VisualJs.Assign(v_ctx_62,new V("shadowColor"),"=",v_p_102[new V("color")]);
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
v_ctx_62[new V("save")].Call();
v_ctx_62[new V("translate")].Call(v_p_102[new V("pos")][new V("x")], v_p_102[new V("pos")][new V("y")]);
V v_s2_105 = (v_p_102[new V("size")] * ((new V(0.55d) + (v_alpha_103 * new V(0.65d)))));
v_ctx_62[new V("rotate")].Call(VisualJs.MathObject[new V("atan2")].Call(VisualJs.Or(VisualJs.Chain(v_p_102[new V("vel")],optional_106=>optional_106[new V("y")]), () => new V(0d)), VisualJs.Or(VisualJs.Chain(v_p_102[new V("vel")],optional_107=>optional_107[new V("x")]), () => new V(1d))));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("moveTo")].Call((v_s2_105 * new V(1.8d)), new V(0d));
v_ctx_62[new V("lineTo")].Call(new V(0d), ((-v_s2_105) * new V(0.55d)));
v_ctx_62[new V("lineTo")].Call(((-v_s2_105) * new V(1.2d)), new V(0d));
v_ctx_62[new V("lineTo")].Call(new V(0d), (v_s2_105 * new V(0.55d)));
v_ctx_62[new V("closePath")].Call();
v_ctx_62[new V("fill")].Call();
v_ctx_62[new V("restore")].Call();
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
}
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",new V(1d));
for (V v_damageIndex_108 = new V(0d); new V(v_damageIndex_108 < v_s_63[new V("damageNumbers")][new V("length")]).Truth; v_damageIndex_108 += v_damageStride_74) {
V v_dn_109 = v_s_63[new V("damageNumbers")][v_damageIndex_108];
if (new V(v_dn_109[new V("sourceSphereType")] == new V("orbital")).Truth) continue;
V v_alpha_110 = VisualJs.MathObject[new V("min")].Call(new V(1d), ((v_dn_109[new V("life")] / v_dn_109[new V("maxLife")]) * new V(1.5d)));
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",v_alpha_110);
V v_damageColor_111 = (v_dn_109[new V("crit")].Truth ? new V("#ffd166") : (new V(v_dn_109[new V("element")] == new V("fire")).Truth ? new V("#ff5a36") : (new V(v_dn_109[new V("element")] == new V("freeze")).Truth ? new V("#69d6ff") : (new V(v_dn_109[new V("element")] == new V("poison")).Truth ? new V("#72f08e") : new V("#e8f6ff")))));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",v_damageColor_111);
VisualJs.Assign(v_ctx_62,new V("shadowColor"),"=",(v_dn_109[new V("crit")].Truth ? new V("#b8860b") : v_damageColor_111));
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_62,new V("font"),"=",(new V("bold ") + (v_dn_109[new V("crit")].Truth ? new V(20d) : new V(14d)) + new V("px system-ui, sans-serif")));
VisualJs.Assign(v_ctx_62,new V("textAlign"),"=",new V("center"));
v_ctx_62[new V("fillText")].Call(new V(v_dn_109[new V("value")].Text), v_dn_109[new V("pos")][new V("x")], v_dn_109[new V("pos")][new V("y")]);
}
for (V v_damageIndex_112 = new V(0d); new V(v_damageIndex_112 < v_s_63[new V("damageNumbers")][new V("length")]).Truth; v_damageIndex_112 += new V(1d)) {
V v_dn_113 = v_s_63[new V("damageNumbers")][v_damageIndex_112];
if (new V(v_dn_113[new V("sourceSphereType")] != new V("orbital")).Truth) continue;
V v_alpha_114 = VisualJs.MathObject[new V("min")].Call(new V(1d), ((v_dn_113[new V("life")] / v_dn_113[new V("maxLife")]) * new V(1.5d)));
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",v_alpha_114);
V v_damageColor_115 = (v_dn_113[new V("crit")].Truth ? new V("#ffd166") : (new V(v_dn_113[new V("element")] == new V("fire")).Truth ? new V("#ff5a36") : (new V(v_dn_113[new V("element")] == new V("freeze")).Truth ? new V("#69d6ff") : (new V(v_dn_113[new V("element")] == new V("poison")).Truth ? new V("#72f08e") : new V("#e8f6ff")))));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",v_damageColor_115);
VisualJs.Assign(v_ctx_62,new V("shadowColor"),"=",(v_dn_113[new V("crit")].Truth ? new V("#b8860b") : v_damageColor_115));
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_62,new V("font"),"=",(new V("bold ") + (v_dn_113[new V("crit")].Truth ? new V(20d) : new V(14d)) + new V("px system-ui, sans-serif")));
VisualJs.Assign(v_ctx_62,new V("textAlign"),"=",new V("center"));
v_ctx_62[new V("fillText")].Call(new V(v_dn_113[new V("value")].Text), v_dn_113[new V("pos")][new V("x")], v_dn_113[new V("pos")][new V("y")]);
}
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",new V(1d));
for (V v_lightningIndex_116 = new V(0d); new V(v_lightningIndex_116 < v_s_63[new V("lightnings")][new V("length")]).Truth; v_lightningIndex_116 += v_lightningStride_75) {
V v_l_117 = v_s_63[new V("lightnings")][v_lightningIndex_116];
V v_a_118 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (v_l_117[new V("life")] / new V(0.3d))));
if ((!Visual_mutationVisual.Scope[new V("renderChainLightningVfx")].Call(v_ctx_62, v_l_117, v_s_63[new V("player")], v_a_118, v_s_63[new V("time")])).Truth) Scope[new V("drawEnergyBolt")].Call(v_ctx_62, v_l_117[new V("from")], v_l_117[new V("to")], v_a_118);
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",new V(1d));
}
v_ctx_62[new V("restore")].Call();
if (v_s_63[new V("bossArrow")].Truth) {
V v_ax_119 = ((v_canvasW_67 / new V(2d)) + (v_s_63[new V("bossArrow")][new V("x")] * (((v_canvasW_67 / new V(2d)) - new V(40d)))));
V v_ay_120 = ((v_canvasH_68 / new V(2d)) + (v_s_63[new V("bossArrow")][new V("y")] * (((v_canvasH_68 / new V(2d)) - new V(40d)))));
v_ctx_62[new V("save")].Call();
v_ctx_62[new V("translate")].Call(v_ax_119, v_ay_120);
v_ctx_62[new V("rotate")].Call(VisualJs.MathObject[new V("atan2")].Call(v_s_63[new V("bossArrow")][new V("y")], v_s_63[new V("bossArrow")][new V("x")]));
VisualJs.Assign(v_ctx_62,new V("shadowColor"),"=",new V("#ff4d70"));
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",new V("#ff5c79"));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("moveTo")].Call(new V(16d), new V(0d));
v_ctx_62[new V("lineTo")].Call((-new V(6d)), (-new V(9d)));
v_ctx_62[new V("lineTo")].Call((-new V(2d)), new V(0d));
v_ctx_62[new V("lineTo")].Call((-new V(6d)), new V(9d));
v_ctx_62[new V("closePath")].Call();
v_ctx_62[new V("fill")].Call();
VisualJs.Assign(v_ctx_62,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_62,new V("strokeStyle"),"=",new V("#ffdbe4"));
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(1d));
v_ctx_62[new V("stroke")].Call();
v_ctx_62[new V("restore")].Call();
}
Scope[new V("drawCharacterHud")].Call(v_ctx_62, v_s_63, v_canvasW_67, v_canvasH_68);
if (new V(VisualJs.Chain(v_s_63[new V("flashText")],optional_121=>optional_121[new V("kind")]) == new V("boundary")).Truth) {
V v_progress_122 = (new V(1d) - VisualJs.MathObject[new V("min")].Call(new V(1d), (v_s_63[new V("flashText")][new V("life")] / new V(1.35d))));
V v_fade_123 = (new V(1d) - v_progress_122);
v_ctx_62[new V("save")].Call();
V v_boundaryX_124 = (((v_canvasW_67 / new V(2d)) + ((v_s_63[new V("player")][new V("pos")][new V("x")] - v_s_63[new V("camera")][new V("x")]))) + v_shakeX_76);
V v_boundaryY_125 = (((v_canvasH_68 / new V(2d)) + ((v_s_63[new V("player")][new V("pos")][new V("y")] - v_s_63[new V("camera")][new V("y")]))) + v_shakeY_77);
v_ctx_62[new V("translate")].Call(v_boundaryX_124, v_boundaryY_125);
VisualJs.Assign(v_ctx_62,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",(new V(0.46d) * v_fade_123));
VisualJs.Assign(v_ctx_62,new V("strokeStyle"),"=",v_s_63[new V("flashText")][new V("color")]);
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(2.2d));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("arc")].Call(new V(0d), new V(0d), ((Visual_engineState.Scope[new V("PLAYER_RADIUS")] + new V(8d)) + (v_progress_122 * new V(58d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_62[new V("stroke")].Call();
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",(new V(0.2d) * v_fade_123));
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(1d));
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("arc")].Call(new V(0d), new V(0d), ((Visual_engineState.Scope[new V("PLAYER_RADIUS")] + new V(20d)) + (v_progress_122 * new V(96d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_62[new V("stroke")].Call();
for (V v_i_126 = new V(0d); new V(v_i_126 < new V(4d)).Truth; v_i_126++) {
V v_a_127 = (((v_i_126 * VisualJs.MathObject[new V("PI")]) / new V(2d)) + (v_progress_122 * new V(1.7d)));
V v_rr_128 = ((Visual_engineState.Scope[new V("PLAYER_RADIUS")] + new V(14d)) + (v_progress_122 * new V(62d)));
V v_size_129 = (new V(5d) + (v_progress_122 * new V(4d)));
V v_x_130 = (VisualJs.MathObject[new V("cos")].Call(v_a_127) * v_rr_128);
V v_y_131 = (VisualJs.MathObject[new V("sin")].Call(v_a_127) * v_rr_128);
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",(new V(0.5d) * v_fade_123));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",v_s_63[new V("flashText")][new V("color")]);
v_ctx_62[new V("beginPath")].Call();
v_ctx_62[new V("moveTo")].Call((v_x_130 + (VisualJs.MathObject[new V("cos")].Call(v_a_127) * v_size_129)), (v_y_131 + (VisualJs.MathObject[new V("sin")].Call(v_a_127) * v_size_129)));
v_ctx_62[new V("lineTo")].Call((v_x_130 - ((VisualJs.MathObject[new V("sin")].Call(v_a_127) * v_size_129) * new V(0.6d))), (v_y_131 + ((VisualJs.MathObject[new V("cos")].Call(v_a_127) * v_size_129) * new V(0.6d))));
v_ctx_62[new V("lineTo")].Call((v_x_130 + ((VisualJs.MathObject[new V("sin")].Call(v_a_127) * v_size_129) * new V(0.6d))), (v_y_131 - ((VisualJs.MathObject[new V("cos")].Call(v_a_127) * v_size_129) * new V(0.6d))));
v_ctx_62[new V("closePath")].Call();
v_ctx_62[new V("fill")].Call();
}
v_ctx_62[new V("restore")].Call();
}
if (v_s_63[new V("flashText")].Truth) {
v_ctx_62[new V("save")].Call();
V v_maxTextWidth_132 = VisualJs.MathObject[new V("min")].Call((v_canvasW_67 - new V(44d)), new V(224d));
V v_fontSize_133 = VisualJs.MathObject[new V("max")].Call(new V(10d), VisualJs.MathObject[new V("min")].Call(new V(12d), (v_canvasW_67 * new V(0.032d))));
VisualJs.Assign(v_ctx_62,new V("font"),"=",(new V("bold ") + v_fontSize_133 + new V("px system-ui, sans-serif")));
VisualJs.Assign(v_ctx_62,new V("textAlign"),"=",new V("center"));
V v_words_134 = v_s_63[new V("flashText")][new V("text")][new V("split")].Call(VisualJs.Regex("/\\s+/"));
V v_lines_135 = VisualJs.Arr();
V v_current_136 = new V("");
foreach (V v_word_137 in v_words_134.Items) {
V v_next_138 = (v_current_136.Truth ? ((v_current_136 + new V(" ")) + v_word_137) : v_word_137);
if (VisualJs.Or(new V(v_ctx_62[new V("measureText")].Call(v_next_138)[new V("width")] <= v_maxTextWidth_132), () => (!v_current_136)).Truth) v_current_136 = v_next_138; else {
v_lines_135[new V("push")].Call(v_current_136);
v_current_136 = v_word_137;
}
}
if (v_current_136.Truth) v_lines_135[new V("push")].Call(v_current_136);
V v_displayLines_139 = (new V(v_lines_135[new V("length")] <= new V(2d)).Truth ? v_lines_135 : VisualJs.Arr(v_lines_135[new V(0d)], (v_lines_135[new V("slice")].Call(new V(1d))[new V("join")].Call(new V(" "))[new V("slice")].Call(new V(0d), new V(34d)) + new V("…"))));
V v_lineGap_140 = (v_fontSize_133 + new V(2d));
V v_boxHeight_141 = ((v_displayLines_139[new V("length")] * v_lineGap_140) + new V(9d));
V v_widest_142 = VisualJs.MathObject[new V("max")].Call(VisualJs.Concat(v_displayLines_139[new V("map")].Call(VisualJs.Fn(args_143 => { V v_line_144 = VisualJs.Arg(args_143, 0);
return v_ctx_62[new V("measureText")].Call(v_line_144)[new V("width")]; })), VisualJs.Arr(new V(104d))).Items.ToArray());
V v_boxWidth_145 = VisualJs.MathObject[new V("min")].Call((v_canvasW_67 - new V(22d)), VisualJs.MathObject[new V("max")].Call(new V(122d), (v_widest_142 + new V(16d))));
V v_bannerY_146 = VisualJs.MathObject[new V("max")].Call(new V(142d), VisualJs.MathObject[new V("min")].Call((v_canvasH_68 * new V(0.205d)), new V(178d)));
V v_top_147 = ((v_bannerY_146 - v_boxHeight_141) + new V(3d));
VisualJs.Assign(v_ctx_62,new V("globalAlpha"),"=",VisualJs.MathObject[new V("min")].Call(new V(1d), VisualJs.MathObject[new V("max")].Call(new V(0d), (v_s_63[new V("flashText")][new V("life")] / new V(0.82d)))));
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",new V("rgba(2,7,13,.92)"));
v_ctx_62[new V("fillRect")].Call(((v_canvasW_67 / new V(2d)) - (v_boxWidth_145 / new V(2d))), v_top_147, v_boxWidth_145, v_boxHeight_141);
VisualJs.Assign(v_ctx_62,new V("strokeStyle"),"=",v_s_63[new V("flashText")][new V("color")]);
VisualJs.Assign(v_ctx_62,new V("lineWidth"),"=",new V(1d));
v_ctx_62[new V("strokeRect")].Call(((v_canvasW_67 / new V(2d)) - (v_boxWidth_145 / new V(2d))), v_top_147, v_boxWidth_145, v_boxHeight_141);
VisualJs.Assign(v_ctx_62,new V("fillStyle"),"=",v_s_63[new V("flashText")][new V("color")]);
v_displayLines_139[new V("forEach")].Call(VisualJs.Fn(args_148 => { V v_line_149 = VisualJs.Arg(args_148, 0);
V v_index_150 = VisualJs.Arg(args_148, 1);
return v_ctx_62[new V("fillText")].Call(v_line_149, (v_canvasW_67 / new V(2d)), ((v_top_147 + new V(12d)) + (v_index_150 * v_lineGap_140))); }));
v_ctx_62[new V("restore")].Call();
}
VisualJs.Assign(Scope,new V("RENDER_COST_MS"),"=",(VisualJs.PerformanceObject[new V("now")].Call() - v_renderStarted_69));
if (VisualJs.And(new V(Scope[new V("RENDER_COST_MS")] > new V(10d)), () => new V((Scope[new V("RENDER_TIME")] - Scope[new V("RENDER_LAST_LOG")]) >= new V(3d))).Truth) {
VisualJs.Assign(Scope,new V("RENDER_LAST_LOG"),"=",Scope[new V("RENDER_TIME")]);
VisualJs.ConsoleObject[new V("info")].Call(new V("[ECHO_RENDER_PERF]"), VisualJs.Obj(new V("ms"), VisualJs.Number(Scope[new V("RENDER_COST_MS")][new V("toFixed")].Call(new V(2d))), new V("enemies"), v_s_63[new V("enemies")][new V("length")], new V("spheres"), v_s_63[new V("spheres")][new V("length")], new V("projectiles"), v_s_63[new V("sphereProjectiles")][new V("length")], new V("particles"), v_s_63[new V("particles")][new V("length")], new V("damageNumbers"), v_s_63[new V("damageNumbers")][new V("length")], new V("lightnings"), v_s_63[new V("lightnings")][new V("length")]));
}
return default;
}
private static V F_drawRegionLayer(V[] args) {
V v_ctx_151 = VisualJs.Arg(args,0);
V v_s_152 = VisualJs.Arg(args,1);
if ((!v_s_152[new V("region")]).Truth) return default;
v_ctx_151[new V("save")].Call();
V v_seamTime_153 = (v_s_152[new V("time")] * new V(0.65d));
foreach (V v_p_154 in Visual_region.Scope[new V("REGION_POCKETS")].Items) {
V v_active_155 = new V(v_p_154[new V("id")] == v_s_152[new V("region")][new V("pocketId")]);
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",(v_active_155.Truth ? new V(0.13d) : new V(0.035d)));
VisualJs.Assign(v_ctx_151,new V("strokeStyle"),"=",v_p_154[new V("accent")]);
VisualJs.Assign(v_ctx_151,new V("lineWidth"),"=",(v_active_155.Truth ? new V(1.8d) : new V(1d)));
v_ctx_151[new V("beginPath")].Call();
v_ctx_151[new V("arc")].Call(v_p_154[new V("center")][new V("x")], v_p_154[new V("center")][new V("y")], v_p_154[new V("radius")], new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_151[new V("stroke")].Call();
for (V v_i_156 = new V(0d); new V(v_i_156 < new V(6d)).Truth; v_i_156++) {
V v_a_157 = ((((v_i_156 * VisualJs.MathObject[new V("PI")]) / new V(3d)) + (v_seamTime_153 * ((new V((v_i_156 % new V(2d)) == new V(0d)).Truth ? new V(0.08d) : (-new V(0.06d)))))) + (v_p_154[new V("center")][new V("x")] * new V(0.0001d)));
V v_span_158 = (new V(0.1d) + ((v_active_155.Truth ? new V(0.045d) : new V(0.02d))));
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",(v_active_155.Truth ? new V(0.16d) : new V(0.045d)));
v_ctx_151[new V("beginPath")].Call();
v_ctx_151[new V("arc")].Call(v_p_154[new V("center")][new V("x")], v_p_154[new V("center")][new V("y")], ((v_p_154[new V("radius")] - new V(4d)) + (((v_i_156 % new V(2d))) * new V(2d))), v_a_157, (v_a_157 + v_span_158));
v_ctx_151[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",(v_active_155.Truth ? new V(0.24d) : new V(0.055d)));
for (V v_i_159 = new V(0d); new V(v_i_159 < new V(8d)).Truth; v_i_159++) {
V v_a_160 = (((v_i_159 * VisualJs.MathObject[new V("PI")]) / new V(4d)) + (v_seamTime_153 * new V(0.08d)));
V v_x_161 = (v_p_154[new V("center")][new V("x")] + (VisualJs.MathObject[new V("cos")].Call(v_a_160) * v_p_154[new V("radius")]));
V v_y_162 = (v_p_154[new V("center")][new V("y")] + (VisualJs.MathObject[new V("sin")].Call(v_a_160) * v_p_154[new V("radius")]));
VisualJs.Assign(v_ctx_151,new V("fillStyle"),"=",v_p_154[new V("accent")]);
v_ctx_151[new V("beginPath")].Call();
v_ctx_151[new V("moveTo")].Call(v_x_161, v_y_162);
v_ctx_151[new V("lineTo")].Call((v_x_161 - (VisualJs.MathObject[new V("cos")].Call((v_a_160 - new V(0.16d))) * new V(7d))), (v_y_162 - (VisualJs.MathObject[new V("sin")].Call((v_a_160 - new V(0.16d))) * new V(7d))));
v_ctx_151[new V("lineTo")].Call((v_x_161 - (VisualJs.MathObject[new V("cos")].Call((v_a_160 + new V(0.16d))) * new V(7d))), (v_y_162 - (VisualJs.MathObject[new V("sin")].Call((v_a_160 + new V(0.16d))) * new V(7d))));
v_ctx_151[new V("closePath")].Call();
v_ctx_151[new V("fill")].Call();
}
}
V v_poiLabels_163 = VisualJs.Obj(new V("resonance_cache"), new V("RESONANCE CACHE"), new V("breach_node"), new V("BREACH NODE"), new V("echo_relay"), new V("ECHO RELAY"), new V("lost_signal"), new V("LOST SIGNAL"), new V("elite_nest"), new V("ELITE NEST"), new V("rupture"), new V("RUPTURE"), new V("boss_trace"), new V("BOSS TRACE"));
foreach (V v_p_164 in v_s_152[new V("region")][new V("pois")].Items) {
if ((!v_p_164[new V("alive")]).Truth) continue;
V v_color_165 = (VisualJs.Or(new V(v_p_164[new V("type")] == new V("elite_nest")), () => new V(v_p_164[new V("type")] == new V("rupture"))).Truth ? new V("#ff6b6b") : (new V(v_p_164[new V("type")] == new V("boss_trace")).Truth ? new V("#ffb84d") : new V("#55e6c1")));
V v_pulse_166 = (new V(38d) + (VisualJs.MathObject[new V("sin")].Call(((v_s_152[new V("time")] * new V(4d)) + v_p_164[new V("id")])) * new V(4d)));
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",new V(0.18d));
VisualJs.Assign(v_ctx_151,new V("fillStyle"),"=",v_color_165);
v_ctx_151[new V("beginPath")].Call();
v_ctx_151[new V("arc")].Call(v_p_164[new V("pos")][new V("x")], v_p_164[new V("pos")][new V("y")], v_pulse_166, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_151[new V("fill")].Call();
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",new V(0.9d));
VisualJs.Assign(v_ctx_151,new V("strokeStyle"),"=",v_color_165);
VisualJs.Assign(v_ctx_151,new V("lineWidth"),"=",new V(2d));
v_ctx_151[new V("beginPath")].Call();
v_ctx_151[new V("moveTo")].Call(v_p_164[new V("pos")][new V("x")], (v_p_164[new V("pos")][new V("y")] - new V(11d)));
v_ctx_151[new V("lineTo")].Call((v_p_164[new V("pos")][new V("x")] + new V(11d)), v_p_164[new V("pos")][new V("y")]);
v_ctx_151[new V("lineTo")].Call(v_p_164[new V("pos")][new V("x")], (v_p_164[new V("pos")][new V("y")] + new V(11d)));
v_ctx_151[new V("lineTo")].Call((v_p_164[new V("pos")][new V("x")] - new V(11d)), v_p_164[new V("pos")][new V("y")]);
v_ctx_151[new V("closePath")].Call();
v_ctx_151[new V("stroke")].Call();
VisualJs.Assign(v_ctx_151,new V("globalAlpha"),"=",new V(0.86d));
VisualJs.Assign(v_ctx_151,new V("fillStyle"),"=",v_color_165);
VisualJs.Assign(v_ctx_151,new V("font"),"=",new V("bold 8px system-ui,sans-serif"));
VisualJs.Assign(v_ctx_151,new V("textAlign"),"=",new V("center"));
v_ctx_151[new V("fillText")].Call(v_poiLabels_163[v_p_164[new V("type")]], v_p_164[new V("pos")][new V("x")], (v_p_164[new V("pos")][new V("y")] - new V(16d)));
}
v_ctx_151[new V("restore")].Call();
return default;
}
private static V F_drawCharacterHud(V[] args) {
V v__ctx_167 = VisualJs.Arg(args,0);
V v__s_168 = VisualJs.Arg(args,1);
V v__canvasW_169 = VisualJs.Arg(args,2);
V v__canvasH_170 = VisualJs.Arg(args,3);

return default;
}
private static V F_drawPlayerShield(V[] args) {
V v_ctx_171 = VisualJs.Arg(args,0);
V v_s_172 = VisualJs.Arg(args,1);
V v_charges_173 = VisualJs.Or(v_s_172[new V("player")][new V("shieldCharges")], () => new V(0d));
V v_timer_174 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.Or(v_s_172[new V("player")][new V("shieldTimer")], () => new V(0d)));
if (VisualJs.Or(new V(v_charges_173 <= new V(0d)), () => new V(v_timer_174 <= new V(0d))).Truth) return default;
V v_pulse_175 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((Scope[new V("RENDER_TIME")] * new V(8d))) * new V(0.035d)));
V v_radius_176 = (Visual_engineState.Scope[new V("PLAYER_RADIUS")] * new V(1.38d));
V v_recast_177 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), ((VisualJs.Or(v_s_172[new V("player")][new V("shieldVisualPulse")], () => new V(0d))) / new V(0.85d))));
V v_recastWave_178 = (new V(1d) + (((new V(1d) - v_recast_177)) * new V(0.55d)));
V v_recastAlpha_179 = (new V(0.2d) + (v_recast_177 * new V(0.65d)));
v_ctx_171[new V("save")].Call();
v_ctx_171[new V("translate")].Call(v_s_172[new V("player")][new V("pos")][new V("x")], v_s_172[new V("player")][new V("pos")][new V("y")]);
v_ctx_171[new V("scale")].Call(v_pulse_175, v_pulse_175);
VisualJs.Assign(v_ctx_171,new V("globalCompositeOperation"),"=",new V("lighter"));
if (new V(Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_172) == new V("spherist")).Truth) {
Scope[new V("drawArtSprite")].Call(v_ctx_171, new V("player-spherist-shield"), new V(0d), new V(0d), (v_radius_176 * new V(2.2d)), new V(0d), new V(0.78d));
}
VisualJs.Assign(v_ctx_171,new V("fillStyle"),"=",new V("rgba(64,214,255,0.08)"));
VisualJs.Assign(v_ctx_171,new V("strokeStyle"),"=",new V("rgba(99,230,255,0.88)"));
VisualJs.Assign(v_ctx_171,new V("lineWidth"),"=",new V(2.2d));
v_ctx_171[new V("beginPath")].Call();
for (V v_i_180 = new V(0d); new V(v_i_180 < new V(8d)).Truth; v_i_180++) {
V v_ang_181 = ((((-VisualJs.MathObject[new V("PI")]) / new V(8d)) + ((v_i_180 * VisualJs.MathObject[new V("PI")]) / new V(4d))) + (VisualJs.MathObject[new V("sin")].Call((Scope[new V("RENDER_TIME")] * new V(1.8d))) * new V(0.03d)));
V v_rr_182 = (v_radius_176 * ((new V(1d) + (VisualJs.MathObject[new V("sin")].Call(((Scope[new V("RENDER_TIME")] * new V(4d)) + v_i_180)) * new V(0.012d)))));
V v_x_183 = (VisualJs.MathObject[new V("cos")].Call(v_ang_181) * v_rr_182);
V v_y_184 = ((VisualJs.MathObject[new V("sin")].Call(v_ang_181) * v_rr_182) * new V(0.88d));
if (new V(v_i_180 == new V(0d)).Truth) v_ctx_171[new V("moveTo")].Call(v_x_183, v_y_184); else v_ctx_171[new V("lineTo")].Call(v_x_183, v_y_184);
}
v_ctx_171[new V("closePath")].Call();
v_ctx_171[new V("fill")].Call();
v_ctx_171[new V("stroke")].Call();
VisualJs.Assign(v_ctx_171,new V("strokeStyle"),"=",new V("rgba(225,251,255,0.72)"));
VisualJs.Assign(v_ctx_171,new V("lineWidth"),"=",new V(1d));
v_ctx_171[new V("beginPath")].Call();
v_ctx_171[new V("arc")].Call(new V(0d), new V(0d), (v_radius_176 * new V(0.86d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_171[new V("stroke")].Call();
if (new V(v_recast_177 > new V(0.02d)).Truth) {
VisualJs.Assign(v_ctx_171,new V("globalAlpha"),"=",v_recastAlpha_179);
VisualJs.Assign(v_ctx_171,new V("strokeStyle"),"=",new V("#dffaff"));
VisualJs.Assign(v_ctx_171,new V("lineWidth"),"=",(new V(2.2d) + (v_recast_177 * new V(1.8d))));
v_ctx_171[new V("beginPath")].Call();
v_ctx_171[new V("arc")].Call(new V(0d), new V(0d), (v_radius_176 * v_recastWave_178), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_171[new V("stroke")].Call();
VisualJs.Assign(v_ctx_171,new V("globalAlpha"),"=",(v_recastAlpha_179 * new V(0.42d)));
VisualJs.Assign(v_ctx_171,new V("lineWidth"),"=",new V(5d));
v_ctx_171[new V("beginPath")].Call();
v_ctx_171[new V("arc")].Call(new V(0d), new V(0d), (v_radius_176 * ((v_recastWave_178 + new V(0.08d)))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_171[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_171,new V("globalAlpha"),"=",new V(1d));
for (V v_i_185 = new V(0d); new V(v_i_185 < v_charges_173).Truth; v_i_185++) {
V v_ang_186 = ((((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + (((v_i_185 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / VisualJs.MathObject[new V("max")].Call(new V(1d), v_charges_173))) + (Scope[new V("RENDER_TIME")] * new V(0.45d)));
VisualJs.Assign(v_ctx_171,new V("fillStyle"),"=",new V("#dffaff"));
v_ctx_171[new V("beginPath")].Call();
v_ctx_171[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_ang_186) * v_radius_176), ((VisualJs.MathObject[new V("sin")].Call(v_ang_186) * v_radius_176) * new V(0.88d)), new V(3.2d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_171[new V("fill")].Call();
}
VisualJs.Assign(v_ctx_171,new V("fillStyle"),"=",new V("#dffaff"));
VisualJs.Assign(v_ctx_171,new V("font"),"=",new V("bold 9px system-ui,sans-serif"));
VisualJs.Assign(v_ctx_171,new V("textAlign"),"=",new V("center"));
v_ctx_171[new V("fillText")].Call(new V(v_charges_173.Text), new V(0d), new V(3d));
v_ctx_171[new V("restore")].Call();
return default;
}
private static V F_drawStellaChest(V[] args) {
V v_ctx_187 = VisualJs.Arg(args,0);
V v_chest_188 = VisualJs.Arg(args,1);
V v_t_189 = Scope[new V("RENDER_TIME")];
V v_p_190 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_189 * new V(2.6d))) * new V(0.08d)));
V v_rot_191 = (v_t_189 * new V(0.25d));
v_ctx_187[new V("save")].Call();
v_ctx_187[new V("translate")].Call(v_chest_188[new V("pos")][new V("x")], v_chest_188[new V("pos")][new V("y")]);
v_ctx_187[new V("scale")].Call(v_p_190, v_p_190);
VisualJs.Assign(v_ctx_187,new V("globalCompositeOperation"),"=",new V("source-over"));
Scope[new V("drawGroundShadow")].Call(v_ctx_187, new V(24d), new V(7d), new V(7d));
VisualJs.Assign(v_ctx_187,new V("shadowColor"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_187,new V("shadowBlur"),"=",new V(13d));
VisualJs.Assign(v_ctx_187,new V("fillStyle"),"=",new V("rgba(24,12,2,.96)"));
VisualJs.Assign(v_ctx_187,new V("strokeStyle"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_187,new V("lineWidth"),"=",new V(2d));
v_ctx_187[new V("beginPath")].Call();
v_ctx_187[new V("moveTo")].Call(new V(0d), (-new V(23d)));
v_ctx_187[new V("lineTo")].Call(new V(18d), (-new V(9d)));
v_ctx_187[new V("lineTo")].Call(new V(20d), new V(10d));
v_ctx_187[new V("lineTo")].Call(new V(0d), new V(23d));
v_ctx_187[new V("lineTo")].Call((-new V(20d)), new V(10d));
v_ctx_187[new V("lineTo")].Call((-new V(18d)), (-new V(9d)));
v_ctx_187[new V("closePath")].Call();
v_ctx_187[new V("fill")].Call();
v_ctx_187[new V("stroke")].Call();
v_ctx_187[new V("save")].Call();
v_ctx_187[new V("rotate")].Call(v_rot_191);
VisualJs.Assign(v_ctx_187,new V("strokeStyle"),"=",new V("#fff0bf"));
VisualJs.Assign(v_ctx_187,new V("lineWidth"),"=",new V(1.4d));
for (V v_i_192 = new V(0d); new V(v_i_192 < new V(4d)).Truth; v_i_192++) {
v_ctx_187[new V("beginPath")].Call();
v_ctx_187[new V("moveTo")].Call(((-new V(17d)) + (v_i_192 * new V(4d))), (-new V(8d)));
v_ctx_187[new V("lineTo")].Call((new V(17d) - (v_i_192 * new V(4d))), new V(8d));
v_ctx_187[new V("stroke")].Call();
}
v_ctx_187[new V("restore")].Call();
VisualJs.Assign(v_ctx_187,new V("fillStyle"),"=",new V("#fff8df"));
v_ctx_187[new V("beginPath")].Call();
v_ctx_187[new V("arc")].Call(new V(0d), new V(0d), new V(5d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_187[new V("fill")].Call();
VisualJs.Assign(v_ctx_187,new V("font"),"=",new V("bold 8px system-ui,sans-serif"));
VisualJs.Assign(v_ctx_187,new V("textAlign"),"=",new V("center"));
VisualJs.Assign(v_ctx_187,new V("fillStyle"),"=",new V("#ffd27e"));
v_ctx_187[new V("fillText")].Call(new V("STELLA"), new V(0d), new V(37d));
v_ctx_187[new V("restore")].Call();
return default;
}
private static V F_drawCharacterWorldIndicators(V[] args) {
V v_ctx_193 = VisualJs.Arg(args,0);
V v_s_194 = VisualJs.Arg(args,1);
V v_characterId_195 = Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_194);
if (new V(v_characterId_195 == new V("engineer")).Truth) Scope[new V("drawEngineerLinks")].Call(v_ctx_193, v_s_194);
if (new V(v_characterId_195 == new V("architect")).Truth) Scope[new V("drawArchitectFormation")].Call(v_ctx_193, v_s_194);
if (new V(v_characterId_195 == new V("berserker")).Truth) Scope[new V("drawBerserkerRange")].Call(v_ctx_193, v_s_194);
return default;
}
private static V F_drawCharacterTargetIndicators(V[] args) {
V v_ctx_196 = VisualJs.Arg(args,0);
V v_s_197 = VisualJs.Arg(args,1);
V v_characterId_198 = Visual_characterRuntime.Scope[new V("getCharacterId")].Call(v_s_197);
if (new V(v_characterId_198 == new V("hunter")).Truth) Scope[new V("drawHunterTarget")].Call(v_ctx_196, v_s_197);
if (new V(v_characterId_198 == new V("alchemist")).Truth) Scope[new V("drawAlchemistReactions")].Call(v_ctx_196, v_s_197);
return default;
}
private static V F_drawEngineerLinks(V[] args) {
V v_ctx_199 = VisualJs.Arg(args,0);
V v_s_200 = VisualJs.Arg(args,1);
V v_range_201 = Visual_characterRuntime.Scope[new V("getEngineerNetworkRange")].Call(v_s_200);
v_ctx_199[new V("save")].Call();
VisualJs.Assign(v_ctx_199,new V("strokeStyle"),"=",new V("rgba(74,122,138,0.42)"));
VisualJs.Assign(v_ctx_199,new V("lineWidth"),"=",new V(1.5d));
v_ctx_199[new V("setLineDash")].Call(VisualJs.Arr(new V(5d), new V(4d)));
for (V v_i_202 = new V(0d); new V(v_i_202 < v_s_200[new V("spheres")][new V("length")]).Truth; v_i_202++) {
V v_a_203 = v_s_200[new V("spheres")][v_i_202];
if ((!v_a_203[new V("alive")]).Truth) continue;
for (V v_j_204 = (v_i_202 + new V(1d)); new V(v_j_204 < v_s_200[new V("spheres")][new V("length")]).Truth; v_j_204++) {
V v_b_205 = v_s_200[new V("spheres")][v_j_204];
if (VisualJs.Or((!v_b_205[new V("alive")]), () => new V(Scope[new V("dist2D")].Call(v_a_203[new V("pos")], v_b_205[new V("pos")]) > v_range_201)).Truth) continue;
v_ctx_199[new V("beginPath")].Call();
v_ctx_199[new V("moveTo")].Call(v_a_203[new V("pos")][new V("x")], v_a_203[new V("pos")][new V("y")]);
v_ctx_199[new V("lineTo")].Call(v_b_205[new V("pos")][new V("x")], v_b_205[new V("pos")][new V("y")]);
v_ctx_199[new V("stroke")].Call();
}
}
v_ctx_199[new V("setLineDash")].Call(VisualJs.Arr());
v_ctx_199[new V("restore")].Call();
return default;
}
private static V F_drawArchitectFormation(V[] args) {
V v_ctx_206 = VisualJs.Arg(args,0);
V v_s_207 = VisualJs.Arg(args,1);
V v_result_208 = VisualBindings.Get(new V("characterRuntime.ts"),new V("getCharacterFormation")).Call(v_s_207);
if (new V(v_result_208[new V("type")] == new V("none")).Truth) return default;
V v_spheres_209 = v_s_207[new V("spheres")][new V("filter")].Call(VisualJs.Fn(args_210 => { V v_sphere_211 = VisualJs.Arg(args_210, 0);
return v_sphere_211[new V("alive")]; }))[new V("slice")].Call(new V(0d), new V(8d));
v_ctx_206[new V("save")].Call();
VisualJs.Assign(v_ctx_206,new V("strokeStyle"),"=",new V("rgba(212,148,61,0.55)"));
VisualJs.Assign(v_ctx_206,new V("fillStyle"),"=",new V("rgba(212,148,61,0.08)"));
VisualJs.Assign(v_ctx_206,new V("lineWidth"),"=",new V(2d));
v_ctx_206[new V("setLineDash")].Call(VisualJs.Arr(new V(8d), new V(5d)));
if (new V(v_result_208[new V("type")] == new V("line")).Truth) {
V v_ordered_212 = VisualJs.Concat(v_spheres_209)[new V("sort")].Call(VisualJs.Fn(args_213 => { V v_a_214 = VisualJs.Arg(args_213, 0);
V v_b_215 = VisualJs.Arg(args_213, 1);
return (new V(v_a_214[new V("pos")][new V("x")] == v_b_215[new V("pos")][new V("x")]).Truth ? (v_a_214[new V("pos")][new V("y")] - v_b_215[new V("pos")][new V("y")]) : (v_a_214[new V("pos")][new V("x")] - v_b_215[new V("pos")][new V("x")])); }));
Scope[new V("drawPolyline")].Call(v_ctx_206, v_ordered_212);
} else if (new V(v_result_208[new V("type")] == new V("triangle")).Truth) {
Scope[new V("drawPolygon")].Call(v_ctx_206, v_spheres_209[new V("slice")].Call(new V(0d), new V(3d)));
} else if (new V(v_result_208[new V("type")] == new V("square")).Truth) {
V v_center_216 = v_spheres_209[new V("reduce")].Call(VisualJs.Fn(args_217 => { V v_acc_218 = VisualJs.Arg(args_217, 0);
V v_sphere_219 = VisualJs.Arg(args_217, 1);
return (VisualJs.Obj(new V("x"), (v_acc_218[new V("x")] + v_sphere_219[new V("pos")][new V("x")]), new V("y"), (v_acc_218[new V("y")] + v_sphere_219[new V("pos")][new V("y")]))); }), VisualJs.Obj(new V("x"), new V(0d), new V("y"), new V(0d)));
VisualJs.Assign(v_center_216,new V("x"),"/=",v_spheres_209[new V("length")]);
VisualJs.Assign(v_center_216,new V("y"),"/=",v_spheres_209[new V("length")]);
V v_ordered_220 = VisualJs.Concat(v_spheres_209[new V("slice")].Call(new V(0d), new V(4d)))[new V("sort")].Call(VisualJs.Fn(args_221 => { V v_a_222 = VisualJs.Arg(args_221, 0);
V v_b_223 = VisualJs.Arg(args_221, 1);
return (VisualJs.MathObject[new V("atan2")].Call((v_a_222[new V("pos")][new V("y")] - v_center_216[new V("y")]), (v_a_222[new V("pos")][new V("x")] - v_center_216[new V("x")])) - VisualJs.MathObject[new V("atan2")].Call((v_b_223[new V("pos")][new V("y")] - v_center_216[new V("y")]), (v_b_223[new V("pos")][new V("x")] - v_center_216[new V("x")]))); }));
Scope[new V("drawPolygon")].Call(v_ctx_206, v_ordered_220);
} else if (new V(v_result_208[new V("type")] == new V("cluster")).Truth) {
for (V v_i_224 = new V(0d); new V(v_i_224 < v_spheres_209[new V("length")]).Truth; v_i_224++) {
V v_a_225 = v_spheres_209[v_i_224];
if (new V(Scope[new V("dist2D")].Call(v_a_225[new V("pos")], v_s_207[new V("player")][new V("pos")]) > new V(220d)).Truth) continue;
v_ctx_206[new V("beginPath")].Call();
v_ctx_206[new V("arc")].Call(v_a_225[new V("pos")][new V("x")], v_a_225[new V("pos")][new V("y")], new V(20d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_206[new V("stroke")].Call();
}
}
v_ctx_206[new V("setLineDash")].Call(VisualJs.Arr());
v_ctx_206[new V("restore")].Call();
return default;
}
private static V F_drawHunterTarget(V[] args) {
V v_ctx_226 = VisualJs.Arg(args,0);
V v_s_227 = VisualJs.Arg(args,1);
V v_target_228 = v_s_227[new V("player")][new V("hunterMarkTarget")];
if (VisualJs.And(VisualJs.And(v_target_228, () => new V(v_target_228[new V("hp")] > new V(0d))), () => new V(v_s_227[new V("player")][new V("hunterMarkTimer")] > new V(0d))).Truth) {
v_ctx_226[new V("save")].Call();
V v_hunt_229 = VisualJs.And(new V(v_s_227[new V("player")][new V("hunterHuntTarget")] == v_target_228), () => new V(v_s_227[new V("player")][new V("hunterHuntTimer")] > new V(0d)));
VisualJs.Assign(v_ctx_226,new V("strokeStyle"),"=",(v_hunt_229.Truth ? new V("#d4943d") : new V("#c46d3d")));
VisualJs.Assign(v_ctx_226,new V("lineWidth"),"=",(v_hunt_229.Truth ? new V(3d) : new V(2d)));
v_ctx_226[new V("setLineDash")].Call((v_hunt_229.Truth ? VisualJs.Arr(new V(10d), new V(5d)) : VisualJs.Arr(new V(6d), new V(4d))));
v_ctx_226[new V("beginPath")].Call();
v_ctx_226[new V("arc")].Call(v_target_228[new V("pos")][new V("x")], v_target_228[new V("pos")][new V("y")], (v_target_228[new V("radius")] + ((v_hunt_229.Truth ? new V(14d) : new V(9d)))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_226[new V("stroke")].Call();
v_ctx_226[new V("setLineDash")].Call(VisualJs.Arr());
VisualJs.Assign(v_ctx_226,new V("fillStyle"),"=",(v_hunt_229.Truth ? new V("#d4943d") : new V("#c46d3d")));
VisualJs.Assign(v_ctx_226,new V("font"),"=",new V("bold 10px Georgia, serif"));
VisualJs.Assign(v_ctx_226,new V("textAlign"),"=",new V("center"));
v_ctx_226[new V("fillText")].Call((v_hunt_229.Truth ? new V("HUNT") : new V("MARK")), v_target_228[new V("pos")][new V("x")], ((v_target_228[new V("pos")][new V("y")] - v_target_228[new V("radius")]) - new V(12d)));
v_ctx_226[new V("restore")].Call();
}
return default;
}
private static V F_drawAlchemistReactions(V[] args) {
V v_ctx_230 = VisualJs.Arg(args,0);
V v_s_231 = VisualJs.Arg(args,1);
foreach (V v_enemy_232 in v_s_231[new V("enemies")].Items) {
if (new V(v_enemy_232[new V("hp")] <= new V(0d)).Truth) continue;
V v_count_233 = Scope[new V("countStatusEffects")].Call(v_enemy_232);
if (new V(v_count_233 < new V(2d)).Truth) continue;
V v_symbol_234 = new V("✦");
if (VisualJs.And(new V(v_enemy_232[new V("fireTimer")] > new V(0d)), () => new V(v_enemy_232[new V("poisonTimer")] > new V(0d))).Truth) v_symbol_234 = new V("TP"); else if (VisualJs.And(new V(v_enemy_232[new V("freezeTimer")] > new V(0d)), () => new V(v_enemy_232[new V("poisonTimer")] > new V(0d))).Truth) v_symbol_234 = new V("CP"); else if (VisualJs.And(new V(v_enemy_232[new V("fireTimer")] > new V(0d)), () => new V(v_enemy_232[new V("freezeTimer")] > new V(0d))).Truth) v_symbol_234 = new V("TS");
v_ctx_230[new V("save")].Call();
VisualJs.Assign(v_ctx_230,new V("fillStyle"),"=",new V("#8a5a8a"));
VisualJs.Assign(v_ctx_230,new V("font"),"=",new V("bold 10px Georgia, serif"));
VisualJs.Assign(v_ctx_230,new V("textAlign"),"=",new V("center"));
v_ctx_230[new V("fillText")].Call(v_symbol_234, v_enemy_232[new V("pos")][new V("x")], ((v_enemy_232[new V("pos")][new V("y")] - v_enemy_232[new V("radius")]) - new V(6d)));
v_ctx_230[new V("restore")].Call();
}
return default;
}
private static V F_drawBerserkerRange(V[] args) {
V v_ctx_235 = VisualJs.Arg(args,0);
V v_s_236 = VisualJs.Arg(args,1);
v_ctx_235[new V("save")].Call();
V v_missing_237 = VisualJs.MathObject[new V("max")].Call(new V(0d), (new V(1d) - (v_s_236[new V("player")][new V("hp")] / VisualJs.MathObject[new V("max")].Call(new V(1d), v_s_236[new V("player")][new V("maxHp")]))));
V v_steps_238 = VisualJs.MathObject[new V("min")].Call(new V(4d), VisualJs.MathObject[new V("floor")].Call((v_missing_237 / new V(0.2d))));
VisualJs.Assign(v_ctx_235,new V("strokeStyle"),"=",(new V(v_steps_238 > new V(0d)).Truth ? new V("rgba(196,69,61,0.22)") : new V("rgba(196,69,61,0.10)")));
VisualJs.Assign(v_ctx_235,new V("lineWidth"),"=",(new V(v_steps_238 > new V(0d)).Truth ? new V(2d) : new V(1d)));
v_ctx_235[new V("setLineDash")].Call(VisualJs.Arr(new V(6d), new V(5d)));
v_ctx_235[new V("beginPath")].Call();
v_ctx_235[new V("arc")].Call(v_s_236[new V("player")][new V("pos")][new V("x")], v_s_236[new V("player")][new V("pos")][new V("y")], new V(110d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_235[new V("stroke")].Call();
v_ctx_235[new V("setLineDash")].Call(VisualJs.Arr());
v_ctx_235[new V("restore")].Call();
return default;
}
private static V F_countStatusEffects(V[] args) {
V v_enemy_239 = VisualJs.Arg(args,0);
return ((VisualJs.Number(new V(v_enemy_239[new V("fireTimer")] > new V(0d))) + VisualJs.Number(new V(v_enemy_239[new V("freezeTimer")] > new V(0d)))) + VisualJs.Number(new V(v_enemy_239[new V("poisonTimer")] > new V(0d))));
}
private static V F_dist2D(V[] args) {
V v_a_240 = VisualJs.Arg(args,0);
V v_b_241 = VisualJs.Arg(args,1);
return VisualJs.MathObject[new V("hypot")].Call((v_a_240[new V("x")] - v_b_241[new V("x")]), (v_a_240[new V("y")] - v_b_241[new V("y")]));
}
private static V F_drawPolyline(V[] args) {
V v_ctx_242 = VisualJs.Arg(args,0);
V v_spheres_243 = VisualJs.Arg(args,1);
if (new V(v_spheres_243[new V("length")] < new V(2d)).Truth) return default;
v_ctx_242[new V("beginPath")].Call();
v_spheres_243[new V("forEach")].Call(VisualJs.Fn(args_244 => { V v_sphere_245 = VisualJs.Arg(args_244, 0);
V v_index_246 = VisualJs.Arg(args_244, 1);
return (new V(v_index_246 == new V(0d)).Truth ? v_ctx_242[new V("moveTo")].Call(v_sphere_245[new V("pos")][new V("x")], v_sphere_245[new V("pos")][new V("y")]) : v_ctx_242[new V("lineTo")].Call(v_sphere_245[new V("pos")][new V("x")], v_sphere_245[new V("pos")][new V("y")])); }));
v_ctx_242[new V("stroke")].Call();
return default;
}
private static V F_drawPolygon(V[] args) {
V v_ctx_247 = VisualJs.Arg(args,0);
V v_spheres_248 = VisualJs.Arg(args,1);
if (new V(v_spheres_248[new V("length")] < new V(3d)).Truth) return default;
v_ctx_247[new V("beginPath")].Call();
v_spheres_248[new V("forEach")].Call(VisualJs.Fn(args_249 => { V v_sphere_250 = VisualJs.Arg(args_249, 0);
V v_index_251 = VisualJs.Arg(args_249, 1);
return (new V(v_index_251 == new V(0d)).Truth ? v_ctx_247[new V("moveTo")].Call(v_sphere_250[new V("pos")][new V("x")], v_sphere_250[new V("pos")][new V("y")]) : v_ctx_247[new V("lineTo")].Call(v_sphere_250[new V("pos")][new V("x")], v_sphere_250[new V("pos")][new V("y")])); }));
v_ctx_247[new V("closePath")].Call();
v_ctx_247[new V("stroke")].Call();
v_ctx_247[new V("fill")].Call();
return default;
}
private static V F_drawVoidAmbient(V[] args) {
V v_ctx_252 = VisualJs.Arg(args,0);
V v_w_253 = VisualJs.Arg(args,1);
V v_h_254 = VisualJs.Arg(args,2);
V v_time_255 = VisualJs.Arg(args,3);
v_ctx_252[new V("save")].Call();
V v_count_256 = VisualJs.MathObject[new V("min")].Call(new V(90d), VisualJs.MathObject[new V("floor")].Call((((v_w_253 * v_h_254)) / new V(11000d))));
for (V v_i_257 = new V(0d); new V(v_i_257 < v_count_256).Truth; v_i_257++) {
V v_x_258 = ((((v_i_257 * new V(97.13d))) % v_w_253));
V v_y_259 = (((((v_i_257 * new V(53.71d)) + (v_time_255 * ((new V(2d) + ((v_i_257 % new V(3d)))))))) % v_h_254));
V v_pulse_260 = (new V(0.28d) + (new V(0.22d) * VisualJs.MathObject[new V("sin")].Call(((v_time_255 * new V(1.4d)) + v_i_257))));
VisualJs.Assign(v_ctx_252,new V("fillStyle"),"=",(new V("rgba(120,190,255,") + (v_pulse_260 * new V(0.16d)) + new V(")")));
v_ctx_252[new V("beginPath")].Call();
v_ctx_252[new V("arc")].Call(v_x_258, v_y_259, (new V(0.7d) + (((v_i_257 % new V(3d))) * new V(0.35d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_252[new V("fill")].Call();
}
v_ctx_252[new V("restore")].Call();
return default;
}
private static V F_drawVoidField(V[] args) {
V v_ctx_261 = VisualJs.Arg(args,0);
V v_w_262 = VisualJs.Arg(args,1);
V v_h_263 = VisualJs.Arg(args,2);
V v_theme_264 = VisualJs.Arg(args,3);
V v_ox_265 = VisualJs.Arg(args,4).Undefined?new V(0d):VisualJs.Arg(args,4);
V v_oy_266 = VisualJs.Arg(args,5).Undefined?new V(0d):VisualJs.Arg(args,5);
V v_focusX_267 = VisualJs.Arg(args,6).Undefined?new V(0d):VisualJs.Arg(args,6);
V v_focusY_268 = VisualJs.Arg(args,7).Undefined?new V(0d):VisualJs.Arg(args,7);
V v_cx_269 = v_focusX_267;
V v_cy_270 = v_focusY_268;
V v_outer_271 = VisualJs.MathObject[new V("max")].Call(v_w_262, v_h_263);
V v_g_272 = v_ctx_261[new V("createRadialGradient")].Call(v_cx_269, (v_cy_270 - (v_h_263 * new V(0.14d))), new V(10d), v_cx_269, v_cy_270, (v_outer_271 * new V(0.78d)));
v_g_272[new V("addColorStop")].Call(new V(0d), new V("rgba(41,84,142,0.18)"));
v_g_272[new V("addColorStop")].Call(new V(0.26d), new V("rgba(58,37,121,0.10)"));
v_g_272[new V("addColorStop")].Call(new V(0.62d), new V("rgba(3,10,23,0.18)"));
v_g_272[new V("addColorStop")].Call(new V(1d), new V("rgba(0,0,0,0.82)"));
VisualJs.Assign(v_ctx_261,new V("fillStyle"),"=",v_g_272);
v_ctx_261[new V("fillRect")].Call(v_ox_265, v_oy_266, v_w_262, v_h_263);
v_ctx_261[new V("save")].Call();
V v_nebulae_273 = VisualJs.Arr(VisualJs.Arr(new V(0.18d), new V(0.3d), new V(0.24d), new V("rgba(44,168,255,0.035)")), VisualJs.Arr(new V(0.76d), new V(0.28d), new V(0.22d), new V("rgba(178,75,255,0.030)")), VisualJs.Arr(new V(0.52d), new V(0.69d), new V(0.3d), new V("rgba(70,117,255,0.030)")));
foreach (V item_274 in v_nebulae_273.Items) {
V v_nx_275 = item_274[new V(0d)];
V v_ny_276 = item_274[new V(1d)];
V v_nr_277 = item_274[new V(2d)];
V v_color_278 = item_274[new V(3d)];
{
V v_x_279 = (v_ox_265 + (v_w_262 * v_nx_275));
V v_y_280 = (v_oy_266 + (v_h_263 * v_ny_276));
VisualJs.Assign(v_ctx_261,new V("fillStyle"),"=",v_color_278);
v_ctx_261[new V("beginPath")].Call();
v_ctx_261[new V("arc")].Call(v_x_279, v_y_280, (v_w_262 * v_nr_277), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_261[new V("fill")].Call();
}
}
v_ctx_261[new V("restore")].Call();
v_ctx_261[new V("save")].Call();
VisualJs.Assign(v_ctx_261,new V("strokeStyle"),"=",new V("rgba(96,184,255,0.032)"));
VisualJs.Assign(v_ctx_261,new V("lineWidth"),"=",new V(1d));
for (V v_i_281 = new V(1d); new V(v_i_281 < new V(5d)).Truth; v_i_281++) {
V v_y_282 = ((v_cy_270 + (v_h_263 * new V(0.22d))) + (((v_i_281 * v_i_281) * v_h_263) * new V(0.045d)));
v_ctx_261[new V("beginPath")].Call();
v_ctx_261[new V("moveTo")].Call((v_ox_265 + (v_w_262 * new V(0.08d))), v_y_282);
v_ctx_261[new V("lineTo")].Call((v_ox_265 + (v_w_262 * new V(0.92d))), v_y_282);
v_ctx_261[new V("stroke")].Call();
}
v_ctx_261[new V("restore")].Call();
v_ctx_261[new V("save")].Call();
V v_horizon_283 = (v_oy_266 + (v_h_263 * new V(0.4d)));
VisualJs.Assign(v_ctx_261,new V("strokeStyle"),"=",new V("rgba(115,206,255,0.055)"));
VisualJs.Assign(v_ctx_261,new V("lineWidth"),"=",new V(1d));
for (V v_i_284 = new V(0d); new V(v_i_284 < new V(8d)).Truth; v_i_284++) {
V v_yy_285 = (v_horizon_283 + ((VisualJs.MathObject[new V("pow")].Call((v_i_284 / new V(8d)), new V(1.9d)) * v_h_263) * new V(0.48d)));
v_ctx_261[new V("beginPath")].Call();
v_ctx_261[new V("moveTo")].Call((v_ox_265 + (v_w_262 * new V(0.06d))), v_yy_285);
v_ctx_261[new V("lineTo")].Call((v_ox_265 + (v_w_262 * new V(0.94d))), v_yy_285);
v_ctx_261[new V("stroke")].Call();
}
v_ctx_261[new V("restore")].Call();
v_ctx_261[new V("save")].Call();
for (V v_i_286 = new V(0d); new V(v_i_286 < new V(120d)).Truth; v_i_286++) {
V v_sx_287 = (v_ox_265 + ((((v_i_286 * new V(137.31d))) % v_w_262)));
V v_sy_288 = (v_oy_266 + ((((v_i_286 * new V(71.93d))) % v_h_263)));
V v_alpha_289 = (new V(0.12d) + (((((v_i_286 * new V(17d))) % new V(60d))) / new V(600d)));
VisualJs.Assign(v_ctx_261,new V("fillStyle"),"=",(new V("rgba(190,230,255,") + v_alpha_289 + new V(")")));
v_ctx_261[new V("beginPath")].Call();
v_ctx_261[new V("arc")].Call(v_sx_287, v_sy_288, (new V(0.35d) + (((v_i_286 % new V(3d))) * new V(0.18d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_261[new V("fill")].Call();
}
v_ctx_261[new V("restore")].Call();
return default;
}
private static V F_drawModernXp(V[] args) {
V v_ctx_290 = VisualJs.Arg(args,0);
V v_x_291 = VisualJs.Arg(args,1);
V v_y_292 = VisualJs.Arg(args,2);
V v_r_293 = VisualJs.Arg(args,3);
V v_color_294 = VisualJs.Arg(args,4);
V v_rotation_295 = VisualJs.Arg(args,5);
V v_rot_296 = v_rotation_295;
v_ctx_290[new V("save")].Call();
v_ctx_290[new V("translate")].Call(v_x_291, v_y_292);
v_ctx_290[new V("rotate")].Call(v_rot_296);
VisualJs.Assign(v_ctx_290,new V("fillStyle"),"=",v_color_294);
v_ctx_290[new V("beginPath")].Call();
v_ctx_290[new V("moveTo")].Call(new V(0d), ((-v_r_293) * new V(1.1d)));
v_ctx_290[new V("lineTo")].Call((v_r_293 * new V(0.62d)), ((-v_r_293) * new V(0.35d)));
v_ctx_290[new V("lineTo")].Call((v_r_293 * new V(0.52d)), (v_r_293 * new V(0.68d)));
v_ctx_290[new V("lineTo")].Call(new V(0d), (v_r_293 * new V(1.05d)));
v_ctx_290[new V("lineTo")].Call(((-v_r_293) * new V(0.52d)), (v_r_293 * new V(0.68d)));
v_ctx_290[new V("lineTo")].Call(((-v_r_293) * new V(0.62d)), ((-v_r_293) * new V(0.35d)));
v_ctx_290[new V("closePath")].Call();
v_ctx_290[new V("fill")].Call();
VisualJs.Assign(v_ctx_290,new V("strokeStyle"),"=",new V("rgba(230,253,255,.82)"));
VisualJs.Assign(v_ctx_290,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.7d), (v_r_293 * new V(0.1d))));
v_ctx_290[new V("stroke")].Call();
VisualJs.Assign(v_ctx_290,new V("fillStyle"),"=",new V("rgba(255,255,255,.58)"));
v_ctx_290[new V("beginPath")].Call();
v_ctx_290[new V("arc")].Call(((-v_r_293) * new V(0.2d)), ((-v_r_293) * new V(0.3d)), VisualJs.MathObject[new V("max")].Call(new V(0.8d), (v_r_293 * new V(0.12d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_290[new V("fill")].Call();
v_ctx_290[new V("restore")].Call();
return default;
}
private static V F_drawModernHealth(V[] args) {
V v_ctx_297 = VisualJs.Arg(args,0);
V v_x_298 = VisualJs.Arg(args,1);
V v_y_299 = VisualJs.Arg(args,2);
V v_color_300 = VisualJs.Arg(args,3);
V v_t_301 = Scope[new V("RENDER_TIME")];
V v_p_302 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_301 * new V(5d))) * new V(0.07d)));
v_ctx_297[new V("save")].Call();
v_ctx_297[new V("translate")].Call(v_x_298, v_y_299);
v_ctx_297[new V("scale")].Call(v_p_302, v_p_302);
VisualJs.Assign(v_ctx_297,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_297,new V("shadowColor"),"=",v_color_300);
VisualJs.Assign(v_ctx_297,new V("shadowBlur"),"=",new V(4d));
VisualJs.Assign(v_ctx_297,new V("fillStyle"),"=",new V("rgba(7,13,22,.94)"));
VisualJs.Assign(v_ctx_297,new V("strokeStyle"),"=",new V("rgba(255,125,150,.90)"));
VisualJs.Assign(v_ctx_297,new V("lineWidth"),"=",new V(1.4d));
v_ctx_297[new V("beginPath")].Call();
v_ctx_297[new V("moveTo")].Call((-new V(4d)), (-new V(12d)));
v_ctx_297[new V("lineTo")].Call(new V(4d), (-new V(12d)));
v_ctx_297[new V("lineTo")].Call(new V(4d), (-new V(4d)));
v_ctx_297[new V("lineTo")].Call(new V(12d), (-new V(4d)));
v_ctx_297[new V("lineTo")].Call(new V(12d), new V(4d));
v_ctx_297[new V("lineTo")].Call(new V(4d), new V(4d));
v_ctx_297[new V("lineTo")].Call(new V(4d), new V(12d));
v_ctx_297[new V("lineTo")].Call((-new V(4d)), new V(12d));
v_ctx_297[new V("lineTo")].Call((-new V(4d)), new V(4d));
v_ctx_297[new V("lineTo")].Call((-new V(12d)), new V(4d));
v_ctx_297[new V("lineTo")].Call((-new V(12d)), (-new V(4d)));
v_ctx_297[new V("lineTo")].Call((-new V(4d)), (-new V(4d)));
v_ctx_297[new V("closePath")].Call();
v_ctx_297[new V("fill")].Call();
v_ctx_297[new V("stroke")].Call();
VisualJs.Assign(v_ctx_297,new V("fillStyle"),"=",new V("#fff"));
v_ctx_297[new V("fillRect")].Call((-new V(2d)), (-new V(7d)), new V(4d), new V(14d));
v_ctx_297[new V("fillRect")].Call((-new V(7d)), (-new V(2d)), new V(14d), new V(4d));
v_ctx_297[new V("restore")].Call();
return default;
}
private static V F_drawModernMinion(V[] args) {
V v_ctx_303 = VisualJs.Arg(args,0);
V v_x_304 = VisualJs.Arg(args,1);
V v_y_305 = VisualJs.Arg(args,2);
V v_r_306 = VisualJs.Arg(args,3);
V v_rotation_307 = VisualJs.Arg(args,4);
V v_color_308 = VisualJs.Arg(args,5);
V v_t_309 = Scope[new V("RENDER_TIME")];
V v_bob_310 = ((VisualJs.MathObject[new V("sin")].Call(((v_t_309 * new V(5d)) + (v_x_304 * new V(0.01d)))) * v_r_306) * new V(0.08d));
V v_rgb_311 = Scope[new V("hexToRgb")].Call(v_color_308);
v_ctx_303[new V("save")].Call();
v_ctx_303[new V("translate")].Call(v_x_304, (v_y_305 + v_bob_310));
v_ctx_303[new V("rotate")].Call((v_rotation_307 + (v_t_309 * new V(0.12d))));
VisualJs.Assign(v_ctx_303,new V("globalCompositeOperation"),"=",new V("source-over"));
Scope[new V("drawGroundShadow")].Call(v_ctx_303, (v_r_306 * new V(0.72d)), (v_r_306 * new V(0.22d)), new V(4d));
VisualJs.Assign(v_ctx_303,new V("shadowColor"),"=",v_color_308);
VisualJs.Assign(v_ctx_303,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_303,new V("fillStyle"),"=",new V("#06101c"));
VisualJs.Assign(v_ctx_303,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_311 + new V(",.86)")));
VisualJs.Assign(v_ctx_303,new V("lineWidth"),"=",new V(1.1d));
v_ctx_303[new V("beginPath")].Call();
v_ctx_303[new V("moveTo")].Call(new V(0d), (-v_r_306));
v_ctx_303[new V("lineTo")].Call((v_r_306 * new V(0.72d)), ((-v_r_306) * new V(0.34d)));
v_ctx_303[new V("lineTo")].Call((v_r_306 * new V(0.52d)), (v_r_306 * new V(0.62d)));
v_ctx_303[new V("lineTo")].Call(new V(0d), (v_r_306 * new V(0.72d)));
v_ctx_303[new V("lineTo")].Call(((-v_r_306) * new V(0.52d)), (v_r_306 * new V(0.62d)));
v_ctx_303[new V("lineTo")].Call(((-v_r_306) * new V(0.72d)), ((-v_r_306) * new V(0.34d)));
v_ctx_303[new V("closePath")].Call();
v_ctx_303[new V("fill")].Call();
v_ctx_303[new V("stroke")].Call();
for (V v_i_312 = new V(0d); new V(v_i_312 < new V(3d)).Truth; v_i_312++) {
V v_a_313 = ((v_t_309 * new V(1.7d)) + (((v_i_312 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
VisualJs.Assign(v_ctx_303,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_311 + new V(",.42)")));
v_ctx_303[new V("beginPath")].Call();
v_ctx_303[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_313) * v_r_306) * new V(0.25d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_313) * v_r_306) * new V(0.2d)));
v_ctx_303[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_313) * v_r_306) * new V(0.88d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_313) * v_r_306) * new V(0.52d)));
v_ctx_303[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_303,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_303,new V("fillStyle"),"=",new V("#fff"));
v_ctx_303[new V("beginPath")].Call();
v_ctx_303[new V("arc")].Call(new V(0d), ((-v_r_306) * new V(0.1d)), (v_r_306 * new V(0.24d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_303[new V("fill")].Call();
VisualJs.Assign(v_ctx_303,new V("fillStyle"),"=",v_color_308);
v_ctx_303[new V("beginPath")].Call();
v_ctx_303[new V("arc")].Call(new V(0d), ((-v_r_306) * new V(0.1d)), (v_r_306 * new V(0.16d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_303[new V("fill")].Call();
v_ctx_303[new V("restore")].Call();
return default;
}
private static V F_drawEnergyBolt(V[] args) {
V v_ctx_314 = VisualJs.Arg(args,0);
V v_from_315 = VisualJs.Arg(args,1);
V v_to_316 = VisualJs.Arg(args,2);
V v_alpha_317 = VisualJs.Arg(args,3);
V v_dx_318 = (v_to_316[new V("x")] - v_from_315[new V("x")]);
V v_dy_319 = (v_to_316[new V("y")] - v_from_315[new V("y")]);
V v_len_320 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_dx_318, v_dy_319), () => new V(1d));
V v_nx_321 = ((-v_dy_319) / v_len_320);
V v_ny_322 = (v_dx_318 / v_len_320);
V v_points_323 = new V(7d);
V v_wobble_324 = VisualJs.MathObject[new V("min")].Call(new V(26d), (v_len_320 * new V(0.08d)));
v_ctx_314[new V("save")].Call();
VisualJs.Assign(v_ctx_314,new V("lineCap"),"=",new V("round"));
VisualJs.Assign(v_ctx_314,new V("shadowColor"),"=",new V("#6eeaff"));
VisualJs.Assign(v_ctx_314,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_314,new V("strokeStyle"),"=",(new V("rgba(105,235,255,") + (new V(0.28d) * v_alpha_317) + new V(")")));
VisualJs.Assign(v_ctx_314,new V("lineWidth"),"=",new V(7d));
v_ctx_314[new V("beginPath")].Call();
v_ctx_314[new V("moveTo")].Call(v_from_315[new V("x")], v_from_315[new V("y")]);
for (V v_i_325 = new V(1d); new V(v_i_325 < v_points_323).Truth; v_i_325++) {
V v_p_326 = (v_i_325 / v_points_323);
V v_wave_327 = ((VisualJs.MathObject[new V("sin")].Call(((v_i_325 * new V(3.7d)) + (Scope[new V("RENDER_TIME")] * new V(33.3333333d)))) * v_wobble_324) * new V(0.35d));
v_ctx_314[new V("lineTo")].Call(((v_from_315[new V("x")] + (v_dx_318 * v_p_326)) + (v_nx_321 * v_wave_327)), ((v_from_315[new V("y")] + (v_dy_319 * v_p_326)) + (v_ny_322 * v_wave_327)));
}
v_ctx_314[new V("lineTo")].Call(v_to_316[new V("x")], v_to_316[new V("y")]);
v_ctx_314[new V("stroke")].Call();
VisualJs.Assign(v_ctx_314,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_314,new V("strokeStyle"),"=",(new V("rgba(235,252,255,") + (new V(0.9d) * v_alpha_317) + new V(")")));
VisualJs.Assign(v_ctx_314,new V("lineWidth"),"=",new V(1.5d));
v_ctx_314[new V("beginPath")].Call();
v_ctx_314[new V("moveTo")].Call(v_from_315[new V("x")], v_from_315[new V("y")]);
for (V v_i_328 = new V(1d); new V(v_i_328 < v_points_323).Truth; v_i_328++) {
V v_p_329 = (v_i_328 / v_points_323);
V v_wave_330 = ((VisualJs.MathObject[new V("sin")].Call(((v_i_328 * new V(3.7d)) + (Scope[new V("RENDER_TIME")] * new V(33.3333333d)))) * v_wobble_324) * new V(0.35d));
v_ctx_314[new V("lineTo")].Call(((v_from_315[new V("x")] + (v_dx_318 * v_p_329)) + (v_nx_321 * v_wave_330)), ((v_from_315[new V("y")] + (v_dy_319 * v_p_329)) + (v_ny_322 * v_wave_330)));
}
v_ctx_314[new V("lineTo")].Call(v_to_316[new V("x")], v_to_316[new V("y")]);
v_ctx_314[new V("stroke")].Call();
v_ctx_314[new V("restore")].Call();
return default;
}
private static V F_hexToRgb(V[] args) {
V v_hex_331 = VisualJs.Arg(args,0);
V v_h_332 = v_hex_331[new V("replace")].Call(new V("#"), new V(""));
return (new V("") + VisualJs.NumberObject[new V("parseInt")].Call(v_h_332[new V("substring")].Call(new V(0d), new V(2d)), new V(16d)) + new V(",") + VisualJs.NumberObject[new V("parseInt")].Call(v_h_332[new V("substring")].Call(new V(2d), new V(4d)), new V(16d)) + new V(",") + VisualJs.NumberObject[new V("parseInt")].Call(v_h_332[new V("substring")].Call(new V(4d), new V(6d)), new V(16d)) + new V(""));
}
private static V F_drawPlayer(V[] args) {
V v_ctx_333 = VisualJs.Arg(args,0);
V v_p_334 = VisualJs.Arg(args,1);
V v_r_335 = Visual_engineState.Scope[new V("PLAYER_RADIUS")];
V v_id_336 = Visual_characterRuntime.Scope[new V("getCharacterId")].Call((VisualJs.Obj(new V("player"), v_p_334)));
V v_color_337 = Visual_characters.Scope[new V("CHARACTER_DEFS")][v_id_336][new V("color")];
V v_rgb_338 = Scope[new V("hexToRgb")].Call(v_color_337);
V v_t_339 = Scope[new V("RENDER_TIME")];
V v_bob_340 = (VisualJs.MathObject[new V("sin")].Call((v_t_339 * new V(2.4d))) * new V(0.8d));
V v_pulse_341 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_339 * new V(5.5d))) * new V(0.025d)));
v_ctx_333[new V("save")].Call();
v_ctx_333[new V("translate")].Call(v_p_334[new V("pos")][new V("x")], ((v_p_334[new V("pos")][new V("y")] - (v_r_335 * new V(0.1d))) + v_bob_340));
v_ctx_333[new V("scale")].Call(v_pulse_341, v_pulse_341);
Scope[new V("drawGroundShadow")].Call(v_ctx_333, (v_r_335 * new V(0.95d)), (v_r_335 * new V(0.25d)), new V(8d));
VisualJs.Assign(v_ctx_333,new V("globalCompositeOperation"),"=",new V("source-over"));
if (new V(v_id_336 == new V("spherist")).Truth) {
V v_animState_342 = Scope[new V("getSpheristAnimationState")].Call(v_p_334);
V v_moving_343 = new V(v_animState_342 == new V("move"));
V v_speed_344 = VisualJs.MathObject[new V("hypot")].Call(Scope[new V("SPHERIST_VX")], Scope[new V("SPHERIST_VY")]);
V v_movingIntensity_345 = VisualJs.MathObject[new V("min")].Call(new V(1d), (v_speed_344 / new V(0.35d)));
V v_movingRight_346 = new V(Scope[new V("SPHERIST_VX")] >= new V(0d));
V v_viewKey_347 = (v_moving_343.Truth ? new V("player-spherist-side") : new V("player-spherist"));
V v_viewScale_348 = new V(1d);
V v_viewBob_349 = new V(0d);
V v_viewRotation_350 = new V(0d);
if (new V(v_animState_342 == new V("death")).Truth) {
V v_deathProgress_351 = VisualJs.MathObject[new V("min")].Call(new V(1d), VisualJs.MathObject[new V("max")].Call(new V(0d), (new V(1d) - (v_p_334[new V("deathTimer")] / new V(0.9d)))));
V v_fold_352 = (v_deathProgress_351 * v_deathProgress_351);
v_ctx_333[new V("translate")].Call(new V(0d), (((-v_deathProgress_351) * v_r_335) * new V(0.1d)));
v_ctx_333[new V("transform")].Call((new V(1d) - (v_fold_352 * new V(0.45d))), new V(0d), ((-new V(0.1d)) * v_fold_352), (new V(1d) + (v_fold_352 * new V(0.06d))), new V(0d), new V(0d));
v_ctx_333[new V("scale")].Call((v_pulse_341 * ((new V(1d) - (v_fold_352 * new V(0.18d))))), (v_pulse_341 * ((new V(1d) - (v_fold_352 * new V(0.18d))))));
V v_drawn_353 = Scope[new V("drawArtSprite")].Call(v_ctx_333, new V("player-spherist"), new V(0d), new V(0d), (v_r_335 * new V(2.9d)), (v_viewRotation_350 + (v_fold_352 * new V(0.25d))), (new V(1d) - (v_deathProgress_351 * new V(0.92d))));
if (v_drawn_353.Truth) {
V v_fragmentProgress_354 = VisualJs.MathObject[new V("max")].Call(new V(0d), (((v_deathProgress_351 - new V(0.22d))) / new V(0.78d)));
v_ctx_333[new V("save")].Call();
VisualJs.Assign(v_ctx_333,new V("globalAlpha"),"=",(((new V(1d) - v_fragmentProgress_354)) * new V(0.72d)));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",new V("#dffaff"));
for (V v_i_355 = new V(0d); new V(v_i_355 < new V(8d)).Truth; v_i_355++) {
V v_a_356 = (((v_i_355 * VisualJs.MathObject[new V("PI")]) / new V(4d)) + new V(0.18d));
V v_travel_357 = (v_r_335 * ((new V(0.1d) + (v_fragmentProgress_354 * new V(0.72d)))));
V v_size_358 = (new V(2d) + (v_fragmentProgress_354 * new V(2d)));
v_ctx_333[new V("save")].Call();
v_ctx_333[new V("translate")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_356) * v_travel_357), (VisualJs.MathObject[new V("sin")].Call(v_a_356) * v_travel_357));
v_ctx_333[new V("rotate")].Call((v_a_356 + (v_fragmentProgress_354 * new V(1.3d))));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(new V(0d), (-v_size_358));
v_ctx_333[new V("lineTo")].Call((v_size_358 * new V(0.75d)), new V(0d));
v_ctx_333[new V("lineTo")].Call(new V(0d), v_size_358);
v_ctx_333[new V("lineTo")].Call(((-v_size_358) * new V(0.75d)), new V(0d));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("restore")].Call();
}
v_ctx_333[new V("restore")].Call();
v_ctx_333[new V("restore")].Call();
return default;
}
} else {
v_ctx_333[new V("translate")].Call(new V(0d), v_viewBob_349);
v_ctx_333[new V("scale")].Call((v_pulse_341 * v_viewScale_348), (v_pulse_341 * v_viewScale_348));
if (VisualJs.And(v_moving_343, () => v_movingRight_346).Truth) v_ctx_333[new V("scale")].Call((-new V(1d)), new V(1d));
V v_drawn_359 = Scope[new V("drawArtSprite")].Call(v_ctx_333, v_viewKey_347, new V(0d), new V(0d), (v_r_335 * new V(2.9d)), v_viewRotation_350, new V(1d));
if (v_drawn_359.Truth) {
v_ctx_333[new V("save")].Call();
VisualJs.Assign(v_ctx_333,new V("globalCompositeOperation"),"=",new V("lighter"));
V v_sparkPulse_360 = (new V(0.45d) + (new V(0.35d) * VisualJs.MathObject[new V("sin")].Call((Scope[new V("RENDER_TIME")] * new V(5.5d)))));
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(100,232,255,") + (new V(0.24d) + (v_sparkPulse_360 * new V(0.18d))) + new V(")")));
VisualJs.Assign(v_ctx_333,new V("lineWidth"),"=",new V(1d));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("arc")].Call(new V(0d), new V(0d), (v_r_335 * new V(1.18d)), ((-new V(0.55d)) + (Scope[new V("RENDER_TIME")] * new V(0.7d))), (new V(0.55d) + (Scope[new V("RENDER_TIME")] * new V(0.7d))));
v_ctx_333[new V("stroke")].Call();
for (V v_i_361 = new V(0d); new V(v_i_361 < new V(4d)).Truth; v_i_361++) {
V v_a_362 = ((Scope[new V("RENDER_TIME")] * new V(0.9d)) + ((v_i_361 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_rr_363 = (v_r_335 * ((new V(1.08d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call(((Scope[new V("RENDER_TIME")] * new V(3d)) + v_i_361))))));
V v_sx_364 = (VisualJs.MathObject[new V("cos")].Call(v_a_362) * v_rr_363);
V v_sy_365 = ((VisualJs.MathObject[new V("sin")].Call(v_a_362) * v_rr_363) * new V(0.7d));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",(new V("rgba(220,252,255,") + (new V(0.34d) + (v_sparkPulse_360 * new V(0.2d))) + new V(")")));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(v_sx_364, (v_sy_365 - new V(1.8d)));
v_ctx_333[new V("lineTo")].Call((v_sx_364 + new V(1.8d)), v_sy_365);
v_ctx_333[new V("lineTo")].Call(v_sx_364, (v_sy_365 + new V(1.8d)));
v_ctx_333[new V("lineTo")].Call((v_sx_364 - new V(1.8d)), v_sy_365);
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
}
v_ctx_333[new V("restore")].Call();
v_ctx_333[new V("restore")].Call();
return default;
}
}
}
VisualJs.Assign(v_ctx_333,new V("shadowColor"),"=",v_color_337);
VisualJs.Assign(v_ctx_333,new V("shadowBlur"),"=",new V(5d));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",new V("#071321"));
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.90)")));
VisualJs.Assign(v_ctx_333,new V("lineWidth"),"=",new V(1.5d));
if (new V(v_id_336 == new V("hunter")).Truth) {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(new V(0d), ((-v_r_335) * new V(1d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.48d)), ((-v_r_335) * new V(0.12d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.34d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.44d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.34d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.48d)), ((-v_r_335) * new V(0.12d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.46)")));
VisualJs.Assign(v_ctx_333,new V("lineWidth"),"=",new V(1d));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.92d)), ((-v_r_335) * new V(0.34d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), ((-v_r_335) * new V(0.72d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.92d)), ((-v_r_335) * new V(0.34d)));
v_ctx_333[new V("stroke")].Call();
} else if (new V(v_id_336 == new V("engineer")).Truth) {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.72d)), ((-v_r_335) * new V(0.56d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.72d)), ((-v_r_335) * new V(0.56d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.72d)), (v_r_335 * new V(0.56d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.72d)), (v_r_335 * new V(0.56d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.52)")));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.72d)), new V(0d));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.72d)), new V(0d));
v_ctx_333[new V("moveTo")].Call(new V(0d), ((-v_r_335) * new V(0.56d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.56d)));
v_ctx_333[new V("stroke")].Call();
for (V v_i_366 = new V(0d); new V(v_i_366 < new V(4d)).Truth; v_i_366++) {
V v_a_367 = ((v_t_339 * new V(0.6d)) + ((v_i_366 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",v_color_337);
v_ctx_333[new V("fillRect")].Call((((VisualJs.MathObject[new V("cos")].Call(v_a_367) * v_r_335) * new V(0.86d)) - new V(2d)), (((VisualJs.MathObject[new V("sin")].Call(v_a_367) * v_r_335) * new V(0.62d)) - new V(2d)), new V(4d), new V(4d));
}
} else if (new V(v_id_336 == new V("berserker")).Truth) {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.82d)), (v_r_335 * new V(0.4d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.58d)), ((-v_r_335) * new V(0.62d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.18d)), ((-v_r_335) * new V(0.92d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), ((-v_r_335) * new V(0.56d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.18d)), ((-v_r_335) * new V(0.92d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.58d)), ((-v_r_335) * new V(0.62d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.82d)), (v_r_335 * new V(0.4d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.34d)), (v_r_335 * new V(0.78d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.34d)), (v_r_335 * new V(0.78d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
} else if (new V(v_id_336 == new V("alchemist")).Truth) {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.48d)), ((-v_r_335) * new V(0.82d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.48d)), ((-v_r_335) * new V(0.82d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.62d)), ((-v_r_335) * new V(0.28d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.42d)), (v_r_335 * new V(0.7d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.9d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.42d)), (v_r_335 * new V(0.7d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.62d)), ((-v_r_335) * new V(0.28d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
for (V v_i_368 = new V(0d); new V(v_i_368 < new V(3d)).Truth; v_i_368++) {
V v_a_369 = ((v_t_339 * new V(0.9d)) + (v_i_368 * new V(2.1d)));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.55)")));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("arc")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_369) * v_r_335) * new V(0.9d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_369) * v_r_335) * new V(0.45d)), (new V(2d) + v_i_368), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_333[new V("fill")].Call();
}
} else if (new V(v_id_336 == new V("architect")).Truth) {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(new V(0d), ((-v_r_335) * new V(1d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.78d)), ((-v_r_335) * new V(0.3d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.58d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.46d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.58d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.78d)), ((-v_r_335) * new V(0.3d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.52)")));
VisualJs.Assign(v_ctx_333,new V("lineWidth"),"=",new V(1.1d));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(new V(0d), (-v_r_335));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.46d)));
v_ctx_333[new V("moveTo")].Call(((-v_r_335) * new V(0.78d)), ((-v_r_335) * new V(0.3d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.58d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("moveTo")].Call((v_r_335 * new V(0.78d)), ((-v_r_335) * new V(0.3d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.58d)), (v_r_335 * new V(0.72d)));
v_ctx_333[new V("stroke")].Call();
} else {
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(new V(0d), ((-v_r_335) * new V(1.02d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.74d)), ((-v_r_335) * new V(0.42d)));
v_ctx_333[new V("lineTo")].Call((v_r_335 * new V(0.62d)), (v_r_335 * new V(0.48d)));
v_ctx_333[new V("lineTo")].Call(new V(0d), (v_r_335 * new V(0.78d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.62d)), (v_r_335 * new V(0.48d)));
v_ctx_333[new V("lineTo")].Call(((-v_r_335) * new V(0.74d)), ((-v_r_335) * new V(0.42d)));
v_ctx_333[new V("closePath")].Call();
v_ctx_333[new V("fill")].Call();
v_ctx_333[new V("stroke")].Call();
for (V v_i_370 = new V(0d); new V(v_i_370 < new V(3d)).Truth; v_i_370++) {
V v_a_371 = ((v_t_339 * new V(1.3d)) + (((v_i_370 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
VisualJs.Assign(v_ctx_333,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_338 + new V(",.40)")));
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_371) * v_r_335) * new V(0.35d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_371) * v_r_335) * new V(0.3d)));
v_ctx_333[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_371) * v_r_335) * new V(1d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_371) * v_r_335) * new V(0.62d)));
v_ctx_333[new V("stroke")].Call();
}
}
VisualJs.Assign(v_ctx_333,new V("shadowBlur"),"=",new V(6d));
V v_core_372 = v_ctx_333[new V("createRadialGradient")].Call(((-v_r_335) * new V(0.18d)), ((-v_r_335) * new V(0.2d)), new V(1d), new V(0d), new V(0d), (v_r_335 * new V(0.55d)));
v_core_372[new V("addColorStop")].Call(new V(0d), new V("#ffffff"));
v_core_372[new V("addColorStop")].Call(new V(0.28d), v_color_337);
v_core_372[new V("addColorStop")].Call(new V(0.68d), (new V("rgba(") + v_rgb_338 + new V(",.62)")));
v_core_372[new V("addColorStop")].Call(new V(1d), new V("rgba(0,0,0,0)"));
VisualJs.Assign(v_ctx_333,new V("fillStyle"),"=",v_core_372);
v_ctx_333[new V("beginPath")].Call();
v_ctx_333[new V("arc")].Call(new V(0d), new V(0d), (v_r_335 * new V(0.5d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_333[new V("fill")].Call();
VisualJs.Assign(v_ctx_333,new V("shadowBlur"),"=",new V(0d));
v_ctx_333[new V("restore")].Call();
Scope[new V("drawCharacterVfx")].Call(v_ctx_333, v_p_334);
return default;
}
private static V F_getEnemyFacingAngle(V[] args) {
V v_e_373 = VisualJs.Arg(args,0);
V v_previous_374 = Scope[new V("ENEMY_LAST_POS")][new V("get")].Call(v_e_373);
if (v_previous_374.Truth) {
V v_dx_375 = (v_e_373[new V("pos")][new V("x")] - v_previous_374[new V("x")]);
V v_dy_376 = (v_e_373[new V("pos")][new V("y")] - v_previous_374[new V("y")]);
if (new V(((v_dx_375 * v_dx_375) + (v_dy_376 * v_dy_376)) > new V(0.04d)).Truth) Scope[new V("ENEMY_FACING")][new V("set")].Call(v_e_373, VisualJs.MathObject[new V("atan2")].Call(v_dy_376, v_dx_375));
} else if (new V(VisualJs.TypeOf(v_e_373[new V("rotation")]) == new V("number")).Truth) {
Scope[new V("ENEMY_FACING")][new V("set")].Call(v_e_373, v_e_373[new V("rotation")]);
}
Scope[new V("ENEMY_LAST_POS")][new V("set")].Call(v_e_373, VisualJs.Obj(new V("x"), v_e_373[new V("pos")][new V("x")], new V("y"), v_e_373[new V("pos")][new V("y")]));
return VisualJs.Coalesce(Scope[new V("ENEMY_FACING")][new V("get")].Call(v_e_373), () => new V(0d));
}
private static V F_glowCircle(V[] args) {
V v_ctx_377 = VisualJs.Arg(args,0);
V v_radius_378 = VisualJs.Arg(args,1);
V v_color_379 = VisualJs.Arg(args,2);
V v_alpha_380 = VisualJs.Arg(args,3).Undefined?new V(0.26d):VisualJs.Arg(args,3);
v_ctx_377[new V("save")].Call();
VisualJs.Assign(v_ctx_377,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_377,new V("globalAlpha"),"=",(v_alpha_380 * new V(0.16d)));
VisualJs.Assign(v_ctx_377,new V("fillStyle"),"=",v_color_379);
v_ctx_377[new V("beginPath")].Call();
v_ctx_377[new V("arc")].Call(new V(0d), new V(0d), v_radius_378, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_377[new V("fill")].Call();
VisualJs.Assign(v_ctx_377,new V("globalAlpha"),"=",(v_alpha_380 * new V(0.55d)));
v_ctx_377[new V("beginPath")].Call();
v_ctx_377[new V("arc")].Call(new V(0d), new V(0d), (v_radius_378 * new V(0.52d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_377[new V("fill")].Call();
v_ctx_377[new V("restore")].Call();
return default;
}
private static V F_drawGroundShadow(V[] args) {
V v_ctx_381 = VisualJs.Arg(args,0);
V v_rx_382 = VisualJs.Arg(args,1);
V v_ry_383 = VisualJs.Arg(args,2);
V v__blur_384 = VisualJs.Arg(args,3);
v_ctx_381[new V("save")].Call();
VisualJs.Assign(v_ctx_381,new V("globalAlpha"),"=",new V(0.42d));
VisualJs.Assign(v_ctx_381,new V("fillStyle"),"=",new V("rgba(0,0,0,.34)"));
v_ctx_381[new V("beginPath")].Call();
v_ctx_381[new V("ellipse")].Call(new V(0d), new V(5d), v_rx_382, v_ry_383, new V(0d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_381[new V("fill")].Call();
VisualJs.Assign(v_ctx_381,new V("globalAlpha"),"=",new V(0.16d));
v_ctx_381[new V("beginPath")].Call();
v_ctx_381[new V("ellipse")].Call(new V(0d), new V(5d), (v_rx_382 * new V(0.72d)), (v_ry_383 * new V(0.68d)), new V(0d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_381[new V("fill")].Call();
v_ctx_381[new V("restore")].Call();
return default;
}
private static V F_drawSphereVfx(V[] args) {
V v_ctx_385 = VisualJs.Arg(args,0);
V v_sphere_386 = VisualJs.Arg(args,1);
V v_color_387 = VisualJs.Arg(args,2);
V v_r_388 = VisualJs.Arg(args,3);
V v_time_389 = VisualJs.Arg(args,4);
V v_rgb_390 = Scope[new V("hexToRgb")].Call(v_color_387);
v_ctx_385[new V("save")].Call();
v_ctx_385[new V("translate")].Call(v_sphere_386[new V("pos")][new V("x")], v_sphere_386[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_385,new V("globalCompositeOperation"),"=",new V("lighter"));
V v_pulse_391 = (new V(0.78d) + (VisualJs.MathObject[new V("sin")].Call(((v_time_389 * new V(4.2d)) + (v_sphere_386[new V("pos")][new V("x")] * new V(0.01d)))) * new V(0.22d)));
if (new V(v_sphere_386[new V("type")] == new V("standard")).Truth) {
for (V v_i_392 = new V(0d); new V(v_i_392 < new V(3d)).Truth; v_i_392++) {
V v_a_393 = ((v_time_389 * new V(1.2d)) + (((v_i_392 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
V v_x_394 = ((VisualJs.MathObject[new V("cos")].Call(v_a_393) * v_r_388) * new V(1.35d));
V v_y_395 = ((VisualJs.MathObject[new V("sin")].Call(v_a_393) * v_r_388) * new V(0.72d));
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_390 + new V(",") + (new V(0.48d) * v_pulse_391) + new V(")")));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("moveTo")].Call(v_x_394, (v_y_395 - new V(3d)));
v_ctx_385[new V("lineTo")].Call((v_x_394 + new V(3d)), v_y_395);
v_ctx_385[new V("lineTo")].Call(v_x_394, (v_y_395 + new V(3d)));
v_ctx_385[new V("lineTo")].Call((v_x_394 - new V(3d)), v_y_395);
v_ctx_385[new V("closePath")].Call();
v_ctx_385[new V("fill")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("sniper")).Truth) {
V v_sweep_396 = ((((((v_time_389 * new V(0.9d))) % new V(1d))) * new V(2d)) - new V(1d));
VisualJs.Assign(v_ctx_385,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_390 + new V(",") + (new V(0.38d) * v_pulse_391) + new V(")")));
VisualJs.Assign(v_ctx_385,new V("lineWidth"),"=",new V(1.2d));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("moveTo")].Call(((v_sweep_396 * v_r_388) * new V(1.7d)), ((-v_r_388) * new V(0.75d)));
v_ctx_385[new V("lineTo")].Call(((v_sweep_396 * v_r_388) * new V(1.7d)), (v_r_388 * new V(0.55d)));
v_ctx_385[new V("stroke")].Call();
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_390 + new V(",") + (new V(0.25d) * v_pulse_391) + new V(")")));
v_ctx_385[new V("fillRect")].Call((v_r_388 * new V(0.78d)), (-new V(2d)), (v_r_388 * new V(0.58d)), new V(4d));
} else if (new V(v_sphere_386[new V("type")] == new V("shotgun")).Truth) {
for (V v_i_397 = (-new V(1d)); new V(v_i_397 <= new V(1d)).Truth; v_i_397++) {
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(255,170,90,") + (new V(0.3d) * v_pulse_391) + new V(")")));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("arc")].Call((v_r_388 * new V(0.9d)), ((v_i_397 * v_r_388) * new V(0.23d)), ((v_r_388 * new V(0.09d)) + (v_pulse_391 * new V(1.1d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_385[new V("fill")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("chain")).Truth) {
for (V v_i_398 = new V(0d); new V(v_i_398 < new V(3d)).Truth; v_i_398++) {
V v_a_399 = ((v_time_389 * new V(2.8d)) + (((v_i_398 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
V v_x_400 = ((VisualJs.MathObject[new V("cos")].Call(v_a_399) * v_r_388) * new V(1.25d));
V v_y_401 = ((VisualJs.MathObject[new V("sin")].Call(v_a_399) * v_r_388) * new V(0.62d));
VisualJs.Assign(v_ctx_385,new V("strokeStyle"),"=",(new V("rgba(255,242,150,") + (new V(0.56d) * v_pulse_391) + new V(")")));
VisualJs.Assign(v_ctx_385,new V("lineWidth"),"=",new V(1.4d));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_385[new V("lineTo")].Call(v_x_400, v_y_401);
v_ctx_385[new V("stroke")].Call();
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",new V("#fff6b0"));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("arc")].Call(v_x_400, v_y_401, new V(2.1d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_385[new V("fill")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("aura")).Truth) {
for (V v_i_402 = new V(0d); new V(v_i_402 < new V(7d)).Truth; v_i_402++) {
V v_a_403 = ((v_time_389 * new V(0.8d)) + (((v_i_402 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(7d)));
V v_rr_404 = (v_r_388 * ((new V(1d) + (new V(0.18d) * VisualJs.MathObject[new V("sin")].Call(((v_time_389 * new V(2d)) + v_i_402))))));
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(87,230,180,") + (new V(0.24d) + (new V(0.12d) * v_pulse_391)) + new V(")")));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_403) * v_rr_404), ((VisualJs.MathObject[new V("sin")].Call(v_a_403) * v_rr_404) * new V(0.45d)), new V(2.2d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_385[new V("fill")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("prism")).Truth) {
for (V v_i_405 = new V(0d); new V(v_i_405 < new V(3d)).Truth; v_i_405++) {
V v_a_406 = ((v_time_389 * new V(1.5d)) + (((v_i_405 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
VisualJs.Assign(v_ctx_385,new V("strokeStyle"),"=",(new V("rgba(255,141,225,") + (new V(0.3d) + (new V(0.14d) * v_pulse_391)) + new V(")")));
VisualJs.Assign(v_ctx_385,new V("lineWidth"),"=",new V(1d));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_406) * v_r_388) * new V(0.35d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_406) * v_r_388) * new V(0.35d)));
v_ctx_385[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_406) * v_r_388) * new V(1.55d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_406) * v_r_388) * new V(0.95d)));
v_ctx_385[new V("stroke")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("gravity")).Truth) {
for (V v_i_407 = new V(0d); new V(v_i_407 < new V(8d)).Truth; v_i_407++) {
V v_a_408 = ((v_time_389 * (-new V(0.7d))) + (((v_i_407 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(8d)));
V v_rr_409 = (v_r_388 * ((new V(1.8d) - ((((((v_time_389 * new V(0.55d)) + (v_i_407 * new V(0.22d)))) % new V(1d))) * new V(0.95d)))));
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(165,140,255,") + (new V(0.32d) * v_pulse_391) + new V(")")));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_408) * v_rr_409), ((VisualJs.MathObject[new V("sin")].Call(v_a_408) * v_rr_409) * new V(0.55d)), new V(1.7d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_385[new V("fill")].Call();
}
} else if (new V(v_sphere_386[new V("type")] == new V("pulse")).Truth) {
V v_wave_410 = ((((v_time_389 * new V(1.8d))) % new V(1d)));
VisualJs.Assign(v_ctx_385,new V("strokeStyle"),"=",(new V("rgba(255,211,90,") + (((new V(1d) - v_wave_410)) * new V(0.5d)) + new V(")")));
VisualJs.Assign(v_ctx_385,new V("lineWidth"),"=",new V(1.4d));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("ellipse")].Call(new V(0d), new V(0d), (v_r_388 * ((new V(0.8d) + (v_wave_410 * new V(1.9d))))), (v_r_388 * ((new V(0.32d) + (v_wave_410 * new V(0.78d))))), new V(0d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_385[new V("stroke")].Call();
} else if (new V(v_sphere_386[new V("type")] == new V("void")).Truth) {
for (V v_i_411 = new V(0d); new V(v_i_411 < new V(5d)).Truth; v_i_411++) {
V v_a_412 = ((v_time_389 * (-new V(1.1d))) + (((v_i_411 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(5d)));
V v_rr_413 = (v_r_388 * ((new V(1d) + (new V(0.55d) * VisualJs.MathObject[new V("sin")].Call(((v_time_389 * new V(1.7d)) + v_i_411))))));
VisualJs.Assign(v_ctx_385,new V("fillStyle"),"=",(new V("rgba(194,140,255,") + (new V(0.28d) + (new V(0.12d) * v_pulse_391)) + new V(")")));
v_ctx_385[new V("beginPath")].Call();
v_ctx_385[new V("moveTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_412) * v_rr_413), (((VisualJs.MathObject[new V("sin")].Call(v_a_412) * v_rr_413) * new V(0.55d)) - new V(4d)));
v_ctx_385[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call((v_a_412 + new V(0.18d))) * v_rr_413), ((VisualJs.MathObject[new V("sin")].Call((v_a_412 + new V(0.18d))) * v_rr_413) * new V(0.55d)));
v_ctx_385[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_412) * v_rr_413), (((VisualJs.MathObject[new V("sin")].Call(v_a_412) * v_rr_413) * new V(0.55d)) + new V(4d)));
v_ctx_385[new V("closePath")].Call();
v_ctx_385[new V("fill")].Call();
}
}
v_ctx_385[new V("restore")].Call();
return default;
}
private static V F_drawCharacterVfx(V[] args) {
V v_ctx_414 = VisualJs.Arg(args,0);
V v_p_415 = VisualJs.Arg(args,1);
V v_id_416 = Visual_characterRuntime.Scope[new V("getCharacterId")].Call((VisualJs.Obj(new V("player"), v_p_415)));
V v_color_417 = Visual_characters.Scope[new V("CHARACTER_DEFS")][v_id_416][new V("color")];
V v_rgb_418 = Scope[new V("hexToRgb")].Call(v_color_417);
V v_t_419 = Scope[new V("RENDER_TIME")];
V v_r_420 = Visual_engineState.Scope[new V("PLAYER_RADIUS")];
v_ctx_414[new V("save")].Call();
v_ctx_414[new V("translate")].Call(v_p_415[new V("pos")][new V("x")], (v_p_415[new V("pos")][new V("y")] - (v_r_420 * new V(0.1d))));
VisualJs.Assign(v_ctx_414,new V("globalCompositeOperation"),"=",new V("source-over"));
if (new V(v_id_416 == new V("spherist")).Truth) {
v_ctx_414[new V("restore")].Call();
return default;
} else if (new V(v_id_416 == new V("hunter")).Truth) {
V v_scan_421 = ((VisualJs.MathObject[new V("sin")].Call((v_t_419 * new V(3.5d))) * new V(0.5d)) + new V(0.5d));
VisualJs.Assign(v_ctx_414,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_418 + new V(",") + (new V(0.25d) + (v_scan_421 * new V(0.3d))) + new V(")")));
VisualJs.Assign(v_ctx_414,new V("lineWidth"),"=",new V(1.2d));
v_ctx_414[new V("beginPath")].Call();
v_ctx_414[new V("moveTo")].Call(((-v_r_420) * new V(0.95d)), (v_r_420 * ((new V(0.15d) - (v_scan_421 * new V(0.25d))))));
v_ctx_414[new V("lineTo")].Call((v_r_420 * new V(0.95d)), (v_r_420 * ((new V(0.15d) - (v_scan_421 * new V(0.25d))))));
v_ctx_414[new V("stroke")].Call();
} else if (new V(v_id_416 == new V("engineer")).Truth) {
for (V v_i_422 = new V(0d); new V(v_i_422 < new V(4d)).Truth; v_i_422++) {
V v_a_423 = ((v_t_419 * new V(0.6d)) + ((v_i_422 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_x_424 = ((VisualJs.MathObject[new V("cos")].Call(v_a_423) * v_r_420) * new V(1.05d));
V v_y_425 = ((VisualJs.MathObject[new V("sin")].Call(v_a_423) * v_r_420) * new V(0.72d));
VisualJs.Assign(v_ctx_414,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_418 + new V(",.34)")));
v_ctx_414[new V("fillRect")].Call((v_x_424 - new V(3d)), (v_y_425 - new V(3d)), new V(6d), new V(6d));
}
} else if (new V(v_id_416 == new V("berserker")).Truth) {
V v_pulse_426 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_419 * new V(7d))) * new V(0.08d)));
VisualJs.Assign(v_ctx_414,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_418 + new V(",.58)")));
VisualJs.Assign(v_ctx_414,new V("lineWidth"),"=",new V(2d));
v_ctx_414[new V("beginPath")].Call();
v_ctx_414[new V("moveTo")].Call(((-v_r_420) * new V(0.9d)), ((v_r_420 * new V(0.55d)) * v_pulse_426));
v_ctx_414[new V("lineTo")].Call(new V(0d), ((v_r_420 * new V(0.85d)) * v_pulse_426));
v_ctx_414[new V("lineTo")].Call((v_r_420 * new V(0.9d)), ((v_r_420 * new V(0.55d)) * v_pulse_426));
v_ctx_414[new V("stroke")].Call();
} else if (new V(v_id_416 == new V("alchemist")).Truth) {
for (V v_i_427 = new V(0d); new V(v_i_427 < new V(5d)).Truth; v_i_427++) {
V v_a_428 = ((v_t_419 * new V(0.8d)) + (v_i_427 * new V(1.2d)));
V v_rr_429 = (v_r_420 * ((new V(0.85d) + (new V(0.15d) * VisualJs.MathObject[new V("sin")].Call(((v_t_419 * new V(2d)) + v_i_427))))));
VisualJs.Assign(v_ctx_414,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_418 + new V(",.38)")));
v_ctx_414[new V("beginPath")].Call();
v_ctx_414[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_428) * v_rr_429), ((VisualJs.MathObject[new V("sin")].Call(v_a_428) * v_rr_429) * new V(0.6d)), (new V(2.2d) + ((v_i_427 % new V(2d)))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_414[new V("fill")].Call();
}
} else if (new V(v_id_416 == new V("architect")).Truth) {
for (V v_i_430 = new V(0d); new V(v_i_430 < new V(4d)).Truth; v_i_430++) {
V v_a_431 = ((v_t_419 * new V(0.7d)) + ((v_i_430 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_x_432 = ((VisualJs.MathObject[new V("cos")].Call(v_a_431) * v_r_420) * new V(1.15d));
V v_y_433 = ((VisualJs.MathObject[new V("sin")].Call(v_a_431) * v_r_420) * new V(0.72d));
VisualJs.Assign(v_ctx_414,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_418 + new V(",.46)")));
VisualJs.Assign(v_ctx_414,new V("lineWidth"),"=",new V(1.2d));
v_ctx_414[new V("beginPath")].Call();
v_ctx_414[new V("moveTo")].Call((v_x_432 - new V(5d)), v_y_433);
v_ctx_414[new V("lineTo")].Call((v_x_432 + new V(5d)), v_y_433);
v_ctx_414[new V("stroke")].Call();
v_ctx_414[new V("beginPath")].Call();
v_ctx_414[new V("moveTo")].Call(v_x_432, (v_y_433 - new V(5d)));
v_ctx_414[new V("lineTo")].Call(v_x_432, (v_y_433 + new V(5d)));
v_ctx_414[new V("stroke")].Call();
}
}
v_ctx_414[new V("restore")].Call();
return default;
}
private static V F_drawEliteCrest(V[] args) {
V v_ctx_434 = VisualJs.Arg(args,0);
V v_e_435 = VisualJs.Arg(args,1);
V v_t_436 = VisualJs.Arg(args,2);
V v_r_437 = v_e_435[new V("radius")];
V v_pulse_438 = (new V(0.7d) + (new V(0.3d) * VisualJs.MathObject[new V("sin")].Call((v_t_436 * new V(5d)))));
V v_rgb_439 = Scope[new V("hexToRgb")].Call(v_e_435[new V("color")]);
v_ctx_434[new V("save")].Call();
VisualJs.Assign(v_ctx_434,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_434,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_439 + new V(",") + (new V(0.38d) + (new V(0.2d) * v_pulse_438)) + new V(")")));
VisualJs.Assign(v_ctx_434,new V("lineWidth"),"=",new V(1.5d));
v_ctx_434[new V("beginPath")].Call();
v_ctx_434[new V("arc")].Call(new V(0d), new V(0d), (v_r_437 * new V(1.3d)), ((-VisualJs.MathObject[new V("PI")]) * new V(0.82d)), (VisualJs.MathObject[new V("PI")] * new V(0.82d)));
v_ctx_434[new V("stroke")].Call();
for (V v_i_440 = (-new V(1d)); new V(v_i_440 <= new V(1d)).Truth; v_i_440++) {
V v_a_441 = (((-VisualJs.MathObject[new V("PI")]) * new V(0.5d)) + ((v_i_440 * VisualJs.MathObject[new V("PI")]) * new V(0.5d)));
V v_x_442 = ((VisualJs.MathObject[new V("cos")].Call(v_a_441) * v_r_437) * new V(1.3d));
V v_y_443 = ((VisualJs.MathObject[new V("sin")].Call(v_a_441) * v_r_437) * new V(1.3d));
VisualJs.Assign(v_ctx_434,new V("fillStyle"),"=",v_e_435[new V("color")]);
v_ctx_434[new V("fillRect")].Call((v_x_442 - new V(2.5d)), (v_y_443 - new V(2.5d)), new V(5d), new V(5d));
}
v_ctx_434[new V("restore")].Call();
return default;
}
private static V F_drawEnemyStatusVfx(V[] args) {
V v_ctx_444 = VisualJs.Arg(args,0);
V v_e_445 = VisualJs.Arg(args,1);
V v_t_446 = VisualJs.Arg(args,2);
V v_r_447 = v_e_445[new V("radius")];
v_ctx_444[new V("save")].Call();
VisualJs.Assign(v_ctx_444,new V("globalCompositeOperation"),"=",new V("source-over"));
if (new V(v_e_445[new V("fireTimer")] > new V(0d)).Truth) {
V v_p_448 = (new V(0.5d) + (new V(0.5d) * VisualJs.MathObject[new V("sin")].Call((v_t_446 * new V(10d)))));
for (V v_i_449 = new V(0d); new V(v_i_449 < new V(4d)).Truth; v_i_449++) {
V v_a_450 = ((v_t_446 * new V(2.4d)) + ((v_i_449 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_rr_451 = (v_r_447 * ((new V(0.8d) + (new V(0.18d) * VisualJs.MathObject[new V("sin")].Call(((v_t_446 * new V(5d)) + v_i_449))))));
VisualJs.Assign(v_ctx_444,new V("fillStyle"),"=",(new V("rgba(255,102,61,") + (new V(0.28d) + (new V(0.18d) * v_p_448)) + new V(")")));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_450) * v_rr_451), (((VisualJs.MathObject[new V("sin")].Call(v_a_450) * v_rr_451) * new V(0.65d)) - (v_r_447 * new V(0.18d))), (new V(2.2d) + (v_p_448 * new V(1.4d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_444[new V("fill")].Call();
}
VisualJs.Assign(v_ctx_444,new V("strokeStyle"),"=",(new V("rgba(255,190,90,") + (new V(0.25d) + (new V(0.2d) * v_p_448)) + new V(")")));
VisualJs.Assign(v_ctx_444,new V("lineWidth"),"=",new V(1.4d));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("moveTo")].Call(((-v_r_447) * new V(0.35d)), ((-v_r_447) * new V(0.65d)));
v_ctx_444[new V("quadraticCurveTo")].Call(new V(0d), ((-v_r_447) * ((new V(1.15d) + (v_p_448 * new V(0.15d))))), (v_r_447 * new V(0.35d)), ((-v_r_447) * new V(0.65d)));
v_ctx_444[new V("stroke")].Call();
}
if (new V(v_e_445[new V("poisonTimer")] > new V(0d)).Truth) {
for (V v_i_452 = new V(0d); new V(v_i_452 < new V(5d)).Truth; v_i_452++) {
V v_a_453 = ((v_t_446 * new V(0.65d)) + (v_i_452 * new V(1.23d)));
V v_rr_454 = (v_r_447 * ((new V(0.85d) + (new V(0.18d) * (((((v_t_446 * new V(0.7d)) + (v_i_452 * new V(0.2d)))) % new V(1d)))))));
VisualJs.Assign(v_ctx_444,new V("fillStyle"),"=",new V("rgba(101,227,143,.38)"));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("arc")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_453) * v_rr_454), (((VisualJs.MathObject[new V("sin")].Call(v_a_453) * v_rr_454) * new V(0.55d)) - (v_r_447 * new V(0.12d))), (new V(2d) + (v_i_452 % new V(2d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_444[new V("fill")].Call();
}
VisualJs.Assign(v_ctx_444,new V("strokeStyle"),"=",new V("rgba(113,245,157,.35)"));
VisualJs.Assign(v_ctx_444,new V("lineWidth"),"=",new V(1d));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("moveTo")].Call(((-v_r_447) * new V(0.7d)), (v_r_447 * new V(0.35d)));
v_ctx_444[new V("quadraticCurveTo")].Call(new V(0d), (v_r_447 * new V(0.72d)), (v_r_447 * new V(0.7d)), (v_r_447 * new V(0.35d)));
v_ctx_444[new V("stroke")].Call();
}
if (new V(v_e_445[new V("freezeTimer")] > new V(0d)).Truth) {
V v_spin_455 = (v_t_446 * new V(0.8d));
VisualJs.Assign(v_ctx_444,new V("strokeStyle"),"=",new V("rgba(140,233,255,.72)"));
VisualJs.Assign(v_ctx_444,new V("lineWidth"),"=",new V(1.3d));
for (V v_i_456 = new V(0d); new V(v_i_456 < new V(6d)).Truth; v_i_456++) {
V v_a_457 = (v_spin_455 + ((v_i_456 * VisualJs.MathObject[new V("PI")]) / new V(3d)));
V v_x_458 = ((VisualJs.MathObject[new V("cos")].Call(v_a_457) * v_r_447) * new V(1.12d));
V v_y_459 = ((VisualJs.MathObject[new V("sin")].Call(v_a_457) * v_r_447) * new V(0.72d));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("moveTo")].Call(v_x_458, v_y_459);
v_ctx_444[new V("lineTo")].Call((v_x_458 + ((VisualJs.MathObject[new V("cos")].Call(v_a_457) * v_r_447) * new V(0.22d))), (v_y_459 + ((VisualJs.MathObject[new V("sin")].Call(v_a_457) * v_r_447) * new V(0.22d))));
v_ctx_444[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_444,new V("fillStyle"),"=",new V("rgba(210,250,255,.32)"));
v_ctx_444[new V("beginPath")].Call();
v_ctx_444[new V("arc")].Call(new V(0d), new V(0d), (v_r_447 * new V(0.22d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_444[new V("fill")].Call();
}
v_ctx_444[new V("restore")].Call();
return default;
}
private static V F_drawStandardAttackVfx(V[] args) {
V v_ctx_460 = VisualJs.Arg(args,0);
V v_sphere_461 = VisualJs.Arg(args,1);
V v_r_462 = VisualJs.Arg(args,2);
V v_time_463 = VisualJs.Arg(args,3);
V v_charge_464 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (new V(1d) - (v_sphere_461[new V("attackTimer")] / new V(1.2d)))));
if (new V(v_charge_464 < new V(0.04d)).Truth) return default;
V v_dir_465 = VisualJs.Or(v_sphere_461[new V("rotation")], () => new V(0d));
V v_reach_466 = (v_r_462 * ((new V(1.02d) + (v_charge_464 * new V(0.84d)))));
V v_color_467 = new V("#63e6ff");
v_ctx_460[new V("save")].Call();
v_ctx_460[new V("translate")].Call(v_sphere_461[new V("pos")][new V("x")], v_sphere_461[new V("pos")][new V("y")]);
v_ctx_460[new V("rotate")].Call(v_dir_465);
VisualJs.Assign(v_ctx_460,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_460,new V("globalAlpha"),"=",(new V(0.2d) + (v_charge_464 * new V(0.24d))));
VisualJs.Assign(v_ctx_460,new V("strokeStyle"),"=",v_color_467);
VisualJs.Assign(v_ctx_460,new V("lineWidth"),"=",(new V(1.35d) + (v_charge_464 * new V(0.45d))));
v_ctx_460[new V("beginPath")].Call();
v_ctx_460[new V("moveTo")].Call((v_reach_466 * new V(0.68d)), ((-v_r_462) * new V(0.18d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(0.9d)), ((-v_r_462) * new V(0.32d)), v_reach_466, ((-v_r_462) * new V(0.08d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(1.08d)), new V(0d), v_reach_466, (v_r_462 * new V(0.08d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(0.9d)), (v_r_462 * new V(0.32d)), (v_reach_466 * new V(0.68d)), (v_r_462 * new V(0.18d)));
v_ctx_460[new V("stroke")].Call();
VisualJs.Assign(v_ctx_460,new V("globalAlpha"),"=",(new V(0.34d) + (v_charge_464 * new V(0.32d))));
v_ctx_460[new V("beginPath")].Call();
v_ctx_460[new V("moveTo")].Call((v_reach_466 * new V(0.74d)), ((-v_r_462) * new V(0.13d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(0.94d)), new V(0d), (v_reach_466 * new V(1.02d)), ((-v_r_462) * new V(0.03d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(1.1d)), new V(0d), (v_reach_466 * new V(1.02d)), (v_r_462 * new V(0.03d)));
v_ctx_460[new V("quadraticCurveTo")].Call((v_reach_466 * new V(0.94d)), new V(0d), (v_reach_466 * new V(0.74d)), (v_r_462 * new V(0.13d)));
v_ctx_460[new V("stroke")].Call();
VisualJs.Assign(v_ctx_460,new V("globalAlpha"),"=",(new V(0.24d) + (v_charge_464 * new V(0.28d))));
v_ctx_460[new V("beginPath")].Call();
v_ctx_460[new V("arc")].Call(v_reach_466, new V(0d), (v_r_462 * ((new V(0.15d) + (v_charge_464 * new V(0.08d))))), ((-VisualJs.MathObject[new V("PI")]) * new V(0.72d)), (VisualJs.MathObject[new V("PI")] * new V(0.72d)));
v_ctx_460[new V("stroke")].Call();
VisualJs.Assign(v_ctx_460,new V("globalAlpha"),"=",(new V(0.72d) + (v_charge_464 * new V(0.18d))));
VisualJs.Assign(v_ctx_460,new V("fillStyle"),"=",new V("#ffffff"));
v_ctx_460[new V("beginPath")].Call();
v_ctx_460[new V("arc")].Call((v_reach_466 * new V(0.98d)), new V(0d), (v_r_462 * ((new V(0.065d) + (v_charge_464 * new V(0.035d))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_460[new V("fill")].Call();
v_ctx_460[new V("restore")].Call();
return default;
}
private static V F_drawModernSphere(V[] args) {
V v_ctx_468 = VisualJs.Arg(args,0);
V v_s_469 = VisualJs.Arg(args,1);
V v_sphere_470 = VisualJs.Arg(args,2);
V v_time_471 = VisualJs.Arg(args,3);
V v_def_472 = Visual_gameData.Scope[new V("SPHERE_TYPES")][v_sphere_470[new V("type")]];
V v_r_473 = VisualJs.MathObject[new V("max")].Call(new V(15d), VisualJs.MathObject[new V("min")].Call(new V(25d), ((v_sphere_470[new V("radius")] * new V(0.19d)) + ((VisualJs.Or(v_sphere_470[new V("visualTier")], () => new V(0d))) * new V(0.8d)))));
V v_scale_474 = (v_r_473 / new V(24d));
if (new V(v_sphere_470[new V("type")] == new V("standard")).Truth) {
V v_drawn_475 = Scope[new V("drawStandardSphereAssembly")].Call(v_ctx_468, v_s_469, v_sphere_470, v_r_473, v_time_471);
if ((!v_drawn_475).Truth) {
Scope[new V("drawGroundShadow")].Call(v_ctx_468, (v_r_473 * new V(1.05d)), (v_r_473 * new V(0.24d)), new V(4d));
v_ctx_468[new V("save")].Call();
v_ctx_468[new V("translate")].Call(v_sphere_470[new V("pos")][new V("x")], v_sphere_470[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_468,new V("strokeStyle"),"=",new V("rgba(90,220,255,0.28)"));
VisualJs.Assign(v_ctx_468,new V("lineWidth"),"=",new V(1d));
v_ctx_468[new V("beginPath")].Call();
v_ctx_468[new V("arc")].Call(new V(0d), new V(0d), (v_r_473 * new V(0.62d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_468[new V("stroke")].Call();
v_ctx_468[new V("restore")].Call();
}
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawStandardAttackVfx")].Call(v_ctx_468, v_sphere_470, v_r_473, v_time_471);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("orbital")).Truth) {
V v_orbitalRadiusScale_476 = (Visual_engineSpheres.Scope[new V("getSphereRadius")].Call(v_s_469, v_sphere_470) / VisualJs.MathObject[new V("max")].Call(new V(1d), v_sphere_470[new V("radius")]));
Visual_orbitalVisual.Scope[new V("renderOrbitalSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474, v_s_469[new V("enemies")], v_orbitalRadiusScale_476);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("sniper")).Truth) {
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_sniperVisual.Scope[new V("renderSniperSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("shotgun")).Truth) {
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_shotgunVisual.Scope[new V("renderShotgunSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("chain")).Truth) {
Visual_chainVisual.Scope[new V("renderChainSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("aura")).Truth) {
Visual_auraVisual.Scope[new V("renderAuraSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("prism")).Truth) {
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_prismVisual.Scope[new V("renderPrismSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("gravity")).Truth) {
Visual_gravityVisual.Scope[new V("renderGravitySphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("pulse")).Truth) {
Visual_pulseVisual.Scope[new V("renderPulseSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
if (new V(v_sphere_470[new V("type")] == new V("void")).Truth) {
Visual_mutationVisual.Scope[new V("renderSphereMutationVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_voidVisual.Scope[new V("renderVoidSphereRuntimeVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_elementalVisual.Scope[new V("renderSphereElementalVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Visual_modifierVisual.Scope[new V("renderSphereModifierVfx")].Call(v_ctx_468, v_sphere_470, v_s_469[new V("player")], v_time_471, v_scale_474);
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
return default;
}
Scope[new V("drawNetworkDisabledIndicator")].Call(v_ctx_468, v_sphere_470, v_time_471);
Scope[new V("drawSphereVfx")].Call(v_ctx_468, v_sphere_470, v_def_472[new V("color")], v_r_473, v_time_471);
return default;
}
private static V F_drawFormationMemory(V[] args) {
V v_ctx_477 = VisualJs.Arg(args,0);
V v_s_478 = VisualJs.Arg(args,1);
V v_memory_479 = v_s_478[new V("formationMemory")];
if (VisualJs.Or(VisualJs.Or((!v_memory_479), () => new V(v_memory_479[new V("expiresAt")] <= v_s_478[new V("time")])), () => new V(v_memory_479[new V("nodes")][new V("length")] < new V(2d))).Truth) return default;
V v_alpha_480 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (((v_memory_479[new V("expiresAt")] - v_s_478[new V("time")])) / new V(0.9d))));
V v_color_481 = (new V(v_memory_479[new V("type")] == new V("triangle")).Truth ? new V("#ffb84d") : (new V(v_memory_479[new V("type")] == new V("square")).Truth ? new V("#69b7ff") : (new V(v_memory_479[new V("type")] == new V("cluster")).Truth ? new V("#b38cff") : new V("#63e6ff"))));
v_ctx_477[new V("save")].Call();
VisualJs.Assign(v_ctx_477,new V("strokeStyle"),"=",v_color_481);
VisualJs.Assign(v_ctx_477,new V("globalAlpha"),"=",(new V(0.34d) * v_alpha_480));
VisualJs.Assign(v_ctx_477,new V("lineWidth"),"=",new V(1.4d));
VisualJs.Assign(v_ctx_477,new V("shadowColor"),"=",v_color_481);
VisualJs.Assign(v_ctx_477,new V("shadowBlur"),"=",new V(5d));
v_ctx_477[new V("setLineDash")].Call(VisualJs.Arr(new V(5d), new V(7d)));
v_ctx_477[new V("beginPath")].Call();
v_memory_479[new V("nodes")][new V("forEach")].Call(VisualJs.Fn(args_482 => { V v_node_483 = VisualJs.Arg(args_482, 0);
V v_index_484 = VisualJs.Arg(args_482, 1);
if (new V(v_index_484 == new V(0d)).Truth) v_ctx_477[new V("moveTo")].Call(v_node_483[new V("x")], v_node_483[new V("y")]); else v_ctx_477[new V("lineTo")].Call(v_node_483[new V("x")], v_node_483[new V("y")]);
return default; }));
if (VisualJs.Or(new V(v_memory_479[new V("type")] == new V("triangle")), () => new V(v_memory_479[new V("type")] == new V("square"))).Truth) v_ctx_477[new V("closePath")].Call();
v_ctx_477[new V("stroke")].Call();
foreach (V v_node_485 in v_memory_479[new V("nodes")].Items) {
VisualJs.Assign(v_ctx_477,new V("globalAlpha"),"=",(new V(0.22d) * v_alpha_480));
v_ctx_477[new V("beginPath")].Call();
v_ctx_477[new V("arc")].Call(v_node_485[new V("x")], v_node_485[new V("y")], (new V(10d) + (VisualJs.MathObject[new V("sin")].Call((v_s_478[new V("time")] * new V(7d))) * new V(1.5d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_477[new V("stroke")].Call();
}
v_ctx_477[new V("setLineDash")].Call(VisualJs.Arr());
v_ctx_477[new V("restore")].Call();
return default;
}
private static V F_drawWavyNetworkLink(V[] args) {
V v_ctx_486 = VisualJs.Arg(args,0);
V v_from_487 = VisualJs.Arg(args,1);
V v_to_488 = VisualJs.Arg(args,2);
V v_color_489 = VisualJs.Arg(args,3);
V v_alpha_490 = VisualJs.Arg(args,4);
V v_time_491 = VisualJs.Arg(args,5);
V v_seed_492 = VisualJs.Arg(args,6);
V v_active_493 = VisualJs.Arg(args,7);
V v_dx_494 = (v_to_488[new V("x")] - v_from_487[new V("x")]);
V v_dy_495 = (v_to_488[new V("y")] - v_from_487[new V("y")]);
V v_len_496 = VisualJs.Or(VisualJs.MathObject[new V("hypot")].Call(v_dx_494, v_dy_495), () => new V(1d));
V v_nx_497 = ((-v_dy_495) / v_len_496);
V v_ny_498 = (v_dx_494 / v_len_496);
V v_amp_499 = VisualJs.MathObject[new V("min")].Call(new V(6.2d), (new V(2.2d) + (v_len_496 * new V(0.008d))));
V v_steps_500 = new V(24d);
V v_phase_501 = ((v_time_491 * ((v_active_493.Truth ? new V(1.9d) : new V(1.35d)))) + v_seed_492);
V v_pointAt_502 = default; v_pointAt_502 = VisualJs.Fn(args_503 => { V v_p_504 = VisualJs.Arg(args_503, 0);
V v_q_505 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), v_p_504));
V v_wave_506 = ((VisualJs.MathObject[new V("sin")].Call((v_phase_501 + ((v_q_505 * VisualJs.MathObject[new V("PI")]) * new V(4.2d)))) * v_amp_499) * VisualJs.MathObject[new V("sin")].Call((VisualJs.MathObject[new V("PI")] * v_q_505)));
return VisualJs.Obj(new V("x"), ((v_from_487[new V("x")] + (v_dx_494 * v_q_505)) + (v_nx_497 * v_wave_506)), new V("y"), ((v_from_487[new V("y")] + (v_dy_495 * v_q_505)) + (v_ny_498 * v_wave_506))); });
V v_path_507 = default; v_path_507 = VisualJs.Fn(args_508 => { v_ctx_486[new V("beginPath")].Call();
V v_first_509 = v_pointAt_502.Call(new V(0d));
v_ctx_486[new V("moveTo")].Call(v_first_509[new V("x")], v_first_509[new V("y")]);
for (V v_i_510 = new V(1d); new V(v_i_510 <= v_steps_500).Truth; v_i_510++) {
V v_p_511 = v_pointAt_502.Call((v_i_510 / v_steps_500));
v_ctx_486[new V("lineTo")].Call(v_p_511[new V("x")], v_p_511[new V("y")]);
}
return default; });
v_ctx_486[new V("save")].Call();
VisualJs.Assign(v_ctx_486,new V("lineJoin"),"=",new V("round"));
VisualJs.Assign(v_ctx_486,new V("lineCap"),"=",new V("round"));
VisualJs.Assign(v_ctx_486,new V("globalAlpha"),"=",(v_alpha_490 * ((v_active_493.Truth ? new V(0.22d) : new V(0.12d)))));
VisualJs.Assign(v_ctx_486,new V("strokeStyle"),"=",v_color_489);
VisualJs.Assign(v_ctx_486,new V("lineWidth"),"=",(v_active_493.Truth ? new V(5.8d) : new V(3.6d)));
v_path_507.Call();
v_ctx_486[new V("stroke")].Call();
VisualJs.Assign(v_ctx_486,new V("globalAlpha"),"=",(v_alpha_490 * ((v_active_493.Truth ? new V(0.82d) : new V(0.64d)))));
VisualJs.Assign(v_ctx_486,new V("lineWidth"),"=",(v_active_493.Truth ? new V(2.55d) : new V(1.95d)));
v_path_507.Call();
v_ctx_486[new V("stroke")].Call();
VisualJs.Assign(v_ctx_486,new V("globalAlpha"),"=",(v_alpha_490 * ((v_active_493.Truth ? new V(0.72d) : new V(0.58d)))));
VisualJs.Assign(v_ctx_486,new V("lineWidth"),"=",(v_active_493.Truth ? new V(0.92d) : new V(0.7d)));
v_path_507.Call();
v_ctx_486[new V("stroke")].Call();
V v_packetCount_512 = new V(3d);
for (V v_packet_513 = new V(0d); new V(v_packet_513 < v_packetCount_512).Truth; v_packet_513++) {
V v_raw_514 = ((((((((v_time_491 * ((v_active_493.Truth ? new V(0.34d) : new V(0.27d)))) + (v_seed_492 * new V(0.027d))) + (v_packet_513 / v_packetCount_512))) % new V(1d)) + new V(1d))) % new V(1d));
V v_p_515 = (new V((v_packet_513 % new V(2d)) == new V(0d)).Truth ? v_raw_514 : (new V(1d) - v_raw_514));
V v_tailP_516 = VisualJs.MathObject[new V("max")].Call(new V(0d), (v_p_515 - ((v_active_493.Truth ? new V(0.075d) : new V(0.06d)))));
V v_head_517 = v_pointAt_502.Call(v_p_515);
V v_tail_518 = v_pointAt_502.Call(v_tailP_516);
V v_angle_519 = VisualJs.MathObject[new V("atan2")].Call((v_head_517[new V("y")] - v_tail_518[new V("y")]), (v_head_517[new V("x")] - v_tail_518[new V("x")]));
v_ctx_486[new V("save")].Call();
v_ctx_486[new V("translate")].Call(v_head_517[new V("x")], v_head_517[new V("y")]);
v_ctx_486[new V("rotate")].Call(v_angle_519);
VisualJs.Assign(v_ctx_486,new V("globalAlpha"),"=",(((v_active_493.Truth ? new V(0.86d) : new V(0.62d))) * v_alpha_490));
VisualJs.Assign(v_ctx_486,new V("fillStyle"),"=",new V("#f8ffff"));
v_ctx_486[new V("beginPath")].Call();
v_ctx_486[new V("moveTo")].Call(new V(7.5d), new V(0d));
v_ctx_486[new V("lineTo")].Call((-new V(2d)), (-new V(3d)));
v_ctx_486[new V("lineTo")].Call((-new V(5.2d)), new V(0d));
v_ctx_486[new V("lineTo")].Call((-new V(2d)), new V(3d));
v_ctx_486[new V("closePath")].Call();
v_ctx_486[new V("fill")].Call();
VisualJs.Assign(v_ctx_486,new V("globalAlpha"),"=",(((v_active_493.Truth ? new V(0.48d) : new V(0.34d))) * v_alpha_490));
VisualJs.Assign(v_ctx_486,new V("fillStyle"),"=",v_color_489);
v_ctx_486[new V("beginPath")].Call();
v_ctx_486[new V("moveTo")].Call((-new V(1.5d)), new V(0d));
v_ctx_486[new V("lineTo")].Call((-new V(10.5d)), (-new V(1.7d)));
v_ctx_486[new V("lineTo")].Call((-new V(15d)), new V(0d));
v_ctx_486[new V("lineTo")].Call((-new V(10.5d)), new V(1.7d));
v_ctx_486[new V("closePath")].Call();
v_ctx_486[new V("fill")].Call();
v_ctx_486[new V("restore")].Call();
}
v_ctx_486[new V("restore")].Call();
return default;
}
private static V F_drawSphereNetwork(V[] args) {
V v_ctx_520 = VisualJs.Arg(args,0);
V v_s_521 = VisualJs.Arg(args,1);
V v_sourceNetwork_522 = VisualJs.Arg(args,2);
V v_runtimeNodes_523 = Visual_networkRuntime.Scope[new V("buildRuntimeNetworkNodes")].Call(v_s_521[new V("spheres")], v_s_521[new V("minions")], new V((VisualJs.Or(v_s_521[new V("player")][new V("abilities")][new V("minion")], () => new V(0d))) >= new V(3d)));
V binding_524 = Visual_networkRender.Scope[new V("getSphereVisualNetwork")].Call(v_sourceNetwork_522, v_s_521[new V("spheres")][new V("length")]);
V v_network_525 = binding_524[new V("network")];
V v_externalLinks_526 = binding_524[new V("externalLinks")];

if (VisualJs.And(new V(v_network_525[new V("nodes")][new V("length")] < new V(2d)), () => new V(v_externalLinks_526[new V("length")] == new V(0d))).Truth) return default;
foreach (V v_link_527 in v_externalLinks_526.Items) {
V v_a_528 = VisualJs.Chain(v_runtimeNodes_523[v_link_527[new V("a")]],optional_529=>optional_529[new V("pos")]);
V v_b_530 = VisualJs.Chain(v_runtimeNodes_523[v_link_527[new V("b")]],optional_531=>optional_531[new V("pos")]);
if (VisualJs.Or((!v_a_528), () => (!v_b_530)).Truth) continue;
Scope[new V("drawWavyNetworkLink")].Call(v_ctx_520, v_a_528, v_b_530, new V("#63b9ff"), new V(0.24d), v_s_521[new V("time")], ((v_link_527[new V("a")] * new V(17.13d)) + (v_link_527[new V("b")] * new V(29.71d))), new V(false));
}
V v_t_532 = v_s_521[new V("time")];
V v_pulse_533 = (new V(0.82d) + (VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(4.2d))) * new V(0.1d)));
V v_hasNode_534 = default; v_hasNode_534 = VisualJs.Fn(args_535 => { V v_shape_536 = VisualJs.Arg(args_535, 0);
V v_index_537 = VisualJs.Arg(args_535, 1);
return new V(VisualJs.Chain(v_shape_536,optional_538=>optional_538[new V("nodes")][new V("includes")].Call(v_index_537)).Truth); });
V v_drawFormationTag_539 = default; v_drawFormationTag_539 = VisualJs.Fn(args_540 => { V v_x_541 = VisualJs.Arg(args_540, 0);
V v_y_542 = VisualJs.Arg(args_540, 1);
V v_label_543 = VisualJs.Arg(args_540, 2);
V v_color_544 = VisualJs.Arg(args_540, 3);
V v_width_545 = VisualJs.Arg(args_540,4).Undefined?new V(96d):VisualJs.Arg(args_540,4);
v_ctx_520[new V("save")].Call();
v_ctx_520[new V("translate")].Call(v_x_541, v_y_542);
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("rgba(3,8,18,0.86)"));
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",v_color_544);
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.92d));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",v_color_544);
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(0d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("roundRect")].Call(((-v_width_545) / new V(2d)), (-new V(10d)), v_width_545, new V(20d), new V(7d));
v_ctx_520[new V("fill")].Call();
v_ctx_520[new V("stroke")].Call();
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#eef9ff"));
VisualJs.Assign(v_ctx_520,new V("font"),"=",new V("bold 8px system-ui, sans-serif"));
VisualJs.Assign(v_ctx_520,new V("textAlign"),"=",new V("center"));
VisualJs.Assign(v_ctx_520,new V("textBaseline"),"=",new V("middle"));
v_ctx_520[new V("fillText")].Call(v_label_543, new V(0d), new V(0.5d));
v_ctx_520[new V("restore")].Call();
return default; });
foreach (V v_link_546 in v_network_525[new V("links")].Items) {
V v_lineNode_547 = VisualJs.And(VisualJs.And(new V(VisualJs.Chain(v_network_525[new V("line")],optional_548=>optional_548[new V("active")]) != new V(false)), () => v_hasNode_534.Call(v_network_525[new V("line")], v_link_546[new V("a")])), () => v_hasNode_534.Call(v_network_525[new V("line")], v_link_546[new V("b")]));
V v_triangleNode_549 = VisualJs.And(VisualJs.And(new V(VisualJs.Chain(v_network_525[new V("triangle")],optional_550=>optional_550[new V("active")]) != new V(false)), () => v_hasNode_534.Call(v_network_525[new V("triangle")], v_link_546[new V("a")])), () => v_hasNode_534.Call(v_network_525[new V("triangle")], v_link_546[new V("b")]));
V v_clusterNode_551 = VisualJs.And(v_hasNode_534.Call(v_network_525[new V("cluster")], v_link_546[new V("a")]), () => v_hasNode_534.Call(v_network_525[new V("cluster")], v_link_546[new V("b")]));
V v_squareNode_552 = VisualJs.And(v_hasNode_534.Call(v_network_525[new V("square")], v_link_546[new V("a")]), () => v_hasNode_534.Call(v_network_525[new V("square")], v_link_546[new V("b")]));
V v_alpha_553 = new V(0.27d);
V v_color_554 = new V("#63b9ff");
V v_active_555 = new V(false);
if (v_lineNode_547.Truth) {
v_alpha_553 = new V(0.5d);
v_color_554 = new V("#63e6ff");
v_active_555 = new V(true);
} else if (v_squareNode_552.Truth) {
v_alpha_553 = new V(0.48d);
v_color_554 = new V("#69b7ff");
v_active_555 = new V(true);
} else if (v_triangleNode_549.Truth) {
v_alpha_553 = new V(0.54d);
v_color_554 = new V("#ffb84d");
v_active_555 = new V(true);
} else if (v_clusterNode_551.Truth) {
v_alpha_553 = new V(0.44d);
v_color_554 = new V("#b38cff");
v_active_555 = new V(true);
}
V v_a_556 = v_s_521[new V("spheres")][v_link_546[new V("a")]][new V("pos")];
V v_b_557 = v_s_521[new V("spheres")][v_link_546[new V("b")]][new V("pos")];
V v_seed_558 = ((v_link_546[new V("a")] * new V(17.13d)) + (v_link_546[new V("b")] * new V(29.71d)));
Scope[new V("drawWavyNetworkLink")].Call(v_ctx_520, v_a_556, v_b_557, v_color_554, (v_alpha_553 * v_pulse_533), v_t_532, v_seed_558, v_active_555);
}
if (v_network_525[new V("line")].Truth) {
V v_cx_559 = new V(0d);
V v_cy_560 = new V(0d);
foreach (V v_index_561 in v_network_525[new V("line")][new V("nodes")].Items) {
v_cx_559 += v_s_521[new V("spheres")][v_index_561][new V("pos")][new V("x")];
v_cy_560 += v_s_521[new V("spheres")][v_index_561][new V("pos")][new V("y")];
}
v_cx_559 /= v_network_525[new V("line")][new V("nodes")][new V("length")];
v_cy_560 /= v_network_525[new V("line")][new V("nodes")][new V("length")];
v_ctx_520[new V("save")].Call();
v_ctx_520[new V("translate")].Call(v_cx_559, v_cy_560);
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#63e6ff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.2d) + (new V(0.05d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(5d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#63e6ff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(4d));
V v_spread_562 = (new V(34d) + (v_network_525[new V("line")][new V("nodes")][new V("length")] * new V(5d)));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("moveTo")].Call((-v_spread_562), new V(0d));
v_ctx_520[new V("lineTo")].Call(v_spread_562, new V(0d));
v_ctx_520[new V("stroke")].Call();
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("triangle")].Truth) {
V v_points_563 = v_network_525[new V("triangle")][new V("nodes")][new V("map")].Call(VisualJs.Fn(args_564 => { V v_index_565 = VisualJs.Arg(args_564, 0);
return v_s_521[new V("spheres")][v_index_565][new V("pos")]; }));
V v_triangleAlpha_566 = (new V(v_network_525[new V("triangle")][new V("active")] == new V(false)).Truth ? new V(0.3d) : new V(1d));
v_ctx_520[new V("save")].Call();
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(((new V(0.028d) + (new V(0.018d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(3d)))))) * v_triangleAlpha_566));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("moveTo")].Call(v_points_563[new V(0d)][new V("x")], v_points_563[new V(0d)][new V("y")]);
v_ctx_520[new V("lineTo")].Call(v_points_563[new V(1d)][new V("x")], v_points_563[new V(1d)][new V("y")]);
v_ctx_520[new V("lineTo")].Call(v_points_563[new V(2d)][new V("x")], v_points_563[new V(2d)][new V("y")]);
v_ctx_520[new V("closePath")].Call();
v_ctx_520[new V("fill")].Call();
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#ffcf73"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(((new V(0.24d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(4d)))))) * v_triangleAlpha_566));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(4d));
v_ctx_520[new V("stroke")].Call();
v_ctx_520[new V("restore")].Call();
V v_cx_567 = new V(0d);
V v_cy_568 = new V(0d);
V v_totalCharge_569 = new V(0d);
V v_activePulse_570 = new V(false);
foreach (V v_index_571 in v_network_525[new V("triangle")][new V("nodes")].Items) {
V v_sphere_572 = v_s_521[new V("spheres")][v_index_571];
v_cx_567 += v_sphere_572[new V("pos")][new V("x")];
v_cy_568 += v_sphere_572[new V("pos")][new V("y")];
v_totalCharge_569 += (v_sphere_572[new V("formationHitCount")] % new V(3d));
v_activePulse_570 = VisualJs.Or(v_activePulse_570, () => new V(v_sphere_572[new V("resonancePulseTimer")] > new V(0d)));
}
v_cx_567 /= v_network_525[new V("triangle")][new V("nodes")][new V("length")];
v_cy_568 /= v_network_525[new V("triangle")][new V("nodes")][new V("length")];
V v_averageCharge_573 = VisualJs.MathObject[new V("round")].Call((v_totalCharge_569 / v_network_525[new V("triangle")][new V("nodes")][new V("length")]));
foreach (V v_index_574 in v_network_525[new V("triangle")][new V("nodes")].Items) {
V v_sphere_575 = v_s_521[new V("spheres")][v_index_574];
V v_charge_576 = (v_sphere_575[new V("formationHitCount")] % new V(3d));
V v_progress_577 = (v_charge_576 / new V(3d));
V v_radius_578 = (new V(25d) + (VisualJs.MathObject[new V("sin")].Call(((v_t_532 * new V(5d)) + v_index_574)) * new V(1.5d)));
v_ctx_520[new V("save")].Call();
v_ctx_520[new V("translate")].Call(v_sphere_575[new V("pos")][new V("x")], v_sphere_575[new V("pos")][new V("y")]);
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1.4d));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.2d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), v_radius_578, ((-VisualJs.MathObject[new V("PI")]) / new V(2d)), (VisualJs.MathObject[new V("PI")] * new V(1.5d)));
v_ctx_520[new V("stroke")].Call();
if (new V(v_charge_576 > new V(0d)).Truth) {
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.86d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(4d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), v_radius_578, ((-VisualJs.MathObject[new V("PI")]) / new V(2d)), (((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + ((VisualJs.MathObject[new V("PI")] * new V(2d)) * v_progress_577)));
v_ctx_520[new V("stroke")].Call();
}
if (new V(v_sphere_575[new V("resonancePulseTimer")] > new V(0d)).Truth) {
V v_flash_579 = (v_sphere_575[new V("resonancePulseTimer")] / new V(0.45d));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.45d) + (new V(0.4d) * v_flash_579)));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#fff0c2"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(6d));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(2.6d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), ((v_radius_578 + new V(5d)) + (((new V(1d) - v_flash_579)) * new V(10d))), ((-VisualJs.MathObject[new V("PI")]) / new V(2d)), (VisualJs.MathObject[new V("PI")] * new V(1.5d)));
v_ctx_520[new V("stroke")].Call();
}
for (V v_tick_580 = new V(0d); new V(v_tick_580 < new V(3d)).Truth; v_tick_580++) {
V v_a_581 = (((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + ((((v_tick_580 / new V(3d))) * VisualJs.MathObject[new V("PI")]) * new V(2d)));
V v_inner_582 = (v_radius_578 + new V(2d));
V v_outer_583 = (v_inner_582 + ((new V(v_tick_580 < v_charge_576).Truth ? new V(5d) : new V(2d))));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(v_tick_580 < v_charge_576).Truth ? new V(0.95d) : new V(0.3d)));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("moveTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_581) * v_inner_582), (VisualJs.MathObject[new V("sin")].Call(v_a_581) * v_inner_582));
v_ctx_520[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_581) * v_outer_583), (VisualJs.MathObject[new V("sin")].Call(v_a_581) * v_outer_583));
v_ctx_520[new V("stroke")].Call();
}
v_ctx_520[new V("restore")].Call();
}
v_ctx_520[new V("save")].Call();
v_ctx_520[new V("translate")].Call(v_cx_567, v_cy_568);
V v_corePulse_584 = (new V(1d) + ((v_activePulse_570.Truth ? (new V(0.28d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(24d))))) : (new V(0.04d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(6d)))))));
v_ctx_520[new V("scale")].Call(v_corePulse_584, v_corePulse_584);
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#ffb84d"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(v_activePulse_570.Truth ? new V(0.9d) : new V(0.26d)));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",(v_activePulse_570.Truth ? new V("#fff0c2") : new V("#ffb84d")));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(7d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), (v_activePulse_570.Truth ? new V(4.5d) : new V(2.5d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("fill")].Call();
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("square")].Truth) {
V v_points_585 = v_network_525[new V("square")][new V("nodes")][new V("map")].Call(VisualJs.Fn(args_586 => { V v_index_587 = VisualJs.Arg(args_586, 0);
return v_s_521[new V("spheres")][v_index_587][new V("pos")]; }));
V v_cx_588 = new V(0d);
V v_cy_589 = new V(0d);
foreach (V v_p_590 in v_points_585.Items) {
v_cx_588 += v_p_590[new V("x")];
v_cy_589 += v_p_590[new V("y")];
}
v_cx_588 /= v_points_585[new V("length")];
v_cy_589 /= v_points_585[new V("length")];
v_ctx_520[new V("save")].Call();
V v_shieldPulse_591 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(4.8d))) * new V(0.035d)));
v_ctx_520[new V("translate")].Call(v_cx_588, v_cy_589);
v_ctx_520[new V("scale")].Call(v_shieldPulse_591, v_shieldPulse_591);
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#69b7ff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.28d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(4d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1.8d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#69b7ff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(4d));
v_ctx_520[new V("beginPath")].Call();
v_points_585[new V("forEach")].Call(VisualJs.Fn(args_592 => { V v_p_593 = VisualJs.Arg(args_592, 0);
V v_i_594 = VisualJs.Arg(args_592, 1);
V v_x_595 = (v_p_593[new V("x")] - v_cx_588);
V v_y_596 = (v_p_593[new V("y")] - v_cy_589);
if (new V(v_i_594 == new V(0d)).Truth) v_ctx_520[new V("moveTo")].Call(v_x_595, v_y_596); else v_ctx_520[new V("lineTo")].Call(v_x_595, v_y_596);
return default; }));
v_ctx_520[new V("closePath")].Call();
v_ctx_520[new V("stroke")].Call();
V v_radius_597 = (VisualJs.MathObject[new V("max")].Call(VisualJs.Concat(v_points_585[new V("map")].Call(VisualJs.Fn(args_598 => { V v_p_599 = VisualJs.Arg(args_598, 0);
return VisualJs.MathObject[new V("hypot")].Call((v_p_599[new V("x")] - v_cx_588), (v_p_599[new V("y")] - v_cy_589)); }))).Items.ToArray()) + new V(22d));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.16d) + (new V(0.05d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(6d))))));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#69b7ff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(6d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), v_radius_597, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("stroke")].Call();
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#69b7ff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.3d) + (new V(0.1d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(8d))))));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#69b7ff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(6d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(new V(0d), new V(0d), new V(4.5d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("fill")].Call();
for (V v_i_600 = new V(0d); new V(v_i_600 < new V(4d)).Truth; v_i_600++) {
V v_a_601 = ((v_t_532 * new V(0.8d)) + ((v_i_600 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
V v_x_602 = (VisualJs.MathObject[new V("cos")].Call(v_a_601) * v_radius_597);
V v_y_603 = (VisualJs.MathObject[new V("sin")].Call(v_a_601) * v_radius_597);
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.65d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(v_x_602, v_y_603, new V(2.2d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("fill")].Call();
}
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("cluster")].Truth) {
V v_cx_604 = new V(0d);
V v_cy_605 = new V(0d);
foreach (V v_index_606 in v_network_525[new V("cluster")][new V("nodes")].Items) {
v_cx_604 += v_s_521[new V("spheres")][v_index_606][new V("pos")][new V("x")];
v_cy_605 += v_s_521[new V("spheres")][v_index_606][new V("pos")][new V("y")];
}
v_cx_604 /= v_network_525[new V("cluster")][new V("nodes")][new V("length")];
v_cy_605 /= v_network_525[new V("cluster")][new V("nodes")][new V("length")];
V v_radius_607 = new V(0d);
foreach (V v_index_608 in v_network_525[new V("cluster")][new V("nodes")].Items) {
v_radius_607 = VisualJs.MathObject[new V("max")].Call(v_radius_607, VisualJs.MathObject[new V("hypot")].Call((v_s_521[new V("spheres")][v_index_608][new V("pos")][new V("x")] - v_cx_604), (v_s_521[new V("spheres")][v_index_608][new V("pos")][new V("y")] - v_cy_605)));
}
v_ctx_520[new V("save")].Call();
V v_clusterPulse_609 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(3.4d))) * new V(0.035d)));
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#b38cff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.2d) + (new V(0.07d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(3.2d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1.4d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#b38cff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(5d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(v_cx_604, v_cy_605, ((v_radius_607 + new V(18d)) + (VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(3d))) * new V(3d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("stroke")].Call();
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("rgba(206,178,255,0.42)"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.55d));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(0.8d));
foreach (V v_index_610 in v_network_525[new V("cluster")][new V("nodes")].Items) {
V v_p_611 = v_s_521[new V("spheres")][v_index_610][new V("pos")];
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("moveTo")].Call(v_cx_604, v_cy_605);
v_ctx_520[new V("lineTo")].Call(v_p_611[new V("x")], v_p_611[new V("y")]);
v_ctx_520[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#b38cff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.42d) + (new V(0.1d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(7d))))));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#b38cff"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(6d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(v_cx_604, v_cy_605, (new V(4.2d) * v_clusterPulse_609), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("fill")].Call();
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("ring")].Truth) {
V v_points_612 = v_network_525[new V("ring")][new V("nodes")][new V("map")].Call(VisualJs.Fn(args_613 => { V v_index_614 = VisualJs.Arg(args_613, 0);
return v_s_521[new V("spheres")][v_index_614][new V("pos")]; }));
v_ctx_520[new V("save")].Call();
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#55e69a"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.24d) + (new V(0.08d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(5d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1.6d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#55e69a"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(0d));
v_ctx_520[new V("beginPath")].Call();
v_points_612[new V("forEach")].Call(VisualJs.Fn(args_615 => { V v_p_616 = VisualJs.Arg(args_615, 0);
V v_i_617 = VisualJs.Arg(args_615, 1);
return (new V(v_i_617 == new V(0d)).Truth ? v_ctx_520[new V("moveTo")].Call(v_p_616[new V("x")], v_p_616[new V("y")]) : v_ctx_520[new V("lineTo")].Call(v_p_616[new V("x")], v_p_616[new V("y")])); }));
v_ctx_520[new V("closePath")].Call();
v_ctx_520[new V("stroke")].Call();
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("lattice")].Truth) {
V v_cx_618 = new V(0d);
V v_cy_619 = new V(0d);
foreach (V v_index_620 in v_network_525[new V("lattice")][new V("nodes")].Items) {
v_cx_618 += v_s_521[new V("spheres")][v_index_620][new V("pos")][new V("x")];
v_cy_619 += v_s_521[new V("spheres")][v_index_620][new V("pos")][new V("y")];
}
v_cx_618 /= v_network_525[new V("lattice")][new V("nodes")][new V("length")];
v_cy_619 /= v_network_525[new V("lattice")][new V("nodes")][new V("length")];
v_ctx_520[new V("save")].Call();
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#39d8ff"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.2d) + (new V(0.06d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(6d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1d));
foreach (V v_index_621 in v_network_525[new V("lattice")][new V("nodes")].Items) {
V v_p_622 = v_s_521[new V("spheres")][v_index_621][new V("pos")];
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("moveTo")].Call(v_cx_618, v_cy_619);
v_ctx_520[new V("lineTo")].Call(v_p_622[new V("x")], v_p_622[new V("y")]);
v_ctx_520[new V("stroke")].Call();
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",new V(0.62d));
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(v_p_622[new V("x")], v_p_622[new V("y")], (new V(2.8d) + (VisualJs.MathObject[new V("sin")].Call(((v_t_532 * new V(7d)) + v_index_621)) * new V(0.7d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
VisualJs.Assign(v_ctx_520,new V("fillStyle"),"=",new V("#39d8ff"));
v_ctx_520[new V("fill")].Call();
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.2d) + (new V(0.06d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(6d))))));
}
v_ctx_520[new V("restore")].Call();
}
if (v_network_525[new V("fractal")].Truth) {
V v_points_623 = v_network_525[new V("fractal")][new V("nodes")][new V("map")].Call(VisualJs.Fn(args_624 => { V v_index_625 = VisualJs.Arg(args_624, 0);
return v_s_521[new V("spheres")][v_index_625][new V("pos")]; }));
V v_cx_626 = new V(0d);
V v_cy_627 = new V(0d);
foreach (V v_p_628 in v_points_623.Items) {
v_cx_626 += v_p_628[new V("x")];
v_cy_627 += v_p_628[new V("y")];
}
v_cx_626 /= v_points_623[new V("length")];
v_cy_627 /= v_points_623[new V("length")];
V v_radius_629 = new V(0d);
foreach (V v_p_630 in v_points_623.Items) v_radius_629 = VisualJs.MathObject[new V("max")].Call(v_radius_629, VisualJs.MathObject[new V("hypot")].Call((v_p_630[new V("x")] - v_cx_626), (v_p_630[new V("y")] - v_cy_627)));
v_ctx_520[new V("save")].Call();
VisualJs.Assign(v_ctx_520,new V("strokeStyle"),"=",new V("#ff6b9d"));
VisualJs.Assign(v_ctx_520,new V("globalAlpha"),"=",(new V(0.24d) + (new V(0.1d) * VisualJs.MathObject[new V("sin")].Call((v_t_532 * new V(8d))))));
VisualJs.Assign(v_ctx_520,new V("lineWidth"),"=",new V(1.2d));
VisualJs.Assign(v_ctx_520,new V("shadowColor"),"=",new V("#ff6b9d"));
VisualJs.Assign(v_ctx_520,new V("shadowBlur"),"=",new V(0d));
foreach (V v_scale_631 in VisualJs.Arr(new V(1d), new V(0.62d), new V(0.34d)).Items) {
v_ctx_520[new V("beginPath")].Call();
v_ctx_520[new V("arc")].Call(v_cx_626, v_cy_627, VisualJs.MathObject[new V("max")].Call(new V(8d), (v_radius_629 * v_scale_631)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_520[new V("stroke")].Call();
}
v_ctx_520[new V("restore")].Call();
}
return default;
}
private static V F_drawBossAttackTelegraph(V[] args) {
V v_ctx_632 = VisualJs.Arg(args,0);
V v_e_633 = VisualJs.Arg(args,1);
V v_playerPos_634 = VisualJs.Arg(args,2);
V v_t_635 = VisualJs.Arg(args,3);
if ((!v_e_633[new V("isBoss")]).Truth) return default;
V v_r_636 = v_e_633[new V("radius")];
V v_pulse_637 = (new V(0.55d) + (VisualJs.MathObject[new V("sin")].Call((v_t_635 * new V(14d))) * new V(0.16d)));
v_ctx_632[new V("save")].Call();
if (VisualJs.And(new V(v_e_633[new V("bossType")] == new V("charger")), () => v_e_633[new V("isCharging")]).Truth) {
V v_windup_638 = new V(v_e_633[new V("chargeTimer")] > new V(0.35d));
V v_reach_639 = (v_windup_638.Truth ? (v_r_636 * new V(2.7d)) : (v_r_636 * new V(4.2d)));
VisualJs.Assign(v_ctx_632,new V("strokeStyle"),"=",(v_windup_638.Truth ? (new V("rgba(255,184,77,") + (new V(0.58d) + (v_pulse_637 * new V(0.25d))) + new V(")")) : (new V("rgba(255,98,87,") + (new V(0.7d) + (v_pulse_637 * new V(0.2d))) + new V(")"))));
VisualJs.Assign(v_ctx_632,new V("lineWidth"),"=",(v_windup_638.Truth ? VisualJs.MathObject[new V("max")].Call(new V(2d), (v_r_636 * new V(0.025d))) : VisualJs.MathObject[new V("max")].Call(new V(3d), (v_r_636 * new V(0.04d)))));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_632[new V("lineTo")].Call((v_e_633[new V("chargeDir")][new V("x")] * v_reach_639), (v_e_633[new V("chargeDir")][new V("y")] * v_reach_639));
v_ctx_632[new V("stroke")].Call();
if (v_windup_638.Truth) {
V v_nx_640 = (-v_e_633[new V("chargeDir")][new V("y")]);
V v_ny_641 = v_e_633[new V("chargeDir")][new V("x")];
V v_tip_642 = (v_r_636 * new V(2.05d));
V v_half_643 = (v_r_636 * new V(0.55d));
VisualJs.Assign(v_ctx_632,new V("fillStyle"),"=",(new V("rgba(255,184,77,") + (new V(0.08d) + (v_pulse_637 * new V(0.05d))) + new V(")")));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("moveTo")].Call(((v_e_633[new V("chargeDir")][new V("x")] * v_r_636) * new V(0.82d)), ((v_e_633[new V("chargeDir")][new V("y")] * v_r_636) * new V(0.82d)));
v_ctx_632[new V("lineTo")].Call(((v_e_633[new V("chargeDir")][new V("x")] * v_tip_642) + (v_nx_640 * v_half_643)), ((v_e_633[new V("chargeDir")][new V("y")] * v_tip_642) + (v_ny_641 * v_half_643)));
v_ctx_632[new V("lineTo")].Call(((v_e_633[new V("chargeDir")][new V("x")] * v_tip_642) - (v_nx_640 * v_half_643)), ((v_e_633[new V("chargeDir")][new V("y")] * v_tip_642) - (v_ny_641 * v_half_643)));
v_ctx_632[new V("closePath")].Call();
v_ctx_632[new V("fill")].Call();
}
}
if (VisualJs.And(new V(v_e_633[new V("bossType")] == new V("shooter")), () => new V(v_e_633[new V("bossShootTimer")] <= Visual_bossBalance.Scope[new V("BOSS_TELEGRAPH_WINDOWS")][new V("shooter")])).Truth) {
V v_dx_644 = (v_playerPos_634[new V("x")] - v_e_633[new V("pos")][new V("x")]);
V v_dy_645 = (v_playerPos_634[new V("y")] - v_e_633[new V("pos")][new V("y")]);
V v_angle_646 = VisualJs.MathObject[new V("atan2")].Call(v_dy_645, v_dx_644);
VisualJs.Assign(v_ctx_632,new V("strokeStyle"),"=",(new V("rgba(255,106,95,") + (new V(0.46d) + (v_pulse_637 * new V(0.22d))) + new V(")")));
VisualJs.Assign(v_ctx_632,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1.5d), (v_r_636 * new V(0.018d))));
for (V v_k_647 = (-new V(1d)); new V(v_k_647 <= new V(1d)).Truth; v_k_647++) {
V v_a_648 = (v_angle_646 + (v_k_647 * new V(0.11d)));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_648) * v_r_636) * new V(1.05d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_648) * v_r_636) * new V(1.05d)));
v_ctx_632[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_648) * v_r_636) * new V(4.8d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_648) * v_r_636) * new V(4.8d)));
v_ctx_632[new V("stroke")].Call();
}
}
if (VisualJs.And(new V(v_e_633[new V("bossType")] == new V("summoner")), () => new V(v_e_633[new V("summonTimer")] <= Visual_bossBalance.Scope[new V("BOSS_TELEGRAPH_WINDOWS")][new V("summoner")])).Truth) {
V v_progress_649 = (new V(1d) - (VisualJs.MathObject[new V("max")].Call(new V(0d), v_e_633[new V("summonTimer")]) / Visual_bossBalance.Scope[new V("BOSS_TELEGRAPH_WINDOWS")][new V("summoner")]));
V v_ringRadius_650 = (v_r_636 * ((new V(1.15d) + (v_progress_649 * new V(2.2d)))));
VisualJs.Assign(v_ctx_632,new V("strokeStyle"),"=",(new V("rgba(162,124,255,") + (new V(0.34d) + (v_pulse_637 * new V(0.18d))) + new V(")")));
VisualJs.Assign(v_ctx_632,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1.5d), (v_r_636 * new V(0.02d))));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("arc")].Call(new V(0d), new V(0d), v_ringRadius_650, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_632[new V("stroke")].Call();
for (V v_i_651 = new V(0d); new V(v_i_651 < new V(3d)).Truth; v_i_651++) {
V v_a_652 = ((v_t_635 * new V(2.2d)) + ((((v_i_651 / new V(3d))) * VisualJs.MathObject[new V("PI")]) * new V(2d)));
V v_nx_653 = (VisualJs.MathObject[new V("cos")].Call(v_a_652) * ((v_r_636 * ((new V(0.95d) + (v_progress_649 * new V(0.8d)))))));
V v_ny_654 = (VisualJs.MathObject[new V("sin")].Call(v_a_652) * ((v_r_636 * ((new V(0.95d) + (v_progress_649 * new V(0.8d)))))));
VisualJs.Assign(v_ctx_632,new V("fillStyle"),"=",(new V("rgba(201,181,255,") + (new V(0.48d) + (v_pulse_637 * new V(0.18d))) + new V(")")));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("arc")].Call(v_nx_653, v_ny_654, VisualJs.MathObject[new V("max")].Call(new V(2d), (v_r_636 * new V(0.045d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_632[new V("fill")].Call();
}
}
if (VisualJs.And(new V(v_e_633[new V("bossType")] == new V("aura")), () => new V(v_e_633[new V("bossShootTimer")] <= Visual_bossBalance.Scope[new V("BOSS_TELEGRAPH_WINDOWS")][new V("aura")])).Truth) {
V v_ringRadius_655 = (v_e_633[new V("auraRadius")] + (v_r_636 * ((new V(0.12d) + ((new V(0.1d) + (v_pulse_637 * new V(0.05d))))))));
VisualJs.Assign(v_ctx_632,new V("strokeStyle"),"=",(new V("rgba(255,98,185,") + (new V(0.42d) + (v_pulse_637 * new V(0.2d))) + new V(")")));
VisualJs.Assign(v_ctx_632,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1.5d), (v_r_636 * new V(0.02d))));
v_ctx_632[new V("beginPath")].Call();
v_ctx_632[new V("arc")].Call(new V(0d), new V(0d), v_ringRadius_655, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_632[new V("stroke")].Call();
}
v_ctx_632[new V("restore")].Call();
return default;
}
private static V F_drawModernEnemy(V[] args) {
V v_ctx_656 = VisualJs.Arg(args,0);
V v_e_657 = VisualJs.Arg(args,1);
V v_playerPos_658 = VisualJs.Arg(args,2);
V v_t_659 = Scope[new V("RENDER_TIME")];
V v_facing_660 = Scope[new V("getEnemyFacingAngle")].Call(v_e_657);
V v_color_661 = (new V(v_e_657[new V("freezeTimer")] > new V(0d)).Truth ? new V("#69d6ff") : v_e_657[new V("color")]);
V v_hit_662 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (v_e_657[new V("hitFlash")] / new V(0.15d))));
v_ctx_656[new V("save")].Call();
v_ctx_656[new V("translate")].Call(v_e_657[new V("pos")][new V("x")], v_e_657[new V("pos")][new V("y")]);
v_ctx_656[new V("rotate")].Call(v_facing_660);
V v_pulse_663 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call(((v_t_659 * new V(5d)) + (v_e_657[new V("pos")][new V("x")] * new V(0.01d)))) * new V(0.025d)));
v_ctx_656[new V("scale")].Call((v_pulse_663 + (v_hit_662 * new V(0.05d))), (v_pulse_663 + (v_hit_662 * new V(0.05d))));
Scope[new V("glowCircle")].Call(v_ctx_656, (v_e_657[new V("radius")] * ((v_e_657[new V("isBoss")].Truth ? new V(1.65d) : new V(1.15d)))), v_color_661, (v_e_657[new V("isBoss")].Truth ? new V(0.21d) : new V(0.1d)));
if (v_e_657[new V("isBoss")].Truth) {
Scope[new V("drawBossAttackTelegraph")].Call(v_ctx_656, v_e_657, v_playerPos_658, v_t_659);
Visual_enemyVisual.Scope[new V("drawBossCreature")].Call(v_ctx_656, v_e_657, v_t_659);
} else {
Visual_enemyVisual.Scope[new V("drawEnemyCreature")].Call(v_ctx_656, v_e_657, v_t_659);
if (v_e_657[new V("isElite")].Truth) Scope[new V("drawEliteCrest")].Call(v_ctx_656, v_e_657, v_t_659);
if (new V(v_e_657[new V("tier")] > new V(0d)).Truth) {
v_ctx_656[new V("save")].Call();
v_ctx_656[new V("translate")].Call(new V(0d), ((-v_e_657[new V("radius")]) * new V(1.55d)));
v_ctx_656[new V("rotate")].Call((v_t_659 * new V(0.35d)));
VisualJs.Assign(v_ctx_656,new V("strokeStyle"),"=",(new V("rgba(") + Scope[new V("hexToRgb")].Call(v_color_661) + new V(",.46)")));
VisualJs.Assign(v_ctx_656,new V("lineWidth"),"=",new V(1.2d));
V v_n_664 = VisualJs.MathObject[new V("min")].Call(new V(3d), v_e_657[new V("tier")]);
for (V v_i_665 = new V(0d); new V(v_i_665 < v_n_664).Truth; v_i_665++) {
V v_a_666 = (((v_i_665 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / v_n_664);
v_ctx_656[new V("beginPath")].Call();
v_ctx_656[new V("moveTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_666) * v_e_657[new V("radius")]) * new V(0.1d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_666) * v_e_657[new V("radius")]) * new V(0.1d)));
v_ctx_656[new V("lineTo")].Call(((VisualJs.MathObject[new V("cos")].Call(v_a_666) * v_e_657[new V("radius")]) * new V(0.38d)), ((VisualJs.MathObject[new V("sin")].Call(v_a_666) * v_e_657[new V("radius")]) * new V(0.38d)));
v_ctx_656[new V("stroke")].Call();
}
v_ctx_656[new V("restore")].Call();
}
Scope[new V("drawLinkBreakerTelegraph")].Call(v_ctx_656, v_e_657, v_t_659);
Scope[new V("drawEnemyStatusVfx")].Call(v_ctx_656, v_e_657, v_t_659);
if (new V(v_hit_662 > new V(0d)).Truth) {
VisualJs.Assign(v_ctx_656,new V("strokeStyle"),"=",(new V("rgba(255,255,255,") + (v_hit_662 * new V(0.75d)) + new V(")")));
VisualJs.Assign(v_ctx_656,new V("lineWidth"),"=",new V(2d));
v_ctx_656[new V("beginPath")].Call();
v_ctx_656[new V("moveTo")].Call(((-v_e_657[new V("radius")]) * new V(0.8d)), ((-v_e_657[new V("radius")]) * new V(0.8d)));
v_ctx_656[new V("lineTo")].Call((v_e_657[new V("radius")] * new V(0.8d)), (v_e_657[new V("radius")] * new V(0.8d)));
v_ctx_656[new V("moveTo")].Call((v_e_657[new V("radius")] * new V(0.8d)), ((-v_e_657[new V("radius")]) * new V(0.8d)));
v_ctx_656[new V("lineTo")].Call(((-v_e_657[new V("radius")]) * new V(0.8d)), (v_e_657[new V("radius")] * new V(0.8d)));
v_ctx_656[new V("stroke")].Call();
}
}
v_ctx_656[new V("restore")].Call();
if (v_e_657[new V("isBoss")].Truth) {
V v_barW_667 = VisualJs.MathObject[new V("max")].Call(new V(110d), (v_e_657[new V("radius")] * new V(1.55d)));
V v_barH_668 = new V(6d);
V v_ratio_669 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), (v_e_657[new V("hp")] / VisualJs.MathObject[new V("max")].Call(new V(1d), v_e_657[new V("maxHp")]))));
V v_bx_670 = (v_e_657[new V("pos")][new V("x")] - (v_barW_667 / new V(2d)));
V v_by_671 = ((v_e_657[new V("pos")][new V("y")] - v_e_657[new V("radius")]) - new V(30d));
v_ctx_656[new V("save")].Call();
VisualJs.Assign(v_ctx_656,new V("shadowColor"),"=",v_color_661);
VisualJs.Assign(v_ctx_656,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_656,new V("fillStyle"),"=",new V("rgba(2,7,18,.88)"));
v_ctx_656[new V("fillRect")].Call(v_bx_670, v_by_671, v_barW_667, v_barH_668);
VisualJs.Assign(v_ctx_656,new V("fillStyle"),"=",v_color_661);
v_ctx_656[new V("fillRect")].Call(v_bx_670, v_by_671, (v_barW_667 * v_ratio_669), v_barH_668);
VisualJs.Assign(v_ctx_656,new V("strokeStyle"),"=",new V("rgba(220,250,255,.68)"));
VisualJs.Assign(v_ctx_656,new V("lineWidth"),"=",new V(1d));
v_ctx_656[new V("strokeRect")].Call(v_bx_670, v_by_671, v_barW_667, v_barH_668);
VisualJs.Assign(v_ctx_656,new V("shadowBlur"),"=",new V(0d));
VisualJs.Assign(v_ctx_656,new V("fillStyle"),"=",new V("rgba(230,250,255,.78)"));
VisualJs.Assign(v_ctx_656,new V("font"),"=",new V("bold 8px system-ui,sans-serif"));
VisualJs.Assign(v_ctx_656,new V("textAlign"),"=",new V("center"));
v_ctx_656[new V("fillText")].Call((new V(v_e_657[new V("bossType")] == new V("charger")).Truth ? new V("BREACHER") : (new V(v_e_657[new V("bossType")] == new V("shooter")).Truth ? new V("VOID LANCER") : (new V(v_e_657[new V("bossType")] == new V("summoner")).Truth ? new V("BROOD MIND") : new V("AURA TITAN")))), v_e_657[new V("pos")][new V("x")], (v_by_671 - new V(4d)));
v_ctx_656[new V("restore")].Call();
}
return default;
}
private static V F_drawLinkBreakerTelegraph(V[] args) {
V v_ctx_672 = VisualJs.Arg(args,0);
V v_e_673 = VisualJs.Arg(args,1);
V v_t_674 = VisualJs.Arg(args,2);
if (VisualJs.Or(VisualJs.Or((!v_e_673[new V("isElite")]), () => (!v_e_673[new V("elitePulseTarget")])), () => new V((VisualJs.Or(v_e_673[new V("elitePulseTelegraphTimer")], () => new V(0d))) <= new V(0d))).Truth) return default;
V v_target_675 = v_e_673[new V("elitePulseTarget")];
V v_dx_676 = (v_target_675[new V("pos")][new V("x")] - v_e_673[new V("pos")][new V("x")]);
V v_dy_677 = (v_target_675[new V("pos")][new V("y")] - v_e_673[new V("pos")][new V("y")]);
V v_remaining_678 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), ((VisualJs.Or(v_e_673[new V("elitePulseTelegraphTimer")], () => new V(0d))) / Visual_eliteBalance.Scope[new V("LINK_BREAKER_TELEGRAPH_SECONDS")])));
V v_pulse_679 = (new V(0.65d) + (new V(0.35d) * VisualJs.MathObject[new V("sin")].Call((v_t_674 * new V(16d)))));
v_ctx_672[new V("save")].Call();
VisualJs.Assign(v_ctx_672,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_672,new V("strokeStyle"),"=",new V("#ff4d70"));
VisualJs.Assign(v_ctx_672,new V("shadowColor"),"=",new V("#ff4d70"));
VisualJs.Assign(v_ctx_672,new V("shadowBlur"),"=",new V(4d));
VisualJs.Assign(v_ctx_672,new V("globalAlpha"),"=",(new V(0.3d) + (((new V(1d) - v_remaining_678)) * new V(0.55d))));
VisualJs.Assign(v_ctx_672,new V("lineWidth"),"=",new V(2.5d));
v_ctx_672[new V("beginPath")].Call();
v_ctx_672[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_672[new V("lineTo")].Call(v_dx_676, v_dy_677);
v_ctx_672[new V("stroke")].Call();
v_ctx_672[new V("translate")].Call(v_dx_676, v_dy_677);
v_ctx_672[new V("rotate")].Call(VisualJs.MathObject[new V("atan2")].Call(v_dy_677, v_dx_676));
VisualJs.Assign(v_ctx_672,new V("globalAlpha"),"=",(new V(0.55d) + (new V(0.35d) * v_pulse_679)));
VisualJs.Assign(v_ctx_672,new V("lineWidth"),"=",new V(2d));
v_ctx_672[new V("beginPath")].Call();
v_ctx_672[new V("moveTo")].Call((-new V(16d)), (-new V(10d)));
v_ctx_672[new V("lineTo")].Call((-new V(5d)), new V(0d));
v_ctx_672[new V("lineTo")].Call((-new V(16d)), new V(10d));
v_ctx_672[new V("moveTo")].Call(new V(16d), (-new V(10d)));
v_ctx_672[new V("lineTo")].Call(new V(5d), new V(0d));
v_ctx_672[new V("lineTo")].Call(new V(16d), new V(10d));
v_ctx_672[new V("stroke")].Call();
VisualJs.Assign(v_ctx_672,new V("globalAlpha"),"=",new V(0.32d));
VisualJs.Assign(v_ctx_672,new V("lineWidth"),"=",new V(1d));
v_ctx_672[new V("beginPath")].Call();
v_ctx_672[new V("moveTo")].Call((-new V(9d)), (-new V(15d)));
v_ctx_672[new V("lineTo")].Call(new V(9d), (-new V(15d)));
v_ctx_672[new V("moveTo")].Call((-new V(9d)), new V(15d));
v_ctx_672[new V("lineTo")].Call(new V(9d), new V(15d));
v_ctx_672[new V("stroke")].Call();
v_ctx_672[new V("restore")].Call();
return default;
}
private static V F_drawNetworkDisabledIndicator(V[] args) {
V v_ctx_680 = VisualJs.Arg(args,0);
V v_sphere_681 = VisualJs.Arg(args,1);
V v_t_682 = VisualJs.Arg(args,2);
if (new V((VisualJs.Or(v_sphere_681[new V("networkDisabledTimer")], () => new V(0d))) <= new V(0d)).Truth) return default;
V v_remaining_683 = VisualJs.MathObject[new V("max")].Call(new V(0d), VisualJs.MathObject[new V("min")].Call(new V(1d), ((VisualJs.Or(v_sphere_681[new V("networkDisabledTimer")], () => new V(0d))) / Visual_eliteBalance.Scope[new V("LINK_BREAKER_DISABLED_SECONDS")])));
V v_pulse_684 = (new V(0.6d) + (new V(0.4d) * VisualJs.MathObject[new V("sin")].Call((v_t_682 * new V(13d)))));
v_ctx_680[new V("save")].Call();
VisualJs.Assign(v_ctx_680,new V("globalCompositeOperation"),"=",new V("source-over"));
v_ctx_680[new V("translate")].Call(new V(0d), new V(0d));
VisualJs.Assign(v_ctx_680,new V("strokeStyle"),"=",new V("#ff4d70"));
VisualJs.Assign(v_ctx_680,new V("shadowColor"),"=",new V("#ff4d70"));
VisualJs.Assign(v_ctx_680,new V("shadowBlur"),"=",new V(4d));
VisualJs.Assign(v_ctx_680,new V("globalAlpha"),"=",(new V(0.55d) + (new V(0.35d) * v_pulse_684)));
VisualJs.Assign(v_ctx_680,new V("lineWidth"),"=",new V(2.2d));
v_ctx_680[new V("beginPath")].Call();
v_ctx_680[new V("moveTo")].Call((-new V(17d)), (-new V(10d)));
v_ctx_680[new V("lineTo")].Call((-new V(4d)), new V(0d));
v_ctx_680[new V("lineTo")].Call((-new V(17d)), new V(10d));
v_ctx_680[new V("moveTo")].Call(new V(17d), (-new V(10d)));
v_ctx_680[new V("lineTo")].Call(new V(4d), new V(0d));
v_ctx_680[new V("lineTo")].Call(new V(17d), new V(10d));
v_ctx_680[new V("stroke")].Call();
VisualJs.Assign(v_ctx_680,new V("globalAlpha"),"=",new V(0.28d));
VisualJs.Assign(v_ctx_680,new V("lineWidth"),"=",new V(1.2d));
V v_sweep_685 = ((((new V(1d) - v_remaining_683)) * VisualJs.MathObject[new V("PI")]) * new V(2d));
v_ctx_680[new V("beginPath")].Call();
v_ctx_680[new V("arc")].Call(new V(0d), new V(0d), new V(27d), ((-VisualJs.MathObject[new V("PI")]) / new V(2d)), (((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + v_sweep_685));
v_ctx_680[new V("stroke")].Call();
v_ctx_680[new V("restore")].Call();
return default;
}
private static V F_drawEnemyProjectile(V[] args) {
V v_ctx_686 = VisualJs.Arg(args,0);
V v_x_687 = VisualJs.Arg(args,1);
V v_y_688 = VisualJs.Arg(args,2);
V v_r_689 = VisualJs.Arg(args,3);
V v_vel_690 = VisualJs.Arg(args,4);
V v_sniper_691 = VisualJs.Arg(args,5);
V v_color_692 = (v_sniper_691.Truth ? new V("#ffe066") : new V("#ff6b55"));
V v_rgb_693 = Scope[new V("hexToRgb")].Call(v_color_692);
V v_angle_694 = VisualJs.MathObject[new V("atan2")].Call(v_vel_690[new V("y")], v_vel_690[new V("x")]);
v_ctx_686[new V("save")].Call();
v_ctx_686[new V("translate")].Call(v_x_687, v_y_688);
v_ctx_686[new V("rotate")].Call(v_angle_694);
VisualJs.Assign(v_ctx_686,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_686,new V("shadowColor"),"=",v_color_692);
VisualJs.Assign(v_ctx_686,new V("shadowBlur"),"=",new V(7d));
VisualJs.Assign(v_ctx_686,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_693 + new V(",.30)")));
VisualJs.Assign(v_ctx_686,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(2d), (v_r_689 * new V(0.55d))));
v_ctx_686[new V("beginPath")].Call();
v_ctx_686[new V("moveTo")].Call(((-v_r_689) * new V(4.8d)), new V(0d));
v_ctx_686[new V("lineTo")].Call(((-v_r_689) * new V(0.35d)), new V(0d));
v_ctx_686[new V("stroke")].Call();
VisualJs.Assign(v_ctx_686,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_686,new V("fillStyle"),"=",new V("#07101a"));
VisualJs.Assign(v_ctx_686,new V("strokeStyle"),"=",v_color_692);
VisualJs.Assign(v_ctx_686,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_689 * new V(0.14d))));
if (v_sniper_691.Truth) {
v_ctx_686[new V("beginPath")].Call();
v_ctx_686[new V("moveTo")].Call((v_r_689 * new V(1.9d)), new V(0d));
v_ctx_686[new V("lineTo")].Call(((-v_r_689) * new V(0.8d)), ((-v_r_689) * new V(0.6d)));
v_ctx_686[new V("lineTo")].Call(((-v_r_689) * new V(0.45d)), new V(0d));
v_ctx_686[new V("lineTo")].Call(((-v_r_689) * new V(0.8d)), (v_r_689 * new V(0.6d)));
v_ctx_686[new V("closePath")].Call();
} else {
v_ctx_686[new V("beginPath")].Call();
v_ctx_686[new V("moveTo")].Call((v_r_689 * new V(1.35d)), new V(0d));
v_ctx_686[new V("lineTo")].Call(new V(0d), ((-v_r_689) * new V(0.72d)));
v_ctx_686[new V("lineTo")].Call(((-v_r_689) * new V(0.95d)), new V(0d));
v_ctx_686[new V("lineTo")].Call(new V(0d), (v_r_689 * new V(0.72d)));
v_ctx_686[new V("closePath")].Call();
}
v_ctx_686[new V("fill")].Call();
v_ctx_686[new V("stroke")].Call();
VisualJs.Assign(v_ctx_686,new V("fillStyle"),"=",new V("#ffffff"));
v_ctx_686[new V("beginPath")].Call();
v_ctx_686[new V("arc")].Call((v_r_689 * new V(0.35d)), new V(0d), VisualJs.MathObject[new V("max")].Call(new V(1.4d), (v_r_689 * new V(0.24d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_686[new V("fill")].Call();
v_ctx_686[new V("restore")].Call();
return default;
}
private static V F_drawBossProjectile(V[] args) {
V v_ctx_695 = VisualJs.Arg(args,0);
V v_x_696 = VisualJs.Arg(args,1);
V v_y_697 = VisualJs.Arg(args,2);
V v_r_698 = VisualJs.Arg(args,3);
V v_color_699 = VisualJs.Arg(args,4);
V v_bossType_700 = VisualJs.Arg(args,5);
V v_vel_701 = VisualJs.Arg(args,6);
V v_t_702 = Scope[new V("RENDER_TIME")];
V v_a_703 = VisualJs.MathObject[new V("atan2")].Call(v_vel_701[new V("y")], v_vel_701[new V("x")]);
V v_rgb_704 = Scope[new V("hexToRgb")].Call(v_color_699);
v_ctx_695[new V("save")].Call();
v_ctx_695[new V("translate")].Call(v_x_696, v_y_697);
v_ctx_695[new V("rotate")].Call(v_a_703);
VisualJs.Assign(v_ctx_695,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.20)")));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(2d), (v_r_698 * new V(0.45d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("moveTo")].Call(((-v_r_698) * new V(3.8d)), new V(0d));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.35d)), new V(0d));
v_ctx_695[new V("stroke")].Call();
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",new V("rgba(245,252,255,.65)"));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(0.6d), (v_r_698 * new V(0.08d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("moveTo")].Call(((-v_r_698) * new V(3.2d)), new V(0d));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.4d)), new V(0d));
v_ctx_695[new V("stroke")].Call();
if (new V(v_bossType_700 == new V("shooter")).Truth) {
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",new V("#06101c"));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.95)")));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_698 * new V(0.1d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("moveTo")].Call((v_r_698 * new V(1.35d)), new V(0d));
v_ctx_695[new V("lineTo")].Call((v_r_698 * new V(0.25d)), ((-v_r_698) * new V(0.75d)));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.8d)), ((-v_r_698) * new V(0.42d)));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.45d)), (v_r_698 * new V(0.42d)));
v_ctx_695[new V("lineTo")].Call((v_r_698 * new V(0.25d)), (v_r_698 * new V(0.75d)));
v_ctx_695[new V("closePath")].Call();
v_ctx_695[new V("fill")].Call();
v_ctx_695[new V("stroke")].Call();
for (V v_i_705 = new V(0d); new V(v_i_705 < new V(3d)).Truth; v_i_705++) {
V v_q_706 = ((v_t_702 * new V(4d)) + (v_i_705 * new V(2.1d)));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.55)")));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call(new V(0d), new V(0d), (v_r_698 * ((new V(1.05d) + (v_i_705 * new V(0.18d))))), v_q_706, (v_q_706 + new V(1.1d)));
v_ctx_695[new V("stroke")].Call();
}
} else if (new V(v_bossType_700 == new V("charger")).Truth) {
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",new V("#130b06"));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.95)")));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_698 * new V(0.11d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("moveTo")].Call((v_r_698 * new V(1.5d)), new V(0d));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.45d)), ((-v_r_698) * new V(0.62d)));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.95d)), new V(0d));
v_ctx_695[new V("lineTo")].Call(((-v_r_698) * new V(0.45d)), (v_r_698 * new V(0.62d)));
v_ctx_695[new V("closePath")].Call();
v_ctx_695[new V("fill")].Call();
v_ctx_695[new V("stroke")].Call();
foreach (V v_sy_707 in VisualJs.Arr((-new V(1d)), new V(1d)).Items) {
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",new V("rgba(255,232,190,.72)"));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("moveTo")].Call(((-v_r_698) * new V(0.1d)), ((v_sy_707 * v_r_698) * new V(0.25d)));
v_ctx_695[new V("lineTo")].Call((v_r_698 * new V(0.9d)), ((v_sy_707 * v_r_698) * new V(0.55d)));
v_ctx_695[new V("stroke")].Call();
}
} else if (new V(v_bossType_700 == new V("summoner")).Truth) {
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",new V("#0c0717"));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.95)")));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_698 * new V(0.1d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call(new V(0d), new V(0d), (v_r_698 * new V(0.65d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_695[new V("fill")].Call();
v_ctx_695[new V("stroke")].Call();
for (V v_i_708 = new V(0d); new V(v_i_708 < new V(5d)).Truth; v_i_708++) {
V v_q_709 = ((v_t_702 * new V(2.6d)) + (((v_i_708 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(5d)));
V v_px_710 = ((VisualJs.MathObject[new V("cos")].Call(v_q_709) * v_r_698) * new V(1.15d));
V v_py_711 = ((VisualJs.MathObject[new V("sin")].Call(v_q_709) * v_r_698) * new V(1.15d));
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",new V("#d9c7ff"));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call(v_px_710, v_py_711, (v_r_698 * new V(0.12d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_695[new V("fill")].Call();
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.6)")));
v_ctx_695[new V("stroke")].Call();
}
} else {
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",new V("#120511"));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.95)")));
VisualJs.Assign(v_ctx_695,new V("lineWidth"),"=",VisualJs.MathObject[new V("max")].Call(new V(1d), (v_r_698 * new V(0.1d))));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call(new V(0d), new V(0d), (v_r_698 * new V(0.72d)), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_695[new V("fill")].Call();
v_ctx_695[new V("stroke")].Call();
for (V v_i_712 = new V(0d); new V(v_i_712 < new V(4d)).Truth; v_i_712++) {
V v_q_713 = ((v_t_702 * new V(3d)) + (((v_i_712 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(4d)));
VisualJs.Assign(v_ctx_695,new V("strokeStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.68)")));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call(new V(0d), new V(0d), (v_r_698 * ((new V(0.95d) + (v_i_712 * new V(0.22d))))), v_q_713, (v_q_713 + new V(1d)));
v_ctx_695[new V("stroke")].Call();
}
}
VisualJs.Assign(v_ctx_695,new V("globalCompositeOperation"),"=",new V("lighter"));
VisualJs.Assign(v_ctx_695,new V("fillStyle"),"=",(new V("rgba(") + v_rgb_704 + new V(",.65)")));
v_ctx_695[new V("beginPath")].Call();
v_ctx_695[new V("arc")].Call((v_r_698 * new V(0.35d)), new V(0d), ((v_r_698 * new V(0.22d)) * ((new V(1d) + (new V(0.12d) * VisualJs.MathObject[new V("sin")].Call((v_t_702 * new V(10d))))))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_695[new V("fill")].Call();
v_ctx_695[new V("restore")].Call();
return default;
}
private static V F_drawChest(V[] args) {
V v_ctx_714 = VisualJs.Arg(args,0);
V v_chest_715 = VisualJs.Arg(args,1);
V v_t_716 = Scope[new V("RENDER_TIME")];
V v_p_717 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_716 * new V(3.2d))) * new V(0.05d)));
v_ctx_714[new V("save")].Call();
v_ctx_714[new V("translate")].Call(v_chest_715[new V("pos")][new V("x")], v_chest_715[new V("pos")][new V("y")]);
v_ctx_714[new V("scale")].Call(v_p_717, v_p_717);
VisualJs.Assign(v_ctx_714,new V("globalCompositeOperation"),"=",new V("source-over"));
Scope[new V("drawGroundShadow")].Call(v_ctx_714, new V(18d), new V(6d), new V(5d));
VisualJs.Assign(v_ctx_714,new V("shadowColor"),"=",new V("#a98aff"));
VisualJs.Assign(v_ctx_714,new V("shadowBlur"),"=",new V(10d));
VisualJs.Assign(v_ctx_714,new V("fillStyle"),"=",new V("rgba(4,10,19,.96)"));
VisualJs.Assign(v_ctx_714,new V("strokeStyle"),"=",new V("rgba(194,160,255,.86)"));
VisualJs.Assign(v_ctx_714,new V("lineWidth"),"=",new V(1.4d));
v_ctx_714[new V("beginPath")].Call();
v_ctx_714[new V("moveTo")].Call((-new V(17d)), new V(7d));
v_ctx_714[new V("lineTo")].Call((-new V(13d)), (-new V(8d)));
v_ctx_714[new V("lineTo")].Call(new V(0d), (-new V(14d)));
v_ctx_714[new V("lineTo")].Call(new V(13d), (-new V(8d)));
v_ctx_714[new V("lineTo")].Call(new V(17d), new V(7d));
v_ctx_714[new V("lineTo")].Call(new V(0d), new V(14d));
v_ctx_714[new V("closePath")].Call();
v_ctx_714[new V("fill")].Call();
v_ctx_714[new V("stroke")].Call();
VisualJs.Assign(v_ctx_714,new V("strokeStyle"),"=",new V("rgba(111,224,255,.55)"));
v_ctx_714[new V("beginPath")].Call();
v_ctx_714[new V("moveTo")].Call((-new V(13d)), (-new V(8d)));
v_ctx_714[new V("lineTo")].Call(new V(0d), (-new V(2d)));
v_ctx_714[new V("lineTo")].Call(new V(13d), (-new V(8d)));
v_ctx_714[new V("moveTo")].Call(new V(0d), (-new V(2d)));
v_ctx_714[new V("lineTo")].Call(new V(0d), new V(10d));
v_ctx_714[new V("stroke")].Call();
VisualJs.Assign(v_ctx_714,new V("fillStyle"),"=",new V("#fff"));
v_ctx_714[new V("beginPath")].Call();
v_ctx_714[new V("arc")].Call(new V(0d), (-new V(2d)), new V(3d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_714[new V("fill")].Call();
v_ctx_714[new V("restore")].Call();
return default;
}
private static V F_drawRune(V[] args) {
V v_ctx_718 = VisualJs.Arg(args,0);
V v_rune_719 = VisualJs.Arg(args,1);
V v_def_720 = Visual_runes.Scope[new V("RUNE_DEFS")][v_rune_719[new V("type")]];
V v_t_721 = Scope[new V("RENDER_TIME")];
V v_p_722 = (new V(1d) + (VisualJs.MathObject[new V("sin")].Call((v_t_721 * new V(4d))) * new V(0.06d)));
v_ctx_718[new V("save")].Call();
v_ctx_718[new V("translate")].Call(v_rune_719[new V("pos")][new V("x")], v_rune_719[new V("pos")][new V("y")]);
v_ctx_718[new V("scale")].Call(v_p_722, v_p_722);
VisualJs.Assign(v_ctx_718,new V("globalCompositeOperation"),"=",new V("source-over"));
VisualJs.Assign(v_ctx_718,new V("shadowColor"),"=",v_def_720[new V("color")]);
VisualJs.Assign(v_ctx_718,new V("shadowBlur"),"=",new V(8d));
VisualJs.Assign(v_ctx_718,new V("fillStyle"),"=",new V("rgba(5,12,22,.96)"));
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",v_def_720[new V("color")]);
VisualJs.Assign(v_ctx_718,new V("lineWidth"),"=",new V(1.5d));
if (new V(v_rune_719[new V("type")] == new V("phase")).Truth) {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call(new V(0d), (-new V(16d)));
v_ctx_718[new V("lineTo")].Call(new V(8d), (-new V(8d)));
v_ctx_718[new V("lineTo")].Call(new V(16d), new V(0d));
v_ctx_718[new V("lineTo")].Call(new V(8d), new V(8d));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(16d));
v_ctx_718[new V("lineTo")].Call((-new V(8d)), new V(8d));
v_ctx_718[new V("lineTo")].Call((-new V(16d)), new V(0d));
v_ctx_718[new V("lineTo")].Call((-new V(8d)), (-new V(8d)));
v_ctx_718[new V("closePath")].Call();
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",new V("#eaffff"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call((-new V(7d)), (-new V(7d)));
v_ctx_718[new V("lineTo")].Call(new V(7d), new V(7d));
v_ctx_718[new V("moveTo")].Call(new V(7d), (-new V(7d)));
v_ctx_718[new V("lineTo")].Call((-new V(7d)), new V(7d));
v_ctx_718[new V("stroke")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("harvest")).Truth) {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call(new V(0d), (-new V(16d)));
v_ctx_718[new V("quadraticCurveTo")].Call(new V(15d), (-new V(8d)), new V(10d), new V(9d));
v_ctx_718[new V("quadraticCurveTo")].Call(new V(0d), new V(18d), (-new V(10d)), new V(9d));
v_ctx_718[new V("quadraticCurveTo")].Call((-new V(15d)), (-new V(8d)), new V(0d), (-new V(16d)));
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
for (V v_i_723 = new V(0d); new V(v_i_723 < new V(3d)).Truth; v_i_723++) {
V v_a_724 = ((v_t_721 * new V(1.3d)) + (((v_i_723 * VisualJs.MathObject[new V("PI")]) * new V(2d)) / new V(3d)));
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",new V("rgba(255,255,255,.55)"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call(new V(0d), new V(0d));
v_ctx_718[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_724) * new V(9d)), (VisualJs.MathObject[new V("sin")].Call(v_a_724) * new V(7d)));
v_ctx_718[new V("stroke")].Call();
}
} else if (new V(v_rune_719[new V("type")] == new V("purge")).Truth) {
v_ctx_718[new V("beginPath")].Call();
for (V v_i_725 = new V(0d); new V(v_i_725 < new V(10d)).Truth; v_i_725++) {
V v_a_726 = (((-VisualJs.MathObject[new V("PI")]) / new V(2d)) + ((v_i_725 * VisualJs.MathObject[new V("PI")]) / new V(5d)));
V v_rr_727 = (new V((v_i_725 % new V(2d)) == new V(0d)).Truth ? new V(17d) : new V(8d));
V v_x_728 = (VisualJs.MathObject[new V("cos")].Call(v_a_726) * v_rr_727);
V v_y_729 = (VisualJs.MathObject[new V("sin")].Call(v_a_726) * v_rr_727);
if (new V(v_i_725 == new V(0d)).Truth) v_ctx_718[new V("moveTo")].Call(v_x_728, v_y_729); else v_ctx_718[new V("lineTo")].Call(v_x_728, v_y_729);
}
v_ctx_718[new V("closePath")].Call();
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
VisualJs.Assign(v_ctx_718,new V("fillStyle"),"=",new V("rgba(255,255,255,.55)"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(4d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("fill")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("resonance")).Truth) {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(15d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("stroke")].Call();
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(7d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("stroke")].Call();
V v_a_730 = (v_t_721 * new V(2d));
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",new V("#fff"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_730) * new V(4d)), (VisualJs.MathObject[new V("sin")].Call(v_a_730) * new V(4d)));
v_ctx_718[new V("lineTo")].Call((VisualJs.MathObject[new V("cos")].Call(v_a_730) * new V(13d)), (VisualJs.MathObject[new V("sin")].Call(v_a_730) * new V(13d)));
v_ctx_718[new V("stroke")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("fortify")).Truth) {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call(new V(0d), (-new V(17d)));
v_ctx_718[new V("lineTo")].Call(new V(13d), (-new V(10d)));
v_ctx_718[new V("lineTo")].Call(new V(10d), new V(9d));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(17d));
v_ctx_718[new V("lineTo")].Call((-new V(10d)), new V(9d));
v_ctx_718[new V("lineTo")].Call((-new V(13d)), (-new V(10d)));
v_ctx_718[new V("closePath")].Call();
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",new V("rgba(230,250,255,.7)"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call((-new V(7d)), (-new V(3d)));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(6d));
v_ctx_718[new V("lineTo")].Call(new V(8d), (-new V(7d)));
v_ctx_718[new V("stroke")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("hunt")).Truth) {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call((-new V(15d)), (-new V(9d)));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(0d));
v_ctx_718[new V("lineTo")].Call((-new V(15d)), new V(9d));
v_ctx_718[new V("moveTo")].Call(new V(15d), (-new V(9d)));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(0d));
v_ctx_718[new V("lineTo")].Call(new V(15d), new V(9d));
v_ctx_718[new V("stroke")].Call();
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), (new V(6d) + (VisualJs.MathObject[new V("sin")].Call((v_t_721 * new V(7d))) * new V(1.2d))), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("echo")).Truth) {
for (V v_i_731 = new V(0d); new V(v_i_731 < new V(3d)).Truth; v_i_731++) {
V v_rr_732 = ((new V(5d) + (v_i_731 * new V(5d))) + (VisualJs.MathObject[new V("sin")].Call(((v_t_721 * new V(3d)) + v_i_731)) * new V(1.2d)));
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",(new V("rgba(220,248,255,") + (new V(0.3d) + (v_i_731 * new V(0.2d))) + new V(")")));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), v_rr_732, new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_718,new V("fillStyle"),"=",new V("#fff"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(3d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("fill")].Call();
} else if (new V(v_rune_719[new V("type")] == new V("gravity")).Truth) {
for (V v_i_733 = new V(0d); new V(v_i_733 < new V(4d)).Truth; v_i_733++) {
V v_a_734 = ((v_t_721 * new V(0.7d)) + ((v_i_733 * VisualJs.MathObject[new V("PI")]) / new V(2d)));
VisualJs.Assign(v_ctx_718,new V("strokeStyle"),"=",(new V("rgba(190,150,255,") + (new V(0.75d) - (v_i_733 * new V(0.12d))) + new V(")")));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), (new V(6d) + (v_i_733 * new V(3d))), v_a_734, (v_a_734 + new V(1.9d)));
v_ctx_718[new V("stroke")].Call();
}
VisualJs.Assign(v_ctx_718,new V("fillStyle"),"=",new V("#fff"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(2.8d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("fill")].Call();
} else {
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("moveTo")].Call(new V(0d), (-new V(15d)));
v_ctx_718[new V("lineTo")].Call(new V(13d), new V(0d));
v_ctx_718[new V("lineTo")].Call(new V(0d), new V(15d));
v_ctx_718[new V("lineTo")].Call((-new V(13d)), new V(0d));
v_ctx_718[new V("closePath")].Call();
v_ctx_718[new V("fill")].Call();
v_ctx_718[new V("stroke")].Call();
VisualJs.Assign(v_ctx_718,new V("fillStyle"),"=",new V("#fff"));
v_ctx_718[new V("beginPath")].Call();
v_ctx_718[new V("arc")].Call(new V(0d), new V(0d), new V(3.5d), new V(0d), (VisualJs.MathObject[new V("PI")] * new V(2d)));
v_ctx_718[new V("fill")].Call();
}
VisualJs.Assign(v_ctx_718,new V("shadowBlur"),"=",new V(0d));
v_ctx_718[new V("restore")].Call();
return default;
}
}
}
