using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
public static class RunEconomyRuntimeAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var t=r.GetType();object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);object Call(string n,params object[] a)=>t.GetMethod(n,F).Invoke(r,a);
  var names=new[]{"_runEconomy","_mainMenu","_gameOver","_paused","_userPaused","_levelUp","_choosingEvolution","_stellaRewardPending","_tutorialStep","_xp"};var old=new Dictionary<string,object>();foreach(var n in names)old[n]=Get(n);
  var owned=(List<string>)Get("_ownedArtifacts");var before=owned.ToArray();var core=Get("_playerCore");var player=RunSnapshotCodec.Capture(core);string key="echosphere_active_run_v1";bool had=PlayerPrefs.HasKey(key);string prefs=PlayerPrefs.GetString(key,"");int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  try{
   var state=RunEconomyRules.Create(123);RunEconomyRules.AdvanceMerchant(state,390);state.Echo=5000;Set("_runEconomy",state);Set("_mainMenu",false);Set("_gameOver",false);Set("_paused",false);Set("_userPaused",false);Set("_levelUp",false);Set("_choosingEvolution",false);Set("_stellaRewardPending",false);Set("_tutorialStep",-1);
   Call("OpenNativeMerchant");Check(state.Open&&(bool)Get("_paused"),"merchant owns pause");int money=state.Echo;
   Call("BuyNativeMerchant",0);Check(state.Offers[0].Sold&&owned.Contains(state.Offers[0].ArtifactId)&&state.Echo<money,"runtime purchase applies");int paid=state.Echo;Call("BuyNativeMerchant",0);Check(state.Echo==paid,"runtime repeat is rejected");
   var save=Call("LoadRunSnapshot");var snapshot=(SnapshotValue)save.GetType().GetField("runtime",F).GetValue(save);var stored=RunSnapshotCodec.Read<RunEconomyState>(snapshot,"_runEconomy");Check(stored.Echo==paid&&stored.Offers[0].Sold&&stored.Offers[0].ArtifactId==state.Offers[0].ArtifactId,"actual PlayerPrefs saved receipt");
   Call("SetUserPaused",true);Check((bool)Get("_paused"),"focus loss holds simulation");Call("SetUserPaused",false);Check((bool)Get("_paused"),"resume cannot release merchant pause");Call("CloseNativeMerchant");Check(!state.Open&&!(bool)Get("_paused"),"closing releases merchant pause");
   Set("_runEconomy",null);Set("_xp",0f);r.CollectExperienceOrb(1,9);Check((float)Get("_xp")>0&&Get("_runEconomy")==null,"legacy pickup does not opt into economy");
   Set("_runEconomy",state);Set("_xp",0f);int wallet=state.Echo;r.CollectExperienceOrb(1,9);Check(state.Echo==wallet+9,"Echo payload independent from XP");
   return "PASS "+checks+" actual runtime/pause/persistence checks; original prefs restored";
  }finally{
   foreach(var n in names)Set(n,old[n]);owned.Clear();owned.AddRange(before);RunSnapshotCodec.Restore(player,core.GetType(),core);
   if(had)PlayerPrefs.SetString(key,prefs);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();
   if(had&&PlayerPrefs.GetString(key)!=prefs)throw new Exception("Original prefs restore failed");
  }
 }
}
