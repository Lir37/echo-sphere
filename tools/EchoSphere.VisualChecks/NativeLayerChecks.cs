using EchoSphere.Presentation;
using System.IO.Compression;
using System.Xml.Linq;

internal static class NativeLayerChecks
{
    private static V Tree(V value){if(value.Raw is Dictionary<string,V>&&!value["tag"].Undefined)return VisualDom.Element(value["tag"].Text,value["properties"],value["children"].Items.Select(Tree).ToArray());if(value.Raw is List<V>)return VisualJs.Arr(value.Items.Select(Tree).ToArray());return value;}
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("Native layer: "+message);}
    internal static void Run()
    {
        var sheet=new VisualCss(VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"SourceUiStyles.json"))));
        var surfaces=0;var shears=0;var clippedBrushes=0;var svgCount=0;var textCount=0;var depthText=0;var declarations=new HashSet<string>();
        void Inspect(V tree){var root=VisualCssNode.Build(Tree(tree));sheet.Apply(root);
            void Visit(VisualCssNode node){
                var matrix=VisualCssTransform.Parse(sheet,node);
                if(VisualCssTransform.Is2D(matrix)){
                    var split=VisualCssTransform.Native2D(matrix);var first=VisualCssTransform.Parse(sheet,node,$"rotate({VisualJs.NumberText(split.Rotation)}rad) scale({VisualJs.NumberText(split.X)},{VisualJs.NumberText(split.Y)}) rotate({VisualJs.NumberText(split.ContentRotation)}rad)");
                    Require(new[]{0,1,4,5}.All(index=>Math.Abs(first[index]-matrix[index])<1e-9),"native pose changed "+sheet.Get(node,"transform"));surfaces++;if(Math.Abs(split.ContentRotation)>1e-9){shears++;declarations.Add(sheet.Get(node,"transform"));}
                }
                if(sheet.Get(node,"overflow")=="hidden"&&VisualCssPaint.HasVisibleBrush(sheet,node))clippedBrushes++;
                if(node.Tag=="#text"&&!string.IsNullOrWhiteSpace(node.Text)){for(var ancestor=node.Parent;ancestor!=null;ancestor=ancestor.Parent)if(sheet.Get(ancestor,"transform-style")=="preserve-3d"){depthText++;break;}}
                if(node.Tag=="svg"){
                    node.Width=78;node.Height=78;var xml=XElement.Parse(VisualSvg.Serialize(node,sheet));Require(!xml.DescendantsAndSelf().SelectMany(item=>item.Attributes()).Any(attribute=>attribute.Name.LocalName=="fillOpacity"||attribute.Name.LocalName=="strokeOpacity"),"React opacity names escaped into SVG");svgCount++;
                    void Texts(VisualCssNode item){if(item.Tag=="text")textCount++;foreach(var child in item.Children)Texts(child);}Texts(node);
                }
                foreach(var child in node.Children)Visit(child);
            }Visit(root);
        }
        foreach(var width in new[]{360d,390d,1280d}){sheet.Width=width;sheet.Height=844;foreach(var fixture in VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-ui-fixtures.json"))).Items)Inspect(fixture["tree"]);}
        using(var stream=new GZipStream(File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-ui-interactions.json.gz")),CompressionMode.Decompress))using(var reader=new StreamReader(stream)){
            sheet.Width=390;foreach(var fixture in VisualJson.Parse(reader.ReadToEnd()).Items)foreach(var frame in fixture["frames"].Items)Inspect(frame["tree"]);
        }
        var random=new Random(19471);
        for(var index=0;index<5000;index++){
            var matrix=VisualCssTransform.Identity();foreach(var at in new[]{0,1,4,5})matrix[at]=random.NextDouble()*20-10;
            if(index%9==0)matrix[0]=matrix[1]=0;if(index%11==0){matrix[4]=matrix[0];matrix[5]=matrix[1];}
            var pose=VisualCssTransform.Native2D(matrix);var co=Math.Cos(pose.Rotation);var si=Math.Sin(pose.Rotation);var ct=Math.Cos(pose.ContentRotation);var st=Math.Sin(pose.ContentRotation);
            var actual=new[]{co*pose.X*ct-si*pose.Y*st,si*pose.X*ct+co*pose.Y*st,-co*pose.X*st-si*pose.Y*ct,-si*pose.X*st+co*pose.Y*ct};
            Require(actual.Zip(new[]{matrix[0],matrix[1],matrix[4],matrix[5]}).All(pair=>Math.Abs(pair.First-pair.Second)<1e-9),"native pose loses reflection/shear/singular scale");
        }
        var emptySheet=new VisualCss(VisualJs.Obj("rules",VisualJs.Arr(),"keyframes",VisualJs.Obj())) {Width=160,Height=120};
        var brush=new VisualCssNode {Tag="div",X=0,Y=0,Width=40,Height=32};brush.Style["background-color"]="#123456";brush.Style["box-shadow"]="0 0 12px #55dfff";
        var raster=VisualCssPaint.Paint(emptySheet,brush,1);Require(raster.Left<0&&raster.Top<0&&raster.Width>brush.Width&&raster.Height>brush.Height,"screen-edge crop discarded shadow texels before CSS transform");
        brush.Style["box-shadow"]="0 0 0 9999px rgba(0,0,0,.8)";var viewport=new VisualCssPaint.Viewport {Left=-72,Top=-45,Right=130,Bottom=110};var cutout=VisualCssPaint.Paint(emptySheet,brush,1,viewport);
        Require(cutout.Left==-72&&cutout.Top==-45&&cutout.Width==202&&cutout.Height==155,"large cutout must use the inverse-transformed local viewport");
        var svg=VisualCssNode.Build(VisualDom.Element("svg",VisualJs.Obj("viewBox","0 0 20 20"),VisualDom.Element("path",VisualJs.Obj("d","M0 0H20V20H0Z","fill","rgba(20 80 200 / .5)","fillOpacity",".6","stroke","rgba(255, 0, 0, .4)","strokeOpacity",".5"))));sheet.Apply(svg);
        VisualCssNode FindSvg(VisualCssNode item)=>item.Tag=="svg"?item:item.Children.Select(FindSvg).FirstOrDefault(child=>child!=null);
        var shape=XElement.Parse(VisualSvg.Serialize(FindSvg(svg),sheet)).Elements().Single();
        Require(shape.Attribute("fill-opacity")!=null&&shape.Attribute("stroke-opacity")!=null,"missing opacity attributes: "+shape);
        Require(Math.Abs(double.Parse(shape.Attribute("fill-opacity").Value,System.Globalization.CultureInfo.InvariantCulture)-.3)<1e-6&&Math.Abs(double.Parse(shape.Attribute("stroke-opacity").Value,System.Globalization.CultureInfo.InvariantCulture)-.2)<1e-6,"RGBA alpha must multiply the authored React opacity exactly once");
        var icon=FindSvg(svg);icon.Children[0].Properties["fill"]="#55dfff18";icon.Children[0].Properties["stroke"]="currentColor";icon.Children[0].Style["color"]="color-mix(in srgb,#55e6c1 72%,#fff 28%)";
        var modern=XElement.Parse(VisualSvg.Serialize(icon,sheet)).Elements().Single();Require(modern.Attribute("fill").Value=="rgb(85,223,255)"&&modern.Attribute("stroke").Value.StartsWith("rgb(")&&Math.Abs(double.Parse(modern.Attribute("fill-opacity").Value,System.Globalization.CultureInfo.InvariantCulture)-24d/255*.6)<1e-6,"drag hex-alpha or icon currentColor/color-mix was not normalized");
        Require(shears>0&&clippedBrushes>0&&textCount>0,"missing source coverage for shear, clipped brushes or interactive SVG text");
        Console.WriteLine($"Native layer contracts: PASS ({surfaces} source 2D poses, {shears} shear cases; 5000 additional matrices; {clippedBrushes} clipped brushes; {svgCount} SVGs, {textCount} dynamic SVG text nodes; alpha/shadow bounds).");
        Console.WriteLine("Authored non-TRS transforms: "+string.Join(" | ",declarations.OrderBy(value=>value)));
        Require(depthText==0,"source now has text inside preserve-3d; depth text surfaces need an explicit adapter: "+depthText);
    }
}
