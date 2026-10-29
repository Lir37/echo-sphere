using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using EchoSphere.Presentation;
public static class ContourCharacterUIAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
  var route=runtime.GetType().GetField("_mainMenuPage",F);var original=route.GetValue(runtime);
  string[] ids={"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"};
  int index=0,frames=0,last=-1,checks=0;var deadline=UnityEditor.EditorApplication.timeSinceStartup+60;
  string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/character-contours"));Directory.CreateDirectory(directory);
  route.SetValue(runtime,Enum.Parse(route.FieldType,"Characters"));
  UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;route.SetValue(runtime,original);var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks});UnityEditor.SessionState.SetString("EchoContourCharacterUI",json);File.WriteAllText(Path.Combine(directory,"ui-acceptance.json"),json);}
  UnityEditor.SessionState.SetString("EchoContourCharacterUI","RUNNING");
  tick=()=>{try{
   if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Portrait scroll deadline at "+index);
   UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;frames++;
   if(index<10){
    var screen=UnityEngine.Object.FindAnyObjectByType<NativeCharacterScreen>();if(screen==null)return;
    var document=screen.GetComponent<UIDocument>();var root=document?.rootVisualElement;if(root==null)return;var card=root.Q("character-"+ids[index]);if(card==null)return;
    var scroll=root.Q<ScrollView>("character-scroll");var image=card.Q<Image>("character-portrait");scroll.ScrollTo(image);
    if(frames<8||image?.sprite==null)return;
    if(image.sprite.texture.width!=1254||image.sprite.texture.height!=1254)throw new Exception("Wrong portrait size "+ids[index]);checks++;
    if(image.sprite.name!=ids[index])throw new Exception("Wrong portrait owner "+ids[index]+": "+image.sprite.name);checks++;
    if(index==0)ScreenCapture.CaptureScreenshot(Path.Combine(directory,"characters-menu-unity.png"));
    index++;frames=0;if(index==10)route.SetValue(runtime,Enum.Parse(route.FieldType,"Home"));
   }else if(frames>15){
    var home=UnityEngine.Object.FindAnyObjectByType<NativeMainMenuScreen>();var root=home.GetComponent<UIDocument>().rootVisualElement;
    var avatar=root.Q<Image>("native-menu-character-portrait");if(avatar?.sprite==null)throw new Exception("Missing home avatar");checks++;
    if(root.Q("native-menu-character-portrait").resolvedStyle.display==DisplayStyle.None)throw new Exception("Home portrait hidden");checks++;
    if(frames==16)ScreenCapture.CaptureScreenshot(Path.Combine(directory,"home-menu-unity.png"));if(frames>18)Finish("PASS");
   }
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
