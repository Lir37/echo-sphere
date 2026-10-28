// Run in Play Mode against the existing runtime; never instantiate a second runtime.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(runtime==null||!runtime.UsesSourcePresentation)throw new System.InvalidOperationException("Existing source-presentation runtime required");
var fixture=new UnityEngine.GameObject("Inactive presentation diagnostic");
var wasPaused=UnityEditor.EditorApplication.isPaused;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
UnityEngine.GameObject starRoot=null;
try{
    foreach(var name in new[]{"SourcePlayerVisual2D","SourceElementalVisual2D","StandardSphereAttackVfx2D","ProjectileVfx2D","BossPresentationVfx2D","EnemyCreatureVisual2D","SphereRuntimeVfx2D","SourceRegionVisual2D","SourceCanvasMesh","CombatArcVfx2D"})
        if(typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Runtime."+name)!=null)throw new System.Exception("Retired runtime type returned: "+name);
    // Legacy fallback must own world-space stars independently of its moving quad.
    var cameraObject=new UnityEngine.GameObject("Backdrop diagnostic camera");cameraObject.transform.SetParent(fixture.transform,false);
    var camera=cameraObject.AddComponent<UnityEngine.Camera>();camera.enabled=false;
    var backdropObject=new UnityEngine.GameObject("Backdrop ownership diagnostic");backdropObject.transform.SetParent(fixture.transform,false);
    var backdrop=backdropObject.AddComponent<EchoSphere.Runtime.SourceBackdrop2D>();backdrop.Initialize(null,camera);
    starRoot=(UnityEngine.GameObject)typeof(EchoSphere.Runtime.SourceBackdrop2D).GetField("_starsRoot",flags).GetValue(backdrop);
    if(starRoot==null||starRoot.transform.childCount!=210||starRoot.transform.parent!=null||starRoot.transform.position!=UnityEngine.Vector3.zero)throw new System.Exception("World-space star ownership mismatch");
    var stars=new UnityEngine.GameObject[210];for(int i=0;i<stars.Length;i++)stars[i]=starRoot.transform.GetChild(i).gameObject;
    var first=stars[0].transform.position;backdrop.transform.position=new UnityEngine.Vector3(13,7,2);
    if(stars[0].transform.position!=first)throw new System.Exception("Backdrop movement moves world stars");
    UnityEngine.Object.Destroy(fixture);
    UnityEditor.SessionState.SetString("EchoInactivePresentationCheck","RUNNING: waiting for native destruction");
    var deadline=UnityEditor.EditorApplication.timeSinceStartup+10;
    UnityEditor.EditorApplication.CallbackFunction callback=null;
    callback=()=>{
        if(fixture!=null||starRoot!=null){if(UnityEditor.EditorApplication.timeSinceStartup<deadline)return;UnityEditor.EditorApplication.update-=callback;UnityEditor.EditorApplication.isPaused=wasPaused;UnityEditor.SessionState.SetString("EchoInactivePresentationCheck","FAIL: objects survived destruction");return;}
        UnityEditor.EditorApplication.update-=callback;
        UnityEditor.EditorApplication.isPaused=wasPaused;
        foreach(var star in stars)if(star!=null){UnityEditor.SessionState.SetString("EchoInactivePresentationCheck","FAIL: star survived owner destruction");return;}
        UnityEditor.SessionState.SetString("EchoInactivePresentationCheck","PASS: retired runtime types absent; 210 world-space stars owned and destroyed");
    };
    UnityEditor.EditorApplication.update+=callback;
    UnityEditor.EditorApplication.isPaused=false;
}catch{if(fixture!=null)UnityEngine.Object.Destroy(fixture);if(starRoot!=null)UnityEngine.Object.Destroy(starRoot);throw;}
return UnityEditor.SessionState.GetString("EchoInactivePresentationCheck","not started");
