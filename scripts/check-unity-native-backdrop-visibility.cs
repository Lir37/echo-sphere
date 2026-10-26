// Run in Play Mode after the native runtime starts. No gameplay actions/save commands.
// Counts actual Unity filter callbacks, including a visible control for every hidden case.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("EchoGameplayPrefsBackup","")))throw new System.Exception("Back up diagnostic preferences before Play; restore after Stop");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();
if(type.GetField("_playerCore",flags).GetValue(runtime)==null)throw new System.Exception("Wait for runtime Start");
string[] names={"_mainMenu","_gameOver","_userPaused","_paused","_levelUp","_stellaRewardPending","_tutorialStep","_runSaveAccumulator"};
var saved=names.Select(name=>type.GetField(name,flags).GetValue(runtime)).ToArray();
if(!(bool)saved[0])throw new System.Exception("Start from native main menu");
var rewards=(System.Collections.Generic.List<EchoSphere.Core.ArtifactRewardDefinition>)type.GetField("_stellaRewardChoices",flags).GetValue(runtime);var savedRewards=rewards.ToArray();
bool oldEnabled=runtime.enabled,oldBackground=UnityEngine.Application.runInBackground;
int stage=0,frames=0,last=-1,errors=0,checks=0;string[] stages={"tutorial","pause","tutorial-return","rewards","main-menu","run-return"};
double deadline=UnityEditor.EditorApplication.timeSinceStartup+180;
var counts=new System.Collections.Generic.Dictionary<string,int>();var observations=new System.Collections.Generic.List<object>();
var wrapped=new System.Collections.Generic.HashSet<UnityEngine.UIElements.FilterFunctionDefinition>();var restore=new System.Collections.Generic.List<System.Action>();
UnityEngine.Application.LogCallback onError=(message,trace,kind)=>{if(kind==UnityEngine.LogType.Error||kind==UnityEngine.LogType.Exception||kind==UnityEngine.LogType.Assert)errors++;};
void Set(string name,object value)=>type.GetField(name,flags).SetValue(runtime,value);
void Configure(){
 Set("_mainMenu",stage==4);Set("_gameOver",false);Set("_userPaused",stage==1);Set("_paused",true);Set("_levelUp",false);Set("_stellaRewardPending",stage==3);Set("_tutorialStep",stage==1||stage==3||stage==4?-1:1);Set("_runSaveAccumulator",-100f);
 rewards.Clear();if(stage==3)rewards.AddRange(EchoSphere.Presentation.NativeRunCatalog.Instance.Sets.SelectMany(set=>set.ArtifactIds).Distinct().Take(3).Select(EchoSphere.Core.ArtifactRewardRules.Get));
 var ui=type.GetField("_sourceUi",flags).GetValue(runtime);if(ui!=null)ui.GetType().GetField("Dirty",flags).SetValue(ui,true);counts.Clear();
}
void Wrap(){
 var document=type.GetField("_nativeRunDocument",flags).GetValue(runtime);if(document==null)return;
 var registry=typeof(EchoSphere.Presentation.NativeRunDocument).GetField("_backdrops",flags).GetValue(document);
 var bindings=(System.Collections.IDictionary)registry.GetType().GetField("_bindings",flags).GetValue(registry);
 foreach(System.Collections.DictionaryEntry pair in bindings){
  var element=(UnityEngine.UIElements.VisualElement)pair.Key;var effect=pair.Value.GetType().GetField("Effect",flags).GetValue(pair.Value);
  var definition=(UnityEngine.UIElements.FilterFunctionDefinition)effect.GetType().GetField("_definition",flags).GetValue(effect);if(!wrapped.Add(definition))continue;
  var passes=definition.passes;
  for(int i=0;i<passes.Length;i++){
   int slot=i;var pass=passes[i];var prior=pass.applySettingsCallback;string key=element.name+"/"+i;
   pass.applySettingsCallback=(properties,context)=>{counts.TryGetValue(key,out int count);counts[key]=count+1;prior?.Invoke(properties,context);};passes[i]=pass;
   restore.Add(()=>{if(definition==null)return;var current=definition.passes;current[slot].applySettingsCallback=prior;definition.passes=current;});
  }
  definition.passes=passes;
 }
}
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string result){
 UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.logMessageReceived-=onError;
 try{System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/native-backdrop-visibility.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks,errors,observations},Newtonsoft.Json.Formatting.Indented));}
 finally{foreach(var action in restore)action();rewards.Clear();rewards.AddRange(savedRewards);for(int i=0;i<names.Length;i++)Set(names[i],saved[i]);runtime.enabled=oldEnabled;UnityEngine.Application.runInBackground=oldBackground;UnityEditor.SessionState.SetString("EchoBackdropVisibility",result);}
}
runtime.enabled=false;UnityEngine.Application.runInBackground=true;UnityEngine.Application.logMessageReceived+=onError;
try{Configure();}catch{Finish("FAIL configure");throw;}
tick=()=>{try{
 UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new System.Exception("Play/deadline");
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;type.GetMethod("LateUpdate",flags).Invoke(runtime,null);Wrap();
 if(++frames==6)counts.Clear();if(frames<40)return;
 int Calls(string prefix)=>counts.Where(pair=>pair.Key.StartsWith(prefix)).Sum(pair=>pair.Value);
 int pause=Calls("native-pause-overlay/"),reward=Calls("native-rewards-host/"),tutorial=Calls("tutorial-");
 if((stage==1?pause<=0:pause!=0)||(stage==3?reward<=0:reward!=0)||(stage==0||stage==2||stage==5?tutorial<=0:tutorial!=0))throw new System.Exception("Wrong filter activity at "+stages[stage]+": pause="+pause+" rewards="+reward+" tutorial="+tutorial);
 if(stage==4&&counts.Values.Sum()!=0)throw new System.Exception("Hidden run still draws filters from main menu");
 if(errors!=0)throw new System.Exception("New Console errors: "+errors);
 observations.Add(new{stage=stages[stage],pause,reward,tutorial,calls=counts.ToArray()});checks++;
 if(++stage==stages.Length){Finish("PASS hidden filters inactive; visible filters resume");return;}frames=0;Configure();
}catch(System.Exception ex){Finish("FAIL "+ex);}};
UnityEditor.EditorApplication.update+=tick;UnityEditor.SessionState.SetString("EchoBackdropVisibility","RUNNING");return "Started six native filter visibility cases";
