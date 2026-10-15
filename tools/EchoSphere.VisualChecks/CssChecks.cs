using EchoSphere.Presentation;
using System.Globalization;

internal static class CssChecks
{
    private static V Tree(V value)
    {if(value.Raw is Dictionary<string,V>&&!value["tag"].Undefined)return VisualDom.Element(value["tag"].Text,value["properties"],value["children"].Items.Select(Tree).ToArray());if(value.Raw is List<V>)return VisualJs.Arr(value.Items.Select(Tree).ToArray());return value;}
    private static Dictionary<string,VisualCssNode> Nodes(VisualCssNode root)
    {var result=new Dictionary<string,VisualCssNode>();void Visit(VisualCssNode node,string path){result[path]=node;var index=0;foreach(var child in node.Children)if(child.Pseudo!=null)Visit(child,path+child.Pseudo);else Visit(child,path+"/"+index++);}Visit(root,"0");return result;}
    private static string Default(string property)=>property switch {"display"=>"block","position"=>"static","font-weight"=>"400","font-size"=>"16px","line-height"=>"normal","letter-spacing"=>"normal","text-align"=>"start","text-transform"=>"none","white-space"=>"normal","opacity"=>"1","visibility"=>"visible","background-color"=>"transparent","pointer-events"=>"auto","user-select"=>"auto","overflow-wrap"=>"normal","text-shadow"=>"none",_=>property.EndsWith("-color")?"currentColor":property.EndsWith("-style")?"none":"0px"};
    private static bool Shadows(VisualCss sheet,VisualCssNode node,string actual,string expected)
    {var a=VisualCss.Split(actual,',');var b=VisualCss.Split(expected,',');if(a.Count!=b.Count)return false;for(var i=0;i<a.Count;i++){var aw=VisualCss.Split(a[i].Trim(),' ').Where(x=>x.Length>0).ToArray();var bw=VisualCss.Split(b[i].Trim(),' ').Where(x=>x.Length>0).ToArray();bool Length(string x)=>System.Text.RegularExpressions.Regex.IsMatch(x,@"^[-+\d.]");var ac=VisualCssColor.Parse(aw.FirstOrDefault(x=>!Length(x))??"currentColor",sheet.Get(node,"color","#000000"));var bc=VisualCssColor.Parse(bw.FirstOrDefault(x=>!Length(x))??"currentColor");if(Math.Abs(ac.R-bc.R)>.00001||Math.Abs(ac.G-bc.G)>.00001||Math.Abs(ac.B-bc.B)>.00001||Math.Abs(ac.A-bc.A)>.00051)return false;var al=aw.Where(Length).Select(x=>sheet.Length(x,node.Width)).ToArray();var bl=bw.Where(Length).Select(x=>sheet.Length(x,node.Width)).ToArray();if(al.Length!=bl.Length||al.Where((x,j)=>Math.Abs(x-bl[j])>.001).Any())return false;}return true;}
    public static void Run()
    {
        V Read(string name)=>VisualJson.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,name)));
        var ui=Read("source-ui-fixtures.json").Items.ToDictionary(value=>value["label"].Text,value=>value["tree"]);
        using var compressed=File.OpenRead(Path.Combine(AppContext.BaseDirectory,"source-css-fixtures.json.gz"));using var decompressed=new System.IO.Compression.GZipStream(compressed,System.IO.Compression.CompressionMode.Decompress);using var reader=new StreamReader(decompressed);var fixtures=VisualJson.Parse(reader.ReadToEnd());
        var sheet=new VisualCss(Read("SourceUiStyles.json"));
        var errors=new List<string>();var gridErrors=new List<string>();var comparisons=0;var grids=0;var screens=0;var containingBlocks=0;
        foreach(var fixture in fixtures.Items){
            var root=VisualCssNode.Build(Tree(ui[fixture["ui"].Text]));var nodes=Nodes(root);sheet.Width=fixture["viewport"]["width"].Number;sheet.Height=fixture["viewport"]["height"].Number;
            void Bounds(){foreach(var expected in fixture["nodes"].Items)if(nodes.TryGetValue(expected["path"].Text,out var node)){var rect=expected["rect"];var layout=expected["layout"];node.X=rect["x"].Number;node.Y=rect["y"].Number;node.Width=layout.Undefined?rect["width"].Number:layout["width"].Number;node.Height=layout.Undefined?rect["height"].Number:layout["height"].Number;}}
            Bounds();sheet.Apply(root);nodes=Nodes(root);Bounds();
            foreach(var expected in fixture["nodes"].Items){
                if(!nodes.TryGetValue(expected["path"].Text,out var node)){errors.Add(fixture["label"].Text+" "+expected["path"].Text+": missing element "+expected["tag"].Text);continue;}
                if(node.Pseudo==null&&expected["rect"]["width"].Number>0d&&new[]{"absolute","fixed"}.Contains(sheet.Get(node,"position"))){var block=VisualCssPosition.ContainingBlock(sheet,node);var blockPath=block==null?null:nodes.First(pair=>pair.Value==block).Key;var wanted=expected["offsetParent"].Null?null:expected["offsetParent"].Text;if(blockPath!=wanted&&!(block==null&&sheet.Get(node,"position")=="absolute"&&wanted=="0/0"))errors.Add(fixture["label"].Text+" "+expected["path"].Text+" containing block: "+blockPath+" != "+wanted);containingBlocks++;}
                foreach(var property in (Dictionary<string,V>)expected["style"].Raw){
                    var key=property.Key;var wanted=property.Value.Text;var actual=sheet.Get(node,key,Default(key));var same=actual==wanted;
                    try{
                        if(!same&&key=="opacity")same=Math.Abs(double.Parse(actual,CultureInfo.InvariantCulture)-double.Parse(wanted,CultureInfo.InvariantCulture))<.00001;
                        if(!same&&key=="text-shadow")same=Shadows(sheet,node,actual,wanted);
                        if(!same&&(key=="color"||key.EndsWith("-color"))){var a=VisualCssColor.Parse(actual,sheet.Get(node,"color","#000000"));var b=VisualCssColor.Parse(wanted);same=Math.Abs(a.R-b.R)<.00001&&Math.Abs(a.G-b.G)<.00001&&Math.Abs(a.B-b.B)<.00001&&(Math.Abs(a.A-b.A)<.00051||Math.Floor(a.A*255+.5)==Math.Floor(b.A*255+.5));}
                        if(!same&&(key=="font-size"||key=="line-height"||key=="letter-spacing"||key.EndsWith("-width")||key.EndsWith("-radius"))){var font=sheet.Length(sheet.Get(node,"font-size","16px"),16);var a=key=="line-height"&&double.TryParse(actual,NumberStyles.Float,CultureInfo.InvariantCulture,out var multiplier)?font*multiplier:sheet.Length(actual,node.Width,font);var b=sheet.Length(wanted,node.Width,font);if(key.EndsWith("-width")&&sheet.Get(node,key.Replace("-width","-style"),"none")=="none")a=0;same=Math.Abs(a-b)<.001;}
                    }catch(Exception){}
                    if(!same)errors.Add(fixture["label"].Text+" "+expected["path"].Text+" "+node.Tag+"."+node.Attribute("className")+" "+key+": "+actual+" != "+wanted);
                    comparisons++;
                }
            }
            var expectedNodes=fixture["nodes"].Items.Where(item=>nodes.ContainsKey(item["path"].Text)).ToDictionary(item=>nodes[item["path"].Text],item=>item);
            foreach(var node in nodes.Values.Where(node=>sheet.Get(node,"display")=="grid"&&node.Width>0d&&node.Height>0d)){
                var layout=VisualCssGrid.Compute(sheet,node);foreach(var cell in layout.Cells){
                    if(!expectedNodes.TryGetValue(cell.Node,out var expected)||sheet.Get(cell.Node,"transform","none")!="none")continue;
                    var rect=expected["rect"];if(rect["width"].Number==0d&&rect["height"].Number==0d)continue;
                    var parentRect=expectedNodes[node]["rect"];var scaleX=parentRect["width"].Number/node.Width;var scaleY=parentRect["height"].Number/node.Height;
                    var actual=new[]{cell.X,cell.Y,cell.Width,cell.Height};var wanted=new[]{(rect["x"].Number-node.X)/scaleX,(rect["y"].Number-node.Y)/scaleY,rect["width"].Number/scaleX,rect["height"].Number/scaleY};
                    if(actual.Where((value,index)=>Math.Abs(value-wanted[index])>.04d).Any())gridErrors.Add(fixture["label"].Text+" "+node.Tag+"."+node.Attribute("className")+" → "+cell.Node.Tag+"."+cell.Node.Attribute("className")+": "+string.Join(",",actual.Select(x=>x.ToString("F3",CultureInfo.InvariantCulture)))+" != "+string.Join(",",wanted.Select(x=>x.ToString("F3",CultureInfo.InvariantCulture))));grids++;
                }
            }
            screens++;
        }
        if(errors.Count>0){File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"failed-css-styles.txt"),errors);throw new InvalidOperationException($"Source CSS differs in {errors.Count} / {comparisons} computed properties. First: {errors[0]}");}
        Console.WriteLine($"Browser CSS → native cascade: PASS ({comparisons} computed properties / {screens} screen-viewports).");
        Console.WriteLine($"Browser CSS containing blocks → native positioned/fixed elements: PASS ({containingBlocks} elements).");
        if(gridErrors.Count>0){File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"failed-grid-layouts.txt"),gridErrors);throw new InvalidOperationException($"Source grid differs in {gridErrors.Count} / {grids} cells: {gridErrors[0]}");}
        Console.WriteLine($"Browser CSS grid → native track placement: PASS ({grids} cells, browser intrinsic-size inputs).");
        GridTrackChecks.Run();
    }
}
