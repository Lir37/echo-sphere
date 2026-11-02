using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.SceneManagement;using EchoSphere.Runtime;using Newtonsoft.Json;
public static class PlayerCopyRuntimeCheck {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 static Type R=typeof(EchoSphereRuntime);static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,F).Invoke(o,args);static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play Mode required");
  var prefs=R.GetNestedType("PlayerPrefs",F);var previous=SceneManager.GetActiveScene();var scene=SceneManager.CreateScene("Player copy check");SceneManager.SetActiveScene(scene);
  string saved=UnityEngine.PlayerPrefs.GetString("echosphere_active_run_v1","absent");var states=new List<object>();
  using((IDisposable)prefs.GetMethod("Isolate",F).Invoke(null,null)){
   try{
    var go=new GameObject("Player copy fixture");go.SetActive(false);var runtime=go.AddComponent<EchoSphereRuntime>();runtime.enabled=false;go.SetActive(true);
    Call(runtime,"CreatePlayer");Call(runtime,"CreateSpheres");Call(runtime,"RefreshNetworkState");
    var surfaceType=R.Assembly.GetType("EchoSphere.Presentation.NativeWorldSurface",true);
    Set(runtime,"_worldSurface",go.AddComponent(surfaceType));
    foreach(bool ru in new[]{true,false}){
     prefs.GetMethod("SetString",F).Invoke(null,new object[]{"echosphere_lang",ru?"ru":"en"});
     string banner="Move closer to the Sphere formation first.";
     Call(runtime,"PrepareNativeFrameBanner",banner,260d,12d);
     string cached=(string)R.GetField("_nativeBannerMessage",F).GetValue(runtime);
     if(cached!=EchoSphere.Presentation.NativeLocalizationCatalog.Instance.CanvasText(banner,ru))throw new Exception("Banner wrapped before localization");
     var lines=(IEnumerable<string>)R.GetField("_nativeBannerLines",F).GetValue(runtime);
     if(ru&&lines.Any(line=>System.Text.RegularExpressions.Regex.IsMatch(line,"[a-zA-Z]")))throw new Exception("Untranslated wrapped banner");
     foreach(string method in new[]{"ReadNativeMainMenuState","ReadNativeCharacterState","ReadNativePauseStatsState","ReadNativePauseAbilitiesState","ReadNativePauseArtifactsState","ReadNativePauseSynergiesState","ReadNativePauseChromeState"}){
      var m=R.GetMethods(F).Single(x=>x.Name==method&&x.GetParameters().Length==0);states.Add(new{language=ru?"ru":"en",screen=method,state=m.Invoke(runtime,null)});
     }
     foreach(string page in new[]{"Shop","Leaderboard","Achievements"}){var field=R.GetField("_mainMenuPage",F);field.SetValue(runtime,Enum.Parse(field.FieldType,page));states.Add(new{language=ru?"ru":"en",screen="progression/"+page,state=Call(runtime,"ReadNativeProgressionState")});}
     foreach(string section in new[]{"basics","spheres","monsters","bosses","abilities","artifacts","evolutions","runes","regions"}){Set(runtime,"_knowledgeSection",section);states.Add(new{language=ru?"ru":"en",screen="archive/"+section,state=Call(runtime,"ReadNativeArchiveState")});}
    }
   }finally{foreach(var go in scene.GetRootGameObjects())UnityEngine.Object.DestroyImmediate(go);SceneManager.SetActiveScene(previous);SceneManager.UnloadSceneAsync(scene);}
  }
  if(saved!=UnityEngine.PlayerPrefs.GetString("echosphere_active_run_v1","absent"))throw new Exception("Live save changed");
  Directory.CreateDirectory("Library/player-copy");File.WriteAllText("Library/player-copy/runtime-states.json",JsonConvert.SerializeObject(states,Formatting.Indented));
  return new{result="PASS",states=states.Count,save="unchanged",output="Library/player-copy/runtime-states.json"};
 }
}
