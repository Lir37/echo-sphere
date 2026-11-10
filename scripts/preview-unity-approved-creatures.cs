// Preview-only native mesh concepts. Never installed in the production enemy path.
using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEditor;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Presentation;using EchoSphere.Core;
public static class ApprovedCreatureRenderPreview
{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static Type T(string name)=>typeof(NativeWorldPresentation).Assembly.GetType("EchoSphere.Presentation."+name,true);
 static object C(object o,string name,params object[] args){var m=o.GetType().GetMethods(F).Where(x=>x.Name==name&&x.GetParameters().Length>=args.Length).OrderBy(x=>x.GetParameters().Length).First();var p=m.GetParameters();return m.Invoke(o,p.Select((x,i)=>i<args.Length?args[i]:x.DefaultValue).ToArray());}
 static int Region;
 static BestiaryEntry[] Entries;
 public static object Region3()=>Run(2);public static object Region4()=>Run(3);public static object Region5()=>Run(4);public static object Region6()=>Run(5);
 public static object Run(int region){Region=region;Entries=BestiaryCatalog.All.Where(e=>(int)e.Region==region).ToArray();return Main();}

 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");string dir="Library/approved-creature-render-"+(Region+1);Directory.CreateDirectory(dir);
  var owner=new GameObject("Isolated Bastion game-sized concept preview");var surface=owner.AddComponent(T("NativeWorldSurface"));C(surface,"Initialize");C(surface,"SetViewport",450f,1000f,1.5f);
  var depth=Activator.CreateInstance(T("NativeWorldDepthBackdrop"),true);var player=Activator.CreateInstance(T("NativeWorldPlayer"),true);var creatures=Activator.CreateInstance(T("NativeWorldCreatures"),true);var feedback=Activator.CreateInstance(T("NativeWorldFeedback"),true);var minions=Activator.CreateInstance(T("NativeWorldMinions"),true);
  var standard=Activator.CreateInstance(T("NativeWorldStandardSphere"),true);
  Matrix4x4 M(float x,float y)=>Matrix4x4.Translate(new Vector3(x,y,0));
  var views=new object[Entries.Length];var viewType=T("NativeWorldCreatures").GetNestedType("EnemyView",F);var signalType=T("NativeWorldCreatures").GetNestedType("BossSignal",F);
  void Creature(int id,float x,float y,double time){
   var entry=Entries[id];object view=views[id]??(views[id]=Activator.CreateInstance(viewType));
   void S(string n,object v)=>viewType.GetField(n,F).SetValue(view,v);
   S("Identity",entry);S("X",(double)x+Math.Sin(time*2)*2);S("Y",(double)y);S("Radius",entry.Boss?48d:20d);S("RegionStyle",Region);S("BestiaryType",entry.Type);S("Role",entry.Role);S("Variant",entry.Variant);S("Boss",entry.Boss);S("Elite",entry.Elite);S("Hp",100d);S("MaxHp",100d);S("BossName",entry.NameRu);S("Color",Region==2?new Color(.68f,.82f,.42f):Region==3?new Color(.48f,.86f,.96f):Region==4?new Color(1,.81f,.39f):new Color(.75f,.48f,.90f));S("FaceTarget",true);S("AimAngle",Math.Atan2(510-y,225-x));S("HasRotation",true);S("Rotation",Math.Atan2(510-y,225-x));
   double attack=Math.Pow(Math.Max(0,Math.Sin(time*2.2+id*.7)),8);S("AttackPose",attack);
   var signal=Activator.CreateInstance(signalType);signalType.GetField("Boss",F).SetValue(signal,entry.Boss);signalType.GetField("Campaign",F).SetValue(signal,true);signalType.GetField("Telegraph",F).SetValue(signal,attack>.02);signalType.GetField("Remaining",F).SetValue(signal,1-attack);signalType.GetField("Window",F).SetValue(signal,1d);S("Signal",signal);
   C(creatures,"DrawEnemy",surface,view,time);
  }
  // Actual retained UI classes, not painted mock HUD labels.
  var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");var rt=new RenderTexture(675,1500,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();panel.targetTexture=rt;panel.scale=1.5f;
  var doc=owner.AddComponent<UIDocument>();doc.panelSettings=panel;var rootUi=doc.rootVisualElement;rootUi.style.position=Position.Absolute;rootUi.style.left=rootUi.style.right=rootUi.style.top=rootUi.style.bottom=0;
  var field=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};field.style.position=Position.Absolute;field.style.left=field.style.right=field.style.top=field.style.bottom=0;rootUi.Add(field);
  var hud=new NativeHudView(rootUi,()=>false,_=>null);hud.Bind(new NativeHudView.State{Russian=true,Level="12",Hp="140/140",XpPercent=40,HpPercent=100,ResonancePercent=45,Resonance="45/100",Spheres="СФЕРЫ 3/5",Wave="ВОЛНА 8",Links="3",Timer="06:42",HasRegion=true,Region=new[]{"РАСКОЛОТЫЙ БАСТИОН","ГРАВИТАЦИОННЫЕ СКЛАДКИ","РЕЗОНАНСНЫЙ ШТОРМ","НУЛЕВОЙ АРХИВ"}[Region-2],Phase="",Pocket=""});
  int page=0,frame=0,last=-1;double started=EditorApplication.timeSinceStartup;bool bg=Application.runInBackground;Application.runInBackground=true;
  void Draw(double time)
  {
   C(surface,"Begin",new Vector2(225,500));C(surface,"ResetTransform");C(depth,"Draw",surface,450f,1000f,Region,Vector2.zero,null,time);C(player,"BeginFrame");C(creatures,"BeginFrame");C(feedback,"BeginFrame",surface);
   for(int slot=0;slot<6;slot++){int id=page*6+slot;if(id>=Entries.Length)break;float x=slot%2==0?102:348,y=230+slot/2*220;Creature(id,x,y,time);C(feedback,"Text",(id+1).ToString("00")+" "+Entries[id].NameEn,(double)x,(double)(y+61),new Color(.8f,.85f,.72f),8f);}
   var ps=Activator.CreateInstance(T("NativeWorldPlayer").GetNestedType("State",F));void S(string n,object v)=>ps.GetType().GetField(n,F).SetValue(ps,v);S("X",225d);S("Y",510d);S("Radius",14d);S("Hp",100d);S("Time",time);S("Character","hunter");S("Color",new Color(.75f,.3f,1));C(player,"Draw",surface,ps,creatures);

   C(standard,"BeginFrame");C(standard,"DrawCore",surface,21d,.7d,(Matrix4x4?)M(181,555));C(standard,"DrawRing",surface,24d,time*.2,.9d,(Matrix4x4?)M(181,555));
   C(surface,"End");field.image=surface.GetType().GetProperty("UiTexture",F).GetValue(surface) as Texture;hud.Prepare(1.5f);
  }
  EditorApplication.CallbackFunction tick=null;void Done(string result){EditorApplication.update-=tick;hud.Dispose();panel.targetTexture=null;foreach(var o in new[]{depth,player,creatures,feedback,minions,standard})((IDisposable)o).Dispose();UnityEngine.Object.Destroy(owner);rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(panel);Application.runInBackground=bg;File.WriteAllText(dir+"/result.json",result);}
  tick=()=>{try{EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(EditorApplication.timeSinceStartup-started>120)throw new Exception("deadline");Draw(2+frame/15d);if(!(bool)depth.GetType().GetProperty("Ready",F).GetValue(depth)||!hud.Ready)return;if(page==0 && frame<24){
    var prev=RenderTexture.active;RenderTexture.active=rt;var f=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);f.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);f.Apply();File.WriteAllBytes(dir+"/motion-"+frame.ToString("D2")+".png",f.EncodeToPNG());UnityEngine.Object.Destroy(f);RenderTexture.active=prev;
   }if(++frame<24)return;
   var old=RenderTexture.active;var shot=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);RenderTexture.active=rt;shot.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);shot.Apply();File.WriteAllBytes(dir+"/game-page-"+(page+1)+".png",shot.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(shot);if(++page==(Entries.Length+5)/6){Done("PASS:"+Entries.Length+" production creatures, region "+(Region+1));return;}frame=0;
  }catch(Exception e){Done(e.ToString());}};EditorApplication.update+=tick;return "Queued region "+(Region+1)+": "+Entries.Length+" production native models";
 }
}
