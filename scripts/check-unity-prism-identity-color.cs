using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
public static class PrismIdentityColorAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();var type=runtime.GetType();var asm=type.Assembly;
  object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,F).Invoke(o,args);
  var go=new GameObject("Prism identity GPU acceptance");var surface=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",150f,150f,1.75f);
  var presentation=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldPresentation"));
  var oldSurface=type.GetField("_worldSurface",F).GetValue(runtime);var oldPresentation=type.GetField("_nativeWorldPresentation",F).GetValue(runtime);type.GetField("_worldSurface",F).SetValue(runtime,surface);type.GetField("_nativeWorldPresentation",F).SetValue(runtime,presentation);
  var stateType=type.GetNestedType("NativeSphereVisual",F);int checks=0,cases=0;Color32[] baseline=null;
  try{
   foreach(var branch in new[]{"","prism_split","prism_spectrum","prism_mirror"})foreach(int final in new[]{-1,0,1,2})foreach(var element in new string[]{null,"fire","freeze","poison"}){
    if(branch.Length==0&&final>=0)continue;
    var state=Activator.CreateInstance(stateType);void Set(string n,object v)=>stateType.GetField(n,F).SetValue(state,v);
    Set("Type",SphereId.Prism);Set("Family","prism");Set("Branch",branch);Set("Element",element);Set("Final",(double)final);Set("Alive",true);
    Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");Call(presentation,"BeginFrame",surface);type.GetMethod("DrawNativeSphereFamily",F).Invoke(runtime,new object[]{state,.5d,1d,false});Call(surface,"End");
    var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
    try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);var pixels=image.GetPixels32();if(baseline==null){checks++;if(pixels.Count(x=>x.a>100)<100)throw new Exception("Prism body not visible");baseline=pixels;}else{checks++;if(!baseline.SequenceEqual(pixels))throw new Exception("Prism core/orbits recolored by "+branch+" / "+final+" / "+element);}cases++;}
    finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
   }
   var result=new{result="PASS",checks,cases,scope="Actual runtime sphere projection/draw path, base+IV/VII and null/fire/freeze/poison; core+orbits pixel-identical; element accent/effects kept separate"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/prism-identity-color-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{type.GetField("_worldSurface",F).SetValue(runtime,oldSurface);type.GetField("_nativeWorldPresentation",F).SetValue(runtime,oldPresentation);UnityEngine.Object.DestroyImmediate(go);}
 }
}
