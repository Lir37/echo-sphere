if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
var enabled=runtime.GetType().GetField("_nativeMainMenuEnabled",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
var previousEnabled=(bool)enabled.GetValue(runtime);enabled.SetValue(runtime,false);
var previousBackground=UnityEngine.Application.runInBackground;var previousPaused=UnityEditor.EditorApplication.isPaused;
UnityEditor.SessionState.SetBool("EchoNativeABPreviousBackground",previousBackground);UnityEditor.SessionState.SetBool("EchoNativeABPreviousPaused",previousPaused);
UnityEngine.Application.runInBackground=true;UnityEditor.EditorApplication.isPaused=false;
UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditorInternal.ProfilerDriver.profileEditor=false;
int stage=0,startFrame=UnityEngine.Time.frameCount;double start=UnityEditor.EditorApplication.timeSinceStartup;
var summaries=new System.Collections.Generic.List<object>();
UnityEditor.SessionState.SetString("EchoNativeMenuAB","WARM source");
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
 return new{name,count,frameMs=ms/count,fps=1000*count/ms,gcBytes=alloc/count,width=UnityEngine.Screen.width,height=UnityEngine.Screen.height,markers,native=runtime.UsesNativeMainMenu};
}
void Record(string name){UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditorInternal.ProfilerDriver.ClearAllFrames();UnityEditorInternal.ProfilerDriver.enabled=true;startFrame=UnityEngine.Time.frameCount;UnityEditor.SessionState.SetString("EchoNativeMenuAB","RECORD "+name);}
tick=()=>{
 try{
  if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>180)throw new System.Exception("Play Mode ended/deadline");
  if(stage==0&&UnityEngine.Time.frameCount-startFrame>=10){Record("source");stage=1;return;}
  if(stage==1&&UnityEngine.Time.frameCount-startFrame>=20){UnityEditorInternal.ProfilerDriver.enabled=false;summaries.Add(Summary("source"));enabled.SetValue(runtime,true);startFrame=UnityEngine.Time.frameCount;stage=2;UnityEditor.SessionState.SetString("EchoNativeMenuAB","WARM native");return;}
  if(stage==2&&runtime.UsesNativeMainMenu&&UnityEngine.Time.frameCount-startFrame>=12){Record("native");stage=3;return;}
  if(stage!=3||UnityEngine.Time.frameCount-startFrame<20)return;
  UnityEditorInternal.ProfilerDriver.enabled=false;summaries.Add(Summary("native"));UnityEditor.EditorApplication.update-=tick;enabled.SetValue(runtime,previousEnabled);UnityEditor.EditorApplication.isPaused=true;
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS paired Editor menu CPU timeline/GC capture",summaries},Newtonsoft.Json.Formatting.Indented);
  var path=System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-menu-profile-ab.json");System.IO.File.WriteAllText(path,json);UnityEditor.SessionState.SetString("EchoNativeMenuAB",json);
 }catch(System.Exception e){UnityEditorInternal.ProfilerDriver.enabled=false;UnityEditor.EditorApplication.update-=tick;enabled.SetValue(runtime,previousEnabled);UnityEngine.Application.runInBackground=previousBackground;UnityEditor.EditorApplication.isPaused=previousPaused;UnityEditor.SessionState.SetString("EchoNativeMenuAB","FAIL "+e);}
};UnityEditor.EditorApplication.update+=tick;return "Started paired menu Profiler capture; native candidate left paused for screenshot afterward";
