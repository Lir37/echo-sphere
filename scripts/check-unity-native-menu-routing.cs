// Exercise actual runtime page routing and UIDocument recreation, without save edits.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
var type=runtime.GetType();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object Get(string name)=>type.GetField(name,flags).GetValue(runtime);
void Set(string name,object value)=>type.GetField(name,flags).SetValue(runtime,value);
var source=(EchoSphere.Presentation.SourceUiDocument)Get("_sourceUi");
var document=source==null?null:source.GetComponent<UnityEngine.UIElements.UIDocument>();
var oldFlag=(bool)Get("_nativeMainMenuEnabled");var oldPage=Get("_mainMenuPage");var oldRegion=Get("_regionSelect");
var background=UnityEngine.Application.runInBackground;var paused=UnityEditor.EditorApplication.isPaused;
Set("_nativeMainMenuEnabled",true);UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
int phase=0,frame=UnityEngine.Time.frameCount;var start=UnityEditor.EditorApplication.timeSinceStartup;
var routes=new[]{EchoSphere.Presentation.NativeMainMenuView.Destination.Characters,EchoSphere.Presentation.NativeMainMenuView.Destination.Shop,EchoSphere.Presentation.NativeMainMenuView.Destination.Leaderboard,EchoSphere.Presentation.NativeMainMenuView.Destination.Achievements,EchoSphere.Presentation.NativeMainMenuView.Destination.Knowledge,EchoSphere.Presentation.NativeMainMenuView.Destination.Settings,EchoSphere.Presentation.NativeMainMenuView.Destination.Map};
var home=System.Enum.Parse(oldPage.GetType(),"Home");int index=0;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){UnityEditor.EditorApplication.update-=tick;Set("_nativeMainMenuEnabled",oldFlag);Set("_mainMenuPage",oldPage);Set("_regionSelect",oldRegion);if(source!=null)source.Dirty=true;UnityEngine.Application.runInBackground=background;UnityEditor.EditorApplication.isPaused=paused;UnityEditor.SessionState.SetString("EchoNativeMenuRouting",result);}
void Require(bool value,string message){if(!value)throw new System.Exception(message);}
void Submit(UnityEngine.UIElements.VisualElement element){Require(element!=null&&element.panel!=null,"Detached control");using(var evt=UnityEngine.UIElements.NavigationSubmitEvent.GetPooled()){evt.target=element;element.SendEvent(evt);}}
string Text(EchoSphere.Presentation.VisualCssNode node){var text=node.Text??"";foreach(var child in node.Children)text+=Text(child);return text;}
void Back(){
 if(routes[index]==EchoSphere.Presentation.NativeMainMenuView.Destination.Settings){var screen=Get("_nativeSettingsScreen");var buttons=(System.Collections.IDictionary)screen.GetType().GetField("_buttons",flags).GetValue(screen);Submit((UnityEngine.UIElements.Button)buttons["back"]);return;}
 var entries=(System.Collections.IDictionary)source.GetType().GetField("_entries",flags).GetValue(source);
 foreach(var entry in entries.Values){var node=(EchoSphere.Presentation.VisualCssNode)entry.GetType().GetField("Node").GetValue(entry);if(node.Tag!="button")continue;var text=Text(node).Trim();var aria=node.Properties["aria-label"].Text;var classes=node.Properties["className"].Text;if(text=="Назад"||text=="Back"||aria=="Назад"||aria=="Back"||classes.Contains("es-stage-select-back")){Submit((UnityEngine.UIElements.VisualElement)entry.GetType().GetField("Element").GetValue(entry));return;}}
 throw new System.Exception("Authored Back button not found on "+routes[index]);
}
UnityEditor.SessionState.SetString("EchoNativeMenuRouting","RUNNING");
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>600){Finish("FAIL Play ended/deadline at "+index+" phase "+phase);return;}
 if(UnityEngine.Time.frameCount-frame<3)return;frame=UnityEngine.Time.frameCount;
 try{
  if(phase==0){if(!runtime.UsesNativeMainMenu)return;Require(document==null||!document.enabled,"Legacy UIDocument still renders Home");var screen=(EchoSphere.Presentation.NativeMainMenuScreen)Get("_nativeMainMenuScreen");var names=new[]{"open-character","nav-1","nav-2","nav-3","nav-4","nav-5","open-map"};Submit(UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(screen.View.Root,names[index]));phase=1;return;}
  if(phase==1){Require(!runtime.UsesNativeMainMenu,"Native Home remained active on "+routes[index]);source=(EchoSphere.Presentation.SourceUiDocument)Get("_sourceUi");document=source==null?null:source.GetComponent<UnityEngine.UIElements.UIDocument>();Require(document!=null,"Source owner not created");if(routes[index]==EchoSphere.Presentation.NativeMainMenuView.Destination.Settings){Require((bool)Get("_nativeSettingsWasActive"),"Native Settings not active");Require(!document.enabled,"Settings retained legacy renderer");}else{Require(document.enabled&&document.rootVisualElement!=null,"Legacy root not recreated for "+routes[index]);Require(document.rootVisualElement.childCount>0,"Legacy root empty on "+routes[index]);}Back();phase=2;return;}
  if(!runtime.UsesNativeMainMenu)return;Require(!document.enabled,"Return Home failed after "+routes[index]);index++;UnityEditor.SessionState.SetString("EchoNativeMenuRouting","RUNNING accepted "+index+"/7 routes");if(index==routes.Length){Finish("PASS 7 actual native page buttons and authored Back controls; legacy UIDocument recreated/populated on 6 source pages; Settings uses its existing native owner; preferences unchanged");return;}phase=0;
 }catch(System.Exception e){Finish("FAIL "+e);}
};UnityEditor.EditorApplication.update+=tick;return "Started actual native/legacy page routing acceptance";
