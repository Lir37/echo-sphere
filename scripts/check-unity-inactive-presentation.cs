// Run in Play Mode against the existing runtime; never instantiate a second runtime.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(runtime==null||!runtime.UsesSourcePresentation)throw new System.InvalidOperationException("Existing source-presentation runtime required");
var fixture=new UnityEngine.GameObject("Inactive presentation diagnostic");
var wasPaused=UnityEditor.EditorApplication.isPaused;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
UnityEngine.GameObject starRoot=null;
try{
    var player=fixture.AddComponent<EchoSphere.Runtime.SourcePlayerVisual2D>();player.Initialize(runtime,null);
    var elemental=fixture.AddComponent<EchoSphere.Runtime.SourceElementalVisual2D>();elemental.Initialize(runtime,null);
    var standard=fixture.AddComponent<EchoSphere.Runtime.StandardSphereAttackVfx2D>();standard.Initialize(runtime,null);
    var projectile=fixture.AddComponent<EchoSphere.Runtime.ProjectileVfx2D>();projectile.Initialize(runtime,UnityEngine.Vector2.right,4,UnityEngine.Color.white,null);
    foreach(var component in new UnityEngine.Behaviour[]{player,elemental,standard,projectile}){
        if(component.enabled)throw new System.Exception(component.GetType().Name+" still updates");
        foreach(var field in component.GetType().GetFields(flags))if((typeof(UnityEngine.Mesh).IsAssignableFrom(field.FieldType)||typeof(UnityEngine.Material).IsAssignableFrom(field.FieldType))&&field.GetValue(component)!=null)throw new System.Exception(component.GetType().Name+" allocated "+field.Name);
    }
    if(fixture.transform.childCount!=0||fixture.GetComponentsInChildren<UnityEngine.Renderer>().Length!=0)throw new System.Exception("Inactive producer created render children");
    projectile.SetSourceMotion(UnityEngine.Vector2.up,7);
    boss.SetCombatState(.1f,.2f,.3f,true,UnityEngine.Vector2.right,4);
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
        UnityEditor.SessionState.SetString("EchoInactivePresentationCheck","PASS: four inactive producers, no mesh/material/render children, setters safe; 210 world-space stars owned and destroyed");
    };
    UnityEditor.EditorApplication.update+=callback;
    UnityEditor.EditorApplication.isPaused=false;
}catch{if(fixture!=null)UnityEngine.Object.Destroy(fixture);if(starRoot!=null)UnityEngine.Object.Destroy(starRoot);throw;}
return UnityEditor.SessionState.GetString("EchoInactivePresentationCheck","not started");
