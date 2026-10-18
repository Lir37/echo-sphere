if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var doc=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
if(doc==null||!(bool)doc.GetType().GetField("_presentationSuspended",flags).GetValue(doc))throw new System.Exception("Open native Settings first");
var root=doc.GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement;
if(root.enabledSelf||root.style.opacity.value!=0)throw new System.Exception("Hidden source still paints or accepts normal input");
var render=doc.Render;int renders=0,timers=0;
var timeout=doc.Environment.Get("setTimeout").Call(EchoSphere.Presentation.VisualJs.Fn(args=>{timers++;return default;}),new EchoSphere.Presentation.V(0));
doc.Render=()=>{renders++;return render();};doc.Dirty=true;
var first=UnityEngine.Time.frameCount;var deadline=UnityEditor.EditorApplication.timeSinceStartup+15;
UnityEditor.SessionState.SetString("EchoSettingsSuspensionCheck","RUNNING");
UnityEditor.EditorApplication.CallbackFunction check=null;
check=()=>{
 if(UnityEngine.Time.frameCount<first+3&&UnityEditor.EditorApplication.timeSinceStartup<deadline)return;
 UnityEditor.EditorApplication.update-=check;UnityEditor.EditorApplication.isPaused=true;
 try{
 if(renders!=0||timers!=1)throw new System.Exception("Hidden rendering or suspended hook timers: renders="+renders+", timers="+timers);
 if(doc.Environment.Get("innerWidth").Number!=UnityEngine.Screen.width||doc.Environment.Get("innerHeight").Number!=UnityEngine.Screen.height)throw new System.Exception("Hidden viewport stale");
 doc.SetPresentationSuspended(false);
 if(!root.enabledSelf||root.style.opacity.keyword!=UnityEngine.UIElements.StyleKeyword.Null)throw new System.Exception("Source root state was not restored");
 doc.Draw(1);if(renders!=1||doc.Dirty)throw new System.Exception("Resume did not rebuild the current source page");
 doc.SetPresentationSuspended(true);
 UnityEditor.SessionState.SetString("EchoSettingsSuspensionCheck","PASS: hidden tree skips rendering; timers and viewport live; resume restores root and rebuilds");
 }catch(System.Exception error){UnityEditor.SessionState.SetString("EchoSettingsSuspensionCheck","FAIL "+error.Message);}
 finally{doc.Render=render;doc.Environment.Get("clearTimeout").Call(timeout);doc.SetPresentationSuspended(true);}
};
UnityEditor.EditorApplication.update+=check;UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
return "Checking three actual player frames";
