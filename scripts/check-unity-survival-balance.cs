using System;using System.Linq;using System.IO;using System.Reflection;using System.Collections.Generic;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class SurvivalBalanceCheck {
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 public static object Main(){if(!Application.isPlaying)throw new Exception("Play required");int checks=0;void Check(bool condition,string name){checks++;if(!condition)throw new Exception(name);}void Near(float x,float y,string name)=>Check(Mathf.Abs(x-y)<.002f,name+" got "+x+" expected "+y);object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);object Call(object o,string n,params object[] a)=>o.GetType().GetMethods(F).Single(m=>m.Name==n&&m.GetParameters().Length==a.Length).Invoke(o,a);
 var objects=new HashSet<GameObject>();GameObject Make(string n){var g=new GameObject("Owned survival check "+n);objects.Add(g);return g;}var root=Make("runtime");root.SetActive(false);var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;root.SetActive(true);Set(runtime,"_knowledgeLoaded",true);Set(runtime,"_shopSaveLoaded",true);Set(runtime,"_shopSave",Activator.CreateInstance(runtime.GetType().GetNestedType("ShopSaveData",F)));Set(runtime,"_mainMenu",false);Set(runtime,"_tutorialStep",-1);var player=Make("core");var core=player.AddComponent<PlayerCoreController2D>();Set(runtime,"_player",player.transform);Set(runtime,"_playerCore",core);Set(runtime,"_runEconomy",RunEconomyRules.Create(123));var owned=(List<string>)Get(runtime,"_ownedArtifacts");owned.Add("vampire_ring");var agents=(List<MonoBehaviour>)Get(runtime,"_simulationAgents");var enemies=(List<EnemyAgent2D>)Get(runtime,"_enemies");void Health(float hp)=>core.GetType().GetProperty("CurrentHp",F).SetValue(core,hp);EnemyAgent2D Enemy(float hp){var g=Make("target");var e=g.AddComponent<EnemyAgent2D>();e.Initialize(runtime,player.transform,hp,0);return e;}
 try{
  Health(50);var target=Enemy(100);target.ReceiveSphereDamage(SphereId.Standard,20,true,false,null,true);Near(core.CurrentHp,50.2f,"ring one percent");
  Health(50);var weak=Enemy(10);weak.ReceiveSphereDamage(SphereId.Standard,1000,true,false,null,true);Near(core.CurrentHp,50.1f,"overkill capped and no five HP kill bonus");weak.ReceiveSphereDamage(SphereId.Standard,1000,true,false,null,true);Near(core.CurrentHp,50.1f,"dead target no second heal");
  Health(50);Enemy(10).ReceiveDamage(1000,emitFeedback:false);Near(core.CurrentHp,50,"non-sphere kill no flat ring healing");
  Health(99.95f);Enemy(100).ReceiveSphereDamage(SphereId.Standard,20,true,false,null,true);Near(core.CurrentHp,100,"healing respects max HP");
  var levels=(Dictionary<RunUpgradeKind,int>)Get(runtime,"_abilityLevels");levels[RunUpgradeKind.Vampire]=1;Health(50);Enemy(10).ReceiveSphereDamage(SphereId.Standard,1000,true,false,null,true);Near(core.CurrentHp,50.4f,"ability unchanged rate but no overkill");levels.Clear();
  foreach (int fps in new[] {30, 60, 120})
  {
   core.SetMaxHpForNewRun(100);
   Health(40);
   var durable = Enemy(1000000);
   durable.ReceiveSphereDamage(SphereId.Standard, 1000, true, false, null, true);
   Near(core.CurrentHp, 42, "initial combat healing burst");
   for (int frame = 0; frame < 3 * fps; frame++)
   {
    core.Tick(1f / fps);
    durable.ReceiveSphereDamage(SphereId.Standard, 1000, true, false, null, true);
   }
   Near(core.CurrentHp, 54, "sustained combat healing at " + fps + " FPS");
   durable.ReceiveSphereDamage(SphereId.Standard, 1000, true, false, null, true);
   Near(core.CurrentHp, 54, "additional hits cannot spend exhausted healing credit");
  }
  var rng=(SeededRng)Get(runtime,"_rng");Set(runtime,"_wave",20);
  foreach(EchoRegionId region in Enum.GetValues(typeof(EchoRegionId))){Set(runtime,"_selectedRegion",region);var snapshots=new List<(float hp,float damage)>();foreach(float time in new[]{0f,1800f}){Set(runtime,"_runTime",time);rng.Reset(20261104);Call(runtime,"SpawnRegionalEnemy",false,null);var enemy=enemies.Last();snapshots.Add((enemy.MaxHp,enemy.ContactDamage));enemy.gameObject.SetActive(false);}Near(snapshots[1].hp/snapshots[0].hp,1.6f,"same wave HP clock "+region);Near(snapshots[1].damage/snapshots[0].damage,1.45f,"same wave damage clock "+region);}
  Set(runtime,"_selectedRegion",EchoRegionId.ResonanceBasin);var bossHp=new List<float>();var bossDamage=new List<float>();var boltDamage=new List<float>();foreach(float time in new[]{0f,1800f}){Set(runtime,"_runTime",time);rng.Reset(20261104);var boss=runtime.SpawnBossForRegionEvent("shooter",20);objects.Add(boss.gameObject);bossHp.Add(boss.MaxHp);var combat=boss.GetComponent<BossCombatAgent2D>();bossDamage.Add((float)Get(combat,"_damage"));Call(combat,"ExecuteCampaignAttack",CampaignBossRules.Pattern("shooter",0,0));boltDamage.Add((float)Get(agents.OfType<BossProjectileAgent2D>().Last(),"_damage"));boss.gameObject.SetActive(false);}Near(bossHp[1]/bossHp[0],1.6f,"boss health clock");Near(bossDamage[1]/bossDamage[0],1.45f,"boss contact clock");Near(boltDamage[1]/boltDamage[0],1.45f,"boss projectile clock");
  var shipDamage=new List<float>();foreach(float time in new[]{0f,1800f}){Set(runtime,"_runTime",time);var ship=Enemy(100);Set(ship,"_role","ranged");ship.SetCampaignRegion(EchoRegionId.SpectralRift);Set(ship,"_regionalWindup",.001f);Set(ship,"_regionalAim",Vector2.zero);Call(ship,"TickRegionalAttack",.01f,Vector2.zero,2f);shipDamage.Add((float)Get(agents.OfType<BossProjectileAgent2D>().Last(),"_damage"));ship.gameObject.SetActive(false);}Near(shipDamage[1]/shipDamage[0],1.45f,"ship projectile clock");
  Set(runtime,"_tutorialStep",1);Near(runtime.CampaignEnemyHealthMultiplier,1,"tutorial HP unchanged");Near(runtime.CampaignEnemyDamageMultiplier,1,"tutorial damage unchanged");Set(runtime,"_tutorialStep",-1);Set(runtime,"_runEconomy",null);Near(runtime.CampaignEnemyHealthMultiplier,1,"legacy HP unchanged");Near(runtime.CampaignEnemyDamageMultiplier,1,"legacy damage unchanged");
  var result=new{status="PASS",checks,scope="Real isolated Unity combat and spawn paths: ring healing, overkill, kill bonus removal, HP cap, ability rate, all six regional spawns, boss, tutorial and legacy. No device save changes."};Directory.CreateDirectory("Library/survival-balance");File.WriteAllText("Library/survival-balance/editor.json",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
 }finally{foreach(var a in agents.ToArray())if(a!=null)objects.Add(a.gameObject);foreach(var g in objects.Where(g=>g!=null&&g!=root))UnityEngine.Object.DestroyImmediate(g);Set(runtime,"_mainMenu",true);UnityEngine.Object.DestroyImmediate(root);}
 }
}