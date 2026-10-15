using EchoSphere.Presentation;
static class GridTrackChecks
{
 public static void Run(){var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-grid-tracks.json")));var sheet=new VisualCss(VisualJs.Obj("rules",VisualJs.Arr(),"keyframes",VisualJs.Obj()));var count=0;
  foreach(var fixture in fixtures.Items){var node=new VisualCssNode{Tag="div",Width=fixture["width"].Number,Height=fixture["height"].Number};node.Style["grid-template-columns"]=fixture["template"].Text;node.Style["gap"]="5px";
   foreach(var side in new[]{"top","right","bottom","left"}){node.Style["padding-"+side]="7px";node.Style["border-"+side+"-width"]="1px";}
   foreach(var cell in fixture["cells"].Items){var child=new VisualCssNode{Tag="div",Parent=node,Height=20};child.Style["height"]="20px";node.Children.Add(child);}
   var actual=VisualCssGrid.Compute(sheet,node);for(var index=0;index<actual.Cells.Count;index++){var cell=actual.Cells[index];var expected=fixture["cells"][index];if(new[]{cell.X-expected["x"].Number,cell.Y-expected["y"].Number,cell.Width-expected["width"].Number,cell.Height-expected["height"].Number}.Any(d=>Math.Abs(d)>.04))throw new InvalidOperationException(fixture["template"].Text+" @"+fixture["width"].Text+" cell "+index+" differs from browser grid tracks");count++;}
  }Console.WriteLine($"Browser authored grid fractions/minimums → native tracks: PASS ({count} cells).");
 }
}
