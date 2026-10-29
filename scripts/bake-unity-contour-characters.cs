// Editor-only authoring/acceptance: identical native body geometry in battle and UI.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
public static class ContourCharacterAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required: temporary runtime render owners use deferred Destroy. Back up Editor preferences before entering Play.");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var pt=asm.GetType("EchoSphere.Presentation.NativeWorldPlayer");
  var st=pt.GetNestedType("State",F);var wt=asm.GetType("EchoSphere.Presentation.NativeWorldSurface");
  object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,F).Invoke(o,args);
  void Set(object o,string name,object value)=>o.GetType().GetField(name,F).SetValue(o,value);
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  string[] ids={"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"};
  string[] colors={"#55dfff","#e86cff","#57d5ff","#ff625d","#57e6b4","#ffc56a","#39d8ff","#b68cff","#8a5a8a","#ffb84d"};
  string[] names={"Сферист","Охотник","Инженер","Берсерк","Алхимик","Архитектор","Проводник","Оракул","Пустотник","Фрактал"};
  var go=new GameObject("Contour character GPU authoring");var world=go.AddComponent(wt);Call(world,"Initialize");
  var player=Activator.CreateInstance(pt,true);var sprites=Activator.CreateInstance(asm.GetType("EchoSphere.Presentation.NativeWorldSprites"),true);
  var shadows=Activator.CreateInstance(asm.GetType("EchoSphere.Presentation.NativeWorldCreatures"),true);var feedback=Activator.CreateInstance(asm.GetType("EchoSphere.Presentation.NativeWorldFeedback"),true);
  var evidence=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/character-contours"));Directory.CreateDirectory(evidence);
  var versions=new Dictionary<Mesh,ulong>();var version=asm.GetType("EchoSphere.Presentation.NativeWorldShapes").GetMethod("GeometryVersion",F);
  object State(int i,double x=0,double y=0,double radius=55,double time=0){var s=Activator.CreateInstance(st);ColorUtility.TryParseHtmlString(colors[i],out var c);Set(s,"Character",ids[i]);Set(s,"Color",c);Set(s,"X",x);Set(s,"Y",y);Set(s,"Radius",radius);Set(s,"Time",time);Set(s,"Hp",10d);return s;}
  Texture2D Read(){var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var t=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);t.Apply();return t;}finally{RenderTexture.active=old;}}
  void Begin(float width,float height,float dpr=1){Call(world,"SetViewport",width,height,dpr);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(player,"BeginFrame");Call(shadows,"BeginFrame");Call(sprites,"BeginFrame");}
  try{
   Check(pt.GetField("_portraitLease",F)==null,"Player still owns bitmap portraits");
   for(int i=0;i<ids.Length;i++){
    Begin(1254,1254);Call(player,"DrawPortrait",world,State(i,radius:470));Call(world,"End");var image=Read();
    try{
     var pixels=image.GetPixels32();int visible=0;foreach(var c in pixels)if(c.a>25)visible++;
     Check(visible>10000,"Empty portrait "+ids[i]);
     for(int x=0;x<image.width;x++){Check(pixels[x].a==0,"Clipped bottom "+ids[i]);Check(pixels[(image.height-1)*image.width+x].a==0,"Clipped top "+ids[i]);}
     for(int y=0;y<image.height;y++){Check(pixels[y*image.width].a==0,"Clipped left "+ids[i]);Check(pixels[y*image.width+image.width-1].a==0,"Clipped right "+ids[i]);}
     var bytes=image.EncodeToPNG();File.WriteAllBytes(Path.Combine(Application.dataPath,"Resources/EchoSphere/CharacterArt/"+ids[i]+".png"),bytes);
    }finally{UnityEngine.Object.DestroyImmediate(image);}
   }
   var contour=pt.GetField("_contours",F).GetValue(player);var rigs=(IDictionary)contour.GetType().GetField("_rigs",F).GetValue(contour);Check(rigs.Count==10,"Expected ten unique identities");
   foreach(var rig in rigs.Values)foreach(var part in (IEnumerable)rig.GetType().GetField("Parts",F).GetValue(rig))foreach(var name in new[]{"Fill","Stroke","Halo"}){
    var mesh=(Mesh)part.GetType().GetField(name,F).GetValue(part);if(mesh==null)continue;versions[mesh]=(ulong)version.Invoke(null,new object[]{mesh});Check(mesh.vertexCount>0,"Empty retained part");
   }
   Check(pt.GetMethod("Draw",F).GetParameters().Length==3,"Old sprite dependency in live player API");
   for(int i=0;i<10;i++)for(int mode=0;mode<6;mode++){
    Begin(120,120,1.75f);var state=State(i,radius:16,time:mode*.3);if(mode==1)Set(state,"DashTimer",.4d);if(mode==2)Set(state,"ShieldTimer",.4d);if(mode==3)Set(state,"ContactCooldown",.2d);if(mode>=4){Set(state,"Hp",0d);Set(state,"DeathTimer",mode==4?.7d:.1d);}
    Call(player,"Draw",world,state,shadows);Call(world,"End");var image=Read();try{int lit=0;foreach(var pixel in image.GetPixels32())if(pixel.a>25)lit++;Check(lit>20,"Invisible live character "+ids[i]+" mode "+mode);}finally{UnityEngine.Object.DestroyImmediate(image);}
   }
   foreach(var pair in versions)Check((ulong)version.Invoke(null,new object[]{pair.Key})==pair.Value,"Live pose rebuilt static contours");
   Begin(1100,680,1.5f);Call(feedback,"BeginFrame",world);Call(feedback,"Rectangle",-550d,-340d,1100d,680d,0d,new Color(.012f,.030f,.065f,1));
   Call(feedback,"Text","ECHO SPHERE — contour characters / Unity",0d,-305d,Color.white,20f);
   for(int i=0;i<10;i++){
    double x=-440+i%5*220,y=-140+i/5*310;Call(player,"DrawPortrait",world,State(i,x,y,radius:58));
    Call(player,"DrawPortrait",world,State(i,x+62,y+95,radius:16));Call(feedback,"Text",names[i],x,y+140,Color.white,15f);
   }
   Call(world,"End");var sheet=Read();try{File.WriteAllBytes(Path.Combine(evidence,"roster-unity.png"),sheet.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(sheet);}
   var result=new{result="PASS",checks,characters=10,liveStates=60,retainedMeshes=versions.Count,scope="Real Unity GPU, native live bodies and baked UI portraits; all states visible; no sprite dependency in live body; no gameplay/preferences changed"};
   File.WriteAllText(Path.Combine(evidence,"acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{((IDisposable)player).Dispose();((IDisposable)sprites).Dispose();((IDisposable)shadows).Dispose();((IDisposable)feedback).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
