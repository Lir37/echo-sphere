using EchoSphere.Core;
internal static class CampaignBossChecks
{
 internal static int Run()
 {
  int checks=0;void Check(bool v,string why){checks++;if(!v)throw new Exception("Campaign bosses: "+why);}
  for(int boss=0;boss<BossCombatRules.BossTypeCount;boss++)
  {
   string type=BossCombatRules.GetTypeForDefeatedCount(boss);var attacks=new HashSet<CampaignBossAttack>();
   for(int i=0;i<3;i++)
   {
    var a=CampaignBossRules.Pattern(type,i,0);var b=CampaignBossRules.Pattern(type,i,1);attacks.Add(a.Attack);
    Check(a.Windup>=.7f&&a.Cooldown>0&&a.Count>0&&a.Radius>0,"valid telegraphed pattern");
    Check(a.Attack==b.Attack&&b.Cooldown<a.Cooldown,"phase changes real attack cadence");
   }
   Check(attacks.Count==3,"three distinct attacks: "+type);
   var crossing=new CampaignBossState{Remaining=.001f};
   Check(CampaignBossRules.Advance(type,crossing,.01f,1)==CampaignBossEvent.Telegraph,"first phase windup");
   float remaining=crossing.Remaining;CampaignBossRules.Advance(type,crossing,.01f,.4f);
   Check(crossing.Phase==1&&crossing.AttackPhase==0&&crossing.Remaining<remaining,"phase crossing preserves already warned pattern");
   var state=new CampaignBossState();int warnings=0,hits=0;var seen=new HashSet<int>();
   for(int step=0;step<2400;step++)
   {
    int index=state.AttackIndex;bool warned=state.Telegraphing;
    var signal=CampaignBossRules.Advance(type,state,.05f,step<1200?1f:.4f);
    if(signal==CampaignBossEvent.Telegraph){warnings++;Check(!warned&&state.Remaining>=.7f,"warning before attack");state.AimX=.6f;state.AimY=.8f;}
    if(signal==CampaignBossEvent.Execute){hits++;seen.Add(index);Check(warned&&!state.Telegraphing,"execute consumes warning");}
    // Save in every cooldown/windup/phase and compare the next simulation step.
    var restored=new CampaignBossState();RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(state),typeof(CampaignBossState),restored);
    Check(restored.AimX==state.AimX&&restored.AimY==state.AimY,"locked aim survives restore");
    var nextA=CampaignBossRules.Advance(type,state,.05f,.9f);var nextB=CampaignBossRules.Advance(type,restored,.05f,.9f);
    Check(nextA==nextB&&state.Remaining==restored.Remaining&&state.Phase==restored.Phase&&state.AttackIndex==restored.AttackIndex,"restored next step deterministic");
    // Both states advanced identically; count events on this half-step as well.
    if(nextA==CampaignBossEvent.Telegraph)warnings++;
    if(nextA==CampaignBossEvent.Execute){hits++;seen.Add(index);}
   }
   Check(seen.Count==3&&hits>6&&warnings>=hits,"complete repeated attack cycle");
   Check(state.Phase==1,"phase remains after healing");float timer=state.Remaining;
   Check(CampaignBossRules.Advance(type,state,0,1)==CampaignBossEvent.None&&state.Remaining==timer,"zero time cannot fire");
   Check(CampaignBossRules.Advance(type,state,float.NaN,1)==CampaignBossEvent.None&&state.Remaining==timer,"invalid time cannot fire");
  }
  Console.WriteLine("Campaign bosses: "+checks+" checks PASS");return checks;
 }
}
