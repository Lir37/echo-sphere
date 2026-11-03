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
    case 1:return new RegionalEnemyProfile(sniper?RegionalMotion.Hold:role=="tank_guard"||role=="anchor"?RegionalMotion.Hold:RegionalMotion.Orbit,sniper?105:role=="charger"?60:role=="swarmer"?65:role=="tank_guard"?90:80,0,sniper?3.2f:role=="swarmer"?1.8f:2.4f,sniper?.65f:.5f,sniper?350:250,"fleet",role=="bomber"||role=="tank_guard"?3:role=="ranged"?2:1);
    case 2:if(role=="corruptor")return new RegionalEnemyProfile(RegionalMotion.Skirmish,80,0,3,.5f,150,"web");return ranged||role=="bomber"||role=="corruptor"||role=="splitter"?new RegionalEnemyProfile(RegionalMotion.Skirmish,sniper?105:75,0,4,.7f,190,"spore",3):new RegionalEnemyProfile(RegionalMotion.Lunge,0,0,3.2f,.65f,0,"bite");
    case 3:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?110:85,0,sniper?3:2.6f,.5f,280,"crystal",sniper?1:3):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
    case 4:return ranged?new RegionalEnemyProfile(RegionalMotion.Orbit,sniper?100:70,0,2.4f,.5f,310,"arc",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,0,0,0,"");
    case 5:return ranged||role=="corruptor"?new RegionalEnemyProfile(RegionalMotion.Flank,sniper?105:80,0,3f,.5f,230,"void",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,3,.8f,0,"tentacle");
    default:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?110:80,0,sniper?2.2f:3.2f,sniper?.7f:.5f,sniper?320:245,"organic"):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
   }
  }
  // Closing inside the firing distance never orders a retreat.
  public static float Radial(RegionalEnemyProfile p,float distance)=>p.Range<=0||distance>p.Range?1:0;
  public static float Telegraph(float remaining,float window)=>window<=0?0:Math.Max(0,Math.Min(1,1-remaining/window));
 }
}
