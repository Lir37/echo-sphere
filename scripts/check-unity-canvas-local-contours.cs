// Real Unity tessellator; no scene, save or gameplay state is changed.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Edit Mode");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var go=new UnityEngine.GameObject("Canvas local contour check"){hideFlags=UnityEngine.HideFlags.HideAndDontSave};
var c=go.AddComponent<EchoSphere.Presentation.SourceCanvasSurface>();var type=c.GetType();
var cache=type.GetField("_geometryCache",flags).GetValue(c);var ct=cache.GetType();
var local=type.GetField("_retainLocalContours",flags);int checks=0;
void Assert(bool ok,string name){checks++;if(!ok)throw new System.Exception(name);}
void Call(string name,params double[] numbers){var args=new EchoSphere.Presentation.V[numbers.Length];for(int i=0;i<args.Length;i++)args[i]=new EchoSphere.Presentation.V(numbers[i]);c.Call(name,args);}
void Reset(){foreach(var name in new[]{"_vertices","_colors","_uv","_data","_triangles"})((System.Collections.IList)type.GetField(name,flags).GetValue(c)).Clear();Call("beginPath");Call("resetTransform");}
void Path(int kind){Call("moveTo",2,3);if(kind==0){Call("lineTo",33,7);Call("lineTo",14,29);}else if(kind==1){Call("bezierCurveTo",8,25,26,32,33,7);Call("lineTo",14,29);}else{Call("arc",14,17,11,0,6.283185307179586);}Call("closePath");}
string Digest(){var b=new System.Text.StringBuilder();foreach(var name in new[]{"_vertices","_colors","_uv","_data","_triangles"})foreach(var v in (System.Collections.IEnumerable)type.GetField(name,flags).GetValue(c))b.Append(v is UnityEngine.Vector3 a?a.ToString("R"):v is UnityEngine.Vector4 d?d.ToString("R"):v is UnityEngine.Color e?e.ToString("R"):v.ToString()).Append(';');return b.ToString();}
try{
 for(int kind=0;kind<3;kind++)for(int stroke=0;stroke<2;stroke++){
  Reset();local.SetValue(c,false);Path(kind);Call(stroke==0?"fill":"stroke");var expected=Digest();
  Reset();local.SetValue(c,true);Path(kind);Call(stroke==0?"fill":"stroke");Assert(Digest()==expected,"Identity mesh parity");
 }
 var rows=new System.Collections.Generic.List<object>();
 foreach(bool enabled in new[]{false,true}){
  local.SetValue(c,enabled);ct.GetMethod("Clear").Invoke(cache,null);var hits=(long)ct.GetProperty("Hits").GetValue(cache);var misses=(long)ct.GetProperty("Misses").GetValue(cache);var watch=System.Diagnostics.Stopwatch.StartNew();
  for(int i=0;i<80;i++){Reset();Call("translate",178.123+i*.713,241.91-i*.329);Call("rotate",i*.079);Path(1);Call("stroke");}
  watch.Stop();var h=(long)ct.GetProperty("Hits").GetValue(cache)-hits;var m=(long)ct.GetProperty("Misses").GetValue(cache)-misses;
  if(enabled)Assert(h==79&&m==1,"Translated/rotated exact local cache reuse");rows.Add(new{enabled,hits=h,misses=m,ms=watch.Elapsed.TotalMilliseconds});
 }
 // Changing construction matrix or paint matrix must keep the old, exact path.
 for(int mode=0;mode<3;mode++){
  string expected=null;foreach(bool enabled in new[]{false,true}){Reset();local.SetValue(c,enabled);Call("translate",41.123,67.913);Path(1);if(mode==0){Call("translate",13.7,9.11);Call("lineTo",44,25);}else if(mode==1)Call("rotate",.733);else Call("scale",0,1);Call("stroke");var actual=Digest();if(expected==null)expected=actual;else Assert(actual==expected,"Mixed/paint/singular transform fallback "+mode);}
 }
 var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{checks,rows,scope="Real Unity tessellation microbenchmark; not gameplay FPS"});UnityEditor.SessionState.SetString("EchoCanvasLocalContours",json);return json;
}finally{UnityEngine.Object.DestroyImmediate(go);}
