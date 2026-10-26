// Actual UI Toolkit layout at phone density, portrait and landscape sizes.
// Uses owned offscreen panels and real localized tutorial models; no game/save actions.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("EchoGameplayPrefsBackup","")))throw new System.Exception("Back up diagnostic preferences before Play; restore after Stop");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(runtime.GetType().GetField("_playerCore",flags).GetValue(runtime)==null)throw new System.Exception("Wait for runtime Start");
if(!(bool)runtime.GetType().GetField("_mainMenu",flags).GetValue(runtime))throw new System.Exception("Start from native main menu");
bool enabled=runtime.enabled,background=UnityEngine.Application.runInBackground;int size=0,step=0,frames=0,last=-1,checks=0,errors=0;
var sizes=new[]{(width:1080,height:2400,density:2.75f),(width:480,height:800,density:1f),(width:1260,height:542,density:1f)};
UnityEngine.GameObject owner=null;UnityEngine.UIElements.PanelSettings panel=null;UnityEngine.RenderTexture texture=null;
EchoSphere.Presentation.NativeTutorialView view=null;System.IDisposable registryOwner=null;
float width=0,height=0,density=1;UnityEngine.Rect? target=null;
var observations=new System.Collections.Generic.List<object>();double deadline=UnityEditor.EditorApplication.timeSinceStartup+180;
UnityEngine.Application.LogCallback onError=(message,trace,kind)=>{if(kind==UnityEngine.LogType.Error||kind==UnityEngine.LogType.Exception||kind==UnityEngine.LogType.Assert)errors++;};
void Release(){view?.Dispose();view=null;registryOwner?.Dispose();registryOwner=null;if(owner!=null){var document=owner.GetComponent<UnityEngine.UIElements.UIDocument>();if(document!=null)document.enabled=false;}if(panel!=null)panel.targetTexture=null;if(owner!=null)UnityEngine.Object.Destroy(owner);if(texture!=null){texture.Release();UnityEngine.Object.Destroy(texture);}if(panel!=null)UnityEngine.Object.Destroy(panel);owner=null;texture=null;panel=null;}
void Configure(){
 Release();var config=sizes[size];density=config.density;width=config.width/density;height=config.height/density;
 owner=new UnityEngine.GameObject("Owned tutorial viewport check");panel=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();panel.name="Tutorial viewport check";panel.scaleMode=UnityEngine.UIElements.PanelScaleMode.ConstantPixelSize;panel.scale=density;panel.forceGammaRendering=true;panel.themeStyleSheet=UnityEngine.Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("EchoSphere/SourceUiTheme");
 texture=new UnityEngine.RenderTexture(config.width,config.height,0,UnityEngine.RenderTextureFormat.ARGB32);texture.Create();panel.targetTexture=texture;
 var document=owner.AddComponent<UnityEngine.UIElements.UIDocument>();document.panelSettings=panel;document.rootVisualElement.style.width=width;document.rootVisualElement.style.height=height;
 var registryType=typeof(EchoSphere.Presentation.NativeTutorialView).Assembly.GetType("EchoSphere.Presentation.NativeUiBackdropRegistry");var registry=(EchoSphere.Presentation.INativeUiBackdrops)System.Activator.CreateInstance(registryType,true);registryOwner=(System.IDisposable)registry;
 view=new EchoSphere.Presentation.NativeTutorialView(document.rootVisualElement,registry,()=>{},()=>{});
 var stepField=runtime.GetType().GetField("_tutorialStep",flags);var oldStep=stepField.GetValue(runtime);
 try{stepField.SetValue(runtime,step);view.Bind((EchoSphere.Presentation.NativeTutorialView.State)runtime.GetType().GetMethod("ReadNativeTutorialState",flags).Invoke(runtime,null));}finally{stepField.SetValue(runtime,oldStep);}
 target=step==1?new UnityEngine.Rect(width*.75f-58,height-160,116,116):step==2?new UnityEngine.Rect(0,32,width,height-64):step==3?new UnityEngine.Rect(8,24,width*.5f-16,120):step==4?new UnityEngine.Rect(0,0,width,height):(UnityEngine.Rect?)null;
}
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){
 UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.logMessageReceived-=onError;
 try{System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/native-tutorial-viewport.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,errors,observations},Newtonsoft.Json.Formatting.Indented));}
 finally{Release();runtime.enabled=enabled;UnityEngine.Application.runInBackground=background;UnityEditor.SessionState.SetString("EchoTutorialViewport",result);}
}
runtime.enabled=false;UnityEngine.Application.runInBackground=true;UnityEngine.Application.logMessageReceived+=onError;
try{Configure();}catch{Finish("FAIL configure");throw;}
tick=()=>{try{
 UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new System.Exception("Play/deadline");
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;view.Prepare(width,height,density,target);if(++frames<10)return;
 var card=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(view.Root,"tutorial-card");var next=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(view.Root,"tutorial-next");var bounds=card.worldBound;float tolerance=1/density+.01f;
 if(!view.Ready||!float.IsFinite(bounds.height)||bounds.height<=0||bounds.xMin<12-tolerance||bounds.xMax>width-12+tolerance||bounds.yMin<12-tolerance||bounds.yMax>height-12+tolerance)throw new System.Exception("Tutorial card clipped at size="+size+" step="+step+": "+bounds);
 if(step!=4&&(next.worldBound.yMin<0||next.worldBound.yMax>height))throw new System.Exception("Next is outside viewport");checks++;
 if(frames<14)return;
 if(size==0&&step==2){UnityEngine.Texture2D read=null;var previous=UnityEngine.RenderTexture.active;try{UnityEngine.RenderTexture.active=texture;read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false);read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/native-tutorial-phone-fixed.png"),UnityEngine.ImageConversion.EncodeToPNG(read));}finally{UnityEngine.RenderTexture.active=previous;if(read!=null)UnityEngine.Object.Destroy(read);}}
 if(step==1){view.Prepare(width,height+24,density,target);var shade=(UnityEngine.RenderTexture)typeof(EchoSphere.Presentation.NativeTutorialView).GetField("_shadeTarget",flags).GetValue(view);if(shade.height!=UnityEngine.Mathf.CeilToInt((height+24)*density))throw new System.Exception("Spotlight did not resize");view.Prepare(width,height,density,target);shade=(UnityEngine.RenderTexture)typeof(EchoSphere.Presentation.NativeTutorialView).GetField("_shadeTarget",flags).GetValue(view);if(shade.height!=UnityEngine.Mathf.CeilToInt(height*density))throw new System.Exception("Spotlight did not restore size");checks++;}
 if(errors!=0)throw new System.Exception("New Console errors: "+errors);
 observations.Add(new{size,step,width,height,density,card=new{bounds.x,bounds.y,bounds.width,bounds.height}});
 if(++step==6){step=0;size++;}if(size==sizes.Length){Finish("PASS all 18 tutorial viewport cases");return;}frames=0;Configure();
}catch(System.Exception ex){Finish("FAIL "+ex);}};
UnityEditor.EditorApplication.update+=tick;UnityEditor.SessionState.SetString("EchoTutorialViewport","RUNNING");return "Started 18 actual tutorial layout cases";
