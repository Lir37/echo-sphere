using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;using EchoSphere.Presentation;
public static class MetaCommerceAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);object Call(string n,params object[] a)=>t.GetMethod(n,F).Invoke(r,a);
  var names=new[]{"_metaWallet","_metaWalletRaw","_metaEntitlementsRaw","_mainMenu","_mainMenuPage","_sphereCharacterId","_characterMasteryLevel","_expandedCharacterId","_nativeCharacterMessage","_nativeMenuModelVersion","_nativeCharacterState","_nativeProgressionState","_nativeProgressionScreen","_shopSaveLoaded","_characterProfilesLoaded"};var before=names.ToDictionary(n=>n,Get);
  var profiles=Get("_characterProfiles");var profileSave=RunSnapshotCodec.Capture(profiles);var shop=Get("_shopSave");var shopSave=RunSnapshotCodec.Capture(shop);
  var strings=new[]{"echosphere_meta_wallet_v1","echosphere_meta_wallet_backup_v1","echosphere_meta_wallet_recovery","echosphere_shop","echosphere_character_profiles","echosphere_character"};var prefs=strings.ToDictionary(k=>k,k=>new{Had=PlayerPrefs.HasKey(k),Value=PlayerPrefs.GetString(k,"")});bool hadGold=PlayerPrefs.HasKey("echosphere_gold");int gold=PlayerPrefs.GetInt("echosphere_gold");var go=new GameObject("Meta commerce fixture");int checks=0;void A(bool v,string why){checks++;if(!v)throw new Exception(why);}
  object Field(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);void Assign(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  void Page(string n)=>Set("_mainMenuPage",Enum.Parse(t.GetField("_mainMenuPage",F).FieldType,n));
  try{
   Set("_mainMenu",true);Call("WriteMetaWallet",MetaCurrencyRules.MigrateGold(10000));Call("GrantMetaOnce","fixture:funds",0,100,20);Set("_metaEntitlementsRaw",null);
   var hunter=Call("GetCharacterProfile",SphereCharacterId.Hunter);Assign(hunter,"unlocked",false);Assign(hunter,"masteryLevel",3);Assign(hunter,"masteryXp",321);Page("Characters");int cost=MetaCurrencyRules.CharacterUnlockShards(CharacterRules.Get(SphereCharacterId.Hunter).UnlockCost);
   Call("ChooseNativeCharacter","hunter");A((bool)Field(hunter,"unlocked")&&(int)Call("GetGold")==10000&&(int)Call("GetMetaBalance",CurrencyId.ResonanceShards)==100-cost,"actual character purchase Shards only");A((int)Field(hunter,"masteryLevel")==3&&(int)Field(hunter,"masteryXp")==321,"purchase preserves mastery");int balance=(int)Call("GetMetaBalance",CurrencyId.ResonanceShards);Call("ChooseNativeCharacter","hunter");A((int)Call("GetMetaBalance",CurrencyId.ResonanceShards)==balance,"selecting owned character free");
   Assign(hunter,"unlocked",false);Call("SaveCharacterProfiles");Set("_metaEntitlementsRaw",null);Call("GetGold");A((bool)Field(hunter,"unlocked")&&(int)Call("GetMetaBalance",CurrencyId.ResonanceShards)==balance,"paid character recovers after interrupted profile write");A((int)Field(hunter,"masteryXp")==321,"recovery preserves mastery XP");
   var state=(NativeCharacterScreen.State)Call("ReadNativeCharacterState");A(state.Gold.Contains("Shards")||state.Gold.Contains("Осколки"),"native character balance uses Shards label");
   A(t.GetMethod("ResearchNativeCharacterMastery",F)==null,"no mastery purchase handler");var legacy=(MetaWallet)Call("ReadMetaWallet");legacy.Receipts.Add("purchase:mastery:hunter:4");Call("WriteMetaWallet",legacy);
   Assign(hunter,"masteryLevel",3);Assign(hunter,"masteryXp",321);Call("SaveCharacterProfiles");Set("_metaEntitlementsRaw",null);Call("GetMetaBalance",CurrencyId.ResonanceShards);A((int)Field(hunter,"masteryLevel")==4&&(int)Field(hunter,"masteryXp")==1500,"paid mastery recovers after interrupted profile write");
   Call("WriteMetaWallet",MetaCurrencyRules.MigrateGold(1000));Set("_metaEntitlementsRaw",null);Page("Shop");Set("_nativeProgressionScreen",go.AddComponent<NativeProgressionScreen>());var upgrades=Field(shop,"upgrades");upgrades.GetType().GetMethod("Set").Invoke(upgrades,new object[]{"hp",0});int price=PersistentUpgradeRules.GetCost("hp",0);
   Call("BuyNativeShopUpgrade","hp");A((int)Call("GetShopUpgradeLevel","hp")==1&&(int)Call("GetGold")==1000-price,"actual shop purchase unchanged price");
   upgrades.GetType().GetMethod("Set").Invoke(upgrades,new object[]{"hp",0});Call("SaveShopState");Set("_metaEntitlementsRaw",null);Call("GetGold");A((int)Call("GetShopUpgradeLevel","hp")==1&&(int)Call("GetGold")==1000-price,"paid upgrade recovery no double spend");
   var shopState=(NativeProgressionScreen.State)Call("ReadNativeProgressionState");A(shopState.Gold.EndsWith("CR")||shopState.Gold.EndsWith("КР"),"native shop Credits");A(shopState.Shop.First(x=>x.Id=="hp").Price.StartsWith(PersistentUpgradeRules.GetCost("hp",1).ToString()),"next price remains authored progression curve");
   var result=(NativeRunResultView.State)Call("ReadNativeRunResultState");
   var output=new{result="PASS",checks,scope="Actual native character/shop handlers,authored prices,mastery preservation,owned selection and interrupted entitlement recovery;original prefs/runtime restored"};File.WriteAllText(Path.Combine(Application.dataPath,"../Library/meta-commerce-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(output));return output;
  }finally{
   UnityEngine.Object.DestroyImmediate(go);RunSnapshotCodec.Restore(profileSave,profiles.GetType(),profiles);RunSnapshotCodec.Restore(shopSave,shop.GetType(),shop);foreach(var pair in before)Set(pair.Key,pair.Value);
   foreach(var pair in prefs){if(pair.Value.Had)PlayerPrefs.SetString(pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey(pair.Key);}if(hadGold)PlayerPrefs.SetInt("echosphere_gold",gold);else PlayerPrefs.DeleteKey("echosphere_gold");PlayerPrefs.Save();
   foreach(var pair in prefs)if(PlayerPrefs.HasKey(pair.Key)!=pair.Value.Had||(pair.Value.Had&&PlayerPrefs.GetString(pair.Key)!=pair.Value.Value))throw new Exception("Restore failed: "+pair.Key);
  }
 }
}
