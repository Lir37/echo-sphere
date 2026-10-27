using System;
using System.Linq;
using System.Reflection;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;
public static class NativeStateAcceptance {
 public static object Main(){
  var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var go=new GameObject("Native state acceptance");var runtime=go.AddComponent<EchoSphereRuntime>();runtime.enabled=false;
  var rt=runtime.GetType();rt.GetField("_mainMenu",flags).SetValue(runtime,false);rt.GetField("_paused",flags).SetValue(runtime,false);
  var player=new GameObject("Fixture player");player.transform.SetParent(go.transform);
  int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
  var select=(Func<FieldInfo,bool>)Delegate.CreateDelegate(typeof(Func<FieldInfo,bool>),rt.GetMethod("EntitySnapshotField",BindingFlags.NonPublic|BindingFlags.Static));
  try{
   foreach(var variant in new[]{null,"","wisp","skitter","brute","linkbreaker"}){
    var eg=new GameObject("Fixture enemy");eg.transform.SetParent(go.transform);var enemy=eg.AddComponent<EnemyAgent2D>();enemy.Initialize(runtime,player.transform,25,1,1,variant);
    var expected=string.IsNullOrEmpty(variant)?"wisp":variant;
    Check((string)typeof(EnemyAgent2D).GetProperty("SourceVisualVariant",flags).GetValue(enemy)==expected,"variant owner");
    Check(eg.GetComponents<MonoBehaviour>().Length==1,"retired components instantiated");
    var snapshot=RunSnapshotCodec.Capture(enemy,select);
    Check(!snapshot.nodes.Any(n=>n.name=="_visualVariant"),"legacy snapshot schema expanded");
    RunSnapshotCodec.Restore(snapshot,typeof(EnemyAgent2D),enemy);
    Check((string)typeof(EnemyAgent2D).GetProperty("SourceVisualVariant",flags).GetValue(enemy)==expected,"legacy restore lost variant");
   }
   runtime.SpawnSourceParticle(new Vector2(1,2),new Vector2(20,-10),1,4,"#ff0000",2);
   var particle=go.scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<SourceParticle2D>(true)).Single();
   Check(particle.GetComponent<SpriteRenderer>()==null,"particle has hidden renderer");
   var color=typeof(SourceParticle2D).GetProperty("SavedColor",flags);Check((Color)color.GetValue(particle)==Color.red,"birth saved color");
   particle.SimulationTick(.1f);var expectedColor=new Color(1,0,0,.45f);Check((Color)color.GetValue(particle)==expectedColor,"faded saved color");
   var saved=RunSnapshotCodec.Capture(particle,select);Check(!saved.nodes.Any(n=>n.name=="_savedColor"),"particle schema expanded");
   particle.SimulationTick(.1f);RunSnapshotCodec.Restore(saved,typeof(SourceParticle2D),particle);
   typeof(SourceParticle2D).GetMethod("RestoreSavedColor",flags).Invoke(particle,new object[]{expectedColor});
   Check((Color)color.GetValue(particle)==expectedColor,"saved envelope restore");
   particle.SimulationTick(.1f);Check(Math.Abs(((Color)color.GetValue(particle)).a-.4f)<.00001,"restored fade");
   var result=new {result="PASS",checks,scope="owned edit-mode fixture, unchanged snapshot fields, no player prefs writes"};System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/native-state-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(player);foreach(var p in UnityEngine.Object.FindObjectsByType<SourceParticle2D>(FindObjectsInactive.Include))if(p.name=="Source Particle")UnityEngine.Object.DestroyImmediate(p.gameObject);}
 }
}
