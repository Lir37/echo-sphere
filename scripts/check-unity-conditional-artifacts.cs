using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class ConditionalArtifactAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();var inventory=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var original=inventory.ToArray();var spheres=(List<SphereAttackAgent>)t.GetField("_spheres",F).GetValue(r);var oldSpheres=spheres.ToArray();var policy=t.GetField("_runEconomy",F);var old=policy.GetValue(r);var net=t.GetField("_networkState",F);var oldNet=net.GetValue(r);var levels=(Dictionary<SphereId,int>)t.GetField("_sphereLevels",F).GetValue(r);var oldLevels=new Dictionary<SphereId,int>(levels);var created=new List<GameObject>();int checks=0;
  void Near(float actual,float expected,string name){checks++;if(Math.Abs(actual-expected)>.0001f)throw new Exception(name+": "+actual+" expected "+expected);}
  SphereAttackAgent Add(float x,float y,SphereId id,int level){var go=new GameObject("Conditional artifact fixture");created.Add(go);go.transform.position=new Vector3(x,y);var s=go.AddComponent<SphereAttackAgent>();s.GetType().GetField("_type",F).SetValue(s,id);s.GetType().GetField("_progressionLevel",F).SetValue(s,level);spheres.Add(s);return s;}
  float Damage(string id,SphereAttackAgent s){inventory.Clear();inventory.Add(id);return (float)t.GetMethod("GetArtifactSphereDamageMultiplier",F).Invoke(r,new object[]{s});}
  try{
   spheres.Clear();levels.Clear();levels[SphereId.Standard]=7;levels[SphereId.Sniper]=1;policy.SetValue(r,RunEconomyRules.Create(1));inventory.Clear();float baseline=r.SphereDamageMultiplier;
   var a=Add(0,0,SphereId.Standard,7);var b=Add(1,0,SphereId.Standard,1);var c=Add(30,0,SphereId.Standard,1);var d=Add(0,1,SphereId.Sniper,1);
   var n=new SphereNetworkState();var triangle=new SphereNetworkShape(SphereNetworkFormation.Triangle,1,new List<int>{0,1,3});typeof(SphereNetworkState).GetProperty("DominantFormation",F).SetValue(n,triangle);net.SetValue(r,n);
   Near(Damage("resonance_core",a),1.12f,"near owner");Near(Damage("resonance_core",b),1.12f,"near same-family copy");Near(Damage("resonance_core",c),1,"far same-family copy");Near(r.SphereDamageMultiplier,baseline,"no unconditional resonance core damage");
   Near(Damage("triangle_circuit",a),1.15f,"triangle member");Near(Damage("triangle_circuit",c),1,"outside triangle same-family copy");
   Near(Damage("relay_matrix",a),1,"VII donor does not buff itself");Near(Damage("relay_matrix",b),1.1f,"other node gets relay");b.GetType().GetField("_progressionLevel",F).SetValue(b,7);Near(Damage("relay_matrix",a),1.1f,"two VII donors buff one another");
   Near(Damage("network_coil",a),1.08f,"two types coil");d.gameObject.SetActive(false);Near(Damage("network_coil",a),1,"one type coil off");Near(Damage("fivefold_resonance",a),1.04f,"one distinct type still counts");d.gameObject.SetActive(true);
   Near(Damage("unified_mind",a),1.21f,"highest VII gives21 not hidden global16");Near(r.SphereDamageMultiplier,baseline,"unified no hidden global");
   Near(Damage("zero_sphere",a),1,"zero no hidden family20");Near(r.SphereDamageMultiplier,baseline*1.35f,"zero exactly35 global");
   inventory.Clear();inventory.Add("formation_compass");Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Standard),1.08f,"compass exactly8");policy.SetValue(r,null);Near(r.GetArtifactSphereRadiusMultiplier(SphereId.Standard),1.1664f,"legacy compass unchanged");policy.SetValue(r,RunEconomyRules.Create(1));
   triangle.Active=false;Near(Damage("geometry_die",a),1,"inactive geometry does not qualify");typeof(SphereNetworkState).GetProperty("SecondaryFormation",F).SetValue(n,triangle);typeof(SphereNetworkState).GetProperty("DominantFormation",F).SetValue(n,null);Near(Damage("triangle_circuit",a),1,"inactive secondary does not qualify");triangle.Active=true;Near(Damage("geometry_die",a),1.08f,"active secondary qualifies");
   var result=new{result="PASS",checks,scope="Actual per-node artifact context:near/far same-family copies,triangle membership,other-VII donors,unique type changes,inactive secondary,exact global bonuses and legacy compass;no preferences writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/conditional-artifacts-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{inventory.Clear();inventory.AddRange(original);spheres.Clear();spheres.AddRange(oldSpheres);levels.Clear();foreach(var pair in oldLevels)levels[pair.Key]=pair.Value;policy.SetValue(r,old);net.SetValue(r,oldNet);foreach(var go in created)UnityEngine.Object.DestroyImmediate(go);}
 }
}
