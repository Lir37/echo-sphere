// Actual UI Toolkit controls and Yoga layout. No PlayerPrefs or game actions.
// Composition/artwork acceptance is deliberately outside this control contract.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMainMenu.uxml",UnityEditor.ImportAssetOptions.ForceUpdate);
UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMainMenu.uss",UnityEditor.ImportAssetOptions.ForceUpdate);
var owner=new UnityEngine.GameObject("Native main menu control acceptance");
var panel=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
panel.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.sortingOrder=2200;
panel.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=panel;
var parent=new UnityEngine.UIElements.VisualElement();parent.style.position=UnityEngine.UIElements.Position.Absolute;parent.style.flexShrink=0;parent.style.width=1260;parent.style.height=542;document.rootVisualElement.Add(parent);
var visits=new System.Collections.Generic.List<EchoSphere.Presentation.NativeMainMenuView.Destination>();
var choices=new System.Collections.Generic.List<string>();var names=new System.Collections.Generic.List<string>();
var view=new EchoSphere.Presentation.NativeMainMenuView(parent,value=>visits.Add(value),value=>names.Add(value),value=>choices.Add(value));
var state=new EchoSphere.Presentation.NativeMainMenuView.State(true,"Тест","Сферист","Универсал","normal","Сложность","Имя","Магазин","Рекорды","Достижения","Настройки",85);
view.Show(state);
int phase=0,frame=UnityEngine.Time.frameCount;var start=UnityEditor.EditorApplication.timeSinceStartup;
var wasPaused=UnityEditor.EditorApplication.isPaused;var wasBackground=UnityEngine.Application.runInBackground;
UnityEngine.Application.runInBackground=true;
UnityEditor.SessionState.SetString("EchoNativeMainMenuControls","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Require(bool value,string message){if(!value)throw new System.Exception(message);}
UnityEngine.UIElements.Button Button(string name)=>UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(view.Root,name);
void Submit(string name){var button=Button(name);using(var evt=UnityEngine.UIElements.NavigationSubmitEvent.GetPooled()){evt.target=button;button.SendEvent(evt);}}
void Finish(string result){
 UnityEditor.EditorApplication.update-=tick;view.Dispose();
 UnityEngine.Object.Destroy(owner);UnityEngine.Object.Destroy(panel);
 UnityEngine.Application.runInBackground=wasBackground;UnityEditor.EditorApplication.isPaused=wasPaused;
 UnityEditor.SessionState.SetString("EchoNativeMainMenuControls",result);
}
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>45){Finish("FAIL Play Mode ended/deadline");return;}
 if(UnityEngine.Time.frameCount-frame<6)return;frame=UnityEngine.Time.frameCount;
 try{
  if(phase==0){
   Require(view.Root.resolvedStyle.display==UnityEngine.UIElements.DisplayStyle.Flex,"Bound view hidden");
   Require(Button("nav-0").worldBound.y==Button("nav-5").worldBound.y,"Landscape nav must use one row: "+Button("nav-0").worldBound+" / "+Button("nav-5").worldBound+" parent="+parent.contentRect+" root="+view.Root.contentRect+" narrow="+view.Root.ClassListContains("narrow"));
   Require(UnityEngine.Mathf.Abs(view.Artwork.layout.width-155)<.1f,"Short viewport source art size: "+view.Artwork.layout+" parent="+parent.contentRect+" inline="+view.Artwork.style.width+" short="+view.Root.ClassListContains("short"));
   var original=view.Root;for(int i=0;i<120;i++)view.Show(state);Require(object.ReferenceEquals(original,view.Root),"Repeated state replaced native tree");
   Require(names.Count==0&&choices.Count==0&&visits.Count==0,"Model binding fired user actions");
   Submit("open-map");Submit("open-character");for(int i=0;i<6;i++)Submit("nav-"+i);Submit("continue-run");
   var expected=new[]{0,1,1,2,3,4,5,6,7};Require(visits.Count==expected.Length,"Navigation callback count");
   for(int i=0;i<expected.Length;i++)Require((int)visits[i]==expected[i],"Navigation destination/order "+i);
   foreach(var id in new[]{"easy","normal","hard","nightmare"})Submit("difficulty-"+id);
   Require(string.Join(",",choices)=="easy,normal,hard,nightmare","Difficulty source IDs/order");
   var input=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.TextField>(view.Root,"player-name");
   input.value="12345678901234567890";Require(input.value.Length==16&&names.Count==1&&names[0]=="1234567890123456","Name limit/native edit event");
   var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();Require(runtime!=null,"Existing runtime missing");
   var real=runtime.ReadNativeMainMenuState();Require(!string.IsNullOrEmpty(real.CharacterName)&&!string.IsNullOrEmpty(real.SettingsTitle),"Real runtime model unbound");
   parent.style.width=480;parent.style.height=854;phase=1;return;
  }
  if(phase==1){
   Require(view.Root.ClassListContains("narrow")&&!view.Root.ClassListContains("short"),"Portrait breakpoint");
   Require(UnityEngine.Mathf.Abs(view.Artwork.layout.width-210)<.1f,"Portrait source art size");
   Require(Button("nav-0").worldBound.y==Button("nav-2").worldBound.y&&Button("nav-3").worldBound.y>Button("nav-0").worldBound.y,"Portrait three-column nav");
   Require(Button("open-character").worldBound.width<=420,"Loadout exceeds source max width");
   var english=new EchoSphere.Presentation.NativeMainMenuView.State(false,"Player","Spherist","All-rounder","hard","Difficulty","Name","Shop","Leaderboard","Achievements","Settings",null);
   view.Show(english);Require(Button("continue-run").resolvedStyle.display==UnityEngine.UIElements.DisplayStyle.None||Button("continue-run").style.display.value==UnityEngine.UIElements.DisplayStyle.None,"Missing save still shown");
   Require(Button("difficulty-hard").ClassListContains("is-active")&&!Button("difficulty-normal").ClassListContains("is-active"),"Selection stale after model update");
   Require(UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Label>(view.Root,"nav-label-5").text=="SETTINGS","Language update stale");
   view.Hide();Require(view.Root.style.display.value==UnityEngine.UIElements.DisplayStyle.None,"Hide failed");view.Show(english);
   parent.style.width=1260;parent.style.height=542;phase=2;return;
  }
  Require(!view.Root.ClassListContains("narrow")&&view.Root.ClassListContains("short"),"Landscape restore breakpoint");
  Require(Button("nav-0").worldBound.y==Button("nav-5").worldBound.y,"Landscape restore nav");
  Require(names.Count==1&&choices.Count==4&&visits.Count==9,"Refresh emitted extra user actions");
  var root=view.Root;view.Dispose();view.Dispose();Require(root.parent==null,"Dispose retained native menu");
  Finish("PASS native controls/layout: 9 navigation events, 4 difficulties, 16-character name, silent model refresh, RU/EN, save presence, retained tree, 1260x542 → 480x854 → 1260x542, idempotent cleanup. Artwork/blur/visual parity NOT accepted.");
 }catch(System.Exception e){Finish("FAIL "+e);}
};
UnityEditor.EditorApplication.update+=tick;UnityEditor.EditorApplication.isPaused=false;
return "Started native main menu real-frame control/layout acceptance";
