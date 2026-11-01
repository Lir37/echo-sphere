using System;
namespace EchoSphere.Core {
 public enum RegionalMotion { Chase, Orbit, Hold, Skirmish, Lunge, Flank }
 public readonly struct RegionalEnemyProfile {
  public readonly RegionalMotion Motion; public readonly float Range,Minimum,Interval,Windup,Speed; public readonly int Pellets; public readonly string Shot;
  public RegionalEnemyProfile(RegionalMotion motion,float range,float minimum,float interval,float windup,float speed,string shot,int pellets=1){Motion=motion;Range=range;Minimum=minimum;Interval=interval;Windup=windup;Speed=speed;Shot=shot;Pellets=pellets;}
 }
 public static class RegionalEnemyRules {
  public static RegionalEnemyProfile Get(int region,string role){
   bool sniper=role=="sniper",ranged=sniper||role=="ranged";
   // Only explicit firing roles shoot. Other silhouettes retain regional movement
   // without silently becoming ranged units. Distances are source pixels.
   switch(region){
    case 1:return ranged?new RegionalEnemyProfile(sniper?RegionalMotion.Hold:RegionalMotion.Orbit,sniper?110:80,0,sniper?3.8f:2.8f,sniper?.85f:.55f,sniper?350:250,"fleet"):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,0,0,0,"");
    case 2:return ranged||role=="bomber"||role=="corruptor"?new RegionalEnemyProfile(RegionalMotion.Skirmish,sniper?105:75,0,4,.7f,190,"spore",3):new RegionalEnemyProfile(RegionalMotion.Lunge,0,0,3.2f,.65f,0,"bite");
    case 3:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?110:85,0,sniper?4:3.6f,.9f,280,"crystal"):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
    case 4:return ranged?new RegionalEnemyProfile(RegionalMotion.Orbit,sniper?100:70,0,2.9f,.65f,310,"arc",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,0,0,0,"");
    case 5:return ranged?new RegionalEnemyProfile(RegionalMotion.Flank,sniper?105:80,0,3.7f,.8f,230,"void",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,3,.8f,0,"tentacle");
    default:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?110:80,0,sniper?2.2f:3.2f,sniper?.7f:.5f,sniper?320:245,"organic"):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
   }
  }
  // Closing inside the firing distance never orders a retreat.
  public static float Radial(RegionalEnemyProfile p,float distance)=>p.Range<=0||distance>p.Range?1:0;
  public static float Telegraph(float remaining,float window)=>window<=0?0:Math.Max(0,Math.Min(1,1-remaining/window));
 }
}
