if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
if(UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.NativeBackdropPanelProbe>()!=null)throw new System.Exception("Probe already running");
var owner=new UnityEngine.GameObject("Native panel backdrop architecture probe");
var probe=owner.AddComponent<EchoSphere.Presentation.NativeBackdropPanelProbe>();probe.Initialize();
int first=UnityEngine.Time.frameCount,last=first;
var deadline=UnityEditor.EditorApplication.timeSinceStartup+120;
UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","RUNNING");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(owner==null){UnityEditor.EditorApplication.update-=tick;return;}
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline){UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","FAIL runtime closed or 120-second deadline; no parity acceptance");UnityEditor.EditorApplication.update-=tick;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;return;}
 if(UnityEngine.Time.frameCount==last)return;last=UnityEngine.Time.frameCount;
 if(last<first+12)return;
 try{
  var sample=probe.Compare();
  if(probe.SampleCount<12)return;
  var result=new{status=probe.MaximumMeanError<=1&&probe.ClickVerified?"PASS limited composition fixture":"FAIL pixel parity; production remains unchanged",samples=probe.SampleCount,probe.MaximumMeanError,probe.MaximumChannelError,probe.DifferentPixels,probe.ReferenceCopies,probe.CandidateReplicaCount,probe.TargetBytes,probe.ClickVerified,last=sample};
  UnityEditor.SessionState.SetString("EchoNativeBackdropProbe",Newtonsoft.Json.JsonConvert.SerializeObject(result));
  UnityEditor.EditorApplication.update-=tick;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;
 }catch(System.Exception e){UnityEditor.SessionState.SetString("EchoNativeBackdropProbe","FAIL "+e);UnityEditor.EditorApplication.update-=tick;probe.Animate=false;UnityEditor.EditorApplication.isPaused=true;}
};
UnityEditor.EditorApplication.update+=tick;UnityEditor.EditorApplication.isPaused=false;return "Started 12-frame native panel pixel comparison after warmup";
