using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class NetworkArtifactAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();var names=new[]{"_runEconomy","_networkState","_mainMenu","_paused","_userPaused"};var old=new Dictionary<string,object>();foreach(var name in names)old[name]=t.GetField(name,F).GetValue(r);
  var inventory=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var before=inventory.ToArray();var spheres=(List<SphereAttackAgent>)t.GetField("_spheres",F).GetValue(r);var original=spheres.ToArray();var player=(Transform)t.GetField("_player",F).GetValue(r);var created=new List<GameObject>();int checks=0;
  void Near(float a,float b,string why){checks++;if(Math.Abs(a-b)>.0002f)throw new Exception(why+": "+a+" expected "+b);}
  try{
   spheres.Clear();inventory.Clear();t.GetField("_runEconomy",F).SetValue(r,RunEconomyRules.Create(2));t.GetField("_mainMenu",F).SetValue(r,false);t.GetField("_paused",F).SetValue(r,false);t.GetField("_userPaused",F).SetValue(r,false);
   for(int i=0;i<7;i++){var go=new GameObject("Network artifact fixture");created.Add(go);go.transform.position=i==6?new Vector3(30,0):new Vector3(Mathf.Cos(i*Mathf.PI/3),Mathf.Sin(i*Mathf.PI/3));var s=go.AddComponent<SphereAttackAgent>();s.Initialize(r,player,i==0?SphereId.Orbital:SphereId.Standard,12);spheres.Add(s);}
   var state=new SphereNetworkState();for(int i=0;i<6;i++)state.Links.Add(new SphereNetworkLink(i,(i+1)%6,1));var ring=new SphereNetworkShape(SphereNetworkFormation.Ring,1,new List<int>{0,1,2,3,4,5});typeof(SphereNetworkState).GetProperty("DominantFormation",F).SetValue(state,ring);t.GetField("_networkState",F).SetValue(r,state);
   inventory.Add("ring_engine");Near(r.GetFormationBonus(SphereNetworkFormation.Ring,0),1.12f,"ring engine numeric strength");Near(r.GetFormationBonus(SphereNetworkFormation.Triangle,0),0,"other formation absent");Near(r.SphereDamageMultiplier,GetBaseDamage(),"engine no global damage");
   float projected=(float)(double)t.GetMethod("NativeLiveOrbitalNetworkMultiplier",F).Invoke(r,new object[]{spheres[0]});Near(projected,1+.12f*1.12f+.06f,"ring/links radius composition");spheres[0].SimulationTick(0);
   var first=spheres[0].GetOrbitalElementAt(0);Near(Vector2.Distance(first.position,spheres[0].transform.position),25f*1.68f*(r.GetOrbitalLiveRadiusPixels(1)/130f)*projected*SpherePlacementRules.SourcePixelsToWorld,"orbit collision positions use projected radius");
   inventory.Clear();inventory.Add("echo_thread");float Damage(SphereAttackAgent s)=>(float)t.GetMethod("GetArtifactSphereDamageMultiplier",F).Invoke(r,new object[]{s});Near(Damage(spheres[1]),1.05f,"linked thread");Near(Damage(spheres[6]),1,"isolated same-type thread off");
   inventory.Clear();inventory.Add("signal_knot");Near(r.NetworkRecoverySpeed,1.05f,"recovery speed");spheres[1].ApplyNetworkDisable(2);spheres[1].SimulationTick(.5f);Near((float)spheres[1].GetType().GetProperty("SourceProjectionNetworkDisabledTimer",F).GetValue(spheres[1]),1.475f,"live disable timer accelerated");
   t.GetField("_runEconomy",F).SetValue(r,null);Near(r.NetworkRecoverySpeed,1,"legacy recovery");spheres[1].ApplyNetworkDisable(2);spheres[1].SimulationTick(.5f);Near((float)spheres[1].GetType().GetProperty("SourceProjectionNetworkDisabledTimer",F).GetValue(spheres[1]),1.5f,"legacy timer unchanged");
   inventory.Clear();inventory.Add("ring_engine");Near(r.GetFormationBonus(SphereNetworkFormation.Ring,0),1,"legacy ring engine unchanged");t.GetField("_runEconomy",F).SetValue(r,RunEconomyRules.Create(2));ring.Active=false;typeof(SphereNetworkState).GetProperty("DominantFormation",F).SetValue(state,null);typeof(SphereNetworkState).GetProperty("SecondaryFormation",F).SetValue(state,ring);Near(r.GetFormationBonus(SphereNetworkFormation.Ring,0),0,"disabled secondary grants nothing");ring.Active=true;Near(r.GetFormationBonus(SphereNetworkFormation.Ring,0),.56f,"secondary engine remains half strength");
   var result=new{result="PASS",checks,scope="Actual geometry bonuses,linked-node damage,network disable tick,orbital collision-anchor radius vs render multiplier,and legacy isolation;no prefs writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/network-artifact-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
   float GetBaseDamage(){var saved=inventory.ToArray();inventory.Clear();float value=r.SphereDamageMultiplier;inventory.AddRange(saved);return value;}
  }finally{foreach(var name in names)t.GetField(name,F).SetValue(r,old[name]);inventory.Clear();inventory.AddRange(before);spheres.Clear();spheres.AddRange(original);foreach(var go in created)UnityEngine.Object.DestroyImmediate(go);}
 }
}
