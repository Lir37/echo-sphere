using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEditor;using UnityEngine;using EchoSphere.Core;using EchoSphere.Presentation;
public static class RegionDepthCheck
{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");Directory.CreateDirectory("Library/region-depth");
  int checks=0;void A(bool value,string why){checks++;if(!value)throw new Exception(why);}
  var asm=typeof(NativeWorldPresentation).Assembly;Type T(string name)=>asm.GetType("EchoSphere.Presentation."+name,true);
  object Call(object obj,string method,params object[] args)=>obj.GetType().GetMethods(F).Single(m=>m.Name==method&&m.GetParameters().Length==args.Length).Invoke(obj,args);
  var type=T("NativeWorldDepthBackdrop");var assets=T("NativeRegionBackdropAssets");int Owners()=>(int)assets.GetProperty("OwnerCount",F).GetValue(null);
  int baseline=Owners();
  foreach(var viewport in new[]{new Vector2(360,800),new Vector2(1080,2400),new Vector2(800,360)})
  {
   var span=(Vector2)type.GetMethod("ImageSpan",F).Invoke(null,new object[]{viewport.x,viewport.y,941f/1672});
   foreach(float cx in new[]{-1200f,0,1200f})foreach(float cy in new[]{-1200f,0,1200f})foreach(float x in new[]{0f,viewport.x})foreach(float y in new[]{0f,viewport.y})
   {var uv=(Vector2)type.GetMethod("DistantUv",F).Invoke(null,new object[]{new Vector2(x,y),viewport,new Vector2(cx,cy),span});A(uv.x>=0&&uv.x<=1&&uv.y>=0&&uv.y<=1,"parallax image covers arena/camera extremes");}
   Vector2 U(Vector2 camera)=>(Vector2)type.GetMethod("DistantUv",F).Invoke(null,new object[]{viewport*.5f,viewport,camera,span});
   var dx=U(new Vector2(200,0))-U(Vector2.zero);var dy=U(new Vector2(0,200))-U(Vector2.zero);
   A(Math.Abs(dx.x*span.x-7)<.001&&Math.Abs(dx.y)<.0001,"far horizontal motion is 3.5 percent");
   A(Math.Abs(dy.x*span.x-1.12)<.001&&Math.Abs(-dy.y*span.y-6.58)<.001,"oblique projection affects only distant plane");
  }
  for(int r=0;r<6;r++)
  {
   string id=CampaignRegions.SourceId((EchoRegionId)r);string path="Assets/Resources/EchoSphere/RegionBackdrops/"+id+".jpg";
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);A(importer!=null&&!importer.isReadable&&importer.mipmapEnabled,"background import/lifetime "+id);
   var android=importer.GetPlatformTextureSettings("Android");A(android.overridden&&android.format==TextureImporterFormat.ASTC_6x6,"ASTC "+id);
   var list=CampaignRegionContent.GetContracts((EchoRegionId)r);for(int i=0;i<list.Count;i++)A((int)type.GetMethod("TrialStyle",F).Invoke(null,new object[]{r,list[i].Id})==i+1,"existing contract mapping "+list[i].Id);
   A((int)type.GetMethod("TrialStyle",F).Invoke(null,new object[]{r,"not-a-contract"})==0,"no guessed challenge");
  }
  var shader=Resources.Load<Shader>("EchoSphere/NativeWorldDepthBackdrop");A(shader!=null&&shader.isSupported,"supported background shader");
  var backdrop=Activator.CreateInstance(type,true);var owner=new GameObject("Owned regional depth verification");var surface=owner.AddComponent(T("NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",450f,800f,1f);
  var field=Activator.CreateInstance(T("NativeWorldField"),true);var creatures=Activator.CreateInstance(T("NativeWorldCreatures"),true);var player=Activator.CreateInstance(T("NativeWorldPlayer"),true);
  object Player(Vector2 camera){var state=Activator.CreateInstance(T("NativeWorldPlayer").GetNestedType("State",F));void S(string key,object value)=>state.GetType().GetField(key,F).SetValue(state,value);S("X",(double)camera.x);S("Y",(double)camera.y);S("Radius",14d);S("Hp",100d);S("Time",2d);S("Character","hunter");S("Color",new Color(.75f,.3f,1));return state;}
  void Actors(int region,Vector2 camera)
  {
   Call(field,"Draw",surface,2400f,2400f,-1200f,-1200f,camera.x,camera.y);Call(creatures,"BeginFrame");Call(player,"BeginFrame");Call(player,"Draw",surface,Player(camera),creatures);
   var viewType=T("NativeWorldCreatures").GetNestedType("EnemyView",F);
   for(int i=0;i<4;i++){var view=Activator.CreateInstance(viewType);void S(string key,object value)=>viewType.GetField(key,F).SetValue(view,value);S("Identity",(object)("depth-enemy-"+region+"-"+i));S("X",i%2==0?-116d:112d);S("Y",i<2?-140d:170d);S("Radius",18d);S("Hp",50d);S("MaxHp",50d);S("Role",i%2==0?"grunt":"ranged");S("Variant",i%2==0?"beetle":"wisp");S("RegionStyle",region);S("Fleet",region==1);S("Color",new Color(.25f,.7f,.8f));S("Portrait",true);Call(creatures,"DrawEnemy",surface,view,2d);}
  }
  byte[] Draw(int region,int trial,Vector2 camera,bool actors)
  {
   var random=JsonUtility.ToJson(UnityEngine.Random.state);Call(surface,"Begin",new Vector2(225,400));Call(surface,"ResetTransform");
   string contract=trial==0?null:CampaignRegionContent.GetContracts((EchoRegionId)region)[trial-1].Id;
   Call(backdrop,"Draw",surface,450f,800f,region,camera,contract,45d);A(JsonUtility.ToJson(UnityEngine.Random.state)==random,"background consumes no Unity RNG");
   if(actors){Call(surface,"Save");Call(surface,"Translate",225d-camera.x,400d-camera.y);Actors(region,camera);Call(surface,"Restore");}
   Call(surface,"End");var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var old=RenderTexture.active;var image=new Texture2D(450,800,TextureFormat.RGB24,false);
   try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,450,800),0,0);image.Apply();var pixels=image.GetPixels32();if(!actors)A(pixels.Count(p=>p.r>180&&p.b>180&&p.g<20)<10,"no error-shader magenta");return image.EncodeToPNG();}
   finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
  }
  int regionIndex=0,trialIndex=0,phase=0,cycles=0;double since=EditorApplication.timeSinceStartup,deadline=since+180;var hashes=new HashSet<string>();EditorApplication.CallbackFunction tick=null;var keepBackground=Application.runInBackground;Application.runInBackground=true;
  void Finish(string status,string error=null){EditorApplication.update-=tick;((IDisposable)backdrop).Dispose();((IDisposable)field).Dispose();((IDisposable)creatures).Dispose();((IDisposable)player).Dispose();UnityEngine.Object.DestroyImmediate(owner);Application.runInBackground=keepBackground;File.WriteAllText("Library/region-depth/check.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status,error,checks,styles=hashes.Count,cycles,ownersBefore=baseline,ownersAfter=Owners(),scope="actual shader/GPU/native actor captures, camera extremes and parallax math, six regions and18 contracts,20 transitions, shared texture leases, no save edits"}));}
  tick=()=>{try{EditorApplication.QueuePlayerLoopUpdate();double now=EditorApplication.timeSinceStartup;if(now>deadline)throw new Exception("deadline");
   Draw(regionIndex,trialIndex,Vector2.zero,false);if(!(bool)type.GetProperty("Ready",F).GetValue(backdrop)||(int)type.GetProperty("DisplayedRegion",F).GetValue(backdrop)!=regionIndex||now-since<.8)return;
   if(phase==0){var bytes=Draw(regionIndex,trialIndex,Vector2.zero,true);File.WriteAllBytes("Library/region-depth/region-"+regionIndex+"-trial-"+trialIndex+".png",bytes);using(var sha=System.Security.Cryptography.SHA256.Create())A(hashes.Add(Convert.ToBase64String(sha.ComputeHash(bytes))),"distinct region/trial capture");
    if(trialIndex==0){var acquire=assets.GetMethod("Acquire",F);var a=(IDisposable)acquire.Invoke(null,new object[]{regionIndex});var b=(IDisposable)acquire.Invoke(null,new object[]{regionIndex});a.Dispose();A((bool)type.GetProperty("Ready",F).GetValue(backdrop),"disposing shared lease keeps current texture");b.Dispose();File.WriteAllBytes("Library/region-depth/region-"+regionIndex+"-pan.png",Draw(regionIndex,0,new Vector2(200,100),true));}
    if(++trialIndex<4)return;trialIndex=0;if(++regionIndex<6){since=now;return;}phase=1;regionIndex=0;since=now;return;}
   A((int)type.GetProperty("GeometryBuilds",F).GetValue(backdrop)==1,"retained full-screen mesh");A(Owners()<=baseline+2,"bounded active/fading leases");cycles++;if(cycles<20){regionIndex=(regionIndex+1)%6;since=now;return;}
   var errors=ShaderUtil.GetShaderMessages(shader).Where(m=>m.severity.ToString()=="Error").Select(m=>m.message).ToArray();A(errors.Length==0,string.Join(";",errors));Finish("PASS");
  }catch(Exception e){Finish("FAIL",e.ToString());}};
  EditorApplication.update+=tick;return new{status="queued",checks};
 }
}
