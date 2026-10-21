// Same saved run; A recreates prior periodic invalidation, B retains source UI,
// C uses the unified native screen. A/B/C/A/B/C, effects and game state unchanged.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
var r=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
var ui=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
bool Paused()=>(bool)r.GetType().GetField("_userPaused",flags).GetValue(r);
double RunTime()=>System.Convert.ToDouble(r.GetType().GetField("_runTime",flags).GetValue(r));
if(!Paused())throw new System.Exception("User pause required");
var frozen=RunTime();bool oldBackground=UnityEngine.Application.runInBackground;
bool oldScreen=(bool)r.GetType().GetField("_nativePauseScreenPilot",flags).GetValue(r);
bool oldHud=(bool)r.GetType().GetField("_nativeHudPilot",flags).GetValue(r);
bool oldRecording=UnityEditorInternal.ProfilerDriver.enabled;
UnityEngine.Application.runInBackground=true;UnityEditorInternal.ProfilerDriver.enabled=false;
var names=new[]{"GC Allocated In Frame","Main Thread","EchoSphere.UI.BuildTree","EchoSphere.UI.ResolveCss","EchoSphere.UI.Reconcile","EchoSphere.UI.PrepareBackdrops","EchoSphere.Canvas.EndFrame"};
var handles=new System.Collections.Generic.List<Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle>();
Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetAvailable(handles);
var recorders=new System.Collections.Generic.Dictionary<string,Unity.Profiling.ProfilerRecorder>();
var units=new System.Collections.Generic.Dictionary<string,string>();
foreach(var h in handles){var d=Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetDescription(h);if(!names.Contains(d.Name))continue;recorders[d.Name]=Unity.Profiling.ProfilerRecorder.StartNew(d.Category,d.Name,1);units[d.Name]=d.UnitType.ToString();}
if(recorders.Count!=names.Length){foreach(var p in recorders)p.Value.Dispose();throw new System.Exception("Missing counters");}
var samples=new System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<long>>();foreach(var n in names)samples[n]=new System.Collections.Generic.List<long>();
var rows=new System.Collections.Generic.List<object>();int phase=0,warm=0,count=0,last=-1;double phaseStart=0,nextInvalidation=0;
var start=UnityEditor.EditorApplication.timeSinceStartup;UnityEditor.EditorApplication.CallbackFunction tick=null;
void Begin(){r.SetNativePauseScreenPilot(phase%3==2);r.SetNativeHudPilot(true);r.GetType().GetField("_pausePlannerTab",flags).SetValue(r,0);ui.Dirty=true;warm=count=0;nextInvalidation=0;foreach(var a in samples.Values)a.Clear();UnityEditor.SessionState.SetString("EchoNativePauseScreenProfile","WARM "+phase);}
void Finish(string status){UnityEditor.EditorApplication.update-=tick;foreach(var p in recorders)p.Value.Dispose();r.SetNativePauseScreenPilot(oldScreen);r.SetNativeHudPilot(oldHud);UnityEngine.Application.runInBackground=oldBackground;UnityEditorInternal.ProfilerDriver.enabled=oldRecording;var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status,width=UnityEngine.Screen.width,height=UnityEngine.Screen.height,scope="Editor wall frame times and ProfilerRecorder; A explicitly invalidates stable source UI every100ms to recreate previous cadence, B source event invalidation, C unified native screen event invalidation. HUD held native; original world, shell effects and button blur retained. No device/GPU/pixel parity claim. Warm10/sample20 frames each phase. Existing Timeline capture not cleared.",rows});UnityEditor.SessionState.SetString("EchoNativePauseScreenProfile",json);System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-pause-screen-profile.json"),json);}
Begin();tick=()=>{try{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>300)throw new System.Exception("Deadline/Play ended");
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;
 if(!Paused()||System.Math.Abs(RunTime()-frozen)>.000001)throw new System.Exception("Simulation changed");
 double now=UnityEditor.EditorApplication.timeSinceStartup;
 if(phase%3==0&&now>=nextInvalidation){ui.Dirty=true;nextInvalidation=now+.1;}
 if(phase%3==2&&!r.UsesNativePauseScreen)return;
 if(++warm<=10){phaseStart=now;return;}
 foreach(var p in recorders)samples[p.Key].Add(p.Value.LastValue);
 if(++count<20)return;
 var stats=new System.Collections.Generic.Dictionary<string,object>();foreach(var p in samples){var avg=p.Value.Average(v=>(double)v);stats[p.Key]=new{mean=units[p.Key]=="TimeNanoseconds"?avg/1000000:avg,unit=units[p.Key]=="TimeNanoseconds"?"ms":units[p.Key]};}
 var elapsed=now-phaseStart;rows.Add(new{phase,variant=new[]{"periodic source","retained source","unified native"}[phase%3],count,frameMs=elapsed*1000/count,fps=count/elapsed,counters=stats});
 if(++phase==6){Finish("PASS paused A/B/C/A/B/C");return;}Begin();
}catch(System.Exception e){Finish("FAIL "+e);}};UnityEditor.EditorApplication.update+=tick;return "Started unified pause paired profile";
