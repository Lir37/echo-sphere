using EchoSphere.Core;
internal static class RegionalContentChecks
{
 internal static int Run()
 {
  int n=0;void Check(bool v,string why){n++;if(!v)throw new Exception("Regional content: "+why);}
  var contracts=new HashSet<string>();var lore=new HashSet<string>();
  for(int i=0;i<6;i++)
  {
   var region=(EchoRegionId)i;var list=CampaignRegionContent.GetContracts(region);Check(list.Count==3,"three contracts per region");
   foreach(var c in list){Check(contracts.Add(c.Id),"unique contract ID");Check(CampaignRegionContent.Find(region,c.Id)==c,"valid route contract");Check(c.DescriptionRu.Length>10&&c.DescriptionEn.Length>10,"localized contract rule");if(i>=2)Check(c.Rule!=RegionContractRule.None,"new contract changes actual rule");}
   Check(CampaignRegionContent.Find(region,"bogus")==null,"cross-region/unrecognized contract rejected");
   var p=new CampaignRegionProgress{GeometryMask=0x7f,PeakSpheres=4,LevelSevenFamilies=3,PoiMask=127,DominantSeconds=120};Check(CampaignRegionContent.MasteryMask(region,p)==7,"all three objectives achievable");
   var copy=new CampaignRegionProgress();RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(p),typeof(CampaignRegionProgress),copy);Check(CampaignRegionContent.MasteryMask(region,copy)==7,"mastery snapshot roundtrip");
   Check(CampaignRegionContent.MasteryDescriptions(region,true).Length==3&&CampaignRegionContent.MasteryDescriptions(region,false).Length==3,"three localized objectives");
   var independent=new[]{new CampaignRegionProgress{GeometryMask=254},new CampaignRegionProgress{PoiMask=127},new CampaignRegionProgress{DominantSeconds=120},new CampaignRegionProgress{LevelSevenFamilies=3},new CampaignRegionProgress{PeakSpheres=4}};
   int active=0,combined=0;foreach(var objectiveCase in independent){int mask=CampaignRegionContent.MasteryMask(region,objectiveCase);if(mask!=0){active++;Check((mask&(mask-1))==0,"each mastery uses a distinct objective");combined|=mask;}}Check(active==3&&combined==7,"three independent mastery objectives");
   var entries=CampaignRegionLore.All.Where(x=>x.Region==region).ToArray();Check(entries.Length==5,"five lore entries per region");foreach(var e in entries){Check(lore.Add(e.Id),"stable unique lore ID");Check(e.Ru.Length>30&&e.En.Length>30,"authored bilingual lore");}
   for(int step=0;step<100;step++){string role=CampaignRegionContent.SelectSupport(region,step/100f,step*240);Check(!string.IsNullOrEmpty(role),"systemic support mix exists");}
  }
  var tooMany=new CampaignRegionProgress{PeakSpheres=5,GeometryMask=127,DominantSeconds=120};Check((CampaignRegionContent.MasteryMask(EchoRegionId.ResonanceBasin,tooMany)&4)==0,"last-second removal cannot earn four-sphere mastery");
  Check(CampaignRegionContent.PoiTypes.Length==7&&CampaignRegionContent.PoiTypes.Distinct().Count()==7,"seven actual POI identifiers");for(int i=0;i<7;i++)Check(CampaignRegionContent.PoiIndex(CampaignRegionContent.PoiTypes[i])==i,"POI discovery bit mapping");
  Check(!ConvergenceRules.Unlocked(_=>false)&&!ConvergenceRules.Unlocked(r=>r!=EchoRegionId.NullArchive)&&ConvergenceRules.Unlocked(_=>true),"all six stabilizations required");
  var ecologies=new HashSet<EchoRegionId>();for(int t=0;t<1500;t++){ecologies.Add(ConvergenceRules.Primary(t));Check(ConvergenceRules.SpawnRegion(t,.5f)==ConvergenceRules.Primary(t),"initial ecology rotation is pure");}Check(ecologies.Count==6,"all six ecologies before25min");
  foreach(int t in new[]{1500,2400,3600}){var mix=new HashSet<EchoRegionId>();for(int j=0;j<100;j++)mix.Add(ConvergenceRules.SpawnRegion(t,j/100f));Check(mix.Count==(t==1500?3:t==2400?4:6),"mixed ecology escalation");}
  var state=new ConvergenceState();int bosses=0;for(int t=0;t<=7200;t++){var boss=ConvergenceRules.Advance(state,t,false);if(boss!=null){bosses++;Check(BossCombatRules.IsKnownBossType(boss),"implemented convergence boss family");if(bosses==1)Check(t==1500,"major event at25min");if(bosses==2)Check(t==2400,"final event at40min");}var restored=new ConvergenceState();RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(state),typeof(ConvergenceState),restored);Check(restored.MajorEvents==state.MajorEvents&&restored.NextEscalationAt==state.NextEscalationAt&&restored.EscalationBosses==state.EscalationBosses,"boss schedule roundtrip");Check(ConvergenceRules.Advance(restored,t,true)==null,"active boss blocks duplicate event");}
  Check(bosses>20,"post40 escalation continues");Console.WriteLine("Regional content: "+n+" checks PASS");return n;
 }
}
