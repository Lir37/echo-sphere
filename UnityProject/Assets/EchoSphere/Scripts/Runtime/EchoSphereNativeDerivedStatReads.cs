using System;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private static double NativeOrZero(V value)=>value.Truth?value.Number:0;
  private static double NativeArtifactEffectTotal(V scene,string effect){double total=0;foreach(var id in scene["player"]["artifacts"].Items){var artifact=NativeRunCatalog.Instance.GetArtifact(id.Text);if(artifact!=null)total+=artifact.Effect(effect);}return total;}
  internal static double NativePlannerCritChance(V scene)=>Math.Min(.75,.05+NativeOrZero(scene["player"]["abilities"]["crit"])*.1+NativeOrZero(scene["shopUpgrades"]["crit"])*.05+NativeArtifactEffectTotal(scene,"critChance"));
  internal static double NativePlannerDodgeChance(V scene)=>Math.Min(.75,NativeOrZero(scene["player"]["abilities"]["dodge"])*.1+NativeArtifactEffectTotal(scene,"dodgeChance"));
  internal static double NativePlannerVampire(V scene)=>NativeOrZero(scene["player"]["abilities"]["vampire"])*.03+NativeArtifactEffectTotal(scene,"vampire");
  internal static double NativePlannerMoveSpeed(V scene)
  {
   var player=scene["player"];var abilities=player["abilities"];string character=NativeCharacterWorldId(scene);
   double speed=180;speed*=1+NativeOrZero(abilities["movespeed"])*.1;speed*=1+NativeOrZero(scene["shopUpgrades"]["speed"])*.05;speed*=Math.Max(.5,1+NativeArtifactEffectTotal(scene,"moveSpeed"));
   if(player["swiftBootsTimer"].Number>0)speed*=1.1;if(player["mutationStage"].Number>=3)speed*=1.2;speed*=1+NativeRunCatalog.Instance.GetCharacter(character).MoveSpeed;
   if(player["hunterTrophyTimer"].Number>0&&character=="hunter")speed*=1.1;
   if(character=="berserker"&&player["characterMasteryLevel"].Number>=5&&player["buffTimer"].Number>0)speed*=1.05;
   if(abilities["momentum"].Truth&&player["dashTimer"].Number>0)speed*=1+abilities["momentum"].Number*.05;
   return speed;
  }
 }
}
