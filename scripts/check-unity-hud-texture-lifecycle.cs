using System;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using EchoSphere.Presentation;
using EchoSphere.Core;
public static class HudTextureLifecycleAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var surfaceType=asm.GetType("EchoSphere.Presentation.NativeHudSurface");var cacheType=asm.GetType("EchoSphere.Presentation.NativeMenuBrushCache");
  object Field(object obj,string name)=>obj.GetType().GetField(name,F).GetValue(obj);
  object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,F).Invoke(obj,args);
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  var go=new GameObject("HUD texture lifetime acceptance");var target=new RenderTexture(1080,2400,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();
  var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=2.75f;panel.forceGammaRendering=true;panel.targetTexture=target;panel.clearColor=true;panel.clearDepthStencil=true;panel.colorClearValue=Color.clear;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
  var doc=go.AddComponent<UIDocument>();doc.panelSettings=panel;var parent=doc.rootVisualElement;parent.style.position=Position.Absolute;parent.style.left=parent.style.right=parent.style.top=parent.style.bottom=0;
  var hud=new NativeHudView(parent,()=>false,_=>null);var probe=new VisualElement();probe.style.position=Position.Absolute;probe.style.top=600;probe.style.left=10;probe.style.width=97;probe.style.height=11;parent.Add(probe);
  var probeSurface=Activator.CreateInstance(surfaceType,F,null,new object[]{probe,new NativeMenuBrush.Paint{Fill=new Color(.1f,.4f,.2f,1)}},null);
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/hud-texture-lifecycle"));Directory.CreateDirectory(dir);
  var names=new[]{"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"};int frame=0,last=-1;RenderTexture old=null;bool bg=Application.runInBackground;Application.runInBackground=true;
  var deadline=UnityEditor.EditorApplication.timeSinceStartup+180;UnityEditor.EditorApplication.CallbackFunction tick=null;
  NativeMenuBrush Brush(object surface)=>(NativeMenuBrush)Field(Field(surface,"_skin"),"Brush");
  object Surface(string name){foreach(var s in (IEnumerable)Field(hud,"_surfaces"))if(((VisualElement)surfaceType.GetProperty("Host",F).GetValue(s)).name==name)return s;throw new Exception("Missing surface "+name);}
  void Capture(int index){
   var previous=RenderTexture.active;var pixels=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
   try{RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();
    Color At(float x,float y)=>pixels.GetPixel(Mathf.Clamp((int)(x*panel.scale),0,target.width-1),Mathf.Clamp(target.height-1-(int)(y*panel.scale),0,target.height-1));
    var right=hud.Root.Q("hud-right").worldBound;var dark=At(right.x+6,right.y+6);Check(dark.r+dark.g+dark.b<1.2,"White right HUD: "+names[index]);
    var hp=hud.Root.Q("hud-hp-bar-fill").worldBound;var red=At(hp.x+hp.width*.25f,hp.center.y);Check(red.r>.35&&red.r>red.g*1.5,"White/missing HP bar: "+names[index]);
    File.WriteAllBytes(Path.Combine(dir,names[index]+".png"),pixels.EncodeToPNG());
   }finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(pixels);}
  }
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;doc.enabled=false;hud.Dispose();((IDisposable)probeSurface).Dispose();UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(panel);target.Release();UnityEngine.Object.Destroy(target);Application.runInBackground=bg;
   var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,characters=10,retirementFrames=3,scope="Offscreen actual UIToolkit GPU: dynamic HP/radar, ten character display states, delayed texture retirement, forced GPU-target loss + recreation/rebind; live gameplay untouched"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoHudTextureLifecycle",json);
  }
  UnityEditor.SessionState.SetString("EchoHudTextureLifecycle","RUNNING");
  tick=()=>{try{
   if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Deadline at "+frame);
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;frame++;
   int index=Math.Min(9,Math.Max(0,(frame-35)/18));CharacterRules.TryParseSourceId(names[index],out var id);var definition=CharacterRules.Get(id);
   hud.Bind(new NativeHudView.State{Russian=true,HasRegion=true,Level="УРОВЕНЬ 2",Hp="88 / 100",XpPercent=35,HpPercent=90-frame%25,Resonance="30 / 100",ResonancePercent=30,Spheres="3 / 5",Wave="ВОЛНА 2",Links="3",Timer="00:37",Region=definition.NameRu,Phase="ДВИЖЕНИЕ",Pocket="ПРОВЕРКА HUD",PlayerX=frame*2,PlayerY=frame,
    Enemies=new[]{new NativeHudRadarView.Enemy(300+Math.Sin(frame*.1)*200,250,true,false,false)},Pois=new[]{new NativeHudRadarView.Poi(1800,900,true,"boss_trace")}});
   hud.Prepare(panel.scale);Call(probeSurface,"Prepare",panel.scale);
   if(frame==10){old=Brush(probeSurface).Texture;Check(old!=null&&old.IsCreated(),"Probe texture missing");Call(probeSurface,"SetPaint",new NativeMenuBrush.Paint{Fill=Color.red});Call(probeSurface,"Prepare",panel.scale);Check(old.IsCreated(),"Old texture released before retained UI can retire it");}
   if(frame==11)Check(old!=null&&old.IsCreated(),"Old texture destroyed in following frame");
   if(frame==17)Check(old==null||!old.IsCreated(),"Retired texture leaked after grace period");
   if(frame==25){Brush(Surface("hud-right")).Texture.Release();Brush(Surface("hud-hp-bar-fill")).Texture.Release();}
   if(frame==27){Check(Brush(Surface("hud-right")).Texture.IsCreated(),"Right HUD lost target not recovered");Check(Brush(Surface("hud-hp-bar-fill")).Texture.IsCreated(),"HP lost target not recovered");}
   if(frame>=35&&(frame-35)%18==17){Check(hud.Ready,"HUD not ready");Capture(index);Check((int)cacheType.GetProperty("RetiredCount",F).GetValue(null)<64,"Unbounded retired skin queue");}
   if(frame>=215)Finish("PASS");
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
