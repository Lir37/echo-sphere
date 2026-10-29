using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
using EchoSphere.Presentation;
public static class MechanicalWeaponsAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");UnityEditor.EditorApplication.isPaused=true;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;int checks=0;
  void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
  var asm=typeof(EchoSphereRuntime).Assembly;var wt=asm.GetType("EchoSphere.Presentation.NativeWorldSurface");var weaponType=asm.GetType("EchoSphere.Presentation.NativeWorldSphereWeapons");var stateType=weaponType.GetNestedType("State",BindingFlags.NonPublic);var coreType=asm.GetType("EchoSphere.Presentation.NativeWorldStandardSphere");var familyType=asm.GetType("EchoSphere.Presentation.NativeWorldSphereFamilies");var familyStateType=familyType.GetNestedType("State",BindingFlags.NonPublic);var feedbackType=asm.GetType("EchoSphere.Presentation.NativeWorldFeedback");
  void Set(object o,string name,object value)=>o.GetType().GetField(name,F).SetValue(o,value);
  object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,F).Invoke(o,args);
  var go=new GameObject("Mechanical weapons GPU acceptance");var world=go.AddComponent(wt);Call(world,"Initialize");Call(world,"SetViewport",240f,180f,1.75f);var weapons=Activator.CreateInstance(weaponType,true);var cores=Activator.CreateInstance(coreType,true);var families=Activator.CreateInstance(familyType,true);var feedback=Activator.CreateInstance(feedbackType,true);var output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../../UnityEvaluations/ECHO-SPHERE-architecture-20261028/user-feedback"));output=@"C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-architecture-20261028\user-feedback";Directory.CreateDirectory(output);
  object Model(string family,string branch,int final,double x=0,double y=0,double radius=32){var s=Activator.CreateInstance(stateType);Set(s,"Identity",new object());Set(s,"Family",family);Set(s,"Branch",branch);Set(s,"Final",final);Set(s,"X",x);Set(s,"Y",y);Set(s,"Radius",radius);Set(s,"AttackDelay",1.2d);return s;}
  Color32[] Read(){var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);return image.GetPixels32();}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
  void Save(string name){var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
  Color32[] Frame(object state){Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(weapons,"BeginFrame");Call(weapons,"Draw",world,state);Call(world,"End");return Read();}
  var rows=new List<object>();
  try{
   var model=Model("standard","standard_resonator",0);var idle=Frame(model);Check(idle.Count(c=>c.r+c.g+c.b>0)>50,"Weapon invisible");Set(model,"Time",10d);Check(idle.SequenceEqual(Frame(model)),"Idle weapon cycles without shots");Set(model,"ShotSerial",1u);Set(model,"Time",10.01d);Frame(model);Set(model,"Time",10.07d);var attack=Frame(model);Check(!idle.SequenceEqual(attack),"Parts did not move on shot");Save("mechanical-weapon-attack.png");Set(model,"Time",11d);Check(idle.SequenceEqual(Frame(model)),"Parts did not return to exact rest geometry");Save("mechanical-weapon-idle.png");
   // Build all actual branch/final rigs, then inspect every rigid part pose.
   foreach(var family in new[]{"standard","sniper","shotgun"}){
    Frame(Model(family,"",-1));foreach(var branch in NativeRunCatalog.Instance.GetSphere(family).Branches){Frame(Model(family,branch.Id,-1));for(int i=0;i<3;i++)Frame(Model(family,branch.Id,i));}
   }
   var rigs=(IDictionary)weaponType.GetField("_rigs",F).GetValue(weapons);Check(rigs.Count==39,"Expected3base +36branch/final rigs");var geometryByFamily=new Dictionary<string,HashSet<string>>();int parts=0,moved=0;
   var pose=weaponType.GetMethod("Pose",BindingFlags.Static|BindingFlags.NonPublic);
   foreach(DictionaryEntry entry in rigs){var key=entry.Key;string family=(string)key.GetType().GetField("Item1").GetValue(key),branch=(string)key.GetType().GetField("Item2").GetValue(key);var list=(IEnumerable)entry.Value.GetType().GetField("Parts",F).GetValue(entry.Value);using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    foreach(var part in list){parts++;var type=part.GetType();var rest=(Matrix4x4)pose.Invoke(null,new[]{part,(object)double.PositiveInfinity,.3d});
     foreach(var age in new[]{0d,.02,.06,.12,.20,.28,.5}){var matrix=(Matrix4x4)pose.Invoke(null,new[]{part,(object)age,.3d});Check(Math.Abs(matrix.determinant-1)<.0001,"Part scale changed during attack");Check(Math.Abs(matrix.GetColumn(0).magnitude-1)<.0001&&Math.Abs(matrix.GetColumn(1).magnitude-1)<.0001,"Animated resizing");if(matrix!=rest)moved++;}
     var mesh=(Mesh)type.GetField("Stroke",F).GetValue(part);foreach(var vertex in mesh.vertices){var point=rest.MultiplyPoint3x4(vertex);writer.Write(point.x);writer.Write(point.y);}writer.Write(mesh.vertexCount);
    }
    if(branch.Length>0){if(!geometryByFamily.TryGetValue(family,out var set)){set=new HashSet<string>();geometryByFamily.Add(family,set);}string hash=Convert.ToBase64String(SHA256.Create().ComputeHash(stream.ToArray()));Check(set.Add(hash),"Identical branch/final geometry in "+family+"/"+branch);}
   }
   Check(moved>0,"No articulated parts");foreach(var set in geometryByFamily.Values)Check(set.Count==12,"Missing one of12designs");
   // Actual engine contact sheets, with the same cores/materials as battle.
   foreach(var family in new[]{"standard","sniper","shotgun"}){
    Call(world,"SetViewport",780f,800f,1.5f);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(weapons,"BeginFrame");Call(cores,"BeginFrame");Call(families,"BeginFrame");Call(feedback,"BeginFrame",world);Call(feedback,"Rectangle",-390d,-400d,780d,800d,0d,new Color(.012f,.030f,.065f,1));Call(feedback,"Text",family+" - Unity / IV + VII",-350d,-360d,Color.white,18f);
    var branches=NativeRunCatalog.Instance.GetSphere(family).Branches;
    for(int col=0;col<3;col++)for(int row=0;row<4;row++){double x=-270+col*260,y=-240+row*180;var branch=branches[col];int final=row-1;string label=row==0?"IV: "+branch.Name(true):"VII: "+branch.Finals[final].Name(true).Split('•').Last().Trim();Call(feedback,"Text",label,x-70,y-68,Color.white,10f);
     if(family=="standard"){Call(cores,"DrawCore",world,40d,.5d,(Matrix4x4?)Matrix4x4.Translate(new Vector3((float)x,(float)y,0)),(Color?)new Color(.39f,.90f,1));Call(cores,"DrawRing",world,47d,.6d,.75d,(Matrix4x4?)Matrix4x4.Translate(new Vector3((float)x,(float)y,0)));}
     else{var fs=Activator.CreateInstance(familyStateType);Set(fs,"Family",family);Set(fs,"X",x);Set(fs,"Y",y);Set(fs,"Scale",40d/24);Set(fs,"Time",2.5d);Call(families,"DrawFamily",world,cores,fs,false);}
     Call(weapons,"Draw",world,Model(family,branch.Id,final,x,y,40));
    }Call(world,"End");Save("mechanical-"+family+"-unity.png");rows.Add(new{family,designs=12});
   }
   // Warm poses must never upload or retessellate meshes.
   var versions=new Dictionary<Mesh,ulong>();var shapes=asm.GetType("EchoSphere.Presentation.NativeWorldShapes");var version=shapes.GetMethod("GeometryVersion",BindingFlags.Static|BindingFlags.NonPublic);
   foreach(DictionaryEntry entry in rigs)foreach(var part in (IEnumerable)entry.Value.GetType().GetField("Parts",F).GetValue(entry.Value))foreach(string name in new[]{"Fill","Stroke","Halo"}){var mesh=(Mesh)part.GetType().GetField(name,F).GetValue(part);if(mesh!=null)versions[mesh]=(ulong)version.Invoke(null,new object[]{mesh});}
   Call(world,"SetViewport",240f,180f,1.75f);for(int i=0;i<12;i++){Set(model,"Time",20d+i*.02);if(i==0)Set(model,"ShotSerial",2u);Frame(model);}foreach(var pair in versions)Check((ulong)version.Invoke(null,new object[]{pair.Key})==pair.Value,"Animation rebuilt retained geometry");
   var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var rt=runtime.GetType();var select=rt.GetMethod("EntitySnapshotField",BindingFlags.NonPublic|BindingFlags.Static);Check(!(bool)select.Invoke(null,new object[]{typeof(SphereAttackAgent).GetField("_presentationShotSerial",F)}),"Shot serial entered save");
   foreach(string name in new[]{"_mainMenu","_paused","_userPaused","_gameOver","_levelUp","_choosingEvolution","_stellaRewardPending"})Set(runtime,name,false);Set(runtime,"_tutorialStep",-1);Set(runtime,"_level",1);Set(runtime,"_xpRequired",RunBalanceRules.GetXpToNextLevel(1));Set(runtime,"_xp",3f);rt.GetMethod("NativeDebugLevelUp",F).Invoke(runtime,null);Check((int)rt.GetField("_level",F).GetValue(runtime)==2,"Test button did not grant exactly one level");Check((bool)rt.GetField("_levelUp",F).GetValue(runtime),"Normal upgrade selection did not open");Check((float)rt.GetField("_xp",F).GetValue(runtime)==3,"Test button discarded partial XP");var choices=(IList)rt.GetField("_levelUpChoices",F).GetValue(runtime);Check(choices.Count>0,"Missing ordinary choices");rt.GetMethod("NativeDebugLevelUp",F).Invoke(runtime,null);Check((int)rt.GetField("_level",F).GetValue(runtime)==2,"Repeated modal click granted another level");
   rt.GetMethod("ReadNativeMobileState",F).Invoke(runtime,null);var dock=rt.GetField("_nativeMobileDockState",F).GetValue(runtime);var controls=(IEnumerable)dock.GetType().GetField("Controls",F).GetValue(dock);Check(controls.Cast<object>().Count(c=>(string)c.GetType().GetField("Id",F).GetValue(c)=="debug-levelup")==1,"Missing or duplicate test control");Set(runtime,"_userPaused",true);Set(runtime,"_paused",true);
   var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result="PASS",checks,parts,moved,rigs=rigs.Count,rows,scope="Actual GPU idle/shot/rest;39retained rigs,12unique per family,rigid transforms,no tessellation changes;normal levelup via test control;temporary test preferences must be restored."});File.WriteAllText(Path.Combine(Application.dataPath,"../Library/mechanical-weapons-acceptance.json"),json);return new{result="PASS",checks,parts,rigs=rigs.Count};
  }finally{((IDisposable)weapons).Dispose();((IDisposable)cores).Dispose();((IDisposable)families).Dispose();((IDisposable)feedback).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
