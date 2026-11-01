using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using EchoSphere.Core;using EchoSphere.Runtime;
public static class CampaignBossAcceptance
{
 public static object Main()
 {
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();
  object Get(string n)=>t.GetField(n,F).GetValue(r);void Set(string n,object v)=>t.GetField(n,F).SetValue(r,v);
  var names=new[]{"_runEconomy","_mainMenu","_paused","_gameOver","_simulationFrame","_resonanceCharge","_message","_messageTimer","_sourceFlashColor","_sourceFlashKind","_networkState","_combo","_characterScreenShakeIntensity","_deathTimer"};
  var old=names.ToDictionary(n=>n,Get);var core=(PlayerCoreController2D)Get("_playerCore");var playerSnapshot=RunSnapshotCodec.Capture(core);var player=(Transform)Get("_player");var pos=player.position;
  var rng=(SeededRng)Get("_rng");uint seed=rng.State;var enemies=(List<EnemyAgent2D>)Get("_enemies");var oldEnemies=enemies.ToArray();var spheres=(List<SphereAttackAgent>)Get("_spheres");var oldSpheres=spheres.ToArray();var owned=(List<string>)Get("_ownedArtifacts");var oldOwned=owned.ToArray();
  var existing=new HashSet<GameObject>(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Select(x=>x.gameObject));int checks=0;
  void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
  object Field(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
  void Assign(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  try
  {
   Set("_mainMenu",false);Set("_paused",false);Set("_gameOver",false);Set("_simulationFrame",true);Set("_runEconomy",RunEconomyRules.Create(123));owned.Clear();spheres.Clear();enemies.Clear();player.position=Vector3.zero;core.SetMaxHpForNewRun(100000);core.SetShieldCharges(0);core.Tick(100);
   for(int i=0;i<2;i++){var go=new GameObject("Boss sphere fixture");go.transform.position=new Vector3(1,i*.3f);var sphere=go.AddComponent<SphereAttackAgent>();sphere.Initialize(r,player,SphereId.Standard,12);spheres.Add(sphere);}
   for(int b=0;b<8;b++)for(int phase=0;phase<2;phase++)for(int index=0;index<3;index++)
   {
    player.position=Vector3.zero;string type=BossCombatRules.GetTypeForDefeatedCount(b);var pattern=CampaignBossRules.Pattern(type,index,phase);
    var go=new GameObject("Campaign boss fixture / "+type);go.transform.position=new Vector3(2,0);
    var enemy=go.AddComponent<EnemyAgent2D>();enemy.Initialize(r,player,10000,1,1,type,type,damage:1);enemies.Add(enemy);t.GetMethod("RegisterEnemyQuery",F).Invoke(r,new object[]{enemy});
    if(phase>0)enemy.ScaleCurrentHp(.4f);
    var boss=go.AddComponent<BossCombatAgent2D>();boss.Initialize(r,player,enemy,type,1,1,10,Color.magenta);enemy.SetBossCombat(boss);
    var state=new CampaignBossState{AttackIndex=index,Remaining=.001f};Assign(boss,"_campaign",state);
    boss.Tick(.01f);Check(state.Telegraphing&&state.Phase==phase,"actual warning and phase "+type);Check(go.transform.position==new Vector3(2,0),"holds windup position");
    float ax=state.AimX,ay=state.AimY;var snap=RunSnapshotCodec.Capture(boss);Assign(boss,"_campaign",null);RunSnapshotCodec.Restore(snap,boss.GetType(),boss);state=boss.CampaignState;
    Check(state.Telegraphing&&state.AimX==ax&&state.AimY==ay,"actual auxiliary save restores warning/aim");
    // Projection must expose the locked cue independently of moving target/facing.
    var view=t.GetMethod("ProjectNativeEnemyView",F).Invoke(null,new object[]{enemy,(Vector2)player.position,true,false});var signal=Field(view,"Signal");Check((string)Field(view,"BossName")==CampaignBossRules.Name(type,false),"actual boss family name");Check((bool)Field(signal,"Campaign")&&(bool)Field(signal,"Telegraph")&&(int)Field(signal,"Attack")== (int)pattern.Attack,"native warning projection");
    float timer=state.Remaining;Set("_mainMenu",true);boss.Tick(1);Check(state.Remaining==timer,"pause/menu cannot advance boss");Set("_mainMenu",false);
    foreach(var s in spheres)Assign(s,"_networkDisabledTimer",0f);
    Set("_resonanceCharge",100f);core.Tick(100);float hp=core.CurrentHp;
    int bulletCount=UnityEngine.Object.FindObjectsByType<BossProjectileAgent2D>().Length;int minions=enemies.Count;
    if(pattern.Attack==CampaignBossAttack.Fan)player.position=new Vector3(2,8);
    boss.Tick(pattern.Windup-.01f);boss.Tick(.02f);Check(!state.Telegraphing&&state.AttackIndex==(index+1)%3,"actual attack consumes warning");
    switch(pattern.Attack)
    {
     case CampaignBossAttack.Fan:case CampaignBossAttack.Radial:case CampaignBossAttack.Cross:
      var bullets=UnityEngine.Object.FindObjectsByType<BossProjectileAgent2D>().Where(x=>x.SourceOwner==enemy).ToArray();Check(bullets.Length==pattern.Count,"actual projectile count "+type);Check(bullets.All(x=>Math.Abs(((Vector2)Field(x,"_velocity")).magnitude-BossCombatRules.ToWorldUnits(pattern.Speed))<.001f),"phase projectile speed");if(pattern.Attack==CampaignBossAttack.Fan)Check(bullets.Any(x=>Vector2.Dot(((Vector2)Field(x,"_velocity")).normalized,new Vector2(ax,ay))>.999f),"fan retains warned aim after player moves");break;
     case CampaignBossAttack.Summon:Check(enemies.Count-minions==pattern.Count,"actual summon count");break;
     case CampaignBossAttack.Dash:Check(go.transform.position.x<2,"actual locked dash movement");var dash=RunSnapshotCodec.Capture(boss);Assign(boss,"_campaignDashRemaining",0f);RunSnapshotCodec.Restore(dash,boss.GetType(),boss);Check((float)Field(boss,"_campaignDashRemaining")>0,"mid-dash restored");break;
     case CampaignBossAttack.Pulse:Check(core.CurrentHp<hp,"pulse hits inside visible range");break;
     case CampaignBossAttack.Disable:Check(spheres.Count(x=>x.NetworkDisabled)==pattern.Count,"bounded network attack");break;
     case CampaignBossAttack.NullField:Check((float)Get("_resonanceCharge")==75,"actual null drain");break;
     case CampaignBossAttack.Shift:Check(Math.Abs(Vector2.Distance(go.transform.position,player.position)-pattern.Radius*SpherePlacementRules.SourcePixelsToWorld)<.001f,"actual shift distance");break;
    }
    if(pattern.Attack==CampaignBossAttack.Pulse||pattern.Attack==CampaignBossAttack.NullField||pattern.Attack==CampaignBossAttack.Disable)
    {
     player.position=new Vector3(100,100);foreach(var sphere in spheres){sphere.transform.position=new Vector3(100,100);Assign(sphere,"_networkDisabledTimer",0f);}
     core.Tick(100);hp=core.CurrentHp;Set("_resonanceCharge",100f);state=new CampaignBossState{AttackIndex=index,Phase=phase,AttackPhase=phase,Telegraphing=true,Remaining=.001f};Assign(boss,"_campaign",state);boss.Tick(.01f);
     Check(core.CurrentHp==hp&&(float)Get("_resonanceCharge")==100&&spheres.All(x=>!x.NetworkDisabled),"outside telegraphed area unaffected");
     for(int i=0;i<spheres.Count;i++)spheres[i].transform.position=new Vector3(1,i*.3f);
    }
    // Legacy restored run must never enter new director, even with a saved modern auxiliary field.
    Set("_runEconomy",null);timer=state.Remaining;boss.Tick(.01f);Check(state.Remaining==timer&&boss.CampaignState==null,"legacy branch isolated");Set("_runEconomy",RunEconomyRules.Create(123));
    foreach(var bullet in UnityEngine.Object.FindObjectsByType<BossProjectileAgent2D>().Where(x=>x.SourceOwner==enemy))UnityEngine.Object.DestroyImmediate(bullet.gameObject);
    enemies.Remove(enemy);UnityEngine.Object.DestroyImmediate(go);
   }
   var result=new{result="PASS",checks,bossFamilies=8,attacks=24,phases=2,scope="Actual Editor execution of every campaign attack in both phases,auxiliary warning/aim/dash save,projection,pause and legacy isolation;not Android or balance acceptance"};
   File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/campaign-boss-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }
  finally
  {
   foreach(var a in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))if(a!=null&&!existing.Contains(a.gameObject)&&a is ISourceSimulationAgent)UnityEngine.Object.DestroyImmediate(a.gameObject);
   enemies.Clear();enemies.AddRange(oldEnemies);spheres.Clear();spheres.AddRange(oldSpheres);owned.Clear();owned.AddRange(oldOwned);foreach(var n in names)Set(n,old[n]);RunSnapshotCodec.Restore(playerSnapshot,core.GetType(),core);player.position=pos;rng.RestoreState(seed);
  }
 }
}
