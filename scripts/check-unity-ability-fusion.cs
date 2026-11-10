using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEngine;using EchoSphere.Runtime;using EchoSphere.Core;using EchoSphere.Presentation;
public static class AbilityFusionCheck
{
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");int checks=0;void A(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  object G(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);void S(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);object C(object o,string n,params object[] a){try{return o.GetType().GetMethods(F).Single(m=>m.Name==n&&m.GetParameters().Length==a.Length).Invoke(o,a);}catch(TargetInvocationException e){throw new Exception(n+": "+e.InnerException,e);}}
  var go=new GameObject("Owned fusion contracts");go.SetActive(false);var r=go.AddComponent<EchoSphereRuntime>();r.enabled=false;go.SetActive(true);var p=new GameObject("Owned fusion player");var core=p.AddComponent<PlayerCoreController2D>();S(r,"_player",p.transform);S(r,"_playerCore",core);S(r,"_mainMenu",false);S(r,"_tutorialStep",-1);S(r,"_knowledgeLoaded",true);S(r,"_shopSaveLoaded",true);S(r,"_shopSave",Activator.CreateInstance(r.GetType().GetNestedType("ShopSaveData",F)));S(r,"_runEconomy",RunEconomyRules.Create(123));
  var links=(Array)r.GetType().GetField("ActiveAbilitySynergyLinks",F).GetValue(null);var sl=(Dictionary<SphereId,int>)G(r,"_sphereLevels");var sb=(Dictionary<SphereId,string>)G(r,"_sphereBranches");var sf=(Dictionary<SphereId,string>)G(r,"_sphereFinals");var al=(Dictionary<RunUpgradeKind,int>)G(r,"_abilityLevels");var ab=(Dictionary<RunUpgradeKind,string>)G(r,"_abilityBranches");var af=(Dictionary<RunUpgradeKind,string>)G(r,"_abilityFinals");var fused=(List<string>)G(r,"_fusedAbilityLinks");var next=(Dictionary<string,float>)G(r,"_fusionNextAt");var keys=(Dictionary<RunUpgradeKind,KeyCode>)G(r,"_activeAbilityHotkeys");var choices=(List<RunUpgradeChoice>)G(r,"_levelUpChoices");var spheres=(List<SphereAttackAgent>)G(r,"_spheres");var actors=(List<MonoBehaviour>)G(r,"_simulationAgents");
  var objects=new List<GameObject>();
  S(r,"_sourceKnowledge",Resources.Load<NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog"));
  try
  {
   A(links.Length==30,"all authored combinations");
   foreach(var link in links)
   {
    string id=(string)G(link,"Id"),branch=(string)G(link,"SphereBranch"),abilityFinal=(string)G(link,"AbilityFinal");var sphere=(SphereId)G(link,"Sphere");var ability=(RunUpgradeKind)G(link,"Ability");string abilityBranch=abilityFinal.Substring(0,abilityFinal.Length-3);int variant=abilityFinal[abilityFinal.Length-1]-'1';
    sl.Clear();sb.Clear();sf.Clear();al.Clear();ab.Clear();af.Clear();fused.Clear();next.Clear();keys.Clear();choices.Clear();spheres.Clear();
    sl[sphere]=7;sb[sphere]=branch;al[ability]=7;ab[ability]=abilityBranch;var finals=LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(ability,true,abilityBranch);af[ability]=finals[variant].Id;keys[ability]=KeyCode.Alpha1;
    A(!(bool)C(r,"CanFuse",link),id+" cannot fuse before final sphere mutation choice");
    foreach(var final in SphereEvolutionCatalog.GetFinals(branch)){sf[sphere]=final.Id;A((bool)C(r,"CanFuse",link),id+" selected final tier variant "+final.Id);}
    al[ability]=6;A(!(bool)C(r,"CanFuse",link),id+" requires ability VII");al[ability]=7;sl[sphere]=6;A(!(bool)C(r,"CanFuse",link),id+" requires sphere VII");sl[sphere]=7;
    af[ability]=finals[(variant+1)%finals.Count].Id;A(!(bool)C(r,"CanFuse",link),id+" rejects wrong final ability mutation");af[ability]=finals[variant].Id;
    A(!(bool)C(r,"IsActiveAbilitySynergyRaw",sphere,branch,ability,abilityFinal),id+" not active before acceptance");
    C(r,"BuildLevelUpChoices");A(choices.Any(c=>c.Kind==RunUpgradeKind.Fusion&&c.Title==id),id+" guaranteed in real level-up builder");
    S(r,"_levelUp",true);A(!(bool)C(r,"RerollLevelUpChoices"),id+" reroll cannot remove guaranteed offer");
    var ui=C(r,"ReadNativeUpgradeState");A(ui!=null,id+" actual localized native upgrade model");
    S(r,"_levelUp",false);S(r,"_paused",false);A((bool)C(r,"ChooseAbilityFusion",id),id+" acceptance");A(!keys.ContainsKey(ability),id+" manual button removed");A(!(bool)C(r,"ChooseAbilityFusion",id),id+" duplicate command rejected");
    A((bool)C(r,"IsActiveAbilitySynergyRaw",sphere,branch,ability,abilityFinal),id+" active after acceptance");A(r.HasSphereFusion(sphere),id+" persistent visual upgrade");
    Func<FieldInfo,bool> fields=f=>f.Name=="_fusedAbilityLinks"||f.Name=="_fusionNextAt";var save=RunSnapshotCodec.Capture(r,fields);fused.Clear();next.Clear();RunSnapshotCodec.Restore(save,r.GetType(),r,fields);A(fused.Contains(id)&&next.ContainsKey(id),id+" save/restore fusion and cooldown");
    var sg=new GameObject("Owned fusion sphere "+id);objects.Add(sg);var agent=sg.AddComponent<SphereAttackAgent>();agent.Initialize(r,p.transform,sphere,12);spheres.Add(agent);S(r,"_runTime",100f);next[id]=0;
    r.TriggerSphereFusion(agent);A(next[id]>100,id+" real sphere-triggered effect/cooldown");float until=next[id];r.TriggerSphereFusion(agent);A(next[id]==until,id+" cooldown prevents repeat");
    sg.SetActive(false);S(r,"_runTime",0f);S(r,"_levelUp",false);S(r,"_paused",false);
   }
   var result=new{status="PASS",links=links.Length,checks,scope="real runtime: final mutation gates, guaranteed offers, duplicate guard, native UI model, hotkeys, saved fusion/cooldowns, all30 automatic effects; no PlayerPrefs writes"};Directory.CreateDirectory("Library/fusion-check");File.WriteAllText("Library/fusion-check/result.json",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }
  finally{S(r,"_mainMenu",true);foreach(var a in actors.ToArray())if(a!=null)objects.Add(a.gameObject);foreach(var o in objects.Distinct())if(o!=null)UnityEngine.Object.DestroyImmediate(o);UnityEngine.Object.DestroyImmediate(p);UnityEngine.Object.DestroyImmediate(go);}
 }
}
