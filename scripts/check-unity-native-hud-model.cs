// Display-only RU/EN oracle. Run in Edit Mode with no live source hook tree.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Edit Mode required");
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
var hooks=(System.Collections.IDictionary)typeof(EchoSphere.Presentation.VisualDom).GetField("States",flags).GetValue(null);
if(hooks.Count!=0)throw new System.Exception("Live source hooks must not be disturbed");
var assembly=typeof(EchoSphere.Presentation.NativeHudView).Assembly;
var app=(EchoSphere.Presentation.V)assembly.GetType("EchoSphere.Presentation.Visual_App").GetField("Scope").GetValue(null);
var project=typeof(EchoSphere.Runtime.EchoSphereRuntime).GetMethod("ProjectNativeHudState",flags);
var fixtures=Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../../tools/EchoSphere.VisualChecks/source-ui-fixtures.json"))).Where(x=>(string)x["name"]=="Hud").ToArray();
var view=new EchoSphere.Presentation.NativeHudView(new UnityEngine.UIElements.VisualElement(),()=>false,id=>null);int checks=0,cases=0;
void Require(bool ok,string message){checks++;if(!ok)throw new System.Exception(message);}
System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken> Nodes(Newtonsoft.Json.Linq.JToken v){if(v is Newtonsoft.Json.Linq.JObject){yield return v;foreach(var c in v["children"]??new Newtonsoft.Json.Linq.JArray())foreach(var n in Nodes(c))yield return n;}else if(v is Newtonsoft.Json.Linq.JArray)foreach(var c in v)foreach(var n in Nodes(c))yield return n;}
string Text(Newtonsoft.Json.Linq.JToken v)=>v is Newtonsoft.Json.Linq.JObject?string.Concat((v["children"]??new Newtonsoft.Json.Linq.JArray()).Select(Text)):v is Newtonsoft.Json.Linq.JArray?string.Concat(v.Select(Text)):v.ToString();
Newtonsoft.Json.Linq.JToken Freeze(EchoSphere.Presentation.V v){if(v.Raw is EchoSphere.Presentation.VisualDom.Node n)return new Newtonsoft.Json.Linq.JObject{["tag"]=n.Tag,["properties"]=Newtonsoft.Json.Linq.JToken.Parse(EchoSphere.Presentation.VisualJson.Stringify(n.Properties)),["children"]=new Newtonsoft.Json.Linq.JArray(n.Children.Select(Freeze))};if(v.Raw is System.Collections.Generic.List<EchoSphere.Presentation.V>)return new Newtonsoft.Json.Linq.JArray(v.Items.Select(Freeze));return new Newtonsoft.Json.Linq.JValue(v.Text);}
string Label(string name)=>System.Text.RegularExpressions.Regex.Replace(UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Label>(view.Root,name).text,"<[^>]+>","");
try{
 foreach(var fixture in fixtures)for(int variant=0;variant<13;variant++){
  var scene=EchoSphere.Presentation.VisualJson.Parse(fixture["props"]["st"].ToString());var lang=(string)fixture["props"]["lang"];bool ru=lang=="ru";
  if(variant>0){var player=scene["player"];player["hp"]=variant%3==0?-2.4:37.2+variant;player["maxHp"]=100.3;player["xp"]=variant*9.7;player["xpToNext"]=73;player["resonanceCharge"]=variant*11.6-12;player["level"]=variant+1;scene["time"]=variant*73.91;scene["wave"]=variant*2;scene["bossActive"]=variant%2==0;
   if(variant%4==0)scene["region"]=EchoSphere.Presentation.VisualJs.Null;else{var region=scene["region"];region["mode"]=variant%2==0?"endless":"stabilization";if(variant%3==0)region["pocketId"]="unknown-fixture-pocket";}
   var network=scene["nativeNetwork"];network["dominantFormation"]=EchoSphere.Presentation.VisualJs.Obj("type",variant%2==0?"line":"triangle","active",variant%3!=0);network["secondaryFormation"]=variant%2==0?EchoSphere.Presentation.VisualJs.Obj("type","cluster","active",true):EchoSphere.Presentation.VisualJs.Null;
  }
  var state=(EchoSphere.Presentation.NativeHudView.State)project.Invoke(null,new object[]{scene,ru});view.Bind(state);
  Newtonsoft.Json.Linq.JToken tree=fixture["tree"];
  if(variant>0){var translations=EchoSphere.Presentation.VisualJson.Parse(fixture["translations"].ToString());var props=EchoSphere.Presentation.VisualJs.Obj("st",scene,"lang",lang,"t",EchoSphere.Presentation.VisualJs.Fn(args=>translations[args[0]]));EchoSphere.Presentation.VisualDom.BeginFrame();try{tree=Freeze(EchoSphere.Presentation.VisualDom.Component("Hud",app["Hud"],props));EchoSphere.Presentation.VisualDom.EndFrame();}finally{EchoSphere.Presentation.VisualDom.Reset();}}
  var nodes=Nodes(tree).ToArray();Newtonsoft.Json.Linq.JToken Row(string cls)=>nodes.First(n=>((string)n["properties"]?["className"]??"").Split(' ').Contains(cls));
  void Match(string name,string cls,bool upper=false){var expected=Text(Row(cls));if(upper)expected=expected.ToUpperInvariant();Require(Label(name)==expected,lang+" variant"+variant+" "+name+" got="+Label(name)+" expected="+expected);}
  Match("hud-level","es-hud-title",true);Match("hud-hp","es-hud-value");Match("hud-time","es-time-hud-value");
  var counters=Row("es-hud-meta-grid")["children"].OfType<Newtonsoft.Json.Linq.JObject>().ToArray();Require(Label("hud-spheres")==Text(counters[0]),"Sphere counter");Require(Label("hud-wave")==Text(counters[1]),"Wave counter");
  foreach(var item in new[]{("es-progress",state.XpPercent),("es-hp-progress",state.HpPercent)}){var bar=Row(item.Item1)["children"].OfType<Newtonsoft.Json.Linq.JObject>().First();var expected=double.Parse(((string)bar["properties"]["style"]["width"]).TrimEnd('%'),System.Globalization.CultureInfo.InvariantCulture);Require(System.Math.Abs(expected-item.Item2)<.0001,"Bar percentage");}
  var resonanceBar=Row("mt-0.5")["children"].OfType<Newtonsoft.Json.Linq.JObject>().First();var resonanceWidth=double.Parse(((string)resonanceBar["properties"]["style"]["width"]).TrimEnd('%') ,System.Globalization.CultureInfo.InvariantCulture);Require(System.Math.Abs(resonanceWidth-state.ResonancePercent)<.0001,"Resonance percentage");Require(state.ResonanceReady==((string)resonanceBar["properties"]["style"]["background"]=="#ffb84d"),"Resonance ready color"); var resonanceHeader=nodes.First(n=>((string)n["properties"]?["className"]??"").Contains("tracking-[0.12em]"));Require(Label("hud-resonance-value")==Text(resonanceHeader["children"].OfType<Newtonsoft.Json.Linq.JObject>().Last()),"Resonance counter"); Require(state.HasRegion==scene["region"].Truth,"Region visibility");if(state.HasRegion){Match("hud-region-title","es-hud-region-title");Match("hud-region-phase","es-hud-region-phase");Match("hud-region-pocket","es-hud-region-pocket");}
  Require(state.Boss==scene["bossActive"].Truth,"Boss visibility");if(state.Boss)Match("hud-boss","es-boss-telemetry",true);
  foreach(var n in nodes.Where(n=>n["properties"]?["data-formation-slot"]!=null)){int slot=(string)n["properties"]["data-formation-slot"]=="dominant"?0:1;Require(Label("hud-formation-name-"+slot)==Text(n),"Formation name");}
  cases++;
 }
 var result=Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS native HUD display model/text vs original source",cases,checks,frozenBrowserCases=2,generatedSourceCases=cases-2,scope="RU/EN, rounding, percentages, counters, timer, formations, boss, region modes/pocket fallback; synthetic display snapshots only; no gameplay state/preferences changed"});UnityEditor.SessionState.SetString("EchoNativeHudModel",result);System.IO.File.WriteAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/native-hud-model-evidence.json"),result);return result;
}finally{view.Dispose();EchoSphere.Presentation.VisualDom.Reset();}
