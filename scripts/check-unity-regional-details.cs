using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
public static class RegionalDetailAcceptance {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 public static object Main(){
  if(!Application.isPlaying)throw new Exception("Play required");var asm=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;var type=asm.GetType("EchoSphere.Presentation.NativeWorldCreatures");var vt=type.GetNestedType("EnemyView",F);
  object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F).Invoke(o,a);void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
  var go=new GameObject("Regional size/detail GPU check");var surface=go.AddComponent(asm.GetType("EchoSphere.Presentation.NativeWorldSurface"));Call(surface,"Initialize");Call(surface,"SetViewport",900f,600f,1.5f);
  var creatures=Activator.CreateInstance(type,true);var roles=new[]{"grunt","ranged","tank_guard","healer","phase","bomber"};var views=new object[30];
  var bg=(Mesh)asm.GetType("EchoSphere.Presentation.NativeWorldShapes").GetMethod("Rectangle",F).Invoke(null,new object[]{-450f,-300f,900f,600f});var mat=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var prop=new MaterialPropertyBlock();prop.SetColor("_NativeColor",new Color(.01f,.025f,.06f,1));var output=Path.GetFullPath("Library/regional-details");Directory.CreateDirectory(output);int checks=0,warmed=0;
  void Assert(bool value,string why){checks++;if(!value)throw new Exception(why);}
  try{
   for(int style=0;style<6;style++){var e=Activator.CreateInstance(vt);Set(e,"RegionStyle",style);double scale=(double)type.GetMethod("RegionalBodyScale",F).Invoke(null,new[]{e});Assert(Math.Abs(scale-(style==0?1d:2d/3d))<1e-9,"Visual scale / original region protected");}
   for(int i=0;i<views.Length;i++){var e=Activator.CreateInstance(vt);views[i]=e;Set(e,"Identity",new object());Set(e,"RegionStyle",i/6+1);Set(e,"Fleet",i<6);Set(e,"X",(double)(i%6*150-375));Set(e,"Y",(double)(i/6*120-240));Set(e,"Radius",25d);Set(e,"Role",roles[i%6]);Set(e,"Variant","skitter");Set(e,"Hp",100d);Set(e,"MaxHp",100d);Set(e,"Color",new Color(.35f,.8f,1));}
   var initial=new Dictionary<int,Dictionary<Mesh,Matrix4x4>>();var changed=new int[5];var attacks=new int[5];var idle=new Dictionary<int,Dictionary<Mesh,Matrix4x4>>();
   for(int frame=-1;frame<12;frame++){
    Call(surface,"Begin",Vector2.zero);Call(surface,"ResetTransform");var draw=surface.GetType().GetMethod("DrawNativeMesh",F);var args=draw.GetParameters().Select(p=>p.DefaultValue).ToArray();args[0]=bg;args[1]=mat;args[2]=prop;draw.Invoke(surface,args);Call(creatures,"BeginFrame");
    for(int i=0;i<views.Length;i++){
     Set(views[i],"AttackPose",frame>=8?1d:0d);Set(views[i],"Freeze",frame<0||frame>=9);Call(creatures,"DrawEnemy",surface,views[i],frame==8?.7d:frame*.10d);
     if(i%6!=0||frame<0)continue;int style=i/6+1;
     var art=(IDictionary)type.GetField("_art",F).GetValue(creatures);var meshes=new HashSet<Mesh>();
     foreach(DictionaryEntry pair in art){int id=(int)pair.Key.GetType().GetField("Item1").GetValue(pair.Key);bool detail=style==1?id>=2122&&id<=2124:id>=20000+style*1000+12&&id<=20000+style*1000+15;if(!detail)continue;var at=pair.Value.GetType();foreach(string name in new[]{"Fill","Stroke"}){var mesh=(Mesh)at.GetField(name,F).GetValue(pair.Value);if(mesh!=null)meshes.Add(mesh);}}
     var matrices=new Dictionary<Mesh,Matrix4x4>();foreach(var batch in (IEnumerable)surface.GetType().GetField("_batches",F).GetValue(surface)){var bt=batch.GetType();var mesh=(Mesh)bt.GetField("NativeMesh",F).GetValue(batch);if(mesh!=null&&meshes.Contains(mesh))matrices[mesh]=(Matrix4x4)bt.GetField("NativeMatrix",F).GetValue(batch);}
     Assert(matrices.Count>0,"New detail meshes missing region "+style);
     if(frame==0)initial[style]=matrices;else if(frame==7){changed[style-1]=matrices.Count(p=>initial[style].ContainsKey(p.Key)&&p.Value!=initial[style][p.Key]);idle[style]=matrices;}else if(frame==8){attacks[style-1]=matrices.Count(p=>idle[style].ContainsKey(p.Key)&&p.Value!=idle[style][p.Key]);idle[style]=matrices;}else if(frame>=9)Assert(matrices.All(p=>idle[style].ContainsKey(p.Key)&&p.Value==idle[style][p.Key]),"Frozen details moved region "+style);
    }
    Call(surface,"End");int count=((IDictionary)type.GetField("_art",F).GetValue(creatures)).Count;if(frame==0)warmed=count;else if(frame>0)Assert(count==warmed,"Geometry rebuilt for animation");
    if(frame==0||frame==7||frame==8){var rt=(RenderTexture)surface.GetType().GetProperty("UiTexture",F).GetValue(surface);var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,"frame-"+frame+".png"),image.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
   }
   for(int i=0;i<5;i++){Assert(changed[i]>0,"Idle detail motion missing region "+(i+2));Assert(attacks[i]>0,"Attack detail motion missing region "+(i+2));}
   var result=new{result="PASS",checks,regions="2-6",samples=30,frames=12,visualScale=2d/3d,retainedMeshes=warmed,idleMovingDetailMeshes=changed,attackMovingDetailMeshes=attacks,scope="Actual Unity GPU geometry and rigid-detail transforms; model radii/AI untouched; no Android FPS claim"};File.WriteAllText(Path.Combine(output,"result.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }finally{Call(creatures,"Dispose");UnityEngine.Object.DestroyImmediate(bg);UnityEngine.Object.DestroyImmediate(mat);UnityEngine.Object.DestroyImmediate(go);}
 }
}
