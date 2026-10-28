// Run after the complete tutorial flow returns to the native main menu.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
UnityEditor.EditorApplication.isPaused=false;
var operation=UnityEngine.Resources.UnloadUnusedAssets();int frame=UnityEngine.Time.frameCount;
UnityEditor.EditorApplication.CallbackFunction sample=null;
UnityEditor.SessionState.SetString("EchoLifecycleResourceSample","RUNNING");
sample=()=>{
 if(!UnityEditor.EditorApplication.isPlaying){UnityEditor.EditorApplication.update-=sample;UnityEditor.SessionState.SetString("EchoLifecycleResourceSample","FAIL Play stopped");return;}
 UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(!operation.isDone||UnityEngine.Time.frameCount<frame+3)return;
 UnityEditor.EditorApplication.update-=sample;
 try{
  System.GC.Collect();System.GC.WaitForPendingFinalizers();System.GC.Collect();
  var runtimes=UnityEngine.Object.FindObjectsByType<EchoSphere.Runtime.EchoSphereRuntime>(UnityEngine.FindObjectsInactive.Include);
  var r=runtimes.Single();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
  if(!(bool)r.GetType().GetField("_mainMenu",flags).GetValue(r)||(bool)r.GetType().GetField("_regionSelect",flags).GetValue(r))throw new System.Exception("Not at returned home menu");
  var assembly=r.GetType().Assembly;var worldType=assembly.GetType("EchoSphere.Presentation.NativeWorldSurface");
  var textures=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Texture>();var meshes=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Mesh>();var materials=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Material>();
  bool Game(UnityEngine.Object value)=>value.name.StartsWith("Native",System.StringComparison.Ordinal)||value.name.StartsWith("ECHO",System.StringComparison.Ordinal)||value.name.StartsWith("Source",System.StringComparison.Ordinal);
  var result=new {result="PASS",runtimeOwners=runtimes.Length,worldOwners=UnityEngine.Object.FindObjectsByType(worldType,UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Length,
   gameTextures=textures.Count(t=>Game(t)),gameMeshes=meshes.Count(m=>Game(m)),gameMaterials=materials.Count(m=>Game(m)),renderTextures=textures.Count(t=>t is UnityEngine.RenderTexture),allTextures=textures.Length,allMeshes=meshes.Length,allMaterials=materials.Length,
   referenceTypes=assembly.GetTypes().Count(t=>t.Name.StartsWith("Visual_",System.StringComparison.Ordinal))};
  UnityEditor.SessionState.SetString("EchoLifecycleResourceSample",Newtonsoft.Json.JsonConvert.SerializeObject(result));
 }catch(System.Exception error){UnityEditor.SessionState.SetString("EchoLifecycleResourceSample","FAIL "+error.Message);}
};
UnityEditor.EditorApplication.update+=sample;
return "Queued post-cycle native resource sample";
