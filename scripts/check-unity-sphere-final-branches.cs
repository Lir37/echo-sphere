using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Presentation;
public static class SphereFinalBranchAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();
  int checks=0,branches=0,finals=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  object Get(object o,string name)=>o.GetType().GetField(name,F).GetValue(o);
  void Set(object o,string name,object v)=>o.GetType().GetField(name,F).SetValue(o,v);
  var branchMap=(Dictionary<SphereId,string>)Get(runtime,"_sphereBranches");var originalBranches=new Dictionary<SphereId,string>(branchMap);
  var choices=(List<SphereEvolutionOption>)Get(runtime,"_evolutionChoices");var originalChoices=choices.ToArray();
  var old=new Dictionary<string,object>();foreach(var field in new[]{"_levelUp","_choosingEvolution","_pendingFinalEvolution","_pendingEvolutionIsAbility","_pendingEvolutionSphere"})old[field]=Get(runtime,field);
  var rows=new List<object>();
  try{
   Set(runtime,"_levelUp",true);Set(runtime,"_choosingEvolution",true);Set(runtime,"_pendingEvolutionIsAbility",false);
   foreach(SphereId family in Enum.GetValues(typeof(SphereId))){
    string id=family.ToString().ToLowerInvariant();var catalog=NativeRunCatalog.Instance.GetSphere(id);if(catalog==null)continue;
    var parents=SphereEvolutionCatalog.GetBranches(family);Check(parents.Count==3,"Three IV branches required "+id);Check(catalog.Branches.Count==3,"UI IV branches missing "+id);
    Set(runtime,"_pendingEvolutionSphere",family);Set(runtime,"_pendingFinalEvolution",false);choices.Clear();choices.AddRange(parents);
    var iv=(IList)type.GetMethod("BuildNativeUpgradeInputs",F).Invoke(runtime,null);Check(iv.Count==3,"Live IV inputs missing "+id);
    var offeredFinalIds=new HashSet<string>();
    foreach(var parent in parents){
     branches++;branchMap[family]=parent.Id;var pool=SphereEvolutionCatalog.GetFinals(parent.Id);Check(pool.Count==3,"Three dependent VII finals required "+parent.Id);
     var uiParent=catalog.Branches.Single(x=>x.Id==parent.Id);Check(uiParent.FinalCards.Count==3,"Missing UI finals "+parent.Id);
     Set(runtime,"_pendingFinalEvolution",true);choices.Clear();choices.AddRange(pool);
     var vii=(IList)type.GetMethod("BuildNativeUpgradeInputs",F).Invoke(runtime,null);Check(vii.Count==3,"Live VII inputs missing "+parent.Id);
     for(int i=0;i<3;i++){
      Check(pool[i].Id==parent.Id+"_final_"+(i+1),"Foreign final "+pool[i].Id);Check(offeredFinalIds.Add(pool[i].Id),"Final reused between IV branches "+id);
      var card=vii[i];Check((string)Get(card,"SphereBranch")==parent.Id,"Native card refers to another parent");Check((int)Get(card,"Index")==i,"Core/UI final index mismatch");
      var text=(NativeRunCatalog.Text)Get(card,"Text");Check(text.NameRu==uiParent.FinalCards[i].NameRu&&text.DescriptionRu==uiParent.FinalCards[i].DescriptionRu,"Wrong native final text");finals++;
     }
     rows.Add(new{sphere=id,branch=parent.Id,finals=pool.Select(x=>x.Id).ToArray(),titles=uiParent.FinalCards.Select(x=>x.NameRu).ToArray()});
    }
    Check(offeredFinalIds.Count==9,"Nine branch-specific VII finals required "+id);
   }
   Check(branches==30&&finals==90,"Expected all ten families");
   var result=new{result="PASS",checks,families=10,branches,finals,rows,scope="Real runtime BuildNativeUpgradeInputs plus Core/native catalogs; all VII proposals depend on selected IV parent; no progression rule changes required"};
   string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/sphere-final-branch-acceptance.json"));File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(result));return new{result="PASS",checks,families=10,branches,finals};
  }finally{branchMap.Clear();foreach(var row in originalBranches)branchMap.Add(row.Key,row.Value);choices.Clear();choices.AddRange(originalChoices);foreach(var row in old)Set(runtime,row.Key,row.Value);}
 }
}
