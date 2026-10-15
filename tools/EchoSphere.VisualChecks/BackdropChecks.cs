using EchoSphere.Presentation;
using System.IO.Compression;

internal static class BackdropChecks
{
    private sealed class Node { internal string Name;internal List<Node> Children=new();internal Node(string name,params Node[] children){Name=name;Children.AddRange(children);} }
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("Backdrop: "+message);}
    private static V Tree(V value){if(value.Raw is Dictionary<string,V>&&!value["tag"].Undefined)return VisualDom.Element(value["tag"].Text,value["properties"],value["children"].Items.Select(Tree).ToArray());if(value.Raw is List<V>)return VisualJs.Arr(value.Items.Select(Tree).ToArray());return value;}
    internal static void Run()
    {
        var stop=new Node("panel",new Node("own foreground"));var skipped=new Node("clipped prior backdrop",new Node("outside ROI"));
        var root=new Node("root",new Node("world"),skipped,new Node("ancestor",new Node("prior text"),stop,new Node("later sibling")),new Node("later root"));
        var prefix=VisualBackdrop.PaintPrefix(root,stop,node=>node.Children.Count,(node,index)=>node.Children[index],node=>node==skipped).ToArray();
        Require(string.Join("/",prefix.Select(item=>item.Node.Name))=="root/world/ancestor/prior text","paint prefix includes self, foreground, clipped or later content");
        Require(prefix[3].Parent.Name=="ancestor","paint prefix lost the stacking/group hierarchy");
        var random=new Random(72631);
        for(var index=0;index<5000;index++){
            var a=random.NextDouble()*20-10;var b=random.NextDouble()*20-10;var c=random.NextDouble()*20-10;var d=random.NextDouble()*20-10;
            if(index%10==0){a=0;b=0;}if(index%17==0){c=a;d=b;}
            var parts=VisualBackdrop.Decompose(a,b,c,d);var co=Math.Cos(parts.First);var si=Math.Sin(parts.First);var ct=Math.Cos(parts.Second);var st=Math.Sin(parts.Second);
            var actual=new[]{co*parts.X*ct-si*parts.Y*st,si*parts.X*ct+co*parts.Y*st,-co*parts.X*st-si*parts.Y*ct,-si*parts.X*st+co*parts.Y*ct};
            Require(actual.Zip(new[]{a,b,c,d}).All(pair=>Math.Abs(pair.First-pair.Second)<1e-10),"affine mapping lost shear, reflection or rotation");
        }
        var sheet=new VisualCss(VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiStyles.json"))));
        var ancestor=new VisualCssNode {Tag="div"};var node=new VisualCssNode {Tag="div",Parent=ancestor};
        foreach(var property in new[]{"filter","mask-image","clip-path","backdrop-filter","-webkit-backdrop-filter","opacity","mix-blend-mode","will-change"}){
            node.Style.Clear();node.Style[property]=property=="opacity"?".5":property=="mix-blend-mode"?"multiply":property=="will-change"?"transform, opacity":"blur(8px)";Require(VisualBackdrop.IsRoot(sheet,node),"missing backdrop root for "+property);
        }
        node.Style.Clear();node.Style["transform"]="rotate(15deg) scale(1.3)";node.Style["z-index"]="20";node.Style["position"]="fixed";Require(!VisualBackdrop.IsRoot(sheet,node),"transform/z-index/fixed incorrectly stop the backdrop");
        node.Style["backdrop-filter"]="blur(0px)";Require(VisualBackdrop.Sigma(sheet,node)==0&&VisualBackdrop.IsRoot(sheet,node),"blur(0) must retain backdrop root semantics");
        node.Style["backdrop-filter"]="none";Require(VisualBackdrop.Sigma(sheet,node)<0,"none must remove blur");
        node.Style.Remove("backdrop-filter");node.Style["-webkit-backdrop-filter"]="blur(12px)";Require(VisualBackdrop.Sigma(sheet,node)==12,"prefixed CSS fallback lost");
        var fixtures=VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-ui-fixtures.json")));var count=0;var maximum=0;var radii=new SortedSet<double>();
        foreach(var width in new[]{360d,390d,1280d})foreach(var fixture in fixtures.Items){sheet.Width=width;sheet.Height=844;var tree=VisualCssNode.Build(Tree(fixture["tree"]));sheet.Apply(tree);var perView=0;
            void Visit(VisualCssNode item){var sigma=VisualBackdrop.Sigma(sheet,item);if(sigma>=0){radii.Add(sigma);count++;perView++;}foreach(var child in item.Children)Visit(child);}Visit(tree);maximum=Math.Max(maximum,perView);
        }
        Require(count>0&&radii.SetEquals(new[]{4d,6d,8d,12d}),"authored backdrop declarations changed; audit the new filter operations: "+string.Join('/',radii));
        Console.WriteLine($"Backdrop composition policy: PASS (5000 affine mappings; prefix/groups; {count} authored surfaces across 228 views, max {maximum}/view; CSS radii {string.Join('/',radii)} px).");
        using var file=File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-backdrop-fixtures.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var reader=new StreamReader(gzip);
        var pixels=0;var worst=0d;var scenarios=0;
        foreach(var fixture in VisualJson.Parse(reader.ReadToEnd()).Items){
            var dpr=fixture["dpr"].Number;var sigma=fixture["sigma"].Number*dpr;var radius=(int)Math.Ceiling(4*sigma);var weights=Enumerable.Range(-radius,radius*2+1).Select(offset=>Math.Exp(-offset*(double)offset/(2*sigma*sigma))).ToArray();var total=weights.Sum();for(var i=0;i<weights.Length;i++)weights[i]/=total;
            var source=fixture["source"].Items.Select(value=>value.Number).ToArray();var expected=fixture["expected"].Items.Select(value=>value.Number).ToArray();var sourceWidth=(int)fixture["sourceWidth"].Number;var sourceHeight=(int)fixture["sourceHeight"].Number;
            var x=(int)fixture["x"].Number;var y=(int)fixture["y"].Number;var width=(int)fixture["width"].Number;var height=(int)fixture["height"].Number;var horizontal=new double[width*(height+radius*2)*4];
            // Same premultiplied separable Gaussian as the shader, with an 8-bit
            // intermediate target. The reference is independent Chromium pixels.
            for(var row=0;row<height+radius*2;row++)for(var column=0;column<width;column++)for(var channel=0;channel<4;channel++){
                int Mirror(int coordinate,int length){coordinate%=length*2;if(coordinate<0)coordinate+=length*2;return coordinate<length?coordinate:length*2-1-coordinate;}
                var value=0d;for(var offset=-radius;offset<=radius;offset++){var sx=x+Mirror(column+offset,width);var sy=y+Mirror(row-radius,height);if(sx>=0&&sx<sourceWidth&&sy>=0&&sy<sourceHeight){var at=(sy*sourceWidth+sx)*4;value+=source[at+channel]*(channel==3?1:source[at+3]/255)*weights[offset+radius];}}
                horizontal[(row*width+column)*4+channel]=Math.Round(value);
            }
            var error=0d;var max=0d;var location="";
            for(var row=0;row<height;row++)for(var column=0;column<width;column++){
                var filtered=new double[4];for(var channel=0;channel<4;channel++)for(var offset=-radius;offset<=radius;offset++)filtered[channel]+=horizontal[((row+radius+offset)*width+column)*4+channel]*weights[offset+radius];
                // The blurred premultiplied image is then composited over the
                // live, unfiltered backdrop, as in CSS and the retained UI tree.
                var at=((y+row)*sourceWidth+x+column)*4;var alpha=filtered[3]+source[at+3]*(1-filtered[3]/255);
                for(var channel=0;channel<4;channel++){
                    var value=channel==3?alpha:(filtered[channel]+source[at+channel]*source[at+3]/255*(1-filtered[3]/255))*255/Math.Max(.000001,alpha);
                    var difference=Math.Abs(value-expected[(row*width+column)*4+channel]);error+=difference;if(difference>max){max=difference;location=$"({column},{row}) channel {channel}, {value:F2} / {expected[(row*width+column)*4+channel]}";}
                }
            }
            var mean=error/(width*height*4);worst=Math.Max(worst,mean);pixels+=width*height;scenarios++;
            Require(mean<2d&&max<10d,$"Chromium blur({fixture["sigma"].Number}px), DPR {dpr}, transparent={fixture["transparent"].Truth}: mean {mean:F3}/255, max {max:F3}/255 {location}");
        }
        Console.WriteLine($"Chromium backdrop-filter pixels: PASS ({scenarios} cases, {pixels} pixels, DPR 1/1.75/3; worst mean {worst:F3}/255). GPU execution is not covered.");
    }
}
