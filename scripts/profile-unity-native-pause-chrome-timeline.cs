// Bounded CPU Timeline capture on a genuinely paused saved scene. No ClearAllFrames.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
if(UnityEditorInternal.ProfilerDriver.enabled)throw new System.Exception("Existing live Profiler recording must be kept intact");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
bool Paused()=>(bool)runtime.GetType().GetField("_userPaused",flags).GetValue(runtime);
double RunTime()=>System.Convert.ToDouble(runtime.GetType().GetField("_runTime",flags).GetValue(runtime));
if(!Paused()||!runtime.UsesNativeHud||!runtime.UsesNativePauseStats)throw new System.Exception("Paused scene with held native HUD/stats required");
var frozen=RunTime();var original=(bool)runtime.GetType().GetField("_nativePauseChromePilot",flags).GetValue(runtime);
var oldCpu=UnityEngine.Profiling.Profiler.GetAreaEnabled(UnityEngine.Profiling.ProfilerArea.CPU);
var oldBackground=UnityEngine.Application.runInBackground;
var rows=new System.Collections.Generic.List<object>();
int phase=0,warm=0,recorded=0,last=-1;bool recording=false;double beforeNs=-1;
var start=UnityEditor.EditorApplication.timeSinceStartup;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string status){
 UnityEditor.EditorApplication.update-=tick;UnityEditorInternal.ProfilerDriver.enabled=false;
 UnityEngine.Profiling.Profiler.SetAreaEnabled(UnityEngine.Profiling.ProfilerArea.CPU,oldCpu);
 runtime.SetNativePauseChromePilot(original);UnityEngine.Application.runInBackground=oldBackground;
 var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status,rows,limits="Four recorded CPU frames per variant after warmup; inclusive per-marker times, never add nested markers. Existing frames not explicitly cleared. Editor Timeline, not GPU/Android/FPS acceptance"});
 UnityEditor.SessionState.SetString("EchoNativePauseChromeTimeline",json);
 System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-pause-chrome-timeline.json"),json);
}
runtime.SetNativePauseChromePilot(false);UnityEngine.Application.runInBackground=true;
UnityEngine.Profiling.Profiler.SetAreaEnabled(UnityEngine.Profiling.ProfilerArea.CPU,true);
UnityEditor.SessionState.SetString("EchoNativePauseChromeTimeline","RUNNING");
tick=()=>{try{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>180)throw new System.Exception("Play/deadline ended");
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;
 if(!Paused()||System.Math.Abs(RunTime()-frozen)>.000001)throw new System.Exception("Paused simulation advanced");
 if(!recording){
  if(runtime.UsesNativePauseChrome!=(phase==1)||++warm<8)return;
  int previous=UnityEditorInternal.ProfilerDriver.lastFrameIndex;
  if(previous>=0){using(var prior=UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(previous,0)){if(prior.valid)beforeNs=prior.frameStartTimeNs;}}
  recorded=0;recording=true;UnityEditorInternal.ProfilerDriver.enabled=true;return;
 }
 if(++recorded<4)return;
 UnityEditorInternal.ProfilerDriver.enabled=false;
 int end=UnityEditorInternal.ProfilerDriver.lastFrameIndex;
 int first=System.Math.Max(UnityEditorInternal.ProfilerDriver.firstFrameIndex,end-3);
 int captured=0;
 for(int frame=first;frame<=end;frame++){
  using(var data=UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(frame,0)){
   if(!data.valid||data.frameStartTimeNs<=beforeNs)continue;
   var samples=new System.Collections.Generic.List<(string Name,double Ms)>();
   for(int i=0;i<data.sampleCount;i++){var name=data.GetSampleName(i);if(name.StartsWith("EchoSphere.UI.")||name.StartsWith("EchoSphere.Canvas.")||name.StartsWith("UIElements."))samples.Add((name,data.GetSampleTimeMs(i)));}
   var markers=samples.GroupBy(s=>s.Name).Select(g=>new{name=g.Key,calls=g.Count(),inclusiveMs=g.Sum(s=>s.Ms),maximumMs=g.Max(s=>s.Ms)}).ToArray();
   rows.Add(new{native=phase==1,frame,thread=data.threadName,data.frameTimeMs,data.sampleCount,markers});captured++;
  }
 }
 if(captured==0)throw new System.Exception("No fresh CPU Timeline frame; no stale-data acceptance");
 if(++phase==2){Finish("PASS fresh CPU Timeline for source/native pause chrome");return;}
 runtime.SetNativePauseChromePilot(true);recording=false;warm=0;beforeNs=-1;
}catch(System.Exception e){Finish("FAIL "+e);}};
UnityEditor.EditorApplication.update+=tick;return "Started bounded source/native CPU Timeline; simulation frozen";
