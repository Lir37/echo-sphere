using System;
using EchoSphere.Core;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  internal struct NativeSphereVisual
  {
   internal SphereAttackAgent Identity;internal SphereId Type;internal string Family,Branch,Element;
   internal double X,Y,Radius,Tier,Rotation,AttackDelay,AttackTimer,AuraTimer,DisabledTimer,Final;
   internal bool Alive,Resonance;
  }
  internal NativeSphereVisual ProjectNativeSphereVisual(SphereAttackAgent sphere)
  {
   var point=NativeSourcePixels(sphere.transform.position);string family=sphere.Type.ToString().ToLowerInvariant(),branch=GetSphereBranch(sphere.Type)??"";double final=NativeSphereFinalIndex(sphere.Type);
   string element=NativeRunCatalog.Instance.GetSphereBranch(branch)?.Element;if(sphere.Type==SphereId.Prism&&branch=="prism_spectrum")element=final==1?"freeze":final==2?"poison":"fire";
   return new NativeSphereVisual {Identity=sphere,Type=sphere.Type,Family=family,Branch=branch,Element=element,X=point.x,Y=point.y,Radius=130,Tier=sphere.ProgressionLevel,Rotation=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitAngle:(double)(sphere.Type==SphereId.Gravity?sphere.VisualRotation:-sphere.VisualRotation),AttackDelay=1.2,AttackTimer=sphere.AttackTimer,AuraTimer=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitalContactCheckTimer:sphere.AttackTimer,DisabledTimer=sphere.SourceProjectionNetworkDisabledTimer,Final=final,Alive=sphere.gameObject.activeInHierarchy,Resonance=_resonanceEventActive};
  }
  private static double NativeSphereScale(NativeSphereVisual sphere)=>Math.Max(15,Math.Min(25,sphere.Radius*.19+sphere.Tier*.8))/24;
  private double NativeOrbitalRadiusScale(NativeSphereVisual sphere)=>NativePlannerSphereRadius(_sourceRenderState,RenderEntity(sphere.Identity))/Math.Max(1,sphere.Radius);
  private void DrawNativeSphereFrame(SphereAttackAgent component,double time)
  {
   var sphere=ProjectNativeSphereVisual(component);double r=Math.Max(15,Math.Min(25,sphere.Radius*.19+sphere.Tier*.8)),scale=r/24;
   if(sphere.Type==SphereId.Standard){
    if(!DrawNativeStandardAssembly(sphere,r,time)){DrawNativeViewGroundShadow(r*1.05,r*.24,4);_worldSurface.Save();NativeWorldTranslate(sphere.X,sphere.Y);_nativeWorldFeedback.StandardFallback(r);_worldSurface.Restore();}
    DrawNativeSphereMutation(sphere,time,scale);_nativeWorldStandardSphere.DrawAttack(_worldSurface,sphere.X,sphere.Y,double.IsNaN(sphere.Rotation)?0:sphere.Rotation,r,sphere.AttackTimer);DrawNativeSphereElements(sphere,time,scale);DrawNativeSphereModifiers(sphere,time,scale);DrawNativeSphereDisabled(sphere,time);return;
   }
   if(sphere.Type==SphereId.Orbital)DrawNativeOrbitalSphere(sphere,time,scale,NativeOrbitalRadiusScale(sphere),false);
   else if(sphere.Type==SphereId.Sniper||sphere.Type==SphereId.Shotgun||sphere.Type==SphereId.Prism||sphere.Type==SphereId.Void){DrawNativeSphereMutation(sphere,time,scale);DrawNativeSphereFamily(sphere,time,scale);}
   else if(sphere.Type==SphereId.Chain||sphere.Type==SphereId.Aura||sphere.Type==SphereId.Gravity||sphere.Type==SphereId.Pulse){DrawNativeSphereFamily(sphere,time,scale);DrawNativeSphereMutation(sphere,time,scale);}
   else throw new InvalidOperationException("Unknown sphere presentation type: "+sphere.Family);
   DrawNativeSphereElements(sphere,time,scale);DrawNativeSphereModifiers(sphere,time,scale);DrawNativeSphereDisabled(sphere,time);
  }
  private void DrawNativeSphereDisabled(NativeSphereVisual sphere,double time)=>_nativeWorldCreatures.DrawDisabledNetwork(_worldSurface,double.IsNaN(sphere.DisabledTimer)?0:sphere.DisabledTimer,2,time);
 }
}
