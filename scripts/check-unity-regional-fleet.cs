using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
public static class RegionalFleetAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var type=asm.GetType("EchoSphere.Presentation.NativeWorldCreatures");var viewType=type.GetNestedType("EnemyView",F);var signalType=type.GetNestedType("BossSignal",F);
  object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  var go=new GameObject("Regional fleet GPU acceptance");var surface=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",900f,600f,1.75f);
  var creatures=type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,Type.EmptyTypes,null).Invoke(null);var roles=new[]{"grunt","swarmer","tank_guard","ranged","sniper","healer","disruptor","charger","phase","leech","splitter","corruptor","scavenger","anchor","bomber","grunt","shooter","charger","summoner","aura","conductor","architect","null","stella_warden"};
  var background=(Mesh)asm.GetType("EchoSphere.Presentation.NativeWorldShapes").GetMethod("Rectangle",F).Invoke(null,new object[]{-450f,-300f,900f,600f});var material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var properties=new MaterialPropertyBlock();properties.SetColor("_NativeColor",new Color(.01f,.025f,.06f,1));
  var views=new object[roles.Length];int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/regional-fleet"));Directory.CreateDirectory(dir);
  try{
   for(int i=0;i<roles.Length;i++){
    var v=Activator.CreateInstance(viewType);views[i]=v;Set(v,"Identity",new object());Set(v,"X",(double)(i%6*150-375));Set(v,"Y",(double)(i/6*150-225));Set(v,"Radius",i>=16?25d:20d);Set(v,"Role",roles[i]);Set(v,"Variant",i>=16?roles[i]:"skitter");Set(v,"Boss",i>=16);Set(v,"BossName",roles[i].ToUpperInvariant());Set(v,"Fleet",true);Set(v,"Elite",i==15);Set(v,"Hp",100d);Set(v,"MaxHp",100d);Set(v,"Color",new Color(.35f,.8f,1));Set(v,"Freeze",i==14);
    var signal=Activator.CreateInstance(signalType);Set(signal,"Boss",i>=16);Set(signal,"Type",roles[i]);Set(signal,"Radius",i>=16?25d:20d);Set(signal,"ChargeTimer",-1d);Set(signal,"ShootTimer",-1d);Set(signal,"SummonTimer",-1d);Set(signal,"ShooterWindow",.7d);Set(signal,"SummonerWindow",.9d);Set(signal,"AuraWindow",.8d);Set(v,"Signal",signal);
   }
   int warmed=0;
   for(int frame=0;frame<5;frame++){
    Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");var draw=surface.GetType().GetMethod("DrawNativeMesh",F);var args=draw.GetParameters().Select(p=>p.DefaultValue).ToArray();args[0]=background;args[1]=material;args[2]=properties;draw.Invoke(surface,args);Call(creatures,"BeginFrame");foreach(var v in views)Call(creatures,"DrawEnemy",surface,v,frame*.13d);Call(surface,"End");
    int count=((IDictionary)type.GetField("_art",F).GetValue(creatures)).Count;if(frame==0)warmed=count;else Check(count==warmed,"Immutable hull geometry rebuilt");
   }
   var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var previous=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
   try{
    RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();Check(image.GetPixels32().Count(p=>p.a>100&&p.g>100)>3000,"Fleet body visible");
    File.WriteAllBytes(Path.Combine(dir,"fleet-unity.png"),image.EncodeToPNG());
   }finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
   var result=new{result="PASS",checks,roles=roles.Take(16),bosses=roles.Skip(16),retainedArt=warmed,scope="Actual fleet render:16regular/elite states,8boss families,immutable art stable over5frames;not Android/performance/art acceptance"};File.WriteAllText(Path.Combine(dir,"acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{Call(creatures,"Dispose");UnityEngine.Object.DestroyImmediate(background);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(go);}
 }
}
