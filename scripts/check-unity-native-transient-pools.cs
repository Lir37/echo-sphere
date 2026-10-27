// Run in Play Mode. Isolated disabled runtime: no scene gameplay, RNG or saves.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var go=new UnityEngine.GameObject("Transient pool acceptance");var runtime=go.AddComponent<EchoSphere.Runtime.EchoSphereRuntime>();runtime.enabled=false;
var type=runtime.GetType();type.GetField("_mainMenu",flags).SetValue(runtime,false);
object Field(object value,string name)=>value.GetType().GetField(name,flags).GetValue(value);
void Retire(UnityEngine.MonoBehaviour agent)=>type.GetMethod("RetireNativeTransient",flags).Invoke(runtime,new object[]{agent});
void Flush(){var pool=Field(runtime,"_nativeTransientPools");pool.GetType().GetMethod("Flush",flags).Invoke(pool,null);}
int checks=0;void Check(bool value,string label){if(!value)throw new System.Exception(label);checks++;}
var agents=(System.Collections.Generic.List<UnityEngine.MonoBehaviour>)Field(runtime,"_simulationAgents");
try{
 var first=runtime.SpawnProjectile(new UnityEngine.Vector2(2,3),UnityEngine.Vector2.right,14,UnityEngine.Color.red,1,2,0,null,1,.01f);
 ((System.Collections.Generic.HashSet<int>)Field(first,"_hitEnemyIds")).Add(999);
 first.GetType().GetField("_sourceElement",flags).SetValue(first,EchoSphere.Core.SphereElement.Fire);
 Retire(first);Retire(first);Check(first.gameObject.activeInHierarchy&&agents.Contains(first),"retired before phase boundary");
 var beforeFlush=runtime.SpawnProjectile(UnityEngine.Vector2.zero,UnityEngine.Vector2.left,2,UnityEngine.Color.blue);
 Check(!ReferenceEquals(first,beforeFlush),"reused identity during current frame");Flush();Check(!first.gameObject.activeInHierarchy&&!agents.Contains(first),"release registration");
 var reused=runtime.SpawnProjectile(new UnityEngine.Vector2(7,8),UnityEngine.Vector2.up,23,UnityEngine.Color.green,4,0,0,null,0,3);
 Check(ReferenceEquals(first,reused),"projectile not reused");Check(reused.transform.position==new UnityEngine.Vector3(7,8,0),"projectile position");
 Check(((System.Collections.Generic.HashSet<int>)Field(reused,"_hitEnemyIds")).Count==0,"stale hit IDs");Check((EchoSphere.Core.SphereElement)Field(reused,"_sourceElement")==EchoSphere.Core.SphereElement.None,"stale element");
 Check((float)Field(reused,"_damage")==23&&(float)Field(reused,"_life")==3&&(int)Field(reused,"_remainingPierces")==0&&(int)Field(reused,"_remainingRicochets")==0,"stale projectile stats");
 Check(ReferenceEquals(Field(reused,"_runtime"),runtime)&&Field(reused,"_evolutionOwner")==null,"stale owner");
 var referenceGo=new UnityEngine.GameObject("Fresh projectile snapshot reference");var reference=referenceGo.AddComponent<EchoSphere.Runtime.ProjectileAgent>();reference.Initialize(runtime,UnityEngine.Vector2.up,23,UnityEngine.Color.green,4,0,0,null,0,3);
 Check(UnityEngine.JsonUtility.ToJson(EchoSphere.Core.RunSnapshotCodec.Capture(reference))==UnityEngine.JsonUtility.ToJson(EchoSphere.Core.RunSnapshotCodec.Capture(reused)),"reused projectile save differs from fresh instance");
 agents.Remove(reference);UnityEngine.Object.Destroy(referenceGo);
 Retire(beforeFlush);Retire(reused);Flush();
 runtime.SpawnSourceParticle(new UnityEngine.Vector2(1,2),new UnityEngine.Vector2(20,-10),1,4,"#ff0000");var particle=agents.OfType<EchoSphere.Runtime.SourceParticle2D>().Single();
 particle.SimulationTick(.1f);Check(UnityEngine.Vector2.Distance(particle.transform.position,new UnityEngine.Vector2(1,2)+new UnityEngine.Vector2(20,-10)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld*.1f)<.00001f,"particle position integration");
 Check(UnityEngine.Vector2.Distance((UnityEngine.Vector2)Field(particle,"_velocity"),new UnityEngine.Vector2(20,-10)*EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld*.95f)<.00001f,"particle per-step damping");
 Retire(particle);Flush();runtime.SpawnSourceParticle(new UnityEngine.Vector2(8,9),UnityEngine.Vector2.zero,.5f,7,"#00ff00",2);
 var next=agents.OfType<EchoSphere.Runtime.SourceParticle2D>().Single();Check(ReferenceEquals(particle,next),"particle not reused");Check((float)Field(next,"_life")==.5f&&(float)Field(next,"_maxLife")==2&&(float)Field(next,"_sourceSize")==7,"stale particle fields");
 var snapshot=EchoSphere.Core.RunSnapshotCodec.Capture(next);next.SimulationTick(.1f);EchoSphere.Core.RunSnapshotCodec.Restore(snapshot,typeof(EchoSphere.Runtime.SourceParticle2D),next);
 Check((float)Field(next,"_life")==.5f&&(float)Field(next,"_maxLife")==2,"pooled particle save roundtrip");
 next.SimulationTick(.6f);Check(next.gameObject.activeInHierarchy,"particle retired before boundary");Flush();Check(!next.gameObject.activeInHierarchy,"expired particle retained");
 for(int i=0;i<230;i++)runtime.SpawnSourceParticle(UnityEngine.Vector2.zero,UnityEngine.Vector2.zero,1,3,"#ffffff");
 Check(agents.OfType<EchoSphere.Runtime.SourceParticle2D>().Count(p=>p.gameObject.activeInHierarchy)==220,"particle cap changed");Flush();Check(agents.OfType<EchoSphere.Runtime.SourceParticle2D>().Count()==220,"retired particles remain registered");
 foreach(var agent in agents.ToArray())Retire(agent);Flush();Check(agents.Count==0,"pool did not release all agents");
 for(int i=0;i<50;i++){var p=runtime.SpawnProjectile(UnityEngine.Vector2.zero,UnityEngine.Vector2.right,1,UnityEngine.Color.white);Retire(p);Flush();}Check(agents.Count==0,"reuse cycles grew registration");
 var result=new{result="PASS",checks,projectileReuseCycles=50,particleCap=220};System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/native-transient-pools-check.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
}finally{UnityEngine.Object.Destroy(go);}
