using System;
using System.Reflection;
using UnityEngine;
public static class PortraitLeaseAcceptance {
 public static object Main(){
  var flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public;
  var library=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeCharacterArtwork");var lt=library.GetNestedType("Lease",flags);
  var a=Activator.CreateInstance(lt,true);var b=Activator.CreateInstance(lt,true);int checks=0;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  try{
   var x=lt.GetMethod("Select",flags).Invoke(a,new object[]{"oracle"});var y=lt.GetMethod("Select",flags).Invoke(b,new object[]{"oracle"});
   var sprite=(Sprite)x.GetType().GetProperty("Sprite").GetValue(x);Check(sprite!=null&&ReferenceEquals(x,y),"owners do not share resource");
   ((IDisposable)a).Dispose();Check(sprite!=null,"first owner destroyed shared sprite");
   ((IDisposable)b).Dispose();Check(!((System.Collections.IDictionary)library.GetField("Cache",flags).GetValue(null)).Contains("oracle"),"last owner retained cache entry");
   var z=lt.GetMethod("Select",flags).Invoke(a,new object[]{"oracle"});Check((Sprite)z.GetType().GetProperty("Sprite").GetValue(z)!=null,"released resource cannot reload");
   return new{result="PASS",checks};
  }finally{((IDisposable)a).Dispose();((IDisposable)b).Dispose();}
 }
}

