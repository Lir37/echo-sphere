// Actual mesh-envelope and GPU parity checks. No scene gameplay or preferences.
// Mesh owners use Unity's deferred destruction, so run in Play Mode.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
object Validate(){
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var wt=assembly.GetType("EchoSphere.Presentation.NativeWorldSurface");var ct=assembly.GetType("EchoSphere.Presentation.NativeWorldCreatures");var viewType=ct.GetNestedType("EnemyView",flags);var signalType=ct.GetNestedType("BossSignal",flags);
object Call(object o,string name,params object[] input){var m=o.GetType().GetMethods(flags).Single(x=>x.Name==name);var p=m.GetParameters();var args=new object[p.Length];for(int i=0;i<args.Length;i++)args[i]=i<input.Length?input[i]:p[i].DefaultValue;return m.Invoke(o,args);}
void Set(object o,string n,object v)=>o.GetType().GetField(n,flags).SetValue(o,v);
object Field(object o,string n)=>o.GetType().GetField(n,flags).GetValue(o);
var go=new UnityEngine.GameObject("Early enemy culling GPU fixture");var world=go.AddComponent(wt);var reference=System.Activator.CreateInstance(ct,true);var actual=System.Activator.CreateInstance(ct,true);
var variants=new[]{"wisp","leech","serpent","skitter","moth","beetle","brute","prism","stalker","widow","shooter","charger","summoner","aura"};
int envelopes=0,vertices=0,pixels=0,maxDelta=0,culled=0;long beforeBatches=0,afterBatches=0;
UnityEngine.Color32[] Read(){var texture=(UnityEngine.RenderTexture)wt.GetProperty("UiTexture",flags).GetValue(world);var old=UnityEngine.RenderTexture.active;var copy=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false);try{UnityEngine.RenderTexture.active=texture;copy.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);return copy.GetPixels32();}finally{UnityEngine.RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(copy);}}
try{
Call(world,"Initialize");Call(world,"SetViewport",320f,240f,1f);
// Audit the complete emitted body geometry against the analytic envelope.
foreach(var variant in variants)foreach(float radius in new[]{1f,9f,19f,64f,120f})foreach(double time in new[]{0,.13,1.7,13.9,99.1}){
 bool boss=System.Array.IndexOf(variants,variant)>=10;Call(world,"Begin",UnityEngine.Vector2.zero);Call(world,"ResetTransform");Call(reference,"BeginFrame");
 Call(reference,"Draw",world,(double)radius,UnityEngine.Color.cyan,time,variant,boss,(UnityEngine.Matrix4x4?)UnityEngine.Matrix4x4.identity);
 if(!boss){Call(reference,"DrawElite",world,(double)radius,UnityEngine.Color.cyan,time);Call(reference,"DrawStatus",world,(double)radius,time,true,true,true);}
 float extent=4*radius+32;foreach(var batch in (System.Collections.IEnumerable)Field(world,"_batches")){
  var mesh=(UnityEngine.Mesh)Field(batch,"NativeMesh");var matrix=(UnityEngine.Matrix4x4)Field(batch,"NativeMatrix");
  foreach(var vertex in mesh.vertices){var p=matrix.MultiplyPoint3x4(vertex);float x=p.x/EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld,y=-p.y/EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;
   if(System.Math.Abs(x)>extent+.01||System.Math.Abs(y)>extent+.01)throw new System.Exception("Body envelope exceeded: "+variant+" / "+radius+" / "+time+" / "+x+","+y);vertices++;}
 }envelopes++;
}
// Same identities follow an inside/outside/inside route in two independent
// pose owners. Offscreen frames must still advance the same gait/facing state.
Set(reference,"DisableEarlyBodyCullingForValidation",true);
foreach(float dpr in new[]{1f,1.75f})foreach(var variant in variants){
 bool boss=System.Array.IndexOf(variants,variant)>=10;var identity=new object();int step=0;
 foreach(var point in new[]{new UnityEngine.Vector2(70,70),new UnityEngine.Vector2(700,700),new UnityEngine.Vector2(650,660),new UnityEngine.Vector2(300,200),new UnityEngine.Vector2(-500,80)}){
  var view=System.Activator.CreateInstance(viewType);Set(view,"Identity",identity);Set(view,"X",(double)point.x);Set(view,"Y",(double)point.y);Set(view,"Radius",19d);Set(view,"Variant",variant);Set(view,"Boss",boss);Set(view,"BossName","BODY CULL");Set(view,"Hp",50d);Set(view,"MaxHp",100d);Set(view,"Color",UnityEngine.Color.cyan);Set(view,"Elite",true);Set(view,"Tier",3d);Set(view,"Fire",true);Set(view,"Poison",true);Set(view,"Freeze",step==2);Set(view,"HitFlash",.1d);Set(view,"LinkTarget",!boss);Set(view,"LinkX",140d);Set(view,"LinkY",120d);Set(view,"LinkTimer",.4d);Set(view,"LinkWindow",.75d);
  var signal=System.Activator.CreateInstance(signalType);Set(signal,"Boss",boss);Set(signal,"Type",variant);Set(signal,"Radius",19d);Set(signal,"Charging",true);Set(signal,"ChargeX",-1d);Set(signal,"ChargeY",0d);Set(signal,"ChargeTimer",.5d);Set(signal,"ShootTimer",0d);Set(signal,"SummonTimer",0d);Set(signal,"ShooterWindow",.7d);Set(signal,"SummonerWindow",.9d);Set(signal,"AuraWindow",.8d);Set(signal,"AuraRadius",650d);Set(signal,"TargetX",140d-point.x);Set(signal,"TargetY",120d-point.y);Set(view,"Signal",signal);
  Call(world,"SetViewport",320f,240f,dpr);
  UnityEngine.Color32[] Render(object owner){Call(world,"Begin",new UnityEngine.Vector2(160,120));Call(world,"ResetTransform");Call(owner,"BeginFrame");Call(owner,"DrawEnemy",world,view,step*.31);Call(world,"End");return Read();}
  var expected=Render(reference);beforeBatches+=((System.Collections.ICollection)Field(world,"_batches")).Count;
  var result=Render(actual);afterBatches+=((System.Collections.ICollection)Field(world,"_batches")).Count;culled+=(int)ct.GetProperty("CulledBodyCount",flags).GetValue(actual);
  for(int i=0;i<result.Length;i++){var a=expected[i];var b=result[i];int delta=System.Math.Max(System.Math.Max(System.Math.Abs(a.r-b.r),System.Math.Abs(a.g-b.g)),System.Math.Max(System.Math.Abs(a.b-b.b),System.Math.Abs(a.a-b.a)));maxDelta=System.Math.Max(maxDelta,delta);if(delta>1)throw new System.Exception("Early culling GPU mismatch "+variant+" / step "+step+" / "+delta);}
  pixels++;step++;
 }
}
if(culled==0||afterBatches>=beforeBatches)throw new System.Exception("No early geometry reduction");
var report=new{result="PASS",envelopes,vertices,gpuCases=pixels,maxDelta,culled,beforeBatches,afterBatches};System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Library/native-enemy-early-culling.json"),Newtonsoft.Json.JsonConvert.SerializeObject(report));return report;
}finally{((System.IDisposable)reference).Dispose();((System.IDisposable)actual).Dispose();UnityEngine.Object.DestroyImmediate(go);}
}
UnityEditor.EditorApplication.CallbackFunction check=null;check=()=>{UnityEditor.EditorApplication.update-=check;try{var result=Validate();UnityEditor.SessionState.SetString("EchoEarlyEnemyCulling",Newtonsoft.Json.JsonConvert.SerializeObject(result));}catch(System.Exception e){UnityEditor.SessionState.SetString("EchoEarlyEnemyCulling","FAIL "+e.GetBaseException().Message);}};
UnityEditor.SessionState.SetString("EchoEarlyEnemyCulling","RUNNING");UnityEditor.EditorApplication.update+=check;return "Queued actual mesh and GPU validation";
