using EchoSphere.Presentation;

internal sealed class CanvasStateChecks : IVisualCanvasSink
{
    private readonly IVisualCanvasSink _next;
    private Dictionary<string,V> _state=new Dictionary<string,V>();
    private readonly Stack<Dictionary<string,V>> _saved=new Stack<Dictionary<string,V>>();
    private readonly string _label;
    private static int _paints,_zeroShadows,_maxStops,_nonfinite,_images,_texts;
    internal CanvasStateChecks(IVisualCanvasSink next,string label){_next=next;_label=label;}
    public void Set(string name,V value){_state[name]=value;_next.Set(name,value);}
    public V Call(string name,V[] args)
    {
        if(args.Any(value=>value.Raw is double number&&!double.IsFinite(number)))_nonfinite++;
        if(name=="save")_saved.Push(new Dictionary<string,V>(_state));
        else if(name=="restore"&&_saved.Count>0)_state=_saved.Pop();
        if(new[]{"fill","stroke","fillRect","strokeRect","fillText","drawImage"}.Contains(name)){
            _paints++;
            V Read(string key,V fallback)=>_state.TryGetValue(key,out var value)?value:fallback;
            if(VisualCssColor.Parse(Read("shadowColor","transparent").Text).A>0&&Read("shadowBlur",0).Number==0&&Read("shadowOffsetX",0).Number==0&&Read("shadowOffsetY",0).Number==0)_zeroShadows++;
            var paint=Read(name.StartsWith("stroke")?"strokeStyle":"fillStyle","#000000");
            if(paint.Raw is VisualGradient gradient)_maxStops=Math.Max(_maxStops,gradient.Stops.Count);
            if(name=="drawImage"){
                _images++;
                if(args.Length!=3&&args.Length!=5)throw new InvalidOperationException(_label+": authored drawImage now requires a cropped-image adapter");
                if(args.Length==5&&(args[3].Number<0||args[4].Number<0))throw new InvalidOperationException(_label+": authored negative image dimensions need the Canvas no-flip rectangle mapping");
            }
            if(name=="fillText"){
                _texts++;
                if(paint.Raw is VisualGradient)throw new InvalidOperationException(_label+": authored Canvas gradient text needs a glyph paint adapter");
                if(!new[]{"alphabetic","middle","top","bottom","hanging","ideographic"}.Contains(Read("textBaseline","alphabetic").Text))throw new InvalidOperationException(_label+": new authored text baseline");
            }
        }
        return _next.Call(name,args);
    }
    internal static void Report(){
        if(_nonfinite>0)throw new InvalidOperationException("Authored Canvas now emits nonfinite coordinates; audit native ignore semantics before changing the reference");
        if(_maxStops>16)throw new InvalidOperationException("Native Canvas stop buffer is smaller than the authored gradients");
        if(_paints==0||_images==0||_texts==0)throw new InvalidOperationException("Missing source Canvas paint/image/text coverage");
        foreach(var sample in new[]{(Input:.75,Expected:1d),(Input:1.25,Expected:1.25),(Input:3d,Expected:1.75),(Input:double.NaN,Expected:1d),(Input:double.PositiveInfinity,Expected:1d)}){
            var backing=Visual_renderScale.Scope["calculateBackingSize"].Call(390,844,sample.Input);
            if(backing["dpr"].Number!=sample.Expected||backing["width"].Number!=Math.Floor(390*sample.Expected+.5)||backing["height"].Number!=Math.Floor(844*sample.Expected+.5))throw new InvalidOperationException("Original Canvas backing policy changed for DPR "+sample.Input);
        }
        Console.WriteLine($"Authored Canvas adapter contracts: PASS ({_paints} paint operations; {_images} image calls; {_texts} text calls; maximum {_maxStops}/16 gradient stops; {_nonfinite} nonfinite calls; {_zeroShadows} zero-blur/offset shadow states).");
        Console.WriteLine("Original Canvas resolution policy: PASS (DPR .75/1.25/3/NaN/Infinity; CSS density is separate).");
    }
}
