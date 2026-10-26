// Actual tutorial, pickup/reward, death/result and return actions in Play Mode.
// Back up preferences first; stop Play and restore afterwards. Starts a fresh run.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var initial=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(!(bool)initial.GetType().GetField("_mainMenu",flags).GetValue(initial))throw new System.Exception("Start from native menu");
if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("EchoGameplayPrefsBackup","")))throw new System.Exception("Preference backup required");
UnityEngine.PlayerPrefs.SetInt("echosphere_tutorial_completed_v1",0);
UnityEngine.Application.runInBackground=true;
int phase=0,checks=0,errors=0,last=-1;double deadline=UnityEditor.EditorApplication.timeSinceStartup+180;
var stages=new List<object>();UnityEngine.Vector2 startPoint=default,dragPoint=default,expectedPlacement=default;
UnityEngine.Vector3 movementStart=default;bool reversed=false;EchoSphere.Runtime.SphereAttackAgent placed=null;
UnityEngine.UIElements.VisualElement dragTarget=null;int beforeCount=0,rewardBefore=0;string rewardId=null;
UnityEditor.EditorApplication.CallbackFunction tick=null;
UnityEngine.Application.LogCallback errorHandler=(message,trace,kind)=>{if(kind==UnityEngine.LogType.Error||kind==UnityEngine.LogType.Exception||kind==UnityEngine.LogType.Assert)errors++;};
object Read(EchoSphere.Runtime.EchoSphereRuntime r,string key)=>r.GetType().GetField(key,flags).GetValue(r);
object Call(EchoSphere.Runtime.EchoSphereRuntime r,string key,params object[] values)=>r.GetType().GetMethod(key,flags).Invoke(r,values);
void Require(bool value,string message){checks++;if(!value)throw new System.Exception(message);}
bool Visible(UnityEngine.UIElements.VisualElement e){for(;e!=null;e=e.parent)if(e.resolvedStyle.display==UnityEngine.UIElements.DisplayStyle.None||e.resolvedStyle.visibility==UnityEngine.UIElements.Visibility.Hidden||e.resolvedStyle.opacity<=0)return false;return true;}
UnityEngine.UIElements.Button Find(string name){foreach(var doc in UnityEngine.Object.FindObjectsByType<UnityEngine.UIElements.UIDocument>()){
 if(!doc.enabled||doc.rootVisualElement==null)continue;var b=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(doc.rootVisualElement,name);
 if(b!=null&&b.enabledInHierarchy&&Visible(b)&&b.worldBound.width>0&&b.worldBound.height>0){var picked=b.panel.Pick(b.worldBound.center);if(picked==b||b.Contains(picked))return b;}
 }return null;}
bool Submit(string name){var b=Find(name);if(b==null)return false;using(var e=UnityEngine.UIElements.NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}checks++;return true;}
void Pointer(UnityEngine.UIElements.VisualElement e,string kind,UnityEngine.Vector2 point){var raw=new UnityEngine.Event{type=kind=="down"?UnityEngine.EventType.MouseDown:kind=="move"?UnityEngine.EventType.MouseMove:UnityEngine.EventType.MouseUp,button=0,mousePosition=point};
 if(kind=="down"){using(var p=UnityEngine.UIElements.PointerDownEvent.GetPooled(raw)){p.target=e;e.SendEvent(p);}}
 else if(kind=="move"){using(var p=UnityEngine.UIElements.PointerMoveEvent.GetPooled(raw)){p.target=e;e.SendEvent(p);}}
 else{using(var p=UnityEngine.UIElements.PointerUpEvent.GetPooled(raw)){p.target=e;e.SendEvent(p);}}}
bool World(EchoSphere.Runtime.EchoSphereRuntime r,UnityEngine.Vector2 p,out double x,out double y){var args=new object[]{(double)p.x,(double)p.y,0d,0d};bool result=(bool)Call(r,"NativeMobileWorld",args);x=(double)args[2];y=(double)args[3];return result;}
UnityEngine.Vector2 ScreenPoint(EchoSphere.Runtime.EchoSphereRuntime r,UnityEngine.Vector3 world){World(r,UnityEngine.Vector2.zero,out double x,out double y);World(r,UnityEngine.Vector2.one,out double dx,out double dy);float unit=EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;return new UnityEngine.Vector2((float)((world.x/unit-x)/(dx-x)),(float)((-world.y/unit-y)/(dy-y)));}
UnityEngine.UIElements.VisualElement Gesture(EchoSphere.Runtime.EchoSphereRuntime r)=>(UnityEngine.UIElements.VisualElement)Read(r,"_nativeMobileGestureRoot");
bool PickField(EchoSphere.Runtime.EchoSphereRuntime r,UnityEngine.Vector2 point,out UnityEngine.UIElements.VisualElement picked){var gesture=Gesture(r);picked=gesture?.panel?.Pick(point);return picked!=null&&(picked==gesture||gesture.Contains(picked));}
List<EchoSphere.Runtime.SphereAttackAgent> Spheres(EchoSphere.Runtime.EchoSphereRuntime r)=>(List<EchoSphere.Runtime.SphereAttackAgent>)Read(r,"_spheres");
void Advance(){stages.Add(new{phase,frame=UnityEngine.Time.frameCount});phase++;UnityEditor.SessionState.SetString("EchoNativeTutorialFlow","RUNNING phase="+phase);}
void Finish(string result){UnityEditor.EditorApplication.update-=tick;UnityEngine.Application.logMessageReceived-=errorHandler;UnityEditor.EditorApplication.isPaused=true;UnityEditor.SessionState.SetString("EchoNativeTutorialFlow",Newtonsoft.Json.JsonConvert.SerializeObject(new{result,phase,checks,errors,stages,width=UnityEngine.Screen.width,height=UnityEngine.Screen.height,scope="Real tutorial, gameplay Update, UI submissions and routed pointer callbacks in Editor; preferences require restoration; not physical touch/device acceptance"}));}
UnityEditor.SessionState.SetString("EchoNativeTutorialFlow","RUNNING");
tick=()=>{try{
 if(!UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new System.Exception("Tutorial deadline at phase "+phase);
 if(last==UnityEngine.Time.frameCount)return;last=UnityEngine.Time.frameCount;
 var r=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();if(r==null)return;
 Require(errors==0,"New Unity Console error");
 if(phase==0){if(Submit("open-map"))Advance();return;}
 if(phase==1){if(Submit("start-resonance_basin"))Advance();return;}
 int step=(int)Read(r,"_tutorialStep");
 if(phase<19)Require(!(bool)Read(r,"_regionSelect")&&!(bool)Read(r,"_mainMenu"),"Unexpected menu/region during run");
 if(phase==2){if(step!=0)return;if(Submit("tutorial-next"))Advance();return;}
 if(phase==3){if(step!=1)return;var bounds=Call(r,"ReadNativeRunTutorialTarget","movement");if(bounds==null)return;startPoint=((UnityEngine.Rect)bounds).center;
  if(!PickField(r,startPoint,out dragTarget))return;movementStart=((UnityEngine.Transform)Read(r,"_player")).position;Pointer(dragTarget,"down",startPoint);Pointer(dragTarget,"move",startPoint+new UnityEngine.Vector2(0,-58));Require((UnityEngine.Vector2)Read(r,"_nativeMobileMoveInput")==UnityEngine.Vector2.up,"Vertical joystick inversion");Advance();return;}
 if(phase==4){var position=((UnityEngine.Transform)Read(r,"_player")).position;
  // The initial tower is directly above the spawn. Verify Up before reaching
  // its collision radius, then move Down into free space to finish this step.
  if(!reversed){if(position.y-movementStart.y<.2f)return;Require(position.y>movementStart.y&&UnityEngine.Mathf.Abs(position.x-movementStart.x)<.02f,"Actual player movement disagrees with Up");Pointer(dragTarget,"move",startPoint+new UnityEngine.Vector2(0,58));Require((UnityEngine.Vector2)Read(r,"_nativeMobileMoveInput")==UnityEngine.Vector2.down,"Vertical Down inversion");reversed=true;return;}
  if(!(bool)Call(r,"CanAdvanceNativeTutorial")||position.y>=movementStart.y)return;Pointer(dragTarget,"up",startPoint+new UnityEngine.Vector2(0,58));Require(position.y<movementStart.y,"Actual player movement disagrees with Down");if(Submit("tutorial-next"))Advance();return;}
 if(phase==5){if(step!=2)return;var field=(UnityEngine.UIElements.VisualElement)Read(r,"_nativeRunField");var rect=field.worldBound;
  foreach(float y in new[]{.3f,.45f,.6f})foreach(float x in new[]{.25f,.4f,.6f,.75f}){var point=new UnityEngine.Vector2(rect.xMin+rect.width*x,rect.yMin+rect.height*y);
   if(!PickField(r,point,out var target)||!World(r,point,out double wx,out double wy)||!(bool)Call(r,"NativeMobileCanPlace",wx,wy)||Call(r,"NativeMobileNear",wx,wy,34d)!=null)continue;
   var before=Spheres(r).ToArray();beforeCount=before.Length;expectedPlacement=new UnityEngine.Vector2((float)wx,-(float)wy)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;
   Pointer(target,"down",point);Pointer(target,"up",point);placed=Spheres(r).FirstOrDefault(s=>!before.Contains(s));Require(placed!=null&&Spheres(r).Count==beforeCount+1,"Tap did not place one sphere");
   Require(UnityEngine.Vector2.Distance(placed.transform.position,expectedPlacement)<.0001f,"Sphere placed away from tap");Advance();return;}
  return;}
 if(phase==6){if(Submit("tutorial-next"))Advance();return;}
 if(phase==7){if(step!=3)return;if(Submit("tutorial-next"))Advance();return;}
 if(phase==8){if(step!=4)return;var choices=(System.Collections.IList)Read(r,"_levelUpChoices");Require(choices.Count>0,"Tutorial has no upgrade choices");for(int i=0;i<choices.Count;i++)if(Submit("pick-upgrade-"+i)){Advance();break;}return;}
 if(phase==9){if(step!=5)return;if(Submit("tutorial-next"))Advance();return;}
 if(phase==10){Require(step==-1&&UnityEngine.PlayerPrefs.GetInt("echosphere_tutorial_completed_v1")==1,"Tutorial completion not persisted");
  startPoint=ScreenPoint(r,placed.transform.position);if(!PickField(r,startPoint,out dragTarget))return;var handle=Call(r,"NativeMobileHandle",placed);var field=(UnityEngine.UIElements.VisualElement)Read(r,"_nativeRunField");
  foreach(var offset in new[]{new UnityEngine.Vector2(70,0),new UnityEngine.Vector2(-70,0),new UnityEngine.Vector2(0,70),new UnityEngine.Vector2(0,-70)}){var target=startPoint+offset;
   if(!field.worldBound.Contains(target)||!World(r,target,out double wx,out double wy)||!(bool)Call(r,"NativeMobileCanMove",handle,wx,wy))continue;
   expectedPlacement=new UnityEngine.Vector2((float)wx,-(float)wy)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;dragPoint=target;
   Pointer(dragTarget,"down",startPoint);Pointer(dragTarget,"move",target);Require(Read(r,"_nativeMobileGhost")!=null,"No drag preview");Advance();return;}
  throw new System.Exception("No legal drag destination");}
 if(phase==11){World(r,dragPoint,out double releaseX,out double releaseY);expectedPlacement=new UnityEngine.Vector2((float)releaseX,-(float)releaseY)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;Pointer(dragTarget,"up",dragPoint);Require(UnityEngine.Vector2.Distance(placed.transform.position,expectedPlacement)<.0001f,"Sphere drag committed away from pointer");Require(Spheres(r).Count==beforeCount+1,"Drag changed sphere count");Advance();return;}
 if(phase==12){var b=Find("native-mobile-pause");if(b==null)return;Pointer(b,"down",b.worldBound.center);Pointer(b,"up",b.worldBound.center);Advance();return;}
 if(phase==13){if(!r.UsesNativePauseChrome)return;Require((bool)Read(r,"_userPaused"),"Pause command not applied");if(Submit("pause-chrome-resume"))Advance();return;}
 if(phase==14){if((bool)Read(r,"_paused"))return;Call(r,"SpawnStellaChest",r.PlayerPosition);Advance();return;}
 if(phase==15){if(!(bool)Read(r,"_stellaRewardPending"))return;var choices=(List<EchoSphere.Core.ArtifactRewardDefinition>)Read(r,"_stellaRewardChoices");
  Require(choices.Count>0,"Collected chest produced no choices");foreach(var choice in choices){var b=Find("artifact-reward-"+choice.Id);if(b==null)continue;
   var point=new UnityEngine.Vector2(b.worldBound.center.x,b.worldBound.yMin+20);var picked=b.panel.Pick(point);if(picked==null||!(picked==b||b.Contains(picked)))continue;
   bool nested=false;for(var e=picked;e!=b;e=e.parent)if(e is UnityEngine.UIElements.Button)nested=true;if(nested)continue;
   rewardBefore=((List<string>)Read(r,"_ownedArtifacts")).Count;rewardId=choice.Id;
   Pointer(picked,"down",point);Pointer(picked,"up",point);Pointer(picked,"down",point);Pointer(picked,"up",point);
   Require(((List<string>)Read(r,"_ownedArtifacts")).Count==rewardBefore+1,"Reward applied more or less than once");Advance();return;}
  return;}
 if(phase==16){Require(!(bool)Read(r,"_stellaRewardPending")&&((List<string>)Read(r,"_ownedArtifacts")).Contains(rewardId),"Reward was not consumed");Advance();return;}
 if(phase==17){if(!(bool)Read(r,"_gameOver")){r.TryDamagePlayer(1000000);return;}if(Find("result-return")!=null)Advance();return;}
 if(phase==18){if(Submit("result-return"))Advance();return;}
 if(phase==19){if(!(bool)Read(r,"_mainMenu")||!(bool)Read(r,"_regionSelect"))return;if(Submit("region-back"))Advance();return;}
 if(phase==20){if(!r.UsesNativeMainMenu)return;Require(!(bool)Read(r,"_regionSelect"),"Region did not close after Back");Finish("PASS tutorial, actual Up/Down, exact tap/drag, pause/resume, real chest/reward, duplicate input guard, death/result and intended region/home return");}
}catch(System.Exception error){Finish("FAIL "+error);}};
UnityEngine.Application.logMessageReceived+=errorHandler;UnityEditor.EditorApplication.update+=tick;return "Started complete native tutorial and placement flow";
