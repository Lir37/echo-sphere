// Isolated native GPU fixture; never changes the live runtime or preferences.
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using UnityEngine;
public static class CharacterMotionAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var a=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var pt=a.GetType("EchoSphere.Presentation.NativeWorldPlayer");var st=pt.GetNestedType("State",F);var wt=a.GetType("EchoSphere.Presentation.NativeWorldSurface");
  object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,F).Invoke(o,args);
  void Set(object o,string n,object value)=>o.GetType().GetField(n,F).SetValue(o,value);
  object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  var go=new GameObject("Character motion fixture");var world=go.AddComponent(wt);Call(world,"Initialize");Call(world,"SetViewport",140f,100f,1.75f);
  var feedback=Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldFeedback"),true);bool backdrop=false;
  var player=Activator.CreateInstance(pt,true);var shadows=Activator.CreateInstance(a.GetType("EchoSphere.Presentation.NativeWorldCreatures"),true);
  var core=Get(player,"_core");object state=null;var point=Vector2.zero;
  void Frame(double time){Set(state,"X",(double)point.x);Set(state,"Y",(double)point.y);Set(state,"Time",time);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");Call(player,"BeginFrame");Call(shadows,"BeginFrame");if(backdrop){Call(feedback,"BeginFrame",world);Call(feedback,"Rectangle",-70d,-50d,140d,100d,0d,new Color(.012f,.030f,.065f,1));}Call(player,"Draw",world,state,shadows);Call(world,"End");}
  IList Batches()=>(IList)Get(world,"_batches");
  int TrailDraws(){
   var trail=(Mesh)Get(player,"_cometMesh");int count=0;
   foreach(var batch in Batches())if(trail!=null&&ReferenceEquals(Get(batch,"NativeMesh"),trail)){
    count++;Check((float)Get(batch,"Blur")==0,"Trail must not add blur passes");var tint=(Color)Get(state,"Color");
    foreach(var color in trail.colors){Check(color.a<=.141f,"Trail too opaque");Check(Math.Abs(color.r-tint.r)+Math.Abs(color.g-tint.g)+Math.Abs(color.b-tint.b)<.0001,"Trail lost character hue");}
   }return count;
  }
  try{
   foreach(var id in new[]{"spherist","hunter","engineer","berserker","alchemist","architect","conductor","oracle","voidwalker","fractal"}){
    state=Activator.CreateInstance(st);Set(state,"Character",id);Set(state,"Radius",16d);Set(state,"Hp",10d);EchoSphere.Core.CharacterRules.TryParseSourceId(id,out var character);ColorUtility.TryParseHtmlString(EchoSphere.Core.CharacterRules.Get(character).ColorHex,out var tint);Set(state,"Color",tint);point=Vector2.zero;Frame(100);
    foreach(var direction in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down}){
     for(int i=0;i<3;i++){
      point+=direction*2;double time=(double)Get(player,"_lastTime")+.016;Frame(time);
      foreach(var batch in Batches())if(ReferenceEquals(Get(batch,"NativeMaterial"),core)){
       var matrix=(Matrix4x4)Get(batch,"NativeMatrix");Check(matrix.m00>0,"Body mirrored on movement: "+id);Check(Math.Abs(matrix.m01)<.00001&&Math.Abs(matrix.m10)<.00001,"Body turned on movement: "+id);
      }
     }
     Check(TrailDraws()>0,"Missing movement trail "+id);
    }
    double stop=(double)Get(player,"_lastTime")+.40;Frame(stop);Check(TrailDraws()==0,"Trail persists at rest "+id);
    point+=Vector2.right*1000;Frame(stop+.016);Check((int)Get(player,"_trailCount")==0,"Trail spans teleport "+id);
   }
   Call(world,"SetViewport",230f,130f,1.75f);backdrop=true;state=Activator.CreateInstance(st);Set(state,"Character","spherist");Set(state,"Radius",16d);Set(state,"Hp",10d);Set(state,"Color",Color.cyan);point=new Vector2(-50,0);Frame(300);
   for(int i=0;i<18;i++){point+=new Vector2(5,0);Frame(300.016+i*.016);}
   var rt=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var previous=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/character-contours"));Directory.CreateDirectory(dir);
   try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,"movement-trail-unity.png"),image.EncodeToPNG());}finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);}
   var result=new{result="PASS",checks,characters=10,directions=4,trailSamples=18,fadeSeconds=.38,scope="Native body matrices never mirror/turn with motion; trail appears, fades at rest, resets at teleport and uses no blur; live scene untouched"};File.WriteAllText(Path.Combine(dir,"motion-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{((IDisposable)player).Dispose();((IDisposable)shadows).Dispose();((IDisposable)feedback).Dispose();UnityEngine.Object.DestroyImmediate(go);}
 }
}
