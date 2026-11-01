using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class EnemyRosterAcceptance
{
 public static object Main()
 {
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);void Assign(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);object Call(string n,params object[] args)=>t.GetMethod(n,F).Invoke(r,args);
  var names=new[]{"_runEconomy","_mainMenu","_paused","_gameOver","_simulationFrame","_resonanceCharge","_message","_messageTimer","_sourceFlashColor","_sourceFlashKind","_networkState","_combo","_characterScreenShakeIntensity","_deathTimer","_selectedRegion","_wave"};var old=names.ToDictionary(n=>n,Get);
  var core=(PlayerCoreController2D)Get("_playerCore");var savedCore=RunSnapshotCodec.Capture(core);var player=(Transform)Get("_player");var position=player.position;var rng=(SeededRng)Get("_rng");uint seed=rng.State;
  var enemies=(List<EnemyAgent2D>)Get("_enemies");var oldEnemies=enemies.ToArray();var spheres=(List<SphereAttackAgent>)Get("_spheres");var oldSpheres=spheres.ToArray();var owned=(List<string>)Get("_ownedArtifacts");var oldOwned=owned.ToArray();var abilities=(Dictionary<RunUpgradeKind,int>)Get("_abilityLevels");var oldAbilities=new Dictionary<RunUpgradeKind,int>(abilities);
  var before=new HashSet<GameObject>(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Select(x=>x.gameObject));int checks=0;void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
  EnemyAgent2D Enemy(string role,string elite="",float x=3){var go=new GameObject("Enemy roster fixture");go.transform.position=new Vector3(x,0);var e=go.AddComponent<EnemyAgent2D>();e.Initialize(r,player,1000,1,1,"wisp","",elite!="",EnemyArchetype.Normal,role=="phase",role,eliteVariant:elite);enemies.Add(e);Call("RegisterEnemyQuery",e);return e;}
  void ResetPlayer(){core.SetMaxHpForNewRun(100000);core.SetShieldCharges(0);core.Tick(100);}
  try
  {
   Set("_mainMenu",false);Set("_paused",false);Set("_gameOver",false);Set("_simulationFrame",true);Set("_runEconomy",RunEconomyRules.Create(123));owned.Clear();abilities.Clear();spheres.Clear();enemies.Clear();player.position=Vector3.zero;ResetPlayer();
   var sg=new GameObject("Roster sphere fixture");sg.transform.position=Vector3.one;var sphere=sg.AddComponent<SphereAttackAgent>();sphere.Initialize(r,player,SphereId.Standard,12);spheres.Add(sphere);
   // Exercise actual spawn selection, role modifiers and serialized identity, not a duplicate catalog.
   var roles=new HashSet<string>();Set("_wave",0);Set("_selectedRegion",EchoRegionId.ResonanceBasin);
   foreach(var point in new[]{new Vector2(0,0),new Vector2(520,260),new Vector2(500,-430),new Vector2(-480,-430),new Vector2(-520,300)})
   {
    player.position=point*SpherePlacementRules.SourcePixelsToWorld;
    for(int n=0;n<60;n++){Call("SpawnEnemy");var e=enemies[enemies.Count-1];roles.Add(e.Role);Check(e.CurrentHp>0&&e.SourceSpeed>0&&e.ContactDamage>0,"spawned role has actual stats");var snap=RunSnapshotCodec.Capture(e);string role=e.Role;Assign(e,"_role","");RunSnapshotCodec.Restore(snap,e.GetType(),e);Check(e.Role==role,"role persists");enemies.Remove(e);UnityEngine.Object.DestroyImmediate(e.gameObject);}
   }
   Check(roles.Count==15,"all fifteen roles reachable in actual regional spawns");player.position=Vector3.zero;
   foreach(string role in roles)
   {
    Assign(sphere,"_networkDisabledTimer",0f);ResetPlayer();var e=Enemy(role,x:role=="ranged"||role=="sniper"?6:role=="leech"?2:3);var ally=Enemy("grunt",x:3.1f);ally.ScaleCurrentHp(.5f);e.ScaleCurrentHp(.5f);var at=e.transform.position;float hp=e.CurrentHp;
    e.SimulationTick(.1f);Check(e.transform.position.x<at.x,"role simulation movement "+role);
    if(role=="ranged"||role=="sniper")Check(UnityEngine.Object.FindObjectsByType<BossProjectileAgent2D>().Count(x=>x.SourceOwner==e)==1,"actual role projectile "+role);
    if(role=="healer")Check(ally.CurrentHp>500,"actual ally healing");
    if(role=="disruptor")Check(sphere.NetworkDisabled,"actual role disruption");
    if(role=="leech")Check(e.CurrentHp>hp,"actual leech recovery");
    if(role=="splitter"||role=="bomber")
    {
     int count=enemies.Count;float ahp=ally.CurrentHp;Call("ApplySourceEnemyDeathEffects",e);
     Check(role=="splitter"?enemies.Count==count+2:ally.CurrentHp<ahp,"actual death behavior "+role);
    }
    if(role=="corruptor"){e.transform.position=new Vector3(.1f,0);Set("_resonanceCharge",100f);e.SimulationTick(.01f);Check((float)Get("_resonanceCharge")<100,"actual contact corruption");}
    enemies.Remove(e);enemies.Remove(ally);UnityEngine.Object.DestroyImmediate(e.gameObject);UnityEngine.Object.DestroyImmediate(ally.gameObject);
   }
   var elites=(string[])t.GetField("EliteVariantIds",F).GetValue(null);Check(elites.Length==10&&elites.Distinct().Count()==10,"ten actual selectable elite variants");
   foreach(string id in elites)
   {
    ResetPlayer();Assign(sphere,"_networkDisabledTimer",0f);Set("_resonanceCharge",100f);var e=Enemy("grunt",id,id=="phantom_hunter"?6:1);float hp=core.CurrentHp;var at=e.transform.position;
    if(id=="scavenger_prime")e.ScaleCurrentHp(.4f);Assign(e,"_elitePulseTimer",0f);e.SimulationTick(.1f);
    if(id=="resonance_leech"||id=="geometry_shifter")Check((float)Get("_resonanceCharge")<100,"elite resonance effect "+id);
    if(id=="pyroclast"||id=="stasis_warden")Check(core.CurrentHp<hp,"elite area damage "+id);
    if(id=="phantom_hunter"||id=="scavenger_prime")Check(at.x-e.transform.position.x>.12f,"elite speed effect "+id);
    if(id=="nullifier")Check(sphere.NetworkDisabled,"elite nullification");
    if(id=="mirror_warden"){core.Tick(100);hp=core.CurrentHp;float dealt=e.ReceiveSphereDamage(SphereId.Standard,100,true,false);Check(dealt<100&&core.CurrentHp<hp,"elite mitigation and reflection");}
    if(id=="linkbreaker"){Assign(e,"_linkBreakerCooldownTimer",0f);e.SimulationTick(.01f);Check((float)typeof(EnemyAgent2D).GetField("_linkBreakerTelegraphTimer",F).GetValue(e)>0,"elite locked warning");e.SimulationTick(.8f);Check(sphere.NetworkDisabled,"elite warned network break");}
    if(id=="splitter_prime"){int count=enemies.Count;Call("ApplySourceEnemyDeathEffects",e);Check(enemies.Count==count+2,"elite split children");}
    var snap=RunSnapshotCodec.Capture(e);Assign(e,"_eliteVariant","");RunSnapshotCodec.Restore(snap,e.GetType(),e);Check(e.EliteVariant==id,"elite identity roundtrip");enemies.Remove(e);UnityEngine.Object.DestroyImmediate(e.gameObject);
   }
   var result=new{result="PASS",checks,roles=roles.OrderBy(x=>x).ToArray(),elites,scope="Actual regional spawn coverage,role stats/identity/movement,special role handlers,all ten elite effect handlers and snapshot identities;not balance or Android acceptance"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/enemy-roster-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }
  finally
  {
   foreach(var a in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))if(a!=null&&!before.Contains(a.gameObject)&&a is ISourceSimulationAgent)UnityEngine.Object.DestroyImmediate(a.gameObject);
   enemies.Clear();enemies.AddRange(oldEnemies);spheres.Clear();spheres.AddRange(oldSpheres);owned.Clear();owned.AddRange(oldOwned);abilities.Clear();foreach(var p in oldAbilities)abilities[p.Key]=p.Value;foreach(var n in names)Set(n,old[n]);RunSnapshotCodec.Restore(savedCore,core.GetType(),core);player.position=position;rng.RestoreState(seed);
  }
 }
}
