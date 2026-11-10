using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using UnityEngine;using EchoSphere.Core;
public static class ApprovedCreatureCheck {
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static object Call(object o,string n,params object[] args){var m=o.GetType().GetMethods(F).Where(x=>x.Name==n&&x.GetParameters().Length>=args.Length).OrderBy(x=>x.GetParameters().Length).First();return m.Invoke(o,m.GetParameters().Select((p,i)=>i<args.Length?args[i]:p.DefaultValue).ToArray());}
 public static object Main(){
  if(!Application.isPlaying)throw new Exception("Play required");var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var ct=asm.GetType("EchoSphere.Presentation.NativeWorldCreatures");var vt=ct.GetNestedType("EnemyView",F);var st=ct.GetNestedType("BossSignal",F);var go=new GameObject("Approved production creature checks");var surface=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",400f,400f,1f);surface.GetType().GetField("ImmediateWorldForValidation",F).SetValue(surface,true);var creatures=Activator.CreateInstance(ct,true);int checks=0,changed=0,quad=0;var output="Library/approved-creatures";Directory.CreateDirectory(output);var random=JsonUtility.ToJson(UnityEngine.Random.state);
  void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  void Check(bool pass,string why){checks++;if(!pass)throw new Exception(why);}
  HashSet<Mesh> Meshes(){var result=new HashSet<Mesh>();foreach(DictionaryEntry pair in (IDictionary)ct.GetField("_approvedParts",F).GetValue(creatures))foreach(var f in pair.Value.GetType().GetFields(F))if(f.FieldType==typeof(Mesh))result.Add((Mesh)f.GetValue(pair.Value));return result;}
  List<Matrix4x4> Snapshot(HashSet<Mesh> meshes){var result=new List<Matrix4x4>();foreach(var b in (IEnumerable)surface.GetType().GetField("_batches",F).GetValue(surface)){var bt=b.GetType();if(meshes.Contains((Mesh)bt.GetField("NativeMesh",F).GetValue(b))){Check(((Color)bt.GetField("Shadow",F).GetValue(b)).a==0,"anatomy must not request per-part blur");result.Add((Matrix4x4)bt.GetField("NativeMatrix",F).GetValue(b));}}return result;}
  void Draw(object v,double t){Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");Call(creatures,"BeginFrame");Call(creatures,"DrawEnemy",surface,v,t);Call(surface,"End");}
  try{
   var entries=BestiaryCatalog.All.Where(x=>(int)x.Region>=2).ToArray();
   foreach(var entry in entries){var v=Activator.CreateInstance(vt);Set(v,"Identity",new object());Set(v,"RegionStyle",(int)entry.Region);Set(v,"BestiaryType",entry.Type);Set(v,"Role",entry.Role);Set(v,"Variant",entry.Variant);Set(v,"Boss",entry.Boss);Set(v,"Elite",entry.Elite);Set(v,"Radius",entry.Boss?48d:20d);Set(v,"Color",Color.cyan);Set(v,"Hp",100d);Set(v,"MaxHp",100d);Set(v,"BossName","");Set(v,"HasRotation",true);Set(v,"Portrait",true);
    var signal=Activator.CreateInstance(st);Set(signal,"Boss",entry.Boss);Set(signal,"Campaign",true);Set(signal,"Window",1d);Set(signal,"Remaining",0d);Set(v,"Signal",signal);
    int index=(int)Call(creatures,"ApprovedIndex",v);var projected=(BestiaryEntry[][])ct.GetField("ApprovedEntries",F).GetValue(null);Check(projected[(int)entry.Region][index].Id==entry.Id,"exact archive identity "+entry.Id);
    Draw(v,0);Draw(v,.1);var meshes=Meshes();var idle=Snapshot(meshes);Check(idle.Count>15,"complete detailed body "+entry.Id);int count=meshes.Count;
    Set(v,"AttackPose",1d);Set(signal,"Telegraph",true);Set(v,"Signal",signal);Draw(v,.1);var attack=Snapshot(meshes);Check(attack.Count==idle.Count,"attack preserves anatomy topology "+entry.Id);Check(attack.Where((m,i)=>m!=idle[i]).Any(),"attack articulation "+entry.Id);changed++;
    Check(Meshes().Count==count,"attack does not rebuild meshes "+entry.Id);
    Set(v,"Freeze",true);Draw(v,.2);var frozen=Snapshot(meshes);Draw(v,.4);var frozenLater=Snapshot(meshes);Check(frozen.SequenceEqual(frozenLater),"frozen body is stable "+entry.Id);Check(Meshes().Count==count,"freeze does not rebuild meshes");
    if((bool)Call(creatures,"ApprovedQuadruped",v))quad++;
   }
   foreach(int region in new[]{0,1}){var v=Activator.CreateInstance(vt);Set(v,"RegionStyle",region);Set(v,"Fleet",region==1);Check((double)Call(creatures,"RegionalBodyScale",v)==(region==0?1:2d/3d),"first two region scale preserved");}
   Check(JsonUtility.ToJson(UnityEngine.Random.state)==random,"rendering leaves RNG unchanged");
   string result=Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS",checks,entries=entries.Length,attackArticulation=changed,quadrupedVariants=quad,retainedMeshes=Meshes().Count,scope="Actual production DrawEnemy for all region 3-6 identities; idle/attack/freeze, shared mesh reuse, no per-part blur, first-two-region size and RNG"},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(output+"/result.json",result);return result;
  }finally{((IDisposable)creatures).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
