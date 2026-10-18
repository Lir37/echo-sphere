// Diagnostic only: run with Editor Play Mode paused. Restores cache and render bindings in finally.
if(!UnityEditor.EditorApplication.isPlaying||!UnityEditor.EditorApplication.isPaused)throw new System.Exception("Pause Play Mode before this comparison");
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var r=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Runtime.EchoSphereRuntime>();
r.GetType().GetMethod("UpdateSourceRenderState",f).Invoke(r,null);
var state=(EchoSphere.Presentation.V)r.GetType().GetField("_sourceRenderState",f).GetValue(r);
var c=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceCanvasSurface>();
var field=c.GetType().GetField("_geometryCache",f);var original=field.GetValue(c);var type=original.GetType();
var module=c.GetType().Assembly.GetType("EchoSphere.Presentation.Visual_renderer");
var scope=(EchoSphere.Presentation.V)module.GetField("Scope",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).GetValue(null);
var random=EchoSphere.Presentation.VisualJs.MathObject["random"];var now=EchoSphere.Presentation.VisualJs.PerformanceObject["now"];
var rows=new System.Collections.Generic.List<object>();string expected=null;int checks=0;
string Digest(){var builder=new System.Text.StringBuilder();foreach(var name in new[]{"_vertices","_colors","_uv","_data","_triangles"}){var list=(System.Collections.IEnumerable)c.GetType().GetField(name,f).GetValue(c);foreach(var v in list)builder.Append(v is UnityEngine.Vector3 a? a.ToString("R",System.Globalization.CultureInfo.InvariantCulture):v is UnityEngine.Vector4 b?b.ToString("R",System.Globalization.CultureInfo.InvariantCulture):v is UnityEngine.Color d?d.ToString("R",System.Globalization.CultureInfo.InvariantCulture):v.ToString()).Append(';');}using(var hash=System.Security.Cryptography.SHA256.Create())return System.BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(builder.ToString())));}
void Draw(){var w=UnityEngine.Mathf.Min(1280f,UnityEngine.Screen.width);var h=UnityEngine.Mathf.Min(800f,UnityEngine.Screen.height);c.SetViewport(w,h,1);c.Begin(new UnityEngine.Vector2(w*.5f,h*.5f));c.Canvas.Set("canvas",EchoSphere.Presentation.VisualJs.Obj("nativeViewport",EchoSphere.Presentation.VisualJs.Obj("cssWidth",w,"cssHeight",h,"dpr",1)));scope["render"].Call(c.Canvas.Value,state,w,h);c.End();}
try{
 EchoSphere.Presentation.VisualJs.MathObject["random"]=EchoSphere.Presentation.VisualJs.Fn(a=>.5d);EchoSphere.Presentation.VisualJs.PerformanceObject["now"]=EchoSphere.Presentation.VisualJs.Fn(a=>0d);
 foreach(var capacity in new[]{0,512,2048}){
  var cache=System.Activator.CreateInstance(type,new object[]{capacity==0?512:capacity,262144,786432,262144});type.GetField("Enabled",f).SetValue(cache,capacity!=0);field.SetValue(c,cache);
  Draw();var watch=System.Diagnostics.Stopwatch.StartNew();Draw();watch.Stop();
  var digest=Digest();if(expected==null)expected=digest;else if(expected!=digest)throw new System.Exception("Canvas buffer mismatch");checks++;
  rows.Add(new{capacity,ms=watch.Elapsed.TotalMilliseconds,hits=type.GetProperty("Hits").GetValue(cache),misses=type.GetProperty("Misses").GetValue(cache),entries=type.GetProperty("Count").GetValue(cache)});
 }
}finally{field.SetValue(c,original);EchoSphere.Presentation.VisualJs.MathObject["random"]=random;EchoSphere.Presentation.VisualJs.PerformanceObject["now"]=now;}
var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{checks,rows,scope="Frozen current render state, exact mesh buffers; full Canvas generation, not FPS"});UnityEditor.SessionState.SetString("EchoCanvasBenchmark",json);return json;
