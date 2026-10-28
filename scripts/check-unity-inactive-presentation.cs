// Inspect the real native scene. Does not create fallback renderers or save data.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Play Mode required");
var runtime=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
if(runtime==null||!runtime.UsesSourcePresentation)throw new System.InvalidOperationException("Native runtime required");
var assembly=runtime.GetType().Assembly;
string[] retired={"SourcePlayerVisual2D","SourceElementalVisual2D","StandardSphereAttackVfx2D","ProjectileVfx2D","BossPresentationVfx2D","EnemyCreatureVisual2D","SphereRuntimeVfx2D","SourceRegionVisual2D","SourceCanvasMesh","CombatArcVfx2D","SourceBackdrop2D","SpherePulseVfx2D","HunterParticleVfx2D","EngineerRelayVfx2D","ImpactVfx2D","PulseEffect2D"};
foreach(var name in retired)if(assembly.GetType("EchoSphere.Runtime."+name)!=null)throw new System.Exception("Retired runtime type returned: "+name);
foreach(var transform in UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include))
 if(transform.name=="Source Backdrop"||transform.name=="Source Backdrop Stars"||transform.name=="Source World Bounds")throw new System.Exception("Legacy backdrop object returned");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
if(runtime.GetType().GetProperty("_nativeWorldField",flags).GetValue(runtime)==null||runtime.GetType().GetProperty("_nativeWorldAmbient",flags).GetValue(runtime)==null)throw new System.Exception("Active retained field/ambient absent");
var result=new{result="PASS retired renderers absent; retained native field/ambient active",types=retired.Length};
UnityEditor.SessionState.SetString("EchoInactivePresentationCheck",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
