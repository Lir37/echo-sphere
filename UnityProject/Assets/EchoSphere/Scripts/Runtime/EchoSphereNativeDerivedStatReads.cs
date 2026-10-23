using System;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
#if UNITY_EDITOR
  private static double NativeOrZero(V value)=>value.Truth?value.Number:0;
  private static double NativeArtifactEffectTotal(V scene,string effect){double total=0;foreach(var id in scene["player"]["artifacts"].Items){var artifact=NativeRunCatalog.Instance.GetArtifact(id.Text);if(artifact!=null)total+=artifact.Effect(effect);}return total;}
#endif
  private double NativeLivePlannerCritChance()=>Math.Min(.75,.05+(double)NativeLiveAbilityLevel("crit")*.1+(double)GetShopUpgradeLevel("crit")*.05+NativeLiveArtifactEffect("critChance"));
  private double NativeLivePlannerDodgeChance()=>Math.Min(.75,(double)NativeLiveAbilityLevel("dodge")*.1+NativeLiveArtifactEffect("dodgeChance"));
  private double NativeLivePlannerVampire()=>(double)NativeLiveAbilityLevel("vampire")*.03+NativeLiveArtifactEffect("vampire");
  private double NativeLivePlannerMoveSpeed()
  {
   string character=EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId);
   double speed=180;speed*=1+(double)NativeLiveAbilityLevel("movespeed")*.1;speed*=1+(double)GetShopUpgradeLevel("speed")*.05;speed*=Math.Max(.5,1+NativeLiveArtifactEffect("moveSpeed"));
   if((double)_swiftBootsTimer>0)speed*=1.1;if((double)_mutationStage>=3)speed*=1.2;speed*=1+NativeRunCatalog.Instance.GetCharacter(character).MoveSpeed;
   if((double)_hunterTrophyTimer>0&&character=="hunter")speed*=1.1;
   if(character=="berserker"&&(double)_characterMasteryLevel>=5&&(double)_buffTimer>0)speed*=1.05;
   if(NativeLiveAbilityLevel("momentum")!=0&&(double)_playerCore.DashTimer>0)speed*=1+(double)NativeLiveAbilityLevel("momentum")*.05;
   return speed;
  }  private double NativeLiveXpPlannerMultiplier(){double mult=1+GetShopUpgradeLevel("xp")*.05;foreach(var id in _ownedArtifacts){var artifact=NativeRunCatalog.Instance.GetArtifact(id);if(artifact!=null)mult+=artifact.Effect("xpGain");}return mult;}
#if UNITY_EDITOR
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
#endif
 }
}
