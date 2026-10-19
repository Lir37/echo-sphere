// Saved run with game simulation paused: compares contour modes, not native gameplay migration.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>(); var canvas=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceCanvasSurface>();
var stateFlags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
if(runtime==null||canvas==null||(bool)runtime.GetType().GetField("_mainMenu",stateFlags).GetValue(runtime)||!(bool)runtime.GetType().GetField("_userPaused",stateFlags).GetValue(runtime))throw new System.Exception("Existing saved run with user pause required");
var enabled=canvas.GetType().GetField("_retainLocalContours",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
var previousEnabled=(bool)enabled.GetValue(canvas);enabled.SetValue(canvas,false);
var previousBackground=UnityEngine.Application.runInBackground;var previousPaused=UnityEditor.EditorApplication.isPaused;
UnityEditor.SessionState.SetBool("EchoCanvasLocalABPreviousBackground",previousBackground);UnityEditor.SessionState.SetBool("EchoCanvasLocalABPreviousPaused",previousPaused);
UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditorInternal.ProfilerDriver.profileEditor=false;
int stage=0,startFrame=UnityEngine.Time.frameCount;double start=UnityEditor.EditorApplication.timeSinceStartup;
var summaries=new System.Collections.Generic.List<object>();
UnityEditor.SessionState.SetString("EchoCanvasLocalAB","WARM source");
UnityEditor.EditorApplication.CallbackFunction tick=null;
object Summary(string name){
 int first=UnityEditorInternal.ProfilerDriver.firstFrameIndex+2,last=UnityEditorInternal.ProfilerDriver.lastFrameIndex-1,count=0;double ms=0,alloc=0;
 var totals=new System.Collections.Generic.Dictionary<string,double>();
 for(int f=first;f<=last;f++)using(var v=UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(f,0)){
  if(!v.valid)continue;count++;ms+=v.frameTimeMs;
  for(int i=0;i<v.sampleCount;i++){
   var sample=v.GetSampleName(i);if(sample=="GC.Alloc"&&v.GetSampleMetadataCount(i)>0)alloc+=v.GetSampleMetadataAsLong(i,0);
   if(sample.StartsWith("EchoSphere.")||sample=="UIElements.UpdatePanels"||sample=="UIElements.RenderPanels"||sample.StartsWith("UIR.")){totals.TryGetValue(sample,out var value);totals[sample]=value+v.GetSampleTimeMs(i);}
  }
 }
 if(count<10)throw new System.Exception("Too few profiler frames: "+count);
 var markers=new System.Collections.Generic.Dictionary<string,double>();foreach(var pair in totals)markers[pair.Key]=pair.Value/count;
 return new{name,count,frameMs=ms/count,fps=1000*count/ms,gcBytes=alloc/count,width=UnityEngine.Screen.width,height=UnityEngine.Screen.height,markers,retainedLocalContours=(bool)enabled.GetValue(canvas)};
}
void Record(string name){UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditorInternal.ProfilerDriver.ClearAllFrames();UnityEditorInternal.ProfilerDriver.enabled=true;startFrame=UnityEngine.Time.frameCount;UnityEditor.SessionState.SetString("EchoCanvasLocalAB","RECORD "+name);}
tick=()=>{
 try{
  if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>180)throw new System.Exception("Play Mode ended/deadline");
  if(stage==0&&UnityEngine.Time.frameCount-startFrame>=10){Record("source");stage=1;return;}
  if(stage==1&&UnityEngine.Time.frameCount-startFrame>=20){UnityEditorInternal.ProfilerDriver.enabled=false;summaries.Add(Summary("world roundtrip"));enabled.SetValue(canvas,true);startFrame=UnityEngine.Time.frameCount;stage=2;UnityEditor.SessionState.SetString("EchoCanvasLocalAB","WARM native");return;}
  if(stage==2&&(bool)enabled.GetValue(canvas)&&UnityEngine.Time.frameCount-startFrame>=12){Record("native");stage=3;return;}
  if(stage!=3||UnityEngine.Time.frameCount-startFrame<20)return;
  UnityEditorInternal.ProfilerDriver.enabled=false;summaries.Add(Summary("retained local"));UnityEditor.EditorApplication.update-=tick;enabled.SetValue(canvas,previousEnabled);UnityEditor.EditorApplication.isPaused=true;
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS paired Editor paused-simulation Canvas CPU timeline/GC capture",summaries},Newtonsoft.Json.Formatting.Indented);
  var path=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/canvas-local-profile-ab.json");System.IO.File.WriteAllText(path,json);UnityEditor.SessionState.SetString("EchoCanvasLocalAB",json);
 }catch(System.Exception e){UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditor.EditorApplication.update-=tick;enabled.SetValue(canvas,previousEnabled);UnityEngine.Application.runInBackground=previousBackground;UnityEditor.EditorApplication.isPaused=previousPaused;UnityEditor.SessionState.SetString("EchoCanvasLocalAB","FAIL "+e);}
};UnityEditor.EditorApplication.update+=tick;return "Started saved-run paused-simulation Canvas A/B capture";


