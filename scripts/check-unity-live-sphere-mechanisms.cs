using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Runtime;
using EchoSphere.Core;
public static class LiveSphereMechanismsAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");UnityEditor.EditorApplication.isPaused=true;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;int checks=0;
  void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var type=runtime.GetType();
  foreach(var name in new[]{"_mainMenu","_regionSelect","_paused","_userPaused","_gameOver","_levelUp","_stellaRewardPending","_choosingEvolution"})type.GetField(name,F).SetValue(runtime,false);type.GetField("_tutorialStep",F).SetValue(runtime,-1);
  var enemies=(List<EnemyAgent2D>)type.GetField("_enemies",F).GetValue(runtime);foreach(var enemy in enemies)if(enemy!=null)enemy.gameObject.SetActive(false);enemies.Clear();
  var surface=type.GetField("_worldSurface",F).GetValue(runtime);Check(surface!=null,"Initialize native battle document first");
  var owner=type.GetProperty("WorldPresentation",F).GetValue(runtime);var weapons=owner.GetType().GetProperty("SphereWeapons",F).GetValue(owner);
  object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,F).Invoke(target,args);
  var spheres=(List<SphereAttackAgent>)type.GetField("_spheres",F).GetValue(runtime);var rows=new List<object>();
  foreach(var family in new[]{SphereId.Standard,SphereId.Sniper,SphereId.Shotgun,SphereId.Chain,SphereId.Aura,SphereId.Gravity,SphereId.Pulse,SphereId.Prism,SphereId.Void,SphereId.Orbital}){
   var go=new GameObject("Live source/render bridge "+family);var sphere=go.AddComponent<SphereAttackAgent>();sphere.Initialize(runtime,runtime.transform,family,12);spheres.Add(sphere);
   var target=runtime.SpawnBossMinion(Vector2.right*.1f,1000000,0,0,"normal",Color.white);
   try{
    sphere.ResetAttackTimer();sphere.SimulationTick(.016f);
    if(family!=SphereId.Orbital){Check(sphere.PresentationShotSerial==1,"Real event missing "+family);Check(sphere.PresentationShotTime==runtime.CurrentRunTime,"Wrong event clock "+family);}
    var model=type.GetMethod("ProjectNativeSphereVisual",F).Invoke(runtime,new object[]{sphere});var mt=model.GetType();var center=new Vector2((float)(double)mt.GetField("X",F).GetValue(model),(float)(double)mt.GetField("Y",F).GetValue(model));
    Call(surface,"Begin",center);Call(surface,"ResetTransform");Call(owner,"BeginFrame",surface);type.GetMethod("DrawNativeSphereFrame",F).Invoke(runtime,new object[]{sphere,(double)runtime.CurrentRunTime});
    if(family==SphereId.Orbital){double scale=(double)type.GetMethod("NativeSphereScale",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new[]{model});double radius=(double)type.GetMethod("NativeOrbitalRadiusScale",F).Invoke(runtime,new[]{model});type.GetMethod("DrawNativeOrbitalSphere",F).Invoke(runtime,new[]{model,(object)(double)runtime.CurrentRunTime,scale,radius,true});}
    Call(surface,"End");
    var rigs=(IDictionary)weapons.GetType().GetField("_rigs",F).GetValue(weapons);string id=family.ToString().ToLowerInvariant();Check(rigs.Keys.Cast<object>().Any(key=>(string)key.GetType().GetField("Item1").GetValue(key)==id),"Runtime did not use retained mechanism "+family);
    rows.Add(new{family=id,eventSerial=sphere.PresentationShotSerial,rigs=rigs.Count});
   }finally{spheres.Remove(sphere);target.gameObject.SetActive(false);enemies.Remove(target);UnityEngine.Object.DestroyImmediate(go);}
  }
  type.GetField("_paused",F).SetValue(runtime,true);type.GetField("_userPaused",F).SetValue(runtime,true);
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result="PASS",checks,rows,scope="Real simulation -> native projection -> scene-owned frame renderer, all10families; orbital uses separate contact satellite pass; temporary preferences require restore"});System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/live-sphere-mechanisms-acceptance.json"),json);return new{result="PASS",checks,families=rows.Count};
 }
}
