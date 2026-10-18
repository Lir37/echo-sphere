// Bounded real Editor baseline. Starts only on request, warms actual player frames,
// captures the player main thread, pauses, and writes one JSON line outside Assets.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(runtime==null)throw new System.InvalidOperationException("Existing runtime required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var label=UnityEditor.SessionState.GetString("EchoVisualBaselineScenario","unlabelled");
var startFrame=UnityEngine.Time.frameCount;var deadline=UnityEditor.EditorApplication.timeSinceStartup+120;double end=0;
UnityEditor.SessionState.SetString("EchoVisualBaselineStatus","WARMUP "+label);
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 try{
  if(!UnityEditor.EditorApplication.isPlaying||runtime==null)throw new System.Exception("Runtime closed during capture");
  if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new System.Exception("Warmup/capture timeout");
  if(end==0){
   if(UnityEngine.Time.frameCount<startFrame+12)return;
   UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditorInternal.ProfilerDriver.ClearAllFrames();UnityEditorInternal.ProfilerDriver.profileEditor=false;UnityEditorInternal.ProfilerDriver.enabled=true;
   end=UnityEditor.EditorApplication.timeSinceStartup+3;UnityEditor.SessionState.SetString("EchoVisualBaselineStatus","RECORDING "+label);return;
  }
  if(UnityEditor.EditorApplication.timeSinceStartup<end)return;
  UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditor.EditorApplication.isPaused=true;
  int count=0;double ms=0,alloc=0;var totals=new System.Collections.Generic.Dictionary<string,double>();
  for(int frame=UnityEditorInternal.ProfilerDriver.firstFrameIndex+2;frame<=UnityEditorInternal.ProfilerDriver.lastFrameIndex-1;frame++)using(var view=UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(frame,0)){
   if(!view.valid)continue;count++;ms+=view.frameTimeMs;
   for(int i=0;i<view.sampleCount;i++){
    var name=view.GetSampleName(i);
    if(name=="GC.Alloc"&&view.GetSampleMetadataCount(i)>0)alloc+=view.GetSampleMetadataAsLong(i,0);
    if(name.StartsWith("EchoSphere.")||name=="PlayerLoop"||name=="WaitForTargetFPS"||name=="Gfx.WaitForPresentOnGfxThread"||name=="UIElements.UpdatePanels"||name=="UIElements.RenderPanels"){
     totals.TryGetValue(name,out var value);totals[name]=value+view.GetSampleTimeMs(i);
    }
   }
  }
  if(count==0)throw new System.Exception("No valid player frames; discard this capture");
  var markers=new System.Collections.Generic.Dictionary<string,double>();foreach(var p in totals)markers[p.Key]=p.Value/count;
  var result=new{scenario=label,utc=System.DateTime.UtcNow.ToString("o"),UnityEngine.Screen.width,UnityEngine.Screen.height,UnityEngine.Application.unityVersion,UnityEngine.SystemInfo.graphicsDeviceName,samples=count,frameMs=ms/count,fps=1000*count/ms,gcBytes=alloc/count,
   mainMenu=runtime.GetType().GetField("_mainMenu",flags).GetValue(runtime),page=runtime.GetType().GetField("_mainMenuPage",flags).GetValue(runtime).ToString(),regionSelect=runtime.GetType().GetField("_regionSelect",flags).GetValue(runtime),paused=runtime.GetType().GetField("_paused",flags).GetValue(runtime),gameOver=runtime.GetType().GetField("_gameOver",flags).GetValue(runtime),runTime=runtime.GetType().GetField("_runTime",flags).GetValue(runtime),
   renderers=UnityEngine.Object.FindObjectsByType<UnityEngine.Renderer>(UnityEngine.FindObjectsSortMode.None).Length,allocated=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),reserved=UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong(),markers,scope="Short Editor main-thread capture; inclusive markers, sampling/Editor overhead included; GPU timing unavailable"};
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(result);var path=System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath,"..","Temp","native-visual-baseline-2026-10-18.jsonl"));
  System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.AppendAllText(path,json+System.Environment.NewLine);
  UnityEditor.SessionState.SetString("EchoVisualBaselineStatus",json);
 }catch(System.Exception error){UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditor.EditorApplication.isPaused=true;UnityEditor.SessionState.SetString("EchoVisualBaselineStatus","FAIL "+error.Message);}
 UnityEditor.EditorApplication.update-=tick;
};
UnityEditor.EditorApplication.update+=tick;UnityEngine.Application.runInBackground=true;UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();UnityEditor.EditorApplication.isPaused=false;
return "Scheduled "+label+" after 12 actual warmup frames";
