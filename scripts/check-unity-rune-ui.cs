using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Core;using EchoSphere.Presentation;
public static class RuneUiAcceptance
{
 public static object Main(){
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  int checks=0;void A(bool v,string why){checks++;if(!v)throw new Exception(why);}
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/rune-ui"));Directory.CreateDirectory(dir);
  object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);
  var asm=typeof(NativeArchiveScreen).Assembly;var gpuGo=new GameObject("Rune GPU acceptance");var surface=gpuGo.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));var pickables=Activator.CreateInstance(asm.GetType("EchoSphere.Presentation.NativeWorldPickableViews"),true);var feedback=Activator.CreateInstance(asm.GetType("EchoSphere.Presentation.NativeWorldFeedback"),true);
  try{
   Call(surface,"Initialize");Call(surface,"SetViewport",600f,1000f,1f);surface.GetType().GetField("ImmediateWorldForValidation",F)?.SetValue(surface,true);
   for(int frame=0;frame<2;frame++){
    Call(surface,"Begin",new Vector2(300,500));Call(surface,"ResetTransform");Call(feedback,"BeginFrame",surface);Call(feedback,"Background",600d,1000d,new Color(.01f,.02f,.04f,1),Color.black);Call(pickables,"BeginFrame");
    for(int i=0;i<10;i++){string id=RuneRules.TypeAt(i);var definition=NativeRunCatalog.Instance.GetRune(id);ColorUtility.TryParseHtmlString(definition.Color,out var color);Call(pickables,"DrawRune",surface,150d+300*(i%2),100d+200*(i/2),id,color,frame*.2d);}
    Call(surface,"End");var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
    try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();for(int i=0;i<10;i++){int x=150+300*(i%2),y=rt.height-(100+200*(i/2));int visible=0;for(int yy=y-25;yy<y+25;yy++)for(int xx=x-25;xx<x+25;xx++){var p=image.GetPixel(xx,yy);if(Mathf.Max(p.r,Mathf.Max(p.g,p.b))>.3f)visible++;}A(visible>20,"actual GPU rune "+RuneRules.TypeAt(i)+" frame"+frame);}File.WriteAllBytes(Path.Combine(dir,"runes-"+frame+".png"),image.EncodeToPNG());}
    finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
   }
  }finally{Call(pickables,"Dispose");Call(feedback,"Dispose");UnityEngine.Object.DestroyImmediate(gpuGo);}
  NativeArchiveScreen.State State(bool ru)=>new NativeArchiveScreen.State{Russian=ru,Section="runes",Query="",Known=0,Cards=Enumerable.Range(0,10).Select(i=>new NativeArchiveScreen.Card{Id="rune:"+RuneRules.TypeAt(i),Title=RuneRules.Name(RuneRules.TypeAt(i),ru),Description=RuneRules.Description(RuneRules.TypeAt(i),ru),Details="22s",Open=true}).ToArray()};
  var go=new GameObject("Rune archive UI acceptance");var screen=go.AddComponent<NativeArchiveScreen>();const float scale=2.75f;screen.Show(State(true),scale,()=>{},_=>{},_=>{});
  var panel=(PanelSettings)typeof(NativeArchiveScreen).GetField("_panel",F).GetValue(screen);var root=(VisualElement)typeof(NativeArchiveScreen).GetField("_root",F).GetValue(screen);var scroll=(ScrollView)typeof(NativeArchiveScreen).GetField("_scroll",F).GetValue(screen);var nav=(ScrollView)typeof(NativeArchiveScreen).GetField("_navigation",F).GetValue(screen);var target=new RenderTexture(1080,2400,24);target.Create();panel.targetTexture=target;panel.clearColor=panel.clearDepthStencil=true;panel.colorClearValue=Color.black;root.style.width=1080/scale;root.style.height=2400/scale;
  var states=new[]{State(true),State(false)};int frameCount=0,last=-1;double deadline=UnityEditor.EditorApplication.timeSinceStartup+45;UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Capture(string name){var old=RenderTexture.active;var image=new Texture2D(1080,2400,TextureFormat.RGBA32,false);try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1080,2400),0,0);image.Apply();A(image.GetPixels32().Count(p=>p.r>110&&p.g>110)>1000,"actual archive text rendered");File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.Destroy(image);}}
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;panel.targetTexture=null;UnityEngine.Object.Destroy(go);target.Release();UnityEngine.Object.Destroy(target);var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,languages=new[]{"ru","en"},runes=10,scope="Actual GPU glyphs and retained UI Toolkit portrait1080x2400,scroll reachability and horizontal bounds;not Android acceptance"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoRuneUi",json);}
  tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out");frameCount++;int language=frameCount<=24?0:1;
   screen.Show(states[language],scale,()=>{},_=>{},_=>{});scroll=(ScrollView)typeof(NativeArchiveScreen).GetField("_scroll",F).GetValue(screen);nav=(ScrollView)typeof(NativeArchiveScreen).GetField("_navigation",F).GetValue(screen);var bg=typeof(NativeArchiveScreen).GetField("_background",F).GetValue(screen);Call(bg,"SetViewport",1080/scale,2400/scale,scale);
   if(frameCount==12||frameCount==36){
    A(root.Query<Button>().ToList().Count(b=>b.name.StartsWith("archive-section-"))==9,"all nine navigation tabs");A(scroll.mode==ScrollViewMode.Vertical&&scroll.horizontalScrollerVisibility==ScrollerVisibility.Hidden&&scroll.verticalScrollerVisibility==ScrollerVisibility.Hidden,"mobile gesture-only scroll");
    foreach(var label in root.Query<Label>().ToList().Where(l=>l.name=="archive-card-title"||l.name=="archive-card-description"))A(label.worldBound.xMin>=root.worldBound.xMin-1&&label.worldBound.xMax<=root.worldBound.xMax+1,"rune label horizontal bounds");
    Capture(language==0?"archive-ru-top.png":"archive-en-top.png");scroll.scrollOffset=new Vector2(0,scroll.verticalScroller.highValue);nav.ScrollTo(root.Q<Button>("archive-section-runes"));
   }
   if(frameCount==20||frameCount==44){var cell=root.Query<Label>().ToList().First(l=>l.text==RuneRules.Name("runic_cell",language==0));A(cell.worldBound.Overlaps(scroll.contentViewport.worldBound),"last rune reachable");Capture(language==0?"archive-ru-bottom.png":"archive-en-bottom.png");A(root.Q<Button>("archive-section-runes").worldBound.Overlaps(nav.contentViewport.worldBound),"rune tab reachable; offset="+nav.scrollOffset+" high="+nav.verticalScroller.highValue+" viewport="+nav.contentViewport.worldBound+" button="+root.Q<Button>("archive-section-runes").worldBound);}
   if(frameCount==24){scroll.scrollOffset=nav.scrollOffset=Vector2.zero;}if(frameCount==45)Finish("PASS");
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.SessionState.SetString("EchoRuneUi","RUNNING");UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
