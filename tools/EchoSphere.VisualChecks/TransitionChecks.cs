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
  Timelines();
 }
 private static void Timelines()
 {
  var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-transition-timelines.json")));var count=0;
  foreach(var fixture in fixtures.Items){var sheet=new VisualCss(VisualJs.Obj("keyframes",VisualJs.Obj(),"rules",VisualJs.Arr(VisualJs.Obj("selector",".sample","conditions",VisualJs.Arr(),"declarations",fixture["declarations"])))){Width=390,Height=844};var node=new VisualCssNode{Tag="div",Properties=VisualJs.Obj("className","sample"),Width=240,Height=160};var motion=new VisualCssAnimation();
   void Frame(V values,double time){sheet.Apply(node);foreach(var p in (Dictionary<string,V>)values.Raw)node.Style[p.Key]=p.Value.Text;motion.Apply(sheet,node,time);}
   var child=new VisualCssNode{Tag="span",Parent=node};node.Children.Add(child);sheet.Apply(node);
   Frame(fixture["from"],0);foreach(var sample in fixture["samples"].Items){Frame(fixture["to"],sample["time"].Number);child.InheritAnimatedValues();if(!ColorEqual(VisualCssColor.Parse(sheet.Get(child,"color")),VisualCssColor.Parse(sample["inheritedColor"].Text)))throw new InvalidOperationException(fixture["label"].Text+" animated inherited color differs");foreach(var p in (Dictionary<string,V>)sample["style"].Raw){var actual=sheet.Get(node,p.Key);var equal=p.Key=="transform"?VisualCssTransform.Parse(sheet,node).Select((value,i)=>Math.Abs(value-sample["matrix"][i].Number)<.002).All(x=>x):p.Key.EndsWith("color",StringComparison.Ordinal)?ColorEqual(VisualCssColor.Parse(actual),VisualCssColor.Parse(p.Value.Text)):p.Key=="display"?actual==p.Value.Text:Math.Abs(double.Parse(actual,CultureInfo.InvariantCulture)-p.Value.Number)<.00002;
     if(!equal)throw new InvalidOperationException(fixture["label"].Text+" timeline @"+sample["time"].Text+" "+p.Key+": "+actual+" != "+p.Value.Text);
    }count++;}
  }Console.WriteLine($"Browser transition shorthand/longhand timelines → native animation host: PASS ({count} frames).");
 }
 // Chromium serializes legacy animated colors to 8-bit channels and alpha.
 // Compare premultiplied channels with a two-code-value serialization margin.
 private static bool ColorEqual(VisualCssColor a,VisualCssColor b)=>Math.Abs(a.R*a.A-b.R*b.A)<=2d/255d&&Math.Abs(a.G*a.A-b.G*b.A)<=2d/255d&&Math.Abs(a.B*a.A-b.B*b.A)<=2d/255d&&Math.Abs(a.A-b.A)<=2d/255d;
}
