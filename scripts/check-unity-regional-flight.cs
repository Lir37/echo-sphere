using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.SceneManagement;using EchoSphere.Runtime;using EchoSphere.Core;
public static class RegionalFlightCheck {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main(){if(!Application.isPlaying)throw new Exception("Play required");var R=typeof(EchoSphereRuntime);var P=R.GetNestedType("PlayerPrefs",F);var L=R.Assembly.GetType("EchoSphere.Runtime.NativeSimulationLifetime");var previous=SceneManager.GetActiveScene();var scene=SceneManager.CreateScene("Regional flight fixture");SceneManager.SetActiveScene(scene);int checks=0;
 object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);void A(bool x,string why){checks++;if(!x)throw new Exception(why);}
 using((IDisposable)P.GetMethod("Isolate",F).Invoke(null,null))using((IDisposable)L.GetMethod("Isolate",F).Invoke(null,null))try{
  var go=new GameObject("Regional flight fixture runtime");go.SetActive(false);var runtime=go.AddComponent<EchoSphereRuntime>();runtime.enabled=false;go.SetActive(true);Call(runtime,"CreatePlayer");var player=(Transform)R.GetField("_player",F).GetValue(runtime);player.position=new Vector2(5,5);Set(runtime,"_mainMenu",false);Set(runtime,"_paused",false);Set(runtime,"_tutorialStep",-1);
  foreach(var sample in new[]{(1,"ranged"),(2,"splitter"),(3,"sniper"),(4,"ranged"),(5,"corruptor")}){
   var enemy=new GameObject("Regional flight owner").AddComponent<EnemyAgent2D>();enemy.Initialize(runtime,player,100,0,role:sample.Item2);enemy.SetCampaignRegion((EchoRegionId)sample.Item1);Set(enemy,"_regionalCooldown",0f);
   var p=RegionalEnemyRules.Get(sample.Item1,sample.Item2);A(p.Range>0,"Origin pocket firing role not enabled");Call(enemy,"TickRegionalAttack",.01f,Vector2.right,.1f);Call(enemy,"TickRegionalAttack",p.Windup+.01f,Vector2.up,.1f);
   var agents=(List<MonoBehaviour>)R.GetField("_simulationAgents",F).GetValue(runtime);var bullets=agents.OfType<BossProjectileAgent2D>().Where(b=>b!=null&&b.SourceOwner==enemy).ToArray();A(bullets.Length==p.Pellets,"Volley absent");
   var b=bullets[0];var initial=b.transform.position;Set(enemy,"_freezeTimer",2f);enemy.SimulationTick(.1f);Call(runtime,"TickSourceEnemyProjectiles",enemy,.1f);A(b.transform.position==initial,"Regional projectile advanced in owner phase");Call(runtime,"TickSourceUnownedEnemyProjectiles",.1f);A(b.transform.position!=initial,"Frozen owner froze regional shot");
   Call(runtime,"RemoveSourceEnemyProjectiles",enemy);A(b.gameObject.activeInHierarchy&&b.SourceOwner==null,"Shot removed on source death");Call(runtime,"PrepareNativeEntityFrame");var independent=(System.Collections.ICollection)R.GetField("_nativeIndependentProjectiles",F).GetValue(runtime);A(independent.Count>=bullets.Length,"Detached shots not presented");var pos=b.transform.position;Call(runtime,"TickSourceUnownedEnemyProjectiles",.1f);A(b.transform.position!=pos,"Detached shot stopped");
   foreach(var shot in bullets)UnityEngine.Object.DestroyImmediate(shot.gameObject);UnityEngine.Object.DestroyImmediate(enemy.gameObject);agents.RemoveAll(x=>x==null);
  }
  var result=new{result="PASS",checks,scope="Actual windup/volleys in all five new regions, autonomous flight while owner frozen and after owner removal, orphan presentation; isolated prefs"};Directory.CreateDirectory("Library/region-variety");File.WriteAllText("Library/region-variety/flight.json",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
 }finally{foreach(var go in scene.GetRootGameObjects())UnityEngine.Object.DestroyImmediate(go);SceneManager.SetActiveScene(previous);SceneManager.UnloadSceneAsync(scene);}
 }
}

