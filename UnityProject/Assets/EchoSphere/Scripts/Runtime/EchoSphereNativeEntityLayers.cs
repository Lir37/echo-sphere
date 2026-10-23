using System;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private static readonly Color NativeExperienceColor=NativeMutationColor("#63e6ff"),NativeHealthColor=NativeMutationColor("#ff5c72");
  internal struct NativePixelPoint {internal double x,y;}
  private static NativePixelPoint NativeSourcePixels(Vector2 world)=>new NativePixelPoint {x=world.x/SpherePlacementRules.SourcePixelsToWorld,y=-world.y/SpherePlacementRules.SourcePixelsToWorld};
  private void DrawNativeExperienceLayer(){foreach(var agent in _simulationAgents)if(agent is ExperienceOrbAgent orb&&orb!=null&&orb.gameObject.activeInHierarchy){var point=NativeSourcePixels(orb.transform.position);_nativeWorldPickups.DrawXp(_worldSurface,(float)point.x,(float)point.y,6,NativeExperienceColor,orb.SourceProjectionRotation);}}
  private void DrawNativeHealthLayer(){foreach(var agent in _simulationAgents)if(agent is HealthPackAgent2D pack&&pack!=null&&pack.gameObject.activeInHierarchy){var point=NativeSourcePixels(pack.transform.position);_nativeWorldPickups.DrawHealth(_worldSurface,(float)point.x,(float)point.y,NativeHealthColor,_runTime);}}
  private void DrawNativeRuneLayer(){foreach(var agent in _simulationAgents)if(agent is RuneAgent2D rune&&rune!=null&&rune.gameObject.activeInHierarchy){var point=NativeSourcePixels(rune.transform.position);var definition=NativeRunCatalog.Instance.GetRune(rune.SourceProjectionType);_nativeWorldPickableViews.DrawRune(_worldSurface,point.x,point.y,rune.SourceProjectionType,NativeMutationColor(definition.Color),_runTime);}}
  private void DrawNativeChestLayer(bool stella)
  {
   string label=stella?"STELLA":null;if(stella&&_worldSurface.LocalizeCanvasText)label=NativeLocalizationCatalog.Instance.CanvasText(label,IsRussianUi);
   foreach(var agent in _simulationAgents)if(agent!=null&&agent.gameObject.activeInHierarchy&&(stella?agent is StellaChestAgent2D:agent is ArtifactChestAgent2D)){var point=NativeSourcePixels(agent.transform.position);_nativeWorldPickableViews.DrawChest(_worldSurface,point.x,point.y,_runTime,stella,label);}
  }
  private void DrawNativeDroneLayer(){foreach(var agent in _simulationAgents)if(agent is AbilityDroneAgent2D drone&&drone!=null&&drone.gameObject.activeInHierarchy){var point=NativeSourcePixels(drone.transform.position);string color=NativeRunCatalog.Instance.GetSphere(drone.SourceProjectionAnchorType.ToString().ToLowerInvariant())?.Color;_nativeWorldMinions.Draw(_worldSurface,(float)point.x,(float)point.y,12,drone.SourceProjectionRotation,NativeMutationColor(string.IsNullOrEmpty(color)?"#ffb84d":color),_runTime);}}
  internal struct NativeParticleView {internal double X,Y,Size,Alpha,VelocityX,VelocityY;internal Color Color;}
  internal static NativeParticleView ProjectNativeParticle(SourceParticle2D particle)
  {
   var point=NativeSourcePixels(particle.transform.position);var velocity=NativeSourcePixels(particle.SourceProjectionVelocity);
   return new NativeParticleView {X=point.x,Y=point.y,Size=particle.SourceProjectionSize,Alpha=(double)particle.SourceProjectionLife/particle.SourceProjectionMaxLife,VelocityX=velocity.x!=0?velocity.x:1,VelocityY=velocity.y,Color=NativeMutationColor("#"+ColorUtility.ToHtmlStringRGB(particle.SourceProjectionColor))};
  }
  private void DrawNativeParticleLayer(int stride)
  {
   int index=0;foreach(var agent in _simulationAgents)if(agent is SourceParticle2D particle&&particle!=null&&particle.gameObject.activeInHierarchy){if(index++%stride!=0)continue;var view=ProjectNativeParticle(particle);_worldSurface.Shadow=view.Color;_worldSurface.ShadowBlur=0;_nativeWorldFeedback.Particle(view.X,view.Y,view.Size,view.Alpha,view.VelocityX,view.VelocityY,view.Color);}
  }
 }
}
