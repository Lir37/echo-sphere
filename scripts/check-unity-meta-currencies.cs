using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class MetaCurrencyAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);object Call(string n,params object[] a)=>t.GetMethod(n,F).Invoke(r,a);
  const string W="echosphere_meta_wallet_v1",B="echosphere_meta_wallet_backup_v1",R="echosphere_meta_wallet_recovery",G="echosphere_gold";
  var keys=new[]{W,B,R,"echosphere_shop","echosphere_character_profiles","echosphere_active_run_v1"};var saved=keys.ToDictionary(k=>k,k=>new{Had=PlayerPrefs.HasKey(k),Value=PlayerPrefs.GetString(k,"")});bool hadGold=PlayerPrefs.HasKey(G);int gold=PlayerPrefs.GetInt(G,0);
  string beforeProfiles=PlayerPrefs.GetString("echosphere_character_profiles","");string beforeShop=PlayerPrefs.GetString("echosphere_shop","");
  var names=new[]{"_metaWallet","_metaWalletRaw","_metaEntitlementsRaw","_runMetaReceipt","_knowledgeSection","_knowledgeQuery"};var old=names.ToDictionary(n=>n,Get);int checks=0;void A(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  File.WriteAllText(Path.Combine(Application.dataPath,"../Library/point7-wallet-prefs-backup.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{saved,hadGold,gold}));
  void Empty(){PlayerPrefs.DeleteKey(W);PlayerPrefs.DeleteKey(B);Set("_metaWallet",null);Set("_metaWalletRaw",null);}
  try{
   foreach(int value in new[]{0,1,1500,int.MaxValue}){
    Empty();PlayerPrefs.SetInt(G,value);A((int)Call("GetGold")==value,"actual migration1:1 "+value);A(PlayerPrefs.HasKey(W)&&PlayerPrefs.HasKey(B),"persisted primary and backup");
    PlayerPrefs.SetInt(G,23);Set("_metaWallet",null);A((int)Call("GetGold")==value,"legacy key never added twice");
   }
   Empty();PlayerPrefs.SetInt(G,1000);var wallet=(MetaWallet)Call("ReadMetaWallet");
   A((bool)Call("GrantMetaOnce","fixture:grant",100,3,2),"multi-resource award commits");A(!(bool)Call("GrantMetaOnce","fixture:grant",100,3,2),"duplicate award rejected");
   A((int)Call("GetMetaBalance",CurrencyId.Credits)==1100&&(int)Call("GetMetaBalance",CurrencyId.ResonanceShards)==3&&(int)Call("GetMetaBalance",CurrencyId.ArchiveData)==2,"all balances independent");
   A((bool)Call("TryBuyMeta","purchase:fixture",CurrencyId.Credits,1000),"actual purchase commits");A((bool)Call("TryBuyMeta","purchase:fixture",CurrencyId.Credits,1000)&&(int)Call("GetGold")==100,"retry completes without charging again");
   A(!(bool)Call("TryBuyMeta","purchase:unaffordable-fixture",CurrencyId.ResonanceShards,4)&&(int)Call("GetMetaBalance",CurrencyId.ResonanceShards)==3,"unaffordable no side effects");
   Call("SaveGold",12);A((int)Call("GetGold")==12&&(int)Call("GetMetaBalance",CurrencyId.ArchiveData)==2,"source adapter writes Credits only");
   string raw=PlayerPrefs.GetString(W);PlayerPrefs.SetString(W,"{");Set("_metaWallet",null);wallet=(MetaWallet)Call("ReadMetaWallet");A(wallet.Credits==12&&wallet.ResonanceShards==3&&PlayerPrefs.GetString(R)=="{","corrupt primary recovers backup without losing raw");
   PlayerPrefs.SetString(W,"{");PlayerPrefs.SetString(B,"{");Set("_metaWallet",null);bool rejected=false;try{Call("ReadMetaWallet");}catch(TargetInvocationException e){rejected=e.InnerException is InvalidOperationException;}
   A(rejected&&PlayerPrefs.GetString(W)=="{"&&PlayerPrefs.GetInt(G)==12,"both corrupt refuses destructive remigration");PlayerPrefs.SetString(W,raw);PlayerPrefs.SetString(B,raw);Set("_metaWallet",null);
   var omitted=(HashSet<string>)t.GetField("NonRunFields",F).GetValue(null);Set("_runMetaReceipt","fixture:stable-run");var snapshot=RunSnapshotCodec.Capture(r,f=>!omitted.Contains(f.Name));
   A(snapshot.nodes.All(n=>n.name!="_metaWallet"&&n.name!="_metaWalletRaw"),"active run never owns persistent wallet");A(RunSnapshotCodec.Read<string>(snapshot,"_runMetaReceipt")=="fixture:stable-run","run reward receipt saved");
   var stale=RunSnapshotCodec.Capture(r,f=>f.Name=="_metaWallet");Call("SaveGold",99);RunSnapshotCodec.Restore(stale,t,r,f=>!omitted.Contains(f.Name));A((int)Call("GetGold")==99,"loading old run data cannot roll back persistent wallet");
   A(PlayerPrefs.GetString("echosphere_character_profiles","")==beforeProfiles&&PlayerPrefs.GetString("echosphere_shop","")==beforeShop,"migration leaves mastery/unlocks/shop untouched");
   Set("_knowledgeSection","basics");Set("_knowledgeQuery","");var archive=(EchoSphere.Presentation.NativeArchiveScreen.State)Call("ReadNativeArchiveState");A(archive.Cards.Count(c=>c.Id.StartsWith("currency:"))==4,"four purpose cards");A(!string.IsNullOrEmpty(archive.WalletSummary),"native archive wallet shown");
   Call("ClearMetaWallet");PlayerPrefs.DeleteKey(G);A(!PlayerPrefs.HasKey(W)&&!PlayerPrefs.HasKey(B),"reset clears both wallet copies");A((int)Call("GetGold")==0,"reset does not resurrect old balances");
   var result=new{result="PASS",checks,scope="Actual Unity PlayerPrefs migration,independent balances,transaction receipts,corruption recovery,run isolation and progression preservation;synthetic keys restored,no phone writes"};File.WriteAllText(Path.Combine(Application.dataPath,"../Library/meta-currency-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{
   foreach(var pair in saved){if(pair.Value.Had)PlayerPrefs.SetString(pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey(pair.Key);}if(hadGold)PlayerPrefs.SetInt(G,gold);else PlayerPrefs.DeleteKey(G);PlayerPrefs.Save();foreach(var pair in old)Set(pair.Key,pair.Value);
   foreach(var pair in saved)if(PlayerPrefs.HasKey(pair.Key)!=pair.Value.Had||(pair.Value.Had&&PlayerPrefs.GetString(pair.Key)!=pair.Value.Value))throw new Exception("Prefs restore mismatch: "+pair.Key);
   if(PlayerPrefs.HasKey(G)!=hadGold||(hadGold&&PlayerPrefs.GetInt(G)!=gold))throw new Exception("Gold restore mismatch");
  }
 }
}
