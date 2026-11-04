using EchoSphere.Core;
using System.Text.Json;
internal static class SurvivalSensitivity {
 internal static void Run(string path) {
  int scenarios=0;float minReduction=1,maxReduction=0;
  var ring=ArtifactRewardRules.Get("vampire_ring");
  var samples=new List<object>();
  foreach(var region in Enum.GetValues<EchoRegionId>())
  foreach(string difficulty in new[]{"easy","normal","hard","nightmare"})
  foreach(var archetype in new[]{EnemyArchetype.Normal,EnemyArchetype.Fast,EnemyArchetype.Tank})
  foreach(int wave in new[]{1,10,20,40,60})
  foreach(float seconds in new[]{0f,300,600,900,1200,1800,3600})
  foreach(float killsPerSecond in new[]{.5f,2,5,10})
  foreach(float overkill in new[]{1f,2,5,10}) {
   var profile=EnemySpawnRules.GetProfile(archetype);var d=DifficultyRules.GetProfile(difficulty);var pocket=RegionPocketRules.GetAt(region,0,0);
   float hp=(profile.HpBase+wave*profile.HpPerWave)*d.EnemyHpMultiplier*pocket.Hp;
   float damage=(profile.DamageBase+wave*profile.DamagePerWave)*d.EnemyDamageMultiplier*pocket.Damage;
   float oldHealing=(hp*overkill*.03f+5)*killsPerSecond;
   float newHealing=hp*ArtifactGameplayRules.Effect(ring,"vampire",true)*killsPerSecond;
   float reduction=1-newHealing/oldHealing;minReduction=Math.Min(minReduction,reduction);maxReduction=Math.Max(maxReduction,reduction);
   if(!(newHealing<oldHealing)||!(RegionalCombatTuning.HealthTimeMultiplier(seconds)>=1)||!(RegionalCombatTuning.DamageTimeMultiplier(seconds)>=1))throw new Exception("Survival balance invariant");
   scenarios++;
   if(region==EchoRegionId.ResonanceBasin&&difficulty=="normal"&&archetype==EnemyArchetype.Normal&&wave==40&&killsPerSecond==5&&overkill==5)
    samples.Add(new{seconds,wave,hp,damage,oldHealingPerSecond=oldHealing,newHealingPerSecond=newHealing,hpWithTime=hp*RegionalCombatTuning.HealthTimeMultiplier(seconds),damageWithTime=damage*RegionalCombatTuning.DamageTimeMultiplier(seconds),newHealingWithTime=hp*RegionalCombatTuning.HealthTimeMultiplier(seconds)*.01f*killsPerSecond});
  }
  var report=new{model="Deterministic sensitivity sweep, NOT full combat/player survival simulation",scenarios,regions=6,difficulties=4,archetypes=3,waves=new[]{1,10,20,40,60},timesSeconds=new[]{0,300,600,900,1200,1800,3600},minHealingReductionPercent=minReduction*100,maxHealingReductionPercent=maxReduction*100,assumptions="Same enemy HP and kill throughput for isolated ring comparison; overkill factor 1/2/5/10; .5/2/5/10 kills per second; no ability/mutation lifesteal, shield, hit frequency, player evasion, HP overheal or victory probabilities simulated. Separate samples show additive time scaling on existing wave profiles. Existing damage formulas/catalog values are used; clocks and waves are independent inputs.",samples};
  Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(new{scenarios,report.minHealingReductionPercent,report.maxHealingReductionPercent,path}));
 }
}