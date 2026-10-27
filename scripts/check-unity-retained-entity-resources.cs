using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
public static class EntityViewAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var wt=asm.GetType("EchoSphere.Presentation.NativeWorldSurface");var ct=asm.GetType("EchoSphere.Presentation.NativeWorldCreatures");var vt=ct.GetNestedType("EnemyView",flags);
  object Call(object o,string n,params object[] v){var m=o.GetType().GetMethod(n,flags);var ps=m.GetParameters();var args=new object[ps.Length];for(int i=0;i<args.Length;i++)args[i]=i<v.Length?v[i]:ps[i].DefaultValue;return m.Invoke(o,args);}
  var go=new GameObject("Entity view acceptance");var world=go.AddComponent(wt);Call(world,"Initialize");Call(world,"SetViewport",320f,240f,1f);
  var creatures=Activator.CreateInstance(ct,true);var ft=asm.GetType("EchoSphere.Presentation.NativeWorldFeedback");var feedback=Activator.CreateInstance(ft,true);int checks=0;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  IDictionary Views()=>(IDictionary)ct.GetField("_entityViews",flags).GetValue(creatures);
  void Begin(){Call(world,"Begin",new Vector2(160,120));Call(creatures,"BeginFrame");Call(feedback,"BeginFrame",world);}
  void Draw(object identity){var view=Activator.CreateInstance(vt);foreach(var kv in new[]{("Identity",identity),("Radius",(object)19d),("Color",(object)Color.white),("Variant",(object)"wisp"),("Hp",(object)10d),("MaxHp",(object)10d)})vt.GetField(kv.Item1,flags).SetValue(view,kv.Item2);Call(creatures,"DrawEnemy",world,view,1d);}
  try{
   var a=new object();var b=new object();Begin();Draw(a);Draw(b);var ra=Views()[a];var rb=Views()[b];Check(!ReferenceEquals(ra,rb),"entity resources shared mutable slots");
   var eventA=new object();var eventB=new object();Call(feedback,"EventText",eventA,"10",0d,0d,Color.white,14f);Call(feedback,"EventText",eventB,"20",0d,0d,Color.white,14f);var labels=(IDictionary)ft.GetField("_eventLabels",flags).GetValue(feedback);var oldLabel=labels[eventB];
   Begin();Draw(b);Check(ReferenceEquals(rb,Views()[b]),"survivor resources changed after preceding death");Call(feedback,"EventText",eventB,"20",0d,0d,Color.white,14f);Check(ReferenceEquals(oldLabel,labels[eventB]),"survivor label changed after preceding death");
   Begin();Check(!Views().Contains(a)&&Views().Contains(b),"retirement timing");Check(!labels.Contains(eventA)&&labels.Contains(eventB),"event retirement timing");
   Begin();Check(Views().Count==0&&labels.Count==0,"dead owners retained");
   var c=new object();Draw(c);Check(ReferenceEquals(Views()[c],ra)||ReferenceEquals(Views()[c],rb),"retired views not reused");
   return new {result="PASS",checks};
  }finally{((IDisposable)creatures).Dispose();((IDisposable)feedback).Dispose();UnityEngine.Object.Destroy(go);}
 }
}
