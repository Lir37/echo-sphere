using System;
namespace EchoSphere.Core
{
 [Serializable] public sealed class ConvergenceState { public int MajorEvents,EscalationBosses;public float NextEscalationAt=2550f; }
 public static class ConvergenceRules
 {
  public static bool Unlocked(Func<EchoRegionId,bool> stable){if(stable==null)return false;for(int i=0;i<6;i++)if(!stable((EchoRegionId)i))return false;return true;}
  public static EchoRegionId Primary(float time)=>(EchoRegionId)((int)(Math.Max(0,time)/250)%6);
  public static EchoRegionId SpawnRegion(float time,float roll)
  {
   int first=(int)Primary(time);if(time<1500)return(EchoRegionId)first;
   int count=time<2400?3:Math.Min(6,4+(int)((time-2400)/600));
   int slot=Math.Min(count-1,(int)(Math.Max(0,Math.Min(.999999f,roll))*count));return(EchoRegionId)((first+slot)%6);
  }
  public static string Advance(ConvergenceState state,float time,bool bossActive)
  {
   if(state==null||bossActive||float.IsNaN(time)||float.IsInfinity(time))return null;
   if(state.MajorEvents==0&&time>=1500){state.MajorEvents=1;return "conductor";}
   if(state.MajorEvents==1&&time>=2400){state.MajorEvents=2;return "stella_warden";}
   if(state.MajorEvents>=2&&time>=state.NextEscalationAt){state.NextEscalationAt+=150;return BossCombatRules.GetTypeForDefeatedCount(state.EscalationBosses++);}
   return null;
  }
 }
}
