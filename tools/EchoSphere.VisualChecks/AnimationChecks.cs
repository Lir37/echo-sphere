using EchoSphere.Presentation;
using System.Globalization;
using System.Text.RegularExpressions;

static class AnimationChecks
{
    public static void Run()
    {
        var styles=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiStyles.json")));
        var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-animation-fixtures.json")));var count=0;var failures=new List<string>();
        foreach(var fixture in fixtures.Items){
            var data=VisualJs.Obj("keyframes",styles["keyframes"],"rules",VisualJs.Arr(VisualJs.Obj("selector",".sample","conditions",VisualJs.Arr(),"declarations",fixture["declarations"])));
            var sheet=new VisualCss(data){Width=390,Height=844};var motion=new VisualCssAnimation();
            var node=new VisualCssNode {Tag="div",Properties=VisualJs.Obj("className","sample"),Width=fixture["width"].Number,Height=fixture["height"].Number};
            foreach(var sample in fixture["samples"].Items){
                sheet.Apply(node);foreach(var property in new[]{"transform","opacity","filter"})node.Style[property]=fixture["base"][property].Text;
                // Keep function lists from the authored declaration: computed
                // matrices alone discard complete revolutions during animation.
                foreach(var d in fixture["declarations"].Items)if(d["property"].Text=="transform"||d["property"].Text=="filter")node.Style[d["property"].Text]=sheet.Variables(d["value"].Text,node.Style);
                motion.Apply(sheet,node,sample["time"].Number);
                var matrix=VisualCssTransform.Parse(sheet,node);var expected=sample["matrix"].Items.ToArray();
                for(var i=0;i<16;i++)if(Math.Abs(matrix[i]-expected[i].Number)>.002d){failures.Add(fixture["label"].Text+" @"+sample["time"].Text+" transform["+i+"]: "+matrix[i]+" != "+expected[i].Text);break;}
                if(Math.Abs(double.Parse(sheet.Get(node,"opacity","1"),CultureInfo.InvariantCulture)-sample["opacity"].Number)>.00002d)failures.Add(fixture["label"].Text+" @"+sample["time"].Text+" opacity: "+sheet.Get(node,"opacity")+" != "+sample["opacity"].Text);
                var actualFilter=sheet.Get(node,"filter","none");var wantedFilter=sample["filter"].Text;
                if(actualFilter=="none"&&Regex.IsMatch(wantedFilter,@"^(?:brightness|contrast|saturate)\(1\)$"))actualFilter=wantedFilter;
                if(actualFilter!=wantedFilter){var a=Regex.Matches(actualFilter,@"[-\d.]+").Select(x=>double.Parse(x.Value,CultureInfo.InvariantCulture)).ToArray();var b=Regex.Matches(wantedFilter,@"[-\d.]+").Select(x=>double.Parse(x.Value,CultureInfo.InvariantCulture)).ToArray();if(a.Length!=b.Length||a.Where((x,i)=>Math.Abs(x-b[i])>.00002d).Any())failures.Add(fixture["label"].Text+" @"+sample["time"].Text+" filter: "+actualFilter+" != "+wantedFilter);}
                count++;
            }
        }
        if(failures.Count>0){File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"failed-animation-comparisons.txt"),failures);throw new InvalidOperationException(failures.Count+" browser keyframe differences: "+failures[0]);}
        Console.WriteLine($"Browser CSS keyframes → native timelines: PASS ({count} samples).");
    }
}
