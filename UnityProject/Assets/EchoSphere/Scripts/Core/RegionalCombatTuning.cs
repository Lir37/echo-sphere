using System;
namespace EchoSphere.Core {
 // Authored campaign tuning; source/legacy spawn profiles are left intact.
 public static class RegionalCombatTuning {
  // Clock multipliers compound the existing wave/difficulty/role stats, without speeding up enemies.
  public static float HealthTimeMultiplier(float seconds)=>1f+.02f*Minutes(seconds);
  public static float DamageTimeMultiplier(float seconds)=>1f+.015f*Minutes(seconds);
  private static float Minutes(float seconds)=>float.IsFinite(seconds)?Math.Max(0,seconds)/60f:0f;
  public const float FirstAttackDelay=.25f, CrystalClosedMultiplier=.55f, CrystalOpenMultiplier=1.20f, CrystalOpenSeconds=1.2f;
  public const int OrdinaryBroodCap=3, BossBroodCap=10, BossBroodPhaseTwoCap=14, GlobalBroodCap=24, FleetGroupSize=3, EnergyLinkCap=6;
  public const float SummonSpeed=135, SummonHpShare=.12f, SummonMinHp=12, SummonMaxHp=90, SummonBodyOffset=.25f, FleetCommandRange=160, MeleeWindupRange=100;
  public const float EnergyRange=160, EnergyWindup=.8f, EnergyInterval=4.5f, EnergyPartnerRetry=.25f, EnergyThickness=10, EnergyDamage=8, ShadowRadius=38;
  public static float SpeedMultiplier(float speed,string role){bool heavy=role=="tank_guard"||role=="anchor",fast=role=="swarmer"||role=="phase"||role=="charger";float gain=heavy?1.45f:fast?1.12f:1.30f,ceiling=heavy?140:fast?220:180;return Math.Max(1,Math.Min(gain,ceiling/Math.Max(1,speed)));}
  private static readonly int[][] Favored={new[]{0,9},new[]{2,7},new[]{4,8},new[]{5,6},new[]{3,1},new[]{1,2,7}};
  public static int EliteIndex(int region,float roll,bool wide){var favored=Favored[Math.Max(0,Math.Min(5,region))];int bonus=wide?1:3,total=10+favored.Length*bonus;float ticket=Math.Max(0,Math.Min(.999999f,float.IsFinite(roll)?roll:.5f))*total;for(int i=0;i<10;i++){int weight=1;foreach(int f in favored)if(f==i){weight+=bonus;break;}ticket-=weight;if(ticket<0)return i;}return 9;}
 }
}
