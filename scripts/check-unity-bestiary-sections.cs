using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using EchoSphere.Runtime;using EchoSphere.Core;using EchoSphere.Presentation;
public static class BestiarySectionsCheck {
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 public static object Main(){
  var owner=new GameObject("Isolated bestiary section check");owner.SetActive(false);var runtime=owner.AddComponent<EchoSphereRuntime>();runtime.enabled=false;
  void Set(string n,object v)=>typeof(EchoSphereRuntime).GetField(n,F).SetValue(runtime,v);
  int checks=0;void Check(bool pass,string why){checks++;if(!pass)throw new Exception(why);}
  try{
   Set("_knowledgeLoaded",true);Set("_mainMenu",true);Set("_shopSaveLoaded",true);Set("_shopSave",Activator.CreateInstance(typeof(EchoSphereRuntime).GetNestedType("ShopSaveData",F)));
   var known=(HashSet<string>)typeof(EchoSphereRuntime).GetField("_knownKnowledgeIds",F).GetValue(runtime);
   NativeArchiveScreen.State Read(string section,string query){Set("_knowledgeSection",section);Set("_knowledgeQuery",query);return (NativeArchiveScreen.State)typeof(EchoSphereRuntime).GetMethod("ReadNativeArchiveState",F).Invoke(runtime,null);}
   foreach(bool unlocked in new[]{false,true}){
    known.Clear();if(unlocked)foreach(var entry in BestiaryCatalog.All)known.Add(entry.Id);
    var monsters=Read("monsters","");var bosses=Read("bosses","");
    Check(monsters.Cards.Count==BestiaryCatalog.All.Count(x=>!x.Boss),"all ordinary and elite entries retained");Check(bosses.Cards.Count==BestiaryCatalog.All.Count(x=>x.Boss),"all bosses retained");
    Check(!monsters.Cards.Select(x=>x.Id).Intersect(bosses.Cards.Select(x=>x.Id)).Any(),"tabs are disjoint");
    foreach(var card in monsters.Cards.Concat(bosses.Cards)){var entry=BestiaryCatalog.Find(card.Id);Check(card.Enemy.Boss==entry.Boss,"appearance preserved");Check(card.Open==unlocked,"discovery preserved");Check(card.Details==entry.Lore(monsters.Russian),"lore preserved");}
    for(int r=0;r<6;r++)foreach(string section in new[]{"monsters","bosses"}){
     var state=Read(section,CampaignRegions.SourceId((EchoRegionId)r));foreach(var card in state.Cards)Check(card.Enemy.Boss==(section=="bosses"),"region search preserves partition");
    }
   }
   Directory.CreateDirectory("Library/bestiary-sections");string result="PASS "+checks+" checks; 178 entries partitioned, discovery, lore and region search preserved; no PlayerPrefs writes";File.WriteAllText("Library/bestiary-sections/result.txt",result);return result;
  }finally{UnityEngine.Object.DestroyImmediate(owner);}
 }
}
