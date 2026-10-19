// Native whole-menu candidate. No production selection or gameplay callbacks.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/EchoSphere/NativeMainMenu.uss",UnityEditor.ImportAssetOptions.ForceUpdate);
var owner=new UnityEngine.GameObject("Native menu composition acceptance");
var panel=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
panel.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.sortingOrder=2200;panel.forceGammaRendering=true;
panel.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=panel;
var parent=document.rootVisualElement;parent.style.position=UnityEngine.UIElements.Position.Absolute;parent.style.left=parent.style.right=parent.style.top=parent.style.bottom=0;
var calls=new System.Collections.Generic.List<string>();
var view=new EchoSphere.Presentation.NativeMainMenuView(parent,v=>calls.Add(v.ToString()),v=>calls.Add("name:"+v),v=>calls.Add("difficulty:"+v));
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();view.Show(runtime.ReadNativeMainMenuState());
var composition=new EchoSphere.Presentation.NativeMainMenuComposition(view);
var wasPaused=UnityEditor.EditorApplication.isPaused;var background=UnityEngine.Application.runInBackground;
UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
UnityEditor.SessionState.SetString("EchoNativeMenuComposition","RUNNING");
var start=UnityEditor.EditorApplication.timeSinceStartup;var frame=UnityEngine.Time.frameCount;int settled=0,lastPaint=-1;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){UnityEditor.EditorApplication.update-=tick;composition.Dispose();view.Dispose();UnityEngine.Object.Destroy(owner);UnityEngine.Object.Destroy(panel);UnityEngine.Application.runInBackground=background;UnityEditor.EditorApplication.isPaused=wasPaused;UnityEditor.SessionState.SetString("EchoNativeMenuComposition",result);}
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>120){Finish("FAIL Play Mode ended/deadline");return;}
 if(UnityEngine.Time.frameCount==frame)return;frame=UnityEngine.Time.frameCount;
 try{
  if(!composition.Prepare(1,new UnityEngine.Color(85f/255,223f/255,1)))return;
  if(lastPaint==composition.PrepareCount)settled++;else settled=0;lastPaint=composition.PrepareCount;
  if(settled<8)return;
  var directory=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-menu-composition");System.IO.Directory.CreateDirectory(directory);
  var boxes=new System.Collections.Generic.List<object>();foreach(var name in new[]{"menu-logo","core-caption","core-artwork","core-title","character-avatar","difficulty-normal","continue-run","nav-0","nav-5"}){var e=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(view.Root,name);boxes.Add(new{name,layout=e.layout.ToString(),world=e.worldBound.ToString(),font=e.resolvedStyle.fontSize});}
  System.IO.File.WriteAllText(System.IO.Path.Combine(directory,"layout.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{width=UnityEngine.Screen.width,height=UnityEngine.Screen.height,prepares=composition.PrepareCount,boxes},Newtonsoft.Json.Formatting.Indented));
  if(calls.Count!=0)throw new System.Exception("Binding fired game action");
  // Leave candidate visible for Game View capture; explicit cleanup eval follows.
  UnityEditor.EditorApplication.update-=tick;UnityEditor.EditorApplication.isPaused=true;
  UnityEditor.SessionState.SetString("EchoNativeMenuComposition","PASS bounded composition settled; owner retained for screenshot; full parity pending");
  // Keep the exact ownership closure reachable until the screenshot is taken.
  parent.userData=(System.Action)(()=>Finish("PASS bounded composition/cache/no actions and cleanup; full parity pending"));
 }catch(System.Exception e){Finish("FAIL "+e);}
};
UnityEditor.EditorApplication.update+=tick;return "Started native menu composition acceptance";
