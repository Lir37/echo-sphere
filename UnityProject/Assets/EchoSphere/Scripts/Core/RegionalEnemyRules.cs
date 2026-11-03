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
   // Fleet hulls fight with onboard weapons. Other factions mix melee and firing roles.
   // Distances are source pixels; no firing profile orders retreat.
   switch(region){
    case 1:return new RegionalEnemyProfile(sniper||role=="tank_guard"||role=="anchor"?RegionalMotion.Hold:role=="swarmer"||role=="phase"?RegionalMotion.Flank:RegionalMotion.Orbit,sniper?205:role=="charger"?120:role=="swarmer"?125:role=="tank_guard"?165:150,0,sniper?2.7f:role=="swarmer"?1.6f:2.1f,sniper?.65f:.5f,sniper?370:280,"fleet",role=="bomber"||role=="tank_guard"?3:role=="ranged"?2:1);
    case 2:if(role=="anchor")return new RegionalEnemyProfile(RegionalMotion.Hold,155,0,6,.8f,0,"brood");if(role=="corruptor")return new RegionalEnemyProfile(RegionalMotion.Skirmish,135,0,2.8f,.5f,180,"web");return ranged||role=="bomber"||role=="splitter"?new RegionalEnemyProfile(RegionalMotion.Skirmish,sniper?175:130,0,3.2f,.55f,220,"spore",3):new RegionalEnemyProfile(RegionalMotion.Lunge,0,0,2.8f,.5f,0,"bite");
    case 3:return ranged||role=="tank_guard"?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?200:role=="tank_guard"?125:155,0,sniper?2.7f:2.3f,.55f,310,"crystal",sniper?1:3):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
    case 4:return ranged?new RegionalEnemyProfile(RegionalMotion.Orbit,sniper?190:145,0,2.1f,.5f,340,"arc",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,0,0,0,"");
    case 5:if(role=="phase"||role=="leech")return new RegionalEnemyProfile(RegionalMotion.Flank,170,0,3.6f,.8f,0,"shadow");return ranged||role=="corruptor"?new RegionalEnemyProfile(RegionalMotion.Flank,sniper?195:145,0,2.6f,.55f,260,"void",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,2.8f,.7f,0,"tentacle");
    default:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?200:145,0,sniper?1.9f:2.7f,sniper?.65f:.5f,sniper?350:270,"organic"):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
   }
  }
  // Closing inside the firing distance never orders a retreat.
  public static float Radial(RegionalEnemyProfile p,float distance)=>p.Range<=0||distance>p.Range?1:0;
  public static float Telegraph(float remaining,float window)=>window<=0?0:Math.Max(0,Math.Min(1,1-remaining/window));
 }
}
