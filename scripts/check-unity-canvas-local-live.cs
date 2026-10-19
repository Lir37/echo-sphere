// Paused actual saved run; freezes only render noise/time and restores all bindings.
if(!UnityEditor.EditorApplication.isPlaying||!UnityEditor.EditorApplication.isPaused)throw new System.Exception("Paused Play Mode required");
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var r=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();r.GetType().GetMethod("UpdateSourceRenderState",f).Invoke(r,null);
var state=(EchoSphere.Presentation.V)r.GetType().GetField("_sourceRenderState",f).GetValue(r);
var c=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceCanvasSurface>();var t=c.GetType();var field=t.GetField("_geometryCache",f);var original=field.GetValue(c);var ct=original.GetType();var mode=t.GetField("_retainLocalContours",f);var previous=mode.GetValue(c);
var scope=(EchoSphere.Presentation.V)t.Assembly.GetType("EchoSphere.Presentation.Visual_renderer").GetField("Scope").GetValue(null);
var random=EchoSphere.Presentation.VisualJs.MathObject["random"];var now=EchoSphere.Presentation.VisualJs.PerformanceObject["now"];
var rows=new System.Collections.Generic.List<object>();UnityEngine.Color32[] reference=null;double mean=0;int maximum=0,different=0;
void Draw(){var w=UnityEngine.Screen.width;var h=UnityEngine.Screen.height;c.Begin(new UnityEngine.Vector2(w*.5f,h*.5f));c.Canvas.Set("canvas",EchoSphere.Presentation.VisualJs.Obj("nativeViewport",EchoSphere.Presentation.VisualJs.Obj("cssWidth",w,"cssHeight",h,"dpr",1)));scope["render"].Call(c.Canvas.Value,state,w,h);c.End();}
UnityEngine.Color32[] Pixels(){var rt=(UnityEngine.RenderTexture)t.GetField("_target",f).GetValue(c);var old=UnityEngine.RenderTexture.active;var image=new UnityEngine.Texture2D(rt.width,rt.height,UnityEngine.TextureFormat.RGBA32,false,true);try{UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,rt.width,rt.height),0,0);image.Apply();return image.GetPixels32();}finally{UnityEngine.RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}}
try{
 EchoSphere.Presentation.VisualJs.MathObject["random"]=EchoSphere.Presentation.VisualJs.Fn(a=>.5d);EchoSphere.Presentation.VisualJs.PerformanceObject["now"]=EchoSphere.Presentation.VisualJs.Fn(a=>0d);
 foreach(bool enabled in new[]{false,true}){
  mode.SetValue(c,enabled);var cache=System.Activator.CreateInstance(ct,new object[]{512,262144,786432,262144});field.SetValue(c,cache);Draw();var watch=System.Diagnostics.Stopwatch.StartNew();Draw();watch.Stop();var pixels=Pixels();
  if(reference==null)reference=pixels;else{if(reference.Length!=pixels.Length)throw new System.Exception("Size changed");long sum=0;for(int i=0;i<pixels.Length;i++){var a=reference[i];var b=pixels[i];int d0=System.Math.Abs(a.r-b.r),d1=System.Math.Abs(a.g-b.g),d2=System.Math.Abs(a.b-b.b),d3=System.Math.Abs(a.a-b.a);var max=System.Math.Max(System.Math.Max(d0,d1),System.Math.Max(d2,d3));if(max>0)different++;maximum=System.Math.Max(maximum,max);sum+=d0+d1+d2+d3;}mean=(double)sum/(pixels.Length*4);}
  rows.Add(new{enabled,ms=watch.Elapsed.TotalMilliseconds,hits=ct.GetProperty("Hits").GetValue(cache),misses=ct.GetProperty("Misses").GetValue(cache),entries=ct.GetProperty("Count").GetValue(cache)});
 }
 if(maximum>1||mean>.001)throw new System.Exception("Frozen saved-run raster parity exceeded one byte / .001 mean");
 var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{rows,meanByteError=mean,maxByteError=maximum,differentPixels=different,scope="Frozen actual saved-run Canvas, not active gameplay FPS"},Newtonsoft.Json.Formatting.Indented);System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/canvas-local-live.json"),json);UnityEditor.SessionState.SetString("EchoCanvasLocalLive",json);return json;
}finally{field.SetValue(c,original);mode.SetValue(c,previous);EchoSphere.Presentation.VisualJs.MathObject["random"]=random;EchoSphere.Presentation.VisualJs.PerformanceObject["now"]=now;}
