using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
public static class SubmenuScaleAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();
  var page=type.GetField("_mainMenuPage",F);var menu=type.GetField("_mainMenu",F);var region=type.GetField("_regionSelect",F);var originalPage=page.GetValue(runtime);var originalMenu=menu.GetValue(runtime);var originalRegion=region.GetValue(runtime);
  var names=new[]{"Home","Characters","Shop","Leaderboard","Achievements","Knowledge","Settings","Region"};
  var fields=new[]{"_nativeMainMenuScreen","_nativeCharacterScreen","_nativeProgressionScreen","_nativeProgressionScreen","_nativeProgressionScreen","_nativeArchiveScreen","_nativeSettingsScreen","_nativeRegionScreen"};
  int index=0,frame=0,last=-1,checks=0;double deadline=UnityEditor.EditorApplication.timeSinceStartup+90;
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/submenu-scale"));Directory.CreateDirectory(dir);
  void Select(){menu.SetValue(runtime,true);region.SetValue(runtime,names[index]=="Region");page.SetValue(runtime,Enum.Parse(page.FieldType,names[index]=="Region"?"Home":names[index]));frame=0;}
  UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;menu.SetValue(runtime,originalMenu);region.SetValue(runtime,originalRegion);page.SetValue(runtime,originalPage);var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,pages=names,factor=.86,scope="Actual runtime menu panel scales; home unchanged, no clicks/profile actions"});File.WriteAllText(Path.Combine(dir,"acceptance.json"),json);UnityEditor.SessionState.SetString("EchoSubmenuScale",json);}
  Select();UnityEditor.SessionState.SetString("EchoSubmenuScale","RUNNING");
  tick=()=>{try{
   if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("Deadline "+names[index]);UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;
   if(++frame<15)return;var screen=(Component)type.GetField(fields[index],F).GetValue(runtime);if(screen==null)return;var document=screen.GetComponent<UIDocument>();if(document?.panelSettings==null)return;
   float dpr=Convert.ToSingle(type.GetProperty("SourceDevicePixelRatio",F).GetValue(runtime));float expected=dpr*(index==0?1:.86f);checks++;if(Math.Abs(document.panelSettings.scale-expected)>.0001)throw new Exception(names[index]+" scale "+document.panelSettings.scale+" expected "+expected);
   if(names[index]=="Characters"&&frame==15)ScreenCapture.CaptureScreenshot(Path.Combine(dir,"characters-menu-unity.png"));
   if(frame<18)return;if(++index==names.Length){Finish("PASS");return;}Select();
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.EditorApplication.update+=tick;return "RUNNING";
 }
}
