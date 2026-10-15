using EchoSphere.Presentation;

internal static class PresentationAudit
{
 private static V Tree(V value){if(value.Raw is Dictionary<string,V>&&!value["tag"].Undefined)return VisualDom.Element(value["tag"].Text,value["properties"],value["children"].Items.Select(Tree).ToArray());if(value.Raw is List<V>)return VisualJs.Arr(value.Items.Select(Tree).ToArray());return value;}
 public static void Run()
 {
  var ui=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-ui-fixtures.json")));
  var sheet=new VisualCss(VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiStyles.json"))));var used=new SortedDictionary<string,HashSet<string>>();
  foreach(var width in new[]{360d,390d,1280d})foreach(var fixture in ui.Items){sheet.Width=width;sheet.Height=844;var root=VisualCssNode.Build(Tree(fixture["tree"]));sheet.Apply(root);
   void Visit(VisualCssNode node){foreach(var p in node.Style.Where(p=>!p.Key.StartsWith("--"))){if(!used.TryGetValue(p.Key,out var values)){values=new HashSet<string>();used.Add(p.Key,values);}values.Add(p.Value);}foreach(var child in node.Children)Visit(child);}Visit(root);
  }
  foreach(var property in used)Console.WriteLine(property.Key+": "+string.Join(" | ",property.Value.OrderBy(x=>x)));
 }
}
