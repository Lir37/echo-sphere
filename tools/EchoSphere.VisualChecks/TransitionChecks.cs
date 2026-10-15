using EchoSphere.Presentation;
using System.Globalization;
using System.Text.RegularExpressions;

static class TransitionChecks
{
 public static void Run()
 {
  var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-transition-fixtures.json")));var count=0;var failures=new List<string>();
  foreach(var fixture in fixtures.Items)foreach(var sample in fixture["samples"].Items){
   var property=fixture["property"].Text;var time=sample["time"].Number;
   // Web Animations serializes a none endpoint as transparent/identity lists.
   // Sample its mathematical limit for an equivalent native representation.
   var actual=VisualCssAnimation.Interpolate(fixture["from"].Text,fixture["to"].Text,VisualCssAnimation.Ease("ease",time==0?1e-12:time),property);var expected=sample["value"].Text;
   bool equal;
   if(property=="transform"){
    var sheet=new VisualCss(VisualJs.Obj("rules",VisualJs.Arr(),"keyframes",VisualJs.Obj())){Width=390,Height=844};var node=new VisualCssNode{Width=240,Height=160};node.Style["transform"]=actual;
    var matrix=VisualCssTransform.Parse(sheet,node);equal=sample["matrix"].Items.Select((v,i)=>Math.Abs(v.Number-matrix[i])<.002d).All(value=>value);
   }else if(property.EndsWith("color",StringComparison.Ordinal)){
    equal=ColorEqual(VisualCssColor.Parse(actual),VisualCssColor.Parse(expected));
   }else {
    var colors=new Regex(@"rgba?\([^)]*\)");var ac=colors.Matches(actual);var bc=colors.Matches(expected);
    var colorsEqual=ac.Count==bc.Count&&ac.Select((color,i)=>ColorEqual(VisualCssColor.Parse(color.Value),VisualCssColor.Parse(bc[i].Value))).All(value=>value);
    double[] Values(string value)=>Regex.Matches(colors.Replace(value,"color"),@"[-+]?(?:\d*\.\d+|\d+\.?\d*)(?:e[-+]?\d+)?").Select(m=>double.Parse(m.Value,CultureInfo.InvariantCulture)).ToArray();
    var a=Values(actual);var b=Values(expected);equal=actual==expected||colorsEqual&&a.Length==b.Length&&(a.Length>0||ac.Count>0)&&a.Select((v,i)=>Math.Abs(v-b[i])<.0021d).All(value=>value);
   }
   if(!equal)failures.Add(fixture["label"].Text+" "+property+" @"+sample["time"].Text+": "+actual+" != "+expected);count++;
  }
  if(failures.Count>0){File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"failed-transition-comparisons.txt"),failures);throw new InvalidOperationException(failures.Count+" browser transition differences: "+failures[0]);}
  Console.WriteLine($"Browser CSS transition interpolation → native C#: PASS ({count} samples).");
 }
 // Chromium serializes legacy animated colors to 8-bit channels and alpha.
 // Compare premultiplied channels with a two-code-value serialization margin.
 private static bool ColorEqual(VisualCssColor a,VisualCssColor b)=>Math.Abs(a.R*a.A-b.R*b.A)<=2d/255d&&Math.Abs(a.G*a.A-b.G*b.A)<=2d/255d&&Math.Abs(a.B*a.A-b.B*b.A)<=2d/255d&&Math.Abs(a.A-b.A)<=2d/255d;
}
