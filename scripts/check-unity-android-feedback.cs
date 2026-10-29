// Run in diagnostic Play Mode after backing up Editor preferences. Stop/restore afterwards.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
UnityEditor.EditorApplication.isPaused=true;
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var r=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=r.GetType();int checks=0;
void Set(string name,object value)=>type.GetField(name,f).SetValue(r,value);
void Check(bool ok,string reason){checks++;if(!ok)throw new System.Exception(reason);}
Set("_mainMenu",false);Set("_regionSelect",false);Set("_gameOver",false);Set("_paused",false);Set("_userPaused",false);Set("_tutorialStep",-1);Set("_levelUp",false);Set("_choosingEvolution",false);Set("_stellaRewardPending",false);
var spheres=(System.Collections.Generic.List<EchoSphere.Runtime.SphereAttackAgent>)type.GetField("_spheres",f).GetValue(r);
var enemies=(System.Collections.Generic.List<EchoSphere.Runtime.EnemyAgent2D>)type.GetField("_enemies",f).GetValue(r);
foreach(var e in enemies)if(e!=null)e.gameObject.SetActive(false);enemies.Clear();
var sphere=spheres[0];Check(sphere.Type==EchoSphere.Core.SphereId.Standard,"Standard fixture required");
var initialShotSerial=sphere.PresentationShotSerial;
for(int i=0;i<200;i++)sphere.SimulationTick(.05f);
Check(sphere.PresentationShotSerial==initialShotSerial,"Empty cadence emitted shot serial");
Check(!sphere.PresentationAttackActive,"Empty cadence animated a weapon");
var target=r.SpawnBossMinion((UnityEngine.Vector2)sphere.transform.position+UnityEngine.Vector2.right*.3f,100000,0,0,"normal",UnityEngine.Color.white);
sphere.ResetAttackTimer();sphere.SimulationTick(.05f);
Check(sphere.PresentationAttackActive,"A real shot did not activate its presentation");
Check(sphere.PresentationShotSerial==initialShotSerial+1,"Real shot did not emit exactly one event");
sphere.SimulationTick(.001f);Check(sphere.PresentationShotSerial==initialShotSerial+1,"Cooldown repeated shot event");
Check(sphere.AttackTimer>0,"Attack cadence was not preserved");
target.gameObject.SetActive(false);enemies.Clear();
for(int i=0;i<200;i++)sphere.SimulationTick(.05f);
Check(!sphere.PresentationAttackActive,"Weapon animation did not return to idle without targets");
// Production save adapter must exclude the new presentation-only flag.
var select=type.GetMethod("EntitySnapshotField",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
Check(!(bool)select.Invoke(null,new object[]{typeof(EchoSphere.Runtime.SphereAttackAgent).GetField("_presentationAttackActive",f)}),"Presentation flag entered the save contract");
Set("_userPaused",true);Set("_paused",true);
UnityEngine.PlayerPrefs.SetString("echosphere_interface_scale","0.75");
UnityEngine.Application.runInBackground=true;
UnityEditor.EditorApplication.isPaused=false;
var start=UnityEditor.EditorApplication.timeSinceStartup;int frames=0,last=-1;ulong built=0;object world=null;int stage=0;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string status){UnityEditor.EditorApplication.update-=tick;UnityEditor.EditorApplication.isPaused=true;var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status,checks,frames,scope="Actual Unity Play: empty/real/empty attack cadence, save exclusion, paused world retention/invalidation, mobile scroll and combat HUD scale. No Android acceptance."});UnityEditor.SessionState.SetString("EchoAndroidFeedbackCheck",json);System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/android-feedback-check.json"),json);}
tick=()=>{try{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup-start>180)throw new System.Exception("Fixture deadline");
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;frames++;
 var doc=type.GetField("_nativeRunDocument",f).GetValue(r);if(doc==null)return;
 world=type.GetProperty("WorldPresentation",f).GetValue(r);
 ulong Count()=>(ulong)world.GetType().GetProperty("FrameBuilds",f).GetValue(world);
 if(stage==0&&frames>=30){
  Check(r.UsesNativePauseScreen,"Native pause did not become ready");
  var pause=(EchoSphere.Presentation.NativePauseScreenView)type.GetField("_nativePauseScreen",f).GetValue(r);
  Check(pause.Body.mode==UnityEngine.UIElements.ScrollViewMode.Vertical,"Horizontal pause scroll enabled");
  Check(pause.Body.horizontalScrollerVisibility==UnityEngine.UIElements.ScrollerVisibility.Hidden&&pause.Body.verticalScrollerVisibility==UnityEngine.UIElements.ScrollerVisibility.Hidden,"Desktop scrollbar visible");
  var hud=(EchoSphere.Presentation.NativeHudView)type.GetField("_nativeHudIsland",f).GetValue(r);
  var left=(UnityEngine.UIElements.VisualElement)hud.GetType().GetField("_left",f).GetValue(hud);
  Check(System.Math.Abs(left.resolvedStyle.scale.value.x-.75f)<.0001,"Combat HUD did not use 75% scale");
  Check(left.worldBound.xMin>=-.1f,"HUD escaped left screen edge");
  var dock=type.GetField("_nativeMobileDock",f).GetValue(r);
  var dockRoot=(UnityEngine.UIElements.VisualElement)dock.GetType().GetProperty("Root",f).GetValue(dock);
  foreach(var label in UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.Label>(dockRoot).ToList()){
   UnityEngine.UIElements.Button button=null;for(var node=label.parent;node!=null;node=node.parent)if(node is UnityEngine.UIElements.Button found){button=found;break;}
   if(button==null)continue;var bounds=label.worldBound;var box=button.worldBound;
   Check(bounds.xMin>=box.xMin-.5&&bounds.xMax<=box.xMax+.5&&bounds.yMin>=box.yMin-.5&&bounds.yMax<=box.yMax+.5,"Mobile control label escaped its button: "+button.name);
   if(label.resolvedStyle.whiteSpace==UnityEngine.UIElements.WhiteSpace.NoWrap){var needed=label.MeasureTextSize(label.text,0,UnityEngine.UIElements.VisualElement.MeasureMode.Undefined,0,UnityEngine.UIElements.VisualElement.MeasureMode.Undefined);Check(needed.x<=label.contentRect.width+1,"Mobile word does not fit: "+button.name);}
  }
  built=Count();stage=1;frames=0;return;
 }
 if(stage==1&&frames>=20){Check(Count()==built,"Paused world rebuilt without state/layout changes");
  var ui=type.GetField("_sourceUi",f).GetValue(r);ui.GetType().GetField("Dirty",f).SetValue(ui,true);stage=2;frames=0;return;}
 if(stage==2&&frames>=4){Check(Count()>built,"Dirty pause did not refresh the retained world");built=Count();stage=3;frames=0;return;}
 if(stage==3&&frames>=15){Check(Count()==built,"Pause did not settle after invalidation");Finish("PASS");}
 }catch(System.Exception error){Finish("FAIL "+error);}};
UnityEditor.SessionState.SetString("EchoAndroidFeedbackCheck","RUNNING");UnityEditor.EditorApplication.update+=tick;
return new{status="Started",checks};
