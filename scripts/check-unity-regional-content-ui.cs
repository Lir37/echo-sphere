using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Core;using EchoSphere.Runtime;using EchoSphere.Presentation;
public static class RegionalContentUiAcceptance
{
 public static object Main()
 {
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();bool had=PlayerPrefs.HasKey("echosphere_lang");string old=PlayerPrefs.GetString("echosphere_lang");
  NativeRegionScreen.State Read(string lang){try{PlayerPrefs.SetString("echosphere_lang",lang);return(NativeRegionScreen.State)t.GetMethod("ReadNativeRegionState",F).Invoke(r,null);}finally{if(had)PlayerPrefs.SetString("echosphere_lang",old);else PlayerPrefs.DeleteKey("echosphere_lang");}}
  var states=new[]{Read("ru"),Read("en")};foreach(var state in states)foreach(var row in state.Regions){row.Available=row.Stable=row.Endless=true;row.MasteryMask=7;row.LoreKnown=5;row.Completed=3;foreach(var challenge in row.Challenges)challenge.Completed=true;}
  var go=new GameObject("Regional portrait UI acceptance");var screen=go.AddComponent<NativeRegionScreen>();const float scale=2.75f;screen.Show(states[0],scale,()=>{},(a,b,c)=>{});
  var panel=(PanelSettings)typeof(NativeRegionScreen).GetField("_panel",F).GetValue(screen);var root=(VisualElement)typeof(NativeRegionScreen).GetField("_root",F).GetValue(screen);var scroll=root.Q<ScrollView>("region-scroll");var target=new RenderTexture(1080,2400,24);target.Create();panel.targetTexture=target;panel.clearColor=panel.clearDepthStencil=true;panel.colorClearValue=Color.black;root.style.width=1080/scale;root.style.height=2400/scale;
  int checks=0,frame=0,last=-1;string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/regional-content-ui"));Directory.CreateDirectory(dir);double deadline=UnityEditor.EditorApplication.timeSinceStartup+45;UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
  void Capture(string name){var previous=RenderTexture.active;var image=new Texture2D(1080,2400,TextureFormat.RGBA32,false);try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1080,2400),0,0);image.Apply();Check(image.GetPixels32().Any(p=>p.r>100&&p.g>100),"actual UI text rendered");File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());}finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(image);}}
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;panel.targetTexture=null;UnityEngine.Object.Destroy(go);target.Release();UnityEngine.Object.Destroy(target);var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,languages=new[]{"ru","en"},scope="Actual UI Toolkit portrait1080x2400 rendering,gesture-only vertical scroll,horizontal bounds and Convergence reachability;synthetic unlocked view,no progression writes,not Android acceptance"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoRegionalContentUi",json);}
  tick=()=>{try{
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out");frame++;
   int language=frame<=24?0:1;screen.Show(states[language],scale,()=>{},(a,b,c)=>{});var background=typeof(NativeRegionScreen).GetField("_background",F).GetValue(screen);background.GetType().GetMethod("SetViewport",F).Invoke(background,new object[]{1080/scale,2400/scale,scale});
   if(frame==12||frame==36)
   {
    Check(root.worldBound.width>350&&root.worldBound.width<400,"portrait layout at requested panel scale");Check(scroll.mode==ScrollViewMode.Vertical&&scroll.horizontalScrollerVisibility==ScrollerVisibility.Hidden&&scroll.verticalScrollerVisibility==ScrollerVisibility.Hidden,"touch-only scroll without desktop bars");
    foreach(var row in states[language].Regions){var card=root.Q<VisualElement>("region-"+row.Id);Check(card.worldBound.xMin>=root.worldBound.xMin-1&&card.worldBound.xMax<=root.worldBound.xMax+1,"region card within horizontal bounds");foreach(var label in card.Query<Label>().ToList())Check(label.worldBound.xMin>=card.worldBound.xMin-1&&label.worldBound.xMax<=card.worldBound.xMax+1,"label remains within card: "+label.name);}
    Capture(language==0?"regions-ru-top.png":"regions-en-top.png");scroll.scrollOffset=new Vector2(0,scroll.verticalScroller.highValue);
   }
   if(frame==20||frame==44){var button=root.Q<Button>("start-convergence");Check(button.worldBound.Overlaps(scroll.contentViewport.worldBound),"Convergence reachable by vertical scroll");Capture(language==0?"regions-ru-bottom.png":"regions-en-bottom.png");}
   if(frame==24)scroll.scrollOffset=Vector2.zero;
   if(frame==45)Finish("PASS");
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.SessionState.SetString("EchoRegionalContentUi","RUNNING");UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
