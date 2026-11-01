using System;
namespace EchoSphere.Core {
 public enum RegionalMotion { Chase, Orbit, Hold, Skirmish, Lunge, Flank }
 public readonly struct RegionalEnemyProfile {
  public readonly RegionalMotion Motion; public readonly float Range,Minimum,Interval,Windup,Speed; public readonly int Pellets; public readonly string Shot;
  public RegionalEnemyProfile(RegionalMotion motion,float range,float minimum,float interval,float windup,float speed,string shot,int pellets=1){Motion=motion;Range=range;Minimum=minimum;Interval=interval;Windup=windup;Speed=speed;Shot=shot;Pellets=pellets;}
 }
 public static class RegionalEnemyRules {
  public static RegionalEnemyProfile Get(int region,string role){
   bool sniper=role=="sniper",ranged=sniper||role=="ranged",heavy=role=="tank_guard"||role=="anchor",support=role=="healer"||role=="disruptor";
   switch(region){
    case 1:return new RegionalEnemyProfile(sniper||heavy?RegionalMotion.Hold:RegionalMotion.Orbit,sniper?490:heavy?350:support?400:280,sniper?370:heavy?250:180,sniper?3.8f:heavy?3.5f:2.8f,sniper?.85f:.55f,sniper?350:250,"fleet",heavy?3:1);
    case 2:return ranged||role=="bomber"||role=="corruptor"?new RegionalEnemyProfile(RegionalMotion.Skirmish,260,160,4,.7f,190,"spore",3):new RegionalEnemyProfile(RegionalMotion.Lunge,0,0,3.2f,.65f,0,"bite");
    case 3:return new RegionalEnemyProfile(heavy?RegionalMotion.Hold:RegionalMotion.Skirmish,sniper?460:330,heavy?200:240,sniper?4:3.6f,.9f,280,"crystal",heavy?3:1);
    case 4:return new RegionalEnemyProfile(RegionalMotion.Orbit,220,120,2.9f,.65f,310,"arc",2);
    case 5:return ranged||support?new RegionalEnemyProfile(RegionalMotion.Flank,340,220,3.7f,.8f,230,"void",2):new RegionalEnemyProfile(RegionalMotion.Flank,0,0,3,.8f,0,"tentacle");
    default:return ranged?new RegionalEnemyProfile(RegionalMotion.Hold,sniper?440:340,240,sniper?2.2f:3.2f,sniper?.7f:.5f,sniper?320:245,"organic"):new RegionalEnemyProfile(RegionalMotion.Chase,0,0,0,0,0,"");
   }
  }
  public static float Radial(RegionalEnemyProfile p,float distance){if(p.Range<=0)return 1;if(distance<p.Minimum)return -1;if(distance>p.Range)return 1;return 0;}
  public static float Telegraph(float remaining,float window)=>window<=0?0:Math.Max(0,Math.Min(1,1-remaining/window));
 }
}
