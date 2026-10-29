using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Runtime;
using EchoSphere.Core;
public static class SphereTrackingAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");UnityEditor.EditorApplication.isPaused=true;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=runtime.GetType();int checks=0;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  foreach(var name in new[]{"_mainMenu","_paused","_userPaused","_gameOver","_levelUp","_stellaRewardPending","_choosingEvolution"})t.GetField(name,F).SetValue(runtime,false);t.GetField("_tutorialStep",F).SetValue(runtime,-1);
  var enemies=(List<EnemyAgent2D>)t.GetField("_enemies",F).GetValue(runtime);foreach(var e in enemies)if(e!=null)e.gameObject.SetActive(false);enemies.Clear();
  var enemy=runtime.SpawnBossMinion(Vector2.right,100000,0,0,"normal",Color.white);var rows=new List<object>();
  var selector=t.GetMethod("EntitySnapshotField",BindingFlags.Static|BindingFlags.NonPublic);
  foreach(var name in new[]{"_presentationAimRotation","_presentationAimValid"})Check(!(bool)selector.Invoke(null,new object[]{typeof(SphereAttackAgent).GetField(name,F)}),"Aim entered save: "+name);
  foreach(var family in new[]{SphereId.Standard,SphereId.Sniper,SphereId.Shotgun,SphereId.Chain,SphereId.Prism,SphereId.Void}){
   var go=new GameObject("Tracking fixture "+family);var sphere=go.AddComponent<SphereAttackAgent>();sphere.Initialize(runtime,runtime.transform,family,12);typeof(SphereAttackAgent).GetField("_attackTimer",F).SetValue(sphere,10f);float oldRotation=sphere.VisualRotation,hp=enemy.CurrentHp;enemy.transform.position=new Vector2(sphere.AttackRange*.4f,0);
   try{
    sphere.SimulationTick(.01f);Check(Math.Abs(sphere.PresentationRotation)<.001,"Did not acquire target during cooldown "+family);
    enemy.transform.position=new Vector2(0,sphere.AttackRange*.4f);sphere.SimulationTick(.01f);Check(Math.Abs(sphere.PresentationRotation-Math.PI/2)<.001,"Did not lead moving target "+family);
    var model=t.GetMethod("ProjectNativeSphereVisual",F).Invoke(runtime,new object[]{sphere});Check(Math.Abs((double)model.GetType().GetField("Rotation",F).GetValue(model)+Math.PI/2)<.001,"Native view ignored aim "+family);
    Check(sphere.VisualRotation==oldRotation,"Changed cadence-owned rotation "+family);Check(enemy.CurrentHp==hp&&!sphere.PresentationAttackActive&&sphere.AttackTimer>9,"Tracking fired or damaged target "+family);
    float last=sphere.PresentationRotation;enemy.transform.position=new Vector2(sphere.AttackRange*2,0);sphere.SimulationTick(.01f);Check(sphere.PresentationRotation==last,"Out-of-range target changed aim "+family);
    rows.Add(new{family=family.ToString(),range=sphere.AttackRange,angle=sphere.PresentationRotation,attackTimer=sphere.AttackTimer});
   }finally{UnityEngine.Object.DestroyImmediate(go);}
  }
  enemy.gameObject.SetActive(false);enemies.Clear();var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result="PASS",checks,rows,scope="Real simulation calls: six directed families track moving in-range target during cooldown, native projection follows, no shot/damage/cadence rotation/save changes."});System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/sphere-tracking-acceptance.json"),json);return new{result="PASS",checks,families=rows.Count};
 }
}
