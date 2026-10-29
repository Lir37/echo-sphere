using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class LivePauseStatsAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();var policy=t.GetField("_runEconomy",F);var old=policy.GetValue(r);var inventory=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var before=inventory.ToArray();var spheres=(List<SphereAttackAgent>)t.GetField("_spheres",F).GetValue(r);var original=spheres.ToArray();var levels=(Dictionary<SphereId,int>)t.GetField("_sphereLevels",F).GetValue(r);var oldLevels=new Dictionary<SphereId,int>(levels);var created=new List<GameObject>();var player=(Transform)t.GetField("_player",F).GetValue(r);int checks=0;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  string Round(double v)=>Math.Floor(v+.5d).ToString("0",CultureInfo.InvariantCulture);
  try{
   spheres.Clear();levels.Clear();inventory.Clear();inventory.AddRange(new[]{"sniper_scope","sniper_crown","chain_crown","chain_battery","shotgun_shell","aura_mist","folded_core","overclock","zero_sphere","formation_compass"});policy.SetValue(r,RunEconomyRules.Create(1));
   foreach(var definition in GameCatalog.Spheres){levels[definition.Id]=1;var go=new GameObject("Live pause stats acceptance");created.Add(go);var sphere=go.AddComponent<SphereAttackAgent>();sphere.Initialize(r,player,definition.Id,12);spheres.Add(sphere);}
   var ui=r.ReadNativePauseStatsState();
   foreach(var sphere in spheres){string id=sphere.Type.ToString().ToLowerInvariant();var card=ui.Spheres.Single(x=>x.Id==id);Check(card.OnField,"Missing placed card "+id);float damage=r.GetSourceSphereAttackDamage(sphere),interval=sphere.EffectiveAttackInterval;float dps=sphere.GetSourceDpsEstimate(damage,interval);
    Check(card.Metrics[0].Value.Split('/')[0]==Round(damage),"Damage differs from simulation: "+id);
    Check(card.Metrics[1].Value==interval.ToString("F2",CultureInfo.InvariantCulture)+"с","Cadence differs from attack timer: "+id);
    Check(card.Metrics[2].Value==Round(sphere.AttackRange/SpherePlacementRules.SourcePixelsToWorld),"Range differs from targeting: "+id);
    Check(card.Metrics[3].Value==dps.ToString("F1",CultureInfo.InvariantCulture),"DPS differs from agent estimator: "+id);
   }
   var result=new{result="PASS",checks,sphereFamilies=10,scope="Actual pause cards with modern mixed artifact inventory match each live agent damage,cadence,target range and DPS estimator; no preferences writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/live-pause-stats-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{policy.SetValue(r,old);inventory.Clear();inventory.AddRange(before);spheres.Clear();spheres.AddRange(original);levels.Clear();foreach(var pair in oldLevels)levels[pair.Key]=pair.Value;foreach(var go in created)UnityEngine.Object.DestroyImmediate(go);}
 }
}
