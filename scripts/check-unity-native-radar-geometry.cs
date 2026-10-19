// Independent authored RadarHud tree oracle; no gameplay or save state changes.
var parent=new UnityEngine.UIElements.VisualElement();var view=new EchoSphere.Presentation.NativeHudRadarView(parent);
var module=(EchoSphere.Presentation.V)typeof(EchoSphere.Presentation.NativeHudRadarView).Assembly.GetType("EchoSphere.Presentation.Visual_App").GetField("Scope").GetValue(null);
int checks=0;
void Assert(bool ok,string message){checks++;if(!ok)throw new System.Exception(message);}
System.Collections.Generic.IEnumerable<EchoSphere.Presentation.VisualDom.Node> Nodes(EchoSphere.Presentation.V value){if(value.Raw is EchoSphere.Presentation.VisualDom.Node node){yield return node;foreach(var child in node.Children)foreach(var nested in Nodes(child))yield return nested;}else if(value.Raw is System.Collections.Generic.List<EchoSphere.Presentation.V> items)foreach(var child in items)foreach(var nested in Nodes(child))yield return nested;}
double Percent(EchoSphere.Presentation.V value)=>double.Parse(value.Text.TrimEnd('%'),System.Globalization.CultureInfo.InvariantCulture);
try{
 for(int pass=0;pass<24;pass++){
  var enemies=new System.Collections.Generic.List<EchoSphere.Presentation.NativeHudRadarView.Enemy>();var pois=new System.Collections.Generic.List<EchoSphere.Presentation.NativeHudRadarView.Poi>();var sourceEnemies=new System.Collections.Generic.List<EchoSphere.Presentation.V>();var sourcePois=new System.Collections.Generic.List<EchoSphere.Presentation.V>();double px=pass*17.13,py=-pass*3.47;
  for(int i=0;i<90;i++){double x=px+(i*173+pass*31)%3600-1800,y=py+(i*97-pass*17)%3200-1600;bool alive=i%7!=pass%7,boss=i%11==0,elite=i%5==0;enemies.Add(new EchoSphere.Presentation.NativeHudRadarView.Enemy(x,y,alive,boss,elite));sourceEnemies.Add(EchoSphere.Presentation.VisualJs.Obj("pos",EchoSphere.Presentation.VisualJs.Obj("x",x,"y",y),"hp",alive?1:0,"type","fixture","isBoss",boss,"isElite",elite));}
  var types=new[]{"boss_trace","elite_nest","rupture","cache"};
  for(int i=0;i<32;i++){double x=px+(i*137+pass*19)%2200-1100,y=py+(i*199-pass*13)%1800-900;bool alive=i%4!=pass%4;var kind=types[i%4];pois.Add(new EchoSphere.Presentation.NativeHudRadarView.Poi(x,y,alive,kind));sourcePois.Add(EchoSphere.Presentation.VisualJs.Obj("id",i,"pos",EchoSphere.Presentation.VisualJs.Obj("x",x,"y",y),"alive",alive,"type",kind));}
  var st=EchoSphere.Presentation.VisualJs.Obj("player",EchoSphere.Presentation.VisualJs.Obj("pos",EchoSphere.Presentation.VisualJs.Obj("x",px,"y",py)),"enemies",EchoSphere.Presentation.VisualJs.Arr(sourceEnemies.ToArray()),"region",EchoSphere.Presentation.VisualJs.Obj("pois",EchoSphere.Presentation.VisualJs.Arr(sourcePois.ToArray())));
  var tree=module["RadarHud"].Call(EchoSphere.Presentation.VisualJs.Obj("st",st));var expected=Nodes(tree).Where(n=>(n.Properties["className"].Text??"").Split(' ').Contains("es-radar-dot")||n.Properties["style"]["borderBottom"].Truth||n.Properties["style"]["width"].Number==7).ToArray();
  view.Bind(px,py,enemies,pois);Assert(view.VisibleEnemies==40&&view.VisiblePois==12,"Alive filtering/caps");Assert(expected.Length==52,"Source dot count");
  for(int i=0;i<expected.Length;i++){
   bool enemy=i<40;var actual=UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(view.Root,enemy?"radar-enemy-"+i:"radar-poi-"+(i-40));var node=expected[i];var style=node.Properties["style"];
   Assert(System.Math.Abs(actual.style.left.value.value-Percent(style["left"]))<.00001,"Authored x position");Assert(System.Math.Abs(actual.style.top.value.value-Percent(style["top"]))<.00001,"Authored y position");
   float size=enemy?(node.Properties["className"].Text.Contains("boss")?5:node.Properties["className"].Text.Contains("elite")?3:2):style["borderBottom"].Truth?10:7;Assert(actual.style.width.value.value==size,"Dot size/type precedence");
   if(!enemy){var transform=style["transform"].Text;var match=System.Text.RegularExpressions.Regex.Match(transform,@"rotate\(([-+\d.Ee]+)(rad|deg)\)");var angle=double.Parse(match.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);if(match.Groups[2].Value=="deg")angle*=System.Math.PI/180;Assert(System.Math.Abs(actual.style.rotate.value.angle.value-angle)<.000001,"POI orientation");
    var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var dot=((System.Array)view.GetType().GetField("_pois",f).GetValue(view)).GetValue(i-40);UnityEngine.Color actualColor;
    if(style["borderBottom"].Truth){var arrow=dot.GetType().GetField("Arrow",f).GetValue(dot);actualColor=(UnityEngine.Color)arrow.GetType().GetField("_color",f).GetValue(arrow);}else{var surface=dot.GetType().GetField("Surface",f).GetValue(dot);actualColor=((EchoSphere.Presentation.NativeMenuBrush.Paint)surface.GetType().GetField("_paint",f).GetValue(surface)).Fill;}
    var hex=System.Text.RegularExpressions.Regex.Match(style["borderBottom"].Truth?style["borderBottom"].Text:style["background"].Text,@"#[0-9a-fA-F]{6}").Value;UnityEngine.ColorUtility.TryParseHtmlString(hex,out var expectedColor);Assert(actualColor==expectedColor,"Authored POI color");
   }
  }
 }
 view.Bind(0,0,System.Array.Empty<EchoSphere.Presentation.NativeHudRadarView.Enemy>(),System.Array.Empty<EchoSphere.Presentation.NativeHudRadarView.Poi>());Assert(view.VisibleEnemies==0&&view.VisiblePois==0,"Empty frame");
 for(int i=0;i<40;i++)Assert(UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(view.Root,"radar-enemy-"+i).style.display.value==UnityEngine.UIElements.DisplayStyle.None,"Reused dot hidden");
 var result="PASS native radar / authored RadarHud: "+checks+" comparisons, 24 states, caps/filtering/positions/types/POI orientation/reuse";UnityEditor.SessionState.SetString("EchoNativeRadarGeometry",result);return result;
}finally{view.Dispose();}
