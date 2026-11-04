using EchoSphere.Core;
using System.Text.Json;
// Economy sensitivity model, NOT combat/AI/device simulation.
// Uses real waves, archetype/elite XP, prices, stock, reward pools and save codec.
// Assumed per-enemy lifetime/collection probability represent player throughput.
record Profile(string Name,float Lifetime,float Collection);
record Event(double Time,int Echo,bool Elite,bool Boss);
record Row(int Seed,string Region,string Profile,string Policy,bool Exploration,int Kills,int Echo,int Bought,int Chests,int Boss,int Explore,int Artifacts,int Wallet,int Runes,int Cells);
static class Program
{
 static readonly Profile[] Profiles={new("slow",18,.55f),new("regular",8,.80f),new("fast",3,.95f),new("ceiling",.5f,1)};
 static List<Event> Events(uint seed,EchoRegionId region,Profile p)
 {
  var rng=new SeededRng(seed);var events=new List<Event>();double start=3;int wave=0;
  double[] bosses={RegionBossScheduleRules.GetRegularBossTime(region,0)+15,RegionBossScheduleRules.GetRegularBossTime(region,1)+15,1750};int boss=0;
  while(start<1800){wave++;int count=RunBalanceRules.GetEnemiesPerWave(wave);double lastDeath=start;
   for(int i=0;i<count;i++){double spawn=start+i*RunBalanceRules.GetWaveSpawnInterval(wave);if(spawn>=1800)break;
    double death=spawn+p.Lifetime*(.5+rng.NextFloat());lastDeath=Math.Max(lastDeath,death);
    var archetype=EnemySpawnRules.SelectBase(wave,rng.NextFloat());bool elite=EnemySpawnRules.ShouldBeElite(wave,rng.NextFloat());
    int xp=EnemySpawnRules.GetProfile(archetype).XpReward*(elite?5:1);
    bool collect=rng.NextFloat()<p.Collection;
    if(death+5<1800)events.Add(new Event(death+5,collect?xp:0,elite,false));
   }
   while(boss<bosses.Length&&bosses[boss]<=lastDeath+3){events.Add(new Event(bosses[boss],(int)BossCombatRules.GetXpReward(wave),false,true));boss++;}
   start=lastDeath+3;
  }
  while(boss<bosses.Length){events.Add(new Event(bosses[boss],(int)BossCombatRules.GetXpReward(wave),false,true));boss++;}
  return events.OrderBy(e=>e.Time).ToList();
 }
 static Row Run(uint seed,EchoRegionId region,Profile profile,string policy,bool exploration,int pricingVersion)
 {
  var events=Events(seed,region,profile);var state=RunEconomyRules.Create(seed);state.PricingVersion=pricingVersion;var owned=new List<string>();var rewardRng=new SeededRng(seed^0x73513u);var runeRng=new SeededRng(seed^0x8411u);
  int at=0,total=0,bought=0,chests=0,bosses=0,explore=0,runes=0,cells=0;
  void Reward(bool legendary){var choices=ArtifactRewardRules.PickChoices(owned,rewardRng.NextFloat,legendary,3,true);if(choices.Count==0)throw new Exception("empty reward");owned.Add(choices[0].Id);}
  for(int second=0;second<=1800;second++){
   while(at<events.Count&&events[at].Time<=second){var e=events[at++];RunEconomyRules.Collect(state,e.Echo);total+=e.Echo;
    if(e.Boss){Reward(second>=1500);bosses++;}
    if(runeRng.NextFloat()<RuneRules.DropChance(e.Boss,e.Elite)){runes++;if(RuneRules.Pick(runeRng.NextFloat(),true)=="runic_cell")cells++;}
   }
   if(RunEconomyRules.TakeScheduledChest(state,second)){Reward(false);chests++;}
   if(exploration&&(second==600||second==1200)&&RunEconomyRules.TakeExplorationChest(state,second)){Reward(false);explore++;}
   if(RunEconomyRules.AdvanceMerchant(state,second)){
    // Arrive+40 auto-open: actual runtime stock excludes artifacts acquired
    // during the warning, not those owned at arrival.
   }
   if(state.Pending&&!state.Open&&second>=state.ArrivedAt+40){RunEconomyRules.Prepare(state,owned);state.Open=true;
    while(true){var slots=Enumerable.Range(0,state.Offers.Count).Where(i=>RunEconomyRules.CanBuy(state,i,owned)).ToList();if(slots.Count==0)break;
     int slot=policy=="cheap"?slots.OrderBy(i=>state.Offers[i].Price).First():policy=="rarity"?slots.OrderByDescending(i=>ArtifactRewardRules.Get(state.Offers[i].ArtifactId).Rarity).ThenBy(i=>state.Offers[i].Price).First():slots[(int)(rewardRng.NextFloat()*slots.Count)%slots.Count];
     int before=state.Echo,price=state.Offers[slot].Price;if(!RunEconomyRules.TryBuy(state,slot,owned)||state.Echo!=before-price)throw new Exception("bad transaction");bought++;
     if(RunEconomyRules.TryBuy(state,slot,owned))throw new Exception("double charge");
    }
    RunEconomyRules.Close(state);
    state=(RunEconomyState)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(state),typeof(RunEconomyState));
   }
  }
  if(owned.Count!=owned.Distinct().Count()||state.Echo<0||state.CollectedEcho!=total||state.SpentEcho+state.Echo!=total||state.MerchantPurchases!=bought||chests!=10||bosses!=3||explore>2)throw new Exception("economy invariant");
  return new Row((int)seed,region.ToString(),profile.Name,policy,exploration,events.Count,total,bought,chests,bosses,explore,owned.Count,state.Echo,runes,cells);
 }
 static int Quantile(IEnumerable<int> items,double p){var v=items.OrderBy(x=>x).ToArray();return v[(int)Math.Floor((v.Length-1)*p)];}
 static void Main(string[] args)
 {
  if(args.Length>0&&args[0]=="survival"){SurvivalSensitivity.Run(args.Length>1?args[1]:"UnityProject/Library/survival-sensitivity.json");return;}
  int seeds=args.Length>0?int.Parse(args[0]):128;string path=args.Length>1?args[1]:"UnityProject/Library/point6-balance.json";int pricingVersion=args.Length>2?int.Parse(args[2]):2;var rows=new List<Row>();
  foreach(var region in Enum.GetValues<EchoRegionId>())foreach(var profile in Profiles)foreach(var policy in new[]{"cheap","rarity","mixed"})for(uint seed=1;seed<=seeds;seed++)foreach(bool exploration in new[]{false,true})rows.Add(Run(seed,region,profile,policy,exploration,pricingVersion));
  var groups=rows.GroupBy(r=>new{r.Profile,r.Policy,r.Exploration}).Select(g=>new{g.Key,Runs=g.Count(),EchoP50=Quantile(g.Select(x=>x.Echo),.5),BoughtP10=Quantile(g.Select(x=>x.Bought),.1),BoughtP50=Quantile(g.Select(x=>x.Bought),.5),BoughtP90=Quantile(g.Select(x=>x.Bought),.9),ArtifactsP10=Quantile(g.Select(x=>x.Artifacts),.1),ArtifactsP50=Quantile(g.Select(x=>x.Artifacts),.5),ArtifactsP90=Quantile(g.Select(x=>x.Artifacts),.9),TargetShare=g.Count(x=>x.Artifacts>=20&&x.Artifacts<=23)/(double)g.Count(),RunesP50=Quantile(g.Select(x=>x.Runes),.5),SpecialCellsP50=Quantile(g.Select(x=>x.Cells),.5)}).ToArray();
  var report=new{Model="Economy sensitivity, no combat survival/DPS/positions/phone measurements",PricingVersion=pricingVersion,SeedCount=seeds,TotalRuns=rows.Count,Profiles,Assumptions="Normal difficulty; neutral pocket XP; all six regional boss times, 15s regular boss kill delay and final defeated at1750; enemy lifetimes/collection are assumed; 5s loot delay; two discovered POIs at600/1200; no bonus merchant artifacts; no XP/combo income multiplication; exact catalog/prices; same seeds before/after exploration. Rune counts cover defeated enemies including uncollected XP; pickup reach/expiry and tactical use are not simulated; special cells sampled separately, not granted in ordinary campaign.",Groups=groups};
  Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
  string csv=Path.ChangeExtension(path,"csv");File.WriteAllLines(csv,new[]{"seed,region,profile,policy,exploration,kills,echo,bought,chests,boss,explore,artifacts,wallet,runes,specialCells"}.Concat(rows.Select(r=>$"{r.Seed},{r.Region},{r.Profile},{r.Policy},{r.Exploration},{r.Kills},{r.Echo},{r.Bought},{r.Chests},{r.Boss},{r.Explore},{r.Artifacts},{r.Wallet},{r.Runes},{r.Cells}")));
  Console.WriteLine(JsonSerializer.Serialize(new{report.TotalRuns,Groups=groups.Where(g=>g.Key.Exploration)}));Console.WriteLine("Report: "+path);
 }
}
