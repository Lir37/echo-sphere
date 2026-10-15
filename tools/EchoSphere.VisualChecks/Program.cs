using EchoSphere.Presentation;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

static V Value(JsonElement value)=>value.ValueKind switch {
    JsonValueKind.Number=>new V(value.GetDouble()),JsonValueKind.String=>new V(value.GetString()),
    JsonValueKind.True=>new V(true),JsonValueKind.False=>new V(false),JsonValueKind.Null=>VisualJs.Null,
    JsonValueKind.Array=>VisualJs.Arr(value.EnumerateArray().Select(Value).ToArray()),
    JsonValueKind.Object=>VisualJs.Obj(value.EnumerateObject().SelectMany(x=>new[]{new V(x.Name),Value(x.Value)}).ToArray()),_=>default
};
static V Module(string module)=>module switch {
    "aura"=>Visual_auraVisual.Scope,"chain"=>Visual_chainVisual.Scope,"elemental"=>Visual_elementalVisual.Scope,
    "gravity"=>Visual_gravityVisual.Scope,"modifier"=>Visual_modifierVisual.Scope,"mutation"=>Visual_mutationVisual.Scope,
    "orbital"=>Visual_orbitalVisual.Scope,"prism"=>Visual_prismVisual.Scope,"pulse"=>Visual_pulseVisual.Scope,
    "shotgun"=>Visual_shotgunVisual.Scope,"sniper"=>Visual_sniperVisual.Scope,"void"=>Visual_voidVisual.Scope,
    "enemy"=>Visual_enemyVisual.Scope,"renderer"=>Visual_renderer.Scope,_=>throw new InvalidOperationException(module)
};
try {
if(args.Contains("--audit-only")){PresentationAudit.Run();return 0;}
if(args.Contains("--animations-only")){AnimationChecks.Run();TransitionChecks.Run();return 0;}
if(args.Contains("--paint-only")){PaintChecks.Run();return 0;}
if(args.Contains("--css-only")){CssChecks.Run();return 0;}
if(args.Contains("--interactions-only")){UiInteractionChecks.Run();return 0;}
using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"source-visual-fixtures.json")));
var count=0;
VisualJs.Assign(VisualJs.MathObject,"random","=",VisualJs.Fn(a=>.5d));
VisualJs.Assign(VisualJs.PerformanceObject,"now","=",VisualJs.Fn(a=>0d));
VisualJs.ImageFactory=()=>new V(new FixtureImage());
foreach(var fixture in json.RootElement.EnumerateArray())
{
    var trace=new VisualTrace();var canvas=new VisualCanvas(trace);
    canvas.Set("canvas",VisualJs.Obj("nativeViewport",VisualJs.Obj("cssWidth",390,"cssHeight",844,"dpr",1)));trace.Clear();
    var arguments=new List<V>{canvas.Value};arguments.AddRange(fixture.GetProperty("args").EnumerateArray().Select(Value));
    var module=fixture.GetProperty("module").GetString();var method=fixture.GetProperty("name").GetString();
    if(fixture.TryGetProperty("frames",out var frames)){
        foreach(var frame in frames.EnumerateArray()){
            foreach(var property in frame.GetProperty("patch").EnumerateObject()){var parts=property.Name.Split('.');var owner=arguments[1];foreach(var part in parts.Take(parts.Length-1))owner=owner[part];owner[parts[^1]]=Value(property.Value);}
            trace.Clear();Module(module)[method].Call(arguments.ToArray());var frameHash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(trace.Trace.ToString()))).ToLowerInvariant();
            if(trace.Count!=frame.GetProperty("count").GetInt32()||frameHash!=frame.GetProperty("sha").GetString())throw new InvalidOperationException(fixture.GetProperty("label").GetString()+" stateful frame "+count+" drawing commands differ");count++;
        }
        continue;
    }
    try {Module(module)[method].Call(arguments.ToArray());}
    catch(Exception error){throw new InvalidOperationException(fixture.GetProperty("label").GetString()+": "+error.Message,error);}
    var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(trace.Trace.ToString()))).ToLowerInvariant();
    if(trace.Count!=fixture.GetProperty("count").GetInt32()||hash!=fixture.GetProperty("sha").GetString())
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"failed-visual-trace.txt"),trace.Trace.ToString());
        throw new InvalidOperationException(fixture.GetProperty("label").GetString()+": drawing commands differ: "+trace.Count+" / "+fixture.GetProperty("count")+"; "+hash+" / "+fixture.GetProperty("sha").GetString());
    }
    count++;
}
Console.WriteLine($"Source TypeScript → native C# drawing command streams: PASS ({count} scenarios). No Unity compilation.");
UiChecks.Run();
CssChecks.Run();
AnimationChecks.Run();
TransitionChecks.Run();
PaintChecks.Run();
UiInteractionChecks.Run();
return 0;
} catch(Exception error) {
    // A comparison failure must remain a normal command-line failure. Letting it
    // escape Main invokes Windows Error Reporting and obscures the failed case.
    var logPath=Path.Combine(AppContext.BaseDirectory,"visual-checks-error.log");
    var diagnostic=DateTimeOffset.UtcNow.ToString("O")+Environment.NewLine+error;
    Console.Error.WriteLine("Visual comparison FAILED: "+error.Message);
    try {
        File.WriteAllText(logPath,diagnostic);
        Console.Error.WriteLine("Details: "+logPath);
    } catch(Exception logError) when(logError is IOException or UnauthorizedAccessException) {
        Console.Error.WriteLine(diagnostic);
        Console.Error.WriteLine("Could not write diagnostic log: "+logError.Message);
    }
    return 1;
}
sealed class FixtureImage : IVisualHost {
    private string _src;private double _width,_height;
    public V Get(string key)=>key switch {"src"=>_src==null?default:new V(_src),"complete"=>_src!=null,"naturalWidth" or "width"=>_width,"naturalHeight" or "height"=>_height,_=>default};
    public void Set(string key,V value) {if(key!="src")return;_src=value.Text;if(SourceCanvasArtIndex.Dimensions.TryGetValue(_src,out var dimensions)){_width=dimensions[0];_height=dimensions[1];}}
}
