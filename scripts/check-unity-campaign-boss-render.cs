using System;using System.IO;using System.Linq;using System.Collections;using System.Reflection;using UnityEngine;using EchoSphere.Core;
public static class CampaignBossRenderAcceptance
{
 public static object Main()
 {
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var type=asm.GetType("EchoSphere.Presentation.NativeWorldCreatures");var viewType=type.GetNestedType("EnemyView",F);var signalType=type.GetNestedType("BossSignal",F);
  object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  var go=new GameObject("Campaign boss GPU acceptance");var surface=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",1800f,3600f,1f);
  var creatures=Activator.CreateInstance(type,true);var background=(Mesh)asm.GetType("EchoSphere.Presentation.NativeWorldShapes").GetMethod("Rectangle",F).Invoke(null,new object[]{-900f,-1800f,1800f,3600f});var material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var properties=new MaterialPropertyBlock();properties.SetColor("_NativeColor",new Color(.01f,.025f,.06f,1));
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/campaign-boss-render"));Directory.CreateDirectory(dir);int checks=0;void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
  try
  {
   for(int phase=0;phase<2;phase++)for(int attack=0;attack<3;attack++)
   {
    var views=new object[8];for(int i=0;i<8;i++)
    {
     string boss=BossCombatRules.GetTypeForDefeatedCount(i);var p=CampaignBossRules.Pattern(boss,attack,phase);var v=Activator.CreateInstance(viewType);views[i]=v;Set(v,"Identity",new object());Set(v,"X",i%2==0?-450d:450d);Set(v,"Y",(double)(i/2*900-1350));Set(v,"Radius",34d);Set(v,"Boss",true);Set(v,"Variant",boss);Set(v,"BossName",boss+" / "+p.Attack);Set(v,"Hp",phase==0?100d:40d);Set(v,"MaxHp",100d);ColorUtility.TryParseHtmlString(BossCombatRules.GetColorHex(boss),out var color);Set(v,"Color",color);
     var signal=Activator.CreateInstance(signalType);Set(signal,"Boss",true);Set(signal,"Campaign",true);Set(signal,"Telegraph",true);Set(signal,"Type",boss);Set(signal,"Radius",34d);Set(signal,"Phase",phase);Set(signal,"Attack",(int)p.Attack);Set(signal,"Count",p.Count);Set(signal,"Range",(double)p.Radius);Set(signal,"Remaining",(double)(p.Windup*.4f));Set(signal,"Window",(double)p.Windup);Set(signal,"AimX",1d);Set(signal,"AimY",0d);Set(v,"Signal",signal);
    }
    for(int frame=0;frame<2;frame++)
    {
     Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");var draw=surface.GetType().GetMethod("DrawNativeMesh",F);var args=draw.GetParameters().Select(p=>p.DefaultValue).ToArray();args[0]=background;args[1]=material;args[2]=properties;draw.Invoke(surface,args);Call(creatures,"BeginFrame");foreach(var v in views)Call(creatures,"DrawEnemy",surface,v,frame*.15d);Call(surface,"End");
    }
    var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var previous=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
    try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();Check(image.GetPixels32().Count(p=>p.r>100&&p.a>100)>5000,"actual boss bodies and warning lines visible");File.WriteAllBytes(Path.Combine(dir,"phase-"+(phase+1)+"-attack-"+(attack+1)+".png"),image.EncodeToPNG());}
    finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
   }
   var result=new{result="PASS",checks,sheets=6,bossStates=48,scope="Actual Unity GPU rendering of24warnings in both phases;not Android or visual designer acceptance"};File.WriteAllText(Path.Combine(dir,"acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }
  finally{Call(creatures,"Dispose");UnityEngine.Object.DestroyImmediate(background);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(go);}
 }
}
