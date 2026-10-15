using EchoSphere.Presentation;
using System.IO.Compression;

static class PaintChecks
{
    public static void Run()
    {
        using var stream=new GZipStream(File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-paint-fixtures.json.gz")),CompressionMode.Decompress);using var reader=new StreamReader(stream);
        var fixtures=VisualJson.Parse(reader.ReadToEnd());var failures=new List<string>();var compared=0;var worst=0d;
        foreach(var fixture in fixtures.Items){
            var sheet=new VisualCss(VisualJs.Obj("rules",VisualJs.Arr(),"keyframes",VisualJs.Obj())) {Width=fixture["viewport"]["width"].Number,Height=fixture["viewport"]["height"].Number};
            var node=new VisualCssNode {Tag="div",X=fixture["x"].Number,Y=fixture["y"].Number,Width=fixture["width"].Number,Height=fixture["height"].Number};foreach(var key in ((Dictionary<string,V>)fixture["style"].Raw).Keys)node.Style[key]=fixture["style"][key].Text;
            var image=VisualCssPaint.Paint(sheet,node,1d);var expected=Convert.FromBase64String(fixture["pixels"].Text);var total=0d;var max=0d;var n=0;var bad=0;
            // Compare premultiplied channels: RGB in a fully transparent pixel is
            // undefined. Browser edge AA and Gaussian kernel discretization are
            // measured, not silently treated as exact pixel equality.
            for(var y=0;y<image.Height;y++)for(var x=0;x<image.Width;x++){
                var sx=(int)Math.Round(node.X+image.Left)+x;var sy=(int)Math.Round(node.Y+image.Top)+y;if(sx<0||sy<0||sx>=sheet.Width||sy>=sheet.Height)continue;
                var i=(y*image.Width+x)*4;var j=(sy*(int)sheet.Width+sx)*4;var aa=image.Pixels[i+3]/255d;var ba=expected[j+3]/255d;
                double error=Math.Abs(image.Pixels[i+3]-expected[j+3]);for(var c=0;c<3;c++)error=Math.Max(error,Math.Abs(image.Pixels[i+c]*aa-expected[j+c]*ba));total+=error;max=Math.Max(max,error);if(error>8d)bad++;n++;
            }
            var mean=total/Math.Max(1,n);worst=Math.Max(worst,mean);compared+=n;
            if(mean>1.5d||bad>n*.04d){
                failures.Add(fixture["label"].Text+": mean="+mean.ToString("F3")+", max="+max.ToString("F2")+", >8/255="+bad+"/"+n);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-paint-"+failures.Count+".json"),System.Text.Json.JsonSerializer.Serialize(new {label=fixture["label"].Text,width=image.Width,height=image.Height,x=node.X+image.Left,y=node.Y+image.Top,actual=Convert.ToBase64String(image.Pixels),expected=fixture["pixels"].Text}));
            }
        }
        if(failures.Count>0){File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"failed-paint-comparisons.txt"),failures);throw new InvalidOperationException(failures.Count+" CSS raster differences: "+failures[0]);}
        Console.WriteLine($"Browser CSS brushes → native raster: PASS ({fixtures.Items.Count()} cases / {compared} pixels; maximum mean channel difference {worst:F3}/255).");
    }
}
