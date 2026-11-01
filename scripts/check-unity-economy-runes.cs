using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class EconomyRuneAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);object Call(string n,params object[] args)=>t.GetMethod(n,F).Invoke(r,args);
  var flagNames=new[]{"_mainMenu","_paused","_userPaused","_simulationFrame","_tutorialStep","_levelUp","_choosingEvolution","_stellaRewardPending","_gameOver","_regionStabilized","_regionEndlessMode","_convergenceRun","_runTime","_knowledgeSection","_knowledgeQuery"};
  var originalFlags=flagNames.ToDictionary(n=>n,Get);
  var omitted=(HashSet<string>)t.GetField("NonRunFields",F).GetValue(null);var model=RunSnapshotCodec.Capture(r,f=>!omitted.Contains(f.Name));
  var core=(PlayerCoreController2D)Get("_playerCore");var coreSave=RunSnapshotCodec.Capture(core);var player=(Transform)Get("_player");var position=player.position;
  var enemies=(List<EnemyAgent2D>)Get("_enemies");var beforeEnemies=enemies.ToArray();var spheres=(List<SphereAttackAgent>)Get("_spheres");var beforeSpheres=spheres.ToArray();var owned=(List<string>)Get("_ownedArtifacts");var beforeOwned=owned.ToArray();
  var existing=new HashSet<GameObject>(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Select(a=>a.gameObject));
  const string key="echosphere_active_run_v1";bool had=PlayerPrefs.HasKey(key);string prefs=PlayerPrefs.GetString(key,"");int checks=0;
  File.WriteAllText(Path.Combine(Application.dataPath,"../Library/point6-fixture-prefs-backup.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{had,prefs}));
  void A(bool v,string why){checks++;if(!v)throw new Exception(why);}
  void Assign(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  EnemyAgent2D Enemy(float x,bool elite=false){var go=new GameObject("Rune acceptance enemy");go.transform.position=new Vector3(x,0);var e=go.AddComponent<EnemyAgent2D>();e.Initialize(r,player,100000,0,1,isElite:elite);enemies.Add(e);Call("RegisterEnemyQuery",e);return e;}
  try{
   Set("_mainMenu",false);Set("_gameOver",false);Set("_paused",false);Set("_userPaused",false);Set("_simulationFrame",true);Set("_tutorialStep",-1);Set("_regionStabilized",false);Set("_levelUp",false);Set("_choosingEvolution",false);Set("_stellaRewardPending",false);Set("_regionEndlessMode",false);Set("_convergenceRun",false);Set("_runTime",600f);
   enemies.Clear();spheres.Clear();owned.Clear();player.position=Vector3.zero;core.SetMaxHpForNewRun(100000);core.SetShieldCharges(0);core.Tick(100);
   var economy=RunEconomyRules.Create(321);Set("_runEconomy",economy);
   A((bool)Call("TryCreateExplorationChest")&&(bool)Call("TryCreateExplorationChest")&&!(bool)Call("TryCreateExplorationChest"),"runtime exploration creates only two chests");
   A(UnityEngine.Object.FindObjectsByType<ArtifactChestAgent2D>().Count(a=>a.name=="Exploration Artifact Cache")==2,"exactly two live exploration pickups");
   Call("SaveCurrentRun");var save=Call("LoadRunSnapshot");var stored=RunSnapshotCodec.Read<RunEconomyState>((SnapshotValue)save.GetType().GetField("runtime",F).GetValue(save),"_runEconomy");A(stored.ExplorationRewards==2&&stored.PricingVersion==2,"actual saved economy ownership");
   foreach(var a in UnityEngine.Object.FindObjectsByType<ArtifactChestAgent2D>().Where(a=>!existing.Contains(a.gameObject)))UnityEngine.Object.DestroyImmediate(a.gameObject);
   var chestGo=new GameObject("Rune acceptance cache");var chest=chestGo.AddComponent<ArtifactChestAgent2D>();chest.Initialize(r);int opened=(int)Get("_runChestOpens");r.CollectArtifactChest(chest);r.CollectArtifactChest(chest);A((int)Get("_runChestOpens")==opened+1&&!chestGo.activeSelf,"cache double pickup cannot overwrite/reward twice");Set("_stellaRewardPending",false);Set("_paused",false);
   float radius=r.GetExperienceMagnetRadiusWorld();r.ActivateRunePickup("harvest",Vector2.zero);A(Mathf.Abs(r.GetExperienceMagnetRadiusWorld()-radius*2)<.00001f,"actual harvest magnet");
   r.ActivateRunePickup("overdrive",Vector2.zero);A(economy.Runes.Overdrive==8,"overdrive duration");
   foreach(SphereId id in Enum.GetValues(typeof(SphereId))){economy.Runes.Overdrive=0;float normal=r.GetSphereAttackSpeedMultiplier(id);economy.Runes.Overdrive=8;A(Mathf.Abs(r.GetSphereAttackSpeedMultiplier(id)/normal-1.35f)<.0001f,"network speed "+id);}
   r.ActivateRunePickup("phase",Vector2.zero);A(Mathf.Abs(core.InvulnerableTimer-.8f)<.0001f,"phase exact duration");core.Tick(1);
   var clearCore=RunSnapshotCodec.Capture(core);uint seed=((SeededRng)Get("_rng")).State;
   economy.Runes.Fortify=0;float hp=core.CurrentHp;r.TryDamagePlayer(100);float normalLoss=hp-core.CurrentHp;RunSnapshotCodec.Restore(clearCore,core.GetType(),core);((SeededRng)Get("_rng")).RestoreState(seed);r.ActivateRunePickup("fortify",Vector2.zero);hp=core.CurrentHp;r.TryDamagePlayer(100);A(normalLoss>0&&Mathf.Abs((hp-core.CurrentHp)/normalLoss-.65f)<.001f,"contact damage reduced exactly once");
   RunSnapshotCodec.Restore(clearCore,core.GetType(),core);economy.Runes.Fortify=0;hp=core.CurrentHp;r.TryDamagePlayerOverTime(100);normalLoss=hp-core.CurrentHp;RunSnapshotCodec.Restore(clearCore,core.GetType(),core);economy.Runes.Fortify=8;hp=core.CurrentHp;r.TryDamagePlayerOverTime(100);A(normalLoss>0&&Mathf.Abs((hp-core.CurrentHp)/normalLoss-.65f)<.001f,"DoT reduction exactly once");
   var e=Enemy(1);var elite=Enemy(1,true);float ordinaryHp=e.CurrentHp,eliteHp=elite.CurrentHp;r.ActivateRunePickup("purge",Vector2.zero);A(e.CurrentHp<ordinaryHp&&elite.CurrentHp==eliteHp,"purge ordinary only");
   r.ActivateRunePickup("hunt",Vector2.zero);A(elite.IsMarked&&elite.IsRuneHuntMarked,"hunt marks elite");
   var markedSave=RunSnapshotCodec.Capture(elite);Assign(elite,"_runeHuntUntil",0f);RunSnapshotCodec.Restore(markedSave,elite.GetType(),elite);A(elite.IsRuneHuntMarked,"hunt model survives save without a Unity target reference");
   Set("_runTime",608.01f);A(!elite.IsRuneHuntMarked,"hunt expires after8seconds of simulation");Set("_runTime",600f);
   r.ActivateRunePickup("gravity",Vector2.zero);float distance=e.transform.position.magnitude;Call("TickCampaignRunes",.5f);A(e.transform.position.magnitude<distance&&economy.Runes.Gravity==3.5f,"temporary zone pulls over time");
   Set("_simulationFrame",false);Set("_paused",true);float timer=economy.Runes.Overdrive;Call("TickCampaignRunes",1f);A(economy.Runes.Overdrive==timer,"pause preserves tactical timers");Set("_paused",false);Set("_simulationFrame",true);
   A(EchoSphere.Presentation.NativeRunCatalog.Instance.GetRune("runic_cell").Color=="#8cffb8","native cell definition");r.ActivateRunePickup("runic_cell",Vector2.zero);A(economy.Runes.CellsCollected==0,"ordinary cell rejected");Set("_regionEndlessMode",true);
   var cellGo=new GameObject("Rune acceptance cell");var cell=cellGo.AddComponent<RuneAgent2D>();cell.transform.position=Vector3.zero;cell.Initialize(r,"runic_cell",Vector2.zero,22);cell.SimulationTick(.01f);cell.SimulationTick(.01f);A(economy.Runes.CellsCollected==1&&!cellGo.activeSelf,"cell single consumption before deferred Destroy");
   var round=(RunEconomyState)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(economy),typeof(RunEconomyState));A(RuneRules.CellReward(round.Runes)==5&&round.Runes.Gravity>0,"actual rune model save");
   Set("_knowledgeSection","runes");Set("_knowledgeQuery","");var archive=(EchoSphere.Presentation.NativeArchiveScreen.State)Call("ReadNativeArchiveState");A(archive.Cards.Count==10&&archive.Cards.All(c=>c.Open&&!string.IsNullOrEmpty(c.Description)),"all rune reference cards");
   // Real cast integration: empty field preserves Echo; a target consumes it.
   foreach(var enemy in enemies)if(enemy!=null)enemy.gameObject.SetActive(false);enemies.Clear();
   foreach(SphereId id in Enum.GetValues(typeof(SphereId))){
    var go=new GameObject("Rune acceptance "+id);var sphere=go.AddComponent<SphereAttackAgent>();sphere.Initialize(r,player,id,12);spheres.Add(sphere);sphere.transform.position=Vector3.zero;
    economy.Runes.EchoReady=true;Assign(sphere,"_attackTimer",0f);sphere.SimulationTick(.001f);A(economy.Runes.EchoReady,"empty "+id+" does not consume Echo");
    var target=Enemy(.25f);Assign(sphere,"_attackTimer",0f);
    if(id==SphereId.Orbital){Assign(sphere,"_orbitalContactCheckTimer",0f);target.transform.position=sphere.GetOrbitalElementAt(0).position;Call("RefreshEnemyQuery",target);}
    var sphereSave=RunSnapshotCodec.Capture(sphere);var enemySave=RunSnapshotCodec.Capture(target);Vector3 targetPosition=target.transform.position;uint attackSeed=((SeededRng)Get("_rng")).State;
    int projectiles=UnityEngine.Object.FindObjectsByType<ProjectileAgent>().Length;float hpBefore=target.CurrentHp;
    economy.Runes.EchoReady=false;sphere.SimulationTick(.001f);int normalShots=UnityEngine.Object.FindObjectsByType<ProjectileAgent>().Length-projectiles;float normalDamage=hpBefore-target.CurrentHp;
    if(id!=SphereId.Orbital){RunSnapshotCodec.Restore(sphereSave,sphere.GetType(),sphere);RunSnapshotCodec.Restore(enemySave,target.GetType(),target);target.transform.position=targetPosition;Call("RefreshEnemyQuery",target);}
    else {Assign(sphere,"_orbitalContactCheckTimer",0f);var latch=sphere.GetType().GetField("_orbitalContactLatch",F).GetValue(sphere) as IDictionary;latch.Clear();}
    ((SeededRng)Get("_rng")).RestoreState(attackSeed);economy.Runes.EchoReady=true;
    projectiles=UnityEngine.Object.FindObjectsByType<ProjectileAgent>().Length;hpBefore=target.CurrentHp;sphere.SimulationTick(.001f);
    A(!economy.Runes.EchoReady,"successful "+id+" consumes Echo");
    int repeatedShots=UnityEngine.Object.FindObjectsByType<ProjectileAgent>().Length-projectiles;
    if(normalShots>0)A(repeatedShots==normalShots*2,"real repeated projectile count "+id);
    else A(normalDamage>0&&hpBefore-target.CurrentHp>normalDamage*1.4f,"real repeated contact/beam/area damage "+id);
target.gameObject.SetActive(false);enemies.Clear();spheres.Remove(sphere);UnityEngine.Object.DestroyImmediate(go);
   }
   Set("_runEconomy",null);r.ActivateRunePickup("fortify",Vector2.zero);A(core.ShieldCharges>=2,"legacy fortify remains shield rule");
   var result=new{result="PASS",checks,scope="Actual Editor runtime economy/runes/casts/pause/save; no full-battle balance or Android acceptance"};File.WriteAllText(Path.Combine(Application.dataPath,"../Library/economy-rune-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{
   foreach(var agent in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))if(agent!=null&&!existing.Contains(agent.gameObject)&&agent is ISourceSimulationAgent)UnityEngine.Object.DestroyImmediate(agent.gameObject);
   enemies.Clear();enemies.AddRange(beforeEnemies);spheres.Clear();spheres.AddRange(beforeSpheres);owned.Clear();owned.AddRange(beforeOwned);RunSnapshotCodec.Restore(model,t,r,f=>!omitted.Contains(f.Name));RunSnapshotCodec.Restore(coreSave,core.GetType(),core);player.position=position;foreach(var item in originalFlags)Set(item.Key,item.Value);
   var index=Get("_enemyQueries");if(index!=null)index.GetType().GetMethod("Clear",F).Invoke(index,null);foreach(var enemy in beforeEnemies)if(enemy!=null)Call("RegisterEnemyQuery",enemy);
   if(had)PlayerPrefs.SetString(key,prefs);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();if(PlayerPrefs.HasKey(key)!=had||(had&&PlayerPrefs.GetString(key)!=prefs))throw new Exception("Prefs restore failed");
   foreach(var item in originalFlags)if(!Equals(Get(item.Key),item.Value))throw new Exception("Runtime flag restore failed: "+item.Key);
  }
 }
}
