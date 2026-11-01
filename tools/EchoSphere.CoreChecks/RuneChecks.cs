using EchoSphere.Core;
internal static class RuneChecks
{
 internal static int Run(){int checks=0;void A(bool v,string why){checks++;if(!v)throw new Exception("Rune: "+why);}
  for(int i=0;i<10000;i++){float roll=i/10000f;A(RuneRules.Pick(roll,false)!="runic_cell","ordinary pool excludes meta cell");A(!string.IsNullOrEmpty(RuneRules.Name(RuneRules.Pick(roll,true),false)),"all special rolls named");}
  A(RuneRules.DropChance(true,true)==1&&RuneRules.DropChance(false,true)==.35f&&RuneRules.DropChance(false,false)==0,"existing drop cadence preserved");
  var s=new RuneRunState();A(!RuneRules.Activate(s,"runic_cell",false,0,0)&&s.CellsCollected==0,"ordinary cell cannot credit");
  A(RuneRules.Activate(s,"runic_cell",true,0,0)&&RuneRules.CellReward(s)==5,"special reward");
  A(RuneRules.Activate(s,"overdrive",false,0,0)&&RuneRules.AttackSpeed(s)==1.35f,"attack speed8s");
  RuneRules.Activate(s,"harvest",false,0,0);RuneRules.Activate(s,"fortify",false,0,0);RuneRules.Activate(s,"gravity",false,3,4);
  A(RuneRules.Magnet(s)==2&&RuneRules.DamageTaken(s)==.65f&&s.Gravity==4&&s.GravityX==3&&s.GravityY==4,"tactical timers");
  RuneRules.Tick(s,2);RuneRules.Activate(s,"overdrive",false,0,0);A(s.Overdrive==8,"refresh does not stack multiplicatively");
  RuneRules.Activate(s,"echo",false,0,0);A(!RuneRules.ConsumeEcho(s,false)&&s.EchoReady,"empty cast does not waste echo");
  var restored=(RuneRunState)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(s),typeof(RuneRunState));A(restored.EchoReady&&restored.Gravity==2&&restored.Overdrive==8&&RuneRules.CellReward(restored)==5,"all effects roundtrip");
  A(RuneRules.ConsumeEcho(restored,true)&&!RuneRules.ConsumeEcho(restored,true),"one successful cast only");
  RuneRules.Tick(s,float.NaN);A(s.Overdrive==8,"bad dt ignored");RuneRules.Tick(s,10);A(RuneRules.AttackSpeed(s)==1&&RuneRules.Magnet(s)==1&&RuneRules.DamageTaken(s)==1&&s.Gravity==0,"expiry restores all factors");
  A(RuneRules.AttackSpeed(null)==1&&RuneRules.Magnet(null)==1&&RuneRules.DamageTaken(null)==1&&!RuneRules.ConsumeEcho(null,true),"legacy neutral");
  s.CellsCollected=int.MaxValue;A(RuneRules.CellReward(s)==int.MaxValue,"malformed reward saturates");RuneRules.Activate(s,"runic_cell",true,0,0);A(s.CellsCollected==int.MaxValue/5,"cell count saturation");
  Console.WriteLine("Runes: "+checks+" checks PASS");return checks;
 }
}
