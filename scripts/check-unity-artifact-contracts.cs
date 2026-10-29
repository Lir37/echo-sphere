using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
public static class ArtifactContractsAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var t=r.GetType();var inventory=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var original=inventory.ToArray();var policy=t.GetField("_runEconomy",F);var old=policy.GetValue(r);var go=new GameObject("Artifact contract fixture");int checks=0;
  void Near(float actual,float expected,string name){checks++;if(Math.Abs(actual-expected)>.0001f)throw new Exception(name+": "+actual+" expected "+expected);}
  float Family(SphereId type)=>(float)t.GetMethod("GetArtifactSphereFamilyDamageMultiplier",F).Invoke(r,new object[]{type});
  try{
   inventory.Clear();policy.SetValue(r,RunEconomyRules.Create(1));float baseline=r.SphereDamageMultiplier;inventory.Add("sniper_scope");Near(Family(SphereId.Sniper),1.1f,"scope sniper family");Near(Family(SphereId.Standard),1,"scope does not boost standard");Near(r.SphereDamageMultiplier,baseline,"scope no global multiplier");
   inventory.Clear();inventory.Add("sniper_crown");Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Sniper),1.1f,"crown sniper range");Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Standard),1,"crown standard range unchanged");Near(r.SphereDamageMultiplier,baseline,"crown no global damage");
   var sphere=go.AddComponent<EchoSphere.Runtime.SphereAttackAgent>();var st=sphere.GetType();st.GetField("_runtime",F).SetValue(sphere,r);st.GetField("_type",F).SetValue(sphere,SphereId.Chain);st.GetField("_sourceModifiers",F).SetValue(sphere,r.GetSourceSphereModifiers(SphereId.Chain,1));
   inventory.Clear();float dps=sphere.GetSourceDpsEstimate(10,1);inventory.Add("chain_crown");Near(sphere.GetSourceDpsEstimate(10,1),dps+7,"actual chain DPS includes one secondary jump");Near(r.GetArtifactChainExtraJumps(),1,"chain attack loop bonus");
   policy.SetValue(r,null);Near(sphere.GetSourceDpsEstimate(10,1),dps,"legacy chain unchanged");inventory.Clear();inventory.Add("sniper_scope");Near(Family(SphereId.Sniper),1,"legacy sniper family unchanged");Near(r.SphereDamageMultiplier,baseline*1.06f,"legacy scope keeps old global multiplier");
   policy.SetValue(r,RunEconomyRules.Create(1));inventory.Clear();inventory.Add("chain_battery");Near(Family(SphereId.Chain),1.1f,"battery boosts chain");Near(Family(SphereId.Standard),1,"battery no standard bonus");Near(r.SphereDamageMultiplier,baseline,"battery no hidden global");
   inventory.Clear();inventory.Add("shotgun_shell");Near(Family(SphereId.Shotgun),1.1f,"shell boosts shotgun");Near(Family(SphereId.Sniper),1,"shell no sniper bonus");
   inventory.Clear();inventory.Add("aura_mist");Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Aura),1.1f,"mist boosts aura radius");Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Standard),1,"mist not standard radius");
   inventory.Clear();inventory.Add("folded_core");Near(r.SphereDamageMultiplier,baseline*1.08f,"folded exact8");inventory.Clear();inventory.Add("overclock");Near(r.SphereDamageMultiplier,baseline*.95f,"overclock cost");
   inventory.Clear();inventory.Add("vampire_ring");var cards=r.ReadNativePauseArtifactsState();Near(cards.Artifacts[0].Description.Contains("5 HP")?1:0,1,"live clarified card");policy.SetValue(r,null);cards=r.ReadNativePauseArtifactsState();Near(cards.Artifacts[0].Description.Contains("5 HP")?1:0,0,"legacy card preserved");
   var result=new{result="PASS",checks,scope="Actual runtime family damage/range/global multipliers and SphereAttackAgent DPS; legacy isolation; no PlayerPrefs writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/artifact-contracts-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{inventory.Clear();inventory.AddRange(original);policy.SetValue(r,old);UnityEngine.Object.DestroyImmediate(go);}
 }
}
