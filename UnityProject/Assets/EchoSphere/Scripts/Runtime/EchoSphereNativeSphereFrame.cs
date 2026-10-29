using System;
using System.Linq;
using System.Collections.Generic;
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
   internal bool Alive,Resonance,AttackActive;
  }
  internal NativeSphereVisual ProjectNativeSphereVisual(SphereAttackAgent sphere)
  {
   var point=NativeSourcePixels(sphere.transform.position);string family=sphere.Type.ToString().ToLowerInvariant(),branch=GetSphereBranch(sphere.Type)??"";double final=NativeSphereFinalIndex(sphere.Type);
   string element=NativeRunCatalog.Instance.GetSphereBranch(branch)?.Element;if(sphere.Type==SphereId.Prism&&branch=="prism_spectrum")element=final==1?"freeze":final==2?"poison":"fire";
   return new NativeSphereVisual {Identity=sphere,Type=sphere.Type,Family=family,Branch=branch,Element=element,X=point.x,Y=point.y,Radius=130,Tier=sphere.ProgressionLevel,Rotation=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitAngle:(double)(sphere.Type==SphereId.Gravity?sphere.VisualRotation:-sphere.PresentationRotation),AttackDelay=sphere.AttackDelay,AttackTimer=sphere.AttackTimer,AttackActive=sphere.PresentationAttackActive,AuraTimer=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitalContactCheckTimer:sphere.AttackTimer,DisabledTimer=sphere.SourceProjectionNetworkDisabledTimer,Final=final,Alive=sphere.gameObject.activeInHierarchy,Resonance=_resonanceEventActive};
  }
  private static double NativeSphereScale(NativeSphereVisual sphere)=>Math.Max(15,Math.Min(25,sphere.Radius*.19+sphere.Tier*.8))/24;
  private double NativeOrbitalRadiusScale(NativeSphereVisual sphere)=>NativeLiveSphereRadius(sphere.Type)/Math.Max(1,sphere.Radius);
  private double NativeLiveArtifactEffect(string effect)
  {double total=0;foreach(var id in _ownedArtifacts){var artifact=NativeRunCatalog.Instance.GetArtifact(id);if(artifact!=null)total+=artifact.Effect(effect);}return total;}
  private List<NativePixelPoint> NativeLiveLocalSpherePoints()
  {
   var player=NativeSourcePixels(PlayerPosition);double Distance(NativePixelPoint point)=>NativeMobileDistance(point.x-player.x,point.y-player.y);
   return _spheres.Where(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy).Select(sphere=>NativeSourcePixels(sphere.transform.position)).Where(point=>Distance(point)<=420).OrderBy(Distance).Take(8).ToList();
  }
  private int NativeLiveEngineerComponentSize()
  {
   var local=NativeLiveLocalSpherePoints();double mastery=_characterMasteryLevel!=0?_characterMasteryLevel:1,range=mastery>=6?260:mastery>=2?240:220;int best=0;
   for(int start=0;start<local.Count;start++){var seen=new bool[local.Count];var queue=new Queue<int>();seen[start]=true;queue.Enqueue(start);int count=1;while(queue.Count>0){int current=queue.Dequeue();for(int i=0;i<local.Count;i++)if(!seen[i]&&NativeMobileDistance(local[current].x-local[i].x,local[current].y-local[i].y)<=range){seen[i]=true;count++;queue.Enqueue(i);}}best=Math.Max(best,count);}return best;
  }
  internal double NativeLiveSphereRadius(SphereId type)
  {
   double radius=130;radius*=1+GetAbilityLevel(RunUpgradeKind.Radius)*.15;radius*=1+GetShopUpgradeLevel("radius")*.05;radius*=Math.Max(.5,1+NativeLiveArtifactEffect("sphereRadius"));
   if(_chaosOrbBuff=="radius"&&_chaosOrbBuffTimer>0)radius*=1.2;if(_mutationStage>=2)radius*=1.15;
   double mastery=_characterMasteryLevel!=0?_characterMasteryLevel:1,multiplier=1+NativeRunCatalog.Instance.GetCharacter(CharacterRules.ToSourceId(_sphereCharacterId)).SphereRadius;
   if(_sphereCharacterId==SphereCharacterId.Spherist&&mastery>=5&&_spheres.Count(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy)>=8)multiplier+=.05;
   if(_sphereCharacterId==SphereCharacterId.Architect){if(NativeLiveLocalSpherePoints().Count>=4)multiplier+=.05;if(mastery>=3)multiplier+=.02;}
   if(_sphereCharacterId==SphereCharacterId.Engineer&&NativeLiveEngineerComponentSize()>=3)multiplier+=.08;radius*=multiplier;radius*=GetSourceSphereModifiers(type).radius;
   if(_sphereCharacterId==SphereCharacterId.Architect&&_characterMasteryLevel>=3)radius*=1.02;return radius;
  }
  private void DrawNativeSphereFrame(SphereAttackAgent component,double time)
  {
   var sphere=ProjectNativeSphereVisual(component);double r=Math.Max(15,Math.Min(25,sphere.Radius*.19+sphere.Tier*.8)),scale=r/24;
   if(sphere.Type==SphereId.Standard){
    if(!DrawNativeStandardAssembly(sphere,r,time)){DrawNativeViewGroundShadow(r*1.05,r*.24,4);_worldSurface.Save();NativeWorldTranslate(sphere.X,sphere.Y);_nativeWorldFeedback.StandardFallback(r);_worldSurface.Restore();}
    DrawRetainedSphereWeapon(sphere,time,r);DrawNativeSphereElements(sphere,time,scale);DrawNativeSphereModifiers(sphere,time,scale);DrawNativeSphereDisabled(sphere,time);return;
   }
   if(sphere.Type==SphereId.Orbital)DrawNativeOrbitalSphere(sphere,time,scale,NativeOrbitalRadiusScale(sphere),false);
   else if(sphere.Type==SphereId.Sniper||sphere.Type==SphereId.Shotgun){DrawNativeSphereFamily(sphere,time,scale,false);DrawRetainedSphereWeapon(sphere,time,r);}
   else if(sphere.Type==SphereId.Prism||sphere.Type==SphereId.Void){DrawNativeSphereMutation(sphere,time,scale);DrawNativeSphereFamily(sphere,time,scale);}
   else if(sphere.Type==SphereId.Chain||sphere.Type==SphereId.Aura||sphere.Type==SphereId.Gravity||sphere.Type==SphereId.Pulse){DrawNativeSphereFamily(sphere,time,scale);DrawNativeSphereMutation(sphere,time,scale);}
   else throw new InvalidOperationException("Unknown sphere presentation type: "+sphere.Family);
   DrawNativeSphereElements(sphere,time,scale);DrawNativeSphereModifiers(sphere,time,scale);DrawNativeSphereDisabled(sphere,time);
  }
  private void DrawRetainedSphereWeapon(NativeSphereVisual sphere,double time,double radius)
  {
   WorldPresentation.SphereWeapons.Draw(_worldSurface,new NativeWorldSphereWeapons.State{Identity=sphere.Identity,Family=sphere.Family,Branch=sphere.Branch,Final=(int)sphere.Final,X=sphere.X,Y=sphere.Y,Radius=radius,Rotation=double.IsNaN(sphere.Rotation)?0:sphere.Rotation,Time=time,AttackDelay=sphere.AttackDelay,ShotSerial=sphere.Identity.PresentationShotSerial,Disabled=sphere.DisabledTimer>0||!sphere.Alive,Resonance=sphere.Resonance});
  }
  private void DrawNativeSphereDisabled(NativeSphereVisual sphere,double time)=>_nativeWorldCreatures.DrawDisabledNetwork(_worldSurface,double.IsNaN(sphere.DisabledTimer)?0:sphere.DisabledTimer,2,time);
 }
}
