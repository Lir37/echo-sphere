using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public static class SphereCoreSizeAcceptance {
 public static object Main(){
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var surfaceType=asm.GetType("EchoSphere.Presentation.NativeWorldSurface");var coreType=asm.GetType("EchoSphere.Presentation.NativeWorldStandardSphere");var familyType=asm.GetType("EchoSphere.Presentation.NativeWorldSphereFamilies");var stateType=familyType.GetNestedType("State",BindingFlags.NonPublic);var inputType=familyType.GetNestedType("OrbitalInput",BindingFlags.NonPublic);int checks=0;var rows=new List<object>();
  object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,F).Invoke(o,args);void Set(object o,string name,object value)=>o.GetType().GetField(name,F).SetValue(o,value);
  var go=new GameObject("Core size actual mesh fixture");var surface=go.AddComponent(surfaceType);Call(surface,"Initialize");Call(surface,"SetViewport",160f,160f,1f);
  try{foreach(var scale in new[]{.75d,1d,1.25d})foreach(var family in new[]{"sniper","shotgun","chain","aura","prism","gravity","pulse","void","orbital"}){
    var cores=Activator.CreateInstance(coreType,true);var renderer=Activator.CreateInstance(familyType,true);
    try{Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");Call(cores,"BeginFrame");var state=Activator.CreateInstance(stateType);Set(state,"Family",family);Set(state,"Scale",scale);Set(state,"Time",1d);
     if(family=="orbital"){var input=Activator.CreateInstance(inputType);Set(input,"View",state);Set(input,"Identity",new object());Set(input,"RadiusScale",1d);Call(renderer,"DrawOrbital",surface,cores,input,false);}else Call(renderer,"DrawFamily",surface,cores,state);
     Call(surface,"End");var rigs=(IDictionary)coreType.GetField("_rigs",F).GetValue(cores);if(rigs.Count!=1)throw new Exception("Unexpected core geometry count "+family);
     foreach(DictionaryEntry entry in rigs){var mesh=(Mesh)entry.Value.GetType().GetField("Core",F).GetValue(entry.Value);float expected=(float)(24*scale*.42*2);checks++;if(Math.Abs(mesh.bounds.size.x-expected)>.05f||Math.Abs(mesh.bounds.size.y-expected)>.05f)throw new Exception("Different core size "+family+" "+mesh.bounds.size+" expected "+expected);rows.Add(new{family,scale,width=mesh.bounds.size.x,height=mesh.bounds.size.y});}
    }finally{((IDisposable)renderer).Dispose();((IDisposable)cores).Dispose();}
   }}finally{UnityEngine.Object.DestroyImmediate(go);}
  System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/sphere-core-size-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{result="PASS",checks,rows}));return new{result="PASS",checks};
 }
}
