using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
using EchoSphere.Presentation;
public static class AllSphereMechanismsAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");UnityEditor.EditorApplication.isPaused=true;
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;int checks=0;
  void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
  var asm=typeof(EchoSphereRuntime).Assembly;var wt=asm.GetType("EchoSphere.Presentation.NativeWorldSurface");var weaponType=asm.GetType("EchoSphere.Presentation.NativeWorldSphereWeapons");var stateType=weaponType.GetNestedType("State",BindingFlags.NonPublic);var coreType=asm.GetType("EchoSphere.Presentation.NativeWorldStandardSphere");var familyType=asm.GetType("EchoSphere.Presentation.NativeWorldSphereFamilies");var familyStateType=familyType.GetNestedType("State",BindingFlags.NonPublic);var feedbackType=asm.GetType("EchoSphere.Presentation.NativeWorldFeedback");
  void Set(object o,string name,object value)=>o.GetType().GetField(name,F).SetValue(o,value);
  object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,F).Invoke(o,args);
  var go=new GameObject("Mechanical weapons GPU acceptance");var world=go.AddComponent(wt);Call(world,"Initialize");Call(world,"SetViewport",240f,180f,1.75f);var weapons=Activator.CreateInstance(weaponType,true);var cores=Activator.CreateInstance(coreType,true);var families=Activator.CreateInstance(familyType,true);var feedback=Activator.CreateInstance(feedbackType,true);Set(families,"MechanicalWeapons",weapons);var output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../../UnityEvaluations/ECHO-SPHERE-architecture-20261028/user-feedback"));output=@"C:\Users\toxik\Documents\UnityEvaluations\ECHO-SPHERE-architecture-20261028\user-feedback";Directory.CreateDirectory(output);
  object Model(string family,string branch,int final,double x=0,double y=0,double radius=32){var s=Activator.CreateInstance(stateType);Set(s,"Family",family);Set(s,"Branch",branch);Set(s,"Final",final);Set(s,"X",x);Set(s,"Y",y);Set(s,"Radius",radius);Set(s,"AttackDelay",1.2d);Set(s,"ShotTime",double.NegativeInfinity);return s;}
  Color32[] Read(){var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);return image.GetPixels32();}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
  void Save(string name){var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
  Color32[] Frame(object state){Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(weapons,"BeginFrame");Call(weapons,"Draw",world,state);Call(world,"End");return Read();}
  var rows=new List<object>();
  try{
   var model=Model("standard","standard_resonator",0);var idle=Frame(model);Check(idle.Count(c=>c.r+c.g+c.b>0)>50,"Weapon invisible");Set(model,"Time",10d);Check(idle.SequenceEqual(Frame(model)),"Idle weapon cycles without shots");Set(model,"ShotSerial",1u);Set(model,"Time",10.01d);Set(model,"ShotTime",10.01d);Frame(model);Set(model,"Time",10.07d);var attack=Frame(model);Check(!idle.SequenceEqual(attack),"Parts did not move on shot");Save("mechanical-weapon-attack.png");Set(model,"Time",11d);Check(idle.SequenceEqual(Frame(model)),"Parts did not return to exact rest geometry");Save("mechanical-weapon-idle.png");
   // Build all actual branch/final rigs, then inspect every rigid part pose.
   foreach(var family in new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"}){
    Frame(Model(family,"",-1));foreach(var branch in NativeRunCatalog.Instance.GetSphere(family).Branches){Frame(Model(family,branch.Id,-1));for(int i=0;i<3;i++)Frame(Model(family,branch.Id,i));}
   }
   var rigs=(IDictionary)weaponType.GetField("_rigs",F).GetValue(weapons);Check(rigs.Count==130,"Expected10base +120branch/final rigs");var geometryByFamily=new Dictionary<string,HashSet<string>>();int parts=0,moved=0;
   var pose=weaponType.GetMethod("Pose",BindingFlags.Static|BindingFlags.NonPublic);
   foreach(DictionaryEntry entry in rigs){var key=entry.Key;string family=(string)key.GetType().GetField("Item1").GetValue(key),branch=(string)key.GetType().GetField("Item2").GetValue(key);var list=(IEnumerable)entry.Value.GetType().GetField("Parts",F).GetValue(entry.Value);using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    foreach(var part in list){parts++;float maxTravel=0;var type=part.GetType();var rest=(Matrix4x4)pose.Invoke(null,new[]{part,(object)double.PositiveInfinity,.3d});
     foreach(var age in new[]{0d,.02,.06,.12,.20,.28,.5}){var matrix=(Matrix4x4)pose.Invoke(null,new[]{part,(object)age,.3d});Check(Math.Abs(matrix.determinant-1)<.0001,"Part scale changed during attack");Check(Math.Abs(matrix.GetColumn(0).magnitude-1)<.0001&&Math.Abs(matrix.GetColumn(1).magnitude-1)<.0001,"Animated resizing");if(matrix!=rest)moved++;var stroke=(Mesh)type.GetField("Stroke",F).GetValue(part);foreach(var v in stroke.vertices)maxTravel=Math.Max(maxTravel,Vector3.Distance(rest.MultiplyPoint3x4(v),matrix.MultiplyPoint3x4(v)));}
     Check(maxTravel>=.055f,"Part motion invisible at game size: "+family+"/"+branch+" travel="+maxTravel);
     var mesh=(Mesh)type.GetField("Stroke",F).GetValue(part);foreach(var vertex in mesh.vertices){var point=rest.MultiplyPoint3x4(vertex);writer.Write(point.x);writer.Write(point.y);}writer.Write(mesh.vertexCount);
    }
    if(branch.Length>0){if(!geometryByFamily.TryGetValue(family,out var set)){set=new HashSet<string>();geometryByFamily.Add(family,set);}string hash=Convert.ToBase64String(SHA256.Create().ComputeHash(stream.ToArray()));Check(set.Add(hash),"Identical branch/final geometry in "+family+"/"+branch);}
   }
   // Mirrored mutation fins must counter-rotate, not cancel the sign twice.
   int mirrorPairs=0;
   foreach(DictionaryEntry entry in rigs){
    var key=entry.Key;string branch=(string)key.GetType().GetField("Item2").GetValue(key);int final=(int)key.GetType().GetField("Item3").GetValue(key);var all=((IEnumerable)entry.Value.GetType().GetField("Parts",F).GetValue(entry.Value)).Cast<object>().ToArray();
    Vector2 Rest(object p)=>(Vector2)p.GetType().GetField("Rest",F).GetValue(p);
    float Number(object p,string name)=>(float)p.GetType().GetField(name,F).GetValue(p);
    bool Cheek(object p)=>Math.Abs(Number(p,"Swing"))>.2799f&&Math.Abs(Number(p,"Swing"))<.2801f&&Math.Abs(((Vector2)p.GetType().GetField("Slide",F).GetValue(p)).x+.035f)<.0001f;
    foreach(var top in all.Where(p=>Rest(p).y<0&&(Cheek(p)||(branch=="standard_swarm"&&final==1&&Math.Abs(Rest(p).x-1.07f)<.0001f&&Math.Abs(Rest(p).y+.31f)<.0001f)))){
     var origin=Rest(top);var bottom=all.Single(p=>Math.Abs(Rest(p).x-origin.x)<.0001f&&Math.Abs(Rest(p).y+origin.y)<.0001f&&Math.Abs(Math.Abs(Number(p,"Swing"))-Math.Abs(Number(top,"Swing")))<.0001f);mirrorPairs++;
     Check(Math.Abs(Number(top,"Swing")+Number(bottom,"Swing"))<.0001f,"Same-direction mutation fin swing: "+branch+"/"+final);
     foreach(var age in new[]{.03d,.09,.18,.27,.42}){
      var a=(Matrix4x4)pose.Invoke(null,new[]{top,(object)age,.48d});var b=(Matrix4x4)pose.Invoke(null,new[]{bottom,(object)age,.48d});
      Check(Math.Abs((a.m13-origin.y)+(b.m13+origin.y))<.0001f,"Mutation pair has same lateral translation: "+branch);
      Check(Math.Abs(Math.Atan2(a.m10,a.m00)+Math.Atan2(b.m10,b.m00))<.0001f,"Mutation pair does not counter-rotate: "+branch);
     }
    }
    if(branch=="standard_resonator"&&final==2){var shutters=all.Where(p=>Math.Abs(Math.Abs(Rest(p).y)-.21f)<.0001f&&Rest(p).x>=1.099f&&Rest(p).x<=1.281f).ToArray();Check(shutters.Length==3,"Expected three staggered suppression shutters");foreach(var shutter in shutters){var slide=(Vector2)shutter.GetType().GetField("Slide",F).GetValue(shutter);Check(slide.y*Rest(shutter).y>0,"Suppression shutter moves toward wrong side");}}
   }
   Check(mirrorPairs>=10,"Missing mirrored mutation fixtures");
   // VII must add enough actual geometry to its own IV parent, not just a hash difference.
   foreach(var family in new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"})foreach(var branch in NativeRunCatalog.Instance.GetSphere(family).Branches){
    int Count(int final){object rig=rigs[ValueTuple.Create(family,branch.Id,final)];return ((IList)rig.GetType().GetField("Parts",F).GetValue(rig)).Count;}
    int iv=Count(-1);for(int final=0;final<3;final++)Check(Count(final)>=iv+6,"VII too similar to IV: "+branch.Id+"/"+final);
   }
   // Compare actual rendered IV/VII silhouettes at combat radius, not only mesh hashes.
   foreach(var family in new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"})foreach(var branch in NativeRunCatalog.Instance.GetSphere(family).Branches){
    var iv=Frame(Model(family,branch.Id,-1,radius:24));int ink=iv.Count(p=>p.r+p.g+p.b>40);
    for(int final=0;final<3;final++){var vii=Frame(Model(family,branch.Id,final,radius:24));int changed=0;for(int i=0;i<iv.Length;i++)if(Math.Abs(iv[i].r-vii[i].r)+Math.Abs(iv[i].g-vii[i].g)+Math.Abs(iv[i].b-vii[i].b)>45)changed++;Check(changed>=Math.Max(24,ink*.18),"VII has no clear rendered difference: "+branch.Id+"/"+final+" changed="+changed+" IV="+ink);}
   }
   // Slow/stalled frames must still show a recent shot; old offscreen shots must stay settled.
   foreach(var family in new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"}){
    var slow=Model(family,"",-1);Set(slow,"Time",30d);var rest=Frame(slow);Set(slow,"ShotSerial",1u);Set(slow,"ShotTime",30.01d);Set(slow,"Time",30.24d);Check(!rest.SequenceEqual(Frame(slow)),"Missed shot after a 240ms render gap: "+family);Set(slow,"Time",31d);Check(rest.SequenceEqual(Frame(slow)),"Old shot replayed on return: "+family);
   }
   Check(moved>0,"No articulated parts");foreach(var set in geometryByFamily.Values)Check(set.Count==12,"Missing one of12designs");
   // Fields are omnidirectional, with small parts around the core and no aim root.
   foreach(var family in new[]{"aura","gravity","pulse"})foreach(var branch in NativeRunCatalog.Instance.GetSphere(family).Branches)for(int final=-1;final<3;final++){
    var field=Model(family,branch.Id,final);var a=Frame(field);Set(field,"Rotation",1.23d);Check(a.SequenceEqual(Frame(field)),"Area field aims like a gun: "+branch.Id);
    object rig=rigs[ValueTuple.Create(family,branch.Id,final)];var list=((IEnumerable)rig.GetType().GetField("Parts",F).GetValue(rig)).Cast<object>().ToArray();var quadrants=new HashSet<int>();
    foreach(var part in list){var rest=(Vector2)part.GetType().GetField("Rest",F).GetValue(part);Check(rest.magnitude>=.50f&&rest.magnitude<=1.01f,"Field part not around core: "+branch.Id);quadrants.Add((rest.x>=0?1:0)+(rest.y>=0?2:0));Check(Math.Abs((float)part.GetType().GetField("Recoil",F).GetValue(part))<.00001,"Field has directional gun recoil");}Check(quadrants.Count==4,"Field parts only on one side");
   }
   // Blade radial tips stay outward; asymmetry trails EACH ring's actual motion.
   var placement=weaponType.GetMethod("OrbitalSatellitePose",BindingFlags.Static|BindingFlags.NonPublic);
   foreach(bool inner in new[]{true,false})foreach(double phase in new[]{0d,.4,1.7,3.8,5.9})for(int i=0;i<4;i++){
    double angle=OrbitalVisualGeometryRules.ElementAngle(phase,!inner,i,4),next=OrbitalVisualGeometryRules.ElementAngle(phase+.001,!inner,i,4);
    var matrix=(Matrix4x4)placement.Invoke(null,new object[]{0d,0d,1d,angle,inner});var outward=new Vector3((float)Math.Cos(angle),(float)Math.Sin(angle),0);var motion=new Vector3((float)(Math.Cos(next)-Math.Cos(angle)),(float)(Math.Sin(next)-Math.Sin(angle)),0).normalized;
    Check(Vector3.Dot(matrix.MultiplyVector(Vector3.right),outward)>.9999f,"Orbital cutting tip faces inward");Check(Vector3.Dot(matrix.MultiplyVector(Vector3.down),motion)<-.99f,"Asymmetric orbital edge faces wrong travel direction");Check(Math.Abs(matrix.GetColumn(0).magnitude-1)<.0001&&Math.Abs(matrix.GetColumn(1).magnitude-1)<.0001,"Orbital mirror changes size");
   }
   // Actual engine contact sheets, with the same cores/materials as battle.
   foreach(var family in new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"}){
    float sheetHeight=family=="orbital"?1320f:1080f;Call(world,"SetViewport",1000f,sheetHeight,1.5f);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(weapons,"BeginFrame");Call(cores,"BeginFrame");Call(families,"BeginFrame");Call(feedback,"BeginFrame",world);Call(feedback,"Rectangle",-500d,-sheetHeight/2,1000d,(double)sheetHeight,0d,new Color(.012f,.030f,.065f,1));Call(feedback,"Text",family+" - Unity / IV + VII",0d,-sheetHeight/2+35d,Color.white,18f);
    var branches=NativeRunCatalog.Instance.GetSphere(family).Branches;
    for(int col=0;col<3;col++)for(int row=0;row<4;row++){double x=-330+col*330,y=family=="orbital"?-450+row*300:-350+row*230;var branch=branches[col];int final=row-1;string label=row==0?"IV: "+branch.Name(true):"VII: "+branch.Finals[final].Name(true).Split('•').Last().Trim();Call(feedback,"Text",label,x+20,y-(family=="orbital"?145:95),Color.white,10f);
     if(family=="standard"){Call(cores,"DrawCore",world,55d,.5d,(Matrix4x4?)Matrix4x4.Translate(new Vector3((float)x,(float)y,0)),(Color?)new Color(.39f,.90f,1));Call(cores,"DrawRing",world,65d,.6d,.75d,(Matrix4x4?)Matrix4x4.Translate(new Vector3((float)x,(float)y,0)));}
     else if(family=="orbital"){
      var fs=Activator.CreateInstance(familyStateType);Set(fs,"Family",family);Set(fs,"X",x);Set(fs,"Y",y);Set(fs,"Scale",55d/24);Set(fs,"Time",2.5d);Set(fs,"Rotation",.45d);
      var inputType=familyType.GetNestedType("OrbitalInput",BindingFlags.NonPublic);var input=Activator.CreateInstance(inputType);Set(input,"Identity",new object());Set(input,"View",fs);Set(input,"Branch",branch.Id);Set(input,"Final",final);Set(input,"RadiusScale",1d);
      int level=final<0?4:7;var count=OrbitalVisualGeometryRules.RingCounts(level,branch.Id=="orbital_dance"&&final==2?1:0);var inside=new double[count.Inner];var outside=new double[count.Outer];for(int i=0;i<inside.Length;i++)inside[i]=OrbitalVisualGeometryRules.ElementAngle(.45,false,i,inside.Length);for(int i=0;i<outside.Length;i++)outside[i]=OrbitalVisualGeometryRules.ElementAngle(.45,true,i,outside.Length);Set(input,"InnerAngles",inside);Set(input,"OuterAngles",outside);Set(input,"Enemies",Array.CreateInstance(familyType.GetNestedType("EnemyPoint",BindingFlags.NonPublic),0));
      Call(families,"DrawOrbital",world,cores,input,false);Call(families,"DrawOrbital",world,cores,input,true);
     }
     else{var fs=Activator.CreateInstance(familyStateType);Set(fs,"Family",family);Set(fs,"X",x);Set(fs,"Y",y);Set(fs,"Scale",55d/24);Set(fs,"Time",2.5d);Call(families,"DrawFamily",world,cores,fs,false);}
     if(family!="orbital")Call(weapons,"Draw",world,Model(family,branch.Id,final,x,y,55));
    }Call(world,"End");Save("all-spheres-"+family+"-unity.png");rows.Add(new{family,designs=12});
   }
   // Engine-rendered mechanical poses: translation/rotation of every piece, never scale.
   Call(world,"SetViewport",900f,440f,1.5f);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(weapons,"BeginFrame");Call(feedback,"BeginFrame",world);Call(feedback,"Rectangle",-450d,-220d,900d,440d,0d,new Color(.012f,.030f,.065f,1));
   var phases=new[]{0d,.06,.14,.24,.36,.60};var familyNames=new[]{"standard","sniper","shotgun","chain","aura","orbital","prism","gravity","pulse","void"};
   for(int row=0;row<3;row++)for(int col=0;col<phases.Length;col++){var sample=Model(familyNames[row],"",-1,-415+col*150,-140+row*145,54);Set(sample,"ShotSerial",1u);Set(sample,"ShotTime",0d);Set(sample,"Time",phases[col]);Call(weapons,"Draw",world,sample);Call(feedback,"Text",familyNames[row]+" "+phases[col].ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),-360d+col*150,-190d+row*145,Color.white,12f);}
   Call(world,"End");Save("mechanical-moving-parts-unity.png");
   // Warm poses must never upload or retessellate meshes.
   var versions=new Dictionary<Mesh,ulong>();var shapes=asm.GetType("EchoSphere.Presentation.NativeWorldShapes");var version=shapes.GetMethod("GeometryVersion",BindingFlags.Static|BindingFlags.NonPublic);
   foreach(DictionaryEntry entry in rigs)foreach(var part in (IEnumerable)entry.Value.GetType().GetField("Parts",F).GetValue(entry.Value))foreach(string name in new[]{"Fill","Stroke","Halo"}){var mesh=(Mesh)part.GetType().GetField(name,F).GetValue(part);if(mesh!=null)versions[mesh]=(ulong)version.Invoke(null,new object[]{mesh});}
   Call(world,"SetViewport",240f,180f,1.75f);for(int i=0;i<12;i++){Set(model,"Time",20d+i*.02);if(i==0){Set(model,"ShotSerial",2u);Set(model,"ShotTime",20d);}Frame(model);}foreach(var pair in versions)Check((ulong)version.Invoke(null,new object[]{pair.Key})==pair.Value,"Animation rebuilt retained geometry");
   var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var rt=runtime.GetType();var select=rt.GetMethod("EntitySnapshotField",BindingFlags.NonPublic|BindingFlags.Static);Check(!(bool)select.Invoke(null,new object[]{typeof(SphereAttackAgent).GetField("_presentationShotSerial",F)}),"Shot serial entered save");Check(!(bool)select.Invoke(null,new object[]{typeof(SphereAttackAgent).GetField("_presentationShotTime",F)}),"Shot timestamp entered save");
   foreach(string name in new[]{"_mainMenu","_paused","_userPaused","_gameOver","_levelUp","_choosingEvolution","_stellaRewardPending"})Set(runtime,name,false);Set(runtime,"_tutorialStep",-1);Set(runtime,"_level",1);Set(runtime,"_xpRequired",RunBalanceRules.GetXpToNextLevel(1));Set(runtime,"_xp",3f);rt.GetMethod("NativeDebugLevelUp",F).Invoke(runtime,null);Check((int)rt.GetField("_level",F).GetValue(runtime)==2,"Test button did not grant exactly one level");Check((bool)rt.GetField("_levelUp",F).GetValue(runtime),"Normal upgrade selection did not open");Check((float)rt.GetField("_xp",F).GetValue(runtime)==3,"Test button discarded partial XP");var choices=(IList)rt.GetField("_levelUpChoices",F).GetValue(runtime);Check(choices.Count>0,"Missing ordinary choices");rt.GetMethod("NativeDebugLevelUp",F).Invoke(runtime,null);Check((int)rt.GetField("_level",F).GetValue(runtime)==2,"Repeated modal click granted another level");
   rt.GetMethod("ReadNativeMobileState",F).Invoke(runtime,null);var dock=rt.GetField("_nativeMobileDockState",F).GetValue(runtime);var controls=(IEnumerable)dock.GetType().GetField("Controls",F).GetValue(dock);Check(controls.Cast<object>().Count(c=>(string)c.GetType().GetField("Id",F).GetValue(c)=="debug-levelup")==1,"Missing or duplicate test control");Set(runtime,"_userPaused",true);Set(runtime,"_paused",true);
   var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{result="PASS",checks,parts,moved,mirrorPairs,rigs=rigs.Count,rows,scope="11 mirrored mutation pairs counter-rotate/translate over5phases; Actual GPU idle/shot/rest and slow frame recovery;130retained rigs,12unique per family,VII adds6+parts,every part moves55milli-radius or more,rigid transforms,no tessellation changes;normal levelup via test control;temporary test preferences must be restored."});File.WriteAllText(Path.Combine(Application.dataPath,"../Library/all-sphere-mechanisms-acceptance.json"),json);return new{result="PASS",checks,parts,rigs=rigs.Count};
  }finally{((IDisposable)weapons).Dispose();((IDisposable)cores).Dispose();((IDisposable)families).Dispose();((IDisposable)feedback).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
