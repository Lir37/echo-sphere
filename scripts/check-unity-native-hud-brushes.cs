// Native shader paints vs the existing source CSS painter, before UI transforms.
// Run export-unity-gameplay-ui-reference.cs once on the paused saved run first.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var view=new EchoSphere.Presentation.NativeHudView(new UnityEngine.UIElements.VisualElement(),()=>false,id=>null);
var surfaceType=typeof(EchoSphere.Presentation.NativeHudView).Assembly.GetType("EchoSphere.Presentation.NativeHudSurface");
var paints=new System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.NativeMenuBrush.Paint>();
foreach(var owner in new object[]{view,view.Radar})foreach(var surface in (System.Collections.IEnumerable)owner.GetType().GetField("_surfaces",flags).GetValue(owner)){
 var host=(UnityEngine.UIElements.VisualElement)surfaceType.GetField("_host",flags).GetValue(surface);paints[host.name]=(EchoSphere.Presentation.NativeMenuBrush.Paint)surfaceType.GetField("_paint",flags).GetValue(surface);
}
var reference=Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/gameplay-ui-reference.json")));
Newtonsoft.Json.Linq.JToken Row(string token)=>reference.First(r=>((string)r["classes"]??"").Split(' ').Contains(token));
Newtonsoft.Json.Linq.JToken Child(Newtonsoft.Json.Linq.JToken row)=>reference.First(r=>(int)r["parent"]==(int)row["id"]&&(string)r["tag"]=="span");
var cases=new System.Collections.Generic.List<(string Name,EchoSphere.Presentation.NativeMenuBrush.Paint Paint,EchoSphere.Presentation.VisualCssNode Node)>();
void Add(string name,string host,Newtonsoft.Json.Linq.JToken row,double width,double height){var node=new EchoSphere.Presentation.VisualCssNode{Tag="div",Width=width,Height=height};foreach(var p in ((Newtonsoft.Json.Linq.JObject)row["style"]).Properties())node.Style[p.Name]=(string)p.Value;cases.Add((name,paints[host],node));}
Add("panel-left","hud-left",Row("es-top-left"),300,140);Add("panel-right","hud-right",Row("es-top-right"),300,140);
Add("xp-track","hud-xp-bar",Row("es-progress"),278,5);Add("xp-horizontal","hud-xp-bar-fill",Child(Row("es-progress")),120,3);
Add("hp-track","hud-hp-bar",Row("es-hp-progress"),278,4);Add("hp-horizontal","hud-hp-bar-fill",Child(Row("es-hp-progress")),120,2);
Add("radar","native-hud-radar",Row("es-radar"),62,62);Add("ring1","radar-ring-0",Row("es-radar-ring-1"),38,38);Add("ring2","radar-ring-1",Row("es-radar-ring-2"),16,16);
Add("enemy-normal","radar-enemy-0",Row("es-radar-dot"),2,2);Add("player","radar-player",Row("es-radar-player"),4,4);
var enemyPaints=(EchoSphere.Presentation.NativeMenuBrush.Paint[])view.Radar.GetType().GetField("_enemyPaint",flags).GetValue(view.Radar);
foreach(var item in new[]{("elite",1,3),("boss",2,5)}){var node=new EchoSphere.Presentation.VisualCssNode {Tag="span",Width=item.Item3,Height=item.Item3,Properties=EchoSphere.Presentation.VisualJs.Obj("className","es-radar-dot "+item.Item1)};cases.Add((item.Item1,enemyPaints[item.Item2],node));}
var sheet=new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse(UnityEngine.Resources.Load<UnityEngine.TextAsset>("EchoSphere/SourceUiStyles").text)){Width=1260,Height=542};
foreach(var item in cases)if(item.Node.Style.Count==0)sheet.Apply(item.Node);
var brush=new EchoSphere.Presentation.NativeMenuBrush();var results=new System.Collections.Generic.List<object>();int index=0;var scales=new[]{1f,1.75f,3f};
UnityEditor.SessionState.SetString("EchoNativeHudBrushes","RUNNING");UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(string status){UnityEditor.EditorApplication.update-=tick;brush.Dispose();view.Dispose();var json=Newtonsoft.Json.JsonConvert.SerializeObject(new{status,results});UnityEditor.SessionState.SetString("EchoNativeHudBrushes",json);System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-hud-brush-evidence.json"),json);}
tick=()=>{
 UnityEngine.Texture2D read=null;var previous=UnityEngine.RenderTexture.active;
 try{
  var item=cases[index%cases.Count];var scale=scales[index/cases.Count];var expected=EchoSphere.Presentation.VisualCssPaint.Paint(sheet,item.Node,scale);brush.Render((float)item.Node.Width,(float)item.Node.Height,scale,item.Paint);var texture=brush.Texture;
  if(texture.width!=expected.Width||texture.height!=expected.Height)throw new System.Exception("Bounds mismatch "+item.Name+" "+texture.width+"x"+texture.height+" / "+expected.Width+"x"+expected.Height);
  read=new UnityEngine.Texture2D(texture.width,texture.height,UnityEngine.TextureFormat.RGBA32,false,false);UnityEngine.RenderTexture.active=texture;read.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);read.Apply();var pixels=read.GetPixels32();double sum=0,maximum=0;int raw=0,alpha=0;
  for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++){var a=pixels[(texture.height-1-y)*texture.width+x];var at=(y*texture.width+x)*4;var e=expected.Pixels;alpha=System.Math.Max(alpha,System.Math.Abs(e[at+3]-a.a));var differences=new[]{System.Math.Abs(e[at]*e[at+3]/255d-a.r*a.a/255d),System.Math.Abs(e[at+1]*e[at+3]/255d-a.g*a.a/255d),System.Math.Abs(e[at+2]*e[at+3]/255d-a.b*a.a/255d),System.Math.Abs(e[at+3]-a.a)};foreach(var d in differences){sum+=d;maximum=System.Math.Max(maximum,d);}if(e[at+3]>=128||a.a>=128)raw=System.Math.Max(raw,System.Math.Max(System.Math.Abs(e[at]-a.r),System.Math.Max(System.Math.Abs(e[at+1]-a.g),System.Math.Abs(e[at+2]-a.b))));}
  results.Add(new{item.Name,scale,maximum,mean=sum/(pixels.Length*4),raw,alpha});
  if(maximum>2||raw>1||alpha>1)throw new System.Exception("HUD shader/source mismatch "+item.Name+" at "+scale+": "+maximum+" / "+raw+" / "+alpha);
  UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(read);read=null;
  if(++index==cases.Count*scales.Length)Finish("PASS 39 native HUD GPU/source brush cases; triangle and complete UI parity are separate");
 }catch(System.Exception e){Finish("FAIL "+e);}
 finally{UnityEngine.RenderTexture.active=previous;if(read!=null)UnityEngine.Object.DestroyImmediate(read);}
};UnityEditor.EditorApplication.update+=tick;return "Started 39 native HUD GPU/source brush cases";
