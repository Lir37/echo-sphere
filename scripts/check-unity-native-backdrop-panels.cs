if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
if(UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.NativeBackdropPanelProbe>()!=null)throw new System.Exception("Probe already running");
var owner=new UnityEngine.GameObject("Native panel backdrop architecture probe");
var backgroundOrder=UnityEditor.SessionState.GetInt("EchoNativeBackdropBackgroundOrder",2101);
var externalPrefix=UnityEditor.SessionState.GetBool("EchoNativeBackdropExternalPrefix",false);
var earlyPrefix=UnityEditor.SessionState.GetBool("EchoNativeBackdropEarlyPrefix",false);
var probe=owner.AddComponent<EchoSphere.Presentation.NativeBackdropPanelProbe>();probe.Initialize(backgroundOrder,externalPrefix,earlyPrefix);
var phaseFrames=UnityEngine.Mathf.Max(1,UnityEditor.SessionState.GetInt("EchoNativeBackdropPhaseFrames",4));probe.PhaseFrames=phaseFrames;
int first=UnityEngine.Time.frameCount,last=first;
var comparisons=new System.Collections.Generic.List<string>();
var deadline=UnityEditor.EditorApplication.timeSinceStartup+120;
var wasBackground=UnityEngine.Application.runInBackground;
UnityEngine.Application.runInBackground=true;
UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(owner==null){UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.runInBackground=wasBackground;UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","FAIL probe destroyed during comparison");return;}
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline){UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","FAIL runtime closed or 120-second deadline; no parity acceptance");UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.runInBackground=wasBackground;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;return;}
 if(UnityEngine.Time.frameCount==last)return;last=UnityEngine.Time.frameCount;
 if(last<first+12)return;
 try{
  var sample=probe.Compare();
  comparisons.Add(sample);
  if(probe.SampleCount<32)return;
  var result=new{status=probe.MaximumMeanError<=.01&&probe.MaximumChannelError<=1&&probe.ClickVerified?"PASS limited composition fixture":"FAIL pixel parity; production remains unchanged",backgroundOrder,externalPrefix,earlyPrefix,phaseFrames,samples=probe.SampleCount,probe.MaximumMeanError,probe.MaximumChannelError,probe.DifferentPixels,probe.ReferenceCopies,probe.CandidateReplicaCount,probe.TargetBytes,probe.ClickVerified,last=sample,comparisons};
  UnityEditor.SessionState.SetString("EchoNativeBackdropProbe",Newtonsoft.Json.JsonConvert.SerializeObject(result));
  UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.runInBackground=wasBackground;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;
 }catch(System.Exception e){UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","FAIL "+e);UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.runInBackground=wasBackground;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;}
};
UnityEditor.EditorApplication.update+=tick;UnityEditor.EditorApplication.isPaused=false;return "Started 32-frame native panel pixel comparison after warmup";
