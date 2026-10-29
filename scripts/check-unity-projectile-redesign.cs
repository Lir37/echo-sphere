using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
public static class ProjectileRedesignAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var wt=a.GetType("EchoSphere.Presentation.NativeWorldSurface");var ft=a.GetType("EchoSphere.Presentation.NativeWorldSphereFamilies");var pt=ft.GetNestedType("ProjectileView",F);
  object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,F).Invoke(o,args);
  void Set(object o,string n,object value)=>o.GetType().GetField(n,F).SetValue(o,value);
  var go=new GameObject("Projectile redesign GPU fixture");var world=go.AddComponent(wt);Call(world,"Initialize");Call(world,"SetViewport",1000f,650f,1.5f);
  var hostile=Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldProjectiles"),true);var friendly=Activator.CreateInstance(ft,true);var text=Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldFeedback"),true);
  var shapes=a.GetType("EchoSphere.Presentation.NativeWorldShapes");var version=shapes.GetMethod("GeometryVersion",F);var versions=new Dictionary<Mesh,ulong>();
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  void Begin(){Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(hostile,"BeginFrame");Call(friendly,"BeginFrame");Call(text,"BeginFrame",world);}
  object Model(string family,string branch,double x,double y,double r){var p=Activator.CreateInstance(pt);Set(p,"Family",family);Set(p,"Branch",branch);Set(p,"X",x);Set(p,"Y",y);Set(p,"Radius",r);Set(p,"Vx",300d);Set(p,"Vy",0d);ColorUtility.TryParseHtmlString(family=="sniper"||family=="void"?"#d66bff":family=="shotgun"?"#ffb54b":"#55dcff",out var c);Set(p,"Color",c);return p;}
  void Draw(double time){
   Begin();Call(text,"Rectangle",-500d,-325d,1000d,650d,0d,new Color(.012f,.030f,.065f,1));Call(text,"Text","PROJECTILES — Unity / actual + enlarged",0d,-280d,Color.white,18f);
   string[] families={"standard","sniper","shotgun","prism","void"};
   for(int i=0;i<6;i++){double x=-400+i%3*310,y=-170+i/3*140;var color=i==1?new Color(1,.87f,.30f):i<2?new Color(1,.38f,.29f):new Color(.76f,.40f,1);
    Call(hostile,"Draw",world,(float)x,(float)y,8f,300d,0d,i,color,time);Call(hostile,"Draw",world,(float)(x+95),(float)(y+15),3f,300d,0d,i,color,time);
    Call(text,"Text","Enemy "+i,x+30,y+55,Color.white,13f);
   }
   for(int i=0;i<5;i++){double x=-400+i*190;Call(friendly,"DrawProjectile",world,Model(families[i],"",x,170,7));Call(friendly,"DrawProjectile",world,Model(families[i],"",x+40,210,3));Call(text,"Text",families[i],x,270d,Color.white,13f);}
   Call(world,"End");
  }
  try{
   Draw(1);
   foreach(DictionaryEntry pair in (IDictionary)hostile.GetType().GetField("_art",F).GetValue(hostile))foreach(var name in new[]{"Spine","Fins","Wake"}){var mesh=(Mesh)pair.Value.GetType().GetField(name,F).GetValue(pair.Value);Check(mesh!=null&&mesh.vertexCount>0,"Missing hostile detail");versions[mesh]=(ulong)version.Invoke(null,new object[]{mesh});}
   foreach(DictionaryEntry pair in (IDictionary)ft.GetField("_art",F).GetValue(friendly)){int kind=(int)pair.Key.GetType().GetField("Item1").GetValue(pair.Key);if(kind<550||kind>574)continue;foreach(var name in new[]{"Fill","Stroke"}){var mesh=(Mesh)pair.Value.GetType().GetField(name,F).GetValue(pair.Value);if(mesh!=null)versions[mesh]=(ulong)version.Invoke(null,new object[]{mesh});}}
   for(int i=0;i<4;i++)Draw(1.2+i*.1);foreach(var pair in versions)Check((ulong)version.Invoke(null,new object[]{pair.Key})==pair.Value,"Projectile detail rebuilt while flying");
   var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/projectile-redesign"));Directory.CreateDirectory(dir);
   try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,"projectiles-unity.png"),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
   var result=new{result="PASS",checks,enemyKinds=6,friendlyFamilies=5,retainedDetailMeshes=versions.Count,scope="Actual native GPU, warm geometry stable, no simulation/trajectory/radius changes"};File.WriteAllText(Path.Combine(dir,"acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{((IDisposable)hostile).Dispose();((IDisposable)friendly).Dispose();((IDisposable)text).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
